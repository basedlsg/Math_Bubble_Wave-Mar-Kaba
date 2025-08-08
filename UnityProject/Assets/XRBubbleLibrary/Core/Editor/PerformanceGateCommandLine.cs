using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XRBubbleLibrary.Core.Editor
{
    /// <summary>
    /// Command-line interface for running performance gates in CI/CD environments.
    /// Provides static methods that can be called from Unity's batch mode for automated builds.
    /// Implements Requirement 4.6: CI/CD pipeline integration with build blocking.
    /// </summary>
    public static class PerformanceGateCommandLine
    {
        /// <summary>
        /// Main entry point for running all performance gates from command line.
        /// This method is designed to be called from Unity batch mode in CI/CD pipelines.
        /// </summary>
        [MenuItem("XR Bubble Library/CI-CD/Run All Performance Gates")]
        public static void RunAllGatesFromCommandLine()
        {
            try
            {
                Debug.Log("[PerformanceGateCommandLine] Starting CI/CD performance gate execution...");
                
                // Initialize the performance gate runner
                var runner = PerformanceGateRunner.Instance;
                
                // Configure for CI environment
                ConfigureForCIEnvironment(runner);
                
                // Run all performance gates
                var result = runner.RunAllGates();
                
                // Generate comprehensive report
                var reportPath = runner.GeneratePerformanceReport();
                Debug.Log($"[PerformanceGateCommandLine] Performance report generated: {reportPath}");
                
                // Log detailed results
                LogDetailedResults(result);
                
                // Exit with appropriate code
                if (result.Success)
                {
                    Debug.Log("[PerformanceGateCommandLine] All performance gates PASSED - build can proceed");
                    EditorApplication.Exit(0);
                }
                else
                {
                    Debug.LogError("[PerformanceGateCommandLine] Performance gates FAILED - blocking build");
                    LogFailureDetails(result);
                    EditorApplication.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerformanceGateCommandLine] Fatal error during gate execution: {ex.Message}");
                Debug.LogError($"[PerformanceGateCommandLine] Stack trace: {ex.StackTrace}");
                EditorApplication.Exit(2);
            }
        }
        
        /// <summary>
        /// Runs a specific performance gate from command line.
        /// Usage: Unity -executeMethod PerformanceGateCommandLine.RunSpecificGateFromCommandLine -gateName "Unit Tests"
        /// </summary>
        public static void RunSpecificGateFromCommandLine()
        {
            try
            {
                var gateName = GetCommandLineArgument("-gateName");
                if (string.IsNullOrEmpty(gateName))
                {
                    Debug.LogError("[PerformanceGateCommandLine] Gate name not specified. Use -gateName argument.");
                    EditorApplication.Exit(1);
                    return;
                }
                
                Debug.Log($"[PerformanceGateCommandLine] Running specific gate: {gateName}");
                
                var runner = PerformanceGateRunner.Instance;
                ConfigureForCIEnvironment(runner);
                
                var result = runner.RunSpecificGate(gateName);
                
                LogDetailedResults(result);
                
                if (result.Success)
                {
                    Debug.Log($"[PerformanceGateCommandLine] Gate '{gateName}' PASSED");
                    EditorApplication.Exit(0);
                }
                else
                {
                    Debug.LogError($"[PerformanceGateCommandLine] Gate '{gateName}' FAILED");
                    LogFailureDetails(result);
                    EditorApplication.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerformanceGateCommandLine] Fatal error: {ex.Message}");
                EditorApplication.Exit(2);
            }
        }
        
        /// <summary>
        /// Validates the configuration of all performance gates without running them.
        /// Useful for pre-flight checks in CI/CD pipelines.
        /// </summary>
        [MenuItem("XR Bubble Library/CI-CD/Validate Gate Configuration")]
        public static void ValidateGateConfigurationFromCommandLine()
        {
            try
            {
                Debug.Log("[PerformanceGateCommandLine] Validating performance gate configuration...");
                
                var runner = PerformanceGateRunner.Instance;
                var validation = runner.ValidateConfiguration();
                
                Debug.Log($"[PerformanceGateCommandLine] Configuration validation completed:");
                Debug.Log($"  - Gates validated: {validation.GatesValidated}");
                Debug.Log($"  - Issues found: {validation.Issues.Count}");
                Debug.Log($"  - Warnings: {validation.Warnings.Count}");
                
                foreach (var issue in validation.Issues)
                {
                    Debug.LogError($"  - Issue: {issue}");
                }
                
                foreach (var warning in validation.Warnings)
                {
                    Debug.LogWarning($"  - Warning: {warning}");
                }
                
                if (validation.IsValid)
                {
                    Debug.Log("[PerformanceGateCommandLine] Configuration validation PASSED");
                    EditorApplication.Exit(0);
                }
                else
                {
                    Debug.LogError("[PerformanceGateCommandLine] Configuration validation FAILED");
                    EditorApplication.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerformanceGateCommandLine] Configuration validation error: {ex.Message}");
                EditorApplication.Exit(2);
            }
        }
        
        /// <summary>
        /// Generates a performance report without running gates.
        /// Useful for generating reports from previous execution history.
        /// </summary>
        public static void GenerateReportFromCommandLine()
        {
            try
            {
                Debug.Log("[PerformanceGateCommandLine] Generating performance report...");
                
                var runner = PerformanceGateRunner.Instance;
                var reportPath = runner.GeneratePerformanceReport();
                
                Debug.Log($"[PerformanceGateCommandLine] Report generated successfully: {reportPath}");
                
                // Also log report path to a file for CI systems to pick up
                var reportInfoPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "performance_report_path.txt");
                File.WriteAllText(reportInfoPath, reportPath);
                
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerformanceGateCommandLine] Report generation failed: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }
        
        #region Private Methods
        
        private static void ConfigureForCIEnvironment(PerformanceGateRunner runner)
        {
            // Set CI-appropriate performance thresholds
            var ciThresholds = new PerformanceThreshold
            {
                MinimumFPS = GetFloatCommandLineArgument("-minFPS", 60.0f),
                MaximumFrameTimeMs = GetFloatCommandLineArgument("-maxFrameTime", 16.67f),
                MaximumMemoryUsageMB = GetFloatCommandLineArgument("-maxMemory", 1024.0f),
                MaximumBuildTimeMinutes = GetFloatCommandLineArgument("-maxBuildTime", 10.0f),
                MaximumTestTimeMinutes = GetFloatCommandLineArgument("-maxTestTime", 5.0f),
                FailOnWarnings = GetBoolCommandLineArgument("-failOnWarnings", false),
                CustomThresholds = new System.Collections.Generic.Dictionary<string, float>
                {
                    ["MaxGCAllocPerFrame"] = GetFloatCommandLineArgument("-maxGCAlloc", 1.0f),
                    ["MaxDrawCalls"] = GetFloatCommandLineArgument("-maxDrawCalls", 500.0f),
                    ["MaxTriangles"] = GetFloatCommandLineArgument("-maxTriangles", 100000.0f),
                    ["MaxTextureMemoryMB"] = GetFloatCommandLineArgument("-maxTextureMemory", 256.0f)
                }
            };
            
            runner.SetFailureThreshold(ciThresholds);
            
            Debug.Log("[PerformanceGateCommandLine] Configured CI environment thresholds:");
            Debug.Log($"  - Minimum FPS: {ciThresholds.MinimumFPS}");
            Debug.Log($"  - Maximum Frame Time: {ciThresholds.MaximumFrameTimeMs}ms");
            Debug.Log($"  - Maximum Memory: {ciThresholds.MaximumMemoryUsageMB}MB");
            Debug.Log($"  - Fail on Warnings: {ciThresholds.FailOnWarnings}");
        }
        
        private static void LogDetailedResults(PerformanceGateResult result)
        {
            Debug.Log($"[PerformanceGateCommandLine] === Performance Gate Results ===");
            Debug.Log($"  Gate: {result.GateName}");
            Debug.Log($"  Success: {result.Success}");
            Debug.Log($"  Executed At: {result.ExecutedAt:yyyy-MM-dd HH:mm:ss} UTC");
            Debug.Log($"  Execution Time: {result.ExecutionTime.TotalSeconds:F2} seconds");
            Debug.Log($"  Summary: {result.Summary}");
            
            if (result.PerformanceMetrics.Count > 0)
            {
                Debug.Log("  Performance Metrics:");
                foreach (var metric in result.PerformanceMetrics)
                {
                    Debug.Log($"    - {metric.Key}: {metric.Value}");
                }
            }
            
            if (result.WarningMessages.Count > 0)
            {
                Debug.Log("  Warnings:");
                foreach (var warning in result.WarningMessages)
                {
                    Debug.LogWarning($"    - {warning}");
                }
            }
            
            if (result.GateResults.Count > 0)
            {
                Debug.Log("  Individual Gate Results:");
                foreach (var gateResult in result.GateResults)
                {
                    var status = gateResult.Success ? "PASS" : "FAIL";
                    var critical = gateResult.IsCritical ? " (CRITICAL)" : "";
                    Debug.Log($"    - {gateResult.GateName}: {status}{critical} ({gateResult.ExecutionTime.TotalSeconds:F1}s)");
                    
                    if (!gateResult.Success && gateResult.Errors.Count > 0)
                    {
                        foreach (var error in gateResult.Errors)
                        {
                            Debug.LogError($"      Error: {error}");
                        }
                    }
                }
            }
        }
        
        private static void LogFailureDetails(PerformanceGateResult result)
        {
            Debug.LogError("[PerformanceGateCommandLine] === FAILURE DETAILS ===");
            
            if (result.ErrorMessages.Count > 0)
            {
                Debug.LogError("  Error Messages:");
                foreach (var error in result.ErrorMessages)
                {
                    Debug.LogError($"    - {error}");
                }
            }
            
            // Log critical failures specifically
            var criticalFailures = result.GateResults?.FindAll(g => !g.Success && g.IsCritical);
            if (criticalFailures != null && criticalFailures.Count > 0)
            {
                Debug.LogError("  Critical Gate Failures:");
                foreach (var failure in criticalFailures)
                {
                    Debug.LogError($"    - {failure.GateName}: {failure.Message}");
                }
            }
            
            Debug.LogError("[PerformanceGateCommandLine] Build BLOCKED due to performance gate failures");
        }
        
        private static string GetCommandLineArgument(string argumentName)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return null;
        }
        
        private static float GetFloatCommandLineArgument(string argumentName, float defaultValue)
        {
            var value = GetCommandLineArgument(argumentName);
            if (float.TryParse(value, out var result))
            {
                return result;
            }
            return defaultValue;
        }
        
        private static bool GetBoolCommandLineArgument(string argumentName, bool defaultValue)
        {
            var value = GetCommandLineArgument(argumentName);
            if (bool.TryParse(value, out var result))
            {
                return result;
            }
            return defaultValue;
        }
        
        #endregion
    }
}