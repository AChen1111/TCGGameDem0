"""Copy user-provided original sprites with their exported import metadata."""
from pathlib import Path
import json, shutil, hashlib

source = Path('C:/Users/ldc20/OneDrive/Desktop/UI')
dest = Path('Assets/UI/Sprite/Battle')
assets = json.loads((source / 'ui_manifest.json').read_text(encoding='utf-8-sig'))['assets']
records = json.loads((dest / 'import-metadata.json').read_text(encoding='utf-8-sig'))
names = {'GUI_LP_Blue','GUI_LP_Red','GUI_PhaseButtonBase2','GUI_PhaseButtonCursor2',
         'img_Phase_near','img_Phase_far','img_TunChange_bg01','img_TunChange_bg02'}
for icon in ['04','05','06','07','08','09','11','13']:
    names.update(f'GUI_T_DuelButtonActIcon{icon}_{state}' for state in range(1,5))
for name in sorted(names):
    original = next(a for a in assets if a['type'] == 'Sprite' and a['name'] == name)
    path = source / 'Sprites' / 'CommonUI' / (name + '.png')
    shutil.copyfile(path, dest / path.name)
    record = dict(name=name, pivot=original['pivot'], border=original['border'],
                  pixelsPerUnit=original.get('pixels_per_unit',100), source=str(path),
                  sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    records = [r for r in records if r['name'] != name] + [record]
(dest/'import-metadata.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print('Copied original sprites:',len(names))
