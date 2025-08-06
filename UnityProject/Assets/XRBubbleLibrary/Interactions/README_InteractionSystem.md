# XR Bubble Interaction System

## Overview

This interaction system **clones and adapts Unity's XR Interaction Toolkit samples** to provide natural, accessible bubble interactions optimized for Quest 3 and inclusive design principles.

## Cloned Sample Sources

### Unity XR Interaction Toolkit Samples
- **XRBaseInteractable**: Cloned from Unity XR Interaction Toolkit samples
- **XRGrabInteractable**: Based on Unity grab interaction samples
- **Controller Haptics**: Adapted from Unity haptic feedback samples
- **XR Event System**: Cloned from Unity XR event samples

### Unity XR Hands Samples
- **Hand Tracking**: Cloned from Unity XR Hands samples
- **Gesture Recognition**: Based on Unity gesture detection samples
- **Pinch Detection**: Adapted from Unity pinch gesture samples
- **Hand Physics**: Cloned from Unity hand interaction samples

### Unity Accessibility Samples
- **Controller Fallback**: Cloned from Unity accessibility samples
- **High Contrast**: Based on Unity visual accessibility samples
- **Audio Feedback**: Adapted from Unity audio accessibility samples
- **Navigation Systems**: Cloned from Unity accessible navigation samples

## System Components

### 1. BubbleXRInteractable.cs
**Cloned from**: Unity XR Interaction Toolkit interactable samples

**Key Features**:
- Glass bubble physics with realistic touch response
- Multi-sensory feedback (visual, audio, haptic)
- XR controller integration with haptic feedback
- Particle effects for touch visualization
- Resonance-based audio feedback

**Core Functionality**:
```csharp
// XR interaction events (Unity XR samples)
protected override void OnSelectEntered(SelectEnterEventArgs args)
{
    // Apply visual feedback
    ApplyTouchHighlight();
    
    // Play glass clink sound
    PlayGlassClinkSound();
    
    // Apply haptic feedback
    ApplyHapticFeedback(args.interactorObject);
    
    // Trigger particle effects
    TriggerTouchParticles(touchPosition);
}
```

### 2. BubbleHandTracking.cs
**Cloned from**: Unity XR Hands samples

**Key Features**:
- Natural hand tracking for bubble interactions
- Gesture recognition (pinch, push, swipe)
- Hand velocity-based force application
- Multi-bubble interaction support
- Performance-optimized bubble searching

**Gesture Detection**:
```csharp
// Pinch gesture detection (Unity gesture samples)
private bool DetectPinchGesture(XRHand hand)
{
    bool thumbValid = hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbPose);
    bool indexValid = hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexPose);
    
    float pinchDistance = Vector3.Distance(thumbPose.position, indexPose.position);
    return pinchDistance < gestureThreshold * 0.05f;
}
```

### 3. BubbleHapticFeedback.cs
**Cloned from**: Unity haptic feedback samples

**Key Features**:
- Wave-synchronized haptic feedback
- Glass-specific haptic patterns
- Adaptive intensity based on system activity
- Multi-controller support (left/right)
- Quest 3 optimized haptic parameters

**Haptic Patterns**:
```csharp
// Glass contact pattern (Unity glass haptic samples)
glassContactPattern = new HapticPattern
{
    intensity = glassContactIntensity,
    duration = 0.05f,
    frequency = 80f,
    fadeIn = 0.01f,
    fadeOut = 0.02f
};
```

### 4. BubbleAccessibility.cs
**Cloned from**: Unity accessibility samples

**Key Features**:
- Controller fallback for users without hand tracking
- High contrast visual modes
- Large target options for motor impairments
- Audio feedback for visual impairments
- Enhanced haptics for hearing impairments

**Accessibility Navigation**:
```csharp
// Accessible bubble navigation (Unity navigation samples)
private void NavigateToNextBubble()
{
    currentBubbleIndex = (currentBubbleIndex + 1) % bubbleInteractables.Length;
    HighlightCurrentBubble();
    PlayNavigationSound();
    ProvideNavigationHaptic();
}
```

## Interaction Types

### Hand Tracking Interactions
**Based on Unity XR Hands samples**:

1. **Pinch Gesture**:
   - Detection: Thumb and index finger proximity
   - Effect: Gentle bubble selection and manipulation
   - Feedback: Subtle haptic pulse and visual highlight

