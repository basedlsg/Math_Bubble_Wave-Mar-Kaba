using UnityEngine;
using System;

namespace XRBubbleLibrary
{
    /// <summary>
    /// Configuration data for XR bubble behavior and appearance
    /// Based on cloned Unity UI Toolkit patterns, modified for 3D spatial bubbles
    /// </summary>
    [Serializable]
    public struct BubbleConfiguration
    {
        [Header("Visual Properties")]
        [Range(0.05f, 0.5f)]
        public float radius;                    // Physical size in meters
        
        [ColorUsage(true, true)]
        public Color baseColor;                 // HDR color with alpha for neon-pastel effect
        
        [Range(0.0f, 1.0f)]
        public float glowIntensity;            // Underlight glow strength
        
        [Header("Physics Properties")]
        [Range(0.1f, 2.0f)]
        public float springStiffness;          // N/m - spring constant for interaction
        
        [Range(0.1f, 1.0f)]
        public float springDamping;            // N·s/m - damping coefficient
        
        [Range(0.01f, 0.1f)]
        public float mass;                     // kg - bubble mass
        
        [Header("Breathing Animation")]
        [Range(0.2f, 0.5f)]
        public float breathingFrequency;       // Hz - comfortable oscillation rate
        
        [Range(0.01f, 0.05f)]
        public float breathingAmplitude;       // Breathing movement amplitude
        
        [Header("Wave Positioning")]
        public bool useWavePositioning;        // Enable mathematical wave-based positioning
        
        [Range(0.5f, 3.0f)]
        public float waveRadius;               // Distance from user center
        
        [Range(0.0f, 360.0f)]
        public float wavePhaseOffset;          // Phase offset in wave pattern
        
        [Header("Interaction Settings")]
        public bool enableHandTracking;        // Use hand tracking vs controller
        
        public bool enableHapticFeedback;      // Tactile response on interaction
        
        public bool enableAudioFeedback;       // Glass clinking sounds
        
        [Header("Performance")]
        public bool useCloudPhysics;           // Offload complex physics to cloud
        
        public int lodLevel;                   // Level of detail (0=highest, 2=lowest)
        
        /// <summary>
        /// Default configuration for neon-pastel glass bubbles
        /// </summary>
        public static BubbleConfiguration Default => new BubbleConfiguration
        {
            radius = 0.15f,
            baseColor = new Color(0.8f, 0.4f, 1.0f, 0.7f), // Neon purple with transparency
            glowIntensity = 0.6f,
            springStiffness = 1.2f,
            springDamping = 0.4f,
            mass = 0.05f,
            breathingFrequency = 0.3f,
            breathingAmplitude = 0.02f,
            useWavePositioning = true,
            waveRadius = 1.5f,
            wavePhaseOffset = 0.0f,
            enableHandTracking = true,
            enableHapticFeedback = true,
            enableAudioFeedback = true,
            useCloudPhysics = false,
            lodLevel = 0
        };
        
        /// <summary>
        /// Validate configuration values for Quest 3 performance
        /// </summary>
        public bool IsValid()
        {
            return radius > 0.0f && 
                   radius < 1.0f && 
                   breathingFrequency >= 0.2f && 
                   breathingFrequency <= 0.5f &&
                   mass > 0.0f;
        }
    }
}