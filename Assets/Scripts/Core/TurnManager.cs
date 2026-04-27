// Assets/Scripts/Core/TurnManager.cs
using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public Player Current { get; private set; } = Player.Black;
    public int MoveCount { get; private set; }

    public event Action<Player> OnTurnChanged;

    public void Reset()
    {
        Current = Player.Black;
        MoveCount = 0;
    }

    public void Next()
    {
        Current = Current == Player.Black ? Player.White : Player.Black;
        MoveCount++;
        OnTurnChanged?.Invoke(Current);
    }

    public void Revert()
    {
        Current = Current == Player.Black ? Player.White : Player.Black;
        MoveCount = Mathf.Max(0, MoveCount - 1);
        OnTurnChanged?.Invoke(Current);
    }
}