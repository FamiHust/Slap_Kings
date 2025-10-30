using UnityEngine;
using DG.Tweening;

namespace Duc.Managers
{
    public class SlowMotionManager : SingletonManager<SlowMotionManager>
    {
        [Header("Slow Motion Settings")]
        [SerializeField] private float m_SlowScale = 0.2f;
        [SerializeField] private float m_SlowDuration = 0.8f; 
        [SerializeField] private float m_RecoverDuration = 0.2f;
        
        [Header("Camera Zoom Settings")]
        [SerializeField] private bool m_EnableCameraZoom = true;
        [SerializeField] private float m_TargetFOV = 35f; 
        [SerializeField] private float m_TargetOrthoSize = 3.5f;
        [SerializeField] private float m_ZoomInDuration = 0.1f;
        [SerializeField] private float m_ZoomOutDuration = 0.2f;
        [SerializeField] private Ease m_ZoomEaseIn = Ease.OutCubic;
        [SerializeField] private Ease m_ZoomEaseOut = Ease.InCubic;

        [Header("Knockback Settings")] 
        [SerializeField] private float m_KnockbackDistance = 1.2f;
        [SerializeField] private float m_KnockbackDuration = 0.25f; 
        [SerializeField] private Ease m_KnockbackEase = Ease.OutCubic;

        private Tween m_TimeScaleTween;
        private Tween m_CameraZoomTween;
        private float m_DefaultFixedDeltaTime;
        private float m_DefaultFOV;
        private float m_DefaultOrthoSize;
        private Camera m_MainCam;

        protected override void OnInitialize()
        {
            m_DefaultFixedDeltaTime = Time.fixedDeltaTime;
            m_MainCam = Camera.main;
            if (m_MainCam != null)
            {
                if (m_MainCam.orthographic)
                {
                    m_DefaultOrthoSize = m_MainCam.orthographicSize;
                }
                else
                {
                    m_DefaultFOV = m_MainCam.fieldOfView;
                }
            }
        }

        protected override void OnCleanup()
        {
            if (m_TimeScaleTween != null && m_TimeScaleTween.IsActive())
            {
                m_TimeScaleTween.Kill(false);
            }
            RestoreTimeScaleImmediate();
            RestoreCameraZoomImmediate();
        }

        public void PlayPlayerLastHitSlowMotion()
        {
            PlaySlowMotion(m_SlowScale, m_SlowDuration, m_RecoverDuration, true);
        }

        public void PlaySlowMotion(float targetScale, float holdDuration, float recoverDuration, bool knockbackOnRecover)
        {
            if (m_TimeScaleTween != null && m_TimeScaleTween.IsActive())
            {
                m_TimeScaleTween.Kill(false);
            }

            DOTween.Kill("__TimeScaleTween");
            m_TimeScaleTween = DOTween.To(() => Time.timeScale, v => SetTimeScale(v), targetScale, 0.1f)
                .SetId("__TimeScaleTween")
                .SetUpdate(true);

            if (m_EnableCameraZoom)
            {
                ZoomCameraIn();
            }

            DOVirtual.DelayedCall(holdDuration, () =>
            {
                m_TimeScaleTween = DOTween.To(() => Time.timeScale, v => SetTimeScale(v), 1f, recoverDuration)
                    .SetId("__TimeScaleTween")
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        RestoreFixedDeltaTime();
                        if (m_EnableCameraZoom)
                        {
                            ZoomCameraOut();
                        }
                        if (knockbackOnRecover)
                        {
                            TryKnockbackAI();
                        }
                    });
            }).SetUpdate(true);
        }

        private void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0.01f, 1f);
            Time.fixedDeltaTime = m_DefaultFixedDeltaTime * Time.timeScale;
        }

        private void RestoreTimeScaleImmediate()
        {
            Time.timeScale = 1f;
            RestoreFixedDeltaTime();
        }

        private void ZoomCameraIn()
        {
            if (m_MainCam == null) m_MainCam = Camera.main;
            if (m_MainCam == null) return;

            if (m_CameraZoomTween != null && m_CameraZoomTween.IsActive())
            {
                m_CameraZoomTween.Kill(false);
            }

            if (m_MainCam.orthographic)
            {
                m_CameraZoomTween = DOTween.To(() => m_MainCam.orthographicSize, v => m_MainCam.orthographicSize = v, m_TargetOrthoSize, m_ZoomInDuration)
                    .SetEase(m_ZoomEaseIn)
                    .SetUpdate(true);
            }
            else
            {
                m_CameraZoomTween = DOTween.To(() => m_MainCam.fieldOfView, v => m_MainCam.fieldOfView = v, m_TargetFOV, m_ZoomInDuration)
                    .SetEase(m_ZoomEaseIn)
                    .SetUpdate(true);
            }
        }

        private void ZoomCameraOut()
        {
            if (m_MainCam == null) m_MainCam = Camera.main;
            if (m_MainCam == null) return;

            if (m_CameraZoomTween != null && m_CameraZoomTween.IsActive())
            {
                m_CameraZoomTween.Kill(false);
            }

            if (m_MainCam.orthographic)
            {
                m_CameraZoomTween = DOTween.To(() => m_MainCam.orthographicSize, v => m_MainCam.orthographicSize = v, m_DefaultOrthoSize, m_ZoomOutDuration)
                    .SetEase(m_ZoomEaseOut)
                    .SetUpdate(true);
            }
            else
            {
                m_CameraZoomTween = DOTween.To(() => m_MainCam.fieldOfView, v => m_MainCam.fieldOfView = v, m_DefaultFOV, m_ZoomOutDuration)
                    .SetEase(m_ZoomEaseOut)
                    .SetUpdate(true);
            }
        }

        private void RestoreCameraZoomImmediate()
        {
            if (m_MainCam == null) m_MainCam = Camera.main;
            if (m_MainCam == null) return;

            if (m_CameraZoomTween != null && m_CameraZoomTween.IsActive())
            {
                m_CameraZoomTween.Kill(false);
            }

            if (m_MainCam.orthographic)
            {
                m_MainCam.orthographicSize = (m_DefaultOrthoSize > 0f) ? m_DefaultOrthoSize : m_MainCam.orthographicSize;
            }
            else
            {
                m_MainCam.fieldOfView = (m_DefaultFOV > 0f) ? m_DefaultFOV : m_MainCam.fieldOfView;
            }
        }

        private void RestoreFixedDeltaTime()
        {
            Time.fixedDeltaTime = m_DefaultFixedDeltaTime;
        }

        private void TryKnockbackAI()
        {
            var ai = Object.FindObjectOfType<AIStateMachine>();
            if (ai == null) return;

            Transform aiTransform = ai.transform;
            Vector3 backward = -aiTransform.forward;
            Vector3 target = aiTransform.position + backward * m_KnockbackDistance;

            var rb = ai.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(backward * (m_KnockbackDistance * 10f), ForceMode.Impulse);
                return;
            }

            aiTransform.DOMove(target, m_KnockbackDuration)
                .SetEase(m_KnockbackEase)
                .SetUpdate(true);
        }
    }
}


