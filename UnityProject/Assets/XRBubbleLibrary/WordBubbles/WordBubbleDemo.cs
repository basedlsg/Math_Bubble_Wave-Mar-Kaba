using UnityEngine;
using XRBubbleLibrary.WaveMatrix;

namespace XRBubbleLibrary.WordBubbles
{
    /// <summary>
    /// Demo component to test word bubble system
    /// Creates sample word bubbles and demonstrates functionality
    /// </summary>
    public class WordBubbleDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private bool createDemoBubbles = true;
        [SerializeField] private int numberOfBubbles = 10;
        [SerializeField] private bool randomizeConfidence = true;
        
        [Header("Demo Words")]
        [SerializeField] private string[] demoWords = {
            "Hello", "World", "Unity", "VR", "Bubble", "Wave", "Matrix", "Text", "Interface", "Demo"
        };
        
        [Header("Components")]
        [SerializeField] private WordBubbleFactory bubbleFactory;
        [SerializeField] private WaveMatrixPositioner wavePositioner;
        [SerializeField] private BreathingAnimationSystem breathingSystem;
        
        [Header("Visual Settings")]
        [SerializeField] private BubbleVisualSettings visualSettings = BubbleVisualSettings.Default;
        
        // Demo state
        private WordBubble[] demoBubbles;
        private float lastUpdateTime;
        private int currentPresetIndex = 0;
        
        // Visual presets for testing
        private BubbleVisualSettings[] visualPresets = {
            BubbleVisualSettings.Default,
            BubbleVisualSettings.HighContrast,
            BubbleVisualSettings.Subtle,
            BubbleVisualSettings.Vibrant,
            BubbleVisualSettings.Glass
        };
        
        void Start()
        {
            InitializeDemo();
        }
        
        void InitializeDemo()
        {
            // Get or create components
            if (bubbleFactory == null)
                bubbleFactory = GetComponent<WordBubbleFactory>();
            
            if (wavePositioner == null)
                wavePositioner = FindObjectOfType<WaveMatrixPositioner>();
            
            if (breathingSystem == null)
                breathingSystem = FindObjectOfType<BreathingAnimationSystem>();
            
            // Create demo bubbles if enabled
            if (createDemoBubbles)
            {
                CreateDemoBubbles();
            }
            
            Debug.Log($"WordBubbleDemo initialized with {numberOfBubbles} demo bubbles");
        }
        
        void CreateDemoBubbles()
        {
            if (bubbleFactory == null)
            {
                Debug.LogWarning("WordBubbleFactory not found - cannot create demo bubbles");
                return;
            }
            
            // Prepare words and confidences
            string[] words = PrepareWords();
            float[] confidences = PrepareConfidences();
            
            // Create bubbles
            demoBubbles = bubbleFactory.CreateWordBubbles(words, confidences);
            
            Debug.Log($"Created {demoBubbles.Length} demo word bubbles");
        }
        
        string[] PrepareWords()
        {
            string[] words = new string[numberOfBubbles];
            
            for (int i = 0; i < numberOfBubbles; i++)
            {
                if (i < demoWords.Length)
                {
                    words[i] = demoWords[i];
                }
                else
                {
                    // Generate additional words if needed
                    words[i] = $"Word{i + 1}";
                }
            }
            
            return words;
        }
        
        float[] PrepareConfidences()
        {
            float[] confidences = new float[numberOfBubbles];
            
            for (int i = 0; i < numberOfBubbles; i++)
            {
                if (randomizeConfidence)
                {
                    confidences[i] = Random.Range(0.1f, 1.0f);
                }
                else
                {
                    // Create gradient from high to low confidence
                    confidences[i] = Mathf.Lerp(1.0f, 0.1f, (float)i / (numberOfBubbles - 1));
                }
            }
            
            return confidences;
        }
        
        void Update()
        {
            // Demo controls
            HandleDemoControls();
            
            // Periodic updates
            if (Time.time - lastUpdateTime > 5f)
            {
                UpdateDemoState();
                lastUpdateTime = Time.time;
            }
        }
        
        void HandleDemoControls()
        {
            // Keyboard controls for testing
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CycleVisualPresets();
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                RecreateAllBubbles();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearAllBubbles();
            }
            
