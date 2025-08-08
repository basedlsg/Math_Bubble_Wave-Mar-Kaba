using UnityEngine;
using Unity.Mathematics;
using System;

namespace XRBubbleLibrary.Demo
{
    /// <summary>
    /// Interface for managing bubble instantiation and lifecycle in the demo scene.
    /// Handles exactly 100 bubbles with efficient pooling and performance optimization.
    /// Implements Requirement 5.1: Demonstrate wave mathematics with exactly 100 bubbles.
    /// Implements Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public interface IBubbleManagementSystem : IDisposable
    {
        /// <summary>
        /// Current number of active bubbles in the scene.
        /// </summary>
        int ActiveBubbleCount { get; }
        
        /// <summary>
        /// Target number of bubbles for the demo.
        /// </summary>
        int TargetBubbleCount { get; }
        
        /// <summary>
        /// Whether the bubble management system is initialized.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Whether the demo is currently running.
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// Initialize the bubble management system.
        /// </summary>
        /// <param name="targetCount">Target number of bubbles (should be 100)</param>
        /// <param name="container">Parent transform for bubble objects</param>
        /// <returns>True if initialization succeeded</returns>
        bool Initialize(int targetCount, Transform container);
        
        /// <summary>
        /// Start the bubble demo with wave animations.
        /// </summary>
        void StartDemo();
        
        /// <summary>
        /// Stop the bubble demo.
        /// </summary>
        void StopDemo();
        
        /// <summary>
        /// Reset all bubbles to initial positions.
        /// </summary>
        void ResetBubbles();
        
        /// <summary>
        /// Update bubble positions based on wave mathematics.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        void UpdateBubblePositions(float deltaTime);
        
        /// <summary>
        /// Validate that exactly the target number of bubbles are active.
        /// </summary>
        /// <returns>Validation result</returns>
        BubbleCountValidationResult ValidateBubbleCount();
        
        /// <summary>
        /// Get performance statistics for the bubble management system.
        /// </summary>
        /// <returns>Performance statistics</returns>
        BubbleManagementStats GetPerformanceStats();
        
        /// <summary>
        /// Get all active bubble transforms for external processing.
        /// </summary>
        /// <returns>Array of active bubble transforms</returns>
        Transform[] GetActiveBubbleTransforms();
        
        /// <summary>
        /// Get bubble data for wave mathematics calculations.
        /// </summary>
        /// <returns>Array of bubble data structures</returns>
        BubbleData[] GetBubbleData();
        
        /// <summary>
        /// Set bubble positions from wave mathematics results.
        /// </summary>
        /// <param name="positions">Array of calculated positions</param>
        void SetBubblePositions(float3[] positions);
        
        /// <summary>
        /// Enable or disable bubble pooling for performance optimization.
        /// </summary>
        /// <param name="enabled">Whether to enable pooling</param>
        void SetPoolingEnabled(bool enabled);
        
        /// <summary>
        /// Configure bubble visual properties.
        /// </summary>
        /// <param name="config">Visual configuration</param>
        void ConfigureBubbleVisuals(BubbleVisualConfig config);
    }
    
    /// <summary>
    /// Result of bubble count validation.
    /// </summary>
    public struct BubbleCountValidationResult
    {
        /// <summary>
        /// Whether the bubble count is exactly as expected.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// Expected bubble count.
        /// </summary>
        public int ExpectedCount;
        
        /// <summary>
        /// Actual bubble count.
        /// </summary>
        public int ActualCount;
        
        /// <summary>
        /// Difference between expected and actual count.
        /// </summary>
        public int CountDifference;
        
        /// <summary>
        /// Validation message.
        /// </summary>
        public string Message;
        
        /// <summary>
        /// Time when validation was performed.
        /// </summary>
        public float ValidationTime;
        
        /// <summary>
        /// Create a successful validation result.
        /// </summary>
        public static BubbleCountValidationResult Success(int count)
        {
            return new BubbleCountValidationResult
            {
                IsValid = true,
                ExpectedCount = count,
                ActualCount = count,
                CountDifference = 0,
                Message = $"Bubble count validation passed: {count} bubbles active",
                ValidationTime = Time.time
            };
        }
        
