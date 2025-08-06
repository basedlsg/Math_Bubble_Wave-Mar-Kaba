# Unity XR Foundation Setup - Task 1

## Project Configuration
**Unity Version**: 2023.3.5f1 (LTS)
**Template**: URP (Universal Render Pipeline) 14.0.9
**Target Platforms**: Quest 3 (Android) + Windows x64

## Required Packages
```json
{
  "com.unity.xr.interaction.toolkit": "2.5.4",
  "com.unity.xr.core-utils": "2.2.3", 
  "com.unity.xr.hands": "1.3.0",
  "com.unity.xr.openxr": "1.10.0",
  "com.unity.render-pipelines.universal": "14.0.9"
}
```

## Setup Steps Completed
1. ✅ Created project directory structure
2. ⏳ Unity project creation (requires Unity Hub)
3. ⏳ Package installation
4. ⏳ XR provider configuration
5. ⏳ Build settings configuration

## Clone Sources Identified
- **XR Interaction Toolkit Samples**: https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples
- **Unity UI Toolkit Samples**: https://github.com/Unity-Technologies/ui-toolkit-sample-projects
- **Unity Shader Graph Samples**: Unity Asset Store - "Shader Graph Sample Projects"

## Next Steps
1. Create Unity project using Unity Hub
2. Install required XR packages via Package Manager
3. Clone XR Interaction Toolkit samples from GitHub
4. Configure OpenXR provider for Quest 3
5. Set up build configurations for Android and Windows

## Performance Targets
- Quest 3: 72 FPS sustained (Snapdragon XR2 Gen 2)
- Windows PC: 90 FPS (RTX 3070 minimum)
- Memory: <400MB on Quest 3