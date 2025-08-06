using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Threading.Tasks;
using XRBubbleLibrary.AI;
using XRBubbleLibrary.Mathematics;
using XRBubbleLibrary.Audio;
using XRBubbleLibrary.Voice;
using XRBubbleLibrary.UI;
using XRBubbleLibrary.Performance;

namespace XRBubbleLibrary.Integration
{
    /// <summary>
    /// AI-Enhanced Bubble System - Master integration component
    /// Combines mathematical wave physics, AI optimization, spatial audio, and voice control
    /// Optimized for Quest 3 with 72fps performance target
    /// </summary>
    public class AIEnhancedBubbleSystem : MonoBehaviour
    {
        [Header("System Components")]
        public LocalAIModel aiModel;
        public AdvancedWaveSystem waveSystem;
        public SteamAudioRenderer audioRenderer;
        public OnDeviceVoiceProcessor voiceProcessor;
        public BubbleUIManager bubbleUIManager;
        public BubblePerformanceManager performanceManager;
        
        [Header("Integration Configuration")]
        public bool enableAIEnhancement = true;
        public bool enableVoiceControl = true;
        public bool enableSpatialAudio = true;
        public bool enablePerformanceOptimization = true;
        
        [Range(0.0f, 1.0f)]
        public float aiInfluenceStrength = 0.3f;
        
        [Header("User Experience")]
        [Range(5, 50)]
        public int maxActiveBubbles = 20;
        
        [Range(0.5f, 3.0f)]
        public float bubbleInteractionRange = 1.5f;
        
        public bool enableHapticFeedback = true;
        
        // System state
        private List<EnhancedBubble> activeBubbles = new List<EnhancedBubble>();
        private Dictionary<int, BubbleState> bubbleStates = new Dictionary<int, BubbleState>();
        
        // Performance tracking
        private SystemPerformanceMetrics performanceMetrics;
        private float lastSystemUpdateTime = 0.0f;
        
        // User interaction tracking
        private float3 lastUserPosition;
        private float lastInteractionTime = 0.0f;
        
        // System events
        public System.Action<EnhancedBubble> OnBubbleCreated;
        public System.Action<EnhancedBubble> OnBubbleDestroyed;
        public System.Action<SpatialCommand> OnVoiceCommandExecuted;
        
        void Start()
        {
            InitializeIntegratedSystem();
        }
        
        void Update()
        {
            UpdateIntegratedSystem();
        }
        
        void OnDestroy()
        {
            CleanupIntegratedSystem();
        }
        
        /// <summary>
        /// Initialize the complete AI-enhanced bubble system
        /// </summary>
        void InitializeIntegratedSystem()
        {
            Debug.Log("Initializing AI-Enhanced Bubble System...");
            
            // Initialize or find system components
            InitializeSystemComponents();
            
            // Setup component interactions
            SetupComponentIntegration();
            
            // Initialize user tracking
            lastUserPosition = Camera.main?.transform.position ?? float3.zero;
            
            // Create initial bubble arrangement
            CreateInitialBubbleArrangement();
            
            Debug.Log("AI-Enhanced Bubble System initialized successfully");
        }
        
