using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Interface for comfort study protocol design and management.
    /// Implements Requirement 7.1: Structured comfort validation study design.
    /// Implements Requirement 7.2: SIM/SSQ study protocol for motion sickness assessment.
    /// </summary>
    public interface IComfortStudyProtocol
    {
        /// <summary>
        /// Whether the comfort study protocol is initialized.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Current study protocol configuration.
        /// </summary>
        ComfortStudyConfiguration Configuration { get; }
        
        /// <summary>
        /// Current study session information.
        /// </summary>
        ComfortStudySession CurrentSession { get; }
        
        /// <summary>
        /// Event fired when a study session is started.
        /// </summary>
        event Action<StudySessionStartedEventArgs> StudySessionStarted;
        
        /// <summary>
        /// Event fired when a study session is completed.
        /// </summary>
        event Action<StudySessionCompletedEventArgs> StudySessionCompleted;
        
        /// <summary>
        /// Event fired when comfort data is collected.
        /// </summary>
        event Action<ComfortDataCollectedEventArgs> ComfortDataCollected;
        
        /// <summary>
        /// Initialize the comfort study protocol system.
        /// </summary>
        /// <param name="config">Study protocol configuration</param>
        /// <returns>True if initialization successful</returns>
        bool Initialize(ComfortStudyConfiguration config);
        
        /// <summary>
        /// Create a new comfort study protocol.
        /// </summary>
        /// <param name="protocolDesign">Protocol design parameters</param>
        /// <returns>Created study protocol</returns>
        StudyProtocol CreateStudyProtocol(StudyProtocolDesign protocolDesign);
        
        /// <summary>
        /// Validate study protocol design for completeness and compliance.
        /// </summary>
        /// <param name="protocol">Protocol to validate</param>
        /// <returns>Validation result</returns>
        ProtocolValidationResult ValidateProtocol(StudyProtocol protocol);
        
        /// <summary>
        /// Start a comfort study session with a participant.
        /// </summary>
        /// <param name="participantInfo">Participant information</param>
        /// <param name="protocol">Study protocol to follow</param>
        /// <returns>Started study session</returns>
        ComfortStudySession StartStudySession(ParticipantInfo participantInfo, StudyProtocol protocol);
        
        /// <summary>
        /// Collect SIM (Simulator Sickness Questionnaire) data during session.
        /// </summary>
        /// <param name="sessionId">Study session identifier</param>
        /// <param name="simData">SIM questionnaire responses</param>
        /// <returns>Data collection result</returns>
        DataCollectionResult CollectSIMData(string sessionId, SIMQuestionnaireData simData);
        
        /// <summary>
        /// Collect SSQ (Simulator Sickness Questionnaire) data during session.
        /// </summary>
        /// <param name="sessionId">Study session identifier</param>
        /// <param name="ssqData">SSQ questionnaire responses</param>
        /// <returns>Data collection result</returns>
        DataCollectionResult CollectSSQData(string sessionId, SSQQuestionnaireData ssqData);
        
        /// <summary>
        /// Complete a study session and generate results.
        /// </summary>
        /// <param name="sessionId">Study session identifier</param>
        /// <returns>Session completion result</returns>
        SessionCompletionResult CompleteStudySession(string sessionId);
        
        /// <summary>
        /// Generate study protocol documentation.
        /// </summary>
        /// <param name="protocol">Protocol to document</param>
        /// <returns>Generated protocol documentation</returns>
        ProtocolDocumentation GenerateProtocolDocumentation(StudyProtocol protocol);
        
        /// <summary>
        /// Analyze comfort study results across multiple sessions.
        /// </summary>
        /// <param name="sessionResults">Collection of session results</param>
        /// <returns>Comprehensive study analysis</returns>
        ComfortStudyAnalysis AnalyzeStudyResults(SessionCompletionResult[] sessionResults);
        
        /// <summary>
        /// Get pre-registered hypotheses for the study.
        /// </summary>
        /// <returns>List of pre-registered hypotheses</returns>
        StudyHypothesis[] GetPreRegisteredHypotheses();
        
        /// <summary>
        /// Evaluate study success criteria against results.
        /// </summary>
        /// <param name="studyAnalysis">Study analysis results</param>
        /// <returns>Success criteria evaluation</returns>
        SuccessCriteriaEvaluation EvaluateSuccessCriteria(ComfortStudyAnalysis studyAnalysis);
        
        /// <summary>
        /// Export study data for external analysis.
        /// </summary>
        /// <param name="format">Export format</param>
        /// <returns>Exported study data</returns>
        StudyDataExport ExportStudyData(DataExportFormat format);
        
        /// <summary>
        /// Reset study protocol state and clear data.
        /// </summary>
        void ResetStudyProtocol();
    }
    
    /// <summary>
    /// Configuration for comfort study protocol.
    /// </summary>
    [Serializable]
    public struct ComfortStudyConfiguration
    {
        /// <summary>
        /// Target number of participants for the study.
        /// </summary>
        public int TargetParticipantCount;
        
        /// <summary>
        /// Duration of each study session in minutes.
        /// </summary>
        public float SessionDurationMinutes;
        
        /// <summary>
        /// Interval between SIM/SSQ measurements in minutes.
        /// </summary>
        public float MeasurementIntervalMinutes;
        
        /// <summary>
        /// Whether to require IRB approval for the study.
        /// </summary>
        public bool RequireIRBApproval;
        
        /// <summary>
        /// Whether to enable debug logging.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Study data storage directory.
        /// </summary>
        public string DataStorageDirectory;
        
        /// <summary>
        /// Default comfort study configuration for n=12 participants.
        /// </summary>
        public static ComfortStudyConfiguration Default => new ComfortStudyConfiguration
        {
            TargetParticipantCount = 12,
            SessionDurationMinutes = 15f,
            MeasurementIntervalMinutes = 5f,
            RequireIRBApproval = true,
            EnableDebugLogging = false,
            DataStorageDirectory = "ComfortStudyData"
        };
    }
    
    /// <summary>
    /// Current comfort study session information.
    /// </summary>
    [Serializable]
    public struct ComfortStudySession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId;
        
        /// <summary>
        /// Participant information.
        /// </summary>
        public ParticipantInfo Participant;
        
        /// <summary>
        /// Study protocol being followed.
        /// </summary>
        public StudyProtocol Protocol;
        
        /// <summary>
        /// Session start time.
        /// </summary>
        public DateTime StartTime;
        
        /// <summary>
        /// Session end time (if completed).
        /// </summary>
        public DateTime? EndTime;
        
        /// <summary>
        /// Whether the session is currently active.
        /// </summary>
        public bool IsActive;
        
        /// <summary>
        /// Current session phase.
        /// </summary>
        public StudySessionPhase CurrentPhase;
        
        /// <summary>
        /// Collected comfort data during session.
        /// </summary>
        public List<ComfortDataPoint> CollectedData;
        
        /// <summary>
        /// Session notes and observations.
        /// </summary>
        public string SessionNotes;
    }
    
    /// <summary>
    /// Study protocol design parameters.
    /// </summary>
    [Serializable]
    public struct StudyProtocolDesign
    {
        /// <summary>
        /// Protocol name and identifier.
        /// </summary>
        public string ProtocolName;
        
        /// <summary>
        /// Study objective and purpose.
        /// </summary>
        public string StudyObjective;
        
        /// <summary>
        /// Target participant demographics.
        /// </summary>
        public ParticipantDemographics TargetDemographics;
        
        /// <summary>
        /// Experimental conditions to test.
        /// </summary>
        public ExperimentalCondition[] ExperimentalConditions;
        
        /// <summary>
        /// Pre-registered hypotheses.
        /// </summary>
        public StudyHypothesis[] PreRegisteredHypotheses;
        
        /// <summary>
        /// Success criteria for the study.
        /// </summary>
        public SuccessCriteria[] SuccessCriteria;
        
        /// <summary>
        /// Data collection procedures.
        /// </summary>
        public DataCollectionProcedure[] DataCollectionProcedures;
        
        /// <summary>
        /// Ethical considerations and safeguards.
        /// </summary>
        public EthicalConsiderations EthicalConsiderations;
    }
    
    /// <summary>
    /// Complete study protocol definition.
    /// </summary>
    [Serializable]
    public struct StudyProtocol
    {
        /// <summary>
        /// Protocol unique identifier.
        /// </summary>
        public string ProtocolId;
        
        /// <summary>
        /// Protocol design parameters.
        /// </summary>
        public StudyProtocolDesign Design;
        
        /// <summary>
        /// Protocol creation timestamp.
        /// </summary>
        public DateTime CreationTime;
        
        /// <summary>
        /// Protocol version number.
        /// </summary>
        public string Version;
        
        /// <summary>
        /// Protocol approval status.
        /// </summary>
        public ProtocolApprovalStatus ApprovalStatus;
        
        /// <summary>
        /// IRB approval information (if required).
        /// </summary>
        public IRBApprovalInfo? IRBApproval;
        
        /// <summary>
        /// Protocol validation results.
        /// </summary>
        public ProtocolValidationResult ValidationResult;
    }
    
    /// <summary>
    /// Participant information for comfort study.
    /// </summary>
    [Serializable]
    public struct ParticipantInfo
    {
        /// <summary>
        /// Anonymous participant identifier.
        /// </summary>
        public string ParticipantId;
        
        /// <summary>
        /// Participant age range.
        /// </summary>
        public AgeRange AgeRange;
        
        /// <summary>
        /// Participant gender (optional).
        /// </summary>
        public Gender? Gender;
        
        /// <summary>
        /// VR experience level.
        /// </summary>
        public VRExperienceLevel VRExperience;
        
        /// <summary>
        /// Motion sickness susceptibility.
        /// </summary>
        public MotionSicknessSusceptibility MotionSusceptibility;
        
        /// <summary>
        /// Consent to participate in study.
        /// </summary>
        public bool HasConsented;
        
        /// <summary>
        /// Participant enrollment timestamp.
        /// </summary>
        public DateTime EnrollmentTime;
        
        /// <summary>
        /// Additional demographic information.
        /// </summary>
        public Dictionary<string, string> AdditionalDemographics;
    }
    
    /// <summary>
    /// SIM (Simulator Sickness Questionnaire) data.
    /// </summary>
    [Serializable]
    public struct SIMQuestionnaireData
    {
        /// <summary>
        /// Data collection timestamp.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// General discomfort level (0-3 scale).
        /// </summary>
        public int GeneralDiscomfort;
        
        /// <summary>
        /// Fatigue level (0-3 scale).
        /// </summary>
        public int Fatigue;
        
        /// <summary>
        /// Headache intensity (0-3 scale).
        /// </summary>
        public int Headache;
        
        /// <summary>
        /// Eye strain level (0-3 scale).
        /// </summary>
        public int EyeStrain;
        
        /// <summary>
        /// Difficulty focusing (0-3 scale).
        /// </summary>
        public int DifficultyFocusing;
        
        /// <summary>
        /// Salivation changes (0-3 scale).
        /// </summary>
        public int Salivation;
        
        /// <summary>
        /// Sweating level (0-3 scale).
        /// </summary>
        public int Sweating;
        
        /// <summary>
        /// Nausea level (0-3 scale).
        /// </summary>
        public int Nausea;
        
        /// <summary>
        /// Difficulty concentrating (0-3 scale).
        /// </summary>
        public int DifficultyConcentrating;
        
        /// <summary>
        /// Fullness of head sensation (0-3 scale).
        /// </summary>
        public int FullnessOfHead;
        
        /// <summary>
        /// Blurred vision (0-3 scale).
        /// </summary>
        public int BlurredVision;
        
        /// <summary>
        /// Dizziness with eyes open (0-3 scale).
        /// </summary>
        public int DizzinessEyesOpen;
        
        /// <summary>
        /// Dizziness with eyes closed (0-3 scale).
        /// </summary>
        public int DizzinessEyesClosed;
        
        /// <summary>
        /// Vertigo sensation (0-3 scale).
        /// </summary>
        public int Vertigo;
        
        /// <summary>
        /// Stomach awareness (0-3 scale).
        /// </summary>
        public int StomachAwareness;
        
        /// <summary>
        /// Burping frequency (0-3 scale).
        /// </summary>
        public int Burping;
        
        /// <summary>
        /// Additional symptoms or notes.
        /// </summary>
        public string AdditionalSymptoms;
    }
    
    /// <summary>
    /// SSQ (Simulator Sickness Questionnaire) data.
    /// </summary>
    [Serializable]
    public struct SSQQuestionnaireData
    {
        /// <summary>
        /// Data collection timestamp.
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Nausea subscale score.
        /// </summary>
        public float NauseaScore;
        
        /// <summary>
        /// Oculomotor subscale score.
        /// </summary>
        public float OculomotorScore;
        
        /// <summary>
        /// Disorientation subscale score.
        /// </summary>
        public float DisorientationScore;
        
        /// <summary>
        /// Total SSQ score.
        /// </summary>
        public float TotalScore;
        
        /// <summary>
        /// Individual item responses (16 items).
        /// </summary>
        public int[] ItemResponses;
        
        /// <summary>
        /// Questionnaire completion time in seconds.
        /// </summary>
        public float CompletionTimeSeconds;
        
        /// <summary>
        /// Additional notes from participant.
        /// </summary>
        public string ParticipantNotes;
    }
    
    /// <summary>
    /// Study hypothesis definition.
    /// </summary>
    [Serializable]
    public struct StudyHypothesis
    {
        /// <summary>
        /// Hypothesis identifier.
        /// </summary>
        public string HypothesisId;
        
        /// <summary>
        /// Hypothesis statement.
        /// </summary>
        public string Statement;
        
        /// <summary>
        /// Hypothesis type (null, alternative, directional).
        /// </summary>
        public HypothesisType Type;
        
        /// <summary>
        /// Expected effect size.
        /// </summary>
        public float ExpectedEffectSize;
        
        /// <summary>
        /// Statistical power target.
        /// </summary>
        public float TargetPower;
        
        /// <summary>
        /// Alpha level for significance testing.
        /// </summary>
        public float AlphaLevel;
        
        /// <summary>
        /// Primary outcome measure.
        /// </summary>
        public string PrimaryOutcome;
        
        /// <summary>
        /// Pre-registration timestamp.
        /// </summary>
        public DateTime PreRegistrationTime;
    }
    
    /// <summary>
    /// Success criteria for study evaluation.
    /// </summary>
    [Serializable]
    public struct SuccessCriteria
    {
        /// <summary>
        /// Criteria identifier.
        /// </summary>
        public string CriteriaId;
        
        /// <summary>
        /// Criteria description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Measurement metric.
        /// </summary>
        public string Metric;
        
        /// <summary>
        /// Target threshold value.
        /// </summary>
        public float TargetThreshold;
        
        /// <summary>
        /// Comparison operator (greater than, less than, equal to).
        /// </summary>
        public ComparisonOperator Operator;
        
        /// <summary>
        /// Criteria priority level.
        /// </summary>
        public CriteriaPriority Priority;
        
        /// <summary>
        /// Whether this is a primary or secondary endpoint.
        /// </summary>
        public bool IsPrimaryEndpoint;
    }
    
    /// <summary>
    /// Experimental condition definition.
    /// </summary>
    [Serializable]
    public struct ExperimentalCondition
    {
        /// <summary>
        /// Condition identifier.
        /// </summary>
        public string ConditionId;
        
        /// <summary>
        /// Condition name and description.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Wave parameters for this condition.
        /// </summary>
        public WaveParameters WaveParameters;
        
        /// <summary>
        /// Expected comfort level for this condition.
        /// </summary>
        public ComfortLevel ExpectedComfortLevel;
        
        /// <summary>
        /// Condition presentation order.
        /// </summary>
        public int PresentationOrder;
        
        /// <summary>
        /// Duration of condition exposure in minutes.
        /// </summary>
        public float ExposureDurationMinutes;
    }
    
    /// <summary>
    /// Wave parameters for experimental conditions.
    /// </summary>
    [Serializable]
    public struct WaveParameters
    {
        /// <summary>
        /// Wave amplitude.
        /// </summary>
        public float Amplitude;
        
        /// <summary>
        /// Wave frequency in Hz.
        /// </summary>
        public float Frequency;
        
        /// <summary>
        /// Wave phase offset.
        /// </summary>
        public float PhaseOffset;
        
        /// <summary>
        /// Wave movement speed.
        /// </summary>
        public float MovementSpeed;
        
        /// <summary>
        /// Additional wave parameters.
        /// </summary>
        public Dictionary<string, float> AdditionalParameters;
    }
    
    /// <summary>
    /// Data collection procedure definition.
    /// </summary>
    [Serializable]
    public struct DataCollectionProcedure
    {
        /// <summary>
        /// Procedure identifier.
        /// </summary>
        public string ProcedureId;
        
        /// <summary>
        /// Procedure name and description.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Data collection timing.
        /// </summary>
        public DataCollectionTiming Timing;
        
        /// <summary>
        /// Data collection method.
        /// </summary>
        public DataCollectionMethod Method;
        
        /// <summary>
        /// Required data fields.
        /// </summary>
        public string[] RequiredFields;
        
        /// <summary>
        /// Procedure instructions for participants.
        /// </summary>
        public string ParticipantInstructions;
        
        /// <summary>
        /// Procedure instructions for researchers.
        /// </summary>
        public string ResearcherInstructions;
    }
    
    /// <summary>
    /// Ethical considerations for the study.
    /// </summary>
    [Serializable]
    public struct EthicalConsiderations
    {
        /// <summary>
        /// Informed consent requirements.
        /// </summary>
        public string InformedConsentRequirements;
        
        /// <summary>
        /// Risk mitigation strategies.
        /// </summary>
        public string[] RiskMitigationStrategies;
        
        /// <summary>
        /// Data privacy and anonymization procedures.
        /// </summary>
        public string DataPrivacyProcedures;
        
        /// <summary>
        /// Participant withdrawal procedures.
        /// </summary>
        public string WithdrawalProcedures;
        
        /// <summary>
        /// Emergency procedures for adverse events.
        /// </summary>
        public string EmergencyProcedures;
        
        /// <summary>
        /// IRB approval requirements.
        /// </summary>
        public string IRBRequirements;
    }
    
    // Enums and supporting types
    
    /// <summary>
    /// Study session phases.
    /// </summary>
    public enum StudySessionPhase
    {
        PreSession,
        Baseline,
        Exposure,
        PostSession,
        Completed
    }
    
    /// <summary>
    /// Age ranges for participants.
    /// </summary>
    public enum AgeRange
    {
        Under18,
        Age18To25,
        Age26To35,
        Age36To45,
        Age46To55,
        Over55
    }
    
    /// <summary>
    /// Gender options.
    /// </summary>
    public enum Gender
    {
        Male,
        Female,
        NonBinary,
        PreferNotToSay
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
        Unknown
    }
    
    /// <summary>
    /// Hypothesis types.
    /// </summary>
    public enum HypothesisType
    {
        Null,
        Alternative,
        Directional
    }
    
    /// <summary>
    /// Comparison operators for success criteria.
    /// </summary>
    public enum ComparisonOperator
    {
        LessThan,
        LessThanOrEqual,
        Equal,
        GreaterThanOrEqual,
        GreaterThan
    }
    
    /// <summary>
    /// Criteria priority levels.
    /// </summary>
    public enum CriteriaPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Expected comfort levels.
    /// </summary>
    public enum ComfortLevel
    {
        VeryUncomfortable,
        Uncomfortable,
        Neutral,
        Comfortable,
        VeryComfortable
    }
    
    /// <summary>
    /// Data collection timing options.
    /// </summary>
    public enum DataCollectionTiming
    {
        PreSession,
        DuringSession,
        PostSession,
        Continuous
    }
    
    /// <summary>
    /// Data collection methods.
    /// </summary>
    public enum DataCollectionMethod
    {
        SIMQuestionnaire,
        SSQQuestionnaire,
        PhysiologicalMeasurement,
        BehavioralObservation,
        InterviewData
    }
    
    /// <summary>
    /// Protocol approval status.
    /// </summary>
    public enum ProtocolApprovalStatus
    {
        Draft,
        UnderReview,
        Approved,
        Rejected,
        RequiresRevision
    }
    
    /// <summary>
    /// Data export formats.
    /// </summary>
    public enum DataExportFormat
    {
        CSV,
        JSON,
        XML,
        SPSS,
        R
    }
    
    // Event argument classes and result structures will be defined in the implementation file
    // to keep this interface file focused on the core contract
}