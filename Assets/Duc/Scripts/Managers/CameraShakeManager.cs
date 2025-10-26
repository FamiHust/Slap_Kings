using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

namespace Duc.Managers
{
    public class CameraShakeManager : SingletonManager<CameraShakeManager>
    {
        [Header("Zoom Shake Settings")]
        [SerializeField] private float m_ZoomInAmount = 3f;
        [SerializeField] private float m_ZoomOutAmount = 5f;
        [SerializeField] private float m_ZoomDuration = 0.2f;
        [SerializeField] private float m_ZoomInDuration = 0.08f;
        [SerializeField] private float m_ZoomOutDuration = 0.08f;
        [SerializeField] private Ease m_ZoomInEase = Ease.OutQuart;
        [SerializeField] private Ease m_ZoomOutEase = Ease.OutBack;
        
        [Header("Position Shake Settings (Optional)")]
        [SerializeField] private bool m_EnablePositionShake = false;
        [SerializeField] private float m_PositionShakeIntensity = 0.5f;
        [SerializeField] private float m_PositionShakeDuration = 0.3f;
        
        [Header("Camera References")]
        [SerializeField] private List<CinemachineVirtualCamera> m_Cameras = new List<CinemachineVirtualCamera>();
        
        private Dictionary<CinemachineVirtualCamera, float> m_OriginalFOVs = new Dictionary<CinemachineVirtualCamera, float>();
        private Dictionary<CinemachineVirtualCamera, Vector3> m_OriginalPositions = new Dictionary<CinemachineVirtualCamera, Vector3>();
        private Dictionary<CinemachineVirtualCamera, Sequence> m_ActiveZoomSequences = new Dictionary<CinemachineVirtualCamera, Sequence>();
        
        protected override void OnInitialize()
        {
            InitializeCameras();
            SubscribeToEvents();
        }
        
        protected override void OnCleanup()
        {
            UnsubscribeFromEvents();
            StopAllShakes();
        }
        
        private void InitializeCameras()
        {
            if (m_Cameras.Count == 0)
            {
                CinemachineVirtualCamera[] foundCameras = FindObjectsOfType<CinemachineVirtualCamera>();
                m_Cameras.AddRange(foundCameras);
            }
            
            foreach (var camera in m_Cameras)
            {
                if (camera != null)
                {
                    m_OriginalFOVs[camera] = camera.m_Lens.FieldOfView;
                    m_OriginalPositions[camera] = camera.transform.position;
                }
            }
        }
        
        private void SubscribeToEvents()
        {
            GameEventSystem.Instance.Subscribe<PlayerHealthChangedEvent>(OnPlayerHit);
            GameEventSystem.Instance.Subscribe<AIHealthChangedEvent>(OnAIHit);
            
            GameEventSystem.Instance.Subscribe<CameraShakeEvent>(OnCameraShakeEvent);
            GameEventSystem.Instance.Subscribe<CameraFOVShakeEvent>(OnCameraFOVShakeEvent);
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEventSystem.Instance.Unsubscribe<PlayerHealthChangedEvent>(OnPlayerHit);
            GameEventSystem.Instance.Unsubscribe<AIHealthChangedEvent>(OnAIHit);
            
            GameEventSystem.Instance.Unsubscribe<CameraShakeEvent>(OnCameraShakeEvent);
            GameEventSystem.Instance.Unsubscribe<CameraFOVShakeEvent>(OnCameraFOVShakeEvent);
        }
        
        private void OnPlayerHit(PlayerHealthChangedEvent hitEvent)
        {
            if (hitEvent.DamageAmount > 0)
            {
                ZoomShakeAllCameras(m_ZoomInAmount, m_ZoomOutAmount, m_ZoomDuration);
                TriggerVibration();
            }
        }
        
