using UnityEngine;
using UnityEngine.UI;

public class Symbol : MonoBehaviour
{
    public Image image;                // drag the Image component here in Inspector (or auto-find)
    public Sprite[] symbolSprites;

    private void Awake()
    {
        // AUTO-FIND the Image if you forgot to drag it (saves your life)
        if (image == null)
            image = GetComponent<Image>();
    }

    public void SetSymbolID(int id)
    {
        if (image == null)
        {
            Debug.LogError("Symbol has no Image component!", this);
            return;
        }

        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            image.color = Color.gray; // fallback
            return;
        }

        if (id < 0 || id >= symbolSprites.Length) id = 0;

        image.sprite = symbolSprites[id];
    }
}