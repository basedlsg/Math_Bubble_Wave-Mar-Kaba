using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for collecting and managing supporting evidence for development claims.
    /// Implements evidence-based development practices to ensure all performance and 
    /// functionality claims are backed by verifiable data.
    /// </summary>
    public interface IEvidenceCollector
    {
        /// <summary>
        /// Collects all available evidence for the current system state.
        /// </summary>
        /// <returns>Collection of evidence files with metadata</returns>
        List<EvidenceFile> CollectAllEvidence();
        
        /// <summary>
        /// Collects evidence asynchronously for better performance.
        /// </summary>
        /// <returns>Task containing collection of evidence files</returns>
        Task<List<EvidenceFile>> CollectAllEvidenceAsync();
        
        /// <summary>
        /// Collects evidence for a specific module or component.
        /// </summary>
        /// <param name="moduleName">Name of the module to collect evidence for</param>
        /// <returns>Evidence files specific to the module</returns>
        List<EvidenceFile> CollectModuleEvidence(string moduleName);
        
        /// <summary>
        /// Validates the integrity of collected evidence files.
        /// </summary>
        /// <param name="evidenceFiles">Evidence files to validate</param>
        /// <returns>Validation results for each evidence file</returns>
        List<EvidenceValidationResult> ValidateEvidence(List<EvidenceFile> evidenceFiles);
        
        /// <summary>
        /// Generates hash for evidence file to ensure integrity.
        /// </summary>
        /// <param name="filePath">Path to the evidence file</param>
        /// <returns>SHA-256 hash of the file</returns>
        string GenerateEvidenceHash(string filePath);
        
        /// <summary>
        /// Archives evidence files for long-term storage and audit.
        /// </summary>
        /// <param name="evidenceFiles">Evidence files to archive</param>
        /// <returns>Archive operation result</returns>
        EvidenceArchiveResult ArchiveEvidence(List<EvidenceFile> evidenceFiles);
        
        /// <summary>
        /// Retrieves evidence files by hash for verification.
        /// </summary>
        /// <param name="hash">Hash of the evidence file to retrieve</param>
        /// <returns>Evidence file matching the hash, or null if not found</returns>
        EvidenceFile GetEvidenceByHash(string hash);
        
        /// <summary>
        /// Searches for evidence files matching specific criteria.
        /// </summary>
        /// <param name="criteria">Search criteria for evidence files</param>
        /// <returns>Evidence files matching the criteria</returns>
        List<EvidenceFile> SearchEvidence(EvidenceSearchCriteria criteria);
    }
}