// Assets/Scripts/Modes/AIPlayMode.cs
using System.Collections;
using UnityEngine;

public class AIPlayMode : IGameMode
{
    private GameManager _gm;
    private OmokAI _ai;
    private readonly Player _aiPlayer = Player.White;
    private readonly int _difficulty;

    public AIPlayMode(int difficulty = 2) => _difficulty = difficulty;

    public void Initialize(GameManager gm)
    {
        _gm = gm;
        _ai = new OmokAI();
        _ai.Setup((int)_aiPlayer, _difficulty);
    }

    public void OnTurnStart(Player current)
    {
        if (current == _aiPlayer)
            _gm.StartCoroutine(Think());
    }

    public void OnStonePlace(int row, int col, Player player) { }
    public void OnGameEnd(Player winner) { }

    private IEnumerator Think()
    {
        _gm.SetInput(false);
        yield return new WaitForSeconds(0.4f);

        var board = _gm.Board.GetCopy();
        var (r, c) = _ai.GetBestMove(board, (int)_aiPlayer);

        _gm.OnBoardTapped(r, c);
        _gm.SetInput(true);
    }
}