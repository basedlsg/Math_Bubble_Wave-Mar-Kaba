using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for Burst Compilation Validator system.
    /// Implements Requirement 4.2: Burst compilation validation for build quality assurance.
    /// Implements Requirement 4.6: Automated build quality gates and failure prevention.
    /// </summary>
    public interface IBurstCompilationValidator
    {
        /// <summary>
        /// Whether the Burst compilation validator is initialized and ready.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Current validator configuration settings.
        /// </summary>
        BurstValidatorConfiguration Configuration { get; }
        
        /// <summary>
        /// Current Burst compilation status.
        /// </summary>
        BurstCompilationStatus CompilationStatus { get; }
        
        /// <summary>
        /// Event fired when Burst compilation validation starts.
        /// </summary>
        event Action<BurstValidationStartedEventArgs> ValidationStarted;
        
        /// <summary>
        /// Event fired when Burst compilation validation completes.
        /// </summary>
        event Action<BurstValidationCompletedEventArgs> ValidationCompleted;
        
        /// <summary>
        /// Event fired when Burst compilation failure is detected.
        /// </summary>
        event Action<BurstCompilationFailedEventArgs> CompilationFailed;
        
        /// <summary>
        /// Initialize the Burst compilation validator.
        /// </summary>
        /// <param name="config">Validator configuration</param>
        /// <returns>True if initialization successful</returns>
        bool Initialize(BurstValidatorConfiguration config);
        
        /// <summary>
        /// Validate Burst compilation for all Burst-enabled code.
        /// </summary>
        /// <returns>Burst compilation validation result</returns>
        Task<BurstValidationResult> ValidateBurstCompilation();
        
        /// <summary>
        /// Validate Burst compilation for specific assembly.
        /// </summary>
        /// <param name="assemblyName">Assembly name to validate</param>
        /// <returns>Assembly-specific Burst validation result</returns>
        Task<AssemblyBurstValidationResult> ValidateAssemblyBurstCompilation(string assemblyName);
        
        /// <summary>
        /// Check if code is compatible with Burst compilation.
        /// </summary>
        /// <param name="codeAnalysisRequest">Code analysis request</param>
        /// <returns>Burst compatibility analysis result</returns>
        BurstCompatibilityResult AnalyzeBurstCompatibility(BurstCodeAnalysisRequest codeAnalysisRequest);
        
        /// <summary>
        /// Measure performance impact of Burst compilation.
        /// </summary>
        /// <param name="performanceTestConfig">Performance test configuration</param>
        /// <returns>Burst performance impact analysis</returns>
        Task<BurstPerformanceImpactResult> MeasureBurstPerformanceImpact(BurstPerformanceTestConfiguration performanceTestConfig);
        
        /// <summary>
        /// Detect and analyze Burst compilation failures.
        /// </summary>
        /// <param name="compilationContext">Compilation context for analysis</param>
        /// <returns>Compilation failure analysis result</returns>
        BurstCompilationFailureAnalysis AnalyzeCompilationFailures(BurstCompilationContext compilationContext);
        
        /// <summary>
        /// Generate Burst compilation validation report.
        /// </summary>
        /// <param name="validationId">Validation identifier</param>
        /// <param name="reportFormat">Report format</param>
        /// <returns>Generated Burst validation report</returns>
        BurstValidationReport GenerateValidationReport(string validationId, ReportFormat reportFormat);
        
        /// <summary>
        /// Export Burst compilation data in specified format.
        /// </summary>
        /// <param name="validationId">Validation identifier</param>
        /// <param name="exportFormat">Export format</param>
        /// <returns>Exported Burst compilation data</returns>
        BurstCompilationDataExport ExportCompilationData(string validationId, DataExportFormat exportFormat);
        
        /// <summary>
        /// Get Burst compilation statistics and metrics.
        /// </summary>
        /// <returns>Burst compilation statistics</returns>
        BurstCompilationStatistics GetCompilationStatistics();
        
        /// <summary>
        /// Configure Burst compilation validation settings.
        /// </summary>
        /// <param name="settings">Validation settings</param>
        void ConfigureValidationSettings(BurstValidationSettings settings);
        
        /// <summary>
        /// Get historical Burst compilation data.
        /// </summary>
        /// <param name="timeRange">Time range for historical data</param>
        /// <returns>Historical Burst compilation data</returns>
        HistoricalBurstData GetHistoricalCompilationData(TimeRange timeRange);
        
        /// <summary>
        /// Validate Burst compilation integration with CI/CD pipeline.
        /// </summary>
        /// <returns>CI/CD integration validation result</returns>
        BurstCIIntegrationResult ValidateCIIntegration();
        
        /// <summary>
        /// Reset Burst compilation validator state.
        /// </summary>
        void ResetValidator();
    }
    
    /// <summary>
    /// Configuration for Burst compilation validator.
    /// </summary>
    [Serializable]
    public struct BurstValidatorConfiguration
    {
        /// <summary>
        /// Whether to enable deep Burst analysis.
        /// </summary>
        public bool EnableDeepAnalysis;
        
        /// <summary>
        /// Whether to enable performance impact measurement.
        /// </summary>
        public bool EnablePerformanceMeasurement;
        
        /// <summary>
        /// Whether to enable automatic compilation testing.
        /// </summary>
        public bool EnableAutomaticTesting;
        
        /// <summary>
        /// Compilation timeout in seconds.
        /// </summary>
        public float CompilationTimeoutSeconds;
        
        /// <summary>
        /// Whether to enable detailed error reporting.
        /// </summary>
        public bool EnableDetailedErrorReporting;
        
        /// <summary>
        /// Whether to enable compatibility analysis.
        /// </summary>
        public bool EnableCompatibilityAnalysis;
        
        /// <summary>
        /// Maximum number of compilation retries.
        /// </summary>
        public int MaxCompilationRetries;
        
        /// <summary>
        /// Whether to enable debug logging.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Validation settings.
        /// </summary>
        public BurstValidationSettings ValidationSettings;
        
        /// <summary>
        /// Default Burst validator configuration.
        /// </summary>
        public static BurstValidatorConfiguration Default => new BurstValidatorConfiguration
        {
            EnableDeepAnalysis = true,
            EnablePerformanceMeasurement = true,
            EnableAutomaticTesting = true,
            CompilationTimeoutSeconds = 300f,
            EnableDetailedErrorReporting = true,
            EnableCompatibilityAnalysis = true,
            MaxCompilationRetries = 3,
            EnableDebugLogging = false,
            ValidationSettings = BurstValidationSettings.Default
        };
    }
    
    /// <summary>
    /// Burst validation settings.
    /// </summary>
    [Serializable]
    public struct BurstValidationSettings
    {
        /// <summary>
        /// Whether to fail validation on compilation warnings.
        /// </summary>
        public bool FailOnWarnings;
        
        /// <summary>
        /// Whether to validate performance improvements.
        /// </summary>
        public bool ValidatePerformanceImprovements;
        
        /// <summary>
        /// Minimum expected performance improvement percentage.
        /// </summary>
        public float MinimumPerformanceImprovementPercent;
        
        /// <summary>
        /// Whether to validate memory usage improvements.
        /// </summary>
        public bool ValidateMemoryImprovements;
        
        /// <summary>
        /// Whether to enable strict compatibility checking.
        /// </summary>
        public bool EnableStrictCompatibilityChecking;
        
        /// <summary>
        /// Assemblies to exclude from validation.
        /// </summary>
        public string[] ExcludedAssemblies;
        
        /// <summary>
        /// Custom validation rules.
        /// </summary>
        public string[] CustomValidationRules;
        
        /// <summary>
        /// Default Burst validation settings.
        /// </summary>
        public static BurstValidationSettings Default => new BurstValidationSettings
        {
            FailOnWarnings = false,
            ValidatePerformanceImprovements = true,
            MinimumPerformanceImprovementPercent = 10f,
            ValidateMemoryImprovements = true,
            EnableStrictCompatibilityChecking = true,
            ExcludedAssemblies = new string[0],
            CustomValidationRules = new string[0]
        };
    }
    
    /// <summary>
    /// Burst compilation status.
    /// </summary>
    public enum BurstCompilationStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed,
        Cancelled,
        Timeout
    }
    
    /// <summary>
    /// Burst validation result.
    /// </summary>
    [Serializable]
    public struct BurstValidationResult
    {
        /// <summary>
        /// Validation identifier.
        /// </summary>
        public string ValidationId;
        
        /// <summary>
        /// Whether validation passed.
        /// </summary>
        public bool ValidationPassed;
        
        /// <summary>
        /// Validation timestamp.
        /// </summary>
        public DateTime ValidationTimestamp;
        
        /// <summary>
        /// Total validation duration.
        /// </summary>
        public TimeSpan ValidationDuration;
        
        /// <summary>
        /// Number of assemblies validated.
        /// </summary>
        public int AssembliesValidated;
        
        /// <summary>
        /// Number of compilation failures.
        /// </summary>
        public int CompilationFailures;
        
        /// <summary>
        /// Assembly validation results.
        /// </summary>
        public AssemblyBurstValidationResult[] AssemblyResults;
        
        /// <summary>
        /// Overall performance impact.
        /// </summary>
        public BurstPerformanceImpactSummary PerformanceImpact;
        
        /// <summary>
        /// Compilation failure analysis.
        /// </summary>
        public BurstCompilationFailureAnalysis FailureAnalysis;
        
        /// <summary>
        /// Validation summary.
        /// </summary>
        public string ValidationSummary;
        
        /// <summary>
        /// Validation recommendations.
        /// </summary>
        public string[] ValidationRecommendations;
    }
    
    /// <summary>
    /// Assembly-specific Burst validation result.
    /// </summary>
    [Serializable]
    public struct AssemblyBurstValidationResult
    {
        /// <summary>
        /// Assembly name.
        /// </summary>
        public string AssemblyName;
        
        /// <summary>
        /// Whether assembly compilation passed.
        /// </summary>
        public bool CompilationPassed;
        
        /// <summary>
        /// Compilation duration.
        /// </summary>
        public TimeSpan CompilationDuration;
        
        /// <summary>
        /// Number of Burst-compiled methods.
        /// </summary>
        public int BurstCompiledMethods;
        
        /// <summary>
        /// Number of compilation errors.
        /// </summary>
        public int CompilationErrors;
        
        /// <summary>
        /// Number of compilation warnings.
        /// </summary>
        public int CompilationWarnings;
        
        /// <summary>
        /// Compilation error details.
        /// </summary>
        public BurstCompilationError[] CompilationErrorDetails;
        
        /// <summary>
        /// Performance impact for this assembly.
        /// </summary>
        public BurstPerformanceImpactResult PerformanceImpact;
        
        /// <summary>
        /// Compatibility analysis result.
        /// </summary>
        public BurstCompatibilityResult CompatibilityResult;
        
        /// <summary>
        /// Assembly validation notes.
        /// </summary>
        public string ValidationNotes;
    }
    
    /// <summary>
    /// Burst code analysis request.
    /// </summary>
    [Serializable]
    public struct BurstCodeAnalysisRequest
    {
        /// <summary>
        /// Analysis identifier.
        /// </summary>
        public string AnalysisId;
        
        /// <summary>
        /// Assembly name to analyze.
        /// </summary>
        public string AssemblyName;
        
        /// <summary>
        /// Specific methods to analyze (optional).
        /// </summary>
        public string[] MethodNames;
        
        /// <summary>
        /// Analysis depth level.
        /// </summary>
        public BurstAnalysisDepth AnalysisDepth;
        
        /// <summary>
        /// Whether to include performance analysis.
        /// </summary>
        public bool IncludePerformanceAnalysis;
        
        /// <summary>
        /// Whether to include compatibility analysis.
        /// </summary>
        public bool IncludeCompatibilityAnalysis;
        
        /// <summary>
        /// Custom analysis parameters.
        /// </summary>
        public Dictionary<string, object> CustomParameters;
    }
    
    /// <summary>
    /// Burst compatibility analysis result.
    /// </summary>
    [Serializable]
    public struct BurstCompatibilityResult
    {
        /// <summary>
        /// Whether code is compatible with Burst.
        /// </summary>
        public bool IsCompatible;
        
        /// <summary>
        /// Compatibility score (0-100).
        /// </summary>
        public float CompatibilityScore;
        
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp;
        
        /// <summary>
        /// Compatibility issues identified.
        /// </summary>
        public BurstCompatibilityIssue[] CompatibilityIssues;
        
        /// <summary>
        /// Supported Burst features.
        /// </summary>
        public string[] SupportedFeatures;
        
        /// <summary>
        /// Unsupported features found.
        /// </summary>
        public string[] UnsupportedFeatures;
        
        /// <summary>
        /// Compatibility recommendations.
        /// </summary>
        public string[] CompatibilityRecommendations;
        
        /// <summary>
        /// Analysis confidence level (0-100).
        /// </summary>
        public float AnalysisConfidence;
    }
    
    /// <summary>
    /// Burst performance test configuration.
    /// </summary>
    [Serializable]
    public struct BurstPerformanceTestConfiguration
    {
        /// <summary>
        /// Test identifier.
        /// </summary>
        public string TestId;
        
        /// <summary>
        /// Assembly to test.
        /// </summary>
        public string AssemblyName;
        
        /// <summary>
        /// Methods to test (optional).
        /// </summary>
        public string[] MethodNames;
        
        /// <summary>
        /// Number of test iterations.
        /// </summary>
        public int TestIterations;
        
        /// <summary>
        /// Test duration in seconds.
        /// </summary>
        public float TestDurationSeconds;
        
        /// <summary>
        /// Whether to test with and without Burst.
        /// </summary>
        public bool CompareWithoutBurst;
        
        /// <summary>
        /// Whether to measure memory impact.
        /// </summary>
        public bool MeasureMemoryImpact;
        
        /// <summary>
        /// Custom test parameters.
        /// </summary>
        public Dictionary<string, object> CustomParameters;
    }
    
    /// <summary>
    /// Burst performance impact result.
    /// </summary>
    [Serializable]
    public struct BurstPerformanceImpactResult
    {
        /// <summary>
        /// Test identifier.
        /// </summary>
        public string TestId;
        
        /// <summary>
        /// Performance improvement percentage.
        /// </summary>
        public float PerformanceImprovementPercent;
        
        /// <summary>
        /// Execution time with Burst (milliseconds).
        /// </summary>
        public float BurstExecutionTimeMS;
        
        /// <summary>
        /// Execution time without Burst (milliseconds).
        /// </summary>
        public float NonBurstExecutionTimeMS;
        
        /// <summary>
        /// Memory usage with Burst (MB).
        /// </summary>
        public float BurstMemoryUsageMB;
        
        /// <summary>
        /// Memory usage without Burst (MB).
        /// </summary>
        public float NonBurstMemoryUsageMB;
        
        /// <summary>
        /// Memory improvement percentage.
        /// </summary>
        public float MemoryImprovementPercent;
        
        /// <summary>
        /// Test timestamp.
        /// </summary>
        public DateTime TestTimestamp;
        
        /// <summary>
        /// Test confidence level (0-100).
        /// </summary>
        public float TestConfidence;
        
        /// <summary>
        /// Performance test notes.
        /// </summary>
        public string TestNotes;
    }
    
    /// <summary>
    /// Burst compilation context.
    /// </summary>
    [Serializable]
    public struct BurstCompilationContext
    {
        /// <summary>
        /// Compilation identifier.
        /// </summary>
        public string CompilationId;
        
        /// <summary>
        /// Unity version.
        /// </summary>
        public string UnityVersion;
        
        /// <summary>
        /// Burst version.
        /// </summary>
        public string BurstVersion;
        
        /// <summary>
        /// Target platform.
        /// </summary>
        public string TargetPlatform;
        
        /// <summary>
        /// Build configuration.
        /// </summary>
        public string BuildConfiguration;
        
        /// <summary>
        /// Compilation timestamp.
        /// </summary>
        public DateTime CompilationTimestamp;
        
        /// <summary>
        /// Environment variables.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables;
        
        /// <summary>
        /// Compilation parameters.
        /// </summary>
        public Dictionary<string, object> CompilationParameters;
    }
    
    /// <summary>
    /// Burst compilation failure analysis.
    /// </summary>
    [Serializable]
    public struct BurstCompilationFailureAnalysis
    {
        /// <summary>
        /// Analysis identifier.
        /// </summary>
        public string AnalysisId;
        
        /// <summary>
        /// Total number of failures.
        /// </summary>
        public int TotalFailures;
        
        /// <summary>
        /// Failure categories.
        /// </summary>
        public BurstFailureCategory[] FailureCategories;
        
        /// <summary>
        /// Most common failure types.
        /// </summary>
        public BurstFailureType[] CommonFailureTypes;
        
        /// <summary>
        /// Detailed failure analysis.
        /// </summary>
        public BurstCompilationError[] DetailedFailures;
        
        /// <summary>
        /// Root cause analysis.
        /// </summary>
        public string[] RootCauses;
        
        /// <summary>
        /// Resolution recommendations.
        /// </summary>
        public string[] ResolutionRecommendations;
        
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp;
        
        /// <summary>
        /// Analysis confidence level (0-100).
        /// </summary>
        public float AnalysisConfidence;
    }
    
    // Supporting enums and data structures
    
    /// <summary>
    /// Burst analysis depth levels.
    /// </summary>
    public enum BurstAnalysisDepth
    {
        Basic,
        Standard,
        Deep,
        Comprehensive
    }
    
    /// <summary>
    /// Burst failure categories.
    /// </summary>
    public enum BurstFailureCategory
    {
        SyntaxError,
        CompatibilityIssue,
        PerformanceRegression,
        MemoryIssue,
        PlatformSpecific,
        ConfigurationError
    }
    
    /// <summary>
    /// Burst failure types.
    /// </summary>
    public enum BurstFailureType
    {
        UnsupportedFeature,
        InvalidSyntax,
        MissingDependency,
        PerformanceRegression,
        MemoryLeak,
        PlatformIncompatibility,
        ConfigurationMismatch
    }
    
    // Event argument classes
    
    /// <summary>
    /// Event arguments for Burst validation started.
    /// </summary>
    public class BurstValidationStartedEventArgs : EventArgs
    {
        public string ValidationId { get; set; }
        public BurstValidatorConfiguration Configuration { get; set; }
        public DateTime StartTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for Burst validation completed.
    /// </summary>
    public class BurstValidationCompletedEventArgs : EventArgs
    {
        public string ValidationId { get; set; }
        public BurstValidationResult Result { get; set; }
        public DateTime CompletionTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for Burst compilation failed.
    /// </summary>
    public class BurstCompilationFailedEventArgs : EventArgs
    {
        public string ValidationId { get; set; }
        public BurstCompilationError[] Errors { get; set; }
        public DateTime FailureTimestamp { get; set; }
    }
    
    // Additional supporting structures would be defined in the implementation file
    // to keep this interface focused on the core contract
}