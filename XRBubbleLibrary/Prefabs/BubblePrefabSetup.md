# Bubble Prefab Setup Guide

## Prefab Structure
```
BubblePrefab (GameObject)
├── BubbleVisual (GameObject)
│   ├── MeshRenderer (Sphere mesh with BubbleGlass material)
│   └── MeshFilter (Unity sphere primitive)
├── BubbleCollider (GameObject)
│   └── SphereCollider (Trigger enabled, radius slightly larger than visual)
└── BubbleAudio (GameObject)
    └── AudioSource (3D spatial audio for glass clink sounds)
```

## Component Configuration

### Root GameObject Components
```csharp
// Required Components on BubblePrefab root:
1. BubblePhysics (our custom script)
2. BubbleInteraction (our custom XR interaction script)
3. Rigidbody (kinematic, for physics queries)
4. XRGrabInteractable (from XR Interaction Toolkit)
```

### BubbleVisual Configuration
```
MeshFilter:
- Mesh: Unity Sphere primitive
- Scale: (1, 1, 1) - size controlled by BubbleConfiguration

MeshRenderer:
- Material: BubbleGlass_Normal (created from our shader)
- Cast Shadows: Off (transparent objects don't need shadows)
- Receive Shadows: Off
- Light Probes: Blend Probes
- Reflection Probes: Blend Probes and Skybox
```

### BubbleCollider Configuration
```
SphereCollider:
- Is Trigger: True
- Radius: 1.1 (slightly larger than visual for easier interaction)
- Center: (0, 0, 0)
- Material: None (using trigger events)
```

### BubbleAudio Configuration
```
AudioSource:
- Spatial Blend: 1.0 (full 3D)
- Volume: 0.7
- Pitch: 1.0 (randomized in script)
- Doppler Level: 0.5
- Min Distance: 0.5
- Max Distance: 5.0
- Rolloff Mode: Logarithmic
```

## Prefab Variants for Different States

### 1. Normal Bubble Prefab
```
Name: BubbleNormal
Material: BubbleGlass_Normal (purple)
Configuration:
- Radius: 0.15m
- Breathing Speed: 0.3 Hz
- Glow Intensity: 0.6
```

### 2. Hover Bubble Prefab
```
Name: BubbleHover  
Material: BubbleGlass_Hover (blue)
Configuration:
- Radius: 0.16m (slightly larger)
- Breathing Speed: 0.4 Hz
- Glow Intensity: 0.8
```

### 3. Pressed Bubble Prefab
```
Name: BubblePressed
Material: BubbleGlass_Pressed (pink)
Configuration:
- Radius: 0.13m (compressed)
- Breathing Speed: 0.5 Hz
- Glow Intensity: 1.0
```

## XR Interaction Setup

### XRGrabInteractable Configuration
```csharp
// Component settings for XR interaction
XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();

// Interaction settings
grabInteractable.selectMode = InteractableSelectMode.Single;
grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
grabInteractable.trackPosition = false; // Bubbles stay in wave positions
grabInteractable.trackRotation = false;

// Haptic feedback settings
grabInteractable.selectEntered.AddListener(OnBubbleSelected);
grabInteractable.selectExited.AddListener(OnBubbleDeselected);
grabInteractable.hoverEntered.AddListener(OnBubbleHoverEnter);
grabInteractable.hoverExited.AddListener(OnBubbleHoverExit);
```

### Hand Tracking Configuration
```csharp
// Enable hand tracking for natural interaction
XRDirectInteractor handInteractor = FindObjectOfType<XRDirectInteractor>();
if (handInteractor != null)
{
    // Configure for hand tracking
    handInteractor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.StateChange;
    handInteractor.hideControllerOnSelect = false;
}
```

## Bubble Factory Script

