using System;
using System.Collections.Generic;
using UnityEngine;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Data structures for comfort validation feedback loop system.
    /// </summary>
    
    /// <summary>
    /// Configuration for comfort validation feedback loop.
    /// </summary>
    [Serializable]
    public class ComfortFeedbackConfiguration
    {
        [Header("Feedback Loop Settings")]
        [Tooltip("Whether to enable automatic parameter adjustments")]
        public bool EnableAutomaticAdjustments = true;
        
        [Tooltip("Whether to enable predictive comfort analysis")]
        public bool EnablePredictiveAnalysis = true;
        
        [Tooltip("Whether to enable automatic rollback on severe comfort issues")]
        public bool EnableAutomaticRollback = true;
        
        [Header("Timing Configuration")]
        [Tooltip("Interval between comfort data processing in seconds")]
        public float ProcessingIntervalSeconds = 5f;
        
        [Tooltip("Time window for trend analysis in minutes")]
        public float TrendAnalysisWindowMinutes = 2f;
        
        [Tooltip("Maximum time to wait before triggering adjustments in seconds")]
        public float MaxAdjustmentDelaySeconds = 30f;
        
        [Header("Adjustment Sensitivity")]
        [Tooltip("Sensitivity multiplier for parameter adjustments (0.1 = conservative, 2.0 = aggressive)")]
        public float AdjustmentSensitivity = 1.0f;
        
        [Tooltip("Minimum comfort score before triggering adjustments (0-100)")]
        public float MinimumComfortThreshold = 60f;
        
        [Tooltip("Critical comfort score that triggers immediate rollback (0-100)")]
        public float CriticalComfortThreshold = 30f;
        
        [Header("Safety Limits")]
        [Tooltip("Maximum number of adjustments per session")]
        public int MaxAdjustmentsPerSession = 10;
        
        [Tooltip("Minimum time between adjustments in seconds")]
        public float MinTimeBetweenAdjustments = 15f;
        
        [Tooltip("Maximum parameter change per adjustment (0-1 scale)")]
        public float MaxParameterChangePerAdjustment = 0.2f;
        
        /// <summary>
        /// Default configuration for Quest 3 comfort validation.
        /// </summary>
        public static ComfortFeedbackConfiguration Quest3Default => new ComfortFeedbackConfiguration
        {
            EnableAutomaticAdjustments = true,
            EnablePredictiveAnalysis = true,
            EnableAutomaticRollback = true,
            ProcessingIntervalSeconds = 5f,
            TrendAnalysisWindowMinutes = 2f,
            MaxAdjustmentDelaySeconds = 30f,
            AdjustmentSensitivity = 1.0f,
            MinimumComfortThreshold = 60f,
            CriticalComfortThreshold = 30f,
            MaxAdjustmentsPerSession = 10,
            MinTimeBetweenAdjustments = 15f,
            MaxParameterChangePerAdjustment = 0.2f
        };
    }
    
    /// <summary>
    /// Comfort validation feedback loop session information.
    /// </summary>
    [Serializable]
    public class ComfortFeedbackSession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// Associated data collection session ID.
        /// </summary>
        public string DataCollectionSessionId { get; set; }
        
        /// <summary>
        /// Session start time.
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Session end time (if completed).
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// Current session status.
        /// </summary>
        public FeedbackSessionStatus Status { get; set; }
        
        /// <summary>
        /// Initial wave matrix settings.
        /// </summary>
        public WaveMatrixSettings InitialWaveSettings { get; set; }
        
        /// <summary>
        /// Current wave matrix settings.
        /// </summary>
        public WaveMatrixSettings CurrentWaveSettings { get; set; }
        
        /// <summary>
        /// History of parameter adjustments made.
        /// </summary>
        public List<WaveParameterAdjustment> AdjustmentHistory { get; set; } = new List<WaveParameterAdjustment>();
        
        /// <summary>
        /// Comfort data points collected during session.
        /// </summary>
        public List<ComfortDataPoint> ComfortDataHistory { get; set; } = new List<ComfortDataPoint>();
        
        /// <summary>
        /// Current comfort validation thresholds.
        /// </summary>
        public ComfortValidationThresholds ValidationThresholds { get; set; }
        
        /// <summary>
        /// Session performance metrics.
        /// </summary>
        public FeedbackLoopMetrics PerformanceMetrics { get; set; }
        
        /// <summary>
        /// Last known good wave settings for rollback.
        /// </summary>
        public WaveMatrixSettings LastKnownGoodSettings { get; set; }
        
        /// <summary>
        /// Session configuration.
        /// </summary>
        public ComfortFeedbackConfiguration Configuration { get; set; }
    }
    
    /// <summary>
    /// Wave parameter adjustment information.
    /// </summary>
    [Serializable]
    public class WaveParameterAdjustment
    {
        /// <summary>
        /// Unique adjustment identifier.
        /// </summary>
        public string AdjustmentId { get; set; }
        
        /// <summary>
        /// Timestamp when adjustment was applied.
        /// </summary>
        public DateTime AppliedAt { get; set; }
        
        /// <summary>
        /// Type of adjustment made.
        /// </summary>
        public AdjustmentType Type { get; set; }
        
        /// <summary>
        /// Reason for the adjustment.
        /// </summary>
        public string Reason { get; set; }
        
        /// <summary>
        /// Wave settings before adjustment.
        /// </summary>
        public WaveMatrixSettings SettingsBefore { get; set; }
        
        /// <summary>
        /// Wave settings after adjustment.
        /// </summary>
        public WaveMatrixSettings SettingsAfter { get; set; }
        
        /// <summary>
        /// Comfort issues that triggered this adjustment.
        /// </summary>
        public ComfortIssue[] TriggeringIssues { get; set; }
        
        /// <summary>
        /// Adjustment strategy used.
        /// </summary>
        public AdjustmentStrategy Strategy { get; set; }
        
        /// <summary>
        /// Effectiveness of the adjustment (measured after application).
        /// </summary>
        public float? EffectivenessScore { get; set; }
        
        /// <summary>
        /// Whether this adjustment was automatically applied.
        /// </summary>
        public bool IsAutomatic { get; set; }
        
        /// <summary>
        /// Additional metadata about the adjustment.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Comfort data processing result.
    /// </summary>
    [Serializable]
    public class ComfortDataProcessingResult
    {
        /// <summary>
        /// Whether processing was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }
        
        /// <summary>
        /// Processing timestamp.
        /// </summary>
        public DateTime ProcessedAt { get; set; }
        
        /// <summary>
        /// Current comfort score (0-100).
        /// </summary>
        public float CurrentComfortScore { get; set; }
        
        /// <summary>
        /// Comfort trend direction.
        /// </summary>
        public TrendDirection ComfortTrend { get; set; }
        
        /// <summary>
        /// Comfort issues identified.
        /// </summary>
        public ComfortIssue[] IdentifiedIssues { get; set; }
        
        /// <summary>
        /// Whether adjustments were triggered.
        /// </summary>
        public bool AdjustmentsTriggered { get; set; }
        
        /// <summary>
        /// Adjustments that were applied.
        /// </summary>
        public WaveParameterAdjustment[] AppliedAdjustments { get; set; }
        
        /// <summary>
        /// Processing messages and warnings.
        /// </summary>
        public string[] ProcessingMessages { get; set; }
        
        /// <summary>
        /// Predicted comfort trajectory.
        /// </summary>
        public ComfortPrediction ComfortPrediction { get; set; }
    }
    
    /// <summary>
    /// Comfort trend analysis result.
    /// </summary>
    [Serializable]
    public class ComfortTrendAnalysis
    {
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp { get; set; }
        
        /// <summary>
        /// Overall comfort trend direction.
        /// </summary>
        public TrendDirection OverallTrend { get; set; }
        
        /// <summary>
        /// Trend strength (0-1 scale).
        /// </summary>
        public float TrendStrength { get; set; }
        
        /// <summary>
        /// Comfort score trajectory over time.
        /// </summary>
        public ComfortTrajectoryPoint[] ComfortTrajectory { get; set; }
        
        /// <summary>
        /// Identified comfort patterns.
        /// </summary>
        public ComfortPattern[] IdentifiedPatterns { get; set; }
        
        /// <summary>
        /// Predicted future comfort levels.
        /// </summary>
        public ComfortPrediction[] FuturePredictions { get; set; }
        
        /// <summary>
        /// Risk factors for comfort degradation.
        /// </summary>
        public ComfortRiskFactor[] RiskFactors { get; set; }
        
        /// <summary>
        /// Recommended preventive actions.
        /// </summary>
        public string[] PreventiveRecommendations { get; set; }
        
        /// <summary>
        /// Analysis confidence level (0-1 scale).
        /// </summary>
        public float AnalysisConfidence { get; set; }
    }
    
    /// <summary>
    /// Parameter adjustment result.
    /// </summary>
    [Serializable]
    public class ParameterAdjustmentResult
    {
        /// <summary>
        /// Whether adjustments were successfully applied.
        /// </summary>
        public bool IsSuccessful { get; set; }
        
        /// <summary>
        /// Adjustment timestamp.
        /// </summary>
        public DateTime AdjustmentTimestamp { get; set; }
        
        /// <summary>
        /// Number of parameters adjusted.
        /// </summary>
        public int ParametersAdjusted { get; set; }
        
        /// <summary>
        /// Adjustments that were applied.
        /// </summary>
        public WaveParameterAdjustment[] AppliedAdjustments { get; set; }
        
        /// <summary>
        /// Adjustments that failed to apply.
        /// </summary>
        public WaveParameterAdjustment[] FailedAdjustments { get; set; }
        
        /// <summary>
        /// Validation result for adjusted parameters.
        /// </summary>
        public WaveParameterValidationResult ValidationResult { get; set; }
        
        /// <summary>
        /// Expected impact on comfort.
        /// </summary>
        public ComfortImpactPrediction ExpectedImpact { get; set; }
        
        /// <summary>
        /// Adjustment messages and warnings.
        /// </summary>
        public string[] AdjustmentMessages { get; set; }
        
        /// <summary>
        /// Time taken to apply adjustments.
        /// </summary>
        public TimeSpan AdjustmentDuration { get; set; }
    }
    
    /// <summary>
    /// Comfort validation result.
    /// </summary>
    [Serializable]
    public class ComfortValidationResult
    {
        /// <summary>
        /// Whether comfort levels meet validation criteria.
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Validation timestamp.
        /// </summary>
        public DateTime ValidationTimestamp { get; set; }
        
        /// <summary>
        /// Current overall comfort score (0-100).
        /// </summary>
        public float OverallComfortScore { get; set; }
        
        /// <summary>
        /// Individual comfort metric scores.
        /// </summary>
        public Dictionary<ComfortMetricType, float> MetricScores { get; set; }
        
        /// <summary>
        /// Comfort validation violations.
        /// </summary>
        public ComfortValidationViolation[] Violations { get; set; }
        
        /// <summary>
        /// Comfort issues requiring attention.
        /// </summary>
        public ComfortIssue[] ComfortIssues { get; set; }
        
        /// <summary>
        /// Validation recommendations.
        /// </summary>
        public string[] ValidationRecommendations { get; set; }
        
        /// <summary>
        /// Time until next validation check.
        /// </summary>
        public TimeSpan NextValidationIn { get; set; }
    }
    
    /// <summary>
    /// Rollback operation result.
    /// </summary>
    [Serializable]
    public class RollbackResult
    {
        /// <summary>
        /// Whether rollback was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }
        
        /// <summary>
        /// Rollback timestamp.
        /// </summary>
        public DateTime RollbackTimestamp { get; set; }
        
        /// <summary>
        /// Reason for rollback.
        /// </summary>
        public string RollbackReason { get; set; }
        
        /// <summary>
        /// Settings before rollback.
        /// </summary>
        public WaveMatrixSettings SettingsBefore { get; set; }
        
        /// <summary>
        /// Settings after rollback.
        /// </summary>
        public WaveMatrixSettings SettingsAfter { get; set; }
        
        /// <summary>
        /// Number of adjustments rolled back.
        /// </summary>
        public int AdjustmentsRolledBack { get; set; }
        
        /// <summary>
        /// Time taken to perform rollback.
        /// </summary>
        public TimeSpan RollbackDuration { get; set; }
        
        /// <summary>
        /// Rollback messages and status updates.
        /// </summary>
        public string[] RollbackMessages { get; set; }
        
        /// <summary>
        /// Expected comfort improvement from rollback.
        /// </summary>
        public ComfortImpactPrediction ExpectedImprovement { get; set; }
    }
    
    /// <summary>
    /// Feedback loop performance metrics.
    /// </summary>
    [Serializable]
    public class FeedbackLoopMetrics
    {
        /// <summary>
        /// Total number of comfort data points processed.
        /// </summary>
        public int TotalDataPointsProcessed { get; set; }
        
        /// <summary>
        /// Total number of adjustments made.
        /// </summary>
        public int TotalAdjustmentsMade { get; set; }
        
        /// <summary>
        /// Number of successful adjustments.
        /// </summary>
        public int SuccessfulAdjustments { get; set; }
        
        /// <summary>
        /// Number of failed adjustments.
        /// </summary>
        public int FailedAdjustments { get; set; }
        
        /// <summary>
        /// Number of rollbacks triggered.
        /// </summary>
        public int RollbacksTriggered { get; set; }
        
        /// <summary>
        /// Average time between comfort data processing.
        /// </summary>
        public TimeSpan AverageProcessingInterval { get; set; }
        
        /// <summary>
        /// Average time to apply adjustments.
        /// </summary>
        public TimeSpan AverageAdjustmentTime { get; set; }
        
        /// <summary>
        /// Overall feedback loop effectiveness score (0-1).
        /// </summary>
        public float OverallEffectiveness { get; set; }
        
        /// <summary>
        /// Comfort improvement rate (comfort score change per minute).
        /// </summary>
        public float ComfortImprovementRate { get; set; }
        
        /// <summary>
        /// System response time to comfort issues.
        /// </summary>
        public TimeSpan AverageResponseTime { get; set; }
    }
    
    /// <summary>
    /// Comfort validation thresholds configuration.
    /// </summary>
    [Serializable]
    public class ComfortValidationThresholds
    {
        [Header("Comfort Score Thresholds")]
        [Tooltip("Minimum acceptable overall comfort score (0-100)")]
        public float MinimumComfortScore = 60f;
        
        [Tooltip("Critical comfort score that triggers immediate action (0-100)")]
        public float CriticalComfortScore = 30f;
        
        [Tooltip("Target comfort score to maintain (0-100)")]
        public float TargetComfortScore = 80f;
        
        [Header("Motion Sickness Thresholds")]
        [Tooltip("Maximum acceptable SIM score")]
        public float MaxSIMScore = 20f;
        
        [Tooltip("Maximum acceptable SSQ score")]
        public float MaxSSQScore = 15f;
        
        [Tooltip("Critical motion sickness score")]
        public float CriticalMotionSicknessScore = 40f;
        
        [Header("Physiological Thresholds")]
        [Tooltip("Maximum acceptable heart rate increase (BPM)")]
        public float MaxHeartRateIncrease = 20f;
        
        [Tooltip("Maximum acceptable skin conductance increase")]
        public float MaxSkinConductanceIncrease = 2f;
        
        [Tooltip("Maximum acceptable stress indicator score")]
        public float MaxStressIndicatorScore = 70f;
        
        [Header("Behavioral Thresholds")]
        [Tooltip("Maximum number of discomfort indicators per minute")]
        public int MaxDiscomfortIndicatorsPerMinute = 3;
        
        [Tooltip("Minimum observer confidence level required")]
        public float MinObserverConfidence = 70f;
        
        [Header("Trend Analysis Thresholds")]
        [Tooltip("Minimum trend strength to trigger adjustments")]
        public float MinTrendStrengthForAction = 0.6f;
        
        [Tooltip("Maximum acceptable comfort decline rate per minute")]
        public float MaxComfortDeclineRate = 5f;
        
        /// <summary>
        /// Default thresholds for Quest 3 comfort validation.
        /// </summary>
        public static ComfortValidationThresholds Quest3Default => new ComfortValidationThresholds
        {
            MinimumComfortScore = 60f,
            CriticalComfortScore = 30f,
            TargetComfortScore = 80f,
            MaxSIMScore = 20f,
            MaxSSQScore = 15f,
            CriticalMotionSicknessScore = 40f,
            MaxHeartRateIncrease = 20f,
            MaxSkinConductanceIncrease = 2f,
            MaxStressIndicatorScore = 70f,
            MaxDiscomfortIndicatorsPerMinute = 3,
            MinObserverConfidence = 70f,
            MinTrendStrengthForAction = 0.6f,
            MaxComfortDeclineRate = 5f
        };
    }
    
    /// <summary>
    /// Adjustment strategy configuration.
    /// </summary>
    [Serializable]
    public class AdjustmentStrategy
    {
        /// <summary>
        /// Strategy identifier.
        /// </summary>
        public string StrategyId { get; set; }
        
        /// <summary>
        /// Strategy name.
        /// </summary>
        public string StrategyName { get; set; }
        
        /// <summary>
        /// Strategy description.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Strategy priority (higher = preferred).
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Comfort issues this strategy addresses.
        /// </summary>
        public ComfortIssueType[] AddressedIssues { get; set; }
        
        /// <summary>
        /// Parameter adjustment rules.
        /// </summary>
        public ParameterAdjustmentRule[] AdjustmentRules { get; set; }
        
        /// <summary>
        /// Strategy effectiveness score (0-1).
        /// </summary>
        public float EffectivenessScore { get; set; }
        
        /// <summary>
        /// Whether this strategy is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Conservative wave parameter reduction strategy.
        /// </summary>
        public static AdjustmentStrategy ConservativeReduction => new AdjustmentStrategy
        {
            StrategyId = "conservative-reduction",
            StrategyName = "Conservative Parameter Reduction",
            Description = "Gradually reduces wave parameters to improve comfort",
            Priority = 100,
            AddressedIssues = new[] { ComfortIssueType.MotionSickness, ComfortIssueType.Disorientation },
            EffectivenessScore = 0.8f,
            IsEnabled = true
        };
        
        /// <summary>
        /// Frequency adjustment strategy for motion sickness.
        /// </summary>
        public static AdjustmentStrategy FrequencyAdjustment => new AdjustmentStrategy
        {
            StrategyId = "frequency-adjustment",
            StrategyName = "Wave Frequency Adjustment",
            Description = "Adjusts wave frequency to reduce motion sickness",
            Priority = 90,
            AddressedIssues = new[] { ComfortIssueType.MotionSickness },
            EffectivenessScore = 0.75f,
            IsEnabled = true
        };
    }
    
    /// <summary>
    /// Feedback loop termination result.
    /// </summary>
    [Serializable]
    public class FeedbackLoopTerminationResult
    {
        /// <summary>
        /// Whether termination was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }
        
        /// <summary>
        /// Termination timestamp.
        /// </summary>
        public DateTime TerminationTimestamp { get; set; }
        
        /// <summary>
        /// Reason for termination.
        /// </summary>
        public string TerminationReason { get; set; }
        
        /// <summary>
        /// Final session metrics.
        /// </summary>
        public FeedbackLoopMetrics FinalMetrics { get; set; }
        
        /// <summary>
        /// Final comfort validation result.
        /// </summary>
        public ComfortValidationResult FinalComfortValidation { get; set; }
        
        /// <summary>
        /// Session summary report.
        /// </summary>
        public string SessionSummary { get; set; }
    }
    
    /// <summary>
    /// Comprehensive comfort feedback report.
    /// </summary>
    [Serializable]
    public class ComfortFeedbackReport
    {
        /// <summary>
        /// Report identifier.
        /// </summary>
        public string ReportId { get; set; }
        
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// Report generation timestamp.
        /// </summary>
        public DateTime GeneratedAt { get; set; }
        
        /// <summary>
        /// Session summary information.
        /// </summary>
        public ComfortFeedbackSession SessionSummary { get; set; }
        
        /// <summary>
        /// Comfort analysis results.
        /// </summary>
        public ComfortTrendAnalysis ComfortAnalysis { get; set; }
        
        /// <summary>
        /// Adjustment effectiveness analysis.
        /// </summary>
        public AdjustmentEffectivenessAnalysis AdjustmentAnalysis { get; set; }
        
        /// <summary>
        /// Performance metrics summary.
        /// </summary>
        public FeedbackLoopMetrics PerformanceMetrics { get; set; }
        
        /// <summary>
        /// Key insights and findings.
        /// </summary>
        public string[] KeyInsights { get; set; }
        
        /// <summary>
        /// Recommendations for future sessions.
        /// </summary>
        public string[] Recommendations { get; set; }
        
        /// <summary>
        /// Report confidence level (0-1).
        /// </summary>
        public float ReportConfidence { get; set; }
    }
    
    // Supporting enums and structures
    
    /// <summary>
    /// Feedback session status.
    /// </summary>
    public enum FeedbackSessionStatus
    {
        NotStarted,
        Active,
        Paused,
        Completed,
        Terminated,
        Error
    }
    
    /// <summary>
    /// Types of parameter adjustments.
    /// </summary>
    public enum AdjustmentType
    {
        Automatic,
        Manual,
        Rollback,
        Emergency
    }
    
    /// <summary>
    /// Comfort metric types for validation.
    /// </summary>
    public enum ComfortMetricType
    {
        OverallComfort,
        MotionSickness,
        PhysiologicalStress,
        BehavioralIndicators,
        SubjectiveRating
    }
    
    /// <summary>
    /// Types of comfort issues.
    /// </summary>
    public enum ComfortIssueType
    {
        MotionSickness,
        Disorientation,
        EyeStrain,
        PhysiologicalStress,
        BehavioralDiscomfort,
        CognitiveOverload
    }
    
    /// <summary>
    /// Comfort issue information.
    /// </summary>
    [Serializable]
    public class ComfortIssue
    {
        /// <summary>
        /// Issue identifier.
        /// </summary>
        public string IssueId { get; set; }
        
        /// <summary>
        /// Type of comfort issue.
        /// </summary>
        public ComfortIssueType Type { get; set; }
        
        /// <summary>
        /// Issue severity level.
        /// </summary>
        public ComfortIssueSeverity Severity { get; set; }
        
        /// <summary>
        /// Issue description.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// When the issue was first detected.
        /// </summary>
        public DateTime DetectedAt { get; set; }
        
        /// <summary>
        /// Confidence level in issue detection (0-1).
        /// </summary>
        public float DetectionConfidence { get; set; }
        
        /// <summary>
        /// Recommended actions to address the issue.
        /// </summary>
        public string[] RecommendedActions { get; set; }
        
        /// <summary>
        /// Associated comfort data that triggered this issue.
        /// </summary>
        public ComfortDataPoint[] AssociatedData { get; set; }
    }
    
    /// <summary>
    /// Comfort issue severity levels.
    /// </summary>
    public enum ComfortIssueSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Comfort validation violation.
    /// </summary>
    [Serializable]
    public class ComfortValidationViolation
    {
        /// <summary>
        /// Violation identifier.
        /// </summary>
        public string ViolationId { get; set; }
        
        /// <summary>
        /// Metric that was violated.
        /// </summary>
        public ComfortMetricType MetricType { get; set; }
        
        /// <summary>
        /// Current value that violated the threshold.
        /// </summary>
        public float CurrentValue { get; set; }
        
        /// <summary>
        /// Threshold value that was violated.
        /// </summary>
        public float ThresholdValue { get; set; }
        
        /// <summary>
        /// Violation severity.
        /// </summary>
        public ComfortIssueSeverity Severity { get; set; }
        
        /// <summary>
        /// When the violation occurred.
        /// </summary>
        public DateTime ViolationTimestamp { get; set; }
        
        /// <summary>
        /// Duration of the violation.
        /// </summary>
        public TimeSpan ViolationDuration { get; set; }
    }
    
    // Additional supporting structures
    
    /// <summary>
    /// Comfort trajectory point for trend analysis.
    /// </summary>
    [Serializable]
    public class ComfortTrajectoryPoint
    {
        public DateTime Timestamp { get; set; }
        public float ComfortScore { get; set; }
        public ComfortLevel ComfortLevel { get; set; }
        public float MotionSicknessScore { get; set; }
    }
    
    /// <summary>
    /// Comfort pattern identification.
    /// </summary>
    [Serializable]
    public class ComfortPattern
    {
        public string PatternId { get; set; }
        public string PatternName { get; set; }
        public string Description { get; set; }
        public float PatternStrength { get; set; }
        public TimeSpan PatternDuration { get; set; }
    }
    
    /// <summary>
    /// Comfort prediction information.
    /// </summary>
    [Serializable]
    public class ComfortPrediction
    {
        public DateTime PredictionTimestamp { get; set; }
        public float PredictedComfortScore { get; set; }
        public ComfortLevel PredictedComfortLevel { get; set; }
        public float PredictionConfidence { get; set; }
        public TimeSpan PredictionHorizon { get; set; }
    }
    
    /// <summary>
    /// Comfort risk factor.
    /// </summary>
    [Serializable]
    public class ComfortRiskFactor
    {
        public string RiskFactorId { get; set; }
        public string Description { get; set; }
        public float RiskLevel { get; set; }
        public ComfortIssueType[] PotentialIssues { get; set; }
        public string[] MitigationStrategies { get; set; }
    }
    
    /// <summary>
    /// Comfort impact prediction.
    /// </summary>
    [Serializable]
    public class ComfortImpactPrediction
    {
        public float ExpectedComfortChange { get; set; }
        public ComfortLevel ExpectedComfortLevel { get; set; }
        public float PredictionConfidence { get; set; }
        public TimeSpan ExpectedTimeToEffect { get; set; }
        public string[] PotentialSideEffects { get; set; }
    }
    
    /// <summary>
    /// Parameter adjustment rule.
    /// </summary>
    [Serializable]
    public class ParameterAdjustmentRule
    {
        public string RuleId { get; set; }
        public string ParameterName { get; set; }
        public AdjustmentDirection Direction { get; set; }
        public float AdjustmentMagnitude { get; set; }
        public ComfortIssueType[] TriggerConditions { get; set; }
        public float ConfidenceThreshold { get; set; }
    }
    
    /// <summary>
    /// Adjustment direction.
    /// </summary>
    public enum AdjustmentDirection
    {
        Increase,
        Decrease,
        Optimize
    }
    
    /// <summary>
    /// Adjustment effectiveness analysis.
    /// </summary>
    [Serializable]
    public class AdjustmentEffectivenessAnalysis
    {
        public float OverallEffectiveness { get; set; }
        public Dictionary<string, float> StrategyEffectiveness { get; set; }
        public TimeSpan AverageTimeToEffect { get; set; }
        public float SuccessRate { get; set; }
        public string[] MostEffectiveStrategies { get; set; }
        public string[] LeastEffectiveStrategies { get; set; }
    }
    
    // Event argument classes
    
    /// <summary>
    /// Event arguments for comfort validation failed.
    /// </summary>
    public class ComfortValidationFailedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public ComfortValidationResult ValidationResult { get; set; }
        public ComfortIssue[] ComfortIssues { get; set; }
        public DateTime FailureTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for parameter adjustment.
    /// </summary>
    public class ParameterAdjustmentEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public WaveParameterAdjustment[] Adjustments { get; set; }
        public AdjustmentStrategy Strategy { get; set; }
        public DateTime AdjustmentTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for comfort validation passed.
    /// </summary>
    public class ComfortValidationPassedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public ComfortValidationResult ValidationResult { get; set; }
        public DateTime PassedTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for automatic rollback.
    /// </summary>
    public class AutomaticRollbackEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public RollbackResult RollbackResult { get; set; }
        public string RollbackReason { get; set; }
        public DateTime RollbackTimestamp { get; set; }
    }
}