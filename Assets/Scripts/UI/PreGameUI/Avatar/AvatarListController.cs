/// <summary>头像滑动列表,直接复用 GridListController.</summary>
public class AvatarListController : GridListController
{
    protected override string key => AddressKeys.Prefab.AvatarItemPrefab;
}