# Feature Gate Attribute System

## Overview

The Feature Gate Attribute System provides runtime feature gating with attribute-based conditional execution. It builds on the Compiler Flag Management System to provide additional safety layers and automatic validation of experimental features. This system is a core component of the "do-it-right" recovery Phase 0 implementation.

## Key Features

- **Attribute-Based Gating**: Declarative feature requirements using C# attributes
- **Runtime Validation**: Automatic validation of feature access at runtime
- **Reflection-Based Discovery**: Automatic discovery of all feature gates in the system
- **Comprehensive Reporting**: Detailed analysis and reporting of feature gate usage
- **Performance Optimized**: Minimal overhead with conditional compilation support
- **Editor Integration**: Visual tools for validation and analysis

## Core Components

### FeatureGateAttribute

The main attribute for marking code that requires experimental features:

```csharp
[FeatureGate(ExperimentalFeature.AI_INTEGRATION)]
public class AIEnhancedBubbleSystem
{
    [FeatureGate(ExperimentalFeature.VOICE_PROCESSING, 
                 ErrorMessage = "Voice processing required for speech recognition",
                 ThrowOnDisabled = false)]
    public void ProcessVoiceInput(string input)
    {
        // Voice processing code
    }
}
```

#### Attribute Properties

- **RequiredFeature**: The experimental feature required (required)
- **ErrorMessage**: Custom error message when feature is disabled (optional)
- **ThrowOnDisabled**: Whether to throw exception or log warning (default: true)
- **ValidateInRelease**: Whether to validate in release builds (default: false)

### FeatureValidationSystem

MonoBehaviour component that provides automatic validation:

```csharp
public class MyGameManager : MonoBehaviour
{
    private FeatureValidationSystem validator;
    
    void Start()
    {
        validator = gameObject.AddComponent<FeatureValidationSystem>();
        validator.OnValidationComplete += HandleValidationResults;
        validator.OnFeatureGateViolation += HandleViolation;
    }
}
```

## Usage Patterns

### Class-Level Feature Gates

Apply feature requirements to entire classes:

```csharp
[FeatureGate(ExperimentalFeature.AI_INTEGRATION)]
public class AIBubbleController : MonoBehaviour
{
    // All methods in this class require AI integration
    public void ProcessAIBehavior() { }
    public void UpdateAIState() { }
}
```

### Method-Level Feature Gates

Apply feature requirements to specific methods:

```csharp
public class BubbleManager : MonoBehaviour
{
    public void UpdateBubbles()
    {
        // Standard bubble update logic
    }
    
    [FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS)]
    public void UpdateAdvancedWaveEffects()
    {
        // Advanced wave algorithm logic
    }
}
```

### Property-Level Feature Gates

Apply feature requirements to properties:

```csharp
public class BubbleConfiguration
{
    public float StandardAmplitude { get; set; }
    
    [FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS)]
    public float AdvancedWaveAmplitude { get; set; }
}
```

### Manual Validation

Manually validate feature access in code:

```csharp
public void ProcessExperimentalFeature()
{
    // Manual validation using extension method
    this.ValidateFeatureGates(); // Validates current method
    
    // Or validate specific feature
    CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION);
    
    // Experimental feature code here
}
```

## Validation Modes

### Automatic Validation

The FeatureValidationSystem can run automatic validation:

```csharp
// Configure automatic validation
validationSystem.enableAutomaticValidation = true;
validationSystem.validationInterval = 30f; // seconds
validationSystem.validateOnStart = true;
```

### Manual Validation

Trigger validation manually when needed:

```csharp
// Full validation of all feature gates
var results = validationSystem.PerformFullValidation();

// Incremental validation (faster, checks only changed features)
var incrementalResults = validationSystem.PerformIncrementalValidation();

// Validate specific feature
var featureResult = validationSystem.ValidateFeature(ExperimentalFeature.AI_INTEGRATION);
```

### Editor Validation

Use the Unity Editor interface for validation:

```
Unity Menu → XR Bubble Library → Feature Validation
```

The editor provides:
- Real-time validation status
- Violation analysis and reporting
- Feature gate discovery and analysis
- Report generation and export

## Error Handling

### FeatureDisabledException

Thrown when accessing disabled features:

```csharp
try
{
    ProcessAIFeature();
}
catch (FeatureDisabledException ex)
{
    Debug.LogWarning($"Feature {ex.DisabledFeature} is disabled: {ex.Message}");
    // Fallback logic
}
```

### Graceful Degradation

Configure non-throwing behavior for graceful degradation:

```csharp
[FeatureGate(ExperimentalFeature.AI_INTEGRATION, ThrowOnDisabled = false)]
public void OptionalAIFeature()
{
    // This will log a warning instead of throwing if AI is disabled
    // Implement fallback logic here
}
```

## Validation Results

### ValidationResults Structure

```csharp
public class ValidationResults
{
    public DateTime ValidationTime;
    public ValidationType ValidationType; // Full or Incremental
    public bool Success;
    public float ValidationDuration;
    public string ValidationError;
    public List<FeatureGateViolation> Violations;
}
```

