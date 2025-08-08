# Build Configuration Validator

The Build Configuration Validator is a comprehensive system for validating experimental feature flag combinations and ensuring build configuration consistency. It's part of the "do-it-right" recovery Phase 0 implementation.

## Overview

The validator performs multiple types of validation:

- **Dependency Validation**: Ensures required dependencies between features are satisfied
- **Flag Combination Validation**: Checks for problematic or conflicting flag combinations
- **Build Consistency Validation**: Validates configuration appropriateness for different build types
- **Performance Impact Validation**: Warns about performance-intensive feature combinations
- **Security Validation**: Highlights security and privacy considerations

## Usage

### Automatic Validation

The validator automatically runs before every build through Unity's `IPreprocessBuildWithReport` interface. Builds will be cancelled if critical configuration errors are detected.

### Manual Validation

#### Via Menu Items

- **XR Bubble Library > Validate Build Configuration**: Run validation and log results
- **XR Bubble Library > Generate Configuration Report**: Create detailed markdown report
- **XR Bubble Library > Show Validation Rules**: Display all validation rules
- **XR Bubble Library > Configuration Validator Window**: Open interactive validation window

#### Via Code

```csharp
// Quick validation check
bool isValid = BuildConfigurationValidator.IsConfigurationValid();

// Detailed validation with results
var result = BuildConfigurationValidator.ValidateConfiguration();
if (!result.IsValid)
{
    Debug.LogError($"Configuration invalid: {result.Issues.Count} issues found");
    foreach (var issue in result.Issues)
    {
        Debug.LogError($"{issue.Severity}: {issue.Title} - {issue.Description}");
    }
}

// Generate human-readable report
string report = result.GenerateReport();
```

#### Via Dependency Injection

```csharp
IBuildConfigurationValidator validator = new BuildConfigurationValidatorService();
var result = validator.ValidateConfiguration();
```

## Validation Rules

### Dependency Rules

1. **Cloud Inference → AI Integration**: Cloud inference requires AI integration to be enabled
2. **On-Device ML → AI Integration**: On-device ML requires AI integration to be enabled

### Performance Rules

1. **Multiple Performance Features**: Warns when multiple performance-intensive features are enabled
2. **Quest 3 Performance**: Specific warnings for Quest 3 deployment considerations
3. **Thermal Considerations**: Warnings about thermal throttling with intensive features

### Security Rules

1. **Cloud Inference Privacy**: Highlights data transmission and privacy considerations
2. **Voice Processing Privacy**: Warns about audio data handling requirements

### Build Consistency Rules

1. **Experimental Features in Release**: Warns about experimental features in release builds
2. **Development-Only Features**: Checks for development-specific features in production builds

## Validation Severity Levels

- **Error** 🔴: Critical issues that will prevent builds
- **Warning** 🟡: Important considerations that should be reviewed
- **Info** ℹ️: Informational notices about configuration implications

## Integration with CI/CD

The validator integrates with Unity's build system and will automatically:

1. Run validation before every build
2. Cancel builds with critical configuration errors
3. Log detailed validation results to the console
4. Provide actionable recommendations for fixing issues

## Configuration Validator Window

The interactive validator window provides:

- Real-time validation status
- Auto-refresh capability (every 5 seconds)
- Detailed issue display with color coding
- One-click report generation
- Scrollable issue list with recommendations

## API Reference

### BuildConfigurationValidator (Static)

- `ValidateConfiguration()`: Comprehensive validation returning detailed results
- `IsConfigurationValid()`: Quick boolean validation check
- `GetValidationRules()`: List of all validation rules
- `ValidateAndLog()`: Validation with console logging

### ValidationResult

- `IsValid`: Boolean indicating overall validation status
- `Issues`: List of all validation issues found
- `ValidatedAt`: Timestamp of validation
- `BuildConfiguration`: Current build configuration
- `GenerateReport()`: Generate human-readable markdown report

### ValidationIssue

- `Severity`: Error, Warning, or Info level
- `Title`: Short issue description
- `Description`: Detailed issue explanation
- `Recommendation`: Suggested fix or action
- `RelatedFeature`: Associated experimental feature (optional)

## Testing

Comprehensive unit tests are provided in `BuildConfigurationValidatorTests.cs`:

- Validation result structure tests
- Report generation tests
- Severity level handling tests
- Edge case handling tests
- Static method functionality tests

## Best Practices

1. **Run validation frequently** during development to catch issues early
2. **Review warnings carefully** even if builds succeed
3. **Use the validator window** for interactive development workflow
4. **Generate reports** for documentation and review purposes
5. **Test on target hardware** when performance warnings are present

## Troubleshooting

### Build Cancelled Due to Validation Errors

1. Check the Unity console for detailed error messages
2. Run manual validation to see full report
3. Fix dependency issues by enabling required features
4. Review flag combinations for conflicts

### Performance Warnings

1. Test on actual Quest 3 hardware
2. Consider implementing auto-LOD systems
3. Monitor thermal performance during extended use
4. Enable features incrementally to isolate performance impact

### Security/Privacy Warnings

1. Review data handling policies
2. Implement proper user consent mechanisms
3. Consider local processing alternatives
4. Ensure compliance with privacy regulations

## Future Enhancements

- Integration with automated testing pipelines
- Performance benchmark integration
- Custom validation rule definitions
- Integration with external validation services
- Automated fix suggestions and application