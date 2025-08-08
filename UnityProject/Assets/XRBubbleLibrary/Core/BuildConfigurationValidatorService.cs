using System.Collections.Generic;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Service implementation of build configuration validation.
    /// Provides dependency injection support and wraps the static validator.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class BuildConfigurationValidatorService : IBuildConfigurationValidator
    {
        /// <summary>
        /// Perform comprehensive validation of the current build configuration.
        /// </summary>
        public BuildConfigurationValidator.ValidationResult ValidateConfiguration()
        {
            return BuildConfigurationValidator.ValidateConfiguration();
        }
        
        /// <summary>
        /// Quick check if the current configuration is valid.
        /// </summary>
        public bool IsConfigurationValid()
        {
            return BuildConfigurationValidator.IsConfigurationValid();
        }
        
        /// <summary>
        /// Get a list of all validation rules for documentation.
        /// </summary>
        public List<string> GetValidationRules()
        {
            return BuildConfigurationValidator.GetValidationRules();
        }
        
        /// <summary>
        /// Validate configuration and provide console logging.
        /// </summary>
        public void ValidateAndLog()
        {
            BuildConfigurationValidator.ValidateAndLog();
        }
    }
}