using UnityEngine;
using XRBubbleLibrary.UI;

namespace XRBubbleLibrary.Physics
{
    /// <summary>
    /// Pooled bubble component for object pool management
    /// Cloned from Unity pooling patterns, optimized for bubble lifecycle
    /// </summary>
    [RequireComponent(typeof(EnhancedBubblePhysics))]
    public class PooledBubble : MonoBehaviour
    {
        [Header("Pool Management")]
        [SerializeField] private int bubbleId;
        [SerializeField] private int currentLODLevel = 0;
        
        [Header("Lifecycle Tracking")]
        [SerializeField] private float activeTime = 0.0f;
        [SerializeField] private float inactiveTime = 0.0f;
        [SerializeField] private bool isCurrentlyActive = false;
        
        // Component references
        private BubbleObjectPool parentPool;
        private EnhancedBubblePhysics bubblePhysics;
        private BubbleUIElement uiElement;
        private ParticleSystem breathingParticles;
        private Renderer bubbleRenderer;
        private Collider bubbleCollider;
        
        // State tracking
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Quaternion originalRotation;
        private float lastInteractionTime;
        private bool wasInteractedWith = false;
        
        // Performance optimization
        private float lastVisibilityCheck = 0.0f;
        private const float visibilityCheckInterval = 0.5f; // Check visibility twice per second
        
        void Awake()
        {
            CacheComponents();
            StoreOriginalTransform();
        }
        
        void Update()
        {
            if (isCurrentlyActive)
            {
                activeTime += Time.deltaTime;
                
                // Check if bubble has been inactive (no interaction) for too long
                if (Time.time - lastInteractionTime > 1.0f)
                {
                    inactiveTime += Time.deltaTime;
                }
                else
                {
                    inactiveTime = 0.0f;
                }
                
                // Periodic visibility check for performance
                if (Time.time - lastVisibilityCheck >= visibilityCheckInterval)
                {
                    CheckVisibility();
                    lastVisibilityCheck = Time.time;
                }
            }
        }
        
        /// <summary>
        /// Cache component references for performance
        /// </summary>
        void CacheComponents()
        {
            bubblePhysics = GetComponent<EnhancedBubblePhysics>();
            uiElement = GetComponent<BubbleUIElement>();
            breathingParticles = GetComponent<ParticleSystem>();
            bubbleRenderer = GetComponent<Renderer>();
            bubbleCollider = GetComponent<Collider>();
            
            // If components don't exist, try to find them in children
            if (bubbleRenderer == null)
                bubbleRenderer = GetComponentInChildren<Renderer>();
            if (bubbleCollider == null)
                bubbleCollider = GetComponentInChildren<Collider>();
            if (breathingParticles == null)
                breathingParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        /// <summary>
        /// Store original transform for reset purposes
        /// </summary>
        void StoreOriginalTransform()
        {
            originalPosition = transform.localPosition;
            originalScale = transform.localScale;
            originalRotation = transform.localRotation;
        }
        
        /// <summary>
        /// Initialize bubble with pool reference and ID
        /// </summary>
        public void Initialize(BubbleObjectPool pool, int id)
        {
            parentPool = pool;
            bubbleId = id;
            gameObject.name = $"PooledBubble_{id}";
        }
        
        /// <summary>
        /// Called when bubble is retrieved from pool
        /// </summary>
        public void OnGetFromPool()
        {
            isCurrentlyActive = true;
            activeTime = 0.0f;
            inactiveTime = 0.0f;
            lastInteractionTime = Time.time;
            wasInteractedWith = false;
            
            // Reset transform
            transform.localPosition = originalPosition;
            transform.localScale = originalScale;
            transform.localRotation = originalRotation;
            
            // Enable components
            EnableComponents(true);
            
            // Reset physics state
            if (bubblePhysics != null)
            {
                bubblePhysics.enabled = true;
            }
            
            // Reset UI state
            if (uiElement != null)
            {
                uiElement.enabled = true;
            }
            
            // Start particle effects
            if (breathingParticles != null)
            {
                breathingParticles.Play();
            }
        }
        
        /// <summary>
        /// Called when bubble is returned to pool
        /// </summary>
        public void OnReturnToPool()
        {
            isCurrentlyActive = false;
            
            // Disable components for performance
            EnableComponents(false);
            
            // Stop particle effects
            if (breathingParticles != null)
            {
                breathingParticles.Stop();
                breathingParticles.Clear();
            }
            
            // Reset physics state
            if (bubblePhysics != null)
            {
                bubblePhysics.enabled = false;
            }
            
            // Reset UI state
            if (uiElement != null)
            {
                uiElement.Cleanup();
                uiElement.enabled = false;
            }
            
            // Reset transform
            transform.localPosition = originalPosition;
            transform.localScale = originalScale;
            transform.localRotation = originalRotation;
        }
        
        /// <summary>
        /// Enable/disable components for performance
        /// </summary>
        void EnableComponents(bool enabled)
        {
            if (bubbleRenderer != null)
                bubbleRenderer.enabled = enabled;
            
            if (bubbleCollider != null)
                bubbleCollider.enabled = enabled;
        }
        
        /// <summary>
        /// Set LOD level for performance optimization
        /// </summary>
        public void SetLODLevel(int lodLevel)
        {
            currentLODLevel = lodLevel;
            
            // Apply LOD settings to components
            if (bubblePhysics != null)
            {
                // Adjust physics update frequency based on LOD
                switch (lodLevel)
                {
                    case 0: // High quality - full physics
                        bubblePhysics.enabled = true;
                        break;
                    case 1: // Medium quality - reduced physics
                        bubblePhysics.enabled = true;
                        // Could reduce update frequency here
                        break;
                    case 2: // Low quality - minimal physics
                        bubblePhysics.enabled = false;
                        break;
                }
            }
            
            if (uiElement != null)
            {
                uiElement.SetLODLevel(lodLevel);
            }
            
            if (breathingParticles != null)
            {
                var emission = breathingParticles.emission;
                switch (lodLevel)
                {
                    case 0:
                        emission.enabled = true;
                        emission.rateOverTime = 10.0f;
                        break;
                    case 1:
                        emission.enabled = true;
                        emission.rateOverTime = 5.0f;
                        break;
                    case 2:
                        emission.enabled = false;
                        break;
                }
            }
        }
        
        /// <summary>
        /// Check visibility and adjust performance accordingly
        /// </summary>
        void CheckVisibility()
        {
            if (bubbleRenderer == null) return;
            
            bool isVisible = bubbleRenderer.isVisible;
            
            // Adjust performance based on visibility
            if (bubblePhysics != null)
            {
                bubblePhysics.SetVisibility(isVisible);
            }
            
            // Disable particle effects if not visible
            if (breathingParticles != null && !isVisible)
            {
                if (breathingParticles.isPlaying)
                {
                    breathingParticles.Pause();
                }
            }
            else if (breathingParticles != null && isVisible)
            {
                if (breathingParticles.isPaused)
                {
                    breathingParticles.Play();
                }
            }
        }
        
        /// <summary>
        /// Handle bubble interaction
        /// </summary>
        public void OnInteraction()
        {
            lastInteractionTime = Time.time;
            wasInteractedWith = true;
            inactiveTime = 0.0f;
        }
        
        /// <summary>
        /// Return this bubble to the pool
        /// </summary>
        public void ReturnToPool()
        {
            if (parentPool != null)
            {
                parentPool.ReturnBubble(this);
            }
        }
        
        /// <summary>
        /// Get bubble ID
        /// </summary>
        public int GetId()
        {
            return bubbleId;
        }
        
        /// <summary>
        /// Get current LOD level
        /// </summary>
        public int GetLODLevel()
        {
            return currentLODLevel;
        }
        
        /// <summary>
        /// Get active time
        /// </summary>
        public float GetActiveTime()
        {
            return activeTime;
        }
        
        /// <summary>
        /// Get inactive time
        /// </summary>
        public float GetInactiveTime()
        {
            return inactiveTime;
        }
        
        /// <summary>
        /// Check if bubble is currently active
        /// </summary>
        public bool IsActive()
        {
            return isCurrentlyActive;
        }
        
        /// <summary>
        /// Check if bubble was interacted with
        /// </summary>
        public bool WasInteractedWith()
        {
            return wasInteractedWith;
        }
        
        /// <summary>
        /// Get distance from camera for LOD calculations
        /// </summary>
        public float GetDistanceFromCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                return Vector3.Distance(transform.position, mainCamera.transform.position);
            }
            return float.MaxValue;
        }
        
