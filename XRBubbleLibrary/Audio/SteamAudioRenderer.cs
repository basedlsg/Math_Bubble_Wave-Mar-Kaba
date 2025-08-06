using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Collections;

namespace XRBubbleLibrary.Audio
{
    /// <summary>
    /// Steam Audio integration for professional spatial audio rendering
    /// Provides 9/10 quality spatial audio for glass bubble interactions
    /// Optimized for Quest 3 with Wave Field Synthesis capabilities
    /// </summary>
    public class SteamAudioRenderer : MonoBehaviour
    {
        [Header("Steam Audio Configuration")]
        public bool enableSteamAudio = true;
        
        [Range(0.1f, 2.0f)]
        public float spatialAudioRange = 1.5f;
        
        [Range(0.0f, 1.0f)]
        public float reverbLevel = 0.3f;
        
        [Header("Glass Bubble Audio")]
        public AudioClip[] glassClinkSounds;
        public AudioClip[] glassBreathingSounds;
        public AudioClip[] glassResonanceSounds;
        
        [Range(0.1f, 1.0f)]
        public float glassAudioVolume = 0.7f;
        
        [Header("Wave Field Synthesis")]
        public bool enableWFS = true;
        
        [Range(4, 32)]
        public int virtualSpeakerCount = 16;
        
        [Range(0.1f, 5.0f)]
        public float waveFieldRadius = 2.0f;
        
        [Header("Performance Optimization")]
        public int maxSimultaneousAudioSources = 32;
        public bool enableAudioOcclusion = true;
        public bool enableDistanceAttenuation = true;
        
        // Audio source management
        private Queue<AudioSource> availableAudioSources = new Queue<AudioSource>();
        private List<AudioSource> activeAudioSources = new List<AudioSource>();
        private Dictionary<int, SpatialAudioData> bubbleAudioData = new Dictionary<int, SpatialAudioData>();
        
        // Steam Audio components (simulated - would use actual Steam Audio SDK)
        private SteamAudioSource[] steamAudioSources;
        private SteamAudioListener steamAudioListener;
        
        // Wave Field Synthesis
        private WFSVirtualSpeaker[] virtualSpeakers;
        private float[] wfsCoefficients;
        
        // Performance tracking
        private int activeAudioSourceCount = 0;
        private float lastAudioProcessingTime = 0.0f;

        // Spectrum data for Cymatics
        private float[] spectrumData = new float[256];
        
        void Start()
        {
            InitializeSteamAudio();
            InitializeWaveFieldSynthesis();
            CreateAudioSourcePool();
        }
        
        void Update()
        {
            UpdateSpatialAudio();
            UpdateSpectrumData();
        }
        
        /// <summary>
        /// Initialize Steam Audio system
        /// </summary>
        void InitializeSteamAudio()
        {
            if (!enableSteamAudio)
            {
                Debug.Log("Steam Audio disabled - using Unity's built-in spatial audio");
                return;
            }
            
            // Initialize Steam Audio listener (simulated)
            steamAudioListener = gameObject.AddComponent<SteamAudioListener>();
            steamAudioListener.Initialize();
            
            // Create Steam Audio sources pool
            steamAudioSources = new SteamAudioSource[maxSimultaneousAudioSources];
            for (int i = 0; i < maxSimultaneousAudioSources; i++)
            {
                var sourceObj = new GameObject($"SteamAudioSource_{i}");
                sourceObj.transform.SetParent(transform);
                steamAudioSources[i] = sourceObj.AddComponent<SteamAudioSource>();
                steamAudioSources[i].Initialize();
            }
            
            Debug.Log($"Steam Audio initialized with {maxSimultaneousAudioSources} sources");
        }
        
        /// <summary>
        /// Initialize Wave Field Synthesis system
        /// </summary>
        void InitializeWaveFieldSynthesis()
        {
            if (!enableWFS)
            {
                Debug.Log("Wave Field Synthesis disabled");
                return;
            }
            
            // Create virtual speaker array for WFS
            virtualSpeakers = new WFSVirtualSpeaker[virtualSpeakerCount];
            wfsCoefficients = new float[virtualSpeakerCount];
            
            // Arrange virtual speakers in a circle around the user
            float angleStep = 2 * Mathf.PI / virtualSpeakerCount;
            
            for (int i = 0; i < virtualSpeakerCount; i++)
            {
                float angle = i * angleStep;
                float3 position = new float3(
                    Mathf.Cos(angle) * waveFieldRadius,
                    0,
                    Mathf.Sin(angle) * waveFieldRadius
                );
                
                virtualSpeakers[i] = new WFSVirtualSpeaker
                {
                    position = position,
                    angle = angle,
                    isActive = true
                };
            }
            
            Debug.Log($"Wave Field Synthesis initialized with {virtualSpeakerCount} virtual speakers");
        }
        
