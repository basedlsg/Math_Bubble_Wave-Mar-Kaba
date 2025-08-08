using System;
using System.Collections.Generic;

namespace XRBubbleLibrary.UserComfort
{
    /// <summary>
    /// Interface for academic partnership infrastructure management.
    /// Implements Requirement 7.2: Academic partnership for IRB approval and oversight.
    /// Implements Requirement 7.3: Data sharing and analysis agreements.
    /// </summary>
    public interface IAcademicPartnershipInfrastructure
    {
        /// <summary>
        /// Whether the academic partnership infrastructure is initialized.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Current partnership configuration.
        /// </summary>
        AcademicPartnershipConfiguration Configuration { get; }
        
        /// <summary>
        /// Current active partnerships.
        /// </summary>
        AcademicPartnership[] ActivePartnerships { get; }
        
        /// <summary>
        /// Event fired when a new partnership is established.
        /// </summary>
        event Action<PartnershipEstablishedEventArgs> PartnershipEstablished;
        
        /// <summary>
        /// Event fired when IRB approval is received.
        /// </summary>
        event Action<IRBApprovalReceivedEventArgs> IRBApprovalReceived;
        
        /// <summary>
        /// Event fired when data sharing agreement is signed.
        /// </summary>
        event Action<DataSharingAgreementSignedEventArgs> DataSharingAgreementSigned;
        
        /// <summary>
        /// Initialize the academic partnership infrastructure.
        /// </summary>
        /// <param name="config">Partnership configuration</param>
        /// <returns>True if initialization successful</returns>
        bool Initialize(AcademicPartnershipConfiguration config);
        
        /// <summary>
        /// Establish a new academic partnership.
        /// </summary>
        /// <param name="partnershipRequest">Partnership establishment request</param>
        /// <returns>Established partnership</returns>
        AcademicPartnership EstablishPartnership(PartnershipRequest partnershipRequest);
        
        /// <summary>
        /// Submit IRB application through academic partner.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <param name="irbApplication">IRB application details</param>
        /// <returns>IRB submission result</returns>
        IRBSubmissionResult SubmitIRBApplication(string partnershipId, IRBApplication irbApplication);
        
        /// <summary>
        /// Create data sharing agreement with academic partner.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <param name="agreementTerms">Data sharing agreement terms</param>
        /// <returns>Created data sharing agreement</returns>
        DataSharingAgreement CreateDataSharingAgreement(string partnershipId, DataSharingTerms agreementTerms);
        
        /// <summary>
        /// Validate partnership compliance with requirements.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <returns>Compliance validation result</returns>
        PartnershipComplianceResult ValidatePartnershipCompliance(string partnershipId);
        
        /// <summary>
        /// Get IRB approval status for a partnership.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <returns>IRB approval status</returns>
        IRBApprovalStatus GetIRBApprovalStatus(string partnershipId);
        
        /// <summary>
        /// Share study data with academic partner according to agreement.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <param name="studyData">Study data to share</param>
        /// <returns>Data sharing result</returns>
        DataSharingResult ShareStudyData(string partnershipId, StudyDataPackage studyData);
        
        /// <summary>
        /// Request collaborative analysis from academic partner.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <param name="analysisRequest">Analysis request details</param>
        /// <returns>Collaborative analysis result</returns>
        CollaborativeAnalysisResult RequestCollaborativeAnalysis(string partnershipId, AnalysisRequest analysisRequest);
        
        /// <summary>
        /// Generate partnership documentation and reports.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <returns>Partnership documentation</returns>
        PartnershipDocumentation GeneratePartnershipDocumentation(string partnershipId);
        
        /// <summary>
        /// Renew existing partnership agreement.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <param name="renewalTerms">Renewal terms</param>
        /// <returns>Partnership renewal result</returns>
        PartnershipRenewalResult RenewPartnership(string partnershipId, PartnershipRenewalTerms renewalTerms);
        
        /// <summary>
        /// Terminate partnership and handle data obligations.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <param name="terminationReason">Reason for termination</param>
        /// <returns>Partnership termination result</returns>
        PartnershipTerminationResult TerminatePartnership(string partnershipId, string terminationReason);
        
