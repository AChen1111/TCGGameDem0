from __future__ import annotations

import json
import re
import subprocess
from dataclasses import dataclass
from pathlib import Path, PurePosixPath
from typing import Any

import UnityPy


CACHE_ROOT = Path(
    r"D:\SteamLibrary\steamapps\common\Yu-Gi-Oh!  Master Duel\LocalData\1031741c\0000"
)
OUTPUT_ROOT = Path(__file__).resolve().parent


@dataclass(frozen=True)
class Target:
    asset_name: str
    bundle_hash: str

    @property
    def bundle_path(self) -> Path:
        return CACHE_ROOT / self.bundle_hash[:2] / self.bundle_hash

    @property
    def output_name(self) -> str:
        return self.asset_name.removesuffix("_SkeletonData")


TARGETS = [
    Target("P7969JS_SkeletonData", "00b6ad30"),
    Target("P19891JS_SkeletonData", "008f5e8a"),
    Target("P15123JS_SkeletonData", "0069f808"),
    Target("P18732JS_SkeletonData", "0029d3bc"),
    Target("P15242JS_SkeletonData", "01db6255"),
    Target("P12325JS_SkeletonData", "02bd5124"),
    Target("P21614JS_SkeletonData", "02b2ed9f"),
    Target("P16528JS_SkeletonData", "02989946"),
    Target("P21907JS_SkeletonData", "03ea58c0"),
    Target("P20519JS_SkeletonData", "03d1b28e"),
    Target("P17433JS_SkeletonData", "039269ec"),
    Target("Sp780007_12JS_SkeletonData", "034aad4f"),
    Target("P17445JS_SkeletonData", "04e97948"),
    Target("P19842JS_SkeletonData", "04e1021c"),
    Target("P13934JS_SkeletonData", "04c80185"),
    Target("P3438JS_SkeletonData", "04b8a600"),
    Target("P17247JS_SkeletonData", "04b03bb3"),
    Target("P8258JS_SkeletonData", "049d60b6"),
    Target("Sp780006_04JS_SkeletonData", "0437f394"),
    Target("Spine590001JS_SkeletonData", "02e7b7dc"),
]


bundle_cache: dict[Path, tuple[Any, dict[str, Any]]] = {}
cab_to_bundle: dict[str, Path] = {}
named_object_cache: dict[tuple[str, str], Any] = {}


def load_bundle(path: Path) -> tuple[Any, dict[str, Any]]:
    path = path.resolve()
    cached = bundle_cache.get(path)
    if cached is not None:
        return cached

    environment = UnityPy.load(str(path))
    serialized_files: dict[str, Any] = {}
    for obj in environment.objects:
        assets_file = obj.assets_file
        serialized_files[assets_file.name] = assets_file

    bundle_cache[path] = (environment, serialized_files)
    for cab_name in serialized_files:
        cab_to_bundle[cab_name] = path
    return environment, serialized_files


def external_cab(assets_file: Any, file_id: int) -> str:
    if file_id <= 0 or file_id > len(assets_file.externals):
        raise ValueError(f"Invalid external file ID: {file_id}")
    return PurePosixPath(assets_file.externals[file_id - 1].path).name


