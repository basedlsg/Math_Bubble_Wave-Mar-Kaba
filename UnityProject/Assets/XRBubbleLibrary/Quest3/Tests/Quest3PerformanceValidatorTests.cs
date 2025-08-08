using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Linq;

namespace XRBubbleLibrary.Quest3.Tests
{
    /// <summary>
    /// Comprehensive tests for Quest 3 Performance Validator.
    /// Validates hardware-specific performance testing and validation capabilities.
    /// Implements Requirement 8.1: Quest 3 hardware performance validation.
    /// Implements Requirement 8.2: Real-time performance monitoring.
    /// Implements Requirement 8.3: Performance data collection and analysis.
    /// </summary>
    public class Quest3PerformanceValidatorTests
    {
        private IQuest3PerformanceValidator _validator;
        private Quest3PerformanceConfig _testConfig;
        
        [SetUp]
        public void SetUp()
        {
            _validator = new Quest3PerformanceValidator();
            _testConfig = Quest3PerformanceConfig.Default;
        }
        
        [TearDown]
        public void TearDown()
        {
            _validator?.Dispose();
            _validator = null;
        }
        
        #region Initialization Tests
        
        [Test]
        public void Initialize_WithValidConfig_InitializesCorrectly()
        {
            // Act
            bool result = _validator.Initialize(_testConfig);
            
            // Assert
            Assert.IsTrue(result, "Initialization should succeed with valid config");
            Assert.IsTrue(_validator.IsInitialized, "Validator should be initialized");
            Assert.IsFalse(_validator.IsValidationRunning, "Should not be running initially");
            Assert.AreEqual(_testConfig.TargetFrameRate, _validator.Configuration.TargetFrameRate, 
                "Should store configuration correctly");
            
            UnityEngine.Debug.Log($"[Quest3Validator] Initialized with {_testConfig.TargetFrameRate} FPS target");
        }
        
        [Test]
        public void Initialize_WithInvalidConfig_ReturnsFalse()
        {
            // Arrange - Invalid configuration
            var invalidConfig = _testConfig;
            invalidConfig.TargetFrameRate = -1f; // Invalid frame rate
            
            // Act
            bool result = _validator.Initialize(invalidConfig);
            
            // Assert
            Assert.IsFalse(result, "Initialization should fail with invalid config");
            Assert.IsFalse(_validator.IsInitialized, "Validator should not be initialized");
        }
        
        [Test]
        public void Initialize_WithHighPerformanceConfig_ConfiguresCorrectly()
        {
            // Arrange
            var highPerfConfig = Quest3PerformanceConfig.HighPerformance;
            
            // Act
            bool result = _validator.Initialize(highPerfConfig);
            
            // Assert
            Assert.IsTrue(result, "Should initialize with high performance config");
            Assert.AreEqual(90f, _validator.Configuration.TargetFrameRate, "Should set 90 FPS target");
            Assert.AreEqual(11.11f, _validator.Configuration.MaxFrameTimeMs, 0.01f, "Should set correct frame time");
        }
        
        #endregion
        
        #region Validation Session Tests
        
        [Test]
        public void StartValidationSession_WhenInitialized_StartsCorrectly()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            const float testDuration = 30f;
            
            // Act
            var session = _validator.StartValidationSession(testDuration);
            
            // Assert
            Assert.IsTrue(_validator.IsValidationRunning, "Should be running after start");
            Assert.IsNotNull(session.SessionId, "Should have session ID");
            Assert.AreEqual(testDuration, session.PlannedDurationSeconds, "Should set correct duration");
            Assert.IsTrue(session.IsActive, "Session should be active");
            Assert.AreEqual(_testConfig.TargetFrameRate, session.Configuration.TargetFrameRate, 
                "Should use correct configuration");
            
            UnityEngine.Debug.Log($"[Quest3Validator] Started session {session.SessionId} for {testDuration}s");
        }
        
