using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MineGarden
{
    /// <summary>
    /// Connects Mine Garden to the existing MainMenuScene and GameScene buttons.
    /// </summary>
    public static class SceneFlow
    {
        const string ControlsCopy =
            "Left click digs a plot.\nRight click plants a flower.\n\nDigging a weed costs a life. You have three.\nA new level restores them.\nThe first dig is always safe.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void HookScene()
        {
            // AfterSceneLoad runs only for the first scene. Play loads GameScene later, so listen for every load.
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartScene(SceneManager.GetActiveScene());
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartScene(scene);
        }

        static void StartScene(Scene scene)
        {
            if (scene.name == GameManager.MainMenuScene)
                WireMainMenu();
            else if (scene.name == GameManager.GameScene)
                WireGame();
        }

        static void WireMainMenu()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null || canvas.GetComponent<SceneHook>() != null)
                return;

            canvas.gameObject.AddComponent<SceneHook>();
            UiClick.HookAll(canvas.transform);

            Transform root = canvas.transform;
            Transform menu = FindChild(root, "MainMenu");
            Transform controls = FindChild(root, "ControlsMenu");
            if (menu == null)
                return;

            Button play = FindButton(menu, "PlayButton");
            Button controlsButton = FindButton(menu, "ControlsButton");
            Button quit = FindButton(menu, "QuitButton");
            Button close = controls != null ? FindButton(controls, "CloseButton") : null;
            LabelIfPlaceholder(play, "Play");
            LabelIfPlaceholder(controlsButton, "Controls");
            LabelIfPlaceholder(quit, "Quit");
            LabelIfPlaceholder(close, "Close");
            FillDefaultControls(controls);

            Transform difficultyMenu = FindChild(menu, "DifficultyMenu");
            if (difficultyMenu != null)
                difficultyMenu.gameObject.SetActive(false);

            Button easy = FindDifficultyButton(menu, difficultyMenu, "EasyButton");
            Button medium = FindDifficultyButton(menu, difficultyMenu, "MediumButton");
            Button hard = FindDifficultyButton(menu, difficultyMenu, "HardButton");
            Button back = FindDifficultyButton(menu, difficultyMenu, "BackButton");

            if (play != null)
            {
                play.onClick.AddListener(() =>
                {
                    Show(play, false);
                    Show(controlsButton, false);
                    Show(quit, false);
                    if (difficultyMenu != null)
                        difficultyMenu.gameObject.SetActive(true);
                    else if (easy == null)
                        CreateDifficultyButtons(play);
                });
            }

            WireDifficulty(easy, Difficulty.Easy);
            WireDifficulty(medium, Difficulty.Medium);
            WireDifficulty(hard, Difficulty.Hard);
            if (back != null)
            {
                back.onClick.AddListener(() =>
                {
                    if (difficultyMenu != null)
                        difficultyMenu.gameObject.SetActive(false);
                    Show(play, true);
                    Show(controlsButton, true);
                    Show(quit, true);
                });
            }

            if (controlsButton != null && controls != null)
                controlsButton.onClick.AddListener(() => controls.gameObject.SetActive(true));
            if (close != null && controls != null)
                close.onClick.AddListener(() => controls.gameObject.SetActive(false));
            if (quit != null)
                quit.onClick.AddListener(GameManager.QuitGame);

            CreateTitle(menu, "Mine Garden", new Vector2(0f, 40f));
        }

        static List<GameObject> CreateDifficultyButtons(Button template)
        {
            var created = new List<GameObject>();
            created.Add(MakeChoice(template, "Easy", new Vector2(0f, -59f), Difficulty.Easy));
            created.Add(MakeChoice(template, "Medium", new Vector2(0f, -104f), Difficulty.Medium));
            created.Add(MakeChoice(template, "Hard", new Vector2(0f, -148f), Difficulty.Hard));

            GameObject backObject = Object.Instantiate(template.gameObject, template.transform.parent);
            backObject.name = "BackButton";
            var backRect = backObject.GetComponent<RectTransform>();
            backRect.anchoredPosition = new Vector2(0f, -192f);
            var back = backObject.GetComponent<Button>();
            back.onClick = new Button.ButtonClickedEvent();
            Label(back, "Back");
            UiClick.Hook(back);
            back.onClick.AddListener(() =>
            {
                for (int i = 0; i < created.Count; i++)
                    created[i].SetActive(false);
                Show(template, true);
                Show(FindButton(template.transform.parent, "ControlsButton"), true);
                Show(FindButton(template.transform.parent, "QuitButton"), true);
            });
            created.Add(backObject);
            return created;
        }

        static GameObject MakeChoice(Button template, string label, Vector2 position, Difficulty difficulty)
        {
            GameObject choice = Object.Instantiate(template.gameObject, template.transform.parent);
            choice.name = label + "Button";
            choice.GetComponent<RectTransform>().anchoredPosition = position;
            var button = choice.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            Label(button, label);
            UiClick.Hook(button);
            button.onClick.AddListener(() =>
            {
                GameSession.Selected = difficulty;
                SceneManager.LoadScene(GameManager.GameScene);
            });
            return choice;
        }

        static void WireGame()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null || canvas.GetComponent<SceneHook>() != null)
                return;

            canvas.gameObject.AddComponent<SceneHook>();
            UiClick.HookAll(canvas.transform);
            Canvas.ForceUpdateCanvases();

            Transform root = canvas.transform;
            Transform hud = FindChild(root, "HUD");
            Transform win = FindChild(root, "WinMenu");
            Transform lose = FindChild(root, "LoseMenu");
            Transform pause = FindChild(root, "PauseMenu");
            Transform controls = FindChild(root, "ControlsMenu");

            LabelIfPlaceholder(FindButton(win, "NextLevelButton"), "Next Level");
            LabelIfPlaceholder(FindButton(lose, "RetryButton"), "Retry");
            LabelIfPlaceholder(FindButton(pause, "ResumeButton"), "Resume");
            LabelIfPlaceholder(FindButton(pause, "ControlsButton"), "Controls");
            LabelEveryPlaceholder(root, "MainMenuButton", "Main Menu");
            LabelEveryPlaceholder(root, "QuitButton", "Quit");
            LabelEveryPlaceholder(root, "CloseButton", "Close");
            FillDefaultControls(controls);

            TMP_Text weeds = CreateHudText(hud, "Weed Counter", new Vector2(250f, -28f), "Weeds  0");
            TMP_Text timer = CreateHudText(hud, "Timer", new Vector2(470f, -28f), "Time  0:00");
            TMP_Text winMessage = CreateResultText(win, "Garden clear");
            TMP_Text loseMessage = CreateResultText(lose, "Weed found");

            Button pauseButton = CreatePauseButton(hud, pause);
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.PauseGame(); });

            WireMenu(pause, "ResumeButton", () => { if (GameManager.Instance != null) GameManager.Instance.Resume(); });
            WireMenu(pause, "ControlsButton", () => { if (controls != null) { controls.gameObject.SetActive(true); controls.SetAsLastSibling(); } });
            WireMenu(controls, "CloseButton", () => { if (controls != null) controls.gameObject.SetActive(false); });
            WireMenu(lose, "RetryButton", () => { if (GameManager.Instance != null) GameManager.Instance.Restart(); });
            WireMenu(win, "NextLevelButton", () => { if (GameManager.Instance != null) GameManager.Instance.NextGarden(); });
            WireEvery(root, "MainMenuButton", () => { if (GameManager.Instance != null) GameManager.Instance.LoadMainMenu(); });
            WireEvery(root, "QuitButton", GameManager.QuitGame);

            RectTransform gridRect = CreateBed(root);
            var host = new GameObject("MineGarden");
            var game = host.AddComponent<GameManager>();
            var grid = host.AddComponent<GridManager>();
            var ui = host.AddComponent<UIManager>();
            ui.AdoptSceneInterface(
                win != null ? win.gameObject : null,
                lose != null ? lose.gameObject : null,
                pause != null ? pause.gameObject : null,
                timer,
                weeds,
                winMessage,
                loseMessage);
            grid.Attach(gridRect, gridRect.GetComponent<GridLayoutGroup>());
            grid.UseArt(GardenArt.Load());
            ui.AdoptLifePoints(FindLifePoints(hud));
            game.Attach(grid, ui);
            game.StartGame(GameSession.Selected);
        }

        static RectTransform CreateBed(Transform canvas)
        {
            Transform existing = FindChild(canvas, "GardenBed");
            if (existing is RectTransform existingRect)
            {
                if (existingRect.GetComponent<GridLayoutGroup>() == null)
                    existingRect.gameObject.AddComponent<GridLayoutGroup>();
                return existingRect;
            }

            var bed = new GameObject("GardenBed", typeof(RectTransform), typeof(GridLayoutGroup));
            bed.transform.SetParent(canvas, false);
            bed.transform.SetSiblingIndex(0);
            var rect = (RectTransform)bed.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Rect canvasRect = ((RectTransform)canvas).rect;
            float width = canvasRect.width > 2f ? canvasRect.width : Screen.width;
            float height = canvasRect.height > 2f ? canvasRect.height : Screen.height;
            float size = Mathf.Max(240f, Mathf.Min(width - 48f, height - 150f));
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(0f, -24f);

            var layout = bed.GetComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = new Vector2(4f, 4f);
            layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 1;
            return rect;
        }

        static GameObject[] FindLifePoints(Transform hud)
        {
            Transform bar = hud != null ? FindChild(hud, "HPBar") : null;
            if (bar == null)
                return new GameObject[0];

            var points = new List<GameObject>();
            for (int i = 0; i < bar.childCount; i++)
            {
                Transform child = bar.GetChild(i);
                if (child.name.StartsWith("HpPoint"))
                    points.Add(child.gameObject);
            }

            return points.ToArray();
        }

        static TMP_Text CreateHudText(Transform hud, string name, Vector2 position, string content)
        {
            if (hud == null)
                return null;

            Transform existing = FindChild(hud, name);
            if (existing != null)
                return existing.GetComponent<TMP_Text>();

            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(hud, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(210f, 36f);
            rect.anchoredPosition = position;

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = TileView.DefaultFont();
            text.fontSize = 22f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Left;
            text.color = new Color(0.12f, 0.22f, 0.12f);
            text.raycastTarget = false;
            text.text = content;
            return text;
        }

        static TMP_Text CreateResultText(Transform menu, string content)
        {
            Transform background = menu != null ? FindChild(menu, "Background") : null;
            if (background == null)
                return null;

            Transform existing = FindChild(menu, "ResultText");
            if (existing != null)
                return existing.GetComponent<TMP_Text>();

            var textObject = new GameObject("ResultText", typeof(RectTransform));
            textObject.transform.SetParent(background, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(310f, 44f);
            rect.anchoredPosition = new Vector2(0f, -8f);

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = TileView.DefaultFont();
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.12f, 0.18f, 0.12f);
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.text = content;
            return text;
        }

        static void CreateTitle(Transform parent, string content, Vector2 position)
        {
            if (parent == null || FindChild(parent, "Title") != null)
                return;
            var textObject = new GameObject("Title", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(420f, 48f);
            rect.anchoredPosition = position;

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = TileView.DefaultFont();
            text.fontSize = 32f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.15f, 0.28f, 0.16f);
            text.raycastTarget = false;
            text.text = content;
        }

        static Button CreatePauseButton(Transform hud, Transform pause)
        {
            if (hud == null || pause == null)
                return null;

            Button existing = FindButton(hud, "PauseButton");
            if (existing != null)
                return existing;

            Button template = FindButton(pause, "ResumeButton");
            if (template == null)
                return null;

            GameObject buttonObject = Object.Instantiate(template.gameObject, hud);
            buttonObject.name = "PauseButton";
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(120f, 30f);
            rect.anchoredPosition = new Vector2(-16f, -16f);
            var button = buttonObject.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            Label(button, "Pause");
            UiClick.Hook(button);
            return button;
        }

        static Button FindDifficultyButton(Transform menu, Transform difficultyMenu, string name)
        {
            if (difficultyMenu != null)
            {
                Button nested = FindButton(difficultyMenu, name);
                if (nested != null)
                    return nested;
            }

            return FindButton(menu, name);
        }

        static void WireDifficulty(Button button, Difficulty difficulty)
        {
            if (button == null)
                return;

            button.onClick.AddListener(() =>
            {
                GameSession.Selected = difficulty;
                SceneManager.LoadScene(GameManager.GameScene);
            });
        }

        static void LabelIfPlaceholder(Button button, string text)
        {
            if (button == null)
                return;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null && label.text == "Button")
                label.text = text;
        }

        static void LabelEveryPlaceholder(Transform root, string buttonName, string text)
        {
            if (root == null)
                return;

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].name == buttonName)
                    LabelIfPlaceholder(buttons[i], text);
            }
        }

        static void FillDefaultControls(Transform controls)
        {
            if (controls == null)
                return;

            Transform info = FindChild(controls, "InformationText (TMP)");
            TMP_Text text = info != null ? info.GetComponent<TMP_Text>() : null;
            if (text != null && text.text == "New Text")
                text.text = ControlsCopy;
        }

        static void WireMenu(Transform menu, string buttonName, UnityEngine.Events.UnityAction action)
        {
            Button button = menu != null ? FindButton(menu, buttonName) : null;
            if (button == null)
                return;

            button.onClick.AddListener(action);
        }

        static void WireEvery(Transform root, string buttonName, UnityEngine.Events.UnityAction action)
        {
            if (root == null)
                return;

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].name == buttonName)
                    buttons[i].onClick.AddListener(action);
            }
        }

        static void Label(Button button, string text)
        {
            if (button == null)
                return;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
                return;

            label.text = text;
            label.fontSize = 16f;
            label.enableAutoSizing = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.alignment = TextAlignmentOptions.Center;
        }

        static Button FindButton(Transform root, string name)
        {
            Transform found = FindChild(root, name);
            return found != null ? found.GetComponent<Button>() : null;
        }

        static Transform FindChild(Transform root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChild(root.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }

        static void Show(Button button, bool visible)
        {
            if (button != null)
                button.gameObject.SetActive(visible);
        }

        sealed class SceneHook : MonoBehaviour
        {
        }
    }
}
