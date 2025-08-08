using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Linq;

namespace XRBubbleLibrary.Quest3.Tests
{
    /// <summary>
    /// Comprehensive unit tests for Auto-LOD Controller system.
    /// Tests Requirement 5.5: Automatic LOD system for performance maintenance.
    /// Tests Requirement 8.4: Auto-LOD controller for quality adjustment.
    /// </summary>
    [TestFixture]
    public class AutoLODControllerTests
    {
        private GameObject _testGameObject;
        private AutoLODController _autoLODController;
        private AutoLODConfiguration _testConfig;
        
        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("AutoLODControllerTest");
            _autoLODController = _testGameObject.AddComponent<AutoLODController>();
            
            _testConfig = new AutoLODConfiguration
            {
                TargetFPS = 72f,
                MinimumFPS = 60f,
                FrameThresholdCount = 3,
                AdjustmentCooldownSeconds = 1f,
                EnableBubbleCountAdjustment = true,
                EnableRenderingQualityAdjustment = true,
                EnableEffectQualityAdjustment = true,
                MaxLODLevel = 4,
                AdjustmentAggressiveness = 0.7f,
                EnableDebugLogging = false // Disable for tests
            };
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                Object.DestroyImmediate(_testGameObject);
            }
        }
        
        #region Initialization Tests
        
        [Test]
        public void Initialize_WithValidConfig_ReturnsTrue()
        {
            // Act
            bool result = _autoLODController.Initialize(_testConfig);
            
            // Assert
            Assert.IsTrue(result, "Auto-LOD controller initialization should succeed with valid config");
            Assert.IsTrue(_autoLODController.IsInitialized, "Controller should be initialized");
            Assert.IsTrue(_autoLODController.IsAutoLODEnabled, "Auto-LOD should be enabled by default");
            Assert.AreEqual(0, _autoLODController.CurrentLODLevel, "Should start at highest quality (LOD 0)");
            Assert.AreEqual(_testConfig.TargetFPS, _autoLODController.Configuration.TargetFPS, 
                "Configuration should be stored correctly");
        }
        
        [Test]
        public void Initialize_WithDefaultConfiguration_UsesCorrectValues()
        {
            // Arrange
            var defaultConfig = AutoLODConfiguration.Quest3Default;
            
            // Act
            bool result = _autoLODController.Initialize(defaultConfig);
            
            // Assert
            Assert.IsTrue(result, "Should initialize with default configuration");
            Assert.AreEqual(72f, defaultConfig.TargetFPS, "Default should target 72 FPS for Quest 3");
            Assert.AreEqual(60f, defaultConfig.MinimumFPS, "Default minimum should be 60 FPS");
            Assert.IsTrue(defaultConfig.EnableBubbleCountAdjustment, "Should enable bubble count adjustment");
            Assert.IsTrue(defaultConfig.EnableRenderingQualityAdjustment, "Should enable rendering quality adjustment");
        }
        
        [Test]
        public void Initialize_ConservativeConfiguration_HasCorrectSettings()
        {
            // Arrange
            var conservativeConfig = AutoLODConfiguration.Conservative;
            
            // Act
            bool result = _autoLODController.Initialize(conservativeConfig);
            
            // Assert
            Assert.IsTrue(result, "Should initialize with conservative configuration");
            Assert.AreEqual(65f, conservativeConfig.MinimumFPS, "Conservative should have higher minimum FPS");
            Assert.AreEqual(10, conservativeConfig.FrameThresholdCount, "Conservative should wait longer before adjusting");
            Assert.AreEqual(5f, conservativeConfig.AdjustmentCooldownSeconds, "Conservative should have longer cooldown");
            Assert.AreEqual(0.3f, conservativeConfig.AdjustmentAggressiveness, "Conservative should be less aggressive");
        }
        
        [Test]
        public void Initialize_AggressiveConfiguration_HasCorrectSettings()
        {
            // Arrange
            var aggressiveConfig = AutoLODConfiguration.Aggressive;
            
            // Act
            bool result = _autoLODController.Initialize(aggressiveConfig);
            
            // Assert
            Assert.IsTrue(result, "Should initialize with aggressive configuration");
            Assert.AreEqual(55f, aggressiveConfig.MinimumFPS, "Aggressive should have lower minimum FPS");
            Assert.AreEqual(3, aggressiveConfig.FrameThresholdCount, "Aggressive should adjust quickly");
            Assert.AreEqual(1f, aggressiveConfig.AdjustmentCooldownSeconds, "Aggressive should have short cooldown");
            Assert.AreEqual(1.0f, aggressiveConfig.AdjustmentAggressiveness, "Aggressive should be fully aggressive");
        }
        
        #endregion
        
        #region Auto-LOD Enable/Disable Tests
        
        [Test]
        public void SetAutoLODEnabled_False_DisablesAutoLOD()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            
            // Act
            _autoLODController.SetAutoLODEnabled(false);
            
            // Assert
            Assert.IsFalse(_autoLODController.IsAutoLODEnabled, "Auto-LOD should be disabled");
            Assert.AreEqual(0, _autoLODController.CurrentLODLevel, "Should reset to highest quality when disabled");
        }
        
        [Test]
        public void SetAutoLODEnabled_True_EnablesAutoLOD()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            _autoLODController.SetAutoLODEnabled(false);
            
            // Act
            _autoLODController.SetAutoLODEnabled(true);
            
            // Assert
            Assert.IsTrue(_autoLODController.IsAutoLODEnabled, "Auto-LOD should be enabled");
        }
        
        #endregion
        
        #region Performance Metrics Update Tests
        
        [Test]
        public void UpdatePerformanceMetrics_WithGoodPerformance_MaintainsLODLevel()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            int initialLODLevel = _autoLODController.CurrentLODLevel;
            
            // Act - Simulate good performance
            for (int i = 0; i < 10; i++)
            {
                _autoLODController.UpdatePerformanceMetrics(75f, 13.3f, 60f, 65f);
            }
            
            // Assert
            Assert.AreEqual(initialLODLevel, _autoLODController.CurrentLODLevel, 
                "LOD level should remain unchanged with good performance");
        }
        
        [Test]
        public void UpdatePerformanceMetrics_WithPoorPerformance_IncreasesLODLevel()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            int initialLODLevel = _autoLODController.CurrentLODLevel;
            bool lodLevelChanged = false;
            
            _autoLODController.LODLevelChanged += (args) => { lodLevelChanged = true; };
            
            // Act - Simulate poor performance for threshold count
            for (int i = 0; i < _testConfig.FrameThresholdCount + 1; i++)
            {
                _autoLODController.UpdatePerformanceMetrics(50f, 20f, 85f, 90f);
            }
            
            // Wait for cooldown
            System.Threading.Thread.Sleep((int)(_testConfig.AdjustmentCooldownSeconds * 1000) + 100);
            
            // Trigger another update
            _autoLODController.UpdatePerformanceMetrics(50f, 20f, 85f, 90f);
            
            // Assert
            Assert.IsTrue(lodLevelChanged, "LOD level should change with poor performance");
            Assert.Greater(_autoLODController.CurrentLODLevel, initialLODLevel, 
                "LOD level should increase (lower quality) with poor performance");
        }
        
        [Test]
        public void UpdatePerformanceMetrics_WithCriticalPerformance_AdjustsAggressively()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            int initialLODLevel = _autoLODController.CurrentLODLevel;
            
            // Act - Simulate critical performance below minimum FPS
            for (int i = 0; i < _testConfig.FrameThresholdCount + 1; i++)
            {
                _autoLODController.UpdatePerformanceMetrics(45f, 22f, 95f, 95f); // Below minimum FPS
            }
            
            // Wait for cooldown
            System.Threading.Thread.Sleep((int)(_testConfig.AdjustmentCooldownSeconds * 1000) + 100);
            
            // Trigger another update
            _autoLODController.UpdatePerformanceMetrics(45f, 22f, 95f, 95f);
            
            // Assert
            Assert.Greater(_autoLODController.CurrentLODLevel, initialLODLevel + 1, 
                "Should make aggressive adjustment for critical performance");
        }
        
        [Test]
        public void UpdatePerformanceMetrics_WithExcellentPerformance_DecreasesLODLevel()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            _autoLODController.SetManualLODLevel(2); // Start at higher LOD level
            int initialLODLevel = _autoLODController.CurrentLODLevel;
            
            // Act - Simulate excellent performance well above target
            for (int i = 0; i < 10; i++)
            {
                _autoLODController.UpdatePerformanceMetrics(90f, 11f, 40f, 45f);
            }
            
            // Wait for longer cooldown (2x for quality increase)
            System.Threading.Thread.Sleep((int)(_testConfig.AdjustmentCooldownSeconds * 2000) + 100);
            
            // Trigger another update
            _autoLODController.UpdatePerformanceMetrics(90f, 11f, 40f, 45f);
            
            // Assert
            Assert.Less(_autoLODController.CurrentLODLevel, initialLODLevel, 
                "LOD level should decrease (higher quality) with excellent performance");
        }
        
        #endregion
        
        #region Manual LOD Override Tests
        
        [Test]
        public void SetManualLODLevel_ValidLevel_SetsLODLevel()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            int targetLevel = 2;
            bool eventFired = false;
            
            _autoLODController.LODLevelChanged += (args) => { eventFired = true; };
            
            // Act
            _autoLODController.SetManualLODLevel(targetLevel);
            
            // Assert
            Assert.AreEqual(targetLevel, _autoLODController.CurrentLODLevel, "Should set manual LOD level");
            Assert.IsTrue(eventFired, "Should fire LOD level changed event");
        }
        
        [Test]
        public void SetManualLODLevel_WithDuration_ActivatesTemporaryOverride()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            int targetLevel = 3;
            float duration = 2f;
            
            // Act
            _autoLODController.SetManualLODLevel(targetLevel, duration);
            var status = _autoLODController.GetCurrentStatus();
            
            // Assert
            Assert.AreEqual(targetLevel, _autoLODController.CurrentLODLevel, "Should set manual LOD level");
            Assert.IsTrue(status.IsManualOverrideActive, "Manual override should be active");
            Assert.Greater(status.ManualOverrideTimeRemaining, 0f, "Should have time remaining for override");
        }
        
        [Test]
        public void SetManualLODLevel_ExceedsMaxLevel_ClampsToMaxLevel()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            int excessiveLevel = _testConfig.MaxLODLevel + 5;
            
            // Act
            _autoLODController.SetManualLODLevel(excessiveLevel);
            
            // Assert
            Assert.AreEqual(_testConfig.MaxLODLevel, _autoLODController.CurrentLODLevel, 
                "Should clamp to maximum LOD level");
        }
        
        [Test]
        public void SetManualLODLevel_NegativeLevel_ClampsToZero()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            
            // Act
            _autoLODController.SetManualLODLevel(-1);
            
            // Assert
            Assert.AreEqual(0, _autoLODController.CurrentLODLevel, "Should clamp to minimum LOD level (0)");
        }
        
        #endregion
        
        #region LOD Status and Recommendations Tests
        
        [Test]
        public void GetCurrentStatus_ReturnsValidStatus()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            _autoLODController.UpdatePerformanceMetrics(70f, 14.3f, 65f, 70f);
            
            // Act
            var status = _autoLODController.GetCurrentStatus();
            
            // Assert
            Assert.AreEqual(_autoLODController.CurrentLODLevel, status.CurrentLODLevel, 
                "Status should reflect current LOD level");
            Assert.AreEqual(_autoLODController.IsAutoLODEnabled, status.IsActive, 
                "Status should reflect Auto-LOD enabled state");
            Assert.IsNotNull(status.CurrentSettings, "Should have current LOD settings");
            Assert.GreaterOrEqual(status.TimeSinceLastAdjustment, 0f, 
                "Time since last adjustment should be non-negative");
        }
        
        [Test]
        public void GetLODRecommendations_WithPoorPerformance_RecommendsHigherLOD()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            _autoLODController.UpdatePerformanceMetrics(55f, 18f, 80f, 85f); // Poor performance
            
            // Act
            var recommendations = _autoLODController.GetLODRecommendations(72f);
            
            // Assert
            Assert.Greater(recommendations.RecommendedLODLevel, _autoLODController.CurrentLODLevel, 
                "Should recommend higher LOD level for poor performance");
            Assert.Greater(recommendations.ExpectedFPSImprovement, 0f, 
                "Should expect FPS improvement");
            Assert.Greater(recommendations.Confidence, 0f, "Should have confidence in recommendation");
            Assert.IsNotEmpty(recommendations.RecommendedAdjustments, "Should have specific adjustments");
        }
        
        [Test]
        public void GetLODRecommendations_WithGoodPerformance_RecommendsLowerLOD()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            _autoLODController.SetManualLODLevel(2); // Start at higher LOD
            _autoLODController.UpdatePerformanceMetrics(85f, 11.8f, 50f, 55f); // Good performance
            
            // Act
            var recommendations = _autoLODController.GetLODRecommendations(72f);
            
            // Assert
            Assert.LessOrEqual(recommendations.RecommendedLODLevel, _autoLODController.CurrentLODLevel, 
                "Should recommend same or lower LOD level for good performance");
            Assert.IsNotNull(recommendations.RecommendationReasoning, "Should provide reasoning");
        }
        
        #endregion
        
        #region LOD Settings Tests
        
        [Test]
        public void ApplyLODSettings_ValidSettings_ReturnsTrue()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            var settings = LODSettings.MediumQuality;
            
            // Act
            bool result = _autoLODController.ApplyLODSettings(settings);
            
            // Assert
            Assert.IsTrue(result, "Should successfully apply LOD settings");
            
            var currentSettings = _autoLODController.GetCurrentLODSettings();
            Assert.AreEqual(settings.LODLevel, currentSettings.LODLevel, "Should apply LOD level");
            Assert.AreEqual(settings.BubbleCount, currentSettings.BubbleCount, "Should apply bubble count");
            Assert.AreEqual(settings.RenderingQuality, currentSettings.RenderingQuality, 0.01f, 
                "Should apply rendering quality");
        }
        
        [Test]
        public void GetCurrentLODSettings_ReturnsCurrentSettings()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            var expectedSettings = LODSettings.PerformanceFocused;
            _autoLODController.ApplyLODSettings(expectedSettings);
            
            // Act
            var currentSettings = _autoLODController.GetCurrentLODSettings();
            
            // Assert
            Assert.AreEqual(expectedSettings.LODLevel, currentSettings.LODLevel, "Should return current LOD level");
            Assert.AreEqual(expectedSettings.BubbleCount, currentSettings.BubbleCount, "Should return current bubble count");
            Assert.AreEqual(expectedSettings.RenderingQuality, currentSettings.RenderingQuality, 0.01f, 
                "Should return current rendering quality");
        }
        
        [Test]
        public void ResetToHighestQuality_ResetsLODToZero()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            _autoLODController.SetManualLODLevel(3); // Set to lower quality
            bool eventFired = false;
            
            _autoLODController.LODLevelChanged += (args) => { eventFired = true; };
            
            // Act
            _autoLODController.ResetToHighestQuality();
            
            // Assert
            Assert.AreEqual(0, _autoLODController.CurrentLODLevel, "Should reset to LOD level 0");
            Assert.IsTrue(eventFired, "Should fire LOD level changed event");
            
            var settings = _autoLODController.GetCurrentLODSettings();
            Assert.AreEqual(LODSettings.HighestQuality.BubbleCount, settings.BubbleCount, 
                "Should reset to highest quality settings");
        }
        
        #endregion
        
        #region Performance History Tests
        
        [Test]
        public void GetPerformanceHistory_WithNoData_ReturnsEmptyHistory()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            
            // Act
            var history = _autoLODController.GetPerformanceHistory();
            
            // Assert
            Assert.AreEqual(0, history.RecentSamples.Length, "Should have no samples initially");
            Assert.AreEqual(0f, history.AverageFPS, "Average FPS should be 0 with no data");
            Assert.AreEqual(0, history.LODAdjustmentCount, "Should have no LOD adjustments initially");
        }
        
        [Test]
        public void GetPerformanceHistory_WithData_ReturnsValidHistory()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            
            // Add some performance data
            for (int i = 0; i < 10; i++)
            {
                _autoLODController.UpdatePerformanceMetrics(70f + i, 14f, 60f, 65f);
            }
            
            // Act
            var history = _autoLODController.GetPerformanceHistory();
            
            // Assert
            Assert.AreEqual(10, history.RecentSamples.Length, "Should have 10 samples");
            Assert.Greater(history.AverageFPS, 0f, "Should have positive average FPS");
            Assert.Greater(history.MaximumFPS, history.MinimumFPS, "Max FPS should be greater than min FPS");
            Assert.GreaterOrEqual(history.FPSStandardDeviation, 0f, "Standard deviation should be non-negative");
        }
        
        #endregion
        
        #region Bubble Count Adjustment Tests
        
        [Test]
        public void ConfigureBubbleCountAdjustment_ValidConfig_UpdatesConfiguration()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            var bubbleConfig = new BubbleCountAdjustmentConfig
            {
                MaxBubbleCount = 80,
                MinBubbleCount = 20,
                BubblesPerLODLevel = 10,
                PrioritizeCentralBubbles = true,
                CountChangeSmoothingFactor = 0.5f
            };
            
            // Act
            _autoLODController.ConfigureBubbleCountAdjustment(bubbleConfig);
            
            // Assert
            var status = _autoLODController.GetBubbleCountStatus();
            Assert.LessOrEqual(status.CurrentBubbleCount, bubbleConfig.MaxBubbleCount, 
                "Current bubble count should be within new limits");
            Assert.GreaterOrEqual(status.CurrentBubbleCount, bubbleConfig.MinBubbleCount, 
                "Current bubble count should be within new limits");
        }
        
        [Test]
        public void GetBubbleCountStatus_ReturnsValidStatus()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            var bubbleConfig = BubbleCountAdjustmentConfig.Default;
            _autoLODController.ConfigureBubbleCountAdjustment(bubbleConfig);
            
            // Act
            var status = _autoLODController.GetBubbleCountStatus();
            
            // Assert
            Assert.GreaterOrEqual(status.CurrentBubbleCount, bubbleConfig.MinBubbleCount, 
                "Current count should be within limits");
            Assert.LessOrEqual(status.CurrentBubbleCount, bubbleConfig.MaxBubbleCount, 
                "Current count should be within limits");
            Assert.GreaterOrEqual(status.TimeSinceLastChange, 0f, 
                "Time since last change should be non-negative");
            Assert.AreEqual(bubbleConfig.MaxBubbleCount, status.MaxBubbleCount, 
                "Should report correct max bubble count");
        }
        
        #endregion
        
        #region Event Tests
        
        [Test]
        public void LODLevelChanged_Event_FiresOnLevelChange()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            bool eventFired = false;
            AutoLODLevelChangedEventArgs eventArgs = default;
            
            _autoLODController.LODLevelChanged += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            _autoLODController.SetManualLODLevel(2);
            
            // Assert
            Assert.IsTrue(eventFired, "LOD level changed event should fire");
            Assert.AreEqual(0, eventArgs.PreviousLevel, "Should report correct previous level");
            Assert.AreEqual(2, eventArgs.NewLevel, "Should report correct new level");
            Assert.IsNotNull(eventArgs.Reason, "Should provide reason for change");
            Assert.IsNotNull(eventArgs.NewSettings, "Should provide new LOD settings");
        }
        
        [Test]
        public void PerformanceThresholdCrossed_Event_FiresOnThresholdCross()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            bool eventFired = false;
            AutoLODPerformanceEventArgs eventArgs = default;
            
            _autoLODController.PerformanceThresholdCrossed += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act - Simulate performance below minimum threshold
            _autoLODController.UpdatePerformanceMetrics(45f, 22f, 95f, 95f);
            
            // Assert
            Assert.IsTrue(eventFired, "Performance threshold crossed event should fire");
            Assert.AreEqual(PerformanceThresholdType.MinimumFPS, eventArgs.ThresholdType, 
                "Should identify correct threshold type");
            Assert.IsFalse(eventArgs.CrossedUpward, "Should indicate downward threshold crossing");
        }
        
        [Test]
        public void BubbleCountAdjusted_Event_FiresOnCountChange()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            bool eventFired = false;
            BubbleCountAdjustedEventArgs eventArgs = default;
            
            _autoLODController.BubbleCountAdjusted += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act - Apply LOD settings with different bubble count
            var settings = LODSettings.MediumQuality;
            _autoLODController.ApplyLODSettings(settings);
            
            // Assert
            Assert.IsTrue(eventFired, "Bubble count adjusted event should fire");
            Assert.AreEqual(settings.BubbleCount, eventArgs.NewCount, "Should report correct new count");
            Assert.IsNotNull(eventArgs.Reason, "Should provide reason for adjustment");
        }
        
        #endregion
        
        #region Integration Tests
        
        [UnityTest]
        public IEnumerator FullAutoLODAdjustment_SimulatePerformanceDrop()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            int initialLODLevel = _autoLODController.CurrentLODLevel;
            bool lodChanged = false;
            
            _autoLODController.LODLevelChanged += (args) => { lodChanged = true; };
            
            // Act - Simulate consistent poor performance
            for (int i = 0; i < _testConfig.FrameThresholdCount + 2; i++)
            {
                _autoLODController.UpdatePerformanceMetrics(55f, 18f, 85f, 90f);
                yield return new WaitForSeconds(0.1f);
            }
            
            // Wait for cooldown period
            yield return new WaitForSeconds(_testConfig.AdjustmentCooldownSeconds + 0.5f);
            
            // Trigger final update
            _autoLODController.UpdatePerformanceMetrics(55f, 18f, 85f, 90f);
            
            // Assert
            Assert.IsTrue(lodChanged, "LOD level should change due to poor performance");
            Assert.Greater(_autoLODController.CurrentLODLevel, initialLODLevel, 
                "LOD level should increase (lower quality)");
            
            var history = _autoLODController.GetPerformanceHistory();
            Assert.Greater(history.LODAdjustmentCount, 0, "Should record LOD adjustment in history");
        }
        
        [Test]
        public void Dispose_CleansUpResources()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            _autoLODController.UpdatePerformanceMetrics(70f, 14f, 60f, 65f);
            
            // Act
            _autoLODController.Dispose();
            
            // Assert
            Assert.IsFalse(_autoLODController.IsInitialized, "Should not be initialized after disposal");
            Assert.IsFalse(_autoLODController.IsAutoLODEnabled, "Should not be enabled after disposal");
            
            var history = _autoLODController.GetPerformanceHistory();
            Assert.AreEqual(0, history.RecentSamples.Length, "Performance history should be cleared");
        }
        
        #endregion
        
        #region LOD Settings Validation Tests
        
        [Test]
        public void LODSettings_HighestQuality_HasCorrectValues()
        {
            // Act
            var settings = LODSettings.HighestQuality;
            
            // Assert
            Assert.AreEqual(0, settings.LODLevel, "Highest quality should be LOD level 0");
            Assert.AreEqual(100, settings.BubbleCount, "Should have maximum bubble count");
            Assert.AreEqual(1.0f, settings.RenderingQuality, 0.01f, "Should have maximum rendering quality");
            Assert.AreEqual(1.0f, settings.EffectQuality, 0.01f, "Should have maximum effect quality");
            Assert.IsTrue(settings.EnableDynamicBatching, "Should enable dynamic batching");
            Assert.IsTrue(settings.EnableGPUInstancing, "Should enable GPU instancing");
        }
        
        [Test]
        public void LODSettings_MediumQuality_HasBalancedValues()
        {
            // Act
            var settings = LODSettings.MediumQuality;
            
            // Assert
            Assert.AreEqual(2, settings.LODLevel, "Medium quality should be LOD level 2");
            Assert.AreEqual(75, settings.BubbleCount, "Should have reduced bubble count");
            Assert.Less(settings.RenderingQuality, 1.0f, "Should have reduced rendering quality");
            Assert.Greater(settings.RenderingQuality, 0.5f, "Should still have decent rendering quality");
            Assert.IsTrue(settings.EnableDynamicBatching, "Should still enable dynamic batching");
        }
        
        [Test]
        public void LODSettings_PerformanceFocused_HasOptimizedValues()
        {
            // Act
            var settings = LODSettings.PerformanceFocused;
            
            // Assert
            Assert.AreEqual(4, settings.LODLevel, "Performance focused should be LOD level 4");
            Assert.AreEqual(50, settings.BubbleCount, "Should have significantly reduced bubble count");
            Assert.Less(settings.RenderingQuality, 0.8f, "Should have reduced rendering quality");
            Assert.Less(settings.ShadowQuality, 0.5f, "Should have reduced shadow quality");
            Assert.Less(settings.MaxDrawDistance, 1000f, "Should have reduced draw distance");
        }
        
        #endregion
        
        #region Performance Calculation Tests
        
        [Test]
        public void PerformanceScore_WithGoodMetrics_ReturnsHighScore()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            
            // Act
            _autoLODController.UpdatePerformanceMetrics(75f, 13.3f, 50f, 55f); // Good performance
            var status = _autoLODController.GetCurrentStatus();
            
            // Assert
            Assert.Greater(status.CurrentMetrics.PerformanceScore, 70f, 
                "Good performance should result in high performance score");
        }
        
        [Test]
        public void PerformanceScore_WithPoorMetrics_ReturnsLowScore()
        {
            // Arrange
            _autoLODController.Initialize(_testConfig);
            
            // Act
            _autoLODController.UpdatePerformanceMetrics(45f, 22f, 95f, 95f); // Poor performance
            var status = _autoLODController.GetCurrentStatus();
            
            // Assert
            Assert.Less(status.CurrentMetrics.PerformanceScore, 50f, 
                "Poor performance should result in low performance score");
        }
        
        #endregion
    }
}