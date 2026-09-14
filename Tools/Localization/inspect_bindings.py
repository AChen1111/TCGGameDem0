"""只读核对序列化绑定和 CSV, 不编译或执行游戏逻辑."""
import csv,json,re,sys
sys.dont_write_bytecode = True
from inventory import ROOT,documents,inspect,paths

rows=list(csv.DictReader((ROOT/'TableData/Localization/Translations.csv').open(encoding='utf-8')))
table={r['key']:r for r in rows}
assert len(table)==len(rows),'Duplicate CSV key'
for row in rows:
    assert row['zh-CN'] and row['en'],row['key']
    assert set(re.findall(r'\{(\w+)\}',row['zh-CN']))==set(re.findall(r'\{(\w+)\}',row['en'])),row['key']
original=[]
for line in (ROOT/'映射表.md').read_text(encoding='utf-8-sig').splitlines():
    match=re.match(r'^\| `((?:ui|err|shop|card)\.[^`]+)` \| (.*) \|$',line)
    if not match:continue
    cells=[c.strip().replace(r'\|','|').replace('<br>','\n') for c in re.split(r'(?<!\\)\|',match[2])]
    assert [table[match[1]]['zh-CN'],table[match[1]]['en']]==cells,match[1]
    original.append(match[1])
assert len(original)==411

guid=re.search(r'guid: (\w+)',(ROOT/'Assets/Shared/Localization/LocalizedText.cs.meta').read_text())[1]
direct=excluded=overrides=0
for path in paths():
    docs=documents(path)
    ids=[id for typ,id,_,raw in docs]
    assert len(ids)==len(set(ids)),f'Duplicate fileID: {path}'
    localized={}
    for typ,id,_,raw in docs:
        if f'guid: {guid}' not in raw:continue
        go=re.search(r'm_GameObject: \{fileID: (\d+)\}',raw)[1]
        assert go not in localized,(path,go)
        key=re.search(r'^  key: (.*)$',raw,re.M)[1]
        assert key in table,(path,key)
        assert not key.startswith('card.'),(path,key)
        localized[go]=id
        gameobject=next(r for t,i,_,r in docs if t=='1' and i==go)
        assert f'component: {{fileID: {id}}}' in gameobject,(path,go)
    for text in inspect(path)['texts']:
        if text['name'] in ('Text','UserID'):
            assert text['go'] not in localized,(path,text)
            excluded+=1
        else:
            assert text['go'] in localized,(path,text)
            direct+=1
    for override in inspect(path)['overrides']:
        raw=next(r for t,i,_,r in docs if i==override['instance'])
        assert 'propertyPath: key' in raw,(path,override)
        source_meta=next(p for p in (ROOT/'Assets/UI/Prefab').rglob('*.prefab.meta')
                         if 'guid: '+override['guid'] in p.read_text())
        source_ids={id for typ,id,_,body in documents(source_meta.with_suffix('')) if f'guid: {guid}' in body}
        refs=re.findall(r'- target: \{fileID: (\d+), guid: '+override['guid']+r', type: 3\}\n      propertyPath: key\n      value: (.*)',raw)
        assert any(id in source_ids and key in table for id,key in refs),(path,override)
        overrides+=1

# 静态字面量 key 必须有表项; 动态品类前缀由商品 ID 在运行时补全.
for folder in ['Assets/Scripts/UI','Assets/Scripts/Player','Assets/Scripts/Network','Assets/AOT']:
    for path in (ROOT/folder).rglob('*.cs'):
        for key in re.findall(r'"((?:ui|err|shop)\.[a-zA-Z0-9_.]+)"',path.read_text(encoding='utf-8-sig')):
            if not key.endswith('.'):assert key in table,(path,key)

print(json.dumps({'entries':len(rows),'original_entries_preserved':len(original),
                  'bound_direct_texts':direct,'bound_inherited_overrides':overrides,
                  'excluded_player_texts':excluded},ensure_ascii=False))
