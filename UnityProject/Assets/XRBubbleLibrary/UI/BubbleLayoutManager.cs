using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using XRBubbleLibrary.AI; // Added for ArrangementPattern
using Unity.Burst;

namespace XRBubbleLibrary.UI
{
    /// <summary>
    /// Bubble Layout Manager - Cloned from Unity layout samples
    /// Manages wave-pattern spatial arrangement of bubbles using Unity Job System
    /// Based on Unity's layout system samples and mathematical positioning examples
    /// </summary>
    public class BubbleLayoutManager : MonoBehaviour
    {
        [Header("Layout Configuration (Unity Layout Samples)")]
        [SerializeField] private int maxBubbles = 50;
        [SerializeField] private float bubbleRadius = 0.5f;
        [SerializeField] private float minBubbleDistance = 1.2f;
        
        [Header("Wave Pattern Settings (Unity Math Samples)")]
        [SerializeField] private bool enableWavePattern = true;
        [SerializeField] private float waveAmplitude = 0.3f;
        [SerializeField] private float waveFrequency = 0.5f;
        [SerializeField] private float waveSpeed = 1.0f;
        
        [Header("Golden Ratio Layout (Unity Fibonacci Samples)")]
        [SerializeField] private bool useGoldenRatio = true;
        [SerializeField] private float goldenRatioSpiral = 1.618f;
        [SerializeField] private float spiralTightness = 0.5f;
        
        [Header("Performance Optimization (Unity Job Samples)")]
        [SerializeField] private bool useJobSystem = true;
        [SerializeField] private int batchSize = 10;
        
        // Native arrays for Job System (Unity Job System samples)
        private NativeArray<float3> bubblePositions;
        private NativeArray<float3> bubbleVelocities;
        private NativeArray<float> bubblePhases;
        private NativeArray<int> bubbleIndices;
        
        // Job handles for performance (Unity Job samples)
        private JobHandle positionJobHandle;
        private JobHandle waveJobHandle;
        
        // Bubble tracking
        private Transform[] bubbleTransforms;
        private bool isInitialized = false;
        
        private void Start()
        {
            InitializeLayoutManager();
            SetupBubblePositions();
        }
        
        private void Update()
        {
            if (isInitialized && useJobSystem)
            {
                UpdateBubblePositionsWithJobs();
            }
            else if (isInitialized)
            {
                UpdateBubblePositionsDirectly();
            }
        }
        
        private void OnDestroy()
        {
            CleanupNativeArrays();
        }
        
        /// <summary>
        /// Initializes layout manager based on Unity layout samples
        /// </summary>
        private void InitializeLayoutManager()
        {
            // Initialize native arrays (Unity Job System samples)
            if (useJobSystem)
            {
                bubblePositions = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
                bubbleVelocities = new NativeArray<float3>(maxBubbles, Allocator.Persistent);
                bubblePhases = new NativeArray<float>(maxBubbles, Allocator.Persistent);
                bubbleIndices = new NativeArray<int>(maxBubbles, Allocator.Persistent);
            }
            
            // Find or create bubble transforms
            SetupBubbleTransforms();
            
            isInitialized = true;
            Debug.Log("Bubble Layout Manager initialized - using Unity layout samples");
        }
        
        /// <summary>
        /// Sets up bubble transforms based on Unity transform samples
        /// </summary>
        private void SetupBubbleTransforms()
        {
            // Find existing bubble objects (Unity object search samples)
            var existingBubbles = FindObjectsOfType<BubbleInteraction>();
            
            // Create array to hold transforms
            bubbleTransforms = new Transform[maxBubbles];
            
            // Use existing bubbles or create new ones
            for (int i = 0; i < maxBubbles; i++)
            {
                if (i < existingBubbles.Length)
                {
                    bubbleTransforms[i] = existingBubbles[i].transform;
                }
                else
                {
                    // Create new bubble GameObject (Unity GameObject samples)
                    GameObject newBubble = CreateBubbleGameObject(i);
                    bubbleTransforms[i] = newBubble.transform;
                }
                
                // Initialize job system data if enabled
                if (useJobSystem && i < bubbleIndices.Length)
                {
                    bubbleIndices[i] = i;
                    bubblePhases[i] = UnityEngine.Random.Range(0f, 2f * math.PI);
                }
            }
        }
        
