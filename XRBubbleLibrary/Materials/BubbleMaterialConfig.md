# Bubble Material Configuration

## Material Variants for Different States

### 1. Normal State Material
```
Material Name: BubbleGlass_Normal
Base Color: Neon Purple (0.8, 0.4, 1.0, 0.7)
Glow Color: Soft Purple (1.0, 0.4, 0.8, 1.5)
Glow Intensity: 0.6
Breathing Speed: 0.3 Hz
Transparency: 0.7
```

### 2. Hover State Material  
```
Material Name: BubbleGlass_Hover
Base Color: Neon Blue (0.4, 0.4, 1.0, 0.8)
Glow Color: Bright Blue (0.4, 0.8, 1.0, 2.0)
Glow Intensity: 0.8
Breathing Speed: 0.4 Hz (slightly faster)
Transparency: 0.8
```

### 3. Pressed State Material
```
Material Name: BubbleGlass_Pressed
Base Color: Neon Pink (1.0, 0.4, 0.8, 0.9)
Glow Color: Intense Pink (1.0, 0.2, 0.6, 2.5)
Glow Intensity: 1.0
Breathing Speed: 0.5 Hz (fastest)
Transparency: 0.9
```

## Neon-Pastel Color Palette Implementation

### Primary Colors (HDR Values)
```csharp
// Neon Pink - Warm, inviting
public static Color NeonPink = new Color(1.0f, 0.4f, 0.8f, 0.7f) * 1.5f;

// Neon Purple - Mysterious, elegant  
public static Color NeonPurple = new Color(0.8f, 0.4f, 1.0f, 0.7f) * 1.5f;

// Neon Blue - Cool, calming
public static Color NeonBlue = new Color(0.4f, 0.4f, 1.0f, 0.7f) * 1.5f;

// Neon Teal - Fresh, energetic
public static Color NeonTeal = new Color(0.4f, 1.0f, 1.0f, 0.7f) * 1.5f;
```

### Gradient Transitions
```csharp
// Smooth HSV interpolation for natural color flow
public static Color InterpolateNeonColors(float t)
{
    // t = 0.0 to 1.0 across the color spectrum
    if (t < 0.33f)
        return Color.Lerp(NeonPink, NeonPurple, t * 3.0f);
    else if (t < 0.66f)
        return Color.Lerp(NeonPurple, NeonBlue, (t - 0.33f) * 3.0f);
    else
        return Color.Lerp(NeonBlue, NeonTeal, (t - 0.66f) * 3.0f);
}
```

## Material Property Animation

### Breathing Effect Implementation
```csharp
// Animate glow intensity for breathing effect
void UpdateBreathingEffect()
{
    float time = Time.time * breathingSpeed + phaseOffset;
    float breathingPulse = Mathf.Sin(time) * breathingIntensity;
    
    // Apply to material
    material.SetFloat("_GlowIntensity", baseGlowIntensity + breathingPulse);
    
    // Subtle scale breathing (optional)
    float scaleBreathing = 1.0f + (breathingPulse * 0.02f);
    transform.localScale = Vector3.one * baseScale * scaleBreathing;
}
```

### Color Transition Animation
```csharp
// Smooth color transitions between states
IEnumerator TransitionToColor(Color targetColor, float duration)
{
    Color startColor = material.GetColor("_BaseColor");
    float elapsed = 0.0f;
    
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        
        // Smooth curve for natural transition
        t = Mathf.SmoothStep(0.0f, 1.0f, t);
        
        Color currentColor = Color.Lerp(startColor, targetColor, t);
        material.SetColor("_BaseColor", currentColor);
        
        yield return null;
    }
}
```

## Performance Optimization Settings

### LOD Material Variants
```
LOD 0 (Close): Full shader with all effects
- All breathing animation
- Full glow effects
- High-quality transparency
- Normal map sampling

LOD 1 (Medium): Reduced effects
- Simplified breathing (no scale)
- Reduced glow intensity
- Standard transparency
- No normal map

LOD 2 (Far): Minimal effects
- No breathing animation
- Basic glow only
- Simple transparency
- Flat shading
```

### Quest 3 Mobile Optimization
```
Texture Settings:
- Max Size: 512x512 for normal maps
- Compression: ASTC 6x6 for Quest 3
- Mip Maps: Enabled for distance rendering
- Filter Mode: Bilinear (not Trilinear)

Shader Keywords:
- _NORMALMAP (disable for LOD 2)
- _BREATHING_ANIMATION (disable for LOD 2)
- _DISTANCE_FADE (always enabled)
```

## Material Creation Script
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "BubbleMaterial", menuName = "XR Bubble Library/Bubble Material")]
public class BubbleMaterialConfig : ScriptableObject
{
    [Header("Visual Properties")]
    public Color baseColor = new Color(0.8f, 0.4f, 1.0f, 0.7f);
    public Color glowColor = new Color(1.0f, 0.4f, 0.8f, 1.5f);
    public float glowIntensity = 0.6f;
    public float transparency = 0.7f;
    
    [Header("Animation")]
    public float breathingSpeed = 0.3f;
    public float breathingIntensity = 0.2f;
    public float phaseOffset = 0.0f;
    
    [Header("Performance")]
    public int lodLevel = 0;
    
    public Material CreateMaterial(Shader bubbleShader)
    {
        Material mat = new Material(bubbleShader);
        
        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_GlowColor", glowColor);
        mat.SetFloat("_GlowIntensity", glowIntensity);
        mat.SetFloat("_Transparency", transparency);
        mat.SetFloat("_BreathingSpeed", breathingSpeed);
        mat.SetFloat("_BreathingIntensity", breathingIntensity);
        mat.SetFloat("_PhaseOffset", phaseOffset);
        mat.SetFloat("_LODLevel", lodLevel);
        
        return mat;
    }
}
```

## Testing and Validation

### Visual Quality Checklist
- [ ] Glass transparency looks realistic
- [ ] Neon colors are vibrant but not harsh
- [ ] Underlight glow is visible and pleasant
- [ ] Breathing animation is subtle and comfortable
- [ ] Color transitions are smooth
- [ ] Multiple bubbles look harmonious together

### Performance Checklist
- [ ] 72 FPS maintained with 50+ bubbles on Quest 3
- [ ] GPU memory usage <100MB for materials
- [ ] Draw calls <50 for 100 bubbles (with instancing)
- [ ] Shader compilation time <5 seconds
- [ ] LOD transitions are smooth and unnoticeable