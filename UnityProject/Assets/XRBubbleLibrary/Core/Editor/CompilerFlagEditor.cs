using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XRBubbleLibrary.Core.Editor
{
    /// <summary>
    /// Unity Editor window for managing experimental feature compiler flags.
    /// Provides visual interface for enabling/disabling features and viewing dependencies.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class CompilerFlagEditor : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool showDependencies = true;
        private bool showDescription = true;
        private string statusReport = "";
        private GUIStyle headerStyle;
        private GUIStyle warningStyle;
        private GUIStyle successStyle;

        // Feature dependency mapping
        private readonly Dictionary<ExperimentalFeature, ExperimentalFeature[]> dependencies = 
            new Dictionary<ExperimentalFeature, ExperimentalFeature[]>
            {
                { ExperimentalFeature.CLOUD_INFERENCE, new[] { ExperimentalFeature.AI_INTEGRATION } },
                { ExperimentalFeature.ON_DEVICE_ML, new[] { ExperimentalFeature.AI_INTEGRATION } }
            };

        [MenuItem("XR Bubble Library/Experimental Features")]
        public static void ShowWindow()
        {
            var window = GetWindow<CompilerFlagEditor>("Experimental Features");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshStatusReport();
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
                };
            }

            if (warningStyle == null)
            {
                warningStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = Color.yellow }
                };
            }

            if (successStyle == null)
            {
                successStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = Color.green }
                };
            }
        }

        private void OnGUI()
        {
            InitializeStyles();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            EditorGUILayout.LabelField("XR Bubble Library - Experimental Features", headerStyle);
            EditorGUILayout.Space();

            // Warning about rebuild requirement
            EditorGUILayout.HelpBox(
                "Changing compiler flags requires a full project rebuild to take effect. " +
                "Runtime overrides are available for testing but don't affect compile-time exclusion.",
                MessageType.Info);

            EditorGUILayout.Space();

            // Feature configuration section
            DrawFeatureConfiguration();

            EditorGUILayout.Space();

            // Dependencies section
            if (showDependencies)
            {
                DrawDependencyVisualization();
                EditorGUILayout.Space();
            }

            // Status report section
            DrawStatusReport();

            // Control buttons
            DrawControlButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawFeatureConfiguration()
        {
            EditorGUILayout.LabelField("Feature Configuration", EditorStyles.boldLabel);

            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                DrawFeatureControl(feature);
            }
        }

        private void DrawFeatureControl(ExperimentalFeature feature)
        {
            EditorGUILayout.BeginHorizontal();

            // Feature name and status
            var compileTimeEnabled = GetCompileTimeState(feature);
            var runtimeEnabled = CompilerFlagManager.Instance.IsFeatureEnabled(feature);
            
            var statusIcon = compileTimeEnabled ? "✅" : "❌";
            var statusText = $"{statusIcon} {feature}";
            
            if (compileTimeEnabled != runtimeEnabled)
            {
                statusText += " (runtime override)";
            }

            EditorGUILayout.LabelField(statusText, GUILayout.Width(250));

            // Compile-time toggle
            EditorGUI.BeginChangeCheck();
            var newCompileTimeState = EditorGUILayout.Toggle("Compile-time", compileTimeEnabled, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
            {
                SetCompileTimeFlag(feature, newCompileTimeState);
            }

            // Runtime toggle (only if compile-time enabled)
            EditorGUI.BeginDisabledGroup(!compileTimeEnabled);
            EditorGUI.BeginChangeCheck();
            var newRuntimeState = EditorGUILayout.Toggle("Runtime", runtimeEnabled, GUILayout.Width(80));
            if (EditorGUI.EndChangeCheck())
            {
                CompilerFlagManager.Instance.SetFeatureState(feature, newRuntimeState);
                RefreshStatusReport();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            // Feature description
            if (showDescription)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(GetFeatureDescription(feature), EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }

            // Dependency warnings
            if (dependencies.ContainsKey(feature) && runtimeEnabled)
            {
                var missingDeps = dependencies[feature].Where(dep => 
                    !CompilerFlagManager.Instance.IsFeatureEnabled(dep)).ToArray();
                
                if (missingDeps.Length > 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox(
                        $"Warning: {feature} requires {string.Join(", ", missingDeps)} to be enabled",
                        MessageType.Warning);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space(5);
        }

        private void DrawDependencyVisualization()
        {
            showDependencies = EditorGUILayout.Foldout(showDependencies, "Feature Dependencies", true);
            
            if (showDependencies)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.HelpBox(
                    "Dependency relationships between experimental features:", 
                    MessageType.Info);

                foreach (var kvp in dependencies)
                {
                    var dependent = kvp.Key;
                    var deps = kvp.Value;
                    
                    var dependentEnabled = CompilerFlagManager.Instance.IsFeatureEnabled(dependent);
                    var allDepsEnabled = deps.All(dep => CompilerFlagManager.Instance.IsFeatureEnabled(dep));
                    
                    var statusIcon = dependentEnabled && allDepsEnabled ? "✅" : 
                                   dependentEnabled && !allDepsEnabled ? "⚠️" : "❌";
                    
                    EditorGUILayout.LabelField(
                        $"{statusIcon} {dependent} → {string.Join(", ", deps)}", 
                        EditorStyles.miniLabel);
                }
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawStatusReport()
        {
            EditorGUILayout.LabelField("Status Report", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.TextArea(statusReport, EditorStyles.wordWrappedLabel, GUILayout.Height(150));
            EditorGUILayout.EndVertical();
        }

        private void DrawControlButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh Status"))
            {
                RefreshStatusReport();
            }

            if (GUILayout.Button("Reset Runtime Overrides"))
            {
                CompilerFlagManager.Instance.ResetToDefaults();
                RefreshStatusReport();
            }

            if (GUILayout.Button("Validate Configuration"))
            {
                CompilerFlagManager.Instance.ValidateConfiguration();
                RefreshStatusReport();
            }

            showDescription = GUILayout.Toggle(showDescription, "Show Descriptions");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Rebuild warning
            if (HasPendingChanges())
            {
                EditorGUILayout.HelpBox(
                    "Compiler flag changes detected. Rebuild the project to apply changes.",
                    MessageType.Warning);
                
                if (GUILayout.Button("Force Rebuild"))
                {
                    ForceProjectRebuild();
                }
            }
        }

        private bool GetCompileTimeState(ExperimentalFeature feature)
        {
            return feature switch
            {
                ExperimentalFeature.AI_INTEGRATION => HasDefineSymbol("EXP_AI"),
                ExperimentalFeature.VOICE_PROCESSING => HasDefineSymbol("EXP_VOICE"),
                ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS => HasDefineSymbol("EXP_ADVANCED_WAVE"),
                ExperimentalFeature.CLOUD_INFERENCE => HasDefineSymbol("EXP_CLOUD_INFERENCE"),
                ExperimentalFeature.ON_DEVICE_ML => HasDefineSymbol("EXP_ON_DEVICE_ML"),
                _ => false
            };
        }

        private void SetCompileTimeFlag(ExperimentalFeature feature, bool enabled)
        {
            var symbol = feature switch
            {
                ExperimentalFeature.AI_INTEGRATION => "EXP_AI",
                ExperimentalFeature.VOICE_PROCESSING => "EXP_VOICE",
                ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS => "EXP_ADVANCED_WAVE",
                ExperimentalFeature.CLOUD_INFERENCE => "EXP_CLOUD_INFERENCE",
                ExperimentalFeature.ON_DEVICE_ML => "EXP_ON_DEVICE_ML",
                _ => null
            };

            if (symbol != null)
            {
                if (enabled)
                {
                    AddDefineSymbol(symbol);
                }
                else
                {
                    RemoveDefineSymbol(symbol);
                }
            }
        }

        private bool HasDefineSymbol(string symbol)
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            return defines.Contains(symbol);
        }

        private void AddDefineSymbol(string symbol)
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            
            if (!defines.Contains(symbol))
            {
                if (!string.IsNullOrEmpty(defines))
                {
                    defines += ";";
                }
                defines += symbol;
                
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines);
                Debug.Log($"Added compiler flag: {symbol}");
            }
        }

        private void RemoveDefineSymbol(string symbol)
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            
            if (defines.Contains(symbol))
            {
                var symbolList = defines.Split(';').ToList();
                symbolList.Remove(symbol);
                defines = string.Join(";", symbolList);
                
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines);
                Debug.Log($"Removed compiler flag: {symbol}");
            }
        }

        private void RefreshStatusReport()
        {
            statusReport = CompilerFlagManager.Instance.GenerateStatusReport();
        }

        private string GetFeatureDescription(ExperimentalFeature feature)
        {
            return feature switch
            {
                ExperimentalFeature.AI_INTEGRATION => "AI-enhanced bubble behavior and interaction systems",
                ExperimentalFeature.VOICE_PROCESSING => "Voice recognition and speech-to-text capabilities",
                ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS => "Experimental wave mathematics and advanced algorithms",
                ExperimentalFeature.CLOUD_INFERENCE => "Cloud-based AI inference for enhanced processing",
                ExperimentalFeature.ON_DEVICE_ML => "On-device machine learning and neural network processing",
                _ => "Unknown experimental feature"
            };
        }

        private bool HasPendingChanges()
        {
            // Check if any compile-time flags differ from current state
            foreach (ExperimentalFeature feature in Enum.GetValues<ExperimentalFeature>())
            {
                var compileTime = GetCompileTimeState(feature);
                var expected = CompilerFlags.IsFeatureEnabled(feature);
                
                if (compileTime != expected)
                {
                    return true;
                }
            }
            
            return false;
        }

        private void ForceProjectRebuild()
        {
            Debug.Log("Forcing project rebuild to apply compiler flag changes...");
            
            // Clear script assemblies to force rebuild
            var method = typeof(EditorUtility).GetMethod("RequestScriptReload", 
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            method?.Invoke(null, null);
        }
    }
}