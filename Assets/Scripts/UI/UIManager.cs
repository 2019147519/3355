// Assets/Scripts/UI/UIManager.cs
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _gameHUDPanel;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private GameObject _pausePanel;

    private readonly Stack<GameObject> _stack = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _mainMenuPanel.SetActive(true);
        _gameHUDPanel.SetActive(false);
        _pausePanel.SetActive(false);
        _stack.Push(_mainMenuPanel);
    }

    public void ShowMainMenu() => SwitchTo(_mainMenuPanel);
    public void ShowGameHUD() => SwitchTo(_gameHUDPanel);
    public void ShowResult() => Push(_resultPanel);
    public void ShowPause() => Push(_pausePanel);

    public void HideTop()
    {
        if (_stack.Count == 0) return;
        _stack.Pop().SetActive(false);
        if (_stack.Count > 0) _stack.Peek().SetActive(true);
    }

    private void SwitchTo(GameObject p)
    {
        while (_stack.Count > 0) _stack.Pop().SetActive(false);
        p.SetActive(true);
        _stack.Push(p);
    }

    private void Push(GameObject p)
    {
        if (_stack.Count > 0) _stack.Peek().SetActive(false);
        p.SetActive(true);
        _stack.Push(p);
    }
}