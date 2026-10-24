namespace MineGarden
{
    public enum GameState
    {
        Menu,
        Playing,
        Won,
        Lost
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    /// <summary>
    /// Remembers the garden picked on the main menu until GameScene loads.
    /// </summary>
    public static class GameSession
    {
        public static Difficulty Selected = Difficulty.Easy;
    }

    public static class DifficultyPresets
    {
        public static void Get(Difficulty difficulty, out int width, out int height, out int weedCount)
        {
            switch (difficulty)
            {
                case Difficulty.Medium:
                    width = 8;
                    height = 8;
                    weedCount = 10;
                    return;
                case Difficulty.Hard:
                    width = 10;
                    height = 10;
                    weedCount = 18;
                    return;
                default:
                    width = 6;
                    height = 6;
                    weedCount = 5;
                    return;
            }
        }
    }
}
