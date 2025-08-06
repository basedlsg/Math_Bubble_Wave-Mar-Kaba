using UnityEngine;
using XRBubbleLibrary.WaveMatrix;
using XRBubbleLibrary.WordBubbles;

namespace XRBubbleLibrary.Setup
{
    /// <summary>
    /// Quick setup script to get the wave matrix word interface working for testing
    /// </summary>
    public class QuickDemoSetup : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private int numberOfBubbles = 20;
        [SerializeField] private string[] demoWords = {
            "Hello", "World", "Unity", "VR", "Quest", "Wave", "Matrix", "Bubble", 
            "Voice", "AI", "Interface", "Demo", "Test", "Working", "Great", 
            "Awesome", "Cool", "Nice", "Perfect", "Done"
        };
        
        [Header("Components")]
        [SerializeField] private WaveMatrixPositioner wavePositioner;
        [SerializeField] private BreathingAnimationSystem breathingSystem;
        [SerializeField] private WordBubbleFactory bubbleFactory;
        
        void Start()
        {
            SetupDemo();
        }
        
        void SetupDemo()
        {
            // Get or create required components
            if (wavePositioner == null)
                wavePositioner = FindObjectOfType<WaveMatrixPositioner>();
            
            if (breathingSystem == null)
                breathingSystem = FindObjectOfType<BreathingAnimationSystem>();
            
            if (bubbleFactory == null)
                bubbleFactory = FindObjectOfType<WordBubbleFactory>();
            
            // Create components if they don't exist
            if (wavePositioner == null)
            {
                var waveGO = new GameObject("WaveMatrixPositioner");
                wavePositioner = waveGO.AddComponent<WaveMatrixPositioner>();
            }
            
            if (breathingSystem == null)
            {
                var breathingGO = new GameObject("BreathingAnimationSystem");
                breathingSystem = breathingGO.AddComponent<BreathingAnimationSystem>();
            }
            
            if (bubbleFactory == null)
            {
                var factoryGO = new GameObject("WordBubbleFactory");
                bubbleFactory = factoryGO.AddComponent<WordBubbleFactory>();
            }
            
            // Create demo bubbles
            CreateDemoBubbles();
            
            Debug.Log($"Quick Demo Setup Complete: {numberOfBubbles} bubbles created");
        }
        
        void CreateDemoBubbles()
        {
            if (bubbleFactory == null) return;
            
            // Prepare words and random confidences
            string[] words = new string[numberOfBubbles];
            float[] confidences = new float[numberOfBubbles];
            
            for (int i = 0; i < numberOfBubbles; i++)
            {
                words[i] = demoWords[i % demoWords.Length];
                confidences[i] = Random.Range(0.2f, 1.0f);
            }
            
            // Create bubbles
            bubbleFactory.CreateWordBubbles(words, confidences);
        }
        
        [ContextMenu("Recreate Demo")]
        public void RecreateDemo()
        {
            if (bubbleFactory != null)
            {
                bubbleFactory.ClearAllBubbles();
                CreateDemoBubbles();
            }
        }
        
        [ContextMenu("Clear All Bubbles")]
        public void ClearDemo()
        {
            if (bubbleFactory != null)
            {
                bubbleFactory.ClearAllBubbles();
            }
        }
    }
}