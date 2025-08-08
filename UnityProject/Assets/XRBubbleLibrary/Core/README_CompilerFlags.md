# Compiler Flag Management System

## Overview

The Compiler Flag Management System is a core component of the "do-it-right" recovery Phase 0 implementation. It provides rigorous control over experimental features, ensuring that only validated functionality is included in builds while maintaining zero runtime overhead for disabled features.

## Key Features

- **Compile-time Safety**: Features are completely excluded from builds when disabled
- **Zero Runtime Overhead**: Disabled features have no performance impact
- **Dependency Validation**: Automatic checking of feature dependencies
- **Editor Integration**: Visual interface for managing flags
- **Evidence-based Development**: All features require validation before enabling

## Experimental Features

### Available Features

1. **AI_INTEGRATION** (`EXP_AI`)
   - AI-enhanced bubble behavior and interaction systems
   - Required by: Cloud Inference, On-Device ML

2. **VOICE_PROCESSING** (`EXP_VOICE`)
   - Voice recognition and speech-to-text capabilities
   - Independent feature with no dependencies

3. **ADVANCED_WAVE_ALGORITHMS** (`EXP_ADVANCED_WAVE`)
   - Experimental wave mathematics and advanced algorithms
   - Independent feature for testing new wave calculations

4. **CLOUD_INFERENCE** (`EXP_CLOUD_INFERENCE`)
   - Cloud-based AI inference for enhanced processing
   - Requires: AI_INTEGRATION

5. **ON_DEVICE_ML** (`EXP_ON_DEVICE_ML`)
   - On-device machine learning and neural network processing
   - Requires: AI_INTEGRATION

## Usage

### Editor Interface

Access the compiler flag editor through:
```
Unity Menu → XR Bubble Library → Experimental Features
```

The editor provides:
- Visual toggle switches for each feature
- Dependency relationship visualization
- Real-time status reporting
- Configuration validation
- Runtime override capabilities

### Programmatic Access

#### Static Access (Recommended for Performance)
```csharp
// Check if AI features are available (compile-time constant)
if (CompilerFlags.AI_ENABLED)
{
    // AI code here - completely excluded if disabled
}

// Validate feature access (editor only, zero overhead in builds)
CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION);
```

#### Manager Access (For Dynamic Control)
```csharp
var manager = CompilerFlagManager.Instance;

// Check feature state (considers runtime overrides)
if (manager.IsFeatureEnabled(ExperimentalFeature.VOICE_PROCESSING))
{
    // Voice processing code
}

// Runtime override (editor/development only)
manager.SetFeatureState(ExperimentalFeature.AI_INTEGRATION, false);
```

### Code Wrapping Pattern

Wrap experimental code using compiler directives:

```csharp
#if EXP_AI
using XRBubbleLibrary.AI;

public class MyAIFeature
{
    public void ProcessAIInput(string input)
    {
        CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION);
        // AI processing code here
    }
}
#else
// Stub implementation for disabled state
public class MyAIFeature
{
    public void ProcessAIInput(string input)
    {
        Debug.LogWarning("AI features are disabled. Enable EXP_AI to use this functionality.");
    }
}
#endif
```

## Assembly Definition Integration

Experimental features should use conditional assembly definitions:

```json
{
    "name": "XRBubbleLibrary.AI",
    "references": ["XRBubbleLibrary.Core"],
    "defineConstraints": ["EXP_AI"],
    "autoReferenced": false
}
```

This ensures the entire assembly is excluded when the feature is disabled.

## Build Configuration

### Development Builds
- All features available for testing
- Runtime overrides enabled
- Validation checks active
- Detailed logging enabled

### Release Builds
- Only validated features included
- Runtime overrides disabled
- Validation checks compiled out
- Minimal logging

## Validation and Evidence Requirements

Before enabling any experimental feature in production:

1. **Performance Validation**: Feature must maintain 72Hz on Quest 3
2. **Evidence Collection**: Complete performance data and test results
3. **Dependency Verification**: All required features validated
4. **Safety Testing**: User comfort and safety validation complete

## Best Practices

### For Developers

1. **Always Use Compiler Flags**: Wrap all experimental code
2. **Provide Stub Implementations**: Ensure graceful degradation
3. **Validate Dependencies**: Check required features before use
4. **Document Evidence**: Link all features to supporting data

### For Testing

1. **Test Both States**: Verify functionality with features enabled/disabled
2. **Performance Testing**: Validate zero overhead for disabled features
3. **Integration Testing**: Test feature combinations and dependencies
4. **Evidence Validation**: Ensure all claims are backed by data

### For Production

1. **Evidence First**: Only enable features with complete validation
2. **Conservative Approach**: Prefer stability over experimental features
3. **Monitoring**: Continuous performance monitoring in production
4. **Rollback Ready**: Ability to disable features immediately if issues arise

## Troubleshooting

### Common Issues

**Feature Not Available at Runtime**
- Check if feature is compile-time enabled
- Verify all dependencies are satisfied
- Rebuild project after flag changes

**Performance Issues**
- Disable experimental features one by one
- Check performance budgets and evidence
- Use auto-rollback if performance drops

**Compilation Errors**
- Ensure all experimental code is properly wrapped
- Check assembly definition constraints
- Validate dependency relationships

### Debug Information

Generate detailed status reports:
```csharp
var report = CompilerFlagManager.Instance.GenerateStatusReport();
Debug.Log(report);
```

## Integration with CI/CD

The compiler flag system integrates with our CI/CD pipeline:

1. **Build Validation**: Ensures consistent flag states
2. **Performance Gates**: Blocks builds that don't meet performance requirements
3. **Evidence Checking**: Validates that enabled features have supporting evidence
4. **Automatic Rollback**: Disables features that cause performance regressions

## Future Enhancements

Planned improvements to the compiler flag system:

1. **Remote Configuration**: Cloud-based feature flag management
2. **A/B Testing**: Gradual rollout of experimental features
3. **Analytics Integration**: Automatic performance and usage tracking
4. **Advanced Dependencies**: More complex dependency relationships

## Support

For issues with the compiler flag system:

1. Check the Unity Console for validation warnings
2. Use the Editor interface to verify configuration
3. Generate status reports for debugging
4. Consult the evidence repository for feature validation data

This system is designed to support our "evidence before rhetoric" principle, ensuring that every experimental feature is rigorously validated before being enabled in production builds.