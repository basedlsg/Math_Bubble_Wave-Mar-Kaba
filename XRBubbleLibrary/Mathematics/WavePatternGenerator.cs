using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

namespace XRBubbleLibrary.Mathematics
{
    /// <summary>
    /// Mathematical wave pattern generator for natural bubble arrangements
    /// Based on fundamental wave equations and golden ratio proportions
    /// </summary>
    public static class WavePatternGenerator
    {
        // Mathematical constants
        public const float GoldenRatio = 1.618033988749f;
        public const float InverseGoldenRatio = 0.618033988749f;
        public const float TwoPi = 2.0f * Mathf.PI;
        
        /// <summary>
        /// Generate positions using Fibonacci spiral for natural arrangement
        /// Based on mathematical patterns found in nature
        /// </summary>
        public static Vector3[] GenerateFibonacciSpiral(int count, float radius, float height = 0.0f)
        {
            Vector3[] positions = new Vector3[count];
            
            for (int i = 0; i < count; i++)
            {
                // Fibonacci spiral angle
                float angle = i * TwoPi * InverseGoldenRatio;
                
                // Spiral radius based on square root for even distribution
                float spiralRadius = radius * Mathf.Sqrt(i / (float)count);
                
                // Calculate position
                float x = spiralRadius * Mathf.Cos(angle);
                float z = spiralRadius * Mathf.Sin(angle);
                float y = height + (i / (float)count) * 0.2f; // Slight height variation
                
                positions[i] = new Vector3(x, y, z);
            }
            
            return positions;
        }
        
        /// <summary>
        /// Generate wave-based positions using sine and cosine functions
        /// Creates comfortable, natural oscillation patterns
        /// </summary>
        public static Vector3[] GenerateWavePattern(int count, float radius, WaveParameters parameters)
        {
            Vector3[] positions = new Vector3[count];
            
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1); // Normalized position (0 to 1)
                
                // Primary wave (horizontal)
                float primaryAngle = t * TwoPi * parameters.primaryFrequency + parameters.primaryPhase;
                float primaryWave = Mathf.Sin(primaryAngle) * parameters.primaryAmplitude;
                
                // Secondary wave (vertical)
                float secondaryAngle = t * TwoPi * parameters.secondaryFrequency + parameters.secondaryPhase;
                float secondaryWave = Mathf.Cos(secondaryAngle) * parameters.secondaryAmplitude;
                
                // Tertiary wave (depth) for 3D variation
                float tertiaryAngle = t * TwoPi * parameters.tertiaryFrequency + parameters.tertiaryPhase;
                float tertiaryWave = Mathf.Sin(tertiaryAngle * 0.7f) * parameters.tertiaryAmplitude;
                
                // Calculate base position on circle
                float circleAngle = t * TwoPi;
                float baseX = Mathf.Sin(circleAngle) * radius;
                float baseZ = Mathf.Cos(circleAngle) * radius;
                
                // Apply wave modulations
                float x = baseX + primaryWave;
                float y = parameters.baseHeight + secondaryWave;
                float z = baseZ + tertiaryWave;
                
                positions[i] = new Vector3(x, y, z);
            }
            
