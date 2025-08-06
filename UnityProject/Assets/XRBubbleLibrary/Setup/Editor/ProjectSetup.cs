using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Linq;

namespace XRBubbleLibrary.Setup
{
    public class ProjectSetup : EditorWindow
    {
        private static ListRequest _packageListRequest;
        private static bool _particleSystemEnabled;

        [MenuItem("XR Bubble Library/Validate Project Setup")]
        public static void ShowWindow()
        {
            GetWindow<ProjectSetup>("XR Bubble Project Setup");
            CheckPackages();
        }

        private static void CheckPackages()
        {
            _packageListRequest = Client.List();
            EditorApplication.update += Progress;
        }

        private static void Progress()
        {
            if (_packageListRequest.IsCompleted)
            {
                if (_packageListRequest.Status == StatusCode.Success)
                {
                    _particleSystemEnabled = _packageListRequest.Result.Any(p => p.name == "com.unity.modules.particlesystem");
                }
                else if (_packageListRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError(_packageListRequest.Error.message);
                }
                EditorApplication.update -= Progress;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("XR Bubble Library Project Validator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("This tool checks for common project setup issues that can cause compilation errors.", MessageType.Info);

            GUILayout.Space(10);

            // Particle System Check
            GUILayout.Label("Particle System Module", EditorStyles.boldLabel);
            
            if (_particleSystemEnabled)
            {
                EditorGUILayout.HelpBox("Particle System module is enabled.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("The 'Particle System' built-in package is not enabled. This is required for many visual effects.", MessageType.Error);
                if (GUILayout.Button("Enable Particle System Module"))
                {
                    Client.Add("com.unity.modules.particlesystem");
                }
            }

            GUILayout.Space(10);

            // Assembly Definitions Check
            GUILayout.Label("Assembly Definitions", EditorStyles.boldLabel);
            string[] expectedAssemblies = { "XRBubbleLibrary.Core", "XRBubbleLibrary.Physics", "XRBubbleLibrary.Mathematics", "XRBubbleLibrary.Interactions", "XRBubbleLibrary.UI", "XRBubbleLibrary.AI" };
            bool allAssembliesFound = true;
            foreach (string assemblyName in expectedAssemblies)
            {
                if (!AssetDatabase.FindAssets($"t:asmdef {assemblyName}").Any())
                {
                    EditorGUILayout.HelpBox($"Assembly Definition '{assemblyName}' is missing.", MessageType.Error);
                    allAssembliesFound = false;
                }
            }

            if (allAssembliesFound)
            {
                EditorGUILayout.HelpBox("All required Assembly Definitions are present.", MessageType.Info);
            }
        }
    }
}
