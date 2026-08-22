#!/usr/bin/env python3
import http.server
import os
import socketserver

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", "ServerData"))
os.makedirs(ROOT, exist_ok=True)
os.chdir(ROOT)
print("CDN", ROOT, "http://127.0.0.1:8000")
with socketserver.ThreadingTCPServer(("127.0.0.1", 8000), http.server.SimpleHTTPRequestHandler) as httpd:
    httpd.serve_forever()
