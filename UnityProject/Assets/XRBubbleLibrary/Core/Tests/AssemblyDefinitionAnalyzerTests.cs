using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the enhanced Assembly Definition Analyzer.
    /// Tests dependency analysis, circular dependency detection, and module state determination.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class AssemblyDefinitionAnalyzerTests
    {
        private IAssemblyDefinitionAnalyzer _analyzer;
        private List<string> _testAssemblyPaths;
        
        [SetUp]
        public void SetUp()
        {
            _analyzer = new AssemblyDefinitionAnalyzerService();
            _testAssemblyPaths = GetTestAssemblyPaths();
        }
        
        [Test]
        public void AnalyzeAssemblyDefinitions_WithValidPaths_ReturnsAnalysisResult()
        {
            // Act
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.AnalyzedAt > System.DateTime.MinValue);
            Assert.IsNotEmpty(result.AnalysisVersion);
            Assert.IsNotNull(result.Assemblies);
            Assert.IsNotNull(result.Dependencies);
            Assert.IsNotNull(result.CircularDependencies);
            Assert.IsNotNull(result.ModuleStates);
            Assert.IsNotNull(result.ValidationIssues);
        }
        
        [Test]
        public void AnalyzeAssemblyDefinitions_FindsExpectedAssemblies()
        {
            // Act
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Assert
            Assert.IsTrue(result.Assemblies.Count > 0, "Should find at least one assembly");
            
            // Check for core assembly
            var coreAssembly = result.Assemblies.FirstOrDefault(a => a.Name == "XRBubbleLibrary.Core");
            Assert.IsNotNull(coreAssembly, "Should find Core assembly");
            Assert.IsNotEmpty(coreAssembly.FilePath);
            Assert.IsNotNull(coreAssembly.ModuleDirectory);
        }
        
        [Test]
        public void AnalyzeAssemblyDefinitions_AnalyzesFileStructure()
        {
            // Act
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Assert
            var coreAssembly = result.Assemblies.FirstOrDefault(a => a.Name == "XRBubbleLibrary.Core");
            if (coreAssembly != null)
            {
                Assert.IsNotNull(coreAssembly.SourceFiles);
                Assert.IsNotNull(coreAssembly.TestFiles);
                Assert.IsNotNull(coreAssembly.EditorFiles);
                
                // Core assembly should have source files
                Assert.IsTrue(coreAssembly.SourceFiles.Count > 0, "Core assembly should have source files");
            }
        }
        
        [Test]
        public void AnalyzeAssemblyDefinitions_BuildsDependencyGraph()
        {
            // Act
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Assert
            Assert.IsNotNull(result.Dependencies);
            
            // Should have some dependencies
            if (result.Assemblies.Count > 1)
            {
                Assert.IsTrue(result.Dependencies.Count > 0, "Should find dependencies between assemblies");
            }
            
            // Check dependency types
            var dependencyTypes = result.Dependencies.Select(d => d.Type).Distinct().ToList();
            Assert.IsTrue(dependencyTypes.Count > 0, "Should have different types of dependencies");
        }
        
        [Test]
        public void AnalyzeAssemblyDefinitions_DeterminesModuleStates()
        {
            // Act
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Assert
            Assert.IsNotNull(result.ModuleStates);
            Assert.AreEqual(result.Assemblies.Count, result.ModuleStates.Count, 
                "Should have state for each assembly");
            
            // Check that states are valid
            foreach (var state in result.ModuleStates.Values)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(ModuleState), state));
            }
            
            // Core assembly should be implemented (if it has enough files)
            if (result.ModuleStates.ContainsKey("XRBubbleLibrary.Core"))
            {
                var coreState = result.ModuleStates["XRBubbleLibrary.Core"];
                Assert.IsTrue(coreState == ModuleState.Implemented || coreState == ModuleState.Conceptual,
                    "Core assembly should be implemented or conceptual");
            }
        }
        
        [Test]
        public void AnalyzeAssemblyDefinitions_DetectsCircularDependencies()
        {
            // Act
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Assert
            Assert.IsNotNull(result.CircularDependencies);
            
            // If circular dependencies are found, they should have valid data
            foreach (var circular in result.CircularDependencies)
            {
                Assert.IsNotNull(circular.AssemblyChain);
                Assert.IsTrue(circular.AssemblyChain.Count >= 2, "Circular dependency should have at least 2 assemblies");
                Assert.IsNotEmpty(circular.Description);
                Assert.IsTrue(System.Enum.IsDefined(typeof(AssemblyDefinitionAnalyzer.CircularDependencySeverity), circular.Severity));
            }
        }
        
        [Test]
        public void AnalyzeAssemblyDefinitions_ValidatesConfiguration()
        {
            // Act
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Assert
            Assert.IsNotNull(result.ValidationIssues);
            
            // Validation issues should have valid data
            foreach (var issue in result.ValidationIssues)
            {
                Assert.IsNotEmpty(issue.Title);
                Assert.IsNotEmpty(issue.Description);
                Assert.IsTrue(System.Enum.IsDefined(typeof(AssemblyDefinitionAnalyzer.ValidationSeverity), issue.Severity));
            }
        }
        
        [Test]
        public async void AnalyzeAssemblyDefinitionsAsync_ReturnsValidResult()
        {
            // Act
            var result = await _analyzer.AnalyzeAssemblyDefinitionsAsync(_testAssemblyPaths);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.AnalyzedAt > System.DateTime.MinValue);
            Assert.IsNotNull(result.Assemblies);
        }
        
        [Test]
        public void AnalyzeSingleAssembly_WithValidPath_ReturnsAssemblyInfo()
        {
            // Arrange
            var testPath = _testAssemblyPaths.FirstOrDefault();
            Assert.IsNotNull(testPath, "Need at least one test assembly path");
            
            // Act
            var assemblyInfo = _analyzer.AnalyzeSingleAssembly(testPath);
            
            // Assert
            Assert.IsNotNull(assemblyInfo);
            Assert.IsNotEmpty(assemblyInfo.Name);
            Assert.AreEqual(testPath, assemblyInfo.FilePath);
            Assert.IsNotNull(assemblyInfo.References);
            Assert.IsNotNull(assemblyInfo.DefineConstraints);
        }
        
        [Test]
        public void AnalyzeSingleAssembly_WithInvalidPath_ReturnsNull()
        {
            // Arrange
            var invalidPath = "nonexistent/path/to/assembly.asmdef";
            
            // Act
            var assemblyInfo = _analyzer.AnalyzeSingleAssembly(invalidPath);
            
            // Assert
            Assert.IsNull(assemblyInfo);
        }
        
        [Test]
        public void BuildDependencyGraph_WithValidAssemblies_ReturnsDependencies()
        {
            // Arrange
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Act
            var dependencies = _analyzer.BuildDependencyGraph(result.Assemblies);
            
            // Assert
            Assert.IsNotNull(dependencies);
            
            // Should have dependencies if assemblies reference each other
            if (result.Assemblies.Any(a => a.References.Count > 0))
            {
                Assert.IsTrue(dependencies.Count > 0, "Should find dependencies");
            }
        }
        
        [Test]
        public void DetectCircularDependencies_WithNoCycles_ReturnsEmpty()
        {
            // Arrange
            var dependencies = new List<AssemblyDefinitionAnalyzer.DependencyRelationship>
            {
                new AssemblyDefinitionAnalyzer.DependencyRelationship
                {
                    FromAssembly = "A",
                    ToAssembly = "B",
                    Type = AssemblyDefinitionAnalyzer.DependencyType.Direct
                },
                new AssemblyDefinitionAnalyzer.DependencyRelationship
                {
                    FromAssembly = "B",
                    ToAssembly = "C",
                    Type = AssemblyDefinitionAnalyzer.DependencyType.Direct
                }
            };
            
            // Act
            var circularDeps = _analyzer.DetectCircularDependencies(dependencies);
            
            // Assert
            Assert.IsNotNull(circularDeps);
            Assert.AreEqual(0, circularDeps.Count, "Should not find circular dependencies in linear chain");
        }
        
        [Test]
        public void DetermineModuleStates_WithValidData_ReturnsStates()
        {
            // Arrange
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Act
            var states = _analyzer.DetermineModuleStates(result.Assemblies, result.Dependencies);
            
            // Assert
            Assert.IsNotNull(states);
            Assert.AreEqual(result.Assemblies.Count, states.Count);
            
            foreach (var state in states.Values)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(ModuleState), state));
            }
        }
        
        [Test]
        public void ValidateConfiguration_WithValidData_ReturnsIssues()
        {
            // Arrange
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Act
            var issues = _analyzer.ValidateConfiguration(result.Assemblies, result.Dependencies, result.CircularDependencies);
            
            // Assert
            Assert.IsNotNull(issues);
            
            foreach (var issue in issues)
            {
                Assert.IsNotEmpty(issue.Title);
                Assert.IsNotEmpty(issue.Description);
                Assert.IsTrue(System.Enum.IsDefined(typeof(AssemblyDefinitionAnalyzer.ValidationSeverity), issue.Severity));
            }
        }
        
        [Test]
        public void GenerateAnalysisReport_WithValidResult_ReturnsMarkdown()
        {
            // Arrange
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Act
            var report = _analyzer.GenerateAnalysisReport(result);
            
            // Assert
            Assert.IsNotEmpty(report);
            Assert.That(report, Contains.Substring("# Assembly Definition Analysis Report"));
            Assert.That(report, Contains.Substring("## Summary"));
            Assert.That(report, Contains.Substring("## Assembly Details"));
            Assert.That(report, Contains.Substring("Generated"));
        }
        
        [Test]
        public void ExportToJson_WithValidResult_ReturnsJson()
        {
            // Arrange
            var result = _analyzer.AnalyzeAssemblyDefinitions(_testAssemblyPaths);
            
            // Act
            var json = _analyzer.ExportToJson(result);
            
            // Assert
            Assert.IsNotEmpty(json);
            Assert.That(json, Contains.Substring("AnalyzedAt"));
            Assert.That(json, Contains.Substring("Assemblies"));
            Assert.That(json, Contains.Substring("Dependencies"));
        }
        
        [Test]
        public void GetDependencyChain_WithConnectedAssemblies_ReturnsPath()
        {
            // Arrange
            var dependencies = new List<AssemblyDefinitionAnalyzer.DependencyRelationship>
            {
                new AssemblyDefinitionAnalyzer.DependencyRelationship
                {
                    FromAssembly = "A",
                    ToAssembly = "B",
                    Type = AssemblyDefinitionAnalyzer.DependencyType.Direct
                },
                new AssemblyDefinitionAnalyzer.DependencyRelationship
                {
                    FromAssembly = "B",
                    ToAssembly = "C",
                    Type = AssemblyDefinitionAnalyzer.DependencyType.Direct
                }
            };
            
            // Act
            var chain = _analyzer.GetDependencyChain("A", "C", dependencies);
            
            // Assert
            Assert.IsNotNull(chain);
            Assert.IsTrue(chain.Count >= 2, "Should find path from A to C");
            Assert.AreEqual("A", chain.First());
            Assert.AreEqual("C", chain.Last());
        }
        
        [Test]
        public void GetDependencyChain_WithDisconnectedAssemblies_ReturnsEmpty()
        {
            // Arrange
            var dependencies = new List<AssemblyDefinitionAnalyzer.DependencyRelationship>
            {
                new AssemblyDefinitionAnalyzer.DependencyRelationship
                {
                    FromAssembly = "A",
                    ToAssembly = "B",
                    Type = AssemblyDefinitionAnalyzer.DependencyType.Direct
                }
            };
            
            // Act
            var chain = _analyzer.GetDependencyChain("A", "Z", dependencies);
            
            // Assert
            Assert.IsNotNull(chain);
            Assert.AreEqual(0, chain.Count, "Should not find path from A to Z");
        }
        
        [Test]
        public void CalculateImplementationScore_WithSourceFiles_ReturnsPositiveScore()
        {
            // Arrange
            var assembly = new AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo
            {
                Name = "TestAssembly",
                SourceFiles = new List<FileInfo> { new FileInfo("test1.cs"), new FileInfo("test2.cs") },
                TestFiles = new List<FileInfo> { new FileInfo("test.cs") },
                EditorFiles = new List<FileInfo>()
            };
            var dependencies = new List<AssemblyDefinitionAnalyzer.DependencyRelationship>();
            
            // Act
            var score = _analyzer.CalculateImplementationScore(assembly, dependencies);
            
            // Assert
            Assert.IsTrue(score > 0, "Should return positive score for assembly with source files");
            Assert.IsTrue(score >= 2.0f, "Should account for source files");
        }
        
        [Test]
        public void CalculateImplementationScore_WithNoFiles_ReturnsZeroScore()
        {
            // Arrange
            var assembly = new AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo
            {
                Name = "EmptyAssembly",
                SourceFiles = new List<FileInfo>(),
                TestFiles = new List<FileInfo>(),
                EditorFiles = new List<FileInfo>()
            };
            var dependencies = new List<AssemblyDefinitionAnalyzer.DependencyRelationship>();
            
            // Act
            var score = _analyzer.CalculateImplementationScore(assembly, dependencies);
            
            // Assert
            Assert.AreEqual(0f, score, "Should return zero score for empty assembly");
        }
        
        [Test]
        public void AssemblyDefinitionInfo_HasRequiredProperties()
        {
            // Arrange & Act
            var assemblyInfo = new AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo
            {
                Name = "TestAssembly",
                FilePath = "/path/to/test.asmdef",
                RootNamespace = "Test.Namespace"
            };
            
            // Assert
            Assert.AreEqual("TestAssembly", assemblyInfo.Name);
            Assert.AreEqual("/path/to/test.asmdef", assemblyInfo.FilePath);
            Assert.AreEqual("Test.Namespace", assemblyInfo.RootNamespace);
            Assert.IsNotNull(assemblyInfo.References);
            Assert.IsNotNull(assemblyInfo.DefineConstraints);
            Assert.IsNotNull(assemblyInfo.SourceFiles);
            Assert.IsNotNull(assemblyInfo.TestFiles);
            Assert.IsNotNull(assemblyInfo.EditorFiles);
        }
        
        [Test]
        public void DependencyRelationship_HasRequiredProperties()
        {
            // Arrange & Act
            var dependency = new AssemblyDefinitionAnalyzer.DependencyRelationship
            {
                FromAssembly = "AssemblyA",
                ToAssembly = "AssemblyB",
                Type = AssemblyDefinitionAnalyzer.DependencyType.Direct,
                IsOptional = false,
                Reason = "Direct reference"
            };
            
            // Assert
            Assert.AreEqual("AssemblyA", dependency.FromAssembly);
            Assert.AreEqual("AssemblyB", dependency.ToAssembly);
            Assert.AreEqual(AssemblyDefinitionAnalyzer.DependencyType.Direct, dependency.Type);
            Assert.IsFalse(dependency.IsOptional);
            Assert.AreEqual("Direct reference", dependency.Reason);
        }
        
        [Test]
        public void CircularDependency_HasRequiredProperties()
        {
            // Arrange & Act
            var circular = new AssemblyDefinitionAnalyzer.CircularDependency
            {
                AssemblyChain = new List<string> { "A", "B", "C", "A" },
                Description = "Test circular dependency",
                Severity = AssemblyDefinitionAnalyzer.CircularDependencySeverity.High
            };
            
            // Assert
            Assert.IsNotNull(circular.AssemblyChain);
            Assert.AreEqual(4, circular.AssemblyChain.Count);
            Assert.AreEqual("Test circular dependency", circular.Description);
            Assert.AreEqual(AssemblyDefinitionAnalyzer.CircularDependencySeverity.High, circular.Severity);
        }
        
        [Test]
        public void ValidationIssue_HasRequiredProperties()
        {
            // Arrange & Act
            var issue = new AssemblyDefinitionAnalyzer.ValidationIssue
            {
                AssemblyName = "TestAssembly",
                Severity = AssemblyDefinitionAnalyzer.ValidationSeverity.Warning,
                Title = "Test Issue",
                Description = "Test description",
                Recommendation = "Test recommendation"
            };
            
            // Assert
            Assert.AreEqual("TestAssembly", issue.AssemblyName);
            Assert.AreEqual(AssemblyDefinitionAnalyzer.ValidationSeverity.Warning, issue.Severity);
            Assert.AreEqual("Test Issue", issue.Title);
            Assert.AreEqual("Test description", issue.Description);
            Assert.AreEqual("Test recommendation", issue.Recommendation);
        }
        
        /// <summary>
        /// Get list of test assembly paths for testing.
        /// </summary>
        private List<string> GetTestAssemblyPaths()
        {
            var paths = new List<string>();
            
            // Find assembly definitions in the XRBubbleLibrary directory
            var xrBubbleLibraryPath = Path.Combine(Application.dataPath, "XRBubbleLibrary");
            
            if (Directory.Exists(xrBubbleLibraryPath))
            {
                var asmdefFiles = Directory.GetFiles(xrBubbleLibraryPath, "*.asmdef", SearchOption.AllDirectories);
                paths.AddRange(asmdefFiles);
            }
            
            return paths;
        }
    }
}