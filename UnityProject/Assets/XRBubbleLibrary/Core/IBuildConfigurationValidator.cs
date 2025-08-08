using System.Collections.Generic;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for build configuration validation.
    /// Provides dependency injection support for testing and modularity.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public interface IBuildConfigurationValidator
    {
        /// <summary>
        /// Perform comprehensive validation of the current build configuration.
        /// </summary>
        /// <returns>Validation result with all issues and recommendations</returns>
        BuildConfigurationValidator.ValidationResult ValidateConfiguration();
        
        /// <summary>
        /// Quick check if the current configuration is valid.
        /// </summary>
        /// <returns>True if configuration is valid, false otherwise</returns>
        bool IsConfigurationValid();
        
        /// <summary>
        /// Get a list of all validation rules for documentation.
        /// </summary>
        /// <returns>List of validation rules as human-readable strings</returns>
        List<string> GetValidationRules();
        
        /// <summary>
        /// Validate configuration and provide console logging.
        /// </summary>
        void ValidateAndLog();
    }
}