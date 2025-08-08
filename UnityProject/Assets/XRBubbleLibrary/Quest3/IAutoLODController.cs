using System;
using UnityEngine;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Interface for automatic Level of Detail (LOD) control system for Quest 3 performance optimization.
    /// Automatically adjusts rendering quality and bubble count when FPS drops below target thresholds.
    /// Implements Requirement 5.5: Automatic LOD system for performance maintenance.
    /// Implements Requirement 8.4: Auto-LOD controller for quality adjustment.
    /// </summary>
    public interface IAutoLODController
    {
        /// <summary>
        /// Whether the Auto-LOD controller is initialized and active.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Whether Auto-LOD adjustments are currently enabled.
        /// </summary>
        bool IsAutoLODEnabled { get; }
        
        /// <summary>
        /// Current LOD level being applied (0 = highest quality, higher = lower quality).
        /// </summary>
        int CurrentLODLevel { get; }
        
        /// <summary>
        /// Current Auto-LOD configuration settings.
        /// </summary>
        AutoLODConfiguration Configuration { get; }
        
        /// <summary>
        /// Initialize the Auto-LOD controller with the specified configuration.
        /// </summary>
        /// <param name="config">Auto-LOD configuration settings</param>
        /// <returns>True if initialization succeeded</returns>
        bool Initialize(AutoLODConfiguration config);
        
        /// <summary>
        /// Enable or disable automatic LOD adjustments.
        /// </summary>
        /// <param name="enabled">Whether to enable Auto-LOD</param>
        void SetAutoLODEnabled(bool enabled);
        
        /// <summary>
        /// Update the Auto-LOD system with current performance metrics.
        /// This should be called every frame to monitor performance.
        /// </summary>
        /// <param name="currentFPS">Current frame rate</param>
        /// <param name="frameTime">Current frame time in milliseconds</param>
        /// <param name="cpuUsage">Current CPU usage percentage (0-100)</param>
        /// <param name="gpuUsage">Current GPU usage percentage (0-100)</param>
        void UpdatePerformanceMetrics(float currentFPS, float frameTime, float cpuUsage, float gpuUsage);
        
        /// <summary>
        /// Manually set the LOD level (overrides automatic adjustments temporarily).
        /// </summary>
        /// <param name="lodLevel">Target LOD level (0 = highest quality)</param>
        /// <param name="duration">Duration to maintain manual override (0 = permanent until next auto adjustment)</param>
        void SetManualLODLevel(int lodLevel, float duration = 0f);
        
        /// <summary>
        /// Get the current performance status and LOD recommendations.
        /// </summary>
        /// <returns>Current Auto-LOD status information</returns>
        AutoLODStatus GetCurrentStatus();
        
        /// <summary>
        /// Get LOD adjustment recommendations based on current performance.
        /// </summary>
        /// <param name="targetFPS">Target frame rate to achieve</param>
        /// <returns>Recommended LOD adjustments</returns>
        AutoLODRecommendations GetLODRecommendations(float targetFPS = 72f);
        
        /// <summary>
        /// Apply a specific LOD configuration to the scene.
        /// </summary>
        /// <param name="lodSettings">LOD settings to apply</param>
        /// <returns>True if LOD settings were applied successfully</returns>
        bool ApplyLODSettings(LODSettings lodSettings);
        
        /// <summary>
        /// Get the current LOD settings being applied.
        /// </summary>
        /// <returns>Current LOD settings</returns>
        LODSettings GetCurrentLODSettings();
        
        /// <summary>
        /// Reset LOD to the highest quality level.
        /// </summary>
        void ResetToHighestQuality();
        
        /// <summary>
        /// Get performance history for LOD decision analysis.
        /// </summary>
        /// <returns>Recent performance data used for LOD decisions</returns>
        AutoLODPerformanceHistory GetPerformanceHistory();
        
        /// <summary>
        /// Configure bubble count adjustment parameters.
        /// </summary>
        /// <param name="bubbleConfig">Bubble count adjustment configuration</param>
        void ConfigureBubbleCountAdjustment(BubbleCountAdjustmentConfig bubbleConfig);
        
        /// <summary>
        /// Get the current bubble count adjustment status.
        /// </summary>
        /// <returns>Current bubble count and adjustment information</returns>
        BubbleCountStatus GetBubbleCountStatus();
        
        /// <summary>
        /// Event fired when LOD level changes automatically.
        /// </summary>
        event Action<AutoLODLevelChangedEventArgs> LODLevelChanged;
        
        /// <summary>
        /// Event fired when performance thresholds are crossed.
        /// </summary>
        event Action<AutoLODPerformanceEventArgs> PerformanceThresholdCrossed;
        
        /// <summary>
        /// Event fired when bubble count is adjusted.
        /// </summary>
        event Action<BubbleCountAdjustedEventArgs> BubbleCountAdjusted;
        
        /// <summary>
        /// Clean up Auto-LOD controller resources.
        /// </summary>
        void Dispose();
    }
    
    /// <summary>
    /// Configuration settings for the Auto-LOD controller.
    /// </summary>
    [Serializable]
    public struct AutoLODConfiguration
    {
        /// <summary>
        /// Target frame rate to maintain (typically 72 FPS for Quest 3).
        /// </summary>
        public float TargetFPS;
        
        /// <summary>
        /// Minimum acceptable frame rate before aggressive LOD reduction.
        /// </summary>
        public float MinimumFPS;
        
        /// <summary>
        /// Number of consecutive frames below target before LOD adjustment.
        /// </summary>
        public int FrameThresholdCount;
        
        /// <summary>
        /// Time in seconds to wait between LOD adjustments.
        /// </summary>
        public float AdjustmentCooldownSeconds;
        
        /// <summary>
        /// Whether to enable automatic bubble count reduction.
        /// </summary>
        public bool EnableBubbleCountAdjustment;
        
        /// <summary>
        /// Whether to enable automatic rendering quality adjustment.
        /// </summary>
        public bool EnableRenderingQualityAdjustment;
        
        /// <summary>
        /// Whether to enable automatic effect quality adjustment.
        /// </summary>
        public bool EnableEffectQualityAdjustment;
        
        /// <summary>
        /// Maximum LOD level to apply (prevents over-reduction of quality).
        /// </summary>
        public int MaxLODLevel;
        
        /// <summary>
        /// Aggressiveness of LOD adjustments (0.0 = conservative, 1.0 = aggressive).
        /// </summary>
        public float AdjustmentAggressiveness;
        
        /// <summary>
        /// Whether to log LOD adjustments for debugging.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Default configuration optimized for Quest 3.
        /// </summary>
        public static AutoLODConfiguration Quest3Default => new AutoLODConfiguration
        {
            TargetFPS = 72f,
            MinimumFPS = 60f,
            FrameThresholdCount = 5,
            AdjustmentCooldownSeconds = 2f,
            EnableBubbleCountAdjustment = true,
            EnableRenderingQualityAdjustment = true,
            EnableEffectQualityAdjustment = true,
            MaxLODLevel = 4,
            AdjustmentAggressiveness = 0.7f,
            EnableDebugLogging = true
        };
        
        /// <summary>
        /// Conservative configuration for stable performance.
        /// </summary>
        public static AutoLODConfiguration Conservative => new AutoLODConfiguration
        {
            TargetFPS = 72f,
            MinimumFPS = 65f,
            FrameThresholdCount = 10,
            AdjustmentCooldownSeconds = 5f,
            EnableBubbleCountAdjustment = true,
            EnableRenderingQualityAdjustment = false,
            EnableEffectQualityAdjustment = true,
            MaxLODLevel = 2,
            AdjustmentAggressiveness = 0.3f,
            EnableDebugLogging = false
        };
        
        /// <summary>
        /// Aggressive configuration for maximum performance maintenance.
        /// </summary>
        public static AutoLODConfiguration Aggressive => new AutoLODConfiguration
        {
            TargetFPS = 72f,
            MinimumFPS = 55f,
            FrameThresholdCount = 3,
            AdjustmentCooldownSeconds = 1f,
            EnableBubbleCountAdjustment = true,
            EnableRenderingQualityAdjustment = true,
            EnableEffectQualityAdjustment = true,
            MaxLODLevel = 6,
            AdjustmentAggressiveness = 1.0f,
            EnableDebugLogging = true
        };
    }
    
    /// <summary>
    /// Current status of the Auto-LOD system.
    /// </summary>
    public struct AutoLODStatus
    {
        /// <summary>
        /// Current LOD level being applied.
        /// </summary>
        public int CurrentLODLevel;
        
        /// <summary>
        /// Whether Auto-LOD is currently active.
        /// </summary>
        public bool IsActive;
        
        /// <summary>
        /// Current performance metrics.
        /// </summary>
        public AutoLODPerformanceMetrics CurrentMetrics;
        
        /// <summary>
        /// Time since last LOD adjustment.
        /// </summary>
        public float TimeSinceLastAdjustment;
        
        /// <summary>
        /// Number of consecutive frames below target FPS.
        /// </summary>
        public int ConsecutiveFramesBelowTarget;
        
        /// <summary>
        /// Whether manual LOD override is active.
        /// </summary>
        public bool IsManualOverrideActive;
        
        /// <summary>
        /// Time remaining for manual override.
        /// </summary>
        public float ManualOverrideTimeRemaining;
        
        /// <summary>
        /// Current LOD settings being applied.
        /// </summary>
        public LODSettings CurrentSettings;
        
        /// <summary>
        /// Performance trend over recent frames.
        /// </summary>
        public PerformanceTrend RecentTrend;
    }
    
    /// <summary>
    /// Performance metrics used by Auto-LOD system.
    /// </summary>
    public struct AutoLODPerformanceMetrics
    {
        /// <summary>
        /// Current frame rate.
        /// </summary>
        public float CurrentFPS;
        
        /// <summary>
        /// Average frame rate over recent frames.
        /// </summary>
        public float AverageFPS;
        
        /// <summary>
        /// Current frame time in milliseconds.
        /// </summary>
        public float FrameTimeMs;
        
        /// <summary>
        /// Current CPU usage percentage.
        /// </summary>
        public float CPUUsage;
        
        /// <summary>
        /// Current GPU usage percentage.
        /// </summary>
        public float GPUUsage;
        
        /// <summary>
        /// Frame rate stability (lower = more stable).
        /// </summary>
        public float FrameRateVariance;
        
        /// <summary>
        /// Performance score (0-100, higher = better).
        /// </summary>
        public float PerformanceScore;
        
        /// <summary>
        /// Timestamp of these metrics.
        /// </summary>
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// LOD adjustment recommendations.
    /// </summary>
    public struct AutoLODRecommendations
    {
        /// <summary>
        /// Recommended LOD level to achieve target performance.
        /// </summary>
        public int RecommendedLODLevel;
        
        /// <summary>
        /// Confidence in the recommendation (0.0 to 1.0).
        /// </summary>
        public float Confidence;
        
        /// <summary>
        /// Specific adjustments recommended.
        /// </summary>
        public LODAdjustmentType[] RecommendedAdjustments;
        
        /// <summary>
        /// Expected performance improvement from recommendations.
        /// </summary>
        public float ExpectedFPSImprovement;
        
        /// <summary>
        /// Quality impact assessment.
        /// </summary>
        public QualityImpactAssessment QualityImpact;
        
        /// <summary>
        /// Reasoning for the recommendations.
        /// </summary>
        public string RecommendationReasoning;
    }
    
    /// <summary>
    /// Specific LOD settings to apply.
    /// </summary>
    [Serializable]
    public struct LODSettings
    {
        /// <summary>
        /// LOD level (0 = highest quality).
        /// </summary>
        public int LODLevel;
        
        /// <summary>
        /// Target bubble count for this LOD level.
        /// </summary>
        public int BubbleCount;
        
        /// <summary>
        /// Rendering quality level (0.0 to 1.0).
        /// </summary>
        public float RenderingQuality;
        
        /// <summary>
        /// Effect quality level (0.0 to 1.0).
        /// </summary>
        public float EffectQuality;
        
        /// <summary>
        /// Texture quality level (0.0 to 1.0).
        /// </summary>
        public float TextureQuality;
        
        /// <summary>
        /// Shadow quality level (0.0 to 1.0).
        /// </summary>
        public float ShadowQuality;
        
        /// <summary>
        /// Anti-aliasing quality level (0.0 to 1.0).
        /// </summary>
        public float AntiAliasingQuality;
        
        /// <summary>
        /// Post-processing quality level (0.0 to 1.0).
        /// </summary>
        public float PostProcessingQuality;
        
        /// <summary>
        /// Whether to enable dynamic batching.
        /// </summary>
        public bool EnableDynamicBatching;
        
        /// <summary>
        /// Whether to enable GPU instancing.
        /// </summary>
        public bool EnableGPUInstancing;
        
        /// <summary>
        /// Maximum draw distance for objects.
        /// </summary>
        public float MaxDrawDistance;
        
        /// <summary>
        /// Highest quality LOD settings.
        /// </summary>
        public static LODSettings HighestQuality => new LODSettings
        {
            LODLevel = 0,
            BubbleCount = 100,
            RenderingQuality = 1.0f,
            EffectQuality = 1.0f,
            TextureQuality = 1.0f,
            ShadowQuality = 1.0f,
            AntiAliasingQuality = 1.0f,
            PostProcessingQuality = 1.0f,
            EnableDynamicBatching = true,
            EnableGPUInstancing = true,
            MaxDrawDistance = 1000f
        };
        
        /// <summary>
        /// Medium quality LOD settings.
        /// </summary>
        public static LODSettings MediumQuality => new LODSettings
        {
            LODLevel = 2,
            BubbleCount = 75,
            RenderingQuality = 0.8f,
            EffectQuality = 0.7f,
            TextureQuality = 0.8f,
            ShadowQuality = 0.6f,
            AntiAliasingQuality = 0.7f,
            PostProcessingQuality = 0.8f,
            EnableDynamicBatching = true,
            EnableGPUInstancing = true,
            MaxDrawDistance = 750f
        };
        
        /// <summary>
        /// Performance-focused LOD settings.
        /// </summary>
        public static LODSettings PerformanceFocused => new LODSettings
        {
            LODLevel = 4,
            BubbleCount = 50,
            RenderingQuality = 0.6f,
            EffectQuality = 0.5f,
            TextureQuality = 0.6f,
            ShadowQuality = 0.3f,
            AntiAliasingQuality = 0.4f,
            PostProcessingQuality = 0.5f,
            EnableDynamicBatching = true,
            EnableGPUInstancing = true,
            MaxDrawDistance = 500f
        };
    }
    
    /// <summary>
    /// Performance history for LOD decision making.
    /// </summary>
    public struct AutoLODPerformanceHistory
    {
        /// <summary>
        /// Recent performance samples.
        /// </summary>
        public AutoLODPerformanceMetrics[] RecentSamples;
        
        /// <summary>
        /// Average FPS over the history period.
        /// </summary>
        public float AverageFPS;
        
        /// <summary>
        /// Minimum FPS recorded in history.
        /// </summary>
        public float MinimumFPS;
        
        /// <summary>
        /// Maximum FPS recorded in history.
        /// </summary>
        public float MaximumFPS;
        
        /// <summary>
        /// FPS standard deviation (stability measure).
        /// </summary>
        public float FPSStandardDeviation;
        
        /// <summary>
        /// Number of LOD adjustments made.
        /// </summary>
        public int LODAdjustmentCount;
        
        /// <summary>
        /// History of LOD level changes.
        /// </summary>
        public LODLevelChange[] LODHistory;
        
        /// <summary>
        /// Time span covered by this history.
        /// </summary>
        public TimeSpan HistoryTimeSpan;
    }
    
    /// <summary>
    /// Configuration for bubble count adjustment.
    /// </summary>
    [Serializable]
    public struct BubbleCountAdjustmentConfig
    {
        /// <summary>
        /// Maximum number of bubbles allowed.
        /// </summary>
        public int MaxBubbleCount;
        
        /// <summary>
        /// Minimum number of bubbles to maintain.
        /// </summary>
        public int MinBubbleCount;
        
        /// <summary>
        /// Number of bubbles to reduce per LOD level.
        /// </summary>
        public int BubblesPerLODLevel;
        
        /// <summary>
        /// Whether to prioritize central bubbles when reducing count.
        /// </summary>
        public bool PrioritizeCentralBubbles;
        
        /// <summary>
        /// Smoothing factor for bubble count changes (0.0 = instant, 1.0 = very smooth).
        /// </summary>
        public float CountChangeSmoothingFactor;
        
        /// <summary>
        /// Default bubble count adjustment configuration.
        /// </summary>
        public static BubbleCountAdjustmentConfig Default => new BubbleCountAdjustmentConfig
        {
            MaxBubbleCount = 100,
            MinBubbleCount = 25,
            BubblesPerLODLevel = 15,
            PrioritizeCentralBubbles = true,
            CountChangeSmoothingFactor = 0.3f
        };
    }
    
    /// <summary>
    /// Current bubble count status.
    /// </summary>
    public struct BubbleCountStatus
    {
        /// <summary>
        /// Current active bubble count.
        /// </summary>
        public int CurrentBubbleCount;
        
        /// <summary>
        /// Target bubble count for current LOD level.
        /// </summary>
        public int TargetBubbleCount;
        
        /// <summary>
        /// Maximum possible bubble count.
        /// </summary>
        public int MaxBubbleCount;
        
        /// <summary>
        /// Number of bubbles reduced from maximum.
        /// </summary>
        public int BubblesReduced;
        
        /// <summary>
        /// Whether bubble count is currently being adjusted.
        /// </summary>
        public bool IsAdjusting;
        
        /// <summary>
        /// Time since last bubble count change.
        /// </summary>
        public float TimeSinceLastChange;
    }
    
    /// <summary>
    /// Types of LOD adjustments that can be made.
    /// </summary>
    public enum LODAdjustmentType
    {
        ReduceBubbleCount,
        IncreaseRenderingQuality,
        DecreaseRenderingQuality,
        ReduceEffectQuality,
        ReduceShadowQuality,
        DisablePostProcessing,
        ReduceTextureQuality,
        ReduceDrawDistance,
        DisableDynamicBatching
    }
    
    /// <summary>
    /// Assessment of quality impact from LOD changes.
    /// </summary>
    public struct QualityImpactAssessment
    {
        /// <summary>
        /// Overall visual quality impact (0.0 = no impact, 1.0 = severe impact).
        /// </summary>
        public float OverallImpact;
        
        /// <summary>
        /// Impact on bubble visual quality.
        /// </summary>
        public float BubbleQualityImpact;
        
        /// <summary>
        /// Impact on lighting and shadows.
        /// </summary>
        public float LightingImpact;
        
        /// <summary>
        /// Impact on effects and post-processing.
        /// </summary>
        public float EffectsImpact;
        
        /// <summary>
        /// User-noticeable quality changes.
        /// </summary>
        public string[] NoticeableChanges;
    }
    
    /// <summary>
    /// Performance trend analysis.
    /// </summary>
    public enum PerformanceTrend
    {
        Stable,
        Improving,
        Degrading,
        Volatile,
        Unknown
    }
    
    /// <summary>
    /// Record of LOD level change.
    /// </summary>
    public struct LODLevelChange
    {
        /// <summary>
        /// Previous LOD level.
        /// </summary>
        public int PreviousLevel;
        
        /// <summary>
        /// New LOD level.
        /// </summary>
        public int NewLevel;
        
        /// <summary>
        /// Timestamp of the change.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Reason for the change.
        /// </summary>
        public string Reason;
        
        /// <summary>
        /// Performance metrics at time of change.
        /// </summary>
        public AutoLODPerformanceMetrics MetricsAtChange;
    }
    
    /// <summary>
    /// Event arguments for LOD level changes.
    /// </summary>
    public struct AutoLODLevelChangedEventArgs
    {
        /// <summary>
        /// Previous LOD level.
        /// </summary>
        public int PreviousLevel;
        
        /// <summary>
        /// New LOD level.
        /// </summary>
        public int NewLevel;
        
        /// <summary>
        /// Reason for the change.
        /// </summary>
        public string Reason;
        
        /// <summary>
        /// New LOD settings applied.
        /// </summary>
        public LODSettings NewSettings;
        
        /// <summary>
        /// Performance metrics that triggered the change.
        /// </summary>
        public AutoLODPerformanceMetrics TriggeringMetrics;
        
        /// <summary>
        /// Timestamp of the change.
        /// </summary>
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Event arguments for performance threshold events.
    /// </summary>
    public struct AutoLODPerformanceEventArgs
    {
        /// <summary>
        /// Type of threshold that was crossed.
        /// </summary>
        public PerformanceThresholdType ThresholdType;
        
        /// <summary>
        /// Current performance metrics.
        /// </summary>
        public AutoLODPerformanceMetrics CurrentMetrics;
        
        /// <summary>
        /// Threshold value that was crossed.
        /// </summary>
        public float ThresholdValue;
        
        /// <summary>
        /// Whether threshold was crossed upward or downward.
        /// </summary>
        public bool CrossedUpward;
        
        /// <summary>
        /// Timestamp of the event.
        /// </summary>
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Event arguments for bubble count adjustments.
    /// </summary>
    public struct BubbleCountAdjustedEventArgs
    {
        /// <summary>
        /// Previous bubble count.
        /// </summary>
        public int PreviousCount;
        
        /// <summary>
        /// New bubble count.
        /// </summary>
        public int NewCount;
        
        /// <summary>
        /// Reason for the adjustment.
        /// </summary>
        public string Reason;
        
        /// <summary>
        /// LOD level that triggered the adjustment.
        /// </summary>
        public int TriggeringLODLevel;
        
        /// <summary>
        /// Timestamp of the adjustment.
        /// </summary>
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Types of performance thresholds.
    /// </summary>
    public enum PerformanceThresholdType
    {
        TargetFPS,
        MinimumFPS,
        CPUUsage,
        GPUUsage,
        FrameTime
    }
}