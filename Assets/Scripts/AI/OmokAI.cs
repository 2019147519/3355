// Assets/Scripts/AI/OmokAI.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class OmokAI
{
    // ── 난이도 파라미터 ──────────────────────────
    private struct DifficultyConfig
    {
        public int Depth;           // 탐색 깊이
        public int CandidateRange;  // 후보 반경
        public int MaxCandidates;   // 최대 후보 수
        public float DefenseWeight;   // 수비 가중치 (낮을수록 멍청)
        public bool UseImmediate;    // 즉시 승리/막기 선처리 여부
        public bool UseMoveOrder;    // Move Ordering 여부
        public float BlunderChance;   // 실수 확률 (0~1)
    }

    private static readonly DifficultyConfig[] Configs =
    {
        // 쉬움 — 얕은 탐색, 자주 실수, 수비 약함
        new() { Depth=2, CandidateRange=1, MaxCandidates=8,
                DefenseWeight=0.8f, UseImmediate=false,
                UseMoveOrder=false, BlunderChance=0.35f },

        // 보통 — 즉시 승리만 처리, 막기는 가끔 놓침
        new() { Depth=3, CandidateRange=2, MaxCandidates=12,
                DefenseWeight=1.2f, UseImmediate=true,
                UseMoveOrder=false, BlunderChance=0.15f },

        // 어려움 — 풀 기능
        new() { Depth=5, CandidateRange=2, MaxCandidates=20,
                DefenseWeight=1.8f, UseImmediate=true,
                UseMoveOrder=true,  BlunderChance=0f },
    };

    private DifficultyConfig _cfg;
    private int _aiPlayer;
    private int _humanPlayer;

    private readonly Dictionary<long, (int score, int depth)> _ttable = new();
    private static readonly long[,,] _zobrist = InitZobrist();
    private readonly System.Random _rng = new();

    // ── 세팅 ─────────────────────────────────────
    public void Setup(int aiPlayer, int difficulty)
    {
        _aiPlayer = aiPlayer;
        _humanPlayer = aiPlayer == 1 ? 2 : 1;
        _cfg = Configs[Mathf.Clamp(difficulty - 1, 0, 2)];
        _ttable.Clear();
    }

    // ── 최선 수 반환 ─────────────────────────────
    public (int row, int col) GetBestMove(int[,] board, int player)
    {
        _aiPlayer = player;
        _humanPlayer = player == 1 ? 2 : 1;
        _ttable.Clear();

        var candidates = GetCandidates(board);
        if (candidates.Count == 0)
            return (BoardManager.Size / 2, BoardManager.Size / 2);

        // ── 실수 (Blunder) ──────────────────────
        // 쉬움/보통은 일정 확률로 랜덤 착수
        if (_cfg.BlunderChance > 0f && _rng.NextDouble() < _cfg.BlunderChance)
        {
            var rand = candidates[_rng.Next(candidates.Count)];
            return (rand.r, rand.c);
        }

        // ── 즉시 승리 선처리 ────────────────────
        if (_cfg.UseImmediate)
        {
            foreach (var (r, c) in candidates)
                if (CanWin(board, r, c, _aiPlayer)) return (r, c);

            // 막기 — 보통은 가끔 놓침
            foreach (var (r, c) in candidates)
            {
                if (!CanWin(board, r, c, _humanPlayer)) continue;
                // 보통 난이도: 15% 확률로 막기 실패
                if (_cfg.BlunderChance > 0f && _rng.NextDouble() < 0.15f) continue;
                return (r, c);
            }
        }

        // ── Minimax 탐색 ────────────────────────
        int bestScore = int.MinValue;
        int bestR = candidates[0].r, bestC = candidates[0].c;

        foreach (var (r, c) in candidates)
        {
            if (_aiPlayer == 1 && RenjuRule.IsForbidden(board, r, c)) continue;

            board[r, c] = _aiPlayer;
            int score = AlphaBeta(board, _cfg.Depth - 1,
                                    int.MinValue, int.MaxValue, false);
            board[r, c] = 0;

            if (score > bestScore) { bestScore = score; bestR = r; bestC = c; }
        }

        return (bestR, bestC);
    }

    // ── Alpha-Beta ───────────────────────────────
    private int AlphaBeta(int[,] board, int depth, int alpha, int beta, bool isMax)
    {
        long hash = ZobristHash(board);
        if (_ttable.TryGetValue(hash, out var cached) && cached.depth >= depth)
            return cached.score;

        if (depth == 0)
        {
            int s = BoardEvaluator.Evaluate(board, _aiPlayer, _cfg.DefenseWeight);
            _ttable[hash] = (s, 0);
            return s;
        }

        var candidates = GetCandidates(board);
        if (candidates.Count == 0)
            return BoardEvaluator.Evaluate(board, _aiPlayer, _cfg.DefenseWeight);

        int result;

        if (isMax)
        {
            result = int.MinValue;
            foreach (var (r, c) in candidates)
            {
                if (_aiPlayer == 1 && RenjuRule.IsForbidden(board, r, c)) continue;

                board[r, c] = _aiPlayer;
                if (WinChecker.CheckWin(board, r, c, _aiPlayer))
                {
                    board[r, c] = 0;
                    result = 10_000_000 + depth;
                    break;
                }
                int score = AlphaBeta(board, depth - 1, alpha, beta, false);
                board[r, c] = 0;
                result = Math.Max(result, score);
                alpha = Math.Max(alpha, result);
                if (beta <= alpha) break;   // α–β pruning
            }
        }
        else
        {
            result = int.MaxValue;
            foreach (var (r, c) in candidates)
            {
                if (_humanPlayer == 1 && RenjuRule.IsForbidden(board, r, c)) continue;

                board[r, c] = _humanPlayer;
                if (WinChecker.CheckWin(board, r, c, _humanPlayer))
                {
                    board[r, c] = 0;
                    result = -(10_000_000 + depth);
                    break;
                }
                int score = AlphaBeta(board, depth - 1, alpha, beta, true);
                board[r, c] = 0;
                result = Math.Min(result, score);
                beta = Math.Min(beta, result);
                if (beta <= alpha) break;   // α–β pruning
            }
        }

        _ttable[hash] = (result, depth);
        return result;
    }

    // ── 후보 생성 ────────────────────────────────
    private List<(int r, int c)> GetCandidates(int[,] board)
    {
        int n = board.GetLength(0);
        var scored = new List<(int score, int r, int c)>();
        var visited = new HashSet<(int, int)>();

        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
            {
                if (board[r, c] == 0 || !HasNeighbor(board, r, c, n)) continue;

                for (int dr = -_cfg.CandidateRange; dr <= _cfg.CandidateRange; dr++)
                    for (int dc = -_cfg.CandidateRange; dc <= _cfg.CandidateRange; dc++)
                    {
                        int nr = r + dr, nc = c + dc;
                        if (nr < 0 || nr >= n || nc < 0 || nc >= n || board[nr, nc] != 0) continue;
                        if (!visited.Add((nr, nc))) continue;

                        int s = _cfg.UseMoveOrder ? HeuristicScore(board, nr, nc) : 0;
                        scored.Add((s, nr, nc));
                    }
            }

        if (scored.Count == 0)
            scored.Add((0, n / 2, n / 2));

        if (_cfg.UseMoveOrder)
            scored.Sort((a, b) => b.score.CompareTo(a.score));

        if (scored.Count > _cfg.MaxCandidates)
            scored.RemoveRange(_cfg.MaxCandidates, scored.Count - _cfg.MaxCandidates);

        var result = new List<(int, int)>(scored.Count);
        foreach (var (_, r, c) in scored) result.Add((r, c));
        return result;
    }

    // ── 휴리스틱 점수 ────────────────────────────
    private int HeuristicScore(int[,] board, int r, int c)
    {
        board[r, c] = _aiPlayer;
        int atk = BoardEvaluator.ScoreFor(board, _aiPlayer);
        board[r, c] = 0;

        board[r, c] = _humanPlayer;
        int def = BoardEvaluator.ScoreFor(board, _humanPlayer);
        board[r, c] = 0;

        return atk + def;
    }

    // ── 즉시 승리 가능 여부 ──────────────────────
    private bool CanWin(int[,] board, int r, int c, int player)
    {
        if (board[r, c] != 0) return false;
        board[r, c] = player;
        bool win = WinChecker.CheckWin(board, r, c, player);
        board[r, c] = 0;
        return win;
    }

    private bool HasNeighbor(int[,] board, int r, int c, int n)
    {
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < n && nc >= 0 && nc < n && board[nr, nc] != 0) return true;
            }
        return false;
    }

    // ── Zobrist 해시 ─────────────────────────────
    private static long[,,] InitZobrist()
    {
        var rng = new System.Random(42);
        var table = new long[BoardManager.Size, BoardManager.Size, 3];
        for (int r = 0; r < BoardManager.Size; r++)
            for (int c = 0; c < BoardManager.Size; c++)
                for (int p = 0; p < 3; p++)
                {
                    var buf = new byte[8];
                    rng.NextBytes(buf);
                    table[r, c, p] = BitConverter.ToInt64(buf, 0);
                }
        return table;
    }

    private static long ZobristHash(int[,] board)
    {
        long hash = 0;
        int n = board.GetLength(0);
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
                if (board[r, c] != 0)
                    hash ^= _zobrist[r, c, board[r, c]];
        return hash;
    }
}