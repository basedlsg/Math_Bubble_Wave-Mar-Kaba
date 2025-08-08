using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XRBubbleLibrary.Core.Editor
{
    /// <summary>
    /// Unity Editor integration for the Report Formatting and Generation System.
    /// Provides GUI tools for generating, formatting, and managing development state reports.
    /// </summary>
    public class ReportFormatterEditor : EditorWindow
    {
        private IReportFormatter _reportFormatter;
        private IDevStateGenerator _devStateGenerator;
        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private string[] _tabNames = { "Generate", "Format", "Schedule", "History" };
        
        // Generate tab
        private DevStateReport _currentReport;
        private bool _isGenerating = false;
        
        // Format tab
        private ReportFormat _selectedFormat = ReportFormat.Markdown;
        private MarkdownFormatOptions _markdownOptions;
        private HtmlFormatOptions _htmlOptions;
        private JsonFormatOptions _jsonOptions;
        private CsvFormatOptions _csvOptions;
        private string _formattedOutput = "";
        private bool _isFormatting = false;
        
        // Schedule tab
        private bool _nightlyGenerationEnabled = false;
        private int _generationHour = 2; // 2 AM
        private string _outputDirectory = "";
        
        // History tab
        private List<ReportHistoryEntry> _reportHistory;
        private bool _isLoadingHistory = false;
        
        [MenuItem("XR Bubble Library/Report Formatting System")]
        public static void ShowWindow()
        {
            var window = GetWindow<ReportFormatterEditor>("Report Formatting System");
            window.minSize = new Vector2(900, 700);
            window.Show();
        }
        
        private void OnEnable()
        {
            _reportFormatter = new ReportFormatter();
            _devStateGenerator = DevStateGenerator.Instance;
            
            InitializeFormatOptions();
            LoadReportHistory();
            
            _outputDirectory = Path.Combine(Application.dataPath, "..", "Reports");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            EditorGUILayout.LabelField("Report Formatting System", EditorStyles.largeLabel);
            EditorGUILayout.LabelField("Generate, format, and manage development state reports", EditorStyles.helpBox);
            EditorGUILayout.Space();
            
            // Tab selection
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            EditorGUILayout.Space();
            
            // Tab content
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            switch (_selectedTab)
            {
                case 0:
                    DrawGenerateTab();
                    break;
                case 1:
                    DrawFormatTab();
                    break;
                case 2:
                    DrawScheduleTab();
                    break;
                case 3:
                    DrawHistoryTab();
                    break;
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        #region Generate Tab
        
        private void DrawGenerateTab()
        {
            EditorGUILayout.LabelField("Report Generation", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Generation controls
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !_isGenerating;
            if (GUILayout.Button("Generate New Report", GUILayout.Height(30)))
            {
                GenerateNewReport();
            }
            
            if (GUILayout.Button("Generate and Save", GUILayout.Height(30)))
            {
                GenerateAndSaveReport();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (_isGenerating)
            {
                EditorGUILayout.HelpBox("Generating development state report...", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Display current report
            if (_currentReport != null)
            {
                EditorGUILayout.LabelField("Current Report", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Generated: {_currentReport.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
                EditorGUILayout.LabelField($"Build Version: {_currentReport.BuildVersion}");
                EditorGUILayout.LabelField($"Unity Version: {_currentReport.UnityVersion}");
                EditorGUILayout.LabelField($"Build Configuration: {_currentReport.BuildConfiguration}");
                
                if (_currentReport.Summary != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Summary:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"  Total Modules: {_currentReport.Summary.TotalModules}");
                    EditorGUILayout.LabelField($"  Implemented: {_currentReport.Summary.ImplementedModules} ({_currentReport.Summary.ImplementedPercentage:F1}%)");
                    EditorGUILayout.LabelField($"  Disabled: {_currentReport.Summary.DisabledModules} ({_currentReport.Summary.DisabledPercentage:F1}%)");
                    EditorGUILayout.LabelField($"  Conceptual: {_currentReport.Summary.ConceptualModules} ({_currentReport.Summary.ConceptualPercentage:F1}%)");
                }
                
                if (_currentReport.SupportingEvidence != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"Evidence Files: {_currentReport.SupportingEvidence.Count}");
                }
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space();
                
                // Validation
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Validate Report"))
                {
                    ValidateCurrentReport();
                }
                
                if (GUILayout.Button("Copy to Format Tab"))
                {
                    _selectedTab = 1; // Switch to format tab
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void GenerateNewReport()
        {
            _isGenerating = true;
            
            try
            {
                _currentReport = _devStateGenerator.GenerateReport();
                Debug.Log("[ReportFormatterEditor] Generated new development state report");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatterEditor] Failed to generate report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate report: {ex.Message}", "OK");
            }
            finally
            {
                _isGenerating = false;
            }
        }
        
        private void GenerateAndSaveReport()
        {
            _isGenerating = true;
            
            try
            {
                var filePath = _devStateGenerator.GenerateAndSaveDevStateFile();
                _currentReport = _devStateGenerator.GenerateReport();
                
                Debug.Log($"[ReportFormatterEditor] Generated and saved report: {filePath}");
                
                if (EditorUtility.DisplayDialog("Report Saved", 
                    $"Development state report saved successfully!\n\nPath: {filePath}", 
                    "Open File", "OK"))
                {
                    Application.OpenURL($"file://{filePath}");
                }
                
                AddToHistory(filePath, ReportFormat.Markdown);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatterEditor] Failed to generate and save report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate and save report: {ex.Message}", "OK");
            }
            finally
            {
                _isGenerating = false;
            }
        }
        
        private void ValidateCurrentReport()
        {
            if (_currentReport == null)
            {
                EditorUtility.DisplayDialog("No Report", "Please generate a report first.", "OK");
                return;
            }
            
            var validation = _reportFormatter.ValidateReport(_currentReport);
            
            var message = $"Validation Results:\n\n" +
                         $"Valid: {(validation.IsValid ? "Yes" : "No")}\n" +
                         $"Errors: {validation.Errors.Count}\n" +
                         $"Warnings: {validation.Warnings.Count}";
            
            if (validation.Errors.Any())
            {
                message += "\n\nErrors:\n" + string.Join("\n", validation.Errors);
            }
            
            if (validation.Warnings.Any())
            {
                message += "\n\nWarnings:\n" + string.Join("\n", validation.Warnings);
            }
            
            EditorUtility.DisplayDialog("Report Validation", message, "OK");
        }
        
        #endregion
        
        #region Format Tab
        
        private void DrawFormatTab()
        {
            EditorGUILayout.LabelField("Report Formatting", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (_currentReport == null)
            {
                EditorGUILayout.HelpBox("Please generate a report first in the Generate tab.", MessageType.Info);
                return;
            }
            
            // Format selection
            EditorGUILayout.LabelField("Output Format", EditorStyles.boldLabel);
            _selectedFormat = (ReportFormat)EditorGUILayout.EnumPopup("Format", _selectedFormat);
            EditorGUILayout.Space();
            
            // Format-specific options
            DrawFormatOptions();
            
            EditorGUILayout.Space();
            
            // Format controls
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !_isFormatting;
            if (GUILayout.Button("Format Report", GUILayout.Height(30)))
            {
                FormatCurrentReport();
            }
            
            if (GUILayout.Button("Format and Save", GUILayout.Height(30)))
            {
                FormatAndSaveReport();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (_isFormatting)
            {
                EditorGUILayout.HelpBox("Formatting report...", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Display formatted output
            if (!string.IsNullOrEmpty(_formattedOutput))
            {
                EditorGUILayout.LabelField("Formatted Output", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                
                var outputScrollPosition = EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(300));
                EditorGUILayout.TextArea(_formattedOutput, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy to Clipboard"))
                {
                    EditorGUIUtility.systemCopyBuffer = _formattedOutput;
                    Debug.Log("[ReportFormatterEditor] Formatted output copied to clipboard");
                }
                
                if (GUILayout.Button("Clear Output"))
                {
                    _formattedOutput = "";
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawFormatOptions()
        {
            switch (_selectedFormat)
            {
                case ReportFormat.Markdown:
                    DrawMarkdownOptions();
                    break;
                case ReportFormat.Html:
                    DrawHtmlOptions();
                    break;
                case ReportFormat.Json:
                    DrawJsonOptions();
                    break;
                case ReportFormat.Csv:
                    DrawCsvOptions();
                    break;
            }
        }
        
        private void DrawMarkdownOptions()
        {
            EditorGUILayout.LabelField("Markdown Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            _markdownOptions.IncludeTableOfContents = EditorGUILayout.Toggle("Include Table of Contents", _markdownOptions.IncludeTableOfContents);
            _markdownOptions.IncludeDetailedModules = EditorGUILayout.Toggle("Include Detailed Modules", _markdownOptions.IncludeDetailedModules);
            _markdownOptions.IncludePerformanceMetrics = EditorGUILayout.Toggle("Include Performance Metrics", _markdownOptions.IncludePerformanceMetrics);
            _markdownOptions.IncludeEvidence = EditorGUILayout.Toggle("Include Evidence", _markdownOptions.IncludeEvidence);
            _markdownOptions.IncludeCompilerFlags = EditorGUILayout.Toggle("Include Compiler Flags", _markdownOptions.IncludeCompilerFlags);
            _markdownOptions.IncludeDependencyAnalysis = EditorGUILayout.Toggle("Include Dependency Analysis", _markdownOptions.IncludeDependencyAnalysis);
            _markdownOptions.IncludeValidationWarnings = EditorGUILayout.Toggle("Include Validation Warnings", _markdownOptions.IncludeValidationWarnings);
            _markdownOptions.UseUtcTime = EditorGUILayout.Toggle("Use UTC Time", _markdownOptions.UseUtcTime);
            _markdownOptions.MaxEvidencePerType = EditorGUILayout.IntSlider("Max Evidence Per Type", _markdownOptions.MaxEvidencePerType, 1, 50);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawHtmlOptions()
        {
            EditorGUILayout.LabelField("HTML Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            _htmlOptions.PageTitle = EditorGUILayout.TextField("Page Title", _htmlOptions.PageTitle);
            _htmlOptions.IncludeCss = EditorGUILayout.Toggle("Include CSS", _htmlOptions.IncludeCss);
            _htmlOptions.IncludeJavaScript = EditorGUILayout.Toggle("Include JavaScript", _htmlOptions.IncludeJavaScript);
            _htmlOptions.IncludeNavigation = EditorGUILayout.Toggle("Include Navigation", _htmlOptions.IncludeNavigation);
            _htmlOptions.IncludeSearch = EditorGUILayout.Toggle("Include Search", _htmlOptions.IncludeSearch);
            _htmlOptions.CustomCssPath = EditorGUILayout.TextField("Custom CSS Path", _htmlOptions.CustomCssPath ?? "");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawJsonOptions()
        {
            EditorGUILayout.LabelField("JSON Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            _jsonOptions.PrettyPrint = EditorGUILayout.Toggle("Pretty Print", _jsonOptions.PrettyPrint);
            _jsonOptions.IncludeMetadata = EditorGUILayout.Toggle("Include Metadata", _jsonOptions.IncludeMetadata);
            _jsonOptions.IncludeValidation = EditorGUILayout.Toggle("Include Validation", _jsonOptions.IncludeValidation);
            _jsonOptions.SchemaPath = EditorGUILayout.TextField("Schema Path", _jsonOptions.SchemaPath ?? "");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawCsvOptions()
        {
            EditorGUILayout.LabelField("CSV Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            _csvOptions.IncludeHeaders = EditorGUILayout.Toggle("Include Headers", _csvOptions.IncludeHeaders);
            _csvOptions.SummaryOnly = EditorGUILayout.Toggle("Summary Only", _csvOptions.SummaryOnly);
            _csvOptions.IncludePerformanceColumns = EditorGUILayout.Toggle("Include Performance Columns", _csvOptions.IncludePerformanceColumns);
            
            var delimiterString = EditorGUILayout.TextField("Delimiter", _csvOptions.Delimiter.ToString());
            if (!string.IsNullOrEmpty(delimiterString))
            {
                _csvOptions.Delimiter = delimiterString[0];
            }
            
            var quoteString = EditorGUILayout.TextField("Quote Character", _csvOptions.QuoteCharacter.ToString());
            if (!string.IsNullOrEmpty(quoteString))
            {
                _csvOptions.QuoteCharacter = quoteString[0];
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void FormatCurrentReport()
        {
            _isFormatting = true;
            
            try
            {
                switch (_selectedFormat)
                {
                    case ReportFormat.Markdown:
                        _formattedOutput = _reportFormatter.FormatAsMarkdown(_currentReport, _markdownOptions);
                        break;
                    case ReportFormat.Html:
                        _formattedOutput = _reportFormatter.FormatAsHtml(_currentReport, _htmlOptions);
                        break;
                    case ReportFormat.Json:
                        _formattedOutput = _reportFormatter.FormatAsJson(_currentReport, _jsonOptions);
                        break;
                    case ReportFormat.Csv:
                        _formattedOutput = _reportFormatter.FormatAsCsv(_currentReport, _csvOptions);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported format: {_selectedFormat}");
                }
                
                Debug.Log($"[ReportFormatterEditor] Formatted report as {_selectedFormat}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatterEditor] Failed to format report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to format report: {ex.Message}", "OK");
            }
            finally
            {
                _isFormatting = false;
            }
        }
        
        private void FormatAndSaveReport()
        {
            FormatCurrentReport();
            
            if (string.IsNullOrEmpty(_formattedOutput))
            {
                EditorUtility.DisplayDialog("Error", "No formatted output to save.", "OK");
                return;
            }
            
            var extension = _selectedFormat switch
            {
                ReportFormat.Markdown => "md",
                ReportFormat.Html => "html",
                ReportFormat.Json => "json",
                ReportFormat.Csv => "csv",
                _ => "txt"
            };
            
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"dev_state_report_{timestamp}.{extension}";
            var filePath = Path.Combine(_outputDirectory, fileName);
            
            try
            {
                Directory.CreateDirectory(_outputDirectory);
                File.WriteAllText(filePath, _formattedOutput);
                
                Debug.Log($"[ReportFormatterEditor] Saved formatted report: {filePath}");
                
                if (EditorUtility.DisplayDialog("Report Saved", 
                    $"Formatted report saved successfully!\n\nPath: {filePath}", 
                    "Open File", "OK"))
                {
                    Application.OpenURL($"file://{filePath}");
                }
                
                AddToHistory(filePath, _selectedFormat);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatterEditor] Failed to save formatted report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to save report: {ex.Message}", "OK");
            }
        }
        
        #endregion
        
        #region Schedule Tab
        
        private void DrawScheduleTab()
        {
            EditorGUILayout.LabelField("Report Scheduling", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("Automated report scheduling is not yet implemented in this version.", MessageType.Info);
            
            // Placeholder for future scheduling features
            EditorGUILayout.LabelField("Nightly Generation", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            _nightlyGenerationEnabled = EditorGUILayout.Toggle("Enable Nightly Generation", _nightlyGenerationEnabled);
            GUI.enabled = _nightlyGenerationEnabled;
            _generationHour = EditorGUILayout.IntSlider("Generation Hour (24h)", _generationHour, 0, 23);
            _outputDirectory = EditorGUILayout.TextField("Output Directory", _outputDirectory);
            GUI.enabled = true;
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Configure Scheduling"))
            {
                EditorUtility.DisplayDialog("Not Implemented", 
                    "Automated scheduling will be implemented in a future version.", "OK");
            }
        }
        
        #endregion
        
        #region History Tab
        
        private void DrawHistoryTab()
        {
            EditorGUILayout.LabelField("Report History", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh History"))
            {
                LoadReportHistory();
            }
            
            if (GUILayout.Button("Clear History"))
            {
                if (EditorUtility.DisplayDialog("Clear History", 
                    "Are you sure you want to clear the report history?", "Yes", "No"))
                {
                    _reportHistory.Clear();
                    SaveReportHistory();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            if (_isLoadingHistory)
            {
                EditorGUILayout.HelpBox("Loading report history...", MessageType.Info);
                return;
            }
            
            if (_reportHistory == null || !_reportHistory.Any())
            {
                EditorGUILayout.HelpBox("No report history found.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField($"Total Reports: {_reportHistory.Count}", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            foreach (var entry in _reportHistory.OrderByDescending(e => e.GeneratedAt))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{entry.GeneratedAt:yyyy-MM-dd HH:mm:ss}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"[{entry.Format}]", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField($"File: {Path.GetFileName(entry.FilePath)}");
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                {
                    if (File.Exists(entry.FilePath))
                    {
                        Application.OpenURL($"file://{entry.FilePath}");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("File Not Found", 
                            $"The report file could not be found:\n{entry.FilePath}", "OK");
                    }
                }
                
                if (GUILayout.Button("Show in Explorer", GUILayout.Width(120)))
                {
                    if (File.Exists(entry.FilePath))
                    {
                        EditorUtility.RevealInFinder(entry.FilePath);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("File Not Found", 
                            $"The report file could not be found:\n{entry.FilePath}", "OK");
                    }
                }
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    _reportHistory.Remove(entry);
                    SaveReportHistory();
                    break; // Exit loop since we modified the collection
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void InitializeFormatOptions()
        {
            _markdownOptions = new MarkdownFormatOptions();
            _htmlOptions = new HtmlFormatOptions();
            _jsonOptions = new JsonFormatOptions();
            _csvOptions = new CsvFormatOptions();
        }
        
        private void LoadReportHistory()
        {
            _isLoadingHistory = true;
            
            try
            {
                var historyFile = Path.Combine(Application.persistentDataPath, "report_history.json");
                
                if (File.Exists(historyFile))
                {
                    var json = File.ReadAllText(historyFile);
                    var historyData = JsonUtility.FromJson<ReportHistoryData>(json);
                    _reportHistory = historyData?.Entries ?? new List<ReportHistoryEntry>();
                }
                else
                {
                    _reportHistory = new List<ReportHistoryEntry>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatterEditor] Failed to load report history: {ex.Message}");
                _reportHistory = new List<ReportHistoryEntry>();
            }
            finally
            {
                _isLoadingHistory = false;
            }
        }
        
        private void SaveReportHistory()
        {
            try
            {
                var historyFile = Path.Combine(Application.persistentDataPath, "report_history.json");
                var historyData = new ReportHistoryData { Entries = _reportHistory };
                var json = JsonUtility.ToJson(historyData, true);
                
                File.WriteAllText(historyFile, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatterEditor] Failed to save report history: {ex.Message}");
            }
        }
        
        private void AddToHistory(string filePath, ReportFormat format)
        {
            if (_reportHistory == null)
            {
                _reportHistory = new List<ReportHistoryEntry>();
            }
            
            _reportHistory.Add(new ReportHistoryEntry
            {
                FilePath = filePath,
                Format = format,
                GeneratedAt = DateTime.UtcNow
            });
            
            // Keep only the last 50 entries
            if (_reportHistory.Count > 50)
            {
                _reportHistory = _reportHistory.OrderByDescending(e => e.GeneratedAt).Take(50).ToList();
            }
            
            SaveReportHistory();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Data structure for report history persistence.
    /// </summary>
    [Serializable]
    public class ReportHistoryData
    {
        public List<ReportHistoryEntry> Entries = new List<ReportHistoryEntry>();
    }
    
    /// <summary>
    /// Individual report history entry.
    /// </summary>
    [Serializable]
    public class ReportHistoryEntry
    {
        public string FilePath;
        public ReportFormat Format;
        public DateTime GeneratedAt;
    }
    
    /// <summary>
    /// Menu items for quick report formatting operations.
    /// </summary>
    public static class ReportFormatterMenuItems
    {
        [MenuItem("XR Bubble Library/Reports/Generate Development State Report")]
        public static void GenerateDevStateReport()
        {
            try
            {
                var generator = DevStateGenerator.Instance;
                var filePath = generator.GenerateAndSaveDevStateFile();
                
                Debug.Log($"[ReportFormatter] Generated development state report: {filePath}");
                
                if (EditorUtility.DisplayDialog("Report Generated", 
                    $"Development state report generated successfully!\n\nPath: {filePath}", 
                    "Open Report", "OK"))
                {
                    Application.OpenURL($"file://{filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatter] Failed to generate report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate report: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/Reports/Generate HTML Report")]
        public static void GenerateHtmlReport()
        {
            try
            {
                var generator = DevStateGenerator.Instance;
                var formatter = new ReportFormatter();
                
                var report = generator.GenerateReport();
                var htmlContent = formatter.FormatAsHtml(report);
                
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var fileName = $"dev_state_report_{timestamp}.html";
                var outputDir = Path.Combine(Application.dataPath, "..", "Reports");
                var filePath = Path.Combine(outputDir, fileName);
                
                Directory.CreateDirectory(outputDir);
                File.WriteAllText(filePath, htmlContent);
                
                Debug.Log($"[ReportFormatter] Generated HTML report: {filePath}");
                
                if (EditorUtility.DisplayDialog("HTML Report Generated", 
                    $"HTML report generated successfully!\n\nPath: {filePath}", 
                    "Open Report", "OK"))
                {
                    Application.OpenURL($"file://{filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatter] Failed to generate HTML report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate HTML report: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/Reports/Generate JSON Report")]
        public static void GenerateJsonReport()
        {
            try
            {
                var generator = DevStateGenerator.Instance;
                var formatter = new ReportFormatter();
                
                var report = generator.GenerateReport();
                var jsonContent = formatter.FormatAsJson(report);
                
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var fileName = $"dev_state_report_{timestamp}.json";
                var outputDir = Path.Combine(Application.dataPath, "..", "Reports");
                var filePath = Path.Combine(outputDir, fileName);
                
                Directory.CreateDirectory(outputDir);
                File.WriteAllText(filePath, jsonContent);
                
                Debug.Log($"[ReportFormatter] Generated JSON report: {filePath}");
                
                if (EditorUtility.DisplayDialog("JSON Report Generated", 
                    $"JSON report generated successfully!\n\nPath: {filePath}", 
                    "Open Report", "OK"))
                {
                    Application.OpenURL($"file://{filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatter] Failed to generate JSON report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate JSON report: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/Reports/Generate CSV Report")]
        public static void GenerateCsvReport()
        {
            try
            {
                var generator = DevStateGenerator.Instance;
                var formatter = new ReportFormatter();
                
                var report = generator.GenerateReport();
                var csvContent = formatter.FormatAsCsv(report);
                
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var fileName = $"dev_state_report_{timestamp}.csv";
                var outputDir = Path.Combine(Application.dataPath, "..", "Reports");
                var filePath = Path.Combine(outputDir, fileName);
                
                Directory.CreateDirectory(outputDir);
                File.WriteAllText(filePath, csvContent);
                
                Debug.Log($"[ReportFormatter] Generated CSV report: {filePath}");
                
                if (EditorUtility.DisplayDialog("CSV Report Generated", 
                    $"CSV report generated successfully!\n\nPath: {filePath}", 
                    "Open Report", "OK"))
                {
                    Application.OpenURL($"file://{filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportFormatter] Failed to generate CSV report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate CSV report: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/Reports/Open Reports Folder")]
        public static void OpenReportsFolder()
        {
            var outputDir = Path.Combine(Application.dataPath, "..", "Reports");
            Directory.CreateDirectory(outputDir);
            EditorUtility.RevealInFinder(outputDir);
        }
    }
}