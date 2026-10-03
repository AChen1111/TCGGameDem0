using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using AChen.Duel.Presentation;
using UnityEngine.UI;

public sealed class BattleZoneProperties : IPanelProperties
{
    public BattleSceneController Scene { get; }
    public ZoneRef Zone { get; }
    public BattleZoneProperties(BattleSceneController scene, ZoneRef zone) { Scene = scene; Zone = zone; }
}
public sealed class BattleZoneWindow : APanelController<BattleZoneProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtTitle;
    [SerializeField] Button m_BtnClose;
    [SerializeField] TextMeshProUGUI m_TxtClose;
    [SerializeField] TextMeshProUGUI m_TxtEmpty;
    // --tag_end: 自动生成--
    [SerializeField] GridListController m_list;
    bool m_bound;
    protected override float TransitionDuration => 0;
    protected override void OnOpen() => Bind();
    protected override void OnResume() { Unbind(); Bind(); }
    void Bind()
    {
        m_bound = true;
        transform.SetAsLastSibling();
        m_BtnClose.onClick.AddListener(Properties.Scene.CloseZone);
        Properties.Scene.Source.Changed += OnChanged;
        Refresh();
    }
    void OnChanged(DuelViewChange change)
    {
        if (change.Kind == DuelChangeKind.Action && change.View.HasPendingAction)
        {
            Properties.Scene.CloseZone();
            return;
        }
            if (change.Kind is DuelChangeKind.Move or DuelChangeKind.Reset or DuelChangeKind.Position or DuelChangeKind.AnimationCompleted or DuelChangeKind.State)
            Refresh();
    }
    void Refresh()
    {
        var scene = Properties.Scene;
        var cards = Properties.Zone.IsSlot
            ? scene.Source.Current.Cards.Where(c => c.Zone.Kind == Properties.Zone.Kind && (Properties.Zone.Player == -1 || c.Owner == Properties.Zone.Player)).ToArray()
            : scene.Source.Current.InZone(Properties.Zone).ToArray();
        cards = cards.Where(scene.CanInspect).ToArray();
        m_TxtTitle.text = (Properties.Zone.Player == -1 ? "共享 · " : Properties.Zone.Player == 0 ? "我方 · " : "对方 · ") + BattleLabels.Zone(Properties.Zone.Kind) + "  " + cards.Length;
        m_TxtEmpty.gameObject.SetActive(cards.Length == 0);
        var data = cards.Select(card => new BattleCardRowData(card, scene.CardObject(card.InstanceId).Art, scene)).ToList();
        int selected = data.FindIndex(x => x.Card.InstanceId == scene.SelectedCardId);
        m_list.InitList(AddressKeys.Prefab.BattleCardRow, data,
            index => scene.SelectCard(data[index].Card.InstanceId), selected, ScreenToken).Forget();
    }
    void Unbind()
    {
        if (!m_bound) return;
        m_bound = false;
        Properties.Scene.Source.Changed -= OnChanged;
        m_BtnClose.onClick.RemoveAllListeners(); m_list.ClearList();
    }
    protected override void OnHide() => Unbind();
    protected override void OnClose() => Unbind();
}
