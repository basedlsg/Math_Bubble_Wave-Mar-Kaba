# Voice Processing System

## Overview

The Voice Processing System provides on-device speech recognition capabilities for the XR Bubble Library. This system is designed with privacy-first principles, processing all voice commands locally without cloud dependencies.

## Compiler Flag Integration

The Voice Processing System is fully integrated with the XR Bubble Library's compiler flag system:

### Compile-Time Control
- **EXP_VOICE**: Master compiler flag that controls voice processing availability
- When `EXP_VOICE` is defined, full voice processing functionality is available
- When `EXP_VOICE` is not defined, stub implementations provide graceful degradation

### Runtime Control
- Voice processing can be enabled/disabled at runtime through the `CompilerFlagManager`
- Feature gates validate access to voice processing methods
- Graceful degradation when features are disabled

## Architecture

### Core Components

#### OnDeviceVoiceProcessor
The main voice processing component that handles:
- Microphone input capture
- Speech recognition processing
- Voice command interpretation
- Spatial command generation
- Performance monitoring

#### Feature Gates
All voice processing methods are protected by feature gates:
```csharp
[FeatureGate(ExperimentalFeature.VOICE_PROCESSING)]
public void StartListening()
{
    CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.VOICE_PROCESSING);
    // Voice processing logic...
}
```

#### Assembly Constraints
The Voice assembly has proper define constraints:
```json
{
    "defineConstraints": ["EXP_VOICE"]
}
```

## Usage

### Basic Voice Processing
```csharp
// Get voice processor component
var voiceProcessor = GetComponent<OnDeviceVoiceProcessor>();

// Start listening for voice commands
voiceProcessor.StartListening();

// Process test command (for debugging)
voiceProcessor.ProcessTestCommand("arrange bubbles");

// Subscribe to voice command events
voiceProcessor.OnVoiceCommandRecognized += HandleVoiceCommand;
voiceProcessor.OnSpatialCommandGenerated += HandleSpatialCommand;
```

### AI Integration
When both `EXP_VOICE` and `EXP_AI` are enabled:
```csharp
// Enable AI enhancement for better recognition
voiceProcessor.SetAIEnhancement(true);
```

### Performance Monitoring
```csharp
// Get performance metrics
var metrics = voiceProcessor.GetPerformanceMetrics();
Debug.Log($"Voice Processing Status: {metrics}");
```

## Supported Voice Commands

The system recognizes the following spatial commands:

### Arrangement Commands
- "arrange" / "organize" / "layout" → `SpatialAction.Arrange`

### Creation Commands  
- "create" / "make" / "spawn" / "add" → `SpatialAction.Create`

### Deletion Commands
- "delete" / "remove" / "clear" / "destroy" → `SpatialAction.Delete`

### Movement Commands
- "move" / "shift" / "relocate" → `SpatialAction.Move`

## Data Structures

### VoiceCommand
```csharp
public struct VoiceCommand
{
    public string recognizedText;
    public float confidence;
    public float timestamp;
    public float processingTimeMs;
}
```

### SpatialCommand
```csharp
public struct SpatialCommand
{
    public SpatialAction action;
    public float3[] positions;
    public float confidence;
    public string sourceCommand;
}
```

### VoiceProcessingMetrics
```csharp
public struct VoiceProcessingMetrics
{
    public bool isListening;
    public bool isProcessing;
    public float lastProcessingTimeMs;
    public int totalCommandsProcessed;
    public float recognitionThreshold;
    public bool aiEnhancementEnabled;
}
```

## Configuration

### Inspector Settings

#### Voice Recognition Configuration
- **Enable Voice Recognition**: Master toggle for voice processing
- **Recognition Threshold**: Minimum confidence required (0.0-1.0)
- **Max Recognition Attempts**: Maximum retry attempts for failed recognition

#### Audio Input Settings
- **Microphone Device**: Target microphone (null = default)
- **Sample Rate**: Audio sample rate (16000 Hz recommended for speech)
- **Recording Length**: Maximum recording duration in seconds

#### Command Processing
- **Command Timeout**: Maximum time to wait for voice input
- **Enable Continuous Listening**: Automatically restart listening after processing
- **Silence Threshold**: Minimum audio level to detect speech

#### AI Integration
- **Enable AI Enhancement**: Use AI to improve recognition accuracy
- **AI Confidence Boost**: Additional confidence boost from AI (0.0-1.0)

