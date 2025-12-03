using UnityEngine;
using System.Collections;

public class Reel : MonoBehaviour
{
    public Transform symbolContainer;
    public GameObject symbolPrefab;           // simple Image with symbol sprite
    public float spinDuration = 1.5f;
    public float extraSpins = 3f;

    private Symbol[] symbols;
    private bool spinning = false;

    void Awake()
    {
        symbols = symbolContainer.GetComponentsInChildren<Symbol>();
    }

    public void StartSpin()
    {
        if (spinning) return;
        spinning = true;
        StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        float time = 0;
        float speed = 800f; // pixels per second

        while (time < spinDuration + extraSpins * 0.4f)
        {
            symbolContainer.localPosition += Vector3.down * speed * Time.deltaTime;
            if (symbolContainer.localPosition.y <= -1200) // 10 symbols × 120px height
                symbolContainer.localPosition += Vector3.up * 1200;

            time += Time.deltaTime;
            yield return null;
        }
    }

    public void StopAtSymbols(int[] targetSymbols) // 5 symbols top-to-bottom
    {
        StopAllCoroutines();
        StartCoroutine(StopRoutine(targetSymbols));
    }

    IEnumerator StopRoutine(int[] target)
    {
        // Fast blur then ease-in stop
        float fastTime = 0.4f;
        float easeTime = 0.6f;
        float fastSpeed = 1200f;

        while (fastTime > 0)
        {
            symbolContainer.localPosition += Vector3.down * fastSpeed * Time.deltaTime;
            if (symbolContainer.localPosition.y <= -1200)
                symbolContainer.localPosition += Vector3.up * 1200;
            fastTime -= Time.deltaTime;
            yield return null;
        }

        // Position exactly on target
        float targetY = -240f; // center the middle symbol (3rd one visible)
        for (int i = 0; i < 3; i++)
            symbols[i + 1].SetSymbolID(target[i]); // middle three visible

        float ease = 0;
        Vector3 startPos = symbolContainer.localPosition;
        Vector3 endPos = new Vector3(0, targetY, 0);

        while (ease < easeTime)
        {
            ease += Time.deltaTime;
            float t = ease / easeTime;
            t = t * t * (3f - 2f * t); // smoothstep
            symbolContainer.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        symbolContainer.localPosition = endPos;
        spinning = false;
    }
}