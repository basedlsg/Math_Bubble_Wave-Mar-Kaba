using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Analyzes assembly definitions and determines the implementation status of each module.
    /// Uses reflection and file system analysis to determine module states.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public static class ModuleStatusAnalyzer
    {
        /// <summary>
        /// Analyze all assembly definitions and determine their current status.
        /// </summary>
        /// <param name="assemblyPaths">List of assembly definition file paths</param>
        /// <returns>List of module status information</returns>
        public static List<ModuleStatus> AnalyzeModules(List<string> assemblyPaths)
        {
            var modules = new List<ModuleStatus>();
            
            foreach (var assemblyPath in assemblyPaths)
            {
                try
                {
                    var moduleStatus = AnalyzeModule(assemblyPath);
                    if (moduleStatus != null)
                    {
                        modules.Add(moduleStatus);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ModuleStatusAnalyzer] Failed to analyze module at {assemblyPath}: {ex.Message}");
                }
            }
            
            return modules;
        }
        
        /// <summary>
        /// Analyze a single assembly definition and determine its status.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly definition file</param>
        /// <returns>Module status information</returns>
        public static ModuleStatus AnalyzeModule(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                Debug.LogWarning($"[ModuleStatusAnalyzer] Assembly definition file not found: {assemblyPath}");
                return null;
            }
            
            try
            {
                // Read and parse the assembly definition
                var assemblyContent = File.ReadAllText(assemblyPath);
                var assemblyDef = JsonConvert.DeserializeObject<AssemblyDefinition>(assemblyContent);
                
                if (assemblyDef == null)
                {
                    Debug.LogWarning($"[ModuleStatusAnalyzer] Failed to parse assembly definition: {assemblyPath}");
                    return null;
                }
                
                // Create module status
                var moduleStatus = new ModuleStatus
                {
                    ModuleName = GetModuleDisplayName(assemblyDef.name),
                    AssemblyName = assemblyDef.name,
                    AssemblyPath = assemblyPath,
                    Dependencies = assemblyDef.references?.ToList() ?? new List<string>(),
                    DefineConstraints = assemblyDef.defineConstraints?.ToList() ?? new List<string>(),
                    AutoReferenced = assemblyDef.autoReferenced,
                    LastValidated = DateTime.UtcNow,
                    Description = GetModuleDescription(assemblyDef.name)
                };
                
                // Determine module state
                moduleStatus.State = DetermineModuleState(assemblyDef, assemblyPath);
                
                // Add validation notes
                moduleStatus.ValidationNotes = GenerateValidationNotes(assemblyDef, moduleStatus.State);
                
                return moduleStatus;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModuleStatusAnalyzer] Error analyzing module {assemblyPath}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Determine the implementation state of a module based on its assembly definition and file system analysis.
        /// </summary>
        /// <param name="assemblyDef">Parsed assembly definition</param>
        /// <param name="assemblyPath">Path to the assembly definition file</param>
        /// <returns>Module state</returns>
        private static ModuleState DetermineModuleState(AssemblyDefinition assemblyDef, string assemblyPath)
        {
            // Check if module is disabled by compiler flags
            if (IsModuleDisabledByFlags(assemblyDef))
            {
                return ModuleState.Disabled;
            }
            
            // Check if module has substantial implementation
            var moduleDirectory = Path.GetDirectoryName(assemblyPath);
            if (HasSubstantialImplementation(moduleDirectory, assemblyDef.name))
            {
                return ModuleState.Implemented;
            }
            
            // Default to conceptual if it exists but isn't fully implemented
            return ModuleState.Conceptual;
        }
        
        /// <summary>
        /// Check if a module is disabled by compiler flags.
        /// </summary>
        /// <param name="assemblyDef">Assembly definition to check</param>
        /// <returns>True if module is disabled by flags</returns>
        private static bool IsModuleDisabledByFlags(AssemblyDefinition assemblyDef)
        {
            if (assemblyDef.defineConstraints == null || assemblyDef.defineConstraints.Length == 0)
            {
                return false;
            }
            
            // Check each define constraint
            foreach (var constraint in assemblyDef.defineConstraints)
            {
                // Map constraint to experimental feature
                var feature = MapConstraintToFeature(constraint);
                if (feature.HasValue)
                {
                    // Check if the feature is disabled
                    if (!CompilerFlags.IsFeatureEnabled(feature.Value))
                    {
                        return true;
                    }
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
        /// Check if a module directory has substantial implementation.
        /// </summary>
        /// <param name="moduleDirectory">Directory containing the module</param>
        /// <param name="assemblyName">Name of the assembly</param>
        /// <returns>True if module has substantial implementation</returns>
        private static bool HasSubstantialImplementation(string moduleDirectory, string assemblyName)
        {
            if (!Directory.Exists(moduleDirectory))
            {
                return false;
            }
            
            // Count C# files (excluding meta files)
            var csFiles = Directory.GetFiles(moduleDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".meta"))
                .ToList();
            
            // Consider substantial if there are multiple implementation files
            var implementationFiles = csFiles.Where(f => !IsTestFile(f) && !IsEditorFile(f)).ToList();
            
            // Different thresholds based on module type
            var threshold = GetImplementationThreshold(assemblyName);
            
            return implementationFiles.Count >= threshold;
        }
        
        /// <summary>
        /// Get the implementation threshold for a given assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly</param>
        /// <returns>Minimum number of files required to be considered implemented</returns>
        private static int GetImplementationThreshold(string assemblyName)
        {
            return assemblyName switch
            {
                "XRBubbleLibrary.Core" => 5, // Core module should have substantial implementation
                "XRBubbleLibrary.Mathematics" => 3, // Math module needs several files
                "XRBubbleLibrary.AI" => 2, // AI module can be smaller but functional
                "XRBubbleLibrary.Voice" => 2, // Voice module can be smaller but functional
                _ => 2 // Default threshold
            };
        }
        
        /// <summary>
        /// Check if a file is a test file.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if file is a test file</returns>
        private static bool IsTestFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return fileName.Contains("Test") || fileName.Contains("Tests") || 
                   filePath.Contains("Tests") || filePath.Contains("Test");
        }
        
        /// <summary>
        /// Check if a file is an editor-only file.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if file is editor-only</returns>
        private static bool IsEditorFile(string filePath)
        {
            return filePath.Contains("Editor");
        }
        
        /// <summary>
        /// Get a human-readable display name for a module.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <returns>Display name</returns>
        private static string GetModuleDisplayName(string assemblyName)
        {
            return assemblyName switch
            {
                "XRBubbleLibrary.Core" => "Core System",
                "XRBubbleLibrary.Mathematics" => "Wave Mathematics",
                "XRBubbleLibrary.AI" => "AI Integration",
                "XRBubbleLibrary.Voice" => "Voice Processing",
                "XRBubbleLibrary.Integration" => "System Integration",
                "XRBubbleLibrary.WaveMatrix" => "Wave Matrix",
                "XRBubbleLibrary.WordBubbles" => "Word Bubbles",
                _ => assemblyName.Replace("XRBubbleLibrary.", "")
            };
        }
        
        /// <summary>
        /// Get a description of what a module provides.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <returns>Module description</returns>
        private static string GetModuleDescription(string assemblyName)
        {
            return assemblyName switch
            {
                "XRBubbleLibrary.Core" => "Core functionality including compiler flags, feature gates, and base interfaces",
                "XRBubbleLibrary.Mathematics" => "Wave mathematics, pattern generation, and cymatics visualization",
                "XRBubbleLibrary.AI" => "AI-enhanced bubble behavior and machine learning integration",
                "XRBubbleLibrary.Voice" => "Voice recognition, speech processing, and audio interaction",
                "XRBubbleLibrary.Integration" => "System integration and cross-module coordination",
                "XRBubbleLibrary.WaveMatrix" => "Wave matrix positioning and breathing animation systems",
                "XRBubbleLibrary.WordBubbles" => "Text rendering and word bubble visualization",
                _ => $"Module providing {assemblyName.Replace("XRBubbleLibrary.", "").ToLower()} functionality"
            };
        }
        
        /// <summary>
        /// Generate validation notes for a module based on its state and configuration.
        /// </summary>
        /// <param name="assemblyDef">Assembly definition</param>
        /// <param name="state">Determined module state</param>
        /// <returns>Validation notes</returns>
        private static string GenerateValidationNotes(AssemblyDefinition assemblyDef, ModuleState state)
        {
            var notes = new List<string>();
            
            switch (state)
            {
                case ModuleState.Implemented:
                    notes.Add("Module has substantial implementation and is functional");
                    break;
                    
                case ModuleState.Disabled:
                    notes.Add("Module is disabled by compiler flags");
                    if (assemblyDef.defineConstraints != null)
                    {
                        notes.Add($"Required flags: {string.Join(", ", assemblyDef.defineConstraints)}");
                    }
                    break;
                    
                case ModuleState.Conceptual:
                    notes.Add("Module exists but lacks substantial implementation");
                    break;
            }
            
            // Add dependency notes
            if (assemblyDef.references != null && assemblyDef.references.Length > 0)
            {
                notes.Add($"Dependencies: {string.Join(", ", assemblyDef.references)}");
            }
            
            // Add auto-reference note
            if (assemblyDef.autoReferenced)
            {
                notes.Add("Automatically referenced by Unity");
            }
            else
            {
                notes.Add("Must be explicitly referenced");
            }
            
            return string.Join("; ", notes);
        }
        
        /// <summary>
        /// Data structure representing a Unity assembly definition file.
        /// </summary>
        private class AssemblyDefinition
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
            public object[] versionDefines { get; set; }
            public bool noEngineReferences { get; set; }
        }
    }
}