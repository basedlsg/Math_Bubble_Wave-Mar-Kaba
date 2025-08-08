using Unity.Mathematics;
using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Interface for Quest 3 hardware-specific performance validation.
    /// Provides comprehensive testing and validation for Quest 3 VR performance requirements.
    /// Implements Requirement 8.1: Quest 3 hardware performance validation.
    /// Implements Requirement 8.2: Real-time performance monitoring.
    /// Implements Requirement 8.3: Performance data collection and analysis.
    /// </summary>
    public interface IQuest3PerformanceValidator
    {
        /// <summary>
        /// Whether the validator is initialized and ready for testing.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Whether a performance validation session is currently running.
        /// </summary>
        bool IsValidationRunning { get; }
        
        /// <summary>
        /// Current Quest 3 performance configuration.
        /// </summary>
        Quest3PerformanceConfig Configuration { get; }
        
        /// <summary>
        /// Initialize the Quest 3 performance validator.
        /// </summary>
        /// <param name="config">Performance validation configuration</param>
        /// <returns>True if initialization succeeded</returns>
        bool Initialize(Quest3PerformanceConfig config);
        
        /// <summary>
        /// Start a comprehensive Quest 3 performance validation session.
        /// </summary>
        /// <param name="testDurationSeconds">Duration of the test session in seconds</param>
        /// <returns>Validation session handle</returns>
        Quest3ValidationSession StartValidationSession(float testDurationSeconds = 60f);
        
        /// <summary>
        /// Stop the current validation session and get results.
        /// </summary>
        /// <returns>Complete validation results</returns>
        Quest3ValidationResult StopValidationSession();
        
        /// <summary>
        /// Validate specific performance metrics against Quest 3 requirements.
        /// </summary>
        /// <param name="metrics">Performance metrics to validate</param>
        /// <returns>Validation result for the metrics</returns>
        Quest3MetricsValidationResult ValidateMetrics(Quest3PerformanceMetrics metrics);
        
        /// <summary>
        /// Run a quick performance check (5-10 seconds) for immediate feedback.
        /// </summary>
        /// <param name="bubbleCount">Number of bubbles to test with</param>
        /// <returns>Quick validation result</returns>
        Quest3QuickValidationResult RunQuickValidation(int bubbleCount = 100);
        
        /// <summary>
        /// Validate thermal performance and safety limits.
        /// </summary>
        /// <returns>Thermal validation result</returns>
        Quest3ThermalValidationResult ValidateThermalPerformance();
        
        /// <summary>
        /// Test performance across different bubble counts to find optimal settings.
        /// </summary>
        /// <param name="minBubbles">Minimum bubble count to test</param>
        /// <param name="maxBubbles">Maximum bubble count to test</param>
        /// <param name="stepSize">Step size for bubble count increments</param>
        /// <returns>Scalability test results</returns>
        Quest3ScalabilityTestResult TestPerformanceScalability(int minBubbles = 50, int maxBubbles = 200, int stepSize = 25);
        
        /// <summary>
        /// Get real-time performance metrics during validation.
        /// </summary>
        /// <returns>Current performance metrics</returns>
        Quest3PerformanceMetrics GetCurrentMetrics();
        
        /// <summary>
        /// Get historical performance data from previous sessions.
        /// </summary>
        /// <param name="sessionCount">Number of recent sessions to retrieve</param>
        /// <returns>Historical performance data</returns>
        Quest3PerformanceHistory GetPerformanceHistory(int sessionCount = 10);
        
        /// <summary>
        /// Generate a comprehensive performance report for Quest 3 certification.
        /// </summary>
        /// <returns>Detailed performance report</returns>
        Quest3PerformanceReport GeneratePerformanceReport();
        
        /// <summary>
        /// Validate that the application meets Quest 3 store requirements.
        /// </summary>
        /// <returns>Store compliance validation result</returns>
        Quest3StoreComplianceResult ValidateStoreCompliance();
        
        /// <summary>
        /// Configure performance monitoring parameters.
        /// </summary>
        /// <param name="config">Monitoring configuration</param>
        void ConfigureMonitoring(Quest3MonitoringConfig config);
        
        /// <summary>
        /// Clean up resources and stop all monitoring.
        /// </summary>
        void Dispose();
    }
    
    /// <summary>
    /// Configuration for Quest 3 performance validation.
    /// </summary>
    [Serializable]
    public struct Quest3PerformanceConfig
    {
        /// <summary>
        /// Target frame rate for Quest 3 (72 or 90 FPS).
        /// </summary>
        public float TargetFrameRate;
        
        /// <summary>
        /// Maximum acceptable frame time in milliseconds.
        /// </summary>
        public float MaxFrameTimeMs;
        
        /// <summary>
        /// CPU usage threshold (0.0 to 1.0).
        /// </summary>
        public float CpuUsageThreshold;
        
        /// <summary>
        /// GPU usage threshold (0.0 to 1.0).
        /// </summary>
        public float GpuUsageThreshold;
        
        /// <summary>
        /// Maximum acceptable temperature in Celsius.
        /// </summary>
        public float MaxTemperatureCelsius;
        
        /// <summary>
        /// Memory usage threshold in MB.
        /// </summary>
        public float MemoryUsageThresholdMB;
        
        /// <summary>
        /// Whether to enable thermal monitoring.
        /// </summary>
        public bool EnableThermalMonitoring;
        
        /// <summary>
        /// Whether to enable detailed profiling.
        /// </summary>
        public bool EnableDetailedProfiling;
        
        /// <summary>
        /// Minimum test duration for reliable results.
        /// </summary>
        public float MinTestDurationSeconds;
        
        /// <summary>
        /// Default Quest 3 performance configuration.
        /// </summary>
        public static Quest3PerformanceConfig Default => new Quest3PerformanceConfig
        {
            TargetFrameRate = 72f,
            MaxFrameTimeMs = 13.89f, // 72 FPS
            CpuUsageThreshold = 0.8f,
            GpuUsageThreshold = 0.85f,
            MaxTemperatureCelsius = 45f,
            MemoryUsageThresholdMB = 2048f, // 2GB
            EnableThermalMonitoring = true,
            EnableDetailedProfiling = true,
            MinTestDurationSeconds = 10f
        };
        
        /// <summary>
        /// High performance Quest 3 configuration (90 FPS).
        /// </summary>
        public static Quest3PerformanceConfig HighPerformance => new Quest3PerformanceConfig
        {
            TargetFrameRate = 90f,
            MaxFrameTimeMs = 11.11f, // 90 FPS
            CpuUsageThreshold = 0.75f,
            GpuUsageThreshold = 0.8f,
            MaxTemperatureCelsius = 42f,
            MemoryUsageThresholdMB = 1536f, // 1.5GB
            EnableThermalMonitoring = true,
            EnableDetailedProfiling = true,
            MinTestDurationSeconds = 15f
        };
    }
    
    /// <summary>
    /// Quest 3 performance validation session handle.
    /// </summary>
    public struct Quest3ValidationSession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Session start time.
        /// </summary>
        public DateTime StartTime;
        
        /// <summary>
        /// Planned session duration in seconds.
        /// </summary>
        public float PlannedDurationSeconds;
        
        /// <summary>
        /// Configuration used for this session.
        /// </summary>
        public Quest3PerformanceConfig Configuration;
        
        /// <summary>
        /// Whether the session is currently active.
        /// </summary>
        public bool IsActive;
    }
    
    /// <summary>
    /// Comprehensive Quest 3 performance validation result.
    /// </summary>
    public struct Quest3ValidationResult
    {
        /// <summary>
        /// Whether the application meets Quest 3 performance requirements.
        /// </summary>
        public bool MeetsPerformanceRequirements;
        
        /// <summary>
        /// Overall performance score (0.0 to 1.0).
        /// </summary>
        public float PerformanceScore;
        
        /// <summary>
        /// Session information.
        /// </summary>
        public Quest3ValidationSession Session;
        
        /// <summary>
        /// Detailed performance metrics.
        /// </summary>
        public Quest3PerformanceMetrics Metrics;
        
        /// <summary>
        /// Performance issues found during validation.
        /// </summary>
        public Quest3PerformanceIssue[] Issues;
        
        /// <summary>
        /// Recommendations for performance improvement.
        /// </summary>
        public string[] Recommendations;
        
        /// <summary>
        /// Detailed validation report.
        /// </summary>
        public string ValidationReport;
        
        /// <summary>
        /// Time when validation completed.
        /// </summary>
        public DateTime CompletionTime;
        
        /// <summary>
        /// Actual test duration in seconds.
        /// </summary>
        public float ActualDurationSeconds;
    }
    
    /// <summary>
    /// Quest 3 performance metrics.
    /// </summary>
    public struct Quest3PerformanceMetrics
    {
        /// <summary>
        /// Average frame rate in FPS.
        /// </summary>
        public float AverageFrameRate;
        
        /// <summary>
        /// Minimum frame rate observed.
        /// </summary>
        public float MinFrameRate;
        
        /// <summary>
        /// Maximum frame rate observed.
        /// </summary>
        public float MaxFrameRate;
        
        /// <summary>
        /// Frame rate standard deviation.
        /// </summary>
        public float FrameRateStdDev;
        
        /// <summary>
        /// Average frame time in milliseconds.
        /// </summary>
        public float AverageFrameTimeMs;
        
        /// <summary>
        /// 95th percentile frame time.
        /// </summary>
        public float P95FrameTimeMs;
        
        /// <summary>
        /// 99th percentile frame time.
        /// </summary>
        public float P99FrameTimeMs;
        
        /// <summary>
        /// Number of dropped frames.
        /// </summary>
        public int DroppedFrames;
        
        /// <summary>
        /// Average CPU usage (0.0 to 1.0).
        /// </summary>
        public float AverageCpuUsage;
        
        /// <summary>
        /// Peak CPU usage (0.0 to 1.0).
        /// </summary>
        public float PeakCpuUsage;
        
        /// <summary>
        /// Average GPU usage (0.0 to 1.0).
        /// </summary>
        public float AverageGpuUsage;
        
        /// <summary>
        /// Peak GPU usage (0.0 to 1.0).
        /// </summary>
        public float PeakGpuUsage;
        
        /// <summary>
        /// Memory usage in MB.
        /// </summary>
        public float MemoryUsageMB;
        
        /// <summary>
        /// Peak memory usage in MB.
        /// </summary>
        public float PeakMemoryUsageMB;
        
        /// <summary>
        /// Average temperature in Celsius.
        /// </summary>
        public float AverageTemperatureCelsius;
        
        /// <summary>
        /// Peak temperature in Celsius.
        /// </summary>
        public float PeakTemperatureCelsius;
        
        /// <summary>
        /// Battery usage rate (mAh/hour).
        /// </summary>
        public float BatteryUsageRate;
        
        /// <summary>
        /// Total number of frames measured.
        /// </summary>
        public long TotalFrames;
        
        /// <summary>
        /// Measurement duration in seconds.
        /// </summary>
        public float MeasurementDurationSeconds;
    }
    
    /// <summary>
    /// Performance issue found during Quest 3 validation.
    /// </summary>
    public struct Quest3PerformanceIssue
    {
        /// <summary>
        /// Type of performance issue.
        /// </summary>
        public Quest3IssueType IssueType;
        
        /// <summary>
        /// Severity of the issue.
        /// </summary>
        public Quest3IssueSeverity Severity;
        
        /// <summary>
        /// Description of the issue.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Measured value that caused the issue.
        /// </summary>
        public float MeasuredValue;
        
        /// <summary>
        /// Expected/threshold value.
        /// </summary>
        public float ThresholdValue;
        
        /// <summary>
        /// Recommended action to resolve the issue.
        /// </summary>
        public string RecommendedAction;
        
        /// <summary>
        /// Time when the issue was detected.
        /// </summary>
        public DateTime DetectionTime;
    }
    
    /// <summary>
    /// Types of Quest 3 performance issues.
    /// </summary>
    public enum Quest3IssueType
    {
        FrameRateBelow72FPS,
        FrameRateBelow90FPS,
        HighFrameTime,
        DroppedFrames,
        HighCpuUsage,
        HighGpuUsage,
        HighMemoryUsage,
        ThermalThrottling,
        BatteryDrain,
        InconsistentFrameRate,
        RenderingArtifacts,
        PerformanceRegression
    }
    
    /// <summary>
    /// Severity levels for Quest 3 performance issues.
    /// </summary>
    public enum Quest3IssueSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>
    /// Result of Quest 3 metrics validation.
    /// </summary>
    public struct Quest3MetricsValidationResult
    {
        /// <summary>
        /// Whether the metrics meet Quest 3 requirements.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// Validation score (0.0 to 1.0).
        /// </summary>
        public float ValidationScore;
        
        /// <summary>
        /// Issues found in the metrics.
        /// </summary>
        public Quest3PerformanceIssue[] Issues;
        
        /// <summary>
        /// Validation summary message.
        /// </summary>
        public string ValidationSummary;
    }
    
    /// <summary>
    /// Result of Quest 3 quick validation.
    /// </summary>
    public struct Quest3QuickValidationResult
    {
        /// <summary>
        /// Whether quick validation passed.
        /// </summary>
        public bool Passed;
        
        /// <summary>
        /// Quick performance score.
        /// </summary>
        public float QuickScore;
        
        /// <summary>
        /// Average frame rate during quick test.
        /// </summary>
        public float AverageFrameRate;
        
        /// <summary>
        /// Any critical issues found.
        /// </summary>
        public Quest3PerformanceIssue[] CriticalIssues;
        
        /// <summary>
        /// Test duration in seconds.
        /// </summary>
        public float TestDurationSeconds;
        
        /// <summary>
        /// Quick validation summary.
        /// </summary>
        public string Summary;
    }
    
    /// <summary>
    /// Result of Quest 3 thermal validation.
    /// </summary>
    public struct Quest3ThermalValidationResult
    {
        /// <summary>
        /// Whether thermal performance is acceptable.
        /// </summary>
        public bool ThermalPerformanceAcceptable;
        
        /// <summary>
        /// Peak temperature reached in Celsius.
        /// </summary>
        public float PeakTemperatureCelsius;
        
        /// <summary>
        /// Average temperature in Celsius.
        /// </summary>
        public float AverageTemperatureCelsius;
        
        /// <summary>
        /// Whether thermal throttling was detected.
        /// </summary>
        public bool ThermalThrottlingDetected;
        
        /// <summary>
        /// Time to reach thermal equilibrium in seconds.
        /// </summary>
        public float TimeToThermalEquilibriumSeconds;
        
        /// <summary>
        /// Thermal safety margin in Celsius.
        /// </summary>
        public float ThermalSafetyMarginCelsius;
        
        /// <summary>
        /// Thermal validation summary.
        /// </summary>
        public string ThermalSummary;
    }
    
    /// <summary>
    /// Result of Quest 3 scalability testing.
    /// </summary>
    public struct Quest3ScalabilityTestResult
    {
        /// <summary>
        /// Whether scalability testing passed.
        /// </summary>
        public bool ScalabilityTestPassed;
        
        /// <summary>
        /// Recommended maximum bubble count for 72 FPS.
        /// </summary>
        public int RecommendedMaxBubbles72FPS;
        
        /// <summary>
        /// Recommended maximum bubble count for 90 FPS.
        /// </summary>
        public int RecommendedMaxBubbles90FPS;
        
        /// <summary>
        /// Performance data points for different bubble counts.
        /// </summary>
        public Quest3ScalabilityDataPoint[] DataPoints;
        
        /// <summary>
        /// Performance scaling characteristics.
        /// </summary>
        public string ScalingCharacteristics;
    }
    
    /// <summary>
    /// Data point for scalability testing.
    /// </summary>
    public struct Quest3ScalabilityDataPoint
    {
        /// <summary>
        /// Number of bubbles tested.
        /// </summary>
        public int BubbleCount;
        
        /// <summary>
        /// Average frame rate achieved.
        /// </summary>
        public float AverageFrameRate;
        
        /// <summary>
        /// Average frame time in milliseconds.
        /// </summary>
        public float AverageFrameTimeMs;
        
        /// <summary>
        /// CPU usage during test.
        /// </summary>
        public float CpuUsage;
        
        /// <summary>
        /// GPU usage during test.
        /// </summary>
        public float GpuUsage;
        
        /// <summary>
        /// Memory usage in MB.
        /// </summary>
        public float MemoryUsageMB;
    }
    
    /// <summary>
    /// Quest 3 performance history data.
    /// </summary>
    public struct Quest3PerformanceHistory
    {
        /// <summary>
        /// Historical validation sessions.
        /// </summary>
        public Quest3ValidationResult[] Sessions;
        
        /// <summary>
        /// Performance trend analysis.
        /// </summary>
        public string TrendAnalysis;
        
        /// <summary>
        /// Average performance score over time.
        /// </summary>
        public float AveragePerformanceScore;
        
        /// <summary>
        /// Performance improvement or regression rate.
        /// </summary>
        public float PerformanceChangeRate;
    }
    
    /// <summary>
    /// Comprehensive Quest 3 performance report.
    /// </summary>
    public struct Quest3PerformanceReport
    {
        /// <summary>
        /// Report generation timestamp.
        /// </summary>
        public DateTime ReportTimestamp;
        
        /// <summary>
        /// Overall certification status.
        /// </summary>
        public bool CertificationReady;
        
        /// <summary>
        /// Executive summary of performance.
        /// </summary>
        public string ExecutiveSummary;
        
        /// <summary>
        /// Detailed performance analysis.
        /// </summary>
        public string DetailedAnalysis;
        
        /// <summary>
        /// All validation results included in report.
        /// </summary>
        public Quest3ValidationResult[] ValidationResults;
        
        /// <summary>
        /// Performance recommendations.
        /// </summary>
        public string[] Recommendations;
        
        /// <summary>
        /// Technical specifications summary.
        /// </summary>
        public string TechnicalSpecifications;
    }
    
    /// <summary>
    /// Quest 3 store compliance validation result.
    /// </summary>
    public struct Quest3StoreComplianceResult
    {
        /// <summary>
        /// Whether the application meets store requirements.
        /// </summary>
        public bool MeetsStoreRequirements;
        
        /// <summary>
        /// Compliance score (0.0 to 1.0).
        /// </summary>
        public float ComplianceScore;
        
        /// <summary>
        /// Store requirement violations.
        /// </summary>
        public string[] Violations;
        
        /// <summary>
        /// Required fixes for store submission.
        /// </summary>
        public string[] RequiredFixes;
        
        /// <summary>
        /// Compliance validation summary.
        /// </summary>
        public string ComplianceSummary;
    }
    
    /// <summary>
    /// Configuration for Quest 3 performance monitoring.
    /// </summary>
    [Serializable]
    public struct Quest3MonitoringConfig
    {
        /// <summary>
        /// Monitoring sample rate in Hz.
        /// </summary>
        public float SampleRateHz;
        
        /// <summary>
        /// Whether to log performance data to file.
        /// </summary>
        public bool LogToFile;
        
        /// <summary>
        /// Whether to display real-time performance overlay.
        /// </summary>
        public bool ShowPerformanceOverlay;
        
        /// <summary>
        /// Whether to enable automatic issue detection.
        /// </summary>
        public bool EnableAutomaticIssueDetection;
        
        /// <summary>
        /// Performance alert thresholds.
        /// </summary>
        public Quest3PerformanceConfig AlertThresholds;
        
        /// <summary>
        /// Default monitoring configuration.
        /// </summary>
        public static Quest3MonitoringConfig Default => new Quest3MonitoringConfig
        {
            SampleRateHz = 10f,
            LogToFile = true,
            ShowPerformanceOverlay = false,
            EnableAutomaticIssueDetection = true,
            AlertThresholds = Quest3PerformanceConfig.Default
        };
    }
}