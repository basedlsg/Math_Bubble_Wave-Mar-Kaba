using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Supporting data structures for comfort study protocol system.
    /// </summary>
    
    /// <summary>
    /// Result of protocol validation.
    /// </summary>
    [Serializable]
    public struct ProtocolValidationResult
    {
        /// <summary>
        /// Whether the protocol is valid.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// Validation score (0-100).
        /// </summary>
        public float ValidationScore;
        
        /// <summary>
        /// Validation messages and warnings.
        /// </summary>
        public string[] ValidationMessages;
        
        /// <summary>
        /// Required corrections for approval.
        /// </summary>
        public string[] RequiredCorrections;
        
        /// <summary>
        /// Validation timestamp.
        /// </summary>
        public DateTime ValidationTimestamp;
        
        /// <summary>
        /// Validator identifier.
        /// </summary>
        public string ValidatorId;
    }
    
    /// <summary>
    /// Result of data collection operation.
    /// </summary>
    [Serializable]
    public struct DataCollectionResult
    {
        /// <summary>
        /// Whether data collection was successful.
        /// </summary>
        public bool IsSuccessful;
        
        /// <summary>
        /// Collected data identifier.
        /// </summary>
        public string DataId;
        
        /// <summary>
        /// Data collection timestamp.
        /// </summary>
        public DateTime CollectionTimestamp;
        
        /// <summary>
        /// Data quality score (0-100).
        /// </summary>
        public float DataQualityScore;
        
        /// <summary>
        /// Collection messages and notes.
        /// </summary>
        public string[] CollectionMessages;
        
        /// <summary>
        /// Any errors encountered during collection.
        /// </summary>
        public string[] CollectionErrors;
    }
    
    /// <summary>
    /// Result of session completion.
    /// </summary>
    [Serializable]
    public struct SessionCompletionResult
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
        /// Participant information.
        /// </summary>
        public ParticipantInfo Participant;
        
        /// <summary>
        /// All collected comfort data.
        /// </summary>
        public ComfortDataPoint[] CollectedData;
        
        /// <summary>
        /// Session summary statistics.
        /// </summary>
        public SessionSummaryStatistics SummaryStatistics;
        
        /// <summary>
        /// Session completion notes.
        /// </summary>
        public string CompletionNotes;
        
        /// <summary>
        /// Any adverse events reported.
        /// </summary>
        public AdverseEvent[] AdverseEvents;
    }
    
    /// <summary>
    /// Individual comfort data point.
    /// </summary>
    [Serializable]
    public struct ComfortDataPoint
    {
        /// <summary>
        /// Data point identifier.
        /// </summary>
        public string DataPointId;
        
        /// <summary>
        /// Data collection timestamp.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Time since session start in minutes.
        /// </summary>
        public float TimeFromStartMinutes;
        
        /// <summary>
        /// Data collection method used.
        /// </summary>
        public DataCollectionMethod CollectionMethod;
        
        /// <summary>
        /// SIM questionnaire data (if applicable).
        /// </summary>
        public SIMQuestionnaireData? SIMData;
        
        /// <summary>
        /// SSQ questionnaire data (if applicable).
        /// </summary>
        public SSQQuestionnaireData? SSQData;
        
        /// <summary>
        /// Physiological measurements (if applicable).
        /// </summary>
        public PhysiologicalData? PhysiologicalData;
        
        /// <summary>
        /// Behavioral observations (if applicable).
        /// </summary>
        public BehavioralObservation? BehavioralData;
        
        /// <summary>
        /// Current experimental condition.
        /// </summary>
        public string CurrentCondition;
        
        /// <summary>
        /// Data quality indicators.
        /// </summary>
        public DataQualityIndicators QualityIndicators;
    }
    
    /// <summary>
    /// Physiological measurement data.
    /// </summary>
    [Serializable]
    public struct PhysiologicalData
    {
        /// <summary>
        /// Heart rate in beats per minute.
        /// </summary>
        public float HeartRateBPM;
        
        /// <summary>
        /// Skin conductance level.
        /// </summary>
        public float SkinConductance;
        
        /// <summary>
        /// Eye tracking data (if available).
        /// </summary>
        public EyeTrackingData? EyeTracking;
        
        /// <summary>
        /// Head movement data.
        /// </summary>
        public HeadMovementData HeadMovement;
        
        /// <summary>
        /// Additional physiological measurements.
        /// </summary>
        public Dictionary<string, float> AdditionalMeasurements;
    }
    
    /// <summary>
    /// Eye tracking measurement data.
    /// </summary>
    [Serializable]
    public struct EyeTrackingData
    {
        /// <summary>
        /// Pupil diameter in millimeters.
        /// </summary>
        public float PupilDiameter;
        
        /// <summary>
        /// Blink rate per minute.
        /// </summary>
        public float BlinkRate;
        
        /// <summary>
        /// Gaze stability measure.
        /// </summary>
        public float GazeStability;
        
        /// <summary>
        /// Saccade frequency.
        /// </summary>
        public float SaccadeFrequency;
    }
    
    /// <summary>
    /// Head movement measurement data.
    /// </summary>
    [Serializable]
    public struct HeadMovementData
    {
        /// <summary>
        /// Head rotation velocity in degrees per second.
        /// </summary>
        public float RotationVelocity;
        
        /// <summary>
        /// Head translation velocity in meters per second.
        /// </summary>
        public float TranslationVelocity;
        
        /// <summary>
        /// Head movement stability measure.
        /// </summary>
        public float MovementStability;
        
        /// <summary>
        /// Postural sway measurement.
        /// </summary>
        public float PosturalSway;
    }
    
    /// <summary>
    /// Behavioral observation data.
    /// </summary>
    [Serializable]
    public struct BehavioralObservation
    {
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
        public DiscomfortIndicator[] DiscomfortIndicators;
        
        /// <summary>
        /// Participant verbal reports.
        /// </summary>
        public string[] VerbalReports;
        
        /// <summary>
        /// Observer notes.
        /// </summary>
        public string ObserverNotes;
        
        /// <summary>
        /// Observation confidence level (0-100).
        /// </summary>
        public float ConfidenceLevel;
    }
    
    /// <summary>
    /// Data quality indicators.
    /// </summary>
    [Serializable]
    public struct DataQualityIndicators
    {
        /// <summary>
        /// Overall data quality score (0-100).
        /// </summary>
        public float OverallQuality;
        
        /// <summary>
        /// Data completeness percentage.
        /// </summary>
        public float CompletenessPercent;
        
        /// <summary>
        /// Data consistency score.
        /// </summary>
        public float ConsistencyScore;
        
        /// <summary>
        /// Response time for questionnaires in seconds.
        /// </summary>
        public float ResponseTimeSeconds;
        
        /// <summary>
        /// Whether data passed validation checks.
        /// </summary>
        public bool PassedValidation;
        
        /// <summary>
        /// Quality flags and warnings.
        /// </summary>
        public string[] QualityFlags;
    }
    
    /// <summary>
    /// Session summary statistics.
    /// </summary>
    [Serializable]
    public struct SessionSummaryStatistics
    {
        /// <summary>
        /// Average SIM total score.
        /// </summary>
        public float AverageSIMScore;
        
        /// <summary>
        /// Average SSQ total score.
        /// </summary>
        public float AverageSSQScore;
        
        /// <summary>
        /// Peak discomfort level reached.
        /// </summary>
        public float PeakDiscomfortLevel;
        
        /// <summary>
        /// Time to peak discomfort in minutes.
        /// </summary>
        public float TimeToPeakDiscomfortMinutes;
        
        /// <summary>
        /// Number of data collection points.
        /// </summary>
        public int DataPointCount;
        
        /// <summary>
        /// Data collection completion rate.
        /// </summary>
        public float CompletionRate;
        
        /// <summary>
        /// Session quality score.
        /// </summary>
        public float SessionQualityScore;
        
        /// <summary>
        /// Whether session met minimum data requirements.
        /// </summary>
        public bool MetMinimumRequirements;
    }
    
    /// <summary>
    /// Adverse event information.
    /// </summary>
    [Serializable]
    public struct AdverseEvent
    {
        /// <summary>
        /// Event identifier.
        /// </summary>
        public string EventId;
        
        /// <summary>
        /// Event occurrence timestamp.
        /// </summary>
        public DateTime OccurrenceTime;
        
        /// <summary>
        /// Event severity level.
        /// </summary>
        public AdverseEventSeverity Severity;
        
        /// <summary>
        /// Event description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Actions taken in response.
        /// </summary>
        public string ActionsTaken;
        
        /// <summary>
        /// Event outcome.
        /// </summary>
        public AdverseEventOutcome Outcome;
        
        /// <summary>
        /// Whether event was related to study procedures.
        /// </summary>
        public bool StudyRelated;
        
        /// <summary>
        /// Reporter identifier.
        /// </summary>
        public string ReporterId;
    }
    
    /// <summary>
    /// Protocol documentation structure.
    /// </summary>
    [Serializable]
    public struct ProtocolDocumentation
    {
        /// <summary>
        /// Protocol identifier.
        /// </summary>
        public string ProtocolId;
        
        /// <summary>
        /// Generated documentation content.
        /// </summary>
        public string DocumentationContent;
        
        /// <summary>
        /// Documentation format.
        /// </summary>
        public DocumentationFormat Format;
        
        /// <summary>
        /// Documentation generation timestamp.
        /// </summary>
        public DateTime GenerationTimestamp;
        
        /// <summary>
        /// Documentation version.
        /// </summary>
        public string Version;
        
        /// <summary>
        /// Included sections.
        /// </summary>
        public DocumentationSection[] IncludedSections;
        
        /// <summary>
        /// Documentation file path (if saved).
        /// </summary>
        public string FilePath;
    }
    
    /// <summary>
    /// Comprehensive study analysis results.
    /// </summary>
    [Serializable]
    public struct ComfortStudyAnalysis
    {
        /// <summary>
        /// Analysis identifier.
        /// </summary>
        public string AnalysisId;
        
        /// <summary>
        /// Number of participants analyzed.
        /// </summary>
        public int ParticipantCount;
        
        /// <summary>
        /// Overall study completion rate.
        /// </summary>
        public float StudyCompletionRate;
        
        /// <summary>
        /// Descriptive statistics for comfort measures.
        /// </summary>
        public ComfortStatistics ComfortStatistics;
        
        /// <summary>
        /// Hypothesis test results.
        /// </summary>
        public HypothesisTestResult[] HypothesisResults;
        
        /// <summary>
        /// Effect size calculations.
        /// </summary>
        public EffectSizeAnalysis[] EffectSizes;
        
        /// <summary>
        /// Adverse event summary.
        /// </summary>
        public AdverseEventSummary AdverseEventSummary;
        
        /// <summary>
        /// Data quality assessment.
        /// </summary>
        public StudyDataQualityAssessment DataQuality;
        
        /// <summary>
        /// Analysis timestamp.
        /// </summary>
        public DateTime AnalysisTimestamp;
        
        /// <summary>
        /// Analysis notes and interpretations.
        /// </summary>
        public string AnalysisNotes;
    }
    
    /// <summary>
    /// Comfort statistics summary.
    /// </summary>
    [Serializable]
    public struct ComfortStatistics
    {
        /// <summary>
        /// Mean SIM scores across conditions.
        /// </summary>
        public Dictionary<string, float> MeanSIMScores;
        
        /// <summary>
        /// Mean SSQ scores across conditions.
        /// </summary>
        public Dictionary<string, float> MeanSSQScores;
        
        /// <summary>
        /// Standard deviations for comfort measures.
        /// </summary>
        public Dictionary<string, float> StandardDeviations;
        
        /// <summary>
        /// Confidence intervals for means.
        /// </summary>
        public Dictionary<string, ConfidenceInterval> ConfidenceIntervals;
        
        /// <summary>
        /// Dropout rates by condition.
        /// </summary>
        public Dictionary<string, float> DropoutRates;
    }
    
    /// <summary>
    /// Hypothesis test result.
    /// </summary>
    [Serializable]
    public struct HypothesisTestResult
    {
        /// <summary>
        /// Hypothesis identifier.
        /// </summary>
        public string HypothesisId;
        
        /// <summary>
        /// Statistical test used.
        /// </summary>
        public string StatisticalTest;
        
        /// <summary>
        /// Test statistic value.
        /// </summary>
        public float TestStatistic;
        
        /// <summary>
        /// P-value.
        /// </summary>
        public float PValue;
        
        /// <summary>
        /// Whether result is statistically significant.
        /// </summary>
        public bool IsSignificant;
        
        /// <summary>
        /// Effect size estimate.
        /// </summary>
        public float EffectSize;
        
        /// <summary>
        /// Confidence interval for effect size.
        /// </summary>
        public ConfidenceInterval EffectSizeCI;
        
        /// <summary>
        /// Test interpretation.
        /// </summary>
        public string Interpretation;
    }
    
    /// <summary>
    /// Effect size analysis.
    /// </summary>
    [Serializable]
    public struct EffectSizeAnalysis
    {
        /// <summary>
        /// Comparison identifier.
        /// </summary>
        public string ComparisonId;
        
        /// <summary>
        /// Effect size measure (Cohen's d, eta-squared, etc.).
        /// </summary>
        public string EffectSizeMeasure;
        
        /// <summary>
        /// Effect size value.
        /// </summary>
        public float EffectSizeValue;
        
        /// <summary>
        /// Effect size interpretation (small, medium, large).
        /// </summary>
        public EffectSizeMagnitude Magnitude;
        
        /// <summary>
        /// Confidence interval for effect size.
        /// </summary>
        public ConfidenceInterval ConfidenceInterval;
        
        /// <summary>
        /// Practical significance assessment.
        /// </summary>
        public string PracticalSignificance;
    }
    
    /// <summary>
    /// Success criteria evaluation result.
    /// </summary>
    [Serializable]
    public struct SuccessCriteriaEvaluation
    {
        /// <summary>
        /// Overall study success status.
        /// </summary>
        public bool StudySuccessful;
        
        /// <summary>
        /// Individual criteria evaluation results.
        /// </summary>
        public CriteriaEvaluationResult[] CriteriaResults;
        
        /// <summary>
        /// Primary endpoint success rate.
        /// </summary>
        public float PrimaryEndpointSuccessRate;
        
        /// <summary>
        /// Secondary endpoint success rate.
        /// </summary>
        public float SecondaryEndpointSuccessRate;
        
        /// <summary>
        /// Evaluation timestamp.
        /// </summary>
        public DateTime EvaluationTimestamp;
        
        /// <summary>
        /// Evaluation summary.
        /// </summary>
        public string EvaluationSummary;
    }
    
    /// <summary>
    /// Individual criteria evaluation result.
    /// </summary>
    [Serializable]
    public struct CriteriaEvaluationResult
    {
        /// <summary>
        /// Criteria identifier.
        /// </summary>
        public string CriteriaId;
        
        /// <summary>
        /// Whether criteria was met.
        /// </summary>
        public bool CriteriaMet;
        
        /// <summary>
        /// Actual measured value.
        /// </summary>
        public float ActualValue;
        
        /// <summary>
        /// Target threshold value.
        /// </summary>
        public float TargetValue;
        
        /// <summary>
        /// Difference from target.
        /// </summary>
        public float DifferenceFromTarget;
        
        /// <summary>
        /// Evaluation confidence level.
        /// </summary>
        public float ConfidenceLevel;
        
        /// <summary>
        /// Evaluation notes.
        /// </summary>
        public string EvaluationNotes;
    }
    
    /// <summary>
    /// Study data export structure.
    /// </summary>
    [Serializable]
    public struct StudyDataExport
    {
        /// <summary>
        /// Export identifier.
        /// </summary>
        public string ExportId;
        
        /// <summary>
        /// Export format.
        /// </summary>
        public DataExportFormat Format;
        
        /// <summary>
        /// Exported data content.
        /// </summary>
        public string DataContent;
        
        /// <summary>
        /// Export timestamp.
        /// </summary>
        public DateTime ExportTimestamp;
        
        /// <summary>
        /// Number of records exported.
        /// </summary>
        public int RecordCount;
        
        /// <summary>
        /// Export file path (if saved).
        /// </summary>
        public string FilePath;
        
        /// <summary>
        /// Data anonymization level.
        /// </summary>
        public DataAnonymizationLevel AnonymizationLevel;
    }
    
    /// <summary>
    /// IRB approval information.
    /// </summary>
    [Serializable]
    public struct IRBApprovalInfo
    {
        /// <summary>
        /// IRB approval number.
        /// </summary>
        public string ApprovalNumber;
        
        /// <summary>
        /// Approving institution.
        /// </summary>
        public string Institution;
        
        /// <summary>
        /// Approval date.
        /// </summary>
        public DateTime ApprovalDate;
        
        /// <summary>
        /// Approval expiration date.
        /// </summary>
        public DateTime ExpirationDate;
        
        /// <summary>
        /// Approval conditions or restrictions.
        /// </summary>
        public string[] ApprovalConditions;
        
        /// <summary>
        /// Contact information for IRB.
        /// </summary>
        public string IRBContactInfo;
    }
    
    /// <summary>
    /// Target participant demographics.
    /// </summary>
    [Serializable]
    public struct ParticipantDemographics
    {
        /// <summary>
        /// Target age ranges.
        /// </summary>
        public AgeRange[] TargetAgeRanges;
        
        /// <summary>
        /// Target gender distribution.
        /// </summary>
        public Dictionary<Gender, float> GenderDistribution;
        
        /// <summary>
        /// Target VR experience levels.
        /// </summary>
        public VRExperienceLevel[] TargetVRExperience;
        
        /// <summary>
        /// Inclusion criteria.
        /// </summary>
        public string[] InclusionCriteria;
        
        /// <summary>
        /// Exclusion criteria.
        /// </summary>
        public string[] ExclusionCriteria;
        
        /// <summary>
        /// Recruitment strategy.
        /// </summary>
        public string RecruitmentStrategy;
    }
    
    // Additional enums
    
    /// <summary>
    /// Discomfort indicators for behavioral observation.
    /// </summary>
    public enum DiscomfortIndicator
    {
        Nausea,
        Dizziness,
        Headache,
        EyeStrain,
        Fatigue,
        Sweating,
        PallorFlushing,
        RestlessMovement,
        VerbalComplaint,
        RequestToStop
    }
    
    /// <summary>
    /// Adverse event severity levels.
    /// </summary>
    public enum AdverseEventSeverity
    {
        Mild,
        Moderate,
        Severe,
        LifeThreatening
    }
    
    /// <summary>
    /// Adverse event outcomes.
    /// </summary>
    public enum AdverseEventOutcome
    {
        Resolved,
        Ongoing,
        ResolvedWithSequelae,
        Fatal,
        Unknown
    }
    
    /// <summary>
    /// Documentation formats.
    /// </summary>
    public enum DocumentationFormat
    {
        Markdown,
        HTML,
        PDF,
        Word,
        LaTeX
    }
    
    /// <summary>
    /// Documentation sections.
    /// </summary>
    public enum DocumentationSection
    {
        Abstract,
        Introduction,
        Methods,
        Procedures,
        EthicalConsiderations,
        StatisticalAnalysis,
        References,
        Appendices
    }
    
    /// <summary>
    /// Effect size magnitudes.
    /// </summary>
    public enum EffectSizeMagnitude
    {
        Negligible,
        Small,
        Medium,
        Large,
        VeryLarge
    }
    
    /// <summary>
    /// Data anonymization levels.
    /// </summary>
    public enum DataAnonymizationLevel
    {
        None,
        Pseudonymized,
        Anonymized,
        FullyDeidentified
    }
    
    /// <summary>
    /// Confidence interval structure.
    /// </summary>
    [Serializable]
    public struct ConfidenceInterval
    {
        /// <summary>
        /// Lower bound of confidence interval.
        /// </summary>
        public float LowerBound;
        
        /// <summary>
        /// Upper bound of confidence interval.
        /// </summary>
        public float UpperBound;
        
        /// <summary>
        /// Confidence level (e.g., 0.95 for 95% CI).
        /// </summary>
        public float ConfidenceLevel;
    }
    
    /// <summary>
    /// Adverse event summary.
    /// </summary>
    [Serializable]
    public struct AdverseEventSummary
    {
        /// <summary>
        /// Total number of adverse events.
        /// </summary>
        public int TotalEvents;
        
        /// <summary>
        /// Events by severity level.
        /// </summary>
        public Dictionary<AdverseEventSeverity, int> EventsBySeverity;
        
        /// <summary>
        /// Study-related events count.
        /// </summary>
        public int StudyRelatedEvents;
        
        /// <summary>
        /// Participant dropout due to adverse events.
        /// </summary>
        public int DropoutsDueToAE;
        
        /// <summary>
        /// Most common adverse events.
        /// </summary>
        public string[] MostCommonEvents;
    }
    
    /// <summary>
    /// Study data quality assessment.
    /// </summary>
    [Serializable]
    public struct StudyDataQualityAssessment
    {
        /// <summary>
        /// Overall data quality score (0-100).
        /// </summary>
        public float OverallQualityScore;
        
        /// <summary>
        /// Data completeness rate.
        /// </summary>
        public float CompletenessRate;
        
        /// <summary>
        /// Data consistency score.
        /// </summary>
        public float ConsistencyScore;
        
        /// <summary>
        /// Protocol adherence rate.
        /// </summary>
        public float ProtocolAdherenceRate;
        
        /// <summary>
        /// Quality issues identified.
        /// </summary>
        public string[] QualityIssues;
        
        /// <summary>
        /// Recommendations for data quality improvement.
        /// </summary>
        public string[] QualityRecommendations;
    }
    
    // Event argument classes
    
    /// <summary>
    /// Event arguments for study session started.
    /// </summary>
    public class StudySessionStartedEventArgs : EventArgs
    {
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// Participant information.
        /// </summary>
        public ParticipantInfo Participant { get; set; }
        
        /// <summary>
        /// Study protocol being followed.
        /// </summary>
        public StudyProtocol Protocol { get; set; }
        
        /// <summary>
        /// Session start timestamp.
        /// </summary>
        public DateTime StartTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for study session completed.
    /// </summary>
    public class StudySessionCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Session completion result.
        /// </summary>
        public SessionCompletionResult CompletionResult { get; set; }
        
        /// <summary>
        /// Session completion timestamp.
        /// </summary>
        public DateTime CompletionTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for comfort data collected.
    /// </summary>
    public class ComfortDataCollectedEventArgs : EventArgs
    {
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// Collected comfort data point.
        /// </summary>
        public ComfortDataPoint DataPoint { get; set; }
        
        /// <summary>
        /// Data collection result.
        /// </summary>
        public DataCollectionResult CollectionResult { get; set; }
        
        /// <summary>
        /// Data collection timestamp.
        /// </summary>
        public DateTime CollectionTimestamp { get; set; }
    }
}