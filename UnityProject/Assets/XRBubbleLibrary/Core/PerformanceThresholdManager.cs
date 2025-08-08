using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Manages and enforces performance thresholds across the system.
    /// Provides configurable performance budgets and threshold violation detection.
    /// </summary>
    public class PerformanceThresholdManager : IPerformanceThresholdManager
    {
        private PerformanceThresholds _currentThresholds;
        private readonly List<ThresholdViolation> _violationHistory;
        private readonly List<Action<ThresholdViolation>> _violationCallbacks;
        private readonly ViolationCallbackConfig _callbackConfig;
        private readonly Dictionary<PerformanceMetricType, Queue<float>> _recentValues;
        private readonly Dictionary<PerformanceMetricType, int> _consecutiveViolations;
        
        private const int MAX_HISTORY_SIZE = 10000;
        private const int RECENT_VALUES_WINDOW = 100;
        
        public PerformanceThresholds CurrentThresholds => _currentThresholds?.Clone();
        
        public PerformanceThresholdManager()
        {
            _currentThresholds = CreateDefaultThresholds();
            _violationHistory = new List<ThresholdViolation>();
            _violationCallbacks = new List<Action<ThresholdViolation>>();
            _callbackConfig = new ViolationCallbackConfig();
            _recentValues = new Dictionary<PerformanceMetricType, Queue<float>>();
            _consecutiveViolations = new Dictionary<PerformanceMetricType, int>();
            
            InitializeRecentValuesTracking();
        }
        
        public PerformanceThresholdManager(PerformanceThresholds initialThresholds) : this()
        {
            if (initialThresholds != null && initialThresholds.IsValid())
            {
                _currentThresholds = initialThresholds.Clone();
            }
        }
        
        public void UpdateThresholds(PerformanceThresholds thresholds)
        {
            if (thresholds == null)
            {
                Debug.LogError("Cannot update thresholds with null configuration");
                return;
            }
            
            if (!thresholds.IsValid())
            {
                Debug.LogError("Cannot update thresholds with invalid configuration");
                return;
            }
            
            var oldThresholds = _currentThresholds;
            _currentThresholds = thresholds.Clone();
            
            Debug.Log($"Performance thresholds updated. FPS: {_currentThresholds.MinimumFPS}, " +
                     $"Memory: {_currentThresholds.MaximumMemoryUsageMB}MB, " +
                     $"CPU: {_currentThresholds.MaximumCPUUsage}%");
            
            // Reset consecutive violation counters when thresholds change
            _consecutiveViolations.Clear();
        }
        
        public ThresholdValidationResult ValidatePerformance(PerformanceMetrics metrics)
        {
            if (metrics == null)
            {
                return new ThresholdValidationResult
                {
                    IsValid = false,
                    Summary = "Cannot validate null performance metrics",
                    ValidatedAt = DateTime.UtcNow,
                    OverallHealthScore = 0f
                };
            }
            
            var result = new ThresholdValidationResult
            {
                ValidatedAt = DateTime.UtcNow
            };
            
            // Check each metric type
            CheckMetric(PerformanceMetricType.FPS, metrics.AverageFPS, result);
            CheckMetric(PerformanceMetricType.FrameTime, metrics.AverageFrameTime, result);
            CheckMetric(PerformanceMetricType.MemoryUsage, metrics.MemoryUsage / (1024f * 1024f), result); // Convert to MB
            CheckMetric(PerformanceMetricType.CPUUsage, metrics.CPUUsage, result);
            CheckMetric(PerformanceMetricType.GPUUsage, metrics.GPUUsage, result);
            CheckMetric(PerformanceMetricType.ThermalState, metrics.ThermalState, result);
            
            // Calculate overall health score
            result.OverallHealthScore = CalculateHealthScore(result.Violations);
            result.IsValid = result.Violations.Count == 0;
            result.Summary = GenerateValidationSummary(result);
            
            // Store violations in history
            foreach (var violation in result.Violations)
            {
                AddViolationToHistory(violation);
                TriggerViolationCallbacks(violation);
            }
            
            return result;
        }
        
        public bool IsThresholdViolated(PerformanceMetricType metricType, float value)
        {
            return GetViolationAmount(metricType, value) > 0;
        }
        
        public List<ThresholdViolation> GetViolationHistory(TimeSpan period)
        {
            var cutoffTime = DateTime.UtcNow - period;
            return _violationHistory.Where(v => v.OccurredAt >= cutoffTime).ToList();
        }
        
        public void RegisterViolationCallback(Action<ThresholdViolation> callback)
        {
            if (callback != null && !_violationCallbacks.Contains(callback))
            {
                _violationCallbacks.Add(callback);
            }
        }
        
        public ThresholdRecommendations GetRecommendedAdjustments()
        {
            var recommendations = new ThresholdRecommendations
            {
                GeneratedAt = DateTime.UtcNow
            };
            
            // Analyze recent performance data to suggest threshold adjustments
            foreach (var metricType in Enum.GetValues(typeof(PerformanceMetricType)).Cast<PerformanceMetricType>())
            {
                if (_recentValues.ContainsKey(metricType) && _recentValues[metricType].Count > 10)
                {
                    var values = _recentValues[metricType].ToArray();
                    var recommendedThreshold = CalculateRecommendedThreshold(metricType, values);
                    
                    if (recommendedThreshold.HasValue)
                    {
                        recommendations.RecommendedThresholds[metricType] = recommendedThreshold.Value;
                    }
                }
            }
            
            recommendations.DataPointsAnalyzed = _recentValues.Values.Sum(q => q.Count);
            recommendations.ConfidenceScore = CalculateRecommendationConfidence(recommendations);
            recommendations.ReasoningReport = GenerateRecommendationReasoning(recommendations);
            
            return recommendations;
        }
        
        private void CheckMetric(PerformanceMetricType metricType, float value, ThresholdValidationResult result)
        {
            // Track recent values for trend analysis
            TrackRecentValue(metricType, value);
            
            var violationAmount = GetViolationAmount(metricType, value);
            
            if (violationAmount > 0)
            {
                // Increment consecutive violations counter
                _consecutiveViolations[metricType] = _consecutiveViolations.GetValueOrDefault(metricType, 0) + 1;
                
                var violation = new ThresholdViolation
                {
                    MetricType = metricType,
                    CurrentValue = value,
                    ThresholdValue = GetThresholdValue(metricType),
                    ViolationAmount = violationAmount,
                    Severity = DetermineSeverity(metricType, violationAmount),
                    OccurredAt = DateTime.UtcNow,
                    Description = GenerateViolationDescription(metricType, value, violationAmount)
                };
                
                violation.AdditionalData["ConsecutiveViolations"] = _consecutiveViolations[metricType];
                violation.AdditionalData["HeadroomViolation"] = IsHeadroomViolated(metricType, value);
                
                result.Violations.Add(violation);
            }
            else
            {
                // Reset consecutive violations counter on successful check
                _consecutiveViolations[metricType] = 0;
            }
        }
        
        private float GetViolationAmount(PerformanceMetricType metricType, float value)
        {
            switch (metricType)
            {
                case PerformanceMetricType.FPS:
                    return Math.Max(0, _currentThresholds.MinimumFPS - value);
                    
                case PerformanceMetricType.FrameTime:
                    return Math.Max(0, value - _currentThresholds.MaximumFrameTime);
                    
                case PerformanceMetricType.MemoryUsage:
                    return Math.Max(0, value - _currentThresholds.MaximumMemoryUsageMB);
                    
                case PerformanceMetricType.CPUUsage:
                    return Math.Max(0, value - _currentThresholds.MaximumCPUUsage);
                    
                case PerformanceMetricType.GPUUsage:
                    return Math.Max(0, value - _currentThresholds.MaximumGPUUsage);
                    
                case PerformanceMetricType.ThermalState:
                    return Math.Max(0, value - _currentThresholds.MaximumThermalState);
                    
                default:
                    return 0;
            }
        }
        
        private float GetThresholdValue(PerformanceMetricType metricType)
        {
            switch (metricType)
            {
                case PerformanceMetricType.FPS:
                    return _currentThresholds.MinimumFPS;
                case PerformanceMetricType.FrameTime:
                    return _currentThresholds.MaximumFrameTime;
                case PerformanceMetricType.MemoryUsage:
                    return _currentThresholds.MaximumMemoryUsageMB;
                case PerformanceMetricType.CPUUsage:
                    return _currentThresholds.MaximumCPUUsage;
                case PerformanceMetricType.GPUUsage:
                    return _currentThresholds.MaximumGPUUsage;
                case PerformanceMetricType.ThermalState:
                    return _currentThresholds.MaximumThermalState;
                default:
                    return 0;
            }
        }
        
        private ViolationSeverity DetermineSeverity(PerformanceMetricType metricType, float violationAmount)
        {
            var thresholdValue = GetThresholdValue(metricType);
            var violationPercentage = violationAmount / thresholdValue;
            
            // Check for consecutive violations
            var consecutiveCount = _consecutiveViolations.GetValueOrDefault(metricType, 0);
            
            if (consecutiveCount >= _currentThresholds.ViolationTolerance || violationPercentage > 0.2f)
            {
                return ViolationSeverity.Critical;
            }
            else if (violationPercentage > 0.1f || consecutiveCount > 1)
            {
                return ViolationSeverity.Warning;
            }
            else
            {
                return ViolationSeverity.Info;
            }
        }
        
        private bool IsHeadroomViolated(PerformanceMetricType metricType, float value)
        {
            var thresholdValue = GetThresholdValue(metricType);
            var headroomThreshold = thresholdValue * (1f - _currentThresholds.RequiredHeadroomPercentage);
            
            switch (metricType)
            {
                case PerformanceMetricType.FPS:
                    return value < headroomThreshold;
                default:
                    return value > headroomThreshold;
            }
        }    
    
        private float CalculateHealthScore(List<ThresholdViolation> violations)
        {
            if (violations.Count == 0)
                return 1.0f;
            
            float totalPenalty = 0f;
            foreach (var violation in violations)
            {
                float penalty = violation.Severity switch
                {
                    ViolationSeverity.Info => 0.05f,
                    ViolationSeverity.Warning => 0.15f,
                    ViolationSeverity.Critical => 0.3f,
                    _ => 0.1f
                };
                totalPenalty += penalty;
            }
            
            return Math.Max(0f, 1f - totalPenalty);
        }
        
        private string GenerateValidationSummary(ThresholdValidationResult result)
        {
            if (result.IsValid)
            {
                return $"All performance metrics within thresholds. Health score: {result.OverallHealthScore:F2}";
            }
            
            var criticalCount = result.Violations.Count(v => v.Severity == ViolationSeverity.Critical);
            var warningCount = result.Violations.Count(v => v.Severity == ViolationSeverity.Warning);
            var infoCount = result.Violations.Count(v => v.Severity == ViolationSeverity.Info);
            
            return $"Performance violations detected: {criticalCount} critical, {warningCount} warning, {infoCount} info. " +
                   $"Health score: {result.OverallHealthScore:F2}";
        }
        
        private string GenerateViolationDescription(PerformanceMetricType metricType, float value, float violationAmount)
        {
            var unit = GetMetricUnit(metricType);
            var thresholdValue = GetThresholdValue(metricType);
            
            return metricType switch
            {
                PerformanceMetricType.FPS => $"FPS dropped to {value:F1}{unit}, below threshold of {thresholdValue:F1}{unit}",
                _ => $"{metricType} at {value:F1}{unit} exceeds threshold of {thresholdValue:F1}{unit} by {violationAmount:F1}{unit}"
            };
        }
        
        private string GetMetricUnit(PerformanceMetricType metricType)
        {
            return metricType switch
            {
                PerformanceMetricType.FPS => " FPS",
                PerformanceMetricType.FrameTime => "ms",
                PerformanceMetricType.MemoryUsage => "MB",
                PerformanceMetricType.CPUUsage => "%",
                PerformanceMetricType.GPUUsage => "%",
                PerformanceMetricType.ThermalState => "",
                _ => ""
            };
        }
        
        private void AddViolationToHistory(ThresholdViolation violation)
        {
            _violationHistory.Add(violation);
            
            // Maintain history size limit
            if (_violationHistory.Count > MAX_HISTORY_SIZE)
            {
                _violationHistory.RemoveRange(0, _violationHistory.Count - MAX_HISTORY_SIZE);
            }
        }
        
        private void TriggerViolationCallbacks(ThresholdViolation violation)
        {
            if (violation.Severity < _callbackConfig.MinimumSeverity)
                return;
            
            foreach (var callback in _violationCallbacks)
            {
                try
                {
                    callback.Invoke(violation);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in violation callback: {ex.Message}");
                }
            }
        }
        
        private void InitializeRecentValuesTracking()
        {
            foreach (var metricType in Enum.GetValues(typeof(PerformanceMetricType)).Cast<PerformanceMetricType>())
            {
                _recentValues[metricType] = new Queue<float>();
                _consecutiveViolations[metricType] = 0;
            }
        }
        
        private void TrackRecentValue(PerformanceMetricType metricType, float value)
        {
            var queue = _recentValues[metricType];
            queue.Enqueue(value);
            
            if (queue.Count > RECENT_VALUES_WINDOW)
            {
                queue.Dequeue();
            }
        }
        
        private float? CalculateRecommendedThreshold(PerformanceMetricType metricType, float[] values)
        {
            if (values.Length < 10)
                return null;
            
            // Calculate statistics
            var mean = values.Average();
            var stdDev = CalculateStandardDeviation(values, mean);
            
            // Recommend threshold based on statistical analysis
            return metricType switch
            {
                PerformanceMetricType.FPS => Math.Max(60f, mean - (2 * stdDev)),
                PerformanceMetricType.FrameTime => mean + (2 * stdDev),
                PerformanceMetricType.MemoryUsage => mean + (2 * stdDev),
                PerformanceMetricType.CPUUsage => Math.Min(90f, mean + (2 * stdDev)),
                PerformanceMetricType.GPUUsage => Math.Min(95f, mean + (2 * stdDev)),
                PerformanceMetricType.ThermalState => Math.Min(0.9f, mean + (2 * stdDev)),
                _ => null
            };
        }
        
        private float CalculateStandardDeviation(float[] values, float mean)
        {
            var sumOfSquares = values.Sum(v => (v - mean) * (v - mean));
            return (float)Math.Sqrt(sumOfSquares / values.Length);
        }
        
        private float CalculateRecommendationConfidence(ThresholdRecommendations recommendations)
        {
            if (recommendations.DataPointsAnalyzed < 50)
                return 0.3f;
            else if (recommendations.DataPointsAnalyzed < 100)
                return 0.6f;
            else
                return 0.9f;
        }
        
        private string GenerateRecommendationReasoning(ThresholdRecommendations recommendations)
        {
            var reasoning = $"Analysis based on {recommendations.DataPointsAnalyzed} data points. ";
            
            if (recommendations.RecommendedThresholds.Count == 0)
            {
                reasoning += "Insufficient data for threshold recommendations.";
            }
            else
            {
                reasoning += $"Recommended adjustments for {recommendations.RecommendedThresholds.Count} metrics " +
                           $"based on statistical analysis (mean ± 2σ). ";
                reasoning += $"Confidence: {recommendations.ConfidenceScore:P0}";
            }
            
            return reasoning;
        }
        
        private static PerformanceThresholds CreateDefaultThresholds()
        {
            return new PerformanceThresholds
            {
                MinimumFPS = 72f,
                MaximumFrameTime = 13.89f, // 72 FPS
                MaximumMemoryUsageMB = 512,
                MemoryWarningThresholdMB = 400,
                MaximumCPUUsage = 80f,
                MaximumGPUUsage = 85f,
                MaximumThermalState = 0.8f,
                RequiredHeadroomPercentage = 0.3f,
                ViolationTolerance = 3,
                ViolationTimeWindow = 5f
            };
        }
    }
}