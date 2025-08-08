using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the Development State Generator system.
    /// Tests report generation, module analysis, and validation functionality.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class DevStateGeneratorTests
    {
        private IDevStateGenerator _generator;
        private ICompilerFlagManager _mockFlagManager;
        
        [SetUp]
        public void SetUp()
        {
            _mockFlagManager = new CompilerFlagManager();
            _generator = new DevStateGenerator(_mockFlagManager);
        }
        
        [Test]
        public void GenerateReport_ReturnsValidReport()
        {
            // Act
            var report = _generator.GenerateReport();
            
            // Assert
            Assert.IsNotNull(report);
            Assert.IsTrue(report.GeneratedAt > DateTime.MinValue);
            Assert.IsNotEmpty(report.BuildVersion);
            Assert.IsNotEmpty(report.UnityVersion);
            Assert.IsNotEmpty(report.BuildConfiguration);
            Assert.IsNotNull(report.Modules);
            Assert.IsNotNull(report.CompilerFlags);
            Assert.IsNotNull(report.Summary);
        }
        
        [Test]
        public void GenerateReport_ContainsExpectedModules()
        {
            // Act
            var report = _generator.GenerateReport();
            
            // Assert
            Assert.IsTrue(report.Modules.Count > 0, "Should find at least one module");
            
            // Check for core module
            var coreModule = report.Modules.FirstOrDefault(m => m.AssemblyName == "XRBubbleLibrary.Core");
            Assert.IsNotNull(coreModule, "Should find Core module");
            Assert.AreEqual("Core System", coreModule.ModuleName);
        }
        
        [Test]
        public void GenerateReport_SummaryMatchesModuleCounts()
        {
            // Act
            var report = _generator.GenerateReport();
            
            // Assert
            var actualImplemented = report.Modules.Count(m => m.State == ModuleState.Implemented);
            var actualDisabled = report.Modules.Count(m => m.State == ModuleState.Disabled);
            var actualConceptual = report.Modules.Count(m => m.State == ModuleState.Conceptual);
            
            Assert.AreEqual(actualImplemented, report.Summary.ImplementedModules);
            Assert.AreEqual(actualDisabled, report.Summary.DisabledModules);
            Assert.AreEqual(actualConceptual, report.Summary.ConceptualModules);
            Assert.AreEqual(report.Modules.Count, report.Summary.TotalModules);
        }
        
        [Test]
        public void GenerateReport_IncludesCompilerFlags()
        {
            // Act
            var report = _generator.GenerateReport();
            
            // Assert
            Assert.IsTrue(report.CompilerFlags.Count > 0, "Should include compiler flags");
            
            // Check for expected flags
            Assert.IsTrue(report.CompilerFlags.ContainsKey(ExperimentalFeature.AI_INTEGRATION));
            Assert.IsTrue(report.CompilerFlags.ContainsKey(ExperimentalFeature.VOICE_PROCESSING));
            Assert.IsTrue(report.CompilerFlags.ContainsKey(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS));
        }
        
        [Test]
        public void GenerateReport_IncludesPerformanceMetrics()
        {
            // Act
            var report = _generator.GenerateReport();
            
            // Assert
            Assert.IsNotNull(report.CurrentPerformance);
            Assert.IsTrue(report.CurrentPerformance.CapturedAt > DateTime.MinValue);
            Assert.IsNotEmpty(report.CurrentPerformance.BuildVersion);
            Assert.IsTrue(report.CurrentPerformance.AverageFPS >= 0);
            Assert.IsTrue(report.CurrentPerformance.AverageFrameTime >= 0);
        }
        
        [Test]
        public void GetBuildVersion_ReturnsValidVersion()
        {
            // Act
            var version = _generator.GetBuildVersion();
            
            // Assert
            Assert.IsNotEmpty(version);
            Assert.IsTrue(version.Length > 0);
        }
        
        [Test]
        public void GetAssemblyDefinitions_ReturnsValidPaths()
        {
            // Act
            var assemblyPaths = _generator.GetAssemblyDefinitions();
            
            // Assert
            Assert.IsNotNull(assemblyPaths);
            Assert.IsTrue(assemblyPaths.Count > 0, "Should find at least one assembly definition");
            
            // Check that all paths exist and are .asmdef files
            foreach (var path in assemblyPaths)
            {
                Assert.IsTrue(File.Exists(path), $"Assembly definition file should exist: {path}");
                Assert.IsTrue(path.EndsWith(".asmdef"), $"Should be .asmdef file: {path}");
            }
        }
        
        [Test]
        public void ValidateReportAccuracy_WithValidReport_ReturnsTrue()
        {
            // Arrange
            var report = _generator.GenerateReport();
            
            // Act
            var isValid = _generator.ValidateReportAccuracy(report);
            
            // Assert
            Assert.IsTrue(isValid);
        }
        
        [Test]
        public void ValidateReportAccuracy_WithNullReport_ReturnsFalse()
        {
            // Act
            var isValid = _generator.ValidateReportAccuracy(null);
            
            // Assert
            Assert.IsFalse(isValid);
        }
        
        [Test]
        public void ValidateReportAccuracy_WithInconsistentSummary_ReturnsFalse()
        {
            // Arrange
            var report = _generator.GenerateReport();
            report.Summary.ImplementedModules = 999; // Invalid count
            
            // Act
            var isValid = _generator.ValidateReportAccuracy(report);
            
            // Assert
            Assert.IsFalse(isValid);
        }
        
        [Test]
        public void ToMarkdown_GeneratesValidMarkdown()
        {
            // Arrange
            var report = _generator.GenerateReport();
            
            // Act
            var markdown = report.ToMarkdown();
            
            // Assert
            Assert.IsNotEmpty(markdown);
            Assert.That(markdown, Contains.Substring("# XR Bubble Library - Development State Report"));
            Assert.That(markdown, Contains.Substring("## Summary"));
            Assert.That(markdown, Contains.Substring("## Compiler Flags Status"));
            Assert.That(markdown, Contains.Substring("## Module Status"));
            Assert.That(markdown, Contains.Substring("Generated"));
        }
        
        [Test]
        public void ToJson_GeneratesValidJson()
        {
            // Arrange
            var report = _generator.GenerateReport();
            
            // Act
            var json = report.ToJson();
            
            // Assert
            Assert.IsNotEmpty(json);
            Assert.That(json, Contains.Substring("GeneratedAt"));
            Assert.That(json, Contains.Substring("BuildVersion"));
            Assert.That(json, Contains.Substring("Modules"));
        }
        
        [Test]
        public void DevStateSummary_CalculatesPercentagesCorrectly()
        {
            // Arrange
            var summary = new DevStateSummary
            {
                TotalModules = 10,
                ImplementedModules = 6,
                DisabledModules = 2,
                ConceptualModules = 2
            };
            
            // Act & Assert
            Assert.AreEqual(60f, summary.ImplementedPercentage, 0.1f);
            Assert.AreEqual(20f, summary.DisabledPercentage, 0.1f);
            Assert.AreEqual(20f, summary.ConceptualPercentage, 0.1f);
        }
        
        [Test]
        public void DevStateSummary_WithZeroModules_ReturnsZeroPercentages()
        {
            // Arrange
            var summary = new DevStateSummary
            {
                TotalModules = 0,
                ImplementedModules = 0,
                DisabledModules = 0,
                ConceptualModules = 0
            };
            
            // Act & Assert
            Assert.AreEqual(0f, summary.ImplementedPercentage);
            Assert.AreEqual(0f, summary.DisabledPercentage);
            Assert.AreEqual(0f, summary.ConceptualPercentage);
        }
        
        [Test]
        public void ModuleStatusAnalyzer_AnalyzeModule_WithValidAssembly_ReturnsStatus()
        {
            // Arrange
            var assemblyPaths = _generator.GetAssemblyDefinitions();
            Assert.IsTrue(assemblyPaths.Count > 0, "Need at least one assembly for testing");
            
            var testAssemblyPath = assemblyPaths.First();
            
            // Act
            var moduleStatus = ModuleStatusAnalyzer.AnalyzeModule(testAssemblyPath);
            
            // Assert
            Assert.IsNotNull(moduleStatus);
            Assert.IsNotEmpty(moduleStatus.ModuleName);
            Assert.IsNotEmpty(moduleStatus.AssemblyName);
            Assert.AreEqual(testAssemblyPath, moduleStatus.AssemblyPath);
            Assert.IsTrue(Enum.IsDefined(typeof(ModuleState), moduleStatus.State));
            Assert.IsNotNull(moduleStatus.Dependencies);
            Assert.IsNotNull(moduleStatus.DefineConstraints);
        }
        
        [Test]
        public void ModuleStatusAnalyzer_AnalyzeModule_WithInvalidPath_ReturnsNull()
        {
            // Arrange
            var invalidPath = "nonexistent/path/to/assembly.asmdef";
            
            // Act
            var moduleStatus = ModuleStatusAnalyzer.AnalyzeModule(invalidPath);
            
            // Assert
            Assert.IsNull(moduleStatus);
        }
        
        [Test]
        public void ModuleStatusAnalyzer_AnalyzeModules_ReturnsAllValidModules()
        {
            // Arrange
            var assemblyPaths = _generator.GetAssemblyDefinitions();
            
            // Act
            var modules = ModuleStatusAnalyzer.AnalyzeModules(assemblyPaths);
            
            // Assert
            Assert.IsNotNull(modules);
            Assert.IsTrue(modules.Count > 0);
            
            // All modules should have valid data
            foreach (var module in modules)
            {
                Assert.IsNotEmpty(module.ModuleName);
                Assert.IsNotEmpty(module.AssemblyName);
                Assert.IsNotEmpty(module.AssemblyPath);
                Assert.IsTrue(Enum.IsDefined(typeof(ModuleState), module.State));
            }
        }
        
        [Test]
        public void EvidenceFile_HasRequiredProperties()
        {
            // Arrange
            var evidence = new EvidenceFile
            {
                FileName = "test.log",
                FilePath = "/path/to/test.log",
                CreatedAt = DateTime.UtcNow,
                ClaimSupported = "Test claim",
                Type = EvidenceType.TestResults,
                SHA256Hash = "abcd1234"
            };
            
            // Assert
            Assert.AreEqual("test.log", evidence.FileName);
            Assert.AreEqual("/path/to/test.log", evidence.FilePath);
            Assert.AreEqual("Test claim", evidence.ClaimSupported);
            Assert.AreEqual(EvidenceType.TestResults, evidence.Type);
            Assert.AreEqual("abcd1234", evidence.SHA256Hash);
            Assert.IsNotNull(evidence.Metadata);
        }
        
        [Test]
        public void PerformanceMetrics_HasRequiredProperties()
        {
            // Arrange
            var metrics = new PerformanceMetrics
            {
                AverageFPS = 60.0f,
                AverageFrameTime = 16.67f,
                MemoryUsage = 1024 * 1024 * 100, // 100 MB
                CPUUsage = 45.5f,
                CapturedAt = DateTime.UtcNow,
                BuildVersion = "1.0.0"
            };
            
            // Assert
            Assert.AreEqual(60.0f, metrics.AverageFPS, 0.1f);
            Assert.AreEqual(16.67f, metrics.AverageFrameTime, 0.1f);
            Assert.AreEqual(1024 * 1024 * 100, metrics.MemoryUsage);
            Assert.AreEqual(45.5f, metrics.CPUUsage, 0.1f);
            Assert.AreEqual("1.0.0", metrics.BuildVersion);
            Assert.IsNotNull(metrics.AdditionalMetrics);
        }
        
        [Test]
        public void DevStateReport_ValidateConsistency_WithValidReport_ReturnsNoIssues()
        {
            // Arrange
            var report = _generator.GenerateReport();
            
            // Act
            var issues = report.ValidateConsistency();
            
            // Assert
            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count, $"Should have no validation issues, but found: {string.Join(", ", issues)}");
        }
        
        [Test]
        public void DevStateReport_ValidateConsistency_WithDuplicateModules_ReturnsIssues()
        {
            // Arrange
            var report = new DevStateReport();
            report.Modules.Add(new ModuleStatus { ModuleName = "TestModule" });
            report.Modules.Add(new ModuleStatus { ModuleName = "TestModule" }); // Duplicate
            report.Summary = new DevStateSummary { TotalModules = 2 };
            
            // Act
            var issues = report.ValidateConsistency();
            
            // Assert
            Assert.IsTrue(issues.Count > 0);
            Assert.IsTrue(issues.Any(i => i.Contains("Duplicate module name")));
        }
        
        [Test]
        public void DevStateReport_ValidateConsistency_WithInconsistentSummary_ReturnsIssues()
        {
            // Arrange
            var report = new DevStateReport();
            report.Modules.Add(new ModuleStatus { State = ModuleState.Implemented });
            report.Summary = new DevStateSummary 
            { 
                TotalModules = 1,
                ImplementedModules = 5 // Inconsistent with actual count
            };
            
            // Act
            var issues = report.ValidateConsistency();
            
            // Assert
            Assert.IsTrue(issues.Count > 0);
            Assert.IsTrue(issues.Any(i => i.Contains("Summary implemented count")));
        }
        
        [Test]
        public async void GenerateReportAsync_ReturnsValidReport()
        {
            // Act
            var report = await _generator.GenerateReportAsync();
            
            // Assert
            Assert.IsNotNull(report);
            Assert.IsTrue(report.GeneratedAt > DateTime.MinValue);
            Assert.IsNotEmpty(report.BuildVersion);
        }
        
        [Test]
        public void GenerateAndSaveDevStateFile_CreatesFile()
        {
            // Act
            var filePath = _generator.GenerateAndSaveDevStateFile();
            
            // Assert
            Assert.IsNotEmpty(filePath);
            Assert.IsTrue(File.Exists(filePath), $"DEV_STATE.md file should be created at: {filePath}");
            
            // Verify file content
            var content = File.ReadAllText(filePath);
            Assert.IsNotEmpty(content);
            Assert.That(content, Contains.Substring("# XR Bubble Library - Development State Report"));
            
            // Cleanup
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        
        [Test]
        public async void GenerateAndSaveDevStateFileAsync_CreatesFile()
        {
            // Act
            var filePath = await _generator.GenerateAndSaveDevStateFileAsync();
            
            // Assert
            Assert.IsNotEmpty(filePath);
            Assert.IsTrue(File.Exists(filePath), $"DEV_STATE.md file should be created at: {filePath}");
            
            // Cleanup
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}