        private void OnAIHit(AIHealthChangedEvent hitEvent)
        {
            if (hitEvent.DamageAmount > 0)
            {
                ZoomShakeAllCameras(m_ZoomInAmount * 0.7f, m_ZoomOutAmount * 0.7f, m_ZoomDuration);
                TriggerVibration();
            }
        }
        
        private void OnCameraShakeEvent(CameraShakeEvent shakeEvent)
        {
            if (shakeEvent.ShakeFOV)
            {
                ZoomShakeAllCameras(m_ZoomInAmount, m_ZoomOutAmount, m_ZoomDuration);
            }
            else
            {
                ShakeAllCameras(shakeEvent.Intensity, shakeEvent.Duration);
            }
            TriggerVibration();
        }
        
        private void OnCameraFOVShakeEvent(CameraFOVShakeEvent fovShakeEvent)
        {
            ZoomShakeAllCameras(fovShakeEvent.FOVAmount, fovShakeEvent.FOVAmount * 1.2f, fovShakeEvent.Duration);
            TriggerVibration();
        }
        
        public void ZoomShakeAllCameras(float zoomInAmount = -1f, float zoomOutAmount = -1f, float duration = -1f)
        {
            float zoomIn = zoomInAmount > 0 ? zoomInAmount : m_ZoomInAmount;
            float zoomOut = zoomOutAmount > 0 ? zoomOutAmount : m_ZoomOutAmount;
            float zoomDuration = duration > 0 ? duration : m_ZoomDuration;
            
            foreach (var camera in m_Cameras)
            {
                if (camera != null)
                {
                    ZoomShakeCamera(camera, zoomIn, zoomOut, zoomDuration);
                }
            }
            
            TriggerVibration();
        }
        
        public void ZoomShakeCamera(CinemachineVirtualCamera camera, float zoomInAmount = -1f, float zoomOutAmount = -1f, float duration = -1f)
        {
            if (camera == null) return;
            
            float zoomIn = zoomInAmount > 0 ? zoomInAmount : m_ZoomInAmount;
            float zoomOut = zoomOutAmount > 0 ? zoomOutAmount : m_ZoomOutAmount;
            float zoomDuration = duration > 0 ? duration : m_ZoomDuration;
            
            if (m_ActiveZoomSequences.ContainsKey(camera) && m_ActiveZoomSequences[camera] != null)
            {
                m_ActiveZoomSequences[camera].Kill();
            }
            
            Sequence zoomSequence = DOTween.Sequence();
            float originalFOV = m_OriginalFOVs[camera];
            
            float targetFOVIn = originalFOV - zoomIn;
            zoomSequence.Append(DOTween.To(() => camera.m_Lens.FieldOfView, 
                x => camera.m_Lens.FieldOfView = x, 
                targetFOVIn, m_ZoomInDuration)
                .SetEase(m_ZoomInEase));
            
            float targetFOVOut = originalFOV + zoomOut;
            zoomSequence.Append(DOTween.To(() => camera.m_Lens.FieldOfView, 
                x => camera.m_Lens.FieldOfView = x, 
                targetFOVOut, m_ZoomOutDuration)
                .SetEase(m_ZoomOutEase));
            
            zoomSequence.Append(DOTween.To(() => camera.m_Lens.FieldOfView, 
                x => camera.m_Lens.FieldOfView = x, 
                originalFOV, m_ZoomOutDuration * 0.5f)
                .SetEase(Ease.OutQuart));
            
            if (m_EnablePositionShake)
            {
                Vector3 originalPos = m_OriginalPositions[camera];
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-m_PositionShakeIntensity, m_PositionShakeIntensity),
                    Random.Range(-m_PositionShakeIntensity, m_PositionShakeIntensity),
                    0
                );
                
                zoomSequence.Join(DOTween.To(() => camera.transform.position, 
                    x => camera.transform.position = x, 
                    originalPos + shakeOffset, m_PositionShakeDuration)
                    .SetEase(Ease.OutQuart)
                    .OnComplete(() => camera.transform.position = originalPos));
            }
            
