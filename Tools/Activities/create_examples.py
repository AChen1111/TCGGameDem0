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
        "popup": {"trigger": "lobbyReady", "frequency": "oncePerActivity", "priority": priority, "policyVersion": 1, "stopWhenCompleted": True},
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
text = json.dumps(examples, ensure_ascii=False, indent=2) + "\n"
(root / "Tools/Activities/examples.json").write_text(text, encoding="utf-8")
public = root / "Backend/AdminWeb/public"
public.mkdir(parents=True, exist_ok=True)
(public / "activity-examples.json").write_text(text, encoding="utf-8")
print("Generated six activity drafts with published source identifiers")
