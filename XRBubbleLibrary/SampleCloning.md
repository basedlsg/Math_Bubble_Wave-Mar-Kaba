# Unity Sample Cloning Instructions

## XR Interaction Toolkit Samples
**Source**: https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples
**Clone Command**: `git clone https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples.git`

**Key Samples to Extract**:
- `XR Origin Setup` - Basic XR camera rig configuration
- `Hand Tracking Samples` - Hand interaction examples
- `Haptic Feedback Examples` - Tactile response patterns
- `XR Device Simulator` - Testing without headset

## UI Toolkit Samples
**Source**: https://github.com/Unity-Technologies/ui-toolkit-sample-projects
**Clone Command**: `git clone https://github.com/Unity-Technologies/ui-toolkit-sample-projects.git`

**Key Samples to Extract**:
- `Runtime UI` - 3D world space UI examples
- `Layout Systems` - Flexible UI arrangement
- `Custom Controls` - Bubble-shaped UI elements
- `Styling Examples` - Visual customization patterns

## Shader Graph Samples
**Source**: Unity Asset Store - "Shader Graph Sample Projects"
**Download**: Via Unity Package Manager or Asset Store

**Key Shaders to Clone**:
- `Glass Material` - Transparency and refraction
- `Fresnel Effect` - Realistic bubble appearance  
- `Emission Glow` - Underlight effects
- `Gradient Systems` - Color transitions

## Audio Samples
**Source**: Unity Learn Audio Examples
**Location**: Unity Package Manager - Audio samples

**Key Audio to Clone**:
- `Procedural Audio` - Generated sound effects
- `Glass/Crystal Sounds` - Clinking interaction audio
- `Spatial Audio` - 3D positioned sound
- `Audio Filters` - Harmonic resonance effects

## Physics Samples
**Source**: Unity Learn Physics Examples
**Location**: Unity Documentation and Learn platform

**Key Physics to Clone**:
- `Spring Systems` - Oscillation and damping
- `Particle Systems` - Breathing animation patterns
- `Object Pooling` - Performance optimization
- `LOD Systems` - Distance-based quality

## Implementation Strategy
1. **Clone entire repositories** to local development machine
2. **Extract relevant scripts** and prefabs into XR Bubble project
3. **Modify for neon-pastel aesthetic** and wave-based behavior
4. **Test performance** on Quest 3 hardware constraints
5. **Integrate systems** for cohesive bubble experience

## Modification Guidelines
- **Colors**: Change to pink, blue, purple, teal gradients
- **Animation**: Add 0.2-0.5 Hz breathing oscillation
- **Audio**: Modify for pleasant glass clinking sounds
- **Haptics**: Adjust for comfortable bubble interaction
- **Performance**: Optimize for Quest 3 mobile GPU