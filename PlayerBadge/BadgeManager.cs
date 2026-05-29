using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader;

namespace PlayerBadge
{
    public class BadgeManager
    {
        private readonly List<BadgeData> _badges = new List<BadgeData>();
        private readonly HashSet<string> _rainbowPlayers = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, OriginalBadge> _originalBadges = new Dictionary<string, OriginalBadge>(StringComparer.Ordinal);
        private readonly string[] _availableColors = { "red", "yellow", "cyan", "green", "aqua", "pink", "white", "orange" };

        private float _accumulator;
        private int _currentColorIndex;

        public void LoadBadges()
        {
            try
            {
                string configPath = GetConfigPath();
                string directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(configPath))
                {
                    CreateExampleConfigFile(configPath);
                }

                _badges.Clear();
                foreach (string line in File.ReadAllLines(configPath))
                {
                    BadgeData badge = BadgeData.ParseFromConfigLine(line);
                    if (badge == null)
                    {
                        continue;
                    }

                    _badges.Add(badge);
                    if (PlayerBadgePlugin.Instance.Config.Debug)
                    {
                        Logger.Debug($"[PlayerBadge] Loaded title: {badge}");
                    }
                }

                Logger.Info(PlayerBadgePlugin.Instance.Text(
                    $"Loaded {_badges.Count} custom title entries.",
                    $"已加载 {_badges.Count} 条自定义称号配置。"));
            }
            catch (Exception ex)
            {
                Logger.Error(PlayerBadgePlugin.Instance.Text(
                    $"Failed to load title config: {ex.Message}",
                    $"加载称号配置失败：{ex.Message}"));
            }
        }

        public void ApplyBadgeToPlayer(Player player)
        {
            if (player == null || string.IsNullOrWhiteSpace(player.UserId))
            {
                return;
            }

            try
            {
                BadgeData badge = _badges.FirstOrDefault(entry => entry.MatchesPlayer(player));
                if (badge == null)
                {
                    RestorePlayer(player);
                    return;
                }

                StoreOriginal(player);
                player.GroupName = badge.DisplayText;
                player.GroupColor = badge.IsRainbow
                    ? _availableColors[_currentColorIndex]
                    : badge.UsesInlineColors
                        ? "white"
                        : badge.Color;

                if (badge.IsRainbow)
                {
                    _rainbowPlayers.Add(player.UserId);
                }
                else
                {
                    _rainbowPlayers.Remove(player.UserId);
                }

                if (PlayerBadgePlugin.Instance.Config.Debug)
                {
                    Logger.Debug($"[PlayerBadge] Applied title to {player.LogName}: {badge}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(PlayerBadgePlugin.Instance.Text(
                    $"Failed to apply title to {player?.LogName}: {ex.Message}",
                    $"为玩家 {player?.LogName} 应用称号失败：{ex.Message}"));
            }
        }

        public void RemoveRainbowPlayer(Player player)
        {
            if (!string.IsNullOrWhiteSpace(player?.UserId))
            {
                _rainbowPlayers.Remove(player.UserId);
            }
        }

        public void ReloadConfig()
        {
            _rainbowPlayers.Clear();
            LoadBadges();
            foreach (Player player in Player.ReadyList)
            {
                ApplyBadgeToPlayer(player);
            }
        }

