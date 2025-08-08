using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Supporting data structures for comfort data collection system.
    /// </summary>
    
    /// <summary>
    /// Data quality requirements for collection session.
    /// </summary>
    [Serializable]
    public struct DataQualityRequirements
    {
        /// <summary>
        /// Minimum data quality score required (0-100).
        /// </summary>
        public float MinimumQualityScore;
        
        /// <summary>
        /// Required data completeness percentage.
        /// </summary>
        public float RequiredCompletenessPercentage;
        
        /// <summary>
        /// Maximum allowed response time for questionnaires in seconds.
        /// </summary>
        public float MaxResponseTimeSeconds;
        
        /// <summary>
        /// Whether to require physiological data validation.
        /// </summary>
        public bool RequirePhysiologicalValidation;
        
        /// <summary>
        /// Whether to require behavioral data validation.
        /// </summary>
        public bool RequireBehavioralValidation;
        
        /// <summary>
        /// Custom quality validation rules.
        /// </summary>
        public string[] CustomValidationRules;
    }
    
    /// <summary>
    /// Session quality metrics.
    /// </summary>
    [Serializable]
    public struct SessionQualityMetrics
    {
        /// <summary>
        /// Overall session quality score (0-100).
        /// </summary>
        public float OverallQualityScore;
        
        /// <summary>
        /// Data collection completion rate.
        /// </summary>
        public float CompletionRate;
        
        /// <summary>
        /// Average data quality score.
        /// </summary>
        public float AverageDataQuality;
        
        /// <summary>
        /// Number of data validation failures.
        /// </summary>
        public int ValidationFailures;
        
        /// <summary>
        /// Number of missing data points.
        /// </summary>
        public int MissingDataPoints;
        
        /// <summary>
        /// Session reliability score.
        /// </summary>
        public float ReliabilityScore;
        
        /// <summary>
        /// Quality improvement recommendations.
        /// </summary>
        public string[] QualityRecommendations;
    }
    
    /// <summary>
    /// Automated collection schedule configuration.
    /// </summary>
    [Serializable]
    public struct AutomatedCollectionSchedule
    {
        /// <summary>
        /// Collection interval in seconds.
        /// </summary>
        public float CollectionIntervalSeconds;
        
        /// <summary>
        /// Data collection methods to automate.
        /// </summary>
        public DataCollectionMethod[] AutomatedMethods;
        
        /// <summary>
        /// Whether to enable adaptive scheduling.
        /// </summary>
        public bool EnableAdaptiveScheduling;
        
        /// <summary>
        /// Schedule start time.
        /// </summary>
        public DateTime ScheduleStartTime;
        
        /// <summary>
        /// Schedule end time.
        /// </summary>
        public DateTime ScheduleEndTime;
        
        /// <summary>
        /// Trigger conditions for data collection.
        /// </summary>
        public CollectionTrigger[] TriggerConditions;
        
        /// <summary>
        /// Schedule priority level.
        /// </summary>
        public SchedulePriority Priority;
    }
    
    /// <summary>
    /// Collection trigger condition.
    /// </summary>
    [Serializable]
    public struct CollectionTrigger
    {
        /// <summary>
        /// Trigger identifier.
        /// </summary>
        public string TriggerId;
        
        /// <summary>
        /// Trigger type.
        /// </summary>
        public TriggerType Type;
        
        /// <summary>
        /// Trigger condition description.
        /// </summary>
        public string Condition;
        
        /// <summary>
        /// Trigger threshold value.
        /// </summary>
        public float ThresholdValue;
        
        /// <summary>
        /// Whether trigger is enabled.
        /// </summary>
        public bool IsEnabled;
    }
    
    /// <summary>
    /// Behavioral observation data.
    /// </summary>
    [Serializable]
    public struct BehavioralObservation
    {
        /// <summary>
        /// Observation identifier.
        /// </summary>
        public string ObservationId;
        
        /// <summary>
        /// Observation category.
        /// </summary>
        public ObservationCategory Category;
        
        /// <summary>
        /// Observation description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Observation severity level.
        /// </summary>
        public ObservationSeverity Severity;
        
        /// <summary>
        /// Observation duration in seconds.
        /// </summary>
        public float DurationSeconds;
        
        /// <summary>
        /// Observation confidence level (0-100).
        /// </summary>
        public float ConfidenceLevel;
    }
    
    /// <summary>
    /// Environmental factor information.
    /// </summary>
    [Serializable]
    public struct EnvironmentalFactor
    {
        /// <summary>
        /// Factor identifier.
        /// </summary>
        public string FactorId;
        
        /// <summary>
        /// Factor type.
        /// </summary>
        public EnvironmentalFactorType Type;
        
        /// <summary>
        /// Factor description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Factor measurement value.
        /// </summary>
        public float MeasurementValue;
        
        /// <summary>
        /// Measurement unit.
        /// </summary>
        public string MeasurementUnit;
        
        /// <summary>
        /// Factor impact assessment.
        /// </summary>
        public FactorImpact Impact;
    }
    
    /// <summary>
    /// Data validation result.
    /// </summary>
    [Serializable]
    public struct DataValidationResult
    {
        /// <summary>
        /// Whether validation passed.
        /// </summary>
        public bool IsValid;
        
        /// <summary>
        /// Validation score (0-100).
        /// </summary>
        public float ValidationScore;
        
        /// <summary>
        /// Validation timestamp.
        /// </summary>
        public DateTime ValidationTimestamp;
        
        /// <summary>
        /// Validation method used.
        /// </summary>
        public string ValidationMethod;
        
        /// <summary>
        /// Validation messages.
        /// </summary>
        public string[] ValidationMessages;
        
        /// <summary>
        /// Validation warnings.
        /// </summary>
        public string[] ValidationWarnings;
    }
    
    /// <summary>
    /// Data validation issue.
    /// </summary>
    [Serializable]
    public struct DataValidationIssue
    {
        /// <summary>
        /// Issue identifier.
        /// </summary>
        public string IssueId;
        
        /// <summary>
        /// Issue type.
        /// </summary>
        public ValidationIssueType Type;
        
        /// <summary>
        /// Issue description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Issue severity level.
        /// </summary>
        public ValidationIssueSeverity Severity;
        
        /// <summary>
        /// Affected data point identifier.
        /// </summary>
        public string AffectedDataPointId;
        
        /// <summary>
        /// Recommended resolution.
        /// </summary>
        public string RecommendedResolution;
    }
    
    /// <summary>
    /// Real-time comfort metrics.
    /// </summary>
    [Serializable]
    public struct RealTimeComfortMetrics
    {
        /// <summary>
        /// Current comfort level.
        /// </summary>
        public ComfortLevel CurrentComfortLevel;
        
        /// <summary>
        /// Current motion sickness score.
        /// </summary>
        public float CurrentMotionSicknessScore;
        
        /// <summary>
        /// Comfort trend direction.
        /// </summary>
        public TrendDirection ComfortTrend;
        
        /// <summary>
        /// Time since session start in minutes.
        /// </summary>
        public float SessionTimeMinutes;
        
        /// <summary>
        /// Number of data points collected.
        /// </summary>
        public int DataPointsCollected;
        
        /// <summary>
        /// Current data quality score.
        /// </summary>
        public float CurrentDataQuality;
        
        /// <summary>
        /// Active alerts and warnings.
        /// </summary>
        public ComfortAlert[] ActiveAlerts;
        
        /// <summary>
        /// Metrics update timestamp.
        /// </summary>
        public DateTime UpdateTimestamp;
    }
    
    /// <summary>
    /// Comfort alert information.
    /// </summary>
    [Serializable]
    public struct ComfortAlert
    {
        /// <summary>
        /// Alert identifier.
        /// </summary>
        public string AlertId;
        
        /// <summary>
        /// Alert type.
        /// </summary>
        public ComfortAlertType Type;
        
        /// <summary>
        /// Alert severity level.
        /// </summary>
        public AlertSeverity Severity;
        
        /// <summary>
        /// Alert message.
        /// </summary>
        public string Message;
        
        /// <summary>
        /// Alert trigger timestamp.
        /// </summary>
        public DateTime TriggerTimestamp;
        
        /// <summary>
        /// Recommended actions.
        /// </summary>
        public string[] RecommendedActions;
    }
    
    /// <summary>
    /// Data quality configuration.
    /// </summary>
    [Serializable]
    public struct DataQualityConfiguration
    {
        /// <summary>
        /// Quality validation rules.
        /// </summary>
        public QualityValidationRule[] ValidationRules;
        
        /// <summary>
        /// Quality thresholds.
        /// </summary>
        public QualityThreshold[] QualityThresholds;
        
        /// <summary>
        /// Automated quality checks to enable.
        /// </summary>
        public AutomatedQualityCheck[] AutomatedChecks;
        
        /// <summary>
        /// Quality alert configuration.
        /// </summary>
        public QualityAlertConfiguration AlertConfiguration;
        
        /// <summary>
        /// Whether to enable real-time quality monitoring.
        /// </summary>
        public bool EnableRealTimeMonitoring;
    }
    
    /// <summary>
    /// Quality validation rule.
    /// </summary>
    [Serializable]
    public struct QualityValidationRule
    {
        /// <summary>
        /// Rule identifier.
        /// </summary>
        public string RuleId;
        
        /// <summary>
        /// Rule name.
        /// </summary>
        public string RuleName;
        
        /// <summary>
        /// Rule description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Rule condition.
        /// </summary>
        public string Condition;
        
        /// <summary>
        /// Rule severity level.
        /// </summary>
        public RuleSeverity Severity;
        
        /// <summary>
        /// Whether rule is enabled.
        /// </summary>
        public bool IsEnabled;
    }
    
    /// <summary>
    /// Quality threshold configuration.
    /// </summary>
    [Serializable]
    public struct QualityThreshold
    {
        /// <summary>
        /// Threshold identifier.
        /// </summary>
        public string ThresholdId;
        
        /// <summary>
        /// Metric name.
        /// </summary>
        public string MetricName;
        
        /// <summary>
        /// Minimum acceptable value.
        /// </summary>
        public float MinimumValue;
        
        /// <summary>
        /// Maximum acceptable value.
        /// </summary>
        public float MaximumValue;
        
        /// <summary>
        /// Threshold action to take when violated.
        /// </summary>
        public ThresholdAction Action;
    }
    
    /// <summary>
    /// Automated quality check configuration.
    /// </summary>
    [Serializable]
    public struct AutomatedQualityCheck
    {
        /// <summary>
        /// Check identifier.
        /// </summary>
        public string CheckId;
        
        /// <summary>
        /// Check type.
        /// </summary>
        public QualityCheckType Type;
        
        /// <summary>
        /// Check frequency in seconds.
        /// </summary>
        public float FrequencySeconds;
        
        /// <summary>
        /// Whether check is enabled.
        /// </summary>
        public bool IsEnabled;
        
        /// <summary>
        /// Check parameters.
        /// </summary>
        public Dictionary<string, object> Parameters;
    }
    
    /// <summary>
    /// Quality alert configuration.
    /// </summary>
    [Serializable]
    public struct QualityAlertConfiguration
    {
        /// <summary>
        /// Whether to enable quality alerts.
        /// </summary>
        public bool EnableAlerts;
        
        /// <summary>
        /// Alert threshold levels.
        /// </summary>
        public AlertThreshold[] AlertThresholds;
        
        /// <summary>
        /// Alert notification methods.
        /// </summary>
        public AlertNotificationMethod[] NotificationMethods;
        
        /// <summary>
        /// Alert escalation rules.
        /// </summary>
        public AlertEscalationRule[] EscalationRules;
    }
    
    /// <summary>
    /// Data collection statistics.
    /// </summary>
    [Serializable]
    public struct DataCollectionStatistics
    {
        /// <summary>
        /// Total number of sessions conducted.
        /// </summary>
        public int TotalSessions;
        
        /// <summary>
        /// Total data points collected.
        /// </summary>
        public int TotalDataPoints;
        
        /// <summary>
        /// Average session duration in minutes.
        /// </summary>
        public float AverageSessionDuration;
        
        /// <summary>
        /// Average data quality score.
        /// </summary>
        public float AverageDataQuality;
        
        /// <summary>
        /// Session completion rate.
        /// </summary>
        public float SessionCompletionRate;
        
        /// <summary>
        /// Data validation success rate.
        /// </summary>
        public float ValidationSuccessRate;
        
        /// <summary>
        /// Statistics by collection method.
        /// </summary>
        public Dictionary<DataCollectionMethod, MethodStatistics> MethodStatistics;
        
        /// <summary>
        /// Statistics generation timestamp.
        /// </summary>
        public DateTime StatisticsTimestamp;
    }
    
    /// <summary>
    /// Method-specific statistics.
    /// </summary>
    [Serializable]
    public struct MethodStatistics
    {
        /// <summary>
        /// Number of data points collected.
        /// </summary>
        public int DataPointsCollected;
        
        /// <summary>
        /// Average data quality score.
        /// </summary>
        public float AverageQuality;
        
        /// <summary>
        /// Success rate for this method.
        /// </summary>
        public float SuccessRate;
        
        /// <summary>
        /// Average collection time in seconds.
        /// </summary>
        public float AverageCollectionTime;
    }
    
    /// <summary>
    /// Artifact detection result.
    /// </summary>
    [Serializable]
    public struct ArtifactDetectionResult
    {
        /// <summary>
        /// Artifact type detected.
        /// </summary>
        public ArtifactType Type;
        
        /// <summary>
        /// Artifact severity level.
        /// </summary>
        public ArtifactSeverity Severity;
        
        /// <summary>
        /// Artifact confidence level (0-100).
        /// </summary>
        public float ConfidenceLevel;
        
        /// <summary>
        /// Artifact description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Recommended correction action.
        /// </summary>
        public string RecommendedAction;
    }
    
    // Analysis result structures
    
    /// <summary>
    /// Comfort level analysis results.
    /// </summary>
    [Serializable]
    public struct ComfortLevelAnalysis
    {
        /// <summary>
        /// Average comfort level during session.
        /// </summary>
        public ComfortLevel AverageComfortLevel;
        
        /// <summary>
        /// Minimum comfort level reached.
        /// </summary>
        public ComfortLevel MinimumComfortLevel;
        
        /// <summary>
        /// Maximum comfort level reached.
        /// </summary>
        public ComfortLevel MaximumComfortLevel;
        
        /// <summary>
        /// Comfort level stability score.
        /// </summary>
        public float StabilityScore;
        
        /// <summary>
        /// Time to reach minimum comfort in minutes.
        /// </summary>
        public float TimeToMinimumComfort;
        
        /// <summary>
        /// Comfort recovery time in minutes.
        /// </summary>
        public float ComfortRecoveryTime;
        
        /// <summary>
        /// Comfort level distribution.
        /// </summary>
        public Dictionary<ComfortLevel, float> ComfortDistribution;
    }
    
    /// <summary>
    /// Motion sickness analysis results.
    /// </summary>
    [Serializable]
    public struct MotionSicknessAnalysis
    {
        /// <summary>
        /// Average SIM score.
        /// </summary>
        public float AverageSIMScore;
        
        /// <summary>
        /// Average SSQ score.
        /// </summary>
        public float AverageSSQScore;
        
        /// <summary>
        /// Peak motion sickness score.
        /// </summary>
        public float PeakMotionSicknessScore;
        
        /// <summary>
        /// Time to peak motion sickness in minutes.
        /// </summary>
        public float TimeToPeakSickness;
        
        /// <summary>
        /// Motion sickness onset time in minutes.
        /// </summary>
        public float OnsetTime;
        
        /// <summary>
        /// Motion sickness severity classification.
        /// </summary>
        public MotionSicknessSeverity SeverityClassification;
        
        /// <summary>
        /// Predominant symptoms.
        /// </summary>
        public string[] PredominantSymptoms;
    }
    
    /// <summary>
    /// Physiological response analysis results.
    /// </summary>
    [Serializable]
    public struct PhysiologicalResponseAnalysis
    {
        /// <summary>
        /// Heart rate analysis.
        /// </summary>
        public HeartRateAnalysis HeartRateAnalysis;
        
        /// <summary>
        /// Skin conductance analysis.
        /// </summary>
        public SkinConductanceAnalysis SkinConductanceAnalysis;
        
        /// <summary>
        /// Respiratory analysis.
        /// </summary>
        public RespiratoryAnalysis RespiratoryAnalysis;
        
        /// <summary>
        /// Eye tracking analysis.
        /// </summary>
        public EyeTrackingAnalysis EyeTrackingAnalysis;
        
        /// <summary>
        /// Overall physiological stress level.
        /// </summary>
        public float OverallStressLevel;
        
        /// <summary>
        /// Physiological adaptation indicators.
        /// </summary>
        public string[] AdaptationIndicators;
    }
    
    /// <summary>
    /// Behavioral pattern analysis results.
    /// </summary>
    [Serializable]
    public struct BehavioralPatternAnalysis
    {
        /// <summary>
        /// Most frequent behavioral indicators.
        /// </summary>
        public DiscomfortIndicator[] FrequentIndicators;
        
        /// <summary>
        /// Behavioral pattern timeline.
        /// </summary>
        public BehavioralPattern[] PatternTimeline;
        
        /// <summary>
        /// Observer agreement score.
        /// </summary>
        public float ObserverAgreementScore;
        
        /// <summary>
        /// Behavioral consistency score.
        /// </summary>
        public float ConsistencyScore;
        
        /// <summary>
        /// Identified behavioral clusters.
        /// </summary>
        public BehavioralCluster[] BehavioralClusters;
    }
    
    /// <summary>
    /// Temporal trend analysis results.
    /// </summary>
    [Serializable]
    public struct TemporalTrendAnalysis
    {
        /// <summary>
        /// Overall comfort trend direction.
        /// </summary>
        public TrendDirection OverallTrend;
        
        /// <summary>
        /// Trend change points.
        /// </summary>
        public TrendChangePoint[] ChangePoints;
        
        /// <summary>
        /// Trend stability score.
        /// </summary>
        public float TrendStability;
        
        /// <summary>
        /// Seasonal patterns identified.
        /// </summary>
        public SeasonalPattern[] SeasonalPatterns;
        
        /// <summary>
        /// Trend prediction confidence.
        /// </summary>
        public float PredictionConfidence;
    }
    
    // Supporting enums
    
    public enum SchedulePriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum TriggerType
    {
        TimeInterval,
        ComfortThreshold,
        PhysiologicalThreshold,
        BehavioralIndicator,
        UserRequest
    }
    
    public enum ObservationCategory
    {
        Physical,
        Behavioral,
        Verbal,
        Environmental,
        Technical
    }
    
    public enum ObservationSeverity
    {
        Minimal,
        Mild,
        Moderate,
        Severe,
        Critical
    }
    
    public enum EnvironmentalFactorType
    {
        Temperature,
        Humidity,
        Lighting,
        Noise,
        AirQuality,
        Other
    }
    
    public enum FactorImpact
    {
        Negligible,
        Minor,
        Moderate,
        Major,
        Critical
    }
    
    public enum ValidationIssueType
    {
        MissingData,
        InvalidRange,
        InconsistentData,
        QualityThreshold,
        TimingIssue,
        FormatError
    }
    
    public enum ValidationIssueSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    public enum ComfortAlertType
    {
        HighMotionSickness,
        LowComfortLevel,
        DataQualityIssue,
        SessionTimeout,
        PhysiologicalAlert,
        BehavioralAlert
    }
    
    public enum AlertSeverity
    {
        Info,
        Warning,
        High,
        Critical
    }
    
    public enum RuleSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    public enum ThresholdAction
    {
        Log,
        Alert,
        Terminate,
        Adjust
    }
    
    public enum QualityCheckType
    {
        Completeness,
        Consistency,
        Accuracy,
        Timeliness,
        Validity
    }
    
    public enum AlertNotificationMethod
    {
        Console,
        Email,
        SMS,
        Dashboard,
        API
    }
    
    public enum ArtifactType
    {
        MotionArtifact,
        ElectricalNoise,
        SignalDrift,
        Saturation,
        Disconnection
    }
    
    public enum ArtifactSeverity
    {
        Minimal,
        Minor,
        Moderate,
        Major,
        Severe
    }
    
    public enum MotionSicknessSeverity
    {
        None,
        Minimal,
        Slight,
        Moderate,
        Severe,
        Extreme
    }
    
    // Additional supporting structures
    
    /// <summary>
    /// Alert threshold configuration.
    /// </summary>
    [Serializable]
    public struct AlertThreshold
    {
        public string MetricName;
        public float ThresholdValue;
        public AlertSeverity Severity;
        public string AlertMessage;
    }
    
    /// <summary>
    /// Alert escalation rule.
    /// </summary>
    [Serializable]
    public struct AlertEscalationRule
    {
        public string RuleId;
        public TimeSpan EscalationDelay;
        public AlertSeverity TriggerSeverity;
        public string[] EscalationActions;
    }
    
    /// <summary>
    /// Heart rate analysis results.
    /// </summary>
    [Serializable]
    public struct HeartRateAnalysis
    {
        public float AverageHeartRate;
        public float HeartRateVariability;
        public float MaxHeartRate;
        public float MinHeartRate;
        public string[] AnomalousPatterns;
    }
    
    /// <summary>
    /// Skin conductance analysis results.
    /// </summary>
    [Serializable]
    public struct SkinConductanceAnalysis
    {
        public float AverageConductance;
        public float ConductanceVariability;
        public int NumberOfPeaks;
        public float StressIndicatorScore;
    }
    
    /// <summary>
    /// Respiratory analysis results.
    /// </summary>
    [Serializable]
    public struct RespiratoryAnalysis
    {
        public float AverageRespiratoryRate;
        public float RespiratoryVariability;
        public string[] BreathingPatterns;
        public float StressIndicatorScore;
    }
    
    /// <summary>
    /// Eye tracking analysis results.
    /// </summary>
    [Serializable]
    public struct EyeTrackingAnalysis
    {
        public float AveragePupilDiameter;
        public float BlinkRate;
        public float GazeStability;
        public string[] EyeMovementPatterns;
    }
    
    /// <summary>
    /// Behavioral pattern information.
    /// </summary>
    [Serializable]
    public struct BehavioralPattern
    {
        public string PatternId;
        public string Description;
        public DateTime StartTime;
        public TimeSpan Duration;
        public float Intensity;
    }
    
    /// <summary>
    /// Behavioral cluster information.
    /// </summary>
    [Serializable]
    public struct BehavioralCluster
    {
        public string ClusterId;
        public DiscomfortIndicator[] Indicators;
        public float ClusterStrength;
        public string Description;
    }
    
    /// <summary>
    /// Trend change point information.
    /// </summary>
    [Serializable]
    public struct TrendChangePoint
    {
        public DateTime Timestamp;
        public TrendDirection BeforeTrend;
        public TrendDirection AfterTrend;
        public float ChangeSignificance;
        public string Description;
    }
    
    /// <summary>
    /// Seasonal pattern information.
    /// </summary>
    [Serializable]
    public struct SeasonalPattern
    {
        public string PatternId;
        public TimeSpan Period;
        public float Amplitude;
        public string Description;
    }
    
    // Event argument classes
    
    /// <summary>
    /// Event arguments for data collection started.
    /// </summary>
    public class DataCollectionStartedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public DataCollectionSessionConfiguration Configuration { get; set; }
        public DateTime StartTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for data collection completed.
    /// </summary>
    public class DataCollectionCompletedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public DataCollectionCompletionResult CompletionResult { get; set; }
        public DateTime CompletionTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for data validation failed.
    /// </summary>
    public class DataValidationFailedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public DataValidationIssue[] ValidationIssues { get; set; }
        public DateTime FailureTimestamp { get; set; }
    }
}    

    /// <summary>
    /// Event arguments for comfort data collected.
    /// </summary>
    public class ComfortDataCollectedEventArgs : EventArgs
    {
        public string SessionId { get; set; }
        public ComfortDataPoint DataPoint { get; set; }
        public ComfortDataCollectionResult CollectionResult { get; set; }
        public DateTime CollectionTimestamp { get; set; }
    }
    
    /// <summary>
    /// Participant information for data collection.
    /// </summary>
    [Serializable]
    public struct ParticipantInfo
    {
        /// <summary>
        /// Unique participant identifier.
        /// </summary>
        public string ParticipantId;
        
        /// <summary>
        /// Participant age.
        /// </summary>
        public int Age;
        
        /// <summary>
        /// Participant gender.
        /// </summary>
        public string Gender;
        
        /// <summary>
        /// VR experience level.
        /// </summary>
        public VRExperienceLevel ExperienceLevel;
        
        /// <summary>
        /// Motion sickness susceptibility.
        /// </summary>
        public MotionSicknessSusceptibility Susceptibility;
        
        /// <summary>
        /// Additional participant characteristics.
        /// </summary>
        public Dictionary<string, string> AdditionalCharacteristics;
    }
    
    /// <summary>
    /// Comfort data point collected during session.
    /// </summary>
    [Serializable]
    public struct ComfortDataPoint
    {
        /// <summary>
        /// Unique data point identifier.
        /// </summary>
        public string DataPointId;
        
        /// <summary>
        /// Data collection timestamp.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Time from session start in minutes.
        /// </summary>
        public float TimeFromStartMinutes;
        
        /// <summary>
        /// Data collection method used.
        /// </summary>
        public DataCollectionMethod CollectionMethod;
        
        /// <summary>
        /// Current experimental condition.
        /// </summary>
        public string CurrentCondition;
        
        /// <summary>
        /// SIM questionnaire data (if applicable).
        /// </summary>
        public SIMQuestionnaireData? SIMData;
        
        /// <summary>
        /// SSQ questionnaire data (if applicable).
        /// </summary>
        public SSQQuestionnaireData? SSQData;
        
        /// <summary>
        /// Physiological data (if applicable).
        /// </summary>
        public PhysiologicalComfortData? PhysiologicalData;
        
        /// <summary>
        /// Behavioral data (if applicable).
        /// </summary>
        public BehavioralComfortData? BehavioralData;
        
        /// <summary>
        /// Data quality indicators.
        /// </summary>
        public ComfortDataQualityIndicators QualityIndicators;
        
        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object> Metadata;
    }
    
    /// <summary>
    /// SIM (Simulator Sickness Questionnaire) data.
    /// </summary>
    [Serializable]
    public struct SIMQuestionnaireData
    {
        /// <summary>
        /// Questionnaire responses.
        /// </summary>
        public SIMResponse[] Responses;
        
        /// <summary>
        /// Total SIM score.
        /// </summary>
        public float TotalScore;
        
        /// <summary>
        /// Nausea subscore.
        /// </summary>
        public float NauseaScore;
        
        /// <summary>
        /// Oculomotor subscore.
        /// </summary>
        public float OculomotorScore;
        
        /// <summary>
        /// Disorientation subscore.
        /// </summary>
        public float DisorientationScore;
        
        /// <summary>
        /// Time taken to complete questionnaire in seconds.
        /// </summary>
        public float CompletionTimeSeconds;
        
        /// <summary>
        /// Questionnaire administration timestamp.
        /// </summary>
        public DateTime AdministrationTimestamp;
        
        /// <summary>
        /// Response quality indicators.
        /// </summary>
        public QuestionnaireQuality ResponseQuality;
    }
    
    /// <summary>
    /// SSQ (Simulator Sickness Questionnaire) data.
    /// </summary>
    [Serializable]
    public struct SSQQuestionnaireData
    {
        /// <summary>
        /// Questionnaire responses.
        /// </summary>
        public SSQResponse[] Responses;
        
        /// <summary>
        /// Total SSQ score.
        /// </summary>
        public float TotalScore;
        
        /// <summary>
        /// Nausea subscore.
        /// </summary>
        public float NauseaScore;
        
        /// <summary>
        /// Oculomotor subscore.
        /// </summary>
        public float OculomotorScore;
        
        /// <summary>
        /// Disorientation subscore.
        /// </summary>
        public float DisorientationScore;
        
        /// <summary>
        /// Time taken to complete questionnaire in seconds.
        /// </summary>
        public float CompletionTimeSeconds;
        
        /// <summary>
        /// Questionnaire administration timestamp.
        /// </summary>
        public DateTime AdministrationTimestamp;
        
        /// <summary>
        /// Response quality indicators.
        /// </summary>
        public QuestionnaireQuality ResponseQuality;
    }
    
    /// <summary>
    /// Individual SIM response.
    /// </summary>
    [Serializable]
    public struct SIMResponse
    {
        /// <summary>
        /// Question identifier.
        /// </summary>
        public string QuestionId;
        
        /// <summary>
        /// Question text.
        /// </summary>
        public string QuestionText;
        
        /// <summary>
        /// Response value (0-3 scale).
        /// </summary>
        public int ResponseValue;
        
        /// <summary>
        /// Response timestamp.
        /// </summary>
        public DateTime ResponseTimestamp;
        
        /// <summary>
        /// Time taken to respond in seconds.
        /// </summary>
        public float ResponseTimeSeconds;
    }
    
    /// <summary>
    /// Individual SSQ response.
    /// </summary>
    [Serializable]
    public struct SSQResponse
    {
        /// <summary>
        /// Question identifier.
        /// </summary>
        public string QuestionId;
        
        /// <summary>
        /// Question text.
        /// </summary>
        public string QuestionText;
        
        /// <summary>
        /// Response value (0-3 scale).
        /// </summary>
        public int ResponseValue;
        
        /// <summary>
        /// Response timestamp.
        /// </summary>
        public DateTime ResponseTimestamp;
        
        /// <summary>
        /// Time taken to respond in seconds.
        /// </summary>
        public float ResponseTimeSeconds;
    }
    
    /// <summary>
    /// Comfort data quality indicators.
    /// </summary>
    [Serializable]
    public struct ComfortDataQualityIndicators
    {
        /// <summary>
        /// Overall quality score (0-100).
        /// </summary>
        public float OverallQuality;
        
        /// <summary>
        /// Data completeness score (0-100).
        /// </summary>
        public float CompletenessScore;
        
        /// <summary>
        /// Data consistency score (0-100).
        /// </summary>
        public float ConsistencyScore;
        
        /// <summary>
        /// Data accuracy score (0-100).
        /// </summary>
        public float AccuracyScore;
        
        /// <summary>
        /// Data timeliness score (0-100).
        /// </summary>
        public float TimelinessScore;
        
        /// <summary>
        /// Quality flags and warnings.
        /// </summary>
        public string[] QualityFlags;
    }
    
    /// <summary>
    /// Questionnaire response quality indicators.
    /// </summary>
    [Serializable]
    public struct QuestionnaireQuality
    {
        /// <summary>
        /// Response completeness percentage.
        /// </summary>
        public float CompletenessPercentage;
        
        /// <summary>
        /// Response consistency score.
        /// </summary>
        public float ConsistencyScore;
        
        /// <summary>
        /// Average response time in seconds.
        /// </summary>
        public float AverageResponseTime;
        
        /// <summary>
        /// Response pattern analysis.
        /// </summary>
        public string[] ResponsePatterns;
        
        /// <summary>
        /// Quality concerns identified.
        /// </summary>
        public string[] QualityConcerns;
    }
    
    /// <summary>
    /// Comfort data report.
    /// </summary>
    [Serializable]
    public struct ComfortDataReport
    {
        /// <summary>
        /// Report identifier.
        /// </summary>
        public string ReportId;
        
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Report format.
        /// </summary>
        public ReportFormat ReportFormat;
        
        /// <summary>
        /// Report title.
        /// </summary>
        public string ReportTitle;
        
        /// <summary>
        /// Report content.
        /// </summary>
        public string ReportContent;
        
        /// <summary>
        /// Report summary.
        /// </summary>
        public string ReportSummary;
        
        /// <summary>
        /// Report generation timestamp.
        /// </summary>
        public DateTime GenerationTimestamp;
        
        /// <summary>
        /// Report metadata.
        /// </summary>
        public Dictionary<string, object> ReportMetadata;
    }
    
    /// <summary>
    /// Comfort data export.
    /// </summary>
    [Serializable]
    public struct ComfortDataExport
    {
        /// <summary>
        /// Export identifier.
        /// </summary>
        public string ExportId;
        
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Export format.
        /// </summary>
        public DataExportFormat ExportFormat;
        
        /// <summary>
        /// Export content.
        /// </summary>
        public string ExportContent;
        
        /// <summary>
        /// Whether export was successful.
        /// </summary>
        public bool IsSuccessful;
        
        /// <summary>
        /// Export timestamp.
        /// </summary>
        public DateTime ExportTimestamp;
        
        /// <summary>
        /// Number of records exported.
        /// </summary>
        public int RecordCount;
        
        /// <summary>
        /// Export file path.
        /// </summary>
        public string FilePath;
        
        /// <summary>
        /// Export messages.
        /// </summary>
        public string[] ExportMessages;
    }
    
    /// <summary>
    /// Session summary statistics.
    /// </summary>
    [Serializable]
    public struct SessionSummaryStatistics
    {
        /// <summary>
        /// Total number of data points collected.
        /// </summary>
        public int TotalDataPoints;
        
        /// <summary>
        /// Average data quality score.
        /// </summary>
        public float AverageDataQuality;
        
        /// <summary>
        /// Session duration.
        /// </summary>
        public TimeSpan SessionDuration;
        
        /// <summary>
        /// Data points collected.
        /// </summary>
        public ComfortDataPoint[] DataPoints;
        
        /// <summary>
        /// Statistics by collection method.
        /// </summary>
        public Dictionary<DataCollectionMethod, int> DataPointsByMethod;
    }
    
    // Additional enums
    
    /// <summary>
    /// Data collection methods.
    /// </summary>
    public enum DataCollectionMethod
    {
        SIMQuestionnaire,
        SSQQuestionnaire,
        PhysiologicalMeasurement,
        BehavioralObservation,
        SelfReport,
        AutomatedSensing
    }
    
    /// <summary>
    /// Data export formats.
    /// </summary>
    public enum DataExportFormat
    {
        JSON,
        CSV,
        XML,
        Excel,
        SPSS
    }
    
    /// <summary>
    /// VR experience levels.
    /// </summary>
    public enum VRExperienceLevel
    {
        None,
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }
    
    /// <summary>
    /// Motion sickness susceptibility levels.
    /// </summary>
    public enum MotionSicknessSusceptibility
    {
        Low,
        Moderate,
        High,
        VeryHigh,
        Unknown
    }
    
    /// <summary>
    /// Comfort levels.
    /// </summary>
    public enum ComfortLevel
    {
        Unknown,
        VeryUncomfortable,
        Uncomfortable,
        SlightlyUncomfortable,
        Neutral,
        SlightlyComfortable,
        Comfortable,
        VeryComfortable
    }
    
    /// <summary>
    /// Discomfort indicators.
    /// </summary>
    public enum DiscomfortIndicator
    {
        Nausea,
        Dizziness,
        Headache,
        EyeStrain,
        Fatigue,
        Sweating,
        PaleSkin,
        HeadMovement,
        BodyMovement,
        VerbalComplaint,
        FacialExpression,
        PosturalInstability
    }
    
    /// <summary>
    /// Trend directions.
    /// </summary>
    public enum TrendDirection
    {
        Unknown,
        Improving,
        Stable,
        Declining,
        Fluctuating
    }
}