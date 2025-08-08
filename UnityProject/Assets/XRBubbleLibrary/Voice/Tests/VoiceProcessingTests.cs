using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using XRBubbleLibrary.Core;
using XRBubbleLibrary.Voice;

namespace XRBubbleLibrary.Voice.Tests
{
    /// <summary>
    /// Comprehensive unit tests for voice processing code auditing and feature gating.
    /// Validates proper compiler flag wrapping and feature gate functionality.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class VoiceProcessingTests
    {
        private GameObject testGameObject;
        private OnDeviceVoiceProcessor voiceProcessor;
        private CompilerFlagManager flagManager;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject
            testGameObject = new GameObject("TestVoiceProcessor");
            voiceProcessor = testGameObject.AddComponent<OnDeviceVoiceProcessor>();
            
            flagManager = CompilerFlagManager.Instance;
            flagManager.ResetToDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            
            flagManager?.ResetToDefaults();
        }

        [Test]
        public void OnDeviceVoiceProcessor_CompilerFlagWrapping_WorksCorrectly()
        {
#if EXP_VOICE
            // When Voice is compile-time enabled, system should be available
            Assert.IsNotNull(voiceProcessor, "OnDeviceVoiceProcessor should be available when EXP_VOICE is enabled");
            
            // Should have proper feature gate attribute
            var processorType = typeof(OnDeviceVoiceProcessor);
            Assert.IsTrue(processorType.HasFeatureGate(), "OnDeviceVoiceProcessor should have feature gate attribute");
            
            var requiredFeature = processorType.GetRequiredFeature();
            Assert.IsTrue(requiredFeature.HasValue, "Should have required feature");
            Assert.AreEqual(ExperimentalFeature.VOICE_PROCESSING, requiredFeature.Value, "Should require voice processing");
#else
            // When Voice is compile-time disabled, system should still exist but be stubbed
            Assert.IsNotNull(voiceProcessor, "OnDeviceVoiceProcessor stub should be available when EXP_VOICE is disabled");
            
            // Test that stub methods provide appropriate warnings
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Voice Processing is disabled.*"));
            voiceProcessor.StartListening();
#endif
        }

        [Test]
        public void OnDeviceVoiceProcessor_FeatureGateValidation_WorksCorrectly()
        {
#if EXP_VOICE
            // Test feature gate validation when Voice is enabled
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.VOICE_PROCESSING))
            {
                Assert.DoesNotThrow(() => voiceProcessor.StartListening(), 
                    "Should not throw when voice processing is enabled");
            }
            else
            {
                // If Voice is disabled at runtime, should throw or warn
                Assert.Throws<FeatureDisabledException>(() => 
                    CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.VOICE_PROCESSING));
            }
#else
            // When compile-time disabled, should always warn
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Voice Processing is disabled.*"));
            voiceProcessor.StartListening();
#endif
        }

        [UnityTest]
        public IEnumerator OnDeviceVoiceProcessor_Initialization_HandlesFeatureStates()
        {
#if EXP_VOICE
            // Test initialization with Voice enabled
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.VOICE_PROCESSING))
            {
                // Should initialize without errors
                voiceProcessor.gameObject.SetActive(true);
                yield return null; // Wait one frame for Start() to execute
                
                // System should be active and functional
                Assert.IsTrue(voiceProcessor.gameObject.activeInHierarchy, "Voice processor should remain active when feature is enabled");
            }
            else
            {
                // Should handle disabled state gracefully
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*Feature.*disabled.*"));
                voiceProcessor.gameObject.SetActive(true);
                yield return null;
            }
#else
            // When compile-time disabled, should disable GameObject
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Voice Processing is disabled.*"));
            voiceProcessor.gameObject.SetActive(true);
            yield return null;
            
            Assert.IsFalse(voiceProcessor.gameObject.activeInHierarchy, "Voice processor should be disabled when EXP_VOICE is not defined");
#endif
        }

        [Test]
        public void OnDeviceVoiceProcessor_AIIntegration_HandlesConditionalCompilation()
        {
#if EXP_VOICE && EXP_AI
            // When both Voice and AI are enabled, AI features should be available
            Assert.DoesNotThrow(() => voiceProcessor.SetAIEnhancement(true), 
                "Should be able to enable AI enhancement when both EXP_VOICE and EXP_AI are enabled");
#elif EXP_VOICE
            // When Voice is enabled but AI is not, AI methods should warn appropriately
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*EXP_AI is disabled.*"));
            voiceProcessor.SetAIEnhancement(true);
#else
            // When Voice is disabled, AI integration is irrelevant
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Voice Processing is disabled.*"));
            voiceProcessor.SetAIEnhancement(true);
#endif
        }

        [Test]
        public void OnDeviceVoiceProcessor_VoiceCommands_WorkWithFeatureGates()
        {
#if EXP_VOICE
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.VOICE_PROCESSING))
            {
                // Should be able to process test commands when Voice is enabled
                Assert.DoesNotThrow(() => voiceProcessor.ProcessTestCommand("test arrange"), 
                    "Should be able to process test commands when voice processing is enabled");
            }
            else
            {
                // Should handle disabled state appropriately
                voiceProcessor.ProcessTestCommand("test arrange"); // Should not throw, might warn
            }
