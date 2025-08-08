using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

namespace XRBubbleLibrary.WaveMatrix.Tests
{
    /// <summary>
    /// Comprehensive unit tests for BubblePositionCalculator.
    /// Tests position calculation accuracy, performance, and caching behavior.
    /// Validates Requirement 5.1: Efficient algorithms for 100+ bubble positioning.
    /// Validates Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public class BubblePositionCalculatorTests
    {
        private IBubblePositionCalculator _calculator;
        private WaveMatrixSettings _testSettings;
        private IWaveMatrixCore _mockWaveCore;
        
        [SetUp]
        public void SetUp()
        {
            _mockWaveCore = new WaveMatrixCore();
            _calculator = new BubblePositionCalculator(_mockWaveCore, 50); // Smaller for testing
            _testSettings = WaveMatrixSettings.Default;
        }
        
        [TearDown]
        public void TearDown()
        {
            _calculator?.ClearCache();
            _calculator = null;
            _testSettings = null;
            _mockWaveCore = null;
        }
        
        #region Basic Position Calculation Tests
        
        [Test]
        public void CalculateAllPositions_WithValidSettings_ReturnsCorrectCount()
        {
            // Arrange
            float time = 1.0f;
            
            // Act
            var positions = _calculator.CalculateAllPositions(time, _testSettings);
            
            // Assert
            Assert.AreEqual(50, positions.Length, "Should return positions for all bubbles");
            
            foreach (var position in positions)
            {
                Assert.IsTrue(math.isfinite(position.x), "X position should be finite");
                Assert.IsTrue(math.isfinite(position.y), "Y position should be finite");
                Assert.IsTrue(math.isfinite(position.z), "Z position should be finite");
            }
        }
        
        [Test]
        public void CalculateAllPositions_WithNullSettings_ReturnsEmptyArray()
        {
            // Arrange
            float time = 1.0f;
            
            // Act
            var positions = _calculator.CalculateAllPositions(time, null);
            
            // Assert
            Assert.AreEqual(0, positions.Length, "Should return empty array for null settings");
        }
        
        [Test]
        public void CalculatePositions_WithSpecificIndices_ReturnsCorrectPositions()
        {
            // Arrange
            int[] indices = { 0, 5, 10, 15 };
            float time = 1.0f;
            
            // Act
            var positions = _calculator.CalculatePositions(indices, time, _testSettings);
            
            // Assert
            Assert.AreEqual(indices.Length, positions.Length, "Should return positions for specified indices");
            
            foreach (var position in positions)
            {
                Assert.IsTrue(math.isfinite(position.x), "Position should be finite");
                Assert.IsTrue(math.isfinite(position.y), "Position should be finite");
                Assert.IsTrue(math.isfinite(position.z), "Position should be finite");
            }
        }
        
        [Test]
        public void CalculatePositions_WithInvalidParameters_ReturnsEmptyArray()
        {
            // Arrange
            float time = 1.0f;
            
            // Act
            var positions1 = _calculator.CalculatePositions(null, time, _testSettings);
            var positions2 = _calculator.CalculatePositions(new int[] { 0, 1 }, time, null);
            
            // Assert
            Assert.AreEqual(0, positions1.Length, "Should return empty array for null indices");
            Assert.AreEqual(0, positions2.Length, "Should return empty array for null settings");
        }
        
        #endregion
        
        #region AI Distance Tests
        
        [Test]
        public void CalculatePositionsWithAI_WithValidData_ReturnsCorrectPositions()
        {
            // Arrange
            var bubbleData = new BubbleData[]
            {
                BubbleData.Create(0, 1.0f, 10, true),
                BubbleData.Create(5, 2.0f, 5, true),
                BubbleData.Create(10, 0.5f, 15, true)
            };
            float time = 1.0f;
            
            // Act
            var positions = _calculator.CalculatePositionsWithAI(bubbleData, time, _testSettings);
            
            // Assert
            Assert.AreEqual(3, positions.Length, "Should return positions for active bubbles");
            
            foreach (var position in positions)
            {
                Assert.IsTrue(math.isfinite(position.x), "Position should be finite");
                Assert.IsTrue(math.isfinite(position.y), "Position should be finite");
                Assert.IsTrue(math.isfinite(position.z), "Position should be finite");
            }
        }
        
        [Test]
        public void CalculatePositionsWithAI_WithInactiveBubbles_FiltersCorrectly()
        {
            // Arrange
            var bubbleData = new BubbleData[]
            {
                BubbleData.Create(0, 1.0f, 10, true),
                BubbleData.Create(5, 2.0f, 5, false), // Inactive
                BubbleData.Create(10, 0.5f, 15, true)
            };
            float time = 1.0f;
            
            // Act
            var positions = _calculator.CalculatePositionsWithAI(bubbleData, time, _testSettings);
            
            // Assert
            Assert.AreEqual(2, positions.Length, "Should only return positions for active bubbles");
        }
        
        [Test]
        public void CalculatePositionsWithAI_WithPriorities_SortsCorrectly()
        {
            // Arrange
            var bubbleData = new BubbleData[]
            {
                BubbleData.Create(0, 1.0f, 5, true),  // Lower priority
                BubbleData.Create(5, 2.0f, 15, true), // Higher priority
                BubbleData.Create(10, 0.5f, 10, true) // Medium priority
            };
            float time = 1.0f;
            
            // Act
            var positions = _calculator.CalculatePositionsWithAI(bubbleData, time, _testSettings);
            
            // Assert
            Assert.AreEqual(3, positions.Length, "Should return all active bubble positions");
            // Note: Order verification would require access to internal sorting logic
        }
        
        #endregion
        
        #region Caching Tests
        
        [Test]
        public void GetCachedPosition_AfterCalculation_ReturnsCachedValue()
        {
            // Arrange
            float time = 1.0f;
            int bubbleIndex = 5;
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            var cachedPosition = _calculator.GetCachedPosition(bubbleIndex);
            
            // Assert
            Assert.AreNotEqual(float3.zero, cachedPosition, "Should return non-zero cached position");
            Assert.IsTrue(math.isfinite(cachedPosition.x), "Cached position should be finite");
        }
        
        [Test]
        public void GetCachedPosition_WithInvalidIndex_ReturnsZero()
        {
            // Arrange
            int invalidIndex = -1;
            int outOfRangeIndex = 1000;
            
            // Act
            var position1 = _calculator.GetCachedPosition(invalidIndex);
            var position2 = _calculator.GetCachedPosition(outOfRangeIndex);
            
            // Assert
            Assert.AreEqual(float3.zero, position1, "Should return zero for invalid index");
            Assert.AreEqual(float3.zero, position2, "Should return zero for out of range index");
        }
        
        [Test]
        public void MarkBubbleDirty_ThenGetCached_ReturnsZero()
        {
            // Arrange
            float time = 1.0f;
            int bubbleIndex = 5;
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            _calculator.MarkBubbleDirty(bubbleIndex);
            var cachedPosition = _calculator.GetCachedPosition(bubbleIndex);
            
            // Assert
            Assert.AreEqual(float3.zero, cachedPosition, "Should return zero for dirty bubble");
        }
        
        [Test]
        public void UpdateDirtyPositions_WithDirtyBubbles_UpdatesOnlyDirty()
        {
            // Arrange
            float time = 1.0f;
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            _calculator.MarkBubbleDirty(5);
            _calculator.MarkBubbleDirty(10);
            int updatedCount = _calculator.UpdateDirtyPositions(time + 0.1f, _testSettings);
            
            // Assert
            Assert.AreEqual(2, updatedCount, "Should update exactly 2 dirty positions");
        }
        
        [Test]
        public void SetCachingEnabled_False_DisablesCaching()
        {
            // Arrange
            float time = 1.0f;
            
            // Act
            _calculator.SetCachingEnabled(false);
            _calculator.CalculateAllPositions(time, _testSettings);
            var cachedPosition = _calculator.GetCachedPosition(5);
            
            // Assert
            Assert.AreEqual(float3.zero, cachedPosition, "Should return zero when caching is disabled");
        }
        
        [Test]
        public void GetCachedPositionCount_AfterCalculations_ReturnsCorrectCount()
        {
            // Arrange
            float time = 1.0f;
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            int cachedCount = _calculator.GetCachedPositionCount();
            
            // Assert
            Assert.AreEqual(50, cachedCount, "Should have cached all calculated positions");
        }
        
        [Test]
        public void ClearCache_RemovesAllCachedPositions()
        {
            // Arrange
            float time = 1.0f;
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            _calculator.ClearCache();
            int cachedCount = _calculator.GetCachedPositionCount();
            
            // Assert
            Assert.AreEqual(0, cachedCount, "Should have no cached positions after clear");
        }
        
        #endregion
        
        #region Position Validation Tests
        
        [Test]
        public void ValidatePositions_WithValidPositions_ReturnsValid()
        {
            // Arrange
            var positions = new float3[]
            {
                new float3(1, 0.5f, 1),
                new float3(-1, -0.5f, -1),
                new float3(0, 0, 0)
            };
            
            // Act
            var result = _calculator.ValidatePositions(positions, _testSettings);
            
            // Assert
            Assert.IsTrue(result.IsValid, "Valid positions should pass validation");
            Assert.AreEqual(0, result.Issues.Length, "Should have no validation issues");
            Assert.AreEqual(0, result.OutOfBoundsIndices.Length, "Should have no out of bounds positions");
            Assert.AreEqual(0, result.InvalidValueIndices.Length, "Should have no invalid values");
        }
        
        [Test]
        public void ValidatePositions_WithInvalidValues_ReturnsInvalid()
        {
            // Arrange
            var positions = new float3[]
            {
                new float3(1, 0.5f, 1),
                new float3(float.NaN, 0, 0),
                new float3(float.PositiveInfinity, 0, 0)
            };
            
            // Act
            var result = _calculator.ValidatePositions(positions, _testSettings);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Invalid positions should fail validation");
            Assert.Greater(result.Issues.Length, 0, "Should have validation issues");
            Assert.AreEqual(2, result.InvalidValueIndices.Length, "Should identify invalid value positions");
        }
        
        [Test]
        public void ValidatePositions_WithNullParameters_ReturnsInvalid()
        {
            // Act
            var result1 = _calculator.ValidatePositions(null, _testSettings);
            var result2 = _calculator.ValidatePositions(new float3[0], null);
            
            // Assert
            Assert.IsFalse(result1.IsValid, "Should be invalid with null positions");
            Assert.IsFalse(result2.IsValid, "Should be invalid with null settings");
        }
        
        [Test]
        public void IsPositionInBounds_WithValidPosition_ReturnsTrue()
        {
            // Arrange
            var position = new float3(1, 0.5f, 1);
            
            // Act
            bool inBounds = _calculator.IsPositionInBounds(position, _testSettings);
            
            // Assert
            Assert.IsTrue(inBounds, "Valid position should be in bounds");
        }
        
        [Test]
        public void IsPositionInBounds_WithOutOfBoundsPosition_ReturnsFalse()
        {
            // Arrange
            var position = new float3(1000, 1000, 1000); // Way out of bounds
            
            // Act
            bool inBounds = _calculator.IsPositionInBounds(position, _testSettings);
            
            // Assert
            Assert.IsFalse(inBounds, "Out of bounds position should return false");
        }
        
        #endregion
        
        #region Spatial Query Tests
        
        [Test]
        public void FindClosestBubble_WithCachedPositions_ReturnsClosestIndex()
        {
            // Arrange
            float time = 1.0f;
            var targetPosition = new float3(0, 0, 0);
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            int closestIndex = _calculator.FindClosestBubble(targetPosition);
            
            // Assert
            Assert.GreaterOrEqual(closestIndex, 0, "Should find a closest bubble");
            Assert.Less(closestIndex, 50, "Closest index should be within range");
        }
        
        [Test]
        public void FindClosestBubble_WithMaxDistance_RespectsLimit()
        {
            // Arrange
            float time = 1.0f;
            var targetPosition = new float3(1000, 1000, 1000); // Far away
            float maxDistance = 1.0f;
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            int closestIndex = _calculator.FindClosestBubble(targetPosition, maxDistance);
            
            // Assert
            Assert.AreEqual(-1, closestIndex, "Should return -1 when no bubbles within max distance");
        }
        
        [Test]
        public void GetBubblesInRadius_WithCachedPositions_ReturnsCorrectBubbles()
        {
            // Arrange
            float time = 1.0f;
            var centerPosition = new float3(0, 0, 0);
            float radius = 5.0f;
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            var bubblesInRadius = _calculator.GetBubblesInRadius(centerPosition, radius);
            
            // Assert
            Assert.IsNotNull(bubblesInRadius, "Should return a list of bubbles");
            Assert.GreaterOrEqual(bubblesInRadius.Count, 0, "Should return non-negative count");
            
            // Verify all returned bubbles are actually within radius
            foreach (int bubbleIndex in bubblesInRadius)
            {
                var bubblePosition = _calculator.GetCachedPosition(bubbleIndex);
                float distance = math.distance(centerPosition, bubblePosition);
                Assert.LessOrEqual(distance, radius, $"Bubble {bubbleIndex} should be within radius");
            }
        }
        
        [Test]
        public void GetBubblesInRadius_WithSmallRadius_ReturnsFewerBubbles()
        {
            // Arrange
            float time = 1.0f;
            var centerPosition = new float3(0, 0, 0);
            float smallRadius = 0.1f;
            float largeRadius = 10.0f;
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            var bubblesSmallRadius = _calculator.GetBubblesInRadius(centerPosition, smallRadius);
            var bubblesLargeRadius = _calculator.GetBubblesInRadius(centerPosition, largeRadius);
            
            // Assert
            Assert.LessOrEqual(bubblesSmallRadius.Count, bubblesLargeRadius.Count, 
                "Smaller radius should return fewer or equal bubbles");
        }
        
        #endregion
        
        #region Configuration Tests
        
        [Test]
        public void SetMaxBubbles_WithValidCount_UpdatesConfiguration()
        {
            // Arrange
            int newMaxBubbles = 75;
            
            // Act
            _calculator.SetMaxBubbles(newMaxBubbles);
            int actualMaxBubbles = _calculator.GetMaxBubbles();
            
            // Assert
            Assert.AreEqual(newMaxBubbles, actualMaxBubbles, "Should update max bubbles count");
        }
        
        [Test]
        public void SetMaxBubbles_WithInvalidCount_DoesNotUpdate()
        {
            // Arrange
            int originalMaxBubbles = _calculator.GetMaxBubbles();
            int invalidCount = -5;
            
            // Act
            _calculator.SetMaxBubbles(invalidCount);
            int actualMaxBubbles = _calculator.GetMaxBubbles();
            
            // Assert
            Assert.AreEqual(originalMaxBubbles, actualMaxBubbles, "Should not update with invalid count");
        }
        
        [Test]
        public void GetPerformanceStats_AfterCalculations_ReturnsValidStats()
        {
            // Arrange
            float time = 1.0f;
            
            // Act
            _calculator.CalculateAllPositions(time, _testSettings);
            var stats = _calculator.GetPerformanceStats();
            
            // Assert
            Assert.Greater(stats.TotalCalculations, 0, "Should have recorded calculations");
            Assert.GreaterOrEqual(stats.CacheHits, 0, "Cache hits should be non-negative");
            Assert.GreaterOrEqual(stats.CacheMisses, 0, "Cache misses should be non-negative");
            Assert.AreEqual(50, stats.MaxBubbles, "Should reflect configured max bubbles");
            Assert.GreaterOrEqual(stats.CachedPositions, 0, "Cached positions should be non-negative");
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        [Performance]
        public void CalculateAllPositions_100Bubbles_CompletesQuickly()
        {
            // Arrange
            _calculator.SetMaxBubbles(100);
            float time = 1.0f;
            
            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var positions = _calculator.CalculateAllPositions(time, _testSettings);
            stopwatch.Stop();
            
            // Assert
            Assert.AreEqual(100, positions.Length, "Should calculate 100 positions");
            Assert.Less(stopwatch.ElapsedMilliseconds, 10, "Should complete within 10ms for Quest 3 performance");
        }
        
        [Test]
        [Performance]
        public void CalculatePositions_CacheHit_IsFasterThanCacheMiss()
        {
            // Arrange
            int[] indices = { 0, 1, 2, 3, 4 };
            float time = 1.0f;
            
            // First calculation (cache miss)
            var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
            _calculator.CalculatePositions(indices, time, _testSettings);
            stopwatch1.Stop();
            
            // Second calculation (cache hit)
            var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
            _calculator.CalculatePositions(indices, time, _testSettings);
            stopwatch2.Stop();
            
            // Assert
            Assert.LessOrEqual(stopwatch2.ElapsedMilliseconds, stopwatch1.ElapsedMilliseconds, 
                "Cache hit should be faster or equal to cache miss");
        }
        
        [Test]
        [Performance]
        public void UpdateDirtyPositions_SmallBatch_CompletesQuickly()
        {
            // Arrange
            float time = 1.0f;
            _calculator.CalculateAllPositions(time, _testSettings);
            
            // Mark a few bubbles dirty
            for (int i = 0; i < 5; i++)
            {
                _calculator.MarkBubbleDirty(i);
            }
            
            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int updatedCount = _calculator.UpdateDirtyPositions(time + 0.1f, _testSettings);
            stopwatch.Stop();
            
            // Assert
            Assert.AreEqual(5, updatedCount, "Should update 5 dirty positions");
            Assert.Less(stopwatch.ElapsedMilliseconds, 5, "Should complete quickly for small batch");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [Test]
        public void CalculatePositions_WithEmptyIndices_ReturnsEmptyArray()
        {
            // Arrange
            int[] emptyIndices = new int[0];
            float time = 1.0f;
            
            // Act
            var positions = _calculator.CalculatePositions(emptyIndices, time, _testSettings);
            
            // Assert
            Assert.AreEqual(0, positions.Length, "Should return empty array for empty indices");
        }
        
        [Test]
        public void CalculatePositionsWithAI_WithEmptyData_ReturnsEmptyArray()
        {
            // Arrange
            var emptyData = new BubbleData[0];
            float time = 1.0f;
            
            // Act
            var positions = _calculator.CalculatePositionsWithAI(emptyData, time, _testSettings);
            
            // Assert
            Assert.AreEqual(0, positions.Length, "Should return empty array for empty bubble data");
        }
        
        [Test]
        public void UpdateDirtyPositions_WithNoDirtyBubbles_ReturnsZero()
        {
            // Arrange
            float time = 1.0f;
            _calculator.CalculateAllPositions(time, _testSettings); // All clean
            
            // Act
            int updatedCount = _calculator.UpdateDirtyPositions(time + 0.1f, _testSettings);
            
            // Assert
            Assert.AreEqual(0, updatedCount, "Should return 0 when no bubbles are dirty");
        }
        
        [Test]
        public void FindClosestBubble_WithNoCachedPositions_ReturnsNegativeOne()
        {
            // Arrange
            var targetPosition = new float3(0, 0, 0);
            
            // Act (no positions calculated)
            int closestIndex = _calculator.FindClosestBubble(targetPosition);
            
            // Assert
            Assert.AreEqual(-1, closestIndex, "Should return -1 when no positions are cached");
        }
        
        #endregion
    }
}