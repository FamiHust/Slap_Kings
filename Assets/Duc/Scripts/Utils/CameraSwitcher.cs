using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using Duc.Managers;

namespace Duc
{
    public class CameraSwitcher : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private CinemachineVirtualCamera m_Cam1;
        [SerializeField] private CinemachineVirtualCamera m_Cam2;

        [SerializeField] private GameplayInput m_GameplayInput;
        
        [Header("Camera Transition Settings")]
        [SerializeField] private float m_TransitionSpeed;
        [SerializeField] private float m_BlendTime;
        
        private bool isCam1Active = true;

        private void Start()
        {
            m_Cam1.Priority = 20; 
            m_Cam2.Priority = 0;
            
            SetupCameraTransitions();
            RegisterCamerasWithShakeManager();
        }
        
        private void RegisterCamerasWithShakeManager()
        {
            if (CameraShakeManager.Instance != null)
            {
                if (m_Cam1 != null)
                    CameraShakeManager.Instance.AddCamera(m_Cam1);
                if (m_Cam2 != null)
                    CameraShakeManager.Instance.AddCamera(m_Cam2);
            }
        }
        
        private void SetupCameraTransitions()
        {
            var brain = CinemachineCore.Instance.GetActiveBrain(0);
            if (brain != null)
            {
                brain.m_DefaultBlend.m_Time = m_BlendTime;
            }
            
            SetupCameraBodySettings(m_Cam1);
            SetupCameraBodySettings(m_Cam2);
        }
        
        private void SetupCameraBodySettings(CinemachineVirtualCamera vcam)
        {
            if (vcam == null) return;
            
            var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_XDamping = m_TransitionSpeed;
                transposer.m_YDamping = m_TransitionSpeed;
                transposer.m_ZDamping = m_TransitionSpeed;
            }
            
            var composer = vcam.GetCinemachineComponent<CinemachineComposer>();
            if (composer != null)
            {
                composer.m_HorizontalDamping = m_TransitionSpeed;
                composer.m_VerticalDamping = m_TransitionSpeed;
            }
        }

        public void SwitchCamera()
        {
            bool previousState = isCam1Active;
            isCam1Active = !isCam1Active;
            
            if (isCam1Active)
            {
                m_Cam1.Priority = 20;
                m_Cam2.Priority = 0;
                m_GameplayInput.enabled = true;
            }
            else
            {
                m_Cam1.Priority = 0;
                m_Cam2.Priority = 20;
                m_GameplayInput.enabled = false;
            }
        }

        public void SwitchToPlayerCamera()
        {
            isCam1Active = true;
            m_Cam1.Priority = 20;
            m_Cam2.Priority = 0;
            m_GameplayInput.enabled = true;
        }

        public void SwitchToAICamera()
        {
            isCam1Active = false;
            m_Cam1.Priority = 0;
            m_Cam2.Priority = 20;
            m_GameplayInput.enabled = false;
        }

        public void SetTransitionSpeed(float speed)
        {
            m_TransitionSpeed = Mathf.Max(0.1f, speed);
            SetupCameraBodySettings(m_Cam1);
            SetupCameraBodySettings(m_Cam2);
        }
    
        public void SetBlendTime(float blendTime)
        {
            m_BlendTime = Mathf.Max(0.01f, blendTime);
            
            var brain = CinemachineCore.Instance.GetActiveBrain(0);
            if (brain != null)
            {
                brain.m_DefaultBlend.m_Time = m_BlendTime;
            }
        }

        public (float transitionSpeed, float blendTime) GetTransitionSettings()
        {
            return (m_TransitionSpeed, m_BlendTime);
        }
        
        // Camera Zoom Shake Methods
        public void TriggerCameraZoomShake(float zoomInAmount = 3f, float zoomOutAmount = 5f, float duration = 0.2f)
        {
            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ZoomShakeAllCameras(zoomInAmount, zoomOutAmount, duration);
            }
        }
        
        public void TriggerFOVShake(float fovAmount = 3f, float duration = 0.2f)
        {
            if (CameraShakeManager.Instance != null)
            {
                this.PublishEvent(new CameraFOVShakeEvent(fovAmount, duration));
            }
        }
        
        public void TriggerActiveCameraZoomShake(float zoomInAmount = 3f, float zoomOutAmount = 5f, float duration = 0.2f)
        {
            if (CameraShakeManager.Instance != null)
            {
                CinemachineVirtualCamera activeCamera = isCam1Active ? m_Cam1 : m_Cam2;
                if (activeCamera != null)
                {
                    CameraShakeManager.Instance.ZoomShakeCamera(activeCamera, zoomInAmount, zoomOutAmount, duration);
                }
            }
        }
        
        // Giữ lại cho tương thích
        public void TriggerCameraShake(float intensity = 1f, float duration = 0.5f)
        {
            TriggerCameraZoomShake(intensity * 3f, intensity * 5f, duration);
        }
        
        public void TriggerActiveCameraShake(float intensity = 1f, float duration = 0.5f)
        {
            TriggerActiveCameraZoomShake(intensity * 3f, intensity * 5f, duration);
        }
        
        public void StopAllCameraShakes()
        {
            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.StopAllShakes();
            }
        }
    }
}
