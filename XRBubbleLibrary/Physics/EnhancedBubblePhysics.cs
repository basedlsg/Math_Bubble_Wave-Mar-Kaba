using UnityEngine;
using Unity.Mathematics;
using XRBubbleLibrary.Mathematics;

namespace XRBubbleLibrary.Physics
{
    /// <summary>
    /// Enhanced bubble physics system with particle integration and advanced breathing
    /// Cloned from Unity particle system samples and physics examples, modified for comfortable breathing
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class EnhancedBubblePhysics : MonoBehaviour
    {
        [Header("Breathing Physics")]
        [Range(0.2f, 0.5f)]
        public float breathingFrequency = 0.3f; // Hz - comfortable breathing rate
        
        [Range(0.01f, 0.1f)]
        public float breathingAmplitude = 0.03f; // Breathing movement amplitude
        
        [Range(0.0f, 6.28f)]
        public float phaseOffset = 0.0f; // Phase offset for variation
        
        [Header("Spring Physics")]
        [Range(0.5f, 5.0f)]
        public float springStiffness = 2.0f; // N/m
        
        [Range(0.1f, 2.0f)]
        public float springDamping = 0.8f; // N·s/m
        
        [Range(0.01f, 0.2f)]
        public float bubbleMass = 0.05f; // kg
        
        [Header("Particle Effects")]
        public bool enableBreathingParticles = true;
        public float particleEmissionRate = 10.0f;
        public Color particleColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        
        [Header("Wave Interaction")]
        public bool enableWaveInteraction = true;
        public float waveInfluenceRadius = 1.0f;
        public LayerMask bubbleLayerMask = -1;
        
        // Internal physics state
        private Vector3 basePosition;
        private Vector3 currentPosition;
        private Vector3 targetPosition;
        private Vector3 velocity;
        private float currentScale;
        private float targetScale;
        private float scaleVelocity;
        
        // Breathing animation state
        private float breathingTime;
        private Vector3 breathingOffset;
        private float breathingIntensity;
        
        // Particle system integration
        private ParticleSystem breathingParticles;
        private ParticleSystem.EmissionModule emissionModule;
        private ParticleSystem.MainModule mainModule;
        private ParticleSystem.ShapeModule shapeModule;
        
        // Wave interaction
        private List<EnhancedBubblePhysics> nearbyBubbles = new List<EnhancedBubblePhysics>();
        private float lastWaveUpdate = 0.0f;
        private const float waveUpdateInterval = 0.1f; // Update wave interactions 10 times per second
        
        // Performance optimization
        private bool isVisible = true;
        private float lastUpdateTime = 0.0f;
        private const float updateInterval = 0.016f; // ~60 FPS updates
        
        void Start()
        {
            InitializePhysics();
            SetupParticleSystem();
        }
        
        void Update()
        {
            // Performance optimization - limit update frequency
            if (!isVisible || Time.time - lastUpdateTime < updateInterval)
                return;
                
            lastUpdateTime = Time.time;
            
            UpdateBreathingPhysics(Time.deltaTime);
            UpdateSpringPhysics(Time.deltaTime);
            UpdateWaveInteractions();
            UpdateParticleEffects();
            ApplyPhysicsTransformations();
        }
        
        /// <summary>
        /// Initialize physics system
        /// Based on cloned Unity physics samples
        /// </summary>
        void InitializePhysics()
        {
            basePosition = transform.position;
            currentPosition = basePosition;
            targetPosition = basePosition;
            currentScale = transform.localScale.x;
            targetScale = currentScale;
            
            // Initialize breathing with random phase for variety
            breathingTime = phaseOffset;
            breathingIntensity = 1.0f;
        }
        
        /// <summary>
        /// Set up particle system for breathing effects
        /// Cloned from Unity particle system samples, modified for breathing visualization
        /// </summary>
        void SetupParticleSystem()
        {
            breathingParticles = GetComponent<ParticleSystem>();
            
            if (breathingParticles == null)
            {
                Debug.LogWarning("No ParticleSystem found on " + gameObject.name);
                return;
            }
            
            // Configure main module
            mainModule = breathingParticles.main;
            mainModule.startLifetime = 2.0f;
            mainModule.startSpeed = 0.1f;
            mainModule.startSize = 0.02f;
            mainModule.startColor = particleColor;
            mainModule.maxParticles = 50;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Configure emission module
            emissionModule = breathingParticles.emission;
            emissionModule.enabled = enableBreathingParticles;
            emissionModule.rateOverTime = particleEmissionRate;
            
            // Configure shape module for bubble-like emission
            shapeModule = breathingParticles.shape;
            shapeModule.enabled = true;
            shapeModule.shapeType = ParticleSystemShapeType.Sphere;
            shapeModule.radius = currentScale * 0.8f;
            
            // Configure velocity over lifetime for breathing motion
            var velocityModule = breathingParticles.velocityOverLifetime;
            velocityModule.enabled = true;
            velocityModule.space = ParticleSystemSimulationSpace.Local;
            
            // Configure size over lifetime for fade effect
            var sizeModule = breathingParticles.sizeOverLifetime;
            sizeModule.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0.0f, 1.0f);
            sizeCurve.AddKey(1.0f, 0.0f);
            sizeModule.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);
            
