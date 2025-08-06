using UnityEngine;
using TMPro;

namespace XRBubbleLibrary.WordBubbles
{
    /// <summary>
    /// Specialized text renderer for word bubbles
    /// Handles text sizing, positioning, and readability optimization for VR
    /// </summary>
    public class BubbleTextRenderer : MonoBehaviour
    {
        [Header("Text Component")]
        [SerializeField] private TextMeshPro textMesh;
        
        [Header("Text Settings")]
        [SerializeField] private BubbleTextSettings textSettings = BubbleTextSettings.Default;
        
        [Header("Auto-Sizing")]
        [SerializeField] private bool enableAutoSizing = true;
        [SerializeField] private bool maintainAspectRatio = true;
        
        // Internal state
        private string currentText = "";
        private Vector3 originalLocalPosition;
        private float originalFontSize;
        
        void Awake()
        {
            InitializeTextRenderer();
        }
        
        void InitializeTextRenderer()
        {
            // Get or create TextMeshPro component
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMeshPro>();
                
                if (textMesh == null)
                {
                    textMesh = gameObject.AddComponent<TextMeshPro>();
                }
            }
            
            // Store original values
            originalLocalPosition = transform.localPosition;
            originalFontSize = textMesh.fontSize;
            
            // Apply initial settings
            ApplyTextSettings();
        }
        
        void ApplyTextSettings()
        {
            if (textMesh == null) return;
            
            // Basic text properties
            textMesh.color = textSettings.textColor;
            textMesh.fontSize = textSettings.fontSize;
            textMesh.fontStyle = textSettings.fontStyle;
            textMesh.alignment = textSettings.alignment;
            
            // Auto-sizing
            textMesh.enableAutoSizing = enableAutoSizing;
            if (enableAutoSizing)
            {
                textMesh.fontSizeMin = textSettings.minFontSize;
                textMesh.fontSizeMax = textSettings.maxFontSize;
            }
            
            // Text container settings
            textMesh.enableWordWrapping = textSettings.enableWordWrapping;
            textMesh.overflowMode = textSettings.overflowMode;
            
            // VR optimization settings
            textMesh.enableCulling = true;
            textMesh.isOrthographic = false;
            
            // Material settings for VR clarity
            if (textMesh.fontMaterial != null)
            {
                textMesh.fontMaterial.SetFloat("_OutlineWidth", textSettings.outlineWidth);
                textMesh.fontMaterial.SetColor("_OutlineColor", textSettings.outlineColor);
            }
        }
        
        /// <summary>
        /// Set the text content and optimize for display
        /// </summary>
        public void SetText(string text)
        {
            if (textMesh == null) return;
            
            currentText = text;
            textMesh.text = text;
            
            // Optimize text display
            OptimizeTextDisplay();
        }
        
        /// <summary>
        /// Update text settings and apply them
        /// </summary>
        public void UpdateTextSettings(BubbleTextSettings newSettings)
        {
            textSettings = newSettings;
            ApplyTextSettings();
            
            // Re-optimize current text
            if (!string.IsNullOrEmpty(currentText))
            {
                OptimizeTextDisplay();
            }
        }
        
        /// <summary>
        /// Optimize text display for VR readability
        /// </summary>
        void OptimizeTextDisplay()
        {
            if (textMesh == null || string.IsNullOrEmpty(currentText)) return;
            
            // Force text mesh to update
            textMesh.ForceMeshUpdate();
            
            // Adjust positioning based on text bounds
            if (textSettings.centerTextInBubble)
            {
                CenterTextInBubble();
            }
            
            // Adjust font size based on text length if needed
            if (textSettings.adaptiveFontSizing)
            {
                AdaptFontSizeToContent();
            }
            
            // Apply VR-specific optimizations
            ApplyVROptimizations();
        }
        
        /// <summary>
        /// Center text within the bubble bounds
        /// </summary>
        void CenterTextInBubble()
        {
            if (textMesh == null) return;
            
            // Get text bounds
            Bounds textBounds = textMesh.bounds;
            
            // Calculate offset to center text
            Vector3 offset = -textBounds.center + transform.position;
            
            // Apply offset while maintaining original local position
            transform.localPosition = originalLocalPosition + transform.InverseTransformVector(offset);
        }
        
        /// <summary>
        /// Adapt font size based on content length
        /// </summary>
        void AdaptFontSizeToContent()
        {
            if (textMesh == null || !enableAutoSizing) return;
            
            int textLength = currentText.Length;
            
            // Adjust font size based on text length
            float sizeMultiplier = 1f;
            
            if (textLength > 10)
            {
                sizeMultiplier = Mathf.Lerp(1f, 0.7f, (textLength - 10) / 20f);
            }
            else if (textLength < 4)
            {
                sizeMultiplier = Mathf.Lerp(1.2f, 1f, textLength / 4f);
            }
            
            float adaptedSize = textSettings.fontSize * sizeMultiplier;
            adaptedSize = Mathf.Clamp(adaptedSize, textSettings.minFontSize, textSettings.maxFontSize);
            
            textMesh.fontSize = adaptedSize;
        }
        
        /// <summary>
        /// Apply VR-specific text optimizations
        /// </summary>
        void ApplyVROptimizations()
        {
            if (textMesh == null) return;
            
            // Ensure text faces the camera/user
            if (textSettings.alwaysFaceUser)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    Vector3 directionToCamera = mainCamera.transform.position - transform.position;
                    directionToCamera.y = 0; // Keep text upright
                    
                    if (directionToCamera != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(-directionToCamera);
                    }
                }
            }
            
            // Adjust text quality based on distance to user
            if (textSettings.distanceBasedQuality)
            {
                AdjustQualityBasedOnDistance();
            }
        }
        
        /// <summary>
        /// Adjust text quality based on distance to user
        /// </summary>
        void AdjustQualityBasedOnDistance()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || textMesh == null) return;
            
            float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
            
            // Adjust text quality based on distance
            if (distance > textSettings.highQualityDistance)
            {
                // Reduce quality for distant text
                textMesh.enableCulling = true;
            }
            else
            {
                // High quality for close text
                textMesh.enableCulling = false;
            }
        }
        
        /// <summary>
        /// Get the current text bounds
        /// </summary>
        public Bounds GetTextBounds()
        {
            if (textMesh != null)
            {
                textMesh.ForceMeshUpdate();
                return textMesh.bounds;
            }
            return new Bounds();
        }
        
        /// <summary>
        /// Get the current text mesh component
        /// </summary>
        public TextMeshPro GetTextMesh()
        {
            return textMesh;
        }
        
        /// <summary>
        /// Set text color
        /// </summary>
        public void SetTextColor(Color color)
        {
            if (textMesh != null)
            {
                textMesh.color = color;
                textSettings.textColor = color;
            }
        }
        
        /// <summary>
        /// Set font size
        /// </summary>
        public void SetFontSize(float size)
        {
            if (textMesh != null)
            {
                textMesh.fontSize = size;
                textSettings.fontSize = size;
            }
        }
        
        /// <summary>
        /// Animate text appearance (for future use)
        /// </summary>
        public void AnimateTextAppearance(float duration = 0.5f)
        {
            // Placeholder for text animation
            // Could implement typewriter effect, fade-in, etc.
        }
        
        // Debug visualization
        void OnDrawGizmos()
        {
            if (textMesh != null && Application.isPlaying)
            {
                // Draw text bounds
                Gizmos.color = Color.yellow;
                Bounds bounds = GetTextBounds();
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}