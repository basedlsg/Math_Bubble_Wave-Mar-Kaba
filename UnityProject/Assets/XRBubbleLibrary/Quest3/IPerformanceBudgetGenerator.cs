using System;
using UnityEngine;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Interface for automated performance budget generation and analysis.
    /// Generates perf_budget.md with frame-time analysis and pie chart visualization.
    /// Implements Requirement 6.1: Automated performance budget generation.
    /// Implements Requirement 6.2: Frame-time analysis and breakdown system.
    /// Implements Requirement 6.3: Performance data visualization.
    /// </summary>
    public interface IPerformanceBudgetGenerator
    {
        /// <summary>
        /// Whether the performance budget generator is initialized.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Current budget generation configuration.
        /// </summary>
        PerformanceBudgetConfiguration Configuration { get; }
        
        /// <summary>
        /// Initialize the performance budget generator.
        /// </summary>
        /// <param name="config">Budget generation configuration</param>
        /// <returns>True if initialization succeeded</returns>
        bool Initialize(PerformanceBudgetConfiguration config);
        
        /// <summary>
        /// Generate a complete performance budget analysis.
        /// </summary>
        /// <param name="performanceData">Performance data to analyze</param>
        /// <returns>Complete performance budget analysis</returns>
        PerformanceBudgetAnalysis GenerateBudgetAnalysis(PerformanceDataSet performanceData);
        
        /// <summary>
        /// Generate automated perf_budget.md documentation.
        /// </summary>
        /// <param name="analysis">Performance budget analysis</param>
        /// <param name="outputPath">Path to save the markdown file</param>
        /// <returns>True if generation succeeded</returns>
        bool GeneratePerformanceBudgetMarkdown(PerformanceBudgetAnalysis analysis, string outputPath);
        
        /// <summary>
        /// Generate frame-time breakdown pie chart data.
        /// </summary>
        /// <param name="frameTimeData">Frame timing data</param>
        /// <returns>Pie chart data for visualization</returns>
        FrameTimePieChartData GenerateFrameTimePieChart(FrameTimeBreakdown frameTimeData);
        
        /// <summary>
        /// Analyze frame-time breakdown and identify bottlenecks.
        /// </summary>
        /// <param name="performanceData">Performance data to analyze</param>
        /// <returns>Detailed frame-time analysis</returns>
        FrameTimeAnalysis AnalyzeFrameTimeBreakdown(PerformanceDataSet performanceData);
        
        /// <summary>
        /// Validate performance against Quest 3 budget requirements.
        /// </summary>
        /// <param name="analysis">Performance analysis to validate</param>
        /// <returns>Budget validation result</returns>
        PerformanceBudgetValidation ValidatePerformanceBudget(PerformanceBudgetAnalysis analysis);
        
        /// <summary>
        /// Generate performance optimization recommendations.
        /// </summary>
        /// <param name="analysis">Performance analysis</param>
        /// <returns>Optimization recommendations</returns>
        PerformanceOptimizationRecommendations GenerateOptimizationRecommendations(PerformanceBudgetAnalysis analysis);
        
        /// <summary>
        /// Export performance budget data in various formats.
        /// </summary>
        /// <param name="analysis">Performance analysis to export</param>
        /// <param name="format">Export format</param>
        /// <param name="outputPath">Output file path</param>
        /// <returns>True if export succeeded</returns>
        bool ExportPerformanceBudget(PerformanceBudgetAnalysis analysis, BudgetExportFormat format, string outputPath);
        
        /// <summary>
        /// Clean up performance budget generator resources.
        /// </summary>
        void Dispose();
    }
}    /
// <summary>
    /// Configuration for performance budget generation.
    /// </summary>
    [Serializable]
    public struct PerformanceBudgetConfiguration
    {
        /// <summary>
        /// Target frame rate for budget calculations (typically 72 FPS for Quest 3).
        /// </summary>
        public float TargetFrameRate;
        
        /// <summary>
        /// Target frame time in milliseconds.
        /// </summary>
        public float TargetFrameTimeMs;
        
        /// <summary>
        /// Required performance headroom percentage (0.0 to 1.0).
        /// </summary>
        public float RequiredHeadroomPercent;
        
        /// <summary>
        /// Whether to include detailed frame-time breakdown.
        /// </summary>
        public bool IncludeFrameTimeBreakdown;
        
        /// <summary>
        /// Whether to generate pie chart visualization data.
        /// </summary>
        public bool GeneratePieChartData;
        
        /// <summary>
        /// Whether to include optimization recommendations.
        /// </summary>
        public bool IncludeOptimizationRecommendations;
        
        /// <summary>
        /// Whether to validate against Quest 3 certification requirements.
        /// </summary>
        public bool ValidateQuest3Certification;
        
        /// <summary>
        /// Output directory for generated files.
        /// </summary>
        public string OutputDirectory;
        
        /// <summary>
        /// Default configuration for Quest 3 performance budgeting.
        /// </summary>
        public static PerformanceBudgetConfiguration Quest3Default => new PerformanceBudgetConfiguration
        {
            TargetFrameRate = 72f,
            TargetFrameTimeMs = 13.89f, // 1000ms / 72fps
            RequiredHeadroomPercent = 0.3f, // 30% headroom rule
            IncludeFrameTimeBreakdown = true,
            GeneratePieChartData = true,
            IncludeOptimizationRecommendations = true,
            ValidateQuest3Certification = true,
            OutputDirectory = "PerformanceBudgets/"
        };
    }
    
    /// <summary>
    /// Complete performance budget analysis results.
    /// </summary>
    public struct PerformanceBudgetAnalysis
    {
        /// <summary>
        /// Overall performance summary.
        /// </summary>
        public PerformanceSummary Summary;
        
        /// <summary>
        /// Detailed frame-time analysis.
        /// </summary>
        public FrameTimeAnalysis FrameTimeAnalysis;
        
        /// <summary>
        /// Performance budget breakdown.
        /// </summary>
        public PerformanceBudgetBreakdown BudgetBreakdown;
        
        /// <summary>
        /// Pie chart data for visualization.
        /// </summary>
        public FrameTimePieChartData PieChartData;
        
        /// <summary>
        /// Budget validation results.
        /// </summary>
        public PerformanceBudgetValidation Validation;
        
        /// <summary>
        /// Optimization recommendations.
        /// </summary>
        public PerformanceOptimizationRecommendations Recommendations;
        
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp;
        
        /// <summary>
        /// Configuration used for analysis.
        /// </summary>
        public PerformanceBudgetConfiguration Configuration;
    }
    
    /// <summary>
    /// Performance data set for analysis.
    /// </summary>
    public struct PerformanceDataSet
    {
        /// <summary>
        /// Frame rate samples.
        /// </summary>
        public float[] FrameRateSamples;
        
        /// <summary>
        /// Frame time samples in milliseconds.
        /// </summary>
        public float[] FrameTimeSamples;
        
        /// <summary>
        /// CPU usage samples.
        /// </summary>
        public float[] CPUUsageSamples;
        
        /// <summary>
        /// GPU usage samples.
        /// </summary>
        public float[] GPUUsageSamples;
        
        /// <summary>
        /// Memory usage samples in MB.
        /// </summary>
        public float[] MemoryUsageSamples;
        
        /// <summary>
        /// Detailed frame-time breakdown data.
        /// </summary>
        public FrameTimeBreakdown[] FrameTimeBreakdowns;
        
        /// <summary>
        /// Data collection duration.
        /// </summary>
        public TimeSpan CollectionDuration;
        
        /// <summary>
        /// Sample count.
        /// </summary>
        public int SampleCount;
    }
    
    /// <summary>
    /// Frame-time breakdown for a single frame.
    /// </summary>
    public struct FrameTimeBreakdown
    {
        /// <summary>
        /// Total frame time in milliseconds.
        /// </summary>
        public float TotalFrameTimeMs;
        
        /// <summary>
        /// CPU time in milliseconds.
        /// </summary>
        public float CPUTimeMs;
        
        /// <summary>
        /// GPU time in milliseconds.
        /// </summary>
        public float GPUTimeMs;
        
        /// <summary>
        /// Rendering time in milliseconds.
        /// </summary>
        public float RenderingTimeMs;
        
        /// <summary>
        /// Physics simulation time in milliseconds.
        /// </summary>
        public float PhysicsTimeMs;
        
        /// <summary>
        /// Script execution time in milliseconds.
        /// </summary>
        public float ScriptTimeMs;
        
        /// <summary>
        /// Audio processing time in milliseconds.
        /// </summary>
        public float AudioTimeMs;
        
        /// <summary>
        /// VR compositor time in milliseconds.
        /// </summary>
        public float VRCompositorTimeMs;
        
        /// <summary>
        /// Garbage collection time in milliseconds.
        /// </summary>
        public float GarbageCollectionTimeMs;
        
        /// <summary>
        /// Other/unaccounted time in milliseconds.
        /// </summary>
        public float OtherTimeMs;
        
        /// <summary>
        /// Frame timestamp.
        /// </summary>
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Pie chart data for frame-time visualization.
    /// </summary>
    public struct FrameTimePieChartData
    {
        /// <summary>
        /// Pie chart segments.
        /// </summary>
        public PieChartSegment[] Segments;
        
        /// <summary>
        /// Total frame time represented.
        /// </summary>
        public float TotalFrameTimeMs;
        
        /// <summary>
        /// Chart title.
        /// </summary>
        public string ChartTitle;
        
        /// <summary>
        /// Chart generation timestamp.
        /// </summary>
        public DateTime GenerationTimestamp;
    }
    
    /// <summary>
    /// Individual pie chart segment.
    /// </summary>
    public struct PieChartSegment
    {
        /// <summary>
        /// Segment label.
        /// </summary>
        public string Label;
        
        /// <summary>
        /// Time value in milliseconds.
        /// </summary>
        public float TimeMs;
        
        /// <summary>
        /// Percentage of total frame time.
        /// </summary>
        public float Percentage;
        
        /// <summary>
        /// Segment color (hex format).
        /// </summary>
        public string Color;
        
        /// <summary>
        /// Whether this segment represents a performance bottleneck.
        /// </summary>
        public bool IsBottleneck;
    }
    
    // Additional supporting enums and structures
    public enum BudgetExportFormat { Markdown, JSON, CSV, HTML }
    public enum PerformanceCategory { Excellent, Good, Acceptable, Poor, Critical }
    public enum OptimizationPriority { Low, Medium, High, Critical }
    
    // Additional structures would be defined here...
    public struct PerformanceSummary { /* Performance summary data */ }
    public struct FrameTimeAnalysis { /* Frame time analysis data */ }
    public struct PerformanceBudgetBreakdown { /* Budget breakdown data */ }
    public struct PerformanceBudgetValidation { /* Validation results */ }
    public struct PerformanceOptimizationRecommendations { /* Optimization recommendations */ }
}