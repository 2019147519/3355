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
    [SerializeField] private float _timeLimit = 60f;

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

        // ★ 람다 대신 메서드 참조 — OnDisable에서 정확히 해제 가능
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

    // ── 이벤트 핸들러 ─────────────────────────
    private void OnTurnChanged(Player p)
    {
        bool isBlack = p == Player.Black;
        _turnText.text = isBlack ? "● 흑돌 차례" : "○ 백돌 차례";
        _blackIndicator.SetActive(isBlack);
        _whiteIndicator.SetActive(!isBlack);
        RestartTimer();
    }

    private void OnMoveMade(int r, int c, int p)
        => _moveText.text = $"{GameManager.Instance.Turn.MoveCount}수";

    private void OnGameOver(Player _) => StopTimer();

    // ── 타이머 ────────────────────────────────
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
            _timerText.text = Mathf.CeilToInt(t).ToString() + "초";
            _timerBar.fillAmount = ratio;
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

    private void OnUndo() => GameManager.Instance.RequestUndo();
    private void OnPause()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowPause();
    }
}