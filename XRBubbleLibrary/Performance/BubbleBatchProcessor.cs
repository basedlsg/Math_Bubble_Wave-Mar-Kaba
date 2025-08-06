using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;
using XRBubbleLibrary.Physics;

namespace XRBubbleLibrary.Performance
{
    /// <summary>
    /// Batch processing system for bubble physics using Unity Job System
    /// Cloned from Unity Job System samples, optimized for Quest 3 performance
    /// </summary>
    public class BubbleBatchProcessor : MonoBehaviour
    {
        [Header("Batch Configuration")]
        [Range(10, 100)]
        public int maxBatchSize = 50;
        
        [Range(1, 10)]
        public int jobsPerFrame = 4;
        
        public bool enableMultithreading = true;
        
        [Header("Processing Settings")]
        public bool enablePhysicsProcessing = true;
        public bool enableBreathingProcessing = true;
        public bool enableWaveProcessing = true;
        
        [Header("Performance Monitoring")]
        public bool enableProfiling = false;
        public float maxProcessingTimeMs = 2.0f; // Max 2ms per frame for Quest 3
        
        // Job data structures
        private NativeArray<BubbleJobData> bubbleJobData;
        private NativeArray<float3> positions;
        private NativeArray<float3> velocities;
        private NativeArray<float> scales;
        private NativeArray<float> breathingPhases;
        
        // Job handles for tracking completion
        private JobHandle currentJobHandle;
        private List<JobHandle> activeJobHandles = new List<JobHandle>();
        
        // Bubble tracking
        private List<PooledBubble> registeredBubbles = new List<PooledBubble>();
        private Dictionary<int, int> bubbleToJobIndex = new Dictionary<int, int>();
        
        // Performance tracking
        private float lastProcessingTime = 0.0f;
        private int processedBubblesThisFrame = 0;
        private float frameStartTime = 0.0f;
        