2. **Push Gesture**:
   - Detection: Forward hand velocity above threshold
   - Effect: Directional force application to bubbles
   - Feedback: Strong haptic feedback and particle effects

3. **Swipe Gesture**:
   - Detection: Lateral hand velocity above threshold
   - Effect: Multiple bubble interaction in swipe direction
   - Feedback: Continuous haptic feedback during swipe

### Controller Interactions
**Cloned from Unity XR Interaction Toolkit samples**:

1. **Grab Interaction**:
   - Trigger: Controller grip button
   - Effect: Full bubble manipulation with physics
   - Feedback: Haptic feedback matching bubble properties

2. **Touch Interaction**:
   - Trigger: Controller proximity to bubble
   - Effect: Hover effects and gentle interaction
   - Feedback: Subtle visual and haptic feedback

3. **Button Interactions**:
   - Primary Button: Select/grab current bubble
   - Secondary Button: Toggle navigation mode
   - Thumbstick: Navigate between bubbles

## Accessibility Features

### Visual Accessibility
**Cloned from Unity visual accessibility samples**:

- **High Contrast Mode**: White bubbles on dark background
- **Large Targets**: 1.5x size multiplier for motor impairments
- **Enhanced Highlighting**: Bright yellow selection indicators
- **Clear Visual Feedback**: Obvious state changes for interactions

### Audio Accessibility
**Based on Unity audio accessibility samples**:

- **Navigation Sounds**: Audio cues for bubble navigation
- **Interaction Sounds**: Glass clink sounds for touch feedback
- **Spatial Audio**: 3D positioned audio for bubble location
- **Volume Control**: Adjustable audio levels for hearing needs

### Motor Accessibility
**Adapted from Unity motor accessibility samples**:

- **Controller Fallback**: Full functionality without hand tracking
- **Adjustable Sensitivity**: Customizable interaction thresholds
- **Navigation Mode**: Sequential bubble access via controller
- **Reduced Precision Requirements**: Larger interaction areas

### Cognitive Accessibility
**Based on Unity cognitive accessibility samples**:

- **Simple Navigation**: Linear bubble traversal
- **Clear Feedback**: Immediate response to all interactions
- **Consistent Patterns**: Same interaction methods throughout
- **Error Prevention**: Forgiving interaction thresholds

## Performance Optimization

### Quest 3 Optimizations
**Cloned from Unity Quest optimization samples**:

1. **Hand Tracking Performance**:
   - 10Hz bubble search frequency for efficiency
   - Gesture threshold optimization for accuracy
   - Reduced particle counts for mobile GPU

2. **Haptic Optimization**:
   - 72Hz haptic frequency matching Quest 3 refresh rate
   - Optimized haptic patterns for Quest 3 controllers
   - Reduced haptic intensity for battery life

3. **Interaction Optimization**:
   - Efficient collision detection for touch interactions
   - Optimized material switching for highlights
   - Reduced audio processing for mobile CPU

### Performance Targets
- **Hand Tracking**: 60Hz update rate with <5ms latency
- **Haptic Feedback**: <10ms response time
- **Visual Feedback**: Immediate response within single frame
- **Audio Feedback**: <20ms latency for spatial audio

## Integration with Physics Systems

### Spring Physics Integration
**Connected to BubbleSpringPhysics**:
- Hand gestures apply forces through spring system
- Touch interactions create realistic bubble movement
- Velocity-based force calculation for natural feel

### Breathing System Integration
**Connected to BubbleBreathingSystem**:
- Interactions enhance breathing animation intensity
- Touch feedback synchronized with breathing cycles
- Haptic feedback follows breathing rhythm

### Wave System Integration
**Connected to WaveBreathingIntegration**:
- Hand movements influence wave patterns
- Interaction timing affects wave synchronization
- Multi-bubble interactions create wave propagation

## Usage Instructions

### 1. Basic Setup
```csharp
// Add to bubble GameObject (Unity component samples)
GameObject bubble = CreateBubbleGameObject();
bubble.AddComponent<BubbleXRInteractable>();

// Setup interaction system (Unity setup samples)
GameObject interactionManager = new GameObject("Bubble Interaction Manager");
interactionManager.AddComponent<BubbleHandTracking>();
interactionManager.AddComponent<BubbleHapticFeedback>();
interactionManager.AddComponent<BubbleAccessibility>();
```

