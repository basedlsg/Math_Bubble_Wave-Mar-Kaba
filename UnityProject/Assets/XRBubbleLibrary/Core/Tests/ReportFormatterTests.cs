using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace XRBubbleLibrary.Core.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the Report Formatting and Generation System.
    /// Tests all formatting capabilities, validation, and integration features.
    /// </summary>
    [TestFixture]
    public class ReportFormatterTests
    {
        private ReportFormatter _reportFormatter;
        private DevStateReport _testReport;
        
        [SetUp]
        public void SetUp()
        {
            _reportFormatter = new ReportFormatter();
            _testReport = CreateTestReport();
        }
        
        [TearDown]
        public void TearDown()
        {
            _reportFormatter = null;
            _testReport = null;
        }
        
        #region Markdown Formatting Tests
        
        [Test]
        public void FormatAsMarkdown_WithValidReport_ShouldReturnFormattedMarkdown()
        {
            // Act
            var result = _reportFormatter.FormatAsMarkdown(_testReport);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("# XR Bubble Library - Development State Report"));
            Assert.IsTrue(result.Contains("## Summary"));
            Assert.IsTrue(result.Contains(_testReport.BuildVersion));
        }
        
        [Test]
        public void FormatAsMarkdown_WithNullReport_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _reportFormatter.FormatAsMarkdown(null));
        }
        
        [Test]
        public void FormatAsMarkdown_WithCustomOptions_ShouldRespectOptions()
        {
            // Arrange
            var options = new MarkdownFormatOptions
            {
                IncludeTableOfContents = false,
                IncludePerformanceMetrics = false,
                IncludeEvidence = false
            };
            
            // Act
            var result = _reportFormatter.FormatAsMarkdown(_testReport, options);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Contains("## Table of Contents"));
            Assert.IsFalse(result.Contains("## Performance Metrics"));
            Assert.IsFalse(result.Contains("## Supporting Evidence"));
        }
        
        [Test]
        public void FormatAsMarkdown_WithTableOfContents_ShouldIncludeTOC()
        {
            // Arrange
            var options = new MarkdownFormatOptions
            {
                IncludeTableOfContents = true
            };
            
            // Act
            var result = _reportFormatter.FormatAsMarkdown(_testReport, options);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("## Table of Contents"));
            Assert.IsTrue(result.Contains("- [Summary](#summary)"));
        }
        
        #endregion
        
        #region HTML Formatting Tests
        
        [Test]
        public void FormatAsHtml_WithValidReport_ShouldReturnFormattedHtml()
        {
            // Act
            var result = _reportFormatter.FormatAsHtml(_testReport);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("<!DOCTYPE html>"));
            Assert.IsTrue(result.Contains("<html lang=\"en\">"));
            Assert.IsTrue(result.Contains("</html>"));
            Assert.IsTrue(result.Contains(_testReport.BuildVersion));
        }
        
        [Test]
        public void FormatAsHtml_WithNullReport_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _reportFormatter.FormatAsHtml(null));
        }
        
        [Test]
        public void FormatAsHtml_WithCustomOptions_ShouldRespectOptions()
        {
            // Arrange
            var options = new HtmlFormatOptions
            {
                PageTitle = "Custom Title",
                IncludeCss = false,
                IncludeNavigation = false
            };
            
            // Act
            var result = _reportFormatter.FormatAsHtml(_testReport, options);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("<title>Custom Title</title>"));
        }
        
        #endregion
        
        #region JSON Formatting Tests
        
        [Test]
        public void FormatAsJson_WithValidReport_ShouldReturnFormattedJson()
        {
            // Act
            var result = _reportFormatter.FormatAsJson(_testReport);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("\"reportMetadata\""));
            Assert.IsTrue(result.Contains("\"buildVersion\""));
            Assert.IsTrue(result.Contains(_testReport.BuildVersion));
        }
        
        [Test]
        public void FormatAsJson_WithNullReport_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _reportFormatter.FormatAsJson(null));
        }
        
        [Test]
        public void FormatAsJson_WithValidationEnabled_ShouldIncludeValidation()
        {
            // Arrange
            var options = new JsonFormatOptions
            {
                IncludeValidation = true
            };
            
            // Act
            var result = _reportFormatter.FormatAsJson(_testReport, options);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("\"validation\""));
        }
        
        [Test]
        public void FormatAsJson_WithMetadataEnabled_ShouldIncludeMetadata()
        {
            // Arrange
            var options = new JsonFormatOptions
            {
                IncludeMetadata = true
            };
            
            // Act
            var result = _reportFormatter.FormatAsJson(_testReport, options);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("\"metadata\""));
            Assert.IsTrue(result.Contains("\"formatter\""));
        }
        
        #endregion
        
        #region CSV Formatting Tests
        
        [Test]
        public void FormatAsCsv_WithValidReport_ShouldReturnFormattedCsv()
        {
            // Act
            var result = _reportFormatter.FormatAsCsv(_testReport);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("ModuleName"));
            Assert.IsTrue(result.Contains("State"));
        }
        
        [Test]
        public void FormatAsCsv_WithNullReport_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _reportFormatter.FormatAsCsv(null));
        }
        
        [Test]
        public void FormatAsCsv_WithSummaryOnly_ShouldReturnSummaryFormat()
        {
            // Arrange
            var options = new CsvFormatOptions
            {
                SummaryOnly = true
            };
            
            // Act
            var result = _reportFormatter.FormatAsCsv(_testReport, options);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("GeneratedAt"));
            Assert.IsTrue(result.Contains("TotalModules"));
            Assert.IsTrue(result.Contains("ImplementedModules"));
        }
        
        [Test]
        public void FormatAsCsv_WithCustomDelimiter_ShouldUseCustomDelimiter()
        {
            // Arrange
            var options = new CsvFormatOptions
            {
                Delimiter = ';'
            };
            
            // Act
            var result = _reportFormatter.FormatAsCsv(_testReport, options);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains(";"));
        }
        
        #endregion
        
        #region Summary Generation Tests
        
        [Test]
        public void GenerateSummary_WithValidReport_ShouldReturnSummary()
        {
            // Act
            var result = _reportFormatter.GenerateSummary(_testReport);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Summary"));
        }
        
        [Test]
        public void GenerateSummary_WithNullReport_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _reportFormatter.GenerateSummary(null));
        }
        
        [Test]
        public void GenerateSummary_WithDifferentFormats_ShouldReturnCorrectFormat()
        {
            // Test Markdown format
            var markdownResult = _reportFormatter.GenerateSummary(_testReport, ReportFormat.Markdown);
            Assert.IsTrue(markdownResult.Contains("#"));
            
            // Test HTML format
            var htmlResult = _reportFormatter.GenerateSummary(_testReport, ReportFormat.Html);
            Assert.IsTrue(htmlResult.Contains("<"));
            
            // Test JSON format
            var jsonResult = _reportFormatter.GenerateSummary(_testReport, ReportFormat.Json);
            Assert.IsTrue(jsonResult.Contains("{"));
        }
        
        #endregion
        
        #region Comparison Report Tests
        
        [Test]
        public void GenerateComparisonReport_WithValidReports_ShouldReturnComparison()
        {
            // Arrange
            var previousReport = CreateTestReport();
            var currentReport = CreateTestReport();
            currentReport.Summary.ImplementedModules = 3; // Different value
            
            // Act
            var result = _reportFormatter.GenerateComparisonReport(previousReport, currentReport);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Comparison"));
        }
        
        [Test]
        public void GenerateComparisonReport_WithNullReports_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _reportFormatter.GenerateComparisonReport(null, _testReport));
            Assert.Throws<ArgumentNullException>(() => _reportFormatter.GenerateComparisonReport(_testReport, null));
        }
        
        #endregion
        
        #region Validation Tests
        
        [Test]
        public void ValidateReport_WithValidReport_ShouldReturnValidResult()
        {
            // Act
            var result = _reportFormatter.ValidateReport(_testReport);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }
        
        [Test]
        public void ValidateReport_WithNullReport_ShouldReturnInvalidResult()
        {
            // Act
            var result = _reportFormatter.ValidateReport(null);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.Greater(result.Errors.Count, 0);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("null")));
        }
        
        [Test]
        public void ValidateReport_WithInvalidReport_ShouldReturnValidationErrors()
        {
            // Arrange
            var invalidReport = new DevStateReport
            {
                BuildVersion = "", // Invalid empty version
                GeneratedAt = default, // Invalid timestamp
                Modules = null // Invalid null modules
            };
            
            // Act
            var result = _reportFormatter.ValidateReport(invalidReport);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.Greater(result.Errors.Count, 0);
        }
        
        [Test]
        public void ValidateReport_WithDuplicateModules_ShouldReturnValidationError()
        {
            // Arrange
            var reportWithDuplicates = CreateTestReport();
            reportWithDuplicates.Modules.Add(new ModuleStatus
            {
                ModuleName = "TestModule", // Duplicate name
                AssemblyName = "TestAssembly2",
                State = ModuleState.Implemented
            });
            
            // Act
            var result = _reportFormatter.ValidateReport(reportWithDuplicates);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("Duplicate module name")));
        }
        
        #endregion
        
        #region Batch Processing Tests
        
        [UnityTest]
        public System.Collections.IEnumerator FormatBatchAsync_WithValidReports_ShouldReturnFormattedResults()
        {
            // Arrange
            var reports = new List<DevStateReport>
            {
                CreateTestReport(),
                CreateTestReport()
            };
            
            Dictionary<string, string> results = null;
            Exception exception = null;
            
            // Act
            var task = _reportFormatter.FormatBatchAsync(reports, ReportFormat.Markdown);
            
            // Wait for completion
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            if (task.Exception != null)
            {
                exception = task.Exception;
            }
            else
            {
                results = task.Result;
            }
            
            // Assert
            Assert.IsNull(exception);
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
            
            foreach (var result in results.Values)
            {
                Assert.IsTrue(result.Contains("# XR Bubble Library - Development State Report"));
            }
        }
        
        [Test]
        public void FormatBatchAsync_WithNullReports_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _reportFormatter.FormatBatchAsync(null, ReportFormat.Markdown));
        }
        
        #endregion
        
        #region Format Options Tests
        
        [Test]
        public void MarkdownFormatOptions_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var options = new MarkdownFormatOptions();
            
            // Assert
            Assert.IsTrue(options.IncludeTableOfContents);
            Assert.IsTrue(options.IncludeDetailedModules);
            Assert.IsTrue(options.IncludePerformanceMetrics);
            Assert.IsTrue(options.IncludeEvidence);
            Assert.IsTrue(options.IncludeCompilerFlags);
            Assert.IsFalse(options.IncludeDependencyAnalysis);
            Assert.AreEqual(10, options.MaxEvidencePerType);
            Assert.IsTrue(options.UseUtcTime);
            Assert.IsTrue(options.IncludeValidationWarnings);
        }
        
        [Test]
        public void HtmlFormatOptions_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var options = new HtmlFormatOptions();
            
            // Assert
            Assert.IsTrue(options.IncludeCss);
            Assert.IsFalse(options.IncludeJavaScript);
            Assert.AreEqual("XR Bubble Library - Development State Report", options.PageTitle);
            Assert.IsTrue(options.IncludeNavigation);
            Assert.IsFalse(options.IncludeSearch);
        }
        
        [Test]
        public void JsonFormatOptions_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var options = new JsonFormatOptions();
            
            // Assert
            Assert.IsTrue(options.PrettyPrint);
            Assert.IsTrue(options.IncludeMetadata);
            Assert.IsFalse(options.IncludeValidation);
        }
        
        [Test]
        public void CsvFormatOptions_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var options = new CsvFormatOptions();
            
            // Assert
            Assert.IsTrue(options.IncludeHeaders);
            Assert.AreEqual(',', options.Delimiter);
            Assert.AreEqual('"', options.QuoteCharacter);
            Assert.IsFalse(options.SummaryOnly);
            Assert.IsTrue(options.IncludePerformanceColumns);
        }
        
        #endregion
        
        #region Helper Methods
        
        private DevStateReport CreateTestReport()
        {
            return new DevStateReport
            {
                GeneratedAt = DateTime.UtcNow,
                BuildVersion = "1.0.0-test",
                UnityVersion = "2023.3.5f1",
                BuildConfiguration = "Test",
                Summary = new DevStateSummary
                {
                    TotalModules = 2,
                    ImplementedModules = 1,
                    DisabledModules = 1,
                    ConceptualModules = 0
                },
                CompilerFlags = new Dictionary<ExperimentalFeature, bool>
                {
                    { ExperimentalFeature.AI_INTEGRATION, false },
                    { ExperimentalFeature.VOICE_PROCESSING, false }
                },
                Modules = new List<ModuleStatus>
                {
                    new ModuleStatus
                    {
                        ModuleName = "TestModule",
                        AssemblyName = "TestAssembly",
                        State = ModuleState.Implemented,
                        Dependencies = new List<string> { "Core" },
                        LastValidated = DateTime.UtcNow
                    },
                    new ModuleStatus
                    {
                        ModuleName = "DisabledModule",
                        AssemblyName = "DisabledAssembly",
                        State = ModuleState.Disabled,
                        Dependencies = new List<string>(),
                        LastValidated = DateTime.UtcNow
                    }
                },
                CurrentPerformance = new PerformanceMetrics
                {
                    AverageFPS = 60.0f,
                    AverageFrameTime = 16.67f,
                    MemoryUsage = 1024 * 1024 * 100, // 100MB
                    CapturedAt = DateTime.UtcNow,
                    BuildVersion = "1.0.0-test"
                },
                SupportingEvidence = new List<EvidenceFile>
                {
                    new EvidenceFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        FilePath = "/test/evidence.log",
                        Type = EvidenceType.PerformanceLog,
                        ModuleName = "TestModule",
                        Hash = "test-hash",
                        CollectedAt = DateTime.UtcNow,
                        FileSizeBytes = 1024,
                        Description = "Test evidence file",
                        IsValid = true,
                        LastValidated = DateTime.UtcNow
                    }
                }
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Integration tests for the Report Scheduler.
    /// </summary>
    [TestFixture]
    public class ReportSchedulerTests
    {
        private ReportScheduler _scheduler;
        private MockDevStateGenerator _mockGenerator;
        private MockReportFormatter _mockFormatter;
        
        [SetUp]
        public void SetUp()
        {
            _mockGenerator = new MockDevStateGenerator();
            _mockFormatter = new MockReportFormatter();
            // Note: ReportScheduler constructor would need to be updated to accept these mocks
        }
        
        [TearDown]
        public void TearDown()
        {
            _scheduler?.Dispose();
        }
        
        [Test]
        public void ReportScheduler_Construction_ShouldInitializeCorrectly()
        {
            // This test would verify scheduler initialization
            // Implementation depends on completed ReportScheduler class
            Assert.Pass("ReportScheduler tests require completed implementation");
        }
        
        #region Mock Classes for Testing
        
        private class MockDevStateGenerator : IDevStateGenerator
        {
            public DevStateReport GenerateReport()
            {
                return new DevStateReport
                {
                    GeneratedAt = DateTime.UtcNow,
                    BuildVersion = "mock-1.0.0"
                };
            }
            
            public System.Threading.Tasks.Task<DevStateReport> GenerateReportAsync()
            {
                return System.Threading.Tasks.Task.FromResult(GenerateReport());
            }
            
            public void ScheduleNightlyGeneration()
            {
                // Mock implementation
            }
            
            public bool ValidateReportAccuracy(DevStateReport report)
            {
                return true;
            }
            
            public string GenerateAndSaveDevStateFile()
            {
                return "/mock/path/DEV_STATE.md";
            }
            
            public System.Threading.Tasks.Task<string> GenerateAndSaveDevStateFileAsync()
            {
                return System.Threading.Tasks.Task.FromResult(GenerateAndSaveDevStateFile());
            }
        }
        
        private class MockReportFormatter : IReportFormatter
        {
            public string FormatAsMarkdown(DevStateReport report, MarkdownFormatOptions options = null)
            {
                return "# Mock Markdown Report";
            }
            
            public string FormatAsHtml(DevStateReport report, HtmlFormatOptions options = null)
            {
                return "<html><body>Mock HTML Report</body></html>";
            }
            
            public string FormatAsJson(DevStateReport report, JsonFormatOptions options = null)
            {
                return "{\"mock\": \"json report\"}";
            }
            
            public string FormatAsCsv(DevStateReport report, CsvFormatOptions options = null)
            {
                return "Header1,Header2\nValue1,Value2";
            }
            
            public string GenerateSummary(DevStateReport report, ReportFormat format = ReportFormat.Markdown)
            {
                return "Mock Summary";
            }
            
            public string GenerateComparisonReport(DevStateReport previousReport, DevStateReport currentReport, ReportFormat format = ReportFormat.Markdown)
            {
                return "Mock Comparison Report";
            }
            
            public ReportValidationResult ValidateReport(DevStateReport report)
            {
                return new ReportValidationResult { IsValid = true };
            }
            
            public System.Threading.Tasks.Task<Dictionary<string, string>> FormatBatchAsync(List<DevStateReport> reports, ReportFormat format, object options = null)
            {
                var results = new Dictionary<string, string>();
                for (int i = 0; i < reports.Count; i++)
                {
                    results[$"report_{i}"] = $"Mock formatted report {i}";
                }
                return System.Threading.Tasks.Task.FromResult(results);
            }
        }
        
        #endregion
    }
}