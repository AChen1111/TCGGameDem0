"""ADB discovery and device-scoped operations; no shell command interpolation."""
from __future__ import annotations

from dataclasses import dataclass
import os
from pathlib import Path
import re
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[2]
LOG_DIR = ROOT / "Temp" / "AndroidDebug" / "logs"
HIDDEN = subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0


class DebugError(Exception):
    pass


def tcp_port(value: str | int) -> int:
    text = str(value)
    if not text.isascii() or not text.isdecimal() or not 1 <= int(text) <= 65535:
        raise ValueError("端口必须是 1 至 65535 的整数。")
    return int(text)


@dataclass(frozen=True)
class Device:
    serial: str
    state: str
    model: str = ""

    @property
    def label(self) -> str:
        status = {"device": "已连接", "offline": "离线", "unauthorized": "未授权"}.get(self.state, self.state)
        return f"{self.model or 'Android'} | {self.serial} | {status}"


def parse_devices(output: str) -> list[Device]:
    result = []
    for line in output.splitlines():
        if not line.strip() or line.startswith(("List of devices", "*", "adb:")):
            continue
        fields = line.split()
        if len(fields) < 2:
            continue
        if fields[1] not in {"device", "offline", "unauthorized", "recovery", "sideload", "bootloader", "no"}:
            continue
        model = next((x[6:] for x in fields[2:] if x.startswith("model:")), "")
        state = "无权限" if fields[1] == "no" else fields[1]
        result.append(Device(fields[0], state, model.replace("_", " ")))
    return result


@dataclass(frozen=True)
class Mapping:
    direction: str
    source: str
    target: str

    @property
    def label(self) -> str:
        left, right = ("手机", "电脑") if self.direction == "reverse" else ("电脑", "手机")
        return f"{left} {self.source}  ->  {right} {self.target}"


def parse_mappings(output: str, direction: str, serial: str) -> list[Mapping]:
    result = []
    for line in output.splitlines():
        fields = line.split()
        if len(fields) != 3:
            continue
        # forward --list is global; reverse --list has a transport label (e.g. UsbFfs).
        if direction == "forward" and fields[0] != serial:
            continue
        if not fields[1].startswith(("tcp:", "localabstract:", "localreserved:", "localfilesystem:", "jdwp:", "vsock:")):
            continue
        result.append(Mapping(direction, fields[1], fields[2]))
    return result


def _running_adb() -> list[Path]:
    if os.name != "nt":
        return []
    shell = Path(os.environ.get("SystemRoot", "C:/Windows")) / "System32/WindowsPowerShell/v1.0/powershell.exe"
    try:
        output = subprocess.run(
            [str(shell), "-NoProfile", "-NonInteractive", "-Command",
             "$OutputEncoding = [System.Text.UTF8Encoding]::new($false); [Console]::OutputEncoding = $OutputEncoding; "
             "(Get-CimInstance Win32_Process -Filter \"name = 'adb.exe'\").ExecutablePath | Select-Object -Unique"],
            capture_output=True, encoding="utf-8", errors="replace", timeout=5, creationflags=HIDDEN,
        ).stdout
        return [Path(x.strip()) for x in output.splitlines() if x.strip()]
    except (OSError, subprocess.TimeoutExpired):
        return []