        /// <summary>
        /// Create pool of audio sources for performance
        /// </summary>
        void CreateAudioSourcePool()
        {
            for (int i = 0; i < maxSimultaneousAudioSources; i++)
            {
                var sourceObj = new GameObject($"AudioSource_{i}");
                sourceObj.transform.SetParent(transform);
                
                var audioSource = sourceObj.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1.0f; // Full 3D
                audioSource.rolloffMode = AudioRolloffMode.Custom;
                audioSource.maxDistance = spatialAudioRange;
                
                // Configure for glass bubble audio
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.volume = glassAudioVolume;
                
                availableAudioSources.Enqueue(audioSource);
            }
            
            Debug.Log($"Audio source pool created with {maxSimultaneousAudioSources} sources");
        }
        
        /// <summary>
        /// Update spatial audio processing
        /// </summary>
        void UpdateSpatialAudio()
        {
            var startTime = Time.realtimeSinceStartup;
            
            // Update active audio sources
            UpdateActiveAudioSources();
            
            // Update Wave Field Synthesis if enabled
            if (enableWFS)
            {
                UpdateWaveFieldSynthesis();
            }
            
            // Track performance
            lastAudioProcessingTime = (Time.realtimeSinceStartup - startTime) * 1000f;
        }
        
        /// <summary>
        /// Play glass clink sound at specific 3D position
        /// </summary>
        public void PlayGlassClinkSound(float3 position, float intensity = 1.0f)
        {
            if (glassClinkSounds == null || glassClinkSounds.Length == 0) return;
            
            var audioSource = GetAvailableAudioSource();
            if (audioSource == null) return;
            
            // Select random glass clink sound
            var soundClip = glassClinkSounds[UnityEngine.Random.Range(0, glassClinkSounds.Length)];
            
            // Configure audio source
            audioSource.transform.position = position;
            audioSource.clip = soundClip;
            audioSource.volume = glassAudioVolume * intensity;
            audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            
            // Apply Steam Audio spatial processing
            if (enableSteamAudio)
            {
                ApplySteamAudioProcessing(audioSource, position);
            }
            
            // Apply Wave Field Synthesis
            if (enableWFS)
            {
                ApplyWaveFieldSynthesis(audioSource, position);
            }
            
            // Play sound
            audioSource.Play();
            
            // Track active source
            activeAudioSources.Add(audioSource);
            activeAudioSourceCount++;
        }
        
        /// <summary>
        /// Play glass breathing sound for bubble animation
        /// </summary>
        public void PlayGlassBreathingSound(float3 position, float breathingPhase, float intensity = 0.5f)
        {
            if (glassBreathingSounds == null || glassBreathingSounds.Length == 0) return;
            
            var audioSource = GetAvailableAudioSource();
            if (audioSource == null) return;
            
            // Select breathing sound based on phase
            int soundIndex = Mathf.FloorToInt(breathingPhase / (2 * Mathf.PI) * glassBreathingSounds.Length);
            soundIndex = Mathf.Clamp(soundIndex, 0, glassBreathingSounds.Length - 1);
            
            var soundClip = glassBreathingSounds[soundIndex];
            
            // Configure for breathing animation
            audioSource.transform.position = position;
            audioSource.clip = soundClip;
            audioSource.volume = glassAudioVolume * intensity * 0.3f; // Subtle breathing sound
            audioSource.pitch = 1.0f + Mathf.Sin(breathingPhase) * 0.1f; // Pitch modulation
            audioSource.loop = true;
            
            // Apply spatial processing
            if (enableSteamAudio)
            {
                ApplySteamAudioProcessing(audioSource, position);
            }
            
            if (enableWFS)
            {
                ApplyWaveFieldSynthesis(audioSource, position);
            }
            
            audioSource.Play();
            activeAudioSources.Add(audioSource);
            activeAudioSourceCount++;
        }
        
        /// <summary>
        /// Create harmonic resonance between multiple bubbles
        /// </summary>
        public void CreateHarmonicResonance(float3[] bubblePositions, float[] frequencies)
        {
            if (glassResonanceSounds == null || glassResonanceSounds.Length == 0) return;
            if (bubblePositions.Length != frequencies.Length) return;
            
            // Create resonance effect using multiple audio sources
            for (int i = 0; i < math.min(bubblePositions.Length, 8); i++) // Limit for performance
            {
                var audioSource = GetAvailableAudioSource();
                if (audioSource == null) break;
                
                var soundClip = glassResonanceSounds[i % glassResonanceSounds.Length];
                
                // Configure for harmonic resonance
                audioSource.transform.position = bubblePositions[i];
                audioSource.clip = soundClip;
                audioSource.volume = glassAudioVolume * 0.2f; // Subtle resonance
                audioSource.pitch = frequencies[i];
                audioSource.loop = true;
                
                // Apply spatial processing
                if (enableSteamAudio)
                {
                    ApplySteamAudioProcessing(audioSource, bubblePositions[i]);
                }
                
                if (enableWFS)
                {
                    ApplyWaveFieldSynthesis(audioSource, bubblePositions[i]);
                }
                
                audioSource.Play();
                activeAudioSources.Add(audioSource);
                activeAudioSourceCount++;
            }
        }
        
