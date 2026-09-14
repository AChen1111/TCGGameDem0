"""只读检查语言导表产物与 CSV 是否逐字段一致, 不启动 Unity 或编译 C#."""
import csv
import hashlib
import io
import json
import struct
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
schema_file = Path(__file__).with_name('Localization.schema.json')
schema = json.loads(schema_file.read_text(encoding='utf-8-sig'))
output = ROOT / schema['bytesOutput']
stream = io.BytesIO(output.read_bytes())
assert stream.read(4) == b'ETB1', 'Invalid format'
fingerprint = hashlib.sha256(schema_file.read_bytes()).digest()
assert stream.read(32) == fingerprint, 'Schema mismatch'
count = struct.unpack('<i', stream.read(4))[0]

def read_string():
    length = 0
    for shift in range(0, 35, 7):
        byte = stream.read(1)
        assert byte, 'Truncated string length'
        length |= (byte[0] & 127) << shift
        if byte[0] < 128:
            value = stream.read(length)
            assert len(value) == length, 'Truncated string'
            return value.decode('utf-8')
    raise AssertionError('Invalid string length')

with (ROOT / schema['source']).open(encoding='utf-8-sig', newline='') as source:
    rows = list(csv.DictReader(source))
assert count == len(rows), 'Row count mismatch'
for row in rows:
    for field in schema['columns']:
        assert field['type'] == 'string', 'Localization inspection expects string fields'
        expected = row[field['header']].replace('\r\n', '\n').replace('<br>', '\n')
        assert read_string() == expected, (row['key'], field['name'])
assert stream.read() == b'', 'Trailing data'
assert fingerprint.hex() in (ROOT / schema['csOutput']).read_text(encoding='utf-8'), 'Generated C# schema differs'
assert not (ROOT / 'Assets/Resources/Localization/Translations.csv').exists(), 'Runtime CSV remains'
print(json.dumps({'rows': count, 'bytes': output.stat().st_size, 'all_fields_preserved': True}))
