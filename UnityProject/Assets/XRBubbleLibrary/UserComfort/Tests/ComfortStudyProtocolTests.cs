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
    /// Comprehensive unit tests for ComfortStudyProtocol.
    /// Tests Requirement 7.1: Structured comfort validation study design.
    /// Tests Requirement 7.2: SIM/SSQ study protocol for motion sickness assessment.
    /// </summary>
    [TestFixture]
    public class ComfortStudyProtocolTests
    {
        private ComfortStudyProtocol _comfortStudyProtocol;
        private GameObject _testGameObject;
        
        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("ComfortStudyProtocolTest");
            _comfortStudyProtocol = _testGameObject.AddComponent<ComfortStudyProtocol>();
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
            var config = ComfortStudyConfiguration.Default;
            
            // Act
            bool result = _comfortStudyProtocol.Initialize(config);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_comfortStudyProtocol.IsInitialized);
            Assert.AreEqual(config.TargetParticipantCount, _comfortStudyProtocol.Configuration.TargetParticipantCount);
            Assert.AreEqual(config.SessionDurationMinutes, _comfortStudyProtocol.Configuration.SessionDurationMinutes);
        }
        
        [Test]
        public void Initialize_WithInvalidParticipantCount_ReturnsFalse()
        {
            // Arrange
            var config = ComfortStudyConfiguration.Default;
            config.TargetParticipantCount = 0;
            
            // Act
            bool result = _comfortStudyProtocol.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_comfortStudyProtocol.IsInitialized);
        }
        
        [Test]
        public void Initialize_WithInvalidSessionDuration_ReturnsFalse()
        {
            // Arrange
            var config = ComfortStudyConfiguration.Default;
            config.SessionDurationMinutes = -5f;
            
            // Act
            bool result = _comfortStudyProtocol.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_comfortStudyProtocol.IsInitialized);
        }
        
        [Test]
        public void Initialize_SetsDefaultConfiguration()
        {
            // Arrange
            var config = ComfortStudyConfiguration.Default;
            
            // Act
            _comfortStudyProtocol.Initialize(config);
            
            // Assert
            Assert.AreEqual(12, _comfortStudyProtocol.Configuration.TargetParticipantCount);
            Assert.AreEqual(15f, _comfortStudyProtocol.Configuration.SessionDurationMinutes);
            Assert.AreEqual(5f, _comfortStudyProtocol.Configuration.MeasurementIntervalMinutes);
            Assert.IsTrue(_comfortStudyProtocol.Configuration.RequireIRBApproval);
        }
        
        #endregion
        
        #region Protocol Creation Tests
        
        [Test]
        public void CreateStudyProtocol_WithValidDesign_ReturnsProtocol()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocolDesign = CreateValidProtocolDesign();
            
            // Act
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(protocolDesign);
            
            // Assert
            Assert.IsNotNull(protocol.ProtocolId);
            Assert.AreEqual(protocolDesign.ProtocolName, protocol.Design.ProtocolName);
            Assert.AreEqual(protocolDesign.StudyObjective, protocol.Design.StudyObjective);
            Assert.AreEqual(ProtocolApprovalStatus.Draft, protocol.ApprovalStatus);
            Assert.AreEqual("1.0", protocol.Version);
        }
        
        [Test]
        public void CreateStudyProtocol_WhenNotInitialized_ThrowsException()
        {
            // Arrange
            // Don't initialize the protocol
            var protocolDesign = CreateValidProtocolDesign();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                _comfortStudyProtocol.CreateStudyProtocol(protocolDesign));
        }
        
        [Test]
        public void CreateStudyProtocol_GeneratesUniqueProtocolIds()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocolDesign1 = CreateValidProtocolDesign();
            var protocolDesign2 = CreateValidProtocolDesign();
            protocolDesign2.ProtocolName = "Different Protocol";
            
            // Act
            var protocol1 = _comfortStudyProtocol.CreateStudyProtocol(protocolDesign1);
            var protocol2 = _comfortStudyProtocol.CreateStudyProtocol(protocolDesign2);
            
            // Assert
            Assert.AreNotEqual(protocol1.ProtocolId, protocol2.ProtocolId);
        }
        
        #endregion
        
        #region Protocol Validation Tests
        
        [Test]
        public void ValidateProtocol_WithCompleteProtocol_ReturnsValid()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            
            // Act
            var result = _comfortStudyProtocol.ValidateProtocol(protocol);
            
            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.ValidationScore >= 80f);
            Assert.AreEqual(0, result.RequiredCorrections.Length);
            Assert.IsNotNull(result.ValidationTimestamp);
        }
        
        [Test]
        public void ValidateProtocol_WithMissingProtocolName_ReturnsInvalid()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocolDesign = CreateValidProtocolDesign();
            protocolDesign.ProtocolName = "";
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(protocolDesign);
            
            // Act
            var result = _comfortStudyProtocol.ValidateProtocol(protocol);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("Protocol name")));
            Assert.IsTrue(result.RequiredCorrections.Any(c => c.Contains("protocol name")));
        }
        
        [Test]
        public void ValidateProtocol_WithMissingHypotheses_ReturnsInvalid()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocolDesign = CreateValidProtocolDesign();
            protocolDesign.PreRegisteredHypotheses = new StudyHypothesis[0];
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(protocolDesign);
            
            // Act
            var result = _comfortStudyProtocol.ValidateProtocol(protocol);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("hypotheses")));
            Assert.IsTrue(result.RequiredCorrections.Any(c => c.Contains("hypothesis")));
        }
        
        [Test]
        public void ValidateProtocol_WithMissingSuccessCriteria_ReturnsInvalid()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocolDesign = CreateValidProtocolDesign();
            protocolDesign.SuccessCriteria = new SuccessCriteria[0];
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(protocolDesign);
            
            // Act
            var result = _comfortStudyProtocol.ValidateProtocol(protocol);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("Success criteria")));
        }
        
        [Test]
        public void ValidateProtocol_WithoutPrimaryEndpoint_ReturnsInvalid()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocolDesign = CreateValidProtocolDesign();
            // Make all criteria secondary endpoints
            for (int i = 0; i < protocolDesign.SuccessCriteria.Length; i++)
            {
                protocolDesign.SuccessCriteria[i].IsPrimaryEndpoint = false;
            }
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(protocolDesign);
            
            // Act
            var result = _comfortStudyProtocol.ValidateProtocol(protocol);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("primary endpoint")));
        }
        
        [Test]
        public void ValidateProtocol_WithMissingSIMSSQProcedure_ReturnsInvalid()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocolDesign = CreateValidProtocolDesign();
            // Remove SIM/SSQ procedures
            protocolDesign.DataCollectionProcedures = protocolDesign.DataCollectionProcedures
                .Where(p => p.Method != DataCollectionMethod.SIMQuestionnaire && 
                           p.Method != DataCollectionMethod.SSQQuestionnaire)
                .ToArray();
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(protocolDesign);
            
            // Act
            var result = _comfortStudyProtocol.ValidateProtocol(protocol);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("SIM or SSQ")));
        }
        
        #endregion
        
        #region Study Session Tests
        
        [Test]
        public void StartStudySession_WithValidData_ReturnsActiveSession()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            
            // Act
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            
            // Assert
            Assert.IsNotNull(session.SessionId);
            Assert.AreEqual(participant.ParticipantId, session.Participant.ParticipantId);
            Assert.AreEqual(protocol.ProtocolId, session.Protocol.ProtocolId);
            Assert.IsTrue(session.IsActive);
            Assert.AreEqual(StudySessionPhase.PreSession, session.CurrentPhase);
            Assert.IsNotNull(session.CollectedData);
            Assert.AreEqual(0, session.CollectedData.Count);
        }
        
        [Test]
        public void StartStudySession_WhenNotInitialized_ThrowsException()
        {
            // Arrange
            // Don't initialize the protocol
            var protocol = new StudyProtocol();
            var participant = CreateValidParticipant();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                _comfortStudyProtocol.StartStudySession(participant, protocol));
        }
        
        [Test]
        public void StartStudySession_WithoutConsent_ThrowsException()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            participant.HasConsented = false;
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _comfortStudyProtocol.StartStudySession(participant, protocol));
        }
        
        [Test]
        public void StartStudySession_FiresSessionStartedEvent()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            
            bool eventFired = false;
            StudySessionStartedEventArgs eventArgs = null;
            
            _comfortStudyProtocol.StudySessionStarted += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(session.SessionId, eventArgs.SessionId);
            Assert.AreEqual(participant.ParticipantId, eventArgs.Participant.ParticipantId);
        }
        
        #endregion
        
        #region SIM Data Collection Tests
        
        [Test]
        public void CollectSIMData_WithValidData_ReturnsSuccess()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            var simData = CreateValidSIMData();
            
            // Act
            var result = _comfortStudyProtocol.CollectSIMData(session.SessionId, simData);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotNull(result.DataId);
            Assert.IsTrue(result.DataQualityScore > 0);
            Assert.IsTrue(result.CollectionMessages.Any(m => m.Contains("successfully")));
            Assert.AreEqual(0, result.CollectionErrors.Length);
        }
        
        [Test]
        public void CollectSIMData_WithInvalidSessionId_ReturnsFailure()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var simData = CreateValidSIMData();
            
            // Act
            var result = _comfortStudyProtocol.CollectSIMData("invalid-session-id", simData);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.CollectionErrors.Any(e => e.Contains("Session not found")));
        }
        
        [Test]
        public void CollectSIMData_WithInvalidData_ReturnsFailure()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            var simData = CreateValidSIMData();
            simData.GeneralDiscomfort = 5; // Invalid range (should be 0-3)
            
            // Act
            var result = _comfortStudyProtocol.CollectSIMData(session.SessionId, simData);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.CollectionErrors.Any(e => e.Contains("out of range")));
        }
        
        [Test]
        public void CollectSIMData_FiresDataCollectedEvent()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            var simData = CreateValidSIMData();
            
            bool eventFired = false;
            ComfortDataCollectedEventArgs eventArgs = null;
            
            _comfortStudyProtocol.ComfortDataCollected += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            var result = _comfortStudyProtocol.CollectSIMData(session.SessionId, simData);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(session.SessionId, eventArgs.SessionId);
            Assert.AreEqual(DataCollectionMethod.SIMQuestionnaire, eventArgs.DataPoint.CollectionMethod);
            Assert.IsTrue(eventArgs.DataPoint.SIMData.HasValue);
        }
        
        #endregion
        
        #region SSQ Data Collection Tests
        
        [Test]
        public void CollectSSQData_WithValidData_ReturnsSuccess()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            var ssqData = CreateValidSSQData();
            
            // Act
            var result = _comfortStudyProtocol.CollectSSQData(session.SessionId, ssqData);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotNull(result.DataId);
            Assert.IsTrue(result.DataQualityScore > 0);
            Assert.IsTrue(result.CollectionMessages.Any(m => m.Contains("successfully")));
            Assert.AreEqual(0, result.CollectionErrors.Length);
        }
        
        [Test]
        public void CollectSSQData_WithInvalidTotalScore_ReturnsFailure()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            var ssqData = CreateValidSSQData();
            ssqData.TotalScore = 300f; // Invalid (max is 235.62)
            
            // Act
            var result = _comfortStudyProtocol.CollectSSQData(session.SessionId, ssqData);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.CollectionErrors.Any(e => e.Contains("out of valid range")));
        }
        
        [Test]
        public void CollectSSQData_WithIncorrectItemCount_ReturnsFailure()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            var ssqData = CreateValidSSQData();
            ssqData.ItemResponses = new int[10]; // Should be 16 items
            
            // Act
            var result = _comfortStudyProtocol.CollectSSQData(session.SessionId, ssqData);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.CollectionErrors.Any(e => e.Contains("exactly 16 item responses")));
        }
        
        #endregion
        
        #region Session Completion Tests
        
        [Test]
        public void CompleteStudySession_WithActiveSession_ReturnsSuccess()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            
            // Collect some data first
            _comfortStudyProtocol.CollectSIMData(session.SessionId, CreateValidSIMData());
            _comfortStudyProtocol.CollectSSQData(session.SessionId, CreateValidSSQData());
            
            // Act
            var result = _comfortStudyProtocol.CompleteStudySession(session.SessionId);
            
            // Assert
            Assert.IsTrue(result.CompletedSuccessfully);
            Assert.AreEqual(session.SessionId, result.SessionId);
            Assert.AreEqual(participant.ParticipantId, result.Participant.ParticipantId);
            Assert.IsTrue(result.SessionDuration.TotalSeconds > 0);
            Assert.AreEqual(2, result.CollectedData.Length); // SIM + SSQ data
            Assert.IsNotNull(result.SummaryStatistics);
        }
        
        [Test]
        public void CompleteStudySession_WithInvalidSessionId_ReturnsFailure()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            
            // Act
            var result = _comfortStudyProtocol.CompleteStudySession("invalid-session-id");
            
            // Assert
            Assert.IsFalse(result.CompletedSuccessfully);
            Assert.IsTrue(result.CompletionNotes.Contains("Session not found"));
        }
        
        [Test]
        public void CompleteStudySession_FiresSessionCompletedEvent()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            
            bool eventFired = false;
            StudySessionCompletedEventArgs eventArgs = null;
            
            _comfortStudyProtocol.StudySessionCompleted += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            var result = _comfortStudyProtocol.CompleteStudySession(session.SessionId);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(session.SessionId, eventArgs.CompletionResult.SessionId);
            Assert.IsTrue(eventArgs.CompletionResult.CompletedSuccessfully);
        }
        
        #endregion
        
        #region Protocol Documentation Tests
        
        [Test]
        public void GenerateProtocolDocumentation_WithValidProtocol_ReturnsDocumentation()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            
            // Act
            var documentation = _comfortStudyProtocol.GenerateProtocolDocumentation(protocol);
            
            // Assert
            Assert.AreEqual(protocol.ProtocolId, documentation.ProtocolId);
            Assert.IsNotNull(documentation.DocumentationContent);
            Assert.IsTrue(documentation.DocumentationContent.Contains(protocol.Design.ProtocolName));
            Assert.IsTrue(documentation.DocumentationContent.Contains(protocol.Design.StudyObjective));
            Assert.AreEqual(DocumentationFormat.Markdown, documentation.Format);
            Assert.IsTrue(documentation.IncludedSections.Length > 0);
        }
        
        [Test]
        public void GenerateProtocolDocumentation_IncludesRequiredSections()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            
            // Act
            var documentation = _comfortStudyProtocol.GenerateProtocolDocumentation(protocol);
            
            // Assert
            Assert.IsTrue(documentation.IncludedSections.Contains(DocumentationSection.Methods));
            Assert.IsTrue(documentation.IncludedSections.Contains(DocumentationSection.Procedures));
            Assert.IsTrue(documentation.IncludedSections.Contains(DocumentationSection.EthicalConsiderations));
        }
        
        #endregion
        
        #region Study Analysis Tests
        
        [Test]
        public void AnalyzeStudyResults_WithValidSessions_ReturnsAnalysis()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var sessionResults = CreateTestSessionResults(5);
            
            // Act
            var analysis = _comfortStudyProtocol.AnalyzeStudyResults(sessionResults);
            
            // Assert
            Assert.IsNotNull(analysis.AnalysisId);
            Assert.AreEqual(5, analysis.ParticipantCount);
            Assert.IsTrue(analysis.StudyCompletionRate > 0);
            Assert.IsNotNull(analysis.ComfortStatistics);
            Assert.IsNotNull(analysis.HypothesisResults);
            Assert.IsNotNull(analysis.EffectSizes);
            Assert.IsNotNull(analysis.AdverseEventSummary);
            Assert.IsNotNull(analysis.DataQuality);
        }
        
        [Test]
        public void AnalyzeStudyResults_WithEmptyResults_ReturnsEmptyAnalysis()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var emptyResults = new SessionCompletionResult[0];
            
            // Act
            var analysis = _comfortStudyProtocol.AnalyzeStudyResults(emptyResults);
            
            // Assert
            Assert.IsNotNull(analysis.AnalysisId);
            Assert.AreEqual(0, analysis.ParticipantCount);
            Assert.AreEqual(0f, analysis.StudyCompletionRate);
        }
        
        #endregion
        
        #region Pre-registered Hypotheses Tests
        
        [Test]
        public void GetPreRegisteredHypotheses_ReturnsValidHypotheses()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            
            // Act
            var hypotheses = _comfortStudyProtocol.GetPreRegisteredHypotheses();
            
            // Assert
            Assert.IsNotNull(hypotheses);
            Assert.IsTrue(hypotheses.Length > 0);
            
            foreach (var hypothesis in hypotheses)
            {
                Assert.IsNotNull(hypothesis.HypothesisId);
                Assert.IsNotNull(hypothesis.Statement);
                Assert.IsNotNull(hypothesis.PrimaryOutcome);
                Assert.IsTrue(hypothesis.ExpectedEffectSize > 0);
                Assert.IsTrue(hypothesis.TargetPower > 0 && hypothesis.TargetPower <= 1);
                Assert.IsTrue(hypothesis.AlphaLevel > 0 && hypothesis.AlphaLevel <= 1);
            }
        }
        
        [Test]
        public void GetPreRegisteredHypotheses_IncludesMotionSicknessHypothesis()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            
            // Act
            var hypotheses = _comfortStudyProtocol.GetPreRegisteredHypotheses();
            
            // Assert
            Assert.IsTrue(hypotheses.Any(h => h.Statement.Contains("motion sickness") || h.Statement.Contains("simulator sickness")));
        }
        
        #endregion
        
        #region Success Criteria Evaluation Tests
        
        [Test]
        public void EvaluateSuccessCriteria_WithValidAnalysis_ReturnsEvaluation()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var analysis = CreateTestStudyAnalysis();
            
            // Act
            var evaluation = _comfortStudyProtocol.EvaluateSuccessCriteria(analysis);
            
            // Assert
            Assert.IsNotNull(evaluation.CriteriaResults);
            Assert.IsTrue(evaluation.CriteriaResults.Length > 0);
            Assert.IsTrue(evaluation.PrimaryEndpointSuccessRate >= 0 && evaluation.PrimaryEndpointSuccessRate <= 1);
            Assert.IsTrue(evaluation.SecondaryEndpointSuccessRate >= 0 && evaluation.SecondaryEndpointSuccessRate <= 1);
            Assert.IsNotNull(evaluation.EvaluationSummary);
        }
        
        [Test]
        public void EvaluateSuccessCriteria_WithHighCompletionRate_ReturnsSuccess()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var analysis = CreateTestStudyAnalysis();
            analysis.StudyCompletionRate = 0.95f; // High completion rate
            
            // Act
            var evaluation = _comfortStudyProtocol.EvaluateSuccessCriteria(analysis);
            
            // Assert
            Assert.IsTrue(evaluation.StudySuccessful);
            Assert.IsTrue(evaluation.CriteriaResults.Any(c => c.CriteriaMet));
        }
        
        #endregion
        
        #region Data Export Tests
        
        [Test]
        public void ExportStudyData_WithCSVFormat_ReturnsCSVExport()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            
            // Act
            var export = _comfortStudyProtocol.ExportStudyData(DataExportFormat.CSV);
            
            // Assert
            Assert.IsNotNull(export.ExportId);
            Assert.AreEqual(DataExportFormat.CSV, export.Format);
            Assert.IsNotNull(export.DataContent);
            Assert.IsTrue(export.FilePath.EndsWith(".csv"));
            Assert.AreEqual(DataAnonymizationLevel.Anonymized, export.AnonymizationLevel);
        }
        
        [Test]
        public void ExportStudyData_WithJSONFormat_ReturnsJSONExport()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            
            // Act
            var export = _comfortStudyProtocol.ExportStudyData(DataExportFormat.JSON);
            
            // Assert
            Assert.AreEqual(DataExportFormat.JSON, export.Format);
            Assert.IsTrue(export.DataContent.Contains("{"));
            Assert.IsTrue(export.FilePath.EndsWith(".json"));
        }
        
        #endregion
        
        #region State Management Tests
        
        [Test]
        public void ResetStudyProtocol_ClearsAllData()
        {
            // Arrange
            _comfortStudyProtocol.Initialize(ComfortStudyConfiguration.Default);
            var protocol = _comfortStudyProtocol.CreateStudyProtocol(CreateValidProtocolDesign());
            var participant = CreateValidParticipant();
            var session = _comfortStudyProtocol.StartStudySession(participant, protocol);
            
            // Act
            _comfortStudyProtocol.ResetStudyProtocol();
            
            // Assert
            // Verify that attempting to use the previous session fails
            var result = _comfortStudyProtocol.CollectSIMData(session.SessionId, CreateValidSIMData());
            Assert.IsFalse(result.IsSuccessful);
        }
        
        #endregion
        
        #region Helper Methods
        
        private StudyProtocolDesign CreateValidProtocolDesign()
        {
            return new StudyProtocolDesign
            {
                ProtocolName = "VR Comfort Validation Study",
                StudyObjective = "Evaluate user comfort during wave-based text input in VR",
                TargetDemographics = new ParticipantDemographics
                {
                    TargetAgeRanges = new[] { AgeRange.Age18To25, AgeRange.Age26To35 },
                    GenderDistribution = new Dictionary<Gender, float> { { Gender.Male, 0.5f }, { Gender.Female, 0.5f } },
                    TargetVRExperience = new[] { VRExperienceLevel.Beginner, VRExperienceLevel.Intermediate },
                    InclusionCriteria = new[] { "Age 18-35", "Normal vision" },
                    ExclusionCriteria = new[] { "Motion sickness history", "Vestibular disorders" },
                    RecruitmentStrategy = "University recruitment"
                },
                ExperimentalConditions = new[]
                {
                    new ExperimentalCondition
                    {
                        ConditionId = "baseline",
                        Name = "Baseline Condition",
                        WaveParameters = new WaveParameters { Amplitude = 0f, Frequency = 0f },
                        ExpectedComfortLevel = ComfortLevel.Comfortable,
                        PresentationOrder = 1,
                        ExposureDurationMinutes = 5f
                    }
                },
                PreRegisteredHypotheses = new[]
                {
                    new StudyHypothesis
                    {
                        HypothesisId = "H1",
                        Statement = "Wave-based input will not increase motion sickness",
                        Type = HypothesisType.Null,
                        ExpectedEffectSize = 0.3f,
                        TargetPower = 0.8f,
                        AlphaLevel = 0.05f,
                        PrimaryOutcome = "SSQ Total Score",
                        PreRegistrationTime = DateTime.Now
                    }
                },
                SuccessCriteria = new[]
                {
                    new SuccessCriteria
                    {
                        CriteriaId = "primary_comfort",
                        Description = "No significant increase in motion sickness",
                        Metric = "SSQ Total Score",
                        TargetThreshold = 20f,
                        Operator = ComparisonOperator.LessThan,
                        Priority = CriteriaPriority.High,
                        IsPrimaryEndpoint = true
                    }
                },
                DataCollectionProcedures = new[]
                {
                    new DataCollectionProcedure
                    {
                        ProcedureId = "sim_collection",
                        Name = "SIM Questionnaire Collection",
                        Timing = DataCollectionTiming.DuringSession,
                        Method = DataCollectionMethod.SIMQuestionnaire,
                        RequiredFields = new[] { "GeneralDiscomfort", "Nausea", "Fatigue" },
                        ParticipantInstructions = "Rate your current symptoms",
                        ResearcherInstructions = "Administer SIM questionnaire every 5 minutes"
                    },
                    new DataCollectionProcedure
                    {
                        ProcedureId = "ssq_collection",
                        Name = "SSQ Questionnaire Collection",
                        Timing = DataCollectionTiming.PostSession,
                        Method = DataCollectionMethod.SSQQuestionnaire,
                        RequiredFields = new[] { "TotalScore", "NauseaScore", "OculomotorScore" },
                        ParticipantInstructions = "Complete the simulator sickness questionnaire",
                        ResearcherInstructions = "Administer SSQ immediately after session"
                    }
                },
                EthicalConsiderations = new EthicalConsiderations
                {
                    InformedConsentRequirements = "Written informed consent required",
                    RiskMitigationStrategies = new[] { "Immediate session termination if severe symptoms" },
                    DataPrivacyProcedures = "All data anonymized with participant IDs",
                    WithdrawalProcedures = "Participants may withdraw at any time",
                    EmergencyProcedures = "Research staff trained in motion sickness response",
                    IRBRequirements = "IRB approval required before data collection"
                }
            };
        }
        
        private ParticipantInfo CreateValidParticipant()
        {
            return new ParticipantInfo
            {
                ParticipantId = "P001",
                AgeRange = AgeRange.Age18To25,
                Gender = Gender.Female,
                VRExperience = VRExperienceLevel.Beginner,
                MotionSusceptibility = MotionSicknessSusceptibility.Low,
                HasConsented = true,
                EnrollmentTime = DateTime.Now,
                AdditionalDemographics = new Dictionary<string, string>()
            };
        }
        
        private SIMQuestionnaireData CreateValidSIMData()
        {
            return new SIMQuestionnaireData
            {
                Timestamp = DateTime.Now,
                GeneralDiscomfort = 1,
                Fatigue = 0,
                Headache = 0,
                EyeStrain = 1,
                DifficultyFocusing = 0,
                Salivation = 0,
                Sweating = 0,
                Nausea = 0,
                DifficultyConcentrating = 0,
                FullnessOfHead = 0,
                BlurredVision = 0,
                DizzinessEyesOpen = 0,
                DizzinessEyesClosed = 0,
                Vertigo = 0,
                StomachAwareness = 0,
                Burping = 0,
                AdditionalSymptoms = ""
            };
        }
        
        private SSQQuestionnaireData CreateValidSSQData()
        {
            return new SSQQuestionnaireData
            {
                Timestamp = DateTime.Now,
                NauseaScore = 5.0f,
                OculomotorScore = 7.5f,
                DisorientationScore = 3.0f,
                TotalScore = 15.5f,
                ItemResponses = new int[] { 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0 },
                CompletionTimeSeconds = 45f,
                ParticipantNotes = ""
            };
        }
        
        private SessionCompletionResult[] CreateTestSessionResults(int count)
        {
            var results = new SessionCompletionResult[count];
            
            for (int i = 0; i < count; i++)
            {
                results[i] = new SessionCompletionResult
                {
                    SessionId = $"session_{i}",
                    CompletedSuccessfully = true,
                    CompletionTimestamp = DateTime.Now.AddMinutes(-i * 30),
                    SessionDuration = TimeSpan.FromMinutes(15),
                    Participant = CreateValidParticipant(),
                    CollectedData = new[]
                    {
                        new ComfortDataPoint
                        {
                            DataPointId = $"data_{i}_1",
                            CollectionMethod = DataCollectionMethod.SIMQuestionnaire,
                            SIMData = CreateValidSIMData()
                        },
                        new ComfortDataPoint
                        {
                            DataPointId = $"data_{i}_2",
                            CollectionMethod = DataCollectionMethod.SSQQuestionnaire,
                            SSQData = CreateValidSSQData()
                        }
                    },
                    SummaryStatistics = new SessionSummaryStatistics
                    {
                        AverageSIMScore = 2.0f,
                        AverageSSQScore = 15.0f,
                        DataPointCount = 2,
                        CompletionRate = 1.0f,
                        SessionQualityScore = 85f,
                        MetMinimumRequirements = true
                    },
                    AdverseEvents = new AdverseEvent[0]
                };
            }
            
            return results;
        }
        
        private ComfortStudyAnalysis CreateTestStudyAnalysis()
        {
            return new ComfortStudyAnalysis
            {
                AnalysisId = "analysis_001",
                ParticipantCount = 12,
                StudyCompletionRate = 0.92f,
                ComfortStatistics = new ComfortStatistics
                {
                    MeanSIMScores = new Dictionary<string, float> { { "baseline", 2.5f } },
                    MeanSSQScores = new Dictionary<string, float> { { "baseline", 18.0f } }
                },
                HypothesisResults = new HypothesisTestResult[0],
                EffectSizes = new EffectSizeAnalysis[0],
                AdverseEventSummary = new AdverseEventSummary { TotalEvents = 0 },
                DataQuality = new StudyDataQualityAssessment { OverallQualityScore = 88f },
                AnalysisTimestamp = DateTime.Now
            };
        }
        
        #endregion
    }
}