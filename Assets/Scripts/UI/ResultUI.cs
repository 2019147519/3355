// Assets/Scripts/UI/ResultUI.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultUI : MonoBehaviour
{
    [Header("Result View")]
    [SerializeField] private GameObject _resultView;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _subText;
    [SerializeField] private TextMeshProUGUI _moveText;
    [SerializeField] private float _resultDisplayTime = 2.5f;

    [Header("Rematch View")]
    [SerializeField] private GameObject _rematchView;
    [SerializeField] private Button _rematchYesBtn;
    [SerializeField] private Button _rematchNoBtn;

    [Header("PostGame View")]
    [SerializeField] private GameObject _postGameView;
    [SerializeField] private Button _mainMenuBtn;
    [SerializeField] private Button _startGameBtn;

    [Header("페이드")]
    [SerializeField] private CanvasGroup _cg;
    [SerializeField] private float _maxAlpha = 0.92f;

    private void Awake()
    {
        _rematchYesBtn.onClick.AddListener(OnRematchYes);
        _rematchNoBtn.onClick.AddListener(OnRematchNo);
        _mainMenuBtn.onClick.AddListener(OnMainMenu);
        _startGameBtn.onClick.AddListener(OnStartGame);

        gameObject.SetActive(false);
    }

    // ── 외부 진입점 ─────────────────────────────
    public void Show(Player winner)
    {
        gameObject.SetActive(true);
        AudioManager.Instance?.PlayWin();

        _titleText.text = winner switch
        {
            Player.Black => "흑돌 승리!",
            Player.White => "백돌 승리!",
            _ => "무승부"
        };
        _subText.text = winner == Player.None ? "모든 칸이 채워졌습니다." : "5목 완성!";
        _moveText.text = $"총 {GameManager.Instance.Turn.MoveCount}수";

        StopAllCoroutines();
        StartCoroutine(ResultFlow());
    }

    // ── 전체 흐름 ────────────────────────────────
    private IEnumerator ResultFlow()
    {
        // 1. 결과 표시
        ShowView(_resultView);
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(_resultDisplayTime);
        yield return StartCoroutine(FadeOut());

        // 2. 모든 모드 재대결 뷰로
        ShowView(_rematchView);
        yield return StartCoroutine(FadeIn());
    }

    // ── 재대결 ───────────────────────────────────
    private void OnRematchYes()
    {
        gameObject.SetActive(false);

        var mode = GameManager.Instance.CurrentMode;
        // 싱글 → 그대로 싱글 재시작
        // AI   → 같은 난이도/색상 그대로 재시작
        // 멀티 → 추후 구현
        GameManager.Instance.StartGame(mode);
    }

    private void OnRematchNo()
    {
        StartCoroutine(TransitionToPostGame());
    }

    private IEnumerator TransitionToPostGame()
    {
        yield return StartCoroutine(FadeOut());
        ShowView(_postGameView);
        yield return StartCoroutine(FadeIn());
    }

    // ── PostGame ─────────────────────────────────
    private void OnMainMenu()
    {
        gameObject.SetActive(false);
        AudioManager.Instance?.PlayMenuBGM();
        UIManager.Instance.ShowMainMenu();
    }

    private void OnStartGame()
    {
        // 모든 모드 → 모드 선택으로
        gameObject.SetActive(false);
        AudioManager.Instance?.PlayMenuBGM();
        UIManager.Instance.ShowMainMenu();
        FindAnyObjectByType<MainMenuUI>()?.GoToGameMode();
    }

    // ── 뷰 전환 ──────────────────────────────────
    private void ShowView(GameObject target)
    {
        _resultView.SetActive(target == _resultView);
        _rematchView.SetActive(target == _rematchView);
        _postGameView.SetActive(target == _postGameView);

        _rematchYesBtn.interactable = true;
        _rematchNoBtn.interactable = true;
    }

    // ── 페이드 ───────────────────────────────────
    private IEnumerator FadeIn()
    {
        _cg.alpha = 0f;
        _cg.interactable = false;
        _cg.blocksRaycasts = false;

        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
        {
            _cg.alpha = Mathf.Lerp(0f, _maxAlpha, t);
            yield return null;
        }

        _cg.alpha = _maxAlpha;
        _cg.interactable = true;
        _cg.blocksRaycasts = true;
    }

    private IEnumerator FadeOut()
    {
        _cg.interactable = false;
        _cg.blocksRaycasts = false;

        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
        {
            _cg.alpha = Mathf.Lerp(_maxAlpha, 0f, t);
            yield return null;
        }

        _cg.alpha = 0f;
    }
}