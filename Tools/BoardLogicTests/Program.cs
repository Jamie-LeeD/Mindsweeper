using System;

namespace MineGarden.Tests
{
    static class Program
    {
        static int checks;
        static int failures;

        static void Main()
        {
            Presets();
            KnownAdjacentCounts();
            RandomAdjacentCounts();
            FirstClickProtection();
            WeedCountIsClamped();
            FlagsSurvivePlacement();
            FloodFill();
            FlagsBlockRevealAndDoNotWin();
            DirectActions();
            LargeEmptyFlood();

            Console.WriteLine(failures == 0
                ? "Passed " + checks + " checks."
                : "Failed " + failures + " of " + checks + " checks.");
            Environment.Exit(failures == 0 ? 0 : 1);
        }

        static void Presets()
        {
            ExpectSize(Difficulty.Easy, 6, 6, 5);
            ExpectSize(Difficulty.Medium, 8, 8, 10);
            ExpectSize(Difficulty.Hard, 10, 10, 18);
        }

        static void ExpectSize(Difficulty difficulty, int width, int height, int weeds)
        {
            DifficultyPresets.Get(difficulty, out int actualWidth, out int actualHeight, out int actualWeeds);
            Check(actualWidth == width && actualHeight == height && actualWeeds == weeds, difficulty + " preset");
        }

        static void KnownAdjacentCounts()
        {
            TileData[,] center = BoardLogic.CreateHiddenBoard(3, 3);
            center[1, 1].hasWeed = true;
            BoardLogic.CalculateAdjacentWeeds(center);
            Check(center[1, 1].adjacentWeeds == 0, "a lone weed does not count itself");
            Check(center[0, 0].adjacentWeeds == 1 && center[2, 2].adjacentWeeds == 1 && center[1, 0].adjacentWeeds == 1,
                "all eight neighbours of a center weed are 1");

            TileData[,] corner = BoardLogic.CreateHiddenBoard(3, 3);
            corner[0, 0].hasWeed = true;
            BoardLogic.CalculateAdjacentWeeds(corner);
            Check(corner[1, 0].adjacentWeeds == 1 && corner[0, 1].adjacentWeeds == 1 && corner[1, 1].adjacentWeeds == 1,
                "corner weed touches three plots");
            Check(corner[2, 0].adjacentWeeds == 0 && corner[0, 2].adjacentWeeds == 0 && corner[2, 2].adjacentWeeds == 0,
                "plots two steps away stay at 0");

            TileData[,] pair = BoardLogic.CreateHiddenBoard(3, 3);
            pair[0, 0].hasWeed = true;
            pair[1, 0].hasWeed = true;
            BoardLogic.CalculateAdjacentWeeds(pair);
            Check(pair[0, 1].adjacentWeeds == 2 && pair[1, 1].adjacentWeeds == 2, "a plot beside two weeds shows 2");
            Check(pair[2, 0].adjacentWeeds == 1 && pair[0, 0].adjacentWeeds == 1, "edge of a pair shows 1");
        }

