// Assets/Scripts/Core/GameManager.cs
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core — Board 오브젝트의 각 컴포넌트를 드래그")]
    [SerializeField] private BoardManager _board;
    [SerializeField] private TurnManager _turn;
    [SerializeField] private StoneController _stone;
    [SerializeField] private EffectManager _effect;

    [Header("Input")]
    [SerializeField] private InputHandler _input;

    [Header("UI 연결")]
    [SerializeField] private ResultUI _resultUI;

    public BoardManager Board => _board;
    public TurnManager Turn => _turn;
    public GameMode CurrentMode { get; private set; }

    public event Action<Player> OnTurnChanged;
    public event Action<Player> OnGameOver;
    public event Action<int, int, int> OnMoveMade;

    private GameState _state = GameState.MainMenu;
    private IGameMode _mode;
    private int _aiDiff = 2;

    private void Awake()
    {
        // ★ 단일 씬 — DontDestroyOnLoad 없음
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── 외부 진입 ──────────────────────────────
    public void StartGame(GameMode mode)
    {
        CurrentMode = mode;
        _board.Init();
        _turn.Reset();
        _stone.ClearAll();
        _effect.ClearWinLine();
        _state = GameState.Playing;

        _board.OnForbiddenMove += OnForbiddenMove;

        _mode = mode switch
        {
            GameMode.AI => new AIPlayMode(_aiDiff),
            GameMode.Multi => new MultiPlayMode(),
            _ => new SinglePlayMode()
        };
        _mode.Initialize(this);

        SetInput(true);
        FireTurn();
    }

    public void SetAIDifficulty(int level) => _aiDiff = level;

    // ── 착수 ───────────────────────────────────
    public void OnBoardTapped(int row, int col)
    {
        if (_state != GameState.Playing) return;

        int player = (int)_turn.Current;
        if (!_board.TryPlace(row, col, player)) return;

        OnMoveMade?.Invoke(row, col, player);
        _mode.OnStonePlace(row, col, _turn.Current);

        if (_board.CheckWin(row, col, player))
        {
            _effect.ShowWinLine(_board.GetWinLine(row, col, player));
            EndGame(_turn.Current);
            return;
        }

        if (_board.IsFull()) { EndGame(Player.None); return; }

        _turn.Next();
        FireTurn();
    }

    private void OnForbiddenMove(int row, int col, ForbiddenType type)
    {
        string msg = type switch
        {
            ForbiddenType.DoubleThree => "3-3 금수입니다!",
            ForbiddenType.DoubleFour => "4-4 금수입니다!",
            ForbiddenType.Overline => "장목(6목 이상) 금수입니다!",
            _ => "금수입니다!"
        };
        ToastUI.Show(msg);
    }

    private void OnDestroy()
    {
        if (_board != null)
            _board.OnForbiddenMove -= OnForbiddenMove;
    }

    // ── 무르기 ─────────────────────────────────
    public void RequestUndo()
    {
        if (_state != GameState.Playing) return;
        if (!_board.Undo(out _, out _, out _)) return;

        if (CurrentMode == GameMode.AI)
            _board.Undo(out _, out _, out _); // AI 수도 같이 무르기

        _turn.Revert();
        FireTurn();
    }

    // ── 타임아웃 ───────────────────────────────
    public void OnTimeOut()
    {
        if (_state != GameState.Playing) return;
        ToastUI.Show("시간 초과! 턴을 넘깁니다.");
        _turn.Next();
        FireTurn();
    }

    // ── 종료 ───────────────────────────────────
    private void EndGame(Player winner)
    {
        _state = GameState.GameOver;
        SetInput(false);
        _mode.OnGameEnd(winner);
        OnGameOver?.Invoke(winner);
        _resultUI.Show(winner);
    }

    private void FireTurn()
    {
        OnTurnChanged?.Invoke(_turn.Current);
        _mode.OnTurnStart(_turn.Current);
    }

    public void SetInput(bool on) => _input.SetEnabled(on);

    // AIPlayMode가 Coroutine 시작할 때 사용
    public new Coroutine StartCoroutine(System.Collections.IEnumerator r)
        => base.StartCoroutine(r);


}