using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XRBubbleLibrary.Core.Editor
{
    /// <summary>
    /// Unity Editor integration for the README Warning System.
    /// Provides GUI tools for managing README warnings and validating consistency.
    /// </summary>
    public class ReadmeWarningEditor : EditorWindow
    {
        private ReadmeWarningService _warningService;
        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private string[] _tabNames = { "Status", "Templates", "History", "Validation" };
        
        // Status tab
        private ReadmeWarningStatus _currentStatus;
        private bool _isRefreshing = false;
        
        // Templates tab
        private WarningTemplate _selectedTemplate;
        
        // History tab
        private System.Collections.Generic.List<WarningUpdateRecord> _updateHistory;
        
        // Validation tab
        private ReadmeValidationResult _lastValidation;
        private bool _isValidating = false;
        
        [MenuItem("XR Bubble Library/README Warning System")]
        public static void ShowWindow()
        {
            var window = GetWindow<ReadmeWarningEditor>("README Warning System");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            _warningService = new ReadmeWarningService();
            RefreshStatus();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            EditorGUILayout.LabelField("README Warning System", EditorStyles.largeLabel);
            EditorGUILayout.LabelField("Manage dynamic README warnings based on feature states", EditorStyles.helpBox);
            EditorGUILayout.Space();
            
            // Quick actions
            DrawQuickActions();
            EditorGUILayout.Space();
            
            // Tab selection
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            EditorGUILayout.Space();
            
            // Tab content
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            switch (_selectedTab)
            {
                case 0:
                    DrawStatusTab();
                    break;
                case 1:
                    DrawTemplatesTab();
                    break;
                case 2:
                    DrawHistoryTab();
                    break;
                case 3:
                    DrawValidationTab();
                    break;
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        #region Quick Actions
        
        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !_isRefreshing;
            if (GUILayout.Button("Update README", GUILayout.Height(30)))
            {
                UpdateReadme();
            }
            
            if (GUILayout.Button("Refresh Status", GUILayout.Height(30)))
            {
                RefreshStatus();
            }
            
            if (GUILayout.Button("Validate Consistency", GUILayout.Height(30)))
            {
                ValidateConsistency();
            }
            
            if (GUILayout.Button("Force Update", GUILayout.Height(30)))
            {
                ForceUpdate();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (_isRefreshing)
            {
                EditorGUILayout.HelpBox("Refreshing status...", MessageType.Info);
            }
        }
        
        #endregion
        
        #region Status Tab
        
        private void DrawStatusTab()
        {
            EditorGUILayout.LabelField("Current Status", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (_currentStatus == null)
            {
                EditorGUILayout.HelpBox("Status not loaded. Click 'Refresh Status' to load current information.", MessageType.Info);
                return;
            }
            
            // File existence status
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("File Status", EditorStyles.boldLabel);
            
            DrawStatusIndicator("README.md exists", _currentStatus.ReadmeExists);
            DrawStatusIndicator("DEV_STATE.md exists", _currentStatus.DevStateExists);
            DrawStatusIndicator("README has warning", _currentStatus.ReadmeHasWarning);
            DrawStatusIndicator("Warning is consistent", _currentStatus.IsConsistent);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            
            // Current warning information
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Warning Information", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Expected Warning:");
            EditorGUILayout.SelectableLabel(_currentStatus.CurrentWarning ?? "None", EditorStyles.wordWrappedLabel);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Actual README Warning:");
            EditorGUILayout.SelectableLabel(_currentStatus.ActualReadmeWarning ?? "None", EditorStyles.wordWrappedLabel);
            
            if (_currentStatus.WarningTemplate != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Template: {_currentStatus.WarningTemplate.Name}");
                EditorGUILayout.LabelField($"Priority: {_currentStatus.WarningTemplate.Priority}");
                EditorGUILayout.LabelField($"Critical: {(_currentStatus.WarningTemplate.IsCritical ? "Yes" : "No")}");
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            
            // Feature states
            if (_currentStatus.FeatureStates != null && _currentStatus.FeatureStates.Any())
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Feature States", EditorStyles.boldLabel);
                
                foreach (var feature in _currentStatus.FeatureStates)
                {
                    DrawStatusIndicator(feature.Key.ToString(), feature.Value);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            
            // Validation issues
            if (_currentStatus.ValidationIssues != null && _currentStatus.ValidationIssues.Any())
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Validation Issues", EditorStyles.boldLabel);
                
                foreach (var issue in _currentStatus.ValidationIssues)
                {
                    EditorGUILayout.HelpBox(issue, MessageType.Warning);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // Error message
            if (!string.IsNullOrEmpty(_currentStatus.ErrorMessage))
            {
                EditorGUILayout.HelpBox($"Error: {_currentStatus.ErrorMessage}", MessageType.Error);
            }
            
            // Update recommendation
            if (_currentStatus.NeedsUpdate)
            {
                EditorGUILayout.HelpBox("README needs to be updated to match current feature states.", MessageType.Warning);
                
                if (GUILayout.Button("Update README Now"))
                {
                    UpdateReadme();
                }
            }
        }
        
        private void DrawStatusIndicator(string label, bool status)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            
            var color = status ? Color.green : Color.red;
            var symbol = status ? "✅" : "❌";
            
            var originalColor = GUI.color;
            GUI.color = color;
            EditorGUILayout.LabelField(symbol, GUILayout.Width(30));
            GUI.color = originalColor;
            
            EditorGUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Templates Tab
        
        private void DrawTemplatesTab()
        {
            EditorGUILayout.LabelField("Warning Templates", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            var currentTemplate = _warningService.GetWarningStatus()?.WarningTemplate;
            
            if (currentTemplate != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Current Template", EditorStyles.boldLabel);
                
                EditorGUILayout.LabelField($"ID: {currentTemplate.Id}");
                EditorGUILayout.LabelField($"Name: {currentTemplate.Name}");
                EditorGUILayout.LabelField($"Priority: {currentTemplate.Priority}");
                EditorGUILayout.LabelField($"Critical: {(currentTemplate.IsCritical ? "Yes" : "No")}");
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Template:");
                EditorGUILayout.SelectableLabel(currentTemplate.Template, EditorStyles.wordWrappedLabel);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Conditions:");
                
                if (currentTemplate.Conditions != null)
                {
                    if (currentTemplate.Conditions.AllExperimentalDisabled)
                    {
                        EditorGUILayout.LabelField("• All experimental features disabled");
                    }
                    
                    if (currentTemplate.Conditions.AllExperimentalEnabled)
                    {
                        EditorGUILayout.LabelField("• All experimental features enabled");
                    }
                    
                    if (currentTemplate.Conditions.ValidationComplete)
                    {
                        EditorGUILayout.LabelField("• Validation complete");
                    }
                    
                    if (currentTemplate.Conditions.RequiredFeatureStates?.Any() == true)
                    {
                        EditorGUILayout.LabelField("• Specific feature states:");
                        foreach (var requirement in currentTemplate.Conditions.RequiredFeatureStates)
                        {
                            EditorGUILayout.LabelField($"  - {requirement.Key}: {(requirement.Value ? "Enabled" : "Disabled")}");
                        }
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("No template information available. Refresh status to load template data.", MessageType.Info);
            }
        }
        
        #endregion
        
        #region History Tab
        
        private void DrawHistoryTab()
        {
            EditorGUILayout.LabelField("Update History", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh History"))
            {
                RefreshHistory();
            }
            
            if (_updateHistory != null && _updateHistory.Any())
            {
                if (GUILayout.Button("Clear History"))
                {
                    if (EditorUtility.DisplayDialog("Clear History", 
                        "Are you sure you want to clear the update history?", "Yes", "No"))
                    {
                        _updateHistory.Clear();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            if (_updateHistory == null)
            {
                EditorGUILayout.HelpBox("History not loaded. Click 'Refresh History' to load update records.", MessageType.Info);
                return;
            }
            
            if (!_updateHistory.Any())
            {
                EditorGUILayout.HelpBox("No update history available.", MessageType.Info);
                return;
            }
            
            // Display history records
            foreach (var record in _updateHistory.OrderByDescending(r => r.UpdatedAt).Take(20))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{record.UpdatedAt:yyyy-MM-dd HH:mm:ss}", EditorStyles.boldLabel);
                
                var statusColor = record.UpdateSuccessful ? Color.green : Color.red;
                var statusSymbol = record.UpdateSuccessful ? "✅" : "❌";
                
                var originalColor = GUI.color;
                GUI.color = statusColor;
                EditorGUILayout.LabelField(statusSymbol, GUILayout.Width(30));
                GUI.color = originalColor;
                
                EditorGUILayout.EndHorizontal();
                
                if (!string.IsNullOrEmpty(record.PreviousWarning))
                {
                    EditorGUILayout.LabelField("Previous:", EditorStyles.miniLabel);
                    EditorGUILayout.SelectableLabel(record.PreviousWarning, EditorStyles.wordWrappedMiniLabel);
                }
                
                if (!string.IsNullOrEmpty(record.NewWarning))
                {
                    EditorGUILayout.LabelField("New:", EditorStyles.miniLabel);
                    EditorGUILayout.SelectableLabel(record.NewWarning, EditorStyles.wordWrappedMiniLabel);
                }
                
                if (!string.IsNullOrEmpty(record.TemplateUsed))
                {
                    EditorGUILayout.LabelField($"Template: {record.TemplateUsed}", EditorStyles.miniLabel);
                }
                
                if (!string.IsNullOrEmpty(record.ErrorMessage))
                {
                    EditorGUILayout.HelpBox($"Error: {record.ErrorMessage}", MessageType.Error);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        
        #endregion
        
        #region Validation Tab
        
        private void DrawValidationTab()
        {
            EditorGUILayout.LabelField("Consistency Validation", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !_isValidating;
            if (GUILayout.Button("Run Validation", GUILayout.Height(30)))
            {
                ValidateConsistency();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            if (_isValidating)
            {
                EditorGUILayout.HelpBox("Running validation...", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            if (_lastValidation == null)
            {
                EditorGUILayout.HelpBox("No validation results available. Click 'Run Validation' to check consistency.", MessageType.Info);
                return;
            }
            
            // Validation summary
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Validation Summary", EditorStyles.boldLabel);
            
            DrawStatusIndicator("README is consistent", _lastValidation.IsConsistent);
            EditorGUILayout.LabelField($"Validated at: {_lastValidation.ValidatedAt:yyyy-MM-dd HH:mm:ss}");
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            
            // Current vs Expected
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Warning Comparison", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Current README Warning:");
            EditorGUILayout.SelectableLabel(_lastValidation.CurrentReadmeWarning ?? "None", EditorStyles.wordWrappedLabel);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Expected Warning:");
            EditorGUILayout.SelectableLabel(_lastValidation.ExpectedWarning ?? "None", EditorStyles.wordWrappedLabel);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            
            // Inconsistencies
            if (_lastValidation.Inconsistencies != null && _lastValidation.Inconsistencies.Any())
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Inconsistencies Found", EditorStyles.boldLabel);
                
                foreach (var inconsistency in _lastValidation.Inconsistencies)
                {
                    EditorGUILayout.HelpBox(inconsistency, MessageType.Warning);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Fix Inconsistencies"))
                {
                    UpdateReadme();
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void RefreshStatus()
        {
            _isRefreshing = true;
            
            try
            {
                _currentStatus = _warningService.GetWarningStatus();
                Debug.Log("[ReadmeWarningEditor] Status refreshed successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningEditor] Failed to refresh status: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to refresh status: {ex.Message}", "OK");
            }
            finally
            {
                _isRefreshing = false;
            }
        }
        
        private void UpdateReadme()
        {
            try
            {
                var success = _warningService.UpdateProjectReadme();
                
                if (success)
                {
                    Debug.Log("[ReadmeWarningEditor] README updated successfully");
                    EditorUtility.DisplayDialog("Success", "README warning updated successfully!", "OK");
                    RefreshStatus();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to update README warning. Check console for details.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningEditor] Failed to update README: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to update README: {ex.Message}", "OK");
            }
        }
        
        private void ValidateConsistency()
        {
            _isValidating = true;
            
            try
            {
                _lastValidation = _warningService.ValidateReadmeConsistency();
                Debug.Log($"[ReadmeWarningEditor] Validation complete. Consistent: {_lastValidation.IsConsistent}");
                
                if (_lastValidation.IsConsistent)
                {
                    EditorUtility.DisplayDialog("Validation Complete", "README warning is consistent with current feature states.", "OK");
                }
                else
                {
                    var message = $"Found {_lastValidation.Inconsistencies.Count} inconsistencies:\n\n" +
                                 string.Join("\n", _lastValidation.Inconsistencies.Take(3));
                    
                    if (_lastValidation.Inconsistencies.Count > 3)
                    {
                        message += $"\n... and {_lastValidation.Inconsistencies.Count - 3} more";
                    }
                    
                    EditorUtility.DisplayDialog("Validation Issues Found", message, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningEditor] Validation failed: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Validation failed: {ex.Message}", "OK");
            }
            finally
            {
                _isValidating = false;
            }
        }
        
        private void ForceUpdate()
        {
            try
            {
                var success = _warningService.ForceWarningUpdate();
                
                if (success)
                {
                    Debug.Log("[ReadmeWarningEditor] Forced update completed successfully");
                    EditorUtility.DisplayDialog("Success", "README warning force updated successfully!", "OK");
                    RefreshStatus();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to force update README warning. Check console for details.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningEditor] Failed to force update: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to force update: {ex.Message}", "OK");
            }
        }
        
        private void RefreshHistory()
        {
            try
            {
                _updateHistory = _warningService.GetUpdateHistory();
                Debug.Log($"[ReadmeWarningEditor] Loaded {_updateHistory.Count} history records");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarningEditor] Failed to refresh history: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to refresh history: {ex.Message}", "OK");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Menu items for quick README warning operations.
    /// </summary>
    public static class ReadmeWarningMenuItems
    {
        [MenuItem("XR Bubble Library/README/Update Warning")]
        public static void UpdateReadmeWarning()
        {
            try
            {
                var service = new ReadmeWarningService();
                var success = service.UpdateProjectReadme();
                
                if (success)
                {
                    Debug.Log("[ReadmeWarning] README warning updated successfully");
                    EditorUtility.DisplayDialog("Success", "README warning updated successfully!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to update README warning. Check console for details.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarning] Failed to update README warning: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to update README warning: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/README/Validate Consistency")]
        public static void ValidateReadmeConsistency()
        {
            try
            {
                var service = new ReadmeWarningService();
                var result = service.ValidateReadmeConsistency();
                
                if (result.IsConsistent)
                {
                    Debug.Log("[ReadmeWarning] README validation passed");
                    EditorUtility.DisplayDialog("Validation Passed", "README warning is consistent with current feature states.", "OK");
                }
                else
                {
                    Debug.LogWarning($"[ReadmeWarning] README validation found {result.Inconsistencies.Count} issues");
                    
                    var message = $"Found {result.Inconsistencies.Count} inconsistencies:\n\n" +
                                 string.Join("\n", result.Inconsistencies.Take(3));
                    
                    if (result.Inconsistencies.Count > 3)
                    {
                        message += $"\n... and {result.Inconsistencies.Count - 3} more";
                    }
                    
                    if (EditorUtility.DisplayDialog("Validation Issues Found", message, "Fix Now", "OK"))
                    {
                        UpdateReadmeWarning();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarning] Failed to validate README consistency: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to validate README consistency: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/README/Force Update")]
        public static void ForceReadmeUpdate()
        {
            try
            {
                var service = new ReadmeWarningService();
                var success = service.ForceWarningUpdate();
                
                if (success)
                {
                    Debug.Log("[ReadmeWarning] README force update completed successfully");
                    EditorUtility.DisplayDialog("Success", "README warning force updated successfully!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to force update README warning. Check console for details.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarning] Failed to force update README warning: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to force update README warning: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/README/Ensure README Exists")]
        public static void EnsureReadmeExists()
        {
            try
            {
                var service = new ReadmeWarningService();
                var success = service.EnsureReadmeExists();
                
                if (success)
                {
                    Debug.Log("[ReadmeWarning] README file ensured and updated successfully");
                    EditorUtility.DisplayDialog("Success", "README file created/updated successfully!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to ensure README exists. Check console for details.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReadmeWarning] Failed to ensure README exists: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to ensure README exists: {ex.Message}", "OK");
            }
        }
    }
}