#else
            // Stub implementation should warn about disabled features
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Voice Processing is disabled.*"));
            voiceProcessor.ProcessTestCommand("test arrange");
#endif
        }

        [Test]
        public void OnDeviceVoiceProcessor_PerformanceMetrics_ReflectFeatureState()
        {
#if EXP_VOICE
            var metrics = voiceProcessor.GetPerformanceMetrics();
            
            // Metrics should be available and reflect current state
            Assert.GreaterOrEqual(metrics.totalCommandsProcessed, 0, "Total commands processed should be non-negative");
            Assert.GreaterOrEqual(metrics.lastProcessingTimeMs, 0, "Processing time should be non-negative");
            
            // Voice metrics should reflect feature state
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.VOICE_PROCESSING))
            {
                // Voice metrics should be available
                Assert.IsNotNull(metrics.ToString(), "Metrics should have string representation");
            }
#else
            // Stub implementation should return default metrics
            var metrics = voiceProcessor.GetPerformanceMetrics();
            Assert.AreEqual(0, metrics.totalCommandsProcessed, "Stub should return zero processed commands");
            Assert.AreEqual(0f, metrics.lastProcessingTimeMs, "Stub should return zero processing time");
            Assert.IsFalse(metrics.isListening, "Stub should not be listening");
            Assert.IsFalse(metrics.isProcessing, "Stub should not be processing");
#endif
        }

        [Test]
        public void OnDeviceVoiceProcessor_FeatureGateAttributes_AreProperlyApplied()
        {
            var processorType = typeof(OnDeviceVoiceProcessor);
            
#if EXP_VOICE
            // When Voice is compile-time enabled, should have proper feature gates
            Assert.IsTrue(processorType.HasFeatureGate(), "OnDeviceVoiceProcessor should have class-level feature gate");
            
            var requiredFeature = processorType.GetRequiredFeature();
            Assert.IsTrue(requiredFeature.HasValue, "Should have required feature");
            Assert.AreEqual(ExperimentalFeature.VOICE_PROCESSING, requiredFeature.Value, "Should require voice processing");
            
            // Check specific methods have feature gates
            var startListeningMethod = processorType.GetMethod("StartListening");
            if (startListeningMethod != null)
            {
                Assert.IsTrue(startListeningMethod.HasFeatureGate(), "StartListening should have feature gate");
            }
            
            var processTestCommandMethod = processorType.GetMethod("ProcessTestCommand");
            if (processTestCommandMethod != null)
            {
                Assert.IsTrue(processTestCommandMethod.HasFeatureGate(), "ProcessTestCommand should have feature gate");
            }
#else
            // Stub implementation might not have feature gates (that's okay)
            // The important thing is that it exists and provides appropriate warnings
            Assert.IsNotNull(processorType, "OnDeviceVoiceProcessor type should exist even when disabled");
#endif
        }

        [Test]
        public void OnDeviceVoiceProcessor_GracefulDegradation_WorksCorrectly()
        {
#if EXP_VOICE
            // Test graceful degradation when Voice is compile-time enabled but runtime disabled
            if (CompilerFlags.VOICE_ENABLED)
            {
                // Disable Voice at runtime
                flagManager.SetFeatureState(ExperimentalFeature.VOICE_PROCESSING, false);
                
                // Should handle disabled state gracefully
                Assert.DoesNotThrow(() => voiceProcessor.SetAIEnhancement(false), 
                    "Should handle voice disabling gracefully");
                
                // Re-enable for cleanup
                flagManager.SetFeatureState(ExperimentalFeature.VOICE_PROCESSING, true);
            }
#else
            // When compile-time disabled, all voice operations should warn appropriately
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Voice Processing is disabled.*"));
            voiceProcessor.StartListening();
            
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Voice Processing is disabled.*"));
            voiceProcessor.StopListeningAndProcess();
            
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Voice Processing is disabled.*"));
            voiceProcessor.ProcessTestCommand("test");
            
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Voice Processing is disabled.*"));
            voiceProcessor.SetAIEnhancement(true);
#endif
        }

        [Test]
        public void OnDeviceVoiceProcessor_AssemblyConstraints_AreCorrect()
        {
            // Verify that the Voice assembly has proper constraints
            var voiceAssembly = System.Reflection.Assembly.GetAssembly(typeof(OnDeviceVoiceProcessor));
            Assert.IsNotNull(voiceAssembly, "Should be able to get Voice assembly");
            
#if EXP_VOICE
            // When Voice is enabled, assembly should be loaded
            Assert.IsTrue(voiceAssembly.GetTypes().Length > 0, "Voice assembly should contain types when enabled");
#endif
            
            // Assembly should contain our voice processing types
            var processorType = voiceAssembly.GetType("XRBubbleLibrary.Voice.OnDeviceVoiceProcessor");
            Assert.IsNotNull(processorType, "Should contain OnDeviceVoiceProcessor type");
        }

        [Test]
        public void OnDeviceVoiceProcessor_ErrorHandling_WorksCorrectly()
        {
#if EXP_VOICE
            // Test error handling when Voice features are accessed incorrectly
            if (!flagManager.IsFeatureEnabled(ExperimentalFeature.VOICE_PROCESSING))
            {
                // Should throw appropriate exceptions
                Assert.Throws<FeatureDisabledException>(() => 
                    CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.VOICE_PROCESSING));
            }