        /// <summary>
        /// Force update LOD based on distance
        /// </summary>
        public void UpdateLODByDistance(float maxDistance)
        {
            float distance = GetDistanceFromCamera();
            float normalizedDistance = distance / maxDistance;
            
            int newLODLevel;
            if (normalizedDistance < 0.3f)
                newLODLevel = 0; // High quality
            else if (normalizedDistance < 0.6f)
                newLODLevel = 1; // Medium quality
            else
                newLODLevel = 2; // Low quality
            
            if (newLODLevel != currentLODLevel)
            {
                SetLODLevel(newLODLevel);
            }
        }
        
        /// <summary>
        /// Get bubble performance metrics
        /// </summary>
        public BubbleMetrics GetMetrics()
        {
            return new BubbleMetrics
            {
                id = bubbleId,
                activeTime = activeTime,
                inactiveTime = inactiveTime,
                lodLevel = currentLODLevel,
                distanceFromCamera = GetDistanceFromCamera(),
                wasInteractedWith = wasInteractedWith,
                isVisible = bubbleRenderer != null ? bubbleRenderer.isVisible : false
            };
        }
        
        void OnTriggerEnter(Collider other)
        {
            // Handle XR interaction
            if (other.CompareTag("Hand") || other.CompareTag("Controller"))
            {
                OnInteraction();
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw bubble info
            Gizmos.color = isCurrentlyActive ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            
            // Draw LOD level
            Gizmos.color = currentLODLevel == 0 ? Color.green : 
                          currentLODLevel == 1 ? Color.yellow : Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.2f, Vector3.one * 0.05f);
        }
    }
    
    /// <summary>
    /// Performance metrics for individual bubbles
    /// </summary>
    [System.Serializable]
    public struct BubbleMetrics
    {
        public int id;
        public float activeTime;
        public float inactiveTime;
        public int lodLevel;
        public float distanceFromCamera;
        public bool wasInteractedWith;
        public bool isVisible;
        
        public override string ToString()
        {
            return $"Bubble {id} - Active: {activeTime:F1}s, LOD: {lodLevel}, " +
                   $"Distance: {distanceFromCamera:F1}m, Interacted: {wasInteractedWith}";
        }
    }
}