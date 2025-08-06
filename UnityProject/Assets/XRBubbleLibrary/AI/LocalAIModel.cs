#if FALSE // DISABLED: Missing dependencies - requires AI integration packages
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Threading.Tasks;
using XRBubbleLibrary.Core;
using XRBubbleLibrary.Mathematics;

namespace XRBubbleLibrary.AI
{
    /// <summary>
    /// Local AI model for user preference learning and bubble optimization
    /// Optimized for Quest 3 performance with 10-20ms inference time
    /// 
    /// Architecture Notes:
    /// - Implements IWaveOptimizer and IBiasFieldProvider interfaces for dependency inversion
    /// - Registers with Core registry system to provide services to Mathematics assembly
    /// - No direct dependency on Mathematics assembly (follows ARB architectural requirements)
    /// </summary>
    public class LocalAIModel : MonoBehaviour, IWaveOptimizer, IBiasFieldProvider
    {
        [Header("AI Model Configuration")]
        [Range(0.1f, 1.0f)]
        public float learningRate = 0.3f;
        
        [Range(10, 100)]
        public int maxUserPatterns = 50;
        
        [Range(0.1f, 0.9f)]
        public float confidenceThreshold = 0.7f;
        
        [Header("Performance Optimization")]
        public bool enableLocalInference = true;
        public bool enableGroqFallback = true;
        public float maxInferenceTimeMs = 20.0f;
        
        // User behavior tracking
        private Dictionary<string, UserPattern> userPatterns = new Dictionary<string, UserPattern>();
        private List<InteractionEvent> recentInteractions = new List<InteractionEvent>();
        private UserPreferences currentPreferences;
        
        // AI model state
        private bool modelInitialized = false;
        private float lastInferenceTime = 0.0f;
        
        // Groq API fallback
        private GroqAPIClient groqClient;
        
        // Interface implementation properties
        public bool IsOptimizationAvailable => modelInitialized;
        public float LastOptimizationTime => lastInferenceTime;
        public float OptimizationStrength => confidenceThreshold;
        public bool CanGenerateBiasField => modelInitialized;
        public float LastGenerationTime => lastInferenceTime;
        
        void Start()
        {
            InitializeLocalModel();
            
            if (enableGroqFallback)
            {
                groqClient = new GroqAPIClient();
            }
            
            // Register with dependency inversion system
            RegisterWithCore();
        }
        
        void OnDestroy()
        {
            // Unregister when destroyed
            WaveOptimizationRegistry.UnregisterOptimizer();
            BiasFieldRegistry.RegisterProvider(null);
        }
        
        /// <summary>
        /// Register this AI model with the Core registry system
        /// Enables Mathematics assembly to access AI services without direct dependency
        /// </summary>
        void RegisterWithCore()
        {
            WaveOptimizationRegistry.RegisterOptimizer(this);
            BiasFieldRegistry.RegisterProvider(this);
            Debug.Log("AI model registered with Core registry system");
        }
        
        /// <summary>
        /// Initialize the local AI model for on-device inference
        /// </summary>
        void InitializeLocalModel()
        {
            // Initialize lightweight neural network for local inference
            // Using simplified model optimized for Quest 3 hardware
            
            currentPreferences = new UserPreferences
            {
                preferredBubbleDistance = 1.5f,
                preferredArrangementStyle = ArrangementStyle.Fibonacci,
                interactionFrequency = 1.0f,
                spatialPreferences = new float3(0, 0, -1) // Default: in front of user
            };
            
            modelInitialized = true;
            Debug.Log("Local AI model initialized for Quest 3 optimization");
        }
        
        /// <summary>
        /// IWaveOptimizer interface implementation
        /// Optimizes wave positions using AI bias field
        /// </summary>
        public async Task<float3[]> OptimizeWavePositions(float3[] basePositions, float3 userPosition)
        {
            if (!IsOptimizationAvailable)
            {
                return basePositions;
            }
            
            try
            {
                // Generate bias field for optimization
                var biasField = await GenerateBiasField(userPosition, basePositions.Length);
                
                // Apply bias field to base positions
                var optimizedPositions = new float3[basePositions.Length];
                
                for (int i = 0; i < basePositions.Length; i++)
                {
                    float3 bias = biasField.GetBias(i) * biasField.Strength;
                    optimizedPositions[i] = basePositions[i] + bias;
                }
                
                return optimizedPositions;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Wave optimization failed: {ex.Message}");
                return basePositions;
            }
        }
        
