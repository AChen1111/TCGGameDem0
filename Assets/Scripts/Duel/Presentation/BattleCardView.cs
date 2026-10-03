using LitMotion;
using UnityEngine;
using TMPro;

namespace AChen.Duel.Presentation
{
    public sealed class BattleCardView : MonoBehaviour
    {
        [SerializeField] BattleHighlightView m_surface;
        [SerializeField] BattleHighlightView m_backSurface;
        [SerializeField] BattleHighlightView m_outline;
        [SerializeField] BoxCollider m_hitbox;
        [SerializeField] Transform m_cardPlane, m_pivot, m_offset, m_turn;
        [SerializeField] Texture m_back;
        [SerializeField] TextMeshPro m_stats;
        Texture m_art;
        MotionHandle m_hoverMotion;
        BattleCardPose m_restPose;
        bool m_hand, m_selected;
        public int InstanceId { get; private set; }
        public Texture Art => m_art;
        public bool IsFaceDown { get; private set; }
        public BoxCollider Hitbox => m_hitbox;
        public void Bind(int instanceId, Texture art) { InstanceId = instanceId; m_art = art; }
        public Vector3 ActionWorldAnchor => m_offset.position;
        public BattleCardPose CapturePose() => new BattleCardPose(transform.position, transform.rotation,
            m_cardPlane.localRotation, m_pivot.localPosition, m_pivot.localScale,
            m_offset.localPosition, m_offset.localRotation, m_turn.localRotation, m_hand);
        public void Apply(CardView card, bool faceVisible)
        {
            IsFaceDown = !faceVisible;
            m_surface.SetTexture(m_art); m_backSurface.SetTexture(m_back);
            m_surface.SetNegated(card.Negated && faceVisible && card.Zone.IsSlot);
            SetEffectAvailable(card.EffectAvailable);
            bool monsterZone = card.Zone.Kind is DuelZone.Monster or DuelZone.ExtraMonster;
            m_stats.gameObject.SetActive(monsterZone && card.Definition.Kind==CardKind.Monster && faceVisible);
            string level = card.Definition.Frame==CardFrame.Link ? "LINK " + card.Level : "★" + card.Level;
            m_stats.text=level+"\n"+(card.Definition.Frame==CardFrame.Link?CardCatalog.FormatStat(card.Attack):CardCatalog.FormatStat(card.Attack)+" / "+(card.Defense.HasValue ? CardCatalog.FormatStat(card.Defense.Value) : "—"))
                +(card.MaterialCount > 0 ? "\n素材 " + card.MaterialCount : "");
            m_stats.transform.position = transform.position + new Vector3(0,.35f,card.Owner==0?-5.3f:5.3f);
        }
        void LateUpdate() => m_stats.transform.rotation=Quaternion.Euler(70,0,0);
        public void SetEffectAvailable(bool available)
        { m_surface.SetEffectAvailable(available); m_backSurface.SetEffectAvailable(available); m_outline.SetEffectAvailable(available); }
        public void SetSelected(bool selected)
        {
            m_selected=selected; m_outline.SetSelected(selected);
            if (!m_hand) return;
            m_hoverMotion.TryCancel(); var from=CapturePose();
            var to=m_restPose.WithAppeal(selected);
            m_hoverMotion=LMotion.Create(0f,1f,.15f).WithEase(Ease.OutCubic).Bind(t=>SetPose(BattleCardPose.Lerp(from,to,t))).AddTo(this);
        }
        public void SetRestPose(BattleCardPose pose) {m_restPose=pose;m_hand=pose.Hand;}
        public void SetPose(BattleCardPose pose)
        {
            transform.SetPositionAndRotation(pose.Position,pose.Rotation);
            m_cardPlane.localRotation=pose.PlaneRotation; m_pivot.localPosition=pose.PivotPosition;
            m_pivot.localScale=pose.Scale; m_offset.localPosition=pose.OffsetPosition;
            m_offset.localRotation=pose.OffsetRotation; m_turn.localRotation=pose.TurnRotation;
        }
        public void Hover(bool raised)
        {
            if (!m_hand || m_selected) return;
            m_hoverMotion.TryCancel();
            Vector3 target=raised?new Vector3(0,2,1):Vector3.zero;
            m_hoverMotion=LMotion.Create(m_offset.localPosition,target,.1f).WithEase(Ease.OutCubic)
                .Bind(p=>m_offset.localPosition=p).AddTo(this);
        }
        public void StopMotion() => m_hoverMotion.TryCancel();
        void OnDisable() => StopMotion();
    }
}
