#if EXP_VOICE
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using XRBubbleLibrary.Core;

// Conditional AI import
#if EXP_AI
using XRBubbleLibrary.AI;
#endif

namespace XRBubbleLibrary.Voice
{
    /// <summary>
    /// On-device voice processing system for XR bubble commands
    /// Optimized for Quest 3 with 20-50ms latency target
    /// Integrates with AI system for voice-to-spatial-arrangement
    /// 
    /// EXPERIMENTAL FEATURE: Requires EXP_VOICE compiler flag
    /// Part of the "do-it-right" recovery Phase 0 implementation
    /// </summary>
    [FeatureGate(ExperimentalFeature.VOICE_PROCESSING,
                 ErrorMessage = "On-Device Voice Processor requires Voice Processing to be enabled")]
    public class OnDeviceVoiceProcessor : MonoBehaviour
    {
        [Header("Voice Recognition Configuration")]
        [SerializeField] private bool enableVoiceProcessing = true;
        
        [Range(0.1f, 1.0f)]
        [SerializeField] private float voiceActivationThreshold = 0.3f;
        
        [Range(1.0f, 10.0f)]
        [SerializeField] private float maxRecordingDuration = 5.0f;
        
        [Header("Spatial Command Processing")]
#if EXP_AI
        [SerializeField] private LocalAIModel aiModel;
        [SerializeField] private bool enableAIEnhancement = true;
#else
        [SerializeField] private bool enableAIEnhancement = false;
#endif
        
        [Range(0.5f, 1.0f)]
        [SerializeField] private float commandConfidenceThreshold = 0.7f;
        
        [Header("Performance Optimization")]
        [Range(8000, 48000)]
        [SerializeField] private int sampleRate = 16000; // Optimized for Quest 3
        
        [Range(512, 4096)]
        [SerializeField] private int bufferSize = 1024;
        
        [SerializeField] private bool enableNoiseReduction = true;
        
        // Voice recording
        private AudioClip recordedClip;
        private bool isRecording = false;
        private float recordingStartTime = 0.0f;
        
        // Voice recognition (simulated on-device model)
        private OnDeviceVoiceModel voiceModel;
        private Queue<float> audioBuffer = new Queue<float>();
        
        // Command processing
        private Dictionary<string, SpatialCommandTemplate> commandTemplates;
        private List<VoiceCommand> recentCommands = new List<VoiceCommand>();
        
        // Performance tracking
        private float lastProcessingTime = 0.0f;
        private int processedCommandsCount = 0;
        
        // Audio input
        private string microphoneDevice;
        private AudioSource audioSource;
        
        void Start()
        {
            // Validate feature access before initialization
            CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.VOICE_PROCESSING);
            
            InitializeVoiceProcessor();
            InitializeCommandTemplates();
        }
        
        void Update()
        {
            if (enableVoiceProcessing)
            {
                ProcessVoiceInput();
            }
        }