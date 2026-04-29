// Assets/Scripts/AI/BoardEvaluator.cs
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class BoardEvaluator
{
    private static readonly (int dr, int dc)[] Dirs = { (0, 1), (1, 0), (1, 1), (1, -1) };


    private static readonly Dictionary<string, int> patterns = new()
    {
        // 0 = 빈칸
        // 1 = 내 돌
        // 2 = 상대 돌 혹은 벽으로 막힌 곳
        
        // =========================
        // 5목 (무조건 승리)
        // =========================
        { "11111", 9_000_000 },

        // =========================
        // 열린 4 (양쪽 열림)
        // =========================
        { "011110", 200_000 },

        // =========================
        // 닫힌 4 (한쪽 막힘)
        // =========================
        { "211110", 20_000 },
        { "011112", 20_000 },

        // =========================
        // 확장 열린 4 (한 칸 띄운 형태)
        // =========================
        { "0110110", 180_000 },
        { "0101110", 180_000 },

        // =========================
        // 열린 3 (양쪽 열림, 최소 2칸 여유)
        // =========================
        { "0011100", 12_000 },
        { "011100", 10_000 },
        { "001110", 10_000 },
        { "010110", 10_000 },
        { "011010", 10_000 },

        // =========================
        // 반열린 3 (한쪽 막힘 or 공간 부족)
        // =========================
        { "211100", 1500 },
        { "001112", 1500 },


        { "210110", 1500 },
        { "011012", 1500 },

        // =========================
        // 열린 2 (양쪽 여유 충분)
        // =========================
        { "001100", 800 },
        { "0010100", 800 },
        { "0001100", 800 },

        // =========================
        // 반열린 2
        // =========================
        { "211000", 100 },
        { "000112", 100 },


        // =========================
        // 약한 연결 (1~2개 기반)
        // =========================
        { "010", 30 },
        { "0010", 20 },
        { "0100", 20 },


        // 필요하면 계속 추가
    };

    public static int Evaluate(int[,] board, int aiPlayer, float defenseWeight = 1.5f)
    {
        int human = aiPlayer == 1 ? 2 : 1;

        int atk = ScoreFor(board, aiPlayer);
        int def = ScoreFor(board, human);

        return atk - (int)(def * defenseWeight);
    }

    public static int ScoreFor(int[,] board, int player)
    {
        int score = 0;
        int n = board.GetLength(0);

        foreach (var line in GetAllLines(board, player, n))
        {
            score += EvaluateFullLine(line);
        }

        return score;
    }

    // Player 돌이 있는 모든 라인을 반환
    public static List<string> GetAllLines(int[,] board, int player, int n)
    {
        List<(int r, int c)> positions = new();
        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++)
            {
                if (board[r, c] == player)
                    positions.Add((r, c));
            }
        }
        bool[][] starts = new bool[6][];
        starts[0] = new bool[n];
        starts[1] = new bool[n];
        starts[2] = new bool[n];
        starts[3] = new bool[n];
        starts[4] = new bool[n];
        starts[5] = new bool[n];

        foreach (var pos in positions)
        {
            starts[0][pos.r] = true;
            starts[1][pos.c] = true;
            if (pos.r >= pos.c)
                starts[2][pos.r - pos.c] = true;
            else
                starts[3][pos.c - pos.r] = true;

            if (pos.r + pos.c >= n - 1)
                starts[4][pos.r + pos.c - n + 1] = true;
            else
                starts[5][pos.r + pos.c] = true;
        }
        List<string> lines = new();

        for (int i = 0; i < n; i++)
        {
            if (starts[0][i])
                lines.Add(GetFullLine(board, i, 0, Dirs[0].dr, Dirs[0].dc, player, n));
        }
        for (int i = 0; i < n; i++)
        {
            if (starts[1][i])
                lines.Add(GetFullLine(board, 0, i, Dirs[1].dr, Dirs[1].dc, player, n));
        }
        for (int i = 0; i < n - 4; i++)
        {
            if (starts[2][i])
                lines.Add(GetFullLine(board, i, 0, Dirs[2].dr, Dirs[2].dc, player, n));
        }
        for (int i = 1; i < n - 4; i++)
        {
            if (starts[3][i])
                lines.Add(GetFullLine(board, 0, i, Dirs[2].dr, Dirs[2].dc, player, n));
        }
        for (int i = 0; i < n - 4; i++)
        {
            if (starts[4][i])
                lines.Add(GetFullLine(board, i, n-1, Dirs[3].dr, Dirs[3].dc, player, n));
        }
        for (int i = 4; i < n - 1; i++)
        {
            if (starts[5][i])
                lines.Add(GetFullLine(board, 0, i, Dirs[3].dr, Dirs[3].dc, player, n));
        }


        return lines;
    }

    private static string GetFullLine(int[,] board, int r, int c, int dr, int dc, int player, int n)
    {
        StringBuilder sb = new();

        sb.Append('2');
        while (r >= 0 && r < n && c >= 0 && c < n)
        {

            if (board[r, c] == 0) sb.Append('0');
            else if (board[r, c] == player) sb.Append('1');
            else sb.Append('2');

            r += dr;
            c += dc;
        }
        sb.Append('2');
        return sb.ToString();
    }

    private static int EvaluateFullLine(string line)
    {
        int score = 0;

        foreach (var kv in patterns.OrderByDescending(p => p.Value))
        {
            int index = 0;

            while ((index = line.IndexOf(kv.Key, index)) != -1)
            {
                score += kv.Value;

                // 겹치는 영역 스킵
                index += kv.Key.Length;
            }
        }

        return score;
    }
}