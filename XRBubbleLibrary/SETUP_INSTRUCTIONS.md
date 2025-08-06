# XR Bubble Library Setup Instructions

## Prerequisites
- Unity Hub installed
- Unity 2023.3.5f1 (LTS) installed via Unity Hub
- Git installed for cloning samples
- Quest 3 device for testing (optional but recommended)

## Step 1: Create Unity Project
1. Open Unity Hub
2. Click "New Project"
3. Select "3D (URP)" template
4. Set project name: "XRBubbleLibrary"
5. Set location: Current directory
6. Click "Create Project"

## Step 2: Install Required Packages
**Important**: Before installing the packages below, go to the Package Manager, select the **Packages: In Project** view, find the **Particle System** module, and ensure it is enabled.

Open Package Manager (Window > Package Manager) and install:

```
1. XR Interaction Toolkit (2.5.4)
   - Window > Package Manager
   - Unity Registry
   - Search "XR Interaction Toolkit"
   - Install version 2.5.4

2. XR Core Utilities (2.2.3)
   - Should install automatically with XR Interaction Toolkit
   - If not, search and install manually

3. XR Hands (1.3.0)
   - Search "XR Hands" in Package Manager
   - Install version 1.3.0

4. OpenXR Plugin (1.10.0)
   - Search "OpenXR" in Package Manager
   - Install version 1.10.0
```

## Step 3: Configure XR Settings
1. Go to Edit > Project Settings
2. Navigate to XR Plug-in Management
3. Check "OpenXR" for both Standalone and Android platforms
4. Click on OpenXR settings
5. Add "Meta Quest Support" feature group
6. Add "Hand Tracking" feature

## Step 4: Clone Sample Repositories
Open terminal/command prompt in project root and run:

```bash
# Clone XR Interaction Toolkit samples
git clone https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples.git Samples/XRInteractionSamples

# Clone UI Toolkit samples  
git clone https://github.com/Unity-Technologies/ui-toolkit-sample-projects.git Samples/UIToolkitSamples

# Clone Unity Learn samples (if available)
git clone https://github.com/Unity-Technologies/Unity-Learn-Examples.git Samples/UnityLearnSamples
```

## Step 5: Import Created Scripts
1. Copy all files from `Scripts/` folder into `Assets/Scripts/`
2. The scripts are already configured for the clone-and-modify approach
3. Scripts included:
   - `BubbleConfiguration.cs` - Configuration data structure
   - `BubblePhysics.cs` - Physics and breathing animation
   - `BubbleInteraction.cs` - XR interaction handling

## Step 6: Configure Build Settings
### For Quest 3 (Android):
1. File > Build Settings
2. Switch Platform to Android
3. Set Texture Compression to ASTC
4. Player Settings:
   - Minimum API Level: 29
   - Target API Level: 33
   - Scripting Backend: IL2CPP
   - Architecture: ARM64

### For Windows PC:
1. File > Build Settings
2. Switch Platform to Windows, Mac, Linux
3. Architecture: x86_64
4. Player Settings:
   - Scripting Backend: Mono
   - Api Compatibility: .NET Standard 2.1

## Step 7: Extract and Modify Samples
1. **Glass Shaders**: Extract from Unity Shader Graph samples
   - Look for transparency, fresnel, and emission examples
   - Modify colors for neon-pastel palette (pink, blue, purple, teal)
   - Add underlight glow effects

2. **XR Interactions**: Extract from XR Interaction Toolkit samples
   - Copy XRBaseInteractable examples
   - Modify for bubble-specific interactions
   - Integrate with created BubbleInteraction.cs

3. **Physics Systems**: Extract from Unity physics samples
   - Copy spring physics examples
   - Modify for breathing animation (0.2-0.5 Hz)
   - Integrate with created BubblePhysics.cs

## Step 8: Create Test Scene
1. Create new scene: "BubbleTestScene"
2. Add XR Origin from XR Interaction Toolkit
3. Create bubble prefab using created scripts
4. Test in XR Device Simulator or Quest 3

## Current Implementation Status
✅ **Completed**:
- Project structure and documentation
- Core bubble scripts with clone-and-modify approach
- Build settings configuration
- Sample cloning strategy

⏳ **Next Tasks**:
- Unity project creation and package installation
- Sample repository cloning and integration
- Shader modification for neon-pastel aesthetic
- XR interaction integration and testing

## Troubleshooting
- **XR packages not installing**: Ensure Unity 2023.3.5f1 LTS is used
- **Build errors**: Check Android SDK and NDK are properly configured
- **Performance issues**: Use Unity Profiler to identify bottlenecks
- **Quest 3 not detected**: Enable Developer Mode and USB Debugging

## Performance Targets
- Quest 3: 72 FPS sustained, <400MB memory
- Windows PC: 90 FPS, <1GB memory
- Bubble count: 50-100 simultaneous bubbles with LOD system

## Next Developer Notes
- Follow clone-and-modify approach - never build from scratch
- Maintain neon-pastel aesthetic throughout
- Test frequently on Quest 3 hardware
- Update Cline memory file with progress
- Focus on visual polish over complex features