        /// <summary>
        /// Get partnership performance metrics and statistics.
        /// </summary>
        /// <param name="partnershipId">Partnership identifier</param>
        /// <returns>Partnership performance metrics</returns>
        PartnershipPerformanceMetrics GetPartnershipMetrics(string partnershipId);
        
        /// <summary>
        /// Reset partnership infrastructure state.
        /// </summary>
        void ResetPartnershipInfrastructure();
    }
    
    /// <summary>
    /// Configuration for academic partnership infrastructure.
    /// </summary>
    [Serializable]
    public struct AcademicPartnershipConfiguration
    {
        /// <summary>
        /// Organization name establishing partnerships.
        /// </summary>
        public string OrganizationName;
        
        /// <summary>
        /// Primary contact information.
        /// </summary>
        public ContactInformation PrimaryContact;
        
        /// <summary>
        /// Legal entity information.
        /// </summary>
        public LegalEntityInformation LegalEntity;
        
        /// <summary>
        /// Default partnership terms and conditions.
        /// </summary>
        public PartnershipTermsAndConditions DefaultTerms;
        
        /// <summary>
        /// Data protection and privacy requirements.
        /// </summary>
        public DataProtectionRequirements DataProtection;
        
        /// <summary>
        /// Whether to require IRB approval for all partnerships.
        /// </summary>
        public bool RequireIRBApproval;
        
        /// <summary>
        /// Whether to enable debug logging.
        /// </summary>
        public bool EnableDebugLogging;
        
        /// <summary>
        /// Partnership data storage directory.
        /// </summary>
        public string PartnershipDataDirectory;
        
        /// <summary>
        /// Default academic partnership configuration.
        /// </summary>
        public static AcademicPartnershipConfiguration Default => new AcademicPartnershipConfiguration
        {
            OrganizationName = "VR Research Organization",
            PrimaryContact = new ContactInformation
            {
                Name = "Research Director",
                Email = "research@organization.edu",
                Phone = "+1-555-0123",
                Address = "123 Research Ave, University City, ST 12345"
            },
            LegalEntity = new LegalEntityInformation
            {
                EntityName = "VR Research Organization",
                EntityType = LegalEntityType.Corporation,
                RegistrationNumber = "REG123456",
                TaxId = "TAX789012"
            },
            DefaultTerms = new PartnershipTermsAndConditions
            {
                PartnershipDurationMonths = 24,
                DataRetentionYears = 7,
                IntellectualPropertySharing = IntellectualPropertyPolicy.Shared,
                PublicationRights = PublicationRightsPolicy.JointApproval
            },
            DataProtection = new DataProtectionRequirements
            {
                RequireEncryption = true,
                RequireAnonymization = true,
                ComplianceStandards = new[] { "GDPR", "HIPAA", "FERPA" }
            },
            RequireIRBApproval = true,
            EnableDebugLogging = false,
            PartnershipDataDirectory = "PartnershipData"
        };
    }
    
    /// <summary>
    /// Academic partnership information.
    /// </summary>
    [Serializable]
    public struct AcademicPartnership
    {
        /// <summary>
        /// Unique partnership identifier.
        /// </summary>
        public string PartnershipId;
        
        /// <summary>
        /// Partner institution information.
        /// </summary>
        public AcademicInstitution PartnerInstitution;
        
        /// <summary>
        /// Partnership establishment date.
        /// </summary>
        public DateTime EstablishmentDate;
        
        /// <summary>
        /// Partnership expiration date.
        /// </summary>
        public DateTime ExpirationDate;
        
        /// <summary>
        /// Current partnership status.
        /// </summary>
        public PartnershipStatus Status;
        
        /// <summary>
        /// Partnership terms and conditions.
        /// </summary>
        public PartnershipTermsAndConditions Terms;
        
        /// <summary>
        /// IRB approval information (if applicable).
        /// </summary>
        public IRBApprovalInfo? IRBApproval;
        
        /// <summary>
        /// Data sharing agreement (if applicable).
        /// </summary>
        public DataSharingAgreement? DataSharingAgreement;
        
