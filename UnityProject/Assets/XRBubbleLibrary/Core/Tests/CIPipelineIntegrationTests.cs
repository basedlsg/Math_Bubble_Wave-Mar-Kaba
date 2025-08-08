using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Integration tests for CI/CD pipeline functionality.
    /// Validates that the performance gates system works correctly in CI environments.
    /// </summary>
    public class CIPipelineIntegrationTests
    {
        [Test]
        public void CIPipeline_PerformanceGateCommandLine_ClassExists()
        {
            // Arrange & Act
            var commandLineType = Type.GetType("XRBubbleLibrary.Core.Editor.PerformanceGateCommandLine");
            
            // Assert
            Assert.IsNotNull(commandLineType, "PerformanceGateCommandLine class should exist");
        }
        
        [Test]
        public void CIPipeline_PerformanceGateRunner_CanBeInstantiated()
        {
            // Arrange & Act
            var runner = PerformanceGateRunner.Instance;
            
            // Assert
            Assert.IsNotNull(runner, "PerformanceGateRunner should be instantiable");
        }
        
        [Test]
        public void CIPipeline_PerformanceGateRunner_HasDefaultGates()
        {
            // Arrange
            var runner = PerformanceGateRunner.Instance;
            
            // Act
            var gates = runner.GetRegisteredGates();
            
            // Assert
            Assert.IsNotNull(gates, "Registered gates should not be null");
            Assert.IsTrue(gates.Count >= 4, "Should have at least 4 default gates registered");
            
            // Check for expected gates
            var gateNames = new System.Collections.Generic.List<string>();
            foreach (var gate in gates)
            {
                gateNames.Add(gate.Name);
            }
            
            Assert.Contains("Unit Tests", gateNames, "Should have Unit Tests gate");
            Assert.Contains("Burst Compilation", gateNames, "Should have Burst Compilation gate");
            Assert.Contains("Performance Stub Test", gateNames, "Should have Performance Stub Test gate");
            Assert.Contains("Performance Profiling", gateNames, "Should have Performance Profiling gate");
        }
        
        [Test]
        public void CIPipeline_PerformanceGateRunner_ConfigurationIsValid()
        {
            // Arrange
            var runner = PerformanceGateRunner.Instance;
            
            // Act
            var validation = runner.ValidateConfiguration();
            
            // Assert
            Assert.IsNotNull(validation, "Validation result should not be null");
            Assert.IsTrue(validation.IsValid, $"Configuration should be valid. Issues: {string.Join(", ", validation.Issues)}");
            Assert.IsTrue(validation.GatesValidated > 0, "Should validate at least one gate");
        }
        
        [Test]
        public void CIPipeline_PerformanceThreshold_CanBeConfigured()
        {
            // Arrange
            var runner = PerformanceGateRunner.Instance;
            var customThreshold = new PerformanceThreshold
            {
                MinimumFPS = 72.0f,
                MaximumFrameTimeMs = 13.89f,
                MaximumMemoryUsageMB = 1024.0f,
                CustomThresholds = new System.Collections.Generic.Dictionary<string, float>
                {
                    ["TestThreshold"] = 42.0f
                }
            };
            
            // Act
            runner.SetFailureThreshold(customThreshold);
            var retrievedThreshold = runner.GetFailureThreshold();
            
            // Assert
            Assert.IsNotNull(retrievedThreshold, "Retrieved threshold should not be null");
            Assert.AreEqual(72.0f, retrievedThreshold.MinimumFPS, "Should set custom minimum FPS");
            Assert.AreEqual(13.89f, retrievedThreshold.MaximumFrameTimeMs, 0.01f, "Should set custom frame time");
            Assert.AreEqual(1024.0f, retrievedThreshold.MaximumMemoryUsageMB, "Should set custom memory limit");
            Assert.IsTrue(retrievedThreshold.CustomThresholds.ContainsKey("TestThreshold"), "Should contain custom threshold");
            Assert.AreEqual(42.0f, retrievedThreshold.CustomThresholds["TestThreshold"], "Should set custom threshold value");
        }
        
        [Test]
        public void CIPipeline_PerformanceGateRunner_CanGenerateReport()
        {
            // Arrange
            var runner = PerformanceGateRunner.Instance;
            
            // Act & Assert
            Assert.DoesNotThrow(() => {
                var reportPath = runner.GeneratePerformanceReport();
                Assert.IsNotNull(reportPath, "Report path should not be null");
                Assert.IsTrue(reportPath.EndsWith(".md"), "Report should be a markdown file");
                
                // Verify file exists
                Assert.IsTrue(File.Exists(reportPath), $"Report file should exist at: {reportPath}");
                
                // Verify file has content
                var content = File.ReadAllText(reportPath);
                Assert.IsTrue(content.Length > 0, "Report should have content");
                Assert.IsTrue(content.Contains("Performance Gate Report"), "Report should contain expected header");
            }, "Should generate performance report without throwing");
        }
        
        [Test]
        public void CIPipeline_PerformanceGates_HaveCorrectPriorities()
        {
            // Arrange
            var runner = PerformanceGateRunner.Instance;
            var gates = runner.GetRegisteredGates();
            
            // Act & Assert
            foreach (var gate in gates)
            {
                switch (gate.Name)
                {
                    case "Unit Tests":
                        Assert.AreEqual(1000, gate.Priority, "Unit Tests should have highest priority");
                        Assert.IsTrue(gate.IsCritical, "Unit Tests should be critical");
                        break;
                    case "Burst Compilation":
                        Assert.AreEqual(900, gate.Priority, "Burst Compilation should have second highest priority");
                        Assert.IsTrue(gate.IsCritical, "Burst Compilation should be critical");
                        break;
                    case "Performance Stub Test":
                        Assert.AreEqual(800, gate.Priority, "Performance Stub Test should have high priority");
                        Assert.IsTrue(gate.IsCritical, "Performance Stub Test should be critical");
                        break;
                    case "Performance Profiling":
                        Assert.AreEqual(800, gate.Priority, "Performance Profiling should have high priority");
                        Assert.IsTrue(gate.IsCritical, "Performance Profiling should be critical");
                        break;
                }
            }
        }
        
        [Test]
        public void CIPipeline_PerformanceGates_HaveValidConfiguration()
        {
            // Arrange
            var runner = PerformanceGateRunner.Instance;
            var gates = runner.GetRegisteredGates();
            
            // Act & Assert
            foreach (var gate in gates)
            {
                Assert.IsNotNull(gate.Name, $"Gate should have a name");
                Assert.IsNotEmpty(gate.Name, $"Gate name should not be empty");
                Assert.IsNotNull(gate.Description, $"Gate {gate.Name} should have a description");
                Assert.IsNotEmpty(gate.Description, $"Gate {gate.Name} description should not be empty");
                Assert.IsTrue(gate.Priority >= 0, $"Gate {gate.Name} should have non-negative priority");
                Assert.IsTrue(gate.ExpectedExecutionTime.TotalSeconds > 0, $"Gate {gate.Name} should have positive expected execution time");
                
                // Validate configuration
                Assert.IsTrue(gate.ValidateConfiguration(), $"Gate {gate.Name} configuration should be valid");
                
                // Check status
                var status = gate.GetStatus();
                Assert.IsNotNull(status, $"Gate {gate.Name} should return status");
                Assert.AreEqual(gate.Name, status.GateName, $"Status should match gate name");
            }
        }
        
        [Test]
        public void CIPipeline_EnvironmentDetection_WorksCorrectly()
        {
            // Arrange
            var runner = PerformanceGateRunner.Instance;
            
            // Act
            var history = runner.GetExecutionHistory();
            
            // Assert - This test mainly validates that environment detection doesn't crash
            Assert.IsNotNull(history, "Execution history should not be null");
            // History might be empty if no gates have been run yet, which is fine
        }
        
        [UnityTest]
        public IEnumerator CIPipeline_AsyncExecution_WorksCorrectly()
        {
            // Arrange
            var runner = PerformanceGateRunner.Instance;
            
            // Act
            var asyncTask = runner.RunAllGatesAsync();
            
            // Wait for completion with timeout
            var timeout = Time.time + 30.0f; // 30 second timeout
            while (!asyncTask.IsCompleted && Time.time < timeout)
            {
                yield return null;
            }
            
            // Assert
            Assert.IsTrue(asyncTask.IsCompleted, "Async execution should complete within timeout");
            
            var result = asyncTask.Result;
            Assert.IsNotNull(result, "Async result should not be null");
            Assert.IsNotNull(result.Summary, "Async result should have summary");
        }
        
        [Test]
        public void CIPipeline_CIScriptsExist()
        {
            // Arrange
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var ciScriptsPath = Path.Combine(projectRoot, "CI", "Scripts");
            
            // Act & Assert
            Assert.IsTrue(Directory.Exists(ciScriptsPath), $"CI Scripts directory should exist at: {ciScriptsPath}");
            
            var unixScript = Path.Combine(ciScriptsPath, "run-performance-gates.sh");
            var windowsScript = Path.Combine(ciScriptsPath, "run-performance-gates.bat");
            var pythonScript = Path.Combine(ciScriptsPath, "analyze-performance.py");
            
            Assert.IsTrue(File.Exists(unixScript), $"Unix CI script should exist at: {unixScript}");
            Assert.IsTrue(File.Exists(windowsScript), $"Windows CI script should exist at: {windowsScript}");
            Assert.IsTrue(File.Exists(pythonScript), $"Python analysis script should exist at: {pythonScript}");
        }
        
        [Test]
        public void CIPipeline_GitHubWorkflowExists()
        {
            // Arrange
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var workflowPath = Path.Combine(projectRoot, ".github", "workflows", "performance-gates.yml");
            
            // Act & Assert
            Assert.IsTrue(File.Exists(workflowPath), $"GitHub Actions workflow should exist at: {workflowPath}");
            
            var content = File.ReadAllText(workflowPath);
            Assert.IsTrue(content.Contains("Performance Gates CI/CD"), "Workflow should have correct name");
            Assert.IsTrue(content.Contains("run-performance-gates"), "Workflow should reference CI scripts");
        }
        
        [Test]
        public void CIPipeline_JenkinsfileExists()
        {
            // Arrange
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var jenkinsfilePath = Path.Combine(projectRoot, "CI", "Jenkinsfile");
            
            // Act & Assert
            Assert.IsTrue(File.Exists(jenkinsfilePath), $"Jenkinsfile should exist at: {jenkinsfilePath}");
            
            var content = File.ReadAllText(jenkinsfilePath);
            Assert.IsTrue(content.Contains("pipeline"), "Jenkinsfile should be a valid pipeline");
            Assert.IsTrue(content.Contains("PerformanceGateCommandLine"), "Jenkinsfile should reference performance gates");
        }
        
        [Test]
        public void CIPipeline_Quest3Thresholds_AreCorrect()
        {
            // Arrange
            var runner = PerformanceGateRunner.Instance;
            var threshold = runner.GetFailureThreshold();
            
            // Act & Assert - Verify Quest 3 specific thresholds are reasonable
            Assert.IsTrue(threshold.MinimumFPS >= 60.0f, "Minimum FPS should be at least 60 for VR");
            Assert.IsTrue(threshold.MaximumMemoryUsageMB <= 2048.0f, "Memory limit should be reasonable for Quest 3");
            Assert.IsTrue(threshold.MaximumFrameTimeMs <= 20.0f, "Frame time should be reasonable for VR");
            
            // Check custom thresholds exist
            Assert.IsNotNull(threshold.CustomThresholds, "Custom thresholds should exist");
            
            if (threshold.CustomThresholds.ContainsKey("MaxDrawCalls"))
            {
                Assert.IsTrue(threshold.CustomThresholds["MaxDrawCalls"] > 0, "Max draw calls should be positive");
            }
            
            if (threshold.CustomThresholds.ContainsKey("MaxTriangles"))
            {
                Assert.IsTrue(threshold.CustomThresholds["MaxTriangles"] > 0, "Max triangles should be positive");
            }
        }
    }
}