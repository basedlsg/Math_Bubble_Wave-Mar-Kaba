using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// High-level service for managing README warnings with automatic updates and monitoring.
    /// Provides convenient methods for integrating README warning management into the development workflow.
    /// </summary>
    public class ReadmeWarningService
    {
        private readonly IReadmeWarningManager _warningManager;
        private readonly ICompilerFlagManager _compilerFlagManager;
        private readonly string _projectReadmePath;
        private readonly string _devStatePath;
        
        /// <summary>
        /// Initializes a new instance of the ReadmeWarningService.
        /// </summary>
        public ReadmeWarningService() : this(ReadmeWarningManager.Instance, CompilerFlagManager.Instance)
        {
        }
        
        /// <summary>
        /// Initializes a new instance with dependency injection.
        /// </summary>
        /// <param name="warningManager">Warning manager instance</param>
        /// <param name="compilerFlagManager">Compiler flag manager instance</param>
        public ReadmeWarningService(IReadmeWarningManager warningManager, ICompilerFlagManager compilerFlagManager)
        {
            _warningManager = warningManager ?? throw new ArgumentNullException(nameof(warningManager));
            _compilerFlagManager = compilerFlagManager ?? throw new ArgumentNullException(nameof(compilerFlagManager));
            
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            _projectReadmePath = Path.Combine(projectRoot, "README.md");
            _devStatePath = Path.Combine(projectRoot, "DEV_STATE.md");
            
            // Register for compiler flag changes to automatically update warnings
            RegisterForAutomaticUpdates();
        }
        
        /// <summary>
        /// Updates the project README with the current warning message.
        /// </summary>
        /// <returns>True if update was successful, false otherwise</returns>
        public bool UpdateProjectReadme()
        {
            try
            {
                Debug.Log("[ReadmeWarningService] Updating project README warning...");
                return _warningManager.UpdateReadmeWarning(_projectReadmePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningService] Failed to update project README: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Updates the project README asynchronously.
        /// </summary>
        /// <returns>Task containing the update result</returns>
        public async Task<bool> UpdateProjectReadmeAsync()
        {
            return await _warningManager.UpdateReadmeWarningAsync(_projectReadmePath);
        }
        
        /// <summary>
        /// Validates that the README warning is consistent with the current system state.
        /// </summary>
        /// <returns>Validation result with any inconsistencies found</returns>
        public ReadmeValidationResult ValidateReadmeConsistency()
        {
            try
            {
                Debug.Log("[ReadmeWarningService] Validating README consistency...");
                return _warningManager.ValidateConsistency(_projectReadmePath, _devStatePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningService] Failed to validate README consistency: {ex.Message}");
                return new ReadmeValidationResult
                {
                    IsConsistent = false,
                    Inconsistencies = { $"Validation failed: {ex.Message}" },
                    ValidatedAt = DateTime.UtcNow
                };
            }
        }
        
        /// <summary>
        /// Gets the current warning message that should be displayed.
        /// </summary>
        /// <returns>Current warning message</returns>
        public string GetCurrentWarningMessage()
        {
            try
            {
                return _warningManager.GenerateWarningMessage();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningService] Failed to get current warning message: {ex.Message}");
                return "⚠️ Warning: Unable to determine current feature status. Please check system configuration.";
            }
        }
        
        /// <summary>
        /// Forces an immediate update of the README warning based on current feature states.
        /// </summary>
        /// <returns>True if update was successful, false otherwise</returns>
        public bool ForceWarningUpdate()
        {
            try
            {
                Debug.Log("[ReadmeWarningService] Forcing README warning update...");
                _warningManager.RefreshWarningState();
                return UpdateProjectReadme();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningService] Failed to force warning update: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Ensures the README file exists and has the correct warning.
        /// Creates the README if it doesn't exist.
        /// </summary>
        /// <returns>True if README is ready, false otherwise</returns>
        public bool EnsureReadmeExists()
        {
            try
            {
                if (!File.Exists(_projectReadmePath))
                {
                    Debug.Log("[ReadmeWarningService] Creating project README file...");
                    CreateDefaultReadme();
                }
                
                return UpdateProjectReadme();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningService] Failed to ensure README exists: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets comprehensive information about the current README warning status.
        /// </summary>
        /// <returns>README warning status information</returns>
        public ReadmeWarningStatus GetWarningStatus()
        {
            var status = new ReadmeWarningStatus
            {
                CheckedAt = DateTime.UtcNow,
                ReadmeExists = File.Exists(_projectReadmePath),
                DevStateExists = File.Exists(_devStatePath)
            };
            
            try
            {
                status.CurrentWarning = GetCurrentWarningMessage();
                status.WarningTemplate = _warningManager.GetCurrentWarningTemplate();
                
                if (status.ReadmeExists)
                {
                    var readmeContent = File.ReadAllText(_projectReadmePath);
                    status.ReadmeHasWarning = !string.IsNullOrEmpty(ExtractWarningFromContent(readmeContent));
                    status.ActualReadmeWarning = ExtractWarningFromContent(readmeContent);
                }
                
                var validation = ValidateReadmeConsistency();
                status.IsConsistent = validation.IsConsistent;
                status.ValidationIssues = validation.Inconsistencies;
                
                status.FeatureStates = _compilerFlagManager.GetAllFeatureStates();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningService] Failed to get warning status: {ex.Message}");
                status.ErrorMessage = ex.Message;
            }
            
            return status;
        }
        
        /// <summary>
        /// Registers a callback to be notified when the README warning is updated.
        /// </summary>
        /// <param name="callback">Callback to invoke when warning is updated</param>
        public void RegisterUpdateCallback(Action<string> callback)
        {
            _warningManager.RegisterWarningUpdateCallback(callback);
        }
        
        /// <summary>
        /// Unregisters a previously registered callback.
        /// </summary>
        /// <param name="callback">Callback to remove</param>
        public void UnregisterUpdateCallback(Action<string> callback)
        {
            _warningManager.UnregisterWarningUpdateCallback(callback);
        }
        
        /// <summary>
        /// Gets the history of README warning updates.
        /// </summary>
        /// <returns>List of warning update records</returns>
        public System.Collections.Generic.List<WarningUpdateRecord> GetUpdateHistory()
        {
            return _warningManager.GetWarningUpdateHistory();
        }
        
        #region Private Methods
        
        private void RegisterForAutomaticUpdates()
        {
            // Register callback to automatically update README when feature states change
            _warningManager.RegisterWarningUpdateCallback(warning =>
            {
                Debug.Log($"[ReadmeWarningService] Feature state changed, warning updated: {warning}");
            });
        }
        
        private void CreateDefaultReadme()
        {
            var defaultContent = @"# XR Bubble Library

A comprehensive XR bubble interaction library for Unity, optimized for Quest 3 hardware.

## Features

- Wave-based bubble mathematics system
- Quest 3 hardware optimization
- Evidence-based development practices
- Comprehensive testing and validation

## Getting Started

1. Import the XR Bubble Library package
2. Add the XRBubbleLibrary prefab to your scene
3. Configure bubble settings in the inspector
4. Run on Quest 3 hardware for optimal performance

## Documentation

- [Development State](DEV_STATE.md) - Current implementation status
- [Performance Budget](perf_budget.md) - Performance analysis and budgets
- [Evidence Reports](Evidence/Reports/) - Supporting evidence for all claims

## Requirements

- Unity 2023.3.5f1 LTS or newer
- Quest 3 hardware for validation
- XR Interaction Toolkit 2.5.4+

## License

[License information]
";
            
            File.WriteAllText(_projectReadmePath, defaultContent);
            Debug.Log($"[ReadmeWarningService] Created default README: {_projectReadmePath}");
        }
        
        private string ExtractWarningFromContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
            
            var lines = content.Split('\n');
            var firstLine = lines.Length > 0 ? lines[0].Trim() : string.Empty;
            
            // Check if the first line is a warning
            if (!string.IsNullOrEmpty(firstLine) && 
                (firstLine.StartsWith("⚠️") || firstLine.StartsWith("✅") || 
                 firstLine.Contains("disabled") || firstLine.Contains("experimental") || 
                 firstLine.Contains("validated")))
            {
                return firstLine;
            }
            
            return string.Empty;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Comprehensive status information about README warnings.
    /// </summary>
    [Serializable]
    public class ReadmeWarningStatus
    {
        /// <summary>
        /// When the status was checked.
        /// </summary>
        public DateTime CheckedAt { get; set; }
        
        /// <summary>
        /// Whether the README file exists.
        /// </summary>
        public bool ReadmeExists { get; set; }
        
        /// <summary>
        /// Whether the DEV_STATE.md file exists.
        /// </summary>
        public bool DevStateExists { get; set; }
        
        /// <summary>
        /// Whether the README has a warning message.
        /// </summary>
        public bool ReadmeHasWarning { get; set; }
        
        /// <summary>
        /// The current warning message that should be displayed.
        /// </summary>
        public string CurrentWarning { get; set; }
        
        /// <summary>
        /// The actual warning message in the README file.
        /// </summary>
        public string ActualReadmeWarning { get; set; }
        
        /// <summary>
        /// Whether the README warning is consistent with current feature states.
        /// </summary>
        public bool IsConsistent { get; set; }
        
        /// <summary>
        /// List of validation issues found.
        /// </summary>
        public System.Collections.Generic.List<string> ValidationIssues { get; set; } = new System.Collections.Generic.List<string>();
        
        /// <summary>
        /// Current feature states.
        /// </summary>
        public System.Collections.Generic.Dictionary<ExperimentalFeature, bool> FeatureStates { get; set; }
        
        /// <summary>
        /// Warning template being used.
        /// </summary>
        public WarningTemplate WarningTemplate { get; set; }
        
        /// <summary>
        /// Any error message if status check failed.
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Whether the README needs to be updated.
        /// </summary>
        public bool NeedsUpdate => !IsConsistent || CurrentWarning != ActualReadmeWarning;
    }
}