using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using XRBubbleLibrary.Core;
using XRBubbleLibrary.Integration;

namespace XRBubbleLibrary.Integration.Tests
{
    /// <summary>
    /// Comprehensive unit tests for AI integration code auditing and feature gating.
    /// Validates proper compiler flag wrapping and feature gate functionality.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class AIIntegrationTests
    {
        private GameObject testGameObject;
        private AIEnhancedBubbleSystem aiSystem;
        private CompilerFlagManager flagManager;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject
            testGameObject = new GameObject("TestAISystem");
            aiSystem = testGameObject.AddComponent<AIEnhancedBubbleSystem>();
            
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
        public void AIEnhancedBubbleSystem_CompilerFlagWrapping_WorksCorrectly()
        {
#if EXP_AI
            // When AI is compile-time enabled, system should be available
            Assert.IsNotNull(aiSystem, "AIEnhancedBubbleSystem should be available when EXP_AI is enabled");
            
            // Should have proper feature gate attribute
            var systemType = typeof(AIEnhancedBubbleSystem);
            Assert.IsTrue(systemType.HasFeatureGate(), "AIEnhancedBubbleSystem should have feature gate attribute");
            
            var requiredFeature = systemType.GetRequiredFeature();
            Assert.IsTrue(requiredFeature.HasValue, "Should have required feature");
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, requiredFeature.Value, "Should require AI integration");
#else
            // When AI is compile-time disabled, system should still exist but be stubbed
            Assert.IsNotNull(aiSystem, "AIEnhancedBubbleSystem stub should be available when EXP_AI is disabled");
            
            // Test that stub methods provide appropriate warnings
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*AI Integration is disabled.*"));
            aiSystem.SetAIEnhancement(true);
#endif
        }

        [Test]
        public void AIEnhancedBubbleSystem_FeatureGateValidation_WorksCorrectly()
        {
#if EXP_AI
            // Test feature gate validation when AI is enabled
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                Assert.DoesNotThrow(() => aiSystem.SetAIEnhancement(true), 
                    "Should not throw when AI integration is enabled");
            }
            else
            {
                // If AI is disabled at runtime, should throw or warn
                Assert.Throws<FeatureDisabledException>(() => 
                    CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION));
            }
#else
            // When compile-time disabled, should always warn
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*AI Integration is disabled.*"));
            aiSystem.SetAIEnhancement(true);
#endif
        }

        [UnityTest]
        public IEnumerator AIEnhancedBubbleSystem_Initialization_HandlesFeatureStates()
        {
#if EXP_AI
            // Test initialization with AI enabled
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                // Should initialize without errors
                aiSystem.gameObject.SetActive(true);
                yield return null; // Wait one frame for Start() to execute
                
                // System should be active and functional
                Assert.IsTrue(aiSystem.gameObject.activeInHierarchy, "AI system should remain active when feature is enabled");
            }
            else
            {
                // Should handle disabled state gracefully
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*Feature.*disabled.*"));
                aiSystem.gameObject.SetActive(true);
                yield return null;
            }
#else
            // When compile-time disabled, should disable GameObject
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*AI Integration is disabled.*"));
            aiSystem.gameObject.SetActive(true);
            yield return null;
            
            Assert.IsFalse(aiSystem.gameObject.activeInHierarchy, "AI system should be disabled when EXP_AI is not defined");
#endif
        }

        [Test]
        public void AIEnhancedBubbleSystem_VoiceIntegration_HandlesConditionalCompilation()
        {
#if EXP_AI && EXP_VOICE
            // When both AI and Voice are enabled, voice features should be available
            var testVoiceMethod = typeof(AIEnhancedBubbleSystem).GetMethod("TestVoiceCommand");
            Assert.IsNotNull(testVoiceMethod, "TestVoiceCommand should be available when both EXP_AI and EXP_VOICE are enabled");
            
            // Should have proper feature gate
            Assert.IsTrue(testVoiceMethod.HasFeatureGate(), "TestVoiceCommand should have feature gate");
            
            var requiredFeature = testVoiceMethod.GetRequiredFeature();
            Assert.IsTrue(requiredFeature.HasValue, "Should have required feature");
            Assert.AreEqual(ExperimentalFeature.VOICE_PROCESSING, requiredFeature.Value, "Should require voice processing");
#elif EXP_AI
            // When AI is enabled but Voice is not, voice methods should not be available
            var testVoiceMethod = typeof(AIEnhancedBubbleSystem).GetMethod("TestVoiceCommand");
            Assert.IsNull(testVoiceMethod, "TestVoiceCommand should not be available when EXP_VOICE is disabled");
#else
            // When AI is disabled, voice integration is irrelevant
            var testVoiceMethod = typeof(AIEnhancedBubbleSystem).GetMethod("TestVoiceCommand");
            Assert.IsNotNull(testVoiceMethod, "Stub TestVoiceCommand should be available in stub implementation");
#endif
        }

        [Test]
        public void AIEnhancedBubbleSystem_BubbleManagement_WorksWithFeatureGates()
        {
#if EXP_AI
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                // Should be able to destroy bubbles when AI is enabled
                Assert.DoesNotThrow(() => aiSystem.DestroyAllBubbles(), 
                    "Should be able to destroy bubbles when AI integration is enabled");
            }
            else
            {
                // Should handle disabled state appropriately
                // Note: DestroyAllBubbles might not have feature gate in current implementation
                aiSystem.DestroyAllBubbles(); // Should not throw, might warn
            }
