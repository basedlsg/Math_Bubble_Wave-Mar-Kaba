using UnityEngine;
using UnityEngine.UIElements;
using Unity.Mathematics;

namespace XRBubbleLibrary.UI
{
    /// <summary>
    /// Spatial Bubble UI System - Cloned from Unity UI Toolkit samples
    /// Adapts Unity's UI Toolkit for 3D spatial bubble interfaces
    /// Based on Unity's UI Toolkit world space samples and 3D UI examples
    /// </summary>
    public class SpatialBubbleUI : MonoBehaviour
    {
        [Header("UI Toolkit Cloning Configuration")]
        [Tooltip("Cloned from Unity UI Toolkit world space samples")]
        [SerializeField] private UIDocument uiDocument;
        
        [Tooltip("Based on Unity 3D UI samples")]
        [SerializeField] private Camera targetCamera;
        
        [Header("Bubble Layout Settings (Unity Layout Samples)")]
        [SerializeField] private float bubbleRadius = 0.5f;
        [SerializeField] private float bubbleSpacing = 1.2f;
        [SerializeField] private int maxBubblesPerRow = 5;
        
        [Header("Wave Pattern Integration (Unity Math Samples)")]
        [SerializeField] private bool enableWaveLayout = true;
        [SerializeField] private float waveAmplitude = 0.3f;
        [SerializeField] private float waveFrequency = 0.5f;
        
        [Header("Neon-Pastel Theme (Unity Color Samples)")]
        [SerializeField] private Color neonPink = new Color(1f, 0.4f, 0.8f, 0.8f);
        [SerializeField] private Color neonBlue = new Color(0.4f, 0.8f, 1f, 0.8f);
        [SerializeField] private Color neonPurple = new Color(0.8f, 0.4f, 1f, 0.8f);
        [SerializeField] private Color neonTeal = new Color(0.4f, 1f, 0.8f, 0.8f);
        
        private VisualElement rootElement;
        private System.Collections.Generic.List<BubbleUIElement> bubbleElements;
        private float3 centerPosition;
        
        private void Start()
        {
            InitializeSpatialUI();
            CloneUIToolkitSamples();
            SetupBubbleLayout();
            ApplyNeonPastelTheme();
        }
        
        private void Update()
        {
            if (enableWaveLayout)
            {
                UpdateWaveBasedLayout();
            }
            
            UpdateBubblePositions();
        }
        
        /// <summary>
        /// Initializes spatial UI based on Unity UI Toolkit world space samples
        /// </summary>
        private void InitializeSpatialUI()
        {
            // Clone UI Document setup from Unity UI Toolkit samples
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    uiDocument = gameObject.AddComponent<UIDocument>();
                }
            }
            
