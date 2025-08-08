using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

namespace XRBubbleLibrary.WaveMatrix.Tests
{
    /// <summary>
    /// Comprehensive tests for WaveParameterValidator.
    /// Validates parameter validation, stability checking, and Quest 3 performance validation.
    /// Implements Requirement 5.1: Mathematical accuracy and stability validation.
    /// Implements Requirement 5.2: Performance validation for Quest 3.
    /// </summary>
    public class WaveParameterValidatorTests
    {
        private IWaveParameterValidator _validator;
        private WaveMatrixSettings _validSettings;
        
        [SetUp]
        public void SetUp()
        {
            _validator = new WaveParameterValidator();
            _validSettings = WaveMatrixSettings.Default;
        }
        
        [TearDown]
        public void TearDown()
        {
            _validator = null;
            _validSettings = null;
        }
        
        #region Basic Validation Tests
        
        [Test]
        public void ValidateSettings_WithValidSettings_ReturnsSuccess()
        {
            // Act
            var result = _validator.ValidateSettings(_validSettings);
            
            // Assert
            Assert.IsTrue(result.IsValid, "Valid settings should pass validation");
            Assert.AreEqual(ValidationSeverity.None, result.Severity, "Should have no severity issues");
            Assert.Greater(result.ValidationScore, 0.8f, "Should have high validation score");
            Assert.IsNotNull(result.ValidationReport, "Should provide validation report");
            
            UnityEngine.Debug.Log($"[Validation] Valid settings score: {result.ValidationScore:F2}");
        }
        
        [Test]
        public void ValidateSettings_WithInvalidAmplitude_ReturnsFailure()
        {
            // Arrange
            var invalidSettings = _validSettings;
            invalidSettings.WaveAmplitude = -1f; // Invalid negative amplitude
            
            // Act
            var result = _validator.ValidateSettings(invalidSettings);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Invalid amplitude should fail validation");
            Assert.GreaterOrEqual(result.Severity, ValidationSeverity.Error, "Should have error severity");
            Assert.Greater(result.Issues.Length, 0, "Should report issues");
            Assert.Contains("WaveAmplitude", result.Issues.Select(i => i.ParameterName).ToArray(), 
                "Should identify amplitude as problematic parameter");
        }
        
        [Test]
        public void ValidateSettings_WithExtremeFrequency_ReturnsWarning()
        {
            // Arrange
            var extremeSettings = _validSettings;
            extremeSettings.WaveFrequency = 50f; // Very high frequency
            
            // Act
            var result = _validator.ValidateSettings(extremeSettings);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Extreme frequency should fail validation");
            Assert.GreaterOrEqual(result.Severity, ValidationSeverity.Warning, "Should have warning or error");
            Assert.Greater(result.Issues.Length, 0, "Should report frequency issues");
            
            var frequencyIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "WaveFrequency");
            Assert.IsNotNull(frequencyIssue, "Should identify frequency issue");
            Assert.AreEqual(ValidationIssueType.PerformanceImpact, frequencyIssue.Type, 
                "High frequency should be performance issue");
        }
        
        #endregion
        
        #region Individual Parameter Validation Tests
        
        [Test]
        public void ValidateWaveParameters_WithValidParameters_ReturnsSuccess()
        {
            // Act
            var result = _validator.ValidateWaveParameters(1.0f, 2.0f, 1.5f, 1.0f);
            
            // Assert
            Assert.IsTrue(result.IsValid, "Valid parameters should pass");
            Assert.AreEqual(ValidationSeverity.None, result.Severity, "Should have no issues");
            Assert.AreEqual(1.0f, result.ValidationScore, 0.01f, "Should have perfect score");
        }
        