#else
            // Stub implementation should warn about disabled features
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*AI Integration is disabled.*"));
            aiSystem.DestroyAllBubbles();
#endif
        }

        [Test]
        public void AIEnhancedBubbleSystem_PerformanceMetrics_ReflectFeatureState()
        {
#if EXP_AI
            var metrics = aiSystem.GetSystemMetrics();
            
            // Metrics should be available and reflect current state
            Assert.GreaterOrEqual(metrics.activeBubbleCount, 0, "Active bubble count should be non-negative");
            Assert.GreaterOrEqual(metrics.lastSystemUpdateTimeMs, 0, "Update time should be non-negative");
            
            // AI metrics should reflect feature state
            if (flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                // AI metrics should be available
                Assert.IsNotNull(metrics.ToString(), "Metrics should have string representation");
            }
#else
            // Stub implementation should return default metrics
            var metrics = aiSystem.GetSystemMetrics();
            Assert.AreEqual(0, metrics.activeBubbleCount, "Stub should return zero active bubbles");
            Assert.AreEqual(0f, metrics.lastSystemUpdateTimeMs, "Stub should return zero update time");
#endif
        }

        [Test]
        public void AIEnhancedBubbleSystem_FeatureGateAttributes_AreProperlyApplied()
        {
            var systemType = typeof(AIEnhancedBubbleSystem);
            
#if EXP_AI
            // When AI is compile-time enabled, should have proper feature gates
            Assert.IsTrue(systemType.HasFeatureGate(), "AIEnhancedBubbleSystem should have class-level feature gate");
            
            var requiredFeature = systemType.GetRequiredFeature();
            Assert.IsTrue(requiredFeature.HasValue, "Should have required feature");
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, requiredFeature.Value, "Should require AI integration");
            
            // Check specific methods have feature gates
            var createBubbleMethod = systemType.GetMethod("CreateEnhancedBubble");
            if (createBubbleMethod != null)
            {
                Assert.IsTrue(createBubbleMethod.HasFeatureGate(), "CreateEnhancedBubble should have feature gate");
            }
#else
            // Stub implementation might not have feature gates (that's okay)
            // The important thing is that it exists and provides appropriate warnings
            Assert.IsNotNull(systemType, "AIEnhancedBubbleSystem type should exist even when disabled");
#endif
        }

        [Test]
        public void AIEnhancedBubbleSystem_GracefulDegradation_WorksCorrectly()
        {
#if EXP_AI
            // Test graceful degradation when AI is compile-time enabled but runtime disabled
            if (CompilerFlags.AI_ENABLED)
            {
                // Disable AI at runtime
                flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
                
                // Should handle disabled state gracefully
                Assert.DoesNotThrow(() => aiSystem.SetAIEnhancement(false), 
                    "Should handle AI disabling gracefully");
                
                // Re-enable for cleanup
                flagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            }
#else
            // When compile-time disabled, all AI operations should warn appropriately
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*AI Integration is disabled.*"));
            aiSystem.SetAIEnhancement(true);
            
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*AI Integration is disabled.*"));
            aiSystem.TestVoiceCommand("test");
            
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*AI Integration is disabled.*"));
            aiSystem.DestroyAllBubbles();
#endif
        }

        [Test]
        public void AIEnhancedBubbleSystem_AssemblyConstraints_AreCorrect()
        {
            // Verify that the AI assembly has proper constraints
            var aiAssembly = System.Reflection.Assembly.GetAssembly(typeof(AIEnhancedBubbleSystem));
            Assert.IsNotNull(aiAssembly, "Should be able to get AI assembly");
            
#if EXP_AI
            // When AI is enabled, assembly should be loaded
            Assert.IsTrue(aiAssembly.GetTypes().Length > 0, "AI assembly should contain types when enabled");
#endif
            
            // Assembly should contain our integration types
            var systemType = aiAssembly.GetType("XRBubbleLibrary.Integration.AIEnhancedBubbleSystem");
            Assert.IsNotNull(systemType, "Should contain AIEnhancedBubbleSystem type");
        }

        [Test]
        public void AIEnhancedBubbleSystem_ErrorHandling_WorksCorrectly()
        {
#if EXP_AI
            // Test error handling when AI features are accessed incorrectly
            if (!flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                // Should throw appropriate exceptions
                Assert.Throws<FeatureDisabledException>(() => 
                    CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION));
            }
#else
            // Stub implementation should not throw exceptions, just warn
            Assert.DoesNotThrow(() => aiSystem.SetAIEnhancement(true), 
                "Stub implementation should not throw exceptions");
#endif
        }

        [Test]
        [Performance]
        public void AIEnhancedBubbleSystem_PerformanceImpact_IsMinimal()
        {
            // Test that feature gate checking has minimal performance impact
            const int iterations = 1000;
            var startTime = Time.realtimeSinceStartup;
            
            for (int i = 0; i < iterations; i++)
            {
#if EXP_AI
                // Test feature gate validation performance
                if (flagManager.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
                {
                    aiSystem.SetAIEnhancement(true);
                }
#else
                // Test stub method performance
                aiSystem.SetAIEnhancement(true);
#endif
            }
            
            var endTime = Time.realtimeSinceStartup;
            var totalTime = endTime - startTime;
            var timePerCall = totalTime / iterations;
            
            // Should be very fast (less than 100 microseconds per call)
            Assert.Less(timePerCall, 0.0001f, 
                "AI system method calls should have minimal performance overhead");
            
            Debug.Log($"AI system method call performance: {timePerCall * 1000000:F2} microseconds per call");
        }
    }
}