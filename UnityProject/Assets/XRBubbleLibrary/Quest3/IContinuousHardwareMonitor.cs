using System;
using UnityEngine;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Interface for continuous hardware monitoring during Quest 3 testing.
    /// Provides real-time monitoring of FPS, CPU, GPU, thermal, and safety limits.
    /// Implements Requirement 8.2: Real-time FPS, CPU, GPU, and thermal monitoring.
    /// Implements Requirement 8.3: Performance data collection and analysis.
    /// Implements Requirement 8.4: Automatic test termination for safety limits.
    /// Implements Requirement 8.5: Continuous hardware monitoring during testing.
    /// </summary>
    public interface IContinuousHardwareMonitor
    {
        /// <summary>
        /// Whether the hardware monitor is initialized and active.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Whether continuous monitoring is currently active.
        /// </summary>
        bool IsMonitoringActive { get; }
        
        /// <summary>
        /// Current monitoring configuration.
        /// </summary>
        HardwareMonitoringConfiguration Configuration { get; }
        
        /// <summary>
        /// Initialize the continuous hardware monitor with the specified configuration.
        /// </summary>
        /// <param name="config">Hardware monitoring configuration</param>
        /// <returns>True if initialization succeeded</returns>
        bool Initialize(HardwareMonitoringConfiguration config);
        
        /// <summary>
        /// Start continuous hardware monitoring.
        /// </summary>
        /// <param name="sessionName">Name for the monitoring session</param>
        /// <returns>Monitoring session handle</returns>
        HardwareMonitoringSession StartMonitoring(string sessionName = "Quest3_Hardware_Monitoring");
        
        /// <summary>
        /// Stop continuous hardware monitoring and get results.
        /// </summary>
        /// <returns>Complete monitoring results</returns>
        HardwareMonitoringResult StopMonitoring();
        
        /// <summary>
        /// Get current real-time hardware metrics.
        /// </summary>
        /// <returns>Current hardware performance data</returns>
        RealTimeHardwareMetrics GetCurrentMetrics();
        
        /// <summary>
        /// Get aggregated hardware statistics since monitoring started.
        /// </summary>
        /// <returns>Aggregated hardware performance statistics</returns>
        HardwarePerformanceStatistics GetAggregatedStatistics();
        
        /// <summary>
        /// Configure safety limits for automatic test termination.
        /// </summary>
        /// <param name="safetyLimits">Safety limit configuration</param>
        void ConfigureSafetyLimits(HardwareSafetyLimits safetyLimits);
        
        /// <summary>
        /// Get current safety status and any active warnings.
        /// </summary>
        /// <returns>Current safety status information</returns>
        HardwareSafetyStatus GetSafetyStatus();
        
        /// <summary>
        /// Force an immediate safety check against all configured limits.
        /// </summary>
        /// <returns>Safety check result with any violations</returns>
        SafetyCheckResult PerformSafetyCheck();
        
        /// <summary>
        /// Get thermal monitoring data with temperature trends.
        /// </summary>
        /// <returns>Detailed thermal monitoring information</returns>
        ThermalMonitoringData GetThermalData();
        
        /// <summary>
        /// Get power consumption and battery monitoring data.
        /// </summary>
        /// <returns>Power and battery monitoring information</returns>
        PowerMonitoringData GetPowerData();
        
        /// <summary>
        /// Get performance trend analysis over the monitoring period.
        /// </summary>
        /// <returns>Performance trend analysis results</returns>
        PerformanceTrendAnalysis GetPerformanceTrends();
        
        /// <summary>
        /// Export monitoring data to file for analysis.
        /// </summary>
        /// <param name="filePath">Path to export the monitoring data</param>
        /// <param name="format">Export format</param>
        /// <returns>True if export succeeded</returns>
        bool ExportMonitoringData(string filePath, MonitoringDataExportFormat format = MonitoringDataExportFormat.CSV);
        
        /// <summary>
        /// Set custom performance thresholds for monitoring alerts.
        /// </summary>
        /// <param name="thresholds">Custom performance thresholds</param>
        void SetPerformanceThresholds(PerformanceThresholds thresholds);
        
        /// <summary>
        /// Get monitoring session history for analysis.
        /// </summary>
        /// <returns>Historical monitoring session data</returns>
        MonitoringSessionHistory GetSessionHistory();
        
        /// <summary>
        /// Event fired when safety limits are exceeded.
        /// </summary>
        event Action<SafetyLimitExceededEventArgs> SafetyLimitExceeded;
        
        /// <summary>
        /// Event fired when performance thresholds are crossed.
        /// </summary>
        event Action<PerformanceThresholdEventArgs> PerformanceThresholdCrossed;
        
        /// <summary>
        /// Event fired when thermal warnings are detected.
        /// </summary>
        event Action<ThermalWarningEventArgs> ThermalWarningDetected;
        
        /// <summary>
        /// Event fired when monitoring data is updated.
        /// </summary>
        event Action<MonitoringDataUpdatedEventArgs> MonitoringDataUpdated;
        
        /// <summary>
        /// Clean up hardware monitoring resources.
        /// </summary>
        void Dispose();
    }
    
    /// <summary>
    /// Configuration for continuous hardware monitoring.
    /// </summary>
    [Serializable]
    public struct HardwareMonitoringConfiguration
    {
        /// <summary>
        /// Monitoring sample rate in Hz.
        /// </summary>
        public float SampleRateHz;
        
        /// <summary>
        /// Whether to monitor FPS and frame timing.
        /// </summary>
        public bool MonitorFrameRate;
        
        /// <summary>
        /// Whether to monitor CPU usage and frequency.
        /// </summary>
        public bool MonitorCPU;
        
        /// <summary>
        /// Whether to monitor GPU usage and frequency.
        /// </summary>
        public bool MonitorGPU;
        
        /// <summary>
        /// Whether to monitor memory usage.
        /// </summary>
        public bool MonitorMemory;
        
        /// <summary>
        /// Whether to monitor thermal sensors.
        /// </summary>
        public bool MonitorThermal;
        
        /// <summary>
        /// Whether to monitor power consumption and battery.
        /// </summary>
        public bool MonitorPower;
        
        /// <summary>
        /// Whether to enable safety limit checking.
        /// </summary>
        public bool EnableSafetyLimits;
        
        /// <summary>
        /// Whether to automatically terminate tests on safety violations.
        /// </summary>
        public bool AutoTerminateOnSafetyViolation;
        
        /// <summary>
        /// Maximum monitoring duration in seconds (0 = unlimited).
        /// </summary>
        public float MaxMonitoringDurationSeconds;
        
        /// <summary>
        /// Maximum number of samples to store in memory.
        /// </summary>
        public int MaxSamplesInMemory;
        
        /// <summary>
        /// Whether to log monitoring data to file continuously.
        /// </summary>
        public bool EnableContinuousLogging;
        
        /// <summary>
        /// Directory for continuous logging output.
        /// </summary>
        public string LoggingDirectory;
        
        /// <summary>
        /// Whether to enable debug logging.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Default configuration for Quest 3 hardware monitoring.
        /// </summary>
        public static HardwareMonitoringConfiguration Quest3Default => new HardwareMonitoringConfiguration
        {
            SampleRateHz = 10f,
            MonitorFrameRate = true,
            MonitorCPU = true,
            MonitorGPU = true,
            MonitorMemory = true,
            MonitorThermal = true,
            MonitorPower = true,
            EnableSafetyLimits = true,
            AutoTerminateOnSafetyViolation = true,
            MaxMonitoringDurationSeconds = 3600f, // 1 hour max
            MaxSamplesInMemory = 36000, // 1 hour at 10Hz
            EnableContinuousLogging = true,
            LoggingDirectory = "HardwareMonitoring/",
            EnableDebugLogging = false
        };
        
        /// <summary>
        /// High-frequency monitoring configuration for detailed analysis.
        /// </summary>
        public static HardwareMonitoringConfiguration HighFrequency => new HardwareMonitoringConfiguration
        {
            SampleRateHz = 60f,
            MonitorFrameRate = true,
            MonitorCPU = true,
            MonitorGPU = true,
            MonitorMemory = true,
            MonitorThermal = true,
            MonitorPower = true,
            EnableSafetyLimits = true,
            AutoTerminateOnSafetyViolation = true,
            MaxMonitoringDurationSeconds = 600f, // 10 minutes max at high frequency
            MaxSamplesInMemory = 36000, // 10 minutes at 60Hz
            EnableContinuousLogging = true,
            LoggingDirectory = "HardwareMonitoring/HighFreq/",
            EnableDebugLogging = true
        };
        
        /// <summary>
        /// Performance testing configuration with safety focus.
        /// </summary>
        public static HardwareMonitoringConfiguration PerformanceTesting => new HardwareMonitoringConfiguration
        {
            SampleRateHz = 30f,
            MonitorFrameRate = true,
            MonitorCPU = true,
            MonitorGPU = true,
            MonitorMemory = true,
            MonitorThermal = true,
            MonitorPower = true,
            EnableSafetyLimits = true,
            AutoTerminateOnSafetyViolation = true,
            MaxMonitoringDurationSeconds = 1800f, // 30 minutes
            MaxSamplesInMemory = 54000, // 30 minutes at 30Hz
            EnableContinuousLogging = true,
            LoggingDirectory = "HardwareMonitoring/PerfTest/",
            EnableDebugLogging = false
        };
    }
    
    /// <summary>
    /// Hardware monitoring session information.
    /// </summary>
    public struct HardwareMonitoringSession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// User-provided session name.
        /// </summary>
        public string SessionName;
        
        /// <summary>
        /// Session start timestamp.
        /// </summary>
        public DateTime StartTime;
        
        /// <summary>
        /// Configuration used for this session.
        /// </summary>
        public HardwareMonitoringConfiguration Configuration;
        
        /// <summary>
        /// Whether the session is currently active.
        /// </summary>
        public bool IsActive;
        
        /// <summary>
        /// Number of samples collected so far.
        /// </summary>
        public int SamplesCollected;
        
        /// <summary>
        /// Session duration so far.
        /// </summary>
        public TimeSpan Duration;
        
        /// <summary>
        /// Data output path for this session.
        /// </summary>
        public string DataPath;
    }
    
    /// <summary>
    /// Real-time hardware metrics snapshot.
    /// </summary>
    public struct RealTimeHardwareMetrics
    {
        /// <summary>
        /// Current frame rate in FPS.
        /// </summary>
        public float CurrentFPS;
        
        /// <summary>
        /// Current frame time in milliseconds.
        /// </summary>
        public float FrameTimeMs;
        
        /// <summary>
        /// Current CPU usage percentage (0-100).
        /// </summary>
        public float CPUUsagePercent;
        
        /// <summary>
        /// Current CPU frequency in MHz.
        /// </summary>
        public float CPUFrequencyMHz;
        
        /// <summary>
        /// Current GPU usage percentage (0-100).
        /// </summary>
        public float GPUUsagePercent;
        
        /// <summary>
        /// Current GPU frequency in MHz.
        /// </summary>
        public float GPUFrequencyMHz;
        
        /// <summary>
        /// Current memory usage in MB.
        /// </summary>
        public float MemoryUsageMB;
        
        /// <summary>
        /// Available memory in MB.
        /// </summary>
        public float AvailableMemoryMB;
        
        /// <summary>
        /// Current CPU temperature in Celsius.
        /// </summary>
        public float CPUTemperatureCelsius;
        
        /// <summary>
        /// Current GPU temperature in Celsius.
        /// </summary>
        public float GPUTemperatureCelsius;
        
        /// <summary>
        /// Current battery temperature in Celsius.
        /// </summary>
        public float BatteryTemperatureCelsius;
        
        /// <summary>
        /// Current power consumption in watts.
        /// </summary>
        public float PowerConsumptionW;
        
        /// <summary>
        /// Current battery level percentage (0-100).
        /// </summary>
        public float BatteryLevelPercent;
        
        /// <summary>
        /// Whether the device is currently charging.
        /// </summary>
        public bool IsCharging;
        
        /// <summary>
        /// Whether thermal throttling is active.
        /// </summary>
        public bool IsThermalThrottling;
        
        /// <summary>
        /// Timestamp of this metrics snapshot.
        /// </summary>
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Aggregated hardware performance statistics.
    /// </summary>
    public struct HardwarePerformanceStatistics
    {
        /// <summary>
        /// Frame rate statistics.
        /// </summary>
        public PerformanceStatistic FrameRateStats;
        
        /// <summary>
        /// CPU usage statistics.
        /// </summary>
        public PerformanceStatistic CPUUsageStats;
        
        /// <summary>
        /// GPU usage statistics.
        /// </summary>
        public PerformanceStatistic GPUUsageStats;
        
        /// <summary>
        /// Memory usage statistics.
        /// </summary>
        public PerformanceStatistic MemoryUsageStats;
        
        /// <summary>
        /// CPU temperature statistics.
        /// </summary>
        public PerformanceStatistic CPUTemperatureStats;
        
        /// <summary>
        /// GPU temperature statistics.
        /// </summary>
        public PerformanceStatistic GPUTemperatureStats;
        
        /// <summary>
        /// Power consumption statistics.
        /// </summary>
        public PerformanceStatistic PowerConsumptionStats;
        
        /// <summary>
        /// Battery level statistics.
        /// </summary>
        public PerformanceStatistic BatteryLevelStats;
        
        /// <summary>
        /// Total monitoring duration.
        /// </summary>
        public TimeSpan TotalDuration;
        
        /// <summary>
        /// Total number of samples collected.
        /// </summary>
        public int TotalSamples;
        
        /// <summary>
        /// Number of thermal throttling events detected.
        /// </summary>
        public int ThermalThrottlingEvents;
        
        /// <summary>
        /// Number of safety limit violations.
        /// </summary>
        public int SafetyViolations;
    }
    
    /// <summary>
    /// Statistical data for a performance metric.
    /// </summary>
    public struct PerformanceStatistic
    {
        /// <summary>
        /// Current value.
        /// </summary>
        public float Current;
        
        /// <summary>
        /// Average value over monitoring period.
        /// </summary>
        public float Average;
        
        /// <summary>
        /// Minimum value recorded.
        /// </summary>
        public float Minimum;
        
        /// <summary>
        /// Maximum value recorded.
        /// </summary>
        public float Maximum;
        
        /// <summary>
        /// Standard deviation of values.
        /// </summary>
        public float StandardDeviation;
        
        /// <summary>
        /// 95th percentile value.
        /// </summary>
        public float Percentile95;
        
        /// <summary>
        /// 99th percentile value.
        /// </summary>
        public float Percentile99;
    }
    
    /// <summary>
    /// Safety limits configuration for automatic test termination.
    /// </summary>
    [Serializable]
    public struct HardwareSafetyLimits
    {
        /// <summary>
        /// Maximum allowed CPU temperature in Celsius.
        /// </summary>
        public float MaxCPUTemperatureCelsius;
        
        /// <summary>
        /// Maximum allowed GPU temperature in Celsius.
        /// </summary>
        public float MaxGPUTemperatureCelsius;
        
        /// <summary>
        /// Maximum allowed battery temperature in Celsius.
        /// </summary>
        public float MaxBatteryTemperatureCelsius;
        
        /// <summary>
        /// Maximum allowed power consumption in watts.
        /// </summary>
        public float MaxPowerConsumptionW;
        
        /// <summary>
        /// Minimum allowed battery level percentage.
        /// </summary>
        public float MinBatteryLevelPercent;
        
        /// <summary>
        /// Maximum allowed memory usage in MB.
        /// </summary>
        public float MaxMemoryUsageMB;
        
        /// <summary>
        /// Minimum allowed frame rate in FPS.
        /// </summary>
        public float MinFrameRateFPS;
        
        /// <summary>
        /// Maximum allowed consecutive thermal throttling events.
        /// </summary>
        public int MaxConsecutiveThermalThrottlingEvents;
        
        /// <summary>
        /// Time window for safety limit violations before termination (seconds).
        /// </summary>
        public float ViolationTimeWindowSeconds;
        
        /// <summary>
        /// Default safety limits for Quest 3.
        /// </summary>
        public static HardwareSafetyLimits Quest3Default => new HardwareSafetyLimits
        {
            MaxCPUTemperatureCelsius = 85f,
            MaxGPUTemperatureCelsius = 85f,
            MaxBatteryTemperatureCelsius = 45f,
            MaxPowerConsumptionW = 12f,
            MinBatteryLevelPercent = 10f,
            MaxMemoryUsageMB = 3000f,
            MinFrameRateFPS = 30f,
            MaxConsecutiveThermalThrottlingEvents = 5,
            ViolationTimeWindowSeconds = 10f
        };
        
        /// <summary>
        /// Conservative safety limits for extended testing.
        /// </summary>
        public static HardwareSafetyLimits Conservative => new HardwareSafetyLimits
        {
            MaxCPUTemperatureCelsius = 75f,
            MaxGPUTemperatureCelsius = 75f,
            MaxBatteryTemperatureCelsius = 40f,
            MaxPowerConsumptionW = 10f,
            MinBatteryLevelPercent = 20f,
            MaxMemoryUsageMB = 2500f,
            MinFrameRateFPS = 45f,
            MaxConsecutiveThermalThrottlingEvents = 3,
            ViolationTimeWindowSeconds = 5f
        };
    }
    
    /// <summary>
    /// Current safety status information.
    /// </summary>
    public struct HardwareSafetyStatus
    {
        /// <summary>
        /// Whether all safety limits are currently within acceptable ranges.
        /// </summary>
        public bool AllLimitsOK;
        
        /// <summary>
        /// Active safety warnings.
        /// </summary>
        public SafetyWarning[] ActiveWarnings;
        
        /// <summary>
        /// Current safety risk level.
        /// </summary>
        public SafetyRiskLevel RiskLevel;
        
        /// <summary>
        /// Time since last safety violation.
        /// </summary>
        public TimeSpan TimeSinceLastViolation;
        
        /// <summary>
        /// Number of safety violations in current session.
        /// </summary>
        public int TotalViolationsThisSession;
        
        /// <summary>
        /// Whether automatic termination is armed.
        /// </summary>
        public bool AutoTerminationArmed;
        
        /// <summary>
        /// Estimated time to automatic termination if current trends continue.
        /// </summary>
        public TimeSpan? EstimatedTimeToTermination;
    }
    
    /// <summary>
    /// Safety check result with detailed information.
    /// </summary>
    public struct SafetyCheckResult
    {
        /// <summary>
        /// Whether the safety check passed.
        /// </summary>
        public bool Passed;
        
        /// <summary>
        /// Safety violations found during check.
        /// </summary>
        public SafetyViolation[] Violations;
        
        /// <summary>
        /// Current safety score (0-100, higher is safer).
        /// </summary>
        public float SafetyScore;
        
        /// <summary>
        /// Recommended actions based on safety check.
        /// </summary>
        public string[] RecommendedActions;
        
        /// <summary>
        /// Whether immediate termination is recommended.
        /// </summary>
        public bool RecommendImmediateTermination;
        
        /// <summary>
        /// Timestamp of the safety check.
        /// </summary>
        public DateTime CheckTimestamp;
    }
    
    /// <summary>
    /// Thermal monitoring data with temperature trends.
    /// </summary>
    public struct ThermalMonitoringData
    {
        /// <summary>
        /// Current thermal state.
        /// </summary>
        public ThermalState CurrentState;
        
        /// <summary>
        /// Temperature readings from all sensors.
        /// </summary>
        public ThermalSensorReading[] SensorReadings;
        
        /// <summary>
        /// Temperature trend over recent samples.
        /// </summary>
        public TemperatureTrend RecentTrend;
        
        /// <summary>
        /// Thermal throttling events detected.
        /// </summary>
        public ThermalThrottlingEvent[] ThrottlingEvents;
        
        /// <summary>
        /// Estimated time to thermal limit at current trend.
        /// </summary>
        public TimeSpan? EstimatedTimeToThermalLimit;
        
        /// <summary>
        /// Thermal safety margin in Celsius.
        /// </summary>
        public float ThermalSafetyMarginCelsius;
    }
    
    /// <summary>
    /// Power consumption and battery monitoring data.
    /// </summary>
    public struct PowerMonitoringData
    {
        /// <summary>
        /// Current power consumption breakdown.
        /// </summary>
        public PowerConsumptionBreakdown CurrentConsumption;
        
        /// <summary>
        /// Battery status information.
        /// </summary>
        public BatteryStatus BatteryStatus;
        
        /// <summary>
        /// Power efficiency metrics.
        /// </summary>
        public PowerEfficiencyMetrics Efficiency;
        
        /// <summary>
        /// Estimated remaining battery life at current consumption.
        /// </summary>
        public TimeSpan EstimatedBatteryLife;
        
        /// <summary>
        /// Power consumption trend over recent samples.
        /// </summary>
        public PowerConsumptionTrend RecentTrend;
    }
    
    /// <summary>
    /// Performance trend analysis results.
    /// </summary>
    public struct PerformanceTrendAnalysis
    {
        /// <summary>
        /// Frame rate trend analysis.
        /// </summary>
        public TrendAnalysis FrameRateTrend;
        
        /// <summary>
        /// CPU usage trend analysis.
        /// </summary>
        public TrendAnalysis CPUUsageTrend;
        
        /// <summary>
        /// GPU usage trend analysis.
        /// </summary>
        public TrendAnalysis GPUUsageTrend;
        
        /// <summary>
        /// Memory usage trend analysis.
        /// </summary>
        public TrendAnalysis MemoryUsageTrend;
        
        /// <summary>
        /// Temperature trend analysis.
        /// </summary>
        public TrendAnalysis TemperatureTrend;
        
        /// <summary>
        /// Power consumption trend analysis.
        /// </summary>
        public TrendAnalysis PowerConsumptionTrend;
        
        /// <summary>
        /// Overall performance trend summary.
        /// </summary>
        public OverallTrendSummary OverallTrend;
        
        /// <summary>
        /// Predicted performance at current trends.
        /// </summary>
        public PerformancePrediction Prediction;
    }
    
    /// <summary>
    /// Performance thresholds for monitoring alerts.
    /// </summary>
    [Serializable]
    public struct PerformanceThresholds
    {
        /// <summary>
        /// Frame rate warning threshold.
        /// </summary>
        public float FrameRateWarningThreshold;
        
        /// <summary>
        /// Frame rate critical threshold.
        /// </summary>
        public float FrameRateCriticalThreshold;
        
        /// <summary>
        /// CPU usage warning threshold.
        /// </summary>
        public float CPUUsageWarningThreshold;
        
        /// <summary>
        /// CPU usage critical threshold.
        /// </summary>
        public float CPUUsageCriticalThreshold;
        
        /// <summary>
        /// GPU usage warning threshold.
        /// </summary>
        public float GPUUsageWarningThreshold;
        
        /// <summary>
        /// GPU usage critical threshold.
        /// </summary>
        public float GPUUsageCriticalThreshold;
        
        /// <summary>
        /// Memory usage warning threshold.
        /// </summary>
        public float MemoryUsageWarningThreshold;
        
        /// <summary>
        /// Memory usage critical threshold.
        /// </summary>
        public float MemoryUsageCriticalThreshold;
        
        /// <summary>
        /// Temperature warning threshold.
        /// </summary>
        public float TemperatureWarningThreshold;
        
        /// <summary>
        /// Temperature critical threshold.
        /// </summary>
        public float TemperatureCriticalThreshold;
        
        /// <summary>
        /// Default performance thresholds for Quest 3.
        /// </summary>
        public static PerformanceThresholds Quest3Default => new PerformanceThresholds
        {
            FrameRateWarningThreshold = 60f,
            FrameRateCriticalThreshold = 45f,
            CPUUsageWarningThreshold = 80f,
            CPUUsageCriticalThreshold = 95f,
            GPUUsageWarningThreshold = 85f,
            GPUUsageCriticalThreshold = 95f,
            MemoryUsageWarningThreshold = 2000f,
            MemoryUsageCriticalThreshold = 2800f,
            TemperatureWarningThreshold = 70f,
            TemperatureCriticalThreshold = 80f
        };
    }
    
    /// <summary>
    /// Monitoring session history.
    /// </summary>
    public struct MonitoringSessionHistory
    {
        /// <summary>
        /// All completed monitoring sessions.
        /// </summary>
        public HardwareMonitoringResult[] CompletedSessions;
        
        /// <summary>
        /// Total monitoring time across all sessions.
        /// </summary>
        public TimeSpan TotalMonitoringTime;
        
        /// <summary>
        /// Average session duration.
        /// </summary>
        public TimeSpan AverageSessionDuration;
        
        /// <summary>
        /// Total safety violations across all sessions.
        /// </summary>
        public int TotalSafetyViolations;
        
        /// <summary>
        /// Most common safety violations.
        /// </summary>
        public string[] CommonSafetyViolations;
        
        /// <summary>
        /// Performance trends across sessions.
        /// </summary>
        public CrossSessionTrends PerformanceTrends;
    }
    
    /// <summary>
    /// Complete hardware monitoring result.
    /// </summary>
    public struct HardwareMonitoringResult
    {
        /// <summary>
        /// Session information.
        /// </summary>
        public HardwareMonitoringSession Session;
        
        /// <summary>
        /// Aggregated performance statistics.
        /// </summary>
        public HardwarePerformanceStatistics Statistics;
        
        /// <summary>
        /// Safety status summary.
        /// </summary>
        public HardwareSafetyStatus FinalSafetyStatus;
        
        /// <summary>
        /// All safety violations that occurred.
        /// </summary>
        public SafetyViolation[] SafetyViolations;
        
        /// <summary>
        /// Performance trend analysis.
        /// </summary>
        public PerformanceTrendAnalysis TrendAnalysis;
        
        /// <summary>
        /// Raw monitoring data samples.
        /// </summary>
        public RealTimeHardwareMetrics[] RawSamples;
        
        /// <summary>
        /// Whether monitoring completed successfully.
        /// </summary>
        public bool CompletedSuccessfully;
        
        /// <summary>
        /// Reason for monitoring termination.
        /// </summary>
        public string TerminationReason;
        
        /// <summary>
        /// Monitoring quality assessment.
        /// </summary>
        public MonitoringQualityAssessment QualityAssessment;
    }
    
    // Supporting enums and structures
    public enum MonitoringDataExportFormat { CSV, JSON, Binary }
    public enum SafetyRiskLevel { Low, Medium, High, Critical }
    public enum ThermalState { Normal, Warning, Critical, Throttling }
    public enum TemperatureTrend { Stable, Rising, Falling, Volatile }
    public enum PowerConsumptionTrend { Stable, Increasing, Decreasing, Volatile }
    public enum OverallTrendSummary { Stable, Improving, Degrading, Volatile }
    
    // Event argument structures
    public struct SafetyLimitExceededEventArgs
    {
        public SafetyViolation Violation;
        public RealTimeHardwareMetrics CurrentMetrics;
        public bool RecommendTermination;
        public DateTime Timestamp;
    }
    
    public struct PerformanceThresholdEventArgs
    {
        public string MetricName;
        public float CurrentValue;
        public float ThresholdValue;
        public bool IsCritical;
        public DateTime Timestamp;
    }
    
    public struct ThermalWarningEventArgs
    {
        public ThermalState ThermalState;
        public float CurrentTemperature;
        public string SensorLocation;
        public DateTime Timestamp;
    }
    
    public struct MonitoringDataUpdatedEventArgs
    {
        public RealTimeHardwareMetrics CurrentMetrics;
        public int TotalSamples;
        public TimeSpan MonitoringDuration;
        public DateTime Timestamp;
    }
    
    // Additional supporting structures would be defined here...
    public struct SafetyWarning { /* Safety warning details */ }
    public struct SafetyViolation { /* Safety violation details */ }
    public struct ThermalSensorReading { /* Thermal sensor data */ }
    public struct ThermalThrottlingEvent { /* Throttling event data */ }
    public struct PowerConsumptionBreakdown { /* Power consumption breakdown */ }
    public struct BatteryStatus { /* Battery status information */ }
    public struct PowerEfficiencyMetrics { /* Power efficiency data */ }
    public struct TrendAnalysis { /* Trend analysis data */ }
    public struct PerformancePrediction { /* Performance prediction data */ }
    public struct CrossSessionTrends { /* Cross-session trend data */ }
    public struct MonitoringQualityAssessment { /* Monitoring quality data */ }
}