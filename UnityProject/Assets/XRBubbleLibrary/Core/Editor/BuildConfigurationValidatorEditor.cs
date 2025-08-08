using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace XRBubbleLibrary.Core.Editor
{
    /// <summary>
    /// Editor integration for BuildConfigurationValidator.
    /// Provides pre-build validation hooks and editor UI for configuration validation.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class BuildConfigurationValidatorEditor : IPreprocessBuildWithReport
    {
        /// <summary>
        /// Order for build preprocessing. Lower numbers execute first.
        /// </summary>
        public int callbackOrder => 0;
        
        /// <summary>
        /// Pre-build validation hook that runs before every build.
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("🔍 Running pre-build configuration validation...");
            
            var result = BuildConfigurationValidator.ValidateConfiguration();
            
            if (!result.IsValid)
            {
                var errorCount = result.Issues.Count(i => i.Severity == BuildConfigurationValidator.ValidationSeverity.Error);
                var warningCount = result.Issues.Count(i => i.Severity == BuildConfigurationValidator.ValidationSeverity.Warning);
                
                Debug.LogError($"❌ Build configuration validation failed with {errorCount} error(s) and {warningCount} warning(s)");
                Debug.LogError("Build will be cancelled due to configuration errors.");
                Debug.LogError($"Detailed report:\n{result.GenerateReport()}");
                
                // Cancel the build by throwing an exception
                throw new BuildFailedException($"Build configuration validation failed with {errorCount} error(s). " +
                                             "Please fix configuration issues before building.");
            }
            else
            {
                var warningCount = result.Issues.Count(i => i.Severity == BuildConfigurationValidator.ValidationSeverity.Warning);
                if (warningCount > 0)
                {
                    Debug.LogWarning($"⚠️ Build configuration validation passed with {warningCount} warning(s)");
                    Debug.LogWarning($"Warnings:\n{result.GenerateReport()}");
                }
                else
                {
                    Debug.Log("✅ Build configuration validation passed - no issues detected");
                }
            }
        }
        
        /// <summary>
        /// Menu item for manual configuration validation.
        /// </summary>
        [MenuItem("XR Bubble Library/Validate Build Configuration", priority = 100)]
        public static void ValidateConfigurationManually()
        {
            Debug.Log("🔍 Running manual build configuration validation...");
            BuildConfigurationValidator.ValidateAndLog();
        }
        
        /// <summary>
        /// Menu item to generate and display detailed validation report.
        /// </summary>
        [MenuItem("XR Bubble Library/Generate Configuration Report", priority = 101)]
        public static void GenerateConfigurationReport()
        {
            var result = BuildConfigurationValidator.ValidateConfiguration();
            var report = result.GenerateReport();
            
            // Create a temporary file to display the report
            var tempPath = System.IO.Path.GetTempFileName() + ".md";
            System.IO.File.WriteAllText(tempPath, report);
            
            // Open the report in the default markdown viewer
            System.Diagnostics.Process.Start(tempPath);
            
            Debug.Log($"📄 Configuration report generated and opened: {tempPath}");
            Debug.Log($"Report summary: {(result.IsValid ? "✅ VALID" : "❌ INVALID")} " +
                     $"({result.Issues.Count} issue(s) found)");
        }
        
        /// <summary>
        /// Menu item to display validation rules.
        /// </summary>
        [MenuItem("XR Bubble Library/Show Validation Rules", priority = 102)]
        public static void ShowValidationRules()
        {
            var rules = BuildConfigurationValidator.GetValidationRules();
            var rulesText = "# Build Configuration Validation Rules\n\n";
            
            for (int i = 0; i < rules.Count; i++)
            {
                rulesText += $"{i + 1}. {rules[i]}\n";
            }
            
            Debug.Log($"📋 Build Configuration Validation Rules:\n{rulesText}");
            
            // Also create a temporary file for the rules
            var tempPath = System.IO.Path.GetTempFileName() + ".md";
            System.IO.File.WriteAllText(tempPath, rulesText);
            System.Diagnostics.Process.Start(tempPath);
        }
        
        /// <summary>
        /// Custom editor window for configuration validation.
        /// </summary>
        public class ConfigurationValidatorWindow : EditorWindow
        {
            private BuildConfigurationValidator.ValidationResult _lastResult;
            private Vector2 _scrollPosition;
            private bool _autoRefresh = true;
            private double _lastRefreshTime;
            
            [MenuItem("XR Bubble Library/Configuration Validator Window", priority = 200)]
            public static void ShowWindow()
            {
                var window = GetWindow<ConfigurationValidatorWindow>("Config Validator");
                window.minSize = new Vector2(400, 300);
                window.Show();
            }
            
            private void OnEnable()
            {
                RefreshValidation();
            }
            
            private void OnGUI()
            {
                EditorGUILayout.BeginVertical();
                
                // Header
                EditorGUILayout.LabelField("Build Configuration Validator", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                
                // Controls
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                {
                    RefreshValidation();
                }
                
                _autoRefresh = EditorGUILayout.Toggle("Auto Refresh", _autoRefresh);
                
                if (GUILayout.Button("Generate Report", GUILayout.Width(120)))
                {
                    GenerateConfigurationReport();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // Status
                if (_lastResult != null)
                {
                    var statusColor = _lastResult.IsValid ? Color.green : Color.red;
                    var statusText = _lastResult.IsValid ? "✅ VALID" : "❌ INVALID";
                    
                    var oldColor = GUI.color;
                    GUI.color = statusColor;
                    EditorGUILayout.LabelField($"Status: {statusText}", EditorStyles.boldLabel);
                    GUI.color = oldColor;
                    
                    EditorGUILayout.LabelField($"Last Updated: {_lastResult.ValidatedAt:HH:mm:ss}");
                    EditorGUILayout.LabelField($"Build Config: {_lastResult.BuildConfiguration}");
                    EditorGUILayout.LabelField($"Issues Found: {_lastResult.Issues.Count}");
                    
                    EditorGUILayout.Space();
                    
                    // Issues list
                    if (_lastResult.Issues.Count > 0)
                    {
                        EditorGUILayout.LabelField("Issues:", EditorStyles.boldLabel);
                        
                        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                        
                        foreach (var issue in _lastResult.Issues)
                        {
                            var severityColor = issue.Severity switch
                            {
                                BuildConfigurationValidator.ValidationSeverity.Error => Color.red,
                                BuildConfigurationValidator.ValidationSeverity.Warning => Color.yellow,
                                BuildConfigurationValidator.ValidationSeverity.Info => Color.cyan,
                                _ => Color.white
                            };
                            
                            var oldColor = GUI.color;
                            GUI.color = severityColor;
                            EditorGUILayout.LabelField($"[{issue.Severity}] {issue.Title}", EditorStyles.boldLabel);
                            GUI.color = oldColor;
                            
                            EditorGUILayout.LabelField(issue.Description, EditorStyles.wordWrappedLabel);
                            
                            if (!string.IsNullOrEmpty(issue.Recommendation))
                            {
                                EditorGUILayout.LabelField($"💡 {issue.Recommendation}", EditorStyles.wordWrappedMiniLabel);
                            }
                            
                            EditorGUILayout.Space();
                        }
                        
                        EditorGUILayout.EndScrollView();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("✅ No issues detected", EditorStyles.centeredGreyMiniLabel);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Click Refresh to validate configuration", EditorStyles.centeredGreyMiniLabel);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            private void Update()
            {
                if (_autoRefresh && EditorApplication.timeSinceStartup - _lastRefreshTime > 5.0)
                {
                    RefreshValidation();
                }
            }
            
            private void RefreshValidation()
            {
                _lastResult = BuildConfigurationValidator.ValidateConfiguration();
                _lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }
    }
}