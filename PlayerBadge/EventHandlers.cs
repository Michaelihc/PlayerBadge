using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Console;

namespace PlayerBadge
{
    public class EventHandlers
    {
        public void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            if (PlayerBadgePlugin.Instance.Config.Debug)
            {
                Logger.Debug($"[PlayerBadge] Checking title for {ev.Player.LogName}.");
            }

            PlayerBadgePlugin.Instance.BadgeManager.ApplyBadgeToPlayer(ev.Player);
        }

        public void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            PlayerBadgePlugin.Instance.BadgeManager?.RemoveRainbowPlayer(ev.Player);
        }

        public void OnWaitingForPlayers()
        {
            if (PlayerBadgePlugin.Instance.Config.Debug)
            {
                Logger.Debug("[PlayerBadge] Reloading title config while waiting for players.");
            }

            PlayerBadgePlugin.Instance.BadgeManager.ReloadConfig();
        }
    }
}
