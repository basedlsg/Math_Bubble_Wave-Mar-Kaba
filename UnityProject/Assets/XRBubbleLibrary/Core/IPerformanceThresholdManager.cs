using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for managing and enforcing performance thresholds across the system.
    /// Provides configurable performance budgets and threshold violation detection.
    /// </summary>
    public interface IPerformanceThresholdManager
    {
        /// <summary>
        /// Gets the current performance thresholds configuration
        /// </summary>
        PerformanceThresholds CurrentThresholds { get; }
        
        /// <summary>
        /// Updates the performance thresholds with new values
        /// </summary>
        /// <param name="thresholds">New threshold configuration</param>
        void UpdateThresholds(PerformanceThresholds thresholds);
        
        /// <summary>
        /// Validates current performance metrics against established thresholds
        /// </summary>
        /// <param name="metrics">Performance metrics to validate</param>
        /// <returns>Validation result with details of any violations</returns>
        ThresholdValidationResult ValidatePerformance(PerformanceMetrics metrics);
        
        /// <summary>
        /// Checks if a specific metric violates its threshold
        /// </summary>
        /// <param name="metricType">Type of metric to check</param>
        /// <param name="value">Current value of the metric</param>
        /// <returns>True if threshold is violated</returns>
        bool IsThresholdViolated(PerformanceMetricType metricType, float value);
        
        /// <summary>
        /// Gets threshold violation history for analysis
        /// </summary>
        /// <param name="period">Time period to retrieve history for</param>
        /// <returns>List of threshold violations in the specified period</returns>
        List<ThresholdViolation> GetViolationHistory(TimeSpan period);
        
        /// <summary>
        /// Registers a callback to be invoked when thresholds are violated
        /// </summary>
        /// <param name="callback">Callback to invoke on threshold violation</param>
        void RegisterViolationCallback(Action<ThresholdViolation> callback);
        
        /// <summary>
        /// Calculates recommended threshold adjustments based on historical data
        /// </summary>
        /// <returns>Recommended threshold adjustments</returns>
        ThresholdRecommendations GetRecommendedAdjustments();
    }
}