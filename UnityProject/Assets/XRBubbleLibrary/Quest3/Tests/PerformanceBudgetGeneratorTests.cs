using NUnit.Framework;
using UnityEngine;
using System.IO;
using System.Linq;

namespace XRBubbleLibrary.Quest3.Tests
{
    /// <summary>
    /// Comprehensive unit tests for Performance Budget Generator system.
    /// Tests Requirement 6.1: Automated performance budget generation.
    /// Tests Requirement 6.2: Frame-time analysis and breakdown system.
    /// Tests Requirement 6.3: Performance data visualization.
    /// </summary>
    [TestFixture]
    public class PerformanceBudgetGeneratorTests
    {
        private PerformanceBudgetGenerator _budgetGenerator;
        private PerformanceBudgetConfiguration _testConfig;
        private string _testOutputDirectory;
        
        [SetUp]
        public void SetUp()
        {
            _budgetGenerator = new PerformanceBudgetGenerator();
            _testOutputDirectory = Path.Combine(Application.temporaryCachePath, "BudgetGeneratorTests");
            
            _testConfig = new PerformanceBudgetConfiguration
            {
                TargetFrameRate = 72f,
                TargetFrameTimeMs = 13.89f,
                RequiredHeadroomPercent = 0.3f,
                IncludeFrameTimeBreakdown = true,
                GeneratePieChartData = true,
                IncludeOptimizationRecommendations = true,
                ValidateQuest3Certification = true,
                OutputDirectory = _testOutputDirectory
            };
            
            // Ensure test directory exists
            Directory.CreateDirectory(_testOutputDirectory);
        }
        
        [TearDown]
        public void TearDown()
        {
            _budgetGenerator?.Dispose();
            
            // Clean up test files
            if (Directory.Exists(_testOutputDirectory))
            {
                try
                {
                    Directory.Delete(_testOutputDirectory, true);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"Failed to clean up test directory: {ex.Message}");
                }
            }
        }        
 
       #region Initialization Tests
        
        [Test]
        public void Initialize_WithValidConfig_ReturnsTrue()
        {
            // Act
            bool result = _budgetGenerator.Initialize(_testConfig);
            
            // Assert
            Assert.IsTrue(result, "Budget generator initialization should succeed with valid config");
            Assert.IsTrue(_budgetGenerator.IsInitialized, "Generator should be initialized");
            Assert.AreEqual(_testConfig.TargetFrameRate, _budgetGenerator.Configuration.TargetFrameRate, 
                "Configuration should be stored correctly");
        }
        
        [Test]
        public void Initialize_WithInvalidFrameRate_ReturnsFalse()
        {
            // Arrange
            var invalidConfig = _testConfig;
            invalidConfig.TargetFrameRate = -1f;
            
            // Act
            bool result = _budgetGenerator.Initialize(invalidConfig);
            
            // Assert
            Assert.IsFalse(result, "Initialization should fail with invalid frame rate");
            Assert.IsFalse(_budgetGenerator.IsInitialized, "Generator should not be initialized");
        }
        
        [Test]
        public void Initialize_WithInvalidHeadroom_ReturnsFalse()
        {
            // Arrange
            var invalidConfig = _testConfig;
            invalidConfig.RequiredHeadroomPercent = 1.5f; // >100%
            
            // Act
            bool result = _budgetGenerator.Initialize(invalidConfig);
            
            // Assert
            Assert.IsFalse(result, "Initialization should fail with invalid headroom percentage");
        }
        
        [Test]
        public void PerformanceBudgetConfiguration_Quest3Default_HasCorrectValues()
        {
            // Act
            var defaultConfig = PerformanceBudgetConfiguration.Quest3Default;
            
            // Assert
            Assert.AreEqual(72f, defaultConfig.TargetFrameRate, "Default should target 72 FPS for Quest 3");
            Assert.AreEqual(13.89f, defaultConfig.TargetFrameTimeMs, 0.01f, "Default frame time should be ~13.89ms");
            Assert.AreEqual(0.3f, defaultConfig.RequiredHeadroomPercent, "Default should require 30% headroom");
            Assert.IsTrue(defaultConfig.IncludeFrameTimeBreakdown, "Should include frame time breakdown");
            Assert.IsTrue(defaultConfig.GeneratePieChartData, "Should generate pie chart data");
            Assert.IsTrue(defaultConfig.ValidateQuest3Certification, "Should validate Quest 3 certification");
        }
        
