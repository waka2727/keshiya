from pathlib import Path
from PIL import Image
import numpy as np,json,hashlib
r=Path(__file__).resolve().parents[2];(r/'TestResults-External').mkdir(exist_ok=True);checks=[]
def ck(ok,msg):
 checks.append(('PASS ' if ok else 'FAIL ')+msg)
for n in range(1,9):
 id=f'TEST_{n:03}';p=r/'Assets/Keshiya/ExternalArt'/id;source=r/'Tools/ExternalArt/Source'/id
 for layer in ['Paper','Protected','Erasable','EraseMask','ProtectMask','CompletePreview','InitialPreview']:
  ck(Image.open(p/(layer+'.png')).size==(1240,1754),id+' runtime '+layer)
  ck(Image.open(source/(layer+'.png')).size==(2480,3508),id+' source '+layer)
 ck(np.asarray(Image.open(p/'Paper.png'))[:,:,3].min()==255,id+' opaque paper')
 ck(np.array_equal(np.asarray(Image.open(p/'Erasable.png'))[:,:,3],np.asarray(Image.open(p/'EraseMask.png'))[:,:,0]),id+' target alpha aligns mask')
 m=json.loads((source/'manifest.json').read_text(encoding='utf8'));ck(bool(m['strokes']),id+' reproducible QA paths')
 brief=json.loads((r/f'Tools/ExternalArt/job-{n:03}.json').read_text(encoding='utf8'));ck(brief['requiredErasure']==.95,id+' completion remains .95')
letter=json.loads((r/'Tools/ExternalArt/Source/TEST_002/manifest.json').read_text(encoding='utf8'));ck(any('していまます' in s for s in letter['texts']),'intentional duplicated ma retained')
report='\n'.join(checks)+f'\nChecks={len(checks)}; Failures={sum(x.startswith("FAIL") for x in checks)}\n';(r/'TestResults-External/asset-validation.txt').write_text(report,encoding='utf8');print(report[-70:])
