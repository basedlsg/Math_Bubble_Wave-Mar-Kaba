using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Validates build configuration consistency and flag combinations.
    /// Ensures that experimental feature flags are properly configured
    /// and dependencies are satisfied across different build configurations.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public static class BuildConfigurationValidator
    {
        /// <summary>
        /// Validation result containing all issues found during validation.
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
            public DateTime ValidatedAt { get; set; }
            public string BuildConfiguration { get; set; }
            
            public string GenerateReport()
            {
                var report = $"# Build Configuration Validation Report\n\n";
                report += $"**Validation Time**: {ValidatedAt:yyyy-MM-dd HH:mm:ss} UTC\n";
                report += $"**Build Configuration**: {BuildConfiguration}\n";
                report += $"**Status**: {(IsValid ? "✅ VALID" : "❌ INVALID")}\n\n";
                
                if (Issues.Count == 0)
                {
                    report += "No configuration issues detected.\n";
                }
                else
                {
                    report += $"## Issues Found ({Issues.Count})\n\n";
                    foreach (var issue in Issues)
                    {
                        var severity = issue.Severity switch
                        {
                            ValidationSeverity.Error => "🔴 ERROR",
                            ValidationSeverity.Warning => "🟡 WARNING",
                            ValidationSeverity.Info => "ℹ️ INFO",
                            _ => "❓ UNKNOWN"
                        };
                        
                        report += $"### {severity}: {issue.Title}\n";
                        report += $"{issue.Description}\n";
                        
                        if (!string.IsNullOrEmpty(issue.Recommendation))
                        {
                            report += $"**Recommendation**: {issue.Recommendation}\n";
                        }
                        
                        report += "\n";
                    }
                }
                
                return report;
            }
        }
        
        /// <summary>
        /// Individual validation issue with severity and recommendations.
        /// </summary>
        public class ValidationIssue
        {
            public ValidationSeverity Severity { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Recommendation { get; set; }
            public ExperimentalFeature? RelatedFeature { get; set; }
        }
        
        /// <summary>
        /// Severity levels for validation issues.
        /// </summary>
        public enum ValidationSeverity
        {
            Info,
            Warning,
            Error
        }
        
        /// <summary>
        /// Comprehensive validation of the current build configuration.
        /// Checks all flag combinations, dependencies, and consistency rules.
        /// </summary>
        public static ValidationResult ValidateConfiguration()
        {
            var result = new ValidationResult
            {
                ValidatedAt = DateTime.UtcNow,
                BuildConfiguration = GetCurrentBuildConfiguration()
            };
            
            // Run all validation checks
            ValidateDependencyRules(result);
            ValidateFlagCombinations(result);
            ValidateBuildConsistency(result);
            ValidatePerformanceImplications(result);
            ValidateSecurityImplications(result);
            
            // Determine overall validity
            result.IsValid = !result.Issues.Any(i => i.Severity == ValidationSeverity.Error);
            
            return result;
        }
        
        /// <summary>
        /// Validate that feature dependencies are properly satisfied.
        /// </summary>
        private static void ValidateDependencyRules(ValidationResult result)
        {
            var states = CompilerFlags.GetAllFeatureStates();
            
            // Cloud inference requires AI integration
            if (states[ExperimentalFeature.CLOUD_INFERENCE] && 
                !states[ExperimentalFeature.AI_INTEGRATION])
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Title = "Cloud Inference Missing AI Integration Dependency",
                    Description = "Cloud inference is enabled but AI integration is disabled. " +
                                "Cloud inference requires AI integration to function properly.",
                    Recommendation = "Enable AI integration (EXP_AI) or disable cloud inference (EXP_CLOUD_INFERENCE).",
                    RelatedFeature = ExperimentalFeature.CLOUD_INFERENCE
                });
            }
            
            // On-device ML requires AI integration
            if (states[ExperimentalFeature.ON_DEVICE_ML] && 
                !states[ExperimentalFeature.AI_INTEGRATION])
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Title = "On-Device ML Missing AI Integration Dependency",
                    Description = "On-device ML is enabled but AI integration is disabled. " +
                                "On-device ML requires AI integration to function properly.",
                    Recommendation = "Enable AI integration (EXP_AI) or disable on-device ML (EXP_ON_DEVICE_ML).",
                    RelatedFeature = ExperimentalFeature.ON_DEVICE_ML
                });
            }
            
            // Advanced wave algorithms should be carefully considered with AI
            if (states[ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS] && 
                states[ExperimentalFeature.AI_INTEGRATION])
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Title = "Advanced Wave Algorithms with AI Integration",
                    Description = "Both advanced wave algorithms and AI integration are enabled. " +
                                "This combination may have significant performance implications.",
                    Recommendation = "Monitor performance carefully and consider disabling one feature if performance targets are not met.",
                    RelatedFeature = ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS
                });
            }
        }
        
        /// <summary>
        /// Validate specific flag combinations for known issues.
        /// </summary>
        private static void ValidateFlagCombinations(ValidationResult result)
        {
            var states = CompilerFlags.GetAllFeatureStates();
            var enabledFeatures = states.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            
            // Warn if too many experimental features are enabled simultaneously
            if (enabledFeatures.Count >= 3)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Title = "Multiple Experimental Features Enabled",
                    Description = $"Multiple experimental features are enabled simultaneously: {string.Join(", ", enabledFeatures)}. " +
                                "This may impact performance and stability.",
                    Recommendation = "Consider enabling experimental features incrementally to isolate potential issues."
                });
            }
            
            // Check for conflicting AI features
            if (states[ExperimentalFeature.CLOUD_INFERENCE] && 
                states[ExperimentalFeature.ON_DEVICE_ML])
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Title = "Conflicting AI Processing Methods",
                    Description = "Both cloud inference and on-device ML are enabled. " +
                                "This may lead to redundant processing and increased resource usage.",
                    Recommendation = "Choose either cloud inference or on-device ML based on your deployment requirements."
                });
            }
            
            // Validate voice processing implications
            if (states[ExperimentalFeature.VOICE_PROCESSING] && 
                !states[ExperimentalFeature.AI_INTEGRATION])
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Info,
                    Title = "Voice Processing Without AI Integration",
                    Description = "Voice processing is enabled without AI integration. " +
                                "Voice processing may have limited functionality without AI features.",
                    Recommendation = "Consider enabling AI integration for enhanced voice processing capabilities."
                });
            }
        }
        
        /// <summary>
        /// Validate build configuration consistency across different build types.
        /// </summary>
        private static void ValidateBuildConsistency(ValidationResult result)
        {
            var buildConfig = GetCurrentBuildConfiguration();
            var states = CompilerFlags.GetAllFeatureStates();
            var enabledFeatures = states.Where(kvp => kvp.Value).ToList();
            
            // Warn about experimental features in release builds
            if (buildConfig.Contains("Release") && enabledFeatures.Any())
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Title = "Experimental Features in Release Build",
                    Description = $"Experimental features are enabled in a release build: {string.Join(", ", enabledFeatures.Select(f => f.Key))}. " +
                                "This may not be intended for production deployment.",
                    Recommendation = "Verify that experimental features are intentionally enabled for this release build."
                });
            }
            
            // Check for development-only features in non-development builds
            if (!buildConfig.Contains("Development") && !buildConfig.Contains("Editor"))
            {
                if (states[ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS])
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Info,
                        Title = "Advanced Wave Algorithms in Non-Development Build",
                        Description = "Advanced wave algorithms are enabled in a non-development build. " +
                                    "Ensure this is intentional for your deployment target.",
                        Recommendation = "Verify that advanced wave algorithms are stable enough for your target deployment."
                    });
                }
            }
        }
        
        /// <summary>
        /// Validate performance implications of current flag configuration.
        /// </summary>
        private static void ValidatePerformanceImplications(ValidationResult result)
        {
            var states = CompilerFlags.GetAllFeatureStates();
            var performanceImpactFeatures = new[]
            {
                ExperimentalFeature.AI_INTEGRATION,
                ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS,
                ExperimentalFeature.ON_DEVICE_ML,
                ExperimentalFeature.VOICE_PROCESSING
            };
            
            var enabledPerformanceFeatures = performanceImpactFeatures
                .Where(f => states[f])
                .ToList();
            
            if (enabledPerformanceFeatures.Count >= 2)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Title = "High Performance Impact Configuration",
                    Description = $"Multiple performance-intensive features are enabled: {string.Join(", ", enabledPerformanceFeatures)}. " +
                                "This configuration may not meet Quest 3 performance targets (72 Hz).",
                    Recommendation = "Conduct thorough performance testing on Quest 3 hardware before deployment. " +
                                   "Consider implementing auto-LOD systems to maintain performance."
                });
            }
            
            // Specific warnings for Quest 3 deployment
            if (states[ExperimentalFeature.ON_DEVICE_ML])
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Info,
                    Title = "On-Device ML Performance Consideration",
                    Description = "On-device ML is enabled. This feature may have significant impact on Quest 3 performance, " +
                                "particularly thermal throttling during extended use.",
                    Recommendation = "Implement thermal monitoring and performance scaling for on-device ML workloads."
                });
            }
        }
        
        /// <summary>
        /// Validate security implications of current flag configuration.
        /// </summary>
        private static void ValidateSecurityImplications(ValidationResult result)
        {
            var states = CompilerFlags.GetAllFeatureStates();
            
            // Cloud inference security considerations
            if (states[ExperimentalFeature.CLOUD_INFERENCE])
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Info,
                    Title = "Cloud Inference Security Consideration",
                    Description = "Cloud inference is enabled. This feature requires network connectivity " +
                                "and may transmit user data to external services.",
                    Recommendation = "Ensure proper data privacy policies and user consent mechanisms are in place. " +
                                   "Consider data encryption and secure transmission protocols."
                });
            }
            
            // Voice processing privacy considerations
            if (states[ExperimentalFeature.VOICE_PROCESSING])
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Info,
                    Title = "Voice Processing Privacy Consideration",
                    Description = "Voice processing is enabled. This feature processes audio input " +
                                "which may contain sensitive user information.",
                    Recommendation = "Implement proper audio data handling, storage limitations, and user privacy controls. " +
                                   "Consider local processing where possible to minimize privacy risks."
                });
            }
        }
        
        /// <summary>
        /// Get the current build configuration as a human-readable string.
        /// </summary>
        private static string GetCurrentBuildConfiguration()
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
        
        /// <summary>
        /// Validate configuration and log results to Unity console.
        /// Useful for pre-build validation hooks.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void ValidateAndLog()
        {
            var result = ValidateConfiguration();
            
            if (result.IsValid)
            {
                UnityEngine.Debug.Log($"✅ Build configuration validation passed ({result.BuildConfiguration})");
                
                if (result.Issues.Any(i => i.Severity == ValidationSeverity.Warning))
                {
                    UnityEngine.Debug.LogWarning($"⚠️ {result.Issues.Count(i => i.Severity == ValidationSeverity.Warning)} warning(s) found during validation");
                }
            }
            else
            {
                var errorCount = result.Issues.Count(i => i.Severity == ValidationSeverity.Error);
                UnityEngine.Debug.LogError($"❌ Build configuration validation failed with {errorCount} error(s)");
                
                // Log each error individually for better visibility
                foreach (var error in result.Issues.Where(i => i.Severity == ValidationSeverity.Error))
                {
                    UnityEngine.Debug.LogError($"Configuration Error: {error.Title} - {error.Description}");
                }
            }
            
            // Log detailed report for debugging
            UnityEngine.Debug.Log($"Detailed validation report:\n{result.GenerateReport()}");
        }
        
        /// <summary>
        /// Quick validation check that returns true if configuration is valid.
        /// Useful for automated checks without detailed reporting.
        /// </summary>
        public static bool IsConfigurationValid()
        {
            return ValidateConfiguration().IsValid;
        }
        
        /// <summary>
        /// Get a list of all validation rules for documentation purposes.
        /// </summary>
        public static List<string> GetValidationRules()
        {
            return new List<string>
            {
                "Cloud inference requires AI integration to be enabled",
                "On-device ML requires AI integration to be enabled",
                "Multiple experimental features may impact performance",
                "Conflicting AI processing methods should be avoided",
                "Experimental features in release builds require verification",
                "Performance-intensive features require Quest 3 testing",
                "Cloud inference requires privacy policy compliance",
                "Voice processing requires audio data handling policies"
            };
        }
    }
}