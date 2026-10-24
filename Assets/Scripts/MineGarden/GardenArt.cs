using UnityEngine;

namespace MineGarden
{
    /// <summary>
    /// The plot sprites in Assets/Tiles. Loaded from Resources, with an editor fallback if a reference is missing.
    /// </summary>
    public class GardenArt : ScriptableObject
    {
        public Sprite hiddenTile;
        public Sprite revealedTile;
        public Sprite numberedTile;
        public Sprite weedTile;
        public Sprite flagTile;

        public static GardenArt Load()
        {
            GardenArt art = Resources.Load<GardenArt>("GardenSprites");
            if (art == null)
                art = CreateInstance<GardenArt>();

#if UNITY_EDITOR
            art.FillMissingFromProject();
#endif
            return art;
        }

#if UNITY_EDITOR
        void FillMissingFromProject()
        {
            hiddenTile = hiddenTile != null ? hiddenTile : LoadSprite("Assets/Tiles/HiddenTile.png");
            revealedTile = revealedTile != null ? revealedTile : LoadSprite("Assets/Tiles/RevealedTile.png");
            numberedTile = numberedTile != null ? numberedTile : LoadSprite("Assets/Tiles/NumberedTile.png");
            weedTile = weedTile != null ? weedTile : LoadSprite("Assets/Tiles/HiddenHazard.png");
            flagTile = flagTile != null ? flagTile : LoadSprite("Assets/Tiles/WeedMarker.png");
        }

        static Sprite LoadSprite(string path)
        {
            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                    return sprite;
            }

            return null;
        }
#endif
    }
}
