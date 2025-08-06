# XR Bubble Library - Performance Optimization Setup Guide

## Overview

This guide covers the setup and configuration of the performance optimization system for the XR Bubble Library. The system is designed to maintain 72 FPS on Quest 3 and 90 FPS on Windows PC through adaptive quality management, LOD systems, and batch processing.

## Core Components

### 1. BubblePerformanceManager
**Purpose**: Central performance management and adaptive quality control
**Location**: `XRBubbleLibrary/Performance/BubblePerformanceManager.cs`

**Key Features**:
- Automatic quality adjustment based on frame rate
- Memory monitoring and management
- Thermal throttling for mobile devices
- Performance statistics tracking

### 2. BubbleLODManager
**Purpose**: Level-of-Detail management for bubble rendering
**Location**: `XRBubbleLibrary/Performance/BubbleLODManager.cs`

**Key Features**:
- Distance-based quality reduction
- Frustum culling for off-screen bubbles
- Batch processing for LOD updates
- Dynamic bubble count management

### 3. BubbleBatchProcessor
**Purpose**: Unity Job System integration for physics processing
**Location**: `XRBubbleLibrary/Performance/BubbleBatchProcessor.cs`

**Key Features**:
- Multi-threaded bubble physics calculations
- Breathing animation processing
- Wave pattern calculations
- Adaptive batch sizing

### 4. BubblePerformanceTester
**Purpose**: Automated performance testing and validation
**Location**: `XRBubbleLibrary/Performance/BubblePerformanceTester.cs`

**Key Features**:
- Automated bubble count optimization
- Performance regression testing
- CSV export for analysis
- Quick performance checks

## Setup Instructions

### Step 1: Unity Project Configuration

1. **Unity Version**: Ensure Unity 2023.3.5f1 (LTS) or newer
2. **Render Pipeline**: Configure Universal Render Pipeline (URP) 14.0.9+
3. **XR Packages**: Install required XR packages:
   ```
   XR Interaction Toolkit 2.5.4+
   XR Core Utilities 2.2.3+
   XR Hands 1.3.0+
   OpenXR Plugin 1.10.0+
   ```

### Step 2: Performance System Integration

1. **Create Performance Manager GameObject**:
   ```csharp
   // In your scene initialization code
   GameObject perfManager = new GameObject("BubblePerformanceManager");
   perfManager.AddComponent<BubblePerformanceManager>();
   ```

2. **Configure LOD Manager**:
   ```csharp
   // LOD Manager is automatically created by Performance Manager
   // Access via singleton pattern
   BubbleLODManager lodManager = BubbleLODManager.Instance;
   
   // Configure for Quest 3
   lodManager.targetFrameRate = 72;
   lodManager.maxActiveBubbles = 150;
   lodManager.maxRenderDistance = 7.0f;
   ```

3. **Setup Batch Processor**:
   ```csharp
   // Batch Processor is automatically created
   BubbleBatchProcessor batchProcessor = BubbleBatchProcessor.Instance;
   
   // Configure for optimal performance
   batchProcessor.maxBatchSize = 40;
   batchProcessor.jobsPerFrame = 3;
   batchProcessor.enableMultithreading = true;
   ```

### Step 3: Bubble Integration

1. **Register Bubbles with Performance System**:
   ```csharp
   public class PooledBubble : MonoBehaviour
   {
       void OnEnable()
       {
           // Register with performance systems
           BubbleLODManager.Instance.RegisterBubble(this);
           BubbleBatchProcessor.Instance.RegisterBubble(this);
       }
       
       void OnDisable()
       {
           // Unregister from performance systems
           BubbleLODManager.Instance.UnregisterBubble(this);
           BubbleBatchProcessor.Instance.UnregisterBubble(this);
       }
   }
   ```

2. **Implement LOD Support**:
   ```csharp
   public void SetLODLevel(int lodLevel)
   {
       switch (lodLevel)
       {
           case 0: // High quality
               SetHighQualityRendering();
               break;
           case 1: // Medium quality
               SetMediumQualityRendering();
               break;
           case 2: // Low quality
               SetLowQualityRendering();
               break;
           default: // Invisible
               SetVisible(false);
               break;
       }
   }
   ```

### Step 4: Platform-Specific Configuration

#### Quest 3 Configuration
```csharp
#if UNITY_ANDROID
void ConfigureForQuest3()
{
    var perfManager = BubblePerformanceManager.Instance;
    
    // Conservative settings for mobile
    perfManager.SetTargetFrameRate(72);
    perfManager.maxTotalBubbles = 150;
    perfManager.enableThermalMonitoring = true;
    perfManager.enableMemoryMonitoring = true;
    
    // Reduce quality for thermal management
    perfManager.SetPerformanceFeature(PerformanceFeature.AdaptiveQuality, true);
}
#endif
```

#### Windows PC Configuration
```csharp
#if UNITY_STANDALONE_WIN
void ConfigureForWindowsPC()
{
    var perfManager = BubblePerformanceManager.Instance;
    
    // Aggressive settings for PC
    perfManager.SetTargetFrameRate(90);
    perfManager.maxTotalBubbles = 200;
    perfManager.enableThermalMonitoring = false;
    perfManager.enableMemoryMonitoring = false;
    
    // Higher quality settings
    perfManager.SetPerformanceFeature(PerformanceFeature.AdaptiveQuality, true);
}
#endif
```

