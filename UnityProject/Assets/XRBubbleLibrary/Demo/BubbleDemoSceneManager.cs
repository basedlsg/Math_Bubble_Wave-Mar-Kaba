using UnityEngine;
using Unity.Mathematics;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.Demo
{
    /// <summary>
    /// Manages the 100-bubble demo scene infrastructure.
    /// Handles scene setup, initialization, and Quest 3 optimization.
    /// Implements Requirement 5.1: Demonstrate wave mathematics with exactly 100 bubbles.
    /// </summary>
    public class BubbleDemoSceneManager : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private int _targetBubbleCount = 100;
        [SerializeField] private bool _validateOnStart = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        
        [Header("Scene Setup")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Light _mainLight;
        [SerializeField] private Transform _bubbleContainer;
        
        [Header("Wave System")]
        [SerializeField] private WaveMatrixSettings _waveSettings = WaveMatrixSettings.Default;
        [SerializeField] private bool _validateWaveParameters = true;
        
        [Header("Quest 3 Optimization")]
        [SerializeField] private bool _enableQuest3Optimizations = true;
        [SerializeField] private float _targetFrameRate = 72f;
        [SerializeField] private int _renderTextureSize = 1024;
        
        // Scene components
        private IBubbleManagementSystem _bubbleManager;
        private IWaveParameterValidator _parameterValidator;
        private IPerformanceOptimizedWaveSystem _waveSystem;
        
        // Performance monitoring
        private float _lastFrameTime;
        private int _frameCount;
        private float _averageFrameTime;
        
        // Scene state
        private bool _isInitialized;
        private bool _isRunning;
        private DemoSceneValidationResult _validationResult;
        
        /// <summary>
        /// Current bubble count in the scene.
        /// </summary>
        public int CurrentBubbleCount => _bubbleManager?.ActiveBubbleCount ?? 0;
        
        /// <summary>
        /// Whether the demo scene is properly initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether the demo is currently running.
        /// </summary>
        public bool IsRunning => _isRunning;
        
        /// <summary>
        /// Current average frame time in milliseconds.
        /// </summary>
        public float AverageFrameTime => _averageFrameTime;
        
        /// <summary>
        /// Scene validation result.
        /// </summary>
        public DemoSceneValidationResult ValidationResult => _validationResult;
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            if (_validateOnStart)
            {
                ValidateScene();
            }
            
            InitializeScene();
        }
        
        private void Update()
        {
            if (!_isInitialized || !_isRunning) return;
            
            UpdatePerformanceMonitoring();
            UpdateWaveSystem();
        }
        
        private void OnDestroy()
        {
            CleanupScene();
        }
        
        /// <summary>
        /// Initialize scene components and dependencies.
        /// </summary>
        private void InitializeComponents()
        {
            // Initialize wave system components
            _waveSystem = new PerformanceOptimizedWaveSystem();
            _parameterValidator = new WaveParameterValidator();
            
            // Find or create scene components
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main ?? FindObjectOfType<Camera>();
            }
            
            if (_mainLight == null)
            {
                _mainLight = FindObjectOfType<Light>();
            }
            
            if (_bubbleContainer == null)
            {
                var containerGO = new GameObject("BubbleContainer");
                _bubbleContainer = containerGO.transform;
                _bubbleContainer.SetParent(transform);
            }
            
            Debug.Log("[BubbleDemoSceneManager] Components initialized");
        }
        
        /// <summary>
        /// Initialize the demo scene with proper settings.
        /// </summary>
        private void InitializeScene()
        {
            try
            {
                // Apply Quest 3 optimizations
                if (_enableQuest3Optimizations)
                {
                    ApplyQuest3Optimizations();
                }
                
                // Validate wave parameters
                if (_validateWaveParameters)
                {
                    var paramValidation = _parameterValidator.ValidateSettings(_waveSettings);
                    if (!paramValidation.IsValid)
                    {
                        Debug.LogWarning($"[BubbleDemoSceneManager] Wave parameter validation failed: {paramValidation.ValidationReport}");
                        _waveSettings = _parameterValidator.SuggestCorrections(_waveSettings, paramValidation);
                        Debug.Log("[BubbleDemoSceneManager] Applied corrected wave settings");
                    }
                }
                
                // Initialize bubble management system
                _bubbleManager = new BubbleManagementSystem(_targetBubbleCount, _bubbleContainer, _waveSystem);
                
                // Warm up systems
                _waveSystem.WarmUp();
                
                _isInitialized = true;
                _isRunning = true;
                
                Debug.Log($"[BubbleDemoSceneManager] Scene initialized successfully with {_targetBubbleCount} bubbles");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BubbleDemoSceneManager] Failed to initialize scene: {ex.Message}");
                _isInitialized = false;
            }
        }
        
        /// <summary>
        /// Apply Quest 3 specific optimizations.
        /// </summary>
        private void ApplyQuest3Optimizations()
        {
            // Set target frame rate
            Application.targetFrameRate = (int)_targetFrameRate;
            
            // Configure camera for VR
            if (_mainCamera != null)
            {
                _mainCamera.fieldOfView = 90f; // Wide FOV for VR
                _mainCamera.nearClipPlane = 0.1f;
                _mainCamera.farClipPlane = 1000f;
                
                // Enable VR-friendly rendering
                _mainCamera.allowHDR = false; // Disable HDR for performance
                _mainCamera.allowMSAA = true; // Enable MSAA for VR
            }
            
            // Configure lighting for performance
            if (_mainLight != null)
            {
                _mainLight.shadows = LightShadows.None; // Disable shadows for performance
                _mainLight.intensity = 1.2f; // Slightly brighter for VR
            }
            
            // Set quality settings for Quest 3
            QualitySettings.vSyncCount = 0; // Disable VSync for VR
            QualitySettings.antiAliasing = 4; // 4x MSAA for VR
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            QualitySettings.shadowResolution = ShadowResolution.Low; // Low shadow resolution
            
            Debug.Log("[BubbleDemoSceneManager] Quest 3 optimizations applied");
        }
        
        /// <summary>
        /// Update performance monitoring.
        /// </summary>
        private void UpdatePerformanceMonitoring()
        {
            if (!_enablePerformanceMonitoring) return;
            
            _frameCount++;
            _lastFrameTime = Time.unscaledDeltaTime * 1000f; // Convert to milliseconds
            
            // Calculate rolling average
            _averageFrameTime = (_averageFrameTime * (_frameCount - 1) + _lastFrameTime) / _frameCount;
            
            // Reset every 1000 frames to prevent overflow
            if (_frameCount >= 1000)
            {
                _frameCount = 1;
                _averageFrameTime = _lastFrameTime;
            }
            
            // Log performance warnings
            if (_lastFrameTime > (1000f / _targetFrameRate) * 1.5f) // 50% over target
            {
                Debug.LogWarning($"[BubbleDemoSceneManager] Frame time spike: {_lastFrameTime:F2}ms (target: {1000f / _targetFrameRate:F2}ms)");
            }
        }
        
        /// <summary>
        /// Update the wave system.
        /// </summary>
        private void UpdateWaveSystem()
        {
            _waveSystem.UpdateWaveState(Time.deltaTime, _waveSettings);
        }
        
        /// <summary>
        /// Validate the demo scene setup.
        /// </summary>
        public DemoSceneValidationResult ValidateScene()
        {
            var issues = new System.Collections.Generic.List<string>();
            bool isValid = true;
            
            // Validate essential components
            if (_mainCamera == null)
            {
                issues.Add("Main camera is missing");
                isValid = false;
            }
            
            if (_bubbleContainer == null)
            {
                issues.Add("Bubble container is missing");
                isValid = false;
            }
            
            // Validate bubble count
            if (_targetBubbleCount != 100)
            {
                issues.Add($"Target bubble count is {_targetBubbleCount}, should be exactly 100");
                isValid = false;
            }
            
            // Validate wave parameters
            if (_parameterValidator != null)
            {
                var paramValidation = _parameterValidator.ValidateSettings(_waveSettings);
                if (!paramValidation.IsValid)
                {
                    issues.Add($"Wave parameters invalid: {paramValidation.ValidationReport}");
                    isValid = false;
                }
            }
            
            // Validate performance settings
            if (_targetFrameRate < 60f)
            {
                issues.Add($"Target frame rate {_targetFrameRate} is below minimum 60 FPS");
                isValid = false;
            }
            
            _validationResult = new DemoSceneValidationResult
            {
                IsValid = isValid,
                Issues = issues.ToArray(),
                BubbleCount = _targetBubbleCount,
                FrameRateTarget = _targetFrameRate,
                ValidationTime = Time.time
            };
            
            if (isValid)
            {
                Debug.Log("[BubbleDemoSceneManager] Scene validation passed");
            }
            else
            {
                Debug.LogWarning($"[BubbleDemoSceneManager] Scene validation failed: {string.Join(", ", issues)}");
            }
            
            return _validationResult;
        }
        
        /// <summary>
        /// Start the bubble demo.
        /// </summary>
        public void StartDemo()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[BubbleDemoSceneManager] Cannot start demo - scene not initialized");
                return;
            }
            
            _isRunning = true;
            _bubbleManager?.StartDemo();
            
            Debug.Log("[BubbleDemoSceneManager] Demo started");
        }
        
        /// <summary>
        /// Stop the bubble demo.
        /// </summary>
        public void StopDemo()
        {
            _isRunning = false;
            _bubbleManager?.StopDemo();
            
            Debug.Log("[BubbleDemoSceneManager] Demo stopped");
        }
        
        /// <summary>
        /// Reset the demo to initial state.
        /// </summary>
        public void ResetDemo()
        {
            StopDemo();
            _bubbleManager?.ResetBubbles();
            _frameCount = 0;
            _averageFrameTime = 0f;
            
            Debug.Log("[BubbleDemoSceneManager] Demo reset");
        }
        
        /// <summary>
        /// Get current performance statistics.
        /// </summary>
        public DemoPerformanceStats GetPerformanceStats()
        {
            return new DemoPerformanceStats
            {
                CurrentFrameTime = _lastFrameTime,
                AverageFrameTime = _averageFrameTime,
                TargetFrameTime = 1000f / _targetFrameRate,
                FrameCount = _frameCount,
                BubbleCount = CurrentBubbleCount,
                IsRunning = _isRunning,
                WaveSystemStats = _waveSystem?.GetPerformanceStats() ?? default
            };
        }
        
        /// <summary>
        /// Clean up scene resources.
        /// </summary>
        private void CleanupScene()
        {
            _isRunning = false;
            
            _bubbleManager?.Dispose();
            _waveSystem?.Dispose();
            
            Debug.Log("[BubbleDemoSceneManager] Scene cleanup completed");
        }
        
        #region Editor Support
        
        [ContextMenu("Validate Scene")]
        private void ValidateSceneFromEditor()
        {
            ValidateScene();
        }
        
        [ContextMenu("Apply Quest 3 Optimizations")]
        private void ApplyQuest3OptimizationsFromEditor()
        {
            ApplyQuest3Optimizations();
        }
        
        [ContextMenu("Reset Demo")]
        private void ResetDemoFromEditor()
        {
            ResetDemo();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Result of demo scene validation.
    /// </summary>
    [System.Serializable]
    public struct DemoSceneValidationResult
    {
        /// <summary>
        /// Whether the scene is valid for the demo.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// List of validation issues found.
        /// </summary>
        public string[] Issues;
        
        /// <summary>
        /// Target bubble count for the demo.
        /// </summary>
        public int BubbleCount;
        
        /// <summary>
        /// Target frame rate for the demo.
        /// </summary>
        public float FrameRateTarget;
        
        /// <summary>
        /// Time when validation was performed.
        /// </summary>
        public float ValidationTime;
    }
    
    /// <summary>
    /// Performance statistics for the demo.
    /// </summary>
    [System.Serializable]
    public struct DemoPerformanceStats
    {
        /// <summary>
        /// Current frame time in milliseconds.
        /// </summary>
        public float CurrentFrameTime;
        
        /// <summary>
        /// Average frame time in milliseconds.
        /// </summary>
        public float AverageFrameTime;
        
        /// <summary>
        /// Target frame time in milliseconds.
        /// </summary>
        public float TargetFrameTime;
        
        /// <summary>
        /// Total frame count.
        /// </summary>
        public int FrameCount;
        
        /// <summary>
        /// Current bubble count.
        /// </summary>
        public int BubbleCount;
        
        /// <summary>
        /// Whether the demo is currently running.
        /// </summary>
        public bool IsRunning;
        
        /// <summary>
        /// Wave system performance statistics.
        /// </summary>
        public WaveSystemPerformanceStats WaveSystemStats;
    }
}