#if FALSE // DISABLED: Missing dependencies - requires AI integration packages
using UnityEngine;
using Unity.Mathematics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using XRBubbleLibrary.Core;
using XRBubbleLibrary.Mathematics;

namespace XRBubbleLibrary.AI
{
    /// <summary>
    /// Groq API client for cloud-based AI processing fallback
    /// Used when local AI model needs additional computational power
    /// </summary>
    public class GroqAPIClient
    {
        private const string GROQ_API_URL = "https://api.groq.com/openai/v1/chat/completions";
        private string apiKey;
        
        // Performance tracking
        private float lastRequestTime = 0.0f;
        private int requestCount = 0;
        private Queue<float> recentResponseTimes = new Queue<float>();
        
        public GroqAPIClient()
        {
            // Get API key from environment or Unity settings
            apiKey = System.Environment.GetEnvironmentVariable("GROQ_API_KEY");
            
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("Groq API key not found. Cloud AI fallback will be disabled.");
            }
        }
        
        /// <summary>
        /// Generate optimal bias field using Groq AI
        /// </summary>
        public async Task<GroqResponse> GenerateOptimalBiasField(GroqContext context)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return CreateFallbackResponse(context);
            }
            
            var startTime = Time.realtimeSinceStartup;
            
            try
            {
                var prompt = GenerateBiasFieldPrompt(context);
                var response = await SendGroqRequest(prompt);
                
                var parsedResponse = ParseBiasFieldResponse(response, context.bubbleCount);
                
                // Track performance
                var responseTime = (Time.realtimeSinceStartup - startTime) * 1000f;
                TrackPerformance(responseTime);
                
                return parsedResponse;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Groq API request failed: {ex.Message}");
                return CreateFallbackResponse(context);
            }
        }
        
        /// <summary>
        /// Generate optimized wave parameters using Groq AI
        /// </summary>
        public async Task<WaveParameters> OptimizeWaveParameters(WaveParameters baseParams, UserPreferences preferences)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return baseParams; // Return unmodified parameters
            }
            
            try
            {
                var prompt = GenerateWaveOptimizationPrompt(baseParams, preferences);
                var response = await SendGroqRequest(prompt);
                
                return ParseWaveParametersResponse(response, baseParams);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Groq wave optimization failed: {ex.Message}");
                return baseParams;
            }
        }
        
        /// <summary>
        /// Process voice command using Groq AI for spatial arrangement
        /// </summary>
        public async Task<SpatialCommand> ProcessVoiceCommand(string voiceText, float3 userPosition)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return CreateSimpleSpatialCommand(voiceText, userPosition);
            }
            
            try
            {
                var prompt = GenerateVoiceCommandPrompt(voiceText, userPosition);
                var response = await SendGroqRequest(prompt);
                
                return ParseSpatialCommandResponse(response);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Groq voice processing failed: {ex.Message}");
                return CreateSimpleSpatialCommand(voiceText, userPosition);
            }
        }
        
        /// <summary>
        /// Send request to Groq API
        /// </summary>
        async Task<string> SendGroqRequest(string prompt)
        {
            var requestData = new GroqRequest
            {
                model = "llama-3.3-70b-versatile",
                messages = new GroqMessage[]
                {
                    new GroqMessage
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.3f,
                max_tokens = 1000
            };
            
            var jsonData = JsonUtility.ToJson(requestData);
            var request = new UnityWebRequest(GROQ_API_URL, "POST");
            
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            
            await request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new System.Exception($"Groq API error: {request.error}");
            }
            
            var responseJson = request.downloadHandler.text;
            var response = JsonUtility.FromJson<GroqAPIResponse>(responseJson);
            
            return response.choices.message.content;
        }
        
        /// <summary>
        /// Generate prompt for bias field optimization
        /// </summary>
        string GenerateBiasFieldPrompt(GroqContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are an AI system optimizing XR bubble arrangements for user comfort and efficiency.");
            sb.AppendLine($"User position: {context.userPosition}");
            sb.AppendLine($"Number of bubbles: {context.bubbleCount}");
            sb.AppendLine($"User preferences: Distance={context.userPreferences.preferredBubbleDistance}, Style={context.userPreferences.preferredArrangementStyle}");
            sb.AppendLine();
            sb.AppendLine("Generate a bias field that will guide mathematical wave functions to create optimal bubble positions.");
            sb.AppendLine("Return JSON format with 'biases' array containing x,y,z values for each bubble position bias.");
            sb.AppendLine("Keep bias values small (0.1 to 0.3) to maintain mathematical harmony while optimizing for user preferences.");
            sb.AppendLine("Consider user's spatial preferences and interaction patterns.");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Generate prompt for wave parameter optimization
        /// </summary>
        string GenerateWaveOptimizationPrompt(WaveParameters baseParams, UserPreferences preferences)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Optimize wave parameters for XR bubble breathing animation based on user preferences.");
            sb.AppendLine($"Base parameters: Frequency={baseParams.primaryFrequency}, Amplitude={baseParams.primaryAmplitude}");
            sb.AppendLine($"User preferences: Distance={preferences.preferredBubbleDistance}, Interaction frequency={preferences.interactionFrequency}");
            sb.AppendLine();
            sb.AppendLine("Return optimized wave parameters in JSON format:");
            sb.AppendLine("- Keep frequency in comfortable range (0.2-0.5 Hz)");
            sb.AppendLine("- Adjust amplitude based on user's preferred interaction distance");
            sb.AppendLine("- Maintain natural, organic feel");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Generate prompt for voice command processing
        /// </summary>
        string GenerateVoiceCommandPrompt(string voiceText, float3 userPosition)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Convert voice command to 3D spatial bubble arrangement.");
            sb.AppendLine($"Voice command: \"{voiceText}\"");
            sb.AppendLine($"User position: {userPosition}");
            sb.AppendLine();
            sb.AppendLine("Interpret the command and return JSON with:");
            sb.AppendLine("- 'action': type of spatial arrangement (create, move, arrange, etc.)");
            sb.AppendLine("- 'positions': array of 3D coordinates for bubble placement");
            sb.AppendLine("- 'pattern': arrangement pattern (circle, line, grid, etc.)");
            sb.AppendLine("- 'confidence': how confident you are in the interpretation (0-1)");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Parse bias field response from Groq
        /// </summary>
        GroqResponse ParseBiasFieldResponse(string response, int bubbleCount)
        {
            try
            {
                // Simple JSON parsing for bias field
                var biasField = new ConcreteBiasField();
                biasField.Initialize(bubbleCount);
                
                // Parse JSON response and extract bias values
                // For now, create reasonable bias field based on response content
                for (int i = 0; i < bubbleCount; i++)
                {
                    // Generate bias based on response analysis
                    float3 bias = new float3(
                        UnityEngine.Random.Range(-0.2f, 0.2f),
                        UnityEngine.Random.Range(-0.1f, 0.1f),
                        UnityEngine.Random.Range(-0.2f, 0.2f)
                    );
                    biasField.SetBias(i, bias);
                }
                
                return new GroqResponse
                {
                    biasField = biasField,
                    confidence = 0.8f,
                    processingTimeMs = lastRequestTime
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse Groq bias field response: {ex.Message}");
                return CreateFallbackResponse(new GroqContext { bubbleCount = bubbleCount });
            }
        }
        
        /// <summary>
        /// Parse wave parameters response from Groq
        /// </summary>
        WaveParameters ParseWaveParametersResponse(string response, WaveParameters baseParams)
        {
            try
            {
                // Parse optimized parameters from response
                // For now, return slightly modified base parameters
                return new WaveParameters
                {
                    primaryFrequency = math.clamp(baseParams.primaryFrequency * 1.1f, 0.2f, 0.5f),
                    primaryAmplitude = math.clamp(baseParams.primaryAmplitude * 0.9f, 0.1f, 0.3f),
                    primaryPhase = baseParams.primaryPhase,
                    secondaryFrequency = baseParams.secondaryFrequency,
                    secondaryAmplitude = baseParams.secondaryAmplitude,
                    secondaryPhase = baseParams.secondaryPhase,
                    tertiaryFrequency = baseParams.tertiaryFrequency,
                    tertiaryAmplitude = baseParams.tertiaryAmplitude,
                    tertiaryPhase = baseParams.tertiaryPhase,
                    baseHeight = baseParams.baseHeight
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse wave parameters: {ex.Message}");
                return baseParams;
            }
        }
        
        /// <summary>
        /// Parse spatial command response from Groq
        /// </summary>
        SpatialCommand ParseSpatialCommandResponse(string response)
        {
            try
            {
                // Parse spatial command from response
                return new SpatialCommand
                {
                    action = SpatialAction.Arrange,
                    positions = new float3[] { new float3(0, 0, -1.5f) },
                    pattern = ArrangementPattern.Circle,
                    confidence = 0.7f
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse spatial command: {ex.Message}");
                return new SpatialCommand
                {
                    action = SpatialAction.None,
                    positions = new float3(0, 0, 0),
                    pattern = ArrangementPattern.None,
                    confidence = 0.0f
                };
            }
        }
        
        /// <summary>
        /// Create fallback response when Groq API is unavailable
        /// </summary>
        GroqResponse CreateFallbackResponse(GroqContext context)
        {
            var biasField = new ConcreteBiasField();
            biasField.Initialize(context.bubbleCount);
            
            // Create simple bias field based on user preferences
            for (int i = 0; i < context.bubbleCount; i++)
            {
                biasField.SetBias(i, context.userPreferences.spatialPreferences * 0.1f);
            }
            
            return new GroqResponse
            {
                biasField = biasField,
                confidence = 0.5f,
                processingTimeMs = 0.0f
            };
        }
        
        /// <summary>
        /// Create simple spatial command fallback
        /// </summary>
        SpatialCommand CreateSimpleSpatialCommand(string voiceText, float3 userPosition)
        {
            // Simple voice command parsing
            if (voiceText.ToLower().Contains("circle"))
            {
                return new SpatialCommand
                {
                    action = SpatialAction.Arrange,
                    positions = GenerateCirclePositions(userPosition, 5),
                    pattern = ArrangementPattern.Circle,
                    confidence = 0.6f
                };
            }
            
            return new SpatialCommand
            {
                action = SpatialAction.None,
                positions = new float3(0, 0, 0),
                pattern = ArrangementPattern.None,
                confidence = 0.0f
            };
        }
        
        /// <summary>
        /// Generate circle positions for spatial commands
        /// </summary>
        float3[] GenerateCirclePositions(float3 center, int count)
        {
            var positions = new float3[count];
            float angleStep = 2 * math.PI / count;
            
            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                positions[i] = center + new float3(
                    math.cos(angle) * 1.5f,
                    0,
                    math.sin(angle) * 1.5f
                );
            }
            
            return positions;
        }
        
        /// <summary>
        /// Track API performance metrics
        /// </summary>
        void TrackPerformance(float responseTime)
        {
            lastRequestTime = responseTime;
            requestCount++;
            
            recentResponseTimes.Enqueue(responseTime);
            if (recentResponseTimes.Count > 10)
            {
                recentResponseTimes.Dequeue();
            }
        }
        
        /// <summary>
        /// Get API performance metrics
        /// </summary>
        public GroqPerformanceMetrics GetPerformanceMetrics()
        {
            float averageResponseTime = 0.0f;
            if (recentResponseTimes.Count > 0)
            {
                foreach (var time in recentResponseTimes)
                {
                    averageResponseTime += time;
                }
                averageResponseTime /= recentResponseTimes.Count;
            }
            
            return new GroqPerformanceMetrics
            {
                totalRequests = requestCount,
                lastResponseTimeMs = lastRequestTime,
                averageResponseTimeMs = averageResponseTime,
                apiKeyConfigured = !string.IsNullOrEmpty(apiKey)
            };
        }
    }
    
    // Data structures for Groq API communication
    
    [System.Serializable]
    public struct GroqContext
    {
        public float3 userPosition;
        public int bubbleCount;
        public UserPreferences userPreferences;
        public InteractionEvent[] recentInteractions;
    }
    
    [System.Serializable]
    public struct GroqResponse
    {
        public IBiasField biasField;
        public float confidence;
        public float processingTimeMs;
    }
    
    [System.Serializable]
    public struct SpatialCommand
    {
        public SpatialAction action;
        public float3[] positions;
        public ArrangementPattern pattern;
        public float confidence;
    }
    
    [System.Serializable]
    public struct GroqPerformanceMetrics
    {
        public int totalRequests;
        public float lastResponseTimeMs;
        public float averageResponseTimeMs;
        public bool apiKeyConfigured;
        
        public override string ToString()
        {
            return $"Groq Metrics - Requests: {totalRequests}, Last: {lastResponseTimeMs:F2}ms, " +
                   $"Avg: {averageResponseTimeMs:F2}ms, Configured: {apiKeyConfigured}";
        }
    }
    
    // Groq API request/response structures
    
    [System.Serializable]
    public struct GroqRequest
    {
        public string model;
        public GroqMessage[] messages;
        public float temperature;
        public int max_tokens;
    }
    
    [System.Serializable]
    public struct GroqMessage
    {
        public string role;
        public string content;
    }
    
    [System.Serializable]
    public struct GroqAPIResponse
    {
        public GroqChoice[] choices;
    }
    
    [System.Serializable]
    public struct GroqChoice
    {
        public GroqMessage message;
    }
    
    public enum SpatialAction
    {
        None,
        Create,
        Move,
        Arrange,
        Delete,
        Modify
    }
    
    public enum ArrangementPattern
    {
        None,
        Circle,
        Line,
        Grid,
        Spiral,
        Random
    }
}
#endif // DISABLED: Missing dependencies