            return positions;
        }
        
        /// <summary>
        /// Generate positions using harmonic series for musical spatial arrangement
        /// Creates mathematically pleasing relationships between bubble positions
        /// </summary>
        public static Vector3[] GenerateHarmonicPattern(int count, float radius, float fundamentalFreq = 1.0f)
        {
            Vector3[] positions = new Vector3[count];
            
            // Harmonic ratios (musical intervals)
            float[] harmonicRatios = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 8.0f, 9.0f };
            
            for (int i = 0; i < count; i++)
            {
                // Select harmonic ratio
                int harmonicIndex = i % harmonicRatios.Length;
                float harmonicRatio = harmonicRatios[harmonicIndex];
                
                // Calculate frequency and phase
                float frequency = fundamentalFreq * harmonicRatio;
                float phase = (i / (float)count) * TwoPi;
                
                // Generate position based on harmonic
                float angle = phase * frequency;
                float harmonicRadius = radius * (1.0f + 0.2f * Mathf.Sin(angle));
                
                float x = Mathf.Sin(phase) * harmonicRadius;
                float z = Mathf.Cos(phase) * harmonicRadius;
                float y = Mathf.Sin(angle) * 0.1f; // Subtle height variation
                
                positions[i] = new Vector3(x, y, z);
            }
            
            return positions;
        }
        
        /// <summary>
        /// Generate breathing animation offsets using comfortable frequencies
        /// Ensures oscillation stays within human comfort zone (0.2-0.5 Hz)
        /// </summary>
        public static Vector3 CalculateBreathingOffset(float time, float frequency, float amplitude, float phaseOffset = 0.0f)
        {
            // Clamp frequency to comfortable range
            frequency = Mathf.Clamp(frequency, 0.2f, 0.5f);
            
            // Calculate breathing phase
            float phase = time * frequency * TwoPi + phaseOffset;
            
            // Generate smooth breathing motion
            float breathingIntensity = Mathf.Sin(phase);
            
            // Apply to Y axis primarily, with subtle X/Z variation
            float y = breathingIntensity * amplitude;
            float x = Mathf.Sin(phase * 0.7f) * amplitude * 0.3f;
            float z = Mathf.Cos(phase * 0.5f) * amplitude * 0.3f;
            
            return new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Calculate wave interference patterns for multiple bubble interactions
        /// Creates natural-looking wave interactions between nearby bubbles
        /// </summary>
        public static float CalculateWaveInterference(Vector3 position, List<WaveSource> waveSources)
        {
            float totalAmplitude = 0.0f;
            
            foreach (var source in waveSources)
            {
                // Calculate distance to wave source
                float distance = Vector3.Distance(position, source.position);
                
                // Calculate wave phase based on distance and time
                float phase = (distance / source.wavelength) * TwoPi - (Time.time * source.frequency * TwoPi);
                
                // Calculate amplitude with distance falloff
                float amplitude = source.amplitude / (1.0f + distance * source.falloff);
                
                // Add wave contribution
                totalAmplitude += amplitude * Mathf.Sin(phase);
            }
            
            return totalAmplitude;
        }
        
        /// <summary>
        /// Generate Perlin noise-based organic variation
        /// Adds natural randomness while maintaining smooth transitions
        /// </summary>
        public static Vector3 CalculateOrganicVariation(Vector3 basePosition, float time, float scale = 1.0f, float intensity = 0.1f)
        {
            // Use position and time as noise coordinates
            float noiseX = Mathf.PerlinNoise(basePosition.x * scale + time * 0.1f, basePosition.z * scale);
            float noiseY = Mathf.PerlinNoise(basePosition.z * scale, basePosition.x * scale + time * 0.15f);
            float noiseZ = Mathf.PerlinNoise(basePosition.x * scale + time * 0.12f, basePosition.y * scale);
            
            // Convert from 0-1 to -1 to 1 range
            noiseX = (noiseX - 0.5f) * 2.0f;
            noiseY = (noiseY - 0.5f) * 2.0f;
            noiseZ = (noiseZ - 0.5f) * 2.0f;
            
            return new Vector3(noiseX, noiseY, noiseZ) * intensity;
        }
        
        /// <summary>
        /// Calculate optimal spacing between bubbles using golden ratio
        /// Ensures visually pleasing proportions
        /// </summary>
        public static float CalculateOptimalSpacing(float baseRadius, int ringIndex)
        {
            // Use golden ratio for natural spacing progression
            return baseRadius * Mathf.Pow(GoldenRatio, ringIndex * 0.5f);
        }
        
        /// <summary>
        /// Generate color gradient positions based on wave mathematics
        /// Creates smooth color transitions across bubble arrangements
        /// </summary>
        public static float CalculateColorGradientPosition(Vector3 position, Vector3 center, float waveLength)
        {
            // Calculate distance from center
            float distance = Vector3.Distance(position, center);
            
            // Convert to wave phase for smooth color transitions
            float phase = (distance / waveLength) * TwoPi;
            
            // Return normalized gradient position (0 to 1)
            return (Mathf.Sin(phase) + 1.0f) * 0.5f;
        }
    }
    
    /// <summary>
    /// Parameters for wave pattern generation
    /// </summary>
    [System.Serializable]
    public struct WaveParameters
    {
        [Header("Primary Wave (Horizontal)")]
        public float primaryFrequency;
        public float primaryAmplitude;
        public float primaryPhase;
        
        [Header("Secondary Wave (Vertical)")]
        public float secondaryFrequency;
        public float secondaryAmplitude;
        public float secondaryPhase;
        
        [Header("Tertiary Wave (Depth)")]
        public float tertiaryFrequency;
        public float tertiaryAmplitude;
        public float tertiaryPhase;
        
        [Header("Base Configuration")]
        public float baseHeight;
        
        /// <summary>
        /// Default comfortable wave parameters
        /// </summary>
        public static WaveParameters Default => new WaveParameters
        {
            primaryFrequency = 1.0f,
            primaryAmplitude = 0.2f,
            primaryPhase = 0.0f,
            secondaryFrequency = 0.7f,
            secondaryAmplitude = 0.15f,
            secondaryPhase = Mathf.PI * 0.5f,
            tertiaryFrequency = 1.3f,
            tertiaryAmplitude = 0.1f,
            tertiaryPhase = Mathf.PI,
            baseHeight = 0.0f
        };
    }
    
    /// <summary>
    /// Wave source for interference calculations
    /// </summary>
    [System.Serializable]
    public struct WaveSource
    {
        public Vector3 position;
        public float frequency;
        public float amplitude;
        public float wavelength;
        public float falloff;
        
        public static WaveSource Create(Vector3 pos, float freq = 1.0f, float amp = 1.0f)
        {
            return new WaveSource
            {
                position = pos,
                frequency = freq,
                amplitude = amp,
                wavelength = 1.0f / freq,
                falloff = 0.5f
            };
        }
    }
}