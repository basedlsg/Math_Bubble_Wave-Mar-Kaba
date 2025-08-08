using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Enhanced assembly definition analyzer with sophisticated dependency analysis,
    /// circular dependency detection, and comprehensive module status determination.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public static class AssemblyDefinitionAnalyzer
    {
        /// <summary>
        /// Comprehensive analysis result containing all assembly information and relationships.
        /// </summary>
        public class AnalysisResult
        {
            public List<AssemblyDefinitionInfo> Assemblies { get; set; } = new List<AssemblyDefinitionInfo>();
            public List<DependencyRelationship> Dependencies { get; set; } = new List<DependencyRelationship>();
            public List<CircularDependency> CircularDependencies { get; set; } = new List<CircularDependency>();
            public Dictionary<string, ModuleState> ModuleStates { get; set; } = new Dictionary<string, ModuleState>();
            public List<ValidationIssue> ValidationIssues { get; set; } = new List<ValidationIssue>();
            public DateTime AnalyzedAt { get; set; }
            public string AnalysisVersion { get; set; }
        }
        
        /// <summary>
        /// Detailed information about a single assembly definition.
        /// </summary>
        public class AssemblyDefinitionInfo
        {
            public string Name { get; set; }
            public string FilePath { get; set; }
            public string RootNamespace { get; set; }
            public List<string> References { get; set; } = new List<string>();
            public List<string> IncludePlatforms { get; set; } = new List<string>();
            public List<string> ExcludePlatforms { get; set; } = new List<string>();
            public List<string> DefineConstraints { get; set; } = new List<string>();
            public List<VersionDefine> VersionDefines { get; set; } = new List<VersionDefine>();
            public bool AllowUnsafeCode { get; set; }
            public bool OverrideReferences { get; set; }
            public bool AutoReferenced { get; set; }
            public bool NoEngineReferences { get; set; }
            public List<string> PrecompiledReferences { get; set; } = new List<string>();
            public FileInfo FileInfo { get; set; }
            public DirectoryInfo ModuleDirectory { get; set; }
            public List<FileInfo> SourceFiles { get; set; } = new List<FileInfo>();
            public List<FileInfo> TestFiles { get; set; } = new List<FileInfo>();
            public List<FileInfo> EditorFiles { get; set; } = new List<FileInfo>();
        }
        
        /// <summary>
        /// Represents a dependency relationship between two assemblies.
        /// </summary>
        public class DependencyRelationship
        {
            public string FromAssembly { get; set; }
            public string ToAssembly { get; set; }
            public DependencyType Type { get; set; }
            public bool IsOptional { get; set; }
            public string Reason { get; set; }
        }
        
        /// <summary>
        /// Represents a circular dependency between assemblies.
        /// </summary>
        public class CircularDependency
        {
            public List<string> AssemblyChain { get; set; } = new List<string>();
            public string Description { get; set; }
            public CircularDependencySeverity Severity { get; set; }
        }
        
        /// <summary>
        /// Version define information from assembly definitions.
        /// </summary>
        public class VersionDefine
        {
            public string Name { get; set; }
            public string Expression { get; set; }
            public string Define { get; set; }
        }
        
        /// <summary>
        /// Validation issue found during analysis.
        /// </summary>
        public class ValidationIssue
        {
            public string AssemblyName { get; set; }
            public ValidationSeverity Severity { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Recommendation { get; set; }
        }
        
        /// <summary>
        /// Types of dependencies between assemblies.
        /// </summary>
        public enum DependencyType
        {
            Direct,
            Indirect,
            Conditional,
            Platform,
            Version
        }
        
        /// <summary>
        /// Severity levels for circular dependencies.
        /// </summary>
        public enum CircularDependencySeverity
        {
            Low,
            Medium,
            High,
            Critical
        }
        
        /// <summary>
        /// Severity levels for validation issues.
        /// </summary>
        public enum ValidationSeverity
        {
            Info,
            Warning,
            Error,
            Critical
        }
    }
} 
       /// <summary>
        /// Perform comprehensive analysis of all assembly definitions in the project.
        /// </summary>
        /// <param name="assemblyPaths">List of assembly definition file paths</param>
        /// <returns>Comprehensive analysis result</returns>
        public static AnalysisResult AnalyzeAssemblyDefinitions(List<string> assemblyPaths)
        {
            Debug.Log($"[AssemblyDefinitionAnalyzer] Starting comprehensive analysis of {assemblyPaths.Count} assemblies...");
            
            var result = new AnalysisResult
            {
                AnalyzedAt = DateTime.UtcNow,
                AnalysisVersion = "1.0.0"
            };
            
            try
            {
                // Phase 1: Parse all assembly definitions
                result.Assemblies = ParseAssemblyDefinitions(assemblyPaths);
                Debug.Log($"[AssemblyDefinitionAnalyzer] Parsed {result.Assemblies.Count} assembly definitions");
                
                // Phase 2: Analyze file system structure
                AnalyzeFileSystemStructure(result.Assemblies);
                Debug.Log($"[AssemblyDefinitionAnalyzer] Analyzed file system structure");
                
                // Phase 3: Build dependency graph
                result.Dependencies = BuildDependencyGraph(result.Assemblies);
                Debug.Log($"[AssemblyDefinitionAnalyzer] Built dependency graph with {result.Dependencies.Count} relationships");
                
                // Phase 4: Detect circular dependencies
                result.CircularDependencies = DetectCircularDependencies(result.Dependencies);
                if (result.CircularDependencies.Count > 0)
                {
                    Debug.LogWarning($"[AssemblyDefinitionAnalyzer] Found {result.CircularDependencies.Count} circular dependencies");
                }
                
                // Phase 5: Determine module states
                result.ModuleStates = DetermineModuleStates(result.Assemblies, result.Dependencies);
                Debug.Log($"[AssemblyDefinitionAnalyzer] Determined states for {result.ModuleStates.Count} modules");
                
                // Phase 6: Validate configuration
                result.ValidationIssues = ValidateConfiguration(result.Assemblies, result.Dependencies, result.CircularDependencies);
                if (result.ValidationIssues.Count > 0)
                {
                    Debug.LogWarning($"[AssemblyDefinitionAnalyzer] Found {result.ValidationIssues.Count} validation issues");
                }
                
                Debug.Log($"[AssemblyDefinitionAnalyzer] Analysis completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssemblyDefinitionAnalyzer] Analysis failed: {ex.Message}");
                result.ValidationIssues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Critical,
                    Title = "Analysis Failed",
                    Description = $"Assembly definition analysis failed: {ex.Message}",
                    Recommendation = "Check assembly definition files for syntax errors and ensure all files are accessible"
                });
                return result;
            }
        }
        
        /// <summary>
        /// Parse all assembly definition files and extract their information.
        /// </summary>
        /// <param name="assemblyPaths">List of assembly definition file paths</param>
        /// <returns>List of parsed assembly definition information</returns>
        private static List<AssemblyDefinitionInfo> ParseAssemblyDefinitions(List<string> assemblyPaths)
        {
            var assemblies = new List<AssemblyDefinitionInfo>();
            
            foreach (var assemblyPath in assemblyPaths)
            {
                try
                {
                    var assemblyInfo = ParseAssemblyDefinition(assemblyPath);
                    if (assemblyInfo != null)
                    {
                        assemblies.Add(assemblyInfo);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AssemblyDefinitionAnalyzer] Failed to parse {assemblyPath}: {ex.Message}");
                }
            }
            
            return assemblies;
        }
        
        /// <summary>
        /// Parse a single assembly definition file.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly definition file</param>
        /// <returns>Parsed assembly definition information</returns>
        private static AssemblyDefinitionInfo ParseAssemblyDefinition(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                Debug.LogWarning($"[AssemblyDefinitionAnalyzer] Assembly definition file not found: {assemblyPath}");
                return null;
            }
            
            try
            {
                var content = File.ReadAllText(assemblyPath);
                var rawAssembly = JsonConvert.DeserializeObject<RawAssemblyDefinition>(content);
                
                if (rawAssembly == null)
                {
                    Debug.LogWarning($"[AssemblyDefinitionAnalyzer] Failed to deserialize assembly definition: {assemblyPath}");
                    return null;
                }
                
                var assemblyInfo = new AssemblyDefinitionInfo
                {
                    Name = rawAssembly.name ?? Path.GetFileNameWithoutExtension(assemblyPath),
                    FilePath = assemblyPath,
                    RootNamespace = rawAssembly.rootNamespace,
                    References = rawAssembly.references?.ToList() ?? new List<string>(),
                    IncludePlatforms = rawAssembly.includePlatforms?.ToList() ?? new List<string>(),
                    ExcludePlatforms = rawAssembly.excludePlatforms?.ToList() ?? new List<string>(),
                    DefineConstraints = rawAssembly.defineConstraints?.ToList() ?? new List<string>(),
                    AllowUnsafeCode = rawAssembly.allowUnsafeCode,
                    OverrideReferences = rawAssembly.overrideReferences,
                    AutoReferenced = rawAssembly.autoReferenced,
                    NoEngineReferences = rawAssembly.noEngineReferences,
                    PrecompiledReferences = rawAssembly.precompiledReferences?.ToList() ?? new List<string>(),
                    FileInfo = new FileInfo(assemblyPath),
                    ModuleDirectory = new DirectoryInfo(Path.GetDirectoryName(assemblyPath))
                };
                
                // Parse version defines if present
                if (rawAssembly.versionDefines != null)
                {
                    foreach (var versionDefine in rawAssembly.versionDefines)
                    {
                        assemblyInfo.VersionDefines.Add(new VersionDefine
                        {
                            Name = versionDefine.name,
                            Expression = versionDefine.expression,
                            Define = versionDefine.define
                        });
                    }
                }
                
                return assemblyInfo;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssemblyDefinitionAnalyzer] Error parsing assembly definition {assemblyPath}: {ex.Message}");
                return null;
            }
        } 
       /// <summary>
        /// Analyze the file system structure for each assembly to determine implementation completeness.
        /// </summary>
        /// <param name="assemblies">List of assembly definition information</param>
        private static void AnalyzeFileSystemStructure(List<AssemblyDefinitionInfo> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                try
                {
                    if (assembly.ModuleDirectory.Exists)
                    {
                        // Find all C# files in the module directory
                        var allCsFiles = assembly.ModuleDirectory
                            .GetFiles("*.cs", SearchOption.AllDirectories)
                            .Where(f => !f.Name.EndsWith(".meta"))
                            .ToList();
                        
                        // Categorize files
                        foreach (var file in allCsFiles)
                        {
                            if (IsTestFile(file))
                            {
                                assembly.TestFiles.Add(file);
                            }
                            else if (IsEditorFile(file))
                            {
                                assembly.EditorFiles.Add(file);
                            }
                            else
                            {
                                assembly.SourceFiles.Add(file);
                            }
                        }
                        
                        Debug.Log($"[AssemblyDefinitionAnalyzer] {assembly.Name}: {assembly.SourceFiles.Count} source, " +
                                 $"{assembly.TestFiles.Count} test, {assembly.EditorFiles.Count} editor files");
                    }
                    else
                    {
                        Debug.LogWarning($"[AssemblyDefinitionAnalyzer] Module directory not found for {assembly.Name}: {assembly.ModuleDirectory.FullName}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AssemblyDefinitionAnalyzer] Error analyzing file structure for {assembly.Name}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Build a comprehensive dependency graph showing all relationships between assemblies.
        /// </summary>
        /// <param name="assemblies">List of assembly definition information</param>
        /// <returns>List of dependency relationships</returns>
        private static List<DependencyRelationship> BuildDependencyGraph(List<AssemblyDefinitionInfo> assemblies)
        {
            var dependencies = new List<DependencyRelationship>();
            var assemblyLookup = assemblies.ToDictionary(a => a.Name, a => a);
            
            foreach (var assembly in assemblies)
            {
                // Direct references
                foreach (var reference in assembly.References)
                {
                    dependencies.Add(new DependencyRelationship
                    {
                        FromAssembly = assembly.Name,
                        ToAssembly = reference,
                        Type = DependencyType.Direct,
                        IsOptional = false,
                        Reason = "Direct assembly reference"
                    });
                }
                
                // Platform-specific dependencies
                if (assembly.IncludePlatforms.Count > 0 || assembly.ExcludePlatforms.Count > 0)
                {
                    foreach (var reference in assembly.References)
                    {
                        dependencies.Add(new DependencyRelationship
                        {
                            FromAssembly = assembly.Name,
                            ToAssembly = reference,
                            Type = DependencyType.Platform,
                            IsOptional = true,
                            Reason = $"Platform-specific dependency (Include: {string.Join(",", assembly.IncludePlatforms)}, Exclude: {string.Join(",", assembly.ExcludePlatforms)})"
                        });
                    }
                }
                
                // Conditional dependencies based on define constraints
                if (assembly.DefineConstraints.Count > 0)
                {
                    foreach (var reference in assembly.References)
                    {
                        dependencies.Add(new DependencyRelationship
                        {
                            FromAssembly = assembly.Name,
                            ToAssembly = reference,
                            Type = DependencyType.Conditional,
                            IsOptional = true,
                            Reason = $"Conditional dependency (Requires: {string.Join(",", assembly.DefineConstraints)})"
                        });
                    }
                }
                
                // Version-specific dependencies
                foreach (var versionDefine in assembly.VersionDefines)
                {
                    dependencies.Add(new DependencyRelationship
                    {
                        FromAssembly = assembly.Name,
                        ToAssembly = versionDefine.Name,
                        Type = DependencyType.Version,
                        IsOptional = true,
                        Reason = $"Version dependency ({versionDefine.Expression} -> {versionDefine.Define})"
                    });
                }
            }
            
            // Build indirect dependencies
            var indirectDependencies = BuildIndirectDependencies(dependencies, assemblyLookup);
            dependencies.AddRange(indirectDependencies);
            
            return dependencies;
        }
        
        /// <summary>
        /// Build indirect dependencies by following the dependency chain.
        /// </summary>
        /// <param name="directDependencies">List of direct dependencies</param>
        /// <param name="assemblyLookup">Dictionary for assembly lookup</param>
        /// <returns>List of indirect dependencies</returns>
        private static List<DependencyRelationship> BuildIndirectDependencies(
            List<DependencyRelationship> directDependencies, 
            Dictionary<string, AssemblyDefinitionInfo> assemblyLookup)
        {
            var indirectDependencies = new List<DependencyRelationship>();
            var visited = new HashSet<string>();
            
            foreach (var assembly in assemblyLookup.Keys)
            {
                var indirectRefs = FindIndirectDependencies(assembly, directDependencies, visited, new HashSet<string>());
                indirectDependencies.AddRange(indirectRefs);
            }
            
            return indirectDependencies;
        }
        
        /// <summary>
        /// Recursively find indirect dependencies for an assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to analyze</param>
        /// <param name="dependencies">List of all dependencies</param>
        /// <param name="globalVisited">Global set of visited assemblies</param>
        /// <param name="currentPath">Current dependency path to detect cycles</param>
        /// <returns>List of indirect dependencies</returns>
        private static List<DependencyRelationship> FindIndirectDependencies(
            string assemblyName, 
            List<DependencyRelationship> dependencies, 
            HashSet<string> globalVisited,
            HashSet<string> currentPath)
        {
            var indirectDeps = new List<DependencyRelationship>();
            
            if (currentPath.Contains(assemblyName) || globalVisited.Contains(assemblyName))
            {
                return indirectDeps; // Avoid infinite recursion
            }
            
            currentPath.Add(assemblyName);
            
            var directRefs = dependencies
                .Where(d => d.FromAssembly == assemblyName && d.Type == DependencyType.Direct)
                .ToList();
            
            foreach (var directRef in directRefs)
            {
                var transitiveRefs = dependencies
                    .Where(d => d.FromAssembly == directRef.ToAssembly && d.Type == DependencyType.Direct)
                    .ToList();
                
                foreach (var transitiveRef in transitiveRefs)
                {
                    indirectDeps.Add(new DependencyRelationship
                    {
                        FromAssembly = assemblyName,
                        ToAssembly = transitiveRef.ToAssembly,
                        Type = DependencyType.Indirect,
                        IsOptional = false,
                        Reason = $"Indirect dependency via {directRef.ToAssembly}"
                    });
                }
                
                // Recurse for deeper dependencies
                var deeperDeps = FindIndirectDependencies(directRef.ToAssembly, dependencies, globalVisited, currentPath);
                foreach (var deeperDep in deeperDeps)
                {
                    indirectDeps.Add(new DependencyRelationship
                    {
                        FromAssembly = assemblyName,
                        ToAssembly = deeperDep.ToAssembly,
                        Type = DependencyType.Indirect,
                        IsOptional = deeperDep.IsOptional,
                        Reason = $"Deep indirect dependency via {directRef.ToAssembly} -> {deeperDep.Reason}"
                    });
                }
            }
            
            currentPath.Remove(assemblyName);
            globalVisited.Add(assemblyName);
            
            return indirectDeps;
        }       
 /// <summary>
        /// Detect circular dependencies in the assembly dependency graph.
        /// </summary>
        /// <param name="dependencies">List of all dependencies</param>
        /// <returns>List of detected circular dependencies</returns>
        private static List<CircularDependency> DetectCircularDependencies(List<DependencyRelationship> dependencies)
        {
            var circularDependencies = new List<CircularDependency>();
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            var currentPath = new List<string>();
            
            // Get all unique assembly names
            var allAssemblies = dependencies
                .SelectMany(d => new[] { d.FromAssembly, d.ToAssembly })
                .Distinct()
                .ToList();
            
            foreach (var assembly in allAssemblies)
            {
                if (!visited.Contains(assembly))
                {
                    var cycles = DetectCyclesFromAssembly(assembly, dependencies, visited, recursionStack, currentPath);
                    circularDependencies.AddRange(cycles);
                }
            }
            
            return circularDependencies;
        }
        
        /// <summary>
        /// Detect cycles starting from a specific assembly using depth-first search.
        /// </summary>
        /// <param name="assembly">Starting assembly</param>
        /// <param name="dependencies">List of all dependencies</param>
        /// <param name="visited">Set of globally visited assemblies</param>
        /// <param name="recursionStack">Current recursion stack</param>
        /// <param name="currentPath">Current path being explored</param>
        /// <returns>List of circular dependencies found</returns>
        private static List<CircularDependency> DetectCyclesFromAssembly(
            string assembly,
            List<DependencyRelationship> dependencies,
            HashSet<string> visited,
            HashSet<string> recursionStack,
            List<string> currentPath)
        {
            var cycles = new List<CircularDependency>();
            
            visited.Add(assembly);
            recursionStack.Add(assembly);
            currentPath.Add(assembly);
            
            var directDependencies = dependencies
                .Where(d => d.FromAssembly == assembly && d.Type == DependencyType.Direct)
                .ToList();
            
            foreach (var dependency in directDependencies)
            {
                var targetAssembly = dependency.ToAssembly;
                
                if (recursionStack.Contains(targetAssembly))
                {
                    // Found a cycle
                    var cycleStartIndex = currentPath.IndexOf(targetAssembly);
                    var cycleChain = currentPath.Skip(cycleStartIndex).Concat(new[] { targetAssembly }).ToList();
                    
                    var circularDep = new CircularDependency
                    {
                        AssemblyChain = cycleChain,
                        Description = $"Circular dependency detected: {string.Join(" -> ", cycleChain)}",
                        Severity = DetermineCycleSeverity(cycleChain, dependencies)
                    };
                    
                    cycles.Add(circularDep);
                }
                else if (!visited.Contains(targetAssembly))
                {
                    var nestedCycles = DetectCyclesFromAssembly(targetAssembly, dependencies, visited, recursionStack, currentPath);
                    cycles.AddRange(nestedCycles);
                }
            }
            
            recursionStack.Remove(assembly);
            currentPath.RemoveAt(currentPath.Count - 1);
            
            return cycles;
        }
        
        /// <summary>
        /// Determine the severity of a circular dependency based on the assemblies involved.
        /// </summary>
        /// <param name="cycleChain">Chain of assemblies in the cycle</param>
        /// <param name="dependencies">List of all dependencies</param>
        /// <returns>Severity level of the circular dependency</returns>
        private static CircularDependencySeverity DetermineCycleSeverity(List<string> cycleChain, List<DependencyRelationship> dependencies)
        {
            // Critical if involves core assemblies
            if (cycleChain.Any(a => a.Contains("Core")))
            {
                return CircularDependencySeverity.Critical;
            }
            
            // High if cycle is short (direct circular reference)
            if (cycleChain.Count <= 3)
            {
                return CircularDependencySeverity.High;
            }
            
            // Medium if involves conditional dependencies
            var hasConditionalDeps = false;
            for (int i = 0; i < cycleChain.Count - 1; i++)
            {
                var from = cycleChain[i];
                var to = cycleChain[i + 1];
                if (dependencies.Any(d => d.FromAssembly == from && d.ToAssembly == to && d.Type == DependencyType.Conditional))
                {
                    hasConditionalDeps = true;
                    break;
                }
            }
            
            if (hasConditionalDeps)
            {
                return CircularDependencySeverity.Medium;
            }
            
            // Default to low severity
            return CircularDependencySeverity.Low;
        }
        
        /// <summary>
        /// Determine the implementation state of each module based on comprehensive analysis.
        /// </summary>
        /// <param name="assemblies">List of assembly definition information</param>
        /// <param name="dependencies">List of dependency relationships</param>
        /// <returns>Dictionary mapping assembly names to their states</returns>
        private static Dictionary<string, ModuleState> DetermineModuleStates(
            List<AssemblyDefinitionInfo> assemblies, 
            List<DependencyRelationship> dependencies)
        {
            var moduleStates = new Dictionary<string, ModuleState>();
            
            foreach (var assembly in assemblies)
            {
                var state = DetermineModuleState(assembly, dependencies);
                moduleStates[assembly.Name] = state;
                
                Debug.Log($"[AssemblyDefinitionAnalyzer] {assembly.Name}: {state}");
            }
            
            return moduleStates;
        }
        
        /// <summary>
        /// Determine the implementation state of a single module using enhanced criteria.
        /// </summary>
        /// <param name="assembly">Assembly definition information</param>
        /// <param name="dependencies">List of dependency relationships</param>
        /// <returns>Module implementation state</returns>
        private static ModuleState DetermineModuleState(AssemblyDefinitionInfo assembly, List<DependencyRelationship> dependencies)
        {
            // Check if module is disabled by compiler flags
            if (IsModuleDisabledByFlags(assembly))
            {
                return ModuleState.Disabled;
            }
            
            // Check if module has substantial implementation
            var implementationScore = CalculateImplementationScore(assembly, dependencies);
            var threshold = GetImplementationThreshold(assembly.Name);
            
            if (implementationScore >= threshold)
            {
                return ModuleState.Implemented;
            }
            
            // Check if module has any meaningful content
            if (assembly.SourceFiles.Count > 0 || assembly.TestFiles.Count > 0)
            {
                return ModuleState.Conceptual;
            }
            
            // Module exists but has no content
            return ModuleState.Conceptual;
        }
        
        /// <summary>
        /// Calculate a comprehensive implementation score for a module.
        /// </summary>
        /// <param name="assembly">Assembly definition information</param>
        /// <param name="dependencies">List of dependency relationships</param>
        /// <returns>Implementation score (higher = more implemented)</returns>
        private static float CalculateImplementationScore(AssemblyDefinitionInfo assembly, List<DependencyRelationship> dependencies)
        {
            float score = 0f;
            
            // Base score from source files
            score += assembly.SourceFiles.Count * 1.0f;
            
            // Bonus for having tests
            score += assembly.TestFiles.Count * 0.5f;
            
            // Bonus for having editor integration
            score += assembly.EditorFiles.Count * 0.3f;
            
            // Bonus for being referenced by other modules
            var incomingReferences = dependencies.Count(d => d.ToAssembly == assembly.Name && d.Type == DependencyType.Direct);
            score += incomingReferences * 0.2f;
            
            // Penalty for having many unresolved dependencies
            var unresolvedDeps = assembly.References.Count(r => !dependencies.Any(d => d.FromAssembly == assembly.Name && d.ToAssembly == r));
            score -= unresolvedDeps * 0.1f;
            
            // Bonus for having a clear namespace structure
            if (!string.IsNullOrEmpty(assembly.RootNamespace))
            {
                score += 0.5f;
            }
            
            // Bonus for having version defines (indicates maturity)
            score += assembly.VersionDefines.Count * 0.1f;
            
            return Math.Max(0f, score);
        }     
   /// <summary>
        /// Check if a module is disabled by compiler flags.
        /// </summary>
        /// <param name="assembly">Assembly definition information</param>
        /// <returns>True if module is disabled by flags</returns>
        private static bool IsModuleDisabledByFlags(AssemblyDefinitionInfo assembly)
        {
            if (assembly.DefineConstraints.Count == 0)
            {
                return false;
            }
            
            foreach (var constraint in assembly.DefineConstraints)
            {
                var feature = MapConstraintToFeature(constraint);
                if (feature.HasValue && !CompilerFlags.IsFeatureEnabled(feature.Value))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Map a define constraint to an experimental feature.
        /// </summary>
        /// <param name="constraint">Define constraint string</param>
        /// <returns>Corresponding experimental feature, if any</returns>
        private static ExperimentalFeature? MapConstraintToFeature(string constraint)
        {
            return constraint switch
            {
                "EXP_AI" => ExperimentalFeature.AI_INTEGRATION,
                "EXP_VOICE" => ExperimentalFeature.VOICE_PROCESSING,
                "EXP_ADVANCED_WAVE" => ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS,
                "EXP_CLOUD_INFERENCE" => ExperimentalFeature.CLOUD_INFERENCE,
                "EXP_ON_DEVICE_ML" => ExperimentalFeature.ON_DEVICE_ML,
                _ => null
            };
        }
        
        /// <summary>
        /// Get the implementation threshold for a given assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly</param>
        /// <returns>Minimum score required to be considered implemented</returns>
        private static float GetImplementationThreshold(string assemblyName)
        {
            return assemblyName switch
            {
                "XRBubbleLibrary.Core" => 8.0f, // Core module should have substantial implementation
                "XRBubbleLibrary.Mathematics" => 5.0f, // Math module needs several files
                "XRBubbleLibrary.AI" => 3.0f, // AI module can be smaller but functional
                "XRBubbleLibrary.Voice" => 3.0f, // Voice module can be smaller but functional
                "XRBubbleLibrary.Integration" => 4.0f, // Integration module should have moderate implementation
                _ => 2.0f // Default threshold
            };
        }
        
        /// <summary>
        /// Validate the overall configuration and detect potential issues.
        /// </summary>
        /// <param name="assemblies">List of assembly definition information</param>
        /// <param name="dependencies">List of dependency relationships</param>
        /// <param name="circularDependencies">List of circular dependencies</param>
        /// <returns>List of validation issues</returns>
        private static List<ValidationIssue> ValidateConfiguration(
            List<AssemblyDefinitionInfo> assemblies,
            List<DependencyRelationship> dependencies,
            List<CircularDependency> circularDependencies)
        {
            var issues = new List<ValidationIssue>();
            
            // Validate circular dependencies
            foreach (var circularDep in circularDependencies)
            {
                var severity = circularDep.Severity switch
                {
                    CircularDependencySeverity.Critical => ValidationSeverity.Critical,
                    CircularDependencySeverity.High => ValidationSeverity.Error,
                    CircularDependencySeverity.Medium => ValidationSeverity.Warning,
                    CircularDependencySeverity.Low => ValidationSeverity.Info,
                    _ => ValidationSeverity.Warning
                };
                
                issues.Add(new ValidationIssue
                {
                    AssemblyName = circularDep.AssemblyChain.FirstOrDefault(),
                    Severity = severity,
                    Title = "Circular Dependency Detected",
                    Description = circularDep.Description,
                    Recommendation = "Refactor assemblies to remove circular dependencies by extracting common interfaces or restructuring dependencies"
                });
            }
            
            // Validate missing dependencies
            foreach (var assembly in assemblies)
            {
                foreach (var reference in assembly.References)
                {
                    if (!assemblies.Any(a => a.Name == reference) && !IsUnityBuiltInAssembly(reference))
                    {
                        issues.Add(new ValidationIssue
                        {
                            AssemblyName = assembly.Name,
                            Severity = ValidationSeverity.Warning,
                            Title = "Missing Assembly Reference",
                            Description = $"Assembly {assembly.Name} references {reference} which was not found in the project",
                            Recommendation = "Ensure the referenced assembly exists or remove the reference if it's no longer needed"
                        });
                    }
                }
            }
            
            // Validate define constraints
            foreach (var assembly in assemblies)
            {
                foreach (var constraint in assembly.DefineConstraints)
                {
                    var feature = MapConstraintToFeature(constraint);
                    if (!feature.HasValue && !IsUnityBuiltInDefine(constraint))
                    {
                        issues.Add(new ValidationIssue
                        {
                            AssemblyName = assembly.Name,
                            Severity = ValidationSeverity.Info,
                            Title = "Unknown Define Constraint",
                            Description = $"Assembly {assembly.Name} uses unknown define constraint: {constraint}",
                            Recommendation = "Verify that the define constraint is correct and properly configured in the project"
                        });
                    }
                }
            }
            
            // Validate platform constraints
            foreach (var assembly in assemblies)
            {
                if (assembly.IncludePlatforms.Count > 0 && assembly.ExcludePlatforms.Count > 0)
                {
                    issues.Add(new ValidationIssue
                    {
                        AssemblyName = assembly.Name,
                        Severity = ValidationSeverity.Warning,
                        Title = "Conflicting Platform Constraints",
                        Description = $"Assembly {assembly.Name} has both include and exclude platform constraints",
                        Recommendation = "Use either include or exclude platform constraints, not both"
                    });
                }
            }
            
            // Validate unsafe code usage
            foreach (var assembly in assemblies.Where(a => a.AllowUnsafeCode))
            {
                issues.Add(new ValidationIssue
                {
                    AssemblyName = assembly.Name,
                    Severity = ValidationSeverity.Info,
                    Title = "Unsafe Code Enabled",
                    Description = $"Assembly {assembly.Name} allows unsafe code",
                    Recommendation = "Ensure unsafe code is necessary and properly reviewed for security implications"
                });
            }
            
            return issues;
        }
        
        /// <summary>
        /// Check if an assembly reference is a Unity built-in assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly</param>
        /// <returns>True if it's a Unity built-in assembly</returns>
        private static bool IsUnityBuiltInAssembly(string assemblyName)
        {
            var unityAssemblies = new[]
            {
                "Unity.Mathematics",
                "Unity.Collections",
                "Unity.Jobs",
                "Unity.Burst",
                "Unity.Entities",
                "Unity.Rendering.Hybrid",
                "Unity.Physics",
                "Unity.NetCode",
                "Unity.InputSystem",
                "Unity.XR.Management",
                "Unity.XR.OpenXR",
                "UnityEngine",
                "UnityEditor"
            };
            
            return unityAssemblies.Any(ua => assemblyName.StartsWith(ua));
        }
        
        /// <summary>
        /// Check if a define constraint is a Unity built-in define.
        /// </summary>
        /// <param name="define">Define constraint</param>
        /// <returns>True if it's a Unity built-in define</returns>
        private static bool IsUnityBuiltInDefine(string define)
        {
            var unityDefines = new[]
            {
                "UNITY_EDITOR",
                "UNITY_STANDALONE",
                "UNITY_ANDROID",
                "UNITY_IOS",
                "UNITY_WEBGL",
                "DEVELOPMENT_BUILD",
                "DEBUG",
                "ENABLE_BURST_AOT",
                "ENABLE_UNITY_COLLECTIONS_CHECKS"
            };
            
            return unityDefines.Contains(define);
        }
        
        /// <summary>
        /// Check if a file is a test file based on its path and name.
        /// </summary>
        /// <param name="file">File information</param>
        /// <returns>True if the file is a test file</returns>
        private static bool IsTestFile(FileInfo file)
        {
            return file.Name.Contains("Test") || 
                   file.Name.Contains("Tests") || 
                   file.DirectoryName.Contains("Tests") || 
                   file.DirectoryName.Contains("Test");
        }
        
        /// <summary>
        /// Check if a file is an editor-only file based on its path.
        /// </summary>
        /// <param name="file">File information</param>
        /// <returns>True if the file is editor-only</returns>
        private static bool IsEditorFile(FileInfo file)
        {
            return file.DirectoryName.Contains("Editor");
        }
        
        /// <summary>
        /// Raw assembly definition structure for JSON deserialization.
        /// </summary>
        private class RawAssemblyDefinition
        {
            public string name { get; set; }
            public string rootNamespace { get; set; }
            public string[] references { get; set; }
            public string[] includePlatforms { get; set; }
            public string[] excludePlatforms { get; set; }
            public bool allowUnsafeCode { get; set; }
            public bool overrideReferences { get; set; }
            public string[] precompiledReferences { get; set; }
            public bool autoReferenced { get; set; }
            public string[] defineConstraints { get; set; }
            public RawVersionDefine[] versionDefines { get; set; }
            public bool noEngineReferences { get; set; }
        }
        
        /// <summary>
        /// Raw version define structure for JSON deserialization.
        /// </summary>
        private class RawVersionDefine
        {
            public string name { get; set; }
            public string expression { get; set; }
            public string define { get; set; }
        }
    }
}