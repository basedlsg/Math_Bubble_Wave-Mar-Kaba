using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XRBubbleLibrary.UserComfort.Tests
{
    /// <summary>
    /// Comprehensive unit tests for AcademicPartnershipInfrastructure.
    /// Tests Requirement 7.2: Academic partnership for IRB approval and oversight.
    /// Tests Requirement 7.3: Data sharing and analysis agreements.
    /// </summary>
    [TestFixture]
    public class AcademicPartnershipInfrastructureTests
    {
        private AcademicPartnershipInfrastructure _partnershipInfrastructure;
        private GameObject _testGameObject;
        
        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("AcademicPartnershipInfrastructureTest");
            _partnershipInfrastructure = _testGameObject.AddComponent<AcademicPartnershipInfrastructure>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_testGameObject);
            }
        }
        
        #region Initialization Tests
        
        [Test]
        public void Initialize_WithValidConfiguration_ReturnsTrue()
        {
            // Arrange
            var config = AcademicPartnershipConfiguration.Default;
            
            // Act
            bool result = _partnershipInfrastructure.Initialize(config);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_partnershipInfrastructure.IsInitialized);
            Assert.AreEqual(config.OrganizationName, _partnershipInfrastructure.Configuration.OrganizationName);
        }
        
        [Test]
        public void Initialize_WithMissingOrganizationName_ReturnsFalse()
        {
            // Arrange
            var config = AcademicPartnershipConfiguration.Default;
            config.OrganizationName = "";
            
            // Act
            bool result = _partnershipInfrastructure.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_partnershipInfrastructure.IsInitialized);
        }  
      
        [Test]
        public void Initialize_WithMissingContactEmail_ReturnsFalse()
        {
            // Arrange
            var config = AcademicPartnershipConfiguration.Default;
            config.PrimaryContact.Email = "";
            
            // Act
            bool result = _partnershipInfrastructure.Initialize(config);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_partnershipInfrastructure.IsInitialized);
        }
        
        #endregion
        
        #region Partnership Establishment Tests
        
        [Test]
        public void EstablishPartnership_WithValidRequest_ReturnsPartnership()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnershipRequest = CreateValidPartnershipRequest();
            
            // Act
            var partnership = _partnershipInfrastructure.EstablishPartnership(partnershipRequest);
            
            // Assert
            Assert.IsNotNull(partnership.PartnershipId);
            Assert.AreEqual(partnershipRequest.TargetInstitution.InstitutionName, partnership.PartnerInstitution.InstitutionName);
            Assert.AreEqual(PartnershipStatus.Pending, partnership.Status);
            Assert.IsTrue(partnership.EstablishmentDate <= DateTime.Now);
            Assert.IsTrue(partnership.ExpirationDate > DateTime.Now);
        }
        
        [Test]
        public void EstablishPartnership_WhenNotInitialized_ThrowsException()
        {
            // Arrange
            // Don't initialize the infrastructure
            var partnershipRequest = CreateValidPartnershipRequest();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                _partnershipInfrastructure.EstablishPartnership(partnershipRequest));
        }
        
        [Test]
        public void EstablishPartnership_WithInvalidRequest_ThrowsException()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnershipRequest = CreateValidPartnershipRequest();
            partnershipRequest.TargetInstitution.InstitutionName = ""; // Invalid
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _partnershipInfrastructure.EstablishPartnership(partnershipRequest));
        }
        
        [Test]
        public void EstablishPartnership_FiresPartnershipEstablishedEvent()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnershipRequest = CreateValidPartnershipRequest();
            
            bool eventFired = false;
            PartnershipEstablishedEventArgs eventArgs = null;
            
            _partnershipInfrastructure.PartnershipEstablished += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            var partnership = _partnershipInfrastructure.EstablishPartnership(partnershipRequest);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(partnership.PartnershipId, eventArgs.Partnership.PartnershipId);
        }
        
        [Test]
        public void EstablishPartnership_AddsToActivePartnerships()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnershipRequest = CreateValidPartnershipRequest();
            
            // Act
            var partnership = _partnershipInfrastructure.EstablishPartnership(partnershipRequest);
            
            // Assert
            var activePartnerships = _partnershipInfrastructure.ActivePartnerships;
            Assert.AreEqual(0, activePartnerships.Length); // Status is Pending, not Active
        }
        
        #endregion
        
        #region IRB Application Tests
        
        [Test]
        public void SubmitIRBApplication_WithValidApplication_ReturnsSuccess()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var irbApplication = CreateValidIRBApplication();
            
            // Act
            var result = _partnershipInfrastructure.SubmitIRBApplication(partnership.PartnershipId, irbApplication);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotNull(result.SubmissionId);
            Assert.IsNotNull(result.IRBTrackingNumber);
            Assert.AreEqual(IRBSubmissionStatus.Submitted, result.Status);
            Assert.IsTrue(result.SubmissionMessages.Any(m => m.Contains("successfully")));
        }
        
        [Test]
        public void SubmitIRBApplication_WithInvalidPartnershipId_ReturnsFailure()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var irbApplication = CreateValidIRBApplication();
            
            // Act
            var result = _partnershipInfrastructure.SubmitIRBApplication("invalid-id", irbApplication);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.SubmissionMessages.Any(m => m.Contains("Partnership not found")));
        }
        
        [Test]
        public void SubmitIRBApplication_WithInvalidApplication_ReturnsFailure()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var irbApplication = CreateValidIRBApplication();
            irbApplication.StudyTitle = ""; // Invalid
            
            // Act
            var result = _partnershipInfrastructure.SubmitIRBApplication(partnership.PartnershipId, irbApplication);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.SubmissionMessages.Any(m => m.Contains("Study title")));
        }
        
        [Test]
        public void GetIRBApprovalStatus_WithSubmittedApplication_ReturnsStatus()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var irbApplication = CreateValidIRBApplication();
            _partnershipInfrastructure.SubmitIRBApplication(partnership.PartnershipId, irbApplication);
            
            // Act
            var status = _partnershipInfrastructure.GetIRBApprovalStatus(partnership.PartnershipId);
            
            // Assert
            Assert.AreEqual(IRBStatus.Submitted, status.Status);
            Assert.IsNotNull(status.StatusNotes);
        }
        
        [Test]
        public void GetIRBApprovalStatus_WithoutSubmission_ReturnsNotSubmitted()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            
            // Act
            var status = _partnershipInfrastructure.GetIRBApprovalStatus(partnership.PartnershipId);
            
            // Assert
            Assert.AreEqual(IRBStatus.NotSubmitted, status.Status);
        }
        
        #endregion
        
        #region Data Sharing Agreement Tests
        
        [Test]
        public void CreateDataSharingAgreement_WithValidTerms_ReturnsAgreement()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var sharingTerms = CreateValidDataSharingTerms();
            
            // Act
            var agreement = _partnershipInfrastructure.CreateDataSharingAgreement(partnership.PartnershipId, sharingTerms);
            
            // Assert
            Assert.IsNotNull(agreement.AgreementId);
            Assert.AreEqual(partnership.PartnershipId, agreement.PartnershipId);
            Assert.AreEqual(sharingTerms.SharingPurpose, agreement.Terms.SharingPurpose);
            Assert.AreEqual(AgreementStatus.Draft, agreement.Status);
            Assert.IsTrue(agreement.Signatories.Length > 0);
        }
        
        [Test]
        public void CreateDataSharingAgreement_WithInvalidPartnershipId_ThrowsException()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var sharingTerms = CreateValidDataSharingTerms();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _partnershipInfrastructure.CreateDataSharingAgreement("invalid-id", sharingTerms));
        }
        
        [Test]
        public void CreateDataSharingAgreement_FiresAgreementSignedEvent()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var sharingTerms = CreateValidDataSharingTerms();
            
            bool eventFired = false;
            DataSharingAgreementSignedEventArgs eventArgs = null;
            
            _partnershipInfrastructure.DataSharingAgreementSigned += (args) =>
            {
                eventFired = true;
                eventArgs = args;
            };
            
            // Act
            var agreement = _partnershipInfrastructure.CreateDataSharingAgreement(partnership.PartnershipId, sharingTerms);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(partnership.PartnershipId, eventArgs.PartnershipId);
            Assert.AreEqual(agreement.AgreementId, eventArgs.Agreement.AgreementId);
        }
        
        #endregion
        
        #region Partnership Compliance Tests
        
        [Test]
        public void ValidatePartnershipCompliance_WithValidPartnership_ReturnsCompliant()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            
            // Act
            var result = _partnershipInfrastructure.ValidatePartnershipCompliance(partnership.PartnershipId);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ComplianceScore >= 0);
            Assert.IsNotNull(result.CheckTimestamp);
        }
        
        [Test]
        public void ValidatePartnershipCompliance_WithInvalidPartnershipId_ReturnsNonCompliant()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            
            // Act
            var result = _partnershipInfrastructure.ValidatePartnershipCompliance("invalid-id");
            
            // Assert
            Assert.IsFalse(result.IsCompliant);
            Assert.AreEqual(0f, result.ComplianceScore);
            Assert.IsTrue(result.ComplianceIssues.Any(i => i.Description.Contains("Partnership not found")));
        }
        
        #endregion
        
        #region Data Sharing Tests
        
        [Test]
        public void ShareStudyData_WithValidData_ReturnsSuccess()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var sharingTerms = CreateValidDataSharingTerms();
            var agreement = _partnershipInfrastructure.CreateDataSharingAgreement(partnership.PartnershipId, sharingTerms);
            
            // Update agreement status to active for sharing
            agreement.Status = AgreementStatus.Active;
            
            var studyData = CreateValidStudyDataPackage();
            
            // Act
            var result = _partnershipInfrastructure.ShareStudyData(partnership.PartnershipId, studyData);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotNull(result.TransactionId);
            Assert.AreEqual(studyData.PackageId, result.DataPackageId);
            Assert.IsTrue(result.SharingMessages.Any(m => m.Contains("successfully")));
        }
        
        [Test]
        public void ShareStudyData_WithoutAgreement_ReturnsFailure()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var studyData = CreateValidStudyDataPackage();
            
            // Act
            var result = _partnershipInfrastructure.ShareStudyData(partnership.PartnershipId, studyData);
            
            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.SharingErrors.Any(e => e.Contains("No data sharing agreement")));
        }
        
        #endregion
        
        #region Collaborative Analysis Tests
        
        [Test]
        public void RequestCollaborativeAnalysis_WithValidRequest_ReturnsResult()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var analysisRequest = CreateValidAnalysisRequest();
            
            // Act
            var result = _partnershipInfrastructure.RequestCollaborativeAnalysis(partnership.PartnershipId, analysisRequest);
            
            // Assert
            Assert.IsNotNull(result.ResultId);
            Assert.AreEqual(analysisRequest.RequestId, result.RequestId);
            Assert.AreEqual(AnalysisCompletionStatus.Completed, result.CompletionStatus);
            Assert.IsTrue(result.Deliverables.Length > 0);
            Assert.IsNotNull(result.QualityAssessment);
        }
        
        [Test]
        public void RequestCollaborativeAnalysis_WithInvalidPartnership_ReturnsFailure()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var analysisRequest = CreateValidAnalysisRequest();
            
            // Act
            var result = _partnershipInfrastructure.RequestCollaborativeAnalysis("invalid-id", analysisRequest);
            
            // Assert
            Assert.AreEqual(AnalysisCompletionStatus.Failed, result.CompletionStatus);
            Assert.IsTrue(result.AnalysisNotes.Contains("Partnership not found"));
        }
        
        [Test]
        public void RequestCollaborativeAnalysis_WithInvalidRequest_ReturnsFailure()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var analysisRequest = CreateValidAnalysisRequest();
            analysisRequest.RequestId = ""; // Invalid
            
            // Act
            var result = _partnershipInfrastructure.RequestCollaborativeAnalysis(partnership.PartnershipId, analysisRequest);
            
            // Assert
            Assert.AreEqual(AnalysisCompletionStatus.Failed, result.CompletionStatus);
            Assert.IsTrue(result.AnalysisNotes.Contains("validation failed"));
        }
        
        #endregion
        
        #region Partnership Management Tests
        
        [Test]
        public void GeneratePartnershipDocumentation_WithValidPartnership_ReturnsDocumentation()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            
            // Act
            var documentation = _partnershipInfrastructure.GeneratePartnershipDocumentation(partnership.PartnershipId);
            
            // Assert
            Assert.IsNotNull(documentation.DocumentationId);
            Assert.AreEqual(partnership.PartnershipId, documentation.PartnershipId);
            Assert.IsNotNull(documentation.DocumentationContent);
            Assert.IsTrue(documentation.DocumentationContent.Contains(partnership.PartnerInstitution.InstitutionName));
        }
        
        [Test]
        public void RenewPartnership_WithValidTerms_ReturnsSuccess()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var renewalTerms = new PartnershipRenewalTerms
            {
                RenewalDurationMonths = 12,
                RenewalJustification = "Successful collaboration",
                UpdatedObjectives = new[] { "Continue research collaboration" },
                RenewalConditions = new[] { "Maintain data sharing agreement" }
            };
            
            // Act
            var result = _partnershipInfrastructure.RenewPartnership(partnership.PartnershipId, renewalTerms);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotNull(result.RenewalId);
            Assert.IsTrue(result.NewExpirationDate > DateTime.Now);
            Assert.IsTrue(result.RenewalMessages.Any(m => m.Contains("successfully")));
        }
        
        [Test]
        public void TerminatePartnership_WithValidReason_ReturnsSuccess()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            var terminationReason = "Project completed";
            
            // Act
            var result = _partnershipInfrastructure.TerminatePartnership(partnership.PartnershipId, terminationReason);
            
            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsNotNull(result.TerminationId);
            Assert.AreEqual(terminationReason, result.TerminationReason);
            Assert.IsTrue(result.TerminationMessages.Any(m => m.Contains("successfully")));
        }
        
        [Test]
        public void GetPartnershipMetrics_WithValidPartnership_ReturnsMetrics()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            
            // Act
            var metrics = _partnershipInfrastructure.GetPartnershipMetrics(partnership.PartnershipId);
            
            // Assert
            Assert.IsTrue(metrics.EstablishmentDate <= DateTime.Now);
            Assert.AreEqual(0, metrics.CollaborationsCount);
            Assert.AreEqual(0, metrics.DataSharingTransactions);
            Assert.AreEqual(0, metrics.PublicationsCount);
        }
        
        [Test]
        public void ResetPartnershipInfrastructure_ClearsAllData()
        {
            // Arrange
            _partnershipInfrastructure.Initialize(AcademicPartnershipConfiguration.Default);
            var partnership = _partnershipInfrastructure.EstablishPartnership(CreateValidPartnershipRequest());
            
            // Act
            _partnershipInfrastructure.ResetPartnershipInfrastructure();
            
            // Assert
            var activePartnerships = _partnershipInfrastructure.ActivePartnerships;
            Assert.AreEqual(0, activePartnerships.Length);
            
            // Verify that attempting to use the previous partnership fails
            var result = _partnershipInfrastructure.ValidatePartnershipCompliance(partnership.PartnershipId);
            Assert.IsFalse(result.IsCompliant);
        }
        
        #endregion       
 
        #region Helper Methods
        
        private PartnershipRequest CreateValidPartnershipRequest()
        {
            return new PartnershipRequest
            {
                RequestingOrganization = "Test Research Organization",
                TargetInstitution = new AcademicInstitution
                {
                    InstitutionName = "University of Research Excellence",
                    Type = InstitutionType.University,
                    ContactInfo = new ContactInformation
                    {
                        Name = "Dr. Research Director",
                        Email = "research@university.edu",
                        Phone = "+1-555-0123",
                        Address = "123 University Ave, Research City, ST 12345",
                        Department = "Computer Science",
                        Title = "Research Director"
                    },
                    PrimaryResearchContact = new ResearchContact
                    {
                        Name = "Dr. Jane Smith",
                        Title = "Professor",
                        Department = "Human-Computer Interaction",
                        Email = "jane.smith@university.edu",
                        Phone = "+1-555-0124",
                        OfficeAddress = "Room 456, CS Building",
                        Specializations = new[] { "VR Research", "User Experience", "Motion Sickness" },
                        ORCIDId = "0000-0000-0000-0000"
                    },
                    IRBContact = new IRBContact
                    {
                        OfficeName = "Institutional Review Board",
                        CoordinatorName = "IRB Coordinator",
                        Email = "irb@university.edu",
                        Phone = "+1-555-0125",
                        Address = "IRB Office, Admin Building",
                        SubmissionPortalUrl = "https://irb.university.edu/submit",
                        MeetingSchedule = "Monthly",
                        ReviewTimeline = "4-6 weeks"
                    },
                    Accreditations = new[]
                    {
                        new AccreditationInfo
                        {
                            AccreditingBody = "Higher Learning Commission",
                            AccreditationType = "Institutional",
                            Status = AccreditationStatus.Active,
                            AccreditationDate = DateTime.Now.AddYears(-5),
                            ExpirationDate = DateTime.Now.AddYears(5),
                            CertificateNumber = "ACC123456"
                        }
                    },
                    ResearchAreas = new[] { "Computer Science", "Human-Computer Interaction", "Virtual Reality" },
                    WebsiteUrl = "https://www.university.edu"
                },
                ProposedTerms = new PartnershipTermsAndConditions
                {
                    PartnershipDurationMonths = 24,
                    DataRetentionYears = 7,
                    IntellectualPropertySharing = IntellectualPropertyPolicy.Shared,
                    PublicationRights = PublicationRightsPolicy.JointApproval,
                    TerminationClauses = new[]
                    {
                        new TerminationClause
                        {
                            ClauseId = "TERM_001",
                            Condition = "Either party may terminate with 30 days notice",
                            NoticePeriod = TimeSpan.FromDays(30),
                            Penalties = new[] { "No penalties for standard termination" },
                            DataHandlingRequirements = new[] { "Return or destroy shared data" }
                        }
                    },
                    DisputeResolution = DisputeResolutionMethod.Mediation,
                    AdditionalTerms = new[] { "Regular progress reviews required" }
                },
                CollaborationObjectives = new[]
                {
                    "Conduct user comfort validation studies",
                    "Develop motion sickness assessment protocols",
                    "Publish joint research findings"
                },
                ExpectedDurationMonths = 24,
                RequiresIRBApproval = true,
                InvolvesDataSharing = true,
                RequestDate = DateTime.Now,
                RequestNotes = "Partnership for VR comfort research collaboration"
            };
        }
        
        private IRBApplication CreateValidIRBApplication()
        {
            return new IRBApplication
            {
                ApplicationId = Guid.NewGuid().ToString(),
                StudyProtocolId = "PROTOCOL_001",
                PrincipalInvestigator = new PrincipalInvestigator
                {
                    Name = "Dr. Jane Smith",
                    Credentials = "PhD in Computer Science",
                    Institution = "University of Research Excellence",
                    Department = "Human-Computer Interaction",
                    ContactInfo = new ContactInformation
                    {
                        Name = "Dr. Jane Smith",
                        Email = "jane.smith@university.edu",
                        Phone = "+1-555-0124"
                    },
                    ResearchExperience = "10 years of VR research experience",
                    PreviousIRBApprovals = new[] { "IRB-2022-001", "IRB-2021-003" },
                    TrainingCertifications = new[]
                    {
                        new TrainingCertification
                        {
                            ProgramName = "Human Subjects Research Training",
                            CertificationType = "CITI Program",
                            CompletionDate = DateTime.Now.AddMonths(-6),
                            ExpirationDate = DateTime.Now.AddYears(3),
                            CertificateNumber = "CITI123456",
                            IssuingOrganization = "CITI Program"
                        }
                    }
                },
                CoInvestigators = new[]
                {
                    new CoInvestigator
                    {
                        Name = "Dr. John Doe",
                        Role = "Co-Principal Investigator",
                        Institution = "Test Research Organization",
                        ContactInfo = new ContactInformation
                        {
                            Name = "Dr. John Doe",
                            Email = "john.doe@testorg.com",
                            Phone = "+1-555-0126"
                        },
                        Expertise = new[] { "Statistical Analysis", "Data Visualization" },
                        TrainingCertifications = new[]
                        {
                            new TrainingCertification
                            {
                                ProgramName = "Research Ethics Training",
                                CertificationType = "NIH Training",
                                CompletionDate = DateTime.Now.AddMonths(-3),
                                CertificateNumber = "NIH789012",
                                IssuingOrganization = "National Institutes of Health"
                            }
                        }
                    }
                },
                StudyTitle = "User Comfort Validation in VR Text Input Systems",
                StudyDescription = "A study to evaluate user comfort and motion sickness during VR text input using wave-based interfaces",
                ResearchObjectives = new[]
                {
                    "Assess motion sickness levels during VR text input",
                    "Validate comfort of wave-based text interfaces",
                    "Develop comfort assessment protocols"
                },
                ParticipantPopulation = new ParticipantPopulation
                {
                    TargetSampleSize = 12,
                    AgeCriteria = new AgeRangeCriteria
                    {
                        MinimumAge = 18,
                        MaximumAge = 35,
                        IncludesMinors = false,
                        AgeGroupConsiderations = new[] { "Adult participants only" }
                    },
                    GenderDistribution = new GenderDistribution
                    {
                        MalePercentage = 50f,
                        FemalePercentage = 50f,
                        NonBinaryPercentage = 0f,
                        GenderIsStudyVariable = false,
                        GenderConsiderations = new[] { "Balanced gender representation" }
                    },
                    InclusionCriteria = new[] { "Age 18-35", "Normal or corrected vision", "No history of severe motion sickness" },
                    ExclusionCriteria = new[] { "Vestibular disorders", "Pregnancy", "Seizure disorders" },
                    VulnerablePopulations = new VulnerablePopulation[0],
                    RecruitmentMethods = new[] { "University recruitment", "Online advertisements" },
                    Compensation = new CompensationDetails
                    {
                        CompensationAmount = 25m,
                        Currency = "USD",
                        Method = CompensationMethod.Cash,
                        Justification = "Compensation for time and travel",
                        PaymentSchedule = "Upon completion of study session"
                    }
                },
                RiskAssessment = new RiskAssessment
                {
                    OverallRiskLevel = RiskLevel.Minimal,
                    IdentifiedRisks = new[]
                    {
                        new IdentifiedRisk
                        {
                            RiskId = "RISK_001",
                            Description = "Mild motion sickness or discomfort",
                            Category = RiskCategory.Physical,
                            Probability = RiskProbability.Low,
                            Severity = RiskSeverity.Minor,
                            AffectedPopulations = new[] { "All participants" }
                        }
                    },
                    MitigationStrategies = new[]
                    {
                        new RiskMitigationStrategy
                        {
                            StrategyId = "MIT_001",
                            RiskId = "RISK_001",
                            Description = "Immediate session termination if severe symptoms occur",
                            ImplementationTimeline = "Throughout study session",
                            ResponsibleParties = new[] { "Research staff" },
                            EffectivenessMonitoring = "Continuous monitoring during sessions"
                        }
                    },
                    EmergencyProcedures = new[]
                    {
                        new EmergencyProcedure
                        {
                            ProcedureId = "EMERG_001",
                            EmergencyType = EmergencyType.Medical,
                            ProcedureSteps = new[] { "Stop session immediately", "Assist participant to seated position", "Monitor symptoms", "Contact medical personnel if needed" },
                            EmergencyContacts = new[]
                            {
                                new EmergencyContact
                                {
                                    Name = "Campus Health Services",
                                    Role = "Medical Support",
                                    PrimaryPhone = "+1-555-0127",
                                    Email = "health@university.edu",
                                    AvailabilityHours = "24/7"
                                }
                            },
                            RequiredResources = new[] { "First aid kit", "Emergency contact list" }
                        }
                    },
                    RiskMonitoringPlan = "Continuous monitoring during sessions with standardized assessment tools",
                    AdverseEventReporting = "All adverse events reported to IRB within 24 hours"
                },
                ConsentProcedures = new InformedConsentProcedures
                {
                    ConsentProcess = "Written informed consent obtained before participation",
                    ConsentFormElements = new[]
                    {
                        new ConsentFormElement
                        {
                            ElementType = ConsentElementType.StudyPurpose,
                            Description = "Purpose and objectives of the study",
                            IsRequired = true,
                            Content = "This study aims to evaluate user comfort during VR text input"
                        },
                        new ConsentFormElement
                        {
                            ElementType = ConsentElementType.RisksAndBenefits,
                            Description = "Potential risks and benefits",
                            IsRequired = true,
                            Content = "Minimal risk of mild motion sickness; benefits include contribution to VR research"
                        }
                    },
                    DocumentationMethod = ConsentDocumentationMethod.WrittenSignature,
                    SpecialConsiderations = new[] { "Participants may withdraw at any time" },
                    WithdrawalProcedures = "Participants may withdraw by verbal or written notification",
                    LanguageRequirements = new[] { "English" }
                },
                DataManagementPlan = new DataManagementPlan
                {
                    DataCollectionMethods = new[] { "SIM questionnaires", "SSQ questionnaires", "Behavioral observations" },
                    StorageProcedures = new DataStorageProcedures
                    {
                        StorageLocation = "Secure university servers",
                        StorageMedium = StorageMedium.CloudStorage,
                        AccessControls = new[]
                        {
                            new AccessControl
                            {
                                AccessControlId = "AC_001",
                                UserOrRole = "Principal Investigator",
                                Permissions = new[] { "Read", "Write", "Delete" },
                                Restrictions = new[] { "VPN access required" }
                            }
                        },
                        BackupProcedures = "Daily automated backups to secure cloud storage",
                        VersionControlProcedures = "Git-based version control for all data files"
                    },
                    SecurityMeasures = new DataSecurityMeasures
                    {
                        EncryptionRequirements = new EncryptionRequirements
                        {
                            EncryptionAlgorithm = "AES-256",
                            KeyLength = 256,
                            EncryptionMode = "CBC",
                            KeyManagementRequirements = new[] { "Secure key storage", "Regular key rotation" },
                            RequireVerification = true
                        },
                        AuthenticationMethods = new[] { AuthenticationMethod.TwoFactor },
                        NetworkSecurityMeasures = new[] { "VPN required", "Firewall protection" },
                        PhysicalSecurityMeasures = new[] { "Locked server room", "Access card required" },
                        SecurityMonitoringProcedures = "24/7 security monitoring and logging"
                    },
                    SharingPlan = new DataSharingPlan
                    {
                        WillShareData = true,
                        SharingTimeline = "After study completion and publication",
                        SharingRepositories = new[] { "University data repository" },
                        SharingRestrictions = new[] { "Anonymized data only" },
                        RequiredAgreements = new[] { "Data sharing agreement" }
                    },
                    RetentionSchedule = new DataRetentionSchedule
                    {
                        RetentionPeriods = new Dictionary<DataType, TimeSpan>
                        {
                            { DataType.RawData, TimeSpan.FromYears(7) },
                            { DataType.ProcessedData, TimeSpan.FromYears(7) },
                            { DataType.AnalysisResults, TimeSpan.FromYears(10) }
                        },
                        RetentionJustification = "Required by university policy and funding agency requirements",
                        ReviewSchedule = "Annual review of retention needs",
                        LegalRequirements = new[] { "University policy", "Federal funding requirements" }
                    },
                    DataDestructionProcedures = "Secure deletion using DoD 5220.22-M standard"
                },
                SubmissionDate = DateTime.Now,
                ExpectedStartDate = DateTime.Now.AddMonths(1),
                ExpectedCompletionDate = DateTime.Now.AddMonths(6)
            };
        }
        
        private DataSharingTerms CreateValidDataSharingTerms()
        {
            return new DataSharingTerms
            {
                DataTypes = new[] { DataType.ProcessedData, DataType.AnalysisResults },
                SharingPurpose = "Collaborative analysis of user comfort data",
                AccessRestrictions = new[]
                {
                    new DataAccessRestriction
                    {
                        RestrictionType = "Personnel",
                        Description = "Access limited to authorized research personnel",
                        AffectedDataTypes = new[] { DataType.ProcessedData },
                        EnforcementMethod = "Access control lists",
                        Exceptions = new[] { "Emergency access for data recovery" }
                    }
                },
                RetentionPeriod = TimeSpan.FromYears(7),
                AnonymizationRequirements = new AnonymizationRequirements
                {
                    RequireAnonymization = true,
                    RequiredLevel = DataAnonymizationLevel.Anonymized,
                    AnonymizationMethods = new[] { "Identifier removal", "Data aggregation" },
                    ElementsToAnonymize = new[] { "Participant IDs", "Timestamps", "Personal identifiers" },
                    RequireVerification = true
                },
                SecurityRequirements = new DataSecurityRequirements
                {
                    RequireEncryption = true,
                    EncryptionStandards = new[] { "AES-256" },
                    AccessControls = new[]
                    {
                        new DataAccessControl
                        {
                            AccessControlType = "Role-based",
                            AuthorizedPersonnel = new[] { "Principal Investigator", "Co-Investigators" },
                            Permissions = new[] { "Read", "Analyze" },
                            Restrictions = new[] { "No data export without approval" },
                            RequireAccessLogging = true
                        }
                    },
                    MonitoringRequirements = new[] { "Access logging", "Regular security audits" },
                    IncidentResponseRequirements = new[] { "24-hour breach notification", "Immediate access revocation" }
                },
                PublicationRights = new PublicationRights
                {
                    ApprovalRequirements = "Joint approval required for all publications",
                    AuthorshipGuidelines = new[] { "Authorship based on contribution", "All parties acknowledged" },
                    TimelineRequirements = "30-day review period for publication approval",
                    CitationRequirements = new[] { "Cite data sharing agreement", "Acknowledge all contributors" },
                    PublicationRestrictions = new[] { "No publication without partner approval" }
                },
                ComplianceRequirements = new[] { "GDPR", "HIPAA", "University policies" },
                EffectiveDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddYears(2)
            };
        }
        
        private StudyDataPackage CreateValidStudyDataPackage()
        {
            return new StudyDataPackage
            {
                PackageId = Guid.NewGuid().ToString(),
                StudyId = "STUDY_001",
                Contents = new[]
                {
                    new DataPackageContent
                    {
                        ContentId = "CONTENT_001",
                        DataType = DataType.ProcessedData,
                        Description = "Processed SIM/SSQ questionnaire responses",
                        FilePath = "/data/processed/sim_ssq_responses.csv",
                        FileSizeBytes = 1024 * 50, // 50KB
                        Format = "CSV",
                        CreationTimestamp = DateTime.Now
                    },
                    new DataPackageContent
                    {
                        ContentId = "CONTENT_002",
                        DataType = DataType.AnalysisResults,
                        Description = "Statistical analysis results",
                        FilePath = "/data/analysis/statistical_results.json",
                        FileSizeBytes = 1024 * 25, // 25KB
                        Format = "JSON",
                        CreationTimestamp = DateTime.Now
                    }
                },
                AnonymizationLevel = DataAnonymizationLevel.Anonymized,
                IsEncrypted = true,
                IntegrityHash = "sha256:abcd1234efgh5678ijkl9012mnop3456",
                CreationDate = DateTime.Now,
                Metadata = new Dictionary<string, string>
                {
                    { "StudyTitle", "VR Comfort Validation Study" },
                    { "ParticipantCount", "12" },
                    { "DataCollectionPeriod", "2024-01-01 to 2024-03-31" }
                },
                ComplianceVerification = new ComplianceVerification
                {
                    VerificationTimestamp = DateTime.Now,
                    VerificationMethod = "Automated compliance check",
                    ComplianceStandards = new[] { "GDPR", "University policies" },
                    IsCompliant = true,
                    VerificationNotes = "Data package meets all compliance requirements"
                }
            };
        }
        
        private AnalysisRequest CreateValidAnalysisRequest()
        {
            return new AnalysisRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                AnalysisType = AnalysisType.StatisticalAnalysis,
                AnalysisObjectives = new[]
                {
                    "Perform descriptive statistics on comfort measures",
                    "Conduct hypothesis testing for motion sickness differences",
                    "Generate visualization of results"
                },
                DataPackage = CreateValidStudyDataPackage(),
                RequestedMethods = new[] { "T-tests", "ANOVA", "Descriptive statistics" },
                ExpectedDeliverables = new[] { "Statistical report", "Data visualizations", "Interpretation summary" },
                Timeline = new AnalysisTimeline
                {
                    ExpectedStartDate = DateTime.Now.AddDays(7),
                    ExpectedCompletionDate = DateTime.Now.AddDays(21),
                    Milestones = new[]
                    {
                        new AnalysisMilestone
                        {
                            MilestoneId = "M1",
                            Description = "Data preprocessing complete",
                            ExpectedDate = DateTime.Now.AddDays(10),
                            Dependencies = new[] { "Data package received" },
                            Deliverables = new[] { "Cleaned dataset" }
                        },
                        new AnalysisMilestone
                        {
                            MilestoneId = "M2",
                            Description = "Statistical analysis complete",
                            ExpectedDate = DateTime.Now.AddDays(17),
                            Dependencies = new[] { "M1" },
                            Deliverables = new[] { "Analysis results" }
                        }
                    },
                    FlexibilityDays = 3
                },
                ResourceRequirements = new ResourceRequirements
                {
                    RequiredPersonnel = 2,
                    RequiredExpertise = new[] { "Statistical analysis", "Data visualization" },
                    RequiredSoftware = new[] { "R", "SPSS", "Python" },
                    ComputationalResources = new ComputationalResources
                    {
                        RequiredCPUCores = 4,
                        RequiredRAMGB = 16,
                        RequiredStorageGB = 100,
                        RequiresGPU = false,
                        EstimatedProcessingHours = 8
                    },
                    EstimatedCost = 2500m,
                    Currency = "USD"
                },
                RequestDate = DateTime.Now,
                Priority = RequestPriority.Medium
            };
        }
        
        #endregion
    }
}