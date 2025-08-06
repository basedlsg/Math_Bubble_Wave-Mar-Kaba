using UnityEngine;
using Unity.Mathematics;
using XRBubbleLibrary.WaveMatrix;
using TMPro;

namespace XRBubbleLibrary.WordBubbles
{
    /// <summary>
    /// Core word bubble component that displays text within a breathing, translucent bubble
    /// Integrates with wave matrix positioning and breathing animation systems
    /// </summary>
    public class WordBubble : MonoBehaviour, IBreathingElement
    {
        [Header("Word Content")]
        [SerializeField] private string word = "Hello";
        [SerializeField] private float aiConfidence = 0.5f; // 0-1, affects positioning distance
        
        [Header("Visual Components")]
        [SerializeField] private TextMeshPro textComponent;
        [SerializeField] private Renderer bubbleRenderer;
        [SerializeField] private Collider bubbleCollider;
        
        [Header("Bubble Settings")]
        [SerializeField] private BubbleVisualSettings visualSettings = BubbleVisualSettings.Default;
        
        // Internal state
        private Vector3 originalScale;
        private Color originalTextColor;
        private Material bubbleMaterial;
        private MaterialPropertyBlock propertyBlock;
        private bool isInteracted = false;
        private bool isSelected = false;
        
        // Breathing animation state
        private BreathingElementConfig breathingConfig;
        
        // Events
        public System.Action<WordBubble> OnBubbleSelected;
        public System.Action<WordBubble> OnBubbleHovered;
        public System.Action<WordBubble> OnBubbleUnhovered;
        
        #region Properties
        
        public string Word 
        { 
            get => word; 
            set => SetWord(value); 
        }
        
        public float AIConfidence 
        { 
            get => aiConfidence; 
            set => SetAIConfidence(value); 
        }
        
        public bool IsInteracted => isInteracted;
        public bool IsSelected => isSelected;
        public Vector3 Position => transform.position;
        
        #endregion
        
        void Awake()
        {
            InitializeBubble();
        }
        
        void Start()
        {
            SetupBubbleComponents();
            RegisterForBreathing();
        }
        
        void InitializeBubble()
        {
            // Store original values
            originalScale = transform.localScale;
            
            // Get or create components
            if (textComponent == null)
                textComponent = GetComponentInChildren<TextMeshPro>();
            
            if (bubbleRenderer == null)
                bubbleRenderer = GetComponent<Renderer>();
            
            if (bubbleCollider == null)
                bubbleCollider = GetComponent<Collider>();
            
            // Create MaterialPropertyBlock for performance
            propertyBlock = new MaterialPropertyBlock();
            
            // Create breathing configuration
            breathingConfig = new BreathingElementConfig
            {
                scaleIntensity = visualSettings.breathingIntensity,
                opacityIntensity = visualSettings.opacityBreathingIntensity,
                baseOpacity = visualSettings.baseOpacity
            };
        }
        
        void SetupBubbleComponents()
        {
            // Setup text component
            if (textComponent != null)
            {
                textComponent.text = word;
                textComponent.fontSize = visualSettings.fontSize;
                textComponent.color = visualSettings.textColor;
                textComponent.alignment = TextAlignmentOptions.Center;
                textComponent.enableAutoSizing = true;
                textComponent.fontSizeMin = visualSettings.minFontSize;
                textComponent.fontSizeMax = visualSettings.maxFontSize;
                
                originalTextColor = textComponent.color;
            }
            
            // Setup bubble material
            if (bubbleRenderer != null)
            {
                bubbleMaterial = CreateBubbleMaterial();
                bubbleRenderer.material = bubbleMaterial;
            }
            
            // Setup collider for interaction
            if (bubbleCollider == null)
            {
                var sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = visualSettings.colliderRadius;
                bubbleCollider = sphereCollider;
            }
        }
        
        Material CreateBubbleMaterial()
        {
            // Create glass-like material
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Glass properties
            material.color = visualSettings.bubbleColor;
            material.SetFloat("_Metallic", visualSettings.metallic);
            material.SetFloat("_Smoothness", visualSettings.smoothness);
            
            // Transparency
            material.SetFloat("_Surface", 1); // Transparent
            material.SetFloat("_Blend", 0); // Alpha blend
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = 3000;
            
            return material;
        }
        
        void RegisterForBreathing()
        {
            // Find breathing system and register
            var breathingSystem = FindObjectOfType<BreathingAnimationSystem>();
            if (breathingSystem != null)
            {
                breathingSystem.RegisterBreathingElement(this, breathingConfig);
            }
        }
        
        #region Public Methods
        
        public void SetWord(string newWord)
        {
            word = newWord;
            if (textComponent != null)
            {
                textComponent.text = word;
            }
        }
        
        public void SetAIConfidence(float confidence)
        {
            aiConfidence = Mathf.Clamp01(confidence);
            
            // Update visual feedback based on confidence
            UpdateConfidenceVisuals();
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateSelectionVisuals();
        }
        
        public void SetInteracted(bool interacted)
        {
            isInteracted = interacted;
            UpdateInteractionVisuals();
        }
        
