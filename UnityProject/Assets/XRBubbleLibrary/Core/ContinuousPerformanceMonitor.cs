using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Implementation of continuous performance monitoring system.
    /// Provides real-time performance tracking with threshold violation detection.
    /// Implements Requirement 9.1: Continuous performance monitoring infrastructure.
    /// Implements Requirement 9.3: Threshold violation detection.
    /// </summary>
    public class ContinuousPerformanceMonitor : IContinuousPerformanceMonitor
    {
        private bool _isInitialized;
        private bool _isMonitoring;
        private PerformanceMonitorConfiguration _configuration;
        private IPerformanceThresholdManager _thresholdManager;
        
        private readonly Dictionary<string, PerformanceMonitoringSession> _activeSessions;
        private readonly List<IPerformanceMetricCollector> _customCollectors;
        private readonly Dictionary<string, PerformanceViolation> _activeViolations;
        
        private Timer _dataCollectionTimer;
        private Timer _statisticsTimer;
        private Timer _cleanupTimer;
        
        private PerformanceMetrics _currentMetrics;
        private readonly object _metricsLock = new object();
        
        private const int MAX_ACTIVE_SESSIONS = 20;
        private const int MAX_ALERTS_PER_MINUTE = 10;
        private int _alertsThisMinute = 0;
        private DateTime _lastAlertReset = DateTime.UtcNow;
        
        public bool IsMonitoring => _isMonitoring && _activeSessions.Count > 0;
        public PerformanceMonitorConfiguration Configuration => _configuration;
        public PerformanceMetrics CurrentMetrics
        {
            get
            {
                lock (_metricsLock)
                {
                    return _currentMetrics;
                }
            }
        }
        public PerformanceMonitoringSession[] ActiveSessions => _activeSessions.Values.ToArray();
        
        public event Action<PerformanceViolationEventArgs> PerformanceViolationDetected;
        public event Action<PerformanceRecoveryEventArgs> PerformanceRecovered;
        public event Action<PerformanceDataCollectedEventArgs> PerformanceDataCollected;
        public event Action<MonitoringSessionStartedEventArgs> MonitoringSessionStarted;
        public event Action<MonitoringSessionEndedEventArgs> MonitoringSessionEnded;
        
        public ContinuousPerformanceMonitor()
        {
            _activeSessions = new Dictionary<string, PerformanceMonitoringSession>();
            _customCollectors = new List<IPerformanceMetricCollector>();
            _activeViolations = new Dictionary<string, PerformanceViolation>();
            _currentMetrics = new PerformanceMetrics();
        }
        
        public bool Initialize(PerformanceMonitorConfiguration config, IPerformanceThresholdManager thresholdManager)
        {
            if (config == null)
            {
                Debug.LogError("Cannot initialize with null configuration");
                return false;
            }
            
            if (thresholdManager == null)
            {
                Debug.LogError("Cannot initialize with null threshold manager");
                return false;
            }
            
            _configuration = config;
            _thresholdManager = thresholdManager;
            
            // Subscribe to threshold violation events
            _thresholdManager.RegisterViolationCallback(OnThresholdViolation);
            
            // Initialize timers
            InitializeTimers();
            
            _isInitialized = true;
            
            Debug.Log("Continuous performance monitor initialized successfully");
            return true;
        }
        
        public PerformanceMonitoringSession StartMonitoring(string sessionId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("Performance monitor not initialized");
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
            
            var session = new PerformanceMonitoringSession
            {
                SessionId = sessionId,
                StartTime = DateTime.UtcNow,
                Status = MonitoringSessionStatus.Active,
                Configuration = _configuration,
                Statistics = new PerformanceStatistics(),
                LastDataCollection = DateTime.UtcNow,
                SessionQualityScore = 1.0f
            };
            
            _activeSessions[sessionId] = session;
            
            // Start monitoring if this is the first session
            if (!_isMonitoring)
            {
                StartMonitoringTimers();
                _isMonitoring = true;
            }
            
            // Fire event
            MonitoringSessionStarted?.Invoke(new MonitoringSessionStartedEventArgs
            {
                SessionId = sessionId,
                Session = session,
                StartTimestamp = DateTime.UtcNow
            });
            
            Debug.Log($"Started performance monitoring for session {sessionId}");
            return session;
        }
        
        public MonitoringTerminationResult StopMonitoring(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new MonitoringTerminationResult
                {
                    IsSuccessful = false,
                    TerminationTimestamp = DateTime.UtcNow,
                    TerminationReason = "Session not found",
                    TerminationMessages = new[] { "Session not found" }
                };
            }
            
            var result = new MonitoringTerminationResult
            {
                TerminationTimestamp = DateTime.UtcNow,
                TerminationReason = "Manual termination"
            };
            
            try
            {
                // Update session status
                session.Status = MonitoringSessionStatus.Completed;
                session.EndTime = DateTime.UtcNow;
                
                // Calculate final statistics
                result.FinalStatistics = CalculateSessionStatistics(session);
                result.SessionDuration = session.EndTime.Value - session.StartTime;
                result.TotalDataPointsCollected = session.DataPointsCollected;
                result.SessionSummary = GenerateSessionSummary(session);
                
                // Remove from active sessions
                _activeSessions.Remove(sessionId);
                
                // Stop monitoring if no active sessions
                if (_activeSessions.Count == 0)
                {
                    StopMonitoringTimers();
                    _isMonitoring = false;
                }
                
                result.IsSuccessful = true;
                result.TerminationMessages = new[] { "Session terminated successfully" };
                
                // Fire event
                MonitoringSessionEnded?.Invoke(new MonitoringSessionEndedEventArgs
                {
                    SessionId = sessionId,
                    TerminationResult = result,
                    EndTimestamp = DateTime.UtcNow
                });
                
                Debug.Log($"Stopped performance monitoring for session {sessionId}");
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.TerminationMessages = new[] { $"Error during termination: {ex.Message}" };
                Debug.LogError($"Error stopping monitoring for session {sessionId}: {ex.Message}");
            }
            
            return result;
        }
        
        public PerformanceMetrics CollectCurrentMetrics()
        {
            var metrics = new PerformanceMetrics
            {
                CapturedAt = DateTime.UtcNow,
                BuildVersion = Application.version
            };
            
            try
            {
                // Collect Unity performance metrics
                metrics.AverageFPS = 1f / Time.unscaledDeltaTime;
                metrics.AverageFrameTime = Time.unscaledDeltaTime * 1000f; // Convert to milliseconds
                metrics.MinimumFPS = metrics.AverageFPS; // Simplified for now
                metrics.MaximumFPS = metrics.AverageFPS;
                
                // Collect memory metrics
                metrics.MemoryUsage = GC.GetTotalMemory(false);
                
                // Collect system metrics (simplified)
                metrics.CPUUsage = GetCPUUsage();
                metrics.GPUUsage = GetGPUUsage();
                metrics.ThermalState = GetThermalState();
                
                // Collect custom metrics
                var customMetrics = new Dictionary<string, object>();
                foreach (var collector in _customCollectors.Where(c => c.IsEnabled))
                {
                    try
                    {
                        var collectorMetrics = collector.CollectMetrics();
                        foreach (var metric in collectorMetrics)
                        {
                            customMetrics[$"{collector.CollectorId}_{metric.Key}"] = metric.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Error collecting metrics from {collector.CollectorId}: {ex.Message}");
                    }
                }
                metrics.AdditionalMetrics = customMetrics;
                
                // Update current metrics
                lock (_metricsLock)
                {
                    _currentMetrics = metrics;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error collecting performance metrics: {ex.Message}");
            }
            
            return metrics;
        }
        
        public PerformanceMetrics[] GetPerformanceHistory(string sessionId, TimeSpan period)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new PerformanceMetrics[0];
            }
            
            var cutoffTime = DateTime.UtcNow - period;
            return session.PerformanceHistory
                .Where(m => m.CapturedAt >= cutoffTime)
                .OrderBy(m => m.CapturedAt)
                .ToArray();
        }
        
        public PerformanceStatistics GetPerformanceStatistics(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new PerformanceStatistics();
            }
            
            return CalculateSessionStatistics(session);
        }
        
        public void UpdateConfiguration(PerformanceMonitorConfiguration config)
        {
            if (config == null)
            {
                Debug.LogError("Cannot update with null configuration");
                return;
            }
            
            _configuration = config;
            
            // Update all active sessions
            foreach (var session in _activeSessions.Values)
            {
                session.Configuration = config;
            }
            
            // Restart timers with new intervals
            if (_isMonitoring)
            {
                StopMonitoringTimers();
                InitializeTimers();
                StartMonitoringTimers();
            }
            
            Debug.Log("Performance monitor configuration updated");
        }
        
        public void SetPerformanceThresholds(PerformanceThresholds thresholds)
        {
            _thresholdManager?.UpdateThresholds(thresholds);
            Debug.Log("Performance thresholds updated");
        }
        
        public void RegisterCustomMetricCollector(IPerformanceMetricCollector collector)
        {
            if (collector == null)
            {
                Debug.LogError("Cannot register null collector");
                return;
            }
            
            if (_customCollectors.Any(c => c.CollectorId == collector.CollectorId))
            {
                Debug.LogWarning($"Collector {collector.CollectorId} already registered");
                return;
            }
            
            if (collector.Initialize())
            {
                _customCollectors.Add(collector);
                Debug.Log($"Registered custom metric collector: {collector.CollectorName}");
            }
            else
            {
                Debug.LogError($"Failed to initialize collector: {collector.CollectorName}");
            }
        }
        
        public void UnregisterCustomMetricCollector(string collectorId)
        {
            var collector = _customCollectors.FirstOrDefault(c => c.CollectorId == collectorId);
            if (collector != null)
            {
                collector.Cleanup();
                _customCollectors.Remove(collector);
                Debug.Log($"Unregistered custom metric collector: {collectorId}");
            }
        }
        
        public PerformanceMetrics ForceDataCollection(string sessionId)
        {
            if (!_activeSessions.ContainsKey(sessionId))
            {
                Debug.LogWarning($"Session {sessionId} not found for forced data collection");
                return new PerformanceMetrics();
            }
            
            var metrics = CollectCurrentMetrics();
            ProcessCollectedMetrics(metrics);
            
            return metrics;
        }        

        public PerformanceTrendAnalysis AnalyzePerformanceTrends(string sessionId, TimeSpan analysisWindow)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return null;
            }
            
            var cutoffTime = DateTime.UtcNow - analysisWindow;
            var relevantMetrics = session.PerformanceHistory
                .Where(m => m.CapturedAt >= cutoffTime)
                .OrderBy(m => m.CapturedAt)
                .ToArray();
            
            if (relevantMetrics.Length < 2)
            {
                return new PerformanceTrendAnalysis
                {
                    AnalysisTimestamp = DateTime.UtcNow,
                    AnalysisWindow = analysisWindow,
                    OverallTrend = TrendDirection.Stable,
                    TrendStrength = 0f,
                    AnalysisConfidence = 0.1f,
                    Recommendations = new[] { "Insufficient data for trend analysis" }
                };
            }
            
            var analysis = new PerformanceTrendAnalysis
            {
                AnalysisTimestamp = DateTime.UtcNow,
                AnalysisWindow = analysisWindow,
                MetricTrends = new Dictionary<PerformanceMetricType, MetricTrend>()
            };
            
            // Analyze trends for each metric type
            foreach (var metricType in _configuration.MonitoredMetrics)
            {
                var metricTrend = AnalyzeMetricTrend(metricType, relevantMetrics);
                analysis.MetricTrends[metricType] = metricTrend;
            }
            
            // Calculate overall trend
            analysis.OverallTrend = CalculateOverallTrend(analysis.MetricTrends.Values);
            analysis.TrendStrength = CalculateTrendStrength(analysis.MetricTrends.Values);
            
            // Detect patterns and anomalies
            analysis.IdentifiedPatterns = DetectPerformancePatterns(relevantMetrics);
            analysis.DetectedAnomalies = DetectPerformanceAnomalies(relevantMetrics);
            
            // Generate predictions
            analysis.FuturePredictions = GeneratePerformancePredictions(relevantMetrics);
            
            // Calculate confidence and generate recommendations
            analysis.AnalysisConfidence = CalculateAnalysisConfidence(relevantMetrics.Length, analysis.TrendStrength);
            analysis.Recommendations = GenerateTrendRecommendations(analysis);
            
            return analysis;
        }
        
        public PerformanceMonitoringReport GenerateMonitoringReport(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return null;
            }
            
            var report = new PerformanceMonitoringReport
            {
                ReportId = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                GeneratedAt = DateTime.UtcNow,
                ReportPeriod = DateTime.UtcNow - session.StartTime,
                SessionSummary = session
            };
            
            // Generate statistics
            report.Statistics = CalculateSessionStatistics(session);
            
            // Generate trend analysis
            report.TrendAnalysis = AnalyzePerformanceTrends(sessionId, TimeSpan.FromHours(1));
            
            // Generate violation summary
            report.ViolationSummary = GenerateViolationSummary(session);
            
            // Generate insights and recommendations
            report.KeyInsights = GenerateKeyInsights(session);
            report.Recommendations = GenerateRecommendations(session);
            
            // Calculate report quality
            report.ReportQuality = CalculateReportQuality(session);
            
            return report;
        }
        
        public PerformanceDataExport ExportPerformanceData(string sessionId, DataExportFormat format, TimeSpan timeRange)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new PerformanceDataExport
                {
                    ExportId = Guid.NewGuid().ToString(),
                    ExportedAt = DateTime.UtcNow,
                    Format = format,
                    TimeRange = timeRange,
                    DataPointsExported = 0,
                    ExportData = "Session not found",
                    Quality = new ExportQuality { DataCompleteness = 0f }
                };
            }
            
            var cutoffTime = DateTime.UtcNow - timeRange;
            var dataToExport = session.PerformanceHistory
                .Where(m => m.CapturedAt >= cutoffTime)
                .OrderBy(m => m.CapturedAt)
                .ToArray();
            
            var export = new PerformanceDataExport
            {
                ExportId = Guid.NewGuid().ToString(),
                ExportedAt = DateTime.UtcNow,
                Format = format,
                TimeRange = timeRange,
                DataPointsExported = dataToExport.Length
            };
            
            // Generate export data based on format
            switch (format)
            {
                case DataExportFormat.JSON:
                    export.ExportData = JsonUtility.ToJson(dataToExport, true);
                    break;
                case DataExportFormat.CSV:
                    export.ExportData = GenerateCSVExport(dataToExport);
                    break;
                default:
                    export.ExportData = JsonUtility.ToJson(dataToExport, true);
                    break;
            }
            
            export.FileSizeBytes = System.Text.Encoding.UTF8.GetByteCount(export.ExportData);
            export.Quality = CalculateExportQuality(dataToExport, session);
            
            return export;
        }
        
        public bool PauseMonitoring(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return false;
            }
            
            session.Status = MonitoringSessionStatus.Paused;
            Debug.Log($"Paused monitoring for session {sessionId}");
            return true;
        }
        
        public bool ResumeMonitoring(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return false;
            }
            
            session.Status = MonitoringSessionStatus.Active;
            Debug.Log($"Resumed monitoring for session {sessionId}");
            return true;
        }
        
        public PerformanceDashboardData GetDashboardData(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return new PerformanceDashboardData
                {
                    Timestamp = DateTime.UtcNow,
                    CurrentMetrics = new PerformanceMetrics(),
                    RecentHistory = new PerformanceMetrics[0],
                    ActiveViolations = new PerformanceViolation[0],
                    ActiveAlerts = new PerformanceAlert[0]
                };
            }
            
            var dashboard = new PerformanceDashboardData
            {
                Timestamp = DateTime.UtcNow,
                CurrentMetrics = CurrentMetrics
            };
            
            // Get recent history (last 5 minutes)
            var recentCutoff = DateTime.UtcNow - TimeSpan.FromMinutes(5);
            dashboard.RecentHistory = session.PerformanceHistory
                .Where(m => m.CapturedAt >= recentCutoff)
                .OrderBy(m => m.CapturedAt)
                .ToArray();
            
            // Get active violations
            dashboard.ActiveViolations = _activeViolations.Values.ToArray();
            
            // Generate health indicators
            dashboard.HealthIndicators = GenerateHealthIndicators(session);
            
            // Generate active alerts
            dashboard.ActiveAlerts = GenerateActiveAlerts(session);
            
            // Calculate short-term trends
            dashboard.ShortTermTrends = CalculateShortTermTrends(dashboard.RecentHistory);
            
            // Generate system status
            dashboard.SystemStatus = GenerateSystemStatus();
            
            return dashboard;
        }
        
        public void ResetMonitoringSystem()
        {
            // Stop all monitoring
            StopMonitoringTimers();
            _isMonitoring = false;
            
            // Clear all sessions
            foreach (var session in _activeSessions.Values)
            {
                session.Status = MonitoringSessionStatus.Terminated;
            }
            _activeSessions.Clear();
            
            // Clear violations
            _activeViolations.Clear();
            
            // Cleanup custom collectors
            foreach (var collector in _customCollectors)
            {
                collector.Cleanup();
            }
            _customCollectors.Clear();
            
            Debug.Log("Performance monitoring system reset");
        }
        
        #region Private Helper Methods
        
        private void InitializeTimers()
        {
            var dataCollectionInterval = TimeSpan.FromSeconds(_configuration.DataCollectionIntervalSeconds);
            var statisticsInterval = TimeSpan.FromSeconds(_configuration.StatisticsIntervalSeconds);
            var cleanupInterval = TimeSpan.FromHours(1); // Cleanup every hour
            
            _dataCollectionTimer = new Timer(OnDataCollectionTimer, null, Timeout.Infinite, Timeout.Infinite);
            _statisticsTimer = new Timer(OnStatisticsTimer, null, Timeout.Infinite, Timeout.Infinite);
            _cleanupTimer = new Timer(OnCleanupTimer, null, Timeout.Infinite, Timeout.Infinite);
        }
        
        private void StartMonitoringTimers()
        {
            var dataCollectionInterval = (int)(_configuration.DataCollectionIntervalSeconds * 1000);
            var statisticsInterval = (int)(_configuration.StatisticsIntervalSeconds * 1000);
            var cleanupInterval = 3600000; // 1 hour in milliseconds
            
            _dataCollectionTimer?.Change(0, dataCollectionInterval);
            _statisticsTimer?.Change(statisticsInterval, statisticsInterval);
            _cleanupTimer?.Change(cleanupInterval, cleanupInterval);
        }
        
        private void StopMonitoringTimers()
        {
            _dataCollectionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _statisticsTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _cleanupTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
        
        private void OnDataCollectionTimer(object state)
        {
            if (!_isMonitoring || _activeSessions.Count == 0)
                return;
            
            try
            {
                var metrics = CollectCurrentMetrics();
                ProcessCollectedMetrics(metrics);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in data collection timer: {ex.Message}");
            }
        }
        
        private void OnStatisticsTimer(object state)
        {
            if (!_isMonitoring || _activeSessions.Count == 0)
                return;
            
            try
            {
                foreach (var session in _activeSessions.Values)
                {
                    if (session.Status == MonitoringSessionStatus.Active)
                    {
                        session.Statistics = CalculateSessionStatistics(session);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in statistics timer: {ex.Message}");
            }
        }
        
        private void OnCleanupTimer(object state)
        {
            if (!_configuration.EnableAutomaticDataCleanup)
                return;
            
            try
            {
                var cutoffTime = DateTime.UtcNow - TimeSpan.FromHours(_configuration.DataRetentionHours);
                
                foreach (var session in _activeSessions.Values)
                {
                    // Remove old performance data
                    session.PerformanceHistory.RemoveAll(m => m.CapturedAt < cutoffTime);
                    
                    // Remove old violations
                    session.ViolationHistory.RemoveAll(v => v.OccurredAt < cutoffTime);
                    
                    // Limit data points per session
                    if (session.PerformanceHistory.Count > _configuration.MaxDataPointsPerSession)
                    {
                        var excess = session.PerformanceHistory.Count - _configuration.MaxDataPointsPerSession;
                        session.PerformanceHistory.RemoveRange(0, excess);
                    }
                }
                
                Debug.Log("Performed automatic data cleanup");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in cleanup timer: {ex.Message}");
            }
        }
        
        private void ProcessCollectedMetrics(PerformanceMetrics metrics)
        {
            foreach (var session in _activeSessions.Values)
            {
                if (session.Status != MonitoringSessionStatus.Active)
                    continue;
                
                // Add to session history
                session.PerformanceHistory.Add(metrics);
                session.DataPointsCollected++;
                session.LastDataCollection = DateTime.UtcNow;
                
                // Check for threshold violations
                if (_configuration.EnableThresholdViolationDetection)
                {
                    CheckForThresholdViolations(session.SessionId, metrics);
                }
                
                // Fire data collected event
                PerformanceDataCollected?.Invoke(new PerformanceDataCollectedEventArgs
                {
                    SessionId = session.SessionId,
                    CollectedMetrics = metrics,
                    CollectionTimestamp = DateTime.UtcNow,
                    CollectionDuration = 0f // Would be measured in real implementation
                });
            }
        }
        
        private void CheckForThresholdViolations(string sessionId, PerformanceMetrics metrics)
        {
            var validationResult = _thresholdManager.ValidatePerformance(metrics);
            
            foreach (var violation in validationResult.Violations)
            {
                var performanceViolation = new PerformanceViolation
                {
                    ViolationId = Guid.NewGuid().ToString(),
                    OccurredAt = DateTime.UtcNow,
                    MetricType = violation.MetricType,
                    CurrentValue = violation.CurrentValue,
                    ThresholdValue = violation.ThresholdValue,
                    Severity = MapToViolationSeverity(violation.Severity),
                    IsResolved = false
                };
                
                // Add to active violations
                var violationKey = $"{sessionId}_{violation.MetricType}";
                _activeViolations[violationKey] = performanceViolation;
                
                // Add to session history
                if (_activeSessions.TryGetValue(sessionId, out var session))
                {
                    session.ViolationHistory.Add(performanceViolation);
                }
                
                // Fire violation event
                if (ShouldFireAlert())
                {
                    PerformanceViolationDetected?.Invoke(new PerformanceViolationEventArgs
                    {
                        SessionId = sessionId,
                        Violation = performanceViolation,
                        CurrentMetrics = metrics,
                        DetectionTimestamp = DateTime.UtcNow
                    });
                }
            }
        }
        
        private void OnThresholdViolation(ThresholdViolation violation)
        {
            // This is called by the threshold manager when violations are detected
            // Additional processing can be done here if needed
        }
        
        private bool ShouldFireAlert()
        {
            // Reset alert counter every minute
            if (DateTime.UtcNow - _lastAlertReset > TimeSpan.FromMinutes(1))
            {
                _alertsThisMinute = 0;
                _lastAlertReset = DateTime.UtcNow;
            }
            
            if (_alertsThisMinute >= _configuration.MaxAlertsPerMinute)
            {
                return false;
            }
            
            _alertsThisMinute++;
            return true;
        }
        
        private ViolationSeverity MapToViolationSeverity(Core.ViolationSeverity severity)
        {
            return severity switch
            {
                Core.ViolationSeverity.Info => ViolationSeverity.Info,
                Core.ViolationSeverity.Warning => ViolationSeverity.Warning,
                Core.ViolationSeverity.Critical => ViolationSeverity.Critical,
                _ => ViolationSeverity.Warning
            };
        }
        
        // Simplified metric collection methods (would be more sophisticated in real implementation)
        private float GetCPUUsage()
        {
            // Placeholder - would use platform-specific APIs
            return UnityEngine.Random.Range(20f, 80f);
        }
        
        private float GetGPUUsage()
        {
            // Placeholder - would use platform-specific APIs
            return UnityEngine.Random.Range(30f, 90f);
        }
        
        private float GetThermalState()
        {
            // Placeholder - would use platform-specific APIs
            return UnityEngine.Random.Range(0.2f, 0.8f);
        }
        
        private PerformanceStatistics CalculateSessionStatistics(PerformanceMonitoringSession session)
        {
            if (session.PerformanceHistory.Count == 0)
            {
                return new PerformanceStatistics
                {
                    CalculatedAt = DateTime.UtcNow,
                    TimePeriod = DateTime.UtcNow - session.StartTime,
                    DataPointsAnalyzed = 0,
                    StabilityScore = 1.0f,
                    OverallHealthScore = 1.0f,
                    PerformanceTrend = TrendDirection.Stable,
                    DataCollectionEfficiency = 1.0f
                };
            }
            
            var metrics = session.PerformanceHistory;
            var statistics = new PerformanceStatistics
            {
                CalculatedAt = DateTime.UtcNow,
                TimePeriod = DateTime.UtcNow - session.StartTime,
                DataPointsAnalyzed = metrics.Count
            };
            
            // Calculate average metrics
            statistics.AverageMetrics = new PerformanceMetrics
            {
                AverageFPS = metrics.Average(m => m.AverageFPS),
                AverageFrameTime = metrics.Average(m => m.AverageFrameTime),
                MemoryUsage = (long)metrics.Average(m => m.MemoryUsage),
                CPUUsage = metrics.Average(m => m.CPUUsage),
                GPUUsage = metrics.Average(m => m.GPUUsage),
                ThermalState = metrics.Average(m => m.ThermalState)
            };
            
            // Calculate min/max metrics
            statistics.MinimumMetrics = new PerformanceMetrics
            {
                AverageFPS = metrics.Min(m => m.AverageFPS),
                AverageFrameTime = metrics.Min(m => m.AverageFrameTime),
                MemoryUsage = metrics.Min(m => m.MemoryUsage),
                CPUUsage = metrics.Min(m => m.CPUUsage),
                GPUUsage = metrics.Min(m => m.GPUUsage),
                ThermalState = metrics.Min(m => m.ThermalState)
            };
            
            statistics.MaximumMetrics = new PerformanceMetrics
            {
                AverageFPS = metrics.Max(m => m.AverageFPS),
                AverageFrameTime = metrics.Max(m => m.AverageFrameTime),
                MemoryUsage = metrics.Max(m => m.MemoryUsage),
                CPUUsage = metrics.Max(m => m.CPUUsage),
                GPUUsage = metrics.Max(m => m.GPUUsage),
                ThermalState = metrics.Max(m => m.ThermalState)
            };
            
            // Calculate percentiles
            statistics.Percentiles = CalculatePercentiles(metrics);
            
            // Calculate violation statistics
            statistics.TotalViolations = session.ViolationHistory.Count;
            statistics.ViolationsBySeverity = session.ViolationHistory
                .GroupBy(v => v.Severity)
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Calculate scores
            statistics.StabilityScore = CalculateStabilityScore(metrics);
            statistics.OverallHealthScore = CalculateHealthScore(statistics);
            statistics.PerformanceTrend = CalculatePerformanceTrend(metrics);
            statistics.DataCollectionEfficiency = CalculateDataCollectionEfficiency(session);
            
            return statistics;
        } 
       
        private Dictionary<PerformanceMetricType, PerformancePercentiles> CalculatePercentiles(List<PerformanceMetrics> metrics)
        {
            var percentiles = new Dictionary<PerformanceMetricType, PerformancePercentiles>();
            
            // Calculate FPS percentiles
            var fpsValues = metrics.Select(m => m.AverageFPS).OrderBy(v => v).ToArray();
            percentiles[PerformanceMetricType.FPS] = CalculatePercentileValues(fpsValues);
            
            // Calculate Frame Time percentiles
            var frameTimeValues = metrics.Select(m => m.AverageFrameTime).OrderBy(v => v).ToArray();
            percentiles[PerformanceMetricType.FrameTime] = CalculatePercentileValues(frameTimeValues);
            
            // Calculate Memory percentiles
            var memoryValues = metrics.Select(m => (float)m.MemoryUsage).OrderBy(v => v).ToArray();
            percentiles[PerformanceMetricType.MemoryUsage] = CalculatePercentileValues(memoryValues);
            
            return percentiles;
        }
        
        private PerformancePercentiles CalculatePercentileValues(float[] sortedValues)
        {
            if (sortedValues.Length == 0)
                return new PerformancePercentiles();
            
            return new PerformancePercentiles
            {
                P50 = GetPercentile(sortedValues, 0.5f),
                P90 = GetPercentile(sortedValues, 0.9f),
                P95 = GetPercentile(sortedValues, 0.95f),
                P99 = GetPercentile(sortedValues, 0.99f),
                P999 = GetPercentile(sortedValues, 0.999f)
            };
        }
        
        private float GetPercentile(float[] sortedValues, float percentile)
        {
            if (sortedValues.Length == 0)
                return 0f;
            
            var index = (int)Math.Ceiling(sortedValues.Length * percentile) - 1;
            index = Math.Max(0, Math.Min(index, sortedValues.Length - 1));
            
            return sortedValues[index];
        }
        
        private float CalculateStabilityScore(List<PerformanceMetrics> metrics)
        {
            if (metrics.Count < 2)
                return 1.0f;
            
            // Calculate coefficient of variation for FPS (lower is more stable)
            var fpsValues = metrics.Select(m => m.AverageFPS).ToArray();
            var fpsMean = fpsValues.Average();
            var fpsStdDev = CalculateStandardDeviation(fpsValues, fpsMean);
            var fpsCoV = fpsMean > 0 ? fpsStdDev / fpsMean : 0f;
            
            // Convert to stability score (0-1, higher is more stable)
            return Math.Max(0f, 1f - fpsCoV);
        }
        
        private float CalculateStandardDeviation(float[] values, float mean)
        {
            if (values.Length < 2)
                return 0f;
            
            var sumSquaredDiffs = values.Sum(v => (v - mean) * (v - mean));
            return (float)Math.Sqrt(sumSquaredDiffs / values.Length);
        }
        
        private float CalculateHealthScore(PerformanceStatistics statistics)
        {
            float score = 1.0f;
            
            // Penalize for violations
            if (statistics.TotalViolations > 0)
            {
                var violationPenalty = Math.Min(0.5f, statistics.TotalViolations * 0.05f);
                score -= violationPenalty;
            }
            
            // Factor in stability
            score *= statistics.StabilityScore;
            
            return Math.Max(0f, score);
        }
        
        private TrendDirection CalculatePerformanceTrend(List<PerformanceMetrics> metrics)
        {
            if (metrics.Count < 2)
                return TrendDirection.Stable;
            
            var recentMetrics = metrics.TakeLast(10).ToArray();
            if (recentMetrics.Length < 2)
                return TrendDirection.Stable;
            
            var firstFPS = recentMetrics.First().AverageFPS;
            var lastFPS = recentMetrics.Last().AverageFPS;
            var change = lastFPS - firstFPS;
            
            if (change > 2f) return TrendDirection.Improving;
            if (change < -2f) return TrendDirection.Declining;
            return TrendDirection.Stable;
        }
        
        private float CalculateDataCollectionEfficiency(PerformanceMonitoringSession session)
        {
            var expectedCollections = (DateTime.UtcNow - session.StartTime).TotalSeconds / _configuration.DataCollectionIntervalSeconds;
            var actualCollections = session.DataPointsCollected;
            
            return expectedCollections > 0 ? Math.Min(1f, actualCollections / (float)expectedCollections) : 1f;
        }
        
        private MetricTrend AnalyzeMetricTrend(PerformanceMetricType metricType, PerformanceMetrics[] metrics)
        {
            if (metrics.Length < 2)
            {
                return new MetricTrend
                {
                    MetricType = metricType,
                    Direction = TrendDirection.Stable,
                    Strength = 0f,
                    ChangeRate = 0f,
                    Volatility = 0f,
                    IsStable = true
                };
            }
            
            var values = ExtractMetricValues(metricType, metrics);
            var trend = new MetricTrend
            {
                MetricType = metricType
            };
            
            // Calculate trend direction and strength
            var firstValue = values.First();
            var lastValue = values.Last();
            var change = lastValue - firstValue;
            var timeSpan = (metrics.Last().CapturedAt - metrics.First().CapturedAt).TotalMinutes;
            
            trend.ChangeRate = timeSpan > 0 ? change / (float)timeSpan : 0f;
            
            if (Math.Abs(change) < 0.1f)
            {
                trend.Direction = TrendDirection.Stable;
                trend.Strength = 0f;
            }
            else if (change > 0)
            {
                trend.Direction = metricType == PerformanceMetricType.FPS ? TrendDirection.Improving : TrendDirection.Declining;
                trend.Strength = Math.Min(1f, Math.Abs(change) / firstValue);
            }
            else
            {
                trend.Direction = metricType == PerformanceMetricType.FPS ? TrendDirection.Declining : TrendDirection.Improving;
                trend.Strength = Math.Min(1f, Math.Abs(change) / firstValue);
            }
            
            // Calculate volatility
            var mean = values.Average();
            var variance = values.Sum(v => (v - mean) * (v - mean)) / values.Length;
            trend.Volatility = (float)Math.Sqrt(variance);
            
            // Determine stability
            trend.IsStable = trend.Volatility < (mean * 0.1f); // Less than 10% volatility
            
            return trend;
        }
        
        private float[] ExtractMetricValues(PerformanceMetricType metricType, PerformanceMetrics[] metrics)
        {
            return metricType switch
            {
                PerformanceMetricType.FPS => metrics.Select(m => m.AverageFPS).ToArray(),
                PerformanceMetricType.FrameTime => metrics.Select(m => m.AverageFrameTime).ToArray(),
                PerformanceMetricType.MemoryUsage => metrics.Select(m => (float)m.MemoryUsage).ToArray(),
                PerformanceMetricType.CPUUsage => metrics.Select(m => m.CPUUsage).ToArray(),
                PerformanceMetricType.GPUUsage => metrics.Select(m => m.GPUUsage).ToArray(),
                PerformanceMetricType.ThermalState => metrics.Select(m => m.ThermalState).ToArray(),
                _ => new float[0]
            };
        }
        
        private TrendDirection CalculateOverallTrend(IEnumerable<MetricTrend> metricTrends)
        {
            var trends = metricTrends.ToArray();
            if (trends.Length == 0)
                return TrendDirection.Stable;
            
            var improvingCount = trends.Count(t => t.Direction == TrendDirection.Improving);
            var decliningCount = trends.Count(t => t.Direction == TrendDirection.Declining);
            
            if (improvingCount > decliningCount)
                return TrendDirection.Improving;
            else if (decliningCount > improvingCount)
                return TrendDirection.Declining;
            else
                return TrendDirection.Stable;
        }
        
        private float CalculateTrendStrength(IEnumerable<MetricTrend> metricTrends)
        {
            var trends = metricTrends.ToArray();
            return trends.Length > 0 ? trends.Average(t => t.Strength) : 0f;
        }
        
        private PerformancePattern[] DetectPerformancePatterns(PerformanceMetrics[] metrics)
        {
            // Placeholder for pattern detection
            return new PerformancePattern[0];
        }
        
        private PerformanceAnomaly[] DetectPerformanceAnomalies(PerformanceMetrics[] metrics)
        {
            // Placeholder for anomaly detection
            return new PerformanceAnomaly[0];
        }
        
        private PerformancePrediction[] GeneratePerformancePredictions(PerformanceMetrics[] metrics)
        {
            // Placeholder for prediction generation
            return new PerformancePrediction[0];
        }
        
        private float CalculateAnalysisConfidence(int dataPointCount, float trendStrength)
        {
            var dataConfidence = Math.Min(1f, dataPointCount / 100f);
            var trendConfidence = trendStrength;
            
            return (dataConfidence + trendConfidence) / 2f;
        }
        
        private string[] GenerateTrendRecommendations(PerformanceTrendAnalysis analysis)
        {
            var recommendations = new List<string>();
            
            if (analysis.OverallTrend == TrendDirection.Declining)
            {
                recommendations.Add("Performance is declining - consider optimization");
                recommendations.Add("Monitor system resources for bottlenecks");
            }
            else if (analysis.OverallTrend == TrendDirection.Improving)
            {
                recommendations.Add("Performance is improving - maintain current settings");
            }
            else
            {
                recommendations.Add("Performance is stable - continue monitoring");
            }
            
            return recommendations.ToArray();
        }
        
        private ViolationSummary GenerateViolationSummary(PerformanceMonitoringSession session)
        {
            var violations = session.ViolationHistory;
            
            return new ViolationSummary
            {
                TotalViolations = violations.Count,
                CriticalViolations = violations.Count(v => v.Severity == ViolationSeverity.Critical),
                WarningViolations = violations.Count(v => v.Severity == ViolationSeverity.Warning),
                InfoViolations = violations.Count(v => v.Severity == ViolationSeverity.Info),
                TotalViolationTime = TimeSpan.FromSeconds(violations.Sum(v => v.Duration.TotalSeconds)),
                ViolationRate = CalculateViolationRate(violations, session),
                ViolationsByMetric = violations.GroupBy(v => v.MetricType).ToDictionary(g => g.Key, g => g.Count()),
                MostSevereViolation = violations.OrderByDescending(v => v.Severity).FirstOrDefault(),
                LongestViolation = violations.OrderByDescending(v => v.Duration).FirstOrDefault()
            };
        }
        
        private float CalculateViolationRate(List<PerformanceViolation> violations, PerformanceMonitoringSession session)
        {
            var sessionHours = (DateTime.UtcNow - session.StartTime).TotalHours;
            return sessionHours > 0 ? violations.Count / (float)sessionHours : 0f;
        }
        
        private string[] GenerateKeyInsights(PerformanceMonitoringSession session)
        {
            var insights = new List<string>();
            
            if (session.PerformanceHistory.Count > 0)
            {
                var avgFPS = session.PerformanceHistory.Average(m => m.AverageFPS);
                insights.Add($"Average FPS: {avgFPS:F1}");
                
                var memoryUsage = session.PerformanceHistory.Average(m => m.MemoryUsage) / (1024 * 1024);
                insights.Add($"Average memory usage: {memoryUsage:F1} MB");
            }
            
            if (session.ViolationHistory.Count > 0)
            {
                insights.Add($"Total violations: {session.ViolationHistory.Count}");
            }
            
            return insights.ToArray();
        }
        
        private string[] GenerateRecommendations(PerformanceMonitoringSession session)
        {
            var recommendations = new List<string>();
            
            if (session.ViolationHistory.Count > 10)
            {
                recommendations.Add("High number of violations - review performance thresholds");
            }
            
            if (session.PerformanceHistory.Count > 0)
            {
                var avgFPS = session.PerformanceHistory.Average(m => m.AverageFPS);
                if (avgFPS < 60f)
                {
                    recommendations.Add("Low average FPS - consider performance optimization");
                }
            }
            
            return recommendations.ToArray();
        }
        
        private float CalculateReportQuality(PerformanceMonitoringSession session)
        {
            float quality = 1.0f;
            
            // Factor in data completeness
            var expectedDataPoints = (DateTime.UtcNow - session.StartTime).TotalSeconds / _configuration.DataCollectionIntervalSeconds;
            var completeness = expectedDataPoints > 0 ? session.DataPointsCollected / (float)expectedDataPoints : 1f;
            quality *= Math.Min(1f, completeness);
            
            // Factor in session duration
            var sessionHours = (DateTime.UtcNow - session.StartTime).TotalHours;
            var durationFactor = Math.Min(1f, sessionHours / 1f); // Full quality after 1 hour
            quality *= durationFactor;
            
            return quality;
        }
        
        private string GenerateCSVExport(PerformanceMetrics[] metrics)
        {
            var csv = "Timestamp,FPS,FrameTime,MemoryUsage,CPUUsage,GPUUsage,ThermalState\n";
            
            foreach (var metric in metrics)
            {
                csv += $"{metric.CapturedAt:yyyy-MM-dd HH:mm:ss}," +
                       $"{metric.AverageFPS:F2}," +
                       $"{metric.AverageFrameTime:F2}," +
                       $"{metric.MemoryUsage}," +
                       $"{metric.CPUUsage:F2}," +
                       $"{metric.GPUUsage:F2}," +
                       $"{metric.ThermalState:F2}\n";
            }
            
            return csv;
        }
        
        private ExportQuality CalculateExportQuality(PerformanceMetrics[] exportedData, PerformanceMonitoringSession session)
        {
            var totalDataPoints = session.PerformanceHistory.Count;
            var exportedCount = exportedData.Length;
            
            return new ExportQuality
            {
                DataCompleteness = totalDataPoints > 0 ? (float)exportedCount / totalDataPoints : 1f,
                DataAccuracy = 1f, // Assume perfect accuracy for now
                MissingDataPoints = Math.Max(0, totalDataPoints - exportedCount),
                CorruptedDataPoints = 0,
                QualityIssues = new string[0]
            };
        }
        
        private string GenerateSessionSummary(PerformanceMonitoringSession session)
        {
            var duration = (session.EndTime ?? DateTime.UtcNow) - session.StartTime;
            return $"Session ran for {duration.TotalMinutes:F1} minutes, collected {session.DataPointsCollected} data points, " +
                   $"detected {session.ViolationHistory.Count} violations";
        }
        
        private PerformanceHealthIndicators GenerateHealthIndicators(PerformanceMonitoringSession session)
        {
            var statistics = session.Statistics ?? CalculateSessionStatistics(session);
            
            return new PerformanceHealthIndicators
            {
                OverallHealthScore = statistics.OverallHealthScore,
                StabilityScore = statistics.StabilityScore,
                EfficiencyScore = CalculateEfficiencyScore(session),
                ReliabilityScore = CalculateReliabilityScore(session),
                OverallStatus = DetermineHealthStatus(statistics.OverallHealthScore),
                MetricHealthStatus = GenerateMetricHealthStatus(session)
            };
        }
        
        private float CalculateEfficiencyScore(PerformanceMonitoringSession session)
        {
            // Placeholder efficiency calculation
            return 0.8f;
        }
        
        private float CalculateReliabilityScore(PerformanceMonitoringSession session)
        {
            // Placeholder reliability calculation
            return 0.9f;
        }
        
        private HealthStatus DetermineHealthStatus(float healthScore)
        {
            if (healthScore >= 0.9f) return HealthStatus.Excellent;
            if (healthScore >= 0.7f) return HealthStatus.Good;
            if (healthScore >= 0.5f) return HealthStatus.Warning;
            return HealthStatus.Critical;
        }
        
        private Dictionary<PerformanceMetricType, HealthStatus> GenerateMetricHealthStatus(PerformanceMonitoringSession session)
        {
            var status = new Dictionary<PerformanceMetricType, HealthStatus>();
            
            foreach (var metricType in _configuration.MonitoredMetrics)
            {
                // Simplified health status determination
                status[metricType] = HealthStatus.Good;
            }
            
            return status;
        }
        
        private PerformanceAlert[] GenerateActiveAlerts(PerformanceMonitoringSession session)
        {
            var alerts = new List<PerformanceAlert>();
            
            foreach (var violation in _activeViolations.Values)
            {
                if (!violation.IsResolved)
                {
                    alerts.Add(new PerformanceAlert
                    {
                        AlertId = violation.ViolationId,
                        TriggeredAt = violation.OccurredAt,
                        Severity = MapToAlertSeverity(violation.Severity),
                        AlertMessage = $"{violation.MetricType} violation: {violation.CurrentValue:F1}",
                        AffectedMetric = violation.MetricType,
                        CurrentValue = violation.CurrentValue,
                        ThresholdValue = violation.ThresholdValue,
                        IsAcknowledged = false,
                        RecommendedActions = new[] { "Monitor system performance", "Consider optimization" }
                    });
                }
            }
            
            return alerts.ToArray();
        }
        
        private AlertSeverity MapToAlertSeverity(ViolationSeverity severity)
        {
            return severity switch
            {
                ViolationSeverity.Info => AlertSeverity.Info,
                ViolationSeverity.Warning => AlertSeverity.Warning,
                ViolationSeverity.Critical => AlertSeverity.Critical,
                _ => AlertSeverity.Warning
            };
        }
        
        private Dictionary<PerformanceMetricType, TrendDirection> CalculateShortTermTrends(PerformanceMetrics[] recentHistory)
        {
            var trends = new Dictionary<PerformanceMetricType, TrendDirection>();
            
            if (recentHistory.Length < 2)
            {
                foreach (var metricType in _configuration.MonitoredMetrics)
                {
                    trends[metricType] = TrendDirection.Stable;
                }
                return trends;
            }
            
            foreach (var metricType in _configuration.MonitoredMetrics)
            {
                var values = ExtractMetricValues(metricType, recentHistory);
                var firstValue = values.First();
                var lastValue = values.Last();
                var change = lastValue - firstValue;
                
                if (Math.Abs(change) < 0.1f)
                    trends[metricType] = TrendDirection.Stable;
                else if (change > 0)
                    trends[metricType] = metricType == PerformanceMetricType.FPS ? TrendDirection.Improving : TrendDirection.Declining;
                else
                    trends[metricType] = metricType == PerformanceMetricType.FPS ? TrendDirection.Declining : TrendDirection.Improving;
            }
            
            return trends;
        }
        
        private SystemStatusIndicators GenerateSystemStatus()
        {
            return new SystemStatusIndicators
            {
                IsMonitoringActive = _isMonitoring,
                IsDataCollectionHealthy = true, // Would check actual health
                AreThresholdsConfigured = _thresholdManager != null,
                IsAlertingFunctional = true,
                SystemLoadPercentage = 25f, // Placeholder
                ActiveSessions = _activeSessions.Count,
                LastDataCollection = _activeSessions.Values.Any() ? _activeSessions.Values.Max(s => s.LastDataCollection) : DateTime.MinValue,
                SystemMessages = new string[0]
            };
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        public void Dispose()
        {
            ResetMonitoringSystem();
            
            _dataCollectionTimer?.Dispose();
            _statisticsTimer?.Dispose();
            _cleanupTimer?.Dispose();
        }
        
        #endregion
    }
}