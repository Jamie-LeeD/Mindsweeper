namespace MineGarden
{
    /// <summary>
    /// One garden plot. This is plain data and is not a scene object.
    /// </summary>
    public class TileData
    {
        public int x;
        public int y;
        public bool hasWeed;
        public bool isRevealed;
        public bool isFlagged;
        public int adjacentWeeds;
    }
}
