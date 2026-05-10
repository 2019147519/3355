using System;
using BackEnd;
using UnityEngine;

public class BackendManager : MonoBehaviour
{
    public static BackendManager Instance { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public string CurrentNickname { get; private set; }

    public event Action<string> OnStatus;
    public event Action OnLoginSucceeded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
        EnsureOnlineMatchManager();
        EnsureWinRankingManager();
    }

    private void Update()
    {
        if (IsInitialized)
            Backend.Match.Poll();
    }

    public bool Initialize()
    {
        if (IsInitialized) return true;

        var bro = Backend.Initialize();
        IsInitialized = bro.IsSuccess();

        Report(IsInitialized ? "Backend initialized." : $"Backend initialization failed: {bro}");
        return IsInitialized;
    }

    public void Login(string id, string password)
    {
        if (!ValidateCredentials(id, password)) return;
        if (!Initialize()) return;

        var trimmedId = id.Trim();
        var bro = Backend.BMember.CustomLogin(trimmedId, password);
        if (!bro.IsSuccess())
        {
            Report($"Login failed: {bro.GetMessage()}");
            return;
        }

        CompleteLoginWithoutNicknameUpdate(trimmedId);
    }

    public void SignUp(string id, string password, string passwordConfirm, string nickname)
    {
        if (!ValidateCredentials(id, password)) return;
        if (password != passwordConfirm)
        {
            Report("Password confirmation does not match.");
            return;
        }
        if (!Initialize()) return;

        var trimmedId = id.Trim();
        var bro = Backend.BMember.CustomSignUp(trimmedId, password);
        if (!bro.IsSuccess())
        {
            Report($"Sign-up failed: {bro.GetMessage()}");
            return;
        }

        CompleteSignUp(string.IsNullOrWhiteSpace(nickname) ? trimmedId : nickname.Trim());
    }

    private bool ValidateCredentials(string id, string password)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
        {
            Report("Enter both ID and password.");
            return false;
        }
        return true;
    }

    private void CompleteSignUp(string nickname)
    {
        var nickBro = Backend.BMember.UpdateNickname(nickname);
        if (!nickBro.IsSuccess())
        {
            Report($"Nickname setup failed: {nickBro.GetMessage()}");
            return;
        }

        CompleteLoginState(nickname);
    }

    private void CompleteLoginWithoutNicknameUpdate(string fallbackName)
    {
        string nickname = Backend.UserNickName;
        if (string.IsNullOrWhiteSpace(nickname))
            nickname = ReadNicknameFromServer();
        if (string.IsNullOrWhiteSpace(nickname))
            nickname = fallbackName;

        CompleteLoginState(nickname);
    }

    private string ReadNicknameFromServer()
    {
        var bro = Backend.BMember.GetUserInfo();
        if (!bro.IsSuccess())
            return string.Empty;

        var row = bro.GetReturnValuetoJSON()["row"];
        if (row == null || !row.ContainsKey("nickname") || row["nickname"] == null)
            return string.Empty;

        return row["nickname"].ToString();
    }

    private void CompleteLoginState(string nickname)
    {
        IsLoggedIn = true;
        CurrentNickname = nickname;
        PlayerPrefs.SetString("BackendLastNickname", nickname);
        PlayerPrefs.Save();
        Report($"{nickname} logged in.");
        OnLoginSucceeded?.Invoke();
    }

    private void Report(string message)
    {
        Debug.Log($"[BackendManager] {message}");
        OnStatus?.Invoke(message);
    }

    private static void EnsureOnlineMatchManager()
    {
        if (OnlineMatchManager.Instance != null) return;
        var go = new GameObject("OnlineMatchManager");
        go.AddComponent<OnlineMatchManager>();
    }

    private static void EnsureWinRankingManager()
    {
        if (BackendWinRankingManager.Instance != null) return;
        var go = new GameObject("BackendWinRankingManager");
        go.AddComponent<BackendWinRankingManager>();
    }
}
