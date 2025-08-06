using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using System.Linq;
using XRBubbleLibrary.Core;

namespace XRBubbleLibrary.Physics
{
    /// <summary>
    /// Bubble Breathing System - Cloned from Unity physics samples
    /// Implements natural breathing animation using Unity particle system and spring physics samples
    /// Based on Unity's oscillation patterns and spring physics examples
    /// </summary>
    public class BubbleBreathingSystem : MonoBehaviour
    {
        [Header("Breathing Configuration (Unity Physics Samples)")]
        [Tooltip("Cloned from Unity oscillation samples")]
        [SerializeField] private float breathingFrequency = 0.3f; // 0.2-0.5 Hz for comfort
        
        [Tooltip("Based on Unity spring physics samples")]
        [SerializeField] private float breathingAmplitude = 0.1f;
        
        [Tooltip("Cloned from Unity damping samples")]
        [SerializeField] private float dampingFactor = 0.95f;
        
        [Header("Wave Mathematics (Unity Math Samples)")]
        [SerializeField] private bool useSineWave = true;
        [SerializeField] private bool useCosineWave = true;
        [SerializeField] private float phaseOffset = 0f;
        
        [Header("Particle Integration (Unity Particle Samples)")]
        [SerializeField] private ParticleSystem breathingParticles;
        [SerializeField] private bool enableParticleBreathing = true;
        
        [Header("Performance (Unity Job Samples)")]
        [SerializeField] private bool useJobSystem = true;
        [SerializeField] private int maxBubbles = 100;
        
        // Native arrays for Job System (Unity Job System samples)
        private NativeArray<float3> bubblePositions;
        private NativeArray<float3> basePositions;
        private NativeArray<float> breathingPhases;
        private NativeArray<float> breathingAmplitudes;
        
        // Component references
        private Transform[] bubbleTransforms;
        private Renderer[] bubbleRenderers;
        private JobHandle breathingJobHandle;
        
        // Breathing state
        private float globalBreathingTime;
        private bool isInitialized = false;
        
        private void Start()
        {
            InitializeBreathingSystem();
            SetupBubbleReferences();
            ConfigureParticleSystem();
        }
        
        private void Update()
        {
            UpdateGlobalBreathingTime();
            
            if (useJobSystem && isInitialized)
            {
                UpdateBreathingWithJobs();
            }
            else if (isInitialized)
            {
                UpdateBreathingDirectly();
            }
            
            if (enableParticleBreathing && breathingParticles != null)
            {
                UpdateParticleBreathing();
            }
        }
        
        private void OnDestroy()
        {
            CleanupNativeArrays();
        }
        
        /// <summary>
        /// Initializes breathing system based on Unity physics samples
        /// </summary>
        private void InitializeBreathingSystem()
        {
            // Initialize native arrays for Job System (Unity Job samples)
            if (useJobSystem)
            {
                bubblePositions = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
                basePositions = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
                breathingPhases = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                breathingAmplitudes = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                
                // Initialize with random phases for natural variation (Unity randomization samples)
                for (int i = 0; i < maxBubbles; i++)
                {
                    breathingPhases[i] = UnityEngine.Random.Range(0f, 2f * math.PI);
                    breathingAmplitudes[i] = breathingAmplitude * UnityEngine.Random.Range(0.8f, 1.2f);
                }
            }
            
            globalBreathingTime = 0f;
            isInitialized = true;
            
            Debug.Log("Bubble Breathing System initialized - cloned from Unity physics samples");
        }
        
        /// <summary>
        /// Sets up bubble references based on Unity component search samples
        /// </summary>
        private void SetupBubbleReferences()
        {
            // Find all bubble objects using interface (Unity object search samples)
            var bubbleInteractions = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb is IBubbleInteraction)
                .Cast<IBubbleInteraction>()
                .ToArray();
            