        [Test]
        public void ValidateWaveParameters_WithZeroAmplitude_ReturnsWarning()
        {
            // Act
            var result = _validator.ValidateWaveParameters(0f, 2.0f, 1.5f, 1.0f);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Zero amplitude should fail validation");
            Assert.GreaterOrEqual(result.Severity, ValidationSeverity.Warning, "Should have warning");
            Assert.Less(result.ValidationScore, 1.0f, "Should have reduced score");
            
            var amplitudeIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "WaveAmplitude");
            Assert.IsNotNull(amplitudeIssue, "Should identify amplitude issue");
            Assert.AreEqual(ValidationIssueType.StabilityRisk, amplitudeIssue.Type, 
                "Zero amplitude should be stability risk");
        }
        
        [Test]
        public void ValidateWaveParameters_WithNegativeFrequency_ReturnsError()
        {
            // Act
            var result = _validator.ValidateWaveParameters(1.0f, -1.0f, 1.5f, 1.0f);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Negative frequency should fail validation");
            Assert.AreEqual(ValidationSeverity.Error, result.Severity, "Should have error severity");
            Assert.AreEqual(0.5f, result.ValidationScore, 0.1f, "Should have significantly reduced score");
        }
        
        [Test]
        public void ValidateWaveParameters_WithExcessiveSpeed_ReturnsWarning()
        {
            // Act
            var result = _validator.ValidateWaveParameters(1.0f, 2.0f, 15f, 1.0f);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Excessive speed should fail validation");
            Assert.GreaterOrEqual(result.Severity, ValidationSeverity.Warning, "Should have warning");
            
            var speedIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "WaveSpeed");
            Assert.IsNotNull(speedIssue, "Should identify speed issue");
            Assert.That(speedIssue.Description, Does.Contain("motion sickness"), 
                "Should warn about VR motion sickness");
        }
        
        #endregion
        
        #region Grid Validation Tests
        
        [Test]
        public void ValidateGridSettings_WithValidGrid_ReturnsSuccess()
        {
            // Act
            var result = _validator.ValidateGridSettings(new int2(20, 20), 1.0f, 50);
            
            // Assert
            Assert.IsTrue(result.IsValid, "Valid grid should pass validation");
            Assert.AreEqual(ValidationSeverity.None, result.Severity, "Should have no issues");
            Assert.Greater(result.ValidationScore, 0.8f, "Should have high score");
        }
        
        [Test]
        public void ValidateGridSettings_WithZeroGridSize_ReturnsError()
        {
            // Act
            var result = _validator.ValidateGridSettings(new int2(0, 10), 1.0f, 50);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Zero grid dimension should fail");
            Assert.AreEqual(ValidationSeverity.Critical, result.Severity, "Should be critical error");
            Assert.AreEqual(0f, result.ValidationScore, "Should have zero score");
            
            var gridIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "GridSize");
            Assert.IsNotNull(gridIssue, "Should identify grid size issue");
            Assert.AreEqual(ValidationIssueType.SafetyViolation, gridIssue.Type, 
                "Zero grid should be safety violation");
        }
        
        [Test]
        public void ValidateGridSettings_WithTooManyBubbles_ReturnsError()
        {
            // Arrange - More bubbles than grid cells
            var gridSize = new int2(10, 10); // 100 cells
            int bubbleCount = 150; // More than available cells
            
            // Act
            var result = _validator.ValidateGridSettings(gridSize, 1.0f, bubbleCount);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Too many bubbles should fail validation");
            Assert.GreaterOrEqual(result.Severity, ValidationSeverity.Error, "Should have error severity");
            
            var bubbleIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "BubbleCount");
            Assert.IsNotNull(bubbleIssue, "Should identify bubble count issue");
            Assert.AreEqual(ValidationIssueType.ParameterConflict, bubbleIssue.Type, 
                "Should be parameter conflict");
        }
        
        [Test]
        public void ValidateGridSettings_WithLargeGrid_ReturnsPerformanceWarning()
        {
            // Arrange - Very large grid
            var largeGrid = new int2(200, 200); // 40,000 cells (way over limit)
            
            // Act
            var result = _validator.ValidateGridSettings(largeGrid, 1.0f, 100);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Large grid should fail validation");
            Assert.GreaterOrEqual(result.Severity, ValidationSeverity.Error, "Should have error severity");
            
            var gridIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "GridSize");
            Assert.IsNotNull(gridIssue, "Should identify grid size issue");
            Assert.AreEqual(ValidationIssueType.PerformanceImpact, gridIssue.Type, 
                "Large grid should be performance issue");
        }
        
        #endregion
        
        #region Performance Validation Tests
        
        [Test]
        public void ValidatePerformance_WithOptimalSettings_MeetsTarget()
        {
            // Arrange
            const float targetFrameTime = 13.89f; // Quest 3 target
            const int bubbleCount = 100;
            
            // Act
            var result = _validator.ValidatePerformance(_validSettings, bubbleCount, targetFrameTime);
            
            // Assert
            Assert.IsTrue(result.MeetsPerformanceTarget, "Optimal settings should meet performance target");
            Assert.Less(result.EstimatedFrameTimeMs, targetFrameTime, "Should estimate under target time");
            Assert.Greater(result.PerformanceMarginMs, 0f, "Should have positive performance margin");
            Assert.Greater(result.PerformanceRating, 0.5f, "Should have decent performance rating");
            
            UnityEngine.Debug.Log($"[Performance] Estimated: {result.EstimatedFrameTimeMs:F2}ms, " +
                                $"Target: {result.TargetFrameTimeMs:F2}ms, Rating: {result.PerformanceRating:F2}");
        }
        
        [Test]
        public void ValidatePerformance_WithHeavySettings_IdentifiesBottlenecks()
        {
            // Arrange - Heavy settings that should cause performance issues
            var heavySettings = _validSettings;
            heavySettings.GridSize = new int2(100, 100); // Large grid
            heavySettings.WaveFrequency = 15f; // High frequency
            
            const float targetFrameTime = 13.89f;
            const int bubbleCount = 200; // Many bubbles
            
            // Act
            var result = _validator.ValidatePerformance(heavySettings, bubbleCount, targetFrameTime);
            
            // Assert
            Assert.IsFalse(result.MeetsPerformanceTarget, "Heavy settings should not meet target");
            Assert.Greater(result.EstimatedFrameTimeMs, targetFrameTime, "Should estimate over target time");
            Assert.Less(result.PerformanceMarginMs, 0f, "Should have negative performance margin");
            Assert.Greater(result.PerformanceBottlenecks.Length, 0, "Should identify bottlenecks");
            Assert.Greater(result.OptimizationSuggestions.Length, 0, "Should provide optimization suggestions");
            
            UnityEngine.Debug.Log($"[Performance] Heavy settings bottlenecks: {string.Join(", ", result.PerformanceBottlenecks)}");
        }
        
        #endregion
        
        #region Stability Validation Tests
        
        [Test]
        public void ValidateStability_WithStableSettings_ReturnsStable()
        {
            // Act
            var result = _validator.ValidateStability(_validSettings, 5f);
            
            // Assert
            Assert.IsTrue(result.IsStable, "Valid settings should be stable");
            Assert.IsFalse(result.NumericalIssuesDetected, "Should not detect numerical issues");
            Assert.IsFalse(result.ArtifactsDetected, "Should not detect artifacts");
            Assert.Greater(result.StabilityScore, 0.8f, "Should have high stability score");
            Assert.AreEqual(5f, result.TestedTimeRange, "Should test specified time range");
            
            UnityEngine.Debug.Log($"[Stability] Score: {result.StabilityScore:F2}, " +
                                $"Max deviation: {result.MaxPositionDeviation:F2}");
        }
        
        [Test]
        public void ValidateStability_WithExtremeAmplitude_DetectsInstability()
        {
            // Arrange
            var unstableSettings = _validSettings;
            unstableSettings.WaveAmplitude = 100f; // Extreme amplitude
            
            // Act
            var result = _validator.ValidateStability(unstableSettings, 2f);
            
            // Assert
            Assert.IsFalse(result.IsStable, "Extreme amplitude should be unstable");
            Assert.Greater(result.MaxPositionDeviation, unstableSettings.WaveAmplitude, 
                "Should detect excessive deviation");
            Assert.Less(result.StabilityScore, 0.8f, "Should have reduced stability score");
            Assert.Greater(result.StabilityIssues.Length, 0, "Should report stability issues");
        }
        
        [Test]
        public void ValidateStability_WithHighFrequency_DetectsAliasing()
        {
            // Arrange - High frequency that causes aliasing
            var aliasingSettings = _validSettings;
            aliasingSettings.WaveFrequency = 20f; // Very high frequency
            aliasingSettings.CellSize = 0.1f; // Small cell size
            
            // Act
            var result = _validator.ValidateStability(aliasingSettings, 1f);
            
            // Assert
            Assert.IsFalse(result.IsStable, "High frequency should cause instability");
            Assert.IsTrue(result.ArtifactsDetected, "Should detect aliasing artifacts");
            Assert.Greater(result.StabilityIssues.Length, 0, "Should report aliasing issues");
            
            var aliasingIssue = result.StabilityIssues.FirstOrDefault(issue => issue.Contains("Nyquist"));
            Assert.IsNotNull(aliasingIssue, "Should mention Nyquist limit in issues");
        }
        
        #endregion
        
        #region Parameter Interaction Tests
        
        [Test]
        public void ValidateParameterInteractions_WithGoodCombination_ReturnsValid()
        {
            // Act
            var result = _validator.ValidateParameterInteractions(_validSettings);
            
            // Assert
            Assert.IsTrue(result.IsValid, "Good parameter combination should be valid");
            Assert.AreEqual(ValidationSeverity.None, result.Severity, "Should have no issues");
            Assert.Greater(result.ValidationScore, 0.8f, "Should have high score");
        }
        
        [Test]
        public void ValidateParameterInteractions_WithPoorResolution_ReturnsWarning()
        {
            // Arrange - High frequency with large cell size (poor resolution)
            var poorResolutionSettings = _validSettings;
            poorResolutionSettings.WaveFrequency = 10f; // High frequency
            poorResolutionSettings.CellSize = 2f; // Large cell size
            
            // Act
            var result = _validator.ValidateParameterInteractions(poorResolutionSettings);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Poor resolution should fail validation");
            Assert.GreaterOrEqual(result.Severity, ValidationSeverity.Warning, "Should have warning");
            
            var resolutionIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "FrequencyVsCellSize");
            Assert.IsNotNull(resolutionIssue, "Should identify resolution issue");
            Assert.AreEqual(ValidationIssueType.ParameterConflict, resolutionIssue.Type, 
                "Should be parameter conflict");
        }
        
        [Test]
        public void ValidateParameterInteractions_WithHighMotionIntensity_ReturnsWarning()
        {
            // Arrange - High amplitude + high speed = motion sickness risk
            var highMotionSettings = _validSettings;
            highMotionSettings.WaveAmplitude = 3f;
            highMotionSettings.WaveSpeed = 3f; // Combined intensity = 9 (over threshold)
            
            // Act
            var result = _validator.ValidateParameterInteractions(highMotionSettings);
            
            // Assert
            Assert.IsFalse(result.IsValid, "High motion intensity should fail validation");
            Assert.GreaterOrEqual(result.Severity, ValidationSeverity.Warning, "Should have warning");
            
            var motionIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "AmplitudeVsSpeed");
            Assert.IsNotNull(motionIssue, "Should identify motion intensity issue");
            Assert.That(motionIssue.Description, Does.Contain("motion sickness"), 
                "Should mention motion sickness risk");
        }
        
        #endregion
        
        #region Safety Bounds Tests
        
        [Test]
        public void ValidateSafetyBounds_WithValidBounds_ReturnsValid()
        {
            // Act
            var result = _validator.ValidateSafetyBounds(_validSettings);
            
            // Assert
            Assert.IsTrue(result.IsValid, "Valid bounds should pass validation");
            Assert.AreEqual(ValidationSeverity.None, result.Severity, "Should have no issues");
            Assert.AreEqual(1.0f, result.ValidationScore, "Should have perfect score");
        }
        
        [Test]
        public void ValidateSafetyBounds_WithNaNValues_ReturnsCritical()
        {
            // Arrange
            var nanSettings = _validSettings;
            nanSettings.WaveAmplitude = float.NaN;
            
            // Act
            var result = _validator.ValidateSafetyBounds(nanSettings);
            
            // Assert
            Assert.IsFalse(result.IsValid, "NaN values should fail validation");
            Assert.AreEqual(ValidationSeverity.Critical, result.Severity, "Should be critical error");
            Assert.AreEqual(0f, result.ValidationScore, "Should have zero score");
            
            var nanIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "WaveAmplitude");
            Assert.IsNotNull(nanIssue, "Should identify NaN amplitude");
            Assert.AreEqual(ValidationIssueType.SafetyViolation, nanIssue.Type, 
                "NaN should be safety violation");
        }
        
        [Test]
        public void ValidateSafetyBounds_WithInfiniteValues_ReturnsCritical()
        {
            // Arrange
            var infiniteSettings = _validSettings;
            infiniteSettings.WaveFrequency = float.PositiveInfinity;
            
            // Act
            var result = _validator.ValidateSafetyBounds(infiniteSettings);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Infinite values should fail validation");
            Assert.AreEqual(ValidationSeverity.Critical, result.Severity, "Should be critical error");
            Assert.AreEqual(0f, result.ValidationScore, "Should have zero score");
        }
        
        [Test]
        public void ValidateSafetyBounds_WithNegativeValues_ReturnsError()
        {
            // Arrange
            var negativeSettings = _validSettings;
            negativeSettings.CellSize = -1f; // Negative cell size
            
            // Act
            var result = _validator.ValidateSafetyBounds(negativeSettings);
            
            // Assert
            Assert.IsFalse(result.IsValid, "Negative cell size should fail validation");
            Assert.AreEqual(ValidationSeverity.Error, result.Severity, "Should have error severity");
            Assert.Less(result.ValidationScore, 1f, "Should have reduced score");
            
            var negativeIssue = result.Issues.FirstOrDefault(i => i.ParameterName == "CellSize");
            Assert.IsNotNull(negativeIssue, "Should identify negative cell size");
            Assert.AreEqual(ValidationIssueType.SafetyViolation, negativeIssue.Type, 
                "Negative value should be safety violation");
        }
        
        #endregion
        
        #region Correction Suggestions Tests
        
        [Test]
        public void SuggestCorrections_WithInvalidSettings_ReturnsValidSettings()
        {
            // Arrange
            var invalidSettings = _validSettings;
            invalidSettings.WaveAmplitude = 50f; // Too high
            invalidSettings.WaveFrequency = 100f; // Too high
            invalidSettings.WaveSpeed = 50f; // Too high
            
            var validationResult = _validator.ValidateSettings(invalidSettings);
            
            // Act
            var correctedSettings = _validator.SuggestCorrections(invalidSettings, validationResult);
            
            // Assert
            Assert.Less(correctedSettings.WaveAmplitude, invalidSettings.WaveAmplitude, 
                "Should reduce excessive amplitude");
            Assert.Less(correctedSettings.WaveFrequency, invalidSettings.WaveFrequency, 
                "Should reduce excessive frequency");
            Assert.Less(correctedSettings.WaveSpeed, invalidSettings.WaveSpeed, 
                "Should reduce excessive speed");
            
            // Verify corrected settings are valid
            var correctedValidation = _validator.ValidateSettings(correctedSettings);
            Assert.IsTrue(correctedValidation.IsValid, "Corrected settings should be valid");
            
            UnityEngine.Debug.Log($"[Corrections] Original amplitude: {invalidSettings.WaveAmplitude:F2} -> " +
                                $"Corrected: {correctedSettings.WaveAmplitude:F2}");
        }
        
        #endregion
        
        #region Recommended Ranges Tests
        
        [Test]
        public void GetRecommendedRanges_ReturnsValidRanges()
        {
            // Act
            var ranges = _validator.GetRecommendedRanges();
            
            // Assert
            Assert.Greater(ranges.AmplitudeRange.Max, ranges.AmplitudeRange.Min, 
                "Amplitude range should be valid");
            Assert.Greater(ranges.FrequencyRange.Max, ranges.FrequencyRange.Min, 
                "Frequency range should be valid");
            Assert.Greater(ranges.SpeedRange.Max, ranges.SpeedRange.Min, 
                "Speed range should be valid");
            Assert.Greater(ranges.CellSizeRange.Max, ranges.CellSizeRange.Min, 
                "Cell size range should be valid");
            Assert.Greater(ranges.GridSizeRange.Max, ranges.GridSizeRange.Min, 
                "Grid size range should be valid");
            Assert.Greater(ranges.MaxRecommendedBubbles, 0, 
                "Should have positive max bubble count");
            
            UnityEngine.Debug.Log($"[Ranges] Amplitude: {ranges.AmplitudeRange.Min}-{ranges.AmplitudeRange.Max}, " +
                                $"Frequency: {ranges.FrequencyRange.Min}-{ranges.FrequencyRange.Max}, " +
                                $"Max bubbles: {ranges.MaxRecommendedBubbles}");
        }
        
        [Test]
        public void RecommendedRanges_ContainDefaultSettings()
        {
            // Act
            var ranges = _validator.GetRecommendedRanges();
            
            // Assert
            Assert.IsTrue(ranges.AmplitudeRange.Contains(_validSettings.WaveAmplitude), 
                "Default amplitude should be in recommended range");
            Assert.IsTrue(ranges.FrequencyRange.Contains(_validSettings.WaveFrequency), 
                "Default frequency should be in recommended range");
            Assert.IsTrue(ranges.SpeedRange.Contains(_validSettings.WaveSpeed), 
                "Default speed should be in recommended range");
            Assert.IsTrue(ranges.CellSizeRange.Contains(_validSettings.CellSize), 
                "Default cell size should be in recommended range");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [Test]
        public void ValidateSettings_WithMinimalSettings_HandlesGracefully()
        {
            // Arrange - Minimal but valid settings
            var minimalSettings = new WaveMatrixSettings
            {
                GridSize = new int2(1, 1),
                CellSize = 1f,
                WaveAmplitude = 0.1f,
                WaveFrequency = 0.5f,
                WaveSpeed = 0.5f
            };
            
            // Act
            var result = _validator.ValidateSettings(minimalSettings);
            
            // Assert
            Assert.IsNotNull(result, "Should handle minimal settings");
            Assert.IsNotNull(result.ValidationReport, "Should provide validation report");
            Assert.GreaterOrEqual(result.ValidationScore, 0f, "Should have non-negative score");
        }
        
        [Test]
        public void ValidateStability_WithZeroTimeRange_HandlesGracefully()
        {
            // Act
            var result = _validator.ValidateStability(_validSettings, 0f);
            
            // Assert
            Assert.IsNotNull(result, "Should handle zero time range");
            Assert.AreEqual(0f, result.TestedTimeRange, "Should record zero time range");
        }
        
        #endregion
    }
}