## Performance Testing

### Automated Testing Setup

1. **Add Performance Tester to Scene**:
   ```csharp
   GameObject tester = new GameObject("BubblePerformanceTester");
   var perfTester = tester.AddComponent<BubblePerformanceTester>();
   
   // Configure test parameters
   perfTester.maxBubblesToTest = 200;
   perfTester.minimumFPS = 72;
   perfTester.testDurationSeconds = 10.0f;
   ```

2. **Run Performance Tests**:
   ```csharp
   // Start automated testing
   perfTester.StartPerformanceTests();
   
   // Or run quick check
   perfTester.QuickPerformanceCheck();
   ```

3. **Export Results**:
   ```csharp
   // Export test results to CSV
   string csvData = perfTester.ExportResultsToCSV();
   System.IO.File.WriteAllText("performance_results.csv", csvData);
   ```

## Performance Monitoring

### Real-Time Statistics

```csharp
void Update()
{
    var stats = BubblePerformanceManager.Instance.GetStatistics();
    
    Debug.Log($"FPS: {stats.currentFPS:F1}");
    Debug.Log($"Bubbles: {stats.visibleBubbles}/{stats.totalBubbles}");
    Debug.Log($"Memory: {stats.memoryUsageMB}MB");
    Debug.Log($"Quality Level: {stats.qualityLevel}");
}
```

### Performance Alerts

```csharp
void MonitorPerformance()
{
    var stats = BubblePerformanceManager.Instance.GetStatistics();
    
    if (stats.currentFPS < 60)
    {
        Debug.LogWarning("Performance critical - FPS below 60!");
        // Trigger emergency optimization
        BubblePerformanceManager.Instance.ForceOptimization();
    }
    
    if (stats.memoryUsageMB > 350)
    {
        Debug.LogWarning("Memory usage high - approaching limit!");
        // Reduce bubble count
        ReduceBubbleCount();
    }
}
```

## Optimization Guidelines

### Quest 3 Specific Optimizations

1. **Thermal Management**:
   - Monitor device temperature
   - Reduce quality when thermal throttling occurs
   - Implement gradual quality reduction

2. **Memory Management**:
   - Keep total memory under 400MB
   - Use object pooling aggressively
   - Monitor garbage collection frequency

3. **Rendering Optimization**:
   - Use URP Forward+ renderer
   - Enable GPU instancing where possible
   - Minimize draw calls through batching

### Windows PC Optimizations

1. **Multi-Threading**:
   - Enable all Job System features
   - Use larger batch sizes
   - Leverage multiple CPU cores

2. **Quality Settings**:
   - Higher LOD distances
   - More complex shaders
   - Increased particle counts

## Troubleshooting

### Common Performance Issues

1. **Low FPS on Quest 3**:
   - Check thermal throttling status
   - Reduce bubble count
   - Lower LOD distances
   - Disable complex shaders

2. **Memory Leaks**:
   - Verify bubble pooling is working
   - Check for unreleased native arrays
   - Monitor Job System handle completion

3. **Inconsistent Frame Times**:
   - Enable adaptive quality
   - Reduce batch processing load
   - Check for blocking operations

### Debug Tools

1. **Performance Statistics Display**:
   ```csharp
   void OnGUI()
   {
       var stats = BubblePerformanceManager.Instance.GetStatistics();
       GUILayout.Label($"Performance: {stats}");
   }
   ```

2. **Unity Profiler Integration**:
   - Use Unity Profiler for detailed analysis
   - Monitor Job System performance
   - Track memory allocations

## Best Practices

1. **Always Test on Target Hardware**:
   - Quest 3 performance differs significantly from PC
   - Test thermal behavior under sustained load
   - Validate memory usage patterns

2. **Use Adaptive Quality**:
   - Enable automatic quality adjustment
   - Set conservative initial settings
   - Allow system to optimize over time

3. **Monitor Continuously**:
   - Implement performance telemetry
   - Track user experience metrics
   - Adjust settings based on real usage

4. **Profile Regularly**:
   - Run automated performance tests
   - Check for performance regressions
   - Validate optimization effectiveness

## Integration with Existing Systems

### BubbleObjectPool Integration
```csharp
public class BubbleObjectPool : MonoBehaviour
{
    void Start()
    {
        // Configure pool size based on performance capabilities
        var perfManager = BubblePerformanceManager.Instance;
        int maxBubbles = perfManager.maxTotalBubbles;
        
        InitializePool(maxBubbles);
    }
}
```

### XR Interaction Integration
```csharp
public class BubbleInteraction : MonoBehaviour
{
    void OnInteractionStart()
    {
        // Temporarily boost quality for interaction
        var perfManager = BubblePerformanceManager.Instance;
        perfManager.SetPerformanceFeature(PerformanceFeature.AdaptiveQuality, false);
    }
    
    void OnInteractionEnd()
    {
        // Re-enable adaptive quality
        var perfManager = BubblePerformanceManager.Instance;
        perfManager.SetPerformanceFeature(PerformanceFeature.AdaptiveQuality, true);
    }
}
```

This performance optimization system provides comprehensive tools for maintaining target frame rates across different XR platforms while automatically adapting to hardware capabilities and thermal constraints.