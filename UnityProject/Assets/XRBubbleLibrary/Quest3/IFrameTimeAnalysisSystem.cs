using System;
using UnityEngine;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Interface for detailed frame-time breakdown and analysis system.
    /// Creates pie chart generation for resource allocation and bottleneck identification.
    /// Implements Requirement 6.2: Frame-time analysis and breakdown system.
    /// Implements Requirement 6.3: Performance data visualization.
    /// </summary>
    public interface IFrameTimeAnalysisSystem
    {
        /// <summary>
        /// Whether the frame-time analysis system is initialized.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Whether frame-time analysis is currently active.
        /// </summary>
        bool IsAnalysisActive { get; }
        
        /// <summary>
        /// Current analysis configuration.
        /// </summary>
        FrameTimeAnalysisConfiguration Configuration { get; }
        
        /// <summary>
        /// Initialize the frame-time analysis system.
        /// </summary>
        /// <param name="config">Analysis configuration</param>
        /// <returns>True if initialization succeeded</returns>
        bool Initialize(FrameTimeAnalysisConfiguration config);
        
        /// <summary>
        /// Start frame-time analysis session.
        /// </summary>
        /// <param name="sessionName">Name for the analysis session</param>
        /// <returns>Analysis session handle</returns>
        FrameTimeAnalysisSession StartAnalysis(string sessionName = "FrameTime_Analysis");
        
        /// <summary>
        /// Stop frame-time analysis and get results.
        /// </summary>
        /// <returns>Complete frame-time analysis results</returns>
        FrameTimeAnalysisResult StopAnalysis();
        
        /// <summary>
        /// Analyze a single frame's timing data.
        /// </summary>
        /// <param name="frameData">Frame timing data to analyze</param>
        /// <returns>Single frame analysis result</returns>
        SingleFrameAnalysis AnalyzeSingleFrame(FrameTimingData frameData);
        
        /// <summary>
        /// Analyze multiple frames and generate comprehensive breakdown.
        /// </summary>
        /// <param name="frameDataArray">Array of frame timing data</param>
        /// <returns>Multi-frame analysis result</returns>
        MultiFrameAnalysis AnalyzeMultipleFrames(FrameTimingData[] frameDataArray);
        
        /// <summary>
        /// Generate pie chart data for resource allocation visualization.
        /// </summary>
        /// <param name="frameBreakdown">Frame breakdown data</param>
        /// <returns>Pie chart data for visualization</returns>
        ResourceAllocationPieChart GenerateResourceAllocationPieChart(DetailedFrameBreakdown frameBreakdown);
        
        /// <summary>
        /// Identify performance bottlenecks from frame-time data.
        /// </summary>
        /// <param name="analysisResult">Frame-time analysis result</param>
        /// <returns>Identified bottlenecks with recommendations</returns>
        PerformanceBottleneckAnalysis IdentifyBottlenecks(FrameTimeAnalysisResult analysisResult);
        
        /// <summary>
        /// Generate frame-time trend analysis over time.
        /// </summary>
        /// <param name="frameDataArray">Time-series frame data</param>
        /// <returns>Trend analysis results</returns>
        FrameTimeTrendAnalysis AnalyzeFrameTimeTrends(FrameTimingData[] frameDataArray);
        
        /// <summary>
        /// Compare frame-time performance between different scenarios.
        /// </summary>
        /// <param name="baselineData">Baseline frame data</param>
        /// <param name="comparisonData">Comparison frame data</param>
        /// <returns>Comparative analysis results</returns>
        FrameTimeComparison CompareFrameTimePerformance(FrameTimingData[] baselineData, FrameTimingData[] comparisonData);
        
        /// <summary>
        /// Generate detailed frame-time report with visualizations.
        /// </summary>
        /// <param name="analysisResult">Analysis result to report on</param>
        /// <param name="outputPath">Path to save the report</param>
        /// <returns>True if report generation succeeded</returns>
        bool GenerateFrameTimeReport(FrameTimeAnalysisResult analysisResult, string outputPath);
        
        /// <summary>
        /// Export frame-time analysis data in various formats.
        /// </summary>
        /// <param name="analysisResult">Analysis result to export</param>
        /// <param name="format">Export format</param>
        /// <param name="outputPath">Output file path</param>
        /// <returns>True if export succeeded</returns>
        bool ExportAnalysisData(FrameTimeAnalysisResult analysisResult, AnalysisExportFormat format, string outputPath);
        
        /// <summary>
        /// Get real-time frame-time metrics during analysis.
        /// </summary>
        /// <returns>Current frame-time metrics</returns>
        RealTimeFrameMetrics GetRealTimeMetrics();
        
        /// <summary>
        /// Configure bottleneck detection thresholds.
        /// </summary>
        /// <param name="thresholds">Bottleneck detection thresholds</param>
        void ConfigureBottleneckThresholds(BottleneckDetectionThresholds thresholds);
        
        /// <summary>
        /// Event fired when a performance bottleneck is detected.
        /// </summary>
        event Action<BottleneckDetectedEventArgs> BottleneckDetected;
        
        /// <summary>
        /// Event fired when frame-time analysis data is updated.
        /// </summary>
        event Action<FrameTimeAnalysisUpdatedEventArgs> AnalysisDataUpdated;
        
        /// <summary>
        /// Clean up frame-time analysis system resources.
        /// </summary>
        void Dispose();
    }
} 
   
    /// <summary>
    /// Configuration for frame-time analysis system.
    /// </summary>
    [Serializable]
    public struct FrameTimeAnalysisConfiguration
    {
        /// <summary>
        /// Analysis sampling rate in Hz.
        /// </summary>
        public float SamplingRateHz;
        
        /// <summary>
        /// Whether to enable detailed component breakdown.
        /// </summary>
        public bool EnableDetailedBreakdown;
        
        /// <summary>
        /// Whether to enable real-time bottleneck detection.
        /// </summary>
        public bool EnableBottleneckDetection;
        
        /// <summary>
        /// Whether to generate pie chart visualization data.
        /// </summary>
        public bool GeneratePieChartData;
        
        /// <summary>
        /// Whether to enable trend analysis.
        /// </summary>
        public bool EnableTrendAnalysis;
        
        /// <summary>
        /// Maximum number of frames to analyze in memory.
        /// </summary>
        public int MaxFramesInMemory;
        
        /// <summary>
        /// Minimum frame count required for reliable analysis.
        /// </summary>
        public int MinFramesForAnalysis;
        
        /// <summary>
        /// Target frame rate for analysis (typically 72 FPS for Quest 3).
        /// </summary>
        public float TargetFrameRate;
        
        /// <summary>
        /// Output directory for analysis reports.
        /// </summary>
        public string OutputDirectory;
        
        /// <summary>
        /// Whether to enable debug logging.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Default configuration for Quest 3 frame-time analysis.
        /// </summary>
        public static FrameTimeAnalysisConfiguration Quest3Default => new FrameTimeAnalysisConfiguration
        {
            SamplingRateHz = 72f, // Match Quest 3 refresh rate
            EnableDetailedBreakdown = true,
            EnableBottleneckDetection = true,
            GeneratePieChartData = true,
            EnableTrendAnalysis = true,
            MaxFramesInMemory = 4320, // 60 seconds at 72 FPS
            MinFramesForAnalysis = 72, // 1 second minimum
            TargetFrameRate = 72f,
            OutputDirectory = "FrameTimeAnalysis/",
            EnableDebugLogging = false
        };
        
        /// <summary>
        /// High-frequency analysis configuration for detailed profiling.
        /// </summary>
        public static FrameTimeAnalysisConfiguration HighFrequency => new FrameTimeAnalysisConfiguration
        {
            SamplingRateHz = 120f,
            EnableDetailedBreakdown = true,
            EnableBottleneckDetection = true,
            GeneratePieChartData = true,
            EnableTrendAnalysis = true,
            MaxFramesInMemory = 7200, // 60 seconds at 120 FPS
            MinFramesForAnalysis = 120, // 1 second minimum
            TargetFrameRate = 72f,
            OutputDirectory = "FrameTimeAnalysis/HighFreq/",
            EnableDebugLogging = true
        };
    }
    
    /// <summary>
    /// Frame-time analysis session information.
    /// </summary>
    public struct FrameTimeAnalysisSession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// User-provided session name.
        /// </summary>
        public string SessionName;
        
        /// <summary>
        /// Session start timestamp.
        /// </summary>
        public DateTime StartTime;
        
        /// <summary>
        /// Configuration used for this session.
        /// </summary>
        public FrameTimeAnalysisConfiguration Configuration;
        
        /// <summary>
        /// Whether the session is currently active.
        /// </summary>
        public bool IsActive;
        
        /// <summary>
        /// Number of frames analyzed so far.
        /// </summary>
        public int FramesAnalyzed;
        
        /// <summary>
        /// Session duration so far.
        /// </summary>
        public TimeSpan Duration;
    }
    
    /// <summary>
    /// Individual frame timing data.
    /// </summary>
    public struct FrameTimingData
    {
        /// <summary>
        /// Frame number/index.
        /// </summary>
        public int FrameNumber;
        
        /// <summary>
        /// Total frame time in milliseconds.
        /// </summary>
        public float TotalFrameTimeMs;
        
        /// <summary>
        /// Frame timestamp.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Detailed breakdown of frame components.
        /// </summary>
        public DetailedFrameBreakdown Breakdown;
        
        /// <summary>
        /// Frame rate at this frame.
        /// </summary>
        public float FrameRate;
        
        /// <summary>
        /// Whether this frame experienced performance issues.
        /// </summary>
        public bool HasPerformanceIssues;
        
        /// <summary>
        /// Performance issues detected in this frame.
        /// </summary>
        public string[] PerformanceIssues;
    }
    
    /// <summary>
    /// Detailed breakdown of frame timing components.
    /// </summary>
    public struct DetailedFrameBreakdown
    {
        /// <summary>
        /// CPU processing time in milliseconds.
        /// </summary>
        public float CPUTimeMs;
        
        /// <summary>
        /// GPU processing time in milliseconds.
        /// </summary>
        public float GPUTimeMs;
        
        /// <summary>
        /// Rendering pipeline time in milliseconds.
        /// </summary>
        public float RenderingTimeMs;
        
        /// <summary>
        /// Physics simulation time in milliseconds.
        /// </summary>
        public float PhysicsTimeMs;
        
        /// <summary>
        /// Script execution time in milliseconds.
        /// </summary>
        public float ScriptExecutionTimeMs;
        
        /// <summary>
        /// Animation system time in milliseconds.
        /// </summary>
        public float AnimationTimeMs;
        
        /// <summary>
        /// Audio processing time in milliseconds.
        /// </summary>
        public float AudioTimeMs;
        
        /// <summary>
        /// VR compositor time in milliseconds.
        /// </summary>
        public float VRCompositorTimeMs;
        
        /// <summary>
        /// Input processing time in milliseconds.
        /// </summary>
        public float InputTimeMs;
        
        /// <summary>
        /// Garbage collection time in milliseconds.
        /// </summary>
        public float GarbageCollectionTimeMs;
        
        /// <summary>
        /// UI rendering time in milliseconds.
        /// </summary>
        public float UIRenderingTimeMs;
        
        /// <summary>
        /// Particle system time in milliseconds.
        /// </summary>
        public float ParticleSystemTimeMs;
        
        /// <summary>
        /// Post-processing time in milliseconds.
        /// </summary>
        public float PostProcessingTimeMs;
        
        /// <summary>
        /// Other/unaccounted time in milliseconds.
        /// </summary>
        public float OtherTimeMs;
        
        /// <summary>
        /// Wait time (idle) in milliseconds.
        /// </summary>
        public float WaitTimeMs;
    }
    
    /// <summary>
    /// Pie chart data for resource allocation visualization.
    /// </summary>
    public struct ResourceAllocationPieChart
    {
        /// <summary>
        /// Pie chart segments representing different components.
        /// </summary>
        public ResourceAllocationSegment[] Segments;
        
        /// <summary>
        /// Total frame time represented by the chart.
        /// </summary>
        public float TotalFrameTimeMs;
        
        /// <summary>
        /// Chart title and metadata.
        /// </summary>
        public string ChartTitle;
        
        /// <summary>
        /// Chart generation timestamp.
        /// </summary>
        public DateTime GenerationTimestamp;
        
        /// <summary>
        /// Performance category of the frame.
        /// </summary>
        public FramePerformanceCategory PerformanceCategory;
    }
    
    /// <summary>
    /// Individual segment in resource allocation pie chart.
    /// </summary>
    public struct ResourceAllocationSegment
    {
        /// <summary>
        /// Component name/label.
        /// </summary>
        public string ComponentName;
        
        /// <summary>
        /// Time consumed by this component in milliseconds.
        /// </summary>
        public float TimeMs;
        
        /// <summary>
        /// Percentage of total frame time.
        /// </summary>
        public float Percentage;
        
        /// <summary>
        /// Visual color for this segment (hex format).
        /// </summary>
        public string Color;
        
        /// <summary>
        /// Whether this component is a performance bottleneck.
        /// </summary>
        public bool IsBottleneck;
        
        /// <summary>
        /// Optimization priority for this component.
        /// </summary>
        public ComponentOptimizationPriority OptimizationPriority;
        
        /// <summary>
        /// Brief description of what this component does.
        /// </summary>
        public string Description;
    }
    
    /// <summary>
    /// Complete frame-time analysis result.
    /// </summary>
    public struct FrameTimeAnalysisResult
    {
        /// <summary>
        /// Analysis session information.
        /// </summary>
        public FrameTimeAnalysisSession Session;
        
        /// <summary>
        /// Single frame analysis results.
        /// </summary>
        public SingleFrameAnalysis[] SingleFrameResults;
        
        /// <summary>
        /// Multi-frame aggregate analysis.
        /// </summary>
        public MultiFrameAnalysis AggregateAnalysis;
        
        /// <summary>
        /// Resource allocation pie chart data.
        /// </summary>
        public ResourceAllocationPieChart ResourceAllocation;
        
        /// <summary>
        /// Identified performance bottlenecks.
        /// </summary>
        public PerformanceBottleneckAnalysis BottleneckAnalysis;
        
        /// <summary>
        /// Frame-time trend analysis.
        /// </summary>
        public FrameTimeTrendAnalysis TrendAnalysis;
        
        /// <summary>
        /// Analysis quality and reliability metrics.
        /// </summary>
        public AnalysisQualityMetrics QualityMetrics;
        
        /// <summary>
        /// Analysis completion timestamp.
        /// </summary>
        public DateTime CompletionTimestamp;
    }
    
    // Supporting enums and additional structures
    public enum AnalysisExportFormat { JSON, CSV, HTML, Binary }
    public enum FramePerformanceCategory { Excellent, Good, Acceptable, Poor, Critical }
    public enum ComponentOptimizationPriority { Low, Medium, High, Critical }
    public enum BottleneckSeverity { Minor, Moderate, Severe, Critical }
    public enum TrendDirection { Stable, Improving, Degrading, Volatile }
    
    // Additional structures would be defined here...
    public struct SingleFrameAnalysis { /* Single frame analysis data */ }
    public struct MultiFrameAnalysis { /* Multi-frame analysis data */ }
    public struct PerformanceBottleneckAnalysis { /* Bottleneck analysis data */ }
    public struct FrameTimeTrendAnalysis { /* Trend analysis data */ }
    public struct FrameTimeComparison { /* Comparison analysis data */ }
    public struct RealTimeFrameMetrics { /* Real-time metrics data */ }
    public struct BottleneckDetectionThresholds { /* Bottleneck thresholds */ }
    public struct AnalysisQualityMetrics { /* Analysis quality data */ }
    
    // Event argument structures
    public struct BottleneckDetectedEventArgs
    {
        public string ComponentName;
        public float TimeMs;
        public float Percentage;
        public BottleneckSeverity Severity;
        public DateTime DetectionTimestamp;
    }
    
    public struct FrameTimeAnalysisUpdatedEventArgs
    {
        public int FramesAnalyzed;
        public float AverageFrameTimeMs;
        public float CurrentFrameRate;
        public DateTime UpdateTimestamp;
    }
}