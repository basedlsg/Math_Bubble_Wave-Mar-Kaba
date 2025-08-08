#if EXP_ADVANCED_WAVE
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using System.Threading.Tasks;
using XRBubbleLibrary.Core;

// Conditional AI import
#if EXP_AI
using XRBubbleLibrary.AI;
#endif

namespace XRBubbleLibrary.Mathematics
{
    /// <summary>
    /// Advanced wave system combining mathematical wave physics with optional AI optimization
    /// Hybrid approach: deterministic mathematical foundation + optional AI bias fields
    /// Optimized for Quest 3 performance (60-90 FPS target)
    /// 
    /// EXPERIMENTAL FEATURE: Requires EXP_ADVANCED_WAVE compiler flag
    /// Part of the "do-it-right" recovery Phase 0 implementation
    /// </summary>
    [FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS,
                 ErrorMessage = "Advanced Wave System requires Advanced Wave Algorithms to be enabled")]
    public class AdvancedWaveSystem : MonoBehaviour
    {
        [Header("Wave Physics Configuration")]
        [Range(0.2f, 0.5f)]
        [SerializeField] private float baseBreathingFrequency = 0.3f;
        
        [Range(0.1f, 0.5f)]
        [SerializeField] private float waveAmplitude = 0.2f;
        
        [Range(1.0f, 5.0f)]
        [SerializeField] private float waveSpeed = 2.0f;
        
        [Header("AI Integration")]
#if EXP_AI
        [SerializeField] private bool enableAIOptimization = true;
        [SerializeField] private LocalAIModel aiModel;
        
        [Range(0.0f, 1.0f)]
        [SerializeField] private float aiBiasStrength = 0.3f;
#else
        [SerializeField] private bool enableAIOptimization = false;
        [SerializeField] private float aiBiasStrength = 0.0f;
#endif
        
        [Header("Performance Optimization")]
        [SerializeField] private bool enableJobSystem = true;
        [SerializeField] private int maxBubblesPerJob = 64;
        
        [Range(1, 8)]
        [SerializeField] private int jobBatchSize = 4;
        
        // Wave computation data
        private NativeArray<WaveData> waveDataArray;
        private NativeArray<float3> positionArray;
        private NativeArray<float3> velocityArray;
        private NativeArray<float> phaseArray;
        
        // AI integration (conditional)
#if EXP_AI
        private BiasField currentBiasField;
        private float lastAIUpdateTime = 0.0f;
        private const float AI_UPDATE_INTERVAL = 0.1f; // Update AI bias 10 times per second
#endif
        
        // Job system
        private JobHandle currentJobHandle;
        private List<AdvancedWaveJob> activeJobs = new List<AdvancedWaveJob>();
        
        // Performance tracking
        private float lastComputeTime = 0.0f;
        private int activeBubbleCount = 0;
        
        // Mathematical constants
        private const float GOLDEN_RATIO = 1.618033988749f;
        private const float TWO_PI = 2.0f * math.PI;
        
        void Start()
        {
            // Validate feature access before initialization
            CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
            
            InitializeWaveSystem();
        }
        
        void Update()
        {
            UpdateWaveSystem();
        }
        
        void OnDestroy()
        {
            CleanupWaveSystem();
        }
        
        /// <summary>
        /// Initialize the advanced wave system
        /// </summary>
        [FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS)]
        void InitializeWaveSystem()
        {
            // Initialize native arrays for job system
            int maxBubbles = 200; // Quest 3 optimized maximum
            
            waveDataArray = new NativeArray<WaveData>(maxBubbles, Allocator.Persistent);
            positionArray = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
            velocityArray = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
            phaseArray = new NativeArray<float>(maxBubbles, Allocator.Persistent);
            
#if EXP_AI
            // Initialize AI model if not assigned and AI is enabled
            if (aiModel == null && enableAIOptimization)
            {
                CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION);
                
                aiModel = FindObjectOfType<LocalAIModel>();
                if (aiModel == null)
                {
                    Debug.LogWarning("[AdvancedWaveSystem] LocalAIModel not found. AI optimization disabled.");
                    enableAIOptimization = false;
                }
            }
