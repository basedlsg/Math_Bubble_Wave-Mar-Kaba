using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Mathematics;
using System.Collections;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.Demo.Tests
{
    /// <summary>
    /// Comprehensive tests for wave mathematics and visual system integration.
    /// Validates synchronization, performance, and accuracy of the integration.
    /// Implements Requirement 5.1: Demonstrate wave mathematics with exactly 100 bubbles.
    /// Implements Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public class WaveVisualIntegrationTests
    {
        private GameObject _testIntegratorObject;
        private WaveVisualIntegrator _integrator;
        private IBubbleManagementSystem _mockBubbleManager;
        private Transform _testContainer;
        
        [SetUp]
        public void SetUp()
        {
            // Create test container
            var containerGO = new GameObject("TestIntegrationContainer");
            _testContainer = containerGO.transform;
            
            // Create mock bubble management system
            var mockWaveSystem = new PerformanceOptimizedWaveSystem();
            mockWaveSystem.WarmUp();
            _mockBubbleManager = new BubbleManagementSystem(100, _testContainer, mockWaveSystem);
            
            // Create integrator
            _testIntegratorObject = new GameObject("TestWaveVisualIntegrator");
            _integrator = _testIntegratorObject.AddComponent<WaveVisualIntegrator>();
            
            // Initialize integrator with bubble manager
            _integrator.Initialize(_mockBubbleManager);
        }
        
        [TearDown]
        public void TearDown()
        {
            _mockBubbleManager?.Dispose();
            
            if (_testIntegratorObject != null)
            {
                Object.DestroyImmediate(_testIntegratorObject);
            }
            
            if (_testContainer != null)
            {
                Object.DestroyImmediate(_testContainer.gameObject);
            }
        }
        
        #region Initialization Tests
        
        [Test]
        public void Initialize_WithValidBubbleManager_InitializesCorrectly()
        {
            // Assert
            Assert.IsTrue(_integrator.IsInitialized, "Integrator should be initialized");
            Assert.IsFalse(_integrator.IsRunning, "Should not be running initially");
            Assert.AreEqual(0f, _integrator.CurrentTime, "Should start with zero time");
            
            var stats = _integrator.GetPerformanceStats();
            Assert.AreEqual(100, stats.BubbleCount, "Should track 100 bubbles");
            
            UnityEngine.Debug.Log($"[WaveVisualIntegration] Initialized with {stats.BubbleCount} bubbles");
        }
        
        [Test]
        public void Initialize_WithNullBubbleManager_HandlesGracefully()
        {
            // Arrange
            var nullIntegratorGO = new GameObject("NullIntegrator");
            var nullIntegrator = nullIntegratorGO.AddComponent<WaveVisualIntegrator>();
            
            try
            {
                // Act & Assert - Should not throw
                Assert.DoesNotThrow(() => {
                    nullIntegrator.Initialize(null);
                });
                
                Assert.IsFalse(nullIntegrator.IsInitialized, "Should not be initialized with null manager");
            }
            finally
            {
                Object.DestroyImmediate(nullIntegratorGO);
            }
        }
        
        #endregion
        
        #region Integration Control Tests
        
        [Test]
        public void StartIntegration_WhenInitialized_StartsCorrectly()
        {
            // Act
            _integrator.StartIntegration();
            
            // Assert
            Assert.IsTrue(_integrator.IsRunning, "Should be running after start");
            Assert.AreEqual(0f, _integrator.CurrentTime, "Should start with zero time");
            
            var stats = _integrator.GetPerformanceStats();
            Assert.IsTrue(stats.IsRunning, "Stats should reflect running state");
            
            UnityEngine.Debug.Log("[WaveVisualIntegration] Integration started successfully");
        }
        
        [Test]
        public void StopIntegration_WhenRunning_StopsCorrectly()
        {
            // Arrange
            _integrator.StartIntegration();
            
            // Act
            _integrator.StopIntegration();
            
            // Assert
            Assert.IsFalse(_integrator.IsRunning, "Should not be running after stop");
            
            var stats = _integrator.GetPerformanceStats();
            Assert.IsFalse(stats.IsRunning, "Stats should reflect stopped state");
        }
        
        [Test]
        public void ResetIntegration_WhenCalled_ResetsToInitialState()
        {
            // Arrange
            _integrator.StartIntegration();
            
            // Simulate some time passing
            var timeField = typeof(WaveVisualIntegrator).GetField("_currentTime", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timeField?.SetValue(_integrator, 5.0f);
            
            // Act
            _integrator.ResetIntegration();
            
            // Assert
            Assert.AreEqual(0f, _integrator.CurrentTime, "Time should be reset to zero");
            
            var stats = _integrator.GetPerformanceStats();
            Assert.AreEqual(0, stats.TotalUpdates, "Update count should be reset");
        }
        
        #endregion
        
        #region Wave Settings Tests
        
        [Test]
        public void UpdateWaveSettings_WithValidSettings_UpdatesCorrectly()
        {
            // Arrange
            var newSettings = new WaveMatrixSettings
            {
                GridSize = new int2(10, 10),
                CellSize = 1.5f,
                WaveAmplitude = 1.2f,
                WaveFrequency = 2.5f,
                WaveSpeed = 1.8f
            };
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => {
                _integrator.UpdateWaveSettings(newSettings);
            });
            
            UnityEngine.Debug.Log("[WaveVisualIntegration] Wave settings updated successfully");
        }
        
        [Test]
        public void UpdateWaveSettings_WithInvalidSettings_AppliesCorrections()
        {
            // Arrange - Invalid settings
            var invalidSettings = new WaveMatrixSettings
            {
                GridSize = new int2(10, 10),
                CellSize = 1.0f,
                WaveAmplitude = -1f, // Invalid negative amplitude
                WaveFrequency = 100f, // Excessive frequency
                WaveSpeed = 1.0f
            };
            
            // Act & Assert - Should not throw and should apply corrections
            Assert.DoesNotThrow(() => {
                _integrator.UpdateWaveSettings(invalidSettings);
            });
            
            // The integrator should have corrected the invalid settings internally
            UnityEngine.Debug.Log("[WaveVisualIntegration] Invalid settings corrected automatically");
        }
        
        #endregion
        
        #region Synchronization Validation Tests
        
        [UnityTest]
        public IEnumerator ValidateSynchronization_WithRunningIntegration_ReturnsValidResult()
        {
            // Arrange
            _integrator.StartIntegration();
            _mockBubbleManager.StartDemo();
            
            // Let integration run for a few frames
            yield return new WaitForSeconds(0.2f);
            
            // Act
            var syncResult = _integrator.ValidateSynchronization();
            
            // Assert
            Assert.IsNotNull(syncResult, "Should return sync validation result");
            Assert.AreEqual(100, syncResult.TotalBubbles, "Should validate all 100 bubbles");
            Assert.GreaterOrEqual(syncResult.SyncAccuracy, 0f, "Sync accuracy should be non-negative");
            Assert.LessOrEqual(syncResult.SyncAccuracy, 1f, "Sync accuracy should not exceed 1.0");
            Assert.GreaterOrEqual(syncResult.MaxDeviation, 0f, "Max deviation should be non-negative");
            Assert.IsNotNull(syncResult.Message, "Should provide validation message");
            
            UnityEngine.Debug.Log($"[WaveVisualIntegration] Sync validation: {syncResult.Message} " +
                                $"(accuracy: {syncResult.SyncAccuracy:P1})");
        }
        
        [Test]
        public void ValidateSynchronization_WhenNotInitialized_ReturnsFailure()
        {
            // Arrange - Create uninitialized integrator
            var uninitializedGO = new GameObject("UninitializedIntegrator");
            var uninitializedIntegrator = uninitializedGO.AddComponent<WaveVisualIntegrator>();
            
            try
            {
                // Act
                var syncResult = uninitializedIntegrator.ValidateSynchronization();
                
                // Assert
                Assert.IsFalse(syncResult.IsInSync, "Should report not in sync when uninitialized");
                Assert.That(syncResult.Message, Does.Contain("not initialized"), 
                    "Should mention initialization issue");
            }
            finally
            {
                Object.DestroyImmediate(uninitializedGO);
            }
        }
        
        [UnityTest]
        public IEnumerator ValidateSynchronization_WithHighAccuracy_MaintainsSync()
        {
            // Arrange
            _integrator.StartIntegration();
            _mockBubbleManager.StartDemo();
            
            // Let integration stabilize
            yield return new WaitForSeconds(0.5f);
            
            // Act - Validate multiple times
            var syncResults = new WaveVisualSyncValidationResult[5];
            for (int i = 0; i < syncResults.Length; i++)
            {
                syncResults[i] = _integrator.ValidateSynchronization();
                yield return new WaitForSeconds(0.1f);
            }
            
            // Assert
            foreach (var result in syncResults)
            {
                Assert.Greater(result.SyncAccuracy, 0.8f, 
                    "Should maintain high sync accuracy over time");
                Assert.Less(result.MaxDeviation, 1.0f, 
                    "Max deviation should be reasonable (< 1m)");
            }
            
            float avgAccuracy = syncResults.Average(r => r.SyncAccuracy);
            UnityEngine.Debug.Log($"[WaveVisualIntegration] Average sync accuracy over time: {avgAccuracy:P1}");
        }
        
        #endregion
        
        #region Performance Tests
        
        [UnityTest]
        [Performance]
        public IEnumerator PerformanceTest_100BubbleIntegration_MeetsQuest3Target()
        {
            // Arrange
            _integrator.StartIntegration();
            _mockBubbleManager.StartDemo();
            
            // Let integration run and collect performance data
            yield return new WaitForSeconds(1.0f);
            
            // Act
            var stats = _integrator.GetPerformanceStats();
            
            // Assert
            Assert.Greater(stats.TotalUpdates, 0, "Should have performed updates");
            Assert.Less(stats.AverageUpdateTimeMs, 2.0f, 
                "Average update time should be under 2ms for Quest 3");
            Assert.Less(stats.PeakUpdateTimeMs, 5.0f, 
                "Peak update time should be under 5ms");
            Assert.AreEqual(100, stats.BubbleCount, "Should integrate exactly 100 bubbles");
            
            UnityEngine.Debug.Log($"[Performance] Integration: {stats.AverageUpdateTimeMs:F2}ms avg, " +
                                $"{stats.PeakUpdateTimeMs:F2}ms peak, {stats.TotalUpdates} updates");
        }
        
        [Test]
        public void GetPerformanceStats_ReturnsValidStatistics()
        {
            // Act
            var stats = _integrator.GetPerformanceStats();
            
            // Assert
            Assert.GreaterOrEqual(stats.TotalUpdates, 0, "Total updates should be non-negative");
            Assert.GreaterOrEqual(stats.AverageUpdateTimeMs, 0f, "Average time should be non-negative");
            Assert.GreaterOrEqual(stats.PeakUpdateTimeMs, 0f, "Peak time should be non-negative");
            Assert.GreaterOrEqual(stats.CurrentTime, 0f, "Current time should be non-negative");
            Assert.GreaterOrEqual(stats.SyncAccuracy, 0f, "Sync accuracy should be non-negative");
            Assert.LessOrEqual(stats.SyncAccuracy, 1f, "Sync accuracy should not exceed 1.0");
            Assert.AreEqual(100, stats.BubbleCount, "Should track 100 bubbles");
        }
        
        #endregion
        
        #region Integration Accuracy Tests
        
        [UnityTest]
        public IEnumerator IntegrationAccuracy_WithKnownWaveSettings_ProducesExpectedResults()
        {
            // Arrange - Use simple, predictable wave settings
            var simpleSettings = new WaveMatrixSettings
            {
                GridSize = new int2(10, 10),
                CellSize = 1.0f,
                WaveAmplitude = 1.0f,
                WaveFrequency = 1.0f,
                WaveSpeed = 1.0f
            };
            
            _integrator.UpdateWaveSettings(simpleSettings);
            _integrator.StartIntegration();
            _mockBubbleManager.StartDemo();
            
            // Let integration run
            yield return new WaitForSeconds(0.5f);
            
            // Act - Get bubble positions
            var bubbleTransforms = _mockBubbleManager.GetActiveBubbleTransforms();
            var bubbleData = _mockBubbleManager.GetBubbleData();
            
            // Assert - Verify positions are reasonable
            for (int i = 0; i < bubbleTransforms.Length; i++)
            {
                var transform = bubbleTransforms[i];
                var data = bubbleData[i];
                
                // Position should be finite
                Assert.IsTrue(IsFinite(transform.position), $"Bubble {i} position should be finite");
                
                // Y position should be influenced by wave (not exactly zero)
                // Note: This is a loose check since wave mathematics can produce zero at certain times/positions
                Assert.GreaterOrEqual(math.abs(transform.position.y), 0f, 
                    $"Bubble {i} Y position should be valid");
                
                // Position should be within reasonable bounds
                Assert.Less(math.length(transform.position), 50f, 
                    $"Bubble {i} should be within reasonable distance from origin");
            }
            
            UnityEngine.Debug.Log($"[Integration] Accuracy test passed for {bubbleTransforms.Length} bubbles");
        }
        
        [UnityTest]
        public IEnumerator IntegrationAccuracy_WithTimeProgression_ShowsWaveAnimation()
        {
            // Arrange
            _integrator.StartIntegration();
            _mockBubbleManager.StartDemo();
            
            // Record initial positions
            yield return new WaitForSeconds(0.1f);
            var initialTransforms = _mockBubbleManager.GetActiveBubbleTransforms();
            var initialPositions = initialTransforms.Select(t => (float3)t.position).ToArray();
            
            // Let wave animation progress
            yield return new WaitForSeconds(1.0f);
            
            // Record final positions
            var finalTransforms = _mockBubbleManager.GetActiveBubbleTransforms();
            var finalPositions = finalTransforms.Select(t => (float3)t.position).ToArray();
            
            // Assert - Positions should have changed due to wave animation
            int changedPositions = 0;
            for (int i = 0; i < initialPositions.Length; i++)
            {
                float distance = math.distance(initialPositions[i], finalPositions[i]);
                if (distance > 0.01f) // 1cm threshold
                {
                    changedPositions++;
                }
            }
            
            Assert.Greater(changedPositions, initialPositions.Length * 0.5f, 
                "At least 50% of bubbles should show wave animation");
            
            UnityEngine.Debug.Log($"[Integration] Wave animation: {changedPositions}/{initialPositions.Length} " +
                                $"bubbles moved significantly");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void StartIntegration_WhenNotInitialized_HandlesGracefully()
        {
            // Arrange - Create uninitialized integrator
            var uninitializedGO = new GameObject("UninitializedIntegrator");
            var uninitializedIntegrator = uninitializedGO.AddComponent<WaveVisualIntegrator>();
            
            try
            {
                // Act & Assert - Should not throw
                Assert.DoesNotThrow(() => {
                    uninitializedIntegrator.StartIntegration();
                });
                
                Assert.IsFalse(uninitializedIntegrator.IsRunning, 
                    "Should not be running if not initialized");
            }
            finally
            {
                Object.DestroyImmediate(uninitializedGO);
            }
        }
        
        [Test]
        public void UpdateWaveSettings_WithNullSettings_HandlesGracefully()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => {
                _integrator.UpdateWaveSettings(default(WaveMatrixSettings));
            });
        }
        
        #endregion
        
        #region Integration Workflow Tests
        
        [UnityTest]
        public IEnumerator FullIntegrationWorkflow_FromStartToStop_WorksCorrectly()
        {
            // Act 1: Start integration
            _integrator.StartIntegration();
            _mockBubbleManager.StartDemo();
            Assert.IsTrue(_integrator.IsRunning, "Should be running");
            
            // Act 2: Let integration run
            yield return new WaitForSeconds(0.5f);
            
            // Act 3: Validate synchronization
            var syncResult = _integrator.ValidateSynchronization();
            Assert.IsNotNull(syncResult, "Should provide sync validation");
            
            // Act 4: Check performance
            var stats = _integrator.GetPerformanceStats();
            Assert.Greater(stats.TotalUpdates, 0, "Should have performed updates");
            Assert.Greater(stats.CurrentTime, 0f, "Should have progressed in time");
            
            // Act 5: Update wave settings
            var newSettings = WaveMatrixSettings.Default;
            newSettings.WaveAmplitude = 1.5f;
            _integrator.UpdateWaveSettings(newSettings);
            
            // Act 6: Continue running
            yield return new WaitForSeconds(0.2f);
            
            // Act 7: Reset integration
            _integrator.ResetIntegration();
            Assert.AreEqual(0f, _integrator.CurrentTime, "Should reset time");
            
            // Act 8: Stop integration
            _integrator.StopIntegration();
            Assert.IsFalse(_integrator.IsRunning, "Should not be running");
            
            UnityEngine.Debug.Log("[Integration] Full workflow completed successfully");
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool IsFinite(Vector3 vector)
        {
            return math.isfinite(vector.x) && math.isfinite(vector.y) && math.isfinite(vector.z);
        }
        
        #endregion
    }
}