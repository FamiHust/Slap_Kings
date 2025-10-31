using UnityEngine;

namespace Duc
{
    public class AudienceUnit : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator m_Animator;

        [Header("Trigger Parameters (Animator)")]
        [SerializeField] private string m_VictoryTrigger = "Victory";
        [SerializeField] private string m_DefeatTrigger = "Defeat";
        [SerializeField] private string m_ApplauseTrigger = "Applause";

        [Header("BlendTree Index Parameters (Animator)")]
        [SerializeField] private string m_VictoryIndexParam = "VictoryIndex";
        [SerializeField] private string m_DefeatIndexParam = "DefeatIndex";
        [SerializeField] private string m_ApplauseIndexParam = "ApplauseIndex";

        [Header("Clips Count per Tree (discrete indices: 0..N-1)")]
        [SerializeField] private int m_VictoryClipsCount = 3;
        [SerializeField] private int m_DefeatClipsCount = 3;
        [SerializeField] private int m_ApplauseClipsCount = 3;

        private System.Random m_Random;
        private int m_LastVictoryIndex = -1;
        private int m_LastDefeatIndex = -1;
        private int m_LastApplauseIndex = -1;

        private void Awake()
        {
            if (m_Animator == null)
                m_Animator = GetComponent<Animator>();
            int seed = unchecked(GetInstanceID() * 397) ^ System.Environment.TickCount;
            m_Random = new System.Random(seed);
        }

        private void OnEnable()
        {
            var mgr = AudienceAnimationManager.Instance;
            if (mgr != null)
                mgr.Register(this);
        }

        private void OnDisable()
        {
            var mgr = AudienceAnimationManager.Instance;
            if (mgr != null)
                mgr.Unregister(this);
        }

        public void PlayVictoryRandom()
        {
            if (m_Animator == null) return;
            int idx = GetRandomIndex(m_VictoryClipsCount, ref m_LastVictoryIndex);
            if (!string.IsNullOrEmpty(m_VictoryIndexParam))
                m_Animator.SetFloat(m_VictoryIndexParam, idx);
            if (!string.IsNullOrEmpty(m_VictoryTrigger))
            {
                m_Animator.ResetTrigger(m_VictoryTrigger);
                m_Animator.SetTrigger(m_VictoryTrigger);
            }
        }

        public void PlayDefeatRandom()
        {
            if (m_Animator == null) return;
            int idx = GetRandomIndex(m_DefeatClipsCount, ref m_LastDefeatIndex);
            if (!string.IsNullOrEmpty(m_DefeatIndexParam))
                m_Animator.SetFloat(m_DefeatIndexParam, idx);
            if (!string.IsNullOrEmpty(m_DefeatTrigger))
            {
                m_Animator.ResetTrigger(m_DefeatTrigger);
                m_Animator.SetTrigger(m_DefeatTrigger);
            }
        }

        public void PlayApplauseRandom()
        {
            if (m_Animator == null) return;
            int idx = GetRandomIndex(m_ApplauseClipsCount, ref m_LastApplauseIndex);
            if (!string.IsNullOrEmpty(m_ApplauseIndexParam))
                m_Animator.SetFloat(m_ApplauseIndexParam, idx);
            if (!string.IsNullOrEmpty(m_ApplauseTrigger))
            {
                m_Animator.ResetTrigger(m_ApplauseTrigger);
                m_Animator.SetTrigger(m_ApplauseTrigger);
            }
        }

        private int GetRandomIndex(int count, ref int lastIndex)
        {
            if (count <= 1)
            {
                lastIndex = 0;
                return 0;
            }
            int idx;
            // Try to avoid repeating the same index consecutively per unit
            do
            {
                idx = m_Random.Next(0, count);
            } while (idx == lastIndex && count > 1);
            lastIndex = idx;
            return idx;
        }
    }
}


