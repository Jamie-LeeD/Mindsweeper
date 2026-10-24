using MineGarden;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MineGarden.Editor
{
    /// <summary>
    /// Adds the play-mode UI into MainMenuScene and GameScene so it can be selected and edited.
    /// Existing objects are left alone.
    /// </summary>
    [InitializeOnLoad]
    static class MineGardenSceneBuilder
    {
        static MineGardenSceneBuilder()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.delayCall += BuildOpenScenes;
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            EditorApplication.delayCall += BuildOpenScenes;
        }

        static void BuildOpenScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                    continue;

                bool changed = false;
                if (scene.name == GameManager.MainMenuScene)
                    changed = BuildMainMenu(scene);
                else if (scene.name == GameManager.GameScene)
                    changed = BuildGame(scene);

                if (changed)
                    EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        static bool BuildMainMenu(Scene scene)
        {
            Transform menu = FindInScene(scene, "MainMenu");
            if (menu == null)
                return false;

            bool changed = false;
            changed |= EnsureTitle(menu);
            changed |= EnsureDifficultyMenu(menu);
            changed |= LabelIfPlaceholder(FindChild(menu, "PlayButton"), "Play");
            changed |= LabelIfPlaceholder(FindChild(menu, "ControlsButton"), "Controls");
            changed |= LabelIfPlaceholder(FindChild(menu, "QuitButton"), "Quit");

            Transform controls = FindInScene(scene, "ControlsMenu");
            changed |= LabelIfPlaceholder(controls != null ? FindChild(controls, "CloseButton") : null, "Close");
            changed |= FillControls(controls);
            return changed;
        }

        static bool BuildGame(Scene scene)
        {
            Transform canvas = FindInScene(scene, "Canvas");
            Transform hud = FindInScene(scene, "HUD");
            if (canvas == null || hud == null)
                return false;

            bool changed = false;
            changed |= EnsureGardenBed(canvas);
            changed |= EnsureHudText(hud, "Weed Counter", new Vector2(250f, -28f), "Weeds  0");
            changed |= EnsureHudText(hud, "Timer", new Vector2(470f, -28f), "Time  0:00");
            changed |= EnsurePauseButton(hud, FindInScene(scene, "PauseMenu"));
            changed |= EnsureResult(FindInScene(scene, "WinMenu"), "Garden clear");
            changed |= EnsureResult(FindInScene(scene, "LoseMenu"), "Weed found");
            changed |= LabelNamedButtons(canvas);
            changed |= FillControls(FindInScene(scene, "ControlsMenu"));
            return changed;
        }

        static bool EnsureTitle(Transform menu)
        {
            if (FindChild(menu, "Title") != null)
                return false;

            CreateText(menu, "Title", "Mine Garden", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(420f, 60f), 32f, TextAlignmentOptions.Center);
            return true;
        }

        static bool EnsureDifficultyMenu(Transform menu)
        {
            Button template = FindChild(menu, "PlayButton") != null ? FindChild(menu, "PlayButton").GetComponent<Button>() : null;
            if (template == null)
                return false;

            Transform difficulty = FindChild(menu, "DifficultyMenu");
            bool changed = false;
            if (difficulty == null)
            {
                var menuObject = new GameObject("DifficultyMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                Undo.RegisterCreatedObjectUndo(menuObject, "Add Mine Garden UI");
                menuObject.transform.SetParent(menu, false);
                var rect = (RectTransform)menuObject.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                var image = menuObject.GetComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0f);
                image.raycastTarget = false;
                menuObject.SetActive(false);
                difficulty = menuObject.transform;
                changed = true;
            }

            changed |= EnsureChoice(template, difficulty, "EasyButton", "Easy", new Vector2(0f, -59f));
            changed |= EnsureChoice(template, difficulty, "MediumButton", "Medium", new Vector2(0f, -104f));
            changed |= EnsureChoice(template, difficulty, "HardButton", "Hard", new Vector2(0f, -148f));
            changed |= EnsureChoice(template, difficulty, "BackButton", "Back", new Vector2(0f, -192f));
            return changed;
        }

        static bool EnsureChoice(Button template, Transform parent, string name, string label, Vector2 position)
        {
            if (FindChild(parent, name) != null)
                return false;

            GameObject copy = Object.Instantiate(template.gameObject, parent);
            copy.name = name;
            copy.SetActive(true);
            Undo.RegisterCreatedObjectUndo(copy, "Add Mine Garden UI");
            var rect = copy.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            var button = copy.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            TMP_Text text = copy.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
                text.text = label;
            return true;
        }

        static bool EnsureGardenBed(Transform canvas)
        {
            if (FindChild(canvas, "GardenBed") != null)
                return false;

            var bed = new GameObject("GardenBed", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(GridLayoutGroup));
            Undo.RegisterCreatedObjectUndo(bed, "Add Mine Garden UI");
            bed.transform.SetParent(canvas, false);
            bed.transform.SetSiblingIndex(0);

            var rect = (RectTransform)bed.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -36f);
            rect.sizeDelta = new Vector2(-48f, -120f);

            var image = bed.GetComponent<Image>();
            image.color = new Color(0.55f, 0.40f, 0.28f, 0.35f);
            image.raycastTarget = false;

            var layout = bed.GetComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = new Vector2(4f, 4f);
            layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 1;
            return true;
        }

        static bool EnsureHudText(Transform hud, string name, Vector2 position, string content)
        {
            if (FindChild(hud, name) != null)
                return false;

            CreateText(hud, name, content, new Vector2(0f, 1f), new Vector2(0f, 1f), position, new Vector2(220f, 40f), 22f, TextAlignmentOptions.Left);
            return true;
        }

        static bool EnsurePauseButton(Transform hud, Transform pause)
        {
            if (hud == null || FindChild(hud, "PauseButton") != null || pause == null)
                return false;

            Transform template = FindChild(pause, "ResumeButton");
            if (template == null)
                return false;

            GameObject copy = Object.Instantiate(template.gameObject, hud);
            copy.name = "PauseButton";
            copy.SetActive(true);
            Undo.RegisterCreatedObjectUndo(copy, "Add Mine Garden UI");
            var rect = copy.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(140f, 36f);
            rect.anchoredPosition = new Vector2(-16f, -16f);
            var button = copy.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            TMP_Text text = copy.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
                text.text = "Pause";
            return true;
        }

        static bool EnsureResult(Transform menu, string content)
        {
            if (menu == null || FindChild(menu, "ResultText") != null)
                return false;

            Transform background = FindChild(menu, "Background");
            if (background == null)
                return false;

            CreateText(background, "ResultText", content, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(310f, 48f), 18f, TextAlignmentOptions.Center);
            return true;
        }

        static void CreateText(Transform parent, string name, string content, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(textObject, "Add Mine Garden UI");
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = TileView.DefaultFont();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = alignment;
            text.color = new Color(0.12f, 0.22f, 0.12f);
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
        }

        static bool LabelNamedButtons(Transform root)
        {
            bool changed = false;
            changed |= LabelIfPlaceholder(FindChild(root, "NextLevelButton"), "Next Level");
            changed |= LabelIfPlaceholder(FindChild(root, "RetryButton"), "Retry");
            changed |= LabelIfPlaceholder(FindChild(root, "ResumeButton"), "Resume");
            changed |= LabelIfPlaceholder(FindChild(root, "ControlsButton"), "Controls");
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "MainMenuButton")
                    changed |= LabelIfPlaceholder(child, "Main Menu");
                else if (child.name == "QuitButton")
                    changed |= LabelIfPlaceholder(child, "Quit");
                else if (child.name == "CloseButton")
                    changed |= LabelIfPlaceholder(child, "Close");
            }

            return changed;
        }

        static bool LabelIfPlaceholder(Transform buttonObject, string label)
        {
            if (buttonObject == null)
                return false;

            TMP_Text text = buttonObject.GetComponentInChildren<TMP_Text>(true);
            if (text == null || text.text != "Button")
                return false;

            Undo.RecordObject(text, "Label Mine Garden button");
            text.text = label;
            EditorUtility.SetDirty(text);
            return true;
        }

        static bool FillControls(Transform controls)
        {
            if (controls == null)
                return false;

            Transform info = FindChild(controls, "InformationText (TMP)");
            TMP_Text text = info != null ? info.GetComponent<TMP_Text>() : null;
            if (text == null || text.text != "New Text")
                return false;

            Undo.RecordObject(text, "Fill Mine Garden controls");
            text.text = "Left click digs a plot.\nRight click plants a flower.\n\nDigging a weed costs a life. You have three.\nA new level restores them.\nThe first dig is always safe.";
            text.textWrappingMode = TextWrappingModes.Normal;
            EditorUtility.SetDirty(text);
            return true;
        }

        static Transform FindInScene(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform found = FindChild(roots[i].transform, name);
                if (found != null)
                    return found;
            }

            return null;
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
    }
}