        // Singleton pattern
        private static BubbleBatchProcessor instance;
        public static BubbleBatchProcessor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<BubbleBatchProcessor>();
                    if (instance == null)
                    {
                        GameObject processor = new GameObject("BubbleBatchProcessor");
                        instance = processor.AddComponent<BubbleBatchProcessor>();
                    }
                }
                return instance;
            }
        }
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeBatchProcessor();
        }
        
        void Update()
        {
            if (enableProfiling)
            {
                frameStartTime = Time.realtimeSinceStartup * 1000f;
            }
            
            ProcessBubbleBatches();
            
            if (enableProfiling)
            {
                lastProcessingTime = (Time.realtimeSinceStartup * 1000f) - frameStartTime;
                
                if (lastProcessingTime > maxProcessingTimeMs)
                {
                    Debug.LogWarning($"Bubble processing exceeded target time: {lastProcessingTime:F2}ms > {maxProcessingTimeMs}ms");
                    AdaptPerformance();
                }
            }
        }
        
        void OnDestroy()
        {
            CleanupBatchProcessor();
            
            if (instance == this)
            {
                instance = null;
            }
        }
        
        /// <summary>
        /// Initialize the batch processing system
        /// Based on cloned Unity Job System samples
        /// </summary>
        void InitializeBatchProcessor()
        {
            // Initialize native arrays with initial capacity
            int initialCapacity = maxBatchSize * 4; // Room for growth
            
            bubbleJobData = new NativeArray<BubbleJobData>(initialCapacity, Allocator.Persistent);
            positions = new NativeArray<float3>(initialCapacity, Allocator.Persistent);
            velocities = new NativeArray<float3>(initialCapacity, Allocator.Persistent);
            scales = new NativeArray<float>(initialCapacity, Allocator.Persistent);
            breathingPhases = new NativeArray<float>(initialCapacity, Allocator.Persistent);
            
            Debug.Log("BubbleBatchProcessor initialized with capacity: " + initialCapacity);
        }
        
        /// <summary>
        /// Process bubble batches using Unity Job System
        /// </summary>
        void ProcessBubbleBatches()
        {
            if (registeredBubbles.Count == 0) return;
            
            // Complete previous frame's jobs
            CompleteActiveJobs();
            
            // Process bubbles in batches
            int bubblesProcessed = 0;
            int jobsThisFrame = 0;
            
            for (int i = 0; i < registeredBubbles.Count && jobsThisFrame < jobsPerFrame; i += maxBatchSize)
            {
                int batchSize = Mathf.Min(maxBatchSize, registeredBubbles.Count - i);
                
                if (batchSize > 0)
                {
                    ProcessBubbleBatch(i, batchSize);
                    bubblesProcessed += batchSize;
                    jobsThisFrame++;
                }
            }
            
            processedBubblesThisFrame = bubblesProcessed;
        }
        
        /// <summary>
        /// Process a single batch of bubbles
        /// </summary>
        void ProcessBubbleBatch(int startIndex, int batchSize)
        {
            // Prepare job data
            for (int i = 0; i < batchSize; i++)
            {
                int bubbleIndex = startIndex + i;
                if (bubbleIndex >= registeredBubbles.Count) break;
                
                PooledBubble bubble = registeredBubbles[bubbleIndex];
                if (bubble == null || !bubble.IsActive()) continue;
                
                // Fill job data arrays
                positions[i] = bubble.transform.position;
                velocities[i] = bubble.GetVelocity();
                scales[i] = bubble.GetScale();
                breathingPhases[i] = bubble.GetBreathingPhase();
                
                bubbleJobData[i] = new BubbleJobData
                {
                    bubbleId = bubble.GetId(),
                    mass = bubble.GetMass(),
                    springConstant = bubble.GetSpringConstant(),
                    dampingFactor = bubble.GetDampingFactor(),
                    breathingFrequency = bubble.GetBreathingFrequency(),
                    deltaTime = Time.deltaTime
                };
            }
            
            // Create and schedule jobs based on enabled processing
            if (enablePhysicsProcessing)
            {
                var physicsJob = new BubblePhysicsJob
                {
                    positions = positions,
                    velocities = velocities,
                    jobData = bubbleJobData,
                    batchSize = batchSize
                };
                
                JobHandle physicsHandle = enableMultithreading ? 
                    physicsJob.Schedule(batchSize, 10) : 
                    physicsJob.Schedule();
                    
                activeJobHandles.Add(physicsHandle);
            }
            
            if (enableBreathingProcessing)
            {
                var breathingJob = new BubbleBreathingJob
                {
                    scales = scales,
                    breathingPhases = breathingPhases,
                    jobData = bubbleJobData,
                    batchSize = batchSize
                };
                
                JobHandle breathingHandle = enableMultithreading ? 
                    breathingJob.Schedule(batchSize, 10) : 
                    breathingJob.Schedule();
                    
                activeJobHandles.Add(breathingHandle);
            }
            
            if (enableWaveProcessing)
            {
                var waveJob = new BubbleWaveJob
                {
                    positions = positions,
                    jobData = bubbleJobData,
                    batchSize = batchSize,
                    waveTime = Time.time
                };
                
                JobHandle waveHandle = enableMultithreading ? 
                    waveJob.Schedule(batchSize, 10) : 
                    waveJob.Schedule();
                    
                activeJobHandles.Add(waveHandle);
            }
        }
        
        /// <summary>
        /// Complete all active jobs and apply results
        /// </summary>
        void CompleteActiveJobs()
        {
            if (activeJobHandles.Count == 0) return;
            
            // Complete all jobs
            JobHandle.CompleteAll(activeJobHandles.ToArray());
            
            // Apply results back to bubbles
            ApplyJobResults();
            
            // Clear completed jobs
            activeJobHandles.Clear();
        }
        
        /// <summary>
        /// Apply job results back to bubble objects
        /// </summary>
        void ApplyJobResults()
        {
            for (int i = 0; i < processedBubblesThisFrame && i < registeredBubbles.Count; i++)
            {
                PooledBubble bubble = registeredBubbles[i];
                if (bubble == null || !bubble.IsActive()) continue;
                
                // Apply physics results
                if (enablePhysicsProcessing && i < positions.Length)
                {
                    bubble.SetPosition(positions[i]);
                    bubble.SetVelocity(velocities[i]);
                }
                
                // Apply breathing results
                if (enableBreathingProcessing && i < scales.Length)
                {
                    bubble.SetScale(scales[i]);
                    bubble.SetBreathingPhase(breathingPhases[i]);
                }
            }
        }
        
        /// <summary>
        /// Register a bubble for batch processing
        /// </summary>
        public void RegisterBubble(PooledBubble bubble)
        {
            if (bubble == null || registeredBubbles.Contains(bubble)) return;
            
            registeredBubbles.Add(bubble);
            bubbleToJobIndex[bubble.GetId()] = registeredBubbles.Count - 1;
            
            // Expand arrays if needed
            if (registeredBubbles.Count > bubbleJobData.Length)
            {
                ExpandArrays();
            }
        }
        
        /// <summary>
        /// Unregister a bubble from batch processing
        /// </summary>
        public void UnregisterBubble(PooledBubble bubble)
        {
            if (bubble == null) return;
            
            int index = registeredBubbles.IndexOf(bubble);
            if (index >= 0)
            {
                registeredBubbles.RemoveAt(index);
                bubbleToJobIndex.Remove(bubble.GetId());
                
                // Update indices for remaining bubbles
                for (int i = index; i < registeredBubbles.Count; i++)
                {
                    bubbleToJobIndex[registeredBubbles[i].GetId()] = i;
                }
            }
        }
        
        /// <summary>
        /// Expand native arrays when more capacity is needed
        /// </summary>
        void ExpandArrays()
        {
            int newCapacity = bubbleJobData.Length * 2;
            
            // Create new arrays
            var newBubbleJobData = new NativeArray<BubbleJobData>(newCapacity, Allocator.Persistent);
            var newPositions = new NativeArray<float3>(newCapacity, Allocator.Persistent);
            var newVelocities = new NativeArray<float3>(newCapacity, Allocator.Persistent);
            var newScales = new NativeArray<float>(newCapacity, Allocator.Persistent);
            var newBreathingPhases = new NativeArray<float>(newCapacity, Allocator.Persistent);
            
            // Copy existing data
            NativeArray<BubbleJobData>.Copy(bubbleJobData, newBubbleJobData, bubbleJobData.Length);
            NativeArray<float3>.Copy(positions, newPositions, positions.Length);
            NativeArray<float3>.Copy(velocities, newVelocities, velocities.Length);
            NativeArray<float>.Copy(scales, newScales, scales.Length);
            NativeArray<float>.Copy(breathingPhases, newBreathingPhases, breathingPhases.Length);
            
            // Dispose old arrays
            bubbleJobData.Dispose();
            positions.Dispose();
            velocities.Dispose();
            scales.Dispose();
            breathingPhases.Dispose();
            
            // Assign new arrays
            bubbleJobData = newBubbleJobData;
            positions = newPositions;
            velocities = newVelocities;
            scales = newScales;
            breathingPhases = newBreathingPhases;
            
            Debug.Log($"Expanded batch processor arrays to capacity: {newCapacity}");
        }
        
        /// <summary>
        /// Adapt performance based on processing time
        /// </summary>
        void AdaptPerformance()
        {
            if (lastProcessingTime > maxProcessingTimeMs * 1.5f)
            {
                // Reduce batch size
                maxBatchSize = Mathf.Max(maxBatchSize - 5, 10);
                
                // Reduce jobs per frame
                jobsPerFrame = Mathf.Max(jobsPerFrame - 1, 1);
                
                Debug.Log($"Reduced performance - Batch Size: {maxBatchSize}, Jobs/Frame: {jobsPerFrame}");
            }
            else if (lastProcessingTime < maxProcessingTimeMs * 0.5f)
            {
                // Increase batch size
                maxBatchSize = Mathf.Min(maxBatchSize + 2, 100);
                
                // Increase jobs per frame
                jobsPerFrame = Mathf.Min(jobsPerFrame + 1, 10);
            }
        }
        
        /// <summary>
        /// Get batch processing statistics
        /// </summary>
        public BatchProcessingStats GetStatistics()
        {
            return new BatchProcessingStats
            {
                registeredBubbles = registeredBubbles.Count,
                processedBubblesThisFrame = processedBubblesThisFrame,
                lastProcessingTimeMs = lastProcessingTime,
                currentBatchSize = maxBatchSize,
                jobsPerFrame = jobsPerFrame,
                activeJobs = activeJobHandles.Count,
                arrayCapacity = bubbleJobData.Length
            };
        }
        
        /// <summary>
        /// Cleanup native arrays and jobs
        /// </summary>
        void CleanupBatchProcessor()
        {
            // Complete any remaining jobs
            CompleteActiveJobs();
            
            // Dispose native arrays
            if (bubbleJobData.IsCreated) bubbleJobData.Dispose();
            if (positions.IsCreated) positions.Dispose();
            if (velocities.IsCreated) velocities.Dispose();
            if (scales.IsCreated) scales.Dispose();
            if (breathingPhases.IsCreated) breathingPhases.Dispose();
            
            registeredBubbles.Clear();
            bubbleToJobIndex.Clear();
            activeJobHandles.Clear();
        }
    }
    
    /// <summary>
    /// Job data structure for bubble processing
    /// </summary>
    [System.Serializable]
    public struct BubbleJobData
    {
        public int bubbleId;
        public float mass;
        public float springConstant;
        public float dampingFactor;
        public float breathingFrequency;
        public float deltaTime;
    }
    
    /// <summary>
    /// Physics processing job for bubbles
    /// </summary>
    public struct BubblePhysicsJob : IJobParallelFor
    {
        public NativeArray<float3> positions;
        public NativeArray<float3> velocities;
        [ReadOnly] public NativeArray<BubbleJobData> jobData;
        [ReadOnly] public int batchSize;
        
        public void Execute(int index)
        {
            if (index >= batchSize) return;
            
            BubbleJobData data = jobData[index];
            float3 currentPos = positions[index];
            float3 currentVel = velocities[index];
            
            // Simple spring physics calculation
            float3 springForce = -data.springConstant * currentPos;
            float3 dampingForce = -data.dampingFactor * currentVel;
            float3 totalForce = springForce + dampingForce;
            
            // Apply physics integration
            float3 acceleration = totalForce / data.mass;
            currentVel += acceleration * data.deltaTime;
            currentPos += currentVel * data.deltaTime;
            
            // Update arrays
            positions[index] = currentPos;
            velocities[index] = currentVel;
        }
    }
    
    /// <summary>
    /// Breathing animation job for bubbles
    /// </summary>
    public struct BubbleBreathingJob : IJobParallelFor
    {
        public NativeArray<float> scales;
        public NativeArray<float> breathingPhases;
        [ReadOnly] public NativeArray<BubbleJobData> jobData;
        [ReadOnly] public int batchSize;
        
        public void Execute(int index)
        {
            if (index >= batchSize) return;
            
            BubbleJobData data = jobData[index];
            float currentPhase = breathingPhases[index];
            
            // Update breathing phase
            currentPhase += data.breathingFrequency * data.deltaTime;
            if (currentPhase > math.PI * 2f)
                currentPhase -= math.PI * 2f;
            
            // Calculate breathing scale (0.2-0.5 Hz comfortable range)
            float breathingScale = 1.0f + 0.1f * math.sin(currentPhase);
            
            // Update arrays
            scales[index] = breathingScale;
            breathingPhases[index] = currentPhase;
        }
    }
    
    /// <summary>
    /// Wave positioning job for bubbles
    /// </summary>
    public struct BubbleWaveJob : IJobParallelFor
    {
        public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<BubbleJobData> jobData;
        [ReadOnly] public int batchSize;
        [ReadOnly] public float waveTime;
        
        public void Execute(int index)
        {
            if (index >= batchSize) return;
            
            float3 currentPos = positions[index];
            
            // Apply subtle wave motion for natural movement
            float waveOffset = math.sin(waveTime * 0.5f + index * 0.1f) * 0.02f;
            currentPos.y += waveOffset;
            
            positions[index] = currentPos;
        }
    }
    
    /// <summary>
    /// Batch processing statistics
    /// </summary>
    [System.Serializable]
    public struct BatchProcessingStats
    {
        public int registeredBubbles;
        public int processedBubblesThisFrame;
        public float lastProcessingTimeMs;
        public int currentBatchSize;
        public int jobsPerFrame;
        public int activeJobs;
        public int arrayCapacity;
        
        public override string ToString()
        {
            return $"Batch Stats - Bubbles: {registeredBubbles}, Processed: {processedBubblesThisFrame}, " +
                   $"Time: {lastProcessingTimeMs:F2}ms, Batch: {currentBatchSize}, Jobs: {jobsPerFrame}";
        }
    }
}
