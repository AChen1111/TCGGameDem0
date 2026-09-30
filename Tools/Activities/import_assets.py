"""Copy the selected MDPro3 UI sprites and their original import metadata."""
import csv
import json
import shutil
import sys
from pathlib import Path

source = Path(sys.argv[1])
project = Path(__file__).resolve().parents[2]
destination = project / "Assets/UI/Sprite/Activities"
destination.mkdir(parents=True, exist_ok=True)
selections = {
    "window": "Sprites/CommonUI/GUI_CommonWindowS.png",
    "button": "Sprites/CommonUI/GUI_CommonButtonM.png",
    "button_hover": "Sprites/CommonUI/GUI_CommonButtonM_Over.png",
    "close": "Sprites/CommonUI/GUI_ButtonClose.png",
    "checked": "Sprites/CommonUI/GUI_CommonButtonPlof_Check_On.png",
    "item_base": "Sprites/CommonUI/GUI_CommonItemFrameS_Base.png",
    "item_frame": "Sprites/CommonUI/GUI_CommonItemFrameS_Frame.png",
    "item_hover": "Sprites/CommonUI/GUI_CommonItemFrameS_Over.png",
    "topic_base": "Sprites/Menus/servantui/GUI_Home_TopicsItemFrameS_Base.png",
    "topic_frame": "Sprites/Menus/servantui/GUI_Home_TopicsItemFrameS_Frame.png",
    "topic_hover": "Sprites/Menus/servantui/GUI_Home_TopicsItemFrameS_Over.png",
    "tab": "Sprites/CommonUI/GUI_Home_TopicsTab_Base.png",
    "gold": "Sprites/Builtin/coins.png",
    "gift": "Sprites/Builtin/cta_icon_mystery_chest_gold.png",
    "notice": "Sprites/Menus/popup/GUI_CommonIcon_DialogAlert.png",
    "card": "Sprites/CommonUI/GUI_DeckEdit_Icon_InfoSwitching0.png",
}
manifest = json.loads((source / "ui_manifest.json").read_text(encoding="utf-8"))
entries = {a.get("file", "").replace("\\", "/"): a for a in manifest["assets"] if a["type"] == "Sprite"}
metadata = []
for name, relative in selections.items():
    shutil.copyfile(source / relative, destination / f"activity_{name}.png")
    entry = entries[relative]
    metadata.append({"name": f"activity_{name}", "source": relative, "sha256": entry["sha256"],
                     "pivot": entry.get("pivot", [0.5, 0.5]), "border": entry.get("border", [0, 0, 0, 0]),
                     "pixelsPerUnit": entry.get("pixels_per_unit", 100)})
(destination / "import-metadata.json").write_text(json.dumps(metadata, ensure_ascii=False, indent=2), encoding="utf-8")
with (project / "TableData/activity-resources.csv").open("w", encoding="utf-8", newline="") as stream:
    writer = csv.writer(stream)
    writer.writerows([["ResourceKey"], ["string"]] + [[m["name"]] for m in metadata])
print(f"Imported {len(metadata)} activity UI sprites")
