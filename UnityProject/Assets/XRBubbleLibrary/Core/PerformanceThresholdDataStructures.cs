using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Configuration for performance thresholds across different metrics
    /// </summary>
    [Serializable]
    public class PerformanceThresholds
    {
        [Header("Frame Rate Thresholds")]
        [Tooltip("Minimum acceptable FPS")]
        public float MinimumFPS = 72f;
        
        [Tooltip("Maximum acceptable frame time in milliseconds")]
        public float MaximumFrameTime = 13.89f; // 72 FPS = 13.89ms
        
        [Header("Memory Thresholds")]
        [Tooltip("Maximum memory usage in MB")]
        public long MaximumMemoryUsageMB = 512;
        
        [Tooltip("Memory usage warning threshold in MB")]
        public long MemoryWarningThresholdMB = 400;
        
        [Header("CPU/GPU Thresholds")]
        [Tooltip("Maximum CPU usage percentage")]
        public float MaximumCPUUsage = 80f;
        
        [Tooltip("Maximum GPU usage percentage")]
        public float MaximumGPUUsage = 85f;
        
        [Header("Thermal Thresholds")]
        [Tooltip("Maximum thermal state (0-1 scale)")]
        public float MaximumThermalState = 0.8f;
        
        [Header("Headroom Configuration")]
        [Tooltip("Required performance headroom percentage (30% = 0.3)")]
        public float RequiredHeadroomPercentage = 0.3f;
        
        [Header("Violation Tolerance")]
        [Tooltip("Number of consecutive violations before triggering action")]
        public int ViolationTolerance = 3;
        
        [Tooltip("Time window for violation counting in seconds")]
        public float ViolationTimeWindow = 5f;
        
        /// <summary>
        /// Validates that all threshold values are reasonable
        /// </summary>
        public bool IsValid()
        {
            return MinimumFPS > 0 &&
                   MaximumFrameTime > 0 &&
                   MaximumMemoryUsageMB > 0 &&
                   MemoryWarningThresholdMB > 0 &&
                   MemoryWarningThresholdMB <= MaximumMemoryUsageMB &&
                   MaximumCPUUsage > 0 && MaximumCPUUsage <= 100 &&
                   MaximumGPUUsage > 0 && MaximumGPUUsage <= 100 &&
                   MaximumThermalState > 0 && MaximumThermalState <= 1 &&
                   RequiredHeadroomPercentage >= 0 && RequiredHeadroomPercentage < 1 &&
                   ViolationTolerance > 0 &&
                   ViolationTimeWindow > 0;
        }
        
        /// <summary>
        /// Creates a copy of the current thresholds
        /// </summary>
        public PerformanceThresholds Clone()
        {
            return new PerformanceThresholds
            {
                MinimumFPS = MinimumFPS,
                MaximumFrameTime = MaximumFrameTime,
                MaximumMemoryUsageMB = MaximumMemoryUsageMB,
                MemoryWarningThresholdMB = MemoryWarningThresholdMB,
                MaximumCPUUsage = MaximumCPUUsage,
                MaximumGPUUsage = MaximumGPUUsage,
                MaximumThermalState = MaximumThermalState,
                RequiredHeadroomPercentage = RequiredHeadroomPercentage,
                ViolationTolerance = ViolationTolerance,
                ViolationTimeWindow = ViolationTimeWindow
            };
        }
    }
    
    /// <summary>
    /// Types of performance metrics that can be monitored
    /// </summary>
    public enum PerformanceMetricType
    {
        FPS,
        FrameTime,
        MemoryUsage,
        CPUUsage,
        GPUUsage,
        ThermalState
    }
    
    /// <summary>
    /// Result of threshold validation with details of any violations
    /// </summary>
    public class ThresholdValidationResult
    {
        public bool IsValid { get; set; }
        public List<ThresholdViolation> Violations { get; set; } = new List<ThresholdViolation>();
        public float OverallHealthScore { get; set; } // 0-1 scale
        public string Summary { get; set; }
        public DateTime ValidatedAt { get; set; }
        
        public bool HasCriticalViolations => Violations.Exists(v => v.Severity == ViolationSeverity.Critical);
        public bool HasWarningViolations => Violations.Exists(v => v.Severity == ViolationSeverity.Warning);
    }
    
    /// <summary>
    /// Details of a specific threshold violation
    /// </summary>
    public class ThresholdViolation
    {
        public PerformanceMetricType MetricType { get; set; }
        public float CurrentValue { get; set; }
        public float ThresholdValue { get; set; }
        public float ViolationAmount { get; set; }
        public ViolationSeverity Severity { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Severity levels for threshold violations
    /// </summary>
    public enum ViolationSeverity
    {
        Info,       // Minor deviation, informational only
        Warning,    // Approaching threshold, should be monitored
        Critical    // Threshold exceeded, immediate action required
    }
    
    /// <summary>
    /// Recommendations for threshold adjustments based on historical data
    /// </summary>
    public class ThresholdRecommendations
    {
        public Dictionary<PerformanceMetricType, float> RecommendedThresholds { get; set; } = new Dictionary<PerformanceMetricType, float>();
        public string ReasoningReport { get; set; }
        public float ConfidenceScore { get; set; } // 0-1 scale
        public DateTime GeneratedAt { get; set; }
        public int DataPointsAnalyzed { get; set; }
    }
    
    /// <summary>
    /// Configuration for threshold violation callbacks
    /// </summary>
    public class ViolationCallbackConfig
    {
        public ViolationSeverity MinimumSeverity { get; set; } = ViolationSeverity.Warning;
        public bool EnableBatching { get; set; } = true;
        public float BatchingWindowSeconds { get; set; } = 1f;
        public int MaxCallbacksPerSecond { get; set; } = 10;
    }
}