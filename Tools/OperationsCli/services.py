"""运营终端的 HTTP、后端进程及 Unity 构建连接."""

from __future__ import annotations

import hashlib
import http.client
import json
import os
from pathlib import Path
import re
import secrets
import shutil
import socket
import subprocess
import time
from urllib.parse import quote, urlsplit
import uuid
import zipfile


ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / "Temp" / "OperationsCli"
DEVELOPMENT = ROOT / "Library" / "Development"
AUTH_ENV = "ACHEN_BACKEND_AUTH_SIGNING_KEY"
PLATFORMS = {"StandaloneWindows64", "Android", "iOS"}
SEMVER = re.compile(
    r"(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)"
    r"(?:-((?:0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*)"
    r"(?:\.(?:0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*))*))?"
    r"(?:\+([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?"
)
NO_WINDOW = getattr(subprocess, "CREATE_NO_WINDOW", 0)


class OperationError(Exception):
    pass


def validate_url(value: str) -> str:
    try:
        parts = urlsplit(value.strip())
        parts.port
    except ValueError as error:
        raise OperationError("后端地址或端口无效。") from error
    if (parts.scheme not in {"http", "https"} or not parts.hostname
            or parts.username or parts.password or parts.query or parts.fragment):
        raise OperationError("请输入 HTTP/HTTPS 后端地址，不包含账号、查询参数或片段。")
    return value.strip().rstrip("/")


class BackendClient:
    def __init__(self, base_url: str):
        self.base_url = validate_url(base_url)

    def request(self, method: str, path: str, body=None, *,
                allow_missing=False, artifact: Path | None = None, sha256="", timeout=15):
        parts = urlsplit(self.base_url)
        connection_type = (http.client.HTTPSConnection if parts.scheme == "https"
                           else http.client.HTTPConnection)
        connection = connection_type(parts.hostname, parts.port, timeout=600 if artifact else timeout)
        headers = {}
        route = parts.path.rstrip("/") + path
        try:
            if artifact:
                headers.update({"Content-Type": "application/zip",
                                "X-Artifact-Sha256": sha256,
                                "Content-Length": str(artifact.stat().st_size)})
                # 直接流式上传, 大型 Addressables ZIP 不整体读入内存.
                with artifact.open("rb") as stream:
                    connection.request(method, route, body=stream, headers=headers)
                    response = connection.getresponse()
                    raw = response.read()
            else:
                data = None if body is None else json.dumps(body, ensure_ascii=False).encode("utf-8")
                if data is not None:
                    headers["Content-Type"] = "application/json"
                connection.request(method, route, body=data, headers=headers)
                response = connection.getresponse()
                raw = response.read()
            if allow_missing and response.status == 404:
                return None
            try:
                value = json.loads(raw) if raw else {}
            except (ValueError, UnicodeError):
                value = {"detail": raw.decode("utf-8", errors="replace")[:1000]}
            if response.status >= 300:
                problem = value if isinstance(value, dict) else {}
                message = problem.get("detail") or problem.get("title") or response.reason
                code = problem.get("code", "HTTP_ERROR")
                trace = problem.get("traceId") or response.getheader("X-Request-Id", "")
                errors = problem.get("errors")
                detail = f"; 字段={json.dumps(errors, ensure_ascii=False)}" if errors else ""
                raise OperationError(f"{message} [{code}] (HTTP {response.status}; Trace={trace}){detail}")
            return value
        except (OSError, http.client.HTTPException) as error:
            uncertain = " 写操作结果可能未返回，请查询后端状态后再决定是否重试。" if method != "GET" else ""
            raise OperationError(f"后端请求失败：{type(error).__name__}。{uncertain}") from error
        finally:
            connection.close()