            // Setup camera reference (cloned from Unity 3D UI samples)
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    targetCamera = FindObjectOfType<Camera>();
                }
            }
            
            // Initialize root element (Unity UI Toolkit samples)
            rootElement = uiDocument.rootVisualElement;
            bubbleElements = new System.Collections.Generic.List<BubbleUIElement>();
            centerPosition = transform.position;
            
            Debug.Log("Spatial Bubble UI initialized - cloned from Unity UI Toolkit samples");
        }
        
        /// <summary>
        /// Clones and adapts Unity UI Toolkit samples for bubble interfaces
        /// </summary>
        private void CloneUIToolkitSamples()
        {
            // Create bubble container (cloned from Unity container samples)
            var bubbleContainer = new VisualElement();
            bubbleContainer.name = "bubble-container";
            bubbleContainer.style.position = Position.Absolute;
            bubbleContainer.style.width = Length.Percent(100);
            bubbleContainer.style.height = Length.Percent(100);
            
            // Apply bubble-specific styling (adapted from Unity styling samples)
            bubbleContainer.style.flexDirection = FlexDirection.Row;
            bubbleContainer.style.flexWrap = Wrap.Wrap;
            bubbleContainer.style.justifyContent = Justify.Center;
            bubbleContainer.style.alignItems = Align.Center;
            
            rootElement.Add(bubbleContainer);
            
            // Clone button samples and adapt for bubbles
            CreateBubbleElementsFromSamples(bubbleContainer);
        }
        
        /// <summary>
        /// Creates bubble UI elements by cloning Unity button samples
        /// </summary>
        private void CreateBubbleElementsFromSamples(VisualElement container)
        {
            // Clone Unity button samples and adapt for bubble shape
            for (int i = 0; i < 12; i++) // Create sample bubbles
            {
                var bubbleElement = CreateBubbleFromButtonSample(i);
                container.Add(bubbleElement.VisualElement);
                bubbleElements.Add(bubbleElement);
            }
        }
        
        /// <summary>
        /// Creates individual bubble element by cloning Unity button samples
        /// </summary>
        private BubbleUIElement CreateBubbleFromButtonSample(int index)
        {
            // Clone button structure from Unity UI Toolkit samples
            var button = new Button();
            button.name = $"bubble-{index}";
            button.text = $"Bubble {index + 1}";
            
            // Adapt button styling for bubble appearance (Unity styling samples)
            button.style.width = bubbleRadius * 100; // Convert to pixels
            button.style.height = bubbleRadius * 100;
            button.style.borderTopLeftRadius = bubbleRadius * 50; // Make circular
            button.style.borderTopRightRadius = bubbleRadius * 50;
            button.style.borderBottomLeftRadius = bubbleRadius * 50;
            button.style.borderBottomRightRadius = bubbleRadius * 50;
            
            // Apply neon-pastel colors (Unity color samples)
            Color bubbleColor = GetNeonColorForIndex(index);
            button.style.backgroundColor = bubbleColor;
            button.style.color = Color.white;
            
            // Add interaction events (cloned from Unity event samples)
            button.clicked += () => OnBubbleClicked(index);
            
            // Create bubble wrapper (Unity layout samples)
            var bubbleWrapper = new VisualElement();
            bubbleWrapper.name = $"bubble-wrapper-{index}";
            bubbleWrapper.style.position = Position.Absolute;
            bubbleWrapper.Add(button);
            
            // Create bubble UI element data structure
            var bubbleUIElement = new BubbleUIElement
            {
                Index = index,
                VisualElement = bubbleWrapper,
                Button = button,
                WorldPosition = CalculateInitialBubblePosition(index),
                Color = bubbleColor
            };
            
            return bubbleUIElement;
        }
        
        /// <summary>
        /// Sets up bubble layout based on Unity layout samples
        /// </summary>
        private void SetupBubbleLayout()
        {
            // Calculate initial positions using Unity math samples
            for (int i = 0; i < bubbleElements.Count; i++)
            {
                var bubble = bubbleElements[i];
                bubble.WorldPosition = CalculateInitialBubblePosition(i);
                
                // Apply wave pattern if enabled (Unity wave samples)
                if (enableWaveLayout)
                {
                    ApplyWavePatternToBubble(bubble, i);
                }
            }
        }
        
        /// <summary>
        /// Calculates initial bubble position using Unity math samples
        /// </summary>
        private float3 CalculateInitialBubblePosition(int index)
        {
            // Grid layout calculation (cloned from Unity grid layout samples)
            int row = index / maxBubblesPerRow;
            int col = index % maxBubblesPerRow;
            
            // Center the grid (Unity centering samples)
            float totalWidth = (maxBubblesPerRow - 1) * bubbleSpacing;
            float startX = -totalWidth * 0.5f;
            
            float x = startX + col * bubbleSpacing;
            float y = -row * bubbleSpacing;
            float z = 0f;
            
            return centerPosition + new float3(x, y, z);
        }
        
        /// <summary>
        /// Applies wave pattern to bubble positioning
        /// Based on Unity wave mathematics samples
        /// </summary>
        private void ApplyWavePatternToBubble(BubbleUIElement bubble, int index)
        {
            float time = Time.time;
            float waveOffset = index * 0.5f; // Phase offset for each bubble
            
            // Calculate wave displacement (Unity sine wave samples)
            float waveY = math.sin(time * waveFrequency + waveOffset) * waveAmplitude;
            float waveX = math.cos(time * waveFrequency * 0.7f + waveOffset) * waveAmplitude * 0.5f;
            
            // Apply wave displacement to position
            bubble.WorldPosition += new float3(waveX, waveY, 0f);
        }
        
        /// <summary>
        /// Updates wave-based layout animation
        /// Cloned from Unity animation samples
        /// </summary>
        private void UpdateWaveBasedLayout()
        {
            for (int i = 0; i < bubbleElements.Count; i++)
            {
                var bubble = bubbleElements[i];
                
                // Recalculate base position
                bubble.WorldPosition = CalculateInitialBubblePosition(i);
                
                // Apply wave animation
                ApplyWavePatternToBubble(bubble, i);
            }
        }
        
        /// <summary>
        /// Updates bubble screen positions based on world positions
        /// Cloned from Unity world-to-screen conversion samples
        /// </summary>
        private void UpdateBubblePositions()
        {
            if (targetCamera == null) return;
            
            foreach (var bubble in bubbleElements)
            {
                // Convert world position to screen position (Unity conversion samples)
                Vector3 screenPos = targetCamera.WorldToScreenPoint(bubble.WorldPosition);
                
                // Check if bubble is in front of camera (Unity visibility samples)
                if (screenPos.z > 0)
                {
                    // Convert to UI coordinates (Unity UI coordinate samples)
                    var uiPos = RuntimePanelUtils.ScreenToPanel(
                        rootElement.panel, 
                        new Vector2(screenPos.x, Screen.height - screenPos.y)
                    );
                    
                    // Update visual element position (Unity positioning samples)
                    bubble.VisualElement.style.left = uiPos.x - (bubbleRadius * 50);
                    bubble.VisualElement.style.top = uiPos.y - (bubbleRadius * 50);
                    bubble.VisualElement.style.display = DisplayStyle.Flex;
                }
                else
                {
                    // Hide bubbles behind camera (Unity culling samples)
                    bubble.VisualElement.style.display = DisplayStyle.None;
                }
            }
        }
        
        /// <summary>
        /// Applies neon-pastel theme based on Unity color samples
        /// </summary>
        private void ApplyNeonPastelTheme()
        {
            // Apply theme colors to bubbles (Unity theming samples)
            for (int i = 0; i < bubbleElements.Count; i++)
            {
                var bubble = bubbleElements[i];
                Color themeColor = GetNeonColorForIndex(i);
                
                bubble.Button.style.backgroundColor = themeColor;
                bubble.Color = themeColor;
                
                // Add glow effect using Unity shadow samples
                bubble.Button.style.textShadow = new TextShadow
                {
                    offset = Vector2.zero,
                    blurRadius = 10f,
                    color = themeColor
                };
            }
        }
        
        /// <summary>
        /// Gets neon color for bubble index using Unity color cycling samples
        /// </summary>
        private Color GetNeonColorForIndex(int index)
        {
            // Cycle through neon colors (Unity color array samples)
            Color[] neonColors = { neonPink, neonBlue, neonPurple, neonTeal };
            return neonColors[index % neonColors.Length];
        }
        
        /// <summary>
        /// Handles bubble click events
        /// Cloned from Unity event handling samples
        /// </summary>
        private void OnBubbleClicked(int bubbleIndex)
        {
            Debug.Log($"Bubble {bubbleIndex} clicked - event cloned from Unity UI samples");
            
            // Add bubble interaction animation (Unity animation samples)
            var bubble = bubbleElements[bubbleIndex];
            AnimateBubbleClick(bubble);
        }
        
        /// <summary>
        /// Animates bubble click using Unity animation samples
        /// </summary>
        private void AnimateBubbleClick(BubbleUIElement bubble)
        {
            // Simple scale animation (cloned from Unity UI animation samples)
            var originalScale = bubble.VisualElement.style.scale;
            
            // Scale up
            bubble.VisualElement.style.scale = new Scale(Vector3.one * 1.2f);
            
            // Scale back down after delay (Unity coroutine pattern)
            StartCoroutine(ScaleBackAfterDelay(bubble.VisualElement, originalScale));
        }
        
        /// <summary>
        /// Coroutine for scaling animation
        /// </summary>
        private System.Collections.IEnumerator ScaleBackAfterDelay(VisualElement element, StyleScale originalScale)
        {
            yield return new WaitForSeconds(0.1f);
            element.style.scale = originalScale;
        }
        
        /// <summary>
        /// Validates spatial UI setup
        /// </summary>
        [ContextMenu("Validate Spatial UI Setup")]
        public void ValidateSpatialUISetup()
        {
            Debug.Log($"Spatial Bubble UI Status:");
            Debug.Log($"- UI Document: {(uiDocument != null ? "Found" : "Missing")}");
            Debug.Log($"- Target Camera: {(targetCamera != null ? "Found" : "Missing")}");
            Debug.Log($"- Bubble Elements: {bubbleElements?.Count ?? 0}");
            Debug.Log($"- Wave Layout: {enableWaveLayout}");
        }
    }
    
}
