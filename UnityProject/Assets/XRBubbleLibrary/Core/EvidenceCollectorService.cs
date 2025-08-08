using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Service class that provides high-level evidence collection and management functionality.
    /// Integrates with the DevStateGenerator to provide evidence-based development documentation.
    /// </summary>
    public class EvidenceCollectorService
    {
        private readonly IEvidenceCollector _evidenceCollector;
        private readonly string _reportOutputPath;
        
        /// <summary>
        /// Initializes a new instance of the EvidenceCollectorService.
        /// </summary>
        public EvidenceCollectorService() : this(new EvidenceCollector())
        {
        }
        
        /// <summary>
        /// Initializes a new instance with a custom evidence collector.
        /// </summary>
        /// <param name="evidenceCollector">Custom evidence collector implementation</param>
        public EvidenceCollectorService(IEvidenceCollector evidenceCollector)
        {
            _evidenceCollector = evidenceCollector ?? throw new ArgumentNullException(nameof(evidenceCollector));
            _reportOutputPath = Path.Combine(Application.dataPath, "..", "Evidence", "Reports");
            
            Directory.CreateDirectory(_reportOutputPath);
        }
        
        /// <summary>
        /// Generates a comprehensive evidence report for all modules.
        /// </summary>
        /// <returns>Path to the generated evidence report</returns>
        public string GenerateEvidenceReport()
        {
            try
            {
                var allEvidence = _evidenceCollector.CollectAllEvidence();
                var validationResults = _evidenceCollector.ValidateEvidence(allEvidence);
                
                var reportContent = GenerateEvidenceReportContent(allEvidence, validationResults);
                
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var reportPath = Path.Combine(_reportOutputPath, $"evidence_report_{timestamp}.md");
                
                File.WriteAllText(reportPath, reportContent);
                
                Debug.Log($"[EvidenceCollectorService] Generated evidence report: {reportPath}");
                return reportPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollectorService] Failed to generate evidence report: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Generates an evidence report asynchronously.
        /// </summary>
        /// <returns>Task containing the path to the generated evidence report</returns>
        public async Task<string> GenerateEvidenceReportAsync()
        {
            return await Task.Run(() => GenerateEvidenceReport());
        }
        
        /// <summary>
        /// Generates evidence report for a specific module.
        /// </summary>
        /// <param name="moduleName">Name of the module to generate report for</param>
        /// <returns>Path to the generated module evidence report</returns>
        public string GenerateModuleEvidenceReport(string moduleName)
        {
            try
            {
                var moduleEvidence = _evidenceCollector.CollectModuleEvidence(moduleName);
                var validationResults = _evidenceCollector.ValidateEvidence(moduleEvidence);
                
                var reportContent = GenerateModuleEvidenceReportContent(moduleName, moduleEvidence, validationResults);
                
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var reportPath = Path.Combine(_reportOutputPath, $"evidence_report_{moduleName}_{timestamp}.md");
                
                File.WriteAllText(reportPath, reportContent);
                
                Debug.Log($"[EvidenceCollectorService] Generated module evidence report: {reportPath}");
                return reportPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollectorService] Failed to generate module evidence report: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Validates all evidence and returns a summary of validation results.
        /// </summary>
        /// <returns>Evidence validation summary</returns>
        public EvidenceValidationSummary ValidateAllEvidence()
        {
            var allEvidence = _evidenceCollector.CollectAllEvidence();
            var validationResults = _evidenceCollector.ValidateEvidence(allEvidence);
            
            return new EvidenceValidationSummary
            {
                TotalEvidenceFiles = allEvidence.Count,
                ValidFiles = validationResults.Count(r => r.IsValid),
                InvalidFiles = validationResults.Count(r => !r.IsValid),
                ValidationResults = validationResults,
                ValidatedAt = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Archives all current evidence for long-term storage.
        /// </summary>
        /// <returns>Archive operation result</returns>
        public EvidenceArchiveResult ArchiveAllEvidence()
        {
            var allEvidence = _evidenceCollector.CollectAllEvidence();
            return _evidenceCollector.ArchiveEvidence(allEvidence);
        }
        
        /// <summary>
        /// Searches for evidence matching the specified criteria.
        /// </summary>
        /// <param name="criteria">Search criteria</param>
        /// <returns>List of matching evidence files</returns>
        public List<EvidenceFile> SearchEvidence(EvidenceSearchCriteria criteria)
        {
            return _evidenceCollector.SearchEvidence(criteria);
        }
        
        /// <summary>
        /// Gets evidence statistics grouped by module.
        /// </summary>
        /// <returns>Dictionary of module names to evidence statistics</returns>
        public Dictionary<string, EvidenceStatistics> GetEvidenceStatisticsByModule()
        {
            var allEvidence = _evidenceCollector.CollectAllEvidence();
            var validationResults = _evidenceCollector.ValidateEvidence(allEvidence);
            
            var statistics = new Dictionary<string, EvidenceStatistics>();
            
            var moduleGroups = allEvidence.GroupBy(e => e.ModuleName);
            
            foreach (var moduleGroup in moduleGroups)
            {
                var moduleName = moduleGroup.Key;
                var moduleEvidence = moduleGroup.ToList();
                var moduleValidation = validationResults.Where(r => r.EvidenceFile.ModuleName == moduleName).ToList();
                
                statistics[moduleName] = new EvidenceStatistics
                {
                    ModuleName = moduleName,
                    TotalFiles = moduleEvidence.Count,
                    ValidFiles = moduleValidation.Count(r => r.IsValid),
                    InvalidFiles = moduleValidation.Count(r => !r.IsValid),
                    TotalSizeBytes = moduleEvidence.Sum(e => e.FileSizeBytes),
                    EvidenceTypes = moduleEvidence.GroupBy(e => e.Type)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    LastCollected = moduleEvidence.Max(e => e.CollectedAt)
                };
            }
            
            return statistics;
        }
        
        #region Private Methods
        
        private string GenerateEvidenceReportContent(List<EvidenceFile> evidenceFiles, List<EvidenceValidationResult> validationResults)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("# Evidence Collection Report");
            report.AppendLine();
            report.AppendLine($"**Generated**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine($"**Total Evidence Files**: {evidenceFiles.Count}");
            report.AppendLine($"**Valid Files**: {validationResults.Count(r => r.IsValid)}");
            report.AppendLine($"**Invalid Files**: {validationResults.Count(r => !r.IsValid)}");
            report.AppendLine();
            
            // Summary by module
            report.AppendLine("## Evidence Summary by Module");
            report.AppendLine();
            
            var moduleGroups = evidenceFiles.GroupBy(e => e.ModuleName);
            foreach (var moduleGroup in moduleGroups.OrderBy(g => g.Key))
            {
                var moduleName = moduleGroup.Key;
                var moduleEvidence = moduleGroup.ToList();
                var moduleValidation = validationResults.Where(r => r.EvidenceFile.ModuleName == moduleName);
                
                report.AppendLine($"### {moduleName}");
                report.AppendLine($"- **Total Files**: {moduleEvidence.Count}");
                report.AppendLine($"- **Valid Files**: {moduleValidation.Count(r => r.IsValid)}");
                report.AppendLine($"- **Total Size**: {FormatBytes(moduleEvidence.Sum(e => e.FileSizeBytes))}");
                report.AppendLine();
                
                // Evidence types breakdown
                var typeGroups = moduleEvidence.GroupBy(e => e.Type);
                foreach (var typeGroup in typeGroups)
                {
                    report.AppendLine($"  - **{typeGroup.Key}**: {typeGroup.Count()} files");
                }
                report.AppendLine();
            }
            
            // Detailed evidence list
            report.AppendLine("## Detailed Evidence Files");
            report.AppendLine();
            
            foreach (var moduleGroup in moduleGroups.OrderBy(g => g.Key))
            {
                report.AppendLine($"### {moduleGroup.Key} Evidence Files");
                report.AppendLine();
                
                foreach (var evidence in moduleGroup.OrderBy(e => e.Type).ThenBy(e => e.CollectedAt))
                {
                    var validation = validationResults.FirstOrDefault(r => r.EvidenceFile.Id == evidence.Id);
                    var status = validation?.IsValid == true ? "✅ Valid" : "❌ Invalid";
                    
                    report.AppendLine($"#### {evidence.Description}");
                    report.AppendLine($"- **Status**: {status}");
                    report.AppendLine($"- **Type**: {evidence.Type}");
                    report.AppendLine($"- **File**: `{Path.GetFileName(evidence.FilePath)}`");
                    report.AppendLine($"- **Size**: {FormatBytes(evidence.FileSizeBytes)}");
                    report.AppendLine($"- **Collected**: {evidence.CollectedAt:yyyy-MM-dd HH:mm:ss} UTC");
                    report.AppendLine($"- **Hash**: `{evidence.Hash.Substring(0, 16)}...`");
                    
                    if (validation != null && !validation.IsValid)
                    {
                        report.AppendLine($"- **Validation Errors**: {string.Join(", ", validation.ValidationErrors)}");
                    }
                    
                    report.AppendLine();
                }
            }
            
            return report.ToString();
        }
        
        private string GenerateModuleEvidenceReportContent(string moduleName, List<EvidenceFile> evidenceFiles, List<EvidenceValidationResult> validationResults)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine($"# Evidence Report - {moduleName} Module");
            report.AppendLine();
            report.AppendLine($"**Generated**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine($"**Module**: {moduleName}");
            report.AppendLine($"**Total Evidence Files**: {evidenceFiles.Count}");
            report.AppendLine($"**Valid Files**: {validationResults.Count(r => r.IsValid)}");
            report.AppendLine($"**Invalid Files**: {validationResults.Count(r => !r.IsValid)}");
            report.AppendLine();
            
            if (evidenceFiles.Any())
            {
                report.AppendLine("## Evidence Files");
                report.AppendLine();
                
                foreach (var evidence in evidenceFiles.OrderBy(e => e.Type).ThenBy(e => e.CollectedAt))
                {
                    var validation = validationResults.FirstOrDefault(r => r.EvidenceFile.Id == evidence.Id);
                    var status = validation?.IsValid == true ? "✅ Valid" : "❌ Invalid";
                    
                    report.AppendLine($"### {evidence.Description}");
                    report.AppendLine($"- **Status**: {status}");
                    report.AppendLine($"- **Type**: {evidence.Type}");
                    report.AppendLine($"- **File**: `{evidence.FilePath}`");
                    report.AppendLine($"- **Size**: {FormatBytes(evidence.FileSizeBytes)}");
                    report.AppendLine($"- **Collected**: {evidence.CollectedAt:yyyy-MM-dd HH:mm:ss} UTC");
                    report.AppendLine($"- **Hash**: `{evidence.Hash}`");
                    
                    if (validation != null && !validation.IsValid)
                    {
                        report.AppendLine($"- **Validation Errors**: {string.Join(", ", validation.ValidationErrors)}");
                    }
                    
                    report.AppendLine();
                }
            }
            else
            {
                report.AppendLine("## No Evidence Found");
                report.AppendLine();
                report.AppendLine($"No evidence files were found for the {moduleName} module.");
                report.AppendLine();
            }
            
            return report.ToString();
        }
        
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Summary of evidence validation results.
    /// </summary>
    [Serializable]
    public class EvidenceValidationSummary
    {
        public int TotalEvidenceFiles { get; set; }
        public int ValidFiles { get; set; }
        public int InvalidFiles { get; set; }
        public List<EvidenceValidationResult> ValidationResults { get; set; }
        public DateTime ValidatedAt { get; set; }
        
        public double ValidationSuccessRate => TotalEvidenceFiles > 0 ? (double)ValidFiles / TotalEvidenceFiles : 0.0;
    }
    
    /// <summary>
    /// Statistics for evidence files in a specific module.
    /// </summary>
    [Serializable]
    public class EvidenceStatistics
    {
        public string ModuleName { get; set; }
        public int TotalFiles { get; set; }
        public int ValidFiles { get; set; }
        public int InvalidFiles { get; set; }
        public long TotalSizeBytes { get; set; }
        public Dictionary<EvidenceType, int> EvidenceTypes { get; set; }
        public DateTime LastCollected { get; set; }
        
        public double ValidationRate => TotalFiles > 0 ? (double)ValidFiles / TotalFiles : 0.0;
    }
}