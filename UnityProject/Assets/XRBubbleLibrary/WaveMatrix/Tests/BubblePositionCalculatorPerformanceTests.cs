using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XRBubbleLibrary.WaveMatrix.Tests
{
    /// <summary>
    /// Performance benchmark tests for BubblePositionCalculator.
    /// Validates Quest 3 performance requirements for 100+ bubble positioning.
    /// Implements Requirement 6.1: Efficient algorithms for 100+ bubble positioning.
    /// </summary>
    [Category("Performance")]
    public class BubblePositionCalculatorPerformanceTests
    {
        private IBubblePositionCalculator _calculator;
        private WaveMatrixSettings _testSettings;
        
        // Quest 3 performance targets
        private const float TARGET_FPS = 72f;
        private const float MAX_FRAME_TIME_MS = 13.89f; // ~72 FPS
        private const float MAX_POSITION_CALC_TIME_MS = 2.0f; // Budget for position calculations per frame
        private const int QUEST3_MAX_BUBBLES = 100; // Target for 100-bubble demo
        
        [SetUp]
        public void SetUp()
        {
            _calculator = new BubblePositionCalculator();
            _testSettings = WaveMatrixSettings.Default;
        }
        
        [TearDown]
        public void TearDown()
        {
            _calculator?.ClearCaches();
            _calculator = null;
            _testSettings = null;
        }
        
        #region Single Frame Performance Tests
        
        [Test]
        [Performance]
        public void CalculateAllPositions_100Bubbles_MeetsQuest3FrameTime()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(QUEST3_MAX_BUBBLES);
            float time = 1.0f;
            int iterations = 100; // Simulate 100 frames
            
            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _calculator.CalculateAllPositions(bubbleData, time + i * 0.016f, _testSettings);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            
            Assert.Less(averageTimeMs, MAX_POSITION_CALC_TIME_MS, 
                $"Average calculation time {averageTimeMs:F2}ms exceeds Quest 3 target of {MAX_POSITION_CALC_TIME_MS}ms");
            
            UnityEngine.Debug.Log($"[Performance] 100 bubbles: {averageTimeMs:F2}ms average (target: {MAX_POSITION_CALC_TIME_MS}ms)");
            
            // Verify all positions are valid
            var lastPositions = _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
            Assert.AreEqual(QUEST3_MAX_BUBBLES, lastPositions.Length, "Should return positions for all bubbles");
        }
        
        [Test]
        [Performance]
        public void UpdateDirtyPositions_50PercentDirty_MeetsPerformanceTarget()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(QUEST3_MAX_BUBBLES);
            
            // Mark 50% as dirty (realistic scenario)
            for (int i = 0; i < bubbleData.Length; i += 2)
            {
                bubbleData[i].IsDirty = true;
            }
            
            float time = 1.0f;
            int iterations = 100;
            
            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _calculator.UpdateDirtyPositions(bubbleData, time, _testSettings);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _calculator.UpdateDirtyPositions(bubbleData, time + i * 0.016f, _testSettings);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            
            Assert.Less(averageTimeMs, MAX_POSITION_CALC_TIME_MS * 0.5f, 
                $"Dirty position updates {averageTimeMs:F2}ms should be faster than full calculations");
            
            UnityEngine.Debug.Log($"[Performance] 50% dirty updates: {averageTimeMs:F2}ms average");
        }
        
        [Test]
        [Performance]
        public void CalculatePositionsSubset_25Bubbles_ScalesLinearly()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(QUEST3_MAX_BUBBLES);
            int[] subsetIndices = Enumerable.Range(0, 25).ToArray();
            var results = new float3[subsetIndices.Length];
            float time = 1.0f;
            int iterations = 200;
            
            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _calculator.CalculatePositionsSubset(subsetIndices, bubbleData, time, _testSettings, results);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _calculator.CalculatePositionsSubset(subsetIndices, bubbleData, time + i * 0.016f, _testSettings, results);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            float expectedTimeMs = MAX_POSITION_CALC_TIME_MS * 0.25f; // Should scale linearly
            
            Assert.Less(averageTimeMs, expectedTimeMs, 
                $"Subset calculation {averageTimeMs:F2}ms should scale linearly with bubble count");
            
            UnityEngine.Debug.Log($"[Performance] 25 bubble subset: {averageTimeMs:F2}ms average (expected: {expectedTimeMs:F2}ms)");
        }
        
        #endregion
        
        #region Memory Performance Tests
        
        [Test]
        [Performance]
        public void MemoryUsage_100Bubbles_StaysWithinBounds()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(QUEST3_MAX_BUBBLES);
            float time = 1.0f;
            
            // Get initial memory
            long initialMemory = System.GC.GetTotalMemory(true);
            
            // Act - Perform many calculations to test memory growth
            for (int i = 0; i < 1000; i++)
            {
                _calculator.CalculateAllPositions(bubbleData, time + i * 0.016f, _testSettings);
                
                // Occasionally update dirty positions
                if (i % 10 == 0)
                {
                    _calculator.UpdateDirtyPositions(bubbleData, time + i * 0.016f, _testSettings, forceUpdate: true);
                }
            }
            
            // Force garbage collection and measure
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryGrowth = finalMemory - initialMemory;
            
            // Assert
            const long MAX_MEMORY_GROWTH = 1024 * 1024; // 1MB max growth
            
            Assert.Less(memoryGrowth, MAX_MEMORY_GROWTH, 
                $"Memory growth {memoryGrowth / 1024}KB should stay under {MAX_MEMORY_GROWTH / 1024}KB");
            
            UnityEngine.Debug.Log($"[Performance] Memory growth: {memoryGrowth / 1024}KB after 1000 calculations");
        }
        
        [Test]
        [Performance]
        public void CacheEfficiency_RepeatedCalculations_ShowsImprovement()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(50);
            float time = 1.0f;
            
            // First run - cold cache
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
            }
            stopwatch.Stop();
            float coldCacheTime = stopwatch.ElapsedMilliseconds;
            
            // Second run - warm cache (same time, should use cache)
            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
            }
            stopwatch.Stop();
            float warmCacheTime = stopwatch.ElapsedMilliseconds;
            
            // Assert
            float improvementRatio = coldCacheTime / warmCacheTime;
            
            Assert.Greater(improvementRatio, 1.1f, 
                $"Warm cache should be at least 10% faster (ratio: {improvementRatio:F2})");
            
            UnityEngine.Debug.Log($"[Performance] Cache improvement: {improvementRatio:F2}x faster with warm cache");
        }
        
        #endregion
        
        #region Stress Tests
        
        [Test]
        [Performance]
        public void StressTest_200Bubbles_HandlesGracefully()
        {
            // Arrange - Test beyond normal limits
            var bubbleData = CreateTestBubbleData(200);
            float time = 1.0f;
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            Assert.DoesNotThrow(() => {
                var positions = _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
                Assert.AreEqual(200, positions.Length, "Should handle 200 bubbles");
            });
            
            stopwatch.Stop();
            
            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 50, 
                "Even 200 bubbles should complete within reasonable time");
            
            UnityEngine.Debug.Log($"[Performance] 200 bubbles stress test: {stopwatch.ElapsedMilliseconds}ms");
        }
        
        [Test]
        [Performance]
        public void StressTest_RapidUpdates_MaintainsStability()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(QUEST3_MAX_BUBBLES);
            float time = 0f;
            
            // Act - Simulate rapid frame updates
            var stopwatch = Stopwatch.StartNew();
            
            for (int frame = 0; frame < 1000; frame++)
            {
                time += 0.016f; // 60 FPS
                
                // Mix of operations per frame
                _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
                
                if (frame % 5 == 0)
                {
                    _calculator.UpdateDirtyPositions(bubbleData, time, _testSettings);
                }
                
                if (frame % 10 == 0)
                {
                    var stats = _calculator.GetPerformanceStats();
                    Assert.GreaterOrEqual(stats.TotalCalculations, 0, "Stats should remain valid");
                }
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageFrameTime = (float)stopwatch.ElapsedMilliseconds / 1000f;
            
            Assert.Less(averageFrameTime, MAX_POSITION_CALC_TIME_MS, 
                $"Average frame time {averageFrameTime:F2}ms should meet Quest 3 target");
            
            UnityEngine.Debug.Log($"[Performance] 1000 frame stress test: {averageFrameTime:F2}ms average per frame");
        }
        
        #endregion
        
        #region Optimization Tests
        
        [Test]
        [Performance]
        public void OptimizeLayout_100Bubbles_CompletesInReasonableTime()
        {
            // Arrange
            var bubbleData = CreateOverlappingBubbleData(QUEST3_MAX_BUBBLES);
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = _calculator.OptimizeLayout(bubbleData, _testSettings, 10);
            stopwatch.Stop();
            
            // Assert
            Assert.IsTrue(result.Success, "Optimization should succeed");
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, 
                "Layout optimization should complete within 100ms");
            Assert.Less(result.OptimizationTimeMs, 100, 
                "Reported optimization time should be under 100ms");
            
            UnityEngine.Debug.Log($"[Performance] Layout optimization: {result.OptimizationTimeMs:F2}ms, " +
                                $"improved by {result.ImprovementScore:F2}, quality: {result.FinalQualityScore:F2}");
        }
        
        #endregion
        
        #region Spatial Query Performance Tests
        
        [Test]
        [Performance]
        public void FindClosestBubble_100Bubbles_FastLookup()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(QUEST3_MAX_BUBBLES);
            var searchPosition = new float3(0, 0, 0);
            int iterations = 1000;
            
            // Set positions for bubbles
            for (int i = 0; i < bubbleData.Length; i++)
            {
                bubbleData[i].Position = new float3(
                    UnityEngine.Random.Range(-10f, 10f),
                    UnityEngine.Random.Range(-10f, 10f),
                    UnityEngine.Random.Range(-10f, 10f)
                );
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                int closestIndex = _calculator.FindClosestBubble(searchPosition, bubbleData);
                Assert.GreaterOrEqual(closestIndex, -1, "Should return valid index or -1");
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            
            Assert.Less(averageTimeMs, 0.1f, 
                $"Closest bubble lookup {averageTimeMs:F3}ms should be very fast");
            
            UnityEngine.Debug.Log($"[Performance] Closest bubble lookup: {averageTimeMs:F3}ms average");
        }
        
        [Test]
        [Performance]
        public void FindBubblesInRadius_100Bubbles_EfficientSearch()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(QUEST3_MAX_BUBBLES);
            var searchPosition = new float3(0, 0, 0);
            float radius = 5.0f;
            var results = new List<int>();
            int iterations = 500;
            
            // Set positions for bubbles
            for (int i = 0; i < bubbleData.Length; i++)
            {
                bubbleData[i].Position = new float3(
                    UnityEngine.Random.Range(-10f, 10f),
                    UnityEngine.Random.Range(-10f, 10f),
                    UnityEngine.Random.Range(-10f, 10f)
                );
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                int foundCount = _calculator.FindBubblesInRadius(searchPosition, radius, bubbleData, results);
                Assert.GreaterOrEqual(foundCount, 0, "Should return valid count");
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            
            Assert.Less(averageTimeMs, 0.2f, 
                $"Radius search {averageTimeMs:F3}ms should be efficient");
            
            UnityEngine.Debug.Log($"[Performance] Radius search: {averageTimeMs:F3}ms average");
        }
        
        #endregion
        
        #region Helper Methods
        
        private BubbleData[] CreateTestBubbleData(int count)
        {
            var bubbleData = new BubbleData[count];
            
            for (int i = 0; i < count; i++)
            {
                bubbleData[i] = BubbleData.Create(
                    bubbleId: i,
                    gridIndex: i,
                    aiDistance: UnityEngine.Random.Range(0f, 3f)
                );
                bubbleData[i].Radius = 0.5f;
            }
            
            return bubbleData;
        }
        
        private BubbleData[] CreateOverlappingBubbleData(int count)
        {
            var bubbleData = new BubbleData[count];
            
            // Create bubbles in a small area to force overlaps
            for (int i = 0; i < count; i++)
            {
                bubbleData[i] = BubbleData.Create(i, i);
                bubbleData[i].Position = new float3(
                    UnityEngine.Random.Range(-2f, 2f),
                    UnityEngine.Random.Range(-2f, 2f),
                    UnityEngine.Random.Range(-2f, 2f)
                );
                bubbleData[i].Radius = 0.5f;
            }
            
            return bubbleData;
        }
        
        #endregion
    }
}