// Assets/Scripts/UI/ToastUI.cs
using System.Collections;
using UnityEngine;
using TMPro;

public class ToastUI : MonoBehaviour
{
    private static ToastUI _instance;

    [SerializeField] private GameObject      _toastRoot;
    [SerializeField] private TextMeshProUGUI _toastText;
    [SerializeField] private CanvasGroup     _canvasGroup;

    private Coroutine _current;

    private void Awake() => _instance = this;

    public static void Show(string message, float duration = 2f)
    {
        if (_instance == null) return;
        if (_instance._current != null)
            _instance.StopCoroutine(_instance._current);
        _instance._current = _instance.StartCoroutine(
            _instance.ShowRoutine(message, duration));
    }

    private IEnumerator ShowRoutine(string message, float duration)
    {
        _toastText.text    = message;
        _toastRoot.SetActive(true);
        _canvasGroup.alpha = 0f;

        // 페이드인
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        // 페이드아웃
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.3f;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        _toastRoot.SetActive(false);
    }
}