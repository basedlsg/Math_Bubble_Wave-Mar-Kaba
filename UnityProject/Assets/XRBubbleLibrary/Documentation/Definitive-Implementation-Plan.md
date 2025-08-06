# Definitive Implementation Plan

**Date:** 2025-08-03
**Status:** Approved

**Objective:** To provide a clear, step-by-step guide for the final implementation of the architectural changes. This plan is based on the findings of the committee's deep dive and is designed to be low-risk and minimally invasive.

---

## Phase 1: Code and Configuration Correction

**Objective:** To resolve all outstanding compilation errors and warnings in a controlled manner.

1.  **Correct `Core.asmdef`:**
    *   **Action:** Modify the `UnityProject/Assets/XRBubbleLibrary/Core/Core.asmdef` file to add a reference to the `Unity.Mathematics` package.
    *   **Expected Outcome:** Resolves all `CS0234` and `CS0246` errors related to the `Unity.Mathematics` namespace.

2.  **Remove Duplicated Code:**
    *   **Action:** Edit the `UnityProject/Assets/XRBubbleLibrary/Core/IBiasField.cs` and `UnityProject/Assets/XRBubbleLibrary/Core/IWaveOptimizer.cs` files to remove the duplicated `BiasFieldRegistry` and `WaveOptimizationRegistry` class definitions.
    *   **Expected Outcome:** Resolves all `CS0101` and `CS0111` errors related to duplicate definitions.

3.  **Stabilize Project Dependencies:**
    *   **Action:** Modify the `UnityProject/Packages/manifest.json` file to re-introduce the `com.unity.textmeshpro` package.
    *   **Expected Outcome:** Resolves the ShaderGraph import failure and restores the project to a stable state.

## Phase 2: Finalization and Verification

**Objective:** To ensure the project is in a clean, stable, and well-documented state.

1.  **Properly Deprecate TextMeshPro:**
    *   **Action:** Once the project is stable, follow the official Unity documentation to migrate from the `com.unity.textmeshpro` package to the `com.unity.ugui` package. This may involve running a Unity-provided upgrader tool.
    *   **Expected Outcome:** The project is up-to-date with the latest Unity UI best practices, and the deprecated package is removed without causing side effects.

2.  **Final Compilation and Testing:**
    *   **Action:** Perform a full re-import and compilation of the project.
    *   **Expected Outcome:** The project compiles without any errors or warnings.

3.  **Review and Cleanup:**
    *   **Action:** Review all modified files to ensure they are clean, well-formatted, and free of any commented-out or unnecessary code.
    *   **Expected Outcome:** The codebase is in a professional, maintainable state.

---

### Committee Conclusion

This plan represents the final, definitive set of actions required to complete the architectural refactoring. Each step is designed to address a specific, well-understood issue. Upon completion of this plan, the project will be stable, robust, and ready for future development.