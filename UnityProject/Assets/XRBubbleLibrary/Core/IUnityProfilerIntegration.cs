using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for Unity Profiler Integration system.
    /// Implements Requirement 4.3: Unity editor profiler integration for automated testing.
    /// Implements Requirement 4.4: Median FPS calculation and threshold checking.
    /// Implements Requirement 4.5: Performance data analysis and reporting.
    /// </summary>
    public interface IUnityProfilerIntegration
    {
        /// <summary>
        /// Whether the profiler integration is initialized and ready.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Whether profiler is currently active and collecting data.
        /// </summary>
        bool IsProfilingActive { get; }
        
        /// <summary>
        /// Current profiler configuration settings.
        /// </summary>
        ProfilerConfiguration Configuration { get; }
        
        /// <summary>
        /// Event fired when profiling session starts.
        /// </summary>
        event Action<ProfilingSessionStartedEventArgs> ProfilingSessionStarted;
        
        /// <summary>
        /// Event fired when profiling session completes.
        /// </summary>
        event Action<ProfilingSessionCompletedEventArgs> ProfilingSessionCompleted;
        
        /// <summary>
        /// Event fired when performance threshold is violated.
        /// </summary>
        event Action<PerformanceThresholdViolatedEventArgs> PerformanceThresholdViolated;
        
        /// <summary>
        /// Initialize the profiler integration system.
        /// </summary>
        /// <param name="config">Profiler configuration</param>
        /// <returns>True if initialization successful</returns>
        bool Initialize(ProfilerConfiguration config);
        
        /// <summary>
        /// Start a profiling session with specified parameters.
        /// </summary>
        /// <param name="sessionConfig">Session configuration</param>
        /// <returns>Started profiling session</returns>
        Task<ProfilingSession> StartProfilingSession(ProfilingSessionConfiguration sessionConfig);
        
        /// <summary>
        /// Stop the current profiling session and return results.
        /// </summary>
        /// <returns>Profiling session results</returns>
        Task<ProfilingSessionResult> StopProfilingSession();
        
        /// <summary>
        /// Get real-time performance metrics during active session.
        /// </summary>
        /// <returns>Current performance metrics</returns>
        PerformanceMetrics GetRealTimeMetrics();
        
        /// <summary>
        /// Calculate median FPS from collected profiler data.
        /// </summary>
        /// <param name="sessionId">Profiling session identifier</param>
        /// <returns>Median FPS calculation result</returns>
        MedianFPSResult CalculateMedianFPS(string sessionId);
        
        /// <summary>
        /// Check performance data against configured thresholds.
        /// </summary>
        /// <param name="metrics">Performance metrics to check</param>
        /// <returns>Threshold check result</returns>
        ThresholdCheckResult CheckPerformanceThresholds(PerformanceMetrics metrics);
        
        /// <summary>
        /// Analyze performance data and generate insights.
        /// </summary>
        /// <param name="sessionId">Profiling session identifier</param>
        /// <returns>Performance analysis result</returns>
        PerformanceAnalysisResult AnalyzePerformanceData(string sessionId);
        
        /// <summary>
        /// Generate performance report for a profiling session.
        /// </summary>
        /// <param name="sessionId">Profiling session identifier</param>
        /// <param name="reportFormat">Report format</param>
        /// <returns>Generated performance report</returns>
        PerformanceReport GeneratePerformanceReport(string sessionId, ReportFormat reportFormat);
        
        /// <summary>
        /// Export profiler data in specified format.
        /// </summary>
        /// <param name="sessionId">Profiling session identifier</param>
        /// <param name="exportFormat">Export format</param>
        /// <returns>Exported profiler data</returns>
        ProfilerDataExport ExportProfilerData(string sessionId, DataExportFormat exportFormat);
        
        /// <summary>
        /// Configure performance thresholds for monitoring.
        /// </summary>
        /// <param name="thresholds">Performance thresholds configuration</param>
        void ConfigurePerformanceThresholds(PerformanceThresholds thresholds);
        
        /// <summary>
        /// Get historical performance data for trend analysis.
        /// </summary>
        /// <param name="timeRange">Time range for historical data</param>
        /// <returns>Historical performance data</returns>
        HistoricalPerformanceData GetHistoricalData(TimeRange timeRange);
        
        /// <summary>
        /// Validate profiler integration accuracy.
        /// </summary>
        /// <returns>Profiler validation result</returns>
        ProfilerValidationResult ValidateProfilerAccuracy();
        
        /// <summary>
        /// Reset profiler integration system state.
        /// </summary>
        void ResetProfilerIntegration();
    }
    
    /// <summary>
    /// Configuration for profiler integration system.
    /// </summary>
    [Serializable]
    public struct ProfilerConfiguration
    {
        /// <summary>
        /// Whether to enable deep profiling for detailed analysis.
        /// </summary>
        public bool EnableDeepProfiling;
        
        /// <summary>
        /// Profiler sampling frequency in Hz.
        /// </summary>
        public float SamplingFrequency;
        
        /// <summary>
        /// Maximum profiler data retention time in hours.
        /// </summary>
        public int MaxDataRetentionHours;
        
        /// <summary>
        /// Whether to enable GPU profiling.
        /// </summary>
        public bool EnableGPUProfiling;
        
        /// <summary>
        /// Whether to enable memory profiling.
        /// </summary>
        public bool EnableMemoryProfiling;
        
        /// <summary>
        /// Whether to enable audio profiling.
        /// </summary>
        public bool EnableAudioProfiling;
        
        /// <summary>
        /// Performance thresholds for monitoring.
        /// </summary>
        public PerformanceThresholds PerformanceThresholds;
        
        /// <summary>
        /// Whether to enable debug logging.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Default profiler configuration.
        /// </summary>
        public static ProfilerConfiguration Default => new ProfilerConfiguration
        {
            EnableDeepProfiling = false,
            SamplingFrequency = 60f,
            MaxDataRetentionHours = 24,
            EnableGPUProfiling = true,
            EnableMemoryProfiling = true,
            EnableAudioProfiling = false,
            PerformanceThresholds = PerformanceThresholds.Default,
            EnableDebugLogging = false
        };
    }
    
    /// <summary>
    /// Configuration for a profiling session.
    /// </summary>
    [Serializable]
    public struct ProfilingSessionConfiguration
    {
        /// <summary>
        /// Session name for identification.
        /// </summary>
        public string SessionName;
        
        /// <summary>
        /// Duration of profiling session in seconds.
        /// </summary>
        public float DurationSeconds;
        
        /// <summary>
        /// Target frame rate for performance validation.
        /// </summary>
        public float TargetFrameRate;
        
        /// <summary>
        /// Whether to enable real-time threshold checking.
        /// </summary>
        public bool EnableRealTimeThresholdChecking;
        
        /// <summary>
        /// Whether to capture detailed frame data.
        /// </summary>
        public bool CaptureDetailedFrameData;
        
        /// <summary>
        /// Custom performance thresholds for this session.
        /// </summary>
        public PerformanceThresholds? CustomThresholds;
        
        /// <summary>
        /// Session-specific notes.
        /// </summary>
        public string SessionNotes;
    }
    
    /// <summary>
    /// Performance thresholds for monitoring and validation.
    /// </summary>
    [Serializable]
    public struct PerformanceThresholds
    {
        /// <summary>
        /// Minimum acceptable FPS.
        /// </summary>
        public float MinimumFPS;
        
        /// <summary>
        /// Maximum acceptable frame time in milliseconds.
        /// </summary>
        public float MaximumFrameTimeMS;
        
        /// <summary>
        /// Maximum acceptable memory usage in MB.
        /// </summary>
        public float MaximumMemoryUsageMB;
        
        /// <summary>
        /// Maximum acceptable CPU usage percentage.
        /// </summary>
        public float MaximumCPUUsagePercent;
        
        /// <summary>
        /// Maximum acceptable GPU usage percentage.
        /// </summary>
        public float MaximumGPUUsagePercent;
        
        /// <summary>
        /// Maximum acceptable garbage collection time in milliseconds.
        /// </summary>
        public float MaximumGCTimeMS;
        
        /// <summary>
        /// Default performance thresholds for Quest 3.
        /// </summary>
        public static PerformanceThresholds Default => new PerformanceThresholds
        {
            MinimumFPS = 72f,
            MaximumFrameTimeMS = 13.89f, // 72 FPS = 13.89ms per frame
            MaximumMemoryUsageMB = 512f,
            MaximumCPUUsagePercent = 80f,
            MaximumGPUUsagePercent = 85f,
            MaximumGCTimeMS = 2f
        };
    }
    
    /// <summary>
    /// Profiling session information.
    /// </summary>
    [Serializable]
    public struct ProfilingSession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Session configuration.
        /// </summary>
        public ProfilingSessionConfiguration Configuration;
        
        /// <summary>
        /// Session start time.
        /// </summary>
        public DateTime StartTime;
        
        /// <summary>
        /// Session end time (if completed).
        /// </summary>
        public DateTime? EndTime;
        
        /// <summary>
        /// Current session status.
        /// </summary>
        public ProfilingSessionStatus Status;
        
        /// <summary>
        /// Collected performance data.
        /// </summary>
        public List<PerformanceDataPoint> PerformanceData;
        
        /// <summary>
        /// Session quality metrics.
        /// </summary>
        public SessionQualityMetrics QualityMetrics;
        
        /// <summary>
        /// Session notes and observations.
        /// </summary>
        public string SessionNotes;
    }
    
    /// <summary>
    /// Performance metrics captured from profiler.
    /// </summary>
    [Serializable]
    public struct PerformanceMetrics
    {
        /// <summary>
        /// Current frame rate in FPS.
        /// </summary>
        public float CurrentFPS;
        
        /// <summary>
        /// Current frame time in milliseconds.
        /// </summary>
        public float CurrentFrameTimeMS;
        
        /// <summary>
        /// Current memory usage in MB.
        /// </summary>
        public float CurrentMemoryUsageMB;
        
        /// <summary>
        /// Current CPU usage percentage.
        /// </summary>
        public float CurrentCPUUsagePercent;
        
        /// <summary>
        /// Current GPU usage percentage.
        /// </summary>
        public float CurrentGPUUsagePercent;
        
        /// <summary>
        /// Current garbage collection time in milliseconds.
        /// </summary>
        public float CurrentGCTimeMS;
        
        /// <summary>
        /// Number of draw calls in current frame.
        /// </summary>
        public int DrawCalls;
        
        /// <summary>
        /// Number of triangles rendered in current frame.
        /// </summary>
        public int Triangles;
        
        /// <summary>
        /// Number of vertices rendered in current frame.
        /// </summary>
        public int Vertices;
        
        /// <summary>
        /// Timestamp when metrics were captured.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Additional custom metrics.
        /// </summary>
        public Dictionary<string, float> CustomMetrics;
    }
    
    /// <summary>
    /// Individual performance data point.
    /// </summary>
    [Serializable]
    public struct PerformanceDataPoint
    {
        /// <summary>
        /// Data point identifier.
        /// </summary>
        public string DataPointId;
        
        /// <summary>
        /// Performance metrics for this data point.
        /// </summary>
        public PerformanceMetrics Metrics;
        
        /// <summary>
        /// Time offset from session start in seconds.
        /// </summary>
        public float TimeOffsetSeconds;
        
        /// <summary>
        /// Frame number since session start.
        /// </summary>
        public int FrameNumber;
        
        /// <summary>
        /// Data quality indicators.
        /// </summary>
        public DataQualityIndicators QualityIndicators;
    }
    
    /// <summary>
    /// Result of median FPS calculation.
    /// </summary>
    [Serializable]
    public struct MedianFPSResult
    {
        /// <summary>
        /// Calculated median FPS.
        /// </summary>
        public float MedianFPS;
        
        /// <summary>
        /// Average FPS for comparison.
        /// </summary>
        public float AverageFPS;
        
        /// <summary>
        /// Minimum FPS recorded.
        /// </summary>
        public float MinimumFPS;
        
        /// <summary>
        /// Maximum FPS recorded.
        /// </summary>
        public float MaximumFPS;
        
        /// <summary>
        /// Standard deviation of FPS values.
        /// </summary>
        public float StandardDeviation;
        
        /// <summary>
        /// 95th percentile FPS.
        /// </summary>
        public float Percentile95;
        
        /// <summary>
        /// 5th percentile FPS.
        /// </summary>
        public float Percentile5;
        
        /// <summary>
        /// Number of data points used in calculation.
        /// </summary>
        public int DataPointCount;
        
        /// <summary>
        /// Calculation timestamp.
        /// </summary>
        public DateTime CalculationTimestamp;
    }
    
    /// <summary>
    /// Result of threshold checking.
    /// </summary>
    [Serializable]
    public struct ThresholdCheckResult
    {
        /// <summary>
        /// Whether all thresholds passed.
        /// </summary>
        public bool AllThresholdsPassed;
        
        /// <summary>
        /// Individual threshold check results.
        /// </summary>
        public ThresholdViolation[] Violations;
        
        /// <summary>
        /// Overall performance score (0-100).
        /// </summary>
        public float PerformanceScore;
        
        /// <summary>
        /// Check timestamp.
        /// </summary>
        public DateTime CheckTimestamp;
        
        /// <summary>
        /// Performance metrics that were checked.
        /// </summary>
        public PerformanceMetrics CheckedMetrics;
        
        /// <summary>
        /// Thresholds that were applied.
        /// </summary>
        public PerformanceThresholds AppliedThresholds;
    }
    
    /// <summary>
    /// Individual threshold violation.
    /// </summary>
    [Serializable]
    public struct ThresholdViolation
    {
        /// <summary>
        /// Name of the violated threshold.
        /// </summary>
        public string ThresholdName;
        
        /// <summary>
        /// Actual value that violated the threshold.
        /// </summary>
        public float ActualValue;
        
        /// <summary>
        /// Threshold value that was violated.
        /// </summary>
        public float ThresholdValue;
        
        /// <summary>
        /// Severity of the violation.
        /// </summary>
        public ViolationSeverity Severity;
        
        /// <summary>
        /// Description of the violation.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Recommended action to resolve violation.
        /// </summary>
        public string RecommendedAction;
    }
    
    // Supporting enums and data structures
    
    /// <summary>
    /// Profiling session status.
    /// </summary>
    public enum ProfilingSessionStatus
    {
        NotStarted,
        Active,
        Paused,
        Completed,
        Terminated,
        Error
    }
    
    /// <summary>
    /// Violation severity levels.
    /// </summary>
    public enum ViolationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>
    /// Report format options.
    /// </summary>
    public enum ReportFormat
    {
        JSON,
        CSV,
        HTML,
        PDF,
        Markdown
    }
    
    /// <summary>
    /// Data export format options.
    /// </summary>
    public enum DataExportFormat
    {
        JSON,
        CSV,
        XML,
        Binary
    }
    
    // Event argument classes
    
    /// <summary>
    /// Event arguments for profiling session started.
    /// </summary>
    public class ProfilingSessionStartedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public ProfilingSessionConfiguration Configuration { get; set; }
        public DateTime StartTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for profiling session completed.
    /// </summary>
    public class ProfilingSessionCompletedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public ProfilingSessionResult Result { get; set; }
        public DateTime CompletionTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for performance threshold violated.
    /// </summary>
    public class PerformanceThresholdViolatedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public ThresholdViolation[] Violations { get; set; }
        public PerformanceMetrics CurrentMetrics { get; set; }
        public DateTime ViolationTimestamp { get; set; }
    }
    
    // Additional supporting structures would be defined in the implementation file
    // to keep this interface focused on the core contract
}