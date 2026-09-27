"""Inventory and export packed Spine runtime assets without modifying source bundles.

Requires UnityPy. Run inventory first, then export with the saved inventory.
"""
import argparse
import hashlib
import json
import posixpath
from collections import Counter
from concurrent.futures import ProcessPoolExecutor
from pathlib import Path

import UnityPy
from PIL import Image


def raw_text(obj):
    return obj.read().m_Script.encode('utf-8', 'surrogateescape')


def inspect_bundle(path):
    rows, errors = [], []
    try:
        with path.open('rb') as handle:
            if not handle.read(16).startswith((b'UnityFS', b'UnityRaw', b'UnityWeb')):
                return rows, errors
        env = UnityPy.load(str(path))
        containers = {o.path_id: name for name, o in env.container.items()}
        for obj in env.objects:
            if obj.type.name != 'TextAsset':
                continue
            data = raw_text(obj)
            name = obj.peek_name()
            kind, version = None, None
            if data.lstrip().startswith(b'{'):
                try:
                    parsed = json.loads(data)
                    if 'skeleton' in parsed and 'bones' in parsed:
                        kind = 'skeleton'
                        version = parsed['skeleton'].get('spine')
                except (ValueError, UnicodeError):
                    pass
            if name and '.atlas' in name.lower() and b'size:' in data:
                kind = 'atlas'
            if kind:
                rows.append(dict(bundle=str(path), path_id=obj.path_id,
                                 container=containers.get(obj.path_id, ''), name=name,
                                 kind=kind, version=version,
                                 sha256=hashlib.sha256(data).hexdigest()))
    except Exception as exc:
        errors.append(dict(bundle=str(path), error=str(exc)))
    return rows, errors


def inventory(source, report):
    rows, errors = [], []
    files = sorted(p for p in source.rglob('*') if p.is_file())
    with ProcessPoolExecutor(max_workers=6) as pool:
        for i, (found, failed) in enumerate(pool.map(inspect_bundle, files, chunksize=8)):
            for row in found + failed:
                row['bundle'] = str(Path(row['bundle']).relative_to(source))
            rows.extend(found)
            errors.extend(failed)
            if (i + 1) % 500 == 0:
                print(f'scanned {i + 1}/{len(files)}; Spine assets={len(rows)}', flush=True)
    report.write_text(json.dumps(dict(assets=rows, errors=errors), ensure_ascii=False, indent=2), encoding='utf-8')
    print(dict(assets=len(rows), kinds=dict(Counter(r['kind'] for r in rows)), errors=len(errors)), flush=True)


def parse_atlas(text):
    pages, page, region = [], None, None
    for line in text.splitlines():
        line = line.strip()
        if not line:
            page, region = None, None
        elif page is None:
            page = dict(name=line, size=None, regions=[])
            pages.append(page)
        elif ':' not in line:
            region = dict(name=line, rotate=False)
            page['regions'].append(region)
        else:
            key, value = (part.strip() for part in line.split(':', 1))
            target = region if region is not None else page
            if key in ('size', 'xy', 'bounds'):
                target[key] = [int(x) for x in value.split(',')]
            elif key == 'rotate':
                target[key] = value in ('90', 'true')
    return pages


