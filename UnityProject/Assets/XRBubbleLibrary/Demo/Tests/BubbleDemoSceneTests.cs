using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.Demo.Tests
{
    /// <summary>
    /// Comprehensive tests for the 100-bubble demo scene infrastructure.
    /// Validates scene setup, bubble management, and Quest 3 optimization.
    /// Implements Requirement 5.1: Demonstrate wave mathematics with exactly 100 bubbles.
    /// Implements Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public class BubbleDemoSceneTests
    {
        private GameObject _testSceneObject;
        private BubbleDemoSceneManager _sceneManager;
        private Camera _testCamera;
        private Light _testLight;
        
        [SetUp]
        public void SetUp()
        {
            // Create test scene object
            _testSceneObject = new GameObject("TestBubbleDemoScene");
            _sceneManager = _testSceneObject.AddComponent<BubbleDemoSceneManager>();
            
            // Create test camera
            var cameraGO = new GameObject("TestCamera");
            _testCamera = cameraGO.AddComponent<Camera>();
            
            // Create test light
            var lightGO = new GameObject("TestLight");
            _testLight = lightGO.AddComponent<Light>();
            
            // Set references using reflection to access private fields
            var cameraField = typeof(BubbleDemoSceneManager).GetField("_mainCamera", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cameraField?.SetValue(_sceneManager, _testCamera);
            
            var lightField = typeof(BubbleDemoSceneManager).GetField("_mainLight", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            lightField?.SetValue(_sceneManager, _testLight);
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_testSceneObject != null)
            {
                Object.DestroyImmediate(_testSceneObject);
            }
            
            if (_testCamera != null)
            {
                Object.DestroyImmediate(_testCamera.gameObject);
            }
            
            if (_testLight != null)
            {
                Object.DestroyImmediate(_testLight.gameObject);
            }
        }
        
        #region Scene Validation Tests
        
        [Test]
        public void ValidateScene_WithValidSetup_ReturnsSuccess()
        {
            // Act
            var result = _sceneManager.ValidateScene();
            
            // Assert
            Assert.IsTrue(result.IsValid, "Valid scene setup should pass validation");
            Assert.AreEqual(100, result.BubbleCount, "Should target exactly 100 bubbles");
            Assert.AreEqual(72f, result.FrameRateTarget, "Should target 72 FPS for Quest 3");
            Assert.AreEqual(0, result.Issues.Length, "Should have no validation issues");
            
            UnityEngine.Debug.Log($"[SceneValidation] Validation passed: {result.BubbleCount} bubbles, {result.FrameRateTarget} FPS target");
        }
        
        [Test]
        public void ValidateScene_WithMissingCamera_ReturnsFailure()
        {
            // Arrange - Remove camera reference
            var cameraField = typeof(BubbleDemoSceneManager).GetField("_mainCamera", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cameraField?.SetValue(_sceneManager, null);
            
            // Act
            var result = _sceneManager.ValidateScene();
            
            // Assert
            Assert.IsFalse(result.IsValid, "Missing camera should fail validation");
            Assert.Greater(result.Issues.Length, 0, "Should report validation issues");
            Assert.That(result.Issues, Has.Some.Contains("camera"), "Should mention missing camera");
        }
        
        [Test]
        public void ValidateScene_WithWrongBubbleCount_ReturnsFailure()
        {
            // Arrange - Set wrong bubble count using reflection
            var bubbleCountField = typeof(BubbleDemoSceneManager).GetField("_targetBubbleCount", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bubbleCountField?.SetValue(_sceneManager, 50); // Wrong count
            
            // Act
            var result = _sceneManager.ValidateScene();
            
            // Assert
            Assert.IsFalse(result.IsValid, "Wrong bubble count should fail validation");
            Assert.AreEqual(50, result.BubbleCount, "Should report actual bubble count");
            Assert.That(result.Issues, Has.Some.Contains("100"), "Should mention required count of 100");
        }
        
        #endregion
        
        #region Scene Initialization Tests
        
        [UnityTest]
        public IEnumerator InitializeScene_WithValidSetup_InitializesCorrectly()
        {
            // Wait for initialization
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            Assert.IsTrue(_sceneManager.IsInitialized, "Scene should be initialized");
            Assert.AreEqual(100, _sceneManager.CurrentBubbleCount, "Should have exactly 100 bubbles");
            
            UnityEngine.Debug.Log($"[SceneInit] Scene initialized with {_sceneManager.CurrentBubbleCount} bubbles");
        }
        
        [Test]
        public void Quest3Optimizations_WhenApplied_ConfiguresCorrectly()
        {
            // Act - Apply optimizations through context menu (simulated)
            var method = typeof(BubbleDemoSceneManager).GetMethod("ApplyQuest3OptimizationsFromEditor", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(_sceneManager, null);
            
            // Assert
            Assert.AreEqual(72, Application.targetFrameRate, "Should set Quest 3 target frame rate");
            Assert.AreEqual(90f, _testCamera.fieldOfView, "Should set VR field of view");
            Assert.AreEqual(0.1f, _testCamera.nearClipPlane, "Should set appropriate near clip plane");
            Assert.IsFalse(_testCamera.allowHDR, "Should disable HDR for performance");
            Assert.IsTrue(_testCamera.allowMSAA, "Should enable MSAA for VR");
            
            Assert.AreEqual(LightShadows.None, _testLight.shadows, "Should disable shadows for performance");
            Assert.AreEqual(1.2f, _testLight.intensity, "Should set VR-appropriate light intensity");
            
            UnityEngine.Debug.Log("[Quest3] Optimizations applied successfully");
        }
        
        #endregion
        
        #region Performance Monitoring Tests
        
        [UnityTest]
        public IEnumerator PerformanceMonitoring_DuringDemo_TracksFrameTime()
        {
            // Arrange
            _sceneManager.StartDemo();
            
            // Act - Let demo run for a few frames
            yield return new WaitForSeconds(0.2f);
            
            var stats = _sceneManager.GetPerformanceStats();
            
            // Assert
            Assert.Greater(stats.FrameCount, 0, "Should track frame count");
            Assert.Greater(stats.AverageFrameTime, 0f, "Should track average frame time");
            Assert.Less(stats.AverageFrameTime, 50f, "Frame time should be reasonable (< 50ms)");
            Assert.AreEqual(100, stats.BubbleCount, "Should track correct bubble count");
            Assert.IsTrue(stats.IsRunning, "Should report running state");
            
            UnityEngine.Debug.Log($"[Performance] Avg frame time: {stats.AverageFrameTime:F2}ms, " +
                                $"Frame count: {stats.FrameCount}");
        }
        
        [Test]
        public void GetPerformanceStats_WhenCalled_ReturnsValidStats()
        {
            // Act
            var stats = _sceneManager.GetPerformanceStats();
            
            // Assert
            Assert.GreaterOrEqual(stats.FrameCount, 0, "Frame count should be non-negative");
            Assert.GreaterOrEqual(stats.AverageFrameTime, 0f, "Average frame time should be non-negative");
            Assert.AreEqual(13.89f, stats.TargetFrameTime, 0.01f, "Should have Quest 3 target frame time");
            Assert.IsNotNull(stats.WaveSystemStats, "Should include wave system stats");
        }
        
        #endregion
        
        #region Demo Control Tests
        
        [Test]
        public void StartDemo_WhenInitialized_StartsSuccessfully()
        {
            // Arrange
            // Wait for initialization in a coroutine context would be better, but for unit test:
            var initMethod = typeof(BubbleDemoSceneManager).GetMethod("InitializeScene", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            initMethod?.Invoke(_sceneManager, null);
            
            // Act
            _sceneManager.StartDemo();
            
            // Assert
            Assert.IsTrue(_sceneManager.IsRunning, "Demo should be running after start");
            
            UnityEngine.Debug.Log("[DemoControl] Demo started successfully");
        }
        
        [Test]
        public void StopDemo_WhenRunning_StopsSuccessfully()
        {
            // Arrange
            var initMethod = typeof(BubbleDemoSceneManager).GetMethod("InitializeScene", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            initMethod?.Invoke(_sceneManager, null);
            _sceneManager.StartDemo();
            
            // Act
            _sceneManager.StopDemo();
            
            // Assert
            Assert.IsFalse(_sceneManager.IsRunning, "Demo should not be running after stop");
            
            UnityEngine.Debug.Log("[DemoControl] Demo stopped successfully");
        }
        
        [Test]
        public void ResetDemo_WhenCalled_ResetsToInitialState()
        {
            // Arrange
            var initMethod = typeof(BubbleDemoSceneManager).GetMethod("InitializeScene", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            initMethod?.Invoke(_sceneManager, null);
            _sceneManager.StartDemo();
            
            // Act
            _sceneManager.ResetDemo();
            
            // Assert
            Assert.IsFalse(_sceneManager.IsRunning, "Demo should not be running after reset");
            Assert.AreEqual(0f, _sceneManager.AverageFrameTime, "Average frame time should be reset");
            
            UnityEngine.Debug.Log("[DemoControl] Demo reset successfully");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void StartDemo_WhenNotInitialized_HandlesGracefully()
        {
            // Arrange - Create uninitialized scene manager
            var uninitializedGO = new GameObject("UninitializedScene");
            var uninitializedManager = uninitializedGO.AddComponent<BubbleDemoSceneManager>();
            
            try
            {
                // Act & Assert - Should not throw
                Assert.DoesNotThrow(() => {
                    uninitializedManager.StartDemo();
                });
                
                Assert.IsFalse(uninitializedManager.IsRunning, "Should not be running if not initialized");
            }
            finally
            {
                Object.DestroyImmediate(uninitializedGO);
            }
        }
        
        [Test]
        public void ValidateScene_WithNullReferences_HandlesGracefully()
        {
            // Arrange - Create scene manager with null references
            var nullRefGO = new GameObject("NullRefScene");
            var nullRefManager = nullRefGO.AddComponent<BubbleDemoSceneManager>();
            
            try
            {
                // Act & Assert - Should not throw
                Assert.DoesNotThrow(() => {
                    var result = nullRefManager.ValidateScene();
                    Assert.IsFalse(result.IsValid, "Should fail validation with null references");
                });
            }
            finally
            {
                Object.DestroyImmediate(nullRefGO);
            }
        }
        
        #endregion
        
        #region Integration Tests
        
        [UnityTest]
        public IEnumerator FullDemoWorkflow_FromStartToStop_WorksCorrectly()
        {
            // Arrange - Wait for initialization
            yield return new WaitForSeconds(0.1f);
            
            // Act 1: Validate scene
            var validationResult = _sceneManager.ValidateScene();
            Assert.IsTrue(validationResult.IsValid, "Scene should be valid");
            
            // Act 2: Start demo
            _sceneManager.StartDemo();
            Assert.IsTrue(_sceneManager.IsRunning, "Demo should be running");
            
            // Act 3: Let demo run
            yield return new WaitForSeconds(0.5f);
            
            // Act 4: Check performance
            var stats = _sceneManager.GetPerformanceStats();
            Assert.Greater(stats.FrameCount, 0, "Should have processed frames");
            Assert.AreEqual(100, stats.BubbleCount, "Should maintain 100 bubbles");
            
            // Act 5: Stop demo
            _sceneManager.StopDemo();
            Assert.IsFalse(_sceneManager.IsRunning, "Demo should be stopped");
            
            // Act 6: Reset demo
            _sceneManager.ResetDemo();
            
            UnityEngine.Debug.Log("[Integration] Full demo workflow completed successfully");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [Test]
        public void SceneManager_WithMinimalSetup_InitializesBasicComponents()
        {
            // Arrange - Create minimal scene manager
            var minimalGO = new GameObject("MinimalScene");
            var minimalManager = minimalGO.AddComponent<BubbleDemoSceneManager>();
            
            try
            {
                // Act - Let Awake() run
                yield return null;
                
                // Assert - Should create basic components
                Assert.IsNotNull(minimalManager, "Scene manager should be created");
                
                var validation = minimalManager.ValidateScene();
                // Should fail validation but not crash
                Assert.IsNotNull(validation, "Should return validation result");
            }
            finally
            {
                Object.DestroyImmediate(minimalGO);
            }
        }
        
        [Test]
        public void PerformanceStats_WithZeroFrames_ReturnsValidDefaults()
        {
            // Act
            var stats = _sceneManager.GetPerformanceStats();
            
            // Assert
            Assert.AreEqual(0, stats.FrameCount, "Should start with zero frames");
            Assert.AreEqual(0f, stats.AverageFrameTime, "Should start with zero average time");
            Assert.Greater(stats.TargetFrameTime, 0f, "Should have positive target frame time");
            Assert.IsFalse(stats.IsRunning, "Should not be running initially");
        }
        
        #endregion
    }
}