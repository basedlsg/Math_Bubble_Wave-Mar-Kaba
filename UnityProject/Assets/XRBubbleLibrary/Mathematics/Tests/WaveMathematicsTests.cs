using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using XRBubbleLibrary.Core;

namespace XRBubbleLibrary.Mathematics.Tests
{
    /// <summary>
    /// Comprehensive tests for wave mathematics system.
    /// Tests both core functionality (always available) and experimental features (conditionally available).
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class WaveMathematicsTests
    {
        [Test]
        public void WavePatternGenerator_GenerateWavePattern_ReturnsCorrectCount()
        {
            // Arrange
            int count = 10;
            float radius = 2.0f;
            var parameters = WaveParameters.Default;
            
            // Act
            var positions = WavePatternGenerator.GenerateWavePattern(count, radius, parameters);
            
            // Assert
            Assert.IsNotNull(positions);
            Assert.AreEqual(count, positions.Length);
        }
        
        [Test]
        public void WavePatternGenerator_GenerateWavePattern_PositionsWithinRadius()
        {
            // Arrange
            int count = 8;
            float radius = 1.5f;
            var parameters = WaveParameters.Default;
            
            // Act
            var positions = WavePatternGenerator.GenerateWavePattern(count, radius, parameters);
            
            // Assert
            foreach (var position in positions)
            {
                float distance = Mathf.Sqrt(position.x * position.x + position.z * position.z);
                Assert.LessOrEqual(distance, radius + 0.1f, "Position should be within radius bounds");
            }
        }
        
        [Test]
        public void WavePatternGenerator_CalculateWaveInterference_WithSingleSource()
        {
            // Arrange
            var position = new float3(1, 0, 1);
            var sources = new List<WaveSource>
            {
                new WaveSource(new float3(0, 0, 0), 1.0f, 0.5f, 0f)
            };
            
            // Act
            float interference = WavePatternGenerator.CalculateWaveInterference(position, sources);
            
            // Assert
            Assert.IsTrue(interference >= -0.5f && interference <= 0.5f, 
                         "Interference should be within amplitude bounds");
        }
        
        [Test]
        public void WavePatternGenerator_CalculateWaveInterference_WithMultipleSources()
        {
            // Arrange
            var position = new float3(0, 0, 0);
            var sources = new List<WaveSource>
            {
                new WaveSource(new float3(1, 0, 0), 1.0f, 0.3f, 0f),
                new WaveSource(new float3(-1, 0, 0), 1.0f, 0.3f, 0f),
                new WaveSource(new float3(0, 0, 1), 2.0f, 0.2f, 0f)
            };
            
            // Act
            float interference = WavePatternGenerator.CalculateWaveInterference(position, sources);
            
            // Assert
            Assert.IsTrue(interference >= -0.8f && interference <= 0.8f, 
                         "Multi-source interference should be within expected bounds");
        }
        
        [Test]
        public void WaveParameters_Default_HasValidValues()
        {
            // Act
            var defaultParams = WaveParameters.Default;
            
            // Assert
            Assert.Greater(defaultParams.primaryFrequency, 0f);
            Assert.Greater(defaultParams.primaryAmplitude, 0f);
            Assert.Greater(defaultParams.secondaryFrequency, 0f);
            Assert.Greater(defaultParams.secondaryAmplitude, 0f);
            Assert.Greater(defaultParams.tertiaryFrequency, 0f);
            Assert.Greater(defaultParams.tertiaryAmplitude, 0f);
            Assert.Greater(defaultParams.baseHeight, 0f);
        }
        
        [Test]
        public void WaveSource_Default_HasValidValues()
        {
            // Act
            var defaultSource = WaveSource.Default;
            
            // Assert
            Assert.Greater(defaultSource.frequency, 0f);
            Assert.Greater(defaultSource.amplitude, 0f);
            Assert.AreEqual(0f, defaultSource.phase);
            Assert.AreEqual(float3.zero, defaultSource.position);
        }
        
        [Test]
        public void WaveSource_Constructor_SetsValuesCorrectly()
        {
            // Arrange
            var position = new float3(1, 2, 3);
            float frequency = 2.5f;
            float amplitude = 0.8f;
            float phase = 1.57f;
            
            // Act
            var source = new WaveSource(position, frequency, amplitude, phase);
            
            // Assert
            Assert.AreEqual(position, source.position);
            Assert.AreEqual(frequency, source.frequency);
            Assert.AreEqual(amplitude, source.amplitude);
            Assert.AreEqual(phase, source.phase);
        }
        
        [Test]
        public void AdvancedWaveSystem_ExistsAndCanBeInstantiated()
        {
            // Arrange
            var gameObject = new GameObject("TestAdvancedWaveSystem");
            
            // Act
            var advancedWaveSystem = gameObject.AddComponent<AdvancedWaveSystem>();
            
            // Assert
            Assert.IsNotNull(advancedWaveSystem);
            
            // Cleanup
            Object.DestroyImmediate(gameObject);
        }
        
        [Test]
        public void AdvancedWaveSystem_GetPerformanceMetrics_ReturnsValidMetrics()
        {
            // Arrange
            var gameObject = new GameObject("TestAdvancedWaveSystem");
            var advancedWaveSystem = gameObject.AddComponent<AdvancedWaveSystem>();
            
            // Act
            var metrics = advancedWaveSystem.GetPerformanceMetrics();
            
            // Assert
            Assert.IsNotNull(metrics);
            Assert.GreaterOrEqual(metrics.lastComputeTimeMs, 0f);
            Assert.GreaterOrEqual(metrics.activeBubbleCount, 0);
            Assert.GreaterOrEqual(metrics.currentBiasFieldSize, 0);
            
            // Cleanup
            Object.DestroyImmediate(gameObject);
        }
        
#if EXP_ADVANCED_WAVE
        [Test]
        public void AdvancedWaveSystem_GenerateOptimalBubblePositions_WithAdvancedWaveEnabled()
        {
            // This test only runs when advanced wave algorithms are enabled
            
            // Arrange
            var gameObject = new GameObject("TestAdvancedWaveSystem");
            var advancedWaveSystem = gameObject.AddComponent<AdvancedWaveSystem>();
            int bubbleCount = 10;
            var userPosition = new float3(0, 1.6f, 0);
            
            // Act
            var positionsTask = advancedWaveSystem.GenerateOptimalBubblePositions(bubbleCount, userPosition);
            positionsTask.Wait();
            var positions = positionsTask.Result;
            
            // Assert
            Assert.IsNotNull(positions);
            Assert.AreEqual(bubbleCount, positions.Length);
            
            // Verify positions are reasonable (not all zero, within reasonable bounds)
            bool hasNonZeroPositions = false;
            foreach (var pos in positions)
            {
                if (math.lengthsq(pos) > 0.01f)
                {
                    hasNonZeroPositions = true;
                    break;
                }
            }
            Assert.IsTrue(hasNonZeroPositions, "Should generate non-zero positions");
            
            // Cleanup
            Object.DestroyImmediate(gameObject);
        }
        
        [Test]
        public void AdvancedWaveSystem_RegisterBubble_WithAdvancedWaveEnabled()
        {
            // This test only runs when advanced wave algorithms are enabled
            
            // Arrange
            var gameObject = new GameObject("TestAdvancedWaveSystem");
            var advancedWaveSystem = gameObject.AddComponent<AdvancedWaveSystem>();
            var initialPosition = new float3(1, 2, 3);
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => advancedWaveSystem.RegisterBubble(1, initialPosition));
            Assert.DoesNotThrow(() => advancedWaveSystem.UnregisterBubble(1));
            
            // Cleanup
            Object.DestroyImmediate(gameObject);
        }
        
