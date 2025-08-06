using UnityEngine;

namespace XRBubbleLibrary.WordBubbles
{
    /// <summary>
    /// Visual settings for word bubbles
    /// Controls appearance, materials, and visual feedback
    /// </summary>
    [System.Serializable]
    public class BubbleVisualSettings
    {
        [Header("Bubble Material")]
        public Color bubbleColor = new Color(0.3f, 0.7f, 1.0f, 0.6f);
        
        [Range(0f, 1f)]
        public float metallic = 0.1f;
        
        [Range(0f, 1f)]
        public float smoothness = 0.9f;
        
        [Range(0.1f, 1f)]
        public float baseOpacity = 0.7f;
        
        [Header("Text Settings")]
        public Color textColor = Color.white;
        public float fontSize = 0.5f;
        public float minFontSize = 0.2f;
        public float maxFontSize = 1.0f;
        
        [Header("Interaction Colors")]
        public Color hoverColor = new Color(1.0f, 0.8f, 0.3f, 0.8f);
        public Color selectedColor = new Color(0.3f, 1.0f, 0.3f, 0.9f);
        public Color selectedTextColor = Color.black;
        
        [Header("Breathing Animation")]
        [Range(0f, 2f)]
        public float breathingIntensity = 1.0f;
        
        [Range(0f, 1f)]
        public float opacityBreathingIntensity = 0.3f;
        
        [Header("Interaction")]
        public float colliderRadius = 0.6f;
        
        [Header("Size Scaling")]
        public float minScale = 0.8f;
        public float maxScale = 1.5f;
        
        /// <summary>
        /// Default bubble visual settings optimized for VR readability
        /// </summary>
        public static BubbleVisualSettings Default => new BubbleVisualSettings();
        
        /// <summary>
        /// High contrast settings for better readability
        /// </summary>
        public static BubbleVisualSettings HighContrast => new BubbleVisualSettings
        {
            bubbleColor = new Color(0.1f, 0.1f, 0.1f, 0.8f),
            textColor = Color.white,
            hoverColor = new Color(0.2f, 0.2f, 0.2f, 0.9f),
            selectedColor = new Color(0.0f, 0.5f, 1.0f, 0.9f),
            selectedTextColor = Color.white,
            metallic = 0.0f,
            smoothness = 0.3f,
            breathingIntensity = 0.5f,
            opacityBreathingIntensity = 0.2f
        };
        
        /// <summary>
        /// Subtle settings for minimal distraction
        /// </summary>
        public static BubbleVisualSettings Subtle => new BubbleVisualSettings
        {
            bubbleColor = new Color(0.8f, 0.8f, 0.8f, 0.3f),
            textColor = new Color(0.2f, 0.2f, 0.2f, 1.0f),
            hoverColor = new Color(0.9f, 0.9f, 0.9f, 0.5f),
            selectedColor = new Color(0.7f, 0.9f, 1.0f, 0.6f),
            selectedTextColor = new Color(0.1f, 0.1f, 0.1f, 1.0f),
            metallic = 0.0f,
            smoothness = 0.6f,
            breathingIntensity = 0.3f,
            opacityBreathingIntensity = 0.1f,
            fontSize = 0.4f
        };
        
        /// <summary>
        /// Vibrant settings for engaging experience
        /// </summary>
        public static BubbleVisualSettings Vibrant => new BubbleVisualSettings
        {
            bubbleColor = new Color(0.2f, 0.8f, 1.0f, 0.7f),
            textColor = Color.white,
            hoverColor = new Color(1.0f, 0.6f, 0.2f, 0.8f),
            selectedColor = new Color(0.2f, 1.0f, 0.2f, 0.9f),
            selectedTextColor = Color.black,
            metallic = 0.3f,
            smoothness = 0.95f,
            breathingIntensity = 1.5f,
            opacityBreathingIntensity = 0.5f,
            fontSize = 0.6f
        };
        
        /// <summary>
        /// Glass-like settings for premium aesthetic
        /// </summary>
        public static BubbleVisualSettings Glass => new BubbleVisualSettings
        {
            bubbleColor = new Color(0.9f, 0.95f, 1.0f, 0.4f),
            textColor = new Color(0.1f, 0.3f, 0.6f, 1.0f),
            hoverColor = new Color(0.8f, 0.9f, 1.0f, 0.6f),
            selectedColor = new Color(0.6f, 0.8f, 1.0f, 0.8f),
            selectedTextColor = new Color(0.0f, 0.2f, 0.5f, 1.0f),
            metallic = 0.05f,
            smoothness = 0.98f,
            breathingIntensity = 0.8f,
            opacityBreathingIntensity = 0.4f,
            baseOpacity = 0.5f
        };
        
        /// <summary>
        /// Validate settings to ensure they work well in VR
        /// </summary>
        public void ValidateSettings()
        {
            // Ensure colors have appropriate alpha values
            bubbleColor.a = Mathf.Clamp(bubbleColor.a, 0.1f, 0.9f);
            hoverColor.a = Mathf.Clamp(hoverColor.a, 0.2f, 1.0f);
            selectedColor.a = Mathf.Clamp(selectedColor.a, 0.3f, 1.0f);
            
            // Ensure text is readable
            textColor.a = Mathf.Clamp(textColor.a, 0.7f, 1.0f);
            selectedTextColor.a = Mathf.Clamp(selectedTextColor.a, 0.8f, 1.0f);
            
            // Clamp material properties
            metallic = Mathf.Clamp01(metallic);
            smoothness = Mathf.Clamp01(smoothness);
            baseOpacity = Mathf.Clamp(baseOpacity, 0.1f, 1.0f);
            
            // Clamp font sizes
            fontSize = Mathf.Clamp(fontSize, 0.1f, 2.0f);
            minFontSize = Mathf.Clamp(minFontSize, 0.05f, fontSize);
            maxFontSize = Mathf.Clamp(maxFontSize, fontSize, 3.0f);
            
            // Clamp breathing settings
            breathingIntensity = Mathf.Clamp(breathingIntensity, 0f, 3f);
            opacityBreathingIntensity = Mathf.Clamp01(opacityBreathingIntensity);
            
            // Clamp interaction settings
            colliderRadius = Mathf.Clamp(colliderRadius, 0.1f, 2.0f);
            
            // Ensure scale limits make sense
            minScale = Mathf.Clamp(minScale, 0.1f, 1.0f);
            maxScale = Mathf.Clamp(maxScale, 1.0f, 3.0f);
            if (minScale >= maxScale) maxScale = minScale + 0.1f;
        }
        
        /// <summary>
        /// Create a copy of these settings
        /// </summary>
        public BubbleVisualSettings Clone()
        {
            return new BubbleVisualSettings
            {
                bubbleColor = this.bubbleColor,
                metallic = this.metallic,
                smoothness = this.smoothness,
                baseOpacity = this.baseOpacity,
                textColor = this.textColor,
                fontSize = this.fontSize,
                minFontSize = this.minFontSize,
                maxFontSize = this.maxFontSize,
                hoverColor = this.hoverColor,
                selectedColor = this.selectedColor,
                selectedTextColor = this.selectedTextColor,
                breathingIntensity = this.breathingIntensity,
                opacityBreathingIntensity = this.opacityBreathingIntensity,
                colliderRadius = this.colliderRadius,
                minScale = this.minScale,
                maxScale = this.maxScale
            };
        }
    }
}