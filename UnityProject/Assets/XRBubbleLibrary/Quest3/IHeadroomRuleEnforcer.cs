using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Interface for headroom rule enforcement system.
    /// Implements Requirement 6.3: 30% headroom rule enforcement.
    /// Implements Requirement 6.4: Feature addition evaluation against budget.
    /// Implements Requirement 6.5: Automatic warnings when headroom is insufficient.
    /// </summary>
    public interface IHeadroomRuleEnforcer
    {
        /// <summary>
        /// Whether the headroom rule enforcer is initialized.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Current headroom enforcement configuration.
        /// </summary>
        HeadroomEnforcementConfiguration Configuration { get; }
        
        /// <summary>
        /// Current performance headroom status.
        /// </summary>
        HeadroomStatus CurrentHeadroomStatus { get; }
        
        /// <summary>
        /// Event fired when headroom violations are detected.
        /// </summary>
        event Action<HeadroomViolationEventArgs> HeadroomViolationDetected;
        
        /// <summary>
        /// Event fired when headroom warnings are triggered.
        /// </summary>
        event Action<HeadroomWarningEventArgs> HeadroomWarningTriggered;
        
        /// <summary>
        /// Initialize the headroom rule enforcement system.
        /// </summary>
        /// <param name="config">Enforcement configuration</param>
        /// <returns>True if initialization successful</returns>
        bool Initialize(HeadroomEnforcementConfiguration config);
        
        /// <summary>
        /// Validate current performance against headroom rules.
        /// </summary>
        /// <param name="performanceData">Current performance data</param>
        /// <returns>Headroom validation result</returns>
        HeadroomValidationResult ValidateHeadroom(PerformanceData performanceData);
        
        /// <summary>
        /// Evaluate whether a new feature can be added within headroom constraints.
        /// </summary>
        /// <param name="featureImpact">Estimated performance impact of new feature</param>
        /// <param name="currentPerformance">Current system performance</param>
        /// <returns>Feature addition evaluation result</returns>
        FeatureAdditionEvaluation EvaluateFeatureAddition(FeaturePerformanceImpact featureImpact, PerformanceData currentPerformance);
        
        /// <summary>
        /// Calculate available headroom for new features.
        /// </summary>
        /// <param name="performanceData">Current performance data</param>
        /// <returns>Available headroom analysis</returns>
        AvailableHeadroomAnalysis CalculateAvailableHeadroom(PerformanceData performanceData);
        
        /// <summary>
        /// Generate headroom compliance report.
        /// </summary>
        /// <param name="performanceHistory">Historical performance data</param>
        /// <returns>Comprehensive headroom compliance report</returns>
        HeadroomComplianceReport GenerateComplianceReport(PerformanceData[] performanceHistory);
        
        /// <summary>
        /// Set headroom warning thresholds.
        /// </summary>
        /// <param name="thresholds">Warning threshold configuration</param>
        void SetWarningThresholds(HeadroomWarningThresholds thresholds);
        
        /// <summary>
        /// Enable or disable automatic headroom monitoring.
        /// </summary>
        /// <param name="enabled">Whether to enable automatic monitoring</param>
        void SetAutomaticMonitoring(bool enabled);
        
        /// <summary>
        /// Get headroom rule enforcement history.
        /// </summary>
        /// <returns>Historical enforcement data</returns>
        HeadroomEnforcementHistory GetEnforcementHistory();
        
        /// <summary>
        /// Reset headroom enforcement state and history.
        /// </summary>
        void ResetEnforcementState();
    }
    
    /// <summary>
    /// Configuration for headroom rule enforcement.
    /// </summary>
    [Serializable]
    public struct HeadroomEnforcementConfiguration
    {
        /// <summary>
        /// Target frame rate for headroom calculations (default: 72 Hz for Quest 3).
        /// </summary>
        public float TargetFrameRate;
        
        /// <summary>
        /// Required headroom percentage (default: 30%).
        /// </summary>
        public float RequiredHeadroomPercent;
        
        /// <summary>
        /// Warning threshold percentage (default: 20% headroom remaining).
        /// </summary>
        public float WarningThresholdPercent;
        
        /// <summary>
        /// Critical threshold percentage (default: 10% headroom remaining).
        /// </summary>
        public float CriticalThresholdPercent;
        
        /// <summary>
        /// Whether to enable automatic monitoring.
        /// </summary>
        public bool EnableAutomaticMonitoring;
        
        /// <summary>
        /// Monitoring interval in seconds.
        /// </summary>
        public float MonitoringIntervalSeconds;
        
        /// <summary>
        /// Whether to enable debug logging.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Default Quest 3 headroom enforcement configuration.
        /// </summary>
        public static HeadroomEnforcementConfiguration Quest3Default => new HeadroomEnforcementConfiguration
        {
            TargetFrameRate = 72f,
            RequiredHeadroomPercent = 30f,
            WarningThresholdPercent = 20f,
            CriticalThresholdPercent = 10f,
            EnableAutomaticMonitoring = true,
            MonitoringIntervalSeconds = 1f,
            EnableDebugLogging = false
        };
    }
    
    /// <summary>
    /// Current headroom status information.
    /// </summary>
    [Serializable]
    public struct HeadroomStatus
    {
        /// <summary>
        /// Current available headroom percentage.
        /// </summary>
        public float AvailableHeadroomPercent;
        
        /// <summary>
        /// Current headroom compliance level.
        /// </summary>
        public HeadroomComplianceLevel ComplianceLevel;
        
        /// <summary>
        /// Time remaining until headroom violation (if trending toward violation).
        /// </summary>
        public TimeSpan TimeToViolation;
        
        /// <summary>
        /// Whether headroom is currently compliant.
        /// </summary>
        public bool IsCompliant;
        
        /// <summary>
        /// Last update timestamp.
        /// </summary>
        public DateTime LastUpdateTime;
    }
    
    /// <summary>
    /// Headroom compliance levels.
    /// </summary>
    public enum HeadroomComplianceLevel
    {
        /// <summary>
        /// Headroom is above required threshold (>30%).
        /// </summary>
        Compliant,
        
        /// <summary>
        /// Headroom is in warning range (20-30%).
        /// </summary>
        Warning,
        
        /// <summary>
        /// Headroom is in critical range (10-20%).
        /// </summary>
        Critical,
        
        /// <summary>
        /// Headroom is below minimum threshold (<10%).
        /// </summary>
        Violation
    }
    
    /// <summary>
    /// Performance data for headroom calculations.
    /// </summary>
    [Serializable]
    public struct PerformanceData
    {
        /// <summary>
        /// Current frame rate.
        /// </summary>
        public float CurrentFrameRate;
        
        /// <summary>
        /// Current frame time in milliseconds.
        /// </summary>
        public float CurrentFrameTimeMs;
        
        /// <summary>
        /// CPU usage percentage.
        /// </summary>
        public float CPUUsagePercent;
        
        /// <summary>
        /// GPU usage percentage.
        /// </summary>
        public float GPUUsagePercent;
        
        /// <summary>
        /// Memory usage in MB.
        /// </summary>
        public float MemoryUsageMB;
        
        /// <summary>
        /// Thermal state (0-1, where 1 is thermal throttling).
        /// </summary>
        public float ThermalState;
        
        /// <summary>
        /// Timestamp of performance measurement.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Additional performance metrics.
        /// </summary>
        public Dictionary<string, float> AdditionalMetrics;
    }
    
    /// <summary>
    /// Result of headroom validation.
    /// </summary>
    [Serializable]
    public struct HeadroomValidationResult
    {
        /// <summary>
        /// Whether headroom validation passed.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// Current headroom percentage.
        /// </summary>
        public float HeadroomPercent;
        
        /// <summary>
        /// Compliance level.
        /// </summary>
        public HeadroomComplianceLevel ComplianceLevel;
        
        /// <summary>
        /// Validation messages and warnings.
        /// </summary>
        public string[] ValidationMessages;
        
        /// <summary>
        /// Recommended actions to improve headroom.
        /// </summary>
        public string[] RecommendedActions;
        
        /// <summary>
        /// Validation timestamp.
        /// </summary>
        public DateTime ValidationTimestamp;
    }
    
    /// <summary>
    /// Estimated performance impact of a feature.
    /// </summary>
    [Serializable]
    public struct FeaturePerformanceImpact
    {
        /// <summary>
        /// Feature name or identifier.
        /// </summary>
        public string FeatureName;
        
        /// <summary>
        /// Estimated CPU impact percentage.
        /// </summary>
        public float EstimatedCPUImpact;
        
        /// <summary>
        /// Estimated GPU impact percentage.
        /// </summary>
        public float EstimatedGPUImpact;
        
        /// <summary>
        /// Estimated memory impact in MB.
        /// </summary>
        public float EstimatedMemoryImpact;
        
        /// <summary>
        /// Estimated frame time impact in milliseconds.
        /// </summary>
        public float EstimatedFrameTimeImpact;
        
        /// <summary>
        /// Confidence level of impact estimates (0-1).
        /// </summary>
        public float ConfidenceLevel;
        
        /// <summary>
        /// Impact estimation methodology.
        /// </summary>
        public string EstimationMethod;
    }
    
    /// <summary>
    /// Result of feature addition evaluation.
    /// </summary>
    [Serializable]
    public struct FeatureAdditionEvaluation
    {
        /// <summary>
        /// Whether the feature can be added within headroom constraints.
        /// </summary>
        public bool CanAddFeature;
        
        /// <summary>
        /// Projected headroom after feature addition.
        /// </summary>
        public float ProjectedHeadroomPercent;
        
        /// <summary>
        /// Risk level of adding the feature.
        /// </summary>
        public FeatureAdditionRisk RiskLevel;
        
        /// <summary>
        /// Evaluation reasoning and details.
        /// </summary>
        public string[] EvaluationDetails;
        
        /// <summary>
        /// Recommended mitigations if feature is risky.
        /// </summary>
        public string[] RecommendedMitigations;
        
        /// <summary>
        /// Evaluation timestamp.
        /// </summary>
        public DateTime EvaluationTimestamp;
    }
    
    /// <summary>
    /// Risk levels for feature addition.
    /// </summary>
    public enum FeatureAdditionRisk
    {
        /// <summary>
        /// Low risk - plenty of headroom available.
        /// </summary>
        Low,
        
        /// <summary>
        /// Medium risk - some headroom concerns.
        /// </summary>
        Medium,
        
        /// <summary>
        /// High risk - minimal headroom remaining.
        /// </summary>
        High,
        
        /// <summary>
        /// Critical risk - would violate headroom rules.
        /// </summary>
        Critical
    }
    
    /// <summary>
    /// Available headroom analysis.
    /// </summary>
    [Serializable]
    public struct AvailableHeadroomAnalysis
    {
        /// <summary>
        /// Total available headroom percentage.
        /// </summary>
        public float TotalAvailablePercent;
        
        /// <summary>
        /// Available CPU headroom percentage.
        /// </summary>
        public float AvailableCPUPercent;
        
        /// <summary>
        /// Available GPU headroom percentage.
        /// </summary>
        public float AvailableGPUPercent;
        
        /// <summary>
        /// Available memory headroom in MB.
        /// </summary>
        public float AvailableMemoryMB;
        
        /// <summary>
        /// Available frame time headroom in milliseconds.
        /// </summary>
        public float AvailableFrameTimeMs;
        
        /// <summary>
        /// Headroom breakdown by component.
        /// </summary>
        public Dictionary<string, float> ComponentHeadroom;
        
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp;
    }
    
    /// <summary>
    /// Headroom compliance report.
    /// </summary>
    [Serializable]
    public struct HeadroomComplianceReport
    {
        /// <summary>
        /// Overall compliance status.
        /// </summary>
        public bool IsCompliant;
        
        /// <summary>
        /// Compliance percentage over reporting period.
        /// </summary>
        public float CompliancePercentage;
        
        /// <summary>
        /// Number of violations detected.
        /// </summary>
        public int ViolationCount;
        
        /// <summary>
        /// Number of warnings triggered.
        /// </summary>
        public int WarningCount;
        
        /// <summary>
        /// Average headroom over reporting period.
        /// </summary>
        public float AverageHeadroom;
        
        /// <summary>
        /// Minimum headroom recorded.
        /// </summary>
        public float MinimumHeadroom;
        
        /// <summary>
        /// Compliance trend analysis.
        /// </summary>
        public ComplianceTrend Trend;
        
        /// <summary>
        /// Detailed compliance history.
        /// </summary>
        public HeadroomComplianceEntry[] ComplianceHistory;
        
        /// <summary>
        /// Report generation timestamp.
        /// </summary>
        public DateTime ReportTimestamp;
        
        /// <summary>
        /// Reporting period.
        /// </summary>
        public TimeSpan ReportingPeriod;
    }
    
    /// <summary>
    /// Compliance trend directions.
    /// </summary>
    public enum ComplianceTrend
    {
        /// <summary>
        /// Compliance is improving over time.
        /// </summary>
        Improving,
        
        /// <summary>
        /// Compliance is stable.
        /// </summary>
        Stable,
        
        /// <summary>
        /// Compliance is degrading over time.
        /// </summary>
        Degrading
    }
    
    /// <summary>
    /// Individual compliance history entry.
    /// </summary>
    [Serializable]
    public struct HeadroomComplianceEntry
    {
        /// <summary>
        /// Timestamp of compliance check.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Headroom percentage at time of check.
        /// </summary>
        public float HeadroomPercent;
        
        /// <summary>
        /// Compliance level at time of check.
        /// </summary>
        public HeadroomComplianceLevel ComplianceLevel;
        
        /// <summary>
        /// Performance data at time of check.
        /// </summary>
        public PerformanceData PerformanceData;
    }
    
    /// <summary>
    /// Warning threshold configuration.
    /// </summary>
    [Serializable]
    public struct HeadroomWarningThresholds
    {
        /// <summary>
        /// Warning threshold percentage.
        /// </summary>
        public float WarningThreshold;
        
        /// <summary>
        /// Critical threshold percentage.
        /// </summary>
        public float CriticalThreshold;
        
        /// <summary>
        /// Violation threshold percentage.
        /// </summary>
        public float ViolationThreshold;
        
        /// <summary>
        /// Time window for trend analysis in seconds.
        /// </summary>
        public float TrendAnalysisWindow;
    }
    
    /// <summary>
    /// Headroom enforcement history.
    /// </summary>
    [Serializable]
    public struct HeadroomEnforcementHistory
    {
        /// <summary>
        /// Total number of enforcement checks performed.
        /// </summary>
        public int TotalChecks;
        
        /// <summary>
        /// Number of violations detected.
        /// </summary>
        public int ViolationsDetected;
        
        /// <summary>
        /// Number of warnings issued.
        /// </summary>
        public int WarningsIssued;
        
        /// <summary>
        /// Number of features blocked due to headroom constraints.
        /// </summary>
        public int FeaturesBlocked;
        
        /// <summary>
        /// Historical enforcement entries.
        /// </summary>
        public HeadroomEnforcementEntry[] EnforcementHistory;
        
        /// <summary>
        /// History start timestamp.
        /// </summary>
        public DateTime HistoryStartTime;
        
        /// <summary>
        /// History end timestamp.
        /// </summary>
        public DateTime HistoryEndTime;
    }
    
    /// <summary>
    /// Individual enforcement history entry.
    /// </summary>
    [Serializable]
    public struct HeadroomEnforcementEntry
    {
        /// <summary>
        /// Timestamp of enforcement action.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Type of enforcement action.
        /// </summary>
        public HeadroomEnforcementAction Action;
        
        /// <summary>
        /// Headroom percentage at time of action.
        /// </summary>
        public float HeadroomPercent;
        
        /// <summary>
        /// Details of enforcement action.
        /// </summary>
        public string ActionDetails;
        
        /// <summary>
        /// Performance data at time of action.
        /// </summary>
        public PerformanceData PerformanceData;
    }
    
    /// <summary>
    /// Types of headroom enforcement actions.
    /// </summary>
    public enum HeadroomEnforcementAction
    {
        /// <summary>
        /// Warning issued for low headroom.
        /// </summary>
        WarningIssued,
        
        /// <summary>
        /// Critical alert issued.
        /// </summary>
        CriticalAlert,
        
        /// <summary>
        /// Violation detected.
        /// </summary>
        ViolationDetected,
        
        /// <summary>
        /// Feature addition blocked.
        /// </summary>
        FeatureBlocked,
        
        /// <summary>
        /// Feature addition approved.
        /// </summary>
        FeatureApproved
    }
    
    /// <summary>
    /// Event arguments for headroom violations.
    /// </summary>
    public class HeadroomViolationEventArgs : EventArgs
    {
        /// <summary>
        /// Current headroom percentage.
        /// </summary>
        public float HeadroomPercent { get; set; }
        
        /// <summary>
        /// Violation severity.
        /// </summary>
        public HeadroomComplianceLevel Severity { get; set; }
        
        /// <summary>
        /// Performance data at time of violation.
        /// </summary>
        public PerformanceData PerformanceData { get; set; }
        
        /// <summary>
        /// Violation timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Recommended actions to resolve violation.
        /// </summary>
        public string[] RecommendedActions { get; set; }
    }
    
    /// <summary>
    /// Event arguments for headroom warnings.
    /// </summary>
    public class HeadroomWarningEventArgs : EventArgs
    {
        /// <summary>
        /// Warning message.
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Current headroom percentage.
        /// </summary>
        public float HeadroomPercent { get; set; }
        
        /// <summary>
        /// Warning level.
        /// </summary>
        public HeadroomComplianceLevel WarningLevel { get; set; }
        
        /// <summary>
        /// Performance data at time of warning.
        /// </summary>
        public PerformanceData PerformanceData { get; set; }
        
        /// <summary>
        /// Warning timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}