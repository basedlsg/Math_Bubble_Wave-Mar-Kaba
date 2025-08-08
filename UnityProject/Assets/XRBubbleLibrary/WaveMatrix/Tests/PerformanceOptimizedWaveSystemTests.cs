using NUnit.Framework;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using System.Diagnostics;

namespace XRBubbleLibrary.WaveMatrix.Tests
{
    /// <summary>
    /// Comprehensive tests for PerformanceOptimizedWaveSystem.
    /// Validates Burst compilation, SIMD optimizations, and Quest 3 performance.
    /// Implements Requirement 5.2: Performance-optimized algorithms for Quest 3.
    /// Implements Requirement 5.3: Burst compilation validation.
    /// Implements Requirement 5.4: Performance stub testing with Unity profiler.
    /// </summary>
    public class PerformanceOptimizedWaveSystemTests
    {
        private IPerformanceOptimizedWaveSystem _waveSystem;
        private WaveMatrixSettings _testSettings;
        
        [SetUp]
        public void SetUp()
        {
            _waveSystem = new PerformanceOptimizedWaveSystem();
            _testSettings = WaveMatrixSettings.Default;
            _waveSystem.WarmUp(); // Ensure Burst compilation is active
        }
        
        [TearDown]
        public void TearDown()
        {
            _waveSystem?.Dispose();
            _waveSystem = null;
            _testSettings = null;
        }
        
        #region Burst Compilation Tests
        
