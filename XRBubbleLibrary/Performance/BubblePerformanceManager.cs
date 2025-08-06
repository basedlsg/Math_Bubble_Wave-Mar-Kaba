using UnityEngine;
using System.Collections.Generic;
using XRBubbleLibrary.Physics;

namespace XRBubbleLibrary.Performance
{
    /// <summary>
    /// Central performance management system for XR Bubble Library
    /// Integrates LOD management, batch processing, and performance monitoring
    /// Cloned from Unity performance optimization samples, optimized for Quest 3
    /// </summary>
    public class BubblePerformanceManager : MonoBehaviour
    {
        [Header("Performance Targets")]
        [Range(60, 120)]
        public int targetFrameRate = 72; // Quest 3 target
        
        [Range(0.010f, 0.020f)]
        public float maxFrameTimeMs = 0.014f; // ~72 FPS
        
        [Range(100, 500)]
        public int maxTotalBubbles = 200;
        
        [Header("Adaptive Quality")]
        public bool enableAdaptiveQuality = true;
        
        [Range(0.1f, 2.0f)]
        public float qualityAdjustmentSpeed = 0.5f;
        
        [Range(0.05f, 0.2f)]
        public float performanceCheckInterval = 0.1f;
        
        [Header("Memory Management")]
        public bool enableMemoryMonitoring = true;
        
        [Range(100, 1000)]
        public int maxMemoryMB = 400; // Quest 3 app memory limit
        
        [Range(50, 200)]
        public int memoryWarningThresholdMB = 300;
        
        [Header("Thermal Management")]
        public bool enableThermalMonitoring = true;
        public float thermalThrottleTemperature = 45.0f; // Celsius
        
        // Component references
        private BubbleLODManager lodManager;
        private BubbleBatchProcessor batchProcessor;
        private BubbleObjectPool objectPool;
        
        // Performance tracking
        private float[] frameTimeHistory = new float[60]; // 1 second at 60 FPS
        private int frameTimeIndex = 0;
        private float averageFrameTime = 0.016f;
        private float lastPerformanceCheck = 0.0f;
        
        // Quality levels
        private int currentQualityLevel = 2; // 0=Low, 1=Medium, 2=High
        private float lastQualityAdjustment = 0.0f;
        
        // Memory tracking
        private long lastMemoryUsage = 0;
        private float lastMemoryCheck = 0.0f;
        private const float memoryCheckInterval = 1.0f;
        
        // Thermal tracking
        private float currentTemperature = 25.0f;
        private bool thermalThrottling = false;
        
        // Statistics
        private PerformanceStatistics currentStats;
        
        // Singleton pattern
        private static BubblePerformanceManager instance;
        public static BubblePerformanceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<BubblePerformanceManager>();
                    if (instance == null)
                    {
                        GameObject perfManager = new GameObject("BubblePerformanceManager");
                        instance = perfManager.AddComponent<BubblePerformanceManager>();
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
            
            InitializePerformanceManager();
        }
        
        void Update()
        {
            UpdateFrameTimeTracking();
            
            if (Time.time - lastPerformanceCheck >= performanceCheckInterval)
            {
                CheckPerformance();
                lastPerformanceCheck = Time.time;
            }
            
            if (enableMemoryMonitoring && Time.time - lastMemoryCheck >= memoryCheckInterval)
            {
                CheckMemoryUsage();
                lastMemoryCheck = Time.time;
            }
            
            if (enableThermalMonitoring)
            {
                CheckThermalStatus();
            }
        }
        
        /// <summary>
        /// Initialize the performance management system
        /// </summary>
        void InitializePerformanceManager()
        {
            // Get or create component references
            lodManager = BubbleLODManager.Instance;
            batchProcessor = BubbleBatchProcessor.Instance;
            objectPool = FindObjectOfType<BubbleObjectPool>();
            
            // Set initial quality level based on platform
            #if UNITY_ANDROID
                currentQualityLevel = 1; // Medium quality for Quest 3
            #else
                currentQualityLevel = 2; // High quality for PC
            #endif
            
            ApplyQualitySettings(currentQualityLevel);
            
            // Initialize frame time history
            for (int i = 0; i < frameTimeHistory.Length; i++)
            {
                frameTimeHistory[i] = 0.016f; // Start with 60 FPS assumption
            }
            
            Debug.Log($"BubblePerformanceManager initialized - Quality Level: {currentQualityLevel}");
        }
        
