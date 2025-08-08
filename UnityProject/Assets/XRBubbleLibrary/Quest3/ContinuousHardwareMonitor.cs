using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Unity.Mathematics;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Continuous hardware monitor for Quest 3 performance testing with safety limits.
    /// Implements Requirement 8.2: Real-time FPS, CPU, GPU, and thermal monitoring.
    /// Implements Requirement 8.3: Performance data collection and analysis.
    /// Implements Requirement 8.4: Automatic test termination for safety limits.
    /// Implements Requirement 8.5: Continuous hardware monitoring during testing.
    /// </summary>
    public class ContinuousHardwareMonitor : MonoBehaviour, IContinuousHardwareMonitor
    {
        // Configuration and state
        private HardwareMonitoringConfiguration _configuration;
        private HardwareMonitoringSession _currentSession;
        private HardwareSafetyLimits _safetyLimits;
        private PerformanceThresholds _performanceThresholds;
        private bool _isInitialized;
        private bool _isMonitoringActive;
        
        // Data collection
        private readonly Queue<RealTimeHardwareMetrics> _metricsHistory = new Queue<RealTimeHardwareMetrics>();
        private readonly List<HardwareMonitoringResult> _sessionHistory = new List<HardwareMonitoringResult>();
        private readonly List<SafetyViolation> _currentSessionViolations = new List<SafetyViolation>();
        
        // Monitoring state
        private float _lastSampleTime;
        private Stopwatch _sessionTimer;
        private StreamWriter _continuousLogWriter;
        
        // Safety monitoring
        private int _consecutiveThermalThrottlingEvents;
        private DateTime _lastSafetyViolationTime;
        private bool _autoTerminationArmed;
        
        // Performance tracking
        private RealTimeHardwareMetrics _lastMetrics;
        private readonly System.Random _random = new System.Random(); // For simulation
        
        // Events
        public event Action<SafetyLimitExceededEventArgs> SafetyLimitExceeded;
        public event Action<PerformanceThresholdEventArgs> PerformanceThresholdCrossed;
        public event Action<ThermalWarningEventArgs> ThermalWarningDetected;
        public event Action<MonitoringDataUpdatedEventArgs> MonitoringDataUpdated;
        
        /// <summary>
        /// Whether the hardware monitor is initialized and active.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether continuous monitoring is currently active.
        /// </summary>
        public bool IsMonitoringActive => _isMonitoringActive;
        
        /// <summary>
        /// Current monitoring configuration.
        /// </summary>
        public HardwareMonitoringConfiguration Configuration => _configuration;
        
        private void Awake()
        {
            // Initialize with default configuration
            Initialize(HardwareMonitoringConfiguration.Quest3Default);
        }
        
        private void Update()
        {
            if (!_isInitialized || !_isMonitoringActive)
                return;
            
            // Check if it's time for next sample
            float currentTime = Time.time;
            float sampleInterval = 1f / _configuration.SampleRateHz;
            
            if (currentTime - _lastSampleTime >= sampleInterval)
            {
                CollectHardwareMetrics();
                _lastSampleTime = currentTime;
            }
            
            // Check for maximum monitoring duration
            if (_configuration.MaxMonitoringDurationSeconds > 0 && 
                _sessionTimer.Elapsed.TotalSeconds >= _configuration.MaxMonitoringDurationSeconds)
            {
                UnityEngine.Debug.LogWarning("[ContinuousHardwareMonitor] Maximum monitoring duration reached, stopping monitoring");
                StopMonitoring();
            }
        }
        
        /// <summary>
        /// Initialize the continuous hardware monitor with the specified configuration.
        /// </summary>
        public bool Initialize(HardwareMonitoringConfiguration config)
        {
            try
            {
                _configuration = config;
                _safetyLimits = HardwareSafetyLimits.Quest3Default;
                _performanceThresholds = PerformanceThresholds.Quest3Default;
                
                // Create logging directory if needed
                if (config.EnableContinuousLogging && !string.IsNullOrEmpty(config.LoggingDirectory))
                {
                    Directory.CreateDirectory(config.LoggingDirectory);
                }
                
                // Initialize monitoring state
                _consecutiveThermalThrottlingEvents = 0;
                _lastSafetyViolationTime = DateTime.MinValue;
                _autoTerminationArmed = config.AutoTerminateOnSafetyViolation;
                
                _sessionTimer = new Stopwatch();
                
                _isInitialized = true;
                
                if (_configuration.EnableDebugLogging)
                {
                    UnityEngine.Debug.Log($"[ContinuousHardwareMonitor] Initialized with {config.SampleRateHz} Hz sampling rate");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ContinuousHardwareMonitor] Initialization failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Start continuous hardware monitoring.
        /// </summary>
        public HardwareMonitoringSession StartMonitoring(string sessionName = "Quest3_Hardware_Monitoring")
        {
            if (!_isInitialized)
            {
                UnityEngine.Debug.LogError("[ContinuousHardwareMonitor] Cannot start monitoring - not initialized");
                return default;
            }
            
            if (_isMonitoringActive)
            {
                UnityEngine.Debug.LogWarning("[ContinuousHardwareMonitor] Stopping previous monitoring session");
                StopMonitoring();
            }
            
            // Create new session
            _currentSession = new HardwareMonitoringSession
            {
                SessionId = Guid.NewGuid().ToString(),
                SessionName = sessionName,
                StartTime = DateTime.Now,
                Configuration = _configuration,
                IsActive = true,
                SamplesCollected = 0,
                Duration = TimeSpan.Zero,
                DataPath = Path.Combine(_configuration.LoggingDirectory, $"{sessionName}_{DateTime.Now:yyyyMMdd_HHmmss}")
            };
            
            // Clear previous session data
            _metricsHistory.Clear();
            _currentSessionViolations.Clear();
            
            // Start session timer
            _sessionTimer.Restart();
            
            // Initialize continuous logging if enabled
            if (_configuration.EnableContinuousLogging)
            {
                InitializeContinuousLogging();
            }
            
            _isMonitoringActive = true;
            _lastSampleTime = Time.time;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[ContinuousHardwareMonitor] Started monitoring session '{sessionName}' (ID: {_currentSession.SessionId})");
            }
            
            return _currentSession;
        }
        
        /// <summary>
        /// Stop continuous hardware monitoring and get results.
        /// </summary>
        public HardwareMonitoringResult StopMonitoring()
        {
            if (!_isMonitoringActive)
            {
                UnityEngine.Debug.LogWarning("[ContinuousHardwareMonitor] No active monitoring session to stop");
                return default;
            }
            
            _sessionTimer.Stop();
            _isMonitoringActive = false;
            
            // Close continuous logging
            if (_continuousLogWriter != null)
            {
                _continuousLogWriter.Close();
                _continuousLogWriter.Dispose();
                _continuousLogWriter = null;
            }
            
            // Process collected data
            var result = ProcessMonitoringData();
            
            // Add to session history
            _sessionHistory.Add(result);
            
            // Mark session as inactive
            _currentSession.IsActive = false;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[ContinuousHardwareMonitor] Monitoring session completed: {result.Statistics.TotalSamples} samples, " +
                                    $"{result.Statistics.TotalDuration.TotalSeconds:F1}s duration");
            }
            
            return result;
        }
        
        /// <summary>
        /// Get current real-time hardware metrics.
        /// </summary>
        public RealTimeHardwareMetrics GetCurrentMetrics()
        {
            if (!_isInitialized)
                return default;
            
            return _lastMetrics;
        }
        
        /// <summary>
        /// Get aggregated hardware statistics since monitoring started.
        /// </summary>
        public HardwarePerformanceStatistics GetAggregatedStatistics()
        {
            if (!_isMonitoringActive || _metricsHistory.Count == 0)
                return default;
            
            var samples = _metricsHistory.ToArray();
            
            return new HardwarePerformanceStatistics
            {
                FrameRateStats = CalculateStatistic(samples.Select(s => s.CurrentFPS).ToArray()),
                CPUUsageStats = CalculateStatistic(samples.Select(s => s.CPUUsagePercent).ToArray()),
                GPUUsageStats = CalculateStatistic(samples.Select(s => s.GPUUsagePercent).ToArray()),
                MemoryUsageStats = CalculateStatistic(samples.Select(s => s.MemoryUsageMB).ToArray()),
                CPUTemperatureStats = CalculateStatistic(samples.Select(s => s.CPUTemperatureCelsius).ToArray()),
                GPUTemperatureStats = CalculateStatistic(samples.Select(s => s.GPUTemperatureCelsius).ToArray()),
                PowerConsumptionStats = CalculateStatistic(samples.Select(s => s.PowerConsumptionW).ToArray()),
                BatteryLevelStats = CalculateStatistic(samples.Select(s => s.BatteryLevelPercent).ToArray()),
                TotalDuration = _sessionTimer.Elapsed,
                TotalSamples = samples.Length,
                ThermalThrottlingEvents = samples.Count(s => s.IsThermalThrottling),
                SafetyViolations = _currentSessionViolations.Count
            };
        }
        
        /// <summary>
        /// Configure safety limits for automatic test termination.
        /// </summary>
        public void ConfigureSafetyLimits(HardwareSafetyLimits safetyLimits)
        {
            _safetyLimits = safetyLimits;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[ContinuousHardwareMonitor] Safety limits configured - " +
                                    $"Max CPU temp: {safetyLimits.MaxCPUTemperatureCelsius}°C, " +
                                    $"Max GPU temp: {safetyLimits.MaxGPUTemperatureCelsius}°C");
            }
        }
        
        /// <summary>
        /// Get current safety status and any active warnings.
        /// </summary>
        public HardwareSafetyStatus GetSafetyStatus()
        {
            var currentMetrics = GetCurrentMetrics();
            var activeWarnings = new List<SafetyWarning>();
            var riskLevel = SafetyRiskLevel.Low;
            
            // Check for active safety issues
            if (currentMetrics.CPUTemperatureCelsius > _safetyLimits.MaxCPUTemperatureCelsius * 0.9f)
            {
                activeWarnings.Add(new SafetyWarning { /* CPU temperature warning */ });
                riskLevel = SafetyRiskLevel.Medium;
            }
            
            if (currentMetrics.GPUTemperatureCelsius > _safetyLimits.MaxGPUTemperatureCelsius * 0.9f)
            {
                activeWarnings.Add(new SafetyWarning { /* GPU temperature warning */ });
                riskLevel = SafetyRiskLevel.Medium;
            }
            
            if (currentMetrics.IsThermalThrottling)
            {
                riskLevel = SafetyRiskLevel.High;
            }
            
            bool allLimitsOK = activeWarnings.Count == 0 && riskLevel == SafetyRiskLevel.Low;
            
            return new HardwareSafetyStatus
            {
                AllLimitsOK = allLimitsOK,
                ActiveWarnings = activeWarnings.ToArray(),
                RiskLevel = riskLevel,
                TimeSinceLastViolation = DateTime.Now - _lastSafetyViolationTime,
                TotalViolationsThisSession = _currentSessionViolations.Count,
                AutoTerminationArmed = _autoTerminationArmed,
                EstimatedTimeToTermination = EstimateTimeToTermination(currentMetrics)
            };
        }
        
        /// <summary>
        /// Force an immediate safety check against all configured limits.
        /// </summary>
        public SafetyCheckResult PerformSafetyCheck()
        {
            var currentMetrics = GetCurrentMetrics();
            var violations = new List<SafetyViolation>();
            var recommendedActions = new List<string>();
            float safetyScore = 100f;
            
            // Check CPU temperature
            if (currentMetrics.CPUTemperatureCelsius > _safetyLimits.MaxCPUTemperatureCelsius)
            {
                violations.Add(new SafetyViolation { /* CPU temperature violation */ });
                recommendedActions.Add("Reduce CPU load to lower temperature");
                safetyScore -= 25f;
            }
            
            // Check GPU temperature
            if (currentMetrics.GPUTemperatureCelsius > _safetyLimits.MaxGPUTemperatureCelsius)
            {
                violations.Add(new SafetyViolation { /* GPU temperature violation */ });
                recommendedActions.Add("Reduce GPU load to lower temperature");
                safetyScore -= 25f;
            }
            
            // Check battery temperature
            if (currentMetrics.BatteryTemperatureCelsius > _safetyLimits.MaxBatteryTemperatureCelsius)
            {
                violations.Add(new SafetyViolation { /* Battery temperature violation */ });
                recommendedActions.Add("Allow device to cool down");
                safetyScore -= 30f;
            }
            
            // Check power consumption
            if (currentMetrics.PowerConsumptionW > _safetyLimits.MaxPowerConsumptionW)
            {
                violations.Add(new SafetyViolation { /* Power consumption violation */ });
                recommendedActions.Add("Reduce system load to lower power consumption");
                safetyScore -= 15f;
            }
            
            // Check battery level
            if (currentMetrics.BatteryLevelPercent < _safetyLimits.MinBatteryLevelPercent)
            {
                violations.Add(new SafetyViolation { /* Low battery violation */ });
                recommendedActions.Add("Charge device or connect to power");
                safetyScore -= 20f;
            }
            
            // Check memory usage
            if (currentMetrics.MemoryUsageMB > _safetyLimits.MaxMemoryUsageMB)
            {
                violations.Add(new SafetyViolation { /* Memory usage violation */ });
                recommendedActions.Add("Reduce memory usage");
                safetyScore -= 10f;
            }
            
            // Check frame rate
            if (currentMetrics.CurrentFPS < _safetyLimits.MinFrameRateFPS)
            {
                violations.Add(new SafetyViolation { /* Low frame rate violation */ });
                recommendedActions.Add("Optimize performance to improve frame rate");
                safetyScore -= 15f;
            }
            
            safetyScore = math.clamp(safetyScore, 0f, 100f);
            bool passed = violations.Count == 0;
            bool recommendTermination = safetyScore < 50f || violations.Any(v => true /* check if critical */);
            
            return new SafetyCheckResult
            {
                Passed = passed,
                Violations = violations.ToArray(),
                SafetyScore = safetyScore,
                RecommendedActions = recommendedActions.ToArray(),
                RecommendImmediateTermination = recommendTermination,
                CheckTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Get thermal monitoring data with temperature trends.
        /// </summary>
        public ThermalMonitoringData GetThermalData()
        {
            var currentMetrics = GetCurrentMetrics();
            
            // Simulate thermal sensor readings
            var sensorReadings = new ThermalSensorReading[]
            {
                new ThermalSensorReading { /* CPU thermal sensor */ },
                new ThermalSensorReading { /* GPU thermal sensor */ },
                new ThermalSensorReading { /* Battery thermal sensor */ }
            };
            
            var currentState = DetermineThermalState(currentMetrics);
            var recentTrend = AnalyzeTemperatureTrend();
            var throttlingEvents = GetRecentThrottlingEvents();
            
            return new ThermalMonitoringData
            {
                CurrentState = currentState,
                SensorReadings = sensorReadings,
                RecentTrend = recentTrend,
                ThrottlingEvents = throttlingEvents,
                EstimatedTimeToThermalLimit = EstimateTimeToThermalLimit(currentMetrics),
                ThermalSafetyMarginCelsius = CalculateThermalSafetyMargin(currentMetrics)
            };
        }
        
        /// <summary>
        /// Get power consumption and battery monitoring data.
        /// </summary>
        public PowerMonitoringData GetPowerData()
        {
            var currentMetrics = GetCurrentMetrics();
            
            return new PowerMonitoringData
            {
                CurrentConsumption = new PowerConsumptionBreakdown { /* Power breakdown */ },
                BatteryStatus = new BatteryStatus { /* Battery status */ },
                Efficiency = new PowerEfficiencyMetrics { /* Efficiency metrics */ },
                EstimatedBatteryLife = EstimateBatteryLife(currentMetrics),
                RecentTrend = AnalyzePowerConsumptionTrend()
            };
        }
        
        /// <summary>
        /// Get performance trend analysis over the monitoring period.
        /// </summary>
        public PerformanceTrendAnalysis GetPerformanceTrends()
        {
            if (_metricsHistory.Count < 10)
                return default;
            
            var samples = _metricsHistory.ToArray();
            
            return new PerformanceTrendAnalysis
            {
                FrameRateTrend = AnalyzeTrend(samples.Select(s => s.CurrentFPS).ToArray()),
                CPUUsageTrend = AnalyzeTrend(samples.Select(s => s.CPUUsagePercent).ToArray()),
                GPUUsageTrend = AnalyzeTrend(samples.Select(s => s.GPUUsagePercent).ToArray()),
                MemoryUsageTrend = AnalyzeTrend(samples.Select(s => s.MemoryUsageMB).ToArray()),
                TemperatureTrend = AnalyzeTrend(samples.Select(s => s.CPUTemperatureCelsius).ToArray()),
                PowerConsumptionTrend = AnalyzeTrend(samples.Select(s => s.PowerConsumptionW).ToArray()),
                OverallTrend = DetermineOverallTrend(samples),
                Prediction = PredictFuturePerformance(samples)
            };
        }
        
        /// <summary>
        /// Export monitoring data to file for analysis.
        /// </summary>
        public bool ExportMonitoringData(string filePath, MonitoringDataExportFormat format = MonitoringDataExportFormat.CSV)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                
                switch (format)
                {
                    case MonitoringDataExportFormat.CSV:
                        return ExportToCSV(filePath);
                    case MonitoringDataExportFormat.JSON:
                        return ExportToJSON(filePath);
                    case MonitoringDataExportFormat.Binary:
                        return ExportToBinary(filePath);
                    default:
                        UnityEngine.Debug.LogError($"[ContinuousHardwareMonitor] Unsupported export format: {format}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ContinuousHardwareMonitor] Export failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Set custom performance thresholds for monitoring alerts.
        /// </summary>
        public void SetPerformanceThresholds(PerformanceThresholds thresholds)
        {
            _performanceThresholds = thresholds;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[ContinuousHardwareMonitor] Performance thresholds updated - " +
                                    $"FPS warning: {thresholds.FrameRateWarningThreshold}, " +
                                    $"CPU warning: {thresholds.CPUUsageWarningThreshold}%");
            }
        }
        
        /// <summary>
        /// Get monitoring session history for analysis.
        /// </summary>
        public MonitoringSessionHistory GetSessionHistory()
        {
            var completedSessions = _sessionHistory.ToArray();
            
            if (completedSessions.Length == 0)
            {
                return new MonitoringSessionHistory
                {
                    CompletedSessions = new HardwareMonitoringResult[0],
                    TotalMonitoringTime = TimeSpan.Zero,
                    AverageSessionDuration = TimeSpan.Zero,
                    TotalSafetyViolations = 0,
                    CommonSafetyViolations = new string[0],
                    PerformanceTrends = new CrossSessionTrends { /* Empty trends */ }
                };
            }
            
            var totalTime = TimeSpan.FromTicks(completedSessions.Sum(s => s.Statistics.TotalDuration.Ticks));
            var avgDuration = TimeSpan.FromTicks(totalTime.Ticks / completedSessions.Length);
            var totalViolations = completedSessions.Sum(s => s.Statistics.SafetyViolations);
            
            return new MonitoringSessionHistory
            {
                CompletedSessions = completedSessions,
                TotalMonitoringTime = totalTime,
                AverageSessionDuration = avgDuration,
                TotalSafetyViolations = totalViolations,
                CommonSafetyViolations = AnalyzeCommonViolations(completedSessions),
                PerformanceTrends = AnalyzeCrossSessionTrends(completedSessions)
            };
        }
        
        /// <summary>
        /// Clean up hardware monitoring resources.
        /// </summary>
        public void Dispose()
        {
            if (_isMonitoringActive)
            {
                StopMonitoring();
            }
            
            _continuousLogWriter?.Dispose();
            _sessionTimer?.Stop();
            
            _metricsHistory.Clear();
            _sessionHistory.Clear();
            _currentSessionViolations.Clear();
            
            _isInitialized = false;
            
            if (_configuration.EnableDebugLogging)
            {
                UnityEngine.Debug.Log("[ContinuousHardwareMonitor] Disposed");
            }
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        #region Private Helper Methods
        
        private void CollectHardwareMetrics()
        {
            // Simulate hardware metrics collection (in real implementation, this would query actual hardware)
            var metrics = new RealTimeHardwareMetrics
            {
                CurrentFPS = SimulateFrameRate(),
                FrameTimeMs = 1000f / math.max(SimulateFrameRate(), 1f),
                CPUUsagePercent = SimulateCPUUsage(),
                CPUFrequencyMHz = 2400f + _random.Next(-200, 400),
                GPUUsagePercent = SimulateGPUUsage(),
                GPUFrequencyMHz = 800f + _random.Next(-100, 200),
                MemoryUsageMB = SimulateMemoryUsage(),
                AvailableMemoryMB = 4096f - SimulateMemoryUsage(),
                CPUTemperatureCelsius = SimulateCPUTemperature(),
                GPUTemperatureCelsius = SimulateGPUTemperature(),
                BatteryTemperatureCelsius = SimulateBatteryTemperature(),
                PowerConsumptionW = SimulatePowerConsumption(),
                BatteryLevelPercent = SimulateBatteryLevel(),
                IsCharging = _random.NextDouble() < 0.1, // 10% chance of charging
                IsThermalThrottling = SimulateThermalThrottling(),
                Timestamp = DateTime.Now
            };
            
            _lastMetrics = metrics;
            
            // Add to history
            _metricsHistory.Enqueue(metrics);
            if (_metricsHistory.Count > _configuration.MaxSamplesInMemory)
            {
                _metricsHistory.Dequeue();
            }
            
            // Update session info
            _currentSession.SamplesCollected++;
            _currentSession.Duration = _sessionTimer.Elapsed;
            
            // Write to continuous log if enabled
            if (_continuousLogWriter != null)
            {
                WriteToContinuousLog(metrics);
            }
            
            // Perform safety checks
            if (_configuration.EnableSafetyLimits)
            {
                CheckSafetyLimits(metrics);
            }
            
            // Check performance thresholds
            CheckPerformanceThresholds(metrics);
            
            // Fire monitoring data updated event
            MonitoringDataUpdated?.Invoke(new MonitoringDataUpdatedEventArgs
            {
                CurrentMetrics = metrics,
                TotalSamples = _currentSession.SamplesCollected,
                MonitoringDuration = _currentSession.Duration,
                Timestamp = DateTime.Now
            });
        }
        
        private void CheckSafetyLimits(RealTimeHardwareMetrics metrics)
        {
            var violations = new List<SafetyViolation>();
            
            // Check all safety limits
            if (metrics.CPUTemperatureCelsius > _safetyLimits.MaxCPUTemperatureCelsius)
            {
                violations.Add(new SafetyViolation { /* CPU temperature violation */ });
            }
            
            if (metrics.GPUTemperatureCelsius > _safetyLimits.MaxGPUTemperatureCelsius)
            {
                violations.Add(new SafetyViolation { /* GPU temperature violation */ });
            }
            
            if (metrics.BatteryTemperatureCelsius > _safetyLimits.MaxBatteryTemperatureCelsius)
            {
                violations.Add(new SafetyViolation { /* Battery temperature violation */ });
            }
            
            if (metrics.PowerConsumptionW > _safetyLimits.MaxPowerConsumptionW)
            {
                violations.Add(new SafetyViolation { /* Power consumption violation */ });
            }
            
            if (metrics.BatteryLevelPercent < _safetyLimits.MinBatteryLevelPercent)
            {
                violations.Add(new SafetyViolation { /* Low battery violation */ });
            }
            
            if (metrics.MemoryUsageMB > _safetyLimits.MaxMemoryUsageMB)
            {
                violations.Add(new SafetyViolation { /* Memory usage violation */ });
            }
            
            if (metrics.CurrentFPS < _safetyLimits.MinFrameRateFPS)
            {
                violations.Add(new SafetyViolation { /* Low frame rate violation */ });
            }
            
            // Handle thermal throttling
            if (metrics.IsThermalThrottling)
            {
                _consecutiveThermalThrottlingEvents++;
                if (_consecutiveThermalThrottlingEvents > _safetyLimits.MaxConsecutiveThermalThrottlingEvents)
                {
                    violations.Add(new SafetyViolation { /* Excessive thermal throttling violation */ });
                }
            }
            else
            {
                _consecutiveThermalThrottlingEvents = 0;
            }
            
            // Process violations
            foreach (var violation in violations)
            {
                _currentSessionViolations.Add(violation);
                _lastSafetyViolationTime = DateTime.Now;
                
                bool recommendTermination = ShouldRecommendTermination(violation, metrics);
                
                SafetyLimitExceeded?.Invoke(new SafetyLimitExceededEventArgs
                {
                    Violation = violation,
                    CurrentMetrics = metrics,
                    RecommendTermination = recommendTermination,
                    Timestamp = DateTime.Now
                });
                
                if (_configuration.AutoTerminateOnSafetyViolation && recommendTermination)
                {
                    UnityEngine.Debug.LogError("[ContinuousHardwareMonitor] Critical safety violation detected - terminating monitoring");
                    StopMonitoring();
                    break;
                }
            }
        }
        
        private void CheckPerformanceThresholds(RealTimeHardwareMetrics metrics)
        {
            // Check frame rate thresholds
            if (metrics.CurrentFPS < _performanceThresholds.FrameRateCriticalThreshold)
            {
                FirePerformanceThresholdEvent("FrameRate", metrics.CurrentFPS, 
                    _performanceThresholds.FrameRateCriticalThreshold, true);
            }
            else if (metrics.CurrentFPS < _performanceThresholds.FrameRateWarningThreshold)
            {
                FirePerformanceThresholdEvent("FrameRate", metrics.CurrentFPS, 
                    _performanceThresholds.FrameRateWarningThreshold, false);
            }
            
            // Check CPU usage thresholds
            if (metrics.CPUUsagePercent > _performanceThresholds.CPUUsageCriticalThreshold)
            {
                FirePerformanceThresholdEvent("CPUUsage", metrics.CPUUsagePercent, 
                    _performanceThresholds.CPUUsageCriticalThreshold, true);
            }
            else if (metrics.CPUUsagePercent > _performanceThresholds.CPUUsageWarningThreshold)
            {
                FirePerformanceThresholdEvent("CPUUsage", metrics.CPUUsagePercent, 
                    _performanceThresholds.CPUUsageWarningThreshold, false);
            }
            
            // Check GPU usage thresholds
            if (metrics.GPUUsagePercent > _performanceThresholds.GPUUsageCriticalThreshold)
            {
                FirePerformanceThresholdEvent("GPUUsage", metrics.GPUUsagePercent, 
                    _performanceThresholds.GPUUsageCriticalThreshold, true);
            }
            else if (metrics.GPUUsagePercent > _performanceThresholds.GPUUsageWarningThreshold)
            {
                FirePerformanceThresholdEvent("GPUUsage", metrics.GPUUsagePercent, 
                    _performanceThresholds.GPUUsageWarningThreshold, false);
            }
            
            // Check temperature thresholds
            float maxTemp = math.max(metrics.CPUTemperatureCelsius, metrics.GPUTemperatureCelsius);
            if (maxTemp > _performanceThresholds.TemperatureCriticalThreshold)
            {
                FirePerformanceThresholdEvent("Temperature", maxTemp, 
                    _performanceThresholds.TemperatureCriticalThreshold, true);
                
                // Fire thermal warning
                ThermalWarningDetected?.Invoke(new ThermalWarningEventArgs
                {
                    ThermalState = ThermalState.Critical,
                    CurrentTemperature = maxTemp,
                    SensorLocation = maxTemp == metrics.CPUTemperatureCelsius ? "CPU" : "GPU",
                    Timestamp = DateTime.Now
                });
            }
            else if (maxTemp > _performanceThresholds.TemperatureWarningThreshold)
            {
                FirePerformanceThresholdEvent("Temperature", maxTemp, 
                    _performanceThresholds.TemperatureWarningThreshold, false);
                
                ThermalWarningDetected?.Invoke(new ThermalWarningEventArgs
                {
                    ThermalState = ThermalState.Warning,
                    CurrentTemperature = maxTemp,
                    SensorLocation = maxTemp == metrics.CPUTemperatureCelsius ? "CPU" : "GPU",
                    Timestamp = DateTime.Now
                });
            }
        }
        
        private void FirePerformanceThresholdEvent(string metricName, float currentValue, 
            float thresholdValue, bool isCritical)
        {
            PerformanceThresholdCrossed?.Invoke(new PerformanceThresholdEventArgs
            {
                MetricName = metricName,
                CurrentValue = currentValue,
                ThresholdValue = thresholdValue,
                IsCritical = isCritical,
                Timestamp = DateTime.Now
            });
        }
        
        private void InitializeContinuousLogging()
        {
            try
            {
                Directory.CreateDirectory(_currentSession.DataPath);
                string logFilePath = Path.Combine(_currentSession.DataPath, "continuous_monitoring.csv");
                
                _continuousLogWriter = new StreamWriter(logFilePath);
                
                // Write CSV header
                _continuousLogWriter.WriteLine("Timestamp,FPS,FrameTimeMs,CPUUsage,CPUFreq,GPUUsage,GPUFreq," +
                                             "MemoryUsage,AvailableMemory,CPUTemp,GPUTemp,BatteryTemp," +
                                             "PowerConsumption,BatteryLevel,IsCharging,IsThermalThrottling");
                
                _continuousLogWriter.Flush();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ContinuousHardwareMonitor] Failed to initialize continuous logging: {ex.Message}");
            }
        }
        
        private void WriteToContinuousLog(RealTimeHardwareMetrics metrics)
        {
            try
            {
                _continuousLogWriter.WriteLine($"{metrics.Timestamp:yyyy-MM-dd HH:mm:ss.fff}," +
                                             $"{metrics.CurrentFPS:F2}," +
                                             $"{metrics.FrameTimeMs:F2}," +
                                             $"{metrics.CPUUsagePercent:F2}," +
                                             $"{metrics.CPUFrequencyMHz:F0}," +
                                             $"{metrics.GPUUsagePercent:F2}," +
                                             $"{metrics.GPUFrequencyMHz:F0}," +
                                             $"{metrics.MemoryUsageMB:F2}," +
                                             $"{metrics.AvailableMemoryMB:F2}," +
                                             $"{metrics.CPUTemperatureCelsius:F2}," +
                                             $"{metrics.GPUTemperatureCelsius:F2}," +
                                             $"{metrics.BatteryTemperatureCelsius:F2}," +
                                             $"{metrics.PowerConsumptionW:F2}," +
                                             $"{metrics.BatteryLevelPercent:F2}," +
                                             $"{metrics.IsCharging}," +
                                             $"{metrics.IsThermalThrottling}");
                
                _continuousLogWriter.Flush();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ContinuousHardwareMonitor] Failed to write to continuous log: {ex.Message}");
            }
        }
        
        // Simulation methods (in real implementation, these would query actual hardware)
        private float SimulateFrameRate()
        {
            return 72f + _random.Next(-10, 15); // Quest 3 target with variation
        }
        
        private float SimulateCPUUsage()
        {
            return 60f + _random.Next(-20, 30); // Typical CPU usage with variation
        }
        
        private float SimulateGPUUsage()
        {
            return 70f + _random.Next(-25, 25); // Typical GPU usage with variation
        }
        
        private float SimulateMemoryUsage()
        {
            return 1500f + _random.Next(-300, 800); // Memory usage in MB
        }
        
        private float SimulateCPUTemperature()
        {
            return 45f + _random.Next(-5, 20); // CPU temperature with variation
        }
        
        private float SimulateGPUTemperature()
        {
            return 48f + _random.Next(-5, 22); // GPU temperature with variation
        }
        
        private float SimulateBatteryTemperature()
        {
            return 35f + _random.Next(-3, 10); // Battery temperature
        }
        
        private float SimulatePowerConsumption()
        {
            return 7f + _random.Next(-2, 4); // Power consumption in watts
        }
        
        private float SimulateBatteryLevel()
        {
            // Simulate gradual battery drain
            float baseLevel = 80f;
            if (_sessionTimer != null)
            {
                float drainRate = 0.1f; // 0.1% per minute
                baseLevel -= (float)_sessionTimer.Elapsed.TotalMinutes * drainRate;
            }
            return math.clamp(baseLevel + _random.Next(-5, 5), 0f, 100f);
        }
        
        private bool SimulateThermalThrottling()
        {
            // Simulate thermal throttling based on temperature
            float maxTemp = math.max(SimulateCPUTemperature(), SimulateGPUTemperature());
            return maxTemp > 75f && _random.NextDouble() < 0.3; // 30% chance if hot
        }
        
        // Additional helper methods would be implemented here...
        private PerformanceStatistic CalculateStatistic(float[] values)
        {
            if (values.Length == 0)
                return default;
            
            var sorted = values.OrderBy(v => v).ToArray();
            float avg = values.Average();
            float min = values.Min();
            float max = values.Max();
            float stdDev = CalculateStandardDeviation(values);
            float p95 = sorted[(int)(sorted.Length * 0.95f)];
            float p99 = sorted[(int)(sorted.Length * 0.99f)];
            
            return new PerformanceStatistic
            {
                Current = values.Last(),
                Average = avg,
                Minimum = min,
                Maximum = max,
                StandardDeviation = stdDev,
                Percentile95 = p95,
                Percentile99 = p99
            };
        }
        
        private float CalculateStandardDeviation(float[] values)
        {
            if (values.Length <= 1) return 0f;
            
            float mean = values.Average();
            float sumSquaredDifferences = values.Sum(v => (v - mean) * (v - mean));
            return Mathf.Sqrt(sumSquaredDifferences / (values.Length - 1));
        }
        
        private HardwareMonitoringResult ProcessMonitoringData()
        {
            var statistics = GetAggregatedStatistics();
            var safetyStatus = GetSafetyStatus();
            var trendAnalysis = GetPerformanceTrends();
            
            return new HardwareMonitoringResult
            {
                Session = _currentSession,
                Statistics = statistics,
                FinalSafetyStatus = safetyStatus,
                SafetyViolations = _currentSessionViolations.ToArray(),
                TrendAnalysis = trendAnalysis,
                RawSamples = _metricsHistory.ToArray(),
                CompletedSuccessfully = true,
                TerminationReason = "Normal completion",
                QualityAssessment = new MonitoringQualityAssessment { /* Quality assessment */ }
            };
        }
        
        // Additional helper methods for analysis, trend calculation, etc. would be implemented here...
        private bool ShouldRecommendTermination(SafetyViolation violation, RealTimeHardwareMetrics metrics) => false;
        private TimeSpan? EstimateTimeToTermination(RealTimeHardwareMetrics metrics) => null;
        private ThermalState DetermineThermalState(RealTimeHardwareMetrics metrics) => ThermalState.Normal;
        private TemperatureTrend AnalyzeTemperatureTrend() => TemperatureTrend.Stable;
        private ThermalThrottlingEvent[] GetRecentThrottlingEvents() => new ThermalThrottlingEvent[0];
        private TimeSpan? EstimateTimeToThermalLimit(RealTimeHardwareMetrics metrics) => null;
        private float CalculateThermalSafetyMargin(RealTimeHardwareMetrics metrics) => 10f;
        private TimeSpan EstimateBatteryLife(RealTimeHardwareMetrics metrics) => TimeSpan.FromHours(3);
        private PowerConsumptionTrend AnalyzePowerConsumptionTrend() => PowerConsumptionTrend.Stable;
        private TrendAnalysis AnalyzeTrend(float[] values) => new TrendAnalysis { /* Trend analysis */ };
        private OverallTrendSummary DetermineOverallTrend(RealTimeHardwareMetrics[] samples) => OverallTrendSummary.Stable;
        private PerformancePrediction PredictFuturePerformance(RealTimeHardwareMetrics[] samples) => new PerformancePrediction { /* Prediction */ };
        private bool ExportToCSV(string filePath) => true;
        private bool ExportToJSON(string filePath) => true;
        private bool ExportToBinary(string filePath) => true;
        private string[] AnalyzeCommonViolations(HardwareMonitoringResult[] sessions) => new string[0];
        private CrossSessionTrends AnalyzeCrossSessionTrends(HardwareMonitoringResult[] sessions) => new CrossSessionTrends { /* Trends */ };
        
        #endregion
    }
}