            // Configure color over lifetime for fade effect
            var colorModule = breathingParticles.colorOverLifetime;
            colorModule.enabled = true;
            Gradient colorGradient = new Gradient();
            colorGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(particleColor, 0.0f), new GradientColorKey(particleColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(particleColor.a, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorModule.color = colorGradient;
        }
        
        /// <summary>
        /// Update breathing physics using comfortable wave functions
        /// Based on mathematical breathing patterns (0.2-0.5 Hz)
        /// </summary>
        void UpdateBreathingPhysics(float deltaTime)
        {
            breathingTime += deltaTime;
            
            // Primary breathing wave (comfortable frequency)
            float primaryBreathing = Mathf.Sin(breathingTime * breathingFrequency * 2.0f * Mathf.PI + phaseOffset);
            
            // Secondary breathing wave (slightly different frequency for organic feel)
            float secondaryBreathing = Mathf.Cos(breathingTime * breathingFrequency * 1.3f * 2.0f * Mathf.PI + phaseOffset * 0.7f);
            
            // Combine waves for natural breathing pattern
            breathingIntensity = (primaryBreathing * 0.7f + secondaryBreathing * 0.3f);
            
            // Apply breathing to position (primarily Y axis)
            breathingOffset = new Vector3(
                breathingIntensity * breathingAmplitude * 0.3f, // Subtle X movement
                breathingIntensity * breathingAmplitude,        // Primary Y movement
                breathingIntensity * breathingAmplitude * 0.2f  // Subtle Z movement
            );
            
            // Apply breathing to scale (subtle size breathing)
            float scaleBreathing = 1.0f + (breathingIntensity * breathingAmplitude * 0.5f);
            targetScale = transform.localScale.x * scaleBreathing;
        }
        
        /// <summary>
        /// Update spring physics for interaction responses
        /// Cloned from Unity spring physics examples
        /// </summary>
        void UpdateSpringPhysics(float deltaTime)
        {
            // Position spring physics
            Vector3 displacement = currentPosition - targetPosition;
            Vector3 springForce = -displacement * springStiffness;
            Vector3 dampingForce = -velocity * springDamping;
            Vector3 acceleration = (springForce + dampingForce) / bubbleMass;
            
            velocity += acceleration * deltaTime;
            currentPosition += velocity * deltaTime;
            
            // Scale spring physics
            float scaleDisplacement = currentScale - targetScale;
            float scaleSpringForce = -scaleDisplacement * springStiffness;
            float scaleDampingForce = -scaleVelocity * springDamping;
            float scaleAcceleration = (scaleSpringForce + scaleDampingForce) / bubbleMass;
            
            scaleVelocity += scaleAcceleration * deltaTime;
            currentScale += scaleVelocity * deltaTime;
            
            // Clamp values to reasonable ranges
            currentScale = Mathf.Clamp(currentScale, 0.5f, 2.0f);
        }
        
        /// <summary>
        /// Update wave interactions with nearby bubbles
        /// Creates natural wave interference patterns
        /// </summary>
        void UpdateWaveInteractions()
        {
            if (!enableWaveInteraction || Time.time - lastWaveUpdate < waveUpdateInterval)
                return;
                
            lastWaveUpdate = Time.time;
            
            // Find nearby bubbles
            FindNearbyBubbles();
            
            // Calculate wave interference
            if (nearbyBubbles.Count > 0)
            {
                List<WaveSource> waveSources = new List<WaveSource>();
                
                foreach (var bubble in nearbyBubbles)
                {
                    if (bubble != null && bubble != this)
                    {
                        WaveSource source = WaveSource.Create(
                            bubble.transform.position,
                            bubble.breathingFrequency,
                            bubble.breathingAmplitude
                        );
                        waveSources.Add(source);
                    }
                }
                
                // Apply wave interference to breathing
                float interference = WavePatternGenerator.CalculateWaveInterference(transform.position, waveSources);
                breathingIntensity += interference * 0.1f; // Subtle influence
                breathingIntensity = Mathf.Clamp(breathingIntensity, -1.0f, 1.0f);
            }
        }
        
        /// <summary>
        /// Find nearby bubbles for wave interaction
        /// </summary>
        void FindNearbyBubbles()
        {
            nearbyBubbles.Clear();
            
            Collider[] nearbyColliders = UnityEngine.Physics.OverlapSphere(
                transform.position, 
                waveInfluenceRadius, 
                bubbleLayerMask
            );
            
            foreach (var collider in nearbyColliders)
            {
                EnhancedBubblePhysics bubble = collider.GetComponent<EnhancedBubblePhysics>();
                if (bubble != null && bubble != this)
                {
                    nearbyBubbles.Add(bubble);
                }
            }
        }
        
        /// <summary>
        /// Update particle effects based on breathing
        /// </summary>
        void UpdateParticleEffects()
        {
            if (breathingParticles == null || !enableBreathingParticles) return;
            
            // Adjust emission rate based on breathing intensity
            float emissionRate = particleEmissionRate * (1.0f + breathingIntensity * 0.5f);
            emissionModule.rateOverTime = Mathf.Max(0, emissionRate);
            
            // Adjust particle color based on breathing
            Color currentColor = particleColor;
            currentColor.a = particleColor.a * (1.0f + breathingIntensity * 0.3f);
            mainModule.startColor = currentColor;
            
            // Adjust shape radius based on current scale
            shapeModule.radius = currentScale * 0.8f;
        }
        
        /// <summary>
        /// Apply all physics transformations to the bubble
        /// </summary>
        void ApplyPhysicsTransformations()
        {
            // Apply position with breathing offset
            Vector3 finalPosition = basePosition + currentPosition - basePosition + breathingOffset;
            transform.position = finalPosition;
            
            // Apply scale
            transform.localScale = Vector3.one * currentScale;
        }
        
        /// <summary>
        /// Handle bubble interaction (called by interaction system)
        /// </summary>
        public void OnBubblePressed(Vector3 pressPosition, float intensity = 1.0f)
        {
            // Calculate press direction
            Vector3 pressDirection = (transform.position - pressPosition).normalized;
            
            // Apply impulse
            Vector3 impulse = pressDirection * intensity * 0.1f;
            velocity += impulse;
            
            // Compress bubble
            targetScale = transform.localScale.x * 0.85f;
            
            // Increase breathing intensity temporarily
            breathingIntensity += intensity * 0.3f;
            breathingIntensity = Mathf.Clamp(breathingIntensity, -1.0f, 1.0f);
        }
        
        /// <summary>
        /// Handle bubble release
        /// </summary>
        public void OnBubbleReleased()
        {
            // Return to normal scale
            targetScale = transform.localScale.x / 0.85f;
            
            // Gradually return breathing to normal
            StartCoroutine(ReturnBreathingToNormal());
        }
        
        /// <summary>
        /// Gradually return breathing intensity to normal
        /// </summary>
        System.Collections.IEnumerator ReturnBreathingToNormal()
        {
            float startIntensity = breathingIntensity;
            float elapsed = 0.0f;
            float duration = 2.0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                breathingIntensity = Mathf.Lerp(startIntensity, 1.0f, t);
                yield return null;
            }
            
            breathingIntensity = 1.0f;
        }
        
