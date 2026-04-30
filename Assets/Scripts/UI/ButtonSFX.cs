// Assets/Scripts/UI/ButtonSFX.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(
            () => AudioManager.Instance?.PlayButton()
        );
    }
}