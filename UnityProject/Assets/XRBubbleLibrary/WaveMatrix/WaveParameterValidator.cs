using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Validates wave parameters for stability and performance on Quest 3 VR.
    /// Implements Requirement 5.1: Mathematical accuracy and stability validation.
    /// Implements Requirement 5.2: Performance validation for Quest 3.
    /// </summary>
    public class WaveParameterValidator : IWaveParameterValidator
    {
        // Quest 3 hardware constraints
        private const float QUEST3_TARGET_FRAME_TIME_MS = 13.89f; // 72 FPS
        private const long QUEST3_MAX_MEMORY_BYTES = 8L * 1024 * 1024 * 1024; // 8GB total
        private const long WAVE_SYSTEM_MEMORY_BUDGET = 64L * 1024 * 1024; // 64MB budget
        private const int QUEST3_MAX_RECOMMENDED_BUBBLES = 100;
        
        // Mathematical stability constants
        private const float MIN_SAFE_AMPLITUDE = 0.001f;
        private const float MAX_SAFE_AMPLITUDE = 10.0f;
        private const float MIN_SAFE_FREQUENCY = 0.1f;
        private const float MAX_SAFE_FREQUENCY = 20.0f;
        private const float MIN_SAFE_SPEED = 0.1f;
        private const float MAX_SAFE_SPEED = 10.0f;
        private const float MIN_SAFE_CELL_SIZE = 0.1f;
        private const float MAX_SAFE_CELL_SIZE = 10.0f;
        
        // Performance thresholds
        private const int MAX_GRID_CELLS = 10000; // 100x100 max
        private const float PERFORMANCE_WARNING_THRESHOLD = 0.8f; // 80% of frame budget
        
        private readonly WaveParameterRanges _recommendedRanges;
        
        /// <summary>
        /// Initialize the wave parameter validator.
        /// </summary>
        public WaveParameterValidator()
        {
            _recommendedRanges = WaveParameterRanges.Quest3Safe;
        }
        
        /// <summary>
        /// Validate wave matrix settings for stability and performance.
        /// </summary>
        public WaveParameterValidationResult ValidateSettings(WaveMatrixSettings settings)
        {
            var issues = new List<ValidationIssue>();
            var actions = new List<string>();
            float totalScore = 0f;
            int validationCount = 0;
            
            // Validate individual wave parameters
            var waveResult = ValidateWaveParameters(settings.WaveAmplitude, settings.WaveFrequency, settings.WaveSpeed, settings.CellSize);
            if (!waveResult.IsValid)
            {
                issues.AddRange(waveResult.Issues);
                actions.AddRange(waveResult.RecommendedActions);
            }
            totalScore += waveResult.ValidationScore;
            validationCount++;
            
            // Validate grid settings
            var gridResult = ValidateGridSettings(settings.GridSize, settings.CellSize, QUEST3_MAX_RECOMMENDED_BUBBLES);
            if (!gridResult.IsValid)
            {
                issues.AddRange(gridResult.Issues);
                actions.AddRange(gridResult.RecommendedActions);
            }
            totalScore += gridResult.ValidationScore;
            validationCount++;
            
            // Validate parameter interactions
            var interactionResult = ValidateParameterInteractions(settings);
            if (!interactionResult.IsValid)
            {
                issues.AddRange(interactionResult.Issues);
                actions.AddRange(interactionResult.RecommendedActions);
            }
            totalScore += interactionResult.ValidationScore;
            validationCount++;
            
            // Validate safety bounds
            var safetyResult = ValidateSafetyBounds(settings);
            if (!safetyResult.IsValid)
            {
                issues.AddRange(safetyResult.Issues);
                actions.AddRange(safetyResult.RecommendedActions);
            }
            totalScore += safetyResult.ValidationScore;
            validationCount++;
            
            // Calculate overall results
            float averageScore = validationCount > 0 ? totalScore / validationCount : 0f;
            bool isValid = issues.Count == 0 || !issues.Any(i => i.Severity >= ValidationSeverity.Error);
            var severity = issues.Count > 0 ? issues.Max(i => i.Severity) : ValidationSeverity.None;
            
            string report = GenerateValidationReport(settings, issues, averageScore);
            
            return new WaveParameterValidationResult
            {
                IsValid = isValid,
                Severity = severity,
                Issues = issues.ToArray(),
                ValidationScore = averageScore,
                ValidationReport = report,
                RecommendedActions = actions.Distinct().ToArray()
            };
        }
        
        /// <summary>
        /// Validate individual wave parameters for mathematical stability.
        /// </summary>
        public WaveParameterValidationResult ValidateWaveParameters(float amplitude, float frequency, float speed, float cellSize)
        {
            var issues = new List<ValidationIssue>();
            var actions = new List<string>();
            float score = 1.0f;
            
            // Validate amplitude
            if (amplitude < MIN_SAFE_AMPLITUDE)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.StabilityRisk,
                    ParameterName = "WaveAmplitude",
                    CurrentValue = amplitude,
                    RecommendedValue = $">= {MIN_SAFE_AMPLITUDE}",
                    Description = "Wave amplitude too small, may cause numerical precision issues",
                    Severity = ValidationSeverity.Warning
                });
                actions.Add($"Increase wave amplitude to at least {MIN_SAFE_AMPLITUDE}");
                score -= 0.2f;
            }
            else if (amplitude > MAX_SAFE_AMPLITUDE)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.StabilityRisk,
                    ParameterName = "WaveAmplitude",
                    CurrentValue = amplitude,
                    RecommendedValue = $"<= {MAX_SAFE_AMPLITUDE}",
                    Description = "Wave amplitude too large, may cause position overflow",
                    Severity = ValidationSeverity.Error
                });
                actions.Add($"Reduce wave amplitude to {MAX_SAFE_AMPLITUDE} or less");
                score -= 0.5f;
            }
            else if (!_recommendedRanges.AmplitudeRange.Contains(amplitude))
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.PerformanceImpact,
                    ParameterName = "WaveAmplitude",
                    CurrentValue = amplitude,
                    RecommendedValue = $"{_recommendedRanges.AmplitudeRange.Min} - {_recommendedRanges.AmplitudeRange.Max}",
                    Description = "Wave amplitude outside recommended range for Quest 3",
                    Severity = ValidationSeverity.Info
                });
                score -= 0.1f;
            }
            
            // Validate frequency
            if (frequency < MIN_SAFE_FREQUENCY)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.StabilityRisk,
                    ParameterName = "WaveFrequency",
                    CurrentValue = frequency,
                    RecommendedValue = $">= {MIN_SAFE_FREQUENCY}",
                    Description = "Wave frequency too low, may cause static appearance",
                    Severity = ValidationSeverity.Warning
                });
                actions.Add($"Increase wave frequency to at least {MIN_SAFE_FREQUENCY}");
                score -= 0.2f;
            }
            else if (frequency > MAX_SAFE_FREQUENCY)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.PerformanceImpact,
                    ParameterName = "WaveFrequency",
                    CurrentValue = frequency,
                    RecommendedValue = $"<= {MAX_SAFE_FREQUENCY}",
                    Description = "Wave frequency too high, may cause performance issues and aliasing",
                    Severity = ValidationSeverity.Error
                });
                actions.Add($"Reduce wave frequency to {MAX_SAFE_FREQUENCY} or less");
                score -= 0.5f;
            }
            
            // Validate speed
            if (speed < MIN_SAFE_SPEED)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.StabilityRisk,
                    ParameterName = "WaveSpeed",
                    CurrentValue = speed,
                    RecommendedValue = $">= {MIN_SAFE_SPEED}",
                    Description = "Wave speed too low, may appear static",
                    Severity = ValidationSeverity.Info
                });
                score -= 0.1f;
            }
            else if (speed > MAX_SAFE_SPEED)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.PerformanceImpact,
                    ParameterName = "WaveSpeed",
                    CurrentValue = speed,
                    RecommendedValue = $"<= {MAX_SAFE_SPEED}",
                    Description = "Wave speed too high, may cause motion sickness in VR",
                    Severity = ValidationSeverity.Warning
                });
                actions.Add($"Reduce wave speed to {MAX_SAFE_SPEED} or less for VR comfort");
                score -= 0.3f;
            }
            
            // Validate cell size
            if (cellSize < MIN_SAFE_CELL_SIZE)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.PerformanceImpact,
                    ParameterName = "CellSize",
                    CurrentValue = cellSize,
                    RecommendedValue = $">= {MIN_SAFE_CELL_SIZE}",
                    Description = "Cell size too small, may cause excessive memory usage",
                    Severity = ValidationSeverity.Warning
                });
                actions.Add($"Increase cell size to at least {MIN_SAFE_CELL_SIZE}");
                score -= 0.3f;
            }
            else if (cellSize > MAX_SAFE_CELL_SIZE)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.StabilityRisk,
                    ParameterName = "CellSize",
                    CurrentValue = cellSize,
                    RecommendedValue = $"<= {MAX_SAFE_CELL_SIZE}",
                    Description = "Cell size too large, may cause coarse wave resolution",
                    Severity = ValidationSeverity.Warning
                });
                actions.Add($"Reduce cell size to {MAX_SAFE_CELL_SIZE} or less");
                score -= 0.2f;
            }
            
            score = math.clamp(score, 0f, 1f);
            bool isValid = !issues.Any(i => i.Severity >= ValidationSeverity.Error);
            var severity = issues.Count > 0 ? issues.Max(i => i.Severity) : ValidationSeverity.None;
            
            return new WaveParameterValidationResult
            {
                IsValid = isValid,
                Severity = severity,
                Issues = issues.ToArray(),
                ValidationScore = score,
                ValidationReport = $"Wave parameters validation: {issues.Count} issues found, score: {score:F2}",
                RecommendedActions = actions.ToArray()
            };
        }
        
        /// <summary>
        /// Validate grid settings for performance and memory constraints.
        /// </summary>
        public WaveParameterValidationResult ValidateGridSettings(int2 gridSize, float cellSize, int bubbleCount)
        {
            var issues = new List<ValidationIssue>();
            var actions = new List<string>();
            float score = 1.0f;
            
            // Calculate total grid cells
            int totalCells = gridSize.x * gridSize.y;
            
            // Validate grid size
            if (gridSize.x <= 0 || gridSize.y <= 0)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.SafetyViolation,
                    ParameterName = "GridSize",
                    CurrentValue = totalCells,
                    RecommendedValue = "> 0",
                    Description = "Grid dimensions must be positive",
                    Severity = ValidationSeverity.Critical
                });
                actions.Add("Set grid dimensions to positive values");
                score = 0f;
            }
            else if (totalCells > MAX_GRID_CELLS)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.PerformanceImpact,
                    ParameterName = "GridSize",
                    CurrentValue = totalCells,
                    RecommendedValue = $"<= {MAX_GRID_CELLS}",
                    Description = "Grid too large, may cause performance issues on Quest 3",
                    Severity = ValidationSeverity.Error
                });
                actions.Add($"Reduce grid size to {math.sqrt(MAX_GRID_CELLS):F0}x{math.sqrt(MAX_GRID_CELLS):F0} or smaller");
                score -= 0.5f;
            }
            else if (totalCells > _recommendedRanges.GridSizeRange.Max * _recommendedRanges.GridSizeRange.Max)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.PerformanceImpact,
                    ParameterName = "GridSize",
                    CurrentValue = totalCells,
                    RecommendedValue = $"<= {_recommendedRanges.GridSizeRange.Max}x{_recommendedRanges.GridSizeRange.Max}",
                    Description = "Grid size above recommended range for Quest 3",
                    Severity = ValidationSeverity.Warning
                });
                score -= 0.2f;
            }
            
            // Validate bubble count vs grid size
            if (bubbleCount > totalCells)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.ParameterConflict,
                    ParameterName = "BubbleCount",
                    CurrentValue = bubbleCount,
                    RecommendedValue = $"<= {totalCells}",
                    Description = "More bubbles than grid cells available",
                    Severity = ValidationSeverity.Error
                });
                actions.Add($"Reduce bubble count to {totalCells} or increase grid size");
                score -= 0.4f;
            }
            else if (bubbleCount > QUEST3_MAX_RECOMMENDED_BUBBLES)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.PerformanceImpact,
                    ParameterName = "BubbleCount",
                    CurrentValue = bubbleCount,
                    RecommendedValue = $"<= {QUEST3_MAX_RECOMMENDED_BUBBLES}",
                    Description = "Bubble count exceeds Quest 3 recommended maximum",
                    Severity = ValidationSeverity.Warning
                });
                actions.Add($"Consider reducing bubble count to {QUEST3_MAX_RECOMMENDED_BUBBLES} for optimal performance");
                score -= 0.2f;
            }
            
            // Estimate memory usage
            long estimatedMemory = EstimateMemoryUsage(gridSize, cellSize, bubbleCount);
            if (estimatedMemory > WAVE_SYSTEM_MEMORY_BUDGET)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.MemoryExcessive,
                    ParameterName = "MemoryUsage",
                    CurrentValue = estimatedMemory / (1024f * 1024f),
                    RecommendedValue = $"<= {WAVE_SYSTEM_MEMORY_BUDGET / (1024 * 1024)}MB",
                    Description = "Estimated memory usage exceeds budget",
                    Severity = ValidationSeverity.Error
                });
                actions.Add("Reduce grid size or bubble count to lower memory usage");
                score -= 0.4f;
            }
            
            score = math.clamp(score, 0f, 1f);
            bool isValid = !issues.Any(i => i.Severity >= ValidationSeverity.Error);
            var severity = issues.Count > 0 ? issues.Max(i => i.Severity) : ValidationSeverity.None;
            
            return new WaveParameterValidationResult
            {
                IsValid = isValid,
                Severity = severity,
                Issues = issues.ToArray(),
                ValidationScore = score,
                ValidationReport = $"Grid settings validation: {totalCells} cells, {estimatedMemory / (1024 * 1024)}MB estimated",
                RecommendedActions = actions.ToArray()
            };
        }
        
        /// <summary>
        /// Validate performance characteristics for Quest 3 hardware.
        /// </summary>
        public WavePerformanceValidationResult ValidatePerformance(WaveMatrixSettings settings, int expectedBubbleCount, float targetFrameTimeMs)
        {
            // Estimate performance based on complexity
            float estimatedFrameTime = EstimateFrameTime(settings, expectedBubbleCount);
            bool meetsTarget = estimatedFrameTime <= targetFrameTimeMs;
            float margin = targetFrameTimeMs - estimatedFrameTime;
            
            var bottlenecks = new List<string>();
            var suggestions = new List<string>();
            
            // Identify bottlenecks
            if (settings.GridSize.x * settings.GridSize.y > 2500) // 50x50
            {
                bottlenecks.Add("Large grid size increases calculation complexity");
                suggestions.Add("Reduce grid size for better performance");
            }
            
            if (settings.WaveFrequency > 10f)
            {
                bottlenecks.Add("High wave frequency increases calculation load");
                suggestions.Add("Reduce wave frequency to improve performance");
            }
            
            if (expectedBubbleCount > 100)
            {
                bottlenecks.Add("High bubble count increases processing time");
                suggestions.Add("Reduce bubble count for Quest 3 optimization");
            }
            
            if (settings.CellSize < 0.5f)
            {
                bottlenecks.Add("Small cell size increases memory bandwidth requirements");
                suggestions.Add("Increase cell size to reduce memory pressure");
            }
            
            // Calculate performance rating
            float performanceRating = meetsTarget ? 
                math.clamp(1.0f - (estimatedFrameTime / targetFrameTimeMs), 0f, 1f) :
                math.clamp(targetFrameTimeMs / estimatedFrameTime, 0f, 1f);
            
            return new WavePerformanceValidationResult
            {
                MeetsPerformanceTarget = meetsTarget,
                EstimatedFrameTimeMs = estimatedFrameTime,
                TargetFrameTimeMs = targetFrameTimeMs,
                PerformanceMarginMs = margin,
                EstimatedMemoryUsage = EstimateMemoryUsage(settings.GridSize, settings.CellSize, expectedBubbleCount),
                PerformanceBottlenecks = bottlenecks.ToArray(),
                OptimizationSuggestions = suggestions.ToArray(),
                PerformanceRating = performanceRating
            };
        }
        
        /// <summary>
        /// Check if settings will produce stable wave animations without artifacts.
        /// </summary>
        public WaveStabilityValidationResult ValidateStability(WaveMatrixSettings settings, float timeRange = 10f)
        {
            var issues = new List<string>();
            bool isStable = true;
            float maxDeviation = 0f;
            bool numericalIssues = false;
            bool artifacts = false;
            
            // Test wave calculations over time range
            const int testSamples = 100;
            float timeStep = timeRange / testSamples;
            var testPositions = new List<float3>();
            
            for (int i = 0; i < testSamples; i++)
            {
                float time = i * timeStep;
                
                // Test a few sample positions
                for (int gridIndex = 0; gridIndex < math.min(10, settings.GridSize.x * settings.GridSize.y); gridIndex++)
                {
                    int2 gridPos = new int2(gridIndex % settings.GridSize.x, gridIndex / settings.GridSize.x);
                    float3 basePosition = new float3(gridPos.x * settings.CellSize, 0f, gridPos.y * settings.CellSize);
                    
                    // Calculate wave position
                    float wavePhase = time * settings.WaveSpeed;
                    float wave = math.sin(basePosition.x * settings.WaveFrequency + wavePhase) * settings.WaveAmplitude;
                    float3 wavePosition = basePosition + new float3(0, wave, 0);
                    
                    // Check for numerical issues
                    if (!math.isfinite(wavePosition.x) || !math.isfinite(wavePosition.y) || !math.isfinite(wavePosition.z))
                    {
                        numericalIssues = true;
                        issues.Add($"Numerical instability detected at time {time:F2}");
                        isStable = false;
                    }
                    
                    // Track maximum deviation
                    float deviation = math.length(wavePosition - basePosition);
                    maxDeviation = math.max(maxDeviation, deviation);
                    
                    testPositions.Add(wavePosition);
                }
            }
            
            // Check for excessive deviation
            if (maxDeviation > settings.WaveAmplitude * 2f)
            {
                artifacts = true;
                issues.Add($"Excessive position deviation detected: {maxDeviation:F2}");
                isStable = false;
            }
            
            // Check for frequency aliasing
            float nyquistFrequency = 1f / (2f * settings.CellSize);
            if (settings.WaveFrequency > nyquistFrequency)
            {
                artifacts = true;
                issues.Add($"Wave frequency {settings.WaveFrequency:F2} exceeds Nyquist limit {nyquistFrequency:F2}");
                isStable = false;
            }
            
            // Calculate stability score
            float stabilityScore = isStable ? 1.0f : math.clamp(1.0f - (issues.Count * 0.2f), 0f, 1f);
            
            return new WaveStabilityValidationResult
            {
                IsStable = isStable,
                MaxPositionDeviation = maxDeviation,
                TestedTimeRange = timeRange,
                StabilityIssues = issues.ToArray(),
                StabilityScore = stabilityScore,
                NumericalIssuesDetected = numericalIssues,
                ArtifactsDetected = artifacts
            };
        }
        
        /// <summary>
        /// Get recommended safe parameter ranges for Quest 3.
        /// </summary>
        public WaveParameterRanges GetRecommendedRanges()
        {
            return _recommendedRanges;
        }
        
        /// <summary>
        /// Suggest corrected parameters if validation fails.
        /// </summary>
        public WaveMatrixSettings SuggestCorrections(WaveMatrixSettings settings, WaveParameterValidationResult validationResult)
        {
            var correctedSettings = settings;
            
            foreach (var issue in validationResult.Issues.Where(i => i.Severity >= ValidationSeverity.Warning))
            {
                switch (issue.ParameterName)
                {
                    case "WaveAmplitude":
                        correctedSettings.WaveAmplitude = _recommendedRanges.AmplitudeRange.Clamp(settings.WaveAmplitude);
                        break;
                    case "WaveFrequency":
                        correctedSettings.WaveFrequency = _recommendedRanges.FrequencyRange.Clamp(settings.WaveFrequency);
                        break;
                    case "WaveSpeed":
                        correctedSettings.WaveSpeed = _recommendedRanges.SpeedRange.Clamp(settings.WaveSpeed);
                        break;
                    case "CellSize":
                        correctedSettings.CellSize = _recommendedRanges.CellSizeRange.Clamp(settings.CellSize);
                        break;
                    case "GridSize":
                        int maxDim = _recommendedRanges.GridSizeRange.Max;
                        correctedSettings.GridSize = new int2(
                            math.min(settings.GridSize.x, maxDim),
                            math.min(settings.GridSize.y, maxDim)
                        );
                        break;
                }
            }
            
            return correctedSettings;
        }
        
        /// <summary>
        /// Validate parameter combinations for potential conflicts or instabilities.
        /// </summary>
        public WaveParameterValidationResult ValidateParameterInteractions(WaveMatrixSettings settings)
        {
            var issues = new List<ValidationIssue>();
            var actions = new List<string>();
            float score = 1.0f;
            
            // Check wavelength vs cell size ratio
            float wavelength = 2f * math.PI / settings.WaveFrequency;
            float cellsPerWavelength = wavelength / settings.CellSize;
            
            if (cellsPerWavelength < 4f)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.ParameterConflict,
                    ParameterName = "FrequencyVsCellSize",
                    CurrentValue = cellsPerWavelength,
                    RecommendedValue = ">= 4",
                    Description = "Insufficient grid resolution for wave frequency (aliasing risk)",
                    Severity = ValidationSeverity.Warning
                });
                actions.Add("Increase cell size or decrease wave frequency to avoid aliasing");
                score -= 0.3f;
            }
            
            // Check amplitude vs speed for motion comfort
            float motionIntensity = settings.WaveAmplitude * settings.WaveSpeed;
            if (motionIntensity > 5f) // Arbitrary comfort threshold
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.ParameterConflict,
                    ParameterName = "AmplitudeVsSpeed",
                    CurrentValue = motionIntensity,
                    RecommendedValue = "<= 5",
                    Description = "High amplitude + speed combination may cause VR motion sickness",
                    Severity = ValidationSeverity.Warning
                });
                actions.Add("Reduce either wave amplitude or speed for VR comfort");
                score -= 0.2f;
            }
            
            // Check grid size vs performance
            int totalCells = settings.GridSize.x * settings.GridSize.y;
            float complexityFactor = totalCells * settings.WaveFrequency;
            if (complexityFactor > 50000f) // Arbitrary performance threshold
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.PerformanceImpact,
                    ParameterName = "GridVsFrequency",
                    CurrentValue = complexityFactor,
                    RecommendedValue = "<= 50000",
                    Description = "High grid size + frequency combination may impact performance",
                    Severity = ValidationSeverity.Warning
                });
                actions.Add("Reduce grid size or wave frequency for better performance");
                score -= 0.2f;
            }
            
            score = math.clamp(score, 0f, 1f);
            bool isValid = !issues.Any(i => i.Severity >= ValidationSeverity.Error);
            var severity = issues.Count > 0 ? issues.Max(i => i.Severity) : ValidationSeverity.None;
            
            return new WaveParameterValidationResult
            {
                IsValid = isValid,
                Severity = severity,
                Issues = issues.ToArray(),
                ValidationScore = score,
                ValidationReport = $"Parameter interaction validation: {issues.Count} conflicts found",
                RecommendedActions = actions.ToArray()
            };
        }
        
        /// <summary>
        /// Check if parameters are within safe bounds to prevent numerical issues.
        /// </summary>
        public WaveParameterValidationResult ValidateSafetyBounds(WaveMatrixSettings settings)
        {
            var issues = new List<ValidationIssue>();
            var actions = new List<string>();
            float score = 1.0f;
            
            // Check for NaN or infinite values
            if (!math.isfinite(settings.WaveAmplitude))
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.SafetyViolation,
                    ParameterName = "WaveAmplitude",
                    CurrentValue = settings.WaveAmplitude,
                    RecommendedValue = "finite value",
                    Description = "Wave amplitude is not a finite number",
                    Severity = ValidationSeverity.Critical
                });
                score = 0f;
            }
            
            if (!math.isfinite(settings.WaveFrequency))
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.SafetyViolation,
                    ParameterName = "WaveFrequency",
                    CurrentValue = settings.WaveFrequency,
                    RecommendedValue = "finite value",
                    Description = "Wave frequency is not a finite number",
                    Severity = ValidationSeverity.Critical
                });
                score = 0f;
            }
            
            if (!math.isfinite(settings.WaveSpeed))
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.SafetyViolation,
                    ParameterName = "WaveSpeed",
                    CurrentValue = settings.WaveSpeed,
                    RecommendedValue = "finite value",
                    Description = "Wave speed is not a finite number",
                    Severity = ValidationSeverity.Critical
                });
                score = 0f;
            }
            
            if (!math.isfinite(settings.CellSize))
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.SafetyViolation,
                    ParameterName = "CellSize",
                    CurrentValue = settings.CellSize,
                    RecommendedValue = "finite value",
                    Description = "Cell size is not a finite number",
                    Severity = ValidationSeverity.Critical
                });
                score = 0f;
            }
            
            // Check for negative values where they shouldn't be
            if (settings.WaveAmplitude < 0f)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.SafetyViolation,
                    ParameterName = "WaveAmplitude",
                    CurrentValue = settings.WaveAmplitude,
                    RecommendedValue = ">= 0",
                    Description = "Wave amplitude cannot be negative",
                    Severity = ValidationSeverity.Error
                });
                actions.Add("Set wave amplitude to a positive value");
                score -= 0.5f;
            }
            
            if (settings.WaveFrequency <= 0f)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.SafetyViolation,
                    ParameterName = "WaveFrequency",
                    CurrentValue = settings.WaveFrequency,
                    RecommendedValue = "> 0",
                    Description = "Wave frequency must be positive",
                    Severity = ValidationSeverity.Error
                });
                actions.Add("Set wave frequency to a positive value");
                score -= 0.5f;
            }
            
            if (settings.CellSize <= 0f)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.SafetyViolation,
                    ParameterName = "CellSize",
                    CurrentValue = settings.CellSize,
                    RecommendedValue = "> 0",
                    Description = "Cell size must be positive",
                    Severity = ValidationSeverity.Error
                });
                actions.Add("Set cell size to a positive value");
                score -= 0.5f;
            }
            
            score = math.clamp(score, 0f, 1f);
            bool isValid = !issues.Any(i => i.Severity >= ValidationSeverity.Error);
            var severity = issues.Count > 0 ? issues.Max(i => i.Severity) : ValidationSeverity.None;
            
            return new WaveParameterValidationResult
            {
                IsValid = isValid,
                Severity = severity,
                Issues = issues.ToArray(),
                ValidationScore = score,
                ValidationReport = $"Safety bounds validation: {issues.Count} safety issues found",
                RecommendedActions = actions.ToArray()
            };
        }
        
        #region Private Helper Methods
        
        private float EstimateFrameTime(WaveMatrixSettings settings, int bubbleCount)
        {
            // Simplified performance model based on complexity factors
            float baseTime = 0.1f; // Base overhead in ms
            
            // Grid complexity factor
            int totalCells = settings.GridSize.x * settings.GridSize.y;
            float gridFactor = totalCells / 2500f; // Normalized to 50x50 grid
            
            // Wave complexity factor
            float waveFactor = settings.WaveFrequency / 5f; // Normalized to frequency 5
            
            // Bubble count factor
            float bubbleFactor = bubbleCount / 100f; // Normalized to 100 bubbles
            
            // Cell size factor (smaller cells = more memory bandwidth)
            float cellFactor = 1f / settings.CellSize;
            
            return baseTime * (1f + gridFactor + waveFactor + bubbleFactor + cellFactor * 0.1f);
        }
        
        private long EstimateMemoryUsage(int2 gridSize, float cellSize, int bubbleCount)
        {
            // Estimate memory usage based on data structures
            long gridMemory = gridSize.x * gridSize.y * sizeof(float) * 4; // Grid positions
            long bubbleMemory = bubbleCount * 64; // Bubble data structures
            long bufferMemory = bubbleCount * sizeof(float) * 3 * 4; // Position buffers
            
            return gridMemory + bubbleMemory + bufferMemory;
        }
        
        private string GenerateValidationReport(WaveMatrixSettings settings, List<ValidationIssue> issues, float score)
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine($"Wave Parameter Validation Report");
            report.AppendLine($"Overall Score: {score:F2}/1.0");
            report.AppendLine($"Grid Size: {settings.GridSize.x}x{settings.GridSize.y} ({settings.GridSize.x * settings.GridSize.y} cells)");
            report.AppendLine($"Wave Amplitude: {settings.WaveAmplitude:F2}");
            report.AppendLine($"Wave Frequency: {settings.WaveFrequency:F2}");
            report.AppendLine($"Wave Speed: {settings.WaveSpeed:F2}");
            report.AppendLine($"Cell Size: {settings.CellSize:F2}");
            report.AppendLine();
            
            if (issues.Count > 0)
            {
                report.AppendLine($"Issues Found ({issues.Count}):");
                foreach (var issue in issues.OrderByDescending(i => i.Severity))
                {
                    report.AppendLine($"  [{issue.Severity}] {issue.ParameterName}: {issue.Description}");
                }
            }
            else
            {
                report.AppendLine("No issues found - all parameters are valid!");
            }
            
            return report.ToString();
        }
        
        #endregion
    }
}