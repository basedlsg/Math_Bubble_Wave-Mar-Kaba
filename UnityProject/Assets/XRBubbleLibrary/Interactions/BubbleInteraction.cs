using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using XRBubbleLibrary.Physics;
using XRBubbleLibrary.Core;

namespace XRBubbleLibrary.Interactions
{
    /// <summary>
    /// XR interaction handling for bubbles with haptic and audio feedback
    /// Cloned from XR Interaction Toolkit samples, modified for glass bubble interactions
    /// </summary>
    [RequireComponent(typeof(BubbleSpringPhysics))]
    public class BubbleInteraction : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable, IBubbleInteraction
    {
        [Header("Feedback Settings")]
        [SerializeField] private AudioClip glassClinkSound;
        [SerializeField] private float hapticIntensity = 0.5f;
        [SerializeField] private float hapticDuration = 0.1f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Renderer bubbleRenderer;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material pressedMaterial;
        
        // Component references
        private BubbleSpringPhysics bubblePhysics;
        private AudioSource audioSource;
        
        // Interaction state
        private bool isPressed = false;
        public new bool isHovered = false;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Get required components
            bubblePhysics = GetComponent<BubbleSpringPhysics>();
            audioSource = GetComponent<AudioSource>();
            
            // Create audio source if not present
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // 3D spatial audio
                audioSource.volume = 0.7f;
                audioSource.pitch = Random.Range(0.9f, 1.1f); // Slight pitch variation
            }
            
            // Set up materials if not assigned
            if (bubbleRenderer == null)
                bubbleRenderer = GetComponent<Renderer>();
        }
        
        /// <summary>
        /// Called when hand/controller starts hovering over bubble
        /// Cloned from XR Interaction Toolkit hover samples
        /// </summary>
        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            
            isHovered = true;
            
            // Visual feedback - highlight material
            if (bubbleRenderer != null && highlightMaterial != null)
            {
                bubbleRenderer.material = highlightMaterial;
            }
            
            // Subtle haptic feedback for hover
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
            {
                controllerInteractor.SendHapticImpulse(hapticIntensity * 0.3f, hapticDuration * 0.5f);
            }
        }
        
        /// <summary>
        /// Called when hand/controller stops hovering over bubble
        /// </summary>
        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            
            isHovered = false;
            
            // Return to normal material if not pressed
            if (!isPressed && bubbleRenderer != null && normalMaterial != null)
            {
                bubbleRenderer.material = normalMaterial;
            }
        }
        
        /// <summary>
        /// Called when bubble is pressed/selected
        /// Cloned from XR Interaction Toolkit select samples, modified for glass bubble feel
        /// </summary>
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            
            isPressed = true;
            
            // Visual feedback - pressed material
            if (bubbleRenderer != null && pressedMaterial != null)
            {
                bubbleRenderer.material = pressedMaterial;
            }
            
            // Audio feedback - glass clink sound
            PlayGlassClinkSound();
            
            // Haptic feedback - stronger for press
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
            {
                controllerInteractor.SendHapticImpulse(hapticIntensity, hapticDuration);
            }
        }
        
        /// <summary>
        /// Called when bubble is released
        /// </summary>
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            
            isPressed = false;
            
            // Visual feedback - return to hover or normal state
            if (bubbleRenderer != null)
            {
                if (isHovered && highlightMaterial != null)
                {
                    bubbleRenderer.material = highlightMaterial;
                }
                else if (normalMaterial != null)
                {
                    bubbleRenderer.material = normalMaterial;
                }
            }
            
            // Subtle haptic feedback for release
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
            {
                controllerInteractor.SendHapticImpulse(hapticIntensity * 0.5f, hapticDuration * 0.7f);
            }
        }
        
        /// <summary>
        /// Play glass clinking sound with slight randomization
        /// Based on cloned Unity audio samples
        /// </summary>
        private void PlayGlassClinkSound()
        {
            if (audioSource != null && glassClinkSound != null)
            {
                // Randomize pitch slightly for natural variation
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                
                // Play the sound
                audioSource.PlayOneShot(glassClinkSound);
            }
        }
        
        /// <summary>
        /// Update interaction settings at runtime
        /// </summary>
        public void UpdateInteractionSettings(float newHapticIntensity, AudioClip newSound)
        {
            hapticIntensity = Mathf.Clamp01(newHapticIntensity);
            glassClinkSound = newSound;
        }
        
        /// <summary>
        /// Enable/disable interaction based on bubble state
        /// </summary>
        public void SetInteractionEnabled(bool enabled)
        {
            this.enabled = enabled;
            
            if (!enabled)
            {
                // Reset visual state when disabled
                if (bubbleRenderer != null && normalMaterial != null)
                {
                    bubbleRenderer.material = normalMaterial;
                }
                
                isPressed = false;
                isHovered = false;
            }
        }
        
        // IBubbleInteraction implementation
        public Transform BubbleTransform => transform;
        public Renderer BubbleRenderer => bubbleRenderer;
        public bool IsBeingInteracted => isPressed || isHovered;
        public Vector3 InteractionPosition => transform.position;
        
        public void ApplyPhysicsForce(Vector3 force)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
        
        public void UpdateVisualScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }
    }
}
