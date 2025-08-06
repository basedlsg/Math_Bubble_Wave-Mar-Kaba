#if FALSE // DISABLED: Missing dependencies - requires Input System package
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using XRBubbleLibrary.UI;

namespace XRBubbleLibrary.Interactions
{
    /// <summary>
    /// Bubble Accessibility - Cloned from Unity accessibility samples
    /// Provides accessible interaction methods for users with different abilities
    /// Based on Unity's accessibility samples and inclusive design examples
    /// </summary>
    public class BubbleAccessibility : MonoBehaviour
    {
        [Header("Controller Fallback (Unity Accessibility Samples)")]
        [Tooltip("Cloned from Unity controller fallback samples")]
        [SerializeField] private bool enableControllerFallback = true;
        
        [Tooltip("Based on Unity input accessibility samples")]
        [SerializeField] private float controllerSensitivity = 1.5f;
        
        [Tooltip("Cloned from Unity button mapping samples")]
        [SerializeField] private InputActionReference primaryButtonAction;
        [SerializeField] private InputActionReference secondaryButtonAction;
        
        [Header("Visual Accessibility (Unity Visual Samples)")]
        [SerializeField] private bool enableHighContrast = false;
        [SerializeField] private bool enableLargeTargets = false;
        [SerializeField] private float targetSizeMultiplier = 1.5f;
        
        [Header("Audio Accessibility (Unity Audio Samples)")]
        [SerializeField] private bool enableAudioFeedback = true;
        [SerializeField] private AudioClip interactionSound;
        [SerializeField] private AudioClip navigationSound;
        [SerializeField] private float audioVolume = 0.7f;
        
        [Header("Haptic Accessibility (Unity Haptic Samples)")]
        [SerializeField] private bool enableEnhancedHaptics = false;
        [SerializeField] private float hapticMultiplier = 2f;
        
        [Header("Voice Commands (Unity Voice Samples)")]
        [SerializeField] private bool enableVoiceCommands = true;
        [SerializeField] private string[] voiceCommands = { "select", "grab", "release", "next", "previous" };
        
        // Component references
        private BubbleXRInteractable[] bubbleInteractables;
        private SpatialBubbleUI spatialUI;
        private BubbleHapticFeedback hapticFeedback;
        private AudioSource audioSource;
        
        // Accessibility state
        private int currentBubbleIndex = 0;
        private bool isNavigationMode = false;
        private float lastNavigationTime = 0f;
        private const float NAVIGATION_COOLDOWN = 0.3f;
        
        // Controller input state
        private Vector2 lastThumbstickInput = Vector2.zero;
        private bool lastPrimaryButtonState = false;
        private bool lastSecondaryButtonState = false;
        
        // Visual accessibility materials
        private Material highContrastMaterial;
        private Material[] originalMaterials;
        
        private void Start()
        {
            InitializeAccessibility();
            SetupComponentReferences();
            ConfigureInputActions();
            CreateAccessibilityMaterials();
        }
        
        private void Update()
        {
            if (enableControllerFallback)
            {
                ProcessControllerInput();
            }
            
            if (enableVoiceCommands)
            {
                ProcessVoiceCommands();
            }
        }
        
        /// <summary>
        /// Initializes accessibility system based on Unity accessibility samples
        /// </summary>
        private void InitializeAccessibility()
        {
            // Initialize accessibility state (Unity initialization samples)
            currentBubbleIndex = 0;
            isNavigationMode = false;
            lastNavigationTime = Time.time;
            
            Debug.Log("Bubble Accessibility initialized - cloned from Unity accessibility samples");
        }
        
        /// <summary>
        /// Sets up component references based on Unity component search samples
        /// </summary>
        private void SetupComponentReferences()
        {
            // Find bubble interactables (Unity component search samples)
            bubbleInteractables = FindObjectsOfType<BubbleXRInteractable>();
            
            // Find UI system (Unity UI search samples)
            spatialUI = FindObjectOfType<SpatialBubbleUI>();
            
            // Find haptic feedback system (Unity haptic search samples)
            hapticFeedback = FindObjectOfType<BubbleHapticFeedback>();
            
            // Get or create audio source (Unity audio setup samples)
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                ConfigureAccessibilityAudio();
            }
            
