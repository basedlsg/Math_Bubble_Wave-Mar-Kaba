using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Core wave mathematics system for high-performance bubble positioning calculations.
    /// Optimized for Quest 3 VR performance with efficient batch processing and caching.
    /// Implements Requirement 5.1: Core wave mathematics for bubble positioning.
    /// Implements Requirement 5.2: Performance-optimized algorithms for Quest 3.
    /// </summary>
    public class WaveMatrixCore : IWaveMatrixCore
    {
        // Performance optimization constants
        private const int MAX_BATCH_SIZE = 100;
        private const float MIN_WAVE_FREQUENCY = 0.1f;
        private const float MAX_WAVE_FREQUENCY = 10.0f;
        private const float MIN_WAVE_AMPLITUDE = 0.01f;
        private const float MAX_WAVE_AMPLITUDE = 5.0f;
        
        // Cache for frequently used calculations
        private readonly Dictionary<int, float3> _gridPositionCache = new Dictionary<int, float3>();
        private WaveMatrixSettings _lastCachedSettings;
        private float _currentTime = 0f;
        
        /// <summary>
        /// Calculate wave-based position for a bubble at given index and time.
        /// </summary>
        public float3 CalculateWavePosition(int bubbleIndex, float time, WaveMatrixSettings settings)
        {
            return CalculateWavePosition(bubbleIndex, time, 0f, settings);
        }
        
        /// <summary>
        /// Calculate wave-based position with additional distance offset.
        /// </summary>
        public float3 CalculateWavePosition(int bubbleIndex, float time, float distanceOffset, WaveMatrixSettings settings)
        {
            if (settings == null)
            {
                Debug.LogError("[WaveMatrixCore] Settings cannot be null");
                return float3.zero;
            }
            
            if (bubbleIndex < 0)
            {
                Debug.LogWarning($"[WaveMatrixCore] Invalid bubble index: {bubbleIndex}");
                return float3.zero;
            }
            
            // Get base grid position
            float3 gridPos = GetGridPosition(bubbleIndex, settings);
            
            // Apply distance offset
            gridPos.z += distanceOffset * settings.aiDistanceScale;
            
            // Calculate wave height
            float waveHeight = CalculateWaveHeight(new float2(gridPos.x, gridPos.z), time, settings);
            
            return new float3(gridPos.x, waveHeight, gridPos.z);
        }
        
        /// <summary>
        /// Calculate wave height at a specific world position and time.
        /// </summary>
        public float CalculateWaveHeight(float2 worldPosition, float time, WaveMatrixSettings settings)
        {
            if (settings == null)
            {
                return 0f;
            }
            
            float height = 0f;
            float x = worldPosition.x;
            float z = worldPosition.y;
            
            // Primary wave component
            height += math.sin(x * settings.primaryWave.frequency + time * settings.primaryWave.speed) 
                     * settings.primaryWave.amplitude;
            
            // Secondary wave component
            height += math.sin(z * settings.secondaryWave.frequency + time * settings.secondaryWave.speed) 
                     * settings.secondaryWave.amplitude;
            
            // Tertiary wave component (radial)
            float radialDistance = math.length(worldPosition);
            height += math.sin(radialDistance * settings.tertiaryWave.frequency + time * settings.tertiaryWave.speed) 
                     * settings.tertiaryWave.amplitude;
            
            // Interference pattern
            if (settings.enableInterference)
            {
                float interference = math.sin(x * settings.interferenceFreq + time) 
                                   * math.cos(z * settings.interferenceFreq + time) 
                                   * settings.interferenceAmplitude;
                height += interference;
            }
            
            return height;
        }
        
        /// <summary>
        /// Calculate multiple wave positions efficiently in batch.
        /// </summary>
        public void CalculateWavePositionsBatch(int[] bubbleIndices, float time, WaveMatrixSettings settings, float3[] results)
        {
            if (bubbleIndices == null || results == null)
            {
                Debug.LogError("[WaveMatrixCore] Batch arrays cannot be null");
                return;
            }
            
            if (bubbleIndices.Length != results.Length)
            {
                Debug.LogError("[WaveMatrixCore] Batch arrays must be same length");
                return;
            }
            
            if (bubbleIndices.Length > MAX_BATCH_SIZE)
            {
                Debug.LogWarning($"[WaveMatrixCore] Batch size {bubbleIndices.Length} exceeds recommended maximum {MAX_BATCH_SIZE}");
            }
            
            // Process in batches for optimal performance
            for (int i = 0; i < bubbleIndices.Length; i++)
            {
                results[i] = CalculateWavePosition(bubbleIndices[i], time, settings);
            }
        }
        
        /// <summary>
        /// Calculate multiple wave positions with distance offsets efficiently in batch.
        /// </summary>
        public void CalculateWavePositionsBatch(int[] bubbleIndices, float time, float[] distanceOffsets, WaveMatrixSettings settings, float3[] results)
        {
            if (bubbleIndices == null || distanceOffsets == null || results == null)
            {
                Debug.LogError("[WaveMatrixCore] Batch arrays cannot be null");
                return;
            }
            
            if (bubbleIndices.Length != distanceOffsets.Length || bubbleIndices.Length != results.Length)
            {
                Debug.LogError("[WaveMatrixCore] All batch arrays must be same length");
                return;
            }
            
            // Process in batches for optimal performance
            for (int i = 0; i < bubbleIndices.Length; i++)
            {
                results[i] = CalculateWavePosition(bubbleIndices[i], time, distanceOffsets[i], settings);
            }
        }
        
        /// <summary>
        /// Update wave state for time-based animations.
        /// </summary>
        public float UpdateWaveTime(float deltaTime, WaveMatrixSettings settings)
        {
            if (settings == null)
            {
                return _currentTime;
            }
            
            _currentTime += deltaTime * settings.timeScale;
            return _currentTime;
        }
        
        /// <summary>
        /// Validate wave matrix settings for performance and correctness.
        /// </summary>
        public WaveMatrixValidationResult ValidateSettings(WaveMatrixSettings settings)
        {
            if (settings == null)
            {
                return WaveMatrixValidationResult.Invalid("Settings cannot be null");
            }
            
            var issues = new List<string>();
            var warnings = new List<string>();
            float performanceImpact = 0f;
            
            // Validate grid dimensions
            if (settings.gridWidth <= 0 || settings.gridHeight <= 0)
            {
                issues.Add("Grid dimensions must be positive");
            }
            
            if (settings.gridWidth * settings.gridHeight > 100)
            {
                warnings.Add($"Large grid size ({settings.gridWidth}x{settings.gridHeight}) may impact Quest 3 performance");
                performanceImpact += 0.3f;
            }
            
            // Validate spacing
            if (settings.spacing <= 0f)
            {
                issues.Add("Grid spacing must be positive");
            }
            
            // Validate wave parameters
            ValidateWaveComponent("Primary wave", settings.primaryWave, issues, warnings, ref performanceImpact);
            ValidateWaveComponent("Secondary wave", settings.secondaryWave, issues, warnings, ref performanceImpact);
            ValidateWaveComponent("Tertiary wave", settings.tertiaryWave, issues, warnings, ref performanceImpact);
            
            // Validate interference settings
            if (settings.enableInterference)
            {
                if (settings.interferenceFreq < MIN_WAVE_FREQUENCY || settings.interferenceFreq > MAX_WAVE_FREQUENCY)
                {
                    warnings.Add($"Interference frequency {settings.interferenceFreq} outside recommended range [{MIN_WAVE_FREQUENCY}, {MAX_WAVE_FREQUENCY}]");
                }
                
                if (math.abs(settings.interferenceAmplitude) > MAX_WAVE_AMPLITUDE)
                {
                    warnings.Add($"Interference amplitude {settings.interferenceAmplitude} may cause excessive motion");
                }
                
                performanceImpact += 0.1f; // Interference adds computational cost
            }
            
            // Validate time scale
            if (settings.timeScale <= 0f)
            {
                issues.Add("Time scale must be positive");
            }
            
            if (settings.timeScale > 5f)
            {
                warnings.Add($"High time scale ({settings.timeScale}) may cause motion sickness in VR");
            }
            
            // Validate AI distance scale
            if (settings.aiDistanceScale < 0f)
            {
                warnings.Add("Negative AI distance scale may cause confusing positioning");
            }
            
            // Clamp performance impact
            performanceImpact = math.clamp(performanceImpact, 0f, 1f);
            
            return new WaveMatrixValidationResult
            {
                IsValid = issues.Count == 0,
                Issues = issues.ToArray(),
                Warnings = warnings.ToArray(),
                PerformanceImpact = performanceImpact
            };
        }
        
        /// <summary>
        /// Get grid position for a bubble index without wave displacement.
        /// </summary>
        public float3 GetGridPosition(int bubbleIndex, WaveMatrixSettings settings)
        {
            if (settings == null)
            {
                return float3.zero;
            }
            
            // Check cache first
            if (_lastCachedSettings == settings && _gridPositionCache.TryGetValue(bubbleIndex, out float3 cachedPos))
            {
                return cachedPos;
            }
            
            // Calculate grid position
            int gridX = bubbleIndex % settings.gridWidth;
            int gridZ = bubbleIndex / settings.gridWidth;
            
            float worldX = gridX * settings.spacing;
            float worldZ = gridZ * settings.spacing;
            
            // Center the grid
            worldX -= (settings.gridWidth * settings.spacing) * 0.5f;
            worldZ -= (settings.gridHeight * settings.spacing) * 0.5f;
            
            float3 gridPos = new float3(worldX, 0f, worldZ);
            
            // Update cache
            if (_lastCachedSettings != settings)
            {
                _gridPositionCache.Clear();
                _lastCachedSettings = settings;
            }
            
            _gridPositionCache[bubbleIndex] = gridPos;
            
            return gridPos;
        }
        
        /// <summary>
        /// Convert world position to grid index (useful for hit testing).
        /// </summary>
        public int WorldPositionToGridIndex(float3 worldPosition, WaveMatrixSettings settings)
        {
            if (settings == null)
            {
                return -1;
            }
            
            // Offset to grid space
            float gridSpaceX = worldPosition.x + (settings.gridWidth * settings.spacing) * 0.5f;
            float gridSpaceZ = worldPosition.z + (settings.gridHeight * settings.spacing) * 0.5f;
            
            // Convert to grid coordinates
            int gridX = Mathf.RoundToInt(gridSpaceX / settings.spacing);
            int gridZ = Mathf.RoundToInt(gridSpaceZ / settings.spacing);
            
            // Check bounds
            if (gridX < 0 || gridX >= settings.gridWidth || gridZ < 0 || gridZ >= settings.gridHeight)
            {
                return -1;
            }
            
            return gridZ * settings.gridWidth + gridX;
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Validate a wave component for correctness and performance.
        /// </summary>
        private void ValidateWaveComponent(string componentName, WaveMatrixSettings.WaveComponent wave, 
                                         List<string> issues, List<string> warnings, ref float performanceImpact)
        {
            if (wave.frequency < MIN_WAVE_FREQUENCY || wave.frequency > MAX_WAVE_FREQUENCY)
            {
                warnings.Add($"{componentName} frequency {wave.frequency} outside recommended range [{MIN_WAVE_FREQUENCY}, {MAX_WAVE_FREQUENCY}]");
            }
            
            if (math.abs(wave.amplitude) < MIN_WAVE_AMPLITUDE)
            {
                warnings.Add($"{componentName} amplitude {wave.amplitude} may be too small to be visible");
            }
            
            if (math.abs(wave.amplitude) > MAX_WAVE_AMPLITUDE)
            {
                warnings.Add($"{componentName} amplitude {wave.amplitude} may cause excessive motion in VR");
                performanceImpact += 0.1f;
            }
            
            if (wave.speed < 0f)
            {
                warnings.Add($"{componentName} has negative speed, causing reverse wave motion");
            }
            
            if (wave.speed > 10f)
            {
                warnings.Add($"{componentName} speed {wave.speed} may cause motion sickness in VR");
                performanceImpact += 0.1f;
            }
        }
        
        #endregion
        
        #region Performance Monitoring
        
        /// <summary>
        /// Get performance statistics for monitoring and optimization.
        /// </summary>
        public WaveMatrixPerformanceStats GetPerformanceStats()
        {
            return new WaveMatrixPerformanceStats
            {
                CachedGridPositions = _gridPositionCache.Count,
                CurrentTime = _currentTime,
                LastCachedSettings = _lastCachedSettings != null ? _lastCachedSettings.GetHashCode() : 0
            };
        }
        
        /// <summary>
        /// Clear all caches to free memory.
        /// </summary>
        public void ClearCaches()
        {
            _gridPositionCache.Clear();
            _lastCachedSettings = null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Performance statistics for the wave matrix core system.
    /// </summary>
    public struct WaveMatrixPerformanceStats
    {
        /// <summary>
        /// Number of cached grid positions.
        /// </summary>
        public int CachedGridPositions;
        
        /// <summary>
        /// Current wave time.
        /// </summary>
        public float CurrentTime;
        
        /// <summary>
        /// Hash of the last cached settings.
        /// </summary>
        public int LastCachedSettings;
    }
}