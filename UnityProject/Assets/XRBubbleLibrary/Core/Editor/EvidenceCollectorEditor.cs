using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XRBubbleLibrary.Core.Editor
{
    /// <summary>
    /// Unity Editor integration for the Evidence Collection System.
    /// Provides GUI tools for collecting, validating, and managing evidence files.
    /// </summary>
    public class EvidenceCollectorEditor : EditorWindow
    {
        private EvidenceCollectorService _evidenceService;
        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private string[] _tabNames = { "Collection", "Validation", "Search", "Archive" };
        
        // Collection tab
        private List<EvidenceFile> _collectedEvidence;
        private bool _isCollecting = false;
        
        // Validation tab
        private EvidenceValidationSummary _validationSummary;
        private bool _isValidating = false;
        
        // Search tab
        private EvidenceSearchCriteria _searchCriteria;
        private List<EvidenceFile> _searchResults;
        private bool _isSearching = false;
        
        // Archive tab
        private EvidenceArchiveResult _lastArchiveResult;
        private bool _isArchiving = false;
        
        [MenuItem("XR Bubble Library/Evidence Collection System")]
        public static void ShowWindow()
        {
            var window = GetWindow<EvidenceCollectorEditor>("Evidence Collection System");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            _evidenceService = new EvidenceCollectorService();
            _searchCriteria = new EvidenceSearchCriteria();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            EditorGUILayout.LabelField("Evidence Collection System", EditorStyles.largeLabel);
            EditorGUILayout.LabelField("Manage evidence files for evidence-based development", EditorStyles.helpBox);
            EditorGUILayout.Space();
            
            // Tab selection
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            EditorGUILayout.Space();
            
            // Tab content
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            switch (_selectedTab)
            {
                case 0:
                    DrawCollectionTab();
                    break;
                case 1:
                    DrawValidationTab();
                    break;
                case 2:
                    DrawSearchTab();
                    break;
                case 3:
                    DrawArchiveTab();
                    break;
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        #region Collection Tab
        
        private void DrawCollectionTab()
        {
            EditorGUILayout.LabelField("Evidence Collection", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Collection controls
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !_isCollecting;
            if (GUILayout.Button("Collect All Evidence", GUILayout.Height(30)))
            {
                CollectAllEvidence();
            }
            
            if (GUILayout.Button("Generate Evidence Report", GUILayout.Height(30)))
            {
                GenerateEvidenceReport();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (_isCollecting)
            {
                EditorGUILayout.HelpBox("Collecting evidence files...", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Display collected evidence
            if (_collectedEvidence != null && _collectedEvidence.Count > 0)
            {
                EditorGUILayout.LabelField($"Collected Evidence Files ({_collectedEvidence.Count})", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                
                // Group by module
                var moduleGroups = _collectedEvidence.GroupBy(e => e.ModuleName).OrderBy(g => g.Key);
                
                foreach (var moduleGroup in moduleGroups)
                {
                    EditorGUILayout.LabelField($"Module: {moduleGroup.Key}", EditorStyles.boldLabel);
                    
                    foreach (var evidence in moduleGroup.OrderBy(e => e.Type))
                    {
                        DrawEvidenceFileInfo(evidence);
                    }
                    
                    EditorGUILayout.Space();
                }
            }
            else if (_collectedEvidence != null)
            {
                EditorGUILayout.HelpBox("No evidence files found.", MessageType.Info);
            }
        }
        
        private void CollectAllEvidence()
        {
            _isCollecting = true;
            
            try
            {
                _collectedEvidence = _evidenceService.SearchEvidence(new EvidenceSearchCriteria { MaxResults = 1000 });
                Debug.Log($"[EvidenceCollectorEditor] Collected {_collectedEvidence.Count} evidence files");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollectorEditor] Failed to collect evidence: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to collect evidence: {ex.Message}", "OK");
            }
            finally
            {
                _isCollecting = false;
            }
        }
        
        private void GenerateEvidenceReport()
        {
            try
            {
                var reportPath = _evidenceService.GenerateEvidenceReport();
                Debug.Log($"[EvidenceCollectorEditor] Generated evidence report: {reportPath}");
                
                if (EditorUtility.DisplayDialog("Report Generated", 
                    $"Evidence report generated successfully!\n\nPath: {reportPath}", 
                    "Open Report", "OK"))
                {
                    Application.OpenURL($"file://{reportPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollectorEditor] Failed to generate report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate report: {ex.Message}", "OK");
            }
        }
        
        #endregion
        
        #region Validation Tab
        
        private void DrawValidationTab()
        {
            EditorGUILayout.LabelField("Evidence Validation", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Validation controls
            GUI.enabled = !_isValidating;
            if (GUILayout.Button("Validate All Evidence", GUILayout.Height(30)))
            {
                ValidateAllEvidence();
            }
            GUI.enabled = true;
            
            if (_isValidating)
            {
                EditorGUILayout.HelpBox("Validating evidence files...", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Display validation results
            if (_validationSummary != null)
            {
                EditorGUILayout.LabelField("Validation Summary", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Total Files: {_validationSummary.TotalEvidenceFiles}");
                EditorGUILayout.LabelField($"Valid Files: {_validationSummary.ValidFiles}");
                EditorGUILayout.LabelField($"Invalid Files: {_validationSummary.InvalidFiles}");
                EditorGUILayout.LabelField($"Success Rate: {_validationSummary.ValidationSuccessRate:P1}");
                EditorGUILayout.LabelField($"Validated At: {_validationSummary.ValidatedAt:yyyy-MM-dd HH:mm:ss}");
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space();
                
                // Show invalid files
                var invalidResults = _validationSummary.ValidationResults.Where(r => !r.IsValid).ToList();
                if (invalidResults.Any())
                {
                    EditorGUILayout.LabelField($"Invalid Evidence Files ({invalidResults.Count})", EditorStyles.boldLabel);
                    
                    foreach (var result in invalidResults)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField($"File: {Path.GetFileName(result.EvidenceFile.FilePath)}", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"Module: {result.EvidenceFile.ModuleName}");
                        EditorGUILayout.LabelField($"Type: {result.EvidenceFile.Type}");
                        
                        if (result.ValidationErrors.Any())
                        {
                            EditorGUILayout.LabelField("Errors:", EditorStyles.boldLabel);
                            foreach (var error in result.ValidationErrors)
                            {
                                EditorGUILayout.LabelField($"  • {error}", EditorStyles.wordWrappedLabel);
                            }
                        }
                        
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }
                }
            }
        }
        
        private void ValidateAllEvidence()
        {
            _isValidating = true;
            
            try
            {
                _validationSummary = _evidenceService.ValidateAllEvidence();
                Debug.Log($"[EvidenceCollectorEditor] Validated {_validationSummary.TotalEvidenceFiles} evidence files");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollectorEditor] Failed to validate evidence: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to validate evidence: {ex.Message}", "OK");
            }
            finally
            {
                _isValidating = false;
            }
        }
        
        #endregion
        
        #region Search Tab
        
        private void DrawSearchTab()
        {
            EditorGUILayout.LabelField("Evidence Search", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Search criteria
            EditorGUILayout.LabelField("Search Criteria", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Evidence type filter
            var evidenceTypes = Enum.GetValues(typeof(EvidenceType)).Cast<EvidenceType>().ToArray();
            var typeNames = evidenceTypes.Select(t => t.ToString()).Prepend("Any Type").ToArray();
            var selectedTypeIndex = _searchCriteria.Type.HasValue ? 
                Array.IndexOf(evidenceTypes, _searchCriteria.Type.Value) + 1 : 0;
            
            selectedTypeIndex = EditorGUILayout.Popup("Evidence Type", selectedTypeIndex, typeNames);
            _searchCriteria.Type = selectedTypeIndex > 0 ? evidenceTypes[selectedTypeIndex - 1] : (EvidenceType?)null;
            
            // Module name filter
            _searchCriteria.ModuleName = EditorGUILayout.TextField("Module Name", _searchCriteria.ModuleName ?? "");
            if (string.IsNullOrWhiteSpace(_searchCriteria.ModuleName))
                _searchCriteria.ModuleName = null;
            
            // Description filter
            _searchCriteria.DescriptionContains = EditorGUILayout.TextField("Description Contains", _searchCriteria.DescriptionContains ?? "");
            if (string.IsNullOrWhiteSpace(_searchCriteria.DescriptionContains))
                _searchCriteria.DescriptionContains = null;
            
            // Max results
            _searchCriteria.MaxResults = EditorGUILayout.IntSlider("Max Results", _searchCriteria.MaxResults, 1, 1000);
            
            // Only valid files
            _searchCriteria.OnlyValid = EditorGUILayout.Toggle("Only Valid Files", _searchCriteria.OnlyValid);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            
            // Search button
            GUI.enabled = !_isSearching;
            if (GUILayout.Button("Search Evidence", GUILayout.Height(30)))
            {
                SearchEvidence();
            }
            GUI.enabled = true;
            
            if (_isSearching)
            {
                EditorGUILayout.HelpBox("Searching evidence files...", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Display search results
            if (_searchResults != null)
            {
                EditorGUILayout.LabelField($"Search Results ({_searchResults.Count})", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                
                if (_searchResults.Any())
                {
                    foreach (var evidence in _searchResults)
                    {
                        DrawEvidenceFileInfo(evidence);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No evidence files match the search criteria.", MessageType.Info);
                }
            }
        }
        
        private void SearchEvidence()
        {
            _isSearching = true;
            
            try
            {
                _searchResults = _evidenceService.SearchEvidence(_searchCriteria);
                Debug.Log($"[EvidenceCollectorEditor] Found {_searchResults.Count} matching evidence files");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollectorEditor] Failed to search evidence: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to search evidence: {ex.Message}", "OK");
            }
            finally
            {
                _isSearching = false;
            }
        }
        
        #endregion
        
        #region Archive Tab
        
        private void DrawArchiveTab()
        {
            EditorGUILayout.LabelField("Evidence Archive", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Archive controls
            GUI.enabled = !_isArchiving;
            if (GUILayout.Button("Archive All Evidence", GUILayout.Height(30)))
            {
                ArchiveAllEvidence();
            }
            GUI.enabled = true;
            
            if (_isArchiving)
            {
                EditorGUILayout.HelpBox("Archiving evidence files...", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Display last archive result
            if (_lastArchiveResult != null)
            {
                EditorGUILayout.LabelField("Last Archive Result", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Success: {(_lastArchiveResult.Success ? "Yes" : "No")}");
                EditorGUILayout.LabelField($"Files Archived: {_lastArchiveResult.FilesArchived}");
                EditorGUILayout.LabelField($"Archive Size: {FormatBytes(_lastArchiveResult.ArchiveSizeBytes)}");
                EditorGUILayout.LabelField($"Created At: {_lastArchiveResult.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                
                if (!string.IsNullOrEmpty(_lastArchiveResult.ArchivePath))
                {
                    EditorGUILayout.LabelField($"Archive Path: {_lastArchiveResult.ArchivePath}");
                    
                    if (GUILayout.Button("Open Archive Folder"))
                    {
                        EditorUtility.RevealInFinder(_lastArchiveResult.ArchivePath);
                    }
                }
                
                if (_lastArchiveResult.Errors.Any())
                {
                    EditorGUILayout.LabelField("Errors:", EditorStyles.boldLabel);
                    foreach (var error in _lastArchiveResult.Errors)
                    {
                        EditorGUILayout.LabelField($"  • {error}", EditorStyles.wordWrappedLabel);
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        private void ArchiveAllEvidence()
        {
            _isArchiving = true;
            
            try
            {
                _lastArchiveResult = _evidenceService.ArchiveAllEvidence();
                Debug.Log($"[EvidenceCollectorEditor] Archived {_lastArchiveResult.FilesArchived} evidence files");
                
                if (_lastArchiveResult.Success)
                {
                    EditorUtility.DisplayDialog("Archive Complete", 
                        $"Successfully archived {_lastArchiveResult.FilesArchived} evidence files.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollectorEditor] Failed to archive evidence: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to archive evidence: {ex.Message}", "OK");
            }
            finally
            {
                _isArchiving = false;
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void DrawEvidenceFileInfo(EvidenceFile evidence)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(evidence.Description, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"[{evidence.Type}]", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField($"Module: {evidence.ModuleName}");
            EditorGUILayout.LabelField($"File: {Path.GetFileName(evidence.FilePath)}");
            EditorGUILayout.LabelField($"Size: {FormatBytes(evidence.FileSizeBytes)}");
            EditorGUILayout.LabelField($"Collected: {evidence.CollectedAt:yyyy-MM-dd HH:mm:ss}");
            
            if (evidence.Tags.Any())
            {
                EditorGUILayout.LabelField($"Tags: {string.Join(", ", evidence.Tags)}");
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open File", GUILayout.Width(80)))
            {
                if (File.Exists(evidence.FilePath))
                {
                    Application.OpenURL($"file://{evidence.FilePath}");
                }
                else
                {
                    EditorUtility.DisplayDialog("File Not Found", 
                        $"The evidence file could not be found:\n{evidence.FilePath}", "OK");
                }
            }
            
            if (GUILayout.Button("Show in Explorer", GUILayout.Width(120)))
            {
                if (File.Exists(evidence.FilePath))
                {
                    EditorUtility.RevealInFinder(evidence.FilePath);
                }
                else
                {
                    EditorUtility.DisplayDialog("File Not Found", 
                        $"The evidence file could not be found:\n{evidence.FilePath}", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Menu items for quick evidence collection operations.
    /// </summary>
    public static class EvidenceCollectorMenuItems
    {
        [MenuItem("XR Bubble Library/Evidence/Collect All Evidence")]
        public static void CollectAllEvidence()
        {
            try
            {
                var service = new EvidenceCollectorService();
                var evidence = service.SearchEvidence(new EvidenceSearchCriteria { MaxResults = 1000 });
                
                Debug.Log($"[EvidenceCollector] Collected {evidence.Count} evidence files");
                EditorUtility.DisplayDialog("Evidence Collection", 
                    $"Successfully collected {evidence.Count} evidence files.", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollector] Failed to collect evidence: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to collect evidence: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/Evidence/Generate Evidence Report")]
        public static void GenerateEvidenceReport()
        {
            try
            {
                var service = new EvidenceCollectorService();
                var reportPath = service.GenerateEvidenceReport();
                
                Debug.Log($"[EvidenceCollector] Generated evidence report: {reportPath}");
                
                if (EditorUtility.DisplayDialog("Report Generated", 
                    $"Evidence report generated successfully!\n\nPath: {reportPath}", 
                    "Open Report", "OK"))
                {
                    Application.OpenURL($"file://{reportPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollector] Failed to generate report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate report: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/Evidence/Validate All Evidence")]
        public static void ValidateAllEvidence()
        {
            try
            {
                var service = new EvidenceCollectorService();
                var summary = service.ValidateAllEvidence();
                
                Debug.Log($"[EvidenceCollector] Validated {summary.TotalEvidenceFiles} evidence files");
                
                var message = $"Validation Complete!\n\n" +
                             $"Total Files: {summary.TotalEvidenceFiles}\n" +
                             $"Valid Files: {summary.ValidFiles}\n" +
                             $"Invalid Files: {summary.InvalidFiles}\n" +
                             $"Success Rate: {summary.ValidationSuccessRate:P1}";
                
                EditorUtility.DisplayDialog("Evidence Validation", message, "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollector] Failed to validate evidence: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to validate evidence: {ex.Message}", "OK");
            }
        }
        
        [MenuItem("XR Bubble Library/Evidence/Archive All Evidence")]
        public static void ArchiveAllEvidence()
        {
            try
            {
                var service = new EvidenceCollectorService();
                var result = service.ArchiveAllEvidence();
                
                Debug.Log($"[EvidenceCollector] Archived {result.FilesArchived} evidence files");
                
                var message = result.Success ? 
                    $"Archive Complete!\n\nFiles Archived: {result.FilesArchived}\nArchive Path: {result.ArchivePath}" :
                    $"Archive Failed!\n\nErrors: {string.Join(", ", result.Errors)}";
                
                EditorUtility.DisplayDialog("Evidence Archive", message, "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceCollector] Failed to archive evidence: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to archive evidence: {ex.Message}", "OK");
            }
        }
    }
}