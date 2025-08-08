using System.Collections.Generic;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for assembly definition analysis with dependency injection support.
    /// Provides comprehensive analysis of assembly definitions, dependencies, and module states.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public interface IAssemblyDefinitionAnalyzer
    {
        /// <summary>
        /// Perform comprehensive analysis of all assembly definitions in the project.
        /// </summary>
        /// <param name="assemblyPaths">List of assembly definition file paths</param>
        /// <returns>Comprehensive analysis result</returns>
        AssemblyDefinitionAnalyzer.AnalysisResult AnalyzeAssemblyDefinitions(List<string> assemblyPaths);
        
        /// <summary>
        /// Perform comprehensive analysis asynchronously.
        /// </summary>
        /// <param name="assemblyPaths">List of assembly definition file paths</param>
        /// <returns>Task containing comprehensive analysis result</returns>
        Task<AssemblyDefinitionAnalyzer.AnalysisResult> AnalyzeAssemblyDefinitionsAsync(List<string> assemblyPaths);
        
        /// <summary>
        /// Analyze a single assembly definition file.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly definition file</param>
        /// <returns>Assembly definition information</returns>
        AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo AnalyzeSingleAssembly(string assemblyPath);
        
        /// <summary>
        /// Build dependency graph for a set of assemblies.
        /// </summary>
        /// <param name="assemblies">List of assembly definition information</param>
        /// <returns>List of dependency relationships</returns>
        List<AssemblyDefinitionAnalyzer.DependencyRelationship> BuildDependencyGraph(
            List<AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo> assemblies);
        
        /// <summary>
        /// Detect circular dependencies in the assembly dependency graph.
        /// </summary>
        /// <param name="dependencies">List of all dependencies</param>
        /// <returns>List of detected circular dependencies</returns>
        List<AssemblyDefinitionAnalyzer.CircularDependency> DetectCircularDependencies(
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies);
        
        /// <summary>
        /// Determine module states based on comprehensive analysis.
        /// </summary>
        /// <param name="assemblies">List of assembly definition information</param>
        /// <param name="dependencies">List of dependency relationships</param>
        /// <returns>Dictionary mapping assembly names to their states</returns>
        Dictionary<string, ModuleState> DetermineModuleStates(
            List<AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo> assemblies,
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies);
        
        /// <summary>
        /// Validate assembly configuration and detect issues.
        /// </summary>
        /// <param name="assemblies">List of assembly definition information</param>
        /// <param name="dependencies">List of dependency relationships</param>
        /// <param name="circularDependencies">List of circular dependencies</param>
        /// <returns>List of validation issues</returns>
        List<AssemblyDefinitionAnalyzer.ValidationIssue> ValidateConfiguration(
            List<AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo> assemblies,
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies,
            List<AssemblyDefinitionAnalyzer.CircularDependency> circularDependencies);
        
        /// <summary>
        /// Generate a comprehensive analysis report.
        /// </summary>
        /// <param name="analysisResult">Analysis result to generate report for</param>
        /// <returns>Formatted analysis report</returns>
        string GenerateAnalysisReport(AssemblyDefinitionAnalyzer.AnalysisResult analysisResult);
        
        /// <summary>
        /// Export analysis result to JSON format.
        /// </summary>
        /// <param name="analysisResult">Analysis result to export</param>
        /// <returns>JSON representation of the analysis result</returns>
        string ExportToJson(AssemblyDefinitionAnalyzer.AnalysisResult analysisResult);
        
        /// <summary>
        /// Get dependency chain between two assemblies.
        /// </summary>
        /// <param name="fromAssembly">Source assembly name</param>
        /// <param name="toAssembly">Target assembly name</param>
        /// <param name="dependencies">List of all dependencies</param>
        /// <returns>List of assemblies in the dependency chain, or empty if no path exists</returns>
        List<string> GetDependencyChain(
            string fromAssembly, 
            string toAssembly, 
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies);
        
        /// <summary>
        /// Calculate implementation score for a module.
        /// </summary>
        /// <param name="assembly">Assembly definition information</param>
        /// <param name="dependencies">List of dependency relationships</param>
        /// <returns>Implementation score (higher = more implemented)</returns>
        float CalculateImplementationScore(
            AssemblyDefinitionAnalyzer.AssemblyDefinitionInfo assembly,
            List<AssemblyDefinitionAnalyzer.DependencyRelationship> dependencies);
    }
}