        /// <summary>
        /// Initialize all system components
        /// </summary>
        void InitializeSystemComponents()
        {
            // AI Model
            if (aiModel == null)
            {
                aiModel = FindObjectOfType<LocalAIModel>();
                if (aiModel == null && enableAIEnhancement)
                {
                    var aiObject = new GameObject("LocalAIModel");
                    aiObject.transform.SetParent(transform);
                    aiModel = aiObject.AddComponent<LocalAIModel>();
                }
            }
            
            // Wave System
            if (waveSystem == null)
            {
                waveSystem = FindObjectOfType<AdvancedWaveSystem>();
                if (waveSystem == null)
                {
                    var waveObject = new GameObject("AdvancedWaveSystem");
                    waveObject.transform.SetParent(transform);
                    waveSystem = waveObject.AddComponent<AdvancedWaveSystem>();
                    waveSystem.aiModel = aiModel;
                }
            }
            
            // Audio Renderer
            if (audioRenderer == null && enableSpatialAudio)
            {
                audioRenderer = FindObjectOfType<SteamAudioRenderer>();
                if (audioRenderer == null)
                {
                    var audioObject = new GameObject("SteamAudioRenderer");
                    audioObject.transform.SetParent(transform);
                    audioRenderer = audioObject.AddComponent<SteamAudioRenderer>();
                }
            }
            
            // Voice Processor
            if (voiceProcessor == null && enableVoiceControl)
            {
                voiceProcessor = FindObjectOfType<OnDeviceVoiceProcessor>();
                if (voiceProcessor == null)
                {
                    var voiceObject = new GameObject("OnDeviceVoiceProcessor");
                    voiceObject.transform.SetParent(transform);
                    voiceProcessor = voiceObject.AddComponent<OnDeviceVoiceProcessor>();
                    voiceProcessor.aiModel = aiModel;
                }
            }
            
            // Performance Manager
            if (performanceManager == null && enablePerformanceOptimization)
            {
                performanceManager = BubblePerformanceManager.Instance;
            }
        }
        
        /// <summary>
        /// Setup integration between components
        /// </summary>
        void SetupComponentIntegration()
        {
            // Connect wave system to AI model
            if (waveSystem != null && aiModel != null)
            {
                waveSystem.aiModel = aiModel;
                waveSystem.enableAIOptimization = enableAIEnhancement;
            }
            
            // Connect voice processor to AI model
            if (voiceProcessor != null && aiModel != null)
            {
                voiceProcessor.aiModel = aiModel;
                voiceProcessor.enableAIEnhancement = enableAIEnhancement;
            }
            
            // Setup performance monitoring
            if (performanceManager != null)
            {
                performanceManager.enableAdaptiveQuality = enablePerformanceOptimization;
            }
        }
        
        /// <summary>
        /// Update the integrated system each frame
        /// </summary>
        void UpdateIntegratedSystem()
        {
            var startTime = Time.realtimeSinceStartup;
            
            // Update user position tracking
            UpdateUserTracking();
            
            // Update bubble states
            UpdateBubbleStates();
            
            // Process user interactions
            ProcessUserInteractions();
            
            // Update performance metrics
            UpdatePerformanceMetrics();
            
            // Track system performance
            lastSystemUpdateTime = (Time.realtimeSinceStartup - startTime) * 1000f;
        }
        
        /// <summary>
        /// Create initial bubble arrangement using AI-enhanced wave system
        /// </summary>
        async void CreateInitialBubbleArrangement()
        {
            if (waveSystem == null) return;
            
            var userPosition = Camera.main?.transform.position ?? float3.zero;
            
            // Generate optimal positions using hybrid AI + wave physics
            var positions = await waveSystem.GenerateOptimalBubblePositions(8, userPosition);
            
            // Create bubbles at generated positions
            for (int i = 0; i < positions.Length; i++)
            {
                await CreateEnhancedBubble(positions[i], i);
            }
        }
        