        static void RandomAdjacentCounts()
        {
            var random = new Random(7);
            for (int trial = 0; trial < 40; trial++)
            {
                TileData[,] board = BoardLogic.CreateHiddenBoard(10, 10);
                BoardLogic.PlaceWeeds(board, 4, 4, 18, random);
                BoardLogic.CalculateAdjacentWeeds(board);
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        int expected = 0;
                        for (int i = 0; i < 10; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                if (!board[i, j].hasWeed)
                                    continue;
                                if (Math.Max(Math.Abs(i - x), Math.Abs(j - y)) == 1)
                                    expected++;
                            }
                        }

                        if (board[x, y].adjacentWeeds != expected)
                        {
                            Check(false, "adjacent count at " + x + "," + y);
                            return;
                        }
                    }
                }
            }

            Check(true, "random boards match an independent eight-direction count");
        }

        static void FirstClickProtection()
        {
            var random = new Random(1);
            for (int trial = 0; trial < 50; trial++)
            {
                CheckProtected(10, 10, 18, 5, 5, random, true);
                CheckProtected(6, 6, 5, 0, 0, random, true);
                CheckProtected(8, 8, 10, 7, 3, random, true);
            }

            TileData[,] tight = BoardLogic.CreateHiddenBoard(3, 3);
            int placed = BoardLogic.PlaceWeeds(tight, 1, 1, 1, new Random(3));
            Check(placed == 1 && !tight[1, 1].hasWeed, "center dig on a full 3x3 still stays safe");
            int neighbourWeeds = 0;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (tight[x, y].hasWeed)
                        neighbourWeeds++;
                }
            }

            Check(neighbourWeeds == 1, "the single weed lands in the ring when nothing else is free");
        }

        static void CheckProtected(int width, int height, int weeds, int safeX, int safeY, Random random, bool protectRing)
        {
            TileData[,] board = BoardLogic.CreateHiddenBoard(width, height);
            int placed = BoardLogic.PlaceWeeds(board, safeX, safeY, weeds, random);
            Check(placed == weeds, width + "x" + height + " places the requested weeds");
            Check(!board[safeX, safeY].hasWeed, "first dig is never a weed");
            if (!protectRing)
                return;

            for (int x = safeX - 1; x <= safeX + 1; x++)
            {
                for (int y = safeY - 1; y <= safeY + 1; y++)
                {
                    if (!BoardLogic.InBounds(board, x, y))
                        continue;
                    if (board[x, y].hasWeed)
                    {
                        Check(false, "protected ring contained a weed at " + x + "," + y);
                        return;
                    }
                }
            }
        }

        static void WeedCountIsClamped()
        {
            TileData[,] board = BoardLogic.CreateHiddenBoard(3, 3);
            int placed = BoardLogic.PlaceWeeds(board, 0, 0, 100, new Random(9));
            Check(placed == 8, "cannot place more weeds than the other plots");
            Check(!board[0, 0].hasWeed, "clamped placement still protects the dig");
            Check(BoardLogic.CountWeeds(board) == 8, "weed counter matches placed weeds");

            TileData[,] sameA = BoardLogic.CreateHiddenBoard(8, 8);
            TileData[,] sameB = BoardLogic.CreateHiddenBoard(8, 8);
            BoardLogic.PlaceWeeds(sameA, 2, 3, 10, new Random(42));
            BoardLogic.PlaceWeeds(sameB, 2, 3, 10, new Random(42));
            bool same = true;
            for (int x = 0; x < 8 && same; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (sameA[x, y].hasWeed != sameB[x, y].hasWeed)
                        same = false;
                }
            }

            Check(same, "the same seed places the same weeds");
        }

        static void FlagsSurvivePlacement()
        {
            TileData[,] board = BoardLogic.CreateHiddenBoard(6, 6);
            board[1, 1].isFlagged = true;
            board[4, 4].isFlagged = true;
            BoardLogic.PlaceWeeds(board, 0, 0, 5, new Random(4));
            Check(board[1, 1].isFlagged && board[4, 4].isFlagged, "markers stay put when weeds are placed");
            Check(!board[0, 0].hasWeed, "a marked board still protects the first dig");
            Check(!board[0, 1].hasWeed && !board[1, 0].hasWeed && !board[1, 1].hasWeed, "corner ring stays clear");
        }

        static void FloodFill()
        {
            TileData[,] board = BoardLogic.CreateHiddenBoard(3, 1);
            board[2, 0].hasWeed = true;
            BoardLogic.CalculateAdjacentWeeds(board);
            Check(board[0, 0].adjacentWeeds == 0 && board[1, 0].adjacentWeeds == 1, "row counts");

            RevealOutcome opened = BoardLogic.Reveal(board, 0, 0);
            Check(opened.changed && opened.won && !opened.hitWeed, "opening the empty end clears the row");
            Check(board[0, 0].isRevealed && board[1, 0].isRevealed && !board[2, 0].isRevealed, "flood stops on the weed");

            TileData[,] square = BoardLogic.CreateHiddenBoard(3, 3);
            square[2, 2].hasWeed = true;
            BoardLogic.CalculateAdjacentWeeds(square);
            RevealOutcome area = BoardLogic.Reveal(square, 0, 0);
            Check(area.won && !square[2, 2].isRevealed, "flood opens every safe plot and leaves the weed");
            Check(square[1, 1].isRevealed && square[1, 1].adjacentWeeds == 1, "the numbered border is opened");
        }

        static void FlagsBlockRevealAndDoNotWin()
        {
            TileData[,] board = BoardLogic.CreateHiddenBoard(3, 1);
            board[2, 0].hasWeed = true;
            BoardLogic.CalculateAdjacentWeeds(board);
            Check(BoardLogic.ToggleFlag(board, 1, 0), "can mark a hidden plot");

            RevealOutcome blocked = BoardLogic.Reveal(board, 0, 0);
            Check(blocked.changed && !blocked.won, "a marker stops the win");
            Check(board[0, 0].isRevealed && !board[1, 0].isRevealed && board[1, 0].isFlagged, "flagged plot stays closed");

            Check(BoardLogic.ToggleFlag(board, 1, 0), "can remove the marker");
            RevealOutcome finished = BoardLogic.Reveal(board, 1, 0);
            Check(finished.won && board[1, 0].isRevealed, "clearing the last safe plot wins without a marker");

            TileData[,] revealed = BoardLogic.CreateHiddenBoard(2, 2);
            revealed[0, 0].isRevealed = true;
            Check(!BoardLogic.ToggleFlag(revealed, 0, 0), "opened plots cannot be marked");
            Check(!revealed[0, 0].isFlagged, "opened plot stays unmarked");
        }

        static void DirectActions()
        {
            TileData[,] board = BoardLogic.CreateHiddenBoard(2, 2);
            board[1, 1].hasWeed = true;
            BoardLogic.CalculateAdjacentWeeds(board);

            RevealOutcome miss = BoardLogic.Reveal(board, -1, 0);
            Check(!miss.changed && !miss.hitWeed && !miss.won, "clicks outside the bed do nothing");
            Check(!BoardLogic.ToggleFlag(board, 4, 4), "markers outside the bed do nothing");

            RevealOutcome numbered = BoardLogic.Reveal(board, 0, 1);
            Check(numbered.changed && !numbered.won && board[0, 1].isRevealed && !board[0, 0].isRevealed,
                "opening a number does not flood the bed");

            RevealOutcome again = BoardLogic.Reveal(board, 0, 1);
            Check(!again.changed, "opening the same plot twice does nothing");

            board[1, 0].isFlagged = true;
            RevealOutcome flagged = BoardLogic.Reveal(board, 1, 0);
            Check(!flagged.changed && !board[1, 0].isRevealed, "a marked plot cannot be dug");

            RevealOutcome loss = BoardLogic.Reveal(board, 1, 1);
            Check(loss.hitWeed && !loss.won && board[1, 1].isRevealed, "digging a weed loses");
            Check(!board[0, 0].isRevealed, "a loss does not open the other plots");
        }

        static void LargeEmptyFlood()
        {
            TileData[,] board = BoardLogic.CreateHiddenBoard(30, 30);
            BoardLogic.CalculateAdjacentWeeds(board);
            RevealOutcome outcome = BoardLogic.Reveal(board, 15, 15);
            Check(outcome.won && BoardLogic.CountUnrevealedSafeTiles(board) == 0, "a large empty bed floods without recursion");
        }

        static void Check(bool condition, string message)
        {
            checks++;
            if (condition)
                return;

            failures++;
            Console.WriteLine("FAIL: " + message);
        }
    }
}
