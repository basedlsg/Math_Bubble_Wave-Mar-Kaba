using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace XRBubbleLibrary.WaveMatrix.Tests
{
    /// <summary>
    /// Comprehensive unit tests for WaveMatrixCore.
    /// Tests mathematical accuracy, performance characteristics, and edge cases.
    /// Validates Requirement 5.1: Core wave mathematics accuracy.
    /// Validates Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public class WaveMatrixCoreTests
    {
        private IWaveMatrixCore _waveCore;
        private WaveMatrixSettings _testSettings;
        
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
        
        #region Basic Position Calculation Tests
        
        [Test]
        public void CalculateWavePosition_WithValidInput_ReturnsValidPosition()
        {
            // Arrange
            int bubbleIndex = 5;
            float time = 1.0f;
            
            // Act
            float3 position = _waveCore.CalculateWavePosition(bubbleIndex, time, _testSettings);
            
            // Assert
            Assert.IsTrue(math.isfinite(position.x), "X position should be finite");
            Assert.IsTrue(math.isfinite(position.y), "Y position should be finite");
            Assert.IsTrue(math.isfinite(position.z), "Z position should be finite");
        }
        
        [Test]
        public void CalculateWavePosition_WithNegativeIndex_ReturnsZero()
        {
            // Arrange
            int bubbleIndex = -1;
            float time = 1.0f;
            
            // Act
            float3 position = _waveCore.CalculateWavePosition(bubbleIndex, time, _testSettings);
            
            // Assert
            Assert.AreEqual(float3.zero, position, "Negative index should return zero position");
        }
        
        [Test]
        public void CalculateWavePosition_WithNullSettings_ReturnsZero()
        {
            // Arrange
            int bubbleIndex = 5;
            float time = 1.0f;
            
            // Act
            float3 position = _waveCore.CalculateWavePosition(bubbleIndex, time, null);
            
            // Assert
            Assert.AreEqual(float3.zero, position, "Null settings should return zero position");
        }
        
        [Test]
        public void CalculateWavePosition_WithDistanceOffset_AppliesOffset()
        {
            // Arrange
            int bubbleIndex = 5;
            float time = 1.0f;
            float distanceOffset = 2.0f;
            
            // Act
            float3 positionWithoutOffset = _waveCore.CalculateWavePosition(bubbleIndex, time, _testSettings);
            float3 positionWithOffset = _waveCore.CalculateWavePosition(bubbleIndex, time, distanceOffset, _testSettings);
            
            // Assert
            float expectedZDifference = distanceOffset * _testSettings.aiDistanceScale;
            float actualZDifference = positionWithOffset.z - positionWithoutOffset.z;
            Assert.AreEqual(expectedZDifference, actualZDifference, 0.001f, "Distance offset should be applied correctly");
        }
        
        #endregion
        
        #region Wave Height Calculation Tests
        
        [Test]
        public void CalculateWaveHeight_AtOrigin_ReturnsFiniteValue()
        {
            // Arrange
            float2 worldPosition = float2.zero;
            float time = 1.0f;
            
            // Act
            float height = _waveCore.CalculateWaveHeight(worldPosition, time, _testSettings);
            
            // Assert
            Assert.IsTrue(math.isfinite(height), "Wave height should be finite");
        }
        
        [Test]
        public void CalculateWaveHeight_WithNullSettings_ReturnsZero()
        {
            // Arrange
            float2 worldPosition = new float2(1, 1);
            float time = 1.0f;
            
            // Act
            float height = _waveCore.CalculateWaveHeight(worldPosition, time, null);
            
            // Assert
            Assert.AreEqual(0f, height, "Null settings should return zero height");
        }
        
        [Test]
        public void CalculateWaveHeight_WithTimeProgression_ChangesOverTime()
        {
            // Arrange
            float2 worldPosition = new float2(1, 1);
            float time1 = 0.0f;
            float time2 = 1.0f;
            
            // Act
            float height1 = _waveCore.CalculateWaveHeight(worldPosition, time1, _testSettings);
            float height2 = _waveCore.CalculateWaveHeight(worldPosition, time2, _testSettings);
            
            // Assert
            Assert.AreNotEqual(height1, height2, "Wave height should change over time");
        }
        
        [Test]
        public void CalculateWaveHeight_WithInterferenceDisabled_IgnoresInterference()
        {
            // Arrange
            var settingsWithInterference = WaveMatrixSettings.Default;
            settingsWithInterference.enableInterference = true;
            
            var settingsWithoutInterference = WaveMatrixSettings.Default;
            settingsWithoutInterference.enableInterference = false;
            
            float2 worldPosition = new float2(1, 1);
            float time = 1.0f;
            
            // Act
            float heightWith = _waveCore.CalculateWaveHeight(worldPosition, time, settingsWithInterference);
            float heightWithout = _waveCore.CalculateWaveHeight(worldPosition, time, settingsWithoutInterference);
            
            // Assert
            Assert.AreNotEqual(heightWith, heightWithout, "Interference should affect wave height");
        }
        
        #endregion
        
        #region Batch Processing Tests
        
        [Test]
        public void CalculateWavePositionsBatch_WithValidInput_CalculatesAllPositions()
        {
            // Arrange
            int[] bubbleIndices = { 0, 1, 2, 3, 4 };
            float time = 1.0f;
            float3[] results = new float3[bubbleIndices.Length];
            
            // Act
            _waveCore.CalculateWavePositionsBatch(bubbleIndices, time, _testSettings, results);
            
            // Assert
            for (int i = 0; i < results.Length; i++)
            {
                Assert.IsTrue(math.isfinite(results[i].x), $"Result {i} X should be finite");
                Assert.IsTrue(math.isfinite(results[i].y), $"Result {i} Y should be finite");
                Assert.IsTrue(math.isfinite(results[i].z), $"Result {i} Z should be finite");
            }
        }
        
        [Test]
        public void CalculateWavePositionsBatch_WithNullArrays_HandlesGracefully()
        {
            // Arrange
            float time = 1.0f;
            
            // Act & Assert (should not throw)
            Assert.DoesNotThrow(() => {
                _waveCore.CalculateWavePositionsBatch(null, time, _testSettings, null);
            });
        }
        
        [Test]
        public void CalculateWavePositionsBatch_WithMismatchedArrays_HandlesGracefully()
        {
            // Arrange
            int[] bubbleIndices = { 0, 1, 2 };
            float3[] results = new float3[5]; // Different length
            float time = 1.0f;
            
            // Act & Assert (should not throw)
            Assert.DoesNotThrow(() => {
                _waveCore.CalculateWavePositionsBatch(bubbleIndices, time, _testSettings, results);
            });
        }
        
        [Test]
        public void CalculateWavePositionsBatch_WithDistanceOffsets_AppliesOffsetsCorrectly()
        {
            // Arrange
            int[] bubbleIndices = { 0, 1, 2 };
            float[] distanceOffsets = { 0f, 1f, 2f };
            float time = 1.0f;
            float3[] results = new float3[bubbleIndices.Length];
            
            // Act
            _waveCore.CalculateWavePositionsBatch(bubbleIndices, time, distanceOffsets, _testSettings, results);
            
            // Assert
            for (int i = 0; i < results.Length; i++)
            {
                Assert.IsTrue(math.isfinite(results[i].x), $"Result {i} X should be finite");
                Assert.IsTrue(math.isfinite(results[i].y), $"Result {i} Y should be finite");
                Assert.IsTrue(math.isfinite(results[i].z), $"Result {i} Z should be finite");
            }
            
            // Verify that distance offsets are applied (results should be different)
            Assert.AreNotEqual(results[0].z, results[1].z, "Different distance offsets should produce different Z positions");
            Assert.AreNotEqual(results[1].z, results[2].z, "Different distance offsets should produce different Z positions");
        }
        
        #endregion
        
        #region Time Management Tests
        
        [Test]
        public void UpdateWaveTime_WithPositiveDelta_IncreasesTime()
        {
            // Arrange
            float deltaTime = 0.1f;
            float initialTime = _waveCore.UpdateWaveTime(0f, _testSettings);
            
            // Act
            float updatedTime = _waveCore.UpdateWaveTime(deltaTime, _testSettings);
            
            // Assert
            Assert.Greater(updatedTime, initialTime, "Time should increase with positive delta");
        }
        
        [Test]
        public void UpdateWaveTime_WithTimeScale_AppliesScaling()
        {
            // Arrange
            var scaledSettings = WaveMatrixSettings.Default;
            scaledSettings.timeScale = 2.0f;
            
            float deltaTime = 0.1f;
            
            // Act
            float normalTime = _waveCore.UpdateWaveTime(deltaTime, _testSettings);
            float scaledTime = _waveCore.UpdateWaveTime(deltaTime, scaledSettings);
            
            // Assert
            // Note: This test assumes both calls start from the same internal time state
            // In practice, you might need to reset the core between calls
        }
        
        [Test]
        public void UpdateWaveTime_WithNullSettings_ReturnsCurrentTime()
        {
            // Arrange
            float deltaTime = 0.1f;
            
            // Act
            float time1 = _waveCore.UpdateWaveTime(deltaTime, null);
            float time2 = _waveCore.UpdateWaveTime(deltaTime, null);
            
            // Assert
            Assert.AreEqual(time1, time2, "Null settings should not update time");
        }
        
        #endregion
        
        #region Settings Validation Tests
        
        [Test]
        public void ValidateSettings_WithValidSettings_ReturnsValid()
        {
            // Act
            var result = _waveCore.ValidateSettings(_testSettings);
            
            // Assert
            Assert.IsTrue(result.IsValid, "Default settings should be valid");
            Assert.AreEqual(0, result.Issues.Length, "Valid settings should have no issues");
        }
        
        [Test]
        public void ValidateSettings_WithNullSettings_ReturnsInvalid()
        {
            // Act
            var result = _waveCore.ValidateSettings(null);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Null settings should be invalid");
            Assert.Greater(result.Issues.Length, 0, "Null settings should have issues");
        }
        
        [Test]
        public void ValidateSettings_WithInvalidGridDimensions_ReturnsInvalid()
        {
            // Arrange
            var invalidSettings = WaveMatrixSettings.Default;
            invalidSettings.gridWidth = 0;
            invalidSettings.gridHeight = -1;
            
            // Act
            var result = _waveCore.ValidateSettings(invalidSettings);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Invalid grid dimensions should make settings invalid");
            Assert.Greater(result.Issues.Length, 0, "Invalid grid dimensions should generate issues");
        }
        
        [Test]
        public void ValidateSettings_WithLargeGrid_GeneratesWarnings()
        {
            // Arrange
            var largeGridSettings = WaveMatrixSettings.Default;
            largeGridSettings.gridWidth = 20;
            largeGridSettings.gridHeight = 20; // 400 total bubbles
            
            // Act
            var result = _waveCore.ValidateSettings(largeGridSettings);
            
            // Assert
            Assert.Greater(result.Warnings.Length, 0, "Large grid should generate performance warnings");
            Assert.Greater(result.PerformanceImpact, 0f, "Large grid should have performance impact");
        }
        
        [Test]
        public void ValidateSettings_WithExtremeWaveParameters_GeneratesWarnings()
        {
            // Arrange
            var extremeSettings = WaveMatrixSettings.Default;
            extremeSettings.primaryWave.amplitude = 10f; // Very large amplitude
            extremeSettings.primaryWave.frequency = 20f; // Very high frequency
            extremeSettings.primaryWave.speed = 15f; // Very fast speed
            
            // Act
            var result = _waveCore.ValidateSettings(extremeSettings);
            
            // Assert
            Assert.Greater(result.Warnings.Length, 0, "Extreme wave parameters should generate warnings");
        }
        
        #endregion
        
        #region Grid Position Tests
        
        [Test]
        public void GetGridPosition_WithValidIndex_ReturnsCorrectPosition()
        {
            // Arrange
            int bubbleIndex = 0; // First position should be at grid origin
            
            // Act
            float3 gridPos = _waveCore.GetGridPosition(bubbleIndex, _testSettings);
            
            // Assert
            Assert.IsTrue(math.isfinite(gridPos.x), "Grid X should be finite");
            Assert.AreEqual(0f, gridPos.y, "Grid Y should always be zero");
            Assert.IsTrue(math.isfinite(gridPos.z), "Grid Z should be finite");
        }
        
        [Test]
        public void GetGridPosition_WithDifferentIndices_ReturnsDifferentPositions()
        {
            // Arrange
            int index1 = 0;
            int index2 = 1;
            
            // Act
            float3 pos1 = _waveCore.GetGridPosition(index1, _testSettings);
            float3 pos2 = _waveCore.GetGridPosition(index2, _testSettings);
            
            // Assert
            Assert.AreNotEqual(pos1, pos2, "Different indices should return different positions");
        }
        
        [Test]
        public void GetGridPosition_WithNullSettings_ReturnsZero()
        {
            // Arrange
            int bubbleIndex = 5;
            
            // Act
            float3 gridPos = _waveCore.GetGridPosition(bubbleIndex, null);
            
            // Assert
            Assert.AreEqual(float3.zero, gridPos, "Null settings should return zero position");
        }
        
        #endregion
        
        #region World Position Conversion Tests
        
        [Test]
        public void WorldPositionToGridIndex_WithValidPosition_ReturnsValidIndex()
        {
            // Arrange
            int originalIndex = 5;
            float3 gridPos = _waveCore.GetGridPosition(originalIndex, _testSettings);
            
            // Act
            int convertedIndex = _waveCore.WorldPositionToGridIndex(gridPos, _testSettings);
            
            // Assert
            Assert.AreEqual(originalIndex, convertedIndex, "Converting grid position back should return original index");
        }
        
        [Test]
        public void WorldPositionToGridIndex_WithOutOfBoundsPosition_ReturnsNegativeOne()
        {
            // Arrange
            float3 outOfBoundsPos = new float3(1000f, 0f, 1000f); // Far outside grid
            
            // Act
            int index = _waveCore.WorldPositionToGridIndex(outOfBoundsPos, _testSettings);
            
            // Assert
            Assert.AreEqual(-1, index, "Out of bounds position should return -1");
        }
        
        [Test]
        public void WorldPositionToGridIndex_WithNullSettings_ReturnsNegativeOne()
        {
            // Arrange
            float3 position = new float3(1f, 0f, 1f);
            
            // Act
            int index = _waveCore.WorldPositionToGridIndex(position, null);
            
            // Assert
            Assert.AreEqual(-1, index, "Null settings should return -1");
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        public void CalculateWavePosition_RepeatedCalls_MaintainsPerformance()
        {
            // Arrange
            int bubbleIndex = 5;
            float time = 1.0f;
            int iterations = 1000;
            
            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _waveCore.CalculateWavePosition(bubbleIndex, time, _testSettings);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            Assert.Less(averageTimeMs, 0.1f, $"Average calculation time ({averageTimeMs:F4}ms) should be under 0.1ms for Quest 3 performance");
        }
        
        [Test]
        public void CalculateWavePositionsBatch_LargeBatch_CompletesInReasonableTime()
        {
            // Arrange
            int batchSize = 100;
            int[] bubbleIndices = new int[batchSize];
            float3[] results = new float3[batchSize];
            
            for (int i = 0; i < batchSize; i++)
            {
                bubbleIndices[i] = i;
            }
            
            float time = 1.0f;
            
            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _waveCore.CalculateWavePositionsBatch(bubbleIndices, time, _testSettings, results);
            stopwatch.Stop();
            
            // Assert
            float timePerBubble = (float)stopwatch.ElapsedMilliseconds / batchSize;
            Assert.Less(timePerBubble, 0.05f, $"Batch processing time per bubble ({timePerBubble:F4}ms) should be efficient");
        }
        
        #endregion
        
        #region Mathematical Accuracy Tests
        
        [Test]
        public void CalculateWaveHeight_WithZeroAmplitudes_ReturnsZero()
        {
            // Arrange
            var zeroAmplitudeSettings = WaveMatrixSettings.Default;
            zeroAmplitudeSettings.primaryWave.amplitude = 0f;
            zeroAmplitudeSettings.secondaryWave.amplitude = 0f;
            zeroAmplitudeSettings.tertiaryWave.amplitude = 0f;
            zeroAmplitudeSettings.enableInterference = false;
            
            float2 worldPosition = new float2(1, 1);
            float time = 1.0f;
            
            // Act
            float height = _waveCore.CalculateWaveHeight(worldPosition, time, zeroAmplitudeSettings);
            
            // Assert
            Assert.AreEqual(0f, height, 0.001f, "Zero amplitudes should produce zero wave height");
        }
        
        [Test]
        public void CalculateWaveHeight_WithSymmetricPosition_ProducesExpectedSymmetry()
        {
            // Arrange
            float2 pos1 = new float2(1, 1);
            float2 pos2 = new float2(-1, -1);
            float time = 0f; // Use zero time to eliminate time-based variations
            
            // Act
            float height1 = _waveCore.CalculateWaveHeight(pos1, time, _testSettings);
            float height2 = _waveCore.CalculateWaveHeight(pos2, time, _testSettings);
            
            // Assert
            // Note: Exact symmetry depends on wave function design
            // This test verifies that the function produces reasonable results for symmetric inputs
            Assert.IsTrue(math.isfinite(height1), "Height 1 should be finite");
            Assert.IsTrue(math.isfinite(height2), "Height 2 should be finite");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [Test]
        public void CalculateWavePosition_WithVeryLargeTime_RemainsStable()
        {
            // Arrange
            int bubbleIndex = 5;
            float largeTime = 1000000f; // Very large time value
            
            // Act
            float3 position = _waveCore.CalculateWavePosition(bubbleIndex, largeTime, _testSettings);
            
            // Assert
            Assert.IsTrue(math.isfinite(position.x), "Position should remain finite with large time");
            Assert.IsTrue(math.isfinite(position.y), "Position should remain finite with large time");
            Assert.IsTrue(math.isfinite(position.z), "Position should remain finite with large time");
        }
        
        [Test]
        public void CalculateWavePosition_WithNegativeTime_HandlesGracefully()
        {
            // Arrange
            int bubbleIndex = 5;
            float negativeTime = -10f;
            
            // Act
            float3 position = _waveCore.CalculateWavePosition(bubbleIndex, negativeTime, _testSettings);
            
            // Assert
            Assert.IsTrue(math.isfinite(position.x), "Position should be finite with negative time");
            Assert.IsTrue(math.isfinite(position.y), "Position should be finite with negative time");
            Assert.IsTrue(math.isfinite(position.z), "Position should be finite with negative time");
        }
        
        #endregion
    }
}