#else
            if (enableAIOptimization)
            {
                Debug.LogWarning("[AdvancedWaveSystem] AI optimization requested but EXP_AI is disabled. AI features unavailable.");
                enableAIOptimization = false;
            }
#endif
            
            Debug.Log("[AdvancedWaveSystem] Advanced Wave System initialized for Quest 3 optimization");
        }
        
        /// <summary>
        /// Update the wave system each frame
        /// </summary>
        void UpdateWaveSystem()
        {
            var startTime = Time.realtimeSinceStartup;
            
#if EXP_AI
            // Update AI bias field periodically
            if (enableAIOptimization && Time.time - lastAIUpdateTime >= AI_UPDATE_INTERVAL)
            {
                UpdateAIBiasField();
                lastAIUpdateTime = Time.time;
            }
#endif
            
            // Complete previous frame's jobs
            if (currentJobHandle.IsCompleted)
            {
                currentJobHandle.Complete();
                ApplyJobResults();
            }
            
            // Schedule new wave computation jobs
            if (activeBubbleCount > 0)
            {
                ScheduleWaveJobs();
            }
            
            // Track performance
            lastComputeTime = (Time.realtimeSinceStartup - startTime) * 1000f;
        }
        
        /// <summary>
        /// Generate optimal bubble positions using hybrid mathematical-AI approach
        /// </summary>
        [FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS)]
        public async Task<float3[]> GenerateOptimalBubblePositions(int bubbleCount, float3 userPosition)
        {
            CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
            
            // 1. Generate mathematical foundation using advanced wave patterns
            var mathematicalPositions = GenerateMathematicalWavePattern(bubbleCount, userPosition);
            
#if EXP_AI
            // 2. Apply AI optimization if enabled
            if (enableAIOptimization && aiModel != null)
            {
                CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION);
                
                var biasField = await aiModel.GenerateBiasField(userPosition, bubbleCount);
                return ApplyAIBiasToPositions(mathematicalPositions, biasField);
            }
#endif
            
            return mathematicalPositions;
        }
        
        /// <summary>
        /// Generate mathematical wave pattern foundation
        /// </summary>
        [FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS)]
        float3[] GenerateMathematicalWavePattern(int bubbleCount, float3 userPosition)
        {
            var positions = new float3[bubbleCount];
            
            // Use advanced mathematical patterns for natural arrangement
            for (int i = 0; i < bubbleCount; i++)
            {
                // Fibonacci spiral with wave modulation
                float fibonacciAngle = i * TWO_PI / GOLDEN_RATIO;
                float spiralRadius = math.sqrt(i / (float)bubbleCount) * 2.0f;
                
                // Wave modulation for organic feel
                float waveModulation = math.sin(Time.time * waveSpeed + i * 0.1f) * waveAmplitude;
                float radius = spiralRadius + waveModulation;
                
                // Calculate base position
                float3 basePosition = new float3(
                    math.cos(fibonacciAngle) * radius,
                    math.sin(Time.time * baseBreathingFrequency + i * 0.2f) * 0.1f, // Breathing
                    math.sin(fibonacciAngle) * radius
                );
                
                // Add harmonic variations for musical spacing
                float harmonicRatio = GetHarmonicRatio(i);
                basePosition *= harmonicRatio;
                
                // Position relative to user
                positions[i] = userPosition + basePosition + new float3(0, 0, -1.5f);
            }
            
            return positions;
        }
        
#if EXP_AI
        /// <summary>
        /// Apply AI bias field to mathematical positions
        /// </summary>
        [FeatureGate(ExperimentalFeature.AI_INTEGRATION)]
        float3[] ApplyAIBiasToPositions(float3[] mathematicalPositions, BiasField biasField)
        {
            CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION);
            
            var optimizedPositions = new float3[mathematicalPositions.Length];
            
            for (int i = 0; i < mathematicalPositions.Length; i++)
            {
                // Apply AI bias while preserving mathematical harmony
                float3 bias = biasField.GetBias(i) * aiBiasStrength;
                optimizedPositions[i] = mathematicalPositions[i] + bias;
            }
            
            return optimizedPositions;
        }
        
        /// <summary>
        /// Update AI bias field for wave optimization
        /// </summary>
        [FeatureGate(ExperimentalFeature.AI_INTEGRATION)]
        async void UpdateAIBiasField()
        {
            CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION);
            
            if (aiModel == null) return;
            
            try
            {
                var userPosition = Camera.main?.transform.position ?? float3.zero;
                currentBiasField = await aiModel.GenerateBiasField(userPosition, activeBubbleCount);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AdvancedWaveSystem] AI bias field update failed: {ex.Message}");
            }
        }
