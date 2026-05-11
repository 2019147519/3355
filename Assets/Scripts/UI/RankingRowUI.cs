using TMPro;
using UnityEngine;

public class RankingRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    public void Set(int rank, string nickname, int score)
    {
        SetRankAndScore(rank, score);

        if (_nicknameText != null)
            _nicknameText.text = string.IsNullOrWhiteSpace(nickname) ? "-" : nickname;
    }

    public void SetRankAndScore(int rank, int score)
    {
        if (_rankText != null)
            _rankText.text = rank.ToString();

        if (_scoreText != null)
            _scoreText.text = score.ToString();
    }
}
