using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for orchestrating performance validation gates in CI/CD pipeline.
    /// Implements Requirement 4: Continuous Integration Performance Gates from the "do-it-right" recovery specification.
    /// </summary>
    public interface IPerformanceGateRunner
    {
        /// <summary>
        /// Runs all configured performance gates and returns the overall result.
        /// </summary>
        /// <returns>Performance gate execution result</returns>
        PerformanceGateResult RunAllGates();
        
        /// <summary>
        /// Runs all performance gates asynchronously.
        /// </summary>
        /// <returns>Task containing the performance gate execution result</returns>
        Task<PerformanceGateResult> RunAllGatesAsync();
        
        /// <summary>
        /// Runs a specific performance gate by name.
        /// </summary>
        /// <param name="gateName">Name of the gate to run</param>
        /// <returns>Result of the specific gate execution</returns>
        PerformanceGateResult RunSpecificGate(string gateName);
        
        /// <summary>
        /// Registers a new performance gate for execution.
        /// </summary>
        /// <param name="gate">Performance gate to register</param>
        void RegisterGate(IPerformanceGate gate);
        
        /// <summary>
        /// Unregisters a performance gate from execution.
        /// </summary>
        /// <param name="gateName">Name of the gate to unregister</param>
        void UnregisterGate(string gateName);
        
        /// <summary>
        /// Gets all registered performance gates.
        /// </summary>
        /// <returns>List of registered performance gates</returns>
        List<IPerformanceGate> GetRegisteredGates();
        
        /// <summary>
        /// Gets the execution history for performance gates.
        /// </summary>
        /// <returns>Historical performance gate results</returns>
        List<PerformanceGateExecutionRecord> GetExecutionHistory();
        
        /// <summary>
        /// Validates the configuration of all registered gates.
        /// </summary>
        /// <returns>Configuration validation result</returns>
        GateConfigurationValidationResult ValidateConfiguration();
        
        /// <summary>
        /// Generates a comprehensive performance gate report.
        /// </summary>
        /// <returns>Path to the generated report file</returns>
        string GeneratePerformanceReport();
        
        /// <summary>
        /// Sets the failure threshold for gate execution.
        /// </summary>
        /// <param name="threshold">Threshold configuration</param>
        void SetFailureThreshold(PerformanceThreshold threshold);
        
        /// <summary>
        /// Gets the current failure threshold configuration.
        /// </summary>
        /// <returns>Current threshold configuration</returns>
        PerformanceThreshold GetFailureThreshold();
    }
    
    /// <summary>
    /// Interface for individual performance gates.
    /// </summary>
    public interface IPerformanceGate
    {
        /// <summary>
        /// Unique name of the performance gate.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Description of what this gate validates.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Priority of this gate (higher priority gates run first).
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Whether this gate is critical (failure blocks the build).
        /// </summary>
        bool IsCritical { get; }
        
        /// <summary>
        /// Expected execution time for this gate.
        /// </summary>
        TimeSpan ExpectedExecutionTime { get; }
        
        /// <summary>
        /// Executes the performance gate validation.
        /// </summary>
        /// <returns>Gate execution result</returns>
        PerformanceGateResult Execute();
        
        /// <summary>
        /// Executes the performance gate validation asynchronously.
        /// </summary>
        /// <returns>Task containing the gate execution result</returns>
        Task<PerformanceGateResult> ExecuteAsync();
        
        /// <summary>
        /// Validates the gate's configuration before execution.
        /// </summary>
        /// <returns>Configuration validation result</returns>
        bool ValidateConfiguration();
        
        /// <summary>
        /// Gets detailed information about the gate's current state.
        /// </summary>
        /// <returns>Gate status information</returns>
        PerformanceGateStatus GetStatus();
    }
    
    /// <summary>
    /// Result of performance gate execution.
    /// </summary>
    [Serializable]
    public class PerformanceGateResult
    {
        /// <summary>
        /// Whether the gate execution was successful.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Name of the gate that was executed.
        /// </summary>
        public string GateName { get; set; }
        
        /// <summary>
        /// When the gate was executed.
        /// </summary>
        public DateTime ExecutedAt { get; set; }
        
        /// <summary>
        /// How long the gate took to execute.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// Detailed results from individual gates.
        /// </summary>
        public List<IndividualGateResult> GateResults { get; set; } = new List<IndividualGateResult>();
        
        /// <summary>
        /// Any error messages from gate execution.
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new List<string>();
        
        /// <summary>
        /// Any warning messages from gate execution.
        /// </summary>
        public List<string> WarningMessages { get; set; } = new List<string>();
        
        /// <summary>
        /// Performance metrics collected during execution.
        /// </summary>
        public Dictionary<string, object> PerformanceMetrics { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Path to detailed profiling data, if available.
        /// </summary>
        public string ProfilingDataPath { get; set; }
        
        /// <summary>
        /// Summary of the gate execution.
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// Whether this result should block the build.
        /// </summary>
        public bool ShouldBlockBuild => !Success && GateResults.Any(g => g.IsCritical && !g.Success);
    }
    
    /// <summary>
    /// Result of an individual performance gate.
    /// </summary>
    [Serializable]
    public class IndividualGateResult
    {
        /// <summary>
        /// Name of the gate.
        /// </summary>
        public string GateName { get; set; }
        
        /// <summary>
        /// Whether the gate passed.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Whether this gate is critical.
        /// </summary>
        public bool IsCritical { get; set; }
        
        /// <summary>
        /// Execution time for this gate.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// Detailed message about the gate result.
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Metrics collected by this gate.
        /// </summary>
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Any errors encountered during execution.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// Any warnings generated during execution.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Record of a performance gate execution for historical tracking.
    /// </summary>
    [Serializable]
    public class PerformanceGateExecutionRecord
    {
        /// <summary>
        /// When the execution occurred.
        /// </summary>
        public DateTime ExecutedAt { get; set; }
        
        /// <summary>
        /// The execution result.
        /// </summary>
        public PerformanceGateResult Result { get; set; }
        
        /// <summary>
        /// Build or commit identifier associated with this execution.
        /// </summary>
        public string BuildIdentifier { get; set; }
        
        /// <summary>
        /// Environment where the execution occurred.
        /// </summary>
        public string Environment { get; set; }
        
        /// <summary>
        /// Unity version used during execution.
        /// </summary>
        public string UnityVersion { get; set; }
        
        /// <summary>
        /// System information where execution occurred.
        /// </summary>
        public Dictionary<string, string> SystemInfo { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Result of gate configuration validation.
    /// </summary>
    [Serializable]
    public class GateConfigurationValidationResult
    {
        /// <summary>
        /// Whether all gate configurations are valid.
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Configuration issues found.
        /// </summary>
        public List<string> Issues { get; set; } = new List<string>();
        
        /// <summary>
        /// Warnings about gate configuration.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
        
        /// <summary>
        /// When the validation was performed.
        /// </summary>
        public DateTime ValidatedAt { get; set; }
        
        /// <summary>
        /// Number of gates validated.
        /// </summary>
        public int GatesValidated { get; set; }
    }
    
    /// <summary>
    /// Performance threshold configuration.
    /// </summary>
    [Serializable]
    public class PerformanceThreshold
    {
        /// <summary>
        /// Minimum acceptable FPS for performance gates.
        /// </summary>
        public float MinimumFPS { get; set; } = 60.0f;
        
        /// <summary>
        /// Maximum acceptable frame time in milliseconds.
        /// </summary>
        public float MaximumFrameTimeMs { get; set; } = 16.67f; // ~60 FPS
        
        /// <summary>
        /// Maximum acceptable memory usage in MB.
        /// </summary>
        public float MaximumMemoryUsageMB { get; set; } = 512.0f;
        
        /// <summary>
        /// Maximum acceptable build time in minutes.
        /// </summary>
        public float MaximumBuildTimeMinutes { get; set; } = 10.0f;
        
        /// <summary>
        /// Maximum acceptable test execution time in minutes.
        /// </summary>
        public float MaximumTestTimeMinutes { get; set; } = 5.0f;
        
        /// <summary>
        /// Whether to fail on warnings or only on errors.
        /// </summary>
        public bool FailOnWarnings { get; set; } = false;
        
        /// <summary>
        /// Custom threshold values for specific metrics.
        /// </summary>
        public Dictionary<string, float> CustomThresholds { get; set; } = new Dictionary<string, float>();
    }
    
    /// <summary>
    /// Status information for a performance gate.
    /// </summary>
    [Serializable]
    public class PerformanceGateStatus
    {
        /// <summary>
        /// Name of the gate.
        /// </summary>
        public string GateName { get; set; }
        
        /// <summary>
        /// Current status of the gate.
        /// </summary>
        public GateStatus Status { get; set; }
        
        /// <summary>
        /// Last execution time.
        /// </summary>
        public DateTime? LastExecuted { get; set; }
        
        /// <summary>
        /// Last execution result.
        /// </summary>
        public bool? LastResult { get; set; }
        
        /// <summary>
        /// Configuration validation status.
        /// </summary>
        public bool ConfigurationValid { get; set; }
        
        /// <summary>
        /// Any status messages.
        /// </summary>
        public List<string> StatusMessages { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Enumeration of possible gate statuses.
    /// </summary>
    public enum GateStatus
    {
        /// <summary>
        /// Gate is ready to execute.
        /// </summary>
        Ready,
        
        /// <summary>
        /// Gate is currently executing.
        /// </summary>
        Executing,
        
        /// <summary>
        /// Gate execution completed successfully.
        /// </summary>
        Completed,
        
        /// <summary>
        /// Gate execution failed.
        /// </summary>
        Failed,
        
        /// <summary>
        /// Gate configuration is invalid.
        /// </summary>
        ConfigurationError,
        
        /// <summary>
        /// Gate is disabled.
        /// </summary>
        Disabled
    }
}