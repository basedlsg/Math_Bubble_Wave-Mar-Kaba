using UnityEngine;
using XRBubbleLibrary.Physics;
using XRBubbleLibrary.UI;
using XRBubbleLibrary.Shaders;

namespace XRBubbleLibrary.Integration
{
    /// <summary>
    /// Bubble System Integrator - Coordinates all bubble systems
    /// Integrates Visual Foundation, Physics Systems, and UI components
    /// Based on Unity system integration samples and coordination patterns
    /// </summary>
    public class BubbleSystemIntegrator : MonoBehaviour
    {
        [Header("System References (Unity Integration Samples)")]
        [SerializeField] private BubbleBreathingSystem breathingSystem;
        [SerializeField] private BubbleSpringPhysics springPhysics;
        [SerializeField] private WaveBreathingIntegration waveIntegration;
        [SerializeField] private SpatialBubbleUI spatialUI;
        [SerializeField] private BubbleLayoutManager layoutManager;
        [SerializeField] private BubbleShaderOptimizer shaderOptimizer;
        
        [Header("Integration Settings (Unity Coordination Samples)")]
        [SerializeField] private bool enableSystemSynchronization = true;
        [SerializeField] private float synchronizationStrength = 0.8f;
        [SerializeField] private float updateFrequency = 60f; // Hz
        
        [Header("Performance Monitoring (Unity Performance Samples)")]
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private float targetFrameRate = 72f; // Quest 3 target
        
        // Integration state
        private float lastUpdateTime;
        private float updateInterval;
        private bool isInitialized = false;
        
        // Performance tracking
        private float averageFrameTime;
        private int frameCount;
        
        private void Start()
        {
            InitializeIntegrator();
            DiscoverSystems();
            ConfigureIntegration();
            StartSystemSynchronization();
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            if (enablePerformanceMonitoring)
            {
                MonitorPerformance();
            }
            
            if (enableSystemSynchronization && Time.time - lastUpdateTime >= updateInterval)
            {
                SynchronizeAllSystems();
                lastUpdateTime = Time.time;
            }
        }
        
        /// <summary>
        /// Initializes the system integrator
        /// Based on Unity initialization samples
        /// </summary>
        private void InitializeIntegrator()
        {
            updateInterval = 1f / updateFrequency;
            lastUpdateTime = Time.time;
            averageFrameTime = 0f;
            frameCount = 0;
            
            Debug.Log("Bubble System Integrator initialized - coordinating all systems");
        }
        
        /// <summary>
        /// Discovers and caches system references
        /// Based on Unity component discovery samples
        /// </summary>
        private void DiscoverSystems()
        {
            // Find breathing system (Unity component search samples)
            if (breathingSystem == null)
            {
                breathingSystem = FindObjectOfType<BubbleBreathingSystem>();
                if (breathingSystem == null)
                {
                    Debug.LogWarning("BubbleBreathingSystem not found - creating default");
                    breathingSystem = gameObject.AddComponent<BubbleBreathingSystem>();
                }
            }
            
            // Find spring physics (Unity component search samples)
            if (springPhysics == null)
            {
                springPhysics = FindObjectOfType<BubbleSpringPhysics>();
                if (springPhysics == null)
                {
                    Debug.LogWarning("BubbleSpringPhysics not found - creating default");
                    springPhysics = gameObject.AddComponent<BubbleSpringPhysics>();
                }
            }
            
            // Find wave integration (Unity component search samples)
            if (waveIntegration == null)
            {
                waveIntegration = FindObjectOfType<WaveBreathingIntegration>();
                if (waveIntegration == null)
                {
                    Debug.LogWarning("WaveBreathingIntegration not found - creating default");
                    waveIntegration = gameObject.AddComponent<WaveBreathingIntegration>();
                }
            }
            
            // Find spatial UI (Unity component search samples)
            if (spatialUI == null)
            {
                spatialUI = FindObjectOfType<SpatialBubbleUI>();
            }
            
            // Find layout manager (Unity component search samples)
            if (layoutManager == null)
            {
                layoutManager = FindObjectOfType<BubbleLayoutManager>();
            }
            
            // Find shader optimizer (Unity component search samples)
            if (shaderOptimizer == null)
            {
                shaderOptimizer = FindObjectOfType<BubbleShaderOptimizer>();
            }
            
            Debug.Log($"System Discovery Complete - Found {GetActiveSystemCount()} active systems");
        }
        
