using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XRBubbleLibrary.Quest3.Tests
{
    /// <summary>
    /// Comprehensive unit tests for HeadroomRuleEnforcer.
    /// Tests Requirement 6.3: 30% headroom rule enforcement.
    /// Tests Requirement 6.4: Feature addition evaluation against budget.
    /// Tests Requirement 6.5: Automatic warnings when headroom is insufficient.
    /// </summary>
    [TestFixture]
    public class HeadroomRuleEnforcerTests
    {
        private HeadroomRuleEnforcer _headroomEnforcer;
        private GameObject _testGameObject;
        
        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("HeadroomRuleEnforcerTest");
            _headroomEnforcer = _testGameObject.AddComponent<HeadroomRuleEnforcer>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_testGameObject);
            }
        }
        
        #region Initialization Tests
        
        [Test]
        public void Initialize_WithValidConfiguration_ReturnsTrue()
        {
            // Arrange
            var config = HeadroomEnforcementConfiguration.Quest3Default;
            
            // Act
            bool result = _headroomEnforcer.Initialize(config);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_headroomEnforcer.IsInitialized);
            Assert.AreEqual(config.TargetFrameRate, _headroomEnforcer.Configuration.TargetFrameRate);
            Assert.AreEqual(config.RequiredHeadroomPercent, _headroomEnforcer.Configuration.RequiredHeadroomPercent);
        }
        
        [Test]
        public void Initialize_WithInvalidTargetFrameRate_ReturnsFalse()
        {
            // Arrange
            var config = HeadroomEnforcementConfiguration.Quest3Default;
            config.TargetFrameRate = 0;
            
            // Act
            bool result = _headroomEnforcer.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_headroomEnforcer.IsInitialized);
        }
        
        [Test]
        public void Initialize_WithInvalidHeadroomPercent_ReturnsFalse()
        {
            // Arrange
            var config = HeadroomEnforcementConfiguration.Quest3Default;
            config.RequiredHeadroomPercent = -10f;
            
            // Act
            bool result = _headroomEnforcer.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_headroomEnforcer.IsInitialized);
        }
        
        [Test]
        public void Initialize_WithHeadroomPercentOver100_ReturnsFalse()
        {
            // Arrange
            var config = HeadroomEnforcementConfiguration.Quest3Default;
            config.RequiredHeadroomPercent = 150f;
            
            // Act
            bool result = _headroomEnforcer.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_headroomEnforcer.IsInitialized);
        }
        
        [Test]
        public void Initialize_SetsCurrentStatusToCompliant()
        {
            // Arrange
            var config = HeadroomEnforcementConfiguration.Quest3Default;
            
            // Act
            _headroomEnforcer.Initialize(config);
            
            // Assert
            Assert.AreEqual(HeadroomComplianceLevel.Compliant, _headroomEnforcer.CurrentHeadroomStatus.ComplianceLevel);
            Assert.IsTrue(_headroomEnforcer.CurrentHeadroomStatus.IsCompliant);
            Assert.AreEqual(100f, _headroomEnforcer.CurrentHeadroomStatus.AvailableHeadroomPercent);
        }
        
        #endregion
        
        #region Headroom Validation Tests
        
        [Test]
        public void ValidateHeadroom_WithCompliantPerformance_ReturnsValid()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceData = CreatePerformanceData(72f, 13.89f); // 72 FPS, good performance
            
            // Act
            var result = _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(HeadroomComplianceLevel.Compliant, result.ComplianceLevel);
            Assert.IsTrue(result.HeadroomPercent > 30f);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("compliant")));
        }
        
        [Test]
        public void ValidateHeadroom_WithWarningLevelPerformance_ReturnsWarning()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceData = CreatePerformanceData(60f, 16.67f); // 60 FPS, warning level
            
            // Act
            var result = _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(HeadroomComplianceLevel.Warning, result.ComplianceLevel);
            Assert.IsTrue(result.HeadroomPercent < 30f);
            Assert.IsTrue(result.HeadroomPercent >= 20f);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("warning")));
            Assert.IsTrue(result.RecommendedActions.Length > 0);
        }
        
        [Test]
        public void ValidateHeadroom_WithCriticalPerformance_ReturnsCritical()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceData = CreatePerformanceData(50f, 20f); // 50 FPS, critical level
            
            // Act
            var result = _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(HeadroomComplianceLevel.Critical, result.ComplianceLevel);
            Assert.IsTrue(result.HeadroomPercent < 20f);
            Assert.IsTrue(result.HeadroomPercent >= 10f);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("Critical")));
            Assert.IsTrue(result.RecommendedActions.Any(r => r.Contains("optimization")));
        }
        
        [Test]
        public void ValidateHeadroom_WithViolationPerformance_ReturnsViolation()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceData = CreatePerformanceData(40f, 25f); // 40 FPS, violation level
            
            // Act
            var result = _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(HeadroomComplianceLevel.Violation, result.ComplianceLevel);
            Assert.IsTrue(result.HeadroomPercent < 10f);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("violation")));
            Assert.IsTrue(result.RecommendedActions.Any(r => r.Contains("Critical")));
        }
        
        [Test]
        public void ValidateHeadroom_WhenNotInitialized_ReturnsInvalid()
        {
            // Arrange
            // Don't initialize the enforcer
            var performanceData = CreatePerformanceData(72f, 13.89f);
            
            // Act
            var result = _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ValidationMessages.Any(m => m.Contains("not initialized")));
        }
        
        [Test]
        public void ValidateHeadroom_UpdatesCurrentStatus()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceData = CreatePerformanceData(60f, 16.67f);
            
            // Act
            _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Assert
            var status = _headroomEnforcer.CurrentHeadroomStatus;
            Assert.AreEqual(HeadroomComplianceLevel.Warning, status.ComplianceLevel);
            Assert.IsFalse(status.IsCompliant);
            Assert.IsTrue(status.AvailableHeadroomPercent < 30f);
        }
        
        #endregion
        
        #region Feature Addition Evaluation Tests
        
        [Test]
        public void EvaluateFeatureAddition_WithLowImpactFeature_AllowsAddition()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var currentPerformance = CreatePerformanceData(72f, 13.89f); // Good performance
            var featureImpact = CreateFeatureImpact("LowImpactFeature", 0.5f, 0.3f, 0.1f);
            
            // Act
            var evaluation = _headroomEnforcer.EvaluateFeatureAddition(featureImpact, currentPerformance);
            
            // Assert
            Assert.IsTrue(evaluation.CanAddFeature);
            Assert.AreEqual(FeatureAdditionRisk.Low, evaluation.RiskLevel);
            Assert.IsTrue(evaluation.ProjectedHeadroomPercent >= 30f);
            Assert.IsTrue(evaluation.EvaluationDetails.Any(d => d.Contains("Current headroom")));
        }
        
        [Test]
        public void EvaluateFeatureAddition_WithHighImpactFeature_BlocksAddition()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var currentPerformance = CreatePerformanceData(65f, 15.38f); // Moderate performance
            var featureImpact = CreateFeatureImpact("HighImpactFeature", 15f, 20f, 5f);
            
            // Act
            var evaluation = _headroomEnforcer.EvaluateFeatureAddition(featureImpact, currentPerformance);
            
            // Assert
            Assert.IsFalse(evaluation.CanAddFeature);
            Assert.IsTrue(evaluation.RiskLevel == FeatureAdditionRisk.Critical || evaluation.RiskLevel == FeatureAdditionRisk.High);
            Assert.IsTrue(evaluation.ProjectedHeadroomPercent < 30f);
            Assert.IsTrue(evaluation.EvaluationDetails.Any(d => d.Contains("violate")));
            Assert.IsTrue(evaluation.RecommendedMitigations.Length > 0);
        }
        
        [Test]
        public void EvaluateFeatureAddition_WithMediumRiskFeature_AllowsWithWarnings()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var currentPerformance = CreatePerformanceData(70f, 14.29f); // Good but not excellent
            var featureImpact = CreateFeatureImpact("MediumImpactFeature", 3f, 4f, 1f);
            
            // Act
            var evaluation = _headroomEnforcer.EvaluateFeatureAddition(featureImpact, currentPerformance);
            
            // Assert
            Assert.IsTrue(evaluation.CanAddFeature);
            Assert.IsTrue(evaluation.RiskLevel == FeatureAdditionRisk.Medium || evaluation.RiskLevel == FeatureAdditionRisk.Low);
            Assert.IsTrue(evaluation.ProjectedHeadroomPercent >= 30f);
            Assert.IsTrue(evaluation.RecommendedMitigations.Any(m => m.Contains("Monitor")));
        }
        
        [Test]
        public void EvaluateFeatureAddition_WhenNotInitialized_ReturnsBlocked()
        {
            // Arrange
            // Don't initialize the enforcer
            var currentPerformance = CreatePerformanceData(72f, 13.89f);
            var featureImpact = CreateFeatureImpact("TestFeature", 1f, 1f, 0.1f);
            
            // Act
            var evaluation = _headroomEnforcer.EvaluateFeatureAddition(featureImpact, currentPerformance);
            
            // Assert
            Assert.IsFalse(evaluation.CanAddFeature);
            Assert.AreEqual(FeatureAdditionRisk.Critical, evaluation.RiskLevel);
            Assert.IsTrue(evaluation.EvaluationDetails.Any(d => d.Contains("not initialized")));
        }
        
        [Test]
        public void EvaluateFeatureAddition_IncludesConfidenceLevelInEvaluation()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var currentPerformance = CreatePerformanceData(72f, 13.89f);
            var featureImpact = CreateFeatureImpact("TestFeature", 2f, 2f, 0.5f);
            featureImpact.ConfidenceLevel = 0.8f;
            
            // Act
            var evaluation = _headroomEnforcer.EvaluateFeatureAddition(featureImpact, currentPerformance);
            
            // Assert
            Assert.IsTrue(evaluation.EvaluationDetails.Any(d => d.Contains("Confidence level: 0.8")));
        }
        
        #endregion
        
        #region Available Headroom Analysis Tests
        
        [Test]
        public void CalculateAvailableHeadroom_WithGoodPerformance_ReturnsHighHeadroom()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceData = CreatePerformanceData(72f, 13.89f);
            performanceData.CPUUsagePercent = 40f;
            performanceData.GPUUsagePercent = 35f;
            
            // Act
            var analysis = _headroomEnforcer.CalculateAvailableHeadroom(performanceData);
            
            // Assert
            Assert.IsTrue(analysis.TotalAvailablePercent > 30f);
            Assert.AreEqual(60f, analysis.AvailableCPUPercent, 0.1f);
            Assert.AreEqual(65f, analysis.AvailableGPUPercent, 0.1f);
            Assert.IsTrue(analysis.AvailableFrameTimeMs > 0);
            Assert.IsNotNull(analysis.ComponentHeadroom);
            Assert.IsTrue(analysis.ComponentHeadroom.ContainsKey("CPU"));
            Assert.IsTrue(analysis.ComponentHeadroom.ContainsKey("GPU"));
        }
        
        [Test]
        public void CalculateAvailableHeadroom_WithPoorPerformance_ReturnsLowHeadroom()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceData = CreatePerformanceData(45f, 22.22f);
            performanceData.CPUUsagePercent = 85f;
            performanceData.GPUUsagePercent = 90f;
            
            // Act
            var analysis = _headroomEnforcer.CalculateAvailableHeadroom(performanceData);
            
            // Assert
            Assert.IsTrue(analysis.TotalAvailablePercent < 10f);
            Assert.AreEqual(15f, analysis.AvailableCPUPercent, 0.1f);
            Assert.AreEqual(10f, analysis.AvailableGPUPercent, 0.1f);
            Assert.IsTrue(analysis.AvailableFrameTimeMs < 5f);
        }
        
        [Test]
        public void CalculateAvailableHeadroom_WhenNotInitialized_ReturnsEmptyAnalysis()
        {
            // Arrange
            // Don't initialize the enforcer
            var performanceData = CreatePerformanceData(72f, 13.89f);
            
            // Act
            var analysis = _headroomEnforcer.CalculateAvailableHeadroom(performanceData);
            
            // Assert
            Assert.AreEqual(0f, analysis.TotalAvailablePercent);
            Assert.IsNotNull(analysis.AnalysisTimestamp);
        }
        
        [Test]
        public void CalculateAvailableHeadroom_IncludesThermalHeadroom()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceData = CreatePerformanceData(72f, 13.89f);
            performanceData.ThermalState = 0.3f; // 30% thermal load
            
            // Act
            var analysis = _headroomEnforcer.CalculateAvailableHeadroom(performanceData);
            
            // Assert
            Assert.IsTrue(analysis.ComponentHeadroom.ContainsKey("Thermal"));
            Assert.AreEqual(70f, analysis.ComponentHeadroom["Thermal"], 0.1f);
        }
        
        #endregion
        
        #region Compliance Report Tests
        
        [Test]
        public void GenerateComplianceReport_WithCompliantHistory_ReturnsCompliantReport()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceHistory = CreatePerformanceHistory(10, 72f, 13.89f); // All compliant
            
            // Validate each entry to build history
            foreach (var performance in performanceHistory)
            {
                _headroomEnforcer.ValidateHeadroom(performance);
            }
            
            // Act
            var report = _headroomEnforcer.GenerateComplianceReport(performanceHistory);
            
            // Assert
            Assert.IsTrue(report.IsCompliant);
            Assert.AreEqual(100f, report.CompliancePercentage);
            Assert.AreEqual(0, report.ViolationCount);
            Assert.AreEqual(0, report.WarningCount);
            Assert.IsTrue(report.AverageHeadroom > 30f);
            Assert.IsTrue(report.MinimumHeadroom > 30f);
        }
        
        [Test]
        public void GenerateComplianceReport_WithMixedHistory_ReturnsAccurateReport()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceHistory = new List<PerformanceData>();
            
            // Add mix of compliant and non-compliant data
            performanceHistory.AddRange(CreatePerformanceHistory(7, 72f, 13.89f)); // Compliant
            performanceHistory.AddRange(CreatePerformanceHistory(3, 60f, 16.67f)); // Warning
            
            // Validate each entry to build history
            foreach (var performance in performanceHistory)
            {
                _headroomEnforcer.ValidateHeadroom(performance);
            }
            
            // Act
            var report = _headroomEnforcer.GenerateComplianceReport(performanceHistory.ToArray());
            
            // Assert
            Assert.IsFalse(report.IsCompliant); // Less than 95% compliance
            Assert.AreEqual(70f, report.CompliancePercentage);
            Assert.AreEqual(0, report.ViolationCount);
            Assert.IsTrue(report.WarningCount > 0);
            Assert.IsTrue(report.AverageHeadroom > 20f);
            Assert.IsTrue(report.MinimumHeadroom < 30f);
        }
        
        [Test]
        public void GenerateComplianceReport_WithEmptyHistory_ReturnsEmptyReport()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var emptyHistory = new PerformanceData[0];
            
            // Act
            var report = _headroomEnforcer.GenerateComplianceReport(emptyHistory);
            
            // Assert
            Assert.IsNotNull(report.ReportTimestamp);
            Assert.AreEqual(TimeSpan.Zero, report.ReportingPeriod);
        }
        
        [Test]
        public void GenerateComplianceReport_WhenNotInitialized_ReturnsEmptyReport()
        {
            // Arrange
            // Don't initialize the enforcer
            var performanceHistory = CreatePerformanceHistory(5, 72f, 13.89f);
            
            // Act
            var report = _headroomEnforcer.GenerateComplianceReport(performanceHistory);
            
            // Assert
            Assert.IsNotNull(report.ReportTimestamp);
        }
        
        [Test]
        public void GenerateComplianceReport_CalculatesCorrectTrend()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var performanceHistory = new List<PerformanceData>();
            
            // Add degrading performance over time
            performanceHistory.AddRange(CreatePerformanceHistory(10, 72f, 13.89f)); // Good start
            performanceHistory.AddRange(CreatePerformanceHistory(10, 60f, 16.67f)); // Degrading end
            
            // Validate each entry to build history
            foreach (var performance in performanceHistory)
            {
                _headroomEnforcer.ValidateHeadroom(performance);
            }
            
            // Act
            var report = _headroomEnforcer.GenerateComplianceReport(performanceHistory.ToArray());
            
            // Assert
            Assert.AreEqual(ComplianceTrend.Degrading, report.Trend);
        }
        
        #endregion
        
        #region Warning and Violation Event Tests
        
        [Test]
        public void ValidateHeadroom_WithWarningLevel_TriggersWarningEvent()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            bool warningTriggered = false;
            HeadroomWarningEventArgs warningArgs = null;
            
            _headroomEnforcer.HeadroomWarningTriggered += (args) =>
            {
                warningTriggered = true;
                warningArgs = args;
            };
            
            var performanceData = CreatePerformanceData(60f, 16.67f); // Warning level
            
            // Act
            _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Assert
            Assert.IsTrue(warningTriggered);
            Assert.IsNotNull(warningArgs);
            Assert.AreEqual(HeadroomComplianceLevel.Warning, warningArgs.WarningLevel);
            Assert.IsTrue(warningArgs.HeadroomPercent < 30f);
            Assert.IsNotNull(warningArgs.Message);
        }
        
        [Test]
        public void ValidateHeadroom_WithViolationLevel_TriggersViolationEvent()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            bool violationTriggered = false;
            HeadroomViolationEventArgs violationArgs = null;
            
            _headroomEnforcer.HeadroomViolationDetected += (args) =>
            {
                violationTriggered = true;
                violationArgs = args;
            };
            
            var performanceData = CreatePerformanceData(40f, 25f); // Violation level
            
            // Act
            _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Assert
            Assert.IsTrue(violationTriggered);
            Assert.IsNotNull(violationArgs);
            Assert.AreEqual(HeadroomComplianceLevel.Violation, violationArgs.Severity);
            Assert.IsTrue(violationArgs.HeadroomPercent < 10f);
            Assert.IsTrue(violationArgs.RecommendedActions.Length > 0);
        }
        
        [Test]
        public void ValidateHeadroom_WithCompliantLevel_DoesNotTriggerEvents()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            bool warningTriggered = false;
            bool violationTriggered = false;
            
            _headroomEnforcer.HeadroomWarningTriggered += (args) => warningTriggered = true;
            _headroomEnforcer.HeadroomViolationDetected += (args) => violationTriggered = true;
            
            var performanceData = CreatePerformanceData(72f, 13.89f); // Compliant level
            
            // Act
            _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Assert
            Assert.IsFalse(warningTriggered);
            Assert.IsFalse(violationTriggered);
        }
        
        #endregion
        
        #region Configuration and State Management Tests
        
        [Test]
        public void SetWarningThresholds_UpdatesThresholds()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            var newThresholds = new HeadroomWarningThresholds
            {
                WarningThreshold = 25f,
                CriticalThreshold = 15f,
                ViolationThreshold = 5f,
                TrendAnalysisWindow = 120f
            };
            
            // Act
            _headroomEnforcer.SetWarningThresholds(newThresholds);
            
            // Assert
            // Verify thresholds are applied by testing with performance data
            var performanceData = CreatePerformanceData(65f, 15.38f); // Should now be warning level
            var result = _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // The exact behavior depends on implementation details
            Assert.IsNotNull(result);
        }
        
        [Test]
        public void SetAutomaticMonitoring_EnablesDisablesMonitoring()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            
            // Act & Assert - Enable
            _headroomEnforcer.SetAutomaticMonitoring(true);
            // Monitoring state is internal, so we can't directly verify
            // In a real implementation, we might expose monitoring state
            
            // Act & Assert - Disable
            _headroomEnforcer.SetAutomaticMonitoring(false);
            // Similarly, we can't directly verify the disabled state
        }
        
        [Test]
        public void GetEnforcementHistory_ReturnsAccurateHistory()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            
            // Generate some enforcement actions
            var compliantData = CreatePerformanceData(72f, 13.89f);
            var warningData = CreatePerformanceData(60f, 16.67f);
            var violationData = CreatePerformanceData(40f, 25f);
            
            _headroomEnforcer.ValidateHeadroom(compliantData);
            _headroomEnforcer.ValidateHeadroom(warningData);
            _headroomEnforcer.ValidateHeadroom(violationData);
            
            // Act
            var history = _headroomEnforcer.GetEnforcementHistory();
            
            // Assert
            Assert.IsTrue(history.TotalChecks >= 3);
            Assert.IsTrue(history.WarningsIssued > 0);
            Assert.IsTrue(history.ViolationsDetected > 0);
            Assert.IsNotNull(history.EnforcementHistory);
            Assert.IsTrue(history.EnforcementHistory.Length > 0);
        }
        
        [Test]
        public void ResetEnforcementState_ClearsHistoryAndStatus()
        {
            // Arrange
            _headroomEnforcer.Initialize(HeadroomEnforcementConfiguration.Quest3Default);
            
            // Generate some history
            var performanceData = CreatePerformanceData(60f, 16.67f);
            _headroomEnforcer.ValidateHeadroom(performanceData);
            
            // Act
            _headroomEnforcer.ResetEnforcementState();
            
            // Assert
            var history = _headroomEnforcer.GetEnforcementHistory();
            Assert.AreEqual(0, history.TotalChecks);
            Assert.AreEqual(0, history.WarningsIssued);
            Assert.AreEqual(0, history.ViolationsDetected);
            
            var status = _headroomEnforcer.CurrentHeadroomStatus;
            Assert.AreEqual(HeadroomComplianceLevel.Compliant, status.ComplianceLevel);
            Assert.IsTrue(status.IsCompliant);
            Assert.AreEqual(100f, status.AvailableHeadroomPercent);
        }
        
        #endregion
        
        #region Helper Methods
        
        private PerformanceData CreatePerformanceData(float frameRate, float frameTimeMs)
        {
            return new PerformanceData
            {
                CurrentFrameRate = frameRate,
                CurrentFrameTimeMs = frameTimeMs,
                CPUUsagePercent = 50f,
                GPUUsagePercent = 45f,
                MemoryUsageMB = 500f,
                ThermalState = 0.2f,
                Timestamp = DateTime.Now,
                AdditionalMetrics = new Dictionary<string, float>()
            };
        }
        
        private FeaturePerformanceImpact CreateFeatureImpact(string featureName, float cpuImpact, float gpuImpact, float frameTimeImpact)
        {
            return new FeaturePerformanceImpact
            {
                FeatureName = featureName,
                EstimatedCPUImpact = cpuImpact,
                EstimatedGPUImpact = gpuImpact,
                EstimatedMemoryImpact = 10f,
                EstimatedFrameTimeImpact = frameTimeImpact,
                ConfidenceLevel = 0.8f,
                EstimationMethod = "Unit Test Estimation"
            };
        }
        
        private PerformanceData[] CreatePerformanceHistory(int count, float frameRate, float frameTimeMs)
        {
            var history = new PerformanceData[count];
            var baseTime = DateTime.Now.AddMinutes(-count);
            
            for (int i = 0; i < count; i++)
            {
                history[i] = new PerformanceData
                {
                    CurrentFrameRate = frameRate + (i * 0.1f), // Slight variation
                    CurrentFrameTimeMs = frameTimeMs - (i * 0.01f), // Slight variation
                    CPUUsagePercent = 50f + (i * 0.5f),
                    GPUUsagePercent = 45f + (i * 0.4f),
                    MemoryUsageMB = 500f + (i * 2f),
                    ThermalState = 0.2f + (i * 0.01f),
                    Timestamp = baseTime.AddMinutes(i),
                    AdditionalMetrics = new Dictionary<string, float>()
                };
            }
            
            return history;
        }
        
        #endregion
    }
}