using Unity.Mathematics;
using System.Collections.Generic;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Interface for validating wave parameters for stability and performance.
    /// Ensures wave settings are safe for Quest 3 VR and produce stable results.
    /// Implements Requirement 5.1: Mathematical accuracy and stability validation.
    /// Implements Requirement 5.2: Performance validation for Quest 3.
    /// </summary>
    public interface IWaveParameterValidator
    {
        /// <summary>
        /// Validate wave matrix settings for stability and performance.
        /// </summary>
        /// <param name="settings">Wave matrix settings to validate</param>
        /// <returns>Validation result with detailed feedback</returns>
        WaveParameterValidationResult ValidateSettings(WaveMatrixSettings settings);
        
        /// <summary>
        /// Validate individual wave parameters for mathematical stability.
        /// </summary>
        /// <param name="amplitude">Wave amplitude</param>
        /// <param name="frequency">Wave frequency</param>
        /// <param name="speed">Wave speed</param>
        /// <param name="cellSize">Grid cell size</param>
        /// <returns>Validation result for individual parameters</returns>
        WaveParameterValidationResult ValidateWaveParameters(float amplitude, float frequency, float speed, float cellSize);
        
        /// <summary>
        /// Validate grid settings for performance and memory constraints.
        /// </summary>
        /// <param name="gridSize">Grid dimensions</param>
        /// <param name="cellSize">Size of each grid cell</param>
        /// <param name="bubbleCount">Expected number of bubbles</param>
        /// <returns>Validation result for grid configuration</returns>
        WaveParameterValidationResult ValidateGridSettings(int2 gridSize, float cellSize, int bubbleCount);
        
        /// <summary>
        /// Validate performance characteristics for Quest 3 hardware.
        /// </summary>
        /// <param name="settings">Wave matrix settings</param>
        /// <param name="expectedBubbleCount">Expected number of bubbles</param>
        /// <param name="targetFrameTimeMs">Target frame time in milliseconds</param>
        /// <returns>Performance validation result</returns>
        WavePerformanceValidationResult ValidatePerformance(WaveMatrixSettings settings, int expectedBubbleCount, float targetFrameTimeMs);
        
        /// <summary>
        /// Check if settings will produce stable wave animations without artifacts.
        /// </summary>
        /// <param name="settings">Wave matrix settings to check</param>
        /// <param name="timeRange">Time range to test stability over</param>
        /// <returns>Stability validation result</returns>
        WaveStabilityValidationResult ValidateStability(WaveMatrixSettings settings, float timeRange = 10f);
        
        /// <summary>
        /// Get recommended safe parameter ranges for Quest 3.
        /// </summary>
        /// <returns>Recommended parameter ranges</returns>
        WaveParameterRanges GetRecommendedRanges();
        
        /// <summary>
        /// Suggest corrected parameters if validation fails.
        /// </summary>
        /// <param name="settings">Original settings that failed validation</param>
        /// <param name="validationResult">Validation result with issues</param>
        /// <returns>Suggested corrected settings</returns>
        WaveMatrixSettings SuggestCorrections(WaveMatrixSettings settings, WaveParameterValidationResult validationResult);
        
        /// <summary>
        /// Validate parameter combinations for potential conflicts or instabilities.
        /// </summary>
        /// <param name="settings">Wave matrix settings</param>
        /// <returns>Validation result for parameter interactions</returns>
        WaveParameterValidationResult ValidateParameterInteractions(WaveMatrixSettings settings);
        
        /// <summary>
        /// Check if parameters are within safe bounds to prevent numerical issues.
        /// </summary>
        /// <param name="settings">Wave matrix settings</param>
        /// <returns>Safety bounds validation result</returns>
        WaveParameterValidationResult ValidateSafetyBounds(WaveMatrixSettings settings);
    }
    
    /// <summary>
    /// Result of wave parameter validation.
    /// </summary>
    public struct WaveParameterValidationResult
    {
        /// <summary>
        /// Whether the parameters are valid.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// Severity level of any issues found.
        /// </summary>
        public ValidationSeverity Severity;
        
        /// <summary>
        /// List of validation issues found.
        /// </summary>
        public ValidationIssue[] Issues;
        
        /// <summary>
        /// Overall validation score (0.0 = invalid, 1.0 = perfect).
        /// </summary>
        public float ValidationScore;
        
        /// <summary>
        /// Detailed validation report.
        /// </summary>
        public string ValidationReport;
        
        /// <summary>
        /// Recommended actions to fix issues.
        /// </summary>
        public string[] RecommendedActions;
        
        /// <summary>
        /// Create a successful validation result.
        /// </summary>
        public static WaveParameterValidationResult Success(float score = 1.0f, string report = "All parameters valid")
        {
            return new WaveParameterValidationResult
            {
                IsValid = true,
                Severity = ValidationSeverity.None,
                Issues = new ValidationIssue[0],
                ValidationScore = score,
                ValidationReport = report,
                RecommendedActions = new string[0]
            };
        }
        
        /// <summary>
        /// Create a failed validation result.
        /// </summary>
        public static WaveParameterValidationResult Failure(ValidationSeverity severity, ValidationIssue[] issues, string report)
        {
            return new WaveParameterValidationResult
            {
                IsValid = false,
                Severity = severity,
                Issues = issues ?? new ValidationIssue[0],
                ValidationScore = 0.0f,
                ValidationReport = report,
                RecommendedActions = new string[0]
            };
        }
    }
    
    /// <summary>
    /// Result of wave performance validation.
    /// </summary>
    public struct WavePerformanceValidationResult
    {
        /// <summary>
        /// Whether performance requirements are met.
        /// </summary>
        public bool MeetsPerformanceTarget;
        
        /// <summary>
        /// Estimated frame time in milliseconds.
        /// </summary>
        public float EstimatedFrameTimeMs;
        
        /// <summary>
        /// Target frame time in milliseconds.
        /// </summary>
        public float TargetFrameTimeMs;
        
        /// <summary>
        /// Performance margin (positive = under target, negative = over target).
        /// </summary>
        public float PerformanceMarginMs;
        
        /// <summary>
        /// Estimated memory usage in bytes.
        /// </summary>
        public long EstimatedMemoryUsage;
        
        /// <summary>
        /// Performance bottlenecks identified.
        /// </summary>
        public string[] PerformanceBottlenecks;
        
        /// <summary>
        /// Optimization suggestions.
        /// </summary>
        public string[] OptimizationSuggestions;
        
        /// <summary>
        /// Overall performance rating (0.0 = poor, 1.0 = excellent).
        /// </summary>
        public float PerformanceRating;
    }
    
    /// <summary>
    /// Result of wave stability validation.
    /// </summary>
    public struct WaveStabilityValidationResult
    {
        /// <summary>
        /// Whether the wave system is stable.
        /// </summary>
        public bool IsStable;
        
        /// <summary>
        /// Maximum position deviation observed.
        /// </summary>
        public float MaxPositionDeviation;
        
        /// <summary>
        /// Time range tested for stability.
        /// </summary>
        public float TestedTimeRange;
        
        /// <summary>
        /// Stability issues found.
        /// </summary>
        public string[] StabilityIssues;
        
        /// <summary>
        /// Stability score (0.0 = unstable, 1.0 = perfectly stable).
        /// </summary>
        public float StabilityScore;
        
        /// <summary>
        /// Whether numerical overflow/underflow was detected.
        /// </summary>
        public bool NumericalIssuesDetected;
        
        /// <summary>
        /// Whether wave patterns show unwanted artifacts.
        /// </summary>
        public bool ArtifactsDetected;
    }
    
    /// <summary>
    /// Recommended parameter ranges for Quest 3.
    /// </summary>
    public struct WaveParameterRanges
    {
        /// <summary>
        /// Recommended wave amplitude range.
        /// </summary>
        public FloatRange AmplitudeRange;
        
        /// <summary>
        /// Recommended wave frequency range.
        /// </summary>
        public FloatRange FrequencyRange;
        
        /// <summary>
        /// Recommended wave speed range.
        /// </summary>
        public FloatRange SpeedRange;
        
        /// <summary>
        /// Recommended grid cell size range.
        /// </summary>
        public FloatRange CellSizeRange;
        
        /// <summary>
        /// Recommended grid size range.
        /// </summary>
        public IntRange GridSizeRange;
        
        /// <summary>
        /// Maximum recommended bubble count.
        /// </summary>
        public int MaxRecommendedBubbles;
        
        /// <summary>
        /// Default safe ranges for Quest 3.
        /// </summary>
        public static WaveParameterRanges Quest3Safe => new WaveParameterRanges
        {
            AmplitudeRange = new FloatRange(0.1f, 2.0f),
            FrequencyRange = new FloatRange(0.5f, 5.0f),
            SpeedRange = new FloatRange(0.5f, 3.0f),
            CellSizeRange = new FloatRange(0.5f, 2.0f),
            GridSizeRange = new IntRange(10, 50),
            MaxRecommendedBubbles = 100
        };
    }
    
    /// <summary>
    /// Validation issue details.
    /// </summary>
    public struct ValidationIssue
    {
        /// <summary>
        /// Type of validation issue.
        /// </summary>
        public ValidationIssueType Type;
        
        /// <summary>
        /// Parameter that caused the issue.
        /// </summary>
        public string ParameterName;
        
        /// <summary>
        /// Current value that's problematic.
        /// </summary>
        public float CurrentValue;
        
        /// <summary>
        /// Recommended value or range.
        /// </summary>
        public string RecommendedValue;
        
        /// <summary>
        /// Description of the issue.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Severity of the issue.
        /// </summary>
        public ValidationSeverity Severity;
    }
    
    /// <summary>
    /// Types of validation issues.
    /// </summary>
    public enum ValidationIssueType
    {
        OutOfRange,
        PerformanceImpact,
        StabilityRisk,
        MemoryExcessive,
        NumericalInstability,
        ParameterConflict,
        SafetyViolation
    }
    
    /// <summary>
    /// Validation severity levels.
    /// </summary>
    public enum ValidationSeverity
    {
        None,
        Info,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>
    /// Float value range.
    /// </summary>
    public struct FloatRange
    {
        public float Min;
        public float Max;
        
        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
        
        public bool Contains(float value) => value >= Min && value <= Max;
        public float Clamp(float value) => math.clamp(value, Min, Max);
    }
    
    /// <summary>
    /// Integer value range.
    /// </summary>
    public struct IntRange
    {
        public int Min;
        public int Max;
        
        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }
        
        public bool Contains(int value) => value >= Min && value <= Max;
        public int Clamp(int value) => math.clamp(value, Min, Max);
    }
}