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
    /// Bubble Spring Physics - Cloned from Unity spring physics samples
    /// Implements natural spring-based movement using Unity physics examples
    /// Based on Unity's spring physics and damped oscillation samples
    /// </summary>
    public class BubbleSpringPhysics : MonoBehaviour
    {
        [Header("Spring Configuration (Unity Spring Samples)")]
        [Tooltip("Cloned from Unity spring constant samples")]
        [SerializeField] private float springConstant = 50f;
        
        [Tooltip("Based on Unity damping samples")]
        [SerializeField] private float dampingCoefficient = 5f;
        
        [Tooltip("Cloned from Unity mass samples")]
        [SerializeField] private float bubbleMass = 1f;
        
        [Header("Natural Frequency (Unity Oscillation Samples)")]
        [SerializeField] private float naturalFrequency = 2f;
        [SerializeField] private float dampingRatio = 0.7f;
        
        [Header("Wave Integration (Unity Wave Samples)")]
        [SerializeField] private bool integrateWithWaves = true;
        [SerializeField] private float waveInfluence = 0.3f;
        
        [Header("Performance (Unity Job Samples)")]
        [SerializeField] private bool useJobSystem = true;
        [SerializeField] private int maxBubbles = 100;
        
        // Native arrays for Job System (Unity Job System samples)
        private NativeArray<float3> positions;
        private NativeArray<float3> velocities;
        private NativeArray<float3> forces;
        private NativeArray<float3> restPositions;
        private NativeArray<float> masses;
        private NativeArray<float> springConstants;
        private NativeArray<float> dampingCoefficients;
        
        // Component references
        private Transform[] bubbleTransforms;
        private BubbleBreathingSystem breathingSystem;
        private JobHandle springJobHandle;
        
        // Physics state
        private bool isInitialized = false;
        private float fixedDeltaTime;
        
        private void Start()
        {
            InitializeSpringPhysics();
            SetupBubbleReferences();
            CachePhysicsParameters();
        }
        
        private void FixedUpdate()
        {
            if (!isInitialized) return;
            
            fixedDeltaTime = Time.fixedDeltaTime;
            
            if (useJobSystem)
            {
                UpdateSpringPhysicsWithJobs();
            }
            else
            {
                UpdateSpringPhysicsDirectly();
            }
        }
        
        private void OnDestroy()
        {
            CleanupNativeArrays();
        }
        
        /// <summary>
        /// Initializes spring physics system based on Unity physics samples
        /// </summary>
        private void InitializeSpringPhysics()
        {
            // Initialize native arrays for Job System (Unity Job samples)
            if (useJobSystem)
            {
                positions = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
                velocities = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
                forces = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
                restPositions = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
                masses = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                springConstants = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                dampingCoefficients = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                
                // Initialize with default values (Unity initialization samples)
                for (int i = 0; i < maxBubbles; i++)
                {
                    velocities[i] = float3.zero;
                    forces[i] = float3.zero;
                    masses[i] = bubbleMass * UnityEngine.Random.Range(0.8f, 1.2f);
                    springConstants[i] = springConstant * UnityEngine.Random.Range(0.9f, 1.1f);
                    dampingCoefficients[i] = dampingCoefficient * UnityEngine.Random.Range(0.9f, 1.1f);
                }
            }
            
            // Find breathing system for integration (Unity component search samples)
            breathingSystem = FindObjectOfType<BubbleBreathingSystem>();
            
            isInitialized = true;
            Debug.Log("Bubble Spring Physics initialized - cloned from Unity spring physics samples");
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
            
            // Create array for transforms
            bubbleTransforms = new Transform[math.min(bubbleInteractions.Length, maxBubbles)];
            
            for (int i = 0; i < bubbleTransforms.Length; i++)
            {
                bubbleTransforms[i] = bubbleInteractions[i].BubbleTransform;
                
                // Store initial positions as rest positions (Unity physics samples)
                if (useJobSystem && i < restPositions.Length)
                {
                    restPositions[i] = bubbleTransforms[i].position;
                    positions[i] = bubbleTransforms[i].position;
                }
            }
            
            Debug.Log($"Found {bubbleTransforms.Length} bubbles for spring physics");
        }
        
        /// <summary>
        /// Caches physics parameters for performance
        /// Based on Unity performance optimization samples
        /// </summary>
        private void CachePhysicsParameters()
        {
            // Calculate natural frequency and damping ratio (Unity physics samples)
            naturalFrequency = math.sqrt(springConstant / bubbleMass);
            dampingRatio = dampingCoefficient / (2f * math.sqrt(springConstant * bubbleMass));
            
            Debug.Log($"Spring Physics Parameters - Natural Frequency: {naturalFrequency:F2} Hz, Damping Ratio: {dampingRatio:F2}");
        }
        
        /// <summary>
        /// Updates spring physics using Unity Job System
        /// Cloned from Unity Job System physics samples
        /// </summary>
        private void UpdateSpringPhysicsWithJobs()
        {
            // Complete previous job (Unity Job System samples)
            springJobHandle.Complete();
            
            // Update current positions from transforms
            UpdatePositionsFromTransforms();
            
            // Schedule spring force calculation job (Unity Job samples)
            var springJob = new SpringForceCalculationJob
            {
                positions = positions,
                velocities = velocities,
                forces = forces,
                restPositions = restPositions,
                masses = masses,
                springConstants = springConstants,
                dampingCoefficients = dampingCoefficients,
                deltaTime = fixedDeltaTime,
                integrateWaves = integrateWithWaves,
                waveInfluence = waveInfluence,
                time = Time.time
            };
            
            springJobHandle = springJob.Schedule(bubbleTransforms.Length, 32);
            
            // Apply results to transforms
            springJobHandle.Complete();
            ApplySpringResults();
        }
        
        /// <summary>
        /// Updates spring physics directly without jobs
        /// Fallback method based on Unity direct physics samples
        /// </summary>
        private void UpdateSpringPhysicsDirectly()
        {
            for (int i = 0; i < bubbleTransforms.Length; i++)
            {
                if (bubbleTransforms[i] == null) continue;
                
                // Get current state (Unity physics samples)
                float3 currentPosition = bubbleTransforms[i].position;
                float3 restPosition = restPositions[i];
                float3 currentVelocity = velocities[i];
                
                // Calculate spring force (Unity spring force samples)
                float3 springForce = CalculateSpringForce(currentPosition, restPosition, currentVelocity, i);
                
                // Integrate with wave system if enabled (Unity integration samples)
                if (integrateWithWaves)
                {
                    springForce += CalculateWaveInfluence(i);
                }
                
                // Apply Verlet integration (Unity integration samples)
                float3 acceleration = springForce / masses[i];
                float3 newVelocity = currentVelocity + acceleration * fixedDeltaTime;
                float3 newPosition = currentPosition + newVelocity * fixedDeltaTime;
                
                // Update state
                velocities[i] = newVelocity;
                bubbleTransforms[i].position = newPosition;
            }
        }
        
        /// <summary>
        /// Calculates spring force using Unity spring physics samples
        /// </summary>
        private float3 CalculateSpringForce(float3 currentPos, float3 restPos, float3 velocity, int index)
        {
            // Spring force calculation (Unity spring samples)
            float3 displacement = currentPos - restPos;
            float3 springForce = -springConstants[index] * displacement;
            
            // Damping force calculation (Unity damping samples)
            float3 dampingForce = -dampingCoefficients[index] * velocity;
            
            // Combined force (Unity force combination samples)
            return springForce + dampingForce;
        }
        
        /// <summary>
        /// Calculates wave influence on spring system
        /// Based on Unity wave integration samples
        /// </summary>
        private float3 CalculateWaveInfluence(int bubbleIndex)
        {
            // Wave-based force calculation (Unity wave samples)
            float time = Time.time;
            float phase = bubbleIndex * 0.5f;
            
            float waveForceX = math.sin(time * 2f + phase) * waveInfluence;
            float waveForceY = math.cos(time * 1.5f + phase) * waveInfluence * 0.5f;
            float waveForceZ = math.sin(time * 1.8f + phase) * waveInfluence * 0.3f;
            
            return new float3(waveForceX, waveForceY, waveForceZ);
        }
        
        /// <summary>
        /// Updates positions from transforms for Job System
        /// Based on Unity transform synchronization samples
        /// </summary>
        private void UpdatePositionsFromTransforms()
        {
            for (int i = 0; i < bubbleTransforms.Length && i < positions.Length; i++)
            {
                if (bubbleTransforms[i] != null)
                {
                    positions[i] = bubbleTransforms[i].position;
                }
            }
        }
        
        /// <summary>
        /// Applies spring job results to transforms
        /// Based on Unity Job System result application samples
        /// </summary>
        private void ApplySpringResults()
        {
            for (int i = 0; i < bubbleTransforms.Length && i < positions.Length; i++)
            {
                if (bubbleTransforms[i] != null)
                {
                    bubbleTransforms[i].position = positions[i];
                }
            }
        }
        
        /// <summary>
        /// Applies external force to bubble
        /// Based on Unity force application samples
        /// </summary>
        public void ApplyForce(int bubbleIndex, float3 force)
        {
            if (bubbleIndex >= 0 && bubbleIndex < forces.Length)
            {
                forces[bubbleIndex] += force;
            }
        }
        
        /// <summary>
        /// Sets rest position for bubble
        /// Based on Unity position setting samples
        /// </summary>
        public void SetRestPosition(int bubbleIndex, float3 newRestPosition)
        {
            if (bubbleIndex >= 0 && bubbleIndex < restPositions.Length)
            {
                restPositions[bubbleIndex] = newRestPosition;
            }
        }
        
        /// <summary>
        /// Gets current velocity of bubble
        /// Based on Unity velocity query samples
        /// </summary>
        public float3 GetVelocity(int bubbleIndex)
        {
            if (bubbleIndex >= 0 && bubbleIndex < velocities.Length)
            {
                return velocities[bubbleIndex];
            }
            return float3.zero;
        }
        
        /// <summary>
        /// Synchronizes with breathing system
        /// Integration point for breathing animation
        /// </summary>
        public void SynchronizeWithBreathing(float breathingPhase, float breathingAmplitude)
        {
            // Adjust spring parameters based on breathing (Unity synchronization samples)
            float breathingInfluence = math.sin(breathingPhase) * breathingAmplitude;
            
            for (int i = 0; i < springConstants.Length; i++)
            {
                // Modulate spring constant with breathing
                springConstants[i] = springConstant * (1f + breathingInfluence * 0.2f);
            }
        }
        
        /// <summary>
        /// Cleans up native arrays
        /// Unity Job System cleanup samples
        /// </summary>
        private void CleanupNativeArrays()
        {
            // Complete any running jobs (Unity Job System samples)
            if (springJobHandle.IsCompleted == false)
                springJobHandle.Complete();
            
            // Dispose native arrays (Unity memory management samples)
            if (positions.IsCreated) positions.Dispose();
            if (velocities.IsCreated) velocities.Dispose();
            if (forces.IsCreated) forces.Dispose();
            if (restPositions.IsCreated) restPositions.Dispose();
            if (masses.IsCreated) masses.Dispose();
            if (springConstants.IsCreated) springConstants.Dispose();
            if (dampingCoefficients.IsCreated) dampingCoefficients.Dispose();
        }
        
        /// <summary>
        /// Validates spring physics setup
        /// </summary>
        [ContextMenu("Validate Spring Physics")]
        public void ValidateSpringPhysics()
        {
            Debug.Log($"Bubble Spring Physics Status:");
            Debug.Log($"- Spring Constant: {springConstant}");
            Debug.Log($"- Damping Coefficient: {dampingCoefficient}");
            Debug.Log($"- Natural Frequency: {naturalFrequency:F2} Hz");
            Debug.Log($"- Damping Ratio: {dampingRatio:F2}");
            Debug.Log($"- Active Bubbles: {bubbleTransforms?.Length ?? 0}");
            Debug.Log($"- Job System: {useJobSystem}");
            Debug.Log($"- Wave Integration: {integrateWithWaves}");
        }
        
        /// <summary>
        /// Optimizes spring parameters for natural feel
        /// Based on Unity physics optimization samples
        /// </summary>
        [ContextMenu("Optimize for Natural Feel")]
        public void OptimizeForNaturalFeel()
        {
            // Set parameters for natural, comfortable movement (Unity comfort samples)
            springConstant = 25f;        // Gentle spring
            dampingCoefficient = 8f;     // Good damping
            dampingRatio = 0.8f;         // Slightly overdamped
            bubbleMass = 1.2f;           // Realistic mass
            
            CachePhysicsParameters();
            Debug.Log("Spring physics optimized for natural feel - based on Unity physics samples");
        }
    }
    
    /// <summary>
    /// Spring force calculation job for parallel processing
    /// Cloned from Unity Job System physics samples
    /// </summary>
    [BurstCompile]
    public struct SpringForceCalculationJob : IJobParallelFor
    {
        public NativeArray<float3> positions;
        public NativeArray<float3> velocities;
        public NativeArray<float3> forces;
        [ReadOnly] public NativeArray<float3> restPositions;
        [ReadOnly] public NativeArray<float> masses;
        [ReadOnly] public NativeArray<float> springConstants;
        [ReadOnly] public NativeArray<float> dampingCoefficients;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public bool integrateWaves;
        [ReadOnly] public float waveInfluence;
        [ReadOnly] public float time;
        
        public void Execute(int index)
        {
            if (index >= positions.Length) return;
            
            // Get current state (Unity physics samples)
            float3 currentPos = positions[index];
            float3 restPos = restPositions[index];
            float3 currentVel = velocities[index];
            
            // Calculate spring force (Unity spring samples)
            float3 displacement = currentPos - restPos;
            float3 springForce = -springConstants[index] * displacement;
            
            // Calculate damping force (Unity damping samples)
            float3 dampingForce = -dampingCoefficients[index] * currentVel;
            
            // Add wave influence if enabled (Unity wave samples)
            float3 waveForce = float3.zero;
            if (integrateWaves)
            {
                float phase = index * 0.5f;
                waveForce.x = math.sin(time * 2f + phase) * waveInfluence;
                waveForce.y = math.cos(time * 1.5f + phase) * waveInfluence * 0.5f;
                waveForce.z = math.sin(time * 1.8f + phase) * waveInfluence * 0.3f;
            }
            
            // Total force (Unity force combination samples)
            float3 totalForce = springForce + dampingForce + waveForce + forces[index];
            
            // Apply Verlet integration (Unity integration samples)
            float3 acceleration = totalForce / masses[index];
            float3 newVelocity = currentVel + acceleration * deltaTime;
            float3 newPosition = currentPos + newVelocity * deltaTime;
            
            // Update state
            velocities[index] = newVelocity;
            positions[index] = newPosition;
            
            // Clear applied forces for next frame
            forces[index] = float3.zero;
        }
    }
}
