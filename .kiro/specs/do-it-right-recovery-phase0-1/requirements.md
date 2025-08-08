# Requirements Document - Do-It-Right Recovery Phase 0-1

## Introduction

This specification defines the comprehensive implementation of Phase 0 (Code Hygiene) and Phase 1 (Wave-Math Demo + Baseline Performance) of our "do-it-right" recovery strategy. The primary objective is to establish an evidence-based development framework that eliminates unverified claims, implements rigorous performance gates, and proves our core wave mathematics system can deliver measurable results on Quest 3 hardware before any additional complexity is introduced.

## Requirements

### Requirement 1: Code Hygiene and Compiler Flag System

**User Story:** As a development team, I want all unverified AI/voice features wrapped behind compiler flags so that we can control what gets built and ensure only validated features are exposed to users.

#### Acceptance Criteria

1. WHEN the project is built THEN all AI-related code SHALL be wrapped in `#if EXP_AI ... #endif` compiler directives with EXP_AI disabled by default
2. WHEN the project is built THEN all voice-related code SHALL be wrapped in `#if EXP_VOICE ... #endif` compiler directives with EXP_VOICE disabled by default
3. WHEN a developer attempts to access disabled features THEN the compiler SHALL exclude those code paths from the build
4. WHEN the build system processes assembly definitions THEN it SHALL respect the compiler flag states and exclude disabled assemblies
5. WHEN reviewing the codebase THEN every experimental feature SHALL be clearly marked and conditionally compiled

### Requirement 2: Development State Documentation System

**User Story:** As a project stakeholder, I want an automatically generated development state document so that I can see exactly which modules are implemented, disabled, or conceptual at any given time.

#### Acceptance Criteria

1. WHEN the nightly build runs THEN the system SHALL generate a DEV_STATE.md file automatically
2. WHEN DEV_STATE.md is generated THEN it SHALL list all modules with status: Implemented | Disabled | Conceptual
3. WHEN DEV_STATE.md is generated THEN it SHALL use reflection on *.asmdef files to determine module states
4. WHEN DEV_STATE.md is updated THEN it SHALL include timestamps and build information
5. WHEN a module status changes THEN the next DEV_STATE.md generation SHALL reflect the change accurately

### Requirement 3: README Warning System

**User Story:** As a user or developer accessing the project, I want clear warnings about disabled features so that I understand what is currently functional versus experimental.

#### Acceptance Criteria

1. WHEN someone views the main README THEN the first line SHALL display "⚠️ AI & voice features are disabled until validated on Quest 3"
2. WHEN AI features are enabled THEN the README warning SHALL update to reflect current status
3. WHEN voice features are enabled THEN the README warning SHALL update to reflect current status
4. WHEN all experimental features are validated THEN the warning SHALL be replaced with validation status
5. WHEN the README is updated THEN it SHALL maintain consistency with DEV_STATE.md status

### Requirement 4: Continuous Integration Performance Gates

**User Story:** As a development team, I want automated CI gates that prevent performance regressions so that we maintain quality standards throughout development.

#### Acceptance Criteria

1. WHEN code is committed THEN CI SHALL run unit tests and fail the build if any tests fail
2. WHEN code is committed THEN CI SHALL run Burst compilation and fail the build if compilation fails
3. WHEN code is committed THEN CI SHALL run a "perf stub" test using the Unity editor profiler
4. WHEN the perf stub runs THEN it SHALL fail the build if median FPS drops below 60 in editor testing
5. WHEN performance tests run THEN they SHALL provide detailed profiling data for analysis
6. WHEN CI gates fail THEN the system SHALL prevent merge and provide clear failure reasons

### Requirement 5: Wave Mathematics Core System Validation

**User Story:** As a technical lead, I want to prove our core wave mathematics system works reliably so that we have a solid foundation before adding complexity.

#### Acceptance Criteria

