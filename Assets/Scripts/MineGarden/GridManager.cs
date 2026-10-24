using UnityEngine;
using UnityEngine.UI;

namespace MineGarden
{
    /// <summary>
    /// Builds the visible bed, then asks BoardLogic to place weeds, open plots and toggle markers.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [SerializeField] TileView tilePrefab = null;
        [SerializeField] RectTransform gridContainer;
        [SerializeField] GridLayoutGroup gridLayout;

        TileData[,] board;
        TileView[,] views;
        GardenArt art;
        System.Random random = new System.Random();
        int targetWeeds;
        int placedWeeds;
        bool weedsPlaced;

        public int DisplayedWeedCount
        {
            get
            {
                int weeds = weedsPlaced ? placedWeeds : targetWeeds;
                return weeds - BoardLogic.CountFlags(board);
            }
        }

        public void UseArt(GardenArt gardenArt)
        {
            art = gardenArt;
        }

        public void Attach(RectTransform container, GridLayoutGroup layout)
        {
            if (container != null)
                gridContainer = container;
            if (layout != null)
                gridLayout = layout;
        }

        public void ClearBoard()
        {
            board = null;
            views = null;
            weedsPlaced = false;
            placedWeeds = 0;
            targetWeeds = 0;
            DestroyChildren();
        }

        public void NewBoard(int width, int height, int weedCount)
        {
            ClearBoard();
            if (gridContainer == null || gridLayout == null)
            {
                Debug.LogError("Mine Garden grid container is not assigned.");
                return;
            }

            if (width < 1 || height < 1)
                return;

            targetWeeds = Mathf.Max(0, weedCount);
            random = new System.Random();
            board = BoardLogic.CreateHiddenBoard(width, height);
            views = new TileView[width, height];
            ApplyCellSize(width, height);

            // y = 0 is the top row, matching the grid's upper-left start corner.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileView view = CreateTile();
                    view.Bind(board[x, y], this);
                    views[x, y] = view;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(gridContainer);
        }

        public void RequestReveal(int x, int y)
        {
            if (!CanUseBoard(x, y))
                return;

            TileData tile = board[x, y];
            if (tile.isRevealed || tile.isFlagged)
                return;

            // Markers placed before the first dig must not count as that dig.
            if (!weedsPlaced)
            {
                placedWeeds = BoardLogic.PlaceWeeds(board, x, y, targetWeeds, random);
                BoardLogic.CalculateAdjacentWeeds(board);
                weedsPlaced = true;
                if (GameManager.Instance != null)
                    GameManager.Instance.BeginTimer();
            }

            RevealOutcome outcome = BoardLogic.Reveal(board, x, y);
            RefreshAll();
            if (GameManager.Instance != null)
                GameManager.Instance.RefreshHud();

            if (GameManager.Instance == null)
                return;

            if (outcome.hitWeed)
                GameManager.Instance.NotifyLifeLost();
            else if (outcome.won)
                GameManager.Instance.NotifyVictory();
        }

        public void RequestFlag(int x, int y)
        {
            // A marker is not a dig: weeds stay unplaced and the clock stays stopped.
            if (!CanUseBoard(x, y))
                return;

            if (!BoardLogic.ToggleFlag(board, x, y))
                return;

            if (views != null && views[x, y] != null)
                views[x, y].Refresh();

            if (GameManager.Instance != null)
                GameManager.Instance.RefreshHud();
        }

        public void RevealAllWeeds()
        {
            if (board == null)
                return;

            int width = board.GetLength(0);
            int height = board.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y].hasWeed)
                        board[x, y].isRevealed = true;
                }
            }

            RefreshAll();
        }

        bool CanUseBoard(int x, int y)
        {
            if (board == null)
                return false;
            if (GameManager.Instance != null && !GameManager.Instance.CanAcceptInput())
                return false;
            return BoardLogic.InBounds(board, x, y);
        }

        void ApplyCellSize(int columns, int rows)
        {
            FitBedToScreen();

            float width = gridContainer.sizeDelta.x;
            float height = gridContainer.sizeDelta.y;

            RectOffset padding = gridLayout.padding;
            Vector2 spacing = gridLayout.spacing;
            float availableWidth = width - padding.left - padding.right - spacing.x * (columns - 1);
            float availableHeight = height - padding.top - padding.bottom - spacing.y * (rows - 1);
            float cell = Mathf.Floor(Mathf.Min(availableWidth / columns, availableHeight / rows));
            cell = Mathf.Max(8f, cell);

            gridLayout.cellSize = new Vector2(cell, cell);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columns;
        }

        void FitBedToScreen()
        {
            RectTransform canvas = gridContainer.parent as RectTransform;
            Canvas.ForceUpdateCanvases();

            float canvasWidth = canvas != null && canvas.rect.width > 2f ? canvas.rect.width : Screen.width;
            float canvasHeight = canvas != null && canvas.rect.height > 2f ? canvas.rect.height : Screen.height;

            float topInset = 108f;
            const float bottomInset = 28f;
            const float sideInset = 28f;
            if (canvas != null)
            {
                float hudInset = HudBottomInset(canvas, canvasHeight);
                if (hudInset > topInset)
                    topInset = hudInset;
            }

            float size = Mathf.Min(
                Mathf.Max(8f, canvasWidth - sideInset * 2f),
                Mathf.Max(8f, canvasHeight - topInset - bottomInset));

            gridContainer.anchorMin = new Vector2(0.5f, 0.5f);
            gridContainer.anchorMax = new Vector2(0.5f, 0.5f);
            gridContainer.pivot = new Vector2(0.5f, 0.5f);
            gridContainer.sizeDelta = new Vector2(size, size);
            gridContainer.anchoredPosition = new Vector2(0f, (bottomInset - topInset) * 0.5f);
        }

        static float HudBottomInset(RectTransform canvas, float canvasHeight)
        {
            Transform hud = null;
            for (int i = 0; i < canvas.childCount; i++)
            {
                Transform child = canvas.GetChild(i);
                if (child.name == "HUD")
                {
                    hud = child;
                    break;
                }
            }

            if (hud == null)
                return 0f;

            float top = canvasHeight * 0.5f;
            float lowest = top;
            bool found = false;
            for (int i = 0; i < hud.childCount; i++)
            {
                RectTransform item = hud.GetChild(i) as RectTransform;
                if (item == null || !item.gameObject.activeInHierarchy)
                    continue;

                Vector3[] corners = new Vector3[4];
                item.GetWorldCorners(corners);
                for (int c = 0; c < 4; c++)
                {
                    float y = canvas.InverseTransformPoint(corners[c]).y;
                    if (y < 0f || y >= lowest)
                        continue;

                    lowest = y;
                    found = true;
                }
            }

            if (!found)
                return 0f;

            return top - lowest + 16f;
        }

        TileView CreateTile()
        {
            TileView view;
            if (tilePrefab != null)
                view = Instantiate(tilePrefab, gridContainer);
            else
                view = TileView.CreateRuntime(gridContainer);

            view.UseSprites(art);
            view.gameObject.name = "Tile";
            return view;
        }

        void RefreshAll()
        {
            if (views == null)
                return;

            int width = views.GetLength(0);
            int height = views.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (views[x, y] != null)
                        views[x, y].Refresh();
                }
            }
        }

        void DestroyChildren()
        {
            if (gridContainer == null)
                return;

            // Hide first so the layout group ignores them while the new bed is built this frame.
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                GameObject child = gridContainer.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }
    }
}
