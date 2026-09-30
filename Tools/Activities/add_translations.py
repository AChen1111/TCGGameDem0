"""向活动系统的源文案表追加中英文 key；二进制由 Unity PublishedConfigBuilder 生成。"""
import csv
from pathlib import Path

rows = [
    ("ui.lobby.activities", "活动", "Activities"), ("ui.lobby.mail", "邮件", "Mail"),
    ("ui.activities.title", "活动", "Activities"), ("ui.activities.refresh", "刷新", "Refresh"),
    ("ui.activities.loading", "正在同步活动…", "Loading activities…"),
    ("ui.activities.empty", "暂无活动", "No activities available"),
    ("ui.activities.sync_failed", "活动同步失败，请点击刷新", "Sync failed. Select Refresh to reconnect."),
    ("ui.activities.claim_failed", "领取失败，请刷新后重试", "Unable to claim. Refresh and try again."),
    ("ui.activities.view", "查看详情", "View details"), ("ui.activities.claimable", "有奖励可领取", "Rewards available"),
    ("ui.activities.completed", "活动已完成", "Completed"), ("ui.activities.running", "活动进行中", "Available now"),
    ("ui.activities.upcoming", "活动尚未开始", "Coming soon"), ("ui.activities.ended", "活动已结束", "Activity ended"),
    ("ui.activities.locked", "尚未满足条件", "Locked"), ("ui.activities.permanent", "长期开放", "Always available"),
    ("ui.activities.starts_in", "距离开始：{time}", "Starts in: {time}"),
    ("ui.activities.ends_in", "距离结束：{time}", "Ends in: {time}"),
    ("ui.activities.next_reset", "下次重置：{time}（北京时间）", "Next reset: {time} (UTC+8)"),
    ("ui.activities.reward", "活动奖励", "Activity rewards"), ("ui.activities.claim", "领取", "Claim"),
    ("ui.activities.claimed", "已领取", "Claimed"), ("ui.activities.claimed_today", "今日已领取", "Claimed today"),
    ("ui.activities.retry", "重试上次操作", "Retry previous request"),
    ("ui.activities.exchange", "兑换", "Exchange"), ("ui.activities.cost", "金币成本：{amount}", "Cost: {amount} gold"),
    ("ui.activities.exchange_confirm", "花费 {amount} 金币兑换这份奖励？", "Spend {amount} gold on this reward?"),
    ("ui.activities.goto", "前往活动", "Go to activity"),
    ("ui.activities.sign_in.day", "累计第 {day} 天", "Login day {day}"),
    ("ui.activities.sign_in.progress", "累计登录 {count} / {days} 天", "Login days: {count} / {days}"),
    ("ui.activities.draw.progress", "累计成功抽卡 {count} 次", "Successful draws: {count}"),
    ("ui.activities.milestone", "累计抽卡 {count} 次", "Reach {count} draws"),
    ("ui.activities.reward_gold", "{amount} 金币", "{amount} gold"),
    ("ui.activities.reward_card", "卡牌 {id} · 版本 {variant} × {amount}", "Card {id} · Variant {variant} × {amount}"),
    ("ui.activities.reward_summary", "{rewards}", "{rewards}"),
    ("ui.activities.condition.ACTIVITY_CONDITION_NOT_MET", "尚未满足活动参与条件", "Activity requirements not met"),
    ("ui.activities.condition.ownedCardKindsAtLeast", "需要持有至少 {count} 种卡牌", "Own at least {count} different cards"),
    ("ui.activities.condition.activityCompleted", "需要先完成活动：{activity}", "Complete activity: {activity}"),
    ("ui.activities.condition.playerCreatedBefore", "限 {time} 前注册的玩家", "Registered before {time}"),
    ("ui.activities.condition.playerCreatedAfter", "限 {time} 起注册的玩家", "Registered from {time}"),
    ("activity.notice_national_day_2026.name", "国庆活动公告", "National Day announcement"),
    ("activity.national_day_gift_2026.name", "国庆见面礼", "National Day welcome gift"),
    ("activity.daily_gold.name", "每日免费金币", "Daily free gold"),
    ("activity.login_three_days_2026.name", "累计三日登录", "Three login days"),
    ("activity.daily_card_exchange.name", "每日金币兑换", "Daily card exchange"),
    ("activity.draw_ten_reward_2026.name", "抽卡十次奖励", "Ten draws reward"),
    ("gift.gold_200.name", "每日金币礼包", "Daily gold gift"),
    ("gift.gold_100.name", "100 金币礼包", "100 gold gift"),
    ("gift.gold_500.name", "500 金币礼包", "500 gold gift"),
    ("gift.gold_1000.name", "1000 金币礼包", "1000 gold gift"),
    ("gift.card.name", "卡牌礼包", "Card gift"),
    ("exchange.daily_card.name", "每日卡牌兑换", "Daily card exchange"),
    ("err.activity_not_started", "活动尚未开始，请刷新活动页面", "This activity has not started. Refresh the page."),
    ("err.activity_ended", "活动已结束，请刷新活动页面", "This activity has ended. Refresh the page."),
    ("err.activity_disabled", "活动已下架，请刷新活动页面", "This activity is unavailable. Refresh the page."),
    ("err.activity_condition_not_met", "尚未满足活动参与条件", "Activity requirements not met."),
    ("err.limit_reached", "已达到本项奖励的领取上限", "The claim limit for this reward has been reached."),
    ("err.activity_already_claimed", "这份奖励已领取", "This reward has already been claimed."),
    ("err.activity_version_changed", "活动已更新，请刷新后重新确认", "This activity changed. Refresh and confirm again."),
    ("err.period_changed", "活动周期已变化，请刷新后重试", "The activity period changed. Refresh and try again."),
    ("err.request_id_conflict", "本次请求与原操作不一致，请刷新后重新操作", "This request conflicts with an earlier operation. Refresh and try again."),
    ("err.activity_schema_changed", "活动协议已更新，请重新启动游戏", "Activity data has changed. Restart the game."),
]
path = Path(__file__).resolve().parents[2] / "TableData/Translations.csv"
with path.open(encoding="utf-8-sig", newline="") as file:
    existing = {row[0] for row in csv.reader(file) if row}
new = [row for row in rows if row[0] not in existing]
with path.open("a", encoding="utf-8", newline="") as file:
    csv.writer(file, lineterminator="\n").writerows(new)
print(f"Added {len(new)} activity translation keys")
