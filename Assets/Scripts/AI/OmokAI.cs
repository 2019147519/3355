// Assets/Scripts/AI/OmokAI.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class OmokAI
{
    private struct DifficultyConfig
    {
        public int Depth;
        public int CandidateRange;
        public int MaxCandidates;
        public float DefenseWeight;
        public bool UseImmediate;
        public bool UseMoveOrder;
        public float BlunderChance;
    }

    private static readonly DifficultyConfig[] Configs =
    {
        new() { Depth=2, CandidateRange=1, MaxCandidates=8,
                DefenseWeight=0.8f, UseImmediate=false,
                UseMoveOrder=false, BlunderChance=0.3f },

        new() { Depth=4, CandidateRange=2, MaxCandidates=15,
                DefenseWeight=1.5f, UseImmediate=true,
                UseMoveOrder=true,  BlunderChance=0.08f },

        new() { Depth=5, CandidateRange=2, MaxCandidates=20,
                DefenseWeight=1.8f, UseImmediate=true,
                UseMoveOrder=true,  BlunderChance=0f },
    };

    private DifficultyConfig _cfg;
    private int _aiPlayer;
    private int _humanPlayer;
    private bool _aiIsBlack;    // ★ AI가 흑인지 캐싱

    private readonly Dictionary<long, (int score, int depth)> _ttable = new();
    private static readonly long[,,] _zobrist = InitZobrist();
    private readonly System.Random _rng = new();

    public void Setup(int aiPlayer, int difficulty)
    {
        _aiPlayer = aiPlayer;
        _humanPlayer = aiPlayer == 1 ? 2 : 1;
        _aiIsBlack = aiPlayer == 1;   // ★
        _cfg = Configs[Mathf.Clamp(difficulty - 1, 0, 2)];
        _ttable.Clear();
    }

    // ── 최선 수 반환 ─────────────────────────────
    public (int row, int col) GetBestMove(int[,] board)
    {
        _ttable.Clear();

        var candidates = GetCandidates(board);
        if (candidates.Count == 0)
            return (BoardManager.Size / 2, BoardManager.Size / 2);

        // 후보에서 금수 제거 (AI가 흑일 때)
        if (_aiIsBlack)
            candidates = FilterForbidden(board, candidates, _aiPlayer);

        // 금수 제거 후 후보 없으면 차선책 (중앙 근처 랜덤)
        if (candidates.Count == 0)
            return FallbackMove(board);

        // 실수
        if (_cfg.BlunderChance > 0f && _rng.NextDouble() < _cfg.BlunderChance)
            return candidates[_rng.Next(candidates.Count)];

        // 즉시 승리 선처리
        if (_cfg.UseImmediate)
        {
            foreach (var (r, c) in candidates)
                if (CanWin(board, r, c, _aiPlayer)) return (r, c);

            foreach (var (r, c) in candidates)
            {
                if (!CanWin(board, r, c, _humanPlayer)) continue;
                if (_cfg.BlunderChance > 0f && _rng.NextDouble() < 0.1f) continue;
                return (r, c);
            }
        }

        // Minimax
        int bestScore = int.MinValue;
        int bestR = candidates[0].r, bestC = candidates[0].c;

        foreach (var (r, c) in candidates)
        {
            board[r, c] = _aiPlayer;
            int score = AlphaBeta(board, _cfg.Depth - 1,
                                    int.MinValue, int.MaxValue, false);
            board[r, c] = 0;

            if (score > bestScore)
            {
                bestScore = score;
                bestR = r; bestC = c;
            }
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

        if (isMax) // AI 차례
        {
            // ★ AI가 흑이면 금수 제거
            if (_aiIsBlack)
                candidates = FilterForbidden(board, candidates, _aiPlayer);

            result = int.MinValue;
            foreach (var (r, c) in candidates)
            {
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
                if (beta <= alpha) break;
            }
        }
        else // 상대 차례
        {
            // ★ 상대가 흑이면 금수 제거
            bool humanIsBlack = _humanPlayer == 1;
            if (humanIsBlack)
                candidates = FilterForbidden(board, candidates, _humanPlayer);

            result = int.MaxValue;
            foreach (var (r, c) in candidates)
            {
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
                if (beta <= alpha) break;
            }
        }

        _ttable[hash] = (result, depth);
        return result;
    }

    // ── 금수 필터링 ★ 핵심 ──────────────────────
    // board를 건드리지 않고 별도 복사본으로 검사
    private static List<(int r, int c)> FilterForbidden(
        int[,] board,
        List<(int r, int c)> candidates,
        int player)
    {
        if (player != 1) return candidates; // 흑만 금수

        var filtered = new List<(int r, int c)>(candidates.Count);
        foreach (var (r, c) in candidates)
        {
            // ★ board 복사본으로 검사 — 원본 오염 없음
            var copy = CopyBoard(board);
            if (!RenjuRule.IsForbidden(copy, r, c))
                filtered.Add((r, c));
        }
        return filtered;
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
                if (board[r, c] == 0) continue; // ★ 돌이 있는 셀 기준

                for (int dr = -_cfg.CandidateRange; dr <= _cfg.CandidateRange; dr++)
                    for (int dc = -_cfg.CandidateRange; dc <= _cfg.CandidateRange; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        int nr = r + dr, nc = c + dc;
                        if (nr < 0 || nr >= n || nc < 0 || nc >= n) continue;
                        if (board[nr, nc] != 0) continue;       // ★ 빈 칸만
                        if (!visited.Add((nr, nc))) continue;

                        int s = _cfg.UseMoveOrder ? HeuristicScore(board, nr, nc) : 0;
                        scored.Add((s, nr, nc));
                    }
            }

        // ★ 보드에 돌이 하나도 없을 때만 중앙 반환 (AI 선공 첫 수)
        if (scored.Count == 0)
        {
            int mid = n / 2;
            // 중앙이 비어있으면 중앙, 아니면 중앙 주변 빈 칸
            if (board[mid, mid] == 0)
                scored.Add((0, mid, mid));
            else
            {
                // 중앙 주변 3x3에서 빈 칸 찾기
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int nr = mid + dr, nc = mid + dc;
                        if (nr < 0 || nr >= n || nc < 0 || nc >= n) continue;
                        if (board[nr, nc] == 0)
                            scored.Add((0, nr, nc));
                    }
            }
        }

        if (_cfg.UseMoveOrder)
            scored.Sort((a, b) => b.score.CompareTo(a.score));

        if (scored.Count > _cfg.MaxCandidates)
            scored.RemoveRange(_cfg.MaxCandidates, scored.Count - _cfg.MaxCandidates);

        var result = new List<(int, int)>(scored.Count);
        foreach (var (_, r, c) in scored) result.Add((r, c));
        return result;
    }

    // ── 차선책 (금수 피한 빈칸 탐색) ────────────
    private (int r, int c) FallbackMove(int[,] board)
    {
        int n = board.GetLength(0);
        // 중앙부터 나선형으로 탐색
        for (int dist = 0; dist < n; dist++)
            for (int r = n / 2 - dist; r <= n / 2 + dist; r++)
                for (int c = n / 2 - dist; c <= n / 2 + dist; c++)
                {
                    if (r < 0 || r >= n || c < 0 || c >= n) continue;
                    if (board[r, c] != 0) continue;
                    var copy = CopyBoard(board);
                    if (!RenjuRule.IsForbidden(copy, r, c))
                        return (r, c);
                }
        return (n / 2, n / 2); // 최후 수단
    }

    // ── 즉시 승리 체크 ───────────────────────────
    private bool CanWin(int[,] board, int r, int c, int player)
    {
        if (board[r, c] != 0) return false;
        board[r, c] = player;
        bool win = WinChecker.CheckWin(board, r, c, player);
        board[r, c] = 0;
        return win;
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

    private bool HasNeighbor(int[,] board, int r, int c, int n)
    {
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < n && nc >= 0 && nc < n && board[nr, nc] != 0)
                    return true;
            }
        return false;
    }

    // ── 보드 복사 ────────────────────────────────
    private static int[,] CopyBoard(int[,] board)
    {
        int n = board.GetLength(0);
        var copy = new int[n, n];
        Array.Copy(board, copy, board.Length);
        return copy;
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