using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using System.Diagnostics;

namespace XRBubbleLibrary.WaveMatrix.Tests
{
    /// <summary>
    /// Performance benchmark tests for WaveMatrixCore.
    /// Validates Quest 3 performance requirements and optimization effectiveness.
    /// Implements Requirement 5.2: Performance-optimized algorithms for Quest 3.
    /// </summary>
    [Category("Performance")]
    public class WaveMatrixPerformanceTests
    {
        private IWaveMatrixCore _waveCore;
        private WaveMatrixSettings _testSettings;
        
        // Quest 3 performance targets
        private const float TARGET_FPS = 72f;
        private const float MAX_FRAME_TIME_MS = 13.89f; // ~72 FPS
        private const float MAX_WAVE_CALC_TIME_MS = 1.0f; // Budget for wave calculations per frame
        private const int QUEST3_MAX_BUBBLES = 100; // Target for 100-bubble demo
        
        [SetUp]
        public void SetUp()
        {
            _waveCore = new WaveMatrixCore();
            _testSettings = WaveMatrixSettings.Default;
        }
        
        [TearDown]
        public void TearDown()
        {
            _waveCore = null;
            _testSettings = null;
        }
        
        #region Single Position Performance Tests
        
        [Test]
        [Performance]
        public void CalculateWavePosition_SingleBubble_MeetsQuest3Performance()
        {
            // Arrange
            int bubbleIndex = 5;
            float time = 1.0f;
            int iterations = 10000; // Simulate many calculations per second
            
            // Warm up
            for (int i = 0; i < 100; i++)
            {
                _waveCore.CalculateWavePosition(bubbleIndex, time, _testSettings);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.CalculateWavePosition(bubbleIndex, time + i * 0.001f, _testSettings);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            float maxAllowedTimeMs = MAX_WAVE_CALC_TIME_MS / QUEST3_MAX_BUBBLES; // Budget per bubble
            
            Assert.Less(averageTimeMs, maxAllowedTimeMs, 
                $"Single bubble calculation ({averageTimeMs:F6}ms) exceeds Quest 3 budget ({maxAllowedTimeMs:F6}ms)");
            
            UnityEngine.Debug.Log($"[Performance] Single bubble calculation: {averageTimeMs:F6}ms (target: <{maxAllowedTimeMs:F6}ms)");
        }
        
        [Test]
        [Performance]
        public void CalculateWavePosition_WithDistanceOffset_MaintainsPerformance()
        {
            // Arrange
            int bubbleIndex = 5;
            float time = 1.0f;
            float distanceOffset = 2.0f;
            int iterations = 10000;
            
            // Warm up
            for (int i = 0; i < 100; i++)
            {
                _waveCore.CalculateWavePosition(bubbleIndex, time, distanceOffset, _testSettings);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.CalculateWavePosition(bubbleIndex, time + i * 0.001f, distanceOffset, _testSettings);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            float maxAllowedTimeMs = MAX_WAVE_CALC_TIME_MS / QUEST3_MAX_BUBBLES;
            
            Assert.Less(averageTimeMs, maxAllowedTimeMs, 
                $"Distance offset calculation ({averageTimeMs:F6}ms) exceeds Quest 3 budget ({maxAllowedTimeMs:F6}ms)");
            
            UnityEngine.Debug.Log($"[Performance] Distance offset calculation: {averageTimeMs:F6}ms (target: <{maxAllowedTimeMs:F6}ms)");
        }
        
        #endregion
        
        #region Batch Processing Performance Tests
        
        [Test]
        [Performance]
        public void CalculateWavePositionsBatch_100Bubbles_MeetsQuest3FrameTime()
        {
            // Arrange
            int batchSize = QUEST3_MAX_BUBBLES;
            int[] bubbleIndices = new int[batchSize];
            float3[] results = new float3[batchSize];
            
            for (int i = 0; i < batchSize; i++)
            {
                bubbleIndices[i] = i;
            }
            
            float time = 1.0f;
            int iterations = 1000; // Simulate 1000 frames
            
            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _waveCore.CalculateWavePositionsBatch(bubbleIndices, time, _testSettings, results);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.CalculateWavePositionsBatch(bubbleIndices, time + i * 0.016f, _testSettings, results);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            
            Assert.Less(averageTimeMs, MAX_WAVE_CALC_TIME_MS, 
                $"100-bubble batch calculation ({averageTimeMs:F3}ms) exceeds Quest 3 budget ({MAX_WAVE_CALC_TIME_MS}ms)");
            
            UnityEngine.Debug.Log($"[Performance] 100-bubble batch: {averageTimeMs:F3}ms (target: <{MAX_WAVE_CALC_TIME_MS}ms)");
            
            // Calculate effective FPS impact
            float frameTimeImpact = (averageTimeMs / MAX_FRAME_TIME_MS) * 100f;
            Assert.Less(frameTimeImpact, 10f, $"Wave calculations should use <10% of frame time, actual: {frameTimeImpact:F1}%");
            
            UnityEngine.Debug.Log($"[Performance] Frame time impact: {frameTimeImpact:F1}% (target: <10%)");
        }
        
        [Test]
        [Performance]
        public void CalculateWavePositionsBatch_WithDistanceOffsets_MaintainsPerformance()
        {
            // Arrange
            int batchSize = QUEST3_MAX_BUBBLES;
            int[] bubbleIndices = new int[batchSize];
            float[] distanceOffsets = new float[batchSize];
            float3[] results = new float3[batchSize];
            
            for (int i = 0; i < batchSize; i++)
            {
                bubbleIndices[i] = i;
                distanceOffsets[i] = UnityEngine.Random.Range(0f, 5f); // Random AI distances
            }
            
            float time = 1.0f;
            int iterations = 1000;
            
            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _waveCore.CalculateWavePositionsBatch(bubbleIndices, time, distanceOffsets, _testSettings, results);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.CalculateWavePositionsBatch(bubbleIndices, time + i * 0.016f, distanceOffsets, _testSettings, results);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            
            Assert.Less(averageTimeMs, MAX_WAVE_CALC_TIME_MS, 
                $"100-bubble batch with offsets ({averageTimeMs:F3}ms) exceeds Quest 3 budget ({MAX_WAVE_CALC_TIME_MS}ms)");
            
            UnityEngine.Debug.Log($"[Performance] 100-bubble batch with offsets: {averageTimeMs:F3}ms (target: <{MAX_WAVE_CALC_TIME_MS}ms)");
        }
        
        #endregion
        
        #region Wave Height Performance Tests
        
        [Test]
        [Performance]
        public void CalculateWaveHeight_HighFrequency_MaintainsPerformance()
        {
            // Arrange
            float2 worldPosition = new float2(1, 1);
            float time = 1.0f;
            int iterations = 50000; // High frequency calculations
            
            // Warm up
            for (int i = 0; i < 100; i++)
            {
                _waveCore.CalculateWaveHeight(worldPosition, time, _testSettings);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.CalculateWaveHeight(worldPosition, time + i * 0.0001f, _testSettings);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            float maxAllowedTimeMs = 0.001f; // Very tight budget for height calculations
            
            Assert.Less(averageTimeMs, maxAllowedTimeMs, 
                $"Wave height calculation ({averageTimeMs:F6}ms) exceeds performance budget ({maxAllowedTimeMs:F6}ms)");
            
            UnityEngine.Debug.Log($"[Performance] Wave height calculation: {averageTimeMs:F6}ms (target: <{maxAllowedTimeMs:F6}ms)");
        }
        
        #endregion
        
        #region Memory Performance Tests
        
        [Test]
        [Performance]
        public void CalculateWavePositionsBatch_MemoryAllocation_MinimalGCPressure()
        {
            // Arrange
            int batchSize = QUEST3_MAX_BUBBLES;
            int[] bubbleIndices = new int[batchSize];
            float3[] results = new float3[batchSize];
            
            for (int i = 0; i < batchSize; i++)
            {
                bubbleIndices[i] = i;
            }
            
            float time = 1.0f;
            int iterations = 100;
            
            // Force GC before test
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long initialMemory = System.GC.GetTotalMemory(false);
            
            // Act
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.CalculateWavePositionsBatch(bubbleIndices, time + i * 0.016f, _testSettings, results);
            }
            
            // Assert
            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryIncrease = finalMemory - initialMemory;
            
            // Allow for some memory increase but it should be minimal
            float memoryIncreaseKB = memoryIncrease / 1024f;
            Assert.Less(memoryIncreaseKB, 10f, $"Memory increase ({memoryIncreaseKB:F1}KB) should be minimal to avoid GC pressure");
            
            UnityEngine.Debug.Log($"[Performance] Memory increase: {memoryIncreaseKB:F1}KB (target: <10KB)");
        }
        
        #endregion
        
        #region Scaling Performance Tests
        
        [Test]
        [Performance]
        public void CalculateWavePositionsBatch_ScalingTest_LinearPerformance()
        {
            // Test different batch sizes to verify linear scaling
            int[] batchSizes = { 10, 25, 50, 75, 100 };
            float[] timesMs = new float[batchSizes.Length];
            
            for (int b = 0; b < batchSizes.Length; b++)
            {
                int batchSize = batchSizes[b];
                int[] bubbleIndices = new int[batchSize];
                float3[] results = new float3[batchSize];
                
                for (int i = 0; i < batchSize; i++)
                {
                    bubbleIndices[i] = i;
                }
                
                float time = 1.0f;
                int iterations = 1000;
                
                // Warm up
                for (int i = 0; i < 10; i++)
                {
                    _waveCore.CalculateWavePositionsBatch(bubbleIndices, time, _testSettings, results);
                }
                
                // Measure
                var stopwatch = Stopwatch.StartNew();
                
                for (int i = 0; i < iterations; i++)
                {
                    _waveCore.CalculateWavePositionsBatch(bubbleIndices, time + i * 0.016f, _testSettings, results);
                }
                
                stopwatch.Stop();
                timesMs[b] = (float)stopwatch.ElapsedMilliseconds / iterations;
                
                UnityEngine.Debug.Log($"[Performance] {batchSize} bubbles: {timesMs[b]:F3}ms");
            }
            
            // Verify roughly linear scaling
            float timePerBubble10 = timesMs[0] / batchSizes[0];
            float timePerBubble100 = timesMs[4] / batchSizes[4];
            
            float scalingRatio = timePerBubble100 / timePerBubble10;
            
            // Should be close to 1.0 for linear scaling, allow some variance
            Assert.Less(scalingRatio, 2.0f, $"Performance scaling ratio ({scalingRatio:F2}) indicates non-linear performance degradation");
            
            UnityEngine.Debug.Log($"[Performance] Scaling ratio (100 vs 10 bubbles): {scalingRatio:F2} (target: <2.0)");
        }
        
        #endregion
        
        #region Complex Settings Performance Tests
        
        [Test]
        [Performance]
        public void CalculateWavePosition_ComplexSettings_MaintainsPerformance()
        {
            // Arrange - Create complex settings that stress the system
            var complexSettings = WaveMatrixSettings.Dynamic; // Use dynamic preset
            complexSettings.enableInterference = true;
            complexSettings.primaryWave.frequency = 2.0f;
            complexSettings.secondaryWave.frequency = 1.5f;
            complexSettings.tertiaryWave.frequency = 3.0f;
            
            int bubbleIndex = 5;
            float time = 1.0f;
            int iterations = 10000;
            
            // Warm up
            for (int i = 0; i < 100; i++)
            {
                _waveCore.CalculateWavePosition(bubbleIndex, time, complexSettings);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.CalculateWavePosition(bubbleIndex, time + i * 0.001f, complexSettings);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            float maxAllowedTimeMs = (MAX_WAVE_CALC_TIME_MS / QUEST3_MAX_BUBBLES) * 1.5f; // Allow 50% overhead for complex settings
            
            Assert.Less(averageTimeMs, maxAllowedTimeMs, 
                $"Complex settings calculation ({averageTimeMs:F6}ms) exceeds Quest 3 budget ({maxAllowedTimeMs:F6}ms)");
            
            UnityEngine.Debug.Log($"[Performance] Complex settings: {averageTimeMs:F6}ms (target: <{maxAllowedTimeMs:F6}ms)");
        }
        
        #endregion
        
        #region Grid Position Caching Performance Tests
        
        [Test]
        [Performance]
        public void GetGridPosition_CachingEffectiveness_ImprovedPerformance()
        {
            // Test that repeated calls to GetGridPosition benefit from caching
            int bubbleIndex = 5;
            int iterations = 10000;
            
            // First run (cache miss)
            var stopwatch1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.GetGridPosition(bubbleIndex, _testSettings);
            }
            stopwatch1.Stop();
            
            // Second run (cache hit)
            var stopwatch2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.GetGridPosition(bubbleIndex, _testSettings);
            }
            stopwatch2.Stop();
            
            float firstRunMs = (float)stopwatch1.ElapsedMilliseconds / iterations;
            float secondRunMs = (float)stopwatch2.ElapsedMilliseconds / iterations;
            
            // Second run should be faster due to caching
            Assert.LessOrEqual(secondRunMs, firstRunMs, "Cached grid position calls should be faster or equal");
            
            UnityEngine.Debug.Log($"[Performance] Grid position - First run: {firstRunMs:F6}ms, Cached run: {secondRunMs:F6}ms");
        }
        
        #endregion
        
        #region Real-World Scenario Tests
        
        [Test]
        [Performance]
        public void RealWorldScenario_100BubblesAt72FPS_MeetsQuest3Requirements()
        {
            // Simulate real-world usage: 100 bubbles updating at 72 FPS
            int bubbleCount = QUEST3_MAX_BUBBLES;
            int[] bubbleIndices = new int[bubbleCount];
            float[] distanceOffsets = new float[bubbleCount];
            float3[] results = new float3[bubbleCount];
            
            // Setup realistic bubble distribution
            for (int i = 0; i < bubbleCount; i++)
            {
                bubbleIndices[i] = i;
                distanceOffsets[i] = UnityEngine.Random.Range(0f, 3f); // Realistic AI confidence distances
            }
            
            float frameTime = 1f / TARGET_FPS; // 72 FPS frame time
            int frameCount = 100; // Test 100 frames
            
            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _waveCore.CalculateWavePositionsBatch(bubbleIndices, i * frameTime, distanceOffsets, _testSettings, results);
            }
            
            // Act - Simulate real frame updates
            var stopwatch = Stopwatch.StartNew();
            
            for (int frame = 0; frame < frameCount; frame++)
            {
                float currentTime = frame * frameTime;
                _waveCore.UpdateWaveTime(frameTime, _testSettings);
                _waveCore.CalculateWavePositionsBatch(bubbleIndices, currentTime, distanceOffsets, _testSettings, results);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageFrameTimeMs = (float)stopwatch.ElapsedMilliseconds / frameCount;
            float frameTimePercentage = (averageFrameTimeMs / MAX_FRAME_TIME_MS) * 100f;
            
            Assert.Less(averageFrameTimeMs, MAX_WAVE_CALC_TIME_MS, 
                $"Real-world scenario ({averageFrameTimeMs:F3}ms) exceeds wave calculation budget ({MAX_WAVE_CALC_TIME_MS}ms)");
            
            Assert.Less(frameTimePercentage, 10f, 
                $"Wave calculations should use <10% of frame time, actual: {frameTimePercentage:F1}%");
            
            UnityEngine.Debug.Log($"[Performance] Real-world 100 bubbles @ 72 FPS:");
            UnityEngine.Debug.Log($"  - Average frame time: {averageFrameTimeMs:F3}ms (budget: {MAX_WAVE_CALC_TIME_MS}ms)");
            UnityEngine.Debug.Log($"  - Frame time usage: {frameTimePercentage:F1}% (target: <10%)");
            UnityEngine.Debug.Log($"  - Effective FPS impact: {(averageFrameTimeMs / MAX_FRAME_TIME_MS) * TARGET_FPS:F1} FPS");
        }
        
        #endregion
    }
}