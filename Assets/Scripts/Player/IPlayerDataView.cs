using AChen.Events;
using AChen.Networking;
using AChen.Player;

public interface IPlayerDataView
{
    void LoadPlayerData()
    {
        PlayerData data = PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer : null;
        OnPlayerDataChanged(data);
    }

    void OnPlayerDataChanged(PlayerData data);
}

public static class PlayerDataViews
{
    public static void Bind(IPlayerDataView view)
    {
        EventCenter.AddListener<PlayerData>(GameEvent.PlayerDataChanged, view.OnPlayerDataChanged);
        view.LoadPlayerData();
    }

    public static void Unbind(IPlayerDataView view)
    {
        EventCenter.RemoveListener<PlayerData>(GameEvent.PlayerDataChanged, view.OnPlayerDataChanged);
    }
}
