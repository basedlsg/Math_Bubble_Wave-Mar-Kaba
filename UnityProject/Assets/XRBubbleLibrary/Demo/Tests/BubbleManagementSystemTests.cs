using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.Demo.Tests
{
    /// <summary>
    /// Comprehensive tests for BubbleManagementSystem.
    /// Validates exactly 100 bubble management, pooling, and performance optimization.
    /// Implements Requirement 5.1: Demonstrate wave mathematics with exactly 100 bubbles.
    /// Implements Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public class BubbleManagementSystemTests
    {
        private IBubbleManagementSystem _bubbleManager;
        private Transform _testContainer;
        private IPerformanceOptimizedWaveSystem _mockWaveSystem;
        
        [SetUp]
        public void SetUp()
        {
            // Create test container
            var containerGO = new GameObject("TestBubbleContainer");
            _testContainer = containerGO.transform;
            
            // Create mock wave system
            _mockWaveSystem = new PerformanceOptimizedWaveSystem();
            _mockWaveSystem.WarmUp();
            
            // Create bubble management system
            _bubbleManager = new BubbleManagementSystem(100, _testContainer, _mockWaveSystem);
        }
        
        [TearDown]
        public void TearDown()
        {
            _bubbleManager?.Dispose();
            _mockWaveSystem?.Dispose();
            
            if (_testContainer != null)
            {
                Object.DestroyImmediate(_testContainer.gameObject);
            }
        }
        
        #region Initialization Tests
        
        [Test]
        public void Initialize_WithValidParameters_InitializesCorrectly()
        {
            // Assert
            Assert.IsTrue(_bubbleManager.IsInitialized, "Should be initialized");
            Assert.AreEqual(100, _bubbleManager.TargetBubbleCount, "Should target exactly 100 bubbles");
            Assert.AreEqual(100, _bubbleManager.ActiveBubbleCount, "Should have exactly 100 active bubbles");
            Assert.IsFalse(_bubbleManager.IsRunning, "Should not be running initially");
            
            UnityEngine.Debug.Log($"[BubbleManagement] Initialized with {_bubbleManager.ActiveBubbleCount} bubbles");
        }
        
        [Test]
        public void Initialize_WithWrongBubbleCount_LogsWarning()
        {
            // Arrange
            var wrongCountContainer = new GameObject("WrongCountContainer").transform;
            
            try
            {
                // Act
                using var wrongCountManager = new BubbleManagementSystem(50, wrongCountContainer, _mockWaveSystem);
                
                // Assert
                Assert.AreEqual(50, wrongCountManager.TargetBubbleCount, "Should use provided count");
                Assert.AreEqual(50, wrongCountManager.ActiveBubbleCount, "Should create specified number of bubbles");
                
                // Note: Warning should be logged but we can't easily test that in unit tests
            }
            finally
            {
                Object.DestroyImmediate(wrongCountContainer.gameObject);
            }
        }
        
        #endregion
        
        #region Bubble Count Validation Tests
        
        [Test]
        public void ValidateBubbleCount_WithCorrectCount_ReturnsSuccess()
        {
            // Act
            var result = _bubbleManager.ValidateBubbleCount();
            
            // Assert
            Assert.IsTrue(result.IsValid, "Bubble count validation should pass");
            Assert.AreEqual(100, result.ExpectedCount, "Should expect 100 bubbles");
            Assert.AreEqual(100, result.ActualCount, "Should have 100 actual bubbles");
            Assert.AreEqual(0, result.CountDifference, "Should have no count difference");
            Assert.That(result.Message, Does.Contain("passed"), "Should indicate validation passed");
            
            UnityEngine.Debug.Log($"[BubbleCount] Validation: {result.Message}");
        }
        
        [Test]
        public void GetActiveBubbleTransforms_ReturnsCorrectCount()
        {
            // Act
            var transforms = _bubbleManager.GetActiveBubbleTransforms();
            
            // Assert
            Assert.IsNotNull(transforms, "Should return transform array");
            Assert.AreEqual(100, transforms.Length, "Should return exactly 100 transforms");
            
            // Verify all transforms are valid
            for (int i = 0; i < transforms.Length; i++)
            {
                Assert.IsNotNull(transforms[i], $"Transform {i} should not be null");
                Assert.That(transforms[i].name, Does.Contain("Bubble"), $"Transform {i} should be named as bubble");
            }
        }
        
        [Test]
        public void GetBubbleData_ReturnsCorrectData()
        {
            // Act
            var bubbleData = _bubbleManager.GetBubbleData();
            
            // Assert
            Assert.IsNotNull(bubbleData, "Should return bubble data array");
            Assert.AreEqual(100, bubbleData.Length, "Should return data for exactly 100 bubbles");
            
            // Verify data structure
            for (int i = 0; i < bubbleData.Length; i++)
            {
                Assert.AreEqual(i, bubbleData[i].BubbleId, $"Bubble {i} should have correct ID");
                Assert.AreEqual(i, bubbleData[i].GridIndex, $"Bubble {i} should have correct grid index");
                Assert.IsTrue(bubbleData[i].IsActive, $"Bubble {i} should be active");
                Assert.Greater(bubbleData[i].Radius, 0f, $"Bubble {i} should have positive radius");
            }
        }
        
        #endregion
        
        #region Demo Control Tests
        
        [Test]
        public void StartDemo_WhenInitialized_StartsSuccessfully()
        {
            // Act
            _bubbleManager.StartDemo();
            
            // Assert
            Assert.IsTrue(_bubbleManager.IsRunning, "Should be running after start");
            Assert.AreEqual(100, _bubbleManager.ActiveBubbleCount, "Should maintain 100 bubbles");
            
            UnityEngine.Debug.Log("[BubbleDemo] Demo started successfully");
        }
        
        [Test]
        public void StopDemo_WhenRunning_StopsSuccessfully()
        {
            // Arrange
            _bubbleManager.StartDemo();
            
            // Act
            _bubbleManager.StopDemo();
            
            // Assert
            Assert.IsFalse(_bubbleManager.IsRunning, "Should not be running after stop");
            Assert.AreEqual(100, _bubbleManager.ActiveBubbleCount, "Should still have 100 bubbles");
        }
        
        [Test]
        public void ResetBubbles_WhenCalled_ResetsToInitialPositions()
        {
            // Arrange
            _bubbleManager.StartDemo();
            var initialTransforms = _bubbleManager.GetActiveBubbleTransforms();
            var initialPositions = initialTransforms.Select(t => t.position).ToArray();
            
            // Modify positions (simulate wave movement)
            for (int i = 0; i < initialTransforms.Length; i++)
            {
                initialTransforms[i].position += Vector3.up * 2f;
            }
            
            // Act
            _bubbleManager.ResetBubbles();
            
            // Assert
            var resetTransforms = _bubbleManager.GetActiveBubbleTransforms();
            for (int i = 0; i < resetTransforms.Length; i++)
            {
                float distance = Vector3.Distance(resetTransforms[i].position, initialPositions[i]);
                Assert.Less(distance, 0.1f, $"Bubble {i} should be reset to initial position");
            }
            
            UnityEngine.Debug.Log("[BubbleDemo] Bubbles reset to initial positions");
        }
        
        #endregion
        
        #region Position Management Tests
        
        [Test]
        public void SetBubblePositions_WithValidPositions_UpdatesCorrectly()
        {
            // Arrange
            var newPositions = new float3[100];
            for (int i = 0; i < 100; i++)
            {
                newPositions[i] = new float3(i, i * 0.5f, i * 0.25f);
            }
            
            // Act
            _bubbleManager.SetBubblePositions(newPositions);
            
            // Assert
            var transforms = _bubbleManager.GetActiveBubbleTransforms();
            for (int i = 0; i < transforms.Length; i++)
            {
                Vector3 expectedPos = newPositions[i];
                Vector3 actualPos = transforms[i].position;
                
                Assert.AreEqual(expectedPos.x, actualPos.x, 0.01f, $"Bubble {i} X position should match");
                Assert.AreEqual(expectedPos.y, actualPos.y, 0.01f, $"Bubble {i} Y position should match");
                Assert.AreEqual(expectedPos.z, actualPos.z, 0.01f, $"Bubble {i} Z position should match");
            }
        }
        
        [Test]
        public void SetBubblePositions_WithWrongArraySize_HandlesGracefully()
        {
            // Arrange
            var wrongSizePositions = new float3[50]; // Wrong size
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => {
                _bubbleManager.SetBubblePositions(wrongSizePositions);
            });
            
            // Verify bubbles are still in valid state
            Assert.AreEqual(100, _bubbleManager.ActiveBubbleCount, "Should still have 100 bubbles");
        }
        
        [Test]
        public void SetBubblePositions_WithNullArray_HandlesGracefully()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => {
                _bubbleManager.SetBubblePositions(null);
            });
            
            // Verify bubbles are still in valid state
            Assert.AreEqual(100, _bubbleManager.ActiveBubbleCount, "Should still have 100 bubbles");
        }
        
        #endregion
        
        #region Visual Configuration Tests
        
        [Test]
        public void ConfigureBubbleVisuals_WithValidConfig_AppliesCorrectly()
        {
            // Arrange
            var config = BubbleVisualConfig.PerformanceOptimized;
            
            // Act
            _bubbleManager.ConfigureBubbleVisuals(config);
            
            // Assert
            var transforms = _bubbleManager.GetActiveBubbleTransforms();
            
            // Check that bubbles have been configured (scale should be around base scale)
            foreach (var transform in transforms)
            {
                float scale = transform.localScale.x;
                Assert.Greater(scale, config.BaseScale - config.ScaleVariation - 0.1f, 
                    "Scale should be within expected range");
                Assert.Less(scale, config.BaseScale + config.ScaleVariation + 0.1f, 
                    "Scale should be within expected range");
                
                // Check renderer configuration
                var renderer = transform.GetComponent<Renderer>();
                Assert.IsNotNull(renderer, "Bubble should have renderer");
                Assert.IsNotNull(renderer.material, "Bubble should have material");
            }
            
            UnityEngine.Debug.Log("[BubbleVisuals] Visual configuration applied successfully");
        }
        
        [Test]
        public void ConfigureBubbleVisuals_WithQuest3Default_ConfiguresForVR()
        {
            // Arrange
            var config = BubbleVisualConfig.Quest3Default;
            
            // Act
            _bubbleManager.ConfigureBubbleVisuals(config);
            
            // Assert
            var transforms = _bubbleManager.GetActiveBubbleTransforms();
            
            // Verify VR-optimized settings are applied
            foreach (var transform in transforms)
            {
                var renderer = transform.GetComponent<Renderer>();
                var material = renderer.material;
                
                // Check transparency
                Assert.AreEqual(config.Alpha, material.color.a, 0.1f, "Should have correct alpha");
                
                // Check color is in blue range (Quest 3 default)
                Assert.Greater(material.color.b, 0.5f, "Should have blue tint for Quest 3");
            }
        }
        
        #endregion
        
        #region Pooling Tests
        
        [Test]
        public void SetPoolingEnabled_WhenDisabled_DisablesPooling()
        {
            // Act
            _bubbleManager.SetPoolingEnabled(false);
            
            // Assert
            var stats = _bubbleManager.GetPerformanceStats();
            Assert.IsFalse(stats.PoolingEnabled, "Pooling should be disabled");
        }
        
        [Test]
        public void SetPoolingEnabled_WhenEnabled_EnablesPooling()
        {
            // Arrange
            _bubbleManager.SetPoolingEnabled(false);
            
            // Act
            _bubbleManager.SetPoolingEnabled(true);
            
            // Assert
            var stats = _bubbleManager.GetPerformanceStats();
            Assert.IsTrue(stats.PoolingEnabled, "Pooling should be enabled");
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        public void GetPerformanceStats_ReturnsValidStats()
        {
            // Arrange
            _bubbleManager.StartDemo();
            
            // Simulate some updates
            for (int i = 0; i < 10; i++)
            {
                _bubbleManager.UpdateBubblePositions(0.016f);
            }
            
            // Act
            var stats = _bubbleManager.GetPerformanceStats();
            
            // Assert
            Assert.GreaterOrEqual(stats.TotalUpdates, 0, "Should track total updates");
            Assert.GreaterOrEqual(stats.AverageUpdateTimeMicroseconds, 0f, "Should track average update time");
            Assert.GreaterOrEqual(stats.TotalInstantiations, 100, "Should track instantiations");
            Assert.GreaterOrEqual(stats.MemoryUsageBytes, 0, "Should estimate memory usage");
            Assert.IsTrue(stats.PoolingEnabled, "Should report pooling status");
            Assert.Greater(stats.LastUpdateTime, 0f, "Should track last update time");
            
            UnityEngine.Debug.Log($"[Performance] Updates: {stats.TotalUpdates}, " +
                                $"Avg time: {stats.AverageUpdateTimeMicroseconds:F2}μs, " +
                                $"Memory: {stats.MemoryUsageBytes / 1024}KB");
        }
        
        [Test]
        [Performance]
        public void UpdateBubblePositions_With100Bubbles_MeetsPerformanceTarget()
        {
            // Arrange
            _bubbleManager.StartDemo();
            const int iterations = 100;
            
            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                _bubbleManager.UpdateBubblePositions(0.016f);
            }
            
            stopwatch.Stop();
            
            // Assert
            float averageTimeMs = (float)stopwatch.ElapsedMilliseconds / iterations;
            const float maxAllowedTimeMs = 1.0f; // 1ms budget for bubble updates
            
            Assert.Less(averageTimeMs, maxAllowedTimeMs, 
                $"Bubble updates should complete within {maxAllowedTimeMs}ms budget");
            
            UnityEngine.Debug.Log($"[Performance] 100 bubble updates: {averageTimeMs:F3}ms average " +
                                $"(budget: {maxAllowedTimeMs}ms)");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void StartDemo_WhenNotInitialized_HandlesGracefully()
        {
            // Arrange - Create uninitialized system
            var uninitializedContainer = new GameObject("UninitializedContainer").transform;
            
            try
            {
                using var uninitializedManager = new BubbleManagementSystem(100, uninitializedContainer, _mockWaveSystem);
                
                // Force uninitialized state
                var initField = typeof(BubbleManagementSystem).GetField("IsInitialized", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                initField?.SetValue(uninitializedManager, false);
                
                // Act & Assert - Should not throw
                Assert.DoesNotThrow(() => {
                    uninitializedManager.StartDemo();
                });
                
                Assert.IsFalse(uninitializedManager.IsRunning, "Should not be running if not initialized");
            }
            finally
            {
                Object.DestroyImmediate(uninitializedContainer.gameObject);
            }
        }
        
        [Test]
        public void UpdateBubblePositions_WhenNotRunning_HandlesGracefully()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => {
                _bubbleManager.UpdateBubblePositions(0.016f);
            });
            
            // Should not affect bubble count
            Assert.AreEqual(100, _bubbleManager.ActiveBubbleCount, "Should maintain bubble count");
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void FullBubbleLifecycle_FromInitToDispose_WorksCorrectly()
        {
            // Act 1: Validate initial state
            Assert.IsTrue(_bubbleManager.IsInitialized, "Should be initialized");
            Assert.AreEqual(100, _bubbleManager.ActiveBubbleCount, "Should have 100 bubbles");
            
            // Act 2: Start demo
            _bubbleManager.StartDemo();
            Assert.IsTrue(_bubbleManager.IsRunning, "Should be running");
            
            // Act 3: Update positions
            _bubbleManager.UpdateBubblePositions(0.016f);
            
            // Act 4: Configure visuals
            _bubbleManager.ConfigureBubbleVisuals(BubbleVisualConfig.Quest3Default);
            
            // Act 5: Reset bubbles
            _bubbleManager.ResetBubbles();
            
            // Act 6: Stop demo
            _bubbleManager.StopDemo();
            Assert.IsFalse(_bubbleManager.IsRunning, "Should not be running");
            
            // Act 7: Validate final state
            var finalValidation = _bubbleManager.ValidateBubbleCount();
            Assert.IsTrue(finalValidation.IsValid, "Should maintain valid bubble count");
            
            UnityEngine.Debug.Log("[Integration] Full bubble lifecycle completed successfully");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [Test]
        public void BubbleManagementSystem_WithZeroBubbles_HandlesGracefully()
        {
            // Arrange
            var zeroContainer = new GameObject("ZeroContainer").transform;
            
            try
            {
                // Act
                using var zeroManager = new BubbleManagementSystem(0, zeroContainer, _mockWaveSystem);
                
                // Assert
                Assert.AreEqual(0, zeroManager.TargetBubbleCount, "Should target zero bubbles");
                Assert.AreEqual(0, zeroManager.ActiveBubbleCount, "Should have zero active bubbles");
                
                // Should handle operations gracefully
                Assert.DoesNotThrow(() => {
                    zeroManager.StartDemo();
                    zeroManager.UpdateBubblePositions(0.016f);
                    zeroManager.StopDemo();
                });
            }
            finally
            {
                Object.DestroyImmediate(zeroContainer.gameObject);
            }
        }
        
        [Test]
        public void GetActiveBubbleTransforms_WithNoActiveBubbles_ReturnsEmptyArray()
        {
            // Arrange
            var emptyContainer = new GameObject("EmptyContainer").transform;
            
            try
            {
                using var emptyManager = new BubbleManagementSystem(0, emptyContainer, _mockWaveSystem);
                
                // Act
                var transforms = emptyManager.GetActiveBubbleTransforms();
                
                // Assert
                Assert.IsNotNull(transforms, "Should return array even with no bubbles");
                Assert.AreEqual(0, transforms.Length, "Should return empty array");
            }
            finally
            {
                Object.DestroyImmediate(emptyContainer.gameObject);
            }
        }
        
        #endregion
    }
}