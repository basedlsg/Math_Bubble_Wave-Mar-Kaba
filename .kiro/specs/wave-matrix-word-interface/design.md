# Wave Matrix Word Interface - Design Document

## Overview

This system creates a VR word input interface where spoken words become breathing bubble elements arranged on a mathematical wave matrix. AI predictions position likely words closer to the user, creating an intuitive and beautiful text input experience.

## Architecture

### Core Components

```
VoiceInputManager → WordBubbleFactory → WaveMatrixPositioner
                                    ↓
AIWordPredictor ← BubbleInteractionSystem ← BreathingAnimationSystem
                                    ↓
                            VRInteractionHandler
```

### Component Responsibilities

- **VoiceInputManager**: Captures speech, converts to text
- **WordBubbleFactory**: Creates bubble GameObjects with text
- **WaveMatrixPositioner**: Calculates positions using wave mathematics
- **AIWordPredictor**: Predicts next words, calculates distances
- **BreathingAnimationSystem**: Applies wave-based breathing to all elements
- **BubbleInteractionSystem**: Handles bubble selection and feedback
- **VRInteractionHandler**: Manages VR controller/hand input

## Components and Interfaces

### 1. Voice Input System

```csharp
public class VoiceInputManager : MonoBehaviour
{
    public UnityEvent<string> OnWordSpoken;
    
    // For prototype: Use Unity Microphone class
    // For production: Integrate with platform speech recognition
    
    void Update()
    {
        // Capture microphone input
        // Process audio for speech recognition
        // Trigger OnWordSpoken when word detected
    }
}
```

### 2. Wave Matrix Mathematics

```csharp
public class WaveMatrixPositioner : MonoBehaviour
{
    [Header("Wave Parameters")]
    public float waveAmplitude = 2.0f;
    public float waveFrequency = 1.0f;
    public float waveSpeed = 1.0f;
    
    public Vector3 CalculatePosition(int index, float time, float aiDistance)
    {
        // Base wave matrix position
        float x = index * spacing;
        float z = aiDistance; // AI-predicted distance
        float y = Mathf.Sin(x * waveFrequency + time * waveSpeed) * waveAmplitude;
        
        return new Vector3(x, y, z);
    }
}
```

### 3. Word Bubble System

```csharp
public class WordBubble : MonoBehaviour
{
    public string word;
    public float aiConfidence; // 0-1, affects distance
    public TextMeshPro textComponent;
    public Renderer bubbleRenderer;
    
    void Update()
    {
        // Apply breathing animation
        ApplyBreathingEffect();
    }
    
    void ApplyBreathingEffect()
    {
        float breathScale = 1.0f + Mathf.Sin(Time.time * breathingFreq) * breathingAmp;
        transform.localScale = originalScale * breathScale;
    }
}
```

### 4. AI Word Prediction (Simulated)

```csharp
public class AIWordPredictor : MonoBehaviour
{
    private Dictionary<string, string[]> wordPredictions = new Dictionary<string, string[]>
    {
        {"hello", new[] {"world", "there", "everyone", "friend"}},
        {"how", new[] {"are", "do", "can", "will"}},
        {"what", new[] {"is", "are", "do", "time"}},
        // ... more predictions
    };
    
    public WordPrediction[] PredictNextWords(string currentWord)
    {
        // Return predicted words with confidence scores
        // Higher confidence = closer to user (smaller Z distance)
    }
}
```

## Data Models

### WordPrediction
```csharp
[System.Serializable]
public class WordPrediction
{
    public string word;
    public float confidence; // 0-1
    public Vector3 position; // Calculated by wave matrix
}
```

### WaveParameters
```csharp
[System.Serializable]
public class WaveParameters
{
    public float amplitude = 1.0f;
    public float frequency = 1.0f;
    public float speed = 1.0f;
    public float breathingRate = 2.0f;
}
```

### BubbleVisualSettings
```csharp
[System.Serializable]
public class BubbleVisualSettings
{
    public Material bubbleMaterial;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public Color textColor = Color.white;
    public float transparency = 0.7f;
}
```

## Error Handling

### Voice Recognition Failures
- Visual feedback when speech not recognized
- Retry mechanism with visual prompt
- Fallback to manual text input if needed

### AI Prediction Unavailable
- Use hardcoded word association dictionary
- Graceful degradation to common word patterns
- Still maintain distance-based positioning

### Performance Issues
- LOD system for distant bubbles
- Limit maximum number of active bubbles
- Optimize wave calculations for mobile VR

## Testing Strategy

### Unit Tests
- Wave mathematics calculations
- Word prediction algorithms
- Bubble positioning accuracy
- Breathing animation smoothness

### Integration Tests
- Voice input → bubble creation pipeline
- AI prediction → positioning system
- VR interaction → bubble selection
- Complete user workflow testing

### VR Testing
- Quest 3 hardware validation
- Performance profiling (60+ FPS)
- Motion sickness evaluation
- User experience testing

## Implementation Phases

### Phase 1: Core Wave Matrix
- Basic wave mathematics
- Simple bubble creation
- Breathing animation system
- Static word positioning

### Phase 2: Voice Integration
- Microphone input capture
- Basic speech-to-text
- Word bubble generation
- VR scene integration

### Phase 3: AI Prediction
- Simulated word prediction
- Distance-based positioning
- Dynamic bubble arrangement
- Confidence-based scaling

### Phase 4: VR Interaction
- Controller/hand tracking
- Bubble selection system
- Haptic feedback
- User interface polish

### Phase 5: Optimization
- Performance optimization
- Visual polish
- Quest 3 deployment
- User testing and refinement

## Technical Constraints

### VR Performance
- Must maintain 60+ FPS on Quest 3
- Minimize draw calls for bubble rendering
- Efficient wave calculation algorithms
- Memory management for bubble pooling

### Unity Requirements
- Unity 2022.3 LTS or newer
- XR Toolkit for VR interaction
- TextMeshPro for text rendering
- Built-in render pipeline compatibility

### Hardware Constraints
- Quest 3 mobile processor limitations
- Limited memory for bubble instances
- Thermal management considerations
- Battery life optimization