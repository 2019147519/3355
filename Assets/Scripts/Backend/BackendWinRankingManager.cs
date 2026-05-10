using BackEnd;
using LitJson;
using UnityEngine;

public class BackendWinRankingManager : MonoBehaviour
{
    public static BackendWinRankingManager Instance { get; private set; }

    [Header("Console Settings")]
    [SerializeField] private string _tableName = "omok_rank";
    [SerializeField] private string _scoreColumn = "score";
    [SerializeField] private string _winsColumn = "wins";
    [SerializeField] private string _lossesColumn = "losses";
    [SerializeField] private string _nicknameColumn = "nickname";
    [SerializeField] private string _leaderboardTitle = "OmokWins";
    [SerializeField] private string _leaderboardUuid;

    private string _rowInDate;
    private int _cachedScore = -1;
    private int _cachedWins = -1;
    private int _cachedLosses = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ReportOnlineWin()
        => ReportOnlineResult(true);

    public void ReportOnlineLoss()
        => ReportOnlineResult(false);

    private void ReportOnlineResult(bool won)
    {
        if (BackendManager.Instance == null || !BackendManager.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[BackendWinRankingManager] Login is required.");
            return;
        }

        if (!EnsureLeaderboardUuid())
            return;
        if (!EnsureRankingRow())
            return;

        int nextWins = Mathf.Max(0, _cachedWins) + (won ? 1 : 0);
        int nextLosses = Mathf.Max(0, _cachedLosses) + (won ? 0 : 1);
        int nextScore = Mathf.Max(0, Mathf.Max(0, _cachedScore) + (won ? 1 : -1));

        var param = new Param();
        param.Add(_scoreColumn, nextScore);
        param.Add(_winsColumn, nextWins);
        param.Add(_lossesColumn, nextLosses);
        param.Add(_nicknameColumn, BackendManager.Instance.CurrentNickname);

        var bro = Backend.Leaderboard.User.UpdateMyDataAndRefreshLeaderboard(
            _leaderboardUuid,
            _tableName,
            _rowInDate,
            param);

        if (!bro.IsSuccess())
        {
            Debug.LogWarning($"[BackendWinRankingManager] Ranking update failed: {bro}");
            return;
        }

        _cachedScore = nextScore;
        _cachedWins = nextWins;
        _cachedLosses = nextLosses;
        Debug.Log($"[BackendWinRankingManager] Ranking updated. score={nextScore}, wins={nextWins}, losses={nextLosses}");
    }

    private bool EnsureRankingRow()
    {
        if (!string.IsNullOrEmpty(_rowInDate) &&
            _cachedScore >= 0 &&
            _cachedWins >= 0 &&
            _cachedLosses >= 0)
            return true;

        var bro = Backend.GameData.Get(_tableName, new Where());
        if (bro.IsSuccess())
        {
            JsonData rows = bro.FlattenRows();
            if (rows.Count > 0)
            {
                var row = rows[0];
                _rowInDate = row["inDate"].ToString();
                _cachedScore = row.ContainsKey(_scoreColumn)
                    ? int.Parse(row[_scoreColumn].ToString())
                    : 0;
                _cachedWins = row.ContainsKey(_winsColumn)
                    ? int.Parse(row[_winsColumn].ToString())
                    : 0;
                _cachedLosses = row.ContainsKey(_lossesColumn)
                    ? int.Parse(row[_lossesColumn].ToString())
                    : 0;
                return true;
            }
        }
        else if (bro.GetStatusCode() != "404")
        {
            Debug.LogWarning($"[BackendWinRankingManager] Ranking row lookup failed: {bro}");
            return false;
        }

        var param = new Param();
        param.Add(_scoreColumn, 0);
        param.Add(_winsColumn, 0);
        param.Add(_lossesColumn, 0);
        param.Add(_nicknameColumn, BackendManager.Instance.CurrentNickname);

        var insertBro = Backend.GameData.Insert(_tableName, param);
        if (!insertBro.IsSuccess())
        {
            Debug.LogWarning($"[BackendWinRankingManager] Ranking row insert failed: {insertBro}");
            return false;
        }

        _rowInDate = insertBro.GetInDate();
        _cachedScore = 0;
        _cachedWins = 0;
        _cachedLosses = 0;
        return true;
    }

    private bool EnsureLeaderboardUuid()
    {
        if (!string.IsNullOrWhiteSpace(_leaderboardUuid))
            return true;

        var bro = Backend.Leaderboard.User.GetLeaderboards();
        if (!bro.IsSuccess())
        {
            Debug.LogWarning($"[BackendWinRankingManager] Leaderboard lookup failed: {bro}");
            return false;
        }

        foreach (var item in bro.GetLeaderboardTableList())
        {
            if (item.title != _leaderboardTitle) continue;
            _leaderboardUuid = item.uuid;
            return true;
        }

        Debug.LogWarning($"[BackendWinRankingManager] Leaderboard not found. title={_leaderboardTitle}");
        return false;
    }
}
