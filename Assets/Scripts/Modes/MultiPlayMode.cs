// Assets/Scripts/Modes/MultiPlayMode.cs
// 네트워크 연결 ? 3단계에서 구현, 지금은 인터페이스만
public class MultiPlayMode : IGameMode
{
    private GameManager _gm;

    public void Initialize(GameManager gm) => _gm = gm;

    public void OnTurnStart(Player current)
        => _gm.SetInput(true); // 멀티 구현 전까지는 동일

    public void OnStonePlace(int row, int col, Player player) { }
    public void OnGameEnd(Player winner) => _gm.SetInput(false);
}