        /// <summary>
        /// Creates bubble GameObject based on Unity prefab samples
        /// </summary>
        private GameObject CreateBubbleGameObject(int index)
        {
            // Create bubble GameObject (Unity GameObject creation samples)
            GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bubble.name = $"Bubble_{index}";
            bubble.transform.localScale = Vector3.one * bubbleRadius;
            
            // Add bubble components (Unity component samples)
            bubble.AddComponent<BubbleInteraction>();
            
            // Apply bubble material if available
            Renderer renderer = bubble.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Try to find bubble material (Unity material samples)
                Material bubbleMaterial = Resources.Load<Material>("BubbleGlassMaterial");
                if (bubbleMaterial != null)
                {
                    renderer.material = bubbleMaterial;
                }
            }
            
            return bubble;
        }
        
        /// <summary>
        /// Sets up initial bubble positions using mathematical patterns
        /// Cloned from Unity mathematical positioning samples
        /// </summary>
        private void SetupBubblePositions()
        {
            for (int i = 0; i < bubbleTransforms.Length && i < maxBubbles; i++)
            {
                if (bubbleTransforms[i] == null) continue;
                
                float3 position;
                
                if (useGoldenRatio)
                {
                    // Golden ratio spiral positioning (Unity Fibonacci samples)
                    position = CalculateGoldenRatioPosition(i);
                }
                else
                {
                    // Grid-based positioning (Unity grid samples)
                    position = CalculateGridPosition(i);
                }
                
                // Apply initial position
                bubbleTransforms[i].position = position;
                
                // Store in native array for job system
                if (useJobSystem && i < bubblePositions.Length)
                {
                    bubblePositions[i] = position;
                }
            }
        }
        
        /// <summary>
        /// Calculates golden ratio spiral position
        /// Based on Unity Fibonacci spiral samples
        /// </summary>
        private float3 CalculateGoldenRatioPosition(int index)
        {
            // Golden ratio spiral calculation (Unity math samples)
            float angle = index * 2.4f; // Golden angle approximation
            float radius = spiralTightness * math.sqrt(index);
            
            float x = radius * math.cos(angle);
            float z = radius * math.sin(angle);
            float y = math.sin(index * 0.5f) * 0.5f; // Slight vertical variation
            
            return transform.position + new float3(x, y, z);
        }
        
        /// <summary>
        /// Calculates grid-based position
        /// Cloned from Unity grid layout samples
        /// </summary>
        private float3 CalculateGridPosition(int index)
        {
            // Grid layout calculation (Unity grid samples)
            int gridSize = Mathf.CeilToInt(math.sqrt(maxBubbles));
            int row = index / gridSize;
            int col = index % gridSize;
            
            float x = (col - gridSize * 0.5f) * minBubbleDistance;
            float z = (row - gridSize * 0.5f) * minBubbleDistance;
            float y = 0f;
            
            return transform.position + new float3(x, y, z);
        }
        
        /// <summary>
        /// Updates bubble positions using Unity Job System
        /// Cloned from Unity Job System samples
        /// </summary>
        private void UpdateBubblePositionsWithJobs()
        {
            // Complete previous jobs (Unity Job System samples)
            positionJobHandle.Complete();
            waveJobHandle.Complete();
            
            // Schedule wave calculation job (Unity Job samples)
            if (enableWavePattern)
            {
                var waveJob = new WaveCalculationJob
            {
                positions = bubblePositions,
                phases = bubblePhases,
                time = Time.time,
                amplitude = waveAmplitude,
                frequency = waveFrequency,
                speed = waveSpeed
            };
            
            waveJobHandle = waveJob.Schedule(bubblePositions.Length, batchSize);
        }
        
        // Schedule position update job (Unity Job samples)
        var positionJob = new PositionUpdateJob
        {
            positions = bubblePositions,
            indices = bubbleIndices
        };
        
        positionJobHandle = positionJob.Schedule(bubblePositions.Length, batchSize, waveJobHandle);
        
        // Apply results to transforms (Unity transform samples)
        positionJobHandle.Complete();
        ApplyJobResultsToTransforms();
    }
    