def locate_cabs(cab_names: set[str]) -> None:
    missing = {name for name in cab_names if name not in cab_to_bundle}
    if not missing:
        return

    pattern = "|".join(re.escape(name) for name in sorted(missing))
    result = subprocess.run(
        ["rg", "-a", "-l", "--no-messages", pattern, str(CACHE_ROOT)],
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    if result.returncode not in (0, 1):
        raise RuntimeError(result.stderr.strip() or "rg failed while locating CAB files")

    for line in result.stdout.splitlines():
        candidate = Path(line.strip())
        if not candidate.is_file():
            continue
        try:
            load_bundle(candidate)
        except Exception:
            continue

    still_missing = sorted(name for name in missing if name not in cab_to_bundle)
    if still_missing:
        raise FileNotFoundError(f"Could not locate CAB files: {still_missing}")


def resolve_pointer(source_file: Any, pointer: dict[str, Any]) -> Any:
    file_id = int(pointer["m_FileID"])
    path_id = int(pointer["m_PathID"])
    if file_id == 0:
        target_file = source_file
    else:
        cab_name = external_cab(source_file, file_id)
        locate_cabs({cab_name})
        _, serialized_files = load_bundle(cab_to_bundle[cab_name])
        target_file = serialized_files[cab_name]
    try:
        return target_file.objects[path_id]
    except KeyError as exc:
        raise KeyError(f"Missing Path ID {path_id} in {target_file.name}") from exc


def text_asset_bytes(obj: Any) -> tuple[str, bytes]:
    data = obj.read()
    content = data.m_Script
    if isinstance(content, str):
        raw = content.encode("utf-8", errors="surrogateescape")
    else:
        raw = bytes(content)
    return data.m_Name, raw


def iter_texture_pointers(material_tree: dict[str, Any]):
    saved = material_tree.get("m_SavedProperties", {})
    for entry in saved.get("m_TexEnvs", []):
        if isinstance(entry, (list, tuple)) and len(entry) == 2:
            property_name, texture_environment = entry
        elif isinstance(entry, dict):
            property_name = entry.get("first", "Texture")
            texture_environment = entry.get("second", {})
        else:
            continue
        pointer = texture_environment.get("m_Texture", {})
        if int(pointer.get("m_PathID", 0)) != 0:
            yield str(property_name), pointer


def find_named_objects(
    type_name: str, names: set[str], *, strict: bool = True
) -> dict[str, Any]:
    found = {
        name: named_object_cache[(type_name, name)]
        for name in names
        if (type_name, name) in named_object_cache
    }
    remaining = set(names) - set(found)
    if not remaining:
        return found
    pattern = "|".join(re.escape(name) for name in sorted(remaining))
    result = subprocess.run(
        ["rg", "-a", "-l", "--no-messages", pattern, str(CACHE_ROOT)],
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    if result.returncode not in (0, 1):
        raise RuntimeError(result.stderr.strip() or "rg failed while locating named assets")

    for line in result.stdout.splitlines():
        candidate = Path(line.strip())
        if not candidate.is_file():
            continue
        try:
            environment, _ = load_bundle(candidate)
            for obj in environment.objects:
                if obj.type.name != type_name:
                    continue
                data = obj.read()
                name = getattr(data, "m_Name", "")
                if name in remaining:
                    found[name] = obj
                    named_object_cache[(type_name, name)] = obj
                    remaining.remove(name)
        except Exception:
            continue
        if not remaining:
            break

    if remaining and strict:
        raise FileNotFoundError(f"Could not locate {type_name} assets: {sorted(remaining)}")
    return found


def find_skeleton_asset(target: Target) -> tuple[Any, Any]:
    _, serialized_files = load_bundle(target.bundle_path)
    for assets_file in serialized_files.values():
        for obj in assets_file.objects.values():
            if obj.type.name != "MonoBehaviour":
                continue
            try:
                tree = obj.read_typetree()
            except Exception:
                continue
            if tree.get("m_Name") == target.asset_name:
                return obj, assets_file
    raise LookupError(f"SkeletonDataAsset not found: {target.asset_name}")


def prefetch_dependencies() -> None:
    direct_cabs: set[str] = set()
    skeletons: list[tuple[Any, Any]] = []
    for target in TARGETS:
        skeleton_object, skeleton_file = find_skeleton_asset(target)
        skeletons.append((skeleton_object, skeleton_file))
        tree = skeleton_object.read_typetree()
        pointers = [tree["skeletonJSON"], *tree.get("atlasAssets", [])]
        for pointer in pointers:
            file_id = int(pointer["m_FileID"])
            if file_id > 0:
                direct_cabs.add(external_cab(skeleton_file, file_id))
    locate_cabs(direct_cabs)

    atlas_text_cabs: set[str] = set()
    atlas_objects: list[Any] = []
    for skeleton_object, skeleton_file in skeletons:
        tree = skeleton_object.read_typetree()
        for pointer in tree.get("atlasAssets", []):
            atlas_object = resolve_pointer(skeleton_file, pointer)
            atlas_objects.append(atlas_object)
            atlas_tree = atlas_object.read_typetree()
            atlas_pointer = atlas_tree["atlasFile"]
            file_id = int(atlas_pointer["m_FileID"])
            if file_id > 0:
                atlas_text_cabs.add(external_cab(atlas_object.assets_file, file_id))
    locate_cabs(atlas_text_cabs)

    page_stems: set[str] = set()
    for atlas_object in atlas_objects:
        atlas_tree = atlas_object.read_typetree()
        atlas_text_object = resolve_pointer(atlas_object.assets_file, atlas_tree["atlasFile"])
        _, content = text_asset_bytes(atlas_text_object)
        atlas_text = content.decode("utf-8-sig")
        for line in atlas_text.splitlines():
            stripped = line.strip()
            if stripped.lower().endswith((".png", ".jpg", ".jpeg", ".webp")):
                page_stems.add(Path(stripped).stem)
    find_named_objects("Texture2D", page_stems, strict=False)


def write_text_asset(folder: Path, obj: Any, preferred_suffix: str) -> Path:
    name, content = text_asset_bytes(obj)
    path = folder / name
    if not path.name.lower().endswith(preferred_suffix.lower()):
        path = path.with_name(path.name + preferred_suffix)
    path.write_bytes(content)
    return path


def extract_target(target: Target) -> dict[str, Any]:
    skeleton_object, skeleton_file = find_skeleton_asset(target)

    target_folder = OUTPUT_ROOT / target.output_name
    target_folder.mkdir(parents=True, exist_ok=True)
    skeleton_tree = skeleton_object.read_typetree()

    skeleton_json_object = resolve_pointer(skeleton_file, skeleton_tree["skeletonJSON"])
    skeleton_path = write_text_asset(target_folder, skeleton_json_object, ".json")
    skeleton_json = json.loads(skeleton_path.read_text(encoding="utf-8-sig"))

    atlas_paths: list[Path] = []
    texture_paths: list[Path] = []
    source_bundles = {str(target.bundle_path)}

    for atlas_pointer in skeleton_tree.get("atlasAssets", []):
        atlas_object = resolve_pointer(skeleton_file, atlas_pointer)
        source_bundles.add(str(cab_to_bundle[atlas_object.assets_file.name]))
        atlas_tree = atlas_object.read_typetree()

        atlas_text_object = resolve_pointer(atlas_object.assets_file, atlas_tree["atlasFile"])
        source_bundles.add(str(cab_to_bundle[atlas_text_object.assets_file.name]))
        atlas_path = write_text_asset(target_folder, atlas_text_object, ".atlas")
        atlas_paths.append(atlas_path)

    required_pages: list[str] = []
    for atlas_path in atlas_paths:
        for line in atlas_path.read_text(encoding="utf-8-sig").splitlines():
            stripped = line.strip()
            if stripped.lower().endswith((".png", ".jpg", ".jpeg", ".webp")):
                required_pages.append(stripped)

    page_stems = {Path(page).stem for page in required_pages}
    missing_textures = sorted(
        name for name in page_stems if ("Texture2D", name) not in named_object_cache
    )
    if missing_textures:
        raise FileNotFoundError(f"Texture pages are not cached: {missing_textures}")
    texture_objects = {
        name: named_object_cache[("Texture2D", name)] for name in page_stems
    }
    for page in required_pages:
        texture_object = texture_objects[Path(page).stem]
        source_bundles.add(str(cab_to_bundle[texture_object.assets_file.name]))
        texture = texture_object.read()
        texture_path = target_folder / page
        texture.image.save(texture_path)
        if texture_path not in texture_paths:
            texture_paths.append(texture_path)

    missing_pages = [page for page in required_pages if not (target_folder / page).is_file()]
    if missing_pages:
        raise FileNotFoundError(f"Atlas pages were not exported for {target.asset_name}: {missing_pages}")

    animations = sorted(skeleton_json.get("animations", {}).keys())
    return {
        "name": target.output_name,
        "spine_version": skeleton_json.get("skeleton", {}).get("spine"),
        "animation_count": len(animations),
        "animations": animations,
        "files": [
            str(path.relative_to(OUTPUT_ROOT))
            for path in [skeleton_path, *atlas_paths, *texture_paths]
        ],
        "source_bundle": target.bundle_hash,
        "dependency_bundles": sorted(
            Path(path).name for path in source_bundles if Path(path).name != target.bundle_hash
        ),
    }


def main() -> None:
    OUTPUT_ROOT.mkdir(parents=True, exist_ok=True)
    print("Indexing dependencies for all targets...")
    prefetch_dependencies()
    results = []
    skipped = []
    for index, target in enumerate(TARGETS, start=1):
        print(f"[{index}/{len(TARGETS)}] Extracting {target.output_name}...")
        try:
            result = extract_target(target)
        except Exception as exc:
            skipped.append({"name": target.output_name, "reason": str(exc)})
            print(f"  skipped: {exc}")
            continue
        results.append(result)
        print(
            f"  {result['animation_count']} animations, "
            f"Spine {result['spine_version']}, {len(result['files'])} files"
        )
        if len(results) == 10:
            break

    if len(results) < 10:
        raise RuntimeError(f"Only {len(results)} complete Spine sets were found")

    manifest = {
        "source": str(CACHE_ROOT),
        "format": "Spine JSON + atlas + PNG",
        "count": len(results),
        "sets": results,
        "skipped_incomplete_candidates": skipped,
    }
    (OUTPUT_ROOT / "manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )


if __name__ == "__main__":
    main()
