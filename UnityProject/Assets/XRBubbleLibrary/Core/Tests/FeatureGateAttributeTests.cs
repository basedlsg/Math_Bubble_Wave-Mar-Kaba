using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the feature gate attribute system.
    /// Validates attribute behavior, reflection-based validation, and error handling.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class FeatureGateAttributeTests
    {
        private CompilerFlagManager flagManager;

        [SetUp]
        public void SetUp()
        {
            flagManager = CompilerFlagManager.Instance;
            flagManager.ResetToDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            flagManager?.ResetToDefaults();
        }

        [Test]
        public void FeatureGateAttribute_Constructor_SetsRequiredFeature()
        {
            var attribute = new FeatureGateAttribute(ExperimentalFeature.AI_INTEGRATION);
            
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, attribute.RequiredFeature);
            Assert.IsTrue(attribute.ThrowOnDisabled); // Default value
            Assert.IsFalse(attribute.ValidateInRelease); // Default value
            Assert.IsNull(attribute.ErrorMessage); // Default value
        }

        [Test]
        public void FeatureGateAttribute_ValidateFeatureAccess_ThrowsWhenFeatureDisabled()
        {
            // Ensure AI feature is disabled for this test
            if (CompilerFlags.AI_ENABLED)
            {
                flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
            }

            var attribute = new FeatureGateAttribute(ExperimentalFeature.AI_INTEGRATION);
            
            Assert.Throws<FeatureDisabledException>(() => attribute.ValidateFeatureAccess());
        }

        [Test]
        public void FeatureGateAttribute_ValidateFeatureAccess_PassesWhenFeatureEnabled()
        {
            // Enable AI feature for this test
            if (!CompilerFlags.AI_ENABLED)
            {
                // Can't enable compile-time disabled features, so skip this test
                Assert.Ignore("Cannot test enabled feature when compile-time disabled");
                return;
            }

            flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            var attribute = new FeatureGateAttribute(ExperimentalFeature.AI_INTEGRATION);
            
            Assert.DoesNotThrow(() => attribute.ValidateFeatureAccess());
        }

        [Test]
        public void FeatureGateAttribute_ValidateFeatureAccess_LogsWarningWhenThrowOnDisabledFalse()
        {
            // Ensure AI feature is disabled for this test
            if (CompilerFlags.AI_ENABLED)
            {
                flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
            }

            var attribute = new FeatureGateAttribute(ExperimentalFeature.AI_INTEGRATION)
            {
                ThrowOnDisabled = false
            };

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Feature.*disabled.*"));
            
            Assert.DoesNotThrow(() => attribute.ValidateFeatureAccess());
        }

        [Test]
        public void FeatureGateAttribute_ValidateFeatureAccess_UsesCustomErrorMessage()
        {
            // Ensure AI feature is disabled for this test
            if (CompilerFlags.AI_ENABLED)
            {
                flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
            }

            const string customMessage = "Custom error message for testing";
            var attribute = new FeatureGateAttribute(ExperimentalFeature.AI_INTEGRATION)
            {
                ErrorMessage = customMessage
            };

            var exception = Assert.Throws<FeatureDisabledException>(() => attribute.ValidateFeatureAccess());
            Assert.AreEqual(customMessage, exception.Message);
        }

        [Test]
        public void FeatureDisabledException_Constructor_SetsProperties()
        {
            const string message = "Test message";
            var exception = new FeatureDisabledException(ExperimentalFeature.VOICE_PROCESSING, message);
            
            Assert.AreEqual(ExperimentalFeature.VOICE_PROCESSING, exception.DisabledFeature);
            Assert.AreEqual(message, exception.Message);
        }

        [Test]
        public void FeatureDisabledException_ConstructorWithInnerException_SetsProperties()
        {
            const string message = "Test message";
            var innerException = new InvalidOperationException("Inner exception");
            var exception = new FeatureDisabledException(ExperimentalFeature.VOICE_PROCESSING, message, innerException);
            
            Assert.AreEqual(ExperimentalFeature.VOICE_PROCESSING, exception.DisabledFeature);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }

        [Test]
        public void FeatureGateValidator_ValidateMethod_ValidatesMethodAttribute()
        {
            var method = typeof(TestClassWithFeatureGates).GetMethod(nameof(TestClassWithFeatureGates.AIRequiredMethod));
            
            // If AI is disabled, should throw
            if (!flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                Assert.Throws<FeatureDisabledException>(() => FeatureGateValidator.ValidateMethod(method));
            }
            else
            {
                Assert.DoesNotThrow(() => FeatureGateValidator.ValidateMethod(method));
            }
        }

        [Test]
        public void FeatureGateValidator_ValidateType_ValidatesClassAttribute()
        {
            var type = typeof(AIRequiredTestClass);
            
            // If AI is disabled, should throw
            if (!flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                Assert.Throws<FeatureDisabledException>(() => FeatureGateValidator.ValidateType(type));
            }
            else
            {
                Assert.DoesNotThrow(() => FeatureGateValidator.ValidateType(type));
            }
        }

        [Test]
        public void FeatureGateValidator_GetAllFeatureGates_FindsAllGates()
        {
            var gates = FeatureGateValidator.GetAllFeatureGates();
            
            Assert.IsNotNull(gates);
            Assert.Greater(gates.Length, 0, "Should find at least some feature gates in test classes");
            
            // Should find our test class
            var classGate = Array.Find(gates, g => g.TargetName.Contains(nameof(AIRequiredTestClass)));
            Assert.IsNotNull(classGate, "Should find AIRequiredTestClass gate");
            Assert.AreEqual(FeatureGateTargetType.Class, classGate.TargetType);
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, classGate.Attribute.RequiredFeature);
            
            // Should find our test method
            var methodGate = Array.Find(gates, g => g.TargetName.Contains(nameof(TestClassWithFeatureGates.AIRequiredMethod)));
            Assert.IsNotNull(methodGate, "Should find AIRequiredMethod gate");
            Assert.AreEqual(FeatureGateTargetType.Method, methodGate.TargetType);
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, methodGate.Attribute.RequiredFeature);
        }

        [Test]
        public void FeatureGateValidator_GenerateFeatureGateReport_ContainsExpectedContent()
        {
            var report = FeatureGateValidator.GenerateFeatureGateReport();
            
            Assert.IsNotNull(report);
            Assert.IsNotEmpty(report);
            
            // Should contain header
            Assert.IsTrue(report.Contains("Feature Gate Analysis Report"));
            
            // Should contain feature sections
            Assert.IsTrue(report.Contains("AI_INTEGRATION") || report.Contains("VOICE_PROCESSING"));
            
            // Should contain gate count
            Assert.IsTrue(report.Contains("gates"));
            
            // Should contain timestamp
            Assert.IsTrue(report.Contains("Generated at:"));
        }

        [Test]
        public void FeatureGateExtensions_HasFeatureGate_DetectsAttributeOnType()
        {
            var type = typeof(AIRequiredTestClass);
            Assert.IsTrue(type.HasFeatureGate());
            
            var normalType = typeof(string);
            Assert.IsFalse(normalType.HasFeatureGate());
        }

        [Test]
        public void FeatureGateExtensions_HasFeatureGate_DetectsAttributeOnMethod()
        {
            var method = typeof(TestClassWithFeatureGates).GetMethod(nameof(TestClassWithFeatureGates.AIRequiredMethod));
            Assert.IsTrue(method.HasFeatureGate());
            
            var normalMethod = typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes);
            Assert.IsFalse(normalMethod.HasFeatureGate());
        }

        [Test]
        public void FeatureGateExtensions_GetRequiredFeature_ReturnsCorrectFeature()
        {
            var type = typeof(AIRequiredTestClass);
            var requiredFeature = type.GetRequiredFeature();
            
            Assert.IsTrue(requiredFeature.HasValue);
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, requiredFeature.Value);
            
            var normalType = typeof(string);
            var noFeature = normalType.GetRequiredFeature();
            Assert.IsFalse(noFeature.HasValue);
        }

        [Test]
        public void FeatureGateExtensions_GetRequiredFeature_ReturnsCorrectFeatureForMethod()
        {
            var method = typeof(TestClassWithFeatureGates).GetMethod(nameof(TestClassWithFeatureGates.AIRequiredMethod));
            var requiredFeature = method.GetRequiredFeature();
            
            Assert.IsTrue(requiredFeature.HasValue);
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, requiredFeature.Value);
            
            var normalMethod = typeof(string).GetMethod(nameof(string.ToString), Type.EmptyTypes);
            var noFeature = normalMethod.GetRequiredFeature();
            Assert.IsFalse(noFeature.HasValue);
        }

        [Test]
        public void FeatureGateAttribute_GetDescription_ReturnsFormattedDescription()
        {
            var attribute = new FeatureGateAttribute(ExperimentalFeature.AI_INTEGRATION)
            {
                ThrowOnDisabled = true,
                ValidateInRelease = false
            };
            
            var description = attribute.GetDescription();
            
            Assert.IsNotNull(description);
            Assert.IsNotEmpty(description);
            Assert.IsTrue(description.Contains("AI_INTEGRATION"));
            Assert.IsTrue(description.Contains("throws on disabled"));
            Assert.IsTrue(description.Contains("editor/debug only"));
        }

        [Test]
        public void FeatureGateAttribute_GetDescription_ReflectsConfiguration()
        {
            var attribute = new FeatureGateAttribute(ExperimentalFeature.VOICE_PROCESSING)
            {
                ThrowOnDisabled = false,
                ValidateInRelease = true
            };
            
            var description = attribute.GetDescription();
            
            Assert.IsTrue(description.Contains("VOICE_PROCESSING"));
            Assert.IsTrue(description.Contains("warns on disabled"));
            Assert.IsTrue(description.Contains("validates in release"));
        }

        [Test]
        [Performance]
        public void FeatureGateAttribute_ValidateFeatureAccess_HasMinimalPerformanceOverhead()
        {
            // Test performance when feature is enabled (should be very fast)
            if (!CompilerFlags.AI_ENABLED)
            {
                Assert.Ignore("Cannot test performance when feature is compile-time disabled");
                return;
            }

            flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            var attribute = new FeatureGateAttribute(ExperimentalFeature.AI_INTEGRATION);
            
            const int iterations = 10000;
            var startTime = Time.realtimeSinceStartup;
            
            for (int i = 0; i < iterations; i++)
            {
                attribute.ValidateFeatureAccess();
            }
            
            var endTime = Time.realtimeSinceStartup;
            var totalTime = endTime - startTime;
            var timePerValidation = totalTime / iterations;
            
            // Should be very fast (less than 10 microseconds per validation)
            Assert.Less(timePerValidation, 0.00001f, 
                "Feature gate validation should have minimal performance overhead");
            
            Debug.Log($"Feature gate validation performance: {timePerValidation * 1000000:F2} microseconds per validation");
        }
    }

    // Test classes for feature gate validation

    /// <summary>
    /// Test class with class-level feature gate attribute.
    /// </summary>
    [FeatureGate(ExperimentalFeature.AI_INTEGRATION)]
    public class AIRequiredTestClass
    {
        public void SomeMethod()
        {
            // This method inherits the class-level feature requirement
        }
    }

    /// <summary>
    /// Test class with method-level feature gate attributes.
    /// </summary>
    public class TestClassWithFeatureGates
    {
        [FeatureGate(ExperimentalFeature.AI_INTEGRATION)]
        public void AIRequiredMethod()
        {
            // This method requires AI integration
        }

        [FeatureGate(ExperimentalFeature.VOICE_PROCESSING, 
                     ErrorMessage = "Voice processing is required for this method",
                     ThrowOnDisabled = false)]
        public void VoiceRequiredMethod()
        {
            // This method requires voice processing but only warns when disabled
        }

        [FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS, ValidateInRelease = true)]
        public void AdvancedWaveMethod()
        {
            // This method validates even in release builds
        }

        public void NormalMethod()
        {
            // This method has no feature requirements
        }

        [FeatureGate(ExperimentalFeature.AI_INTEGRATION)]
        public string AIRequiredProperty { get; set; }
    }

    /// <summary>
    /// Test class with multiple feature requirements.
    /// </summary>
    [FeatureGate(ExperimentalFeature.VOICE_PROCESSING)]
    public class VoiceRequiredTestClass
    {
        [FeatureGate(ExperimentalFeature.AI_INTEGRATION)]
        public void MethodWithDifferentRequirement()
        {
            // This method requires AI integration, but the class requires voice processing
            // Both should be validated
        }
    }
}