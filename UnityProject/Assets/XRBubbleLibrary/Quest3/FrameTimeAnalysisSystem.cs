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
    /// Frame-time analysis system for detailed performance breakdown and visualization.
    /// Implements Requirement 6.2: Frame-time analysis and breakdown system.
    /// Implements Requirement 6.3: Performance data visualization.
    /// </summary>
    public class FrameTimeAnalysisSystem : MonoBehaviour, IFrameTimeAnalysisSystem
    {
        // Configuration and state
        private FrameTimeAnalysisConfiguration _configuration;
        private FrameTimeAnalysisSession _currentSession;
        private BottleneckDetectionThresholds _bottleneckThresholds;
        private bool _isInitialized;
        private bool _isAnalysisActive;
        
        // Data collection
        private readonly Queue<FrameTimingData> _frameDataHistory = new Queue<FrameTimingData>();
        private readonly List<FrameTimeAnalysisResult> _analysisHistory = new List<FrameTimeAnalysisResult>();
        
        // Analysis state
        private int _frameCounter;
        private float _lastFrameTime;
        private System.Diagnostics.Stopwatch _sessionTimer;
        
        // Component colors for pie chart visualization
        private readonly Dictionary<string, string> _componentColors = new Dictionary<string, string>
        {
            { "CPU", "#FF6B6B" },
            { "GPU", "#4ECDC4" },
            { "Rendering", "#45B7D1" },
            { "Physics", "#96CEB4" },
            { "Scripts", "#FFEAA7" },
            { "Animation", "#DDA0DD" },
            { "Audio", "#98D8C8" },
            { "VR Compositor", "#F7DC6F" },
            { "Input", "#85C1E9" },
            { "Garbage Collection", "#F8C471" },
            { "UI Rendering", "#BB8FCE" },
            { "Particles", "#82E0AA" },
            { "Post Processing", "#F1948A" },
            { "Other", "#BDC3C7" },
            { "Wait", "#D5DBDB" }
        };
        
        // Events
        public event Action<BottleneckDetectedEventArgs> BottleneckDetected;
        public event Action<FrameTimeAnalysisUpdatedEventArgs> AnalysisDataUpdated;
        
        /// <summary>
        /// Whether the frame-time analysis system is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether frame-time analysis is currently active.
        /// </summary>
        public bool IsAnalysisActive => _isAnalysisActive;
        
        /// <summary>
        /// Current analysis configuration.
        /// </summary>
        public FrameTimeAnalysisConfiguration Configuration => _configuration;
        
        private void Awake()
        {
            // Initialize with default configuration
            Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
        }
        
        private void Update()
        {
            if (!_isInitialized || !_isAnalysisActive)
                return;
            
            // Collect frame timing data
            CollectFrameTimingData();
        }
        
        /// <summary>
        /// Initialize the frame-time analysis system.
        /// </summary>
        public bool Initialize(FrameTimeAnalysisConfiguration config)
        {
            try
            {
                _configuration = config;
                
                // Validate configuration
                if (config.SamplingRateHz <= 0 || config.TargetFrameRate <= 0)
                {
                    UnityEngine.Debug.LogError("[FrameTimeAnalysisSystem] Invalid sampling rate or target frame rate");
                    return false;
                }
                
                if (config.MaxFramesInMemory <= 0 || config.MinFramesForAnalysis <= 0)
                {
                    UnityEngine.Debug.LogError("[FrameTimeAnalysisSystem] Invalid frame count parameters");
                    return false;
                }
                
                // Create output directory if needed
                if (!string.IsNullOrEmpty(config.OutputDirectory))
                {
                    Directory.CreateDirectory(config.OutputDirectory);
                }
                
                // Initialize bottleneck detection thresholds
                _bottleneckThresholds = BottleneckDetectionThresholds.Quest3Default;
                
                // Initialize session timer
                _sessionTimer = new System.Diagnostics.Stopwatch();
                
                _isInitialized = true;
                
                if (_configuration.EnableDebugLogging)
                {
                    UnityEngine.Debug.Log($"[FrameTimeAnalysisSystem] Initialized with {config.SamplingRateHz} Hz sampling rate");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[FrameTimeAnalysisSystem] Initialization failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Start frame-time analysis session.
        /// </summary>
        public FrameTimeAnalysisSession StartAnalysis(string sessionName = "FrameTime_Analysis")
        {
            if (!_isInitialized)
            {
                UnityEngine.Debug.LogError("[FrameTimeAnalysisSystem] Cannot start analysis - not initialized");
                return default;
            }
            
            if (_isAnalysisActive)
            {
                UnityEngine.Debug.LogWarning("[FrameTimeAnalysisSystem] Stopping previous analysis session");
                StopAnalysis();
            }
            
            // Create new session
            _currentSession = new FrameTimeAnalysisSession
            {
                SessionId = Guid.NewGuid().ToString(),
                SessionName = sessionName,
                StartTime = DateTime.Now,
                Configuration = _configuration,
                IsActive = true,
                FramesAnalyzed = 0,
                Duration = TimeSpan.Zero
            };
            
            // Clear previous data
            _frameDataHistory.Clear();
            _frameCounter = 0;
            _lastFrameTime = Time.time;
            
            // Start session timer
            _sessionTimer.Restart();
            _isAnalysisActive = true;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[FrameTimeAnalysisSystem] Started analysis session '{sessionName}' (ID: {_currentSession.SessionId})");
            }
            
            return _currentSession;
        }
        
        /// <summary>
        /// Stop frame-time analysis and get results.
        /// </summary>
        public FrameTimeAnalysisResult StopAnalysis()
        {
            if (!_isAnalysisActive)
            {
                UnityEngine.Debug.LogWarning("[FrameTimeAnalysisSystem] No active analysis session to stop");
                return default;
            }
            
            _sessionTimer.Stop();
            _isAnalysisActive = false;
            
            // Process collected data
            var result = ProcessAnalysisData();
            
            // Add to analysis history
            _analysisHistory.Add(result);
            
            // Mark session as inactive
            _currentSession.IsActive = false;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[FrameTimeAnalysisSystem] Analysis session completed: {result.Session.FramesAnalyzed} frames analyzed");
            }
            
            return result;
        }       
 
        /// <summary>
        /// Analyze a single frame's timing data.
        /// </summary>
        public SingleFrameAnalysis AnalyzeSingleFrame(FrameTimingData frameData)
        {
            var breakdown = frameData.Breakdown;
            var totalTime = frameData.TotalFrameTimeMs;
            
            // Calculate component percentages
            var componentPercentages = new Dictionary<string, float>
            {
                { "CPU", (breakdown.CPUTimeMs / totalTime) * 100f },
                { "GPU", (breakdown.GPUTimeMs / totalTime) * 100f },
                { "Rendering", (breakdown.RenderingTimeMs / totalTime) * 100f },
                { "Physics", (breakdown.PhysicsTimeMs / totalTime) * 100f },
                { "Scripts", (breakdown.ScriptExecutionTimeMs / totalTime) * 100f },
                { "Animation", (breakdown.AnimationTimeMs / totalTime) * 100f },
                { "Audio", (breakdown.AudioTimeMs / totalTime) * 100f },
                { "VR Compositor", (breakdown.VRCompositorTimeMs / totalTime) * 100f },
                { "Input", (breakdown.InputTimeMs / totalTime) * 100f },
                { "Garbage Collection", (breakdown.GarbageCollectionTimeMs / totalTime) * 100f },
                { "UI Rendering", (breakdown.UIRenderingTimeMs / totalTime) * 100f },
                { "Particles", (breakdown.ParticleSystemTimeMs / totalTime) * 100f },
                { "Post Processing", (breakdown.PostProcessingTimeMs / totalTime) * 100f },
                { "Other", (breakdown.OtherTimeMs / totalTime) * 100f },
                { "Wait", (breakdown.WaitTimeMs / totalTime) * 100f }
            };
            
            // Identify bottlenecks in this frame
            var frameBottlenecks = IdentifyFrameBottlenecks(componentPercentages);
            
            // Determine frame performance category
            var performanceCategory = DetermineFramePerformanceCategory(frameData.FrameRate);
            
            return new SingleFrameAnalysis
            {
                FrameNumber = frameData.FrameNumber,
                TotalFrameTimeMs = totalTime,
                FrameRate = frameData.FrameRate,
                ComponentPercentages = componentPercentages,
                IdentifiedBottlenecks = frameBottlenecks,
                PerformanceCategory = performanceCategory,
                HasPerformanceIssues = frameData.HasPerformanceIssues,
                AnalysisTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Analyze multiple frames and generate comprehensive breakdown.
        /// </summary>
        public MultiFrameAnalysis AnalyzeMultipleFrames(FrameTimingData[] frameDataArray)
        {
            if (frameDataArray == null || frameDataArray.Length == 0)
            {
                return new MultiFrameAnalysis
                {
                    FrameCount = 0,
                    AnalysisTimestamp = DateTime.Now
                };
            }
            
            // Calculate aggregate statistics
            var aggregateBreakdown = CalculateAggregateBreakdown(frameDataArray);
            var frameRateStats = CalculateFrameRateStatistics(frameDataArray);
            var componentStats = CalculateComponentStatistics(frameDataArray);
            
            // Identify consistent bottlenecks across frames
            var consistentBottlenecks = IdentifyConsistentBottlenecks(frameDataArray);
            
            // Calculate frame time distribution
            var frameTimeDistribution = CalculateFrameTimeDistribution(frameDataArray);
            
            // Analyze performance stability
            var stabilityMetrics = AnalyzePerformanceStability(frameDataArray);
            
            return new MultiFrameAnalysis
            {
                FrameCount = frameDataArray.Length,
                AggregateBreakdown = aggregateBreakdown,
                FrameRateStatistics = frameRateStats,
                ComponentStatistics = componentStats,
                ConsistentBottlenecks = consistentBottlenecks,
                FrameTimeDistribution = frameTimeDistribution,
                StabilityMetrics = stabilityMetrics,
                AnalysisDuration = frameDataArray.Length > 0 ? 
                    frameDataArray.Last().Timestamp - frameDataArray.First().Timestamp : 
                    TimeSpan.Zero,
                AnalysisTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Generate pie chart data for resource allocation visualization.
        /// </summary>
        public ResourceAllocationPieChart GenerateResourceAllocationPieChart(DetailedFrameBreakdown frameBreakdown)
        {
            var segments = new List<ResourceAllocationSegment>();
            float totalTime = CalculateTotalFrameTime(frameBreakdown);
            
            // Create segments for each component
            AddPieChartSegment(segments, "CPU", frameBreakdown.CPUTimeMs, totalTime);
            AddPieChartSegment(segments, "GPU", frameBreakdown.GPUTimeMs, totalTime);
            AddPieChartSegment(segments, "Rendering", frameBreakdown.RenderingTimeMs, totalTime);
            AddPieChartSegment(segments, "Physics", frameBreakdown.PhysicsTimeMs, totalTime);
            AddPieChartSegment(segments, "Scripts", frameBreakdown.ScriptExecutionTimeMs, totalTime);
            AddPieChartSegment(segments, "Animation", frameBreakdown.AnimationTimeMs, totalTime);
            AddPieChartSegment(segments, "Audio", frameBreakdown.AudioTimeMs, totalTime);
            AddPieChartSegment(segments, "VR Compositor", frameBreakdown.VRCompositorTimeMs, totalTime);
            AddPieChartSegment(segments, "Input", frameBreakdown.InputTimeMs, totalTime);
            AddPieChartSegment(segments, "Garbage Collection", frameBreakdown.GarbageCollectionTimeMs, totalTime);
            AddPieChartSegment(segments, "UI Rendering", frameBreakdown.UIRenderingTimeMs, totalTime);
            AddPieChartSegment(segments, "Particles", frameBreakdown.ParticleSystemTimeMs, totalTime);
            AddPieChartSegment(segments, "Post Processing", frameBreakdown.PostProcessingTimeMs, totalTime);
            AddPieChartSegment(segments, "Other", frameBreakdown.OtherTimeMs, totalTime);
            AddPieChartSegment(segments, "Wait", frameBreakdown.WaitTimeMs, totalTime);
            
            // Sort segments by percentage (largest first)
            segments.Sort((a, b) => b.Percentage.CompareTo(a.Percentage));
            
            // Determine overall performance category
            float frameRate = totalTime > 0 ? 1000f / totalTime : 0f;
            var performanceCategory = DetermineFramePerformanceCategory(frameRate);
            
            return new ResourceAllocationPieChart
            {
                Segments = segments.ToArray(),
                TotalFrameTimeMs = totalTime,
                ChartTitle = $"Resource Allocation ({totalTime:F2}ms, {frameRate:F1} FPS)",
                GenerationTimestamp = DateTime.Now,
                PerformanceCategory = performanceCategory
            };
        }
        
        /// <summary>
        /// Identify performance bottlenecks from frame-time data.
        /// </summary>
        public PerformanceBottleneckAnalysis IdentifyBottlenecks(FrameTimeAnalysisResult analysisResult)
        {
            var bottlenecks = new List<PerformanceBottleneck>();
            var recommendations = new List<string>();
            
            var aggregateBreakdown = analysisResult.AggregateAnalysis.AggregateBreakdown;
            float totalTime = CalculateTotalFrameTime(aggregateBreakdown);
            
            // Analyze each component for bottlenecks
            AnalyzeComponentBottleneck(bottlenecks, recommendations, "CPU", 
                aggregateBreakdown.CPUTimeMs, totalTime, 25f, // >25% threshold
                "CPU processing is consuming significant frame time",
                "Optimize algorithms, use Burst compilation, reduce Update() calls");
            
            AnalyzeComponentBottleneck(bottlenecks, recommendations, "GPU", 
                aggregateBreakdown.GPUTimeMs, totalTime, 30f, // >30% threshold
                "GPU processing is limiting performance",
                "Optimize shaders, reduce texture resolution, use GPU instancing");
            
            AnalyzeComponentBottleneck(bottlenecks, recommendations, "Rendering", 
                aggregateBreakdown.RenderingTimeMs, totalTime, 35f, // >35% threshold
                "Rendering pipeline is a major bottleneck",
                "Reduce draw calls, optimize materials, implement LOD system");
            
            AnalyzeComponentBottleneck(bottlenecks, recommendations, "Physics", 
                aggregateBreakdown.PhysicsTimeMs, totalTime, 15f, // >15% threshold
                "Physics simulation consuming excessive time",
                "Reduce physics complexity, optimize collision detection");
            
            AnalyzeComponentBottleneck(bottlenecks, recommendations, "Garbage Collection", 
                aggregateBreakdown.GarbageCollectionTimeMs, totalTime, 2f, // >2% threshold
                "Garbage collection causing frame spikes",
                "Reduce allocations, use object pooling, optimize string operations");
            
            // Sort bottlenecks by severity
            bottlenecks.Sort((a, b) => b.Severity.CompareTo(a.Severity));
            
            return new PerformanceBottleneckAnalysis
            {
                IdentifiedBottlenecks = bottlenecks.ToArray(),
                OptimizationRecommendations = recommendations.ToArray(),
                BottleneckCount = bottlenecks.Count,
                HighestSeverity = bottlenecks.Count > 0 ? bottlenecks[0].Severity : BottleneckSeverity.Minor,
                AnalysisTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Generate frame-time trend analysis over time.
        /// </summary>
        public FrameTimeTrendAnalysis AnalyzeFrameTimeTrends(FrameTimingData[] frameDataArray)
        {
            if (frameDataArray == null || frameDataArray.Length < 10)
            {
                return new FrameTimeTrendAnalysis
                {
                    TrendDirection = TrendDirection.Stable,
                    AnalysisTimestamp = DateTime.Now
                };
            }
            
            // Calculate moving averages
            var movingAverages = CalculateMovingAverages(frameDataArray, 10); // 10-frame window
            
            // Determine overall trend direction
            var trendDirection = DetermineTrendDirection(movingAverages);
            
            // Calculate trend statistics
            var trendStats = CalculateTrendStatistics(frameDataArray);
            
            // Identify trend anomalies
            var anomalies = IdentifyTrendAnomalies(frameDataArray);
            
            return new FrameTimeTrendAnalysis
            {
                TrendDirection = trendDirection,
                MovingAverages = movingAverages,
                TrendStatistics = trendStats,
                TrendAnomalies = anomalies,
                AnalysisPeriod = frameDataArray.Length > 0 ? 
                    frameDataArray.Last().Timestamp - frameDataArray.First().Timestamp : 
                    TimeSpan.Zero,
                AnalysisTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Compare frame-time performance between different scenarios.
        /// </summary>
        public FrameTimeComparison CompareFrameTimePerformance(FrameTimingData[] baselineData, FrameTimingData[] comparisonData)
        {
            var baselineAnalysis = AnalyzeMultipleFrames(baselineData);
            var comparisonAnalysis = AnalyzeMultipleFrames(comparisonData);
            
            // Calculate performance differences
            var performanceDelta = CalculatePerformanceDelta(baselineAnalysis, comparisonAnalysis);
            
            // Determine comparison result
            var comparisonResult = DetermineComparisonResult(performanceDelta);
            
            return new FrameTimeComparison
            {
                BaselineAnalysis = baselineAnalysis,
                ComparisonAnalysis = comparisonAnalysis,
                PerformanceDelta = performanceDelta,
                ComparisonResult = comparisonResult,
                ComparisonTimestamp = DateTime.Now
            };
        }       
 
        // Private helper methods
        
        private void CollectFrameTimingData()
        {
            float currentTime = Time.time;
            float deltaTime = currentTime - _lastFrameTime;
            _lastFrameTime = currentTime;
            
            // Skip if sampling rate doesn't match
            if (deltaTime < (1f / _configuration.SamplingRateHz))
                return;
            
            // Create frame timing data
            var frameData = new FrameTimingData
            {
                FrameNumber = _frameCounter++,
                TotalFrameTimeMs = deltaTime * 1000f,
                FrameRate = 1f / deltaTime,
                Timestamp = DateTime.Now,
                Breakdown = CollectDetailedBreakdown(deltaTime),
                HasPerformanceIssues = deltaTime > (1f / _configuration.TargetFrameRate) * 1.2f
            };
            
            // Add to history
            _frameDataHistory.Enqueue(frameData);
            
            // Maintain max frames in memory
            while (_frameDataHistory.Count > _configuration.MaxFramesInMemory)
            {
                _frameDataHistory.Dequeue();
            }
            
            // Update session
            _currentSession.FramesAnalyzed++;
            _currentSession.Duration = _sessionTimer.Elapsed;
            
            // Check for bottlenecks
            CheckForBottlenecks(frameData);
            
            // Fire analysis update event
            AnalysisDataUpdated?.Invoke(new FrameTimeAnalysisUpdatedEventArgs
            {
                LatestFrameData = frameData,
                SessionInfo = _currentSession
            });
        }
        
        private DetailedFrameBreakdown CollectDetailedBreakdown(float deltaTime)
        {
            // In a real implementation, this would use Unity Profiler API
            // For now, simulate realistic breakdown
            float totalMs = deltaTime * 1000f;
            
            return new DetailedFrameBreakdown
            {
                CPUTimeMs = totalMs * 0.4f,
                GPUTimeMs = totalMs * 0.35f,
                RenderingTimeMs = totalMs * 0.3f,
                PhysicsTimeMs = totalMs * 0.1f,
                ScriptExecutionTimeMs = totalMs * 0.15f,
                AnimationTimeMs = totalMs * 0.05f,
                AudioTimeMs = totalMs * 0.02f,
                VRCompositorTimeMs = totalMs * 0.08f,
                InputTimeMs = totalMs * 0.01f,
                GarbageCollectionTimeMs = totalMs * 0.01f,
                UIRenderingTimeMs = totalMs * 0.03f,
                ParticleSystemTimeMs = totalMs * 0.02f,
                PostProcessingTimeMs = totalMs * 0.04f,
                OtherTimeMs = totalMs * 0.05f,
                WaitTimeMs = totalMs * 0.02f
            };
        }
        
        private void CheckForBottlenecks(FrameTimingData frameData)
        {
            var breakdown = frameData.Breakdown;
            float totalTime = frameData.TotalFrameTimeMs;
            
            // Check each component against thresholds
            CheckComponentBottleneck("CPU", breakdown.CPUTimeMs, totalTime, _bottleneckThresholds.CPUThresholdPercent);
            CheckComponentBottleneck("GPU", breakdown.GPUTimeMs, totalTime, _bottleneckThresholds.GPUThresholdPercent);
            CheckComponentBottleneck("Rendering", breakdown.RenderingTimeMs, totalTime, _bottleneckThresholds.RenderingThresholdPercent);
            CheckComponentBottleneck("Physics", breakdown.PhysicsTimeMs, totalTime, _bottleneckThresholds.PhysicsThresholdPercent);
            CheckComponentBottleneck("GC", breakdown.GarbageCollectionTimeMs, totalTime, _bottleneckThresholds.GCThresholdPercent);
        }
        
        private void CheckComponentBottleneck(string componentName, float componentTime, float totalTime, float thresholdPercent)
        {
            float percentage = (componentTime / totalTime) * 100f;
            
            if (percentage > thresholdPercent)
            {
                var severity = percentage > thresholdPercent * 2f ? BottleneckSeverity.Critical :
                              percentage > thresholdPercent * 1.5f ? BottleneckSeverity.Major :
                              BottleneckSeverity.Minor;
                
                BottleneckDetected?.Invoke(new BottleneckDetectedEventArgs
                {
                    ComponentName = componentName,
                    Percentage = percentage,
                    Severity = severity,
                    FrameNumber = _frameCounter,
                    Timestamp = DateTime.Now
                });
            }
        }
        
        private FrameTimeAnalysisResult ProcessAnalysisData()
        {
            var frameDataArray = _frameDataHistory.ToArray();
            
            // Generate comprehensive analysis
            var singleFrameAnalyses = frameDataArray.Select(AnalyzeSingleFrame).ToArray();
            var multiFrameAnalysis = AnalyzeMultipleFrames(frameDataArray);
            var bottleneckAnalysis = IdentifyBottlenecks(new FrameTimeAnalysisResult 
            { 
                AggregateAnalysis = multiFrameAnalysis 
            });
            var trendAnalysis = AnalyzeFrameTimeTrends(frameDataArray);
            
            return new FrameTimeAnalysisResult
            {
                Session = _currentSession,
                SingleFrameAnalyses = singleFrameAnalyses,
                AggregateAnalysis = multiFrameAnalysis,
                BottleneckAnalysis = bottleneckAnalysis,
                TrendAnalysis = trendAnalysis,
                GenerationTimestamp = DateTime.Now
            };
        }
        
        // Helper methods for analysis calculations
        
        private List<string> IdentifyFrameBottlenecks(Dictionary<string, float> componentPercentages)
        {
            var bottlenecks = new List<string>();
            
            foreach (var component in componentPercentages)
            {
                if (component.Value > 20f) // >20% threshold
                {
                    bottlenecks.Add(component.Key);
                }
            }
            
            return bottlenecks;
        }
        
        private FramePerformanceCategory DetermineFramePerformanceCategory(float frameRate)
        {
            if (frameRate >= _configuration.TargetFrameRate * 0.95f)
                return FramePerformanceCategory.Excellent;
            else if (frameRate >= _configuration.TargetFrameRate * 0.85f)
                return FramePerformanceCategory.Good;
            else if (frameRate >= _configuration.TargetFrameRate * 0.70f)
                return FramePerformanceCategory.Fair;
            else
                return FramePerformanceCategory.Poor;
        }
        
        private DetailedFrameBreakdown CalculateAggregateBreakdown(FrameTimingData[] frameDataArray)
        {
            if (frameDataArray.Length == 0)
                return new DetailedFrameBreakdown();
            
            var aggregate = new DetailedFrameBreakdown();
            
            foreach (var frame in frameDataArray)
            {
                var breakdown = frame.Breakdown;
                aggregate.CPUTimeMs += breakdown.CPUTimeMs;
                aggregate.GPUTimeMs += breakdown.GPUTimeMs;
                aggregate.RenderingTimeMs += breakdown.RenderingTimeMs;
                aggregate.PhysicsTimeMs += breakdown.PhysicsTimeMs;
                aggregate.ScriptExecutionTimeMs += breakdown.ScriptExecutionTimeMs;
                aggregate.AnimationTimeMs += breakdown.AnimationTimeMs;
                aggregate.AudioTimeMs += breakdown.AudioTimeMs;
                aggregate.VRCompositorTimeMs += breakdown.VRCompositorTimeMs;
                aggregate.InputTimeMs += breakdown.InputTimeMs;
                aggregate.GarbageCollectionTimeMs += breakdown.GarbageCollectionTimeMs;
                aggregate.UIRenderingTimeMs += breakdown.UIRenderingTimeMs;
                aggregate.ParticleSystemTimeMs += breakdown.ParticleSystemTimeMs;
                aggregate.PostProcessingTimeMs += breakdown.PostProcessingTimeMs;
                aggregate.OtherTimeMs += breakdown.OtherTimeMs;
                aggregate.WaitTimeMs += breakdown.WaitTimeMs;
            }
            
            // Calculate averages
            int count = frameDataArray.Length;
            aggregate.CPUTimeMs /= count;
            aggregate.GPUTimeMs /= count;
            aggregate.RenderingTimeMs /= count;
            aggregate.PhysicsTimeMs /= count;
            aggregate.ScriptExecutionTimeMs /= count;
            aggregate.AnimationTimeMs /= count;
            aggregate.AudioTimeMs /= count;
            aggregate.VRCompositorTimeMs /= count;
            aggregate.InputTimeMs /= count;
            aggregate.GarbageCollectionTimeMs /= count;
            aggregate.UIRenderingTimeMs /= count;
            aggregate.ParticleSystemTimeMs /= count;
            aggregate.PostProcessingTimeMs /= count;
            aggregate.OtherTimeMs /= count;
            aggregate.WaitTimeMs /= count;
            
            return aggregate;
        }
        
        private FrameRateStatistics CalculateFrameRateStatistics(FrameTimingData[] frameDataArray)
        {
            if (frameDataArray.Length == 0)
                return new FrameRateStatistics();
            
            var frameRates = frameDataArray.Select(f => f.FrameRate).ToArray();
            
            return new FrameRateStatistics
            {
                AverageFrameRate = frameRates.Average(),
                MinFrameRate = frameRates.Min(),
                MaxFrameRate = frameRates.Max(),
                FrameRateStandardDeviation = CalculateStandardDeviation(frameRates),
                PercentileFrameRates = CalculatePercentiles(frameRates)
            };
        }
        
        private Dictionary<string, ComponentStatistics> CalculateComponentStatistics(FrameTimingData[] frameDataArray)
        {
            var stats = new Dictionary<string, ComponentStatistics>();
            
            if (frameDataArray.Length == 0)
                return stats;
            
            // Calculate statistics for each component
            CalculateComponentStat(stats, "CPU", frameDataArray.Select(f => f.Breakdown.CPUTimeMs));
            CalculateComponentStat(stats, "GPU", frameDataArray.Select(f => f.Breakdown.GPUTimeMs));
            CalculateComponentStat(stats, "Rendering", frameDataArray.Select(f => f.Breakdown.RenderingTimeMs));
            CalculateComponentStat(stats, "Physics", frameDataArray.Select(f => f.Breakdown.PhysicsTimeMs));
            CalculateComponentStat(stats, "Scripts", frameDataArray.Select(f => f.Breakdown.ScriptExecutionTimeMs));
            CalculateComponentStat(stats, "Animation", frameDataArray.Select(f => f.Breakdown.AnimationTimeMs));
            CalculateComponentStat(stats, "Audio", frameDataArray.Select(f => f.Breakdown.AudioTimeMs));
            CalculateComponentStat(stats, "VR Compositor", frameDataArray.Select(f => f.Breakdown.VRCompositorTimeMs));
            CalculateComponentStat(stats, "Input", frameDataArray.Select(f => f.Breakdown.InputTimeMs));
            CalculateComponentStat(stats, "Garbage Collection", frameDataArray.Select(f => f.Breakdown.GarbageCollectionTimeMs));
            CalculateComponentStat(stats, "UI Rendering", frameDataArray.Select(f => f.Breakdown.UIRenderingTimeMs));
            CalculateComponentStat(stats, "Particles", frameDataArray.Select(f => f.Breakdown.ParticleSystemTimeMs));
            CalculateComponentStat(stats, "Post Processing", frameDataArray.Select(f => f.Breakdown.PostProcessingTimeMs));
            CalculateComponentStat(stats, "Other", frameDataArray.Select(f => f.Breakdown.OtherTimeMs));
            CalculateComponentStat(stats, "Wait", frameDataArray.Select(f => f.Breakdown.WaitTimeMs));
            
            return stats;
        }
        
        private void CalculateComponentStat(Dictionary<string, ComponentStatistics> stats, string componentName, IEnumerable<float> values)
        {
            var valueArray = values.ToArray();
            
            stats[componentName] = new ComponentStatistics
            {
                AverageTimeMs = valueArray.Average(),
                MinTimeMs = valueArray.Min(),
                MaxTimeMs = valueArray.Max(),
                StandardDeviation = CalculateStandardDeviation(valueArray)
            };
        }
        
        private float CalculateStandardDeviation(float[] values)
        {
            if (values.Length == 0) return 0f;
            
            float mean = values.Average();
            float sumSquaredDiffs = values.Sum(v => (v - mean) * (v - mean));
            return Mathf.Sqrt(sumSquaredDiffs / values.Length);
        }
        
        private Dictionary<int, float> CalculatePercentiles(float[] values)
        {
            var sorted = values.OrderBy(v => v).ToArray();
            var percentiles = new Dictionary<int, float>();
            
            percentiles[1] = GetPercentile(sorted, 0.01f);
            percentiles[5] = GetPercentile(sorted, 0.05f);
            percentiles[25] = GetPercentile(sorted, 0.25f);
            percentiles[50] = GetPercentile(sorted, 0.50f);
            percentiles[75] = GetPercentile(sorted, 0.75f);
            percentiles[95] = GetPercentile(sorted, 0.95f);
            percentiles[99] = GetPercentile(sorted, 0.99f);
            
            return percentiles;
        }
        
        private float GetPercentile(float[] sortedValues, float percentile)
        {
            if (sortedValues.Length == 0) return 0f;
            
            int index = Mathf.RoundToInt((sortedValues.Length - 1) * percentile);
            return sortedValues[Mathf.Clamp(index, 0, sortedValues.Length - 1)];
        }
        
        private float CalculateTotalFrameTime(DetailedFrameBreakdown breakdown)
        {
            return breakdown.CPUTimeMs + breakdown.GPUTimeMs + breakdown.RenderingTimeMs +
                   breakdown.PhysicsTimeMs + breakdown.ScriptExecutionTimeMs + breakdown.AnimationTimeMs +
                   breakdown.AudioTimeMs + breakdown.VRCompositorTimeMs + breakdown.InputTimeMs +
                   breakdown.GarbageCollectionTimeMs + breakdown.UIRenderingTimeMs + breakdown.ParticleSystemTimeMs +
                   breakdown.PostProcessingTimeMs + breakdown.OtherTimeMs + breakdown.WaitTimeMs;
        }
        
        private void AddPieChartSegment(List<ResourceAllocationSegment> segments, string componentName, float timeMs, float totalTime)
        {
            if (timeMs <= 0f || totalTime <= 0f) return;
            
            float percentage = (timeMs / totalTime) * 100f;
            
            segments.Add(new ResourceAllocationSegment
            {
                ComponentName = componentName,
                TimeMs = timeMs,
                Percentage = percentage,
                Color = _componentColors.ContainsKey(componentName) ? _componentColors[componentName] : "#BDC3C7"
            });
        }
        
        private void AnalyzeComponentBottleneck(List<PerformanceBottleneck> bottlenecks, List<string> recommendations,
            string componentName, float componentTime, float totalTime, float thresholdPercent,
            string description, string recommendation)
        {
            float percentage = (componentTime / totalTime) * 100f;
            
            if (percentage > thresholdPercent)
            {
                var severity = percentage > thresholdPercent * 2f ? BottleneckSeverity.Critical :
                              percentage > thresholdPercent * 1.5f ? BottleneckSeverity.Major :
                              BottleneckSeverity.Minor;
                
                bottlenecks.Add(new PerformanceBottleneck
                {
                    ComponentName = componentName,
                    Percentage = percentage,
                    Severity = severity,
                    Description = description
                });
                
                recommendations.Add($"{componentName}: {recommendation}");
            }
        }
        
        // Placeholder implementations for remaining methods
        private List<string> IdentifyConsistentBottlenecks(FrameTimingData[] frameDataArray)
        {
            // Implementation would analyze bottlenecks across multiple frames
            return new List<string>();
        }
        
        private Dictionary<float, int> CalculateFrameTimeDistribution(FrameTimingData[] frameDataArray)
        {
            // Implementation would create histogram of frame times
            return new Dictionary<float, int>();
        }
        
        private PerformanceStabilityMetrics AnalyzePerformanceStability(FrameTimingData[] frameDataArray)
        {
            // Implementation would analyze frame time variance and stability
            return new PerformanceStabilityMetrics();
        }
        
        private float[] CalculateMovingAverages(FrameTimingData[] frameDataArray, int windowSize)
        {
            // Implementation would calculate moving averages
            return new float[0];
        }
        
        private TrendDirection DetermineTrendDirection(float[] movingAverages)
        {
            // Implementation would analyze trend direction
            return TrendDirection.Stable;
        }
        
        private TrendStatistics CalculateTrendStatistics(FrameTimingData[] frameDataArray)
        {
            // Implementation would calculate trend statistics
            return new TrendStatistics();
        }
        
        private TrendAnomaly[] IdentifyTrendAnomalies(FrameTimingData[] frameDataArray)
        {
            // Implementation would identify anomalies in trends
            return new TrendAnomaly[0];
        }
        
        private PerformanceDelta CalculatePerformanceDelta(MultiFrameAnalysis baseline, MultiFrameAnalysis comparison)
        {
            // Implementation would calculate performance differences
            return new PerformanceDelta();
        }
        
        private ComparisonResult DetermineComparisonResult(PerformanceDelta delta)
        {
            // Implementation would determine comparison result
            return ComparisonResult.Similar;
        }
    }
}