using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using XRBubbleLibrary.WaveMatrix;
using System.Collections.Generic;
using System.Diagnostics;

namespace XRBubbleLibrary.Demo
{
    /// <summary>
    /// Integrates wave mathematics with visual bubble positioning and animation.
    /// Provides smooth interpolation and real-time synchronization between calculations and visuals.
    /// Implements Requirement 5.1: Demonstrate wave mathematics with exactly 100 bubbles.
    /// Implements Requirement 5.2: Performance optimization for Quest 3.
    /// </summary>
    public class WaveVisualIntegrator : MonoBehaviour
    {
        [Header("Integration Settings")]
        [SerializeField] private float _interpolationSpeed = 10f;
        [SerializeField] private bool _enableSmoothInterpolation = true;
        [SerializeField] private bool _enableVisualEffects = true;
        [SerializeField] private float _updateFrequency = 60f; // Updates per second
        
        [Header("Wave Mathematics")]
        [SerializeField] private WaveMatrixSettings _waveSettings = WaveMatrixSettings.Default;
        [SerializeField] private bool _validateWaveParameters = true;
        
        [Header("Performance Monitoring")]
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private float _performanceLogInterval = 5f;
        
        // Core systems
        private IPerformanceOptimizedWaveSystem _waveSystem;
        private IBubblePositionCalculator _positionCalculator;
        private IWaveParameterValidator _parameterValidator;
        private IBubbleManagementSystem _bubbleManager;
        
        // Integration state
        private float _currentTime;
        private float _lastUpdateTime;
        private float _updateInterval;
        private bool _isInitialized;
        private bool _isRunning;
        
        // Performance tracking
        private WaveVisualIntegrationStats _stats;
        private readonly Stopwatch _performanceTimer = new Stopwatch();
        private float _lastPerformanceLogTime;
        
        // Data arrays for efficient processing
        private BubbleData[] _bubbleDataArray;
        private float3[] _targetPositions;
        private float3[] _currentPositions;
        private float3[] _velocities;
        
        // Visual effects
        private readonly Dictionary<int, BubbleVisualState> _bubbleVisualStates = new Dictionary<int, BubbleVisualState>();
        
        /// <summary>
        /// Whether the integrator is initialized and ready.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether the integration is currently running.
        /// </summary>
        public bool IsRunning => _isRunning;
        
        /// <summary>
        /// Current wave time.
        /// </summary>
        public float CurrentTime => _currentTime;
        
        /// <summary>
        /// Integration performance statistics.
        /// </summary>
        public WaveVisualIntegrationStats Stats => _stats;
        
        private void Awake()
        {
            InitializeSystems();
        }
        
        private void Start()
        {
            if (_validateWaveParameters)
            {
                ValidateWaveParameters();
            }
            
            InitializeIntegration();
        }
        
        private void Update()
        {
            if (!_isInitialized || !_isRunning) return;
            
            UpdateIntegration();
            
            if (_enablePerformanceMonitoring)
            {
                UpdatePerformanceMonitoring();
            }
        }
        
        private void OnDestroy()
        {
            CleanupSystems();
        }
        
        /// <summary>
        /// Initialize the wave-visual integration system.
        /// </summary>
        public void Initialize(IBubbleManagementSystem bubbleManager)
        {
            _bubbleManager = bubbleManager;
            InitializeIntegration();
        }
        
        /// <summary>
        /// Start the wave-visual integration.
        /// </summary>
        public void StartIntegration()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[WaveVisualIntegrator] Cannot start - not initialized");
                return;
            }
            
            _isRunning = true;
            _currentTime = 0f;
            _lastUpdateTime = Time.time;
            
            // Initialize bubble visual states
            InitializeBubbleVisualStates();
            
