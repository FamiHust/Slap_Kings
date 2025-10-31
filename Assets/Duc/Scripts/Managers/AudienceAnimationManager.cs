using UnityEngine;

namespace Duc
{
    public class AudienceAnimationManager : SingletonManager<AudienceAnimationManager>
    {
        [Header("Registered Audience Units (auto-managed)")]
        [SerializeField] private System.Collections.Generic.List<AudienceUnit> m_AudienceUnits = new System.Collections.Generic.List<AudienceUnit>();

        protected override void OnInitialize()
        {
            var found = Object.FindObjectsOfType<AudienceUnit>(true);
            for (int i = 0; i < found.Length; i++)
            {
                if (!m_AudienceUnits.Contains(found[i]))
                    m_AudienceUnits.Add(found[i]);
            }
        }

        protected override void OnCleanup()
        {
            // Nothing to cleanup for now
        }

        public void PlayVictoryRandom()
        {
            for (int i = 0; i < m_AudienceUnits.Count; i++)
            {
                var unit = m_AudienceUnits[i];
                if (unit != null) unit.PlayVictoryRandom();
            }
        }

        public void PlayDefeatRandom()
        {
            for (int i = 0; i < m_AudienceUnits.Count; i++)
            {
                var unit = m_AudienceUnits[i];
                if (unit != null) unit.PlayDefeatRandom();
            }
        }

        public void PlayApplauseRandom()
        {
            for (int i = 0; i < m_AudienceUnits.Count; i++)
            {
                var unit = m_AudienceUnits[i];
                if (unit != null) unit.PlayApplauseRandom();
            }
        }

        public void Register(AudienceUnit unit)
        {
            if (unit == null) return;
            if (!m_AudienceUnits.Contains(unit))
                m_AudienceUnits.Add(unit);
        }

        public void Unregister(AudienceUnit unit)
        {
            if (unit == null) return;
            int idx = m_AudienceUnits.IndexOf(unit);
            if (idx >= 0) m_AudienceUnits.RemoveAt(idx);
        }
    }
}


