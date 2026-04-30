// Assets/Scripts/Core/RenjuRule.cs
using System.Collections.Generic;

public static class RenjuRule
{
    private const int Black = 1;

    private static readonly (int dr, int dc)[] Dirs =
        { (0,1), (1,0), (1,1), (1,-1) };

    // ── 외부 진입점 ─────────────────────────────
    public static ForbiddenType GetForbiddenType(int[,] board, int row, int col)
    {
        board[row, col] = Black;
        ForbiddenType result = ForbiddenType.None;

        try
        {
            if (IsOverline(board, row, col))
                result = ForbiddenType.Overline;
            else if (!IsFiveExact(board, row, col))
            {
                int fours = CountFours(board, row, col);
                int openThrees = CountOpenThrees(board, row, col);

                if (fours >= 2) result = ForbiddenType.DoubleFour;
                else if (openThrees >= 2) result = ForbiddenType.DoubleThree;
            }
        }
        finally
        {
            // ★ 예외 발생해도 반드시 복구
            board[row, col] = 0;
        }

        return result;
    }

    public static bool IsForbidden(int[,] board, int row, int col)
        => GetForbiddenType(board, row, col) != ForbiddenType.None;

    // ── 정확히 5목인지 (장목 아님) ──────────────
    private static bool IsFiveExact(int[,] board, int row, int col)
    {
        foreach (var (dr, dc) in Dirs)
        {
            int cnt = 1
                + CountDir(board, row, col, dr, dc)
                + CountDir(board, row, col, -dr, -dc);
            if (cnt == 5) return true;
        }
        return false;
    }

    // ── 장목 (6목 이상) ──────────────────────────
    private static bool IsOverline(int[,] board, int row, int col)
    {
        foreach (var (dr, dc) in Dirs)
        {
            int cnt = 1
                + CountDir(board, row, col, dr, dc)
                + CountDir(board, row, col, -dr, -dc);
            if (cnt >= 6) return true;
        }
        return false;
    }

    // ── 4 카운트 ─────────────────────────────────
    // 방향별로 "4"가 되는 방향 수를 반환
    // 4 = 연속4(열린/닫힌) 또는 X_XXX / XX_XX / XXX_X 패턴
    private static int CountFours(int[,] board, int row, int col)
    {
        int count = 0;
        foreach (var (dr, dc) in Dirs)
        {
            if (HasFour(board, row, col, dr, dc))
                count++;
        }
        return count;
    }

    private static bool HasFour(int[,] board, int row, int col, int dr, int dc)
    {
        int n = board.GetLength(0);

        // 현재 위치 포함 해당 방향 9칸 슬라이딩 윈도우(5칸씩)
        // 흑4 + 빈1 조합이 5칸 안에 있으면 4 성립
        for (int start = -4; start <= 0; start++)
        {
            int blacks = 0, blanks = 0;
            bool valid = true;

            for (int i = 0; i < 5; i++)
            {
                int r = row + (start + i) * dr;
                int c = col + (start + i) * dc;

                if (r < 0 || r >= n || c < 0 || c >= n)
                { valid = false; break; }

                int cell = board[r, c];
                if (cell == Black) blacks++;
                else if (cell == 0) blanks++;
                else { valid = false; break; }
            }

            // 흑4 + 빈1 = 4 패턴
            if (valid && blacks == 4 && blanks == 1)
            {
                // 이 4가 실제로 착수 위치(row,col)를 포함하는지 확인
                bool includesCurrent = false;
                for (int i = 0; i < 5; i++)
                {
                    int r = row + (start + i) * dr;
                    int c = col + (start + i) * dc;
                    if (r == row && c == col) { includesCurrent = true; break; }
                }
                if (includesCurrent) return true;
            }
        }
        return false;
    }

    // ── 열린 3 카운트 ────────────────────────────
    // 열린 3 = 한 번 더 놓으면 열린 4가 되는 패턴
    // _XXX_, _X_XX_, _XX_X_ 모두 포함
    private static int CountOpenThrees(int[,] board, int row, int col)
    {
        int count = 0;
        int n = board.GetLength(0);

        foreach (var (dr, dc) in Dirs)
        {
            if (IsOpenThree(board, row, col, dr, dc, n))
                count++;
        }
        return count;
    }

    private static bool IsOpenThree(int[,] board, int row, int col,
                                     int dr, int dc, int n)
    {
        // 빈 칸 후보들에 가상 착수해서 열린 4가 되는지 확인
        // 현재 위치 기준 ±5 범위 빈칸 탐색
        for (int offset = -4; offset <= 4; offset++)
        {
            if (offset == 0) continue; // 현재 위치는 이미 Black

            int tr = row + offset * dr;
            int tc = col + offset * dc;

            if (tr < 0 || tr >= n || tc < 0 || tc >= n) continue;
            if (board[tr, tc] != 0) continue;

            // 가상 착수 후 열린 4가 되는지
            board[tr, tc] = Black;
            bool makesOpenFour = IsOpenFour(board, tr, tc, dr, dc, n)
                              || IsOpenFour(board, tr, tc, -dr, -dc, n);
            board[tr, tc] = 0;

            if (makesOpenFour) return true;
        }
        return false;
    }

    // 해당 위치·방향에서 열린 4인지
    // 열린 4 = _XXXX_ (양 끝 모두 비어있는 연속 4)
    private static bool IsOpenFour(int[,] board, int r, int c,
                                    int dr, int dc, int n)
    {
        int forward = CountDir(board, r, c, dr, dc);
        int backward = CountDir(board, r, c, -dr, -dc);
        int cnt = 1 + forward + backward;

        if (cnt != 4) return false;

        // 양 끝 빈칸 확인
        int fr = r + (forward + 1) * dr;
        int fc = c + (forward + 1) * dc;
        int br = r + (backward + 1) * -dr;
        int bc = c + (backward + 1) * -dc;

        return IsInBoundsEmpty(board, fr, fc, n)
            && IsInBoundsEmpty(board, br, bc, n);
    }

    // ── 공통 헬퍼 ───────────────────────────────
    private static int CountDir(int[,] b, int r, int c, int dr, int dc)
    {
        int n = b.GetLength(0), cnt = 0;
        for (int i = 1; i <= 5; i++)
        {
            int nr = r + dr * i, nc = c + dc * i;
            if (nr < 0 || nr >= n || nc < 0 || nc >= n || b[nr, nc] != Black) break;
            cnt++;
        }
        return cnt;
    }

    private static bool IsInBoundsEmpty(int[,] b, int r, int c, int n)
        => r >= 0 && r < n && c >= 0 && c < n && b[r, c] == 0;
}