#else
            // Stub implementation should not throw exceptions, just warn
            Assert.DoesNotThrow(() => voiceProcessor.StartListening(), 
                "Stub implementation should not throw exceptions");
#endif
        }

        [Test]
        public void SpatialCommand_DataStructure_IsConsistent()
        {
            // Test that SpatialCommand structure is available regardless of feature state
            var spatialCommand = new SpatialCommand
            {
                action = SpatialAction.Arrange,
                positions = new Unity.Mathematics.float3[] { Unity.Mathematics.float3.zero },
                confidence = 0.8f,
                sourceCommand = "test arrange"
            };
            
            Assert.AreEqual(SpatialAction.Arrange, spatialCommand.action, "Should be able to set spatial action");
            Assert.AreEqual(1, spatialCommand.positions.Length, "Should be able to set positions array");
            Assert.AreEqual(0.8f, spatialCommand.confidence, "Should be able to set confidence");
            Assert.AreEqual("test arrange", spatialCommand.sourceCommand, "Should be able to set source command");
        }

        [Test]
        public void SpatialAction_Enum_HasExpectedValues()
        {
            // Verify that all expected spatial actions are defined
            var expectedActions = new[]
            {
                SpatialAction.Arrange,
                SpatialAction.Create,
                SpatialAction.Delete,
                SpatialAction.Move
            };
            
            var actualActions = System.Enum.GetValues(typeof(SpatialAction));
            
            Assert.AreEqual(expectedActions.Length, actualActions.Length, 
                "Should have expected number of spatial actions");
            
            foreach (var expected in expectedActions)
            {
                Assert.IsTrue(System.Array.Exists(actualActions.Cast<SpatialAction>().ToArray(), action => action == expected),
                    $"Should contain expected spatial action: {expected}");
            }
        }

        [Test]
        public void VoiceProcessingMetrics_ToString_WorksCorrectly()
        {
#if EXP_VOICE
            var metrics = voiceProcessor.GetPerformanceMetrics();
            var metricsString = metrics.ToString();
            
            Assert.IsNotNull(metricsString, "Metrics should have string representation");
            Assert.IsTrue(metricsString.Contains("Voice Processing"), "Should contain voice processing identifier");
#else
            var metrics = voiceProcessor.GetPerformanceMetrics();
            var metricsString = metrics.ToString();
            
            Assert.IsNotNull(metricsString, "Stub metrics should have string representation");
            Assert.IsTrue(metricsString.Contains("DISABLED"), "Should indicate disabled state");
#endif
        }

        [Test]
        public void OnDeviceVoiceProcessor_CompilerFlagIntegration_WorksCorrectly()
        {
            // Test integration with compiler flag system
            var voiceEnabled = CompilerFlags.VOICE_ENABLED;
            
#if EXP_VOICE
            Assert.IsTrue(voiceEnabled, "Voice should be enabled when EXP_VOICE is defined");
#else
            Assert.IsFalse(voiceEnabled, "Voice should be disabled when EXP_VOICE is not defined");
#endif
        }

        [Test]
        public void OnDeviceVoiceProcessor_FeatureValidation_WorksCorrectly()
        {
#if EXP_VOICE
            // Test feature validation system integration
            var isVoiceSupported = FeatureValidationSystem.IsFeatureSupported(ExperimentalFeature.VOICE_PROCESSING);
            
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.VOICE_PROCESSING))
            {
                Assert.IsTrue(isVoiceSupported, "Voice should be supported when enabled");
            }
            else
            {
                Assert.IsFalse(isVoiceSupported, "Voice should not be supported when disabled");
            }
#else
            // When compile-time disabled, feature should not be supported
            var isVoiceSupported = FeatureValidationSystem.IsFeatureSupported(ExperimentalFeature.VOICE_PROCESSING);
            Assert.IsFalse(isVoiceSupported, "Voice should not be supported when EXP_VOICE is not defined");
#endif
        }
    }
}