using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for formatting development state reports into various output formats.
    /// Provides advanced formatting capabilities beyond the basic ToMarkdown method.
    /// </summary>
    public interface IReportFormatter
    {
        /// <summary>
        /// Formats a development state report as Markdown with enhanced formatting.
        /// </summary>
        /// <param name="report">The report to format</param>
        /// <param name="options">Formatting options</param>
        /// <returns>Formatted Markdown string</returns>
        string FormatAsMarkdown(DevStateReport report, MarkdownFormatOptions options = null);
        
        /// <summary>
        /// Formats a development state report as HTML.
        /// </summary>
        /// <param name="report">The report to format</param>
        /// <param name="options">HTML formatting options</param>
        /// <returns>Formatted HTML string</returns>
        string FormatAsHtml(DevStateReport report, HtmlFormatOptions options = null);
        
        /// <summary>
        /// Formats a development state report as JSON with enhanced structure.
        /// </summary>
        /// <param name="report">The report to format</param>
        /// <param name="options">JSON formatting options</param>
        /// <returns>Formatted JSON string</returns>
        string FormatAsJson(DevStateReport report, JsonFormatOptions options = null);
        
        /// <summary>
        /// Formats a development state report as CSV for data analysis.
        /// </summary>
        /// <param name="report">The report to format</param>
        /// <param name="options">CSV formatting options</param>
        /// <returns>Formatted CSV string</returns>
        string FormatAsCsv(DevStateReport report, CsvFormatOptions options = null);
        
        /// <summary>
        /// Generates a summary report with key metrics and status.
        /// </summary>
        /// <param name="report">The report to summarize</param>
        /// <param name="format">Output format for the summary</param>
        /// <returns>Formatted summary string</returns>
        string GenerateSummary(DevStateReport report, ReportFormat format = ReportFormat.Markdown);
        
        /// <summary>
        /// Generates a comparison report between two development state reports.
        /// </summary>
        /// <param name="previousReport">Previous report for comparison</param>
        /// <param name="currentReport">Current report for comparison</param>
        /// <param name="format">Output format for the comparison</param>
        /// <returns>Formatted comparison report</returns>
        string GenerateComparisonReport(DevStateReport previousReport, DevStateReport currentReport, ReportFormat format = ReportFormat.Markdown);
        
        /// <summary>
        /// Validates that a report can be formatted without errors.
        /// </summary>
        /// <param name="report">The report to validate</param>
        /// <returns>Validation result with any issues found</returns>
        ReportValidationResult ValidateReport(DevStateReport report);
        
        /// <summary>
        /// Formats multiple reports as a batch operation.
        /// </summary>
        /// <param name="reports">Reports to format</param>
        /// <param name="format">Output format</param>
        /// <param name="options">Formatting options</param>
        /// <returns>Dictionary of report IDs to formatted content</returns>
        Task<Dictionary<string, string>> FormatBatchAsync(List<DevStateReport> reports, ReportFormat format, object options = null);
    }
    
    /// <summary>
    /// Supported report output formats.
    /// </summary>
    public enum ReportFormat
    {
        Markdown,
        Html,
        Json,
        Csv,
        PlainText
    }
    
    /// <summary>
    /// Options for Markdown formatting.
    /// </summary>
    public class MarkdownFormatOptions
    {
        /// <summary>
        /// Include table of contents.
        /// </summary>
        public bool IncludeTableOfContents { get; set; } = true;
        
        /// <summary>
        /// Include detailed module information.
        /// </summary>
        public bool IncludeDetailedModules { get; set; } = true;
        
        /// <summary>
        /// Include performance metrics section.
        /// </summary>
        public bool IncludePerformanceMetrics { get; set; } = true;
        
        /// <summary>
        /// Include evidence files section.
        /// </summary>
        public bool IncludeEvidence { get; set; } = true;
        
        /// <summary>
        /// Include compiler flags section.
        /// </summary>
        public bool IncludeCompilerFlags { get; set; } = true;
        
        /// <summary>
        /// Include dependency analysis.
        /// </summary>
        public bool IncludeDependencyAnalysis { get; set; } = false;
        
        /// <summary>
        /// Maximum number of evidence files to include per type.
        /// </summary>
        public int MaxEvidencePerType { get; set; } = 10;
        
        /// <summary>
        /// Custom CSS styles for enhanced formatting.
        /// </summary>
        public string CustomStyles { get; set; }
        
        /// <summary>
        /// Include timestamps in UTC or local time.
        /// </summary>
        public bool UseUtcTime { get; set; } = true;
        
        /// <summary>
        /// Include validation warnings in the report.
        /// </summary>
        public bool IncludeValidationWarnings { get; set; } = true;
    }
    
    /// <summary>
    /// Options for HTML formatting.
    /// </summary>
    public class HtmlFormatOptions
    {
        /// <summary>
        /// Include CSS styling in the HTML.
        /// </summary>
        public bool IncludeCss { get; set; } = true;
        
        /// <summary>
        /// Include JavaScript for interactive features.
        /// </summary>
        public bool IncludeJavaScript { get; set; } = false;
        
        /// <summary>
        /// Custom CSS file path to include.
        /// </summary>
        public string CustomCssPath { get; set; }
        
        /// <summary>
        /// Page title for the HTML document.
        /// </summary>
        public string PageTitle { get; set; } = "XR Bubble Library - Development State Report";
        
        /// <summary>
        /// Include navigation menu.
        /// </summary>
        public bool IncludeNavigation { get; set; } = true;
        
        /// <summary>
        /// Include search functionality.
        /// </summary>
        public bool IncludeSearch { get; set; } = false;
    }
    
    /// <summary>
    /// Options for JSON formatting.
    /// </summary>
    public class JsonFormatOptions
    {
        /// <summary>
        /// Pretty print the JSON output.
        /// </summary>
        public bool PrettyPrint { get; set; } = true;
        
        /// <summary>
        /// Include metadata in the JSON output.
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;
        
        /// <summary>
        /// Include validation results in the JSON.
        /// </summary>
        public bool IncludeValidation { get; set; } = false;
        
        /// <summary>
        /// Custom JSON schema to validate against.
        /// </summary>
        public string SchemaPath { get; set; }
    }
    
    /// <summary>
    /// Options for CSV formatting.
    /// </summary>
    public class CsvFormatOptions
    {
        /// <summary>
        /// Include header row in CSV output.
        /// </summary>
        public bool IncludeHeaders { get; set; } = true;
        
        /// <summary>
        /// CSV delimiter character.
        /// </summary>
        public char Delimiter { get; set; } = ',';
        
        /// <summary>
        /// Quote character for CSV fields.
        /// </summary>
        public char QuoteCharacter { get; set; } = '"';
        
        /// <summary>
        /// Include only summary data or detailed module information.
        /// </summary>
        public bool SummaryOnly { get; set; } = false;
        
        /// <summary>
        /// Include performance metrics in separate columns.
        /// </summary>
        public bool IncludePerformanceColumns { get; set; } = true;
    }
    
    /// <summary>
    /// Result of report validation.
    /// </summary>
    public class ReportValidationResult
    {
        /// <summary>
        /// Whether the report is valid for formatting.
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// List of validation errors found.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// List of validation warnings.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
        
        /// <summary>
        /// Timestamp when validation was performed.
        /// </summary>
        public DateTime ValidatedAt { get; set; }
        
        /// <summary>
        /// Additional validation metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}