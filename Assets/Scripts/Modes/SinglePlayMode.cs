// Assets/Scripts/Modes/SinglePlayMode.cs
// 한 화면에서 두 명이 번갈아 착수 — 별도 네트워크 없음
public class SinglePlayMode : IGameMode
{
    private GameManager _gm;

    public void Initialize(GameManager gm) => _gm = gm;

    public void OnTurnStart(Player currentPlayer)
    {
        // HUD에 현재 플레이어 표시만 하면 됨
        // UI 이벤트는 GameManager가 브로드캐스트
    }

    public void OnStonePlace(int row, int col, Player player) { }

    public void OnGameEnd(Player winner)
    {
        // 결과 UI 표시는 GameManager 담당
    }

    public void Cleanup() { }
}