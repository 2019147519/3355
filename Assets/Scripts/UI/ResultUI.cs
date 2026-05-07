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
    [SerializeField] private float _resultDisplayTime = 2.5f; // 자동 사라지는 시간

    [Header("Rematch View (멀티 전용)")]
    [SerializeField] private GameObject _rematchView;
    [SerializeField] private Button _rematchYesBtn;
    [SerializeField] private Button _rematchNoBtn;
    [SerializeField] private TextMeshProUGUI _rematchStatusText; // "상대방 응답 대기중..."

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

        // 텍스트 설정
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
        // 1. ResultView 페이드인
        ShowView(_resultView);
        yield return StartCoroutine(FadeIn());

        // 2. n초 표시 후 페이드아웃
        yield return new WaitForSeconds(_resultDisplayTime);
        yield return StartCoroutine(FadeOut());

        // 3. 모드별 다음 단계
        if (GameManager.Instance.CurrentMode == GameMode.Multi)
        {
            // 멀티 → 재대결 뷰
            ShowView(_rematchView);
            _rematchStatusText.text = "";
            yield return StartCoroutine(FadeIn());
        }
        else
        {
            // 싱글 / AI → 바로 PostGame 뷰
            ShowView(_postGameView);
            yield return StartCoroutine(FadeIn());
        }
    }

    // ── 재대결 버튼 ──────────────────────────────
    private void OnRematchYes()
    {
        // 멀티: 상대 응답 대기 (추후 네트워크 연동)
        _rematchYesBtn.interactable = false;
        _rematchNoBtn.interactable = false;
        _rematchStatusText.text = "상대방 응답 대기 중...";

        // TODO: 상대방 응답 받으면 아래 호출
        // OnRematchResult(true/false)
    }

    private void OnRematchNo()
    {
        StartCoroutine(TransitionToPostGame());
    }

    // 네트워크에서 호출 — 상대 응답 결과
    public void OnRematchResult(bool bothAgreed)
    {
        if (bothAgreed)
        {
            gameObject.SetActive(false);
            GameManager.Instance.StartGame(GameManager.Instance.CurrentMode);
        }
        else
        {
            StartCoroutine(TransitionToPostGame());
        }
    }

    private IEnumerator TransitionToPostGame()
    {
        yield return StartCoroutine(FadeOut());
        ShowView(_postGameView);
        yield return StartCoroutine(FadeIn());
    }

    // ── PostGame 버튼 ────────────────────────────
    private void OnMainMenu()
    {
        gameObject.SetActive(false);
        AudioManager.Instance?.PlayMenuBGM();
        UIManager.Instance.ShowMainMenu();
    }

    private void OnStartGame()
    {
        gameObject.SetActive(false);

        switch (GameManager.Instance.CurrentMode)
        {
            case GameMode.Single:
                // 바로 새 게임
                GameManager.Instance.StartGame(GameMode.Single);
                break;

            case GameMode.AI:
                // 난이도 선택창으로 — MainMenu의 DifficultyPanel 열기
                UIManager.Instance.ShowMainMenu();
                FindAnyObjectByType<MainMenuUI>()?.OpenAIFlow();
                break;

            case GameMode.Multi:
                // 멀티 로비로 (추후 구현)
                UIManager.Instance.ShowMainMenu();
                break;
        }
    }

    // ── 뷰 전환 ─────────────────────────────────
    private void ShowView(GameObject target)
    {
        _resultView.SetActive(target == _resultView);
        _rematchView.SetActive(target == _rematchView);
        _postGameView.SetActive(target == _postGameView);

        // 버튼 인터랙션 초기화
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