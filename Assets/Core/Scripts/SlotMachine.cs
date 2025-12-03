using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    public Reel[] reels;                    // assign 5 Reel components in inspector
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public TextMeshProUGUI winText;

    [Header("Game Settings")]
    public long balance = 100000;           // 1,000.00 in cents
    public int[] possibleBets = { 100, 200, 500, 1000, 2000, 5000 };
    public int currentBetIndex = 2;

    private bool isSpinning = false;

    void Start()
    {
        UpdateUI();
    }

    public void Spin()
    {
        if (isSpinning) return;
        long bet = possibleBets[currentBetIndex];
        if (balance < bet) { Debug.Log("Not enough credits"); return; }

        balance -= bet;
        isSpinning = true;
        winText.text = "";
        UpdateUI();

        // Fire all reels
        foreach (var reel in reels)
            reel.StartSpin();

        // Fake server result after 2–3 seconds (you give buyers this exact pattern)
        Invoke(nameof(StopReelsWithResult), 2.5f);
    }

    void StopReelsWithResult()
    {
        // This is where buyer plugs in their own math result
        // For demo we just make something nice-looking
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
            reels[i].StopAtSymbols(fakeResult[i]);
        }

        Invoke(nameof(EvaluateWin), 1.5f);
    }

    void EvaluateWin()
    {
        // Super simple fake win — buyer replaces this whole method
        long win = 25000; //  // 250.00
        balance += win;
        winText.text = $"WIN: ${(win / 100f):F2}";
        UpdateUI();
        isSpinning = false;
    }

    public void ChangeBet(int direction)
    {
        currentBetIndex = Mathf.Clamp(currentBetIndex + direction, 0, possibleBets.Length - 1);
        UpdateUI();
    }

    void UpdateUI()
    {
        balanceText.text = $"BALANCE: ${(balance / 100f):F2}";
        betText.text = $"BET: ${(possibleBets[currentBetIndex] / 100f):F2}";
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
}