using UnityEngine;
using XRBubbleLibrary.Core;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Simple wave parameters for basic breathing animation
    /// </summary>
    [System.Serializable]
    public class SimpleWaveParams
    {
        public float primaryFrequency = 1.0f;
        public float primaryAmplitude = 0.1f;
        public float secondaryFrequency = 2.0f;
        public float secondaryAmplitude = 0.05f;
        public float tertiaryFrequency = 4.0f;
        public float tertiaryAmplitude = 0.025f;
    }

    /// <summary>
    /// Simple bubble test component to validate wave mathematics integration
    /// Creates a basic bubble with wave-driven breathing animation
    /// </summary>
    public class SimpleBubbleTest : MonoBehaviour, IBubbleInteraction
    {
        [Header("Wave Mathematics Test")]
        public SimpleWaveParams waveParams = new SimpleWaveParams();
        public bool enableBreathing = true;
        
        [Header("Visual Components")]
        public Renderer bubbleRenderer;
        
        // Internal state
        private Vector3 originalScale;
        private float time = 0f;
        private bool isInteracted = false;
        
        void Start()
        {
            // Store original scale
            originalScale = transform.localScale;
            
            // Get renderer if not assigned
            if (bubbleRenderer == null)
                bubbleRenderer = GetComponent<Renderer>();
            
            // Create basic sphere if no mesh
            if (GetComponent<MeshFilter>() == null)
            {
                var meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = CreateSphereMesh();
            }
            
            // Create basic material if no renderer
            if (bubbleRenderer == null)
            {
                bubbleRenderer = gameObject.AddComponent<MeshRenderer>();
                bubbleRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                bubbleRenderer.material.color = Color.blue;
            }
            
            // Add collider for interaction
            if (GetComponent<Collider>() == null)
            {
                var collider = gameObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
            }
        }
        
        void Update()
        {
            if (enableBreathing)
            {
                UpdateBreathingAnimation();
            }
        }
        
        void UpdateBreathingAnimation()
        {
            time += Time.deltaTime;
            
            // Calculate breathing scale using wave mathematics
            float breathingScale = 1.0f;
            
            // Primary wave (main breathing rhythm)
            breathingScale += Mathf.Sin(time * waveParams.primaryFrequency * 2f * Mathf.PI) * waveParams.primaryAmplitude;
            
            // Secondary wave (harmonic enhancement)
            breathingScale += Mathf.Sin(time * waveParams.secondaryFrequency * 2f * Mathf.PI) * waveParams.secondaryAmplitude;
            
            // Tertiary wave (fine detail)
            breathingScale += Mathf.Sin(time * waveParams.tertiaryFrequency * 2f * Mathf.PI) * waveParams.tertiaryAmplitude;
            
            // Apply breathing scale
            transform.localScale = originalScale * breathingScale;
        }
        
        Mesh CreateSphereMesh()
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var mesh = sphere.GetComponent<MeshFilter>().mesh;
            DestroyImmediate(sphere);
            return mesh;
        }
        
        // IBubbleInteraction implementation
        public Transform BubbleTransform => transform;
        public Renderer BubbleRenderer => bubbleRenderer;
        public bool IsBeingInteracted => isInteracted;
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
            transform.localScale = originalScale * scale;
        }
        
        // Simple interaction handling
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Hand") || other.CompareTag("Controller"))
            {
                isInteracted = true;
                if (bubbleRenderer != null)
                {
                    bubbleRenderer.material.color = Color.red;
                }
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Hand") || other.CompareTag("Controller"))
            {
                isInteracted = false;
                if (bubbleRenderer != null)
                {
                    bubbleRenderer.material.color = Color.blue;
                }
            }
        }
    }
}