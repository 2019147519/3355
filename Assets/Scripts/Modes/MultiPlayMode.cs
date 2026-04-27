// Assets/Scripts/Modes/MultiPlayMode.cs
// 네트워크 연결 ? 3단계에서 구현, 지금은 인터페이스만
public class MultiPlayMode : IGameMode
{
    public void Initialize(GameManager gm) { }
    public void OnTurnStart(Player p) { }
    public void OnStonePlace(int r, int c, Player p) { }
    public void OnGameEnd(Player winner) { }
    public void Cleanup() { }
}