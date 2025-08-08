# Wave Mathematics Compiler Flags

This document describes the compiler flag strategy for the XR Bubble Library wave mathematics system, implemented as part of the "do-it-right" recovery Phase 0.

## Overview

The wave mathematics system is divided into two categories:

1. **Core Wave Mathematics** - Always available, fundamental wave functions
2. **Advanced Wave Algorithms** - Experimental features behind `EXP_ADVANCED_WAVE` flag

## Core Wave Mathematics (Always Enabled)

These components provide essential wave functionality and remain available regardless of compiler flags:

### WavePatternGenerator.cs
- **Purpose**: Static utility functions for basic wave pattern generation
- **Key Functions**:
  - `GenerateWavePattern()` - Creates circular wave patterns
  - `CalculateWaveInterference()` - Computes multi-source wave interference
- **Status**: ✅ Always enabled (core functionality)

### WaveParameters.cs
- **Purpose**: Data structures for wave configuration
- **Key Features**:
  - Primary, secondary, and tertiary wave parameters
  - Default parameter sets for common use cases
- **Status**: ✅ Always enabled (core data structures)

### WaveSource.cs
- **Purpose**: Defines wave sources for interference calculations
- **Key Features**:
  - 3D positioned wave sources
  - Frequency, amplitude, and phase configuration
- **Status**: ✅ Always enabled (core data structures)

### CymaticsController.cs
- **Purpose**: Real-time cymatics visualization based on audio
- **Key Features**:
  - Audio-driven pattern generation
  - Interface-based audio renderer integration
  - Real-time texture updates
- **Status**: ✅ Always enabled (core visualization)

## Advanced Wave Algorithms (Experimental)

These components require the `EXP_ADVANCED_WAVE` compiler flag to be enabled:

### AdvancedWaveSystem.cs
- **Purpose**: High-performance wave system with AI integration
- **Key Features**:
  - Unity Job System integration for performance
  - Optional AI bias field optimization (requires `EXP_AI`)
  - Fibonacci spiral positioning with wave modulation
  - Harmonic ratio calculations for musical spacing
  - Quest 3 performance optimization
- **Status**: 🔬 Experimental (requires `EXP_ADVANCED_WAVE`)

## Compiler Flag Implementation

### EXP_ADVANCED_WAVE Flag

The `EXP_ADVANCED_WAVE` compiler flag controls access to advanced wave algorithms:

```csharp
#if EXP_ADVANCED_WAVE
    // Full AdvancedWaveSystem implementation
    public class AdvancedWaveSystem : MonoBehaviour
    {
        // Advanced features with Job System, AI integration, etc.
    }
#else
    // Stub implementation with graceful degradation
    public class AdvancedWaveSystem : MonoBehaviour
    {
        // Warning messages and fallback functionality
    }
#endif
```

### Feature Gate Integration

Advanced wave methods use the `[FeatureGate]` attribute for runtime validation:

```csharp
[FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS,
             ErrorMessage = "Advanced Wave System requires Advanced Wave Algorithms to be enabled")]
public class AdvancedWaveSystem : MonoBehaviour
{
    [FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS)]
    public async Task<float3[]> GenerateOptimalBubblePositions(int bubbleCount, float3 userPosition)
    {
        CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
        // Implementation...
    }
}
```

## AI Integration Flags

The advanced wave system optionally integrates with AI features when both flags are enabled:

```csharp
#if EXP_ADVANCED_WAVE && EXP_AI
    // AI-enhanced wave optimization available
    private async Task<BiasField> GenerateAIBiasField()
    {
        CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION);
        // AI implementation...
    }
#endif
```

## Graceful Degradation

When advanced wave algorithms are disabled, the system provides:

1. **Stub Implementation**: All public methods remain available but log warnings
2. **Fallback Functionality**: Basic implementations for essential features
3. **Clear Messaging**: Informative warnings about disabled features
4. **Performance Metrics**: Disabled state clearly indicated in metrics

### Example Stub Implementation

