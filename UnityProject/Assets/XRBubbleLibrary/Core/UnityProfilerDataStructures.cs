using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Supporting data structures for Unity Profiler Integration system.
    /// </summary>
    
    /// <summary>
    /// Result of a completed profiling session.
    /// </summary>
    [Serializable]
    public struct ProfilingSessionResult
    {
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Whether session completed successfully.
        /// </summary>
        public bool CompletedSuccessfully;
        
        /// <summary>
        /// Session completion timestamp.
        /// </summary>
        public DateTime CompletionTimestamp;
        
        /// <summary>
        /// Total session duration.
        /// </summary>
        public TimeSpan SessionDuration;
        
        /// <summary>
        /// Total number of frames captured.
        /// </summary>
        public int TotalFramesCaptured;
        
        /// <summary>
        /// Session performance summary.
        /// </summary>
        public PerformanceSummary PerformanceSummary;
        
        /// <summary>
        /// Median FPS calculation result.
        /// </summary>
        public MedianFPSResult MedianFPSResult;
        
        /// <summary>
        /// Threshold check results.
        /// </summary>
        public ThresholdCheckResult[] ThresholdCheckResults;
        
        /// <summary>
        /// Session quality assessment.
        /// </summary>
        public SessionQualityMetrics QualityMetrics;
        
        /// <summary>
        /// Session completion notes.
        /// </summary>
        public string CompletionNotes;
    }
    
    /// <summary>
    /// Performance summary for a profiling session.
    /// </summary>
    [Serializable]
    public struct PerformanceSummary
    {
        /// <summary>
        /// Average FPS during session.
        /// </summary>
        public float AverageFPS;
        
        /// <summary>
        /// Median FPS during session.
        /// </summary>
        public float MedianFPS;
        
        /// <summary>
        /// Minimum FPS recorded.
        /// </summary>
        public float MinimumFPS;
        
        /// <summary>
        /// Maximum FPS recorded.
        /// </summary>
        public float MaximumFPS;
        
        /// <summary>
        /// Average frame time in milliseconds.
        /// </summary>
        public float AverageFrameTimeMS;
        
        /// <summary>
        /// Average memory usage in MB.
        /// </summary>
        public float AverageMemoryUsageMB;
        
        /// <summary>
        /// Peak memory usage in MB.
        /// </summary>
        public float PeakMemoryUsageMB;
        
        /// <summary>
        /// Average CPU usage percentage.
        /// </summary>
        public float AverageCPUUsagePercent;
        
        /// <summary>
        /// Average GPU usage percentage.
        /// </summary>
        public float AverageGPUUsagePercent;
        
        /// <summary>
        /// Total garbage collection time in milliseconds.
        /// </summary>
        public float TotalGCTimeMS;
        
        /// <summary>
        /// Number of garbage collection events.
        /// </summary>
        public int GCEventCount;
        
        /// <summary>
        /// Average draw calls per frame.
        /// </summary>
        public float AverageDrawCalls;
        
        /// <summary>
        /// Average triangles per frame.
        /// </summary>
        public float AverageTriangles;
    }
    
    /// <summary>
    /// Session quality metrics.
    /// </summary>
    [Serializable]
    public struct SessionQualityMetrics
    {
        /// <summary>
        /// Overall session quality score (0-100).
        /// </summary>
        public float OverallQualityScore;
        
        /// <summary>
        /// Data completeness percentage.
        /// </summary>
        public float DataCompletenessPercent;
        
        /// <summary>
        /// Data consistency score (0-100).
        /// </summary>
        public float DataConsistencyScore;
        
        /// <summary>
        /// Number of data collection errors.
        /// </summary>
        public int DataCollectionErrors;
        
        /// <summary>
        /// Number of threshold violations.
        /// </summary>
        public int ThresholdViolations;
        
        /// <summary>
        /// Session reliability score (0-100).
        /// </summary>
        public float ReliabilityScore;
        
        /// <summary>
        /// Quality assessment notes.
        /// </summary>
        public string[] QualityNotes;
    }
    
    /// <summary>
    /// Data quality indicators for performance data points.
    /// </summary>
    [Serializable]
    public struct DataQualityIndicators
    {
        /// <summary>
        /// Data point quality score (0-100).
        /// </summary>
        public float QualityScore;
        
        /// <summary>
        /// Whether data point is considered reliable.
        /// </summary>
        public bool IsReliable;
        
        /// <summary>
        /// Data collection accuracy score (0-100).
        /// </summary>
        public float AccuracyScore;
        
        /// <summary>
        /// Data collection timeliness score (0-100).
        /// </summary>
        public float TimelinessScore;
        
        /// <summary>
        /// Quality flags for this data point.
        /// </summary>
        public string[] QualityFlags;
        
        /// <summary>
        /// Anomaly detection results.
        /// </summary>
        public AnomalyDetectionResult[] AnomalyResults;
    }
    
    /// <summary>
    /// Anomaly detection result.
    /// </summary>
    [Serializable]
    public struct AnomalyDetectionResult
    {
        /// <summary>
        /// Type of anomaly detected.
        /// </summary>
        public AnomalyType Type;
        
        /// <summary>
        /// Anomaly severity level.
        /// </summary>
        public AnomalySeverity Severity;
        
        /// <summary>
        /// Anomaly confidence score (0-100).
        /// </summary>
        public float ConfidenceScore;
        
        /// <summary>
        /// Description of the anomaly.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Recommended action for the anomaly.
        /// </summary>
        public string RecommendedAction;
    }
    
    /// <summary>
    /// Performance analysis result.
    /// </summary>
    [Serializable]
    public struct PerformanceAnalysisResult
    {
        /// <summary>
        /// Analysis identifier.
        /// </summary>
        public string AnalysisId;
        
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp;
        
        /// <summary>
        /// Performance trend analysis.
        /// </summary>
        public PerformanceTrendAnalysis TrendAnalysis;
        
        /// <summary>
        /// Bottleneck analysis results.
        /// </summary>
        public BottleneckAnalysis BottleneckAnalysis;
        
        /// <summary>
        /// Memory usage analysis.
        /// </summary>
        public MemoryUsageAnalysis MemoryAnalysis;
        
        /// <summary>
        /// Frame time analysis.
        /// </summary>
        public FrameTimeAnalysis FrameTimeAnalysis;
        
        /// <summary>
        /// Key performance insights.
        /// </summary>
        public string[] KeyInsights;
        
        /// <summary>
        /// Performance optimization recommendations.
        /// </summary>
        public string[] OptimizationRecommendations;
        
        /// <summary>
        /// Analysis confidence level (0-100).
        /// </summary>
        public float AnalysisConfidence;
    }
    
    /// <summary>
    /// Performance trend analysis.
    /// </summary>
    [Serializable]
    public struct PerformanceTrendAnalysis
    {
        /// <summary>
        /// Overall performance trend direction.
        /// </summary>
        public TrendDirection OverallTrend;
        
        /// <summary>
        /// FPS trend analysis.
        /// </summary>
        public MetricTrend FPSTrend;
        
        /// <summary>
        /// Memory usage trend analysis.
        /// </summary>
        public MetricTrend MemoryTrend;
        
        /// <summary>
        /// CPU usage trend analysis.
        /// </summary>
        public MetricTrend CPUTrend;
        
        /// <summary>
        /// GPU usage trend analysis.
        /// </summary>
        public MetricTrend GPUTrend;
        
        /// <summary>
        /// Trend change points.
        /// </summary>
        public TrendChangePoint[] ChangePoints;
        
        /// <summary>
        /// Trend stability score (0-100).
        /// </summary>
        public float StabilityScore;
    }
    
    /// <summary>
    /// Individual metric trend analysis.
    /// </summary>
    [Serializable]
    public struct MetricTrend
    {
        /// <summary>
        /// Metric name.
        /// </summary>
        public string MetricName;
        
        /// <summary>
        /// Trend direction.
        /// </summary>
        public TrendDirection Direction;
        
        /// <summary>
        /// Trend strength (0-100).
        /// </summary>
        public float Strength;
        
        /// <summary>
        /// Trend confidence (0-100).
        /// </summary>
        public float Confidence;
        
        /// <summary>
        /// Rate of change per second.
        /// </summary>
        public float RateOfChange;
    }
    
    /// <summary>
    /// Trend change point information.
    /// </summary>
    [Serializable]
    public struct TrendChangePoint
    {
        /// <summary>
        /// Timestamp of the change point.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Time offset from session start in seconds.
        /// </summary>
        public float TimeOffsetSeconds;
        
        /// <summary>
        /// Trend before the change point.
        /// </summary>
        public TrendDirection BeforeTrend;
        
        /// <summary>
        /// Trend after the change point.
        /// </summary>
        public TrendDirection AfterTrend;
        
        /// <summary>
        /// Significance of the change (0-100).
        /// </summary>
        public float ChangeSignificance;
        
        /// <summary>
        /// Description of the change.
        /// </summary>
        public string Description;
    }
    
    /// <summary>
    /// Bottleneck analysis results.
    /// </summary>
    [Serializable]
    public struct BottleneckAnalysis
    {
        /// <summary>
        /// Primary bottleneck identified.
        /// </summary>
        public PerformanceBottleneck PrimaryBottleneck;
        
        /// <summary>
        /// Secondary bottlenecks identified.
        /// </summary>
        public PerformanceBottleneck[] SecondaryBottlenecks;
        
        /// <summary>
        /// CPU bottleneck analysis.
        /// </summary>
        public CPUBottleneckAnalysis CPUAnalysis;
        
        /// <summary>
        /// GPU bottleneck analysis.
        /// </summary>
        public GPUBottleneckAnalysis GPUAnalysis;
        
        /// <summary>
        /// Memory bottleneck analysis.
        /// </summary>
        public MemoryBottleneckAnalysis MemoryAnalysis;
        
        /// <summary>
        /// Bottleneck resolution recommendations.
        /// </summary>
        public string[] ResolutionRecommendations;
    }
    
    /// <summary>
    /// Individual performance bottleneck.
    /// </summary>
    [Serializable]
    public struct PerformanceBottleneck
    {
        /// <summary>
        /// Bottleneck identifier.
        /// </summary>
        public string BottleneckId;
        
        /// <summary>
        /// Bottleneck type.
        /// </summary>
        public BottleneckType Type;
        
        /// <summary>
        /// Bottleneck severity (0-100).
        /// </summary>
        public float Severity;
        
        /// <summary>
        /// Impact on overall performance (0-100).
        /// </summary>
        public float PerformanceImpact;
        
        /// <summary>
        /// Description of the bottleneck.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Recommended resolution steps.
        /// </summary>
        public string[] ResolutionSteps;
    }
    
    /// <summary>
    /// Memory usage analysis results.
    /// </summary>
    [Serializable]
    public struct MemoryUsageAnalysis
    {
        /// <summary>
        /// Memory allocation pattern analysis.
        /// </summary>
        public MemoryAllocationPattern AllocationPattern;
        
        /// <summary>
        /// Garbage collection analysis.
        /// </summary>
        public GarbageCollectionAnalysis GCAnalysis;
        
        /// <summary>
        /// Memory leak detection results.
        /// </summary>
        public MemoryLeakDetection LeakDetection;
        
        /// <summary>
        /// Memory optimization recommendations.
        /// </summary>
        public string[] OptimizationRecommendations;
    }
    
    /// <summary>
    /// Frame time analysis results.
    /// </summary>
    [Serializable]
    public struct FrameTimeAnalysis
    {
        /// <summary>
        /// Frame time distribution analysis.
        /// </summary>
        public FrameTimeDistribution Distribution;
        
        /// <summary>
        /// Frame time spike analysis.
        /// </summary>
        public FrameTimeSpike[] Spikes;
        
        /// <summary>
        /// Frame time consistency score (0-100).
        /// </summary>
        public float ConsistencyScore;
        
        /// <summary>
        /// Frame time optimization recommendations.
        /// </summary>
        public string[] OptimizationRecommendations;
    }
    
    /// <summary>
    /// Performance report generated from profiler data.
    /// </summary>
    [Serializable]
    public struct PerformanceReport
    {
        /// <summary>
        /// Report identifier.
        /// </summary>
        public string ReportId;
        
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Report format.
        /// </summary>
        public ReportFormat Format;
        
        /// <summary>
        /// Report title.
        /// </summary>
        public string Title;
        
        /// <summary>
        /// Report content.
        /// </summary>
        public string Content;
        
        /// <summary>
        /// Report summary.
        /// </summary>
        public string Summary;
        
        /// <summary>
        /// Report generation timestamp.
        /// </summary>
        public DateTime GenerationTimestamp;
        
        /// <summary>
        /// Report metadata.
        /// </summary>
        public Dictionary<string, object> Metadata;
    }
    
    /// <summary>
    /// Profiler data export.
    /// </summary>
    [Serializable]
    public struct ProfilerDataExport
    {
        /// <summary>
        /// Export identifier.
        /// </summary>
        public string ExportId;
        
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Export format.
        /// </summary>
        public DataExportFormat Format;
        
        /// <summary>
        /// Export content.
        /// </summary>
        public string Content;
        
        /// <summary>
        /// Whether export was successful.
        /// </summary>
        public bool IsSuccessful;
        
        /// <summary>
        /// Export timestamp.
        /// </summary>
        public DateTime ExportTimestamp;
        
        /// <summary>
        /// Number of data points exported.
        /// </summary>
        public int DataPointCount;
        
        /// <summary>
        /// Export file path.
        /// </summary>
        public string FilePath;
        
        /// <summary>
        /// Export messages.
        /// </summary>
        public string[] ExportMessages;
    }
    
    /// <summary>
    /// Historical performance data.
    /// </summary>
    [Serializable]
    public struct HistoricalPerformanceData
    {
        /// <summary>
        /// Time range for the historical data.
        /// </summary>
        public TimeRange TimeRange;
        
        /// <summary>
        /// Historical performance sessions.
        /// </summary>
        public HistoricalSession[] Sessions;
        
        /// <summary>
        /// Performance trend over time.
        /// </summary>
        public PerformanceTrendAnalysis OverallTrend;
        
        /// <summary>
        /// Performance statistics.
        /// </summary>
        public HistoricalStatistics Statistics;
        
        /// <summary>
        /// Data retrieval timestamp.
        /// </summary>
        public DateTime RetrievalTimestamp;
    }
    
    /// <summary>
    /// Time range specification.
    /// </summary>
    [Serializable]
    public struct TimeRange
    {
        /// <summary>
        /// Start time of the range.
        /// </summary>
        public DateTime StartTime;
        
        /// <summary>
        /// End time of the range.
        /// </summary>
        public DateTime EndTime;
        
        /// <summary>
        /// Duration of the time range.
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;
    }
    
    /// <summary>
    /// Historical session summary.
    /// </summary>
    [Serializable]
    public struct HistoricalSession
    {
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Session timestamp.
        /// </summary>
        public DateTime SessionTimestamp;
        
        /// <summary>
        /// Session performance summary.
        /// </summary>
        public PerformanceSummary PerformanceSummary;
        
        /// <summary>
        /// Session quality score.
        /// </summary>
        public float QualityScore;
    }
    
    /// <summary>
    /// Historical performance statistics.
    /// </summary>
    [Serializable]
    public struct HistoricalStatistics
    {
        /// <summary>
        /// Total number of sessions.
        /// </summary>
        public int TotalSessions;
        
        /// <summary>
        /// Average performance across all sessions.
        /// </summary>
        public PerformanceSummary AveragePerformance;
        
        /// <summary>
        /// Best performance recorded.
        /// </summary>
        public PerformanceSummary BestPerformance;
        
        /// <summary>
        /// Worst performance recorded.
        /// </summary>
        public PerformanceSummary WorstPerformance;
        
        /// <summary>
        /// Performance improvement over time.
        /// </summary>
        public float PerformanceImprovement;
    }
    
    /// <summary>
    /// Profiler validation result.
    /// </summary>
    [Serializable]
    public struct ProfilerValidationResult
    {
        /// <summary>
        /// Whether profiler validation passed.
        /// </summary>
        public bool ValidationPassed;
        
        /// <summary>
        /// Profiler accuracy score (0-100).
        /// </summary>
        public float AccuracyScore;
        
        /// <summary>
        /// Validation timestamp.
        /// </summary>
        public DateTime ValidationTimestamp;
        
        /// <summary>
        /// Validation test results.
        /// </summary>
        public ValidationTestResult[] TestResults;
        
        /// <summary>
        /// Validation messages.
        /// </summary>
        public string[] ValidationMessages;
        
        /// <summary>
        /// Validation recommendations.
        /// </summary>
        public string[] ValidationRecommendations;
    }
    
    /// <summary>
    /// Individual validation test result.
    /// </summary>
    [Serializable]
    public struct ValidationTestResult
    {
        /// <summary>
        /// Test name.
        /// </summary>
        public string TestName;
        
        /// <summary>
        /// Whether test passed.
        /// </summary>
        public bool Passed;
        
        /// <summary>
        /// Test score (0-100).
        /// </summary>
        public float Score;
        
        /// <summary>
        /// Test description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Test result details.
        /// </summary>
        public string Details;
    }
    
    // Supporting enums
    
    /// <summary>
    /// Anomaly types for detection.
    /// </summary>
    public enum AnomalyType
    {
        PerformanceSpike,
        PerformanceDrop,
        MemoryLeak,
        UnusualPattern,
        DataInconsistency,
        SystemError
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
    /// Trend directions.
    /// </summary>
    public enum TrendDirection
    {
        Unknown,
        Improving,
        Stable,
        Declining,
        Fluctuating
    }
    
    /// <summary>
    /// Bottleneck types.
    /// </summary>
    public enum BottleneckType
    {
        CPU,
        GPU,
        Memory,
        IO,
        Network,
        Synchronization
    }
    
    // Additional supporting structures for specific analyses
    
    /// <summary>
    /// CPU bottleneck analysis.
    /// </summary>
    [Serializable]
    public struct CPUBottleneckAnalysis
    {
        public float AverageCPUUsage;
        public float PeakCPUUsage;
        public string[] HighCPUFunctions;
        public string[] OptimizationRecommendations;
    }
    
    /// <summary>
    /// GPU bottleneck analysis.
    /// </summary>
    [Serializable]
    public struct GPUBottleneckAnalysis
    {
        public float AverageGPUUsage;
        public float PeakGPUUsage;
        public int AverageDrawCalls;
        public int PeakDrawCalls;
        public string[] OptimizationRecommendations;
    }
    
    /// <summary>
    /// Memory bottleneck analysis.
    /// </summary>
    [Serializable]
    public struct MemoryBottleneckAnalysis
    {
        public float AverageMemoryUsage;
        public float PeakMemoryUsage;
        public int AllocationCount;
        public float GCPressure;
        public string[] OptimizationRecommendations;
    }
    
    /// <summary>
    /// Memory allocation pattern.
    /// </summary>
    [Serializable]
    public struct MemoryAllocationPattern
    {
        public float AllocationRate;
        public float DeallocationRate;
        public string[] AllocationHotspots;
        public string PatternDescription;
    }
    
    /// <summary>
    /// Garbage collection analysis.
    /// </summary>
    [Serializable]
    public struct GarbageCollectionAnalysis
    {
        public int GCEventCount;
        public float TotalGCTime;
        public float AverageGCTime;
        public float MaxGCTime;
        public string[] GCOptimizationRecommendations;
    }
    
    /// <summary>
    /// Memory leak detection.
    /// </summary>
    [Serializable]
    public struct MemoryLeakDetection
    {
        public bool PotentialLeaksDetected;
        public MemoryLeak[] DetectedLeaks;
        public float MemoryGrowthRate;
        public string[] LeakPreventionRecommendations;
    }
    
    /// <summary>
    /// Individual memory leak.
    /// </summary>
    [Serializable]
    public struct MemoryLeak
    {
        public string LeakSource;
        public float LeakRate;
        public string Description;
        public string RecommendedFix;
    }
    
    /// <summary>
    /// Frame time distribution.
    /// </summary>
    [Serializable]
    public struct FrameTimeDistribution
    {
        public float[] FrameTimes;
        public float Mean;
        public float Median;
        public float StandardDeviation;
        public float Percentile95;
        public float Percentile99;
    }
    
    /// <summary>
    /// Frame time spike information.
    /// </summary>
    [Serializable]
    public struct FrameTimeSpike
    {
        public DateTime Timestamp;
        public float SpikeFrameTime;
        public float BaselineFrameTime;
        public float SpikeMultiplier;
        public string PossibleCause;
    }
}