            if (Input.GetKeyDown(KeyCode.B))
            {
                ToggleBreathing();
            }
        }
        
        void UpdateDemoState()
        {
            // Update AI confidences randomly for demo effect
            if (demoBubbles != null && randomizeConfidence)
            {
                foreach (var bubble in demoBubbles)
                {
                    if (bubble != null)
                    {
                        float newConfidence = Random.Range(0.1f, 1.0f);
                        bubble.SetAIConfidence(newConfidence);
                    }
                }
            }
        }
        
        /// <summary>
        /// Cycle through visual presets for testing
        /// </summary>
        public void CycleVisualPresets()
        {
            currentPresetIndex = (currentPresetIndex + 1) % visualPresets.Length;
            visualSettings = visualPresets[currentPresetIndex];
            
            if (bubbleFactory != null)
            {
                bubbleFactory.UpdateAllBubbleVisuals(visualSettings);
            }
            
            Debug.Log($"Switched to visual preset: {GetPresetName(currentPresetIndex)}");
        }
        
        string GetPresetName(int index)
        {
            string[] names = { "Default", "High Contrast", "Subtle", "Vibrant", "Glass" };
            return index < names.Length ? names[index] : "Unknown";
        }
        
        /// <summary>
        /// Recreate all demo bubbles
        /// </summary>
        public void RecreateAllBubbles()
        {
            ClearAllBubbles();
            CreateDemoBubbles();
            Debug.Log("Recreated all demo bubbles");
        }
        
        /// <summary>
        /// Clear all demo bubbles
        /// </summary>
        public void ClearAllBubbles()
        {
            if (bubbleFactory != null)
            {
                bubbleFactory.ClearAllBubbles();
            }
            demoBubbles = null;
            Debug.Log("Cleared all demo bubbles");
        }
        
        /// <summary>
        /// Toggle breathing animation
        /// </summary>
        public void ToggleBreathing()
        {
            if (breathingSystem != null)
            {
                breathingSystem.enabled = !breathingSystem.enabled;
                Debug.Log($"Breathing animation: {(breathingSystem.enabled ? "Enabled" : "Disabled")}");
            }
        }
        
        /// <summary>
        /// Add a new word bubble at runtime
        /// </summary>
        public void AddWordBubble(string word, float confidence = 0.5f)
        {
            if (bubbleFactory != null)
            {
                var bubble = bubbleFactory.CreateWordBubble(word, confidence);
                Debug.Log($"Added word bubble: '{word}' (confidence: {confidence:F2})");
            }
        }
        
        /// <summary>
        /// Remove a word bubble by word
        /// </summary>
        public void RemoveWordBubble(string word)
        {
            if (bubbleFactory != null)
            {
                var bubble = bubbleFactory.FindBubbleByWord(word);
                if (bubble != null)
                {
                    bubbleFactory.DestroyWordBubble(bubble);
                    Debug.Log($"Removed word bubble: '{word}'");
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
                Debug.Log("Updated wave matrix settings");
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
                Debug.Log("Updated breathing animation settings");
            }
        }
        
        // GUI for runtime testing
        void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 50, 300, 400));
            
            GUILayout.Label("Word Bubble Demo Controls", GUI.skin.box);
            
            if (GUILayout.Button("Cycle Visual Presets (Space)"))
                CycleVisualPresets();
            
            if (GUILayout.Button("Recreate Bubbles (R)"))
                RecreateAllBubbles();
            
            if (GUILayout.Button("Clear All Bubbles (C)"))
                ClearAllBubbles();
            
            if (GUILayout.Button("Toggle Breathing (B)"))
                ToggleBreathing();
            
            GUILayout.Space(10);
            
            GUILayout.Label($"Current Preset: {GetPresetName(currentPresetIndex)}");
            
            if (bubbleFactory != null)
            {
                GUILayout.Label($"Active Bubbles: {bubbleFactory.GetActiveBubbleCount()}");
            }
            
            if (breathingSystem != null)
            {
                GUILayout.Label($"Breathing: {(breathingSystem.enabled ? "On" : "Off")}");
            }
            
            GUILayout.EndArea();
        }
    }
}