        /// <summary>
        /// Create an enhanced bubble with full system integration
        /// </summary>
        public async Task<EnhancedBubble> CreateEnhancedBubble(float3 position, int bubbleId)
        {
            // Create bubble object
            var bubbleObject = new GameObject($"EnhancedBubble_{bubbleId}");
            bubbleObject.transform.position = position;
            bubbleObject.transform.SetParent(transform);
            
            // Add enhanced bubble component
            var enhancedBubble = bubbleObject.AddComponent<EnhancedBubble>();
            enhancedBubble.Initialize(bubbleId, this);
            
            // Register with wave system
            if (waveSystem != null)
            {
                waveSystem.RegisterBubble(bubbleId, position);
            }
            
            // Setup audio
            if (audioRenderer != null)
            {
                enhancedBubble.SetupAudio(audioRenderer);
            }
            
            // Register with AI model for learning
            if (aiModel != null && enableAIEnhancement)
            {
                var interactionEvent = new InteractionEvent
                {
                    type = InteractionType.BubbleCreate,
                    bubblePosition = position,
                    userPosition = Camera.main?.transform.position ?? float3.zero,
                    timestamp = Time.time,
                    timeOfDay = System.DateTime.Now.Hour
                };
                
                aiModel.LearnFromInteraction(interactionEvent);
            }
            
            // Add to active bubbles
            activeBubbles.Add(enhancedBubble);
            bubbleStates[bubbleId] = new BubbleState
            {
                position = position,
                isActive = true,
                creationTime = Time.time
            };
            
            // Trigger event
            OnBubbleCreated?.Invoke(enhancedBubble);
            
            // Play creation sound
            if (audioRenderer != null)
            {
                audioRenderer.PlayGlassClinkSound(position, 0.8f);
            }
            
            return enhancedBubble;
        }
        
        /// <summary>
        /// Process voice command for spatial arrangement
        /// </summary>
        public async void ProcessVoiceCommand(SpatialCommand command)
        {
            if (command.confidence < 0.5f) return;
            
            Debug.Log($"Processing voice command: {command.action} with {command.positions.Length} positions");
            
            switch (command.action)
            {
                case SpatialAction.Arrange:
                    await ArrangeBubblesAtPositions(command.positions);
                    break;
                    
                case SpatialAction.Create:
                    await CreateBubblesAtPositions(command.positions);
                    break;
                    
                case SpatialAction.Delete:
                    DestroyAllBubbles();
                    break;
                    
                case SpatialAction.Move:
                    await MoveBubblesToPositions(command.positions);
                    break;
            }
            
            // Trigger event
            OnVoiceCommandExecuted?.Invoke(command);
            
            // Play confirmation sound
            if (audioRenderer != null)
            {
                var userPos = Camera.main?.transform.position ?? float3.zero;
                audioRenderer.PlayGlassClinkSound(userPos, command.confidence);
            }
        }
        
        /// <summary>
        /// Arrange existing bubbles at new positions
        /// </summary>
        async Task ArrangeBubblesAtPositions(float3[] positions)
        {
            int bubbleCount = math.min(activeBubbles.Count, positions.Length);
            
            for (int i = 0; i < bubbleCount; i++)
            {
                if (i < activeBubbles.Count)
                {
                    await MoveBubbleToPosition(activeBubbles[i], positions[i]);
                }
            }
            
            // Create additional bubbles if needed
            if (positions.Length > activeBubbles.Count)
            {
                for (int i = activeBubbles.Count; i < positions.Length; i++)
                {
                    await CreateEnhancedBubble(positions[i], GetNextBubbleId());
                }
            }
        }
        
        /// <summary>
        /// Create new bubbles at specified positions
        /// </summary>
        async Task CreateBubblesAtPositions(float3[] positions)
        {
            foreach (var position in positions)
            {
                if (activeBubbles.Count < maxActiveBubbles)
                {
                    await CreateEnhancedBubble(position, GetNextBubbleId());
                }
            }
        }
        
        /// <summary>
        /// Move bubble to new position with animation
        /// </summary>
        async Task MoveBubbleToPosition(EnhancedBubble bubble, float3 targetPosition)
        {
            if (bubble == null) return;
            
            // Animate movement (simplified)
            var startPosition = bubble.transform.position;
            float duration = 1.0f;
            float elapsed = 0.0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Smooth interpolation
                bubble.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                
                await Task.Yield();
            }
            
            // Update bubble state
            if (bubbleStates.ContainsKey(bubble.BubbleId))
            {
                var state = bubbleStates[bubble.BubbleId];
                state.position = targetPosition;
                bubbleStates[bubble.BubbleId] = state;
            }
        }
        
