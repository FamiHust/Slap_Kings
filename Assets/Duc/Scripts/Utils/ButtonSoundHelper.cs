using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Duc
{
    [RequireComponent(typeof(Button))]
    public class ButtonSoundHelper : MonoBehaviour
    {
        private Button m_Button;
        
        private void Awake()
        {
            m_Button = GetComponent<Button>();
            
            if (m_Button != null)
            {
                m_Button.onClick.AddListener(PlayButtonSound);
            }
        }
        
        private void PlayButtonSound()
        {
            var soundManager = SoundManager.Get();
            if (soundManager != null)
            {
                soundManager.PlayButtonSound();
            }
        }
        
        private void OnDestroy()
        {
            if (m_Button != null)
            {
                m_Button.onClick.RemoveListener(PlayButtonSound);
            }
        }
    }
}