#endif
        
        /// <summary>
        /// Schedule wave computation jobs for performance
        /// </summary>
        void ScheduleWaveJobs()
        {
            if (!enableJobSystem || activeBubbleCount == 0) return;
            
            // Prepare job data
            PrepareJobData();
            
            // Create and schedule wave computation job
            var waveJob = new AdvancedWaveJob
            {
                waveData = waveDataArray,
                positions = positionArray,
                velocities = velocityArray,
                phases = phaseArray,
                deltaTime = Time.deltaTime,
                currentTime = Time.time,
                biasStrength = aiBiasStrength,
                bubbleCount = activeBubbleCount
            };
            
            // Schedule job with appropriate batch size
            currentJobHandle = waveJob.Schedule(activeBubbleCount, jobBatchSize);
        }
        
        /// <summary>
        /// Prepare data for job system processing
        /// </summary>
        void PrepareJobData()
        {
            // Update wave data for each active bubble
            for (int i = 0; i < activeBubbleCount; i++)
            {
                waveDataArray[i] = new WaveData
                {
                    frequency = baseBreathingFrequency + (i * 0.01f), // Slight variation
                    amplitude = waveAmplitude,
                    phase = phaseArray[i],
                    waveSpeed = waveSpeed,
                    harmonicRatio = GetHarmonicRatio(i)
                };
            }
        }
        
        /// <summary>
        /// Apply results from completed jobs
        /// </summary>
        void ApplyJobResults()
        {
            // Results are automatically written to native arrays by jobs
            // Additional processing can be done here if needed
        }
        
        /// <summary>
        /// Get harmonic ratio for musical spacing
        /// </summary>
        float GetHarmonicRatio(int index)
        {
            // Use musical harmonic ratios for pleasing spatial relationships
            float[] harmonicRatios = { 1.0f, 1.125f, 1.25f, 1.333f, 1.5f, 1.667f, 1.875f, 2.0f };
            return harmonicRatios[index % harmonicRatios.Length];
        }
        
        /// <summary>
        /// Register bubble for wave processing
        /// </summary>
        public void RegisterBubble(int bubbleId, float3 initialPosition)
        {
            if (activeBubbleCount >= waveDataArray.Length) return;
            
            int index = activeBubbleCount;
            positionArray[index] = initialPosition;
            velocityArray[index] = float3.zero;
            phaseArray[index] = UnityEngine.Random.Range(0f, TWO_PI);
            
            activeBubbleCount++;
        }
        
        /// <summary>
        /// Unregister bubble from wave processing
        /// </summary>
        public void UnregisterBubble(int bubbleId)
        {
            // Simple implementation - just reduce count
            // More sophisticated implementation would compact arrays
            if (activeBubbleCount > 0)
            {
                activeBubbleCount--;
            }
        }
        
        /// <summary>
        /// Get current wave system performance metrics
        /// </summary>
        public WaveSystemMetrics GetPerformanceMetrics()
        {
            return new WaveSystemMetrics
            {
                lastComputeTimeMs = lastComputeTime,
                activeBubbleCount = activeBubbleCount,
                aiOptimizationEnabled = enableAIOptimization,
                jobSystemEnabled = enableJobSystem,
#if EXP_AI
                currentBiasFieldSize = currentBiasField?.Length ?? 0
#else
                currentBiasFieldSize = 0
#endif
            };
        }
        
        /// <summary>
        /// Cleanup native arrays and jobs
        /// </summary>
        void CleanupWaveSystem()
        {
            // Complete any pending jobs
            if (currentJobHandle.IsCreated)
            {
                currentJobHandle.Complete();
            }
            
            // Dispose native arrays
            if (waveDataArray.IsCreated) waveDataArray.Dispose();
            if (positionArray.IsCreated) positionArray.Dispose();
            if (velocityArray.IsCreated) velocityArray.Dispose();
            if (phaseArray.IsCreated) phaseArray.Dispose();
        }
        
        /// <summary>
        /// Force immediate wave system update (for testing)
        /// </summary>
        [FeatureGate(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS)]
        public void ForceUpdate()
        {
            CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.ADVANCED_WAVE_ALGORITHMS);
            
            if (currentJobHandle.IsCreated)
            {
                currentJobHandle.Complete();
            }
            
            UpdateWaveSystem();
        }
        
        /// <summary>
        /// Enable or disable AI optimization
        /// </summary>
        public void SetAIOptimization(bool enabled)
        {
#if EXP_AI
            if (enabled)
            {
                CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.AI_INTEGRATION);
            }
            enableAIOptimization = enabled;