        [Test]
        public void AdvancedWaveSystem_ForceUpdate_WithAdvancedWaveEnabled()
        {
            // This test only runs when advanced wave algorithms are enabled
            
            // Arrange
            var gameObject = new GameObject("TestAdvancedWaveSystem");
            var advancedWaveSystem = gameObject.AddComponent<AdvancedWaveSystem>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => advancedWaveSystem.ForceUpdate());
            
            // Cleanup
            Object.DestroyImmediate(gameObject);
        }
        
        [Test]
        public void AdvancedWaveSystem_SetAIOptimization_WithAdvancedWaveEnabled()
        {
            // This test only runs when advanced wave algorithms are enabled
            
            // Arrange
            var gameObject = new GameObject("TestAdvancedWaveSystem");
            var advancedWaveSystem = gameObject.AddComponent<AdvancedWaveSystem>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => advancedWaveSystem.SetAIOptimization(true));
            Assert.DoesNotThrow(() => advancedWaveSystem.SetAIOptimization(false));
            
            // Cleanup
            Object.DestroyImmediate(gameObject);
        }
#else
        [Test]
        public void AdvancedWaveSystem_GenerateOptimalBubblePositions_WithAdvancedWaveDisabled()
        {
            // This test only runs when advanced wave algorithms are disabled
            
            // Arrange
            var gameObject = new GameObject("TestAdvancedWaveSystem");
            var advancedWaveSystem = gameObject.AddComponent<AdvancedWaveSystem>();
            int bubbleCount = 5;
            var userPosition = new float3(0, 1.6f, 0);
            
            // Act
            var positionsTask = advancedWaveSystem.GenerateOptimalBubblePositions(bubbleCount, userPosition);
            positionsTask.Wait();
            var positions = positionsTask.Result;
            
            // Assert - Should still return positions (fallback implementation)
            Assert.IsNotNull(positions);
            Assert.AreEqual(bubbleCount, positions.Length);
            
            // Cleanup
            Object.DestroyImmediate(gameObject);
        }
        
        [Test]
        public void AdvancedWaveSystem_StubMethods_WithAdvancedWaveDisabled()
        {
            // This test only runs when advanced wave algorithms are disabled
            
            // Arrange
            var gameObject = new GameObject("TestAdvancedWaveSystem");
            var advancedWaveSystem = gameObject.AddComponent<AdvancedWaveSystem>();
            
            // Act & Assert - Stub methods should not throw
            Assert.DoesNotThrow(() => advancedWaveSystem.RegisterBubble(1, float3.zero));
            Assert.DoesNotThrow(() => advancedWaveSystem.UnregisterBubble(1));
            Assert.DoesNotThrow(() => advancedWaveSystem.ForceUpdate());
            Assert.DoesNotThrow(() => advancedWaveSystem.SetAIOptimization(true));
            
            // Metrics should return disabled state
            var metrics = advancedWaveSystem.GetPerformanceMetrics();
            Assert.AreEqual(0f, metrics.lastComputeTimeMs);
            Assert.AreEqual(0, metrics.activeBubbleCount);
            Assert.IsFalse(metrics.aiOptimizationEnabled);
            Assert.IsFalse(metrics.jobSystemEnabled);
            Assert.AreEqual(0, metrics.currentBiasFieldSize);
            
            // Cleanup
            Object.DestroyImmediate(gameObject);
        }
