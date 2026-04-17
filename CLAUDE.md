# Scuba Man Takes Off – Project Context for Claude

## What is this?
PC Flappy-Bird clone in Unity (URP, Unity 6). Single-player.
GitHub: https://github.com/TiefSeeTaucher69/FlappySteff

## Language Rule
**All visible in-game text must be in English** — UI labels, button text, feedback messages, shop card text, tooltips, error messages shown to the player, etc.
Code comments may be in German or English.

## Scene Order
BootScene → FirstOpen (first time only) → MainMenu → GameScene

**Note:** ItemShop, SettingsScene and EscapeScene **no longer exist as separate scenes** — they are integrated as panels in MainMenu (Pnl_Shop, Pnl_Settings, Quit-Panel).

## Skins
| Name | Status | PlayerPrefs-Key |
|------|--------|----------------|
| `benjo-bird` | **Default (free, always owned)** | — |
| `ginger-bird` | Buyable (25 Cannabis) | `HasSkin_ginger-bird` |
| `tom-bird` | Buyable (25 Cannabis) | `HasSkin_tom-bird` |
| `bennet-bird` | Buyable (25 Cannabis) | `HasSkin_bennet-bird` |
| `jan-bird` | Buyable (25 Cannabis) | `HasSkin_jan-bird` |
| `paulaner-bird` | Buyable (25 Cannabis) | `HasSkin_paulaner-bird` |

Sprites in `Assets/Resources/Skins/`. Loaded via `Resources.Load<Sprite>("Skins/<name>")`.
Active skin: PlayerPrefs `ActiveSkin` (Default: `"benjo-bird"`)

## Items / Power-ups

Items are **consumable stacks** — buying the same item multiple times adds run-uses. Each run consumes 1 stack at `Start()`. Equipping is done in the Loadout tab, not the Shop.

| Name | Stack Key | Legacy Key | Cost |
|------|-----------|-----------|------|
| Invincible | `ItemCount_Invincible` | `HasInvincibleItem` | 50 Cannabis |
| Shrink | `ItemCount_Shrink` | `HasShrinkItem` | 50 Cannabis |
| Laser | `ItemCount_Laser` | `HasLaserItem` | 50 Cannabis |
| SlowMo | `ItemCount_SlowMo` | `HasSlowMoItem` | 50 Cannabis |
| Shield | `ItemCount_Shield` | `HasShieldItem` | 50 Cannabis |

Active item: PlayerPrefs `ActiveItem` ("Invincible" / "Shrink" / "Laser" / "SlowMo" / "Shield" / "")

**Stack consume timing:** At GameScene `Start()` in each item manager. Ranked = no consume.

**Migration:** If `HasXItem == 1` and `ItemCount_X == 0` → give 1 free stack (runs once in `Start()`).

**Shop behavior:** Items show a buy button that increments the stack count. The activate button is hidden for items — activation happens in the Loadout tab.

**Ranked rule:** In Ranked mode (`RankedManager.IsRanked == true`) `ActiveItem` from PlayerPrefs is ignored. Only `RankedManager.WeeklyItem` is active — even without owning any stacks. All managers (InvincibilityManager, ShrinkManager, LaserManager, SlowMoManager, ShieldManager) check: `bool isRankedItem = RankedManager.IsRanked && RankedManager.WeeklyItem == "X"` and never consume stacks in Ranked.

## Trails
| Name | PlayerPrefs-Key | Cost |
|------|----------------|------|
| Red | `HasTrailRed` | 20 Cannabis |
| Purple | `HasTrailPurple` | 20 Cannabis |
| Blue | `HasTrailBlue` | 20 Cannabis |

Active trail: PlayerPrefs `ActiveTrail` ("TrailRed" / "TrailPurple" / "TrailBlue" / "")

## Biomes
| Name | PlayerPrefs-Key | Cost |
|------|----------------|------|
| Mountain | — (default, always owned) | — |
| Others | `HasBiome_[name]` | TBD |

Active biome: PlayerPrefs `ActiveBiome` (Default: `"Mountain"`)
- `BiomeManager.cs` in GameScene applies the biome on `Awake()`: sets background texture (scrolling parallax, two tiles) + pipe material (`BiomeManager.ActivePipeMaterial`)
- Background scrolls at `parallaxFactor` relative to pipe speed; direction-aware (respects `DirectionFlipManager.IsFlipped`)
- Fallback: solid color if no texture assigned

## Pets
| Name | PlayerPrefs-Key | Cost |
|------|----------------|------|
| e.g. BlackCat | `HasPetBlackCat` | 100 Cannabis |

