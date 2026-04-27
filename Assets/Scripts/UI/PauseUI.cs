// Assets/Scripts/UI/PauseUI.cs
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button _resumeBtn;
    [SerializeField] private Button _restartBtn;
    [SerializeField] private Button _mainMenuBtn;

    private void OnEnable()
    {
        Time.timeScale = 0f;
        _resumeBtn.onClick.AddListener(OnResume);
        _restartBtn.onClick.AddListener(OnRestart);
        _mainMenuBtn.onClick.AddListener(OnMainMenu);
    }

    private void OnDisable()
    {
        _resumeBtn.onClick.RemoveAllListeners();
        _restartBtn.onClick.RemoveAllListeners();
        _mainMenuBtn.onClick.RemoveAllListeners();
    }

    private void OnResume()
    {
        Time.timeScale = 1f;
        UIManager.Instance.HideTop();
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        UIManager.Instance.HideTop();
        GameManager.Instance.StartGame(GameManager.Instance.CurrentMode);
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        UIManager.Instance.ShowMainMenu();
    }
}