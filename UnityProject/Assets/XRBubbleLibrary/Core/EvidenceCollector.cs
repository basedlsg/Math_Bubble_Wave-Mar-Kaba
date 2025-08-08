using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Collects and manages supporting evidence for development claims.
    /// Implements evidence-based development practices as specified in Requirement 10.
    /// </summary>
    public class EvidenceCollector : IEvidenceCollector
    {
        private readonly string _evidenceDirectory;
        private readonly string _archiveDirectory;
        private readonly List<EvidenceFile> _evidenceCache;
        
        /// <summary>
        /// Initializes a new instance of the EvidenceCollector.
        /// </summary>
        public EvidenceCollector()
        {
            _evidenceDirectory = Path.Combine(Application.dataPath, "..", "Evidence");
            _archiveDirectory = Path.Combine(_evidenceDirectory, "Archive");
            _evidenceCache = new List<EvidenceFile>();
            
            EnsureDirectoriesExist();
        }
        
        /// <summary>
        /// Initializes a new instance with custom directories.
        /// </summary>
        /// <param name="evidenceDirectory">Custom evidence directory path</param>
        public EvidenceCollector(string evidenceDirectory)
        {
            _evidenceDirectory = evidenceDirectory;
            _archiveDirectory = Path.Combine(_evidenceDirectory, "Archive");
            _evidenceCache = new List<EvidenceFile>();
            
            EnsureDirectoriesExist();
        }
        
        /// <summary>
        /// Collects all available evidence for the current system state.
        /// </summary>
        public List<EvidenceFile> CollectAllEvidence()
        {
            var evidenceFiles = new List<EvidenceFile>();
            
            try
            {
                // Collect performance logs
                evidenceFiles.AddRange(CollectPerformanceLogs());
                
                // Collect test results
                evidenceFiles.AddRange(CollectTestResults());
                
                // Collect build logs
                evidenceFiles.AddRange(CollectBuildLogs());
                
                // Collect configuration files
                evidenceFiles.AddRange(CollectConfigurationFiles());
                
                // Collect profiler data
                evidenceFiles.AddRange(CollectProfilerData());
                
                // Collect screenshots
                evidenceFiles.AddRange(CollectScreenshots());
                
                // Update cache
                _evidenceCache.Clear();
                _evidenceCache.AddRange(evidenceFiles);
                
                Debug.Log($"[EvidenceCollector] Collected {evidenceFiles.Count} evidence files");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollector] Error collecting evidence: {ex.Message}");
            }
            
            return evidenceFiles;
        }
        
        /// <summary>
        /// Collects evidence asynchronously for better performance.
        /// </summary>
        public async Task<List<EvidenceFile>> CollectAllEvidenceAsync()
        {
            return await Task.Run(() => CollectAllEvidence());
        }
        
        /// <summary>
        /// Collects evidence for a specific module or component.
        /// </summary>
        public List<EvidenceFile> CollectModuleEvidence(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                Debug.LogWarning("[EvidenceCollector] Module name is null or empty");
                return new List<EvidenceFile>();
            }
            
            var allEvidence = CollectAllEvidence();
            return allEvidence.Where(e => e.ModuleName == moduleName).ToList();
        }
        
        /// <summary>
        /// Validates the integrity of collected evidence files.
        /// </summary>
        public List<EvidenceValidationResult> ValidateEvidence(List<EvidenceFile> evidenceFiles)
        {
            var results = new List<EvidenceValidationResult>();
            
            foreach (var evidence in evidenceFiles)
            {
                var result = new EvidenceValidationResult
                {
                    EvidenceFile = evidence,
                    ValidatedAt = DateTime.UtcNow
                };
                
                try
                {
                    // Check file accessibility
                    result.FileAccessible = File.Exists(evidence.FilePath);
                    
                    if (result.FileAccessible)
                    {
                        // Verify file size
                        var fileInfo = new FileInfo(evidence.FilePath);
                        result.SizeMatches = fileInfo.Length == evidence.FileSizeBytes;
                        
                        // Verify hash
                        var currentHash = GenerateEvidenceHash(evidence.FilePath);
                        result.HashMatches = currentHash == evidence.Hash;
                        
                        // Overall validation
                        result.IsValid = result.SizeMatches && result.HashMatches;
                    }
                    else
                    {
                        result.ValidationErrors.Add("Evidence file not accessible");
                        result.IsValid = false;
                    }
                }
                catch (Exception ex)
                {
                    result.ValidationErrors.Add($"Validation error: {ex.Message}");
                    result.IsValid = false;
                }
                
                results.Add(result);
            }
            
            return results;
        }
        
        /// <summary>
        /// Generates SHA-256 hash for evidence file to ensure integrity.
        /// </summary>
        public string GenerateEvidenceHash(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Evidence file not found: {filePath}");
            }
            
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    return Convert.ToBase64String(hash);
                }
            }
        }
        
        /// <summary>
        /// Archives evidence files for long-term storage and audit.
        /// </summary>
        public EvidenceArchiveResult ArchiveEvidence(List<EvidenceFile> evidenceFiles)
        {
            var result = new EvidenceArchiveResult
            {
                CreatedAt = DateTime.UtcNow
            };
            
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var archiveName = $"evidence_archive_{timestamp}";
                var archivePath = Path.Combine(_archiveDirectory, archiveName);
                
                Directory.CreateDirectory(archivePath);
                
                foreach (var evidence in evidenceFiles)
                {
                    try
                    {
                        if (File.Exists(evidence.FilePath))
                        {
                            var fileName = Path.GetFileName(evidence.FilePath);
                            var destinationPath = Path.Combine(archivePath, $"{evidence.Id}_{fileName}");
                            
                            File.Copy(evidence.FilePath, destinationPath, true);
                            result.FilesArchived++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Failed to archive {evidence.FilePath}: {ex.Message}");
                    }
                }
                
                // Create archive metadata
                var metadataPath = Path.Combine(archivePath, "archive_metadata.json");
                var metadata = new
                {
                    CreatedAt = result.CreatedAt,
                    FilesArchived = result.FilesArchived,
                    EvidenceFiles = evidenceFiles
                };
                
                File.WriteAllText(metadataPath, JsonUtility.ToJson(metadata, true));
                
                // Calculate archive size
                var archiveInfo = new DirectoryInfo(archivePath);
                result.ArchiveSizeBytes = archiveInfo.GetFiles("*", SearchOption.AllDirectories)
                    .Sum(f => f.Length);
                
                result.ArchivePath = archivePath;
                result.Success = result.FilesArchived > 0;
                
                Debug.Log($"[EvidenceCollector] Archived {result.FilesArchived} files to {archivePath}");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Archive operation failed: {ex.Message}");
                result.Success = false;
                Debug.LogError($"[EvidenceCollector] Archive failed: {ex.Message}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Retrieves evidence files by hash for verification.
        /// </summary>
        public EvidenceFile GetEvidenceByHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return null;
            
            // Check cache first
            var cachedEvidence = _evidenceCache.FirstOrDefault(e => e.Hash == hash);
            if (cachedEvidence != null)
                return cachedEvidence;
            
            // Search all evidence if not in cache
            var allEvidence = CollectAllEvidence();
            return allEvidence.FirstOrDefault(e => e.Hash == hash);
        }
        
        /// <summary>
        /// Searches for evidence files matching specific criteria.
        /// </summary>
        public List<EvidenceFile> SearchEvidence(EvidenceSearchCriteria criteria)
        {
            var allEvidence = CollectAllEvidence();
            var query = allEvidence.AsQueryable();
            
            // Apply filters
            if (criteria.Type.HasValue)
                query = query.Where(e => e.Type == criteria.Type.Value);
            
            if (!string.IsNullOrEmpty(criteria.ModuleName))
                query = query.Where(e => e.ModuleName == criteria.ModuleName);
            
            if (criteria.Tags.Any())
                query = query.Where(e => e.Tags.Any(t => criteria.Tags.Contains(t)));
            
            if (criteria.FromDate.HasValue)
                query = query.Where(e => e.CollectedAt >= criteria.FromDate.Value);
            
            if (criteria.ToDate.HasValue)
                query = query.Where(e => e.CollectedAt <= criteria.ToDate.Value);
            
            if (!string.IsNullOrEmpty(criteria.DescriptionContains))
                query = query.Where(e => e.Description.Contains(criteria.DescriptionContains));
            
            if (criteria.OnlyValid)
                query = query.Where(e => e.IsValid);
            
            return query.Take(criteria.MaxResults).ToList();
        }
        
        #region Private Methods
        
        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(_evidenceDirectory);
            Directory.CreateDirectory(_archiveDirectory);
        }
        
        private List<EvidenceFile> CollectPerformanceLogs()
        {
            var evidenceFiles = new List<EvidenceFile>();
            var logsPath = Path.Combine(Application.persistentDataPath, "Logs");
            
            if (Directory.Exists(logsPath))
            {
                var logFiles = Directory.GetFiles(logsPath, "*.log", SearchOption.AllDirectories);
                
                foreach (var logFile in logFiles)
                {
                    evidenceFiles.Add(CreateEvidenceFile(logFile, EvidenceType.PerformanceLog, 
                        "Performance", "Performance profiling log"));
                }
            }
            
            return evidenceFiles;
        }
        
        private List<EvidenceFile> CollectTestResults()
        {
            var evidenceFiles = new List<EvidenceFile>();
            var testResultsPath = Path.Combine(Application.dataPath, "..", "TestResults");
            
            if (Directory.Exists(testResultsPath))
            {
                var testFiles = Directory.GetFiles(testResultsPath, "*.xml", SearchOption.AllDirectories);
                
                foreach (var testFile in testFiles)
                {
                    evidenceFiles.Add(CreateEvidenceFile(testFile, EvidenceType.TestResult, 
                        "Testing", "Unit test results"));
                }
            }
            
            return evidenceFiles;
        }
        
        private List<EvidenceFile> CollectBuildLogs()
        {
            var evidenceFiles = new List<EvidenceFile>();
            var buildLogsPath = Path.Combine(Application.dataPath, "..", "Logs");
            
            if (Directory.Exists(buildLogsPath))
            {
                var buildFiles = Directory.GetFiles(buildLogsPath, "*build*.log", SearchOption.AllDirectories);
                
                foreach (var buildFile in buildFiles)
                {
                    evidenceFiles.Add(CreateEvidenceFile(buildFile, EvidenceType.BuildLog, 
                        "Build", "Build compilation log"));
                }
            }
            
            return evidenceFiles;
        }
        
        private List<EvidenceFile> CollectConfigurationFiles()
        {
            var evidenceFiles = new List<EvidenceFile>();
            
            // Collect assembly definition files
            var asmdefFiles = Directory.GetFiles(Application.dataPath, "*.asmdef", SearchOption.AllDirectories);
            foreach (var asmdefFile in asmdefFiles)
            {
                evidenceFiles.Add(CreateEvidenceFile(asmdefFile, EvidenceType.Configuration, 
                    "Assembly", "Assembly definition configuration"));
            }
            
            // Collect project settings
            var projectSettingsPath = Path.Combine(Application.dataPath, "..", "ProjectSettings");
            if (Directory.Exists(projectSettingsPath))
            {
                var settingsFiles = Directory.GetFiles(projectSettingsPath, "*.asset", SearchOption.TopDirectoryOnly);
                foreach (var settingsFile in settingsFiles)
                {
                    evidenceFiles.Add(CreateEvidenceFile(settingsFile, EvidenceType.Configuration, 
                        "ProjectSettings", "Unity project configuration"));
                }
            }
            
            return evidenceFiles;
        }
        
        private List<EvidenceFile> CollectProfilerData()
        {
            var evidenceFiles = new List<EvidenceFile>();
            var profilerDataPath = Path.Combine(Application.persistentDataPath, "ProfilerData");
            
            if (Directory.Exists(profilerDataPath))
            {
                var profilerFiles = Directory.GetFiles(profilerDataPath, "*.data", SearchOption.AllDirectories);
                
                foreach (var profilerFile in profilerFiles)
                {
                    evidenceFiles.Add(CreateEvidenceFile(profilerFile, EvidenceType.ProfilerData, 
                        "Performance", "Unity profiler data"));
                }
            }
            
            return evidenceFiles;
        }
        
        private List<EvidenceFile> CollectScreenshots()
        {
            var evidenceFiles = new List<EvidenceFile>();
            var screenshotsPath = Path.Combine(_evidenceDirectory, "Screenshots");
            
            if (Directory.Exists(screenshotsPath))
            {
                var imageFiles = Directory.GetFiles(screenshotsPath, "*.png", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(screenshotsPath, "*.jpg", SearchOption.AllDirectories));
                
                foreach (var imageFile in imageFiles)
                {
                    evidenceFiles.Add(CreateEvidenceFile(imageFile, EvidenceType.Screenshot, 
                        "Visual", "Screenshot evidence"));
                }
            }
            
            return evidenceFiles;
        }
        
        private EvidenceFile CreateEvidenceFile(string filePath, EvidenceType type, string moduleName, string description)
        {
            var fileInfo = new FileInfo(filePath);
            
            return new EvidenceFile
            {
                Id = Guid.NewGuid().ToString(),
                FilePath = filePath,
                Type = type,
                ModuleName = moduleName,
                Hash = GenerateEvidenceHash(filePath),
                CollectedAt = DateTime.UtcNow,
                FileSizeBytes = fileInfo.Length,
                Description = description,
                LastValidated = DateTime.UtcNow,
                Tags = new List<string> { type.ToString(), moduleName.ToLower() }
            };
        }
        
        #endregion
    }
}