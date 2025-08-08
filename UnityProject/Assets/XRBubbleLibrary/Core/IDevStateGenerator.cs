using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Interface for development state generation and reporting.
    /// Provides dependency injection support for testing and modularity.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public interface IDevStateGenerator
    {
        /// <summary>
        /// Generate a comprehensive development state report.
        /// </summary>
        /// <returns>Complete development state report with all modules and evidence</returns>
        DevStateReport GenerateReport();
        
        /// <summary>
        /// Generate a development state report asynchronously.
        /// </summary>
        /// <returns>Task containing the complete development state report</returns>
        Task<DevStateReport> GenerateReportAsync();
        
        /// <summary>
        /// Schedule automatic nightly generation of development state reports.
        /// </summary>
        void ScheduleNightlyGeneration();
        
        /// <summary>
        /// Validate the accuracy of the generated report against actual system state.
        /// </summary>
        /// <param name="report">Report to validate</param>
        /// <returns>True if report is accurate, false otherwise</returns>
        bool ValidateReportAccuracy(DevStateReport report);
        
        /// <summary>
        /// Get the current build version information.
        /// </summary>
        /// <returns>Build version string</returns>
        string GetBuildVersion();
        
        /// <summary>
        /// Get all available assembly definitions in the project.
        /// </summary>
        /// <returns>List of assembly definition file paths</returns>
        List<string> GetAssemblyDefinitions();
        
        /// <summary>
        /// Generate and save the DEV_STATE.md file to the project root.
        /// </summary>
        /// <returns>Path to the generated DEV_STATE.md file</returns>
        string GenerateAndSaveDevStateFile();
        
        /// <summary>
        /// Generate and save the DEV_STATE.md file asynchronously.
        /// </summary>
        /// <returns>Task containing the path to the generated DEV_STATE.md file</returns>
        Task<string> GenerateAndSaveDevStateFileAsync();
    }
}