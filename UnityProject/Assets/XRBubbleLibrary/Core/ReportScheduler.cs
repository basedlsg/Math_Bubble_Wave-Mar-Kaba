using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace XRBubbleLibrary.Core
{
    /// <summary>
    /// Automated scheduling service for development state report generation.
    /// Provides nightly generation, CI/CD integration, and automated report management.
    /// Part of the "do-it-right" recovery Phase 0 implementation.
    /// </summary>
    public class ReportScheduler : IDisposable
    {
        private readonly IDevStateGenerator _devStateGenerator;
        private readonly IReportFormatter _reportFormatter;
        private readonly string _outputDirectory;
        private readonly Timer _scheduledTimer;
        private readonly List<ScheduledTask> _scheduledTasks;
        private bool _disposed = false;
        
        /// <summary>
        /// Event fired when a scheduled report is generated.
        /// </summary>
        public event Action<ReportGenerationResult> ReportGenerated;
        
        /// <summary>
        /// Event fired when 