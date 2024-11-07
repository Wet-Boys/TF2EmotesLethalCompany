using LethalConfig.ConfigItems;
using LethalConfig;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;
using UnityEngine;

namespace TitanFall2Emotes
{
    internal static class LethalConfig
    {
        internal static void SetupLethalConfig()
        {
            var aVeryCoolIconAsset = Assets.Load<Sprite>("lethalconfigicon.png");
            LethalConfigManager.SetModIcon(aVeryCoolIconAsset);
            LethalConfigManager.SetModDescription("Are those Latin rhythms? I love Latin rhythms!");
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(Settings.scout, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(Settings.soldier, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(Settings.pyro, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(Settings.demo, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(Settings.heavy, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(Settings.engi, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(Settings.medic, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(Settings.sniper, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(Settings.spy, false));

        }
    }
}
