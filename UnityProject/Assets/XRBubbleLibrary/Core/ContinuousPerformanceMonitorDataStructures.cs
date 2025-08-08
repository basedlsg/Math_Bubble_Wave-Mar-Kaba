using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Data structures for continuous performance monitoring system.
    /// </summary>
    
    /// <summary>
    /// Configuration for continuous performance monitoring.
    /// </summary>
    [Serializable]
    public class PerformanceMonitorConfiguration
    {
        [Header("Monitoring Settings")]
        [Tooltip("Whether to enable continuous monitoring")]
        public bool EnableContinuousMonitoring = true;
        
        [Tooltip("Whether to enable automatic threshold violation detection")]
        public bool EnableThresholdViolationDetection = true;
        
        [Tooltip("Whether to enable performance trend analysis")]
        public bool EnableTrendAnalysis = true;
        
        [Header("Collection Intervals")]
        [Tooltip("Primary data collection interval in seconds")]
        public float DataCollectionIntervalSeconds = 1f;
        
        [Tooltip("High-frequency monitoring interval in seconds (for critical metrics)")]
        public float HighFrequencyIntervalSeconds = 0.1f;
        
        [Tooltip("Statistics calculation interval in seconds")]
        public float StatisticsIntervalSeconds = 10f;
        
        [Header("Data Retention")]
        [Tooltip("Maximum number of performance data points to retain per session")]
        public int MaxDataPointsPerSession = 10000;
        
        [Tooltip("Data retention period in hours")]
        public float DataRetentionHours = 24f;
        
        [Tooltip("Whether to enable automatic data cleanup")]
        public bool EnableAutomaticDataCleanup = true;
        
        [Header("Performance Metrics")]
        [Tooltip("Metrics to monitor continuously")]
        public PerformanceMetricType[] MonitoredMetrics = new[]
        {
            PerformanceMetricType.FPS,
            PerformanceMetricType.FrameTime,
            PerformanceMetricType.MemoryUsage,
            PerformanceMetricType.CPUUsage,
            PerformanceMetricType.GPUUsage,
            PerformanceMetricType.ThermalState
        };
        
        [Header("Alert Configuration")]
        [Tooltip("Whether to enable real-time alerts")]
        public bool EnableRealTimeAlerts = true;
        
        [Tooltip("Alert escalation delay in seconds")]
        public float AlertEscalationDelaySeconds = 30f;
        
        [Tooltip("Maximum alerts per minute to prevent spam")]
        public int MaxAlertsPerMinute = 10;
        
        [Header("Advanced Settings")]
        [Tooltip("Whether to enable high-precision timing")]
        public bool EnableHighPrecisionTiming = true;
        
        [Tooltip("Whether to enable background monitoring")]
        public bool EnableBackgroundMonitoring = false;
        
        [Tooltip("Performance monitoring priority")]
        public MonitoringPriority Priority = MonitoringPriority.High;
        
        /// <summary>
        /// Default configuration for Quest 3 performance monitoring.
        /// </summary>
        public static PerformanceMonitorConfiguration Quest3Default => new PerformanceMonitorConfiguration
        {
            EnableContinuousMonitoring = true,
            EnableThresholdViolationDetection = true,
            EnableTrendAnalysis = true,
            DataCollectionIntervalSeconds = 1f,
            HighFrequencyIntervalSeconds = 0.1f,
            StatisticsIntervalSeconds = 10f,
            MaxDataPointsPerSession = 10000,
            DataRetentionHours = 24f,
            EnableAutomaticDataCleanup = true,
            MonitoredMetrics = new[]
            {
                PerformanceMetricType.FPS,
                PerformanceMetricType.FrameTime,
                PerformanceMetricType.MemoryUsage,
                PerformanceMetricType.CPUUsage,
                PerformanceMetricType.GPUUsage,
                PerformanceMetricType.ThermalState
            },
            EnableRealTimeAlerts = true,
            AlertEscalationDelaySeconds = 30f,
            MaxAlertsPerMinute = 10,
            EnableHighPrecisionTiming = true,
            EnableBackgroundMonitoring = false,
            Priority = MonitoringPriority.High
        };
    }
    
    /// <summary>
    /// Performance monitoring session information.
    /// </summary>
    [Serializable]
    public class PerformanceMonitoringSession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId { get; set; }
        
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
        public MonitoringSessionStatus Status { get; set; }
        
        /// <summary>
        /// Session configuration.
        /// </summary>
        public PerformanceMonitorConfiguration Configuration { get; set; }
        
        /// <summary>
        /// Performance data collected during session.
        /// </summary>
        public List<PerformanceMetrics> PerformanceHistory { get; set; } = new List<PerformanceMetrics>();
        
        /// <summary>
        /// Performance violations detected during session.
        /// </summary>
        public List<PerformanceViolation> ViolationHistory { get; set; } = new List<PerformanceViolation>();
        
        /// <summary>
        /// Session statistics.
        /// </summary>
        public PerformanceStatistics Statistics { get; set; }
        
        /// <summary>
        /// Custom metric collectors for this session.
        /// </summary>
        public List<IPerformanceMetricCollector> CustomCollectors { get; set; } = new List<IPerformanceMetricCollector>();
        
        /// <summary>
        /// Session metadata and notes.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Last data collection timestamp.
        /// </summary>
        public DateTime LastDataCollection { get; set; }
        
        /// <summary>
        /// Number of data points collected.
        /// </summary>
        public int DataPointsCollected { get; set; }
        
        /// <summary>
        /// Session quality score (0-1).
        /// </summary>
        public float SessionQualityScore { get; set; }
    }
    
    /// <summary>
    /// Performance violation information.
    /// </summary>
    [Serializable]
    public class PerformanceViolation
    {
        /// <summary>
        /// Unique violation identifier.
        /// </summary>
        public string ViolationId { get; set; }
        
        /// <summary>
        /// When the violation occurred.
        /// </summary>
        public DateTime OccurredAt { get; set; }
        
        /// <summary>
        /// Metric that was violated.
        /// </summary>
        public PerformanceMetricType MetricType { get; set; }
        
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
        public ViolationSeverity Severity { get; set; }
        
        /// <summary>
        /// Duration of the violation.
        /// </summary>
        public TimeSpan Duration { get; set; }
        
        /// <summary>
        /// Whether the violation has been resolved.
        /// </summary>
        public bool IsResolved { get; set; }
        
        /// <summary>
        /// When the violation was resolved (if applicable).
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
        
        /// <summary>
        /// Actions taken to resolve the violation.
        /// </summary>
        public string[] ResolutionActions { get; set; }
        
        /// <summary>
        /// Additional violation context.
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Performance statistics for a monitoring session.
    /// </summary>
    [Serializable]
    public class PerformanceStatistics
    {
        /// <summary>
        /// Statistics calculation timestamp.
        /// </summary>
        public DateTime CalculatedAt { get; set; }
        
        /// <summary>
        /// Time period covered by statistics.
        /// </summary>
        public TimeSpan TimePeriod { get; set; }
        
        /// <summary>
        /// Number of data points analyzed.
        /// </summary>
        public int DataPointsAnalyzed { get; set; }
        
        /// <summary>
        /// Average performance metrics.
        /// </summary>
        public PerformanceMetrics AverageMetrics { get; set; }
        
        /// <summary>
        /// Minimum performance metrics.
        /// </summary>
        public PerformanceMetrics MinimumMetrics { get; set; }
        
        /// <summary>
        /// Maximum performance metrics.
        /// </summary>
        public PerformanceMetrics MaximumMetrics { get; set; }
        
        /// <summary>
        /// Performance metric percentiles.
        /// </summary>
        public Dictionary<PerformanceMetricType, PerformancePercentiles> Percentiles { get; set; } = new Dictionary<PerformanceMetricType, PerformancePercentiles>();
        
        /// <summary>
        /// Total number of threshold violations.
        /// </summary>
        public int TotalViolations { get; set; }
        
        /// <summary>
        /// Violations by severity level.
        /// </summary>
        public Dictionary<ViolationSeverity, int> ViolationsBySeverity { get; set; } = new Dictionary<ViolationSeverity, int>();
        
        /// <summary>
        /// Performance stability score (0-1).
        /// </summary>
        public float StabilityScore { get; set; }
        
        /// <summary>
        /// Overall performance health score (0-1).
        /// </summary>
        public float OverallHealthScore { get; set; }
        
        /// <summary>
        /// Performance trend direction.
        /// </summary>
        public TrendDirection PerformanceTrend { get; set; }
        
        /// <summary>
        /// Data collection efficiency (successful collections / total attempts).
        /// </summary>
        public float DataCollectionEfficiency { get; set; }
    }
    
    /// <summary>
    /// Performance percentile information.
    /// </summary>
    [Serializable]
    public class PerformancePercentiles
    {
        public float P50 { get; set; } // Median
        public float P90 { get; set; }
        public float P95 { get; set; }
        public float P99 { get; set; }
        public float P999 { get; set; }
    }
    
    /// <summary>
    /// Performance trend analysis result.
    /// </summary>
    [Serializable]
    public class PerformanceTrendAnalysis
    {
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp { get; set; }
        
        /// <summary>
        /// Time window analyzed.
        /// </summary>
        public TimeSpan AnalysisWindow { get; set; }
        
        /// <summary>
        /// Overall performance trend.
        /// </summary>
        public TrendDirection OverallTrend { get; set; }
        
        /// <summary>
        /// Trend strength (0-1).
        /// </summary>
        public float TrendStrength { get; set; }
        
        /// <summary>
        /// Trends by individual metrics.
        /// </summary>
        public Dictionary<PerformanceMetricType, MetricTrend> MetricTrends { get; set; } = new Dictionary<PerformanceMetricType, MetricTrend>();
        
        /// <summary>
        /// Identified performance patterns.
        /// </summary>
        public PerformancePattern[] IdentifiedPatterns { get; set; }
        
        /// <summary>
        /// Performance anomalies detected.
        /// </summary>
        public PerformanceAnomaly[] DetectedAnomalies { get; set; }
        
        /// <summary>
        /// Predicted future performance.
        /// </summary>
        public PerformancePrediction[] FuturePredictions { get; set; }
        
        /// <summary>
        /// Analysis confidence level (0-1).
        /// </summary>
        public float AnalysisConfidence { get; set; }
        
        /// <summary>
        /// Recommendations based on trend analysis.
        /// </summary>
        public string[] Recommendations { get; set; }
    }
    
    /// <summary>
    /// Metric-specific trend information.
    /// </summary>
    [Serializable]
    public class MetricTrend
    {
        public PerformanceMetricType MetricType { get; set; }
        public TrendDirection Direction { get; set; }
        public float Strength { get; set; }
        public float ChangeRate { get; set; } // Change per unit time
        public float Volatility { get; set; }
        public bool IsStable { get; set; }
    }
    
    /// <summary>
    /// Performance pattern information.
    /// </summary>
    [Serializable]
    public class PerformancePattern
    {
        public string PatternId { get; set; }
        public string PatternName { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public float Confidence { get; set; }
        public PerformanceMetricType[] AffectedMetrics { get; set; }
        public Dictionary<string, object> PatternData { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Performance anomaly information.
    /// </summary>
    [Serializable]
    public class PerformanceAnomaly
    {
        public string AnomalyId { get; set; }
        public DateTime DetectedAt { get; set; }
        public PerformanceMetricType AffectedMetric { get; set; }
        public AnomalySeverity Severity { get; set; }
        public float AnomalyScore { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsResolved { get; set; }
        public string[] PossibleCauses { get; set; }
    }
    
    /// <summary>
    /// Performance prediction information.
    /// </summary>
    [Serializable]
    public class PerformancePrediction
    {
        public DateTime PredictionTimestamp { get; set; }
        public TimeSpan PredictionHorizon { get; set; }
        public PerformanceMetricType MetricType { get; set; }
        public float PredictedValue { get; set; }
        public float ConfidenceInterval { get; set; }
        public float PredictionConfidence { get; set; }
        public string PredictionMethod { get; set; }
    }
    
    /// <summary>
    /// Performance monitoring report.
    /// </summary>
    [Serializable]
    public class PerformanceMonitoringReport
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
        /// Report time period.
        /// </summary>
        public TimeSpan ReportPeriod { get; set; }
        
        /// <summary>
        /// Session summary information.
        /// </summary>
        public PerformanceMonitoringSession SessionSummary { get; set; }
        
        /// <summary>
        /// Performance statistics.
        /// </summary>
        public PerformanceStatistics Statistics { get; set; }
        
        /// <summary>
        /// Trend analysis results.
        /// </summary>
        public PerformanceTrendAnalysis TrendAnalysis { get; set; }
        
        /// <summary>
        /// Violation summary.
        /// </summary>
        public ViolationSummary ViolationSummary { get; set; }
        
        /// <summary>
        /// Key performance insights.
        /// </summary>
        public string[] KeyInsights { get; set; }
        
        /// <summary>
        /// Performance recommendations.
        /// </summary>
        public string[] Recommendations { get; set; }
        
        /// <summary>
        /// Report quality score (0-1).
        /// </summary>
        public float ReportQuality { get; set; }
    }
    
    /// <summary>
    /// Violation summary information.
    /// </summary>
    [Serializable]
    public class ViolationSummary
    {
        public int TotalViolations { get; set; }
        public int CriticalViolations { get; set; }
        public int WarningViolations { get; set; }
        public int InfoViolations { get; set; }
        public TimeSpan TotalViolationTime { get; set; }
        public float ViolationRate { get; set; } // Violations per hour
        public Dictionary<PerformanceMetricType, int> ViolationsByMetric { get; set; } = new Dictionary<PerformanceMetricType, int>();
        public PerformanceViolation MostSevereViolation { get; set; }
        public PerformanceViolation LongestViolation { get; set; }
    }
    
    /// <summary>
    /// Performance data export result.
    /// </summary>
    [Serializable]
    public class PerformanceDataExport
    {
        /// <summary>
        /// Export identifier.
        /// </summary>
        public string ExportId { get; set; }
        
        /// <summary>
        /// Export timestamp.
        /// </summary>
        public DateTime ExportedAt { get; set; }
        
        /// <summary>
        /// Export format used.
        /// </summary>
        public DataExportFormat Format { get; set; }
        
        /// <summary>
        /// Time range exported.
        /// </summary>
        public TimeSpan TimeRange { get; set; }
        
        /// <summary>
        /// Number of data points exported.
        /// </summary>
        public int DataPointsExported { get; set; }
        
        /// <summary>
        /// Export file path or data.
        /// </summary>
        public string ExportData { get; set; }
        
        /// <summary>
        /// Export file size in bytes.
        /// </summary>
        public long FileSizeBytes { get; set; }
        
        /// <summary>
        /// Export quality indicators.
        /// </summary>
        public ExportQuality Quality { get; set; }
    }
    
    /// <summary>
    /// Export quality information.
    /// </summary>
    [Serializable]
    public class ExportQuality
    {
        public float DataCompleteness { get; set; } // 0-1
        public float DataAccuracy { get; set; } // 0-1
        public int MissingDataPoints { get; set; }
        public int CorruptedDataPoints { get; set; }
        public string[] QualityIssues { get; set; }
    }
    
    /// <summary>
    /// Monitoring session termination result.
    /// </summary>
    [Serializable]
    public class MonitoringTerminationResult
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
        /// Final session statistics.
        /// </summary>
        public PerformanceStatistics FinalStatistics { get; set; }
        
        /// <summary>
        /// Session duration.
        /// </summary>
        public TimeSpan SessionDuration { get; set; }
        
        /// <summary>
        /// Total data points collected.
        /// </summary>
        public int TotalDataPointsCollected { get; set; }
        
        /// <summary>
        /// Session summary.
        /// </summary>
        public string SessionSummary { get; set; }
        
        /// <summary>
        /// Termination messages.
        /// </summary>
        public string[] TerminationMessages { get; set; }
    }
    
    /// <summary>
    /// Performance dashboard data for real-time display.
    /// </summary>
    [Serializable]
    public class PerformanceDashboardData
    {
        /// <summary>
        /// Dashboard data timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Current performance metrics.
        /// </summary>
        public PerformanceMetrics CurrentMetrics { get; set; }
        
        /// <summary>
        /// Recent performance history (last few minutes).
        /// </summary>
        public PerformanceMetrics[] RecentHistory { get; set; }
        
        /// <summary>
        /// Active performance violations.
        /// </summary>
        public PerformanceViolation[] ActiveViolations { get; set; }
        
        /// <summary>
        /// Performance health indicators.
        /// </summary>
        public PerformanceHealthIndicators HealthIndicators { get; set; }
        
        /// <summary>
        /// Real-time alerts.
        /// </summary>
        public PerformanceAlert[] ActiveAlerts { get; set; }
        
        /// <summary>
        /// Performance trends (short-term).
        /// </summary>
        public Dictionary<PerformanceMetricType, TrendDirection> ShortTermTrends { get; set; } = new Dictionary<PerformanceMetricType, TrendDirection>();
        
        /// <summary>
        /// System status indicators.
        /// </summary>
        public SystemStatusIndicators SystemStatus { get; set; }
    }
    
    /// <summary>
    /// Performance health indicators.
    /// </summary>
    [Serializable]
    public class PerformanceHealthIndicators
    {
        public float OverallHealthScore { get; set; } // 0-1
        public float StabilityScore { get; set; } // 0-1
        public float EfficiencyScore { get; set; } // 0-1
        public float ReliabilityScore { get; set; } // 0-1
        public HealthStatus OverallStatus { get; set; }
        public Dictionary<PerformanceMetricType, HealthStatus> MetricHealthStatus { get; set; } = new Dictionary<PerformanceMetricType, HealthStatus>();
    }
    
    /// <summary>
    /// Performance alert information.
    /// </summary>
    [Serializable]
    public class PerformanceAlert
    {
        public string AlertId { get; set; }
        public DateTime TriggeredAt { get; set; }
        public AlertSeverity Severity { get; set; }
        public string AlertMessage { get; set; }
        public PerformanceMetricType AffectedMetric { get; set; }
        public float CurrentValue { get; set; }
        public float ThresholdValue { get; set; }
        public bool IsAcknowledged { get; set; }
        public string[] RecommendedActions { get; set; }
    }
    
    /// <summary>
    /// System status indicators.
    /// </summary>
    [Serializable]
    public class SystemStatusIndicators
    {
        public bool IsMonitoringActive { get; set; }
        public bool IsDataCollectionHealthy { get; set; }
        public bool AreThresholdsConfigured { get; set; }
        public bool IsAlertingFunctional { get; set; }
        public float SystemLoadPercentage { get; set; }
        public int ActiveSessions { get; set; }
        public DateTime LastDataCollection { get; set; }
        public string[] SystemMessages { get; set; }
    }
    
    /// <summary>
    /// Interface for custom performance metric collectors.
    /// </summary>
    public interface IPerformanceMetricCollector
    {
        /// <summary>
        /// Unique collector identifier.
        /// </summary>
        string CollectorId { get; }
        
        /// <summary>
        /// Collector name.
        /// </summary>
        string CollectorName { get; }
        
        /// <summary>
        /// Whether the collector is enabled.
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Collect custom performance metrics.
        /// </summary>
        /// <returns>Collected metrics</returns>
        Dictionary<string, float> CollectMetrics();
        
        /// <summary>
        /// Initialize the collector.
        /// </summary>
        /// <returns>True if initialization successful</returns>
        bool Initialize();
        
        /// <summary>
        /// Cleanup collector resources.
        /// </summary>
        void Cleanup();
    }
    
    // Supporting enums
    
    /// <summary>
    /// Monitoring session status.
    /// </summary>
    public enum MonitoringSessionStatus
    {
        NotStarted,
        Active,
        Paused,
        Completed,
        Terminated,
        Error
    }
    
    /// <summary>
    /// Monitoring priority levels.
    /// </summary>
    public enum MonitoringPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Trend direction enumeration.
    /// </summary>
    public enum TrendDirection
    {
        Declining,
        Stable,
        Improving
    }
    
    /// <summary>
    /// Anomaly severity levels.
    /// </summary>
    public enum AnomalySeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Data export formats.
    /// </summary>
    public enum DataExportFormat
    {
        JSON,
        CSV,
        XML,
        Binary
    }
    
    /// <summary>
    /// Health status levels.
    /// </summary>
    public enum HealthStatus
    {
        Critical,
        Warning,
        Good,
        Excellent
    }
    
    /// <summary>
    /// Alert severity levels.
    /// </summary>
    public enum AlertSeverity
    {
        Info,
        Warning,
        High,
        Critical
    }
    
    // Event argument classes
    
    /// <summary>
    /// Event arguments for performance violation detected.
    /// </summary>
    public class PerformanceViolationEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public PerformanceViolation Violation { get; set; }
        public PerformanceMetrics CurrentMetrics { get; set; }
        public DateTime DetectionTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for performance recovery.
    /// </summary>
    public class PerformanceRecoveryEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public PerformanceViolation ResolvedViolation { get; set; }
        public PerformanceMetrics CurrentMetrics { get; set; }
        public DateTime RecoveryTimestamp { get; set; }
        public TimeSpan RecoveryDuration { get; set; }
    }
    
    /// <summary>
    /// Event arguments for performance data collected.
    /// </summary>
    public class PerformanceDataCollectedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public PerformanceMetrics CollectedMetrics { get; set; }
        public DateTime CollectionTimestamp { get; set; }
        public float CollectionDuration { get; set; }
    }
    
    /// <summary>
    /// Event arguments for monitoring session started.
    /// </summary>
    public class MonitoringSessionStartedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public PerformanceMonitoringSession Session { get; set; }
        public DateTime StartTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for monitoring session ended.
    /// </summary>
    public class MonitoringSessionEndedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public MonitoringTerminationResult TerminationResult { get; set; }
        public DateTime EndTimestamp { get; set; }
    }
}