"""在不编译新类型的条件下, 一次性写入显式组件绑定及嵌套预制体覆盖."""
from pathlib import Path
import csv, hashlib, json, re, sys
sys.dont_write_bytecode = True
from inventory import ROOT, documents, inspect, paths

def guid(path):
    return re.search(r'^guid: (\w+)',Path(str(path)+'.meta').read_text(),re.M)[1]

csv_path=ROOT/'TableData/Localization/Translations.csv'
rows=list(csv.DictReader(csv_path.open(encoding='utf-8')))
extras=[
    ('ui.lobby.deck','卡组','Deck'),('ui.lobby.exit','退出','Exit'),('ui.lobby.shop','商城','Shop'),
    ('ui.lobby.mail','邮件','Mail'),('ui.lobby.friend','好友','Friends'),('ui.lobby.settings','设置','Settings')]
existing={r['key'] for r in rows}
for key,zh,en in extras:
    if key not in existing: rows.append({'key':key,'zh-CN':zh,'en':en})
with csv_path.open('w',encoding='utf-8',newline='') as f:
    writer=csv.DictWriter(f,fieldnames=['key','zh-CN','en']);writer.writeheader();writer.writerows(rows)

lookup={r['zh-CN']:r['key'] for r in rows if r['key'].startswith('ui.')}
lookup.update({en:key for key,zh,en in extras})
script_guid=guid(ROOT/'Assets/Shared/Localization/LocalizedText.cs')
component_ids={}
bindings=[]

for p in paths():
    data=inspect(p)
    source=p.read_text(encoding='utf-8-sig')
    asset_guid=guid(p)
    for text in data['texts']:
        name=text['name']
        # 昵称和 TMP_InputField 的编辑内容不参与本地化.
        if name in ('Text','UserID'):continue
        dynamic=name in ('Txt_Value','Txt_gold','Txt_Message','Txt_Num','Txt_RemainTime','Percent')
        key={'Txt_Value':'ui.common.gold_amount','Txt_gold':'ui.common.gold_amount',
             'Txt_Message':'ui.placeholder.new_text','Txt_Num':'ui.common.progress',
             'Txt_RemainTime':'ui.shop.remaining_days','Percent':'ui.update.percent'}.get(name)
        key=key or lookup.get(text['text'])
        if not key:raise ValueError(f'Unmapped {p}: {text}')
        cid=str(int(hashlib.sha256((p.relative_to(ROOT).as_posix()+':'+text['textId']).encode()).hexdigest()[:15],16))
        component_ids[(asset_guid,text['textId'])]=cid
        bindings.append({'asset':p.relative_to(ROOT).as_posix(),'gameObject':text['go'],'name':name,'textId':text['textId'],'componentId':cid,'key':key,'dynamic':dynamic})
        if re.search(r'^--- !u!114 &'+cid+r'$',source,re.M):continue
        docs=documents(p)
        raw=next(raw for typ,id,_,raw in docs if typ=='1' and id==text['go'])
        updated=raw.replace('  m_Layer:',f'  - component: {{fileID: {cid}}}\n  m_Layer:',1)
        source=source.replace(raw,updated)
        source+=f'''--- !u!114 &{cid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {text['go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: AChen.Shared::LocalizedText
  key: {key}
  dynamicContent: {int(dynamic)}
'''
    if source!=p.read_text(encoding='utf-8-sig'):p.write_text(source,encoding='utf-8')

for p in paths():
    source=p.read_text(encoding='utf-8-sig')
    for override in inspect(p)['overrides']:
        component=component_ids.get((override['guid'],override['source']))
        if not component:raise ValueError(f'Unmapped inherited text: {override}')
        key=lookup.get(override['text'])
        if not key:raise ValueError(f'Unmapped override: {override}')
        raw=next(raw for typ,id,_,raw in documents(p) if id==override['instance'])
        extra=f'''    - target: {{fileID: {component}, guid: {override['guid']}, type: 3}}
      propertyPath: key
      value: {key}
      objectReference: {{fileID: 0}}
'''
        if extra not in raw:
            source=source.replace(raw,raw.replace('    m_RemovedComponents:',extra+'    m_RemovedComponents:'))
        bindings.append({'asset':p.relative_to(ROOT).as_posix(),'instance':override['instance'],'key':key,'sourceComponent':component})
    if source!=p.read_text(encoding='utf-8-sig'):p.write_text(source,encoding='utf-8')

settings=ROOT/'Assets/Resources/Localization/Settings.asset'
settings.write_text(f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {guid(ROOT/'Assets/Shared/Localization/LocalizationSettings.cs')}, type: 3}}
  m_Name: Settings
  m_EditorClassIdentifier: AChen.Shared::LocalizationSettings
  chineseFont: {{fileID: 11400000, guid: {guid(ROOT/'Assets/UI/Fonts/FZZYJW SDF.asset')}, type: 2}}
  englishFont: {{fileID: 11400000, guid: {guid(ROOT/'Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset')}, type: 2}}
''',encoding='utf-8')

log_settings=ROOT/'Assets/Scripts/LogSystem/Resources/ALogSettings.asset'
log_settings.write_text(log_settings.read_text(encoding='utf-8-sig').replace('HotUpdate::ALogSettings','AChen.Shared::ALogSettings'),encoding='utf-8')
(ROOT/'Tools/Localization/bindings.json').write_text(json.dumps(bindings,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(f'Bound {len(component_ids)} direct texts and {len(bindings)-len(component_ids)} inherited text overrides.')
