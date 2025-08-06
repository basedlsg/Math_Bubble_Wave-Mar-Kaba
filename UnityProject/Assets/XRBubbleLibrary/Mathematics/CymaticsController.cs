using UnityEngine;
using XRBubbleLibrary.Core;

namespace XRBubbleLibrary.Mathematics
{
    /// <summary>
    /// Manages the real-time generation of Cymatics-inspired visual patterns
    /// based on audio frequencies from any IAudioRenderer implementation.
    /// 
    /// Architecture Notes:
    /// - Uses IAudioRenderer interface to avoid circular assembly dependencies
    /// - Follows Unity dependency injection best practices
    /// - Compatible with any audio renderer that implements IAudioRenderer
    /// </summary>
    public class CymaticsController : MonoBehaviour
    {
        [Header("System References")]
        [Tooltip("Any component implementing IAudioRenderer (e.g., SteamAudioRenderer)")]
        public MonoBehaviour audioRendererComponent;

        [Header("Cymatics Configuration")]
        public Material bubbleMaterial; // Material to apply the patterns to
        public int textureSize = 256;
        public float patternScale = 10.0f;
        public float patternComplexity = 2.0f;

        private Texture2D cymaticsTexture;
        private Color[] pixels;
        private IAudioRenderer audioRenderer;

        void Start()
        {
            // Get the audio renderer interface from the assigned component
            if (audioRendererComponent != null)
            {
                audioRenderer = audioRendererComponent as IAudioRenderer;
                if (audioRenderer == null)
                {
                    Debug.LogError($"CymaticsController: {audioRendererComponent.name} does not implement IAudioRenderer interface!");
                }
            }
            else
            {
                Debug.LogWarning("CymaticsController: No audioRendererComponent assigned. Please assign a component that implements IAudioRenderer.");
            }

            InitializeTexture();
        }

        void Update()
        {
            if (audioRenderer != null && bubbleMaterial != null && audioRenderer.IsPlaying)
            {
                UpdateCymaticsPattern();
            }
        }

        private void InitializeTexture()
        {
            cymaticsTexture = new Texture2D(textureSize, textureSize);
            pixels = new Color[textureSize * textureSize];
            bubbleMaterial.SetTexture("_CymaticsTex", cymaticsTexture);
        }

        private void UpdateCymaticsPattern()
        {
            // Get frequency data using the interface
            float[] spectrumData = audioRenderer.GetFrequencyData(64);
            if (spectrumData == null || spectrumData.Length == 0) return;

            // Get overall amplitude for pattern intensity
            float amplitude = audioRenderer.GetAmplitude();
            
            // Simplified Chladni plate simulation
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float u = (float)x / textureSize;
                    float v = (float)y / textureSize;
                    float value = 0;

                    for (int i = 1; i < spectrumData.Length; i++)
                    {
                        float freq = (float)i * patternScale;
                        value += Mathf.Cos(freq * u) * Mathf.Sin(freq * v * patternComplexity) * spectrumData[i];
                    }

                    // Apply amplitude scaling for dynamic intensity
                    value *= amplitude;
                    
                    pixels[y * textureSize + x] = new Color(value, value, value, 1);
                }
            }

            cymaticsTexture.SetPixels(pixels);
            cymaticsTexture.Apply();
        }
    }
}
