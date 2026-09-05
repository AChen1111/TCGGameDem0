"""Rebuild page-1 region rects of the P14676 atlas from the pixels actually present in P14676.png.

The P14676.png that shipped with the dump is a repacked sheet whose pieces are exactly half the
size the atlas declares, so the original page-1 coordinates point at empty pixels and those
attachments render fully transparent. Page 2 matches its own image and is left untouched.

Region size is taken as declared/2 (exact by construction) and positioned so the rect is centred
on the measured art extent; the repack pads each piece by ~3px, which this reproduces.

Rotation convention was calibrated against page 2: its `Mask` is declared `rotate:90` and renders
correctly today, and turning its packed pixels by np.rot90(k=3) yields the upright art. Every
rotated piece in the page-1 repack needs the same k=3, i.e. `rotate:90`.
"""
import io
import os
import sys

import numpy as np
from PIL import Image
from scipy import ndimage

ASSET_DIR = "Assets/Art/Spine/I：P百变莱娜_P14676JS"
SOURCE_DIR = "Recovery/SpineAnimations_Complete552/I：P百变莱娜_P14676JS"
PAGE1 = "P14676.png"

# Logical size of each page-1 region as declared by the original dump, before the coordinates were
# found to be wrong. Only the sizes are trusted (they are exactly 2x the sheet's art).
DECLARED = {
    "Body": (998, 1400),
    "Eye1": (159, 141),
    "Eye2": (159, 141),
    "Head": (412, 426),
    "LeftHair": (462, 753),
    "LeftLeg": (785, 591),
    "MaskBack": (105, 197),
    "RightHair": (239, 851),
    "RightUpperArm": (666, 649),
    "TailA": (817, 760),
}
PAD = 3


def parse_atlas(text):
    pages, page, region = [], None, None
    for ln in text.replace("\r\n", "\n").split("\n"):
        if not ln.strip():
            page, region = None, None
            continue
        if page is None:
            page = {"name": ln.strip(), "header": [], "regions": []}
            pages.append(page)
            continue
        if ":" not in ln:
            region = {"name": ln, "rot": False, "extra": []}
            page["regions"].append(region)
            continue
        k, v = [s.strip() for s in ln.split(":", 1)]
        if region is None:
            page["header"].append((k, v))
        elif k == "bounds":
            p = [int(q) for q in v.split(",")]
            region["x"], region["y"], region["w"], region["h"] = p[:4]
        elif k == "rotate":
            region["rot"] = v in ("90", "true")
        else:
            region["extra"].append((k, v))
    return pages


def measure(alpha):
    """Art extents on the sheet. The dilation only merges fragments of one piece."""
    lab, count = ndimage.label(ndimage.binary_dilation(alpha > 8, iterations=PAD))
    solid = alpha > 2
    out = []
    for i in range(1, count + 1):
        ys, xs = np.where((lab == i) & solid)
        if ys.size == 0:
            continue
        x, y = int(xs.min()), int(ys.min())
        w, h = int(xs.max()) - x + 1, int(ys.max()) - y + 1
        if (w + 2 * PAD) * (h + 2 * PAD) >= 400:
            out.append([x, y, w, h])
    return out


def gray(img, x, y, w, h):
    return np.array(img.crop((x, y, x + w, y + h)).convert("L"))


def similarity(patch, ref, k):
    m = np.rot90(patch, k)
    m = np.array(Image.fromarray(m).resize((ref.shape[1], ref.shape[0]), Image.LANCZOS))
    return 1.0 - float(np.abs(m / 255.0 - ref / 255.0).mean())


img1 = Image.open(os.path.join(ASSET_DIR, PAGE1)).convert("RGBA")
img2 = Image.open(os.path.join(ASSET_DIR, "P14676_2.png")).convert("RGBA")
alpha1 = np.array(img1)[:, :, 3]
art = measure(alpha1)
print("图上部件数:", len(art))

pages = parse_atlas(io.open(os.path.join(ASSET_DIR, "P14676.atlas.txt"), encoding="utf-8").read())
p1, p2 = pages[0], pages[1]
if p1["name"] != PAGE1 or sorted(r["name"] for r in p1["regions"]) != sorted(DECLARED):
    sys.exit("atlas 结构与预期不符")


def cost(box, w, h):
    """Cost of interpreting box as an unrotated / rotated instance of a w x h region."""
    _, _, pw, ph = box
    return abs(pw - w) + abs(ph - h)


def candidates(w, h, pool, tol=8):
    out = []
    for b in pool:
        un, rot = cost(b, w - 2 * PAD, h - 2 * PAD), cost(b, h - 2 * PAD, w - 2 * PAD)
        if min(un, rot) <= 2 * tol:
            out.append((b, rot < un, min(un, rot)))
    return out