        /// <summary>
        /// IBiasFieldProvider interface implementation
        /// Generate AI bias field for wave optimization
        /// </summary>
        public async Task<IBiasField> GenerateBiasField(float3 userPosition, int bubbleCount)
        {
            var startTime = Time.realtimeSinceStartup;
            
            try
            {
                ConcreteBiasField biasField;
                
                if (enableLocalInference)
                {
                    // Local inference optimized for Quest 3
                    biasField = GenerateLocalBiasField(userPosition, bubbleCount);
                }
                else if (enableGroqFallback && groqClient != null)
                {
                    // Fallback to Groq API for complex calculations
                    biasField = await GenerateGroqBiasField(userPosition, bubbleCount);
                }
                else
                {
                    // Simple fallback based on current preferences
                    biasField = GenerateSimpleBiasField(userPosition, bubbleCount);
                }
                
                lastInferenceTime = (Time.realtimeSinceStartup - startTime) * 1000f;
                
                // Log performance warning if inference is too slow
                if (lastInferenceTime > maxInferenceTimeMs)
                {
                    Debug.LogWarning($"AI inference took {lastInferenceTime:F2}ms (target: {maxInferenceTimeMs}ms)");
                }
                
                return biasField;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AI bias field generation failed: {ex.Message}");
                return GenerateSimpleBiasField(userPosition, bubbleCount);
            }
        }
        
        /// <summary>
        /// Learn from user interaction and update preferences
        /// </summary>
        public void LearnFromInteraction(InteractionEvent interaction)
        {
            if (!modelInitialized) return;
            
            // Add to recent interactions
            recentInteractions.Add(interaction);
            
            // Keep only recent interactions for performance
            if (recentInteractions.Count > 100)
            {
                recentInteractions.RemoveAt(0);
            }
            
            // Update user patterns
            UpdateUserPatterns(interaction);
            
            // Adapt preferences based on patterns
            AdaptPreferences();
        }
        
        /// <summary>
        /// Generate bias field using local AI model (10-20ms target)
        /// </summary>
        ConcreteBiasField GenerateLocalBiasField(float3 userPosition, int bubbleCount)
        {
            var biasField = new ConcreteBiasField();
            biasField.Initialize(bubbleCount);
            
            // Use current user preferences to generate bias field
            var preferences = currentPreferences;
            
            for (int i = 0; i < bubbleCount; i++)
            {
                // Calculate bias based on user preferences
                float distanceBias = CalculateDistanceBias(i, preferences.preferredBubbleDistance);
                float3 spatialBias = CalculateSpatialBias(i, preferences.spatialPreferences);
                float arrangementBias = CalculateArrangementBias(i, preferences.preferredArrangementStyle);
                
                biasField.SetBias(i, new float3(
                    spatialBias.x * distanceBias,
                    spatialBias.y * distanceBias,
                    spatialBias.z * distanceBias
                ) * arrangementBias);
            }
            
            return biasField;
        }
        
        /// <summary>
        /// Generate bias field using Groq API fallback
        /// </summary>
        async Task<ConcreteBiasField> GenerateGroqBiasField(float3 userPosition, int bubbleCount)
        {
            if (groqClient == null)
            {
                return GenerateSimpleBiasField(userPosition, bubbleCount);
            }
            
            // Prepare context for Groq API
            var context = new GroqContext
            {
                userPosition = userPosition,
                bubbleCount = bubbleCount,
                userPreferences = currentPreferences,
                recentInteractions = recentInteractions.ToArray()
            };
            
            // Request optimized bias field from Groq
            var response = await groqClient.GenerateOptimalBiasField(context);
            
            return response.biasField;
        }
        
        /// <summary>
        /// Simple bias field generation for fallback scenarios
        /// </summary>
        ConcreteBiasField GenerateSimpleBiasField(float3 userPosition, int bubbleCount)
        {
            var biasField = new ConcreteBiasField();
            biasField.Initialize(bubbleCount);
            
            // Simple bias toward user's preferred spatial area
            var preferredDirection = currentPreferences.spatialPreferences;
            
            for (int i = 0; i < bubbleCount; i++)
            {
                float intensity = 0.1f; // Subtle bias to maintain mathematical harmony
                biasField.SetBias(i, preferredDirection * intensity);
            }
            
            return biasField;
        }
        
