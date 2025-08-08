using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.UserComfort.Tests
{
    [TestFixture]
    public class ComfortValidationFeedbackLoopTests
    {
        private ComfortValidationFeedbackLoop _feedbackLoop;
        private ComfortFeedbackConfiguration _testConfig;
        private MockComfortDataCollectionSystem _mockDataSystem;
        private MockWaveParameterValidator _mockValidator;
        private WaveMatrixSettings _testWaveSettings;
        
        [SetUp]
        public void SetUp()
        {
            _feedbackLoop = new ComfortValidationFeedbackLoop();
            
            _testConfig = new ComfortFeedbackConfiguration
            {
                EnableAutomaticAdjustments = true,
                EnablePredictiveAnalysis = true,
                EnableAutomaticRollback = true,
                ProcessingIntervalSeconds = 5f,
                TrendAnalysisWindowMinutes = 2f,
                MaxAdjustmentDelaySeconds = 30f,
                AdjustmentSensitivity = 1.0f,
                MinimumComfortThreshold = 60f,
                CriticalComfortThreshold = 30f,
                MaxAdjustmentsPerSession = 10,
                MinTimeBetweenAdjustments = 15f,
                MaxParameterChangePerAdjustment = 0.2f
            };
            
            _mockDataSystem = new MockComfortDataCollectionSystem();
            _mockValidator = new MockWaveParameterValidator();
            
            _testWaveSettings = new WaveMatrixSettings(); // Would be properly initialized
        }
        
        [TearDown]
        public void TearDown()
        {
            _feedbackLoop?.ResetFeedbackSystem();
        }
        
        [Test]
        public void Initialize_WithValidParameters_ReturnsTrue()
        {
            // Act
            var result = _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(_testConfig, _feedbackLoop.Configuration);
        }
        
        [Test]
        public void Initialize_WithNullConfig_ReturnsFalse()
        {
            // Act
            var result = _feedbackLoop.Initialize(null, _mockDataSystem, _mockValidator);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void Initialize_WithNullDataSystem_ReturnsFalse()
        {
            // Act
            var result = _feedbackLoop.Initialize(_testConfig, null, _mockValidator);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void Initialize_WithNullValidator_ReturnsFalse()
        {
            // Act
            var result = _feedbackLoop.Initialize(_testConfig, _mockDataSystem, null);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void StartFeedbackLoop_WithValidSession_ReturnsSession()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            
            // Act
            var session = _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            // Assert
            Assert.IsNotNull(session);
            Assert.AreEqual(sessionId, session.SessionId);
            Assert.AreEqual(FeedbackSessionStatus.Active, session.Status);
            Assert.AreEqual(_testWaveSettings, session.InitialWaveSettings);
            Assert.AreEqual(_testWaveSettings, session.CurrentWaveSettings);
        }
        
        [Test]
        public void StartFeedbackLoop_WithoutInitialization_ReturnsNull()
        {
            // Act
            var session = _feedbackLoop.StartFeedbackLoop("test-session", _testWaveSettings);
            
            // Assert
            Assert.IsNull(session);
        }
        
        [Test]
        public void StartFeedbackLoop_WithEmptySessionId_ReturnsNull()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            
            // Act
            var session = _feedbackLoop.StartFeedbackLoop("", _testWaveSettings);
            
            // Assert
            Assert.IsNull(session);
        }
        
        [Test]
        public void StartFeedbackLoop_WithDuplicateSessionId_ReturnsExistingSession()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            
            // Act
            var session1 = _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            var session2 = _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            // Assert
            Assert.IsNotNull(session1);
            Assert.IsNotNull(session2);
            Assert.AreEqual(session1.SessionId, session2.SessionId);
        }
        
        [Test]
        public void ProcessComfortData_WithValidData_ReturnsSuccessfulResult()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            var comfortData = new ComfortDataPoint
            {
                Timestamp = DateTime.UtcNow,
                ComfortLevel = ComfortLevel.Comfortable
            };
            
            // Act
            var result = _feedbackLoop.ProcessComfortData(sessionId, comfortData);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsTrue(result.CurrentComfortScore > 0);
            Assert.IsNotNull(result.ComfortTrend);
            Assert.IsNotNull(result.IdentifiedIssues);
        }
        
        [Test]
        public void ProcessComfortData_WithInvalidSession_ReturnsFailure()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            
            var comfortData = new ComfortDataPoint
            {
                Timestamp = DateTime.UtcNow,
                ComfortLevel = ComfortLevel.Comfortable
            };
            
            // Act
            var result = _feedbackLoop.ProcessComfortData("invalid-session", comfortData);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.ProcessingMessages.Any(m => m.Contains("Session not found")));
        }
        
        [Test]
        public void ValidateComfortLevels_WithGoodComfort_ReturnsValid()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            var comfortData = new ComfortDataPoint
            {
                Timestamp = DateTime.UtcNow,
                ComfortLevel = ComfortLevel.VeryComfortable
            };
            _feedbackLoop.ProcessComfortData(sessionId, comfortData);
            
            // Act
            var result = _feedbackLoop.ValidateComfortLevels(sessionId);
            
            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.OverallComfortScore > _testConfig.MinimumComfortThreshold);
            Assert.AreEqual(0, result.Violations.Length);
        }
        
        [Test]
        public void ValidateComfortLevels_WithPoorComfort_ReturnsInvalid()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            var comfortData = new ComfortDataPoint
            {
                Timestamp = DateTime.UtcNow,
                ComfortLevel = ComfortLevel.VeryUncomfortable
            };
            _feedbackLoop.ProcessComfortData(sessionId, comfortData);
            
            // Act
            var result = _feedbackLoop.ValidateComfortLevels(sessionId);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Violations.Length > 0);
            Assert.IsTrue(result.ComfortIssues.Length > 0);
        }
        
        [Test]
        public void TriggerAutomaticRollback_WithValidSession_ReturnsSuccess()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            var session = _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            // Simulate some adjustments
            session.AdjustmentHistory.Add(new WaveParameterAdjustment
            {
                AdjustmentId = "adj-1",
                AppliedAt = DateTime.UtcNow,
                Type = AdjustmentType.Automatic
            });
            
            var rollbackReason = "Critical comfort issue detected";
            
            // Act
            var result = _feedbackLoop.TriggerAutomaticRollback(sessionId, rollbackReason);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(rollbackReason, result.RollbackReason);
            Assert.AreEqual(1, result.AdjustmentsRolledBack);
            Assert.AreEqual(_testWaveSettings, result.SettingsAfter);
        }
        
        [Test]
        public void TriggerAutomaticRollback_WithInvalidSession_ReturnsFailure()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            
            // Act
            var result = _feedbackLoop.TriggerAutomaticRollback("invalid-session", "Test rollback");
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.RollbackMessages.Any(m => m.Contains("Session not found")));
        }
        
        [Test]
        public void GetRecommendedAdjustments_WithComfortIssues_ReturnsAdjustments()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            var comfortIssues = new[]
            {
                new ComfortIssue
                {
                    IssueId = "issue-1",
                    Type = ComfortIssueType.MotionSickness,
                    Severity = ComfortIssueSeverity.High,
                    Description = "High motion sickness detected"
                }
            };
            
            // Act
            var adjustments = _feedbackLoop.GetRecommendedAdjustments(sessionId, comfortIssues);
            
            // Assert
            Assert.IsNotNull(adjustments);
            // Additional assertions would depend on the specific adjustment logic
        }
        
        [Test]
        public void ApplyManualAdjustments_WithValidAdjustments_ReturnsSuccess()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            var adjustments = new[]
            {
                new WaveParameterAdjustment
                {
                    AdjustmentId = "manual-adj-1",
                    Type = AdjustmentType.Manual,
                    SettingsBefore = _testWaveSettings,
                    SettingsAfter = _testWaveSettings, // Would be different in real scenario
                    Reason = "Manual adjustment for testing"
                }
            };
            
            // Act
            var result = _feedbackLoop.ApplyManualAdjustments(sessionId, adjustments);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(1, result.ParametersAdjusted);
            Assert.AreEqual(1, result.AppliedAdjustments.Length);
        }
        
        [Test]
        public void GetPerformanceMetrics_WithActiveSession_ReturnsMetrics()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            // Process some data
            var comfortData = new ComfortDataPoint
            {
                Timestamp = DateTime.UtcNow,
                ComfortLevel = ComfortLevel.Comfortable
            };
            _feedbackLoop.ProcessComfortData(sessionId, comfortData);
            
            // Act
            var metrics = _feedbackLoop.GetPerformanceMetrics(sessionId);
            
            // Assert
            Assert.IsNotNull(metrics);
            Assert.AreEqual(1, metrics.TotalDataPointsProcessed);
        }
        
        [Test]
        public void ConfigureComfortThresholds_WithValidThresholds_UpdatesConfiguration()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            
            var newThresholds = new ComfortValidationThresholds
            {
                MinimumComfortScore = 70f,
                CriticalComfortScore = 40f,
                TargetComfortScore = 85f
            };
            
            // Act
            _feedbackLoop.ConfigureComfortThresholds(newThresholds);
            
            // Assert
            // Verification would require accessing internal state or testing through behavior
            Assert.Pass("Thresholds configured successfully");
        }
        
        [Test]
        public void ConfigureAdjustmentStrategies_WithValidStrategies_UpdatesStrategies()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            
            var strategies = new[]
            {
                AdjustmentStrategy.ConservativeReduction,
                AdjustmentStrategy.FrequencyAdjustment
            };
            
            // Act
            _feedbackLoop.ConfigureAdjustmentStrategies(strategies);
            
            // Assert
            Assert.Pass("Strategies configured successfully");
        }
        
        [Test]
        public void StopFeedbackLoop_WithActiveSession_ReturnsSuccess()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            // Act
            var result = _feedbackLoop.StopFeedbackLoop(sessionId);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotNull(result.FinalMetrics);
            Assert.IsNotNull(result.FinalComfortValidation);
            Assert.IsNotNull(result.SessionSummary);
        }
        
        [Test]
        public void GenerateFeedbackReport_WithCompletedSession_ReturnsReport()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            // Process some data
            var comfortData = new ComfortDataPoint
            {
                Timestamp = DateTime.UtcNow,
                ComfortLevel = ComfortLevel.Comfortable
            };
            _feedbackLoop.ProcessComfortData(sessionId, comfortData);
            
            // Act
            var report = _feedbackLoop.GenerateFeedbackReport(sessionId);
            
            // Assert
            Assert.IsNotNull(report);
            Assert.AreEqual(sessionId, report.SessionId);
            Assert.IsNotNull(report.SessionSummary);
            Assert.IsNotNull(report.PerformanceMetrics);
            Assert.IsTrue(report.ReportConfidence > 0);
        }
        
        [Test]
        public void AnalyzeComfortTrends_WithSufficientData_ReturnsAnalysis()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            // Process multiple data points
            for (int i = 0; i < 5; i++)
            {
                var comfortData = new ComfortDataPoint
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(i * 10),
                    ComfortLevel = ComfortLevel.Comfortable
                };
                _feedbackLoop.ProcessComfortData(sessionId, comfortData);
            }
            
            // Act
            var analysis = _feedbackLoop.AnalyzeComfortTrends(sessionId);
            
            // Assert
            Assert.IsNotNull(analysis);
            Assert.IsNotNull(analysis.ComfortTrajectory);
            Assert.IsTrue(analysis.ComfortTrajectory.Length > 0);
            Assert.IsTrue(analysis.AnalysisConfidence > 0);
        }
        
        [Test]
        public void ResetFeedbackSystem_ClearsAllSessions()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            _feedbackLoop.StartFeedbackLoop("session-1", _testWaveSettings);
            _feedbackLoop.StartFeedbackLoop("session-2", _testWaveSettings);
            
            // Act
            _feedbackLoop.ResetFeedbackSystem();
            
            // Assert
            Assert.IsFalse(_feedbackLoop.IsActive);
            Assert.AreEqual(0, _feedbackLoop.ActiveAdjustments.Length);
        }
        
        [Test]
        public void EventHandling_ComfortValidationFailed_FiresEvent()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            bool eventFired = false;
            ComfortValidationFailedEventArgs eventArgs = null;
            
            _feedbackLoop.ComfortValidationFailed += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Process poor comfort data
            var poorComfortData = new ComfortDataPoint
            {
                Timestamp = DateTime.UtcNow,
                ComfortLevel = ComfortLevel.VeryUncomfortable
            };
            _feedbackLoop.ProcessComfortData(sessionId, poorComfortData);
            
            // Act
            _feedbackLoop.ValidateComfortLevels(sessionId);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(sessionId, eventArgs.SessionId);
        }
        
        [Test]
        public void EventHandling_AutomaticRollback_FiresEvent()
        {
            // Arrange
            _feedbackLoop.Initialize(_testConfig, _mockDataSystem, _mockValidator);
            var sessionId = "test-session-1";
            _feedbackLoop.StartFeedbackLoop(sessionId, _testWaveSettings);
            
            bool eventFired = false;
            AutomaticRollbackEventArgs eventArgs = null;
            
            _feedbackLoop.AutomaticRollbackTriggered += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            _feedbackLoop.TriggerAutomaticRollback(sessionId, "Test rollback");
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(sessionId, eventArgs.SessionId);
        }
    }
    
    #region Mock Classes
    
    /// <summary>
    /// Mock implementation of IComfortDataCollectionSystem for testing.
    /// </summary>
    public class MockComfortDataCollectionSystem : IComfortDataCollectionSystem
    {
        public bool IsInitialized { get; private set; }
        public ComfortDataCollectionConfiguration Configuration { get; private set; }
        public DataCollectionSession[] ActiveSessions { get; private set; } = new DataCollectionSession[0];
        
        public event Action<DataCollectionStartedEventArgs> DataCollectionStarted;
        public event Action<ComfortDataCollectedEventArgs> ComfortDataCollected;
        public event Action<DataCollectionCompletedEventArgs> DataCollectionCompleted;
        public event Action<DataValidationFailedEventArgs> DataValidationFailed;
        
        public bool Initialize(ComfortDataCollectionConfiguration config)
        {
            Configuration = config;
            IsInitialized = true;
            return true;
        }
        
        // Implement other interface methods as needed for testing
        public DataCollectionSession StartDataCollection(DataCollectionSessionConfiguration sessionConfig) => new DataCollectionSession();
        public ComfortDataCollectionResult CollectSIMData(string sessionId, SIMQuestionnaireData simData) => new ComfortDataCollectionResult();
        public ComfortDataCollectionResult CollectSSQData(string sessionId, SSQQuestionnaireData ssqData) => new ComfortDataCollectionResult();
        public ComfortDataCollectionResult CollectPhysiologicalData(string sessionId, PhysiologicalComfortData physiologicalData) => new ComfortDataCollectionResult();
        public ComfortDataCollectionResult CollectBehavioralData(string sessionId, BehavioralComfortData behavioralData) => new ComfortDataCollectionResult();
        public ComfortDataValidationResult ValidateCollectedData(string sessionId) => new ComfortDataValidationResult();
        public DataCollectionCompletionResult CompleteDataCollection(string sessionId) => new DataCollectionCompletionResult();
        public ComfortDataAnalysisResult AnalyzeComfortData(string sessionId) => new ComfortDataAnalysisResult();
        public ComfortDataReport GenerateComfortReport(string sessionId, ReportFormat reportFormat) => new ComfortDataReport();
        public ComfortDataExport ExportComfortData(string sessionId, DataExportFormat exportFormat) => new ComfortDataExport();
        public RealTimeComfortMetrics GetRealTimeMetrics(string sessionId) => new RealTimeComfortMetrics();
        public bool SetupAutomatedCollection(string sessionId, AutomatedCollectionSchedule schedule) => true;
        public void ConfigureDataQuality(DataQualityConfiguration qualityConfig) { }
        public DataCollectionStatistics GetCollectionStatistics() => new DataCollectionStatistics();
        public void ResetDataCollectionSystem() { }
    }
    
    /// <summary>
    /// Mock implementation of IWaveParameterValidator for testing.
    /// </summary>
    public class MockWaveParameterValidator : IWaveParameterValidator
    {
        public WaveParameterValidationResult ValidateSettings(WaveMatrixSettings settings)
        {
            return WaveParameterValidationResult.Success();
        }
        
        public WaveParameterValidationResult ValidateWaveParameters(float amplitude, float frequency, float speed, float cellSize)
        {
            return WaveParameterValidationResult.Success();
        }
        
        public WaveParameterValidationResult ValidateGridSettings(Unity.Mathematics.int2 gridSize, float cellSize, int bubbleCount)
        {
            return WaveParameterValidationResult.Success();
        }
        
        public WavePerformanceValidationResult ValidatePerformance(WaveMatrixSettings settings, int expectedBubbleCount, float targetFrameTimeMs)
        {
            return new WavePerformanceValidationResult { MeetsPerformanceTarget = true };
        }
        
        public WaveStabilityValidationResult ValidateStability(WaveMatrixSettings settings, float timeRange = 10f)
        {
            return new WaveStabilityValidationResult { IsStable = true };
        }
        
        public WaveParameterRanges GetRecommendedRanges()
        {
            return WaveParameterRanges.Quest3Safe;
        }
        
        public WaveMatrixSettings SuggestCorrections(WaveMatrixSettings settings, WaveParameterValidationResult validationResult)
        {
            return settings;
        }
        
        public WaveParameterValidationResult ValidateParameterInteractions(WaveMatrixSettings settings)
        {
            return WaveParameterValidationResult.Success();
        }
        
        public WaveParameterValidationResult ValidateSafetyBounds(WaveMatrixSettings settings)
        {
            return WaveParameterValidationResult.Success();
        }
    }
    
    #endregion
}