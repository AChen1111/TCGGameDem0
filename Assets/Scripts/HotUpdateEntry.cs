using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public static class HotUpdateEntry
{
    public const string InitSceneAddress = "Init";

    public static void Boot()
    {
        ALog.Init();
        LuaPadHost.Boot();
        Addressables.LoadSceneAsync(InitSceneAddress, LoadSceneMode.Single);
    }
}
