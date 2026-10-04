"""Read-only comparison against the frozen source archive; never opens a baseline project for writing."""
from pathlib import Path
import argparse,hashlib,json,re,zipfile
p=argparse.ArgumentParser();p.add_argument('frozen_directory');args=p.parse_args()
root=Path(__file__).resolve().parents[2];frozen=Path(args.frozen_directory)
manifest=json.loads((frozen/'manifest.json').read_text(encoding='utf8'))
allowed={f'Assets/Keshiya/Scripts/{n}.cs' for n in ['CrumbEconomy','CrumbPiece','JobArtworkData','JobDefinition','PlayerProgress','PrototypeGame','TutorialHints','WorkSession']}
changes=[];errors=[];assets=0
with zipfile.ZipFile(frozen/'ExternalTest01-Source.zip') as z:
    for name,sha in manifest['sourceFiles'].items():
        old=z.read(name);new=(root/name).read_bytes()
        assert hashlib.sha256(old).hexdigest()==sha
        if new==old: assets+=1;continue
        changes.append(name)
        if name.startswith('Assets/Keshiya/Resources/External-TEST_') and name.endswith('.asset'):
            # The only permitted difference in frozen job assets is new inert metadata.
            text=new.decode('utf8').replace('\r\n','\n')
            text=re.sub(r'^  content:\n.*?(?=^  id:)', '',text,flags=re.M|re.S)
            if text!=old.decode('utf8').replace('\r\n','\n'):errors.append(name+' changed beyond metadata')
        elif name not in allowed:errors.append('Unexpected baseline modification '+name)
release=frozen/'Keshiya-ExternalTest01.zip'
assert hashlib.sha256(release.read_bytes()).hexdigest()==manifest['releaseSha256']
result={'unchangedFiles':assets,'reviewedChangedFiles':changes,'errors':errors,'releaseSha256':manifest['releaseSha256']}
(root/'TestResults-Foundation/baseline-audit.json').write_text(json.dumps(result,indent=2),encoding='utf8')
print(json.dumps(result,indent=2));raise SystemExit(bool(errors))