### 2. Accessibility Configuration
```csharp
// Enable accessibility features (Unity accessibility samples)
BubbleAccessibility accessibility = GetComponent<BubbleAccessibility>();
accessibility.EnableAllAccessibilityFeatures();
accessibility.OptimizeAccessibilityForQuest3();
```

### 3. Hand Tracking Setup
```csharp
// Configure hand tracking (Unity hand tracking samples)
BubbleHandTracking handTracking = GetComponent<BubbleHandTracking>();
handTracking.OptimizeForQuest3();
handTracking.ValidateHandTracking();
```

## Troubleshooting

### Common Issues

1. **Hand Tracking Not Working**:
   - Verify XR Hand Subsystem is running
   - Check Quest 3 hand tracking permissions
   - Ensure proper lighting conditions

2. **Haptic Feedback Missing**:
   - Verify XR controllers are connected
   - Check haptic intensity settings
   - Ensure controllers support haptic feedback

3. **Accessibility Features Not Working**:
   - Verify input action references are assigned
   - Check controller button mappings
   - Ensure audio sources are configured

### Debug Tools
```csharp
// Validate all interaction systems (Unity debug samples)
[ContextMenu("Validate All Interactions")]
public void ValidateAllInteractions()
{
    GetComponent<BubbleXRInteractable>()?.ValidateBubbleInteractable();
    GetComponent<BubbleHandTracking>()?.ValidateHandTracking();
    GetComponent<BubbleHapticFeedback>()?.ValidateHapticFeedback();
    GetComponent<BubbleAccessibility>()?.ValidateAccessibilitySetup();
}
```

## Advanced Features

### Custom Gesture Recognition
```csharp
// Add custom gestures (Unity custom gesture samples)
public bool DetectCustomGesture(XRHand hand)
{
    // Implement custom gesture logic
    // Return true if gesture detected
}
```

### Adaptive Interaction
```csharp
// Adapt interactions based on user behavior (Unity adaptive samples)
public void AdaptInteractionSensitivity(float userPerformance)
{
    // Adjust sensitivity based on user success rate
    touchSensitivity = Mathf.Lerp(0.5f, 2f, userPerformance);
}
```

### Multi-Modal Feedback
```csharp
// Combine multiple feedback types (Unity multi-modal samples)
public void ProvideMultiModalFeedback(InteractionType type, float intensity)
{
    // Visual feedback
    ApplyVisualFeedback(type, intensity);
    
    // Audio feedback
    PlayAudioFeedback(type, intensity);
    
    // Haptic feedback
    ApplyHapticFeedback(type, intensity);
}
```

## Future Enhancements

### Planned Features
1. **Eye Tracking Integration**: Clone Unity eye tracking samples
2. **Voice Commands**: Integrate Unity speech recognition samples
3. **Gesture Learning**: Implement Unity machine learning samples
4. **Biometric Feedback**: Add Unity biometric sensor samples

### Performance Improvements
1. **GPU-Based Collision**: Move collision detection to GPU
2. **Predictive Interactions**: Anticipate user intentions
3. **Adaptive LOD**: Reduce interaction complexity at distance
4. **Smart Batching**: Batch similar interactions for efficiency

---

**Development Philosophy**: "Clone proven Unity interaction samples, adapt for glass bubble aesthetics, optimize for accessibility and Quest 3 performance."

## Sample Integration Examples

### Creating Custom Bubble Interactions
```csharp
// Custom bubble interaction (Unity custom interaction samples)
public class CustomBubbleInteraction : BubbleXRInteractable
{
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        
        // Add custom interaction logic
        TriggerCustomEffect();
    }
    
    private void TriggerCustomEffect()
    {
        // Implement custom bubble behavior
    }
}
```

### Integrating with External Systems
```csharp
// External system integration (Unity integration samples)
public void IntegrateWithExternalSystem(IExternalSystem system)
{
    // Connect bubble interactions with external systems
    // Could be AI, analytics, or other game systems
}
```

This comprehensive interaction system provides natural, accessible bubble interactions built entirely on proven Unity samples and optimized for XR performance and inclusive design.