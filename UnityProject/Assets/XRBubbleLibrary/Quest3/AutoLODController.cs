using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace XRBubbleLibrary.Quest3
{
    /// <summary>
    /// Auto-LOD Controller for maintaining Quest 3 performance through automatic quality adjustment.
    /// Implements Requirement 5.5: Automatic LOD system for performance maintenance.
    /// Implements Requirement 8.4: Auto-LOD controller for quality adjustment.
    /// </summary>
    public class AutoLODController : MonoBehaviour, IAutoLODController
    {
        // Configuration and state
        private AutoLODConfiguration _configuration;
        private bool _isInitialized;
        private bool _isAutoLODEnabled;
        private int _currentLODLevel;
        private LODSettings _currentSettings;
        
        // Performance tracking
        private readonly Queue<AutoLODPerformanceMetrics> _performanceHistory = new Queue<AutoLODPerformanceMetrics>();
        private readonly List<LODLevelChange> _lodHistory = new List<LODLevelChange>();
        private int _consecutiveFramesBelowTarget;
        private float _timeSinceLastAdjustment;
        private float _manualOverrideTimeRemaining;
        private bool _isManualOverrideActive;
        
        // Bubble count management
        private BubbleCountAdjustmentConfig _bubbleConfig;
        private int _currentBubbleCount;
        private int _targetBubbleCount;
        private float _bubbleCountSmoothingVelocity;
        
        // Performance analysis
        private const int MAX_PERFORMANCE_HISTORY = 300; // 5 seconds at 60 FPS
        private const float PERFORMANCE_SMOOTHING_FACTOR = 0.1f;
        private float _smoothedFPS;
        private float _smoothedCPUUsage;
        private float _smoothedGPUUsage;
        
        // Events
        public event Action<AutoLODLevelChangedEventArgs> LODLevelChanged;
        public event Action<AutoLODPerformanceEventArgs> PerformanceThresholdCrossed;
        public event Action<BubbleCountAdjustedEventArgs> BubbleCountAdjusted;
        
        /// <summary>
        /// Whether the Auto-LOD controller is initialized and active.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether Auto-LOD adjustments are currently enabled.
        /// </summary>
        public bool IsAutoLODEnabled => _isAutoLODEnabled;
        
        /// <summary>
        /// Current LOD level being applied (0 = highest quality, higher = lower quality).
        /// </summary>
        public int CurrentLODLevel => _currentLODLevel;
        
        /// <summary>
        /// Current Auto-LOD configuration settings.
        /// </summary>
        public AutoLODConfiguration Configuration => _configuration;
        
        private void Awake()
        {
            // Initialize with default configuration
            Initialize(AutoLODConfiguration.Quest3Default);
        }
        
        private void Update()
        {
            if (!_isInitialized || !_isAutoLODEnabled)
                return;
            
            // Update timing
            _timeSinceLastAdjustment += Time.deltaTime;
            
            // Handle manual override timeout
            if (_isManualOverrideActive && _manualOverrideTimeRemaining > 0f)
            {
                _manualOverrideTimeRemaining -= Time.deltaTime;
                if (_manualOverrideTimeRemaining <= 0f)
                {
                    _isManualOverrideActive = false;
                    if (_configuration.EnableDebugLogging)
                    {
                        Debug.Log("[AutoLODController] Manual override expired, resuming automatic adjustments");
                    }
                }
            }
            
            // Update bubble count smoothing
            UpdateBubbleCountSmoothing();
        }
        
        /// <summary>
        /// Initialize the Auto-LOD controller with the specified configuration.
        /// </summary>
        public bool Initialize(AutoLODConfiguration config)
        {
            try
            {
                _configuration = config;
                _isAutoLODEnabled = true;
                _currentLODLevel = 0;
                _currentSettings = LODSettings.HighestQuality;
                _bubbleConfig = BubbleCountAdjustmentConfig.Default;
                _currentBubbleCount = _bubbleConfig.MaxBubbleCount;
                _targetBubbleCount = _bubbleConfig.MaxBubbleCount;
                
                // Initialize performance tracking
                _consecutiveFramesBelowTarget = 0;
                _timeSinceLastAdjustment = 0f;
                _manualOverrideTimeRemaining = 0f;
                _isManualOverrideActive = false;
                
                // Initialize smoothed values
                _smoothedFPS = config.TargetFPS;
                _smoothedCPUUsage = 50f;
                _smoothedGPUUsage = 50f;
                
                _isInitialized = true;
                
                if (_configuration.EnableDebugLogging)
                {
                    Debug.Log($"[AutoLODController] Initialized with target FPS: {config.TargetFPS}, " +
                             $"minimum FPS: {config.MinimumFPS}, max LOD level: {config.MaxLODLevel}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AutoLODController] Initialization failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Enable or disable automatic LOD adjustments.
        /// </summary>
        public void SetAutoLODEnabled(bool enabled)
        {
            _isAutoLODEnabled = enabled;
            
            if (_configuration.EnableDebugLogging)
            {
                Debug.Log($"[AutoLODController] Auto-LOD {(enabled ? "enabled" : "disabled")}");
            }
            
            if (!enabled)
            {
                // Reset to highest quality when disabled
                ResetToHighestQuality();
            }
        }
        
        /// <summary>
        /// Update the Auto-LOD system with current performance metrics.
        /// </summary>
        public void UpdatePerformanceMetrics(float currentFPS, float frameTime, float cpuUsage, float gpuUsage)
        {
            if (!_isInitialized)
                return;
            
            // Smooth the metrics to reduce noise
            _smoothedFPS = Mathf.Lerp(_smoothedFPS, currentFPS, PERFORMANCE_SMOOTHING_FACTOR);
            _smoothedCPUUsage = Mathf.Lerp(_smoothedCPUUsage, cpuUsage, PERFORMANCE_SMOOTHING_FACTOR);
            _smoothedGPUUsage = Mathf.Lerp(_smoothedGPUUsage, gpuUsage, PERFORMANCE_SMOOTHING_FACTOR);
            
            // Create performance metrics
            var metrics = new AutoLODPerformanceMetrics
            {
                CurrentFPS = currentFPS,
                AverageFPS = _smoothedFPS,
                FrameTimeMs = frameTime,
                CPUUsage = cpuUsage,
                GPUUsage = gpuUsage,
                FrameRateVariance = CalculateFrameRateVariance(),
                PerformanceScore = CalculatePerformanceScore(currentFPS, cpuUsage, gpuUsage),
                Timestamp = DateTime.Now
            };
            
            // Add to performance history
            _performanceHistory.Enqueue(metrics);
            if (_performanceHistory.Count > MAX_PERFORMANCE_HISTORY)
            {
                _performanceHistory.Dequeue();
            }
            
            // Check for performance threshold crossings
            CheckPerformanceThresholds(metrics);
            
            // Perform LOD adjustment if needed
            if (_isAutoLODEnabled && !_isManualOverrideActive)
            {
                EvaluateAndAdjustLOD(metrics);
            }
        }
        
        /// <summary>
        /// Manually set the LOD level (overrides automatic adjustments temporarily).
        /// </summary>
        public void SetManualLODLevel(int lodLevel, float duration = 0f)
        {
            if (!_isInitialized)
                return;
            
            lodLevel = Mathf.Clamp(lodLevel, 0, _configuration.MaxLODLevel);
            
            var previousLevel = _currentLODLevel;
            _currentLODLevel = lodLevel;
            _currentSettings = GetLODSettingsForLevel(lodLevel);
            
            _isManualOverrideActive = true;
            _manualOverrideTimeRemaining = duration;
            
            // Apply the LOD settings
            ApplyLODSettings(_currentSettings);
            
            // Record the change
            RecordLODChange(previousLevel, lodLevel, "Manual override");
            
            if (_configuration.EnableDebugLogging)
            {
                Debug.Log($"[AutoLODController] Manual LOD level set to {lodLevel}" +
                         (duration > 0f ? $" for {duration:F1} seconds" : " permanently"));
            }
            
            // Fire event
            LODLevelChanged?.Invoke(new AutoLODLevelChangedEventArgs
            {
                PreviousLevel = previousLevel,
                NewLevel = lodLevel,
                Reason = "Manual override",
                NewSettings = _currentSettings,
                TriggeringMetrics = GetCurrentPerformanceMetrics(),
                Timestamp = DateTime.Now
            });
        }
        
        /// <summary>
        /// Get the current performance status and LOD recommendations.
        /// </summary>
        public AutoLODStatus GetCurrentStatus()
        {
            var currentMetrics = GetCurrentPerformanceMetrics();
            
            return new AutoLODStatus
            {
                CurrentLODLevel = _currentLODLevel,
                IsActive = _isAutoLODEnabled,
                CurrentMetrics = currentMetrics,
                TimeSinceLastAdjustment = _timeSinceLastAdjustment,
                ConsecutiveFramesBelowTarget = _consecutiveFramesBelowTarget,
                IsManualOverrideActive = _isManualOverrideActive,
                ManualOverrideTimeRemaining = _manualOverrideTimeRemaining,
                CurrentSettings = _currentSettings,
                RecentTrend = AnalyzePerformanceTrend()
            };
        }
        
        /// <summary>
        /// Get LOD adjustment recommendations based on current performance.
        /// </summary>
        public AutoLODRecommendations GetLODRecommendations(float targetFPS = 72f)
        {
            var currentMetrics = GetCurrentPerformanceMetrics();
            var currentFPS = currentMetrics.AverageFPS;
            
            // Calculate recommended LOD level
            int recommendedLevel = _currentLODLevel;
            var adjustments = new List<LODAdjustmentType>();
            float expectedImprovement = 0f;
            float confidence = 0.5f;
            
            if (currentFPS < targetFPS)
            {
                // Performance is below target, recommend reducing quality
                float fpsDeficit = targetFPS - currentFPS;
                int levelIncrease = Mathf.CeilToInt(fpsDeficit / 10f); // Rough estimate: 10 FPS per LOD level
                recommendedLevel = Mathf.Min(_currentLODLevel + levelIncrease, _configuration.MaxLODLevel);
                
                expectedImprovement = levelIncrease * 10f;
                confidence = Mathf.Clamp01(fpsDeficit / 20f); // Higher confidence for larger deficits
                
                // Determine specific adjustments
                if (_configuration.EnableBubbleCountAdjustment && _currentBubbleCount > _bubbleConfig.MinBubbleCount)
                {
                    adjustments.Add(LODAdjustmentType.ReduceBubbleCount);
                }
                
                if (_configuration.EnableRenderingQualityAdjustment)
                {
                    adjustments.Add(LODAdjustmentType.DecreaseRenderingQuality);
                }
                
                if (_configuration.EnableEffectQualityAdjustment)
                {
                    adjustments.Add(LODAdjustmentType.ReduceEffectQuality);
                    adjustments.Add(LODAdjustmentType.ReduceShadowQuality);
                }
            }
            else if (currentFPS > targetFPS + 10f && _currentLODLevel > 0)
            {
                // Performance is well above target, can increase quality
                recommendedLevel = Mathf.Max(_currentLODLevel - 1, 0);
                adjustments.Add(LODAdjustmentType.IncreaseRenderingQuality);
                confidence = 0.7f;
            }
            
            var qualityImpact = AssessQualityImpact(recommendedLevel);
            
            return new AutoLODRecommendations
            {
                RecommendedLODLevel = recommendedLevel,
                Confidence = confidence,
                RecommendedAdjustments = adjustments.ToArray(),
                ExpectedFPSImprovement = expectedImprovement,
                QualityImpact = qualityImpact,
                RecommendationReasoning = GenerateRecommendationReasoning(currentFPS, targetFPS, recommendedLevel)
            };
        }
        
        /// <summary>
        /// Apply a specific LOD configuration to the scene.
        /// </summary>
        public bool ApplyLODSettings(LODSettings lodSettings)
        {
            try
            {
                _currentSettings = lodSettings;
                
                // Apply bubble count adjustment
                if (_configuration.EnableBubbleCountAdjustment)
                {
                    SetTargetBubbleCount(lodSettings.BubbleCount);
                }
                
                // Apply rendering quality settings
                if (_configuration.EnableRenderingQualityAdjustment)
                {
                    ApplyRenderingQuality(lodSettings);
                }
                
                // Apply effect quality settings
                if (_configuration.EnableEffectQualityAdjustment)
                {
                    ApplyEffectQuality(lodSettings);
                }
                
                if (_configuration.EnableDebugLogging)
                {
                    Debug.Log($"[AutoLODController] Applied LOD settings - Level: {lodSettings.LODLevel}, " +
                             $"Bubbles: {lodSettings.BubbleCount}, Quality: {lodSettings.RenderingQuality:F2}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AutoLODController] Failed to apply LOD settings: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get the current LOD settings being applied.
        /// </summary>
        public LODSettings GetCurrentLODSettings()
        {
            return _currentSettings;
        }
        
        /// <summary>
        /// Reset LOD to the highest quality level.
        /// </summary>
        public void ResetToHighestQuality()
        {
            if (!_isInitialized)
                return;
            
            var previousLevel = _currentLODLevel;
            _currentLODLevel = 0;
            _currentSettings = LODSettings.HighestQuality;
            _consecutiveFramesBelowTarget = 0;
            _timeSinceLastAdjustment = 0f;
            
            ApplyLODSettings(_currentSettings);
            
            if (previousLevel != 0)
            {
                RecordLODChange(previousLevel, 0, "Reset to highest quality");
                
                LODLevelChanged?.Invoke(new AutoLODLevelChangedEventArgs
                {
                    PreviousLevel = previousLevel,
                    NewLevel = 0,
                    Reason = "Reset to highest quality",
                    NewSettings = _currentSettings,
                    TriggeringMetrics = GetCurrentPerformanceMetrics(),
                    Timestamp = DateTime.Now
                });
            }
            
            if (_configuration.EnableDebugLogging)
            {
                Debug.Log("[AutoLODController] Reset to highest quality");
            }
        }
        
        /// <summary>
        /// Get performance history for LOD decision analysis.
        /// </summary>
        public AutoLODPerformanceHistory GetPerformanceHistory()
        {
            var samples = _performanceHistory.ToArray();
            
            if (samples.Length == 0)
            {
                return new AutoLODPerformanceHistory
                {
                    RecentSamples = new AutoLODPerformanceMetrics[0],
                    AverageFPS = 0f,
                    MinimumFPS = 0f,
                    MaximumFPS = 0f,
                    FPSStandardDeviation = 0f,
                    LODAdjustmentCount = _lodHistory.Count,
                    LODHistory = _lodHistory.ToArray(),
                    HistoryTimeSpan = TimeSpan.Zero
                };
            }
            
            var fpsSamples = samples.Select(s => s.CurrentFPS).ToArray();
            var avgFPS = fpsSamples.Average();
            var minFPS = fpsSamples.Min();
            var maxFPS = fpsSamples.Max();
            var fpsStdDev = CalculateStandardDeviation(fpsSamples);
            
            var timeSpan = samples.Length > 1 ? 
                samples.Last().Timestamp - samples.First().Timestamp : 
                TimeSpan.Zero;
            
            return new AutoLODPerformanceHistory
            {
                RecentSamples = samples,
                AverageFPS = avgFPS,
                MinimumFPS = minFPS,
                MaximumFPS = maxFPS,
                FPSStandardDeviation = fpsStdDev,
                LODAdjustmentCount = _lodHistory.Count,
                LODHistory = _lodHistory.ToArray(),
                HistoryTimeSpan = timeSpan
            };
        }
        
        /// <summary>
        /// Configure bubble count adjustment parameters.
        /// </summary>
        public void ConfigureBubbleCountAdjustment(BubbleCountAdjustmentConfig bubbleConfig)
        {
            _bubbleConfig = bubbleConfig;
            
            // Clamp current bubble count to new limits
            _currentBubbleCount = Mathf.Clamp(_currentBubbleCount, bubbleConfig.MinBubbleCount, bubbleConfig.MaxBubbleCount);
            _targetBubbleCount = Mathf.Clamp(_targetBubbleCount, bubbleConfig.MinBubbleCount, bubbleConfig.MaxBubbleCount);
            
            if (_configuration.EnableDebugLogging)
            {
                Debug.Log($"[AutoLODController] Bubble count adjustment configured - " +
                         $"Range: {bubbleConfig.MinBubbleCount}-{bubbleConfig.MaxBubbleCount}, " +
                         $"Per LOD: {bubbleConfig.BubblesPerLODLevel}");
            }
        }
        
        /// <summary>
        /// Get the current bubble count adjustment status.
        /// </summary>
        public BubbleCountStatus GetBubbleCountStatus()
        {
            return new BubbleCountStatus
            {
                CurrentBubbleCount = _currentBubbleCount,
                TargetBubbleCount = _targetBubbleCount,
                MaxBubbleCount = _bubbleConfig.MaxBubbleCount,
                BubblesReduced = _bubbleConfig.MaxBubbleCount - _currentBubbleCount,
                IsAdjusting = Mathf.Abs(_currentBubbleCount - _targetBubbleCount) > 0.1f,
                TimeSinceLastChange = _timeSinceLastAdjustment
            };
        }
        
        /// <summary>
        /// Clean up Auto-LOD controller resources.
        /// </summary>
        public void Dispose()
        {
            _isInitialized = false;
            _isAutoLODEnabled = false;
            _performanceHistory.Clear();
            _lodHistory.Clear();
            
            if (_configuration.EnableDebugLogging)
            {
                Debug.Log("[AutoLODController] Disposed");
            }
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        #region Private Helper Methods
        
        private void EvaluateAndAdjustLOD(AutoLODPerformanceMetrics metrics)
        {
            // Check if we're below target FPS
            if (metrics.AverageFPS < _configuration.TargetFPS)
            {
                _consecutiveFramesBelowTarget++;
            }
            else
            {
                _consecutiveFramesBelowTarget = 0;
            }
            
            // Check if adjustment is needed and cooldown has passed
            bool shouldAdjust = false;
            int targetLODLevel = _currentLODLevel;
            string adjustmentReason = "";
            
            if (_consecutiveFramesBelowTarget >= _configuration.FrameThresholdCount && 
                _timeSinceLastAdjustment >= _configuration.AdjustmentCooldownSeconds)
            {
                // Performance is consistently below target
                if (metrics.AverageFPS < _configuration.MinimumFPS)
                {
                    // Critical performance issue - aggressive adjustment
                    int aggressiveIncrease = Mathf.CeilToInt(2f * _configuration.AdjustmentAggressiveness);
                    targetLODLevel = Mathf.Min(_currentLODLevel + aggressiveIncrease, _configuration.MaxLODLevel);
                    adjustmentReason = $"Critical FPS ({metrics.AverageFPS:F1}) - aggressive adjustment";
                    shouldAdjust = true;
                }
                else
                {
                    // Standard performance adjustment
                    targetLODLevel = Mathf.Min(_currentLODLevel + 1, _configuration.MaxLODLevel);
                    adjustmentReason = $"Below target FPS ({metrics.AverageFPS:F1})";
                    shouldAdjust = true;
                }
            }
            else if (metrics.AverageFPS > _configuration.TargetFPS + 15f && 
                     _currentLODLevel > 0 && 
                     _timeSinceLastAdjustment >= _configuration.AdjustmentCooldownSeconds * 2f)
            {
                // Performance is well above target - can increase quality
                targetLODLevel = Mathf.Max(_currentLODLevel - 1, 0);
                adjustmentReason = $"Well above target FPS ({metrics.AverageFPS:F1}) - increasing quality";
                shouldAdjust = true;
            }
            
            if (shouldAdjust && targetLODLevel != _currentLODLevel)
            {
                AdjustLODLevel(targetLODLevel, adjustmentReason, metrics);
            }
        }
        
        private void AdjustLODLevel(int newLevel, string reason, AutoLODPerformanceMetrics triggeringMetrics)
        {
            var previousLevel = _currentLODLevel;
            _currentLODLevel = newLevel;
            _currentSettings = GetLODSettingsForLevel(newLevel);
            
            // Apply the new settings
            ApplyLODSettings(_currentSettings);
            
            // Reset tracking variables
            _consecutiveFramesBelowTarget = 0;
            _timeSinceLastAdjustment = 0f;
            
            // Record the change
            RecordLODChange(previousLevel, newLevel, reason);
            
            if (_configuration.EnableDebugLogging)
            {
                Debug.Log($"[AutoLODController] LOD adjusted from {previousLevel} to {newLevel}: {reason}");
            }
            
            // Fire event
            LODLevelChanged?.Invoke(new AutoLODLevelChangedEventArgs
            {
                PreviousLevel = previousLevel,
                NewLevel = newLevel,
                Reason = reason,
                NewSettings = _currentSettings,
                TriggeringMetrics = triggeringMetrics,
                Timestamp = DateTime.Now
            });
        }
        
        private LODSettings GetLODSettingsForLevel(int level)
        {
            // Define LOD settings for each level
            switch (level)
            {
                case 0: return LODSettings.HighestQuality;
                case 1: return CreateLODSettings(1, 90, 0.9f, 0.9f, 0.9f, 0.8f, 0.9f, 0.9f);
                case 2: return LODSettings.MediumQuality;
                case 3: return CreateLODSettings(3, 60, 0.7f, 0.6f, 0.7f, 0.4f, 0.6f, 0.7f);
                case 4: return LODSettings.PerformanceFocused;
                case 5: return CreateLODSettings(5, 35, 0.5f, 0.4f, 0.5f, 0.2f, 0.3f, 0.4f);
                case 6: return CreateLODSettings(6, 25, 0.4f, 0.3f, 0.4f, 0.1f, 0.2f, 0.3f);
                default: return LODSettings.PerformanceFocused;
            }
        }
        
        private LODSettings CreateLODSettings(int level, int bubbleCount, float rendering, float effects, 
            float texture, float shadow, float antiAliasing, float postProcessing)
        {
            return new LODSettings
            {
                LODLevel = level,
                BubbleCount = Mathf.Clamp(bubbleCount, _bubbleConfig.MinBubbleCount, _bubbleConfig.MaxBubbleCount),
                RenderingQuality = rendering,
                EffectQuality = effects,
                TextureQuality = texture,
                ShadowQuality = shadow,
                AntiAliasingQuality = antiAliasing,
                PostProcessingQuality = postProcessing,
                EnableDynamicBatching = level <= 4,
                EnableGPUInstancing = true,
                MaxDrawDistance = Mathf.Lerp(1000f, 300f, level / 6f)
            };
        }
        
        private void SetTargetBubbleCount(int targetCount)
        {
            var previousCount = _targetBubbleCount;
            _targetBubbleCount = Mathf.Clamp(targetCount, _bubbleConfig.MinBubbleCount, _bubbleConfig.MaxBubbleCount);
            
            if (previousCount != _targetBubbleCount)
            {
                BubbleCountAdjusted?.Invoke(new BubbleCountAdjustedEventArgs
                {
                    PreviousCount = previousCount,
                    NewCount = _targetBubbleCount,
                    Reason = $"LOD level {_currentLODLevel} adjustment",
                    TriggeringLODLevel = _currentLODLevel,
                    Timestamp = DateTime.Now
                });
            }
        }
        
        private void UpdateBubbleCountSmoothing()
        {
            if (Mathf.Abs(_currentBubbleCount - _targetBubbleCount) > 0.1f)
            {
                _currentBubbleCount = Mathf.SmoothDamp(_currentBubbleCount, _targetBubbleCount, 
                    ref _bubbleCountSmoothingVelocity, _bubbleConfig.CountChangeSmoothingFactor);
                
                // Apply the bubble count change to the actual bubble system
                ApplyBubbleCountChange(Mathf.RoundToInt(_currentBubbleCount));
            }
        }
        
        private void ApplyBubbleCountChange(int newCount)
        {
            // In a real implementation, this would interface with the bubble management system
            // For now, we'll just log the change
            if (_configuration.EnableDebugLogging && Mathf.Abs(newCount - _currentBubbleCount) > 1f)
            {
                Debug.Log($"[AutoLODController] Bubble count adjusted to {newCount}");
            }
        }
        
        private void ApplyRenderingQuality(LODSettings settings)
        {
            // Apply rendering quality settings
            // In a real implementation, this would adjust Unity's quality settings
            QualitySettings.renderPipeline = null; // Example - would set appropriate render pipeline
            
            // Adjust texture quality
            QualitySettings.globalTextureMipmapLimit = Mathf.RoundToInt((1f - settings.TextureQuality) * 3f);
            
            // Adjust shadow quality
            if (settings.ShadowQuality > 0.8f)
                QualitySettings.shadows = ShadowQuality.All;
            else if (settings.ShadowQuality > 0.5f)
                QualitySettings.shadows = ShadowQuality.HardOnly;
            else if (settings.ShadowQuality > 0.2f)
                QualitySettings.shadows = ShadowQuality.HardOnly;
            else
                QualitySettings.shadows = ShadowQuality.Disable;
        }
        
        private void ApplyEffectQuality(LODSettings settings)
        {
            // Apply effect quality settings
            // In a real implementation, this would adjust particle systems, post-processing, etc.
            
            // Adjust anti-aliasing
            if (settings.AntiAliasingQuality > 0.8f)
                QualitySettings.antiAliasing = 8;
            else if (settings.AntiAliasingQuality > 0.6f)
                QualitySettings.antiAliasing = 4;
            else if (settings.AntiAliasingQuality > 0.3f)
                QualitySettings.antiAliasing = 2;
            else
                QualitySettings.antiAliasing = 0;
        }
        
        private void CheckPerformanceThresholds(AutoLODPerformanceMetrics metrics)
        {
            // Check FPS thresholds
            if (metrics.CurrentFPS < _configuration.MinimumFPS)
            {
                FirePerformanceThresholdEvent(PerformanceThresholdType.MinimumFPS, metrics, 
                    _configuration.MinimumFPS, false);
            }
            else if (metrics.CurrentFPS < _configuration.TargetFPS)
            {
                FirePerformanceThresholdEvent(PerformanceThresholdType.TargetFPS, metrics, 
                    _configuration.TargetFPS, false);
            }
            
            // Check CPU/GPU usage thresholds
            if (metrics.CPUUsage > 90f)
            {
                FirePerformanceThresholdEvent(PerformanceThresholdType.CPUUsage, metrics, 90f, true);
            }
            
            if (metrics.GPUUsage > 90f)
            {
                FirePerformanceThresholdEvent(PerformanceThresholdType.GPUUsage, metrics, 90f, true);
            }
        }
        
        private void FirePerformanceThresholdEvent(PerformanceThresholdType thresholdType, 
            AutoLODPerformanceMetrics metrics, float thresholdValue, bool crossedUpward)
        {
            PerformanceThresholdCrossed?.Invoke(new AutoLODPerformanceEventArgs
            {
                ThresholdType = thresholdType,
                CurrentMetrics = metrics,
                ThresholdValue = thresholdValue,
                CrossedUpward = crossedUpward,
                Timestamp = DateTime.Now
            });
        }
        
        private void RecordLODChange(int previousLevel, int newLevel, string reason)
        {
            var change = new LODLevelChange
            {
                PreviousLevel = previousLevel,
                NewLevel = newLevel,
                Timestamp = DateTime.Now,
                Reason = reason,
                MetricsAtChange = GetCurrentPerformanceMetrics()
            };
            
            _lodHistory.Add(change);
            
            // Keep history manageable
            if (_lodHistory.Count > 100)
            {
                _lodHistory.RemoveAt(0);
            }
        }
        
        private AutoLODPerformanceMetrics GetCurrentPerformanceMetrics()
        {
            return _performanceHistory.Count > 0 ? _performanceHistory.Last() : new AutoLODPerformanceMetrics
            {
                CurrentFPS = _smoothedFPS,
                AverageFPS = _smoothedFPS,
                FrameTimeMs = 1000f / math.max(_smoothedFPS, 1f),
                CPUUsage = _smoothedCPUUsage,
                GPUUsage = _smoothedGPUUsage,
                FrameRateVariance = 0f,
                PerformanceScore = 50f,
                Timestamp = DateTime.Now
            };
        }
        
        private float CalculateFrameRateVariance()
        {
            if (_performanceHistory.Count < 10)
                return 0f;
            
            var recentSamples = _performanceHistory.TakeLast(10).Select(m => m.CurrentFPS).ToArray();
            var mean = recentSamples.Average();
            var variance = recentSamples.Select(fps => (fps - mean) * (fps - mean)).Average();
            
            return Mathf.Sqrt(variance);
        }
        
        private float CalculatePerformanceScore(float fps, float cpuUsage, float gpuUsage)
        {
            // Calculate a performance score from 0-100
            float fpsScore = Mathf.Clamp01(fps / _configuration.TargetFPS) * 40f;
            float cpuScore = Mathf.Clamp01(1f - cpuUsage / 100f) * 30f;
            float gpuScore = Mathf.Clamp01(1f - gpuUsage / 100f) * 30f;
            
            return fpsScore + cpuScore + gpuScore;
        }
        
        private PerformanceTrend AnalyzePerformanceTrend()
        {
            if (_performanceHistory.Count < 20)
                return PerformanceTrend.Unknown;
            
            var recentSamples = _performanceHistory.TakeLast(20).Select(m => m.CurrentFPS).ToArray();
            var firstHalf = recentSamples.Take(10).Average();
            var secondHalf = recentSamples.Skip(10).Average();
            
            float difference = secondHalf - firstHalf;
            float variance = CalculateStandardDeviation(recentSamples);
            
            if (variance > 10f)
                return PerformanceTrend.Volatile;
            else if (difference > 5f)
                return PerformanceTrend.Improving;
            else if (difference < -5f)
                return PerformanceTrend.Degrading;
            else
                return PerformanceTrend.Stable;
        }
        
        private QualityImpactAssessment AssessQualityImpact(int targetLODLevel)
        {
            float levelDifference = targetLODLevel - _currentLODLevel;
            float impactMagnitude = Mathf.Abs(levelDifference) / (float)_configuration.MaxLODLevel;
            
            var noticeableChanges = new List<string>();
            
            if (levelDifference > 0)
            {
                // Quality reduction
                if (levelDifference >= 2)
                    noticeableChanges.Add("Significant reduction in bubble count");
                if (levelDifference >= 1)
                    noticeableChanges.Add("Reduced visual effects quality");
                if (levelDifference >= 3)
                    noticeableChanges.Add("Lower texture resolution");
            }
            else if (levelDifference < 0)
            {
                // Quality improvement
                noticeableChanges.Add("Improved visual quality");
                if (Mathf.Abs(levelDifference) >= 2)
                    noticeableChanges.Add("Increased bubble count");
            }
            
            return new QualityImpactAssessment
            {
                OverallImpact = impactMagnitude,
                BubbleQualityImpact = impactMagnitude * 0.6f,
                LightingImpact = impactMagnitude * 0.4f,
                EffectsImpact = impactMagnitude * 0.8f,
                NoticeableChanges = noticeableChanges.ToArray()
            };
        }
        
        private string GenerateRecommendationReasoning(float currentFPS, float targetFPS, int recommendedLevel)
        {
            if (currentFPS < targetFPS)
            {
                return $"Current FPS ({currentFPS:F1}) is below target ({targetFPS:F1}). " +
                       $"Recommending LOD level {recommendedLevel} to improve performance.";
            }
            else if (currentFPS > targetFPS + 10f && recommendedLevel < _currentLODLevel)
            {
                return $"Current FPS ({currentFPS:F1}) is well above target ({targetFPS:F1}). " +
                       $"Recommending LOD level {recommendedLevel} to increase visual quality.";
            }
            else
            {
                return $"Current performance ({currentFPS:F1} FPS) is acceptable. " +
                       $"Maintaining LOD level {recommendedLevel}.";
            }
        }
        
        private float CalculateStandardDeviation(float[] values)
        {
            if (values.Length <= 1)
                return 0f;
            
            float mean = values.Average();
            float sumSquaredDifferences = values.Sum(v => (v - mean) * (v - mean));
            return Mathf.Sqrt(sumSquaredDifferences / (values.Length - 1));
        }
        
        #endregion
    }
}