#### Performance Optimization
- **Enable Background Processing**: Process audio on background threads
- **Max Concurrent Recognitions**: Limit simultaneous recognition tasks
- **CPU Usage Limit**: Maximum CPU usage percentage (0.0-1.0)

## Compiler Flag States

### EXP_VOICE Enabled
When `EXP_VOICE` is defined:
- Full voice processing functionality available
- Feature gates validate runtime state
- AI integration available if `EXP_AI` also enabled
- Performance monitoring active
- Comprehensive error handling

### EXP_VOICE Disabled
When `EXP_VOICE` is not defined:
- Stub implementation provides API compatibility
- All methods log warnings about disabled features
- GameObject automatically disabled to prevent confusion
- Metrics return default/disabled values
- No exceptions thrown, graceful degradation

## Testing

### Unit Tests
Comprehensive test coverage includes:
- Compiler flag integration testing
- Feature gate validation
- Graceful degradation verification
- AI integration conditional compilation
- Performance metrics validation
- Assembly constraint verification

### Test Scenarios
- Voice enabled/disabled states
- Runtime feature toggling
- AI integration combinations
- Error handling validation
- Performance metric accuracy

## Performance Considerations

### CPU Usage
- Configurable CPU usage limits
- Background processing support
- Concurrent recognition limiting
- Efficient audio buffer management

### Memory Management
- Audio buffer reuse
- Command queue management
- Metrics structure optimization
- Garbage collection minimization

### Quest 3 Optimization
- 16kHz sample rate for speech recognition
- Minimal processing overhead
- Battery usage optimization
- Thermal management awareness

## Privacy & Security

### On-Device Processing
- All speech recognition performed locally
- No cloud dependencies
- No data transmission
- User privacy protected

### Data Handling
- Audio buffers cleared after processing
- No persistent voice data storage
- Minimal memory footprint
- Secure command processing

## Error Handling

### Graceful Degradation
- Feature disabled warnings
- Microphone unavailable handling
- Recognition failure recovery
- Performance limit enforcement

### Exception Management
- Feature gate exceptions
- Audio system failures
- Threading issues
- Resource exhaustion

## Integration Points

### Core System Integration
- Compiler flag system integration
- Feature validation system
- Performance monitoring
- Error reporting

### AI System Integration
- Conditional AI enhancement
- Confidence boosting
- Context-aware processing
- Model integration

### Spatial System Integration
- Command to spatial action mapping
- Position generation
- Bubble interaction
- Scene management

## Development Guidelines

### Adding New Voice Commands
1. Add command mapping in `SetupCommandMappings()`
2. Update `SpatialAction` enum if needed
3. Implement position generation logic
4. Add unit tests for new commands
5. Update documentation

### Performance Optimization
1. Monitor CPU usage metrics
2. Optimize audio processing algorithms
3. Implement efficient buffering
4. Test on target hardware (Quest 3)
5. Profile memory usage

### Feature Gate Implementation
1. Add `[FeatureGate]` attributes to new methods
2. Call `CompilerFlags.ValidateFeatureAccess()` at method start
3. Provide stub implementations when disabled
4. Add comprehensive unit tests
5. Document feature requirements

## Troubleshooting

### Common Issues

#### Voice Not Working
- Check `EXP_VOICE` compiler flag is defined
- Verify feature is enabled in `CompilerFlagManager`
- Ensure microphone permissions granted
- Check microphone device availability

#### Low Recognition Accuracy
- Adjust recognition threshold
- Enable AI enhancement if available
- Check microphone quality and positioning
- Verify sample rate configuration

#### Performance Issues
- Monitor CPU usage metrics
- Reduce concurrent recognition limit
- Disable background processing if needed
- Check thermal throttling on device

#### AI Integration Not Working
- Verify `EXP_AI` compiler flag is defined
- Check `LocalAIModel` component availability
- Ensure AI enhancement is enabled
- Monitor AI processing performance

## Future Enhancements

### Planned Features
- Multi-language support
- Custom vocabulary training
- Noise cancellation improvements
- Advanced AI integration
- Cloud processing option (privacy-preserving)

### Performance Improvements
- Hardware acceleration support
- Optimized recognition algorithms
- Better memory management
- Enhanced Quest 3 integration

### User Experience
- Visual feedback for voice commands
- Confidence indicators
- Command history
- Voice training interface