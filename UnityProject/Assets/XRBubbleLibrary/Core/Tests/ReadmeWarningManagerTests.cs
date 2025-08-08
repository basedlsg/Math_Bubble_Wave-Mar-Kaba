using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the README Warning System.
    /// Tests warning generation, README updates, consistency validation, and template management.
    /// </summary>
    [TestFixture]
    public class ReadmeWarningManagerTests
    {
        private ReadmeWarningManager _warningManager;
        private MockCompilerFlagManager _mockCompilerFlagManager;
        private string _testReadmePath;
        private string _testDevStatePath;
        
        [SetUp]
        public void SetUp()
        {
            // Create mock compiler flag manager
            _mockCompilerFlagManager = new MockCompilerFlagManager();
            
            // Initialize warning manager with mock
            _warningManager = new ReadmeWarningManager(_mockCompilerFlagManager);
            
            // Create temporary test files
            var tempDir = Path.Combine(Path.GetTempPath(), "ReadmeWarningTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            
            _testReadmePath = Path.Combine(tempDir, "README.md");
            _testDevStatePath = Path.Combine(tempDir, "DEV_STATE.md");
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test files
            var tempDir = Path.GetDirectoryName(_testReadmePath);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
        
        #region Warning Generation Tests
        
        [Test]
        public void GenerateWarningMessage_WithAllFeaturesDisabled_ShouldReturnDefaultWarning()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            
            // Act
            var warning = _warningManager.GenerateWarningMessage();
            
            // Assert
            Assert.IsNotNull(warning);
            Assert.IsTrue(warning.Contains("⚠️"));
            Assert.IsTrue(warning.Contains("AI & voice features are disabled"));
            Assert.IsTrue(warning.Contains("Quest 3"));
        }
        
        [Test]
        public void GenerateWarningMessage_WithAIEnabledVoiceDisabled_ShouldReturnAppropriateWarning()
        {
            // Arrange
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.VOICE_PROCESSING, false);
            
            // Act
            var warning = _warningManager.GenerateWarningMessage();
            
            // Assert
            Assert.IsNotNull(warning);
            Assert.IsTrue(warning.Contains("⚠️"));
            Assert.IsTrue(warning.Contains("Voice features are disabled"));
            Assert.IsTrue(warning.Contains("AI features are experimental"));
        }
        
        [Test]
        public void GenerateWarningMessage_WithVoiceEnabledAIDisabled_ShouldReturnAppropriateWarning()
        {
            // Arrange
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.VOICE_PROCESSING, true);
            
            // Act
            var warning = _warningManager.GenerateWarningMessage();
            
            // Assert
            Assert.IsNotNull(warning);
            Assert.IsTrue(warning.Contains("⚠️"));
            Assert.IsTrue(warning.Contains("AI features are disabled"));
            Assert.IsTrue(warning.Contains("Voice features are experimental"));
        }
        
        [Test]
        public void GenerateWarningMessage_WithBothFeaturesEnabled_ShouldReturnExperimentalWarning()
        {
            // Arrange
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.VOICE_PROCESSING, true);
            
            // Act
            var warning = _warningManager.GenerateWarningMessage();
            
            // Assert
            Assert.IsNotNull(warning);
            Assert.IsTrue(warning.Contains("⚠️"));
            Assert.IsTrue(warning.Contains("AI & voice features are experimental"));
        }
        
        #endregion
        
        #region README Update Tests
        
        [Test]
        public void UpdateReadmeWarning_WithNonExistentFile_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.md");
            
            // Act
            var result = _warningManager.UpdateReadmeWarning(nonExistentPath);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void UpdateReadmeWarning_WithEmptyFile_ShouldAddWarning()
        {
            // Arrange
            File.WriteAllText(_testReadmePath, "");
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            
            // Act
            var result = _warningManager.UpdateReadmeWarning(_testReadmePath);
            
            // Assert
            Assert.IsTrue(result);
            var content = File.ReadAllText(_testReadmePath);
            Assert.IsTrue(content.StartsWith("⚠️"));
        }
        
        [Test]
        public void UpdateReadmeWarning_WithExistingContent_ShouldPrependWarning()
        {
            // Arrange
            var originalContent = "# My Project\n\nThis is a test project.";
            File.WriteAllText(_testReadmePath, originalContent);
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            
            // Act
            var result = _warningManager.UpdateReadmeWarning(_testReadmePath);
            
            // Assert
            Assert.IsTrue(result);
            var content = File.ReadAllText(_testReadmePath);
            Assert.IsTrue(content.StartsWith("⚠️"));
            Assert.IsTrue(content.Contains("# My Project"));
        }
        
        [Test]
        public void UpdateReadmeWarning_WithExistingWarning_ShouldReplaceWarning()
        {
            // Arrange
            var contentWithOldWarning = "⚠️ Old warning message\n\n# My Project\n\nContent here.";
            File.WriteAllText(_testReadmePath, contentWithOldWarning);
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            
            // Act
            var result = _warningManager.UpdateReadmeWarning(_testReadmePath);
            
            // Assert
            Assert.IsTrue(result);
            var content = File.ReadAllText(_testReadmePath);
            Assert.IsTrue(content.StartsWith("⚠️"));
            Assert.IsFalse(content.Contains("Old warning message"));
            Assert.IsTrue(content.Contains("# My Project"));
        }
        
        [Test]
        public void UpdateReadmeWarning_WithSameWarning_ShouldSkipUpdate()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            var expectedWarning = _warningManager.GenerateWarningMessage();
            var contentWithSameWarning = $"{expectedWarning}\n\n# My Project";
            File.WriteAllText(_testReadmePath, contentWithSameWarning);
            var originalWriteTime = File.GetLastWriteTime(_testReadmePath);
            
            // Wait a moment to ensure write time would change if file was modified
            System.Threading.Thread.Sleep(100);
            
            // Act
            var result = _warningManager.UpdateReadmeWarning(_testReadmePath);
            
            // Assert
            Assert.IsTrue(result);
            var newWriteTime = File.GetLastWriteTime(_testReadmePath);
            Assert.AreEqual(originalWriteTime, newWriteTime); // File should not have been modified
        }
        
        #endregion
        
        #region Consistency Validation Tests
        
        [Test]
        public void ValidateConsistency_WithMatchingWarnings_ShouldReturnConsistent()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            var expectedWarning = _warningManager.GenerateWarningMessage();
            File.WriteAllText(_testReadmePath, $"{expectedWarning}\n\n# Project");
            
            // Act
            var result = _warningManager.ValidateConsistency(_testReadmePath, _testDevStatePath);
            
            // Assert
            Assert.IsTrue(result.IsConsistent);
            Assert.AreEqual(0, result.Inconsistencies.Count);
            Assert.AreEqual(expectedWarning, result.CurrentReadmeWarning);
            Assert.AreEqual(expectedWarning, result.ExpectedWarning);
        }
        
        [Test]
        public void ValidateConsistency_WithMismatchedWarnings_ShouldReturnInconsistent()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            File.WriteAllText(_testReadmePath, "⚠️ Wrong warning message\n\n# Project");
            
            // Act
            var result = _warningManager.ValidateConsistency(_testReadmePath, _testDevStatePath);
            
            // Assert
            Assert.IsFalse(result.IsConsistent);
            Assert.Greater(result.Inconsistencies.Count, 0);
            Assert.AreNotEqual(result.CurrentReadmeWarning, result.ExpectedWarning);
        }
        
        [Test]
        public void ValidateConsistency_WithNonExistentReadme_ShouldReturnInconsistent()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.md");
            
            // Act
            var result = _warningManager.ValidateConsistency(nonExistentPath, _testDevStatePath);
            
            // Assert
            Assert.IsFalse(result.IsConsistent);
            Assert.IsTrue(result.Inconsistencies.Any(i => i.Contains("README file not found")));
        }
        
        #endregion
        
        #region Template Management Tests
        
        [Test]
        public void GetCurrentWarningTemplate_WithAllFeaturesDisabled_ShouldReturnCorrectTemplate()
        {
            // Arrange
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            
            // Act
            var template = _warningManager.GetCurrentWarningTemplate();
            
            // Assert
            Assert.IsNotNull(template);
            Assert.AreEqual("all_experimental_disabled", template.Id);
            Assert.IsTrue(template.IsCritical);
        }
        
        [Test]
        public void GetCurrentWarningTemplate_WithMixedFeatureStates_ShouldReturnAppropriateTemplate()
        {
            // Arrange
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, true);
            _mockCompilerFlagManager.SetFeatureState(ExperimentalFeature.VOICE_PROCESSING, false);
            
            // Act
            var template = _warningManager.GetCurrentWarningTemplate();
            
            // Assert
            Assert.IsNotNull(template);
            Assert.AreEqual("ai_enabled_voice_disabled", template.Id);
        }
        
        #endregion
        
        #region Callback Tests
        
        [Test]
        public void RegisterWarningUpdateCallback_ShouldAddCallback()
        {
            // Arrange
            var callbackInvoked = false;
            Action<string> callback = (warning) => callbackInvoked = true;
            
            // Act
            _warningManager.RegisterWarningUpdateCallback(callback);
            _warningManager.UpdateReadmeWarning(_testReadmePath);
            
            // Assert
            Assert.IsTrue(callbackInvoked);
        }
        
        [Test]
        public void UnregisterWarningUpdateCallback_ShouldRemoveCallback()
        {
            // Arrange
            var callbackInvoked = false;
            Action<string> callback = (warning) => callbackInvoked = true;
            
            _warningManager.RegisterWarningUpdateCallback(callback);
            _warningManager.UnregisterWarningUpdateCallback(callback);
            
            // Act
            File.WriteAllText(_testReadmePath, "# Test");
            _warningManager.UpdateReadmeWarning(_testReadmePath);
            
            // Assert
            Assert.IsFalse(callbackInvoked);
        }
        
        #endregion
        
        #region Update History Tests
        
        [Test]
        public void GetWarningUpdateHistory_ShouldReturnUpdateRecords()
        {
            // Arrange
            File.WriteAllText(_testReadmePath, "# Test Project");
            
            // Act
            _warningManager.UpdateReadmeWarning(_testReadmePath);
            var history = _warningManager.GetWarningUpdateHistory();
            
            // Assert
            Assert.IsNotNull(history);
            Assert.Greater(history.Count, 0);
            
            var lastUpdate = history.Last();
            Assert.IsTrue(lastUpdate.UpdateSuccessful);
            Assert.IsNotNull(lastUpdate.NewWarning);
            Assert.AreEqual(_testReadmePath, lastUpdate.ReadmePath);
        }
        
        #endregion
        
        #region Async Tests
        
        [UnityTest]
        public System.Collections.IEnumerator UpdateReadmeWarningAsync_ShouldUpdateAsynchronously()
        {
            // Arrange
            File.WriteAllText(_testReadmePath, "# Test Project");
            _mockCompilerFlagManager.SetAllFeaturesDisabled();
            
            bool updateCompleted = false;
            bool updateResult = false;
            
            // Act
            var task = _warningManager.UpdateReadmeWarningAsync(_testReadmePath);
            task.ContinueWith(t =>
            {
                updateResult = t.Result;
                updateCompleted = true;
            });
            
            // Wait for completion
            while (!updateCompleted)
            {
                yield return null;
            }
            
            // Assert
            Assert.IsTrue(updateResult);
            var content = File.ReadAllText(_testReadmePath);
            Assert.IsTrue(content.StartsWith("⚠️"));
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void GenerateWarningMessage_WithNullCompilerFlagManager_ShouldHandleGracefully()
        {
            // Arrange
            var warningManager = new ReadmeWarningManager(new FailingCompilerFlagManager());
            
            // Act
            var warning = warningManager.GenerateWarningMessage();
            
            // Assert
            Assert.IsNotNull(warning);
            Assert.IsTrue(warning.Contains("Unable to determine current feature status"));
        }
        
        [Test]
        public void UpdateReadmeWarning_WithReadOnlyFile_ShouldReturnFalse()
        {
            // Arrange
            File.WriteAllText(_testReadmePath, "# Test");
            File.SetAttributes(_testReadmePath, FileAttributes.ReadOnly);
            
            try
            {
                // Act
                var result = _warningManager.UpdateReadmeWarning(_testReadmePath);
                
                // Assert
                Assert.IsFalse(result);
            }
            finally
            {
                // Cleanup
                File.SetAttributes(_testReadmePath, FileAttributes.Normal);
            }
        }
        
        #endregion
        
        #region Helper Classes
        
        private class MockCompilerFlagManager : ICompilerFlagManager
        {
            private readonly Dictionary<ExperimentalFeature, bool> _featureStates;
            
            public MockCompilerFlagManager()
            {
                _featureStates = new Dictionary<ExperimentalFeature, bool>();
                SetAllFeaturesDisabled();
            }
            
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
                // Mock implementation - no action needed
            }
            
            public List<string> GetActiveDefineSymbols()
            {
                var symbols = new List<string>();
                foreach (var feature in _featureStates.Where(f => f.Value))
                {
                    symbols.Add($"EXP_{feature.Key.ToString().ToUpper()}");
                }
                return symbols;
            }
            
            public bool ValidateFeatureDependencies(ExperimentalFeature feature)
            {
                return true; // Mock always returns true
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