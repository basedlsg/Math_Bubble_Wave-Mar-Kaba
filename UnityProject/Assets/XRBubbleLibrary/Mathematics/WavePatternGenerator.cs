using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

namespace XRBubbleLibrary.Mathematics
{
    /// <summary>
    /// Static class for generating wave patterns and interference
    /// Based on Unity mathematics and wave function samples
    /// </summary>
    public static class WavePatternGenerator
    {
        /// <summary>
        /// Calculate wave interference from multiple sources
        /// </summary>
        public static float CalculateWaveInterference(float3 position, List<WaveSource> sources)
        {
            float interference = 0.0f;
            
            foreach (var source in sources)
            {
                float distance = math.distance(position, source.position);
                float phase = distance * source.frequency;
                interference += math.sin(phase) * source.amplitude;
            }
            
            return interference;
        }

        public static Vector3[] GenerateWavePattern(int count, float radius, WaveParameters parameters)
        {
            var positions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float angle = i * (2 * Mathf.PI / count);
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                float y = 0;

                // Apply primary wave
                y += Mathf.Sin(x * parameters.primaryFrequency + Time.time) * parameters.primaryAmplitude;

                // Apply secondary wave
                y += Mathf.Cos(z * parameters.secondaryFrequency + Time.time) * parameters.secondaryAmplitude;

                positions[i] = new Vector3(x, y, z);
            }
            return positions;
        }
    }
}
