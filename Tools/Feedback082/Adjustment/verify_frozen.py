"""Read-only validation. Never invokes the External Test packaging command."""
from pathlib import Path
import hashlib
import zipfile

root = Path(__file__).resolve().parents[3]
archive = root.parent / 'outputs/ExternalTest01-Frozen/Keshiya-ExternalTest01.zip'
lines = []
def check(ok, label):
    assert ok, label
    lines.append('PASS ' + label)

with zipfile.ZipFile(archive) as z:
    names = z.namelist()
    check(z.testzip() is None, 'ZIP CRC valid')
    for name in ('Keshiya.exe', 'UnityPlayer.dll', 'UnityCrashHandler64.exe', 'README.txt', 'Feedback.txt'):
        check(any(n.endswith('/' + name) for n in names), name + ' included')
    for folder in ('Keshiya_Data', 'MonoBleedingEdge', 'D3D12'):
        check(any('/' + folder + '/' in n for n in names), folder + ' runtime included')
    check(not any(n.endswith(('.cs', '.pdb', '.log')) or '/TestResults' in n for n in names), 'no sources tests logs debug symbols')
    check(len({n.split('/')[0] for n in names}) == 1 and all('..' not in n.split('/') and not n.startswith('/') for n in names), 'single safe top-level directory')
    for name in ('README.txt', 'Feedback.txt'):
        text = z.read(next(n for n in names if n.endswith('/' + name))).decode('utf-8-sig')
        check('C:\\Users\\' not in text and 'C:/Users/' not in text, name + ' no developer path')
    readme = z.read(next(n for n in names if n.endswith('/README.txt'))).decode('utf-8-sig')
    check('Ctrl' in readme and 'Wheel' in readme, 'documented final wheel controls')
digest = hashlib.sha256(archive.read_bytes()).hexdigest()
assert digest == '9094dea45f531186e9e8be242a7dbf78a7ff37afb90c55ffd5e532ac8539a922', 'Frozen archive changed'
report = '\n'.join(lines) + f'\nChecks={len(lines)}; Failures=0\nSHA256={digest}\n'
folder = root / 'TestResults-Feedback082-Adjustment'
folder.mkdir(exist_ok=True)
(folder / 'archive.txt').write_text(report, encoding='utf-8')
print(report)
