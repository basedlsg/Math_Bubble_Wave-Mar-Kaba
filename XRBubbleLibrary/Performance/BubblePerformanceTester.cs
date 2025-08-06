using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XRBubbleLibrary.Physics;

namespace XRBubbleLibrary.Performance
{
    /// <summary>
    /// Performance testing utility for XR Bubble Library
    /// Cloned from Unity performance testing samples, adapted for Quest 3 validation
    /// </summary>
    public class BubblePerformanceTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestsOnStart = false;
        
        [Range(10, 500)]
        public int maxBubblesToTest = 200;
        
        [Range(1, 10)]
        public int bubbleIncrement = 10;
        
        [Range(5.0f, 30.0f)]
        public float testDurationSeconds = 10.0f;
        
        [Header("Performance Thresholds")]
        [Range(60, 120)]
        public int minimumFPS = 72;
        
        [Range(100, 500)]
        public int maximumMemoryMB = 400;
        
        [Range(0.010f, 0.020f)]
        public float maximumFrameTimeMs = 0.014f;
        
        [Header("Test Results")]
        public bool testsCompleted = false;
        public int optimalBubbleCount = 0;
        public float averageTestFPS = 0.0f;
        public int peakMemoryUsage = 0;
        
        // Test data
        private List<PerformanceTestResult> testResults = new List<PerformanceTestResult>();
        private BubblePerformanceManager performanceManager;
        private BubbleObjectPool bubblePool;
        private List<PooledBubble> testBubbles = new List<PooledBubble>();
        
        // Test state
        private bool testInProgress = false;
        private int currentTestBubbleCount = 0;
        private float testStartTime = 0.0f;
        private List<float> frameTimesSamples = new List<float>();
        private int peakMemoryThisTest = 0;
        
        void Start()
        {
            // Get component references
            performanceManager = BubblePerformanceManager.Instance;
            bubblePool = FindObjectOfType<BubbleObjectPool>();
            
            if (bubblePool == null)
            {
                Debug.LogError("BubbleObjectPool not found! Cannot run performance tests.");
                return;
            }
            
            if (runTestsOnStart)
            {
                StartCoroutine(RunPerformanceTests());
            }
        }
        
        /// <summary>
        /// Run comprehensive performance tests
        /// </summary>
        public void StartPerformanceTests()
        {
            if (testInProgress)
            {
                Debug.LogWarning("Performance tests already in progress!");
                return;
            }
            
            StartCoroutine(RunPerformanceTests());
        }
        
        /// <summary>
        /// Main performance testing coroutine
        /// </summary>
        IEnumerator RunPerformanceTests()
        {
            testInProgress = true;
            testsCompleted = false;
            testResults.Clear();
            
            Debug.Log("Starting XR Bubble Library performance tests...");
            
            // Test different bubble counts
            for (int bubbleCount = bubbleIncrement; bubbleCount <= maxBubblesToTest; bubbleCount += bubbleIncrement)
            {
                Debug.Log($"Testing with {bubbleCount} bubbles...");
                
                yield return StartCoroutine(TestBubbleCount(bubbleCount));
                
                // Clean up between tests
                CleanupTestBubbles();
                
                // Wait for garbage collection
                System.GC.Collect();
                yield return new WaitForSeconds(1.0f);
            }
            
            // Analyze results
            AnalyzeTestResults();
            
            testInProgress = false;
            testsCompleted = true;
            
            Debug.Log($"Performance tests completed. Optimal bubble count: {optimalBubbleCount}");
        }
        
        /// <summary>
        /// Test performance with specific bubble count
        /// </summary>
        IEnumerator TestBubbleCount(int bubbleCount)
        {
            currentTestBubbleCount = bubbleCount;
            frameTimesSamples.Clear();
            peakMemoryThisTest = 0;
            
            // Create test bubbles
            CreateTestBubbles(bubbleCount);
            
            // Wait for system to stabilize
            yield return new WaitForSeconds(2.0f);
            
            // Start performance measurement
            testStartTime = Time.time;
            float testEndTime = testStartTime + testDurationSeconds;
            
            // Collect performance data
            while (Time.time < testEndTime)
            {
                // Record frame time
                frameTimesSamples.Add(Time.unscaledDeltaTime);
                
                // Check memory usage
                #if UNITY_ANDROID
                    int currentMemory = (int)(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false) / (1024 * 1024));
                    if (currentMemory > peakMemoryThisTest)
                    {
                        peakMemoryThisTest = currentMemory;
                    }
                #endif
                
                yield return null; // Wait one frame
            }
            
            // Calculate test results
            PerformanceTestResult result = CalculateTestResult();
            testResults.Add(result);
            
