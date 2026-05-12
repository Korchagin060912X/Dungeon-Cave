# Level 1 setup (Unity 6000.3.3f1)

## 1) Player
- Create `Player` object with:
  - `Rigidbody2D` (Dynamic, Freeze Rotation Z).
  - `CapsuleCollider2D`.
  - `PlayerController2D`.
  - Child object `GroundCheck` under feet.
- Tag object as `Player`.
- In `PlayerController2D`:
  - Assign `GroundCheck`.
  - Set `Ground Mask` to your platforms layer.
  - Set `Ceiling Mask` to obstacles/roof layer.

## 2) Death + Respawn
- Create empty object `LevelSystems` and add `LevelRespawnManager`.
- Add kill zones (pit, enemies, traps) with `BoxCollider2D` (`Is Trigger` ON) + `KillTrigger`.
- Assign `LevelRespawnManager` in every `KillTrigger` and `CrouchHazard`.

## 3) Section 1 (start platforming + code hints)
- Build simple platforms/walls from `Sprite Shape` or `Tilemap` or cubes.
- Place 4 collectibles with `DigitCollectible` and set digits: `8`, `3`, `0`, `3`.
- Create `TMP_Text` in UI (example: `CodeHintText`) and assign to collectibles.

## 4) Section 2 (code puzzle 8303)
- Create a gate object (wall/door) and assign it to `CodePuzzleController.doorToOpen`.
- Add `TMP_InputField`, `TMP_Text` status and button `Submit`.
- On button `OnClick` call `CodePuzzleController.TrySubmitCode()`.

## 5) Section 3 (minecart + crouch traps)
- Create minecart object with `MinecartRideController` + trigger collider.
- Add route points as transforms and assign in order to `routePoints`.
- Place stalagmite triggers (`BoxCollider2D` trigger) with `CrouchHazard`.

## 6) Section 4 (vertical maze + timer + mini map marker)
- Create maze walls/platforms.
- Add object `MazeSystem` with `VerticalMazeController`.
- Assign timer `TMP_Text`.
- Optional: create avalanche object and assign in controller.
- For mini map:
  - Create panel/map image.
  - Create marker (`RectTransform`) and add `MinimapMarker2D`.
  - Assign `player`, `marker`, and world/map bounds.

## 7) Section 5 (key and exit door)
- Add `KeyDoorController` on system object.
- Add collectible key objects with `KeyCollectible`.
- Add door trigger with `DoorExitTrigger`.
- Set `requiredKeys` and `nextSceneName` for transition to level 2 later.

## 8) UI and restart button
- In Canvas add button `Заново`.
- On click call `LevelRespawnManager.RestartLevel()`.
- Add any HUD texts: stamina, code hints, timer, keys.

## 9) Main menu + music
- Create `MenuScene`.
- Add `MainMenuController` and button Play -> `MainMenuController.Play()`.
- Add object with `AudioSource` + `BackgroundMusic`.
  - `Play On Awake` ON
  - `Loop` ON
- Add scenes in build settings:
  - `MenuScene` at index `0`
  - `SampleScene` at index `1`
