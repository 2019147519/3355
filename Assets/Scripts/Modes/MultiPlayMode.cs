// 뒤끝매치 1:1 온라인 대국 모드
public class MultiPlayMode : IGameMode
{
    private GameManager _gm;

    public void Initialize(GameManager gm) => _gm = gm;

    public void OnTurnStart(Player current)
    {
        var online = OnlineMatchManager.Instance;
        _gm.SetInput(online != null && online.IsInGameRoom && current == online.LocalPlayer);
    }

    public void OnStonePlace(int row, int col, Player player)
    {
        var online = OnlineMatchManager.Instance;
        if (online == null || online.IsApplyingRemoteMove) return;
        online.SendMove(row, col, player);
    }

    public void OnGameEnd(Player winner)
    {
        _gm.SetInput(false);
    }
}
