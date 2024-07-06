using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System.Collections.Generic;
using SettingsManager;

namespace FriendlyFire
{
    public unsafe class Plugin : IDalamudPlugin
    {
        [PluginService] public static IDalamudPluginInterface? iPluginInterface { get; private set; } = null;
        [PluginService] public static IFramework? iFramework { get; private set; } = null;
        [PluginService] public static ICommandManager? CommandManager { get; private set; } = null;
        [PluginService] public static IClientState? ClientState { get; private set; } = null;
        [PluginService] public static ICondition? Condition { get; private set; } = null;
        [PluginService] public static IChatGui? ChatGui { get; private set; } = null;
        [PluginService] public static IPluginLog? Log { get; private set; } = null;

        public string Name => "FriendlyFire";

        public static ConfigManager cfgManager = new ConfigManager();

        private Dictionary<int, bool> targetSettignsPerClass = new Dictionary<int, bool>();
        private byte curClassJob = 0;
        private Framework* frameworkInstance = Framework.Instance();
        private bool pluginReady = false;

        public Plugin()
        {
            iFramework!.Update += Update;
            Initialize();
            pluginReady = true;
        }

        public void Dispose()
        {
            iFramework!.Update -= Update;
            cfgManager.Dispose();
            pluginReady = false;
        }

        private unsafe void Update(IFramework framework)
        {
            if (pluginReady)
            {
                IPlayerCharacter? player = ClientState!.LocalPlayer;
                if (player != null)
                {
                    Character* characer = (Character*)player.Address;
                    byte classJob = characer->CharacterData.ClassJob;
                    if (curClassJob != classJob)
                    {
                        curClassJob = classJob;
                        uint optionValue = 1;
                        if (targetSettignsPerClass.ContainsKey(curClassJob))
                            optionValue = 0;
                        cfgManager.SetSettingsValue("AutoNearestTarget", optionValue);
                    }
                }
            }
        }

        public static void PrintEcho(string message) => ChatGui!.Print($"[FriendlyFire] {message}");
        public static void PrintError(string message) => ChatGui!.PrintError($"[FriendlyFire] {message}");

        private void Initialize()
        {
            List<string> cfgSearchStrings = new List<string>() {
                "AutoNearestTarget",
            };
            cfgManager.AddToList(cfgSearchStrings);
            cfgManager.MapSettings();

            targetSettignsPerClass.Add(5, true);  // Bard
            targetSettignsPerClass.Add(6, true);  // White Mage
            targetSettignsPerClass.Add(7, true);  // Black Mage
            targetSettignsPerClass.Add(23, true); // Bard
            targetSettignsPerClass.Add(24, true); // White Mage
            targetSettignsPerClass.Add(25, true); // Black Mage
            targetSettignsPerClass.Add(26, true); // Summoner
            targetSettignsPerClass.Add(27, true); // Summoner
            targetSettignsPerClass.Add(28, true); // Schollar
            targetSettignsPerClass.Add(31, true); // Machinest
            targetSettignsPerClass.Add(33, true); // Astrologen
            targetSettignsPerClass.Add(35, true); // Red Mage
            targetSettignsPerClass.Add(38, true); // Dancer
            targetSettignsPerClass.Add(40, true); // Sage
            targetSettignsPerClass.Add(42, true); // Pictomancer
        }
    }
}
