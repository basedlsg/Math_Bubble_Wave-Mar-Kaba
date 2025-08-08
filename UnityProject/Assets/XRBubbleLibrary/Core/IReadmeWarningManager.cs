using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for managing dynamic README warning generation and updates.
    /// Implements Requirement 3: README Warning System from the "do-it-right" recovery specification.
    /// </summary>
    public interface IReadmeWarningManager
    {
        /// <summary>
        /// Generates the appropriate warning message based on current feature states.
        /// </summary>
        /// <returns>Warning message to display at the top of README</returns>
        string GenerateWarningMessage();
        
        /// <summary>
        /// Updates the README file with the current warning message.
        /// </summary>
        /// <param name="readmePath">Path to the README file to update</param>
        /// <returns>True if update was successful, false otherwise</returns>
        bool UpdateReadmeWarning(string readmePath);
        
        /// <summary>
        /// Updates the README file asynchronously.
        /// </summary>
        /// <param name="readmePath">Path to the README file to update</param>
        /// <returns>Task containing the update result</returns>
        Task<bool> UpdateReadmeWarningAsync(string readmePath);
        
        /// <summary>
        /// Validates that the README warning is consistent with DEV_STATE.md.
        /// </summary>
        /// <param name="readmePath">Path to the README file</param>
        /// <param name="devStatePath">Path to the DEV_STATE.md file</param>
        /// <returns>Validation result with any inconsistencies found</returns>
        ReadmeValidationResult ValidateConsistency(string readmePath, string devStatePath);
        
        /// <summary>
        /// Gets the current warning template based on feature states.
        /// </summary>
        /// <returns>Warning template information</returns>
        WarningTemplate GetCurrentWarningTemplate();
        
        /// <summary>
        /// Registers a callback to be notified when feature states change.
        /// </summary>
        /// <param name="callback">Callback to invoke when warnings need updating</param>
        void RegisterWarningUpdateCallback(Action<string> callback);
        
        /// <summary>
        /// Removes a previously registered callback.
        /// </summary>
        /// <param name="callback">Callback to remove</param>
        void UnregisterWarningUpdateCallback(Action<string> callback);
        
        /// <summary>
        /// Forces a refresh of the warning message based on current system state.
        /// </summary>
        void RefreshWarningState();
        
        /// <summary>
        /// Gets the history of warning updates for audit purposes.
        /// </summary>
        /// <returns>List of warning update records</returns>
        List<WarningUpdateRecord> GetWarningUpdateHistory();
    }
    
    /// <summary>
    /// Result of README validation against DEV_STATE.md.
    /// </summary>
    [Serializable]
    public class ReadmeValidationResult
    {
        /// <summary>
        /// Whether the README warning is consistent with DEV_STATE.md.
        /// </summary>
        public bool IsConsistent { get; set; }
        
        /// <summary>
        /// List of inconsistencies found during validation.
        /// </summary>
        public List<string> Inconsistencies { get; set; } = new List<string>();
        
        /// <summary>
        /// Current warning message in README.
        /// </summary>
        public string CurrentReadmeWarning { get; set; }
        
        /// <summary>
        /// Expected warning message based on DEV_STATE.md.
        /// </summary>
        public string ExpectedWarning { get; set; }
        
        /// <summary>
        /// When the validation was performed.
        /// </summary>
        public DateTime ValidatedAt { get; set; }
    }
    
    /// <summary>
    /// Template for generating warning messages.
    /// </summary>
    [Serializable]
    public class WarningTemplate
    {
        /// <summary>
        /// Unique identifier for the template.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Name of the warning template.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The warning message template with placeholders.
        /// </summary>
        public string Template { get; set; }
        
        /// <summary>
        /// Conditions that must be met for this template to be used.
        /// </summary>
        public WarningConditions Conditions { get; set; }
        
        /// <summary>
        /// Priority of this template (higher priority templates are preferred).
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Whether this template represents a critical warning.
        /// </summary>
        public bool IsCritical { get; set; }
    }
    
    /// <summary>
    /// Conditions for when a warning template should be used.
    /// </summary>
    [Serializable]
    public class WarningConditions
    {
        /// <summary>
        /// Required feature states for this template to apply.
        /// </summary>
        public Dictionary<ExperimentalFeature, bool> RequiredFeatureStates { get; set; } = new Dictionary<ExperimentalFeature, bool>();
        
        /// <summary>
        /// Whether all experimental features must be disabled.
        /// </summary>
        public bool AllExperimentalDisabled { get; set; }
        
        /// <summary>
        /// Whether all experimental features must be enabled.
        /// </summary>
        public bool AllExperimentalEnabled { get; set; }
        
        /// <summary>
        /// Whether validation must be complete for all features.
        /// </summary>
        public bool ValidationComplete { get; set; }
        
        /// <summary>
        /// Custom condition evaluation function name.
        /// </summary>
        public string CustomCondition { get; set; }
    }
    
    /// <summary>
    /// Record of a warning update operation.
    /// </summary>
    [Serializable]
    public class WarningUpdateRecord
    {
        /// <summary>
        /// When the update occurred.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Previous warning message.
        /// </summary>
        public string PreviousWarning { get; set; }
        
        /// <summary>
        /// New warning message.
        /// </summary>
        public string NewWarning { get; set; }
        
        /// <summary>
        /// Template used for the new warning.
        /// </summary>
        public string TemplateUsed { get; set; }
        
        /// <summary>
        /// Feature states at the time of update.
        /// </summary>
        public Dictionary<ExperimentalFeature, bool> FeatureStatesAtUpdate { get; set; }
        
        /// <summary>
        /// Whether the update was successful.
        /// </summary>
        public bool UpdateSuccessful { get; set; }
        
        /// <summary>
        /// Any error message if the update failed.
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Path to the README file that was updated.
        /// </summary>
        public string ReadmePath { get; set; }
    }
}