        /// <summary>
        /// Partnership performance metrics.
        /// </summary>
        public PartnershipPerformanceMetrics PerformanceMetrics;
        
        /// <summary>
        /// Partnership notes and history.
        /// </summary>
        public string[] PartnershipNotes;
    }
    
    /// <summary>
    /// Academic institution information.
    /// </summary>
    [Serializable]
    public struct AcademicInstitution
    {
        /// <summary>
        /// Institution name.
        /// </summary>
        public string InstitutionName;
        
        /// <summary>
        /// Institution type (university, research institute, etc.).
        /// </summary>
        public InstitutionType Type;
        
        /// <summary>
        /// Institution contact information.
        /// </summary>
        public ContactInformation ContactInfo;
        
        /// <summary>
        /// Primary research contact at institution.
        /// </summary>
        public ResearchContact PrimaryResearchContact;
        
        /// <summary>
        /// IRB contact information.
        /// </summary>
        public IRBContact IRBContact;
        
        /// <summary>
        /// Institution accreditation information.
        /// </summary>
        public AccreditationInfo[] Accreditations;
        
        /// <summary>
        /// Research areas and expertise.
        /// </summary>
        public string[] ResearchAreas;
        
        /// <summary>
        /// Institution website URL.
        /// </summary>
        public string WebsiteUrl;
    }
    
    /// <summary>
    /// Partnership establishment request.
    /// </summary>
    [Serializable]
    public struct PartnershipRequest
    {
        /// <summary>
        /// Requesting organization information.
        /// </summary>
        public string RequestingOrganization;
        
        /// <summary>
        /// Target academic institution.
        /// </summary>
        public AcademicInstitution TargetInstitution;
        
        /// <summary>
        /// Proposed partnership terms.
        /// </summary>
        public PartnershipTermsAndConditions ProposedTerms;
        
        /// <summary>
        /// Research collaboration objectives.
        /// </summary>
        public string[] CollaborationObjectives;
        
        /// <summary>
        /// Expected partnership duration in months.
        /// </summary>
        public int ExpectedDurationMonths;
        
        /// <summary>
        /// Whether IRB approval is required.
        /// </summary>
        public bool RequiresIRBApproval;
        
        /// <summary>
        /// Whether data sharing is involved.
        /// </summary>
        public bool InvolvesDataSharing;
        
        /// <summary>
        /// Request submission date.
        /// </summary>
        public DateTime RequestDate;
        
        /// <summary>
        /// Additional request notes.
        /// </summary>
        public string RequestNotes;
    }
    
    /// <summary>
    /// IRB application details.
    /// </summary>
    [Serializable]
    public struct IRBApplication
    {
        /// <summary>
        /// Application identifier.
        /// </summary>
        public string ApplicationId;
        
        /// <summary>
        /// Study protocol reference.
        /// </summary>
        public string StudyProtocolId;
        
        /// <summary>
        /// Principal investigator information.
        /// </summary>
        public PrincipalInvestigator PrincipalInvestigator;
        
        /// <summary>
        /// Co-investigators.
        /// </summary>
        public CoInvestigator[] CoInvestigators;
        
        /// <summary>
        /// Study title and description.
        /// </summary>
        public string StudyTitle;
        
        /// <summary>
        /// Study description.
        /// </summary>
        public string StudyDescription;
        
        /// <summary>
        /// Research objectives.
        /// </summary>
        public string[] ResearchObjectives;
        
        /// <summary>
        /// Participant population details.
        /// </summary>
        public ParticipantPopulation ParticipantPopulation;
        
        /// <summary>
        /// Risk assessment and mitigation.
        /// </summary>
        public RiskAssessment RiskAssessment;
        
        /// <summary>
        /// Informed consent procedures.
        /// </summary>
        public InformedConsentProcedures ConsentProcedures;
        
        /// <summary>
        /// Data management and privacy plan.
        /// </summary>
        public DataManagementPlan DataManagementPlan;
        
        /// <summary>
        /// Application submission date.
        /// </summary>
        public DateTime SubmissionDate;
        