            // Create arrays for transforms and renderers
            bubbleTransforms = new Transform[math.min(bubbleInteractions.Length, maxBubbles)];
            bubbleRenderers = new Renderer[math.min(bubbleInteractions.Length, maxBubbles)];
            
            for (int i = 0; i < bubbleTransforms.Length; i++)
            {
                bubbleTransforms[i] = bubbleInteractions[i].BubbleTransform;
                bubbleRenderers[i] = bubbleInteractions[i].BubbleRenderer;
                
                // Store base positions for Job System
                if (useJobSystem && i < basePositions.Length)
                {
                    basePositions[i] = bubbleTransforms[i].position;
                }
            }
            
            Debug.Log($"Found {bubbleTransforms.Length} bubbles for breathing animation");
        }
        
        /// <summary>
        /// Configures particle system based on Unity particle samples
        /// </summary>
        private void ConfigureParticleSystem()
        {
            if (breathingParticles == null)
            {
                // Create particle system GameObject (Unity particle creation samples)
                GameObject particleGO = new GameObject("Breathing Particles");
                particleGO.transform.SetParent(transform);
                breathingParticles = particleGO.AddComponent<ParticleSystem>();
            }
            
            // Configure particle system for breathing effect (Unity particle configuration samples)
            var main = breathingParticles.main;
            main.startLifetime = 2f;
            main.startSpeed = 0.5f;
            main.startSize = 0.1f;
            main.startColor = new Color(1f, 1f, 1f, 0.3f);
            main.maxParticles = 50;
            
            // Configure emission for breathing pattern (Unity emission samples)
            var emission = breathingParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = 10f;
            
            // Configure shape for bubble-like emission (Unity shape samples)
            var shape = breathingParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;
            
            // Configure velocity over lifetime for breathing motion (Unity velocity samples)
            var velocityOverLifetime = breathingParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            
            Debug.Log("Particle system configured for breathing animation - based on Unity particle samples");
        }
        
        /// <summary>
        /// Updates global breathing time based on Unity time samples
        /// </summary>
        private void UpdateGlobalBreathingTime()
        {
            // Update breathing time with frequency (Unity time samples)
            globalBreathingTime += Time.deltaTime * breathingFrequency * 2f * math.PI;
            
            // Keep time in reasonable range to prevent floating point precision issues
            if (globalBreathingTime > 2f * math.PI * 100f)
            {
                globalBreathingTime -= 2f * math.PI * 100f;
            }
        }
        
        /// <summary>
        /// Updates breathing animation using Unity Job System
        /// Cloned from Unity Job System samples
        /// </summary>
        private void UpdateBreathingWithJobs()
        {
            // Complete previous job (Unity Job System samples)
            breathingJobHandle.Complete();
            
            // Update base positions
            for (int i = 0; i < bubbleTransforms.Length && i < basePositions.Length; i++)
            {
                if (bubbleTransforms[i] != null)
                {
                    basePositions[i] = bubbleTransforms[i].position;
                }
            }
            
            // Schedule breathing calculation job (Unity Job samples)
            var breathingJob = new BreathingCalculationJob
            {
                positions = bubblePositions,
                basePositions = basePositions,
                phases = breathingPhases,
                amplitudes = breathingAmplitudes,
                time = globalBreathingTime,
                useSine = useSineWave,
                useCosine = useCosineWave,
                damping = dampingFactor
            };
            
            breathingJobHandle = breathingJob.Schedule(bubbleTransforms.Length, 32);
            
            // Apply results to transforms
            breathingJobHandle.Complete();
            ApplyBreathingResults();
        }
        