        [Test]
        public void BurstCalculation_WithValidData_ProducesCorrectResults()
        {
            // Arrange
            const int bubbleCount = 10;
            using var gridIndices = new NativeArray<int>(bubbleCount, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var results = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            
            for (int i = 0; i < bubbleCount; i++)
            {
                gridIndices[i] = i;
                distanceOffsets[i] = i * 0.5f;
            }
            
            float time = 1.0f;
            
            // Act
            _waveSystem.CalculateWavePositionsBurst(gridIndices, time, distanceOffsets, _testSettings, results);
            
            // Assert
            for (int i = 0; i < bubbleCount; i++)
            {
                var position = results[i];
                Assert.IsTrue(math.isfinite(position.x), $"Position {i} X should be finite");
                Assert.IsTrue(math.isfinite(position.y), $"Position {i} Y should be finite");
                Assert.IsTrue(math.isfinite(position.z), $"Position {i} Z should be finite");
                Assert.AreNotEqual(float3.zero, position, $"Position {i} should not be zero");
            }
        }
        
        [Test]
        public void BurstCalculation_WithDifferentTimes_ProducesAnimatedResults()
        {
            // Arrange
            const int bubbleCount = 5;
            using var gridIndices = new NativeArray<int>(bubbleCount, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var results1 = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            using var results2 = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            
            for (int i = 0; i < bubbleCount; i++)
            {
                gridIndices[i] = i;
                distanceOffsets[i] = 0f;
            }
            
            float time1 = 0f;
            float time2 = 1f;
            
            // Act
            _waveSystem.CalculateWavePositionsBurst(gridIndices, time1, distanceOffsets, _testSettings, results1);
            _waveSystem.CalculateWavePositionsBurst(gridIndices, time2, distanceOffsets, _testSettings, results2);
            
            // Assert
            int changedPositions = 0;
            for (int i = 0; i < bubbleCount; i++)
            {
                if (!results1[i].Equals(results2[i]))
                {
                    changedPositions++;
                }
            }
            
            Assert.Greater(changedPositions, 0, "Wave animation should change positions over time");
            UnityEngine.Debug.Log($"[Burst] {changedPositions}/{bubbleCount} positions changed with time progression");
        }
        
        [Test]
        public void BurstCalculation_WithInvalidArraySizes_HandlesGracefully()
        {
            // Arrange
            using var gridIndices = new NativeArray<int>(5, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(3, Allocator.TempJob); // Different size
            using var results = new NativeArray<float3>(5, Allocator.TempJob);
            
            // Act & Assert - Should not throw, but should handle gracefully
            Assert.DoesNotThrow(() => {
                _waveSystem.CalculateWavePositionsBurst(gridIndices, 1f, distanceOffsets, _testSettings, results);
            });
        }
        
        #endregion
        
        #region SIMD Optimization Tests
        
        [Test]
        public void SIMDCalculation_WithValidData_ProducesCorrectResults()
        {
            // Arrange
            const int bubbleCount = 16; // Multiple of 4 for optimal SIMD
            using var gridIndices = new NativeArray<int>(bubbleCount, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var results = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            
            for (int i = 0; i < bubbleCount; i++)
            {
                gridIndices[i] = i;
                distanceOffsets[i] = i * 0.25f;
            }
            
            float time = 1.0f;
            
            // Act
            _waveSystem.CalculateWavePositionsSIMD(gridIndices, time, distanceOffsets, _testSettings, results);
            
            // Assert
            for (int i = 0; i < bubbleCount; i++)
            {
                var position = results[i];
                Assert.IsTrue(math.isfinite(position.x), $"SIMD Position {i} X should be finite");
                Assert.IsTrue(math.isfinite(position.y), $"SIMD Position {i} Y should be finite");
                Assert.IsTrue(math.isfinite(position.z), $"SIMD Position {i} Z should be finite");
            }
        }
        
        [Test]
        public void SIMDCalculation_ComparedToBurst_ProducesSimilarResults()
        {
            // Arrange
            const int bubbleCount = 8;
            using var gridIndices = new NativeArray<int>(bubbleCount, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var burstResults = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            using var simdResults = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            
            for (int i = 0; i < bubbleCount; i++)
            {
                gridIndices[i] = i;
                distanceOffsets[i] = i * 0.5f;
            }
            
            float time = 1.0f;
            
            // Act
            _waveSystem.CalculateWavePositionsBurst(gridIndices, time, distanceOffsets, _testSettings, burstResults);
            _waveSystem.CalculateWavePositionsSIMD(gridIndices, time, distanceOffsets, _testSettings, simdResults);
            
            // Assert - Results should be very similar (allowing for minor floating point differences)
            for (int i = 0; i < bubbleCount; i++)
            {
                float distance = math.distance(burstResults[i], simdResults[i]);
                Assert.Less(distance, 0.001f, $"Burst and SIMD results should be very similar for bubble {i}");
            }
            
            UnityEngine.Debug.Log("[SIMD] Burst and SIMD calculations produce consistent results");
        }
        
        #endregion
        
        #region Batch Operations Tests
        
        [Test]
        public void CalculateWaveHeightsBatch_WithValidPositions_ProducesReasonableHeights()
        {
            // Arrange
            const int positionCount = 10;
            using var positions = new NativeArray<float3>(positionCount, Allocator.TempJob);
            using var results = new NativeArray<float>(positionCount, Allocator.TempJob);
            
            for (int i = 0; i < positionCount; i++)
            {
                positions[i] = new float3(i, 0, i);
            }
            
            float time = 1.0f;
            
            // Act
            _waveSystem.CalculateWaveHeightsBatch(positions, time, _testSettings, results);
            
            // Assert
            for (int i = 0; i < positionCount; i++)
            {
                float height = results[i];
                Assert.IsTrue(math.isfinite(height), $"Height {i} should be finite");
                Assert.Less(math.abs(height), _testSettings.WaveAmplitude * 3f, 
                    $"Height {i} should be within reasonable bounds");
            }
        }
        
        [Test]
        public void CalculateWaveNormalsBatch_WithValidPositions_ProducesNormalizedVectors()
        {
            // Arrange
            const int positionCount = 8;
            using var positions = new NativeArray<float3>(positionCount, Allocator.TempJob);
            using var results = new NativeArray<float3>(positionCount, Allocator.TempJob);
            
            for (int i = 0; i < positionCount; i++)
            {
                positions[i] = new float3(i * 2f, 0, i * 2f);
            }
            
            float time = 1.0f;
            
            // Act
            _waveSystem.CalculateWaveNormalsBatch(positions, time, _testSettings, results);
            
            // Assert
            for (int i = 0; i < positionCount; i++)
            {
                float3 normal = results[i];
                Assert.IsTrue(math.isfinite(normal.x) && math.isfinite(normal.y) && math.isfinite(normal.z), 
                    $"Normal {i} should be finite");
                
                float length = math.length(normal);
                Assert.Greater(length, 0.9f, $"Normal {i} should be approximately normalized");
                Assert.Less(length, 1.1f, $"Normal {i} should be approximately normalized");
            }
        }
        
        #endregion
        
        #region Quest 3 Performance Tests
        
        [Test]
        [Performance]
        public void Quest3Performance_100Bubbles_MeetsTargetFrameTime()
        {
            // Arrange
            const int bubbleCount = 100;
            const float targetFrameTimeMs = 13.89f; // 72 FPS
            
            // Act
            var result = _waveSystem.ValidateQuest3Performance(bubbleCount, targetFrameTimeMs);
            
            // Assert
            Assert.IsTrue(result.MeetsPerformanceTarget, 
                $"Should meet Quest 3 performance target. Actual: {result.ActualFrameTimeMs:F2}ms, Target: {result.TargetFrameTimeMs:F2}ms");
            Assert.AreEqual(bubbleCount, result.BubbleCount, "Should test with correct bubble count");
            Assert.IsTrue(result.BurstCompilationActive, "Burst compilation should be active");
            
            UnityEngine.Debug.Log($"[Quest3] Performance validation: {result.PerformanceBreakdown}");
        }
        
        [Test]
        [Performance]
        public void Quest3Performance_200Bubbles_HandlesGracefully()
        {
            // Arrange - Test beyond normal limits
            const int bubbleCount = 200;
            const float targetFrameTimeMs = 13.89f;
            
            // Act
            var result = _waveSystem.ValidateQuest3Performance(bubbleCount, targetFrameTimeMs);
            
            // Assert
            Assert.AreEqual(bubbleCount, result.BubbleCount, "Should test with correct bubble count");
            Assert.Greater(result.ActualFrameTimeMs, 0, "Should record actual frame time");
            
            // May not meet target with 200 bubbles, but should not crash
            UnityEngine.Debug.Log($"[Quest3] 200 bubble stress test: {result.ActualFrameTimeMs:F2}ms " +
                                $"(meets target: {result.MeetsPerformanceTarget})");
        }
        
        [Test]
        [Performance]
        public void Quest3Performance_RepeatedCalls_MaintainsConsistency()
        {
            // Arrange
            const int bubbleCount = 50;
            const float targetFrameTimeMs = 13.89f;
            const int iterations = 10;
            
            var results = new WaveSystemPerformanceResult[iterations];
            
            // Act
            for (int i = 0; i < iterations; i++)
            {
                results[i] = _waveSystem.ValidateQuest3Performance(bubbleCount, targetFrameTimeMs);
            }
            
            // Assert
            float averageFrameTime = 0f;
            int successCount = 0;
            
            for (int i = 0; i < iterations; i++)
            {
                averageFrameTime += results[i].ActualFrameTimeMs;
                if (results[i].MeetsPerformanceTarget) successCount++;
            }
            
            averageFrameTime /= iterations;
            float successRate = (float)successCount / iterations;
            
            Assert.Greater(successRate, 0.8f, "Should maintain >80% success rate across multiple runs");
            Assert.Less(averageFrameTime, targetFrameTimeMs * 1.5f, "Average frame time should be reasonable");
            
            UnityEngine.Debug.Log($"[Quest3] Consistency test: {successRate:P0} success rate, {averageFrameTime:F2}ms average");
        }
        
        #endregion
        
        #region Performance Statistics Tests
        
        [Test]
        public void GetPerformanceStats_AfterCalculations_ReturnsValidStats()
        {
            // Arrange
            const int bubbleCount = 20;
            using var gridIndices = new NativeArray<int>(bubbleCount, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var results = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            
            for (int i = 0; i < bubbleCount; i++)
            {
                gridIndices[i] = i;
                distanceOffsets[i] = 0f;
            }
            
            // Act
            _waveSystem.CalculateWavePositionsBurst(gridIndices, 1f, distanceOffsets, _testSettings, results);
            _waveSystem.CalculateWavePositionsSIMD(gridIndices, 1f, distanceOffsets, _testSettings, results);
            
            var stats = _waveSystem.GetPerformanceStats();
            
            // Assert
            Assert.Greater(stats.TotalCalculations, 0, "Should record total calculations");
            Assert.Greater(stats.BurstJobExecutions, 0, "Should record Burst job executions");
            Assert.Greater(stats.SIMDOperations, 0, "Should record SIMD operations");
            Assert.IsTrue(stats.BurstActive, "Burst should be active after warm-up");
            Assert.IsTrue(stats.SIMDActive, "SIMD should be active after warm-up");
            Assert.Greater(stats.AllocatedNativeArrays, 0, "Should track allocated arrays");
            Assert.Greater(stats.MemoryUsageBytes, 0, "Should track memory usage");
            
            UnityEngine.Debug.Log($"[Stats] Calculations: {stats.TotalCalculations}, " +
                                $"Burst: {stats.BurstJobExecutions}, SIMD: {stats.SIMDOperations}, " +
                                $"Memory: {stats.MemoryUsageBytes / 1024}KB");
        }
        
        [Test]
        public void GetPerformanceStats_EfficiencyRatio_IsReasonable()
        {
            // Arrange & Act
            _waveSystem.ValidateQuest3Performance(50, 13.89f); // Trigger some calculations
            var stats = _waveSystem.GetPerformanceStats();
            
            // Assert
            Assert.GreaterOrEqual(stats.EfficiencyRatio, 0f, "Efficiency ratio should be non-negative");
            Assert.LessOrEqual(stats.EfficiencyRatio, 1f, "Efficiency ratio should not exceed 1.0");
            
            UnityEngine.Debug.Log($"[Stats] Efficiency ratio: {stats.EfficiencyRatio:F3}, " +
                                $"Avg time: {stats.AverageCalculationTimeMicroseconds:F1}μs, " +
                                $"Peak time: {stats.PeakCalculationTimeMicroseconds:F1}μs");
        }
        
        #endregion
        
        #region System Lifecycle Tests
        
        [Test]
        public void WarmUp_InitializesSystemCorrectly()
        {
            // Arrange
            using var freshSystem = new PerformanceOptimizedWaveSystem();
            
            // Act
            freshSystem.WarmUp();
            var stats = freshSystem.GetPerformanceStats();
            
            // Assert
            Assert.IsTrue(stats.BurstActive, "Burst should be active after warm-up");
            Assert.IsTrue(stats.SIMDActive, "SIMD should be active after warm-up");
            Assert.Greater(stats.AllocatedNativeArrays, 0, "Should have allocated arrays");
            
            // Cleanup
            freshSystem.Dispose();
        }
        
        [Test]
        public void UpdateWaveState_UpdatesTimeCorrectly()
        {
            // Arrange
            float deltaTime = 0.016f; // 60 FPS
            
            // Act
            _waveSystem.UpdateWaveState(deltaTime, _testSettings);
            var stats = _waveSystem.GetPerformanceStats();
            
            // Assert
            Assert.Greater(stats.LastUpdateTime, 0, "Should update last update time");
        }
        
        [Test]
        public void Dispose_CleansUpResourcesCorrectly()
        {
            // Arrange
            var disposableSystem = new PerformanceOptimizedWaveSystem();
            disposableSystem.WarmUp();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => {
                disposableSystem.Dispose();
                disposableSystem.Dispose(); // Should handle multiple dispose calls
            });
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void CalculateWavePositionsBurst_WithEmptyArrays_HandlesGracefully()
        {
            // Arrange
            using var emptyIndices = new NativeArray<int>(0, Allocator.TempJob);
            using var emptyOffsets = new NativeArray<float>(0, Allocator.TempJob);
            using var emptyResults = new NativeArray<float3>(0, Allocator.TempJob);
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => {
                _waveSystem.CalculateWavePositionsBurst(emptyIndices, 1f, emptyOffsets, _testSettings, emptyResults);
            });
        }
        
        [Test]
        public void ValidateQuest3Performance_WithZeroBubbles_HandlesGracefully()
        {
            // Act
            var result = _waveSystem.ValidateQuest3Performance(0, 13.89f);
            
            // Assert
            Assert.AreEqual(0, result.BubbleCount, "Should handle zero bubbles");
            Assert.GreaterOrEqual(result.ActualFrameTimeMs, 0, "Should return non-negative frame time");
        }
        
        #endregion
    }
}