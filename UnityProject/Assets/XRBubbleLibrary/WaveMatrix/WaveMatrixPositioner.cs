using UnityEngine;
using Unity.Mathematics;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Core wave mathematics system for positioning word bubbles on a wave matrix
    /// Provides real-time wave calculations optimized for VR performance
    /// </summary>
    public class WaveMatrixPositioner : MonoBehaviour
    {
        [Header("Wave Matrix Parameters")]
        [SerializeField] private WaveMatrixSettings settings = WaveMatrixSettings.Default;
        
        [Header("Performance Settings")]
        [SerializeField] private int maxBubbles = 30; // Reduced for Quest 3 performance
        [SerializeField] private float updateFrequency = 15f; // Reduced from 60Hz for VR performance
        
        // Internal state
        private float currentTime = 0f;
        private float lastUpdateTime = 0f;
        private float updateInterval;
        
        // Wave calculation cache for performance
        private float3[] cachedPositions;
        private bool[] positionsDirty;
        
        void Start()
        {
            InitializeWaveMatrix();
        }
        
        void Update()
        {
            UpdateWaveTime();
            
            // Only update positions at specified frequency for VR performance
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateWavePositions();
                lastUpdateTime = Time.time;
            }
        }
        
        void InitializeWaveMatrix()
        {
            updateInterval = 1f / updateFrequency;
            cachedPositions = new float3[maxBubbles];
            positionsDirty = new bool[maxBubbles];
            
            // Initialize all positions as dirty
            for (int i = 0; i < maxBubbles; i++)
            {
                positionsDirty[i] = true;
            }
            
            Debug.Log($"Wave Matrix initialized: {maxBubbles} max bubbles, {updateFrequency}Hz update rate");
        }
        
        void UpdateWaveTime()
        {
            currentTime += Time.deltaTime * settings.timeScale;
        }
        
        void UpdateWavePositions()
        {
            for (int i = 0; i < maxBubbles; i++)
            {
                if (positionsDirty[i])
                {
                    cachedPositions[i] = CalculateWavePosition(i, currentTime);
                    positionsDirty[i] = false;
                }
            }
        }
        
        /// <summary>
        /// Calculate wave-based position for a bubble at given index and time
        /// </summary>
        public float3 CalculateWavePosition(int bubbleIndex, float time)
        {
            return CalculateWavePosition(bubbleIndex, time, 0f); // Default distance
        }
        
        /// <summary>
        /// Calculate wave-based position with AI-predicted distance
        /// </summary>
        public float3 CalculateWavePosition(int bubbleIndex, float time, float aiDistance)
        {
            // Base grid position
            float gridX = (bubbleIndex % settings.gridWidth) * settings.spacing;
            float gridZ = (bubbleIndex / settings.gridWidth) * settings.spacing;
            
            // Center the grid
            gridX -= (settings.gridWidth * settings.spacing) * 0.5f;
            gridZ -= (settings.gridWidth * settings.spacing) * 0.5f;
            
            // Apply AI distance offset (closer words have smaller Z values)
            gridZ += aiDistance * settings.aiDistanceScale;
            
            // Calculate wave height using multiple wave components
            float waveY = CalculateWaveHeight(gridX, gridZ, time);
            
            return new float3(gridX, waveY, gridZ);
        }
        
        /// <summary>
        /// Calculate wave height at given position and time using multiple wave components
        /// </summary>
        float CalculateWaveHeight(float x, float z, float time)
        {
            float height = 0f;
            
            // Primary wave (main wave pattern)
            height += math.sin(x * settings.primaryWave.frequency + time * settings.primaryWave.speed) 
                     * settings.primaryWave.amplitude;
            
            // Secondary wave (adds complexity)
            height += math.sin(z * settings.secondaryWave.frequency + time * settings.secondaryWave.speed) 
                     * settings.secondaryWave.amplitude;
            
            // Tertiary wave (fine detail)
            float radialDistance = math.sqrt(x * x + z * z);
            height += math.sin(radialDistance * settings.tertiaryWave.frequency + time * settings.tertiaryWave.speed) 
                     * settings.tertiaryWave.amplitude;
            
            // Interference pattern (creates more complex wave interactions)
            if (settings.enableInterference)
            {
                float interference = math.sin(x * settings.interferenceFreq + time) 
                                   * math.cos(z * settings.interferenceFreq + time) 
                                   * settings.interferenceAmplitude;
                height += interference;
            }
            
            return height;
        }
        
        /// <summary>
        /// Get cached position for bubble (performance optimized)
        /// </summary>
        public float3 GetBubblePosition(int bubbleIndex)
        {
            if (bubbleIndex < 0 || bubbleIndex >= maxBubbles)
            {
                Debug.LogWarning($"Bubble index {bubbleIndex} out of range [0, {maxBubbles}]");
                return float3.zero;
            }
            
            return cachedPositions[bubbleIndex];
        }
        
        /// <summary>
        /// Mark bubble position as needing recalculation
        /// </summary>
        public void MarkPositionDirty(int bubbleIndex)
        {
            if (bubbleIndex >= 0 && bubbleIndex < maxBubbles)
            {
                positionsDirty[bubbleIndex] = true;
            }
        }
        
        /// <summary>
        /// Get real-time position (bypasses cache, use sparingly)
        /// </summary>
        public float3 GetRealTimePosition(int bubbleIndex, float aiDistance = 0f)
        {
            return CalculateWavePosition(bubbleIndex, currentTime, aiDistance);
        }
        
        /// <summary>
        /// Update wave settings at runtime
        /// </summary>
        public void UpdateWaveSettings(WaveMatrixSettings newSettings)
        {
            settings = newSettings;
            
            // Mark all positions as dirty to recalculate with new settings
            for (int i = 0; i < maxBubbles; i++)
            {
                positionsDirty[i] = true;
            }
        }
        
        // Debug visualization
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            Gizmos.color = Color.cyan;
            
            // Draw wave matrix grid
            for (int i = 0; i < maxBubbles; i++)
            {
                Vector3 pos = GetBubblePosition(i);
                Gizmos.DrawWireSphere(transform.position + pos, 0.1f);
            }
            
            // Draw center reference
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
        }
    }
}