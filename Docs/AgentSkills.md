# Agent Skills 与审计逻辑

本仓库用 `.cursor/skills/` + `.cursor/rules/` + `.cursor/commands/` 约束 Agent：先路由 skill、按流程做、用审计门槛验收，而不是凭感觉改代码。

相关文档：[Lua 系统](LuaSystem.md) · [日志系统](LogSystem.md)

---

## 1. 目录地图

| 路径 | 作用 |
|------|------|
| `.cursor/skills/*/SKILL.md` | 分阶段工作流（定义→实现→验证→评审→发布） |
| `.cursor/skills/using-agent-skills/SKILL.md` | **总路由**：任务来了先读这个 |
| `.cursor/rules/*.mdc` | 始终生效的硬约束（技术栈、编码规范、强制走 skills） |
| `.cursor/commands/*.md` | 斜杠命令（`/review` `/ship` `/test` 等）绑定的审计入口 |
| `.cursor/references/` | 共享清单（DoD、安全、测试、可观测性等） |
| `.cursor/rules/persona-*.mdc` | `/ship` `/review` 等用的评审人格 |

---

## 2. 强制路由（开干前）

非琐碎技术工作必须：

1. 读 `using-agent-skills`，按阶段选 skill  
2. 打开对应 `.cursor/skills/<name>/SKILL.md` 并按步骤执行  
3. skill 若链接 `reference.md` 或 `../../references/`，一并打开  
4. **优先项目 skills，禁止凭猜测跳过流程**  
5. 回复末尾声明：`Skills: skill-a, skill-b, ...`（没用则 `Skills: none`）

Always-on 规则：

- `agent-skills.mdc` — 强制上述路由  
- `project-coding-standards.mdc` — 精简代码、新代码 C#、不确定用 AskQuestion、列 Skills  
- `project-stack.mdc` — Unity/C#/Pipeline 边界与 `--proxy-disable`

---

## 3. 完整功能生命周期（审计主链）

```
interview-me / idea-refine
        ↓
spec-driven-development          需求与验收标准
        ↓
planning-and-task-breakdown      可验证任务切片
        ↓
context-engineering              加载正确上下文
        ↓
source-driven-development        对照官方文档（需要时）
        ↓
incremental-implementation       薄垂直切片实现
   (+ observability 并行)
        ↓
doubt-driven-development         高风险决策对抗复查
        ↓
test-driven-development          先红后绿
        ↓
code-review-and-quality          五轴评审
        ↓
code-simplification              行为不变下减复杂度
        ↓
git-workflow-and-versioning      干净提交
        ↓
documentation-and-adrs           记 why
        ↓
shipping-and-launch              上线门禁
```

不是每个任务都要走全链。例如修 bug 常见路径：

`debugging-and-error-recovery` → `test-driven-development` → `code-review-and-quality`

本仓库 Unity 改动额外挂：`unity-pipeline`（编译/测试/截图必须 `--proxy-disable`）。

---

## 4. 阶段 → Skill 速查

| 阶段 | Skill | 一句话 |
|------|-------|--------|
| 定义 | `interview-me` | 问清真正要什么 |
| 定义 | `idea-refine` | 发散收敛想法 |
| 定义 | `spec-driven-development` | 先写需求与验收 |
| 计划 | `planning-and-task-breakdown` | 拆成可验证块 |
| 构建 | `incremental-implementation` | 切片实现并验证 |
| 构建 | `source-driven-development` | 对照权威文档 |
| 构建 | `doubt-driven-development` | 对抗式复查决策 |
| 构建 | `context-engineering` | 控制上下文质量 |
| 构建 | `frontend-ui-engineering` | 产品级 UI |
| 构建 | `api-and-interface-design` | 稳定接口契约 |
| 构建 | `unity-pipeline` | Unity CLI / Pipeline |
| 构建 | `project-coding-standards` | 本仓库编码铁律 |
| 验证 | `test-driven-development` | 失败测试先行 |
| 验证 | `browser-testing-with-devtools` | 浏览器运行时验证 |
| 验证 | `debugging-and-error-recovery` | 复现→定位→修复→防回归 |
| 评审 | `code-review-and-quality` | 五轴评审 |
| 评审 | `code-simplification` | 减复杂度不改行为 |
| 评审 | `security-and-hardening` | 安全加固 |
| 评审 | `performance-optimization` | 先测量再优化 |
| 发布 | `git-workflow-and-versioning` | 分支与原子提交 |
| 发布 | `ci-cd-and-automation` | 自动化门禁 |
| 发布 | `deprecation-and-migration` | 弃用与迁移 |
| 发布 | `documentation-and-adrs` | 文档与 ADR |
| 发布 | `observability-and-instrumentation` | 日志/指标/追踪 |
| 发布 | `shipping-and-launch` | 上线清单与回滚 |

---

## 5. 审计门槛：Definition of Done

权威清单：`.cursor/references/definition-of-done.md`

