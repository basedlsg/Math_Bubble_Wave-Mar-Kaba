# Bubble Glass Shader System

## Overview

This shader system is **cloned and adapted from Unity's official glass and transparency shader samples**, optimized specifically for Quest 3 mobile GPU performance.

## Cloned Sample Sources

### Unity Glass Shader Samples
- **Base Glass Effect**: Cloned from Unity URP glass shader samples
- **Fresnel Calculation**: Adapted from Unity fresnel effect samples
- **Transparency Handling**: Based on Unity transparent shader samples
- **Emission Glow**: Cloned from Unity emission shader samples

### Unity Mobile Optimization Samples
- **LOD System**: Adapted from Unity mobile LOD samples
- **GPU Instancing**: Cloned from Unity batching samples
- **Performance Monitoring**: Based on Unity profiler samples
- **Adaptive Quality**: Adapted from Unity adaptive quality samples

## Shader Components

### 1. BubbleGlass.shader
**Cloned from**: Unity URP glass shader samples

**Key Features**:
- Fresnel rim lighting effect (Unity glass samples)
- Neon-pastel color blending (adapted for bubble aesthetic)
- Mobile GPU optimization (Unity Quest samples)
- Transparency with proper alpha blending (Unity transparent samples)

**Neon-Pastel Color Palette**:
- Neon Pink: `(1, 0.4, 0.8, 0.8)`
- Neon Blue: `(0.4, 0.8, 1, 0.8)`
- Neon Purple: `(0.8, 0.4, 1, 0.8)`
- Neon Teal: `(0.4, 1, 0.8, 0.8)`

### 2. BubbleShaderOptimizer.cs
**Cloned from**: Unity mobile optimization samples

**Key Features**:
- Real-time performance monitoring (Unity profiler samples)
- Adaptive quality adjustment (Unity adaptive quality samples)
- LOD-based optimization (Unity LOD samples)
- Quest 3 specific optimizations (Unity Quest samples)

### 3. BubbleGlassMaterial.mat
**Based on**: Unity material configuration samples

**Optimized Settings**:
- Transparency queue for proper rendering order
- GPU instancing enabled for batching
- Mobile-optimized property values

## Performance Optimization

### Quest 3 Specific Optimizations
**Cloned from Unity Quest optimization samples**:

1. **Reduced Shader Complexity**:
   - Simplified fresnel calculations
   - Optimized color blending
   - Reduced texture sampling

2. **LOD System**:
   - Distance-based quality reduction
   - Automatic culling beyond max distance
   - Fade-out transitions

3. **Adaptive Quality**:
   - Real-time FPS monitoring
   - Automatic quality adjustment
   - Thermal throttling response

### Performance Targets
- **Quest 3**: 72 FPS sustained
- **Windows PC**: 90 FPS sustained
- **Thermal Management**: Automatic quality reduction

## Usage Instructions

### 1. Basic Setup
```csharp
// Apply bubble material to renderer (Unity material samples)
Renderer bubbleRenderer = GetComponent<Renderer>();
bubbleRenderer.material = bubbleGlassMaterial;
```

### 2. Performance Optimization
```csharp
// Add optimizer to scene (Unity optimization samples)
GameObject optimizer = new GameObject("Bubble Shader Optimizer");
optimizer.AddComponent<BubbleShaderOptimizer>();
```

### 3. Quality Control
```csharp
// Manual quality override (Unity quality samples)
BubbleShaderOptimizer optimizer = FindObjectOfType<BubbleShaderOptimizer>();
optimizer.ForceHighQuality(); // or ForceLowQuality()
```

## Shader Properties

### Visual Properties
- `_BubbleColor`: Base bubble color
- `_FresnelPower`: Rim lighting intensity (1.0-5.0)
- `_GlowIntensity`: Emission glow strength (0.0-3.0)
- `_Transparency`: Overall transparency (0.0-1.0)

### Neon Color Properties
- `_NeonPink`: Pink color variation
- `_NeonBlue`: Blue color variation
- `_NeonPurple`: Purple color variation
- `_NeonTeal`: Teal color variation

### Performance Properties
- `_LODFade`: Distance-based fade (0.0-1.0)
- `_Refraction`: Refraction effect strength (0.0-1.0)

## Integration with Unity Systems

### URP Integration
**Cloned from Unity URP samples**:
- Forward rendering pass
- Shadow caster pass
- Proper transparency queue
- URP lighting integration

### XR Integration
**Based on Unity XR shader samples**:
- Single-pass stereo rendering support
- Mobile GPU optimization
- VR-specific performance considerations

### Batching Integration
**Cloned from Unity batching samples**:
- GPU instancing support
- Dynamic batching compatibility
- Draw call optimization

## Troubleshooting

### Performance Issues
1. **Low FPS**: Optimizer will automatically reduce quality
2. **Thermal Throttling**: Quality will drop to maintain performance
3. **Draw Call Spikes**: Ensure GPU instancing is enabled

### Visual Issues
1. **Transparency Sorting**: Ensure proper render queue order
2. **Color Banding**: Increase color precision in quality settings
3. **Fresnel Too Strong**: Reduce `_FresnelPower` value

### Quest 3 Specific Issues
1. **Overheating**: Optimizer will reduce quality automatically
2. **Battery Drain**: Lower `_GlowIntensity` for power savings
3. **Tracking Loss**: Shader continues to work in 3DOF mode

## Development Notes

### Clone-and-Modify Approach Benefits
1. **Proven Stability**: Built on tested Unity samples
2. **Best Practices**: Inherits Unity's shader optimization patterns
3. **Maintainability**: Easy to update with new Unity releases
4. **Documentation**: Extensive Unity shader documentation available

### Future Enhancements
1. **Additional Color Palettes**: Clone more Unity color samples
2. **Advanced Effects**: Adapt Unity particle shader samples
3. **Performance Improvements**: Integrate newer Unity optimization samples
4. **Platform Expansion**: Clone Unity console optimization samples

---

**Development Philosophy**: "Clone proven Unity samples, adapt for bubble-specific needs, optimize for target hardware."