```csharp
#if !EXP_ADVANCED_WAVE
public async Task<float3[]> GenerateOptimalBubblePositions(int bubbleCount, float3 userPosition)
{
    Debug.LogWarning("[AdvancedWaveSystem] Advanced Wave Algorithms are disabled. Enable EXP_ADVANCED_WAVE compiler flag to use this functionality.");
    
    // Return simple fallback positions
    var positions = new float3[bubbleCount];
    for (int i = 0; i < bubbleCount; i++)
    {
        positions[i] = userPosition + new float3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-0.5f, 0.5f),
            UnityEngine.Random.Range(1f, 2f)
        );
    }
    
    await Task.Yield();
    return positions;
}
#endif
```

## Testing Strategy

### Conditional Compilation Tests

Tests are written to validate both enabled and disabled states:

```csharp
#if EXP_ADVANCED_WAVE
[Test]
public void AdvancedWaveSystem_GenerateOptimalBubblePositions_WithAdvancedWaveEnabled()
{
    // Test full functionality when enabled
}
#else
[Test]
public void AdvancedWaveSystem_GenerateOptimalBubblePositions_WithAdvancedWaveDisabled()
{
    // Test fallback functionality when disabled
}
#endif
```

### Core Functionality Tests

Core wave mathematics components are tested regardless of compiler flags:

```csharp
[Test]
public void WavePatternGenerator_GenerateWavePattern_ReturnsCorrectCount()
{
    // Always runs - tests core functionality
}
```

## Performance Considerations

### Quest 3 Optimization

The advanced wave system is specifically optimized for Quest 3 hardware:

- **Job System**: Parallel processing for wave calculations
- **Native Arrays**: Memory-efficient data structures
- **Batch Processing**: Configurable batch sizes for optimal performance
- **AI Integration**: Optional AI bias fields with performance monitoring

### Performance Metrics

The system provides detailed performance metrics:

```csharp
public struct WaveSystemMetrics
{
    public float lastComputeTimeMs;
    public int activeBubbleCount;
    public bool aiOptimizationEnabled;
    public bool jobSystemEnabled;
    public int currentBiasFieldSize;
}
```

## Usage Guidelines

### Enabling Advanced Wave Algorithms

1. Add `EXP_ADVANCED_WAVE` to Unity's Scripting Define Symbols
2. Optionally add `EXP_AI` for AI-enhanced wave optimization
3. Rebuild the project to enable advanced features

### Development Workflow

1. **Start with Core**: Use core wave mathematics for basic functionality
2. **Add Advanced Features**: Enable `EXP_ADVANCED_WAVE` for performance optimization
3. **AI Enhancement**: Enable `EXP_AI` for AI-driven wave optimization
4. **Performance Testing**: Monitor Quest 3 performance with enabled features

### Best Practices

1. **Test Both States**: Always test with advanced features both enabled and disabled
2. **Monitor Performance**: Use performance metrics to validate Quest 3 compatibility
3. **Graceful Degradation**: Ensure core functionality works without advanced features
4. **Clear Documentation**: Document which features require which flags

## Troubleshooting

### Common Issues

1. **"Feature X is disabled" warnings**: Enable the appropriate compiler flag
2. **Performance issues**: Monitor metrics and adjust batch sizes
3. **AI integration failures**: Ensure both `EXP_ADVANCED_WAVE` and `EXP_AI` are enabled
4. **Build errors**: Check that all required assemblies are referenced

### Validation Commands

```csharp
// Check if advanced wave algorithms are enabled
bool isEnabled = CompilerFlags.IsFeatureEnabled(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);

// Validate feature access at runtime
CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);

// Get performance metrics
var metrics = advancedWaveSystem.GetPerformanceMetrics();
Debug.Log(metrics.ToString());
```

## Future Enhancements

- Integration with Quest 3 thermal monitoring
- Dynamic quality adjustment based on performance
- Additional AI model support
- Enhanced cymatics visualization
- Real-time wave parameter adjustment

This flagging strategy ensures that core wave mathematics remain stable and always available while allowing experimental advanced features to be developed and tested safely behind compiler flags.