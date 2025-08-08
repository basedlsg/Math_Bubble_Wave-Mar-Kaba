using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Interface for managing IL2CPP builds specifically for Quest 3 hardware testing.
    /// Provides automated build generation, validation, and deployment capabilities.
    /// Implements Requirement 5.3: IL2CPP build automation and testing.
    /// Implements Requirement 8.1: Quest 3-specific performance testing infrastructure.
    /// </summary>
    public interface IIL2CPPBuildManager
    {
        /// <summary>
        /// Whether the build manager is initialized and ready for operations.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Whether a build operation is currently in progress.
        /// </summary>
        bool IsBuildInProgress { get; }
        
        /// <summary>
        /// Current build configuration being used.
        /// </summary>
        IL2CPPBuildConfiguration CurrentConfiguration { get; }
        
        /// <summary>
        /// Initialize the IL2CPP build manager with the specified configuration.
        /// </summary>
        /// <param name="config">Build configuration settings</param>
        /// <returns>True if initialization succeeded</returns>
        bool Initialize(IL2CPPBuildConfiguration config);
        
        /// <summary>
        /// Start an automated IL2CPP build for Quest 3 testing.
        /// </summary>
        /// <param name="buildName">Name for the build (used in file naming)</param>
        /// <param name="buildType">Type of build to generate</param>
        /// <returns>Build session handle for tracking progress</returns>
        IL2CPPBuildSession StartBuild(string buildName, IL2CPPBuildType buildType = IL2CPPBuildType.Development);
        
        /// <summary>
        /// Cancel the current build operation if one is in progress.
        /// </summary>
        /// <returns>True if build was successfully cancelled</returns>
        bool CancelBuild();
        
        /// <summary>
        /// Get the current status of the active build session.
        /// </summary>
        /// <returns>Current build status and progress information</returns>
        IL2CPPBuildStatus GetBuildStatus();
        
        /// <summary>
        /// Wait for the current build to complete and return the result.
        /// </summary>
        /// <param name="timeoutSeconds">Maximum time to wait for completion</param>
        /// <returns>Complete build result with success/failure information</returns>
        IL2CPPBuildResult WaitForBuildCompletion(float timeoutSeconds = 600f);
        
        /// <summary>
        /// Validate an existing IL2CPP build for Quest 3 compatibility.
        /// </summary>
        /// <param name="buildPath">Path to the build to validate</param>
        /// <returns>Validation result with compatibility information</returns>
        IL2CPPBuildValidationResult ValidateBuild(string buildPath);
        
        /// <summary>
        /// Deploy a build to a connected Quest 3 device for testing.
        /// </summary>
        /// <param name="buildPath">Path to the build to deploy</param>
        /// <param name="deviceId">Target Quest 3 device ID (null for first available)</param>
        /// <returns>Deployment result with success/failure information</returns>
        IL2CPPDeploymentResult DeployToQuest3(string buildPath, string deviceId = null);
        
        /// <summary>
        /// Get a list of connected Quest 3 devices available for deployment.
        /// </summary>
        /// <returns>Array of connected Quest 3 device information</returns>
        Quest3DeviceInfo[] GetConnectedQuest3Devices();
        
        /// <summary>
        /// Create a performance-optimized build configuration for Quest 3.
        /// </summary>
        /// <param name="optimizationLevel">Level of optimization to apply</param>
        /// <returns>Optimized build configuration</returns>
        IL2CPPBuildConfiguration CreateOptimizedConfiguration(IL2CPPOptimizationLevel optimizationLevel);
        
        /// <summary>
        /// Generate a build report with detailed information about the build process.
        /// </summary>
        /// <param name="buildResult">Build result to generate report for</param>
        /// <param name="outputPath">Path to save the report</param>
        /// <returns>True if report was successfully generated</returns>
        bool GenerateBuildReport(IL2CPPBuildResult buildResult, string outputPath);
        
        /// <summary>
        /// Configure build settings specifically for Quest 3 hardware requirements.
        /// </summary>
        /// <param name="settings">Quest 3-specific build settings</param>
        void ConfigureQuest3Settings(Quest3BuildSettings settings);
        
        /// <summary>
        /// Get build history and statistics for analysis.
        /// </summary>
        /// <returns>Historical build data and performance metrics</returns>
        IL2CPPBuildHistory GetBuildHistory();
        
        /// <summary>
        /// Clean up build artifacts and temporary files.
        /// </summary>
        /// <param name="olderThanDays">Remove builds older than specified days (0 = all)</param>
        /// <returns>Number of builds cleaned up</returns>
        int CleanupBuildArtifacts(int olderThanDays = 7);
        
        /// <summary>
        /// Event fired when build progress updates.
        /// </summary>
        event Action<IL2CPPBuildProgressEventArgs> BuildProgressUpdated;
        
        /// <summary>
        /// Event fired when a build completes (success or failure).
        /// </summary>
        event Action<IL2CPPBuildCompletedEventArgs> BuildCompleted;
        
        /// <summary>
        /// Clean up build manager resources.
        /// </summary>
        void Dispose();
    }
    
    /// <summary>
    /// Configuration settings for IL2CPP builds.
    /// </summary>
    [Serializable]
    public struct IL2CPPBuildConfiguration
    {
        /// <summary>
        /// Target build type (Development, Release, etc.).
        /// </summary>
        public IL2CPPBuildType BuildType;
        
        /// <summary>
        /// IL2CPP code generation optimization level.
        /// </summary>
        public IL2CPPOptimizationLevel OptimizationLevel;
        
        /// <summary>
        /// Whether to enable script debugging in the build.
        /// </summary>
        public bool EnableScriptDebugging;
        
        /// <summary>
        /// Whether to enable deep profiling support.
        /// </summary>
        public bool EnableDeepProfiling;
        
        /// <summary>
        /// Whether to strip engine code for smaller builds.
        /// </summary>
        public bool StripEngineCode;
        
        /// <summary>
        /// Managed stripping level for code size optimization.
        /// </summary>
        public ManagedStrippingLevel StrippingLevel;
        
        /// <summary>
        /// Output directory for generated builds.
        /// </summary>
        public string OutputDirectory;
        
        /// <summary>
        /// Whether to automatically deploy after successful build.
        /// </summary>
        public bool AutoDeploy;
        
        /// <summary>
        /// Target Quest 3 device for auto-deployment.
        /// </summary>
        public string TargetDeviceId;
        
        /// <summary>
        /// Additional scripting define symbols for the build.
        /// </summary>
        public string[] AdditionalDefineSymbols;
        
        /// <summary>
        /// Quest 3-specific build settings.
        /// </summary>
        public Quest3BuildSettings Quest3Settings;
        
        /// <summary>
        /// Default configuration optimized for Quest 3 development.
        /// </summary>
        public static IL2CPPBuildConfiguration Quest3Development => new IL2CPPBuildConfiguration
        {
            BuildType = IL2CPPBuildType.Development,
            OptimizationLevel = IL2CPPOptimizationLevel.Speed,
            EnableScriptDebugging = true,
            EnableDeepProfiling = false,
            StripEngineCode = false,
            StrippingLevel = ManagedStrippingLevel.Minimal,
            OutputDirectory = "Builds/Quest3/Development",
            AutoDeploy = false,
            TargetDeviceId = null,
            AdditionalDefineSymbols = new string[] { "QUEST3_BUILD", "DEVELOPMENT_BUILD" },
            Quest3Settings = Quest3BuildSettings.Default
        };
        
        /// <summary>
        /// Configuration optimized for Quest 3 performance testing.
        /// </summary>
        public static IL2CPPBuildConfiguration Quest3Performance => new IL2CPPBuildConfiguration
        {
            BuildType = IL2CPPBuildType.Release,
            OptimizationLevel = IL2CPPOptimizationLevel.Speed,
            EnableScriptDebugging = false,
            EnableDeepProfiling = false,
            StripEngineCode = true,
            StrippingLevel = ManagedStrippingLevel.High,
            OutputDirectory = "Builds/Quest3/Performance",
            AutoDeploy = true,
            TargetDeviceId = null,
            AdditionalDefineSymbols = new string[] { "QUEST3_BUILD", "PERFORMANCE_BUILD" },
            Quest3Settings = Quest3BuildSettings.Performance
        };
        
        /// <summary>
        /// Configuration for Quest 3 store submission builds.
        /// </summary>
        public static IL2CPPBuildConfiguration Quest3Release => new IL2CPPBuildConfiguration
        {
            BuildType = IL2CPPBuildType.Release,
            OptimizationLevel = IL2CPPOptimizationLevel.Size,
            EnableScriptDebugging = false,
            EnableDeepProfiling = false,
            StripEngineCode = true,
            StrippingLevel = ManagedStrippingLevel.High,
            OutputDirectory = "Builds/Quest3/Release",
            AutoDeploy = false,
            TargetDeviceId = null,
            AdditionalDefineSymbols = new string[] { "QUEST3_BUILD", "RELEASE_BUILD" },
            Quest3Settings = Quest3BuildSettings.Release
        };
    }
    
    /// <summary>
    /// Types of IL2CPP builds that can be generated.
    /// </summary>
    public enum IL2CPPBuildType
    {
        Development,
        Release,
        Master,
        Profiling
    }
    
    /// <summary>
    /// IL2CPP code generation optimization levels.
    /// </summary>
    public enum IL2CPPOptimizationLevel
    {
        None,
        Speed,
        Size,
        SpeedAndSize
    }
    
    /// <summary>
    /// Managed code stripping levels for build size optimization.
    /// </summary>
    public enum ManagedStrippingLevel
    {
        Disabled,
        Minimal,
        Low,
        Medium,
        High
    }
    
    /// <summary>
    /// Quest 3-specific build settings.
    /// </summary>
    [Serializable]
    public struct Quest3BuildSettings
    {
        /// <summary>
        /// Target Android API level for Quest 3.
        /// </summary>
        public int TargetApiLevel;
        
        /// <summary>
        /// Minimum Android API level supported.
        /// </summary>
        public int MinimumApiLevel;
        
        /// <summary>
        /// Target CPU architecture for Quest 3.
        /// </summary>
        public AndroidArchitecture TargetArchitecture;
        
        /// <summary>
        /// Whether to enable ARM64 optimizations.
        /// </summary>
        public bool EnableARM64Optimizations;
        
        /// <summary>
        /// Graphics API to use for rendering.
        /// </summary>
        public GraphicsDeviceType GraphicsAPI;
        
        /// <summary>
        /// Color space for rendering (Linear recommended for VR).
        /// </summary>
        public ColorSpace ColorSpace;
        
        /// <summary>
        /// Whether to enable multithreaded rendering.
        /// </summary>
        public bool MultithreadedRendering;
        
        /// <summary>
        /// Texture compression format for Quest 3.
        /// </summary>
        public MobileTextureSubtarget TextureCompression;
        
        /// <summary>
        /// Whether to enable GPU skinning for better performance.
        /// </summary>
        public bool EnableGPUSkinning;
        
        /// <summary>
        /// Whether to enable static batching.
        /// </summary>
        public bool EnableStaticBatching;
        
        /// <summary>
        /// Whether to enable dynamic batching.
        /// </summary>
        public bool EnableDynamicBatching;
        
        /// <summary>
        /// Default Quest 3 build settings.
        /// </summary>
        public static Quest3BuildSettings Default => new Quest3BuildSettings
        {
            TargetApiLevel = 32, // Android 12 for Quest 3
            MinimumApiLevel = 29, // Android 10 minimum
            TargetArchitecture = AndroidArchitecture.ARM64,
            EnableARM64Optimizations = true,
            GraphicsAPI = GraphicsDeviceType.Vulkan,
            ColorSpace = ColorSpace.Linear,
            MultithreadedRendering = true,
            TextureCompression = MobileTextureSubtarget.ASTC,
            EnableGPUSkinning = true,
            EnableStaticBatching = true,
            EnableDynamicBatching = false
        };
        
        /// <summary>
        /// Performance-optimized Quest 3 settings.
        /// </summary>
        public static Quest3BuildSettings Performance => new Quest3BuildSettings
        {
            TargetApiLevel = 32,
            MinimumApiLevel = 29,
            TargetArchitecture = AndroidArchitecture.ARM64,
            EnableARM64Optimizations = true,
            GraphicsAPI = GraphicsDeviceType.Vulkan,
            ColorSpace = ColorSpace.Linear,
            MultithreadedRendering = true,
            TextureCompression = MobileTextureSubtarget.ASTC,
            EnableGPUSkinning = true,
            EnableStaticBatching = true,
            EnableDynamicBatching = false
        };
        
        /// <summary>
        /// Release build settings for Quest 3 store submission.
        /// </summary>
        public static Quest3BuildSettings Release => new Quest3BuildSettings
        {
            TargetApiLevel = 32,
            MinimumApiLevel = 29,
            TargetArchitecture = AndroidArchitecture.ARM64,
            EnableARM64Optimizations = true,
            GraphicsAPI = GraphicsDeviceType.Vulkan,
            ColorSpace = ColorSpace.Linear,
            MultithreadedRendering = true,
            TextureCompression = MobileTextureSubtarget.ASTC,
            EnableGPUSkinning = true,
            EnableStaticBatching = true,
            EnableDynamicBatching = false
        };
    }
    
    /// <summary>
    /// Android CPU architecture options.
    /// </summary>
    public enum AndroidArchitecture
    {
        ARMv7,
        ARM64,
        x86,
        All
    }
    
    /// <summary>
    /// Mobile texture compression formats.
    /// </summary>
    public enum MobileTextureSubtarget
    {
        Generic,
        DXT,
        PVRTC,
        ATC,
        ETC,
        ETC2,
        ASTC
    }
    
    /// <summary>
    /// IL2CPP build session tracking information.
    /// </summary>
    public struct IL2CPPBuildSession
    {
        /// <summary>
        /// Unique identifier for this build session.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// User-provided name for this build.
        /// </summary>
        public string BuildName;
        
        /// <summary>
        /// Build type being generated.
        /// </summary>
        public IL2CPPBuildType BuildType;
        
        /// <summary>
        /// Configuration used for this build.
        /// </summary>
        public IL2CPPBuildConfiguration Configuration;
        
        /// <summary>
        /// Timestamp when the build started.
        /// </summary>
        public DateTime StartTime;
        
        /// <summary>
        /// Expected output path for the build.
        /// </summary>
        public string OutputPath;
        
        /// <summary>
        /// Whether the build session is currently active.
        /// </summary>
        public bool IsActive;
    }
    
    /// <summary>
    /// Current status of an IL2CPP build operation.
    /// </summary>
    public struct IL2CPPBuildStatus
    {
        /// <summary>
        /// Current phase of the build process.
        /// </summary>
        public IL2CPPBuildPhase CurrentPhase;
        
        /// <summary>
        /// Overall progress percentage (0.0 to 1.0).
        /// </summary>
        public float OverallProgress;
        
        /// <summary>
        /// Progress within the current phase (0.0 to 1.0).
        /// </summary>
        public float PhaseProgress;
        
        /// <summary>
        /// Current status message.
        /// </summary>
        public string StatusMessage;
        
        /// <summary>
        /// Elapsed time since build started.
        /// </summary>
        public TimeSpan ElapsedTime;
        
        /// <summary>
        /// Estimated time remaining (if available).
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining;
        
        /// <summary>
        /// Whether the build is currently active.
        /// </summary>
        public bool IsActive;
        
        /// <summary>
        /// Any warnings encountered during the build.
        /// </summary>
        public string[] Warnings;
        
        /// <summary>
        /// Any errors encountered during the build.
        /// </summary>
        public string[] Errors;
    }
    
    /// <summary>
    /// Phases of the IL2CPP build process.
    /// </summary>
    public enum IL2CPPBuildPhase
    {
        Initializing,
        PreprocessingScripts,
        CompilingScripts,
        GeneratingIL2CPPCode,
        CompilingNativeCode,
        LinkingBinaries,
        PackagingBuild,
        PostProcessing,
        Completed,
        Failed
    }
    
    /// <summary>
    /// Complete result of an IL2CPP build operation.
    /// </summary>
    public struct IL2CPPBuildResult
    {
        /// <summary>
        /// Build session information.
        /// </summary>
        public IL2CPPBuildSession Session;
        
        /// <summary>
        /// Whether the build completed successfully.
        /// </summary>
        public bool Success;
        
        /// <summary>
        /// Final build output path.
        /// </summary>
        public string OutputPath;
        
        /// <summary>
        /// Total build time.
        /// </summary>
        public TimeSpan BuildTime;
        
        /// <summary>
        /// Size of the generated build in bytes.
        /// </summary>
        public long BuildSizeBytes;
        
        /// <summary>
        /// All warnings generated during the build.
        /// </summary>
        public string[] Warnings;
        
        /// <summary>
        /// All errors encountered during the build.
        /// </summary>
        public string[] Errors;
        
        /// <summary>
        /// Detailed build log.
        /// </summary>
        public string BuildLog;
        
        /// <summary>
        /// Build performance metrics.
        /// </summary>
        public IL2CPPBuildMetrics Metrics;
        
        /// <summary>
        /// Quest 3 compatibility validation result.
        /// </summary>
        public IL2CPPBuildValidationResult ValidationResult;
    }
    
    /// <summary>
    /// Performance metrics for IL2CPP build operations.
    /// </summary>
    public struct IL2CPPBuildMetrics
    {
        /// <summary>
        /// Time spent in each build phase.
        /// </summary>
        public Dictionary<IL2CPPBuildPhase, TimeSpan> PhaseTimings;
        
        /// <summary>
        /// Number of C# scripts compiled.
        /// </summary>
        public int ScriptsCompiled;
        
        /// <summary>
        /// Number of IL2CPP C++ files generated.
        /// </summary>
        public int IL2CPPFilesGenerated;
        
        /// <summary>
        /// Size of generated IL2CPP code in bytes.
        /// </summary>
        public long IL2CPPCodeSizeBytes;
        
        /// <summary>
        /// Number of native libraries linked.
        /// </summary>
        public int NativeLibrariesLinked;
        
        /// <summary>
        /// Final executable size in bytes.
        /// </summary>
        public long ExecutableSizeBytes;
        
        /// <summary>
        /// Memory usage during build peak.
        /// </summary>
        public long PeakMemoryUsageBytes;
        
        /// <summary>
        /// CPU usage during build (average percentage).
        /// </summary>
        public float AverageCpuUsage;
    }
    
    /// <summary>
    /// Validation result for Quest 3 build compatibility.
    /// </summary>
    public struct IL2CPPBuildValidationResult
    {
        /// <summary>
        /// Whether the build is compatible with Quest 3.
        /// </summary>
        public bool IsQuest3Compatible;
        
        /// <summary>
        /// Validation score (0.0 to 1.0).
        /// </summary>
        public float ValidationScore;
        
        /// <summary>
        /// Compatibility issues found.
        /// </summary>
        public IL2CPPCompatibilityIssue[] CompatibilityIssues;
        
        /// <summary>
        /// Performance warnings for Quest 3.
        /// </summary>
        public string[] PerformanceWarnings;
        
        /// <summary>
        /// Recommendations for Quest 3 optimization.
        /// </summary>
        public string[] OptimizationRecommendations;
        
        /// <summary>
        /// Detailed validation report.
        /// </summary>
        public string ValidationReport;
    }
    
    /// <summary>
    /// Quest 3 compatibility issue information.
    /// </summary>
    public struct IL2CPPCompatibilityIssue
    {
        /// <summary>
        /// Type of compatibility issue.
        /// </summary>
        public CompatibilityIssueType IssueType;
        
        /// <summary>
        /// Severity of the issue.
        /// </summary>
        public CompatibilityIssueSeverity Severity;
        
        /// <summary>
        /// Description of the issue.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Recommended fix for the issue.
        /// </summary>
        public string RecommendedFix;
        
        /// <summary>
        /// File or component where the issue was found.
        /// </summary>
        public string Location;
    }
    
    /// <summary>
    /// Types of Quest 3 compatibility issues.
    /// </summary>
    public enum CompatibilityIssueType
    {
        UnsupportedAPI,
        PerformanceIssue,
        MemoryIssue,
        GraphicsIssue,
        PlatformSpecific,
        SecurityIssue
    }
    
    /// <summary>
    /// Severity levels for compatibility issues.
    /// </summary>
    public enum CompatibilityIssueSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>
    /// Result of deploying a build to Quest 3 device.
    /// </summary>
    public struct IL2CPPDeploymentResult
    {
        /// <summary>
        /// Whether deployment was successful.
        /// </summary>
        public bool Success;
        
        /// <summary>
        /// Target device information.
        /// </summary>
        public Quest3DeviceInfo TargetDevice;
        
        /// <summary>
        /// Time taken for deployment.
        /// </summary>
        public TimeSpan DeploymentTime;
        
        /// <summary>
        /// Package name of the deployed application.
        /// </summary>
        public string PackageName;
        
        /// <summary>
        /// Version of the deployed application.
        /// </summary>
        public string AppVersion;
        
        /// <summary>
        /// Any errors encountered during deployment.
        /// </summary>
        public string[] DeploymentErrors;
        
        /// <summary>
        /// Deployment log information.
        /// </summary>
        public string DeploymentLog;
    }
    
    /// <summary>
    /// Information about a connected Quest 3 device.
    /// </summary>
    public struct Quest3DeviceInfo
    {
        /// <summary>
        /// Unique device identifier.
        /// </summary>
        public string DeviceId;
        
        /// <summary>
        /// Human-readable device name.
        /// </summary>
        public string DeviceName;
        
        /// <summary>
        /// Quest 3 firmware version.
        /// </summary>
        public string FirmwareVersion;
        
        /// <summary>
        /// Android version on the device.
        /// </summary>
        public string AndroidVersion;
        
        /// <summary>
        /// Whether the device is connected via USB.
        /// </summary>
        public bool IsUSBConnected;
        
        /// <summary>
        /// Whether the device is connected via WiFi (ADB wireless).
        /// </summary>
        public bool IsWiFiConnected;
        
        /// <summary>
        /// Whether developer mode is enabled.
        /// </summary>
        public bool DeveloperModeEnabled;
        
        /// <summary>
        /// Available storage space in bytes.
        /// </summary>
        public long AvailableStorageBytes;
        
        /// <summary>
        /// Battery level percentage (0-100).
        /// </summary>
        public int BatteryLevel;
        
        /// <summary>
        /// Whether the device is currently charging.
        /// </summary>
        public bool IsCharging;
    }
    
    /// <summary>
    /// Historical build data and statistics.
    /// </summary>
    public struct IL2CPPBuildHistory
    {
        /// <summary>
        /// All completed build sessions.
        /// </summary>
        public IL2CPPBuildResult[] CompletedBuilds;
        
        /// <summary>
        /// Average build time across all builds.
        /// </summary>
        public TimeSpan AverageBuildTime;
        
        /// <summary>
        /// Success rate percentage (0.0 to 1.0).
        /// </summary>
        public float SuccessRate;
        
        /// <summary>
        /// Most common build errors.
        /// </summary>
        public string[] CommonErrors;
        
        /// <summary>
        /// Build time trend over recent builds.
        /// </summary>
        public TimeSpan[] RecentBuildTimes;
        
        /// <summary>
        /// Total number of builds attempted.
        /// </summary>
        public int TotalBuildsAttempted;
        
        /// <summary>
        /// Total number of successful builds.
        /// </summary>
        public int TotalSuccessfulBuilds;
    }
    
    /// <summary>
    /// Event arguments for build progress updates.
    /// </summary>
    public struct IL2CPPBuildProgressEventArgs
    {
        /// <summary>
        /// Build session being updated.
        /// </summary>
        public IL2CPPBuildSession Session;
        
        /// <summary>
        /// Current build status.
        /// </summary>
        public IL2CPPBuildStatus Status;
        
        /// <summary>
        /// Timestamp of this progress update.
        /// </summary>
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Event arguments for build completion.
    /// </summary>
    public struct IL2CPPBuildCompletedEventArgs
    {
        /// <summary>
        /// Complete build result.
        /// </summary>
        public IL2CPPBuildResult Result;
        
        /// <summary>
        /// Timestamp when build completed.
        /// </summary>
        public DateTime CompletionTime;
    }
}