Active pet: PlayerPrefs `ActivePet` (e.g. `"BlackCat"` / `""`)
- Pets follow the bird, collect cannabis leaves (+1 CannabisStash)
- **No pet in Ranked** (`RankedManager.IsRanked` → PetManager spawns nothing)
- Pet prefabs from FantasyMonsters Asset Pack (Skeletal Animation, `Monster` component)
- `PetManager.cs` in GameScene spawns the active pet
- `PetCompanionScript.cs` added via `AddComponent` at runtime, call `Init(steff, logic, detectionRadius, seekSpeed)`
- Cannabis leaf structure: Root has `CannabisMovementScript`, Child (`cannabis_0`) has `CannabisCollisionScript` + Collider + Sprite → Pet scans via `FindObjectsByType<CannabisCollisionScript>` (not Root!)

## Gameplay Events (Quickplay only, disabled in Ranked)

### Direction Flip (`DirectionFlipManager.cs`)
- Reverses pipe movement direction for `flipDuration` (10s)
- First trigger: random 25–45s; cooldown between events: 5–30s
- Mutual exclusion: won't trigger while `GravityInversionManager.IsActive`
- Static API: `DirectionFlipManager.IsFlipped`, `DirectionFlipManager.IsActive`
- On flip moment: destroys pipes that have already passed the bird (old direction), force-spawns a new pipe, triggers camera shake
- Indicator panel: animated arrow cascade + colored wipe tint (purple)
- `BiomeManager` and `PipeMoveScript`/`CannabisMovementScript` all read `IsFlipped` to reverse movement

### Gravity Inversion (`GravityInversionManager.cs`)
- Inverts bird gravity for `invertDuration` (7s); `gravityScale = -originalGravity * 0.75f`
- First trigger: random 30–50s; cooldown: 5–30s
- Mutual exclusion: won't trigger while `DirectionFlipManager.IsActive`
- Static API: `GravityInversionManager.IsInverted`, `GravityInversionManager.IsActive`
- Indicator panel: animated arrow cascade + colored wipe tint (orange)

## Important PlayerPrefs Keys
| Key | Type | Meaning |
|-----|------|---------|
| `Username` | string | Player name (also set as Unity Auth Display Name) |
| `Highscore` | int | Best score (Quickplay) |
| `RankedHighscore` | int | Best score (Ranked) |
| `CannabisStash` | int | In-game currency |
| `TotalScore` | int | Cumulative total score |
| `TotalRuns` | int | Total number of runs played |
| `lastRewardDate` | string | Date of last daily reward (yyyy-MM-dd) |
| `WeeklyMissions` | string (JSON) | Active weekly missions |
| `WeeklyMissionStartTime` | string | Start date of current mission week (binary long) |
| `VSyncEnabled` | int | 0=off, 1=on |
| `FPSCap` | int | 0=30, 1=60, 2=120, 3=240, 4=unlimited |
| `ResolutionIndex` | int | Index in Screen.resolutions |
| `ActivePet` | string | Active pet (e.g. `"BlackCat"`) |
| `HasPet[Name]` | int | 0/1 whether pet is purchased |
| `ActiveBiome` | string | Active biome (default `"Mountain"`) |
| `PlayerAccountsLinked` | int | 0/1 — 1 = player has logged in via Unity Player Accounts |
| `ItemCount_Invincible` | int | Remaining run-uses of Invincible |
| `ItemCount_Shrink` | int | Remaining run-uses of Shrink |
| `ItemCount_Laser` | int | Remaining run-uses of Laser |
| `ItemCount_SlowMo` | int | Remaining run-uses of SlowMo |
| `ItemCount_Shield` | int | Remaining run-uses of Shield |

## Backend: Unity Gaming Services

| Service | Purpose |
|---------|---------|
| Unity Player Accounts | Browser-based OAuth login (Google, email/password etc.) |
| Unity Authentication | `SignInWithUnityAsync(accessToken)` after Player Accounts login |
| Unity Leaderboards | Score submission and display |
| Unity Cloud Save | Persistent player data synced to cloud |

**Leaderboard IDs:** `FlappySteffLeaderboard` (Quickplay), `FlappySteffRankedLeaderboard` (Ranked)

