# Flappy Steff – Projektkontext für Claude

## Was ist das?
PC Flappy-Bird-Klon in Unity (URP, Unity 6). Einzelspieler. Deutsche UI und Kommentare.
GitHub: https://github.com/TiefSeeTaucher69/FlappySteff

## Szenen-Reihenfolge
BootScene → FirstOpen (nur erstes Mal) → MainMenu → GameScene

**Hinweis:** ItemShop, SettingsScene und EscapeScene existieren **nicht mehr als eigene Szenen** — sie sind als Panels in MainMenu integriert (Pnl_Shop, Pnl_Settings, Quit-Panel).

## Skins
| Name | Status | PlayerPrefs-Key |
|------|--------|----------------|
| `benjo-bird` | **Default (kostenlos, immer owned)** | — |
| `ginger-bird` | Kaufbar (25 Cannabis) | `HasSkin_ginger-bird` |
| `tom-bird` | Kaufbar (25 Cannabis) | `HasSkin_tom-bird` |
| `bennet-bird` | Kaufbar (25 Cannabis) | `HasSkin_bennet-bird` |
| `jan-bird` | Kaufbar (25 Cannabis) | `HasSkin_jan-bird` |

Sprites liegen in `Assets/Resources/Skins/`. Werden per `Resources.Load<Sprite>("Skins/<name>")` geladen.
Aktiver Skin: PlayerPrefs `ActiveSkin` (Default: `"benjo-bird"`)

## Items / Power-ups
| Name | PlayerPrefs-Key | Kosten |
|------|----------------|--------|
| Invincible | `HasInvincibleItem` | 50 Cannabis |
| Shrink | `HasShrinkItem` | 50 Cannabis |
| Laser | `HasLaserItem` | 50 Cannabis |

Aktives Item: PlayerPrefs `ActiveItem` ("Invincible" / "Shrink" / "Laser" / "")

**Ranked-Regel:** Im Ranked-Modus (`RankedManager.IsRanked == true`) wird `ActiveItem` aus PlayerPrefs ignoriert. Nur das `RankedManager.WeeklyItem` ist aktiv — auch ohne Kauf. Alle drei Manager (InvincibilityManager, ShrinkManager, LaserManager) prüfen: `bool isActiveItem = !RankedManager.IsRanked && PlayerPrefs.GetString("ActiveItem") == "..."`.

## Trails
| Name | PlayerPrefs-Key | Kosten |
|------|----------------|--------|
| Red | `HasTrailRed` | 20 Cannabis |
| Purple | `HasTrailPurple` | 20 Cannabis |
| Blue | `HasTrailBlue` | 20 Cannabis |

Aktiver Trail: PlayerPrefs `ActiveTrail` ("TrailRed" / "TrailPurple" / "TrailBlue" / "")

## Pets
| Name | PlayerPrefs-Key | Kosten |
|------|----------------|--------|
| z.B. BlackCat | `HasPetBlackCat` | 100 Cannabis |

Aktives Pet: PlayerPrefs `ActivePet` (z.B. `"BlackCat"` / `""`)
- Pets folgen dem Bird, sammeln Cannabis-Blätter ein (geben +1 CannabisStash)
- In **Ranked kein Pet** (`RankedManager.IsRanked` → PetManager spawnt nichts)
- Pet-Prefabs aus FantasyMonsters Asset-Pack (Skeletal Animation, `Monster`-Komponente)
- `PetManager.cs` in GameScene spawnt das aktive Pet
- `PetCompanionScript.cs` per `AddComponent` zur Laufzeit hinzugefügt, `Init(steff, logic, detectionRadius, seekSpeed)` aufrufen
- Cannabis-Blatt-Struktur: Root hat `CannabisMovementScript`, Child (`cannabis_0`) hat `CannabisCollisionScript` + Collider + Sprite → Pet scannt per `FindObjectsByType<CannabisCollisionScript>` (nicht Root!)

## Wichtige PlayerPrefs-Keys
| Key | Typ | Bedeutung |
|-----|-----|-----------|
| `Username` | string | Spielername (auch als Unity Auth Display Name gesetzt) |
| `Highscore` | int | Bester Score (Quickplay) |
| `RankedHighscore` | int | Bester Score (Ranked) |
| `CannabisStash` | int | In-Game Währung |
| `TotalScore` | int | Kumulierter Gesamtscore |
| `lastRewardDate` | string | Datum der letzten Daily Reward (yyyy-MM-dd) |
| `WeeklyMissions` | string (JSON) | Aktive Wochenmissionen |
| `WeeklyMissionStartTime` | string | Startdatum der aktuellen Missionswoche (binary long) |
| `VSyncEnabled` | int | 0=aus, 1=an |
| `FPSCap` | int | 0=30, 1=60, 2=120, 3=240, 4=unlimitiert |
| `ResolutionIndex` | int | Index in Screen.resolutions |
| `ActivePet` | string | Aktives Pet (z.B. `"BlackCat"`) |
| `HasPet[Name]` | int | 0/1 ob Pet gekauft |

## Backend: Unity Gaming Services
Migriert von eigenem Raspberry-Pi-Server (api.benjo.online) auf Unity Services.

