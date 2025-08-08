using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Manages dynamic README warning generation and updates.
    /// Implements Requirement 3: README Warning System from the "do-it-right" recovery specification.
    /// </summary>
    public class ReadmeWarningManager : IReadmeWarningManager
    {
        private readonly ICompilerFlagManager _compilerFlagManager;
        private readonly List<WarningTemplate> _warningTemplates;
        private readonly List<WarningUpdateRecord> _updateHistory;
        private readonly List<Action<string>> _updateCallbacks;
        
        private static ReadmeWarningManager _instance;
        
        /// <summary>
        /// Singleton instance for global access.
        /// </summary>
        public static ReadmeWarningManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ReadmeWarningManager();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the ReadmeWarningManager.
        /// </summary>
        public ReadmeWarningManager() : this(CompilerFlagManager.Instance)
        {
        }
        
        /// <summary>
        /// Initializes a new instance with dependency injection.
        /// </summary>
        /// <param name="compilerFlagManager">Compiler flag manager instance</param>
        public ReadmeWarningManager(ICompilerFlagManager compilerFlagManager)
        {
            _compilerFlagManager = compilerFlagManager ?? throw new ArgumentNullException(nameof(compilerFlagManager));
            _warningTemplates = new List<WarningTemplate>();
            _updateHistory = new List<WarningUpdateRecord>();
            _updateCallbacks = new List<Action<string>>();
            
            InitializeWarningTemplates();
        }
        
        /// <summary>
        /// Generates the appropriate warning message based on current feature states.
        /// </summary>
        public string GenerateWarningMessage()
        {
            try
            {
                var featureStates = _compilerFlagManager.GetAllFeatureStates();
                var template = GetCurrentWarningTemplate();
                
                if (template == null)
                {
                    Debug.LogWarning("[ReadmeWarningManager] No suitable warning template found, using default");
                    return GenerateDefaultWarning(featureStates);
                }
                
                return ProcessWarningTemplate(template, featureStates);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningManager] Failed to generate warning message: {ex.Message}");
                return "⚠️ Warning: Unable to determine current feature status. Please check system configuration.";
            }
        }
        
        /// <summary>
        /// Updates the README file with the current warning message.
        /// </summary>
        public bool UpdateReadmeWarning(string readmePath)
        {
            try
            {
                if (string.IsNullOrEmpty(readmePath) || !File.Exists(readmePath))
                {
                    Debug.LogError($"[ReadmeWarningManager] README file not found: {readmePath}");
                    return false;
                }
                
                var currentContent = File.ReadAllText(readmePath);
                var previousWarning = ExtractCurrentWarning(currentContent);
                var newWarning = GenerateWarningMessage();
                
                // Skip update if warning hasn't changed
                if (previousWarning == newWarning)
                {
                    Debug.Log("[ReadmeWarningManager] Warning unchanged, skipping update");
                    return true;
                }
                
                var updatedContent = UpdateReadmeContent(currentContent, newWarning);
                File.WriteAllText(readmePath, updatedContent);
                
                // Record the update
                RecordWarningUpdate(readmePath, previousWarning, newWarning, true, null);
                
                // Notify callbacks
                NotifyUpdateCallbacks(newWarning);
                
                Debug.Log($"[ReadmeWarningManager] Successfully updated README warning: {readmePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningManager] Failed to update README warning: {ex.Message}");
                RecordWarningUpdate(readmePath, null, null, false, ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Updates the README file asynchronously.
        /// </summary>
        public async Task<bool> UpdateReadmeWarningAsync(string readmePath)
        {
            return await Task.Run(() => UpdateReadmeWarning(readmePath));
        }
        
        /// <summary>
        /// Validates that the README warning is consistent with DEV_STATE.md.
        /// </summary>
        public ReadmeValidationResult ValidateConsistency(string readmePath, string devStatePath)
        {
            var result = new ReadmeValidationResult
            {
                ValidatedAt = DateTime.UtcNow
            };
            
            try
            {
                // Read current README warning
                if (File.Exists(readmePath))
                {
                    var readmeContent = File.ReadAllText(readmePath);
                    result.CurrentReadmeWarning = ExtractCurrentWarning(readmeContent);
                }
                else
                {
                    result.Inconsistencies.Add("README file not found");
                }
                
                // Generate expected warning based on current state
                result.ExpectedWarning = GenerateWarningMessage();
                
                // Check consistency
                if (result.CurrentReadmeWarning != result.ExpectedWarning)
                {
                    result.Inconsistencies.Add($"README warning does not match expected warning based on current feature states");
                }
                
                // Additional validation against DEV_STATE.md if available
                if (File.Exists(devStatePath))
                {
                    ValidateAgainstDevState(devStatePath, result);
                }
                else
                {
                    result.Inconsistencies.Add("DEV_STATE.md file not found for consistency check");
                }
                
                result.IsConsistent = result.Inconsistencies.Count == 0;
                
                Debug.Log($"[ReadmeWarningManager] Validation complete. Consistent: {result.IsConsistent}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningManager] Validation failed: {ex.Message}");
                result.Inconsistencies.Add($"Validation error: {ex.Message}");
                result.IsConsistent = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets the current warning template based on feature states.
        /// </summary>
        public WarningTemplate GetCurrentWarningTemplate()
        {
            try
            {
                var featureStates = _compilerFlagManager.GetAllFeatureStates();
                
                // Find templates that match current conditions
                var matchingTemplates = _warningTemplates
                    .Where(template => EvaluateTemplateConditions(template, featureStates))
                    .OrderByDescending(template => template.Priority)
                    .ToList();
                
                return matchingTemplates.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningManager] Failed to get current warning template: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Registers a callback to be notified when feature states change.
        /// </summary>
        public void RegisterWarningUpdateCallback(Action<string> callback)
        {
            if (callback != null && !_updateCallbacks.Contains(callback))
            {
                _updateCallbacks.Add(callback);
                Debug.Log("[ReadmeWarningManager] Registered warning update callback");
            }
        }
        
        /// <summary>
        /// Removes a previously registered callback.
        /// </summary>
        public void UnregisterWarningUpdateCallback(Action<string> callback)
        {
            if (callback != null && _updateCallbacks.Contains(callback))
            {
                _updateCallbacks.Remove(callback);
                Debug.Log("[ReadmeWarningManager] Unregistered warning update callback");
            }
        }
        
        /// <summary>
        /// Forces a refresh of the warning state based on current system state.
        /// </summary>
        public void RefreshWarningState()
        {
            try
            {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                var readmePath = Path.Combine(projectRoot, "README.md");
                
                if (File.Exists(readmePath))
                {
                    UpdateReadmeWarning(readmePath);
                }
                else
                {
                    Debug.LogWarning("[ReadmeWarningManager] README.md not found in project root");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningManager] Failed to refresh warning state: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Gets the history of warning updates for audit purposes.
        /// </summary>
        public List<WarningUpdateRecord> GetWarningUpdateHistory()
        {
            return new List<WarningUpdateRecord>(_updateHistory);
        }
        
        #region Private Methods
        
        private void InitializeWarningTemplates()
        {
            _warningTemplates.Clear();
            
            // Template for when all experimental features are disabled (default state)
            _warningTemplates.Add(new WarningTemplate
            {
                Id = "all_experimental_disabled",
                Name = "All Experimental Features Disabled",
                Template = "⚠️ AI & voice features are disabled until validated on Quest 3",
                Priority = 100,
                IsCritical = true,
                Conditions = new WarningConditions
                {
                    AllExperimentalDisabled = true
                }
            });
            
            // Template for when AI is enabled but voice is disabled
            _warningTemplates.Add(new WarningTemplate
            {
                Id = "ai_enabled_voice_disabled",
                Name = "AI Enabled, Voice Disabled",
                Template = "⚠️ Voice features are disabled until validated on Quest 3. AI features are experimental.",
                Priority = 80,
                IsCritical = false,
                Conditions = new WarningConditions
                {
                    RequiredFeatureStates = new Dictionary<ExperimentalFeature, bool>
                    {
                        { ExperimentalFeature.AI_INTEGRATION, true },
                        { ExperimentalFeature.VOICE_PROCESSING, false }
                    }
                }
            });
            
            // Template for when voice is enabled but AI is disabled
            _warningTemplates.Add(new WarningTemplate
            {
                Id = "voice_enabled_ai_disabled",
                Name = "Voice Enabled, AI Disabled",
                Template = "⚠️ AI features are disabled until validated on Quest 3. Voice features are experimental.",
                Priority = 80,
                IsCritical = false,
                Conditions = new WarningConditions
                {
                    RequiredFeatureStates = new Dictionary<ExperimentalFeature, bool>
                    {
                        { ExperimentalFeature.AI_INTEGRATION, false },
                        { ExperimentalFeature.VOICE_PROCESSING, true }
                    }
                }
            });
            
            // Template for when both AI and voice are enabled
            _warningTemplates.Add(new WarningTemplate
            {
                Id = "ai_voice_enabled",
                Name = "AI and Voice Enabled",
                Template = "⚠️ AI & voice features are experimental and not yet validated on Quest 3",
                Priority = 70,
                IsCritical = false,
                Conditions = new WarningConditions
                {
                    RequiredFeatureStates = new Dictionary<ExperimentalFeature, bool>
                    {
                        { ExperimentalFeature.AI_INTEGRATION, true },
                        { ExperimentalFeature.VOICE_PROCESSING, true }
                    }
                }
            });
            
            // Template for when all features are validated (future state)
            _warningTemplates.Add(new WarningTemplate
            {
                Id = "all_validated",
                Name = "All Features Validated",
                Template = "✅ All features have been validated on Quest 3 hardware",
                Priority = 50,
                IsCritical = false,
                Conditions = new WarningConditions
                {
                    ValidationComplete = true
                }
            });
            
            Debug.Log($"[ReadmeWarningManager] Initialized {_warningTemplates.Count} warning templates");
        }
        
        private bool EvaluateTemplateConditions(WarningTemplate template, Dictionary<ExperimentalFeature, bool> featureStates)
        {
            var conditions = template.Conditions;
            
            // Check if all experimental features should be disabled
            if (conditions.AllExperimentalDisabled)
            {
                var experimentalFeatures = new[]
                {
                    ExperimentalFeature.AI_INTEGRATION,
                    ExperimentalFeature.VOICE_PROCESSING,
                    ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS,
                    ExperimentalFeature.CLOUD_INFERENCE,
                    ExperimentalFeature.ON_DEVICE_ML
                };
                
                return experimentalFeatures.All(feature => 
                    !featureStates.ContainsKey(feature) || !featureStates[feature]);
            }
            
            // Check if all experimental features should be enabled
            if (conditions.AllExperimentalEnabled)
            {
                var experimentalFeatures = new[]
                {
                    ExperimentalFeature.AI_INTEGRATION,
                    ExperimentalFeature.VOICE_PROCESSING,
                    ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS,
                    ExperimentalFeature.CLOUD_INFERENCE,
                    ExperimentalFeature.ON_DEVICE_ML
                };
                
                return experimentalFeatures.All(feature => 
                    featureStates.ContainsKey(feature) && featureStates[feature]);
            }
            
            // Check specific feature state requirements
            if (conditions.RequiredFeatureStates.Any())
            {
                return conditions.RequiredFeatureStates.All(requirement =>
                    featureStates.ContainsKey(requirement.Key) && 
                    featureStates[requirement.Key] == requirement.Value);
            }
            
            // Check validation complete condition (placeholder for future implementation)
            if (conditions.ValidationComplete)
            {
                // TODO: Implement validation status checking when validation system is available
                return false; // For now, validation is never complete
            }
            
            return false;
        }
        
        private string ProcessWarningTemplate(WarningTemplate template, Dictionary<ExperimentalFeature, bool> featureStates)
        {
            var message = template.Template;
            
            // Replace placeholders with actual values (if any)
            // For now, templates don't have placeholders, but this allows for future expansion
            
            return message;
        }
        
        private string GenerateDefaultWarning(Dictionary<ExperimentalFeature, bool> featureStates)
        {
            var disabledFeatures = new List<string>();
            
            if (!featureStates.GetValueOrDefault(ExperimentalFeature.AI_INTEGRATION, false))
            {
                disabledFeatures.Add("AI");
            }
            
            if (!featureStates.GetValueOrDefault(ExperimentalFeature.VOICE_PROCESSING, false))
            {
                disabledFeatures.Add("voice");
            }
            
            if (disabledFeatures.Count == 0)
            {
                return "⚠️ Experimental features are enabled and not yet validated on Quest 3";
            }
            
            var featuresText = string.Join(" & ", disabledFeatures);
            return $"⚠️ {featuresText} features are disabled until validated on Quest 3";
        }
        
        private string ExtractCurrentWarning(string readmeContent)
        {
            if (string.IsNullOrEmpty(readmeContent))
                return string.Empty;
            
            var lines = readmeContent.Split('\n');
            var firstLine = lines.FirstOrDefault()?.Trim();
            
            // Check if the first line is a warning (starts with warning emoji or contains warning keywords)
            if (!string.IsNullOrEmpty(firstLine) && 
                (firstLine.StartsWith("⚠️") || firstLine.StartsWith("✅") || 
                 firstLine.Contains("disabled") || firstLine.Contains("experimental") || 
                 firstLine.Contains("validated")))
            {
                return firstLine;
            }
            
            return string.Empty;
        }
        
        private string UpdateReadmeContent(string currentContent, string newWarning)
        {
            if (string.IsNullOrEmpty(currentContent))
            {
                return newWarning + "\n\n# XR Bubble Library\n\n";
            }
            
            var lines = currentContent.Split('\n').ToList();
            var firstLine = lines.FirstOrDefault()?.Trim();
            
            // Check if first line is already a warning
            if (!string.IsNullOrEmpty(firstLine) && 
                (firstLine.StartsWith("⚠️") || firstLine.StartsWith("✅") || 
                 firstLine.Contains("disabled") || firstLine.Contains("experimental") || 
                 firstLine.Contains("validated")))
            {
                // Replace existing warning
                lines[0] = newWarning;
            }
            else
            {
                // Insert warning at the beginning
                lines.Insert(0, newWarning);
                lines.Insert(1, ""); // Add blank line after warning
            }
            
            return string.Join("\n", lines);
        }
        
        private void ValidateAgainstDevState(string devStatePath, ReadmeValidationResult result)
        {
            try
            {
                var devStateContent = File.ReadAllText(devStatePath);
                
                // Extract feature states from DEV_STATE.md
                var aiDisabled = devStateContent.Contains("AI_INTEGRATION: ❌ DISABLED") || 
                                devStateContent.Contains("**AI_INTEGRATION**: ❌ DISABLED");
                var voiceDisabled = devStateContent.Contains("VOICE_PROCESSING: ❌ DISABLED") || 
                                   devStateContent.Contains("**VOICE_PROCESSING**: ❌ DISABLED");
                
                // Validate consistency with README warning
                var currentWarning = result.CurrentReadmeWarning?.ToLower() ?? "";
                
                if (aiDisabled && !currentWarning.Contains("ai"))
                {
                    result.Inconsistencies.Add("AI is disabled in DEV_STATE.md but not mentioned in README warning");
                }
                
                if (voiceDisabled && !currentWarning.Contains("voice"))
                {
                    result.Inconsistencies.Add("Voice is disabled in DEV_STATE.md but not mentioned in README warning");
                }
            }
            catch (Exception ex)
            {
                result.Inconsistencies.Add($"Failed to validate against DEV_STATE.md: {ex.Message}");
            }
        }
        
        private void RecordWarningUpdate(string readmePath, string previousWarning, string newWarning, bool successful, string errorMessage)
        {
            var record = new WarningUpdateRecord
            {
                UpdatedAt = DateTime.UtcNow,
                PreviousWarning = previousWarning,
                NewWarning = newWarning,
                TemplateUsed = GetCurrentWarningTemplate()?.Id,
                FeatureStatesAtUpdate = _compilerFlagManager.GetAllFeatureStates(),
                UpdateSuccessful = successful,
                ErrorMessage = errorMessage,
                ReadmePath = readmePath
            };
            
            _updateHistory.Add(record);
            
            // Keep only the last 100 records to prevent memory issues
            if (_updateHistory.Count > 100)
            {
                _updateHistory.RemoveAt(0);
            }
        }
        
        private void NotifyUpdateCallbacks(string newWarning)
        {
            foreach (var callback in _updateCallbacks.ToList()) // ToList to avoid modification during iteration
            {
                try
                {
                    callback(newWarning);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ReadmeWarningManager] Callback failed: {ex.Message}");
                }
            }
        }
        
        #endregion
    }
}