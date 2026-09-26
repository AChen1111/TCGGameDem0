"""TCG 项目运营终端, 通过数字菜单使用现有后台功能."""

from __future__ import annotations

import argparse
import json
import logging
import os
from pathlib import Path
import sys
from urllib.parse import quote
import webbrowser

from services import (BackendClient, LocalBackend, OperationError, ROOT,
                      SEMVER, build_content, package_info, publish_content, validate_url)


LOGGER = logging.getLogger("TCG.Operations")


def text(label: str, default="", *, required=False):
    hint = f" [{default}]" if default else ""
    value = input(f"{label}{hint}：").strip() or default
    if required and not value:
        raise OperationError(f"{label}不能为空。")
    return value


def number(label: str, default: int, minimum=0, maximum=2**63 - 1):
    try:
        value = int(text(label, str(default)))
    except ValueError as error:
        raise OperationError(f"{label}必须是整数。") from error
    if value < minimum or value > maximum:
        raise OperationError(f"{label}须在 {minimum} 至 {maximum} 之间。")
    return value


def choose(title: str, options: list[str]):
    print(f"\n{'─' * 48}\n  {title}\n{'─' * 48}")
    for index, label in enumerate(options, 1):
        print(f"  {index}. {label}")
    print("  0. 返回" if title != "TCG 运营终端" else "  0. 退出（同时停止本终端启动的后端）")
    while True:
        selected = input("\n请选择数字：").strip()
        if selected.isascii() and selected.isdigit() and 0 <= int(selected) <= len(options):
            return int(selected)
        print("请输入菜单中的数字。")


def confirm(summary: str):
    print(f"\n{summary}")
    return input("确认执行？[y/N]：").strip().lower() == "y"


