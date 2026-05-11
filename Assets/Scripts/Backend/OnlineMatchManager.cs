using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BackEnd;
using BackEnd.Tcp;
using LitJson;
using UnityEngine;

public class OnlineMatchManager : MonoBehaviour
{
    public static OnlineMatchManager Instance { get; private set; }

    [SerializeField] private string _matchCardInDate = "2026-05-09T16:38:26.456Z";
    [SerializeField] private MatchType _matchType = MatchType.Random;
    [SerializeField] private MatchModeType _matchModeType = MatchModeType.OneOnOne;

    public Player LocalPlayer { get; private set; } = Player.None;
    public bool IsMatching { get; private set; }
    public bool IsInGameRoom { get; private set; }
    public bool IsApplyingRemoteMove { get; private set; }
    public int CurrentColorSeed { get; private set; }

    public event Action OnRematchAccepted;
    public event Action OnOpponentDeclinedRematch;

    private string _roomToken;
    private string _blackNickname;
    private string _whiteNickname;
    private bool _sentResult;
    private bool _isFinishingSession;
    private Player _pendingResult = Player.None;
    private bool _hasPendingResult;
    private bool? _localRematchChoice;
    private bool? _remoteRematchChoice;
    private int _localRematchSeed;
    private int _remoteRematchSeed;
    private readonly Dictionary<string, SessionId> _sessionByNickname = new();
    private readonly List<string> _roomNicknames = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        RegisterEvents();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            UnregisterEvents();
    }

    public void StartQuickMatch()
    {
        if (IsMatching)
        {
            ShowMatchStatus("이미 매칭 중입니다.");
            return;
        }

        StartCoroutine(StartQuickMatchRoutine());
    }

    private IEnumerator StartQuickMatchRoutine()
    {
        if (BackendManager.Instance == null || !BackendManager.Instance.IsLoggedIn)
        {
            ToastUI.Show("로그인 후 온라인 매칭을 이용할 수 있습니다.");
            yield break;
        }

        IsMatching = true;
        ShowMatchStatus("이전 연결을 정리하는 중입니다.");
        LeaveMatchConnections();
        yield return new WaitForSeconds(0.5f);

        ResetSessionState();
        IsMatching = true;
        ShowMatchStatus("매칭 서버에 접속합니다.");

        try
        {
            ErrorInfo errorInfo;
            if (!Backend.Match.JoinMatchMakingServer(out errorInfo) ||
                errorInfo.Category != ErrorCode.Success)
            {
                IsMatching = false;
                ShowMatchStatus($"매칭 서버 접속 실패: {errorInfo.Reason}", false);
            }
        }
        catch (Exception e)
        {
            IsMatching = false;
            LeaveMatchConnections();
            ShowMatchStatus("이전 연결을 정리하는 중입니다. 잠시 후 다시 시도해 주세요.", false);
            Debug.LogWarning($"[OnlineMatchManager] JoinMatchMakingServer failed: {e.Message}");
        }
    }

    public void CancelMatchmaking()
    {
        if (!IsMatching) return;

        IsMatching = false;
        LeaveMatchConnections();
        ResetSessionState();
        HideMatchStatus();
        ToastUI.Show("매칭을 취소했습니다.");
    }

    public void SendMove(int row, int col, Player player)
    {
        if (!IsInGameRoom || player != LocalPlayer) return;

        TrySendPacket(new OnlineOmokPacket
        {
            type = OnlineOmokPacket.Move,
            row = row,
            col = col,
            player = (int)player
        });
    }

    public void SendRematchChoice(bool accepted)
    {
        if (!IsInGameRoom)
        {
            if (accepted)
                OnOpponentDeclinedRematch?.Invoke();
            return;
        }

        _localRematchChoice = accepted;
        _localRematchSeed = accepted ? UnityEngine.Random.Range(1, int.MaxValue) : 0;

        if (!TrySendPacket(new OnlineOmokPacket
            {
                type = OnlineOmokPacket.Rematch,
                player = (int)LocalPlayer,
                accepted = accepted,
                seed = _localRematchSeed
            }))
        {
            OnOpponentDeclinedRematch?.Invoke();
            return;
        }

        EvaluateRematchChoices();
    }

    public void SetPendingResult(Player winner)
    {
        _pendingResult = winner;
        _hasPendingResult = true;
    }

    public bool SubmitPendingResult()
    {
        if (!_hasPendingResult) return false;
        bool submitted = SubmitResult(_pendingResult);
        _hasPendingResult = false;
        return submitted;
    }

    public void FinishOnlineSession()
    {
        _isFinishingSession = true;
        bool submittedResult = SubmitPendingResult();

        if (!submittedResult)
            LeaveMatchConnections();

        ResetSessionState();

        if (submittedResult)
            StartCoroutine(ClearFinishingSessionFlag());
        else
            _isFinishingSession = false;
    }

    private bool SubmitResult(Player winner)
    {
        if (!IsInGameRoom || _sentResult ||
            string.IsNullOrEmpty(_blackNickname) ||
            string.IsNullOrEmpty(_whiteNickname) ||
            !_sessionByNickname.TryGetValue(_blackNickname, out var blackSessionId) ||
            !_sessionByNickname.TryGetValue(_whiteNickname, out var whiteSessionId))
            return false;

        var result = new MatchGameResult
        {
            m_winners = new List<SessionId>(),
            m_losers = new List<SessionId>(),
            m_draws = new List<SessionId>()
        };

        if (winner == Player.None)
        {
            result.m_draws.Add(blackSessionId);
            result.m_draws.Add(whiteSessionId);
        }
        else
        {
            var winnerSession = winner == Player.Black ? blackSessionId : whiteSessionId;
            var loserSession = winner == Player.Black ? whiteSessionId : blackSessionId;
            result.m_winners.Add(winnerSession);
            result.m_losers.Add(loserSession);
        }

        _sentResult = true;
        try
        {
            Backend.Match.MatchEnd(result);
        }
        catch (Exception e)
        {
            _sentResult = false;
            Debug.LogWarning($"[OnlineMatchManager] MatchEnd failed: {e.Message}");
            return false;
        }

        IsInGameRoom = false;
        return true;
    }

    private IEnumerator ClearFinishingSessionFlag()
    {
        yield return new WaitForSeconds(2f);
        _isFinishingSession = false;
    }

    private bool TrySendPacket(OnlineOmokPacket packet)
    {
        try
        {
            Backend.Match.SendDataToInGameRoom(
                Encoding.UTF8.GetBytes(JsonUtility.ToJson(packet)));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[OnlineMatchManager] SendDataToInGameRoom failed: {e.Message}");
            IsInGameRoom = false;
            return false;
        }
    }

    private void LeaveMatchConnections()
    {
        try
        {
            Backend.Match.LeaveGameServer();
        }
        catch (Exception e)
        {
            Debug.Log($"[OnlineMatchManager] LeaveGameServer skipped: {e.Message}");
        }

        try
        {
            Backend.Match.LeaveMatchMakingServer();
        }
        catch (Exception e)
        {
            Debug.Log($"[OnlineMatchManager] LeaveMatchMakingServer skipped: {e.Message}");
        }

        IsInGameRoom = false;
    }

    private void ResetSessionState()
    {
        IsMatching = false;
        IsInGameRoom = false;
        _roomToken = null;
        _sentResult = false;
        _hasPendingResult = false;
        _blackNickname = null;
        _whiteNickname = null;
        LocalPlayer = Player.None;
        _sessionByNickname.Clear();
        _roomNicknames.Clear();
        ResetRematchState();
    }

    private void RegisterEvents()
    {
        Backend.Match.OnJoinMatchMakingServer += OnJoinMatchMakingServer;
        Backend.Match.OnMatchMakingRoomCreate += OnMatchMakingRoomCreate;
        Backend.Match.OnMatchMakingResponse += OnMatchMakingResponse;
        Backend.Match.OnSessionJoinInServer += OnSessionJoinInServer;
        Backend.Match.OnSessionListInServer += OnSessionListInServer;
        Backend.Match.OnMatchInGameAccess += OnMatchInGameAccess;
        Backend.Match.OnMatchInGameStart += OnMatchInGameStart;
        Backend.Match.OnMatchRelay += OnMatchRelay;
        Backend.Match.OnMatchResult += OnMatchResult;
        Backend.Match.OnSessionOffline += OnSessionOffline;
        Backend.Match.OnLeaveInGameServer += OnLeaveInGameServer;
    }

    private void UnregisterEvents()
    {
        Backend.Match.OnJoinMatchMakingServer -= OnJoinMatchMakingServer;
        Backend.Match.OnMatchMakingRoomCreate -= OnMatchMakingRoomCreate;
        Backend.Match.OnMatchMakingResponse -= OnMatchMakingResponse;
        Backend.Match.OnSessionJoinInServer -= OnSessionJoinInServer;
        Backend.Match.OnSessionListInServer -= OnSessionListInServer;
        Backend.Match.OnMatchInGameAccess -= OnMatchInGameAccess;
        Backend.Match.OnMatchInGameStart -= OnMatchInGameStart;
        Backend.Match.OnMatchRelay -= OnMatchRelay;
        Backend.Match.OnMatchResult -= OnMatchResult;
        Backend.Match.OnSessionOffline -= OnSessionOffline;
        Backend.Match.OnLeaveInGameServer -= OnLeaveInGameServer;
    }

    private void OnJoinMatchMakingServer(JoinChannelEventArgs args)
    {
        if (!IsMatching) return;

        if (args.ErrInfo.Category != ErrorCode.Success)
        {
            IsMatching = false;
            ShowMatchStatus($"매칭 서버 인증 실패: {args.ErrInfo.Reason}", false);
            return;
        }

        ShowMatchStatus("대기방을 생성합니다.");
        Backend.Match.CreateMatchRoom();
    }

    private void OnMatchMakingRoomCreate(MatchMakingInteractionEventArgs args)
    {
        if (!IsMatching) return;

        if (args.ErrInfo != ErrorCode.Success)
        {
            IsMatching = false;
            ShowMatchStatus($"대기방 생성 실패: {args.Reason}", false);
            return;
        }

        var inDate = ResolveMatchCardInDate();
        if (string.IsNullOrEmpty(inDate))
        {
            IsMatching = false;
            ShowMatchStatus("매치를 찾지 못했습니다.", false);
            return;
        }

        ShowMatchStatus("상대를 찾는 중입니다.");
        Backend.Match.RequestMatchMaking(_matchType, _matchModeType, inDate);
    }

    private void OnMatchMakingResponse(MatchMakingResponseEventArgs args)
    {
        if (!IsMatching) return;

        if (args.ErrInfo == ErrorCode.Match_InProgress)
        {
            ShowMatchStatus("매칭 대기 중입니다.");
            return;
        }

        if (args.ErrInfo != ErrorCode.Success)
        {
            IsMatching = false;
            ShowMatchStatus($"매칭 실패: {args.Reason}", false);
            return;
        }

        _roomToken = args.RoomInfo.m_inGameRoomToken;
        ShowMatchStatus("상대를 찾았습니다. 게임 서버에 접속합니다.", false);

        var endpoint = args.RoomInfo.m_inGameServerEndPoint;
        try
        {
            ErrorInfo errorInfo;
            if (!Backend.Match.JoinGameServer(endpoint.m_address, endpoint.m_port, false, out errorInfo) ||
                errorInfo.Category != ErrorCode.Success)
            {
                IsMatching = false;
                ShowMatchStatus($"게임 서버 접속 실패: {errorInfo.Reason}", false);
            }
        }
        catch (Exception e)
        {
            IsMatching = false;
            LeaveMatchConnections();
            ShowMatchStatus("게임 서버 접속에 실패했습니다.", false);
            Debug.LogWarning($"[OnlineMatchManager] JoinGameServer failed: {e.Message}");
        }
    }

    private void OnSessionJoinInServer(JoinChannelEventArgs args)
    {
        if (!IsMatching) return;

        if (args.ErrInfo.Category != ErrorCode.Success)
        {
            IsMatching = false;
            ShowMatchStatus($"게임 서버 인증 실패: {args.ErrInfo.Reason}", false);
            return;
        }

        Backend.Match.JoinGameRoom(_roomToken);
    }

    private void OnSessionListInServer(MatchInGameSessionListEventArgs args)
    {
        if (args.ErrInfo != ErrorCode.Success) return;

        _roomNicknames.Clear();
        foreach (var record in args.GameRecords)
        {
            _roomNicknames.Add(record.m_nickname);
            CacheSession(record);
            if (record.m_nickname == BackendManager.Instance.CurrentNickname)
                AssignLocalPlayerFromSeed(CurrentColorSeed);
        }
    }

    private void OnMatchInGameAccess(MatchInGameSessionEventArgs args)
    {
        if (args.ErrInfo != ErrorCode.Success) return;

        if (!_roomNicknames.Contains(args.GameRecord.m_nickname))
            _roomNicknames.Add(args.GameRecord.m_nickname);

        CacheSession(args.GameRecord);

        if (args.GameRecord.m_nickname == BackendManager.Instance.CurrentNickname)
            AssignLocalPlayerFromSeed(CurrentColorSeed);
    }

    private void OnMatchInGameStart()
    {
        if (!IsMatching) return;

        IsMatching = false;
        IsInGameRoom = true;
        HideMatchStatus();
        _sentResult = false;
        _hasPendingResult = false;
        ResetRematchState();
        CurrentColorSeed = CreateInitialColorSeed();
        AssignLocalPlayerFromSeed(CurrentColorSeed);

        if (LocalPlayer == Player.None)
            AssignLocalPlayerFromSeed(CurrentColorSeed);

        UIManager.Instance.ShowGameHUD();
        GameManager.Instance.StartGame(GameMode.Multi);
        ShowColorToast("온라인 대국 시작");
    }

    private void OnMatchRelay(MatchRelayEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.BinaryUserData);
        var packet = JsonUtility.FromJson<OnlineOmokPacket>(json);
        if (packet == null) return;

        if (packet.type == OnlineOmokPacket.Rematch)
        {
            HandleRematchPacket(packet);
            return;
        }

        if (packet.type != OnlineOmokPacket.Move) return;

        try
        {
            IsApplyingRemoteMove = true;
            GameManager.Instance.OnBoardTapped(packet.row, packet.col);
        }
        finally
        {
            IsApplyingRemoteMove = false;
        }
    }

    private void OnSessionOffline(MatchInGameSessionEventArgs args)
    {
        if (_isFinishingSession || args.ErrInfo != ErrorCode.NetworkOffline)
            return;
        if (IsLocalRecord(args.GameRecord))
            return;

        OnOpponentDeclinedRematch?.Invoke();
    }

    private void OnLeaveInGameServer(MatchInGameSessionEventArgs args)
    {
        if (_isFinishingSession)
            return;

        IsInGameRoom = false;
        IsMatching = false;
    }

    private void HandleRematchPacket(OnlineOmokPacket packet)
    {
        var sender = (Player)packet.player;
        if (sender == LocalPlayer)
            return;

        _remoteRematchChoice = packet.accepted;
        _remoteRematchSeed = packet.seed;
        EvaluateRematchChoices();
    }

    private void EvaluateRematchChoices()
    {
        if (_remoteRematchChoice == false)
        {
            OnOpponentDeclinedRematch?.Invoke();
            ResetRematchState();
            return;
        }

        if (_localRematchChoice == true && _remoteRematchChoice == true)
        {
            CurrentColorSeed = _localRematchSeed ^ _remoteRematchSeed;
            if (CurrentColorSeed == 0)
                CurrentColorSeed = UnityEngine.Random.Range(1, int.MaxValue);
            AssignLocalPlayerFromSeed(CurrentColorSeed);
            ResetRematchState();
            _sentResult = false;
            _hasPendingResult = false;
            OnRematchAccepted?.Invoke();
            ShowColorToast("재대결 시작");
        }
    }

    private void ResetRematchState()
    {
        _localRematchChoice = null;
        _remoteRematchChoice = null;
        _localRematchSeed = 0;
        _remoteRematchSeed = 0;
    }

    private void CacheSession(MatchUserGameRecord record)
    {
        _sessionByNickname[record.m_nickname] = record.m_sessionId;
    }

    private bool IsLocalRecord(MatchUserGameRecord record)
    {
        return BackendManager.Instance != null &&
               record.m_nickname == BackendManager.Instance.CurrentNickname;
    }

    private void OnMatchResult(MatchResultEventArgs args)
    {
        if (args.ErrInfo == ErrorCode.Success)
        {
            Debug.Log("[OnlineMatchManager] Match result saved.");
            return;
        }

        Debug.LogWarning($"[OnlineMatchManager] Match result failed: {args.ErrInfo}, {args.Reason}");
    }

    private void AssignLocalPlayerFromSeed(int seed)
    {
        if (_roomNicknames.Count < 2 || BackendManager.Instance == null)
            return;

        var nicknames = new List<string>(_roomNicknames);
        nicknames.Sort(StringComparer.Ordinal);

        int blackIndex = Mathf.Abs(seed) % 2;
        _blackNickname = nicknames[blackIndex];
        _whiteNickname = nicknames[1 - blackIndex];
        LocalPlayer = BackendManager.Instance.CurrentNickname == _blackNickname
            ? Player.Black
            : Player.White;
    }

    private int CreateInitialColorSeed()
    {
        var nicknames = new List<string>(_roomNicknames);
        nicknames.Sort(StringComparer.Ordinal);

        unchecked
        {
            int hash = 17;
            hash = hash * 31 + StableHash(_roomToken);
            foreach (var nickname in nicknames)
                hash = hash * 31 + StableHash(nickname);
            return hash == 0 ? 1 : hash;
        }
    }

    private static int StableHash(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        unchecked
        {
            int hash = 23;
            for (int i = 0; i < value.Length; i++)
                hash = hash * 31 + value[i];
            return hash;
        }
    }

    private void ShowColorToast(string prefix)
    {
        ToastUI.Show(LocalPlayer == Player.Black
            ? $"{prefix}: 흑돌"
            : $"{prefix}: 백돌");
    }

    private void ShowMatchStatus(string message, bool canCancel = true)
    {
        if (OnlineMatchStatusUI.Instance != null)
            OnlineMatchStatusUI.Instance.Show(message, canCancel);
        else
            ToastUI.Show(message);
    }

    private void HideMatchStatus()
    {
        OnlineMatchStatusUI.Instance?.Hide();
    }

    private string ResolveMatchCardInDate()
    {
        if (!string.IsNullOrWhiteSpace(_matchCardInDate))
            return _matchCardInDate.Trim();

        var bro = Backend.Match.GetMatchList();
        if (!bro.IsSuccess())
        {
            Debug.LogError("Backend.Match.GetMatchList Error: " + bro);
            return null;
        }

        JsonData rows = bro.FlattenRows();
        for (int i = 0; i < rows.Count; i++)
        {
            if (!rows[i].ContainsKey("inDate")) continue;
            if (rows[i].ContainsKey("matchModeType") &&
                rows[i]["matchModeType"].ToString() != "OneOnOne")
                continue;

            _matchCardInDate = rows[i]["inDate"].ToString();
            return _matchCardInDate;
        }

        return null;
    }
}

[Serializable]
public class OnlineOmokPacket
{
    public const string Move = "move";
    public const string Rematch = "rematch";

    public string type;
    public int row;
    public int col;
    public int player;
    public bool accepted;
    public int seed;
}
