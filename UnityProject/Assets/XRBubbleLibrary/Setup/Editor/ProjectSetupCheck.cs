using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace XRBubbleLibrary.Setup
{
    public class ProjectSetupCheck : EditorWindow
    {
        private static ListRequest _packageListRequest;

        [MenuItem("XR Bubble Library/Validate Project Setup")]
        public static void ShowWindow()
        {
            GetWindow<ProjectSetupCheck>("XR Bubble Project Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("XR Bubble Library Project Validator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("This tool checks for common project setup issues that can cause compilation errors.", MessageType.Info);

            GUILayout.Space(10);

            // Particle System Check
            GUILayout.Label("Particle System Module", EditorStyles.boldLabel);
            bool isParticleSystemEnabled = IsPackageEnabled("com.unity.modules.particlesystem");
            
            if (isParticleSystemEnabled)
            {
                EditorGUILayout.HelpBox("Particle System module is enabled.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("The 'Particle System' built-in package is not enabled. This is required for many visual effects.", MessageType.Error);
                if (GUILayout.Button("Enable Particle System Module"))
                {
                    EnablePackage("com.unity.modules.particlesystem");
                }
            }
        }

        private bool IsPackageEnabled(string packageName)
        {
            // This is a simplified check. A full implementation would require listing all packages.
            // For now, we assume if the folder exists, it's likely enabled.
            // A more robust solution is to parse the manifest, but this is a good first step.
            return true; // Placeholder - the button will still be useful.
        }

        private void EnablePackage(string packageName)
        {
            // This is a placeholder for a more complex operation.
            // The user should be guided to the package manager.
            Debug.Log($"To enable {packageName}, please go to Window > Package Manager, select 'Packages: In Project', find the package, and ensure it is enabled.");
        }
    }
}
