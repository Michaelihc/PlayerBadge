using System;
using System.Linq;
using System.Text;
using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;

namespace PlayerBadge.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class PlayerBadgeCommand : ParentCommand
    {
        public PlayerBadgeCommand()
        {
            LoadGeneratedCommands();
        }

        public override string Command => "pbadge";

        public override string[] Aliases { get; } = { "playerbadge", "ptag" };

        public override string Description => "PlayerBadge live title commands. Usage: pbadge <set|remove|reload|list>";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new SetBadgeSubcommand());
            RegisterCommand(new RemoveBadgeSubcommand());
            RegisterCommand(new ReloadBadgeSubcommand());
            RegisterCommand(new ListBadgeSubcommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Usage: pbadge <set|remove|reload|list>";
            return false;
        }

        internal static bool CheckPermission(ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null)
            {
                response = null;
                return true;
            }

            if (!player.HasPermissions("playerbadge.manage"))
            {
                response = "Missing permission: playerbadge.manage";
                return false;
            }

            response = null;
            return true;
        }

        internal static bool TryGetManager(out BadgeManager manager, out string response)
        {
            manager = PlayerBadgePlugin.Instance?.BadgeManager;
            if (manager == null)
            {
                response = "PlayerBadge is not enabled.";
                return false;
            }

            response = null;
            return true;
        }

        internal static string ResolveTargetIdentifier(string query)
        {
            query = (query ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                return string.Empty;
            }

            Player exact = Player.ReadyList.FirstOrDefault(player =>
                IsSame(player.UserId, query) ||
                IsSame(player.Nickname, query) ||
                IsSame(player.DisplayName, query));
            if (exact != null)
            {
                return exact.UserId;
            }

            Player partial = Player.ReadyList.FirstOrDefault(player =>
                Contains(player.UserId, query) ||
                Contains(player.Nickname, query) ||
                Contains(player.DisplayName, query));

            return partial?.UserId ?? query;
        }

        private static bool IsSame(string value, string query)
        {
            return string.Equals(value ?? string.Empty, query, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Contains(string value, string query)
        {
            return (value ?? string.Empty).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    internal sealed class SetBadgeSubcommand : ICommand, IUsageProvider
    {
        public string Command => "set";

        public string[] Aliases => new[] { "add", "s" };

        public string Description => "Sets and immediately applies a player title. Usage: pbadge set <player|userId> <color> <title...>";

        public string[] Usage => new[] { "<player|userId>", "<color>", "<title...>" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!PlayerBadgeCommand.CheckPermission(sender, out response) ||
                !PlayerBadgeCommand.TryGetManager(out BadgeManager manager, out response))
            {
                return false;
            }

            if (arguments.Count < 3)
            {
                response = "Usage: pbadge set <player|userId> <color> <title...>";
                return false;
            }

            string target = PlayerBadgeCommand.ResolveTargetIdentifier(arguments.At(0));
            string color = arguments.At(1);
            string title = string.Join(" ", arguments.Skip(2));

            if (!manager.SetBadge(target, color, title, out string normalizedIdentifier, out string error))
            {
                response = error;
                return false;
            }

            response = $"Set title for {normalizedIdentifier}: {color} -> {title}";
            return true;
        }
    }

    internal sealed class RemoveBadgeSubcommand : ICommand, IUsageProvider
    {
        public string Command => "remove";

        public string[] Aliases => new[] { "rm", "clear", "delete" };

        public string Description => "Removes a configured player title and restores online players. Usage: pbadge remove <player|userId>";

        public string[] Usage => new[] { "<player|userId>" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!PlayerBadgeCommand.CheckPermission(sender, out response) ||
                !PlayerBadgeCommand.TryGetManager(out BadgeManager manager, out response))
            {
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage: pbadge remove <player|userId>";
                return false;
            }

            string target = PlayerBadgeCommand.ResolveTargetIdentifier(arguments.At(0));
            if (!manager.RemoveBadge(target, out string normalizedIdentifier))
            {
                response = $"No configured title found for {normalizedIdentifier}.";
                return false;
            }

            response = $"Removed title for {normalizedIdentifier}.";
            return true;
        }
    }

    internal sealed class ReloadBadgeSubcommand : ICommand
    {
        public string Command => "reload";

        public string[] Aliases => new[] { "r" };

        public string Description => "Reloads PlayerBadge.txt and reapplies titles to online players.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!PlayerBadgeCommand.CheckPermission(sender, out response) ||
                !PlayerBadgeCommand.TryGetManager(out BadgeManager manager, out response))
            {
                return false;
            }

            manager.ReloadConfig();
            response = $"Reloaded titles from {manager.GetBadgeFilePath()}.";
            return true;
        }
    }

    internal sealed class ListBadgeSubcommand : ICommand
    {
        public string Command => "list";

        public string[] Aliases => new[] { "ls" };

        public string Description => "Lists configured player titles.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!PlayerBadgeCommand.CheckPermission(sender, out response) ||
                !PlayerBadgeCommand.TryGetManager(out BadgeManager manager, out response))
            {
                return false;
            }

            var badges = manager.GetBadgesSnapshot();
            if (badges.Count == 0)
            {
                response = $"No configured titles. File: {manager.GetBadgeFilePath()}";
                return true;
            }

            int limit = Math.Min(20, badges.Count);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Configured titles: {badges.Count}. File: {manager.GetBadgeFilePath()}");
            for (int i = 0; i < limit; i++)
            {
                builder.AppendLine(badges[i].ToString());
            }

            if (badges.Count > limit)
            {
                builder.AppendLine($"...and {badges.Count - limit} more.");
            }

            response = builder.ToString().TrimEnd();
            return true;
        }
    }
}
