using System;
using System.Collections.Generic;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Interface for comfort validation feedback loop system.
    /// Implements wave parameter modification based on comfort data.
    /// Implements Requirement 7.4: Comfort data analysis and parameter adjustment.
    /// Implements Requirement 7.5: Automatic comfort validation failure handling.
    /// </summary>
    public interface IComfortValidationFeedbackLoop
    {
        /// <summary>
        /// Whether the feedback loop is currently active.
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Current feedback loop configuration.
        /// </summary>
        ComfortFeedbackConfiguration Configuration { get; }
        
        /// <summary>
        /// Current wave parameter adjustments being applied.
        /// </summary>
        WaveParameterAdjustment[] ActiveAdjustments { get; }
        
        /// <summary>
        /// Event fired when comfort validation fails and adjustments are triggered.
        /// </summary>
        event Action<ComfortValidationFailedEventArgs> ComfortValidationFailed;
        
        /// <summary>
        /// Event fired when wave parameters are automatically adjusted.
        /// </summary>
        event Action<ParameterAdjustmentEventArgs> ParametersAdjusted;
        
        /// <summary>
        /// Event fired when comfort validation passes after adjustments.
        /// </summary>
        event Action<ComfortValidationPassedEventArgs> ComfortValidationPassed;
        
        /// <summary>
        /// Event fired when automatic rollback is triggered.
        /// </summary>
        event Action<AutomaticRollbackEventArgs> AutomaticRollbackTriggered;
        
        /// <summary>
        /// Initialize the comfort validation feedback loop.
        /// </summary>
        /// <param name="config">Feedback loop configuration</param>
        /// <param name="dataCollectionSystem">Comfort data collection system</param>
        /// <param name="parameterValidator">Wave parameter validator</param>
        /// <returns>True if initialization successful</returns>
        bool Initialize(ComfortFeedbackConfiguration config, 
                       IComfortDataCollectionSystem dataCollectionSystem,
                       IWaveParameterValidator parameterValidator);
        
        /// <summary>
        /// Start the comfort validation feedback loop for a session.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <param name="initialWaveSettings">Initial wave matrix settings</param>
        /// <returns>Feedback loop session information</returns>
        ComfortFeedbackSession StartFeedbackLoop(string sessionId, WaveMatrixSettings initialWaveSettings);
        
        /// <summary>
        /// Process new comfort data and determine if adjustments are needed.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <param name="comfortData">New comfort data to process</param>
        /// <returns>Processing result with any adjustments made</returns>
        ComfortDataProcessingResult ProcessComfortData(string sessionId, ComfortDataPoint comfortData);
        
        /// <summary>
        /// Analyze comfort trends and predict potential issues.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <returns>Comfort trend analysis with predictions</returns>
        ComfortTrendAnalysis AnalyzeComfortTrends(string sessionId);
        
        /// <summary>
        /// Apply automatic wave parameter adjustments based on comfort data.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <param name="adjustmentStrategy">Strategy to use for adjustments</param>
        /// <returns>Parameter adjustment result</returns>
        ParameterAdjustmentResult ApplyAutomaticAdjustments(string sessionId, AdjustmentStrategy adjustmentStrategy);
        
        /// <summary>
        /// Validate current comfort levels against thresholds.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <returns>Comfort validation result</returns>
        ComfortValidationResult ValidateComfortLevels(string sessionId);
        
        /// <summary>
        /// Trigger automatic rollback to last known good settings.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <param name="rollbackReason">Reason for rollback</param>
        /// <returns>Rollback operation result</returns>
        RollbackResult TriggerAutomaticRollback(string sessionId, string rollbackReason);
        
        /// <summary>
        /// Get recommended wave parameter adjustments based on comfort data.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <param name="comfortIssues">Identified comfort issues</param>
        /// <returns>Recommended parameter adjustments</returns>
        WaveParameterAdjustment[] GetRecommendedAdjustments(string sessionId, ComfortIssue[] comfortIssues);
        
        /// <summary>
        /// Apply manual wave parameter adjustments.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <param name="adjustments">Manual adjustments to apply</param>
        /// <returns>Manual adjustment result</returns>
        ParameterAdjustmentResult ApplyManualAdjustments(string sessionId, WaveParameterAdjustment[] adjustments);
        
        /// <summary>
        /// Get feedback loop performance metrics.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <returns>Performance metrics for the feedback loop</returns>
        FeedbackLoopMetrics GetPerformanceMetrics(string sessionId);
        
        /// <summary>
        /// Configure comfort validation thresholds.
        /// </summary>
        /// <param name="thresholds">Comfort validation thresholds</param>
        void ConfigureComfortThresholds(ComfortValidationThresholds thresholds);
        
        /// <summary>
        /// Configure automatic adjustment strategies.
        /// </summary>
        /// <param name="strategies">Available adjustment strategies</param>
        void ConfigureAdjustmentStrategies(AdjustmentStrategy[] strategies);
        
        /// <summary>
        /// Stop the feedback loop for a session.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <returns>Session termination result</returns>
        FeedbackLoopTerminationResult StopFeedbackLoop(string sessionId);
        
        /// <summary>
        /// Generate feedback loop report for a completed session.
        /// </summary>
        /// <param name="sessionId">Feedback loop session identifier</param>
        /// <returns>Comprehensive feedback loop report</returns>
        ComfortFeedbackReport GenerateFeedbackReport(string sessionId);
        
        /// <summary>
        /// Reset feedback loop system state.
        /// </summary>
        void ResetFeedbackSystem();
    }
}