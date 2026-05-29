# CHECKLIST

- [x] Converted project to LabAPI `net48`.
- [x] Removed EXILED dependencies from source/project.
- [x] Replaced EXILED verified/left/waiting events with LabAPI events.
- [x] Uses `LabApi.Features.Wrappers.Player.GroupName` and `GroupColor` for titles.
- [x] Preserves static named colors, inline multi-color rich-text titles, and rainbow color cycling.
- [x] Added live `pbadge` RA/game-console commands for set/remove/reload/list.
- [x] Live set/remove persists to `PlayerBadge.txt`, reloads, and immediately reapplies online player titles.
- [x] Adds `language: ""|"cn"|"en"` config semantics for logs/docs.
- [x] Avoids direct hint output; no RueI text is emitted by this plugin.
- [x] Updates localized `README.md`.
- [x] Adds `implementation-notes.md`.
- [x] Build verified.
- [ ] Manual in-game verification on a visible test server.
- [ ] Multiplayer verification for join/leave cleanup and rainbow updates.
