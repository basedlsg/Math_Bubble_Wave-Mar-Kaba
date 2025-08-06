using UnityEditor;
using UnityEngine;
using XRBubbleLibrary.Integration;
using XRBubbleLibrary.UI;
using XRBubbleLibrary.Voice;
using XRBubbleLibrary.Audio;
using XRBubbleLibrary.AI;
using XRBubbleLibrary.Mathematics;
using XRBubbleLibrary.Interactions;

namespace XRBubbleLibrary.Setup
{
    public class SceneSetup
    {
        [MenuItem("XR Bubble Library/Setup Scene")]
        private static void SetupScene()
        {
            // Create the main system GameObject
            GameObject bubbleSystemGO = new GameObject("XRBubbleSystem");

            // Add all the necessary components
            bubbleSystemGO.AddComponent<AdvancedBubbleSystemIntegrator>();
            bubbleSystemGO.AddComponent<BubbleLayoutManager>();
            bubbleSystemGO.AddComponent<OnDeviceVoiceProcessor>();
            bubbleSystemGO.AddComponent<SteamAudioRenderer>();
            bubbleSystemGO.AddComponent<GroqAPIClient>();
            bubbleSystemGO.AddComponent<LocalAIModel>();
            bubbleSystemGO.AddComponent<AdvancedWaveSystem>();
            bubbleSystemGO.AddComponent<BubbleHapticFeedback>();
            bubbleSystemGO.AddComponent<CymaticsController>();
            bubbleSystemGO.AddComponent<EyeTrackingController>();

            // Find the XR Foundation Setup and link it if necessary
            XRFoundationSetup foundationSetup = Object.FindObjectOfType<XRFoundationSetup>();
            if (foundationSetup != null)
            {
                // In a more complex setup, you might link references here.
                // For now, the integrator will find them automatically.
            }

            Debug.Log("XR Bubble Library scene setup complete. The XRBubbleSystem GameObject has been created and configured.");
        }
    }
}
