using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using XRBubbleLibrary.Physics;

namespace XRBubbleLibrary.Performance
{
    /// <summary>
    /// LOD (Level of Detail) management system for bubble performance optimization
    /// Cloned from Unity LOD Group samples, optimized for Quest 3 performance targets
    /// </summary>
    public class BubbleLODManager : MonoBehaviour
    {
        [Header("LOD Distance Settings")]
        [Range(1.0f, 10.0f)]
        public float maxRenderDistance = 8.0f;
        
        [Range(0.1f, 1.0f)]
        public float highQualityDistance = 0.3f; // 30% of max distance
        
        [Range(0.3f, 0.8f)]
        public float mediumQualityDistance = 0.6f; // 60% of max distance
        
        [Header("Performance Targets")]
        [Range(60, 120)]
        public int targetFrameRate = 72; // Quest 3 target
        
        [Range(100, 500)]
        public int maxActiveBubbles = 200;
        
        [Range(50, 200)]
        public int maxHighQualityBubbles = 50;
        
        [Header("Batch Processing")]
        public bool enableBatchProcessing = true;
        
        [Range(5, 50)]
        public int bubblesPerBatch = 20;
        
        [Range(0.016f, 0.1f)]
        public float batchProcessingInterval = 0.033f; // ~30 FPS for LOD updates
        
        [Header("Culling Settings")]
        public bool enableFrustumCulling = true;
        public bool enableOcclusionCulling = false;
        public LayerMask cullingLayers = -1;
        
        // Component references
        private Camera mainCamera;
        private Transform cameraTransform;
        
        // Bubble tracking
        private List<PooledBubble> allBubbles = new List<PooledBubble>();
        private List<PooledBubble> visibleBubbles = new List<PooledBubble>();
        private Dictionary<int, LODBubbleData> bubbleLODData = new Dictionary<int, LODBubbleData>();
        
        // Batch processing
        private Coroutine batchProcessingCoroutine;
        private int currentBatchIndex = 0;
        
        // Performance monitoring
        private float lastFrameTime = 0.0f;
        private float averageFrameTime = 0.016f; // Start with 60 FPS assumption
        private int frameCount = 0;
        private float performanceCheckInterval = 1.0f;
        private float lastPerformanceCheck = 0.0f;
        
        // Culling
        private Plane[] frustumPlanes = new Plane[6];
        private float lastCullingUpdate = 0.0f;
        private const float cullingUpdateInterval = 0.1f; // Update culling 10 times per second
        
