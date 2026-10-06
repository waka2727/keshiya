"""Recreate the hand-panel comparison and validate the focused artwork change."""
from pathlib import Path
import hashlib
import io
import json
import subprocess
import numpy as np
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[3]
baseline = json.loads((Path(__file__).parent / 'baseline.json').read_text(encoding='utf-8'))
folder = root / 'Assets/Keshiya/ExternalArt/TEST_003'
output = root / 'Documentation/Feedback082/Adjustment'
output.mkdir(parents=True, exist_ok=True)
count = 0
def check(ok, label):
    global count
    count += 1
    assert ok, label
    print('PASS', label)

for name in ('Paper', 'Protected', 'ProtectMask', 'CompletePreview'):
    check(hashlib.sha256((folder / (name + '.png')).read_bytes()).hexdigest() == baseline['mangaHashes'][name], name + ' unchanged')
art = Image.open(folder / 'InitialPreview.png').convert('RGB')
erase = np.asarray(Image.open(folder / 'EraseMask.png').convert('L'))
protect = np.asarray(Image.open(folder / 'ProtectMask.png').convert('L'))
check(erase.shape == protect.shape == (1754, 1240), 'mask canvas alignment')
check(not erase[520:911, 825:1156].any(), 'no hand/strap target or overlap')
check(protect[520:911, 825:1156].any(), 'hand ink still protected')
check(.95 < erase.sum() / baseline['mangaOldTotalMass'] < 1, 'less than five percent of total target removed')
check(erase[:500].any() and erase[940:].any() and erase[515:915, :810].any(), 'other panels retain work')
pixels = np.asarray(art).copy()
pixels[protect > 0] = (pixels[protect > 0] * .55 + np.array([255, 90, 75]) * .45).astype('uint8')
pixels[erase > 0] = (pixels[erase > 0] * .3 + np.array([30, 150, 255]) * .7).astype('uint8')
overlay = Image.fromarray(pixels)
ImageDraw.Draw(overlay).rectangle((820, 515, 1160, 915), outline=(0, 190, 90), width=3)
overlay.crop((790, 490, 1190, 940)).save(output / 'hand-overlay.png')
old_bytes = subprocess.check_output(['git', '-c', 'safe.directory=' + root.as_posix(), 'show', baseline['commit'] + ':Assets/Keshiya/ExternalArt/TEST_003/InitialPreview.png'], cwd=root)
old = Image.open(io.BytesIO(old_bytes)).convert('RGB')
comparison = Image.new('RGB', (800, 450))
comparison.paste(old.crop((790, 490, 1190, 940)), (0, 0))
comparison.paste(art.crop((790, 490, 1190, 940)), (400, 0))
comparison.save(output / 'hand-before-after.png')
print('Checks=' + str(count) + '; Failures=0')