#else
            if (enabled)
            {
                Debug.LogWarning("[AdvancedWaveSystem] AI optimization requested but EXP_AI is disabled. Enable EXP_AI compiler flag to use AI features.");
            }
            enableAIOptimization = false;
#endif
        }
    }
    
    /// <summary>
    /// Wave data structure for job system
    /// </summary>
    [System.Serializable]
    public struct WaveData
    {
        public float frequency;
        public float amplitude;
        public float phase;
        public float waveSpeed;
        public float harmonicRatio;
    }
    
    /// <summary>
    /// Advanced wave computation job using Unity Job System
    /// </summary>
    public struct AdvancedWaveJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<WaveData> waveData;
        public NativeArray<float3> positions;
        public NativeArray<float3> velocities;
        public NativeArray<float> phases;
        
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float currentTime;
        [ReadOnly] public float biasStrength;
        [ReadOnly] public int bubbleCount;
        
        public void Execute(int index)
        {
            if (index >= bubbleCount) return;
            
            var data = waveData[index];
            var currentPos = positions[index];
            var currentVel = velocities[index];
            var currentPhase = phases[index];
            
            // Update wave phase
            currentPhase += data.frequency * deltaTime * 2 * math.PI;
            if (currentPhase > 2 * math.PI)
                currentPhase -= 2 * math.PI;
            
            // Calculate wave-based movement
            float3 waveForce = new float3(
                math.sin(currentPhase) * data.amplitude,
                math.sin(currentPhase * 0.7f + currentTime * data.waveSpeed) * data.amplitude * 0.5f,
                math.cos(currentPhase * 0.5f) * data.amplitude * 0.3f
            );
            
            // Apply harmonic scaling
            waveForce *= data.harmonicRatio;
            
            // Simple physics integration
            currentVel += waveForce * deltaTime;
            currentVel *= 0.95f; // Damping
            currentPos += currentVel * deltaTime;
            
            // Write back results
            positions[index] = currentPos;
            velocities[index] = currentVel;
            phases[index] = currentPhase;
        }
    }
    
    /// <summary>
    /// Wave system performance metrics
    /// </summary>
    [System.Serializable]
    public struct WaveSystemMetrics
    {
        public float lastComputeTimeMs;
        public int activeBubbleCount;
        public bool aiOptimizationEnabled;
        public bool jobSystemEnabled;
        public int currentBiasFieldSize;
        
        public override string ToString()
        {
            return $"Wave System - Compute: {lastComputeTimeMs:F2}ms, Bubbles: {activeBubbleCount}, " +
                   $"AI: {aiOptimizationEnabled}, Jobs: {jobSystemEnabled}";
        }
    }
    
#if EXP_AI
    // AI-related types (only available when AI is enabled)
    public struct BiasField
    {
        public int Length { get; private set; }
        private float3[] biases;
        
        public BiasField(int size)
        {
            Length = size;
            biases = new float3[size];
        }
        
        public float3 GetBias(int index)
        {
            if (index >= 0 && index < Length)
                return biases[index];
            return float3.zero;
        }
        
        public void SetBias(int index, float3 bias)
        {
            if (index >= 0 && index < Length)
                biases[index] = bias;
        }
    }
    
    // Placeholder for LocalAIModel (would be implemented in AI assembly)
    public class LocalAIModel : MonoBehaviour
    {
        public async Task<BiasField> GenerateBiasField(float3 userPosition, int bubbleCount)
        {
            await Task.Yield();
            var biasField = new BiasField(bubbleCount);
            
            // Placeholder implementation
            for (int i = 0; i < bubbleCount; i++)
            {
                biasField.SetBias(i, new float3(
                    UnityEngine.Random.Range(-0.1f, 0.1f),
                    UnityEngine.Random.Range(-0.1f, 0.1f),
                    UnityEngine.Random.Range(-0.1f, 0.1f)
                ));
            }
            
            return biasField;
        }
    }
