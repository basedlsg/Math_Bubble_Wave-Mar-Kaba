using Unity.Mathematics;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Bias field interface for AI-driven wave position optimization
    /// Part of dependency inversion pattern to prevent circular dependencies
    /// 
    /// Architecture Notes:
    /// - Core assembly defines interface contracts
    /// - AI assembly provides concrete implementations  
    /// - Mathematics assembly uses interface without AI assembly dependency
    /// - Follows Architecture Review Board requirements for systematic dependency management
    /// </summary>
    public interface IBiasField
    {
        /// <summary>
        /// Get bias vector for specific bubble index
        /// </summary>
        /// <param name="bubbleIndex">Index of bubble to get bias for</param>
        /// <returns>3D bias vector for position adjustment</returns>
        float3 GetBias(int bubbleIndex);
        
        /// <summary>
        /// Get bias vector for specific world position
        /// </summary>
        /// <param name="position">World position to get bias for</param>
        /// <returns>3D bias vector for position adjustment</returns>
        float3 GetBiasAtPosition(float3 position);
        
        /// <summary>
        /// Number of bias entries in this field
        /// </summary>
        int Length { get; }
        
        /// <summary>
        /// Overall strength/influence of this bias field
        /// </summary>
        float Strength { get; }
        
        /// <summary>
        /// Check if bias field is valid and ready for use
        /// </summary>
        bool IsValid { get; }
    }
    
    /// <summary>
    /// Factory interface for creating bias fields
    /// Enables AI assembly to provide bias field generation without Mathematics depending on AI
    /// </summary>
    public interface IBiasFieldProvider
    {
        /// <summary>
        /// Generate bias field for given user position and bubble count
        /// </summary>
        /// <param name="userPosition">Current user position for context</param>
        /// <param name="bubbleCount">Number of bubbles to optimize</param>
        /// <returns>Generated bias field for wave optimization</returns>
        Task<IBiasField> GenerateBiasField(float3 userPosition, int bubbleCount);
        
        /// <summary>
        /// Check if bias field generation is available
        /// </summary>
        bool CanGenerateBiasField { get; }
        
        /// <summary>
        /// Get performance metrics for bias field generation
        /// </summary>
        float LastGenerationTime { get; }
    }
}