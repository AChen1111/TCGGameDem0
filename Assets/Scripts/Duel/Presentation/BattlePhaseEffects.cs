using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace AChen.Duel.Presentation
{
    public sealed class BattlePhaseEffects : MonoBehaviour
    {
        [SerializeField] GameObject[] m_near, m_far, m_turns;
        [SerializeField] PlayableDirector[] m_directors;
        [SerializeField] ParticleSystem[] m_particles;
        GameObject m_current;
        public float Begin(DuelViewChange change)
        {
            Stop();
            m_current=change.Kind==DuelChangeKind.Turn?m_turns[change.View.ActivePlayer]
                :(change.View.ActivePlayer==0?m_near:m_far)[(int)change.View.Phase];
            m_current.SetActive(true);
            float duration=.65f;
            foreach(var director in m_directors.Where(x=>x.gameObject.activeInHierarchy))
            {
                director.time=0; director.Evaluate(); director.Play();
                duration=Mathf.Max(duration,(float)director.duration);
            }
            foreach(var particle in m_particles.Where(x=>x.gameObject.activeInHierarchy))particle.Play(true);
            return duration;
        }
        public void Stop()
        {
            foreach(var director in m_directors)director.Stop();
            foreach(var particle in m_particles)particle.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            foreach(var effect in m_near.Concat(m_far).Concat(m_turns))effect.SetActive(false);
        }
        void OnDisable()=>Stop();
    }
}