#endif
}

#else
// Stub implementation when advanced wave algorithms are disabled
using UnityEngine;
using Unity.Mathematics;
using System.Threading.Tasks;
using XRBubbleLibrary.Core;

namespace XRBubbleLibrary.Mathematics
{
    /// <summary>
    /// Stub implementation of AdvancedWaveSystem when advanced wave algorithms are disabled.
    /// Provides graceful degradation and clear messaging about disabled features.
    /// </summary>
    public class AdvancedWaveSystem : MonoBehaviour
    {
        void Start()
        {
            Debug.LogWarning("[AdvancedWaveSystem] Advanced Wave Algorithms are disabled. Enable EXP_ADVANCED_WAVE compiler flag to use advanced wave features.");
            
            // Optionally disable this GameObject to prevent confusion
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Stub method that warns about disabled advanced wave features
        /// </summary>
        public async Task<float3[]> GenerateOptimalBubblePositions(int bubbleCount, float3 userPosition)
        {
            Debug.LogWarning("[AdvancedWaveSystem] Advanced Wave Algorithms are disabled. Enable EXP_ADVANCED_WAVE compiler flag to use this functionality.");
            
            // Return simple fallback positions
            var positions = new float3[bubbleCount];
            for (int i = 0; i < bubbleCount; i++)
            {
                positions[i] = userPosition + new float3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-0.5f, 0.5f),
                    UnityEngine.Random.Range(1f, 2f)
                );
            }
            
            await Task.Yield();
            return positions;
        }
        
        /// <summary>
        /// Stub method for bubble registration
        /// </summary>
        public void RegisterBubble(int bubbleId, float3 initialPosition)
        {
            Debug.LogWarning("[AdvancedWaveSystem] Advanced Wave Algorithms are disabled. Enable EXP_ADVANCED_WAVE compiler flag to use bubble registration.");
        }
        
        /// <summary>
        /// Stub method for bubble unregistration
        /// </summary>
        public void UnregisterBubble(int bubbleId)
        {
            Debug.LogWarning("[AdvancedWaveSystem] Advanced Wave Algorithms are disabled. Enable EXP_ADVANCED_WAVE compiler flag to use bubble management.");
        }
        
        /// <summary>
        /// Stub method for performance metrics
        /// </summary>
        public WaveSystemMetrics GetPerformanceMetrics()
        {
            return new WaveSystemMetrics
            {
                lastComputeTimeMs = 0f,
                activeBubbleCount = 0,
                aiOptimizationEnabled = false,
                jobSystemEnabled = false,
                currentBiasFieldSize = 0
            };
        }
        
        /// <summary>
        /// Stub method for AI optimization
        /// </summary>
        public void SetAIOptimization(bool enabled)
        {
            Debug.LogWarning("[AdvancedWaveSystem] Advanced Wave Algorithms are disabled. Enable EXP_ADVANCED_WAVE compiler flag to use AI optimization.");
        }
        
        /// <summary>
        /// Stub method for force update
        /// </summary>
        public void ForceUpdate()
        {
            Debug.LogWarning("[AdvancedWaveSystem] Advanced Wave Algorithms are disabled. Enable EXP_ADVANCED_WAVE compiler flag to use force update.");
        }
    }
    
    /// <summary>
    /// Stub wave system metrics for disabled state
    /// </summary>
    [System.Serializable]
    public struct WaveSystemMetrics
    {
        public float lastComputeTimeMs;
        public int activeBubbleCount;
        public bool aiOptimizationEnabled;
        public bool jobSystemEnabled;
        public int currentBiasFieldSize;
        
        public override string ToString()
        {
            return "Wave System - DISABLED (Enable EXP_ADVANCED_WAVE to use advanced features)";
        }
    }
}
#endif