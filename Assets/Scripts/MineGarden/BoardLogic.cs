using System;
using System.Collections.Generic;

namespace MineGarden
{
    public struct RevealOutcome
    {
        public bool changed;
        public bool hitWeed;
        public bool won;
    }

    /// <summary>
    /// Weed placement, counts, flags and flood-fill. No Unity types, so the rules can be tested on their own.
    /// </summary>
    public static class BoardLogic
    {
        public static TileData[,] CreateHiddenBoard(int width, int height)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            var board = new TileData[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    board[x, y] = new TileData { x = x, y = y };
                }
            }

            return board;
        }

        public static bool InBounds(TileData[,] board, int x, int y)
        {
            return board != null
                && x >= 0
                && y >= 0
                && x < board.GetLength(0)
                && y < board.GetLength(1);
        }

        /// <summary>
        /// Places weeds after the first dig. The dug plot never gets a weed.
        /// Its eight neighbours are skipped while enough other plots remain.
        /// </summary>
        public static int PlaceWeeds(TileData[,] board, int safeX, int safeY, int weedCount, Random random)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            int width = board.GetLength(0);
            int height = board.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    board[x, y].hasWeed = false;
                    board[x, y].adjacentWeeds = 0;
                }
            }

            if (weedCount <= 0 || !InBounds(board, safeX, safeY))
                return 0;

            // Preferred plots sit outside the 3x3 area around the first dig.
            // If that area is too small, neighbours may hold weeds, but the dug plot never does.
            var preferred = new List<TileData>();
            var neighbours = new List<TileData>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == safeX && y == safeY)
                        continue;

                    if (IsNeighbour(safeX, safeY, x, y))
                        neighbours.Add(board[x, y]);
                    else
                        preferred.Add(board[x, y]);
                }
            }

            var pool = new List<TileData>(preferred);
            if (pool.Count < weedCount)
                pool.AddRange(neighbours);

            int placeCount = Math.Min(weedCount, pool.Count);
            Shuffle(pool, random);
            for (int i = 0; i < placeCount; i++)
                pool[i].hasWeed = true;

            return placeCount;
        }

        public static void CalculateAdjacentWeeds(TileData[,] board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            int width = board.GetLength(0);
            int height = board.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int count = 0;
                    for (int nx = x - 1; nx <= x + 1; nx++)
                    {
                        for (int ny = y - 1; ny <= y + 1; ny++)
                        {
                            if (nx == x && ny == y)
                                continue;
                            if (InBounds(board, nx, ny) && board[nx, ny].hasWeed)
                                count++;
                        }
                    }

                    board[x, y].adjacentWeeds = count;
                }
            }
        }

        /// <summary>
        /// Opens a plot. A zero opens connected safe plots with a queue, including the numbered edge.
        /// Flagged plots are left alone. Opening a weed loses and does not flood.
        /// </summary>
        public static RevealOutcome Reveal(TileData[,] board, int x, int y)
        {
            var outcome = new RevealOutcome();
            if (!InBounds(board, x, y))
                return outcome;

            TileData start = board[x, y];
            if (start.isRevealed || start.isFlagged)
                return outcome;

            if (start.hasWeed)
            {
                start.isRevealed = true;
                outcome.changed = true;
                outcome.hitWeed = true;
                return outcome;
            }

            var queue = new Queue<TileData>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                TileData tile = queue.Dequeue();
                if (tile.isRevealed || tile.isFlagged || tile.hasWeed)
                    continue;

                tile.isRevealed = true;
                outcome.changed = true;

                if (tile.adjacentWeeds != 0)
                    continue;

                for (int nx = tile.x - 1; nx <= tile.x + 1; nx++)
                {
                    for (int ny = tile.y - 1; ny <= tile.y + 1; ny++)
                    {
                        if (!InBounds(board, nx, ny))
                            continue;

                        TileData neighbour = board[nx, ny];
                        if (neighbour.isRevealed || neighbour.isFlagged || neighbour.hasWeed)
                            continue;

                        queue.Enqueue(neighbour);
                    }
                }
            }

            outcome.won = CountUnrevealedSafeTiles(board) == 0;
            return outcome;
        }

        public static bool ToggleFlag(TileData[,] board, int x, int y)
        {
            if (!InBounds(board, x, y))
                return false;

            TileData tile = board[x, y];
            if (tile.isRevealed)
                return false;

            tile.isFlagged = !tile.isFlagged;
            return true;
        }

        public static int CountFlags(TileData[,] board)
        {
            if (board == null)
                return 0;

            int count = 0;
            int width = board.GetLength(0);
            int height = board.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y].isFlagged)
                        count++;
                }
            }

            return count;
        }

        public static int CountUnrevealedSafeTiles(TileData[,] board)
        {
            if (board == null)
                return 0;

            int count = 0;
            int width = board.GetLength(0);
            int height = board.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileData tile = board[x, y];
                    if (!tile.hasWeed && !tile.isRevealed)
                        count++;
                }
            }

            return count;
        }

        public static int CountWeeds(TileData[,] board)
        {
            if (board == null)
                return 0;

            int count = 0;
            int width = board.GetLength(0);
            int height = board.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y].hasWeed)
                        count++;
                }
            }

            return count;
        }

        static bool IsNeighbour(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x0 - x1);
            int dy = Math.Abs(y0 - y1);
            return dx <= 1 && dy <= 1 && (dx != 0 || dy != 0);
        }

        static void Shuffle(List<TileData> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                TileData temp = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = temp;
            }
        }
    }
}