        [Test]
        public void StartValidationSession_WhenNotInitialized_ReturnsDefault()
        {
            // Act
            var session = _validator.StartValidationSession();
            
            // Assert
            Assert.IsFalse(_validator.IsValidationRunning, "Should not be running if not initialized");
            Assert.IsNull(session.SessionId, "Should return default session");
        }
        
        [Test]
        public void StopValidationSession_WithActiveSession_ReturnsResults()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            var session = _validator.StartValidationSession(10f);
            
            // Let some time pass
            System.Threading.Thread.Sleep(100);
            
            // Act
            var result = _validator.StopValidationSession();
            
            // Assert
            Assert.IsFalse(_validator.IsValidationRunning, "Should not be running after stop");
            Assert.IsNotNull(result.ValidationReport, "Should provide validation report");
            Assert.AreEqual(session.SessionId, result.Session.SessionId, "Should match session ID");
            Assert.Greater(result.ActualDurationSeconds, 0f, "Should record actual duration");
            Assert.GreaterOrEqual(result.PerformanceScore, 0f, "Should have non-negative score");
            Assert.LessOrEqual(result.PerformanceScore, 1f, "Should have score <= 1.0");
            
            UnityEngine.Debug.Log($"[Quest3Validator] Session completed: Score {result.PerformanceScore:F2}, " +
                                $"Requirements met: {result.MeetsPerformanceRequirements}");
        }
        
        #endregion
        
        #region Metrics Validation Tests
        
        [Test]
        public void ValidateMetrics_WithGoodMetrics_ReturnsValid()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            var goodMetrics = new Quest3PerformanceMetrics
            {
                AverageFrameRate = 75f, // Above 72 FPS target
                P95FrameTimeMs = 12f,   // Below 13.89ms limit
                AverageCpuUsage = 0.6f, // Below 0.8 threshold
                AverageGpuUsage = 0.7f, // Below 0.85 threshold
                MemoryUsageMB = 1500f,  // Below 2048MB threshold
                PeakTemperatureCelsius = 40f // Below 45°C limit
            };
            
            // Act
            var result = _validator.ValidateMetrics(goodMetrics);
            
            // Assert
            Assert.IsTrue(result.IsValid, "Good metrics should be valid");
            Assert.Greater(result.ValidationScore, 0.8f, "Should have high validation score");
            Assert.AreEqual(0, result.Issues.Length, "Should have no issues");
            Assert.That(result.ValidationSummary, Does.Contain("PASSED"), "Should indicate passed validation");
        }
        
        [Test]
        public void ValidateMetrics_WithPoorFrameRate_ReturnsInvalid()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            var poorMetrics = new Quest3PerformanceMetrics
            {
                AverageFrameRate = 60f, // Below 72 FPS target
                P95FrameTimeMs = 18f,   // Above 13.89ms limit
                AverageCpuUsage = 0.9f, // Above 0.8 threshold
                AverageGpuUsage = 0.95f, // Above 0.85 threshold
                MemoryUsageMB = 2500f,  // Above 2048MB threshold
                PeakTemperatureCelsius = 50f // Above 45°C limit
            };
            
            // Act
            var result = _validator.ValidateMetrics(poorMetrics);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Poor metrics should be invalid");
            Assert.Less(result.ValidationScore, 0.5f, "Should have low validation score");
            Assert.Greater(result.Issues.Length, 0, "Should have performance issues");
            Assert.That(result.ValidationSummary, Does.Contain("FAILED"), "Should indicate failed validation");
            
            // Check specific issues
            var frameRateIssue = result.Issues.FirstOrDefault(i => i.IssueType == Quest3IssueType.FrameRateBelow72FPS);
            Assert.IsNotNull(frameRateIssue, "Should detect frame rate issue");
            Assert.AreEqual(Quest3IssueSeverity.Error, frameRateIssue.Severity, "Frame rate issue should be error");
        }
        
        [Test]
        public void ValidateMetrics_WithThermalIssues_DetectsCriticalIssue()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            var thermalMetrics = new Quest3PerformanceMetrics
            {
                AverageFrameRate = 72f,
                P95FrameTimeMs = 13f,
                AverageCpuUsage = 0.7f,
                AverageGpuUsage = 0.8f,
                MemoryUsageMB = 1800f,
                PeakTemperatureCelsius = 55f // Critical thermal issue
            };
            
            // Act
            var result = _validator.ValidateMetrics(thermalMetrics);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Thermal issues should make metrics invalid");
            
            var thermalIssue = result.Issues.FirstOrDefault(i => i.IssueType == Quest3IssueType.ThermalThrottling);
            Assert.IsNotNull(thermalIssue, "Should detect thermal throttling issue");
            Assert.AreEqual(Quest3IssueSeverity.Critical, thermalIssue.Severity, "Thermal issue should be critical");
            Assert.That(thermalIssue.RecommendedAction, Does.Contain("thermal"), "Should recommend thermal optimization");
        }
        
        #endregion
        
        #region Quick Validation Tests
        
        [Test]
        public void RunQuickValidation_WithDefaultBubbles_ReturnsResult()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Act
            var result = _validator.RunQuickValidation(100);
            
            // Assert
            Assert.Greater(result.TestDurationSeconds, 0f, "Should record test duration");
            Assert.Greater(result.AverageFrameRate, 0f, "Should measure frame rate");
            Assert.GreaterOrEqual(result.QuickScore, 0f, "Should have non-negative score");
            Assert.LessOrEqual(result.QuickScore, 1f, "Should have score <= 1.0");
            Assert.IsNotNull(result.Summary, "Should provide summary");
            
            UnityEngine.Debug.Log($"[Quest3Validator] Quick validation: {result.Summary}");
        }
        
        [Test]
        public void RunQuickValidation_WithHighBubbleCount_MayFail()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Act - Test with very high bubble count that might cause performance issues
            var result = _validator.RunQuickValidation(500);
            
            // Assert
            Assert.IsNotNull(result.Summary, "Should provide summary even if failed");
            
            if (!result.Passed)
            {
                Assert.Greater(result.CriticalIssues.Length, 0, "Failed validation should have critical issues");
                UnityEngine.Debug.Log($"[Quest3Validator] High bubble count test failed as expected: {result.Summary}");
            }
            else
            {
                UnityEngine.Debug.Log($"[Quest3Validator] High bubble count test passed: {result.Summary}");
            }
        }
        
        #endregion
        
        #region Thermal Validation Tests
        
        [Test]
        public void ValidateThermalPerformance_WithThermalMonitoring_ReturnsResult()
        {
            // Arrange
            var thermalConfig = _testConfig;
            thermalConfig.EnableThermalMonitoring = true;
            _validator.Initialize(thermalConfig);
            
            // Act
            var result = _validator.ValidateThermalPerformance();
            
            // Assert
            Assert.Greater(result.PeakTemperatureCelsius, 0f, "Should measure peak temperature");
            Assert.Greater(result.AverageTemperatureCelsius, 0f, "Should measure average temperature");
            Assert.Less(result.AverageTemperatureCelsius, result.PeakTemperatureCelsius, 
                "Average should be less than peak temperature");
            Assert.IsNotNull(result.ThermalSummary, "Should provide thermal summary");
            
            UnityEngine.Debug.Log($"[Quest3Validator] Thermal validation: {result.ThermalSummary}");
        }
        
        [Test]
        public void ValidateThermalPerformance_WithThermalDisabled_ReturnsAcceptable()
        {
            // Arrange
            var noThermalConfig = _testConfig;
            noThermalConfig.EnableThermalMonitoring = false;
            _validator.Initialize(noThermalConfig);
            
            // Act
            var result = _validator.ValidateThermalPerformance();
            
            // Assert
            Assert.IsTrue(result.ThermalPerformanceAcceptable, "Should be acceptable when monitoring disabled");
            Assert.That(result.ThermalSummary, Does.Contain("disabled"), "Should indicate monitoring is disabled");
        }
        
        #endregion
        
        #region Scalability Tests
        
        [Test]
        public void TestPerformanceScalability_WithReasonableRange_ReturnsResults()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Act
            var result = _validator.TestPerformanceScalability(50, 150, 25);
            
            // Assert
            Assert.Greater(result.DataPoints.Length, 0, "Should have data points");
            Assert.Greater(result.RecommendedMaxBubbles72FPS, 0, "Should recommend max bubbles for 72 FPS");
            Assert.LessOrEqual(result.RecommendedMaxBubbles90FPS, result.RecommendedMaxBubbles72FPS, 
                "90 FPS max should be <= 72 FPS max");
            Assert.IsNotNull(result.ScalingCharacteristics, "Should provide scaling analysis");
            
            // Verify data points are in ascending order
            for (int i = 1; i < result.DataPoints.Length; i++)
            {
                Assert.Greater(result.DataPoints[i].BubbleCount, result.DataPoints[i-1].BubbleCount, 
                    "Data points should be in ascending bubble count order");
            }
            
            UnityEngine.Debug.Log($"[Quest3Validator] Scalability test: 72FPS max = {result.RecommendedMaxBubbles72FPS}, " +
                                $"90FPS max = {result.RecommendedMaxBubbles90FPS}");
        }
        
        [Test]
        public void TestPerformanceScalability_WithSmallRange_HandlesGracefully()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Act
            var result = _validator.TestPerformanceScalability(90, 110, 10);
            
            // Assert
            Assert.Greater(result.DataPoints.Length, 0, "Should have data points even for small range");
            Assert.IsNotNull(result.ScalingCharacteristics, "Should provide scaling analysis");
        }
        
        #endregion
        
        #region Performance History Tests
        
        [Test]
        public void GetPerformanceHistory_WithNoHistory_ReturnsEmptyHistory()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Act
            var history = _validator.GetPerformanceHistory();
            
            // Assert
            Assert.AreEqual(0, history.Sessions.Length, "Should have no sessions initially");
            Assert.AreEqual(0f, history.AveragePerformanceScore, "Should have zero average score");
            Assert.AreEqual(0f, history.PerformanceChangeRate, "Should have zero change rate");
            Assert.IsNotNull(history.TrendAnalysis, "Should provide trend analysis");
        }
        
        [Test]
        public void GetPerformanceHistory_AfterSessions_ReturnsHistory()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Run a few quick sessions
            for (int i = 0; i < 3; i++)
            {
                _validator.StartValidationSession(1f);
                System.Threading.Thread.Sleep(50);
                _validator.StopValidationSession();
            }
            
            // Act
            var history = _validator.GetPerformanceHistory();
            
            // Assert
            Assert.AreEqual(3, history.Sessions.Length, "Should have 3 sessions in history");
            Assert.Greater(history.AveragePerformanceScore, 0f, "Should have positive average score");
            Assert.IsNotNull(history.TrendAnalysis, "Should provide trend analysis");
            
            UnityEngine.Debug.Log($"[Quest3Validator] Performance history: {history.Sessions.Length} sessions, " +
                                $"avg score {history.AveragePerformanceScore:F2}");
        }
        
        #endregion
        
        #region Performance Report Tests
        
        [Test]
        public void GeneratePerformanceReport_WithHistory_ReturnsComprehensiveReport()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Run a session to generate some data
            _validator.StartValidationSession(2f);
            System.Threading.Thread.Sleep(100);
            _validator.StopValidationSession();
            
            // Act
            var report = _validator.GeneratePerformanceReport();
            
            // Assert
            Assert.IsNotNull(report.ExecutiveSummary, "Should have executive summary");
            Assert.IsNotNull(report.DetailedAnalysis, "Should have detailed analysis");
            Assert.IsNotNull(report.TechnicalSpecifications, "Should have technical specifications");
            Assert.Greater(report.ValidationResults.Length, 0, "Should include validation results");
            Assert.Greater(report.Recommendations.Length, 0, "Should provide recommendations");
            Assert.Greater(report.ReportTimestamp.Year, 2020, "Should have valid timestamp");
            
            UnityEngine.Debug.Log($"[Quest3Validator] Generated report: Certification ready = {report.CertificationReady}");
        }
        
        #endregion
        
        #region Store Compliance Tests
        
        [Test]
        public void ValidateStoreCompliance_WithGoodPerformance_PassesCompliance()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Simulate good performance session
            _validator.StartValidationSession(1f);
            System.Threading.Thread.Sleep(50);
            _validator.StopValidationSession();
            
            // Act
            var compliance = _validator.ValidateStoreCompliance();
            
            // Assert
            Assert.GreaterOrEqual(compliance.ComplianceScore, 0f, "Should have non-negative compliance score");
            Assert.LessOrEqual(compliance.ComplianceScore, 1f, "Should have compliance score <= 1.0");
            Assert.IsNotNull(compliance.ComplianceSummary, "Should provide compliance summary");
            
            if (compliance.MeetsStoreRequirements)
            {
                Assert.AreEqual(0, compliance.Violations.Length, "Should have no violations if compliant");
                Assert.AreEqual(0, compliance.RequiredFixes.Length, "Should have no required fixes if compliant");
            }
            
            UnityEngine.Debug.Log($"[Quest3Validator] Store compliance: {compliance.ComplianceSummary}");
        }
        
        #endregion
        
        #region Configuration Tests
        
        [Test]
        public void ConfigureMonitoring_WithValidConfig_UpdatesConfiguration()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            var monitoringConfig = new Quest3MonitoringConfig
            {
                SampleRateHz = 20f,
                LogToFile = true,
                ShowPerformanceOverlay = true,
                EnableAutomaticIssueDetection = true,
                AlertThresholds = Quest3PerformanceConfig.HighPerformance
            };
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => {
                _validator.ConfigureMonitoring(monitoringConfig);
            });
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void StartValidationSession_WhenAlreadyRunning_StopsPreviousSession()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            var firstSession = _validator.StartValidationSession(10f);
            
            // Act
            var secondSession = _validator.StartValidationSession(5f);
            
            // Assert
            Assert.AreNotEqual(firstSession.SessionId, secondSession.SessionId, "Should create new session");
            Assert.IsTrue(_validator.IsValidationRunning, "Should still be running");
            Assert.AreEqual(5f, secondSession.PlannedDurationSeconds, "Should use new duration");
        }
        
        [Test]
        public void StopValidationSession_WhenNotRunning_HandlesGracefully()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => {
                var result = _validator.StopValidationSession();
                // Result will be default/empty but should not crash
            });
        }
        
        [Test]
        public void GetCurrentMetrics_WhenNotRunning_ReturnsDefault()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            
            // Act
            var metrics = _validator.GetCurrentMetrics();
            
            // Assert
            // Should return default metrics without throwing
            Assert.GreaterOrEqual(metrics.MeasurementDurationSeconds, 0f, "Should have non-negative duration");
        }
        
        #endregion
        
        #region Disposal Tests
        
        [Test]
        public void Dispose_WhenCalled_CleansUpCorrectly()
        {
            // Arrange
            _validator.Initialize(_testConfig);
            _validator.StartValidationSession(10f);
            
            // Act
            _validator.Dispose();
            
            // Assert
            Assert.IsFalse(_validator.IsInitialized, "Should not be initialized after disposal");
            Assert.IsFalse(_validator.IsValidationRunning, "Should not be running after disposal");
            
            // Should handle multiple dispose calls
            Assert.DoesNotThrow(() => {
                _validator.Dispose();
            });
        }
        
        #endregion
    }
}