| Service | Zweck |
|---------|-------|
| Unity Authentication | Anonymous Sign-In, Display Name = Username |
| Unity Leaderboards | Score-Submission und -Anzeige |

**Leaderboard-IDs:** `FlappySteffLeaderboard` (Quickplay), `FlappySteffRankedLeaderboard` (Ranked)

Initialisierung: in `BootScene/UpdateCheckerScript.cs` (async Start). Jede Szene hat eigenen Fallback-Init-Check falls direkt im Editor gestartet.

Kein Echtzeit-Multiplayer. Netcode for GameObjects ist installiert aber ungenutzt.

## Ranked-Modus
- `RankedManager` ist DontDestroyOnLoad Singleton (`Assets/Scripts/Game/RankedManager.cs`)
- Wöchentlich deterministisches Item via ISO-Kalenderwoche-Seed (`RankedManager.WeeklyItem`)
- `RankedManager.IsRanked` (static bool) wird in MainMenu gesetzt
- Kein Pet in Ranked
- Eigenes Leaderboard
- Im MainMenu: Play-Bar mit Quickplay/Ranked-Buttons, bei Ranked zeigt `PnlRankedInfo` das Weekly Item (Icon + Name) und Reset-Zeit

## Wochenmissionen
Deterministischer Seed basierend auf ISO-Kalenderwoche + Jahr (z.B. `202614`).
Alle Spieler bekommen dieselben 3 Missionen pro Woche. Vollständig offline/lokal.

## Musik
`MusicPlayerScript.cs` in GameScene: Pitch skaliert mit Pipe-Speed von 1.0 (Startgeschwindigkeit) bis 1.5 (Maximalgeschwindigkeit) via `Mathf.InverseLerp(SpeedManager.startSpeed, SpeedManager.maxSpeed, currentSpeed)`.

## Discord Rich Presence
`Assets/Scripts/Discord/DiscordRichPresenceManager.cs` — DontDestroyOnLoad Singleton, in MainMenu-Szene platziert.
- Bibliothek: Lachee/DiscordRPC (`Assets/Plugins/DiscordRPC/DiscordRPC.dll`)
- Client ID im Inspector (`clientId`-Feld)
- Zeigt je nach Szene: "Im Hauptmenü" / "Score: X | Quickplay" / "Score: X | Ranked" / "Im Shop"
- Score-Update alle 5 Sekunden via Polling auf `LogicScript.playerScore`

## MainMenu UI-Architektur

### Tab-Navigation (TabController.cs)
`Canvas` hat 5 Haupt-Panels: `Pnl_Play`, `Pnl_Scoreboard`, `Pnl_Missions`, `Pnl_Shop`, `Pnl_Settings`.
`TabController.cs` schaltet zwischen ihnen um. Jedes Panel hat eine `CanvasGroup` für Fade-In (alpha 0→1, 0.2s).
TabBar hat ein `Img_Slider` (grün, `LayoutElement.ignoreLayout=true`) das per Coroutine animiert wird.

### Shop (Pnl_Shop)
Shop ist ein Tab in MainMenu, kein eigener Scene-Load mehr.
`ShopBar` hat `HorizontalLayoutGroup` + ein `Img_Slider` Child mit `LayoutElement.ignoreLayout=true`.
`ShopPageSwitcher.cs` schaltet zwischen 5 Kategorien: Items, Trails, Skins, Pets, Biomes.
Jedes Shop-Site-Panel (`ItemShop Site` etc.) hat eine `CanvasGroup` für Fade-In (0.15s).
Slider-Position wird via `indicator.GetComponentInParent<Button>().localPosition.x` ermittelt.

### Shop-Karten (ShopCardScript.cs)
Jede generierte Shop-Karte spielt beim Erscheinen eine Pop-in Animation (Scale 0.8→1.0, 0.15s).

### AnimateSlider-Pattern (in TabController, ShopPageSwitcher, MenuHandlerScript)
Alle Slider-Animationen: `Mathf.Lerp` auf `localPosition.x` + `sizeDelta.x` in Coroutine (0.2s).
`SnapSlider()` setzt initiale Position ohne Animation (wird in `Start()` vor erstem `SwitchTab()` aufgerufen).

### Wichtiger Unity-Hinweis
`GameObject.Find()` findet **keine inaktiven** GameObjects → stattdessen `canvas.transform.Find("Pnl_Shop/...")` nutzen.

## Architektur-Entscheidungen
- `WeeklyMissionManager` ist DontDestroyOnLoad Singleton
- `RankedManager` ist DontDestroyOnLoad Singleton
- `CursorManager` ist DontDestroyOnLoad Singleton (versteckt Cursor im Gameplay, zeigt ihn bei Pause/Tod/Menü)
- `DiscordRichPresenceManager` ist DontDestroyOnLoad Singleton
- Alle Käufe/Fortschritte in PlayerPrefs (kein Cloud Save)
- Auto-Update über GitHub Releases API (lädt .exe Installer herunter)
- Spieler-Eingabe: Tastatur + Maus + Xbox Controller (JoystickButton*)
- `SpeedManagerScript.cs`: `startSpeed = 5f`, `maxSpeed` = Endwert; `SpeedManagerCannabisScript` für Cannabis-Blatt-Geschwindigkeit
