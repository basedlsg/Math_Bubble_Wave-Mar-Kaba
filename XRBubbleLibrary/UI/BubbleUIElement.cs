using UnityEngine;
using UnityEngine.UI;
using TMPro;
using XRBubbleLibrary;

namespace XRBubbleLibrary.UI
{
    /// <summary>
    /// Individual bubble UI element with text, interaction, and visual effects
    /// Cloned from Unity UI Toolkit element patterns, adapted for 3D spatial bubbles
    /// </summary>
    [RequireComponent(typeof(BubblePhysics), typeof(BubbleInteraction))]
    public class BubbleUIElement : MonoBehaviour
    {
        [Header("Visual Components")]
        public Renderer bubbleRenderer;
        public TextMeshPro textComponent;
        public Canvas textCanvas;
        
        [Header("Text Configuration")]
        public float textSize = 0.1f;
        public Color textColor = Color.white;
        public bool faceCamera = true;
        
        [Header("Animation")]
        public bool enableTextBreathing = true;
        public float textBreathingIntensity = 0.02f;
        
        // Component references
        private BubblePhysics bubblePhysics;
        private BubbleInteraction bubbleInteraction;
        private BubbleUIManager uiManager;
        private Material bubbleMaterial;
        
        // State
        private string bubbleId;
        private string displayText;
        private Vector3 baseTextScale;
        private int currentLODLevel = 0;
        
        // Animation
        private float animationTime = 0.0f;
        
        void Awake()
        {
            InitializeComponents();
        }
        
        void Update()
        {
            animationTime += Time.deltaTime;
            UpdateTextAnimation();
            UpdateCameraFacing();
        }
        
        /// <summary>
        /// Initialize all required components
        /// </summary>
        void InitializeComponents()
        {
            // Get required components
            bubblePhysics = GetComponent<BubblePhysics>();
            bubbleInteraction = GetComponent<BubbleInteraction>();
            
            // Set up renderer
            if (bubbleRenderer == null)
            {
                bubbleRenderer = GetComponentInChildren<Renderer>();
            }
            
            if (bubbleRenderer != null)
            {
                bubbleMaterial = bubbleRenderer.material;
            }
            
            // Set up text components
            SetupTextComponents();
        }
        