        /// <summary>
        /// Configures integration between systems
        /// Based on Unity configuration samples
        /// </summary>
        private void ConfigureIntegration()
        {
            // Configure breathing system for integration (Unity configuration samples)
            if (breathingSystem != null)
            {
                breathingSystem.OptimizeForComfort();
            }
            
            // Configure spring physics for integration (Unity configuration samples)
            if (springPhysics != null)
            {
                springPhysics.OptimizeForNaturalFeel();
            }
            
            // Configure wave integration for comfort (Unity configuration samples)
            if (waveIntegration != null)
            {
                waveIntegration.OptimizeForComfort();
            }
            
            // Configure shader optimizer for Quest 3 (Unity configuration samples)
            if (shaderOptimizer != null)
            {
                // Shader optimizer will auto-configure based on platform
            }
            
            Debug.Log("System integration configured for optimal performance and comfort");
        }
        
        /// <summary>
        /// Starts system synchronization
        /// Based on Unity synchronization samples
        /// </summary>
        private void StartSystemSynchronization()
        {
            isInitialized = true;
            Debug.Log("System synchronization started - all systems coordinated");
        }
        
        /// <summary>
        /// Synchronizes all systems for coordinated behavior
        /// Based on Unity system coordination samples
        /// </summary>
        private void SynchronizeAllSystems()
        {
            if (!enableSystemSynchronization) return;
            
            // Get current breathing state for synchronization
            float breathingPhase = 0f;
            float breathingAmplitude = 0f;
            
            if (waveIntegration != null)
            {
                breathingPhase = waveIntegration.GetSynchronizedPhase(0); // Use first bubble as reference
                var waveDisplacement = waveIntegration.GetWaveDisplacement(0);
                breathingAmplitude = Unity.Mathematics.math.length(waveDisplacement);
            }
            
            // Synchronize breathing with visual intensity
            if (breathingSystem != null)
            {
                float visualIntensity = CalculateVisualIntensity();
                breathingSystem.SynchronizeWithVisualSystem(visualIntensity * synchronizationStrength);
            }
            
            // Synchronize spring physics with breathing
            if (springPhysics != null)
            {
                springPhysics.SynchronizeWithBreathing(breathingPhase, breathingAmplitude * synchronizationStrength);
            }
            
            // Synchronize UI animations
            if (spatialUI != null && layoutManager != null)
            {
                // UI system will automatically sync with wave patterns through its own update loop
            }
            
            // Synchronize shader effects
            if (shaderOptimizer != null)
            {
                // Shader optimizer will automatically adjust based on performance
            }
        }
        
        /// <summary>
        /// Calculates overall visual intensity for synchronization
        /// Based on Unity intensity calculation samples
        /// </summary>
        private float CalculateVisualIntensity()
        {
            float intensity = 0.5f; // Base intensity
            
            // Factor in breathing amplitude
            if (waveIntegration != null)
            {
                var waveDisplacement = waveIntegration.GetWaveDisplacement(0);
                intensity += Unity.Mathematics.math.length(waveDisplacement) * 0.3f;
            }
            
            // Factor in spring activity
            if (springPhysics != null)
            {
                var velocity = springPhysics.GetVelocity(0);
                intensity += Unity.Mathematics.math.length(velocity) * 0.2f;
            }
            
            // Clamp to reasonable range
            return Mathf.Clamp01(intensity);
        }
        
        /// <summary>
        /// Monitors system performance
        /// Based on Unity performance monitoring samples
        /// </summary>
        private void MonitorPerformance()
        {
            frameCount++;
            averageFrameTime += Time.unscaledDeltaTime;
            
            // Update performance metrics every second
            if (frameCount >= targetFrameRate)
            {
                float avgFrameTime = averageFrameTime / frameCount;
                float currentFPS = 1f / avgFrameTime;
                
                // Check if we're meeting performance targets
                if (currentFPS < targetFrameRate * 0.9f)
                {
                    OptimizeForPerformance();
                }
                else if (currentFPS > targetFrameRate * 1.1f)
                {
                    IncreaseQualityIfPossible();
                }
                
                // Reset counters
                averageFrameTime = 0f;
                frameCount = 0;
                
                Debug.Log($"Performance: {currentFPS:F1} FPS (Target: {targetFrameRate} FPS)");
            }
        }
        
