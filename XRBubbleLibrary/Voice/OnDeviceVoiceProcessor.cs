#if EXP_VOICE
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

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
        public bool enableVoiceProcessing = true;
        
        [Range(0.1f, 1.0f)]
        public float voiceActivationThreshold = 0.3f;
        
        [Range(1.0f, 10.0f)]
        public float maxRecordingDuration = 5.0f;
        
        [Header("Spatial Command Processing")]
#if EXP_AI
        public LocalAIModel aiModel;
        public bool enableAIEnhancement = true;
#else
        [SerializeField] private bool enableAIEnhancement = false;
#endif
        
        [Range(0.5f, 1.0f)]
        public float commandConfidenceThreshold = 0.7f;
        
        [Header("Performance Optimization")]
        [Range(8000, 48000)]
        public int sampleRate = 16000; // Optimized for Quest 3
        
        [Range(512, 4096)]
        public int bufferSize = 1024;
        
        public bool enableNoiseReduction = true;
        
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
        
        /// <summary>
        /// Initialize the voice processing system
        /// </summary>
        void InitializeVoiceProcessor()
        {
            // Initialize audio source for playback
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            
            // Initialize on-device voice model
            voiceModel = new OnDeviceVoiceModel();
            voiceModel.Initialize(sampleRate, bufferSize);
            
            // Get microphone device
            if (Microphone.devices.Length > 0)
            {
                microphoneDevice = Microphone.devices[0];
                Debug.Log($"Voice processor initialized with microphone: {microphoneDevice}");
            }
            else
            {
                Debug.LogWarning("No microphone devices found. Voice processing disabled.");
                enableVoiceProcessing = false;
            }
            
            // Initialize AI model reference
#if EXP_AI
            if (aiModel == null)
            {
                aiModel = FindObjectOfType<LocalAIModel>();
                if (aiModel == null)
                {
                    Debug.LogWarning("LocalAIModel not found. AI-enhanced voice processing disabled.");
                    enableAIEnhancement = false;
                }
            }
#else
            if (enableAIEnhancement)
            {
                Debug.LogWarning("[OnDeviceVoiceProcessor] AI enhancement requested but EXP_AI is disabled. Enable EXP_AI compiler flag to use AI features.");
                enableAIEnhancement = false;
            }
#endif
        }
        
        /// <summary>
        /// Initialize spatial command templates
        /// </summary>
        void InitializeCommandTemplates()
        {
            commandTemplates = new Dictionary<string, SpatialCommandTemplate>
            {
                ["circle"] = new SpatialCommandTemplate
                {
                    pattern = ArrangementPattern.Circle,
                    keywords = new[] { "circle", "round", "around" },
                    defaultRadius = 1.5f,
                    confidence = 0.9f
                },
                ["line"] = new SpatialCommandTemplate
                {
                    pattern = ArrangementPattern.Line,
                    keywords = new[] { "line", "row", "straight" },
                    defaultRadius = 2.0f,
                    confidence = 0.8f
                },
                ["grid"] = new SpatialCommandTemplate
                {
                    pattern = ArrangementPattern.Grid,
                    keywords = new[] { "grid", "square", "matrix" },
                    defaultRadius = 1.8f,
                    confidence = 0.85f
                },
                ["spiral"] = new SpatialCommandTemplate
                {
                    pattern = ArrangementPattern.Spiral,
                    keywords = new[] { "spiral", "twist", "curve" },
                    defaultRadius = 2.2f,
                    confidence = 0.75f
                },
                ["close"] = new SpatialCommandTemplate
                {
                    pattern = ArrangementPattern.Circle,
                    keywords = new[] { "close", "near", "closer" },
                    defaultRadius = 1.0f,
                    confidence = 0.8f
                },
                ["far"] = new SpatialCommandTemplate
                {
                    pattern = ArrangementPattern.Circle,
                    keywords = new[] { "far", "away", "further" },
                    defaultRadius = 2.5f,
                    confidence = 0.8f
                }
            };
            
            Debug.Log($"Initialized {commandTemplates.Count} voice command templates");
        }
        
        /// <summary>
        /// Process voice input each frame
        /// </summary>
        void ProcessVoiceInput()
        {
            // Check for voice activation
            if (!isRecording && DetectVoiceActivation())
            {
                StartVoiceRecording();
            }
            
            // Check for recording completion
            if (isRecording && ShouldStopRecording())
            {
                StopVoiceRecording();
            }
        }
        
        /// <summary>
        /// Detect voice activation based on audio level
        /// </summary>
        bool DetectVoiceActivation()
        {
            if (string.IsNullOrEmpty(microphoneDevice)) return false;
            
            // Simple voice activation detection
            // In production, would use more sophisticated VAD
            float micLevel = GetMicrophoneLevel();
            return micLevel > voiceActivationThreshold;
        }
        
        /// <summary>
        /// Start voice recording
        /// </summary>
        void StartVoiceRecording()
        {
            if (string.IsNullOrEmpty(microphoneDevice)) return;
            
            recordingStartTime = Time.time;
            recordedClip = Microphone.Start(microphoneDevice, false, (int)maxRecordingDuration, sampleRate);
            isRecording = true;
            
            Debug.Log("Voice recording started");
        }
        
        /// <summary>
        /// Check if recording should stop
        /// </summary>
        bool ShouldStopRecording()
        {
            if (!isRecording) return false;
            
            // Stop if max duration reached
            if (Time.time - recordingStartTime >= maxRecordingDuration)
            {
                return true;
            }
            
            // Stop if silence detected for 1 second
            if (Time.time - recordingStartTime > 1.0f && GetMicrophoneLevel() < voiceActivationThreshold * 0.5f)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Stop voice recording and process command
        /// </summary>
        async void StopVoiceRecording()
        {
            if (!isRecording) return;
            
            Microphone.End(microphoneDevice);
            isRecording = false;
            
            Debug.Log("Voice recording stopped, processing command...");
            
            // Process the recorded audio
            await ProcessVoiceCommand(recordedClip);
        }
        
        /// <summary>
        /// Process voice command from recorded audio
        /// </summary>
        async Task ProcessVoiceCommand(AudioClip audioClip)
        {
            var startTime = Time.realtimeSinceStartup;
            
            try
            {
                // Convert audio to text using on-device model
                string transcribedText = await TranscribeAudio(audioClip);
                
                if (string.IsNullOrEmpty(transcribedText))
                {
                    Debug.Log("No speech detected in recording");
                    return;
                }
                
                Debug.Log($"Transcribed: \"{transcribedText}\"");
                
                // Process spatial command
                var spatialCommand = await ProcessSpatialCommand(transcribedText);
                
                if (spatialCommand.confidence >= commandConfidenceThreshold)
                {
                    // Execute the spatial command
                    ExecuteSpatialCommand(spatialCommand);
                    
                    // Track successful command
                    recentCommands.Add(new VoiceCommand
                    {
                        transcribedText = transcribedText,
                        spatialCommand = spatialCommand,
                        timestamp = Time.time,
                        processingTimeMs = (Time.realtimeSinceStartup - startTime) * 1000f
                    });
                    
                    processedCommandsCount++;
                }
                else
                {
                    Debug.Log($"Command confidence too low: {spatialCommand.confidence:F2} < {commandConfidenceThreshold:F2}");
                }
                
                // Track performance
                lastProcessingTime = (Time.realtimeSinceStartup - startTime) * 1000f;
                
                // Clean up old commands
                CleanupOldCommands();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Voice command processing failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Transcribe audio to text using on-device model
        /// </summary>
        async Task<string> TranscribeAudio(AudioClip audioClip)
        {
            if (audioClip == null) return "";
            
            // Get audio data
            float[] audioData = new float[audioClip.samples];
            audioClip.GetData(audioData, 0);
            
            // Apply noise reduction if enabled
            if (enableNoiseReduction)
            {
                audioData = ApplyNoiseReduction(audioData);
            }
            
            // Use on-device voice model for transcription
            return await voiceModel.TranscribeAudio(audioData, sampleRate);
        }
        
        /// <summary>
        /// Process spatial command from transcribed text
        /// </summary>
        async Task<SpatialCommand> ProcessSpatialCommand(string text)
        {
            // First, try template matching for common commands
            var templateCommand = MatchCommandTemplate(text);
            
            if (templateCommand.confidence >= commandConfidenceThreshold)
            {
                return templateCommand;
            }
            
            // If AI enhancement is enabled, use AI for complex commands
#if EXP_AI
            if (enableAIEnhancement && aiModel != null)
            {
                try
                {
                    var userPosition = Camera.main?.transform.position ?? float3.zero;
                    return await ProcessAIEnhancedCommand(text, userPosition);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"AI command processing failed: {ex.Message}");
                }
            }
#endif
            
            // Return low-confidence fallback
            return new SpatialCommand
            {
                action = SpatialAction.None,
                positions = new float3[0],
                pattern = ArrangementPattern.None,
                confidence = 0.0f
            };
        }
        
        /// <summary>
        /// Match command against predefined templates
        /// </summary>
        SpatialCommand MatchCommandTemplate(string text)
        {
            string lowerText = text.ToLower();
            
            foreach (var template in commandTemplates.Values)
            {
                foreach (var keyword in template.keywords)
                {
                    if (lowerText.Contains(keyword))
                    {
                        return new SpatialCommand
                        {
                            action = SpatialAction.Arrange,
                            positions = GeneratePatternPositions(template.pattern, template.defaultRadius),
                            pattern = template.pattern,
                            confidence = template.confidence
                        };
                    }
                }
            }
            
            return new SpatialCommand
            {
                action = SpatialAction.None,
                positions = new float3[0],
                pattern = ArrangementPattern.None,
                confidence = 0.0f
            };
        }
        
#if EXP_AI
        /// <summary>
        /// Process command using AI enhancement
        /// </summary>
        async Task<SpatialCommand> ProcessAIEnhancedCommand(string text, float3 userPosition)
        {
            // Use Groq API client through AI model for complex spatial reasoning
            var groqClient = new GroqAPIClient();
            return await groqClient.ProcessVoiceCommand(text, userPosition);
        }
#endif
        
        /// <summary>
        /// Generate positions for arrangement patterns
        /// </summary>
        float3[] GeneratePatternPositions(ArrangementPattern pattern, float radius)
        {
            var userPosition = Camera.main?.transform.position ?? float3.zero;
            var positions = new List<float3>();
            
            switch (pattern)
            {
                case ArrangementPattern.Circle:
                    positions.AddRange(GenerateCirclePositions(userPosition, radius, 8));
                    break;
                    
                case ArrangementPattern.Line:
                    positions.AddRange(GenerateLinePositions(userPosition, radius, 6));
                    break;
                    
                case ArrangementPattern.Grid:
                    positions.AddRange(GenerateGridPositions(userPosition, radius, 3, 3));
                    break;
                    
                case ArrangementPattern.Spiral:
                    positions.AddRange(GenerateSpiralPositions(userPosition, radius, 10));
                    break;
            }
            
            return positions.ToArray();
        }
        
        /// <summary>
        /// Generate circle arrangement positions
        /// </summary>
        float3[] GenerateCirclePositions(float3 center, float radius, int count)
        {
            var positions = new float3[count];
            float angleStep = 2 * math.PI / count;
            
            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                positions[i] = center + new float3(
                    math.cos(angle) * radius,
                    0,
                    math.sin(angle) * radius + 1.5f // In front of user
                );
            }
            
            return positions;
        }
        
        /// <summary>
        /// Generate line arrangement positions
        /// </summary>
        float3[] GenerateLinePositions(float3 center, float length, int count)
        {
            var positions = new float3[count];
            float step = length / (count - 1);
            
            for (int i = 0; i < count; i++)
            {
                positions[i] = center + new float3(
                    (i - count / 2.0f) * step,
                    0,
                    1.5f // In front of user
                );
            }
            
            return positions;
        }
        
        /// <summary>
        /// Generate grid arrangement positions
        /// </summary>
        float3[] GenerateGridPositions(float3 center, float spacing, int rows, int cols)
        {
            var positions = new float3[rows * cols];
            int index = 0;
            
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    positions[index++] = center + new float3(
                        (col - cols / 2.0f) * spacing,
                        (row - rows / 2.0f) * spacing,
                        1.5f // In front of user
                    );
                }
            }
            
            return positions;
        }
        
        /// <summary>
        /// Generate spiral arrangement positions
        /// </summary>
        float3[] GenerateSpiralPositions(float3 center, float maxRadius, int count)
        {
            var positions = new float3[count];
            
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                float angle = t * 4 * math.PI; // 2 full rotations
                float radius = t * maxRadius;
                
                positions[i] = center + new float3(
                    math.cos(angle) * radius,
                    0,
                    math.sin(angle) * radius + 1.5f
                );
            }
            
            return positions;
        }
        
        /// <summary>
        /// Execute spatial command
        /// </summary>
        void ExecuteSpatialCommand(SpatialCommand command)
        {
            Debug.Log($"Executing spatial command: {command.action} with {command.positions.Length} positions");
            
            // Broadcast command to bubble system
            var bubbleManager = FindObjectOfType<XRBubbleLibrary.UI.BubbleUIManager>();
            if (bubbleManager != null)
            {
                // Would call bubble manager to arrange bubbles according to command
                // bubbleManager.ArrangeBubbles(command.positions, command.pattern);
            }
            
            // Provide audio feedback
            PlayCommandFeedback(command.confidence);
        }
        
        /// <summary>
        /// Play audio feedback for command execution
        /// </summary>
        void PlayCommandFeedback(float confidence)
        {
            // Play confirmation sound based on confidence
            if (audioSource != null)
            {
                // Would play appropriate feedback sound
                audioSource.pitch = 0.8f + confidence * 0.4f; // Higher pitch for higher confidence
                // audioSource.PlayOneShot(confirmationSound);
            }
        }
        
        /// <summary>
        /// Get current microphone level
        /// </summary>
        float GetMicrophoneLevel()
        {
            if (string.IsNullOrEmpty(microphoneDevice) || !Microphone.IsRecording(microphoneDevice))
            {
                return 0.0f;
            }
            
            // Simple level detection
            return UnityEngine.Random.Range(0.0f, 1.0f); // Simulated for now
        }
        
        /// <summary>
        /// Apply noise reduction to audio data
        /// </summary>
        float[] ApplyNoiseReduction(float[] audioData)
        {
            // Simple noise gate implementation
            float noiseThreshold = 0.01f;
            
            for (int i = 0; i < audioData.Length; i++)
            {
                if (math.abs(audioData[i]) < noiseThreshold)
                {
                    audioData[i] = 0.0f;
                }
            }
            
            return audioData;
        }
        
        /// <summary>
        /// Clean up old commands to maintain performance
        /// </summary>
        void CleanupOldCommands()
        {
            float currentTime = Time.time;
            recentCommands.RemoveAll(cmd => currentTime - cmd.timestamp > 60.0f); // Keep 1 minute history
        }
        
        /// <summary>
        /// Get voice processing performance metrics
        /// </summary>
        public VoiceProcessorMetrics GetPerformanceMetrics()
        {
            return new VoiceProcessorMetrics
            {
                lastProcessingTimeMs = lastProcessingTime,
                processedCommandsCount = processedCommandsCount,
                recentCommandsCount = recentCommands.Count,
                isRecording = isRecording,
                voiceProcessingEnabled = enableVoiceProcessing,
                aiEnhancementEnabled = enableAIEnhancement
            };
        }
        
        /// <summary>
        /// Manually trigger voice command processing (for testing)
        /// </summary>
        [FeatureGate(ExperimentalFeature.VOICE_PROCESSING)]
        public async void ProcessTestCommand(string testCommand)
        {
            CompilerFlags.ValidateFeatureAccess(ExperimentalFeature.VOICE_PROCESSING);
            
            Debug.Log($"Processing test command: \"{testCommand}\"");
            
            var spatialCommand = await ProcessSpatialCommand(testCommand);
            
            if (spatialCommand.confidence >= commandConfidenceThreshold)
            {
                ExecuteSpatialCommand(spatialCommand);
            }
        }
    }
    
    /// <summary>
    /// Simulated on-device voice recognition model
    /// </summary>
    public class OnDeviceVoiceModel
    {
        private int sampleRate;
        private int bufferSize;
        private bool initialized = false;
        
        public void Initialize(int sampleRate, int bufferSize)
        {
            this.sampleRate = sampleRate;
            this.bufferSize = bufferSize;
            this.initialized = true;
            
            Debug.Log($"On-device voice model initialized: {sampleRate}Hz, {bufferSize} buffer");
        }
        
        public async Task<string> TranscribeAudio(float[] audioData, int sampleRate)
        {
            if (!initialized) return "";
            
            // Simulate processing time (20-50ms target)
            await Task.Delay(UnityEngine.Random.Range(20, 50));
            
            // Simulated transcription - in production would use actual speech recognition
            string[] possibleCommands = {
                "arrange in circle",
                "make a line",
                "create grid",
                "spiral pattern",
                "move closer",
                "spread out",
                "delete all",
                "reset position"
            };
            
            // Simple simulation based on audio energy
            float energy = 0.0f;
            foreach (float sample in audioData)
            {
                energy += sample * sample;
            }
            energy /= audioData.Length;
            
            if (energy > 0.001f) // If there's significant audio
            {
                return possibleCommands[UnityEngine.Random.Range(0, possibleCommands.Length)];
            }
            
            return "";
        }
    }
    
    /// <summary>
    /// Spatial command template for pattern matching
    /// </summary>
    [System.Serializable]
    public struct SpatialCommandTemplate
    {
        public ArrangementPattern pattern;
        public string[] keywords;
        public float defaultRadius;
        public float confidence;
    }
    
    /// <summary>
    /// Voice command with processing metadata
    /// </summary>
    [System.Serializable]
    public struct VoiceCommand
    {
        public string transcribedText;
        public SpatialCommand spatialCommand;
        public float timestamp;
        public float processingTimeMs;
    }
    
    /// <summary>
    /// Voice processor performance metrics
    /// </summary>
    [System.Serializable]
    public struct VoiceProcessorMetrics
    {
        public float lastProcessingTimeMs;
        public int processedCommandsCount;
        public int recentCommandsCount;
        public bool isRecording;
        public bool voiceProcessingEnabled;
        public bool aiEnhancementEnabled;
        
        public override string ToString()
        {
            return $"Voice Processor - Processing: {lastProcessingTimeMs:F2}ms, Commands: {processedCommandsCount}, " +
                   $"Recording: {isRecording}, AI: {aiEnhancementEnabled}";
        }
    }
}

