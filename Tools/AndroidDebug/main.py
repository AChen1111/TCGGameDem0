"""Windows CMD Android debug console. Python 3.10+, standard library only."""
from __future__ import annotations

import argparse
from datetime import datetime
from pathlib import Path
import queue
import re
import subprocess
import sys
import threading
import time

from adb_client import Adb, DebugError, Device, HIDDEN, LOG_DIR, Mapping, find_adb, logcat_args, tcp_port


def log_path(serial: str, mode: str) -> Path:
    safe_serial = re.sub(r"[^A-Za-z0-9_.-]", "_", serial)
    return LOG_DIR / f"{datetime.now():%Y%m%d-%H%M%S-%f}_{safe_serial}_{mode}.log"


def stop_process(process: subprocess.Popen) -> None:
    if process.poll() is None:
        process.terminate()
        try:
            process.wait(timeout=3)
        except subprocess.TimeoutExpired:
            process.kill()
            process.wait(timeout=3)


def stream_once(adb: Adb, device: Device, mode: str, path: Path, history: int, deadline: float | None) -> int:
    """Save every line; bound display backlog so high-volume logs cannot exhaust RAM."""
    messages: queue.Queue[str] = queue.Queue(maxsize=1024)
    finished = threading.Event()
    errors: list[Exception] = []
    skipped = [0]
    process = subprocess.Popen(adb.command(*logcat_args(mode, history), serial=device.serial),
                               stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                               encoding="utf-8", errors="replace", creationflags=HIDDEN)

    def read() -> None:
        try:
            with path.open("a", encoding="utf-8", buffering=1) as saved:
                saved.write(f"\n# Capture {datetime.now().isoformat()} | {device.label} | {mode}\n")
                assert process.stdout is not None
                for line in process.stdout:
                    saved.write(line)
                    try:
                        messages.put_nowait(line)
                    except queue.Full:
                        skipped[0] += 1
        except Exception as error:
            errors.append(error)
        finally:
            finished.set()

    reader = threading.Thread(target=read, daemon=True)
    reader.start()
    try:
        while not finished.is_set() or not messages.empty():
            if deadline is not None and time.monotonic() >= deadline:
                return 0
            try:
                print(messages.get(timeout=0.15), end="", flush=True)
            except queue.Empty:
                pass
        if errors:
            raise DebugError(f"保存日志失败：{errors[0]}")
        return process.wait(timeout=3)
    finally:
        stop_process(process)
        reader.join(timeout=3)
        if process.stdout is not None and not reader.is_alive():
            process.stdout.close()
        if skipped[0]:
            print(f"\n屏幕跳过了 {skipped[0]} 行刷屏内容，文件中仍保留全部日志。")


def watch_logs(adb: Adb, device: Device, mode: str, *, output: Path | None = None,
               history: int = 200, duration: float | None = None) -> Path:
    path = output or log_path(device.serial, mode)
    path.parent.mkdir(parents=True, exist_ok=True)
    # Verify the file is writable before starting the child process.
    with path.open("a", encoding="utf-8"):
        pass
    deadline = time.monotonic() + duration if duration is not None else None
    print(f"\n设备：{device.label}\n实时模式：{mode} | 历史最多 {history} 条\n日志保存：{path.resolve()}\nCtrl+C 停止查看并返回菜单。\n", flush=True)
    try:
        while deadline is None or time.monotonic() < deadline:
            status = stream_once(adb, device, mode, path, history, deadline)
            if status == 0:
                break
            try:
                online = adb.run("get-state", serial=device.serial, check=False, timeout=3).strip() == "device"
            except DebugError:
                online = False
            if online:
                raise DebugError("logcat 异常退出。详细原因已写入日志文件，请检查上方 ADB 提示。")
            print("\n设备已断开，等待同一台手机重新连接…（Ctrl+C 返回）", flush=True)
            while deadline is None or time.monotonic() < deadline:
                try:
                    ready = adb.run("get-state", serial=device.serial, check=False, timeout=3).strip() == "device"
                except DebugError:
                    ready = False
                if ready:
                    print("设备恢复连接，继续记录。", flush=True)
                    break
                time.sleep(1)
    except KeyboardInterrupt:
        print("\n已停止实时日志。")
    finally:
        print(f"日志文件：{path.resolve()}", flush=True)
    return path


def dump_crash(adb: Adb, device: Device, output: Path | None = None) -> Path:
    path = output or log_path(device.serial, "crash")
    data = adb.run(*logcat_args("crash", dump=True), serial=device.serial, timeout=30)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(data, encoding="utf-8")
    print(data if data.strip() else "手机当前没有崩溃缓冲区记录。")
    print(f"崩溃日志已保存：{path.resolve()}")
    return path


