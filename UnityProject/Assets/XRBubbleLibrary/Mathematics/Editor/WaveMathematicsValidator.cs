using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using XRBubbleLibrary.Core;

namespace XRBubbleLibrary.Mathematics.Editor
{
    /// <summary>
    /// Editor validation tools for wave mathematics system.
    /// Validates both core functionality and experimental feature flagging.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public static class WaveMathematicsValidator
    {
        /// <summary>
        /// Menu item to validate wave mathematics system configuration.
        /// </summary>
        [MenuItem("XR Bubble Library/Validate Wave Mathematics System", priority = 150)]
        public static void ValidateWaveMathematicsSystem()
        {
            Debug.Log("🌊 Starting Wave Mathematics System Validation...");
            
            var results = new List<string>();
            bool allTestsPassed = true;
            
            // Test 1: Core wave mathematics functionality
            if (TestCoreWaveMathematics())
            {
                results.Add("✅ Core wave mathematics functions working correctly");
            }
            else
            {
                results.Add("❌ Core wave mathematics functions failed");
                allTestsPassed = false;
            }
            
            // Test 2: Advanced wave system availability
            if (TestAdvancedWaveSystemAvailability())
            {
                results.Add("✅ Advanced wave system properly configured");
            }
            else
            {
                results.Add("❌ Advanced wave system configuration issues");
                allTestsPassed = false;
            }
            
            // Test 3: Compiler flag consistency
            if (TestCompilerFlagConsistency())
            {
                results.Add("✅ Compiler flags consistent with implementation");
            }
            else
            {
                results.Add("❌ Compiler flag inconsistencies detected");
                allTestsPassed = false;
            }
            
            // Test 4: Feature gate validation
            if (TestFeatureGateValidation())
            {
                results.Add("✅ Feature gates working correctly");
            }
            else
            {
                results.Add("❌ Feature gate validation failed");
                allTestsPassed = false;
            }
            
            // Test 5: Performance metrics
            if (TestPerformanceMetrics())
            {
                results.Add("✅ Performance metrics system functional");
            }
            else
            {
                results.Add("❌ Performance metrics system issues");
                allTestsPassed = false;
            }
            
            // Display results
            string summary = allTestsPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED";
            Debug.Log($"🌊 Wave Mathematics Validation Complete: {summary}");
            
            foreach (var result in results)
            {
                Debug.Log($"   {result}");
            }
            
            // Additional information about current configuration
            LogCurrentConfiguration();
        }
        
        /// <summary>
        /// Test core wave mathematics functionality that should always work.
        /// </summary>
        private static bool TestCoreWaveMathematics()
        {
            try
            {
                // Test WavePatternGenerator
                var positions = WavePatternGenerator.GenerateWavePattern(8, 2.0f, WaveParameters.Default);
                if (positions == null || positions.Length != 8)
                {
                    Debug.LogError("WavePatternGenerator.GenerateWavePattern failed");
                    return false;
                }
                
                // Test wave interference calculation
                var sources = new List<WaveSource>
                {
                    new WaveSource(new float3(0, 0, 0), 1.0f, 0.5f, 0f),
                    new WaveSource(new float3(1, 0, 0), 1.5f, 0.3f, 0f)
                };
                
                float interference = WavePatternGenerator.CalculateWaveInterference(new float3(0.5f, 0, 0), sources);
                if (float.IsNaN(interference) || float.IsInfinity(interference))
                {
                    Debug.LogError("WavePatternGenerator.CalculateWaveInterference returned invalid result");
                    return false;
                }
                
                // Test WaveParameters
                var defaultParams = WaveParameters.Default;
                if (defaultParams.primaryFrequency <= 0 || defaultParams.primaryAmplitude <= 0)
                {
                    Debug.LogError("WaveParameters.Default has invalid values");
                    return false;
                }
                
                // Test WaveSource
                var defaultSource = WaveSource.Default;
                if (defaultSource.frequency <= 0 || defaultSource.amplitude <= 0)
                {
                    Debug.LogError("WaveSource.Default has invalid values");
                    return false;
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Core wave mathematics test failed with exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Test advanced wave system availability and proper flagging.
        /// </summary>
        private static bool TestAdvancedWaveSystemAvailability()
        {
            try
            {
                // Create a test GameObject with AdvancedWaveSystem
                var testObject = new GameObject("TestAdvancedWaveSystem");
                var advancedWaveSystem = testObject.AddComponent<AdvancedWaveSystem>();
                
                if (advancedWaveSystem == null)
                {
                    Debug.LogError("Failed to create AdvancedWaveSystem component");
                    Object.DestroyImmediate(testObject);
                    return false;
                }
                
                // Test that performance metrics are available
                var metrics = advancedWaveSystem.GetPerformanceMetrics();
                if (metrics.lastComputeTimeMs < 0)
                {
                    Debug.LogError("Invalid performance metrics returned");
                    Object.DestroyImmediate(testObject);
                    return false;
                }
                
                // Test basic method calls don't throw exceptions
                try
                {
                    advancedWaveSystem.RegisterBubble(1, new float3(0, 0, 0));
                    advancedWaveSystem.UnregisterBubble(1);
                    advancedWaveSystem.SetAIOptimization(false);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"AdvancedWaveSystem basic methods failed: {ex.Message}");
                    Object.DestroyImmediate(testObject);
                    return false;
                }
                
                Object.DestroyImmediate(testObject);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Advanced wave system test failed with exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Test compiler flag consistency with actual implementation.
        /// </summary>
        private static bool TestCompilerFlagConsistency()
        {
            try
            {
                bool flagEnabled = CompilerFlags.IsFeatureEnabled(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
                
#if EXP_ADVANCED_WAVE
                if (!flagEnabled)
                {
                    Debug.LogError("EXP_ADVANCED_WAVE is defined but CompilerFlags reports it as disabled");
                    return false;
                }
                
                Debug.Log("Advanced wave algorithms are enabled (EXP_ADVANCED_WAVE defined)");
#else
                if (flagEnabled)
                {
                    Debug.LogError("EXP_ADVANCED_WAVE is not defined but CompilerFlags reports it as enabled");
                    return false;
                }
                
                Debug.Log("Advanced wave algorithms are disabled (EXP_ADVANCED_WAVE not defined)");
#endif
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Compiler flag consistency test failed with exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Test feature gate validation system.
        /// </summary>
        private static bool TestFeatureGateValidation()
        {
            try
            {
                // Test that ValidateFeatureAccess works correctly
                bool shouldThrow = !CompilerFlags.IsFeatureEnabled(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
                
                try
                {
                    CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
                    
                    if (shouldThrow)
                    {
                        // In editor builds, this should not throw even if disabled
                        // The validation is conditional on UNITY_EDITOR
                        Debug.Log("Feature validation passed (editor build - validation is conditional)");
                    }
                    else
                    {
                        Debug.Log("Feature validation passed (feature is enabled)");
                    }
                }
                catch (System.InvalidOperationException)
                {
                    if (shouldThrow)
                    {
                        Debug.Log("Feature validation correctly threw exception for disabled feature");
                    }
                    else
                    {
                        Debug.LogError("Feature validation threw exception for enabled feature");
                        return false;
                    }
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Feature gate validation test failed with exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Test performance metrics system.
        /// </summary>
        private static bool TestPerformanceMetrics()
        {
            try
            {
                var testObject = new GameObject("TestAdvancedWaveSystem");
                var advancedWaveSystem = testObject.AddComponent<AdvancedWaveSystem>();
                
                var metrics = advancedWaveSystem.GetPerformanceMetrics();
                
                // Validate metrics structure
                if (metrics.lastComputeTimeMs < 0)
                {
                    Debug.LogError("Invalid lastComputeTimeMs in metrics");
                    Object.DestroyImmediate(testObject);
                    return false;
                }
                
                if (metrics.activeBubbleCount < 0)
                {
                    Debug.LogError("Invalid activeBubbleCount in metrics");
                    Object.DestroyImmediate(testObject);
                    return false;
                }
                
                if (metrics.currentBiasFieldSize < 0)
                {
                    Debug.LogError("Invalid currentBiasFieldSize in metrics");
                    Object.DestroyImmediate(testObject);
                    return false;
                }
                
                // Test ToString method
                string metricsString = metrics.ToString();
                if (string.IsNullOrEmpty(metricsString))
                {
                    Debug.LogError("Metrics ToString() returned empty string");
                    Object.DestroyImmediate(testObject);
                    return false;
                }
                
                Object.DestroyImmediate(testObject);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Performance metrics test failed with exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Log current wave mathematics system configuration.
        /// </summary>
        private static void LogCurrentConfiguration()
        {
            Debug.Log("📊 Current Wave Mathematics Configuration:");
            
            // Compiler flags status
            bool advancedWaveEnabled = CompilerFlags.IsFeatureEnabled(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
            bool aiEnabled = CompilerFlags.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION);
            
            Debug.Log($"   Advanced Wave Algorithms: {(advancedWaveEnabled ? "✅ ENABLED" : "❌ DISABLED")}");
            Debug.Log($"   AI Integration: {(aiEnabled ? "✅ ENABLED" : "❌ DISABLED")}");
            
            // Available components
            Debug.Log("   Available Components:");
            Debug.Log("     - WavePatternGenerator: ✅ Always Available");
            Debug.Log("     - WaveParameters: ✅ Always Available");
            Debug.Log("     - WaveSource: ✅ Always Available");
            Debug.Log("     - CymaticsController: ✅ Always Available");
            Debug.Log($"     - AdvancedWaveSystem: {(advancedWaveEnabled ? "✅ Full Implementation" : "⚠️ Stub Implementation")}");
            
            // Performance implications
            if (advancedWaveEnabled && aiEnabled)
            {
                Debug.Log("   ⚠️ Performance Note: Both Advanced Wave and AI features are enabled. Monitor Quest 3 performance carefully.");
            }
            else if (advancedWaveEnabled)
            {
                Debug.Log("   ℹ️ Performance Note: Advanced Wave features enabled without AI integration.");
            }
            else
            {
                Debug.Log("   ℹ️ Performance Note: Using core wave mathematics only - optimal for Quest 3 performance.");
            }
        }
        
        /// <summary>
        /// Menu item to test wave system with different bubble counts.
        /// </summary>
        [MenuItem("XR Bubble Library/Test Wave System Performance", priority = 151)]
        public static void TestWaveSystemPerformance()
        {
            Debug.Log("🚀 Testing Wave System Performance...");
            
            var testCounts = new int[] { 10, 25, 50, 100 };
            var userPosition = new float3(0, 1.6f, 0);
            
            foreach (int count in testCounts)
            {
                var startTime = System.DateTime.Now;
                
                // Test core wave pattern generation
                var positions = WavePatternGenerator.GenerateWavePattern(count, 2.0f, WaveParameters.Default);
                
                var coreTime = (System.DateTime.Now - startTime).TotalMilliseconds;
                
                // Test advanced wave system if available
                double advancedTime = 0;
                if (CompilerFlags.IsFeatureEnabled(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS))
                {
                    var testObject = new GameObject("TestAdvancedWaveSystem");
                    var advancedWaveSystem = testObject.AddComponent<AdvancedWaveSystem>();
                    
                    startTime = System.DateTime.Now;
                    var advancedTask = advancedWaveSystem.GenerateOptimalBubblePositions(count, userPosition);
                    advancedTask.Wait();
                    advancedTime = (System.DateTime.Now - startTime).TotalMilliseconds;
                    
                    Object.DestroyImmediate(testObject);
                }
                
                Debug.Log($"   {count} bubbles - Core: {coreTime:F2}ms, Advanced: {(advancedTime > 0 ? $"{advancedTime:F2}ms" : "N/A")}");
            }
            
            Debug.Log("🚀 Wave System Performance Test Complete");
        }
        
        /// <summary>
        /// Menu item to generate wave mathematics configuration report.
        /// </summary>
        [MenuItem("XR Bubble Library/Generate Wave Mathematics Report", priority = 152)]
        public static void GenerateWaveMathematicsReport()
        {
            var report = GenerateConfigurationReport();
            
            // Create a temporary file to display the report
            var tempPath = System.IO.Path.GetTempFileName() + ".md";
            System.IO.File.WriteAllText(tempPath, report);
            
            // Open the report in the default markdown viewer
            System.Diagnostics.Process.Start(tempPath);
            
            Debug.Log($"📄 Wave Mathematics report generated and opened: {tempPath}");
        }
        
        /// <summary>
        /// Generate a comprehensive wave mathematics configuration report.
        /// </summary>
        private static string GenerateConfigurationReport()
        {
            var report = "# Wave Mathematics System Configuration Report\n\n";
            report += $"**Generated**: {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n";
            report += $"**Unity Version**: {Application.unityVersion}\n\n";
            
            // Compiler flags status
            report += "## Compiler Flags Status\n\n";
            bool advancedWaveEnabled = CompilerFlags.IsFeatureEnabled(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
            bool aiEnabled = CompilerFlags.IsFeatureEnabled(ExperimentalFeature.AI_INTEGRATION);
            
            report += $"- **EXP_ADVANCED_WAVE**: {(advancedWaveEnabled ? "✅ ENABLED" : "❌ DISABLED")}\n";
            report += $"- **EXP_AI**: {(aiEnabled ? "✅ ENABLED" : "❌ DISABLED")}\n\n";
            
            // Component availability
            report += "## Component Availability\n\n";
            report += "| Component | Status | Description |\n";
            report += "|-----------|--------|-------------|\n";
            report += "| WavePatternGenerator | ✅ Available | Core wave pattern generation functions |\n";
            report += "| WaveParameters | ✅ Available | Wave configuration data structures |\n";
            report += "| WaveSource | ✅ Available | Wave source definitions for interference |\n";
            report += "| CymaticsController | ✅ Available | Audio-driven cymatics visualization |\n";
            report += $"| AdvancedWaveSystem | {(advancedWaveEnabled ? "✅ Full Implementation" : "⚠️ Stub Implementation")} | {(advancedWaveEnabled ? "Advanced wave algorithms with Job System and AI integration" : "Fallback implementation with warnings")} |\n\n";
            
            // Feature combinations
            report += "## Feature Combinations\n\n";
            if (advancedWaveEnabled && aiEnabled)
            {
                report += "⚠️ **High Performance Impact**: Both Advanced Wave and AI features are enabled. This configuration may impact Quest 3 performance.\n\n";
                report += "**Recommendations**:\n";
                report += "- Monitor frame rate carefully on Quest 3 hardware\n";
                report += "- Consider implementing auto-LOD systems\n";
                report += "- Test thermal performance during extended use\n\n";
            }
            else if (advancedWaveEnabled)
            {
                report += "ℹ️ **Moderate Performance Impact**: Advanced Wave features enabled without AI integration.\n\n";
                report += "**Recommendations**:\n";
                report += "- Monitor Quest 3 performance with large bubble counts\n";
                report += "- Consider enabling AI integration for enhanced optimization\n\n";
            }
            else
            {
                report += "✅ **Optimal Performance**: Using core wave mathematics only - ideal for Quest 3 performance.\n\n";
                report += "**Recommendations**:\n";
                report += "- Consider enabling EXP_ADVANCED_WAVE for enhanced features\n";
                report += "- Current configuration is stable for production use\n\n";
            }
            
            // Validation results
            report += "## Validation Results\n\n";
            report += "| Test | Result | Notes |\n";
            report += "|------|--------|-------|\n";
            report += $"| Core Wave Mathematics | {(TestCoreWaveMathematics() ? "✅ PASS" : "❌ FAIL")} | Basic wave functions and data structures |\n";
            report += $"| Advanced Wave System | {(TestAdvancedWaveSystemAvailability() ? "✅ PASS" : "❌ FAIL")} | Component instantiation and basic methods |\n";
            report += $"| Compiler Flag Consistency | {(TestCompilerFlagConsistency() ? "✅ PASS" : "❌ FAIL")} | Flags match implementation state |\n";
            report += $"| Feature Gate Validation | {(TestFeatureGateValidation() ? "✅ PASS" : "❌ FAIL")} | Runtime feature access validation |\n";
            report += $"| Performance Metrics | {(TestPerformanceMetrics() ? "✅ PASS" : "❌ FAIL")} | Metrics system functionality |\n\n";
            
            // Usage recommendations
            report += "## Usage Recommendations\n\n";
            report += "### For Development\n";
            report += "- Enable `EXP_ADVANCED_WAVE` for enhanced wave features\n";
            report += "- Use performance testing tools to validate Quest 3 compatibility\n";
            report += "- Test both enabled and disabled states during development\n\n";
            
            report += "### For Production\n";
            report += "- Validate performance on actual Quest 3 hardware\n";
            report += "- Consider disabling experimental features for stable releases\n";
            report += "- Monitor thermal performance during extended use\n\n";
            
            report += "### For Quest 3 Deployment\n";
            report += "- Target 72 Hz frame rate consistently\n";
            report += "- Implement auto-LOD systems for performance scaling\n";
            report += "- Use performance budgets to guide feature enablement\n\n";
            
            return report;
        }
    }
}