            Debug.Log($"Test Result - Bubbles: {bubbleCount}, FPS: {result.averageFPS:F1}, " +
                     $"Memory: {result.peakMemoryMB}MB, Passed: {result.testPassed}");
        }
        
        /// <summary>
        /// Create test bubbles for performance testing
        /// </summary>
        void CreateTestBubbles(int count)
        {
            testBubbles.Clear();
            
            // Create bubbles in a grid pattern around the origin
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(count));
            float spacing = 0.5f;
            
            for (int i = 0; i < count; i++)
            {
                int x = i % gridSize;
                int z = i / gridSize;
                
                Vector3 position = new Vector3(
                    (x - gridSize * 0.5f) * spacing,
                    Random.Range(-1.0f, 1.0f),
                    (z - gridSize * 0.5f) * spacing
                );
                
                PooledBubble bubble = bubblePool.GetBubble();
                if (bubble != null)
                {
                    bubble.transform.position = position;
                    bubble.SetActive(true);
                    testBubbles.Add(bubble);
                }
            }
            
            Debug.Log($"Created {testBubbles.Count} test bubbles");
        }
        
        /// <summary>
        /// Clean up test bubbles
        /// </summary>
        void CleanupTestBubbles()
        {
            foreach (var bubble in testBubbles)
            {
                if (bubble != null)
                {
                    bubblePool.ReturnBubble(bubble);
                }
            }
            
            testBubbles.Clear();
        }
        
        /// <summary>
        /// Calculate performance test result
        /// </summary>
        PerformanceTestResult CalculateTestResult()
        {
            PerformanceTestResult result = new PerformanceTestResult();
            result.bubbleCount = currentTestBubbleCount;
            result.testDuration = testDurationSeconds;
            result.peakMemoryMB = peakMemoryThisTest;
            
            // Calculate frame time statistics
            if (frameTimesSamples.Count > 0)
            {
                float totalFrameTime = 0.0f;
                float minFrameTime = float.MaxValue;
                float maxFrameTime = 0.0f;
                
                foreach (float frameTime in frameTimesSamples)
                {
                    totalFrameTime += frameTime;
                    if (frameTime < minFrameTime) minFrameTime = frameTime;
                    if (frameTime > maxFrameTime) maxFrameTime = frameTime;
                }
                
                result.averageFrameTime = totalFrameTime / frameTimesSamples.Count;
                result.minFrameTime = minFrameTime;
                result.maxFrameTime = maxFrameTime;
                result.averageFPS = 1.0f / result.averageFrameTime;
                result.minFPS = 1.0f / maxFrameTime;
                
                // Calculate frame time variance
                float variance = 0.0f;
                foreach (float frameTime in frameTimesSamples)
                {
                    float diff = frameTime - result.averageFrameTime;
                    variance += diff * diff;
                }
                result.frameTimeVariance = variance / frameTimesSamples.Count;
            }
            
            // Determine if test passed
            result.testPassed = result.averageFPS >= minimumFPS &&
                               result.peakMemoryMB <= maximumMemoryMB &&
                               result.averageFrameTime <= maximumFrameTimeMs;
            
            return result;
        }
        
        /// <summary>
        /// Analyze all test results to find optimal settings
        /// </summary>
        void AnalyzeTestResults()
        {
            if (testResults.Count == 0) return;
            
            // Find the highest bubble count that still passes all tests
            optimalBubbleCount = 0;
            float totalFPS = 0.0f;
            int passedTests = 0;
            
            foreach (var result in testResults)
            {
                totalFPS += result.averageFPS;
                
                if (result.testPassed && result.bubbleCount > optimalBubbleCount)
                {
                    optimalBubbleCount = result.bubbleCount;
                }
                
                if (result.testPassed)
                {
                    passedTests++;
                }
                
                if (result.peakMemoryMB > peakMemoryUsage)
                {
                    peakMemoryUsage = result.peakMemoryMB;
                }
            }
            
            averageTestFPS = totalFPS / testResults.Count;
            
            Debug.Log($"Performance Analysis Complete:");
            Debug.Log($"- Optimal Bubble Count: {optimalBubbleCount}");
            Debug.Log($"- Average FPS: {averageTestFPS:F1}");
            Debug.Log($"- Peak Memory: {peakMemoryUsage}MB");
            Debug.Log($"- Tests Passed: {passedTests}/{testResults.Count}");
            
            // Generate recommendations
            GeneratePerformanceRecommendations();
        }
        
        /// <summary>
        /// Generate performance recommendations based on test results
        /// </summary>
        void GeneratePerformanceRecommendations()
        {
            Debug.Log("Performance Recommendations:");
            
            if (optimalBubbleCount < 50)
            {
                Debug.Log("- Consider reducing visual complexity or enabling more aggressive LOD");
            }
            else if (optimalBubbleCount > 150)
            {
                Debug.Log("- Excellent performance! Consider increasing visual quality");
            }
            
            if (peakMemoryUsage > maximumMemoryMB * 0.8f)
            {
                Debug.Log("- Memory usage is high. Consider implementing more aggressive pooling");
            }
            
            if (averageTestFPS < minimumFPS * 1.1f)
            {
                Debug.Log("- FPS is close to minimum. Enable adaptive quality management");
            }
            
            // Set recommended settings in performance manager
            if (performanceManager != null)
            {
                if (optimalBubbleCount < 100)
                {
                    performanceManager.SetTargetFrameRate(72); // Conservative for Quest 3
                }
                else
                {
                    performanceManager.SetTargetFrameRate(90); // Aggressive for good hardware
                }
            }
        }
        
        /// <summary>
        /// Get detailed test results
        /// </summary>
        public List<PerformanceTestResult> GetTestResults()
        {
            return new List<PerformanceTestResult>(testResults);
        }
        
        /// <summary>
        /// Export test results to CSV format
        /// </summary>
        public string ExportResultsToCSV()
        {
            if (testResults.Count == 0) return "";
            
            System.Text.StringBuilder csv = new System.Text.StringBuilder();
            
            // Header
            csv.AppendLine("BubbleCount,AverageFPS,MinFPS,AverageFrameTimeMs,MaxFrameTimeMs,FrameTimeVariance,PeakMemoryMB,TestPassed");
            
            // Data rows
            foreach (var result in testResults)
            {
                csv.AppendLine($"{result.bubbleCount},{result.averageFPS:F2},{result.minFPS:F2}," +
                              $"{result.averageFrameTime * 1000:F2},{result.maxFrameTime * 1000:F2}," +
                              $"{result.frameTimeVariance:F6},{result.peakMemoryMB},{result.testPassed}");
            }
            
            return csv.ToString();
        }
        
        /// <summary>
        /// Run a quick performance check with current bubble count
        /// </summary>
        public void QuickPerformanceCheck()
        {
            StartCoroutine(QuickPerformanceCheckCoroutine());
        }
        
        IEnumerator QuickPerformanceCheckCoroutine()
        {
            Debug.Log("Running quick performance check...");
            
            frameTimesSamples.Clear();
            float checkDuration = 3.0f;
            float startTime = Time.time;
            
            while (Time.time - startTime < checkDuration)
            {
                frameTimesSamples.Add(Time.unscaledDeltaTime);
                yield return null;
            }
            
            if (frameTimesSamples.Count > 0)
            {
                float avgFrameTime = 0.0f;
                foreach (float ft in frameTimesSamples)
                {
                    avgFrameTime += ft;
                }
                avgFrameTime /= frameTimesSamples.Count;
                
                float currentFPS = 1.0f / avgFrameTime;
                
                Debug.Log($"Quick Check Result - FPS: {currentFPS:F1}, " +
                         $"Frame Time: {avgFrameTime * 1000:F2}ms");
                
                if (currentFPS < minimumFPS)
                {
                    Debug.LogWarning("Performance below target! Consider reducing bubble count or quality.");
                }
            }
        }
        
        void OnGUI()
        {
            if (!Debug.isDebugBuild) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 150));
            
            GUILayout.Label("Performance Tester");
            
            if (testInProgress)
            {
                GUILayout.Label($"Testing: {currentTestBubbleCount} bubbles");
                GUILayout.Label($"Progress: {((float)testResults.Count / (maxBubblesToTest / bubbleIncrement)) * 100:F0}%");
            }
            else
            {
                if (GUILayout.Button("Start Performance Tests"))
                {
                    StartPerformanceTests();
                }
                
                if (GUILayout.Button("Quick Check"))
                {
                    QuickPerformanceCheck();
                }
                
                if (testsCompleted)
                {
                    GUILayout.Label($"Optimal: {optimalBubbleCount} bubbles");
                    GUILayout.Label($"Avg FPS: {averageTestFPS:F1}");
                }
            }
            
            GUILayout.EndArea();
        }
    }
    
    /// <summary>
    /// Performance test result data structure
    /// </summary>
    [System.Serializable]
    public struct PerformanceTestResult
    {
        public int bubbleCount;
        public float testDuration;
        public float averageFPS;
        public float minFPS;
        public float averageFrameTime;
        public float minFrameTime;
        public float maxFrameTime;
        public float frameTimeVariance;
        public int peakMemoryMB;
        public bool testPassed;
        
        public override string ToString()
        {
            return $"Test Result - Bubbles: {bubbleCount}, FPS: {averageFPS:F1}, " +
                   $"Memory: {peakMemoryMB}MB, Passed: {testPassed}";
        }
    }
}