using System;
using System.Linq;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    [Serializable]
    public sealed class DuelDemoCardEntry { public string CardId; public string SourcePool; public int Count; }
    [Serializable]
    public sealed class DuelDemoPlayerEntry { public string Name; public int AvatarId; public int LP = 8000; }

    [CreateAssetMenu(menuName = "TCG/Duel/Demo Preset")]
    public sealed class DuelDemoPreset : ScriptableObject
    {
        [SerializeField] DuelDemoCardEntry[] m_main;
        [SerializeField] DuelDemoCardEntry[] m_extra;
        [SerializeField] DuelDemoPlayerEntry[] m_players =
        { new DuelDemoPlayerEntry { Name = "玩家", AvatarId = 1010001 }, new DuelDemoPlayerEntry { Name = "对手", AvatarId = 1010002 } };
        public DuelDemoSession CreateSession() => new DuelDemoSession(Expand(m_main), Expand(m_extra), 5, 180,
            m_players.Select(player => new DuelPlayerView(player.Name, player.AvatarId, player.LP)));
        static DuelCardSpec[] Expand(DuelDemoCardEntry[] entries) => entries.SelectMany(entry =>
        {
            CardCatalog.TryGet(entry.CardId, out var row);
            var spec = new DuelCardSpec(entry.CardId, entry.SourcePool, (CardKind)row.Kind,
                (CardFrame)row.Frame, (CardSpellTrapType)row.SpellTrapType, (CardFlags)row.Flags);
            return Enumerable.Repeat(spec, entry.Count);
        }).ToArray();
    }
}
