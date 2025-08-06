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
            UpdateBubbleVisibility();
            CheckPerformance();
        }
        
        /// <summary>
        /// Initialize the bubble UI system
        /// Based on cloned Unity UI Toolkit initialization patterns
        /// </summary>
        void InitializeBubbleUI()
        {
            // Create bubble container if not assigned
            if (bubbleContainer == null)
            {
                GameObject container = new GameObject("BubbleContainer");
                container.transform.SetParent(transform);
                bubbleContainer = container.transform;
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
            
            // Initialize object pool if enabled
            if (enableObjectPooling)
            {
                InitializeBubblePool();
            }
            
            // Create initial set of bubbles for demonstration
            CreateDemonstrationBubbles();
        }
        
        /// <summary>
        /// Initialize object pool for performance
        /// </summary>
        void InitializeBubblePool()
        {
            for (int i = 0; i < maxBubbles; i++)
            {
                GameObject bubbleObj = Instantiate(bubblePrefab, bubbleContainer);
                BubbleUIElement bubbleElement = bubbleObj.GetComponent<BubbleUIElement>();
                
                if (bubbleElement == null)
                {
                    bubbleElement = bubbleObj.AddComponent<BubbleUIElement>();
                }
                
                bubbleElement.Initialize(this);
                bubbleObj.SetActive(false);
                bubblePool.Enqueue(bubbleElement);
            }
        }
        
        /// <summary>
        /// Set up the neon-pastel color gradient
        /// </summary>
        void SetupColorGradient()
        {
            if (neonPastelGradient == null)
            {
                neonPastelGradient = new Gradient();
                
                // Create neon-pastel color keys
                GradientColorKey[] colorKeys = new GradientColorKey[4];
                colorKeys[0] = new GradientColorKey(new Color(1.0f, 0.4f, 0.8f), 0.0f);    // Neon Pink
                colorKeys[1] = new GradientColorKey(new Color(0.8f, 0.4f, 1.0f), 0.33f);   // Neon Purple
                colorKeys[2] = new GradientColorKey(new Color(0.4f, 0.4f, 1.0f), 0.66f);   // Neon Blue
                colorKeys[3] = new GradientColorKey(new Color(0.4f, 1.0f, 1.0f), 1.0f);    // Neon Teal
                
                // Create alpha keys for transparency
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(0.7f, 0.0f);
                alphaKeys[1] = new GradientAlphaKey(0.7f, 1.0f);
                
                neonPastelGradient.SetKeys(colorKeys, alphaKeys);
            }
        }
        
        /// <summary>
        /// Create demonstration bubbles to show the system
        /// </summary>
        void CreateDemonstrationBubbles()
        {
            // Generate wave pattern positions
            Vector3[] positions = WavePatternGenerator.GenerateWavePattern(20, 1.5f, waveParameters);
            
            for (int i = 0; i < positions.Length; i++)
            {
                string bubbleId = $"demo_bubble_{i}";
                CreateBubble(bubbleId, positions[i], $"Bubble {i + 1}");
            }
        }
        
        /// <summary>
        /// Create a new bubble at specified position
        /// </summary>
        public BubbleUIElement CreateBubble(string id, Vector3 position, string text = "")
        {
            BubbleUIElement bubble = null;
            
            // Get bubble from pool or create new one
            if (enableObjectPooling && bubblePool.Count > 0)
            {
                bubble = bubblePool.Dequeue();
                bubble.gameObject.SetActive(true);
            }
            else if (!enableObjectPooling)
            {
                GameObject bubbleObj = Instantiate(bubblePrefab, bubbleContainer);
                bubble = bubbleObj.GetComponent<BubbleUIElement>();
                if (bubble == null)
                {
                    bubble = bubbleObj.AddComponent<BubbleUIElement>();
                }
                bubble.Initialize(this);
            }
            
            if (bubble != null)
            {
                // Configure bubble
                bubble.SetId(id);
                bubble.SetPosition(position);
                bubble.SetText(text);
                
                // Set color based on position in gradient
                float gradientPosition = WavePatternGenerator.CalculateColorGradientPosition(
                    position, Vector3.zero, 2.0f);
                Color bubbleColor = neonPastelGradient.Evaluate(gradientPosition);
                bubble.SetColor(bubbleColor);
                
                // Add to management collections
                activeBubbles.Add(bubble);
                bubbleRegistry[id] = bubble;
                
                // Add to spatial layout
                spatialLayout.AddBubble(bubble.transform);
            }
            
            return bubble;
        }
        
        /// <summary>
        /// Remove a bubble by ID
        /// </summary>
        public void RemoveBubble(string id)
        {
            if (bubbleRegistry.TryGetValue(id, out BubbleUIElement bubble))
            {
                // Remove from collections
                activeBubbles.Remove(bubble);
                bubbleRegistry.Remove(id);
                spatialLayout.RemoveBubble(bubble.transform);
                
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
        /// Animate bubble colors over time
        /// </summary>
        IEnumerator AnimateColors()
        {
            while (true)
            {
                foreach (var bubble in activeBubbles)
                {
                    if (bubble != null && bubble.gameObject.activeInHierarchy)
                    {
                        // Calculate new gradient position based on time and position
                        float gradientPosition = WavePatternGenerator.CalculateColorGradientPosition(
                            bubble.transform.position, Vector3.zero, 2.0f);
                        
                        // Add time-based offset for animation
                        gradientPosition += (animationTime * colorTransitionSpeed * 0.1f) % 1.0f;
                        gradientPosition = gradientPosition % 1.0f;
                        
                        // Apply new color
                        Color newColor = neonPastelGradient.Evaluate(gradientPosition);
                        bubble.SetColor(newColor);
                    }
                }
                
                yield return new WaitForSeconds(0.1f); // Update colors 10 times per second
            }
        }
        
        /// <summary>
        /// Update bubble visibility based on distance and performance
        /// </summary>
        void UpdateBubbleVisibility()
        {
            if (!enableLODSystem) return;
            
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;
            
            Vector3 cameraPosition = mainCamera.transform.position;
            
            foreach (var bubble in activeBubbles)
            {
                if (bubble != null)
                {
                    float distance = Vector3.Distance(cameraPosition, bubble.transform.position);
                    bool shouldBeVisible = distance <= cullingDistance;
                    
                    if (bubble.gameObject.activeSelf != shouldBeVisible)
                    {
                        bubble.gameObject.SetActive(shouldBeVisible);
                    }
                    
                    // Set LOD level based on distance
                    if (shouldBeVisible)
                    {
                        int lodLevel = distance < cullingDistance * 0.3f ? 0 : 
                                      distance < cullingDistance * 0.6f ? 1 : 2;
                        bubble.SetLODLevel(lodLevel);
                    }
                }
            }
        }
        
        /// <summary>
        /// Check performance and adjust quality if needed
        /// </summary>
        void CheckPerformance()
        {
            frameCount++;
            
            if (Time.time - lastPerformanceCheck >= 1.0f)
            {
                float fps = frameCount / (Time.time - lastPerformanceCheck);
                
                // Adjust quality based on performance
                if (fps < 60.0f && enableLODSystem)
                {
                    // Reduce culling distance to improve performance
                    cullingDistance = Mathf.Max(cullingDistance * 0.9f, 3.0f);
                }
                else if (fps > 90.0f && cullingDistance < 10.0f)
                {
                    // Increase culling distance if performance is good
                    cullingDistance = Mathf.Min(cullingDistance * 1.1f, 10.0f);
                }
                
                frameCount = 0;
                lastPerformanceCheck = Time.time;
            }
        }
        
        /// <summary>
        /// Refresh all bubble positions and colors
        /// </summary>
        public void RefreshAllBubbles()
        {
            spatialLayout.RefreshLayout();
            
            foreach (var bubble in activeBubbles)
            {
                if (bubble != null)
                {
                    // Recalculate color based on new position
                    float gradientPosition = WavePatternGenerator.CalculateColorGradientPosition(
                        bubble.transform.position, Vector3.zero, 2.0f);
                    Color bubbleColor = neonPastelGradient.Evaluate(gradientPosition);
                    bubble.SetColor(bubbleColor);
                }
            }
        }
        
        /// <summary>
        /// Get all active bubbles
        /// </summary>
        public List<BubbleUIElement> GetActiveBubbles()
        {
            return new List<BubbleUIElement>(activeBubbles);
        }
        
        /// <summary>
        /// Clear all bubbles
        /// </summary>
        public void ClearAllBubbles()
        {
            List<string> bubbleIds = new List<string>(bubbleRegistry.Keys);
            foreach (string id in bubbleIds)
            {
                RemoveBubble(id);
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