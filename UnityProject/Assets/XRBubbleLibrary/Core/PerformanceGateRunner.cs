using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Main implementation of the performance gate runner for CI/CD pipeline validation.
    /// Orchestrates execution of multiple performance gates and provides comprehensive reporting.
    /// Implements Requirement 4: Continuous Integration Performance Gates.
    /// </summary>
    public class PerformanceGateRunner : IPerformanceGateRunner
    {
        private readonly Dictionary<string, IPerformanceGate> _registeredGates;
        private readonly List<PerformanceGateExecutionRecord> _executionHistory;
        private PerformanceThreshold _failureThreshold;
        
        private static PerformanceGateRunner _instance;
        
        /// <summary>
        /// Singleton instance for global access.
        /// </summary>
        public static PerformanceGateRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PerformanceGateRunner();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the PerformanceGateRunner.
        /// </summary>
        public PerformanceGateRunner()
        {
            _registeredGates = new Dictionary<string, IPerformanceGate>();
            _executionHistory = new List<PerformanceGateExecutionRecord>();
            _failureThreshold = new PerformanceThreshold();
            
            RegisterDefaultGates();
        }
        
        /// <summary>
        /// Runs all configured performance gates and returns the overall result.
        /// </summary>
        public PerformanceGateResult RunAllGates()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new PerformanceGateResult
            {
                GateName = "All Gates",
                ExecutedAt = DateTime.UtcNow,
                Success = true
            };
            
            try
            {
                UnityEngine.Debug.Log("[PerformanceGateRunner] Starting execution of all performance gates...");
                
                // Validate configuration first
                var configValidation = ValidateConfiguration();
                if (!configValidation.IsValid)
                {
                    result.Success = false;
                    result.ErrorMessages.AddRange(configValidation.Issues);
                    result.Summary = $"Configuration validation failed with {configValidation.Issues.Count} issues";
                    return result;
                }
                
                // Get gates sorted by priority (highest first)
                var gatesToRun = _registeredGates.Values
                    .OrderByDescending(g => g.Priority)
                    .ToList();
                
                UnityEngine.Debug.Log($"[PerformanceGateRunner] Executing {gatesToRun.Count} performance gates");
                
                // Execute each gate
                foreach (var gate in gatesToRun)
                {
                    try
                    {
                        UnityEngine.Debug.Log($"[PerformanceGateRunner] Executing gate: {gate.Name}");
                        
                        var gateStopwatch = Stopwatch.StartNew();
                        var gateResult = gate.Execute();
                        gateStopwatch.Stop();
                        
                        var individualResult = new IndividualGateResult
                        {
                            GateName = gate.Name,
                            Success = gateResult.Success,
                            IsCritical = gate.IsCritical,
                            ExecutionTime = gateStopwatch.Elapsed,
                            Message = gateResult.Summary ?? $"Gate {gate.Name} {(gateResult.Success ? "passed" : "failed")}",
                            Metrics = new Dictionary<string, object>(gateResult.PerformanceMetrics),
                            Errors = new List<string>(gateResult.ErrorMessages),
                            Warnings = new List<string>(gateResult.WarningMessages)
                        };
                        
                        result.GateResults.Add(individualResult);
                        
                        // Aggregate metrics
                        foreach (var metric in gateResult.PerformanceMetrics)
                        {
                            result.PerformanceMetrics[$"{gate.Name}_{metric.Key}"] = metric.Value;
                        }
                        
                        // Check if this gate failure should fail the overall result
                        if (!gateResult.Success)
                        {
                            if (gate.IsCritical)
                            {
                                result.Success = false;
                                UnityEngine.Debug.LogError($"[PerformanceGateRunner] Critical gate {gate.Name} failed");
                            }
                            else
                            {
                                UnityEngine.Debug.LogWarning($"[PerformanceGateRunner] Non-critical gate {gate.Name} failed");
                            }
                        }
                        
                        // Aggregate error and warning messages
                        result.ErrorMessages.AddRange(gateResult.ErrorMessages);
                        result.WarningMessages.AddRange(gateResult.WarningMessages);
                        
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[PerformanceGateRunner] Exception in gate {gate.Name}: {ex.Message}");
                        
                        var errorResult = new IndividualGateResult
                        {
                            GateName = gate.Name,
                            Success = false,
                            IsCritical = gate.IsCritical,
                            ExecutionTime = TimeSpan.Zero,
                            Message = $"Gate execution failed with exception: {ex.Message}",
                            Errors = { ex.Message }
                        };
                        
                        result.GateResults.Add(errorResult);
                        result.ErrorMessages.Add($"Gate {gate.Name} threw exception: {ex.Message}");
                        
                        if (gate.IsCritical)
                        {
                            result.Success = false;
                        }
                    }
                }
                
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                
                // Generate summary
                var passedGates = result.GateResults.Count(g => g.Success);
                var failedGates = result.GateResults.Count(g => !g.Success);
                var criticalFailures = result.GateResults.Count(g => !g.Success && g.IsCritical);
                
                result.Summary = $"Performance gates completed: {passedGates} passed, {failedGates} failed " +
                               $"({criticalFailures} critical failures). Overall: {(result.Success ? "PASS" : "FAIL")}";
                
                UnityEngine.Debug.Log($"[PerformanceGateRunner] {result.Summary}");
                
                // Record execution
                RecordExecution(result);
                
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                result.Success = false;
                result.ErrorMessages.Add($"Performance gate runner failed: {ex.Message}");
                result.Summary = $"Performance gate execution failed with exception: {ex.Message}";
                
                UnityEngine.Debug.LogError($"[PerformanceGateRunner] Fatal error: {ex.Message}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Runs all performance gates asynchronously.
        /// </summary>
        public async Task<PerformanceGateResult> RunAllGatesAsync()
        {
            return await Task.Run(() => RunAllGates());
        }
        
        /// <summary>
        /// Runs a specific performance gate by name.
        /// </summary>
        public PerformanceGateResult RunSpecificGate(string gateName)
        {
            if (string.IsNullOrEmpty(gateName))
            {
                return new PerformanceGateResult
                {
                    Success = false,
                    GateName = gateName,
                    ExecutedAt = DateTime.UtcNow,
                    ErrorMessages = { "Gate name cannot be null or empty" },
                    Summary = "Invalid gate name provided"
                };
            }
            
            if (!_registeredGates.TryGetValue(gateName, out var gate))
            {
                return new PerformanceGateResult
                {
                    Success = false,
                    GateName = gateName,
                    ExecutedAt = DateTime.UtcNow,
                    ErrorMessages = { $"Gate '{gateName}' is not registered" },
                    Summary = $"Gate '{gateName}' not found"
                };
            }
            
            try
            {
                UnityEngine.Debug.Log($"[PerformanceGateRunner] Executing specific gate: {gateName}");
                
                var stopwatch = Stopwatch.StartNew();
                var gateResult = gate.Execute();
                stopwatch.Stop();
                
                gateResult.ExecutionTime = stopwatch.Elapsed;
                
                // Record execution
                RecordExecution(gateResult);
                
                UnityEngine.Debug.Log($"[PerformanceGateRunner] Gate {gateName} completed: {(gateResult.Success ? "PASS" : "FAIL")}");
                
                return gateResult;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceGateRunner] Exception executing gate {gateName}: {ex.Message}");
                
                return new PerformanceGateResult
                {
                    Success = false,
                    GateName = gateName,
                    ExecutedAt = DateTime.UtcNow,
                    ErrorMessages = { ex.Message },
                    Summary = $"Gate execution failed with exception: {ex.Message}"
                };
            }
        }
        
        /// <summary>
        /// Registers a new performance gate for execution.
        /// </summary>
        public void RegisterGate(IPerformanceGate gate)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(nameof(gate));
            }
            
            if (string.IsNullOrEmpty(gate.Name))
            {
                throw new ArgumentException("Gate name cannot be null or empty", nameof(gate));
            }
            
            if (_registeredGates.ContainsKey(gate.Name))
            {
                UnityEngine.Debug.LogWarning($"[PerformanceGateRunner] Replacing existing gate: {gate.Name}");
            }
            
            _registeredGates[gate.Name] = gate;
            UnityEngine.Debug.Log($"[PerformanceGateRunner] Registered gate: {gate.Name} (Priority: {gate.Priority}, Critical: {gate.IsCritical})");
        }
        
        /// <summary>
        /// Unregisters a performance gate from execution.
        /// </summary>
        public void UnregisterGate(string gateName)
        {
            if (string.IsNullOrEmpty(gateName))
            {
                return;
            }
            
            if (_registeredGates.Remove(gateName))
            {
                UnityEngine.Debug.Log($"[PerformanceGateRunner] Unregistered gate: {gateName}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[PerformanceGateRunner] Gate not found for unregistration: {gateName}");
            }
        }
        
        /// <summary>
        /// Gets all registered performance gates.
        /// </summary>
        public List<IPerformanceGate> GetRegisteredGates()
        {
            return new List<IPerformanceGate>(_registeredGates.Values);
        }
        
        /// <summary>
        /// Gets the execution history for performance gates.
        /// </summary>
        public List<PerformanceGateExecutionRecord> GetExecutionHistory()
        {
            return new List<PerformanceGateExecutionRecord>(_executionHistory);
        }
        
        /// <summary>
        /// Validates the configuration of all registered gates.
        /// </summary>
        public GateConfigurationValidationResult ValidateConfiguration()
        {
            var result = new GateConfigurationValidationResult
            {
                ValidatedAt = DateTime.UtcNow,
                IsValid = true,
                GatesValidated = _registeredGates.Count
            };
            
            try
            {
                if (_registeredGates.Count == 0)
                {
                    result.Warnings.Add("No performance gates are registered");
                }
                
                foreach (var gate in _registeredGates.Values)
                {
                    try
                    {
                        if (!gate.ValidateConfiguration())
                        {
                            result.Issues.Add($"Gate '{gate.Name}' has invalid configuration");
                            result.IsValid = false;
                        }
                        
                        // Check for reasonable priority values
                        if (gate.Priority < 0 || gate.Priority > 1000)
                        {
                            result.Warnings.Add($"Gate '{gate.Name}' has unusual priority value: {gate.Priority}");
                        }
                        
                        // Check for reasonable execution time estimates
                        if (gate.ExpectedExecutionTime > TimeSpan.FromMinutes(10))
                        {
                            result.Warnings.Add($"Gate '{gate.Name}' has very long expected execution time: {gate.ExpectedExecutionTime}");
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        result.Issues.Add($"Gate '{gate.Name}' configuration validation threw exception: {ex.Message}");
                        result.IsValid = false;
                    }
                }
                
                // Check for duplicate priorities among critical gates
                var criticalGates = _registeredGates.Values.Where(g => g.IsCritical).ToList();
                var duplicatePriorities = criticalGates
                    .GroupBy(g => g.Priority)
                    .Where(g => g.Count() > 1)
                    .ToList();
                
                foreach (var group in duplicatePriorities)
                {
                    var gateNames = string.Join(", ", group.Select(g => g.Name));
                    result.Warnings.Add($"Multiple critical gates have the same priority {group.Key}: {gateNames}");
                }
                
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Configuration validation failed: {ex.Message}");
                result.IsValid = false;
            }
            
            UnityEngine.Debug.Log($"[PerformanceGateRunner] Configuration validation: {(result.IsValid ? "VALID" : "INVALID")} " +
                                $"({result.Issues.Count} issues, {result.Warnings.Count} warnings)");
            
            return result;
        }
        
        /// <summary>
        /// Generates a comprehensive performance gate report.
        /// </summary>
        public string GeneratePerformanceReport()
        {
            try
            {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                var reportsDir = Path.Combine(projectRoot, "PerformanceReports");
                Directory.CreateDirectory(reportsDir);
                
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var reportPath = Path.Combine(reportsDir, $"performance_gate_report_{timestamp}.md");
                
                var report = GenerateReportContent();
                File.WriteAllText(reportPath, report);
                
                UnityEngine.Debug.Log($"[PerformanceGateRunner] Generated performance report: {reportPath}");
                return reportPath;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceGateRunner] Failed to generate performance report: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Sets the failure threshold for gate execution.
        /// </summary>
        public void SetFailureThreshold(PerformanceThreshold threshold)
        {
            _failureThreshold = threshold ?? throw new ArgumentNullException(nameof(threshold));
            UnityEngine.Debug.Log($"[PerformanceGateRunner] Updated failure threshold: MinFPS={threshold.MinimumFPS}, MaxFrameTime={threshold.MaximumFrameTimeMs}ms");
        }
        
        /// <summary>
        /// Gets the current failure threshold configuration.
        /// </summary>
        public PerformanceThreshold GetFailureThreshold()
        {
            return _failureThreshold;
        }
        
        #region Private Methods
        
        private void RegisterDefaultGates()
        {
            try
            {
                // Register default gates that implement the CI/CD requirements
                
                // Register Unit Test Gate (Requirement 4.1)
                RegisterGate(new UnitTestGate());
                
                // Register Burst Compilation Gate (Requirement 4.2)
                RegisterGate(new BurstCompilationGate());
                
                // Register Performance Stub Gate (Requirement 4.3-4.4)
                RegisterGate(new PerformanceStubGate());
                
                // Register Performance Profiling Gate (Requirement 4.3)
                RegisterGate(new PerformanceProfilingGate());
                
                UnityEngine.Debug.Log("[PerformanceGateRunner] Registered default CI/CD performance gates");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceGateRunner] Failed to register default gates: {ex.Message}");
            }
        }
        
        private void RecordExecution(PerformanceGateResult result)
        {
            try
            {
                var record = new PerformanceGateExecutionRecord
                {
                    ExecutedAt = result.ExecutedAt,
                    Result = result,
                    BuildIdentifier = GetBuildIdentifier(),
                    Environment = GetEnvironmentInfo(),
                    UnityVersion = Application.unityVersion,
                    SystemInfo = GetSystemInfo()
                };
                
                _executionHistory.Add(record);
                
                // Keep only the last 100 records to prevent memory issues
                if (_executionHistory.Count > 100)
                {
                    _executionHistory.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceGateRunner] Failed to record execution: {ex.Message}");
            }
        }
        
        private string GetBuildIdentifier()
        {
            // Try to get build identifier from environment or generate one
            var buildId = Environment.GetEnvironmentVariable("BUILD_ID") ?? 
                         Environment.GetEnvironmentVariable("CI_BUILD_ID") ??
                         $"local-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
            
            return buildId;
        }
        
        private string GetEnvironmentInfo()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
            {
                return "CI";
            }
            
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("UNITY_BATCH_MODE")))
            {
                return "Batch";
            }
            
            return "Editor";
        }
        
        private Dictionary<string, string> GetSystemInfo()
        {
            return new Dictionary<string, string>
            {
                ["Platform"] = Application.platform.ToString(),
                ["UnityVersion"] = Application.unityVersion,
                ["SystemMemorySize"] = SystemInfo.systemMemorySize.ToString(),
                ["GraphicsDeviceName"] = SystemInfo.graphicsDeviceName,
                ["GraphicsMemorySize"] = SystemInfo.graphicsMemorySize.ToString(),
                ["ProcessorType"] = SystemInfo.processorType,
                ["ProcessorCount"] = SystemInfo.processorCount.ToString(),
                ["OperatingSystem"] = SystemInfo.operatingSystem
            };
        }
        
        private string GenerateReportContent()
        {
            var report = new StringBuilder();
            
            report.AppendLine("# Performance Gate Report");
            report.AppendLine();
            report.AppendLine($"**Generated**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine($"**Unity Version**: {Application.unityVersion}");
            report.AppendLine($"**Environment**: {GetEnvironmentInfo()}");
            report.AppendLine();
            
            // Registered gates summary
            report.AppendLine("## Registered Gates");
            report.AppendLine();
            
            if (_registeredGates.Any())
            {
                report.AppendLine("| Gate Name | Priority | Critical | Expected Time | Description |");
                report.AppendLine("|-----------|----------|----------|---------------|-------------|");
                
                foreach (var gate in _registeredGates.Values.OrderByDescending(g => g.Priority))
                {
                    var critical = gate.IsCritical ? "Yes" : "No";
                    var expectedTime = gate.ExpectedExecutionTime.TotalSeconds > 0 ? 
                        $"{gate.ExpectedExecutionTime.TotalSeconds:F1}s" : "Unknown";
                    
                    report.AppendLine($"| {gate.Name} | {gate.Priority} | {critical} | {expectedTime} | {gate.Description} |");
                }
            }
            else
            {
                report.AppendLine("No performance gates are currently registered.");
            }
            
            report.AppendLine();
            
            // Execution history
            report.AppendLine("## Recent Execution History");
            report.AppendLine();
            
            var recentExecutions = _executionHistory
                .OrderByDescending(e => e.ExecutedAt)
                .Take(10)
                .ToList();
            
            if (recentExecutions.Any())
            {
                report.AppendLine("| Executed At | Gate(s) | Result | Duration | Build ID |");
                report.AppendLine("|-------------|---------|--------|----------|----------|");
                
                foreach (var execution in recentExecutions)
                {
                    var result = execution.Result.Success ? "✅ PASS" : "❌ FAIL";
                    var duration = $"{execution.Result.ExecutionTime.TotalSeconds:F1}s";
                    
                    report.AppendLine($"| {execution.ExecutedAt:yyyy-MM-dd HH:mm:ss} | {execution.Result.GateName} | {result} | {duration} | {execution.BuildIdentifier} |");
                }
            }
            else
            {
                report.AppendLine("No execution history available.");
            }
            
            report.AppendLine();
            
            // Current thresholds
            report.AppendLine("## Performance Thresholds");
            report.AppendLine();
            report.AppendLine($"- **Minimum FPS**: {_failureThreshold.MinimumFPS}");
            report.AppendLine($"- **Maximum Frame Time**: {_failureThreshold.MaximumFrameTimeMs} ms");
            report.AppendLine($"- **Maximum Memory Usage**: {_failureThreshold.MaximumMemoryUsageMB} MB");
            report.AppendLine($"- **Maximum Build Time**: {_failureThreshold.MaximumBuildTimeMinutes} minutes");
            report.AppendLine($"- **Maximum Test Time**: {_failureThreshold.MaximumTestTimeMinutes} minutes");
            report.AppendLine($"- **Fail on Warnings**: {(_failureThreshold.FailOnWarnings ? "Yes" : "No")}");
            
            if (_failureThreshold.CustomThresholds.Any())
            {
                report.AppendLine();
                report.AppendLine("### Custom Thresholds");
                foreach (var threshold in _failureThreshold.CustomThresholds)
                {
                    report.AppendLine($"- **{threshold.Key}**: {threshold.Value}");
                }
            }
            
            report.AppendLine();
            
            // System information
            report.AppendLine("## System Information");
            report.AppendLine();
            
            var systemInfo = GetSystemInfo();
            foreach (var info in systemInfo)
            {
                report.AppendLine($"- **{info.Key}**: {info.Value}");
            }
            
            return report.ToString();
        }
        
        #endregion
    }
}