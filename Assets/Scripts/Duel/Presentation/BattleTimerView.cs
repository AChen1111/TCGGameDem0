using TMPro;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    /// <summary>原 Timer_c001 的进度和行动方材质参数适配。</summary>
    public sealed class BattleTimerView : MonoBehaviour
    {
        [SerializeField] Renderer[] m_renderers;
        [SerializeField] TMP_Text m_seconds;
        [SerializeField] Color m_playerOneColor = new Color(.1745283f, .7428502f, 1f);
        [SerializeField] Color m_playerTwoColor = new Color(1f, .08018869f, .17268333f);
        MaterialPropertyBlock m_properties;
        void Awake() => m_properties = new MaterialPropertyBlock();

        public void Refresh(DuelView view)
        {
            float seconds = view.Seconds[view.ActivePlayer];
            m_seconds.text = seconds > 0 ? Mathf.CeilToInt(seconds).ToString() : "时间结束";
            foreach (Renderer renderer in m_renderers)
            {
                renderer.GetPropertyBlock(m_properties);
                m_properties.SetFloat("_MaxTime", seconds / 180f);
                m_properties.SetFloat("_AddTime", 0f);
                m_properties.SetFloat("_SwitchTurn", view.ActivePlayer);
                m_properties.SetFloat("_Active", view.TimerPaused || seconds <= 0 ? 0f : 1f);
                m_properties.SetColor("_ColorP1", m_playerOneColor);
                m_properties.SetColor("_ColorP2", m_playerTwoColor);
                renderer.SetPropertyBlock(m_properties);
            }
        }
    }
}
