using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XRBubbleLibrary.Core.Editor
{
    /// <summary>
    /// Unity Editor window for feature validation system management and reporting.
    /// Provides visual interface for validation results and feature gate analysis.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class FeatureValidationEditor : EditorWindow
    {
        private Vector2 scrollPosition;
        private ValidationResults lastValidationResults;
        private string validationReport = "";
        private bool autoRefresh = true;
        private float lastRefreshTime;
        private const float AUTO_REFRESH_INTERVAL = 5f; // seconds

        private GUIStyle headerStyle;
        private GUIStyle successStyle;
        private GUIStyle warningStyle;
        private GUIStyle errorStyle;

        [MenuItem("XR Bubble Library/Feature Validation")]
        public static void ShowWindow()
        {
            var window = GetWindow<FeatureValidationEditor>("Feature Validation");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshValidationData();
        }

        private void Update()
        {
            if (autoRefresh && Time.realtimeSinceStartup - lastRefreshTime > AUTO_REFRESH_INTERVAL)
            {
                RefreshValidationData();
            }
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
                };
            }

            if (successStyle == null)
            {
                successStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = Color.green }
                };
            }

            if (warningStyle == null)
            {
                warningStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = Color.yellow }
                };
            }

            if (errorStyle == null)
            {
                errorStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = Color.red }
                };
            }
        }

        private void OnGUI()
        {
            InitializeStyles();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            EditorGUILayout.LabelField("XR Bubble Library - Feature Validation", headerStyle);
            EditorGUILayout.Space();

            // Control panel
            DrawControlPanel();
            EditorGUILayout.Space();

            // Validation status
            DrawValidationStatus();
            EditorGUILayout.Space();

            // Feature gate analysis
            DrawFeatureGateAnalysis();
            EditorGUILayout.Space();

            // Detailed validation report
            DrawDetailedReport();

            EditorGUILayout.EndScrollView();
        }

        private void DrawControlPanel()
        {
            EditorGUILayout.LabelField("Validation Control", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Run Full Validation", GUILayout.Width(150)))
            {
                RunFullValidation();
            }

            if (GUILayout.Button("Run Incremental Validation", GUILayout.Width(180)))
            {
                RunIncrementalValidation();
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                RefreshValidationData();
            }

            GUILayout.FlexibleSpace();

            autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh, GUILayout.Width(100));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Quick actions
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Report", GUILayout.Width(120)))
            {
                GenerateAndSaveReport();
            }

            if (GUILayout.Button("Validate All Features", GUILayout.Width(140)))
            {
                ValidateAllFeatures();
            }

            if (GUILayout.Button("Clear Results", GUILayout.Width(100)))
            {
                ClearValidationResults();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawValidationStatus()
        {
            EditorGUILayout.LabelField("Validation Status", EditorStyles.boldLabel);

            if (lastValidationResults == null)
            {
                EditorGUILayout.HelpBox("No validation results available. Run validation to see status.", MessageType.Info);
                return;
            }

            // Status overview
            var statusStyle = lastValidationResults.Success ? successStyle : errorStyle;
            var statusText = lastValidationResults.Success ? "✅ VALIDATION PASSED" : "❌ VALIDATION FAILED";
            
            EditorGUILayout.BeginVertical(statusStyle);
            EditorGUILayout.LabelField(statusText, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Validation Time: {lastValidationResults.ValidationTime:yyyy-MM-dd HH:mm:ss}");
            EditorGUILayout.LabelField($"Duration: {lastValidationResults.ValidationDuration:F3} seconds");
            EditorGUILayout.LabelField($"Type: {lastValidationResults.ValidationType}");
            
            if (lastValidationResults.Violations.Count > 0)
            {
                EditorGUILayout.LabelField($"Violations: {lastValidationResults.Violations.Count}");
            }
            
            if (!string.IsNullOrEmpty(lastValidationResults.ValidationError))
            {
                EditorGUILayout.LabelField($"Error: {lastValidationResults.ValidationError}");
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawFeatureGateAnalysis()
        {
            EditorGUILayout.LabelField("Feature Gate Analysis", EditorStyles.boldLabel);

            if (lastValidationResults == null || lastValidationResults.Violations.Count == 0)
            {
                EditorGUILayout.HelpBox("No violations detected. All feature gates are properly configured.", MessageType.Info);
                return;
            }

            // Group violations by feature
            var violationsByFeature = lastValidationResults.Violations.GroupBy(v => v.Feature);

            foreach (var group in violationsByFeature)
            {
                var feature = group.Key;
                var violations = group.ToList();
                var isEnabled = CompilerFlagManager.Instance.IsFeatureEnabled(feature);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                var statusIcon = isEnabled ? "✅" : "❌";
                EditorGUILayout.LabelField($"{statusIcon} {feature} ({violations.Count} violations)", EditorStyles.boldLabel);
                
                EditorGUI.indentLevel++;
                
                foreach (var violation in violations.Take(5)) // Show first 5 violations
                {
                    var violationIcon = violation.ViolationType switch
                    {
                        ViolationType.FeatureDisabled => "🚫",
                        ViolationType.DependencyMissing => "⚠️",
                        ViolationType.ValidationError => "❌",
                        _ => "❓"
                    };
                    
                    EditorGUILayout.LabelField($"{violationIcon} {violation.TargetName} ({violation.TargetType})");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(violation.ErrorMessage, EditorStyles.wordWrappedMiniLabel);
                    EditorGUI.indentLevel--;
                }
                
                if (violations.Count > 5)
                {
                    EditorGUILayout.LabelField($"... and {violations.Count - 5} more violations");
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }

        private void DrawDetailedReport()
        {
            EditorGUILayout.LabelField("Detailed Validation Report", EditorStyles.boldLabel);
            
            if (string.IsNullOrEmpty(validationReport))
            {
                EditorGUILayout.HelpBox("No detailed report available. Generate report to see full analysis.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            var reportScrollPosition = EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(200));
            EditorGUILayout.TextArea(validationReport, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView(reportScrollPosition);
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Copy to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = validationReport;
                Debug.Log("Validation report copied to clipboard");
            }
            
            if (GUILayout.Button("Save to File"))
            {
                SaveReportToFile();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void RunFullValidation()
        {
            Debug.Log("[FeatureValidation] Running full validation...");
            
            // Find or create validation system
            var validationSystem = FindValidationSystem();
            if (validationSystem != null)
            {
                lastValidationResults = validationSystem.PerformFullValidation();
                validationReport = validationSystem.GenerateValidationReport();
            }
            else
            {
                // Create temporary validation system for validation
                var tempGO = new GameObject("TempValidationSystem");
                var tempSystem = tempGO.AddComponent<FeatureValidationSystem>();
                
                lastValidationResults = tempSystem.PerformFullValidation();
                validationReport = tempSystem.GenerateValidationReport();
                
                DestroyImmediate(tempGO);
            }
            
            lastRefreshTime = Time.realtimeSinceStartup;
            Repaint();
        }

        private void RunIncrementalValidation()
        {
            Debug.Log("[FeatureValidation] Running incremental validation...");
            
            var validationSystem = FindValidationSystem();
            if (validationSystem != null)
            {
                lastValidationResults = validationSystem.PerformIncrementalValidation();
                if (lastValidationResults.Violations.Count > 0)
                {
                    validationReport = validationSystem.GenerateValidationReport();
                }
            }
            else
            {
                Debug.LogWarning("No FeatureValidationSystem found in scene. Running full validation instead.");
                RunFullValidation();
                return;
            }
            
            lastRefreshTime = Time.realtimeSinceStartup;
            Repaint();
        }

        private void ValidateAllFeatures()
        {
            Debug.Log("[FeatureValidation] Validating all individual features...");
            
            var validationSystem = FindValidationSystem();
            if (validationSystem == null)
            {
                var tempGO = new GameObject("TempValidationSystem");
                validationSystem = tempGO.AddComponent<FeatureValidationSystem>();
            }
            
            var allViolations = new System.Collections.Generic.List<FeatureGateViolation>();
            
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                var result = validationSystem.ValidateFeature(feature);
                allViolations.AddRange(result.Violations);
                
                Debug.Log($"[FeatureValidation] {feature}: {(result.Success ? "PASSED" : "FAILED")} " +
                         $"({result.ValidGates}/{result.TotalGates} gates valid)");
            }
            
            // Create summary results
            lastValidationResults = new ValidationResults
            {
                ValidationTime = DateTime.UtcNow,
                ValidationType = ValidationType.Full,
                Success = allViolations.Count == 0,
                Violations = allViolations,
                ValidationDuration = 0f // Individual validation times not tracked
            };
            
            validationReport = validationSystem.GenerateValidationReport();
            
            if (validationSystem.gameObject.name == "TempValidationSystem")
            {
                DestroyImmediate(validationSystem.gameObject);
            }
            
            lastRefreshTime = Time.realtimeSinceStartup;
            Repaint();
        }

        private void RefreshValidationData()
        {
            var validationSystem = FindValidationSystem();
            if (validationSystem != null)
            {
                lastValidationResults = validationSystem.GetLastValidationResults();
                if (lastValidationResults.ValidationTime > DateTime.MinValue)
                {
                    validationReport = validationSystem.GenerateValidationReport();
                }
            }
            
            lastRefreshTime = Time.realtimeSinceStartup;
            Repaint();
        }

        private void GenerateAndSaveReport()
        {
            if (lastValidationResults == null)
            {
                RunFullValidation();
            }
            
            if (!string.IsNullOrEmpty(validationReport))
            {
                SaveReportToFile();
            }
        }

        private void SaveReportToFile()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"FeatureValidationReport_{timestamp}.md";
            var path = EditorUtility.SaveFilePanel("Save Validation Report", "", filename, "md");
            
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, validationReport);
                Debug.Log($"Validation report saved to: {path}");
                
                // Refresh asset database if saved in project
                if (path.StartsWith(Application.dataPath))
                {
                    AssetDatabase.Refresh();
                }
            }
        }

        private void ClearValidationResults()
        {
            lastValidationResults = null;
            validationReport = "";
            Repaint();
        }

        private FeatureValidationSystem FindValidationSystem()
        {
            return FindObjectOfType<FeatureValidationSystem>();
        }
    }

    /// <summary>
    /// Custom property drawer for ValidationResults to show in inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(ValidationResults))]
    public class ValidationResultsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var validationTimeProperty = property.FindPropertyRelative("ValidationTime");
            var successProperty = property.FindPropertyRelative("Success");
            var violationsProperty = property.FindPropertyRelative("Violations");
            
            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            // Show validation status
            var statusText = successProperty.boolValue ? "✅ PASSED" : "❌ FAILED";
            EditorGUI.LabelField(rect, label.text, statusText);
            
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            
            // Show violation count if any
            if (violationsProperty.arraySize > 0)
            {
                EditorGUI.LabelField(rect, "Violations", violationsProperty.arraySize.ToString());
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var violationsProperty = property.FindPropertyRelative("Violations");
            var lines = violationsProperty.arraySize > 0 ? 2 : 1;
            return lines * (EditorGUIUtility.singleLineHeight + 2);
        }
    }
}