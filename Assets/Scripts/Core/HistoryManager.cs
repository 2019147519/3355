// Assets/Scripts/Core/HistoryManager.cs
using System.Collections.Generic;

public class HistoryManager
{
    private readonly Stack<MoveRecord> _history = new();

    public bool CanUndo => _history.Count > 0;

    public void Record(int row, int col, int player)
        => _history.Push(new MoveRecord(row, col, player));

    public MoveRecord Undo()
        => _history.Count > 0 ? _history.Pop() : null;

    public void Clear() => _history.Clear();

    public int MoveCount => _history.Count;
}

public class MoveRecord
{
    public int Row { get; }
    public int Col { get; }
    public int Player { get; }

    public MoveRecord(int row, int col, int player)
    {
        Row = row;
        Col = col;
        Player = player;
    }
}