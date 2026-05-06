// Assets/Scripts/UI/GameHUD.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("턴 표시")]
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private GameObject _blackIndicator;
    [SerializeField] private GameObject _whiteIndicator;

    [Header("타이머")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private Image _timerBar;

    // ★ Inspector 고정값 제거 — 모드별로 코드에서 결정
    private float _timeLimit;

    [Header("수 카운트")]
    [SerializeField] private TextMeshProUGUI _moveText;

    [Header("버튼")]
    [SerializeField] private Button _undoBtn;
    [SerializeField] private Button _pauseBtn;

    private Coroutine _timerCo;

    private void OnEnable()
    {
        _undoBtn.onClick.AddListener(OnUndo);
        _pauseBtn.onClick.AddListener(OnPause);

        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.OnTurnChanged += OnTurnChanged;
        gm.OnMoveMade += OnMoveMade;
        gm.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        _undoBtn.onClick.RemoveAllListeners();
        _pauseBtn.onClick.RemoveAllListeners();

        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.OnTurnChanged -= OnTurnChanged;
        gm.OnMoveMade -= OnMoveMade;
        gm.OnGameOver -= OnGameOver;
    }

    // ── 이벤트 핸들러 ────────────────────────────
    private void OnTurnChanged(Player p)
    {
        bool isBlack = p == Player.Black;
        _turnText.text = isBlack ? "● 흑돌 차례" : "○ 백돌 차례";
        _blackIndicator.SetActive(isBlack);
        _whiteIndicator.SetActive(!isBlack);

        // ★ 모드별 타이머 설정
        _timeLimit = GameManager.Instance.CurrentMode switch
        {
            GameMode.AI => 60f,
            GameMode.Multi => 15f,
            _ => 15f  // Single
        };

        RestartTimer();
    }

    private void OnMoveMade(int r, int c, int p)
        => _moveText.text = $"{GameManager.Instance.Turn.MoveCount}수";

    private void OnGameOver(Player _) => StopTimer();

    // ── 타이머 ───────────────────────────────────
    private void RestartTimer()
    {
        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = StartCoroutine(RunTimer());
    }

    private IEnumerator RunTimer()
    {
        float t = _timeLimit;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            float ratio = t / _timeLimit;
            _timerText.text = Mathf.CeilToInt(t).ToString();
            _timerBar.fillAmount = ratio;
            _timerBar.color = ratio < 0.3f ? Color.red : Color.green;
            yield return null;
        }

        GameManager.Instance?.OnTimeOut();
    }

    public void StopTimer()
    {
        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerText.text = "—";
        _timerBar.fillAmount = 0f;
    }

    // ── 버튼 ─────────────────────────────────────
    private void OnUndo() => GameManager.Instance.RequestUndo();

    private void OnPause()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowPause();
    }
}