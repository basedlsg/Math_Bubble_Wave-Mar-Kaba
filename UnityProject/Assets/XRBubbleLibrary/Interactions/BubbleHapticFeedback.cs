using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Mathematics;
using XRBubbleLibrary.Physics;

namespace XRBubbleLibrary.Interactions
{
    /// <summary>
    /// Bubble Haptic Feedback - Cloned from Unity haptic feedback samples
    /// Provides tactile feedback for bubble interactions using XR controller haptics
    /// Based on Unity's XR haptic samples and multi-sensory feedback examples
    /// </summary>
    public class BubbleHapticFeedback : MonoBehaviour
    {
        [Header("Haptic Configuration (Unity Haptic Samples)")]
        [Tooltip("Cloned from Unity haptic intensity samples")]
        [SerializeField] private float baseHapticIntensity = 0.5f;
        
        [Tooltip("Based on Unity haptic duration samples")]
        [SerializeField] private float baseHapticDuration = 0.1f;
        
        [Tooltip("Cloned from Unity haptic frequency samples")]
        [SerializeField] private float hapticFrequency = 60f; // Hz
        
        [Header("Glass Bubble Haptics (Unity Glass Samples)")]
        [SerializeField] private float glassContactIntensity = 0.3f;
        [SerializeField] private float glassBreakIntensity = 1.0f;
        [SerializeField] private float glassResonanceIntensity = 0.4f;
        
        [Header("Wave-Based Haptics (Unity Wave Samples)")]
        [SerializeField] private bool enableWaveHaptics = true;
        [SerializeField] private float waveHapticAmplitude = 0.2f;
        [SerializeField] private float waveHapticFrequency = 0.3f; // Matches breathing
        
        [Header("Adaptive Haptics (Unity Adaptive Samples)")]
        [SerializeField] private bool enableAdaptiveIntensity = true;
        [SerializeField] private float adaptationSpeed = 2f;
        [SerializeField] private float maxIntensity = 1f;
        
        // Controller references
        private UnityEngine.XR.Interaction.Toolkit.XRBaseController leftController;
        private UnityEngine.XR.Interaction.Toolkit.XRBaseController rightController;
        private UnityEngine.XR.Interaction.Toolkit.XRController leftActionController;
        private UnityEngine.XR.Interaction.Toolkit.XRController rightActionController;
        
        // Haptic state
        private float currentLeftIntensity = 0f;
        private float currentRightIntensity = 0f;
        private float wavePhase = 0f;
        private bool isInitialized = false;
        
        // Component references
        private BubbleBreathingSystem breathingSystem;
        private WaveBreathingIntegration waveIntegration;
        private BubbleSpringPhysics springPhysics;
        
        // Haptic patterns
        private HapticPattern glassContactPattern;
        private HapticPattern breathingPattern;
        private HapticPattern interactionPattern;
        
        private void Start()
        {
            InitializeHapticSystem();
            SetupControllerReferences();
            CreateHapticPatterns();
            SetupComponentReferences();
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            UpdateWaveHaptics();
            UpdateAdaptiveHaptics();
            ApplyHapticFeedback();
        }
        
        /// <summary>
        /// Initializes haptic system based on Unity haptic samples
        /// </summary>
        private void InitializeHapticSystem()
        {
            // Initialize haptic state (Unity initialization samples)
            currentLeftIntensity = 0f;
            currentRightIntensity = 0f;
            wavePhase = 0f;
            
            Debug.Log("Bubble Haptic Feedback initialized - cloned from Unity haptic samples");
        }
        
        /// <summary>
        /// Sets up controller references based on Unity controller samples
        /// </summary>
        private void SetupControllerReferences()
        {
            // Find XR controllers (Unity controller search samples)
            var controllers = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.XRBaseController>();
            
            foreach (var controller in controllers)
            {
                // Identify left and right controllers (Unity controller identification samples)
                if (controller.name.ToLower().Contains("left"))
                {
                    leftController = controller;
                    leftActionController = controller as UnityEngine.XR.Interaction.Toolkit.XRController;
                }
                else if (controller.name.ToLower().Contains("right"))
                {
                    rightController = controller;
                    rightActionController = controller as UnityEngine.XR.Interaction.Toolkit.XRController;
                }
            }
            
            // Validate controller setup
            if (leftController == null || rightController == null)
            {
                Debug.LogWarning("XR Controllers not found - haptic feedback may not work");
            }
            else
            {
                Debug.Log($"Controllers found - Left: {leftController.name}, Right: {rightController.name}");
                isInitialized = true;
            }
        }
        
