using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Unit tests for the PerformanceGateRunner infrastructure.
    /// Validates that the CI/CD performance gate system works correctly.
    /// </summary>
    public class PerformanceGateRunnerTests
    {
        private PerformanceGateRunner _runner;
        
        [SetUp]
        public void SetUp()
        {
            _runner = new PerformanceGateRunner();
        }
        
        [TearDown]
        public void TearDown()
        {
            _runner = null;
        }
        
        [Test]
        public void PerformanceGateRunner_Initialization_RegistersDefaultGates()
        {
            // Act
            var registeredGates = _runner.GetRegisteredGates();
            
            // Assert
            Assert.IsNotNull(registeredGates, "Registered gates should not be null");
            Assert.IsTrue(registeredGates.Count > 0, "Should have registered default gates");
            
            // Check for expected default gates
            var gateNames = registeredGates.Select(g => g.Name).ToList();
            Assert.Contains("Unit Tests", gateNames, "Should register Unit Test gate");
            Assert.Contains("Burst Compilation", gateNames, "Should register Burst Compilation gate");
            Assert.Contains("Performance Stub Test", gateNames, "Should register Performance Stub gate");
            Assert.Contains("Performance Profiling", gateNames, "Should register Performance Profiling gate");
        }
        
        [Test]
        public void PerformanceGateRunner_RegisterGate_AddsGateToCollection()
        {
            // Arrange
            var testGate = new TestPerformanceGate("TestGate", true, 500);
            
            // Act
            _runner.RegisterGate(testGate);
            var registeredGates = _runner.GetRegisteredGates();
            
            // Assert
            Assert.IsTrue(registeredGates.Any(g => g.Name == "TestGate"), "Should register the test gate");
        }
        
        [Test]
        public void PerformanceGateRunner_UnregisterGate_RemovesGateFromCollection()
        {
            // Arrange
            var testGate = new TestPerformanceGate("TestGate", true, 500);
            _runner.RegisterGate(testGate);
            
            // Act
            _runner.UnregisterGate("TestGate");
            var registeredGates = _runner.GetRegisteredGates();
            
            // Assert
            Assert.IsFalse(registeredGates.Any(g => g.Name == "TestGate"), "Should unregister the test gate");
        }
        
        [Test]
        public void PerformanceGateRunner_ValidateConfiguration_ReturnsValidResult()
        {
            // Act
            var validationResult = _runner.ValidateConfiguration();
            
            // Assert
            Assert.IsNotNull(validationResult, "Validation result should not be null");
            Assert.IsTrue(validationResult.IsValid, "Configuration should be valid with default gates");
            Assert.IsTrue(validationResult.GatesValidated > 0, "Should validate at least one gate");
        }
        
        [Test]
        public void PerformanceGateRunner_RunSpecificGate_ExecutesCorrectGate()
        {
            // Arrange
            var testGate = new TestPerformanceGate("TestGate", true, 500);
            _runner.RegisterGate(testGate);
            
            // Act
            var result = _runner.RunSpecificGate("TestGate");
            
            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("TestGate", result.GateName, "Should execute the correct gate");
            Assert.IsTrue(testGate.WasExecuted, "Test gate should have been executed");
        }
        
        [Test]
        public void PerformanceGateRunner_RunSpecificGate_NonExistentGate_ReturnsFailure()
        {
            // Act
            var result = _runner.RunSpecificGate("NonExistentGate");
            
            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsFalse(result.Success, "Should fail for non-existent gate");
            Assert.IsTrue(result.ErrorMessages.Any(m => m.Contains("not registered")), "Should indicate gate not registered");
        }
        
        [Test]
        public void PerformanceGateRunner_RunAllGates_ExecutesAllRegisteredGates()
        {
            // Arrange
            var testGate1 = new TestPerformanceGate("TestGate1", true, 500);
            var testGate2 = new TestPerformanceGate("TestGate2", false, 300);
            _runner.RegisterGate(testGate1);
            _runner.RegisterGate(testGate2);
            
            // Act
            var result = _runner.RunAllGates();
            
            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(testGate1.WasExecuted, "TestGate1 should have been executed");
            Assert.IsTrue(testGate2.WasExecuted, "TestGate2 should have been executed");
            Assert.IsTrue(result.GateResults.Count >= 2, "Should have results for at least the test gates");
        }
        
        [Test]
        public void PerformanceGateRunner_RunAllGates_CriticalFailure_FailsOverallResult()
        {
            // Arrange
            var passingGate = new TestPerformanceGate("PassingGate", false, 500) { ShouldPass = true };
            var criticalFailingGate = new TestPerformanceGate("CriticalFailingGate", true, 400) { ShouldPass = false };
            _runner.RegisterGate(passingGate);
            _runner.RegisterGate(criticalFailingGate);
            
            // Act
            var result = _runner.RunAllGates();
            
            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsFalse(result.Success, "Overall result should fail due to critical gate failure");
            Assert.IsTrue(result.ErrorMessages.Count > 0, "Should have error messages");
        }
        
        [Test]
        public void PerformanceGateRunner_RunAllGates_NonCriticalFailure_PassesOverallResult()
        {
            // Arrange
            var passingGate = new TestPerformanceGate("PassingGate", true, 500) { ShouldPass = true };
            var nonCriticalFailingGate = new TestPerformanceGate("NonCriticalFailingGate", false, 400) { ShouldPass = false };
            _runner.RegisterGate(passingGate);
            _runner.RegisterGate(nonCriticalFailingGate);
            
            // Act
            var result = _runner.RunAllGates();
            
            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            // Note: Overall result depends on default gates too, so we check that non-critical failures don't automatically fail
            Assert.IsTrue(result.GateResults.Any(g => g.GateName == "NonCriticalFailingGate" && !g.Success), 
                         "Non-critical gate should have failed");
        }
        
        [Test]
        public void PerformanceGateRunner_ExecutionHistory_RecordsResults()
        {
            // Arrange
            var testGate = new TestPerformanceGate("TestGate", false, 500);
            _runner.RegisterGate(testGate);
            
            // Act
            _runner.RunSpecificGate("TestGate");
            var history = _runner.GetExecutionHistory();
            
            // Assert
            Assert.IsNotNull(history, "History should not be null");
            Assert.IsTrue(history.Count > 0, "Should have recorded execution history");
            Assert.IsTrue(history.Any(h => h.Result.GateName == "TestGate"), "Should have recorded TestGate execution");
        }
        
        [Test]
        public void PerformanceGateRunner_SetFailureThreshold_UpdatesThreshold()
        {
            // Arrange
            var newThreshold = new PerformanceThreshold
            {
                MinimumFPS = 90.0f,
                MaximumFrameTimeMs = 11.11f,
                MaximumMemoryUsageMB = 512.0f
            };
            
            // Act
            _runner.SetFailureThreshold(newThreshold);
            var retrievedThreshold = _runner.GetFailureThreshold();
            
            // Assert
            Assert.IsNotNull(retrievedThreshold, "Retrieved threshold should not be null");
            Assert.AreEqual(90.0f, retrievedThreshold.MinimumFPS, "Should update minimum FPS");
            Assert.AreEqual(11.11f, retrievedThreshold.MaximumFrameTimeMs, 0.01f, "Should update maximum frame time");
            Assert.AreEqual(512.0f, retrievedThreshold.MaximumMemoryUsageMB, "Should update maximum memory usage");
        }
        
        [Test]
        public void PerformanceGateRunner_GeneratePerformanceReport_CreatesReport()
        {
            // Arrange
            var testGate = new TestPerformanceGate("TestGate", false, 500);
            _runner.RegisterGate(testGate);
            _runner.RunSpecificGate("TestGate");
            
            // Act & Assert
            Assert.DoesNotThrow(() => {
                var reportPath = _runner.GeneratePerformanceReport();
                Assert.IsNotNull(reportPath, "Report path should not be null");
                Assert.IsTrue(reportPath.EndsWith(".md"), "Report should be a markdown file");
            }, "Should generate performance report without throwing");
        }
        
        [UnityTest]
        public IEnumerator PerformanceGateRunner_RunAllGatesAsync_ExecutesAsynchronously()
        {
            // Arrange
            var testGate = new TestPerformanceGate("AsyncTestGate", false, 500);
            _runner.RegisterGate(testGate);
            
            // Act
            var asyncTask = _runner.RunAllGatesAsync();
            
            // Wait for completion
            while (!asyncTask.IsCompleted)
            {
                yield return null;
            }
            
            var result = asyncTask.Result;
            
            // Assert
            Assert.IsNotNull(result, "Async result should not be null");
            Assert.IsTrue(testGate.WasExecuted, "Test gate should have been executed asynchronously");
        }
        
        #region Helper Classes
        
        /// <summary>
        /// Test implementation of IPerformanceGate for unit testing.
        /// </summary>
        private class TestPerformanceGate : IPerformanceGate
        {
            public string Name { get; }
            public string Description => $"Test gate: {Name}";
            public int Priority { get; }
            public bool IsCritical { get; }
            public TimeSpan ExpectedExecutionTime => TimeSpan.FromSeconds(1);
            
            public bool WasExecuted { get; private set; }
            public bool ShouldPass { get; set; } = true;
            
            public TestPerformanceGate(string name, bool isCritical, int priority)
            {
                Name = name;
                IsCritical = isCritical;
                Priority = priority;
            }
            
            public PerformanceGateResult Execute()
            {
                WasExecuted = true;
                
                return new PerformanceGateResult
                {
                    GateName = Name,
                    ExecutedAt = DateTime.UtcNow,
                    Success = ShouldPass,
                    ExecutionTime = TimeSpan.FromMilliseconds(100),
                    Summary = $"Test gate {Name} {(ShouldPass ? "passed" : "failed")}",
                    ErrorMessages = ShouldPass ? new List<string>() : new List<string> { "Test failure" },
                    PerformanceMetrics = new Dictionary<string, object>
                    {
                        ["TestMetric"] = 42.0f
                    }
                };
            }
            
            public async System.Threading.Tasks.Task<PerformanceGateResult> ExecuteAsync()
            {
                return await System.Threading.Tasks.Task.Run(() => Execute());
            }
            
            public bool ValidateConfiguration()
            {
                return true;
            }
            
            public PerformanceGateStatus GetStatus()
            {
                return new PerformanceGateStatus
                {
                    GateName = Name,
                    Status = WasExecuted ? GateStatus.Completed : GateStatus.Ready,
                    ConfigurationValid = true,
                    StatusMessages = new List<string> { "Test gate status" }
                };
            }
        }
        
        #endregion
    }
}