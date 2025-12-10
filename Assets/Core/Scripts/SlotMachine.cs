using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    public Reel[] reels;                    // assign 5 Reel components in inspector
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI freeSpinText;    // optional UI to show remaining free spins
    public Button[] holdButtons;            // optional buttons to toggle hold per reel (length == reels.Length)

    [Header("Game Settings")]
    public long balance = 100000;           // 1,000.00 in cents
    public int[] possibleBets = { 100, 200, 500, 1000, 2000, 5000 };
    public int currentBetIndex = 2;

    [Header("Bonus Settings")]
    public int startingFreeSpins = 0;
    public long bigWinThreshold = 100000;   // WIN >= this (in cents) will be considered a Big Win

    private bool isSpinning = false;
    private bool isInFreeSpin = false;
    private int remainingFreeSpins = 0;
    private bool[] reelHeld;

    void Start()
    {
        // initialize hold states
        reelHeld = new bool[reels.Length];
        UpdateUI();
        UpdateHoldUI();
    }

    public void Spin()
    {
        if (isSpinning) return;

        long bet = possibleBets[currentBetIndex];
        if (!isInFreeSpin && balance < bet) { Debug.Log("Not enough credits"); return; }

        if (!isInFreeSpin)
            balance -= bet; // on paid spin deduct bet
        else
            Debug.Log("Free spin used");

        isSpinning = true;
        winText.text = "";
        UpdateUI();

        // Start spinning only reels that are not held
        for (int i = 0; i < reels.Length; i++)
        {
            if (!reelHeld[i])
                reels[i].StartSpin();
        }

        // Demo server delay
        Invoke(nameof(StopReelsWithResult), 2.5f);
    }

    void StopReelsWithResult()
    {
        // Demo math result - buyer replaces this with server math
        int[][] fakeResult = new int[][]
        {
            new int[] {1, 5, 5, 5, 2},
            new int[] {0, 5, 5, 5, 3},
            new int[] {4, 5, 5, 5, 5},
            new int[] {2, 4, 3, 2, 1},
            new int[] {0, 1, 5, 5, 5}
        };

        for (int i = 0; i < reels.Length; i++)
        {
            // If reel is held, we do not stop it — it remains in place.
            if (reelHeld[i])
                continue;

            // Map provided result to the Reel.InstantStopAt API which expects 3 visible symbols:
            // visibleIDs = new int[] { top, middle, bottom }
            int[] full = fakeResult[i];
            int[] visibleIDs;
            if (full.Length >= 5)
            {
                // common case for a full reel strip: take the middle 3 items (indices 1,2,3)
                visibleIDs = new int[] { full[1], full[2], full[3] };
            }
            else if (full.Length == 3)
            {
                visibleIDs = new int[] { full[0], full[1], full[2] };
            }
            else
            {
                // fallback: pad or repeat the first element so we always pass 3 values
                visibleIDs = new int[3];
                for (int k = 0; k < 3; k++)
                    visibleIDs[k] = (k < full.Length) ? full[k] : full[Mathf.Max(0, full.Length - 1)];
            }

            reels[i].InstantStopAt(visibleIDs);
        }

        Invoke(nameof(EvaluateWin), 1.5f);
    }

    void EvaluateWin()
    {
        // Replace this with real math evaluation. Demo sets a fake win.
        long win = 25000; // 250.00

        balance += win;
        winText.text = $"WIN: ${(win / 100f):F2}";
        UpdateUI();

        // Big Win detection
        if (win >= bigWinThreshold)
            StartCoroutine(BigWinRoutine(win));

        isSpinning = false;

        // Handle Free Spins lifecycle
        if (isInFreeSpin)
        {
            remainingFreeSpins = Mathf.Max(0, remainingFreeSpins - 1);
            UpdateUI();

            if (remainingFreeSpins <= 0)
            {
                EndFreeSpins();
            }
            else
            {
                // Auto-spin next free spin after short delay (no bet charged)
                Invoke(nameof(Spin), 1.0f);
            }
        }
    }

    IEnumerator BigWinRoutine(long win)
    {
        // Simple Big Win visual placeholder — buyer replaces with fancy animation
        var original = winText.color;
        winText.text = $"BIG WIN: ${(win / 100f):F2}";
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 2f;
            winText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.6f, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        // restore
        winText.transform.localScale = Vector3.one;
        winText.color = original;
        yield return null;
    }

    // --- Free Spins API ---
    public void StartFreeSpins(int count)
    {
        if (count <= 0) return;
        isInFreeSpin = true;
        remainingFreeSpins = count;
        freeSpinText?.SetText($"FREE: {remainingFreeSpins}");
        // Immediately perform first free spin
        Invoke(nameof(Spin), 0.25f);
    }

    void EndFreeSpins()
    {
        isInFreeSpin = false;
        remainingFreeSpins = 0;
        freeSpinText?.SetText("");
    }

    public int GetRemainingFreeSpins() => remainingFreeSpins;

    // --- Hold & Respin API ---
    public void ToggleHold(int reelIndex)
    {
        if (reelIndex < 0 || reelIndex >= reelHeld.Length) return;
        if (isSpinning) return; // cannot toggle while spinning
        reelHeld[reelIndex] = !reelHeld[reelIndex];
        UpdateHoldUI();
    }

    public void SetHold(int reelIndex, bool hold)
    {
        if (reelIndex < 0 || reelIndex >= reelHeld.Length) return;
        if (isSpinning) return;
        reelHeld[reelIndex] = hold;
        UpdateHoldUI();
    }

    void UpdateHoldUI()
    {
        if (holdButtons == null || holdButtons.Length != reelHeld.Length)
            return;

        for (int i = 0; i < holdButtons.Length; i++)
        {
            var btn = holdButtons[i];
            if (btn == null) continue;

            btn.interactable = !isSpinning;
            // simple color toggle to show held state
            var colors = btn.colors;
            colors.normalColor = reelHeld[i] ? Color.green : Color.white;
            btn.colors = colors;
        }
    }

    // --- UI, Bets, Buttons ---
    public void ChangeBet(int direction)
    {
        currentBetIndex = Mathf.Clamp(currentBetIndex + direction, 0, possibleBets.Length - 1);
        UpdateUI();
    }

    void UpdateUI()
    {
        balanceText.text = $"BALANCE: ${(balance / 100f):F2}";
        betText.text = $"BET: ${(possibleBets[currentBetIndex] / 100f):F2}";
        freeSpinText?.SetText(isInFreeSpin ? $"FREE: {remainingFreeSpins}" : "");
    }

    // Add these three methods — copy-paste exactly
    public void OnSpinButtonClicked()
    {
        Spin();
    }

    public void OnBetUpButtonClicked()
    {
        ChangeBet(1);
    }

    public void OnBetDownButtonClicked()
    {
        ChangeBet(-1);
    }

    // Helper for UI button wiring that accepts index from inspector (use UnityEvents)
    public void OnHoldButtonClickedInt(int index)
    {
        ToggleHold(index);
    }
}