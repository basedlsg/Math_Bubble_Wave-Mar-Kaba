using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Supporting data structures for academic partnership infrastructure.
    /// </summary>
    
    /// <summary>
    /// Research contact information.
    /// </summary>
    [Serializable]
    public struct ResearchContact
    {
        /// <summary>
        /// Contact name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Academic title.
        /// </summary>
        public string Title;
        
        /// <summary>
        /// Department or division.
        /// </summary>
        public string Department;
        
        /// <summary>
        /// Email address.
        /// </summary>
        public string Email;
        
        /// <summary>
        /// Phone number.
        /// </summary>
        public string Phone;
        
        /// <summary>
        /// Office address.
        /// </summary>
        public string OfficeAddress;
        
        /// <summary>
        /// Research specializations.
        /// </summary>
        public string[] Specializations;
        
        /// <summary>
        /// ORCID identifier (if available).
        /// </summary>
        public string ORCIDId;
    }
    
    /// <summary>
    /// IRB contact information.
    /// </summary>
    [Serializable]
    public struct IRBContact
    {
        /// <summary>
        /// IRB office name.
        /// </summary>
        public string OfficeName;
        
        /// <summary>
        /// IRB coordinator name.
        /// </summary>
        public string CoordinatorName;
        
        /// <summary>
        /// Contact email.
        /// </summary>
        public string Email;
        
        /// <summary>
        /// Contact phone.
        /// </summary>
        public string Phone;
        
        /// <summary>
        /// IRB office address.
        /// </summary>
        public string Address;
        
        /// <summary>
        /// IRB submission portal URL.
        /// </summary>
        public string SubmissionPortalUrl;
        
        /// <summary>
        /// IRB meeting schedule.
        /// </summary>
        public string MeetingSchedule;
        
        /// <summary>
        /// Review timeline information.
        /// </summary>
        public string ReviewTimeline;
    }
    
    /// <summary>
    /// Institution accreditation information.
    /// </summary>
    [Serializable]
    public struct AccreditationInfo
    {
        /// <summary>
        /// Accrediting body name.
        /// </summary>
        public string AccreditingBody;
        
        /// <summary>
        /// Accreditation type.
        /// </summary>
        public string AccreditationType;
        
        /// <summary>
        /// Accreditation status.
        /// </summary>
        public AccreditationStatus Status;
        
        /// <summary>
        /// Accreditation date.
        /// </summary>
        public DateTime AccreditationDate;
        
        /// <summary>
        /// Expiration date.
        /// </summary>
        public DateTime ExpirationDate;
        
        /// <summary>
        /// Accreditation certificate number.
        /// </summary>
        public string CertificateNumber;
    }
    
    /// <summary>
    /// Principal investigator information.
    /// </summary>
    [Serializable]
    public struct PrincipalInvestigator
    {
        /// <summary>
        /// PI name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Academic credentials.
        /// </summary>
        public string Credentials;
        
        /// <summary>
        /// Institution affiliation.
        /// </summary>
        public string Institution;
        
        /// <summary>
        /// Department.
        /// </summary>
        public string Department;
        
        /// <summary>
        /// Contact information.
        /// </summary>
        public ContactInformation ContactInfo;
        
        /// <summary>
        /// Research experience summary.
        /// </summary>
        public string ResearchExperience;
        
        /// <summary>
        /// Previous IRB approvals.
        /// </summary>
        public string[] PreviousIRBApprovals;
        
        /// <summary>
        /// Training certifications.
        /// </summary>
        public TrainingCertification[] TrainingCertifications;
    }
    
    /// <summary>
    /// Co-investigator information.
    /// </summary>
    [Serializable]
    public struct CoInvestigator
    {
        /// <summary>
        /// Co-investigator name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Role in the study.
        /// </summary>
        public string Role;
        
        /// <summary>
        /// Institution affiliation.
        /// </summary>
        public string Institution;
        
        /// <summary>
        /// Contact information.
        /// </summary>
        public ContactInformation ContactInfo;
        
        /// <summary>
        /// Relevant expertise.
        /// </summary>
        public string[] Expertise;
        
        /// <summary>
        /// Training certifications.
        /// </summary>
        public TrainingCertification[] TrainingCertifications;
    }
    
    /// <summary>
    /// Training certification information.
    /// </summary>
    [Serializable]
    public struct TrainingCertification
    {
        /// <summary>
        /// Training program name.
        /// </summary>
        public string ProgramName;
        
        /// <summary>
        /// Certification type.
        /// </summary>
        public string CertificationType;
        
        /// <summary>
        /// Completion date.
        /// </summary>
        public DateTime CompletionDate;
        
        /// <summary>
        /// Expiration date (if applicable).
        /// </summary>
        public DateTime? ExpirationDate;
        
        /// <summary>
        /// Certificate number.
        /// </summary>
        public string CertificateNumber;
        
        /// <summary>
        /// Issuing organization.
        /// </summary>
        public string IssuingOrganization;
    }
    
    /// <summary>
    /// Participant population details.
    /// </summary>
    [Serializable]
    public struct ParticipantPopulation
    {
        /// <summary>
        /// Target sample size.
        /// </summary>
        public int TargetSampleSize;
        
        /// <summary>
        /// Age range criteria.
        /// </summary>
        public AgeRangeCriteria AgeCriteria;
        
        /// <summary>
        /// Gender distribution.
        /// </summary>
        public GenderDistribution GenderDistribution;
        
        /// <summary>
        /// Inclusion criteria.
        /// </summary>
        public string[] InclusionCriteria;
        
        /// <summary>
        /// Exclusion criteria.
        /// </summary>
        public string[] ExclusionCriteria;
        
        /// <summary>
        /// Vulnerable populations involved.
        /// </summary>
        public VulnerablePopulation[] VulnerablePopulations;
        
        /// <summary>
        /// Recruitment methods.
        /// </summary>
        public string[] RecruitmentMethods;
        
        /// <summary>
        /// Compensation details.
        /// </summary>
        public CompensationDetails Compensation;
    }
    
    /// <summary>
    /// Age range criteria.
    /// </summary>
    [Serializable]
    public struct AgeRangeCriteria
    {
        /// <summary>
        /// Minimum age.
        /// </summary>
        public int MinimumAge;
        
        /// <summary>
        /// Maximum age.
        /// </summary>
        public int MaximumAge;
        
        /// <summary>
        /// Whether minors are included.
        /// </summary>
        public bool IncludesMinors;
        
        /// <summary>
        /// Special considerations for age groups.
        /// </summary>
        public string[] AgeGroupConsiderations;
    }
    
    /// <summary>
    /// Gender distribution information.
    /// </summary>
    [Serializable]
    public struct GenderDistribution
    {
        /// <summary>
        /// Target percentage of male participants.
        /// </summary>
        public float MalePercentage;
        
        /// <summary>
        /// Target percentage of female participants.
        /// </summary>
        public float FemalePercentage;
        
        /// <summary>
        /// Target percentage of non-binary participants.
        /// </summary>
        public float NonBinaryPercentage;
        
        /// <summary>
        /// Whether gender is a study variable.
        /// </summary>
        public bool GenderIsStudyVariable;
        
        /// <summary>
        /// Gender-related considerations.
        /// </summary>
        public string[] GenderConsiderations;
    }
    
    /// <summary>
    /// Vulnerable population information.
    /// </summary>
    [Serializable]
    public struct VulnerablePopulation
    {
        /// <summary>
        /// Population type.
        /// </summary>
        public VulnerablePopulationType PopulationType;
        
        /// <summary>
        /// Special protections required.
        /// </summary>
        public string[] SpecialProtections;
        
        /// <summary>
        /// Additional consent requirements.
        /// </summary>
        public string[] AdditionalConsentRequirements;
        
        /// <summary>
        /// Risk mitigation strategies.
        /// </summary>
        public string[] RiskMitigationStrategies;
    }
    
    /// <summary>
    /// Compensation details.
    /// </summary>
    [Serializable]
    public struct CompensationDetails
    {
        /// <summary>
        /// Compensation amount.
        /// </summary>
        public decimal CompensationAmount;
        
        /// <summary>
        /// Compensation currency.
        /// </summary>
        public string Currency;
        
        /// <summary>
        /// Compensation method.
        /// </summary>
        public CompensationMethod Method;
        
        /// <summary>
        /// Compensation justification.
        /// </summary>
        public string Justification;
        
        /// <summary>
        /// Payment schedule.
        /// </summary>
        public string PaymentSchedule;
    }
    
    /// <summary>
    /// Risk assessment information.
    /// </summary>
    [Serializable]
    public struct RiskAssessment
    {
        /// <summary>
        /// Overall risk level.
        /// </summary>
        public RiskLevel OverallRiskLevel;
        
        /// <summary>
        /// Identified risks.
        /// </summary>
        public IdentifiedRisk[] IdentifiedRisks;
        
        /// <summary>
        /// Risk mitigation strategies.
        /// </summary>
        public RiskMitigationStrategy[] MitigationStrategies;
        
        /// <summary>
        /// Emergency procedures.
        /// </summary>
        public EmergencyProcedure[] EmergencyProcedures;
        
        /// <summary>
        /// Risk monitoring plan.
        /// </summary>
        public string RiskMonitoringPlan;
        
        /// <summary>
        /// Adverse event reporting procedures.
        /// </summary>
        public string AdverseEventReporting;
    }
    
    /// <summary>
    /// Identified risk information.
    /// </summary>
    [Serializable]
    public struct IdentifiedRisk
    {
        /// <summary>
        /// Risk identifier.
        /// </summary>
        public string RiskId;
        
        /// <summary>
        /// Risk description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Risk category.
        /// </summary>
        public RiskCategory Category;
        
        /// <summary>
        /// Risk probability.
        /// </summary>
        public RiskProbability Probability;
        
        /// <summary>
        /// Risk severity.
        /// </summary>
        public RiskSeverity Severity;
        
        /// <summary>
        /// Affected populations.
        /// </summary>
        public string[] AffectedPopulations;
    }
    
    /// <summary>
    /// Risk mitigation strategy.
    /// </summary>
    [Serializable]
    public struct RiskMitigationStrategy
    {
        /// <summary>
        /// Strategy identifier.
        /// </summary>
        public string StrategyId;
        
        /// <summary>
        /// Associated risk identifier.
        /// </summary>
        public string RiskId;
        
        /// <summary>
        /// Mitigation description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Implementation timeline.
        /// </summary>
        public string ImplementationTimeline;
        
        /// <summary>
        /// Responsible parties.
        /// </summary>
        public string[] ResponsibleParties;
        
        /// <summary>
        /// Effectiveness monitoring.
        /// </summary>
        public string EffectivenessMonitoring;
    }
    
    /// <summary>
    /// Emergency procedure information.
    /// </summary>
    [Serializable]
    public struct EmergencyProcedure
    {
        /// <summary>
        /// Procedure identifier.
        /// </summary>
        public string ProcedureId;
        
        /// <summary>
        /// Emergency type.
        /// </summary>
        public EmergencyType EmergencyType;
        
        /// <summary>
        /// Procedure steps.
        /// </summary>
        public string[] ProcedureSteps;
        
        /// <summary>
        /// Emergency contacts.
        /// </summary>
        public EmergencyContact[] EmergencyContacts;
        
        /// <summary>
        /// Required equipment or resources.
        /// </summary>
        public string[] RequiredResources;
    }
    
    /// <summary>
    /// Emergency contact information.
    /// </summary>
    [Serializable]
    public struct EmergencyContact
    {
        /// <summary>
        /// Contact name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Contact role.
        /// </summary>
        public string Role;
        
        /// <summary>
        /// Primary phone number.
        /// </summary>
        public string PrimaryPhone;
        
        /// <summary>
        /// Secondary phone number.
        /// </summary>
        public string SecondaryPhone;
        
        /// <summary>
        /// Email address.
        /// </summary>
        public string Email;
        
        /// <summary>
        /// Availability hours.
        /// </summary>
        public string AvailabilityHours;
    }
    
    /// <summary>
    /// Informed consent procedures.
    /// </summary>
    [Serializable]
    public struct InformedConsentProcedures
    {
        /// <summary>
        /// Consent process description.
        /// </summary>
        public string ConsentProcess;
        
        /// <summary>
        /// Consent form elements.
        /// </summary>
        public ConsentFormElement[] ConsentFormElements;
        
        /// <summary>
        /// Consent documentation method.
        /// </summary>
        public ConsentDocumentationMethod DocumentationMethod;
        
        /// <summary>
        /// Special consent considerations.
        /// </summary>
        public string[] SpecialConsiderations;
        
        /// <summary>
        /// Withdrawal procedures.
        /// </summary>
        public string WithdrawalProcedures;
        
        /// <summary>
        /// Consent language requirements.
        /// </summary>
        public string[] LanguageRequirements;
    }
    
    /// <summary>
    /// Consent form element.
    /// </summary>
    [Serializable]
    public struct ConsentFormElement
    {
        /// <summary>
        /// Element type.
        /// </summary>
        public ConsentElementType ElementType;
        
        /// <summary>
        /// Element description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Whether element is required.
        /// </summary>
        public bool IsRequired;
        
        /// <summary>
        /// Element content.
        /// </summary>
        public string Content;
    }
    
    /// <summary>
    /// Data management plan.
    /// </summary>
    [Serializable]
    public struct DataManagementPlan
    {
        /// <summary>
        /// Data collection methods.
        /// </summary>
        public string[] DataCollectionMethods;
        
        /// <summary>
        /// Data storage procedures.
        /// </summary>
        public DataStorageProcedures StorageProcedures;
        
        /// <summary>
        /// Data security measures.
        /// </summary>
        public DataSecurityMeasures SecurityMeasures;
        
        /// <summary>
        /// Data sharing plan.
        /// </summary>
        public DataSharingPlan SharingPlan;
        
        /// <summary>
        /// Data retention schedule.
        /// </summary>
        public DataRetentionSchedule RetentionSchedule;
        
        /// <summary>
        /// Data destruction procedures.
        /// </summary>
        public string DataDestructionProcedures;
    }
    
    /// <summary>
    /// Data storage procedures.
    /// </summary>
    [Serializable]
    public struct DataStorageProcedures
    {
        /// <summary>
        /// Storage location description.
        /// </summary>
        public string StorageLocation;
        
        /// <summary>
        /// Storage medium.
        /// </summary>
        public StorageMedium StorageMedium;
        
        /// <summary>
        /// Access controls.
        /// </summary>
        public AccessControl[] AccessControls;
        
        /// <summary>
        /// Backup procedures.
        /// </summary>
        public string BackupProcedures;
        
        /// <summary>
        /// Version control procedures.
        /// </summary>
        public string VersionControlProcedures;
    }
    
    /// <summary>
    /// Data security measures.
    /// </summary>
    [Serializable]
    public struct DataSecurityMeasures
    {
        /// <summary>
        /// Encryption requirements.
        /// </summary>
        public EncryptionRequirements EncryptionRequirements;
        
        /// <summary>
        /// Access authentication methods.
        /// </summary>
        public AuthenticationMethod[] AuthenticationMethods;
        
        /// <summary>
        /// Network security measures.
        /// </summary>
        public string[] NetworkSecurityMeasures;
        
        /// <summary>
        /// Physical security measures.
        /// </summary>
        public string[] PhysicalSecurityMeasures;
        
        /// <summary>
        /// Security monitoring procedures.
        /// </summary>
        public string SecurityMonitoringProcedures;
    }
    
    /// <summary>
    /// Data sharing plan.
    /// </summary>
    [Serializable]
    public struct DataSharingPlan
    {
        /// <summary>
        /// Whether data will be shared.
        /// </summary>
        public bool WillShareData;
        
        /// <summary>
        /// Data sharing timeline.
        /// </summary>
        public string SharingTimeline;
        
        /// <summary>
        /// Data sharing repositories.
        /// </summary>
        public string[] SharingRepositories;
        
        /// <summary>
        /// Data sharing restrictions.
        /// </summary>
        public string[] SharingRestrictions;
        
        /// <summary>
        /// Data sharing agreements required.
        /// </summary>
        public string[] RequiredAgreements;
    }
    
    /// <summary>
    /// Data retention schedule.
    /// </summary>
    [Serializable]
    public struct DataRetentionSchedule
    {
        /// <summary>
        /// Retention period by data type.
        /// </summary>
        public Dictionary<DataType, TimeSpan> RetentionPeriods;
        
        /// <summary>
        /// Retention justification.
        /// </summary>
        public string RetentionJustification;
        
        /// <summary>
        /// Review schedule for retention decisions.
        /// </summary>
        public string ReviewSchedule;
        
        /// <summary>
        /// Legal or regulatory requirements.
        /// </summary>
        public string[] LegalRequirements;
    }
    
    // Result structures
    
    /// <summary>
    /// IRB submission result.
    /// </summary>
    [Serializable]
    public struct IRBSubmissionResult
    {
        /// <summary>
        /// Submission identifier.
        /// </summary>
        public string SubmissionId;
        
        /// <summary>
        /// Whether submission was successful.
        /// </summary>
        public bool IsSuccessful;
        
        /// <summary>
        /// IRB tracking number.
        /// </summary>
        public string IRBTrackingNumber;
        
        /// <summary>
        /// Submission timestamp.
        /// </summary>
        public DateTime SubmissionTimestamp;
        
        /// <summary>
        /// Expected review timeline.
        /// </summary>
        public string ExpectedReviewTimeline;
        
        /// <summary>
        /// Submission status.
        /// </summary>
        public IRBSubmissionStatus Status;
        
        /// <summary>
        /// Submission messages.
        /// </summary>
        public string[] SubmissionMessages;
        
        /// <summary>
        /// Required follow-up actions.
        /// </summary>
        public string[] RequiredActions;
    }
    
    /// <summary>
    /// Partnership compliance result.
    /// </summary>
    [Serializable]
    public struct PartnershipComplianceResult
    {
        /// <summary>
        /// Whether partnership is compliant.
        /// </summary>
        public bool IsCompliant;
        
        /// <summary>
        /// Compliance score (0-100).
        /// </summary>
        public float ComplianceScore;
        
        /// <summary>
        /// Compliance check timestamp.
        /// </summary>
        public DateTime CheckTimestamp;
        
        /// <summary>
        /// Compliance issues identified.
        /// </summary>
        public ComplianceIssue[] ComplianceIssues;
        
        /// <summary>
        /// Required corrective actions.
        /// </summary>
        public string[] CorrectiveActions;
        
        /// <summary>
        /// Next compliance review date.
        /// </summary>
        public DateTime NextReviewDate;
    }
    
    /// <summary>
    /// Compliance issue information.
    /// </summary>
    [Serializable]
    public struct ComplianceIssue
    {
        /// <summary>
        /// Issue identifier.
        /// </summary>
        public string IssueId;
        
        /// <summary>
        /// Issue description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Issue severity.
        /// </summary>
        public ComplianceIssueSeverity Severity;
        
        /// <summary>
        /// Affected compliance area.
        /// </summary>
        public string ComplianceArea;
        
        /// <summary>
        /// Required resolution timeline.
        /// </summary>
        public TimeSpan ResolutionTimeline;
        
        /// <summary>
        /// Recommended resolution actions.
        /// </summary>
        public string[] RecommendedActions;
    }
    
    /// <summary>
    /// IRB approval status information.
    /// </summary>
    [Serializable]
    public struct IRBApprovalStatus
    {
        /// <summary>
        /// Current approval status.
        /// </summary>
        public IRBStatus Status;
        
        /// <summary>
        /// IRB approval number (if approved).
        /// </summary>
        public string ApprovalNumber;
        
        /// <summary>
        /// Approval date.
        /// </summary>
        public DateTime? ApprovalDate;
        
        /// <summary>
        /// Approval expiration date.
        /// </summary>
        public DateTime? ExpirationDate;
        
        /// <summary>
        /// Approval conditions.
        /// </summary>
        public string[] ApprovalConditions;
        
        /// <summary>
        /// Status update timestamp.
        /// </summary>
        public DateTime StatusUpdateTimestamp;
        
        /// <summary>
        /// Status notes.
        /// </summary>
        public string StatusNotes;
    }
    
    /// <summary>
    /// Data sharing result.
    /// </summary>
    [Serializable]
    public struct DataSharingResult
    {
        /// <summary>
        /// Whether data sharing was successful.
        /// </summary>
        public bool IsSuccessful;
        
        /// <summary>
        /// Data sharing transaction identifier.
        /// </summary>
        public string TransactionId;
        
        /// <summary>
        /// Data sharing timestamp.
        /// </summary>
        public DateTime SharingTimestamp;
        
        /// <summary>
        /// Shared data package identifier.
        /// </summary>
        public string DataPackageId;
        
        /// <summary>
        /// Data sharing compliance verification.
        /// </summary>
        public ComplianceVerification ComplianceVerification;
        
        /// <summary>
        /// Data sharing messages.
        /// </summary>
        public string[] SharingMessages;
        
        /// <summary>
        /// Any sharing errors encountered.
        /// </summary>
        public string[] SharingErrors;
    }
    
    /// <summary>
    /// Collaborative analysis result.
    /// </summary>
    [Serializable]
    public struct CollaborativeAnalysisResult
    {
        /// <summary>
        /// Analysis result identifier.
        /// </summary>
        public string ResultId;
        
        /// <summary>
        /// Analysis request identifier.
        /// </summary>
        public string RequestId;
        
        /// <summary>
        /// Analysis completion status.
        /// </summary>
        public AnalysisCompletionStatus CompletionStatus;
        
        /// <summary>
        /// Analysis deliverables.
        /// </summary>
        public AnalysisDeliverable[] Deliverables;
        
        /// <summary>
        /// Analysis completion timestamp.
        /// </summary>
        public DateTime CompletionTimestamp;
        
        /// <summary>
        /// Analysis quality assessment.
        /// </summary>
        public AnalysisQualityAssessment QualityAssessment;
        
        /// <summary>
        /// Analysis notes and interpretations.
        /// </summary>
        public string AnalysisNotes;
        
        /// <summary>
        /// Follow-up recommendations.
        /// </summary>
        public string[] FollowUpRecommendations;
    }
    
    // Additional enums and supporting structures
    
    public enum AccreditationStatus
    {
        Active,
        Expired,
        Suspended,
        Revoked,
        Pending
    }
    
    public enum VulnerablePopulationType
    {
        Minors,
        Pregnant,
        Prisoners,
        MentallyImpaired,
        Economically,
        Educationally,
        Other
    }
    
    public enum CompensationMethod
    {
        Cash,
        Check,
        GiftCard,
        CourseCredit,
        Other
    }
    
    public enum RiskLevel
    {
        Minimal,
        Low,
        Moderate,
        High,
        Extreme
    }
    
    public enum RiskCategory
    {
        Physical,
        Psychological,
        Social,
        Economic,
        Legal,
        Privacy,
        Other
    }
    
    public enum RiskProbability
    {
        VeryLow,
        Low,
        Moderate,
        High,
        VeryHigh
    }
    
    public enum RiskSeverity
    {
        Negligible,
        Minor,
        Moderate,
        Major,
        Catastrophic
    }
    
    public enum EmergencyType
    {
        Medical,
        Psychological,
        Technical,
        Security,
        Environmental,
        Other
    }
    
    public enum ConsentDocumentationMethod
    {
        WrittenSignature,
        ElectronicSignature,
        VerbalConsent,
        ImpliedConsent,
        Other
    }
    
    public enum ConsentElementType
    {
        StudyPurpose,
        Procedures,
        RisksAndBenefits,
        Confidentiality,
        Compensation,
        Withdrawal,
        ContactInformation,
        Other
    }
    
    public enum StorageMedium
    {
        LocalDisk,
        NetworkStorage,
        CloudStorage,
        PhysicalMedia,
        Database,
        Other
    }
    
    public enum AuthenticationMethod
    {
        Password,
        TwoFactor,
        Biometric,
        Certificate,
        Token,
        Other
    }
    
    public enum IRBSubmissionStatus
    {
        Submitted,
        UnderReview,
        RequiresRevision,
        Approved,
        Rejected,
        Withdrawn
    }
    
    public enum IRBStatus
    {
        NotSubmitted,
        Submitted,
        UnderReview,
        RequiresRevision,
        Approved,
        Rejected,
        Expired,
        Suspended
    }
    
    public enum ComplianceIssueSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum AnalysisCompletionStatus
    {
        InProgress,
        Completed,
        PartiallyCompleted,
        Failed,
        Cancelled
    }
    
    // Additional supporting structures
    
    /// <summary>
    /// Compliance verification information.
    /// </summary>
    [Serializable]
    public struct ComplianceVerification
    {
        /// <summary>
        /// Verification timestamp.
        /// </summary>
        public DateTime VerificationTimestamp;
        
        /// <summary>
        /// Verification method.
        /// </summary>
        public string VerificationMethod;
        
        /// <summary>
        /// Compliance standards checked.
        /// </summary>
        public string[] ComplianceStandards;
        
        /// <summary>
        /// Verification result.
        /// </summary>
        public bool IsCompliant;
        
        /// <summary>
        /// Verification notes.
        /// </summary>
        public string VerificationNotes;
    }
    
    /// <summary>
    /// Analysis deliverable information.
    /// </summary>
    [Serializable]
    public struct AnalysisDeliverable
    {
        /// <summary>
        /// Deliverable identifier.
        /// </summary>
        public string DeliverableId;
        
        /// <summary>
        /// Deliverable type.
        /// </summary>
        public string DeliverableType;
        
        /// <summary>
        /// Deliverable description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// File path or location.
        /// </summary>
        public string FilePath;
        
        /// <summary>
        /// Deliverable format.
        /// </summary>
        public string Format;
        
        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime CreationTimestamp;
        
        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long FileSizeBytes;
    }
    
    /// <summary>
    /// Analysis quality assessment.
    /// </summary>
    [Serializable]
    public struct AnalysisQualityAssessment
    {
        /// <summary>
        /// Overall quality score (0-100).
        /// </summary>
        public float OverallQualityScore;
        
        /// <summary>
        /// Methodological rigor score.
        /// </summary>
        public float MethodologicalRigorScore;
        
        /// <summary>
        /// Data quality score.
        /// </summary>
        public float DataQualityScore;
        
        /// <summary>
        /// Interpretation accuracy score.
        /// </summary>
        public float InterpretationAccuracyScore;
        
        /// <summary>
        /// Quality assessment notes.
        /// </summary>
        public string QualityNotes;
        
        /// <summary>
        /// Quality assessment timestamp.
        /// </summary>
        public DateTime AssessmentTimestamp;
    }
    
    // Event argument classes
    
    /// <summary>
    /// Event arguments for partnership established.
    /// </summary>
    public class PartnershipEstablishedEventArgs : EventArgs
    {
        /// <summary>
        /// Established partnership information.
        /// </summary>
        public AcademicPartnership Partnership { get; set; }
        
        /// <summary>
        /// Partnership establishment timestamp.
        /// </summary>
        public DateTime EstablishmentTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for IRB approval received.
    /// </summary>
    public class IRBApprovalReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Partnership identifier.
        /// </summary>
        public string PartnershipId { get; set; }
        
        /// <summary>
        /// IRB approval information.
        /// </summary>
        public IRBApprovalInfo ApprovalInfo { get; set; }
        
        /// <summary>
        /// Approval received timestamp.
        /// </summary>
        public DateTime ApprovalTimestamp { get; set; }
    }
    
    /// <summary>
    /// Event arguments for data sharing agreement signed.
    /// </summary>
    public class DataSharingAgreementSignedEventArgs : EventArgs
    {
        /// <summary>
        /// Partnership identifier.
        /// </summary>
        public string PartnershipId { get; set; }
        
        /// <summary>
        /// Data sharing agreement information.
        /// </summary>
        public DataSharingAgreement Agreement { get; set; }
        
        /// <summary>
        /// Agreement signing timestamp.
        /// </summary>
        public DateTime SigningTimestamp { get; set; }
    }
} 
   
    // Additional missing data structures
    
    /// <summary>
    /// Partnership performance metrics.
    /// </summary>
    [Serializable]
    public struct PartnershipPerformanceMetrics
    {
        /// <summary>
        /// Partnership establishment date.
        /// </summary>
        public DateTime EstablishmentDate;
        
        /// <summary>
        /// Number of collaborative projects.
        /// </summary>
        public int CollaborationsCount;
        
        /// <summary>
        /// Number of data sharing transactions.
        /// </summary>
        public int DataSharingTransactions;
        
        /// <summary>
        /// Number of joint publications.
        /// </summary>
        public int PublicationsCount;
        
        /// <summary>
        /// Overall satisfaction score (0-100).
        /// </summary>
        public float OverallSatisfactionScore;
        
        /// <summary>
        /// Average response time for requests in days.
        /// </summary>
        public float AverageResponseTimeDays;
        
        /// <summary>
        /// Partnership efficiency rating.
        /// </summary>
        public float EfficiencyRating;
    }
    
    /// <summary>
    /// Agreement signatory information.
    /// </summary>
    [Serializable]
    public struct AgreementSignatory
    {
        /// <summary>
        /// Signatory name.
        /// </summary>
        public string SignatoryName;
        
        /// <summary>
        /// Signatory title.
        /// </summary>
        public string SignatoryTitle;
        
        /// <summary>
        /// Organization name.
        /// </summary>
        public string Organization;
        
        /// <summary>
        /// Signing date.
        /// </summary>
        public DateTime SigningDate;
        
        /// <summary>
        /// Signature method (electronic, physical, etc.).
        /// </summary>
        public string SignatureMethod;
        
        /// <summary>
        /// Signature verification status.
        /// </summary>
        public bool IsVerified;
    }
    
    /// <summary>
    /// Compliance tracking information.
    /// </summary>
    [Serializable]
    public struct ComplianceTracking
    {
        /// <summary>
        /// Last compliance check date.
        /// </summary>
        public DateTime LastComplianceCheck;
        
        /// <summary>
        /// Current compliance status.
        /// </summary>
        public string ComplianceStatus;
        
        /// <summary>
        /// Compliance score (0-100).
        /// </summary>
        public float ComplianceScore;
        
        /// <summary>
        /// Compliance notes.
        /// </summary>
        public string ComplianceNotes;
        
        /// <summary>
        /// Next scheduled compliance review.
        /// </summary>
        public DateTime NextComplianceReview;
    }
    
    /// <summary>
    /// Agreement amendment information.
    /// </summary>
    [Serializable]
    public struct AgreementAmendment
    {
        /// <summary>
        /// Amendment identifier.
        /// </summary>
        public string AmendmentId;
        
        /// <summary>
        /// Amendment date.
        /// </summary>
        public DateTime AmendmentDate;
        
        /// <summary>
        /// Amendment description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Amended clauses.
        /// </summary>
        public string[] AmendedClauses;
        
        /// <summary>
        /// Amendment reason.
        /// </summary>
        public string Reason;
        
        /// <summary>
        /// Amendment approval status.
        /// </summary>
        public bool IsApproved;
    }
    
    /// <summary>
    /// Data package content information.
    /// </summary>
    [Serializable]
    public struct DataPackageContent
    {
        /// <summary>
        /// Content identifier.
        /// </summary>
        public string ContentId;
        
        /// <summary>
        /// Data type.
        /// </summary>
        public DataType DataType;
        
        /// <summary>
        /// Content description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// File path or location.
        /// </summary>
        public string FilePath;
        
        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long FileSizeBytes;
        
        /// <summary>
        /// Content format.
        /// </summary>
        public string Format;
        
        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime CreationTimestamp;
    }
    
    /// <summary>
    /// Analysis timeline information.
    /// </summary>
    [Serializable]
    public struct AnalysisTimeline
    {
        /// <summary>
        /// Expected start date.
        /// </summary>
        public DateTime ExpectedStartDate;
        
        /// <summary>
        /// Expected completion date.
        /// </summary>
        public DateTime ExpectedCompletionDate;
        
        /// <summary>
        /// Key milestones.
        /// </summary>
        public AnalysisMilestone[] Milestones;
        
        /// <summary>
        /// Timeline flexibility in days.
        /// </summary>
        public int FlexibilityDays;
    }
    
    /// <summary>
    /// Analysis milestone information.
    /// </summary>
    [Serializable]
    public struct AnalysisMilestone
    {
        /// <summary>
        /// Milestone identifier.
        /// </summary>
        public string MilestoneId;
        
        /// <summary>
        /// Milestone description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Expected completion date.
        /// </summary>
        public DateTime ExpectedDate;
        
        /// <summary>
        /// Milestone dependencies.
        /// </summary>
        public string[] Dependencies;
        
        /// <summary>
        /// Milestone deliverables.
        /// </summary>
        public string[] Deliverables;
    }
    
    /// <summary>
    /// Resource requirements for analysis.
    /// </summary>
    [Serializable]
    public struct ResourceRequirements
    {
        /// <summary>
        /// Required personnel count.
        /// </summary>
        public int RequiredPersonnel;
        
        /// <summary>
        /// Required expertise areas.
        /// </summary>
        public string[] RequiredExpertise;
        
        /// <summary>
        /// Required software tools.
        /// </summary>
        public string[] RequiredSoftware;
        
        /// <summary>
        /// Required computational resources.
        /// </summary>
        public ComputationalResources ComputationalResources;
        
        /// <summary>
        /// Estimated cost.
        /// </summary>
        public decimal EstimatedCost;
        
        /// <summary>
        /// Cost currency.
        /// </summary>
        public string Currency;
    }
    
    /// <summary>
    /// Computational resource requirements.
    /// </summary>
    [Serializable]
    public struct ComputationalResources
    {
        /// <summary>
        /// Required CPU cores.
        /// </summary>
        public int RequiredCPUCores;
        
        /// <summary>
        /// Required RAM in GB.
        /// </summary>
        public int RequiredRAMGB;
        
        /// <summary>
        /// Required storage in GB.
        /// </summary>
        public int RequiredStorageGB;
        
        /// <summary>
        /// Whether GPU acceleration is required.
        /// </summary>
        public bool RequiresGPU;
        
        /// <summary>
        /// Estimated processing time in hours.
        /// </summary>
        public float EstimatedProcessingHours;
    }
    
    /// <summary>
    /// Termination clause information.
    /// </summary>
    [Serializable]
    public struct TerminationClause
    {
        /// <summary>
        /// Clause identifier.
        /// </summary>
        public string ClauseId;
        
        /// <summary>
        /// Termination condition.
        /// </summary>
        public string Condition;
        
        /// <summary>
        /// Notice period required.
        /// </summary>
        public TimeSpan NoticePeriod;
        
        /// <summary>
        /// Termination penalties.
        /// </summary>
        public string[] Penalties;
        
        /// <summary>
        /// Data handling requirements upon termination.
        /// </summary>
        public string[] DataHandlingRequirements;
    }
    
    /// <summary>
    /// Data access control information.
    /// </summary>
    [Serializable]
    public struct DataAccessControl
    {
        /// <summary>
        /// Access control type.
        /// </summary>
        public string AccessControlType;
        
        /// <summary>
        /// Authorized personnel.
        /// </summary>
        public string[] AuthorizedPersonnel;
        
        /// <summary>
        /// Access permissions.
        /// </summary>
        public string[] Permissions;
        
        /// <summary>
        /// Access restrictions.
        /// </summary>
        public string[] Restrictions;
        
        /// <summary>
        /// Access logging requirements.
        /// </summary>
        public bool RequireAccessLogging;
    }
    
    /// <summary>
    /// Audit requirements information.
    /// </summary>
    [Serializable]
    public struct AuditRequirements
    {
        /// <summary>
        /// Audit frequency.
        /// </summary>
        public string AuditFrequency;
        
        /// <summary>
        /// Audit scope.
        /// </summary>
        public string[] AuditScope;
        
        /// <summary>
        /// Required audit standards.
        /// </summary>
        public string[] AuditStandards;
        
        /// <summary>
        /// Audit documentation requirements.
        /// </summary>
        public string[] DocumentationRequirements;
        
        /// <summary>
        /// External audit requirements.
        /// </summary>
        public bool RequireExternalAudit;
    }
    
    /// <summary>
    /// Data access restriction information.
    /// </summary>
    [Serializable]
    public struct DataAccessRestriction
    {
        /// <summary>
        /// Restriction type.
        /// </summary>
        public string RestrictionType;
        
        /// <summary>
        /// Restriction description.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Affected data types.
        /// </summary>
        public DataType[] AffectedDataTypes;
        
        /// <summary>
        /// Restriction enforcement method.
        /// </summary>
        public string EnforcementMethod;
        
        /// <summary>
        /// Restriction exceptions.
        /// </summary>
        public string[] Exceptions;
    }
    
    /// <summary>
    /// Anonymization requirements.
    /// </summary>
    [Serializable]
    public struct AnonymizationRequirements
    {
        /// <summary>
        /// Whether anonymization is required.
        /// </summary>
        public bool RequireAnonymization;
        
        /// <summary>
        /// Anonymization level required.
        /// </summary>
        public DataAnonymizationLevel RequiredLevel;
        
        /// <summary>
        /// Anonymization methods to use.
        /// </summary>
        public string[] AnonymizationMethods;
        
        /// <summary>
        /// Data elements to anonymize.
        /// </summary>
        public string[] ElementsToAnonymize;
        
        /// <summary>
        /// Anonymization verification requirements.
        /// </summary>
        public bool RequireVerification;
    }
    
    /// <summary>
    /// Data security requirements.
    /// </summary>
    [Serializable]
    public struct DataSecurityRequirements
    {
        /// <summary>
        /// Whether encryption is required.
        /// </summary>
        public bool RequireEncryption;
        
        /// <summary>
        /// Required encryption standards.
        /// </summary>
        public string[] EncryptionStandards;
        
        /// <summary>
        /// Access control requirements.
        /// </summary>
        public DataAccessControl[] AccessControls;
        
        /// <summary>
        /// Security monitoring requirements.
        /// </summary>
        public string[] MonitoringRequirements;
        
        /// <summary>
        /// Incident response requirements.
        /// </summary>
        public string[] IncidentResponseRequirements;
    }
    
    /// <summary>
    /// Publication rights information.
    /// </summary>
    [Serializable]
    public struct PublicationRights
    {
        /// <summary>
        /// Publication approval requirements.
        /// </summary>
        public string ApprovalRequirements;
        
        /// <summary>
        /// Authorship guidelines.
        /// </summary>
        public string[] AuthorshipGuidelines;
        
        /// <summary>
        /// Publication timeline requirements.
        /// </summary>
        public string TimelineRequirements;
        
        /// <summary>
        /// Data citation requirements.
        /// </summary>
        public string[] CitationRequirements;
        
        /// <summary>
        /// Publication restrictions.
        /// </summary>
        public string[] PublicationRestrictions;
    }
    
    /// <summary>
    /// Access control information.
    /// </summary>
    [Serializable]
    public struct AccessControl
    {
        /// <summary>
        /// Access control identifier.
        /// </summary>
        public string AccessControlId;
        
        /// <summary>
        /// User or role with access.
        /// </summary>
        public string UserOrRole;
        
        /// <summary>
        /// Access permissions.
        /// </summary>
        public string[] Permissions;
        
        /// <summary>
        /// Access restrictions.
        /// </summary>
        public string[] Restrictions;
        
        /// <summary>
        /// Access expiration date.
        /// </summary>
        public DateTime? ExpirationDate;
    }
    
    /// <summary>
    /// Encryption requirements.
    /// </summary>
    [Serializable]
    public struct EncryptionRequirements
    {
        /// <summary>
        /// Required encryption algorithm.
        /// </summary>
        public string EncryptionAlgorithm;
        
        /// <summary>
        /// Required key length.
        /// </summary>
        public int KeyLength;
        
        /// <summary>
        /// Encryption mode.
        /// </summary>
        public string EncryptionMode;
        
        /// <summary>
        /// Key management requirements.
        /// </summary>
        public string[] KeyManagementRequirements;
        
        /// <summary>
        /// Encryption verification requirements.
        /// </summary>
        public bool RequireVerification;
    }
    
    /// <summary>
    /// Partnership documentation.
    /// </summary>
    [Serializable]
    public struct PartnershipDocumentation
    {
        /// <summary>
        /// Documentation identifier.
        /// </summary>
        public string DocumentationId;
        
        /// <summary>
        /// Partnership identifier.
        /// </summary>
        public string PartnershipId;
        
        /// <summary>
        /// Documentation content.
        /// </summary>
        public string DocumentationContent;
        
        /// <summary>
        /// Documentation type.
        /// </summary>
        public string DocumentationType;
        
        /// <summary>
        /// Documentation format.
        /// </summary>
        public string DocumentationFormat;
        
        /// <summary>
        /// Generation timestamp.
        /// </summary>
        public DateTime GenerationTimestamp;
        
        /// <summary>
        /// Documentation version.
        /// </summary>
        public string Version;
    }
    
    /// <summary>
    /// Partnership renewal terms.
    /// </summary>
    [Serializable]
    public struct PartnershipRenewalTerms
    {
        /// <summary>
        /// Renewal duration in months.
        /// </summary>
        public int RenewalDurationMonths;
        
        /// <summary>
        /// Updated partnership terms (optional).
        /// </summary>
        public PartnershipTermsAndConditions? UpdatedTerms;
        
        /// <summary>
        /// Renewal justification.
        /// </summary>
        public string RenewalJustification;
        
        /// <summary>
        /// Updated collaboration objectives.
        /// </summary>
        public string[] UpdatedObjectives;
        
        /// <summary>
        /// Renewal conditions.
        /// </summary>
        public string[] RenewalConditions;
    }
    
    /// <summary>
    /// Partnership renewal result.
    /// </summary>
    [Serializable]
    public struct PartnershipRenewalResult
    {
        /// <summary>
        /// Whether renewal was successful.
        /// </summary>
        public bool IsSuccessful;
        
        /// <summary>
        /// Renewal identifier.
        /// </summary>
        public string RenewalId;
        
        /// <summary>
        /// New expiration date.
        /// </summary>
        public DateTime NewExpirationDate;
        
        /// <summary>
        /// Renewal messages.
        /// </summary>
        public string[] RenewalMessages;
        
        /// <summary>
        /// Renewal timestamp.
        /// </summary>
        public DateTime RenewalTimestamp;
        
        /// <summary>
        /// Renewal conditions.
        /// </summary>
        public string[] RenewalConditions;
    }
    
    /// <summary>
    /// Partnership termination result.
    /// </summary>
    [Serializable]
    public struct PartnershipTerminationResult
    {
        /// <summary>
        /// Whether termination was successful.
        /// </summary>
        public bool IsSuccessful;
        
        /// <summary>
        /// Termination identifier.
        /// </summary>
        public string TerminationId;
        
        /// <summary>
        /// Termination reason.
        /// </summary>
        public string TerminationReason;
        
        /// <summary>
        /// Data handling obligations.
        /// </summary>
        public string[] DataObligations;
        
        /// <summary>
        /// Termination messages.
        /// </summary>
        public string[] TerminationMessages;
        
        /// <summary>
        /// Termination timestamp.
        /// </summary>
        public DateTime TerminationTimestamp;
        
        /// <summary>
        /// Final settlement requirements.
        /// </summary>
        public string[] SettlementRequirements;
    }
}