using UnityEngine;
using TMPro;

namespace XRBubbleLibrary.WordBubbles
{
    /// <summary>
    /// Settings for bubble text rendering
    /// Optimized for VR readability and performance
    /// </summary>
    [System.Serializable]
    public class BubbleTextSettings
    {
        [Header("Basic Text Properties")]
        public Color textColor = Color.white;
        public float fontSize = 0.5f;
        public FontStyles fontStyle = FontStyles.Normal;
        public TextAlignmentOptions alignment = TextAlignmentOptions.Center;
        
        [Header("Font Size Limits")]
        public float minFontSize = 0.2f;
        public float maxFontSize = 1.0f;
        
        [Header("Text Container")]
        public bool enableWordWrapping = false;
        public TextOverflowModes overflowMode = TextOverflowModes.Overflow;
        
        [Header("Text Outline")]
        public float outlineWidth = 0.1f;
        public Color outlineColor = Color.black;
        
        [Header("Positioning")]
        public bool centerTextInBubble = true;
        public bool alwaysFaceUser = true;
        
        [Header("Adaptive Sizing")]
        public bool adaptiveFontSizing = true;
        
        [Header("VR Optimization")]
        public bool distanceBasedQuality = true;
        public float highQualityDistance = 3.0f;
        
        /// <summary>
        /// Default text settings optimized for VR word bubbles
        /// </summary>
        public static BubbleTextSettings Default => new BubbleTextSettings();
        
        /// <summary>
        /// High contrast settings for better readability
        /// </summary>
        public static BubbleTextSettings HighContrast => new BubbleTextSettings
        {
            textColor = Color.white,
            fontSize = 0.6f,
            fontStyle = FontStyles.Bold,
            outlineWidth = 0.15f,
            outlineColor = Color.black,
            minFontSize = 0.3f,
            maxFontSize = 1.2f
        };
        
        /// <summary>
        /// Subtle text settings for minimal distraction
        /// </summary>
        public static BubbleTextSettings Subtle => new BubbleTextSettings
        {
            textColor = new Color(0.2f, 0.2f, 0.2f, 1.0f),
            fontSize = 0.4f,
            fontStyle = FontStyles.Normal,
            outlineWidth = 0.05f,
            outlineColor = new Color(0.8f, 0.8f, 0.8f, 0.5f),
            minFontSize = 0.15f,
            maxFontSize = 0.8f
        };
        
        /// <summary>
        /// Large text settings for accessibility
        /// </summary>
        public static BubbleTextSettings Large => new BubbleTextSettings
        {
            textColor = Color.white,
            fontSize = 0.8f,
            fontStyle = FontStyles.Bold,
            outlineWidth = 0.2f,
            outlineColor = Color.black,
            minFontSize = 0.4f,
            maxFontSize = 1.5f,
            adaptiveFontSizing = false
        };
        
        /// <summary>
        /// Compact text settings for dense information
        /// </summary>
        public static BubbleTextSettings Compact => new BubbleTextSettings
        {
            textColor = Color.white,
            fontSize = 0.3f,
            fontStyle = FontStyles.Normal,
            outlineWidth = 0.08f,
            outlineColor = Color.black,
            minFontSize = 0.1f,
            maxFontSize = 0.6f,
            enableWordWrapping = true,
            adaptiveFontSizing = true
        };
        
        /// <summary>
        /// Validate settings for VR use
        /// </summary>
        public void ValidateSettings()
        {
            // Ensure font sizes are reasonable for VR
            fontSize = Mathf.Clamp(fontSize, 0.1f, 2.0f);
            minFontSize = Mathf.Clamp(minFontSize, 0.05f, fontSize);
            maxFontSize = Mathf.Clamp(maxFontSize, fontSize, 3.0f);
            
            // Ensure min < max
            if (minFontSize >= maxFontSize)
                maxFontSize = minFontSize + 0.1f;
            
            // Clamp outline width
            outlineWidth = Mathf.Clamp(outlineWidth, 0f, 0.5f);
            
            // Ensure text color has good alpha
            textColor.a = Mathf.Clamp(textColor.a, 0.5f, 1.0f);
            
            // Clamp distance settings
            highQualityDistance = Mathf.Clamp(highQualityDistance, 1.0f, 10.0f);
        }
        
        /// <summary>
        /// Create a copy of these settings
        /// </summary>
        public BubbleTextSettings Clone()
        {
            return new BubbleTextSettings
            {
                textColor = this.textColor,
                fontSize = this.fontSize,
                fontStyle = this.fontStyle,
                alignment = this.alignment,
                minFontSize = this.minFontSize,
                maxFontSize = this.maxFontSize,
                enableWordWrapping = this.enableWordWrapping,
                overflowMode = this.overflowMode,
                outlineWidth = this.outlineWidth,
                outlineColor = this.outlineColor,
                centerTextInBubble = this.centerTextInBubble,
                alwaysFaceUser = this.alwaysFaceUser,
                adaptiveFontSizing = this.adaptiveFontSizing,
                distanceBasedQuality = this.distanceBasedQuality,
                highQualityDistance = this.highQualityDistance
            };
        }
    }
}