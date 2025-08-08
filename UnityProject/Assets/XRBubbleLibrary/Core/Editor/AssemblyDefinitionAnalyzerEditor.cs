using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace XRBubbleLibrary.Core.Editor
{
    /// <summary>
    /// Editor integration for the enhanced Assembly Definition Analyzer.
    /// Provides menu items and interactive tools for comprehensive assembly analysis.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public static class AssemblyDefinitionAnalyzerEditor
    {
        /// <summary>
        /// Menu item to perform comprehensive assembly definition analysis.
        /// </summary>
        [MenuItem("XR Bubble Library/Analyze Assembly Definitions", priority = 300)]
        public static void AnalyzeAssemblyDefinitions()
        {
            UnityEngine.Debug.Log("🔍 Starting comprehensive assembly definition analysis...");
            
            try
            {
                var analyzer = new AssemblyDefinitionAnalyzerService();
                var assemblyPaths = GetAssemblyDefinitionPaths();
                
                if (assemblyPaths.Count == 0)
                {
                    EditorUtility.DisplayDialog("No Assembly Definitions Found", 
                        "No assembly definition files were found in the XRBubbleLibrary directory.", 
                        "OK");
                    return;
                }
                
                var result = analyzer.AnalyzeAssemblyDefinitions(assemblyPaths);
                
                UnityEngine.Debug.Log($"✅ Analysis completed: {result.Assemblies.Count} assemblies, " +
                                     $"{result.Dependencies.Count} dependencies, " +
                                     $"{result.CircularDependencies.Count} circular dependencies, " +
                                     $"{result.ValidationIssues.Count} validation issues");
                
                // Generate and save report
                var report = analyzer.GenerateAnalysisReport(result);
                var reportPath = SaveAnalysisReport(report);
                
                // Show summary dialog
                var summary = $"Analysis Results:\n\n" +
                             $"• Assemblies: {result.Assemblies.Count}\n" +
                             $"• Dependencies: {result.Dependencies.Count}\n" +
                             $"• Circular Dependencies: {result.CircularDependencies.Count}\n" +
                             $"• Validation Issues: {result.ValidationIssues.Count}\n\n" +
                             $"Report saved to: {reportPath}\n\n" +
                             "Would you like to open the detailed report?";
                
                if (EditorUtility.DisplayDialog("Assembly Analysis Complete", summary, "Open Report", "Close"))
                {
                    OpenAnalysisReport(reportPath);
                }
                
                // Log issues if any
                LogValidationIssues(result.ValidationIssues);
                LogCircularDependencies(result.CircularDependencies);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Assembly definition analysis failed: {ex.Message}");
                EditorUtility.DisplayDialog("Analysis Failed", 
                    $"Assembly definition analysis failed:\n{ex.Message}", 
                    "OK");
            }
        }
        
        /// <summary>
        /// Menu item to detect circular dependencies specifically.
        /// </summary>
        [MenuItem("XR Bubble Library/Detect Circular Dependencies", priority = 301)]
        public static void DetectCircularDependencies()
        {
            UnityEngine.Debug.Log("🔍 Detecting circular dependencies...");
            
            try
            {
                var analyzer = new AssemblyDefinitionAnalyzerService();
                var assemblyPaths = GetAssemblyDefinitionPaths();
                var result = analyzer.AnalyzeAssemblyDefinitions(assemblyPaths);
                
                if (result.CircularDependencies.Count == 0)
                {
                    UnityEngine.Debug.Log("✅ No circular dependencies detected");
                    EditorUtility.DisplayDialog("Circular Dependencies Check", 
                        "No circular dependencies were detected in the assembly definitions.", 
                        "OK");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"⚠️ Found {result.CircularDependencies.Count} circular dependencies");
                    
                    var message = $"Found {result.CircularDependencies.Count} circular dependencies:\n\n";
                    foreach (var circular in result.CircularDependencies.Take(3)) // Show first 3
                    {
                        message += $"• {circular.Severity}: {string.Join(" → ", circular.AssemblyChain)}\n";
                    }
                    
                    if (result.CircularDependencies.Count > 3)
                    {
                        message += $"... and {result.CircularDependencies.Count - 3} more\n";
                    }
                    
                    message += "\nWould you like to see the full analysis report?";
                    
                    if (EditorUtility.DisplayDialog("Circular Dependencies Found", message, "Show Report", "Close"))
                    {
                        AnalyzeAssemblyDefinitions();
                    }
                    
                    LogCircularDependencies(result.CircularDependencies);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Circular dependency detection failed: {ex.Message}");
                EditorUtility.DisplayDialog("Detection Failed", 
                    $"Circular dependency detection failed:\n{ex.Message}", 
                    "OK");
            }
        }
        
        /// <summary>
        /// Menu item to validate assembly configuration.
        /// </summary>
        [MenuItem("XR Bubble Library/Validate Assembly Configuration", priority = 302)]
        public static void ValidateAssemblyConfiguration()
        {
            UnityEngine.Debug.Log("🔍 Validating assembly configuration...");
            
            try
            {
                var analyzer = new AssemblyDefinitionAnalyzerService();
                var assemblyPaths = GetAssemblyDefinitionPaths();
                var result = analyzer.AnalyzeAssemblyDefinitions(assemblyPaths);
                
                var criticalIssues = result.ValidationIssues.Count(i => i.Severity == AssemblyDefinitionAnalyzer.ValidationSeverity.Critical);
                var errorIssues = result.ValidationIssues.Count(i => i.Severity == AssemblyDefinitionAnalyzer.ValidationSeverity.Error);
                var warningIssues = result.ValidationIssues.Count(i => i.Severity == AssemblyDefinitionAnalyzer.ValidationSeverity.Warning);
                var infoIssues = result.ValidationIssues.Count(i => i.Severity == AssemblyDefinitionAnalyzer.ValidationSeverity.Info);
                
                var message = $"Assembly Configuration Validation Results:\n\n" +
                             $"🔴 Critical Issues: {criticalIssues}\n" +
                             $"❌ Error Issues: {errorIssues}\n" +
                             $"⚠️ Warning Issues: {warningIssues}\n" +
                             $"ℹ️ Info Issues: {infoIssues}\n\n";
                
                if (result.ValidationIssues.Count == 0)
                {
                    message += "✅ No configuration issues detected!";
                    UnityEngine.Debug.Log("✅ Assembly configuration validation passed");
                }
                else
                {
                    message += "See console for detailed issue descriptions.";
                    UnityEngine.Debug.LogWarning($"⚠️ Found {result.ValidationIssues.Count} configuration issues");
                }
                
                EditorUtility.DisplayDialog("Configuration Validation", message, "OK");
                LogValidationIssues(result.ValidationIssues);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Assembly configuration validation failed: {ex.Message}");
                EditorUtility.DisplayDialog("Validation Failed", 
                    $"Assembly configuration validation failed:\n{ex.Message}", 
                    "OK");
            }
        }
        
        /// <summary>
        /// Menu item to show assembly definition analyzer window.
        /// </summary>
        [MenuItem("XR Bubble Library/Assembly Definition Analyzer Window", priority = 350)]
        public static void ShowAssemblyAnalyzerWindow()
        {
            AssemblyDefinitionAnalyzerWindow.ShowWindow();
        }
        
        /// <summary>
        /// Get all assembly definition file paths in the project.
        /// </summary>
        private static System.Collections.Generic.List<string> GetAssemblyDefinitionPaths()
        {
            var paths = new System.Collections.Generic.List<string>();
            
            var xrBubbleLibraryPath = Path.Combine(Application.dataPath, "XRBubbleLibrary");
            if (Directory.Exists(xrBubbleLibraryPath))
            {
                var asmdefFiles = Directory.GetFiles(xrBubbleLibraryPath, "*.asmdef", SearchOption.AllDirectories);
                paths.AddRange(asmdefFiles);
            }
            
            return paths;
        }
        
        /// <summary>
        /// Save the analysis report to a file.
        /// </summary>
        private static string SaveAnalysisReport(string report)
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var reportPath = Path.Combine(projectRoot, "ASSEMBLY_ANALYSIS_REPORT.md");
            
            File.WriteAllText(reportPath, report);
            return reportPath;
        }
        
        /// <summary>
        /// Open the analysis report in the default markdown viewer.
        /// </summary>
        private static void OpenAnalysisReport(string reportPath)
        {
            try
            {
                Process.Start(reportPath);
                UnityEngine.Debug.Log($"📄 Opened analysis report: {reportPath}");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to open report automatically: {ex.Message}");
                EditorUtility.DisplayDialog("Report Generated", 
                    $"Analysis report saved to:\n{reportPath}\n\nPlease open it manually.", 
                    "OK");
            }
        }
        
        /// <summary>
        /// Log validation issues to the console with appropriate severity.
        /// </summary>
        private static void LogValidationIssues(System.Collections.Generic.List<AssemblyDefinitionAnalyzer.ValidationIssue> issues)
        {
            foreach (var issue in issues)
            {
                var message = $"[{issue.AssemblyName}] {issue.Title}: {issue.Description}";
                if (!string.IsNullOrEmpty(issue.Recommendation))
                {
                    message += $" Recommendation: {issue.Recommendation}";
                }
                
                switch (issue.Severity)
                {
                    case AssemblyDefinitionAnalyzer.ValidationSeverity.Critical:
                        UnityEngine.Debug.LogError($"🔴 CRITICAL: {message}");
                        break;
                    case AssemblyDefinitionAnalyzer.ValidationSeverity.Error:
                        UnityEngine.Debug.LogError($"❌ ERROR: {message}");
                        break;
                    case AssemblyDefinitionAnalyzer.ValidationSeverity.Warning:
                        UnityEngine.Debug.LogWarning($"⚠️ WARNING: {message}");
                        break;
                    case AssemblyDefinitionAnalyzer.ValidationSeverity.Info:
                        UnityEngine.Debug.Log($"ℹ️ INFO: {message}");
                        break;
                }
            }
        }
        
        /// <summary>
        /// Log circular dependencies to the console.
        /// </summary>
        private static void LogCircularDependencies(System.Collections.Generic.List<AssemblyDefinitionAnalyzer.CircularDependency> circularDependencies)
        {
            foreach (var circular in circularDependencies)
            {
                var severityIcon = circular.Severity switch
                {
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.Critical => "🔴",
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.High => "🟠",
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.Medium => "🟡",
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.Low => "🟢",
                    _ => "⚪"
                };
                
                var message = $"{severityIcon} {circular.Severity} Circular Dependency: {circular.Description}";
                
                if (circular.Severity == AssemblyDefinitionAnalyzer.CircularDependencySeverity.Critical ||
                    circular.Severity == AssemblyDefinitionAnalyzer.CircularDependencySeverity.High)
                {
                    UnityEngine.Debug.LogError(message);
                }
                else
                {
                    UnityEngine.Debug.LogWarning(message);
                }
            }
        }
    }
}    
/// <summary>
    /// Interactive editor window for assembly definition analysis.
    /// </summary>
    public class AssemblyDefinitionAnalyzerWindow : EditorWindow
    {
        private AssemblyDefinitionAnalyzer.AnalysisResult _lastResult;
        private Vector2 _scrollPosition;
        private bool _autoRefresh = false;
        private double _lastRefreshTime;
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Overview", "Dependencies", "Circular Deps", "Validation", "Details" };
        
        [MenuItem("XR Bubble Library/Assembly Definition Analyzer Window", priority = 350)]
        public static void ShowWindow()
        {
            var window = GetWindow<AssemblyDefinitionAnalyzerWindow>("Assembly Analyzer");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }
        
        private void OnEnable()
        {
            RefreshAnalysis();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            EditorGUILayout.LabelField("Assembly Definition Analyzer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Controls
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Analyze", GUILayout.Width(80)))
            {
                RefreshAnalysis();
            }
            
            if (GUILayout.Button("Save Report", GUILayout.Width(100)))
            {
                SaveReport();
            }
            
            _autoRefresh = EditorGUILayout.Toggle("Auto Refresh", _autoRefresh);
            
            if (GUILayout.Button("Export JSON", GUILayout.Width(100)))
            {
                ExportJson();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            if (_lastResult != null)
            {
                // Tab selection
                _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
                EditorGUILayout.Space();
                
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                
                switch (_selectedTab)
                {
                    case 0: DrawOverviewTab(); break;
                    case 1: DrawDependenciesTab(); break;
                    case 2: DrawCircularDependenciesTab(); break;
                    case 3: DrawValidationTab(); break;
                    case 4: DrawDetailsTab(); break;
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("Click 'Analyze' to perform assembly definition analysis", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawOverviewTab()
        {
            EditorGUILayout.LabelField("Analysis Overview", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField($"Analyzed: {_lastResult.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC");
            EditorGUILayout.LabelField($"Version: {_lastResult.AnalysisVersion}");
            EditorGUILayout.Space();
            
            // Summary statistics
            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total Assemblies: {_lastResult.Assemblies.Count}");
            EditorGUILayout.LabelField($"Total Dependencies: {_lastResult.Dependencies.Count}");
            EditorGUILayout.LabelField($"Circular Dependencies: {_lastResult.CircularDependencies.Count}");
            EditorGUILayout.LabelField($"Validation Issues: {_lastResult.ValidationIssues.Count}");
            EditorGUILayout.Space();
            
            // Module states
            var implementedCount = _lastResult.ModuleStates.Count(s => s.Value == ModuleState.Implemented);
            var disabledCount = _lastResult.ModuleStates.Count(s => s.Value == ModuleState.Disabled);
            var conceptualCount = _lastResult.ModuleStates.Count(s => s.Value == ModuleState.Conceptual);
            
            EditorGUILayout.LabelField("Module States", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"✅ Implemented: {implementedCount}");
            EditorGUILayout.LabelField($"❌ Disabled: {disabledCount}");
            EditorGUILayout.LabelField($"🔬 Conceptual: {conceptualCount}");
            EditorGUILayout.Space();
            
            // Assembly list
            EditorGUILayout.LabelField("Assemblies", EditorStyles.boldLabel);
            foreach (var assembly in _lastResult.Assemblies.OrderBy(a => a.Name))
            {
                var state = _lastResult.ModuleStates.GetValueOrDefault(assembly.Name, ModuleState.Conceptual);
                var stateIcon = state switch
                {
                    ModuleState.Implemented => "✅",
                    ModuleState.Disabled => "❌",
                    ModuleState.Conceptual => "🔬",
                    _ => "❓"
                };
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{stateIcon} {assembly.Name}", GUILayout.Width(250));
                EditorGUILayout.LabelField($"{assembly.SourceFiles.Count} files", GUILayout.Width(80));
                EditorGUILayout.LabelField($"{assembly.References.Count} refs", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawDependenciesTab()
        {
            EditorGUILayout.LabelField("Dependency Analysis", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (_lastResult.Dependencies.Count == 0)
            {
                EditorGUILayout.LabelField("No dependencies found", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            var dependencyGroups = _lastResult.Dependencies.GroupBy(d => d.Type);
            foreach (var group in dependencyGroups.OrderBy(g => g.Key.ToString()))
            {
                EditorGUILayout.LabelField($"{group.Key} Dependencies ({group.Count()})", EditorStyles.boldLabel);
                
                foreach (var dependency in group.OrderBy(d => d.FromAssembly).ThenBy(d => d.ToAssembly))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(dependency.FromAssembly, GUILayout.Width(150));
                    EditorGUILayout.LabelField("→", GUILayout.Width(20));
                    EditorGUILayout.LabelField(dependency.ToAssembly, GUILayout.Width(150));
                    
                    if (dependency.IsOptional)
                    {
                        EditorGUILayout.LabelField("(Optional)", GUILayout.Width(80));
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    if (!string.IsNullOrEmpty(dependency.Reason))
                    {
                        EditorGUILayout.LabelField($"  {dependency.Reason}", EditorStyles.miniLabel);
                    }
                    
                    EditorGUILayout.Space(2);
                }
                
                EditorGUILayout.Space();
            }
        }
        
        private void DrawCircularDependenciesTab()
        {
            EditorGUILayout.LabelField("Circular Dependencies", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (_lastResult.CircularDependencies.Count == 0)
            {
                EditorGUILayout.LabelField("✅ No circular dependencies detected", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            foreach (var circular in _lastResult.CircularDependencies.OrderByDescending(c => c.Severity))
            {
                var severityColor = circular.Severity switch
                {
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.Critical => Color.red,
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.High => new Color(1f, 0.5f, 0f),
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.Medium => Color.yellow,
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.Low => Color.green,
                    _ => Color.white
                };
                
                var severityIcon = circular.Severity switch
                {
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.Critical => "🔴",
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.High => "🟠",
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.Medium => "🟡",
                    AssemblyDefinitionAnalyzer.CircularDependencySeverity.Low => "🟢",
                    _ => "⚪"
                };
                
                var oldColor = GUI.color;
                GUI.color = severityColor;
                EditorGUILayout.LabelField($"{severityIcon} {circular.Severity} Severity", EditorStyles.boldLabel);
                GUI.color = oldColor;
                
                EditorGUILayout.LabelField(circular.Description, EditorStyles.wordWrappedLabel);
                
                EditorGUILayout.LabelField("Chain:", EditorStyles.miniLabel);
                foreach (var assembly in circular.AssemblyChain)
                {
                    EditorGUILayout.LabelField($"  • {assembly}", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.Space();
            }
        }
        
        private void DrawValidationTab()
        {
            EditorGUILayout.LabelField("Validation Issues", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (_lastResult.ValidationIssues.Count == 0)
            {
                EditorGUILayout.LabelField("✅ No validation issues found", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            var issueGroups = _lastResult.ValidationIssues.GroupBy(i => i.Severity);
            foreach (var group in issueGroups.OrderByDescending(g => g.Key))
            {
                var severityIcon = group.Key switch
                {
                    AssemblyDefinitionAnalyzer.ValidationSeverity.Critical => "🔴",
                    AssemblyDefinitionAnalyzer.ValidationSeverity.Error => "❌",
                    AssemblyDefinitionAnalyzer.ValidationSeverity.Warning => "⚠️",
                    AssemblyDefinitionAnalyzer.ValidationSeverity.Info => "ℹ️",
                    _ => "❓"
                };
                
                EditorGUILayout.LabelField($"{severityIcon} {group.Key} Issues ({group.Count()})", EditorStyles.boldLabel);
                
                foreach (var issue in group.OrderBy(i => i.AssemblyName))
                {
                    EditorGUILayout.LabelField($"[{issue.AssemblyName}] {issue.Title}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(issue.Description, EditorStyles.wordWrappedLabel);
                    
                    if (!string.IsNullOrEmpty(issue.Recommendation))
                    {
                        EditorGUILayout.LabelField($"💡 {issue.Recommendation}", EditorStyles.wordWrappedMiniLabel);
                    }
                    
                    EditorGUILayout.Space();
                }
            }
        }
        
        private void DrawDetailsTab()
        {
            EditorGUILayout.LabelField("Assembly Details", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            foreach (var assembly in _lastResult.Assemblies.OrderBy(a => a.Name))
            {
                EditorGUILayout.LabelField(assembly.Name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Path: {assembly.FilePath}");
                EditorGUILayout.LabelField($"Namespace: {assembly.RootNamespace ?? "None"}");
                EditorGUILayout.LabelField($"Auto Referenced: {assembly.AutoReferenced}");
                EditorGUILayout.LabelField($"Allow Unsafe: {assembly.AllowUnsafeCode}");
                
                if (assembly.References.Count > 0)
                {
                    EditorGUILayout.LabelField($"References ({assembly.References.Count}):");
                    foreach (var reference in assembly.References)
                    {
                        EditorGUILayout.LabelField($"  • {reference}", EditorStyles.miniLabel);
                    }
                }
                
                if (assembly.DefineConstraints.Count > 0)
                {
                    EditorGUILayout.LabelField($"Define Constraints ({assembly.DefineConstraints.Count}):");
                    foreach (var constraint in assembly.DefineConstraints)
                    {
                        EditorGUILayout.LabelField($"  • {constraint}", EditorStyles.miniLabel);
                    }
                }
                
                EditorGUILayout.LabelField($"Files: {assembly.SourceFiles.Count} source, {assembly.TestFiles.Count} test, {assembly.EditorFiles.Count} editor");
                
                EditorGUILayout.Space();
            }
        }
        
        private void Update()
        {
            if (_autoRefresh && EditorApplication.timeSinceStartup - _lastRefreshTime > 15.0)
            {
                RefreshAnalysis();
            }
        }
        
        private void RefreshAnalysis()
        {
            try
            {
                var analyzer = new AssemblyDefinitionAnalyzerService();
                var assemblyPaths = GetAssemblyDefinitionPaths();
                _lastResult = analyzer.AnalyzeAssemblyDefinitions(assemblyPaths);
                _lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
                
                UnityEngine.Debug.Log($"[AssemblyAnalyzer] Analysis refreshed: {_lastResult.Assemblies.Count} assemblies analyzed");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[AssemblyAnalyzer] Failed to refresh analysis: {ex.Message}");
            }
        }
        
        private void SaveReport()
        {
            if (_lastResult == null) return;
            
            try
            {
                var analyzer = new AssemblyDefinitionAnalyzerService();
                var report = analyzer.GenerateAnalysisReport(_lastResult);
                var reportPath = SaveAnalysisReport(report);
                
                UnityEngine.Debug.Log($"Analysis report saved to: {reportPath}");
                EditorUtility.DisplayDialog("Report Saved", $"Analysis report saved to:\n{reportPath}", "OK");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to save report: {ex.Message}");
                EditorUtility.DisplayDialog("Save Failed", $"Failed to save report:\n{ex.Message}", "OK");
            }
        }
        
        private void ExportJson()
        {
            if (_lastResult == null) return;
            
            try
            {
                var analyzer = new AssemblyDefinitionAnalyzerService();
                var json = analyzer.ExportToJson(_lastResult);
                
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                var jsonPath = Path.Combine(projectRoot, "ASSEMBLY_ANALYSIS_DATA.json");
                File.WriteAllText(jsonPath, json);
                
                UnityEngine.Debug.Log($"Analysis data exported to: {jsonPath}");
                EditorUtility.DisplayDialog("Export Complete", $"Analysis data exported to:\n{jsonPath}", "OK");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to export JSON: {ex.Message}");
                EditorUtility.DisplayDialog("Export Failed", $"Failed to export JSON:\n{ex.Message}", "OK");
            }
        }
        
        private System.Collections.Generic.List<string> GetAssemblyDefinitionPaths()
        {
            var paths = new System.Collections.Generic.List<string>();
            var xrBubbleLibraryPath = Path.Combine(Application.dataPath, "XRBubbleLibrary");
            
            if (Directory.Exists(xrBubbleLibraryPath))
            {
                var asmdefFiles = Directory.GetFiles(xrBubbleLibraryPath, "*.asmdef", SearchOption.AllDirectories);
                paths.AddRange(asmdefFiles);
            }
            
            return paths;
        }
        
        private string SaveAnalysisReport(string report)
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var reportPath = Path.Combine(projectRoot, "ASSEMBLY_ANALYSIS_REPORT.md");
            File.WriteAllText(reportPath, report);
            return reportPath;
        }
    }
}