def write_original(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    if path.exists() and path.read_bytes() != data:
        raise ValueError(f'Refusing to overwrite different existing file: {path}')
    path.write_bytes(data)
    return hashlib.sha256(data).hexdigest()


def export_group(task):
    source, destination, group, records = task
    try:
        env = UnityPy.load(str(source / group))
        objects = {obj.path_id: obj for obj in env.objects}
        containers = {obj.path_id: path for path, obj in env.container.items()}
        textures = [obj for obj in env.objects if obj.type.name == 'Texture2D']
        skeletons = [row for row in records if row['kind'] == 'skeleton']
        paths = [row['container'] for row in records if row['container']]
        base = posixpath.commonpath(paths) if paths else ''
        if base and '.' in base.rsplit('/', 1)[-1]:
            base = posixpath.dirname(base)
        parts = Path(group).parts
        number = next(part for part in reversed(parts) if part.isdigit())
        category = '_'.join(part for part in parts if not part.isdigit())
        results = []
        for atlas in (row for row in records if row['kind'] == 'atlas'):
            atlas_parent = posixpath.dirname(atlas['container'])
            variant = posixpath.relpath(atlas_parent, base) if base and atlas_parent else (
                'packed_' + Path(atlas['bundle']).name if base else '.')
            folder = destination / number / category / variant
            def skeleton_score(row):
                parent = posixpath.dirname(row['container'])
                if not atlas_parent and row['bundle'] == atlas['bundle']:
                    return 1
                return len(posixpath.commonpath([atlas_parent, parent])) if atlas_parent and parent else 0
            skeleton = max(skeletons, key=skeleton_score)
            raw = raw_text(objects[atlas['path_id']])
            atlas_file = folder / (atlas['name'].removesuffix('.txt'))
            atlas_hash = write_original(atlas_file, raw)
            skeleton_file = folder / (skeleton['name'].removesuffix('.json') + '.json')
            skeleton_hash = write_original(skeleton_file, raw_text(objects[skeleton['path_id']]))
            checks = []
            for page in parse_atlas(raw.decode('utf-8-sig')):
                candidates = [obj for obj in textures if obj.peek_name().lower() == Path(page['name']).stem.lower()]
                matching = [obj for obj in candidates if posixpath.dirname(containers.get(obj.path_id, '')) == atlas_parent]
                candidates = matching or candidates
                if len(candidates) != 1:
                    raise ValueError(f"Texture match count={len(candidates)}: {page['name']}")
                tex = candidates[0].read()
                image = tex.image
                image_path = folder / page['name']
                image_path.parent.mkdir(parents=True, exist_ok=True)
                if image_path.exists():
                    with Image.open(image_path) as existing:
                        if existing.size != image.size or existing.convert('RGBA').tobytes() != image.convert('RGBA').tobytes():
                            raise ValueError(f'Refusing to overwrite different existing texture: {image_path}')
                else:
                    image.save(image_path)
                declared = page['size'] or list(image.size)
                sx, sy = image.width / declared[0], image.height / declared[1]
                alpha = image.getchannel('A') if 'A' in image.getbands() else image.convert('RGBA').getchannel('A')
                empty, outside = [], []
                for region in page['regions']:
                    if 'bounds' in region:
                        x, y, width, height = region['bounds']
                    else:
                        x, y = region['xy']
                        width, height = region['size']
                    if region['rotate']:
                        width, height = height, width
                    rect = (int(x*sx), int(y*sy), int((x+width)*sx), int((y+height)*sy))
                    if rect[0] < 0 or rect[1] < 0 or rect[2] > image.width or rect[3] > image.height:
                        outside.append(region['name'])
                    crop = alpha.crop(rect)
                    histogram = crop.histogram()
                    if crop.width * crop.height == 0 or sum(histogram[9:]) / max(1, crop.width * crop.height) < .02:
                        empty.append(region['name'])
                checks.append(dict(page=page['name'], declared_size=declared, actual_size=list(image.size),
                                   regions=len(page['regions']), low_alpha_regions=empty, out_of_bounds=outside,
                                   png_sha256=hashlib.sha256(image_path.read_bytes()).hexdigest()))
            parsed = json.loads(skeleton_file.read_bytes())
            results.append(dict(number=number, category=category, variant=variant,
                                folder=str(folder.relative_to(destination)), source_group=group,
                                spine_version=skeleton['version'], animations=list(parsed.get('animations', {})),
                                atlas=atlas_file.name, atlas_sha256=atlas_hash,
                                skeleton=skeleton_file.name, skeleton_sha256=skeleton_hash, pages=checks))
        return results, []
    except Exception as exc:
        return [], [dict(group=group, error=str(exc))]


def export(source, report, destination):
    data = json.loads(report.read_text(encoding='utf-8'))
    groups = {}
    for row in data['assets']:
        bundle = Path(row['bundle'])
        # MonsterCutin uses eight-digit bundle hashes, which can be all numeric.
        # Single-file numbered bundles exist only in these two source categories.
        single = bundle.parts[0] == 'MonsterCutin2' or bundle.parts[:2] == ('MasterDuel', 'Wallpaper')
        group = str(bundle if single else bundle.parent)
        groups.setdefault(group, []).append(row)
    manifest_path = destination / 'extraction_manifest.json'
    previous = json.loads(manifest_path.read_text(encoding='utf-8'))['sets'] if manifest_path.exists() else []
    completed = Counter(row['source_group'] for row in previous)
    pending = {group for group, records in groups.items()
               if completed[group] != sum(row['kind'] == 'atlas' for row in records)}
    tasks = [(source, destination, group, records) for group, records in sorted(groups.items()) if group in pending]
    rows = [row for row in previous if row['source_group'] not in pending]
    errors = []
    with ProcessPoolExecutor(max_workers=4) as pool:
        for i, (found, failed) in enumerate(pool.map(export_group, tasks)):
            rows.extend(found)
            errors.extend(failed)
            for failure in failed:
                print(failure, flush=True)
            if (i + 1) % 25 == 0:
                print(f'exported {i + 1}/{len(tasks)} groups; sets={len(rows)}; errors={len(errors)}', flush=True)
    summary = dict(groups=len(groups), sets=len(rows), numbers=len({row['number'] for row in rows}),
                   pages=sum(len(row['pages']) for row in rows), errors=len(errors))
    manifest_path.write_text(json.dumps(dict(summary=summary, sets=rows, errors=errors), ensure_ascii=False, indent=2), encoding='utf-8')
    print(summary, flush=True)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('source', type=Path)
    parser.add_argument('report', type=Path)
    parser.add_argument('--export', type=Path)
    args = parser.parse_args()
    if args.export:
        export(args.source, args.report, args.export)
    else:
        inventory(args.source, args.report)
