using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Headroom rule enforcement system for Quest 3 performance management.
    /// Implements Requirement 6.3: 30% headroom rule enforcement.
    /// Implements Requirement 6.4: Feature addition evaluation against budget.
    /// Implements Requirement 6.5: Automatic warnings when headroom is insufficient.
    /// </summary>
    public class HeadroomRuleEnforcer : MonoBehaviour, IHeadroomRuleEnforcer
    {
        // Configuration and state
        private HeadroomEnforcementConfiguration _configuration;
        private HeadroomStatus _currentStatus;
        private HeadroomWarningThresholds _warningThresholds;
        private bool _isInitialized;
        private bool _automaticMonitoringEnabled;
        
        // Data collection and history
        private readonly List<HeadroomComplianceEntry> _complianceHistory = new List<HeadroomComplianceEntry>();
        private readonly List<HeadroomEnforcementEntry> _enforcementHistory = new List<HeadroomEnforcementEntry>();
        private readonly Queue<PerformanceData> _performanceDataHistory = new Queue<PerformanceData>();
        
        // Monitoring state
        private float _lastMonitoringTime;
        private int _consecutiveViolations;
        private int _consecutiveWarnings;
        
        // Constants
        private const int MAX_HISTORY_ENTRIES = 1000;
        private const int MAX_PERFORMANCE_HISTORY = 100;
        private const float TREND_ANALYSIS_WINDOW = 60f; // seconds
        
        // Events
        public event Action<HeadroomViolationEventArgs> HeadroomViolationDetected;
        public event Action<HeadroomWarningEventArgs> HeadroomWarningTriggered;
        
        /// <summary>
        /// Whether the headroom rule enforcer is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Current headroom enforcement configuration.
        /// </summary>
        public HeadroomEnforcementConfiguration Configuration => _configuration;
        
        /// <summary>
        /// Current performance headroom status.
        /// </summary>
        public HeadroomStatus CurrentHeadroomStatus => _currentStatus;
        
        private void Awake()
        {
            // Initialize with default configuration
            Initialize(HeadroomEnforcementConfiguration.Quest3Default);
        }
        
        private void Update()
        {
            if (!_isInitialized || !_automaticMonitoringEnabled)
                return;
            
            // Check if it's time for monitoring update
            if (Time.time - _lastMonitoringTime >= _configuration.MonitoringIntervalSeconds)
            {
                PerformAutomaticMonitoring();
                _lastMonitoringTime = Time.time;
            }
        }
        
        /// <summary>
        /// Initialize the headroom rule enforcement system.
        /// </summary>
        public bool Initialize(HeadroomEnforcementConfiguration config)
        {
            try
            {
                _configuration = config;
                
                // Validate configuration
                if (config.TargetFrameRate <= 0)
                {
                    UnityEngine.Debug.LogError("[HeadroomRuleEnforcer] Invalid target frame rate");
                    return false;
                }
                
                if (config.RequiredHeadroomPercent < 0 || config.RequiredHeadroomPercent > 100)
                {
                    UnityEngine.Debug.LogError("[HeadroomRuleEnforcer] Invalid required headroom percentage");
                    return false;
                }
                
                // Initialize warning thresholds
                _warningThresholds = new HeadroomWarningThresholds
                {
                    WarningThreshold = config.WarningThresholdPercent,
                    CriticalThreshold = config.CriticalThresholdPercent,
                    ViolationThreshold = 0f,
                    TrendAnalysisWindow = TREND_ANALYSIS_WINDOW
                };
                
                // Initialize status
                _currentStatus = new HeadroomStatus
                {
                    AvailableHeadroomPercent = 100f,
                    ComplianceLevel = HeadroomComplianceLevel.Compliant,
                    TimeToViolation = TimeSpan.MaxValue,
                    IsCompliant = true,
                    LastUpdateTime = DateTime.Now
                };
                
                _automaticMonitoringEnabled = config.EnableAutomaticMonitoring;
                _lastMonitoringTime = Time.time;
                
                _isInitialized = true;
                
                if (_configuration.EnableDebugLogging)
                {
                    UnityEngine.Debug.Log($"[HeadroomRuleEnforcer] Initialized with {config.RequiredHeadroomPercent}% headroom requirement");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[HeadroomRuleEnforcer] Initialization failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate current performance against headroom rules.
        /// </summary>
        public HeadroomValidationResult ValidateHeadroom(PerformanceData performanceData)
        {
            if (!_isInitialized)
            {
                return new HeadroomValidationResult
                {
                    IsValid = false,
                    ValidationMessages = new[] { "Headroom enforcer not initialized" },
                    ValidationTimestamp = DateTime.Now
                };
            }
            
            // Calculate current headroom
            float headroomPercent = CalculateHeadroomPercentage(performanceData);
            var complianceLevel = DetermineComplianceLevel(headroomPercent);
            
            // Update current status
            UpdateCurrentStatus(headroomPercent, complianceLevel, performanceData);
            
            // Add to performance history
            AddToPerformanceHistory(performanceData);
            
            // Generate validation messages
            var messages = new List<string>();
            var recommendations = new List<string>();
            
            bool isValid = complianceLevel == HeadroomComplianceLevel.Compliant;
            
            switch (complianceLevel)
            {
                case HeadroomComplianceLevel.Compliant:
                    messages.Add($"Headroom compliant: {headroomPercent:F1}% available");
                    break;
                    
                case HeadroomComplianceLevel.Warning:
                    messages.Add($"Headroom warning: Only {headroomPercent:F1}% available (target: {_configuration.RequiredHeadroomPercent}%)");
                    recommendations.Add("Consider optimizing performance or reducing feature complexity");
                    TriggerHeadroomWarning(headroomPercent, complianceLevel, performanceData);
                    break;
                    
                case HeadroomComplianceLevel.Critical:
                    messages.Add($"Critical headroom: Only {headroomPercent:F1}% available (target: {_configuration.RequiredHeadroomPercent}%)");
                    recommendations.Add("Immediate performance optimization required");
                    recommendations.Add("Consider disabling non-essential features");
                    TriggerHeadroomWarning(headroomPercent, complianceLevel, performanceData);
                    break;
                    
                case HeadroomComplianceLevel.Violation:
                    messages.Add($"Headroom violation: Only {headroomPercent:F1}% available (minimum: {_configuration.CriticalThresholdPercent}%)");
                    recommendations.Add("Critical performance issues detected");
                    recommendations.Add("Disable features immediately to restore headroom");
                    TriggerHeadroomViolation(headroomPercent, complianceLevel, performanceData);
                    break;
            }
            
            // Record compliance entry
            RecordComplianceEntry(headroomPercent, complianceLevel, performanceData);
            
            return new HeadroomValidationResult
            {
                IsValid = isValid,
                HeadroomPercent = headroomPercent,
                ComplianceLevel = complianceLevel,
                ValidationMessages = messages.ToArray(),
                RecommendedActions = recommendations.ToArray(),
                ValidationTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Evaluate whether a new feature can be added within headroom constraints.
        /// </summary>
        public FeatureAdditionEvaluation EvaluateFeatureAddition(FeaturePerformanceImpact featureImpact, PerformanceData currentPerformance)
        {
            if (!_isInitialized)
            {
                return new FeatureAdditionEvaluation
                {
                    CanAddFeature = false,
                    RiskLevel = FeatureAdditionRisk.Critical,
                    EvaluationDetails = new[] { "Headroom enforcer not initialized" },
                    EvaluationTimestamp = DateTime.Now
                };
            }
            
            // Calculate current headroom
            float currentHeadroom = CalculateHeadroomPercentage(currentPerformance);
            
            // Estimate performance after feature addition
            var projectedPerformance = ProjectPerformanceWithFeature(currentPerformance, featureImpact);
            float projectedHeadroom = CalculateHeadroomPercentage(projectedPerformance);
            
            // Determine if feature can be added
            bool canAddFeature = projectedHeadroom >= _configuration.RequiredHeadroomPercent;
            var riskLevel = DetermineFeatureAdditionRisk(projectedHeadroom);
            
            // Generate evaluation details
            var details = new List<string>();
            var mitigations = new List<string>();
            
            details.Add($"Current headroom: {currentHeadroom:F1}%");
            details.Add($"Projected headroom after feature: {projectedHeadroom:F1}%");
            details.Add($"Feature impact: CPU {featureImpact.EstimatedCPUImpact:F1}%, GPU {featureImpact.EstimatedGPUImpact:F1}%");
            details.Add($"Confidence level: {featureImpact.ConfidenceLevel:F1}");
            
            if (!canAddFeature)
            {
                details.Add($"Feature would violate {_configuration.RequiredHeadroomPercent}% headroom requirement");
                mitigations.Add("Optimize existing features to free up performance budget");
                mitigations.Add("Reduce feature complexity or scope");
                mitigations.Add("Implement feature with performance scaling options");
            }
            else if (riskLevel != FeatureAdditionRisk.Low)
            {
                mitigations.Add("Monitor performance closely after feature addition");
                mitigations.Add("Implement feature with ability to disable if needed");
                mitigations.Add("Consider phased rollout to validate performance impact");
            }
            
            // Record enforcement action
            var action = canAddFeature ? HeadroomEnforcementAction.FeatureApproved : HeadroomEnforcementAction.FeatureBlocked;
            RecordEnforcementAction(action, currentHeadroom, $"Feature '{featureImpact.FeatureName}' evaluation", currentPerformance);
            
            return new FeatureAdditionEvaluation
            {
                CanAddFeature = canAddFeature,
                ProjectedHeadroomPercent = projectedHeadroom,
                RiskLevel = riskLevel,
                EvaluationDetails = details.ToArray(),
                RecommendedMitigations = mitigations.ToArray(),
                EvaluationTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Calculate available headroom for new features.
        /// </summary>
        public AvailableHeadroomAnalysis CalculateAvailableHeadroom(PerformanceData performanceData)
        {
            if (!_isInitialized)
            {
                return new AvailableHeadroomAnalysis
                {
                    AnalysisTimestamp = DateTime.Now
                };
            }
            
            // Calculate overall headroom
            float totalHeadroom = CalculateHeadroomPercentage(performanceData);
            
            // Calculate component-specific headroom
            float targetFrameTimeMs = 1000f / _configuration.TargetFrameRate;
            float currentFrameTimeMs = performanceData.CurrentFrameTimeMs;
            float availableFrameTimeMs = Math.Max(0, targetFrameTimeMs - currentFrameTimeMs);
            
            // Estimate component headroom (simplified calculation)
            float availableCPU = Math.Max(0, 100f - performanceData.CPUUsagePercent);
            float availableGPU = Math.Max(0, 100f - performanceData.GPUUsagePercent);
            
            // Component breakdown
            var componentHeadroom = new Dictionary<string, float>
            {
                { "CPU", availableCPU },
                { "GPU", availableGPU },
                { "Memory", 100f }, // Simplified - would need actual memory limits
                { "Thermal", (1f - performanceData.ThermalState) * 100f }
            };
            
            return new AvailableHeadroomAnalysis
            {
                TotalAvailablePercent = totalHeadroom,
                AvailableCPUPercent = availableCPU,
                AvailableGPUPercent = availableGPU,
                AvailableMemoryMB = 1000f, // Simplified
                AvailableFrameTimeMs = availableFrameTimeMs,
                ComponentHeadroom = componentHeadroom,
                AnalysisTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Generate headroom compliance report.
        /// </summary>
        public HeadroomComplianceReport GenerateComplianceReport(PerformanceData[] performanceHistory)
        {
            if (!_isInitialized || performanceHistory == null || performanceHistory.Length == 0)
            {
                return new HeadroomComplianceReport
                {
                    ReportTimestamp = DateTime.Now
                };
            }
            
            // Calculate compliance statistics
            var headroomValues = performanceHistory.Select(p => CalculateHeadroomPercentage(p)).ToArray();
            var complianceResults = headroomValues.Select(h => h >= _configuration.RequiredHeadroomPercent).ToArray();
            
            int compliantCount = complianceResults.Count(c => c);
            float compliancePercentage = (float)compliantCount / complianceResults.Length * 100f;
            
            int violationCount = _complianceHistory.Count(c => c.ComplianceLevel == HeadroomComplianceLevel.Violation);
            int warningCount = _complianceHistory.Count(c => c.ComplianceLevel == HeadroomComplianceLevel.Warning || 
                                                            c.ComplianceLevel == HeadroomComplianceLevel.Critical);
            
            float averageHeadroom = headroomValues.Average();
            float minimumHeadroom = headroomValues.Min();
            
            // Determine trend
            var trend = DetermineComplianceTrend(headroomValues);
            
            var reportingPeriod = performanceHistory.Length > 0 ? 
                performanceHistory.Last().Timestamp - performanceHistory.First().Timestamp : 
                TimeSpan.Zero;
            
            return new HeadroomComplianceReport
            {
                IsCompliant = compliancePercentage >= 95f, // 95% compliance threshold
                CompliancePercentage = compliancePercentage,
                ViolationCount = violationCount,
                WarningCount = warningCount,
                AverageHeadroom = averageHeadroom,
                MinimumHeadroom = minimumHeadroom,
                Trend = trend,
                ComplianceHistory = _complianceHistory.ToArray(),
                ReportTimestamp = DateTime.Now,
                ReportingPeriod = reportingPeriod
            };
        }
        
        /// <summary>
        /// Set headroom warning thresholds.
        /// </summary>
        public void SetWarningThresholds(HeadroomWarningThresholds thresholds)
        {
            _warningThresholds = thresholds;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[HeadroomRuleEnforcer] Warning thresholds updated: Warning={thresholds.WarningThreshold}%, Critical={thresholds.CriticalThreshold}%");
            }
        }
        
        /// <summary>
        /// Enable or disable automatic headroom monitoring.
        /// </summary>
        public void SetAutomaticMonitoring(bool enabled)
        {
            _automaticMonitoringEnabled = enabled;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[HeadroomRuleEnforcer] Automatic monitoring {(enabled ? "enabled" : "disabled")}");
            }
        }
        
        /// <summary>
        /// Get headroom rule enforcement history.
        /// </summary>
        public HeadroomEnforcementHistory GetEnforcementHistory()
        {
            var startTime = _enforcementHistory.Count > 0 ? _enforcementHistory.First().Timestamp : DateTime.Now;
            var endTime = _enforcementHistory.Count > 0 ? _enforcementHistory.Last().Timestamp : DateTime.Now;
            
            return new HeadroomEnforcementHistory
            {
                TotalChecks = _complianceHistory.Count,
                ViolationsDetected = _enforcementHistory.Count(e => e.Action == HeadroomEnforcementAction.ViolationDetected),
                WarningsIssued = _enforcementHistory.Count(e => e.Action == HeadroomEnforcementAction.WarningIssued),
                FeaturesBlocked = _enforcementHistory.Count(e => e.Action == HeadroomEnforcementAction.FeatureBlocked),
                EnforcementHistory = _enforcementHistory.ToArray(),
                HistoryStartTime = startTime,
                HistoryEndTime = endTime
            };
        }
        
        /// <summary>
        /// Reset headroom enforcement state and history.
        /// </summary>
        public void ResetEnforcementState()
        {
            _complianceHistory.Clear();
            _enforcementHistory.Clear();
            _performanceDataHistory.Clear();
            
            _currentStatus = new HeadroomStatus
            {
                AvailableHeadroomPercent = 100f,
                ComplianceLevel = HeadroomComplianceLevel.Compliant,
                TimeToViolation = TimeSpan.MaxValue,
                IsCompliant = true,
                LastUpdateTime = DateTime.Now
            };
            
            _consecutiveViolations = 0;
            _consecutiveWarnings = 0;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log("[HeadroomRuleEnforcer] Enforcement state reset");
            }
        }
        
        // Private helper methods
        
        private void PerformAutomaticMonitoring()
        {
            // Collect current performance data (simplified - would integrate with actual performance monitoring)
            var currentPerformance = new PerformanceData
            {
                CurrentFrameRate = 1f / Time.deltaTime,
                CurrentFrameTimeMs = Time.deltaTime * 1000f,
                CPUUsagePercent = 50f, // Would get from actual monitoring
                GPUUsagePercent = 45f, // Would get from actual monitoring
                MemoryUsageMB = 500f, // Would get from actual monitoring
                ThermalState = 0.2f, // Would get from actual monitoring
                Timestamp = DateTime.Now,
                AdditionalMetrics = new Dictionary<string, float>()
            };
            
            // Validate headroom
            ValidateHeadroom(currentPerformance);
        }
        
        private float CalculateHeadroomPercentage(PerformanceData performanceData)
        {
            // Calculate headroom based on frame rate performance
            float targetFrameTime = 1000f / _configuration.TargetFrameRate; // ms
            float currentFrameTime = performanceData.CurrentFrameTimeMs;
            
            if (currentFrameTime <= 0)
                return 100f;
            
            // Headroom = (target - current) / target * 100
            float headroomPercent = Math.Max(0, (targetFrameTime - currentFrameTime) / targetFrameTime * 100f);
            
            return headroomPercent;
        }
        
        private HeadroomComplianceLevel DetermineComplianceLevel(float headroomPercent)
        {
            if (headroomPercent >= _configuration.RequiredHeadroomPercent)
                return HeadroomComplianceLevel.Compliant;
            else if (headroomPercent >= _configuration.WarningThresholdPercent)
                return HeadroomComplianceLevel.Warning;
            else if (headroomPercent >= _configuration.CriticalThresholdPercent)
                return HeadroomComplianceLevel.Critical;
            else
                return HeadroomComplianceLevel.Violation;
        }
        
        private void UpdateCurrentStatus(float headroomPercent, HeadroomComplianceLevel complianceLevel, PerformanceData performanceData)
        {
            _currentStatus = new HeadroomStatus
            {
                AvailableHeadroomPercent = headroomPercent,
                ComplianceLevel = complianceLevel,
                TimeToViolation = EstimateTimeToViolation(performanceData),
                IsCompliant = complianceLevel == HeadroomComplianceLevel.Compliant,
                LastUpdateTime = DateTime.Now
            };
        }
        
        private TimeSpan EstimateTimeToViolation(PerformanceData performanceData)
        {
            // Simplified estimation - would use trend analysis in real implementation
            if (_currentStatus.ComplianceLevel == HeadroomComplianceLevel.Compliant)
                return TimeSpan.MaxValue;
            
            // Estimate based on current degradation rate
            return TimeSpan.FromMinutes(10); // Placeholder
        }
        
        private void AddToPerformanceHistory(PerformanceData performanceData)
        {
            _performanceDataHistory.Enqueue(performanceData);
            
            while (_performanceDataHistory.Count > MAX_PERFORMANCE_HISTORY)
            {
                _performanceDataHistory.Dequeue();
            }
        }
        
        private void TriggerHeadroomWarning(float headroomPercent, HeadroomComplianceLevel level, PerformanceData performanceData)
        {
            _consecutiveWarnings++;
            
            var warningArgs = new HeadroomWarningEventArgs
            {
                Message = $"Headroom {level.ToString().ToLower()}: {headroomPercent:F1}% available",
                HeadroomPercent = headroomPercent,
                WarningLevel = level,
                PerformanceData = performanceData,
                Timestamp = DateTime.Now
            };
            
            HeadroomWarningTriggered?.Invoke(warningArgs);
            
            RecordEnforcementAction(HeadroomEnforcementAction.WarningIssued, headroomPercent, 
                $"Headroom {level} warning", performanceData);
        }
        
        private void TriggerHeadroomViolation(float headroomPercent, HeadroomComplianceLevel level, PerformanceData performanceData)
        {
            _consecutiveViolations++;
            
            var violationArgs = new HeadroomViolationEventArgs
            {
                HeadroomPercent = headroomPercent,
                Severity = level,
                PerformanceData = performanceData,
                Timestamp = DateTime.Now,
                RecommendedActions = new[]
                {
                    "Disable non-essential features immediately",
                    "Reduce rendering quality",
                    "Optimize performance-critical code paths"
                }
            };
            
            HeadroomViolationDetected?.Invoke(violationArgs);
            
            RecordEnforcementAction(HeadroomEnforcementAction.ViolationDetected, headroomPercent, 
                "Headroom violation detected", performanceData);
        }
        
        private void RecordComplianceEntry(float headroomPercent, HeadroomComplianceLevel complianceLevel, PerformanceData performanceData)
        {
            var entry = new HeadroomComplianceEntry
            {
                Timestamp = DateTime.Now,
                HeadroomPercent = headroomPercent,
                ComplianceLevel = complianceLevel,
                PerformanceData = performanceData
            };
            
            _complianceHistory.Add(entry);
            
            // Maintain history size
            while (_complianceHistory.Count > MAX_HISTORY_ENTRIES)
            {
                _complianceHistory.RemoveAt(0);
            }
        }
        
        private void RecordEnforcementAction(HeadroomEnforcementAction action, float headroomPercent, string details, PerformanceData performanceData)
        {
            var entry = new HeadroomEnforcementEntry
            {
                Timestamp = DateTime.Now,
                Action = action,
                HeadroomPercent = headroomPercent,
                ActionDetails = details,
                PerformanceData = performanceData
            };
            
            _enforcementHistory.Add(entry);
            
            // Maintain history size
            while (_enforcementHistory.Count > MAX_HISTORY_ENTRIES)
            {
                _enforcementHistory.RemoveAt(0);
            }
        }
        
        private PerformanceData ProjectPerformanceWithFeature(PerformanceData currentPerformance, FeaturePerformanceImpact featureImpact)
        {
            return new PerformanceData
            {
                CurrentFrameRate = currentPerformance.CurrentFrameRate, // Would be recalculated
                CurrentFrameTimeMs = currentPerformance.CurrentFrameTimeMs + featureImpact.EstimatedFrameTimeImpact,
                CPUUsagePercent = Math.Min(100f, currentPerformance.CPUUsagePercent + featureImpact.EstimatedCPUImpact),
                GPUUsagePercent = Math.Min(100f, currentPerformance.GPUUsagePercent + featureImpact.EstimatedGPUImpact),
                MemoryUsageMB = currentPerformance.MemoryUsageMB + featureImpact.EstimatedMemoryImpact,
                ThermalState = currentPerformance.ThermalState, // Simplified
                Timestamp = DateTime.Now,
                AdditionalMetrics = currentPerformance.AdditionalMetrics
            };
        }
        
        private FeatureAdditionRisk DetermineFeatureAdditionRisk(float projectedHeadroom)
        {
            if (projectedHeadroom >= _configuration.RequiredHeadroomPercent + 10f)
                return FeatureAdditionRisk.Low;
            else if (projectedHeadroom >= _configuration.RequiredHeadroomPercent)
                return FeatureAdditionRisk.Medium;
            else if (projectedHeadroom >= _configuration.CriticalThresholdPercent)
                return FeatureAdditionRisk.High;
            else
                return FeatureAdditionRisk.Critical;
        }
        
        private ComplianceTrend DetermineComplianceTrend(float[] headroomValues)
        {
            if (headroomValues.Length < 10)
                return ComplianceTrend.Stable;
            
            // Simple trend analysis - compare first and last halves
            int halfPoint = headroomValues.Length / 2;
            float firstHalfAverage = headroomValues.Take(halfPoint).Average();
            float secondHalfAverage = headroomValues.Skip(halfPoint).Average();
            
            float difference = secondHalfAverage - firstHalfAverage;
            
            if (difference > 2f)
                return ComplianceTrend.Improving;
            else if (difference < -2f)
                return ComplianceTrend.Degrading;
            else
                return ComplianceTrend.Stable;
        }
    }
}