        /// <summary>
        /// Creates haptic patterns based on Unity haptic pattern samples
        /// </summary>
        private void CreateHapticPatterns()
        {
            // Glass contact pattern (Unity glass haptic samples)
            glassContactPattern = new HapticPattern
            {
                intensity = glassContactIntensity,
                duration = 0.05f,
                frequency = 80f,
                fadeIn = 0.01f,
                fadeOut = 0.02f
            };
            
            // Breathing pattern (Unity breathing haptic samples)
            breathingPattern = new HapticPattern
            {
                intensity = waveHapticAmplitude,
                duration = 1f / waveHapticFrequency, // Full breathing cycle
                frequency = waveHapticFrequency,
                fadeIn = 0.1f,
                fadeOut = 0.1f
            };
            
            // General interaction pattern (Unity interaction haptic samples)
            interactionPattern = new HapticPattern
            {
                intensity = baseHapticIntensity,
                duration = baseHapticDuration,
                frequency = hapticFrequency,
                fadeIn = 0.02f,
                fadeOut = 0.03f
            };
            
            Debug.Log("Haptic patterns created - glass contact, breathing, and interaction patterns");
        }
        
        /// <summary>
        /// Sets up component references based on Unity component search samples
        /// </summary>
        private void SetupComponentReferences()
        {
            // Find physics and breathing systems (Unity component search samples)
            breathingSystem = FindObjectOfType<BubbleBreathingSystem>();
            waveIntegration = FindObjectOfType<WaveBreathingIntegration>();
            springPhysics = FindObjectOfType<BubbleSpringPhysics>();
            
            if (breathingSystem == null)
            {
                Debug.LogWarning("BubbleBreathingSystem not found - breathing haptics disabled");
            }
            
            if (waveIntegration == null)
            {
                Debug.LogWarning("WaveBreathingIntegration not found - wave haptics disabled");
            }
        }
        
        /// <summary>
        /// Updates wave-based haptics based on Unity wave haptic samples
        /// </summary>
        private void UpdateWaveHaptics()
        {
            if (!enableWaveHaptics) return;
            
            // Update wave phase (Unity wave samples)
            wavePhase += Time.deltaTime * waveHapticFrequency * 2f * math.PI;
            
            // Keep phase in reasonable range
            if (wavePhase > 2f * math.PI * 10f)
            {
                wavePhase -= 2f * math.PI * 10f;
            }
            
            // Calculate wave haptic intensity (Unity wave calculation samples)
            float waveIntensity = math.sin(wavePhase) * waveHapticAmplitude;
            waveIntensity = math.max(0f, waveIntensity); // Only positive values
            
            // Apply wave haptics to both controllers (Unity bilateral haptic samples)
            AddHapticIntensity(waveIntensity, waveIntensity);
        }
        
        /// <summary>
        /// Updates adaptive haptics based on Unity adaptive samples
        /// </summary>
        private void UpdateAdaptiveHaptics()
        {
            if (!enableAdaptiveIntensity) return;
            
            // Get system activity for adaptation (Unity activity sampling samples)
            float systemActivity = CalculateSystemActivity();
            
            // Adapt haptic intensity based on activity (Unity adaptation samples)
            float targetIntensity = Mathf.Lerp(baseHapticIntensity * 0.5f, maxIntensity, systemActivity);
            
            // Smooth adaptation (Unity smoothing samples)
            baseHapticIntensity = Mathf.Lerp(baseHapticIntensity, targetIntensity, Time.deltaTime * adaptationSpeed);
        }
        
        /// <summary>
        /// Calculates system activity for adaptive haptics
        /// Based on Unity activity calculation samples
        /// </summary>
        private float CalculateSystemActivity()
        {
            float activity = 0f;
            
            // Factor in breathing intensity (Unity breathing activity samples)
            if (breathingSystem != null)
            {
                // Estimate breathing activity from system state
                activity += 0.3f; // Base breathing activity
            }
            
            // Factor in wave activity (Unity wave activity samples)
            if (waveIntegration != null)
            {
                var waveDisplacement = waveIntegration.GetWaveDisplacement(0);
                activity += math.length(waveDisplacement) * 0.5f;
            }
            
            // Factor in spring physics activity (Unity physics activity samples)
            if (springPhysics != null)
            {
                var velocity = springPhysics.GetVelocity(0);
                activity += math.length(velocity) * 0.2f;
            }
            
            return Mathf.Clamp01(activity);
        }
        
