# Detailed Architectural Refactor Plan

**Date:** 2025-08-03

**Objective:** To provide a clear, actionable, and research-backed plan for refactoring the XRBubbleLibrary project's architecture.

---

## 1. The Core Problem: Systemic Architectural Failure

The project is in a state of systemic failure due to a lack of foundational architectural structure. The root cause is the absence of assembly definitions (`.asmdef` files), which has created a monolithic, tightly-coupled codebase. This has led to a state of "Unverified State Desynchronization," where the file system, the Unity Asset Database, and our understanding of the project are out of sync.

This is the root cause of the file corruption, asset database corruption, and cascading, unpredictable errors that the project is currently experiencing. Any attempt to address individual errors without first addressing the foundational architectural rot is doomed to fail.

## 2. The Solution: A Phased Architectural Refactor

This refactor will be conducted in a series of phases to minimize risk and disruption.

### Phase 1: Project Setup and Version Control

1.  **Version Control:**
    *   Ensure the project is under version control (e.g., Git).
    *   Verify that the `.gitignore` file is correctly configured to ignore the `Library`, `Logs`, `Temp`, and `UserSettings` folders.

2.  **Create a Test Plan:**
    *   Develop a comprehensive test plan to validate existing functionality before and after the refactor.
    *   This should include both manual and automated tests.

### Phase 2: Assembly Definition and Namespace Refactor

1.  **Create a `Core` Assembly:**
    *   Create a new assembly definition file named `Core.asmdef` in the `UnityProject/Assets/XRBubbleLibrary/Core` directory.
    *   Set the "Root Namespace" to `XRBubbleLibrary.Core`.
    *   This assembly will contain the core interfaces and data structures of the library, such as `IWaveOptimizer` and `IBiasField`.

2.  **Create an `AI` Assembly:**
    *   Create a new assembly definition file named `AI.asmdef` in the `UnityProject/Assets/XRBubbleLibrary/AI` directory.
    *   Set the "Root Namespace" to `XRBubbleLibrary.AI`.
    *   This assembly will contain all the AI-related code, including the `GroqAPIClient`.
    *   Add a reference to the `XRBubbleLibrary.Core` assembly.

3.  **Create a `Mathematics` Assembly:**
    *   Create a new assembly definition file named `Mathematics.asmdef` in the `UnityProject/Assets/XRBubbleLibrary/Mathematics` directory.
    *   Set the "Root Namespace" to `XRBubbleLibrary.Mathematics`.
    *   This assembly will contain the `AdvancedWaveSystem` and other mathematical utilities.
    *   Add a reference to the `XRBubbleLibrary.Core` assembly.

4.  **Refactor Namespaces:**
    *   Ensure that all scripts have namespaces that match their corresponding assembly and folder structure.
    *   Use Visual Studio's refactoring tools to automate this process where possible.

### Phase 3: Asset Database and Finalization

1.  **Full Re-import:**
    *   After creating the assembly definitions and refactoring the namespaces, perform a full re-import of all assets in Unity. This will resolve the asset database corruption issues.

2.  **Testing:**
    *   Execute the test plan created in Phase 1 to ensure that the refactor has not introduced any new issues.

## 3. Recommended Next Steps

1.  **Implement the Architectural Refactor as outlined in this plan.**
2.  **Thoroughly test the refactored project.**
3.  **Document the new architecture and coding conventions.**

By following this plan, you will create a stable, scalable, and maintainable foundation for the XRBubbleLibrary project.