```csharp
using UnityEngine;
using XRBubbleLibrary;

public class BubbleFactory : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject bubblePrefab;
    public Material[] bubbleMaterials; // Normal, Hover, Pressed
    public AudioClip[] glassClinkSounds;
    
    [Header("Spawn Settings")]
    public int maxBubbles = 100;
    public float spawnRadius = 2.0f;
    public LayerMask bubbleLayer = 1 << 8;
    
    // Object pooling for performance
    private Queue<GameObject> bubblePool = new Queue<GameObject>();
    private List<GameObject> activeBubbles = new List<GameObject>();
    
    void Start()
    {
        InitializeBubblePool();
    }
    
    void InitializeBubblePool()
    {
        // Pre-create bubble pool for performance
        for (int i = 0; i < maxBubbles; i++)
        {
            GameObject bubble = CreateBubbleInstance();
            bubble.SetActive(false);
            bubblePool.Enqueue(bubble);
        }
    }
    
    GameObject CreateBubbleInstance()
    {
        GameObject bubble = Instantiate(bubblePrefab);
        
        // Configure components
        BubblePhysics physics = bubble.GetComponent<BubblePhysics>();
        BubbleInteraction interaction = bubble.GetComponent<BubbleInteraction>();
        
        // Set random configuration for variety
        BubbleConfiguration config = BubbleConfiguration.Default;
        config.baseColor = GetRandomNeonColor();
        config.wavePhaseOffset = Random.Range(0f, 360f);
        config.breathingFrequency = Random.Range(0.25f, 0.35f);
        
        physics.UpdateConfiguration(config);
        
        // Set random glass clink sound
        if (glassClinkSounds.Length > 0)
        {
            AudioClip randomSound = glassClinkSounds[Random.Range(0, glassClinkSounds.Length)];
            interaction.UpdateInteractionSettings(0.5f, randomSound);
        }
        
        return bubble;
    }
    
    public GameObject SpawnBubble(Vector3 position)
    {
        if (bubblePool.Count == 0)
        {
            Debug.LogWarning("Bubble pool exhausted!");
            return null;
        }
        
        GameObject bubble = bubblePool.Dequeue();
        bubble.transform.position = position;
        bubble.SetActive(true);
        activeBubbles.Add(bubble);
        
        return bubble;
    }
    
    public void ReturnBubble(GameObject bubble)
    {
        if (activeBubbles.Contains(bubble))
        {
            activeBubbles.Remove(bubble);
            bubble.SetActive(false);
            bubblePool.Enqueue(bubble);
        }
    }
    
    Color GetRandomNeonColor()
    {
        Color[] neonColors = {
            new Color(1.0f, 0.4f, 0.8f, 0.7f), // Neon Pink
            new Color(0.8f, 0.4f, 1.0f, 0.7f), // Neon Purple
            new Color(0.4f, 0.4f, 1.0f, 0.7f), // Neon Blue
            new Color(0.4f, 1.0f, 1.0f, 0.7f)  // Neon Teal
        };
        
        return neonColors[Random.Range(0, neonColors.Length)];
    }
}
```

## Performance Optimization

### LOD System Integration
```csharp
// Add LOD Group component to prefab
LODGroup lodGroup = bubblePrefab.AddComponent<LODGroup>();

// Configure LOD levels
LOD[] lods = new LOD[3];

// LOD 0 - Close (0-30% of max distance)
lods[0] = new LOD(0.3f, new Renderer[] { highQualityRenderer });

// LOD 1 - Medium (30-60% of max distance)  
lods[1] = new LOD(0.6f, new Renderer[] { mediumQualityRenderer });

// LOD 2 - Far (60-100% of max distance)
lods[2] = new LOD(1.0f, new Renderer[] { lowQualityRenderer });

lodGroup.SetLODs(lods);
lodGroup.RecalculateBounds();
```

### GPU Instancing Setup
```csharp
// For rendering many bubbles efficiently
Graphics.DrawMeshInstanced(
    sphereMesh,
    0,
    bubbleMaterial,
    matrices,
    materialPropertyBlock
);
```

## Testing Checklist

### Prefab Validation
- [ ] All components properly configured
- [ ] Materials assigned correctly
- [ ] Audio sources working
- [ ] XR interaction responding
- [ ] Object pooling functional

### Performance Testing
- [ ] 100 bubbles maintain 72 FPS on Quest 3
- [ ] LOD transitions smooth
- [ ] Memory usage <200MB
- [ ] No garbage collection spikes

### Interaction Testing
- [ ] Hand tracking works smoothly
- [ ] Controller interaction responsive
- [ ] Haptic feedback appropriate
- [ ] Audio feedback pleasant
- [ ] Visual state changes correct