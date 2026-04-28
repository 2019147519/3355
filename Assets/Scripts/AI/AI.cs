using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI
{
    const int BOARD_SIZE = 15;

    int[,] board = new int[BOARD_SIZE, BOARD_SIZE];
    // 0 = 빈칸, 1 = AI, -1 = 상대

    int[][] directions = new int[][]
    {
        new int[] {1, 0},  // →
        new int[] {0, 1},  // ↓
        new int[] {1, 1},  // ↘
        new int[] {1, -1}, // ↗
    };

    Dictionary<string, int> patterns = new Dictionary<string, int>()
    {
        // 0 = 빈칸
        // 1 = 내 돌
        // 2 = 상대 돌
        // 3 = 벽
        
        // =========================
        // 5목 (무조건 승리)
        // =========================
        { "11111", 2000000 },

        // =========================
        // 열린 4 (양쪽 열림)
        // =========================
        { "011110", 200000 },

        // =========================
        // 닫힌 4 (한쪽 막힘)
        // =========================
        { "211110", 20000 },
        { "011112", 20000 },
        { "311110", 15000 }, // 벽
        { "011113", 15000 },

        // =========================
        // 확장 열린 4 (한 칸 띄운 형태)
        // =========================
        { "0110110", 180000 },
        { "0101110", 180000 },

        // =========================
        // 열린 3 (양쪽 열림, 최소 2칸 여유)
        // =========================
        { "0011100", 12000 },
        { "011100", 10000 },
        { "001110", 10000 },
        { "010110", 10000 },
        { "011010", 10000 },

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

    List<Vector2Int> GetMoves()
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        for (int x = 0; x < BOARD_SIZE; x++)
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (board[x, y] != 0) continue;

                // 주변 2칸 안에 돌 있으면 후보
                if (HasNeighbor(x, y))
                    moves.Add(new Vector2Int(x, y));
            }

        return moves;
    }

    void MakeMove(Vector2Int move, int player)
    {
        board[move.x, move.y] = player;
    }

    void UndoMove(Vector2Int move)
    {
        board[move.x, move.y] = 0;
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE;
    }

    bool IsStart(int x, int y, int dx, int dy, int player)
    {
        int px = x - dx;
        int py = y - dy;

        if (!InBounds(px, py)) return true;

        return board[px, py] != player;
    }

    bool HasNeighbor(int x, int y, int distance = 2)
    {
        for (int dx = -distance; dx <= distance; dx++)
        {
            for (int dy = -distance; dy <= distance; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (!InBounds(nx, ny)) continue;

                if (board[nx, ny] != 0)
                    return true;
            }
        }

        return false;
    }

    bool IsGameOver()
    {
        return HasFive(1) || HasFive(-1) || IsBoardFull();
    }

    bool HasFive(int player)
    {
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (board[x, y] != player) continue;

                foreach (var dir in directions)
                {
                    if (CheckFive(x, y, dir[0], dir[1], player))
                        return true;
                }
            }
        }

        return false;
    }

    bool CheckFive(int x, int y, int dx, int dy, int player)
    {
        if (!IsStart(x, y, dx, dy, player))
            return false;

        int count = 0;

        int nx = x;
        int ny = y;

        while (InBounds(nx, ny) && board[nx, ny] == player)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        return count >= 5;
    }

    bool IsBoardFull()
    {
        for (int x = 0; x < 15; x++)
            for (int y = 0; y < 15; y++)
            {
                if (board[x, y] == 0)
                    return false;
            }

        return true;
    }

    int Evaluate()
    {
        int myScore = EvaluatePlayer(1);
        int enemyScore = EvaluatePlayer(-1);

        return myScore - (int)(enemyScore * 1.3f);
    }

    int EvaluatePlayer(int player)
    {
        int score = 0;

        for (int x = 0; x < BOARD_SIZE; x++)
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (board[x, y] != player) continue;

                foreach (var dir in directions)
                {
                    //score += EvaluateDirection(x, y, dir[0], dir[1], player);
                    if (!IsStart(x, y, dir[0], dir[1], player))
                        continue;

                    string line = GetLine(x, y, dir[0], dir[1], player);

                    score += EvaluateLine(line);
                }
            }

        return score;
    }

    string GetLine(int x, int y, int dx, int dy, int player)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // 🔥 중심 기준 -4 ~ +4 (총 9칸)
        for (int i = -4; i <= 4; i++)
        {
            int nx = x + dx * i;
            int ny = y + dy * i;

            if (!InBounds(nx, ny))
            {
                sb.Append('3'); // 벽
            }
            else if (board[nx, ny] == 0)
            {
                sb.Append('0'); // 빈칸
            }
            else if (board[nx, ny] == player)
            {
                sb.Append('1'); // 내 돌
            }
            else
            {
                sb.Append('2'); // 상대 돌
            }
        }

        return sb.ToString();
    }

    int EvaluateLine(string line)
    {
        int score = 0;

        foreach (var kv in patterns.OrderByDescending(p => p.Value))
        {
            if (line.Contains(kv.Key))
            {
                score += kv.Value;
                break; // 가장 강한 패턴만 반영
            }
        }

        return score;
    }

    /*
    int EvaluateDirection(int x, int y, int dx, int dy, int player)
    {
        if (!IsStart(x, y, dx, dy, player))
            return 0;

        int count = 1;

        int openEnds = 0;

        // 👉 한쪽 방향
        int nx = x + dx;
        int ny = y + dy;

        while (InBounds(nx, ny) && board[nx, ny] == player)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        if (InBounds(nx, ny) && board[nx, ny] == 0)
            openEnds++;

        // 👉 반대 방향
        nx = x - dx;
        ny = y - dy;

        while (InBounds(nx, ny) && board[nx, ny] == player)
        {
            count++;
            nx -= dx;
            ny -= dy;
        }

        if (InBounds(nx, ny) && board[nx, ny] == 0)
            openEnds++;

        return GetScore(count, openEnds);
    }

    int GetScore(int count, int openEnds)
    {
        if (count >= 5)
            return 1000000;

        if (count == 4)
        {
            if (openEnds == 2) return 100000; // 열린 4
            if (openEnds == 1) return 10000;  // 닫힌 4
        }

        if (count == 3)
        {
            if (openEnds == 2) return 5000;  // 열린 3
            if (openEnds == 1) return 500;   // 닫힌 3
        }

        if (count == 2)
        {
            if (openEnds == 2) return 500;
            if (openEnds == 1) return 50;
        }

        if (count == 1)
        {
            if (openEnds == 2) return 10;
        }

        return 0;
    }
    */

    int Minimax(int depth, int alpha, int beta, bool maximizing)
    {
        if (depth == 0 || IsGameOver())
            return Evaluate();

        var moves = GetMoves();

        if (maximizing)
        {
            int maxValue = int.MinValue;

            foreach (var move in moves)
            {
                MakeMove(move, 1);

                int value = Minimax(depth - 1, alpha, beta, false);

                UndoMove(move);

                maxValue = Mathf.Max(maxValue, value);
                alpha = Mathf.Max(alpha, value);

                if (beta <= alpha) break; // α–β pruning
            }

            return maxValue;
        }
        else
        {
            int minValue = int.MaxValue;

            foreach (var move in moves)
            {
                MakeMove(move, -1);

                int value = Minimax(depth - 1, alpha, beta, true);

                UndoMove(move);

                minValue = Mathf.Min(minValue, value);
                beta = Mathf.Min(beta, value);

                if (beta <= alpha) break; // α–β pruning
            }

            return minValue;
        }
    }

    Vector2Int FindBestMove()
    {
        int bestScore = int.MinValue;
        Vector2Int bestMove = Vector2Int.zero;

        foreach (var move in GetMoves())
        {
            MakeMove(move, 1);

            int score = Minimax(3, int.MinValue, int.MaxValue, false);

            UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }


}
