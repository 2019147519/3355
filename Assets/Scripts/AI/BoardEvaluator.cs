// Assets/Scripts/AI/BoardEvaluator.cs
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class BoardEvaluator
{
    private static readonly (int dr, int dc)[] Dirs =
        { (0,1),(1,0),(1,1),(1,-1) };


    private static readonly Dictionary<string, int> patterns = new()
    {
        // 0 = 빈칸
        // 1 = 내 돌
        // 2 = 상대 돌
        // 3 = 벽
        
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
        { "311110", 15_000 }, // 벽
        { "011113", 15_000 },

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
        { "311100", 1000 }, // 벽
        { "001113", 1000 },

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
        { "311000", 80 },
        { "000113", 80 },

        // =========================
        // 약한 연결 (1~2개 기반)
        // =========================
        { "010", 30 },
        { "0010", 20 },
        { "0100", 20 },

        // =========================
        // 벽 근처 패널티성 패턴
        // =========================
        { "31110", 300 },   // 벽 때문에 성장 제한
        { "01113", 300 },

        { "3110", 50 },
        { "0113", 50 },

        // 필요하면 계속 추가
    };

    // ── 기존 API 유지 ─────────────────────────
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

        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
            {
                if (board[r, c] != player) continue;

                foreach (var (dr, dc) in Dirs)
                {
                    if (!IsStart(board, r, c, dr, dc, player, n))
                        continue;

                    string line = GetLine(board, r, c, dr, dc, player, n);
                    score += EvaluateLine(line);
                }
            }

        return score;
    }

    private static string GetLine(int[,] board, int r, int c, int dr, int dc, int player, int n)
    {
        StringBuilder sb = new();

        // 🔥 중심 기준 -4 ~ +4 (총 9칸)
        for (int i = -4; i <= 4; i++)
        {
            int nr = r + dr * i;
            int nc = c + dc * i;

            if (nr < 0 || nr >= n || nc < 0 || nc >= n)
                sb.Append('3'); // 벽
            else if (board[nr, nc] == 0)
                sb.Append('0'); // 빈칸
            else if (board[nr, nc] == player)
                sb.Append('1'); // 내 돌
            else
                sb.Append('2'); // 상대 돌
        }

        return sb.ToString();
    }

    private static int EvaluateLine(string line)
    {
        int score = 0;
        foreach (var kv in patterns.OrderByDescending(p => p.Value))
        {
            if (line.Contains(kv.Key))
                score += kv.Value;
        }
        return score;
    }

    private static bool IsStart(int[,] board, int r, int c, int dr, int dc, int player, int n)
    {
        int pr = r - dr;
        int pc = c - dc;

        if (pr < 0 || pr >= n || pc < 0 || pc >= n)
            return true;

        return board[pr, pc] != player;
    }
}