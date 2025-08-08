using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Interface for OVR-Metrics integration providing accurate Quest 3 hardware performance measurement.
    /// Implements Requirement 5.4: OVR-Metrics integration for accurate measurement.
    /// Implements Requirement 8.2: Real-time performance monitoring.
    /// Implements Requirement 8.3: Performance data collection and analysis.
    /// </summary>
    public interface IOVRMetricsIntegration
    {
        /// <summary>
        /// Whether OVR-Metrics is available and initialized.
        /// </summary>
        bool IsOVRMetricsAvailable { get; }
        
        /// <summary>
        /// Whether a metrics capture session is currently active.
        /// </summary>
        bool IsCaptureSessionActive { get; }
        
        /// <summary>
        /// Current OVR-Metrics configuration.
        /// </summary>
        OVRMetricsConfig Configuration { get; }
        
        /// <summary>
        /// Initialize OVR-Metrics integration with the specified configuration.
        /// </summary>
        /// <param name="config">OVR-Metrics configuration</param>
        /// <returns>True if initialization succeeded</returns>
        bool Initialize(OVRMetricsConfig config);
        
        /// <summary>
        /// Start a 60-second performance capture session as recommended by Oculus.
        /// </summary>
        /// <param name="sessionName">Name for the capture session</param>
        /// <returns>Capture session handle</returns>
        OVRMetricsCaptureSession StartCaptureSession(string sessionName = "Quest3_Performance_Capture");
        
        /// <summary>
        /// Stop the current capture session and process the results.
        /// </summary>
        /// <returns>Processed metrics data from the capture session</returns>
        OVRMetricsResult StopCaptureSession();
        
        /// <summary>
        /// Get real-time performance metrics from OVR-Metrics.
        /// </summary>
        /// <returns>Current real-time metrics</returns>
        OVRRealTimeMetrics GetRealTimeMetrics();
        
        /// <summary>
        /// Parse CSV data exported from OVR-Metrics capture sessions.
        /// </summary>
        /// <param name="csvFilePath">Path to the CSV file</param>
        /// <returns>Parsed metrics data</returns>
        OVRMetricsResult ParseCSVData(string csvFilePath);
        
        /// <summary>
        /// Analyze performance data and generate insights.
        /// </summary>
        /// <param name="metricsData">Raw metrics data to analyze</param>
        /// <returns>Analysis results with insights and recommendations</returns>
        OVRMetricsAnalysis AnalyzePerformanceData(OVRMetricsResult metricsData);
        
        /// <summary>
        /// Configure automatic performance alerts based on thresholds.
        /// </summary>
        /// <param name="alertConfig">Alert configuration</param>
        void ConfigurePerformanceAlerts(OVRPerformanceAlertConfig alertConfig);
        
        /// <summary>
        /// Get thermal data from OVR-Metrics thermal sensors.
        /// </summary>
        /// <returns>Thermal performance data</returns>
        OVRThermalData GetThermalData();
        
        /// <summary>
        /// Get battery and power consumption data.
        /// </summary>
        /// <returns>Power consumption metrics</returns>
        OVRPowerMetrics GetPowerMetrics();
        
        /// <summary>
        /// Export metrics data to various formats for analysis.
        /// </summary>
        /// <param name="metricsData">Data to export</param>
        /// <param name="format">Export format</param>
        /// <param name="outputPath">Output file path</param>
        /// <returns>True if export succeeded</returns>
        bool ExportMetricsData(OVRMetricsResult metricsData, OVRExportFormat format, string outputPath);
        
        /// <summary>
        /// Validate that OVR-Metrics data meets Quest 3 certification requirements.
        /// </summary>
        /// <param name="metricsData">Metrics data to validate</param>
        /// <returns>Certification validation result</returns>
        OVRCertificationValidationResult ValidateCertificationRequirements(OVRMetricsResult metricsData);
        
        /// <summary>
        /// Clean up OVR-Metrics resources and stop all monitoring.
        /// </summary>
        void Dispose();
    }
    
    /// <summary>
    /// Configuration for OVR-Metrics integration.
    /// </summary>
    [Serializable]
    public struct OVRMetricsConfig
    {
        /// <summary>
        /// Whether to enable CPU performance monitoring.
        /// </summary>
        public bool EnableCpuMonitoring;
        
        /// <summary>
        /// Whether to enable GPU performance monitoring.
        /// </summary>
        public bool EnableGpuMonitoring;
        
        /// <summary>
        /// Whether to enable memory usage monitoring.
        /// </summary>
        public bool EnableMemoryMonitoring;
        
        /// <summary>
        /// Whether to enable thermal monitoring.
        /// </summary>
        public bool EnableThermalMonitoring;
        
        /// <summary>
        /// Whether to enable power consumption monitoring.
        /// </summary>
        public bool EnablePowerMonitoring;
        
        /// <summary>
        /// Metrics sampling rate in Hz (1-60).
        /// </summary>
        public float SamplingRateHz;
        
        /// <summary>
        /// Whether to automatically export CSV data after capture.
        /// </summary>
        public bool AutoExportCSV;
        
        /// <summary>
        /// Directory path for exported data files.
        /// </summary>
        public string ExportDirectory;
        
        /// <summary>
        /// Whether to enable real-time performance overlay.
        /// </summary>
        public bool EnablePerformanceOverlay;
        
        /// <summary>
        /// Whether to log detailed debug information.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Default OVR-Metrics configuration optimized for Quest 3.
        /// </summary>
        public static OVRMetricsConfig Default => new OVRMetricsConfig
        {
            EnableCpuMonitoring = true,
            EnableGpuMonitoring = true,
            EnableMemoryMonitoring = true,
            EnableThermalMonitoring = true,
            EnablePowerMonitoring = true,
            SamplingRateHz = 10f,
            AutoExportCSV = true,
            ExportDirectory = "OVRMetrics_Data",
            EnablePerformanceOverlay = false,
            EnableDebugLogging = false
        };
        
        /// <summary>
        /// High-frequency monitoring configuration for detailed analysis.
        /// </summary>
        public static OVRMetricsConfig HighFrequency => new OVRMetricsConfig
        {
            EnableCpuMonitoring = true,
            EnableGpuMonitoring = true,
            EnableMemoryMonitoring = true,
            EnableThermalMonitoring = true,
            EnablePowerMonitoring = true,
            SamplingRateHz = 30f,
            AutoExportCSV = true,
            ExportDirectory = "OVRMetrics_HighFreq",
            EnablePerformanceOverlay = true,
            EnableDebugLogging = true
        };
    }
    
    /// <summary>
    /// OVR-Metrics capture session handle.
    /// </summary>
    public struct OVRMetricsCaptureSession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Session name provided by user.
        /// </summary>
        public string SessionName;
        
        /// <summary>
        /// Session start timestamp.
        /// </summary>
        public DateTime StartTime;
        
        /// <summary>
        /// Planned capture duration in seconds.
        /// </summary>
        public float PlannedDurationSeconds;
        
        /// <summary>
        /// Configuration used for this session.
        /// </summary>
        public OVRMetricsConfig Configuration;
        
        /// <summary>
        /// Whether the session is currently active.
        /// </summary>
        public bool IsActive;
        
        /// <summary>
        /// Path where session data will be saved.
        /// </summary>
        public string DataPath;
    }
    
    /// <summary>
    /// Complete OVR-Metrics result data.
    /// </summary>
    public struct OVRMetricsResult
    {
        /// <summary>
        /// Session information.
        /// </summary>
        public OVRMetricsCaptureSession Session;
        
        /// <summary>
        /// CPU performance metrics.
        /// </summary>
        public OVRCpuMetrics CpuMetrics;
        
        /// <summary>
        /// GPU performance metrics.
        /// </summary>
        public OVRGpuMetrics GpuMetrics;
        
        /// <summary>
        /// Memory usage metrics.
        /// </summary>
        public OVRMemoryMetrics MemoryMetrics;
        
        /// <summary>
        /// Thermal performance data.
        /// </summary>
        public OVRThermalData ThermalData;
        
        /// <summary>
        /// Power consumption metrics.
        /// </summary>
        public OVRPowerMetrics PowerMetrics;
        
        /// <summary>
        /// Frame rate and timing metrics.
        /// </summary>
        public OVRFrameMetrics FrameMetrics;
        
        /// <summary>
        /// Raw time-series data points.
        /// </summary>
        public OVRDataPoint[] RawDataPoints;
        
        /// <summary>
        /// Total number of samples collected.
        /// </summary>
        public int TotalSamples;
        
        /// <summary>
        /// Actual capture duration in seconds.
        /// </summary>
        public float ActualDurationSeconds;
        
        /// <summary>
        /// Data quality indicators.
        /// </summary>
        public OVRDataQuality DataQuality;
    }
    
    /// <summary>
    /// Real-time OVR-Metrics data.
    /// </summary>
    public struct OVRRealTimeMetrics
    {
        /// <summary>
        /// Current frame rate in FPS.
        /// </summary>
        public float CurrentFrameRate;
        
        /// <summary>
        /// Current frame time in milliseconds.
        /// </summary>
        public float CurrentFrameTimeMs;
        
        /// <summary>
        /// Current CPU usage percentage (0-100).
        /// </summary>
        public float CurrentCpuUsage;
        
        /// <summary>
        /// Current GPU usage percentage (0-100).
        /// </summary>
        public float CurrentGpuUsage;
        
        /// <summary>
        /// Current memory usage in MB.
        /// </summary>
        public float CurrentMemoryUsageMB;
        
        /// <summary>
        /// Current device temperature in Celsius.
        /// </summary>
        public float CurrentTemperatureCelsius;
        
        /// <summary>
        /// Current power consumption in watts.
        /// </summary>
        public float CurrentPowerConsumptionW;
        
        /// <summary>
        /// Current battery level percentage (0-100).
        /// </summary>
        public float CurrentBatteryLevel;
        
        /// <summary>
        /// Timestamp of the measurement.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Whether the data is valid and up-to-date.
        /// </summary>
        public bool IsDataValid;
    }
    
    /// <summary>
    /// CPU performance metrics from OVR-Metrics.
    /// </summary>
    public struct OVRCpuMetrics
    {
        /// <summary>
        /// Average CPU usage percentage.
        /// </summary>
        public float AverageUsagePercent;
        
        /// <summary>
        /// Peak CPU usage percentage.
        /// </summary>
        public float PeakUsagePercent;
        
        /// <summary>
        /// CPU usage standard deviation.
        /// </summary>
        public float UsageStandardDeviation;
        
        /// <summary>
        /// Average CPU frequency in MHz.
        /// </summary>
        public float AverageFrequencyMHz;
        
        /// <summary>
        /// CPU throttling events detected.
        /// </summary>
        public int ThrottlingEvents;
        
        /// <summary>
        /// Time spent in different CPU frequency states.
        /// </summary>
        public OVRFrequencyStateData[] FrequencyStates;
        
        /// <summary>
        /// Per-core CPU usage data.
        /// </summary>
        public float[] PerCoreUsage;
    }
    
    /// <summary>
    /// GPU performance metrics from OVR-Metrics.
    /// </summary>
    public struct OVRGpuMetrics
    {
        /// <summary>
        /// Average GPU usage percentage.
        /// </summary>
        public float AverageUsagePercent;
        
        /// <summary>
        /// Peak GPU usage percentage.
        /// </summary>
        public float PeakUsagePercent;
        
        /// <summary>
        /// GPU usage standard deviation.
        /// </summary>
        public float UsageStandardDeviation;
        
        /// <summary>
        /// Average GPU frequency in MHz.
        /// </summary>
        public float AverageFrequencyMHz;
        
        /// <summary>
        /// GPU throttling events detected.
        /// </summary>
        public int ThrottlingEvents;
        
        /// <summary>
        /// Average GPU memory usage in MB.
        /// </summary>
        public float AverageMemoryUsageMB;
        
        /// <summary>
        /// Peak GPU memory usage in MB.
        /// </summary>
        public float PeakMemoryUsageMB;
        
        /// <summary>
        /// GPU temperature data.
        /// </summary>
        public float AverageTemperatureCelsius;
    }
    
    /// <summary>
    /// Memory usage metrics from OVR-Metrics.
    /// </summary>
    public struct OVRMemoryMetrics
    {
        /// <summary>
        /// Average system memory usage in MB.
        /// </summary>
        public float AverageSystemMemoryMB;
        
        /// <summary>
        /// Peak system memory usage in MB.
        /// </summary>
        public float PeakSystemMemoryMB;
        
        /// <summary>
        /// Average application memory usage in MB.
        /// </summary>
        public float AverageAppMemoryMB;
        
        /// <summary>
        /// Peak application memory usage in MB.
        /// </summary>
        public float PeakAppMemoryMB;
        
        /// <summary>
        /// Memory allocation rate in MB/s.
        /// </summary>
        public float AllocationRateMBPerSecond;
        
        /// <summary>
        /// Garbage collection events detected.
        /// </summary>
        public int GarbageCollectionEvents;
        
        /// <summary>
        /// Average garbage collection pause time in milliseconds.
        /// </summary>
        public float AverageGCPauseTimeMs;
        
        /// <summary>
        /// Memory pressure level (0-100).
        /// </summary>
        public float MemoryPressureLevel;
    }
    
    /// <summary>
    /// Thermal performance data from OVR-Metrics.
    /// </summary>
    public struct OVRThermalData
    {
        /// <summary>
        /// Average device temperature in Celsius.
        /// </summary>
        public float AverageTemperatureCelsius;
        
        /// <summary>
        /// Peak device temperature in Celsius.
        /// </summary>
        public float PeakTemperatureCelsius;
        
        /// <summary>
        /// Temperature readings from different sensors.
        /// </summary>
        public OVRThermalSensorData[] SensorReadings;
        
        /// <summary>
        /// Thermal throttling events detected.
        /// </summary>
        public int ThermalThrottlingEvents;
        
        /// <summary>
        /// Time to reach thermal equilibrium in seconds.
        /// </summary>
        public float TimeToEquilibriumSeconds;
        
        /// <summary>
        /// Thermal safety margin in Celsius.
        /// </summary>
        public float ThermalSafetyMarginCelsius;
        
        /// <summary>
        /// Whether thermal limits were exceeded.
        /// </summary>
        public bool ThermalLimitsExceeded;
    }
    
    /// <summary>
    /// Power consumption metrics from OVR-Metrics.
    /// </summary>
    public struct OVRPowerMetrics
    {
        /// <summary>
        /// Average power consumption in watts.
        /// </summary>
        public float AveragePowerConsumptionW;
        
        /// <summary>
        /// Peak power consumption in watts.
        /// </summary>
        public float PeakPowerConsumptionW;
        
        /// <summary>
        /// Battery drain rate in mAh/hour.
        /// </summary>
        public float BatteryDrainRateMahPerHour;
        
        /// <summary>
        /// Estimated battery life in hours.
        /// </summary>
        public float EstimatedBatteryLifeHours;
        
        /// <summary>
        /// Power efficiency score (0-100).
        /// </summary>
        public float PowerEfficiencyScore;
        
        /// <summary>
        /// CPU power consumption in watts.
        /// </summary>
        public float CpuPowerConsumptionW;
        
        /// <summary>
        /// GPU power consumption in watts.
        /// </summary>
        public float GpuPowerConsumptionW;
        
        /// <summary>
        /// Display power consumption in watts.
        /// </summary>
        public float DisplayPowerConsumptionW;
    }
    
    /// <summary>
    /// Frame rate and timing metrics from OVR-Metrics.
    /// </summary>
    public struct OVRFrameMetrics
    {
        /// <summary>
        /// Average frame rate in FPS.
        /// </summary>
        public float AverageFrameRate;
        
        /// <summary>
        /// Minimum frame rate observed.
        /// </summary>
        public float MinFrameRate;
        
        /// <summary>
        /// Maximum frame rate observed.
        /// </summary>
        public float MaxFrameRate;
        
        /// <summary>
        /// Frame rate standard deviation.
        /// </summary>
        public float FrameRateStandardDeviation;
        
        /// <summary>
        /// Average frame time in milliseconds.
        /// </summary>
        public float AverageFrameTimeMs;
        
        /// <summary>
        /// 95th percentile frame time.
        /// </summary>
        public float P95FrameTimeMs;
        
        /// <summary>
        /// 99th percentile frame time.
        /// </summary>
        public float P99FrameTimeMs;
        
        /// <summary>
        /// Number of dropped frames.
        /// </summary>
        public int DroppedFrames;
        
        /// <summary>
        /// Frame pacing consistency score (0-100).
        /// </summary>
        public float FramePacingScore;
        
        /// <summary>
        /// VSync effectiveness percentage.
        /// </summary>
        public float VSyncEffectiveness;
    }
    
    /// <summary>
    /// Individual data point from OVR-Metrics time series.
    /// </summary>
    public struct OVRDataPoint
    {
        /// <summary>
        /// Timestamp of the measurement.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Frame rate at this point.
        /// </summary>
        public float FrameRate;
        
        /// <summary>
        /// CPU usage percentage.
        /// </summary>
        public float CpuUsage;
        
        /// <summary>
        /// GPU usage percentage.
        /// </summary>
        public float GpuUsage;
        
        /// <summary>
        /// Memory usage in MB.
        /// </summary>
        public float MemoryUsageMB;
        
        /// <summary>
        /// Temperature in Celsius.
        /// </summary>
        public float TemperatureCelsius;
        
        /// <summary>
        /// Power consumption in watts.
        /// </summary>
        public float PowerConsumptionW;
    }
    
    /// <summary>
    /// Data quality indicators for OVR-Metrics capture.
    /// </summary>
    public struct OVRDataQuality
    {
        /// <summary>
        /// Percentage of samples successfully collected (0-100).
        /// </summary>
        public float SampleCompleteness;
        
        /// <summary>
        /// Number of data gaps or missing samples.
        /// </summary>
        public int DataGaps;
        
        /// <summary>
        /// Whether the capture session completed successfully.
        /// </summary>
        public bool CaptureCompleted;
        
        /// <summary>
        /// Data reliability score (0-100).
        /// </summary>
        public float ReliabilityScore;
        
        /// <summary>
        /// Issues detected during capture.
        /// </summary>
        public string[] CaptureIssues;
    }
    
    /// <summary>
    /// Analysis results from OVR-Metrics data.
    /// </summary>
    public struct OVRMetricsAnalysis
    {
        /// <summary>
        /// Overall performance score (0-100).
        /// </summary>
        public float OverallPerformanceScore;
        
        /// <summary>
        /// Performance bottlenecks identified.
        /// </summary>
        public string[] PerformanceBottlenecks;
        
        /// <summary>
        /// Optimization recommendations.
        /// </summary>
        public string[] OptimizationRecommendations;
        
        /// <summary>
        /// Performance trends detected.
        /// </summary>
        public string[] PerformanceTrends;
        
        /// <summary>
        /// Critical issues requiring immediate attention.
        /// </summary>
        public string[] CriticalIssues;
        
        /// <summary>
        /// Detailed analysis report.
        /// </summary>
        public string DetailedReport;
        
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp;
    }
    
    /// <summary>
    /// Configuration for performance alerts.
    /// </summary>
    [Serializable]
    public struct OVRPerformanceAlertConfig
    {
        /// <summary>
        /// Frame rate threshold for alerts.
        /// </summary>
        public float FrameRateThreshold;
        
        /// <summary>
        /// CPU usage threshold for alerts.
        /// </summary>
        public float CpuUsageThreshold;
        
        /// <summary>
        /// GPU usage threshold for alerts.
        /// </summary>
        public float GpuUsageThreshold;
        
        /// <summary>
        /// Memory usage threshold in MB.
        /// </summary>
        public float MemoryThresholdMB;
        
        /// <summary>
        /// Temperature threshold in Celsius.
        /// </summary>
        public float TemperatureThreshold;
        
        /// <summary>
        /// Whether to enable audio alerts.
        /// </summary>
        public bool EnableAudioAlerts;
        
        /// <summary>
        /// Whether to enable visual alerts.
        /// </summary>
        public bool EnableVisualAlerts;
        
        /// <summary>
        /// Whether to log alerts to file.
        /// </summary>
        public bool LogAlertsToFile;
    }
    
    /// <summary>
    /// Thermal sensor data from OVR-Metrics.
    /// </summary>
    public struct OVRThermalSensorData
    {
        /// <summary>
        /// Sensor identifier.
        /// </summary>
        public string SensorId;
        
        /// <summary>
        /// Sensor location description.
        /// </summary>
        public string SensorLocation;
        
        /// <summary>
        /// Average temperature reading.
        /// </summary>
        public float AverageTemperatureCelsius;
        
        /// <summary>
        /// Peak temperature reading.
        /// </summary>
        public float PeakTemperatureCelsius;
        
        /// <summary>
        /// Temperature readings over time.
        /// </summary>
        public float[] TemperatureReadings;
    }
    
    /// <summary>
    /// CPU frequency state data.
    /// </summary>
    public struct OVRFrequencyStateData
    {
        /// <summary>
        /// Frequency level in MHz.
        /// </summary>
        public float FrequencyMHz;
        
        /// <summary>
        /// Percentage of time spent in this frequency state.
        /// </summary>
        public float TimePercentage;
        
        /// <summary>
        /// Power consumption at this frequency.
        /// </summary>
        public float PowerConsumptionW;
    }
    
    /// <summary>
    /// Export formats for OVR-Metrics data.
    /// </summary>
    public enum OVRExportFormat
    {
        CSV,
        JSON,
        XML,
        Binary
    }
    
    /// <summary>
    /// Certification validation result for OVR-Metrics data.
    /// </summary>
    public struct OVRCertificationValidationResult
    {
        /// <summary>
        /// Whether the data meets certification requirements.
        /// </summary>
        public bool MeetsCertificationRequirements;
        
        /// <summary>
        /// Certification score (0-100).
        /// </summary>
        public float CertificationScore;
        
        /// <summary>
        /// Requirements that were not met.
        /// </summary>
        public string[] UnmetRequirements;
        
        /// <summary>
        /// Actions required for certification.
        /// </summary>
        public string[] RequiredActions;
        
        /// <summary>
        /// Certification validation summary.
        /// </summary>
        public string ValidationSummary;
        
        /// <summary>
        /// Validation timestamp.
        /// </summary>
        public DateTime ValidationTimestamp;
    }
}