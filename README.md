# Mine Garden

Mine Garden is a cozy, garden-themed Minesweeper puzzle. Each game grows a new bed. Dig every safe plot without uncovering a weed.

The Unity project is **Mindsweeper** (Unity 6000.3.9f1). The game uses the existing `MainMenuScene` and `GameScene`.

## Controls

- Left click a hidden plot to dig.
- Right click a hidden plot to plant or remove a flower marker.
- Markers are optional. You win by digging every plot that is not a weed.
- The clock starts on the first dig.

## Difficulty

| Garden | Size | Weeds |
| --- | --- | --- |
| Easy | 6×6 | 5 |
| Medium | 8×8 | 10 |
| Hard | 10×10 | 18 |

The first plot you dig is never a weed. Its eight neighbours are kept clear when the bed has room. Retry grows a new bed at the same size. Next Level moves to the next size.

## Scripts

| Script | Responsibility |
| --- | --- |
| `GameManager` | Play, win, loss, pause, clock, and difficulty. |
| `TileData` | One plot. Not a `MonoBehaviour`. |
| `BoardLogic` | Weed placement, neighbour counts, flags, and queue flood-fill. No Unity types. |
| `GridManager` | Builds the grid, fits the cells, and applies the rules. |
| `TileView` | Draws one plot with the tile sprites and forwards clicks. |
| `UIManager` | Timer, weed counter, and the win, loss, and pause panels. |
| `SceneFlow` | Wires the buttons already in `MainMenuScene` and `GameScene`. |
| `GardenArt` | The plot sprites from `Assets/Tiles`. |
| `DifficultyPresets` | The three garden sizes. |

Plots use the sprites in `Assets/Tiles`: grass when hidden, soil when dug, the bush behind a number, the daisy as a flower marker, and the purple weed when one is uncovered.

## Play

1. Open this project in Unity 6000.3.9f1.
2. Open `Assets/Scenes/MainMenuScene.unity`.
3. Press Play.

Play opens Easy, Medium, and Hard on the buttons already in that scene. The game then loads `GameScene`. Pause, Controls, Win, and Lose use the panels already in that scene.

- Pause stops the clock. Resume continues.
- Retry grows a new bed at the same size.
- Next Level moves to the next size.
- Main Menu returns to `MainMenuScene`.
- Quit leaves play mode in the editor, and closes a build.

`MainMenuScene` is first in the build list, then `GameScene`. Those scene files were not rewritten.

## Build

1. File > Build Profiles (or Build Settings).
2. Confirm `MainMenuScene` is enabled and first, and `GameScene` is enabled.
3. Choose Windows and build.

## Board-rule checks

From the project root, with the .NET SDK installed:

```
dotnet run --project Tools/BoardLogicTests/BoardLogicTests.csproj
```

Those checks cover weed placement, neighbour counts, flood-fill, flags, wins, and losses. They do not start Unity.

## Not done in the editor

- The buttons were not clicked inside the Unity Editor after this wiring.
- The three HP points already on the game HUD were left in place.
- To rename the window title, set Product Name to Mine Garden in Project Settings > Player.
