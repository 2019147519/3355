// Assets/Scripts/UI/MainMenuUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("메인 버튼 그룹")]
    [SerializeField] private GameObject _mainButtonGroup; // ★ 버튼들 묶은 부모 오브젝트

    [Header("메인 버튼")]
    [SerializeField] private Button _singleBtn;
    [SerializeField] private Button _aiBtn;
    [SerializeField] private Button _multiBtn;
    [SerializeField] private Button _settingsBtn;

    [Header("난이도 패널")]
    [SerializeField] private GameObject _diffPanel;
    [SerializeField] private Button _easyBtn;
    [SerializeField] private Button _normalBtn;
    [SerializeField] private Button _hardBtn;
    [SerializeField] private Button _diffBackBtn;

    [Header("색상 선택 패널")]
    [SerializeField] private GameObject _colorPanel;
    [SerializeField] private Button _blackBtn;
    [SerializeField] private Button _whiteBtn;
    [SerializeField] private Button _colorBackBtn;

    [Header("설정 패널")]
    [SerializeField] private GameObject _settingsPanel;

    private int _pendingDifficulty = 2;

    private void OnEnable()
    {
        _singleBtn.onClick.AddListener(StartSingle);
        _aiBtn.onClick.AddListener(OpenDiffPanel);
        _multiBtn.onClick.AddListener(OnMulti);
        _settingsBtn.onClick.AddListener(OpenSettings);

        _easyBtn.onClick.AddListener(() => OnDiffSelected(1));
        _normalBtn.onClick.AddListener(() => OnDiffSelected(2));
        _hardBtn.onClick.AddListener(() => OnDiffSelected(3));
        _diffBackBtn.onClick.AddListener(CloseDiffPanel);

        _blackBtn.onClick.AddListener(() => OnColorSelected(Player.White));
        _whiteBtn.onClick.AddListener(() => OnColorSelected(Player.Black));
        _colorBackBtn.onClick.AddListener(CloseColorPanel);
    }

    private void OnDisable()
    {
        _singleBtn.onClick.RemoveAllListeners();
        _aiBtn.onClick.RemoveAllListeners();
        _multiBtn.onClick.RemoveAllListeners();
        _settingsBtn.onClick.RemoveAllListeners();
        _easyBtn.onClick.RemoveAllListeners();
        _normalBtn.onClick.RemoveAllListeners();
        _hardBtn.onClick.RemoveAllListeners();
        _diffBackBtn.onClick.RemoveAllListeners();
        _blackBtn.onClick.RemoveAllListeners();
        _whiteBtn.onClick.RemoveAllListeners();
        _colorBackBtn.onClick.RemoveAllListeners();
    }

    // ── 설정 ─────────────────────────────────────
    private void OpenSettings()
    {
        _mainButtonGroup.SetActive(false); // ★ 메인 버튼 전체 숨김
        _settingsPanel.SetActive(true);
    }

    // SettingsUI 닫기 버튼에서 호출할 수 있도록 public
    public void CloseSettings()
    {
        _settingsPanel.SetActive(false);
        _mainButtonGroup.SetActive(true);  // ★ 메인 버튼 복원
    }

    public void ClosePlayMode()
    {
        _settingsPanel.SetActive(false);
        _mainButtonGroup.SetActive(true);  // ★ 메인 버튼 복원
    }

    // ── 싱글 ─────────────────────────────────────
    private void StartSingle()
    {
        UIManager.Instance.ShowGameHUD();
        GameManager.Instance.StartGame(GameMode.Single);
    }

    // ── AI 난이도 ─────────────────────────────────
    private void OpenDiffPanel()
    {
        _mainButtonGroup.SetActive(false);
        _diffPanel.SetActive(true);
    }

    private void CloseDiffPanel()
    {
        _diffPanel.SetActive(false);
        _mainButtonGroup.SetActive(true);
    }

    private void OnDiffSelected(int level)
    {
        _pendingDifficulty = level;
        GameManager.Instance.SetAIDifficulty(level);
        _diffPanel.SetActive(false);
        _colorPanel.SetActive(true);
    }

    // ── AI 색상 ───────────────────────────────────
    private void CloseColorPanel()
    {
        _colorPanel.SetActive(false);
        _diffPanel.SetActive(true);
    }

    private void OnColorSelected(Player aiColor)
    {
        _colorPanel.SetActive(false);
        GameManager.Instance.SetAIColor(aiColor);
        UIManager.Instance.ShowGameHUD();
        GameManager.Instance.StartGame(GameMode.AI);
    }

    private void OnMulti() => ToastUI.Show("멀티플레이는 준비 중입니다.");
}