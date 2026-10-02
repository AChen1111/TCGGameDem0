using TMPro;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleZoneView : MonoBehaviour
    {
        [SerializeField] DuelZone m_kind;
        [SerializeField] int m_player;
        [SerializeField] int m_slot;
        [SerializeField] Transform m_anchor;
        [SerializeField] BoxCollider m_hitbox;
        [SerializeField] BattleHighlightView m_surface;
        [SerializeField] TextMeshPro m_label;
        public ZoneRef Zone => new ZoneRef(m_kind, m_player, m_slot);
        public Transform Anchor => m_anchor;
        public BoxCollider Hitbox => m_hitbox;
        public void SetHint(bool available) => m_surface.SetTargetHint(available);
        public void SetEffectAvailable(bool available) => m_surface.SetEffectAvailable(available);
        public void Refresh(DuelView view)
        {
            if (!Zone.IsSlot) m_label.text = BattleLabels.Zone(m_kind) + "  " + view.InZone(Zone).Count;
            m_surface.SetEffectAvailable(System.Linq.Enumerable.Contains(view.AvailablePiles, Zone));
        }
    }
}
