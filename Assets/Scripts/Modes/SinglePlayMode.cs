// Assets/Scripts/Modes/SinglePlayMode.cs
// 한 화면에서 두 명이 번갈아 착수 — 별도 네트워크 없음
public class SinglePlayMode : IGameMode
{
    private GameManager _gm;

    public void Initialize(GameManager gm) => _gm = gm;

    public void OnTurnStart(Player current)
        => _gm.SetInput(true); // ★ 항상 입력 허용

    public void OnStonePlace(int row, int col, Player player) { }
    public void OnGameEnd(Player winner) => _gm.SetInput(false);
}