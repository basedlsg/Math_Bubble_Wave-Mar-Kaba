using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Supporting data structures for Burst Compilation Validator system.
    /// </summary>
    
    /// <summary>
    /// Burst compilation error details.
    /// </summary>
    [Serializable]
    public struct BurstCompilationError
    {
        /// <summary>
        /// Error identifier.
        /// </summary>
        public string ErrorId;
        
        /// <summary>
        /// Error type.
        /// </summary>
        public BurstFailureType ErrorType;
        
        /// <summary>
        /// Error severity level.
        /// </summary>
        public BurstErrorSeverity Severity;
        
        /// <summary>
        /// Error message.
        /// </summary>
        public string ErrorMessage;
        
        /// <summary>
        /// Source file path.
        /// </summary>
        public string SourceFilePath;
        
        /// <summary>
        /// Line number where error occurred.
        /// </summary>
        public int LineNumber;
        
        /// <summary>
        /// Column number where error occurred.
        /// </summary>
        public int ColumnNumber;
        
        /// <summary>
        /// Method name where error occurred.
        /// </summary>
        public string MethodName;
        
        /// <summary>
        /// Assembly name where error occurred.
        /// </summary>
        public string AssemblyName;
        
        /// <summary>
        /// Error stack trace.
        /// </summary>
        public string StackTrace;
        
        /// <summary>
        /// Suggested resolution.
        /// </summary>
        public string SuggestedResolution;
        
        /// <summary>
        /// Error timestamp.
        /// </summary>
        public DateTime ErrorTimestamp;
        
        /// <summary>
        /// Additional error context.
        /// </summary>
        public Dictionary<string, string> ErrorContext;
    }
    
    /// <summary>
    /// Burst compatibility issue.
    /// </summary>
    [Serializable]
    public struct BurstCompatibilityIssue
    {
        /// <summary>
        /// Issue identifier.
        /// </summary>
        public string IssueId;
        
        /// <summary>
        /// Issue type.
        /// </summary>
        public BurstCompatibilityIssueType IssueType;
        
        /// <summary>
        /// Issue severity level.
        /// </summary>
        public BurstIssueSeverity Severity;
        
        /// <summary>
        /// Issue description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Affected code location.
        /// </summary>
        public string CodeLocation;
        
        /// <summary>
        /// Unsupported feature details.
        /// </summary>
        public string UnsupportedFeature;
        
        /// <summary>
        /// Recommended fix.
        /// </summary>
        public string RecommendedFix;
        
        /// <summary>
        /// Alternative approaches.
        /// </summary>
        public string[] AlternativeApproaches;
        
        /// <summary>
        /// Issue impact assessment.
        /// </summary>
        public string ImpactAssessment;
        
        /// <summary>
        /// Issue detection timestamp.
        /// </summary>
        public DateTime DetectionTimestamp;
    }
    
    /// <summary>
    /// Burst performance impact summary.
    /// </summary>
    [Serializable]
    public struct BurstPerformanceImpactSummary
    {
        /// <summary>
        /// Overall performance improvement percentage.
        /// </summary>
        public float OverallPerformanceImprovementPercent;
        
        /// <summary>
        /// Average execution time improvement.
        /// </summary>
        public float AverageExecutionTimeImprovementPercent;
        
        /// <summary>
        /// Memory usage improvement percentage.
        /// </summary>
        public float MemoryUsageImprovementPercent;
        
        /// <summary>
        /// Number of methods tested.
        /// </summary>
        public int MethodsTested;
        
        /// <summary>
        /// Number of methods with performance improvements.
        /// </summary>
        public int MethodsWithImprovements;
        
        /// <summary>
        /// Best performing method.
        /// </summary>
        public string BestPerformingMethod;
        
        /// <summary>
        /// Best performance improvement percentage.
        /// </summary>
        public float BestPerformanceImprovementPercent;
        
        /// <summary>
        /// Methods with performance regressions.
        /// </summary>
        public string[] MethodsWithRegressions;
        
        /// <summary>
        /// Performance test confidence level (0-100).
        /// </summary>
        public float TestConfidence;
        
        /// <summary>
        /// Summary generation timestamp.
        /// </summary>
        public DateTime SummaryTimestamp;
    }
    
    /// <summary>
    /// Burst validation report.
    /// </summary>
    [Serializable]
    public struct BurstValidationReport
    {
        /// <summary>
        /// Report identifier.
        /// </summary>
        public string ReportId;
        
        /// <summary>
        /// Validation identifier.
        /// </summary>
        public string ValidationId;
        
        /// <summary>
        /// Report format.
        /// </summary>
        public ReportFormat Format;
        
        /// <summary>
        /// Report title.
        /// </summary>
        public string Title;
        
        /// <summary>
        /// Report content.
        /// </summary>
        public string Content;
        
        /// <summary>
        /// Report summary.
        /// </summary>
        public string Summary;
        
        /// <summary>
        /// Report generation timestamp.
        /// </summary>
        public DateTime GenerationTimestamp;
        
        /// <summary>
        /// Validation results included in report.
        /// </summary>
        public BurstValidationResult ValidationResult;
        
        /// <summary>
        /// Report metadata.
        /// </summary>
        public Dictionary<string, object> Metadata;
    }
    
    /// <summary>
    /// Burst compilation data export.
    /// </summary>
    [Serializable]
    public struct BurstCompilationDataExport
    {
        /// <summary>
        /// Export identifier.
        /// </summary>
        public string ExportId;
        
        /// <summary>
        /// Validation identifier.
        /// </summary>
        public string ValidationId;
        
        /// <summary>
        /// Export format.
        /// </summary>
        public DataExportFormat Format;
        
        /// <summary>
        /// Export content.
        /// </summary>
        public string Content;
        
        /// <summary>
        /// Whether export was successful.
        /// </summary>
        public bool IsSuccessful;
        
        /// <summary>
        /// Export timestamp.
        /// </summary>
        public DateTime ExportTimestamp;
        
        /// <summary>
        /// Number of records exported.
        /// </summary>
        public int RecordCount;
        
        /// <summary>
        /// Export file path.
        /// </summary>
        public string FilePath;
        
        /// <summary>
        /// Export messages.
        /// </summary>
        public string[] ExportMessages;
    }
    
    /// <summary>
    /// Burst compilation statistics.
    /// </summary>
    [Serializable]
    public struct BurstCompilationStatistics
    {
        /// <summary>
        /// Total number of validations performed.
        /// </summary>
        public int TotalValidations;
        
        /// <summary>
        /// Number of successful validations.
        /// </summary>
        public int SuccessfulValidations;
        
        /// <summary>
        /// Number of failed validations.
        /// </summary>
        public int FailedValidations;
        
        /// <summary>
        /// Success rate percentage.
        /// </summary>
        public float SuccessRate;
        
        /// <summary>
        /// Average validation duration.
        /// </summary>
        public TimeSpan AverageValidationDuration;
        
        /// <summary>
        /// Total assemblies validated.
        /// </summary>
        public int TotalAssembliesValidated;
        
        /// <summary>
        /// Total methods compiled with Burst.
        /// </summary>
        public int TotalBurstCompiledMethods;
        
        /// <summary>
        /// Average performance improvement percentage.
        /// </summary>
        public float AveragePerformanceImprovement;
        
        /// <summary>
        /// Most common error types.
        /// </summary>
        public BurstErrorStatistic[] CommonErrorTypes;
        
        /// <summary>
        /// Statistics generation timestamp.
        /// </summary>
        public DateTime StatisticsTimestamp;
    }
    
    /// <summary>
    /// Burst error statistic.
    /// </summary>
    [Serializable]
    public struct BurstErrorStatistic
    {
        /// <summary>
        /// Error type.
        /// </summary>
        public BurstFailureType ErrorType;
        
        /// <summary>
        /// Number of occurrences.
        /// </summary>
        public int Occurrences;
        
        /// <summary>
        /// Percentage of total errors.
        /// </summary>
        public float PercentageOfTotal;
        
        /// <summary>
        /// Most common error message.
        /// </summary>
        public string MostCommonMessage;
        
        /// <summary>
        /// Resolution success rate.
        /// </summary>
        public float ResolutionSuccessRate;
    }
    
    /// <summary>
    /// Historical Burst compilation data.
    /// </summary>
    [Serializable]
    public struct HistoricalBurstData
    {
        /// <summary>
        /// Time range for the historical data.
        /// </summary>
        public TimeRange TimeRange;
        
        /// <summary>
        /// Historical validation results.
        /// </summary>
        public HistoricalBurstValidation[] Validations;
        
        /// <summary>
        /// Performance trend over time.
        /// </summary>
        public BurstPerformanceTrend PerformanceTrend;
        
        /// <summary>
        /// Error trend over time.
        /// </summary>
        public BurstErrorTrend ErrorTrend;
        
        /// <summary>
        /// Historical statistics.
        /// </summary>
        public HistoricalBurstStatistics Statistics;
        
        /// <summary>
        /// Data retrieval timestamp.
        /// </summary>
        public DateTime RetrievalTimestamp;
    }
    
    /// <summary>
    /// Historical Burst validation summary.
    /// </summary>
    [Serializable]
    public struct HistoricalBurstValidation
    {
        /// <summary>
        /// Validation identifier.
        /// </summary>
        public string ValidationId;
        
        /// <summary>
        /// Validation timestamp.
        /// </summary>
        public DateTime ValidationTimestamp;
        
        /// <summary>
        /// Whether validation passed.
        /// </summary>
        public bool ValidationPassed;
        
        /// <summary>
        /// Validation duration.
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
        /// Overall performance improvement.
        /// </summary>
        public float PerformanceImprovement;
    }
    
    /// <summary>
    /// Burst performance trend analysis.
    /// </summary>
    [Serializable]
    public struct BurstPerformanceTrend
    {
        /// <summary>
        /// Overall trend direction.
        /// </summary>
        public TrendDirection OverallTrend;
        
        /// <summary>
        /// Performance improvement trend.
        /// </summary>
        public float PerformanceImprovementTrend;
        
        /// <summary>
        /// Compilation time trend.
        /// </summary>
        public float CompilationTimeTrend;
        
        /// <summary>
        /// Success rate trend.
        /// </summary>
        public float SuccessRateTrend;
        
        /// <summary>
        /// Trend change points.
        /// </summary>
        public TrendChangePoint[] ChangePoints;
        
        /// <summary>
        /// Trend confidence level (0-100).
        /// </summary>
        public float TrendConfidence;
    }
    
    /// <summary>
    /// Burst error trend analysis.
    /// </summary>
    [Serializable]
    public struct BurstErrorTrend
    {
        /// <summary>
        /// Overall error trend direction.
        /// </summary>
        public TrendDirection ErrorTrend;
        
        /// <summary>
        /// Error rate change over time.
        /// </summary>
        public float ErrorRateChange;
        
        /// <summary>
        /// Most improving error types.
        /// </summary>
        public BurstFailureType[] ImprovingErrorTypes;
        
        /// <summary>
        /// Most problematic error types.
        /// </summary>
        public BurstFailureType[] ProblematicErrorTypes;
        
        /// <summary>
        /// Error resolution trend.
        /// </summary>
        public float ErrorResolutionTrend;
        
        /// <summary>
        /// Trend analysis confidence (0-100).
        /// </summary>
        public float AnalysisConfidence;
    }
    
    /// <summary>
    /// Historical Burst compilation statistics.
    /// </summary>
    [Serializable]
    public struct HistoricalBurstStatistics
    {
        /// <summary>
        /// Total historical validations.
        /// </summary>
        public int TotalValidations;
        
        /// <summary>
        /// Average success rate over time.
        /// </summary>
        public float AverageSuccessRate;
        
        /// <summary>
        /// Best performance period.
        /// </summary>
        public TimeRange BestPerformancePeriod;
        
        /// <summary>
        /// Worst performance period.
        /// </summary>
        public TimeRange WorstPerformancePeriod;
        
        /// <summary>
        /// Overall improvement over time.
        /// </summary>
        public float OverallImprovement;
        
        /// <summary>
        /// Most stable assemblies.
        /// </summary>
        public string[] MostStableAssemblies;
        
        /// <summary>
        /// Most problematic assemblies.
        /// </summary>
        public string[] MostProblematicAssemblies;
    }
    
    /// <summary>
    /// Burst CI integration validation result.
    /// </summary>
    [Serializable]
    public struct BurstCIIntegrationResult
    {
        /// <summary>
        /// Whether CI integration is working correctly.
        /// </summary>
        public bool IntegrationWorking;
        
        /// <summary>
        /// CI integration test results.
        /// </summary>
        public CIIntegrationTestResult[] TestResults;
        
        /// <summary>
        /// Integration configuration status.
        /// </summary>
        public CIConfigurationStatus ConfigurationStatus;
        
        /// <summary>
        /// Performance gate integration status.
        /// </summary>
        public bool PerformanceGateIntegration;
        
        /// <summary>
        /// Build pipeline integration status.
        /// </summary>
        public bool BuildPipelineIntegration;
        
        /// <summary>
        /// Integration recommendations.
        /// </summary>
        public string[] IntegrationRecommendations;
        
        /// <summary>
        /// Integration test timestamp.
        /// </summary>
        public DateTime TestTimestamp;
        
        /// <summary>
        /// Integration confidence level (0-100).
        /// </summary>
        public float IntegrationConfidence;
    }
    
    /// <summary>
    /// CI integration test result.
    /// </summary>
    [Serializable]
    public struct CIIntegrationTestResult
    {
        /// <summary>
        /// Test name.
        /// </summary>
        public string TestName;
        
        /// <summary>
        /// Whether test passed.
        /// </summary>
        public bool Passed;
        
        /// <summary>
        /// Test score (0-100).
        /// </summary>
        public float Score;
        
        /// <summary>
        /// Test description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Test result details.
        /// </summary>
        public string Details;
        
        /// <summary>
        /// Test duration.
        /// </summary>
        public TimeSpan TestDuration;
    }
    
    /// <summary>
    /// CI configuration status.
    /// </summary>
    [Serializable]
    public struct CIConfigurationStatus
    {
        /// <summary>
        /// Whether configuration is valid.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// Configuration validation score (0-100).
        /// </summary>
        public float ValidationScore;
        
        /// <summary>
        /// Configuration issues identified.
        /// </summary>
        public string[] ConfigurationIssues;
        
        /// <summary>
        /// Configuration recommendations.
        /// </summary>
        public string[] ConfigurationRecommendations;
        
        /// <summary>
        /// Required configuration changes.
        /// </summary>
        public string[] RequiredChanges;
    }
    
    // Supporting enums
    
    /// <summary>
    /// Burst error severity levels.
    /// </summary>
    public enum BurstErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>
    /// Burst compatibility issue types.
    /// </summary>
    public enum BurstCompatibilityIssueType
    {
        UnsupportedType,
        UnsupportedMethod,
        UnsupportedFeature,
        PerformanceIssue,
        MemoryIssue,
        PlatformSpecific
    }
    
    /// <summary>
    /// Burst issue severity levels.
    /// </summary>
    public enum BurstIssueSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    // Method-specific analysis structures
    
    /// <summary>
    /// Burst method analysis result.
    /// </summary>
    [Serializable]
    public struct BurstMethodAnalysisResult
    {
        /// <summary>
        /// Method name.
        /// </summary>
        public string MethodName;
        
        /// <summary>
        /// Whether method compiled successfully with Burst.
        /// </summary>
        public bool CompiledSuccessfully;
        
        /// <summary>
        /// Compilation duration.
        /// </summary>
        public TimeSpan CompilationDuration;
        
        /// <summary>
        /// Performance improvement with Burst.
        /// </summary>
        public float PerformanceImprovementPercent;
        
        /// <summary>
        /// Memory usage improvement.
        /// </summary>
        public float MemoryImprovementPercent;
        
        /// <summary>
        /// Burst compatibility score (0-100).
        /// </summary>
        public float CompatibilityScore;
        
        /// <summary>
        /// Method-specific compilation errors.
        /// </summary>
        public BurstCompilationError[] CompilationErrors;
        
        /// <summary>
        /// Method optimization recommendations.
        /// </summary>
        public string[] OptimizationRecommendations;
    }
    
    /// <summary>
    /// Burst assembly analysis summary.
    /// </summary>
    [Serializable]
    public struct BurstAssemblyAnalysisSummary
    {
        /// <summary>
        /// Assembly name.
        /// </summary>
        public string AssemblyName;
        
        /// <summary>
        /// Total methods in assembly.
        /// </summary>
        public int TotalMethods;
        
        /// <summary>
        /// Methods eligible for Burst compilation.
        /// </summary>
        public int BurstEligibleMethods;
        
        /// <summary>
        /// Methods successfully compiled with Burst.
        /// </summary>
        public int SuccessfullyCompiledMethods;
        
        /// <summary>
        /// Overall assembly Burst compatibility percentage.
        /// </summary>
        public float AssemblyCompatibilityPercent;
        
        /// <summary>
        /// Average performance improvement across methods.
        /// </summary>
        public float AveragePerformanceImprovement;
        
        /// <summary>
        /// Assembly-level recommendations.
        /// </summary>
        public string[] AssemblyRecommendations;
        
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp;
    }
    
    /// <summary>
    /// Burst optimization recommendation.
    /// </summary>
    [Serializable]
    public struct BurstOptimizationRecommendation
    {
        /// <summary>
        /// Recommendation identifier.
        /// </summary>
        public string RecommendationId;
        
        /// <summary>
        /// Recommendation type.
        /// </summary>
        public BurstOptimizationType Type;
        
        /// <summary>
        /// Recommendation priority.
        /// </summary>
        public BurstOptimizationPriority Priority;
        
        /// <summary>
        /// Recommendation description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Affected code location.
        /// </summary>
        public string CodeLocation;
        
        /// <summary>
        /// Expected performance improvement.
        /// </summary>
        public float ExpectedImprovementPercent;
        
        /// <summary>
        /// Implementation difficulty.
        /// </summary>
        public BurstImplementationDifficulty Difficulty;
        
        /// <summary>
        /// Implementation steps.
        /// </summary>
        public string[] ImplementationSteps;
        
        /// <summary>
        /// Recommendation confidence (0-100).
        /// </summary>
        public float RecommendationConfidence;
    }
    
    /// <summary>
    /// Burst optimization types.
    /// </summary>
    public enum BurstOptimizationType
    {
        CodeRefactoring,
        DataStructureOptimization,
        AlgorithmImprovement,
        MemoryOptimization,
        ParallelizationOpportunity,
        CompilerHintAddition
    }
    
    /// <summary>
    /// Burst optimization priorities.
    /// </summary>
    public enum BurstOptimizationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Burst implementation difficulty levels.
    /// </summary>
    public enum BurstImplementationDifficulty
    {
        Easy,
        Medium,
        Hard,
        VeryHard
    }
    
    /// <summary>
    /// Burst validation quality metrics.
    /// </summary>
    [Serializable]
    public struct BurstValidationQualityMetrics
    {
        /// <summary>
        /// Overall validation quality score (0-100).
        /// </summary>
        public float OverallQualityScore;
        
        /// <summary>
        /// Validation completeness percentage.
        /// </summary>
        public float CompletenessPercent;
        
        /// <summary>
        /// Validation accuracy score (0-100).
        /// </summary>
        public float AccuracyScore;
        
        /// <summary>
        /// Validation reliability score (0-100).
        /// </summary>
        public float ReliabilityScore;
        
        /// <summary>
        /// Number of validation errors.
        /// </summary>
        public int ValidationErrors;
        
        /// <summary>
        /// Quality assessment notes.
        /// </summary>
        public string[] QualityNotes;
        
        /// <summary>
        /// Quality improvement recommendations.
        /// </summary>
        public string[] QualityRecommendations;
    }
}