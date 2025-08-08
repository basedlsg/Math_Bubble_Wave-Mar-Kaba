using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Advanced report formatter for development state reports.
    /// Provides enhanced formatting capabilities with multiple output formats,
    /// scheduling, and integration features.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class ReportFormatter : IReportFormatter
    {
        private readonly string _templateDirectory;
        private readonly Dictionary<ReportFormat, string> _templates;
        
        /// <summary>
        /// Initializes a new instance of the ReportFormatter.
        /// </summary>
        public ReportFormatter()
        {
            _templateDirectory = Path.Combine(Application.dataPath, "XRBubbleLibrary", "Core", "Templates");
            _templates = new Dictionary<ReportFormat, string>();
            
            EnsureTemplateDirectoryExists();
            LoadTemplates();
        }
        
        /// <summary>
        /// Formats a development state report as enhanced Markdown.
        /// </summary>
        public string FormatAsMarkdown(DevStateReport report, MarkdownFormatOptions options = null)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            
            options ??= new MarkdownFormatOptions();
            
            var markdown = new StringBuilder();
            
            // Header with enhanced formatting
            markdown.AppendLine("# XR Bubble Library - Development State Report");
            markdown.AppendLine();
            
            // Metadata section
            AppendMetadataSection(markdown, report, options);
            
            // Table of contents
            if (options.IncludeTableOfContents)
            {
                AppendTableOfContents(markdown, options);
            }
            
            // Summary section with enhanced metrics
            AppendEnhancedSummarySection(markdown, report);
            
            // Compiler flags section
            if (options.IncludeCompilerFlags)
            {
                AppendCompilerFlagsSection(markdown, report);
            }
            
            // Module status section
            if (options.IncludeDetailedModules)
            {
                AppendDetailedModulesSection(markdown, report, options);
            }
            
            // Dependency analysis
            if (options.IncludeDependencyAnalysis)
            {
                AppendDependencyAnalysisSection(markdown, report);
            }
            
            // Performance metrics section
            if (options.IncludePerformanceMetrics)
            {
                AppendPerformanceMetricsSection(markdown, report);
            }
            
            // Evidence section
            if (options.IncludeEvidence)
            {
                AppendEvidenceSection(markdown, report, options);
            }
            
            // Validation warnings
            if (options.IncludeValidationWarnings)
            {
                AppendValidationWarningsSection(markdown, report);
            }
            
            // Footer
            AppendFooterSection(markdown, report);
            
            return markdown.ToString();
        }
        
        /// <summary>
        /// Formats a development state report as HTML.
        /// </summary>
        public string FormatAsHtml(DevStateReport report, HtmlFormatOptions options = null)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            
            options ??= new HtmlFormatOptions();
            
            var html = new StringBuilder();
            
            // HTML document structure
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine($"    <meta charset=\"UTF-8\">");
            html.AppendLine($"    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine($"    <title>{options.PageTitle}</title>");
            
            // CSS styling
            if (options.IncludeCss)
            {
                AppendHtmlStyles(html, options);
            }
            
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Navigation
            if (options.IncludeNavigation)
            {
                AppendHtmlNavigation(html, options);
            }
            
            // Main content
            html.AppendLine("<main class=\"container\">");
            
            // Convert markdown to HTML sections
            var markdownOptions = new MarkdownFormatOptions
            {
                IncludeTableOfContents = false, // Handle separately in HTML
                UseUtcTime = true
            };
            
            var markdownContent = FormatAsMarkdown(report, markdownOptions);
            var htmlContent = ConvertMarkdownToHtml(markdownContent);
            
            html.AppendLine(htmlContent);
            html.AppendLine("</main>");
            
            // JavaScript
            if (options.IncludeJavaScript)
            {
                AppendHtmlJavaScript(html, options);
            }
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
        
        /// <summary>
        /// Formats a development state report as enhanced JSON.
        /// </summary>
        public string FormatAsJson(DevStateReport report, JsonFormatOptions options = null)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            
            options ??= new JsonFormatOptions();
            
            var jsonData = new Dictionary<string, object>
            {
                ["reportMetadata"] = new
                {
                    generatedAt = report.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    buildVersion = report.BuildVersion,
                    unityVersion = report.UnityVersion,
                    buildConfiguration = report.BuildConfiguration,
                    formatVersion = "1.0"
                },
                ["summary"] = report.Summary,
                ["compilerFlags"] = report.CompilerFlags,
                ["modules"] = report.Modules.Select(m => new
                {
                    name = m.ModuleName,
                    assembly = m.AssemblyName,
                    state = m.State.ToString(),
                    dependencies = m.Dependencies,
                    evidence = m.Evidence?.Select(e => new
                    {
                        id = e.Id,
                        type = e.Type.ToString(),
                        path = e.FilePath,
                        hash = e.Hash,
                        collectedAt = e.CollectedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    })
                }),
                ["performance"] = report.CurrentPerformance,
                ["evidence"] = report.SupportingEvidence?.Select(e => new
                {
                    id = e.Id,
                    type = e.Type.ToString(),
                    moduleName = e.ModuleName,
                    description = e.Description,
                    filePath = e.FilePath,
                    hash = e.Hash,
                    collectedAt = e.CollectedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    isValid = e.IsValid,
                    tags = e.Tags
                })
            };
            
            // Add validation results if requested
            if (options.IncludeValidation)
            {
                var validation = ValidateReport(report);
                jsonData["validation"] = new
                {
                    isValid = validation.IsValid,
                    errors = validation.Errors,
                    warnings = validation.Warnings,
                    validatedAt = validation.ValidatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };
            }
            
            // Add metadata if requested
            if (options.IncludeMetadata)
            {
                jsonData["metadata"] = new
                {
                    formatter = "XRBubbleLibrary.Core.ReportFormatter",
                    version = "1.0.0",
                    generatedBy = Environment.UserName,
                    machineName = Environment.MachineName,
                    operatingSystem = SystemInfo.operatingSystem,
                    processorType = SystemInfo.processorType,
                    systemMemorySize = SystemInfo.systemMemorySize
                };
            }
            
            return options.PrettyPrint ? 
                JsonUtility.ToJson(jsonData, true) : 
                JsonUtility.ToJson(jsonData, false);
        }
        
        /// <summary>
        /// Formats a development state report as CSV for data analysis.
        /// </summary>
        public string FormatAsCsv(DevStateReport report, CsvFormatOptions options = null)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            
            options ??= new CsvFormatOptions();
            
            var csv = new StringBuilder();
            
            if (options.SummaryOnly)
            {
                // Summary CSV format
                if (options.IncludeHeaders)
                {
                    csv.AppendLine("GeneratedAt,BuildVersion,TotalModules,ImplementedModules,DisabledModules,ConceptualModules,ImplementedPercentage");
                }
                
                csv.AppendLine($"{report.GeneratedAt:yyyy-MM-dd HH:mm:ss},{EscapeCsvField(report.BuildVersion, options)}," +
                              $"{report.Summary?.TotalModules ?? 0},{report.Summary?.ImplementedModules ?? 0}," +
                              $"{report.Summary?.DisabledModules ?? 0},{report.Summary?.ConceptualModules ?? 0}," +
                              $"{report.Summary?.ImplementedPercentage ?? 0:F1}");
            }
            else
            {
                // Detailed module CSV format
                var headers = new List<string>
                {
                    "ModuleName", "AssemblyName", "State", "Dependencies", "EvidenceCount", "LastValidated"
                };
                
                if (options.IncludePerformanceColumns)
                {
                    headers.AddRange(new[] { "AverageFPS", "FrameTime", "MemoryUsage" });
                }
                
                if (options.IncludeHeaders)
                {
                    csv.AppendLine(string.Join(options.Delimiter.ToString(), headers));
                }
                
                foreach (var module in report.Modules)
                {
                    var row = new List<string>
                    {
                        EscapeCsvField(module.ModuleName, options),
                        EscapeCsvField(module.AssemblyName, options),
                        module.State.ToString(),
                        EscapeCsvField(string.Join(";", module.Dependencies ?? new List<string>()), options),
                        (module.Evidence?.Count ?? 0).ToString(),
                        module.LastValidated.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    
                    if (options.IncludePerformanceColumns)
                    {
                        row.Add((module.Performance?.AverageFPS ?? 0).ToString("F1"));
                        row.Add((module.Performance?.AverageFrameTime ?? 0).ToString("F2"));
                        row.Add((module.Performance?.MemoryUsage ?? 0).ToString());
                    }
                    
                    csv.AppendLine(string.Join(options.Delimiter.ToString(), row));
                }
            }
            
            return csv.ToString();
        }
        
        /// <summary>
        /// Generates a summary report with key metrics and status.
        /// </summary>
        public string GenerateSummary(DevStateReport report, ReportFormat format = ReportFormat.Markdown)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            
            switch (format)
            {
                case ReportFormat.Markdown:
                    return GenerateMarkdownSummary(report);
                case ReportFormat.Html:
                    return GenerateHtmlSummary(report);
                case ReportFormat.Json:
                    return GenerateJsonSummary(report);
                case ReportFormat.PlainText:
                    return GeneratePlainTextSummary(report);
                default:
                    throw new ArgumentException($"Unsupported format: {format}");
            }
        }
        
        /// <summary>
        /// Generates a comparison report between two development state reports.
        /// </summary>
        public string GenerateComparisonReport(DevStateReport previousReport, DevStateReport currentReport, ReportFormat format = ReportFormat.Markdown)
        {
            if (previousReport == null)
                throw new ArgumentNullException(nameof(previousReport));
            if (currentReport == null)
                throw new ArgumentNullException(nameof(currentReport));
            
            var comparison = new StringBuilder();
            
            switch (format)
            {
                case ReportFormat.Markdown:
                    GenerateMarkdownComparison(comparison, previousReport, currentReport);
                    break;
                case ReportFormat.Html:
                    GenerateHtmlComparison(comparison, previousReport, currentReport);
                    break;
                case ReportFormat.Json:
                    GenerateJsonComparison(comparison, previousReport, currentReport);
                    break;
                default:
                    throw new ArgumentException($"Unsupported format: {format}");
            }
            
            return comparison.ToString();
        }
        
        /// <summary>
        /// Validates that a report can be formatted without errors.
        /// </summary>
        public ReportValidationResult ValidateReport(DevStateReport report)
        {
            var result = new ReportValidationResult
            {
                ValidatedAt = DateTime.UtcNow
            };
            
            if (report == null)
            {
                result.Errors.Add("Report is null");
                result.IsValid = false;
                return result;
            }
            
            // Validate basic report structure
            if (string.IsNullOrEmpty(report.BuildVersion))
            {
                result.Warnings.Add("Build version is missing");
            }
            
            if (report.GeneratedAt == default)
            {
                result.Errors.Add("Generated timestamp is invalid");
            }
            
            if (report.Modules == null)
            {
                result.Errors.Add("Modules list is null");
            }
            else
            {
                // Validate modules
                foreach (var module in report.Modules)
                {
                    if (string.IsNullOrEmpty(module.ModuleName))
                    {
                        result.Errors.Add("Module with empty name found");
                    }
                    
                    if (string.IsNullOrEmpty(module.AssemblyName))
                    {
                        result.Warnings.Add($"Module {module.ModuleName} has no assembly name");
                    }
                }
                
                // Check for duplicate module names
                var duplicates = report.Modules.GroupBy(m => m.ModuleName)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);
                
                foreach (var duplicate in duplicates)
                {
                    result.Errors.Add($"Duplicate module name: {duplicate}");
                }
            }
            
            // Validate summary consistency
            if (report.Summary != null && report.Modules != null)
            {
                var actualTotal = report.Modules.Count;
                if (report.Summary.TotalModules != actualTotal)
                {
                    result.Errors.Add($"Summary total modules ({report.Summary.TotalModules}) doesn't match actual count ({actualTotal})");
                }
            }
            
            // Validate evidence files
            if (report.SupportingEvidence != null)
            {
                foreach (var evidence in report.SupportingEvidence)
                {
                    if (string.IsNullOrEmpty(evidence.Id))
                    {
                        result.Warnings.Add("Evidence file with missing ID found");
                    }
                    
                    if (string.IsNullOrEmpty(evidence.FilePath))
                    {
                        result.Warnings.Add($"Evidence {evidence.Id} has no file path");
                    }
                    else if (!File.Exists(evidence.FilePath))
                    {
                        result.Warnings.Add($"Evidence file not found: {evidence.FilePath}");
                    }
                }
            }
            
            result.IsValid = result.Errors.Count == 0;
            return result;
        }
        
        /// <summary>
        /// Formats multiple reports as a batch operation.
        /// </summary>
        public async Task<Dictionary<string, string>> FormatBatchAsync(List<DevStateReport> reports, ReportFormat format, object options = null)
        {
            if (reports == null)
                throw new ArgumentNullException(nameof(reports));
            
            var results = new Dictionary<string, string>();
            var tasks = new List<Task<KeyValuePair<string, string>>>();
            
            foreach (var report in reports)
            {
                var reportId = $"{report.BuildVersion}_{report.GeneratedAt:yyyyMMdd_HHmmss}";
                
                tasks.Add(Task.Run(() =>
                {
                    string formattedContent;
                    
                    switch (format)
                    {
                        case ReportFormat.Markdown:
                            formattedContent = FormatAsMarkdown(report, options as MarkdownFormatOptions);
                            break;
                        case ReportFormat.Html:
                            formattedContent = FormatAsHtml(report, options as HtmlFormatOptions);
                            break;
                        case ReportFormat.Json:
                            formattedContent = FormatAsJson(report, options as JsonFormatOptions);
                            break;
                        case ReportFormat.Csv:
                            formattedContent = FormatAsCsv(report, options as CsvFormatOptions);
                            break;
                        default:
                            throw new ArgumentException($"Unsupported format: {format}");
                    }
                    
                    return new KeyValuePair<string, string>(reportId, formattedContent);
                }));
            }
            
            var completedTasks = await Task.WhenAll(tasks);
            
            foreach (var result in completedTasks)
            {
                results[result.Key] = result.Value;
            }
            
            return results;
        }
        
        #region Private Helper Methods
        
        private void EnsureTemplateDirectoryExists()
        {
            if (!Directory.Exists(_templateDirectory))
            {
                Directory.CreateDirectory(_templateDirectory);
            }
        }
        
        private void LoadTemplates()
        {
            // Load template files if they exist
            var markdownTemplate = Path.Combine(_templateDirectory, "report_template.md");
            if (File.Exists(markdownTemplate))
            {
                _templates[ReportFormat.Markdown] = File.ReadAllText(markdownTemplate);
            }
            
            var htmlTemplate = Path.Combine(_templateDirectory, "report_template.html");
            if (File.Exists(htmlTemplate))
            {
                _templates[ReportFormat.Html] = File.ReadAllText(htmlTemplate);
            }
        }
        
        private void AppendMetadataSection(StringBuilder markdown, DevStateReport report, MarkdownFormatOptions options)
        {
            var timeFormat = options.UseUtcTime ? "yyyy-MM-dd HH:mm:ss UTC" : "yyyy-MM-dd HH:mm:ss";
            var timestamp = options.UseUtcTime ? report.GeneratedAt : report.GeneratedAt.ToLocalTime();
            
            markdown.AppendLine($"**Generated**: {timestamp.ToString(timeFormat)}");
            markdown.AppendLine($"**Build Version**: {report.BuildVersion}");
            markdown.AppendLine($"**Unity Version**: {report.UnityVersion}");
            markdown.AppendLine($"**Build Configuration**: {report.BuildConfiguration}");
            markdown.AppendLine($"**Report Format Version**: 2.0");
            markdown.AppendLine();
        }
        
        private void AppendTableOfContents(StringBuilder markdown, MarkdownFormatOptions options)
        {
            markdown.AppendLine("## Table of Contents");
            markdown.AppendLine();
            markdown.AppendLine("- [Summary](#summary)");
            
            if (options.IncludeCompilerFlags)
                markdown.AppendLine("- [Compiler Flags Status](#compiler-flags-status)");
            
            if (options.IncludeDetailedModules)
                markdown.AppendLine("- [Module Status](#module-status)");
            
            if (options.IncludeDependencyAnalysis)
                markdown.AppendLine("- [Dependency Analysis](#dependency-analysis)");
            
            if (options.IncludePerformanceMetrics)
                markdown.AppendLine("- [Performance Metrics](#performance-metrics)");
            
            if (options.IncludeEvidence)
                markdown.AppendLine("- [Supporting Evidence](#supporting-evidence)");
            
            if (options.IncludeValidationWarnings)
                markdown.AppendLine("- [Validation Warnings](#validation-warnings)");
            
            markdown.AppendLine();
        }
        
        private void AppendEnhancedSummarySection(StringBuilder markdown, DevStateReport report)
        {
            markdown.AppendLine("## Summary");
            markdown.AppendLine();
            
            if (report.Summary != null)
            {
                // Progress bar visualization
                var implementedBar = new string('█', (int)(report.Summary.ImplementedPercentage / 5));
                var disabledBar = new string('▓', (int)(report.Summary.DisabledPercentage / 5));
                var conceptualBar = new string('░', (int)(report.Summary.ConceptualPercentage / 5));
                
                markdown.AppendLine("### Module Status Overview");
                markdown.AppendLine();
                markdown.AppendLine($"```");
                markdown.AppendLine($"Progress: [{implementedBar}{disabledBar}{conceptualBar}]");
                markdown.AppendLine($"          Implemented: {report.Summary.ImplementedPercentage:F1}%");
                markdown.AppendLine($"          Disabled:    {report.Summary.DisabledPercentage:F1}%");
                markdown.AppendLine($"          Conceptual:  {report.Summary.ConceptualPercentage:F1}%");
                markdown.AppendLine($"```");
                markdown.AppendLine();
                
                // Detailed statistics
                markdown.AppendLine("### Detailed Statistics");
                markdown.AppendLine();
                markdown.AppendLine("| Status | Count | Percentage |");
                markdown.AppendLine("|--------|-------|------------|");
                markdown.AppendLine($"| ✅ Implemented | {report.Summary.ImplementedModules} | {report.Summary.ImplementedPercentage:F1}% |");
                markdown.AppendLine($"| ❌ Disabled | {report.Summary.DisabledModules} | {report.Summary.DisabledPercentage:F1}% |");
                markdown.AppendLine($"| 🔬 Conceptual | {report.Summary.ConceptualModules} | {report.Summary.ConceptualPercentage:F1}% |");
                markdown.AppendLine($"| **Total** | **{report.Summary.TotalModules}** | **100.0%** |");
                markdown.AppendLine();
            }
        }
        
        private void AppendCompilerFlagsSection(StringBuilder markdown, DevStateReport report)
        {
            markdown.AppendLine("## Compiler Flags Status");
            markdown.AppendLine();
            
            if (report.CompilerFlags != null && report.CompilerFlags.Any())
            {
                foreach (var flag in report.CompilerFlags.OrderBy(f => f.Key.ToString()))
                {
                    var status = flag.Value ? "✅ ENABLED" : "❌ DISABLED";
                    var impact = flag.Value ? "Features available" : "Features compiled out";
                    markdown.AppendLine($"- **{flag.Key}**: {status} - {impact}");
                }
            }
            else
            {
                markdown.AppendLine("*No compiler flags configured.*");
            }
            
            markdown.AppendLine();
        }
        
        private void AppendDetailedModulesSection(StringBuilder markdown, DevStateReport report, MarkdownFormatOptions options)
        {
            markdown.AppendLine("## Module Status");
            markdown.AppendLine();
            
            if (report.Modules != null && report.Modules.Any())
            {
                // Group modules by state for better organization
                var moduleGroups = report.Modules.GroupBy(m => m.State).OrderBy(g => g.Key);
                
                foreach (var group in moduleGroups)
                {
                    var stateIcon = group.Key switch
                    {
                        ModuleState.Implemented => "✅",
                        ModuleState.Disabled => "❌",
                        ModuleState.Conceptual => "🔬",
                        _ => "❓"
                    };
                    
                    markdown.AppendLine($"### {stateIcon} {group.Key} Modules ({group.Count()})");
                    markdown.AppendLine();
                    
                    foreach (var module in group.OrderBy(m => m.ModuleName))
                    {
                        markdown.AppendLine($"#### {module.ModuleName}");
                        markdown.AppendLine();
                        markdown.AppendLine($"- **Assembly**: {module.AssemblyName}");
                        markdown.AppendLine($"- **State**: {module.State}");
                        
                        if (module.Dependencies != null && module.Dependencies.Any())
                        {
                            markdown.AppendLine($"- **Dependencies**: {string.Join(", ", module.Dependencies)}");
                        }
                        
                        if (module.DefineConstraints != null && module.DefineConstraints.Any())
                        {
                            markdown.AppendLine($"- **Compiler Constraints**: {string.Join(", ", module.DefineConstraints)}");
                        }
                        
                        if (!string.IsNullOrEmpty(module.Description))
                        {
                            markdown.AppendLine($"- **Description**: {module.Description}");
                        }
                        
                        if (module.Evidence != null && module.Evidence.Any())
                        {
                            markdown.AppendLine($"- **Evidence Files**: {module.Evidence.Count}");
                        }
                        
                        markdown.AppendLine($"- **Last Validated**: {module.LastValidated:yyyy-MM-dd HH:mm:ss}");
                        markdown.AppendLine();
                    }
                }
            }
            else
            {
                markdown.AppendLine("*No modules found.*");
                markdown.AppendLine();
            }
        }
        
        private void AppendDependencyAnalysisSection(StringBuilder markdown, DevStateReport report)
        {
            markdown.AppendLine("## Dependency Analysis");
            markdown.AppendLine();
            
            if (report.Modules != null && report.Modules.Any())
            {
                // Create dependency graph
                var dependencies = new Dictionary<string, List<string>>();
                foreach (var module in report.Modules)
                {
                    dependencies[module.ModuleName] = module.Dependencies ?? new List<string>();
                }
                
                // Find modules with no dependencies (root modules)
                var rootModules = dependencies.Where(kvp => !kvp.Value.Any()).Select(kvp => kvp.Key).ToList();
                
                // Find modules with most dependencies
                var mostDependencies = dependencies.OrderByDescending(kvp => kvp.Value.Count).Take(5);
                
                markdown.AppendLine("### Root Modules (No Dependencies)");
                markdown.AppendLine();
                foreach (var root in rootModules)
                {
                    markdown.AppendLine($"- {root}");
                }
                markdown.AppendLine();
                
                markdown.AppendLine("### Modules with Most Dependencies");
                markdown.AppendLine();
                markdown.AppendLine("| Module | Dependency Count | Dependencies |");
                markdown.AppendLine("|--------|------------------|--------------|");
                foreach (var dep in mostDependencies)
                {
                    var depList = string.Join(", ", dep.Value);
                    markdown.AppendLine($"| {dep.Key} | {dep.Value.Count} | {depList} |");
                }
                markdown.AppendLine();
            }
        }
        
        private void AppendPerformanceMetricsSection(StringBuilder markdown, DevStateReport report)
        {
            markdown.AppendLine("## Performance Metrics");
            markdown.AppendLine();
            
            if (report.CurrentPerformance != null)
            {
                markdown.AppendLine("### System Performance");
                markdown.AppendLine();
                markdown.AppendLine("| Metric | Value | Status |");
                markdown.AppendLine("|--------|-------|--------|");
                
                var fpsStatus = report.CurrentPerformance.AverageFPS >= 60 ? "✅ Good" : "⚠️ Below Target";
                markdown.AppendLine($"| Average FPS | {report.CurrentPerformance.AverageFPS:F1} | {fpsStatus} |");
                
                var frameTimeStatus = report.CurrentPerformance.AverageFrameTime <= 16.67f ? "✅ Good" : "⚠️ Above Target";
                markdown.AppendLine($"| Frame Time | {report.CurrentPerformance.AverageFrameTime:F2}ms | {frameTimeStatus} |");
                
                markdown.AppendLine($"| Memory Usage | {report.CurrentPerformance.MemoryUsage / (1024 * 1024):F1} MB | - |");
                markdown.AppendLine($"| CPU Usage | {report.CurrentPerformance.CPUUsage:F1}% | - |");
                markdown.AppendLine();
                
                if (report.CurrentPerformance.AdditionalMetrics != null && report.CurrentPerformance.AdditionalMetrics.Any())
                {
                    markdown.AppendLine("### Additional Metrics");
                    markdown.AppendLine();
                    foreach (var metric in report.CurrentPerformance.AdditionalMetrics)
                    {
                        markdown.AppendLine($"- **{metric.Key}**: {metric.Value}");
                    }
                    markdown.AppendLine();
                }
            }
            else
            {
                markdown.AppendLine("*No performance metrics available.*");
                markdown.AppendLine();
            }
        }
        
        private void AppendEvidenceSection(StringBuilder markdown, DevStateReport report, MarkdownFormatOptions options)
        {
            markdown.AppendLine("## Supporting Evidence");
            markdown.AppendLine();
            
            if (report.SupportingEvidence != null && report.SupportingEvidence.Any())
            {
                var evidenceByType = report.SupportingEvidence.GroupBy(e => e.Type);
                
                foreach (var group in evidenceByType.OrderBy(g => g.Key.ToString()))
                {
                    markdown.AppendLine($"### {group.Key} ({group.Count()} files)");
                    markdown.AppendLine();
                    
                    var evidenceToShow = group.Take(options.MaxEvidencePerType);
                    
                    foreach (var evidence in evidenceToShow.OrderBy(e => e.CollectedAt))
                    {
                        var validationStatus = evidence.IsValid ? "✅ Valid" : "❌ Invalid";
                        
                        markdown.AppendLine($"#### {evidence.Description}");
                        markdown.AppendLine();
                        markdown.AppendLine($"- **Status**: {validationStatus}");
                        markdown.AppendLine($"- **Module**: {evidence.ModuleName}");
                        markdown.AppendLine($"- **File**: `{Path.GetFileName(evidence.FilePath)}`");
                        markdown.AppendLine($"- **Size**: {FormatBytes(evidence.FileSizeBytes)}");
                        markdown.AppendLine($"- **Collected**: {evidence.CollectedAt:yyyy-MM-dd HH:mm:ss}");
                        
                        if (evidence.Tags != null && evidence.Tags.Any())
                        {
                            markdown.AppendLine($"- **Tags**: {string.Join(", ", evidence.Tags)}");
                        }
                        
                        markdown.AppendLine($"- **Hash**: `{evidence.Hash?.Substring(0, Math.Min(16, evidence.Hash.Length ?? 0))}...`");
                        markdown.AppendLine();
                    }
                    
                    if (group.Count() > options.MaxEvidencePerType)
                    {
                        markdown.AppendLine($"*... and {group.Count() - options.MaxEvidencePerType} more files*");
                        markdown.AppendLine();
                    }
                }
            }
            else
            {
                markdown.AppendLine("*No evidence files found.*");
                markdown.AppendLine();
            }
        }
        
        private void AppendValidationWarningsSection(StringBuilder markdown, DevStateReport report)
        {
            var validation = ValidateReport(report);
            
            if (validation.Errors.Any() || validation.Warnings.Any())
            {
                markdown.AppendLine("## Validation Warnings");
                markdown.AppendLine();
                
                if (validation.Errors.Any())
                {
                    markdown.AppendLine("### Errors");
                    markdown.AppendLine();
                    foreach (var error in validation.Errors)
                    {
                        markdown.AppendLine($"- ❌ {error}");
                    }
                    markdown.AppendLine();
                }
                
                if (validation.Warnings.Any())
                {
                    markdown.AppendLine("### Warnings");
                    markdown.AppendLine();
                    foreach (var warning in validation.Warnings)
                    {
                        markdown.AppendLine($"- ⚠️ {warning}");
                    }
                    markdown.AppendLine();
                }
            }
        }
        
        private void AppendFooterSection(StringBuilder markdown, DevStateReport report)
        {
            markdown.AppendLine("---");
            markdown.AppendLine();
            markdown.AppendLine("*This report was automatically generated by the XR Bubble Library Development State Generator.*");
            markdown.AppendLine($"*Generated on {Environment.MachineName} by {Environment.UserName}*");
            markdown.AppendLine($"*Report format version: 2.0*");
            markdown.AppendLine();
            markdown.AppendLine("For more information:");
            markdown.AppendLine("- [Development State Documentation System](UnityProject/Assets/XRBubbleLibrary/Core/README_DevStateGenerator.md)");
            markdown.AppendLine("- [Evidence Collection System](UnityProject/Assets/XRBubbleLibrary/Core/README_EvidenceCollector.md)");
            markdown.AppendLine("- [Report Formatting System](UnityProject/Assets/XRBubbleLibrary/Core/README_ReportFormatter.md)");
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
        
        private string EscapeCsvField(string field, CsvFormatOptions options)
        {
            if (string.IsNullOrEmpty(field))
                return "";
            
            if (field.Contains(options.Delimiter) || field.Contains(options.QuoteCharacter) || field.Contains('\n') || field.Contains('\r'))
            {
                return $"{options.QuoteCharacter}{field.Replace(options.QuoteCharacter.ToString(), $"{options.QuoteCharacter}{options.QuoteCharacter}")}{options.QuoteCharacter}";
            }
            
            return field;
        }
        
        // Additional helper methods for HTML, JSON, and comparison reports would be implemented here
        // For brevity, I'm including stubs for these methods
        
        private void AppendHtmlStyles(StringBuilder html, HtmlFormatOptions options)
        {
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 0; padding: 20px; }");
            html.AppendLine(".container { max-width: 1200px; margin: 0 auto; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; }");
            html.AppendLine("</style>");
        }
        
        private void AppendHtmlNavigation(StringBuilder html, HtmlFormatOptions options)
        {
            html.AppendLine("<nav>");
            html.AppendLine("<ul>");
            html.AppendLine("<li><a href=\"#summary\">Summary</a></li>");
            html.AppendLine("<li><a href=\"#modules\">Modules</a></li>");
            html.AppendLine("<li><a href=\"#performance\">Performance</a></li>");
            html.AppendLine("</ul>");
            html.AppendLine("</nav>");
        }
        
        private void AppendHtmlJavaScript(StringBuilder html, HtmlFormatOptions options)
        {
            html.AppendLine("<script>");
            html.AppendLine("// Interactive features would be implemented here");
            html.AppendLine("</script>");
        }
        
        private string ConvertMarkdownToHtml(string markdown)
        {
            // Basic markdown to HTML conversion
            // In a full implementation, this would use a proper markdown parser
            return markdown
                .Replace("# ", "<h1>").Replace("\n", "</h1>\n")
                .Replace("## ", "<h2>").Replace("\n", "</h2>\n")
                .Replace("### ", "<h3>").Replace("\n", "</h3>\n")
                .Replace("**", "<strong>").Replace("**", "</strong>")
                .Replace("*", "<em>").Replace("*", "</em>");
        }
        
        private string GenerateMarkdownSummary(DevStateReport report)
        {
            var summary = new StringBuilder();
            summary.AppendLine("# Development State Summary");
            summary.AppendLine();
            summary.AppendLine($"**Generated**: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
            summary.AppendLine($"**Build**: {report.BuildVersion}");
            summary.AppendLine();
            
            if (report.Summary != null)
            {
                summary.AppendLine($"- **Total Modules**: {report.Summary.TotalModules}");
                summary.AppendLine($"- **Implemented**: {report.Summary.ImplementedModules} ({report.Summary.ImplementedPercentage:F1}%)");
                summary.AppendLine($"- **Disabled**: {report.Summary.DisabledModules} ({report.Summary.DisabledPercentage:F1}%)");
                summary.AppendLine($"- **Conceptual**: {report.Summary.ConceptualModules} ({report.Summary.ConceptualPercentage:F1}%)");
            }
            
            return summary.ToString();
        }
        
        private string GenerateHtmlSummary(DevStateReport report)
        {
            return $"<div class=\"summary\">{GenerateMarkdownSummary(report)}</div>";
        }
        
        private string GenerateJsonSummary(DevStateReport report)
        {
            var summary = new
            {
                generatedAt = report.GeneratedAt,
                buildVersion = report.BuildVersion,
                summary = report.Summary
            };
            
            return JsonUtility.ToJson(summary, true);
        }
        
        private string GeneratePlainTextSummary(DevStateReport report)
        {
            return GenerateMarkdownSummary(report).Replace("#", "").Replace("**", "").Replace("*", "");
        }
        
        private void GenerateMarkdownComparison(StringBuilder comparison, DevStateReport previousReport, DevStateReport currentReport)
        {
            comparison.AppendLine("# Development State Comparison Report");
            comparison.AppendLine();
            comparison.AppendLine($"**Previous Report**: {previousReport.GeneratedAt:yyyy-MM-dd HH:mm:ss} (Build {previousReport.BuildVersion})");
            comparison.AppendLine($"**Current Report**: {currentReport.GeneratedAt:yyyy-MM-dd HH:mm:ss} (Build {currentReport.BuildVersion})");
            comparison.AppendLine();
            
            // Compare summaries
            if (previousReport.Summary != null && currentReport.Summary != null)
            {
                comparison.AppendLine("## Summary Changes");
                comparison.AppendLine();
                
                var implementedChange = currentReport.Summary.ImplementedModules - previousReport.Summary.ImplementedModules;
                var disabledChange = currentReport.Summary.DisabledModules - previousReport.Summary.DisabledModules;
                var conceptualChange = currentReport.Summary.ConceptualModules - previousReport.Summary.ConceptualModules;
                
                comparison.AppendLine($"- **Implemented**: {previousReport.Summary.ImplementedModules} → {currentReport.Summary.ImplementedModules} ({implementedChange:+0;-0;0})");
                comparison.AppendLine($"- **Disabled**: {previousReport.Summary.DisabledModules} → {currentReport.Summary.DisabledModules} ({disabledChange:+0;-0;0})");
                comparison.AppendLine($"- **Conceptual**: {previousReport.Summary.ConceptualModules} → {currentReport.Summary.ConceptualModules} ({conceptualChange:+0;-0;0})");
                comparison.AppendLine();
            }
        }
        
        private void GenerateHtmlComparison(StringBuilder comparison, DevStateReport previousReport, DevStateReport currentReport)
        {
            comparison.AppendLine("<div class=\"comparison\">");
            GenerateMarkdownComparison(comparison, previousReport, currentReport);
            comparison.AppendLine("</div>");
        }
        
        private void GenerateJsonComparison(StringBuilder comparison, DevStateReport previousReport, DevStateReport currentReport)
        {
            var comparisonData = new
            {
                previousReport = new { generatedAt = previousReport.GeneratedAt, buildVersion = previousReport.BuildVersion, summary = previousReport.Summary },
                currentReport = new { generatedAt = currentReport.GeneratedAt, buildVersion = currentReport.BuildVersion, summary = currentReport.Summary },
                changes = new
                {
                    implementedChange = (currentReport.Summary?.ImplementedModules ?? 0) - (previousReport.Summary?.ImplementedModules ?? 0),
                    disabledChange = (currentReport.Summary?.DisabledModules ?? 0) - (previousReport.Summary?.DisabledModules ?? 0),
                    conceptualChange = (currentReport.Summary?.ConceptualModules ?? 0) - (previousReport.Summary?.ConceptualModules ?? 0)
                }
            };
            
            comparison.AppendLine(JsonUtility.ToJson(comparisonData, true));
        }
        
        #endregion
    }
}