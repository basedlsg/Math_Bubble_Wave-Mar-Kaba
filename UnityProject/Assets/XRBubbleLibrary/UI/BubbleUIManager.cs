using UnityEngine;
using UnityEngine.UIElements;
using XRBubbleLibrary.Mathematics;
using System.Collections.Generic;
using System.Collections;

namespace XRBubbleLibrary.UI
{
    /// <summary>
    /// Main UI manager for bubble-based spatial interface
    /// Cloned from Unity UI Toolkit samples, adapted for 3D spatial bubbles
    /// </summary>
    public class BubbleUIManager : MonoBehaviour
    {
        [Header("Bubble Management")]
        public GameObject bubblePrefab;
        public Transform bubbleContainer;
        public int maxBubbles = 100;
        
        [Header("Layout Configuration")]
        public SpatialBubbleLayout spatialLayout;
        public WaveParameters waveParameters = WaveParameters.Default;
        
        [Header("Color System")]
        public Gradient neonPastelGradient;
        public float colorTransitionSpeed = 2.0f;
        
        [Header("Performance")]
        public bool enableObjectPooling = true;
        public bool enableLODSystem = true;
        public float cullingDistance = 10.0f;
        
        // Bubble management
        private List<BubbleUIElement> activeBubbles = new List<BubbleUIElement>();
        private Queue<BubbleUIElement> bubblePool = new Queue<BubbleUIElement>();
        private Dictionary<string, BubbleUIElement> bubbleRegistry = new Dictionary<string, BubbleUIElement>();
        
        // Animation and timing
        private float animationTime = 0.0f;
        private Coroutine colorAnimationCoroutine;
        
        // Performance tracking
        private int frameCount = 0;
        private float lastPerformanceCheck = 0.0f;
        
        void Start()
        {
            InitializeBubbleUI();
            SetupColorGradient();
            StartColorAnimation();
        }
        
        void Update()
        {
            animationTime += Time.deltaTime;
            UpdateBubbleAnimations();
            UpdatePerformanceOptimization();
        }
        
        /// <summary>
        /// Initialize the bubble UI system
        /// </summary>
        void InitializeBubbleUI()
        {
            // Set up bubble container if not assigned
            if (bubbleContainer == null)
            {
                GameObject containerObj = new GameObject("BubbleContainer");
                containerObj.transform.SetParent(transform);
                bubbleContainer = containerObj.transform;
            }
            
            // Initialize spatial layout if not assigned
            if (spatialLayout == null)
            {
                spatialLayout = GetComponent<SpatialBubbleLayout>();
                if (spatialLayout == null)
                {
                    spatialLayout = gameObject.AddComponent<SpatialBubbleLayout>();
                }
            }
            
            // Pre-populate bubble pool if pooling is enabled
            if (enableObjectPooling)
            {
                PrePopulateBubblePool();
            }
            
            Debug.Log($"BubbleUIManager initialized with {maxBubbles} max bubbles");
        }
        
        /// <summary>
        /// Set up default neon-pastel color gradient
        /// </summary>
        void SetupColorGradient()
        {
            if (neonPastelGradient == null)
            {
                neonPastelGradient = new Gradient();
                
                // Create neon-pastel gradient with HDR colors
                GradientColorKey[] colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(new Color(1f, 0.4f, 0.8f), 0.0f);    // Neon Pink
                colorKeys[1] = new GradientColorKey(new Color(0.8f, 0.4f, 1f), 0.25f);   // Neon Purple
                colorKeys[2] = new GradientColorKey(new Color(0.4f, 0.8f, 1f), 0.5f);    // Neon Blue
                colorKeys[3] = new GradientColorKey(new Color(0.4f, 1f, 0.8f), 0.75f);   // Neon Teal
                colorKeys[4] = new GradientColorKey(new Color(1f, 0.8f, 0.4f), 1.0f);    // Neon Orange
                
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(0.8f, 0.0f);
                alphaKeys[1] = new GradientAlphaKey(0.8f, 1.0f);
                
                neonPastelGradient.SetKeys(colorKeys, alphaKeys);
            }
        }
        
