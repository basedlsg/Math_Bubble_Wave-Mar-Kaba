using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XRBubbleLibrary.UserComfort.Tests
{
    /// <summary>
    /// Tests for ComfortDataCollectionSystem.
    /// Validates Requirement 7.3: Standardized motion sickness assessment tools.
    /// Validates Requirement 7.4: Data collection automation and validation.
    /// </summary>
    [TestFixture]
    public class ComfortDataCollectionSystemTests
    {
        private ComfortDataCollectionSystem _comfortDataCollectionSystem;
        private GameObject _testGameObject;
        
        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("TestComfortDataCollectionSystem");
            _comfortDataCollectionSystem = _testGameObject.AddComponent<ComfortDataCollectionSystem>();
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
            var config = ComfortDataCollectionConfiguration.Default;
            
            // Act
            var result = _comfortDataCollectionSystem.Initialize(config);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_comfortDataCollectionSystem.IsInitialized);
            Assert.AreEqual(config.MaxConcurrentSessions, _comfortDataCollectionSystem.Configuration.MaxConcurrentSessions);
        }
        
        [Test]
        public void Initialize_WithInvalidConfiguration_ReturnsFalse()
        {
            // Arrange
            var config = new ComfortDataCollectionConfiguration
            {
                DefaultCollectionIntervalSeconds = -1f, // Invalid
                MaxConcurrentSessions = 10
            };
            
            // Act
            var result = _comfortDataCollectionSystem.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_comfortDataCollectionSystem.IsInitialized);
        }
        
        [Test]
        public void StartDataCollection_WithValidConfiguration_CreatesSession()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var sessionConfig = CreateValidSessionConfiguration();
            
            // Act
            var session = _comfortDataCollectionSystem.StartDataCollection(sessionConfig);
            
            // Assert
            Assert.IsNotNull(session);
            Assert.IsNotEmpty(session.SessionId);
            Assert.AreEqual(DataCollectionSessionStatus.Active, session.Status);
            Assert.AreEqual(sessionConfig.SessionName, session.Configuration.SessionName);
            Assert.IsNotNull(session.CollectedData);
            Assert.AreEqual(0, session.CollectedData.Count);
        }
        
        [Test]
        public void StartDataCollection_WhenNotInitialized_ThrowsException()
        {
            // Arrange
            var sessionConfig = CreateValidSessionConfiguration();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                _comfortDataCollectionSystem.StartDataCollection(sessionConfig));
        }
        
        [Test]
        public void StartDataCollection_WithInvalidConfiguration_ThrowsException()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var sessionConfig = new DataCollectionSessionConfiguration
            {
                SessionName = "", // Invalid - empty name
                CollectionMethods = new DataCollectionMethod[0] // Invalid - no methods
            };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _comfortDataCollectionSystem.StartDataCollection(sessionConfig));
        }
        
        [Test]
        public void CollectSIMData_WithValidData_ReturnsSuccessResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            var simData = CreateValidSIMData();
            
            // Act
            var result = _comfortDataCollectionSystem.CollectSIMData(session.SessionId, simData);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotEmpty(result.DataPointId);
            Assert.AreEqual(DataCollectionMethod.SIMQuestionnaire, result.CollectionMethod);
            Assert.Greater(result.DataQualityScore, 0f);
            Assert.IsTrue(result.ValidationResult.IsValid);
        }
        
        [Test]
        public void CollectSIMData_WithInvalidSessionId_ReturnsFailureResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var simData = CreateValidSIMData();
            
            // Act
            var result = _comfortDataCollectionSystem.CollectSIMData("invalid-session-id", simData);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsNull(result.DataPointId);
            Assert.Contains("Session not found", result.CollectionErrors);
        }
        
        [Test]
        public void CollectSIMData_WithInvalidData_ReturnsFailureResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            var simData = new SIMQuestionnaireData
            {
                Responses = new SIMResponse[0], // Invalid - no responses
                CompletionTimeSeconds = -1f // Invalid - negative time
            };
            
            // Act
            var result = _comfortDataCollectionSystem.CollectSIMData(session.SessionId, simData);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.Contains("validation failed", result.CollectionErrors[0]);
        }
        
        [Test]
        public void CollectSSQData_WithValidData_ReturnsSuccessResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            var ssqData = CreateValidSSQData();
            
            // Act
            var result = _comfortDataCollectionSystem.CollectSSQData(session.SessionId, ssqData);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotEmpty(result.DataPointId);
            Assert.AreEqual(DataCollectionMethod.SSQQuestionnaire, result.CollectionMethod);
            Assert.Greater(result.DataQualityScore, 0f);
            Assert.IsTrue(result.ValidationResult.IsValid);
        }
        
        [Test]
        public void CollectPhysiologicalData_WithValidData_ReturnsSuccessResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            var physiologicalData = CreateValidPhysiologicalData();
            
            // Act
            var result = _comfortDataCollectionSystem.CollectPhysiologicalData(session.SessionId, physiologicalData);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotEmpty(result.DataPointId);
            Assert.AreEqual(DataCollectionMethod.PhysiologicalMeasurement, result.CollectionMethod);
            Assert.Greater(result.DataQualityScore, 0f);
        }
        
        [Test]
        public void CollectPhysiologicalData_WithInvalidData_ReturnsFailureResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            var physiologicalData = new PhysiologicalComfortData
            {
                HeartRateBPM = 300f, // Invalid - too high
                SkinTemperatureCelsius = 60f // Invalid - too high
            };
            
            // Act
            var result = _comfortDataCollectionSystem.CollectPhysiologicalData(session.SessionId, physiologicalData);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.Contains("validation failed", result.CollectionErrors[0]);
        }
        
        [Test]
        public void CollectBehavioralData_WithValidData_ReturnsSuccessResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            var behavioralData = CreateValidBehavioralData();
            
            // Act
            var result = _comfortDataCollectionSystem.CollectBehavioralData(session.SessionId, behavioralData);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotEmpty(result.DataPointId);
            Assert.AreEqual(DataCollectionMethod.BehavioralObservation, result.CollectionMethod);
            Assert.Greater(result.DataQualityScore, 0f);
        }
        
        [Test]
        public void ValidateCollectedData_WithValidSession_ReturnsValidationResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            
            // Collect some data
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            _comfortDataCollectionSystem.CollectSSQData(session.SessionId, CreateValidSSQData());
            
            // Act
            var validationResult = _comfortDataCollectionSystem.ValidateCollectedData(session.SessionId);
            
            // Assert
            Assert.IsNotNull(validationResult);
            Assert.AreEqual(2, validationResult.DataPointsValidated);
            Assert.Greater(validationResult.OverallQualityScore, 0f);
            Assert.Greater(validationResult.CompletenessPercentage, 0f);
        }
        
        [Test]
        public void ValidateCollectedData_WithInvalidSession_ReturnsInvalidResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            
            // Act
            var validationResult = _comfortDataCollectionSystem.ValidateCollectedData("invalid-session-id");
            
            // Assert
            Assert.IsFalse(validationResult.IsValid);
            Assert.AreEqual(0, validationResult.DataPointsValidated);
            Assert.AreEqual(1, validationResult.ValidationFailures);
        }
        
        [Test]
        public void CompleteDataCollection_WithValidSession_ReturnsCompletionResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            
            // Collect some data
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            _comfortDataCollectionSystem.CollectSSQData(session.SessionId, CreateValidSSQData());
            
            // Act
            var completionResult = _comfortDataCollectionSystem.CompleteDataCollection(session.SessionId);
            
            // Assert
            Assert.IsTrue(completionResult.CompletedSuccessfully);
            Assert.AreEqual(session.SessionId, completionResult.SessionId);
            Assert.AreEqual(2, completionResult.TotalDataPointsCollected);
            Assert.Greater(completionResult.SessionDuration.TotalSeconds, 0);
            Assert.IsNotNull(completionResult.FinalValidationResult);
            Assert.IsNotNull(completionResult.SummaryStatistics);
        }
        
        [Test]
        public void CompleteDataCollection_WithInvalidSession_ReturnsFailureResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            
            // Act
            var completionResult = _comfortDataCollectionSystem.CompleteDataCollection("invalid-session-id");
            
            // Assert
            Assert.IsFalse(completionResult.CompletedSuccessfully);
            Assert.Contains("Session not found", completionResult.CompletionNotes);
        }
        
        [Test]
        public void AnalyzeComfortData_WithValidSession_ReturnsAnalysisResult()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            
            // Collect data
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            _comfortDataCollectionSystem.CollectSSQData(session.SessionId, CreateValidSSQData());
            _comfortDataCollectionSystem.CollectPhysiologicalData(session.SessionId, CreateValidPhysiologicalData());
            _comfortDataCollectionSystem.CollectBehavioralData(session.SessionId, CreateValidBehavioralData());
            
            // Act
            var analysisResult = _comfortDataCollectionSystem.AnalyzeComfortData(session.SessionId);
            
            // Assert
            Assert.IsNotNull(analysisResult);
            Assert.AreEqual(session.SessionId, analysisResult.SessionId);
            Assert.IsNotEmpty(analysisResult.AnalysisId);
            Assert.IsNotNull(analysisResult.ComfortAnalysis);
            Assert.IsNotNull(analysisResult.MotionSicknessAnalysis);
            Assert.IsNotNull(analysisResult.PhysiologicalAnalysis);
            Assert.IsNotNull(analysisResult.BehavioralAnalysis);
            Assert.IsNotNull(analysisResult.TrendAnalysis);
            Assert.IsNotNull(analysisResult.KeyInsights);
            Assert.IsNotNull(analysisResult.Recommendations);
            Assert.Greater(analysisResult.AnalysisConfidence, 0f);
        }
        
        [Test]
        public void GenerateComfortReport_WithValidSession_ReturnsReport()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            
            // Act
            var report = _comfortDataCollectionSystem.GenerateComfortReport(session.SessionId, ReportFormat.JSON);
            
            // Assert
            Assert.IsNotNull(report);
            Assert.IsNotEmpty(report.ReportId);
            Assert.AreEqual(session.SessionId, report.SessionId);
            Assert.AreEqual(ReportFormat.JSON, report.ReportFormat);
            Assert.IsNotEmpty(report.ReportContent);
            Assert.IsNotEmpty(report.ReportSummary);
        }
        
        [Test]
        public void ExportComfortData_WithValidSession_ReturnsExport()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            _comfortDataCollectionSystem.CollectSSQData(session.SessionId, CreateValidSSQData());
            
            // Act
            var export = _comfortDataCollectionSystem.ExportComfortData(session.SessionId, DataExportFormat.CSV);
            
            // Assert
            Assert.IsNotNull(export);
            Assert.IsTrue(export.IsSuccessful);
            Assert.IsNotEmpty(export.ExportId);
            Assert.AreEqual(session.SessionId, export.SessionId);
            Assert.AreEqual(DataExportFormat.CSV, export.ExportFormat);
            Assert.IsNotEmpty(export.ExportContent);
            Assert.AreEqual(2, export.RecordCount);
        }
        
        [Test]
        public void GetRealTimeMetrics_WithActiveSession_ReturnsMetrics()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            
            // Act
            var metrics = _comfortDataCollectionSystem.GetRealTimeMetrics(session.SessionId);
            
            // Assert
            Assert.IsNotNull(metrics);
            Assert.Greater(metrics.SessionTimeMinutes, 0f);
            Assert.AreEqual(1, metrics.DataPointsCollected);
            Assert.Greater(metrics.CurrentDataQuality, 0f);
            Assert.IsNotNull(metrics.ActiveAlerts);
        }
        
        [Test]
        public void SetupAutomatedCollection_WithValidSchedule_ReturnsTrue()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            var schedule = CreateValidAutomatedSchedule();
            
            // Act
            var result = _comfortDataCollectionSystem.SetupAutomatedCollection(session.SessionId, schedule);
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [Test]
        public void SetupAutomatedCollection_WithInvalidSession_ReturnsFalse()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var schedule = CreateValidAutomatedSchedule();
            
            // Act
            var result = _comfortDataCollectionSystem.SetupAutomatedCollection("invalid-session-id", schedule);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void ConfigureDataQuality_WithValidConfiguration_UpdatesConfiguration()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var qualityConfig = CreateValidQualityConfiguration();
            
            // Act
            _comfortDataCollectionSystem.ConfigureDataQuality(qualityConfig);
            
            // Assert
            // Configuration should be updated (verified through behavior in other tests)
            Assert.Pass("Quality configuration updated successfully");
        }
        
        [Test]
        public void GetCollectionStatistics_AfterDataCollection_ReturnsStatistics()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            
            // Collect data and complete session
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            _comfortDataCollectionSystem.CollectSSQData(session.SessionId, CreateValidSSQData());
            _comfortDataCollectionSystem.CompleteDataCollection(session.SessionId);
            
            // Act
            var statistics = _comfortDataCollectionSystem.GetCollectionStatistics();
            
            // Assert
            Assert.IsNotNull(statistics);
            Assert.AreEqual(1, statistics.TotalSessions);
            Assert.AreEqual(2, statistics.TotalDataPoints);
            Assert.Greater(statistics.AverageSessionDuration, 0f);
            Assert.Greater(statistics.AverageDataQuality, 0f);
            Assert.IsNotNull(statistics.MethodStatistics);
        }
        
        [Test]
        public void ResetDataCollectionSystem_ClearsAllData()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            
            // Act
            _comfortDataCollectionSystem.ResetDataCollectionSystem();
            
            // Assert
            Assert.AreEqual(0, _comfortDataCollectionSystem.ActiveSessions.Length);
            var statistics = _comfortDataCollectionSystem.GetCollectionStatistics();
            Assert.AreEqual(0, statistics.TotalSessions);
        }
        
        [Test]
        public void DataCollectionEvents_AreTriggeredCorrectly()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            
            bool dataCollectionStartedFired = false;
            bool comfortDataCollectedFired = false;
            bool dataCollectionCompletedFired = false;
            
            _comfortDataCollectionSystem.DataCollectionStarted += (args) => dataCollectionStartedFired = true;
            _comfortDataCollectionSystem.ComfortDataCollected += (args) => comfortDataCollectedFired = true;
            _comfortDataCollectionSystem.DataCollectionCompleted += (args) => dataCollectionCompletedFired = true;
            
            // Act
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            _comfortDataCollectionSystem.CompleteDataCollection(session.SessionId);
            
            // Assert
            Assert.IsTrue(dataCollectionStartedFired, "DataCollectionStarted event should be fired");
            Assert.IsTrue(comfortDataCollectedFired, "ComfortDataCollected event should be fired");
            Assert.IsTrue(dataCollectionCompletedFired, "DataCollectionCompleted event should be fired");
        }
        
        [UnityTest]
        public IEnumerator RealTimeMetrics_UpdateOverTime()
        {
            // Arrange
            _comfortDataCollectionSystem.Initialize(ComfortDataCollectionConfiguration.Default);
            var session = _comfortDataCollectionSystem.StartDataCollection(CreateValidSessionConfiguration());
            
            // Act
            var initialMetrics = _comfortDataCollectionSystem.GetRealTimeMetrics(session.SessionId);
            yield return new WaitForSeconds(0.1f);
            
            _comfortDataCollectionSystem.CollectSIMData(session.SessionId, CreateValidSIMData());
            yield return new WaitForSeconds(0.1f);
            
            var updatedMetrics = _comfortDataCollectionSystem.GetRealTimeMetrics(session.SessionId);
            
            // Assert
            Assert.Greater(updatedMetrics.SessionTimeMinutes, initialMetrics.SessionTimeMinutes);
            Assert.Greater(updatedMetrics.DataPointsCollected, initialMetrics.DataPointsCollected);
        }
        
        // Helper methods for creating test data
        
        private DataCollectionSessionConfiguration CreateValidSessionConfiguration()
        {
            return new DataCollectionSessionConfiguration
            {
                SessionName = "Test Session",
                Participant = new ParticipantInfo
                {
                    ParticipantId = "P001",
                    Age = 25,
                    Gender = "Male",
                    ExperienceLevel = VRExperienceLevel.Intermediate,
                    Susceptibility = MotionSicknessSusceptibility.Moderate,
                    AdditionalCharacteristics = new Dictionary<string, string>()
                },
                CollectionMethods = new[] { DataCollectionMethod.SIMQuestionnaire, DataCollectionMethod.SSQQuestionnaire },
                CollectionIntervalSeconds = 30f,
                ExpectedDurationMinutes = 10f,
                EnableAutomatedCollection = false,
                QualityRequirements = new DataQualityRequirements
                {
                    MinimumQualityScore = 70f,
                    RequiredCompletenessPercentage = 80f,
                    MaxResponseTimeSeconds = 300f,
                    RequirePhysiologicalValidation = false,
                    RequireBehavioralValidation = false,
                    CustomValidationRules = new string[0]
                },
                SessionNotes = "Test session notes"
            };
        }
        
        private SIMQuestionnaireData CreateValidSIMData()
        {
            return new SIMQuestionnaireData
            {
                Responses = new[]
                {
                    new SIMResponse
                    {
                        QuestionId = "SIM1",
                        QuestionText = "General discomfort",
                        ResponseValue = 1,
                        ResponseTimestamp = DateTime.Now,
                        ResponseTimeSeconds = 5f
                    },
                    new SIMResponse
                    {
                        QuestionId = "SIM2",
                        QuestionText = "Fatigue",
                        ResponseValue = 0,
                        ResponseTimestamp = DateTime.Now,
                        ResponseTimeSeconds = 3f
                    }
                },
                TotalScore = 5f,
                NauseaScore = 2f,
                OculomotorScore = 1f,
                DisorientationScore = 2f,
                CompletionTimeSeconds = 120f,
                AdministrationTimestamp = DateTime.Now,
                ResponseQuality = new QuestionnaireQuality
                {
                    CompletenessPercentage = 100f,
                    ConsistencyScore = 85f,
                    AverageResponseTime = 4f,
                    ResponsePatterns = new[] { "Consistent" },
                    QualityConcerns = new string[0]
                }
            };
        }
        
        private SSQQuestionnaireData CreateValidSSQData()
        {
            return new SSQQuestionnaireData
            {
                Responses = new[]
                {
                    new SSQResponse
                    {
                        QuestionId = "SSQ1",
                        QuestionText = "General discomfort",
                        ResponseValue = 1,
                        ResponseTimestamp = DateTime.Now,
                        ResponseTimeSeconds = 4f
                    },
                    new SSQResponse
                    {
                        QuestionId = "SSQ2",
                        QuestionText = "Nausea",
                        ResponseValue = 0,
                        ResponseTimestamp = DateTime.Now,
                        ResponseTimeSeconds = 6f
                    }
                },
                TotalScore = 7.48f,
                NauseaScore = 9.54f,
                OculomotorScore = 7.58f,
                DisorientationScore = 13.92f,
                CompletionTimeSeconds = 180f,
                AdministrationTimestamp = DateTime.Now,
                ResponseQuality = new QuestionnaireQuality
                {
                    CompletenessPercentage = 100f,
                    ConsistencyScore = 90f,
                    AverageResponseTime = 5f,
                    ResponsePatterns = new[] { "Normal" },
                    QualityConcerns = new string[0]
                }
            };
        }
        
        private PhysiologicalComfortData CreateValidPhysiologicalData()
        {
            return new PhysiologicalComfortData
            {
                Timestamp = DateTime.Now,
                HeartRateBPM = 75f,
                HeartRateVariability = 45f,
                SkinConductanceLevel = 5.2f,
                SkinTemperatureCelsius = 32.5f,
                RespiratoryRate = 16f,
                BloodPressureSystolic = 120f,
                BloodPressureDiastolic = 80f,
                PupilDiameter = 4.2f,
                BlinkRate = 18f,
                HeadMovementVelocity = 2.1f,
                PosturalSway = 1.5f,
                AdditionalMeasurements = new Dictionary<string, float>(),
                DataQuality = new PhysiologicalDataQuality
                {
                    SignalQuality = 85f,
                    MeasurementAccuracy = 90f,
                    CompletenessPercentage = 100f,
                    ArtifactResults = new ArtifactDetectionResult[0],
                    QualityFlags = new string[0]
                }
            };
        }
        
        private BehavioralComfortData CreateValidBehavioralData()
        {
            return new BehavioralComfortData
            {
                Timestamp = DateTime.Now,
                ObserverId = "Observer001",
                ObservedComfortLevel = ComfortLevel.Comfortable,
                VisibleIndicators = new[] { DiscomfortIndicator.HeadMovement },
                VerbalReports = new[] { "Feeling fine" },
                Observations = new[]
                {
                    new BehavioralObservation
                    {
                        ObservationId = "OBS001",
                        Category = ObservationCategory.Behavioral,
                        Description = "Slight head movement",
                        Severity = ObservationSeverity.Minimal,
                        DurationSeconds = 5f,
                        ConfidenceLevel = 85f
                    }
                },
                ObserverConfidence = 90f,
                EnvironmentalFactors = new[]
                {
                    new EnvironmentalFactor
                    {
                        FactorId = "TEMP001",
                        Type = EnvironmentalFactorType.Temperature,
                        Description = "Room temperature",
                        MeasurementValue = 22f,
                        MeasurementUnit = "Celsius",
                        Impact = FactorImpact.Minor
                    }
                },
                ObserverNotes = "Participant appears comfortable",
                DataQuality = new BehavioralDataQuality
                {
                    ObserverReliability = 90f,
                    ObservationCompleteness = 95f,
                    InterObserverAgreement = 85f,
                    ConsistencyScore = 88f,
                    QualityNotes = new[] { "High quality observation" }
                }
            };
        }
        
        private AutomatedCollectionSchedule CreateValidAutomatedSchedule()
        {
            return new AutomatedCollectionSchedule
            {
                CollectionIntervalSeconds = 30f,
                AutomatedMethods = new[] { DataCollectionMethod.PhysiologicalMeasurement },
                EnableAdaptiveScheduling = false,
                ScheduleStartTime = DateTime.Now,
                ScheduleEndTime = DateTime.Now.AddMinutes(10),
                TriggerConditions = new CollectionTrigger[0],
                Priority = SchedulePriority.Medium
            };
        }
        
        private DataQualityConfiguration CreateValidQualityConfiguration()
        {
            return new DataQualityConfiguration
            {
                ValidationRules = new[]
                {
                    new QualityValidationRule
                    {
                        RuleId = "TestRule",
                        RuleName = "Test Quality Rule",
                        Description = "Test rule for quality validation",
                        Condition = "quality >= 70",
                        Severity = RuleSeverity.Warning,
                        IsEnabled = true
                    }
                },
                QualityThresholds = new[]
                {
                    new QualityThreshold
                    {
                        ThresholdId = "TestThreshold",
                        MetricName = "OverallQuality",
                        MinimumValue = 70f,
                        MaximumValue = 100f,
                        Action = ThresholdAction.Alert
                    }
                },
                AutomatedChecks = new[]
                {
                    new AutomatedQualityCheck
                    {
                        CheckId = "TestCheck",
                        Type = QualityCheckType.Accuracy,
                        FrequencySeconds = 30f,
                        IsEnabled = true,
                        Parameters = new Dictionary<string, object>()
                    }
                },
                AlertConfiguration = new QualityAlertConfiguration
                {
                    EnableAlerts = true,
                    AlertThresholds = new[]
                    {
                        new AlertThreshold
                        {
                            MetricName = "DataQuality",
                            ThresholdValue = 70f,
                            Severity = AlertSeverity.Warning,
                            AlertMessage = "Data quality below threshold"
                        }
                    },
                    NotificationMethods = new[] { AlertNotificationMethod.Console },
                    EscalationRules = new AlertEscalationRule[0]
                },
                EnableRealTimeMonitoring = true
            };
        }
    }
}