        /// <summary>
        /// Update frame time tracking for performance analysis
        /// </summary>
        void UpdateFrameTimeTracking()
        {
            float currentFrameTime = Time.unscaledDeltaTime;
            
            // Store in circular buffer
            frameTimeHistory[frameTimeIndex] = currentFrameTime;
            frameTimeIndex = (frameTimeIndex + 1) % frameTimeHistory.Length;
            
            // Calculate moving average
            float totalTime = 0.0f;
            for (int i = 0; i < frameTimeHistory.Length; i++)
            {
                totalTime += frameTimeHistory[i];
            }
            averageFrameTime = totalTime / frameTimeHistory.Length;
        }
        
        /// <summary>
        /// Check overall performance and adjust quality if needed
        /// </summary>
        void CheckPerformance()
        {
            float currentFPS = 1.0f / averageFrameTime;
            float targetFPS = targetFrameRate;
            
            // Update statistics
            currentStats.currentFPS = currentFPS;
            currentStats.averageFrameTime = averageFrameTime;
            currentStats.qualityLevel = currentQualityLevel;
            
            if (enableAdaptiveQuality && Time.time - lastQualityAdjustment >= 2.0f)
            {
                // Check if we need to adjust quality
                if (currentFPS < targetFPS * 0.85f) // 15% below target
                {
                    ReduceQuality();
                }
                else if (currentFPS > targetFPS * 1.15f && currentQualityLevel < 2) // 15% above target
                {
                    IncreaseQuality();
                }
            }
            
            // Log performance warnings
            if (currentFPS < targetFPS * 0.7f) // 30% below target
            {
                Debug.LogWarning($"Performance critical: {currentFPS:F1} FPS (target: {targetFPS})");
            }
        }
        
        /// <summary>
        /// Check memory usage and manage accordingly
        /// </summary>
        void CheckMemoryUsage()
        {
            #if UNITY_ANDROID
                long currentMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false) / (1024 * 1024);
                currentStats.memoryUsageMB = (int)currentMemory;
                
                if (currentMemory > memoryWarningThresholdMB)
                {
                    Debug.LogWarning($"Memory usage high: {currentMemory}MB (threshold: {memoryWarningThresholdMB}MB)");
                    
                    // Force garbage collection
                    System.GC.Collect();
                    
                    // Reduce bubble count if necessary
                    if (currentMemory > maxMemoryMB * 0.9f)
                    {
                        ReduceBubbleCount();
                    }
                }
                
                lastMemoryUsage = currentMemory;
            #endif
        }
        
        /// <summary>
        /// Check thermal status and throttle if necessary
        /// </summary>
        void CheckThermalStatus()
        {
            #if UNITY_ANDROID
                // Simulate thermal monitoring (actual implementation would use Android APIs)
                currentTemperature = Mathf.Lerp(currentTemperature, 
                    averageFrameTime > 0.020f ? 50.0f : 35.0f, Time.deltaTime * 0.1f);
                
                currentStats.temperature = currentTemperature;
                
                if (currentTemperature > thermalThrottleTemperature && !thermalThrottling)
                {
                    Debug.LogWarning($"Thermal throttling activated: {currentTemperature:F1}°C");
                    thermalThrottling = true;
                    ReduceQuality();
                }
                else if (currentTemperature < thermalThrottleTemperature - 5.0f && thermalThrottling)
                {
                    Debug.Log("Thermal throttling deactivated");
                    thermalThrottling = false;
                }
            #endif
        }
        
