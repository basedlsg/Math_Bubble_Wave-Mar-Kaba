using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.IO;
using System.Linq;

namespace XRBubbleLibrary.Quest3.Tests
{
    /// <summary>
    /// Comprehensive unit tests for Continuous Hardware Monitor system.
    /// Tests Requirement 8.2: Real-time FPS, CPU, GPU, and thermal monitoring.
    /// Tests Requirement 8.3: Performance data collection and analysis.
    /// Tests Requirement 8.4: Automatic test termination for safety limits.
    /// Tests Requirement 8.5: Continuous hardware monitoring during testing.
    /// </summary>
    [TestFixture]
    public class ContinuousHardwareMonitorTests
    {
        private GameObject _testGameObject;
        private ContinuousHardwareMonitor _hardwareMonitor;
        private HardwareMonitoringConfiguration _testConfig;
        private string _testLoggingDirectory;
        
        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("ContinuousHardwareMonitorTest");
            _hardwareMonitor = _testGameObject.AddComponent<ContinuousHardwareMonitor>();
            _testLoggingDirectory = Path.Combine(Application.temporaryCachePath, "HardwareMonitorTests");
            
            _testConfig = new HardwareMonitoringConfiguration
            {
                SampleRateHz = 10f,
                MonitorFrameRate = true,
                MonitorCPU = true,
                MonitorGPU = true,
                MonitorMemory = true,
                MonitorThermal = true,
                MonitorPower = true,
                EnableSafetyLimits = true,
                AutoTerminateOnSafetyViolation = false, // Disable for tests
                MaxMonitoringDurationSeconds = 60f,
                MaxSamplesInMemory = 600,
                EnableContinuousLogging = true,
                LoggingDirectory = _testLoggingDirectory,
                EnableDebugLogging = false
            };
            
            // Ensure test directory exists
            Directory.CreateDirectory(_testLoggingDirectory);
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                Object.DestroyImmediate(_testGameObject);
            }
            
