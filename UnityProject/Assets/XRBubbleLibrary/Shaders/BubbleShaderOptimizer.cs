using UnityEngine;
using UnityEngine.Rendering;

namespace XRBubbleLibrary.Shaders
{
    /// <summary>
    /// Bubble Shader Optimizer - Cloned from Unity mobile optimization samples
    /// Optimizes bubble glass shaders for Quest 3 mobile GPU performance
    /// Based on Unity's mobile shader optimization best practices
    /// </summary>
    public class BubbleShaderOptimizer : MonoBehaviour
    {
        [Header("Quest 3 Optimization Settings")]
        [Tooltip("Cloned from Unity mobile LOD samples")]
        [SerializeField] private bool enableLODOptimization = true;
        
        [Tooltip("Based on Unity mobile performance samples")]
        [SerializeField] private float maxRenderDistance = 50f;
        
        [Tooltip("Cloned from Unity mobile batching samples")]
        [SerializeField] private bool enableGPUInstancing = true;
        
        [Header("Shader Quality Settings (Unity Mobile Samples)")]
        [SerializeField] private QualityLevel currentQuality = QualityLevel.High;
        
        [Header("Performance Monitoring (Unity Profiler Samples)")]
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private float targetFrameRate = 72f; // Quest 3 target
        
        private Material[] bubbleMaterials;
        private Camera playerCamera;
        private float lastFrameTime;
        
        public enum QualityLevel
        {
            Low,    // Simplified shaders for thermal throttling
            Medium, // Balanced quality/performance
            High    // Full quality when performance allows
        }
        
        private void Start()
        {
            InitializeOptimizer();
            CacheBubbleMaterials();
            SetupPerformanceMonitoring();
        }
        
        private void Update()
        {
            if (enablePerformanceMonitoring)
            {
                MonitorPerformance();
                AdjustQualityBasedOnPerformance();
            }
            
            if (enableLODOptimization)
            {
                UpdateLODBasedOnDistance();
            }
        }
        
        /// <summary>
        /// Initializes optimizer based on Unity mobile optimization samples
        /// </summary>
        private void InitializeOptimizer()
        {
            // Find player camera (cloned from Unity XR camera samples)
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
            
            // Configure for Quest 3 (based on Unity Android optimization samples)
            if (SystemInfo.deviceModel.Contains("Quest"))
            {
                ConfigureForQuest3();
            }
            
            Debug.Log("Bubble Shader Optimizer initialized - using Unity mobile optimization samples");
        }
        
        /// <summary>
        /// Configures shader settings specifically for Quest 3
        /// Based on Unity's Quest optimization samples
        /// </summary>
        private void ConfigureForQuest3()
        {
            // Quest 3 specific optimizations (cloned from Unity Quest samples)
            QualitySettings.pixelLightCount = 2; // Limit pixel lights
            QualitySettings.shadowDistance = 30f; // Reduce shadow distance
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            
            // Enable GPU instancing for better batching (Unity batching samples)
            if (enableGPUInstancing)
            {
                EnableGPUInstancingOnMaterials();
            }
            
            Debug.Log("Quest 3 shader optimizations applied - based on Unity Quest samples");
        }
        
        /// <summary>
        /// Caches all bubble materials for optimization
        /// Cloned from Unity material management samples
        /// </summary>
        private void CacheBubbleMaterials()
        {
            // Find all renderers with bubble materials (Unity component search samples)
            Renderer[] allRenderers = FindObjectsOfType<Renderer>();
            System.Collections.Generic.List<Material> bubbleMats = new System.Collections.Generic.List<Material>();
            
            foreach (Renderer renderer in allRenderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat != null && mat.shader.name.Contains("BubbleGlass"))
                    {
                        bubbleMats.Add(mat);
                    }
                }
            }
            
