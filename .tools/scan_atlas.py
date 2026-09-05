import io
import os

import numpy as np
from PIL import Image

Image.MAX_IMAGE_PIXELS = None
ROOT = "Recovery/SpineAnimations_Complete552"

PAGE_KEYS = {"size", "format", "filter", "repeat", "pma", "scale"}
REGION_KEYS = {"xy", "size", "bounds", "offset", "orig", "offsets", "rotate", "index"}


def parse_atlas(text):
    """Follows Atlas.cs: blank line ends a page; first line of a block is the page name,
    key:value lines are page fields, a line without ':' starts a region."""
    pages, page, region = [], None, None
    for ln in text.replace("\r\n", "\n").split("\n"):
        if not ln.strip():
            page, region = None, None
            continue
        if page is None:
            page = {"name": ln.strip(), "regions": [], "size": None}
            pages.append(page)
            region = None
            continue
        if ":" not in ln:
            region = {"name": ln, "x": 0, "y": 0, "w": 0, "h": 0, "rot": False}
            page["regions"].append(region)
            continue
        k, v = ln.split(":", 1)
        k, v = k.strip(), v.strip()
        parts = [p.strip() for p in v.split(",")]
        if region is None:
            if k == "size":
                page["size"] = [int(parts[0]), int(parts[1])]
            continue
        if k == "bounds":
            region["x"], region["y"], region["w"], region["h"] = [int(p) for p in parts[:4]]
        elif k == "xy":
            region["x"], region["y"] = int(parts[0]), int(parts[1])
        elif k == "size":
            region["w"], region["h"] = int(parts[0]), int(parts[1])
        elif k == "rotate":
            region["rot"] = v in ("90", "true")
    return pages


rows = []
names = sorted(d for d in os.listdir(ROOT) if os.path.isdir(os.path.join(ROOT, d)))
for i, d in enumerate(names):
    folder = os.path.join(ROOT, d)
    atlases = [f for f in os.listdir(folder) if f.endswith(".atlas") or f.endswith(".atlas.txt")]
    if not atlases:
        rows.append((d, "no-atlas", 0, 0, ""))
        continue
    pages = parse_atlas(io.open(os.path.join(folder, atlases[0]), encoding="utf-8").read())
    total, empty, notes = 0, [], []
    for pg in pages:
        path = os.path.join(folder, pg["name"])
        if not os.path.exists(path):
            notes.append("缺页文件 " + pg["name"])
            continue
        alpha = np.array(Image.open(path).convert("RGBA"))[:, :, 3] > 8
        ph, pw = alpha.shape
        # Spine 用声明的页尺寸归一化 UV, 所以整体缩放的图仍能正确采样.
        # 两种坐标解释(原样 / 按实际比例换算)都试, 取覆盖率更好的那个, 避免误判整体缩放的页.
        candidates = [(1.0, 1.0)]
        if pg["size"] and (pg["size"][0] != pw or pg["size"][1] != ph):
            notes.append("%s 实际 %dx%d 声明 %dx%d" % (pg["name"], pw, ph, pg["size"][0], pg["size"][1]))
            candidates.append((pw / pg["size"][0], ph / pg["size"][1]))
        best = None
        for sx, sy in candidates:
            miss = []
            for r in pg["regions"]:
                w, h = (r["h"], r["w"]) if r["rot"] else (r["w"], r["h"])
                x0, y0 = int(r["x"] * sx), int(r["y"] * sy)
                sub = alpha[y0:y0 + max(1, int(h * sy)), x0:x0 + max(1, int(w * sx))]
                if sub.size == 0 or sub.mean() < 0.02:
                    miss.append(pg["name"] + "/" + r["name"])
            if best is None or len(miss) < len(best):
                best = miss
        total += len(pg["regions"])
        empty.extend(best)
    if empty:
        status = "BROKEN"
    elif notes:
        status = "RESCALED"
    else:
        status = "ok"
    rows.append((d, status, total, len(empty), "; ".join(notes + empty[:8])))
    if (i + 1) % 100 == 0:
        print("scanned", i + 1, "/", len(names), flush=True)

broken = [r for r in rows if r[1] == "BROKEN"]
rescaled = [r for r in rows if r[1] == "RESCALED"]
with io.open(".tools/atlas_scan.txt", "w", encoding="utf-8") as f:
    f.write("总目录 %d\n渲染会缺件(区域落在空白像素) %d\n仅页尺寸被整体缩放(可正常渲染) %d\n\n"
            % (len(rows), len(broken), len(rescaled)))
    f.write("=== 渲染会缺件 ===\n")
    for d, status, total, nempty, detail in broken:
        f.write("%s\n  regions=%d 空白区域=%d\n  %s\n" % (d, total, nempty, detail))
    f.write("\n=== 仅整体缩放 ===\n")
    for d, status, total, nempty, detail in rescaled:
        f.write("%s  %s\n" % (d, detail))
print("done. broken:", len(broken), "rescaled:", len(rescaled), "/", len(rows))
