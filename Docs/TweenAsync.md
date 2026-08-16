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

业务逻辑仍优先写 Lua。LitMotion / UniTask 是 struct API，不要在业务 Lua 里直接 `CS.xxx`；需要时再在 C# 包一层，别名写进 `Include.lua`。
