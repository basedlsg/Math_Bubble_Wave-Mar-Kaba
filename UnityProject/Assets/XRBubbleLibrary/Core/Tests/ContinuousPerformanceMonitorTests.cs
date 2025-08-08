using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace XRBubbleLibrary.Core.Tests
{
    [TestFixture]
    public class ContinuousPerformanceMonitorTests
    {
        private ContinuousPerformanceMonitor _monitor;
        private PerformanceMonitorConfiguration _testConfig;
        private MockPerformanceThresholdManager _mockThresholdManager;
        
        [SetUp]
        public void SetUp()
        {
            _monitor = new ContinuousPerformanceMonitor();
            
            _testConfig = new PerformanceMonitorConfiguration
            {
                EnableContinuousMonitoring = true,
                EnableThresholdViolationDetection = true,
                EnableTrendAnalysis = true,
                DataCollectionIntervalSeconds = 0.1f, // Fast for testing
                HighFrequencyIntervalSeconds = 0.05f,
                StatisticsIntervalSeconds = 1f,
                MaxDataPointsPerSession = 1000,
                DataRetentionHours = 1f,
                EnableAutomaticDataCleanup = true,
                MonitoredMetrics = new[]
                {
                    PerformanceMetricType.FPS,
                    PerformanceMetricType.FrameTime,
                    PerformanceMetricType.MemoryUsage,
                    PerformanceMetricType.CPUUsage
                },
                EnableRealTimeAlerts = true,
                AlertEscalationDelaySeconds = 5f,
                MaxAlertsPerMinute = 5,
                EnableHighPrecisionTiming = true,
                Priority = MonitoringPriority.High
            };
            
            _mockThresholdManager = new MockPerformanceThresholdManager();
        }
        
        [TearDown]
        public void TearDown()
        {
            _monitor?.ResetMonitoringSystem();
            _monitor?.Dispose();
        }
        
        [Test]
        public void Initialize_WithValidParameters_ReturnsTrue()
        {
            // Act
            var result = _monitor.Initialize(_testConfig, _mockThresholdManager);
            
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(_testConfig, _monitor.Configuration);
        }
        
        [Test]
        public void Initialize_WithNullConfig_ReturnsFalse()
        {
            // Act
            var result = _monitor.Initialize(null, _mockThresholdManager);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void Initialize_WithNullThresholdManager_ReturnsFalse()
        {
            // Act
            var result = _monitor.Initialize(_testConfig, null);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void StartMonitoring_WithValidSession_ReturnsSession()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            
            // Act
            var session = _monitor.StartMonitoring(sessionId);
            
            // Assert
            Assert.IsNotNull(session);
            Assert.AreEqual(sessionId, session.SessionId);
            Assert.AreEqual(MonitoringSessionStatus.Active, session.Status);
            Assert.IsTrue(_monitor.IsMonitoring);
        }
        
        [Test]
        public void StartMonitoring_WithoutInitialization_ReturnsNull()
        {
            // Act
            var session = _monitor.StartMonitoring("test-session");
            
            // Assert
            Assert.IsNull(session);
        }
        
        [Test]
        public void StartMonitoring_WithEmptySessionId_ReturnsNull()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            
            // Act
            var session = _monitor.StartMonitoring("");
            
            // Assert
            Assert.IsNull(session);
        }
        
        [Test]
        public void StartMonitoring_WithDuplicateSessionId_ReturnsExistingSession()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            
            // Act
            var session1 = _monitor.StartMonitoring(sessionId);
            var session2 = _monitor.StartMonitoring(sessionId);
            
            // Assert
            Assert.IsNotNull(session1);
            Assert.IsNotNull(session2);
            Assert.AreEqual(session1.SessionId, session2.SessionId);
        }
        
        [Test]
        public void StopMonitoring_WithActiveSession_ReturnsSuccess()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            _monitor.StartMonitoring(sessionId);
            
            // Act
            var result = _monitor.StopMonitoring(sessionId);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotNull(result.FinalStatistics);
            Assert.IsTrue(result.SessionDuration.TotalSeconds > 0);
            Assert.IsFalse(_monitor.IsMonitoring);
        }
        
        [Test]
        public void StopMonitoring_WithInvalidSession_ReturnsFailure()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            
            // Act
            var result = _monitor.StopMonitoring("invalid-session");
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.TerminationMessages.Any(m => m.Contains("Session not found")));
        }
        
        [Test]
        public void CollectCurrentMetrics_ReturnsValidMetrics()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            
            // Act
            var metrics = _monitor.CollectCurrentMetrics();
            
            // Assert
            Assert.IsNotNull(metrics);
            Assert.IsTrue(metrics.AverageFPS > 0);
            Assert.IsTrue(metrics.AverageFrameTime > 0);
            Assert.IsTrue(metrics.MemoryUsage > 0);
            Assert.IsNotNull(metrics.AdditionalMetrics);
        }
        
        [Test]
        public void GetPerformanceHistory_WithActiveSession_ReturnsHistory()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            var session = _monitor.StartMonitoring(sessionId);
            
            // Simulate some data collection
            for (int i = 0; i < 5; i++)
            {
                var metrics = _monitor.CollectCurrentMetrics();
                session.PerformanceHistory.Add(metrics);
                Thread.Sleep(10); // Small delay to ensure different timestamps
            }
            
            // Act
            var history = _monitor.GetPerformanceHistory(sessionId, TimeSpan.FromMinutes(1));
            
            // Assert
            Assert.IsNotNull(history);
            Assert.AreEqual(5, history.Length);
        }
        
        [Test]
        public void GetPerformanceHistory_WithInvalidSession_ReturnsEmpty()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            
            // Act
            var history = _monitor.GetPerformanceHistory("invalid-session", TimeSpan.FromMinutes(1));
            
            // Assert
            Assert.IsNotNull(history);
            Assert.AreEqual(0, history.Length);
        }
        
        [Test]
        public void GetPerformanceStatistics_WithActiveSession_ReturnsStatistics()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            var session = _monitor.StartMonitoring(sessionId);
            
            // Add some test data
            for (int i = 0; i < 10; i++)
            {
                session.PerformanceHistory.Add(new PerformanceMetrics
                {
                    AverageFPS = 60f + i,
                    AverageFrameTime = 16.67f,
                    MemoryUsage = 100000000 + (i * 1000000),
                    CPUUsage = 50f + i,
                    GPUUsage = 60f + i,
                    ThermalState = 0.5f,
                    CapturedAt = DateTime.UtcNow.AddSeconds(-i)
                });
            }
            
            // Act
            var statistics = _monitor.GetPerformanceStatistics(sessionId);
            
            // Assert
            Assert.IsNotNull(statistics);
            Assert.AreEqual(10, statistics.DataPointsAnalyzed);
            Assert.IsNotNull(statistics.AverageMetrics);
            Assert.IsNotNull(statistics.MinimumMetrics);
            Assert.IsNotNull(statistics.MaximumMetrics);
            Assert.IsTrue(statistics.StabilityScore >= 0 && statistics.StabilityScore <= 1);
            Assert.IsTrue(statistics.OverallHealthScore >= 0 && statistics.OverallHealthScore <= 1);
        }
        
        [Test]
        public void UpdateConfiguration_WithValidConfig_UpdatesSuccessfully()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            _monitor.StartMonitoring(sessionId);
            
            var newConfig = new PerformanceMonitorConfiguration
            {
                EnableContinuousMonitoring = true,
                DataCollectionIntervalSeconds = 2f,
                MaxDataPointsPerSession = 500
            };
            
            // Act
            _monitor.UpdateConfiguration(newConfig);
            
            // Assert
            Assert.AreEqual(newConfig, _monitor.Configuration);
        }
        
        [Test]
        public void UpdateConfiguration_WithNullConfig_DoesNotUpdate()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var originalConfig = _monitor.Configuration;
            
            // Act
            _monitor.UpdateConfiguration(null);
            
            // Assert
            Assert.AreEqual(originalConfig, _monitor.Configuration);
        }
        
        [Test]
        public void RegisterCustomMetricCollector_WithValidCollector_RegistersSuccessfully()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var collector = new MockPerformanceMetricCollector("test-collector", "Test Collector");
            
            // Act
            _monitor.RegisterCustomMetricCollector(collector);
            
            // Assert
            Assert.IsTrue(collector.IsInitialized);
        }
        
        [Test]
        public void RegisterCustomMetricCollector_WithNullCollector_DoesNotRegister()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            
            // Act & Assert (should not throw)
            _monitor.RegisterCustomMetricCollector(null);
        }
        
        [Test]
        public void UnregisterCustomMetricCollector_WithValidCollector_UnregistersSuccessfully()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var collector = new MockPerformanceMetricCollector("test-collector", "Test Collector");
            _monitor.RegisterCustomMetricCollector(collector);
            
            // Act
            _monitor.UnregisterCustomMetricCollector("test-collector");
            
            // Assert
            Assert.IsTrue(collector.IsCleanedUp);
        }
        
        [Test]
        public void ForceDataCollection_WithActiveSession_ReturnsMetrics()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            _monitor.StartMonitoring(sessionId);
            
            // Act
            var metrics = _monitor.ForceDataCollection(sessionId);
            
            // Assert
            Assert.IsNotNull(metrics);
            Assert.IsTrue(metrics.AverageFPS > 0);
        }
        
        [Test]
        public void AnalyzePerformanceTrends_WithSufficientData_ReturnsAnalysis()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            var session = _monitor.StartMonitoring(sessionId);
            
            // Add trend data (declining FPS)
            for (int i = 0; i < 20; i++)
            {
                session.PerformanceHistory.Add(new PerformanceMetrics
                {
                    AverageFPS = 80f - (i * 2f), // Declining trend
                    AverageFrameTime = 12.5f + (i * 0.5f),
                    MemoryUsage = 100000000,
                    CPUUsage = 50f,
                    GPUUsage = 60f,
                    ThermalState = 0.5f,
                    CapturedAt = DateTime.UtcNow.AddSeconds(-i)
                });
            }
            
            // Act
            var analysis = _monitor.AnalyzePerformanceTrends(sessionId, TimeSpan.FromMinutes(1));
            
            // Assert
            Assert.IsNotNull(analysis);
            Assert.IsNotNull(analysis.MetricTrends);
            Assert.IsTrue(analysis.MetricTrends.ContainsKey(PerformanceMetricType.FPS));
            Assert.IsTrue(analysis.AnalysisConfidence > 0);
            Assert.IsNotNull(analysis.Recommendations);
        }
        
        [Test]
        public void AnalyzePerformanceTrends_WithInsufficientData_ReturnsLowConfidenceAnalysis()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            _monitor.StartMonitoring(sessionId);
            
            // Act
            var analysis = _monitor.AnalyzePerformanceTrends(sessionId, TimeSpan.FromMinutes(1));
            
            // Assert
            Assert.IsNotNull(analysis);
            Assert.AreEqual(TrendDirection.Stable, analysis.OverallTrend);
            Assert.AreEqual(0f, analysis.TrendStrength);
            Assert.IsTrue(analysis.AnalysisConfidence < 0.5f);
        }
        
        [Test]
        public void GenerateMonitoringReport_WithActiveSession_ReturnsReport()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            var session = _monitor.StartMonitoring(sessionId);
            
            // Add some test data
            for (int i = 0; i < 5; i++)
            {
                session.PerformanceHistory.Add(new PerformanceMetrics
                {
                    AverageFPS = 60f + i,
                    AverageFrameTime = 16.67f,
                    MemoryUsage = 100000000,
                    CPUUsage = 50f,
                    GPUUsage = 60f,
                    ThermalState = 0.5f,
                    CapturedAt = DateTime.UtcNow.AddSeconds(-i)
                });
            }
            
            // Act
            var report = _monitor.GenerateMonitoringReport(sessionId);
            
            // Assert
            Assert.IsNotNull(report);
            Assert.AreEqual(sessionId, report.SessionId);
            Assert.IsNotNull(report.Statistics);
            Assert.IsNotNull(report.TrendAnalysis);
            Assert.IsNotNull(report.ViolationSummary);
            Assert.IsNotNull(report.KeyInsights);
            Assert.IsNotNull(report.Recommendations);
            Assert.IsTrue(report.ReportQuality > 0);
        }
        
        [Test]
        public void ExportPerformanceData_WithValidSession_ReturnsExport()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            var session = _monitor.StartMonitoring(sessionId);
            
            // Add test data
            session.PerformanceHistory.Add(new PerformanceMetrics
            {
                AverageFPS = 60f,
                AverageFrameTime = 16.67f,
                MemoryUsage = 100000000,
                CapturedAt = DateTime.UtcNow
            });
            
            // Act
            var export = _monitor.ExportPerformanceData(sessionId, DataExportFormat.JSON, TimeSpan.FromMinutes(1));
            
            // Assert
            Assert.IsNotNull(export);
            Assert.AreEqual(DataExportFormat.JSON, export.Format);
            Assert.AreEqual(1, export.DataPointsExported);
            Assert.IsNotNull(export.ExportData);
            Assert.IsTrue(export.FileSizeBytes > 0);
            Assert.IsNotNull(export.Quality);
        }
        
        [Test]
        public void PauseMonitoring_WithActiveSession_ReturnsTrue()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            _monitor.StartMonitoring(sessionId);
            
            // Act
            var result = _monitor.PauseMonitoring(sessionId);
            
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(MonitoringSessionStatus.Paused, _monitor.ActiveSessions.First().Status);
        }
        
        [Test]
        public void ResumeMonitoring_WithPausedSession_ReturnsTrue()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            _monitor.StartMonitoring(sessionId);
            _monitor.PauseMonitoring(sessionId);
            
            // Act
            var result = _monitor.ResumeMonitoring(sessionId);
            
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(MonitoringSessionStatus.Active, _monitor.ActiveSessions.First().Status);
        }
        
        [Test]
        public void GetDashboardData_WithActiveSession_ReturnsDashboardData()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            var session = _monitor.StartMonitoring(sessionId);
            
            // Add some recent data
            for (int i = 0; i < 3; i++)
            {
                session.PerformanceHistory.Add(new PerformanceMetrics
                {
                    AverageFPS = 60f + i,
                    AverageFrameTime = 16.67f,
                    MemoryUsage = 100000000,
                    CapturedAt = DateTime.UtcNow.AddSeconds(-i)
                });
            }
            
            // Act
            var dashboard = _monitor.GetDashboardData(sessionId);
            
            // Assert
            Assert.IsNotNull(dashboard);
            Assert.IsNotNull(dashboard.CurrentMetrics);
            Assert.IsNotNull(dashboard.RecentHistory);
            Assert.IsNotNull(dashboard.ActiveViolations);
            Assert.IsNotNull(dashboard.HealthIndicators);
            Assert.IsNotNull(dashboard.ActiveAlerts);
            Assert.IsNotNull(dashboard.ShortTermTrends);
            Assert.IsNotNull(dashboard.SystemStatus);
        }
        
        [Test]
        public void ResetMonitoringSystem_ClearsAllSessions()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            _monitor.StartMonitoring("session-1");
            _monitor.StartMonitoring("session-2");
            
            // Act
            _monitor.ResetMonitoringSystem();
            
            // Assert
            Assert.IsFalse(_monitor.IsMonitoring);
            Assert.AreEqual(0, _monitor.ActiveSessions.Length);
        }
        
        [Test]
        public void EventHandling_PerformanceDataCollected_FiresEvent()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            _monitor.StartMonitoring(sessionId);
            
            bool eventFired = false;
            PerformanceDataCollectedEventArgs eventArgs = null;
            
            _monitor.PerformanceDataCollected += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            _monitor.ForceDataCollection(sessionId);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(sessionId, eventArgs.SessionId);
        }
        
        [Test]
        public void EventHandling_MonitoringSessionStarted_FiresEvent()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            
            bool eventFired = false;
            MonitoringSessionStartedEventArgs eventArgs = null;
            
            _monitor.MonitoringSessionStarted += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            var sessionId = "test-session-1";
            _monitor.StartMonitoring(sessionId);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(sessionId, eventArgs.SessionId);
        }
        
        [Test]
        public void EventHandling_MonitoringSessionEnded_FiresEvent()
        {
            // Arrange
            _monitor.Initialize(_testConfig, _mockThresholdManager);
            var sessionId = "test-session-1";
            _monitor.StartMonitoring(sessionId);
            
            bool eventFired = false;
            MonitoringSessionEndedEventArgs eventArgs = null;
            
            _monitor.MonitoringSessionEnded += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            _monitor.StopMonitoring(sessionId);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(sessionId, eventArgs.SessionId);
        }
    }
    
    #region Mock Classes
    
    /// <summary>
    /// Mock implementation of IPerformanceThresholdManager for testing.
    /// </summary>
    public class MockPerformanceThresholdManager : IPerformanceThresholdManager
    {
        public PerformanceThresholds CurrentThresholds { get; private set; } = new PerformanceThresholds();
        
        public void UpdateThresholds(PerformanceThresholds thresholds)
        {
            CurrentThresholds = thresholds;
        }
        
        public ThresholdValidationResult ValidatePerformance(PerformanceMetrics metrics)
        {
            return new ThresholdValidationResult
            {
                IsValid = true,
                Violations = new ThresholdViolation[0],
                OverallHealthScore = 0.9f,
                Summary = "All metrics within thresholds",
                ValidatedAt = DateTime.UtcNow
            };
        }
        
        public bool IsThresholdViolated(PerformanceMetricType metricType, float value)
        {
            return false; // No violations in mock
        }
        
        public List<ThresholdViolation> GetViolationHistory(TimeSpan period)
        {
            return new List<ThresholdViolation>();
        }
        
        public void RegisterViolationCallback(Action<ThresholdViolation> callback)
        {
            // Mock implementation - store callback if needed
        }
        
        public ThresholdRecommendations GetRecommendedAdjustments()
        {
            return new ThresholdRecommendations
            {
                RecommendedThresholds = new Dictionary<PerformanceMetricType, float>(),
                ReasoningReport = "Mock recommendations",
                ConfidenceScore = 0.8f,
                GeneratedAt = DateTime.UtcNow,
                DataPointsAnalyzed = 100
            };
        }
    }
    
    /// <summary>
    /// Mock implementation of IPerformanceMetricCollector for testing.
    /// </summary>
    public class MockPerformanceMetricCollector : IPerformanceMetricCollector
    {
        public string CollectorId { get; }
        public string CollectorName { get; }
        public bool IsEnabled { get; set; } = true;
        public bool IsInitialized { get; private set; }
        public bool IsCleanedUp { get; private set; }
        
        public MockPerformanceMetricCollector(string id, string name)
        {
            CollectorId = id;
            CollectorName = name;
        }
        
        public Dictionary<string, float> CollectMetrics()
        {
            return new Dictionary<string, float>
            {
                ["CustomMetric1"] = 42.0f,
                ["CustomMetric2"] = 84.0f
            };
        }
        
        public bool Initialize()
        {
            IsInitialized = true;
            return true;
        }
        
        public void Cleanup()
        {
            IsCleanedUp = true;
        }
    }
    
    #endregion
}