using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for continuous performance monitoring system.
    /// Provides real-time performance tracking with threshold violation detection.
    /// Implements Requirement 9.1: Continuous performance monitoring infrastructure.
    /// Implements Requirement 9.3: Threshold violation detection.
    /// </summary>
    public interface IContinuousPerformanceMonitor
    {
        /// <summary>
        /// Whether the performance monitor is currently active.
        /// </summary>
        bool IsMonitoring { get; }
        
        /// <summary>
        /// Current monitoring configuration.
        /// </summary>
        PerformanceMonitorConfiguration Configuration { get; }
        
        /// <summary>
        /// Current real-time performance metrics.
        /// </summary>
        PerformanceMetrics CurrentMetrics { get; }
        
        /// <summary>
        /// Active performance monitoring sessions.
        /// </summary>
        PerformanceMonitoringSession[] ActiveSessions { get; }
        
        /// <summary>
        /// Event fired when performance thresholds are violated.
        /// </summary>
        event Action<PerformanceViolationEventArgs> PerformanceViolationDetected;
        
        /// <summary>
        /// Event fired when performance returns to acceptable levels.
        /// </summary>
        event Action<PerformanceRecoveryEventArgs> PerformanceRecovered;
        
        /// <summary>
        /// Event fired when performance data is collected.
        /// </summary>
        event Action<PerformanceDataCollectedEventArgs> PerformanceDataCollected;
        
        /// <summary>
        /// Event fired when monitoring session starts.
        /// </summary>
        event Action<MonitoringSessionStartedEventArgs> MonitoringSessionStarted;
        
        /// <summary>
        /// Event fired when monitoring session ends.
        /// </summary>
        event Action<MonitoringSessionEndedEventArgs> MonitoringSessionEnded;
        
        /// <summary>
        /// Initialize the continuous performance monitor.
        /// </summary>
        /// <param name="config">Monitor configuration</param>
        /// <param name="thresholdManager">Performance threshold manager</param>
        /// <returns>True if initialization successful</returns>
        bool Initialize(PerformanceMonitorConfiguration config, IPerformanceThresholdManager thresholdManager);
        
        /// <summary>
        /// Start continuous performance monitoring.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <returns>Started monitoring session</returns>
        PerformanceMonitoringSession StartMonitoring(string sessionId);
        
        /// <summary>
        /// Stop continuous performance monitoring.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <returns>Session termination result</returns>
        MonitoringTerminationResult StopMonitoring(string sessionId);
        
        /// <summary>
        /// Collect current performance metrics.
        /// </summary>
        /// <returns>Current performance metrics</returns>
        PerformanceMetrics CollectCurrentMetrics();
        
        /// <summary>
        /// Get performance history for a time period.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <param name="period">Time period to retrieve</param>
        /// <returns>Performance metrics history</returns>
        PerformanceMetrics[] GetPerformanceHistory(string sessionId, TimeSpan period);
        
        /// <summary>
        /// Get performance statistics for a session.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <returns>Performance statistics</returns>
        PerformanceStatistics GetPerformanceStatistics(string sessionId);
        
        /// <summary>
        /// Configure monitoring intervals and thresholds.
        /// </summary>
        /// <param name="config">New monitoring configuration</param>
        void UpdateConfiguration(PerformanceMonitorConfiguration config);
        
        /// <summary>
        /// Set custom performance thresholds for monitoring.
        /// </summary>
        /// <param name="thresholds">Performance thresholds</param>
        void SetPerformanceThresholds(PerformanceThresholds thresholds);
        
        /// <summary>
        /// Register a custom performance metric collector.
        /// </summary>
        /// <param name="collector">Custom metric collector</param>
        void RegisterCustomMetricCollector(IPerformanceMetricCollector collector);
        
        /// <summary>
        /// Unregister a custom performance metric collector.
        /// </summary>
        /// <param name="collectorId">Collector identifier</param>
        void UnregisterCustomMetricCollector(string collectorId);
        
        /// <summary>
        /// Force immediate performance data collection.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <returns>Collected performance metrics</returns>
        PerformanceMetrics ForceDataCollection(string sessionId);
        
        /// <summary>
        /// Analyze performance trends over time.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <param name="analysisWindow">Time window for analysis</param>
        /// <returns>Performance trend analysis</returns>
        PerformanceTrendAnalysis AnalyzePerformanceTrends(string sessionId, TimeSpan analysisWindow);
        
        /// <summary>
        /// Generate performance monitoring report.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <returns>Performance monitoring report</returns>
        PerformanceMonitoringReport GenerateMonitoringReport(string sessionId);
        
        /// <summary>
        /// Export performance data in specified format.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <param name="format">Export format</param>
        /// <param name="timeRange">Time range to export</param>
        /// <returns>Exported performance data</returns>
        PerformanceDataExport ExportPerformanceData(string sessionId, DataExportFormat format, TimeSpan timeRange);
        
        /// <summary>
        /// Pause performance monitoring for a session.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <returns>True if paused successfully</returns>
        bool PauseMonitoring(string sessionId);
        
        /// <summary>
        /// Resume performance monitoring for a session.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <returns>True if resumed successfully</returns>
        bool ResumeMonitoring(string sessionId);
        
        /// <summary>
        /// Get real-time performance dashboard data.
        /// </summary>
        /// <param name="sessionId">Monitoring session identifier</param>
        /// <returns>Dashboard data for real-time display</returns>
        PerformanceDashboardData GetDashboardData(string sessionId);
        
        /// <summary>
        /// Reset performance monitoring system state.
        /// </summary>
        void ResetMonitoringSystem();
    }
}