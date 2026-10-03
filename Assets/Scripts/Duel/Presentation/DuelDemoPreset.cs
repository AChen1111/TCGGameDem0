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
        [SerializeField] DuelDemoCardEntry[] m_opponentMain;
        [SerializeField] DuelDemoCardEntry[] m_opponentExtra;
        [SerializeField] DuelDemoPlayerEntry[] m_players =
        { new DuelDemoPlayerEntry { Name = "玩家", AvatarId = 1010001 }, new DuelDemoPlayerEntry { Name = "对手", AvatarId = 1010002 } };
        public DuelDemoSession CreateSession() => new DuelDemoSession(Expand(m_main), Expand(m_extra), 5, 180,
            m_players.Select(player => new DuelPlayerView(player.Name, player.AvatarId, player.LP)));
        public LocalDuelSession CreateLocalSession()
        {
            var main = new[] { Expand(m_main), Expand(m_opponentMain) };
            var extra = new[] { Expand(m_extra), Expand(m_opponentExtra) };
            var catalog = Core.DuelCardCatalog.CreateDefault();
            for (int seat = 0; seat < 2; seat++)
            {
                if (main[seat].Length < 40 || main[seat].Length > 60 || extra[seat].Length > 15)
                    throw new InvalidOperationException("离线卡组必须为40至60张主卡、至多15张额外卡");
                foreach (var group in main[seat].Concat(extra[seat]).GroupBy(c => catalog.Get(c.CardId).OriginalNameId))
                    if (group.Count() > catalog.Get(group.First().CardId).MaxCopies)
                        throw new InvalidOperationException("离线卡组超过禁限数量：" + group.Key);
                if (main[seat].Any(c => catalog.Get(c.CardId).IsExtra) || extra[seat].Any(c => !catalog.Get(c.CardId).IsExtra))
                    throw new InvalidOperationException("主卡组与额外卡组分类不正确");
            }
            return new LocalDuelSession(() =>
            {
                var bytes = new byte[8];
                using (var random = System.Security.Cryptography.RandomNumberGenerator.Create()) random.GetBytes(bytes);
                return new Core.DuelStartRecord { MainDecks = main.Select(d => d.Select(c => c.CardId).ToArray()).ToArray(),
                    ExtraDecks = extra.Select(d => d.Select(c => c.CardId).ToArray()).ToArray(), Seed = BitConverter.ToUInt64(bytes, 0),
                    FirstPlayer = bytes[0] & 1, Shuffle = true };
            }, main.SelectMany(x => x).Concat(extra.SelectMany(x => x)),
                m_players.Select(player => new DuelPlayerView(player.Name, player.AvatarId)));
        }
        static DuelCardSpec[] Expand(DuelDemoCardEntry[] entries) => entries.SelectMany(entry =>
        {
            CardCatalog.TryGet(entry.CardId, out var row);
            var spec = new DuelCardSpec(entry.CardId, entry.SourcePool, (CardKind)row.Kind,
                (CardFrame)row.Frame, (CardSpellTrapType)row.SpellTrapType, (CardFlags)row.Flags);
            return Enumerable.Repeat(spec, entry.Count);
        }).ToArray();
    }
}