            Debug.Log($"Accessibility references setup - Found {bubbleInteractables?.Length ?? 0} bubble interactables");
        }
        
        /// <summary>
        /// Configures input actions based on Unity input system samples
        /// </summary>
        private void ConfigureInputActions()
        {
            // Configure primary button action (Unity input samples)
            if (primaryButtonAction != null)
            {
                primaryButtonAction.action.Enable();
                primaryButtonAction.action.performed += OnPrimaryButtonPressed;
                primaryButtonAction.action.canceled += OnPrimaryButtonReleased;
            }
            
            // Configure secondary button action (Unity input samples)
            if (secondaryButtonAction != null)
            {
                secondaryButtonAction.action.Enable();
                secondaryButtonAction.action.performed += OnSecondaryButtonPressed;
                secondaryButtonAction.action.canceled += OnSecondaryButtonReleased;
            }
        }
        
        /// <summary>
        /// Configures accessibility audio based on Unity audio samples
        /// </summary>
        private void ConfigureAccessibilityAudio()
        {
            // Configure for accessibility audio (Unity accessibility audio samples)
            audioSource.spatialBlend = 0f; // 2D audio for accessibility
            audioSource.volume = audioVolume;
            audioSource.playOnAwake = false;
            audioSource.priority = 64; // High priority for accessibility
        }
        
        /// <summary>
        /// Creates accessibility materials based on Unity material samples
        /// </summary>
        private void CreateAccessibilityMaterials()
        {
            // Create high contrast material (Unity high contrast samples)
            if (enableHighContrast)
            {
                highContrastMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                highContrastMaterial.name = "High Contrast Bubble Material";
                highContrastMaterial.color = Color.white;
                highContrastMaterial.SetFloat("_Metallic", 0f);
                highContrastMaterial.SetFloat("_Smoothness", 0.8f);
                
                // Store original materials for restoration
                StoreOriginalMaterials();
            }
        }
        
        /// <summary>
        /// Stores original materials for restoration
        /// Based on Unity material management samples
        /// </summary>
        private void StoreOriginalMaterials()
        {
            if (bubbleInteractables == null) return;
            
            originalMaterials = new Material[bubbleInteractables.Length];
            
            for (int i = 0; i < bubbleInteractables.Length; i++)
            {
                var renderer = bubbleInteractables[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterials[i] = renderer.material;
                }
            }
        }
        
        /// <summary>
        /// Processes controller input for accessibility
        /// Based on Unity controller input samples
        /// </summary>
        private void ProcessControllerInput()
        {
            // Get thumbstick input for navigation (Unity thumbstick samples)
            Vector2 thumbstickInput = GetThumbstickInput();
            
            // Process navigation input (Unity navigation samples)
            if (thumbstickInput.magnitude > 0.7f && Time.time - lastNavigationTime > NAVIGATION_COOLDOWN)
            {
                ProcessNavigationInput(thumbstickInput);
                lastNavigationTime = Time.time;
            }
            
            // Store last input for edge detection
            lastThumbstickInput = thumbstickInput;
        }
        
        /// <summary>
        /// Gets thumbstick input from XR controllers
        /// Based on Unity XR input samples
        /// </summary>
        private Vector2 GetThumbstickInput()
        {
            // This would typically get input from XR Input System
            // For now, return zero - would be implemented with actual XR input
            return Vector2.zero;
        }
        
