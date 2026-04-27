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

        // ★ SetActive(false) 대신 CanvasGroup으로 숨김
        //   오브젝트는 항상 켜둬서 Awake/코루틴 문제 없앰
        _cg.alpha = 0f;
        _cg.interactable = false;
        _cg.blocksRaycasts = false;
    }

    public void Show(Player winner)
    {
        _titleText.text = winner switch
        {
            Player.Black => "흑돌 승리!",
            Player.White => "백돌 승리!",
            _ => "무승부"
        };
        _subText.text = winner == Player.None
            ? "모든 칸이 채워졌습니다."
            : "5목 완성!";
        _moveText.text = $"총 {GameManager.Instance.Turn.MoveCount}수";

        // ★ 오브젝트가 항상 켜져 있으므로 코루틴 바로 실행 가능
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        _cg.interactable = false;
        _cg.blocksRaycasts = false;
        _cg.alpha = 0f;

        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
        {
            _cg.alpha = t;
            yield return null;
        }

        _cg.alpha = 1f;
        _cg.interactable = true;
        _cg.blocksRaycasts = true;
    }

    private void Hide()
    {
        StopAllCoroutines();
        _cg.alpha = 0f;
        _cg.interactable = false;
        _cg.blocksRaycasts = false;
    }

    private void OnRematch()
    {
        Hide();
        GameManager.Instance.StartGame(GameManager.Instance.CurrentMode);
    }

    private void OnMenu()
    {
        Hide();
        Time.timeScale = 1f;
        UIManager.Instance.ShowMainMenu();
    }
}