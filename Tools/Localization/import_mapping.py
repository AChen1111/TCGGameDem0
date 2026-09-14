"""一次性从初始 Markdown 导入 CSV; 后续请直接维护 CSV."""
import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
target = ROOT / 'TableData/Localization/Translations.csv'
if target.exists():
    raise SystemExit('CSV 已存在, 不覆盖后续维护的翻译.')
rows = []
for line in (ROOT / '映射表.md').read_text(encoding='utf-8-sig').splitlines():
    match = re.match(r'^\| `((?:ui|err|shop|card)\.[^`]+)` \| (.*) \|$', line)
    if not match:
        continue
    cells = re.split(r'(?<!\\)\|', match[2])
    if len(cells) != 2:
        raise ValueError(line)
    rows.append([match[1]] + [c.strip().replace(r'\|', '|').replace('<br>', '\n') for c in cells])
if len(rows) != 411 or len({r[0] for r in rows}) != 411:
    raise ValueError(f'原表数量或唯一性异常: {len(rows)}')
rows += [
    ['ui.loading.animated', '正在加载中{dots}', 'Loading{dots}'],
    ['err.addressables_update_failed', 'Addressables 更新失败：{message}', 'Addressables update failed: {message}'],
    ['err.unsupported_platform', '当前平台不支持内容更新：{message}', 'Content updates are not supported on this platform: {message}'],
]
target.parent.mkdir(parents=True, exist_ok=True)
with target.open('w', encoding='utf-8', newline='') as output:
    writer = csv.writer(output)
    writer.writerow(['key', 'zh-CN', 'en'])
    writer.writerows(rows)
print(f'Imported {len(rows)} entries, including all 411 original entries.')
