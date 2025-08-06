# Unity Integration Guide - AI-Enhanced XR Bubble Library

## Phase 1: Unity Project Setup

### Prerequisites
- Unity Hub installed
- Unity 2023.3.5f1 (LTS) or newer
- Quest 3 development setup (if available)
- Windows PC with decent GPU for testing

### Step 1: Create Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select "3D (URP)" template
4. Project Name: "AIEnhancedXRBubbles"
5. Location: Choose appropriate directory
6. Unity Version: 2023.3.5f1 (LTS)
7. Click "Create Project"

### Step 2: Configure Project Settings

#### Build Settings
1. File → Build Settings
2. Switch Platform to "Android" (for Quest 3)
3. Add current scene to build
4. Player Settings:
   - Company Name: [Your Company]
   - Product Name: "AI Enhanced XR Bubbles"
   - Version: 0.1.0
   - Bundle Identifier: com.[company].aienhancedxrbubbles

#### XR Settings
1. Edit → Project Settings → XR Plug-in Management
2. Install XR Plug-in Management
3. Enable "OpenXR" provider
4. OpenXR Settings:
   - Add "Meta Quest Support" feature
   - Add "Hand Tracking" feature
   - Add "Eye Tracking" feature (if available)

#### Android Settings (Quest 3)
1. Player Settings → Android
2. Minimum API Level: Android 10.0 (API level 29)
3. Target API Level: Android 13.0 (API level 33)
4. Scripting Backend: IL2CPP
5. Target Architectures: ARM64
6. Graphics APIs: OpenGLES3, Vulkan

### Step 3: Install Required Packages

Open Package Manager (Window → Package Manager) and install:

#### XR Packages
- XR Interaction Toolkit (2.5.4+)
- XR Core Utilities (2.2.3+)
- XR Hands (1.3.0+)
- OpenXR Plugin (1.10.0+)

#### Performance Packages
- Unity Mathematics
- Unity Collections
- Unity Jobs
- Unity Burst

#### Audio Packages
- Audio (built-in)
- Timeline (for audio sequencing)

#### AI/ML Packages (if available)
- ML-Agents (optional)
- Barracuda (for local AI inference)

### Step 4: Project Structure Setup

Create the following folder structure in Assets:
```
Assets/
├── XRBubbleLibrary/
│   ├── AI/
│   ├── Audio/
│   ├── Integration/
│   ├── Mathematics/
│   ├── Materials/
│   ├── Performance/
│   ├── Physics/
│   ├── Prefabs/
│   ├── Scripts/
│   ├── Shaders/
│   ├── UI/
│   └── Voice/
├── Scenes/
├── StreamingAssets/
└── Resources/
```

### Step 5: Import Component Scripts

Copy all our developed scripts into the appropriate folders:
- Copy XRBubbleLibrary/* to Assets/XRBubbleLibrary/
- Maintain the same folder structure

### Step 6: Configure URP Settings

1. Assets → Create → Rendering → URP Asset (with 2D Renderer)
2. Name it "XRBubbleURP"
3. Edit → Project Settings → Graphics
4. Set Scriptable Render Pipeline Settings to "XRBubbleURP"
5. Configure URP Asset:
   - Rendering Path: Forward+
   - Depth Texture: Enabled
   - Opaque Texture: Enabled
   - HDR: Enabled
   - MSAA: 4x (for Quest 3 optimization)

### Step 7: Scene Setup

1. Create new scene: "XRBubbleTestScene"
2. Delete default Main Camera
3. Add XR Origin (Action-based) from XR Interaction Toolkit
4. Configure XR Origin:
   - Add Locomotion System
   - Add Teleportation Provider
   - Configure Hand Tracking (if available)

### Step 8: Initial Component Integration

Create empty GameObjects for our main systems:
1. "AIEnhancedBubbleSystem" (add AIEnhancedBubbleSystem script)
2. "PerformanceManager" (add BubblePerformanceManager script)
3. "AudioRenderer" (add SteamAudioRenderer script)
4. "VoiceProcessor" (add OnDeviceVoiceProcessor script)

### Step 9: Basic Testing Setup

Create a simple test script to verify everything is working:

```csharp
using UnityEngine;
using XRBubbleLibrary.Integration;

public class InitialTest : MonoBehaviour
{
    public AIEnhancedBubbleSystem bubbleSystem;
    
    void Start()
    {
        Debug.Log("AI-Enhanced XR Bubble Library - Initial Test");
        
        if (bubbleSystem != null)
        {
            Debug.Log("Bubble system found and ready");
        }
        else
        {
            Debug.LogError("Bubble system not found!");
        }
    }
    
    void Update()
    {
        // Test input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed - testing voice command");
            bubbleSystem?.TestVoiceCommand("arrange in circle");
        }
    }
}
```

### Step 10: Build Test

1. File → Build Settings
2. Add current scene
3. Build and Run (if Quest 3 connected)
4. Or Build to APK for sideloading

## Next Steps

After completing Phase 1, we'll proceed to:
- Phase 2: Component Implementation
- Phase 3: System Integration  
- Phase 4: Performance Validation

## Troubleshooting

### Common Issues:
1. **XR packages not installing**: Check Unity version compatibility
2. **Build errors**: Ensure all dependencies are installed
3. **Quest 3 not detected**: Enable Developer Mode and USB Debugging
4. **Performance issues**: Check URP settings and reduce quality if needed

### Performance Optimization Tips:
1. Use Forward+ rendering for better XR performance
2. Enable GPU Instancing where possible
3. Use LOD systems for complex objects
4. Monitor frame rate with Unity Profiler
5. Test on actual Quest 3 hardware regularly

This setup provides the foundation for implementing our AI-enhanced XR bubble system in Unity.