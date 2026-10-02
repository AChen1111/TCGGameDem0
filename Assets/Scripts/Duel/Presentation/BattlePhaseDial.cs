using TMPro;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattlePhaseDial : MonoBehaviour
    {
        [SerializeField] BoxCollider m_hitbox;
        [SerializeField] TMP_Text m_main,m_above,m_below;
        [SerializeField] GameObject m_playerPart,m_opponentPart;
        [SerializeField] Renderer[] m_renderers;
        MaterialPropertyBlock m_properties;
        public BoxCollider Hitbox=>m_hitbox;
        void Awake()=>m_properties=new MaterialPropertyBlock();
        public void Refresh(DuelView view,bool animate)
        {
            m_above.text="TURN "+view.Turn;
            m_main.text=view.Phase switch {DuelPhase.Draw=>"Draw",DuelPhase.Standby=>"Standby",DuelPhase.Main1=>"Main1",DuelPhase.Battle=>"Battle",DuelPhase.Main2=>"Main2",_=>"End"};
            m_below.text="";
            m_playerPart.SetActive(view.ActivePlayer==0);m_opponentPart.SetActive(view.ActivePlayer==1);
            foreach(var renderer in m_renderers)
            {renderer.GetPropertyBlock(m_properties);m_properties.SetFloat("_SwitchTurn",view.ActivePlayer);renderer.SetPropertyBlock(m_properties);}
        }
    }
}