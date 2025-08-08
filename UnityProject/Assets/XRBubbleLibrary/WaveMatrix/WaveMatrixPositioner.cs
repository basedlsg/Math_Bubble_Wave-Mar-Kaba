using UnityEngine;
using Unity.Mathematics;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// MonoBehaviour wrapper for the WaveMatrixCore system.
    /// Provides Unity integration for positioning word bubbles on a wave matrix.
    /// Now uses the optimized WaveMatrixCore for improved Quest 3 performance.
    /// </summary>
    public class WaveMatrixPositioner : MonoBehaviour
    {
        [Header("Wave Matrix Parameters")]
        [SerializeField] private WaveMatrixSettings settings = WaveMatrixSettings.Default;
        
        [Header("Performance Settings")]
        [SerializeField] private int maxBubbles = 100; // Increased thanks to WaveMatrixCore optimization
        [SerializeField] private float updateFrequency = 30f; // Improved performance allows higher frequency
        
        // Core wave mathematics system
        private IWaveMatrixCore _waveCore;
        
        // Internal state
        private float currentTime = 0f;
        private float lastUpdateTime = 0f;
        private float updateInterval;
        
        // Wave calculation cache for performance
        private float3[] cachedPositions;
        private bool[] positionsDirty;
        
        void Start()
        {
            InitializeWaveMatrix();
        }
        
        void Update()
        {
            UpdateWaveTime();
            
            // Only update positions at specified frequency for VR performance
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateWavePositions();
                lastUpdateTime = Time.time;
            }
        }
        
        void InitializeWaveMatrix()
        {
            // Initialize the core wave mathematics system
            _waveCore = new WaveMatrixCore();
            
            updateInterval = 1f / updateFrequency;
            cachedPositions = new float3[maxBubbles];
            positionsDirty = new bool[maxBubbles];
            
            // Initialize all positions as dirty
            for (int i = 0; i < maxBubbles; i++)
            {
                positionsDirty[i] = true;
            }
            
            // Validate settings
            var validation = _waveCore.ValidateSettings(settings);
            if (!validation.IsValid)
            {
                Debug.LogWarning($"[WaveMatrixPositioner] Settings validation issues: {string.Join(", ", validation.Issues)}");
            }
            
            if (validation.Warnings.Length > 0)
            {
                Debug.LogWarning($"[WaveMatrixPositioner] Settings warnings: {string.Join(", ", validation.Warnings)}");
            }
            
            Debug.Log($"Wave Matrix initialized: {maxBubbles} max bubbles, {updateFrequency}Hz update rate, Performance Impact: {validation.PerformanceImpact:F2}");
        }
        
        void UpdateWaveTime()
        {
            currentTime = _waveCore.UpdateWaveTime(Time.deltaTime, settings);
        }
        
        void UpdateWavePositions()
        {
            // Use batch processing for better performance
            var indicesToUpdate = new System.Collections.Generic.List<int>();
            
            for (int i = 0; i < maxBubbles; i++)
            {
                if (positionsDirty[i])
                {
                    indicesToUpdate.Add(i);
                    positionsDirty[i] = false;
                }
            }
            
            if (indicesToUpdate.Count > 0)
            {
                var indices = indicesToUpdate.ToArray();
                var results = new float3[indices.Length];
                
                _waveCore.CalculateWavePositionsBatch(indices, currentTime, settings, results);
                
                for (int i = 0; i < indices.Length; i++)
                {
                    cachedPositions[indices[i]] = results[i];
                }
            }
        }
        
        /// <summary>
        /// Calculate wave-based position for a bubble at given index and time.
        /// Now uses the optimized WaveMatrixCore for better performance.
        /// </summary>
        public float3 CalculateWavePosition(int bubbleIndex, float time)
        {
            return _waveCore.CalculateWavePosition(bubbleIndex, time, settings);
        }
        
        /// <summary>
        /// Calculate wave-based position with AI-predicted distance.
        /// Now uses the optimized WaveMatrixCore for better performance.
        /// </summary>
        public float3 CalculateWavePosition(int bubbleIndex, float time, float aiDistance)
        {
            return _waveCore.CalculateWavePosition(bubbleIndex, time, aiDistance, settings);
        }
        
        /// <summary>
        /// Calculate wave height at given position and time.
        /// Now uses the optimized WaveMatrixCore for better performance.
        /// </summary>
        public float CalculateWaveHeight(float x, float z, float time)
        {
            return _waveCore.CalculateWaveHeight(new float2(x, z), time, settings);
        }
        
        /// <summary>
        /// Get cached position for bubble (performance optimized)
        /// </summary>
        public float3 GetBubblePosition(int bubbleIndex)
        {
            if (bubbleIndex < 0 || bubbleIndex >= maxBubbles)
            {
                Debug.LogWarning($"Bubble index {bubbleIndex} out of range [0, {maxBubbles}]");
                return float3.zero;
            }
            
            return cachedPositions[bubbleIndex];
        }
        
        /// <summary>
        /// Mark bubble position as needing recalculation
        /// </summary>
        public void MarkPositionDirty(int bubbleIndex)
        {
            if (bubbleIndex >= 0 && bubbleIndex < maxBubbles)
            {
                positionsDirty[bubbleIndex] = true;
            }
        }
        
        /// <summary>
        /// Get real-time position (bypasses cache, use sparingly)
        /// </summary>
        public float3 GetRealTimePosition(int bubbleIndex, float aiDistance = 0f)
        {
            return CalculateWavePosition(bubbleIndex, currentTime, aiDistance);
        }
        
        /// <summary>
        /// Update wave settings at runtime
        /// </summary>
        public void UpdateWaveSettings(WaveMatrixSettings newSettings)
        {
            settings = newSettings;
            
            // Mark all positions as dirty to recalculate with new settings
            for (int i = 0; i < maxBubbles; i++)
            {
                positionsDirty[i] = true;
            }
        }
        
        // Debug visualization
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            Gizmos.color = Color.cyan;
            
            // Draw wave matrix grid
            for (int i = 0; i < maxBubbles; i++)
            {
                Vector3 pos = GetBubblePosition(i);
                Gizmos.DrawWireSphere(transform.position + pos, 0.1f);
            }
            
            // Draw center reference
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
        }
    }
} 
           Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
        }
        
        /// <summary>
        /// Force update of all bubble positions (useful when settings change)
        /// </summary>
        public void ForceUpdateAllPositions()
        {
            for (int i = 0; i < maxBubbles; i++)
            {
                positionsDirty[i] = true;
            }
            
            UpdateWavePositions();
        }
        
        /// <summary>
        /// Get grid position for a bubble index without wave displacement.
        /// </summary>
        public float3 GetGridPosition(int bubbleIndex)
        {
            return _waveCore.GetGridPosition(bubbleIndex, settings);
        }
        
        /// <summary>
        /// Convert world position to grid index (useful for hit testing).
        /// </summary>
        public int WorldPositionToGridIndex(float3 worldPosition)
        {
            return _waveCore.WorldPositionToGridIndex(worldPosition, settings);
        }
        
        /// <summary>
        /// Calculate multiple bubble positions efficiently in batch.
        /// </summary>
        public void CalculateBubblePositionsBatch(int[] bubbleIndices, float3[] results)
        {
            _waveCore.CalculateWavePositionsBatch(bubbleIndices, currentTime, settings, results);
        }
        
        /// <summary>
        /// Calculate multiple bubble positions with AI distances efficiently in batch.
        /// </summary>
        public void CalculateBubblePositionsBatch(int[] bubbleIndices, float[] aiDistances, float3[] results)
        {
            _waveCore.CalculateWavePositionsBatch(bubbleIndices, currentTime, aiDistances, settings, results);
        }
        
        /// <summary>
        /// Get performance statistics from the core wave system.
        /// </summary>
        public WaveMatrixPerformanceStats GetPerformanceStats()
        {
            if (_waveCore is WaveMatrixCore core)
            {
                return core.GetPerformanceStats();
            }
            
            return new WaveMatrixPerformanceStats();
        }
        
        /// <summary>
        /// Validate current settings and log any issues.
        /// </summary>
        public WaveMatrixValidationResult ValidateCurrentSettings()
        {
            return _waveCore.ValidateSettings(settings);
        }
    }
}