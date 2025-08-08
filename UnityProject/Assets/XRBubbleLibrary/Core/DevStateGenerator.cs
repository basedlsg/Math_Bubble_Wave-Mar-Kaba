using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Main implementation of the development state generator.
    /// Uses reflection-based analysis of assembly definitions to determine system state.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class DevStateGenerator : IDevStateGenerator
    {
        private static DevStateGenerator _instance;
        private readonly ICompilerFlagManager _compilerFlagManager;
        
        /// <summary>
        /// Singleton instance for global access.
        /// </summary>
        public static DevStateGenerator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DevStateGenerator();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Constructor with dependency injection support.
        /// </summary>
        /// <param name="compilerFlagManager">Compiler flag manager instance</param>
        public DevStateGenerator(ICompilerFlagManager compilerFlagManager = null)
        {
            _compilerFlagManager = compilerFlagManager ?? CompilerFlagManager.Instance;
        }
        
        /// <summary>
        /// Generate a comprehensive development state report.
        /// </summary>
        public DevStateReport GenerateReport()
        {
            Debug.Log("[DevStateGenerator] Generating development state report...");
            
            var report = new DevStateReport
            {
                GeneratedAt = DateTime.UtcNow,
                BuildVersion = GetBuildVersion(),
                UnityVersion = Application.unityVersion,
                BuildConfiguration = GetBuildConfiguration()
            };
            
            // Get compiler flag states
            report.CompilerFlags = _compilerFlagManager.GetAllFeatureStates();
            
            // Analyze all modules
            var assemblyPaths = GetAssemblyDefinitions();
            report.Modules = ModuleStatusAnalyzer.AnalyzeModules(assemblyPaths);
            
            // Generate summary
            report.Summary = GenerateSummary(report.Modules);
            
            // Collect performance metrics
            report.CurrentPerformance = CollectPerformanceMetrics();
            
            // Collect supporting evidence
            report.SupportingEvidence = CollectSupportingEvidence();
            
            Debug.Log($"[DevStateGenerator] Report generated with {report.Modules.Count} modules");
            return report;
        }
        
        /// <summary>
        /// Generate a development state report asynchronously.
        /// </summary>
        public async Task<DevStateReport> GenerateReportAsync()
        {
            return await Task.Run(() => GenerateReport());
        }
        
        /// <summary>
        /// Schedule automatic nightly generation of development state reports.
        /// </summary>
        public void ScheduleNightlyGeneration()
        {
            Debug.Log("[DevStateGenerator] Nightly generation scheduling is not implemented in this version");
            // TODO: Implement nightly scheduling when CI/CD system is available
        }
        
        /// <summary>
        /// Validate the accuracy of the generated report against actual system state.
        /// </summary>
        public bool ValidateReportAccuracy(DevStateReport report)
        {
            if (report == null)
            {
                Debug.LogError("[DevStateGenerator] Cannot validate null report");
                return false;
            }
            
            var issues = report.ValidateConsistency();
            if (issues.Count > 0)
            {
                Debug.LogWarning($"[DevStateGenerator] Report validation found {issues.Count} issues:");
                foreach (var issue in issues)
                {
                    Debug.LogWarning($"  - {issue}");
                }
                return false;
            }
            
            // Additional validation against current system state
            var currentFlags = _compilerFlagManager.GetAllFeatureStates();
            foreach (var flag in currentFlags)
            {
                if (!report.CompilerFlags.ContainsKey(flag.Key) || report.CompilerFlags[flag.Key] != flag.Value)
                {
                    Debug.LogWarning($"[DevStateGenerator] Compiler flag mismatch for {flag.Key}");
                    return false;
                }
            }
            
            Debug.Log("[DevStateGenerator] Report validation passed");
            return true;
        }
        
        /// <summary>
        /// Get the current build version information.
        /// </summary>
        public string GetBuildVersion()
        {
            // Try to get version from PlayerSettings or generate one
            var version = Application.version;
            if (string.IsNullOrEmpty(version))
            {
                version = $"dev-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
            }
            return version;
        }
        
        /// <summary>
        /// Get all available assembly definitions in the project.
        /// </summary>
        public List<string> GetAssemblyDefinitions()
        {
            var assemblyPaths = new List<string>();
            
            // Search for .asmdef files in the XRBubbleLibrary directory
            var xrBubbleLibraryPath = Path.Combine(Application.dataPath, "XRBubbleLibrary");
            
            if (Directory.Exists(xrBubbleLibraryPath))
            {
                var asmdefFiles = Directory.GetFiles(xrBubbleLibraryPath, "*.asmdef", SearchOption.AllDirectories);
                assemblyPaths.AddRange(asmdefFiles);
            }
            
            Debug.Log($"[DevStateGenerator] Found {assemblyPaths.Count} assembly definitions");
            return assemblyPaths;
        }
        
        /// <summary>
        /// Generate and save the DEV_STATE.md file to the project root.
        /// </summary>
        public string GenerateAndSaveDevStateFile()
        {
            var report = GenerateReport();
            var markdown = report.ToMarkdown();
            
            // Save to project root
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var devStateFile = Path.Combine(projectRoot, "DEV_STATE.md");
            
            try
            {
                File.WriteAllText(devStateFile, markdown);
                Debug.Log($"[DevStateGenerator] DEV_STATE.md saved to: {devStateFile}");
                return devStateFile;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DevStateGenerator] Failed to save DEV_STATE.md: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Generate and save the DEV_STATE.md file asynchronously.
        /// </summary>
        public async Task<string> GenerateAndSaveDevStateFileAsync()
        {
            return await Task.Run(() => GenerateAndSaveDevStateFile());
        }
        
        /// <summary>
        /// Generate summary statistics from module list.
        /// </summary>
        private DevStateSummary GenerateSummary(List<ModuleStatus> modules)
        {
            return new DevStateSummary
            {
                TotalModules = modules.Count,
                ImplementedModules = modules.Count(m => m.State == ModuleState.Implemented),
                DisabledModules = modules.Count(m => m.State == ModuleState.Disabled),
                ConceptualModules = modules.Count(m => m.State == ModuleState.Conceptual)
            };
        }
        
        /// <summary>
        /// Collect current performance metrics from the system.
        /// </summary>
        private PerformanceMetrics CollectPerformanceMetrics()
        {
            // Basic performance metrics collection
            var metrics = new PerformanceMetrics
            {
                CapturedAt = DateTime.UtcNow,
                BuildVersion = GetBuildVersion()
            };
            
            // Try to get basic Unity performance metrics
            try
            {
                // Note: These are basic metrics. More sophisticated collection would be done
                // by the performance monitoring system in later tasks.
                metrics.AverageFPS = 1.0f / Time.smoothDeltaTime;
                metrics.AverageFrameTime = Time.smoothDeltaTime * 1000f; // Convert to milliseconds
                
                // Memory usage (approximate)
                var totalMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0);
                metrics.MemoryUsage = totalMemory;
                
                // Add Unity-specific metrics
                metrics.AdditionalMetrics["UnityFrameCount"] = Time.frameCount;
                metrics.AdditionalMetrics["UnityTimeScale"] = Time.timeScale;
                metrics.AdditionalMetrics["UnityTargetFrameRate"] = Application.targetFrameRate;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DevStateGenerator] Failed to collect some performance metrics: {ex.Message}");
            }
            
            return metrics;
        }
        
        /// <summary>
        /// Collect supporting evidence files using the Evidence Collection System.
        /// </summary>
        private List<EvidenceFile> CollectSupportingEvidence()
        {
            try
            {
                // Use the new Evidence Collection System
                var evidenceService = new EvidenceCollectorService();
                var evidence = evidenceService.SearchEvidence(new EvidenceSearchCriteria 
                { 
                    MaxResults = 1000,
                    OnlyValid = true
                });
                
                Debug.Log($"[DevStateGenerator] Collected {evidence.Count} evidence files using Evidence Collection System");
                return evidence;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DevStateGenerator] Failed to collect evidence using Evidence Collection System: {ex.Message}");
                
                // Fallback to basic evidence collection
                return CollectBasicEvidence();
            }
        }
        
        /// <summary>
        /// Fallback method for basic evidence collection when the Evidence Collection System is unavailable.
        /// </summary>
        private List<EvidenceFile> CollectBasicEvidence()
        {
            var evidence = new List<EvidenceFile>();
            
            try
            {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                
                // Look for performance reports
                var perfReports = Directory.GetFiles(projectRoot, "*perf*.md", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(projectRoot, "*performance*.md", SearchOption.AllDirectories))
                    .ToList();
                
                foreach (var perfReport in perfReports)
                {
                    evidence.Add(new EvidenceFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        FilePath = perfReport,
                        Type = EvidenceType.PerformanceLog,
                        ModuleName = "Performance",
                        Hash = CalculateFileHash(perfReport),
                        CollectedAt = DateTime.UtcNow,
                        FileSizeBytes = new FileInfo(perfReport).Length,
                        Description = "Performance validation report",
                        Tags = new List<string> { "performance", "validation" },
                        IsValid = true,
                        LastValidated = DateTime.UtcNow
                    });
                }
                
                // Look for test results
                var testResults = Directory.GetFiles(projectRoot, "*test*.xml", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(projectRoot, "*TestResults*.xml", SearchOption.AllDirectories))
                    .ToList();
                
                foreach (var testResult in testResults)
                {
                    evidence.Add(new EvidenceFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        FilePath = testResult,
                        Type = EvidenceType.TestResult,
                        ModuleName = "Testing",
                        Hash = CalculateFileHash(testResult),
                        CollectedAt = DateTime.UtcNow,
                        FileSizeBytes = new FileInfo(testResult).Length,
                        Description = "Unit test results",
                        Tags = new List<string> { "testing", "validation" },
                        IsValid = true,
                        LastValidated = DateTime.UtcNow
                    });
                }
                
                // Look for build logs
                var buildLogs = Directory.GetFiles(projectRoot, "*build*.log", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(projectRoot, "*.log", SearchOption.AllDirectories))
                    .Take(5) // Limit to recent logs
                    .ToList();
                
                foreach (var buildLog in buildLogs)
                {
                    evidence.Add(new EvidenceFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        FilePath = buildLog,
                        Type = EvidenceType.BuildLog,
                        ModuleName = "Build",
                        Hash = CalculateFileHash(buildLog),
                        CollectedAt = DateTime.UtcNow,
                        FileSizeBytes = new FileInfo(buildLog).Length,
                        Description = "Build compilation log",
                        Tags = new List<string> { "build", "compilation" },
                        IsValid = true,
                        LastValidated = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DevStateGenerator] Failed to collect basic evidence files: {ex.Message}");
            }
            
            Debug.Log($"[DevStateGenerator] Collected {evidence.Count} evidence files using fallback method");
            return evidence;
        }
        
        /// <summary>
        /// Calculate SHA256 hash of a file for integrity verification.
        /// Uses the same format as the Evidence Collection System.
        /// </summary>
        private string CalculateFileHash(string filePath)
        {
            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var hash = sha256.ComputeHash(stream);
                        return Convert.ToBase64String(hash);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DevStateGenerator] Failed to calculate hash for {filePath}: {ex.Message}");
                return "hash-unavailable";
            }
        }
        
        /// <summary>
        /// Get the current build configuration.
        /// </summary>
        private string GetBuildConfiguration()
        {
#if UNITY_EDITOR
            return "Editor (Development)";
#elif DEVELOPMENT_BUILD
            return "Development Build";
#elif DEBUG
            return "Debug Build";
#else
            return "Release Build";
#endif
        }
    }
}