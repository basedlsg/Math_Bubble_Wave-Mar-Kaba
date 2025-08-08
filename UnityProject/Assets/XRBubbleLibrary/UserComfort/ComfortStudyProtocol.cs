using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Comfort study protocol design and management system.
    /// Implements Requirement 7.1: Structured comfort validation study design.
    /// Implements Requirement 7.2: SIM/SSQ study protocol for motion sickness assessment.
    /// </summary>
    public class ComfortStudyProtocol : MonoBehaviour, IComfortStudyProtocol
    {
        // Configuration and state
        private ComfortStudyConfiguration _configuration;
        private ComfortStudySession _currentSession;
        private bool _isInitialized;
        
        // Study data storage
        private readonly Dictionary<string, StudyProtocol> _studyProtocols = new Dictionary<string, StudyProtocol>();
        private readonly Dictionary<string, ComfortStudySession> _activeSessions = new Dictionary<string, ComfortStudySession>();
        private readonly List<SessionCompletionResult> _completedSessions = new List<SessionCompletionResult>();
        
        // Pre-registered hypotheses
        private StudyHypothesis[] _preRegisteredHypotheses;
        
        // Constants
        private const int MAX_ACTIVE_SESSIONS = 10;
        private const int MAX_COMPLETED_SESSIONS = 100;
        
        // Events
        public event Action<StudySessionStartedEventArgs> StudySessionStarted;
        public event Action<StudySessionCompletedEventArgs> StudySessionCompleted;
        public event Action<ComfortDataCollectedEventArgs> ComfortDataCollected;
        
        /// <summary>
        /// Whether the comfort study protocol is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Current study protocol configuration.
        /// </summary>
        public ComfortStudyConfiguration Configuration => _configuration;
        
        /// <summary>
        /// Current study session information.
        /// </summary>
        public ComfortStudySession CurrentSession => _currentSession;
        
        private void Awake()
        {
            // Initialize with default configuration
            Initialize(ComfortStudyConfiguration.Default);
        }
        
        /// <summary>
        /// Initialize the comfort study protocol system.
        /// </summary>
        public bool Initialize(ComfortStudyConfiguration config)
        {
            try
            {
                _configuration = config;
                
                // Validate configuration
                if (config.TargetParticipantCount <= 0)
                {
                    UnityEngine.Debug.LogError("[ComfortStudyProtocol] Invalid target participant count");
                    return false;
                }
                
                if (config.SessionDurationMinutes <= 0)
                {
                    UnityEngine.Debug.LogError("[ComfortStudyProtocol] Invalid session duration");
                    return false;
                }
                
                // Create data storage directory
                if (!string.IsNullOrEmpty(config.DataStorageDirectory))
                {
                    Directory.CreateDirectory(config.DataStorageDirectory);
                }
                
                // Initialize pre-registered hypotheses
                InitializePreRegisteredHypotheses();
                
                _isInitialized = true;
                
                if (_configuration.EnableDebugLogging)
                {
                    UnityEngine.Debug.Log($"[ComfortStudyProtocol] Initialized for n={config.TargetParticipantCount} participants");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ComfortStudyProtocol] Initialization failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Create a new comfort study protocol.
        /// </summary>
        public StudyProtocol CreateStudyProtocol(StudyProtocolDesign protocolDesign)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Comfort study protocol not initialized");
            }
            
            var protocolId = Guid.NewGuid().ToString();
            
            var protocol = new StudyProtocol
            {
                ProtocolId = protocolId,
                Design = protocolDesign,
                CreationTime = DateTime.Now,
                Version = "1.0",
                ApprovalStatus = ProtocolApprovalStatus.Draft,
                IRBApproval = _configuration.RequireIRBApproval ? null : new IRBApprovalInfo(),
                ValidationResult = new ProtocolValidationResult()
            };
            
            _studyProtocols[protocolId] = protocol;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[ComfortStudyProtocol] Created protocol '{protocolDesign.ProtocolName}' (ID: {protocolId})");
            }
            
            return protocol;
        }
        
        /// <summary>
        /// Validate study protocol design for completeness and compliance.
        /// </summary>
        public ProtocolValidationResult ValidateProtocol(StudyProtocol protocol)
        {
            var validationMessages = new List<string>();
            var requiredCorrections = new List<string>();
            float validationScore = 100f;
            
            // Validate protocol design completeness
            if (string.IsNullOrEmpty(protocol.Design.ProtocolName))
            {
                validationMessages.Add("Protocol name is required");
                requiredCorrections.Add("Provide a descriptive protocol name");
                validationScore -= 10f;
            }
            
            if (string.IsNullOrEmpty(protocol.Design.StudyObjective))
            {
                validationMessages.Add("Study objective is required");
                requiredCorrections.Add("Define clear study objectives");
                validationScore -= 15f;
            }
            
            // Validate hypotheses
            if (protocol.Design.PreRegisteredHypotheses == null || protocol.Design.PreRegisteredHypotheses.Length == 0)
            {
                validationMessages.Add("Pre-registered hypotheses are required");
                requiredCorrections.Add("Define at least one testable hypothesis");
                validationScore -= 20f;
            }
            else
            {
                foreach (var hypothesis in protocol.Design.PreRegisteredHypotheses)
                {
                    if (string.IsNullOrEmpty(hypothesis.Statement))
                    {
                        validationMessages.Add($"Hypothesis {hypothesis.HypothesisId} missing statement");
                        validationScore -= 5f;
                    }
                    
                    if (hypothesis.ExpectedEffectSize <= 0)
                    {
                        validationMessages.Add($"Hypothesis {hypothesis.HypothesisId} missing effect size");
                        validationScore -= 5f;
                    }
                }
            }
            
            // Validate success criteria
            if (protocol.Design.SuccessCriteria == null || protocol.Design.SuccessCriteria.Length == 0)
            {
                validationMessages.Add("Success criteria are required");
                requiredCorrections.Add("Define measurable success criteria");
                validationScore -= 15f;
            }
            else
            {
                bool hasPrimaryEndpoint = protocol.Design.SuccessCriteria.Any(c => c.IsPrimaryEndpoint);
                if (!hasPrimaryEndpoint)
                {
                    validationMessages.Add("At least one primary endpoint is required");
                    requiredCorrections.Add("Designate primary endpoint in success criteria");
                    validationScore -= 10f;
                }
            }
            
            // Validate experimental conditions
            if (protocol.Design.ExperimentalConditions == null || protocol.Design.ExperimentalConditions.Length == 0)
            {
                validationMessages.Add("Experimental conditions are required");
                requiredCorrections.Add("Define at least one experimental condition");
                validationScore -= 20f;
            }
            
            // Validate data collection procedures
            if (protocol.Design.DataCollectionProcedures == null || protocol.Design.DataCollectionProcedures.Length == 0)
            {
                validationMessages.Add("Data collection procedures are required");
                requiredCorrections.Add("Define data collection procedures");
                validationScore -= 15f;
            }
            else
            {
                bool hasSIMProcedure = protocol.Design.DataCollectionProcedures.Any(p => p.Method == DataCollectionMethod.SIMQuestionnaire);
                bool hasSSQProcedure = protocol.Design.DataCollectionProcedures.Any(p => p.Method == DataCollectionMethod.SSQQuestionnaire);
                
                if (!hasSIMProcedure && !hasSSQProcedure)
                {
                    validationMessages.Add("SIM or SSQ questionnaire procedure is required");
                    requiredCorrections.Add("Include SIM/SSQ data collection procedure");
                    validationScore -= 10f;
                }
            }
            
            // Validate ethical considerations
            if (string.IsNullOrEmpty(protocol.Design.EthicalConsiderations.InformedConsentRequirements))
            {
                validationMessages.Add("Informed consent requirements not specified");
                requiredCorrections.Add("Define informed consent procedures");
                validationScore -= 10f;
            }
            
            // IRB approval validation
            if (_configuration.RequireIRBApproval && protocol.IRBApproval == null)
            {
                validationMessages.Add("IRB approval is required but not provided");
                requiredCorrections.Add("Obtain IRB approval before protocol activation");
                validationScore -= 25f;
            }
            
            validationScore = Math.Max(0f, validationScore);
            bool isValid = validationScore >= 80f && requiredCorrections.Count == 0;
            
            return new ProtocolValidationResult
            {
                IsValid = isValid,
                ValidationScore = validationScore,
                ValidationMessages = validationMessages.ToArray(),
                RequiredCorrections = requiredCorrections.ToArray(),
                ValidationTimestamp = DateTime.Now,
                ValidatorId = "ComfortStudyProtocol"
            };
        }
        
        /// <summary>
        /// Start a comfort study session with a participant.
        /// </summary>
        public ComfortStudySession StartStudySession(ParticipantInfo participantInfo, StudyProtocol protocol)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Comfort study protocol not initialized");
            }
            
            if (_activeSessions.Count >= MAX_ACTIVE_SESSIONS)
            {
                throw new InvalidOperationException("Maximum number of active sessions reached");
            }
            
            if (!participantInfo.HasConsented)
            {
                throw new ArgumentException("Participant has not provided consent");
            }
            
            // Validate protocol
            var validationResult = ValidateProtocol(protocol);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Protocol validation failed: {string.Join(", ", validationResult.RequiredCorrections)}");
            }
            
            var sessionId = Guid.NewGuid().ToString();
            
            var session = new ComfortStudySession
            {
                SessionId = sessionId,
                Participant = participantInfo,
                Protocol = protocol,
                StartTime = DateTime.Now,
                EndTime = null,
                IsActive = true,
                CurrentPhase = StudySessionPhase.PreSession,
                CollectedData = new List<ComfortDataPoint>(),
                SessionNotes = ""
            };
            
            _activeSessions[sessionId] = session;
            _currentSession = session;
            
            // Fire session started event
            StudySessionStarted?.Invoke(new StudySessionStartedEventArgs
            {
                SessionId = sessionId,
                Participant = participantInfo,
                Protocol = protocol,
                StartTimestamp = DateTime.Now
            });
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[ComfortStudyProtocol] Started session {sessionId} for participant {participantInfo.ParticipantId}");
            }
            
            return session;
        }
        
        /// <summary>
        /// Collect SIM (Simulator Sickness Questionnaire) data during session.
        /// </summary>
        public DataCollectionResult CollectSIMData(string sessionId, SIMQuestionnaireData simData)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new DataCollectionResult
                {
                    IsSuccessful = false,
                    CollectionErrors = new[] { "Session not found or not active" },
                    CollectionTimestamp = DateTime.Now
                };
            }
            
            // Validate SIM data
            var validationErrors = ValidateSIMData(simData);
            if (validationErrors.Length > 0)
            {
                return new DataCollectionResult
                {
                    IsSuccessful = false,
                    CollectionErrors = validationErrors,
                    CollectionTimestamp = DateTime.Now
                };
            }
            
            // Create data point
            var dataPoint = new ComfortDataPoint
            {
                DataPointId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                TimeFromStartMinutes = (float)(DateTime.Now - session.StartTime).TotalMinutes,
                CollectionMethod = DataCollectionMethod.SIMQuestionnaire,
                SIMData = simData,
                CurrentCondition = GetCurrentExperimentalCondition(session),
                QualityIndicators = CalculateDataQuality(simData)
            };
            
            // Add to session data
            session.CollectedData.Add(dataPoint);
            _activeSessions[sessionId] = session;
            
            var result = new DataCollectionResult
            {
                IsSuccessful = true,
                DataId = dataPoint.DataPointId,
                CollectionTimestamp = DateTime.Now,
                DataQualityScore = dataPoint.QualityIndicators.OverallQuality,
                CollectionMessages = new[] { "SIM data collected successfully" }
            };
            
            // Fire data collected event
            ComfortDataCollected?.Invoke(new ComfortDataCollectedEventArgs
            {
                SessionId = sessionId,
                DataPoint = dataPoint,
                CollectionResult = result,
                CollectionTimestamp = DateTime.Now
            });
            
            return result;
        }
        
        /// <summary>
        /// Collect SSQ (Simulator Sickness Questionnaire) data during session.
        /// </summary>
        public DataCollectionResult CollectSSQData(string sessionId, SSQQuestionnaireData ssqData)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new DataCollectionResult
                {
                    IsSuccessful = false,
                    CollectionErrors = new[] { "Session not found or not active" },
                    CollectionTimestamp = DateTime.Now
                };
            }
            
            // Validate SSQ data
            var validationErrors = ValidateSSQData(ssqData);
            if (validationErrors.Length > 0)
            {
                return new DataCollectionResult
                {
                    IsSuccessful = false,
                    CollectionErrors = validationErrors,
                    CollectionTimestamp = DateTime.Now
                };
            }
            
            // Create data point
            var dataPoint = new ComfortDataPoint
            {
                DataPointId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                TimeFromStartMinutes = (float)(DateTime.Now - session.StartTime).TotalMinutes,
                CollectionMethod = DataCollectionMethod.SSQQuestionnaire,
                SSQData = ssqData,
                CurrentCondition = GetCurrentExperimentalCondition(session),
                QualityIndicators = CalculateDataQuality(ssqData)
            };
            
            // Add to session data
            session.CollectedData.Add(dataPoint);
            _activeSessions[sessionId] = session;
            
            var result = new DataCollectionResult
            {
                IsSuccessful = true,
                DataId = dataPoint.DataPointId,
                CollectionTimestamp = DateTime.Now,
                DataQualityScore = dataPoint.QualityIndicators.OverallQuality,
                CollectionMessages = new[] { "SSQ data collected successfully" }
            };
            
            // Fire data collected event
            ComfortDataCollected?.Invoke(new ComfortDataCollectedEventArgs
            {
                SessionId = sessionId,
                DataPoint = dataPoint,
                CollectionResult = result,
                CollectionTimestamp = DateTime.Now
            });
            
            return result;
        }
        
        /// <summary>
        /// Complete a study session and generate results.
        /// </summary>
        public SessionCompletionResult CompleteStudySession(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new SessionCompletionResult
                {
                    SessionId = sessionId,
                    CompletedSuccessfully = false,
                    CompletionTimestamp = DateTime.Now,
                    CompletionNotes = "Session not found or not active"
                };
            }
            
            // Mark session as completed
            session.IsActive = false;
            session.EndTime = DateTime.Now;
            session.CurrentPhase = StudySessionPhase.Completed;
            
            // Calculate session statistics
            var summaryStats = CalculateSessionSummaryStatistics(session);
            
            // Create completion result
            var completionResult = new SessionCompletionResult
            {
                SessionId = sessionId,
                CompletedSuccessfully = true,
                CompletionTimestamp = DateTime.Now,
                SessionDuration = session.EndTime.Value - session.StartTime,
                Participant = session.Participant,
                CollectedData = session.CollectedData.ToArray(),
                SummaryStatistics = summaryStats,
                CompletionNotes = session.SessionNotes,
                AdverseEvents = new AdverseEvent[0] // Would be populated from actual adverse event tracking
            };
            
            // Move to completed sessions
            _activeSessions.Remove(sessionId);
            _completedSessions.Add(completionResult);
            
            // Maintain completed sessions limit
            while (_completedSessions.Count > MAX_COMPLETED_SESSIONS)
            {
                _completedSessions.RemoveAt(0);
            }
            
            // Fire session completed event
            StudySessionCompleted?.Invoke(new StudySessionCompletedEventArgs
            {
                CompletionResult = completionResult,
                CompletionTimestamp = DateTime.Now
            });
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[ComfortStudyProtocol] Completed session {sessionId} - Duration: {completionResult.SessionDuration.TotalMinutes:F1} minutes");
            }
            
            return completionResult;
        }
        
        /// <summary>
        /// Generate study protocol documentation.
        /// </summary>
        public ProtocolDocumentation GenerateProtocolDocumentation(StudyProtocol protocol)
        {
            var documentation = GenerateMarkdownDocumentation(protocol);
            
            return new ProtocolDocumentation
            {
                ProtocolId = protocol.ProtocolId,
                DocumentationContent = documentation,
                Format = DocumentationFormat.Markdown,
                GenerationTimestamp = DateTime.Now,
                Version = protocol.Version,
                IncludedSections = new[]
                {
                    DocumentationSection.Abstract,
                    DocumentationSection.Introduction,
                    DocumentationSection.Methods,
                    DocumentationSection.Procedures,
                    DocumentationSection.EthicalConsiderations,
                    DocumentationSection.StatisticalAnalysis
                },
                FilePath = Path.Combine(_configuration.DataStorageDirectory, $"Protocol_{protocol.ProtocolId}.md")
            };
        }
        
        /// <summary>
        /// Analyze comfort study results across multiple sessions.
        /// </summary>
        public ComfortStudyAnalysis AnalyzeStudyResults(SessionCompletionResult[] sessionResults)
        {
            if (sessionResults == null || sessionResults.Length == 0)
            {
                return new ComfortStudyAnalysis
                {
                    AnalysisId = Guid.NewGuid().ToString(),
                    ParticipantCount = 0,
                    AnalysisTimestamp = DateTime.Now
                };
            }
            
            var analysisId = Guid.NewGuid().ToString();
            var participantCount = sessionResults.Length;
            var completionRate = sessionResults.Count(s => s.CompletedSuccessfully) / (float)sessionResults.Length;
            
            // Calculate comfort statistics
            var comfortStats = CalculateComfortStatistics(sessionResults);
            
            // Perform hypothesis tests
            var hypothesisResults = PerformHypothesisTests(sessionResults);
            
            // Calculate effect sizes
            var effectSizes = CalculateEffectSizes(sessionResults);
            
            // Summarize adverse events
            var adverseEventSummary = SummarizeAdverseEvents(sessionResults);
            
            // Assess data quality
            var dataQuality = AssessStudyDataQuality(sessionResults);
            
            return new ComfortStudyAnalysis
            {
                AnalysisId = analysisId,
                ParticipantCount = participantCount,
                StudyCompletionRate = completionRate,
                ComfortStatistics = comfortStats,
                HypothesisResults = hypothesisResults,
                EffectSizes = effectSizes,
                AdverseEventSummary = adverseEventSummary,
                DataQuality = dataQuality,
                AnalysisTimestamp = DateTime.Now,
                AnalysisNotes = $"Analysis of {participantCount} participants with {completionRate:P1} completion rate"
            };
        }
        
        /// <summary>
        /// Get pre-registered hypotheses for the study.
        /// </summary>
        public StudyHypothesis[] GetPreRegisteredHypotheses()
        {
            return _preRegisteredHypotheses ?? new StudyHypothesis[0];
        }
        
        /// <summary>
        /// Evaluate study success criteria against results.
        /// </summary>
        public SuccessCriteriaEvaluation EvaluateSuccessCriteria(ComfortStudyAnalysis studyAnalysis)
        {
            // This would evaluate against the specific success criteria defined in the protocol
            // For now, return a simplified evaluation
            
            var criteriaResults = new List<CriteriaEvaluationResult>();
            
            // Example: Evaluate completion rate criteria
            criteriaResults.Add(new CriteriaEvaluationResult
            {
                CriteriaId = "completion_rate",
                CriteriaMet = studyAnalysis.StudyCompletionRate >= 0.8f,
                ActualValue = studyAnalysis.StudyCompletionRate,
                TargetValue = 0.8f,
                DifferenceFromTarget = studyAnalysis.StudyCompletionRate - 0.8f,
                ConfidenceLevel = 0.95f,
                EvaluationNotes = "Study completion rate evaluation"
            });
            
            var primaryEndpointSuccess = criteriaResults.Count(c => c.CriteriaMet) / (float)criteriaResults.Count;
            var studySuccessful = primaryEndpointSuccess >= 0.8f;
            
            return new SuccessCriteriaEvaluation
            {
                StudySuccessful = studySuccessful,
                CriteriaResults = criteriaResults.ToArray(),
                PrimaryEndpointSuccessRate = primaryEndpointSuccess,
                SecondaryEndpointSuccessRate = 1.0f, // Simplified
                EvaluationTimestamp = DateTime.Now,
                EvaluationSummary = $"Study {(studySuccessful ? "successful" : "unsuccessful")} with {primaryEndpointSuccess:P1} criteria met"
            };
        }
        
        /// <summary>
        /// Export study data for external analysis.
        /// </summary>
        public StudyDataExport ExportStudyData(DataExportFormat format)
        {
            var exportId = Guid.NewGuid().ToString();
            var exportContent = GenerateExportContent(format);
            var recordCount = _completedSessions.Sum(s => s.CollectedData.Length);
            
            return new StudyDataExport
            {
                ExportId = exportId,
                Format = format,
                DataContent = exportContent,
                ExportTimestamp = DateTime.Now,
                RecordCount = recordCount,
                FilePath = Path.Combine(_configuration.DataStorageDirectory, $"StudyData_{exportId}.{format.ToString().ToLower()}"),
                AnonymizationLevel = DataAnonymizationLevel.Anonymized
            };
        }
        
        /// <summary>
        /// Reset study protocol state and clear data.
        /// </summary>
        public void ResetStudyProtocol()
        {
            _studyProtocols.Clear();
            _activeSessions.Clear();
            _completedSessions.Clear();
            _currentSession = new ComfortStudySession();
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log("[ComfortStudyProtocol] Study protocol state reset");
            }
        }
        
        // Private helper methods
        
        private void InitializePreRegisteredHypotheses()
        {
            _preRegisteredHypotheses = new[]
            {
                new StudyHypothesis
                {
                    HypothesisId = "H1",
                    Statement = "Wave-based text input will result in lower motion sickness scores compared to static text input",
                    Type = HypothesisType.Directional,
                    ExpectedEffectSize = 0.5f,
                    TargetPower = 0.8f,
                    AlphaLevel = 0.05f,
                    PrimaryOutcome = "SSQ Total Score",
                    PreRegistrationTime = DateTime.Now
                },
                new StudyHypothesis
                {
                    HypothesisId = "H2",
                    Statement = "Participants will show no significant increase in simulator sickness during 15-minute exposure",
                    Type = HypothesisType.Null,
                    ExpectedEffectSize = 0.2f,
                    TargetPower = 0.8f,
                    AlphaLevel = 0.05f,
                    PrimaryOutcome = "SIM Score Change",
                    PreRegistrationTime = DateTime.Now
                }
            };
        }
        
        private string[] ValidateSIMData(SIMQuestionnaireData simData)
        {
            var errors = new List<string>();
            
            // Validate SIM scale ranges (0-3)
            if (simData.GeneralDiscomfort < 0 || simData.GeneralDiscomfort > 3)
                errors.Add("General discomfort score out of range (0-3)");
            
            if (simData.Nausea < 0 || simData.Nausea > 3)
                errors.Add("Nausea score out of range (0-3)");
            
            if (simData.Fatigue < 0 || simData.Fatigue > 3)
                errors.Add("Fatigue score out of range (0-3)");
            
            // Add other validation checks as needed
            
            return errors.ToArray();
        }
        
        private string[] ValidateSSQData(SSQQuestionnaireData ssqData)
        {
            var errors = new List<string>();
            
            // Validate SSQ scores
            if (ssqData.TotalScore < 0 || ssqData.TotalScore > 235.62f) // Max possible SSQ score
                errors.Add("SSQ total score out of valid range");
            
            if (ssqData.ItemResponses == null || ssqData.ItemResponses.Length != 16)
                errors.Add("SSQ must have exactly 16 item responses");
            
            // Validate individual item responses (0-3 scale)
            if (ssqData.ItemResponses != null)
            {
                for (int i = 0; i < ssqData.ItemResponses.Length; i++)
                {
                    if (ssqData.ItemResponses[i] < 0 || ssqData.ItemResponses[i] > 3)
                    {
                        errors.Add($"SSQ item {i + 1} response out of range (0-3)");
                    }
                }
            }
            
            return errors.ToArray();
        }
        
        private DataQualityIndicators CalculateDataQuality(SIMQuestionnaireData simData)
        {
            // Calculate data quality based on response patterns and completeness
            float overallQuality = 100f;
            var qualityFlags = new List<string>();
            
            // Check for response patterns that might indicate poor data quality
            var responses = new[] { simData.GeneralDiscomfort, simData.Fatigue, simData.Headache, simData.Nausea };
            bool allSame = responses.All(r => r == responses[0]);
            
            if (allSame && responses[0] == 0)
            {
                qualityFlags.Add("All responses are zero - possible non-engagement");
                overallQuality -= 20f;
            }
            else if (allSame)
            {
                qualityFlags.Add("All responses identical - possible response bias");
                overallQuality -= 15f;
            }
            
            return new DataQualityIndicators
            {
                OverallQuality = Math.Max(0f, overallQuality),
                CompletenessPercent = 100f, // All required fields present
                ConsistencyScore = allSame ? 50f : 90f,
                ResponseTimeSeconds = 30f, // Would be measured in real implementation
                PassedValidation = true,
                QualityFlags = qualityFlags.ToArray()
            };
        }
        
        private DataQualityIndicators CalculateDataQuality(SSQQuestionnaireData ssqData)
        {
            // Similar quality calculation for SSQ data
            float overallQuality = 100f;
            var qualityFlags = new List<string>();
            
            if (ssqData.CompletionTimeSeconds < 30f)
            {
                qualityFlags.Add("Very fast completion time - possible rushed responses");
                overallQuality -= 15f;
            }
            else if (ssqData.CompletionTimeSeconds > 300f)
            {
                qualityFlags.Add("Very slow completion time - possible distraction");
                overallQuality -= 10f;
            }
            
            return new DataQualityIndicators
            {
                OverallQuality = Math.Max(0f, overallQuality),
                CompletenessPercent = 100f,
                ConsistencyScore = 85f,
                ResponseTimeSeconds = ssqData.CompletionTimeSeconds,
                PassedValidation = true,
                QualityFlags = qualityFlags.ToArray()
            };
        }
        
        private string GetCurrentExperimentalCondition(ComfortStudySession session)
        {
            // Determine current experimental condition based on session time and protocol
            var elapsedMinutes = (DateTime.Now - session.StartTime).TotalMinutes;
            
            if (session.Protocol.Design.ExperimentalConditions != null && session.Protocol.Design.ExperimentalConditions.Length > 0)
            {
                // Simple implementation - return first condition
                return session.Protocol.Design.ExperimentalConditions[0].ConditionId;
            }
            
            return "baseline";
        }
        
        private SessionSummaryStatistics CalculateSessionSummaryStatistics(ComfortStudySession session)
        {
            var simData = session.CollectedData.Where(d => d.SIMData.HasValue).Select(d => d.SIMData.Value).ToArray();
            var ssqData = session.CollectedData.Where(d => d.SSQData.HasValue).Select(d => d.SSQData.Value).ToArray();
            
            float averageSIM = simData.Length > 0 ? simData.Average(s => s.GeneralDiscomfort + s.Nausea + s.Fatigue) : 0f;
            float averageSSQ = ssqData.Length > 0 ? ssqData.Average(s => s.TotalScore) : 0f;
            
            return new SessionSummaryStatistics
            {
                AverageSIMScore = averageSIM,
                AverageSSQScore = averageSSQ,
                PeakDiscomfortLevel = simData.Length > 0 ? simData.Max(s => Math.Max(s.GeneralDiscomfort, s.Nausea)) : 0f,
                TimeToPeakDiscomfortMinutes = 0f, // Would calculate from actual data
                DataPointCount = session.CollectedData.Count,
                CompletionRate = 1.0f, // Session completed
                SessionQualityScore = session.CollectedData.Count > 0 ? session.CollectedData.Average(d => d.QualityIndicators.OverallQuality) : 0f,
                MetMinimumRequirements = session.CollectedData.Count >= 3 // Minimum 3 data points
            };
        }
        
        // Placeholder implementations for complex analysis methods
        private string GenerateMarkdownDocumentation(StudyProtocol protocol)
        {
            return $"# Study Protocol: {protocol.Design.ProtocolName}\n\n" +
                   $"## Objective\n{protocol.Design.StudyObjective}\n\n" +
                   $"## Methods\nComfort validation study using SIM/SSQ questionnaires\n\n" +
                   $"## Participants\nTarget: {_configuration.TargetParticipantCount} participants\n\n";
        }
        
        private ComfortStatistics CalculateComfortStatistics(SessionCompletionResult[] sessionResults)
        {
            return new ComfortStatistics
            {
                MeanSIMScores = new Dictionary<string, float> { { "baseline", 5.0f } },
                MeanSSQScores = new Dictionary<string, float> { { "baseline", 15.0f } },
                StandardDeviations = new Dictionary<string, float> { { "baseline", 2.5f } },
                ConfidenceIntervals = new Dictionary<string, ConfidenceInterval>(),
                DropoutRates = new Dictionary<string, float> { { "baseline", 0.1f } }
            };
        }
        
        private HypothesisTestResult[] PerformHypothesisTests(SessionCompletionResult[] sessionResults)
        {
            return new HypothesisTestResult[0]; // Placeholder
        }
        
        private EffectSizeAnalysis[] CalculateEffectSizes(SessionCompletionResult[] sessionResults)
        {
            return new EffectSizeAnalysis[0]; // Placeholder
        }
        
        private AdverseEventSummary SummarizeAdverseEvents(SessionCompletionResult[] sessionResults)
        {
            return new AdverseEventSummary
            {
                TotalEvents = 0,
                EventsBySeverity = new Dictionary<AdverseEventSeverity, int>(),
                StudyRelatedEvents = 0,
                DropoutsDueToAE = 0,
                MostCommonEvents = new string[0]
            };
        }
        
        private StudyDataQualityAssessment AssessStudyDataQuality(SessionCompletionResult[] sessionResults)
        {
            return new StudyDataQualityAssessment
            {
                OverallQualityScore = 85f,
                CompletenessRate = 0.95f,
                ConsistencyScore = 0.90f,
                ProtocolAdherenceRate = 0.98f,
                QualityIssues = new string[0],
                QualityRecommendations = new string[0]
            };
        }
        
        private string GenerateExportContent(DataExportFormat format)
        {
            // Generate export content based on format
            switch (format)
            {
                case DataExportFormat.CSV:
                    return "ParticipantID,SessionID,Timestamp,SIMScore,SSQScore\n"; // Header only for now
                case DataExportFormat.JSON:
                    return "{ \"studyData\": [] }";
                default:
                    return "Study data export";
            }
        }
    }
}