using Unity.Mathematics;
using Unity.Collections;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Interface for high-performance wave system optimized for Quest 3 VR.
    /// Provides Burst-compiled mathematical operations and SIMD optimizations.
    /// Implements Requirement 5.2: Performance-optimized algorithms for Quest 3.
    /// Implements Requirement 5.3: Burst compilation validation.
    /// Implements Requirement 5.4: Performance stub testing with Unity profiler.
    /// </summary>
    public interface IPerformanceOptimizedWaveSystem
    {
        /// <summary>
        /// Calculate wave positions for multiple bubbles using Burst-compiled operations.
        /// </summary>
        /// <param name="gridIndices">Grid indices for bubble positions</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="distanceOffsets">AI confidence distance offsets</param>
        /// <param name="settings">Wave matrix settings</param>
        /// <param name="results">Output array for calculated positions</param>
        void CalculateWavePositionsBurst(NativeArray<int> gridIndices, float time, 
            NativeArray<float> distanceOffsets, WaveMatrixSettings settings, NativeArray<float3> results);
        
        /// <summary>
        /// Calculate wave positions using SIMD optimizations where possible.
        /// </summary>
        /// <param name="gridIndices">Grid indices for bubble positions</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="distanceOffsets">AI confidence distance offsets</param>
        /// <param name="settings">Wave matrix settings</param>
        /// <param name="results">Output array for calculated positions</param>
        void CalculateWavePositionsSIMD(NativeArray<int> gridIndices, float time,
            NativeArray<float> distanceOffsets, WaveMatrixSettings settings, NativeArray<float3> results);
        
        /// <summary>
        /// Batch calculate wave heights for multiple positions using optimized operations.
        /// </summary>
        /// <param name="positions">World positions to calculate heights for</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="settings">Wave matrix settings</param>
        /// <param name="results">Output array for calculated heights</param>
        void CalculateWaveHeightsBatch(NativeArray<float3> positions, float time,
            WaveMatrixSettings settings, NativeArray<float> results);
        
        /// <summary>
        /// Calculate wave normals for lighting and visual effects.
        /// </summary>
        /// <param name="positions">World positions to calculate normals for</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="settings">Wave matrix settings</param>
        /// <param name="results">Output array for calculated normals</param>
        void CalculateWaveNormalsBatch(NativeArray<float3> positions, float time,
            WaveMatrixSettings settings, NativeArray<float3> results);
        
        /// <summary>
        /// Update wave state using optimized mathematical operations.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        /// <param name="settings">Wave matrix settings</param>
        void UpdateWaveState(float deltaTime, WaveMatrixSettings settings);
        
        /// <summary>
        /// Validate that the system can achieve target performance on Quest 3.
        /// </summary>
        /// <param name="bubbleCount">Number of bubbles to test with</param>
        /// <param name="targetFrameTimeMs">Target frame time in milliseconds</param>
        /// <returns>Performance validation result</returns>
        WaveSystemPerformanceResult ValidateQuest3Performance(int bubbleCount, float targetFrameTimeMs);
        
        /// <summary>
        /// Get performance statistics for the optimized wave system.
        /// </summary>
        /// <returns>Performance statistics</returns>
        WaveSystemPerformanceStats GetPerformanceStats();
        
        /// <summary>
        /// Warm up the system by pre-compiling Burst jobs and initializing caches.
        /// </summary>
        void WarmUp();
        
        /// <summary>
        /// Clean up native arrays and dispose of resources.
        /// </summary>
        void Dispose();
    }
    
    /// <summary>
    /// Result of Quest 3 performance validation.
    /// </summary>
    public struct WaveSystemPerformanceResult
    {
        /// <summary>
        /// Whether the system meets Quest 3 performance targets.
        /// </summary>
        public bool MeetsPerformanceTarget;
        
        /// <summary>
        /// Actual frame time achieved in milliseconds.
        /// </summary>
        public float ActualFrameTimeMs;
        
        /// <summary>
        /// Target frame time in milliseconds.
        /// </summary>
        public float TargetFrameTimeMs;
        
        /// <summary>
        /// Performance margin (positive = under target, negative = over target).
        /// </summary>
        public float PerformanceMarginMs;
        
        /// <summary>
        /// Number of bubbles tested.
        /// </summary>
        public int BubbleCount;
        
        /// <summary>
        /// Whether Burst compilation is working correctly.
        /// </summary>
        public bool BurstCompilationActive;
        
        /// <summary>
        /// Whether SIMD optimizations are being used.
        /// </summary>
        public bool SIMDOptimizationsActive;
        
        /// <summary>
        /// Detailed performance breakdown.
        /// </summary>
        public string PerformanceBreakdown;
        
        /// <summary>
        /// Create a successful performance result.
        /// </summary>
        public static WaveSystemPerformanceResult Success(float actualMs, float targetMs, int bubbleCount)
        {
            return new WaveSystemPerformanceResult
            {
                MeetsPerformanceTarget = actualMs <= targetMs,
                ActualFrameTimeMs = actualMs,
                TargetFrameTimeMs = targetMs,
                PerformanceMarginMs = targetMs - actualMs,
                BubbleCount = bubbleCount,
                BurstCompilationActive = true,
                SIMDOptimizationsActive = true,
                PerformanceBreakdown = $"Processed {bubbleCount} bubbles in {actualMs:F2}ms (target: {targetMs:F2}ms)"
            };
        }
        
        /// <summary>
        /// Create a failed performance result.
        /// </summary>
        public static WaveSystemPerformanceResult Failure(float actualMs, float targetMs, int bubbleCount, string reason)
        {
            return new WaveSystemPerformanceResult
            {
                MeetsPerformanceTarget = false,
                ActualFrameTimeMs = actualMs,
                TargetFrameTimeMs = targetMs,
                PerformanceMarginMs = targetMs - actualMs,
                BubbleCount = bubbleCount,
                BurstCompilationActive = false,
                SIMDOptimizationsActive = false,
                PerformanceBreakdown = $"Failed: {reason}. Processed {bubbleCount} bubbles in {actualMs:F2}ms (target: {targetMs:F2}ms)"
            };
        }
    }
    
    /// <summary>
    /// Performance statistics for the optimized wave system.
    /// </summary>
    public struct WaveSystemPerformanceStats
    {
        /// <summary>
        /// Total number of wave calculations performed.
        /// </summary>
        public long TotalCalculations;
        
        /// <summary>
        /// Total number of Burst job executions.
        /// </summary>
        public long BurstJobExecutions;
        
        /// <summary>
        /// Total number of SIMD operations performed.
        /// </summary>
        public long SIMDOperations;
        
        /// <summary>
        /// Average calculation time per bubble in microseconds.
        /// </summary>
        public float AverageCalculationTimeMicroseconds;
        
        /// <summary>
        /// Peak calculation time per bubble in microseconds.
        /// </summary>
        public float PeakCalculationTimeMicroseconds;
        
        /// <summary>
        /// Current memory usage in bytes.
        /// </summary>
        public long MemoryUsageBytes;
        
        /// <summary>
        /// Number of native arrays currently allocated.
        /// </summary>
        public int AllocatedNativeArrays;
        
        /// <summary>
        /// Whether Burst compilation is currently active.
        /// </summary>
        public bool BurstActive;
        
        /// <summary>
        /// Whether SIMD optimizations are currently active.
        /// </summary>
        public bool SIMDActive;
        
        /// <summary>
        /// Last update timestamp.
        /// </summary>
        public float LastUpdateTime;
        
        /// <summary>
        /// Performance efficiency ratio (0.0 to 1.0, higher is better).
        /// </summary>
        public float EfficiencyRatio;
        
        /// <summary>
        /// Create default performance stats.
        /// </summary>
        public static WaveSystemPerformanceStats Default => new WaveSystemPerformanceStats
        {
            TotalCalculations = 0,
            BurstJobExecutions = 0,
            SIMDOperations = 0,
            AverageCalculationTimeMicroseconds = 0f,
            PeakCalculationTimeMicroseconds = 0f,
            MemoryUsageBytes = 0,
            AllocatedNativeArrays = 0,
            BurstActive = false,
            SIMDActive = false,
            LastUpdateTime = 0f,
            EfficiencyRatio = 0f
        };
    }
}