        /// <summary>
        /// Applies haptic feedback to controllers based on Unity haptic application samples
        /// </summary>
        private void ApplyHapticFeedback()
        {
            // Apply to left controller (Unity left controller samples)
            if (leftController != null && currentLeftIntensity > 0.01f)
            {
                leftController.SendHapticImpulse(currentLeftIntensity, baseHapticDuration);
            }
            
            // Apply to right controller (Unity right controller samples)
            if (rightController != null && currentRightIntensity > 0.01f)
            {
                rightController.SendHapticImpulse(currentRightIntensity, baseHapticDuration);
            }
            
            // Decay current intensities (Unity decay samples)
            currentLeftIntensity = Mathf.Lerp(currentLeftIntensity, 0f, Time.deltaTime * 5f);
            currentRightIntensity = Mathf.Lerp(currentRightIntensity, 0f, Time.deltaTime * 5f);
        }
        
        /// <summary>
        /// Adds haptic intensity to both controllers
        /// Based on Unity haptic accumulation samples
        /// </summary>
        private void AddHapticIntensity(float leftIntensity, float rightIntensity)
        {
            currentLeftIntensity = Mathf.Clamp01(currentLeftIntensity + leftIntensity);
            currentRightIntensity = Mathf.Clamp01(currentRightIntensity + rightIntensity);
        }
        
        /// <summary>
        /// Triggers glass contact haptic feedback
        /// Based on Unity glass haptic samples
        /// </summary>
        public void TriggerGlassContact(Vector3 contactPosition, float contactForce, bool isLeftHand = false)
        {
            // Calculate haptic intensity based on contact force (Unity force haptic samples)
            float intensity = Mathf.Lerp(glassContactPattern.intensity * 0.5f, glassContactPattern.intensity, contactForce);
            
            // Apply to appropriate controller (Unity controller selection samples)
            if (isLeftHand)
            {
                currentLeftIntensity = Mathf.Max(currentLeftIntensity, intensity);
            }
            else
            {
                currentRightIntensity = Mathf.Max(currentRightIntensity, intensity);
            }
            
            Debug.Log($"Glass contact haptic triggered - Force: {contactForce:F2}, Intensity: {intensity:F2}");
        }
        
        /// <summary>
        /// Triggers glass resonance haptic feedback
        /// Based on Unity resonance haptic samples
        /// </summary>
        public void TriggerGlassResonance(float resonanceFrequency, float resonanceAmplitude)
        {
            // Calculate resonance haptic intensity (Unity resonance samples)
            float intensity = resonanceAmplitude * glassResonanceIntensity;
            
            // Apply resonance to both controllers (Unity bilateral samples)
            AddHapticIntensity(intensity, intensity);
            
            Debug.Log($"Glass resonance haptic triggered - Frequency: {resonanceFrequency:F1}Hz, Amplitude: {resonanceAmplitude:F2}");
        }
        
        /// <summary>
        /// Triggers bubble interaction haptic feedback
        /// Based on Unity interaction haptic samples
        /// </summary>
        public void TriggerBubbleInteraction(InteractionType interactionType, float intensity, bool isLeftHand = false)
        {
            // Select haptic pattern based on interaction type (Unity pattern selection samples)
            HapticPattern pattern = interactionPattern;
            
            switch (interactionType)
            {
                case InteractionType.Touch:
                    pattern = glassContactPattern;
                    break;
                case InteractionType.Grab:
                    pattern = interactionPattern;
                    intensity *= 1.5f; // Stronger for grab
                    break;
                case InteractionType.Release:
                    pattern = interactionPattern;
                    intensity *= 0.7f; // Gentler for release
                    break;
                case InteractionType.Hover:
                    pattern = glassContactPattern;
                    intensity *= 0.3f; // Very gentle for hover
                    break;
            }
            
            // Apply pattern-based intensity (Unity pattern application samples)
            float finalIntensity = pattern.intensity * intensity;
            
            // Apply to appropriate controller (Unity controller selection samples)
            if (isLeftHand)
            {
                currentLeftIntensity = Mathf.Max(currentLeftIntensity, finalIntensity);
            }
            else
            {
                currentRightIntensity = Mathf.Max(currentRightIntensity, finalIntensity);
            }
            
            Debug.Log($"Bubble interaction haptic triggered - Type: {interactionType}, Intensity: {finalIntensity:F2}");
        }
        
