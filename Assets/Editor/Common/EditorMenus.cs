/// <summary>
/// 项目 Editor 菜单根路径, 按功能分组; 窗口类工具同时在 Window/TCG 下提供入口.
/// 热更编辑器程序集(HotUpdate.Editor)无法引用本类, 其菜单字符串与这里保持同一分组约定.
/// </summary>
public static class EditorMenus
{
    public const string Backend = "Tools/后端服务/";
    public const string Release = "Tools/热更发布/";
    public const string Config = "Tools/配置表/";
    public const string Ops = "Tools/运营工具/";
    public const string Art = "Tools/美术资源/";
    public const string Window = "Window/TCG/";
}
