# Committee Review: Final Surgical Plan

**Date:** 2025-08-03
**Status:** Approved

**Objective:** To outline the two precise, minimal, and surgical actions required to resolve the final outstanding errors. This plan is to be executed exactly as written, without deviation.

---

## Action 1: Correct `GroqAPIClient.cs` Syntax

*   **File:** `UnityProject/Assets/XRBubbleLibrary/AI/GroqAPIClient.cs`
*   **Problem:** Two lines of code have a syntax error (`new float3` instead of `new float3[0]`).
*   **Action:**
    1.  Read the `GroqAPIClient.cs` file to get its current, corrupted content.
    2.  Perform a single `apply_diff` operation with two `SEARCH`/`REPLACE` blocks to correct both instances of the syntax error.
*   **Expected Outcome:** The `CS1526` errors will be resolved.

## Action 2: Eliminate Duplicate `WaveSource.cs`

*   **File:** A duplicated `WaveSource.cs` file.
*   **Problem:** The presence of a second `WaveSource.cs` file is causing a `CS0101` error.
*   **Action:**
    1.  Perform a `search_files` operation to locate the exact paths of all instances of `WaveSource.cs`.
    2.  Based on the results, identify the incorrect, duplicated file.
    3.  Execute a `Remove-Item` command to delete *only* the duplicated file.
*   **Expected Outcome:** The `CS0101` error will be resolved.

---

### Committee Conclusion

This plan is the final word on the matter. It is a direct, precise, and minimal set of actions designed to bring the project to a stable, compilable state. There will be no further analysis or deviation. The committee's work is to oversee the exact execution of this plan.