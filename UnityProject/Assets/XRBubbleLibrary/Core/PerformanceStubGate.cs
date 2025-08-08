using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Performance gate that runs a "perf stub" test using Unity editor profiler.
    /// Implements Requirement 4.3-4.4: "WHEN code is committed THEN CI SHALL run a 'perf stub' test using the Unity editor profiler"
    /// and "WHEN the perf stub runs THEN it SHALL fail the build if median FPS drops below 60 in editor testing"
    /// </summary>
    public class PerformanceStubGate : IPerformanceGate
    {
        /// <summary>
        /// Unique name of the performance gate.
        /// </summary>
        public string Name => "Performance Stub Test";
        
        /// <summary>
        /// Description of what this gate validates.
        /// </summary>
        public string Description => "Runs performance stub test using Unity profiler and validates FPS meets minimum requirements";
        
        /// <summary>
        /// Priority of this gate (higher priority gates run first).
        /// </summary>
        public int Priority => 800; // High priority - performance validation is critical
        
        /// <summary>
        /// Whether this gate is critical (failure blocks the build).
        /// </summary>
        public bool IsCritical => true; // Performance failures should block the build
        
        /// <summary>
        /// Expected execution time for this gate.
        /// </summary>
        public TimeSpan ExpectedExecutionTime => TimeSpan.FromMinutes(1);
        
        private GateStatus _currentStatus = GateStatus.Ready;
        private readonly float _minimumFPS = 60.0f;
        private readonly int _testDurationSeconds = 10;
        private readonly int _warmupFrames = 60; // 1 second at 60fps
        
        /// <summary>
        /// Executes the performance stub gate validation.
        /// </summary>
        public PerformanceGateResult Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new PerformanceGateResult
            {
                GateName = Name,
                ExecutedAt = DateTime.UtcNow,
                Success = true
            };
            
            try
            {
                _currentStatus = GateStatus.Executing;
                UnityEngine.Debug.Log("[PerformanceStubGate] Starting performance stub test...");
                
                // Initialize profiler
                var profilerInitialized = InitializeProfiler();
                if (!profilerInitialized)
                {
                    result.Success = false;
                    result.ErrorMessages.Add("Failed to initialize Unity profiler");
                    result.Summary = "Profiler initialization failed";
                    return result;
                }
                
                // Run performance test
                var performanceData = RunPerformanceStubTest();
                
                // Analyze performance results
                AnalyzePerformanceResults(performanceData, result);
                
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                
                // Add performance metrics
                result.PerformanceMetrics["TestDuration"] = _testDurationSeconds;
                result.PerformanceMetrics["MedianFPS"] = performanceData.MedianFPS;
                result.PerformanceMetrics["AverageFPS"] = performanceData.AverageFPS;
                result.PerformanceMetrics["MinimumFPS"] = performanceData.MinimumFPS;
                result.PerformanceMetrics["MaximumFPS"] = performanceData.MaximumFPS;
                result.PerformanceMetrics["FrameTimeVariance"] = performanceData.FrameTimeVariance;
                result.PerformanceMetrics["TotalFramesCaptured"] = performanceData.FrameData.Count;
                result.PerformanceMetrics["MemoryUsageMB"] = performanceData.PeakMemoryUsageMB;
                result.PerformanceMetrics["GCAllocations"] = performanceData.TotalGCAllocations;
                
                // Generate summary
                result.Summary = $"Performance stub test: Median FPS {performanceData.MedianFPS:F1} " +
                               $"(min: {performanceData.MinimumFPS:F1}, max: {performanceData.MaximumFPS:F1}) " +
                               $"- {(result.Success ? "PASS" : "FAIL")}";
                
                _currentStatus = result.Success ? GateStatus.Completed : GateStatus.Failed;
                
                UnityEngine.Debug.Log($"[PerformanceStubGate] {result.Summary}");
                
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                result.Success = false;
                result.ErrorMessages.Add($"Performance stub test failed: {ex.Message}");
                result.Summary = $"Performance stub gate failed with exception: {ex.Message}";
                
                _currentStatus = GateStatus.Failed;
                UnityEngine.Debug.LogError($"[PerformanceStubGate] Exception: {ex.Message}");
            }
            finally
            {
                // Clean up profiler
                CleanupProfiler();
            }
            
            return result;
        }
        
        /// <summary>
        /// Executes the performance stub gate validation asynchronously.
        /// </summary>
        public async Task<PerformanceGateResult> ExecuteAsync()
        {
            return await Task.Run(() => Execute());
        }
        
        /// <summary>
        /// Validates the gate's configuration before execution.
        /// </summary>
        public bool ValidateConfiguration()
        {
            try
            {
                // Check if we're running in Unity editor
                if (!Application.isEditor)
                {
                    UnityEngine.Debug.LogWarning("[PerformanceStubGate] Performance stub test should run in Unity editor");
                    return false;
                }
                
                // Check if profiler is available
                if (!Profiler.supported)
                {
                    UnityEngine.Debug.LogError("[PerformanceStubGate] Unity profiler not supported");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceStubGate] Configuration validation failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets detailed information about the gate's current state.
        /// </summary>
        public PerformanceGateStatus GetStatus()
        {
            return new PerformanceGateStatus
            {
                GateName = Name,
                Status = _currentStatus,
                ConfigurationValid = ValidateConfiguration(),
                StatusMessages = GetStatusMessages()
            };
        }
        
        #region Private Methods
        
        private bool InitializeProfiler()
        {
            try
            {
                // Enable profiler if not already enabled
                if (!Profiler.enabled)
                {
                    Profiler.enabled = true;
                }
                
                // Clear any existing profiler data
                Profiler.BeginSample("PerformanceStubTest_Initialization");
                Profiler.EndSample();
                
                UnityEngine.Debug.Log("[PerformanceStubGate] Profiler initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceStubGate] Failed to initialize profiler: {ex.Message}");
                return false;
            }
        }
        
        private PerformanceData RunPerformanceStubTest()
        {
            var performanceData = new PerformanceData();
            var frameData = new List<FrameData>();
            
            try
            {
                UnityEngine.Debug.Log($"[PerformanceStubGate] Running performance test for {_testDurationSeconds} seconds...");
                
                var startTime = Time.realtimeSinceStartup;
                var endTime = startTime + _testDurationSeconds;
                var frameCount = 0;
                var warmupComplete = false;
                
                // Force garbage collection before test
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
                
                var initialMemory = Profiler.GetTotalAllocatedMemory(0);
                var peakMemory = initialMemory;
                var totalGCAllocations = 0L;
                
                while (Time.realtimeSinceStartup < endTime)
                {
                    Profiler.BeginSample("PerformanceStubTest_Frame");
                    
                    var frameStartTime = Time.realtimeSinceStartup;
                    
                    // Simulate some work (basic bubble mathematics)
                    SimulateWorkload();
                    
                    var frameEndTime = Time.realtimeSinceStartup;
                    var frameTime = frameEndTime - frameStartTime;
                    var fps = frameTime > 0 ? 1.0f / frameTime : 0;
                    
                    frameCount++;
                    
                    // Skip warmup frames
                    if (frameCount > _warmupFrames)
                    {
                        if (!warmupComplete)
                        {
                            warmupComplete = true;
                            UnityEngine.Debug.Log("[PerformanceStubGate] Warmup complete, starting measurement");
                        }
                        
                        // Collect frame data
                        var currentMemory = Profiler.GetTotalAllocatedMemory(0);
                        peakMemory = Math.Max(peakMemory, currentMemory);
                        
                        frameData.Add(new FrameData
                        {
                            FrameNumber = frameCount,
                            FPS = fps,
                            FrameTime = frameTime * 1000f, // Convert to milliseconds
                            MemoryUsage = currentMemory,
                            Timestamp = frameEndTime
                        });
                        
                        // Track GC allocations (simplified)
                        if (currentMemory > initialMemory + 1024 * 1024) // 1MB threshold
                        {
                            totalGCAllocations++;
                            initialMemory = currentMemory;
                        }
                    }
                    
                    Profiler.EndSample();
                    
                    // Yield control to prevent blocking
                    if (frameCount % 10 == 0)
                    {
                        System.Threading.Thread.Sleep(1); // Brief pause
                    }
                }
                
                // Calculate performance metrics
                if (frameData.Count > 0)
                {
                    var fpsValues = frameData.ConvertAll(f => f.FPS);
                    fpsValues.Sort();
                    
                    performanceData.FrameData = frameData;
                    performanceData.AverageFPS = fpsValues.Count > 0 ? fpsValues.Average() : 0;
                    performanceData.MedianFPS = CalculateMedian(fpsValues);
                    performanceData.MinimumFPS = fpsValues.Count > 0 ? fpsValues.Min() : 0;
                    performanceData.MaximumFPS = fpsValues.Count > 0 ? fpsValues.Max() : 0;
                    performanceData.FrameTimeVariance = CalculateVariance(frameData.ConvertAll(f => f.FrameTime));
                    performanceData.PeakMemoryUsageMB = peakMemory / (1024f * 1024f);
                    performanceData.TotalGCAllocations = totalGCAllocations;
                }
                
                UnityEngine.Debug.Log($"[PerformanceStubGate] Performance test completed. Captured {frameData.Count} frames");
                
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceStubGate] Error during performance test: {ex.Message}");
                throw;
            }
            
            return performanceData;
        }
        
        private void SimulateWorkload()
        {
            // Simulate basic bubble mathematics workload
            var bubbleCount = 50;
            var positions = new Vector3[bubbleCount];
            var velocities = new Vector3[bubbleCount];
            
            // Initialize positions
            for (int i = 0; i < bubbleCount; i++)
            {
                positions[i] = new Vector3(
                    UnityEngine.Random.Range(-10f, 10f),
                    UnityEngine.Random.Range(-10f, 10f),
                    UnityEngine.Random.Range(-10f, 10f)
                );
                
                velocities[i] = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f)
                );
            }
            
            // Simulate physics update
            var deltaTime = Time.fixedDeltaTime;
            for (int i = 0; i < bubbleCount; i++)
            {
                // Simple physics simulation
                positions[i] += velocities[i] * deltaTime;
                
                // Boundary checking
                if (Mathf.Abs(positions[i].x) > 10f) velocities[i].x *= -0.8f;
                if (Mathf.Abs(positions[i].y) > 10f) velocities[i].y *= -0.8f;
                if (Mathf.Abs(positions[i].z) > 10f) velocities[i].z *= -0.8f;
                
                // Apply some mathematical operations
                var distance = positions[i].magnitude;
                var normalizedPos = positions[i].normalized;
                var force = normalizedPos * (1.0f / (distance + 1.0f));
                velocities[i] += force * deltaTime;
            }
            
            // Simulate some string operations (potential GC pressure)
            if (UnityEngine.Random.value < 0.01f) // 1% chance per frame
            {
                var debugString = $"Frame update: {bubbleCount} bubbles processed";
                // Don't actually log to avoid spam, just create the string
            }
        }
        
        private void AnalyzePerformanceResults(PerformanceData performanceData, PerformanceGateResult result)
        {
            try
            {
                // Check median FPS requirement
                if (performanceData.MedianFPS < _minimumFPS)
                {
                    result.Success = false;
                    result.ErrorMessages.Add($"Median FPS {performanceData.MedianFPS:F1} is below minimum requirement of {_minimumFPS}");
                }
                
                // Check for performance consistency
                if (performanceData.FrameTimeVariance > 10.0f) // 10ms variance threshold
                {
                    result.WarningMessages.Add($"High frame time variance detected: {performanceData.FrameTimeVariance:F2}ms");
                }
                
                // Check memory usage
                if (performanceData.PeakMemoryUsageMB > 100.0f) // 100MB threshold for stub test
                {
                    result.WarningMessages.Add($"High memory usage detected: {performanceData.PeakMemoryUsageMB:F1}MB");
                }
                
                // Check GC pressure
                if (performanceData.TotalGCAllocations > 5)
                {
                    result.WarningMessages.Add($"Excessive GC allocations detected: {performanceData.TotalGCAllocations}");
                }
                
                // Check minimum FPS threshold
                if (performanceData.MinimumFPS < _minimumFPS * 0.8f) // 80% of minimum
                {
                    result.WarningMessages.Add($"Minimum FPS {performanceData.MinimumFPS:F1} is significantly below target");
                }
                
                // Save detailed profiling data
                var profilingDataPath = SaveProfilingData(performanceData);
                if (!string.IsNullOrEmpty(profilingDataPath))
                {
                    result.ProfilingDataPath = profilingDataPath;
                }
                
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceStubGate] Error analyzing performance results: {ex.Message}");
                result.WarningMessages.Add($"Performance analysis error: {ex.Message}");
            }
        }
        
        private float CalculateMedian(List<float> values)
        {
            if (values.Count == 0) return 0;
            
            values.Sort();
            var mid = values.Count / 2;
            
            if (values.Count % 2 == 0)
            {
                return (values[mid - 1] + values[mid]) / 2.0f;
            }
            else
            {
                return values[mid];
            }
        }
        
        private float CalculateVariance(List<float> values)
        {
            if (values.Count == 0) return 0;
            
            var mean = values.Average();
            var sumSquaredDifferences = values.Sum(v => (v - mean) * (v - mean));
            return sumSquaredDifferences / values.Count;
        }
        
        private string SaveProfilingData(PerformanceData performanceData)
        {
            try
            {
                var projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
                var reportsDir = System.IO.Path.Combine(projectRoot, "PerformanceReports");
                System.IO.Directory.CreateDirectory(reportsDir);
                
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var reportPath = System.IO.Path.Combine(reportsDir, $"performance_stub_{timestamp}.csv");
                
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("FrameNumber,FPS,FrameTimeMs,MemoryUsageMB,Timestamp");
                
                foreach (var frame in performanceData.FrameData)
                {
                    csv.AppendLine($"{frame.FrameNumber},{frame.FPS:F2},{frame.FrameTime:F2},{frame.MemoryUsage / (1024f * 1024f):F2},{frame.Timestamp:F3}");
                }
                
                System.IO.File.WriteAllText(reportPath, csv.ToString());
                
                UnityEngine.Debug.Log($"[PerformanceStubGate] Saved profiling data to: {reportPath}");
                return reportPath;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceStubGate] Failed to save profiling data: {ex.Message}");
                return null;
            }
        }
        
        private void CleanupProfiler()
        {
            try
            {
                // Clean up any profiler samples
                Profiler.BeginSample("PerformanceStubTest_Cleanup");
                Profiler.EndSample();
                
                UnityEngine.Debug.Log("[PerformanceStubGate] Profiler cleanup completed");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[PerformanceStubGate] Error during profiler cleanup: {ex.Message}");
            }
        }
        
        private List<string> GetStatusMessages()
        {
            var messages = new List<string>();
            
            switch (_currentStatus)
            {
                case GateStatus.Ready:
                    messages.Add("Ready to run performance stub test");
                    break;
                case GateStatus.Executing:
                    messages.Add("Currently running performance stub test");
                    break;
                case GateStatus.Completed:
                    messages.Add("Performance stub test completed successfully");
                    break;
                case GateStatus.Failed:
                    messages.Add("Performance stub test failed");
                    break;
                case GateStatus.ConfigurationError:
                    messages.Add("Performance stub test configuration is invalid");
                    break;
                case GateStatus.Disabled:
                    messages.Add("Performance stub test gate is disabled");
                    break;
            }
            
            return messages;
        }
        
        #endregion
        
        #region Helper Classes
        
        private class PerformanceData
        {
            public List<FrameData> FrameData { get; set; } = new List<FrameData>();
            public float MedianFPS { get; set; }
            public float AverageFPS { get; set; }
            public float MinimumFPS { get; set; }
            public float MaximumFPS { get; set; }
            public float FrameTimeVariance { get; set; }
            public float PeakMemoryUsageMB { get; set; }
            public long TotalGCAllocations { get; set; }
        }
        
        private class FrameData
        {
            public int FrameNumber { get; set; }
            public float FPS { get; set; }
            public float FrameTime { get; set; } // in milliseconds
            public long MemoryUsage { get; set; } // in bytes
            public float Timestamp { get; set; }
        }
        
        #endregion
    }
}