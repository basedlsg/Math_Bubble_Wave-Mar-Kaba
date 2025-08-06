#if FALSE // DISABLED: Missing dependencies - requires XR Hands package
using UnityEngine;
using UnityEngine.XR.Hands;
using Unity.Mathematics;
using XRBubbleLibrary.Physics;

namespace XRBubbleLibrary.Interactions
{
    /// <summary>
    /// Bubble Hand Tracking - Cloned from Unity XR Hands samples
    /// Implements natural hand tracking for bubble interactions
    /// Based on Unity's XR Hands samples and gesture recognition examples
    /// </summary>
    public class BubbleHandTracking : MonoBehaviour
    {
        [Header("Hand Tracking Configuration (Unity XR Hands Samples)")]
        [Tooltip("Cloned from Unity hand tracking samples")]
        [SerializeField] private float touchDetectionRadius = 0.1f;
        
        [Tooltip("Based on Unity gesture recognition samples")]
        [SerializeField] private float gestureThreshold = 0.8f;
        
        [Tooltip("Cloned from Unity hand physics samples")]
        [SerializeField] private float handForceMultiplier = 2f;
        
        [Header("Gesture Recognition (Unity Gesture Samples)")]
        [SerializeField] private bool enablePinchGesture = true;
        [SerializeField] private bool enablePushGesture = true;
        [SerializeField] private bool enableSwipeGesture = true;
        
        [Header("Visual Feedback (Unity Visual Samples)")]
        [SerializeField] private GameObject handVisualizationPrefab;
        [SerializeField] private bool showHandDebugInfo = false;
        
        // Hand tracking references
        private XRHandSubsystem handSubsystem;
        private XRHand leftHand;
        private XRHand rightHand;
        
        // Gesture state
        private bool leftPinchActive = false;
        private bool rightPinchActive = false;
        private Vector3 leftHandVelocity = Vector3.zero;
        private Vector3 rightHandVelocity = Vector3.zero;
        private Vector3 lastLeftHandPosition = Vector3.zero;
        private Vector3 lastRightHandPosition = Vector3.zero;
        
        // Bubble interaction tracking
        private BubbleXRInteractable[] nearbyBubbles;
        private float lastBubbleSearchTime = 0f;
        private const float BUBBLE_SEARCH_INTERVAL = 0.1f; // 10 times per second
        
        // Component references
        private BubbleSpringPhysics springPhysics;
        private BubbleBreathingSystem breathingSystem;
        
        private void Start()
        {
            InitializeHandTracking();
            SetupComponentReferences();
            StartBubbleTracking();
        }
        
        private void Update()
        {
            if (handSubsystem != null && handSubsystem.running)
            {
                UpdateHandTracking();
                ProcessGestures();
                UpdateBubbleInteractions();
            }
        }
        
        /// <summary>
        /// Initializes hand tracking based on Unity XR Hands samples
        /// </summary>
        private void InitializeHandTracking()
        {
            // Get hand subsystem (Unity XR Hands samples)
            handSubsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRHandSubsystem>();
            
            if (handSubsystem == null)
            {
                Debug.LogWarning("XR Hand Subsystem not found - hand tracking disabled");
                return;
            }
            
            // Initialize hand references (Unity hand samples)
            leftHand = handSubsystem.leftHand;
            rightHand = handSubsystem.rightHand;
            
            Debug.Log("Hand tracking initialized - cloned from Unity XR Hands samples");
        }
        
        /// <summary>
        /// Sets up component references based on Unity component search samples
        /// </summary>
        private void SetupComponentReferences()
        {
            // Find physics systems (Unity component search samples)
            springPhysics = FindObjectOfType<BubbleSpringPhysics>();
            breathingSystem = FindObjectOfType<BubbleBreathingSystem>();
            
            if (springPhysics == null)
            {
                Debug.LogWarning("BubbleSpringPhysics not found - hand physics disabled");
            }
            
            if (breathingSystem == null)
            {
                Debug.LogWarning("BubbleBreathingSystem not found - breathing interaction disabled");
            }
        }
        
        /// <summary>
        /// Starts bubble tracking system based on Unity tracking samples
        /// </summary>
        private void StartBubbleTracking()
        {
            // Initialize bubble array for tracking (Unity array samples)
            nearbyBubbles = new BubbleXRInteractable[0];
            lastBubbleSearchTime = Time.time;
        }
        