任务「做完」= **本任务验收标准** + **下列站立门槛**：

### Correctness

- 验收标准满足；运行时验证过，不只是编译通过  
- 新行为有测试（无改动会失败、有改动会通过）  
- 旧测试不回归  

### Quality

- 命名与结构自解释；无无关重构  
- 无死代码、调试残留  
- 符合 `project-coding-standards`（最小改动、可加可不加的边界不加）  

### Integration / Docs / Ship

- 与现有系统兼容；公开行为有文档  
- 涉及输入/鉴权/数据时过安全审视  
- 关键路径有可观测性；风险改动有回滚思路  
- 需要时经人确认再合并  

---

## 6. 五轴代码评审（`/review`）

命令：`.cursor/commands/review.md` → skill `code-review-and-quality` + persona `persona-code-reviewer`

| 轴 | 审什么 |
|----|--------|
| Correctness | 是否符合需求；边界与错误路径；测试是否测对 |
| Readability | 命名、控制流、能否更短；抽象是否赚回复杂度 |
| Architecture | 是否贴合现有模式；边界是否干净；有无特性逻辑泄漏到公共层 |
| Security | 输入、密钥、鉴权、不可信外部数据 |
| Performance | 无界循环、热路径分配、同步卡顿等 |

### 发现分级

| 标记 | 含义 | 作者动作 |
|------|------|----------|
| （无前缀） | 必须改 | 合并前处理 |
| **Critical:** | 阻塞合并 | 安全/丢数据/功能坏 |
| **Nit:** | 可选 | 可忽略 |
| **Optional:** / **Consider:** | 建议 | 可采纳可不采纳 |
| **FYI** | 仅告知 | 无需动作 |

批准标准：**明显提升整体代码健康即可批准**，不要求完美。按杠杆排序：正确性/安全 → 结构 → 其余。

---

## 7. `/ship` 发布审计（扇出）

命令：`.cursor/commands/ship.md`

小改动（≤2 文件、&lt;50 行、且不碰鉴权/支付/数据/配置）可跳过扇出；否则：

**Phase A** 并行三个 persona：

1. `persona-code-reviewer` — 五轴质量  
2. `persona-security-auditor` — 安全  
3. `persona-test-engineer` — 测试策略与覆盖  

**Phase B** 主会话合并：质量 Critical/Important、安全 Critical/High、性能、文档/回滚/监控缺口  

**Phase C** 给出：

```text
## Ship Decision: GO | NO-GO
### Blockers
### Recommended fixes
### Acknowledged risks
### Rollback plan
```

其它常用命令：

| 命令 | 用途 |
|------|------|
| `/spec` | 先写规格 |
| `/plan` | 任务拆解 |
| `/build` | 按增量实现 |
| `/test` | 测试策略 |
| `/code-simplify` | 简化评审后代码 |
| `/webperf` | Web 性能（本仓库主路径为 Unity，慎用） |

---

## 8. 本仓库额外硬边界（审计时一并查）

来自 `project-stack.mdc`：

- Unity 操作走 Pipeline；`unity` CLI **必须** `--proxy-disable`  
- 改 C#：`set_autotick` → `recompile` → 再测  
- 不提交 `Library/` `Temp/` `Logs/` `Obj/`、密钥、`.env`  
- 不把 `com.unity.pipeline` 改回 registry；未确认不改 `manifest.json` / 大规模目录  
- 不手改 `.unity` YAML（除非用户明确要求）  
- 新代码一律 C#，不要新增 Lua；XLua/Lua 已归档到 `Unused~/`  

编码审计额外看：

- 最小正确改动；无包装噪音  
- 新逻辑写 C#；动存量 Lua 才用匈牙利命名 + `_` 私有函数  
- 不确定先 AskQuestion（推荐项置顶），不猜着实现  

---

## 9. Agent 自检清单（每次非琐碎改动）

```text
[ ] 是否已用 using-agent-skills 路由并实际读了对应 SKILL？
[ ] 实现是否按增量切片，每片可验证？
[ ] Unity 相关是否走 pipeline + --proxy-disable？
[ ] 测试/编译/运行时证据是否齐全？（DoD Correctness）
[ ] 是否做过五轴自审或 /review？（合并前）
[ ] 公开行为/模块用法是否更新 Docs/？
[ ] 回复是否列出 Skills: ...？
[ ] /ship 场景是否跑过 persona 扇出并给出 GO/NO-GO？
```

---

## 10. 原则摘要（from using-agent-skills）

1. **显式假设** — 非琐碎需求先抛 ASSUMPTIONS，允许纠正  
2. **主动管理困惑** — 不一致时停下提问，不硬猜  
3. **该反对就反对** — 指出具体代价，给替代方案  
4. **强制简单** — 能更短就更短；抽象要赚回复杂度  
5. **范围纪律** — 只改被要求的；不做顺手大扫除  
6. **验证不靠感觉** — 有测试/编译/运行时证据才算完  
