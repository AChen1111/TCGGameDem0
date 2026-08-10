---
name: project-coding-standards
description: Enforces this project's coding rules: keep code minimal, skip optional edge-case guards, Lua Hungarian member naming (m_typeName) and private _funcs, ask when unsure, and list invoked skills in every reply. Use when writing or editing any code in this repo (Lua, C#, or otherwise).
---

# Project Coding Standards

编码时必须遵守以下规则。

## a. 代码尽量精简

- Prefer the smallest correct change.
- Do not add unrelated comments, wrappers, or defensive noise.

## b. 边界条件

- 允许报错；不要为无关边界条件加报错处理。
- 可加可不加的边界条件，一律不加。

## c. Lua 命名

编写 Lua 代码时使用如下命名法：

- 成员变量：`m_类型+变量名`（匈牙利简写）
- 私有函数：以 `_` 开头

示例：

| 种类 | 写法 |
|------|------|
| number | `m_nCount` |
| string | `m_strName` |
| bool | `m_bReady` |
| button | `m_btnConfirm` |
| 私有函数 | `_updateView` |

## d. 不确定就问

有不确定的内容，请询问。不要猜着实现。

## e. 每次输出列出本次 skills

在每次对用户的输出结果末尾，列出这次调用的所有 skills：

```
Skills: skill-a, skill-b, ...
```
