"""Reproducible document layers. Never regenerates the frozen External Test art."""
from pathlib import Path
import argparse, hashlib, json, os, re
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(__file__).parent / 'Source'
LAYERS = ('Paper', 'Protected', 'Erasable', 'EraseMask', 'ProtectMask', 'CompletePreview')
VERSION = '2.0'

def relative(value):
    p = (ROOT / value).resolve()
    if not p.is_relative_to(ROOT):
        raise ValueError('Path outside project')
    return p

def generate(spec):
    if spec['visibility'] == 'existing':
        raise ValueError('Frozen artwork is read-only; copy the definition to a new ID')
    size = (spec['canvasWidth'], spec['canvasHeight'])
    layers = {k: Image.new('RGBA', size, (0, 0, 0, 0)) for k in ('Protected', 'Erasable')}
    layers['Paper'] = Image.new('RGBA', size, (249, 247, 239, 255))
    names = {'paper': 'Paper', 'protected': 'Protected', 'erasable': 'Erasable'}
    colors = {'paper': (190, 185, 170, 255), 'protected': (32, 34, 38, 255), 'erasable': (100, 125, 145, 155)}
    font_path = Path(os.environ.get('WINDIR', 'C:/Windows')) / 'Fonts' / spec['font']
    if not font_path.is_file():
        raise ValueError('Required font not found: ' + spec['font'])
    for item in spec.get('text', []):
        ImageDraw.Draw(layers[names[item['role']]]).text((item['x'], item['y']), item['text'],
            font=ImageFont.truetype(str(font_path), item['size']), fill=colors[item['role']])
    for item in spec.get('lines', []):
        ImageDraw.Draw(layers[names[item['role']]]).line(item['points'], fill=colors[item['role']], width=item['width'])
    for out, src in [('EraseMask', 'Erasable'), ('ProtectMask', 'Protected')]:
        # Coverage is independent of displayed ink opacity; masks use linear red, opaque alpha.
        alpha = np.array(layers[src])[:, :, 3]
        value = np.minimum(255, alpha.astype(float) * (255 / colors[src.lower()][3])).astype('uint8')
        layers[out] = Image.fromarray(np.dstack((value, value, value, np.full_like(value, 255))))
    layers['CompletePreview'] = Image.alpha_composite(layers['Paper'], layers['Protected'])
    layers['InitialPreview'] = Image.alpha_composite(layers['CompletePreview'], layers['Erasable'])
    original = ROOT / 'Tools/JobPipeline/GeneratedSource' / spec['id']
    runtime = relative(spec['artworkFolder'])
    original.mkdir(parents=True, exist_ok=True); runtime.mkdir(parents=True, exist_ok=True)
    for name, img in layers.items():
        img.save(original / (name + '.png'))
        img.resize((spec['runtimeWidth'], spec['runtimeHeight']), Image.Resampling.LANCZOS).save(runtime / (name + '.png'))
    meta = dict(jobId=spec['id'], sourceVersion=spec['sourceVersion'], generatorVersion=VERSION,
        canvasWidth=size[0], canvasHeight=size[1], runtimeWidth=spec['runtimeWidth'], runtimeHeight=spec['runtimeHeight'],
        sourceType=spec['sourceType'], coordinateOrigin='top-left', uv='full-canvas', font=spec['font'],
        fontSha256=hashlib.sha256(font_path.read_bytes()).hexdigest(),
        sourceSha256=hashlib.sha256(json.dumps(spec, sort_keys=True, ensure_ascii=False).encode()).hexdigest(),
        layers={n: hashlib.sha256((runtime/(n+'.png')).read_bytes()).hexdigest() for n in layers})
    (runtime/'pipeline-manifest.json').write_text(json.dumps(meta, ensure_ascii=False, indent=2), encoding='utf8')
    return meta

def validate(spec, seen):
    errors=[]; stats={}; ident=spec.get('id','')
    if not re.fullmatch(r'[A-Z][A-Z0-9_]+',ident): errors.append('Invalid Job ID')
    if ident in seen: errors.append('Duplicate Job ID')
    seen.add(ident)
    if not re.fullmatch(r'CLIENT_[A-Z0-9_]+',spec.get('clientId','')): errors.append('Invalid Client ID')
    for key in ('title','clientLetter','completionMessage','runtimeAsset','paperAsset','writingAsset'):
        if not spec.get(key): errors.append('Missing '+key)
    if spec.get('baseReward',-1)<0: errors.append('Invalid reward')
    folder=relative(spec['artworkFolder']); images={}
    for name in LAYERS:
        p=folder/(name+'.png')
        if not p.is_file(): errors.append('Missing '+name);continue
        with Image.open(p) as img:
            images[name]=np.array(img.convert('RGBA'))
    if len({a.shape for a in images.values()})>1: errors.append('Canvas mismatch')
    for name in ('EraseMask','ProtectMask'):
        if name not in images: continue
        a=images[name]
        if np.any(a[:,:,3]!=255): errors.append(name+' alpha must be opaque')
        if np.any(a[:,:,0]!=a[:,:,1]) or np.any(a[:,:,0]!=a[:,:,2]): errors.append(name+' must be grayscale')
        stats[name+'Pixels']=int(np.count_nonzero(a[:,:,0]))
        stats[name+'Ratio']=stats[name+'Pixels']/a.shape[0]/a.shape[1]
        if not stats[name+'Pixels'] and (name=='EraseMask' or spec.get('precision',False)): errors.append('Unexpected empty '+name)
    if 'EraseMask' in images and 'ProtectMask' in images and images['EraseMask'].shape==images['ProtectMask'].shape:
        e=images['EraseMask'][:,:,0]>0;p=images['ProtectMask'][:,:,0]>0
        stats['overlapPixels']=int(np.count_nonzero(e&p))
        stats['overlapRatio']=stats['overlapPixels']/max(1,int(np.count_nonzero(e)))
        if stats['overlapRatio']>spec.get('maxOverlapRatio',.1): errors.append('Unexpected mask overlap')
    for name in ('Paper','CompletePreview'):
        if name in images and np.any(images[name][:,:,3]!=255): errors.append(name+' must be opaque')
    for name in ('Protected','Erasable'):
        if name in images and np.all(images[name][:,:,3]==255): errors.append(name+' needs transparent background')
    for key in ('paperAsset','writingAsset'):
        if not relative(spec[key]).is_file(): errors.append('Missing reference '+key)
    return {'id':ident,'errors':errors,'statistics':stats}

def main():
    p=argparse.ArgumentParser();p.add_argument('command',choices=['generate','validate']);p.add_argument('--id');args=p.parse_args()
    specs=[json.loads(f.read_text(encoding='utf8')) for f in sorted(SOURCE.glob('*.json'))]
    if args.id: specs=[s for s in specs if s['id']==args.id]
    if not specs: raise SystemExit('No definitions found')
    if args.command=='generate':
        for s in specs: generate(s)
    else:
        seen=set();results=[validate(s,seen) for s in specs]
        out=ROOT/'TestResults-Foundation';out.mkdir(exist_ok=True)
        (out/'pipeline.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf8')
        for r in results: print(r['id'],r['errors'] or 'OK')
        if any(r['errors'] for r in results): raise SystemExit(1)
if __name__=='__main__': main()
