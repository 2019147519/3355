// Assets/Scripts/Modes/AIPlayMode.cs
using System.Collections;
using UnityEngine;

public class AIPlayMode : IGameMode
{
    private GameManager _gm;
    private OmokAI _ai;
    private readonly int _difficulty;

    public Player AIPlayer { get; private set; }
    public Player HumanPlayer { get; private set; }

    public AIPlayMode(int difficulty = 2, Player aiPlayer = Player.White)
    {
        _difficulty = difficulty;
        AIPlayer = aiPlayer;
        HumanPlayer = aiPlayer == Player.White ? Player.Black : Player.White;
    }

    public void Initialize(GameManager gm)
    {
        _gm = gm;
        _ai = new OmokAI();
        _ai.Setup((int)AIPlayer, _difficulty);
    }

    public void OnTurnStart(Player current)
    {
        if (current == AIPlayer)
        {
            _gm.SetInput(false);
            _gm.RunCoroutine(Think()); // ★ RunCoroutine으로 변경
        }
        else
        {
            _gm.SetInput(true);
        }
    }

    public void OnStonePlace(int row, int col, Player player) { }

    public void OnGameEnd(Player winner)
    {
        _gm.SetInput(false);
    }

    private IEnumerator Think()
    {
        yield return new WaitForSeconds(0.4f);
        var board = _gm.Board.GetCopy();
        var (r, c) = _ai.GetBestMove(board);
        _gm.OnBoardTapped(r, c);
    }
}