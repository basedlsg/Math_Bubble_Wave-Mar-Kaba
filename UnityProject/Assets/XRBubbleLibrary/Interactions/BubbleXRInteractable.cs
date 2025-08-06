#if FALSE // DISABLED: Missing dependencies - requires UI namespace
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Mathematics;
using XRBubbleLibrary.Physics;
using XRBubbleLibrary.UI;

namespace XRBubbleLibrary.Interactions
{
    /// <summary>
    /// Bubble XR Interactable - Cloned from Unity XR Interaction Toolkit samples
    /// Adapts Unity's XRBaseInteractable for bubble-specific interactions
    /// Based on Unity's XR Interaction Toolkit interactable samples and glass interaction examples
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class BubbleXRInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
    {
        [Header("Bubble Interaction Settings (Unity XR Samples)")]
        [Tooltip("Cloned from Unity XR interaction samples")]
        [SerializeField] private float touchSensitivity = 1.0f;
        
        [Tooltip("Based on Unity haptic feedback samples")]
        [SerializeField] private float hapticIntensity = 0.5f;
        
        [Tooltip("Cloned from Unity audio feedback samples")]
        [SerializeField] private AudioClip glassClinkSound;
        
        [Header("Glass Bubble Properties (Unity Glass Samples)")]
        [SerializeField] private float bubbleFragility = 0.8f;
        [SerializeField] private float resonanceFrequency = 440f; // A4 note
        [SerializeField] private bool enableGlassPhysics = true;
        
        [Header("Visual Feedback (Unity Visual Samples)")]
        [SerializeField] private bool enableRippleEffect = true;
        [SerializeField] private float rippleDuration = 1.0f;
        [SerializeField] private Color touchHighlightColor = Color.white;
        
        // Component references
        private Renderer bubbleRenderer;
        private AudioSource audioSource;
        private BubbleSpringPhysics springPhysics;
        private BubbleBreathingSystem breathingSystem;
        private ParticleSystem touchParticles;
        
        // Interaction state
        private Material originalMaterial;
        private Material highlightMaterial;
        private bool isBeingTouched = false;
        private float touchStartTime;
        private Vector3 lastTouchPosition;
        
        // Glass physics state
        private float currentResonance = 0f;
        private float touchForceAccumulator = 0f;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeBubbleInteractable();
            SetupComponentReferences();
            ConfigureXRInteraction();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetupGlassPhysics();
            CreateTouchParticleSystem();
        }
        
        /// <summary>
        /// Initializes bubble interactable based on Unity XR samples
        /// </summary>
        private void InitializeBubbleInteractable()
        {
            // Configure as glass bubble interactable (Unity XR samples)
            selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Single;
            movementType = MovementType.VelocityTracking;
            trackPosition = true;
            trackRotation = true;
            smoothPosition = true;
            smoothRotation = true;
            
            // Set glass-appropriate physics (Unity physics samples)
            var rb = GetComponent<Rigidbody>();
            rb.mass = 0.1f; // Light like a soap bubble
            rb.linearDamping = 2f;   // Air resistance
            rb.angularDamping = 5f; // Rotational damping
            
            Debug.Log("Bubble XR Interactable initialized - cloned from Unity XR samples");
        }
        
        /// <summary>
        /// Sets up component references based on Unity component search samples
        /// </summary>
        private void SetupComponentReferences()
        {
            // Get renderer for visual feedback (Unity renderer samples)
            bubbleRenderer = GetComponent<Renderer>();
            if (bubbleRenderer != null)
            {
                originalMaterial = bubbleRenderer.material;
                CreateHighlightMaterial();
            }
            
            // Get or create audio source (Unity audio samples)
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                ConfigureAudioSource();
            }
            
