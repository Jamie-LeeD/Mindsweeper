using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace MineGarden
{
    /// <summary>
    /// Builds the playable UI at runtime and connects the three managers.
    /// Open Assets/Scenes/MainGame and press Play.
    /// </summary>
    [DisallowMultipleComponent]
    public class MineGardenBootstrap : MonoBehaviour
    {
        void Awake()
        {
            EnsureEventSystem();

            var game = GetComponent<GameManager>();
            var grid = GetComponent<GridManager>();
            var ui = GetComponent<UIManager>();
            if (game == null || grid == null || ui == null)
            {
                Debug.LogError("Mine Garden is missing GameManager, GridManager, or UIManager.");
                return;
            }

            ui.BuildDefaultInterface();
            grid.Attach(ui.GridContainer, ui.GridLayout);
            game.Attach(grid, ui);
            ui.Bind(game);
            game.ShowMainMenu();

            if (Camera.main != null)
                Camera.main.backgroundColor = GardenPalette.Background;
        }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            // The project uses the Input System only. This module assigns default click actions on enable,
            // including right click, so flower markers work without the legacy input module.
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }
    }
}
