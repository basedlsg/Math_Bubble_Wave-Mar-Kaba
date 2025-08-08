using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the compiler flag management system.
    /// Validates both compile-time and runtime behavior.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class CompilerFlagTests
    {
        private CompilerFlagManager flagManager;

        [SetUp]
        public void SetUp()
        {
            // Create a fresh instance for each test
            flagManager = new CompilerFlagManager();
        }

        [TearDown]
        public void TearDown()
        {
            // Reset to defaults after each test
            flagManager?.ResetToDefaults();
        }

        [Test]
        public void CompilerFlags_CompileTimeConstants_AreConsistent()
        {
            // Test that compile-time constants match expected values
            // These tests will vary based on actual compiler flags set
            
            // AI Integration
            var aiExpected = 
#if EXP_AI
                true;
#else
                false;
#endif
            Assert.AreEqual(aiExpected, CompilerFlags.AI_ENABLED, 
                "AI_ENABLED constant should match EXP_AI compiler flag");

            // Voice Processing
            var voiceExpected = 
#if EXP_VOICE
                true;
#else
                false;
#endif
            Assert.AreEqual(voiceExpected, CompilerFlags.VOICE_ENABLED, 
                "VOICE_ENABLED constant should match EXP_VOICE compiler flag");

            // Advanced Wave Algorithms
            var advancedWaveExpected = 
#if EXP_ADVANCED_WAVE
                true;
#else
                false;
#endif
            Assert.AreEqual(advancedWaveExpected, CompilerFlags.ADVANCED_WAVE_ENABLED, 
                "ADVANCED_WAVE_ENABLED constant should match EXP_ADVANCED_WAVE compiler flag");
        }

        [Test]
        public void CompilerFlags_IsFeatureEnabled_ReturnsCorrectValues()
        {
            // Test that IsFeatureEnabled returns correct compile-time values
            Assert.AreEqual(CompilerFlags.AI_ENABLED, 
                CompilerFlags.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION));
            
            Assert.AreEqual(CompilerFlags.VOICE_ENABLED, 
                CompilerFlags.IsFeatureEnabled(ExperimentalFeature.VOICE_PROCESSING));
            
            Assert.AreEqual(CompilerFlags.ADVANCED_WAVE_ENABLED, 
                CompilerFlags.IsFeatureEnabled(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS));
            
            Assert.AreEqual(CompilerFlags.CLOUD_INFERENCE_ENABLED, 
                CompilerFlags.IsFeatureEnabled(ExperimentalFeature.CLOUD_INFERENCE));
            
            Assert.AreEqual(CompilerFlags.ON_DEVICE_ML_ENABLED, 
                CompilerFlags.IsFeatureEnabled(ExperimentalFeature.ON_DEVICE_ML));
        }

        [Test]
        public void CompilerFlagManager_IsFeatureEnabled_MatchesCompileTimeDefaults()
        {
            // Test that manager returns same values as static class initially
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                var staticResult = CompilerFlags.IsFeatureEnabled(feature);
                var managerResult = flagManager.IsFeatureEnabled(feature);
                
                Assert.AreEqual(staticResult, managerResult, 
                    $"Manager should match static result for {feature}");
            }
        }

        [Test]
        public void CompilerFlagManager_SetFeatureState_CanDisableEnabledFeatures()
        {
            // Find a feature that is compile-time enabled (if any)
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                if (CompilerFlags.IsFeatureEnabled(feature))
                {
                    // Should be able to disable it at runtime
                    flagManager.SetFeatureState(feature, false);
                    Assert.IsFalse(flagManager.IsFeatureEnabled(feature), 
                        $"Should be able to disable {feature} at runtime");
                    
                    // Should be able to re-enable it
                    flagManager.SetFeatureState(feature, true);
                    Assert.IsTrue(flagManager.IsFeatureEnabled(feature), 
                        $"Should be able to re-enable {feature} at runtime");
                    
                    return; // Only need to test one enabled feature
                }
            }
            
            // If no features are enabled, log a warning but don't fail
            Debug.LogWarning("No compile-time enabled features found for runtime disable test");
        }

        [Test]
        public void CompilerFlagManager_SetFeatureState_CannotEnableDisabledFeatures()
        {
            // Find a feature that is compile-time disabled (if any)
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                if (!CompilerFlags.IsFeatureEnabled(feature))
                {
                    // Should not be able to enable it at runtime
                    LogAssert.Expect(LogType.Warning, 
                        new System.Text.RegularExpressions.Regex($"Cannot enable {feature}.*"));
                    
                    flagManager.SetFeatureState(feature, true);
                    Assert.IsFalse(flagManager.IsFeatureEnabled(feature), 
                        $"Should not be able to enable compile-time disabled {feature}");
                    
                    return; // Only need to test one disabled feature
                }
            }
            
            // If all features are enabled, log a warning but don't fail
            Debug.LogWarning("No compile-time disabled features found for runtime enable test");
        }

        [Test]
        public void CompilerFlagManager_GetAllFeatureStates_ReturnsAllFeatures()
        {
            var states = flagManager.GetAllFeatureStates();
            
            // Should have an entry for every feature
            var expectedFeatureCount = Enum.GetValues<ExperimentalFeature>().Length;
            Assert.AreEqual(expectedFeatureCount, states.Count, 
                "Should return state for all experimental features");
            
            // Should contain all features
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                Assert.IsTrue(states.ContainsKey(feature), 
                    $"Should contain state for {feature}");
            }
        }

        [Test]
        public void CompilerFlagManager_ValidateConfiguration_DetectsDependencyIssues()
        {
            // This test depends on having some features enabled
            // We'll simulate dependency issues by manipulating runtime state
            
            // If AI is compile-time enabled, we can test dependency validation
            if (CompilerFlags.AI_ENABLED)
            {
                // Disable AI but try to keep dependent features enabled
                flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
                
                if (CompilerFlags.CLOUD_INFERENCE_ENABLED)
                {
                    // This should trigger a validation warning
                    LogAssert.Expect(LogType.Warning, 
                        new System.Text.RegularExpressions.Regex("Configuration validation found.*issue"));
                    flagManager.ValidateConfiguration();
                }
            }
        }

        [Test]
        public void CompilerFlagManager_GenerateStatusReport_ContainsAllFeatures()
        {
            var report = flagManager.GenerateStatusReport();
            
            Assert.IsNotNull(report, "Status report should not be null");
            Assert.IsNotEmpty(report, "Status report should not be empty");
            
            // Should contain all feature names
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                Assert.IsTrue(report.Contains(feature.ToString()), 
                    $"Status report should contain {feature}");
            }
            
            // Should contain status indicators
            Assert.IsTrue(report.Contains("ENABLED") || report.Contains("DISABLED"), 
                "Status report should contain status indicators");
            
            // Should contain timestamp
            Assert.IsTrue(report.Contains("Generated at:"), 
                "Status report should contain generation timestamp");
        }

        [Test]
        public void CompilerFlagManager_ResetToDefaults_ClearsRuntimeOverrides()
        {
            // Find a feature we can modify
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                if (CompilerFlags.IsFeatureEnabled(feature))
                {
                    // Disable it at runtime
                    flagManager.SetFeatureState(feature, false);
                    Assert.IsFalse(flagManager.IsFeatureEnabled(feature), 
                        "Feature should be disabled after runtime change");
                    
                    // Reset to defaults
                    flagManager.ResetToDefaults();
                    Assert.IsTrue(flagManager.IsFeatureEnabled(feature), 
                        "Feature should be re-enabled after reset to defaults");
                    
                    return; // Only need to test one feature
                }
            }
        }

        [Test]
        public void CompilerFlags_ValidateFeatureAccess_ThrowsForDisabledFeatures()
        {
            // This test only runs in editor builds due to [Conditional("UNITY_EDITOR")]
#if UNITY_EDITOR
            // Find a disabled feature to test with
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                if (!CompilerFlags.IsFeatureEnabled(feature))
                {
                    Assert.Throws<InvalidOperationException>(() => 
                        CompilerFlags.ValidateFeatureAccess(feature),
                        $"Should throw exception when accessing disabled {feature}");
                    
                    return; // Only need to test one disabled feature
                }
            }
            
            Debug.LogWarning("No disabled features found for access validation test");