        public void UpdateVisualSettings(BubbleVisualSettings newSettings)
        {
            visualSettings = newSettings;
            
            // Update breathing config
            breathingConfig.scaleIntensity = visualSettings.breathingIntensity;
            breathingConfig.opacityIntensity = visualSettings.opacityBreathingIntensity;
            breathingConfig.baseOpacity = visualSettings.baseOpacity;
            
            // Update visual components
            if (textComponent != null)
            {
                textComponent.fontSize = visualSettings.fontSize;
                textComponent.color = visualSettings.textColor;
            }
            
            if (bubbleMaterial != null)
            {
                bubbleMaterial.color = visualSettings.bubbleColor;
                bubbleMaterial.SetFloat("_Metallic", visualSettings.metallic);
                bubbleMaterial.SetFloat("_Smoothness", visualSettings.smoothness);
            }
        }
        
        #endregion
        
        #region Visual Updates
        
        void UpdateConfidenceVisuals()
        {
            if (bubbleMaterial != null)
            {
                // Higher confidence = more opaque, brighter
                Color confidenceColor = visualSettings.bubbleColor;
                confidenceColor.a = Mathf.Lerp(0.3f, 0.8f, aiConfidence);
                
                // Slight color shift based on confidence
                float hue, sat, val;
                Color.RGBToHSV(confidenceColor, out hue, out sat, out val);
                val = Mathf.Lerp(0.6f, 1.0f, aiConfidence);
                confidenceColor = Color.HSVToRGB(hue, sat, val);
                confidenceColor.a = Mathf.Lerp(0.3f, 0.8f, aiConfidence);
                
                bubbleMaterial.color = confidenceColor;
            }
        }
        
        void UpdateSelectionVisuals()
        {
            if (isSelected)
            {
                // Selection glow effect
                if (bubbleMaterial != null)
                {
                    Color selectedColor = visualSettings.selectedColor;
                    bubbleMaterial.color = selectedColor;
                }
                
                if (textComponent != null)
                {
                    textComponent.color = visualSettings.selectedTextColor;
                }
            }
            else
            {
                // Return to normal colors
                UpdateConfidenceVisuals();
                if (textComponent != null)
                {
                    textComponent.color = originalTextColor;
                }
            }
        }
        
        void UpdateInteractionVisuals()
        {
            if (isInteracted && !isSelected)
            {
                // Hover effect
                if (bubbleMaterial != null)
                {
                    Color hoverColor = visualSettings.hoverColor;
                    bubbleMaterial.color = hoverColor;
                }
            }
            else if (!isSelected)
            {
                // Return to confidence-based color
                UpdateConfidenceVisuals();
            }
        }
        
        #endregion
        
        #region IBreathingElement Implementation
        
        public void ApplyBreathing(BreathingValues values)
        {
            // Use MaterialPropertyBlock for performance instead of Transform scaling
            if (bubbleRenderer != null && propertyBlock != null)
            {
                // Set breathing scale in shader
                propertyBlock.SetFloat("_BreathScale", values.scale);
                propertyBlock.SetFloat("_BreathOpacity", values.opacity * breathingConfig.baseOpacity);
                bubbleRenderer.SetPropertyBlock(propertyBlock);
            }
            
            // Fallback to transform scaling if shader doesn't support breathing
            if (bubbleMaterial != null && !bubbleMaterial.HasProperty("_BreathScale"))
            {
                transform.localScale = originalScale * values.scale;
                
                Color currentColor = bubbleMaterial.color;
                currentColor.a = values.opacity * breathingConfig.baseOpacity;
                bubbleMaterial.color = currentColor;
            }
            
            // Subtle text breathing (less intense than bubble)
            if (textComponent != null)
            {
                Color textColor = textComponent.color;
                textColor.a = Mathf.Lerp(0.8f, 1.0f, (values.breathingPhase + 1f) * 0.5f);
                textComponent.color = textColor;
            }
        }
        
        #endregion
        
        #region Interaction Events
        
        void OnTriggerEnter(Collider other)
        {
            if (IsValidInteractor(other))
            {
                SetInteracted(true);
                OnBubbleHovered?.Invoke(this);
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (IsValidInteractor(other))
            {
                SetInteracted(false);
                OnBubbleUnhovered?.Invoke(this);
            }
        }
        
        bool IsValidInteractor(Collider other)
        {
            // Check for VR controller, hand, or other interaction objects
            return other.CompareTag("Hand") || 
                   other.CompareTag("Controller") || 
                   other.CompareTag("Interactor");
        }
        
        public void OnSelect()
        {
            SetSelected(true);
            OnBubbleSelected?.Invoke(this);
        }
        
        #endregion
        
        void OnDestroy()
        {
            // Unregister from breathing system
            var breathingSystem = FindObjectOfType<BreathingAnimationSystem>();
            if (breathingSystem != null)
            {
                breathingSystem.UnregisterBreathingElement(this);
            }
        }
        
        // Debug visualization
        void OnDrawGizmos()
        {
            if (bubbleCollider != null)
            {
                Gizmos.color = isSelected ? Color.green : (isInteracted ? Color.yellow : Color.cyan);
                Gizmos.DrawWireSphere(transform.position, visualSettings.colliderRadius);
            }
        }
    }
}