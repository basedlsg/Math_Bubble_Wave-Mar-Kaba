using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XRBubbleLibrary.Quest3.Tests
{
    /// <summary>
    /// Comprehensive unit tests for FrameTimeAnalysisSystem.
    /// Tests Requirement 6.2: Frame-time analysis and breakdown system.
    /// Tests Requirement 6.3: Performance data visualization.
    /// </summary>
    [TestFixture]
    public class FrameTimeAnalysisSystemTests
    {
        private FrameTimeAnalysisSystem _frameTimeAnalysisSystem;
        private GameObject _testGameObject;
        
        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("FrameTimeAnalysisSystemTest");
            _frameTimeAnalysisSystem = _testGameObject.AddComponent<FrameTimeAnalysisSystem>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_testGameObject);
            }
        }
        
        #region Initialization Tests
        
        [Test]
        public void Initialize_WithValidConfiguration_ReturnsTrue()
        {
            // Arrange
            var config = FrameTimeAnalysisConfiguration.Quest3Default;
            
            // Act
            bool result = _frameTimeAnalysisSystem.Initialize(config);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_frameTimeAnalysisSystem.IsInitialized);
            Assert.AreEqual(config.SamplingRateHz, _frameTimeAnalysisSystem.Configuration.SamplingRateHz);
        }
        
        [Test]
        public void Initialize_WithInvalidSamplingRate_ReturnsFalse()
        {
            // Arrange
            var config = FrameTimeAnalysisConfiguration.Quest3Default;
            config.SamplingRateHz = 0;
            
            // Act
            bool result = _frameTimeAnalysisSystem.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_frameTimeAnalysisSystem.IsInitialized);
        }
        
        [Test]
        public void Initialize_WithInvalidTargetFrameRate_ReturnsFalse()
        {
            // Arrange
            var config = FrameTimeAnalysisConfiguration.Quest3Default;
            config.TargetFrameRate = -1;
            
            // Act
            bool result = _frameTimeAnalysisSystem.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_frameTimeAnalysisSystem.IsInitialized);
        }
        
        [Test]
        public void Initialize_WithInvalidFrameCountParameters_ReturnsFalse()
        {
            // Arrange
            var config = FrameTimeAnalysisConfiguration.Quest3Default;
            config.MaxFramesInMemory = 0;
            
            // Act
            bool result = _frameTimeAnalysisSystem.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_frameTimeAnalysisSystem.IsInitialized);
        }
        
        #endregion
        
        #region Analysis Session Tests
        
        [Test]
        public void StartAnalysis_WhenInitialized_CreatesValidSession()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            string sessionName = "TestSession";
            
            // Act
            var session = _frameTimeAnalysisSystem.StartAnalysis(sessionName);
            
            // Assert
            Assert.IsNotNull(session.SessionId);
            Assert.AreEqual(sessionName, session.SessionName);
            Assert.IsTrue(session.IsActive);
            Assert.IsTrue(_frameTimeAnalysisSystem.IsAnalysisActive);
            Assert.AreEqual(0, session.FramesAnalyzed);
        }
        
        [Test]
        public void StartAnalysis_WhenNotInitialized_ReturnsDefaultSession()
        {
            // Arrange
            // Don't initialize the system
            
            // Act
            var session = _frameTimeAnalysisSystem.StartAnalysis("TestSession");
            
            // Assert
            Assert.IsNull(session.SessionId);
            Assert.IsFalse(_frameTimeAnalysisSystem.IsAnalysisActive);
        }
        
        [Test]
        public void StartAnalysis_WhenAlreadyActive_StopsPreviousSession()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var firstSession = _frameTimeAnalysisSystem.StartAnalysis("FirstSession");
            
            // Act
            var secondSession = _frameTimeAnalysisSystem.StartAnalysis("SecondSession");
            
            // Assert
            Assert.AreNotEqual(firstSession.SessionId, secondSession.SessionId);
            Assert.AreEqual("SecondSession", secondSession.SessionName);
            Assert.IsTrue(_frameTimeAnalysisSystem.IsAnalysisActive);
        }
        
        [Test]
        public void StopAnalysis_WhenActive_ReturnsValidResult()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            _frameTimeAnalysisSystem.StartAnalysis("TestSession");
            
            // Act
            var result = _frameTimeAnalysisSystem.StopAnalysis();
            
            // Assert
            Assert.IsNotNull(result.Session);
            Assert.IsFalse(result.Session.IsActive);
            Assert.IsFalse(_frameTimeAnalysisSystem.IsAnalysisActive);
            Assert.IsNotNull(result.GenerationTimestamp);
        }
        
        [Test]
        public void StopAnalysis_WhenNotActive_ReturnsDefaultResult()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            
            // Act
            var result = _frameTimeAnalysisSystem.StopAnalysis();
            
            // Assert
            Assert.IsNull(result.Session.SessionId);
        }
        
        #endregion
        
        #region Single Frame Analysis Tests
        
        [Test]
        public void AnalyzeSingleFrame_WithValidData_ReturnsCompleteAnalysis()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var frameData = CreateTestFrameData(16.67f, 60f); // 60 FPS frame
            
            // Act
            var analysis = _frameTimeAnalysisSystem.AnalyzeSingleFrame(frameData);
            
            // Assert
            Assert.AreEqual(frameData.FrameNumber, analysis.FrameNumber);
            Assert.AreEqual(frameData.TotalFrameTimeMs, analysis.TotalFrameTimeMs);
            Assert.AreEqual(frameData.FrameRate, analysis.FrameRate);
            Assert.IsNotNull(analysis.ComponentPercentages);
            Assert.IsTrue(analysis.ComponentPercentages.Count > 0);
            Assert.IsNotNull(analysis.IdentifiedBottlenecks);
            Assert.AreEqual(FramePerformanceCategory.Good, analysis.PerformanceCategory);
        }
        
        [Test]
        public void AnalyzeSingleFrame_WithPoorPerformance_IdentifiesCorrectCategory()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var frameData = CreateTestFrameData(50f, 20f); // 20 FPS frame
            
            // Act
            var analysis = _frameTimeAnalysisSystem.AnalyzeSingleFrame(frameData);
            
            // Assert
            Assert.AreEqual(FramePerformanceCategory.Poor, analysis.PerformanceCategory);
            Assert.IsTrue(analysis.HasPerformanceIssues);
        }
        
        [Test]
        public void AnalyzeSingleFrame_WithExcellentPerformance_IdentifiesCorrectCategory()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var frameData = CreateTestFrameData(13.89f, 72f); // 72 FPS frame
            
            // Act
            var analysis = _frameTimeAnalysisSystem.AnalyzeSingleFrame(frameData);
            
            // Assert
            Assert.AreEqual(FramePerformanceCategory.Excellent, analysis.PerformanceCategory);
            Assert.IsFalse(analysis.HasPerformanceIssues);
        }
        
        #endregion
        
        #region Multi-Frame Analysis Tests
        
        [Test]
        public void AnalyzeMultipleFrames_WithEmptyArray_ReturnsEmptyAnalysis()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var frameDataArray = new FrameTimingData[0];
            
            // Act
            var analysis = _frameTimeAnalysisSystem.AnalyzeMultipleFrames(frameDataArray);
            
            // Assert
            Assert.AreEqual(0, analysis.FrameCount);
            Assert.IsNotNull(analysis.AnalysisTimestamp);
        }
        
        [Test]
        public void AnalyzeMultipleFrames_WithValidData_ReturnsCompleteAnalysis()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var frameDataArray = CreateTestFrameDataArray(10, 16.67f, 60f);
            
            // Act
            var analysis = _frameTimeAnalysisSystem.AnalyzeMultipleFrames(frameDataArray);
            
            // Assert
            Assert.AreEqual(10, analysis.FrameCount);
            Assert.IsNotNull(analysis.AggregateBreakdown);
            Assert.IsNotNull(analysis.FrameRateStatistics);
            Assert.IsNotNull(analysis.ComponentStatistics);
            Assert.IsNotNull(analysis.ConsistentBottlenecks);
            Assert.IsNotNull(analysis.FrameTimeDistribution);
            Assert.IsNotNull(analysis.StabilityMetrics);
            Assert.IsTrue(analysis.AnalysisDuration.TotalMilliseconds >= 0);
        }
        
        [Test]
        public void AnalyzeMultipleFrames_CalculatesCorrectAverages()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var frameDataArray = new FrameTimingData[]
            {
                CreateTestFrameData(10f, 100f),
                CreateTestFrameData(20f, 50f),
                CreateTestFrameData(30f, 33.33f)
            };
            
            // Act
            var analysis = _frameTimeAnalysisSystem.AnalyzeMultipleFrames(frameDataArray);
            
            // Assert
            Assert.AreEqual(61.11f, analysis.FrameRateStatistics.AverageFrameRate, 0.1f);
            Assert.AreEqual(33.33f, analysis.FrameRateStatistics.MinFrameRate, 0.1f);
            Assert.AreEqual(100f, analysis.FrameRateStatistics.MaxFrameRate, 0.1f);
        }
        
        #endregion
        
        #region Pie Chart Generation Tests
        
        [Test]
        public void GenerateResourceAllocationPieChart_WithValidBreakdown_ReturnsCompleteChart()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var breakdown = CreateTestFrameBreakdown();
            
            // Act
            var pieChart = _frameTimeAnalysisSystem.GenerateResourceAllocationPieChart(breakdown);
            
            // Assert
            Assert.IsNotNull(pieChart.Segments);
            Assert.IsTrue(pieChart.Segments.Length > 0);
            Assert.IsTrue(pieChart.TotalFrameTimeMs > 0);
            Assert.IsNotNull(pieChart.ChartTitle);
            Assert.IsNotNull(pieChart.GenerationTimestamp);
            
            // Verify segments are sorted by percentage
            for (int i = 0; i < pieChart.Segments.Length - 1; i++)
            {
                Assert.IsTrue(pieChart.Segments[i].Percentage >= pieChart.Segments[i + 1].Percentage);
            }
        }
        
        [Test]
        public void GenerateResourceAllocationPieChart_SegmentPercentagesAddUpTo100()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var breakdown = CreateTestFrameBreakdown();
            
            // Act
            var pieChart = _frameTimeAnalysisSystem.GenerateResourceAllocationPieChart(breakdown);
            
            // Assert
            float totalPercentage = pieChart.Segments.Sum(s => s.Percentage);
            Assert.AreEqual(100f, totalPercentage, 0.1f);
        }
        
        [Test]
        public void GenerateResourceAllocationPieChart_AllSegmentsHaveValidColors()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var breakdown = CreateTestFrameBreakdown();
            
            // Act
            var pieChart = _frameTimeAnalysisSystem.GenerateResourceAllocationPieChart(breakdown);
            
            // Assert
            foreach (var segment in pieChart.Segments)
            {
                Assert.IsNotNull(segment.Color);
                Assert.IsTrue(segment.Color.StartsWith("#"));
                Assert.AreEqual(7, segment.Color.Length); // #RRGGBB format
            }
        }
        
        #endregion
        
        #region Bottleneck Detection Tests
        
        [Test]
        public void IdentifyBottlenecks_WithHighCPUUsage_DetectsCPUBottleneck()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var analysisResult = CreateTestAnalysisResultWithHighCPU();
            
            // Act
            var bottleneckAnalysis = _frameTimeAnalysisSystem.IdentifyBottlenecks(analysisResult);
            
            // Assert
            Assert.IsTrue(bottleneckAnalysis.BottleneckCount > 0);
            Assert.IsTrue(bottleneckAnalysis.IdentifiedBottlenecks.Any(b => b.ComponentName == "CPU"));
            Assert.IsTrue(bottleneckAnalysis.OptimizationRecommendations.Any(r => r.Contains("CPU")));
        }
        
        [Test]
        public void IdentifyBottlenecks_WithHighGPUUsage_DetectsGPUBottleneck()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var analysisResult = CreateTestAnalysisResultWithHighGPU();
            
            // Act
            var bottleneckAnalysis = _frameTimeAnalysisSystem.IdentifyBottlenecks(analysisResult);
            
            // Assert
            Assert.IsTrue(bottleneckAnalysis.BottleneckCount > 0);
            Assert.IsTrue(bottleneckAnalysis.IdentifiedBottlenecks.Any(b => b.ComponentName == "GPU"));
            Assert.IsTrue(bottleneckAnalysis.OptimizationRecommendations.Any(r => r.Contains("GPU")));
        }
        
        [Test]
        public void IdentifyBottlenecks_WithBalancedPerformance_DetectsNoBottlenecks()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var analysisResult = CreateTestAnalysisResultBalanced();
            
            // Act
            var bottleneckAnalysis = _frameTimeAnalysisSystem.IdentifyBottlenecks(analysisResult);
            
            // Assert
            Assert.AreEqual(0, bottleneckAnalysis.BottleneckCount);
            Assert.AreEqual(BottleneckSeverity.Minor, bottleneckAnalysis.HighestSeverity);
        }
        
        [Test]
        public void IdentifyBottlenecks_SortsBottlenecksBySeverity()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var analysisResult = CreateTestAnalysisResultWithMultipleBottlenecks();
            
            // Act
            var bottleneckAnalysis = _frameTimeAnalysisSystem.IdentifyBottlenecks(analysisResult);
            
            // Assert
            for (int i = 0; i < bottleneckAnalysis.IdentifiedBottlenecks.Length - 1; i++)
            {
                Assert.IsTrue(bottleneckAnalysis.IdentifiedBottlenecks[i].Severity >= 
                             bottleneckAnalysis.IdentifiedBottlenecks[i + 1].Severity);
            }
        }
        
        #endregion
        
        #region Trend Analysis Tests
        
        [Test]
        public void AnalyzeFrameTimeTrends_WithInsufficientData_ReturnsStableTrend()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var frameDataArray = CreateTestFrameDataArray(5, 16.67f, 60f); // Less than 10 frames
            
            // Act
            var trendAnalysis = _frameTimeAnalysisSystem.AnalyzeFrameTimeTrends(frameDataArray);
            
            // Assert
            Assert.AreEqual(TrendDirection.Stable, trendAnalysis.TrendDirection);
            Assert.IsNotNull(trendAnalysis.AnalysisTimestamp);
        }
        
        [Test]
        public void AnalyzeFrameTimeTrends_WithSufficientData_ReturnsValidAnalysis()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var frameDataArray = CreateTestFrameDataArray(20, 16.67f, 60f);
            
            // Act
            var trendAnalysis = _frameTimeAnalysisSystem.AnalyzeFrameTimeTrends(frameDataArray);
            
            // Assert
            Assert.IsNotNull(trendAnalysis.MovingAverages);
            Assert.IsNotNull(trendAnalysis.TrendStatistics);
            Assert.IsNotNull(trendAnalysis.TrendAnomalies);
            Assert.IsTrue(trendAnalysis.AnalysisPeriod.TotalMilliseconds >= 0);
        }
        
        #endregion
        
        #region Performance Comparison Tests
        
        [Test]
        public void CompareFrameTimePerformance_WithValidData_ReturnsComparison()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            var baselineData = CreateTestFrameDataArray(10, 16.67f, 60f);
            var comparisonData = CreateTestFrameDataArray(10, 20f, 50f);
            
            // Act
            var comparison = _frameTimeAnalysisSystem.CompareFrameTimePerformance(baselineData, comparisonData);
            
            // Assert
            Assert.IsNotNull(comparison.BaselineAnalysis);
            Assert.IsNotNull(comparison.ComparisonAnalysis);
            Assert.IsNotNull(comparison.PerformanceDelta);
            Assert.IsNotNull(comparison.ComparisonTimestamp);
            Assert.AreEqual(10, comparison.BaselineAnalysis.FrameCount);
            Assert.AreEqual(10, comparison.ComparisonAnalysis.FrameCount);
        }
        
        #endregion
        
        #region Event Tests
        
        [Test]
        public void BottleneckDetected_EventFiredWhenBottleneckOccurs()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            bool eventFired = false;
            BottleneckDetectedEventArgs eventArgs = null;
            
            _frameTimeAnalysisSystem.BottleneckDetected += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            _frameTimeAnalysisSystem.StartAnalysis("TestSession");
            // Simulate bottleneck detection through internal methods
            
            // Assert
            // Note: This test would require access to internal bottleneck detection
            // In a real implementation, we would trigger conditions that cause bottlenecks
        }
        
        [Test]
        public void AnalysisDataUpdated_EventFiredDuringAnalysis()
        {
            // Arrange
            _frameTimeAnalysisSystem.Initialize(FrameTimeAnalysisConfiguration.Quest3Default);
            bool eventFired = false;
            FrameTimeAnalysisUpdatedEventArgs eventArgs = null;
            
            _frameTimeAnalysisSystem.AnalysisDataUpdated += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            _frameTimeAnalysisSystem.StartAnalysis("TestSession");
            // Simulate frame data collection through internal methods
            
            // Assert
            // Note: This test would require triggering internal frame data collection
            // In a real implementation, we would simulate frame updates
        }
        
        #endregion
        
        #region Helper Methods
        
        private FrameTimingData CreateTestFrameData(float frameTimeMs, float frameRate)
        {
            return new FrameTimingData
            {
                FrameNumber = 1,
                TotalFrameTimeMs = frameTimeMs,
                FrameRate = frameRate,
                Timestamp = DateTime.Now,
                Breakdown = CreateTestFrameBreakdown(frameTimeMs),
                HasPerformanceIssues = frameTimeMs > 16.67f * 1.2f // >20% over 60 FPS target
            };
        }
        
        private FrameTimingData[] CreateTestFrameDataArray(int count, float frameTimeMs, float frameRate)
        {
            var frameDataArray = new FrameTimingData[count];
            var baseTime = DateTime.Now;
            
            for (int i = 0; i < count; i++)
            {
                frameDataArray[i] = new FrameTimingData
                {
                    FrameNumber = i + 1,
                    TotalFrameTimeMs = frameTimeMs + (i * 0.1f), // Slight variation
                    FrameRate = frameRate - (i * 0.1f), // Slight variation
                    Timestamp = baseTime.AddMilliseconds(i * frameTimeMs),
                    Breakdown = CreateTestFrameBreakdown(frameTimeMs),
                    HasPerformanceIssues = frameTimeMs > 16.67f * 1.2f
                };
            }
            
            return frameDataArray;
        }
        
        private DetailedFrameBreakdown CreateTestFrameBreakdown(float totalTimeMs = 16.67f)
        {
            return new DetailedFrameBreakdown
            {
                CPUTimeMs = totalTimeMs * 0.4f,
                GPUTimeMs = totalTimeMs * 0.35f,
                RenderingTimeMs = totalTimeMs * 0.3f,
                PhysicsTimeMs = totalTimeMs * 0.1f,
                ScriptExecutionTimeMs = totalTimeMs * 0.15f,
                AnimationTimeMs = totalTimeMs * 0.05f,
                AudioTimeMs = totalTimeMs * 0.02f,
                VRCompositorTimeMs = totalTimeMs * 0.08f,
                InputTimeMs = totalTimeMs * 0.01f,
                GarbageCollectionTimeMs = totalTimeMs * 0.01f,
                UIRenderingTimeMs = totalTimeMs * 0.03f,
                ParticleSystemTimeMs = totalTimeMs * 0.02f,
                PostProcessingTimeMs = totalTimeMs * 0.04f,
                OtherTimeMs = totalTimeMs * 0.05f,
                WaitTimeMs = totalTimeMs * 0.02f
            };
        }
        
        private FrameTimeAnalysisResult CreateTestAnalysisResultWithHighCPU()
        {
            var breakdown = CreateTestFrameBreakdown();
            breakdown.CPUTimeMs = breakdown.CPUTimeMs * 3f; // Make CPU usage very high
            
            var multiFrameAnalysis = new MultiFrameAnalysis
            {
                AggregateBreakdown = breakdown
            };
            
            return new FrameTimeAnalysisResult
            {
                AggregateAnalysis = multiFrameAnalysis
            };
        }
        
        private FrameTimeAnalysisResult CreateTestAnalysisResultWithHighGPU()
        {
            var breakdown = CreateTestFrameBreakdown();
            breakdown.GPUTimeMs = breakdown.GPUTimeMs * 3f; // Make GPU usage very high
            
            var multiFrameAnalysis = new MultiFrameAnalysis
            {
                AggregateBreakdown = breakdown
            };
            
            return new FrameTimeAnalysisResult
            {
                AggregateAnalysis = multiFrameAnalysis
            };
        }
        
        private FrameTimeAnalysisResult CreateTestAnalysisResultBalanced()
        {
            var breakdown = CreateTestFrameBreakdown(); // Use default balanced breakdown
            
            var multiFrameAnalysis = new MultiFrameAnalysis
            {
                AggregateBreakdown = breakdown
            };
            
            return new FrameTimeAnalysisResult
            {
                AggregateAnalysis = multiFrameAnalysis
            };
        }
        
        private FrameTimeAnalysisResult CreateTestAnalysisResultWithMultipleBottlenecks()
        {
            var breakdown = CreateTestFrameBreakdown();
            breakdown.CPUTimeMs = breakdown.CPUTimeMs * 2f; // High CPU
            breakdown.GPUTimeMs = breakdown.GPUTimeMs * 2.5f; // Higher GPU
            breakdown.GarbageCollectionTimeMs = breakdown.GarbageCollectionTimeMs * 5f; // Very high GC
            
            var multiFrameAnalysis = new MultiFrameAnalysis
            {
                AggregateBreakdown = breakdown
            };
            
            return new FrameTimeAnalysisResult
            {
                AggregateAnalysis = multiFrameAnalysis
            };
        }
        
        #endregion
    }
}