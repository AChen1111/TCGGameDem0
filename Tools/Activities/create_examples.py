"""生成计划书示例草稿，用源配置中的实际卡牌与卡包 ID。"""
import csv
import json
from copy import deepcopy
from pathlib import Path

root = Path(__file__).resolve().parents[2]
with (root / "TableData/all-cards.csv").open(encoding="utf-8-sig", newline="") as file:
    cards = list(csv.reader(file))
with (root / "TableData/card-packs.csv").open(encoding="utf-8-sig", newline="") as file:
    packs = list(csv.reader(file))
card_id = cards[2][0]
pack_id = int(next(row[0] for row in packs[2:] if row[6].lower() == "true"))
gifts = [{"id": f"gift_gold_{amount}_v1", "nameKey": f"gift.gold_{amount}.name", "description": "", "rewards": [
    {"rewardType": 1, "rewardId": "gold", "amount": amount, "cardVariant": 0}]} for amount in [100, 200, 500, 1000]]
gifts.append({"id": "gift_fixed_card_v1", "nameKey": "gift.card.name", "description": "", "rewards": [
    {"rewardType": 2, "rewardId": card_id, "amount": 1, "cardVariant": 0}]})

def activity(id, type, display=0, timed=False, priority=0):
    return {"id": id, "nameKey": f"activity.{id}.name", "type": type, "description": "", "descriptionKey": "",
        "scheduleMode": int(timed), "startsAt": "2026-10-01T00:00:00+08:00" if timed else None,
        "endsAt": "2026-10-08T00:00:00+08:00" if timed else None, "displayMode": display, "sortOrder": 0,
        "showBeforeStart": timed, "showLocked": True, "hideWhenCompleted": False, "bannerResourceKey": "",
        "popup": {"trigger": "lobbyReady", "frequency": "oncePerLogin", "priority": priority, "policyVersion": 2, "stopWhenCompleted": False},
        "openConditions": {"allOf": []}, "entries": []}

def entry(id, gold, **kwargs):
    return {"id": id, "giftId": f"gift_gold_{gold}_v1", "nameKey": f"gift.gold_{gold}.name", "periodKind": 0,
        "limitPerPeriod": 1, "totalLimit": 1, "costGold": 0, "sortOrder": 0, **kwargs}

welcome = activity("national_day_gift_2026", 1, 2, True, 90)
welcome["entries"] = [entry("welcome", 1000)]
daily = activity("daily_gold", 1)
daily["entries"] = [entry("daily", 200, periodKind=1, totalLimit=None)]
login = activity("login_three_days_2026", 2, 2, True, 80)
login.update(requiredDays=3, allowCatchUpClaims=True)
login["entries"] = [entry(f"day_{day}", gold, dayIndex=day, sortOrder=day) for day, gold in enumerate([100, 200, 500], 1)]
exchange = activity("daily_card_exchange", 3)
exchange["entries"] = [entry("exchange_offer_001", 200, giftId="gift_fixed_card_v1", nameKey="exchange.daily_card.name", costGold=800, periodKind=1, totalLimit=None)]
milestone = activity("draw_ten_reward_2026", 4, timed=True)
milestone.update(metricType="gachaDrawCount", packIds=[pack_id])
milestone["entries"] = [entry("draw_10", 500, threshold=10)]
notice = activity("notice_national_day_2026", 0, 1, True, 100)
notice["notice"] = {"content": "国庆活动开放：领取见面礼，累计登录三天获得各档金币奖励。", "imageResourceKey": "",
    "actionKind": "activity", "actionTarget": welcome["id"]}
examples = {"gifts": gifts, "activities": [welcome, daily, login, exchange, milestone, notice]}

# CSV 是唯一可编辑来源。礼品模板只用于一次性构造这六个示例，不单独发布。
source = root / "ActivityTableData"
source.mkdir(exist_ok=True)
def write_table(name, fields, types, rows):
    with (source / (name + ".csv")).open("w", encoding="utf-8", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(fields); writer.writerow(types)
        for row in rows:
            writer.writerow([json.dumps(row[k], ensure_ascii=False, separators=(",", ":")) if isinstance(row[k], list) else "" if row[k] is None else str(row[k]) for k in fields])
master_fields = "ActivityId Type DetailTable IsEnabled ScheduleMode StartsAt EndsAt NameKey Description DescriptionKey SortOrder DisplayMode BannerResourceKey ShowLocked HideWhenCompleted PopupTrigger PopupFrequency PopupPriority PopupPolicyVersion StopWhenCompleted ConditionTypes ConditionValues ConditionActivityIds ConditionTimes".split()
master_types = "string int string bool int string? string? string string string int int string bool bool string string int long bool string[] long[] string[] string[]".split()
master = []
common_fields = "EntryId NameKey Description PeriodKind LimitPerPeriod TotalLimit SortOrder RewardTypes RewardIds Amounts CardVariants".split()
common_types = "string string string int int int? int int[] string[] long[] int[]".split()
for a in examples["activities"]:
    master.append(dict(zip(master_fields, [a["id"], a["type"], a["id"], True, a["scheduleMode"], a["startsAt"], a["endsAt"], a["nameKey"], "", "", a["sortOrder"], a["displayMode"], a["bannerResourceKey"], a["showLocked"], a["hideWhenCompleted"], a["popup"]["trigger"], a["popup"]["frequency"], a["popup"]["priority"], a["popup"]["policyVersion"], a["popup"]["stopWhenCompleted"], [], [], [], []])))
    if a["type"] == 0:
        n=a["notice"]
        write_table(a["id"], ["Content", "ImageResourceKey", "ActionKind", "ActionTarget"], ["string"]*4, [dict(Content=n["content"], ImageResourceKey=n["imageResourceKey"], ActionKind=n["actionKind"], ActionTarget=n["actionTarget"])])
        continue
    extras = {1: ([], []), 2: (["DayIndex", "AllowCatchUpClaims"], ["int", "bool"]), 3: (["CostGold"], ["long"]), 4: (["Threshold", "PackIds"], ["long", "int[]"])}[a["type"]]
    rows=[]
    for e in a["entries"]:
        reward = next(g["rewards"] for g in gifts if g["id"] == e["giftId"])
        row=dict(zip(common_fields, [e["id"], e["nameKey"], "", e["periodKind"], e["limitPerPeriod"], e["totalLimit"], e["sortOrder"], [r["rewardType"] for r in reward], [r["rewardId"] for r in reward], [r["amount"] for r in reward], [r["cardVariant"] for r in reward]]))
        row.update(DayIndex=e.get("dayIndex", 0), AllowCatchUpClaims=a.get("allowCatchUpClaims", True), CostGold=e["costGold"], Threshold=e.get("threshold", 0), PackIds=a.get("packIds", []))
        rows.append(row)
    write_table(a["id"], common_fields+extras[0], common_types+extras[1], rows)
write_table("activities", master_fields, master_types, master)
print("Generated ActivityTableData/activities.csv and six detail CSVs")