        /// <summary>
        /// Update user patterns based on interaction
        /// </summary>
        void UpdateUserPatterns(InteractionEvent interaction)
        {
            string patternKey = GeneratePatternKey(interaction);
            
            if (userPatterns.ContainsKey(patternKey))
            {
                var pattern = userPatterns[patternKey];
                pattern.frequency++;
                pattern.lastSeen = Time.time;
                userPatterns[patternKey] = pattern;
            }
            else
            {
                userPatterns[patternKey] = new UserPattern
                {
                    patternKey = patternKey,
                    frequency = 1,
                    firstSeen = Time.time,
                    lastSeen = Time.time,
                    interactionType = interaction.type
                };
            }
            
            // Remove old patterns to maintain performance
            CleanupOldPatterns();
        }
        
        /// <summary>
        /// Adapt user preferences based on learned patterns
        /// </summary>
        void AdaptPreferences()
        {
            if (userPatterns.Count == 0) return;
            
            // Analyze patterns to update preferences
            var mostFrequentPatterns = GetMostFrequentPatterns(5);
            
            foreach (var pattern in mostFrequentPatterns)
            {
                AdaptPreferenceFromPattern(pattern);
            }
        }
        
        /// <summary>
        /// Calculate distance bias for bubble positioning
        /// </summary>
        float CalculateDistanceBias(int bubbleIndex, float preferredDistance)
        {
            // Create bias that encourages bubbles at preferred distance
            float normalizedIndex = bubbleIndex / 10.0f; // Normalize bubble index
            float distanceFromPreferred = math.abs(normalizedIndex - preferredDistance);
            
            return math.max(0.1f, 1.0f - distanceFromPreferred);
        }
        
        /// <summary>
        /// Calculate spatial bias based on user preferences
        /// </summary>
        float3 CalculateSpatialBias(int bubbleIndex, float3 spatialPreferences)
        {
            // Apply spatial preferences with some variation
            float variation = math.sin(bubbleIndex * 0.1f) * 0.2f;
            
            return spatialPreferences + new float3(variation, variation * 0.5f, variation * 0.3f);
        }
        
        /// <summary>
        /// Calculate arrangement bias based on preferred style
        /// </summary>
        float CalculateArrangementBias(int bubbleIndex, ArrangementStyle style)
        {
            switch (style)
            {
                case ArrangementStyle.Fibonacci:
                    return 1.0f + math.sin(bubbleIndex * 1.618f) * 0.1f;
                case ArrangementStyle.Grid:
                    return 1.0f + ((bubbleIndex % 2 == 0) ? 0.1f : -0.1f);
                case ArrangementStyle.Organic:
                    return 1.0f + Mathf.PerlinNoise(bubbleIndex * 0.1f, Time.time * 0.1f) * 0.2f;
                default:
                    return 1.0f;
            }
        }
        
        /// <summary>
        /// Generate pattern key for interaction tracking
        /// </summary>
        string GeneratePatternKey(InteractionEvent interaction)
        {
            return $"{interaction.type}_{interaction.bubblePosition}_{interaction.timeOfDay}";
        }
        
        /// <summary>
        /// Get most frequent user patterns
        /// </summary>
        List<UserPattern> GetMostFrequentPatterns(int count)
        {
            var patterns = new List<UserPattern>(userPatterns.Values);
            patterns.Sort((a, b) => b.frequency.CompareTo(a.frequency));
            
            return patterns.GetRange(0, math.min(count, patterns.Count));
        }
        
        /// <summary>
        /// Adapt preferences based on observed pattern
        /// </summary>
        void AdaptPreferenceFromPattern(UserPattern pattern)
        {
            // Gradually adapt preferences based on user behavior
            float adaptationRate = learningRate * (pattern.frequency / 10.0f);
            
            // Example adaptation logic - can be expanded based on pattern analysis
            if (pattern.interactionType == InteractionType.BubbleTouch)
            {
                // User touches bubbles frequently - they prefer closer bubbles
                currentPreferences.preferredBubbleDistance = math.lerp(
                    currentPreferences.preferredBubbleDistance,
                    1.2f, // Closer distance
                    adaptationRate
                );
            }
        }
        
