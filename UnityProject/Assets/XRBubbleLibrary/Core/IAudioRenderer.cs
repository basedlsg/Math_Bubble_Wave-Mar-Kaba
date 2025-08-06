using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Audio renderer interface following Unity dependency injection best practices
    /// Breaks circular dependency between Mathematics and Audio assemblies
    /// 
    /// Technical Documentation:
    /// - Enables Mathematics assembly to depend on Core instead of Scripts.Audio
    /// - Follows SOLID principles (Dependency Inversion Principle)
    /// - Allows multiple audio renderer implementations
    /// 
    /// Implementation Guide for Future Developers:
    /// 1. Any audio renderer must implement this interface in the Audio assembly
    /// 2. Mathematics assembly components use this interface, not concrete implementations
    /// 3. Assign audio renderer components via inspector to avoid hard dependencies
    /// 4. This pattern prevents "CS0246: type not found" circular dependency errors
    /// 
    /// Assembly Dependencies (following Unity best practices):
    /// Core -> Unity packages only (no custom assemblies)
    /// Mathematics -> Core + Unity packages (no Audio assembly reference)
    /// Audio -> Core + Mathematics + Unity packages (can reference Mathematics)
    /// 
    /// Error Resolution:
    /// Fixed CS0246 error: "The type or namespace name 'SteamAudioRenderer' could not be found"
    /// by implementing interface abstraction pattern instead of direct assembly references
    /// </summary>
    public interface IAudioRenderer
    {
        /// <summary>
        /// Get current audio frequency data for cymatics visualization
        /// </summary>
        /// <param name="frequencyBands">Number of frequency bands to return</param>
        /// <returns>Array of frequency amplitudes (0.0 to 1.0)</returns>
        float[] GetFrequencyData(int frequencyBands);
        
        /// <summary>
        /// Get current audio amplitude (volume level)
        /// </summary>
        /// <returns>Amplitude value (0.0 to 1.0)</returns>
        float GetAmplitude();
        
        /// <summary>
        /// Check if audio is currently playing/active
        /// </summary>
        bool IsPlaying { get; }
        
        /// <summary>
        /// Get dominant frequency in Hz
        /// </summary>
        float GetDominantFrequency();
    }
}