using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;

namespace XRBubbleLibrary.Core.Editor
{
    /// <summary>
    /// Editor integration for the Development State Generator.
    /// Provides menu items and editor tools for generating development state reports.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public static class DevStateGeneratorEditor
    {
        /// <summary>
        /// Menu item to generate development state report manually.
        /// </summary>
        [MenuItem("XR Bubble Library/Generate Development State Report", priority = 200)]
        public static void GenerateDevStateReport()
        {
            UnityEngine.Debug.Log("🔍 Generating Development State Report...");
            
            try
            {
                var generator = DevStateGenerator.Instance;
                var filePath = generator.GenerateAndSaveDevStateFile();
                
                UnityEngine.Debug.Log($"✅ Development State Report generated successfully: {filePath}");
                
                // Ask if user wants to open the file
                if (EditorUtility.DisplayDialog("Development State Report Generated", 
                    $"Report saved to: {filePath}\n\nWould you like to open it?", 
                    "Open", "Close"))
                {
                    OpenDevStateFile(filePath);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Failed to generate Development State Report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to generate Development State Report:\n{ex.Message}", 
                    "OK");
            }
        }
        
        /// <summary>
        /// Menu item to validate existing development state report.
        /// </summary>
        [MenuItem("XR Bubble Library/Validate Development State Report", priority = 201)]
        public static void ValidateDevStateReport()
        {
            UnityEngine.Debug.Log("🔍 Validating Development State Report...");
            
            try
            {
                var generator = DevStateGenerator.Instance;
                var report = generator.GenerateReport();
                var isValid = generator.ValidateReportAccuracy(report);
                
                if (isValid)
                {
                    UnityEngine.Debug.Log("✅ Development State Report validation passed");
                    EditorUtility.DisplayDialog("Validation Successful", 
                        "Development State Report validation passed successfully.", 
                        "OK");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("⚠️ Development State Report validation failed");
                    EditorUtility.DisplayDialog("Validation Failed", 
                        "Development State Report validation failed. Check the console for details.", 
                        "OK");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Failed to validate Development State Report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to validate Development State Report:\n{ex.Message}", 
                    "OK");
            }
        }
        
        /// <summary>
        /// Menu item to open existing DEV_STATE.md file.
        /// </summary>
        [MenuItem("XR Bubble Library/Open Development State Report", priority = 202)]
        public static void OpenDevStateReport()
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var devStateFile = Path.Combine(projectRoot, "DEV_STATE.md");
            
            if (File.Exists(devStateFile))
            {
                OpenDevStateFile(devStateFile);
            }
            else
            {
                if (EditorUtility.DisplayDialog("File Not Found", 
                    "DEV_STATE.md file not found. Would you like to generate it?", 
                    "Generate", "Cancel"))
                {
                    GenerateDevStateReport();
                }
            }
        }
        
        /// <summary>
        /// Menu item to show development state generator window.
        /// </summary>
        [MenuItem("XR Bubble Library/Development State Generator Window", priority = 250)]
        public static void ShowDevStateGeneratorWindow()
        {
            DevStateGeneratorWindow.ShowWindow();
        }
        
        /// <summary>
        /// Open the DEV_STATE.md file in the default markdown viewer.
        /// </summary>
        private static void OpenDevStateFile(string filePath)
        {
            try
            {
                Process.Start(filePath);
                UnityEngine.Debug.Log($"📄 Opened Development State Report: {filePath}");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to open file automatically: {ex.Message}");
                EditorUtility.DisplayDialog("File Generated", 
                    $"Development State Report saved to:\n{filePath}\n\nPlease open it manually.", 
                    "OK");
            }
        }
        
        /// <summary>
        /// Custom editor window for development state generation and monitoring.
        /// </summary>
        public class DevStateGeneratorWindow : EditorWindow
        {
            private DevStateReport _lastReport;
            private Vector2 _scrollPosition;
            private bool _autoRefresh = false;
            private double _lastRefreshTime;
            private string _lastGeneratedFile;
            
            [MenuItem("XR Bubble Library/Development State Generator Window", priority = 250)]
            public static void ShowWindow()
            {
                var window = GetWindow<DevStateGeneratorWindow>("Dev State Generator");
                window.minSize = new Vector2(500, 400);
                window.Show();
            }
            
            private void OnEnable()
            {
                RefreshReport();
            }
            
