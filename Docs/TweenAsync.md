# 动画与异步

Tween 用 **LitMotion**，异步用 **UniTask**。已移除 DOTween。

相关文档：[Lua 系统](LuaSystem.md)

---

## 包

| 包 | 版本 | 用途 |
|---|---|---|
| `com.annulusgames.lit-motion` | v2.0.2 | Tween / Sequence |
| `com.cysharp.unitask` | 2.5.11 | 零分配 async/await |

安装方式见官方文档：

- LitMotion：<https://github.com/annulusgames/LitMotion#installation>
- UniTask：<https://github.com/Cysharp/UniTask#install-via-git-url>

---

## 用法（C#）

```csharp
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;

LMotion.Create(0f, 1f, 0.25f)
    .WithEase(Ease.OutQuad)
    .BindToLocalScale(transform);

await LMotion.Create(0f, 1f, 0.25f)
    .BindToPositionX(transform)
    .ToUniTask(destroyCancellationToken);
```

在 C# 里直接用。存量 Lua 若仍要 tween，不要写 `CS.xxx`，由 C# 包一层再走 `Include.lua` 别名。
