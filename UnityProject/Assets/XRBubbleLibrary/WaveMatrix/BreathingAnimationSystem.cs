using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Breathing animation system that applies wave-based breathing effects to UI elements
    /// Creates living, organic feeling through mathematical wave formulas
    /// </summary>
    public class BreathingAnimationSystem : MonoBehaviour
    {
        [Header("Breathing Parameters")]
        [SerializeField] private BreathingSettings settings = BreathingSettings.Default;
        
        [Header("Performance")]
        [SerializeField] private float updateFrequency = 15f; // Reduced from 60Hz for VR performance
        [SerializeField] private int maxBreathingElements = 50; // Reduced for Quest 3
        
        // Registered breathing elements
        private List<IBreathingElement> breathingElements = new List<IBreathingElement>();
        private Dictionary<IBreathingElement, BreathingState> elementStates = new Dictionary<IBreathingElement, BreathingState>();
        
        // Timing
        private float currentTime = 0f;
        private float lastUpdateTime = 0f;
        private float updateInterval;
        
        void Start()
        {
            InitializeBreathingSystem();
        }
        
        void Update()
        {
            UpdateBreathingTime();
            
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateAllBreathingElements();
                lastUpdateTime = Time.time;
            }
        }
        
        void InitializeBreathingSystem()
        {
            updateInterval = 1f / updateFrequency;
            Debug.Log($"Breathing Animation System initialized: {updateFrequency}Hz update rate");
        }
        
        void UpdateBreathingTime()
        {
            currentTime += Time.deltaTime * settings.globalTimeScale;
        }
        
        void UpdateAllBreathingElements()
        {
            foreach (var element in breathingElements)
            {
                if (element != null && elementStates.ContainsKey(element))
                {
                    UpdateElementBreathing(element, elementStates[element]);
                }
            }
        }
        
        void UpdateElementBreathing(IBreathingElement element, BreathingState state)
        {
            // Calculate breathing values using wave mathematics
            BreathingValues values = CalculateBreathingValues(state, currentTime);
            
            // Apply breathing to element
            element.ApplyBreathing(values);
        }
        
        /// <summary>
        /// Calculate breathing values using multiple wave components
        /// </summary>
        BreathingValues CalculateBreathingValues(BreathingState state, float time)
        {
            float phaseOffset = state.phaseOffset;
            float personalTime = time + phaseOffset;
            
            // Primary breathing wave (main rhythm)
            float primaryBreath = math.sin(personalTime * settings.primaryFrequency * 2f * math.PI) 
                                 * settings.primaryAmplitude;
            
            // Secondary breathing wave (adds complexity)
            float secondaryBreath = math.sin(personalTime * settings.secondaryFrequency * 2f * math.PI) 
                                   * settings.secondaryAmplitude;
            
            // Tertiary breathing wave (fine detail)
            float tertiaryBreath = math.sin(personalTime * settings.tertiaryFrequency * 2f * math.PI) 
                                  * settings.tertiaryAmplitude;
            
            // Combine waves
            float combinedBreath = primaryBreath + secondaryBreath + tertiaryBreath;
            
            // Apply breathing curve for more natural feeling
            combinedBreath = ApplyBreathingCurve(combinedBreath);
            
            // Calculate scale (1.0 + breathing effect)
            float scale = 1f + combinedBreath * state.scaleIntensity;
            scale = math.clamp(scale, settings.minScale, settings.maxScale);
            
            // Calculate opacity (base opacity + breathing effect)
            float opacity = state.baseOpacity + combinedBreath * state.opacityIntensity;
            opacity = math.clamp(opacity, settings.minOpacity, settings.maxOpacity);
            
            return new BreathingValues
            {
                scale = scale,
                opacity = opacity,
                breathingPhase = combinedBreath
            };
        }
        
        /// <summary>
        /// Apply breathing curve for more natural breathing feel
        /// </summary>
        float ApplyBreathingCurve(float rawBreathing)
        {
            // Use a curve that emphasizes the exhale (like natural breathing)
            float normalized = (rawBreathing + 1f) * 0.5f; // Convert from [-1,1] to [0,1]
            float curved = math.pow(normalized, settings.breathingCurve);
            return (curved * 2f) - 1f; // Convert back to [-1,1]
        }
        
        /// <summary>
        /// Register an element for breathing animation
        /// </summary>
        public void RegisterBreathingElement(IBreathingElement element, BreathingElementConfig config = null)
        {
            if (element == null) return;
            
            if (breathingElements.Count >= maxBreathingElements)
            {
                Debug.LogWarning($"Maximum breathing elements ({maxBreathingElements}) reached");
                return;
            }
            
            if (!breathingElements.Contains(element))
            {
                breathingElements.Add(element);
                
                // Create breathing state with random phase offset for variety
                var state = new BreathingState
                {
                    phaseOffset = UnityEngine.Random.Range(0f, 2f * math.PI),
                    scaleIntensity = config?.scaleIntensity ?? settings.defaultScaleIntensity,
                    opacityIntensity = config?.opacityIntensity ?? settings.defaultOpacityIntensity,
                    baseOpacity = config?.baseOpacity ?? 1f
                };
                
                elementStates[element] = state;
                
                Debug.Log($"Registered breathing element: {element.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Unregister an element from breathing animation
        /// </summary>
        public void UnregisterBreathingElement(IBreathingElement element)
        {
            if (element != null && breathingElements.Contains(element))
            {
                breathingElements.Remove(element);
                elementStates.Remove(element);
            }
        }
        
        /// <summary>
        /// Update breathing settings at runtime
        /// </summary>
        public void UpdateBreathingSettings(BreathingSettings newSettings)
        {
            settings = newSettings;
        }
        
        /// <summary>
        /// Get current breathing phase for synchronization
        /// </summary>
        public float GetGlobalBreathingPhase()
        {
            return math.sin(currentTime * settings.primaryFrequency * 2f * math.PI);
        }
    }
    
    /// <summary>
    /// Interface for elements that can receive breathing animation
    /// </summary>
    public interface IBreathingElement
    {
        void ApplyBreathing(BreathingValues values);
    }
    
    /// <summary>
    /// Breathing values calculated by the system
    /// </summary>
    public struct BreathingValues
    {
        public float scale;
        public float opacity;
        public float breathingPhase; // Raw breathing phase [-1, 1]
    }
    
    /// <summary>
    /// Configuration for individual breathing elements
    /// </summary>
    [System.Serializable]
    public class BreathingElementConfig
    {
        public float scaleIntensity = 1f;
        public float opacityIntensity = 1f;
        public float baseOpacity = 1f;
    }
    
    /// <summary>
    /// Internal state for each breathing element
    /// </summary>
    class BreathingState
    {
        public float phaseOffset;
        public float scaleIntensity;
        public float opacityIntensity;
        public float baseOpacity;
    }
}