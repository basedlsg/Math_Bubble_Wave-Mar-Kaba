using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Service implementation of assembly definition analysis.
    /// Provides dependency injection support and wraps the static analyzer.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class AssemblyDefinitionAnalyzerService : IAssemblyDefinitionAnalyzer
    {
        /// <summary>
        /// Perform comprehensive analysis of all assembly definitions in the project.
        /// </summary>
        public AssemblyDefinitionAnalyzer.AnalysisResult AnalyzeAssemblyDefinitions(List<string> assemblyPaths)
        {
            return AssemblyDefinitionAnalyzer.AnalyzeAssemblyDefinitions(assemblyPaths);
        }
        
        /// <summary>
        /// Perform comprehensive analysis asynchronously.
        /// </summary>
        public async Task<AssemblyDefinitionAnalyzer.AnalysisResult> AnalyzeAssemblyDefinitionsAsync(List<string> assemblyPaths)
        {
            return await Task.Run(() => AssemblyDefinitionAnalyzer.AnalyzeAssemblyDefinitions(assemblyPaths));
        }
        
        /// <summary>
        /// Analyze a single assembly definition file.
        /// </summary>
        public AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo AnalyzeSingleAssembly(string assemblyPath)
        {
            var result = AssemblyDefinitionAnalyzer.AnalyzeAssemblyDefinitions(new List<string> { assemblyPath });
            return result.Assemblies.FirstOrDefault();
        }
        
        /// <summary>
        /// Build dependency graph for a set of assemblies.
        /// </summary>
        public List<AssemblyDefinitionAnalyzer.DependencyRelationship> BuildDependencyGraph(
            List<AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo> assemblies)
        {
            var result = AssemblyDefinitionAnalyzer.AnalyzeAssemblyDefinitions(
                assemblies.Select(a => a.FilePath).ToList());
            return result.Dependencies;
        }
        
        /// <summary>
        /// Detect circular dependencies in the assembly dependency graph.
        /// </summary>
        public List<AssemblyDefinitionAnalyzer.CircularDependency> DetectCircularDependencies(
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies)
        {
            // Create a minimal analysis result to use the static analyzer
            var dummyAssemblies = dependencies
                .SelectMany(d => new[] { d.FromAssembly, d.ToAssembly })
                .Distinct()
                .Select(name => new AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo { Name = name })
                .ToList();
            
            var result = AssemblyDefinitionAnalyzer.AnalyzeAssemblyDefinitions(new List<string>());
            return result.CircularDependencies;
        }
        
        /// <summary>
        /// Determine module states based on comprehensive analysis.
        /// </summary>
        public Dictionary<string, ModuleState> DetermineModuleStates(
            List<AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo> assemblies,
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies)
        {
            var result = AssemblyDefinitionAnalyzer.AnalyzeAssemblyDefinitions(
                assemblies.Select(a => a.FilePath).ToList());
            return result.ModuleStates;
        }
        
        /// <summary>
        /// Validate assembly configuration and detect issues.
        /// </summary>
        public List<AssemblyDefinitionAnalyzer.ValidationIssue> ValidateConfiguration(
            List<AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo> assemblies,
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies,
            List<AssemblyDefinitionAnalyzer.CircularDependency> circularDependencies)
        {
            var result = AssemblyDefinitionAnalyzer.AnalyzeAssemblyDefinitions(
                assemblies.Select(a => a.FilePath).ToList());
            return result.ValidationIssues;
        }
        
        /// <summary>
        /// Generate a comprehensive analysis report.
        /// </summary>
        public string GenerateAnalysisReport(AssemblyDefinitionAnalyzer.AnalysisResult analysisResult)
        {
            var report = "# Assembly Definition Analysis Report\n\n";
            
            // Header information
            report += $"**Generated**: {analysisResult.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC\n";
            report += $"**Analysis Version**: {analysisResult.AnalysisVersion}\n";
            report += $"**Total Assemblies**: {analysisResult.Assemblies.Count}\n\n";
            
            // Summary section
            report += "## Summary\n\n";
            var implementedCount = analysisResult.ModuleStates.Count(s => s.Value == ModuleState.Implemented);
            var disabledCount = analysisResult.ModuleStates.Count(s => s.Value == ModuleState.Disabled);
            var conceptualCount = analysisResult.ModuleStates.Count(s => s.Value == ModuleState.Conceptual);
            
            report += $"- **Implemented Modules**: {implementedCount}\n";
            report += $"- **Disabled Modules**: {disabledCount}\n";
            report += $"- **Conceptual Modules**: {conceptualCount}\n";
            report += $"- **Total Dependencies**: {analysisResult.Dependencies.Count}\n";
            report += $"- **Circular Dependencies**: {analysisResult.CircularDependencies.Count}\n";
            report += $"- **Validation Issues**: {analysisResult.ValidationIssues.Count}\n\n";
            
            // Assembly details
            report += "## Assembly Details\n\n";
            report += "| Assembly | State | Source Files | Test Files | Dependencies |\n";
            report += "|----------|-------|--------------|------------|-------------|\n";
            
            foreach (var assembly in analysisResult.Assemblies.OrderBy(a => a.Name))
            {
                var state = analysisResult.ModuleStates.GetValueOrDefault(assembly.Name, ModuleState.Conceptual);
                var stateIcon = state switch
                {
                    ModuleState.Implemented => "✅",
                    ModuleState.Disabled => "❌",
                    ModuleState.Conceptual => "🔬",
                    _ => "❓"
                };
                
                var dependencyCount = analysisResult.Dependencies.Count(d => d.FromAssembly == assembly.Name);
                
                report += $"| {assembly.Name} | {stateIcon} {state} | {assembly.SourceFiles.Count} | {assembly.TestFiles.Count} | {dependencyCount} |\n";
            }
            report += "\n";
            
            // Dependency graph
            if (analysisResult.Dependencies.Count > 0)
            {
                report += "## Dependency Relationships\n\n";
                
                var dependencyGroups = analysisResult.Dependencies.GroupBy(d => d.Type);
                foreach (var group in dependencyGroups.OrderBy(g => g.Key.ToString()))
                {
                    report += $"### {group.Key} Dependencies\n\n";
                    foreach (var dependency in group.OrderBy(d => d.FromAssembly).ThenBy(d => d.ToAssembly))
                    {
                        var optionalText = dependency.IsOptional ? " (Optional)" : "";
                        report += $"- **{dependency.FromAssembly}** → **{dependency.ToAssembly}**{optionalText}\n";
                        if (!string.IsNullOrEmpty(dependency.Reason))
                        {
                            report += $"  - {dependency.Reason}\n";
                        }
                    }
                    report += "\n";
                }
            }
            
            // Circular dependencies
            if (analysisResult.CircularDependencies.Count > 0)
            {
                report += "## Circular Dependencies\n\n";
                foreach (var circular in analysisResult.CircularDependencies.OrderByDescending(c => c.Severity))
                {
                    var severityIcon = circular.Severity switch
                    {
                        AssemblyDefinitionAnalyzer.CircularDependencySeverity.Critical => "🔴",
                        AssemblyDefinitionAnalyzer.CircularDependencySeverity.High => "🟠",
                        AssemblyDefinitionAnalyzer.CircularDependencySeverity.Medium => "🟡",
                        AssemblyDefinitionAnalyzer.CircularDependencySeverity.Low => "🟢",
                        _ => "⚪"
                    };
                    
                    report += $"### {severityIcon} {circular.Severity} Severity\n";
                    report += $"{circular.Description}\n\n";
                }
            }
            
            // Validation issues
            if (analysisResult.ValidationIssues.Count > 0)
            {
                report += "## Validation Issues\n\n";
                
                var issueGroups = analysisResult.ValidationIssues.GroupBy(i => i.Severity);
                foreach (var group in issueGroups.OrderByDescending(g => g.Key))
                {
                    var severityIcon = group.Key switch
                    {
                        AssemblyDefinitionAnalyzer.ValidationSeverity.Critical => "🔴",
                        AssemblyDefinitionAnalyzer.ValidationSeverity.Error => "❌",
                        AssemblyDefinitionAnalyzer.ValidationSeverity.Warning => "⚠️",
                        AssemblyDefinitionAnalyzer.ValidationSeverity.Info => "ℹ️",
                        _ => "❓"
                    };
                    
                    report += $"### {severityIcon} {group.Key} Issues\n\n";
                    foreach (var issue in group.OrderBy(i => i.AssemblyName))
                    {
                        report += $"#### {issue.Title}\n";
                        report += $"**Assembly**: {issue.AssemblyName}\n";
                        report += $"**Description**: {issue.Description}\n";
                        if (!string.IsNullOrEmpty(issue.Recommendation))
                        {
                            report += $"**Recommendation**: {issue.Recommendation}\n";
                        }
                        report += "\n";
                    }
                }
            }
            
            // Footer
            report += "---\n";
            report += "*This report was automatically generated by the XR Bubble Library Assembly Definition Analyzer.*\n";
            
            return report;
        }
        
        /// <summary>
        /// Export analysis result to JSON format.
        /// </summary>
        public string ExportToJson(AssemblyDefinitionAnalyzer.AnalysisResult analysisResult)
        {
            return JsonConvert.SerializeObject(analysisResult, Formatting.Indented);
        }
        
        /// <summary>
        /// Get dependency chain between two assemblies.
        /// </summary>
        public List<string> GetDependencyChain(
            string fromAssembly, 
            string toAssembly, 
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies)
        {
            var visited = new HashSet<string>();
            var path = new List<string>();
            
            if (FindDependencyPath(fromAssembly, toAssembly, dependencies, visited, path))
            {
                return path;
            }
            
            return new List<string>(); // No path found
        }
        
        /// <summary>
        /// Recursively find a dependency path between two assemblies.
        /// </summary>
        private bool FindDependencyPath(
            string current, 
            string target, 
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies,
            HashSet<string> visited,
            List<string> path)
        {
            if (visited.Contains(current))
            {
                return false; // Avoid cycles
            }
            
            visited.Add(current);
            path.Add(current);
            
            if (current == target)
            {
                return true; // Found target
            }
            
            var directDependencies = dependencies
                .Where(d => d.FromAssembly == current && d.Type == AssemblyDefinitionAnalyzer.DependencyType.Direct)
                .ToList();
            
            foreach (var dependency in directDependencies)
            {
                if (FindDependencyPath(dependency.ToAssembly, target, dependencies, visited, path))
                {
                    return true;
                }
            }
            
            path.RemoveAt(path.Count - 1);
            return false;
        }
        
        /// <summary>
        /// Calculate implementation score for a module.
        /// </summary>
        public float CalculateImplementationScore(
            AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo assembly,
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies)
        {
            // This would use the same logic as the static analyzer
            // For now, return a basic calculation
            float score = assembly.SourceFiles.Count * 1.0f;
            score += assembly.TestFiles.Count * 0.5f;
            score += assembly.EditorFiles.Count * 0.3f;
            
            var incomingReferences = dependencies.Count(d => d.ToAssembly == assembly.Name);
            score += incomingReferences * 0.2f;
            
            return score;
        }
    }
}