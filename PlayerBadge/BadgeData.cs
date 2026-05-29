using System;
using LabApi.Features.Wrappers;

namespace PlayerBadge
{
    public class BadgeData
    {
        public BadgeData(string playerId, string platform, string color, string text)
        {
            PlayerId = playerId;
            Platform = platform;
            Color = string.IsNullOrWhiteSpace(color) ? "white" : color;
            Text = text;
            IsRainbow = Color.Equals("rainbow", StringComparison.OrdinalIgnoreCase);
            UsesInlineColors =
                Color.Equals("multi", StringComparison.OrdinalIgnoreCase) ||
                Color.Equals("multicolor", StringComparison.OrdinalIgnoreCase) ||
                Color.Equals("rich", StringComparison.OrdinalIgnoreCase);
            DisplayText = UsesInlineColors ? BuildInlineColorText(text) : text;
        }

        public string PlayerId { get; }

        public string Platform { get; }

        public string Color { get; }

        public string Text { get; }

        public string DisplayText { get; }

        public bool IsRainbow { get; }

        public bool UsesInlineColors { get; }

        public static BadgeData ParseFromConfigLine(string configLine)
        {
            if (string.IsNullOrWhiteSpace(configLine) || configLine.TrimStart().StartsWith("#"))
            {
                return null;
            }

            try
            {
                string[] parts = configLine.Split(':');
                if (parts.Length < 3)
                {
                    return null;
                }

                string idPlatformPart = parts[0];
                int atIndex = idPlatformPart.LastIndexOf('@');
                if (atIndex == -1)
                {
                    return null;
                }

                string playerId = idPlatformPart.Substring(0, atIndex).Trim();
                string platform = idPlatformPart.Substring(atIndex + 1).Trim();
                string color = parts[1].Trim();
                string text = string.Join(":", parts, 2, parts.Length - 2).Trim();

                return string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(text)
                    ? null
                    : new BadgeData(playerId, platform, color, text);
            }
            catch
            {
                return null;
            }
        }

        public bool MatchesPlayer(Player player)
        {
            if (player == null || string.IsNullOrWhiteSpace(player.UserId))
            {
                return false;
            }

            return player.UserId.IndexOf(PlayerId, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public override string ToString()
        {
            return $"{PlayerId}@{Platform}:{Color}:{Text}";
        }

        private static string BuildInlineColorText(string text)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                text.IndexOf("<color=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return text;
            }

            string result = string.Empty;
            string[] segments = text.Split('|');
            foreach (string segment in segments)
            {
                int separator = segment.IndexOf('=');
                if (separator <= 0 || separator >= segment.Length - 1)
                {
                    return text;
                }

                string color = segment.Substring(0, separator).Trim();
                string value = segment.Substring(separator + 1).Trim();
                if (string.IsNullOrWhiteSpace(color) || string.IsNullOrWhiteSpace(value))
                {
                    return text;
                }

                result += $"<color={color}>{value}</color>";
            }

            return result;
        }
    }
}
