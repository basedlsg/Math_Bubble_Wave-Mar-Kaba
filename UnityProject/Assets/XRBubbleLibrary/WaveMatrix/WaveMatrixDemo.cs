using UnityEngine;
using Unity.Mathematics;

namespace XRBubbleLibrary.WaveMatrix
{
    /// <summary>
    /// Demo component to test wave matrix positioning and breathing animation
    /// Creates visual representation of the wave matrix system
    /// </summary>
    public class WaveMatrixDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private GameObject bubblePrefab;
        [SerializeField] private int numberOfBubbles = 25;
        [SerializeField] private bool showWaveMatrix = true;
        [SerializeField] private bool enableBreathing = true;
        
        [Header("Components")]
        [SerializeField] private WaveMatrixPositioner wavePositioner;
        [SerializeField] private BreathingAnimationSystem breathingSystem;
        
        // Demo bubbles
        private GameObject[] demoBubbles;
        private DemoBreathingBubble[] breathingComponents;
        
        void Start()
        {
            InitializeDemo();
        }
        
        void InitializeDemo()
        {
            // Get or create components
            if (wavePositioner == null)
                wavePositioner = GetComponent<WaveMatrixPositioner>();
            
            if (breathingSystem == null)
                breathingSystem = GetComponent<BreathingAnimationSystem>();
            
            // Create demo bubbles
            CreateDemoBubbles();
            
            Debug.Log($"Wave Matrix Demo initialized with {numberOfBubbles} bubbles");
        }
        
        void CreateDemoBubbles()
        {
            demoBubbles = new GameObject[numberOfBubbles];
            breathingComponents = new DemoBreathingBubble[numberOfBubbles];
            
            for (int i = 0; i < numberOfBubbles; i++)
            {
                // Create bubble GameObject
                GameObject bubble = CreateDemoBubble(i);
                demoBubbles[i] = bubble;
                
                // Add breathing component if enabled
                if (enableBreathing && breathingSystem != null)
                {
                    var breathingComponent = bubble.AddComponent<DemoBreathingBubble>();
                    breathingComponents[i] = breathingComponent;
                    breathingSystem.RegisterBreathingElement(breathingComponent);
                }
            }
        }
        
        GameObject CreateDemoBubble(int index)
        {
            GameObject bubble;
            
            if (bubblePrefab != null)
            {
                bubble = Instantiate(bubblePrefab, transform);
            }
            else
            {
                // Create simple sphere if no prefab provided
                bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bubble.transform.parent = transform;
                bubble.transform.localScale = Vector3.one * 0.3f;
                
                // Add simple material
                var renderer = bubble.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    material.color = Color.HSVToRGB((index * 0.1f) % 1f, 0.7f, 0.9f);
                    material.SetFloat("_Metallic", 0.2f);
                    material.SetFloat("_Smoothness", 0.8f);
                    renderer.material = material;
                }
            }
            
            bubble.name = $"DemoBubble_{index}";
            return bubble;
        }
        
        void Update()
        {
            if (showWaveMatrix && wavePositioner != null)
            {
                UpdateBubblePositions();
            }
        }
        
        void UpdateBubblePositions()
        {
            for (int i = 0; i < numberOfBubbles && i < demoBubbles.Length; i++)
            {
                if (demoBubbles[i] != null)
                {
                    // Get position from wave matrix
                    float3 wavePosition = wavePositioner.GetBubblePosition(i);
                    demoBubbles[i].transform.localPosition = wavePosition;
                }
            }
        }
        
        /// <summary>
        /// Toggle wave matrix visualization
        /// </summary>
        public void ToggleWaveMatrix()
        {
            showWaveMatrix = !showWaveMatrix;
            
            for (int i = 0; i < demoBubbles.Length; i++)
            {
                if (demoBubbles[i] != null)
                {
                    demoBubbles[i].SetActive(showWaveMatrix);
                }
            }
        }
        
        /// <summary>
        /// Toggle breathing animation
        /// </summary>
        public void ToggleBreathing()
        {
            enableBreathing = !enableBreathing;
            
            if (breathingSystem != null)
            {
                for (int i = 0; i < breathingComponents.Length; i++)
                {
                    if (breathingComponents[i] != null)
                    {
                        if (enableBreathing)
                        {
                            breathingSystem.RegisterBreathingElement(breathingComponents[i]);
                        }
                        else
                        {
                            breathingSystem.UnregisterBreathingElement(breathingComponents[i]);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Update wave settings for testing
        /// </summary>
        public void SetWaveSettings(WaveMatrixSettings settings)
        {
            if (wavePositioner != null)
            {
                wavePositioner.UpdateWaveSettings(settings);
            }
        }
        
        /// <summary>
        /// Update breathing settings for testing
        /// </summary>
        public void SetBreathingSettings(BreathingSettings settings)
        {
            if (breathingSystem != null)
            {
                breathingSystem.UpdateBreathingSettings(settings);
            }
        }
    }
    
    /// <summary>
    /// Simple breathing element for demo bubbles
    /// </summary>
    public class DemoBreathingBubble : MonoBehaviour, IBreathingElement
    {
        private Vector3 originalScale;
        private Renderer bubbleRenderer;
        private Material bubbleMaterial;
        private Color originalColor;
        
        void Start()
        {
            originalScale = transform.localScale;
            bubbleRenderer = GetComponent<Renderer>();
            
            if (bubbleRenderer != null)
            {
                bubbleMaterial = bubbleRenderer.material;
                originalColor = bubbleMaterial.color;
            }
        }
        
        public void ApplyBreathing(BreathingValues values)
        {
            // Apply scale breathing
            transform.localScale = originalScale * values.scale;
            
            // Apply opacity breathing
            if (bubbleMaterial != null)
            {
                Color breathingColor = originalColor;
                breathingColor.a = values.opacity;
                bubbleMaterial.color = breathingColor;
            }
        }
    }
}