using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackendLoginUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private GameObject _signUpPanel;

    [Header("Login")]
    [SerializeField] private TMP_InputField _loginIdInput;
    [SerializeField] private TMP_InputField _loginPasswordInput;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _showSignUpButton;

    [Header("Sign Up")]
    [SerializeField] private TMP_InputField _signUpIdInput;
    [SerializeField] private TMP_InputField _signUpPasswordInput;
    [SerializeField] private TMP_InputField _signUpPasswordConfirmInput;
    [SerializeField] private TMP_InputField _signUpNicknameInput;
    [SerializeField] private Button _signUpButton;
    [SerializeField] private Button _showLoginButton;

    [Header("Status")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private string _nextSceneName = "GameScene";

    private void Awake()
    {
        EnsureBackendManager();
    }

    private void OnEnable()
    {
        if (BackendManager.Instance != null)
        {
            BackendManager.Instance.OnStatus += SetStatus;
            BackendManager.Instance.OnLoginSucceeded += LoadGameScene;
        }

        _loginButton?.onClick.AddListener(OnLogin);
        _showSignUpButton?.onClick.AddListener(ShowSignUp);
        _signUpButton?.onClick.AddListener(OnSignUp);
        _showLoginButton?.onClick.AddListener(ShowLogin);
    }

    private void OnDisable()
    {
        if (BackendManager.Instance != null)
        {
            BackendManager.Instance.OnStatus -= SetStatus;
            BackendManager.Instance.OnLoginSucceeded -= LoadGameScene;
        }

        _loginButton?.onClick.RemoveListener(OnLogin);
        _showSignUpButton?.onClick.RemoveListener(ShowSignUp);
        _signUpButton?.onClick.RemoveListener(OnSignUp);
        _showLoginButton?.onClick.RemoveListener(ShowLogin);
    }

    public void OnLogin()
    {
        BackendManager.Instance.Login(
            _loginIdInput != null ? _loginIdInput.text : string.Empty,
            _loginPasswordInput != null ? _loginPasswordInput.text : string.Empty);
    }

    public void OnSignUp()
    {
        BackendManager.Instance.SignUp(
            _signUpIdInput != null ? _signUpIdInput.text : string.Empty,
            _signUpPasswordInput != null ? _signUpPasswordInput.text : string.Empty,
            _signUpPasswordConfirmInput != null ? _signUpPasswordConfirmInput.text : string.Empty,
            _signUpNicknameInput != null ? _signUpNicknameInput.text : string.Empty);
    }

    public void ShowLogin()
    {
        if (_loginPanel != null) _loginPanel.SetActive(true);
        if (_signUpPanel != null) _signUpPanel.SetActive(false);
    }

    public void ShowSignUp()
    {
        if (_loginPanel != null) _loginPanel.SetActive(false);
        if (_signUpPanel != null) _signUpPanel.SetActive(true);
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
            _statusText.text = message;
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(_nextSceneName);
    }

    private static void EnsureBackendManager()
    {
        if (BackendManager.Instance != null) return;
        var go = new GameObject("BackendManager");
        go.AddComponent<BackendManager>();
    }
}
