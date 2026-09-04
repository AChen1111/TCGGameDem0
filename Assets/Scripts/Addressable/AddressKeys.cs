public static class AddressKeys
{
    public static class Prefab
    {
        public static readonly string AvatarItem = "AvatarItem";
        public static readonly string AvatarSelectWindow = "AvatarSelectWindow";
        public static readonly string CardPackRowPrefab = "CardPackRowPrefab";
        public static readonly string ChangeNameWindow = "ChangeNameWindow";
        public static readonly string LogInWindow = "LogInWindow";
        public static readonly string MessageWindow = "MessageWindow";
        public static readonly string PreGameUIPanel = "PreGameUIPanel";
        public static readonly string ShopWindows = "ShopWindows";
    }
    public static class Sprite
    {
        public static readonly string a_00 = "a_00";
        public static readonly string a_01 = "a_01";
        public static readonly string a_02 = "a_02";
        public static readonly string a_03 = "a_03";
        public static readonly string a_04 = "a_04";
        public static readonly string a_05 = "a_05";
        public static readonly string a_06 = "a_06";
        public static readonly string a_07 = "a_07";
        public static readonly string a_08 = "a_08";
        public static readonly string a_09 = "a_09";
        public static readonly string a_10 = "a_10";
        public static readonly string a_11 = "a_11";
        public static readonly string a_12 = "a_12";
        public static readonly string a_13 = "a_13";
        public static readonly string c_00 = "c_00";
        public static readonly string c_01 = "c_01";
        public static readonly string c_02 = "c_02";
        public static readonly string c_03 = "c_03";
        public static readonly string c_04 = "c_04";
        public static readonly string c_05 = "c_05";
        public static readonly string c_06 = "c_06";
        public static readonly string c_07 = "c_07";
        public static readonly string c_08 = "c_08";
        public static readonly string c_09 = "c_09";
        public static readonly string c_10 = "c_10";
        public static readonly string w_00_Down = "w_00_Down";
        public static readonly string w_00_Sprite = "w_00_Sprite";
        public static readonly string w_01_Down = "w_01_Down";
        public static readonly string w_01_Sprite = "w_01_Sprite";
        public static readonly string w_02_Down = "w_02_Down";
        public static readonly string w_02_Sprite = "w_02_Sprite";
        public static readonly string w_03_Down = "w_03_Down";
        public static readonly string w_03_Sprite = "w_03_Sprite";
        public static readonly string w_04_Down = "w_04_Down";
        public static readonly string w_04_Sprite = "w_04_Sprite";
        public static readonly string w_05_Down = "w_05_Down";
        public static readonly string w_05_Sprite = "w_05_Sprite";
        public static readonly string w_06_Down = "w_06_Down";
        public static readonly string w_06_Sprite = "w_06_Sprite";
        public static readonly string w_07_Down = "w_07_Down";
        public static readonly string w_07_Sprite = "w_07_Sprite";
        public static readonly string w_08_Down = "w_08_Down";
        public static readonly string w_08_Sprite = "w_08_Sprite";
        public static readonly string w_09_Down = "w_09_Down";
        public static readonly string w_09_Sprite = "w_09_Sprite";
        public static readonly string w_10_Down = "w_10_Down";
        public static readonly string w_10_Sprite = "w_10_Sprite";
    }
    public static class Scene
    {
        public static readonly string GameScene = "GameScene";
        public static readonly string LogIn = "LogIn";
    }
    public static class UISettings
    {
        public static readonly string LogInSetting = "LogInSetting";
        public static readonly string PreGameSceneUI = "PreGameSceneUI";
        public static readonly string UISetting = "UISetting";
    }

    public static string GetAvatarAddress(int avatarId)
    {
        return $"a_{avatarId:D2}";
    }
    public static string GetBackgroundDownAddress(int backgroundId)
    {
        return $"w_{backgroundId:D2}_Down";
    }
    public static string GetBackgroundSpriteAddress(int backgroundId)
    {
        return $"w_{backgroundId:D2}_Sprite";
    }
}
