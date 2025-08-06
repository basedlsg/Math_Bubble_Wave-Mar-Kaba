using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using System.Threading.Tasks;
using XRBubbleLibrary.AI;

namespace XRBubbleLibrary.Mathematics
{
    /// <summary>
    /// Advanced wave system combining mathematical wave physics with AI optimization
    /// Hybrid approach: deterministic mathematical foundation + AI bias fields
    /// Optimized for Quest 3 performance (60-90 FPS target)
    /// </summary>
    public class AdvancedWaveSystem : MonoBehaviour
    {
        [Header("Wave Physics Configuration")]
        [Range(0.2f, 0.5f)]
        public float baseBreathingFrequency = 0.3f;
        
        [Range(0.1f, 0.5f)]
        public float waveAmplitude = 0.2f;
        
        [Range(1.0f, 5.0f)]
        public float waveSpeed = 2.0f;
        
        [Header("AI Integration")]
        public bool enableAIOptimization = true;
        public LocalAIModel aiModel;
        
        [Range(0.0f, 1.0f)]
        public float aiBiasStrength = 0.3f;
        
        [Header("Performance Optimization")]
        public bool enableJobSystem = true;
        public int maxBubblesPerJob = 64;
        
        [Range(1, 8)]
        public int jobBatchSize = 4;
        
        // Wave computation data
        private NativeArray<WaveData> waveDataArray;
        private NativeArray<float3> positionArray;
        private NativeArray<float3> velocityArray;
        private NativeArray<float> phaseArray;
        
        // AI integration
        private BiasField currentBiasField;
        private float lastAIUpdateTime = 0.0f;
        private const float AI_UPDATE_INTERVAL = 0.1f; // Update AI bias 10 times per second
        
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
        void InitializeWaveSystem()
        {
            // Initialize native arrays for job system
            int maxBubbles = 200; // Quest 3 optimized maximum
            
            waveDataArray = new NativeArray<WaveData>(maxBubbles, Allocator.Persistent);
            positionArray = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
            velocityArray = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
            phaseArray = new NativeArray<float>(maxBubbles, Allocator.Persistent);
            
            // Initialize AI model if not assigned
            if (aiModel == null && enableAIOptimization)
            {
                aiModel = FindObjectOfType<LocalAIModel>();
                if (aiModel == null)
                {
                    Debug.LogWarning("LocalAIModel not found. AI optimization disabled.");
                    enableAIOptimization = false;
                }
            }
            
            Debug.Log("Advanced Wave System initialized for Quest 3 optimization");
        }
        
        /// <summary>
        /// Update the wave system each frame
        /// </summary>
        void UpdateWaveSystem()
        {
            var startTime = Time.realtimeSinceStartup;
            
            // Update AI bias field periodically
            if (enableAIOptimization && Time.time - lastAIUpdateTime >= AI_UPDATE_INTERVAL)
            {
                UpdateAIBiasField();
                lastAIUpdateTime = Time.time;
            }
            
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
        public async Task<float3[]> GenerateOptimalBubblePositions(int bubbleCount, float3 userPosition)
        {
            // 1. Generate mathematical foundation using advanced wave patterns
            var mathematicalPositions = GenerateMathematicalWavePattern(bubbleCount, userPosition);
            
            // 2. Apply AI optimization if enabled
            if (enableAIOptimization && aiModel != null)
            {
                var biasField = await aiModel.GenerateBiasField(userPosition, bubbleCount);
                return ApplyAIBiasToPositions(mathematicalPositions, biasField);
            }
            
            return mathematicalPositions;
        }
        
        /// <summary>
        /// Generate mathematical wave pattern foundation
        /// </summary>
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
        
        /// <summary>
        /// Apply AI bias field to mathematical positions
        /// </summary>
        float3[] ApplyAIBiasToPositions(float3[] mathematicalPositions, BiasField biasField)
        {
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
        async void UpdateAIBiasField()
        {
            if (aiModel == null) return;
            
            try
            {
                var userPosition = Camera.main?.transform.position ?? float3.zero;
                currentBiasField = await aiModel.GenerateBiasField(userPosition, activeBubbleCount);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AI bias field update failed: {ex.Message}");
            }
        }
        
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
                currentBiasFieldSize = currentBiasField?.Length ?? 0
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
        public void ForceUpdate()
        {
            if (currentJobHandle.IsCreated)
            {
                currentJobHandle.Complete();
            }
            
            UpdateWaveSystem();
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
}