            Debug.Log("[WaveVisualIntegrator] Integration started");
        }
        
        /// <summary>
        /// Stop the wave-visual integration.
        /// </summary>
        public void StopIntegration()
        {
            _isRunning = false;
            Debug.Log("[WaveVisualIntegrator] Integration stopped");
        }
        
        /// <summary>
        /// Reset the integration to initial state.
        /// </summary>
        public void ResetIntegration()
        {
            _currentTime = 0f;
            _lastUpdateTime = Time.time;
            
            // Reset bubble positions to initial grid layout
            if (_bubbleManager != null)
            {
                _bubbleManager.ResetBubbles();
            }
            
            // Clear visual states
            _bubbleVisualStates.Clear();
            InitializeBubbleVisualStates();
            
            // Reset statistics
            _stats = new WaveVisualIntegrationStats
            {
                StartTime = Time.time
            };
            
            Debug.Log("[WaveVisualIntegrator] Integration reset");
        }
        
        /// <summary>
        /// Update wave settings and validate parameters.
        /// </summary>
        public void UpdateWaveSettings(WaveMatrixSettings newSettings)
        {
            if (_validateWaveParameters && _parameterValidator != null)
            {
                var validation = _parameterValidator.ValidateSettings(newSettings);
                if (!validation.IsValid)
                {
                    Debug.LogWarning($"[WaveVisualIntegrator] Invalid wave settings: {validation.ValidationReport}");
                    newSettings = _parameterValidator.SuggestCorrections(newSettings, validation);
                    Debug.Log("[WaveVisualIntegrator] Applied corrected wave settings");
                }
            }
            
            _waveSettings = newSettings;
            Debug.Log("[WaveVisualIntegrator] Wave settings updated");
        }
        
        /// <summary>
        /// Get current integration performance statistics.
        /// </summary>
        public WaveVisualIntegrationStats GetPerformanceStats()
        {
            _stats.CurrentTime = _currentTime;
            _stats.IsRunning = _isRunning;
            return _stats;
        }
        
        /// <summary>
        /// Validate synchronization between wave mathematics and visual positions.
        /// </summary>
        public WaveVisualSyncValidationResult ValidateSynchronization()
        {
            if (!_isInitialized || _bubbleManager == null)
            {
                return WaveVisualSyncValidationResult.Failure("System not initialized");
            }
            
            var bubbleData = _bubbleManager.GetBubbleData();
            var bubbleTransforms = _bubbleManager.GetActiveBubbleTransforms();
            
            if (bubbleData.Length != bubbleTransforms.Length)
            {
                return WaveVisualSyncValidationResult.Failure("Data and transform count mismatch");
            }
            
            float maxDeviation = 0f;
            int outOfSyncCount = 0;
            const float syncTolerance = 0.1f; // 10cm tolerance
            
            for (int i = 0; i < bubbleData.Length; i++)
            {
                float3 mathPosition = bubbleData[i].Position;
                float3 visualPosition = bubbleTransforms[i].position;
                
                float deviation = math.distance(mathPosition, visualPosition);
                maxDeviation = math.max(maxDeviation, deviation);
                
                if (deviation > syncTolerance)
                {
                    outOfSyncCount++;
                }
            }
            
            bool isInSync = outOfSyncCount == 0;
            float syncAccuracy = 1f - ((float)outOfSyncCount / bubbleData.Length);
            
            return new WaveVisualSyncValidationResult
            {
                IsInSync = isInSync,
                MaxDeviation = maxDeviation,
                OutOfSyncCount = outOfSyncCount,
                TotalBubbles = bubbleData.Length,
                SyncAccuracy = syncAccuracy,
                ValidationTime = Time.time,
                Message = isInSync ? "All bubbles in sync" : $"{outOfSyncCount} bubbles out of sync"
            };
        }
        
        #region Private Methods
        
        private void InitializeSystems()
        {
            // Initialize wave mathematics systems
            _waveSystem = new PerformanceOptimizedWaveSystem();
            _positionCalculator = new BubblePositionCalculator();
            _parameterValidator = new WaveParameterValidator();
            
            // Warm up systems
            _waveSystem.WarmUp();
            
            // Calculate update interval
            _updateInterval = 1f / _updateFrequency;
            
            Debug.Log("[WaveVisualIntegrator] Systems initialized");
        }
        
        private void InitializeIntegration()
        {
            if (_bubbleManager == null)
            {
                Debug.LogWarning("[WaveVisualIntegrator] No bubble manager assigned");
                return;
            }
            
            if (!_bubbleManager.IsInitialized)
            {
                Debug.LogWarning("[WaveVisualIntegrator] Bubble manager not initialized");
                return;
            }
            
            // Initialize data arrays
            int bubbleCount = _bubbleManager.TargetBubbleCount;
            _bubbleDataArray = new BubbleData[bubbleCount];
            _targetPositions = new float3[bubbleCount];
            _currentPositions = new float3[bubbleCount];
            _velocities = new float3[bubbleCount];
            
            // Initialize statistics
            _stats = new WaveVisualIntegrationStats
            {
                StartTime = Time.time,
                BubbleCount = bubbleCount
            };
            
            _isInitialized = true;
            
            Debug.Log($"[WaveVisualIntegrator] Integration initialized for {bubbleCount} bubbles");
        }
        
        private void ValidateWaveParameters()
        {
            if (_parameterValidator == null) return;
            
            var validation = _parameterValidator.ValidateSettings(_waveSettings);
            if (!validation.IsValid)
            {
                Debug.LogWarning($"[WaveVisualIntegrator] Wave parameter validation failed: {validation.ValidationReport}");
                
                // Apply corrections
                _waveSettings = _parameterValidator.SuggestCorrections(_waveSettings, validation);
                Debug.Log("[WaveVisualIntegrator] Applied corrected wave parameters");
            }
            else
            {
                Debug.Log($"[WaveVisualIntegrator] Wave parameters validated successfully (score: {validation.ValidationScore:F2})");
            }
        }
        
        private void UpdateIntegration()
        {
            float currentTime = Time.time;
            float deltaTime = currentTime - _lastUpdateTime;
            
            // Check if it's time for an update
            if (deltaTime < _updateInterval) return;
            
            _performanceTimer.Restart();
            
            try
            {
                // Update wave time
                _currentTime += deltaTime;
                
                // Get current bubble data
                var bubbleData = _bubbleManager.GetBubbleData();
                if (bubbleData.Length != _bubbleDataArray.Length)
                {
                    Debug.LogWarning("[WaveVisualIntegrator] Bubble count mismatch, reinitializing arrays");
                    InitializeIntegration();
                    return;
                }
                
                // Copy bubble data
                System.Array.Copy(bubbleData, _bubbleDataArray, bubbleData.Length);
                
                // Calculate new target positions using wave mathematics
                _positionCalculator.CalculateAllPositions(_bubbleDataArray, _currentTime, _waveSettings, _targetPositions);
                
                // Update visual positions
                UpdateVisualPositions(deltaTime);
                
                // Apply visual effects
                if (_enableVisualEffects)
                {
                    UpdateVisualEffects(deltaTime);
                }
                
                // Update statistics
                _stats.TotalUpdates++;
                _stats.LastUpdateTime = currentTime;
                
                _lastUpdateTime = currentTime;
            }
            finally
            {
                _performanceTimer.Stop();
                UpdatePerformanceStats(_performanceTimer.ElapsedTicks);
            }
        }
        
        private void UpdateVisualPositions(float deltaTime)
        {
            var bubbleTransforms = _bubbleManager.GetActiveBubbleTransforms();
            
            for (int i = 0; i < bubbleTransforms.Length; i++)
            {
                var transform = bubbleTransforms[i];
                float3 targetPos = _targetPositions[i];
                float3 currentPos = transform.position;
                
                if (_enableSmoothInterpolation)
                {
                    // Smooth interpolation with velocity-based damping
                    float3 velocity = _velocities[i];
                    float3 displacement = targetPos - currentPos;
                    
                    // Spring-damper system for smooth movement
                    float springStrength = _interpolationSpeed;
                    float dampingFactor = 0.7f;
                    
                    float3 force = displacement * springStrength - velocity * dampingFactor;
                    velocity += force * deltaTime;
                    float3 newPos = currentPos + velocity * deltaTime;
                    
                    transform.position = newPos;
                    _velocities[i] = velocity;
                    _currentPositions[i] = newPos;
                }
                else
                {
                    // Direct position assignment
                    transform.position = targetPos;
                    _currentPositions[i] = targetPos;
                    _velocities[i] = float3.zero;
                }
                
                // Update bubble data position
                _bubbleDataArray[i].Position = transform.position;
            }
        }
        
        private void UpdateVisualEffects(float deltaTime)
        {
            var bubbleTransforms = _bubbleManager.GetActiveBubbleTransforms();
            
            for (int i = 0; i < bubbleTransforms.Length; i++)
            {
                if (!_bubbleVisualStates.TryGetValue(i, out BubbleVisualState visualState))
                {
                    continue;
                }
                
                var transform = bubbleTransforms[i];
                
                // Update rotation based on wave movement
                float3 velocity = _velocities[i];
                if (math.lengthsq(velocity) > 0.001f)
                {
                    float rotationSpeed = math.length(velocity) * 10f;
                    transform.Rotate(Vector3.up, rotationSpeed * deltaTime);
                }
                
                // Update scale based on wave height
                float waveHeight = _targetPositions[i].y;
                float scaleMultiplier = 1f + math.sin(_currentTime * 2f + i) * 0.1f;
                float targetScale = visualState.BaseScale * scaleMultiplier;
                
                float currentScale = transform.localScale.x;
                float newScale = Mathf.Lerp(currentScale, targetScale, deltaTime * 5f);
                transform.localScale = Vector3.one * newScale;
                
                // Update visual state
                visualState.LastUpdateTime = _currentTime;
                _bubbleVisualStates[i] = visualState;
            }
        }
        
        private void InitializeBubbleVisualStates()
        {
            if (_bubbleManager == null) return;
            
            var bubbleTransforms = _bubbleManager.GetActiveBubbleTransforms();
            
            for (int i = 0; i < bubbleTransforms.Length; i++)
            {
                _bubbleVisualStates[i] = new BubbleVisualState
                {
                    BubbleIndex = i,
                    BaseScale = bubbleTransforms[i].localScale.x,
                    BaseRotation = bubbleTransforms[i].rotation,
                    LastUpdateTime = _currentTime
                };
            }
            
            Debug.Log($"[WaveVisualIntegrator] Initialized visual states for {bubbleTransforms.Length} bubbles");
        }
        
        private void UpdatePerformanceMonitoring()
        {
            if (Time.time - _lastPerformanceLogTime > _performanceLogInterval)
            {
                var stats = GetPerformanceStats();
                
                Debug.Log($"[WaveVisualIntegrator] Performance: {stats.AverageUpdateTimeMs:F2}ms avg, " +
                         $"{stats.TotalUpdates} updates, {stats.SyncAccuracy:P1} sync accuracy");
                
                _lastPerformanceLogTime = Time.time;
            }
        }
        
        private void UpdatePerformanceStats(long elapsedTicks)
        {
            float milliseconds = (float)(elapsedTicks * 1000.0 / Stopwatch.Frequency);
            
            _stats.AverageUpdateTimeMs = (_stats.AverageUpdateTimeMs + milliseconds) * 0.5f;
            
            if (milliseconds > _stats.PeakUpdateTimeMs)
            {
                _stats.PeakUpdateTimeMs = milliseconds;
            }
            
            // Calculate sync accuracy
            var syncValidation = ValidateSynchronization();
            _stats.SyncAccuracy = syncValidation.SyncAccuracy;
            _stats.MaxSyncDeviation = syncValidation.MaxDeviation;
        }
        
        private void CleanupSystems()
        {
            _isRunning = false;
            
            _waveSystem?.Dispose();
            _positionCalculator?.Dispose();
            
            _bubbleVisualStates.Clear();
            
            Debug.Log("[WaveVisualIntegrator] Systems cleaned up");
        }
        
        #endregion
        
        #region Editor Support
        
        [ContextMenu("Start Integration")]
        private void StartIntegrationFromEditor()
        {
            StartIntegration();
        }
        
        [ContextMenu("Stop Integration")]
        private void StopIntegrationFromEditor()
        {
            StopIntegration();
        }
        
        [ContextMenu("Reset Integration")]
        private void ResetIntegrationFromEditor()
        {
            ResetIntegration();
        }
        
        [ContextMenu("Validate Synchronization")]
        private void ValidateSynchronizationFromEditor()
        {
            var result = ValidateSynchronization();
            Debug.Log($"[WaveVisualIntegrator] Sync validation: {result.Message} " +
                     $"(accuracy: {result.SyncAccuracy:P1}, max deviation: {result.MaxDeviation:F3}m)");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Performance statistics for wave-visual integration.
    /// </summary>
    [System.Serializable]
    public struct WaveVisualIntegrationStats
    {
        /// <summary>
        /// Total number of integration updates performed.
        /// </summary>
        public long TotalUpdates;
        
        /// <summary>
        /// Average update time in milliseconds.
        /// </summary>
        public float AverageUpdateTimeMs;
        
        /// <summary>
        /// Peak update time in milliseconds.
        /// </summary>
        public float PeakUpdateTimeMs;
        
        /// <summary>
        /// Current wave time.
        /// </summary>
        public float CurrentTime;
        
        /// <summary>
        /// Time when integration started.
        /// </summary>
        public float StartTime;
        
        /// <summary>
        /// Last update time.
        /// </summary>
        public float LastUpdateTime;
        
        /// <summary>
        /// Number of bubbles being integrated.
        /// </summary>
        public int BubbleCount;
        
        /// <summary>
        /// Whether integration is currently running.
        /// </summary>
        public bool IsRunning;
        
        /// <summary>
        /// Synchronization accuracy (0.0 to 1.0).
        /// </summary>
        public float SyncAccuracy;
        
        /// <summary>
        /// Maximum synchronization deviation in meters.
        /// </summary>
        public float MaxSyncDeviation;
    }
    
    /// <summary>
    /// Result of wave-visual synchronization validation.
    /// </summary>
    public struct WaveVisualSyncValidationResult
    {
        /// <summary>
        /// Whether wave mathematics and visuals are in sync.
        /// </summary>
        public bool IsInSync;
        
        /// <summary>
        /// Maximum deviation between math and visual positions.
        /// </summary>
        public float MaxDeviation;
        
        /// <summary>
        /// Number of bubbles that are out of sync.
        /// </summary>
        public int OutOfSyncCount;
        
        /// <summary>
        /// Total number of bubbles checked.
        /// </summary>
        public int TotalBubbles;
        
        /// <summary>
        /// Synchronization accuracy (0.0 to 1.0).
        /// </summary>
        public float SyncAccuracy;
        
        /// <summary>
        /// Time when validation was performed.
        /// </summary>
        public float ValidationTime;
        
        /// <summary>
        /// Validation result message.
        /// </summary>
        public string Message;
        
        /// <summary>
        /// Create a successful sync validation result.
        /// </summary>
        public static WaveVisualSyncValidationResult Success(int totalBubbles, float maxDeviation)
        {
            return new WaveVisualSyncValidationResult
            {
                IsInSync = true,
                MaxDeviation = maxDeviation,
                OutOfSyncCount = 0,
                TotalBubbles = totalBubbles,
                SyncAccuracy = 1.0f,
                ValidationTime = Time.time,
                Message = $"All {totalBubbles} bubbles in sync (max deviation: {maxDeviation:F3}m)"
            };
        }
        
        /// <summary>
        /// Create a failed sync validation result.
        /// </summary>
        public static WaveVisualSyncValidationResult Failure(string message)
        {
            return new WaveVisualSyncValidationResult
            {
                IsInSync = false,
                MaxDeviation = float.MaxValue,
                OutOfSyncCount = -1,
                TotalBubbles = 0,
                SyncAccuracy = 0.0f,
                ValidationTime = Time.time,
                Message = message
            };
        }
    }
    
    /// <summary>
    /// Visual state for individual bubbles.
    /// </summary>
    private struct BubbleVisualState
    {
        /// <summary>
        /// Index of the bubble.
        /// </summary>
        public int BubbleIndex;
        
        /// <summary>
        /// Base scale of the bubble.
        /// </summary>
        public float BaseScale;
        
        /// <summary>
        /// Base rotation of the bubble.
        /// </summary>
        public Quaternion BaseRotation;
        
        /// <summary>
        /// Last time this bubble was updated.
        /// </summary>
        public float LastUpdateTime;
    }
}