using UnityEngine;
using UnityEngine.SceneManagement;

namespace MineGarden
{
    /// <summary>
    /// Owns the round: menu, play, win, loss, the clock, and difficulty.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Menu;
        public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Easy;
        public float ElapsedTime { get; private set; }

        public const string MainMenuScene = "MainMenuScene";
        public const string GameScene = "GameScene";

        GridManager grid;
        UIManager ui;
        public const int MaxLives = 3;

        bool timerRunning;
        bool clockStarted;
        bool paused;
        int lives = MaxLives;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Mine Garden already has a GameManager.");
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (!timerRunning)
                return;

            ElapsedTime += Time.deltaTime;
            if (ui != null)
                ui.SetTimer(ElapsedTime);
        }

        public void Attach(GridManager gridManager, UIManager uiManager)
        {
            grid = gridManager;
            ui = uiManager;
        }

        public bool CanAcceptInput()
        {
            return State == GameState.Playing && !paused;
        }

        public void ShowMainMenu()
        {
            State = GameState.Menu;
            timerRunning = false;
            ElapsedTime = 0f;
            if (grid != null)
                grid.ClearBoard();
            if (ui != null)
                ui.ShowMenu();
        }

        public void StartGame(Difficulty difficulty)
        {
            if (grid == null || ui == null)
            {
                Debug.LogError("Mine Garden managers are not wired.");
                return;
            }

            CurrentDifficulty = difficulty;
            State = GameState.Playing;
            ElapsedTime = 0f;
            timerRunning = false;
            clockStarted = false;
            paused = false;
            lives = MaxLives;
            if (ui != null)
                ui.SetLives(lives);

            DifficultyPresets.Get(difficulty, out int width, out int height, out int weeds);
            // Show the bed first so the grid has a real size before plots are created.
            ui.ShowGameplay(difficulty);
            ui.SetTimer(0f);
            grid.NewBoard(width, height, weeds);
            ui.SetWeedCount(grid.DisplayedWeedCount);
        }

        public void Restart()
        {
            if (State == GameState.Menu)
                return;

            StartGame(CurrentDifficulty);
        }

        public void BeginTimer()
        {
            if (State != GameState.Playing || paused)
                return;

            clockStarted = true;
            timerRunning = true;
        }

        public void PauseGame()
        {
            if (State != GameState.Playing || paused)
                return;

            paused = true;
            timerRunning = false;
            if (ui != null)
                ui.ShowPause(true);
        }

        public void Resume()
        {
            if (State != GameState.Playing)
                return;

            paused = false;
            if (ui != null)
                ui.ShowPause(false);
            if (clockStarted)
                timerRunning = true;
        }

        public void NextGarden()
        {
            Difficulty next = CurrentDifficulty == Difficulty.Easy
                ? Difficulty.Medium
                : CurrentDifficulty == Difficulty.Medium ? Difficulty.Hard : Difficulty.Easy;
            StartGame(next);
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(MainMenuScene);
        }

        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void RefreshHud()
        {
            if (ui != null && grid != null)
                ui.SetWeedCount(grid.DisplayedWeedCount);
        }

        public void NotifyLifeLost()
        {
            if (State != GameState.Playing)
                return;

            lives = Mathf.Max(0, lives - 1);
            if (ui != null)
                ui.SetLives(lives);

            // Lives remain, so the dug weed stays visible and the bed can still be cleared.
            if (lives > 0)
                return;

            NotifyDefeat();
        }

        public void NotifyVictory()
        {
            if (State != GameState.Playing)
                return;

            State = GameState.Won;
            timerRunning = false;
            if (ui != null)
                ui.ShowVictory(ElapsedTime);
        }

        public void NotifyDefeat()
        {
            if (State != GameState.Playing)
                return;

            State = GameState.Lost;
            timerRunning = false;
            if (grid != null)
                grid.RevealAllWeeds();
            if (ui != null)
                ui.ShowDefeat(ElapsedTime);
        }
    }
}
