from pathlib import Path
import numpy as np
from PIL import Image
root=Path(__file__).resolve().parents[2]
(root/'TestResults-Feedback082').mkdir(exist_ok=True)
f=root/'Assets/Keshiya/ExternalArt/TEST_003'
p=np.asarray(Image.open(f/'ProtectMask.png').convert('L'))>0
e=np.asarray(Image.open(f/'EraseMask.png').convert('L')).astype(float)
near=p.copy();dist=np.full(p.shape,81,dtype=np.uint8);dist[p]=0
for n in range(1,81):
 padded=np.pad(near,1);next=np.zeros_like(near)
 for y in range(3):
  for x in range(3):next|=padded[y:y+near.shape[0],x:x+near.shape[1]]
 dist[next&~near]=n;near=next
print('max clearance',dist.max());safe=dist>67
for n in range(67):
 padded=np.pad(safe,1);next=np.zeros_like(safe)
 for y in range(3):
  for x in range(3):next|=padded[y:y+safe.shape[0],x:x+safe.shape[1]]
 safe=next
print('conservative face coverage',float(e[safe].sum()/e.sum()))
for t in [4,10,20,40,67]:print(t,round(float(e[dist>t].sum()/e.sum()),4))
Image.fromarray(np.where(dist>67,255,0).astype('uint8')).save(root/'TestResults-Feedback082/manga-face-safe.png')

# Exact normal-face superellipse morphology in mask coordinates. No mask padding changes.
def dilate(a,rx,ry):
 out=np.zeros_like(a);cs=np.pad(np.cumsum(a,axis=1,dtype=np.int32),((0,0),(1,0)))
 xs=np.arange(a.shape[1])
 for dy in range(-int(ry),int(ry)+1):
  dx=int(rx*max(0,1-(abs(dy)/ry)**4)**.25)
  span=(cs[:,np.minimum(a.shape[1],xs+dx+1)]-cs[:,np.maximum(0,xs-dx)])>0
  if dy>=0:out[dy:]|=span[:a.shape[0]-dy]
  else:out[:dy]|=span[-dy:]
 return out
safe=~dilate(p,67,56.8);coverage=dilate(safe,67,56.8)
ratio=float(e[coverage].sum()/e.sum());print('face exact-kernel coverage',ratio)
import json
import subprocess,io
oldp=np.asarray(Image.open(io.BytesIO(subprocess.check_output(['git','show','e3dbac7:Assets/Keshiya/ExternalArt/TEST_003/ProtectMask.png'],cwd=root))).convert('L'))>0
olde=np.asarray(Image.open(io.BytesIO(subprocess.check_output(['git','show','e3dbac7:Assets/Keshiya/ExternalArt/TEST_003/EraseMask.png'],cwd=root))).convert('L')).astype(float)
oldcoverage=dilate(~dilate(oldp,67,56.8),67,56.8)
oldratio=float(olde[oldcoverage].sum()/olde.sum());print('baseline face coverage',oldratio)
(root/'TestResults-Feedback082/manga-analysis.json').write_text(json.dumps({'baselineFaceKernelCoverage':oldratio,'faceKernelCoverage':ratio,'baselineProtectPixels':int(oldp.sum()),'protectPixels':int(p.sum()),'baselineErasePixels':int((olde>0).sum()),'erasePixels':int((e>0).sum()),'method':'Normal face superellipse at mask resolution; protect threshold0, proxy only, human validation required'},indent=2))