### Authentication Flow
1. BootScene (`UpdateCheckerScript.cs`): if `PlayerAccountsLinked == 1` and session token exists → auto-login via `SignInAnonymouslyAsync()` (restores session), then `CloudSaveManager.LoadAllAsync()`
2. FirstOpen (`AuthScript.cs`): Login button → `PlayerAccountService.Instance.StartSignInAsync()` opens browser; **subscribe to `SignedIn` event BEFORE calling `StartSignInAsync()`** (event fires while task awaits, i.e. during token exchange after auth-code redirect); after `SignedIn` fires → `SignInWithUnityAsync(AccessToken)`
3. Each scene has a fallback init check when started directly in the Editor

### Cloud Save (`CloudSaveManager.cs`)
DontDestroyOnLoad singleton. Write pattern: write to PlayerPrefs immediately, then async push to cloud (fire-and-forget).
On login: `LoadAllAsync()` loads all cloud keys into PlayerPrefs — **cloud wins over local**.

**Keys synced to cloud:** `CannabisStash`, `Highscore`, `RankedHighscore`, `TotalScore`, `TotalRuns`, all item/trail/skin/pet ownership keys, all `ItemCount_*` stack keys, `Username`
**NOT synced:** `VSyncEnabled`, `FPSCap`, `ResolutionIndex`, active selections (`ActiveSkin`, `ActiveTrail`, `ActiveItem`, `ActivePet`, `ActiveBiome`)

Use `CloudSaveManager.Instance.SaveInt(key, value)` / `.SaveString(key, value)` / `.SaveBatch(dict)` instead of raw `PlayerPrefs.Set*` for synced keys.

No real-time multiplayer. Netcode for GameObjects is installed but unused.

## Ranked Mode
- `RankedManager` is DontDestroyOnLoad Singleton (`Assets/Scripts/Game/RankedManager.cs`)
- Weekly deterministic item via ISO calendar week seed (`RankedManager.WeeklyItem`)
- `RankedManager.IsRanked` (static bool) set in MainMenu
- No pets, no direction flip, no gravity inversion in Ranked
- Separate leaderboard
- In MainMenu: Play-Bar with Quickplay/Ranked buttons; Ranked shows `PnlRankedInfo` with weekly item (icon + name) and reset time

## Weekly Missions
Deterministic seed based on ISO calendar week + year (e.g. `202614`).
All players get the same 3 missions per week. Fully offline/local.

## Music
`MusicPlayerScript.cs` in GameScene: Pitch scales with pipe speed from 1.0 (start speed) to 1.5 (max speed) via `Mathf.InverseLerp(SpeedManager.startSpeed, SpeedManager.maxSpeed, currentSpeed)`.

## Discord Rich Presence
`Assets/Scripts/Discord/DiscordRichPresenceManager.cs` — DontDestroyOnLoad Singleton, placed in MainMenu scene.
- Library: Lachee/DiscordRPC (`Assets/Plugins/DiscordRPC/DiscordRPC.dll`)
- Client ID in Inspector (`clientId` field)
- Shows per scene: "In Main Menu" / "Score: X | Quickplay" / "Score: X | Ranked" / "In Shop"
- Score update every 5 seconds via polling on `LogicScript.playerScore`

## MainMenu UI Architecture

### Tab Navigation (TabController.cs)
`Canvas` has 7 main panels: `Pnl_Play`, `Pnl_Scoreboard`, `Pnl_Missions`, `Pnl_Shop`, `Pnl_Loadout`, `Pnl_Profile`, `Pnl_Settings`.
`TabController.cs` switches between them. Each panel has a `CanvasGroup` for fade-in (alpha 0→1, 0.2s).
TabBar has an `Img_Slider` (green, `LayoutElement.ignoreLayout=true`) that is animated via Coroutine.

### Profile (in Pnl_Settings or own panel — `ProfileScript.cs`)
Shows: username, avatar (first letter of username), Quickplay/Ranked highscore, total score, total runs, avg score.
- Rename: `UpdatePlayerNameAsync` + `CloudSaveManager.SaveString("Username", name)`
- Logout: `AuthenticationService.SignOut()` + `PlayerAccountService.SignOut()`, clears `Username` + `PlayerAccountsLinked`, loads FirstOpen

### Shop (Pnl_Shop)
Shop is a tab in MainMenu, no longer a separate scene load.
`ShopBar` has `HorizontalLayoutGroup` + an `Img_Slider` Child with `LayoutElement.ignoreLayout=true`.
`ShopPageSwitcher.cs` switches between 5 categories: Items, Trails, Skins, Pets, Biomes.
Each Shop site panel (`ItemShop Site` etc.) has a `CanvasGroup` for fade-in (0.15s).
Slider position determined via `indicator.GetComponentInParent<Button>().localPosition.x`.

