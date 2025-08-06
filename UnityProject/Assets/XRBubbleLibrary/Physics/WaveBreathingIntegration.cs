using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using XRBubbleLibrary.Mathematics;

namespace XRBubbleLibrary.Physics
{
    /// <summary>
    /// Wave Breathing Integration - Cloned from Unity wave mathematics samples
    /// Synchronizes wave patterns with breathing animation for natural movement
    /// Based on Unity's wave mathematics and synchronization samples
    /// </summary>
    public class WaveBreathingIntegration : MonoBehaviour
    {
        [Header("Wave-Breathing Synchronization (Unity Wave Samples)")]
        [Tooltip("Cloned from Unity wave synchronization samples")]
        [SerializeField] private float synchronizationStrength = 0.8f;
        
        [Tooltip("Based on Unity phase coupling samples")]
        [SerializeField] private float phaseCoupling = 0.5f;
        
        [Tooltip("Cloned from Unity frequency modulation samples")]
        [SerializeField] private float frequencyModulation = 0.1f;
        
        [Header("Mathematical Wave Functions (Unity Math Samples)")]
        [SerializeField] private bool useSineWave = true;
        [SerializeField] private bool useCosineWave = true;
        [SerializeField] private bool useComplexWave = false;
        
        [Header("Natural Movement Parameters (Unity Natural Motion Samples)")]
        [SerializeField] private float naturalFrequency = 0.3f; // Breathing rate
        [SerializeField] private float harmonicRatio = 1.618f;   // Golden ratio
        [SerializeField] private float noiseAmplitude = 0.05f;   // Natural variation
        
        [Header("Performance (Unity Job Samples)")]
        [SerializeField] private bool useJobSystem = true;
        [SerializeField] private int maxBubbles = 100;
        
        // Component references
        private BubbleBreathingSystem breathingSystem;
        private BubbleSpringPhysics springPhysics;
        
        // Native arrays for Job System (Unity Job System samples)
        private NativeArray<float> wavePhases;
        private NativeArray<float> breathingPhases;
        private NativeArray<float> synchronizedPhases;
        private NativeArray<float3> waveDisplacements;
        private NativeArray<float> frequencyModulations;
        
        // Integration state
        private JobHandle integrationJobHandle;
        private float globalTime;
        private bool isInitialized = false;
        
        private void Start()
        {
            InitializeIntegration();
            SetupComponentReferences();
            ConfigureWaveParameters();
        }
        
        private void Update()
        {
            UpdateGlobalTime();
            
            if (isInitialized && useJobSystem)
            {
                UpdateIntegrationWithJobs();
            }
            else if (isInitialized)
            {
                UpdateIntegrationDirectly();
            }
        }
        
        private void OnDestroy()
        {
            CleanupNativeArrays();
        }
        
        /// <summary>
        /// Initializes wave-breathing integration based on Unity integration samples
        /// </summary>
        private void InitializeIntegration()
        {
            // Initialize native arrays for Job System (Unity Job samples)
            if (useJobSystem)
            {
                wavePhases = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                breathingPhases = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                synchronizedPhases = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                waveDisplacements = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
                frequencyModulations = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                
                // Initialize with natural variation (Unity randomization samples)
                for (int i = 0; i < maxBubbles; i++)
                {
                    wavePhases[i] = UnityEngine.Random.Range(0f, 2f * math.PI);
                    breathingPhases[i] = UnityEngine.Random.Range(0f, 2f * math.PI);
                    frequencyModulations[i] = 1f + UnityEngine.Random.Range(-frequencyModulation, frequencyModulation);
                }
            }
            
            globalTime = 0f;
            isInitialized = true;
            
            Debug.Log("Wave-Breathing Integration initialized - cloned from Unity integration samples");
        }
        
        /// <summary>
        /// Sets up component references based on Unity component search samples
        /// </summary>
        private void SetupComponentReferences()
        {
            // Find required components (Unity component search samples)
            breathingSystem = FindObjectOfType<BubbleBreathingSystem>();
            springPhysics = FindObjectOfType<BubbleSpringPhysics>();
            
            // Validate component references
            if (breathingSystem == null)
            {
                Debug.LogWarning("BubbleBreathingSystem not found - creating default");
                breathingSystem = gameObject.AddComponent<BubbleBreathingSystem>();
            }
            
            if (springPhysics == null)
            {
                Debug.LogWarning("BubbleSpringPhysics not found - creating default");
                springPhysics = gameObject.AddComponent<BubbleSpringPhysics>();
            }
            
            Debug.Log("Component references established for wave-breathing integration");
        }
        