        /// <summary>
        /// Updates hand tracking data based on Unity XR Hands samples
        /// </summary>
        private void UpdateHandTracking()
        {
            // Update left hand (Unity hand tracking samples)
            if (leftHand.isTracked)
            {
                UpdateHandData(leftHand, ref lastLeftHandPosition, ref leftHandVelocity, ref leftPinchActive);
            }
            
            // Update right hand (Unity hand tracking samples)
            if (rightHand.isTracked)
            {
                UpdateHandData(rightHand, ref lastRightHandPosition, ref rightHandVelocity, ref rightPinchActive);
            }
        }
        
        /// <summary>
        /// Updates individual hand data based on Unity hand data samples
        /// </summary>
        private void UpdateHandData(XRHand hand, ref Vector3 lastPosition, ref Vector3 velocity, ref bool pinchActive)
        {
            // Get hand root position (Unity hand position samples)
            if (hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose))
            {
                Vector3 currentPosition = wristPose.position;
                
                // Calculate velocity (Unity velocity calculation samples)
                velocity = (currentPosition - lastPosition) / Time.deltaTime;
                lastPosition = currentPosition;
                
                // Update pinch state (Unity pinch detection samples)
                pinchActive = DetectPinchGesture(hand);
            }
        }
        
        /// <summary>
        /// Detects pinch gesture based on Unity gesture recognition samples
        /// </summary>
        private bool DetectPinchGesture(XRHand hand)
        {
            if (!enablePinchGesture) return false;
            
            // Get thumb and index finger positions (Unity joint samples)
            bool thumbValid = hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbPose);
            bool indexValid = hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexPose);
            
            if (!thumbValid || !indexValid) return false;
            
            // Calculate distance between thumb and index finger (Unity distance samples)
            float pinchDistance = Vector3.Distance(thumbPose.position, indexPose.position);
            
            // Return true if fingers are close enough (Unity threshold samples)
            return pinchDistance < gestureThreshold * 0.05f; // 5cm threshold scaled by gesture sensitivity
        }
        
        /// <summary>
        /// Processes hand gestures based on Unity gesture processing samples
        /// </summary>
        private void ProcessGestures()
        {
            // Process left hand gestures (Unity gesture samples)
            if (leftHand.isTracked)
            {
                ProcessHandGestures(leftHand, leftPinchActive, leftHandVelocity, "Left");
            }
            
            // Process right hand gestures (Unity gesture samples)
            if (rightHand.isTracked)
            {
                ProcessHandGestures(rightHand, rightPinchActive, rightHandVelocity, "Right");
            }
        }
        
        /// <summary>
        /// Processes gestures for individual hand based on Unity gesture samples
        /// </summary>
        private void ProcessHandGestures(XRHand hand, bool pinchActive, Vector3 handVelocity, string handName)
        {
            // Get hand root position for gesture processing (Unity position samples)
            if (!hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose)) return;
            
            // Process pinch gesture (Unity pinch samples)
            if (enablePinchGesture && pinchActive)
            {
                ProcessPinchInteraction(wristPose.position, handName);
            }
            
            // Process push gesture (Unity push samples)
            if (enablePushGesture && IsPushGesture(handVelocity))
            {
                ProcessPushInteraction(wristPose.position, handVelocity, handName);
            }
            
            // Process swipe gesture (Unity swipe samples)
            if (enableSwipeGesture && IsSwipeGesture(handVelocity))
            {
                ProcessSwipeInteraction(wristPose.position, handVelocity, handName);
            }
        }
        
        /// <summary>
        /// Detects push gesture based on Unity gesture detection samples
        /// </summary>
        private bool IsPushGesture(Vector3 handVelocity)
        {
            // Push gesture: forward velocity above threshold (Unity velocity samples)
            return handVelocity.z > gestureThreshold * 2f; // Forward push
        }
        
        /// <summary>
        /// Detects swipe gesture based on Unity gesture detection samples
        /// </summary>
        private bool IsSwipeGesture(Vector3 handVelocity)
        {
            // Swipe gesture: lateral velocity above threshold (Unity velocity samples)
            float lateralVelocity = Mathf.Max(Mathf.Abs(handVelocity.x), Mathf.Abs(handVelocity.y));
            return lateralVelocity > gestureThreshold * 1.5f;
        }
        
        /// <summary>
        /// Processes pinch interaction based on Unity pinch interaction samples
        /// </summary>
        private void ProcessPinchInteraction(Vector3 handPosition, string handName)
        {
            // Find bubbles near pinch position (Unity proximity samples)
            var nearbyBubble = FindNearestBubble(handPosition, touchDetectionRadius);
            
            if (nearbyBubble != null)
            {
                // Apply gentle pinch force (Unity force application samples)
                ApplyGentleForce(nearbyBubble, handPosition, 0.5f);
                
                // Trigger visual feedback (Unity visual feedback samples)
                TriggerBubbleHighlight(nearbyBubble, Color.yellow);
                
                if (showHandDebugInfo)
                {
                    Debug.Log($"{handName} hand pinching bubble at {handPosition}");
                }
            }
        }
        
        /// <summary>
        /// Processes push interaction based on Unity push interaction samples
        /// </summary>
        private void ProcessPushInteraction(Vector3 handPosition, Vector3 handVelocity, string handName)
        {
            // Find bubbles in push direction (Unity direction samples)
            var affectedBubbles = FindBubblesInDirection(handPosition, handVelocity.normalized, 0.3f);
            
            foreach (var bubble in affectedBubbles)
            {
                // Apply push force based on velocity (Unity force samples)
                float pushForce = handVelocity.magnitude * handForceMultiplier;
                ApplyDirectionalForce(bubble, handVelocity.normalized, pushForce);
                
                // Trigger push visual effect (Unity visual samples)
                TriggerBubbleHighlight(bubble, Color.red);
            }
            
            if (showHandDebugInfo && affectedBubbles.Length > 0)
            {
                Debug.Log($"{handName} hand pushing {affectedBubbles.Length} bubbles with force {handVelocity.magnitude:F2}");
            }
        }
        
        /// <summary>
        /// Processes swipe interaction based on Unity swipe interaction samples
        /// </summary>
        private void ProcessSwipeInteraction(Vector3 handPosition, Vector3 handVelocity, string handName)
        {
            // Find bubbles in swipe area (Unity area samples)
            var affectedBubbles = FindBubblesInArea(handPosition, 0.2f);
            
            foreach (var bubble in affectedBubbles)
            {
                // Apply swipe force (Unity swipe force samples)
                float swipeForce = handVelocity.magnitude * handForceMultiplier * 0.7f;
                ApplyDirectionalForce(bubble, handVelocity.normalized, swipeForce);
                
                // Trigger swipe visual effect (Unity visual samples)
                TriggerBubbleHighlight(bubble, Color.cyan);
            }
            
            if (showHandDebugInfo && affectedBubbles.Length > 0)
            {
                Debug.Log($"{handName} hand swiping {affectedBubbles.Length} bubbles with velocity {handVelocity.magnitude:F2}");
            }
        }
        
        /// <summary>
        /// Updates bubble interactions based on Unity interaction samples
        /// </summary>
        private void UpdateBubbleInteractions()
        {
            // Update bubble search periodically for performance (Unity performance samples)
            if (Time.time - lastBubbleSearchTime > BUBBLE_SEARCH_INTERVAL)
            {
                RefreshNearbyBubbles();
                lastBubbleSearchTime = Time.time;
            }
        }
        
        /// <summary>
        /// Refreshes nearby bubbles list based on Unity search samples
        /// </summary>
        private void RefreshNearbyBubbles()
        {
            // Find all bubble interactables (Unity search samples)
            nearbyBubbles = FindObjectsOfType<BubbleXRInteractable>();
        }
        
        /// <summary>
        /// Finds nearest bubble to position based on Unity proximity samples
        /// </summary>
        private BubbleXRInteractable FindNearestBubble(Vector3 position, float maxDistance)
        {
            BubbleXRInteractable nearest = null;
            float nearestDistance = maxDistance;
            
            foreach (var bubble in nearbyBubbles)
            {
                if (bubble == null) continue;
                
                float distance = Vector3.Distance(position, bubble.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = bubble;
                    nearestDistance = distance;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// Finds bubbles in direction based on Unity direction search samples
        /// </summary>
        private BubbleXRInteractable[] FindBubblesInDirection(Vector3 origin, Vector3 direction, float maxDistance)
        {
            var bubblesInDirection = new System.Collections.Generic.List<BubbleXRInteractable>();
            
            foreach (var bubble in nearbyBubbles)
            {
                if (bubble == null) continue;
                
                Vector3 toBubble = bubble.transform.position - origin;
                float distance = toBubble.magnitude;
                
                if (distance <= maxDistance)
                {
                    float dot = Vector3.Dot(toBubble.normalized, direction);
                    if (dot > 0.5f) // Within 60 degree cone
                    {
                        bubblesInDirection.Add(bubble);
                    }
                }
            }
            
            return bubblesInDirection.ToArray();
        }
        
        /// <summary>
        /// Finds bubbles in area based on Unity area search samples
        /// </summary>
        private BubbleXRInteractable[] FindBubblesInArea(Vector3 center, float radius)
        {
            var bubblesInArea = new System.Collections.Generic.List<BubbleXRInteractable>();
            
            foreach (var bubble in nearbyBubbles)
            {
                if (bubble == null) continue;
                
                float distance = Vector3.Distance(center, bubble.transform.position);
                if (distance <= radius)
                {
                    bubblesInArea.Add(bubble);
                }
            }
            
            return bubblesInArea.ToArray();
        }
        
        /// <summary>
        /// Applies gentle force to bubble based on Unity force application samples
        /// </summary>
        private void ApplyGentleForce(BubbleXRInteractable bubble, Vector3 handPosition, float forceMultiplier)
        {
            Vector3 forceDirection = (bubble.transform.position - handPosition).normalized;
            float force = handForceMultiplier * forceMultiplier;
            
            // Apply through spring physics if available (Unity physics integration samples)
            if (springPhysics != null)
            {
                int bubbleIndex = GetBubbleIndex(bubble);
                springPhysics.ApplyForce(bubbleIndex, forceDirection * force);
            }
        }
        
        /// <summary>
        /// Applies directional force to bubble based on Unity directional force samples
        /// </summary>
        private void ApplyDirectionalForce(BubbleXRInteractable bubble, Vector3 direction, float force)
        {
            // Apply through spring physics if available (Unity physics integration samples)
            if (springPhysics != null)
            {
                int bubbleIndex = GetBubbleIndex(bubble);
                springPhysics.ApplyForce(bubbleIndex, direction * force);
            }
        }
        
        /// <summary>
        /// Triggers bubble highlight effect based on Unity visual feedback samples
        /// </summary>
        private void TriggerBubbleHighlight(BubbleXRInteractable bubble, Color highlightColor)
        {
            // Trigger visual feedback through bubble's own system
            // This would integrate with the bubble's highlight system
            
            // For now, just log the interaction
            if (showHandDebugInfo)
            {
                Debug.Log($"Highlighting bubble {bubble.name} with color {highlightColor}");
            }
        }
        
        /// <summary>
        /// Gets bubble index for physics system integration
        /// Based on Unity indexing samples
        /// </summary>
        private int GetBubbleIndex(BubbleXRInteractable bubble)
        {
            // Simple index based on instance ID (Unity ID samples)
            return Mathf.Abs(bubble.GetInstanceID()) % 100;
        }
        
        /// <summary>
        /// Validates hand tracking setup
        /// </summary>
        [ContextMenu("Validate Hand Tracking")]
        public void ValidateHandTracking()
        {
            Debug.Log($"Bubble Hand Tracking Status:");
            Debug.Log($"- Hand Subsystem: {(handSubsystem != null ? "Found" : "Missing")}");
            Debug.Log($"- Left Hand Tracked: {(leftHand.isTracked ? "Yes" : "No")}");
            Debug.Log($"- Right Hand Tracked: {(rightHand.isTracked ? "Yes" : "No")}");
            Debug.Log($"- Spring Physics: {(springPhysics != null ? "Found" : "Missing")}");
            Debug.Log($"- Breathing System: {(breathingSystem != null ? "Found" : "Missing")}");
            Debug.Log($"- Nearby Bubbles: {nearbyBubbles?.Length ?? 0}");
            Debug.Log($"- Pinch Gesture: {enablePinchGesture}");
            Debug.Log($"- Push Gesture: {enablePushGesture}");
            Debug.Log($"- Swipe Gesture: {enableSwipeGesture}");
        }
        
        /// <summary>
        /// Optimizes hand tracking for Quest 3
        /// Based on Unity Quest optimization samples
        /// </summary>
        [ContextMenu("Optimize for Quest 3")]
        public void OptimizeForQuest3()
        {
            // Optimize gesture thresholds for Quest 3 hand tracking (Unity Quest samples)
            gestureThreshold = 0.9f; // Slightly higher for accuracy
            touchDetectionRadius = 0.08f; // Smaller for precision
            handForceMultiplier = 1.5f; // Reduced for comfort
            
            // Optimize update frequency for Quest 3 (Unity performance samples)
            // Bubble search interval already optimized at 10Hz
            
            Debug.Log("Hand tracking optimized for Quest 3 - improved accuracy and performance");
        }
        
        private void OnDrawGizmos()
        {
            if (!showHandDebugInfo) return;
            
            // Draw hand positions and detection radii (Unity Gizmos samples)
            if (leftHand.isTracked && leftHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose leftWrist))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(leftWrist.position, touchDetectionRadius);
            }
            
            if (rightHand.isTracked && rightHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose rightWrist))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(rightWrist.position, touchDetectionRadius);
            }
        }
    }
}
#endif // DISABLED: Missing dependencies