#else
// Stub implementation when voice processing is disabled
using UnityEngine;
using Unity.Mathematics;

namespace XRBubbleLibrary.Voice
{
    /// <summary>
    /// Stub implementation of OnDeviceVoiceProcessor when voice processing is disabled.
    /// Provides graceful degradation and clear messaging about disabled features.
    /// </summary>
    public class OnDeviceVoiceProcessor : MonoBehaviour
    {
        void Start()
        {
            Debug.LogWarning("[OnDeviceVoiceProcessor] Voice Processing is disabled. Enable EXP_VOICE compiler flag to use voice recognition features.");
            
            // Optionally disable this GameObject to prevent confusion
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Stub method for test commands
        /// </summary>
        public void ProcessTestCommand(string testCommand)
        {
            Debug.LogWarning("[OnDeviceVoiceProcessor] Voice Processing is disabled. Enable EXP_VOICE compiler flag to use voice commands.");
        }
        
        /// <summary>
        /// Stub method for performance metrics
        /// </summary>
        public VoiceProcessorMetrics GetPerformanceMetrics()
        {
            return new VoiceProcessorMetrics
            {
                lastProcessingTimeMs = 0f,
                processedCommandsCount = 0,
                recentCommandsCount = 0,
                isRecording = false,
                voiceProcessingEnabled = false,
                aiEnhancementEnabled = false
            };
        }
    }
    
    /// <summary>
    /// Stub voice processor metrics for disabled state
    /// </summary>
    [System.Serializable]
    public struct VoiceProcessorMetrics
    {
        public float lastProcessingTimeMs;
        public int processedCommandsCount;
        public int recentCommandsCount;
        public bool isRecording;
        public bool voiceProcessingEnabled;
        public bool aiEnhancementEnabled;
        
        public override string ToString()
        {
            return "Voice Processor - DISABLED (Enable EXP_VOICE to use voice features)";
        }
    }
    
    /// <summary>
    /// Stub spatial command for disabled state
    /// </summary>
    [System.Serializable]
    public struct SpatialCommand
    {
        public SpatialAction action;
        public float3[] positions;
        public ArrangementPattern pattern;
        public float confidence;
    }
    
    /// <summary>
    /// Spatial actions enum (always available for API consistency)
    /// </summary>
    public enum SpatialAction
    {
        None,
        Arrange,
        Create,
        Delete,
        Move
    }
    
    /// <summary>
    /// Arrangement patterns enum (always available for API consistency)
    /// </summary>
    public enum ArrangementPattern
    {
        None,
        Circle,
        Line,
        Grid,
        Spiral
    }
}
#endif