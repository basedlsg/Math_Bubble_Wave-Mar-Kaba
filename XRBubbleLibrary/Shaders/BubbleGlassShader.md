# Bubble Glass Shader - Clone and Modify Guide

## Base Shader to Clone
**Source**: Unity Shader Graph Sample Projects - "Glass Material"
**Asset Store**: Search "Shader Graph Sample Projects" 
**Alternative**: Unity Learn - URP Shader Graph tutorials

## Neon-Pastel Color Palette
```
Primary Colors (HSV):
- Neon Pink: H=320°, S=60%, V=100% (RGB: 1.0, 0.4, 0.8)
- Neon Blue: H=240°, S=60%, V=100% (RGB: 0.4, 0.4, 1.0)  
- Neon Purple: H=280°, S=60%, V=100% (RGB: 0.8, 0.4, 1.0)
- Neon Teal: H=180°, S=60%, V=100% (RGB: 0.4, 1.0, 1.0)

Gradient Transitions:
- Pink → Purple → Blue → Teal (smooth HSV interpolation)
- Alpha: 0.6-0.8 for glass transparency
- HDR Intensity: 1.5-2.0 for glow effect
```

## Shader Graph Modifications Required

### 1. Base Glass Properties (Clone from Unity samples)
```
Nodes to Clone:
- Fresnel Effect (for rim lighting)
- Refraction (for glass distortion)
- Transparency (for see-through effect)
- Normal Map (for surface detail)

Modifications:
- Replace static colors with neon-pastel gradient
- Increase emission intensity for glow
- Add HDR color support
- Optimize for mobile (Quest 3)
```

### 2. Underlight Glow System
```
New Nodes to Add:
- Emission node with HDR color input
- Multiply node for glow intensity
- Add node to combine with base color
- Fresnel mask for edge glow enhancement

Parameters:
- Glow Intensity: 0.0-2.0 (default 0.6)
- Glow Color: HDR neon-pastel
- Glow Falloff: 0.5-2.0 (default 1.0)
```

### 3. Breathing Animation Support
```
Animation Nodes:
- Time node for animation timing
- Sine node for breathing oscillation
- Multiply node for amplitude control
- Add to emission intensity

Parameters:
- Breathing Speed: 0.2-0.5 Hz
- Breathing Intensity: 0.1-0.3
- Phase Offset: 0.0-360° (for variation)
```

### 4. Quest 3 Mobile Optimization
```
Optimizations:
- Reduce texture samples (max 3 per shader)
- Use simple math operations
- Avoid complex branching
- Use LOD for distance-based quality
- Limit instruction count <100

Performance Targets:
- <2ms GPU time per bubble
- <50 draw calls for 100 bubbles
- <100MB texture memory
```

## Shader Properties Interface
```csharp
Properties
{
    [Header(Base Glass)]
    _BaseColor("Base Color", Color) = (0.8, 0.4, 1.0, 0.7)
    _Transparency("Transparency", Range(0, 1)) = 0.7
    _Refraction("Refraction Strength", Range(0, 1)) = 0.3
    _FresnelPower("Fresnel Power", Range(0, 5)) = 2.0
    
    [Header(Neon Glow)]
    [HDR] _GlowColor("Glow Color", Color) = (1.0, 0.4, 0.8, 1.0)
    _GlowIntensity("Glow Intensity", Range(0, 2)) = 0.6
    _GlowFalloff("Glow Falloff", Range(0.5, 2)) = 1.0
    
    [Header(Breathing Animation)]
    _BreathingSpeed("Breathing Speed", Range(0.2, 0.5)) = 0.3
    _BreathingIntensity("Breathing Intensity", Range(0, 0.5)) = 0.2
    _PhaseOffset("Phase Offset", Range(0, 360)) = 0
    
    [Header(Performance)]
    _LODLevel("LOD Level", Range(0, 2)) = 0
}
```

## Implementation Steps

### Step 1: Clone Base Glass Shader
1. Download Unity Shader Graph samples from Asset Store
2. Locate glass/transparent material examples
3. Copy shader graph to project
4. Rename to "BubbleGlass"

### Step 2: Modify Color System
1. Replace static color with gradient system
2. Add HDR color properties
3. Implement neon-pastel palette
4. Test color transitions

### Step 3: Add Underlight Glow
1. Add emission nodes to shader graph
2. Connect HDR glow color
3. Multiply with fresnel for edge enhancement
4. Test glow intensity and falloff

### Step 4: Implement Breathing Animation
1. Add time-based sine wave
2. Connect to emission intensity
3. Add phase offset for variation
4. Test breathing frequency (0.2-0.5 Hz)

### Step 5: Mobile Optimization
1. Reduce node complexity
2. Optimize texture usage
3. Add LOD system
4. Test on Quest 3 performance

## Testing Checklist
- [ ] Glass transparency works correctly
- [ ] Neon-pastel colors display properly
- [ ] Underlight glow is visible and pleasant
- [ ] Breathing animation is comfortable (not distracting)
- [ ] Performance meets Quest 3 targets (72 FPS)
- [ ] Shader compiles without errors
- [ ] Multiple bubbles render efficiently

## Common Issues and Solutions
**Issue**: Shader too complex for Quest 3
**Solution**: Reduce node count, use simpler math operations

**Issue**: Colors appear washed out
**Solution**: Increase HDR intensity, check gamma correction

**Issue**: Breathing animation too fast/slow
**Solution**: Adjust frequency to 0.2-0.5 Hz range

**Issue**: Glow effect not visible
**Solution**: Increase emission intensity, check HDR settings

**Issue**: Performance drops with multiple bubbles
**Solution**: Implement GPU instancing, use LOD system