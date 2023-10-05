using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using System;
using Dalamud.Memory;

namespace FriendlyFire
{
    public unsafe class Plugin : IDalamudPlugin
    {
        [PluginService] public static DalamudPluginInterface? PluginInterface { get; private set; } = null;
        [PluginService] public static IFramework? iFramework { get; private set; } = null;
        [PluginService] public static IClientState? ClientState { get; private set; } = null;
        [PluginService] public static ICondition? Condition { get; private set; } = null;
        [PluginService] public static IChatGui? ChatGui { get; private set; } = null;
        [PluginService] public static IPluginLog? Log { get; private set; } = null;

        public string Name => "FriendlyFire";

        private ConfigBase*[] cfgBase = new ConfigBase*[4];
        private Dictionary<string, List<Tuple<uint, uint>>> MappedSettings = new Dictionary<string, List<Tuple<uint, uint>>>();

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
            pluginReady = false;
        }

        private unsafe void Update(IFramework framework)
        {
            if (pluginReady)
            {
                PlayerCharacter? player = ClientState!.LocalPlayer;
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
                        SetSettingsValue(MappedSettings["AutoNearestTarget"], optionValue);
                    }
                }
            }
        }

        public static void PrintEcho(string message) => ChatGui!.Print($"[FriendlyFire] {message}");
        public static void PrintError(string message) => ChatGui!.PrintError($"[FriendlyFire] {message}");

        private void Initialize()
        {
            cfgBase[0] = &(frameworkInstance->SystemConfig.CommonSystemConfig.ConfigBase);
            cfgBase[1] = &(frameworkInstance->SystemConfig.CommonSystemConfig.UiConfig);
            cfgBase[2] = &(frameworkInstance->SystemConfig.CommonSystemConfig.UiControlConfig);
            cfgBase[3] = &(frameworkInstance->SystemConfig.CommonSystemConfig.UiControlGamepadConfig);

            List<string> cfgSearchStrings = new List<string>() {
                "AutoNearestTarget"
                };

            MappedSettings.Clear();
            for (uint cfgId = 0; cfgId < cfgBase.Length; cfgId++)
            {
                for (uint i = 0; i < cfgBase[cfgId]->ConfigCount; i++)
                {
                    ConfigEntry cfgItem = cfgBase[cfgId]->ConfigEntry[i];
                    if (cfgItem.Type == 0)
                        continue;

                    string name = MemoryHelper.ReadStringNullTerminated(new IntPtr(cfgItem.Name));
                    if (cfgSearchStrings.Contains(name))
                    {
                        if (!MappedSettings.ContainsKey(name))
                            MappedSettings[name] = new List<Tuple<uint, uint>>();
                        MappedSettings[name].Add(new Tuple<uint, uint>(cfgId, i));
                    }
                }
            }

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

        private void SetSettingsValue(List<Tuple<uint, uint>> list, uint value)
        {
            foreach (Tuple<uint, uint> item in list)
                cfgBase[item.Item1]->ConfigEntry[item.Item2].SetValueUInt(value);
        }

        private uint GetSettingsValue(List<Tuple<uint, uint>> list, int index)
        {
            if (index >= list.Count)
                return 0;
            return cfgBase[list[index].Item1]->ConfigEntry[list[index].Item2].Value.UInt;
        }
    }
}
