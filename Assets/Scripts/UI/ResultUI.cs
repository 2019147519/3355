// Assets/Scripts/UI/ResultUI.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _subText;
    [SerializeField] private TextMeshProUGUI _moveText;
    [SerializeField] private Button _rematchBtn;
    [SerializeField] private Button _menuBtn;
    [SerializeField] private CanvasGroup _cg;

    private void Awake()
    {
        _rematchBtn.onClick.AddListener(OnRematch);
        _menuBtn.onClick.AddListener(OnMenu);
        gameObject.SetActive(false); // ¡Ú ÆíÁý ½Ã ²¨Áø Ã¤·Î
    }

    public void Show(Player winner)
    {
        gameObject.SetActive(true); // ¡Ú ÇÊ¿äÇÒ ¶§¸¸ ÄÔ
        AudioManager.Instance?.PlayWin();

        _titleText.text = winner switch
        {
            Player.Black => "Èæµ¹ ½Â¸®!",
            Player.White => "¹éµ¹ ½Â¸®!",
            _ => "¹«½ÂºÎ"
        };
        _subText.text = winner == Player.None
            ? "¸ðµç Ä­ÀÌ Ã¤¿öÁ³½À´Ï´Ù."
            : "5¸ñ ¿Ï¼º!";
        _moveText.text = $"ÃÑ {GameManager.Instance.Turn.MoveCount}¼ö";

        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        _cg.alpha = 0f;
        _cg.interactable = false;
        _cg.blocksRaycasts = false;

        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
        {
            _cg.alpha = t;
            yield return null;
        }

        _cg.alpha = 1f;
        _cg.interactable = true;
        _cg.blocksRaycasts = true;
    }

    private void OnRematch()
    {
        gameObject.SetActive(false);
        GameManager.Instance.StartGame(GameManager.Instance.CurrentMode);
    }

    private void OnMenu()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        UIManager.Instance.ShowMainMenu();
    }
}