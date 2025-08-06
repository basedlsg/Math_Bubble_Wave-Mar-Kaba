using UnityEngine;

namespace XRBubbleLibrary.Physics
{
    /// <summary>
    /// Configuration helper for breathing particle effects
    /// Cloned from Unity particle system samples, modified for bubble breathing visualization
    /// </summary>
    [CreateAssetMenu(fileName = "BreathingParticleConfig", menuName = "XR Bubble Library/Breathing Particle Config")]
    public class BreathingParticleConfig : ScriptableObject
    {
        [Header("Particle Appearance")]
        public Material particleMaterial;
        public Color startColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        public Color endColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        
        [Header("Emission Settings")]
        [Range(1.0f, 50.0f)]
        public float baseEmissionRate = 10.0f;
        
        [Range(0.0f, 2.0f)]
        public float breathingEmissionMultiplier = 0.5f; // How much breathing affects emission
        
        [Header("Particle Behavior")]
        [Range(0.5f, 5.0f)]
        public float particleLifetime = 2.0f;
        
        [Range(0.01f, 0.5f)]
        public float startSpeed = 0.1f;
        
        [Range(0.005f, 0.1f)]
        public float startSize = 0.02f;
        
        [Range(1, 100)]
        public int maxParticles = 50;
        
        [Header("Movement Patterns")]
        public bool enableRadialMovement = true;
        public float radialSpeed = 0.05f;
        
        public bool enableUpwardDrift = true;
        public float upwardSpeed = 0.02f;
        
        public bool enableOrbitalMotion = false;
        public float orbitalSpeed = 0.1f;
        
        [Header("Breathing Synchronization")]
        public bool syncWithBreathing = true;
        public float breathingSyncStrength = 0.3f;
        
        [Header("Neon-Pastel Colors")]
        public Gradient neonColorGradient;
        public bool useGradientOverLifetime = true;
        
        /// <summary>
        /// Apply this configuration to a particle system
        /// </summary>
        public void ApplyToParticleSystem(ParticleSystem particles)
        {
            if (particles == null) return;
            
            // Main module configuration
            var main = particles.main;
            main.startLifetime = particleLifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = startColor;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Emission module
            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = baseEmissionRate;
            
            // Shape module for bubble-like emission
            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f; // Will be adjusted by EnhancedBubblePhysics
            
            // Velocity over lifetime for movement patterns
            ConfigureVelocityModule(particles);
            
            // Color over lifetime
            ConfigureColorModule(particles);
            
            // Size over lifetime
            ConfigureSizeModule(particles);
            
            // Renderer settings
            ConfigureRenderer(particles);
        }
        
        /// <summary>
        /// Configure velocity module for particle movement
        /// </summary>
        void ConfigureVelocityModule(ParticleSystem particles)
        {
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = enableRadialMovement || enableUpwardDrift || enableOrbitalMotion;
            velocity.space = ParticleSystemSimulationSpace.Local;
            
            if (enableRadialMovement)
            {
                velocity.radial = new ParticleSystem.MinMaxCurve(radialSpeed);
            }
            
            if (enableUpwardDrift)
            {
                velocity.y = new ParticleSystem.MinMaxCurve(upwardSpeed);
            }
            
            if (enableOrbitalMotion)
            {
                velocity.orbitalX = new ParticleSystem.MinMaxCurve(orbitalSpeed);
                velocity.orbitalY = new ParticleSystem.MinMaxCurve(orbitalSpeed * 0.7f);
                velocity.orbitalZ = new ParticleSystem.MinMaxCurve(orbitalSpeed * 0.5f);
            }
        }
        
        /// <summary>
        /// Configure color module for neon-pastel effects
        /// </summary>
        void ConfigureColorModule(ParticleSystem particles)
        {
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            
            if (useGradientOverLifetime && neonColorGradient != null)
            {
                colorOverLifetime.color = neonColorGradient;
            }
            else
            {
                // Create simple fade gradient
                Gradient fadeGradient = new Gradient();
                fadeGradient.SetKeys(
                    new GradientColorKey[] { 
                        new GradientColorKey(startColor, 0.0f), 
                        new GradientColorKey(endColor, 1.0f) 
                    },
                    new GradientAlphaKey[] { 
                        new GradientAlphaKey(startColor.a, 0.0f), 
                        new GradientAlphaKey(endColor.a, 1.0f) 
                    }
                );
                colorOverLifetime.color = fadeGradient;
            }
        }
        