        /// <summary>
        /// Create a failed validation result.
        /// </summary>
        public static BubbleCountValidationResult Failure(int expected, int actual)
        {
            return new BubbleCountValidationResult
            {
                IsValid = false,
                ExpectedCount = expected,
                ActualCount = actual,
                CountDifference = actual - expected,
                Message = $"Bubble count validation failed: expected {expected}, got {actual}",
                ValidationTime = Time.time
            };
        }
    }
    
    /// <summary>
    /// Performance statistics for bubble management.
    /// </summary>
    public struct BubbleManagementStats
    {
        /// <summary>
        /// Total number of bubble updates performed.
        /// </summary>
        public long TotalUpdates;
        
        /// <summary>
        /// Average update time per bubble in microseconds.
        /// </summary>
        public float AverageUpdateTimeMicroseconds;
        
        /// <summary>
        /// Peak update time per bubble in microseconds.
        /// </summary>
        public float PeakUpdateTimeMicroseconds;
        
        /// <summary>
        /// Number of bubbles currently pooled.
        /// </summary>
        public int PooledBubbles;
        
        /// <summary>
        /// Number of bubble instantiations performed.
        /// </summary>
        public int TotalInstantiations;
        
        /// <summary>
        /// Number of bubble destructions performed.
        /// </summary>
        public int TotalDestructions;
        
        /// <summary>
        /// Current memory usage in bytes.
        /// </summary>
        public long MemoryUsageBytes;
        
        /// <summary>
        /// Whether pooling is currently enabled.
        /// </summary>
        public bool PoolingEnabled;
        
        /// <summary>
        /// Last update time.
        /// </summary>
        public float LastUpdateTime;
        
        /// <summary>
        /// Pool efficiency ratio (0.0 to 1.0, higher is better).
        /// </summary>
        public float PoolEfficiency;
    }
    
    /// <summary>
    /// Visual configuration for bubbles.
    /// </summary>
    [System.Serializable]
    public struct BubbleVisualConfig
    {
        /// <summary>
        /// Base bubble scale.
        /// </summary>
        public float BaseScale;
        
        /// <summary>
        /// Scale variation range (0.0 to 1.0).
        /// </summary>
        public float ScaleVariation;
        
        /// <summary>
        /// Base bubble color.
        /// </summary>
        public Color BaseColor;
        
        /// <summary>
        /// Color variation intensity (0.0 to 1.0).
        /// </summary>
        public float ColorVariation;
        
        /// <summary>
        /// Bubble transparency (0.0 to 1.0).
        /// </summary>
        public float Alpha;
        
        /// <summary>
        /// Whether to enable bubble rotation.
        /// </summary>
        public bool EnableRotation;
        
        /// <summary>
        /// Rotation speed multiplier.
        /// </summary>
        public float RotationSpeed;
        
        /// <summary>
        /// Whether to enable scale pulsing based on wave height.
        /// </summary>
        public bool EnableScalePulsing;
        
        /// <summary>
        /// Scale pulsing intensity.
        /// </summary>
        public float PulsingIntensity;
        
        /// <summary>
        /// Default bubble visual configuration optimized for Quest 3.
        /// </summary>
        public static BubbleVisualConfig Quest3Default => new BubbleVisualConfig
        {
            BaseScale = 0.5f,
            ScaleVariation = 0.2f,
            BaseColor = new Color(0.3f, 0.7f, 1.0f, 0.8f), // Light blue
            ColorVariation = 0.3f,
            Alpha = 0.8f,
            EnableRotation = true,
            RotationSpeed = 0.5f,
            EnableScalePulsing = true,
            PulsingIntensity = 0.1f
        };
        
        /// <summary>
        /// Performance-optimized configuration with minimal visual effects.
        /// </summary>
        public static BubbleVisualConfig PerformanceOptimized => new BubbleVisualConfig
        {
            BaseScale = 0.4f,
            ScaleVariation = 0.1f,
            BaseColor = new Color(0.5f, 0.8f, 1.0f, 0.7f),
            ColorVariation = 0.1f,
            Alpha = 0.7f,
            EnableRotation = false,
            RotationSpeed = 0f,
            EnableScalePulsing = false,
            PulsingIntensity = 0f
        };
    }
}