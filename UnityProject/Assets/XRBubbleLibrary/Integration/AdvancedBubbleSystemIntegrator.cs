using UnityEngine;
using XRBubbleLibrary.AI;
using XRBubbleLibrary.Audio;
using XRBubbleLibrary.Interactions;
using XRBubbleLibrary.Mathematics;
using XRBubbleLibrary.Performance;
using XRBubbleLibrary.Physics;
using XRBubbleLibrary.UI;
using XRBubbleLibrary.Voice;

namespace XRBubbleLibrary.Integration
{
    /// <summary>
    /// Central coordinator for the entire XR Bubble Library.
    /// This advanced integrator manages the multi-sensory feedback loops,
    /// AI-driven behaviors, and performance optimizations to create a
    /// cohesive, cutting-edge user experience.
    /// </summary>
    public class AdvancedBubbleSystemIntegrator : MonoBehaviour
    {
        [Header("System References")]
        public BubblePerformanceManager performanceManager;
        public BubbleSystemIntegrator baseIntegrator;
        public OnDeviceVoiceProcessor voiceProcessor;
        public SteamAudioRenderer audioRenderer;
        public GroqAPIClient cloudAIClient;
        public LocalAIModel localAIModel;
        public AdvancedWaveSystem waveSystem;
        public BubbleLayoutManager layoutManager;
        public BubbleHapticFeedback hapticFeedback;
        public CymaticsController cymaticsController;
        public EyeTrackingController eyeTrackingController;

        [Header("Configuration")]
        public bool enableMultiSensoryFeedback = true;
        public bool enableAIAssistance = true;
        public bool enableAdvancedMathematics = true;

        private static AdvancedBubbleSystemIntegrator _instance;
        public static AdvancedBubbleSystemIntegrator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AdvancedBubbleSystemIntegrator>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("AdvancedBubbleSystemIntegrator");
                        _instance = obj.AddComponent<AdvancedBubbleSystemIntegrator>();
                    }
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            InitializeSystems();
        }

        void Update()
        {
            CoordinateSystems();
        }

        private void InitializeSystems()
        {
            // Ensure all required components are available
            performanceManager = BubblePerformanceManager.Instance;
            audioRenderer = FindObjectOfType<SteamAudioRenderer>();
            cymaticsController = FindObjectOfType<CymaticsController>();
            eyeTrackingController = FindObjectOfType<EyeTrackingController>();

            if (cymaticsController != null && audioRenderer != null)
            {
                cymaticsController.audioRenderer = audioRenderer;
            }
            
            Debug.Log("Advanced Bubble System Integrator Initialized and Systems Connected.");
        }

        private void CoordinateSystems()
        {
            if (!enableMultiSensoryFeedback && !enableAIAssistance) return;

            // Example of system coordination:
            // 1. Get performance data
            var perfStats = performanceManager.GetStatistics();

            // 2. Get AI insights
            if (enableAIAssistance)
            {
                // Process voice, context, etc.
            }

            // 3. Update audio based on physics and AI
            if (enableMultiSensoryFeedback)
            {
                // Link wave patterns to audio renderer
            }

            // 4. Update haptics based on interactions and audio
            if (enableMultiSensoryFeedback)
            {
                // Link audio events to haptic feedback
            }
        }

        // Placeholder for future methods
        public void OnBubbleInteraction(BubbleXRInteractable bubble, XRInteractionData data)
        {
            if (!enableMultiSensoryFeedback) return;

            // Coordinate multi-sensory response to interaction
            // e.g., audioRenderer.PlayInteractionSound(bubble.transform.position);
            // e.g., hapticFeedback.TriggerInteractionHaptic(data.InputSource);
        }

        public async void OnVoiceCommand(string command)
        {
            if (!enableAIAssistance || cloudAIClient == null || layoutManager == null) return;

            // Get user position for context
            var userPosition = Camera.main.transform.position;

            // Process voice command through AI
            var spatialCommand = await cloudAIClient.ProcessVoiceCommand(command, userPosition);

            // Apply the resulting spatial layout
            if (spatialCommand.confidence > 0.5f)
            {
                layoutManager.ApplySpatialArrangement(spatialCommand.positions, spatialCommand.pattern);
            }
        }
    }
}
