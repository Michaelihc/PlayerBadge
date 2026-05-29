using System;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using SimpleHints.Api;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace PlayerBadge
{
    public class PlayerBadgePlugin : Plugin<Config>
    {
        private GameObject _runnerObject;

        public static PlayerBadgePlugin Instance { get; private set; }

        public EventHandlers EventHandlers { get; private set; }

        public BadgeManager BadgeManager { get; private set; }

        public override string Name => "PlayerBadge";

        public override string Description => "Localized LabAPI custom title and rainbow title plugin.";

        public override string Author => "kldhsh123, LabAPI migration by Codex";

        public override Version Version => new Version(2, 0, 0);

        public override Version RequiredApiVersion => new Version(LabApiProperties.CompiledVersion);

        public override LabApi.Loader.Features.Plugins.Enums.LoadPriority Priority => LabApi.Loader.Features.Plugins.Enums.LoadPriority.Low;

        public override void Enable()
        {
            Instance = this;
            Logger.Info($"[PlayerBadge] SimpleHints dependency ready={SimpleHintsApi.IsReady}.");

            if (!Config.IsEnabled)
            {
                Logger.Info("[PlayerBadge] Disabled by config.");
                return;
            }

            BadgeManager = new BadgeManager();
            BadgeManager.LoadBadges();

            EventHandlers = new EventHandlers();
            PlayerEvents.Joined += EventHandlers.OnPlayerJoined;
            PlayerEvents.Left += EventHandlers.OnPlayerLeft;
            ServerEvents.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;

            _runnerObject = new GameObject("PlayerBadge LabAPI Runner");
            UnityEngine.Object.DontDestroyOnLoad(_runnerObject);
            _runnerObject.AddComponent<PlayerBadgeUpdateBehaviour>().Initialize(BadgeManager);

            foreach (Player player in Player.ReadyList)
            {
                BadgeManager.ApplyBadgeToPlayer(player);
            }

            Logger.Info(Text("PlayerBadge enabled.", "PlayerBadge 插件已启用。"));
        }

        public override void Disable()
        {
            if (EventHandlers != null)
            {
                PlayerEvents.Joined -= EventHandlers.OnPlayerJoined;
                PlayerEvents.Left -= EventHandlers.OnPlayerLeft;
                ServerEvents.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;
            }

            if (_runnerObject != null)
            {
                UnityEngine.Object.Destroy(_runnerObject);
                _runnerObject = null;
            }

            BadgeManager?.RestoreAll();

            EventHandlers = null;
            BadgeManager = null;
            Instance = null;

            Logger.Info("[PlayerBadge] Disabled.");
        }

        internal string Text(string english, string chinese)
        {
            string language = (Config.Language ?? string.Empty).Trim().ToLowerInvariant();
            return language == "en" ? english : chinese;
        }
    }

    internal sealed class PlayerBadgeUpdateBehaviour : MonoBehaviour
    {
        private BadgeManager _manager;

        public void Initialize(BadgeManager manager)
        {
            _manager = manager;
        }

        private void Update()
        {
            _manager?.Tick(Time.unscaledDeltaTime);
        }
    }
}