        /// <summary>
        /// Set up text display components
        /// </summary>
        void SetupTextComponents()
        {
            // Create text canvas if not present
            if (textCanvas == null)
            {
                GameObject canvasObj = new GameObject("TextCanvas");
                canvasObj.transform.SetParent(transform);
                canvasObj.transform.localPosition = Vector3.zero;
                canvasObj.transform.localRotation = Quaternion.identity;
                canvasObj.transform.localScale = Vector3.one;
                
                textCanvas = canvasObj.AddComponent<Canvas>();
                textCanvas.renderMode = RenderMode.WorldSpace;
                textCanvas.worldCamera = Camera.main;
                
                // Add CanvasScaler for consistent sizing
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = 100.0f;
                
                // Add GraphicRaycaster for UI interaction
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create text component if not present
            if (textComponent == null)
            {
                GameObject textObj = new GameObject("BubbleText");
                textObj.transform.SetParent(textCanvas.transform);
                textObj.transform.localPosition = Vector3.zero;
                textObj.transform.localRotation = Quaternion.identity;
                textObj.transform.localScale = Vector3.one;
                
                textComponent = textObj.AddComponent<TextMeshProUGUI>();
                
                // Configure text properties
                textComponent.text = "";
                textComponent.fontSize = textSize * 100; // Scale for world space
                textComponent.color = textColor;
                textComponent.alignment = TextAlignmentOptions.Center;
                textComponent.fontStyle = FontStyles.Bold;
                
                // Set up RectTransform
                RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 50);
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            
            baseTextScale = textComponent.transform.localScale;
        }
        
        /// <summary>
        /// Initialize bubble with UI manager reference
        /// </summary>
        public void Initialize(BubbleUIManager manager)
        {
            uiManager = manager;
            
            // Set up interaction callbacks
            if (bubbleInteraction != null)
            {
                // Add custom interaction events here if needed
            }
        }
        
        /// <summary>
        /// Set bubble ID
        /// </summary>
        public void SetId(string id)
        {
            bubbleId = id;
            gameObject.name = $"Bubble_{id}";
        }
        
        /// <summary>
        /// Get bubble ID
        /// </summary>
        public string GetId()
        {
            return bubbleId;
        }
        
        /// <summary>
        /// Set bubble position
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
        
        /// <summary>
        /// Set bubble text content
        /// </summary>
        public void SetText(string text)
        {
            displayText = text;
            if (textComponent != null)
            {
                textComponent.text = text;
                
                // Show/hide text canvas based on content
                if (textCanvas != null)
                {
                    textCanvas.gameObject.SetActive(!string.IsNullOrEmpty(text));
                }
            }
        }
        
        /// <summary>
        /// Get bubble text content
        /// </summary>
        public string GetText()
        {
            return displayText;
        }
        
        /// <summary>
        /// Set bubble color
        /// </summary>
        public void SetColor(Color color)
        {
            if (bubbleMaterial != null)
            {
                bubbleMaterial.SetColor("_BaseColor", color);
                bubbleMaterial.SetColor("_GlowColor", color * 1.5f); // Brighter glow
            }
        }
        
        /// <summary>
        /// Set LOD level for performance optimization
        /// </summary>
        public void SetLODLevel(int lodLevel)
        {
            currentLODLevel = lodLevel;
            
            if (bubbleMaterial != null)
            {
                bubbleMaterial.SetFloat("_LODLevel", lodLevel);
            }
            
            // Adjust text visibility based on LOD
            if (textCanvas != null)
            {
                bool showText = lodLevel <= 1 && !string.IsNullOrEmpty(displayText);
                textCanvas.gameObject.SetActive(showText);
            }
        }
        
        /// <summary>
        /// Update text animation (breathing effect)
        /// </summary>
        void UpdateTextAnimation()
        {
            if (!enableTextBreathing || textComponent == null) return;
            
            // Breathing animation for text
            float breathingPhase = animationTime * 0.3f * 2.0f * Mathf.PI; // 0.3 Hz breathing
            float breathingScale = 1.0f + (Mathf.Sin(breathingPhase) * textBreathingIntensity);
            
            textComponent.transform.localScale = baseTextScale * breathingScale;
        }
        
        /// <summary>
        /// Update text to face camera
        /// </summary>
        void UpdateCameraFacing()
        {
            if (!faceCamera || textCanvas == null) return;
            
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Make text face the camera
                Vector3 directionToCamera = mainCamera.transform.position - textCanvas.transform.position;
                textCanvas.transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
        
        /// <summary>
        /// Set text properties
        /// </summary>
        public void SetTextProperties(float size, Color color, bool faceCamera = true)
        {
            textSize = size;
            textColor = color;
            this.faceCamera = faceCamera;
            
            if (textComponent != null)
            {
                textComponent.fontSize = size * 100;
                textComponent.color = color;
            }
        }
        
        /// <summary>
        /// Enable/disable text breathing animation
        /// </summary>
        public void SetTextBreathing(bool enabled, float intensity = 0.02f)
        {
            enableTextBreathing = enabled;
            textBreathingIntensity = intensity;
        }
        
        /// <summary>
        /// Get current bubble configuration
        /// </summary>
        public BubbleConfiguration GetConfiguration()
        {
            if (bubblePhysics != null)
            {
                // This would need to be implemented in BubblePhysics
                // return bubblePhysics.GetConfiguration();
            }
            
            return BubbleConfiguration.Default;
        }
        
        /// <summary>
        /// Update bubble configuration
        /// </summary>
        public void UpdateConfiguration(BubbleConfiguration config)
        {
            if (bubblePhysics != null)
            {
                bubblePhysics.UpdateConfiguration(config);
            }
        }
        
        /// <summary>
        /// Handle bubble selection event
        /// </summary>
        public void OnBubbleSelected()
        {
            // Notify UI manager of selection
            if (uiManager != null)
            {
                // Custom selection logic can be added here
                Debug.Log($"Bubble {bubbleId} selected: {displayText}");
            }
            
            // Visual feedback for selection
            if (bubbleMaterial != null)
            {
                bubbleMaterial.SetFloat("_GlowIntensity", 1.2f);
            }
        }
        
        /// <summary>
        /// Handle bubble deselection event
        /// </summary>
        public void OnBubbleDeselected()
        {
            // Return to normal visual state
            if (bubbleMaterial != null)
            {
                bubbleMaterial.SetFloat("_GlowIntensity", 0.6f);
            }
        }
        
        /// <summary>
        /// Handle bubble hover enter event
        /// </summary>
        public void OnBubbleHoverEnter()
        {
            // Subtle visual feedback for hover
            if (bubbleMaterial != null)
            {
                bubbleMaterial.SetFloat("_GlowIntensity", 0.8f);
            }
        }
        
        /// <summary>
        /// Handle bubble hover exit event
        /// </summary>
        public void OnBubbleHoverExit()
        {
            // Return to normal state
            if (bubbleMaterial != null)
            {
                bubbleMaterial.SetFloat("_GlowIntensity", 0.6f);
            }
        }
        
        /// <summary>
        /// Cleanup when bubble is destroyed or returned to pool
        /// </summary>
        public void Cleanup()
        {
            bubbleId = "";
            displayText = "";
            
            if (textComponent != null)
            {
                textComponent.text = "";
            }
            
            if (textCanvas != null)
            {
                textCanvas.gameObject.SetActive(false);
            }
            
            // Reset material properties
            if (bubbleMaterial != null)
            {
                bubbleMaterial.SetFloat("_GlowIntensity", 0.6f);
                bubbleMaterial.SetFloat("_LODLevel", 0);
            }
        }
        
        void OnDestroy()
        {
            Cleanup();
        }
    }
}