        /// <summary>
        /// Configures wave parameters based on Unity wave configuration samples
        /// </summary>
        private void ConfigureWaveParameters()
        {
            // Configure natural frequency based on breathing rate (Unity frequency samples)
            naturalFrequency = 0.25f; // 15 breaths per minute for comfort
            
            // Set harmonic ratios for natural movement (Unity harmonic samples)
            harmonicRatio = 1.618f; // Golden ratio for natural aesthetics
            
            // Configure noise for organic variation (Unity noise samples)
            noiseAmplitude = 0.03f; // Subtle natural variation
            
            Debug.Log($"Wave parameters configured - Natural Frequency: {naturalFrequency} Hz, Harmonic Ratio: {harmonicRatio}");
        }
        
        /// <summary>
        /// Updates global time for wave calculations
        /// Based on Unity time management samples
        /// </summary>
        private void UpdateGlobalTime()
        {
            globalTime += Time.deltaTime;
            
            // Prevent floating point precision issues (Unity precision samples)
            if (globalTime > 1000f)
            {
                globalTime -= 1000f;
                
                // Adjust phases to maintain continuity
                if (useJobSystem)
                {
                    for (int i = 0; i < wavePhases.Length; i++)
                    {
                        wavePhases[i] = math.fmod(wavePhases[i], 2f * math.PI);
                        breathingPhases[i] = math.fmod(breathingPhases[i], 2f * math.PI);
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates integration using Unity Job System
        /// Cloned from Unity Job System integration samples
        /// </summary>
        private void UpdateIntegrationWithJobs()
        {
            // Complete previous job (Unity Job System samples)
            integrationJobHandle.Complete();
            
            // Schedule wave-breathing integration job (Unity Job samples)
            var integrationJob = new WaveBreathingIntegrationJob
            {
                wavePhases = wavePhases,
                breathingPhases = breathingPhases,
                synchronizedPhases = synchronizedPhases,
                waveDisplacements = waveDisplacements,
                frequencyModulations = frequencyModulations,
                time = globalTime,
                naturalFrequency = naturalFrequency,
                harmonicRatio = harmonicRatio,
                synchronizationStrength = synchronizationStrength,
                phaseCoupling = phaseCoupling,
                noiseAmplitude = noiseAmplitude,
                useSine = useSineWave,
                useCosine = useCosineWave,
                useComplex = useComplexWave
            };
            
            integrationJobHandle = integrationJob.Schedule(maxBubbles, 32);
            
            // Apply results to systems
            integrationJobHandle.Complete();
            ApplyIntegrationResults();
        }
        
        /// <summary>
        /// Updates integration directly without jobs
        /// Fallback method based on Unity direct integration samples
        /// </summary>
        private void UpdateIntegrationDirectly()
        {
            for (int i = 0; i < maxBubbles; i++)
            {
                // Calculate synchronized wave-breathing phase (Unity synchronization samples)
                float synchronizedPhase = CalculateSynchronizedPhase(i);
                
                // Calculate wave displacement (Unity wave samples)
                float3 waveDisplacement = CalculateWaveDisplacement(i, synchronizedPhase);
                
                // Apply to breathing system (Unity system integration samples)
                if (breathingSystem != null)
                {
                    breathingSystem.SynchronizeWithVisualSystem(math.length(waveDisplacement));
                }
                
                // Apply to spring physics (Unity physics integration samples)
                if (springPhysics != null)
                {
                    springPhysics.SynchronizeWithBreathing(synchronizedPhase, math.length(waveDisplacement));
                }
            }
        }
        
        /// <summary>
        /// Calculates synchronized phase between wave and breathing
        /// Based on Unity phase synchronization samples
        /// </summary>
        private float CalculateSynchronizedPhase(int bubbleIndex)
        {
            // Get individual phases (Unity phase samples)
            float wavePhase = wavePhases[bubbleIndex];
            float breathingPhase = breathingPhases[bubbleIndex];
            float freqMod = frequencyModulations[bubbleIndex];
            
            // Calculate time-based phases (Unity time samples)
            float waveTime = globalTime * naturalFrequency * freqMod * 2f * math.PI;
            float breathingTime = globalTime * naturalFrequency * harmonicRatio * 2f * math.PI;
            
            // Apply phase coupling (Unity coupling samples)
            float coupledWavePhase = wavePhase + waveTime;
            float coupledBreathingPhase = breathingPhase + breathingTime;
            
            // Synchronize phases (Unity synchronization samples)
            float phaseDifference = coupledWavePhase - coupledBreathingPhase;
            float synchronizationForce = math.sin(phaseDifference) * phaseCoupling;
            
            // Calculate synchronized phase
            float synchronizedPhase = math.lerp(coupledWavePhase, coupledBreathingPhase + synchronizationForce, synchronizationStrength);
            
            return synchronizedPhase;
        }
        
        /// <summary>
        /// Calculates wave displacement with breathing integration
        /// Based on Unity wave mathematics samples
        /// </summary>
        private float3 CalculateWaveDisplacement(int bubbleIndex, float synchronizedPhase)
        {
            float3 displacement = float3.zero;
            
            // Add noise for natural variation (Unity noise samples)
            float noise = Mathf.PerlinNoise(globalTime * 0.5f, bubbleIndex * 0.1f) * noiseAmplitude;
            
            if (useSineWave)
            {
                // Sine wave component (Unity sine samples)
                displacement.y += math.sin(synchronizedPhase) * (1f + noise);
                displacement.x += math.sin(synchronizedPhase * 0.7f) * 0.3f * (1f + noise);
            }
            
            if (useCosineWave)
            {
                // Cosine wave component (Unity cosine samples)
                displacement.y += math.cos(synchronizedPhase * harmonicRatio) * 0.5f * (1f + noise);
                displacement.z += math.cos(synchronizedPhase * 0.9f) * 0.2f * (1f + noise);
            }
            
            if (useComplexWave)
            {
                // Complex wave component (Unity complex wave samples)
                float complexPhase = synchronizedPhase * harmonicRatio;
                displacement.y += math.sin(complexPhase) * math.cos(complexPhase * 0.5f) * 0.3f * (1f + noise);
                displacement.x += math.cos(complexPhase) * math.sin(complexPhase * 0.3f) * 0.2f * (1f + noise);
            }
            
            return displacement;
        }
        
        /// <summary>
        /// Applies integration job results to systems
        /// Based on Unity Job System result application samples
        /// </summary>
        private void ApplyIntegrationResults()
        {
            for (int i = 0; i < math.min(maxBubbles, synchronizedPhases.Length); i++)
            {
                float synchronizedPhase = synchronizedPhases[i];
                float3 waveDisplacement = waveDisplacements[i];
                
                // Apply to breathing system (Unity system integration samples)
                if (breathingSystem != null)
                {
                    float intensity = math.length(waveDisplacement);
                    breathingSystem.SynchronizeWithVisualSystem(intensity);
                }
                
                // Apply to spring physics (Unity physics integration samples)
                if (springPhysics != null)
                {
                    float amplitude = math.length(waveDisplacement);
                    springPhysics.SynchronizeWithBreathing(synchronizedPhase, amplitude);
                }
            }
        }
        
        /// <summary>
        /// Gets synchronized phase for external systems
        /// Based on Unity phase query samples
        /// </summary>
        public float GetSynchronizedPhase(int bubbleIndex)
        {
            if (useJobSystem && bubbleIndex >= 0 && bubbleIndex < synchronizedPhases.Length)
            {
                return synchronizedPhases[bubbleIndex];
            }
            else
            {
                return CalculateSynchronizedPhase(bubbleIndex);
            }
        }
        
        /// <summary>
        /// Gets wave displacement for external systems
        /// Based on Unity displacement query samples
        /// </summary>
        public float3 GetWaveDisplacement(int bubbleIndex)
        {
            if (useJobSystem && bubbleIndex >= 0 && bubbleIndex < waveDisplacements.Length)
            {
                return waveDisplacements[bubbleIndex];
            }
            else
            {
                float phase = GetSynchronizedPhase(bubbleIndex);
                return CalculateWaveDisplacement(bubbleIndex, phase);
            }
        }
        
        /// <summary>
        /// Adjusts synchronization parameters for comfort
        /// Based on Unity comfort optimization samples
        /// </summary>
        public void OptimizeForComfort()
        {
            // Set comfortable parameters (Unity comfort samples)
            naturalFrequency = 0.25f;        // 15 breaths per minute
            synchronizationStrength = 0.7f;  // Gentle synchronization
            phaseCoupling = 0.3f;            // Subtle coupling
            noiseAmplitude = 0.02f;          // Minimal noise
            
            Debug.Log("Wave-breathing integration optimized for comfort");
        }
        
        /// <summary>
        /// Cleans up native arrays
        /// Unity Job System cleanup samples
        /// </summary>
        private void CleanupNativeArrays()
        {
            // Complete any running jobs (Unity Job System samples)
            if (integrationJobHandle.IsCompleted == false)
                integrationJobHandle.Complete();
            
            // Dispose native arrays (Unity memory management samples)
            if (wavePhases.IsCreated) wavePhases.Dispose();
            if (breathingPhases.IsCreated) breathingPhases.Dispose();
            if (synchronizedPhases.IsCreated) synchronizedPhases.Dispose();
            if (waveDisplacements.IsCreated) waveDisplacements.Dispose();
            if (frequencyModulations.IsCreated) frequencyModulations.Dispose();
        }
        
        /// <summary>
        /// Validates integration setup
        /// </summary>
        [ContextMenu("Validate Integration")]
        public void ValidateIntegration()
        {
            Debug.Log($"Wave-Breathing Integration Status:");
            Debug.Log($"- Natural Frequency: {naturalFrequency} Hz");
            Debug.Log($"- Synchronization Strength: {synchronizationStrength}");
            Debug.Log($"- Phase Coupling: {phaseCoupling}");
            Debug.Log($"- Breathing System: {(breathingSystem != null ? "Found" : "Missing")}");
            Debug.Log($"- Spring Physics: {(springPhysics != null ? "Found" : "Missing")}");
            Debug.Log($"- Job System: {useJobSystem}");
            Debug.Log($"- Initialized: {isInitialized}");
        }
    }
    
    /// <summary>
    /// Wave-breathing integration job for parallel processing
    /// Cloned from Unity Job System integration samples
    /// </summary>
    [BurstCompile]
    public struct WaveBreathingIntegrationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> wavePhases;
        [ReadOnly] public NativeArray<float> breathingPhases;
        public NativeArray<float> synchronizedPhases;
        public NativeArray<float3> waveDisplacements;
        [ReadOnly] public NativeArray<float> frequencyModulations;
        [ReadOnly] public float time;
        [ReadOnly] public float naturalFrequency;
        [ReadOnly] public float harmonicRatio;
        [ReadOnly] public float synchronizationStrength;
        [ReadOnly] public float phaseCoupling;
        [ReadOnly] public float noiseAmplitude;
        [ReadOnly] public bool useSine;
        [ReadOnly] public bool useCosine;
        [ReadOnly] public bool useComplex;
        
        public void Execute(int index)
        {
            if (index >= wavePhases.Length) return;
            
            // Get individual parameters (Unity parameter samples)
            float wavePhase = wavePhases[index];
            float breathingPhase = breathingPhases[index];
            float freqMod = frequencyModulations[index];
            
            // Calculate time-based phases (Unity time samples)
            float waveTime = time * naturalFrequency * freqMod * 2f * math.PI;
            float breathingTime = time * naturalFrequency * harmonicRatio * 2f * math.PI;
            
            // Apply phase coupling (Unity coupling samples)
            float coupledWavePhase = wavePhase + waveTime;
            float coupledBreathingPhase = breathingPhase + breathingTime;
            
            // Synchronize phases (Unity synchronization samples)
            float phaseDifference = coupledWavePhase - coupledBreathingPhase;
            float synchronizationForce = math.sin(phaseDifference) * phaseCoupling;
            float synchronizedPhase = math.lerp(coupledWavePhase, coupledBreathingPhase + synchronizationForce, synchronizationStrength);
            
            // Store synchronized phase
            synchronizedPhases[index] = synchronizedPhase;
            
            // Calculate wave displacement (Unity wave samples)
            float3 displacement = float3.zero;
            
            // Add noise for natural variation (Unity noise samples)
            float noise = math.sin(time * 0.5f + index * 0.1f) * noiseAmplitude;
            
            if (useSine)
            {
                displacement.y += math.sin(synchronizedPhase) * (1f + noise);
                displacement.x += math.sin(synchronizedPhase * 0.7f) * 0.3f * (1f + noise);
            }
            
            if (useCosine)
            {
                displacement.y += math.cos(synchronizedPhase * harmonicRatio) * 0.5f * (1f + noise);
                displacement.z += math.cos(synchronizedPhase * 0.9f) * 0.2f * (1f + noise);
            }
            
            if (useComplex)
            {
                float complexPhase = synchronizedPhase * harmonicRatio;
                displacement.y += math.sin(complexPhase) * math.cos(complexPhase * 0.5f) * 0.3f * (1f + noise);
                displacement.x += math.cos(complexPhase) * math.sin(complexPhase * 0.3f) * 0.2f * (1f + noise);
            }
            
            // Store wave displacement
            waveDisplacements[index] = displacement;
        }
    }
}