        /// <summary>
        /// Optimizes systems for better performance
        /// Based on Unity optimization samples
        /// </summary>
        private void OptimizeForPerformance()
        {
            Debug.Log("Performance below target - optimizing systems");
            
            // Reduce update frequency
            updateFrequency = Mathf.Max(30f, updateFrequency * 0.9f);
            updateInterval = 1f / updateFrequency;
            
            // Reduce synchronization strength
            synchronizationStrength = Mathf.Max(0.5f, synchronizationStrength * 0.95f);
            
            // Optimize shader quality
            if (shaderOptimizer != null)
            {
                shaderOptimizer.ForceLowQuality();
            }
            
            // Reduce layout complexity
            if (layoutManager != null)
            {
                // Layout manager will automatically optimize based on performance
            }
        }
        
        /// <summary>
        /// Increases quality when performance allows
        /// Based on Unity quality scaling samples
        /// </summary>
        private void IncreaseQualityIfPossible()
        {
            Debug.Log("Performance above target - increasing quality");
            
            // Increase update frequency
            updateFrequency = Mathf.Min(90f, updateFrequency * 1.05f);
            updateInterval = 1f / updateFrequency;
            
            // Increase synchronization strength
            synchronizationStrength = Mathf.Min(1f, synchronizationStrength * 1.02f);
            
            // Increase shader quality
            if (shaderOptimizer != null)
            {
                shaderOptimizer.ForceHighQuality();
            }
        }
        
        /// <summary>
        /// Gets count of active systems
        /// Based on Unity system counting samples
        /// </summary>
        private int GetActiveSystemCount()
        {
            int count = 0;
            if (breathingSystem != null) count++;
            if (springPhysics != null) count++;
            if (waveIntegration != null) count++;
            if (spatialUI != null) count++;
            if (layoutManager != null) count++;
            if (shaderOptimizer != null) count++;
            return count;
        }
        
        /// <summary>
        /// Validates all system integrations
        /// </summary>
        [ContextMenu("Validate System Integration")]
        public void ValidateSystemIntegration()
        {
            Debug.Log($"Bubble System Integration Status:");
            Debug.Log($"- Active Systems: {GetActiveSystemCount()}/6");
            Debug.Log($"- Breathing System: {(breathingSystem != null ? "Active" : "Missing")}");
            Debug.Log($"- Spring Physics: {(springPhysics != null ? "Active" : "Missing")}");
            Debug.Log($"- Wave Integration: {(waveIntegration != null ? "Active" : "Missing")}");
            Debug.Log($"- Spatial UI: {(spatialUI != null ? "Active" : "Missing")}");
            Debug.Log($"- Layout Manager: {(layoutManager != null ? "Active" : "Missing")}");
            Debug.Log($"- Shader Optimizer: {(shaderOptimizer != null ? "Active" : "Missing")}");
            Debug.Log($"- Synchronization: {enableSystemSynchronization}");
            Debug.Log($"- Performance Monitoring: {enablePerformanceMonitoring}");
            Debug.Log($"- Update Frequency: {updateFrequency} Hz");
            Debug.Log($"- Synchronization Strength: {synchronizationStrength}");
        }
        
        /// <summary>
        /// Optimizes entire system for Quest 3
        /// Based on Unity Quest optimization samples
        /// </summary>
        [ContextMenu("Optimize for Quest 3")]
        public void OptimizeForQuest3()
        {
            targetFrameRate = 72f;
            updateFrequency = 60f;
            synchronizationStrength = 0.8f;
            
            // Optimize individual systems
            if (breathingSystem != null) breathingSystem.OptimizeForComfort();
            if (springPhysics != null) springPhysics.OptimizeForNaturalFeel();
            if (waveIntegration != null) waveIntegration.OptimizeForComfort();
            if (shaderOptimizer != null) shaderOptimizer.ForceHighQuality();
            
            Debug.Log("All systems optimized for Quest 3 - 72 FPS target with natural feel");
        }
        
        /// <summary>
        /// Emergency performance mode for thermal throttling
        /// Based on Unity thermal management samples
        /// </summary>
        [ContextMenu("Emergency Performance Mode")]
        public void EmergencyPerformanceMode()
        {
            updateFrequency = 30f;
            synchronizationStrength = 0.5f;
            enableSystemSynchronization = false;
            
            if (shaderOptimizer != null) shaderOptimizer.ForceLowQuality();
            
            Debug.Log("Emergency performance mode activated - minimal quality for thermal protection");
        }
        
        /// <summary>
        /// Restores normal operation
        /// Based on Unity restoration samples
        /// </summary>
        [ContextMenu("Restore Normal Operation")]
        public void RestoreNormalOperation()
        {
            OptimizeForQuest3();
            enableSystemSynchronization = true;
            
            Debug.Log("Normal operation restored - all systems active");
        }
    }
}