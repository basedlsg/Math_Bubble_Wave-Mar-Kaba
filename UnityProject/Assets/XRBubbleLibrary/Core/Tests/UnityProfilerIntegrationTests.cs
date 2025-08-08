using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Tests for UnityProfilerIntegration.
    /// Validates Requirement 4.3: Unity editor profiler integration for automated testing.
    /// Validates Requirement 4.4: Median FPS calculation and threshold checking.
    /// Validates Requirement 4.5: Performance data analysis and reporting.
    /// </summary>
    [TestFixture]
    public class UnityProfilerIntegrationTests
    {
        private UnityProfilerIntegration _profilerIntegration;
        private GameObject _testGameObject;
        
        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("TestUnityProfilerIntegration");
            _profilerIntegration = _testGameObject.AddComponent<UnityProfilerIntegration>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_testGameObject);
            }
        }
        
        [Test]
        public void Initialize_WithValidConfiguration_ReturnsTrue()
        {
            // Arrange
            var config = ProfilerConfiguration.Default;
            
            // Act
            var result = _profilerIntegration.Initialize(config);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_profilerIntegration.IsInitialized);
            Assert.AreEqual(config.SamplingFrequency, _profilerIntegration.Configuration.SamplingFrequency);
        }
        
        [Test]
        public void Initialize_WithInvalidConfiguration_ReturnsFalse()
        {
            // Arrange
            var config = new ProfilerConfiguration
            {
                SamplingFrequency = -1f, // Invalid
                EnableDeepProfiling = true
            };
            
            // Act
            var result = _profilerIntegration.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_profilerIntegration.IsInitialized);
        }
        
        [UnityTest]
        public IEnumerator StartProfilingSession_WithValidConfiguration_CreatesSession()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var sessionConfig = CreateValidSessionConfiguration();
            
            // Act
            var sessionTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            yield return new WaitUntil(() => sessionTask.IsCompleted);
            var session = sessionTask.Result;
            
            // Assert
            Assert.IsNotNull(session);
            Assert.IsNotEmpty(session.SessionId);
            Assert.AreEqual(ProfilingSessionStatus.Active, session.Status);
            Assert.AreEqual(sessionConfig.SessionName, session.Configuration.SessionName);
            Assert.IsTrue(_profilerIntegration.IsProfilingActive);
        }
        
        [Test]
        public void StartProfilingSession_WhenNotInitialized_ThrowsException()
        {
            // Arrange
            var sessionConfig = CreateValidSessionConfiguration();
            
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _profilerIntegration.StartProfilingSession(sessionConfig));
        }
        
        [Test]
        public void StartProfilingSession_WhenAlreadyActive_ThrowsException()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var sessionConfig = CreateValidSessionConfiguration();
            
            // Act
            var firstSessionTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            
            // Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _profilerIntegration.StartProfilingSession(sessionConfig));
        }
        
        [UnityTest]
        public IEnumerator StopProfilingSession_WithActiveSession_ReturnsResults()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var sessionConfig = CreateValidSessionConfiguration();
            
            var startTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            yield return new WaitUntil(() => startTask.IsCompleted);
            
            // Wait for some data collection
            yield return new WaitForSeconds(0.5f);
            
            // Act
            var stopTask = _profilerIntegration.StopProfilingSession();
            yield return new WaitUntil(() => stopTask.IsCompleted);
            var result = stopTask.Result;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.CompletedSuccessfully);
            Assert.AreEqual(startTask.Result.SessionId, result.SessionId);
            Assert.Greater(result.TotalFramesCaptured, 0);
            Assert.IsFalse(_profilerIntegration.IsProfilingActive);
        }
        
        [Test]
        public void StopProfilingSession_WithoutActiveSession_ThrowsException()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _profilerIntegration.StopProfilingSession());
        }
        
        [Test]
        public void GetRealTimeMetrics_WhenInitialized_ReturnsMetrics()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            // Act
            var metrics = _profilerIntegration.GetRealTimeMetrics();
            
            // Assert
            Assert.IsNotNull(metrics);
            Assert.Greater(metrics.CurrentFPS, 0f);
            Assert.Greater(metrics.CurrentFrameTimeMS, 0f);
            Assert.GreaterOrEqual(metrics.CurrentMemoryUsageMB, 0f);
            Assert.IsNotNull(metrics.CustomMetrics);
        }
        
        [Test]
        public void GetRealTimeMetrics_WhenNotInitialized_ReturnsEmptyMetrics()
        {
            // Act
            var metrics = _profilerIntegration.GetRealTimeMetrics();
            
            // Assert
            Assert.IsNotNull(metrics);
            Assert.AreEqual(0f, metrics.CurrentFPS);
            Assert.AreEqual(0f, metrics.CurrentFrameTimeMS);
        }
        
        [UnityTest]
        public IEnumerator CalculateMedianFPS_WithCompletedSession_ReturnsAccurateResult()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var sessionConfig = CreateValidSessionConfiguration();
            
            var startTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            yield return new WaitUntil(() => startTask.IsCompleted);
            var session = startTask.Result;
            
            // Wait for data collection
            yield return new WaitForSeconds(1f);
            
            var stopTask = _profilerIntegration.StopProfilingSession();
            yield return new WaitUntil(() => stopTask.IsCompleted);
            
            // Act
            var medianResult = _profilerIntegration.CalculateMedianFPS(session.SessionId);
            
            // Assert
            Assert.IsNotNull(medianResult);
            Assert.Greater(medianResult.MedianFPS, 0f);
            Assert.Greater(medianResult.AverageFPS, 0f);
            Assert.Greater(medianResult.DataPointCount, 0);
            Assert.LessOrEqual(medianResult.MinimumFPS, medianResult.MedianFPS);
            Assert.GreaterOrEqual(medianResult.MaximumFPS, medianResult.MedianFPS);
        }
        
        [Test]
        public void CalculateMedianFPS_WithInvalidSession_ThrowsException()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _profilerIntegration.CalculateMedianFPS("invalid-session-id"));
        }
        
        [Test]
        public void CheckPerformanceThresholds_WithGoodPerformance_PassesAllThresholds()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var metrics = CreateGoodPerformanceMetrics();
            
            // Act
            var result = _profilerIntegration.CheckPerformanceThresholds(metrics);
            
            // Assert
            Assert.IsTrue(result.AllThresholdsPassed);
            Assert.AreEqual(0, result.Violations.Length);
            Assert.Greater(result.PerformanceScore, 90f);
            Assert.AreEqual(metrics.CurrentFPS, result.CheckedMetrics.CurrentFPS);
        }
        
        [Test]
        public void CheckPerformanceThresholds_WithPoorPerformance_DetectsViolations()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var metrics = CreatePoorPerformanceMetrics();
            
            // Act
            var result = _profilerIntegration.CheckPerformanceThresholds(metrics);
            
            // Assert
            Assert.IsFalse(result.AllThresholdsPassed);
            Assert.Greater(result.Violations.Length, 0);
            Assert.Less(result.PerformanceScore, 50f);
            
            // Check for specific violations
            var fpsViolation = result.Violations.FirstOrDefault(v => v.ThresholdName == "MinimumFPS");
            Assert.IsNotNull(fpsViolation);
            Assert.AreEqual(ViolationSeverity.Error, fpsViolation.Severity);
        }
        
        [Test]
        public void CheckPerformanceThresholds_WithMixedPerformance_DetectsPartialViolations()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var metrics = CreateMixedPerformanceMetrics();
            
            // Act
            var result = _profilerIntegration.CheckPerformanceThresholds(metrics);
            
            // Assert
            Assert.IsFalse(result.AllThresholdsPassed);
            Assert.Greater(result.Violations.Length, 0);
            Assert.Greater(result.PerformanceScore, 50f);
            Assert.Less(result.PerformanceScore, 90f);
        }
        
        [UnityTest]
        public IEnumerator AnalyzePerformanceData_WithCompletedSession_ReturnsAnalysis()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var sessionConfig = CreateValidSessionConfiguration();
            
            var startTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            yield return new WaitUntil(() => startTask.IsCompleted);
            var session = startTask.Result;
            
            yield return new WaitForSeconds(1f);
            
            var stopTask = _profilerIntegration.StopProfilingSession();
            yield return new WaitUntil(() => stopTask.IsCompleted);
            
            // Act
            var analysisResult = _profilerIntegration.AnalyzePerformanceData(session.SessionId);
            
            // Assert
            Assert.IsNotNull(analysisResult);
            Assert.AreEqual(session.SessionId, analysisResult.SessionId);
            Assert.IsNotEmpty(analysisResult.AnalysisId);
            Assert.IsNotNull(analysisResult.TrendAnalysis);
            Assert.IsNotNull(analysisResult.BottleneckAnalysis);
            Assert.IsNotNull(analysisResult.MemoryAnalysis);
            Assert.IsNotNull(analysisResult.FrameTimeAnalysis);
            Assert.IsNotNull(analysisResult.KeyInsights);
            Assert.IsNotNull(analysisResult.OptimizationRecommendations);
            Assert.Greater(analysisResult.AnalysisConfidence, 0f);
        }
        
        [Test]
        public void AnalyzePerformanceData_WithInvalidSession_ThrowsException()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _profilerIntegration.AnalyzePerformanceData("invalid-session-id"));
        }
        
        [UnityTest]
        public IEnumerator GeneratePerformanceReport_WithCompletedSession_ReturnsReport()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var sessionConfig = CreateValidSessionConfiguration();
            
            var startTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            yield return new WaitUntil(() => startTask.IsCompleted);
            var session = startTask.Result;
            
            yield return new WaitForSeconds(0.5f);
            
            var stopTask = _profilerIntegration.StopProfilingSession();
            yield return new WaitUntil(() => stopTask.IsCompleted);
            
            // Act
            var report = _profilerIntegration.GeneratePerformanceReport(session.SessionId, ReportFormat.Markdown);
            
            // Assert
            Assert.IsNotNull(report);
            Assert.IsNotEmpty(report.ReportId);
            Assert.AreEqual(session.SessionId, report.SessionId);
            Assert.AreEqual(ReportFormat.Markdown, report.Format);
            Assert.IsNotEmpty(report.Content);
            Assert.IsNotEmpty(report.Summary);
            Assert.IsNotNull(report.Metadata);
        }
        
        [UnityTest]
        public IEnumerator ExportProfilerData_WithCompletedSession_ReturnsExport()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var sessionConfig = CreateValidSessionConfiguration();
            
            var startTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            yield return new WaitUntil(() => startTask.IsCompleted);
            var session = startTask.Result;
            
            yield return new WaitForSeconds(0.5f);
            
            var stopTask = _profilerIntegration.StopProfilingSession();
            yield return new WaitUntil(() => stopTask.IsCompleted);
            
            // Act
            var export = _profilerIntegration.ExportProfilerData(session.SessionId, DataExportFormat.CSV);
            
            // Assert
            Assert.IsNotNull(export);
            Assert.IsTrue(export.IsSuccessful);
            Assert.IsNotEmpty(export.ExportId);
            Assert.AreEqual(session.SessionId, export.SessionId);
            Assert.AreEqual(DataExportFormat.CSV, export.Format);
            Assert.IsNotEmpty(export.Content);
            Assert.Greater(export.DataPointCount, 0);
        }
        
        [Test]
        public void ExportProfilerData_WithInvalidSession_ReturnsFailedExport()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            // Act
            var export = _profilerIntegration.ExportProfilerData("invalid-session-id", DataExportFormat.JSON);
            
            // Assert
            Assert.IsNotNull(export);
            Assert.IsFalse(export.IsSuccessful);
            Assert.Contains("Session not found", export.ExportMessages);
        }
        
        [Test]
        public void ConfigurePerformanceThresholds_WithValidThresholds_UpdatesConfiguration()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var newThresholds = new PerformanceThresholds
            {
                MinimumFPS = 90f,
                MaximumFrameTimeMS = 11.11f,
                MaximumMemoryUsageMB = 256f,
                MaximumCPUUsagePercent = 70f,
                MaximumGPUUsagePercent = 75f,
                MaximumGCTimeMS = 1f
            };
            
            // Act
            _profilerIntegration.ConfigurePerformanceThresholds(newThresholds);
            
            // Test that new thresholds are applied
            var metrics = new PerformanceMetrics
            {
                CurrentFPS = 80f, // Below new threshold
                CurrentFrameTimeMS = 12.5f,
                CurrentMemoryUsageMB = 100f,
                CurrentCPUUsagePercent = 50f,
                CurrentGPUUsagePercent = 60f,
                CurrentGCTimeMS = 0.5f,
                Timestamp = DateTime.Now
            };
            
            var result = _profilerIntegration.CheckPerformanceThresholds(metrics);
            
            // Assert
            Assert.IsFalse(result.AllThresholdsPassed);
            var fpsViolation = result.Violations.FirstOrDefault(v => v.ThresholdName == "MinimumFPS");
            Assert.IsNotNull(fpsViolation);
            Assert.AreEqual(90f, fpsViolation.ThresholdValue);
        }
        
        [Test]
        public void GetHistoricalData_WithValidTimeRange_ReturnsHistoricalData()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var timeRange = new TimeRange
            {
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now
            };
            
            // Act
            var historicalData = _profilerIntegration.GetHistoricalData(timeRange);
            
            // Assert
            Assert.IsNotNull(historicalData);
            Assert.AreEqual(timeRange.StartTime, historicalData.TimeRange.StartTime);
            Assert.AreEqual(timeRange.EndTime, historicalData.TimeRange.EndTime);
            Assert.IsNotNull(historicalData.Sessions);
            Assert.IsNotNull(historicalData.OverallTrend);
            Assert.IsNotNull(historicalData.Statistics);
        }
        
        [Test]
        public void ValidateProfilerAccuracy_WhenInitialized_ReturnsValidationResult()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            // Act
            var validationResult = _profilerIntegration.ValidateProfilerAccuracy();
            
            // Assert
            Assert.IsNotNull(validationResult);
            Assert.Greater(validationResult.AccuracyScore, 0f);
            Assert.IsNotNull(validationResult.TestResults);
            Assert.Greater(validationResult.TestResults.Length, 0);
            Assert.IsNotNull(validationResult.ValidationMessages);
            Assert.IsNotNull(validationResult.ValidationRecommendations);
            
            // Check specific test results
            var profilerAvailabilityTest = validationResult.TestResults.FirstOrDefault(t => t.TestName == "ProfilerAvailability");
            Assert.IsNotNull(profilerAvailabilityTest);
            
            var dataCaptureTest = validationResult.TestResults.FirstOrDefault(t => t.TestName == "DataCaptureAccuracy");
            Assert.IsNotNull(dataCaptureTest);
            
            var thresholdTest = validationResult.TestResults.FirstOrDefault(t => t.TestName == "ThresholdChecking");
            Assert.IsNotNull(thresholdTest);
        }
        
        [Test]
        public void ResetProfilerIntegration_ClearsAllData()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            // Act
            _profilerIntegration.ResetProfilerIntegration();
            
            // Assert
            Assert.IsFalse(_profilerIntegration.IsProfilingActive);
            
            // Verify that historical data is cleared
            var timeRange = new TimeRange
            {
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now
            };
            var historicalData = _profilerIntegration.GetHistoricalData(timeRange);
            Assert.AreEqual(0, historicalData.Sessions.Length);
        }
        
        [UnityTest]
        public IEnumerator PerformanceThresholdViolated_Event_IsTriggeredCorrectly()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            bool eventFired = false;
            PerformanceThresholdViolatedEventArgs eventArgs = null;
            
            _profilerIntegration.PerformanceThresholdViolated += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            var sessionConfig = CreateValidSessionConfiguration();
            sessionConfig.EnableRealTimeThresholdChecking = true;
            
            var startTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            yield return new WaitUntil(() => startTask.IsCompleted);
            
            // Force a threshold violation by configuring very strict thresholds
            _profilerIntegration.ConfigurePerformanceThresholds(new PerformanceThresholds
            {
                MinimumFPS = 1000f, // Impossible to achieve
                MaximumFrameTimeMS = 0.1f,
                MaximumMemoryUsageMB = 1f,
                MaximumCPUUsagePercent = 1f,
                MaximumGPUUsagePercent = 1f,
                MaximumGCTimeMS = 0.01f
            });
            
            // Wait for threshold violation detection
            yield return new WaitForSeconds(1f);
            
            var stopTask = _profilerIntegration.StopProfilingSession();
            yield return new WaitUntil(() => stopTask.IsCompleted);
            
            // Assert
            Assert.IsTrue(eventFired, "PerformanceThresholdViolated event should be fired");
            Assert.IsNotNull(eventArgs);
            Assert.IsNotEmpty(eventArgs.SessionId);
            Assert.Greater(eventArgs.Violations.Length, 0);
        }
        
        [UnityTest]
        public IEnumerator ProfilingSessionEvents_AreTriggeredCorrectly()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            bool sessionStartedFired = false;
            bool sessionCompletedFired = false;
            
            _profilerIntegration.ProfilingSessionStarted += (args) => sessionStartedFired = true;
            _profilerIntegration.ProfilingSessionCompleted += (args) => sessionCompletedFired = true;
            
            var sessionConfig = CreateValidSessionConfiguration();
            
            // Act
            var startTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            yield return new WaitUntil(() => startTask.IsCompleted);
            
            yield return new WaitForSeconds(0.5f);
            
            var stopTask = _profilerIntegration.StopProfilingSession();
            yield return new WaitUntil(() => stopTask.IsCompleted);
            
            // Assert
            Assert.IsTrue(sessionStartedFired, "ProfilingSessionStarted event should be fired");
            Assert.IsTrue(sessionCompletedFired, "ProfilingSessionCompleted event should be fired");
        }
        
        [UnityTest]
        public IEnumerator SessionTimeout_AutomaticallyStopsSession()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            var sessionConfig = CreateValidSessionConfiguration();
            sessionConfig.DurationSeconds = 0.5f; // Very short duration
            
            // Act
            var startTask = _profilerIntegration.StartProfilingSession(sessionConfig);
            yield return new WaitUntil(() => startTask.IsCompleted);
            
            Assert.IsTrue(_profilerIntegration.IsProfilingActive);
            
            // Wait for timeout
            yield return new WaitForSeconds(1f);
            
            // Assert
            Assert.IsFalse(_profilerIntegration.IsProfilingActive);
        }
        
        [Test]
        public void MedianFPSCalculation_WithVariousDataSets_ReturnsAccurateResults()
        {
            // Test with odd number of values
            var oddValues = new float[] { 30f, 60f, 90f };
            var oddMedian = CalculateExpectedMedian(oddValues);
            
            // Test with even number of values
            var evenValues = new float[] { 30f, 45f, 60f, 75f };
            var evenMedian = CalculateExpectedMedian(evenValues);
            
            // Test with single value
            var singleValue = new float[] { 60f };
            var singleMedian = CalculateExpectedMedian(singleValue);
            
            // Assert expected behavior
            Assert.AreEqual(60f, oddMedian);
            Assert.AreEqual(52.5f, evenMedian);
            Assert.AreEqual(60f, singleMedian);
        }
        
        [Test]
        public void PerformanceScoreCalculation_WithDifferentViolations_ReturnsCorrectScores()
        {
            // Arrange
            _profilerIntegration.Initialize(ProfilerConfiguration.Default);
            
            // Test with no violations
            var goodMetrics = CreateGoodPerformanceMetrics();
            var goodResult = _profilerIntegration.CheckPerformanceThresholds(goodMetrics);
            
            // Test with minor violations
            var minorMetrics = CreateMixedPerformanceMetrics();
            var minorResult = _profilerIntegration.CheckPerformanceThresholds(minorMetrics);
            
            // Test with major violations
            var poorMetrics = CreatePoorPerformanceMetrics();
            var poorResult = _profilerIntegration.CheckPerformanceThresholds(poorMetrics);
            
            // Assert
            Assert.Greater(goodResult.PerformanceScore, minorResult.PerformanceScore);
            Assert.Greater(minorResult.PerformanceScore, poorResult.PerformanceScore);
            Assert.AreEqual(100f, goodResult.PerformanceScore);
            Assert.Greater(poorResult.PerformanceScore, 0f);
        }
        
        // Helper methods for creating test data
        
        private ProfilingSessionConfiguration CreateValidSessionConfiguration()
        {
            return new ProfilingSessionConfiguration
            {
                SessionName = "Test Session",
                DurationSeconds = 5f,
                TargetFrameRate = 60f,
                EnableRealTimeThresholdChecking = false,
                CaptureDetailedFrameData = true,
                CustomThresholds = null,
                SessionNotes = "Test session notes"
            };
        }
        
        private PerformanceMetrics CreateGoodPerformanceMetrics()
        {
            return new PerformanceMetrics
            {
                CurrentFPS = 90f,
                CurrentFrameTimeMS = 11.11f,
                CurrentMemoryUsageMB = 200f,
                CurrentCPUUsagePercent = 50f,
                CurrentGPUUsagePercent = 60f,
                CurrentGCTimeMS = 1f,
                DrawCalls = 100,
                Triangles = 50000,
                Vertices = 25000,
                Timestamp = DateTime.Now,
                CustomMetrics = new Dictionary<string, float>()
            };
        }
        
        private PerformanceMetrics CreatePoorPerformanceMetrics()
        {
            return new PerformanceMetrics
            {
                CurrentFPS = 30f, // Below threshold
                CurrentFrameTimeMS = 33.33f, // Above threshold
                CurrentMemoryUsageMB = 600f, // Above threshold
                CurrentCPUUsagePercent = 90f, // Above threshold
                CurrentGPUUsagePercent = 95f, // Above threshold
                CurrentGCTimeMS = 5f, // Above threshold
                DrawCalls = 500,
                Triangles = 200000,
                Vertices = 100000,
                Timestamp = DateTime.Now,
                CustomMetrics = new Dictionary<string, float>()
            };
        }
        
        private PerformanceMetrics CreateMixedPerformanceMetrics()
        {
            return new PerformanceMetrics
            {
                CurrentFPS = 75f, // Good
                CurrentFrameTimeMS = 13.33f, // Good
                CurrentMemoryUsageMB = 550f, // Slightly above threshold
                CurrentCPUUsagePercent = 85f, // Slightly above threshold
                CurrentGPUUsagePercent = 70f, // Good
                CurrentGCTimeMS = 1.5f, // Good
                DrawCalls = 200,
                Triangles = 100000,
                Vertices = 50000,
                Timestamp = DateTime.Now,
                CustomMetrics = new Dictionary<string, float>()
            };
        }
        
        private float CalculateExpectedMedian(float[] values)
        {
            var sorted = values.OrderBy(v => v).ToArray();
            var mid = sorted.Length / 2;
            
            if (sorted.Length % 2 == 0)
                return (sorted[mid - 1] + sorted[mid]) / 2f;
            else
                return sorted[mid];
        }
    }
}