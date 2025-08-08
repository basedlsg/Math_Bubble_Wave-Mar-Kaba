using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Central management system for experimental feature compiler flags.
    /// Provides compile-time and runtime safety for feature gating.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public static class CompilerFlags
    {
        // Compile-time constants for zero runtime overhead when features are disabled
        public const bool AI_ENABLED = 
#if EXP_AI
            true;
#else
            false;
#endif

        public const bool VOICE_ENABLED = 
#if EXP_VOICE
            true;
#else
            false;
#endif

        public const bool ADVANCED_WAVE_ENABLED = 
#if EXP_ADVANCED_WAVE
            true;
#else
            false;
#endif

        public const bool CLOUD_INFERENCE_ENABLED = 
#if EXP_CLOUD_INFERENCE
            true;
#else
            false;
#endif

        public const bool ON_DEVICE_ML_ENABLED = 
#if EXP_ON_DEVICE_ML
            true;
#else
            false;
#endif

        /// <summary>
        /// Runtime validation for additional safety in editor builds.
        /// Completely compiled out in release builds for zero overhead.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void ValidateFeatureAccess(ExperimentalFeature feature)
        {
            if (!IsFeatureEnabled(feature))
            {
                throw new InvalidOperationException(
                    $"Feature {feature} is disabled. Enable via Project Settings > XR Bubble Library > Experimental Features");
            }
        }

        /// <summary>
        /// Check if a specific experimental feature is enabled.
        /// Uses compile-time constants for optimal performance.
        /// </summary>
        public static bool IsFeatureEnabled(ExperimentalFeature feature)
        {
            return feature switch
            {
                ExperimentalFeature.AI_INTEGRATION => AI_ENABLED,
                ExperimentalFeature.VOICE_PROCESSING => VOICE_ENABLED,
                ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS => ADVANCED_WAVE_ENABLED,
                ExperimentalFeature.CLOUD_INFERENCE => CLOUD_INFERENCE_ENABLED,
                ExperimentalFeature.ON_DEVICE_ML => ON_DEVICE_ML_ENABLED,
                _ => false
            };
        }

        /// <summary>
        /// Get all feature states for debugging and documentation purposes.
        /// Only available in editor builds.
        /// </summary>
        public static Dictionary<ExperimentalFeature, bool> GetAllFeatureStates()
        {
            return new Dictionary<ExperimentalFeature, bool>
            {
                { ExperimentalFeature.AI_INTEGRATION, AI_ENABLED },
                { ExperimentalFeature.VOICE_PROCESSING, VOICE_ENABLED },
                { ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS, ADVANCED_WAVE_ENABLED },
                { ExperimentalFeature.CLOUD_INFERENCE, CLOUD_INFERENCE_ENABLED },
                { ExperimentalFeature.ON_DEVICE_ML, ON_DEVICE_ML_ENABLED }
            };
        }

        /// <summary>
        /// Validate that the current configuration is consistent and safe.
        /// Only available in editor builds for development validation.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void ValidateConfiguration()
        {
            // Validate dependency relationships
            if (CLOUD_INFERENCE_ENABLED && !AI_ENABLED)
            {
                Debug.LogWarning("Cloud inference requires AI integration to be enabled");
            }

            if (ON_DEVICE_ML_ENABLED && !AI_ENABLED)
            {
                Debug.LogWarning("On-device ML requires AI integration to be enabled");
            }

            // Log current configuration for transparency
            Debug.Log($"XR Bubble Library Feature Configuration:\n" +
                     $"AI Integration: {AI_ENABLED}\n" +
                     $"Voice Processing: {VOICE_ENABLED}\n" +
                     $"Advanced Wave Algorithms: {ADVANCED_WAVE_ENABLED}\n" +
                     $"Cloud Inference: {CLOUD_INFERENCE_ENABLED}\n" +
                     $"On-Device ML: {ON_DEVICE_ML_ENABLED}");
        }

        /// <summary>
        /// Get a human-readable status report of all experimental features.
        /// Used for automated documentation generation.
        /// </summary>
        public static string GetFeatureStatusReport()
        {
            var report = "# Experimental Feature Status\n\n";
            
            foreach (ExperimentalFeature feature in Enum.GetValues(typeof(ExperimentalFeature)))
            {
                var status = IsFeatureEnabled(feature) ? "ENABLED" : "DISABLED";
                var description = GetFeatureDescription(feature);
                report += $"- **{feature}**: {status} - {description}\n";
            }

            report += $"\n*Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*\n";
            return report;
        }

        private static string GetFeatureDescription(ExperimentalFeature feature)
        {
            return feature switch
            {
                ExperimentalFeature.AI_INTEGRATION => "AI-enhanced bubble behavior and interaction",
                ExperimentalFeature.VOICE_PROCESSING => "Voice recognition and speech-to-text capabilities",
                ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS => "Experimental wave mathematics and algorithms",
                ExperimentalFeature.CLOUD_INFERENCE => "Cloud-based AI inference for enhanced features",
                ExperimentalFeature.ON_DEVICE_ML => "On-device machine learning processing",
                _ => "Unknown experimental feature"
            };
        }
    }

    /// <summary>
    /// Enumeration of all experimental features that can be controlled via compiler flags.
    /// </summary>
    public enum ExperimentalFeature
    {
        AI_INTEGRATION,
        VOICE_PROCESSING,
        ADVANCED_WAVE_ALGORITHMS,
        CLOUD_INFERENCE,
        ON_DEVICE_ML
    }
}