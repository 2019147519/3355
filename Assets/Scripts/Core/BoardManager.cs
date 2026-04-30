// Assets/Scripts/Core/BoardManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public const int Size = 15;
    public int[,] Board { get; private set; }

    private readonly Stack<(int r, int c, int p)> _history = new();

    public event Action<int, int, int> OnStonePlaced;
    public event Action<int, int> OnStoneRemoved;
    public event Action<int, int, ForbiddenType> OnForbiddenMove;

    private void Awake() => Init();

    public void Init()
    {
        Board = new int[Size, Size];
        _history.Clear();
    }

    public bool TryPlace(int row, int col, int player)
    {
        if (row < 0 || row >= Size || col < 0 || col >= Size) return false;
        if (Board[row, col] != 0) return false;

        // ★ 흑 금수 체크 — 복사본으로 검사해서 원본 오염 방지
        if (player == 1)
        {
            var copy = GetCopy();
            var forbidden = RenjuRule.GetForbiddenType(copy, row, col);
            if (forbidden != ForbiddenType.None)
            {
                OnForbiddenMove?.Invoke(row, col, forbidden);
                return false;
            }
        }

        Board[row, col] = player;
        _history.Push((row, col, player));
        OnStonePlaced?.Invoke(row, col, player);
        return true;
    }

    public bool Undo(out int row, out int col, out int player)
    {
        row = col = player = 0;
        if (_history.Count == 0) return false;
        (row, col, player) = _history.Pop();
        Board[row, col] = 0;
        OnStoneRemoved?.Invoke(row, col);
        return true;
    }

    public bool CheckWin(int r, int c, int p) => WinChecker.CheckWin(Board, r, c, p);
    public List<(int, int)> GetWinLine(int r, int c, int p) => WinChecker.GetWinLine(Board, r, c, p);
    public bool IsFull() => WinChecker.IsBoardFull(Board);

    public int[,] GetCopy()
    {
        var copy = new int[Size, Size];
        Array.Copy(Board, copy, Board.Length);
        return copy;
    }

    public int MoveCount => _history.Count;
}