using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Comprehensive development state report containing all module information,
    /// performance metrics, and supporting evidence.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    [System.Serializable]
    public class DevStateReport
    {
        /// <summary>
        /// Timestamp when this report was generated.
        /// </summary>
        public DateTime GeneratedAt { get; set; }
        
        /// <summary>
        /// Build version information.
        /// </summary>
        public string BuildVersion { get; set; }
        
        /// <summary>
        /// Unity version used for this build.
        /// </summary>
        public string UnityVersion { get; set; }
        
        /// <summary>
        /// Current build configuration (Debug, Release, etc.).
        /// </summary>
        public string BuildConfiguration { get; set; }
        
        /// <summary>
        /// List of all modules and their current status.
        /// </summary>
        public List<ModuleStatus> Modules { get; set; } = new List<ModuleStatus>();
        
        /// <summary>
        /// Current performance metrics from the system.
        /// </summary>
        public PerformanceMetrics CurrentPerformance { get; set; }
        
        /// <summary>
        /// List of supporting evidence files for claims and features.
        /// </summary>
        public List<EvidenceFile> SupportingEvidence { get; set; } = new List<EvidenceFile>();
        
        /// <summary>
        /// Current compiler flag states.
        /// </summary>
        public Dictionary<ExperimentalFeature, bool> CompilerFlags { get; set; } = new Dictionary<ExperimentalFeature, bool>();
        
        /// <summary>
        /// Summary statistics about the current state.
        /// </summary>
        public DevStateSummary Summary { get; set; }
        
        /// <summary>
        /// Generate a markdown representation of this report.
        /// </summary>
        /// <returns>Formatted markdown string</returns>
        public string ToMarkdown()
        {
            var markdown = "# XR Bubble Library - Development State Report\n\n";
            
            // Header information
            markdown += $"**Generated**: {GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC\n";
            markdown += $"**Build Version**: {BuildVersion}\n";
            markdown += $"**Unity Version**: {UnityVersion}\n";
            markdown += $"**Build Configuration**: {BuildConfiguration}\n\n";
            
            // Summary section
            if (Summary != null)
            {
                markdown += "## Summary\n\n";
                markdown += $"- **Total Modules**: {Summary.TotalModules}\n";
                markdown += $"- **Implemented**: {Summary.ImplementedModules} ({Summary.ImplementedPercentage:F1}%)\n";
                markdown += $"- **Disabled**: {Summary.DisabledModules} ({Summary.DisabledPercentage:F1}%)\n";
                markdown += $"- **Conceptual**: {Summary.ConceptualModules} ({Summary.ConceptualPercentage:F1}%)\n\n";
            }
            
            // Compiler flags section
            markdown += "## Compiler Flags Status\n\n";
            foreach (var flag in CompilerFlags.OrderBy(f => f.Key.ToString()))
            {
                var status = flag.Value ? "✅ ENABLED" : "❌ DISABLED";
                markdown += $"- **{flag.Key}**: {status}\n";
            }
            markdown += "\n";
            
            // Modules section
            markdown += "## Module Status\n\n";
            markdown += "| Module | Status | Assembly | Dependencies | Evidence |\n";
            markdown += "|--------|--------|----------|--------------|----------|\n";
            
            foreach (var module in Modules.OrderBy(m => m.ModuleName))
            {
                var statusIcon = module.State switch
                {
                    ModuleState.Implemented => "✅",
                    ModuleState.Disabled => "❌",
                    ModuleState.Conceptual => "🔬",
                    _ => "❓"
                };
                
                var dependencies = string.Join(", ", module.Dependencies ?? new List<string>());
                var evidenceCount = module.Evidence?.Count ?? 0;
                var evidenceText = evidenceCount > 0 ? $"{evidenceCount} file(s)" : "None";
                
                markdown += $"| {module.ModuleName} | {statusIcon} {module.State} | {module.AssemblyName} | {dependencies} | {evidenceText} |\n";
            }
            markdown += "\n";
            
            // Performance section
            if (CurrentPerformance != null)
            {
                markdown += "## Performance Metrics\n\n";
                markdown += $"- **Average FPS**: {CurrentPerformance.AverageFPS:F1}\n";
                markdown += $"- **Frame Time**: {CurrentPerformance.AverageFrameTime:F2}ms\n";
                markdown += $"- **Memory Usage**: {CurrentPerformance.MemoryUsage / (1024 * 1024):F1} MB\n";
                markdown += $"- **CPU Usage**: {CurrentPerformance.CPUUsage:F1}%\n";
                
                if (CurrentPerformance.AdditionalMetrics != null && CurrentPerformance.AdditionalMetrics.Count > 0)
                {
                    markdown += "\n### Additional Metrics\n\n";
                    foreach (var metric in CurrentPerformance.AdditionalMetrics)
                    {
                        markdown += $"- **{metric.Key}**: {metric.Value}\n";
                    }
                }
                markdown += "\n";
            }
            
            // Evidence section
            if (SupportingEvidence != null && SupportingEvidence.Count > 0)
            {
                markdown += "## Supporting Evidence\n\n";
                
                var evidenceByType = SupportingEvidence.GroupBy(e => e.Type);
                foreach (var group in evidenceByType.OrderBy(g => g.Key.ToString()))
                {
                    markdown += $"### {group.Key}\n\n";
                    foreach (var evidence in group.OrderBy(e => e.FileName))
                    {
                        markdown += $"- **{evidence.FileName}**\n";
                        markdown += $"  - Path: `{evidence.FilePath}`\n";
                        markdown += $"  - Created: {evidence.CreatedAt:yyyy-MM-dd HH:mm:ss}\n";
                        markdown += $"  - Claim: {evidence.ClaimSupported}\n";
                        if (!string.IsNullOrEmpty(evidence.SHA256Hash))
                        {
                            markdown += $"  - Hash: `{evidence.SHA256Hash.Substring(0, 16)}...`\n";
                        }
                        markdown += "\n";
                    }
                }
            }
            
            // Footer
            markdown += "---\n";
            markdown += $"*This report was automatically generated by the XR Bubble Library Development State Generator.*\n";
            markdown += $"*For more information, see the [Development State Documentation System](UnityProject/Assets/XRBubbleLibrary/Core/README_DevStateGenerator.md).*\n";
            
            return markdown;
        }
        
        /// <summary>
        /// Generate a JSON representation of this report.
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
        
        /// <summary>
        /// Validate the internal consistency of this report.
        /// </summary>
        /// <returns>List of validation issues found</returns>
        public List<string> ValidateConsistency()
        {
            var issues = new List<string>();
            
            // Check that summary matches actual module counts
            if (Summary != null)
            {
                var actualImplemented = Modules.Count(m => m.State == ModuleState.Implemented);
                var actualDisabled = Modules.Count(m => m.State == ModuleState.Disabled);
                var actualConceptual = Modules.Count(m => m.State == ModuleState.Conceptual);
                
                if (Summary.ImplementedModules != actualImplemented)
                {
                    issues.Add($"Summary implemented count ({Summary.ImplementedModules}) doesn't match actual count ({actualImplemented})");
                }
                
                if (Summary.DisabledModules != actualDisabled)
                {
                    issues.Add($"Summary disabled count ({Summary.DisabledModules}) doesn't match actual count ({actualDisabled})");
                }
                
                if (Summary.ConceptualModules != actualConceptual)
                {
                    issues.Add($"Summary conceptual count ({Summary.ConceptualModules}) doesn't match actual count ({actualConceptual})");
                }
            }
            
            // Check for duplicate module names
            var moduleNames = Modules.Select(m => m.ModuleName).ToList();
            var duplicates = moduleNames.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key);
            foreach (var duplicate in duplicates)
            {
                issues.Add($"Duplicate module name found: {duplicate}");
            }
            
            // Check that evidence files reference valid claims
            foreach (var evidence in SupportingEvidence)
            {
                if (string.IsNullOrEmpty(evidence.ClaimSupported))
                {
                    issues.Add($"Evidence file {evidence.FileName} has no associated claim");
                }
            }
            
            return issues;
        }
    }
    
    /// <summary>
    /// Status information for a single module in the system.
    /// </summary>
    [System.Serializable]
    public class ModuleStatus
    {
        /// <summary>
        /// Name of the module.
        /// </summary>
        public string ModuleName { get; set; }
        
        /// <summary>
        /// Name of the assembly this module belongs to.
        /// </summary>
        public string AssemblyName { get; set; }
        
        /// <summary>
        /// Full path to the assembly definition file.
        /// </summary>
        public string AssemblyPath { get; set; }
        
        /// <summary>
        /// Current implementation state of the module.
        /// </summary>
        public ModuleState State { get; set; }
        
        /// <summary>
        /// List of other modules this module depends on.
        /// </summary>
        public List<string> Dependencies { get; set; } = new List<string>();
        
        /// <summary>
        /// Performance metrics specific to this module.
        /// </summary>
        public PerformanceMetrics Performance { get; set; }
        
        /// <summary>
        /// Supporting evidence files for this module.
        /// </summary>
        public List<EvidenceFile> Evidence { get; set; } = new List<EvidenceFile>();
        
        /// <summary>
        /// Timestamp when this module was last validated.
        /// </summary>
        public DateTime LastValidated { get; set; }
        
        /// <summary>
        /// Notes about the validation process or current state.
        /// </summary>
        public string ValidationNotes { get; set; }
        
        /// <summary>
        /// Compiler flags that affect this module.
        /// </summary>
        public List<string> DefineConstraints { get; set; } = new List<string>();
        
        /// <summary>
        /// Whether this module is automatically referenced by Unity.
        /// </summary>
        public bool AutoReferenced { get; set; }
        
        /// <summary>
        /// Description of what this module provides.
        /// </summary>
        public string Description { get; set; }
    }
    
    /// <summary>
    /// Enumeration of possible module implementation states.
    /// </summary>
    public enum ModuleState
    {
        /// <summary>
        /// Module is fully implemented and functional with supporting evidence.
        /// </summary>
        Implemented,
        
        /// <summary>
        /// Module is compiled out via compiler flags and not available.
        /// </summary>
        Disabled,
        
        /// <summary>
        /// Module exists but is not yet validated or fully implemented.
        /// </summary>
        Conceptual
    }
    
    /// <summary>
    /// Summary statistics about the development state.
    /// </summary>
    [System.Serializable]
    public class DevStateSummary
    {
        /// <summary>
        /// Total number of modules in the system.
        /// </summary>
        public int TotalModules { get; set; }
        
        /// <summary>
        /// Number of implemented modules.
        /// </summary>
        public int ImplementedModules { get; set; }
        
        /// <summary>
        /// Number of disabled modules.
        /// </summary>
        public int DisabledModules { get; set; }
        
        /// <summary>
        /// Number of conceptual modules.
        /// </summary>
        public int ConceptualModules { get; set; }
        
        /// <summary>
        /// Percentage of modules that are implemented.
        /// </summary>
        public float ImplementedPercentage => TotalModules > 0 ? (ImplementedModules * 100f) / TotalModules : 0f;
        
        /// <summary>
        /// Percentage of modules that are disabled.
        /// </summary>
        public float DisabledPercentage => TotalModules > 0 ? (DisabledModules * 100f) / TotalModules : 0f;
        
        /// <summary>
        /// Percentage of modules that are conceptual.
        /// </summary>
        public float ConceptualPercentage => TotalModules > 0 ? (ConceptualModules * 100f) / TotalModules : 0f;
    }
    
    /// <summary>
    /// Performance metrics data structure.
    /// </summary>
    [System.Serializable]
    public class PerformanceMetrics
    {
        public float AverageFPS { get; set; }
        public float MinimumFPS { get; set; }
        public float MaximumFPS { get; set; }
        public float AverageFrameTime { get; set; }
        public long MemoryUsage { get; set; }
        public float CPUUsage { get; set; }
        public float GPUUsage { get; set; }
        public float ThermalState { get; set; }
        public DateTime CapturedAt { get; set; }
        public string BuildVersion { get; set; }
        public Dictionary<string, object> AdditionalMetrics { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Evidence file information.
    /// </summary>
    [System.Serializable]
    public class EvidenceFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string SHA256Hash { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClaimSupported { get; set; }
        public EvidenceType Type { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Types of evidence that can be collected.
    /// </summary>
    public enum EvidenceType
    {
        PerformanceLog,
        Screenshot,
        VideoCapture,
        ProfilerData,
        TestResults,
        UserStudyData,
        BuildLog,
        ConfigurationFile
    }
}