def find_adb(explicit: str | None = None) -> Path:
    override = explicit or os.environ.get("ANDROID_ADB")
    if override:
        path = Path(os.path.expandvars(override.strip().strip('"'))).expanduser()
        if path.is_dir():
            path /= "adb.exe" if os.name == "nt" else "adb"
        if path.is_file():
            return path.resolve()
        raise DebugError(f"ADB 路径不存在：{path}")

    candidates = _running_adb()  # Reuse the active Unity/Android Studio ADB version.
    located = shutil.which("adb")
    if located:
        candidates.append(Path(located))
    for key in ("ANDROID_SDK_ROOT", "ANDROID_HOME"):
        if os.environ.get(key):
            candidates.append(Path(os.environ[key]) / "platform-tools" / "adb.exe")
    if os.environ.get("LOCALAPPDATA"):
        candidates.append(Path(os.environ["LOCALAPPDATA"]) / "Android/Sdk/platform-tools/adb.exe")

    version_file = ROOT / "ProjectSettings/ProjectVersion.txt"
    match = re.search(r"m_EditorVersion:\s*(\S+)", version_file.read_text(encoding="utf-8")) if version_file.exists() else None
    version = match.group(1) if match else ""
    unity_roots = [ROOT.anchor and Path(ROOT.anchor) / "UnityEditor",
                   Path(os.environ.get("ProgramFiles", "C:/Program Files")) / "Unity/Hub/Editor"]
    suffix = Path("Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb.exe")
    for install_root in filter(None, unity_roots):
        if version:
            candidates.append(install_root / version / suffix)
        if install_root.is_dir():
            candidates.extend(folder / suffix for folder in sorted(install_root.iterdir(), reverse=True) if folder.is_dir())
    for path in candidates:
        if path.is_file():
            return path.resolve()
    raise DebugError("找不到 adb.exe。安装 Android SDK platform-tools，或使用 --adb 指定路径。")


class Adb:
    def __init__(self, path: Path):
        self.path = path

    def command(self, *args: str, serial: str | None = None) -> list[str]:
        return [str(self.path), *(["-s", serial] if serial else []), *args]

    def run(self, *args: str, serial: str | None = None, check: bool = True, timeout: float = 12) -> str:
        try:
            result = subprocess.run(self.command(*args, serial=serial), capture_output=True,
                                    encoding="utf-8", errors="replace", timeout=timeout, creationflags=HIDDEN)
        except subprocess.TimeoutExpired as error:
            raise DebugError("ADB 操作超时，请检查 USB 连接和手机授权。") from error
        except OSError as error:
            raise DebugError(f"无法运行 ADB：{error}") from error
        if check and result.returncode:
            raise DebugError((result.stderr or result.stdout).strip() or f"ADB 退出码：{result.returncode}")
        return result.stdout

    def devices(self) -> list[Device]:
        return parse_devices(self.run("devices", "-l"))

    def select(self, serial: str | None) -> Device:
        devices = self.devices()
        if serial:
            found = next((x for x in devices if x.serial == serial), None)
            if not found:
                raise DebugError(f"设备 {serial} 未连接。请重新插线或切换设备。")
            if found.state != "device":
                raise DebugError(f"设备不可用：{found.label}。请解锁手机并允许 USB 调试。")
            return found
        ready = [x for x in devices if x.state == "device"]
        if len(ready) == 1:
            return ready[0]
        if not ready:
            raise DebugError("没有已授权的设备。请连接手机，开启 USB 调试，并允许此电脑调试。")
        raise DebugError("连接了多台设备，请在菜单中选择，或使用 --serial 指定序列号。")

    def mappings(self, serial: str, direction: str) -> list[Mapping]:
        return parse_mappings(self.run(direction, "--list", serial=serial), direction, serial)

    def add_mapping(self, serial: str, direction: str, source: int, target: int) -> None:
        if direction not in {"reverse", "forward"}:
            raise ValueError("无效转发方向")
        self.run(direction, f"tcp:{tcp_port(source)}", f"tcp:{tcp_port(target)}", serial=serial)

    def remove_mapping(self, serial: str, mapping: Mapping) -> None:
        # Remove only the chosen mapping; retain Unity's debugger/profiler mappings.
        self.run(mapping.direction, "--remove", mapping.source, serial=serial)


def logcat_args(mode: str, history: int = 200, dump: bool = False) -> list[str]:
    if mode not in {"unity", "errors", "all", "crash"}:
        raise ValueError("无效日志模式")
    if history < 1:
        raise ValueError("历史条数必须大于 0")
    buffers = ["crash"] if mode == "crash" else ["main", "system", "crash"]
    args = ["logcat", *[part for buffer in buffers for part in ("-b", buffer)], "-v", "threadtime"]
    args += ["-d"] if dump else ["-T", str(history)]  # Uppercase T keeps streaming.
    filters = {"unity": ["Unity:V", "AndroidRuntime:E", "libc:F", "DEBUG:F", "CRASH:E", "*:S"],
               "errors": ["*:E"], "all": ["*:V"], "crash": ["*:V"]}
    return args + filters[mode]