        /// <summary>
        /// Reduce quality to improve performance
        /// </summary>
        void ReduceQuality()
        {
            if (currentQualityLevel <= 0) return;
            
            currentQualityLevel--;
            ApplyQualitySettings(currentQualityLevel);
            lastQualityAdjustment = Time.time;
            
            Debug.Log($"Quality reduced to level {currentQualityLevel}");
        }
        
        /// <summary>
        /// Increase quality when performance allows
        /// </summary>
        void IncreaseQuality()
        {
            if (currentQualityLevel >= 2 || thermalThrottling) return;
            
            currentQualityLevel++;
            ApplyQualitySettings(currentQualityLevel);
            lastQualityAdjustment = Time.time;
            
            Debug.Log($"Quality increased to level {currentQualityLevel}");
        }
        
        /// <summary>
        /// Apply quality settings to all performance systems
        /// </summary>
        void ApplyQualitySettings(int qualityLevel)
        {
            switch (qualityLevel)
            {
                case 0: // Low Quality
                    ApplyLowQualitySettings();
                    break;
                case 1: // Medium Quality
                    ApplyMediumQualitySettings();
                    break;
                case 2: // High Quality
                    ApplyHighQualitySettings();
                    break;
            }
        }
        
        /// <summary>
        /// Apply low quality settings for maximum performance
        /// </summary>
        void ApplyLowQualitySettings()
        {
            if (lodManager != null)
            {
                lodManager.maxRenderDistance = 5.0f;
                lodManager.maxActiveBubbles = 100;
                lodManager.maxHighQualityBubbles = 20;
                lodManager.batchProcessingInterval = 0.05f;
            }
            
            if (batchProcessor != null)
            {
                batchProcessor.maxBatchSize = 30;
                batchProcessor.jobsPerFrame = 2;
                batchProcessor.enableMultithreading = true;
            }
            
            maxTotalBubbles = 100;
        }
        
        /// <summary>
        /// Apply medium quality settings for balanced performance
        /// </summary>
        void ApplyMediumQualitySettings()
        {
            if (lodManager != null)
            {
                lodManager.maxRenderDistance = 7.0f;
                lodManager.maxActiveBubbles = 150;
                lodManager.maxHighQualityBubbles = 35;
                lodManager.batchProcessingInterval = 0.033f;
            }
            
            if (batchProcessor != null)
            {
                batchProcessor.maxBatchSize = 40;
                batchProcessor.jobsPerFrame = 3;
                batchProcessor.enableMultithreading = true;
            }
            
            maxTotalBubbles = 150;
        }
        
        /// <summary>
        /// Apply high quality settings for best visual experience
        /// </summary>
        void ApplyHighQualitySettings()
        {
            if (lodManager != null)
            {
                lodManager.maxRenderDistance = 10.0f;
                lodManager.maxActiveBubbles = 200;
                lodManager.maxHighQualityBubbles = 50;
                lodManager.batchProcessingInterval = 0.025f;
            }
            
            if (batchProcessor != null)
            {
                batchProcessor.maxBatchSize = 50;
                batchProcessor.jobsPerFrame = 4;
                batchProcessor.enableMultithreading = true;
            }
            
            maxTotalBubbles = 200;
        }
        
        /// <summary>
        /// Reduce bubble count to manage memory/performance
        /// </summary>
        void ReduceBubbleCount()
        {
            if (objectPool != null)
            {
                int targetReduction = Mathf.Max(10, maxTotalBubbles / 10);
                objectPool.ReducePoolSize(targetReduction);
                
                Debug.Log($"Reduced bubble pool by {targetReduction} bubbles");
            }
        }
        
        /// <summary>
        /// Force immediate performance optimization
        /// </summary>
        public void ForceOptimization()
        {
            // Force LOD updates
            if (lodManager != null)
            {
                lodManager.ForceUpdateAllLODs();
            }
            
            // Complete batch processing
            if (batchProcessor != null)
            {
                // Batch processor handles this internally
            }
            
            // Force garbage collection
            System.GC.Collect();
            
            Debug.Log("Forced performance optimization");
        }
        
