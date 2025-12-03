using UnityEngine;
using System.Collections;

public class Reel : MonoBehaviour
{
    [Header("=== Reel Setup ===]
    public Transform symbolContainer;      // the "Symbols" child object
    public float symbolHeight = 300f;      // exact height of ONE symbol (change in Inspector)
    public int visibleSymbols = 3;         // we always show 3

    [Header("Spin Feel]
    public float spinTime = 1.8f;          // total spin duration per reel
    public float minSpeed = 3000f;         // pixels/sec at start
    public float maxSpeed = 8000f;         // peak speed during blur

    private float totalHeight;
    private bool spinning = false;

    void Awake()
    {
        totalHeight = symbolHeight * 24; // pretend we have 24 symbols → perfect loop
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
            float currentSpeed = Mathf.Lerp(maxSpeed, minSpeed, t); // fast → slow

            symbolContainer.localPosition += Vector3.down * currentSpeed * Time.deltaTime;

            // Perfect circular wrap
            if (symbolContainer.localPosition.y <= -totalHeight)
                symbolContainer.localPosition += Vector3.up * totalHeight;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Auto-stop on next clean position after spin ends
        StopOnNextSymbol();
    }

    public void StopOnNextSymbol()
    {
        StopAllCoroutines();
        StartCoroutine(SnapToNearestSymbol());
    }

    IEnumerator SnapToNearestSymbol()
    {
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
            float t = time / 0.25f;
            symbolContainer.localPosition = Vector3.Lerp(start, overshootPos, t);
            yield return null;
        }

        // 2. bounce back to perfect stop
        time = 0f;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            float t = time / 0.2f;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // smooth ease-out
            symbolContainer.localPosition = Vector3.Lerp(overshootPos, targetPos, t);
            yield return null;
        }

        symbolContainer.localPosition = targetPos;
        spinning = false;
    }

    // Helper for external control (bonus features, etc.)
    public void InstantStopAt(int[] visibleIDs) // visibleIDs = new int[] { top, middle, bottom }
    {
        StopAllCoroutines();
        float targetY = -symbolHeight; // middle symbol centered
        symbolContainer.localPosition = new Vector3(0, targetY, 0);

        var syms = symbolContainer.GetComponentsInChildren<Symbol>();
        for (int i = 0; i < 3; i++)
            syms[i + 1].SetSymbolID(visibleIDs[i]);
    }
}