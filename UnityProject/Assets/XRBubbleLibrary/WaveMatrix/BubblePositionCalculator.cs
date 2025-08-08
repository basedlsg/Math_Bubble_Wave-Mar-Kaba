using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// High-performance bubble position calculator optimized for Quest 3 VR.
    /// Manages positions for 100+ bubbles with efficient caching and batch processing.
    /// Implements Requirement 5.1: Efficient algorithms for 100+ bubble positioning.
    /// Implements Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public class BubblePositionCalculator : IBubblePositionCalculator
    {
        // Performance constants
        private const int DEFAULT_MAX_BUBBLES = 100;
        private const float BOUNDS_MARGIN = 0.1f;
        private const float POSITION_EPSILON = 0.001f;
        
        // Core dependencies
        private readonly IWaveMatrixCore _waveCore;
        
        // Position management
        private float3[] _cachedPositions;
        private bool[] _dirtyFlags;
        private int _maxBubbles;
        private bool _cachingEnabled = true;
        
        // Performance tracking
        private BubblePositionCalculatorStats _stats;
        private readonly Stopwatch _performanceTimer = new Stopwatch();
        
        // Spatial optimization
        private readonly Dictionary<int, float3> _spatialCache = new Dictionary<int, float3>();
        private float _lastCacheTime = -1f;
        
        /// <summary>
        /// Initialize the bubble position calculator with default settings.
        /// </summary>
        public BubblePositionCalculator() : this(new WaveMatrixCore(), DEFAULT_MAX_BUBBLES)
        {
        }
        
        /// <summary>
        /// Initialize the bubble position calculator with custom wave core and bubble count.
        /// </summary>
        /// <param name="waveCore">Wave mathematics core to use for calculations</param>
        /// <param name="maxBubbles">Maximum number of bubbles to support</param>
        public BubblePositionCalculator(IWaveMatrixCore waveCore, int maxBubbles = DEFAULT_MAX_BUBBLES)
        {
            _waveCore = waveCore ?? throw new System.ArgumentNullException(nameof(waveCore));
            SetMaxBubbles(maxBubbles);
            
            UnityEngine.Debug.Log($"[BubblePositionCalculator] Initialized with {maxBubbles} max bubbles");
        }
        
        /// <summary>
        /// Calculate positions for all active bubbles.
        /// </summary>
        public float3[] CalculateAllPositions(float time, WaveMatrixSettings settings)
        {
            if (settings == null)
            {
                UnityEngine.Debug.LogError("[BubblePositionCalculator] Settings cannot be null");
                return new float3[0];
            }
            
            _performanceTimer.Restart();
            
            var positions = new float3[_maxBubbles];
            var indices = new int[_maxBubbles];
            
            // Create index array
            for (int i = 0; i < _maxBubbles; i++)
            {
                indices[i] = i;
            }
            
            // Use batch processing for efficiency
            _waveCore.CalculateWavePositionsBatch(indices, time, settings, positions);
            
            // Update cache if enabled
            if (_cachingEnabled)
            {
                for (int i = 0; i < _maxBubbles; i++)
                {
                    _cachedPositions[i] = positions[i];
                    _dirtyFlags[i] = false;
                }
                _lastCacheTime = time;
            }
            
            _performanceTimer.Stop();
            UpdatePerformanceStats(_maxBubbles, _performanceTimer.ElapsedMilliseconds);
            
            UnityEngine.Debug.Log($"[BubblePositionCalculator] Calculated {_maxBubbles} positions in {_performanceTimer.ElapsedMilliseconds}ms");
            
            return positions;
        }
        
        /// <summary>
        /// Calculate positions for specific bubbles by index.
        /// </summary>
        public float3[] CalculatePositions(int[] bubbleIndices, float time, WaveMatrixSettings settings)
        {
            if (bubbleIndices == null || settings == null)
            {
                UnityEngine.Debug.LogError("[BubblePositionCalculator] Invalid parameters");
                return new float3[0];
            }
            
            _performanceTimer.Restart();
            
            var positions = new float3[bubbleIndices.Length];
            var indicesToCalculate = new List<int>();
            var cachedResults = new List<float3>();
            
            // Check cache first if enabled
            if (_cachingEnabled && math.abs(time - _lastCacheTime) < POSITION_EPSILON)
            {
                for (int i = 0; i < bubbleIndices.Length; i++)
                {
                    int index = bubbleIndices[i];
                    if (index >= 0 && index < _maxBubbles && !_dirtyFlags[index])
                    {
                        positions[i] = _cachedPositions[index];
                        _stats.CacheHits++;
                    }
                    else
                    {
                        indicesToCalculate.Add(i);
                        _stats.CacheMisses++;
                    }
                }
            }
            else
            {
                // All positions need calculation
                indicesToCalculate.AddRange(System.Linq.Enumerable.Range(0, bubbleIndices.Length));
                _stats.CacheMisses += bubbleIndices.Length;
            }
            
            // Calculate uncached positions
            if (indicesToCalculate.Count > 0)
            {
                var uncachedIndices = indicesToCalculate.Select(i => bubbleIndices[i]).ToArray();
                var uncachedPositions = new float3[uncachedIndices.Length];
                
                _waveCore.CalculateWavePositionsBatch(uncachedIndices, time, settings, uncachedPositions);
                
                // Update results and cache
                for (int i = 0; i < indicesToCalculate.Count; i++)
                {
                    int resultIndex = indicesToCalculate[i];
                    int bubbleIndex = bubbleIndices[resultIndex];
                    
                    positions[resultIndex] = uncachedPositions[i];
                    
                    // Update cache
                    if (_cachingEnabled && bubbleIndex >= 0 && bubbleIndex < _maxBubbles)
                    {
                        _cachedPositions[bubbleIndex] = uncachedPositions[i];
                        _dirtyFlags[bubbleIndex] = false;
                    }
                }
                
                _lastCacheTime = time;
            }
            
            _performanceTimer.Stop();
            UpdatePerformanceStats(bubbleIndices.Length, _performanceTimer.ElapsedMilliseconds);
            
            return positions;
        }
        
        /// <summary>
        /// Calculate positions for bubbles with AI distance offsets.
        /// </summary>
        public float3[] CalculatePositionsWithAI(BubbleData[] bubbleData, float time, WaveMatrixSettings settings)
        {
            if (bubbleData == null || settings == null)
            {
                UnityEngine.Debug.LogError("[BubblePositionCalculator] Invalid parameters");
                return new float3[0];
            }
            
            _performanceTimer.Restart();
            
            // Filter active bubbles and sort by priority
            var activeBubbles = bubbleData.Where(b => b.IsActive).OrderByDescending(b => b.Priority).ToArray();
            
            var indices = activeBubbles.Select(b => b.Index).ToArray();
            var aiDistances = activeBubbles.Select(b => b.AIDistance).ToArray();
            var positions = new float3[activeBubbles.Length];
            
            // Use batch processing with AI distances
            _waveCore.CalculateWavePositionsBatch(indices, time, aiDistances, settings, positions);
            
            // Update cache for active bubbles
            if (_cachingEnabled)
            {
                for (int i = 0; i < activeBubbles.Length; i++)
                {
                    int bubbleIndex = activeBubbles[i].Index;
                    if (bubbleIndex >= 0 && bubbleIndex < _maxBubbles)
                    {
                        _cachedPositions[bubbleIndex] = positions[i];
                        _dirtyFlags[bubbleIndex] = false;
                    }
                }
                _lastCacheTime = time;
            }
            
            _performanceTimer.Stop();
            UpdatePerformanceStats(activeBubbles.Length, _performanceTimer.ElapsedMilliseconds);
            
            UnityEngine.Debug.Log($"[BubblePositionCalculator] Calculated {activeBubbles.Length} AI positions in {_performanceTimer.ElapsedMilliseconds}ms");
            
            return positions;
        }
        
        /// <summary>
        /// Update bubble positions incrementally (only dirty bubbles).
        /// </summary>
        public int UpdateDirtyPositions(float time, WaveMatrixSettings settings)
        {
            if (settings == null || !_cachingEnabled)
            {
                return 0;
            }
            
            _performanceTimer.Restart();
            
            // Find dirty positions
            var dirtyIndices = new List<int>();
            for (int i = 0; i < _maxBubbles; i++)
            {
                if (_dirtyFlags[i])
                {
                    dirtyIndices.Add(i);
                }
            }
            
            if (dirtyIndices.Count == 0)
            {
                return 0;
            }
            
            // Calculate dirty positions
            var indices = dirtyIndices.ToArray();
            var positions = new float3[indices.Length];
            
            _waveCore.CalculateWavePositionsBatch(indices, time, settings, positions);
            
            // Update cache
            for (int i = 0; i < indices.Length; i++)
            {
                int bubbleIndex = indices[i];
                _cachedPositions[bubbleIndex] = positions[i];
                _dirtyFlags[bubbleIndex] = false;
            }
            
            _lastCacheTime = time;
            
            _performanceTimer.Stop();
            UpdatePerformanceStats(dirtyIndices.Count, _performanceTimer.ElapsedMilliseconds);
            
            UnityEngine.Debug.Log($"[BubblePositionCalculator] Updated {dirtyIndices.Count} dirty positions in {_performanceTimer.ElapsedMilliseconds}ms");
            
            return dirtyIndices.Count;
        }
        
        /// <summary>
        /// Get cached position for a specific bubble.
        /// </summary>
        public float3 GetCachedPosition(int bubbleIndex)
        {
            if (!_cachingEnabled || bubbleIndex < 0 || bubbleIndex >= _maxBubbles)
            {
                return float3.zero;
            }
            
            if (_dirtyFlags[bubbleIndex])
            {
                UnityEngine.Debug.LogWarning($"[BubblePositionCalculator] Requested cached position for dirty bubble {bubbleIndex}");
                return float3.zero;
            }
            
            _stats.CacheHits++;
            return _cachedPositions[bubbleIndex];
        }
        
        /// <summary>
        /// Set a bubble position as dirty (needs recalculation).
        /// </summary>
        public void MarkBubbleDirty(int bubbleIndex)
        {
            if (bubbleIndex >= 0 && bubbleIndex < _maxBubbles)
            {
                _dirtyFlags[bubbleIndex] = true;
            }
        }
        
        /// <summary>
        /// Mark all bubble positions as dirty.
        /// </summary>
        public void MarkAllBubblesDirty()
        {
            for (int i = 0; i < _maxBubbles; i++)
            {
                _dirtyFlags[i] = true;
            }
            
            UnityEngine.Debug.Log("[BubblePositionCalculator] Marked all bubbles as dirty");
        }
        
        /// <summary>
        /// Validate bubble positions against bounds and constraints.
        /// </summary>
        public BubblePositionValidationResult ValidatePositions(float3[] positions, WaveMatrixSettings settings)
        {
            if (positions == null || settings == null)
            {
                return BubblePositionValidationResult.Invalid("Invalid parameters");
            }
            
            var issues = new List<string>();
            var outOfBoundsIndices = new List<int>();
            var invalidValueIndices = new List<int>();
            float performanceImpact = 0f;
            
            // Calculate bounds
            float maxX = (settings.gridWidth * settings.spacing) * 0.5f + BOUNDS_MARGIN;
            float maxZ = (settings.gridHeight * settings.spacing) * 0.5f + BOUNDS_MARGIN;
            float maxY = math.max(settings.primaryWave.amplitude, 
                         math.max(settings.secondaryWave.amplitude, settings.tertiaryWave.amplitude)) * 2f;
            
            for (int i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];
                
                // Check for invalid values
                if (!math.isfinite(pos.x) || !math.isfinite(pos.y) || !math.isfinite(pos.z))
                {
                    invalidValueIndices.Add(i);
                    continue;
                }
                
                // Check bounds
                if (math.abs(pos.x) > maxX || math.abs(pos.z) > maxZ || math.abs(pos.y) > maxY)
                {
                    outOfBoundsIndices.Add(i);
                }
            }
            
            // Generate issues
            if (invalidValueIndices.Count > 0)
            {
                issues.Add($"{invalidValueIndices.Count} positions have invalid values (NaN/Infinity)");
                performanceImpact += 0.5f;
            }
            
            if (outOfBoundsIndices.Count > 0)
            {
                issues.Add($"{outOfBoundsIndices.Count} positions are out of bounds");
                performanceImpact += 0.3f;
            }
            
            // Check performance impact
            if (positions.Length > 100)
            {
                issues.Add($"Large position count ({positions.Length}) may impact Quest 3 performance");
                performanceImpact += 0.2f;
            }
            
            return new BubblePositionValidationResult
            {
                IsValid = issues.Count == 0,
                Issues = issues.ToArray(),
                OutOfBoundsIndices = outOfBoundsIndices.ToArray(),
                InvalidValueIndices = invalidValueIndices.ToArray(),
                PerformanceImpact = math.clamp(performanceImpact, 0f, 1f)
            };
        }
        
        /// <summary>
        /// Check if a world position is within valid bubble bounds.
        /// </summary>
        public bool IsPositionInBounds(float3 worldPosition, WaveMatrixSettings settings)
        {
            if (settings == null)
            {
                return false;
            }
            
            float maxX = (settings.gridWidth * settings.spacing) * 0.5f + BOUNDS_MARGIN;
            float maxZ = (settings.gridHeight * settings.spacing) * 0.5f + BOUNDS_MARGIN;
            float maxY = math.max(settings.primaryWave.amplitude, 
                         math.max(settings.secondaryWave.amplitude, settings.tertiaryWave.amplitude)) * 2f;
            
            return math.abs(worldPosition.x) <= maxX && 
                   math.abs(worldPosition.z) <= maxZ && 
                   math.abs(worldPosition.y) <= maxY;
        }
        
        /// <summary>
        /// Find the closest bubble to a world position.
        /// </summary>
        public int FindClosestBubble(float3 worldPosition, float maxDistance = float.MaxValue)
        {
            if (!_cachingEnabled)
            {
                return -1;
            }
            
            int closestIndex = -1;
            float closestDistance = maxDistance;
            
            for (int i = 0; i < _maxBubbles; i++)
            {
                if (_dirtyFlags[i])
                {
                    continue; // Skip dirty positions
                }
                
                float distance = math.distance(worldPosition, _cachedPositions[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }
            
            return closestIndex;
        }
        
        /// <summary>
        /// Get all bubbles within a specified radius of a position.
        /// </summary>
        public List<int> GetBubblesInRadius(float3 centerPosition, float radius)
        {
            var bubblesInRadius = new List<int>();
            
            if (!_cachingEnabled)
            {
                return bubblesInRadius;
            }
            
            float radiusSquared = radius * radius;
            
            for (int i = 0; i < _maxBubbles; i++)
            {
                if (_dirtyFlags[i])
                {
                    continue; // Skip dirty positions
                }
                
                float distanceSquared = math.distancesq(centerPosition, _cachedPositions[i]);
                if (distanceSquared <= radiusSquared)
                {
                    bubblesInRadius.Add(i);
                }
            }
            
            return bubblesInRadius;
        }
        
        /// <summary>
        /// Set the maximum number of bubbles this calculator can handle.
        /// </summary>
        public void SetMaxBubbles(int maxBubbles)
        {
            if (maxBubbles <= 0)
            {
                UnityEngine.Debug.LogError($"[BubblePositionCalculator] Invalid max bubbles: {maxBubbles}");
                return;
            }
            
            _maxBubbles = maxBubbles;
            _cachedPositions = new float3[maxBubbles];
            _dirtyFlags = new bool[maxBubbles];
            
            // Mark all as dirty initially
            MarkAllBubblesDirty();
            
            // Update stats
            _stats.MaxBubbles = maxBubbles;
            
            UnityEngine.Debug.Log($"[BubblePositionCalculator] Set max bubbles to {maxBubbles}");
        }
        
        /// <summary>
        /// Get the current maximum bubble count.
        /// </summary>
        public int GetMaxBubbles()
        {
            return _maxBubbles;
        }
        
        /// <summary>
        /// Get performance statistics for the position calculator.
        /// </summary>
        public BubblePositionCalculatorStats GetPerformanceStats()
        {
            // Update current stats
            _stats.CachedPositions = _cachingEnabled ? _cachedPositions.Length : 0;
            _stats.DirtyPositions = _dirtyFlags?.Count(d => d) ?? 0;
            
            return _stats;
        }
        
        /// <summary>
        /// Clear all cached positions and reset state.
        /// </summary>
        public void ClearCache()
        {
            if (_cachedPositions != null)
            {
                for (int i = 0; i < _cachedPositions.Length; i++)
                {
                    _cachedPositions[i] = float3.zero;
                }
            }
            
            MarkAllBubblesDirty();
            _spatialCache.Clear();
            _lastCacheTime = -1f;
            
            // Reset stats
            _stats = new BubblePositionCalculatorStats
            {
                MaxBubbles = _maxBubbles
            };
            
            UnityEngine.Debug.Log("[BubblePositionCalculator] Cache cleared");
        }
        
        /// <summary>
        /// Enable or disable position caching for performance optimization.
        /// </summary>
        public void SetCachingEnabled(bool enabled)
        {
            _cachingEnabled = enabled;
            
            if (!enabled)
            {
                ClearCache();
            }
            
            UnityEngine.Debug.Log($"[BubblePositionCalculator] Caching {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Get the current number of active (non-dirty) cached positions.
        /// </summary>
        public int GetCachedPositionCount()
        {
            if (!_cachingEnabled || _dirtyFlags == null)
            {
                return 0;
            }
            
            return _dirtyFlags.Count(d => !d);
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Update performance statistics after a calculation.
        /// </summary>
        private void UpdatePerformanceStats(int calculationCount, long elapsedMs)
        {
            _stats.TotalCalculations += calculationCount;
            _stats.LastBatchTimeMs = elapsedMs;
            
            // Update average calculation time (rolling average)
            if (_stats.TotalCalculations > 0)
            {
                float newAverage = elapsedMs / (float)calculationCount;
                _stats.AverageCalculationTimeMs = (_stats.AverageCalculationTimeMs * 0.9f) + (newAverage * 0.1f);
            }
        }
        
        #endregion
    }
}