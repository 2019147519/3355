using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class NicknameDisplayUI : MonoBehaviour
{
    [SerializeField] private string _guestName = "Guest";

    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        string nickname = BackendManager.Instance != null
            ? BackendManager.Instance.CurrentNickname
            : string.Empty;

        if (string.IsNullOrWhiteSpace(nickname))
            nickname = PlayerPrefs.GetString("BackendLastNickname", _guestName);
        if (string.IsNullOrWhiteSpace(nickname))
            nickname = _guestName;

        _text.text = nickname;
    }
}
