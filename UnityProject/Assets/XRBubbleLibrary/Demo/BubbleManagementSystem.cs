using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Diagnostics;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.Demo
{
    /// <summary>
    /// Manages exactly 100 bubbles for the demo scene with efficient pooling and performance optimization.
    /// Implements Requirement 5.1: Demonstrate wave mathematics with exactly 100 bubbles.
    /// Implements Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public class BubbleManagementSystem : IBubbleManagementSystem
    {
        // Constants
        private const int REQUIRED_BUBBLE_COUNT = 100;
        private const float BUBBLE_UPDATE_BUDGET_MS = 1.0f; // 1ms budget for bubble updates
        
        // Core properties
        public int ActiveBubbleCount => _activeBubbles.Count;
        public int TargetBubbleCount { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsRunning { get; private set; }
        
        // Dependencies
        private readonly IPerformanceOptimizedWaveSystem _waveSystem;
        private readonly IBubblePositionCalculator _positionCalculator;
        
        // Scene references
        private Transform _container;
        private GameObject _bubblePrefab;
        
        // Bubble management
        private readonly List<BubbleInstance> _activeBubbles = new List<BubbleInstance>();
        private readonly Queue<BubbleInstance> _bubblePool = new Queue<BubbleInstance>();
        private BubbleData[] _bubbleDataArray;
        private float3[] _positionResults;
        
        // Configuration
        private BubbleVisualConfig _visualConfig = BubbleVisualConfig.Quest3Default;
        private bool _poolingEnabled = true;
        
        // Performance tracking
        private BubbleManagementStats _stats;
        private readonly Stopwatch _performanceTimer = new Stopwatch();
        
        // Wave integration
        private WaveMatrixSettings _waveSettings;
        private float _currentTime;
        
        /// <summary>
        /// Initialize the bubble management system.
        /// </summary>
        public BubbleManagementSystem(int targetCount, Transform container, IPerformanceOptimizedWaveSystem waveSystem)
        {
            TargetBubbleCount = targetCount;
            _container = container;
            _waveSystem = waveSystem;
            _positionCalculator = new BubblePositionCalculator();
            
            if (targetCount != REQUIRED_BUBBLE_COUNT)
            {
                UnityEngine.Debug.LogWarning($"[BubbleManagementSystem] Target count {targetCount} != required {REQUIRED_BUBBLE_COUNT}");
            }
            
            Initialize(targetCount, container);
        }
        
        /// <summary>
        /// Initialize the bubble management system.
        /// </summary>
        public bool Initialize(int targetCount, Transform container)
        {
            try
            {
                TargetBubbleCount = targetCount;
                _container = container;
                
                // Create bubble prefab
                CreateBubblePrefab();
                
                // Initialize data arrays
                _bubbleDataArray = new BubbleData[targetCount];
                _positionResults = new float3[targetCount];
                
                // Create initial bubbles
                CreateInitialBubbles();
                
                // Initialize wave settings
                _waveSettings = WaveMatrixSettings.Default;
                
                // Initialize stats
                _stats = new BubbleManagementStats
                {
                    PoolingEnabled = _poolingEnabled,
                    LastUpdateTime = Time.time
                };
                
                IsInitialized = true;
                
                UnityEngine.Debug.Log($"[BubbleManagementSystem] Initialized with {ActiveBubbleCount} bubbles");
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[BubbleManagementSystem] Initialization failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Start the bubble demo with wave animations.
        /// </summary>
        public void StartDemo()
        {
            if (!IsInitialized)
            {
                UnityEngine.Debug.LogError("[BubbleManagementSystem] Cannot start demo - not initialized");
                return;
            }
            
            IsRunning = true;
            _currentTime = 0f;
            
            // Ensure we have exactly the target number of bubbles
            EnsureCorrectBubbleCount();
            
            UnityEngine.Debug.Log("[BubbleManagementSystem] Demo started");
        }
        
        /// <summary>
        /// Stop the bubble demo.
        /// </summary>
        public void StopDemo()
        {
            IsRunning = false;
            UnityEngine.Debug.Log("[BubbleManagementSystem] Demo stopped");
        }
        
        /// <summary>
        /// Reset all bubbles to initial positions.
        /// </summary>
        public void ResetBubbles()
        {
            _currentTime = 0f;
            
            // Reset all bubble positions to grid layout
            for (int i = 0; i < _activeBubbles.Count; i++)
            {
                var bubble = _activeBubbles[i];
                var gridPos = CalculateGridPosition(i);
                bubble.Transform.position = gridPos;
                bubble.Data.Position = gridPos;
                bubble.Data.IsDirty = true;
            }
            
            UnityEngine.Debug.Log("[BubbleManagementSystem] Bubbles reset to initial positions");
        }
        
        /// <summary>
        /// Update bubble positions based on wave mathematics.
        /// </summary>
        public void UpdateBubblePositions(float deltaTime)
        {
            if (!IsRunning || _activeBubbles.Count == 0) return;
            
            _performanceTimer.Restart();
            
            try
            {
                _currentTime += deltaTime;
                
                // Update bubble data array
                for (int i = 0; i < _activeBubbles.Count; i++)
                {
                    _bubbleDataArray[i] = _activeBubbles[i].Data;
                }
                
                // Calculate new positions using wave mathematics
                _positionCalculator.CalculateAllPositions(_bubbleDataArray, _currentTime, _waveSettings, _positionResults);
                
                // Apply positions to bubble transforms
                for (int i = 0; i < _activeBubbles.Count; i++)
                {
                    var bubble = _activeBubbles[i];
                    var newPosition = _positionResults[i];
                    
                    // Smooth interpolation for visual quality
                    bubble.Transform.position = Vector3.Lerp(bubble.Transform.position, newPosition, deltaTime * 10f);
                    
                    // Update bubble data
                    bubble.Data.Position = newPosition;
                    bubble.Data.IsDirty = false;
                    bubble.Data.LastUpdateTime = _currentTime;
                    
                    // Apply visual effects
                    ApplyVisualEffects(bubble, deltaTime);
                }
                
                _stats.TotalUpdates++;
            }
            finally
            {
                _performanceTimer.Stop();
                UpdatePerformanceStats(_performanceTimer.ElapsedTicks);
            }
        }
        
        /// <summary>
        /// Validate that exactly the target number of bubbles are active.
        /// </summary>
        public BubbleCountValidationResult ValidateBubbleCount()
        {
            int actualCount = ActiveBubbleCount;
            int expectedCount = TargetBubbleCount;
            
            if (actualCount == expectedCount)
            {
                return BubbleCountValidationResult.Success(actualCount);
            }
            else
            {
                return BubbleCountValidationResult.Failure(expectedCount, actualCount);
            }
        }
        
        /// <summary>
        /// Get performance statistics for the bubble management system.
        /// </summary>
        public BubbleManagementStats GetPerformanceStats()
        {
            _stats.LastUpdateTime = Time.time;
            return _stats;
        }
        
        /// <summary>
        /// Get all active bubble transforms for external processing.
        /// </summary>
        public Transform[] GetActiveBubbleTransforms()
        {
            var transforms = new Transform[_activeBubbles.Count];
            for (int i = 0; i < _activeBubbles.Count; i++)
            {
                transforms[i] = _activeBubbles[i].Transform;
            }
            return transforms;
        }
        
        /// <summary>
        /// Get bubble data for wave mathematics calculations.
        /// </summary>
        public BubbleData[] GetBubbleData()
        {
            var data = new BubbleData[_activeBubbles.Count];
            for (int i = 0; i < _activeBubbles.Count; i++)
            {
                data[i] = _activeBubbles[i].Data;
            }
            return data;
        }
        
        /// <summary>
        /// Set bubble positions from wave mathematics results.
        /// </summary>
        public void SetBubblePositions(float3[] positions)
        {
            if (positions == null || positions.Length != _activeBubbles.Count)
            {
                UnityEngine.Debug.LogError("[BubbleManagementSystem] Position array length mismatch");
                return;
            }
            
            for (int i = 0; i < _activeBubbles.Count; i++)
            {
                _activeBubbles[i].Transform.position = positions[i];
                _activeBubbles[i].Data.Position = positions[i];
            }
        }
        
        /// <summary>
        /// Enable or disable bubble pooling for performance optimization.
        /// </summary>
        public void SetPoolingEnabled(bool enabled)
        {
            _poolingEnabled = enabled;
            _stats.PoolingEnabled = enabled;
            
            UnityEngine.Debug.Log($"[BubbleManagementSystem] Pooling {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Configure bubble visual properties.
        /// </summary>
        public void ConfigureBubbleVisuals(BubbleVisualConfig config)
        {
            _visualConfig = config;
            
            // Apply configuration to existing bubbles
            foreach (var bubble in _activeBubbles)
            {
                ApplyVisualConfiguration(bubble);
            }
            
            UnityEngine.Debug.Log("[BubbleManagementSystem] Visual configuration updated");
        }
        
        /// <summary>
        /// Clean up resources.
        /// </summary>
        public void Dispose()
        {
            IsRunning = false;
            
            // Destroy all active bubbles
            foreach (var bubble in _activeBubbles)
            {
                if (bubble.GameObject != null)
                {
                    Object.DestroyImmediate(bubble.GameObject);
                }
            }
            _activeBubbles.Clear();
            
            // Clear pool
            while (_bubblePool.Count > 0)
            {
                var pooledBubble = _bubblePool.Dequeue();
                if (pooledBubble.GameObject != null)
                {
                    Object.DestroyImmediate(pooledBubble.GameObject);
                }
            }
            
            // Dispose dependencies
            _positionCalculator?.Dispose();
            
            IsInitialized = false;
            
            UnityEngine.Debug.Log("[BubbleManagementSystem] Disposed");
        }
        
        #region Private Methods
        
        private void CreateBubblePrefab()
        {
            // Create a simple sphere prefab for bubbles
            _bubblePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _bubblePrefab.name = "BubblePrefab";
            
            // Remove collider for performance
            var collider = _bubblePrefab.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
            
            // Configure renderer for transparency
            var renderer = _bubblePrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Create transparent material
                var material = new Material(Shader.Find("Standard"));
                material.SetFloat("_Mode", 3); // Transparent mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                
                material.color = _visualConfig.BaseColor;
                renderer.material = material;
            }
            
            // Deactivate prefab
            _bubblePrefab.SetActive(false);
        }
        
        private void CreateInitialBubbles()
        {
            for (int i = 0; i < TargetBubbleCount; i++)
            {
                var bubble = CreateBubble(i);
                _activeBubbles.Add(bubble);
            }
            
            _stats.TotalInstantiations = TargetBubbleCount;
        }
        
        private BubbleInstance CreateBubble(int index)
        {
            GameObject bubbleGO;
            
            // Use pooling if enabled and available
            if (_poolingEnabled && _bubblePool.Count > 0)
            {
                var pooledBubble = _bubblePool.Dequeue();
                bubbleGO = pooledBubble.GameObject;
                bubbleGO.SetActive(true);
            }
            else
            {
                bubbleGO = Object.Instantiate(_bubblePrefab, _container);
                bubbleGO.SetActive(true);
                _stats.TotalInstantiations++;
            }
            
            bubbleGO.name = $"Bubble_{index:D3}";
            
            // Calculate initial grid position
            var gridPosition = CalculateGridPosition(index);
            bubbleGO.transform.position = gridPosition;
            
            // Create bubble data
            var bubbleData = BubbleData.Create(index, index, UnityEngine.Random.Range(0f, 2f));
            bubbleData.Position = gridPosition;
            
            var bubble = new BubbleInstance
            {
                GameObject = bubbleGO,
                Transform = bubbleGO.transform,
                Data = bubbleData,
                Index = index
            };
            
            // Apply visual configuration
            ApplyVisualConfiguration(bubble);
            
            return bubble;
        }
        
        private float3 CalculateGridPosition(int index)
        {
            // Arrange bubbles in a 10x10 grid
            int gridSize = 10;
            float spacing = 2f;
            
            int x = index % gridSize;
            int z = index / gridSize;
            
            // Center the grid
            float offsetX = (gridSize - 1) * spacing * 0.5f;
            float offsetZ = (gridSize - 1) * spacing * 0.5f;
            
            return new float3(
                x * spacing - offsetX,
                0f,
                z * spacing - offsetZ
            );
        }
        
        private void ApplyVisualConfiguration(BubbleInstance bubble)
        {
            // Apply scale with variation
            float scale = _visualConfig.BaseScale + 
                         UnityEngine.Random.Range(-_visualConfig.ScaleVariation, _visualConfig.ScaleVariation);
            bubble.Transform.localScale = Vector3.one * scale;
            
            // Apply color with variation
            var renderer = bubble.GameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = renderer.material;
                var color = _visualConfig.BaseColor;
                
                if (_visualConfig.ColorVariation > 0f)
                {
                    color.r += UnityEngine.Random.Range(-_visualConfig.ColorVariation, _visualConfig.ColorVariation);
                    color.g += UnityEngine.Random.Range(-_visualConfig.ColorVariation, _visualConfig.ColorVariation);
                    color.b += UnityEngine.Random.Range(-_visualConfig.ColorVariation, _visualConfig.ColorVariation);
                }
                
                color.a = _visualConfig.Alpha;
                material.color = color;
            }
        }
        
        private void ApplyVisualEffects(BubbleInstance bubble, float deltaTime)
        {
            // Apply rotation if enabled
            if (_visualConfig.EnableRotation)
            {
                bubble.Transform.Rotate(Vector3.up, _visualConfig.RotationSpeed * deltaTime * 60f);
            }
            
            // Apply scale pulsing if enabled
            if (_visualConfig.EnableScalePulsing)
            {
                float waveHeight = bubble.Data.Position.y;
                float pulseScale = 1f + math.sin(_currentTime * 2f + bubble.Index) * _visualConfig.PulsingIntensity;
                
                Vector3 baseScale = Vector3.one * _visualConfig.BaseScale;
                bubble.Transform.localScale = baseScale * pulseScale;
            }
        }
        
        private void EnsureCorrectBubbleCount()
        {
            int currentCount = ActiveBubbleCount;
            int targetCount = TargetBubbleCount;
            
            if (currentCount < targetCount)
            {
                // Create missing bubbles
                for (int i = currentCount; i < targetCount; i++)
                {
                    var bubble = CreateBubble(i);
                    _activeBubbles.Add(bubble);
                }
                
                UnityEngine.Debug.Log($"[BubbleManagementSystem] Created {targetCount - currentCount} additional bubbles");
            }
            else if (currentCount > targetCount)
            {
                // Remove excess bubbles
                int excessCount = currentCount - targetCount;
                for (int i = 0; i < excessCount; i++)
                {
                    var bubble = _activeBubbles[_activeBubbles.Count - 1];
                    _activeBubbles.RemoveAt(_activeBubbles.Count - 1);
                    
                    if (_poolingEnabled)
                    {
                        bubble.GameObject.SetActive(false);
                        _bubblePool.Enqueue(bubble);
                    }
                    else
                    {
                        Object.DestroyImmediate(bubble.GameObject);
                        _stats.TotalDestructions++;
                    }
                }
                
                UnityEngine.Debug.Log($"[BubbleManagementSystem] Removed {excessCount} excess bubbles");
            }
            
            // Update data arrays
            if (_bubbleDataArray.Length != targetCount)
            {
                _bubbleDataArray = new BubbleData[targetCount];
                _positionResults = new float3[targetCount];
            }
        }
        
        private void UpdatePerformanceStats(long elapsedTicks)
        {
            float microseconds = (float)(elapsedTicks * 1000000.0 / Stopwatch.Frequency);
            float microsecondsPerBubble = microseconds / math.max(1, ActiveBubbleCount);
            
            _stats.AverageUpdateTimeMicroseconds = 
                (_stats.AverageUpdateTimeMicroseconds + microsecondsPerBubble) * 0.5f;
            
            if (microsecondsPerBubble > _stats.PeakUpdateTimeMicroseconds)
            {
                _stats.PeakUpdateTimeMicroseconds = microsecondsPerBubble;
            }
            
            _stats.PooledBubbles = _bubblePool.Count;
            _stats.MemoryUsageBytes = ActiveBubbleCount * 1024; // Rough estimate
            
            // Calculate pool efficiency
            if (_stats.TotalInstantiations > 0)
            {
                _stats.PoolEfficiency = 1f - ((float)_stats.TotalDestructions / _stats.TotalInstantiations);
            }
        }
        
        #endregion
        
        #region Helper Structures
        
        private struct BubbleInstance
        {
            public GameObject GameObject;
            public Transform Transform;
            public BubbleData Data;
            public int Index;
        }
        
        #endregion
    }
}