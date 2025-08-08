using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the feature validation system.
    /// Tests automatic validation, reflection-based discovery, and reporting.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class FeatureValidationSystemTests
    {
        private GameObject testGameObject;
        private FeatureValidationSystem validationSystem;
        private CompilerFlagManager flagManager;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with validation system
            testGameObject = new GameObject("TestValidationSystem");
            validationSystem = testGameObject.AddComponent<FeatureValidationSystem>();
            
            flagManager = CompilerFlagManager.Instance;
            flagManager.ResetToDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testGameObject);
            }
            
            flagManager?.ResetToDefaults();
        }

        [Test]
        public void FeatureValidationSystem_PerformFullValidation_ReturnsValidResults()
        {
            var results = validationSystem.PerformFullValidation();
            
            Assert.IsNotNull(results);
            Assert.AreEqual(ValidationType.Full, results.ValidationType);
            Assert.Greater(results.ValidationTime, DateTime.MinValue);
            Assert.GreaterOrEqual(results.ValidationDuration, 0f);
            Assert.IsNotNull(results.Violations);
        }

        [Test]
        public void FeatureValidationSystem_PerformFullValidation_DetectsViolations()
        {
            // Disable a feature that has gates in our test classes
            if (CompilerFlags.AI_ENABLED)
            {
                flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
            }

            var results = validationSystem.PerformFullValidation();
            
            // Should detect violations if AI feature is disabled and we have AI-gated test classes
            if (!flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                Assert.Greater(results.Violations.Count, 0, "Should detect violations when AI feature is disabled");
                
                var aiViolations = results.Violations.Where(v => v.Feature == ExperimentalFeature.AI_INTEGRATION).ToArray();
                Assert.Greater(aiViolations.Length, 0, "Should have AI-related violations");
                
                foreach (var violation in aiViolations)
                {
                    Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, violation.Feature);
                    Assert.IsNotNull(violation.ErrorMessage);
                    Assert.IsNotEmpty(violation.ErrorMessage);
                }
            }
        }

        [Test]
        public void FeatureValidationSystem_PerformIncrementalValidation_ReturnsValidResults()
        {
            // Perform initial full validation
            validationSystem.PerformFullValidation();
            
            // Then perform incremental validation
            var results = validationSystem.PerformIncrementalValidation();
            
            Assert.IsNotNull(results);
            Assert.AreEqual(ValidationType.Incremental, results.ValidationType);
            Assert.Greater(results.ValidationTime, DateTime.MinValue);
            Assert.GreaterOrEqual(results.ValidationDuration, 0f);
            Assert.IsNotNull(results.Violations);
        }

        [Test]
        public void FeatureValidationSystem_ValidateFeature_ReturnsFeatureSpecificResults()
        {
            var result = validationSystem.ValidateFeature(ExperimentalFeature.AI_INTEGRATION);
            
            Assert.IsNotNull(result);
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, result.Feature);
            Assert.AreEqual(flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION), result.IsEnabled);
            Assert.Greater(result.ValidationTime, DateTime.MinValue);
            Assert.GreaterOrEqual(result.TotalGates, 0);
            Assert.GreaterOrEqual(result.ValidGates, 0);
            Assert.LessOrEqual(result.ValidGates, result.TotalGates);
            Assert.IsNotNull(result.Violations);
        }

        [Test]
        public void FeatureValidationSystem_GetLastValidationResults_ReturnsLastResults()
        {
            // Initially should return empty results
            var initialResults = validationSystem.GetLastValidationResults();
            Assert.IsFalse(initialResults.Success);
            Assert.IsNotNull(initialResults.ValidationError);
            
            // After validation, should return actual results
            var validationResults = validationSystem.PerformFullValidation();
            var lastResults = validationSystem.GetLastValidationResults();
            
            Assert.AreEqual(validationResults.ValidationTime, lastResults.ValidationTime);
            Assert.AreEqual(validationResults.Success, lastResults.Success);
            Assert.AreEqual(validationResults.Violations.Count, lastResults.Violations.Count);
        }

        [Test]
        public void FeatureValidationSystem_GenerateValidationReport_ContainsExpectedContent()
        {
            validationSystem.PerformFullValidation();
            var report = validationSystem.GenerateValidationReport();
            
            Assert.IsNotNull(report);
            Assert.IsNotEmpty(report);
            
            // Should contain header
            Assert.IsTrue(report.Contains("Feature Validation Report"));
            
            // Should contain validation metadata
            Assert.IsTrue(report.Contains("Validation Time:"));
            Assert.IsTrue(report.Contains("Duration:"));
            Assert.IsTrue(report.Contains("Status:"));
            
            // Should contain either PASSED or FAILED
            Assert.IsTrue(report.Contains("PASSED") || report.Contains("FAILED"));
            
            // Should contain feature summary
            Assert.IsTrue(report.Contains("Feature Gate Summary"));
            
            // Should contain timestamp
            Assert.IsTrue(report.Contains("Generated by FeatureValidationSystem"));
        }

        [Test]
        public void ValidationResults_DefaultConstructor_InitializesCorrectly()
        {
            var results = new ValidationResults();
            
            Assert.AreEqual(DateTime.MinValue, results.ValidationTime);
            Assert.IsFalse(results.Success);
            Assert.AreEqual(0f, results.ValidationDuration);
            Assert.IsNull(results.ValidationError);
            Assert.IsNotNull(results.Violations);
            Assert.AreEqual(0, results.Violations.Count);
        }

        [Test]
        public void FeatureValidationResult_DefaultConstructor_InitializesCorrectly()
        {
            var result = new FeatureValidationResult();
            
            Assert.AreEqual(default(ExperimentalFeature), result.Feature);
            Assert.IsFalse(result.IsEnabled);
            Assert.AreEqual(DateTime.MinValue, result.ValidationTime);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(0, result.TotalGates);
            Assert.AreEqual(0, result.ValidGates);
            Assert.IsNotNull(result.Violations);
            Assert.AreEqual(0, result.Violations.Count);
        }

        [Test]
        public void FeatureGateViolation_Properties_CanBeSetAndRetrieved()
        {
            var violation = new FeatureGateViolation
            {
                Feature = ExperimentalFeature.VOICE_PROCESSING,
                TargetName = "TestTarget",
                TargetType = FeatureGateTargetType.Method,
                ErrorMessage = "Test error message",
                ViolationType = ViolationType.FeatureDisabled
            };
            
            Assert.AreEqual(ExperimentalFeature.VOICE_PROCESSING, violation.Feature);
            Assert.AreEqual("TestTarget", violation.TargetName);
            Assert.AreEqual(FeatureGateTargetType.Method, violation.TargetType);
            Assert.AreEqual("Test error message", violation.ErrorMessage);
            Assert.AreEqual(ViolationType.FeatureDisabled, violation.ViolationType);
        }

        [UnityTest]
        public IEnumerator FeatureValidationSystem_AutomaticValidation_RunsPeriodically()
        {
            // Set a short validation interval for testing
            var validationSystemType = typeof(FeatureValidationSystem);
            var intervalField = validationSystemType.GetField("validationInterval", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            intervalField?.SetValue(validationSystem, 0.1f); // 100ms interval
            
            // Enable automatic validation
            var enableField = validationSystemType.GetField("enableAutomaticValidation", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enableField?.SetValue(validationSystem, true);
            
            // Start the validation system
            validationSystem.gameObject.SetActive(true);
            
            // Wait for at least one validation cycle
            yield return new WaitForSeconds(0.2f);
            
            // Should have performed at least one validation
            var lastResults = validationSystem.GetLastValidationResults();
            Assert.Greater(lastResults.ValidationTime, DateTime.MinValue);
        }

        [Test]
        public void FeatureValidationSystem_EventHandling_FiresOnValidationComplete()
        {
            bool eventFired = false;
            ValidationResults receivedResults = null;
            
            validationSystem.OnValidationComplete += (results) =>
            {
                eventFired = true;
                receivedResults = results;
            };
            
            var validationResults = validationSystem.PerformFullValidation();
            
            Assert.IsTrue(eventFired, "OnValidationComplete event should fire");
            Assert.IsNotNull(receivedResults, "Event should provide validation results");
            Assert.AreEqual(validationResults.ValidationTime, receivedResults.ValidationTime);
        }

        [Test]
        public void FeatureValidationSystem_EventHandling_FiresOnFeatureGateViolation()
        {
            // Only test if we can create violations
            if (!CompilerFlags.AI_ENABLED)
            {
                Assert.Ignore("Cannot test violations when AI feature is compile-time disabled");
                return;
            }

            // Disable AI to create violations
            flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
            
            bool eventFired = false;
            FeatureGateViolation receivedViolation = null;
            
            validationSystem.OnFeatureGateViolation += (violation) =>
            {
                eventFired = true;
                receivedViolation = violation;
            };
            
            var results = validationSystem.PerformFullValidation();
            
            if (results.Violations.Count > 0)
            {
                Assert.IsTrue(eventFired, "OnFeatureGateViolation event should fire when violations occur");
                Assert.IsNotNull(receivedViolation, "Event should provide violation details");
            }
        }

        [Test]
        [Performance]
        public void FeatureValidationSystem_PerformFullValidation_HasReasonablePerformance()
        {
            const int iterations = 10;
            var totalTime = 0f;
            
            for (int i = 0; i < iterations; i++)
            {
                var results = validationSystem.PerformFullValidation();
                totalTime += results.ValidationDuration;
            }
            
            var averageTime = totalTime / iterations;
            
            // Full validation should complete in reasonable time (less than 100ms on average)
            Assert.Less(averageTime, 0.1f, 
                "Full validation should complete in reasonable time");
            
            Debug.Log($"Average full validation time: {averageTime * 1000:F2}ms");
        }

        [Test]
        public void ValidationType_Enum_HasExpectedValues()
        {
            var values = Enum.GetValues(typeof(ValidationType));
            
            Assert.Contains(ValidationType.Full, values);
            Assert.Contains(ValidationType.Incremental, values);
            Assert.AreEqual(2, values.Length, "Should have exactly 2 validation types");
        }

        [Test]
        public void ViolationType_Enum_HasExpectedValues()
        {
            var values = Enum.GetValues(typeof(ViolationType));
            
            Assert.Contains(ViolationType.FeatureDisabled, values);
            Assert.Contains(ViolationType.DependencyMissing, values);
            Assert.Contains(ViolationType.ValidationError, values);
            Assert.AreEqual(3, values.Length, "Should have exactly 3 violation types");
        }

        [Test]
        public void FeatureValidationSystem_DependencyValidation_DetectsMissingDependencies()
        {
            // Test dependency validation by enabling a dependent feature without its dependency
            if (CompilerFlags.AI_ENABLED && CompilerFlags.CLOUD_INFERENCE_ENABLED)
            {
                // Disable AI but keep cloud inference enabled (if possible)
                flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
                
                var results = validationSystem.PerformFullValidation();
                
                // Should detect dependency violation
                var dependencyViolations = results.Violations.Where(v => v.ViolationType == ViolationType.DependencyMissing).ToArray();
                
                if (flagManager.IsFeatureEnabled(ExperimentalFeature.CLOUD_INFERENCE))
                {
                    Assert.Greater(dependencyViolations.Length, 0, "Should detect missing AI dependency for cloud inference");
                    
                    var cloudInferenceViolation = dependencyViolations.FirstOrDefault(v => v.Feature == ExperimentalFeature.CLOUD_INFERENCE);
                    Assert.IsNotNull(cloudInferenceViolation, "Should have specific violation for cloud inference dependency");
                    Assert.IsTrue(cloudInferenceViolation.ErrorMessage.Contains("AI Integration"), "Error message should mention AI Integration dependency");
                }
            }
            else
            {
                Assert.Ignore("Cannot test dependency validation without compile-time enabled features");
            }
        }
    }
}