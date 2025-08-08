using System.Collections.Generic;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for managing experimental feature compiler flags.
    /// Provides dependency injection support for testing and modularity.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public interface ICompilerFlagManager
    {
        /// <summary>
        /// Check if a specific experimental feature is currently enabled.
        /// </summary>
        /// <param name="feature">The experimental feature to check</param>
        /// <returns>True if the feature is enabled, false otherwise</returns>
        bool IsFeatureEnabled(ExperimentalFeature feature);

        /// <summary>
        /// Set the state of an experimental feature.
        /// Note: This only affects runtime checks, not compile-time exclusion.
        /// </summary>
        /// <param name="feature">The feature to configure</param>
        /// <param name="enabled">Whether the feature should be enabled</param>
        void SetFeatureState(ExperimentalFeature feature, bool enabled);

        /// <summary>
        /// Get the current state of all experimental features.
        /// </summary>
        /// <returns>Dictionary mapping features to their enabled state</returns>
        Dictionary<ExperimentalFeature, bool> GetAllFeatureStates();

        /// <summary>
        /// Validate the current configuration for consistency and dependencies.
        /// </summary>
        void ValidateConfiguration();

        /// <summary>
        /// Generate a human-readable status report of all features.
        /// </summary>
        /// <returns>Formatted status report string</returns>
        string GenerateStatusReport();
    }
}