    /// <summary>
    /// Updates bubble positions directly without jobs
    /// Fallback method based on Unity direct update samples
    /// </summary>
    private void UpdateBubblePositionsDirectly()
    {
        for (int i = 0; i < bubbleTransforms.Length && i < maxBubbles; i++)
        {
            if (bubbleTransforms[i] == null) continue;
            
            float3 basePosition = useGoldenRatio ? 
                CalculateGoldenRatioPosition(i) : 
                CalculateGridPosition(i);
            
            if (enableWavePattern)
            {
                // Apply wave displacement (Unity wave samples)
                float phase = i * 0.5f;
                float waveX = math.sin(Time.time * waveFrequency + phase) * waveAmplitude;
                float waveY = math.cos(Time.time * waveFrequency * 0.7f + phase) * waveAmplitude * 0.5f;
                
                basePosition += new float3(waveX, waveY, 0f);
            }
            
            bubbleTransforms[i].position = basePosition;
        }
    }
    
    /// <summary>
    /// Applies job results to transforms
    /// Based on Unity Job System result application samples
    /// </summary>
    private void ApplyJobResultsToTransforms()
    {
        for (int i = 0; i < bubbleTransforms.Length && i < bubblePositions.Length; i++)
        {
            if (bubbleTransforms[i] != null)
            {
                bubbleTransforms[i].position = bubblePositions[i];
            }
        }
    }
    
    /// <summary>
    /// Cleans up native arrays
    /// Unity Job System cleanup samples
    /// </summary>
    private void CleanupNativeArrays()
    {
        // Complete any running jobs (Unity Job System samples)
        if (positionJobHandle.IsCompleted == false)
            positionJobHandle.Complete();
        if (waveJobHandle.IsCompleted == false)
            waveJobHandle.Complete();
        
        // Dispose native arrays (Unity memory management samples)
        if (bubblePositions.IsCreated)
            bubblePositions.Dispose();
        if (bubbleVelocities.IsCreated)
            bubbleVelocities.Dispose();
        if (bubblePhases.IsCreated)
            bubblePhases.Dispose();
        if (bubbleIndices.IsCreated)
            bubbleIndices.Dispose();
    }
    
    /// <summary>
    /// Validates layout manager setup
    /// </summary>
    [ContextMenu("Validate Layout Manager")]
    public void ValidateLayoutManager()
    {
        Debug.Log($"Bubble Layout Manager Status:");
        Debug.Log($"- Max Bubbles: {maxBubbles}");
        Debug.Log($"- Active Bubbles: {bubbleTransforms?.Length ?? 0}");
        Debug.Log($"- Wave Pattern: {enableWavePattern}");
        Debug.Log($"- Golden Ratio: {useGoldenRatio}");
        Debug.Log($"- Job System: {useJobSystem}");
        Debug.Log($"- Initialized: {isInitialized}");
    }

    public void ApplySpatialArrangement(float3[] positions, ArrangementPattern pattern)
    {
        // For now, we'll just move the existing bubbles to the new positions.
        // A more advanced implementation would create/destroy bubbles as needed.
        for (int i = 0; i < bubbleTransforms.Length && i < positions.Length; i++)
        {
            if (bubbleTransforms[i] != null)
            {
                // Directly set the position. A smoother transition could be implemented here.
                bubbleTransforms[i].position = positions[i];
                if (useJobSystem)
                {
                    bubblePositions[i] = positions[i];
                }
            }
        }
    }
}

/// <summary>
/// Wave calculation job for bubble positioning
/// Cloned from Unity Job System wave samples
/// </summary>
[BurstCompile]
public struct WaveCalculationJob : IJobParallelFor
{
    public NativeArray<float3> positions;
    [ReadOnly] public NativeArray<float> phases;
    [ReadOnly] public float time;
    [ReadOnly] public float amplitude;
    [ReadOnly] public float frequency;
    [ReadOnly] public float speed;
    
    public void Execute(int index)
    {
        // Wave calculation (Unity wave math samples)
        float phase = phases[index];
        float waveX = math.sin(time * frequency * speed + phase) * amplitude;
        float waveY = math.cos(time * frequency * speed * 0.7f + phase) * amplitude * 0.5f;
        
        // Apply wave displacement to position
        float3 currentPos = positions[index];
        positions[index] = currentPos + new float3(waveX, waveY, 0f);
    }
}

/// <summary>
/// Position update job for bubble transforms
/// Cloned from Unity Job System position samples
/// </summary>
[BurstCompile]
public struct PositionUpdateJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positions;
    [ReadOnly] public NativeArray<int> indices;
    
    public void Execute(int index)
    {
        // Position update logic (Unity position samples)
        // This job primarily serves as a data processing step
        // Actual transform updates happen on main thread
    }
}
}