            private void OnGUI()
            {
                EditorGUILayout.BeginVertical();
                
                // Header
                EditorGUILayout.LabelField("Development State Generator", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                
                // Controls
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate Report", GUILayout.Width(120)))
                {
                    GenerateAndSaveReport();
                }
                
                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                {
                    RefreshReport();
                }
                
                _autoRefresh = EditorGUILayout.Toggle("Auto Refresh", _autoRefresh);
                
                if (GUILayout.Button("Open File", GUILayout.Width(80)) && !string.IsNullOrEmpty(_lastGeneratedFile))
                {
                    OpenDevStateFile(_lastGeneratedFile);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // Report information
                if (_lastReport != null)
                {
                    EditorGUILayout.LabelField($"Last Generated: {_lastReport.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
                    EditorGUILayout.LabelField($"Build Version: {_lastReport.BuildVersion}");
                    EditorGUILayout.LabelField($"Unity Version: {_lastReport.UnityVersion}");
                    EditorGUILayout.LabelField($"Build Config: {_lastReport.BuildConfiguration}");
                    
                    EditorGUILayout.Space();
                    
                    // Summary
                    if (_lastReport.Summary != null)
                    {
                        EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"Total Modules: {_lastReport.Summary.TotalModules}");
                        EditorGUILayout.LabelField($"Implemented: {_lastReport.Summary.ImplementedModules} ({_lastReport.Summary.ImplementedPercentage:F1}%)");
                        EditorGUILayout.LabelField($"Disabled: {_lastReport.Summary.DisabledModules} ({_lastReport.Summary.DisabledPercentage:F1}%)");
                        EditorGUILayout.LabelField($"Conceptual: {_lastReport.Summary.ConceptualModules} ({_lastReport.Summary.ConceptualPercentage:F1}%)");
                        
                        EditorGUILayout.Space();
                    }
                    
                    // Compiler flags
                    EditorGUILayout.LabelField("Compiler Flags", EditorStyles.boldLabel);
                    foreach (var flag in _lastReport.CompilerFlags)
                    {
                        var status = flag.Value ? "✅ ENABLED" : "❌ DISABLED";
                        EditorGUILayout.LabelField($"{flag.Key}: {status}");
                    }
                    
                    EditorGUILayout.Space();
                    
                    // Modules list
                    EditorGUILayout.LabelField("Modules", EditorStyles.boldLabel);
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                    
                    foreach (var module in _lastReport.Modules)
                    {
                        var statusIcon = module.State switch
                        {
                            ModuleState.Implemented => "✅",
                            ModuleState.Disabled => "❌",
                            ModuleState.Conceptual => "🔬",
                            _ => "❓"
                        };
                        
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{statusIcon} {module.ModuleName}", GUILayout.Width(200));
                        EditorGUILayout.LabelField(module.State.ToString(), GUILayout.Width(100));
                        EditorGUILayout.LabelField(module.AssemblyName, GUILayout.ExpandWidth(true));
                        EditorGUILayout.EndHorizontal();
                        
                        if (!string.IsNullOrEmpty(module.ValidationNotes))
                        {
                            EditorGUILayout.LabelField($"  {module.ValidationNotes}", EditorStyles.miniLabel);
                        }
                        
                        EditorGUILayout.Space(2);
                    }
                    
                    EditorGUILayout.EndScrollView();
                    
                    // Performance metrics
                    if (_lastReport.CurrentPerformance != null)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Performance Metrics", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"Average FPS: {_lastReport.CurrentPerformance.AverageFPS:F1}");
                        EditorGUILayout.LabelField($"Frame Time: {_lastReport.CurrentPerformance.AverageFrameTime:F2}ms");
                        EditorGUILayout.LabelField($"Memory: {_lastReport.CurrentPerformance.MemoryUsage / (1024 * 1024):F1} MB");
                    }
                    
                    // Evidence files
                    if (_lastReport.SupportingEvidence != null && _lastReport.SupportingEvidence.Count > 0)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField($"Evidence Files: {_lastReport.SupportingEvidence.Count}");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Click 'Generate Report' to create a development state report", EditorStyles.centeredGreyMiniLabel);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            private void Update()
            {
                if (_autoRefresh && EditorApplication.timeSinceStartup - _lastRefreshTime > 10.0)
                {
                    RefreshReport();
                }
            }
            
            private void RefreshReport()
            {
                try
                {
                    var generator = DevStateGenerator.Instance;
                    _lastReport = generator.GenerateReport();
                    _lastRefreshTime = EditorApplication.timeSinceStartup;
                    Repaint();
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to refresh development state report: {ex.Message}");
                }
            }
            
            private void GenerateAndSaveReport()
            {
                try
                {
                    var generator = DevStateGenerator.Instance;
                    _lastGeneratedFile = generator.GenerateAndSaveDevStateFile();
                    RefreshReport();
                    
                    UnityEngine.Debug.Log($"Development State Report saved to: {_lastGeneratedFile}");
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to generate and save report: {ex.Message}");
                    EditorUtility.DisplayDialog("Error", $"Failed to generate report:\n{ex.Message}", "OK");
                }
            }
        }
    }
}