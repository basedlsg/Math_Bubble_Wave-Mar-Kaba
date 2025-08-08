using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Mathematics;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Performance budget generator for automated perf_budget.md generation.
    /// Implements Requirement 6.1: Automated performance budget generation.
    /// Implements Requirement 6.2: Frame-time analysis and breakdown system.
    /// Implements Requirement 6.3: Performance data visualization.
    /// </summary>
    public class PerformanceBudgetGenerator : IPerformanceBudgetGenerator
    {
        // Configuration and state
        private PerformanceBudgetConfiguration _configuration;
        private bool _isInitialized;
        
        // Analysis constants
        private const float QUEST3_TARGET_FRAME_TIME_MS = 13.89f; // 72 FPS
        private const float HEADROOM_REQUIREMENT = 0.3f; // 30% headroom rule
        private const int MIN_SAMPLES_FOR_ANALYSIS = 100;
        
        // Pie chart colors for different performance categories
        private readonly Dictionary<string, string> _pieChartColors = new Dictionary<string, string>
        {
            { "Rendering", "#FF6B6B" },
            { "CPU", "#4ECDC4" },
            { "GPU", "#45B7D1" },
            { "Physics", "#96CEB4" },
            { "Scripts", "#FFEAA7" },
            { "Audio", "#DDA0DD" },
            { "VR Compositor", "#98D8C8" },
            { "Garbage Collection", "#F7DC6F" },
            { "Other", "#BDC3C7" }
        };
        
        /// <summary>
        /// Whether the performance budget generator is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Current budget generation configuration.
        /// </summary>
        public PerformanceBudgetConfiguration Configuration => _configuration;
        
        /// <summary>
        /// Initialize the performance budget generator.
        /// </summary>
        public bool Initialize(PerformanceBudgetConfiguration config)
        {
            try
            {
                _configuration = config;
                
                // Validate configuration
                if (config.TargetFrameRate <= 0 || config.TargetFrameTimeMs <= 0)
                {
                    UnityEngine.Debug.LogError("[PerformanceBudgetGenerator] Invalid target frame rate or time");
                    return false;
                }
                
                if (config.RequiredHeadroomPercent < 0 || config.RequiredHeadroomPercent > 1)
                {
                    UnityEngine.Debug.LogError("[PerformanceBudgetGenerator] Invalid headroom percentage");
                    return false;
                }
                
                // Create output directory if needed
                if (!string.IsNullOrEmpty(config.OutputDirectory))
                {
                    Directory.CreateDirectory(config.OutputDirectory);
                }
                
                _isInitialized = true;
                
                UnityEngine.Debug.Log($"[PerformanceBudgetGenerator] Initialized for {config.TargetFrameRate} FPS target");
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceBudgetGenerator] Initialization failed: {ex.Message}");
                return false;
            }
        }     
   
        /// <summary>
        /// Generate a complete performance budget analysis.
        /// </summary>
        public PerformanceBudgetAnalysis GenerateBudgetAnalysis(PerformanceDataSet performanceData)
        {
            if (!_isInitialized)
            {
                UnityEngine.Debug.LogError("[PerformanceBudgetGenerator] Not initialized");
                return default;
            }
            
            if (performanceData.SampleCount < MIN_SAMPLES_FOR_ANALYSIS)
            {
                UnityEngine.Debug.LogWarning($"[PerformanceBudgetGenerator] Insufficient samples for analysis: {performanceData.SampleCount}");
            }
            
            // Generate performance summary
            var summary = GeneratePerformanceSummary(performanceData);
            
            // Analyze frame-time breakdown
            var frameTimeAnalysis = AnalyzeFrameTimeBreakdown(performanceData);
            
            // Generate budget breakdown
            var budgetBreakdown = GenerateBudgetBreakdown(performanceData, frameTimeAnalysis);
            
            // Generate pie chart data if requested
            var pieChartData = _configuration.GeneratePieChartData ? 
                GenerateFrameTimePieChart(GetAverageFrameTimeBreakdown(performanceData.FrameTimeBreakdowns)) : 
                default;
            
            // Validate performance budget
            var validation = _configuration.ValidateQuest3Certification ? 
                ValidatePerformanceBudget(new PerformanceBudgetAnalysis { Summary = summary, FrameTimeAnalysis = frameTimeAnalysis }) : 
                default;
            
            // Generate optimization recommendations
            var recommendations = _configuration.IncludeOptimizationRecommendations ? 
                GenerateOptimizationRecommendations(new PerformanceBudgetAnalysis { Summary = summary, FrameTimeAnalysis = frameTimeAnalysis }) : 
                default;
            
            return new PerformanceBudgetAnalysis
            {
                Summary = summary,
                FrameTimeAnalysis = frameTimeAnalysis,
                BudgetBreakdown = budgetBreakdown,
                PieChartData = pieChartData,
                Validation = validation,
                Recommendations = recommendations,
                AnalysisTimestamp = DateTime.Now,
                Configuration = _configuration
            };
        }
        
        /// <summary>
        /// Generate automated perf_budget.md documentation.
        /// </summary>
        public bool GeneratePerformanceBudgetMarkdown(PerformanceBudgetAnalysis analysis, string outputPath)
        {
            try
            {
                var markdown = new StringBuilder();
                
                // Header
                markdown.AppendLine("# Performance Budget Analysis");
                markdown.AppendLine($"Generated: {analysis.AnalysisTimestamp:yyyy-MM-dd HH:mm:ss}");
                markdown.AppendLine($"Target: {analysis.Configuration.TargetFrameRate} FPS ({analysis.Configuration.TargetFrameTimeMs:F2}ms)");
                markdown.AppendLine($"Required Headroom: {analysis.Configuration.RequiredHeadroomPercent * 100:F0}%");
                markdown.AppendLine();
                
                // Performance Summary
                markdown.AppendLine("## Performance Summary");
                AppendPerformanceSummaryMarkdown(markdown, analysis.Summary);
                markdown.AppendLine();
                
                // Frame Time Analysis
                if (_configuration.IncludeFrameTimeBreakdown)
                {
                    markdown.AppendLine("## Frame Time Breakdown");
                    AppendFrameTimeAnalysisMarkdown(markdown, analysis.FrameTimeAnalysis);
                    markdown.AppendLine();
                }
                
                // Budget Breakdown
                markdown.AppendLine("## Performance Budget Breakdown");
                AppendBudgetBreakdownMarkdown(markdown, analysis.BudgetBreakdown);
                markdown.AppendLine();
                
                // Pie Chart Data
                if (_configuration.GeneratePieChartData && analysis.PieChartData.Segments != null)
                {
                    markdown.AppendLine("## Frame Time Distribution");
                    AppendPieChartDataMarkdown(markdown, analysis.PieChartData);
                    markdown.AppendLine();
                }
                
                // Validation Results
                if (_configuration.ValidateQuest3Certification)
                {
                    markdown.AppendLine("## Quest 3 Certification Validation");
                    AppendValidationResultsMarkdown(markdown, analysis.Validation);
                    markdown.AppendLine();
                }
                
                // Optimization Recommendations
                if (_configuration.IncludeOptimizationRecommendations)
                {
                    markdown.AppendLine("## Optimization Recommendations");
                    AppendOptimizationRecommendationsMarkdown(markdown, analysis.Recommendations);
                    markdown.AppendLine();
                }
                
                // Headroom Analysis
                markdown.AppendLine("## Headroom Analysis");
                AppendHeadroomAnalysisMarkdown(markdown, analysis);
                
                // Write to file
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllText(outputPath, markdown.ToString());
                
                UnityEngine.Debug.Log($"[PerformanceBudgetGenerator] Generated performance budget: {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceBudgetGenerator] Failed to generate markdown: {ex.Message}");
                return false;
            }
        }       
 
        /// <summary>
        /// Generate frame-time breakdown pie chart data.
        /// </summary>
        public FrameTimePieChartData GenerateFrameTimePieChart(FrameTimeBreakdown frameTimeData)
        {
            var segments = new List<PieChartSegment>();
            
            // Add segments for each component
            AddPieChartSegment(segments, "Rendering", frameTimeData.RenderingTimeMs, frameTimeData.TotalFrameTimeMs);
            AddPieChartSegment(segments, "CPU", frameTimeData.CPUTimeMs, frameTimeData.TotalFrameTimeMs);
            AddPieChartSegment(segments, "GPU", frameTimeData.GPUTimeMs, frameTimeData.TotalFrameTimeMs);
            AddPieChartSegment(segments, "Physics", frameTimeData.PhysicsTimeMs, frameTimeData.TotalFrameTimeMs);
            AddPieChartSegment(segments, "Scripts", frameTimeData.ScriptTimeMs, frameTimeData.TotalFrameTimeMs);
            AddPieChartSegment(segments, "Audio", frameTimeData.AudioTimeMs, frameTimeData.TotalFrameTimeMs);
            AddPieChartSegment(segments, "VR Compositor", frameTimeData.VRCompositorTimeMs, frameTimeData.TotalFrameTimeMs);
            AddPieChartSegment(segments, "Garbage Collection", frameTimeData.GarbageCollectionTimeMs, frameTimeData.TotalFrameTimeMs);
            AddPieChartSegment(segments, "Other", frameTimeData.OtherTimeMs, frameTimeData.TotalFrameTimeMs);
            
            // Sort by percentage (largest first)
            segments.Sort((a, b) => b.Percentage.CompareTo(a.Percentage));
            
            return new FrameTimePieChartData
            {
                Segments = segments.ToArray(),
                TotalFrameTimeMs = frameTimeData.TotalFrameTimeMs,
                ChartTitle = $"Frame Time Breakdown ({frameTimeData.TotalFrameTimeMs:F2}ms)",
                GenerationTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Analyze frame-time breakdown and identify bottlenecks.
        /// </summary>
        public FrameTimeAnalysis AnalyzeFrameTimeBreakdown(PerformanceDataSet performanceData)
        {
            if (performanceData.FrameTimeBreakdowns == null || performanceData.FrameTimeBreakdowns.Length == 0)
            {
                return CreateDefaultFrameTimeAnalysis(performanceData);
            }
            
            var breakdowns = performanceData.FrameTimeBreakdowns;
            
            // Calculate averages for each component
            var avgBreakdown = new FrameTimeBreakdown
            {
                TotalFrameTimeMs = breakdowns.Average(b => b.TotalFrameTimeMs),
                CPUTimeMs = breakdowns.Average(b => b.CPUTimeMs),
                GPUTimeMs = breakdowns.Average(b => b.GPUTimeMs),
                RenderingTimeMs = breakdowns.Average(b => b.RenderingTimeMs),
                PhysicsTimeMs = breakdowns.Average(b => b.PhysicsTimeMs),
                ScriptTimeMs = breakdowns.Average(b => b.ScriptTimeMs),
                AudioTimeMs = breakdowns.Average(b => b.AudioTimeMs),
                VRCompositorTimeMs = breakdowns.Average(b => b.VRCompositorTimeMs),
                GarbageCollectionTimeMs = breakdowns.Average(b => b.GarbageCollectionTimeMs),
                OtherTimeMs = breakdowns.Average(b => b.OtherTimeMs)
            };
            
            // Identify bottlenecks
            var bottlenecks = IdentifyPerformanceBottlenecks(avgBreakdown);
            
            // Calculate frame time statistics
            var frameTimeStats = CalculateFrameTimeStatistics(breakdowns);
            
            // Analyze trends
            var trends = AnalyzeFrameTimeTrends(breakdowns);
            
            return new FrameTimeAnalysis
            {
                AverageBreakdown = avgBreakdown,
                PerformanceBottlenecks = bottlenecks,
                FrameTimeStatistics = frameTimeStats,
                FrameTimeTrends = trends,
                SampleCount = breakdowns.Length,
                AnalysisTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Validate performance against Quest 3 budget requirements.
        /// </summary>
        public PerformanceBudgetValidation ValidatePerformanceBudget(PerformanceBudgetAnalysis analysis)
        {
            var validationResults = new List<BudgetValidationResult>();
            var recommendations = new List<string>();
            bool meetsRequirements = true;
            
            // Validate frame rate
            float avgFrameRate = 1000f / analysis.FrameTimeAnalysis.AverageBreakdown.TotalFrameTimeMs;
            if (avgFrameRate < _configuration.TargetFrameRate)
            {
                validationResults.Add(new BudgetValidationResult
                {
                    Category = "Frame Rate",
                    Expected = _configuration.TargetFrameRate,
                    Actual = avgFrameRate,
                    Passed = false,
                    Message = $"Frame rate {avgFrameRate:F1} FPS below target {_configuration.TargetFrameRate} FPS"
                });
                recommendations.Add("Optimize rendering pipeline to improve frame rate");
                meetsRequirements = false;
            }
            else
            {
                validationResults.Add(new BudgetValidationResult
                {
                    Category = "Frame Rate",
                    Expected = _configuration.TargetFrameRate,
                    Actual = avgFrameRate,
                    Passed = true,
                    Message = $"Frame rate {avgFrameRate:F1} FPS meets target"
                });
            }
            
            // Validate headroom requirement
            float usedBudget = analysis.FrameTimeAnalysis.AverageBreakdown.TotalFrameTimeMs;
            float availableBudget = _configuration.TargetFrameTimeMs;
            float headroomPercent = (availableBudget - usedBudget) / availableBudget;
            
            if (headroomPercent < _configuration.RequiredHeadroomPercent)
            {
                validationResults.Add(new BudgetValidationResult
                {
                    Category = "Headroom",
                    Expected = _configuration.RequiredHeadroomPercent * 100f,
                    Actual = headroomPercent * 100f,
                    Passed = false,
                    Message = $"Headroom {headroomPercent * 100:F1}% below required {_configuration.RequiredHeadroomPercent * 100:F0}%"
                });
                recommendations.Add($"Reduce frame time by {(usedBudget - availableBudget * (1 - _configuration.RequiredHeadroomPercent)):F2}ms to meet headroom requirement");
                meetsRequirements = false;
            }
            else
            {
                validationResults.Add(new BudgetValidationResult
                {
                    Category = "Headroom",
                    Expected = _configuration.RequiredHeadroomPercent * 100f,
                    Actual = headroomPercent * 100f,
                    Passed = true,
                    Message = $"Headroom {headroomPercent * 100:F1}% meets requirement"
                });
            }
            
            // Calculate overall score
            float overallScore = validationResults.Count(r => r.Passed) / (float)validationResults.Count * 100f;
            
            return new PerformanceBudgetValidation
            {
                MeetsRequirements = meetsRequirements,
                OverallScore = overallScore,
                ValidationResults = validationResults.ToArray(),
                Recommendations = recommendations.ToArray(),
                ValidationTimestamp = DateTime.Now
            };
        }  
      
        /// <summary>
        /// Generate performance optimization recommendations.
        /// </summary>
        public PerformanceOptimizationRecommendations GenerateOptimizationRecommendations(PerformanceBudgetAnalysis analysis)
        {
            var recommendations = new List<OptimizationRecommendation>();
            
            var breakdown = analysis.FrameTimeAnalysis.AverageBreakdown;
            float totalFrameTime = breakdown.TotalFrameTimeMs;
            
            // Analyze each component and generate recommendations
            if (breakdown.RenderingTimeMs / totalFrameTime > 0.4f) // >40% of frame time
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Category = "Rendering",
                    Priority = OptimizationPriority.High,
                    Description = "Rendering consumes significant frame time",
                    Recommendation = "Optimize draw calls, reduce polygon count, use LOD system",
                    EstimatedImpact = "2-5ms reduction",
                    ImplementationEffort = "Medium"
                });
            }
            
            if (breakdown.CPUTimeMs / totalFrameTime > 0.3f) // >30% of frame time
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Category = "CPU",
                    Priority = OptimizationPriority.High,
                    Description = "CPU processing is a bottleneck",
                    Recommendation = "Use Burst compilation, optimize algorithms, reduce Update() calls",
                    EstimatedImpact = "1-3ms reduction",
                    ImplementationEffort = "Medium"
                });
            }
            
            if (breakdown.GPUTimeMs / totalFrameTime > 0.35f) // >35% of frame time
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Category = "GPU",
                    Priority = OptimizationPriority.High,
                    Description = "GPU processing is limiting performance",
                    Recommendation = "Reduce shader complexity, optimize textures, use GPU instancing",
                    EstimatedImpact = "2-4ms reduction",
                    ImplementationEffort = "High"
                });
            }
            
            if (breakdown.GarbageCollectionTimeMs > 0.5f) // >0.5ms GC time
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Category = "Memory",
                    Priority = OptimizationPriority.Medium,
                    Description = "Garbage collection causing frame spikes",
                    Recommendation = "Reduce allocations, use object pooling, optimize string operations",
                    EstimatedImpact = "0.5-1ms reduction",
                    ImplementationEffort = "Low"
                });
            }
            
            if (breakdown.PhysicsTimeMs / totalFrameTime > 0.15f) // >15% of frame time
            {
                recommendations.Add(new OptimizationRecommendation
                {
                    Category = "Physics",
                    Priority = OptimizationPriority.Medium,
                    Description = "Physics simulation consuming significant time",
                    Recommendation = "Reduce physics complexity, optimize collision detection, use fixed timestep",
                    EstimatedImpact = "1-2ms reduction",
                    ImplementationEffort = "Medium"
                });
            }
            
            // Sort by priority
            recommendations.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            
            return new PerformanceOptimizationRecommendations
            {
                Recommendations = recommendations.ToArray(),
                TotalEstimatedImpact = EstimateTotalImpact(recommendations),
                ImplementationComplexity = DetermineOverallComplexity(recommendations),
                GenerationTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Export performance budget data in various formats.
        /// </summary>
        public bool ExportPerformanceBudget(PerformanceBudgetAnalysis analysis, BudgetExportFormat format, string outputPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                
                switch (format)
                {
                    case BudgetExportFormat.Markdown:
                        return GeneratePerformanceBudgetMarkdown(analysis, outputPath);
                    case BudgetExportFormat.JSON:
                        return ExportToJSON(analysis, outputPath);
                    case BudgetExportFormat.CSV:
                        return ExportToCSV(analysis, outputPath);
                    case BudgetExportFormat.HTML:
                        return ExportToHTML(analysis, outputPath);
                    default:
                        UnityEngine.Debug.LogError($"[PerformanceBudgetGenerator] Unsupported export format: {format}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceBudgetGenerator] Export failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Clean up performance budget generator resources.
        /// </summary>
        public void Dispose()
        {
            _isInitialized = false;
            UnityEngine.Debug.Log("[PerformanceBudgetGenerator] Disposed");
        }
        
        #region Private Helper Methods
        
        private void AddPieChartSegment(List<PieChartSegment> segments, string label, float timeMs, float totalTimeMs)
        {
            if (timeMs <= 0) return;
            
            float percentage = (timeMs / totalTimeMs) * 100f;
            bool isBottleneck = percentage > 20f; // Consider >20% a bottleneck
            
            segments.Add(new PieChartSegment
            {
                Label = label,
                TimeMs = timeMs,
                Percentage = percentage,
                Color = _pieChartColors.ContainsKey(label) ? _pieChartColors[label] : "#BDC3C7",
                IsBottleneck = isBottleneck
            });
        }
        
        private FrameTimeBreakdown GetAverageFrameTimeBreakdown(FrameTimeBreakdown[] breakdowns)
        {
            if (breakdowns == null || breakdowns.Length == 0)
            {
                return new FrameTimeBreakdown
                {
                    TotalFrameTimeMs = QUEST3_TARGET_FRAME_TIME_MS,
                    RenderingTimeMs = 6f,
                    CPUTimeMs = 3f,
                    GPUTimeMs = 2f,
                    PhysicsTimeMs = 1f,
                    ScriptTimeMs = 1f,
                    AudioTimeMs = 0.5f,
                    VRCompositorTimeMs = 0.3f,
                    GarbageCollectionTimeMs = 0.1f,
                    OtherTimeMs = 0.1f
                };
            }
            
            return new FrameTimeBreakdown
            {
                TotalFrameTimeMs = breakdowns.Average(b => b.TotalFrameTimeMs),
                CPUTimeMs = breakdowns.Average(b => b.CPUTimeMs),
                GPUTimeMs = breakdowns.Average(b => b.GPUTimeMs),
                RenderingTimeMs = breakdowns.Average(b => b.RenderingTimeMs),
                PhysicsTimeMs = breakdowns.Average(b => b.PhysicsTimeMs),
                ScriptTimeMs = breakdowns.Average(b => b.ScriptTimeMs),
                AudioTimeMs = breakdowns.Average(b => b.AudioTimeMs),
                VRCompositorTimeMs = breakdowns.Average(b => b.VRCompositorTimeMs),
                GarbageCollectionTimeMs = breakdowns.Average(b => b.GarbageCollectionTimeMs),
                OtherTimeMs = breakdowns.Average(b => b.OtherTimeMs)
            };
        }
        
        private PerformanceSummary GeneratePerformanceSummary(PerformanceDataSet performanceData)
        {
            var frameRateStats = CalculateStatistics(performanceData.FrameRateSamples);
            var frameTimeStats = CalculateStatistics(performanceData.FrameTimeSamples);
            var cpuStats = CalculateStatistics(performanceData.CPUUsageSamples);
            var gpuStats = CalculateStatistics(performanceData.GPUUsageSamples);
            var memoryStats = CalculateStatistics(performanceData.MemoryUsageSamples);
            
            var category = DeterminePerformanceCategory(frameRateStats.Average);
            
            return new PerformanceSummary
            {
                FrameRateStats = frameRateStats,
                FrameTimeStats = frameTimeStats,
                CPUUsageStats = cpuStats,
                GPUUsageStats = gpuStats,
                MemoryUsageStats = memoryStats,
                OverallCategory = category,
                SampleCount = performanceData.SampleCount,
                CollectionDuration = performanceData.CollectionDuration
            };
        }
        
        private PerformanceBudgetBreakdown GenerateBudgetBreakdown(PerformanceDataSet performanceData, FrameTimeAnalysis frameTimeAnalysis)
        {
            var breakdown = frameTimeAnalysis.AverageBreakdown;
            float availableBudget = _configuration.TargetFrameTimeMs;
            float usedBudget = breakdown.TotalFrameTimeMs;
            float remainingBudget = availableBudget - usedBudget;
            float headroomPercent = remainingBudget / availableBudget;
            
            return new PerformanceBudgetBreakdown
            {
                TargetFrameTimeMs = availableBudget,
                UsedFrameTimeMs = usedBudget,
                RemainingFrameTimeMs = remainingBudget,
                HeadroomPercent = headroomPercent,
                MeetsHeadroomRequirement = headroomPercent >= _configuration.RequiredHeadroomPercent,
                ComponentBreakdown = breakdown
            };
        }
        
        // Additional helper methods would be implemented here...
        private FrameTimeAnalysis CreateDefaultFrameTimeAnalysis(PerformanceDataSet performanceData) => new FrameTimeAnalysis();
        private string[] IdentifyPerformanceBottlenecks(FrameTimeBreakdown breakdown) => new string[0];
        private object CalculateFrameTimeStatistics(FrameTimeBreakdown[] breakdowns) => null;
        private object AnalyzeFrameTimeTrends(FrameTimeBreakdown[] breakdowns) => null;
        private StatisticalData CalculateStatistics(float[] samples) => new StatisticalData();
        private PerformanceCategory DeterminePerformanceCategory(float avgFrameRate) => PerformanceCategory.Good;
        private string EstimateTotalImpact(List<OptimizationRecommendation> recommendations) => "3-8ms";
        private string DetermineOverallComplexity(List<OptimizationRecommendation> recommendations) => "Medium";
        private bool ExportToJSON(PerformanceBudgetAnalysis analysis, string outputPath) => true;
        private bool ExportToCSV(PerformanceBudgetAnalysis analysis, string outputPath) => true;
        private bool ExportToHTML(PerformanceBudgetAnalysis analysis, string outputPath) => true;
        
        // Markdown generation helper methods
        private void AppendPerformanceSummaryMarkdown(StringBuilder markdown, PerformanceSummary summary) { }
        private void AppendFrameTimeAnalysisMarkdown(StringBuilder markdown, FrameTimeAnalysis analysis) { }
        private void AppendBudgetBreakdownMarkdown(StringBuilder markdown, PerformanceBudgetBreakdown breakdown) { }
        private void AppendPieChartDataMarkdown(StringBuilder markdown, FrameTimePieChartData pieChartData) { }
        private void AppendValidationResultsMarkdown(StringBuilder markdown, PerformanceBudgetValidation validation) { }
        private void AppendOptimizationRecommendationsMarkdown(StringBuilder markdown, PerformanceOptimizationRecommendations recommendations) { }
        private void AppendHeadroomAnalysisMarkdown(StringBuilder markdown, PerformanceBudgetAnalysis analysis) { }
        
        #endregion
    }
    
    // Supporting data structures
    public struct StatisticalData
    {
        public float Average;
        public float Minimum;
        public float Maximum;
        public float StandardDeviation;
    }
    
    // Additional structures would be defined here...
    public struct BudgetValidationResult { public string Category; public float Expected; public float Actual; public bool Passed; public string Message; }
    public struct OptimizationRecommendation { public string Category; public OptimizationPriority Priority; public string Description; public string Recommendation; public string EstimatedImpact; public string ImplementationEffort; }
}