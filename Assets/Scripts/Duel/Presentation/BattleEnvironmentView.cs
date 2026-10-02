using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleEnvironmentView : MonoBehaviour
    {
        [SerializeField] Animator[] m_animators;
        [SerializeField] string[] m_states;
        [SerializeField] Camera m_camera;
        int m_width,m_height;
        void Start()
        {
            for(int i=0;i<m_animators.Length;i++)m_animators[i].Play(m_states[i],0,0);
            FitCamera();
        }
        void Update() {if(m_width!=Screen.width||m_height!=Screen.height)FitCamera();}
        void FitCamera()
        {
            m_width=Screen.width;m_height=Screen.height;
            float aspect=(float)m_width*9/m_height;
            m_camera.fieldOfView=aspect>16?30+16-aspect:30;
        }
    }
}
