---
name: quit
description: 快速思考。跳过全部项目级 agent skills（.cursor/skills）。Use when the user types /quit or wants a fast answer without reading project skills.
disable-model-invocation: true
---

# Quit（快速思考）

使用 /quit 就不看项目级的skills。

本轮已触发 `/quit`：

- 不要读取 `.cursor/skills/**`（本文件除外，已加载）
- 不要走 `using-agent-skills` 路由，不要为流程打开 TDD / spec / incremental 等 skill
- 直接回答或改代码
- 仍遵守 always-on 规则（技术栈、编码规范、git；操作 Unity Editor 时用 Pipeline CLI）
- 回复末尾写 `Skills: quit`