        /// <summary>
        /// Synchronizes haptics with breathing system
        /// Based on Unity synchronization samples
        /// </summary>
        public void SynchronizeWithBreathing(float breathingPhase, float breathingAmplitude)
        {
            if (!enableWaveHaptics) return;
            
            // Calculate breathing haptic intensity (Unity breathing haptic samples)
            float breathingHaptic = math.sin(breathingPhase) * breathingAmplitude * breathingPattern.intensity;
            breathingHaptic = math.max(0f, breathingHaptic); // Only positive values
            
            // Apply breathing haptics (Unity breathing application samples)
            AddHapticIntensity(breathingHaptic, breathingHaptic);
        }
        
        /// <summary>
        /// Triggers emergency haptic feedback for system alerts
        /// Based on Unity alert haptic samples
        /// </summary>
        public void TriggerEmergencyFeedback()
        {
            // Strong haptic pulse for emergencies (Unity emergency samples)
            currentLeftIntensity = 1f;
            currentRightIntensity = 1f;
            
            Debug.Log("Emergency haptic feedback triggered");
        }
        
        /// <summary>
        /// Validates haptic feedback setup
        /// </summary>
        [ContextMenu("Validate Haptic Feedback")]
        public void ValidateHapticFeedback()
        {
            Debug.Log($"Bubble Haptic Feedback Status:");
            Debug.Log($"- Left Controller: {(leftController != null ? leftController.name : "Missing")}");
            Debug.Log($"- Right Controller: {(rightController != null ? rightController.name : "Missing")}");
            Debug.Log($"- Breathing System: {(breathingSystem != null ? "Found" : "Missing")}");
            Debug.Log($"- Wave Integration: {(waveIntegration != null ? "Found" : "Missing")}");
            Debug.Log($"- Spring Physics: {(springPhysics != null ? "Found" : "Missing")}");
            Debug.Log($"- Wave Haptics: {enableWaveHaptics}");
            Debug.Log($"- Adaptive Haptics: {enableAdaptiveIntensity}");
            Debug.Log($"- Base Intensity: {baseHapticIntensity}");
            Debug.Log($"- Initialized: {isInitialized}");
        }
        
        /// <summary>
        /// Tests haptic feedback on both controllers
        /// </summary>
        [ContextMenu("Test Haptic Feedback")]
        public void TestHapticFeedback()
        {
            Debug.Log("Testing haptic feedback...");
            
            // Test left controller
            if (leftController != null)
            {
                leftController.SendHapticImpulse(0.8f, 0.2f);
                Debug.Log("Left controller haptic test sent");
            }
            
            // Test right controller
            if (rightController != null)
            {
                rightController.SendHapticImpulse(0.8f, 0.2f);
                Debug.Log("Right controller haptic test sent");
            }
        }
        
        /// <summary>
        /// Optimizes haptic feedback for Quest 3
        /// Based on Unity Quest optimization samples
        /// </summary>
        [ContextMenu("Optimize for Quest 3")]
        public void OptimizeForQuest3()
        {
            // Optimize haptic parameters for Quest 3 controllers (Unity Quest samples)
            baseHapticIntensity = 0.6f; // Slightly stronger for Quest 3
            baseHapticDuration = 0.08f; // Shorter for better responsiveness
            hapticFrequency = 72f; // Match Quest 3 refresh rate
            
            // Optimize glass haptics for Quest 3 (Unity glass optimization samples)
            glassContactIntensity = 0.4f;
            glassResonanceIntensity = 0.5f;
            
            // Optimize wave haptics for Quest 3 (Unity wave optimization samples)
            waveHapticAmplitude = 0.15f; // Reduced for comfort
            waveHapticFrequency = 0.25f; // Match comfortable breathing rate
            
            Debug.Log("Haptic feedback optimized for Quest 3 - improved responsiveness and comfort");
        }
    }
    
    /// <summary>
    /// Haptic pattern data structure
    /// Based on Unity haptic pattern samples
    /// </summary>
    [System.Serializable]
    public struct HapticPattern
    {
        public float intensity;
        public float duration;
        public float frequency;
        public float fadeIn;
        public float fadeOut;
    }
    
    /// <summary>
    /// Interaction types for haptic feedback
    /// Based on Unity interaction type samples
    /// </summary>
    public enum InteractionType
    {
        Touch,
        Grab,
        Release,
        Hover,
        Push,
        Swipe
    }
}
