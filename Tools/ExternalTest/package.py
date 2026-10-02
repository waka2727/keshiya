"""Package a normal Unity build; no source, diagnostics, screenshots, or QA outputs."""
from pathlib import Path
import zipfile, hashlib, json
ROOT=Path(__file__).resolve().parents[2]
BUILD=ROOT/'Builds/Windows-ExternalTest01'
ZIP=ROOT/'Builds/Keshiya-ExternalTest01.zip'
PREFIX='Keshiya-ExternalTest01'
FILES=['Keshiya.exe','UnityPlayer.dll','UnityCrashHandler64.exe','README.txt','Feedback.txt']
DIRS=['Keshiya_Data','MonoBleedingEdge','D3D12']
paths=[BUILD/n for n in FILES]
for d in DIRS:
 paths.extend(p for p in (BUILD/d).rglob('*') if p.is_file())
assert all(p.exists() for p in paths)
paths=[p for p in paths if p.suffix.lower() not in {'.pdb','.mdb','.log','.cs'}]
with zipfile.ZipFile(ZIP,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for p in sorted(paths): z.write(p,PREFIX+'/'+p.relative_to(BUILD).as_posix())
checks=[]
def check(ok,name):
 checks.append(('PASS ' if ok else 'FAIL ')+name)
 if not ok: raise AssertionError(name)
with zipfile.ZipFile(ZIP) as z:
 names=z.namelist()
 check(z.testzip() is None,'ZIP CRC valid')
 for f in FILES:check(PREFIX+'/'+f in names,f+' included')
 for d in DIRS:check(any(n.startswith(PREFIX+'/'+d+'/') for n in names),d+' runtime included')
 check(not any('TestResults' in n or n.lower().endswith(('.log','.cs','.pdb','.mdb')) for n in names),'no sources tests logs debug symbols')
 check(all(n.startswith(PREFIX+'/') and '..' not in n.split('/') for n in names),'single safe top-level directory')
 for name in ['README.txt','Feedback.txt']:
  s=z.read(PREFIX+'/'+name).decode('utf-8-sig')
  check('C:\\Users\\' not in s and 'w5402' not in s,name+' no developer path')
 check('Ctrl＋Wheel：' in z.read(PREFIX+'/README.txt').decode('utf-8-sig'),'documented final wheel controls')
summary={'zip':ZIP.name,'bytes':ZIP.stat().st_size,'uncompressedBytes':sum(p.stat().st_size for p in paths),'files':len(paths),'sha256':hashlib.sha256(ZIP.read_bytes()).hexdigest(),'checks':len(checks),'failures':0}
qa=ROOT/'TestResults-External01';qa.mkdir(exist_ok=True)
(qa/'package.txt').write_text('\n'.join(checks)+f'\nChecks={len(checks)}; Failures=0\n',encoding='utf8')
(qa/'package.json').write_text(json.dumps(summary,indent=2),encoding='utf8')
print(json.dumps(summary))