class LocalBackend:
    def __init__(self, client: BackendClient):
        self.client = client
        self.process: subprocess.Popen | None = None
        self.auth_key = os.environ.get(AUTH_ENV, "")
        self.log_path = WORK / "backend.log"

    @property
    def owned(self):
        return self.process is not None and self.process.poll() is None

    def _port_open(self):
        parts = urlsplit(self.client.base_url)
        try:
            with socket.create_connection((parts.hostname, parts.port or 80), timeout=1):
                return True
        except OSError:
            return False

    def start(self):
        if self.owned:
            print(f"后端已由本终端启动，PID={self.process.pid}。")
            return
        parts = urlsplit(self.client.base_url)
        if (parts.scheme != "http" or parts.hostname not in {"127.0.0.1", "localhost", "::1"}
                or parts.path):
            raise OperationError("本地启动需要 HTTP 回环地址，例如 http://127.0.0.1:5080。")
        if self._port_open():
            raise OperationError("该端口已有服务；可直接连接使用，本终端不会接管它。")
        dotnet = shutil.which("dotnet")
        if not dotnet:
            raise OperationError("未找到 dotnet，请安装 .NET 8 SDK 并加入 PATH。")
        directory = ROOT / "Backend" / "src" / "AChen.Backend.Api"
        project = directory / "AChen.Backend.Api.csproj"
        if not project.is_file():
            raise OperationError(f"找不到后端项目：{project}。请先准备 Backend 子模块。")
        DEVELOPMENT.mkdir(parents=True, exist_ok=True)
        auth_file = DEVELOPMENT / "auth.key"
        publish_file = DEVELOPMENT / "publish.key"
        self.auth_key = self.auth_key or (auth_file.read_text(encoding="utf-8").strip() if auth_file.exists() else secrets.token_urlsafe(48))
        publish_key = os.environ.get("ACHEN_CONTENT_PUBLISH_KEY") or (publish_file.read_text(encoding="utf-8").strip() if publish_file.exists() else secrets.token_urlsafe(48))
        if len(self.auth_key) < 32:
            raise OperationError("身份签名密钥须至少 32 个字符。")
        if len(publish_key) < 32:
            raise OperationError("发布密钥须至少 32 个字符。")
        auth_file.write_text(self.auth_key, encoding="utf-8")
        publish_file.write_text(publish_key, encoding="utf-8")
        WORK.mkdir(parents=True, exist_ok=True)
        print(f"正在构建后端，日志：{self.log_path}", flush=True)
        with self.log_path.open("w", encoding="utf-8") as log:
            build = subprocess.Popen([dotnet, "build", str(project), "-c", "Debug", "--nologo",
                                      "--disable-build-servers"], cwd=ROOT, stdout=log,
                                     stderr=subprocess.STDOUT, creationflags=NO_WINDOW)
            try:
                if build.wait() != 0:
                    raise OperationError("后端构建失败，请使用“查看日志”。")
            finally:
                if build.poll() is None:
                    build.terminate()
                    try:
                        build.wait(timeout=10)
                    except subprocess.TimeoutExpired:
                        build.kill()
                        build.wait(timeout=5)
        if self._port_open():
            raise OperationError("构建期间端口被占用，未启动后端。")
        environment = os.environ.copy()
        environment.update({"ASPNETCORE_ENVIRONMENT": "Development", "DOTNET_ENVIRONMENT": "Development",
                            "Auth__SigningKey": self.auth_key, "ContentDelivery__PublishKey": publish_key})
        dll = directory / "bin" / "Debug" / "net8.0" / "AChen.Backend.Api.dll"
        with self.log_path.open("a", encoding="utf-8") as log:
            self.process = subprocess.Popen([dotnet, str(dll), "--urls", self.client.base_url],
                                            cwd=directory, env=environment, stdout=log,
                                            stderr=subprocess.STDOUT, creationflags=NO_WINDOW)
        print(f"等待后端就绪，PID={self.process.pid}…", flush=True)
        try:
            deadline = time.monotonic() + 30
            last_error = "端口未监听"
            while time.monotonic() < deadline:
                if not self.owned:
                    raise OperationError("后端进程已退出，请查看日志。")
                if self._port_open():
                    try:
                        self.client.request("GET", "/ready", timeout=2)
                    except OperationError as error:
                        last_error = str(error)
                    else:
                        print(f"后端启动成功：{self.client.base_url}")
                        return
                time.sleep(0.5)
            raise OperationError("后端在 30 秒内未就绪，请查看日志。" + last_error)
        except BaseException:
            self.stop()
            raise

    def stop(self):
        # 只操作本会话持有的子进程句柄, 不根据端口或外部 PID 杀进程.
        if self.owned:
            process = self.process
            process.terminate()
            try:
                process.wait(timeout=8)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait(timeout=5)
            print(f"本终端启动的后端已停止，PID={process.pid}。")
        self.process = None


