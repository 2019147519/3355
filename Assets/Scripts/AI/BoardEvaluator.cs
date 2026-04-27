// Assets/Scripts/AI/BoardEvaluator.cs
public static class BoardEvaluator
{
    private static readonly (int dr, int dc)[] Dirs =
        { (0,1),(1,0),(1,1),(1,-1) };

    // ── 난이도별 수비 가중치 ─────────────────────
    // 1.0 = 공격=수비 동등 / 2.0 = 수비 최우선
    public static int Evaluate(int[,] board, int aiPlayer, float defenseWeight = 1.5f)
    {
        int human = aiPlayer == 1 ? 2 : 1;
        int atk = ScoreFor(board, aiPlayer);
        int def = ScoreFor(board, human);
        return atk - (int)(def * defenseWeight);
    }

    public static int ScoreFor(int[,] board, int player)
    {
        int total = 0;
        int n = board.GetLength(0);

        foreach (var (dr, dc) in Dirs)
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    if (board[r, c] != player) continue;
                    total += EvalLine(board, r, c, player, dr, dc, n);
                }
        return total;
    }

    private static int EvalLine(int[,] board, int r, int c,
                                 int player, int dr, int dc, int n)
    {
        int pr = r - dr, pc = c - dc;
        if (pr >= 0 && pr < n && pc >= 0 && pc < n && board[pr, pc] == player)
            return 0;

        int count = 0, blanks = 0;
        int cur = r, curc = c;

        while (cur >= 0 && cur < n && curc >= 0 && curc < n)
        {
            if (board[cur, curc] == player) count++;
            else if (board[cur, curc] == 0) { blanks++; if (blanks > 1) break; }
            else break;
            cur += dr; curc += dc;
        }

        if (count < 2) return 0;

        bool openStart = IsEmpty(board, r - dr, c - dc, n);
        bool openEnd = IsEmpty(board, cur, curc, n);
        bool open = openStart && openEnd;

        return (count, open) switch
        {
            ( >= 5, _) => 10_000_000,
            (4, true) => 100_000,
            (4, false) => 10_000,
            (3, true) => 5_000,
            (3, false) => 500,
            (2, true) => 200,
            (2, false) => 50,
            _ => 0
        };
    }

    private static bool IsEmpty(int[,] b, int r, int c, int n)
        => r >= 0 && r < n && c >= 0 && c < n && b[r, c] == 0;
}