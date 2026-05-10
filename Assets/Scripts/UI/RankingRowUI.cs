using TMPro;
using UnityEngine;

public class RankingRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    public void Set(int rank, string nickname, int score)
    {
        _rankText.text = rank.ToString();
        _nicknameText.text = string.IsNullOrWhiteSpace(nickname) ? "-" : nickname;
        _scoreText.text = score.ToString();
    }
}