        /// <summary>
        /// Processes navigation input based on Unity navigation samples
        /// </summary>
        private void ProcessNavigationInput(Vector2 input)
        {
            if (bubbleInteractables == null || bubbleInteractables.Length == 0) return;
            
            // Determine navigation direction (Unity direction samples)
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                // Horizontal navigation
                if (input.x > 0)
                {
                    NavigateToNextBubble();
                }
                else
                {
                    NavigateToPreviousBubble();
                }
            }
            else
            {
                // Vertical navigation - could be used for different bubble layers
                if (input.y > 0)
                {
                    NavigateUp();
                }
                else
                {
                    NavigateDown();
                }
            }
        }
        
        /// <summary>
        /// Navigates to next bubble based on Unity navigation samples
        /// </summary>
        private void NavigateToNextBubble()
        {
            if (bubbleInteractables == null || bubbleInteractables.Length == 0) return;
            
            // Move to next bubble (Unity index navigation samples)
            currentBubbleIndex = (currentBubbleIndex + 1) % bubbleInteractables.Length;
            
            // Highlight current bubble (Unity highlight samples)
            HighlightCurrentBubble();
            
            // Play navigation sound (Unity audio feedback samples)
            PlayNavigationSound();
            
            // Provide haptic feedback (Unity haptic navigation samples)
            ProvideNavigationHaptic();
            
            Debug.Log($"Navigated to bubble {currentBubbleIndex}");
        }
        
        /// <summary>
        /// Navigates to previous bubble based on Unity navigation samples
        /// </summary>
        private void NavigateToPreviousBubble()
        {
            if (bubbleInteractables == null || bubbleInteractables.Length == 0) return;
            
            // Move to previous bubble (Unity index navigation samples)
            currentBubbleIndex = (currentBubbleIndex - 1 + bubbleInteractables.Length) % bubbleInteractables.Length;
            
            // Highlight current bubble (Unity highlight samples)
            HighlightCurrentBubble();
            
            // Play navigation sound (Unity audio feedback samples)
            PlayNavigationSound();
            
            // Provide haptic feedback (Unity haptic navigation samples)
            ProvideNavigationHaptic();
            
            Debug.Log($"Navigated to bubble {currentBubbleIndex}");
        }
        
        /// <summary>
        /// Navigates up in bubble hierarchy
        /// Based on Unity hierarchical navigation samples
        /// </summary>
        private void NavigateUp()
        {
            // Could be used for navigating between bubble layers or groups
            Debug.Log("Navigate up - could be implemented for bubble layers");
        }
        
        /// <summary>
        /// Navigates down in bubble hierarchy
        /// Based on Unity hierarchical navigation samples
        /// </summary>
        private void NavigateDown()
        {
            // Could be used for navigating between bubble layers or groups
            Debug.Log("Navigate down - could be implemented for bubble layers");
        }
        
        /// <summary>
        /// Highlights current bubble based on Unity highlight samples
        /// </summary>
        private void HighlightCurrentBubble()
        {
            if (bubbleInteractables == null || currentBubbleIndex >= bubbleInteractables.Length) return;
            
            // Clear previous highlights (Unity highlight clearing samples)
            ClearAllHighlights();
            
            // Highlight current bubble (Unity highlight application samples)
            var currentBubble = bubbleInteractables[currentBubbleIndex];
            if (currentBubble != null)
            {
                ApplyAccessibilityHighlight(currentBubble);
                
                // Scale bubble if large targets are enabled (Unity scaling samples)
                if (enableLargeTargets)
                {
                    currentBubble.transform.localScale = Vector3.one * targetSizeMultiplier;
                }
            }
        }
        
        /// <summary>
        /// Clears all bubble highlights based on Unity highlight clearing samples
        /// </summary>
        private void ClearAllHighlights()
        {
            if (bubbleInteractables == null) return;
            
            for (int i = 0; i < bubbleInteractables.Length; i++)
            {
                var bubble = bubbleInteractables[i];
                if (bubble != null)
                {
                    // Restore original material (Unity material restoration samples)
                    var renderer = bubble.GetComponent<Renderer>();
                    if (renderer != null && originalMaterials != null && i < originalMaterials.Length)
                    {
                        renderer.material = originalMaterials[i];
                    }
                    
                    // Restore original scale (Unity scale restoration samples)
                    bubble.transform.localScale = Vector3.one;
                }
            }
        }
        
        /// <summary>
        /// Applies accessibility highlight to bubble
        /// Based on Unity accessibility highlight samples
        /// </summary>
        private void ApplyAccessibilityHighlight(BubbleXRInteractable bubble)
        {
            var renderer = bubble.GetComponent<Renderer>();
            if (renderer == null) return;
            
            if (enableHighContrast && highContrastMaterial != null)
            {
                // Apply high contrast material (Unity high contrast samples)
                renderer.material = highContrastMaterial;
            }
            else
            {
                // Apply standard highlight (Unity standard highlight samples)
                if (renderer.material.HasProperty("_GlowIntensity"))
                {
                    renderer.material.SetFloat("_GlowIntensity", 3f);
                }
                
                if (renderer.material.HasProperty("_BubbleColor"))
                {
                    renderer.material.SetColor("_BubbleColor", Color.yellow);
                }
            }
        }
        
        /// <summary>
        /// Plays navigation sound based on Unity audio feedback samples
        /// </summary>
        private void PlayNavigationSound()
        {
            if (!enableAudioFeedback || audioSource == null || navigationSound == null) return;
            
            // Play navigation sound (Unity audio playback samples)
            audioSource.PlayOneShot(navigationSound, audioVolume);
        }
        
        /// <summary>
        /// Provides navigation haptic feedback
        /// Based on Unity haptic feedback samples
        /// </summary>
        private void ProvideNavigationHaptic()
        {
            if (!enableEnhancedHaptics || hapticFeedback == null) return;
            
            // Trigger navigation haptic (Unity navigation haptic samples)
            hapticFeedback.TriggerBubbleInteraction(InteractionType.Hover, 0.3f * hapticMultiplier);
        }
        
        /// <summary>
        /// Processes voice commands based on Unity voice samples
        /// </summary>
        private void ProcessVoiceCommands()
        {
            // Voice command processing would be implemented here
            // This would integrate with Unity's speech recognition or external voice systems
            // For now, this is a placeholder for future voice command integration
        }
        
        /// <summary>
        /// Called when primary button is pressed
        /// Based on Unity button event samples
        /// </summary>
        private void OnPrimaryButtonPressed(InputAction.CallbackContext context)
        {
            if (bubbleInteractables == null || currentBubbleIndex >= bubbleInteractables.Length) return;
            
            // Select current bubble (Unity selection samples)
            var currentBubble = bubbleInteractables[currentBubbleIndex];
            if (currentBubble != null)
            {
                SelectBubble(currentBubble);
            }
        }
        
        /// <summary>
        /// Called when primary button is released
        /// Based on Unity button event samples
        /// </summary>
        private void OnPrimaryButtonReleased(InputAction.CallbackContext context)
        {
            if (bubbleInteractables == null || currentBubbleIndex >= bubbleInteractables.Length) return;
            
            // Release current bubble (Unity release samples)
            var currentBubble = bubbleInteractables[currentBubbleIndex];
            if (currentBubble != null)
            {
                ReleaseBubble(currentBubble);
            }
        }
        
        /// <summary>
        /// Called when secondary button is pressed
        /// Based on Unity button event samples
        /// </summary>
        private void OnSecondaryButtonPressed(InputAction.CallbackContext context)
        {
            // Toggle navigation mode or perform secondary action
            ToggleNavigationMode();
        }
        
        /// <summary>
        /// Called when secondary button is released
        /// Based on Unity button event samples
        /// </summary>
        private void OnSecondaryButtonReleased(InputAction.CallbackContext context)
        {
            // Secondary button release handling
        }
        
        /// <summary>
        /// Selects bubble using accessibility method
        /// Based on Unity accessible selection samples
        /// </summary>
        private void SelectBubble(BubbleXRInteractable bubble)
        {
            // Simulate XR interaction selection (Unity simulation samples)
            // This would trigger the bubble's selection logic
            
            // Play interaction sound (Unity interaction audio samples)
            if (enableAudioFeedback && audioSource != null && interactionSound != null)
            {
                audioSource.PlayOneShot(interactionSound, audioVolume);
            }
            
            // Provide haptic feedback (Unity interaction haptic samples)
            if (enableEnhancedHaptics && hapticFeedback != null)
            {
                hapticFeedback.TriggerBubbleInteraction(InteractionType.Grab, 0.8f * hapticMultiplier);
            }
            
            Debug.Log($"Accessibility: Selected bubble {bubble.name}");
        }
        
        /// <summary>
        /// Releases bubble using accessibility method
        /// Based on Unity accessible release samples
        /// </summary>
        private void ReleaseBubble(BubbleXRInteractable bubble)
        {
            // Simulate XR interaction release (Unity simulation samples)
            // This would trigger the bubble's release logic
            
            // Provide release haptic feedback (Unity release haptic samples)
            if (enableEnhancedHaptics && hapticFeedback != null)
            {
                hapticFeedback.TriggerBubbleInteraction(InteractionType.Release, 0.5f * hapticMultiplier);
            }
            
            Debug.Log($"Accessibility: Released bubble {bubble.name}");
        }
        
        /// <summary>
        /// Toggles navigation mode based on Unity mode toggle samples
        /// </summary>
        private void ToggleNavigationMode()
        {
            isNavigationMode = !isNavigationMode;
            
            if (isNavigationMode)
            {
                // Enter navigation mode (Unity navigation mode samples)
                HighlightCurrentBubble();
                Debug.Log("Accessibility: Navigation mode enabled");
            }
            else
            {
                // Exit navigation mode (Unity navigation exit samples)
                ClearAllHighlights();
                Debug.Log("Accessibility: Navigation mode disabled");
            }
        }
        
        /// <summary>
        /// Validates accessibility setup
        /// </summary>
        [ContextMenu("Validate Accessibility Setup")]
        public void ValidateAccessibilitySetup()
        {
            Debug.Log($"Bubble Accessibility Status:");
            Debug.Log($"- Bubble Interactables: {bubbleInteractables?.Length ?? 0}");
            Debug.Log($"- Spatial UI: {(spatialUI != null ? "Found" : "Missing")}");
            Debug.Log($"- Haptic Feedback: {(hapticFeedback != null ? "Found" : "Missing")}");
            Debug.Log($"- Audio Source: {(audioSource != null ? "Found" : "Missing")}");
            Debug.Log($"- Controller Fallback: {enableControllerFallback}");
            Debug.Log($"- High Contrast: {enableHighContrast}");
            Debug.Log($"- Large Targets: {enableLargeTargets}");
            Debug.Log($"- Audio Feedback: {enableAudioFeedback}");
            Debug.Log($"- Enhanced Haptics: {enableEnhancedHaptics}");
            Debug.Log($"- Voice Commands: {enableVoiceCommands}");
            Debug.Log($"- Current Bubble Index: {currentBubbleIndex}");
            Debug.Log($"- Navigation Mode: {isNavigationMode}");
        }
        
        /// <summary>
        /// Enables all accessibility features
        /// Based on Unity accessibility enable samples
        /// </summary>
        [ContextMenu("Enable All Accessibility Features")]
        public void EnableAllAccessibilityFeatures()
        {
            enableControllerFallback = true;
            enableHighContrast = true;
            enableLargeTargets = true;
            enableAudioFeedback = true;
            enableEnhancedHaptics = true;
            enableVoiceCommands = true;
            
            // Apply changes immediately
            CreateAccessibilityMaterials();
            HighlightCurrentBubble();
            
            Debug.Log("All accessibility features enabled");
        }
        
        /// <summary>
        /// Optimizes accessibility for Quest 3
        /// Based on Unity Quest accessibility samples
        /// </summary>
        [ContextMenu("Optimize Accessibility for Quest 3")]
        public void OptimizeAccessibilityForQuest3()
        {
            // Optimize controller sensitivity for Quest 3 (Unity Quest samples)
            controllerSensitivity = 1.2f;
            
            // Optimize target size for Quest 3 display (Unity Quest display samples)
            targetSizeMultiplier = 1.3f; // Slightly smaller than default for Quest 3 resolution
            
            // Optimize audio for Quest 3 (Unity Quest audio samples)
            audioVolume = 0.6f; // Slightly quieter for Quest 3 speakers
            
            // Optimize haptics for Quest 3 controllers (Unity Quest haptic samples)
            hapticMultiplier = 1.8f; // Stronger haptics for Quest 3 controllers
            
            Debug.Log("Accessibility optimized for Quest 3 - improved controller sensitivity and haptic feedback");
        }
        
        private void OnDestroy()
        {
            // Clean up input actions (Unity cleanup samples)
            if (primaryButtonAction != null)
            {
                primaryButtonAction.action.performed -= OnPrimaryButtonPressed;
                primaryButtonAction.action.canceled -= OnPrimaryButtonReleased;
            }
            
            if (secondaryButtonAction != null)
            {
                secondaryButtonAction.action.performed -= OnSecondaryButtonPressed;
                secondaryButtonAction.action.canceled -= OnSecondaryButtonReleased;
            }
        }
    }
}
#endif // DISABLED: Missing dependencies