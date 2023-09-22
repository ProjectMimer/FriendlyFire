using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Collections.Generic;

namespace FriendlyFire
{
    public sealed class FriendlyFire : IDalamudPlugin
    {
        [PluginService] public static DalamudPluginInterface? PluginInterface { get; private set; }
        [PluginService] public static Framework? Framework { get; private set; }
        [PluginService] public static ClientState? ClientState { get; private set; }
        [PluginService] public static Condition? Condition { get; private set; }
        [PluginService] public static ChatGui? ChatGui { get; private set; }

        public FriendlyFire Plugin { get; init; }
        public string Name => "FriendlyFire";

        private Dictionary<int, bool> targetSettignsPerClass = new Dictionary<int, bool>();
        private byte curClassJob = 0;

        public FriendlyFire()
        {
            Plugin = this;
            Framework!.Update += Update;
            Initialize();
        }

        public void Dispose()
        {
            Framework!.Update -= Update;
        }

        private unsafe void Update(Framework framework)
        {
            PlayerCharacter? player = ClientState!.LocalPlayer;
            if (player != null)
            {
                Character* characer = (Character*)player.Address;
                byte classJob = characer->CharacterData.ClassJob;
                if(curClassJob != classJob)
                {
                    curClassJob = classJob;
                    int optionValue = 1;
                    if (targetSettignsPerClass.ContainsKey(curClassJob))
                        optionValue = 0;
                    ConfigModule.Instance()->SetOption(ConfigOption.AutoNearestTarget, optionValue);
                }
            }
        }

        public static void PrintEcho(string message) => ChatGui!.Print($"[FriendlyFire] {message}");
        public static void PrintError(string message) => ChatGui!.PrintError($"[FriendlyFire] {message}");

        private void Initialize()
        {
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
        }
    }
}
