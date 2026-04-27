// Assets/Scripts/Core/WinChecker.cs
using System.Collections.Generic;

public static class WinChecker
{
    private static readonly (int dr, int dc)[] Dirs =
        { (0,1),(1,0),(1,1),(1,-1) };

    public static bool CheckWin(int[,] board, int row, int col, int player)
    {
        foreach (var (dr, dc) in Dirs)
        {
            int cnt = 1
                + Count(board, row, col, player, dr, dc)
                + Count(board, row, col, player, -dr, -dc);
            if (cnt >= 5) return true;
        }
        return false;
    }

    private static int Count(int[,] b, int r, int c, int p, int dr, int dc)
    {
        int n = b.GetLength(0), cnt = 0;
        for (int i = 1; i <= 4; i++)
        {
            int nr = r + dr * i, nc = c + dc * i;
            if (nr < 0 || nr >= n || nc < 0 || nc >= n || b[nr, nc] != p) break;
            cnt++;
        }
        return cnt;
    }

    public static bool IsBoardFull(int[,] b)
    {
        int n = b.GetLength(0);
        for (int r = 0; r < n; r++) for (int c = 0; c < n; c++) if (b[r, c] == 0) return false;
        return true;
    }

    public static List<(int, int)> GetWinLine(int[,] board, int row, int col, int player)
    {
        int n = board.GetLength(0);
        foreach (var (dr, dc) in Dirs)
        {
            var line = new List<(int, int)> { (row, col) };
            for (int i = 1; i <= 4; i++) { int nr = row + dr * i, nc = col + dc * i; if (nr < 0 || nr >= n || nc < 0 || nc >= n || board[nr, nc] != player) break; line.Add((nr, nc)); }
            for (int i = 1; i <= 4; i++) { int nr = row - dr * i, nc = col - dc * i; if (nr < 0 || nr >= n || nc < 0 || nc >= n || board[nr, nc] != player) break; line.Add((nr, nc)); }
            if (line.Count >= 5) return line;
        }
        return null;
    }
}