using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace XRBubbleLibrary.Core.Tests
{
    [TestFixture]
    public class PerformanceThresholdManagerTests
    {
        private PerformanceThresholdManager _manager;
        private PerformanceThresholds _testThresholds;
        
        [SetUp]
        public void SetUp()
        {
            _testThresholds = new PerformanceThresholds
            {
                MinimumFPS = 72f,
                MaximumFrameTime = 13.89f,
                MaximumMemoryUsageMB = 512,
                MemoryWarningThresholdMB = 400,
                MaximumCPUUsage = 80f,
                MaximumGPUUsage = 85f,
                MaximumThermalState = 0.8f,
                RequiredHeadroomPercentage = 0.3f,
                ViolationTolerance = 3,
                ViolationTimeWindow = 5f
            };
            
            _manager = new PerformanceThresholdManager(_testThresholds);
        }
        
        [TearDown]
        public void TearDown()
        {
            _manager = null;
        }
        
        [Test]
        public void Constructor_WithValidThresholds_SetsThresholdsCorrectly()
        {
            // Act
            var currentThresholds = _manager.CurrentThresholds;
            
            // Assert
            Assert.IsNotNull(currentThresholds);
            Assert.AreEqual(_testThresholds.MinimumFPS, currentThresholds.MinimumFPS);
            Assert.AreEqual(_testThresholds.MaximumMemoryUsageMB, currentThresholds.MaximumMemoryUsageMB);
            Assert.AreEqual(_testThresholds.MaximumCPUUsage, currentThresholds.MaximumCPUUsage);
        }
        
        [Test]
        public void Constructor_WithNullThresholds_UsesDefaults()
        {
            // Act
            var manager = new PerformanceThresholdManager(null);
            var currentThresholds = manager.CurrentThresholds;
            
            // Assert
            Assert.IsNotNull(currentThresholds);
            Assert.IsTrue(currentThresholds.IsValid());
        }
        
        [Test]
        public void UpdateThresholds_WithValidThresholds_UpdatesSuccessfully()
        {
            // Arrange
            var newThresholds = new PerformanceThresholds
            {
                MinimumFPS = 60f,
                MaximumFrameTime = 16.67f,
                MaximumMemoryUsageMB = 256,
                MemoryWarningThresholdMB = 200,
                MaximumCPUUsage = 70f,
                MaximumGPUUsage = 75f,
                MaximumThermalState = 0.7f,
                RequiredHeadroomPercentage = 0.25f,
                ViolationTolerance = 2,
                ViolationTimeWindow = 3f
            };
            
            // Act
            _manager.UpdateThresholds(newThresholds);
            var currentThresholds = _manager.CurrentThresholds;
            
            // Assert
            Assert.AreEqual(60f, currentThresholds.MinimumFPS);
            Assert.AreEqual(256, currentThresholds.MaximumMemoryUsageMB);
            Assert.AreEqual(70f, currentThresholds.MaximumCPUUsage);
        }
        
        [Test]
        public void UpdateThresholds_WithNullThresholds_DoesNotUpdate()
        {
            // Arrange
            var originalThresholds = _manager.CurrentThresholds;
            
            // Act
            _manager.UpdateThresholds(null);
            var currentThresholds = _manager.CurrentThresholds;
            
            // Assert
            Assert.AreEqual(originalThresholds.MinimumFPS, currentThresholds.MinimumFPS);
            Assert.AreEqual(originalThresholds.MaximumMemoryUsageMB, currentThresholds.MaximumMemoryUsageMB);
        }
        
        [Test]
        public void UpdateThresholds_WithInvalidThresholds_DoesNotUpdate()
        {
            // Arrange
            var originalThresholds = _manager.CurrentThresholds;
            var invalidThresholds = new PerformanceThresholds
            {
                MinimumFPS = -10f, // Invalid
                MaximumFrameTime = 16.67f,
                MaximumMemoryUsageMB = 256,
                MemoryWarningThresholdMB = 200,
                MaximumCPUUsage = 70f,
                MaximumGPUUsage = 75f,
                MaximumThermalState = 0.7f,
                RequiredHeadroomPercentage = 0.25f,
                ViolationTolerance = 2,
                ViolationTimeWindow = 3f
            };
            
            // Act
            _manager.UpdateThresholds(invalidThresholds);
            var currentThresholds = _manager.CurrentThresholds;
            
            // Assert
            Assert.AreEqual(originalThresholds.MinimumFPS, currentThresholds.MinimumFPS);
        }
        
        [Test]
        public void ValidatePerformance_WithGoodMetrics_ReturnsValid()
        {
            // Arrange
            var goodMetrics = new PerformanceMetrics
            {
                AverageFPS = 80f,
                AverageFrameTime = 12f,
                MemoryUsage = 300 * 1024 * 1024, // 300MB
                CPUUsage = 60f,
                GPUUsage = 70f,
                ThermalState = 0.5f
            };
            
            // Act
            var result = _manager.ValidatePerformance(goodMetrics);
            
            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Violations.Count);
            Assert.AreEqual(1.0f, result.OverallHealthScore, 0.01f);
            Assert.IsFalse(result.HasCriticalViolations);
            Assert.IsFalse(result.HasWarningViolations);
        }
        
        [Test]
        public void ValidatePerformance_WithLowFPS_ReturnsViolation()
        {
            // Arrange
            var badMetrics = new PerformanceMetrics
            {
                AverageFPS = 50f, // Below 72 FPS threshold
                AverageFrameTime = 20f, // Above 13.89ms threshold
                MemoryUsage = 300 * 1024 * 1024,
                CPUUsage = 60f,
                GPUUsage = 70f,
                ThermalState = 0.5f
            };
            
            // Act
            var result = _manager.ValidatePerformance(badMetrics);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Violations.Count > 0);
            Assert.IsTrue(result.OverallHealthScore < 1.0f);
            
            var fpsViolation = result.Violations.FirstOrDefault(v => v.MetricType == PerformanceMetricType.FPS);
            Assert.IsNotNull(fpsViolation);
            Assert.AreEqual(50f, fpsViolation.CurrentValue);
            Assert.AreEqual(72f, fpsViolation.ThresholdValue);
            Assert.AreEqual(22f, fpsViolation.ViolationAmount);
        }
        
        [Test]
        public void ValidatePerformance_WithHighMemoryUsage_ReturnsViolation()
        {
            // Arrange
            var badMetrics = new PerformanceMetrics
            {
                AverageFPS = 80f,
                AverageFrameTime = 12f,
                MemoryUsage = 600 * 1024 * 1024, // 600MB, above 512MB threshold
                CPUUsage = 60f,
                GPUUsage = 70f,
                ThermalState = 0.5f
            };
            
            // Act
            var result = _manager.ValidatePerformance(badMetrics);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            var memoryViolation = result.Violations.FirstOrDefault(v => v.MetricType == PerformanceMetricType.MemoryUsage);
            Assert.IsNotNull(memoryViolation);
            Assert.AreEqual(600f, memoryViolation.CurrentValue, 0.1f);
            Assert.AreEqual(512f, memoryViolation.ThresholdValue);
        }
        
        [Test]
        public void ValidatePerformance_WithNullMetrics_ReturnsInvalid()
        {
            // Act
            var result = _manager.ValidatePerformance(null);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(0f, result.OverallHealthScore);
            Assert.IsTrue(result.Summary.Contains("null"));
        }
        
        [Test]
        public void IsThresholdViolated_WithViolatingValue_ReturnsTrue()
        {
            // Act & Assert
            Assert.IsTrue(_manager.IsThresholdViolated(PerformanceMetricType.FPS, 50f)); // Below 72
            Assert.IsTrue(_manager.IsThresholdViolated(PerformanceMetricType.MemoryUsage, 600f)); // Above 512
            Assert.IsTrue(_manager.IsThresholdViolated(PerformanceMetricType.CPUUsage, 90f)); // Above 80
        }
        
        [Test]
        public void IsThresholdViolated_WithNonViolatingValue_ReturnsFalse()
        {
            // Act & Assert
            Assert.IsFalse(_manager.IsThresholdViolated(PerformanceMetricType.FPS, 80f)); // Above 72
            Assert.IsFalse(_manager.IsThresholdViolated(PerformanceMetricType.MemoryUsage, 400f)); // Below 512
            Assert.IsFalse(_manager.IsThresholdViolated(PerformanceMetricType.CPUUsage, 70f)); // Below 80
        }
        
        [Test]
        public void GetViolationHistory_WithRecentViolations_ReturnsCorrectHistory()
        {
            // Arrange
            var badMetrics = new PerformanceMetrics
            {
                AverageFPS = 50f,
                AverageFrameTime = 12f,
                MemoryUsage = 300 * 1024 * 1024,
                CPUUsage = 60f,
                GPUUsage = 70f,
                ThermalState = 0.5f
            };
            
            // Act
            _manager.ValidatePerformance(badMetrics);
            var history = _manager.GetViolationHistory(TimeSpan.FromMinutes(1));
            
            // Assert
            Assert.IsTrue(history.Count > 0);
            Assert.IsTrue(history.Any(v => v.MetricType == PerformanceMetricType.FPS));
        }
        
        [Test]
        public void RegisterViolationCallback_WithValidCallback_RegistersSuccessfully()
        {
            // Arrange
            bool callbackTriggered = false;
            ThresholdViolation receivedViolation = null;
            
            Action<ThresholdViolation> callback = (violation) =>
            {
                callbackTriggered = true;
                receivedViolation = violation;
            };
            
            var badMetrics = new PerformanceMetrics
            {
                AverageFPS = 50f,
                AverageFrameTime = 12f,
                MemoryUsage = 300 * 1024 * 1024,
                CPUUsage = 60f,
                GPUUsage = 70f,
                ThermalState = 0.5f
            };
            
            // Act
            _manager.RegisterViolationCallback(callback);
            _manager.ValidatePerformance(badMetrics);
            
            // Assert
            Assert.IsTrue(callbackTriggered);
            Assert.IsNotNull(receivedViolation);
            Assert.AreEqual(PerformanceMetricType.FPS, receivedViolation.MetricType);
        }
        
        [Test]
        public void GetRecommendedAdjustments_WithInsufficientData_ReturnsLowConfidence()
        {
            // Act
            var recommendations = _manager.GetRecommendedAdjustments();
            
            // Assert
            Assert.IsNotNull(recommendations);
            Assert.IsTrue(recommendations.ConfidenceScore < 0.5f);
            Assert.IsTrue(recommendations.ReasoningReport.Contains("Insufficient"));
        }
        
        [Test]
        public void ConsecutiveViolations_IncreaseSeverity()
        {
            // Arrange
            var badMetrics = new PerformanceMetrics
            {
                AverageFPS = 70f, // Slightly below threshold
                AverageFrameTime = 12f,
                MemoryUsage = 300 * 1024 * 1024,
                CPUUsage = 60f,
                GPUUsage = 70f,
                ThermalState = 0.5f
            };
            
            // Act - First violation
            var result1 = _manager.ValidatePerformance(badMetrics);
            var firstViolation = result1.Violations.FirstOrDefault(v => v.MetricType == PerformanceMetricType.FPS);
            
            // Act - Second violation
            var result2 = _manager.ValidatePerformance(badMetrics);
            var secondViolation = result2.Violations.FirstOrDefault(v => v.MetricType == PerformanceMetricType.FPS);
            
            // Act - Third violation
            var result3 = _manager.ValidatePerformance(badMetrics);
            var thirdViolation = result3.Violations.FirstOrDefault(v => v.MetricType == PerformanceMetricType.FPS);
            
            // Assert
            Assert.IsNotNull(firstViolation);
            Assert.IsNotNull(secondViolation);
            Assert.IsNotNull(thirdViolation);
            
            // Severity should increase with consecutive violations
            Assert.IsTrue(thirdViolation.Severity >= secondViolation.Severity);
            Assert.IsTrue(secondViolation.Severity >= firstViolation.Severity);
        }
        
        [Test]
        public void PerformanceThresholds_IsValid_ValidatesCorrectly()
        {
            // Arrange
            var validThresholds = new PerformanceThresholds
            {
                MinimumFPS = 60f,
                MaximumFrameTime = 16.67f,
                MaximumMemoryUsageMB = 512,
                MemoryWarningThresholdMB = 400,
                MaximumCPUUsage = 80f,
                MaximumGPUUsage = 85f,
                MaximumThermalState = 0.8f,
                RequiredHeadroomPercentage = 0.3f,
                ViolationTolerance = 3,
                ViolationTimeWindow = 5f
            };
            
            var invalidThresholds = new PerformanceThresholds
            {
                MinimumFPS = -10f, // Invalid
                MaximumFrameTime = 16.67f,
                MaximumMemoryUsageMB = 512,
                MemoryWarningThresholdMB = 600, // Invalid - higher than max
                MaximumCPUUsage = 80f,
                MaximumGPUUsage = 85f,
                MaximumThermalState = 0.8f,
                RequiredHeadroomPercentage = 0.3f,
                ViolationTolerance = 3,
                ViolationTimeWindow = 5f
            };
            
            // Act & Assert
            Assert.IsTrue(validThresholds.IsValid());
            Assert.IsFalse(invalidThresholds.IsValid());
        }
        
        [Test]
        public void PerformanceThresholds_Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new PerformanceThresholds
            {
                MinimumFPS = 60f,
                MaximumMemoryUsageMB = 512
            };
            
            // Act
            var clone = original.Clone();
            clone.MinimumFPS = 72f;
            clone.MaximumMemoryUsageMB = 256;
            
            // Assert
            Assert.AreEqual(60f, original.MinimumFPS);
            Assert.AreEqual(512, original.MaximumMemoryUsageMB);
            Assert.AreEqual(72f, clone.MinimumFPS);
            Assert.AreEqual(256, clone.MaximumMemoryUsageMB);
        }
    }
}