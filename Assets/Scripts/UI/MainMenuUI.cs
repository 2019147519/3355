// Assets/Scripts/UI/MainMenuUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _singlePlayBtn;
    [SerializeField] private Button _multiPlayBtn;
    [SerializeField] private Button _aiPlayBtn;
    [SerializeField] private Button _settingsBtn;

    [Header("Mode Select Panel")]
    [SerializeField] private GameObject _modeSelectPanel;
    [SerializeField] private Button _easyBtn;
    [SerializeField] private Button _normalBtn;
    [SerializeField] private Button _hardBtn;
    [SerializeField] private Button _modeBackBtn;
    [SerializeField] private TextMeshProUGUI _modeTitleText;

    private GameMode _pendingMode;

    private void OnEnable()
    {
        _singlePlayBtn.onClick.AddListener(OnSinglePlay);
        _multiPlayBtn.onClick.AddListener(OnMultiPlay);
        _aiPlayBtn.onClick.AddListener(OnAIPlay);
        _settingsBtn.onClick.AddListener(OnSettings);
        _easyBtn.onClick.AddListener(() => StartWithDifficulty(1));
        _normalBtn.onClick.AddListener(() => StartWithDifficulty(2));
        _hardBtn.onClick.AddListener(() => StartWithDifficulty(3));
        _modeBackBtn.onClick.AddListener(() => _modeSelectPanel.SetActive(false));
    }

    private void OnDisable()
    {
        _singlePlayBtn.onClick.RemoveAllListeners();
        _multiPlayBtn.onClick.RemoveAllListeners();
        _aiPlayBtn.onClick.RemoveAllListeners();
        _settingsBtn.onClick.RemoveAllListeners();
        _easyBtn.onClick.RemoveAllListeners();
        _normalBtn.onClick.RemoveAllListeners();
        _hardBtn.onClick.RemoveAllListeners();
        _modeBackBtn.onClick.RemoveAllListeners();
    }

    // ── 버튼 핸들러 ──────────────────────────────
    private void OnSinglePlay()
    {
        _pendingMode = GameMode.Single;
        StartGame(); // 난이도 선택 없이 바로
    }

    private void OnMultiPlay()
    {
        // 3단계 — 현재는 준비 중 토스트
        ToastUI.Show("멀티플레이는 준비 중입니다.");
    }

    private void OnAIPlay()
    {
        _pendingMode = GameMode.AI;
        _modeTitleText.text = "난이도 선택";
        _modeSelectPanel.SetActive(true);
    }

    private void OnSettings()
    {
        // SettingsUI 추후 구현
    }

    private void StartWithDifficulty(int level)
    {
        _modeSelectPanel.SetActive(false);

        if (_pendingMode == GameMode.AI)
            GameManager.Instance.SetAIDifficulty(level);

        StartGame();
    }

    private void StartGame()
    {
        UIManager.Instance.ShowGameHUD();
        GameManager.Instance.StartGame(_pendingMode);
    }
}