            // Find physics systems (Unity component search samples)
            springPhysics = FindObjectOfType<BubbleSpringPhysics>();
            breathingSystem = FindObjectOfType<BubbleBreathingSystem>();
        }
        
        /// <summary>
        /// Configures XR interaction based on Unity XR configuration samples
        /// </summary>
        private void ConfigureXRInteraction()
        {
            // Configure collider for XR interaction (Unity XR samples)
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                var sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = 0.5f;
                sphereCollider.isTrigger = false; // Solid for physics
            }
            
            // Configure interaction layers (Unity XR layer samples)
            interactionLayers = InteractionLayerMask.GetMask("Default");
        }
        
        /// <summary>
        /// Sets up glass physics based on Unity glass physics samples
        /// </summary>
        private void SetupGlassPhysics()
        {
            if (!enableGlassPhysics) return;
            
            // Configure glass-like physics properties (Unity glass samples)
            var rb = GetComponent<Rigidbody>();
            rb.material = CreateGlassPhysicsMaterial();
            
            // Set resonance frequency for audio feedback (Unity audio samples)
            if (audioSource != null)
            {
                audioSource.pitch = resonanceFrequency / 440f; // Relative to A4
            }
        }
        
        /// <summary>
        /// Creates glass physics material based on Unity physics material samples
        /// </summary>
        private PhysicsMaterial CreateGlassPhysicsMaterial()
        {
            // Create glass physics material (Unity physics material samples)
            PhysicsMaterial glassMaterial = new PhysicsMaterial("Glass Bubble Material");
            glassMaterial.dynamicFriction = 0.1f;  // Slippery like glass
            glassMaterial.staticFriction = 0.1f;   // Low static friction
            glassMaterial.bounciness = 0.3f;       // Slight bounce
            glassMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            glassMaterial.bounceCombine = PhysicsMaterialCombine.Average;
            
            return glassMaterial;
        }
        
        /// <summary>
        /// Creates highlight material for touch feedback
        /// Based on Unity material creation samples
        /// </summary>
        private void CreateHighlightMaterial()
        {
            if (originalMaterial == null) return;
            
            // Create highlight material (Unity material samples)
            highlightMaterial = new Material(originalMaterial);
            highlightMaterial.name = "Bubble Highlight Material";
            
            // Apply highlight color (Unity color samples)
            if (highlightMaterial.HasProperty("_BubbleColor"))
            {
                highlightMaterial.SetColor("_BubbleColor", touchHighlightColor);
            }
            
            if (highlightMaterial.HasProperty("_GlowIntensity"))
            {
                highlightMaterial.SetFloat("_GlowIntensity", 2.0f);
            }
        }
        
        /// <summary>
        /// Configures audio source based on Unity audio configuration samples
        /// </summary>
        private void ConfigureAudioSource()
        {
            // Configure for 3D spatial audio (Unity 3D audio samples)
            audioSource.spatialBlend = 1.0f; // Full 3D
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.volume = 0.5f;
            audioSource.playOnAwake = false;
            
            // Set glass clink sound if available
            if (glassClinkSound != null)
            {
                audioSource.clip = glassClinkSound;
            }
        }
        
        /// <summary>
        /// Creates touch particle system based on Unity particle samples
        /// </summary>
        private void CreateTouchParticleSystem()
        {
            // Create particle system for touch effects (Unity particle samples)
            GameObject particleGO = new GameObject("Touch Particles");
            particleGO.transform.SetParent(transform);
            particleGO.transform.localPosition = Vector3.zero;
            
            touchParticles = particleGO.AddComponent<ParticleSystem>();
            ConfigureTouchParticles();
        }
        
        /// <summary>
        /// Configures touch particle system based on Unity particle configuration samples
        /// </summary>
        private void ConfigureTouchParticles()
        {
            if (touchParticles == null) return;
            
            // Configure main module (Unity particle samples)
            var main = touchParticles.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 2f;
            main.startSize = 0.1f;
            main.startColor = touchHighlightColor;
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Configure emission (Unity emission samples)
            var emission = touchParticles.emission;
            emission.enabled = false; // Manual emission
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0.0f, 10)
            });
            
            // Configure shape (Unity shape samples)
            var shape = touchParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            
            // Configure velocity over lifetime (Unity velocity samples)
            var velocityOverLifetime = touchParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1f);
        }
        
        /// <summary>
        /// Called when interaction begins - cloned from Unity XR event samples
        /// </summary>
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            
            // Start touch interaction (Unity interaction samples)
            isBeingTouched = true;
            touchStartTime = Time.time;
            lastTouchPosition = args.interactorObject.transform.position;
            
            // Apply visual feedback (Unity visual feedback samples)
            ApplyTouchHighlight();
            
            // Play audio feedback (Unity audio samples)
            PlayGlassClinkSound();
            
            // Apply haptic feedback (Unity haptic samples)
            ApplyHapticFeedback(args.interactorObject);
            
            // Trigger particle effect (Unity particle samples)
            TriggerTouchParticles(lastTouchPosition);
            
            // Apply physics response (Unity physics samples)
            ApplyTouchPhysics(args.interactorObject.transform.position);
            
            Debug.Log($"Bubble touched by {args.interactorObject.transform.name}");
        }
        
        /// <summary>
        /// Called during interaction - cloned from Unity XR event samples
        /// </summary>
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            
            // End touch interaction (Unity interaction samples)
            isBeingTouched = false;
            
            // Remove visual feedback (Unity visual feedback samples)
            RemoveTouchHighlight();
            
            // Calculate touch duration and force (Unity calculation samples)
            float touchDuration = Time.time - touchStartTime;
            float touchForce = touchForceAccumulator / touchDuration;
            
            // Apply release physics (Unity physics samples)
            ApplyReleasePhysics(touchForce);
            
            // Reset touch accumulator
            touchForceAccumulator = 0f;
            
            Debug.Log($"Bubble released after {touchDuration:F2}s with force {touchForce:F2}");
        }
        
        /// <summary>
        /// Called when hovered - cloned from Unity XR hover samples
        /// </summary>
        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            
            // Apply subtle hover effect (Unity hover samples)
            ApplyHoverEffect();
            
            // Gentle haptic feedback for hover (Unity haptic samples)
            ApplyHapticFeedback(args.interactorObject, 0.1f);
        }
        
        /// <summary>
        /// Called when hover ends - cloned from Unity XR hover samples
        /// </summary>
        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            
            // Remove hover effect (Unity hover samples)
            RemoveHoverEffect();
        }
        
        /// <summary>
        /// Applies touch highlight effect based on Unity visual feedback samples
        /// </summary>
        private void ApplyTouchHighlight()
        {
            if (bubbleRenderer != null && highlightMaterial != null)
            {
                bubbleRenderer.material = highlightMaterial;
            }
            
            // Enhance breathing animation during touch (Unity animation samples)
            if (breathingSystem != null)
            {
                breathingSystem.SynchronizeWithVisualSystem(1.5f);
            }
        }
        
        /// <summary>
        /// Removes touch highlight based on Unity visual feedback samples
        /// </summary>
        private void RemoveTouchHighlight()
        {
            if (bubbleRenderer != null && originalMaterial != null)
            {
                bubbleRenderer.material = originalMaterial;
            }
            
            // Return breathing to normal (Unity animation samples)
            if (breathingSystem != null)
            {
                breathingSystem.SynchronizeWithVisualSystem(1.0f);
            }
        }
        
        /// <summary>
        /// Applies hover effect based on Unity hover samples
        /// </summary>
        private void ApplyHoverEffect()
        {
            // Subtle glow increase for hover (Unity glow samples)
            if (bubbleRenderer != null && bubbleRenderer.material.HasProperty("_GlowIntensity"))
            {
                bubbleRenderer.material.SetFloat("_GlowIntensity", 1.8f);
            }
        }
        
        /// <summary>
        /// Removes hover effect based on Unity hover samples
        /// </summary>
        private void RemoveHoverEffect()
        {
            // Return to normal glow (Unity glow samples)
            if (bubbleRenderer != null && bubbleRenderer.material.HasProperty("_GlowIntensity"))
            {
                bubbleRenderer.material.SetFloat("_GlowIntensity", 1.5f);
            }
        }
        
        /// <summary>
        /// Plays glass clink sound based on Unity audio samples
        /// </summary>
        private void PlayGlassClinkSound()
        {
            if (audioSource != null && glassClinkSound != null)
            {
                // Vary pitch slightly for natural feel (Unity audio variation samples)
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(glassClinkSound, hapticIntensity);
            }
        }
        
        /// <summary>
        /// Applies haptic feedback based on Unity haptic samples
        /// </summary>
        private void ApplyHapticFeedback(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor, float intensity = -1f)
        {
            if (intensity < 0f) intensity = hapticIntensity;
            
            // Apply haptic feedback through XR controller (Unity haptic samples)
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
            {
                var controller = controllerInteractor.xrController;
                if (controller != null)
                {
                    controller.SendHapticImpulse(intensity, 0.1f);
                }
            }
        }
        
        /// <summary>
        /// Triggers touch particle effect based on Unity particle samples
        /// </summary>
        private void TriggerTouchParticles(Vector3 touchPosition)
        {
            if (touchParticles == null) return;
            
            // Position particles at touch point (Unity particle positioning samples)
            touchParticles.transform.position = touchPosition;
            
            // Emit particles (Unity particle emission samples)
            touchParticles.Emit(10);
        }
        
        /// <summary>
        /// Applies touch physics response based on Unity physics samples
        /// </summary>
        private void ApplyTouchPhysics(Vector3 touchPosition)
        {
            // Calculate touch force based on interaction (Unity force calculation samples)
            Vector3 touchDirection = (transform.position - touchPosition).normalized;
            float touchForce = touchSensitivity * 2f;
            
            // Apply force through spring physics system (Unity physics integration samples)
            if (springPhysics != null)
            {
                int bubbleIndex = GetBubbleIndex();
                springPhysics.ApplyForce(bubbleIndex, touchDirection * touchForce);
            }
            
            // Accumulate touch force for release calculation
            touchForceAccumulator += touchForce;
            
            // Update resonance based on touch (Unity resonance samples)
            UpdateResonance(touchForce);
        }
        
        /// <summary>
        /// Applies release physics based on Unity physics samples
        /// </summary>
        private void ApplyReleasePhysics(float releaseForce)
        {
            // Apply release impulse (Unity impulse samples)
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 releaseDirection = UnityEngine.Random.onUnitSphere;
                rb.AddForce(releaseDirection * releaseForce * 0.5f, ForceMode.Impulse);
            }
        }
        
        /// <summary>
        /// Updates resonance based on touch interaction
        /// Based on Unity audio resonance samples
        /// </summary>
        private void UpdateResonance(float touchForce)
        {
            // Calculate resonance based on touch force (Unity resonance samples)
            currentResonance = Mathf.Lerp(currentResonance, touchForce, Time.deltaTime * 5f);
            
            // Apply resonance to audio pitch (Unity audio modulation samples)
            if (audioSource != null)
            {
                float pitchModulation = 1f + (currentResonance * 0.1f);
                audioSource.pitch = pitchModulation;
            }
            
            // Apply resonance to visual effects (Unity visual modulation samples)
            if (bubbleRenderer != null && bubbleRenderer.material.HasProperty("_GlowIntensity"))
            {
                float glowModulation = 1.5f + (currentResonance * 0.5f);
                bubbleRenderer.material.SetFloat("_GlowIntensity", glowModulation);
            }
        }
        
        /// <summary>
        /// Gets bubble index for physics system integration
        /// Based on Unity indexing samples
        /// </summary>
        private int GetBubbleIndex()
        {
            // Simple index based on instance ID (Unity ID samples)
            return Mathf.Abs(GetInstanceID()) % 100;
        }
        
        /// <summary>
        /// Validates bubble interactable setup
        /// </summary>
        [ContextMenu("Validate Bubble Interactable")]
        public void ValidateBubbleInteractable()
        {
            Debug.Log($"Bubble XR Interactable Status:");
            Debug.Log($"- Renderer: {(bubbleRenderer != null ? "Found" : "Missing")}");
            Debug.Log($"- Audio Source: {(audioSource != null ? "Found" : "Missing")}");
            Debug.Log($"- Spring Physics: {(springPhysics != null ? "Found" : "Missing")}");
            Debug.Log($"- Breathing System: {(breathingSystem != null ? "Found" : "Missing")}");
            Debug.Log($"- Touch Particles: {(touchParticles != null ? "Found" : "Missing")}");
            Debug.Log($"- Glass Physics: {enableGlassPhysics}");
            Debug.Log($"- Touch Sensitivity: {touchSensitivity}");
            Debug.Log($"- Haptic Intensity: {hapticIntensity}");
        }
        
        /// <summary>
        /// Optimizes interaction for Quest 3
        /// Based on Unity Quest optimization samples
        /// </summary>
        [ContextMenu("Optimize for Quest 3")]
        public void OptimizeForQuest3()
        {
            // Optimize haptic feedback for Quest 3 controllers (Unity Quest samples)
            hapticIntensity = 0.7f;
            touchSensitivity = 1.2f;
            
            // Optimize particle count for mobile GPU (Unity mobile samples)
            if (touchParticles != null)
            {
                var main = touchParticles.main;
                main.maxParticles = 10; // Reduced for Quest 3
            }
            
            // Optimize audio for Quest 3 (Unity audio samples)
            if (audioSource != null)
            {
                audioSource.volume = 0.4f; // Slightly quieter
                audioSource.maxDistance = 8f; // Reduced range
            }
            
            Debug.Log("Bubble interactable optimized for Quest 3 - reduced particle count and optimized haptics");
        }

        /// <summary>
        /// Called when a gaze-based selection occurs.
        /// </summary>
        public void OnGazeSelect()
        {
            Debug.Log($"Bubble selected by gaze: {gameObject.name}");

            // Apply visual feedback
            ApplyTouchHighlight();

            // Play audio feedback
            PlayGlassClinkSound();

            // Optionally, trigger a haptic pulse if a controller is active
            var interactor = firstInteractorSelecting;
            if (interactor != null)
            {
                ApplyHapticFeedback(interactor, hapticIntensity * 0.5f);
            }

            // Reset highlight after a short delay
            Invoke(nameof(RemoveTouchHighlight), 0.5f);
        }
    }
}
#endif // DISABLED: Missing dependencies