        /// <summary>
        /// Start color animation coroutine
        /// </summary>
        void StartColorAnimation()
        {
            if (colorAnimationCoroutine != null)
            {
                StopCoroutine(colorAnimationCoroutine);
            }
            
            colorAnimationCoroutine = StartCoroutine(AnimateColors());
        }
        
        /// <summary>
        /// Pre-populate bubble pool for performance
        /// </summary>
        void PrePopulateBubblePool()
        {
            if (bubblePrefab == null)
            {
                Debug.LogWarning("BubbleUIManager: No bubble prefab assigned, cannot pre-populate pool");
                return;
            }
            
            int poolSize = Mathf.Min(maxBubbles / 2, 20); // Pre-create half or 20, whichever is smaller
            
            for (int i = 0; i < poolSize; i++)
            {
                GameObject bubbleObj = Instantiate(bubblePrefab, bubbleContainer);
                BubbleUIElement bubble = bubbleObj.GetComponent<BubbleUIElement>();
                
                if (bubble != null)
                {
                    bubble.Initialize(this);
                    bubble.gameObject.SetActive(false);
                    bubblePool.Enqueue(bubble);
                }
                else
                {
                    Debug.LogError($"BubbleUIManager: Prefab does not have BubbleUIElement component!");
                    Destroy(bubbleObj);
                }
            }
            
            Debug.Log($"Pre-populated bubble pool with {poolSize} bubbles");
        }
        
        /// <summary>
        /// Create a new bubble at specified position with text
        /// </summary>
        public BubbleUIElement CreateBubble(Vector3 position, string text = "", string id = null)
        {
            BubbleUIElement bubble = GetBubbleFromPool();
            
            if (bubble == null)
            {
                Debug.LogWarning("BubbleUIManager: Could not get bubble from pool, max bubbles reached");
                return null;
            }
            
            // Set up bubble
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
            }
            
            bubble.SetId(id);
            bubble.SetPosition(position);
            bubble.SetText(text);
            bubble.gameObject.SetActive(true);
            
            // Add to active bubbles and registry
            activeBubbles.Add(bubble);
            bubbleRegistry[id] = bubble;
            
            // Set initial color
            Color bubbleColor = neonPastelGradient.Evaluate(Random.Range(0f, 1f));
            bubble.SetColor(bubbleColor);
            
            return bubble;
        }
        
        /// <summary>
        /// Get bubble from pool or create new one
        /// </summary>
        BubbleUIElement GetBubbleFromPool()
        {
            BubbleUIElement bubble = null;
            
            // Try to get from pool first
            if (enableObjectPooling && bubblePool.Count > 0)
            {
                bubble = bubblePool.Dequeue();
            }
            // Create new if under limit
            else if (activeBubbles.Count < maxBubbles && bubblePrefab != null)
            {
                GameObject bubbleObj = Instantiate(bubblePrefab, bubbleContainer);
                bubble = bubbleObj.GetComponent<BubbleUIElement>();
                
                if (bubble != null)
                {
                    bubble.Initialize(this);
                }
                else
                {
                    Debug.LogError("BubbleUIManager: Prefab does not have BubbleUIElement component!");
                    Destroy(bubbleObj);
                }
            }
            
            return bubble;
        }
        
        /// <summary>
        /// Remove bubble and return to pool
        /// </summary>
        public void RemoveBubble(BubbleUIElement bubble)
        {
            if (bubble == null) return;
            
            // Remove from active list and registry
            activeBubbles.Remove(bubble);
            bubbleRegistry.Remove(bubble.GetId());
            
            // Clean up bubble state
            bubble.Cleanup();
            
            // Return to pool or destroy
            if (enableObjectPooling)
            {
                bubble.gameObject.SetActive(false);
                bubblePool.Enqueue(bubble);
            }
            else
            {
                Destroy(bubble.gameObject);
            }
        }
        
        /// <summary>
        /// Remove bubble by ID
        /// </summary>
        public void RemoveBubble(string id)
        {
            if (bubbleRegistry.TryGetValue(id, out BubbleUIElement bubble))
            {
                RemoveBubble(bubble);
            }
        }
        