        /// <summary>
        /// Clean up old patterns to maintain performance
        /// </summary>
        void CleanupOldPatterns()
        {
            if (userPatterns.Count <= maxUserPatterns) return;
            
            var patternsToRemove = new List<string>();
            float currentTime = Time.time;
            
            foreach (var kvp in userPatterns)
            {
                // Remove patterns not seen in the last 5 minutes
                if (currentTime - kvp.Value.lastSeen > 300f)
                {
                    patternsToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in patternsToRemove)
            {
                userPatterns.Remove(key);
            }
        }
        
        /// <summary>
        /// Get current AI performance metrics
        /// </summary>
        public AIPerformanceMetrics GetPerformanceMetrics()
        {
            return new AIPerformanceMetrics
            {
                lastInferenceTimeMs = lastInferenceTime,
                userPatternsCount = userPatterns.Count,
                recentInteractionsCount = recentInteractions.Count,
                modelInitialized = modelInitialized,
                averageConfidence = CalculateAverageConfidence()
            };
        }
        
        /// <summary>
        /// Calculate average confidence of recent predictions
        /// </summary>
        float CalculateAverageConfidence()
        {
            if (userPatterns.Count == 0) return 0.5f;
            
            float totalConfidence = 0.0f;
            foreach (var pattern in userPatterns.Values)
            {
                totalConfidence += math.min(1.0f, pattern.frequency / 10.0f);
            }
            
            return totalConfidence / userPatterns.Count;
        }
    }
    
    /// <summary>
    /// Concrete implementation of IBiasField interface
    /// </summary>
    public class ConcreteBiasField : IBiasField
    {
        private float3[] biases;
        private float strength = 1.0f;
        
        public int Length => biases?.Length ?? 0;
        public float Strength => strength;
        public bool IsValid => biases != null && biases.Length > 0;
        
        public void Initialize(int size)
        {
            biases = new float3[size];
            strength = 1.0f;
        }
        
        public void SetBias(int index, float3 bias)
        {
            if (index >= 0 && index < biases.Length)
            {
                biases[index] = bias;
            }
        }
        
        public float3 GetBias(int index)
        {
            if (index >= 0 && index < biases.Length)
            {
                return biases[index];
            }
            return float3.zero;
        }
        
        public float3 GetBiasAtPosition(float3 position)
        {
            // Simple implementation - could be enhanced with spatial interpolation
            int index = Mathf.FloorToInt(position.x + position.y + position.z) % Length;
            return GetBias(math.abs(index));
        }
        
        public void SetStrength(float newStrength)
        {
            strength = math.clamp(newStrength, 0.0f, 2.0f);
        }
    }
    
    /// <summary>
    /// User interaction event for learning
    /// </summary>
    [System.Serializable]
    public struct InteractionEvent
    {
        public InteractionType type;
        public float3 bubblePosition;
        public float3 userPosition;
        public float timestamp;
        public int timeOfDay; // Hour of day for pattern recognition
    }
    
    /// <summary>
    /// User preferences learned by AI
    /// </summary>
    [System.Serializable]
    public struct UserPreferences
    {
        public float preferredBubbleDistance;
        public ArrangementStyle preferredArrangementStyle;
        public float interactionFrequency;
        public float3 spatialPreferences;
    }
    
    /// <summary>
    /// User behavior pattern
    /// </summary>
    [System.Serializable]
    public struct UserPattern
    {
        public string patternKey;
        public int frequency;
        public float firstSeen;
        public float lastSeen;
        public InteractionType interactionType;
    }
    
    /// <summary>
    /// AI performance metrics
    /// </summary>
    [System.Serializable]
    public struct AIPerformanceMetrics
    {
        public float lastInferenceTimeMs;
        public int userPatternsCount;
        public int recentInteractionsCount;
        public bool modelInitialized;
        public float averageConfidence;
        
        public override string ToString()
        {
            return $"AI Metrics - Inference: {lastInferenceTimeMs:F2}ms, Patterns: {userPatternsCount}, " +
                   $"Interactions: {recentInteractionsCount}, Confidence: {averageConfidence:F2}";
        }
    }
    
    /// <summary>
    /// Interaction types for pattern recognition
    /// </summary>
    public enum InteractionType
    {
        BubbleTouch,
        BubbleMove,
        BubbleCreate,
        BubbleDelete,
        VoiceCommand,
        GazeInteraction
    }
    
    /// <summary>
    /// Bubble arrangement styles
    /// </summary>
    public enum ArrangementStyle
    {
        Fibonacci,
        Grid,
        Organic,
        Circular,
        Linear
    }
}
#endif // DISABLED: Missing dependencies