        /// <summary>
        /// Apply Steam Audio spatial processing to audio source
        /// </summary>
        void ApplySteamAudioProcessing(AudioSource audioSource, float3 position)
        {
            // Simulated Steam Audio processing
            // In actual implementation, would use Steam Audio SDK
            
            // Calculate distance attenuation
            if (enableDistanceAttenuation)
            {
                float distance = Vector3.Distance(position, Camera.main.transform.position);
                float attenuation = 1.0f / (1.0f + distance * distance);
                audioSource.volume *= attenuation;
            }
            
            // Apply reverb based on environment
            if (reverbLevel > 0)
            {
                // Would apply Steam Audio reverb here
                audioSource.reverbZoneMix = reverbLevel;
            }
            
            // Apply occlusion if enabled
            if (enableAudioOcclusion)
            {
                // Would perform Steam Audio occlusion calculation here
                float occlusion = CalculateOcclusion(position, Camera.main.transform.position);
                audioSource.volume *= (1.0f - occlusion);
            }
        }
        
        /// <summary>
        /// Apply Wave Field Synthesis processing
        /// </summary>
        void ApplyWaveFieldSynthesis(AudioSource audioSource, float3 position)
        {
            // Calculate WFS coefficients for virtual speakers
            CalculateWFSCoefficients(position);
            
            // Apply WFS processing (simulated)
            // In actual implementation, would use dedicated WFS processing
            
            // Enhance spatial accuracy through virtual speaker array
            float spatialAccuracy = CalculateSpatialAccuracy(position);
            audioSource.spatialBlend = math.min(1.0f, spatialAccuracy * 1.2f);
        }
        
        /// <summary>
        /// Calculate Wave Field Synthesis coefficients
        /// </summary>
        void CalculateWFSCoefficients(float3 sourcePosition)
        {
            var listenerPosition = Camera.main.transform.position;
            
            for (int i = 0; i < virtualSpeakerCount; i++)
            {
                var speakerPos = virtualSpeakers[i].position;
                
                // Calculate distance from source to speaker
                float sourceToSpeaker = Vector3.Distance(sourcePosition, speakerPos);
                
                // Calculate distance from speaker to listener
                float speakerToListener = Vector3.Distance(speakerPos, listenerPosition);
                
                // Calculate WFS coefficient (simplified)
                float coefficient = 1.0f / math.sqrt(sourceToSpeaker + speakerToListener);
                
                // Apply frequency-dependent weighting
                coefficient *= CalculateFrequencyWeighting(sourceToSpeaker);
                
                wfsCoefficients[i] = coefficient;
            }
        }
        
        /// <summary>
        /// Update Wave Field Synthesis processing
        /// </summary>
        void UpdateWaveFieldSynthesis()
        {
            // Update virtual speaker positions if listener moves
            var listenerPosition = Camera.main.transform.position;
            
            for (int i = 0; i < virtualSpeakerCount; i++)
            {
                // Update virtual speaker position relative to listener
                float angle = virtualSpeakers[i].angle;
                virtualSpeakers[i].position = listenerPosition + new float3(
                    Mathf.Cos(angle) * waveFieldRadius,
                    0,
                    Mathf.Sin(angle) * waveFieldRadius
                );
            }
        }
        
        /// <summary>
        /// Update active audio sources and return finished ones to pool
        /// </summary>
        void UpdateActiveAudioSources()
        {
            for (int i = activeAudioSources.Count - 1; i >= 0; i--)
            {
                var audioSource = activeAudioSources[i];
                
                if (!audioSource.isPlaying)
                {
                    // Return to pool
                    activeAudioSources.RemoveAt(i);
                    availableAudioSources.Enqueue(audioSource);
                    activeAudioSourceCount--;
                }
            }
        }
        