def show_devices(adb: Adb, selected: str | None = None) -> list[Device]:
    devices = adb.devices()
    print("\n连接到的设备：")
    if not devices:
        print("  未发现设备。请连接 USB、开启 USB 调试并在手机上允许授权。")
    for index, device in enumerate(devices, 1):
        print(f"  {index}. {'[当前] ' if device.serial == selected else ''}{device.label}")
    if any(x.state == "unauthorized" for x in devices):
        print("  未授权：解锁手机，勾选‘始终允许此电脑’并确认 USB 调试。")
    if any(x.state == "offline" for x in devices):
        print("  离线：检查数据线、重新插拔或重新开启手机 USB 调试。")
    return devices


def choose_device(adb: Adb, selected: str | None) -> str | None:
    devices = show_devices(adb, selected)
    if not devices:
        return selected
    value = input("输入设备序号切换（回车保持当前，0 返回）：").strip()
    if not value or value == "0":
        return selected
    try:
        index = int(value) - 1
    except ValueError as error:
        raise DebugError("请输入设备列表中的序号。") from error
    if not 0 <= index < len(devices):
        raise DebugError("设备序号超出范围。")
    device = adb.select(devices[index].serial)
    print(f"已选择：{device.label}")
    return device.serial


def show_mappings(adb: Adb, serial: str) -> list[Mapping]:
    mappings = adb.mappings(serial, "reverse") + adb.mappings(serial, "forward")
    print("\n当前设备端口转发：")
    for index, item in enumerate(mappings, 1):
        print(f"  {index}. {item.label}")
    if not mappings:
        print("  尚未配置转发。")
    return mappings


def add_mapping(adb: Adb, serial: str, direction: str, source: int, target: int) -> None:
    adb.add_mapping(serial, direction, source, target)
    print(f"\n转发成功：{Mapping(direction, f'tcp:{source}', f'tcp:{target}').label}")
    if direction == "reverse":
        print(f"手机可通过 http://127.0.0.1:{source} 访问电脑的 {target} 端口（电脑服务需已启动）。")
    show_mappings(adb, serial)


def ask_port(label: str, default: int) -> int:
    try:
        return tcp_port(input(f"{label} [{default}]：").strip() or str(default))
    except ValueError as error:
        raise DebugError(str(error)) from error


def menu(adb: Adb, selected: str | None) -> None:
    print(f"ADB：{adb.path}")
    devices = show_devices(adb, selected)
    ready = [x for x in devices if x.state == "device"]
    if not selected and len(ready) == 1:
        selected = ready[0].serial
    while True:
        print(f"\n{'=' * 58}\n  安卓调试终端    当前设备：{selected or '未选择'}\n{'=' * 58}")
        print("  1. 查看 / 刷新 / 切换设备\n  2. 实时 Unity 日志 + 崩溃信息\n  3. 实时错误日志\n  4. 实时全部日志\n  5. 查看并导出已有崩溃日志\n  6. 一键转发 5080（手机 -> 电脑）\n  7. 自定义端口转发\n  8. 查看 / 删除某一条端口转发\n  0. 退出工具")
        try:
            action = input("\n输入编号：").strip()
            if action == "0":
                return
            if action == "1":
                selected = choose_device(adb, selected)
                continue
            if action not in {"2", "3", "4", "5", "6", "7", "8"}:
                print("请输入菜单中的编号。")
                continue
            if not selected:
                try:
                    selected = adb.select(None).serial
                except DebugError:
                    selected = choose_device(adb, selected)
                    if not selected:
                        continue
            device = adb.select(selected)
            if action in {"2", "3", "4"}:
                watch_logs(adb, device, {"2": "unity", "3": "errors", "4": "all"}[action])
            elif action == "5":
                dump_crash(adb, device)
            elif action == "6":
                add_mapping(adb, selected, "reverse", 5080, 5080)
            elif action == "7":
                print("1. 手机访问电脑（reverse）\n2. 电脑访问手机（forward）\n0. 返回")
                direction_choice = input("方向 [1]：").strip() or "1"
                if direction_choice == "0":
                    continue
                if direction_choice not in {"1", "2"}:
                    raise DebugError("请输入 1 或 2。")
                direction = "reverse" if direction_choice == "1" else "forward"
                source_label, target_label = ("手机端口", "电脑端口") if direction == "reverse" else ("电脑端口", "手机端口")
                source = ask_port(source_label, 5080)
                target = ask_port(target_label, source)
                add_mapping(adb, selected, direction, source, target)
            elif action == "8":
                mappings = show_mappings(adb, selected)
                if mappings:
                    value = input("输入要删除的条目编号（回车 / 0 返回）：").strip()
                    if value and value != "0":
                        if not value.isascii() or not value.isdecimal() or not 1 <= int(value) <= len(mappings):
                            raise DebugError("转发条目编号超出范围。")
                        item = mappings[int(value) - 1]
                        adb.remove_mapping(selected, item)
                        print(f"已删除：{item.label}")
        except KeyboardInterrupt:
            print("\n操作已取消，返回菜单。")
        except (DebugError, OSError) as error:
            print(f"\n操作失败：{error}")