            m_ActiveZoomSequences[camera] = zoomSequence;
        }
        
        public void ShakeAllCameras(float intensity = -1f, float duration = -1f)
        {
            ZoomShakeAllCameras(m_ZoomInAmount, m_ZoomOutAmount, m_ZoomDuration);
        }
        
        public void ShakeCamera(CinemachineVirtualCamera camera, float intensity = -1f, float duration = -1f)
        {
            ZoomShakeCamera(camera, m_ZoomInAmount, m_ZoomOutAmount, m_ZoomDuration);
        }
        
        public void ShakeFOVOnly(CinemachineVirtualCamera camera, float intensity = -1f, float duration = -1f)
        {
            if (camera == null) return;
            
            float zoomAmount = intensity > 0 ? intensity : m_ZoomInAmount;
            float zoomDuration = duration > 0 ? duration : m_ZoomDuration;
            
            ZoomShakeCamera(camera, zoomAmount, zoomAmount * 1.2f, zoomDuration);
        }
        
        public void StopAllShakes()
        {
            foreach (var sequence in m_ActiveZoomSequences.Values)
            {
                if (sequence != null)
                {
                    sequence.Kill();
                }
            }
            m_ActiveZoomSequences.Clear();
            
            foreach (var camera in m_Cameras)
            {
                if (camera != null)
                {
                    camera.transform.position = m_OriginalPositions[camera];
                    camera.m_Lens.FieldOfView = m_OriginalFOVs[camera];
                }
            }
        }
        
        public void AddCamera(CinemachineVirtualCamera camera)
        {
            if (camera != null && !m_Cameras.Contains(camera))
            {
                m_Cameras.Add(camera);
                m_OriginalFOVs[camera] = camera.m_Lens.FieldOfView;
                m_OriginalPositions[camera] = camera.transform.position;
            }
        }
        
        public void RemoveCamera(CinemachineVirtualCamera camera)
        {
            if (camera != null && m_Cameras.Contains(camera))
            {
                m_Cameras.Remove(camera);
                m_OriginalFOVs.Remove(camera);
                m_OriginalPositions.Remove(camera);
                
                if (m_ActiveZoomSequences.ContainsKey(camera))
                {
                    if (m_ActiveZoomSequences[camera] != null)
                    {
                        m_ActiveZoomSequences[camera].Kill();
                    }
                    m_ActiveZoomSequences.Remove(camera);
                }
            }
        }
        
        public void SetZoomShakeSettings(float zoomInAmount, float zoomOutAmount, float duration)
        {
            m_ZoomInAmount = zoomInAmount;
            m_ZoomOutAmount = zoomOutAmount;
            m_ZoomDuration = duration;
        }
        
        public void SetZoomTimingSettings(float zoomInDuration, float zoomOutDuration)
        {
            m_ZoomInDuration = zoomInDuration;
            m_ZoomOutDuration = zoomOutDuration;
        }

        private void TriggerVibration()
        {
            if (VibrationManager.Instance != null)
            {
                VibrationManager.Instance.Vibrate();
            }
        }
        
        public void SetZoomEasing(Ease zoomInEase, Ease zoomOutEase)
        {
            m_ZoomInEase = zoomInEase;
            m_ZoomOutEase = zoomOutEase;
        }
        
        public void SetPositionShakeSettings(bool enable, float intensity, float duration)
        {
            m_EnablePositionShake = enable;
            m_PositionShakeIntensity = intensity;
            m_PositionShakeDuration = duration;
        }
        
        public void SetShakeSettings(float intensity, float duration, float frequency)
        {
            m_ZoomInAmount = intensity;
            m_ZoomOutAmount = intensity * 1.2f;
            m_ZoomDuration = duration;
        }
        
        public void SetFOVShakeSettings(float amount, float duration)
        {
            m_ZoomInAmount = amount;
            m_ZoomOutAmount = amount * 1.2f;
            m_ZoomDuration = duration;
        }
    }
}
