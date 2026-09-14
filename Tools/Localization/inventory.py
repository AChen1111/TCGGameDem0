from pathlib import Path
import re,json
ROOT=Path(__file__).resolve().parents[2]
def documents(path):
    s=path.read_text(encoding='utf-8-sig')
    return [(m[1],m[2],m[3],m[0]) for m in re.finditer(r'^--- !u!(\d+) &(-?\d+)([^\n]*)\n(.*?)(?=^--- !u!|\Z)',s,re.M|re.S)]
def scalar(s):
    if s.startswith('"'): return json.loads(s)
    if s.startswith("'"): return s[1:-1].replace("''", "'")
    return s
def inspect(path):
    docs=documents(path)
    names={id:scalar(re.search(r'^  m_Name: (.*)$',raw,re.M)[1]) for typ,id,_,raw in docs if typ=='1' and re.search(r'^  m_Name: (.*)$',raw,re.M)}
    rows=[]
    for typ,id,_,raw in docs:
        text=re.search(r'^  m_text: (.*)$',raw,re.M)
        if text:
            go=re.search(r'm_GameObject: \{fileID: (\d+)\}',raw)[1]
            rows.append({'textId':id,'go':go,'name':names.get(go),'text':scalar(text[1])})
    overrides=[]
    for typ,id,_,raw in docs:
        if typ!='1001':continue
        for m in re.finditer(r'- target: \{fileID: (-?\d+), guid: (\w+), type: 3\}\n      propertyPath: m_text\n      value: (.*)',raw):
            overrides.append({'instance':id,'source':m[1],'guid':m[2],'text':scalar(m[3])})
    return {'path':path.relative_to(ROOT).as_posix(),'texts':rows,'overrides':overrides}
def paths():
    return sorted(list((ROOT/'Assets/UI/Prefab').rglob('*.prefab'))+list((ROOT/'Assets/Resources').rglob('*.prefab'))+[p for p in (ROOT/'Assets/Scenes').glob('*.unity') if p.name!='Demo.unity'])
if __name__=='__main__':
    for p in paths():
        result=inspect(p)
        if result['texts'] or result['overrides']: print(json.dumps(result,ensure_ascii=False))