class OperationsConsole:
    def __init__(self, url: str):
        self.client = BackendClient(url)
        self.backend = LocalBackend(self.client)

    def redact(self, value):
        result = str(value)
        if self.backend.auth_key:
            result = result.replace(self.backend.auth_key, "[已隐藏]")
        return result

    def report(self, message):
        print(self.redact(message), flush=True)

    def configure(self):
        if self.backend.owned:
            raise OperationError("请先停止本终端启动的后端，再切换连接配置。")
        url = validate_url(text("后端地址", self.client.base_url))
        self.client.base_url = url
        self.report(f"连接已设置：{url}。")

    def service_menu(self):
        action = choose("后端服务", ["查看状态", "构建并启动本地后端", "停止本终端启动的后端",
                                   "查看最近日志", "打开账号注册页"])
        if action == 1:
            owner = f"本终端进程 PID={self.backend.process.pid}" if self.backend.owned else "非本终端启动或未运行"
            self.report(f"地址：{self.client.base_url}\n归属：{owner}")
            self.report("健康检查：" + json.dumps(self.client.request("GET", "/health"), ensure_ascii=False))
            self.report("就绪检查：" + json.dumps(self.client.request("GET", "/ready"), ensure_ascii=False))
        elif action == 2:
            self.backend.start()
        elif action == 3:
            if self.backend.owned:
                self.backend.stop()
            else:
                self.report("本终端没有持有运行中的后端进程。")
        elif action == 4:
            if self.backend.log_path.is_file():
                with self.backend.log_path.open("rb") as stream:
                    stream.seek(max(0, self.backend.log_path.stat().st_size - 64000))
                    lines = stream.read().decode("utf-8", errors="replace").splitlines()
                self.report("\n".join(lines[-80:]))
            else:
                self.report("本终端尚无后端日志。")
        elif action == 5:
            webbrowser.open(self.client.base_url + "/register")

    def gold_menu(self):
        action = choose("账号金币", ["查询账号金币", "给账号添加金币"])
        if not action:
            return
        username = text("账号", required=True)
        if action == 1:
            result = self.client.request("GET", "/api/accounts/admin/gold?username=" + quote(username, safe=""))
            self.report(f"{result['username']} 当前金币：{result['gold']:,}；Revision={result['revision']}")
            return
        amount = number("添加金币数量", 100000, minimum=1)
        if not confirm(f"后端：{self.client.base_url}\n账号：{username}\n直接添加金币：{amount:,}"):
            return
        result = self.client.request("POST", "/api/accounts/admin/gold", {"username": username, "amount": amount})
        LOGGER.info("添加金币成功. Account=%s; Added=%s; Gold=%s; Revision=%s",
                    self.redact(result["username"]), result["addedAmount"], result["gold"], result["revision"])
        self.report(f"{result['previousGold']:,} + {result['addedAmount']:,} = {result['gold']:,}")

    def gift_menu(self):
        print("\n账号礼品 · 发到收件箱，玩家领取后入账。")
        username = text("账号", required=True)
        gold = number("礼品金币", 0)
        card_id = text("卡牌 ID（留空不发卡牌）")
        cards = []
        if card_id:
            count = number("卡牌数量", 1, minimum=1, maximum=2**31 - 1)
            rarity = number("卡牌稀有度（0 普通，1 炫彩）", 0, maximum=2**31 - 1)
            cards.append({"cardId": card_id, "count": count, "rarity": rarity})
        if not gold and not cards:
            raise OperationError("金币和卡牌至少提供一种。")
        titles = ["自动选择", "金币礼包", "卡牌礼包", "混合礼包", "输入本地化 Key"]
        title = choose("礼品标题", titles)
        if not title:
            return
        title_keys = ["", "ui.gifts.pack_gold", "ui.gifts.pack_card", "ui.gifts.pack_mixed"]
        title_key = title_keys[title - 1] if title <= 4 else text("本地化 Key", required=True)
        card_text = f"{card_id} × {cards[0]['count']}，稀有度 {cards[0]['rarity']}" if cards else "无"
        summary = (f"后端：{self.client.base_url}\n收件账号：{username}\n金币：{gold:,}\n"
                   f"卡牌：{card_text}\n标题：{title_key or '自动'}")
        if not confirm(summary):
            return
        result = self.client.request("POST", "/api/accounts/admin/gifts", {
            "username": username, "gold": gold, "cards": cards, "titleKey": title_key})
        LOGGER.info("发放礼品成功. Account=%s; GiftId=%s; Target=%s",
                    self.redact(username), result["giftId"], result["targetPlayerId"])

    def content_menu(self):
        action = choose("内容发布 · development", ["通过 Unity 构建并发布", "发布已有 Release ZIP", "查询 Release 状态"])
        if not action:
            return
        if action == 3:
            release_id = text("Release ID（留空查看最近 10 条）")
            route = ("/api/content/releases/" + quote(release_id, safe="") if release_id
                     else "/api/content/releases?page=1&pageSize=10")
            self.report(json.dumps(self.client.request("GET", route), ensure_ascii=False, indent=2))
            return
        if action == 1:
            version = text("内容版本（SemVer）", required=True)
            if not SEMVER.fullmatch(version):
                raise OperationError("版本须为 SemVer，例如 0.2.1。")
            notes = text("发布备注")
            summary = (f"后端：{self.client.base_url}\n内容版本：{version}\n"
                       "使用 Unity 当前平台与 App 版本，生成配置、构建 Addressables、编译 HybridCLR DLL，"
                       "上传并切换 development 活动版本。")
            if not confirm(summary):
                return
            self.client.request("GET", "/api/content/releases?page=1&pageSize=1")
            path = build_content(version, self.report)
        else:
            path = Path(text("Release ZIP 路径", required=True).strip('"')).expanduser().resolve()
            manifest = package_info(path)
            notes = text("发布备注")
            if not confirm(f"后端：{self.client.base_url}\nZIP：{path}\n平台：{manifest['platform']}\n"
                           f"App：{manifest['appVersion']}\n内容：{manifest['contentVersion']}\n上传并激活到 development。"):
                return
        publish_content(self.client, path, notes, self.report)

    def run(self):
        self.report(f"项目：{ROOT}\n使用数字选择、回车确认；Ctrl+C 取消当前操作。")
        try:
            while True:
                try:
                    self.report(f"\n后端：{self.client.base_url}")
                    action = choose("TCG 运营终端", ["后端服务", "账号礼品", "账号金币", "连接设置"])
                    if not action:
                        return
                    (self.service_menu, self.gift_menu, self.gold_menu, self.configure)[action - 1]()
                except KeyboardInterrupt:
                    self.report("\n已取消当前等待或输入。若写请求已提交，请先查询结果，避免重复发放或发布。")
                except (OperationError, OSError, ValueError, KeyError) as error:
                    LOGGER.error("操作失败. Result=Failed; Error=%s", self.redact(error))
        except EOFError:
            self.report("\n输入已关闭，退出终端。")
        finally:
            self.backend.stop()


def main():
    parser = argparse.ArgumentParser(description="TCG 后端服务、账号礼品和金币交互终端; 内容开发请使用 Unity 开发工作台")
    parser.add_argument("--backend", default=os.environ.get("ACHEN_BACKEND_URL", "http://127.0.0.1:5080"),
                        help="后端基础地址，默认 http://127.0.0.1:5080")
    args = parser.parse_args()
    if sys.version_info < (3, 10):
        parser.error("需要 Python 3.10 或更新版本。")
    if not sys.stdin.isatty():
        parser.error("请在交互终端中启动，或双击项目根目录的运营工具.cmd。")
    for output in (sys.stdout, sys.stderr):
        if hasattr(output, "reconfigure"):
            output.reconfigure(encoding="utf-8", errors="replace")
    logging.basicConfig(level=logging.INFO, format="[%(asctime)s] %(message)s", datefmt="%H:%M:%S")
    try:
        OperationsConsole(args.backend).run()
    except (OperationError, OSError) as error:
        LOGGER.error("终端无法继续：%s", error)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