        // Singleton pattern
        private static BubbleLODManager instance;
        public static BubbleLODManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<BubbleLODManager>();
                    if (instance == null)
                    {
                        GameObject lodManager = new GameObject("BubbleLODManager");
                        instance = lodManager.AddComponent<BubbleLODManager>();
                    }
                }
                return instance;
            }
        }
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeLODManager();
        }
        
        void Update()
        {
            UpdatePerformanceMetrics();
            
            if (enableFrustumCulling && Time.time - lastCullingUpdate >= cullingUpdateInterval)
            {
                UpdateFrustumCulling();
                lastCullingUpdate = Time.time;
            }
        }
        
        /// <summary>
        /// Initialize the LOD management system
        /// Based on cloned Unity LOD samples
        /// </summary>
        void InitializeLODManager()
        {
            // Find main camera
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
            
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
            else
            {
                Debug.LogWarning("No camera found for LOD calculations!");
            }
            
            // Start batch processing
            if (enableBatchProcessing)
            {
                StartBatchProcessing();
            }
            
            Debug.Log("BubbleLODManager initialized");
        }
        
        /// <summary>
        /// Register a bubble for LOD management
        /// </summary>
        public void RegisterBubble(PooledBubble bubble)
        {
            if (bubble == null || allBubbles.Contains(bubble)) return;
            
            allBubbles.Add(bubble);
            
            // Initialize LOD data
            LODBubbleData lodData = new LODBubbleData
            {
                bubble = bubble,
                lastDistance = float.MaxValue,
                lastLODLevel = -1,
                isVisible = false,
                lastUpdateTime = Time.time
            };
            
            bubbleLODData[bubble.GetId()] = lodData;
            
            // Initial LOD calculation
            UpdateBubbleLOD(bubble);
        }
        
        /// <summary>
        /// Unregister a bubble from LOD management
        /// </summary>
        public void UnregisterBubble(PooledBubble bubble)
        {
            if (bubble == null) return;
            
            allBubbles.Remove(bubble);
            visibleBubbles.Remove(bubble);
            bubbleLODData.Remove(bubble.GetId());
        }
        
        /// <summary>
        /// Start batch processing coroutine
        /// </summary>
        void StartBatchProcessing()
        {
            if (batchProcessingCoroutine != null)
            {
                StopCoroutine(batchProcessingCoroutine);
            }
            
            batchProcessingCoroutine = StartCoroutine(BatchProcessBubbles());
        }
        
        /// <summary>
        /// Batch process bubbles for performance optimization
        /// Based on cloned Unity Job System patterns
        /// </summary>
        IEnumerator BatchProcessBubbles()
        {
            while (true)
            {
                if (allBubbles.Count > 0)
                {
                    int startIndex = currentBatchIndex;
                    int endIndex = Mathf.Min(startIndex + bubblesPerBatch, allBubbles.Count);
                    
                    // Process batch of bubbles
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        if (i < allBubbles.Count && allBubbles[i] != null)
                        {
                            UpdateBubbleLOD(allBubbles[i]);
                        }
                    }
                    
                    // Update batch index
                    currentBatchIndex = endIndex;
                    if (currentBatchIndex >= allBubbles.Count)
                    {
                        currentBatchIndex = 0;
                    }
                }
                
                yield return new WaitForSeconds(batchProcessingInterval);
            }
        }
        
        /// <summary>
        /// Update LOD for a specific bubble
        /// </summary>
        void UpdateBubbleLOD(PooledBubble bubble)
        {
            if (bubble == null || cameraTransform == null) return;
            
            int bubbleId = bubble.GetId();
            if (!bubbleLODData.TryGetValue(bubbleId, out LODBubbleData lodData))
                return;
            
            // Calculate distance from camera
            float distance = Vector3.Distance(bubble.transform.position, cameraTransform.position);
            lodData.lastDistance = distance;
            
            // Determine LOD level based on distance
            int newLODLevel = CalculateLODLevel(distance);
            
            // Check if bubble is within render distance
            bool shouldBeVisible = distance <= maxRenderDistance;
            
            // Apply frustum culling if enabled
            if (shouldBeVisible && enableFrustumCulling)
            {
                shouldBeVisible = IsInCameraFrustum(bubble.transform.position);
            }
            
            // Update visibility
            if (lodData.isVisible != shouldBeVisible)
            {
                lodData.isVisible = shouldBeVisible;
                
                if (shouldBeVisible)
                {
                    if (!visibleBubbles.Contains(bubble))
                        visibleBubbles.Add(bubble);
                }
                else
                {
                    visibleBubbles.Remove(bubble);
                }
                
                // Set bubble visibility
                bubble.SetLODLevel(shouldBeVisible ? newLODLevel : -1); // -1 means invisible
            }
            
            // Update LOD level if changed
            if (shouldBeVisible && lodData.lastLODLevel != newLODLevel)
            {
                lodData.lastLODLevel = newLODLevel;
                bubble.SetLODLevel(newLODLevel);
            }
            
            lodData.lastUpdateTime = Time.time;
            bubbleLODData[bubbleId] = lodData;
        }
        
        /// <summary>
        /// Calculate LOD level based on distance
        /// </summary>
        int CalculateLODLevel(float distance)
        {
            float normalizedDistance = distance / maxRenderDistance;
            
            if (normalizedDistance <= highQualityDistance)
                return 0; // High quality
            else if (normalizedDistance <= mediumQualityDistance)
                return 1; // Medium quality
            else
                return 2; // Low quality
        }
        
        /// <summary>
        /// Check if position is within camera frustum
        /// </summary>
        bool IsInCameraFrustum(Vector3 position)
        {
            if (mainCamera == null) return true;
            
            // Check against all frustum planes
            for (int i = 0; i < frustumPlanes.Length; i++)
            {
                if (frustumPlanes[i].GetDistanceToPoint(position) < 0)
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Update frustum culling planes
        /// </summary>
        void UpdateFrustumCulling()
        {
            if (mainCamera == null) return;
            
            GeometryUtility.CalculateFrustumPlanes(mainCamera, frustumPlanes);
        }
        
        /// <summary>
        /// Update performance metrics and adjust quality
        /// </summary>
        void UpdatePerformanceMetrics()
        {
            frameCount++;
            float currentFrameTime = Time.unscaledDeltaTime;
            
            // Calculate moving average of frame time
            averageFrameTime = Mathf.Lerp(averageFrameTime, currentFrameTime, 0.1f);
            
            // Check performance periodically
            if (Time.time - lastPerformanceCheck >= performanceCheckInterval)
            {
                float currentFPS = 1.0f / averageFrameTime;
                
                // Adjust quality based on performance
                if (currentFPS < targetFrameRate * 0.9f) // 10% below target
                {
                    ReduceQuality();
                }
                else if (currentFPS > targetFrameRate * 1.1f) // 10% above target
                {
                    IncreaseQuality();
                }
                
                lastPerformanceCheck = Time.time;
                frameCount = 0;
            }
        }
        
        /// <summary>
        /// Reduce quality to improve performance
        /// </summary>
        void ReduceQuality()
        {
            // Reduce max render distance
            maxRenderDistance = Mathf.Max(maxRenderDistance * 0.9f, 3.0f);
            
            // Reduce high quality distance
            highQualityDistance = Mathf.Max(highQualityDistance * 0.9f, 0.1f);
            
            // Reduce max active bubbles
            maxActiveBubbles = Mathf.Max(maxActiveBubbles - 10, 50);
            
            // Increase batch processing interval (slower updates)
            batchProcessingInterval = Mathf.Min(batchProcessingInterval * 1.1f, 0.1f);
            
            Debug.Log($"Reduced quality - Max Distance: {maxRenderDistance:F1}, Max Bubbles: {maxActiveBubbles}");
        }
        
        /// <summary>
        /// Increase quality when performance allows
        /// </summary>
        void IncreaseQuality()
        {
            // Increase max render distance
            maxRenderDistance = Mathf.Min(maxRenderDistance * 1.05f, 10.0f);
            
            // Increase high quality distance
            highQualityDistance = Mathf.Min(highQualityDistance * 1.05f, 0.5f);
            
            // Increase max active bubbles
            maxActiveBubbles = Mathf.Min(maxActiveBubbles + 5, 300);
            
            // Decrease batch processing interval (faster updates)
            batchProcessingInterval = Mathf.Max(batchProcessingInterval * 0.95f, 0.016f);
        }
        
        /// <summary>
        /// Force update all bubble LODs immediately
        /// </summary>
        public void ForceUpdateAllLODs()
        {
            foreach (var bubble in allBubbles)
            {
                if (bubble != null)
                {
                    UpdateBubbleLOD(bubble);
                }
            }
        }
        
        /// <summary>
        /// Get LOD statistics for debugging
        /// </summary>
        public LODStatistics GetStatistics()
        {
            int highQualityCount = 0;
            int mediumQualityCount = 0;
            int lowQualityCount = 0;
            int invisibleCount = 0;
            
            foreach (var bubble in allBubbles)
            {
                if (bubble != null)
                {
                    int lodLevel = bubble.GetLODLevel();
                    switch (lodLevel)
                    {
                        case 0: highQualityCount++; break;
                        case 1: mediumQualityCount++; break;
                        case 2: lowQualityCount++; break;
                        default: invisibleCount++; break;
                    }
                }
            }
            
            return new LODStatistics
            {
                totalBubbles = allBubbles.Count,
                visibleBubbles = visibleBubbles.Count,
                highQualityBubbles = highQualityCount,
                mediumQualityBubbles = mediumQualityCount,
                lowQualityBubbles = lowQualityCount,
                invisibleBubbles = invisibleCount,
                currentFPS = 1.0f / averageFrameTime,
                maxRenderDistance = maxRenderDistance
            };
        }
        
        /// <summary>
        /// Clear all registered bubbles
        /// </summary>
        public void ClearAllBubbles()
        {
            allBubbles.Clear();
            visibleBubbles.Clear();
            bubbleLODData.Clear();
            currentBatchIndex = 0;
        }
        
        void OnDestroy()
        {
            if (batchProcessingCoroutine != null)
            {
                StopCoroutine(batchProcessingCoroutine);
            }
            
            if (instance == this)
            {
                instance = null;
            }
        }
        
        void OnDrawGizmosSelected()
        {
            if (cameraTransform == null) return;
            
            // Draw LOD distance rings
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cameraTransform.position, maxRenderDistance * highQualityDistance);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(cameraTransform.position, maxRenderDistance * mediumQualityDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cameraTransform.position, maxRenderDistance);
            
            // Draw visible bubbles
            foreach (var bubble in visibleBubbles)
            {
                if (bubble != null)
                {
                    int lodLevel = bubble.GetLODLevel();
                    Gizmos.color = lodLevel == 0 ? Color.green : 
                                  lodLevel == 1 ? Color.yellow : Color.red;
                    Gizmos.DrawWireSphere(bubble.transform.position, 0.05f);
                }
            }
        }
    }
    
    /// <summary>
    /// LOD data for individual bubbles
    /// </summary>
    [System.Serializable]
    public struct LODBubbleData
    {
        public PooledBubble bubble;
        public float lastDistance;
        public int lastLODLevel;
        public bool isVisible;
        public float lastUpdateTime;
    }
    
    /// <summary>
    /// LOD system statistics
    /// </summary>
    [System.Serializable]
    public struct LODStatistics
    {
        public int totalBubbles;
        public int visibleBubbles;
        public int highQualityBubbles;
        public int mediumQualityBubbles;
        public int lowQualityBubbles;
        public int invisibleBubbles;
        public float currentFPS;
        public float maxRenderDistance;
        
        public override string ToString()
        {
            return $"LOD Stats - Total: {totalBubbles}, Visible: {visibleBubbles}, " +
                   $"High: {highQualityBubbles}, Med: {mediumQualityBubbles}, Low: {lowQualityBubbles}, " +
                   $"FPS: {currentFPS:F1}, Distance: {maxRenderDistance:F1}";
        }
    }
}