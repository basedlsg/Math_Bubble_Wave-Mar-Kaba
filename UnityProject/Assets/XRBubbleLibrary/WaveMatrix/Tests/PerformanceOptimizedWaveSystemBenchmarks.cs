using NUnit.Framework;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace XRBubbleLibrary.WaveMatrix.Tests
{
    /// <summary>
    /// Performance benchmark tests for PerformanceOptimizedWaveSystem.
    /// Validates Quest 3 hardware performance requirements and optimization effectiveness.
    /// Implements Requirement 5.2: Performance-optimized algorithms for Quest 3.
    /// Implements Requirement 5.3: Burst compilation validation.
    /// Implements Requirement 5.4: Performance stub testing with Unity profiler.
    /// </summary>
    [Category("Performance")]
    [Category("Benchmark")]
    public class PerformanceOptimizedWaveSystemBenchmarks
    {
        private IPerformanceOptimizedWaveSystem _waveSystem;
        private WaveMatrixSettings _testSettings;
        
        // Quest 3 performance targets
        private const float QUEST3_TARGET_FRAME_TIME_MS = 13.89f; // 72 FPS
        private const float WAVE_CALC_BUDGET_MS = 2.0f; // Budget for wave calculations per frame
        private const int QUEST3_TARGET_BUBBLES = 100;
        
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
        
        #region Burst vs Non-Burst Performance Comparison
        
        [Test]
        [Performance]
        public void Benchmark_BurstVsSIMD_PerformanceComparison()
        {
            // Arrange
            const int bubbleCount = 100;
            const int iterations = 100;
            
            using var gridIndices = new NativeArray<int>(bubbleCount, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var burstResults = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            using var simdResults = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            
            // Initialize test data
            for (int i = 0; i < bubbleCount; i++)
            {
                gridIndices[i] = i;
                distanceOffsets[i] = UnityEngine.Random.Range(0f, 3f);
            }
            
            // Warm up both methods
            for (int i = 0; i < 5; i++)
            {
                _waveSystem.CalculateWavePositionsBurst(gridIndices, 1f, distanceOffsets, _testSettings, burstResults);
                _waveSystem.CalculateWavePositionsSIMD(gridIndices, 1f, distanceOffsets, _testSettings, simdResults);
            }
            
            // Benchmark Burst implementation
            var burstStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _waveSystem.CalculateWavePositionsBurst(gridIndices, i * 0.016f, distanceOffsets, _testSettings, burstResults);
            }
            burstStopwatch.Stop();
            
            // Benchmark SIMD implementation
            var simdStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _waveSystem.CalculateWavePositionsSIMD(gridIndices, i * 0.016f, distanceOffsets, _testSettings, simdResults);
            }
            simdStopwatch.Stop();
            
            // Calculate performance metrics
            float burstAvgMs = (float)burstStopwatch.ElapsedMilliseconds / iterations;
            float simdAvgMs = (float)simdStopwatch.ElapsedMilliseconds / iterations;
            float performanceRatio = burstAvgMs / simdAvgMs;
            
            // Assert
            Assert.Less(burstAvgMs, WAVE_CALC_BUDGET_MS, 
                $"Burst implementation should meet performance budget: {burstAvgMs:F3}ms");
            Assert.Less(simdAvgMs, WAVE_CALC_BUDGET_MS, 
                $"SIMD implementation should meet performance budget: {simdAvgMs:F3}ms");
            
            UnityEngine.Debug.Log($"[Benchmark] Burst: {burstAvgMs:F3}ms, SIMD: {simdAvgMs:F3}ms, " +
                                $"Ratio: {performanceRatio:F2}x, Budget: {WAVE_CALC_BUDGET_MS}ms");
        }
        
        #endregion
        
        #region Scalability Benchmarks
        
        [Test]
        [Performance]
        public void Benchmark_ScalabilityTest_VariousBubbleCounts()
        {
            // Arrange
            int[] bubbleCounts = { 10, 25, 50, 100, 150, 200 };
            var results = new List<(int count, float timeMs, bool meetsTarget)>();
            
            // Act
            foreach (int bubbleCount in bubbleCounts)
            {
                var performanceResult = _waveSystem.ValidateQuest3Performance(bubbleCount, WAVE_CALC_BUDGET_MS);
                results.Add((bubbleCount, performanceResult.ActualFrameTimeMs, performanceResult.MeetsPerformanceTarget));
                
                UnityEngine.Debug.Log($"[Scalability] {bubbleCount} bubbles: {performanceResult.ActualFrameTimeMs:F3}ms " +
                                    $"(meets target: {performanceResult.MeetsPerformanceTarget})");
            }
            
            // Assert
            var targetResult = results.FirstOrDefault(r => r.count == QUEST3_TARGET_BUBBLES);
            Assert.IsTrue(targetResult.meetsTarget, 
                $"Should meet performance target for {QUEST3_TARGET_BUBBLES} bubbles");
            
            // Verify linear scaling (approximately)
            for (int i = 1; i < results.Count - 1; i++)
            {
                float scalingFactor = (float)results[i].count / results[i-1].count;
                float timeRatio = results[i].timeMs / results[i-1].timeMs;
                
                // Allow some variance in scaling due to overhead and optimizations
                Assert.Less(timeRatio, scalingFactor * 1.5f, 
                    $"Performance should scale reasonably from {results[i-1].count} to {results[i].count} bubbles");
            }
        }
        
        [Test]
        [Performance]
        public void Benchmark_BatchSizeOptimization_FindsOptimalBatchSize()
        {
            // Arrange
            const int totalBubbles = 100;
            int[] batchSizes = { 1, 4, 8, 16, 32, 64, 100 };
            var results = new List<(int batchSize, float avgTimeMs)>();
            
            using var gridIndices = new NativeArray<int>(totalBubbles, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(totalBubbles, Allocator.TempJob);
            using var positions = new NativeArray<float3>(totalBubbles, Allocator.TempJob);
            
            // Initialize test data
            for (int i = 0; i < totalBubbles; i++)
            {
                gridIndices[i] = i;
                distanceOffsets[i] = UnityEngine.Random.Range(0f, 3f);
            }
            
            // Test different batch sizes
            foreach (int batchSize in batchSizes)
            {
                const int iterations = 50;
                var stopwatch = Stopwatch.StartNew();
                
                for (int iter = 0; iter < iterations; iter++)
                {
                    // Process in batches
                    for (int start = 0; start < totalBubbles; start += batchSize)
                    {
                        int end = math.min(start + batchSize, totalBubbles);
                        int currentBatchSize = end - start;
                        
                        using var batchIndices = new NativeArray<int>(currentBatchSize, Allocator.TempJob);
                        using var batchOffsets = new NativeArray<float>(currentBatchSize, Allocator.TempJob);
                        using var batchResults = new NativeArray<float3>(currentBatchSize, Allocator.TempJob);
                        
                        // Copy batch data
                        for (int i = 0; i < currentBatchSize; i++)
                        {
                            batchIndices[i] = gridIndices[start + i];
                            batchOffsets[i] = distanceOffsets[start + i];
                        }
                        
                        _waveSystem.CalculateWavePositionsBurst(batchIndices, iter * 0.016f, 
                            batchOffsets, _testSettings, batchResults);
                    }
                }
                
                stopwatch.Stop();
                float avgTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
                results.Add((batchSize, avgTimeMs));
                
                UnityEngine.Debug.Log($"[BatchSize] {batchSize}: {avgTimeMs:F3}ms average");
            }
            
            // Assert
            var optimalResult = results.OrderBy(r => r.avgTimeMs).First();
            Assert.Less(optimalResult.avgTimeMs, WAVE_CALC_BUDGET_MS, 
                "Optimal batch size should meet performance budget");
            
            UnityEngine.Debug.Log($"[BatchSize] Optimal batch size: {optimalResult.batchSize} " +
                                $"({optimalResult.avgTimeMs:F3}ms)");
        }
        
        #endregion
        
        #region Memory Performance Benchmarks
        
        [Test]
        [Performance]
        public void Benchmark_MemoryAllocation_MinimizesGCPressure()
        {
            // Arrange
            const int bubbleCount = 100;
            const int iterations = 1000;
            
            using var gridIndices = new NativeArray<int>(bubbleCount, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var results = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            
            for (int i = 0; i < bubbleCount; i++)
            {
                gridIndices[i] = i;
                distanceOffsets[i] = UnityEngine.Random.Range(0f, 3f);
            }
            
            // Measure initial memory
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long initialMemory = System.GC.GetTotalMemory(false);
            
            // Act - Perform many calculations
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _waveSystem.CalculateWavePositionsBurst(gridIndices, i * 0.016f, distanceOffsets, _testSettings, results);
                
                // Occasionally test other operations
                if (i % 100 == 0)
                {
                    _waveSystem.UpdateWaveState(0.016f, _testSettings);
                    var stats = _waveSystem.GetPerformanceStats();
                }
            }
            stopwatch.Stop();
            
            // Measure final memory
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long finalMemory = System.GC.GetTotalMemory(false);
            
            // Assert
            long memoryGrowth = finalMemory - initialMemory;
            float avgTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            
            Assert.Less(memoryGrowth, 1024 * 1024, // 1MB max growth
                $"Memory growth should be minimal: {memoryGrowth / 1024}KB");
            Assert.Less(avgTimeMs, WAVE_CALC_BUDGET_MS, 
                $"Average calculation time should meet budget: {avgTimeMs:F3}ms");
            
            UnityEngine.Debug.Log($"[Memory] {iterations} iterations: {memoryGrowth / 1024}KB growth, " +
                                $"{avgTimeMs:F3}ms average");
        }
        
        #endregion
        
        #region Real-World Scenario Benchmarks
        
        [Test]
        [Performance]
        public void Benchmark_RealisticUsagePattern_MaintainsPerformance()
        {
            // Arrange - Simulate realistic VR usage pattern
            const int bubbleCount = 100;
            const int totalFrames = 1000; // ~16 seconds at 60 FPS
            
            using var gridIndices = new NativeArray<int>(bubbleCount, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var results = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            using var positions = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            using var heights = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var normals = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            
            // Initialize with realistic data distribution
            for (int i = 0; i < bubbleCount; i++)
            {
                gridIndices[i] = i;
                // Realistic AI confidence distribution
                if (i < bubbleCount * 0.3f) // 30% high confidence
                    distanceOffsets[i] = UnityEngine.Random.Range(0f, 0.5f);
                else if (i < bubbleCount * 0.7f) // 40% medium confidence
                    distanceOffsets[i] = UnityEngine.Random.Range(0.5f, 1.5f);
                else // 30% low confidence
                    distanceOffsets[i] = UnityEngine.Random.Range(1.5f, 3f);
            }
            
            var frameTimes = new List<float>();
            var stopwatch = Stopwatch.StartNew();
            
            // Act - Simulate realistic frame processing
            for (int frame = 0; frame < totalFrames; frame++)
            {
                float currentTime = frame * 0.016f; // 60 FPS
                var frameStopwatch = Stopwatch.StartNew();
                
                // Main wave position calculation (every frame)
                _waveSystem.CalculateWavePositionsBurst(gridIndices, currentTime, distanceOffsets, _testSettings, results);
                
                // Copy positions for other calculations
                for (int i = 0; i < bubbleCount; i++)
                {
                    positions[i] = results[i];
                }
                
                // Height calculation (every frame for physics)
                _waveSystem.CalculateWaveHeightsBatch(positions, currentTime, _testSettings, heights);
                
                // Normal calculation (every few frames for lighting)
                if (frame % 3 == 0)
                {
                    _waveSystem.CalculateWaveNormalsBatch(positions, currentTime, _testSettings, normals);
                }
                
                // Wave state update (every frame)
                _waveSystem.UpdateWaveState(0.016f, _testSettings);
                
                // Performance validation (occasionally)
                if (frame % 100 == 0)
                {
                    var stats = _waveSystem.GetPerformanceStats();
                }
                
                frameStopwatch.Stop();
                frameTimes.Add((float)frameStopwatch.ElapsedMilliseconds);
            }
            
            stopwatch.Stop();
            
            // Calculate statistics
            float avgFrameTime = frameTimes.Average();
            float maxFrameTime = frameTimes.Max();
            float p95FrameTime = frameTimes.OrderBy(t => t).Skip((int)(frameTimes.Count * 0.95)).First();
            int framesOverBudget = frameTimes.Count(t => t > WAVE_CALC_BUDGET_MS);
            
            // Assert
            Assert.Less(avgFrameTime, WAVE_CALC_BUDGET_MS, 
                $"Average frame time should meet budget: {avgFrameTime:F3}ms");
            Assert.Less(p95FrameTime, WAVE_CALC_BUDGET_MS * 1.5f, 
                $"95th percentile should be reasonable: {p95FrameTime:F3}ms");
            Assert.Less(framesOverBudget, totalFrames * 0.05f, 
                $"<5% of frames should exceed budget: {framesOverBudget}/{totalFrames}");
            
            UnityEngine.Debug.Log($"[Realistic] {totalFrames} frames: Avg={avgFrameTime:F3}ms, " +
                                $"Max={maxFrameTime:F3}ms, P95={p95FrameTime:F3}ms, " +
                                $"OverBudget={framesOverBudget} ({(float)framesOverBudget/totalFrames:P1})");
        }
        
        #endregion
        
        #region Optimization Effectiveness Benchmarks
        
        [Test]
        [Performance]
        public void Benchmark_OptimizationEffectiveness_ShowsImprovement()
        {
            // Arrange
            const int bubbleCount = 100;
            const int iterations = 100;
            
            using var gridIndices = new NativeArray<int>(bubbleCount, Allocator.TempJob);
            using var distanceOffsets = new NativeArray<float>(bubbleCount, Allocator.TempJob);
            using var results = new NativeArray<float3>(bubbleCount, Allocator.TempJob);
            
            for (int i = 0; i < bubbleCount; i++)
            {
                gridIndices[i] = i;
                distanceOffsets[i] = UnityEngine.Random.Range(0f, 3f);
            }
            
            // Test without warm-up (cold performance)
            using var coldSystem = new PerformanceOptimizedWaveSystem();
            var coldStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                coldSystem.CalculateWavePositionsBurst(gridIndices, i * 0.016f, distanceOffsets, _testSettings, results);
            }
            coldStopwatch.Stop();
            float coldAvgMs = (float)coldStopwatch.ElapsedMilliseconds / iterations;
            coldSystem.Dispose();
            
            // Test with warm-up (optimized performance)
            var warmStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _waveSystem.CalculateWavePositionsBurst(gridIndices, i * 0.016f, distanceOffsets, _testSettings, results);
            }
            warmStopwatch.Stop();
            float warmAvgMs = (float)warmStopwatch.ElapsedMilliseconds / iterations;
            
            // Calculate improvement
            float improvementRatio = coldAvgMs / warmAvgMs;
            float improvementPercent = (1f - warmAvgMs / coldAvgMs) * 100f;
            
            // Assert
            Assert.Greater(improvementRatio, 1.1f, 
                $"Warm system should be at least 10% faster: {improvementRatio:F2}x");
            Assert.Less(warmAvgMs, WAVE_CALC_BUDGET_MS, 
                $"Optimized system should meet performance budget: {warmAvgMs:F3}ms");
            
            UnityEngine.Debug.Log($"[Optimization] Cold: {coldAvgMs:F3}ms, Warm: {warmAvgMs:F3}ms, " +
                                $"Improvement: {improvementRatio:F2}x ({improvementPercent:F1}%)");
        }
        
        #endregion
        
        #region Stress Test Benchmarks
        
        [Test]
        [Performance]
        public void Benchmark_StressTest_ExtremeBubbleCounts()
        {
            // Arrange - Test with extreme bubble counts
            int[] extremeCounts = { 500, 1000, 2000 };
            var results = new List<(int count, float timeMs, bool stable)>();
            
            foreach (int bubbleCount in extremeCounts)
            {
                bool testStable = true;
                float totalTime = 0f;
                const int iterations = 10;
                
                try
                {
                    for (int iter = 0; iter < iterations; iter++)
                    {
                        var performanceResult = _waveSystem.ValidateQuest3Performance(bubbleCount, 100f); // Generous timeout
                        totalTime += performanceResult.ActualFrameTimeMs;
                        
                        if (!performanceResult.BurstCompilationActive)
                        {
                            testStable = false;
                            break;
                        }
                    }
                    
                    float avgTime = totalTime / iterations;
                    results.Add((bubbleCount, avgTime, testStable));
                    
                    UnityEngine.Debug.Log($"[Stress] {bubbleCount} bubbles: {avgTime:F2}ms average " +
                                        $"(stable: {testStable})");
                }
                catch (System.Exception ex)
                {
                    results.Add((bubbleCount, float.MaxValue, false));
                    UnityEngine.Debug.LogWarning($"[Stress] {bubbleCount} bubbles failed: {ex.Message}");
                }
            }
            
            // Assert - System should handle at least 500 bubbles gracefully
            var result500 = results.FirstOrDefault(r => r.count == 500);
            if (result500.count > 0)
            {
                Assert.IsTrue(result500.stable, "System should remain stable with 500 bubbles");
                Assert.Less(result500.timeMs, 50f, "500 bubbles should complete within reasonable time");
            }
        }
        
        #endregion
    }
}