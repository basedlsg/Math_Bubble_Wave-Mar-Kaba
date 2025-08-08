using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive integration tests for the Warning State Management system.
    /// Tests automatic warning updates, state tracking, and validation functionality.
    /// </summary>
    [TestFixture]
    public class WarningStateManagerTests
    {
        private WarningStateManager _stateManager;
        private MockReadmeWarningManager _mockWarningManager;
        private MockCompilerFlagManager _mockCompilerFlagManager;
        private MockDevStateGenerator _mockDevStateGenerator;
        private string _testDirectory;
        
        [SetUp]
        public void SetUp()
        {
            // Create test directory
            _testDirectory = Path.Combine(Path.GetTempPath(), "WarningStateTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            
            // Create mock dependencies
            _mockWarningManager = new MockReadmeWarningManager();
            _mockCompilerFlagManager = new MockCompilerFlagManager();
            _mockDevStateGenerator = new MockDevStateGenerator();
            
            // Initialize state manager with mocks
            _stateManager = new WarningStateManager(
                _mockWarningManager,
                _mockCompilerFlagManager,
                _mockDevStateGenerator);
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up
            _stateManager?.Dispose();
            
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        
        #region Monitoring Tests
        
        [Test]
        public void StartMonitoring_ShouldEnableMonitoring()
        {
            // Act
            _stateManager.StartMonitoring();
            
            // Assert
            Assert.IsTrue(_stateManager.IsMonitoring);
        }
        
        [Test]
        public void StopMonitoring_ShouldDisableMonitoring()
        {
            // Arrange
            _stateManager.StartMonitoring();
            
            // Act
            _stateManager.StopMonitoring();
            
            // Assert
            Assert.IsFalse(_stateManager.IsMonitoring);
        }
        
        [Test]
        public void StartMonitoring_WhenAlreadyMonitoring_ShouldLogWarning()
        {
            // Arrange
            _stateManager.StartMonitoring();
            
            // Act & Assert (should not throw)
            LogAssert.Expect(LogType.Warning, "[WarningStateManager] Monitoring is already active");
            _stateManager.StartMonitoring();
        }
        
        #endregion
        
        #region State Change Detection Tests
        
        [Test]
        public void ForceStateUpdate_WithNoChanges_ShouldReturnFalse()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            
            // Act
            var result = _stateManager.ForceStateUpdate();
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void ForceStateUpdate_WithFeatureStateChange_ShouldReturnTrue()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            _stateManager.ForceStateUpdate(); // Initialize state
            
            // Change feature state
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            
            // Act
            var result = _stateManager.ForceStateUpdate();
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [Test]
        public void ForceStateUpdate_ShouldUpdateWarningManager()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            _stateManager.ForceStateUpdate(); // Initialize state
            
            // Change feature state
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            
            // Act
            _stateManager.ForceStateUpdate();
            
            // Assert
            Assert.Greater(_mockWarningManager.GenerateWarningMessageCallCount, 1);
        }
        
        #endregion
        
        #region Validation Tests
        
        [Test]
        public void ValidateWarningAccuracy_ShouldReturnValidationResult()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            
            // Act
            var result = _stateManager.ValidateWarningAccuracy();
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ValidatedAt > DateTime.MinValue);
            Assert.IsNotNull(result.FeatureStates);
        }
        
        [Test]
        public void ValidateWarningAccuracy_WithConsistentState_ShouldReturnValid()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            _mockWarningManager.SetConsistentValidation(true);
            
            // Act
            var result = _stateManager.ValidateWarningAccuracy();
            
            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.ReadmeConsistent);
        }
        
        [Test]
        public void ValidateWarningAccuracy_WithInconsistentState_ShouldReturnInvalid()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            _mockWarningManager.SetConsistentValidation(false);
            
            // Act
            var result = _stateManager.ValidateWarningAccuracy();
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(result.ReadmeConsistent);
        }
        
        #endregion
        
        #region State Change History Tests
        
        [Test]
        public void GetStateChangeHistory_InitiallyEmpty_ShouldReturnEmptyList()
        {
            // Act
            var history = _stateManager.GetStateChangeHistory();
            
            // Assert
            Assert.IsNotNull(history);
            Assert.AreEqual(0, history.Count);
        }
        
        [Test]
        public void GetStateChangeHistory_AfterStateChange_ShouldContainRecord()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            _stateManager.ForceStateUpdate(); // Initialize
            
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            _stateManager.ForceStateUpdate(); // Trigger change
            
            // Act
            var history = _stateManager.GetStateChangeHistory();
            
            // Assert
            Assert.Greater(history.Count, 0);
            
            var lastChange = history.Last();
            Assert.IsNotNull(lastChange.ChangedFeatures);
            Assert.Greater(lastChange.ChangedFeatures.Count, 0);
            Assert.AreEqual(ExperimentalFeature.AI_INTEGRATION, lastChange.ChangedFeatures[0].Feature);
        }
        
        #endregion
        
        #region Callback Tests
        
        [Test]
        public void RegisterStateChangeCallback_ShouldAddCallback()
        {
            // Arrange
            var callbackInvoked = false;
            Action<WarningStateChangeEvent> callback = (e) => callbackInvoked = true;
            
            // Act
            _stateManager.RegisterStateChangeCallback(callback);
            
            // Trigger state change
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            _stateManager.ForceStateUpdate(); // Initialize
            
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            _stateManager.ForceStateUpdate(); // Trigger change
            
            // Assert
            Assert.IsTrue(callbackInvoked);
        }
        
        [Test]
        public void UnregisterStateChangeCallback_ShouldRemoveCallback()
        {
            // Arrange
            var callbackInvoked = false;
            Action<WarningStateChangeEvent> callback = (e) => callbackInvoked = true;
            
            _stateManager.RegisterStateChangeCallback(callback);
            _stateManager.UnregisterStateChangeCallback(callback);
            
            // Act
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            _stateManager.ForceStateUpdate(); // Initialize
            
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            _stateManager.ForceStateUpdate(); // Trigger change
            
            // Assert
            Assert.IsFalse(callbackInvoked);
        }
        
        #endregion
        
        #region Current State Tests
        
        [Test]
        public void GetCurrentState_ShouldReturnStateInfo()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            
            // Act
            var state = _stateManager.GetCurrentState();
            
            // Assert
            Assert.IsNotNull(state);
            Assert.IsNotNull(state.CurrentWarning);
            Assert.IsNotNull(state.FeatureStates);
            Assert.IsTrue(state.LastUpdated > DateTime.MinValue);
        }
        
        [Test]
        public void GetCurrentState_WithMonitoring_ShouldReflectMonitoringStatus()
        {
            // Arrange
            _stateManager.StartMonitoring();
            
            // Act
            var state = _stateManager.GetCurrentState();
            
            // Assert
            Assert.IsTrue(state.IsMonitoring);
        }
        
        #endregion
        
        #region Event Tests
        
        [Test]
        public void WarningStateChanged_OnStateChange_ShouldFireEvent()
        {
            // Arrange
            WarningStateChangeEvent receivedEvent = null;
            _stateManager.WarningStateChanged += (e) => receivedEvent = e;
            
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            _stateManager.ForceStateUpdate(); // Initialize
            
            // Act
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            _stateManager.ForceStateUpdate(); // Trigger change
            
            // Assert
            Assert.IsNotNull(receivedEvent);
            Assert.IsNotNull(receivedEvent.ChangeRecord);
            Assert.IsNotNull(receivedEvent.ValidationResult);
        }
        
        #endregion
        
        #region Async Tests
        
        [UnityTest]
        public System.Collections.IEnumerator StartMonitoring_ShouldStartPeriodicValidation()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            
            // Act
            _stateManager.StartMonitoring();
            
            // Wait for periodic validation to potentially run
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            Assert.IsTrue(_stateManager.IsMonitoring);
            // Note: Full periodic validation testing would require more sophisticated mocking
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void ForceStateUpdate_WithFailingDependency_ShouldHandleGracefully()
        {
            // Arrange
            var failingFlagManager = new FailingCompilerFlagManager();
            var stateManager = new WarningStateManager(
                _mockWarningManager,
                failingFlagManager,
                _mockDevStateGenerator);
            
            // Act & Assert (should not throw)
            var result = stateManager.ForceStateUpdate();
            Assert.IsFalse(result);
            
            stateManager.Dispose();
        }
        
        [Test]
        public void ValidateWarningAccuracy_WithFailingDependency_ShouldReturnInvalidResult()
        {
            // Arrange
            var failingFlagManager = new FailingCompilerFlagManager();
            var stateManager = new WarningStateManager(
                _mockWarningManager,
                failingFlagManager,
                _mockDevStateGenerator);
            
            // Act
            var result = stateManager.ValidateWarningAccuracy();
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.Greater(result.ValidationErrors.Count, 0);
            
            stateManager.Dispose();
        }
        
        #endregion
        
        #region Disposal Tests
        
        [Test]
        public void Dispose_ShouldStopMonitoring()
        {
            // Arrange
            _stateManager.StartMonitoring();
            
            // Act
            _stateManager.Dispose();
            
            // Assert
            Assert.IsFalse(_stateManager.IsMonitoring);
        }
        
        [Test]
        public void Dispose_CalledMultipleTimes_ShouldNotThrow()
        {
            // Act & Assert (should not throw)
            _stateManager.Dispose();
            _stateManager.Dispose();
        }
        
        #endregion
        
        #region Mock Classes
        
        private class MockReadmeWarningManager : IReadmeWarningManager
        {
            private readonly List<WarningUpdateRecord> _updateHistory = new List<WarningUpdateRecord>();
            private readonly List<Action<string>> _callbacks = new List<Action<string>>();
            private bool _consistentValidation = true;
            
            public int GenerateWarningMessageCallCount { get; private set; }
            
            public void SetConsistentValidation(bool consistent)
            {
                _consistentValidation = consistent;
            }
            
            public string GenerateWarningMessage()
            {
                GenerateWarningMessageCallCount++;
                return "⚠️ Test warning message";
            }
            
            public bool UpdateReadmeWarning(string readmePath)
            {
                var record = new WarningUpdateRecord
                {
                    UpdatedAt = DateTime.UtcNow,
                    ReadmePath = readmePath,
                    UpdateSuccessful = true,
                    NewWarning = GenerateWarningMessage()
                };
                _updateHistory.Add(record);
                
                // Notify callbacks
                foreach (var callback in _callbacks)
                {
                    callback(record.NewWarning);
                }
                
                return true;
            }
            
            public Task<bool> UpdateReadmeWarningAsync(string readmePath)
            {
                return Task.FromResult(UpdateReadmeWarning(readmePath));
            }
            
            public ReadmeValidationResult ValidateConsistency(string readmePath, string devStatePath)
            {
                return new ReadmeValidationResult
                {
                    IsConsistent = _consistentValidation,
                    ValidatedAt = DateTime.UtcNow,
                    CurrentReadmeWarning = GenerateWarningMessage(),
                    ExpectedWarning = GenerateWarningMessage()
                };
            }
            
            public WarningTemplate GetCurrentWarningTemplate()
            {
                return new WarningTemplate
                {
                    Id = "test_template",
                    Name = "Test Template",
                    Template = "⚠️ Test warning message",
                    Priority = 100
                };
            }
            
            public void RegisterWarningUpdateCallback(Action<string> callback)
            {
                if (callback != null && !_callbacks.Contains(callback))
                {
                    _callbacks.Add(callback);
                }
            }
            
            public void UnregisterWarningUpdateCallback(Action<string> callback)
            {
                if (callback != null)
                {
                    _callbacks.Remove(callback);
                }
            }
            
            public void RefreshWarningState()
            {
                // Mock implementation
            }
            
            public List<WarningUpdateRecord> GetWarningUpdateHistory()
            {
                return new List<WarningUpdateRecord>(_updateHistory);
            }
        }
        
        private class MockCompilerFlagManager : ICompilerFlagManager
        {
            private readonly Dictionary<ExperimentalFeature, bool> _featureStates = new Dictionary<ExperimentalFeature, bool>();
            
            public void SetAllFeaturesDisabled()
            {
                _featureStates[ExperimentalFeature.AI_INTEGRATION] = false;
                _featureStates[ExperimentalFeature.VOICE_PROCESSING] = false;
                _featureStates[ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS] = false;
                _featureStates[ExperimentalFeature.CLOUD_INFERENCE] = false;
                _featureStates[ExperimentalFeature.ON_DEVICE_ML] = false;
            }
            
            public void SetFeatureState(ExperimentalFeature feature, bool enabled)
            {
                _featureStates[feature] = enabled;
            }
            
            public bool IsFeatureEnabled(ExperimentalFeature feature)
            {
                return _featureStates.GetValueOrDefault(feature, false);
            }
            
            public Dictionary<ExperimentalFeature, bool> GetAllFeatureStates()
            {
                return new Dictionary<ExperimentalFeature, bool>(_featureStates);
            }
            
            public void SetFeatureEnabled(ExperimentalFeature feature, bool enabled)
            {
                _featureStates[feature] = enabled;
            }
            
            public void RefreshFeatureStates()
            {
                // Mock implementation
            }
            
            public List<string> GetActiveDefineSymbols()
            {
                return new List<string>();
            }
            
            public bool ValidateFeatureDependencies(ExperimentalFeature feature)
            {
                return true;
            }
        }
        
        private class MockDevStateGenerator : IDevStateGenerator
        {
            public DevStateReport GenerateReport()
            {
                return new DevStateReport
                {
                    GeneratedAt = DateTime.UtcNow,
                    BuildVersion = "1.0.0-test"
                };
            }
            
            public Task<DevStateReport> GenerateReportAsync()
            {
                return Task.FromResult(GenerateReport());
            }
            
            public void ScheduleNightlyGeneration()
            {
                // Mock implementation
            }
            
            public bool ValidateReportAccuracy(DevStateReport report)
            {
                return true;
            }
            
            public string GenerateAndSaveDevStateFile()
            {
                return Path.Combine(Path.GetTempPath(), "DEV_STATE.md");
            }
        }
        
        private class FailingCompilerFlagManager : ICompilerFlagManager
        {
            public bool IsFeatureEnabled(ExperimentalFeature feature)
            {
                throw new InvalidOperationException("Mock failure");
            }
            
            public Dictionary<ExperimentalFeature, bool> GetAllFeatureStates()
            {
                throw new InvalidOperationException("Mock failure");
            }
            
            public void SetFeatureEnabled(ExperimentalFeature feature, bool enabled)
            {
                throw new InvalidOperationException("Mock failure");
            }
            
            public void RefreshFeatureStates()
            {
                throw new InvalidOperationException("Mock failure");
            }
            
            public List<string> GetActiveDefineSymbols()
            {
                throw new InvalidOperationException("Mock failure");
            }
            
            public bool ValidateFeatureDependencies(ExperimentalFeature feature)
            {
                throw new InvalidOperationException("Mock failure");
            }
        }
        
        #endregion
    }
}