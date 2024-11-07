using BepInEx.Bootstrap;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TitanFall2Emotes
{
    public static class Settings
    {
        public static ConfigEntry<bool> scout;
        public static ConfigEntry<bool> soldier;
        public static ConfigEntry<bool> pyro;
        public static ConfigEntry<bool> demo;
        public static ConfigEntry<bool> heavy;
        public static ConfigEntry<bool> engi;
        public static ConfigEntry<bool> medic;
        public static ConfigEntry<bool> sniper;
        public static ConfigEntry<bool> spy;

        internal static void RunAll()
        {
            SetupConfig();
            if (Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
            {
                LethalConfig.SetupLethalConfig();
            }
            LoadSettings();
        }
        private static void SetupConfig()
        {
            scout = TF2Plugin.Instance.Config.Bind<bool>("Merc Selection", "Scout", true, "Puts Scout into the random merc pool");
            soldier = TF2Plugin.Instance.Config.Bind<bool>("Merc Selection", "Soldier", true, "Puts Soldier into the random merc pool");
            pyro = TF2Plugin.Instance.Config.Bind<bool>("Merc Selection", "Pyro", true, "Puts Pyro into the random merc pool");
            demo = TF2Plugin.Instance.Config.Bind<bool>("Merc Selection", "Demoman", true, "Puts Demoman into the random merc pool");
            heavy = TF2Plugin.Instance.Config.Bind<bool>("Merc Selection", "Heavy", true, "Puts Heavy into the random merc pool");
            engi = TF2Plugin.Instance.Config.Bind<bool>("Merc Selection", "Engineer", true, "Puts Engineer into the random merc pool");
            medic = TF2Plugin.Instance.Config.Bind<bool>("Merc Selection", "Medic", true, "Puts Medic into the random merc pool");
            sniper = TF2Plugin.Instance.Config.Bind<bool>("Merc Selection", "Sniper", true, "Puts Sniper into the random merc pool");
            spy = TF2Plugin.Instance.Config.Bind<bool>("Merc Selection", "Spy", true, "Puts Spy into the random merc pool");

            scout.SettingChanged += Merc_SettingChanged;
            soldier.SettingChanged += Merc_SettingChanged;
            pyro.SettingChanged += Merc_SettingChanged;
            demo.SettingChanged += Merc_SettingChanged;
            heavy.SettingChanged += Merc_SettingChanged;
            engi.SettingChanged += Merc_SettingChanged;
            medic.SettingChanged += Merc_SettingChanged;
            sniper.SettingChanged += Merc_SettingChanged;
            spy.SettingChanged += Merc_SettingChanged;
        }

        private static void Merc_SettingChanged(object sender, EventArgs e)
        {
            LoadSettings();
        }



        internal static void LoadSettings()
        {
            TF2Plugin.validMercs.Clear();
            if (demo.Value)
            {
                TF2Plugin.validMercs.Add(0);
            }
            if (engi.Value)
            {
                TF2Plugin.validMercs.Add(1);
            }
            if (heavy.Value)
            {
                TF2Plugin.validMercs.Add(2);
            }
            if (medic.Value)
            {
                TF2Plugin.validMercs.Add(3);
            }
            if (pyro.Value)
            {
                TF2Plugin.validMercs.Add(4);
            }
            if (scout.Value)
            {
                TF2Plugin.validMercs.Add(5);
            }
            if (sniper.Value)
            {
                TF2Plugin.validMercs.Add(6);
            }
            if (soldier.Value)
            {
                TF2Plugin.validMercs.Add(7);
            }
            if (spy.Value)
            {
                TF2Plugin.validMercs.Add(8);
            }
        }
    }
}
