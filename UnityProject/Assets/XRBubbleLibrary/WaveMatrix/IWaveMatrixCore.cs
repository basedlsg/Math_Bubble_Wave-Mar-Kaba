using Unity.Mathematics;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Interface for the core wave mathematics system.
    /// Provides dependency injection support and testability for wave calculations.
    /// Implements Requirement 5.1: Core wave mathematics for bubble positioning.
    /// </summary>
    public interface IWaveMatrixCore
    {
        /// <summary>
        /// Calculate wave-based position for a bubble at given index and time.
        /// </summary>
        /// <param name="bubbleIndex">Index of the bubble in the grid</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="settings">Wave matrix settings to use for calculation</param>
        /// <returns>3D position with wave-based Y displacement</returns>
        float3 CalculateWavePosition(int bubbleIndex, float time, WaveMatrixSettings settings);
        
        /// <summary>
        /// Calculate wave-based position with additional distance offset.
        /// </summary>
        /// <param name="bubbleIndex">Index of the bubble in the grid</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="distanceOffset">Additional Z-distance offset (e.g., from AI confidence)</param>
        /// <param name="settings">Wave matrix settings to use for calculation</param>
        /// <returns>3D position with wave-based Y displacement and distance offset</returns>
        float3 CalculateWavePosition(int bubbleIndex, float time, float distanceOffset, WaveMatrixSettings settings);
        
        /// <summary>
        /// Calculate wave height at a specific world position and time.
        /// </summary>
        /// <param name="worldPosition">World position to calculate wave height for</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="settings">Wave matrix settings to use for calculation</param>
        /// <returns>Wave height at the given position</returns>
        float CalculateWaveHeight(float2 worldPosition, float time, WaveMatrixSettings settings);
        
        /// <summary>
        /// Calculate multiple wave positions efficiently in batch.
        /// </summary>
        /// <param name="bubbleIndices">Array of bubble indices to calculate</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="settings">Wave matrix settings to use for calculation</param>
        /// <param name="results">Output array for calculated positions (must be same length as bubbleIndices)</param>
        void CalculateWavePositionsBatch(int[] bubbleIndices, float time, WaveMatrixSettings settings, float3[] results);
        
        /// <summary>
        /// Calculate multiple wave positions with distance offsets efficiently in batch.
        /// </summary>
        /// <param name="bubbleIndices">Array of bubble indices to calculate</param>
        /// <param name="time">Current time for wave animation</param>
        /// <param name="distanceOffsets">Array of distance offsets for each bubble</param>
        /// <param name="settings">Wave matrix settings to use for calculation</param>
        /// <param name="results">Output array for calculated positions (must be same length as bubbleIndices)</param>
        void CalculateWavePositionsBatch(int[] bubbleIndices, float time, float[] distanceOffsets, WaveMatrixSettings settings, float3[] results);
        
        /// <summary>
        /// Update wave state for time-based animations.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update</param>
        /// <param name="settings">Wave matrix settings to use for time scaling</param>
        /// <returns>Updated time value for wave calculations</returns>
        float UpdateWaveTime(float deltaTime, WaveMatrixSettings settings);
        
        /// <summary>
        /// Validate wave matrix settings for performance and correctness.
        /// </summary>
        /// <param name="settings">Settings to validate</param>
        /// <returns>Validation result with any issues found</returns>
        WaveMatrixValidationResult ValidateSettings(WaveMatrixSettings settings);
        
        /// <summary>
        /// Get grid position for a bubble index without wave displacement.
        /// </summary>
        /// <param name="bubbleIndex">Index of the bubble in the grid</param>
        /// <param name="settings">Wave matrix settings for grid layout</param>
        /// <returns>Base grid position (X, 0, Z)</returns>
        float3 GetGridPosition(int bubbleIndex, WaveMatrixSettings settings);
        
        /// <summary>
        /// Convert world position to grid index (useful for hit testing).
        /// </summary>
        /// <param name="worldPosition">World position to convert</param>
        /// <param name="settings">Wave matrix settings for grid layout</param>
        /// <returns>Grid index, or -1 if outside grid bounds</returns>
        int WorldPositionToGridIndex(float3 worldPosition, WaveMatrixSettings settings);
    }
    
    /// <summary>
    /// Result of wave matrix settings validation.
    /// </summary>
    public struct WaveMatrixValidationResult
    {
        /// <summary>
        /// Whether the settings are valid for use.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// List of validation issues found.
        /// </summary>
        public string[] Issues;
        
        /// <summary>
        /// List of performance warnings.
        /// </summary>
        public string[] Warnings;
        
        /// <summary>
        /// Estimated performance impact (0.0 = minimal, 1.0 = maximum).
        /// </summary>
        public float PerformanceImpact;
        
        /// <summary>
        /// Create a valid result with no issues.
        /// </summary>
        public static WaveMatrixValidationResult Valid => new WaveMatrixValidationResult
        {
            IsValid = true,
            Issues = new string[0],
            Warnings = new string[0],
            PerformanceImpact = 0.0f
        };
        
        /// <summary>
        /// Create an invalid result with specified issues.
        /// </summary>
        public static WaveMatrixValidationResult Invalid(params string[] issues) => new WaveMatrixValidationResult
        {
            IsValid = false,
            Issues = issues,
            Warnings = new string[0],
            PerformanceImpact = 1.0f
        };
    }
}