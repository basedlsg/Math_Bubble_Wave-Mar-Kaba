using UnityEngine;
using Unity.Mathematics;

namespace XRBubbleLibrary
{
    /// <summary>
    /// Physics simulation for XR bubbles with breathing animation
    /// Cloned from Unity physics samples and modified for comfortable wave-based movement
    /// </summary>
    public class BubblePhysics : MonoBehaviour
    {
        [SerializeField] private BubbleConfiguration config;
        
        // Breathing animation state
        private float breathingPhase;
        private float breathingTime;
        private Vector3 basePosition;
        private Vector3 breathingOffset;
        
        // Spring physics state
        private float currentScale;
        private float targetScale;
        private float velocity;
        private const float mass = 1.0f;
        
        // Wave positioning state
        private float wavePhase;
        private Vector3 waveOffset;
        
        // Performance optimization
        private bool isVisible = true;
        private float lastUpdateTime;
        private const float updateInterval = 0.016f; // ~60 FPS updates
        
        void Start()
        {
            InitializeBubble();
        }
        
        void Update()
        {
            // Performance optimization - only update if visible and enough time passed
            if (!isVisible || Time.time - lastUpdateTime < updateInterval)
                return;
                
            lastUpdateTime = Time.time;
            
            UpdateBreathingAnimation(Time.deltaTime);
            UpdateWavePositioning(Time.deltaTime);
            UpdateSpringPhysics(Time.deltaTime);
            ApplyTransformations();
        }
        
        /// <summary>
        /// Initialize bubble with configuration settings
        /// Based on cloned Unity object pooling patterns
        /// </summary>
        private void InitializeBubble()
        {
            basePosition = transform.position;
            currentScale = config.radius;
            targetScale = config.radius;
            breathingPhase = config.wavePhaseOffset * Mathf.Deg2Rad;
            wavePhase = config.wavePhaseOffset * Mathf.Deg2Rad;
        }
        
        /// <summary>
        /// Update breathing animation using comfortable sine wave oscillation
        /// Cloned from Unity animation samples, modified for 0.2-0.5 Hz frequency
        /// </summary>
        private void UpdateBreathingAnimation(float deltaTime)
        {
            breathingTime += deltaTime;
            
            // Comfortable breathing frequency (0.2-0.5 Hz)
            float breathingCycle = breathingTime * config.breathingFrequency * 2.0f * Mathf.PI;
            
            // Sine wave breathing with slight randomization for organic feel
            float breathingIntensity = Mathf.Sin(breathingCycle + breathingPhase);
            
            // Apply breathing offset with comfortable amplitude
            breathingOffset = Vector3.up * (breathingIntensity * config.breathingAmplitude);
        }
        
        /// <summary>
        /// Update wave-based positioning around user
        /// Based on mathematical wave functions for natural spatial arrangement
        /// </summary>
        private void UpdateWavePositioning(float deltaTime)
        {
            if (!config.useWavePositioning)
            {
                waveOffset = Vector3.zero;
                return;
            }
            
            wavePhase += deltaTime * 0.5f; // Slow wave movement
            
            // Create wave pattern around user using sine/cosine
            float x = Mathf.Sin(wavePhase) * config.waveRadius * 0.1f;
            float z = Mathf.Cos(wavePhase * 0.7f) * config.waveRadius * 0.1f;
            
            waveOffset = new Vector3(x, 0, z);
        }
        
        /// <summary>
        /// Update spring physics for interaction feedback
        /// Cloned from Unity spring physics samples
        /// </summary>
        private void UpdateSpringPhysics(float deltaTime)
        {
            // Spring-damper system for smooth scale transitions
            float displacement = currentScale - targetScale;
            float springForce = -config.springStiffness * displacement;
            float dampingForce = -config.springDamping * velocity;
            float acceleration = (springForce + dampingForce) / mass;
            
            velocity += acceleration * deltaTime;
            currentScale += velocity * deltaTime;
            
            // Clamp to reasonable values
            currentScale = Mathf.Clamp(currentScale, config.radius * 0.5f, config.radius * 1.5f);
        }
        
        /// <summary>
        /// Apply all transformations to the bubble
        /// </summary>
        private void ApplyTransformations()
        {
            // Combine base position with breathing and wave offsets
            Vector3 finalPosition = basePosition + breathingOffset + waveOffset;
            transform.position = finalPosition;
            
            // Apply scale with breathing effect
            float finalScale = currentScale + (breathingOffset.y * 0.5f);
            transform.localScale = Vector3.one * finalScale;
        }
        
        /// <summary>
        /// Handle bubble interaction (called by XR Interaction Toolkit)
        /// </summary>
        public void OnBubblePressed()
        {
            targetScale = config.radius * 0.85f; // Compress on press
        }
        
        /// <summary>
        /// Handle bubble release (called by XR Interaction Toolkit)
        /// </summary>
        public void OnBubbleReleased()
        {
            targetScale = config.radius; // Return to normal size
        }
        
        /// <summary>
        /// Update configuration at runtime
        /// </summary>
        public void UpdateConfiguration(BubbleConfiguration newConfig)
        {
            config = newConfig;
            targetScale = config.radius;
        }
        
        /// <summary>
        /// Optimize performance based on distance from user
        /// Based on cloned Unity LOD samples
        /// </summary>
        public void SetVisibility(bool visible)
        {
            isVisible = visible;
            if (!visible)
            {
                // Pause updates when not visible for performance
                enabled = false;
            }
            else
            {
                enabled = true;
            }
        }
    }
}