        /// <summary>
        /// Update user position tracking
        /// </summary>
        void UpdateUserTracking()
        {
            var currentUserPosition = Camera.main?.transform.position ?? float3.zero;
            
            // Check if user moved significantly
            if (math.distance(currentUserPosition, lastUserPosition) > 0.1f)
            {
                lastUserPosition = currentUserPosition;
                
                // Notify AI model of user movement
                if (aiModel != null && enableAIEnhancement)
                {
                    var interactionEvent = new InteractionEvent
                    {
                        type = InteractionType.GazeInteraction,
                        bubblePosition = float3.zero,
                        userPosition = currentUserPosition,
                        timestamp = Time.time,
                        timeOfDay = System.DateTime.Now.Hour
                    };
                    
                    aiModel.LearnFromInteraction(interactionEvent);
                }
            }
        }
        
        /// <summary>
        /// Update bubble states and physics
        /// </summary>
        void UpdateBubbleStates()
        {
            foreach (var bubble in activeBubbles)
            {
                if (bubble != null && bubbleStates.ContainsKey(bubble.BubbleId))
                {
                    var state = bubbleStates[bubble.BubbleId];
                    state.position = bubble.transform.position;
                    bubbleStates[bubble.BubbleId] = state;
                }
            }
        }
        
        /// <summary>
        /// Process user interactions with bubbles
        /// </summary>
        void ProcessUserInteractions()
        {
            // Simple interaction detection (would be enhanced with XR input)
            if (Input.GetMouseButtonDown(0))
            {
                ProcessTouchInteraction();
            }
        }
        
