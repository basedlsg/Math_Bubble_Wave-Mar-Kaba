using UnityEngine;
using Unity.Mathematics;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Wave optimization interface following dependency inversion principle
    /// Breaks circular dependency between Mathematics and AI assemblies
    /// 
    /// Architecture Decision Record (ADR):
    /// - Implements dependency inversion to prevent AI ↔ Mathematics circular references
    /// - Mathematics assembly depends only on Core interfaces, not AI implementation
    /// - AI assembly implements interfaces and can safely reference Mathematics for data structures
    /// - Follows Architecture Review Board requirements for systematic dependency management
    /// 
    /// Implementation Pattern:
    /// 1. Mathematics registers need for optimization through Core interface
    /// 2. AI assembly provides implementation of optimization interface
    /// 3. Core manages registration and availability without creating dependencies
    /// </summary>
    public interface IWaveOptimizer
    {
        /// <summary>
        /// Generate optimized bubble positions using AI bias field
        /// </summary>
        /// <param name="basePositions">Mathematical foundation positions</param>
        /// <param name="userPosition">Current user position for optimization context</param>
        /// <returns>AI-optimized positions maintaining mathematical harmony</returns>
        Task<float3[]> OptimizeWavePositions(float3[] basePositions, float3 userPosition);
        
        /// <summary>
        /// Check if AI optimization is currently available
        /// </summary>
        bool IsOptimizationAvailable { get; }
        
        /// <summary>
        /// Get time of last optimization operation (for performance monitoring)
        /// </summary>
        float LastOptimizationTime { get; }
        
        /// <summary>
        /// Get current optimization strength/bias level
        /// </summary>
        float OptimizationStrength { get; }
    }
}