        public bool SetBadge(string playerIdentifier, string color, string text, out string normalizedIdentifier, out string error)
        {
            normalizedIdentifier = NormalizeIdentifier(playerIdentifier);
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedIdentifier))
            {
                error = "Player identifier cannot be empty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(color))
            {
                error = "Color cannot be empty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                error = "Title text cannot be empty.";
                return false;
            }

            string configPath = GetConfigPath();
            EnsureConfigFile(configPath);
            List<string> lines = File.ReadAllLines(configPath).ToList();
            string identifier = normalizedIdentifier;
            lines.RemoveAll(line => IsMatchingConfiguredIdentifier(line, identifier));
            lines.Add($"{normalizedIdentifier}:{color.Trim()}:{text.Trim()}");
            File.WriteAllLines(configPath, lines);
            ReloadConfig();
            return true;
        }

        public bool RemoveBadge(string playerIdentifier, out string normalizedIdentifier)
        {
            normalizedIdentifier = NormalizeIdentifier(playerIdentifier);
            if (string.IsNullOrWhiteSpace(normalizedIdentifier))
            {
                return false;
            }

            string configPath = GetConfigPath();
            EnsureConfigFile(configPath);
            List<string> lines = File.ReadAllLines(configPath).ToList();
            string identifier = normalizedIdentifier;
            int removed = lines.RemoveAll(line => IsMatchingConfiguredIdentifier(line, identifier));
            if (removed <= 0)
            {
                return false;
            }

            File.WriteAllLines(configPath, lines);
            ReloadConfig();
            return true;
        }

        public IReadOnlyList<BadgeData> GetBadgesSnapshot()
        {
            return _badges.ToList();
        }

        public string GetBadgeFilePath()
        {
            return GetConfigPath();
        }

        public void Tick(float deltaSeconds)
        {
            if (_rainbowPlayers.Count == 0)
            {
                return;
            }

            _accumulator += deltaSeconds;
            float interval = Math.Max(0.1f, PlayerBadgePlugin.Instance.Config.RainbowInterval);
            if (_accumulator < interval)
            {
                return;
            }

            _accumulator = 0f;
            _currentColorIndex = (_currentColorIndex + 1) % _availableColors.Length;
            string color = _availableColors[_currentColorIndex];

            foreach (Player player in Player.ReadyList)
            {
                if (_rainbowPlayers.Contains(player.UserId))
                {
                    player.GroupColor = color;
                }
            }
        }

        public void RestoreAll()
        {
            foreach (Player player in Player.ReadyList)
            {
                RestorePlayer(player);
            }

            _rainbowPlayers.Clear();
            _originalBadges.Clear();
        }

        private void RestorePlayer(Player player)
        {
            if (player == null || string.IsNullOrWhiteSpace(player.UserId))
            {
                return;
            }

            if (_originalBadges.TryGetValue(player.UserId, out OriginalBadge original))
            {
                player.GroupName = original.Name;
                player.GroupColor = original.Color;
                _originalBadges.Remove(player.UserId);
            }

            _rainbowPlayers.Remove(player.UserId);
        }

        private void StoreOriginal(Player player)
        {
            if (!_originalBadges.ContainsKey(player.UserId))
            {
                _originalBadges[player.UserId] = new OriginalBadge(player.GroupName, player.GroupColor);
            }
        }

        private string GetConfigPath()
        {
            string configured = PlayerBadgePlugin.Instance.Config.ConfigFilePath;
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }

            return Path.Combine(PlayerBadgePlugin.Instance.GetConfigDirectory().FullName, "PlayerBadge.txt");
        }

        private static string NormalizeIdentifier(string playerIdentifier)
        {
            string value = (playerIdentifier ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Contains("@") ? value : value + "@any";
        }

        private static bool IsMatchingConfiguredIdentifier(string configLine, string normalizedIdentifier)
        {
            BadgeData badge = BadgeData.ParseFromConfigLine(configLine);
            if (badge == null)
            {
                return false;
            }

            string candidate = NormalizeIdentifier($"{badge.PlayerId}@{badge.Platform}");
            return string.Equals(candidate, normalizedIdentifier, StringComparison.OrdinalIgnoreCase);
        }

        private void EnsureConfigFile(string configPath)
        {
            string directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(configPath))
            {
                CreateExampleConfigFile(configPath);
            }
        }

        private void CreateExampleConfigFile(string configPath)
        {
            string exampleContent = @"# PlayerBadge config / PlayerBadge 配置文件
# Format / 格式: playerId@platform:color:title
# Platforms / 平台: steam, discord, any
# Colors / 颜色: red, yellow, cyan, green, aqua, pink, white, orange, rainbow, multi, rich
# Examples / 示例:
# 76561198000000000@steam:red:Admin
# 123456789@discord:rainbow:VIP
# 76561198111111111@steam:multi:red=Admin|cyan=Helper
# 76561198222222222@steam:rich:<color=#ff5577>Pink</color><color=#55ddff>Blue</color>
";
            File.WriteAllText(configPath, exampleContent);
            Logger.Info(PlayerBadgePlugin.Instance.Text(
                $"Created example title config: {configPath}",
                $"已创建示例称号配置：{configPath}"));
        }

        private readonly struct OriginalBadge
        {
            public OriginalBadge(string name, string color)
            {
                Name = name ?? string.Empty;
                Color = color ?? string.Empty;
            }

            public string Name { get; }

            public string Color { get; }
        }
    }
}
