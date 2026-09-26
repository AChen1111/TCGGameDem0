"""运营终端的 HTTP 与后端进程管理."""

from __future__ import annotations

import http.client
import json
import os
from pathlib import Path
import secrets
import shutil
import socket
import subprocess
import time
from urllib.parse import urlsplit


ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / "Temp" / "OperationsCli"
DEVELOPMENT = ROOT / "Library" / "Development"
AUTH_ENV = "ACHEN_BACKEND_AUTH_SIGNING_KEY"
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
                allow_missing=False, timeout=15):
        parts = urlsplit(self.base_url)
        connection_type = (http.client.HTTPSConnection if parts.scheme == "https"
                           else http.client.HTTPConnection)
        connection = connection_type(parts.hostname, parts.port, timeout=timeout)
        headers = {}
        route = parts.path.rstrip("/") + path
        try:
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