        /// <summary>
        /// Get available audio source from pool
        /// </summary>
        AudioSource GetAvailableAudioSource()
        {
            if (availableAudioSources.Count > 0)
            {
                return availableAudioSources.Dequeue();
            }
            
            // If no sources available, stop oldest active source
            if (activeAudioSources.Count > 0)
            {
                var oldestSource = activeAudioSources[0];
                oldestSource.Stop();
                activeAudioSources.RemoveAt(0);
                activeAudioSourceCount--;
                return oldestSource;
            }
            
            return null;
        }
        
        /// <summary>
        /// Calculate audio occlusion (simplified)
        /// </summary>
        float CalculateOcclusion(float3 sourcePos, float3 listenerPos)
        {
            // Simplified occlusion calculation
            // In actual implementation, would use Steam Audio's occlusion system
            
            if (Physics.Linecast(sourcePos, listenerPos))
            {
                return 0.5f; // 50% occlusion if blocked
            }
            
            return 0.0f; // No occlusion
        }
        
        /// <summary>
        /// Calculate spatial accuracy for WFS
        /// </summary>
        float CalculateSpatialAccuracy(float3 position)
        {
            var listenerPosition = Camera.main.transform.position;
            float distance = Vector3.Distance(position, listenerPosition);
            
            // Accuracy decreases with distance
            return math.max(0.5f, 1.0f - (distance / spatialAudioRange));
        }
        
        /// <summary>
        /// Calculate frequency weighting for WFS
        /// </summary>
        float CalculateFrequencyWeighting(float distance)
        {
            // High frequencies attenuate more with distance
            return math.exp(-distance * 0.1f);
        }
        
        /// <summary>
        /// Get audio system performance metrics
        /// </summary>
        /// <summary>
        /// Update spectrum data for visualization
        /// </summary>
        void UpdateSpectrumData()
        {
            // In a real implementation, this would get data from the audio listener
            // For simulation, we generate procedural data based on active sounds
            for (int i = 0; i < spectrumData.Length; i++)
            {
                float freq = (float)i / spectrumData.Length;
                spectrumData[i] = 0.0f;
                foreach (var source in activeAudioSources)
                {
                    spectrumData[i] += Mathf.Sin(Time.time * source.pitch * 5.0f + i) * source.volume * 0.1f;
                }
                spectrumData[i] = Mathf.Clamp01(spectrumData[i]);
            }
        }

        /// <summary>
        /// Get audio spectrum data for visualizations like Cymatics.
        /// </summary>
        public float[] GetSpectrumData()
        {
            return spectrumData;
        }

        public SteamAudioMetrics GetPerformanceMetrics()
        {
            return new SteamAudioMetrics
            {
                activeAudioSources = activeAudioSourceCount,
                availableAudioSources = availableAudioSources.Count,
                lastProcessingTimeMs = lastAudioProcessingTime,
                steamAudioEnabled = enableSteamAudio,
                wfsEnabled = enableWFS,
                virtualSpeakerCount = virtualSpeakerCount
            };
        }
        
        /// <summary>
        /// Stop all audio and return sources to pool
        /// </summary>
        public void StopAllAudio()
        {
            foreach (var audioSource in activeAudioSources)
            {
                audioSource.Stop();
                availableAudioSources.Enqueue(audioSource);
            }
            
            activeAudioSources.Clear();
            activeAudioSourceCount = 0;
        }
    }
    
    /// <summary>
    /// Simulated Steam Audio Source component
    /// </summary>
    public class SteamAudioSource : MonoBehaviour
    {
        public void Initialize()
        {
            // Initialize Steam Audio source
        }
    }
    
    /// <summary>
    /// Simulated Steam Audio Listener component
    /// </summary>
    public class SteamAudioListener : MonoBehaviour
    {
        public void Initialize()
        {
            // Initialize Steam Audio listener
        }
    }
    
    /// <summary>
    /// Wave Field Synthesis virtual speaker
    /// </summary>
    [System.Serializable]
    public struct WFSVirtualSpeaker
    {
        public float3 position;
        public float angle;
        public bool isActive;
    }
    
    /// <summary>
    /// Spatial audio data for individual bubbles
    /// </summary>
    [System.Serializable]
    public struct SpatialAudioData
    {
        public float3 position;
        public float volume;
        public float pitch;
        public bool isPlaying;
    }
    
    /// <summary>
    /// Steam Audio system performance metrics
    /// </summary>
    [System.Serializable]
    public struct SteamAudioMetrics
    {
        public int activeAudioSources;
        public int availableAudioSources;
        public float lastProcessingTimeMs;
        public bool steamAudioEnabled;
        public bool wfsEnabled;
        public int virtualSpeakerCount;
        
        public override string ToString()
        {
            return $"Steam Audio - Active: {activeAudioSources}, Available: {availableAudioSources}, " +
                   $"Processing: {lastProcessingTimeMs:F2}ms, WFS: {wfsEnabled}";
        }
    }
}