        /// <summary>
        /// Get bubble by ID
        /// </summary>
        public BubbleUIElement GetBubble(string id)
        {
            bubbleRegistry.TryGetValue(id, out BubbleUIElement bubble);
            return bubble;
        }
        
        /// <summary>
        /// Update bubble animations
        /// </summary>
        void UpdateBubbleAnimations()
        {
            foreach (var bubble in activeBubbles)
            {
                if (bubble != null && bubble.gameObject.activeInHierarchy)
                {
                    // Update LOD based on distance if LOD system is enabled
                    if (enableLODSystem)
                    {
                        UpdateBubbleLOD(bubble);
                    }
                }
            }
        }
        
        /// <summary>
        /// Update LOD for individual bubble based on distance
        /// </summary>
        void UpdateBubbleLOD(BubbleUIElement bubble)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;
            
            float distance = Vector3.Distance(bubble.transform.position, mainCamera.transform.position);
            
            int lodLevel;
            if (distance < cullingDistance * 0.3f)
                lodLevel = 0; // High quality
            else if (distance < cullingDistance * 0.6f)
                lodLevel = 1; // Medium quality
            else if (distance < cullingDistance)
                lodLevel = 2; // Low quality
            else
                lodLevel = 3; // Culled
            
            bubble.SetLODLevel(lodLevel);
            
            // Cull distant bubbles
            if (lodLevel >= 3)
            {
                bubble.gameObject.SetActive(false);
            }
            else if (!bubble.gameObject.activeInHierarchy)
            {
                bubble.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// Update performance optimization
        /// </summary>
        void UpdatePerformanceOptimization()
        {
            frameCount++;
            
            // Check performance every second
            if (Time.time - lastPerformanceCheck >= 1.0f)
            {
                float fps = frameCount / (Time.time - lastPerformanceCheck);
                
                // Adjust LOD settings based on performance
                if (fps < 60f && cullingDistance > 3f)
                {
                    cullingDistance *= 0.95f; // Reduce culling distance to improve performance
                }
                else if (fps > 72f && cullingDistance < 15f)
                {
                    cullingDistance *= 1.02f; // Increase culling distance for better quality
                }
                
                frameCount = 0;
                lastPerformanceCheck = Time.time;
            }
        }
        
        /// <summary>
        /// Animate bubble colors over time
        /// </summary>
        IEnumerator AnimateColors()
        {
            while (true)
            {
                float time = Time.time * colorTransitionSpeed;
                
                for (int i = 0; i < activeBubbles.Count; i++)
                {
                    var bubble = activeBubbles[i];
                    if (bubble != null && bubble.gameObject.activeInHierarchy)
                    {
                        // Create slowly shifting color animation
                        float gradientTime = (time + i * 0.1f) % 1.0f;
                        Color newColor = neonPastelGradient.Evaluate(gradientTime);
                        bubble.SetColor(newColor);
                    }
                }
                
                yield return new WaitForSeconds(0.1f); // Update colors 10 times per second
            }
        }
        
        /// <summary>
        /// Clear all bubbles
        /// </summary>
        public void ClearAllBubbles()
        {
            for (int i = activeBubbles.Count - 1; i >= 0; i--)
            {
                RemoveBubble(activeBubbles[i]);
            }
        }
        
        /// <summary>
        /// Get active bubble count
        /// </summary>
        public int GetActiveBubbleCount()
        {
            return activeBubbles.Count;
        }
        
        /// <summary>
        /// Set max bubbles limit
        /// </summary>
        public void SetMaxBubbles(int max)
        {
            maxBubbles = Mathf.Max(1, max);
        }
        
        /// <summary>
        /// Enable/disable object pooling
        /// </summary>
        public void SetObjectPooling(bool enabled)
        {
            enableObjectPooling = enabled;
            
            if (enabled && bubblePool.Count == 0)
            {
                PrePopulateBubblePool();
            }
        }
        
        void OnDestroy()
        {
            if (colorAnimationCoroutine != null)
            {
                StopCoroutine(colorAnimationCoroutine);
            }
        }
    }
}