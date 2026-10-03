using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Presentation
{
    /// <summary>手牌顺序属于本地表现：保留现有成员次序，新进入成员追加，离手后再返回视为新进入。</summary>
    public sealed class BattleHandOrder
    {
        readonly List<int>[] m_seats = { new List<int>(), new List<int>() };
        public IReadOnlyList<int> Cards(int player) => m_seats[player];
        public int IndexOf(int player, int instanceId) => m_seats[player].IndexOf(instanceId);
        public int Count(int player) => m_seats[player].Count;
        public int[][] Capture() => m_seats.Select(seat => seat.ToArray()).ToArray();
        public void Clear() { foreach (var seat in m_seats) seat.Clear(); }
        public void Update(IReadOnlyList<CardView> cards)
        {
            for (int player = 0; player < m_seats.Length; player++)
            {
                var hand = cards.Where(c => c.Owner == player && c.Zone.Kind == DuelZone.Hand).Select(c => c.InstanceId).ToArray();
                var members = new HashSet<int>(hand);
                m_seats[player].RemoveAll(id => !members.Contains(id));
                foreach (int id in hand) if (!m_seats[player].Contains(id)) m_seats[player].Add(id);
            }
        }
    }
}
