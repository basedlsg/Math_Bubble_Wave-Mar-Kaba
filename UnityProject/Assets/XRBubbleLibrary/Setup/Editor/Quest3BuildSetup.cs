using UnityEngine;
using UnityEditor;

namespace XRBubbleLibrary.Setup.Editor
{
    /// <summary>
    /// Quick setup for Quest 3 build configuration
    /// </summary>
    public class Quest3BuildSetup : EditorWindow
    {
        [MenuItem("XR Bubble Library/Setup Quest 3 Build")]
        public static void SetupQuest3Build()
        {
            Debug.Log("Setting up Quest 3 build configuration...");
            
            // Set platform to Android
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            
            // Set Android settings
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // Set XR settings
            PlayerSettings.virtualRealitySupported = true;
            
            // Set rendering settings for VR performance
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.gpuSkinning = true;
            
            // Set quality settings for Quest 3
            QualitySettings.vSyncCount = 0; // Let XR handle VSync
            QualitySettings.antiAliasing = 2; // 2x MSAA for VR
            
            Debug.Log("Quest 3 build configuration complete!");
            Debug.Log("Make sure to:");
            Debug.Log("1. Install XR Interaction Toolkit package");
            Debug.Log("2. Configure XR Management for Oculus/OpenXR");
            Debug.Log("3. Set up your Quest 3 in developer mode");
        }
        
        [MenuItem("XR Bubble Library/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            // Create new scene
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, 
                UnityEditor.SceneManagement.NewSceneMode.Single);
            
            // Add XR Origin (if available)
            var xrOriginPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.unity.xr.interaction.toolkit/Runtime/XR/XROrigin.prefab");
            if (xrOriginPrefab != null)
            {
                PrefabUtility.InstantiatePrefab(xrOriginPrefab);
                Debug.Log("Added XR Origin to scene");
            }
            else
            {
                Debug.LogWarning("XR Origin prefab not found. Install XR Interaction Toolkit package.");
            }
            
            // Create demo setup object
            var demoSetup = new GameObject("QuickDemoSetup");
            demoSetup.AddComponent<QuickDemoSetup>();
            
            // Save scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/WaveMatrixDemo.unity");
            
            Debug.Log("Demo scene created: Assets/Scenes/WaveMatrixDemo.unity");
        }
    }
}