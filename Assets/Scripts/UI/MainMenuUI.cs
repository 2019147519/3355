// Assets/Scripts/UI/MainMenuUI.cs
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("패널들")]
    [SerializeField] private GameObject _selectPanel;
    [SerializeField] private GameObject _gameModePanel;
    [SerializeField] private GameObject _difficultyPanel;
    [SerializeField] private GameObject _colorSelectPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _nicknamePanel;

    // ── 초기화 ───────────────────────────────────
    private void OnEnable()
    {
        // 메인메뉴 열릴 때마다 Select 패널부터 시작
        ShowOnly(_selectPanel);
        // 닉네임은 별도 — 항상 켜둠
        _nicknamePanel.SetActive(true);
    }

    private void OnDisable()
    {
        // 게임 씬 진입 시 닉네임 패널 끔
        _nicknamePanel.SetActive(false);
    }

    // ── 패널 전환 헬퍼 ───────────────────────────
    private void ShowOnly(GameObject target)
    {
        _selectPanel.SetActive(target == _selectPanel);
        _gameModePanel.SetActive(target == _gameModePanel);
        _difficultyPanel.SetActive(target == _difficultyPanel);
        _colorSelectPanel.SetActive(target == _colorSelectPanel);
        _settingsPanel.SetActive(target == _settingsPanel);
        // 닉네임은 ShowOnly 대상 아님 — 별도 관리
    }

    // ══ Select 패널 ══════════════════════════════
    // SelectPanel의 "시작" 버튼 OnClick 연결
    public void OnSelectStart()
        => ShowOnly(_gameModePanel);

    // ══ GameMode 패널 ════════════════════════════
    // SingleBtn OnClick
    public void OnModeSingle()
    {
        _nicknamePanel.SetActive(false);
        UIManager.Instance.ShowGameHUD();
        GameManager.Instance.StartGame(GameMode.Single);
    }

    // AIBtn OnClick
    public void OnModeAI()
        => ShowOnly(_difficultyPanel);

    // MultiBtn OnClick
    public void OnModeMulti()
        => ToastUI.Show("멀티플레이는 준비 중입니다.");

    // GameMode 뒤로가기
    public void OnGameModeBack()
        => ShowOnly(_selectPanel);

    // ══ Difficulty 패널 ══════════════════════════
    public void OnDiffEasy() => SelectDiff(1);
    public void OnDiffNormal() => SelectDiff(2);
    public void OnDiffHard() => SelectDiff(3);

    private void SelectDiff(int level)
    {
        GameManager.Instance.SetAIDifficulty(level);
        ShowOnly(_colorSelectPanel);
    }

    // Difficulty 뒤로가기
    public void OnDiffBack()
        => ShowOnly(_gameModePanel);

    // ══ ColorSelect 패널 ═════════════════════════
    // 내가 흑 선택 → AI는 백
    public void OnColorBlack()
        => StartAI(Player.White);

    // 내가 백 선택 → AI는 흑
    public void OnColorWhite()
        => StartAI(Player.Black);

    private void StartAI(Player aiColor)
    {
        GameManager.Instance.SetAIColor(aiColor);
        _nicknamePanel.SetActive(false);
        UIManager.Instance.ShowGameHUD();
        GameManager.Instance.StartGame(GameMode.AI);
    }

    // ColorSelect 뒤로가기
    public void OnColorBack()
        => ShowOnly(_difficultyPanel);

    // ══ Settings 패널 ════════════════════════════
    // 어느 패널에서든 Settings 버튼 누를 때
    // → 현재 패널 기억 후 Settings 열기
    private GameObject _prevPanel;

    public void OnSettingsOpen()
    {
        // 현재 켜진 패널 기억
        _prevPanel = GetCurrentPanel();
        ShowOnly(_settingsPanel);
    }

    public void OnSettingsClose()
    {
        // 이전 패널로 복귀
        ShowOnly(_prevPanel != null ? _prevPanel : _selectPanel);
    }

    private GameObject GetCurrentPanel()
    {
        if (_selectPanel.activeSelf) return _selectPanel;
        if (_gameModePanel.activeSelf) return _gameModePanel;
        if (_difficultyPanel.activeSelf) return _difficultyPanel;
        if (_colorSelectPanel.activeSelf) return _colorSelectPanel;
        return _selectPanel;
    }
}