using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class AutoAssignSymbols : MonoBehaviour
{
    [Header("12 colors - edit anytime")]
    public Color[] colors = new Color[]
    {
        Color.red, new Color(1f, 0.5f, 0f), Color.yellow, Color.green,
        Color.cyan, Color.blue, new Color(0.7f, 0f, 1f), Color.magenta,
        Color.white, new Color(1f, 0.8f, 0.4f), new Color(0.9f, 0.9f, 0.9f), new Color(1f, 0.3f, 0.6f)
    };

    [ContextMenu("Generate & Assign Symbols Now")]
    void GenerateAndAssign()
    {
        // Updated line — removes the warning
        Symbol[] allSymbols = FindObjectsByType<Symbol>(FindObjectsSortMode.None);

        foreach (Symbol sym in allSymbols)
        {
            sym.symbolSprites = new Sprite[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[256 * 256];
                for (int p = 0; p < pixels.Length; p++) pixels[p] = colors[i];
                tex.SetPixels(pixels);
                tex.Apply();

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 100);
                sprite.name = "AutoSymbol_" + i;
                sym.symbolSprites[i] = sprite;
            }

            if (sym.image != null)
                sym.image.sprite = sym.symbolSprites[0];
        }

        Debug.Log($"Auto-assigned {colors.Length} colored symbols to {allSymbols.Length} symbols! Ready for Asset Store.");
    }
}