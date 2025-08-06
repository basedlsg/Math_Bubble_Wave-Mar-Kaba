using UnityEngine;
using Unity.Mathematics;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Configuration settings for wave matrix calculations
    /// Defines all parameters for wave-based positioning and breathing animations
    /// </summary>
    [System.Serializable]
    public class WaveMatrixSettings
    {
        [Header("Grid Layout")]
        public int gridWidth = 8;
        public float spacing = 1.5f;
        public float timeScale = 1.0f;
        
        [Header("AI Distance Mapping")]
        public float aiDistanceScale = 2.0f; // How much AI confidence affects Z distance
        
        [Header("Wave Components")]
        public WaveComponent primaryWave = new WaveComponent
        {
            amplitude = 0.5f,
            frequency = 0.5f,
            speed = 1.0f
        };
        
        public WaveComponent secondaryWave = new WaveComponent
        {
            amplitude = 0.3f,
            frequency = 0.8f,
            speed = 1.5f
        };
        
        public WaveComponent tertiaryWave = new WaveComponent
        {
            amplitude = 0.2f,
            frequency = 1.2f,
            speed = 2.0f
        };
        
        [Header("Wave Interference")]
        public bool enableInterference = true;
        public float interferenceFreq = 0.3f;
        public float interferenceAmplitude = 0.15f;
        
        /// <summary>
        /// Default wave matrix settings optimized for VR word interface
        /// </summary>
        public static WaveMatrixSettings Default => new WaveMatrixSettings();
        
        /// <summary>
        /// Calm wave settings for reduced motion sensitivity
        /// </summary>
        public static WaveMatrixSettings Calm => new WaveMatrixSettings
        {
            gridWidth = 8,
            spacing = 1.5f,
            timeScale = 0.5f,
            primaryWave = new WaveComponent { amplitude = 0.2f, frequency = 0.3f, speed = 0.5f },
            secondaryWave = new WaveComponent { amplitude = 0.1f, frequency = 0.5f, speed = 0.8f },
            tertiaryWave = new WaveComponent { amplitude = 0.05f, frequency = 0.8f, speed = 1.0f },
            enableInterference = false
        };
        
        /// <summary>
        /// Dynamic wave settings for more active visual experience
        /// </summary>
        public static WaveMatrixSettings Dynamic => new WaveMatrixSettings
        {
            gridWidth = 10,
            spacing = 1.2f,
            timeScale = 1.5f,
            primaryWave = new WaveComponent { amplitude = 0.8f, frequency = 0.7f, speed = 1.5f },
            secondaryWave = new WaveComponent { amplitude = 0.5f, frequency = 1.0f, speed = 2.0f },
            tertiaryWave = new WaveComponent { amplitude = 0.3f, frequency = 1.5f, speed = 2.5f },
            enableInterference = true,
            interferenceFreq = 0.5f,
            interferenceAmplitude = 0.25f
        };
    }
    
    /// <summary>
    /// Individual wave component parameters
    /// </summary>
    [System.Serializable]
    public class WaveComponent
    {
        [Range(0f, 2f)]
        public float amplitude = 0.5f;
        
        [Range(0.1f, 3f)]
        public float frequency = 1.0f;
        
        [Range(0.1f, 5f)]
        public float speed = 1.0f;
        
        /// <summary>
        /// Calculate wave value at given position and time
        /// </summary>
        public float CalculateWave(float position, float time)
        {
            return math.sin(position * frequency + time * speed) * amplitude;
        }
    }
}