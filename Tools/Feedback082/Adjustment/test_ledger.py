"""Aggregate declared assertion counts, never count PASS summary headings twice."""
from pathlib import Path
import json
import re
import subprocess
import sys

root = Path(__file__).resolve().parents[3]
output = root / 'TestResults-Feedback082-Adjustment'
output.mkdir(exist_ok=True)
preview = subprocess.check_output([sys.executable, '-X', 'utf8', str(Path(__file__).with_name('preview.py'))], text=True, encoding='utf-8')
(output / 'artwork.txt').write_text(preview, encoding='utf-8')
processes = json.loads((root / 'TestResults-Foundation/runtime-processes.json').read_text(encoding='utf-8-sig'))
assert len(processes) == 14 and all(p['exit'] == 0 for p in processes), 'Complete all runtime suites first'
rows = []
def add(path, group):
    s = path.read_text(encoding='utf-8-sig')
    assert not re.search(r'^FAIL\b', s, re.M), path.name
    counts = re.findall(r'Checks=(\d+)(?:; Failures=(\d+))?', s)
    if counts:
        count, failures = counts[-1]
        assert not failures or failures == '0', path.name
    else:
        count = re.search(r'^PASS (\d+) .*checks|^PASS (\d+) checks', s)[0].split()[1]
    rows.append({'suite': path.relative_to(root).as_posix(), 'group': group, 'checks': int(count), 'failures': 0})

for p in sorted((root / 'TestResults').glob('*checks.txt')): add(p, 'legacy-editor')
for name, group in [('TestResults-External/editor.txt', 'artwork-editor'), ('TestResults-External/asset-validation.txt', 'artwork-python'), ('TestResults-Foundation/editor.txt', 'foundation-editor'), ('TestResults-Feedback082/checks.txt', 'feedback082-editor'), ('TestResults-Feedback082-Adjustment/checks.txt', 'new-editor'), ('TestResults-Feedback082-Adjustment/artwork.txt', 'new-artwork'), ('TestResults-Feedback082-Adjustment/archive.txt', 'frozen-archive')]: add(root / name, group)
runtime = sorted((root / 'Builds/Feedback082-Adjustment-Normal').glob('TestResults*/runtime.txt'))
assert len(runtime) == 14, 'Expected 14 runtime report files'
for p in runtime: add(p, 'runtime')
python = json.loads((root / 'TestResults-Foundation/python-tests.json').read_text(encoding='utf-8-sig'))
assert python['failures'] == 0
rows.append({'suite': 'JobPipeline Python', 'group': 'pipeline-python', **python})
total = sum(r['checks'] for r in rows)
new = sum(r['checks'] for r in rows if r['group'].startswith('new-'))
result = {'previousActual': total-new, 'added': new, 'total': total, 'failures': 0, 'suites': rows}
(output / 'ledger.json').write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k != 'suites'}))
