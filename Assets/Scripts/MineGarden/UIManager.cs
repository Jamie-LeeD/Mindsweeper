using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineGarden
{
    /// <summary>
    /// Menu, timer, weed counter, and the win and loss panels.
    /// The default layout is built in code so the game can run without a hand-authored canvas.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        GameObject mainMenuPanel;
        GameObject gameplayPanel;
        GameObject victoryPanel;
        GameObject defeatPanel;
        GameObject pausePanel;
        TMP_Text timerText;
        TMP_Text weedText;
        TMP_Text hintText;
        TMP_Text victoryBody;
        TMP_Text defeatBody;
        Button easyButton;
        Button mediumButton;
        Button hardButton;
        Button hudRestartButton;
        Button hudMenuButton;
        Button victoryRestartButton;
        Button victoryMenuButton;
        Button defeatRestartButton;
        Button defeatMenuButton;
        bool bound;
        GameObject[] lifePoints;

        public RectTransform GridContainer { get; private set; }
        public GridLayoutGroup GridLayout { get; private set; }

        public void BuildDefaultInterface()
        {
            if (mainMenuPanel != null)
                return;

            var canvasObject = new GameObject("Mine Garden Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels =
                AdditionalCanvasShaderChannels.TexCoord1 |
                AdditionalCanvasShaderChannels.Normal |
                AdditionalCanvasShaderChannels.Tangent;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (Font() == null)
                Debug.LogError("Mine Garden could not find a TextMeshPro font. Import TMP Essentials.");

            mainMenuPanel = CreateScreen(canvasObject.transform, "Main Menu", GardenPalette.Background);
            BuildMenu(mainMenuPanel.transform);

            gameplayPanel = CreateScreen(canvasObject.transform, "Gameplay", GardenPalette.Background);
            BuildGameplay(gameplayPanel.transform);

            victoryPanel = CreateOverlay(canvasObject.transform, "Victory");
            victoryBody = BuildEndCard(victoryPanel.transform, "Garden clear", out victoryRestartButton, out victoryMenuButton);

            defeatPanel = CreateOverlay(canvasObject.transform, "Defeat");
            defeatBody = BuildEndCard(defeatPanel.transform, "Weed found", out defeatRestartButton, out defeatMenuButton);

            gameplayPanel.SetActive(false);
            victoryPanel.SetActive(false);
            defeatPanel.SetActive(false);
        }

        public void AdoptSceneInterface(GameObject win, GameObject lose, GameObject pause, TMP_Text timer, TMP_Text weeds, TMP_Text winMessage, TMP_Text loseMessage)
        {
            victoryPanel = win;
            defeatPanel = lose;
            pausePanel = pause;
            timerText = timer;
            weedText = weeds;
            victoryBody = winMessage;
            defeatBody = loseMessage;
            bound = true;
        }

        public void AdoptLifePoints(GameObject[] points)
        {
            lifePoints = points;
            SetLives(GameManager.MaxLives);
        }

        public void SetLives(int livesRemaining)
        {
            if (lifePoints == null)
                return;

            for (int i = 0; i < lifePoints.Length; i++)
            {
                if (lifePoints[i] != null)
                    lifePoints[i].SetActive(i < livesRemaining);
            }
        }

        public void ShowPause(bool show)
        {
            SetPanel(pausePanel, show);
            if (show && pausePanel != null)
                pausePanel.transform.SetAsLastSibling();
        }

        public void Bind(GameManager game)
        {
            if (game == null || bound)
                return;

            Wire(easyButton, () => game.StartGame(Difficulty.Easy));
            Wire(mediumButton, () => game.StartGame(Difficulty.Medium));
            Wire(hardButton, () => game.StartGame(Difficulty.Hard));
            Wire(hudRestartButton, game.Restart);
            Wire(hudMenuButton, game.ShowMainMenu);
            Wire(victoryRestartButton, game.Restart);
            Wire(victoryMenuButton, game.ShowMainMenu);
            Wire(defeatRestartButton, game.Restart);
            Wire(defeatMenuButton, game.ShowMainMenu);
            bound = true;
        }

        public void ShowMenu()
        {
            SetPanel(mainMenuPanel, true);
            SetPanel(gameplayPanel, false);
            SetPanel(victoryPanel, false);
            SetPanel(defeatPanel, false);
            SetPanel(pausePanel, false);
            if (mainMenuPanel != null)
                mainMenuPanel.transform.SetAsLastSibling();
        }

        public void ShowGameplay(Difficulty difficulty)
        {
            SetPanel(mainMenuPanel, false);
            SetPanel(gameplayPanel, true);
            SetPanel(victoryPanel, false);
            SetPanel(defeatPanel, false);
            SetPanel(pausePanel, false);
            if (hintText != null)
            {
                hintText.text = difficulty + " garden. Left click digs a plot. Right click plants a flower.";
            }
        }

        public void ShowVictory(float elapsed)
        {
            if (victoryBody != null)
                victoryBody.text = "The garden bed is clear.  Time " + FormatTime(elapsed);
            SetPanel(pausePanel, false);
            SetPanel(victoryPanel, true);
            if (victoryPanel != null)
                victoryPanel.transform.SetAsLastSibling();
        }

        public void ShowDefeat(float elapsed)
        {
            if (defeatBody != null)
                defeatBody.text = "The weeds took your last life.  Time " + FormatTime(elapsed);
            SetPanel(pausePanel, false);
            SetPanel(defeatPanel, true);
            if (defeatPanel != null)
                defeatPanel.transform.SetAsLastSibling();
        }

        public void SetTimer(float elapsed)
        {
            if (timerText != null)
                timerText.text = "Time  " + FormatTime(elapsed);
        }

        public void SetWeedCount(int count)
        {
            if (weedText != null)
                weedText.text = "Weeds  " + count;
        }

        public static string FormatTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return string.Format("{0}:{1:00}", total / 60, total % 60);
        }

        void BuildMenu(Transform parent)
        {
            var card = CreateCard(parent, "Menu Card", new Vector2(680f, 700f));
            CreateFixedText(card, "Title", "Mine Garden", 68f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 230f), new Vector2(600f, 90f));
            CreateFixedText(card, "Subtitle", "Clear every plot without uncovering a weed.", 28f, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0f, 145f), new Vector2(580f, 80f));
            CreateFixedText(card, "Controls", "Left click digs. Right click marks a flower.", 24f, FontStyles.Italic, TextAlignmentOptions.Center, new Vector2(0f, 70f), new Vector2(580f, 50f));

            var buttonColumn = CreateRect(card, "Difficulty Buttons");
            var columnRect = (RectTransform)buttonColumn.transform;
            columnRect.anchorMin = new Vector2(0.5f, 0.5f);
            columnRect.anchorMax = new Vector2(0.5f, 0.5f);
            columnRect.pivot = new Vector2(0.5f, 0.5f);
            columnRect.sizeDelta = new Vector2(520f, 280f);
            columnRect.anchoredPosition = new Vector2(0f, -130f);

            var layout = buttonColumn.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            easyButton = CreateButton(buttonColumn.transform, "Easy     6x6     5 weeds", null);
            mediumButton = CreateButton(buttonColumn.transform, "Medium     8x8     10 weeds", null);
            hardButton = CreateButton(buttonColumn.transform, "Hard     10x10     18 weeds", null);
            SetPreferredHeight(easyButton, 76f);
            SetPreferredHeight(mediumButton, 76f);
            SetPreferredHeight(hardButton, 76f);
        }

        void BuildGameplay(Transform parent)
        {
            var topBar = CreateImage("Top Bar", parent, GardenPalette.Cream, true);
            var topRect = (RectTransform)topBar.transform;
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.offsetMin = new Vector2(0f, -96f);
            topRect.offsetMax = new Vector2(0f, 0f);

            weedText = CreateFixedText(topBar.transform, "Weed Counter", "Weeds  0", 32f, FontStyles.Bold, TextAlignmentOptions.Left, Vector2.zero, new Vector2(280f, 64f));
            PlaceLeft(weedText.rectTransform, 36f, 280f, 64f);

            timerText = CreateFixedText(topBar.transform, "Timer", "Time  0:00", 32f, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, new Vector2(240f, 64f));
            PlaceCenter(timerText.rectTransform, 240f, 64f);

            hudRestartButton = CreateButton(topBar.transform, "Restart", null);
            PlaceRight(hudRestartButton.GetComponent<RectTransform>(), 180f, 64f, 220f);
            hudMenuButton = CreateButton(topBar.transform, "Main Menu", null);
            PlaceRight(hudMenuButton.GetComponent<RectTransform>(), 180f, 64f, 28f);

            var frame = CreateImage("Garden Frame", parent, GardenPalette.Frame, true);
            var frameRect = (RectTransform)frame.transform;
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(848f, 848f);
            frameRect.anchoredPosition = new Vector2(0f, -8f);

            var gridImage = CreateImage("Grid", frame.transform, GardenPalette.Frame, true);
            GridContainer = (RectTransform)gridImage.transform;
            GridContainer.anchorMin = new Vector2(0.5f, 0.5f);
            GridContainer.anchorMax = new Vector2(0.5f, 0.5f);
            GridContainer.pivot = new Vector2(0.5f, 0.5f);
            GridContainer.sizeDelta = new Vector2(800f, 800f);
            GridContainer.anchoredPosition = Vector2.zero;

            GridLayout = gridImage.gameObject.AddComponent<GridLayoutGroup>();
            GridLayout.padding = new RectOffset(12, 12, 12, 12);
            GridLayout.spacing = new Vector2(8f, 8f);
            GridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            GridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            GridLayout.childAlignment = TextAnchor.MiddleCenter;
            GridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            GridLayout.constraintCount = 1;

            hintText = CreateFixedText(parent, "Hint", "Left click digs a plot. Right click plants a flower.", 24f, FontStyles.Italic, TextAlignmentOptions.Center, Vector2.zero, new Vector2(1200f, 40f));
            var hintRect = hintText.rectTransform;
            hintRect.anchorMin = new Vector2(0.5f, 0f);
            hintRect.anchorMax = new Vector2(0.5f, 0f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.sizeDelta = new Vector2(1200f, 40f);
            hintRect.anchoredPosition = new Vector2(0f, 18f);
        }

        TMP_Text BuildEndCard(Transform overlay, string title, out Button restartButton, out Button menuButton)
        {
            var card = CreateCard(overlay, "Card", new Vector2(640f, 420f));
            CreateFixedText(card, "Title", title, 56f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 110f), new Vector2(560f, 80f));
            TMP_Text body = CreateFixedText(card, "Body", string.Empty, 28f, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0f, 20f), new Vector2(560f, 100f));

            restartButton = CreateButton(card, "Restart", null);
            var restartRect = restartButton.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(0f, 0f);
            restartRect.anchorMax = new Vector2(0f, 0f);
            restartRect.pivot = new Vector2(0f, 0f);
            restartRect.sizeDelta = new Vector2(250f, 68f);
            restartRect.anchoredPosition = new Vector2(48f, 40f);

            menuButton = CreateButton(card, "Main Menu", null);
            var menuRect = menuButton.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(1f, 0f);
            menuRect.anchorMax = new Vector2(1f, 0f);
            menuRect.pivot = new Vector2(1f, 0f);
            menuRect.sizeDelta = new Vector2(250f, 68f);
            menuRect.anchoredPosition = new Vector2(-48f, 40f);
            return body;
        }

        GameObject CreateScreen(Transform parent, string name, Color color)
        {
            return CreateImage(name, parent, color, true).gameObject;
        }

        GameObject CreateOverlay(Transform parent, string name)
        {
            var overlay = CreateImage(name, parent, GardenPalette.Overlay, true);
            Stretch((RectTransform)overlay.transform);
            return overlay.gameObject;
        }

        Transform CreateCard(Transform parent, string name, Vector2 size)
        {
            var image = CreateImage(name, parent, GardenPalette.Cream, true);
            var rect = (RectTransform)image.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return image.transform;
        }

        Image CreateImage(string name, Transform parent, Color color, bool raycast)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Stretch((RectTransform)imageObject.transform);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        GameObject CreateRect(Transform parent, string name)
        {
            var rectObject = new GameObject(name, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            return rectObject;
        }

        TMP_Text CreateFixedText(Transform parent, string name, string content, float size, FontStyles style, TextAlignmentOptions alignment, Vector2 position, Vector2 box)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = box;
            rect.anchoredPosition = position;

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = Font();
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = GardenPalette.DarkGreen;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var image = CreateImage(label + " Button", parent, Color.white, true);
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = GardenPalette.Button;
            colors.highlightedColor = GardenPalette.ButtonHover;
            colors.pressedColor = GardenPalette.ButtonPressed;
            colors.selectedColor = GardenPalette.Button;
            colors.disabledColor = new Color(0.62f, 0.66f, 0.60f, 1f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            var text = CreateFixedText(image.transform, "Label", label, 26f, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.zero);
            Stretch(text.rectTransform);
            text.color = GardenPalette.Cream;

            if (onClick != null)
                button.onClick.AddListener(onClick);

            return button;
        }

        static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(UiClick.Play);
            button.onClick.AddListener(action);
        }

        static void SetPreferredHeight(Button button, float height)
        {
            var element = button.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
        }

        static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void PlaceLeft(RectTransform rect, float x, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, 0f);
        }

        static void PlaceCenter(RectTransform rect, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = Vector2.zero;
        }

        static void PlaceRight(RectTransform rect, float width, float height, float inset)
        {
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(-inset, 0f);
        }

        TMP_FontAsset cachedFont;

        TMP_FontAsset Font()
        {
            if (cachedFont == null)
                cachedFont = TileView.DefaultFont();
            return cachedFont;
        }
    }
}