### Shop Cards (ShopCardScript.cs)
Each generated shop card plays a pop-in animation on appearance (Scale 0.8→1.0, 0.15s).
Buy/Activate use `CloudSaveManager.Instance.SaveBatch/SaveString` — not raw PlayerPrefs.
**Stackable items** (all 5 power-ups): buy button always visible, increments `ItemCount_X`, cost text shows current stack count. Activate button hidden — equipping happens in Loadout.

### Loadout (Pnl_Loadout — `LoadoutScript.cs`)
Dedicated tab for equipping Skin, Item, Trail, Pet, Biome before each run.

**Layout:** Character preview (static skin sprite, left), 5 equipment slots in a grid (right), horizontal inventory scroll at the bottom (shows owned items for the selected slot's category).

**Interaction:** Click a slot to select its category → inventory filters. Click an inventory card or drag it onto a slot to equip. `CloudSaveManager.SaveString(activePrefsKey, activeValue)` on equip.

**Key scripts:**
- `LoadoutScript.cs` — main controller (`Assets/Scripts/MainMenu/`)
- `LoadoutSlotUI.cs` — per-slot component; implements `IDropHandler`, `IPointerClickHandler`
- `LoadoutInventoryItemUI.cs` — per-card component; implements drag (`IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`) + `IPointerClickHandler`
- `LoadoutItemData.cs` — `[System.Serializable]` data class (no MonoBehaviour); fields: `displayName`, `icon`, `category`, `activeValue`, `activePrefsKey`, `ownedKey`, `alwaysOwned`, `isStackable`, `countKey`

**Inventory card prefab:** `Assets/Prefabs/InventoryCard.prefab` — 130×130, has `LoadoutInventoryItemUI` + `CanvasGroup` + children: `Img_Icon`, `Txt_Name`, `Txt_StackCount`. Assign to `LoadoutScript.inventoryItemPrefab`.

**Empty slot icon:** `LoadoutScript.emptySlotIcon` — a single fallback sprite shown in slots and inventory cards when no icon is assigned. Set once in Inspector.

**`allItems` list (configure in Inspector):**
- Items: each with `category="Item"`, `activePrefsKey="ActiveItem"`, `isStackable=true`, `countKey="ItemCount_X"`, `activeValue="X"`; plus a "No Item" entry with `alwaysOwned=true`, `activeValue=""`
- Skins: `category="Skin"`, `activePrefsKey="ActiveSkin"`, `ownedKey="HasSkin_X"`, `activeValue="X"`; benjo-bird has `alwaysOwned=true`
- Trails: `category="Trail"`, `activePrefsKey="ActiveTrail"`, `ownedKey="HasTrailX"`; "No Trail" entry `alwaysOwned=true`, `activeValue=""`
- Pets: `category="Pet"`, `activePrefsKey="ActivePet"`, `ownedKey="HasPet[Name]"`; "No Pet" entry `alwaysOwned=true`, `activeValue=""`
- Biomes: `category="Biome"`, `activePrefsKey="ActiveBiome"`; Mountain `alwaysOwned=true`

### AnimateSlider Pattern (TabController, ShopPageSwitcher, MenuHandlerScript)
All slider animations: `Mathf.Lerp` on `localPosition.x` + `sizeDelta.x` in Coroutine (0.2s).
`SnapSlider()` sets initial position without animation (called in `Start()` before first `SwitchTab()`).

### Important Unity Note
`GameObject.Find()` does **not** find inactive GameObjects → use `canvas.transform.Find("Pnl_Shop/...")` instead.

## Architecture Decisions
- `WeeklyMissionManager` is DontDestroyOnLoad Singleton
- `RankedManager` is DontDestroyOnLoad Singleton
- `CloudSaveManager` is DontDestroyOnLoad Singleton (lazy-created if missing)
- `CursorManager` is DontDestroyOnLoad Singleton (hides cursor during gameplay, shows on pause/death/menu)
- `DiscordRichPresenceManager` is DontDestroyOnLoad Singleton
- Purchases/progress: PlayerPrefs locally + Unity Cloud Save remotely (via CloudSaveManager)
- Auto-update via GitHub Releases API (downloads .exe installer)
- Player input: keyboard + mouse + Xbox controller (JoystickButton*)
- `SpeedManagerScript.cs`: `startSpeed = 5f`, `maxSpeed` = end value; `SpeedManagerCannabisScript` for cannabis leaf speed
