import csv
from pathlib import Path
from PIL import Image
root=Path('TableData')
colors=[('赤', 'Crimson'),('金', 'Golden'),('翠', 'Emerald'),('青', 'Azure'),('紫', 'Violet'),('银', 'Silver')]
def tone(path):
 im=Image.open(path).convert('RGBA'); im.thumbnail((32,32))
 pixels=[(r,g,b) for r,g,b,a in im.getdata() if a>80 and max(r,g,b)>50]
 if not pixels:return colors[5]
 r=sum(p[0] for p in pixels)/len(pixels);g=sum(p[1] for p in pixels)/len(pixels);b=sum(p[2] for p in pixels)/len(pixels)
 if r>g*1.22 and r>b*1.1:return colors[0]
 if r> b*1.15 and g>b*1.05:return colors[1]
 if g>r*1.12 and g>b*1.06:return colors[2]
 if b>r*1.15 and b>g*1.06:return colors[3]
 if r>b*.85 and b>g*1.15:return colors[4]
 return colors[5]
first=['红衣飞行员','银发少年','青发剑士','银发术士','怒目武者','青衣战士','绿鬃魔兽','白发少女','赤面骑士','火焰毛球','赤甲战士','白发剑客','金发少女','银翼龙','绿甲魔人','金甲龙','紫甲魔龙','黑发女仆','银甲战士','金甲骑士','赤翼魔龙','黑羽怪鸟','银发少女','黑发剑士','粉发少女','绿发精灵','青眼白龙','黑魔导','黑翼魔龙','火焰巫女','粉色精灵','金甲飞龙','水晶机械','灰鳞巨龙','黄甲战士','白发机械师','暗影龙','蓝衣飞行员','青甲战士','彩色精灵','银甲龙骑士','银翼白龙','粉色小兽','南瓜灯','紫衣少女','红发少女','青发龙骑士','汉堡怪兽','机械小兵','紫发少女']
trans=root/'Translations.csv'
with trans.open(encoding='utf-8-sig',newline='') as f: rows=list(csv.reader(f))
tr={r[0]:r for r in rows[2:]}
for kind,folder,prefix in [('avatar','Avatars','ProfileIcon'),('avatar-frame','Frames','ProfileFrame')]:
 path=root/('avatars.csv' if kind=='avatar' else 'avatar-frames.csv')
 with path.open(encoding='utf-8-sig',newline='') as f: data=list(csv.reader(f))
 if 'NameKey' not in data[0]:
  data[0].append('NameKey');data[1].append('string')
 for index,row in enumerate(data[2:]):
  ident=row[0]; key=f'shop.{kind}.{ident}'
  img=Path('Assets/UI/Sprite/ProfileCustomization')/folder/f'{prefix}{ident}_L.png'
  c,e=tone(img)
  if kind=='avatar':
   zh=first[index] if index<len(first) else f'{c}色角色肖像 {index+1:03d}'
   en=f'Character Portrait {index+1:03d}'
  else:
   zh=f'{c}色纹饰框 {index+1:02d}'
   en=f'{e} Frame {index+1:02d}'
  row[1]=zh
  if len(row)==len(data[0])-1:row.append(key)
  else:row[-1]=key
  tr[key]=[key,zh,en]
 with path.open('w',encoding='utf-8',newline='') as f:csv.writer(f,lineterminator='\n').writerows(data)
for key,zh,en in [('ui.workshop.limit.0','禁用（卡组 0 张）','Forbidden (0 in deck)'),('ui.workshop.limit.1','限制（1 张）','Limited (1 in deck)'),('ui.workshop.limit.2','准限制（2 张）','Semi-Limited (2 in deck)'),('ui.workshop.limit.3','无限制（3 张）','Unlimited (3 in deck)'),('ui.workshop.owned_and_limit','持有 {count} · {limit}','Owned {count} · {limit}'),('ui.workshop.owned_version_and_limit','{version} ×{count} · {limit}','{version} ×{count} · {limit}')]: tr[key]=[key,zh,en]
with trans.open('w',encoding='utf-8',newline='') as f:
 w=csv.writer(f,lineterminator='\n');w.writerows(rows[:2]);w.writerows(tr.values())

# CosmeticData is shared by wallpapers, so keep the table schema aligned.
wallpapers=root/'wallpapers.csv'
with wallpapers.open(encoding='utf-8-sig',newline='') as f: data=list(csv.reader(f))
if 'NameKey' not in data[0]:
 data[0].append('NameKey');data[1].append('string')
 for row in data[2:]:row.append('shop.wallpaper.'+row[0])
 with wallpapers.open('w',encoding='utf-8',newline='') as f:csv.writer(f,lineterminator='\n').writerows(data)
