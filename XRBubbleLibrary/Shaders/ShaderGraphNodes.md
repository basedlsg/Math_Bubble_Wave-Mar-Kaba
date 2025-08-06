# Shader Graph Node Configuration

## Node Structure for Bubble Glass Shader

### Input Nodes
```
1. Base Color (Color Property)
   - Name: "_BaseColor"
   - Default: (0.8, 0.4, 1.0, 0.7)
   - HDR: Enabled

2. Glow Color (Color Property)
   - Name: "_GlowColor" 
   - Default: (1.0, 0.4, 0.8, 2.0)
   - HDR: Enabled

3. Time Node
   - For breathing animation timing

4. Normal Map (Texture2D Property)
   - Name: "_NormalMap"
   - Default: "bump"
```

### Core Glass Effect Nodes

#### Fresnel Effect Chain
```
View Direction (World Space) 
    ↓
Normal Vector (World Space)
    ↓
Dot Product
    ↓
One Minus
    ↓
Power (Fresnel Power = 2.0)
    ↓
Fresnel Output
```

#### Refraction Chain
```
Screen Position
    ↓
Normal Map Sample
    ↓
Multiply (Refraction Strength)
    ↓
Add (Screen Position + Offset)
    ↓
Scene Color (Sample with offset)
```

### Breathing Animation Chain
```
Time
    ↓
Multiply (Breathing Speed = 0.3)
    ↓
Add (Phase Offset)
    ↓
Sine
    ↓
Multiply (Breathing Intensity = 0.2)
    ↓
Breathing Pulse Output
```

### Glow Effect Chain
```
Glow Color (HDR)
    ↓
Multiply (Glow Intensity)
    ↓
Add (Breathing Pulse)
    ↓
Multiply (Fresnel for edge glow)
    ↓
Glow Output
```

### Final Color Combination
```
Base Color ──┐
             ├── Add ──→ Final Color RGB
Glow Output ─┘

Transparency ──┐
               ├── Multiply ──→ Final Alpha
Fresnel ──────┘
```

## Detailed Node Configurations

### 1. Fresnel Calculation
```
Node: Fresnel Effect
Inputs:
- Normal: Normal Vector (World Space)
- View Dir: View Direction (World Space)  
- Power: Float Property "_FresnelPower" (default: 2.0)

Purpose: Creates glass-like rim lighting effect
```

### 2. Breathing Animation
```
Node Chain: Time → Multiply → Add → Sine → Multiply

Time Node:
- Output: Time

Multiply Node 1:
- A: Time
- B: Float Property "_BreathingSpeed" (0.2-0.5)

Add Node:
- A: Previous result
- B: Float Property "_PhaseOffset" (0-6.28)

Sine Node:
- Input: Previous result

Multiply Node 2:
- A: Sine output
- B: Float Property "_BreathingIntensity" (0-0.5)

Purpose: Creates comfortable breathing oscillation
```

### 3. Underlight Glow
```
Node: Multiply
Inputs:
- A: HDR Color Property "_GlowColor"
- B: Float Property "_GlowIntensity"

Node: Add
Inputs:
- A: Previous result
- B: Breathing pulse (from animation chain)

Node: Multiply (Edge Enhancement)
Inputs:
- A: Previous result
- B: Fresnel output

Purpose: Creates neon underlight effect with breathing
```

### 4. Color Blending
```
Node: Add
Inputs:
- A: Base Color Property "_BaseColor"
- B: Glow effect output

Purpose: Combines base glass color with glow
```

### 5. Alpha Calculation
```
Node: Multiply
Inputs:
- A: Float Property "_Transparency" (0.6-0.8)
- B: Fresnel output (for edge transparency)

Node: Add
Inputs:
- A: Previous result
- B: Constant (0.3) for base transparency

Purpose: Creates realistic glass transparency with fresnel
```

## Mobile Optimization Nodes

### LOD Distance Calculation
```
Node: Distance
Inputs:
- A: World Position
- B: Camera Position (World Space)

Node: Divide
Inputs:
- A: Distance output
- B: Float Property "_DistanceFade" (5.0)

Node: Saturate
Input: Previous result

Purpose: Calculate distance-based LOD factor
```

### Simplified Color for Distance
```
Node: Lerp
Inputs:
- A: Full color calculation
- B: Base color only
- T: LOD factor × LOD Level

Purpose: Reduce shader complexity at distance
```

## Performance Guidelines

### Node Count Limits (Quest 3)
```
Total Nodes: <50 for optimal performance
Texture Samples: <3 per shader
Math Operations: Prefer simple (Add, Multiply) over complex (Power, Sine)
Branching: Avoid if/else nodes, use Lerp instead
```

### Optimization Techniques
```
1. Use Step instead of smoothstep when possible
2. Combine multiple Multiply nodes into single operation
3. Pre-calculate constants in properties
4. Use Saturate instead of Clamp(0,1)
5. Minimize texture coordinate calculations
```

## Testing Node Performance

### Shader Variants
```
Create multiple versions for testing:
1. Full quality (all nodes)
2. Medium quality (no normal map)
3. Low quality (basic color only)

Test each on Quest 3 hardware
Measure GPU time per bubble
Target: <2ms per bubble
```

### Debug Outputs
```
Temporary output nodes for testing:
- Fresnel only (to test rim lighting)
- Breathing pulse only (to test animation)
- Glow only (to test emission)
- Distance LOD only (to test optimization)

Remove debug outputs in final version
```

## Common Node Issues and Solutions

### Issue: Shader too complex for mobile
**Solution**: Reduce node count, use simpler math operations

### Issue: Breathing animation too fast/slow
**Solution**: Adjust "_BreathingSpeed" property (0.2-0.5 range)

### Issue: Glow effect not visible
**Solution**: Increase HDR intensity, check emission settings

### Issue: Transparency not working
**Solution**: Ensure material render queue is "Transparent"

### Issue: Performance drops with multiple bubbles
**Solution**: Implement GPU instancing, use LOD system