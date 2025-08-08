using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Central management system for experimental feature compiler flags.
    /// Implements ICompilerFlagManager for dependency injection and testing.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class CompilerFlagManager : ICompilerFlagManager
    {
        private static CompilerFlagManager _instance;
        private readonly Dictionary<ExperimentalFeature, bool> _runtimeOverrides;

        /// <summary>
        /// Singleton instance for global access.
        /// </summary>
        public static CompilerFlagManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CompilerFlagManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Private constructor for singleton pattern.
        /// </summary>
        private CompilerFlagManager()
        {
            _runtimeOverrides = new Dictionary<ExperimentalFeature, bool>();
            
            // Initialize with compile-time states
            InitializeFromCompileTimeFlags();
            
            // Validate configuration on startup
            ValidateConfiguration();
        }

        /// <summary>
        /// Check if a specific experimental feature is currently enabled.
        /// Considers both compile-time flags and runtime overrides.
        /// </summary>
        public bool IsFeatureEnabled(ExperimentalFeature feature)
        {
            // Check for runtime override first
            if (_runtimeOverrides.TryGetValue(feature, out bool overrideValue))
            {
                return overrideValue;
            }

            // Fall back to compile-time constant
            return CompilerFlags.IsFeatureEnabled(feature);
        }

        /// <summary>
        /// Set the runtime state of an experimental feature.
        /// Note: Cannot enable features that are compile-time disabled.
        /// </summary>
        public void SetFeatureState(ExperimentalFeature feature, bool enabled)
        {
            var compileTimeEnabled = CompilerFlags.IsFeatureEnabled(feature);
            
            if (enabled && !compileTimeEnabled)
            {
                Debug.LogWarning($"Cannot enable {feature} at runtime - feature is compile-time disabled. " +
                               $"Enable the corresponding compiler flag and rebuild.");
                return;
            }

            if (enabled == compileTimeEnabled)
            {
                // Remove override if it matches compile-time state
                _runtimeOverrides.Remove(feature);
            }
            else
            {
                // Set override for different state
                _runtimeOverrides[feature] = enabled;
            }

            Debug.Log($"Feature {feature} runtime state set to: {enabled}");
        }

        /// <summary>
        /// Get the current state of all experimental features.
        /// </summary>
        public Dictionary<ExperimentalFeature, bool> GetAllFeatureStates()
        {
            var states = new Dictionary<ExperimentalFeature, bool>();
            
            foreach (ExperimentalFeature feature in Enum.GetValues(typeof(ExperimentalFeature)))
            {
                states[feature] = IsFeatureEnabled(feature);
            }

            return states;
        }

        /// <summary>
        /// Validate the current configuration for consistency and dependencies.
        /// </summary>
        public void ValidateConfiguration()
        {
            var issues = new List<string>();

            // Check dependency relationships
            if (IsFeatureEnabled(ExperimentalFeature.CLOUD_INFERENCE) && 
                !IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                issues.Add("Cloud inference requires AI integration to be enabled");
            }

            if (IsFeatureEnabled(ExperimentalFeature.ON_DEVICE_ML) && 
                !IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION))
            {
                issues.Add("On-device ML requires AI integration to be enabled");
            }

            // Log validation results
            if (issues.Count > 0)
            {
                Debug.LogWarning($"Configuration validation found {issues.Count} issue(s):\n" +
                               string.Join("\n", issues));
            }
            else
            {
                Debug.Log("Configuration validation passed - all dependencies satisfied");
            }
        }

        /// <summary>
        /// Generate a human-readable status report of all features.
        /// </summary>
        public string GenerateStatusReport()
        {
            var report = "# XR Bubble Library - Experimental Feature Status\n\n";
            report += "## Feature States\n\n";
            
            foreach (ExperimentalFeature feature in Enum.GetValues(typeof(ExperimentalFeature)))
            {
                var compileTimeState = CompilerFlags.IsFeatureEnabled(feature);
                var runtimeState = IsFeatureEnabled(feature);
                var hasOverride = _runtimeOverrides.ContainsKey(feature);
                
                var status = runtimeState ? "✅ ENABLED" : "❌ DISABLED";
                var compileInfo = compileTimeState ? "compile-time enabled" : "compile-time disabled";
                var overrideInfo = hasOverride ? " (runtime override)" : "";
                
                report += $"- **{feature}**: {status} ({compileInfo}{overrideInfo})\n";
                report += $"  - {GetFeatureDescription(feature)}\n\n";
            }

            report += "## Configuration Validation\n\n";
            var states = GetAllFeatureStates();
            var validationPassed = ValidateConfigurationSilent(states);
            report += validationPassed ? "✅ All dependencies satisfied\n" : "⚠️ Configuration issues detected\n";

            report += $"\n*Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*\n";
            report += $"*Build Configuration: {GetBuildConfiguration()}*\n";

            return report;
        }

        /// <summary>
        /// Initialize runtime overrides from compile-time flags.
        /// </summary>
        private void InitializeFromCompileTimeFlags()
        {
            // Clear any existing overrides
            _runtimeOverrides.Clear();
            
            Debug.Log("Compiler flag manager initialized with compile-time configuration");
        }

        /// <summary>
        /// Validate configuration without logging (for report generation).
        /// </summary>
        private bool ValidateConfigurationSilent(Dictionary<ExperimentalFeature, bool> states)
        {
            var issues = 0;

            if (states[ExperimentalFeature.CLOUD_INFERENCE] && 
                !states[ExperimentalFeature.AI_INTEGRATION])
            {
                issues++;
            }

            if (states[ExperimentalFeature.ON_DEVICE_ML] && 
                !states[ExperimentalFeature.AI_INTEGRATION])
            {
                issues++;
            }

            return issues == 0;
        }

        /// <summary>
        /// Get feature description for documentation.
        /// </summary>
        private string GetFeatureDescription(ExperimentalFeature feature)
        {
            return feature switch
            {
                ExperimentalFeature.AI_INTEGRATION => "AI-enhanced bubble behavior and interaction systems",
                ExperimentalFeature.VOICE_PROCESSING => "Voice recognition and speech-to-text capabilities",
                ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS => "Experimental wave mathematics and advanced algorithms",
                ExperimentalFeature.CLOUD_INFERENCE => "Cloud-based AI inference for enhanced processing",
                ExperimentalFeature.ON_DEVICE_ML => "On-device machine learning and neural network processing",
                _ => "Unknown experimental feature"
            };
        }

        /// <summary>
        /// Get current build configuration information.
        /// </summary>
        private string GetBuildConfiguration()
        {
#if UNITY_EDITOR
            return "Editor (Development)";
#elif DEVELOPMENT_BUILD
            return "Development Build";
#else
            return "Release Build";
#endif
        }

        /// <summary>
        /// Reset all runtime overrides to compile-time defaults.
        /// Useful for testing and debugging.
        /// </summary>
        public void ResetToDefaults()
        {
            _runtimeOverrides.Clear();
            Debug.Log("All runtime overrides cleared - using compile-time defaults");
        }
    }
}