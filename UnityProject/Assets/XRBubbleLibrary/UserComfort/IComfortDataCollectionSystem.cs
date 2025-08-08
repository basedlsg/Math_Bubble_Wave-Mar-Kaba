using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Interface for comfort data collection system.
    /// Implements Requirement 7.3: Standardized motion sickness assessment tools.
    /// Implements Requirement 7.4: Data collection automation and validation.
    /// </summary>
    public interface IComfortDataCollectionSystem
    {
        /// <summary>
        /// Whether the comfort data collection system is initialized.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Current data collection configuration.
        /// </summary>
        ComfortDataCollectionConfiguration Configuration { get; }
        
        /// <summary>
        /// Current active data collection sessions.
        /// </summary>
        DataCollectionSession[] ActiveSessions { get; }
        
        /// <summary>
        /// Event fired when data collection is started.
        /// </summary>
        event Action<DataCollectionStartedEventArgs> DataCollectionStarted;
        
        /// <summary>
        /// Event fired when comfort data is collected.
        /// </summary>
        event Action<ComfortDataCollectedEventArgs> ComfortDataCollected;
        
        /// <summary>
        /// Event fired when data collection is completed.
        /// </summary>
        event Action<DataCollectionCompletedEventArgs> DataCollectionCompleted;
        
        /// <summary>
        /// Event fired when data validation fails.
        /// </summary>
        event Action<DataValidationFailedEventArgs> DataValidationFailed;
        
        /// <summary>
        /// Initialize the comfort data collection system.
        /// </summary>
        /// <param name="config">Data collection configuration</param>
        /// <returns>True if initialization successful</returns>
        bool Initialize(ComfortDataCollectionConfiguration config);
        
        /// <summary>
        /// Start a new data collection session.
        /// </summary>
        /// <param name="sessionConfig">Session configuration</param>
        /// <returns>Started data collection session</returns>
        DataCollectionSession StartDataCollection(DataCollectionSessionConfiguration sessionConfig);
        
        /// <summary>
        /// Collect SIM (Simulator Sickness Questionnaire) data.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <param name="simData">SIM questionnaire data</param>
        /// <returns>Data collection result</returns>
        ComfortDataCollectionResult CollectSIMData(string sessionId, SIMQuestionnaireData simData);
        
        /// <summary>
        /// Collect SSQ (Simulator Sickness Questionnaire) data.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <param name="ssqData">SSQ questionnaire data</param>
        /// <returns>Data collection result</returns>
        ComfortDataCollectionResult CollectSSQData(string sessionId, SSQQuestionnaireData ssqData);
        
        /// <summary>
        /// Collect physiological comfort data.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <param name="physiologicalData">Physiological measurement data</param>
        /// <returns>Data collection result</returns>
        ComfortDataCollectionResult CollectPhysiologicalData(string sessionId, PhysiologicalComfortData physiologicalData);
        
        /// <summary>
        /// Collect behavioral observation data.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <param name="behavioralData">Behavioral observation data</param>
        /// <returns>Data collection result</returns>
        ComfortDataCollectionResult CollectBehavioralData(string sessionId, BehavioralComfortData behavioralData);
        
        /// <summary>
        /// Validate collected comfort data for quality and completeness.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <returns>Data validation result</returns>
        ComfortDataValidationResult ValidateCollectedData(string sessionId);
        
        /// <summary>
        /// Complete a data collection session and generate report.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <returns>Session completion result</returns>
        DataCollectionCompletionResult CompleteDataCollection(string sessionId);
        
        /// <summary>
        /// Analyze comfort data and generate insights.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <returns>Comfort data analysis result</returns>
        ComfortDataAnalysisResult AnalyzeComfortData(string sessionId);
        
        /// <summary>
        /// Generate comfort data report for a session.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <param name="reportFormat">Report format</param>
        /// <returns>Generated comfort data report</returns>
        ComfortDataReport GenerateComfortReport(string sessionId, ReportFormat reportFormat);
        
        /// <summary>
        /// Export comfort data in specified format.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <param name="exportFormat">Export format</param>
        /// <returns>Exported comfort data</returns>
        ComfortDataExport ExportComfortData(string sessionId, DataExportFormat exportFormat);
        
        /// <summary>
        /// Get real-time comfort metrics for active session.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <returns>Real-time comfort metrics</returns>
        RealTimeComfortMetrics GetRealTimeMetrics(string sessionId);
        
        /// <summary>
        /// Set up automated data collection schedule.
        /// </summary>
        /// <param name="sessionId">Data collection session identifier</param>
        /// <param name="schedule">Automated collection schedule</param>
        /// <returns>True if schedule setup successful</returns>
        bool SetupAutomatedCollection(string sessionId, AutomatedCollectionSchedule schedule);
        
        /// <summary>
        /// Configure data quality thresholds and validation rules.
        /// </summary>
        /// <param name="qualityConfig">Data quality configuration</param>
        void ConfigureDataQuality(DataQualityConfiguration qualityConfig);
        
        /// <summary>
        /// Get data collection statistics and metrics.
        /// </summary>
        /// <returns>Data collection statistics</returns>
        DataCollectionStatistics GetCollectionStatistics();
        
        /// <summary>
        /// Reset data collection system state.
        /// </summary>
        void ResetDataCollectionSystem();
    }
    
    /// <summary>
    /// Configuration for comfort data collection system.
    /// </summary>
    [Serializable]
    public struct ComfortDataCollectionConfiguration
    {
        /// <summary>
        /// Default data collection interval in seconds.
        /// </summary>
        public float DefaultCollectionIntervalSeconds;
        
        /// <summary>
        /// Maximum number of concurrent data collection sessions.
        /// </summary>
        public int MaxConcurrentSessions;
        
        /// <summary>
        /// Data storage directory.
        /// </summary>
        public string DataStorageDirectory;
        
        /// <summary>
        /// Whether to enable real-time data validation.
        /// </summary>
        public bool EnableRealTimeValidation;
        
        /// <summary>
        /// Whether to enable automated data backup.
        /// </summary>
        public bool EnableAutomatedBackup;
        
        /// <summary>
        /// Data retention period in days.
        /// </summary>
        public int DataRetentionDays;
        
        /// <summary>
        /// Whether to enable debug logging.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Supported data collection methods.
        /// </summary>
        public DataCollectionMethod[] SupportedMethods;
        
        /// <summary>
        /// Default comfort data collection configuration.
        /// </summary>
        public static ComfortDataCollectionConfiguration Default => new ComfortDataCollectionConfiguration
        {
            DefaultCollectionIntervalSeconds = 30f,
            MaxConcurrentSessions = 10,
            DataStorageDirectory = "ComfortData",
            EnableRealTimeValidation = true,
            EnableAutomatedBackup = true,
            DataRetentionDays = 365,
            EnableDebugLogging = false,
            SupportedMethods = new[]
            {
                DataCollectionMethod.SIMQuestionnaire,
                DataCollectionMethod.SSQQuestionnaire,
                DataCollectionMethod.PhysiologicalMeasurement,
                DataCollectionMethod.BehavioralObservation
            }
        };
    }
    
    /// <summary>
    /// Data collection session information.
    /// </summary>
    [Serializable]
    public struct DataCollectionSession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Session configuration.
        /// </summary>
        public DataCollectionSessionConfiguration Configuration;
        
        /// <summary>
        /// Session start time.
        /// </summary>
        public DateTime StartTime;
        
        /// <summary>
        /// Session end time (if completed).
        /// </summary>
        public DateTime? EndTime;
        
        /// <summary>
        /// Current session status.
        /// </summary>
        public DataCollectionSessionStatus Status;
        
        /// <summary>
        /// Participant information.
        /// </summary>
        public ParticipantInfo Participant;
        
        /// <summary>
        /// Collected comfort data points.
        /// </summary>
        public List<ComfortDataPoint> CollectedData;
        
        /// <summary>
        /// Session quality metrics.
        /// </summary>
        public SessionQualityMetrics QualityMetrics;
        
        /// <summary>
        /// Session notes and observations.
        /// </summary>
        public string SessionNotes;
        
        /// <summary>
        /// Automated collection schedule (if applicable).
        /// </summary>
        public AutomatedCollectionSchedule? AutomatedSchedule;
    }
    
    /// <summary>
    /// Configuration for data collection session.
    /// </summary>
    [Serializable]
    public struct DataCollectionSessionConfiguration
    {
        /// <summary>
        /// Session name or identifier.
        /// </summary>
        public string SessionName;
        
        /// <summary>
        /// Participant information.
        /// </summary>
        public ParticipantInfo Participant;
        
        /// <summary>
        /// Data collection methods to use.
        /// </summary>
        public DataCollectionMethod[] CollectionMethods;
        
        /// <summary>
        /// Data collection interval in seconds.
        /// </summary>
        public float CollectionIntervalSeconds;
        
        /// <summary>
        /// Expected session duration in minutes.
        /// </summary>
        public float ExpectedDurationMinutes;
        
        /// <summary>
        /// Whether to enable automated data collection.
        /// </summary>
        public bool EnableAutomatedCollection;
        
        /// <summary>
        /// Data quality requirements.
        /// </summary>
        public DataQualityRequirements QualityRequirements;
        
        /// <summary>
        /// Session-specific notes.
        /// </summary>
        public string SessionNotes;
    }
    
    /// <summary>
    /// Physiological comfort data.
    /// </summary>
    [Serializable]
    public struct PhysiologicalComfortData
    {
        /// <summary>
        /// Data collection timestamp.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Heart rate in beats per minute.
        /// </summary>
        public float HeartRateBPM;
        
        /// <summary>
        /// Heart rate variability.
        /// </summary>
        public float HeartRateVariability;
        
        /// <summary>
        /// Skin conductance level.
        /// </summary>
        public float SkinConductanceLevel;
        
        /// <summary>
        /// Skin temperature in Celsius.
        /// </summary>
        public float SkinTemperatureCelsius;
        
        /// <summary>
        /// Respiratory rate per minute.
        /// </summary>
        public float RespiratoryRate;
        
        /// <summary>
        /// Blood pressure systolic.
        /// </summary>
        public float BloodPressureSystolic;
        
        /// <summary>
        /// Blood pressure diastolic.
        /// </summary>
        public float BloodPressureDiastolic;
        
        /// <summary>
        /// Pupil diameter in millimeters.
        /// </summary>
        public float PupilDiameter;
        
        /// <summary>
        /// Eye blink rate per minute.
        /// </summary>
        public float BlinkRate;
        
        /// <summary>
        /// Head movement velocity.
        /// </summary>
        public float HeadMovementVelocity;
        
        /// <summary>
        /// Postural sway measurement.
        /// </summary>
        public float PosturalSway;
        
        /// <summary>
        /// Additional physiological measurements.
        /// </summary>
        public Dictionary<string, float> AdditionalMeasurements;
        
        /// <summary>
        /// Data quality indicators.
        /// </summary>
        public PhysiologicalDataQuality DataQuality;
    }
    
    /// <summary>
    /// Behavioral comfort data.
    /// </summary>
    [Serializable]
    public struct BehavioralComfortData
    {
        /// <summary>
        /// Data collection timestamp.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Observer identifier.
        /// </summary>
        public string ObserverId;
        
        /// <summary>
        /// Observed comfort level.
        /// </summary>
        public ComfortLevel ObservedComfortLevel;
        
        /// <summary>
        /// Visible discomfort indicators.
        /// </summary>
        public DiscomfortIndicator[] VisibleIndicators;
        
        /// <summary>
        /// Participant verbal reports.
        /// </summary>
        public string[] VerbalReports;
        
        /// <summary>
        /// Behavioral observations.
        /// </summary>
        public BehavioralObservation[] Observations;
        
        /// <summary>
        /// Observer confidence level (0-100).
        /// </summary>
        public float ObserverConfidence;
        
        /// <summary>
        /// Environmental factors.
        /// </summary>
        public EnvironmentalFactor[] EnvironmentalFactors;
        
        /// <summary>
        /// Observer notes.
        /// </summary>
        public string ObserverNotes;
        
        /// <summary>
        /// Data quality assessment.
        /// </summary>
        public BehavioralDataQuality DataQuality;
    }
    
    /// <summary>
    /// Comfort data collection result.
    /// </summary>
    [Serializable]
    public struct ComfortDataCollectionResult
    {
        /// <summary>
        /// Whether data collection was successful.
        /// </summary>
        public bool IsSuccessful;
        
        /// <summary>
        /// Collected data point identifier.
        /// </summary>
        public string DataPointId;
        
        /// <summary>
        /// Data collection timestamp.
        /// </summary>
        public DateTime CollectionTimestamp;
        
        /// <summary>
        /// Data quality score (0-100).
        /// </summary>
        public float DataQualityScore;
        
        /// <summary>
        /// Collection method used.
        /// </summary>
        public DataCollectionMethod CollectionMethod;
        
        /// <summary>
        /// Collection messages and notes.
        /// </summary>
        public string[] CollectionMessages;
        
        /// <summary>
        /// Any collection errors encountered.
        /// </summary>
        public string[] CollectionErrors;
        
        /// <summary>
        /// Data validation results.
        /// </summary>
        public DataValidationResult ValidationResult;
    }
    
    /// <summary>
    /// Comfort data validation result.
    /// </summary>
    [Serializable]
    public struct ComfortDataValidationResult
    {
        /// <summary>
        /// Whether all data passed validation.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// Overall data quality score (0-100).
        /// </summary>
        public float OverallQualityScore;
        
        /// <summary>
        /// Validation timestamp.
        /// </summary>
        public DateTime ValidationTimestamp;
        
        /// <summary>
        /// Number of data points validated.
        /// </summary>
        public int DataPointsValidated;
        
        /// <summary>
        /// Number of validation failures.
        /// </summary>
        public int ValidationFailures;
        
        /// <summary>
        /// Validation issues identified.
        /// </summary>
        public DataValidationIssue[] ValidationIssues;
        
        /// <summary>
        /// Data completeness percentage.
        /// </summary>
        public float CompletenessPercentage;
        
        /// <summary>
        /// Data consistency score.
        /// </summary>
        public float ConsistencyScore;
        
        /// <summary>
        /// Validation recommendations.
        /// </summary>
        public string[] ValidationRecommendations;
    }
    
    /// <summary>
    /// Data collection completion result.
    /// </summary>
    [Serializable]
    public struct DataCollectionCompletionResult
    {
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Whether session completed successfully.
        /// </summary>
        public bool CompletedSuccessfully;
        
        /// <summary>
        /// Session completion timestamp.
        /// </summary>
        public DateTime CompletionTimestamp;
        
        /// <summary>
        /// Total session duration.
        /// </summary>
        public TimeSpan SessionDuration;
        
        /// <summary>
        /// Total data points collected.
        /// </summary>
        public int TotalDataPointsCollected;
        
        /// <summary>
        /// Data collection completion rate.
        /// </summary>
        public float CompletionRate;
        
        /// <summary>
        /// Session quality metrics.
        /// </summary>
        public SessionQualityMetrics QualityMetrics;
        
        /// <summary>
        /// Final data validation result.
        /// </summary>
        public ComfortDataValidationResult FinalValidationResult;
        
        /// <summary>
        /// Session summary statistics.
        /// </summary>
        public SessionSummaryStatistics SummaryStatistics;
        
        /// <summary>
        /// Completion notes.
        /// </summary>
        public string CompletionNotes;
    }
    
    /// <summary>
    /// Comfort data analysis result.
    /// </summary>
    [Serializable]
    public struct ComfortDataAnalysisResult
    {
        /// <summary>
        /// Analysis identifier.
        /// </summary>
        public string AnalysisId;
        
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp;
        
        /// <summary>
        /// Comfort level analysis.
        /// </summary>
        public ComfortLevelAnalysis ComfortAnalysis;
        
        /// <summary>
        /// Motion sickness analysis.
        /// </summary>
        public MotionSicknessAnalysis MotionSicknessAnalysis;
        
        /// <summary>
        /// Physiological response analysis.
        /// </summary>
        public PhysiologicalResponseAnalysis PhysiologicalAnalysis;
        
        /// <summary>
        /// Behavioral pattern analysis.
        /// </summary>
        public BehavioralPatternAnalysis BehavioralAnalysis;
        
        /// <summary>
        /// Temporal trend analysis.
        /// </summary>
        public TemporalTrendAnalysis TrendAnalysis;
        
        /// <summary>
        /// Key insights and findings.
        /// </summary>
        public string[] KeyInsights;
        
        /// <summary>
        /// Recommendations based on analysis.
        /// </summary>
        public string[] Recommendations;
        
        /// <summary>
        /// Analysis confidence level (0-100).
        /// </summary>
        public float AnalysisConfidence;
    }
    
    // Supporting enums and data structures
    
    /// <summary>
    /// Data collection session status.
    /// </summary>
    public enum DataCollectionSessionStatus
    {
        NotStarted,
        Active,
        Paused,
        Completed,
        Terminated,
        Error
    }
    
    /// <summary>
    /// Report format options.
    /// </summary>
    public enum ReportFormat
    {
        PDF,
        HTML,
        Markdown,
        JSON,
        CSV
    }
    
    /// <summary>
    /// Physiological data quality indicators.
    /// </summary>
    [Serializable]
    public struct PhysiologicalDataQuality
    {
        /// <summary>
        /// Signal quality score (0-100).
        /// </summary>
        public float SignalQuality;
        
        /// <summary>
        /// Measurement accuracy score (0-100).
        /// </summary>
        public float MeasurementAccuracy;
        
        /// <summary>
        /// Data completeness percentage.
        /// </summary>
        public float CompletenessPercentage;
        
        /// <summary>
        /// Artifact detection results.
        /// </summary>
        public ArtifactDetectionResult[] ArtifactResults;
        
        /// <summary>
        /// Quality flags and warnings.
        /// </summary>
        public string[] QualityFlags;
    }
    
    /// <summary>
    /// Behavioral data quality indicators.
    /// </summary>
    [Serializable]
    public struct BehavioralDataQuality
    {
        /// <summary>
        /// Observer reliability score (0-100).
        /// </summary>
        public float ObserverReliability;
        
        /// <summary>
        /// Observation completeness percentage.
        /// </summary>
        public float ObservationCompleteness;
        
        /// <summary>
        /// Inter-observer agreement (if multiple observers).
        /// </summary>
        public float InterObserverAgreement;
        
        /// <summary>
        /// Observation consistency score.
        /// </summary>
        public float ConsistencyScore;
        
        /// <summary>
        /// Quality assessment notes.
        /// </summary>
        public string[] QualityNotes;
    }
    
    // Additional supporting structures would be defined in the implementation file
    // to keep this interface focused on the core contract
}