        /// <summary>
        /// Expected study start date.
        /// </summary>
        public DateTime ExpectedStartDate;
        
        /// <summary>
        /// Expected study completion date.
        /// </summary>
        public DateTime ExpectedCompletionDate;
    }
    
    /// <summary>
    /// Data sharing agreement terms.
    /// </summary>
    [Serializable]
    public struct DataSharingTerms
    {
        /// <summary>
        /// Types of data to be shared.
        /// </summary>
        public DataType[] DataTypes;
        
        /// <summary>
        /// Data sharing purpose and scope.
        /// </summary>
        public string SharingPurpose;
        
        /// <summary>
        /// Data access restrictions.
        /// </summary>
        public DataAccessRestriction[] AccessRestrictions;
        
        /// <summary>
        /// Data retention period.
        /// </summary>
        public TimeSpan RetentionPeriod;
        
        /// <summary>
        /// Data anonymization requirements.
        /// </summary>
        public AnonymizationRequirements AnonymizationRequirements;
        
        /// <summary>
        /// Data security requirements.
        /// </summary>
        public DataSecurityRequirements SecurityRequirements;
        
        /// <summary>
        /// Publication and dissemination rights.
        /// </summary>
        public PublicationRights PublicationRights;
        
        /// <summary>
        /// Data sharing compliance requirements.
        /// </summary>
        public string[] ComplianceRequirements;
        
        /// <summary>
        /// Agreement effective date.
        /// </summary>
        public DateTime EffectiveDate;
        
        /// <summary>
        /// Agreement expiration date.
        /// </summary>
        public DateTime ExpirationDate;
    }
    
    /// <summary>
    /// Data sharing agreement.
    /// </summary>
    [Serializable]
    public struct DataSharingAgreement
    {
        /// <summary>
        /// Agreement identifier.
        /// </summary>
        public string AgreementId;
        
        /// <summary>
        /// Partnership identifier.
        /// </summary>
        public string PartnershipId;
        
        /// <summary>
        /// Agreement terms.
        /// </summary>
        public DataSharingTerms Terms;
        
        /// <summary>
        /// Agreement status.
        /// </summary>
        public AgreementStatus Status;
        
        /// <summary>
        /// Agreement signing date.
        /// </summary>
        public DateTime SigningDate;
        
        /// <summary>
        /// Signatory information.
        /// </summary>
        public AgreementSignatory[] Signatories;
        
        /// <summary>
        /// Agreement compliance tracking.
        /// </summary>
        public ComplianceTracking ComplianceTracking;
        
        /// <summary>
        /// Agreement amendments (if any).
        /// </summary>
        public AgreementAmendment[] Amendments;
    }
    
    /// <summary>
    /// Study data package for sharing.
    /// </summary>
    [Serializable]
    public struct StudyDataPackage
    {
        /// <summary>
        /// Data package identifier.
        /// </summary>
        public string PackageId;
        
        /// <summary>
        /// Study identifier.
        /// </summary>
        public string StudyId;
        
        /// <summary>
        /// Data package contents.
        /// </summary>
        public DataPackageContent[] Contents;
        
        /// <summary>
        /// Data anonymization level applied.
        /// </summary>
        public DataAnonymizationLevel AnonymizationLevel;
        
        /// <summary>
        /// Data encryption status.
        /// </summary>
        public bool IsEncrypted;
        
        /// <summary>
        /// Data integrity hash.
        /// </summary>
        public string IntegrityHash;
        
        /// <summary>
        /// Package creation date.
        /// </summary>
        public DateTime CreationDate;
        
        /// <summary>
        /// Package metadata.
        /// </summary>
        public Dictionary<string, string> Metadata;
        
        /// <summary>
        /// Data sharing compliance verification.
        /// </summary>
        public ComplianceVerification ComplianceVerification;
    }
    
    /// <summary>
    /// Collaborative analysis request.
    /// </summary>
    [Serializable]
    public struct AnalysisRequest
    {
        /// <summary>
        /// Analysis request identifier.
        /// </summary>
        public string RequestId;
        
