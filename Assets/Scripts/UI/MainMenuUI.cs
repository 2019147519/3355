// Assets/Scripts/UI/MainMenuUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("메인 버튼")]
    [SerializeField] private Button _singleBtn;
    [SerializeField] private Button _aiBtn;
    [SerializeField] private Button _multiBtn;

    [Header("난이도 패널")]
    [SerializeField] private GameObject _diffPanel;
    [SerializeField] private Button _easyBtn;
    [SerializeField] private Button _normalBtn;
    [SerializeField] private Button _hardBtn;
    [SerializeField] private Button _diffBackBtn;

    [Header("색상 선택 패널 ★ NEW")]
    [SerializeField] private GameObject _colorPanel;
    [SerializeField] private Button _blackBtn;   // 내가 흑 (AI=백)
    [SerializeField] private Button _whiteBtn;   // 내가 백 (AI=흑)
    [SerializeField] private Button _colorBackBtn;

    [Header("설정")]
    [SerializeField] private Button _settingsBtn;
    [SerializeField] private GameObject _settingsPanel;

    private int _pendingDifficulty = 2;

    private void OnEnable()
    {
        _singleBtn.onClick.AddListener(StartSingle);
        _aiBtn.onClick.AddListener(OpenDiffPanel);
        _multiBtn.onClick.AddListener(OnMulti);

        _easyBtn.onClick.AddListener(() => OnDiffSelected(1));
        _normalBtn.onClick.AddListener(() => OnDiffSelected(2));
        _hardBtn.onClick.AddListener(() => OnDiffSelected(3));
        _diffBackBtn.onClick.AddListener(() => _diffPanel.SetActive(false));

        _blackBtn.onClick.AddListener(() => OnColorSelected(Player.White)); // 내가 흑 → AI 백
        _whiteBtn.onClick.AddListener(() => OnColorSelected(Player.Black)); // 내가 백 → AI 흑
        _colorBackBtn.onClick.AddListener(() =>
        {
            _colorPanel.SetActive(false);
            _diffPanel.SetActive(true);
        });

        _settingsBtn.onClick.AddListener(OnSettings);
    }

    private void OnDisable()
    {
        _singleBtn.onClick.RemoveAllListeners();
        _aiBtn.onClick.RemoveAllListeners();
        _multiBtn.onClick.RemoveAllListeners();
        _easyBtn.onClick.RemoveAllListeners();
        _normalBtn.onClick.RemoveAllListeners();
        _hardBtn.onClick.RemoveAllListeners();
        _diffBackBtn.onClick.RemoveAllListeners();
        _blackBtn.onClick.RemoveAllListeners();
        _whiteBtn.onClick.RemoveAllListeners();
        _colorBackBtn.onClick.RemoveAllListeners();
        _settingsBtn.onClick.RemoveAllListeners();
    }

    // ── 핸들러 ──────────────────────────────────
    private void StartSingle()
    {
        UIManager.Instance.ShowGameHUD();
        GameManager.Instance.StartGame(GameMode.Single);
    }

    private void OpenDiffPanel()
    {
        _diffPanel.SetActive(true);
    }

    // 난이도 선택 → 색상 선택으로 이동
    private void OnDiffSelected(int level)
    {
        _pendingDifficulty = level;
        GameManager.Instance.SetAIDifficulty(level);
        _diffPanel.SetActive(false);
        _colorPanel.SetActive(true);  // ★ 색상 선택으로
    }

    // 색상 선택 → 게임 시작
    private void OnColorSelected(Player aiColor)
    {
        _colorPanel.SetActive(false);
        GameManager.Instance.SetAIColor(aiColor);
        UIManager.Instance.ShowGameHUD();
        GameManager.Instance.StartGame(GameMode.AI);
    }

    private void OnMulti() => ToastUI.Show("멀티플레이는 준비 중입니다.");

    private void OnSettings()
    {
        AudioManager.Instance.PlayButton();
        _settingsPanel.SetActive(true);
    }
}