using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineMatchStatusUI : MonoBehaviour
{
    public static OnlineMatchStatusUI Instance { get; private set; }

    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Button _cancelButton;

    private bool _canCancel;

    private void Awake()
    {
        Instance = this;
        _cancelButton?.onClick.AddListener(OnCancelClicked);
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        _cancelButton?.onClick.RemoveListener(OnCancelClicked);
    }

    public void Show(string message, bool canCancel = true)
    {
        _canCancel = canCancel;
        if (_panel != null)
            _panel.SetActive(true);
        if (_messageText != null)
            _messageText.text = message;
        if (_cancelButton != null)
            _cancelButton.gameObject.SetActive(true);
    }

    public void SetMessage(string message)
    {
        if (_messageText != null)
            _messageText.text = message;
    }

    public void Hide()
    {
        if (_panel != null)
            _panel.SetActive(false);
    }

    private void OnCancelClicked()
    {
        if (_canCancel)
            OnlineMatchManager.Instance?.CancelMatchmaking();
        else
            Hide();
    }
}
