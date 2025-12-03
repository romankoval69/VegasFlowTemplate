using UnityEngine;

public class ForceAssignSymbols : MonoBehaviour
{
    [ContextMenu("FORCE ASSIGN COLORS NOW")]
    void ForceAssign()
    {
        Symbol[] all = FindObjectsByType<Symbol>(FindObjectsSortMode.None);
        Color[] colors = { Color.red, new Color(1f, 0.5f, 0), Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta, Color.white, new Color(1f, 0.8f, 0.4f), new Color(0.9f, 0.9f, 0.9f), new Color(1f, 0.3f, 0.6f), Color.gray };

        foreach (Symbol s in all)
        {
            if (s.image == null) s.image = s.GetComponent<Image>();

            s.symbolSprites = new Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 100) as Sprite[12]; // dummy array size

            for (int i = 0; i < 12; i++)
            {
                Texture2D tex = new Texture2D(256, 256);
                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 256; y++)
                        tex.SetPixel(x, y, colors[i]);
                tex.Apply();
                s.symbolSprites[i] = Sprite.Create(tex, new Rect(0, 0, 256, 256), Vector2.one * 0.5f, 100);
                s.symbolSprites[i].name = "Color" + i;
            }
            s.image.sprite = s.symbolSprites[0];
            s.image.color = Color.white;
        }
        Debug.Log($"FORCE-ASSIGNED colors to {all.Length} symbols – you can now delete this script");
    }
}