1. WHEN the wave math demo runs THEN it SHALL display exactly 100 bubbles simultaneously
2. WHEN running on Quest 3 hardware THEN the system SHALL maintain 72 Hz frame rate consistently
3. WHEN performance is measured THEN it SHALL use IL2CPP release builds for accurate metrics
4. WHEN performance data is captured THEN it SHALL use OVR-Metrics for 60-second capture sessions
5. WHEN performance falls below 72 Hz THEN the system SHALL automatically reduce bubble count via LOD until ≥72 Hz is achieved
6. WHEN performance data is collected THEN it SHALL be auto-parsed into perf_report.md in the repository

### Requirement 6: Performance Budget Documentation

**User Story:** As a performance engineer, I want detailed performance budgets documented so that we can make informed decisions about feature additions and optimizations.

#### Acceptance Criteria

1. WHEN performance testing completes THEN the system SHALL generate a perf_budget.md document
2. WHEN perf_budget.md is created THEN it SHALL include a frame-time pie chart showing resource allocation
3. WHEN perf_budget.md is created THEN it SHALL enforce a 30% headroom rule for all performance budgets
4. WHEN new features are proposed THEN they SHALL be evaluated against the documented performance budget
5. WHEN performance budgets are exceeded THEN the system SHALL trigger automatic optimization or feature reduction

### Requirement 7: User Comfort Validation Protocol

**User Story:** As a UX researcher, I want a structured comfort validation protocol so that we can ensure our wave mathematics system doesn't cause user discomfort or motion sickness.

#### Acceptance Criteria

1. WHEN comfort validation begins THEN we SHALL draft a SIM/SSQ study protocol for n=12 participants
2. WHEN the study is designed THEN we SHALL pre-register our hypotheses with clear success criteria
3. WHEN conducting the study THEN we SHALL partner with an HCI lab for proper IRB approval and oversight
4. WHEN comfort data is collected THEN it SHALL be analyzed using standardized motion sickness assessment tools
5. WHEN comfort validation fails THEN we SHALL modify the wave mathematics parameters until validation passes

### Requirement 8: Hardware-Driven Performance Validation

**User Story:** As a hardware validation engineer, I want Quest 3-specific performance metrics so that our performance claims are based on actual target hardware rather than development estimates.

#### Acceptance Criteria

1. WHEN performance testing runs THEN it SHALL execute exclusively on Quest 3 hardware
2. WHEN Quest 3 testing occurs THEN it SHALL use OVR Profiler for accurate hardware-specific metrics
3. WHEN performance data is captured THEN it SHALL include CPU, GPU, memory, and thermal metrics
4. WHEN hardware limits are approached THEN the system SHALL implement automatic performance scaling
5. WHEN Quest 3 hardware is unavailable THEN performance testing SHALL be blocked until hardware is available

### Requirement 9: Automated Performance Monitoring and Rollback

**User Story:** As a DevOps engineer, I want automated performance monitoring with rollback capabilities so that performance regressions are caught and reverted immediately.

#### Acceptance Criteria

1. WHEN performance drops below established baselines THEN the system SHALL automatically trigger rollback procedures
2. WHEN rollback is triggered THEN it SHALL revert to the last known good performance state
3. WHEN performance monitoring runs THEN it SHALL track frame time, memory usage, and thermal performance
4. WHEN performance issues are detected THEN the system SHALL generate detailed diagnostic reports
5. WHEN rollback completes THEN it SHALL notify the development team with specific failure analysis

### Requirement 10: Evidence-Based Development Documentation

**User Story:** As a project manager, I want comprehensive documentation of our evidence-based development process so that all stakeholders understand our validation methodology.

#### Acceptance Criteria

1. WHEN development decisions are made THEN they SHALL be backed by measurable data from Quest 3 hardware
2. WHEN performance claims are made THEN they SHALL include corresponding evidence files with matching hashes
3. WHEN documentation is updated THEN it SHALL include links to supporting performance data and test results
4. WHEN external reviews occur THEN reviewers SHALL have access to complete evidence packages
5. WHEN marketing materials reference performance THEN they SHALL be blocked unless corresponding evidence exists