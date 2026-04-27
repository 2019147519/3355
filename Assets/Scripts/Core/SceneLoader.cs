// Assets/Scripts/Core/SceneLoader.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private GameObject _loadingOverlay;
    [SerializeField] private Image _fadeImage;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
        => StartCoroutine(LoadRoutine(sceneName));

    private IEnumerator LoadRoutine(string sceneName)
    {
        // 페이드아웃
        yield return Fade(0f, 1f, 0.3f);

        _loadingOverlay.SetActive(true);
        var op = SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone) yield return null;

        _loadingOverlay.SetActive(false);

        // 페이드인
        yield return Fade(1f, 0f, 0.3f);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        var color = _fadeImage.color;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            color.a = Mathf.Lerp(from, to, t);
            _fadeImage.color = color;
            yield return null;
        }
    }
}