#else
            // In non-editor builds, the method should be compiled out
            // We can't easily test this, but we can document the expectation
            Debug.Log("ValidateFeatureAccess is compiled out in non-editor builds");
#endif
        }

        [Test]
        public void CompilerFlags_GetFeatureStatusReport_IsWellFormed()
        {
            var report = CompilerFlags.GetFeatureStatusReport();
            
            Assert.IsNotNull(report, "Feature status report should not be null");
            Assert.IsNotEmpty(report, "Feature status report should not be empty");
            
            // Should be markdown formatted
            Assert.IsTrue(report.StartsWith("# "), "Report should start with markdown header");
            
            // Should contain all features
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                Assert.IsTrue(report.Contains(feature.ToString()), 
                    $"Report should contain {feature}");
            }
            
            // Should contain status information
            Assert.IsTrue(report.Contains("ENABLED") || report.Contains("DISABLED"), 
                "Report should contain status information");
        }

        [Test]
        public void ExperimentalFeature_Enum_HasExpectedValues()
        {
            // Verify that all expected experimental features are defined
            var expectedFeatures = new[]
            {
                ExperimentalFeature.AI_INTEGRATION,
                ExperimentalFeature.VOICE_PROCESSING,
                ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS,
                ExperimentalFeature.CLOUD_INFERENCE,
                ExperimentalFeature.ON_DEVICE_ML
            };
            
            var actualFeatures = Enum.GetValues<ExperimentalFeature>();
            
            Assert.AreEqual(expectedFeatures.Length, actualFeatures.Length, 
                "Should have expected number of experimental features");
            
            foreach (var expected in expectedFeatures)
            {
                Assert.IsTrue(Array.Exists(actualFeatures, f => f == expected), 
                    $"Should contain {expected} feature");
            }
        }

        [Test]
        [Performance]
        public void CompilerFlags_IsFeatureEnabled_HasMinimalPerformanceOverhead()
        {
            // Performance test to ensure feature checking is fast
            const int iterations = 100000;
            var startTime = Time.realtimeSinceStartup;
            
            for (int i = 0; i < iterations; i++)
            {
                // Test all features
                foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
                {
                    CompilerFlags.IsFeatureEnabled(feature);
                }
            }
            
            var endTime = Time.realtimeSinceStartup;
            var totalTime = endTime - startTime;
            var timePerCheck = totalTime / (iterations * Enum.GetValues<ExperimentalFeature>().Length);
            
            // Should be extremely fast (less than 1 microsecond per check)
            Assert.Less(timePerCheck, 0.000001f, 
                "Feature checking should have minimal performance overhead");
            
            Debug.Log($"Feature check performance: {timePerCheck * 1000000:F2} microseconds per check");
        }
    }
}