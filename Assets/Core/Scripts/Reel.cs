using UnityEngine;
using System.Collections;

public class Reel : MonoBehaviour
{
    [Header("=== Reel Setup ===")]
    public Transform symbolContainer;      // the "Symbols" child object
    public float symbolHeight = 300f;      // exact height of ONE symbol (change in Inspector)
    public int visibleSymbols = 3;         // we always show 3

    [Header("Spin Feel")]
    public float spinTime = 4.0f;          // total spin duration per reel (slowed slightly)
    public float minSpeed = 300f;          // pixels/sec at end (slower)
    public float maxSpeed = 1200f;         // pixels/sec at start (slower)

    private float totalHeight;
    private bool spinning = false;

    void Awake()
    {
        // compute total height from actual child count so wrapping is precise
        int childCount = symbolContainer != null ? symbolContainer.childCount : 0;
        totalHeight = symbolHeight * Mathf.Max(1, childCount);
    }

    public void StartSpin()
    {
        if (spinning) return;
        spinning = true;
        StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        float elapsed = 0f;

        while (elapsed < spinTime)
        {
            float t = elapsed / spinTime;
            // start fast (max) and slow down to min
            float currentSpeed = Mathf.Lerp(maxSpeed, minSpeed, t);

            // move container down
            symbolContainer.localPosition += Vector3.down * currentSpeed * Time.deltaTime;

            // Continuous wrap: whenever we've moved by one symbolHeight,
            // shift the first (top) child to the bottom by changing sibling order only.
            // Do NOT modify individual child localPositions (LayoutGroups will reposition children and cause flicker/gaps).
            while (symbolContainer.localPosition.y <= -symbolHeight)
            {
                // move container back up by one symbol unit to keep positions consistent
                symbolContainer.localPosition += Vector3.up * symbolHeight;

                if (symbolContainer.childCount > 1)
                {
                    // move the first child to the end of sibling order.
                    // LayoutGroup (if present) will immediately layout children without us touching their positions.
                    symbolContainer.GetChild(0).SetAsLastSibling();
                }
            }

            // safety wrap if container goes extremely far (keeps values bounded)
            if (symbolContainer.localPosition.y <= -totalHeight)
                symbolContainer.localPosition += Vector3.up * totalHeight;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Stop WITHOUT snapping to the nearest symbol. Leave the container exactly where it stopped
        // so reels stay in the same visual position they had at the end of spin (no sudden snap).
        spinning = false;
    }

    public void StopOnNextSymbol()
    {
        // Backwards-compatible external call: stop immediately and do NOT snap.
        StopAllCoroutines();
        spinning = false;
    }

    IEnumerator SnapToNearestSymbol()
    {
        // Kept for optional use but not used by default flow.
        float currentY = symbolContainer.localPosition.y;
        float targetY = -Mathf.Round(currentY / symbolHeight) * symbolHeight;

        // make it overshoot a little then bounce back (real slot feel)
        float overshootY = targetY - symbolHeight * 0.25f;

        float time = 0f;
        Vector3 start = symbolContainer.localPosition;
        Vector3 overshootPos = new Vector3(0, overshootY, 0);
        Vector3 targetPos = new Vector3(0, targetY, 0);

        // 1. fast overshoot
        while (time < 0.25f)
        {
            time += Time.deltaTime;
            float s = time / 0.25f;
            symbolContainer.localPosition = Vector3.Lerp(start, overshootPos, s);
            yield return null;
        }

        // 2. bounce back to perfect stop
        time = 0f;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            float s = time / 0.2f;
            s = Mathf.Sin(s * Mathf.PI * 0.5f); // smooth ease-out
            symbolContainer.localPosition = Vector3.Lerp(overshootPos, targetPos, s);
            yield return null;
        }

        symbolContainer.localPosition = targetPos;
        spinning = false;
    }

    // Helper for external control (bonus features, etc.)
    public void InstantStopAt(int[] visibleIDs) // visibleIDs = new int[] { top, middle, bottom }
    {
        // Stop spinning coroutines but DO NOT forcibly snap the container position.
        // Snapping the container caused the visible "snap down" and white gap when LayoutGroups were used.
        StopAllCoroutines();
        spinning = false;

        // Update visible Symbol components' IDs so the visual symbols match the desired result.
        // We avoid changing the container position or child transforms to prevent layout flicker.
        var syms = symbolContainer.GetComponentsInChildren<Symbol>();

        // Attempt to map the requested visibleIDs to the currently visible symbol components.
        // Many setups have the visible symbols as children[1..3] (top→bottom) — preserve that behavior if present.
        if (syms.Length >= visibleSymbols + 2)
        {
            // common layout: syms[1] = top, syms[2] = middle, syms[3] = bottom
            for (int i = 0; i < visibleSymbols && i < visibleIDs.Length; i++)
            {
                int idx = i + 1;
                if (idx < syms.Length)
                    syms[idx].SetSymbolID(visibleIDs[i]);
            }
        }
        else
        {
            // Fallback: set the last visibleSymbols found from top to bottom
            int start = Mathf.Max(0, syms.Length - visibleSymbols);
            for (int i = 0; i < visibleSymbols && i < visibleIDs.Length; i++)
            {
                int idx = start + i;
                if (idx < syms.Length)
                    syms[idx].SetSymbolID(visibleIDs[i]);
            }
        }
    }
}