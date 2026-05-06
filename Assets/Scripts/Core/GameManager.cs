// Assets/Scripts/Core/GameManager.cs
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core")]
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
    private Player _aiColor = Player.White;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartGame(GameMode mode)
    {
        // ★ 이전 게임의 잔여 코루틴 전부 정리
        StopAllCoroutines();

        CurrentMode = mode;
        _board.Init();
        _turn.Reset();
        _stone.ClearAll();
        _effect.ClearWinLine();
        _state = GameState.Playing;

        _board.OnForbiddenMove -= OnForbiddenMove;
        _board.OnForbiddenMove += OnForbiddenMove;

        _mode = mode switch
        {
            GameMode.AI => new AIPlayMode(_aiDiff, _aiColor),
            GameMode.Multi => new MultiPlayMode(),
            _ => new SinglePlayMode()
        };
        _mode.Initialize(this);

        if (mode != GameMode.AI)
            SetInput(true);

        AudioManager.Instance?.PlayGameBGM();
        FireTurn();
    }

    public void SetAIDifficulty(int level) => _aiDiff = level;
    public void SetAIColor(Player color) => _aiColor = color;

    public void OnBoardTapped(int row, int col)
    {
        if (_state != GameState.Playing) return;

        int player = (int)_turn.Current;
        if (!_board.TryPlace(row, col, player)) return;

        OnMoveMade?.Invoke(row, col, player);
        _mode.OnStonePlace(row, col, _turn.Current);
        
        AudioManager.Instance?.PlayStone();

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

    public void RequestUndo()
    {
        if (_state != GameState.Playing) return;

        if (CurrentMode == GameMode.AI)
        {
            var aiMode = _mode as AIPlayMode;
            if (aiMode == null) return;

            // 현재가 사람 차례 = 직전이 AI 수
            bool aiJustMoved = _turn.Current == aiMode.HumanPlayer;

            if (aiJustMoved)
            {
                if (!_board.Undo(out _, out _, out _)) return;
                _turn.Revert();
            }

            // 사람 수 제거
            if (!_board.Undo(out _, out _, out _)) return;
            _turn.Revert();
        }
        else
        {
            if (!_board.Undo(out _, out _, out _)) return;
            _turn.Revert();
        }

        FireTurn();
    }

    public void OnTimeOut()
    {
        if (_state != GameState.Playing) return;
        ToastUI.Show("시간 초과! 턴을 넘깁니다.");
        _turn.Next();
        FireTurn();
    }

    private void EndGame(Player winner)
    {
        _state = GameState.GameOver;
        StopAllCoroutines(); // ★ DelayedModeStart 등 잔여 코루틴 정리
        SetInput(false);
        _mode.OnGameEnd(winner);
        OnGameOver?.Invoke(winner);
        _resultUI.Show(winner);
    }


    private void FireTurn()
    {
        OnTurnChanged?.Invoke(_turn.Current);
        // ★ 다음 프레임에 모드 OnTurnStart 호출 — UI 처리 완료 후 AI 코루틴 시작 보장
        StartCoroutine(DelayedModeStart(_turn.Current));
    }

    private System.Collections.IEnumerator DelayedModeStart(Player current)
    {
        yield return null;
        if (_state == GameState.Playing)
            _mode.OnTurnStart(current);
    }

    private void OnForbiddenMove(int row, int col, ForbiddenType type)
    {
        AudioManager.Instance?.PlayForbidden(); // ★
        string msg = type switch
        {
            ForbiddenType.DoubleThree => "3-3 금수입니다!",
            ForbiddenType.DoubleFour => "4-4 금수입니다!",
            ForbiddenType.Overline => "장목(6목 이상) 금수입니다!",
            _ => "금수입니다!"
        };
        ToastUI.Show(msg);
    }

    public void SetInput(bool on) => _input.SetEnabled(on);

    // ★ 오버라이드 제거, RunCoroutine으로 대체
    public void RunCoroutine(System.Collections.IEnumerator routine)
        => StartCoroutine(routine);

    private void OnDestroy()
    {
        if (_board != null)
            _board.OnForbiddenMove -= OnForbiddenMove;
    }
}