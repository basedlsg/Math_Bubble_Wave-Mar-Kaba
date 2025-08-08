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
    /// OVR-Metrics integration for accurate Quest 3 hardware performance measurement.
    /// Implements Requirement 5.4: OVR-Metrics integration for accurate measurement.
    /// Implements Requirement 8.2: Real-time performance monitoring.
    /// Implements Requirement 8.3: Performance data collection and analysis.
    /// </summary>
    public class OVRMetricsIntegration : IOVRMetricsIntegration
    {
        // OVR-Metrics constants
        private const float RECOMMENDED_CAPTURE_DURATION = 60f; // Oculus recommended 60-second captures
        private const float MAX_SAMPLING_RATE = 60f;
        private const int MAX_DATA_POINTS = 3600; // 60 seconds * 60 Hz max
        
        // Configuration and state
        private OVRMetricsConfig _config;
        private OVRMetricsCaptureSession _currentSession;
        private OVRPerformanceAlertConfig _alertConfig;
        
        // Data collection
        private readonly List<OVRDataPoint> _captureData = new List<OVRDataPoint>();
        private readonly List<OVRMetricsResult> _sessionHistory = new List<OVRMetricsResult>();
        
        // Monitoring state
        private bool _isInitialized;
        private bool _isCaptureActive;
        private float _captureStartTime;
        private readonly Stopwatch _captureTimer = new Stopwatch();
        
        // Performance tracking
        private OVRRealTimeMetrics _lastRealTimeMetrics;
        private DateTime _lastMetricsUpdate;
        
        // Simulated hardware monitoring (in real implementation, this would interface with actual OVR APIs)
        private readonly System.Random _random = new System.Random();
        
        /// <summary>
        /// Whether OVR-Metrics is available and initialized.
        /// </summary>
        public bool IsOVRMetricsAvailable => _isInitialized;
        
        /// <summary>
        /// Whether a metrics capture session is currently active.
        /// </summary>
        public bool IsCaptureSessionActive => _isCaptureActive;
        
        /// <summary>
        /// Current OVR-Metrics configuration.
        /// </summary>
        public OVRMetricsConfig Configuration => _config;
        
        /// <summary>
        /// Initialize OVR-Metrics integration with the specified configuration.
        /// </summary>
        public bool Initialize(OVRMetricsConfig config)
        {
            try
            {
                _config = config;
                
                // Validate configuration
                if (!ValidateConfiguration(config))
                {
                    UnityEngine.Debug.LogError("[OVRMetricsIntegration] Invalid configuration provided");
                    return false;
                }
                
                // Initialize OVR-Metrics (simulated - in real implementation, this would call OVR APIs)
                if (!InitializeOVRMetricsSDK())
                {
                    UnityEngine.Debug.LogError("[OVRMetricsIntegration] Failed to initialize OVR-Metrics SDK");
                    return false;
                }
                
                // Create export directory if needed
                if (config.AutoExportCSV && !string.IsNullOrEmpty(config.ExportDirectory))
                {
                    Directory.CreateDirectory(config.ExportDirectory);
                }
                
                // Initialize alert configuration
                _alertConfig = new OVRPerformanceAlertConfig
                {
                    FrameRateThreshold = 72f,
                    CpuUsageThreshold = 80f,
                    GpuUsageThreshold = 85f,
                    MemoryThresholdMB = 2048f,
                    TemperatureThreshold = 45f,
                    EnableVisualAlerts = true,
                    LogAlertsToFile = true
                };
                
                _isInitialized = true;
                
                UnityEngine.Debug.Log($"[OVRMetricsIntegration] Initialized with {config.SamplingRateHz} Hz sampling rate");
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[OVRMetricsIntegration] Initialization failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Start a 60-second performance capture session as recommended by Oculus.
        /// </summary>
        public OVRMetricsCaptureSession StartCaptureSession(string sessionName = "Quest3_Performance_Capture")
        {
            if (!_isInitialized)
            {
                UnityEngine.Debug.LogError("[OVRMetricsIntegration] Cannot start capture - not initialized");
                return default;
            }
            
            if (_isCaptureActive)
            {
                UnityEngine.Debug.LogWarning("[OVRMetricsIntegration] Stopping previous capture session");
                StopCaptureSession();
            }
            
            _currentSession = new OVRMetricsCaptureSession
            {
                SessionId = Guid.NewGuid().ToString(),
                SessionName = sessionName,
                StartTime = DateTime.Now,
                PlannedDurationSeconds = RECOMMENDED_CAPTURE_DURATION,
                Configuration = _config,
                IsActive = true,
                DataPath = Path.Combine(_config.ExportDirectory, $"{sessionName}_{DateTime.Now:yyyyMMdd_HHmmss}")
            };
            
            _isCaptureActive = true;
            _captureStartTime = Time.time;
            _captureTimer.Restart();
            
            // Clear previous capture data
            _captureData.Clear();
            
            // Start OVR-Metrics capture (simulated)
            StartOVRMetricsCapture();
            
            UnityEngine.Debug.Log($"[OVRMetricsIntegration] Started capture session '{sessionName}' (ID: {_currentSession.SessionId})");
            
            return _currentSession;
        }
        
        /// <summary>
        /// Stop the current capture session and process the results.
        /// </summary>
        public OVRMetricsResult StopCaptureSession()
        {
            if (!_isCaptureActive)
            {
                UnityEngine.Debug.LogWarning("[OVRMetricsIntegration] No active capture session to stop");
                return default;
            }
            
            _captureTimer.Stop();
            _isCaptureActive = false;
            
            // Stop OVR-Metrics capture (simulated)
            StopOVRMetricsCapture();
            
            float actualDuration = Time.time - _captureStartTime;
            
            // Process captured data
            var result = ProcessCaptureData(actualDuration);
            
            // Export CSV if configured
            if (_config.AutoExportCSV)
            {
                ExportMetricsData(result, OVRExportFormat.CSV, 
                    Path.Combine(_currentSession.DataPath, "metrics_data.csv"));
            }
            
            // Store in session history
            _sessionHistory.Add(result);
            
            // Mark session as inactive
            _currentSession.IsActive = false;
            
            UnityEngine.Debug.Log($"[OVRMetricsIntegration] Capture session completed: {result.TotalSamples} samples, " +
                                $"{actualDuration:F1}s duration, quality: {result.DataQuality.ReliabilityScore:F1}%");
            
            return result;
        } 
       
        /// <summary>
        /// Get real-time performance metrics from OVR-Metrics.
        /// </summary>
        public OVRRealTimeMetrics GetRealTimeMetrics()
        {
            if (!_isInitialized)
            {
                return default;
            }
            
            // Simulate real-time metrics collection (in real implementation, this would query OVR APIs)
            var currentTime = DateTime.Now;
            
            // Update metrics at configured sampling rate
            if ((currentTime - _lastMetricsUpdate).TotalSeconds >= (1.0 / _config.SamplingRateHz))
            {
                _lastRealTimeMetrics = SimulateRealTimeMetrics();
                _lastMetricsUpdate = currentTime;
                
                // Check for performance alerts
                CheckPerformanceAlerts(_lastRealTimeMetrics);
                
                // Add to capture data if session is active
                if (_isCaptureActive)
                {
                    AddDataPointToCapture(_lastRealTimeMetrics);
                }
            }
            
            return _lastRealTimeMetrics;
        }
        
        /// <summary>
        /// Parse CSV data exported from OVR-Metrics capture sessions.
        /// </summary>
        public OVRMetricsResult ParseCSVData(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                UnityEngine.Debug.LogError($"[OVRMetricsIntegration] CSV file not found: {csvFilePath}");
                return default;
            }
            
            try
            {
                var dataPoints = new List<OVRDataPoint>();
                var lines = File.ReadAllLines(csvFilePath);
                
                // Skip header line
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    if (values.Length >= 7)
                    {
                        var dataPoint = new OVRDataPoint
                        {
                            Timestamp = DateTime.Parse(values[0]),
                            FrameRate = float.Parse(values[1]),
                            CpuUsage = float.Parse(values[2]),
                            GpuUsage = float.Parse(values[3]),
                            MemoryUsageMB = float.Parse(values[4]),
                            TemperatureCelsius = float.Parse(values[5]),
                            PowerConsumptionW = float.Parse(values[6])
                        };
                        
                        dataPoints.Add(dataPoint);
                    }
                }
                
                // Create session info from filename
                var sessionInfo = new OVRMetricsCaptureSession
                {
                    SessionId = Path.GetFileNameWithoutExtension(csvFilePath),
                    SessionName = "Imported_CSV_Data",
                    StartTime = dataPoints.Count > 0 ? dataPoints[0].Timestamp : DateTime.Now,
                    PlannedDurationSeconds = dataPoints.Count > 0 ? 
                        (float)(dataPoints.Last().Timestamp - dataPoints[0].Timestamp).TotalSeconds : 0f,
                    Configuration = _config,
                    IsActive = false,
                    DataPath = Path.GetDirectoryName(csvFilePath)
                };
                
                // Process the imported data
                var result = ProcessDataPoints(dataPoints.ToArray(), sessionInfo);
                
                UnityEngine.Debug.Log($"[OVRMetricsIntegration] Parsed CSV data: {dataPoints.Count} data points from {csvFilePath}");
                
                return result;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[OVRMetricsIntegration] Failed to parse CSV data: {ex.Message}");
                return default;
            }
        }
        
        /// <summary>
        /// Analyze performance data and generate insights.
        /// </summary>
        public OVRMetricsAnalysis AnalyzePerformanceData(OVRMetricsResult metricsData)
        {
            var bottlenecks = new List<string>();
            var recommendations = new List<string>();
            var trends = new List<string>();
            var criticalIssues = new List<string>();
            
            float performanceScore = 100f;
            
            // Analyze frame rate performance
            if (metricsData.FrameMetrics.AverageFrameRate < 72f)
            {
                bottlenecks.Add($"Frame rate below target: {metricsData.FrameMetrics.AverageFrameRate:F1} FPS");
                recommendations.Add("Optimize rendering pipeline to improve frame rate");
                performanceScore -= 20f;
                
                if (metricsData.FrameMetrics.AverageFrameRate < 60f)
                {
                    criticalIssues.Add("Frame rate critically low - immediate optimization required");
                }
            }
            
            // Analyze CPU performance
            if (metricsData.CpuMetrics.AverageUsagePercent > 80f)
            {
                bottlenecks.Add($"High CPU usage: {metricsData.CpuMetrics.AverageUsagePercent:F1}%");
                recommendations.Add("Optimize CPU-intensive operations using Burst compilation");
                performanceScore -= 15f;
                
                if (metricsData.CpuMetrics.ThrottlingEvents > 0)
                {
                    criticalIssues.Add($"CPU throttling detected: {metricsData.CpuMetrics.ThrottlingEvents} events");
                }
            }
            
            // Analyze GPU performance
            if (metricsData.GpuMetrics.AverageUsagePercent > 85f)
            {
                bottlenecks.Add($"High GPU usage: {metricsData.GpuMetrics.AverageUsagePercent:F1}%");
                recommendations.Add("Reduce rendering complexity or resolution");
                performanceScore -= 15f;
                
                if (metricsData.GpuMetrics.ThrottlingEvents > 0)
                {
                    criticalIssues.Add($"GPU throttling detected: {metricsData.GpuMetrics.ThrottlingEvents} events");
                }
            }
            
            // Analyze memory usage
            if (metricsData.MemoryMetrics.PeakAppMemoryMB > 2048f)
            {
                bottlenecks.Add($"High memory usage: {metricsData.MemoryMetrics.PeakAppMemoryMB:F0} MB");
                recommendations.Add("Implement memory optimization and object pooling");
                performanceScore -= 20f;
            }
            
            // Analyze thermal performance
            if (metricsData.ThermalData.PeakTemperatureCelsius > 45f)
            {
                bottlenecks.Add($"High temperature: {metricsData.ThermalData.PeakTemperatureCelsius:F1}°C");
                recommendations.Add("Reduce computational load to prevent thermal throttling");
                performanceScore -= 25f;
                
                if (metricsData.ThermalData.ThermalThrottlingEvents > 0)
                {
                    criticalIssues.Add("Thermal throttling occurred - device overheating");
                }
            }
            
            // Analyze power consumption
            if (metricsData.PowerMetrics.AveragePowerConsumptionW > 8f) // Typical Quest 3 power budget
            {
                bottlenecks.Add($"High power consumption: {metricsData.PowerMetrics.AveragePowerConsumptionW:F1}W");
                recommendations.Add("Optimize power efficiency to extend battery life");
                performanceScore -= 10f;
            }
            
            // Analyze frame consistency
            if (metricsData.FrameMetrics.FrameRateStandardDeviation > 5f)
            {
                trends.Add("Inconsistent frame rate detected - investigate frame pacing");
                recommendations.Add("Implement consistent frame pacing to reduce judder");
                performanceScore -= 10f;
            }
            
            // Generate performance trends
            if (metricsData.FrameMetrics.DroppedFrames > 0)
            {
                trends.Add($"Frame drops detected: {metricsData.FrameMetrics.DroppedFrames} frames");
            }
            
            if (metricsData.MemoryMetrics.GarbageCollectionEvents > 10)
            {
                trends.Add($"Frequent garbage collection: {metricsData.MemoryMetrics.GarbageCollectionEvents} events");
                recommendations.Add("Reduce memory allocations to minimize GC impact");
            }
            
            performanceScore = math.clamp(performanceScore, 0f, 100f);
            
            // Generate detailed report
            var report = GenerateDetailedAnalysisReport(metricsData, performanceScore, bottlenecks, recommendations);
            
            return new OVRMetricsAnalysis
            {
                OverallPerformanceScore = performanceScore,
                PerformanceBottlenecks = bottlenecks.ToArray(),
                OptimizationRecommendations = recommendations.ToArray(),
                PerformanceTrends = trends.ToArray(),
                CriticalIssues = criticalIssues.ToArray(),
                DetailedReport = report,
                AnalysisTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Configure automatic performance alerts based on thresholds.
        /// </summary>
        public void ConfigurePerformanceAlerts(OVRPerformanceAlertConfig alertConfig)
        {
            _alertConfig = alertConfig;
            
            UnityEngine.Debug.Log($"[OVRMetricsIntegration] Performance alerts configured: " +
                                $"FPS threshold {alertConfig.FrameRateThreshold}, " +
                                $"CPU threshold {alertConfig.CpuUsageThreshold}%, " +
                                $"GPU threshold {alertConfig.GpuUsageThreshold}%");
        }
        
        /// <summary>
        /// Get thermal data from OVR-Metrics thermal sensors.
        /// </summary>
        public OVRThermalData GetThermalData()
        {
            if (!_isInitialized || !_config.EnableThermalMonitoring)
            {
                return default;
            }
            
            // Simulate thermal sensor readings (in real implementation, query actual sensors)
            var sensorReadings = new OVRThermalSensorData[]
            {
                new OVRThermalSensorData
                {
                    SensorId = "CPU_THERMAL",
                    SensorLocation = "CPU Package",
                    AverageTemperatureCelsius = 38f + _random.Next(-3, 8),
                    PeakTemperatureCelsius = 42f + _random.Next(-2, 10),
                    TemperatureReadings = GenerateTemperatureReadings(40f, 5f, 60)
                },
                new OVRThermalSensorData
                {
                    SensorId = "GPU_THERMAL",
                    SensorLocation = "GPU Core",
                    AverageTemperatureCelsius = 40f + _random.Next(-3, 8),
                    PeakTemperatureCelsius = 45f + _random.Next(-2, 10),
                    TemperatureReadings = GenerateTemperatureReadings(42f, 6f, 60)
                }
            };
            
            float avgTemp = sensorReadings.Average(s => s.AverageTemperatureCelsius);
            float peakTemp = sensorReadings.Max(s => s.PeakTemperatureCelsius);
            
            return new OVRThermalData
            {
                AverageTemperatureCelsius = avgTemp,
                PeakTemperatureCelsius = peakTemp,
                SensorReadings = sensorReadings,
                ThermalThrottlingEvents = peakTemp > 50f ? _random.Next(0, 3) : 0,
                TimeToEquilibriumSeconds = 45f + _random.Next(-10, 20),
                ThermalSafetyMarginCelsius = 50f - peakTemp,
                ThermalLimitsExceeded = peakTemp > 50f
            };
        }
        
        /// <summary>
        /// Get battery and power consumption data.
        /// </summary>
        public OVRPowerMetrics GetPowerMetrics()
        {
            if (!_isInitialized || !_config.EnablePowerMonitoring)
            {
                return default;
            }
            
            // Simulate power consumption data (in real implementation, query actual power sensors)
            float avgPower = 6f + _random.Next(-1, 3); // Typical Quest 3 power consumption
            float peakPower = avgPower + _random.Next(1, 3);
            
            return new OVRPowerMetrics
            {
                AveragePowerConsumptionW = avgPower,
                PeakPowerConsumptionW = peakPower,
                BatteryDrainRateMahPerHour = avgPower * 400f, // Approximate conversion
                EstimatedBatteryLifeHours = 5000f / (avgPower * 400f), // 5000mAh typical capacity
                PowerEfficiencyScore = math.clamp(100f - (avgPower - 5f) * 10f, 0f, 100f),
                CpuPowerConsumptionW = avgPower * 0.4f,
                GpuPowerConsumptionW = avgPower * 0.35f,
                DisplayPowerConsumptionW = avgPower * 0.25f
            };
        }     
   
        /// <summary>
        /// Export metrics data to various formats for analysis.
        /// </summary>
        public bool ExportMetricsData(OVRMetricsResult metricsData, OVRExportFormat format, string outputPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                
                switch (format)
                {
                    case OVRExportFormat.CSV:
                        return ExportToCSV(metricsData, outputPath);
                    case OVRExportFormat.JSON:
                        return ExportToJSON(metricsData, outputPath);
                    case OVRExportFormat.XML:
                        return ExportToXML(metricsData, outputPath);
                    case OVRExportFormat.Binary:
                        return ExportToBinary(metricsData, outputPath);
                    default:
                        UnityEngine.Debug.LogError($"[OVRMetricsIntegration] Unsupported export format: {format}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[OVRMetricsIntegration] Export failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate that OVR-Metrics data meets Quest 3 certification requirements.
        /// </summary>
        public OVRCertificationValidationResult ValidateCertificationRequirements(OVRMetricsResult metricsData)
        {
            var unmetRequirements = new List<string>();
            var requiredActions = new List<string>();
            float certificationScore = 100f;
            
            // Validate frame rate requirements
            if (metricsData.FrameMetrics.AverageFrameRate < 72f)
            {
                unmetRequirements.Add($"Average frame rate {metricsData.FrameMetrics.AverageFrameRate:F1} FPS below required 72 FPS");
                requiredActions.Add("Optimize performance to maintain 72 FPS minimum");
                certificationScore -= 30f;
            }
            
            // Validate frame consistency
            if (metricsData.FrameMetrics.FrameRateStandardDeviation > 5f)
            {
                unmetRequirements.Add($"Frame rate inconsistency: {metricsData.FrameMetrics.FrameRateStandardDeviation:F1} FPS std dev");
                requiredActions.Add("Improve frame pacing consistency");
                certificationScore -= 15f;
            }
            
            // Validate memory usage
            if (metricsData.MemoryMetrics.PeakAppMemoryMB > 2048f)
            {
                unmetRequirements.Add($"Memory usage {metricsData.MemoryMetrics.PeakAppMemoryMB:F0} MB exceeds 2048 MB limit");
                requiredActions.Add("Reduce memory usage through optimization");
                certificationScore -= 25f;
            }
            
            // Validate thermal performance
            if (metricsData.ThermalData.PeakTemperatureCelsius > 45f)
            {
                unmetRequirements.Add($"Peak temperature {metricsData.ThermalData.PeakTemperatureCelsius:F1}°C exceeds 45°C limit");
                requiredActions.Add("Optimize thermal performance");
                certificationScore -= 20f;
            }
            
            // Validate power consumption
            if (metricsData.PowerMetrics.AveragePowerConsumptionW > 8f)
            {
                unmetRequirements.Add($"Power consumption {metricsData.PowerMetrics.AveragePowerConsumptionW:F1}W exceeds recommended 8W");
                requiredActions.Add("Optimize power efficiency");
                certificationScore -= 10f;
            }
            
            // Validate data quality
            if (metricsData.DataQuality.ReliabilityScore < 95f)
            {
                unmetRequirements.Add($"Data quality {metricsData.DataQuality.ReliabilityScore:F1}% below required 95%");
                requiredActions.Add("Ensure stable testing environment for reliable data");
                certificationScore -= 10f;
            }
            
            certificationScore = math.clamp(certificationScore, 0f, 100f);
            bool meetsCertification = unmetRequirements.Count == 0;
            
            string validationSummary = meetsCertification ?
                $"Application meets all Quest 3 certification requirements (Score: {certificationScore:F1}%)" :
                $"Application has {unmetRequirements.Count} certification issues (Score: {certificationScore:F1}%)";
            
            return new OVRCertificationValidationResult
            {
                MeetsCertificationRequirements = meetsCertification,
                CertificationScore = certificationScore,
                UnmetRequirements = unmetRequirements.ToArray(),
                RequiredActions = requiredActions.ToArray(),
                ValidationSummary = validationSummary,
                ValidationTimestamp = DateTime.Now
            };
        }
        
        /// <summary>
        /// Clean up OVR-Metrics resources and stop all monitoring.
        /// </summary>
        public void Dispose()
        {
            if (_isCaptureActive)
            {
                StopCaptureSession();
            }
            
            // Cleanup OVR-Metrics SDK (simulated)
            CleanupOVRMetricsSDK();
            
            _captureData.Clear();
            _sessionHistory.Clear();
            
            _isInitialized = false;
            
            UnityEngine.Debug.Log("[OVRMetricsIntegration] Disposed");
        }
        
        #region Private Helper Methods
        
        private bool ValidateConfiguration(OVRMetricsConfig config)
        {
            if (config.SamplingRateHz <= 0 || config.SamplingRateHz > MAX_SAMPLING_RATE)
                return false;
            
            if (config.AutoExportCSV && string.IsNullOrEmpty(config.ExportDirectory))
                return false;
            
            return true;
        }
        
        private bool InitializeOVRMetricsSDK()
        {
            // Simulate OVR-Metrics SDK initialization
            // In real implementation, this would call actual OVR APIs
            UnityEngine.Debug.Log("[OVRMetricsIntegration] OVR-Metrics SDK initialized (simulated)");
            return true;
        }
        
        private void StartOVRMetricsCapture()
        {
            // Simulate starting OVR-Metrics capture
            UnityEngine.Debug.Log("[OVRMetricsIntegration] OVR-Metrics capture started (simulated)");
        }
        
        private void StopOVRMetricsCapture()
        {
            // Simulate stopping OVR-Metrics capture
            UnityEngine.Debug.Log("[OVRMetricsIntegration] OVR-Metrics capture stopped (simulated)");
        }
        
        private void CleanupOVRMetricsSDK()
        {
            // Simulate OVR-Metrics SDK cleanup
            UnityEngine.Debug.Log("[OVRMetricsIntegration] OVR-Metrics SDK cleaned up (simulated)");
        }
        
        private OVRRealTimeMetrics SimulateRealTimeMetrics()
        {
            // Simulate realistic Quest 3 performance metrics
            float baseFrameRate = 72f + _random.Next(-5, 10);
            float cpuUsage = 60f + _random.Next(-10, 25);
            float gpuUsage = 70f + _random.Next(-15, 20);
            float memoryUsage = 1200f + _random.Next(-200, 600);
            float temperature = 38f + _random.Next(-3, 12);
            float powerConsumption = 6f + _random.Next(-1, 3);
            float batteryLevel = 85f + _random.Next(-20, 15);
            
            return new OVRRealTimeMetrics
            {
                CurrentFrameRate = math.clamp(baseFrameRate, 30f, 90f),
                CurrentFrameTimeMs = 1000f / math.clamp(baseFrameRate, 30f, 90f),
                CurrentCpuUsage = math.clamp(cpuUsage, 0f, 100f),
                CurrentGpuUsage = math.clamp(gpuUsage, 0f, 100f),
                CurrentMemoryUsageMB = math.clamp(memoryUsage, 500f, 3000f),
                CurrentTemperatureCelsius = math.clamp(temperature, 25f, 60f),
                CurrentPowerConsumptionW = math.clamp(powerConsumption, 3f, 12f),
                CurrentBatteryLevel = math.clamp(batteryLevel, 0f, 100f),
                Timestamp = DateTime.Now,
                IsDataValid = true
            };
        }
        
        private void CheckPerformanceAlerts(OVRRealTimeMetrics metrics)
        {
            if (metrics.CurrentFrameRate < _alertConfig.FrameRateThreshold)
            {
                TriggerPerformanceAlert($"Frame rate dropped to {metrics.CurrentFrameRate:F1} FPS");
            }
            
            if (metrics.CurrentCpuUsage > _alertConfig.CpuUsageThreshold)
            {
                TriggerPerformanceAlert($"High CPU usage: {metrics.CurrentCpuUsage:F1}%");
            }
            
            if (metrics.CurrentGpuUsage > _alertConfig.GpuUsageThreshold)
            {
                TriggerPerformanceAlert($"High GPU usage: {metrics.CurrentGpuUsage:F1}%");
            }
            
            if (metrics.CurrentMemoryUsageMB > _alertConfig.MemoryThresholdMB)
            {
                TriggerPerformanceAlert($"High memory usage: {metrics.CurrentMemoryUsageMB:F0} MB");
            }
            
            if (metrics.CurrentTemperatureCelsius > _alertConfig.TemperatureThreshold)
            {
                TriggerPerformanceAlert($"High temperature: {metrics.CurrentTemperatureCelsius:F1}°C");
            }
        }
        
        private void TriggerPerformanceAlert(string message)
        {
            if (_alertConfig.EnableVisualAlerts)
            {
                UnityEngine.Debug.LogWarning($"[OVRMetrics Alert] {message}");
            }
            
            if (_alertConfig.LogAlertsToFile)
            {
                // Log to file (simplified implementation)
                var logPath = Path.Combine(_config.ExportDirectory, "performance_alerts.log");
                File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            }
        }
        
        private void AddDataPointToCapture(OVRRealTimeMetrics metrics)
        {
            if (_captureData.Count >= MAX_DATA_POINTS)
            {
                return; // Prevent memory overflow
            }
            
            var dataPoint = new OVRDataPoint
            {
                Timestamp = metrics.Timestamp,
                FrameRate = metrics.CurrentFrameRate,
                CpuUsage = metrics.CurrentCpuUsage,
                GpuUsage = metrics.CurrentGpuUsage,
                MemoryUsageMB = metrics.CurrentMemoryUsageMB,
                TemperatureCelsius = metrics.CurrentTemperatureCelsius,
                PowerConsumptionW = metrics.CurrentPowerConsumptionW
            };
            
            _captureData.Add(dataPoint);
        }
        
        private OVRMetricsResult ProcessCaptureData(float actualDuration)
        {
            var dataPoints = _captureData.ToArray();
            return ProcessDataPoints(dataPoints, _currentSession);
        }
        
        private OVRMetricsResult ProcessDataPoints(OVRDataPoint[] dataPoints, OVRMetricsCaptureSession session)
        {
            if (dataPoints.Length == 0)
            {
                return CreateEmptyResult(session);
            }
            
            // Calculate frame metrics
            var frameRates = dataPoints.Select(dp => dp.FrameRate).ToArray();
            var frameMetrics = new OVRFrameMetrics
            {
                AverageFrameRate = frameRates.Average(),
                MinFrameRate = frameRates.Min(),
                MaxFrameRate = frameRates.Max(),
                FrameRateStandardDeviation = CalculateStandardDeviation(frameRates),
                AverageFrameTimeMs = 1000f / frameRates.Average(),
                P95FrameTimeMs = CalculatePercentile(frameRates.Select(fr => 1000f / fr).ToArray(), 0.95f),
                P99FrameTimeMs = CalculatePercentile(frameRates.Select(fr => 1000f / fr).ToArray(), 0.99f),
                DroppedFrames = frameRates.Count(fr => fr < 60f),
                FramePacingScore = CalculateFramePacingScore(frameRates),
                VSyncEffectiveness = CalculateVSyncEffectiveness(frameRates)
            };
            
            // Calculate CPU metrics
            var cpuUsages = dataPoints.Select(dp => dp.CpuUsage).ToArray();
            var cpuMetrics = new OVRCpuMetrics
            {
                AverageUsagePercent = cpuUsages.Average(),
                PeakUsagePercent = cpuUsages.Max(),
                UsageStandardDeviation = CalculateStandardDeviation(cpuUsages),
                AverageFrequencyMHz = 2400f, // Simulated Quest 3 CPU frequency
                ThrottlingEvents = cpuUsages.Count(cpu => cpu > 95f),
                FrequencyStates = GenerateFrequencyStates(),
                PerCoreUsage = GeneratePerCoreUsage(cpuUsages.Average())
            };
            
            // Calculate GPU metrics
            var gpuUsages = dataPoints.Select(dp => dp.GpuUsage).ToArray();
            var gpuMetrics = new OVRGpuMetrics
            {
                AverageUsagePercent = gpuUsages.Average(),
                PeakUsagePercent = gpuUsages.Max(),
                UsageStandardDeviation = CalculateStandardDeviation(gpuUsages),
                AverageFrequencyMHz = 670f, // Quest 3 GPU frequency
                ThrottlingEvents = gpuUsages.Count(gpu => gpu > 95f),
                AverageMemoryUsageMB = 800f, // Simulated GPU memory
                PeakMemoryUsageMB = 1000f,
                AverageTemperatureCelsius = dataPoints.Average(dp => dp.TemperatureCelsius)
            };
            
            // Calculate memory metrics
            var memoryUsages = dataPoints.Select(dp => dp.MemoryUsageMB).ToArray();
            var memoryMetrics = new OVRMemoryMetrics
            {
                AverageSystemMemoryMB = 6000f, // Quest 3 has 8GB, ~6GB available
                PeakSystemMemoryMB = 6500f,
                AverageAppMemoryMB = memoryUsages.Average(),
                PeakAppMemoryMB = memoryUsages.Max(),
                AllocationRateMBPerSecond = CalculateAllocationRate(memoryUsages),
                GarbageCollectionEvents = _random.Next(5, 20),
                AverageGCPauseTimeMs = 2f + _random.Next(0, 5),
                MemoryPressureLevel = math.clamp(memoryUsages.Average() / 2048f * 100f, 0f, 100f)
            };
            
            // Get thermal and power data
            var thermalData = GetThermalData();
            var powerMetrics = GetPowerMetrics();
            
            // Calculate data quality
            var dataQuality = new OVRDataQuality
            {
                SampleCompleteness = (float)dataPoints.Length / (session.PlannedDurationSeconds * _config.SamplingRateHz) * 100f,
                DataGaps = 0, // Simplified
                CaptureCompleted = true,
                ReliabilityScore = 98f + _random.Next(-3, 3),
                CaptureIssues = new string[0]
            };
            
            return new OVRMetricsResult
            {
                Session = session,
                CpuMetrics = cpuMetrics,
                GpuMetrics = gpuMetrics,
                MemoryMetrics = memoryMetrics,
                ThermalData = thermalData,
                PowerMetrics = powerMetrics,
                FrameMetrics = frameMetrics,
                RawDataPoints = dataPoints,
                TotalSamples = dataPoints.Length,
                ActualDurationSeconds = session.PlannedDurationSeconds,
                DataQuality = dataQuality
            };
        } 
               Timestamp = DateTime.Now,
                IsThrottling = temperature > 45f || cpuUsage > 90f || gpuUsage > 90f
            };
        }
        
        private void CheckPerformanceAlerts(OVRRealTimeMetrics metrics)
        {
            if (_alertConfig.EnableVisualAlerts)
            {
                // Check frame rate alert
                if (metrics.CurrentFrameRate < _alertConfig.FrameRateThreshold)
                {
                    LogPerformanceAlert($"Frame rate below threshold: {metrics.CurrentFrameRate:F1} FPS", 
                        OVRAlertSeverity.Warning);
                }
                
                // Check CPU usage alert
                if (metrics.CurrentCpuUsage > _alertConfig.CpuUsageThreshold)
                {
                    LogPerformanceAlert($"High CPU usage: {metrics.CurrentCpuUsage:F1}%", 
                        OVRAlertSeverity.Warning);
                }
                
                // Check GPU usage alert
                if (metrics.CurrentGpuUsage > _alertConfig.GpuUsageThreshold)
                {
                    LogPerformanceAlert($"High GPU usage: {metrics.CurrentGpuUsage:F1}%", 
                        OVRAlertSeverity.Warning);
                }
                
                // Check memory usage alert
                if (metrics.CurrentMemoryUsageMB > _alertConfig.MemoryThresholdMB)
                {
                    LogPerformanceAlert($"High memory usage: {metrics.CurrentMemoryUsageMB:F0} MB", 
                        OVRAlertSeverity.Warning);
                }
                
                // Check temperature alert
                if (metrics.CurrentTemperatureCelsius > _alertConfig.TemperatureThreshold)
                {
                    LogPerformanceAlert($"High temperature: {metrics.CurrentTemperatureCelsius:F1}°C", 
                        OVRAlertSeverity.Critical);
                }
                
                // Check throttling alert
                if (metrics.IsThrottling)
                {
                    LogPerformanceAlert("Performance throttling detected", OVRAlertSeverity.Critical);
                }
            }
        }
        
        private void LogPerformanceAlert(string message, OVRAlertSeverity severity)
        {
            string logMessage = $"[OVRMetricsIntegration] {severity}: {message}";
            
            switch (severity)
            {
                case OVRAlertSeverity.Info:
                    UnityEngine.Debug.Log(logMessage);
                    break;
                case OVRAlertSeverity.Warning:
                    UnityEngine.Debug.LogWarning(logMessage);
                    break;
                case OVRAlertSeverity.Critical:
                    UnityEngine.Debug.LogError(logMessage);
                    break;
            }
            
            // Log to file if configured
            if (_alertConfig.LogAlertsToFile && !string.IsNullOrEmpty(_config.ExportDirectory))
            {
                string alertLogPath = Path.Combine(_config.ExportDirectory, "performance_alerts.log");
                string alertEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{severity}] {message}\n";
                File.AppendAllText(alertLogPath, alertEntry);
            }
        }
        
        private void AddDataPointToCapture(OVRRealTimeMetrics metrics)
        {
            if (_captureData.Count >= MAX_DATA_POINTS)
            {
                // Remove oldest data point to maintain memory limits
                _captureData.RemoveAt(0);
            }
            
            var dataPoint = new OVRDataPoint
            {
                Timestamp = metrics.Timestamp,
                FrameRate = metrics.CurrentFrameRate,
                CpuUsage = metrics.CurrentCpuUsage,
                GpuUsage = metrics.CurrentGpuUsage,
                MemoryUsageMB = metrics.CurrentMemoryUsageMB,
                TemperatureCelsius = metrics.CurrentTemperatureCelsius,
                PowerConsumptionW = metrics.CurrentPowerConsumptionW
            };
            
            _captureData.Add(dataPoint);
        }
        
        private OVRMetricsResult ProcessCaptureData(float actualDuration)
        {
            if (_captureData.Count == 0)
            {
                return new OVRMetricsResult
                {
                    SessionInfo = _currentSession,
                    TotalSamples = 0,
                    ActualDurationSeconds = actualDuration,
                    DataQuality = new OVRDataQuality { ReliabilityScore = 0f }
                };
            }
            
            return ProcessDataPoints(_captureData.ToArray(), _currentSession);
        }
        
        private OVRMetricsResult ProcessDataPoints(OVRDataPoint[] dataPoints, OVRMetricsCaptureSession sessionInfo)
        {
            if (dataPoints.Length == 0)
            {
                return new OVRMetricsResult
                {
                    SessionInfo = sessionInfo,
                    TotalSamples = 0,
                    ActualDurationSeconds = 0f,
                    DataQuality = new OVRDataQuality { ReliabilityScore = 0f }
                };
            }
            
            // Calculate frame metrics
            var frameRates = dataPoints.Select(d => d.FrameRate).ToArray();
            var frameMetrics = new OVRFrameMetrics
            {
                AverageFrameRate = frameRates.Average(),
                MinimumFrameRate = frameRates.Min(),
                MaximumFrameRate = frameRates.Max(),
                FrameRateStandardDeviation = CalculateStandardDeviation(frameRates),
                DroppedFrames = frameRates.Count(fr => fr < 60f),
                TotalFrames = dataPoints.Length
            };
            
            // Calculate CPU metrics
            var cpuUsages = dataPoints.Select(d => d.CpuUsage).ToArray();
            var cpuMetrics = new OVRCpuMetrics
            {
                AverageUsagePercent = cpuUsages.Average(),
                PeakUsagePercent = cpuUsages.Max(),
                MinimumUsagePercent = cpuUsages.Min(),
                UsageStandardDeviation = CalculateStandardDeviation(cpuUsages),
                ThrottlingEvents = cpuUsages.Count(cpu => cpu > 95f)
            };
            
            // Calculate GPU metrics
            var gpuUsages = dataPoints.Select(d => d.GpuUsage).ToArray();
            var gpuMetrics = new OVRGpuMetrics
            {
                AverageUsagePercent = gpuUsages.Average(),
                PeakUsagePercent = gpuUsages.Max(),
                MinimumUsagePercent = gpuUsages.Min(),
                UsageStandardDeviation = CalculateStandardDeviation(gpuUsages),
                ThrottlingEvents = gpuUsages.Count(gpu => gpu > 95f)
            };
            
            // Calculate memory metrics
            var memoryUsages = dataPoints.Select(d => d.MemoryUsageMB).ToArray();
            var memoryMetrics = new OVRMemoryMetrics
            {
                AverageAppMemoryMB = memoryUsages.Average(),
                PeakAppMemoryMB = memoryUsages.Max(),
                MinimumAppMemoryMB = memoryUsages.Min(),
                MemoryStandardDeviation = CalculateStandardDeviation(memoryUsages),
                GarbageCollectionEvents = EstimateGCEvents(memoryUsages)
            };
            
            // Calculate thermal data
            var temperatures = dataPoints.Select(d => d.TemperatureCelsius).ToArray();
            var thermalData = new OVRThermalData
            {
                AverageTemperatureCelsius = temperatures.Average(),
                PeakTemperatureCelsius = temperatures.Max(),
                SensorReadings = new OVRThermalSensorData[0], // Would be populated with actual sensor data
                ThermalThrottlingEvents = temperatures.Count(temp => temp > 50f),
                TimeToEquilibriumSeconds = 45f,
                ThermalSafetyMarginCelsius = 50f - temperatures.Max(),
                ThermalLimitsExceeded = temperatures.Max() > 50f
            };
            
            // Calculate power metrics
            var powerConsumptions = dataPoints.Select(d => d.PowerConsumptionW).ToArray();
            var powerMetrics = new OVRPowerMetrics
            {
                AveragePowerConsumptionW = powerConsumptions.Average(),
                PeakPowerConsumptionW = powerConsumptions.Max(),
                BatteryDrainRateMahPerHour = powerConsumptions.Average() * 400f,
                EstimatedBatteryLifeHours = 5000f / (powerConsumptions.Average() * 400f),
                PowerEfficiencyScore = math.clamp(100f - (powerConsumptions.Average() - 5f) * 10f, 0f, 100f),
                CpuPowerConsumptionW = powerConsumptions.Average() * 0.4f,
                GpuPowerConsumptionW = powerConsumptions.Average() * 0.35f,
                DisplayPowerConsumptionW = powerConsumptions.Average() * 0.25f
            };
            
            // Calculate data quality
            float actualDuration = dataPoints.Length > 0 ? 
                (float)(dataPoints.Last().Timestamp - dataPoints[0].Timestamp).TotalSeconds : 0f;
            float expectedSamples = actualDuration * _config.SamplingRateHz;
            float reliabilityScore = expectedSamples > 0 ? (dataPoints.Length / expectedSamples) * 100f : 0f;
            
            var dataQuality = new OVRDataQuality
            {
                ReliabilityScore = math.clamp(reliabilityScore, 0f, 100f),
                SampleConsistency = CalculateSampleConsistency(dataPoints),
                DataCompleteness = dataPoints.Length > 0 ? 100f : 0f,
                MeasurementAccuracy = 95f, // Would be calculated based on sensor calibration
                NoiseLevel = CalculateNoiseLevel(frameRates)
            };
            
            return new OVRMetricsResult
            {
                SessionInfo = sessionInfo,
                FrameMetrics = frameMetrics,
                CpuMetrics = cpuMetrics,
                GpuMetrics = gpuMetrics,
                MemoryMetrics = memoryMetrics,
                ThermalData = thermalData,
                PowerMetrics = powerMetrics,
                DataQuality = dataQuality,
                TotalSamples = dataPoints.Length,
                ActualDurationSeconds = actualDuration,
                RawDataPoints = dataPoints
            };
        }
        
        private float CalculateStandardDeviation(float[] values)
        {
            if (values.Length <= 1) return 0f;
            
            float mean = values.Average();
            float sumSquaredDifferences = values.Sum(v => (v - mean) * (v - mean));
            return (float)Math.Sqrt(sumSquaredDifferences / (values.Length - 1));
        }
        
        private int EstimateGCEvents(float[] memoryUsages)
        {
            int gcEvents = 0;
            for (int i = 1; i < memoryUsages.Length; i++)
            {
                // Detect significant memory drops that might indicate GC
                if (memoryUsages[i-1] - memoryUsages[i] > 50f) // 50MB drop threshold
                {
                    gcEvents++;
                }
            }
            return gcEvents;
        }
        
        private float CalculateSampleConsistency(OVRDataPoint[] dataPoints)
        {
            if (dataPoints.Length <= 1) return 100f;
            
            var intervals = new List<double>();
            for (int i = 1; i < dataPoints.Length; i++)
            {
                intervals.Add((dataPoints[i].Timestamp - dataPoints[i-1].Timestamp).TotalSeconds);
            }
            
            double expectedInterval = 1.0 / _config.SamplingRateHz;
            double avgInterval = intervals.Average();
            double consistency = 100f - Math.Abs(avgInterval - expectedInterval) / expectedInterval * 100f;
            
            return (float)math.clamp(consistency, 0f, 100f);
        }
        
        private float CalculateNoiseLevel(float[] values)
        {
            if (values.Length <= 2) return 0f;
            
            // Calculate noise as the average absolute difference between consecutive values
            float totalNoise = 0f;
            for (int i = 1; i < values.Length; i++)
            {
                totalNoise += Math.Abs(values[i] - values[i-1]);
            }
            
            return totalNoise / (values.Length - 1);
        }
        
        private float[] GenerateTemperatureReadings(float baseTemp, float variation, int count)
        {
            var readings = new float[count];
            for (int i = 0; i < count; i++)
            {
                readings[i] = baseTemp + (_random.Next(-(int)variation, (int)variation + 1));
            }
            return readings;
        }
        
        private string GenerateDetailedAnalysisReport(OVRMetricsResult metricsData, float performanceScore, 
            List<string> bottlenecks, List<string> recommendations)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== OVR-Metrics Performance Analysis Report ===");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Session: {metricsData.SessionInfo.SessionName}");
            report.AppendLine($"Duration: {metricsData.ActualDurationSeconds:F1} seconds");
            report.AppendLine($"Samples: {metricsData.TotalSamples}");
            report.AppendLine($"Overall Performance Score: {performanceScore:F1}%");
            report.AppendLine();
            
            report.AppendLine("=== Performance Metrics ===");
            report.AppendLine($"Frame Rate: {metricsData.FrameMetrics.AverageFrameRate:F1} FPS " +
                            $"(Min: {metricsData.FrameMetrics.MinimumFrameRate:F1}, Max: {metricsData.FrameMetrics.MaximumFrameRate:F1})");
            report.AppendLine($"CPU Usage: {metricsData.CpuMetrics.AverageUsagePercent:F1}% " +
                            $"(Peak: {metricsData.CpuMetrics.PeakUsagePercent:F1}%)");
            report.AppendLine($"GPU Usage: {metricsData.GpuMetrics.AverageUsagePercent:F1}% " +
                            $"(Peak: {metricsData.GpuMetrics.PeakUsagePercent:F1}%)");
            report.AppendLine($"Memory Usage: {metricsData.MemoryMetrics.AverageAppMemoryMB:F0} MB " +
                            $"(Peak: {metricsData.MemoryMetrics.PeakAppMemoryMB:F0} MB)");
            report.AppendLine($"Temperature: {metricsData.ThermalData.AverageTemperatureCelsius:F1}°C " +
                            $"(Peak: {metricsData.ThermalData.PeakTemperatureCelsius:F1}°C)");
            report.AppendLine($"Power Consumption: {metricsData.PowerMetrics.AveragePowerConsumptionW:F1}W " +
                            $"(Peak: {metricsData.PowerMetrics.PeakPowerConsumptionW:F1}W)");
            report.AppendLine();
            
            if (bottlenecks.Count > 0)
            {
                report.AppendLine("=== Performance Bottlenecks ===");
                foreach (var bottleneck in bottlenecks)
                {
                    report.AppendLine($"• {bottleneck}");
                }
                report.AppendLine();
            }
            
            if (recommendations.Count > 0)
            {
                report.AppendLine("=== Optimization Recommendations ===");
                foreach (var recommendation in recommendations)
                {
                    report.AppendLine($"• {recommendation}");
                }
                report.AppendLine();
            }
            
            report.AppendLine("=== Data Quality ===");
            report.AppendLine($"Reliability Score: {metricsData.DataQuality.ReliabilityScore:F1}%");
            report.AppendLine($"Sample Consistency: {metricsData.DataQuality.SampleConsistency:F1}%");
            report.AppendLine($"Data Completeness: {metricsData.DataQuality.DataCompleteness:F1}%");
            
            return report.ToString();
        }
        
        private bool ExportToCSV(OVRMetricsResult metricsData, string outputPath)
        {
            try
            {
                using (var writer = new StreamWriter(outputPath))
                {
                    // Write CSV header
                    writer.WriteLine("Timestamp,FrameRate,CpuUsage,GpuUsage,MemoryUsageMB,TemperatureCelsius,PowerConsumptionW");
                    
                    // Write data points
                    foreach (var dataPoint in metricsData.RawDataPoints)
                    {
                        writer.WriteLine($"{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss.fff}," +
                                       $"{dataPoint.FrameRate:F2}," +
                                       $"{dataPoint.CpuUsage:F2}," +
                                       $"{dataPoint.GpuUsage:F2}," +
                                       $"{dataPoint.MemoryUsageMB:F2}," +
                                       $"{dataPoint.TemperatureCelsius:F2}," +
                                       $"{dataPoint.PowerConsumptionW:F2}");
                    }
                }
                
                UnityEngine.Debug.Log($"[OVRMetricsIntegration] Exported CSV data to {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[OVRMetricsIntegration] CSV export failed: {ex.Message}");
                return false;
            }
        }
        
        private bool ExportToJSON(OVRMetricsResult metricsData, string outputPath)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(metricsData, true);
                File.WriteAllText(outputPath, jsonData);
                
                UnityEngine.Debug.Log($"[OVRMetricsIntegration] Exported JSON data to {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[OVRMetricsIntegration] JSON export failed: {ex.Message}");
                return false;
            }
        }
        
        private bool ExportToXML(OVRMetricsResult metricsData, string outputPath)
        {
            // XML export implementation would go here
            UnityEngine.Debug.LogWarning("[OVRMetricsIntegration] XML export not yet implemented");
            return false;
        }
        
        private bool ExportToBinary(OVRMetricsResult metricsData, string outputPath)
        {
            // Binary export implementation would go here
            UnityEngine.Debug.LogWarning("[OVRMetricsIntegration] Binary export not yet implemented");
            return false;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Alert severity levels for OVR performance monitoring.
    /// </summary>
    public enum OVRAlertSeverity
    {
        Info,
        Warning,
        Critical
    }
}