# Mask lives on both pages; matching it identifies which same-sized piece is Head.
mask = next(r for r in p2["regions"] if r["name"] == "Mask")
mask_packed = gray(img2, mask["x"], mask["y"], mask["h"], mask["w"])
head_w, head_h = (v / 2.0 for v in DECLARED["Head"])
head_cand = [b for b, _, _ in candidates(head_w, head_h, art)]
if len(head_cand) != 2:
    sys.exit("Head/Mask 候选数量异常: %s" % head_cand)
scored = sorted(((similarity(gray(img1, *b), mask_packed, 1), tuple(b)) for b in head_cand),
                reverse=True)
mask_box, head_box = list(scored[0][1]), list(scored[1][1])
print("Head/Mask 消歧: Mask=%s (%.4f), Head=%s (%.4f)"
      % (mask_box, scored[0][0], head_box, scored[1][0]))

# Eye1 and Eye2 share position, rotation and size in the skeleton (two ~94% identical variants of
# the same eye), so the dump cannot tell them apart. Assign in raster order.
eye_w, eye_h = (v / 2.0 for v in DECLARED["Eye1"])
eye_boxes = sorted((b for b, _, _ in candidates(eye_w, eye_h, art)), key=lambda b: (b[1], b[0]))
if len(eye_boxes) != 2:
    sys.exit("Eye 候选数量异常: %s" % eye_boxes)

pool = [b for b in art if b != mask_box]
plan = {}
for r in p1["regions"]:
    name = r["name"]
    dw, dh = (v / 2.0 for v in DECLARED[name])
    if name == "Head":
        box, rotated = head_box, False
    elif name in ("Eye1", "Eye2"):
        box = eye_boxes[0 if name == "Eye1" else 1]
        rotated = cost(box, dh - 2 * PAD, dw - 2 * PAD) < cost(box, dw - 2 * PAD, dh - 2 * PAD)
    else:
        cand = candidates(dw, dh, pool)
        if len(cand) != 1:
            sys.exit("%s 匹配不唯一: %s" % (name, cand))
        box, rotated, _ = cand[0]
    if box in pool:
        pool.remove(box)
    # 半数尺寸常为 x.5, round 会走银行家舍入, 这里固定向上取整以贴近真实矩形.
    w, h = int(dw + 0.5), int(dh + 0.5)
    page_w, page_h = (h, w) if rotated else (w, h)
    ax, ay, aw, ah = box
    plan[name] = {"x": ax - (page_w - aw) // 2, "y": ay - (page_h - ah) // 2,
                  "w": w, "h": h, "rot": rotated}

print("\n%-15s %-24s %-24s %s" % ("region", "旧 bounds", "新 bounds", "rotate"))
for r in p1["regions"]:
    n = plan[r["name"]]
    print("%-15s %-24s %-24s %s" % (
        r["name"],
        "%d,%d,%d,%d%s" % (r["x"], r["y"], r["w"], r["h"], " r90" if r["rot"] else ""),
        "%d,%d,%d,%d" % (n["x"], n["y"], n["w"], n["h"]),
        "90" if n["rot"] else "-"))

print("\n覆盖率校验:")
bad = 0
for name, n in plan.items():
    pw, ph = (n["h"], n["w"]) if n["rot"] else (n["w"], n["h"])
    if n["x"] < 0 or n["y"] < 0 or n["x"] + pw > 2048 or n["y"] + ph > 2048:
        sys.exit("%s 矩形越出页边界" % name)
    cover = (alpha1[n["y"]:n["y"] + ph, n["x"]:n["x"] + pw] > 8).mean()
    bad += cover < 0.05
    print("  %-15s %.3f" % (name, cover))
if bad:
    sys.exit("仍有 %d 个区域落在空白像素, 中止写入" % bad)

for path in (os.path.join(ASSET_DIR, "P14676.atlas.txt"), os.path.join(SOURCE_DIR, "P14676.atlas")):
    if not os.path.exists(path):
        print("跳过不存在的文件:", path)
        continue
    text = io.open(path, encoding="utf-8").read()
    newline = "\r\n" if "\r\n" in text else "\n"
    out = []
    for i, page in enumerate(parse_atlas(text)):
        if i:
            out.append("")
        out.append(page["name"])
        out += ["%s:%s" % (k, v) for k, v in page["header"]]
        for r in page["regions"]:
            n = plan.get(r["name"]) if page["name"] == PAGE1 else None
            src = n if n else r
            out.append(r["name"])
            out.append("bounds:%d,%d,%d,%d" % (src["x"], src["y"], src["w"], src["h"]))
            if src["rot"]:
                out.append("rotate:90")
            out += ["%s:%s" % (k, v) for k, v in r["extra"]]
    io.open(path, "w", encoding="utf-8", newline="").write(newline.join(out) + newline)
    print("已写入", path)