        /// <summary>
        /// Get comprehensive performance statistics
        /// </summary>
        public PerformanceStatistics GetStatistics()
        {
            // Update bubble counts
            if (lodManager != null)
            {
                var lodStats = lodManager.GetStatistics();
                currentStats.totalBubbles = lodStats.totalBubbles;
                currentStats.visibleBubbles = lodStats.visibleBubbles;
                currentStats.highQualityBubbles = lodStats.highQualityBubbles;
            }
            
            if (batchProcessor != null)
            {
                var batchStats = batchProcessor.GetStatistics();
                currentStats.processedBubblesThisFrame = batchStats.processedBubblesThisFrame;
                currentStats.batchProcessingTimeMs = batchStats.lastProcessingTimeMs;
            }
            
            return currentStats;
        }
        
        /// <summary>
        /// Set target frame rate and adjust settings accordingly
        /// </summary>
        public void SetTargetFrameRate(int fps)
        {
            targetFrameRate = fps;
            maxFrameTimeMs = 1.0f / fps;
            
            // Adjust quality based on new target
            if (fps >= 90)
            {
                ApplyHighQualitySettings();
            }
            else if (fps >= 72)
            {
                ApplyMediumQualitySettings();
            }
            else
            {
                ApplyLowQualitySettings();
            }
            
            Debug.Log($"Target frame rate set to {fps} FPS");
        }
        
        /// <summary>
        /// Enable or disable specific performance features
        /// </summary>
        public void SetPerformanceFeature(PerformanceFeature feature, bool enabled)
        {
            switch (feature)
            {
                case PerformanceFeature.AdaptiveQuality:
                    enableAdaptiveQuality = enabled;
                    break;
                case PerformanceFeature.MemoryMonitoring:
                    enableMemoryMonitoring = enabled;
                    break;
                case PerformanceFeature.ThermalMonitoring:
                    enableThermalMonitoring = enabled;
                    break;
                case PerformanceFeature.BatchProcessing:
                    if (batchProcessor != null)
                        batchProcessor.enablePhysicsProcessing = enabled;
                    break;
                case PerformanceFeature.LODSystem:
                    if (lodManager != null)
                        lodManager.enableBatchProcessing = enabled;
                    break;
            }
            
            Debug.Log($"Performance feature {feature} {(enabled ? "enabled" : "disabled")}");
        }
        
        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        void OnGUI()
        {
            if (!Debug.isDebugBuild) return;
            
            // Display performance stats in debug builds
            var stats = GetStatistics();
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"FPS: {stats.currentFPS:F1} (Target: {targetFrameRate})");
            GUILayout.Label($"Frame Time: {stats.averageFrameTime * 1000:F2}ms");
            GUILayout.Label($"Quality Level: {stats.qualityLevel}");
            GUILayout.Label($"Bubbles: {stats.visibleBubbles}/{stats.totalBubbles}");
            GUILayout.Label($"Memory: {stats.memoryUsageMB}MB");
            GUILayout.Label($"Temperature: {stats.temperature:F1}°C");
            GUILayout.EndArea();
        }
    }
    
    /// <summary>
    /// Performance feature enumeration
    /// </summary>
    public enum PerformanceFeature
    {
        AdaptiveQuality,
        MemoryMonitoring,
        ThermalMonitoring,
        BatchProcessing,
        LODSystem
    }
    
    /// <summary>
    /// Comprehensive performance statistics
    /// </summary>
    [System.Serializable]
    public struct PerformanceStatistics
    {
        public float currentFPS;
        public float averageFrameTime;
        public int qualityLevel;
        public int totalBubbles;
        public int visibleBubbles;
        public int highQualityBubbles;
        public int processedBubblesThisFrame;
        public float batchProcessingTimeMs;
        public int memoryUsageMB;
        public float temperature;
        
        public override string ToString()
        {
            return $"Performance Stats - FPS: {currentFPS:F1}, Quality: {qualityLevel}, " +
                   $"Bubbles: {visibleBubbles}/{totalBubbles}, Memory: {memoryUsageMB}MB, " +
                   $"Temp: {temperature:F1}°C";
        }
    }
}