def build_content(version: str, report=print) -> Path:
    if not SEMVER.fullmatch(version):
        raise OperationError("内容版本须为 SemVer，例如 0.2.1 或 1.0.0-beta.1。")
    configured = os.environ.get("ACHEN_UNITY_CLI")
    executable = configured or shutil.which("unity")
    if not executable:
        candidate = Path(os.environ.get("LOCALAPPDATA", "")) / "Unity" / "bin" / "unity.exe"
        executable = str(candidate) if candidate.is_file() else None
    if not executable:
        raise OperationError("未找到 Unity CLI。请安装或设置 ACHEN_UNITY_CLI。")
    job_id = uuid.uuid4().hex
    state_path = WORK / "content" / job_id / "status.json"
    code = f"return PythonOperationsBridge.BeginBuild({json.dumps(job_id)}, {json.dumps(version)});"
    report(f"提交 Unity 构建；状态文件：{state_path}")
    report("Ctrl+C 可停止终端等待；Unity 已开始的构建会继续，但不会自动上传或发布。")
    command = [executable, "command", "--proxy-disable", "--project-path", str(ROOT),
               "--format", "json", "--non-interactive", "--no-banner", "--no-pager",
               "--timeout", "15", "eval", "--code", code]
    try:
        response = subprocess.run(command, cwd=ROOT, capture_output=True, encoding="utf-8",
                                  errors="replace", timeout=25, creationflags=NO_WINDOW)
        envelope = json.loads(response.stdout) if response.stdout.strip() else {}
        data = envelope.get("data") or {}
        result = data.get("result") or {}
        if not state_path.exists() and (response.returncode or not envelope.get("success")
                                      or not data.get("success") or not result.get("success")):
            detail = json.dumps(envelope, ensure_ascii=False)[:1800] if envelope else response.stderr[-1200:]
            raise OperationError("Unity 未接受构建，请打开项目并启用 Pipeline，检查阻塞对话框。\n" + detail)
    except subprocess.TimeoutExpired:
        # 提交超时也可能已执行; 只读取状态, 不重复提交构建.
        report("提交响应超时，正在读取同一任务的状态；不会重复提交。")
    except (ValueError, OSError) as error:
        if not state_path.exists():
            raise OperationError("无法调用 Unity CLI。请检查可执行路径和 Pipeline 连接。") from error
    deadline = time.monotonic() + 1800
    missing_deadline = time.monotonic() + 20
    previous = None
    while time.monotonic() < deadline:
        if state_path.is_file():
            try:
                state = json.loads(state_path.read_text(encoding="utf-8-sig"))
            except (OSError, ValueError):
                time.sleep(0.5)
                continue
            message = state.get("message")
            if message != previous:
                report(message or state.get("status", "处理中"))
                previous = message
            if state.get("status") == "failed":
                raise OperationError(message or "Unity 内容构建失败。")
            if state.get("status") == "completed":
                return Path(state["zipPath"])
        elif time.monotonic() > missing_deadline:
            raise OperationError(f"尚未收到 Unity 构建状态。请检查编辑器，勿重复提交；状态路径：{state_path}")
        time.sleep(0.5)
    raise OperationError(f"等待构建超过 30 分钟；未上传或发布。请检查：{state_path}")


def package_info(path: Path):
    try:
        with zipfile.ZipFile(path) as archive:
            entry = archive.getinfo("release-manifest.json")
            if entry.file_size > 8 * 1024 * 1024:
                raise OperationError("Release 清单超过 8 MB。")
            manifest = json.loads(archive.read(entry).decode("utf-8-sig"))
    except (OSError, ValueError, KeyError, zipfile.BadZipFile) as error:
        raise OperationError("ZIP 不存在或缺少有效的 release-manifest.json。") from error
    if (not isinstance(manifest, dict) or manifest.get("platform") not in PLATFORMS
            or not isinstance(manifest.get("contentVersion"), str)
            or not SEMVER.fullmatch(manifest["contentVersion"])
            or not isinstance(manifest.get("appVersion"), str) or not manifest["appVersion"].strip()):
        raise OperationError("Release 清单的平台或版本无效。")
    return manifest


def publish_content(client: BackendClient, path: Path, notes: str, report=print):
    manifest = package_info(path)
    active_path = "/api/content/active-releases/development/" + manifest["platform"] + "/" + quote(manifest["appVersion"], safe="")
    # 在上传前取得并发令牌, 不覆盖其他操作者在上传期间发布的版本.
    current = client.request("GET", active_path, allow_missing=True)
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    release = client.request("POST", "/api/content/releases", {
        "platform": manifest["platform"], "appVersion": manifest["appVersion"],
        "contentVersion": manifest["contentVersion"], "notes": notes.strip() or None})
    release_id = release["id"]
    report(f"已创建 Release={release_id}；正在上传 {path.stat().st_size / 1048576:.1f} MB…")
    try:
        client.request("PUT", f"/api/content/releases/{quote(release_id, safe='')}/artifact",
                       artifact=path, sha256=digest.hexdigest())
        client.request("PUT", active_path, {"releaseId": release_id,
                       "expectedCurrentReleaseId": current["releaseId"] if current else None})
    except (OperationError, KeyboardInterrupt):
        report(f"发布流程未完成，Release={release_id}，ZIP={path}。请先查询 Release 状态，避免重复创建。")
        raise
    report(f"内容发布成功：development / {manifest['platform']} / {manifest['contentVersion']}；Release={release_id}")
    return release_id