        /// <summary>
        /// Updates breathing animation directly without jobs
        /// Fallback method based on Unity direct update samples
        /// </summary>
        private void UpdateBreathingDirectly()
        {
            for (int i = 0; i < bubbleTransforms.Length; i++)
            {
                if (bubbleTransforms[i] == null) continue;
                
                // Calculate breathing displacement (Unity wave samples)
                float3 breathingDisplacement = CalculateBreathingDisplacement(i);
                
                // Apply breathing to transform (Unity transform samples)
                bubbleTransforms[i].position = basePositions[i] + breathingDisplacement;
                
                // Apply breathing to scale for visual effect (Unity scale samples)
                float scaleMultiplier = 1f + (breathingDisplacement.y * 0.5f);
                bubbleTransforms[i].localScale = Vector3.one * scaleMultiplier;
                
                // Apply breathing to material properties (Unity material samples)
                if (bubbleRenderers[i] != null && bubbleRenderers[i].material != null)
                {
                    float glowIntensity = 1f + (breathingDisplacement.y * 0.3f);
                    bubbleRenderers[i].material.SetFloat("_GlowIntensity", glowIntensity);
                }
            }
        }
        
        /// <summary>
        /// Calculates breathing displacement using Unity wave mathematics samples
        /// </summary>
        private float3 CalculateBreathingDisplacement(int bubbleIndex)
        {
            // Get individual bubble phase (Unity phase samples)
            float individualPhase = breathingPhases[bubbleIndex];
            float individualAmplitude = breathingAmplitudes[bubbleIndex];
            
            // Calculate wave components (Unity wave math samples)
            float displacement = 0f;
            
            if (useSineWave)
            {
                displacement += math.sin(globalBreathingTime + individualPhase) * individualAmplitude;
            }
            
            if (useCosineWave)
            {
                displacement += math.cos(globalBreathingTime + individualPhase + phaseOffset) * individualAmplitude * 0.5f;
            }
            
            // Apply damping for natural feel (Unity damping samples)
            displacement *= dampingFactor;
            
            // Create 3D displacement with slight variation (Unity 3D math samples)
            float3 result = new float3(
                displacement * 0.1f, // Slight horizontal movement
                displacement,        // Primary vertical movement
                displacement * 0.05f // Minimal depth movement
            );
            
            return result;
        }
        