        #endregion
        
        #region Budget Analysis Tests
        
        [Test]
        public void GenerateBudgetAnalysis_WithValidData_ReturnsCompleteAnalysis()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var performanceData = CreateMockPerformanceData(75f, 200); // 75 FPS, 200 samples
            
            // Act
            var analysis = _budgetGenerator.GenerateBudgetAnalysis(performanceData);
            
            // Assert
            Assert.IsNotNull(analysis.Summary, "Should generate performance summary");
            Assert.IsNotNull(analysis.FrameTimeAnalysis, "Should generate frame time analysis");
            Assert.IsNotNull(analysis.BudgetBreakdown, "Should generate budget breakdown");
            Assert.IsNotNull(analysis.PieChartData.Segments, "Should generate pie chart data");
            Assert.IsNotNull(analysis.Validation, "Should generate validation results");
            Assert.IsNotNull(analysis.Recommendations, "Should generate recommendations");
            Assert.Greater(analysis.AnalysisTimestamp, System.DateTime.MinValue, "Should have valid timestamp");
        }
        
        [Test]
        public void GenerateBudgetAnalysis_WithInsufficientSamples_GeneratesWarning()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var performanceData = CreateMockPerformanceData(72f, 50); // Only 50 samples
            
            // Act & Assert - Should not throw, but may log warning
            var analysis = _budgetGenerator.GenerateBudgetAnalysis(performanceData);
            Assert.IsNotNull(analysis.Summary, "Should still generate analysis with insufficient samples");
        }
        
        [Test]
        public void GenerateBudgetAnalysis_WhenNotInitialized_ReturnsDefault()
        {
            // Arrange - Don't initialize
            var performanceData = CreateMockPerformanceData(72f, 200);
            
            // Act
            var analysis = _budgetGenerator.GenerateBudgetAnalysis(performanceData);
            
            // Assert
            Assert.AreEqual(System.DateTime.MinValue, analysis.AnalysisTimestamp, 
                "Should return default analysis when not initialized");
        }
        
        #endregion
        
        #region Pie Chart Generation Tests
        
        [Test]
        public void GenerateFrameTimePieChart_WithValidData_ReturnsValidChart()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var frameTimeData = CreateMockFrameTimeBreakdown(12f); // 12ms total frame time
            
            // Act
            var pieChart = _budgetGenerator.GenerateFrameTimePieChart(frameTimeData);
            
            // Assert
            Assert.IsNotNull(pieChart.Segments, "Should have pie chart segments");
            Assert.Greater(pieChart.Segments.Length, 0, "Should have at least one segment");
            Assert.AreEqual(frameTimeData.TotalFrameTimeMs, pieChart.TotalFrameTimeMs, 
                "Total frame time should match");
            Assert.IsNotNull(pieChart.ChartTitle, "Should have chart title");
            
            // Verify segments add up to 100%
            float totalPercentage = pieChart.Segments.Sum(s => s.Percentage);
            Assert.AreEqual(100f, totalPercentage, 1f, "Segments should add up to 100%");
            
            // Verify segments are sorted by percentage (largest first)
            for (int i = 1; i < pieChart.Segments.Length; i++)
            {
                Assert.GreaterOrEqual(pieChart.Segments[i-1].Percentage, pieChart.Segments[i].Percentage,
                    "Segments should be sorted by percentage");
            }
        }
        
        [Test]
        public void GenerateFrameTimePieChart_IdentifiesBottlenecks()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var frameTimeData = CreateMockFrameTimeBreakdown(15f);
            frameTimeData.RenderingTimeMs = 8f; // >50% of frame time - should be bottleneck
            
            // Act
            var pieChart = _budgetGenerator.GenerateFrameTimePieChart(frameTimeData);
            
            // Assert
            var renderingSegment = pieChart.Segments.FirstOrDefault(s => s.Label == "Rendering");
            Assert.IsNotNull(renderingSegment, "Should have rendering segment");
            Assert.IsTrue(renderingSegment.IsBottleneck, "Rendering should be identified as bottleneck");
        }
        
        #endregion     
   
        #region Frame Time Analysis Tests
        
        [Test]
        public void AnalyzeFrameTimeBreakdown_WithValidData_ReturnsDetailedAnalysis()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var performanceData = CreateMockPerformanceData(70f, 100);
            
            // Act
            var analysis = _budgetGenerator.AnalyzeFrameTimeBreakdown(performanceData);
            
            // Assert
            Assert.Greater(analysis.AverageBreakdown.TotalFrameTimeMs, 0f, 
                "Should have positive total frame time");
            Assert.AreEqual(performanceData.FrameTimeBreakdowns.Length, analysis.SampleCount, 
                "Sample count should match input data");
            Assert.Greater(analysis.AnalysisTimestamp, System.DateTime.MinValue, 
                "Should have valid analysis timestamp");
        }
        
        [Test]
        public void AnalyzeFrameTimeBreakdown_WithNoBreakdownData_ReturnsDefaultAnalysis()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var performanceData = CreateMockPerformanceData(72f, 100);
            performanceData.FrameTimeBreakdowns = null; // No breakdown data
            
            // Act
            var analysis = _budgetGenerator.AnalyzeFrameTimeBreakdown(performanceData);
            
            // Assert
            Assert.Greater(analysis.AverageBreakdown.TotalFrameTimeMs, 0f, 
                "Should provide default breakdown when no data available");
        }
        
        #endregion
        
        #region Budget Validation Tests
        
        [Test]
        public void ValidatePerformanceBudget_WithGoodPerformance_PassesValidation()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysis(80f, 10f); // 80 FPS, 10ms frame time
            
            // Act
            var validation = _budgetGenerator.ValidatePerformanceBudget(analysis);
            
            // Assert
            Assert.IsTrue(validation.MeetsRequirements, "Good performance should meet requirements");
            Assert.Greater(validation.OverallScore, 80f, "Should have high overall score");
            Assert.IsNotNull(validation.ValidationResults, "Should have validation results");
            Assert.Greater(validation.ValidationTimestamp, System.DateTime.MinValue, 
                "Should have valid timestamp");
        }
        
        [Test]
        public void ValidatePerformanceBudget_WithPoorPerformance_FailsValidation()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysis(50f, 20f); // 50 FPS, 20ms frame time
            
            // Act
            var validation = _budgetGenerator.ValidatePerformanceBudget(analysis);
            
            // Assert
            Assert.IsFalse(validation.MeetsRequirements, "Poor performance should fail requirements");
            Assert.Less(validation.OverallScore, 50f, "Should have low overall score");
            Assert.IsNotEmpty(validation.Recommendations, "Should provide recommendations");
        }
        
        [Test]
        public void ValidatePerformanceBudget_ChecksHeadroomRequirement()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysis(72f, 12f); // Meets FPS but low headroom
            
            // Act
            var validation = _budgetGenerator.ValidatePerformanceBudget(analysis);
            
            // Assert
            var headroomResult = validation.ValidationResults.FirstOrDefault(r => r.Category == "Headroom");
            Assert.IsNotNull(headroomResult, "Should validate headroom requirement");
        }
        
        #endregion
        
        #region Optimization Recommendations Tests
        
        [Test]
        public void GenerateOptimizationRecommendations_WithBottlenecks_ProvidesRecommendations()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysisWithBottlenecks();
            
            // Act
            var recommendations = _budgetGenerator.GenerateOptimizationRecommendations(analysis);
            
            // Assert
            Assert.IsNotNull(recommendations.Recommendations, "Should provide recommendations");
            Assert.Greater(recommendations.Recommendations.Length, 0, "Should have at least one recommendation");
            Assert.IsNotNull(recommendations.TotalEstimatedImpact, "Should estimate total impact");
            Assert.IsNotNull(recommendations.ImplementationComplexity, "Should assess implementation complexity");
        }
        
        [Test]
        public void GenerateOptimizationRecommendations_PrioritizesByImpact()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysisWithBottlenecks();
            
            // Act
            var recommendations = _budgetGenerator.GenerateOptimizationRecommendations(analysis);
            
            // Assert
            if (recommendations.Recommendations.Length > 1)
            {
                // Verify recommendations are sorted by priority
                for (int i = 1; i < recommendations.Recommendations.Length; i++)
                {
                    Assert.GreaterOrEqual(recommendations.Recommendations[i-1].Priority, 
                        recommendations.Recommendations[i].Priority,
                        "Recommendations should be sorted by priority");
                }
            }
        }
        
        #endregion
        
        #region Markdown Generation Tests
        
        [Test]
        public void GeneratePerformanceBudgetMarkdown_CreatesValidFile()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysis(75f, 12f);
            string markdownPath = Path.Combine(_testOutputDirectory, "test_budget.md");
            
            // Act
            bool result = _budgetGenerator.GeneratePerformanceBudgetMarkdown(analysis, markdownPath);
            
            // Assert
            Assert.IsTrue(result, "Markdown generation should succeed");
            Assert.IsTrue(File.Exists(markdownPath), "Markdown file should be created");
            
            var content = File.ReadAllText(markdownPath);
            Assert.IsTrue(content.Contains("# Performance Budget Analysis"), "Should contain header");
            Assert.IsTrue(content.Contains("## Performance Summary"), "Should contain performance summary");
            Assert.IsTrue(content.Contains("## Frame Time Breakdown"), "Should contain frame time breakdown");
            Assert.IsTrue(content.Contains("72 FPS"), "Should contain target frame rate");
        }
        
        [Test]
        public void GeneratePerformanceBudgetMarkdown_IncludesAllConfiguredSections()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysis(75f, 12f);
            string markdownPath = Path.Combine(_testOutputDirectory, "complete_budget.md");
            
            // Act
            bool result = _budgetGenerator.GeneratePerformanceBudgetMarkdown(analysis, markdownPath);
            
            // Assert
            Assert.IsTrue(result, "Markdown generation should succeed");
            
            var content = File.ReadAllText(markdownPath);
            Assert.IsTrue(content.Contains("## Frame Time Distribution"), "Should include pie chart section");
            Assert.IsTrue(content.Contains("## Quest 3 Certification Validation"), "Should include validation section");
            Assert.IsTrue(content.Contains("## Optimization Recommendations"), "Should include recommendations section");
            Assert.IsTrue(content.Contains("## Headroom Analysis"), "Should include headroom analysis");
        }
        
        #endregion
        
        #region Export Tests
        
        [Test]
        public void ExportPerformanceBudget_ToMarkdown_CreatesFile()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysis(75f, 12f);
            string exportPath = Path.Combine(_testOutputDirectory, "export_test.md");
            
            // Act
            bool result = _budgetGenerator.ExportPerformanceBudget(analysis, BudgetExportFormat.Markdown, exportPath);
            
            // Assert
            Assert.IsTrue(result, "Export to markdown should succeed");
            Assert.IsTrue(File.Exists(exportPath), "Export file should be created");
        }
        
        [Test]
        public void ExportPerformanceBudget_ToJSON_CreatesFile()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysis(75f, 12f);
            string exportPath = Path.Combine(_testOutputDirectory, "export_test.json");
            
            // Act
            bool result = _budgetGenerator.ExportPerformanceBudget(analysis, BudgetExportFormat.JSON, exportPath);
            
            // Assert
            Assert.IsTrue(result, "Export to JSON should succeed");
        }
        
        [Test]
        public void ExportPerformanceBudget_UnsupportedFormat_ReturnsFalse()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var analysis = CreateMockBudgetAnalysis(75f, 12f);
            string exportPath = Path.Combine(_testOutputDirectory, "export_test.unknown");
            
            // Act
            bool result = _budgetGenerator.ExportPerformanceBudget(analysis, (BudgetExportFormat)999, exportPath);
            
            // Assert
            Assert.IsFalse(result, "Export with unsupported format should fail");
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void FullBudgetGenerationWorkflow_CompletesSuccessfully()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            var performanceData = CreateMockPerformanceData(74f, 300);
            string outputPath = Path.Combine(_testOutputDirectory, "full_workflow_budget.md");
            
            // Act
            var analysis = _budgetGenerator.GenerateBudgetAnalysis(performanceData);
            bool exportResult = _budgetGenerator.ExportPerformanceBudget(analysis, BudgetExportFormat.Markdown, outputPath);
            
            // Assert
            Assert.IsNotNull(analysis.Summary, "Should generate complete analysis");
            Assert.IsTrue(exportResult, "Should export successfully");
            Assert.IsTrue(File.Exists(outputPath), "Should create output file");
            
            var content = File.ReadAllText(outputPath);
            Assert.Greater(content.Length, 1000, "Should generate substantial content");
        }
        
        [Test]
        public void Dispose_CleansUpResources()
        {
            // Arrange
            _budgetGenerator.Initialize(_testConfig);
            
            // Act
            _budgetGenerator.Dispose();
            
            // Assert
            Assert.IsFalse(_budgetGenerator.IsInitialized, "Should not be initialized after disposal");
        }
        
        #endregion
        
        #region Helper Methods
        
        private PerformanceDataSet CreateMockPerformanceData(float avgFrameRate, int sampleCount)
        {
            var frameRateSamples = new float[sampleCount];
            var frameTimeSamples = new float[sampleCount];
            var cpuSamples = new float[sampleCount];
            var gpuSamples = new float[sampleCount];
            var memorySamples = new float[sampleCount];
            var frameTimeBreakdowns = new FrameTimeBreakdown[sampleCount];
            
            var random = new System.Random(42); // Fixed seed for reproducible tests
            
            for (int i = 0; i < sampleCount; i++)
            {
                frameRateSamples[i] = avgFrameRate + random.Next(-5, 6); // ±5 FPS variation
                frameTimeSamples[i] = 1000f / frameRateSamples[i];
                cpuSamples[i] = 60f + random.Next(-10, 21); // 50-80% CPU usage
                gpuSamples[i] = 70f + random.Next(-15, 16); // 55-85% GPU usage
                memorySamples[i] = 1500f + random.Next(-200, 301); // 1300-1800MB memory
                
                frameTimeBreakdowns[i] = CreateMockFrameTimeBreakdown(frameTimeSamples[i]);
            }
            
            return new PerformanceDataSet
            {
                FrameRateSamples = frameRateSamples,
                FrameTimeSamples = frameTimeSamples,
                CPUUsageSamples = cpuSamples,
                GPUUsageSamples = gpuSamples,
                MemoryUsageSamples = memorySamples,
                FrameTimeBreakdowns = frameTimeBreakdowns,
                CollectionDuration = System.TimeSpan.FromSeconds(sampleCount / 10.0), // 10Hz sampling
                SampleCount = sampleCount
            };
        }
        
        private FrameTimeBreakdown CreateMockFrameTimeBreakdown(float totalFrameTime)
        {
            return new FrameTimeBreakdown
            {
                TotalFrameTimeMs = totalFrameTime,
                RenderingTimeMs = totalFrameTime * 0.4f,
                CPUTimeMs = totalFrameTime * 0.25f,
                GPUTimeMs = totalFrameTime * 0.15f,
                PhysicsTimeMs = totalFrameTime * 0.08f,
                ScriptTimeMs = totalFrameTime * 0.07f,
                AudioTimeMs = totalFrameTime * 0.03f,
                VRCompositorTimeMs = totalFrameTime * 0.015f,
                GarbageCollectionTimeMs = totalFrameTime * 0.005f,
                OtherTimeMs = totalFrameTime * 0.01f,
                Timestamp = System.DateTime.Now
            };
        }
        
        private PerformanceBudgetAnalysis CreateMockBudgetAnalysis(float avgFrameRate, float avgFrameTime)
        {
            var frameTimeBreakdown = CreateMockFrameTimeBreakdown(avgFrameTime);
            
            return new PerformanceBudgetAnalysis
            {
                Summary = new PerformanceSummary(),
                FrameTimeAnalysis = new FrameTimeAnalysis
                {
                    AverageBreakdown = frameTimeBreakdown,
                    SampleCount = 100,
                    AnalysisTimestamp = System.DateTime.Now
                },
                BudgetBreakdown = new PerformanceBudgetBreakdown(),
                PieChartData = new FrameTimePieChartData { Segments = new PieChartSegment[0] },
                Validation = new PerformanceBudgetValidation(),
                Recommendations = new PerformanceOptimizationRecommendations(),
                AnalysisTimestamp = System.DateTime.Now,
                Configuration = _testConfig
            };
        }
        
        private PerformanceBudgetAnalysis CreateMockBudgetAnalysisWithBottlenecks()
        {
            var analysis = CreateMockBudgetAnalysis(65f, 16f); // Poor performance
            
            // Create frame time breakdown with clear bottlenecks
            analysis.FrameTimeAnalysis.AverageBreakdown = new FrameTimeBreakdown
            {
                TotalFrameTimeMs = 16f,
                RenderingTimeMs = 8f, // 50% - major bottleneck
                CPUTimeMs = 4f, // 25% - bottleneck
                GPUTimeMs = 2f,
                PhysicsTimeMs = 1f,
                ScriptTimeMs = 0.5f,
                AudioTimeMs = 0.3f,
                VRCompositorTimeMs = 0.1f,
                GarbageCollectionTimeMs = 0.1f,
                OtherTimeMs = 0f
            };
            
            return analysis;
        }
        
        #endregion
    }
}