#endif
        
        [Test]
        public void CompilerFlags_AdvancedWaveAlgorithms_StateIsConsistent()
        {
            // Act
            bool isEnabled = CompilerFlags.IsFeatureEnabled(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
            
            // Assert - Should match compile-time state
#if EXP_ADVANCED_WAVE
            Assert.IsTrue(isEnabled, "Advanced wave algorithms should be enabled when EXP_ADVANCED_WAVE is defined");
#else
            Assert.IsFalse(isEnabled, "Advanced wave algorithms should be disabled when EXP_ADVANCED_WAVE is not defined");
#endif
        }
        
        [Test]
        public void CymaticsController_CanBeInstantiated()
        {
            // Arrange
            var gameObject = new GameObject("TestCymaticsController");
            
            // Act
            var cymaticsController = gameObject.AddComponent<CymaticsController>();
            
            // Assert
            Assert.IsNotNull(cymaticsController);
            
            // Cleanup
            Object.DestroyImmediate(gameObject);
        }
        
        [Test]
        public void WaveSystemMetrics_ToString_ReturnsValidString()
        {
            // Arrange
            var metrics = new WaveSystemMetrics
            {
                lastComputeTimeMs = 1.5f,
                activeBubbleCount = 10,
                aiOptimizationEnabled = true,
                jobSystemEnabled = true,
                currentBiasFieldSize = 5
            };
            
            // Act
            string result = metrics.ToString();
            
            // Assert
            Assert.IsNotEmpty(result);
            Assert.That(result, Contains.Substring("Wave System"));
            Assert.That(result, Contains.Substring("1.50")); // Compute time
            Assert.That(result, Contains.Substring("10")); // Bubble count
        }
    }
}