### FeatureGateViolation Structure

```csharp
public class FeatureGateViolation
{
    public ExperimentalFeature Feature;
    public string TargetName; // Class.Method or Class name
    public FeatureGateTargetType TargetType; // Class, Method, or Property
    public string ErrorMessage;
    public ViolationType ViolationType; // FeatureDisabled, DependencyMissing, ValidationError
}
```

## Reporting and Analysis

### Generate Validation Reports

```csharp
// Generate comprehensive validation report
var report = validationSystem.GenerateValidationReport();

// Generate feature gate analysis report
var gateReport = FeatureGateValidator.GenerateFeatureGateReport();
```

### Report Contents

Validation reports include:
- Validation status and timing
- Feature-by-feature violation analysis
- Feature gate discovery results
- Dependency validation results
- Performance metrics

### Export Options

- Copy to clipboard
- Save to markdown file
- Integration with CI/CD reporting
- Evidence collection for audit trails

## Performance Considerations

### Minimal Runtime Overhead

- Validation only runs in editor/debug builds by default
- Compile-time constants eliminate runtime checks when possible
- Reflection-based discovery cached for performance
- Incremental validation for frequent checks

### Performance Testing

```csharp
[Test]
[Performance]
public void FeatureGate_Performance_Test()
{
    // Validate that feature gate checking has minimal overhead
    var attribute = new FeatureGateAttribute(ExperimentalFeature.AI_INTEGRATION);
    
    var startTime = Time.realtimeSinceStartup;
    for (int i = 0; i < 10000; i++)
    {
        attribute.ValidateFeatureAccess();
    }
    var duration = Time.realtimeSinceStartup - startTime;
    
    Assert.Less(duration / 10000, 0.00001f); // Less than 10 microseconds per check
}
```

## Integration with CI/CD

### Automated Validation

Integrate feature gate validation into build pipeline:

```csharp
[MenuItem("Build/Validate Feature Gates")]
public static void ValidateFeatureGatesForBuild()
{
    var validator = new FeatureValidationSystem();
    var results = validator.PerformFullValidation();
    
    if (!results.Success)
    {
        Debug.LogError($"Feature gate validation failed with {results.Violations.Count} violations");
        EditorApplication.Exit(1); // Fail the build
    }
}
```

### Evidence Collection

Feature gate validation results are automatically included in evidence collection:

- Validation timestamps and results
- Violation analysis and resolution
- Feature usage patterns and dependencies
- Performance impact measurements

## Best Practices

### Development Guidelines

1. **Use Appropriate Granularity**: Apply gates at the right level (class vs method)
2. **Provide Clear Error Messages**: Custom messages help with debugging
3. **Consider Graceful Degradation**: Use `ThrowOnDisabled = false` for optional features
4. **Validate Dependencies**: Ensure required features are properly gated
5. **Test Both States**: Test functionality with features enabled and disabled

### Testing Strategies

1. **Unit Test Feature Gates**: Test that gates work correctly
2. **Integration Testing**: Test feature combinations and dependencies
3. **Performance Testing**: Ensure minimal overhead
4. **Validation Testing**: Test the validation system itself

### Production Deployment

1. **Evidence-Based Enabling**: Only enable features with complete validation
2. **Monitoring**: Use validation system to monitor feature usage
3. **Rollback Capability**: Ability to disable features quickly if issues arise
4. **Audit Trails**: Complete documentation of feature state changes

## Troubleshooting

### Common Issues

**Feature Gate Not Detected**
- Ensure assembly is included in validation scope
- Check that attribute is properly applied
- Verify reflection permissions

**Validation Performance Issues**
- Use incremental validation for frequent checks
- Limit validation scope to relevant assemblies
- Cache validation results when appropriate

**False Positive Violations**
- Check feature dependency configuration
- Verify compiler flag states
- Review custom error messages

### Debug Information

Enable detailed logging for troubleshooting:

```csharp
validationSystem.logValidationResults = true;
validationSystem.enableAutomaticValidation = true;
```

Generate detailed reports for analysis:

```csharp
var report = validationSystem.GenerateValidationReport();
Debug.Log(report);
```

## Future Enhancements

Planned improvements to the feature gate system:

1. **Async Validation**: Non-blocking validation for large codebases
2. **Custom Validators**: Pluggable validation logic for specific features
3. **Performance Profiling**: Detailed performance impact analysis
4. **Visual Dependency Graphs**: Interactive visualization of feature relationships
5. **Automated Remediation**: Suggestions for fixing validation issues

## Support

For issues with the feature gate system:

1. Check the Unity Console for validation warnings and errors
2. Use the Feature Validation Editor window for detailed analysis
3. Generate validation reports for debugging
4. Consult the evidence repository for feature validation data
5. Review the comprehensive test suite for usage examples

This system ensures that experimental features are properly controlled and validated, supporting our "evidence before rhetoric" principle by providing comprehensive validation and reporting capabilities.