            bubbleMaterials = bubbleMats.ToArray();
            Debug.Log($"Cached {bubbleMaterials.Length} bubble materials for optimization");
        }
        
        /// <summary>
        /// Enables GPU instancing on bubble materials
        /// Cloned from Unity GPU instancing samples
        /// </summary>
        private void EnableGPUInstancingOnMaterials()
        {
            foreach (Material mat in bubbleMaterials)
            {
                if (mat != null)
                {
                    // Enable GPU instancing (Unity instancing samples)
                    mat.enableInstancing = true;
                }
            }
        }
        
        /// <summary>
        /// Sets up performance monitoring based on Unity profiler samples
        /// </summary>
        private void SetupPerformanceMonitoring()
        {
            if (enablePerformanceMonitoring)
            {
                // Initialize performance tracking (Unity profiler samples)
                lastFrameTime = Time.unscaledTime;
                
                // Set target frame rate for Quest 3 (Unity mobile samples)
                Application.targetFrameRate = (int)targetFrameRate;
            }
        }
        
        /// <summary>
        /// Monitors performance and adjusts quality accordingly
        /// Based on Unity adaptive quality samples
        /// </summary>
        private void MonitorPerformance()
        {
            float currentFrameTime = Time.unscaledTime;
            float deltaTime = currentFrameTime - lastFrameTime;
            float currentFPS = 1f / deltaTime;
            
            // Check if we're hitting performance targets (Unity performance samples)
            if (currentFPS < targetFrameRate * 0.9f) // 90% of target
            {
                // Performance is low, consider reducing quality
                if (currentQuality > QualityLevel.Low)
                {
                    ReduceShaderQuality();
                }
            }
            else if (currentFPS > targetFrameRate * 1.1f) // 110% of target
            {
                // Performance is good, consider increasing quality
                if (currentQuality < QualityLevel.High)
                {
                    IncreaseShaderQuality();
                }
            }
            
            lastFrameTime = currentFrameTime;
        }
        
        /// <summary>
        /// Adjusts shader quality based on performance
        /// Cloned from Unity adaptive quality samples
        /// </summary>
        private void AdjustQualityBasedOnPerformance()
        {
            foreach (Material mat in bubbleMaterials)
            {
                if (mat != null)
                {
                    // Adjust shader properties based on quality level (Unity quality samples)
                    switch (currentQuality)
                    {
                        case QualityLevel.Low:
                            mat.SetFloat("_GlowIntensity", 0.5f);
                            mat.SetFloat("_FresnelPower", 1.0f);
                            break;
                        case QualityLevel.Medium:
                            mat.SetFloat("_GlowIntensity", 1.0f);
                            mat.SetFloat("_FresnelPower", 1.5f);
                            break;
                        case QualityLevel.High:
                            mat.SetFloat("_GlowIntensity", 1.5f);
                            mat.SetFloat("_FresnelPower", 2.0f);
                            break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates LOD based on distance from camera
        /// Cloned from Unity LOD system samples
        /// </summary>
        private void UpdateLODBasedOnDistance()
        {
            if (playerCamera == null) return;
            
            Vector3 cameraPosition = playerCamera.transform.position;
            
            foreach (Material mat in bubbleMaterials)
            {
                if (mat != null)
                {
                    // Find the renderer using this material (Unity component search)
                    Renderer[] renderers = FindObjectsOfType<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        if (System.Array.IndexOf(renderer.materials, mat) >= 0)
                        {
                            float distance = Vector3.Distance(cameraPosition, renderer.transform.position);
                            float lodFade = Mathf.Clamp01(1f - (distance / maxRenderDistance));
                            
                            // Apply LOD fade (Unity LOD samples)
                            mat.SetFloat("_LODFade", lodFade);
                            
                            // Disable rendering if too far (Unity culling samples)
                            renderer.enabled = distance <= maxRenderDistance;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Reduces shader quality for better performance
        /// </summary>
        private void ReduceShaderQuality()
        {
            if (currentQuality > QualityLevel.Low)
            {
                currentQuality--;
                Debug.Log($"Reduced bubble shader quality to {currentQuality} for better performance");
            }
        }
        
        /// <summary>
        /// Increases shader quality when performance allows
        /// </summary>
        private void IncreaseShaderQuality()
        {
            if (currentQuality < QualityLevel.High)
            {
                currentQuality++;
                Debug.Log($"Increased bubble shader quality to {currentQuality}");
            }
        }
        
        /// <summary>
        /// Manual quality override for testing
        /// </summary>
        [ContextMenu("Force High Quality")]
        public void ForceHighQuality()
        {
            currentQuality = QualityLevel.High;
            AdjustQualityBasedOnPerformance();
        }
        
        [ContextMenu("Force Low Quality")]
        public void ForceLowQuality()
        {
            currentQuality = QualityLevel.Low;
            AdjustQualityBasedOnPerformance();
        }
        
        /// <summary>
        /// Validates shader optimization setup
        /// </summary>
        [ContextMenu("Validate Optimization Setup")]
        public void ValidateOptimizationSetup()
        {
            Debug.Log($"Bubble Shader Optimizer Status:");
            Debug.Log($"- Materials cached: {bubbleMaterials?.Length ?? 0}");
            Debug.Log($"- Current quality: {currentQuality}");
            Debug.Log($"- LOD optimization: {enableLODOptimization}");
            Debug.Log($"- GPU instancing: {enableGPUInstancing}");
            Debug.Log($"- Performance monitoring: {enablePerformanceMonitoring}");
        }
    }
}