using BackEnd;
using UnityEngine;
using UnityEngine.UI;

public class RankingPanelUI : MonoBehaviour
{
    [SerializeField] private Transform _content;
    [SerializeField] private RankingRowUI _rowPrefab;
    [SerializeField] private RankingRowUI _myRankRow;
    [SerializeField] private Button _closeButton;
    [SerializeField] private string _leaderboardUuid;
    [SerializeField] private int _limit = 50;
    [SerializeField] private float _rowHeight = 64f;
    [SerializeField] private float _rowSpacing = 8f;

    private void Awake()
    {
        _closeButton?.onClick.AddListener(Close);
        EnsureContentLayout();
    }

    private void OnDestroy()
    {
        _closeButton?.onClick.RemoveListener(Close);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        Clear();
        EnsureContentLayout();

        if (string.IsNullOrWhiteSpace(_leaderboardUuid))
        {
            Debug.LogWarning("[RankingPanelUI] Leaderboard uuid is empty.");
            RefreshMyRankFallback();
            return;
        }
        if (_content == null || _rowPrefab == null)
        {
            Debug.LogWarning("[RankingPanelUI] Content or row prefab is not assigned.");
            RefreshMyRankFallback();
            return;
        }

        var bro = Backend.Leaderboard.User.GetLeaderboard(_leaderboardUuid, _limit);
        if (!bro.IsSuccess())
        {
            Debug.LogWarning($"[RankingPanelUI] Leaderboard load failed: {bro}");
            RefreshMyRankFallback();
            return;
        }

        var rows = bro.GetUserLeaderboardList();
        foreach (var row in rows)
        {
            var item = Instantiate(_rowPrefab, _content);
            EnsureRowLayout(item);
            item.Set(ParseInt(row.rank, 0), row.nickname, ParseInt(row.score, 0));
        }

        RefreshMyRank();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void RefreshMyRank()
    {
        if (_myRankRow == null) return;

        var bro = Backend.Leaderboard.User.GetMyLeaderboard(_leaderboardUuid);
        if (!bro.IsSuccess())
        {
            RefreshMyRankFallback();
            Debug.LogWarning($"[RankingPanelUI] My leaderboard load failed: {bro}");
            return;
        }

        var rows = bro.GetUserLeaderboardList();
        if (rows.Count == 0)
        {
            RefreshMyRankFallback();
            return;
        }

        var row = rows[0];
        _myRankRow.Set(
            ParseInt(row.rank, 0),
            string.IsNullOrWhiteSpace(row.nickname) ? BackendManager.Instance?.CurrentNickname ?? "-" : row.nickname,
            ParseInt(row.score, 0));
    }

    private void RefreshMyRankFallback()
    {
        if (_myRankRow == null) return;
        _myRankRow.Set(0, BackendManager.Instance?.CurrentNickname ?? "-", 0);
    }

    private void Clear()
    {
        if (_content == null) return;

        for (int i = _content.childCount - 1; i >= 0; i--)
            Destroy(_content.GetChild(i).gameObject);
    }

    private static int ParseInt(string value, int fallback)
        => int.TryParse(value, out int parsed) ? parsed : fallback;

    private void EnsureContentLayout()
    {
        if (_content == null) return;

        var rect = _content as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
        }

        var layout = _content.GetComponent<VerticalLayoutGroup>()
                     ?? _content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = _rowSpacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = _content.GetComponent<ContentSizeFitter>()
                     ?? _content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void EnsureRowLayout(RankingRowUI row)
    {
        if (row == null) return;

        var layout = row.GetComponent<LayoutElement>()
                     ?? row.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = _rowHeight;
        layout.preferredHeight = _rowHeight;
        layout.flexibleHeight = 0f;
    }
}