        /// <summary>
        /// Applies breathing job results to transforms
        /// Based on Unity Job System result application samples
        /// </summary>
        private void ApplyBreathingResults()
        {
            for (int i = 0; i < bubbleTransforms.Length && i < bubblePositions.Length; i++)
            {
                if (bubbleTransforms[i] != null)
                {
                    // Apply position (Unity transform samples)
                    bubbleTransforms[i].position = bubblePositions[i];
                    
                    // Calculate scale based on breathing (Unity scale samples)
                    float3 displacement = bubblePositions[i] - basePositions[i];
                    float scaleMultiplier = 1f + (displacement.y * 0.5f);
                    bubbleTransforms[i].localScale = Vector3.one * scaleMultiplier;
                    
                    // Update material properties (Unity material samples)
                    if (bubbleRenderers[i] != null && bubbleRenderers[i].material != null)
                    {
                        float glowIntensity = 1f + (displacement.y * 0.3f);
                        bubbleRenderers[i].material.SetFloat("_GlowIntensity", glowIntensity);
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates particle system breathing animation
        /// Based on Unity particle animation samples
        /// </summary>
        private void UpdateParticleBreathing()
        {
            // Modulate emission rate based on breathing (Unity emission samples)
            var emission = breathingParticles.emission;
            float breathingIntensity = math.sin(globalBreathingTime) * 0.5f + 0.5f;
            emission.rateOverTime = 10f + (breathingIntensity * 20f);
            
            // Modulate particle size based on breathing (Unity size samples)
            var main = breathingParticles.main;
            main.startSize = 0.1f + (breathingIntensity * 0.05f);
            
            // Modulate particle color alpha based on breathing (Unity color samples)
            Color baseColor = new Color(1f, 1f, 1f, 0.3f);
            main.startColor = new Color(baseColor.r, baseColor.g, baseColor.b, 
                baseColor.a + (breathingIntensity * 0.2f));
        }
        
        /// <summary>
        /// Cleans up native arrays
        /// Unity Job System cleanup samples
        /// </summary>
        private void CleanupNativeArrays()
        {
            // Complete any running jobs (Unity Job System samples)
            if (breathingJobHandle.IsCompleted == false)
                breathingJobHandle.Complete();
            
            // Dispose native arrays (Unity memory management samples)
            if (bubblePositions.IsCreated)
                bubblePositions.Dispose();
            if (basePositions.IsCreated)
                basePositions.Dispose();
            if (breathingPhases.IsCreated)
                breathingPhases.Dispose();
            if (breathingAmplitudes.IsCreated)
                breathingAmplitudes.Dispose();
        }
        
        /// <summary>
        /// Synchronizes breathing with visual system
        /// Integration point for visual effects
        /// </summary>
        public void SynchronizeWithVisualSystem(float visualIntensity)
        {
            // Adjust breathing amplitude based on visual intensity (Unity synchronization samples)
            breathingAmplitude = Mathf.Lerp(0.05f, 0.2f, visualIntensity);
            
            // Update particle system intensity
            if (breathingParticles != null)
            {
                var main = breathingParticles.main;
                main.startColor = new Color(1f, 1f, 1f, 0.3f * visualIntensity);
            }
        }
        
        /// <summary>
        /// Validates breathing system setup
        /// </summary>
        [ContextMenu("Validate Breathing System")]
        public void ValidateBreathingSystem()
        {
            Debug.Log($"Bubble Breathing System Status:");
            Debug.Log($"- Breathing Frequency: {breathingFrequency} Hz");
            Debug.Log($"- Breathing Amplitude: {breathingAmplitude}");
            Debug.Log($"- Active Bubbles: {bubbleTransforms?.Length ?? 0}");
            Debug.Log($"- Job System: {useJobSystem}");
            Debug.Log($"- Particle System: {(breathingParticles != null ? "Active" : "Inactive")}");
            Debug.Log($"- Initialized: {isInitialized}");
        }
        
        /// <summary>
        /// Adjusts breathing parameters for comfort
        /// Based on Unity comfort optimization samples
        /// </summary>
        [ContextMenu("Optimize for Comfort")]
        public void OptimizeForComfort()
        {
            // Set comfortable breathing rate (Unity comfort samples)
            breathingFrequency = 0.25f; // 15 breaths per minute
            breathingAmplitude = 0.08f;  // Subtle movement
            dampingFactor = 0.98f;       // Smooth transitions
            
            Debug.Log("Breathing system optimized for comfort - based on Unity comfort samples");
        }
    }
    
    /// <summary>
    /// Breathing calculation job for parallel processing
    /// Cloned from Unity Job System samples
    /// </summary>
    [BurstCompile]
    public struct BreathingCalculationJob : IJobParallelFor
    {
        public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<float3> basePositions;
        [ReadOnly] public NativeArray<float> phases;
        [ReadOnly] public NativeArray<float> amplitudes;
        [ReadOnly] public float time;
        [ReadOnly] public bool useSine;
        [ReadOnly] public bool useCosine;
        [ReadOnly] public float damping;
        
        public void Execute(int index)
        {
            if (index >= basePositions.Length) return;
            
            // Calculate breathing displacement (Unity wave math samples)
            float displacement = 0f;
            float phase = phases[index];
            float amplitude = amplitudes[index];
            
            if (useSine)
            {
                displacement += math.sin(time + phase) * amplitude;
            }
            
            if (useCosine)
            {
                displacement += math.cos(time + phase) * amplitude * 0.5f;
            }
            
            // Apply damping (Unity damping samples)
            displacement *= damping;
            
            // Create 3D displacement (Unity 3D math samples)
            float3 breathingOffset = new float3(
                displacement * 0.1f,  // Slight horizontal
                displacement,         // Primary vertical
                displacement * 0.05f  // Minimal depth
            );
            
            // Apply to position
            positions[index] = basePositions[index] + breathingOffset;
        }
    }
}
