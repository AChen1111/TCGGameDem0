"""Export UI Sprite PNGs, original textures and embedded fonts from MDPro3.

Requires UnityPy 1.25.3 and Pillow. Source bundles are read only.
"""
import argparse
import hashlib
import io
import json
import re
from collections import Counter, OrderedDict
from pathlib import Path, PurePosixPath

import UnityPy
from UnityPy.export import SpriteHelper


def safe_name(name):
    return re.sub(r'[<>:"/\\|?*\x00-\x1f]', '_', name).strip(' .') or 'unnamed'


def object_key(obj):
    return obj.assets_file.name, obj.path_id


def category(path, bundle):
    parts = PurePosixPath(path.replace('\\', '/')).parts
    for marker in ('Addressables', 'Resources', 'UI'):
        if marker in parts:
            return Path(*(safe_name(part) for part in parts[parts.index(marker) + 1:-1]))
    if 'Icon' in parts:
        return Path(*(safe_name(part) for part in parts[parts.index('Icon'):-1]))
    if bundle == 'data.unity3d':
        return Path('Builtin')
    if bundle == 'texture_assets_all.bundle':
        return Path('CommonUI')
    if bundle == 'font_assets_all.bundle':
        return Path('FontAtlases')
    if bundle.startswith('prefab_assets_'):
        return Path('Menus') / safe_name(bundle.removeprefix('prefab_assets_').removesuffix('.bundle'))
    return Path(safe_name(bundle.removesuffix('.bundle')))


def install_sprite_cache():
    # Individual icon bundles otherwise cache a decoded atlas per serialized file.
    # Share a bounded cache, keyed by the actual referenced texture and alpha file.
    cache = OrderedDict()

    def get_image(sprite, texture, alpha_texture):
        tex = texture.deref_parse_as_object()
        alpha = alpha_texture.deref_parse_as_object() if alpha_texture else None
        key = (object_key(tex.object_reader), object_key(alpha.object_reader) if alpha else None)
        if key in cache:
            cache.move_to_end(key)
            return cache[key]
        image = SpriteHelper.get_image_from_texture2d(tex, False)
        if alpha:
            from PIL import Image
            alpha_image = SpriteHelper.get_image_from_texture2d(alpha, False)
            image = Image.merge('RGBA', (*image.split()[:3], alpha_image.split()[0]))
        cache[key] = image
        while len(cache) > 8:
            cache.popitem(last=False)
        return image

    SpriteHelper.get_image = get_image


def write_unique(path, data, path_id):
    digest = hashlib.sha256(data).hexdigest()
    if path.exists() and hashlib.sha256(path.read_bytes()).hexdigest() != digest:
        path = path.with_name(f'{path.stem}__{path_id}{path.suffix}')
    if path.exists() and hashlib.sha256(path.read_bytes()).hexdigest() != digest:
        raise ValueError(f'Refusing to overwrite different existing file: {path}')
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)
    return path, digest


def vector(value, fields):
    return {field: getattr(value, field) for field in fields}


def export(source, destination):
    bundle_root = source / 'MDPro3_Data' / 'StandaloneWindows64' / 'MDPro3'
    bundles = sorted(p for p in bundle_root.glob('*.bundle')
                     if p.name.startswith(('icon_assets_', 'prefab_assets_', 'texture_assets_', 'font_assets_')))
    bundles.append(source / 'MDPro3_Data' / 'data.unity3d')
    print(f'Loading {len(bundles)} UI bundles and player data', flush=True)
    env = UnityPy.load(*(str(path) for path in bundles))
    install_sprite_cache()
    objects = [obj for obj in env.objects if obj.type.name in ('Sprite', 'Texture2D', 'Font')]
    print('Found', dict(Counter(obj.type.name for obj in objects)), flush=True)
    paths, bundle_categories = {}, {}
    for obj in objects:
        for path, pointer in obj.assets_file.container.items():
            key = (obj.assets_file.name, pointer.path_id)
            paths[key] = path
            if path.lower().startswith('assets/addressables/icon/'):
                bundle_categories[obj.assets_file.parent.name] = category(path, obj.assets_file.parent.name)
    manifest_path = destination / 'ui_manifest.json'
    previous = json.loads(manifest_path.read_text(encoding='utf-8'))['assets'] if manifest_path.exists() else []
    completed = {(row['asset_file'], row['path_id']): row for row in previous}
    rows, errors = [], []
    for i, obj in enumerate(objects):
        name, bundle = obj.peek_name(), obj.assets_file.parent.name
        try:
            if object_key(obj) in completed:
                rows.append(completed[object_key(obj)])
                continue
            data = obj.read()
            container = paths.get(object_key(obj), '')
            subfolder = category(container, bundle) if container else bundle_categories.get(bundle, category('', bundle))
            row = dict(type=obj.type.name, name=name, bundle=bundle, container=container,
                       asset_file=obj.assets_file.name, path_id=obj.path_id)
            if obj.type.name == 'Font':
                raw = bytes(data.m_FontData)
                if not raw:
                    row['status'] = 'no_embedded_font_data'
                    rows.append(row)
                    continue
                extension = '.otf' if raw[:4] == b'OTTO' else '.ttf'
                path, digest = write_unique(destination / 'Fonts' / (safe_name(name) + extension), raw, obj.path_id)
            else:
                if obj.type.name == 'Texture2D' and (data.m_Width == 0 or data.m_Height == 0):
                    row['status'] = 'empty_runtime_texture'
                    rows.append(row)
                    continue
                image = data.image
                buffer = io.BytesIO()
                image.save(buffer, format='PNG')
                section = 'Sprites' if obj.type.name == 'Sprite' else 'Textures'
                path, digest = write_unique(destination / section / subfolder / (safe_name(name) + '.png'), buffer.getvalue(), obj.path_id)
                row['size'] = list(image.size)
                if obj.type.name == 'Sprite':
                    row.update(original_rect=vector(data.m_Rect, ('x', 'y', 'width', 'height')),
                               pivot=vector(data.m_Pivot, ('x', 'y')),
                               border=vector(data.m_Border, ('x', 'y', 'z', 'w')),
                               pixels_per_unit=data.m_PixelsToUnits)
            row.update(file=str(path.relative_to(destination)), sha256=digest, status='exported')
            rows.append(row)
        except Exception as exc:
            errors.append(dict(type=obj.type.name, name=name, bundle=bundle, path_id=obj.path_id, error=str(exc)))
            print('ERROR', errors[-1], flush=True)
        if (i + 1) % 250 == 0:
            print(f'Processed {i + 1}/{len(objects)}; errors={len(errors)}', flush=True)
    summary = dict(source_bundles=len(bundles), objects=dict(Counter(obj.type.name for obj in objects)),
                   exported_objects=dict(Counter(row['type'] for row in rows if row['status'] == 'exported')),
                   skipped_objects=dict(Counter(row['status'] for row in rows if row['status'] != 'exported')),
                   unique_files=len({row['file'] for row in rows if row['status'] == 'exported'}), errors=len(errors))
    destination.mkdir(parents=True, exist_ok=True)
    manifest_path.write_text(json.dumps(dict(summary=summary, assets=rows, errors=errors), ensure_ascii=False, indent=2), encoding='utf-8')
    print(summary, flush=True)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('source', type=Path)
    parser.add_argument('destination', type=Path)
    args = parser.parse_args()
    export(args.source, args.destination)
