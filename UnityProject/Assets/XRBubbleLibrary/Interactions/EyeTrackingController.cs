#if FALSE // DISABLED: Missing dependencies - requires Input System package
using UnityEngine;
using UnityEngine.InputSystem;

namespace XRBubbleLibrary.Interactions
{
    /// <summary>
    /// Manages eye tracking input for advanced XR interactions.
    /// This system is designed to be forward-compatible with platforms
    /// that support eye tracking, such as future Meta Quest devices.
    /// </summary>
    public class EyeTrackingController : MonoBehaviour
    {
        [Header("Configuration")]
        public float maxGazeDistance = 10f;
        public LayerMask bubbleLayer;
        public float gazeDwellTime = 0.5f;

        private BubbleXRInteractable currentlyGazedBubble;
        private float gazeTimer;

        void Update()
        {
            if (Application.isEditor) return; // Eye tracking not available in editor

            ProcessEyeGaze();
        }

        private void ProcessEyeGaze()
        {
            Ray gazeRay = GetGazeRay();
            if (Physics.Raycast(gazeRay, out RaycastHit hit, maxGazeDistance, bubbleLayer))
            {
                BubbleXRInteractable bubble = hit.collider.GetComponent<BubbleXRInteractable>();
                if (bubble != null)
                {
                    if (bubble == currentlyGazedBubble)
                    {
                        gazeTimer += Time.deltaTime;
                        if (gazeTimer >= gazeDwellTime)
                        {
                            // Trigger gaze-based interaction
                            bubble.OnGazeSelect();
                            ResetGaze();
                        }
                    }
                    else
                    {
                        ResetGaze();
                        currentlyGazedBubble = bubble;
                    }
                }
                else
                {
                    ResetGaze();
                }
            }
            else
            {
                ResetGaze();
            }
        }

        private Ray GetGazeRay()
        {
            // This would use the platform's eye tracking API.
            // For now, we'll simulate it based on the camera's forward direction.
            return new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        }

        private void ResetGaze()
        {
            currentlyGazedBubble = null;
            gazeTimer = 0;
        }
    }
}
#endif // DISABLED: Missing dependencies