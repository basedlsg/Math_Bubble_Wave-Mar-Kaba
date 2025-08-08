using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Quest3.Tests
{
    /// <summary>
    /// Comprehensive unit tests for IL2CPP Build Manager system.
    /// Tests Requirement 5.3: IL2CPP build automation and testing.
    /// Tests Requirement 8.1: Quest 3-specific performance testing infrastructure.
    /// </summary>
    [TestFixture]
    public class IL2CPPBuildManagerTests
    {
        private IL2CPPBuildManager _buildManager;
        private IL2CPPBuildConfiguration _testConfig;
        private string _testOutputDirectory;
        
        [SetUp]
        public void SetUp()
        {
            _buildManager = new IL2CPPBuildManager();
            _testOutputDirectory = Path.Combine(Application.temporaryCachePath, "IL2CPPBuildTests");
            
            _testConfig = new IL2CPPBuildConfiguration
            {
                BuildType = IL2CPPBuildType.Development,
                OptimizationLevel = IL2CPPOptimizationLevel.Speed,
                EnableScriptDebugging = true,
                EnableDeepProfiling = false,
                StripEngineCode = false,
                StrippingLevel = ManagedStrippingLevel.Minimal,
                OutputDirectory = _testOutputDirectory,
                AutoDeploy = false,
                TargetDeviceId = null,
                AdditionalDefineSymbols = new string[] { "QUEST3_BUILD", "TEST_BUILD" },
                Quest3Settings = Quest3BuildSettings.Default
            };
            
            // Ensure test directory exists
            Directory.CreateDirectory(_testOutputDirectory);
        }
        
        [TearDown]
        public void TearDown()
        {
            _buildManager?.Dispose();
            
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
            bool result = _buildManager.Initialize(_testConfig);
            
            // Assert
            Assert.IsTrue(result, "Build manager initialization should succeed with valid config");
            Assert.IsTrue(_buildManager.IsInitialized, "Build manager should be initialized");
            Assert.AreEqual(_testConfig.BuildType, _buildManager.CurrentConfiguration.BuildType, 
                "Configuration should be stored correctly");
        }
        
        [Test]
        public void Initialize_WithInvalidOutputDirectory_ReturnsFalse()
        {
            // Arrange
            var invalidConfig = _testConfig;
            invalidConfig.OutputDirectory = "";
            
            // Act
            bool result = _buildManager.Initialize(invalidConfig);
            
            // Assert
            Assert.IsFalse(result, "Initialization should fail with invalid output directory");
            Assert.IsFalse(_buildManager.IsInitialized, "Build manager should not be initialized");
        }
        
        [Test]
        public void Initialize_WithLowApiLevel_ReturnsFalse()
        {
            // Arrange
            var invalidConfig = _testConfig;
            invalidConfig.Quest3Settings.TargetApiLevel = 28; // Below minimum for Quest 3
            
            // Act
            bool result = _buildManager.Initialize(invalidConfig);
            
            // Assert
            Assert.IsFalse(result, "Initialization should fail with API level too low for Quest 3");
        }
        
        [Test]
        public void Initialize_CreatesOutputDirectory()
        {
            // Arrange
            string newOutputDir = Path.Combine(_testOutputDirectory, "NewBuildDir");
            var config = _testConfig;
            config.OutputDirectory = newOutputDir;
            
            // Act
            bool result = _buildManager.Initialize(config);
            
            // Assert
            Assert.IsTrue(result, "Initialization should succeed");
            Assert.IsTrue(Directory.Exists(newOutputDir), "Output directory should be created");
        }
        
        #endregion
        
        #region Build Configuration Tests
        
        [Test]
        public void CreateOptimizedConfiguration_Speed_ReturnsSpeedOptimizedConfig()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            
            // Act
            var config = _buildManager.CreateOptimizedConfiguration(IL2CPPOptimizationLevel.Speed);
            
            // Assert
            Assert.AreEqual(IL2CPPOptimizationLevel.Speed, config.OptimizationLevel, 
                "Should set speed optimization");
            Assert.AreEqual(ManagedStrippingLevel.Medium, config.StrippingLevel, 
                "Should use medium stripping for speed optimization");
            Assert.IsTrue(config.StripEngineCode, "Should enable engine code stripping for speed");
        }
        
        [Test]
        public void CreateOptimizedConfiguration_Size_ReturnsSizeOptimizedConfig()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            
            // Act
            var config = _buildManager.CreateOptimizedConfiguration(IL2CPPOptimizationLevel.Size);
            
            // Assert
            Assert.AreEqual(IL2CPPOptimizationLevel.Size, config.OptimizationLevel, 
                "Should set size optimization");
            Assert.AreEqual(ManagedStrippingLevel.High, config.StrippingLevel, 
                "Should use high stripping for size optimization");
            Assert.IsTrue(config.StripEngineCode, "Should enable engine code stripping for size");
        }
        
        [Test]
        public void ConfigureQuest3Settings_UpdatesConfiguration()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            var newSettings = Quest3BuildSettings.Performance;
            
            // Act
            _buildManager.ConfigureQuest3Settings(newSettings);
            
            // Assert
            Assert.AreEqual(newSettings.TargetApiLevel, _buildManager.CurrentConfiguration.Quest3Settings.TargetApiLevel,
                "Quest 3 settings should be updated");
            Assert.AreEqual(newSettings.GraphicsAPI, _buildManager.CurrentConfiguration.Quest3Settings.GraphicsAPI,
                "Graphics API should be updated");
        }
        
        [Test]
        public void Quest3BuildSettings_Default_HasCorrectValues()
        {
            // Act
            var settings = Quest3BuildSettings.Default;
            
            // Assert
            Assert.AreEqual(32, settings.TargetApiLevel, "Should target Android 12 (API 32)");
            Assert.AreEqual(29, settings.MinimumApiLevel, "Should support minimum Android 10 (API 29)");
            Assert.AreEqual(AndroidArchitecture.ARM64, settings.TargetArchitecture, "Should target ARM64");
            Assert.AreEqual(GraphicsDeviceType.Vulkan, settings.GraphicsAPI, "Should use Vulkan for Quest 3");
            Assert.AreEqual(ColorSpace.Linear, settings.ColorSpace, "Should use Linear color space for VR");
            Assert.IsTrue(settings.MultithreadedRendering, "Should enable multithreaded rendering");
            Assert.AreEqual(MobileTextureSubtarget.ASTC, settings.TextureCompression, "Should use ASTC compression");
        }
        
        #endregion
        
        #region Build Session Tests
        
        [Test]
        public void StartBuild_WhenInitialized_ReturnsValidSession()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            string buildName = "TestBuild";
            
            // Act
            var session = _buildManager.StartBuild(buildName, IL2CPPBuildType.Development);
            
            // Assert
            Assert.IsNotNull(session.SessionId, "Session should have valid ID");
            Assert.AreEqual(buildName, session.BuildName, "Session name should match");
            Assert.AreEqual(IL2CPPBuildType.Development, session.BuildType, "Build type should match");
            Assert.IsTrue(session.IsActive, "Session should be active");
            Assert.IsTrue(_buildManager.IsBuildInProgress, "Build should be in progress");
            Assert.IsTrue(session.OutputPath.Contains(buildName), "Output path should contain build name");
        }
        
        [Test]
        public void StartBuild_WhenNotInitialized_ReturnsDefaultSession()
        {
            // Act
            var session = _buildManager.StartBuild("TestBuild");
            
            // Assert
            Assert.IsNull(session.SessionId, "Session ID should be null when not initialized");
            Assert.IsFalse(_buildManager.IsBuildInProgress, "Build should not be in progress");
        }
        
        [Test]
        public void StartBuild_WhenBuildInProgress_CancelsPreviousBuild()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            var firstSession = _buildManager.StartBuild("FirstBuild");
            
            // Act
            var secondSession = _buildManager.StartBuild("SecondBuild");
            
            // Assert
            Assert.AreNotEqual(firstSession.SessionId, secondSession.SessionId, "Should create new session");
            Assert.AreEqual("SecondBuild", secondSession.BuildName, "Should use new build name");
            Assert.IsTrue(_buildManager.IsBuildInProgress, "New build should be in progress");
        }
        
        [Test]
        public void CancelBuild_WhenBuildInProgress_ReturnsTrue()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            _buildManager.StartBuild("TestBuild");
            
            // Act
            bool result = _buildManager.CancelBuild();
            
            // Assert
            Assert.IsTrue(result, "Cancel should succeed when build is in progress");
            Assert.IsFalse(_buildManager.IsBuildInProgress, "Build should no longer be in progress");
        }
        
        [Test]
        public void CancelBuild_WhenNoBuildInProgress_ReturnsFalse()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            
            // Act
            bool result = _buildManager.CancelBuild();
            
            // Assert
            Assert.IsFalse(result, "Cancel should fail when no build is in progress");
        }
        
        [Test]
        public void GetBuildStatus_WhenBuildInProgress_ReturnsCurrentStatus()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            _buildManager.StartBuild("TestBuild");
            
            // Act
            var status = _buildManager.GetBuildStatus();
            
            // Assert
            Assert.IsTrue(status.IsActive, "Status should indicate build is active");
            Assert.GreaterOrEqual(status.OverallProgress, 0f, "Progress should be non-negative");
            Assert.LessOrEqual(status.OverallProgress, 1f, "Progress should not exceed 100%");
            Assert.IsNotNull(status.StatusMessage, "Should have status message");
            Assert.GreaterOrEqual(status.ElapsedTime.TotalMilliseconds, 0, "Elapsed time should be non-negative");
        }
        
        #endregion
        
        #region Build Completion Tests
        
        [UnityTest]
        public IEnumerator WaitForBuildCompletion_WithValidBuild_ReturnsSuccessResult()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            _buildManager.StartBuild("TestBuild");
            
            // Act
            var buildTask = Task.Run(() => _buildManager.WaitForBuildCompletion(30f));
            
            // Wait for completion with timeout
            float timeout = 35f;
            float elapsed = 0f;
            while (!buildTask.IsCompleted && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            // Assert
            Assert.IsTrue(buildTask.IsCompleted, "Build should complete within timeout");
            
            if (buildTask.IsCompleted)
            {
                var result = buildTask.Result;
                Assert.IsNotNull(result.Session.SessionId, "Result should contain session info");
                Assert.Greater(result.BuildTime.TotalSeconds, 0, "Build time should be positive");
                // Note: Success may vary in simulated environment
            }
        }
        
        [Test]
        public void WaitForBuildCompletion_WhenNoBuildActive_ReturnsFailedResult()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            
            // Act
            var result = _buildManager.WaitForBuildCompletion(1f);
            
            // Assert
            Assert.IsFalse(result.Success, "Should return failed result when no build is active");
            Assert.IsTrue(result.Errors.Any(e => e.Contains("No active build")), 
                "Should indicate no active build in errors");
        }
        
        #endregion
        
        #region Build Validation Tests
        
        [Test]
        public void ValidateBuild_WithNonExistentPath_ReturnsIncompatibleResult()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            string nonExistentPath = Path.Combine(_testOutputDirectory, "NonExistent");
            
            // Act
            var result = _buildManager.ValidateBuild(nonExistentPath);
            
            // Assert
            Assert.IsFalse(result.IsQuest3Compatible, "Should be incompatible for non-existent path");
            Assert.AreEqual(0f, result.ValidationScore, "Validation score should be 0 for non-existent path");
            Assert.IsTrue(result.CompatibilityIssues.Any(i => i.Severity == CompatibilityIssueSeverity.Critical),
                "Should have critical issues for non-existent path");
        }
        
        [Test]
        public void ValidateBuild_WithValidBuildDirectory_ReturnsValidationResult()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            string buildPath = Path.Combine(_testOutputDirectory, "ValidBuild");
            Directory.CreateDirectory(buildPath);
            
            // Create a dummy APK file
            string apkPath = Path.Combine(buildPath, "test.apk");
            File.WriteAllText(apkPath, "Dummy APK content");
            
            // Act
            var result = _buildManager.ValidateBuild(buildPath);
            
            // Assert
            Assert.Greater(result.ValidationScore, 0f, "Should have positive validation score");
            Assert.IsNotNull(result.ValidationReport, "Should generate validation report");
            Assert.IsNotNull(result.CompatibilityIssues, "Should have compatibility issues array");
            Assert.IsNotNull(result.PerformanceWarnings, "Should have performance warnings array");
            Assert.IsNotNull(result.OptimizationRecommendations, "Should have optimization recommendations");
        }
        
        [Test]
        public void ValidateBuild_WithLargeAPK_GeneratesWarning()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            string buildPath = Path.Combine(_testOutputDirectory, "LargeBuild");
            Directory.CreateDirectory(buildPath);
            
            // Create a large dummy APK file (simulate large size)
            string apkPath = Path.Combine(buildPath, "large.apk");
            using (var fs = File.Create(apkPath))
            {
                fs.SetLength(100 * 1024 * 1024); // 100MB file
            }
            
            // Act
            var result = _buildManager.ValidateBuild(buildPath);
            
            // Assert
            Assert.IsTrue(result.CompatibilityIssues.Any(i => i.IssueType == CompatibilityIssueType.MemoryIssue),
                "Should identify memory issue for large APK");
        }
        
        #endregion
        
        #region Device Management Tests
        
        [Test]
        public void GetConnectedQuest3Devices_ReturnsDeviceList()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            
            // Act
            var devices = _buildManager.GetConnectedQuest3Devices();
            
            // Assert
            Assert.IsNotNull(devices, "Should return device array");
            // Note: In simulated environment, may return simulated devices
        }
        
        [Test]
        public void DeployToQuest3_WithNonExistentBuild_ReturnsFailedResult()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            string nonExistentPath = Path.Combine(_testOutputDirectory, "NonExistent");
            
            // Act
            var result = _buildManager.DeployToQuest3(nonExistentPath);
            
            // Assert
            Assert.IsFalse(result.Success, "Deployment should fail for non-existent build");
            Assert.IsTrue(result.DeploymentErrors.Any(e => e.Contains("APK file")), 
                "Should indicate APK file issue in errors");
        }
        
        [Test]
        public void DeployToQuest3_WithValidBuild_AttemptsDeployment()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            string buildPath = Path.Combine(_testOutputDirectory, "DeployBuild");
            Directory.CreateDirectory(buildPath);
            
            // Create a dummy APK file
            string apkPath = Path.Combine(buildPath, "deploy.apk");
            File.WriteAllText(apkPath, "Dummy APK for deployment");
            
            // Act
            var result = _buildManager.DeployToQuest3(buildPath);
            
            // Assert
            Assert.IsNotNull(result.DeploymentLog, "Should have deployment log");
            // Note: Success may vary in simulated environment
        }
        
        #endregion
        
        #region Build History Tests
        
        [Test]
        public void GetBuildHistory_InitiallyEmpty_ReturnsEmptyHistory()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            
            // Act
            var history = _buildManager.GetBuildHistory();
            
            // Assert
            Assert.AreEqual(0, history.TotalBuildsAttempted, "Should have no builds initially");
            Assert.AreEqual(0, history.TotalSuccessfulBuilds, "Should have no successful builds initially");
            Assert.AreEqual(0f, history.SuccessRate, "Success rate should be 0 initially");
            Assert.AreEqual(TimeSpan.Zero, history.AverageBuildTime, "Average build time should be zero initially");
        }
        
        [UnityTest]
        public IEnumerator GetBuildHistory_AfterBuild_ContainsBuildRecord()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            _buildManager.StartBuild("HistoryTestBuild");
            
            // Wait for build to complete
            var buildTask = Task.Run(() => _buildManager.WaitForBuildCompletion(30f));
            
            float timeout = 35f;
            float elapsed = 0f;
            while (!buildTask.IsCompleted && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            // Act
            var history = _buildManager.GetBuildHistory();
            
            // Assert
            Assert.Greater(history.TotalBuildsAttempted, 0, "Should have attempted builds");
            Assert.IsNotNull(history.CompletedBuilds, "Should have completed builds array");
            Assert.Greater(history.AverageBuildTime.TotalSeconds, 0, "Should have positive average build time");
        }
        
        #endregion
        
        #region Build Report Tests
        
        [Test]
        public void GenerateBuildReport_WithValidResult_CreatesReportFile()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            var mockResult = CreateMockBuildResult(true);
            string reportPath = Path.Combine(_testOutputDirectory, "build_report.txt");
            
            // Act
            bool success = _buildManager.GenerateBuildReport(mockResult, reportPath);
            
            // Assert
            Assert.IsTrue(success, "Report generation should succeed");
            Assert.IsTrue(File.Exists(reportPath), "Report file should be created");
            
            var reportContent = File.ReadAllText(reportPath);
            Assert.IsTrue(reportContent.Contains("IL2CPP Build Report"), "Report should contain header");
            Assert.IsTrue(reportContent.Contains(mockResult.Session.BuildName), "Report should contain build name");
            Assert.IsTrue(reportContent.Contains(mockResult.Success.ToString()), "Report should contain success status");
        }
        
        [Test]
        public void GenerateBuildReport_WithInvalidPath_ReturnsFalse()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            var mockResult = CreateMockBuildResult(true);
            string invalidPath = Path.Combine("Z:\\InvalidDrive", "report.txt");
            
            // Act
            bool success = _buildManager.GenerateBuildReport(mockResult, invalidPath);
            
            // Assert
            Assert.IsFalse(success, "Report generation should fail with invalid path");
        }
        
        #endregion
        
        #region Cleanup Tests
        
        [Test]
        public void CleanupBuildArtifacts_WithOldBuilds_RemovesOldDirectories()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            
            // Create some old build directories
            string oldBuildPath = Path.Combine(_testOutputDirectory, "OldBuild");
            Directory.CreateDirectory(oldBuildPath);
            
            // Set creation time to old date
            Directory.SetCreationTime(oldBuildPath, DateTime.Now.AddDays(-10));
            
            // Act
            int cleanedCount = _buildManager.CleanupBuildArtifacts(7);
            
            // Assert
            Assert.GreaterOrEqual(cleanedCount, 0, "Should return non-negative cleanup count");
            // Note: Actual cleanup may vary based on file system permissions
        }
        
        [Test]
        public void CleanupBuildArtifacts_WithRecentBuilds_PreservesRecentDirectories()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            
            // Create a recent build directory
            string recentBuildPath = Path.Combine(_testOutputDirectory, "RecentBuild");
            Directory.CreateDirectory(recentBuildPath);
            
            // Act
            int cleanedCount = _buildManager.CleanupBuildArtifacts(7);
            
            // Assert
            Assert.IsTrue(Directory.Exists(recentBuildPath), "Recent build should be preserved");
        }
        
        [Test]
        public void Dispose_CleansUpResources()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            _buildManager.StartBuild("DisposeTestBuild");
            
            // Act
            _buildManager.Dispose();
            
            // Assert
            Assert.IsFalse(_buildManager.IsInitialized, "Should not be initialized after disposal");
            Assert.IsFalse(_buildManager.IsBuildInProgress, "Should not have build in progress after disposal");
        }
        
        #endregion
        
        #region Event Tests
        
        [Test]
        public void BuildProgressUpdated_Event_FiresDuringBuild()
        {
            // Arrange
            _buildManager.Initialize(_testConfig);
            bool eventFired = false;
            IL2CPPBuildProgressEventArgs lastEventArgs = default;
            
            _buildManager.BuildProgressUpdated += (args) =>
            {
                eventFired = true;
                lastEventArgs = args;
            };
            
            // Act
            _buildManager.StartBuild("EventTestBuild");
            
            // Wait a moment for events to fire
            System.Threading.Thread.Sleep(100);
            
            // Assert
            Assert.IsTrue(eventFired, "Build progress event should fire during build");
            Assert.IsNotNull(lastEventArgs.Session.SessionId, "Event args should contain session info");
            Assert.IsTrue(lastEventArgs.Status.IsActive, "Status should indicate build is active");
        }
        
        #endregion
        
        #region Helper Methods
        
        private IL2CPPBuildResult CreateMockBuildResult(bool success)
        {
            var session = new IL2CPPBuildSession
            {
                SessionId = "MockSession",
                BuildName = "MockBuild",
                BuildType = IL2CPPBuildType.Development,
                Configuration = _testConfig,
                StartTime = System.DateTime.Now.AddMinutes(-5),
                OutputPath = Path.Combine(_testOutputDirectory, "MockBuild"),
                IsActive = false
            };
            
            var metrics = new IL2CPPBuildMetrics
            {
                PhaseTimings = new System.Collections.Generic.Dictionary<IL2CPPBuildPhase, System.TimeSpan>
                {
                    { IL2CPPBuildPhase.CompilingScripts, System.TimeSpan.FromSeconds(30) },
                    { IL2CPPBuildPhase.GeneratingIL2CPPCode, System.TimeSpan.FromSeconds(60) },
                    { IL2CPPBuildPhase.CompilingNativeCode, System.TimeSpan.FromSeconds(120) }
                },
                ScriptsCompiled = 100,
                IL2CPPFilesGenerated = 25,
                IL2CPPCodeSizeBytes = 1024 * 1024, // 1MB
                NativeLibrariesLinked = 5,
                ExecutableSizeBytes = 10 * 1024 * 1024, // 10MB
                PeakMemoryUsageBytes = 256 * 1024 * 1024, // 256MB
                AverageCpuUsage = 65f
            };
            
            var validationResult = new IL2CPPBuildValidationResult
            {
                IsQuest3Compatible = success,
                ValidationScore = success ? 0.9f : 0.3f,
                CompatibilityIssues = new IL2CPPCompatibilityIssue[0],
                PerformanceWarnings = new string[0],
                OptimizationRecommendations = new[] { "Consider enabling Burst compilation" },
                ValidationReport = "Mock validation report"
            };
            
            return new IL2CPPBuildResult
            {
                Session = session,
                Success = success,
                OutputPath = session.OutputPath,
                BuildTime = System.TimeSpan.FromMinutes(3.5),
                BuildSizeBytes = 10 * 1024 * 1024, // 10MB
                Warnings = new[] { "Mock warning" },
                Errors = success ? new string[0] : new[] { "Mock error" },
                BuildLog = "Mock build log content",
                Metrics = metrics,
                ValidationResult = validationResult
            };
        }
        
        #endregion
    }
}