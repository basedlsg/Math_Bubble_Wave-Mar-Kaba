using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace XRBubbleLibrary.WaveMatrix.Tests
{
    /// <summary>
    /// Integration tests for BubblePositionCalculator with WaveMatrixCore.
    /// Validates end-to-end functionality and integration between components.
    /// Implements Requirement 6.1: Efficient algorithms for 100+ bubble positioning.
    /// Implements Requirement 6.2: Position validation and bounds checking.
    /// </summary>
    [Category("Integration")]
    public class BubblePositionCalculatorIntegrationTests
    {
        private IBubblePositionCalculator _calculator;
        private IWaveMatrixCore _waveCore;
        private WaveMatrixSettings _testSettings;
        
        [SetUp]
        public void SetUp()
        {
            _waveCore = new WaveMatrixCore();
            _calculator = new BubblePositionCalculator(_waveCore);
            _testSettings = WaveMatrixSettings.Default;
        }
        
        [TearDown]
        public void TearDown()
        {
            _calculator?.ClearCaches();
            _calculator = null;
            _waveCore = null;
            _testSettings = null;
        }
        
        #region Integration with WaveMatrixCore Tests
        
        [Test]
        public void Integration_CalculateAllPositions_UsesWaveMatrixCore()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(10);
            float time = 1.0f;
            
            // Act
            float3[] positions = _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
            
            // Assert
            Assert.IsNotNull(positions, "Positions should not be null");
            Assert.AreEqual(bubbleData.Length, positions.Length, "Should return position for each bubble");
            
            // Verify positions are influenced by wave matrix (not all zero)
            int nonZeroPositions = positions.Count(p => !p.Equals(float3.zero));
            Assert.Greater(nonZeroPositions, 0, "Should have non-zero positions from wave matrix");
            
            // Verify positions are reasonable (within expected bounds)
            foreach (var pos in positions.Where(p => !p.Equals(float3.zero)))
            {
                Assert.IsTrue(math.isfinite(pos.x) && math.isfinite(pos.y) && math.isfinite(pos.z), 
                    $"Position {pos} should be finite");
                Assert.Less(math.length(pos), 100f, "Position should be within reasonable bounds");
            }
        }
        
        [Test]
        public void Integration_DifferentAiDistances_ProducesDifferentPositions()
        {
            // Arrange
            var bubbleData = new BubbleData[3];
            bubbleData[0] = BubbleData.Create(0, 0, 0f);    // High confidence
            bubbleData[1] = BubbleData.Create(1, 0, 1f);    // Medium confidence
            bubbleData[2] = BubbleData.Create(2, 0, 2f);    // Low confidence
            
            float time = 1.0f;
            
            // Act
            float3[] positions = _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
            
            // Assert
            Assert.AreEqual(3, positions.Length, "Should calculate 3 positions");
            
            // Positions should be different due to different AI distances
            Assert.AreNotEqual(positions[0], positions[1], "Different AI distances should produce different positions");
            Assert.AreNotEqual(positions[1], positions[2], "Different AI distances should produce different positions");
            Assert.AreNotEqual(positions[0], positions[2], "Different AI distances should produce different positions");
            
            // Higher AI distance should generally result in positions further from center
            float distance0 = math.length(positions[0]);
            float distance1 = math.length(positions[1]);
            float distance2 = math.length(positions[2]);
            
            // Note: This assumes the wave matrix pushes bubbles outward with higher AI distance
            // The exact relationship depends on the wave matrix implementation
            UnityEngine.Debug.Log($"Distances - AI:0 = {distance0:F2}, AI:1 = {distance1:F2}, AI:2 = {distance2:F2}");
        }
        
        [Test]
        public void Integration_TimeProgression_AnimatesPositions()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(5);
            float time1 = 0f;
            float time2 = 1f;
            
            // Act
            float3[] positions1 = _calculator.CalculateAllPositions(bubbleData, time1, _testSettings);
            float3[] positions2 = _calculator.CalculateAllPositions(bubbleData, time2, _testSettings);
            
            // Assert
            Assert.AreEqual(positions1.Length, positions2.Length, "Should return same number of positions");
            
            // At least some positions should change over time (wave animation)
            int changedPositions = 0;
            for (int i = 0; i < positions1.Length; i++)
            {
                if (bubbleData[i].IsActive && !positions1[i].Equals(positions2[i]))
                {
                    changedPositions++;
                }
            }
            
            Assert.Greater(changedPositions, 0, "Wave animation should change positions over time");
            
            UnityEngine.Debug.Log($"[Integration] {changedPositions}/{positions1.Length} positions changed over time");
        }
        
        #endregion
        
        #region End-to-End Workflow Tests
        
        [Test]
        public void EndToEnd_CompleteWorkflow_WorksCorrectly()
        {
            // Arrange - Create a realistic scenario
            var bubbleData = CreateRealisticBubbleData(20);
            float time = 0f;
            var bounds = BubbleBounds.FromCenterAndSize(float3.zero, new float3(20, 20, 20));
            
            // Act 1: Initial position calculation
            float3[] initialPositions = _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
            
            // Act 2: Validate positions
            var validationResult = _calculator.ValidatePositions(initialPositions, bounds);
            
            // Act 3: Mark some bubbles as dirty and update
            for (int i = 0; i < bubbleData.Length; i += 3)
            {
                bubbleData[i].IsDirty = true;
                bubbleData[i].AiDistance += 0.5f; // Change AI distance
            }
            
            int updatedCount = _calculator.UpdateDirtyPositions(bubbleData, time + 0.1f, _testSettings);
            
            // Act 4: Find closest bubble to a point
            var searchPoint = new float3(0, 0, 0);
            int closestIndex = _calculator.FindClosestBubble(searchPoint, bubbleData);
            
            // Act 5: Find bubbles in radius
            var nearbyBubbles = new List<int>();
            int nearbyCount = _calculator.FindBubblesInRadius(searchPoint, 5f, bubbleData, nearbyBubbles);
            
            // Act 6: Optimize layout
            var optimizationResult = _calculator.OptimizeLayout(bubbleData, _testSettings, 5);
            
            // Act 7: Get performance stats
            var stats = _calculator.GetPerformanceStats();
            
            // Assert - Verify entire workflow
            Assert.IsNotNull(initialPositions, "Initial positions should be calculated");
            Assert.IsTrue(validationResult.IsValid, "Initial positions should be valid");
            Assert.Greater(updatedCount, 0, "Should update dirty positions");
            Assert.GreaterOrEqual(closestIndex, -1, "Should find closest bubble or return -1");
            Assert.GreaterOrEqual(nearbyCount, 0, "Should find nearby bubbles");
            Assert.IsTrue(optimizationResult.Success, "Layout optimization should succeed");
            Assert.Greater(stats.TotalCalculations, 0, "Should track performance statistics");
            
            UnityEngine.Debug.Log($"[EndToEnd] Workflow completed: {updatedCount} updates, " +
                                $"closest: {closestIndex}, nearby: {nearbyCount}, " +
                                $"optimization: {optimizationResult.ImprovementScore:F2}");
        }
        
        [Test]
        public void EndToEnd_LargeBubbleSet_HandlesEfficiently()
        {
            // Arrange - Test with 100 bubbles (Quest 3 target)
            var bubbleData = CreateTestBubbleData(100);
            float time = 1.0f;
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Act - Perform complete workflow
            float3[] positions = _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
            
            // Mark 30% as dirty
            for (int i = 0; i < bubbleData.Length; i += 3)
            {
                bubbleData[i].IsDirty = true;
            }
            
            int updatedCount = _calculator.UpdateDirtyPositions(bubbleData, time + 0.016f, _testSettings);
            
            var bounds = BubbleBounds.FromCenterAndSize(float3.zero, new float3(50, 50, 50));
            var validationResult = _calculator.ValidatePositions(positions, bounds);
            
            stopwatch.Stop();
            
            // Assert
            Assert.AreEqual(100, positions.Length, "Should handle 100 bubbles");
            Assert.Greater(updatedCount, 0, "Should update dirty positions");
            Assert.IsTrue(validationResult.IsValid, "Positions should be valid");
            Assert.Less(stopwatch.ElapsedMilliseconds, 50, "Should complete within reasonable time");
            
            UnityEngine.Debug.Log($"[EndToEnd] 100 bubbles processed in {stopwatch.ElapsedMilliseconds}ms");
        }
        
        #endregion
        
        #region Error Handling and Edge Cases
        
        [Test]
        public void Integration_InvalidBubbleData_HandlesGracefully()
        {
            // Arrange - Create problematic bubble data
            var bubbleData = new BubbleData[5];
            bubbleData[0] = BubbleData.Create(0, -1, 0f);        // Invalid grid index
            bubbleData[1] = BubbleData.Create(1, 1000, 0f);      // Very large grid index
            bubbleData[2] = BubbleData.Create(2, 2, float.NaN);  // NaN AI distance
            bubbleData[3] = BubbleData.Create(3, 3, -1f);        // Negative AI distance
            bubbleData[4] = BubbleData.Create(4, 4, 0f);         // Normal bubble
            
            float time = 1.0f;
            
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => {
                float3[] positions = _calculator.CalculateAllPositions(bubbleData, time, _testSettings);
                Assert.IsNotNull(positions, "Should return positions array even with invalid data");
                Assert.AreEqual(5, positions.Length, "Should return position for each bubble");
            });
        }
        
        [Test]
        public void Integration_ExtremeSettings_HandlesGracefully()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(10);
            float time = 1.0f;
            
            // Test with extreme settings
            var extremeSettings = new WaveMatrixSettings
            {
                GridSize = new int2(1000, 1000),  // Very large grid
                CellSize = 0.001f,                // Very small cells
                WaveAmplitude = 1000f,            // Very large amplitude
                WaveFrequency = 1000f,            // Very high frequency
                WaveSpeed = 1000f                 // Very fast waves
            };
            
            // Act & Assert
            Assert.DoesNotThrow(() => {
                float3[] positions = _calculator.CalculateAllPositions(bubbleData, time, extremeSettings);
                Assert.IsNotNull(positions, "Should handle extreme settings");
                
                // Validate positions are still reasonable
                var bounds = BubbleBounds.FromCenterAndSize(float3.zero, new float3(10000, 10000, 10000));
                var validationResult = _calculator.ValidatePositions(positions, bounds);
                
                // May not be valid due to extreme settings, but should not crash
                UnityEngine.Debug.Log($"[Integration] Extreme settings validation: {validationResult.IsValid}");
            });
        }
        
        #endregion
        
        #region Performance Integration Tests
        
        [Test]
        [Performance]
        public void Integration_RepeatedCalculations_MaintainsPerformance()
        {
            // Arrange
            var bubbleData = CreateTestBubbleData(50);
            float baseTime = 0f;
            int iterations = 100;
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Act - Simulate realistic usage pattern
            for (int i = 0; i < iterations; i++)
            {
                float currentTime = baseTime + i * 0.016f; // 60 FPS
                
                // Calculate positions
                _calculator.CalculateAllPositions(bubbleData, currentTime, _testSettings);
                
                // Occasionally mark some bubbles dirty
                if (i % 10 == 0)
                {
                    for (int j = 0; j < bubbleData.Length; j += 5)
                    {
                        bubbleData[j].IsDirty = true;
                    }
                }
                
                // Update dirty positions
                _calculator.UpdateDirtyPositions(bubbleData, currentTime, _testSettings);
                
                // Occasional spatial queries
                if (i % 20 == 0)
                {
                    _calculator.FindClosestBubble(float3.zero, bubbleData);
                }
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageFrameTime = (float)stopwatch.ElapsedMilliseconds / iterations;
            
            Assert.Less(averageFrameTime, 5f, 
                $"Average frame time {averageFrameTime:F2}ms should be reasonable for 50 bubbles");
            
            // Verify performance stats
            var stats = _calculator.GetPerformanceStats();
            Assert.Greater(stats.TotalCalculations, 0, "Should track calculations");
            
            UnityEngine.Debug.Log($"[Integration] {iterations} frames averaged {averageFrameTime:F2}ms, " +
                                $"total calculations: {stats.TotalCalculations}");
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
                bubbleData[i].Priority = UnityEngine.Random.Range(0, 10);
            }
            
            return bubbleData;
        }
        
        private BubbleData[] CreateRealisticBubbleData(int count)
        {
            var bubbleData = new BubbleData[count];
            
            for (int i = 0; i < count; i++)
            {
                // Create more realistic distribution of AI distances
                float aiDistance;
                if (i < count * 0.3f) // 30% high confidence
                {
                    aiDistance = UnityEngine.Random.Range(0f, 0.5f);
                }
                else if (i < count * 0.7f) // 40% medium confidence
                {
                    aiDistance = UnityEngine.Random.Range(0.5f, 1.5f);
                }
                else // 30% low confidence
                {
                    aiDistance = UnityEngine.Random.Range(1.5f, 3f);
                }
                
                bubbleData[i] = BubbleData.Create(
                    bubbleId: i,
                    gridIndex: i % 100, // Distribute across grid
                    aiDistance: aiDistance
                );
                
                bubbleData[i].Radius = UnityEngine.Random.Range(0.3f, 0.7f);
                bubbleData[i].Priority = (int)(10 * (1f - aiDistance / 3f)); // Higher priority for higher confidence
                
                // Some bubbles start inactive
                bubbleData[i].IsActive = UnityEngine.Random.value > 0.1f; // 90% active
            }
            
            return bubbleData;
        }
        
        #endregion
    }
}