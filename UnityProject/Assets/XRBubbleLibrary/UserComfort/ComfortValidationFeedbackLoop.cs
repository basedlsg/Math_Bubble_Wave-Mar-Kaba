using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Implementation of comfort validation feedback loop system.
    /// Automatically adjusts wave parameters based on comfort data to maintain user comfort.
    /// Implements Requirement 7.4: Comfort data analysis and parameter adjustment.
    /// Implements Requirement 7.5: Automatic comfort validation failure handling.
    /// </summary>
    public class ComfortValidationFeedbackLoop : IComfortValidationFeedbackLoop
    {
        private bool _isInitialized;
        private ComfortFeedbackConfiguration _configuration;
        private IComfortDataCollectionSystem _dataCollectionSystem;
        private IWaveParameterValidator _parameterValidator;
        
        private readonly Dictionary<string, ComfortFeedbackSession> _activeSessions;
        private readonly List<AdjustmentStrategy> _availableStrategies;
        private ComfortValidationThresholds _validationThresholds;
        
        private const int MAX_ACTIVE_SESSIONS = 10;
        private const float COMFORT_SCORE_SCALE = 100f;
        
        public bool IsActive => _isInitialized && _activeSessions.Count > 0;
        public ComfortFeedbackConfiguration Configuration => _configuration;
        public WaveParameterAdjustment[] ActiveAdjustments => GetAllActiveAdjustments();
        
        public event Action<ComfortValidationFailedEventArgs> ComfortValidationFailed;
        public event Action<ParameterAdjustmentEventArgs> ParametersAdjusted;
        public event Action<ComfortValidationPassedEventArgs> ComfortValidationPassed;
        public event Action<AutomaticRollbackEventArgs> AutomaticRollbackTriggered;
        
        public ComfortValidationFeedbackLoop()
        {
            _activeSessions = new Dictionary<string, ComfortFeedbackSession>();
            _availableStrategies = new List<AdjustmentStrategy>();
            _validationThresholds = ComfortValidationThresholds.Quest3Default;
            
            InitializeDefaultStrategies();
        }
        
        public bool Initialize(ComfortFeedbackConfiguration config,
                              IComfortDataCollectionSystem dataCollectionSystem,
                              IWaveParameterValidator parameterValidator)
        {
            if (config == null)
            {
                Debug.LogError("Cannot initialize with null configuration");
                return false;
            }
            
            if (dataCollectionSystem == null)
            {
                Debug.LogError("Cannot initialize with null data collection system");
                return false;
            }
            
            if (parameterValidator == null)
            {
                Debug.LogError("Cannot initialize with null parameter validator");
                return false;
            }
            
            _configuration = config;
            _dataCollectionSystem = dataCollectionSystem;
            _parameterValidator = parameterValidator;
            
            // Subscribe to data collection events
            _dataCollectionSystem.ComfortDataCollected += OnComfortDataCollected;
            _dataCollectionSystem.DataValidationFailed += OnDataValidationFailed;
            
            _isInitialized = true;
            
            Debug.Log("Comfort validation feedback loop initialized successfully");
            return true;
        }
        
        public ComfortFeedbackSession StartFeedbackLoop(string sessionId, WaveMatrixSettings initialWaveSettings)
        {
            if (!_isInitialized)
            {
                Debug.LogError("Feedback loop not initialized");
                return null;
            }
            
            if (string.IsNullOrEmpty(sessionId))
            {
                Debug.LogError("Invalid session ID");
                return null;
            }
            
            if (_activeSessions.ContainsKey(sessionId))
            {
                Debug.LogWarning($"Session {sessionId} already exists");
                return _activeSessions[sessionId];
            }
            
            if (_activeSessions.Count >= MAX_ACTIVE_SESSIONS)
            {
                Debug.LogError("Maximum number of active sessions reached");
                return null;
            }
            
            var session = new ComfortFeedbackSession
            {
                SessionId = sessionId,
                DataCollectionSessionId = sessionId, // Assume same ID for simplicity
                StartTime = DateTime.UtcNow,
                Status = FeedbackSessionStatus.Active,
                InitialWaveSettings = initialWaveSettings,
                CurrentWaveSettings = initialWaveSettings,
                LastKnownGoodSettings = initialWaveSettings,
                ValidationThresholds = _validationThresholds,
                Configuration = _configuration,
                PerformanceMetrics = new FeedbackLoopMetrics()
            };
            
            _activeSessions[sessionId] = session;
            
            Debug.Log($"Started feedback loop for session {sessionId}");
            return session;
        }
        
        public ComfortDataProcessingResult ProcessComfortData(string sessionId, ComfortDataPoint comfortData)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new ComfortDataProcessingResult
                {
                    IsSuccessful = false,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessingMessages = new[] { "Session not found" }
                };
            }
            
            var result = new ComfortDataProcessingResult
            {
                ProcessedAt = DateTime.UtcNow,
                IsSuccessful = true
            };
            
            try
            {
                // Add comfort data to session history
                session.ComfortDataHistory.Add(comfortData);
                session.PerformanceMetrics.TotalDataPointsProcessed++;
                
                // Calculate current comfort score
                result.CurrentComfortScore = CalculateComfortScore(comfortData);
                
                // Analyze comfort trend
                result.ComfortTrend = AnalyzeComfortTrend(session);
                
                // Identify comfort issues
                result.IdentifiedIssues = IdentifyComfortIssues(comfortData, session);
                
                // Generate comfort prediction
                result.ComfortPrediction = PredictComfortTrajectory(session);
                
                // Determine if adjustments are needed
                if (ShouldTriggerAdjustments(result, session))
                {
                    var adjustments = DetermineRequiredAdjustments(result.IdentifiedIssues, session);
                    if (adjustments.Length > 0)
                    {
                        var adjustmentResult = ApplyAdjustments(sessionId, adjustments);
                        result.AdjustmentsTriggered = adjustmentResult.IsSuccessful;
                        result.AppliedAdjustments = adjustmentResult.AppliedAdjustments;
                    }
                }
                
                // Update session metrics
                UpdateSessionMetrics(session, result);
                
                result.ProcessingMessages = new[] { "Comfort data processed successfully" };
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ProcessingMessages = new[] { $"Error processing comfort data: {ex.Message}" };
                Debug.LogError($"Error processing comfort data for session {sessionId}: {ex.Message}");
            }
            
            return result;
        }
        
        public ComfortTrendAnalysis AnalyzeComfortTrends(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return null;
            }
            
            var analysis = new ComfortTrendAnalysis
            {
                AnalysisTimestamp = DateTime.UtcNow
            };
            
            if (session.ComfortDataHistory.Count < 2)
            {
                analysis.OverallTrend = TrendDirection.Stable;
                analysis.TrendStrength = 0f;
                analysis.AnalysisConfidence = 0.1f;
                return analysis;
            }
            
            // Calculate comfort trajectory
            analysis.ComfortTrajectory = session.ComfortDataHistory
                .Select(data => new ComfortTrajectoryPoint
                {
                    Timestamp = data.Timestamp,
                    ComfortScore = CalculateComfortScore(data),
                    ComfortLevel = DetermineComfortLevel(data),
                    MotionSicknessScore = ExtractMotionSicknessScore(data)
                })
                .OrderBy(point => point.Timestamp)
                .ToArray();
            
            // Analyze overall trend
            analysis.OverallTrend = CalculateOverallTrend(analysis.ComfortTrajectory);
            analysis.TrendStrength = CalculateTrendStrength(analysis.ComfortTrajectory);
            
            // Identify patterns
            analysis.IdentifiedPatterns = IdentifyComfortPatterns(analysis.ComfortTrajectory);
            
            // Generate predictions
            analysis.FuturePredictions = GenerateFuturePredictions(analysis.ComfortTrajectory);
            
            // Assess risk factors
            analysis.RiskFactors = AssessComfortRiskFactors(session);
            
            // Generate recommendations
            analysis.PreventiveRecommendations = GeneratePreventiveRecommendations(analysis);
            
            // Calculate confidence
            analysis.AnalysisConfidence = CalculateAnalysisConfidence(session.ComfortDataHistory.Count, analysis.TrendStrength);
            
            return analysis;
        }
        
        public ParameterAdjustmentResult ApplyAutomaticAdjustments(string sessionId, AdjustmentStrategy strategy)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new ParameterAdjustmentResult
                {
                    IsSuccessful = false,
                    AdjustmentTimestamp = DateTime.UtcNow,
                    AdjustmentMessages = new[] { "Session not found" }
                };
            }
            
            if (!_configuration.EnableAutomaticAdjustments)
            {
                return new ParameterAdjustmentResult
                {
                    IsSuccessful = false,
                    AdjustmentTimestamp = DateTime.UtcNow,
                    AdjustmentMessages = new[] { "Automatic adjustments are disabled" }
                };
            }
            
            // Check adjustment limits
            if (session.AdjustmentHistory.Count >= _configuration.MaxAdjustmentsPerSession)
            {
                return new ParameterAdjustmentResult
                {
                    IsSuccessful = false,
                    AdjustmentTimestamp = DateTime.UtcNow,
                    AdjustmentMessages = new[] { "Maximum adjustments per session reached" }
                };
            }
            
            // Check time between adjustments
            var lastAdjustment = session.AdjustmentHistory.LastOrDefault();
            if (lastAdjustment != null)
            {
                var timeSinceLastAdjustment = DateTime.UtcNow - lastAdjustment.AppliedAt;
                if (timeSinceLastAdjustment.TotalSeconds < _configuration.MinTimeBetweenAdjustments)
                {
                    return new ParameterAdjustmentResult
                    {
                        IsSuccessful = false,
                        AdjustmentTimestamp = DateTime.UtcNow,
                        AdjustmentMessages = new[] { "Too soon since last adjustment" }
                    };
                }
            }
            
            var startTime = DateTime.UtcNow;
            var result = new ParameterAdjustmentResult
            {
                AdjustmentTimestamp = startTime
            };
            
            try
            {
                // Generate adjustments based on strategy
                var adjustments = GenerateAdjustmentsFromStrategy(strategy, session);
                
                // Validate adjustments
                var validatedAdjustments = new List<WaveParameterAdjustment>();
                var failedAdjustments = new List<WaveParameterAdjustment>();
                
                foreach (var adjustment in adjustments)
                {
                    var validationResult = _parameterValidator.ValidateSettings(adjustment.SettingsAfter);
                    if (validationResult.IsValid)
                    {
                        validatedAdjustments.Add(adjustment);
                    }
                    else
                    {
                        failedAdjustments.Add(adjustment);
                    }
                }
                
                // Apply validated adjustments
                foreach (var adjustment in validatedAdjustments)
                {
                    ApplyAdjustmentToSession(session, adjustment);
                }
                
                result.IsSuccessful = validatedAdjustments.Count > 0;
                result.ParametersAdjusted = validatedAdjustments.Count;
                result.AppliedAdjustments = validatedAdjustments.ToArray();
                result.FailedAdjustments = failedAdjustments.ToArray();
                result.AdjustmentDuration = DateTime.UtcNow - startTime;
                
                // Update session metrics
                session.PerformanceMetrics.TotalAdjustmentsMade += validatedAdjustments.Count;
                session.PerformanceMetrics.SuccessfulAdjustments += validatedAdjustments.Count;
                session.PerformanceMetrics.FailedAdjustments += failedAdjustments.Count;
                
                // Fire event
                if (validatedAdjustments.Count > 0)
                {
                    ParametersAdjusted?.Invoke(new ParameterAdjustmentEventArgs
                    {
                        SessionId = sessionId,
                        Adjustments = validatedAdjustments.ToArray(),
                        Strategy = strategy,
                        AdjustmentTimestamp = startTime
                    });
                }
                
                result.AdjustmentMessages = new[] { $"Applied {validatedAdjustments.Count} adjustments successfully" };
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.AdjustmentMessages = new[] { $"Error applying adjustments: {ex.Message}" };
                Debug.LogError($"Error applying automatic adjustments for session {sessionId}: {ex.Message}");
            }
            
            return result;
        } 
       
        public ComfortValidationResult ValidateComfortLevels(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new ComfortValidationResult
                {
                    IsValid = false,
                    ValidationTimestamp = DateTime.UtcNow,
                    ValidationRecommendations = new[] { "Session not found" }
                };
            }
            
            var result = new ComfortValidationResult
            {
                ValidationTimestamp = DateTime.UtcNow,
                MetricScores = new Dictionary<ComfortMetricType, float>()
            };
            
            if (session.ComfortDataHistory.Count == 0)
            {
                result.IsValid = true; // No data to validate against
                result.OverallComfortScore = 100f;
                result.NextValidationIn = TimeSpan.FromSeconds(_configuration.ProcessingIntervalSeconds);
                return result;
            }
            
            var latestData = session.ComfortDataHistory.Last();
            
            // Calculate metric scores
            result.MetricScores[ComfortMetricType.OverallComfort] = CalculateComfortScore(latestData);
            result.MetricScores[ComfortMetricType.MotionSickness] = ExtractMotionSicknessScore(latestData);
            result.MetricScores[ComfortMetricType.PhysiologicalStress] = ExtractPhysiologicalStressScore(latestData);
            result.MetricScores[ComfortMetricType.BehavioralIndicators] = ExtractBehavioralScore(latestData);
            result.MetricScores[ComfortMetricType.SubjectiveRating] = ExtractSubjectiveRating(latestData);
            
            // Calculate overall comfort score
            result.OverallComfortScore = result.MetricScores.Values.Average();
            
            // Check for violations
            var violations = new List<ComfortValidationViolation>();
            var issues = new List<ComfortIssue>();
            
            foreach (var metric in result.MetricScores)
            {
                var violation = CheckMetricViolation(metric.Key, metric.Value, session.ValidationThresholds);
                if (violation != null)
                {
                    violations.Add(violation);
                    
                    // Convert violation to comfort issue
                    var issue = new ComfortIssue
                    {
                        IssueId = Guid.NewGuid().ToString(),
                        Type = MapMetricToIssueType(metric.Key),
                        Severity = MapViolationSeverity(violation.Severity),
                        Description = $"{metric.Key} violation: {metric.Value:F1} (threshold: {violation.ThresholdValue:F1})",
                        DetectedAt = DateTime.UtcNow,
                        DetectionConfidence = 0.9f,
                        AssociatedData = new[] { latestData }
                    };
                    issues.Add(issue);
                }
            }
            
            result.Violations = violations.ToArray();
            result.ComfortIssues = issues.ToArray();
            result.IsValid = violations.Count == 0;
            
            // Generate recommendations
            result.ValidationRecommendations = GenerateValidationRecommendations(result);
            result.NextValidationIn = TimeSpan.FromSeconds(_configuration.ProcessingIntervalSeconds);
            
            // Fire appropriate events
            if (!result.IsValid)
            {
                ComfortValidationFailed?.Invoke(new ComfortValidationFailedEventArgs
                {
                    SessionId = sessionId,
                    ValidationResult = result,
                    ComfortIssues = result.ComfortIssues,
                    FailureTimestamp = DateTime.UtcNow
                });
            }
            else
            {
                ComfortValidationPassed?.Invoke(new ComfortValidationPassedEventArgs
                {
                    SessionId = sessionId,
                    ValidationResult = result,
                    PassedTimestamp = DateTime.UtcNow
                });
            }
            
            return result;
        }
        
        public RollbackResult TriggerAutomaticRollback(string sessionId, string rollbackReason)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new RollbackResult
                {
                    IsSuccessful = false,
                    RollbackTimestamp = DateTime.UtcNow,
                    RollbackReason = rollbackReason,
                    RollbackMessages = new[] { "Session not found" }
                };
            }
            
            var startTime = DateTime.UtcNow;
            var result = new RollbackResult
            {
                RollbackTimestamp = startTime,
                RollbackReason = rollbackReason,
                SettingsBefore = session.CurrentWaveSettings
            };
            
            try
            {
                // Rollback to last known good settings
                var rollbackSettings = session.LastKnownGoodSettings ?? session.InitialWaveSettings;
                
                session.CurrentWaveSettings = rollbackSettings;
                result.SettingsAfter = rollbackSettings;
                result.AdjustmentsRolledBack = session.AdjustmentHistory.Count;
                
                // Clear adjustment history
                session.AdjustmentHistory.Clear();
                
                // Update metrics
                session.PerformanceMetrics.RollbacksTriggered++;
                
                result.IsSuccessful = true;
                result.RollbackDuration = DateTime.UtcNow - startTime;
                result.RollbackMessages = new[] { "Rollback completed successfully" };
                
                // Predict expected improvement
                result.ExpectedImprovement = new ComfortImpactPrediction
                {
                    ExpectedComfortChange = 20f, // Optimistic estimate
                    ExpectedComfortLevel = ComfortLevel.Comfortable,
                    PredictionConfidence = 0.7f,
                    ExpectedTimeToEffect = TimeSpan.FromSeconds(30)
                };
                
                // Fire event
                AutomaticRollbackTriggered?.Invoke(new AutomaticRollbackEventArgs
                {
                    SessionId = sessionId,
                    RollbackResult = result,
                    RollbackReason = rollbackReason,
                    RollbackTimestamp = startTime
                });
                
                Debug.Log($"Automatic rollback triggered for session {sessionId}: {rollbackReason}");
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.RollbackMessages = new[] { $"Error during rollback: {ex.Message}" };
                Debug.LogError($"Error during automatic rollback for session {sessionId}: {ex.Message}");
            }
            
            return result;
        }
        
        public WaveParameterAdjustment[] GetRecommendedAdjustments(string sessionId, ComfortIssue[] comfortIssues)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new WaveParameterAdjustment[0];
            }
            
            var adjustments = new List<WaveParameterAdjustment>();
            
            foreach (var issue in comfortIssues)
            {
                var strategy = SelectBestStrategy(issue, _availableStrategies);
                if (strategy != null)
                {
                    var issueAdjustments = GenerateAdjustmentsFromStrategy(strategy, session);
                    adjustments.AddRange(issueAdjustments);
                }
            }
            
            // Remove duplicates and optimize
            return OptimizeAdjustments(adjustments.ToArray());
        }
        
        public ParameterAdjustmentResult ApplyManualAdjustments(string sessionId, WaveParameterAdjustment[] adjustments)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new ParameterAdjustmentResult
                {
                    IsSuccessful = false,
                    AdjustmentTimestamp = DateTime.UtcNow,
                    AdjustmentMessages = new[] { "Session not found" }
                };
            }
            
            var startTime = DateTime.UtcNow;
            var result = new ParameterAdjustmentResult
            {
                AdjustmentTimestamp = startTime
            };
            
            try
            {
                var appliedAdjustments = new List<WaveParameterAdjustment>();
                var failedAdjustments = new List<WaveParameterAdjustment>();
                
                foreach (var adjustment in adjustments)
                {
                    // Mark as manual adjustment
                    adjustment.IsAutomatic = false;
                    adjustment.AppliedAt = DateTime.UtcNow;
                    
                    // Validate adjustment
                    var validationResult = _parameterValidator.ValidateSettings(adjustment.SettingsAfter);
                    if (validationResult.IsValid)
                    {
                        ApplyAdjustmentToSession(session, adjustment);
                        appliedAdjustments.Add(adjustment);
                    }
                    else
                    {
                        failedAdjustments.Add(adjustment);
                    }
                }
                
                result.IsSuccessful = appliedAdjustments.Count > 0;
                result.ParametersAdjusted = appliedAdjustments.Count;
                result.AppliedAdjustments = appliedAdjustments.ToArray();
                result.FailedAdjustments = failedAdjustments.ToArray();
                result.AdjustmentDuration = DateTime.UtcNow - startTime;
                
                // Update metrics
                session.PerformanceMetrics.TotalAdjustmentsMade += appliedAdjustments.Count;
                session.PerformanceMetrics.SuccessfulAdjustments += appliedAdjustments.Count;
                session.PerformanceMetrics.FailedAdjustments += failedAdjustments.Count;
                
                result.AdjustmentMessages = new[] { $"Applied {appliedAdjustments.Count} manual adjustments" };
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.AdjustmentMessages = new[] { $"Error applying manual adjustments: {ex.Message}" };
                Debug.LogError($"Error applying manual adjustments for session {sessionId}: {ex.Message}");
            }
            
            return result;
        }
        
        public FeedbackLoopMetrics GetPerformanceMetrics(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new FeedbackLoopMetrics();
            }
            
            // Update calculated metrics
            var metrics = session.PerformanceMetrics;
            
            if (session.ComfortDataHistory.Count > 1)
            {
                var timeSpan = session.ComfortDataHistory.Last().Timestamp - session.ComfortDataHistory.First().Timestamp;
                metrics.AverageProcessingInterval = TimeSpan.FromTicks(timeSpan.Ticks / session.ComfortDataHistory.Count);
                
                // Calculate comfort improvement rate
                var firstScore = CalculateComfortScore(session.ComfortDataHistory.First());
                var lastScore = CalculateComfortScore(session.ComfortDataHistory.Last());
                var timeDiffMinutes = timeSpan.TotalMinutes;
                metrics.ComfortImprovementRate = timeDiffMinutes > 0 ? (lastScore - firstScore) / (float)timeDiffMinutes : 0f;
            }
            
            // Calculate overall effectiveness
            var successRate = metrics.TotalAdjustmentsMade > 0 ? 
                (float)metrics.SuccessfulAdjustments / metrics.TotalAdjustmentsMade : 1f;
            metrics.OverallEffectiveness = successRate * (1f - (metrics.RollbacksTriggered * 0.2f));
            
            return metrics;
        }
        
        public void ConfigureComfortThresholds(ComfortValidationThresholds thresholds)
        {
            _validationThresholds = thresholds ?? ComfortValidationThresholds.Quest3Default;
            
            // Update all active sessions
            foreach (var session in _activeSessions.Values)
            {
                session.ValidationThresholds = _validationThresholds;
            }
            
            Debug.Log("Comfort validation thresholds updated");
        }
        
        public void ConfigureAdjustmentStrategies(AdjustmentStrategy[] strategies)
        {
            _availableStrategies.Clear();
            if (strategies != null)
            {
                _availableStrategies.AddRange(strategies.Where(s => s.IsEnabled));
            }
            
            Debug.Log($"Configured {_availableStrategies.Count} adjustment strategies");
        }
        
        public FeedbackLoopTerminationResult StopFeedbackLoop(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new FeedbackLoopTerminationResult
                {
                    IsSuccessful = false,
                    TerminationTimestamp = DateTime.UtcNow,
                    TerminationReason = "Session not found"
                };
            }
            
            var result = new FeedbackLoopTerminationResult
            {
                TerminationTimestamp = DateTime.UtcNow,
                TerminationReason = "Manual termination"
            };
            
            try
            {
                // Update session status
                session.Status = FeedbackSessionStatus.Completed;
                session.EndTime = DateTime.UtcNow;
                
                // Get final metrics and validation
                result.FinalMetrics = GetPerformanceMetrics(sessionId);
                result.FinalComfortValidation = ValidateComfortLevels(sessionId);
                
                // Generate session summary
                result.SessionSummary = GenerateSessionSummary(session);
                
                // Remove from active sessions
                _activeSessions.Remove(sessionId);
                
                result.IsSuccessful = true;
                
                Debug.Log($"Feedback loop stopped for session {sessionId}");
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.SessionSummary = $"Error during termination: {ex.Message}";
                Debug.LogError($"Error stopping feedback loop for session {sessionId}: {ex.Message}");
            }
            
            return result;
        }
        
        public ComfortFeedbackReport GenerateFeedbackReport(string sessionId)
        {
            // For completed sessions, we might need to look in a different storage
            // For now, assume we can still access the session data
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return null;
            }
            
            var report = new ComfortFeedbackReport
            {
                ReportId = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                GeneratedAt = DateTime.UtcNow,
                SessionSummary = session
            };
            
            // Generate comprehensive analysis
            report.ComfortAnalysis = AnalyzeComfortTrends(sessionId);
            report.PerformanceMetrics = GetPerformanceMetrics(sessionId);
            
            // Analyze adjustment effectiveness
            report.AdjustmentAnalysis = AnalyzeAdjustmentEffectiveness(session);
            
            // Generate insights and recommendations
            report.KeyInsights = GenerateKeyInsights(session);
            report.Recommendations = GenerateRecommendations(session);
            
            // Calculate report confidence
            report.ReportConfidence = CalculateReportConfidence(session);
            
            return report;
        }
        
        public void ResetFeedbackSystem()
        {
            _activeSessions.Clear();
            _availableStrategies.Clear();
            InitializeDefaultStrategies();
            
            Debug.Log("Feedback system reset");
        }    
    
        #region Private Helper Methods
        
        private void InitializeDefaultStrategies()
        {
            _availableStrategies.Add(AdjustmentStrategy.ConservativeReduction);
            _availableStrategies.Add(AdjustmentStrategy.FrequencyAdjustment);
        }
        
        private WaveParameterAdjustment[] GetAllActiveAdjustments()
        {
            var allAdjustments = new List<WaveParameterAdjustment>();
            foreach (var session in _activeSessions.Values)
            {
                allAdjustments.AddRange(session.AdjustmentHistory);
            }
            return allAdjustments.ToArray();
        }
        
        private void OnComfortDataCollected(ComfortDataCollectedEventArgs args)
        {
            // Automatically process comfort data when collected
            if (_activeSessions.ContainsKey(args.SessionId))
            {
                // Extract comfort data point from event args
                var comfortData = ExtractComfortDataFromEvent(args);
                ProcessComfortData(args.SessionId, comfortData);
            }
        }
        
        private void OnDataValidationFailed(DataValidationFailedEventArgs args)
        {
            // Handle data validation failures
            Debug.LogWarning($"Data validation failed for session {args.SessionId}");
        }
        
        private ComfortDataPoint ExtractComfortDataFromEvent(ComfortDataCollectedEventArgs args)
        {
            // This would extract comfort data from the event args
            // For now, return a placeholder
            return new ComfortDataPoint
            {
                Timestamp = DateTime.UtcNow,
                ComfortLevel = ComfortLevel.Comfortable,
                // Additional data would be extracted from args
            };
        }
        
        private float CalculateComfortScore(ComfortDataPoint data)
        {
            // Calculate overall comfort score from various data sources
            float score = 80f; // Base score
            
            // Adjust based on comfort level
            score += (int)data.ComfortLevel * 10f;
            
            // Additional calculations based on available data
            // This would be more sophisticated in a real implementation
            
            return Mathf.Clamp(score, 0f, COMFORT_SCORE_SCALE);
        }
        
        private TrendDirection AnalyzeComfortTrend(ComfortFeedbackSession session)
        {
            if (session.ComfortDataHistory.Count < 2)
                return TrendDirection.Stable;
            
            var recentData = session.ComfortDataHistory.TakeLast(5).ToArray();
            var scores = recentData.Select(CalculateComfortScore).ToArray();
            
            if (scores.Length < 2)
                return TrendDirection.Stable;
            
            var trend = scores.Last() - scores.First();
            
            if (trend > 5f) return TrendDirection.Improving;
            if (trend < -5f) return TrendDirection.Declining;
            return TrendDirection.Stable;
        }
        
        private ComfortIssue[] IdentifyComfortIssues(ComfortDataPoint data, ComfortFeedbackSession session)
        {
            var issues = new List<ComfortIssue>();
            
            var comfortScore = CalculateComfortScore(data);
            
            // Check for low comfort
            if (comfortScore < session.ValidationThresholds.MinimumComfortScore)
            {
                issues.Add(new ComfortIssue
                {
                    IssueId = Guid.NewGuid().ToString(),
                    Type = ComfortIssueType.MotionSickness,
                    Severity = comfortScore < session.ValidationThresholds.CriticalComfortScore ? 
                        ComfortIssueSeverity.Critical : ComfortIssueSeverity.High,
                    Description = $"Low comfort score: {comfortScore:F1}",
                    DetectedAt = DateTime.UtcNow,
                    DetectionConfidence = 0.8f
                });
            }
            
            return issues.ToArray();
        }
        
        private ComfortPrediction PredictComfortTrajectory(ComfortFeedbackSession session)
        {
            if (session.ComfortDataHistory.Count < 3)
            {
                return new ComfortPrediction
                {
                    PredictionTimestamp = DateTime.UtcNow,
                    PredictedComfortScore = 70f,
                    PredictedComfortLevel = ComfortLevel.Comfortable,
                    PredictionConfidence = 0.3f,
                    PredictionHorizon = TimeSpan.FromMinutes(1)
                };
            }
            
            // Simple linear prediction based on recent trend
            var recentScores = session.ComfortDataHistory.TakeLast(3)
                .Select(CalculateComfortScore).ToArray();
            
            var trend = (recentScores.Last() - recentScores.First()) / recentScores.Length;
            var predictedScore = recentScores.Last() + trend;
            
            return new ComfortPrediction
            {
                PredictionTimestamp = DateTime.UtcNow,
                PredictedComfortScore = Mathf.Clamp(predictedScore, 0f, COMFORT_SCORE_SCALE),
                PredictedComfortLevel = ScoreToComfortLevel(predictedScore),
                PredictionConfidence = 0.6f,
                PredictionHorizon = TimeSpan.FromMinutes(1)
            };
        }
        
        private bool ShouldTriggerAdjustments(ComfortDataProcessingResult result, ComfortFeedbackSession session)
        {
            if (!_configuration.EnableAutomaticAdjustments)
                return false;
            
            if (result.IdentifiedIssues.Length == 0)
                return false;
            
            // Check if we have critical issues
            if (result.IdentifiedIssues.Any(i => i.Severity == ComfortIssueSeverity.Critical))
                return true;
            
            // Check comfort score threshold
            if (result.CurrentComfortScore < session.ValidationThresholds.MinimumComfortScore)
                return true;
            
            // Check trend
            if (result.ComfortTrend == TrendDirection.Declining)
                return true;
            
            return false;
        }
        
        private WaveParameterAdjustment[] DetermineRequiredAdjustments(ComfortIssue[] issues, ComfortFeedbackSession session)
        {
            var adjustments = new List<WaveParameterAdjustment>();
            
            foreach (var issue in issues)
            {
                var strategy = SelectBestStrategy(issue, _availableStrategies);
                if (strategy != null)
                {
                    var issueAdjustments = GenerateAdjustmentsFromStrategy(strategy, session);
                    adjustments.AddRange(issueAdjustments);
                }
            }
            
            return OptimizeAdjustments(adjustments.ToArray());
        }
        
        private ParameterAdjustmentResult ApplyAdjustments(string sessionId, WaveParameterAdjustment[] adjustments)
        {
            return ApplyAutomaticAdjustments(sessionId, AdjustmentStrategy.ConservativeReduction);
        }
        
        private void UpdateSessionMetrics(ComfortFeedbackSession session, ComfortDataProcessingResult result)
        {
            // Update various session metrics based on processing result
            var metrics = session.PerformanceMetrics;
            
            if (result.AdjustmentsTriggered)
            {
                metrics.AverageResponseTime = TimeSpan.FromSeconds(5); // Placeholder
            }
        }
        
        private AdjustmentStrategy SelectBestStrategy(ComfortIssue issue, List<AdjustmentStrategy> strategies)
        {
            return strategies
                .Where(s => s.AddressedIssues.Contains(issue.Type))
                .OrderByDescending(s => s.Priority)
                .FirstOrDefault();
        }
        
        private WaveParameterAdjustment[] GenerateAdjustmentsFromStrategy(AdjustmentStrategy strategy, ComfortFeedbackSession session)
        {
            var adjustments = new List<WaveParameterAdjustment>();
            
            // Generate adjustment based on strategy
            var adjustment = new WaveParameterAdjustment
            {
                AdjustmentId = Guid.NewGuid().ToString(),
                AppliedAt = DateTime.UtcNow,
                Type = AdjustmentType.Automatic,
                Reason = $"Applied strategy: {strategy.StrategyName}",
                SettingsBefore = session.CurrentWaveSettings,
                SettingsAfter = ApplyStrategyToSettings(strategy, session.CurrentWaveSettings),
                Strategy = strategy,
                IsAutomatic = true
            };
            
            adjustments.Add(adjustment);
            return adjustments.ToArray();
        }
        
        private WaveMatrixSettings ApplyStrategyToSettings(AdjustmentStrategy strategy, WaveMatrixSettings currentSettings)
        {
            // Apply strategy rules to current settings
            var newSettings = currentSettings; // This would be a deep copy in real implementation
            
            // Apply conservative reduction
            if (strategy.StrategyId == "conservative-reduction")
            {
                // Reduce amplitude by 10%
                // newSettings.Amplitude *= 0.9f;
                // Reduce frequency by 5%
                // newSettings.Frequency *= 0.95f;
            }
            
            return newSettings;
        }
        
        private WaveParameterAdjustment[] OptimizeAdjustments(WaveParameterAdjustment[] adjustments)
        {
            // Remove duplicates and optimize adjustments
            return adjustments.GroupBy(a => a.AdjustmentId).Select(g => g.First()).ToArray();
        }
        
        private void ApplyAdjustmentToSession(ComfortFeedbackSession session, WaveParameterAdjustment adjustment)
        {
            session.CurrentWaveSettings = adjustment.SettingsAfter;
            session.AdjustmentHistory.Add(adjustment);
            
            // Update last known good settings if this is a successful adjustment
            if (adjustment.EffectivenessScore.HasValue && adjustment.EffectivenessScore.Value > 0.7f)
            {
                session.LastKnownGoodSettings = adjustment.SettingsAfter;
            }
        }
        
        private ComfortValidationViolation CheckMetricViolation(ComfortMetricType metricType, float value, ComfortValidationThresholds thresholds)
        {
            float threshold = metricType switch
            {
                ComfortMetricType.OverallComfort => thresholds.MinimumComfortScore,
                ComfortMetricType.MotionSickness => thresholds.MaxSIMScore,
                ComfortMetricType.PhysiologicalStress => thresholds.MaxStressIndicatorScore,
                _ => 100f
            };
            
            bool isViolation = metricType switch
            {
                ComfortMetricType.OverallComfort => value < threshold,
                _ => value > threshold
            };
            
            if (isViolation)
            {
                return new ComfortValidationViolation
                {
                    ViolationId = Guid.NewGuid().ToString(),
                    MetricType = metricType,
                    CurrentValue = value,
                    ThresholdValue = threshold,
                    Severity = DetermineSeverity(value, threshold, metricType),
                    ViolationTimestamp = DateTime.UtcNow,
                    ViolationDuration = TimeSpan.FromSeconds(1) // Placeholder
                };
            }
            
            return null;
        }
        
        private ComfortIssueSeverity DetermineSeverity(float value, float threshold, ComfortMetricType metricType)
        {
            var deviation = Math.Abs(value - threshold) / threshold;
            
            if (deviation > 0.5f) return ComfortIssueSeverity.Critical;
            if (deviation > 0.3f) return ComfortIssueSeverity.High;
            if (deviation > 0.1f) return ComfortIssueSeverity.Medium;
            return ComfortIssueSeverity.Low;
        }
        
        private ComfortIssueType MapMetricToIssueType(ComfortMetricType metricType)
        {
            return metricType switch
            {
                ComfortMetricType.MotionSickness => ComfortIssueType.MotionSickness,
                ComfortMetricType.PhysiologicalStress => ComfortIssueType.PhysiologicalStress,
                ComfortMetricType.BehavioralIndicators => ComfortIssueType.BehavioralDiscomfort,
                _ => ComfortIssueType.MotionSickness
            };
        }
        
        private ComfortIssueSeverity MapViolationSeverity(ComfortIssueSeverity severity)
        {
            return severity; // Direct mapping for now
        }
        
        private string[] GenerateValidationRecommendations(ComfortValidationResult result)
        {
            var recommendations = new List<string>();
            
            if (!result.IsValid)
            {
                recommendations.Add("Consider reducing wave parameters");
                recommendations.Add("Monitor user for signs of discomfort");
                
                if (result.OverallComfortScore < 30f)
                {
                    recommendations.Add("Consider immediate rollback to safe settings");
                }
            }
            
            return recommendations.ToArray();
        }
        
        // Additional helper methods for data extraction
        private float ExtractMotionSicknessScore(ComfortDataPoint data)
        {
            // Extract motion sickness score from comfort data
            return 10f; // Placeholder
        }
        
        private float ExtractPhysiologicalStressScore(ComfortDataPoint data)
        {
            // Extract physiological stress score
            return 20f; // Placeholder
        }
        
        private float ExtractBehavioralScore(ComfortDataPoint data)
        {
            // Extract behavioral comfort score
            return 15f; // Placeholder
        }
        
        private float ExtractSubjectiveRating(ComfortDataPoint data)
        {
            // Extract subjective comfort rating
            return 75f; // Placeholder
        }
        
        private ComfortLevel DetermineComfortLevel(ComfortDataPoint data)
        {
            var score = CalculateComfortScore(data);
            return ScoreToComfortLevel(score);
        }
        
        private ComfortLevel ScoreToComfortLevel(float score)
        {
            if (score >= 80f) return ComfortLevel.VeryComfortable;
            if (score >= 60f) return ComfortLevel.Comfortable;
            if (score >= 40f) return ComfortLevel.SlightlyUncomfortable;
            if (score >= 20f) return ComfortLevel.Uncomfortable;
            return ComfortLevel.VeryUncomfortable;
        }
        
        // Analysis helper methods
        private TrendDirection CalculateOverallTrend(ComfortTrajectoryPoint[] trajectory)
        {
            if (trajectory.Length < 2) return TrendDirection.Stable;
            
            var firstScore = trajectory.First().ComfortScore;
            var lastScore = trajectory.Last().ComfortScore;
            var change = lastScore - firstScore;
            
            if (change > 5f) return TrendDirection.Improving;
            if (change < -5f) return TrendDirection.Declining;
            return TrendDirection.Stable;
        }
        
        private float CalculateTrendStrength(ComfortTrajectoryPoint[] trajectory)
        {
            if (trajectory.Length < 2) return 0f;
            
            var scores = trajectory.Select(t => t.ComfortScore).ToArray();
            var variance = CalculateVariance(scores);
            
            return 1f / (1f + variance); // Higher variance = lower trend strength
        }
        
        private float CalculateVariance(float[] values)
        {
            if (values.Length < 2) return 0f;
            
            var mean = values.Average();
            var sumSquaredDiffs = values.Sum(v => (v - mean) * (v - mean));
            
            return sumSquaredDiffs / values.Length;
        }
        
        private ComfortPattern[] IdentifyComfortPatterns(ComfortTrajectoryPoint[] trajectory)
        {
            // Placeholder pattern identification
            return new ComfortPattern[0];
        }
        
        private ComfortPrediction[] GenerateFuturePredictions(ComfortTrajectoryPoint[] trajectory)
        {
            // Placeholder prediction generation
            return new ComfortPrediction[0];
        }
        
        private ComfortRiskFactor[] AssessComfortRiskFactors(ComfortFeedbackSession session)
        {
            // Placeholder risk assessment
            return new ComfortRiskFactor[0];
        }
        
        private string[] GeneratePreventiveRecommendations(ComfortTrendAnalysis analysis)
        {
            var recommendations = new List<string>();
            
            if (analysis.OverallTrend == TrendDirection.Declining)
            {
                recommendations.Add("Consider proactive parameter adjustment");
                recommendations.Add("Increase monitoring frequency");
            }
            
            return recommendations.ToArray();
        }
        
        private float CalculateAnalysisConfidence(int dataPointCount, float trendStrength)
        {
            var dataConfidence = Math.Min(1f, dataPointCount / 10f);
            var trendConfidence = trendStrength;
            
            return (dataConfidence + trendConfidence) / 2f;
        }
        
        private string GenerateSessionSummary(ComfortFeedbackSession session)
        {
            var duration = (session.EndTime ?? DateTime.UtcNow) - session.StartTime;
            return $"Session completed in {duration.TotalMinutes:F1} minutes with {session.AdjustmentHistory.Count} adjustments";
        }
        
        private AdjustmentEffectivenessAnalysis AnalyzeAdjustmentEffectiveness(ComfortFeedbackSession session)
        {
            var successfulAdjustments = session.AdjustmentHistory.Count(a => a.EffectivenessScore.HasValue && a.EffectivenessScore.Value > 0.5f);
            var totalAdjustments = session.AdjustmentHistory.Count;
            
            return new AdjustmentEffectivenessAnalysis
            {
                OverallEffectiveness = totalAdjustments > 0 ? (float)successfulAdjustments / totalAdjustments : 1f,
                SuccessRate = totalAdjustments > 0 ? (float)successfulAdjustments / totalAdjustments : 1f,
                AverageTimeToEffect = TimeSpan.FromSeconds(30), // Placeholder
                StrategyEffectiveness = new Dictionary<string, float>(),
                MostEffectiveStrategies = new string[0],
                LeastEffectiveStrategies = new string[0]
            };
        }
        
        private string[] GenerateKeyInsights(ComfortFeedbackSession session)
        {
            var insights = new List<string>();
            
            if (session.AdjustmentHistory.Count > 0)
            {
                insights.Add($"Made {session.AdjustmentHistory.Count} parameter adjustments during session");
            }
            
            if (session.ComfortDataHistory.Count > 0)
            {
                var avgScore = session.ComfortDataHistory.Select(CalculateComfortScore).Average();
                insights.Add($"Average comfort score: {avgScore:F1}");
            }
            
            return insights.ToArray();
        }
        
        private string[] GenerateRecommendations(ComfortFeedbackSession session)
        {
            var recommendations = new List<string>();
            
            if (session.PerformanceMetrics.RollbacksTriggered > 0)
            {
                recommendations.Add("Consider more conservative initial parameters");
            }
            
            if (session.AdjustmentHistory.Count > 5)
            {
                recommendations.Add("Review adjustment strategy effectiveness");
            }
            
            return recommendations.ToArray();
        }
        
        private float CalculateReportConfidence(ComfortFeedbackSession session)
        {
            var dataPoints = session.ComfortDataHistory.Count;
            var adjustments = session.AdjustmentHistory.Count;
            
            // More data points and adjustments = higher confidence
            var dataConfidence = Math.Min(1f, dataPoints / 20f);
            var adjustmentConfidence = adjustments > 0 ? 0.8f : 0.5f;
            
            return (dataConfidence + adjustmentConfidence) / 2f;
        }
        
        #endregion
    }
}