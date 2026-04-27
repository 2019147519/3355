// Assets/Scripts/Modes/IGameMode.cs
public interface IGameMode
{
    void Initialize(GameManager gm);
    void OnTurnStart(Player current);
    void OnStonePlace(int row, int col, Player player);
    void OnGameEnd(Player winner);
}