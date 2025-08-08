using Unity.Mathematics;
using System.Collections.Generic;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Interface for efficient bubble position calculation and management.
    /// Provides high-level position management for 100+ bubbles with Quest 3 optimization.
    /// Implements Requirement 5.1: Efficient algorithms for 100+ bubble positioning.
    /// Implements Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public interface IBubblePositionCalculator
    {
        /// <summary>
        /// Calculate positions for all active bubbles.
        /// </summary>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="settings">Wave matrix settings to use</param>
        /// <returns>Array of bubble positions</returns>
        float3[] CalculateAllPositions(float time, WaveMatrixSettings settings);
        
        /// <summary>
        /// Calculate positions for specific bubbles by index.
        /// </summary>
        /// <param name="bubbleIndices">Indices of bubbles to calculate</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="settings">Wave matrix settings to use</param>
        /// <returns>Array of positions corresponding to the input indices</returns>
        float3[] CalculatePositions(int[] bubbleIndices, float time, WaveMatrixSettings settings);
        
        /// <summary>
        /// Calculate positions for bubbles with AI distance offsets.
        /// </summary>
        /// <param name="bubbleData">Array of bubble data including indices and AI distances</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="settings">Wave matrix settings to use</param>
        /// <returns>Array of positions with AI distance offsets applied</returns>
        float3[] CalculatePositionsWithAI(BubbleData[] bubbleData, float time, WaveMatrixSettings settings);
        
        /// <summary>
        /// Update bubble positions incrementally (only dirty bubbles).
        /// </summary>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="settings">Wave matrix settings to use</param>
        /// <returns>Number of positions updated</returns>
        int UpdateDirtyPositions(float time, WaveMatrixSettings settings);
        
        /// <summary>
        /// Get cached position for a specific bubble.
        /// </summary>
        /// <param name="bubbleIndex">Index of the bubble</param>
        /// <returns>Cached position, or zero if not found</returns>
        float3 GetCachedPosition(int bubbleIndex);
        
        /// <summary>
        /// Set a bubble position as dirty (needs recalculation).
        /// </summary>
        /// <param name="bubbleIndex">Index of the bubble to mark dirty</param>
        void MarkBubbleDirty(int bubbleIndex);
        
        /// <summary>
        /// Mark all bubble positions as dirty.
        /// </summary>
        void MarkAllBubblesDirty();
        
        /// <summary>
        /// Validate bubble positions against bounds and constraints.
        /// </summary>
        /// <param name="positions">Positions to validate</param>
        /// <param name="settings">Settings containing bounds information</param>
        /// <returns>Validation result with any issues found</returns>
        BubblePositionValidationResult ValidatePositions(float3[] positions, WaveMatrixSettings settings);
        
        /// <summary>
        /// Check if a world position is within valid bubble bounds.
        /// </summary>
        /// <param name="worldPosition">Position to check</param>
        /// <param name="settings">Settings containing bounds information</param>
        /// <returns>True if position is within bounds</returns>
        bool IsPositionInBounds(float3 worldPosition, WaveMatrixSettings settings);
        
        /// <summary>
        /// Find the closest bubble to a world position.
        /// </summary>
        /// <param name="worldPosition">Target position</param>
        /// <param name="maxDistance">Maximum search distance</param>
        /// <returns>Index of closest bubble, or -1 if none found within distance</returns>
        int FindClosestBubble(float3 worldPosition, float maxDistance = float.MaxValue);
        
        /// <summary>
        /// Get all bubbles within a specified radius of a position.
        /// </summary>
        /// <param name="centerPosition">Center of search area</param>
        /// <param name="radius">Search radius</param>
        /// <returns>List of bubble indices within the radius</returns>
        List<int> GetBubblesInRadius(float3 centerPosition, float radius);
        
        /// <summary>
        /// Set the maximum number of bubbles this calculator can handle.
        /// </summary>
        /// <param name="maxBubbles">Maximum bubble count</param>
        void SetMaxBubbles(int maxBubbles);
        
        /// <summary>
        /// Get the current maximum bubble count.
        /// </summary>
        /// <returns>Maximum number of bubbles</returns>
        int GetMaxBubbles();
        
        /// <summary>
        /// Get performance statistics for the position calculator.
        /// </summary>
        /// <returns>Performance statistics</returns>
        BubblePositionCalculatorStats GetPerformanceStats();
        
        /// <summary>
        /// Clear all cached positions and reset state.
        /// </summary>
        void ClearCache();
        
        /// <summary>
        /// Enable or disable position caching for performance optimization.
        /// </summary>
        /// <param name="enabled">Whether to enable caching</param>
        void SetCachingEnabled(bool enabled);
        
        /// <summary>
        /// Get the current number of active (non-dirty) cached positions.
        /// </summary>
        /// <returns>Number of cached positions</returns>
        int GetCachedPositionCount();
    }
    
    /// <summary>
    /// Data structure for bubble information including AI distance.
    /// </summary>
    public struct BubbleData
    {
        /// <summary>
        /// Index of the bubble in the grid.
        /// </summary>
        public int Index;
        
        /// <summary>
        /// AI confidence distance offset.
        /// </summary>
        public float AIDistance;
        
        /// <summary>
        /// Optional priority for calculation ordering.
        /// </summary>
        public int Priority;
        
        /// <summary>
        /// Whether this bubble is currently active.
        /// </summary>
        public bool IsActive;
        
        /// <summary>
        /// Create bubble data with index and AI distance.
        /// </summary>
        public static BubbleData Create(int index, float aiDistance = 0f, int priority = 0, bool isActive = true)
        {
            return new BubbleData
            {
                Index = index,
                AIDistance = aiDistance,
                Priority = priority,
                IsActive = isActive
            };
        }
    }
    
    /// <summary>
    /// Result of bubble position validation.
    /// </summary>
    public struct BubblePositionValidationResult
    {
        /// <summary>
        /// Whether all positions are valid.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// List of validation issues found.
        /// </summary>
        public string[] Issues;
        
        /// <summary>
        /// List of positions that are out of bounds.
        /// </summary>
        public int[] OutOfBoundsIndices;
        
        /// <summary>
        /// List of positions that have invalid values (NaN, Infinity).
        /// </summary>
        public int[] InvalidValueIndices;
        
        /// <summary>
        /// Performance impact of the current position configuration.
        /// </summary>
        public float PerformanceImpact;
        
        /// <summary>
        /// Create a valid result with no issues.
        /// </summary>
        public static BubblePositionValidationResult Valid => new BubblePositionValidationResult
        {
            IsValid = true,
            Issues = new string[0],
            OutOfBoundsIndices = new int[0],
            InvalidValueIndices = new int[0],
            PerformanceImpact = 0f
        };
        
        /// <summary>
        /// Create an invalid result with specified issues.
        /// </summary>
        public static BubblePositionValidationResult Invalid(params string[] issues) => new BubblePositionValidationResult
        {
            IsValid = false,
            Issues = issues,
            OutOfBoundsIndices = new int[0],
            InvalidValueIndices = new int[0],
            PerformanceImpact = 1f
        };
    }
    
    /// <summary>
    /// Performance statistics for the bubble position calculator.
    /// </summary>
    public struct BubblePositionCalculatorStats
    {
        /// <summary>
        /// Total number of position calculations performed.
        /// </summary>
        public long TotalCalculations;
        
        /// <summary>
        /// Number of cache hits (positions retrieved from cache).
        /// </summary>
        public long CacheHits;
        
        /// <summary>
        /// Number of cache misses (positions calculated fresh).
        /// </summary>
        public long CacheMisses;
        
        /// <summary>
        /// Current number of cached positions.
        /// </summary>
        public int CachedPositions;
        
        /// <summary>
        /// Maximum number of bubbles configured.
        /// </summary>
        public int MaxBubbles;
        
        /// <summary>
        /// Average calculation time per bubble in milliseconds.
        /// </summary>
        public float AverageCalculationTimeMs;
        
        /// <summary>
        /// Last batch calculation time in milliseconds.
        /// </summary>
        public float LastBatchTimeMs;
        
        /// <summary>
        /// Number of dirty positions that need recalculation.
        /// </summary>
        public int DirtyPositions;
        
        /// <summary>
        /// Cache hit rate as a percentage (0-100).
        /// </summary>
        public float CacheHitRate => TotalCalculations > 0 ? (CacheHits / (float)TotalCalculations) * 100f : 0f;
        
        /// <summary>
        /// Calculate efficiency score (higher is better).
        /// </summary>
        public float EfficiencyScore => CacheHitRate * 0.7f + (MaxBubbles > 0 ? (CachedPositions / (float)MaxBubbles) * 30f : 0f);
    }
}