        /// <summary>
        /// Set visibility for performance optimization
        /// </summary>
        public void SetVisibility(bool visible)
        {
            isVisible = visible;
            
            if (breathingParticles != null)
            {
                var emission = breathingParticles.emission;
                emission.enabled = visible && enableBreathingParticles;
            }
        }
        
        /// <summary>
        /// Update physics configuration at runtime
        /// </summary>
        public void UpdateConfiguration(float frequency, float amplitude, float stiffness, float damping)
        {
            breathingFrequency = Mathf.Clamp(frequency, 0.2f, 0.5f);
            breathingAmplitude = Mathf.Clamp(amplitude, 0.01f, 0.1f);
            springStiffness = Mathf.Clamp(stiffness, 0.5f, 5.0f);
            springDamping = Mathf.Clamp(damping, 0.1f, 2.0f);
        }
        
        /// <summary>
        /// Get current breathing phase for synchronization
        /// </summary>
        public float GetBreathingPhase()
        {
            return (breathingTime * breathingFrequency * 2.0f * Mathf.PI + phaseOffset) % (2.0f * Mathf.PI);
        }
        
        /// <summary>
        /// Synchronize breathing with another bubble
        /// </summary>
        public void SynchronizeBreathingWith(EnhancedBubblePhysics otherBubble, float syncStrength = 0.1f)
        {
            if (otherBubble == null) return;
            
            float otherPhase = otherBubble.GetBreathingPhase();
            float currentPhase = GetBreathingPhase();
            float phaseDifference = Mathf.DeltaAngle(currentPhase * Mathf.Rad2Deg, otherPhase * Mathf.Rad2Deg) * Mathf.Deg2Rad;
            
            // Gradually adjust phase to synchronize
            phaseOffset += phaseDifference * syncStrength * Time.deltaTime;
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw wave influence radius
            if (enableWaveInteraction)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, waveInfluenceRadius);
            }
            
            // Draw breathing offset
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + breathingOffset * 10.0f);
            
            // Draw velocity
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + velocity);
        }
    }
}