        /// <summary>
        /// Configure size module for particle scaling
        /// </summary>
        void ConfigureSizeModule(ParticleSystem particles)
        {
            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            
            // Create size curve that starts full and fades to zero
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0.0f, 1.0f);
            sizeCurve.AddKey(0.8f, 0.8f);
            sizeCurve.AddKey(1.0f, 0.0f);
            
            // Smooth the curve
            for (int i = 0; i < sizeCurve.keys.Length; i++)
            {
                sizeCurve.SmoothTangents(i, 0.3f);
            }
            
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);
        }
        
        /// <summary>
        /// Configure particle renderer
        /// </summary>
        void ConfigureRenderer(ParticleSystem particles)
        {
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                if (particleMaterial != null)
                {
                    renderer.material = particleMaterial;
                }
                
                renderer.sortMode = ParticleSystemSortMode.Distance;
                renderer.alignment = ParticleSystemRenderSpace.Facing;
                renderer.normalDirection = 1.0f;
            }
        }
        
        /// <summary>
        /// Create default neon-pastel gradient
        /// </summary>
        void OnValidate()
        {
            if (neonColorGradient == null)
            {
                neonColorGradient = new Gradient();
                
                // Neon-pastel color keys
                GradientColorKey[] colorKeys = new GradientColorKey[4];
                colorKeys[0] = new GradientColorKey(new Color(1.0f, 0.4f, 0.8f), 0.0f);    // Neon Pink
                colorKeys[1] = new GradientColorKey(new Color(0.8f, 0.4f, 1.0f), 0.33f);   // Neon Purple
                colorKeys[2] = new GradientColorKey(new Color(0.4f, 0.4f, 1.0f), 0.66f);   // Neon Blue
                colorKeys[3] = new GradientColorKey(new Color(0.4f, 1.0f, 1.0f), 1.0f);    // Neon Teal
                
                // Alpha keys for fade effect
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
                alphaKeys[0] = new GradientAlphaKey(0.0f, 0.0f);   // Start transparent
                alphaKeys[1] = new GradientAlphaKey(0.5f, 0.3f);   // Peak opacity
                alphaKeys[2] = new GradientAlphaKey(0.0f, 1.0f);   // End transparent
                
                neonColorGradient.SetKeys(colorKeys, alphaKeys);
            }
        }
        
        /// <summary>
        /// Create a preset configuration for different bubble types
        /// </summary>
        public static BreathingParticleConfig CreatePreset(BubbleParticlePreset preset)
        {
            BreathingParticleConfig config = CreateInstance<BreathingParticleConfig>();
            
            switch (preset)
            {
                case BubbleParticlePreset.Gentle:
                    config.baseEmissionRate = 5.0f;
                    config.breathingEmissionMultiplier = 0.3f;
                    config.particleLifetime = 3.0f;
                    config.startSpeed = 0.05f;
                    config.radialSpeed = 0.02f;
                    config.upwardSpeed = 0.01f;
                    break;
                    
                case BubbleParticlePreset.Active:
                    config.baseEmissionRate = 15.0f;
                    config.breathingEmissionMultiplier = 0.7f;
                    config.particleLifetime = 2.0f;
                    config.startSpeed = 0.15f;
                    config.radialSpeed = 0.08f;
                    config.upwardSpeed = 0.03f;
                    break;
                    
                case BubbleParticlePreset.Energetic:
                    config.baseEmissionRate = 25.0f;
                    config.breathingEmissionMultiplier = 1.0f;
                    config.particleLifetime = 1.5f;
                    config.startSpeed = 0.2f;
                    config.radialSpeed = 0.1f;
                    config.upwardSpeed = 0.05f;
                    config.enableOrbitalMotion = true;
                    config.orbitalSpeed = 0.15f;
                    break;
            }
            
            return config;
        }
    }
    
    /// <summary>
    /// Preset types for different bubble particle behaviors
    /// </summary>
    public enum BubbleParticlePreset
    {
        Gentle,     // Calm, slow particles for relaxed bubbles
        Active,     // Medium activity for normal interaction
        Energetic   // High activity for excited/pressed bubbles
    }
}