        /// <summary>
        /// Analysis type requested.
        /// </summary>
        public AnalysisType AnalysisType;
        
        /// <summary>
        /// Analysis objectives.
        /// </summary>
        public string[] AnalysisObjectives;
        
        /// <summary>
        /// Data to be analyzed.
        /// </summary>
        public StudyDataPackage DataPackage;
        
        /// <summary>
        /// Specific analysis methods requested.
        /// </summary>
        public string[] RequestedMethods;
        
        /// <summary>
        /// Expected deliverables.
        /// </summary>
        public string[] ExpectedDeliverables;
        
        /// <summary>
        /// Analysis timeline.
        /// </summary>
        public AnalysisTimeline Timeline;
        
        /// <summary>
        /// Resource requirements.
        /// </summary>
        public ResourceRequirements ResourceRequirements;
        
        /// <summary>
        /// Request submission date.
        /// </summary>
        public DateTime RequestDate;
        
        /// <summary>
        /// Request priority level.
        /// </summary>
        public RequestPriority Priority;
    }
    
    // Supporting data structures and enums
    
    /// <summary>
    /// Contact information structure.
    /// </summary>
    [Serializable]
    public struct ContactInformation
    {
        public string Name;
        public string Email;
        public string Phone;
        public string Address;
        public string Department;
        public string Title;
    }
    
    /// <summary>
    /// Legal entity information.
    /// </summary>
    [Serializable]
    public struct LegalEntityInformation
    {
        public string EntityName;
        public LegalEntityType EntityType;
        public string RegistrationNumber;
        public string TaxId;
        public string RegisteredAddress;
        public string[] LegalRepresentatives;
    }
    
    /// <summary>
    /// Partnership terms and conditions.
    /// </summary>
    [Serializable]
    public struct PartnershipTermsAndConditions
    {
        public int PartnershipDurationMonths;
        public int DataRetentionYears;
        public IntellectualPropertyPolicy IntellectualPropertySharing;
        public PublicationRightsPolicy PublicationRights;
        public TerminationClause[] TerminationClauses;
        public DisputeResolutionMethod DisputeResolution;
        public string[] AdditionalTerms;
    }
    
    /// <summary>
    /// Data protection requirements.
    /// </summary>
    [Serializable]
    public struct DataProtectionRequirements
    {
        public bool RequireEncryption;
        public bool RequireAnonymization;
        public string[] ComplianceStandards;
        public DataAccessControl[] AccessControls;
        public AuditRequirements AuditRequirements;
    }
    
    // Enums for various categories
    
    public enum PartnershipStatus
    {
        Pending,
        Active,
        Suspended,
        Terminated,
        Expired,
        UnderReview
    }
    
    public enum InstitutionType
    {
        University,
        ResearchInstitute,
        Hospital,
        GovernmentLab,
        PrivateResearch,
        NonProfit
    }
    
    public enum LegalEntityType
    {
        Corporation,
        LLC,
        Partnership,
        NonProfit,
        Government,
        University
    }
    
    public enum IntellectualPropertyPolicy
    {
        Shared,
        Separate,
        JointOwnership,
        LicenseBased
    }
    
    public enum PublicationRightsPolicy
    {
        JointApproval,
        IndependentRights,
        PrimaryAuthorRights,
        InstitutionApproval
    }
    
    public enum AgreementStatus
    {
        Draft,
        UnderReview,
        Signed,
        Active,
        Expired,
        Terminated,
        Amended
    }
    
    public enum DataType
    {
        RawData,
        ProcessedData,
        AnalysisResults,
        Metadata,
        Documentation,
        Code,
        Publications
    }
    
    public enum AnalysisType
    {
        StatisticalAnalysis,
        DataVisualization,
        MachineLearning,
        QualitativeAnalysis,
        MetaAnalysis,
        SystematicReview
    }
    
    public enum RequestPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }
    
    public enum DisputeResolutionMethod
    {
        Negotiation,
        Mediation,
        Arbitration,
        Litigation
    }
    
    // Additional supporting structures would be defined in the implementation file
    // to keep this interface focused on the core contract
}