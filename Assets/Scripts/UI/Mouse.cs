using UnityEngine;

public class Mouse : MonoBehaviour
{
    public Texture2D cursorTexture;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Cursor.SetCursor(cursorTexture, new Vector2(0, 0), CursorMode.ForceSoftware);
    }

    void Update()
    {

    }
}