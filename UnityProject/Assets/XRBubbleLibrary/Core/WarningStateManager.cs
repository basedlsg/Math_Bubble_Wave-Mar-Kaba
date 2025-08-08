using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Manages warning states and automatically updates warnings when feature states change.
    /// Implements automatic warning updates and validation system for Requirement 3.2-3.5.
    /// </summary>
    public class WarningStateManager : IDisposable
    {
        private readonly IReadmeWarningManager _warningManager;
        private readonly ICompilerFlagManager _compilerFlagManager;
        private readonly IDevStateGenerator _devStateGenerator;
        
        private readonly Dictionary<ExperimentalFeature, bool> _lastKnownFeatureStates;
        private readonly List<WarningStateChangeRecord> _stateChangeHistory;
        private readonly List<Action<WarningStateChangeEvent>> _stateChangeCallbacks;
        
        private string _lastKnownWarning;
        private DateTime _lastValidationTime;
        private bool _isMonitoring;
        private bool _disposed;
        
        private static WarningStateManager _instance;
        
        /// <summary>
        /// Singleton instance for global access.
        /// </summary>
        public static WarningStateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WarningStateManager();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Event fired when warning state changes.
        /// </summary>
        public event Action<WarningStateChangeEvent> WarningStateChanged;
        
        /// <summary>
        /// Whether automatic monitoring is enabled.
        /// </summary>
        public bool IsMonitoring => _isMonitoring;
        
        /// <summary>
        /// Initializes a new instance of the WarningStateManager.
        /// </summary>
        public WarningStateManager() : this(
            ReadmeWarningManager.Instance,
            CompilerFlagManager.Instance,
            DevStateGenerator.Instance)
        {
        }
        
        /// <summary>
        /// Initializes a new instance with dependency injection.
        /// </summary>
        public WarningStateManager(
            IReadmeWarningManager warningManager,
            ICompilerFlagManager compilerFlagManager,
            IDevStateGenerator devStateGenerator)
        {
            _warningManager = warningManager ?? throw new ArgumentNullException(nameof(warningManager));
            _compilerFlagManager = compilerFlagManager ?? throw new ArgumentNullException(nameof(compilerFlagManager));
            _devStateGenerator = devStateGenerator ?? throw new ArgumentNullException(nameof(devStateGenerator));
            
            _lastKnownFeatureStates = new Dictionary<ExperimentalFeature, bool>();
            _stateChangeHistory = new List<WarningStateChangeRecord>();
            _stateChangeCallbacks = new List<Action<WarningStateChangeEvent>>();
            
            InitializeState();
        }
        
        /// <summary>
        /// Starts automatic monitoring of feature state changes.
        /// </summary>
        public void StartMonitoring()
        {
            if (_isMonitoring)
            {
                Debug.LogWarning("[WarningStateManager] Monitoring is already active");
                return;
            }
            
            try
            {
                _isMonitoring = true;
                
                // Register for compiler flag changes
                RegisterForFeatureStateChanges();
                
                // Start periodic validation
                StartPeriodicValidation();
                
                Debug.Log("[WarningStateManager] Started automatic warning state monitoring");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Failed to start monitoring: {ex.Message}");
                _isMonitoring = false;
                throw;
            }
        }
        
        /// <summary>
        /// Stops automatic monitoring of feature state changes.
        /// </summary>
        public void StopMonitoring()
        {
            if (!_isMonitoring)
            {
                Debug.LogWarning("[WarningStateManager] Monitoring is not active");
                return;
            }
            
            try
            {
                _isMonitoring = false;
                
                // Unregister from compiler flag changes
                UnregisterFromFeatureStateChanges();
                
                Debug.Log("[WarningStateManager] Stopped automatic warning state monitoring");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Failed to stop monitoring: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Forces an immediate check and update of warning states.
        /// </summary>
        /// <returns>True if any warnings were updated, false otherwise</returns>
        public bool ForceStateUpdate()
        {
            try
            {
                Debug.Log("[WarningStateManager] Forcing warning state update...");
                
                var currentFeatureStates = _compilerFlagManager.GetAllFeatureStates();
                var stateChanged = DetectFeatureStateChanges(currentFeatureStates);
                
                if (stateChanged || ShouldForceUpdate())
                {
                    return ProcessStateChange(currentFeatureStates, "Manual force update");
                }
                
                Debug.Log("[WarningStateManager] No state changes detected, skipping update");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Failed to force state update: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validates the accuracy of current warning states.
        /// </summary>
        /// <returns>Validation result with any issues found</returns>
        public WarningStateValidationResult ValidateWarningAccuracy()
        {
            var result = new WarningStateValidationResult
            {
                ValidatedAt = DateTime.UtcNow
            };
            
            try
            {
                // Get current states
                var currentFeatureStates = _compilerFlagManager.GetAllFeatureStates();
                var currentWarning = _warningManager.GenerateWarningMessage();
                
                // Check README consistency
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                var readmePath = Path.Combine(projectRoot, "README.md");
                var devStatePath = Path.Combine(projectRoot, "DEV_STATE.md");
                
                var readmeValidation = _warningManager.ValidateConsistency(readmePath, devStatePath);
                result.ReadmeConsistent = readmeValidation.IsConsistent;
                result.ReadmeIssues.AddRange(readmeValidation.Inconsistencies);
                
                // Check feature state consistency
                var stateConsistency = ValidateFeatureStateConsistency(currentFeatureStates);
                result.FeatureStatesConsistent = stateConsistency.IsConsistent;
                result.FeatureStateIssues.AddRange(stateConsistency.Issues);
                
                // Check warning template consistency
                var templateConsistency = ValidateWarningTemplateConsistency(currentWarning);
                result.TemplateConsistent = templateConsistency.IsConsistent;
                result.TemplateIssues.AddRange(templateConsistency.Issues);
                
                // Overall validation
                result.IsValid = result.ReadmeConsistent && 
                                result.FeatureStatesConsistent && 
                                result.TemplateConsistent;
                
                result.CurrentWarning = currentWarning;
                result.FeatureStates = new Dictionary<ExperimentalFeature, bool>(currentFeatureStates);
                
                Debug.Log($"[WarningStateManager] Validation complete. Valid: {result.IsValid}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Validation failed: {ex.Message}");
                result.ValidationErrors.Add($"Validation error: {ex.Message}");
                result.IsValid = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets the history of warning state changes.
        /// </summary>
        /// <returns>List of state change records</returns>
        public List<WarningStateChangeRecord> GetStateChangeHistory()
        {
            return new List<WarningStateChangeRecord>(_stateChangeHistory);
        }
        
        /// <summary>
        /// Registers a callback to be notified when warning states change.
        /// </summary>
        /// <param name="callback">Callback to invoke when state changes</param>
        public void RegisterStateChangeCallback(Action<WarningStateChangeEvent> callback)
        {
            if (callback != null && !_stateChangeCallbacks.Contains(callback))
            {
                _stateChangeCallbacks.Add(callback);
                Debug.Log("[WarningStateManager] Registered state change callback");
            }
        }
        
        /// <summary>
        /// Unregisters a previously registered callback.
        /// </summary>
        /// <param name="callback">Callback to remove</param>
        public void UnregisterStateChangeCallback(Action<WarningStateChangeEvent> callback)
        {
            if (callback != null && _stateChangeCallbacks.Contains(callback))
            {
                _stateChangeCallbacks.Remove(callback);
                Debug.Log("[WarningStateManager] Unregistered state change callback");
            }
        }
        
        /// <summary>
        /// Gets comprehensive information about the current warning state.
        /// </summary>
        /// <returns>Current warning state information</returns>
        public WarningStateInfo GetCurrentState()
        {
            try
            {
                var currentFeatureStates = _compilerFlagManager.GetAllFeatureStates();
                var currentWarning = _warningManager.GenerateWarningMessage();
                var template = _warningManager.GetCurrentWarningTemplate();
                
                return new WarningStateInfo
                {
                    CurrentWarning = currentWarning,
                    FeatureStates = new Dictionary<ExperimentalFeature, bool>(currentFeatureStates),
                    WarningTemplate = template,
                    LastUpdated = DateTime.UtcNow,
                    IsMonitoring = _isMonitoring,
                    LastValidationTime = _lastValidationTime,
                    StateChangeCount = _stateChangeHistory.Count
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Failed to get current state: {ex.Message}");
                return new WarningStateInfo
                {
                    CurrentWarning = "Error: Unable to determine current state",
                    LastUpdated = DateTime.UtcNow,
                    IsMonitoring = _isMonitoring,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Performs cleanup and stops monitoring.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            
            try
            {
                StopMonitoring();
                _disposed = true;
                
                Debug.Log("[WarningStateManager] Disposed successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Error during disposal: {ex.Message}");
            }
        }
        
        #region Private Methods
        
        private void InitializeState()
        {
            try
            {
                // Initialize with current feature states
                var currentStates = _compilerFlagManager.GetAllFeatureStates();
                foreach (var state in currentStates)
                {
                    _lastKnownFeatureStates[state.Key] = state.Value;
                }
                
                // Initialize with current warning
                _lastKnownWarning = _warningManager.GenerateWarningMessage();
                _lastValidationTime = DateTime.UtcNow;
                
                Debug.Log("[WarningStateManager] Initialized with current system state");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Failed to initialize state: {ex.Message}");
            }
        }
        
        private void RegisterForFeatureStateChanges()
        {
            // Note: In a real implementation, this would register with the compiler flag manager
            // for change notifications. For now, we'll use periodic checking.
            
            // Register with warning manager for updates
            _warningManager.RegisterWarningUpdateCallback(OnWarningUpdated);
        }
        
        private void UnregisterFromFeatureStateChanges()
        {
            // Unregister from warning manager
            _warningManager.UnregisterWarningUpdateCallback(OnWarningUpdated);
        }
        
        private void OnWarningUpdated(string newWarning)
        {
            if (newWarning != _lastKnownWarning)
            {
                Debug.Log($"[WarningStateManager] Warning updated: {newWarning}");
                _lastKnownWarning = newWarning;
                
                // Trigger state change processing
                var currentFeatureStates = _compilerFlagManager.GetAllFeatureStates();
                ProcessStateChange(currentFeatureStates, "Warning manager update");
            }
        }
        
        private async void StartPeriodicValidation()
        {
            // Start periodic validation every 30 seconds
            while (_isMonitoring && !_disposed)
            {
                try
                {
                    await Task.Delay(30000); // 30 seconds
                    
                    if (_isMonitoring && !_disposed)
                    {
                        PerformPeriodicCheck();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WarningStateManager] Periodic validation error: {ex.Message}");
                }
            }
        }
        
        private void PerformPeriodicCheck()
        {
            try
            {
                var currentFeatureStates = _compilerFlagManager.GetAllFeatureStates();
                var stateChanged = DetectFeatureStateChanges(currentFeatureStates);
                
                if (stateChanged)
                {
                    Debug.Log("[WarningStateManager] Feature state changes detected during periodic check");
                    ProcessStateChange(currentFeatureStates, "Periodic check");
                }
                
                // Update last validation time
                _lastValidationTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Periodic check failed: {ex.Message}");
            }
        }
        
        private bool DetectFeatureStateChanges(Dictionary<ExperimentalFeature, bool> currentStates)
        {
            foreach (var currentState in currentStates)
            {
                var lastKnownState = _lastKnownFeatureStates.GetValueOrDefault(currentState.Key, false);
                if (currentState.Value != lastKnownState)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private bool ShouldForceUpdate()
        {
            // Force update if it's been more than 5 minutes since last validation
            return DateTime.UtcNow - _lastValidationTime > TimeSpan.FromMinutes(5);
        }
        
        private bool ProcessStateChange(Dictionary<ExperimentalFeature, bool> newFeatureStates, string trigger)
        {
            try
            {
                var changedFeatures = new List<FeatureStateChange>();
                
                // Identify changed features
                foreach (var newState in newFeatureStates)
                {
                    var oldState = _lastKnownFeatureStates.GetValueOrDefault(newState.Key, false);
                    if (newState.Value != oldState)
                    {
                        changedFeatures.Add(new FeatureStateChange
                        {
                            Feature = newState.Key,
                            OldState = oldState,
                            NewState = newState.Value
                        });
                    }
                }
                
                if (!changedFeatures.Any() && trigger != "Manual force update")
                {
                    return false;
                }
                
                // Generate new warning
                var oldWarning = _lastKnownWarning;
                var newWarning = _warningManager.GenerateWarningMessage();
                
                // Update README if warning changed
                bool readmeUpdated = false;
                if (newWarning != oldWarning)
                {
                    var projectRoot = Path.GetDirectoryName(Application.dataPath);
                    var readmePath = Path.Combine(projectRoot, "README.md");
                    
                    readmeUpdated = _warningManager.UpdateReadmeWarning(readmePath);
                }
                
                // Record the state change
                var changeRecord = new WarningStateChangeRecord
                {
                    ChangedAt = DateTime.UtcNow,
                    Trigger = trigger,
                    ChangedFeatures = changedFeatures,
                    OldWarning = oldWarning,
                    NewWarning = newWarning,
                    ReadmeUpdated = readmeUpdated,
                    OldFeatureStates = new Dictionary<ExperimentalFeature, bool>(_lastKnownFeatureStates),
                    NewFeatureStates = new Dictionary<ExperimentalFeature, bool>(newFeatureStates)
                };
                
                _stateChangeHistory.Add(changeRecord);
                
                // Keep only the last 100 records
                if (_stateChangeHistory.Count > 100)
                {
                    _stateChangeHistory.RemoveAt(0);
                }
                
                // Update known states
                foreach (var state in newFeatureStates)
                {
                    _lastKnownFeatureStates[state.Key] = state.Value;
                }
                _lastKnownWarning = newWarning;
                
                // Fire events
                var changeEvent = new WarningStateChangeEvent
                {
                    ChangeRecord = changeRecord,
                    ValidationResult = ValidateWarningAccuracy()
                };
                
                FireStateChangeEvent(changeEvent);
                
                Debug.Log($"[WarningStateManager] Processed state change. Features changed: {changedFeatures.Count}, README updated: {readmeUpdated}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Failed to process state change: {ex.Message}");
                return false;
            }
        }
        
        private void FireStateChangeEvent(WarningStateChangeEvent changeEvent)
        {
            try
            {
                // Fire event
                WarningStateChanged?.Invoke(changeEvent);
                
                // Notify callbacks
                foreach (var callback in _stateChangeCallbacks.ToList())
                {
                    try
                    {
                        callback(changeEvent);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[WarningStateManager] Callback failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WarningStateManager] Failed to fire state change event: {ex.Message}");
            }
        }
        
        private FeatureStateConsistencyResult ValidateFeatureStateConsistency(Dictionary<ExperimentalFeature, bool> currentStates)
        {
            var result = new FeatureStateConsistencyResult { IsConsistent = true };
            
            try
            {
                // Check if feature states are consistent with assembly definitions
                // This would involve checking .asmdef files for defineConstraints
                
                // For now, assume consistency (this would be expanded in a full implementation)
                result.IsConsistent = true;
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Feature state validation error: {ex.Message}");
                result.IsConsistent = false;
            }
            
            return result;
        }
        
        private WarningTemplateConsistencyResult ValidateWarningTemplateConsistency(string currentWarning)
        {
            var result = new WarningTemplateConsistencyResult { IsConsistent = true };
            
            try
            {
                var template = _warningManager.GetCurrentWarningTemplate();
                if (template == null)
                {
                    result.Issues.Add("No warning template found for current feature states");
                    result.IsConsistent = false;
                }
                else if (string.IsNullOrEmpty(currentWarning))
                {
                    result.Issues.Add("Current warning is empty but template exists");
                    result.IsConsistent = false;
                }
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Template validation error: {ex.Message}");
                result.IsConsistent = false;
            }
            
            return result;
        }
        
        #endregion
    }
    
    #region Data Structures
    
    /// <summary>
    /// Record of a warning state change event.
    /// </summary>
    [Serializable]
    public class WarningStateChangeRecord
    {
        public DateTime ChangedAt { get; set; }
        public string Trigger { get; set; }
        public List<FeatureStateChange> ChangedFeatures { get; set; } = new List<FeatureStateChange>();
        public string OldWarning { get; set; }
        public string NewWarning { get; set; }
        public bool ReadmeUpdated { get; set; }
        public Dictionary<ExperimentalFeature, bool> OldFeatureStates { get; set; }
        public Dictionary<ExperimentalFeature, bool> NewFeatureStates { get; set; }
    }
    
    /// <summary>
    /// Information about a single feature state change.
    /// </summary>
    [Serializable]
    public class FeatureStateChange
    {
        public ExperimentalFeature Feature { get; set; }
        public bool OldState { get; set; }
        public bool NewState { get; set; }
        
        public string Description => $"{Feature}: {(OldState ? "Enabled" : "Disabled")} → {(NewState ? "Enabled" : "Disabled")}";
    }
    
    /// <summary>
    /// Event data for warning state changes.
    /// </summary>
    public class WarningStateChangeEvent
    {
        public WarningStateChangeRecord ChangeRecord { get; set; }
        public WarningStateValidationResult ValidationResult { get; set; }
    }
    
    /// <summary>
    /// Result of warning state validation.
    /// </summary>
    [Serializable]
    public class WarningStateValidationResult
    {
        public bool IsValid { get; set; }
        public DateTime ValidatedAt { get; set; }
        public bool ReadmeConsistent { get; set; }
        public bool FeatureStatesConsistent { get; set; }
        public bool TemplateConsistent { get; set; }
        public List<string> ReadmeIssues { get; set; } = new List<string>();
        public List<string> FeatureStateIssues { get; set; } = new List<string>();
        public List<string> TemplateIssues { get; set; } = new List<string>();
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public string CurrentWarning { get; set; }
        public Dictionary<ExperimentalFeature, bool> FeatureStates { get; set; }
    }
    
    /// <summary>
    /// Information about the current warning state.
    /// </summary>
    [Serializable]
    public class WarningStateInfo
    {
        public string CurrentWarning { get; set; }
        public Dictionary<ExperimentalFeature, bool> FeatureStates { get; set; }
        public WarningTemplate WarningTemplate { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsMonitoring { get; set; }
        public DateTime LastValidationTime { get; set; }
        public int StateChangeCount { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// Result of feature state consistency validation.
    /// </summary>
    internal class FeatureStateConsistencyResult
    {
        public bool IsConsistent { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Result of warning template consistency validation.
    /// </summary>
    internal class WarningTemplateConsistencyResult
    {
        public bool IsConsistent { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
    }
    
    #endregion
}