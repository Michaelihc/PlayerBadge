# Implementation Notes

## Current Migration

- Converted from EXILED to LabAPI `Plugin<Config>` targeting `net48`.
- Uses `PlayerEvents.Joined`, `PlayerEvents.Left`, and `ServerEvents.WaitingForPlayers`.
- Applies custom titles through LabAPI `Player.GroupName` and `Player.GroupColor`; this avoids `CustomInfo` conflicts with XP/badge HUD plugins.
- Supports `multi` compact title segments (`red=Admin`, `red=Admin|cyan=Helper`) and `rich` raw SCP:SL/Unity rich text titles by keeping inline color tags in `GroupName`.
- Stores the original group text/color for players it modifies and restores them on disable or when a player no longer has a matching title.
- Rainbow titles are updated by a lightweight Unity `MonoBehaviour` timer instead of MEC.
- `BadgeManager` now owns live set/remove/reload/list operations for the title file. The `pbadge` RA/game-console command uses that service, persists changes to `PlayerBadge.txt`, reloads, and immediately reapplies titles to online players.
- RA permission for live changes is `playerbadge.manage`; game console is allowed for local administration.
- The command aliases intentionally omit `tag` because the base server already registers that alias for its native tag command. Keep `pbadge`, `playerbadge`, and `ptag` only unless the native command surface changes.
- The plugin now hard-references `SimpleHints.Api.dll` and uses low LabAPI load priority so SimpleHints can initialize first. PlayerBadge does not currently emit hint text, so this is a load/dependency alignment for the QoL collection rather than a display change.

## Source Evidence

- LabAPI plugin model: `..\.references\LabAPI\LabApi\Loader\Features\Plugins\Plugin{TConfig}.cs`
- Player events: `..\.references\LabAPI\LabApi\Events\Handlers\PlayerEvents.EventHandlers.cs`
- Group title APIs: `..\.references\LabAPI\LabApi\Features\Wrappers\Players\Player.cs`

## Open Questions

- Live multiplayer verification is needed to confirm server-role color names render identically to the older EXILED rank behavior.
