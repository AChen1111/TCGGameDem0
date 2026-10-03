using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleChainView : MonoBehaviour
    {
        sealed class Entry { public long Chain; public int Number, Card; public BattleChainBadge Badge; }
        [SerializeField] BattleChainBadge m_prefab;
        [SerializeField] Camera m_camera;
        readonly List<Entry> m_entries = new List<Entry>();
        public void Add(DuelViewChange change)
        {
            var badge = Instantiate(m_prefab, transform); badge.Bind(change.LinkNumber);
            m_entries.Add(new Entry { Chain = change.ChainId, Number = change.LinkNumber, Card = change.InstanceId, Badge = badge });
        }
        public void Emphasize(DuelViewChange change) => Find(change).Badge.Emphasize(change.IsNegated);
        Entry Find(DuelViewChange change) => m_entries.Single(x => x.Chain == change.ChainId && x.Number == change.LinkNumber);
        public void Remove(DuelViewChange change)
        { var entry = Find(change); Destroy(entry.Badge.gameObject); m_entries.Remove(entry); }
        public void Refresh(BattleSceneController scene, DuelView view)
        {
            var grouped = new Dictionary<string, int>();
            foreach (var entry in m_entries)
            {
                var card = view.Card(entry.Card);
                bool onField = card.Zone.IsSlot;
                string key = onField ? "card:" + entry.Card : card.Zone.ToString();
                int offset = grouped.TryGetValue(key, out int value) ? value : 0; grouped[key] = offset + 1;
                Vector3 anchor = onField ? scene.CardObject(entry.Card).ActionWorldAnchor : scene.RegionAnchor(card.Zone);
                entry.Badge.transform.SetPositionAndRotation(anchor + Vector3.up * 3 + Vector3.right * offset * 5, m_camera.transform.rotation);
            }
        }
        public void Clear()
        { foreach (var entry in m_entries) Destroy(entry.Badge.gameObject); m_entries.Clear(); }
    }
}