        /// <summary>
        /// Process touch/click interaction
        /// </summary>
        void ProcessTouchInteraction()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var enhancedBubble = hit.collider.GetComponent<EnhancedBubble>();
                if (enhancedBubble != null)
                {
                    ProcessBubbleInteraction(enhancedBubble, hit.point);
                }
            }
        }
        
        /// <summary>
        /// Process interaction with specific bubble
        /// </summary>
        void ProcessBubbleInteraction(EnhancedBubble bubble, float3 interactionPoint)
        {
            lastInteractionTime = Time.time;
            
            // Play interaction sound
            if (audioRenderer != null)
            {
                audioRenderer.PlayGlassClinkSound(interactionPoint, 1.0f);
            }
            
            // Haptic feedback
            if (enableHapticFeedback)
            {
                // Would trigger haptic feedback here
            }
            
            // Learn from interaction
            if (aiModel != null && enableAIEnhancement)
            {
                var interactionEvent = new InteractionEvent
                {
                    type = InteractionType.BubbleTouch,
                    bubblePosition = bubble.transform.position,
                    userPosition = Camera.main?.transform.position ?? float3.zero,
                    timestamp = Time.time,
                    timeOfDay = System.DateTime.Now.Hour
                };
                
                aiModel.LearnFromInteraction(interactionEvent);
            }
        }
        
        /// <summary>
        /// Update system performance metrics
        /// </summary>
        void UpdatePerformanceMetrics()
        {
            performanceMetrics = new SystemPerformanceMetrics
            {
                activeBubbleCount = activeBubbles.Count,
                lastSystemUpdateTimeMs = lastSystemUpdateTime,
                aiModelMetrics = aiModel?.GetPerformanceMetrics() ?? default,
                waveSystemMetrics = waveSystem?.GetPerformanceMetrics() ?? default,
                audioMetrics = audioRenderer?.GetPerformanceMetrics() ?? default,
                voiceMetrics = voiceProcessor?.GetPerformanceMetrics() ?? default
            };
        }
        
        /// <summary>
        /// Destroy all bubbles
        /// </summary>
        void DestroyAllBubbles()
        {
            foreach (var bubble in activeBubbles)
            {
                if (bubble != null)
                {
                    OnBubbleDestroyed?.Invoke(bubble);
                    Destroy(bubble.gameObject);
                }
            }
            
            activeBubbles.Clear();
            bubbleStates.Clear();
        }
        
        /// <summary>
        /// Get next available bubble ID
        /// </summary>
        int GetNextBubbleId()
        {
            int id = 0;
            while (bubbleStates.ContainsKey(id))
            {
                id++;
            }
            return id;
        }
        
        /// <summary>
        /// Get comprehensive system performance metrics
        /// </summary>
        public SystemPerformanceMetrics GetSystemMetrics()
        {
            UpdatePerformanceMetrics();
            return performanceMetrics;
        }
        
        /// <summary>
        /// Cleanup integrated system
        /// </summary>
        void CleanupIntegratedSystem()
        {
            DestroyAllBubbles();
            
            // Cleanup would be handled by individual components
            Debug.Log("AI-Enhanced Bubble System cleaned up");
        }
        
        /// <summary>
        /// Enable or disable AI enhancement
        /// </summary>
        public void SetAIEnhancement(bool enabled)
        {
            enableAIEnhancement = enabled;
            
            if (waveSystem != null)
                waveSystem.enableAIOptimization = enabled;
                
            if (voiceProcessor != null)
                voiceProcessor.enableAIEnhancement = enabled;
        }
        
        /// <summary>
        /// Test voice command (for debugging)
        /// </summary>
        public async void TestVoiceCommand(string command)
        {
            if (voiceProcessor != null)
            {
                voiceProcessor.ProcessTestCommand(command);
            }
        }
    }
    
    /// <summary>
    /// Enhanced bubble component with full system integration
    /// </summary>
    public class EnhancedBubble : MonoBehaviour
    {
        public int BubbleId { get; private set; }
        
        private AIEnhancedBubbleSystem parentSystem;
        private SteamAudioRenderer audioRenderer;
        private float creationTime;
        
        public void Initialize(int bubbleId, AIEnhancedBubbleSystem system)
        {
            BubbleId = bubbleId;
            parentSystem = system;
            creationTime = Time.time;
            
            // Add visual components (would be enhanced with actual bubble rendering)
            var renderer = gameObject.AddComponent<MeshRenderer>();
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            var collider = gameObject.AddComponent<SphereCollider>();
            
            // Create simple sphere mesh for visualization
            meshFilter.mesh = CreateSphereMesh();
            
            // Add basic material (would be replaced with glass shader)
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(1, 0.5f, 1, 0.3f); // Pink translucent
            renderer.material = material;
        }
        
        public void SetupAudio(SteamAudioRenderer audioRenderer)
        {
            this.audioRenderer = audioRenderer;
        }
        
        Mesh CreateSphereMesh()
        {
            // Simple sphere mesh creation (would use proper bubble mesh)
            var mesh = new Mesh();
            // Simplified sphere creation
            return mesh;
        }
    }
    
    /// <summary>
    /// Bubble state tracking
    /// </summary>
    [System.Serializable]
    public struct BubbleState
    {
        public float3 position;
        public bool isActive;
        public float creationTime;
    }
    
    /// <summary>
    /// Comprehensive system performance metrics
    /// </summary>
    [System.Serializable]
    public struct SystemPerformanceMetrics
    {
        public int activeBubbleCount;
        public float lastSystemUpdateTimeMs;
        public AIPerformanceMetrics aiModelMetrics;
        public WaveSystemMetrics waveSystemMetrics;
        public SteamAudioMetrics audioMetrics;
        public VoiceProcessorMetrics voiceMetrics;
        
        public override string ToString()
        {
            return $"System Metrics - Bubbles: {activeBubbleCount}, Update: {lastSystemUpdateTimeMs:F2}ms\n" +
                   $"AI: {aiModelMetrics}\n" +
                   $"Wave: {waveSystemMetrics}\n" +
                   $"Audio: {audioMetrics}\n" +
                   $"Voice: {voiceMetrics}";
        }
    }
}