def port_argument(value: str) -> int:
    try:
        return tcp_port(value)
    except ValueError as error:
        raise argparse.ArgumentTypeError(str(error)) from error


def positive_duration(value: str) -> float:
    try:
        number = float(value)
        if not 0 < number < float("inf"):
            raise ValueError
        return number
    except ValueError as error:
        raise argparse.ArgumentTypeError("持续时间必须是大于 0 的秒数。") from error


def parser() -> argparse.ArgumentParser:
    result = argparse.ArgumentParser(description="安卓调试终端：设备、实时日志、端口转发。无子命令时进入菜单。")
    result.add_argument("--adb", help="adb.exe 绝对路径（或设置 ANDROID_ADB）")
    result.add_argument("--serial", "-s", help="设备序列号；多设备时必须指定")
    commands = result.add_subparsers(dest="action")
    commands.add_parser("devices", help="列出所有连接设备")
    logs = commands.add_parser("logs", help="实时查看并保存日志；Ctrl+C 停止")
    logs.add_argument("--mode", choices=["unity", "errors", "all", "crash"], default="unity")
    logs.add_argument("--history", type=int, default=200, help="先显示最近 N 条历史，再持续监听")
    logs.add_argument("--duration", type=positive_duration, help="记录指定秒数后停止")
    logs.add_argument("--output", type=Path, help="指定日志文件（追加写入）")
    commands.add_parser("crash", help="导出崩溃缓冲区")
    commands.add_parser("mappings", help="查看当前设备的所有转发")
    for direction in ("reverse", "forward"):
        child = commands.add_parser(direction, help="手机 -> 电脑" if direction == "reverse" else "电脑 -> 手机")
        child.add_argument("source", type=port_argument, nargs="?", default=5080)
        child.add_argument("target", type=port_argument, nargs="?", help="目标端口，默认与源端口相同")
    remove = commands.add_parser("remove", help="删除指定端口的一条转发")
    remove.add_argument("direction", choices=["reverse", "forward"])
    remove.add_argument("port", type=port_argument)
    return result


def main(argv: list[str] | None = None) -> int:
    for output in (sys.stdout, sys.stderr):
        if hasattr(output, "reconfigure"):
            output.reconfigure(encoding="utf-8", errors="replace")
    args = parser().parse_args(argv)
    if args.action == "logs" and args.history < 1:
        parser().error("--history 必须大于 0")
    try:
        try:
            path = find_adb(args.adb)
        except DebugError:
            if args.action or args.adb or not sys.stdin.isatty():
                raise
            print("未自动找到 ADB。可从 Android SDK 或 Unity Editor 的 SDK/platform-tools 中找到 adb.exe。")
            path = find_adb(input("请粘贴 adb.exe 完整路径："))
        adb = Adb(path)
        if not args.action:
            menu(adb, args.serial)
        elif args.action == "devices":
            show_devices(adb, args.serial)
        else:
            device = adb.select(args.serial)
            if args.action == "logs":
                watch_logs(adb, device, args.mode, output=args.output, history=args.history, duration=args.duration)
            elif args.action == "crash":
                dump_crash(adb, device)
            elif args.action == "mappings":
                show_mappings(adb, device.serial)
            elif args.action in {"reverse", "forward"}:
                add_mapping(adb, device.serial, args.action, args.source, args.target or args.source)
            elif args.action == "remove":
                item = Mapping(args.direction, f"tcp:{args.port}", "")
                adb.remove_mapping(device.serial, item)
                print(f"已删除 {args.direction} tcp:{args.port}。")
        return 0
    except (KeyboardInterrupt, EOFError):
        print("\n已退出调试工具，已配置的转发保持有效。")
        return 0
    except (DebugError, OSError) as error:
        print(f"错误：{error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
