using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Represents a single evidence file with metadata and validation information.
    /// </summary>
    [Serializable]
    public class EvidenceFile
    {
        /// <summary>
        /// Unique identifier for the evidence file.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Full path to the evidence file.
        /// </summary>
        public string FilePath { get; set; }
        
        /// <summary>
        /// Type of evidence (e.g., "performance_log", "screenshot", "test_result").
        /// </summary>
        public EvidenceType Type { get; set; }
        
        /// <summary>
        /// Module or component this evidence relates to.
        /// </summary>
        public string ModuleName { get; set; }
        
        /// <summary>
        /// SHA-256 hash of the file for integrity verification.
        /// </summary>
        public string Hash { get; set; }
        
        /// <summary>
        /// When the evidence was collected.
        /// </summary>
        public DateTime CollectedAt { get; set; }
        
        /// <summary>
        /// Size of the evidence file in bytes.
        /// </summary>
        public long FileSizeBytes { get; set; }
        
        /// <summary>
        /// Description of what this evidence demonstrates.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Tags for categorizing and searching evidence.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();
        
        /// <summary>
        /// Metadata specific to the evidence type.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Whether this evidence file is still valid and accessible.
        /// </summary>
        public bool IsValid { get; set; } = true;
        
        /// <summary>
        /// Last validation timestamp.
        /// </summary>
        public DateTime LastValidated { get; set; }
    }
    
    /// <summary>
    /// Types of evidence that can be collected.
    /// </summary>
    public enum EvidenceType
    {
        /// <summary>
        /// Performance profiling data and logs.
        /// </summary>
        PerformanceLog,
        
        /// <summary>
        /// Screenshots of functionality or UI.
        /// </summary>
        Screenshot,
        
        /// <summary>
        /// Unit test results and coverage reports.
        /// </summary>
        TestResult,
        
        /// <summary>
        /// Build logs and compilation output.
        /// </summary>
        BuildLog,
        
        /// <summary>
        /// Configuration files and settings.
        /// </summary>
        Configuration,
        
        /// <summary>
        /// Video recordings of functionality.
        /// </summary>
        VideoRecording,
        
        /// <summary>
        /// Profiler data from Unity or external tools.
        /// </summary>
        ProfilerData,
        
        /// <summary>
        /// Hardware metrics and system information.
        /// </summary>
        HardwareMetrics,
        
        /// <summary>
        /// Documentation and reports.
        /// </summary>
        Documentation,
        
        /// <summary>
        /// Other types of evidence.
        /// </summary>
        Other
    }
    
    /// <summary>
    /// Result of evidence validation operation.
    /// </summary>
    [Serializable]
    public class EvidenceValidationResult
    {
        /// <summary>
        /// The evidence file that was validated.
        /// </summary>
        public EvidenceFile EvidenceFile { get; set; }
        
        /// <summary>
        /// Whether the validation passed.
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Validation error messages, if any.
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new List<string>();
        
        /// <summary>
        /// When the validation was performed.
        /// </summary>
        public DateTime ValidatedAt { get; set; }
        
        /// <summary>
        /// Hash verification result.
        /// </summary>
        public bool HashMatches { get; set; }
        
        /// <summary>
        /// File accessibility check result.
        /// </summary>
        public bool FileAccessible { get; set; }
        
        /// <summary>
        /// File size verification result.
        /// </summary>
        public bool SizeMatches { get; set; }
    }
    
    /// <summary>
    /// Result of evidence archiving operation.
    /// </summary>
    [Serializable]
    public class EvidenceArchiveResult
    {
        /// <summary>
        /// Whether the archiving operation succeeded.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Path to the created archive.
        /// </summary>
        public string ArchivePath { get; set; }
        
        /// <summary>
        /// Number of files successfully archived.
        /// </summary>
        public int FilesArchived { get; set; }
        
        /// <summary>
        /// Total size of the archive in bytes.
        /// </summary>
        public long ArchiveSizeBytes { get; set; }
        
        /// <summary>
        /// Any errors that occurred during archiving.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// When the archive was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
    
    /// <summary>
    /// Criteria for searching evidence files.
    /// </summary>
    [Serializable]
    public class EvidenceSearchCriteria
    {
        /// <summary>
        /// Filter by evidence type.
        /// </summary>
        public EvidenceType? Type { get; set; }
        
        /// <summary>
        /// Filter by module name.
        /// </summary>
        public string ModuleName { get; set; }
        
        /// <summary>
        /// Filter by tags (any of these tags).
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();
        
        /// <summary>
        /// Filter by date range - start date.
        /// </summary>
        public DateTime? FromDate { get; set; }
        
        /// <summary>
        /// Filter by date range - end date.
        /// </summary>
        public DateTime? ToDate { get; set; }
        
        /// <summary>
        /// Filter by description containing this text.
        /// </summary>
        public string DescriptionContains { get; set; }
        
        /// <summary>
        /// Only return valid evidence files.
        /// </summary>
        public bool OnlyValid { get; set; } = true;
        
        /// <summary>
        /// Maximum number of results to return.
        /// </summary>
        public int MaxResults { get; set; } = 100;
    }
}