using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    [Serializable]
    public sealed class BattleCutinEntry
    {
        public string CardId;
        public string Address;
        public string Animation = "animation";
        public SkeletonDataAsset SkeletonData;
        public Vector2 Center;
        public Vector2 Size;
    }
    /// <summary>顶层遮罩在整个完整演出期间拦截输入；服务端时钟不受影响。</summary>
    public sealed class BattleCutinView : MonoBehaviour
    {
        [SerializeField] GameObject m_overlay;
        [SerializeField] RectTransform m_viewport;
        [SerializeField] SkeletonGraphic m_graphic;
        [SerializeField] BattleCutinEntry[] m_entries;
        Transform m_originalParent;
        public void Initialize(Transform canvasRoot) { m_originalParent = transform.parent; transform.SetParent(canvasRoot, false); }
        public void RestoreParent() => transform.SetParent(m_originalParent, false);
        public bool Playing { get; private set; }
        public bool Contains(string id) => m_entries.Any(e => e.CardId == id);
        public async UniTask PlayOnceAsync(string id, CancellationToken cancellationToken)
        {
            var entry = m_entries.Single(e => e.CardId == id);
            Playing = true;
            m_overlay.SetActive(true); m_overlay.transform.SetAsLastSibling();
            try
            {
                m_graphic.skeletonDataAsset = entry.SkeletonData;
                m_graphic.Initialize(true); m_graphic.timeScale = 1; m_graphic.UnscaledTime = true;
                m_graphic.Update(0); m_graphic.UpdateMesh();
                float meshScale = m_graphic.MeshScale;
                float scale = Mathf.Min(m_viewport.rect.width / (entry.Size.x * meshScale), m_viewport.rect.height / (entry.Size.y * meshScale));
                m_graphic.rectTransform.localScale = Vector3.one * scale;
                m_graphic.rectTransform.anchoredPosition = -entry.Center * meshScale * scale;
                var track = m_graphic.AnimationState.SetAnimation(0, entry.Animation, false);
                await UniTask.WaitUntil(() => track.IsComplete, cancellationToken: cancellationToken);
            }
            finally { m_overlay.SetActive(false); Playing = false; }
        }
    }
}
