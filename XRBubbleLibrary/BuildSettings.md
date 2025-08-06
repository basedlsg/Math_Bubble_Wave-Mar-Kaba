# Unity Build Settings Configuration

## Quest 3 (Android) Build Settings
```
Platform: Android
Architecture: ARM64
Minimum API Level: 29 (Android 10)
Target API Level: 33 (Android 13)
Scripting Backend: IL2CPP
Api Compatibility Level: .NET Standard 2.1
```

## Windows PC Build Settings  
```
Platform: Windows, Mac, Linux
Architecture: x86_64
Scripting Backend: Mono
Api Compatibility Level: .NET Standard 2.1
```

## XR Settings
```
XR Plug-in Management:
- OpenXR (enabled)
- Oculus (disabled - using OpenXR instead)

OpenXR Feature Groups:
- Meta Quest Support
- Hand Tracking
- Eye Tracking (if available)
```

## URP Settings for XR
```
Rendering Path: Forward+
MSAA: 4x (Quest 3), 8x (Windows PC)
HDR: Enabled
Post Processing: Enabled (optimized for mobile)
Shadows: Soft Shadows, optimized cascade settings
```

## Performance Optimization Settings
```
Quality Settings:
- Quest 3: Medium quality preset, customized
- Windows PC: High quality preset

Graphics Settings:
- Texture Quality: Full (Windows), Half (Quest 3)
- Anisotropic Filtering: Per Texture
- Anti Aliasing: MSAA via URP
```