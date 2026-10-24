using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MineGarden
{
    public static class GardenPalette
    {
        public static readonly Color Background = new Color(0.769f, 0.867f, 0.722f);
        public static readonly Color Cream = new Color(0.965f, 0.941f, 0.863f);
        public static readonly Color DarkGreen = new Color(0.118f, 0.227f, 0.184f);
        public static readonly Color Button = new Color(0.184f, 0.420f, 0.310f);
        public static readonly Color ButtonHover = new Color(0.247f, 0.510f, 0.380f);
        public static readonly Color ButtonPressed = new Color(0.133f, 0.322f, 0.235f);
        public static readonly Color Hidden = new Color(0.561f, 0.749f, 0.478f);
        public static readonly Color Revealed = new Color(0.769f, 0.647f, 0.455f);
        public static readonly Color Flag = new Color(0.898f, 0.420f, 0.541f);
        public static readonly Color Weed = new Color(0.290f, 0.173f, 0.353f);
        public static readonly Color WeedIcon = new Color(0.910f, 0.820f, 0.965f);
        public static readonly Color Frame = new Color(0.545f, 0.420f, 0.290f);
        public static readonly Color Overlay = new Color(0.102f, 0.157f, 0.122f, 0.55f);

        public static Color Number(int count)
        {
            switch (count)
            {
                case 1: return new Color(0.16f, 0.42f, 0.30f);
                case 2: return new Color(0.25f, 0.40f, 0.62f);
                case 3: return new Color(0.70f, 0.28f, 0.32f);
                case 4: return new Color(0.35f, 0.28f, 0.55f);
                case 5: return new Color(0.55f, 0.32f, 0.18f);
                case 6: return new Color(0.20f, 0.50f, 0.52f);
                case 7: return new Color(0.25f, 0.22f, 0.20f);
                default: return new Color(0.45f, 0.40f, 0.38f);
            }
        }
    }

    /// <summary>
    /// Draws one plot and forwards clicks. It does not decide what a click means.
    /// </summary>
    public class TileView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image background;
        [SerializeField] TMP_Text numberLabel;
        [SerializeField] Image marker;
        [SerializeField] TMP_Text iconLabel;
        [SerializeField] Color hiddenColor = new Color(0.561f, 0.749f, 0.478f);
        [SerializeField] Color revealedColor = new Color(0.769f, 0.647f, 0.455f);
        [SerializeField] Color flagColor = new Color(0.898f, 0.420f, 0.541f);
        [SerializeField] Color weedColor = new Color(0.290f, 0.173f, 0.353f);
        [SerializeField] Color weedIconColor = new Color(0.910f, 0.820f, 0.965f);

        TileData data;
        GridManager grid;
        Sprite hiddenSprite;
        Sprite revealedSprite;
        Sprite numberedSprite;
        Sprite weedSprite;
        Sprite flagSprite;

        public void UseSprites(GardenArt gardenArt)
        {
            if (gardenArt == null)
                return;

            hiddenSprite = gardenArt.hiddenTile;
            revealedSprite = gardenArt.revealedTile;
            numberedSprite = gardenArt.numberedTile;
            weedSprite = gardenArt.weedTile;
            flagSprite = gardenArt.flagTile;
            if (background != null)
                background.preserveAspect = true;
        }

        public static TileView CreateRuntime(Transform parent)
        {
            var tileObject = new GameObject("Tile", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TileView));
            tileObject.transform.SetParent(parent, false);

            var view = tileObject.GetComponent<TileView>();
            var backgroundImage = tileObject.GetComponent<Image>();
            backgroundImage.color = GardenPalette.Hidden;
            backgroundImage.raycastTarget = true;

            Image markerImage = CreateMarker(tileObject.transform);
            TMP_Text number = CreateLabel(tileObject.transform, "Number");
            TMP_Text icon = CreateLabel(tileObject.transform, "Icon");
            view.Configure(backgroundImage, number, markerImage, icon);
            return view;
        }

        public void Configure(Image backgroundImage, TMP_Text number, Image markerImage, TMP_Text icon)
        {
            background = backgroundImage;
            numberLabel = number;
            marker = markerImage;
            iconLabel = icon;
        }

        public void Bind(TileData tileData, GridManager gridManager)
        {
            data = tileData;
            grid = gridManager;
            Refresh();
        }

        public void Refresh()
        {
            if (data == null || background == null)
                return;

            SetActive(numberLabel, false);
            SetActive(marker, false);
            SetActive(iconLabel, false);

            if (data.isRevealed && data.hasWeed)
            {
                ShowBackground(weedSprite, weedColor);
                if (weedSprite == null && iconLabel != null)
                {
                    iconLabel.gameObject.SetActive(true);
                    iconLabel.text = "W";
                    iconLabel.color = weedIconColor;
                }

                return;
            }

            if (!data.isRevealed && data.isFlagged)
            {
                ShowBackground(flagSprite != null ? flagSprite : hiddenSprite, hiddenColor);
                if (flagSprite == null)
                {
                    if (marker != null)
                    {
                        marker.gameObject.SetActive(true);
                        marker.color = flagColor;
                    }

                    if (iconLabel != null)
                    {
                        iconLabel.gameObject.SetActive(true);
                        iconLabel.text = "\u273f";
                        iconLabel.color = Color.white;
                    }
                }

                return;
            }

            if (!data.isRevealed)
            {
                ShowBackground(hiddenSprite, hiddenColor);
                return;
            }

            bool numbered = data.adjacentWeeds > 0;
            ShowBackground(numbered && numberedSprite != null ? numberedSprite : revealedSprite, revealedColor);
            if (numbered && numberLabel != null)
            {
                numberLabel.gameObject.SetActive(true);
                numberLabel.text = data.adjacentWeeds.ToString();
                numberLabel.color = numberedSprite != null ? new Color(0.12f, 0.22f, 0.1f) : GardenPalette.Number(data.adjacentWeeds);
            }
        }

        void ShowBackground(Sprite sprite, Color fallback)
        {
            background.sprite = sprite;
            background.color = sprite != null ? Color.white : fallback;
            background.preserveAspect = sprite != null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (grid == null || data == null || eventData == null)
                return;

            if (eventData.button == PointerEventData.InputButton.Right)
                grid.RequestFlag(data.x, data.y);
            else if (eventData.button == PointerEventData.InputButton.Left)
                grid.RequestReveal(data.x, data.y);
        }

        static Image CreateMarker(Transform parent)
        {
            var markerObject = new GameObject("Marker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            markerObject.transform.SetParent(parent, false);
            var rect = (RectTransform)markerObject.transform;
            rect.anchorMin = new Vector2(0.22f, 0.22f);
            rect.anchorMax = new Vector2(0.78f, 0.78f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = markerObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.color = GardenPalette.Flag;
            markerObject.SetActive(false);
            return image;
        }

        static TMP_Text CreateLabel(Transform parent, string name)
        {
            var labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);
            Stretch((RectTransform)labelObject.transform);

            var text = labelObject.AddComponent<TextMeshProUGUI>();
            text.font = DefaultFont();
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.enableAutoSizing = true;
            text.fontSizeMin = 12f;
            text.fontSizeMax = 64f;
            text.fontStyle = FontStyles.Bold;
            text.overflowMode = TextOverflowModes.Truncate;
            text.text = string.Empty;
            labelObject.SetActive(false);
            return text;
        }

        public static TMP_FontAsset DefaultFont()
        {
            if (TMP_Settings.defaultFontAsset != null)
                return TMP_Settings.defaultFontAsset;

            return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void SetActive(Component component, bool active)
        {
            if (component != null)
                component.gameObject.SetActive(active);
        }
    }
}
