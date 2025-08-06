using UnityEngine;
using Unity.Mathematics;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Settings for breathing animation system
    /// Controls how UI elements breathe using mathematical wave formulas
    /// </summary>
    [System.Serializable]
    public class BreathingSettings
    {
        [Header("Global Breathing")]
        [Range(0.1f, 3f)]
        public float globalTimeScale = 1f;
        
        [Header("Wave Frequencies (Hz)")]
        [Range(0.1f, 2f)]
        public float primaryFrequency = 0.8f;    // Main breathing rhythm
        
        [Range(0.1f, 3f)]
        public float secondaryFrequency = 1.6f;  // Harmonic complexity
        
        [Range(0.1f, 5f)]
        public float tertiaryFrequency = 3.2f;   // Fine detail
        
        [Header("Wave Amplitudes")]
        [Range(0f, 0.5f)]
        public float primaryAmplitude = 0.15f;
        
        [Range(0f, 0.3f)]
        public float secondaryAmplitude = 0.08f;
        
        [Range(0f, 0.2f)]
        public float tertiaryAmplitude = 0.04f;
        
        [Header("Breathing Curve")]
        [Range(0.5f, 3f)]
        public float breathingCurve = 1.5f; // Curve for natural breathing feel
        
        [Header("Scale Limits")]
        [Range(0.5f, 1f)]
        public float minScale = 0.8f;
        
        [Range(1f, 2f)]
        public float maxScale = 1.3f;
        
        [Header("Opacity Limits")]
        [Range(0.1f, 1f)]
        public float minOpacity = 0.6f;
        
        [Range(0.5f, 1f)]
        public float maxOpacity = 1f;
        
        [Header("Default Intensities")]
        [Range(0f, 2f)]
        public float defaultScaleIntensity = 1f;
        
        [Range(0f, 2f)]
        public float defaultOpacityIntensity = 0.5f;
        
        /// <summary>
        /// Default breathing settings optimized for VR comfort
        /// </summary>
        public static BreathingSettings Default => new BreathingSettings();
        
        /// <summary>
        /// Subtle breathing for motion-sensitive users
        /// </summary>
        public static BreathingSettings Subtle => new BreathingSettings
        {
            globalTimeScale = 0.7f,
            primaryFrequency = 0.5f,
            secondaryFrequency = 1.0f,
            tertiaryFrequency = 2.0f,
            primaryAmplitude = 0.08f,
            secondaryAmplitude = 0.04f,
            tertiaryAmplitude = 0.02f,
            minScale = 0.9f,
            maxScale = 1.15f,
            defaultScaleIntensity = 0.6f,
            defaultOpacityIntensity = 0.3f
        };
        
        /// <summary>
        /// Dramatic breathing for immersive experience
        /// </summary>
        public static BreathingSettings Dramatic => new BreathingSettings
        {
            globalTimeScale = 1.3f,
            primaryFrequency = 1.0f,
            secondaryFrequency = 2.0f,
            tertiaryFrequency = 4.0f,
            primaryAmplitude = 0.25f,
            secondaryAmplitude = 0.15f,
            tertiaryAmplitude = 0.08f,
            minScale = 0.7f,
            maxScale = 1.5f,
            defaultScaleIntensity = 1.5f,
            defaultOpacityIntensity = 0.8f
        };
        
        /// <summary>
        /// Calm breathing for relaxing experience
        /// </summary>
        public static BreathingSettings Calm => new BreathingSettings
        {
            globalTimeScale = 0.5f,
            primaryFrequency = 0.3f,
            secondaryFrequency = 0.6f,
            tertiaryFrequency = 1.2f,
            primaryAmplitude = 0.1f,
            secondaryAmplitude = 0.05f,
            tertiaryAmplitude = 0.025f,
            breathingCurve = 2f,
            minScale = 0.95f,
            maxScale = 1.1f,
            defaultScaleIntensity = 0.8f,
            defaultOpacityIntensity = 0.4f
        };
        
        /// <summary>
        /// Validate settings and clamp to safe ranges
        /// </summary>
        public void ValidateSettings()
        {
            globalTimeScale = math.clamp(globalTimeScale, 0.1f, 3f);
            primaryFrequency = math.clamp(primaryFrequency, 0.1f, 2f);
            secondaryFrequency = math.clamp(secondaryFrequency, 0.1f, 3f);
            tertiaryFrequency = math.clamp(tertiaryFrequency, 0.1f, 5f);
            
            primaryAmplitude = math.clamp(primaryAmplitude, 0f, 0.5f);
            secondaryAmplitude = math.clamp(secondaryAmplitude, 0f, 0.3f);
            tertiaryAmplitude = math.clamp(tertiaryAmplitude, 0f, 0.2f);
            
            breathingCurve = math.clamp(breathingCurve, 0.5f, 3f);
            
            minScale = math.clamp(minScale, 0.5f, 1f);
            maxScale = math.clamp(maxScale, 1f, 2f);
            minOpacity = math.clamp(minOpacity, 0.1f, 1f);
            maxOpacity = math.clamp(maxOpacity, 0.5f, 1f);
            
            // Ensure min < max
            if (minScale >= maxScale) maxScale = minScale + 0.1f;
            if (minOpacity >= maxOpacity) maxOpacity = minOpacity + 0.1f;
        }
    }
}