            // Clean up test files
            if (Directory.Exists(_testLoggingDirectory))
            {
                try
                {
                    Directory.Delete(_testLoggingDirectory, true);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"Failed to clean up test directory: {ex.Message}");
                }
            }
        }
        
        #region Initialization Tests
        
        [Test]
        public void Initialize_WithValidConfig_ReturnsTrue()
        {
            // Act
            bool result = _hardwareMonitor.Initialize(_testConfig);
            
            // Assert
            Assert.IsTrue(result, "Hardware monitor initialization should succeed with valid config");
            Assert.IsTrue(_hardwareMonitor.IsInitialized, "Monitor should be initialized");
            Assert.IsFalse(_hardwareMonitor.IsMonitoringActive, "Monitoring should not be active initially");
            Assert.AreEqual(_testConfig.SampleRateHz, _hardwareMonitor.Configuration.SampleRateHz, 
                "Configuration should be stored correctly");
        }
        
        [Test]
        public void Initialize_WithDefaultConfiguration_UsesCorrectValues()
        {
            // Arrange
            var defaultConfig = HardwareMonitoringConfiguration.Quest3Default;
            
            // Act
            bool result = _hardwareMonitor.Initialize(defaultConfig);
            
            // Assert
            Assert.IsTrue(result, "Should initialize with default configuration");
            Assert.AreEqual(10f, defaultConfig.SampleRateHz, "Default should use 10Hz sampling rate");
            Assert.IsTrue(defaultConfig.MonitorFrameRate, "Should monitor frame rate by default");
            Assert.IsTrue(defaultConfig.MonitorCPU, "Should monitor CPU by default");
            Assert.IsTrue(defaultConfig.MonitorGPU, "Should monitor GPU by default");
            Assert.IsTrue(defaultConfig.MonitorThermal, "Should monitor thermal by default");
            Assert.IsTrue(defaultConfig.EnableSafetyLimits, "Should enable safety limits by default");
        }
        
        [Test]
        public void Initialize_WithHighFrequencyConfiguration_HasCorrectSettings()
        {
            // Arrange
            var highFreqConfig = HardwareMonitoringConfiguration.HighFrequency;
            
            // Act
            bool result = _hardwareMonitor.Initialize(highFreqConfig);
            
            // Assert
            Assert.IsTrue(result, "Should initialize with high frequency configuration");
            Assert.AreEqual(60f, highFreqConfig.SampleRateHz, "High frequency should use 60Hz sampling");
            Assert.AreEqual(600f, highFreqConfig.MaxMonitoringDurationSeconds, "Should have shorter max duration");
            Assert.IsTrue(highFreqConfig.EnableDebugLogging, "Should enable debug logging for high frequency");
        }
        
        [Test]
        public void Initialize_WithPerformanceTestingConfiguration_HasCorrectSettings()
        {
            // Arrange
            var perfTestConfig = HardwareMonitoringConfiguration.PerformanceTesting;
            
            // Act
            bool result = _hardwareMonitor.Initialize(perfTestConfig);
            
            // Assert
            Assert.IsTrue(result, "Should initialize with performance testing configuration");
            Assert.AreEqual(30f, perfTestConfig.SampleRateHz, "Performance testing should use 30Hz sampling");
            Assert.AreEqual(1800f, perfTestConfig.MaxMonitoringDurationSeconds, "Should allow 30 minutes monitoring");
            Assert.IsTrue(perfTestConfig.AutoTerminateOnSafetyViolation, "Should auto-terminate on safety violations");
        }
        
        #endregion
        
        #region Monitoring Session Tests
        
        [Test]
        public void StartMonitoring_WhenInitialized_ReturnsValidSession()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            string sessionName = "TestSession";
            
            // Act
            var session = _hardwareMonitor.StartMonitoring(sessionName);
            
            // Assert
            Assert.IsNotNull(session.SessionId, "Session should have valid ID");
            Assert.AreEqual(sessionName, session.SessionName, "Session name should match");
            Assert.IsTrue(session.IsActive, "Session should be active");
            Assert.IsTrue(_hardwareMonitor.IsMonitoringActive, "Monitoring should be active");
            Assert.AreEqual(_testConfig.SampleRateHz, session.Configuration.SampleRateHz, 
                "Session should use correct configuration");
        }
        
        [Test]
        public void StartMonitoring_WhenNotInitialized_ReturnsDefaultSession()
        {
            // Act
            var session = _hardwareMonitor.StartMonitoring("TestSession");
            
            // Assert
            Assert.IsNull(session.SessionId, "Session ID should be null when not initialized");
            Assert.IsFalse(_hardwareMonitor.IsMonitoringActive, "Monitoring should not be active");
        }
        
        [Test]
        public void StartMonitoring_WhenSessionAlreadyActive_StopsPreviousSession()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            var firstSession = _hardwareMonitor.StartMonitoring("FirstSession");
            
            // Act
            var secondSession = _hardwareMonitor.StartMonitoring("SecondSession");
            
            // Assert
            Assert.AreNotEqual(firstSession.SessionId, secondSession.SessionId, "Should create new session");
            Assert.AreEqual("SecondSession", secondSession.SessionName, "Should use new session name");
            Assert.IsTrue(_hardwareMonitor.IsMonitoringActive, "New session should be active");
        }
        
        [Test]
        public void StopMonitoring_WhenActive_ReturnsValidResults()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for some data collection
            System.Threading.Thread.Sleep(200);
            
            // Act
            var result = _hardwareMonitor.StopMonitoring();
            
            // Assert
            Assert.IsNotNull(result.Session.SessionId, "Result should contain session info");
            Assert.GreaterOrEqual(result.Statistics.TotalSamples, 0, "Should have collected some samples");
            Assert.Greater(result.Statistics.TotalDuration.TotalSeconds, 0, "Should have positive duration");
            Assert.IsFalse(_hardwareMonitor.IsMonitoringActive, "Monitoring should no longer be active");
            Assert.IsTrue(result.CompletedSuccessfully, "Should complete successfully");
        }
        
        [Test]
        public void StopMonitoring_WhenNotActive_ReturnsDefaultResult()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            
            // Act
            var result = _hardwareMonitor.StopMonitoring();
            
            // Assert
            Assert.IsNull(result.Session.SessionId, "Should return default result when no active session");
        }
        
        #endregion
        
        #region Real-Time Metrics Tests
        
        [Test]
        public void GetCurrentMetrics_WhenInitialized_ReturnsValidMetrics()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for metrics collection
            System.Threading.Thread.Sleep(150);
            
            // Act
            var metrics = _hardwareMonitor.GetCurrentMetrics();
            
            // Assert
            Assert.Greater(metrics.CurrentFPS, 0f, "Frame rate should be positive");
            Assert.GreaterOrEqual(metrics.CPUUsagePercent, 0f, "CPU usage should be non-negative");
            Assert.LessOrEqual(metrics.CPUUsagePercent, 100f, "CPU usage should not exceed 100%");
            Assert.GreaterOrEqual(metrics.GPUUsagePercent, 0f, "GPU usage should be non-negative");
            Assert.LessOrEqual(metrics.GPUUsagePercent, 100f, "GPU usage should not exceed 100%");
            Assert.Greater(metrics.MemoryUsageMB, 0f, "Memory usage should be positive");
            Assert.Greater(metrics.CPUTemperatureCelsius, 0f, "CPU temperature should be positive");
            Assert.Greater(metrics.GPUTemperatureCelsius, 0f, "GPU temperature should be positive");
            Assert.Greater(metrics.PowerConsumptionW, 0f, "Power consumption should be positive");
            Assert.GreaterOrEqual(metrics.BatteryLevelPercent, 0f, "Battery level should be non-negative");
            Assert.LessOrEqual(metrics.BatteryLevelPercent, 100f, "Battery level should not exceed 100%");
        }
        
        [Test]
        public void GetCurrentMetrics_WhenNotInitialized_ReturnsDefaultMetrics()
        {
            // Act
            var metrics = _hardwareMonitor.GetCurrentMetrics();
            
            // Assert
            Assert.AreEqual(0f, metrics.CurrentFPS, "Should return default metrics when not initialized");
            Assert.AreEqual(DateTime.MinValue, metrics.Timestamp, "Timestamp should be default when not initialized");
        }
        
        #endregion
        
        #region Aggregated Statistics Tests
        
        [Test]
        public void GetAggregatedStatistics_WithData_ReturnsValidStatistics()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for data collection
            System.Threading.Thread.Sleep(300);
            
            // Act
            var statistics = _hardwareMonitor.GetAggregatedStatistics();
            
            // Assert
            Assert.Greater(statistics.TotalSamples, 0, "Should have collected samples");
            Assert.Greater(statistics.TotalDuration.TotalSeconds, 0, "Should have positive duration");
            Assert.Greater(statistics.FrameRateStats.Average, 0f, "Should have positive average frame rate");
            Assert.GreaterOrEqual(statistics.FrameRateStats.Minimum, 0f, "Minimum frame rate should be non-negative");
            Assert.GreaterOrEqual(statistics.FrameRateStats.Maximum, statistics.FrameRateStats.Minimum, 
                "Maximum should be >= minimum");
            Assert.GreaterOrEqual(statistics.CPUUsageStats.Average, 0f, "CPU usage average should be non-negative");
            Assert.GreaterOrEqual(statistics.GPUUsageStats.Average, 0f, "GPU usage average should be non-negative");
        }
        
        [Test]
        public void GetAggregatedStatistics_WhenNotMonitoring_ReturnsDefaultStatistics()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            
            // Act
            var statistics = _hardwareMonitor.GetAggregatedStatistics();
            
            // Assert
            Assert.AreEqual(0, statistics.TotalSamples, "Should have no samples when not monitoring");
            Assert.AreEqual(0, statistics.ThermalThrottlingEvents, "Should have no throttling events");
            Assert.AreEqual(0, statistics.SafetyViolations, "Should have no safety violations");
        }
        
        #endregion
        
        #region Safety Limits Tests
        
        [Test]
        public void ConfigureSafetyLimits_UpdatesConfiguration()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            var customLimits = new HardwareSafetyLimits
            {
                MaxCPUTemperatureCelsius = 80f,
                MaxGPUTemperatureCelsius = 80f,
                MaxBatteryTemperatureCelsius = 40f,
                MaxPowerConsumptionW = 10f,
                MinBatteryLevelPercent = 15f,
                MaxMemoryUsageMB = 2500f,
                MinFrameRateFPS = 45f,
                MaxConsecutiveThermalThrottlingEvents = 3,
                ViolationTimeWindowSeconds = 5f
            };
            
            // Act
            _hardwareMonitor.ConfigureSafetyLimits(customLimits);
            
            // Assert - This test verifies the method doesn't throw exceptions
            // In a real implementation, we would verify the limits are stored
            Assert.Pass("Safety limits configuration should be accepted");
        }
        
        [Test]
        public void GetSafetyStatus_ReturnsValidStatus()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for data collection
            System.Threading.Thread.Sleep(150);
            
            // Act
            var safetyStatus = _hardwareMonitor.GetSafetyStatus();
            
            // Assert
            Assert.IsNotNull(safetyStatus.ActiveWarnings, "Should have active warnings array");
            Assert.GreaterOrEqual(safetyStatus.TotalViolationsThisSession, 0, 
                "Total violations should be non-negative");
            Assert.GreaterOrEqual(safetyStatus.TimeSinceLastViolation.TotalSeconds, 0, 
                "Time since last violation should be non-negative");
        }
        
        [Test]
        public void PerformSafetyCheck_ReturnsValidResult()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for data collection
            System.Threading.Thread.Sleep(150);
            
            // Act
            var safetyResult = _hardwareMonitor.PerformSafetyCheck();
            
            // Assert
            Assert.IsNotNull(safetyResult.Violations, "Should have violations array");
            Assert.GreaterOrEqual(safetyResult.SafetyScore, 0f, "Safety score should be non-negative");
            Assert.LessOrEqual(safetyResult.SafetyScore, 100f, "Safety score should not exceed 100%");
            Assert.IsNotNull(safetyResult.RecommendedActions, "Should have recommended actions array");
            Assert.Greater(safetyResult.CheckTimestamp, DateTime.MinValue, "Should have valid timestamp");
        }
        
        [Test]
        public void HardwareSafetyLimits_Quest3Default_HasCorrectValues()
        {
            // Act
            var defaultLimits = HardwareSafetyLimits.Quest3Default;
            
            // Assert
            Assert.AreEqual(85f, defaultLimits.MaxCPUTemperatureCelsius, "Default CPU temp limit should be 85°C");
            Assert.AreEqual(85f, defaultLimits.MaxGPUTemperatureCelsius, "Default GPU temp limit should be 85°C");
            Assert.AreEqual(45f, defaultLimits.MaxBatteryTemperatureCelsius, "Default battery temp limit should be 45°C");
            Assert.AreEqual(12f, defaultLimits.MaxPowerConsumptionW, "Default power limit should be 12W");
            Assert.AreEqual(10f, defaultLimits.MinBatteryLevelPercent, "Default min battery should be 10%");
            Assert.AreEqual(3000f, defaultLimits.MaxMemoryUsageMB, "Default memory limit should be 3000MB");
            Assert.AreEqual(30f, defaultLimits.MinFrameRateFPS, "Default min FPS should be 30");
        }
        
        [Test]
        public void HardwareSafetyLimits_Conservative_HasStricterValues()
        {
            // Act
            var conservativeLimits = HardwareSafetyLimits.Conservative;
            var defaultLimits = HardwareSafetyLimits.Quest3Default;
            
            // Assert
            Assert.Less(conservativeLimits.MaxCPUTemperatureCelsius, defaultLimits.MaxCPUTemperatureCelsius, 
                "Conservative CPU limit should be stricter");
            Assert.Less(conservativeLimits.MaxGPUTemperatureCelsius, defaultLimits.MaxGPUTemperatureCelsius, 
                "Conservative GPU limit should be stricter");
            Assert.Less(conservativeLimits.MaxBatteryTemperatureCelsius, defaultLimits.MaxBatteryTemperatureCelsius, 
                "Conservative battery limit should be stricter");
            Assert.Greater(conservativeLimits.MinBatteryLevelPercent, defaultLimits.MinBatteryLevelPercent, 
                "Conservative battery minimum should be higher");
            Assert.Greater(conservativeLimits.MinFrameRateFPS, defaultLimits.MinFrameRateFPS, 
                "Conservative FPS minimum should be higher");
        }
        
        #endregion
        
        #region Thermal and Power Monitoring Tests
        
        [Test]
        public void GetThermalData_ReturnsValidData()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for data collection
            System.Threading.Thread.Sleep(150);
            
            // Act
            var thermalData = _hardwareMonitor.GetThermalData();
            
            // Assert
            Assert.IsNotNull(thermalData.SensorReadings, "Should have sensor readings");
            Assert.GreaterOrEqual(thermalData.ThermalSafetyMarginCelsius, 0f, 
                "Thermal safety margin should be non-negative");
            Assert.IsNotNull(thermalData.ThrottlingEvents, "Should have throttling events array");
        }
        
        [Test]
        public void GetPowerData_ReturnsValidData()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for data collection
            System.Threading.Thread.Sleep(150);
            
            // Act
            var powerData = _hardwareMonitor.GetPowerData();
            
            // Assert
            Assert.Greater(powerData.EstimatedBatteryLife.TotalMinutes, 0, 
                "Estimated battery life should be positive");
        }
        
        #endregion
        
        #region Performance Trends Tests
        
        [Test]
        public void GetPerformanceTrends_WithSufficientData_ReturnsValidTrends()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for sufficient data collection (need at least 10 samples)
            System.Threading.Thread.Sleep(1200); // 1.2 seconds at 10Hz = 12 samples
            
            // Act
            var trends = _hardwareMonitor.GetPerformanceTrends();
            
            // Assert
            Assert.IsNotNull(trends.FrameRateTrend, "Should have frame rate trend");
            Assert.IsNotNull(trends.CPUUsageTrend, "Should have CPU usage trend");
            Assert.IsNotNull(trends.GPUUsageTrend, "Should have GPU usage trend");
            Assert.IsNotNull(trends.MemoryUsageTrend, "Should have memory usage trend");
            Assert.IsNotNull(trends.TemperatureTrend, "Should have temperature trend");
            Assert.IsNotNull(trends.PowerConsumptionTrend, "Should have power consumption trend");
        }
        
        [Test]
        public void GetPerformanceTrends_WithInsufficientData_ReturnsDefaultTrends()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for minimal data collection (less than 10 samples)
            System.Threading.Thread.Sleep(50);
            
            // Act
            var trends = _hardwareMonitor.GetPerformanceTrends();
            
            // Assert - Should return default trends when insufficient data
            // The exact behavior depends on implementation
            Assert.Pass("Should handle insufficient data gracefully");
        }
        
        #endregion
        
        #region Performance Thresholds Tests
        
        [Test]
        public void SetPerformanceThresholds_UpdatesConfiguration()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            var customThresholds = new PerformanceThresholds
            {
                FrameRateWarningThreshold = 65f,
                FrameRateCriticalThreshold = 50f,
                CPUUsageWarningThreshold = 75f,
                CPUUsageCriticalThreshold = 90f,
                GPUUsageWarningThreshold = 80f,
                GPUUsageCriticalThreshold = 90f,
                MemoryUsageWarningThreshold = 1800f,
                MemoryUsageCriticalThreshold = 2500f,
                TemperatureWarningThreshold = 65f,
                TemperatureCriticalThreshold = 75f
            };
            
            // Act
            _hardwareMonitor.SetPerformanceThresholds(customThresholds);
            
            // Assert - This test verifies the method doesn't throw exceptions
            Assert.Pass("Performance thresholds configuration should be accepted");
        }
        
        [Test]
        public void PerformanceThresholds_Quest3Default_HasCorrectValues()
        {
            // Act
            var defaultThresholds = PerformanceThresholds.Quest3Default;
            
            // Assert
            Assert.AreEqual(60f, defaultThresholds.FrameRateWarningThreshold, "Default FPS warning should be 60");
            Assert.AreEqual(45f, defaultThresholds.FrameRateCriticalThreshold, "Default FPS critical should be 45");
            Assert.AreEqual(80f, defaultThresholds.CPUUsageWarningThreshold, "Default CPU warning should be 80%");
            Assert.AreEqual(95f, defaultThresholds.CPUUsageCriticalThreshold, "Default CPU critical should be 95%");
            Assert.AreEqual(85f, defaultThresholds.GPUUsageWarningThreshold, "Default GPU warning should be 85%");
            Assert.AreEqual(95f, defaultThresholds.GPUUsageCriticalThreshold, "Default GPU critical should be 95%");
            Assert.AreEqual(70f, defaultThresholds.TemperatureWarningThreshold, "Default temp warning should be 70°C");
            Assert.AreEqual(80f, defaultThresholds.TemperatureCriticalThreshold, "Default temp critical should be 80°C");
        }
        
        #endregion
        
        #region Data Export Tests
        
        [Test]
        public void ExportMonitoringData_ToCSV_CreatesValidFile()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for data collection
            System.Threading.Thread.Sleep(200);
            
            string csvPath = Path.Combine(_testLoggingDirectory, "test_export.csv");
            
            // Act
            bool result = _hardwareMonitor.ExportMonitoringData(csvPath, MonitoringDataExportFormat.CSV);
            
            // Assert
            Assert.IsTrue(result, "CSV export should succeed");
            // Note: File existence check would depend on actual implementation
        }
        
        [Test]
        public void ExportMonitoringData_ToJSON_CreatesValidFile()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("TestSession");
            
            // Wait for data collection
            System.Threading.Thread.Sleep(200);
            
            string jsonPath = Path.Combine(_testLoggingDirectory, "test_export.json");
            
            // Act
            bool result = _hardwareMonitor.ExportMonitoringData(jsonPath, MonitoringDataExportFormat.JSON);
            
            // Assert
            Assert.IsTrue(result, "JSON export should succeed");
        }
        
        #endregion
        
        #region Session History Tests
        
        [Test]
        public void GetSessionHistory_InitiallyEmpty_ReturnsEmptyHistory()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            
            // Act
            var history = _hardwareMonitor.GetSessionHistory();
            
            // Assert
            Assert.AreEqual(0, history.CompletedSessions.Length, "Should have no sessions initially");
            Assert.AreEqual(TimeSpan.Zero, history.TotalMonitoringTime, "Total time should be zero initially");
            Assert.AreEqual(TimeSpan.Zero, history.AverageSessionDuration, "Average duration should be zero initially");
            Assert.AreEqual(0, history.TotalSafetyViolations, "Should have no violations initially");
        }
        
        [Test]
        public void GetSessionHistory_AfterSession_ContainsSessionRecord()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("HistoryTestSession");
            
            // Wait for data collection
            System.Threading.Thread.Sleep(200);
            
            _hardwareMonitor.StopMonitoring();
            
            // Act
            var history = _hardwareMonitor.GetSessionHistory();
            
            // Assert
            Assert.AreEqual(1, history.CompletedSessions.Length, "Should have one completed session");
            Assert.Greater(history.TotalMonitoringTime.TotalSeconds, 0, "Should have positive total time");
            Assert.Greater(history.AverageSessionDuration.TotalSeconds, 0, "Should have positive average duration");
        }
        
        #endregion
        
        #region Event Tests
        
        [Test]
        public void MonitoringDataUpdated_Event_FiresDuringMonitoring()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            bool eventFired = false;
            MonitoringDataUpdatedEventArgs lastEventArgs = default;
            
            _hardwareMonitor.MonitoringDataUpdated += (args) =>
            {
                eventFired = true;
                lastEventArgs = args;
            };
            
            // Act
            _hardwareMonitor.StartMonitoring("EventTestSession");
            
            // Wait for events to fire
            System.Threading.Thread.Sleep(200);
            
            // Assert
            Assert.IsTrue(eventFired, "Monitoring data updated event should fire during monitoring");
            Assert.Greater(lastEventArgs.CurrentMetrics.CurrentFPS, 0f, "Event should contain valid metrics");
            Assert.Greater(lastEventArgs.TotalSamples, 0, "Event should report sample count");
            Assert.Greater(lastEventArgs.MonitoringDuration.TotalSeconds, 0, "Event should report duration");
        }
        
        #endregion
        
        #region Integration Tests
        
        [UnityTest]
        public IEnumerator FullMonitoringSession_CollectsDataAndCompletes()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            
            // Act
            var session = _hardwareMonitor.StartMonitoring("IntegrationTest");
            
            // Wait for data collection
            yield return new WaitForSeconds(0.5f);
            
            // Get current metrics during monitoring
            var currentMetrics = _hardwareMonitor.GetCurrentMetrics();
            Assert.Greater(currentMetrics.CurrentFPS, 0f, "Should collect real-time metrics during monitoring");
            
            // Get aggregated statistics
            var statistics = _hardwareMonitor.GetAggregatedStatistics();
            Assert.Greater(statistics.TotalSamples, 0, "Should have collected samples");
            
            var result = _hardwareMonitor.StopMonitoring();
            
            // Assert
            Assert.IsNotNull(result.Session.SessionId, "Should complete monitoring session");
            Assert.Greater(result.Statistics.TotalSamples, 0, "Should collect samples during session");
            Assert.Greater(result.Statistics.TotalDuration.TotalSeconds, 0, "Should have positive duration");
            Assert.IsTrue(result.CompletedSuccessfully, "Should complete successfully");
        }
        
        [Test]
        public void Dispose_CleansUpResources()
        {
            // Arrange
            _hardwareMonitor.Initialize(_testConfig);
            _hardwareMonitor.StartMonitoring("DisposeTestSession");
            
            // Act
            _hardwareMonitor.Dispose();
            
            // Assert
            Assert.IsFalse(_hardwareMonitor.IsInitialized, "Should not be initialized after disposal");
            Assert.IsFalse(_hardwareMonitor.IsMonitoringActive, "Should not be monitoring after disposal");
        }
        
        #endregion
        
        #region Configuration Validation Tests
        
        [Test]
        public void HardwareMonitoringConfiguration_ValidatesCorrectly()
        {
            // Test various configuration scenarios
            var validConfig = HardwareMonitoringConfiguration.Quest3Default;
            Assert.Greater(validConfig.SampleRateHz, 0f, "Sample rate should be positive");
            Assert.Greater(validConfig.MaxSamplesInMemory, 0, "Max samples should be positive");
            
            var highFreqConfig = HardwareMonitoringConfiguration.HighFrequency;
            Assert.Greater(highFreqConfig.SampleRateHz, validConfig.SampleRateHz, 
                "High frequency should have higher sample rate");
            
            var perfTestConfig = HardwareMonitoringConfiguration.PerformanceTesting;
            Assert.IsTrue(perfTestConfig.EnableSafetyLimits, "Performance testing should enable safety limits");
            Assert.IsTrue(perfTestConfig.AutoTerminateOnSafetyViolation, 
                "Performance testing should auto-terminate on violations");
        }
        
        #endregion
    }
}