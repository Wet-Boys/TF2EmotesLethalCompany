using BepInEx;
using BepInEx.Configuration;
using EmotesAPI;
using GameNetcodeStuff;
using Mono.Cecil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using TitanFall2Emotes.IMissWwise;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

namespace TitanFall2Emotes
{
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class TF2Plugin : BaseUnityPlugin
    {
        public static TF2Plugin Instance;
        public const string PluginGUID = "com.weliveinasociety.teamfortress2emotes";
        public const string PluginAuthor = "Nunchuk";
        public const string PluginName = "TF2Emotes";
        public const string PluginVersion = "1.0.0";
        internal static List<string> Conga_Emotes = new List<string>();
        internal static List<string> KazotskyKick_Emotes = new List<string>();
        internal static List<string> RPS_Start_Emotes = new List<string>();
        internal static List<string> RPS_Loss_Emotes = new List<string>();
        internal static List<string> RPS_Win_Emotes = new List<string>();
        internal static List<string> Flip_Wait_Emotes = new List<string>();
        internal static List<string> Flip_Flip_Emotes = new List<string>();
        internal static List<string> Flip_Throw_Emotes = new List<string>();
        internal static List<string> Laugh_Emotes = new List<string>();
        public static PluginInfo PInfo { get; private set; }
        internal static AudioContainerHolder containerHolder;

        private static GameObject tf2Networker;
        private void PlayerControllerStart(Action<PlayerControllerB> orig, PlayerControllerB self)
        {
            orig(self);
            if (self.IsServer && TF2Networker.instance is null)
            {
                GameObject networker = Instantiate<GameObject>(tf2Networker);
                networker.GetComponent<NetworkObject>().Spawn(true);
            }
            if (AudioContainerHolder.instance is null)
            {
                self.gameObject.AddComponent<AudioContainerHolder>();
            }
        }
        private static Hook playerControllerStartHook;
        private void NetworkManagerStart(Action<GameNetworkManager> orig, GameNetworkManager self)
        {
            orig(self);
            tf2Networker = Assets.Load<GameObject>($"tf2222networker.prefab");

            tf2Networker.AddComponent<TF2Networker>();
            GameNetworkManager.Instance.GetComponent<NetworkManager>().PrefabHandler.AddNetworkPrefab(tf2Networker);
        }
        private static Hook networkManagerStartHook;

        public void Awake()
        {
            Instance = this;
            PInfo = Info;

            var targetMethod = typeof(PlayerControllerB).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var destMethod = typeof(TF2Plugin).GetMethod(nameof(PlayerControllerStart), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerControllerStartHook = new Hook(targetMethod, destMethod, this);

            targetMethod = typeof(GameNetworkManager).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            destMethod = typeof(TF2Plugin).GetMethod(nameof(NetworkManagerStart), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            networkManagerStartHook = new Hook(targetMethod, destMethod, this);
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                try
                {
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    foreach (var method in methods)
                    {
                        var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                        if (attributes.Length > 0)
                        {
                            method.Invoke(null, null);
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
            Assets.LoadAssetBundlesFromFolder("assetbundles");
            RegisterAllSounds();
            //Assets.AddSoundBank("Init.bnk");
            Rancho();
            RPS();
            Conga();
            Flip();
            KazotskyKick();
            Laugh();

            ///////////////////////////////////////////////////////setup all tf2 emotes as vulnerable



            //DEBUG();
            CustomEmotesAPI.animJoined += CustomEmotesAPI_animJoined;
            CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
            CustomEmotesAPI.emoteSpotJoined_Body += CustomEmotesAPI_emoteSpotJoined_Body;
            //CustomEmotesAPI.boneMapperEnteredJoinSpot += CustomEmotesAPI_boneMapperEnteredJoinSpot;
        }

        //private void CustomEmotesAPI_boneMapperEnteredJoinSpot(BoneMapper mover, BoneMapper joinSpotOwner)
        //{
        //    if (Settings.EnemiesEmoteWithYou.Value && mover.mapperBody.teamComponent.teamIndex != TeamIndex.Player && mover.mapperBody.GetComponent<HealthComponent>().timeSinceLastHit > 5)
        //    {
        //        mover.JoinEmoteSpot();
        //    }
        //}

        private void CustomEmotesAPI_animJoined(string joinedAnimation, BoneMapper joiner, BoneMapper host)
        {
            if (joinedAnimation.EndsWith("_Conga"))
            {
                int num = UnityEngine.Random.Range(0, Conga_Emotes.Count);
                TF2Networker.instance.SyncEmoteToServerRpc(joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Conga_Start", num, joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
            }
            if (joinedAnimation.StartsWith("Kazotsky_"))
            {
                TF2Networker.instance.SyncEmoteToServerRpc(joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Kazotsky_Start", UnityEngine.Random.Range(0, KazotskyKick_Emotes.Count), joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }

        //bone mapper? if not then: model locator? model transform? bone mapper?
        //RoR2.CharacterAI.BaseAI.Target currentTarget = null;
        //private RoR2.CharacterAI.BaseAI.SkillDriverEvaluation? BaseAI_EvaluateSingleSkillDriver(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSingleSkillDriver orig, RoR2.CharacterAI.BaseAI self, ref RoR2.CharacterAI.BaseAI.SkillDriverEvaluation currentSkillDriverEvaluation, RoR2.CharacterAI.AISkillDriver aiSkillDriver, float myHealthFraction)
        //{
        //    var thing = orig(self, ref currentSkillDriverEvaluation, aiSkillDriver, myHealthFraction);
        //    if (thing.HasValue && thing.Value.aimTarget != currentTarget)
        //    {
        //        currentTarget = thing.Value.aimTarget;
        //        DebugClass.Log($"----------set target:   {self.body.teamComponent.teamIndex}     {self.body.gameObject}     {currentTarget}   {currentTarget.characterBody.gameObject}   {currentTarget.characterBody.teamComponent.teamIndex}");
        //    }
        //    return thing;
        //}
        internal int _conga, _conga_demo, _conga_engi, _conga_heavy, _conga_medic, _conga_pyro, _conga_scout, _conga_sniper, _conga_soldier, _conga_spy, _demo_flip_flip, _demo_flip_throw, _demo_flip_waiting, _demo_laugh, _engi_flip_flip, _engi_flip_throw, _engi_flip_waiting, _engi_laugh, _heavy_flip_flip, _heavy_flip_throw, _heavy_flip_waiting, _heavy_laugh, _kazotsky, _medic_flip_flip, _medic_flip_throw, _medic_flip_waiting, _medic_laugh, _play_rancho, _play_ranchoburp, _play_ranchoclose, _play_rancholong, _play_ranchoquick, _pyro_flip_flip, _pyro_flip_throw, _pyro_flip_waiting, _pyro_laugh, _rps_demo_initiate, _rps_demo_loss, _rps_demo_winpaper, _rps_demo_winrock, _rps_demo_winscissors, _rps_engi_initiate, _rps_engi_loss, _rps_engi_winpaper, _rps_engi_winrock, _rps_engi_winscissors, _rps_heavy_initiate, _rps_heavy_loss, _rps_heavy_winpaper, _rps_heavy_winrock, _rps_heavy_winscissors, _rps_medic_initiate, _rps_medic_loss, _rps_medic_winpaper, _rps_medic_winrock, _rps_medic_winscissors, _rps_pyro_initiate, _rps_pyro_loss, _rps_pyro_winpaper, _rps_pyro_winrock, _rps_pyro_winscissors, _rps_scout_initiate, _rps_scout_loss, _rps_scout_lossrock, _rps_scout_winpaper, _rps_scout_winrock, _rps_scout_winscissors, _rps_sniper_loss, _rps_sniper_winpaper, _rps_sniper_winrock, _rps_sniper_winscissors, _rps_sniper_initiate, _rps_soldier_initiate, _rps_soldier_loss, _rps_soldier_winpaper, _rps_soldier_winrock, _rps_soldier_winscissors, _rps_spy_initiate, _rps_spy_losspaper, _rps_spy_lossrock, _rps_spy_lossscissors, _rps_spy_winpaper, _rps_spy_winrock, _rps_spy_winscissors, _scout_flip_flip, _scout_flip_throw, _scout_flip_waiting, _sniper_flip_flip, _sniper_flip_throw, _sniper_flip_waiting, _soldier_flip_flip, _soldier_flip_throw, _soldier_flip_waiting, _spy_flip_flip, _spy_flip_throw, _spy_flip_waiting, _spy_laugh;
        public void RegisterAllSounds()
        {
            //            _conga_demo = RegiserSound([[""]], [0]);

            //todo come back to conga and kazotsky, probably just play these on the character themselves?
            //todo come back to rancho
            //_conga = RegiserSound([["conga"]], [0]);
            //_conga_demo = RegiserSound([[""]], [0]);


            ///////////////////////////////Flip
            _demo_flip_flip = RegisterSound([["taunt_demo_flip_fun_01", "taunt_demo_flip_fun_03"], ["taunt_demo_flip_post_fun_03", "taunt_demo_flip_post_fun_01", "taunt_demo_flip_post_fun_04", "taunt_demo_flip_post_fun_05", "taunt_demo_admire_06", "Taunt_demo_flip_post_fun_point_03", "Taunt_demo_flip_post_fun_point_04", "Taunt_demo_flip_post_fun_point_06"]], [1.2f, 2.2f], -1);
            _demo_flip_throw = RegisterSound([["Taunt_demo_flip_exert_03", "Taunt_demo_flip_neg_01", "Taunt_demo_flip_neg_02"]], [.6f], -1);
            _demo_flip_waiting = RegisterSound([["Taunt_demo_flip_int_05", "Taunt_demo_flip_int_06", "Taunt_demo_flip_int_08", "Taunt_demo_flip_int_10", "Taunt_demo_flip_int_12", "Taunt_demo_flip_int_13", "Taunt_demo_flip_int_14", "Taunt_demo_flip_int_16", "Taunt_demo_flip_int_18", "Taunt_demo_flip_int_20", "Taunt_demo_int_01", "Taunt_demo_int_03", "Taunt_demo_int_04", "Taunt_demo_int_05", "Taunt_demo_int_10", "Taunt_demo_int_19", "Taunt_demo_int_21", "Taunt_demo_int_27", "Taunt_demo_int_30", "Taunt_demo_int_34"]], [0f], 3f);

            _engi_flip_flip = RegisterSound([["Eng_taunt_exert_05", "Eng_taunt_exert_07", "Eng_taunt_exert_10", "Eng_taunt_exert_12", "Eng_taunt_exert_15", "Eng_taunt_exert_19", "Eng_taunt_exert_24", "Eng_taunt_exert_29", "Eng_taunt_flip_fun_01", "Eng_taunt_flip_fun_06", "Eng_taunt_flip_fun_07", "Eng_taunt_flip_fun_25"], ["Eng_taunt_flip_admire_01", "Eng_taunt_flip_admire_02", "Eng_taunt_flip_admire_03", "Eng_taunt_flip_admire_04", "Eng_taunt_flip_admire_06", "Eng_taunt_flip_admire_07", "Eng_taunt_flip_admire_09", "Eng_taunt_flip_admire_10", "Eng_taunt_flip_admire_11", "Eng_taunt_flip_admire_12", "Eng_taunt_flip_admire_14", "Eng_taunt_flip_admire_15", "Eng_taunt_flip_end_01", "Eng_taunt_flip_end_04", "Eng_taunt_flip_end_08"]], [1.2f, 2.2f], -1);
            _engi_flip_throw = RegisterSound([["Eng_taunt_exert_08", "Eng_taunt_exert_30", "Eng_taunt_exert_44", "Eng_taunt_exert_46", "Eng_taunt_exert_47", "Eng_taunt_flip_exert_14", "Eng_taunt_flip_exert_23", "Eng_taunt_flip_exert_24", "Eng_taunt_flip_exert_26"]], [.6f], -1);
            _engi_flip_waiting = RegisterSound([["Eng_taunt_flip_int_01", "Eng_taunt_flip_int_04", "Eng_taunt_flip_int_08", "Eng_taunt_flip_int_11", "Eng_taunt_flip_int_13", "Eng_taunt_flip_int_14"]], [0f], 3);

            _heavy_flip_flip = RegisterSound([["Heavy_taunt_flip_fail_01", "Heavy_taunt_flip_fail_08"], ["Heavy_taunt_flip_end_01", "Heavy_taunt_flip_end_02", "Heavy_taunt_flip_end_03"]], [1.2f, 2.2f], -1);
            _heavy_flip_throw = RegisterSound([["Heavy_taunt_exert_01", "Heavy_taunt_exert_04", "Heavy_taunt_exert_06", "Heavy_taunt_exert_09", "Heavy_taunt_exert_11", "Heavy_taunt_exert_12", "Heavy_taunt_exert_13", "Heavy_taunt_flip_exert_01", "Heavy_taunt_flip_exert_09"]], [.6f], -1);
            _heavy_flip_waiting = RegisterSound([["Heavy_taunt_flip_int_01", "Heavy_taunt_flip_int_02", "Heavy_taunt_flip_int_04", "Heavy_taunt_flip_int_05", "Heavy_taunt_flip_int_10", "Heavy_taunt_flip_int_11", "Heavy_taunt_flip_int_12", "Heavy_taunt_flip_int_13", "Heavy_taunt_flip_int_16"]], [0f], 3);

            _medic_flip_flip = RegisterSound([["Medic_taunt_admire_01", "Medic_taunt_admire_02", "Medic_taunt_admire_03", "Medic_taunt_admire_07", "Medic_taunt_admire_10", "Medic_taunt_admire_13", "Medic_taunt_admire_14", "Medic_taunt_admire_22", "Medic_taunt_flip_end_01", "Medic_taunt_flip_end_02", "Medic_taunt_flip_end_05", "Medic_taunt_flip_end_06", "Medic_taunt_flip_end_08", "Medic_taunt_flip_end_09"]], [2.6f], 0);
            _medic_flip_throw = RegisterSound([["Medic_taunt_exert_01", "Medic_taunt_exert_02", "Medic_taunt_exert_08", "Medic_taunt_exert_09", "Medic_taunt_flip_exert_01", "Medic_taunt_flip_exert_03", "Medic_taunt_flip_exert_04", "Medic_taunt_flip_exert_05", "Medic_taunt_flip_exert_06", "Medic_taunt_flip_exert_07", "Medic_taunt_flip_exert_08", "Medic_taunt_flip_exert_09", "Medic_taunt_flip_exert_10", ""]], [.6f], 0);
            _medic_flip_waiting = RegisterSound([["Medic_taunt_flip_int_05", "Medic_taunt_flip_int_08", "Medic_taunt_flip_int_10", "Medic_taunt_flip_int_12", "Medic_taunt_flip_int_15"]], [0], 3);

            _pyro_flip_flip = RegisterSound([["Pyro_taunt_flip_fun_01", "Pyro_taunt_flip_fun_04", "Pyro_taunt_flip_fun_05", "Pyro_taunt_flip_fun_06", "Pyro_taunt_flip_fun_09", "Pyro_taunt_flip_fun_10", "Pyro_taunt_flip_fun_11"], ["Pyro_taunt_flip_admire_01", "Pyro_taunt_flip_admire_02", "Pyro_taunt_flip_admire_03", "Pyro_taunt_flip_admire_05", "Pyro_taunt_flip_admire_06", "Pyro_taunt_thanks_07", "Pyro_taunt_thanks_08", "Pyro_taunt_thanks_09", ""]], [1.2f, 2.2f], 0);
            _pyro_flip_throw = RegisterSound([["Pyro_taunt_exert_12", "Pyro_taunt_flip_exert_02", "Pyro_taunt_flip_exert_04", "Pyro_taunt_flip_exert_05", "Pyro_taunt_flip_exert_06"]], [.6f], 0);
            _pyro_flip_waiting = RegisterSound([["Pyro_taunt_flip_int_02", "Pyro_taunt_flip_int_05", "Pyro_taunt_flip_int_07"]], [0], 3);

            _scout_flip_flip = RegisterSound([["Scout_taunt_flip_fun_01", "Scout_taunt_flip_fun_02", "Scout_taunt_flip_fun_03", "Scout_taunt_flip_fun_05", "Scout_taunt_flip_fun_06", "Scout_taunt_flip_fun_08", "Scout_taunt_flip_fun_09", "Scout_taunt_flip_fun_10"], ["Scout_taunt_flip_end_01", "Scout_taunt_flip_end_03", "Scout_taunt_flip_end_05", "Scout_taunt_flip_end_07", "Scout_taunt_flip_end_08", "Scout_taunt_flip_end_17", "Scout_taunt_flip_end_19", "Scout_taunt_flip_end_22", "Scout_taunt_flip_end_27"]], [1.2f, 2.2f], 0);
            _scout_flip_throw = RegisterSound([["Scout_taunt_exert_05", "Scout_taunt_exert_13", "Scout_taunt_exert_21", "Scout_taunt_exert_23", "Scout_taunt_exert_30", "Scout_taunt_flip_exert_01", "Scout_taunt_flip_exert_05", "Scout_taunt_flip_exert_08", "Scout_taunt_flip_exert_09", "Scout_taunt_flip_exert_10", "Scout_taunt_flip_exert_13"]], [.6f], 0);
            _scout_flip_waiting = RegisterSound([["Scout_taunt_flip_int_03", "Scout_taunt_flip_int_06", "Scout_taunt_flip_int_07", "Scout_taunt_flip_int_10", "Scout_taunt_flip_int_12", "Scout_taunt_flip_int_13", "Scout_taunt_int_01", "Scout_taunt_int_03", "Scout_taunt_int_05", "Scout_taunt_int_06", "Scout_taunt_int_07", "Scout_taunt_int_08", "Scout_taunt_int_12", "Scout_taunt_int_14", "Scout_taunt_int_17", "Scout_taunt_int_18"]], [0], 3);

            _sniper_flip_flip = RegisterSound([["Sniper_taunt_admire_01", "Sniper_taunt_admire_02", "Sniper_taunt_admire_06", "Sniper_taunt_admire_09", "Sniper_taunt_admire_11", "Sniper_taunt_admire_12", "Sniper_taunt_admire_15", "Sniper_taunt_admire_16", "Sniper_taunt_admire_18", "Sniper_taunt_admire_19", "Sniper_taunt_admire_20", "Sniper_taunt_flip_end_02", "Sniper_taunt_flip_end_03", "Sniper_taunt_flip_end_04", "Sniper_taunt_flip_end_06", "Sniper_taunt_flip_end_07", "Sniper_taunt_flip_fun_05", "Sniper_taunt_flip_fun_06"]], [2.2f], 0);
            _sniper_flip_throw = RegisterSound([["Sniper_taunt_exert_03", "Sniper_taunt_exert_07", "Sniper_taunt_exert_010", "Sniper_taunt_exert_15", "Sniper_taunt_flip_exert_01", "Sniper_taunt_flip_exert_04", "Sniper_taunt_flip_exert_05", "Sniper_taunt_flip_exert_06", "Sniper_taunt_flip_exert_07"]], [.6f], 0);
            _sniper_flip_waiting = RegisterSound([["Sniper_taunt_flip_int_04", "Sniper_taunt_flip_int_06", "Sniper_taunt_flip_int_07", "Sniper_taunt_flip_int_10", "Sniper_taunt_flip_int_11", "Sniper_taunt_int_01", "Sniper_taunt_int_13"]], [0], 3);

            _soldier_flip_flip = RegisterSound([["Soldier_taunt_flip_fun_04", "Soldier_taunt_flip_fun_06", "Soldier_taunt_flip_fun_08"], ["Soldier_taunt_admire_01", "Soldier_taunt_admire_04", "Soldier_taunt_admire_09", "Soldier_taunt_admire_10", "Soldier_taunt_admire_16", "Soldier_taunt_admire_17", "Soldier_taunt_admire_18", "Soldier_taunt_admire_22", "Soldier_taunt_admire_24", "Soldier_taunt_admire_26", "Soldier_taunt_flip_end_01", "Soldier_taunt_flip_end_02", "Soldier_taunt_flip_end_03", "Soldier_taunt_flip_end_05", "Soldier_taunt_flip_end_15", "Soldier_taunt_flip_end_16", "Soldier_taunt_flip_end_17"]], [1.2f, 2.2f], 0);
            _soldier_flip_throw = RegisterSound([["Soldier_taunt_exert_02", "Soldier_taunt_exert_06", "Soldier_taunt_flip_exert_02", "Soldier_taunt_flip_exert_06", "Soldier_taunt_flip_exert_21", "Soldier_taunt_flip_exert_31"]], [.6f], 0);
            _soldier_flip_waiting = RegisterSound([["Soldier_taunt_flip_int_01", "Soldier_taunt_flip_int_03", "Soldier_taunt_flip_int_04", "Soldier_taunt_flip_int_11", "Soldier_taunt_flip_int_15", "Soldier_taunt_flip_int_17", "Soldier_taunt_flip_int_19", "Soldier_taunt_flip_int_20", "Soldier_taunt_flip_int_24"]], [0], 3);

            _spy_flip_flip = RegisterSound([["Spy_taunt_flip_fun_01", "Spy_taunt_flip_fun_02", "Spy_taunt_flip_fun_07", "Spy_taunt_flip_fun_09", "Spy_taunt_flip_fun_12", "Spy_taunt_flip_fun_13"], ["Spy_taunt_bos_int_05", "Spy_taunt_bos_kick_02", "Spy_taunt_flip_admire_05", "Spy_taunt_flip_admire_09", "Spy_taunt_flip_admire_18", "Spy_taunt_flip_admire_20", "Spy_taunt_flip_end_07", "Spy_taunt_flip_end_12", "Spy_taunt_flip_end_14", "Spy_taunt_flip_end_16"]], [1.2f, 2.2f], 0);
            _spy_flip_throw = RegisterSound([["Spy_taunt_flip_exert_01", "Spy_taunt_flip_exert_02", "Spy_taunt_flip_exert_07", "Spy_taunt_flip_exert_08", "Spy_taunt_flip_exert_09", "Spy_taunt_flip_exert_10"]], [.6f], 0);
            _spy_flip_waiting = RegisterSound([["Spy_taunt_flip_int_01", "Spy_taunt_flip_int_02", "Spy_taunt_flip_int_03", "Spy_taunt_flip_int_04", "Spy_taunt_flip_int_07", "Spy_taunt_flip_int_15", "Spy_taunt_flip_int_16", "Spy_taunt_flip_int_20", "Spy_taunt_flip_int_25", "Spy_taunt_flip_int_28"]], [0], 3);



            ///////////////////////////RPS
            _rps_demo_initiate = RegisterSound([["Taunt_demo_rps_int_01", "Taunt_demo_rps_int_06"]], [0], 5);
            _rps_demo_loss = RegisterSound([["Taunt_demo_rps_lose_04", "Taunt_demo_rps_lose_06", "Taunt_demo_rps_lose_08", "Taunt_demo_rps_lose_09", "Taunt_demo_rps_lose_10"], ["Taunt_demo_rps_exert_04"]], [5.5f, 1.7f], 0);
            _rps_demo_winpaper = RegisterSound([["Taunt_demo_rps_win_03", "Taunt_demo_rps_win_04", "Taunt_demo_rps_win_06", "Taunt_demo_rps_win_08", "Taunt_demo_rps_win_16", "Taunt_demo_rps_win_24", "Taunt_demo_rps_win_25", "Taunt_demo_rps_win_27", "Taunt_demo_rps_win_28"], ["Taunt_demo_rps_exert_04"]], [3.5f, 1.7f], 0);
            _rps_demo_winrock = RegisterSound([["Taunt_demo_rps_win_03", "Taunt_demo_rps_win_04", "Taunt_demo_rps_win_06", "Taunt_demo_rps_win_08", "Taunt_demo_rps_win_16", "Taunt_demo_rps_win_24", "Taunt_demo_rps_win_25", "Taunt_demo_rps_win_27", "Taunt_demo_rps_win_28"], ["Taunt_demo_rps_exert_04"]], [3.5f, 1.7f], 0);
            _rps_demo_winscissors = RegisterSound([["Taunt_demo_rps_win_03", "Taunt_demo_rps_win_04", "Taunt_demo_rps_win_06", "Taunt_demo_rps_win_08", "Taunt_demo_rps_win_16", "Taunt_demo_rps_win_24", "Taunt_demo_rps_win_25", "Taunt_demo_rps_win_27", "Taunt_demo_rps_win_28"], ["Taunt_demo_rps_exert_04"]], [3.5f, 1.7f], 0);

            _rps_engi_initiate = RegisterSound([["Eng_taunt_rps_int_01", "Eng_taunt_rps_int_03", "Eng_taunt_rps_int_07"]], [0], 5);
            _rps_engi_loss = RegisterSound([["Eng_taunt_rps_exert_07 (1)"], ["taunt_hard_clap1"], ["taunt_hard_clap1"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Eng_taunt_rps_exert_01"], ["Eng_taunt_rps_exert_02"], ["Eng_taunt_rps_exert_03"], ["Eng_taunt_rps_lose_22", "Eng_taunt_rps_lose_25", "Eng_taunt_rps_lose_27", "Eng_taunt_rps_lose_29", "Eng_taunt_rps_lose_31"]], [0, .5f, .7f, 1.7f, 1.928f, 2.156f, 1.7f, 2.159f, 2.63f, 5.5f], 0);
            _rps_engi_winpaper = RegisterSound([["Eng_taunt_rps_exert_07 (1)"], ["taunt_hard_clap1"], ["taunt_hard_clap1"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Eng_taunt_rps_exert_01"], ["Eng_taunt_rps_exert_02"], ["Eng_taunt_rps_exert_03"], ["Eng_taunt_rps_win_07", "Eng_taunt_rps_win_17", "Eng_taunt_rps_win_26", "Eng_taunt_rps_win_31", "Eng_taunt_rps_win_33"]], [0, .5f, .7f, 1.7f, 1.928f, 2.156f, 1.7f, 2.159f, 2.63f, 3.5f], 0);
            _rps_engi_winrock = RegisterSound([["Eng_taunt_rps_exert_07 (1)"], ["taunt_hard_clap1"], ["taunt_hard_clap1"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Eng_taunt_rps_exert_01"], ["Eng_taunt_rps_exert_02"], ["Eng_taunt_rps_exert_03"], ["Eng_taunt_rps_win_10", "Eng_taunt_rps_win_17", "Eng_taunt_rps_win_26", "Eng_taunt_rps_win_31", "Eng_taunt_rps_win_33"]], [0, .5f, .7f, 1.7f, 1.928f, 2.156f, 1.7f, 2.159f, 2.63f, 3.5f], 0);
            _rps_engi_winscissors = RegisterSound([["Eng_taunt_rps_exert_07 (1)"], ["taunt_hard_clap1"], ["taunt_hard_clap1"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Eng_taunt_rps_exert_01"], ["Eng_taunt_rps_exert_02"], ["Eng_taunt_rps_exert_03"], ["Eng_taunt_rps_win_06", "Eng_taunt_rps_win_17", "Eng_taunt_rps_win_26", "Eng_taunt_rps_win_31", "Eng_taunt_rps_win_33"]], [0, .5f, .7f, 1.7f, 1.928f, 2.156f, 1.7f, 2.159f, 2.63f, 3.5f], 0);

            _rps_heavy_initiate = RegisterSound([["Heavy_taunt_rps_int_01", "Heavy_taunt_rps_int_02", "Heavy_taunt_rps_int_04"]], [0], 5);
            _rps_heavy_loss = RegisterSound([["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["Heavy_taunt_rps_lose_11", "Heavy_taunt_rps_lose_13", "Heavy_taunt_rps_lose_18"]], [1.7f, 2.2f, 2.7f, 5.5f], 0);
            _rps_heavy_winpaper = RegisterSound([["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["Heavy_taunt_kill_02", "Heavy_taunt_kill_09", "Heavy_taunt_rps_win_02", "Heavy_taunt_rps_win_03", "Heavy_taunt_rps_win_04", "Heavy_taunt_rps_win_09", "Heavy_taunt_rps_win_11", "Heavy_taunt_rps_win_12", "Heavy_taunt_rps_win_21", "Heavy_taunt_rps_win_27", "Heavy_taunt_rps_win_34"]], [1.7f, 2.2f, 2.7f, 3.5f], 0);
            _rps_heavy_winrock = RegisterSound([["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["Heavy_taunt_kill_02", "Heavy_taunt_kill_09", "Heavy_taunt_rps_int_05", "Heavy_taunt_rps_win_02", "Heavy_taunt_rps_win_03", "Heavy_taunt_rps_win_04", "Heavy_taunt_rps_win_09", "Heavy_taunt_rps_win_11", "Heavy_taunt_rps_win_12", "Heavy_taunt_rps_win_16", "Heavy_taunt_rps_win_27", "Heavy_taunt_rps_win_33"]], [1.7f, 2.2f, 2.7f, 3.5f], 0);
            _rps_heavy_winscissors = RegisterSound([["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["Heavy_taunt_kill_02", "Heavy_taunt_kill_09", "Heavy_taunt_rps_win_02", "Heavy_taunt_rps_win_03", "Heavy_taunt_rps_win_04", "Heavy_taunt_rps_win_09", "Heavy_taunt_rps_win_11", "Heavy_taunt_rps_win_12", "Heavy_taunt_rps_win_21", "Heavy_taunt_rps_win_27", "Heavy_taunt_rps_win_38"]], [1.7f, 2.2f, 2.7f, 3.5f], 0);





















































        }
        internal int RegisterSound(string[][] audioClipNames, List<float> delays, float repeatTimer)
        {
            return AudioContainerHolder.Setup(audioClipNames, delays, repeatTimer);
        }
        public void Rancho()
        {
            AddAnimation("Engi/Rancho/RanchoRelaxo", null, "Engi/Rancho/engiRanchoPassive", false, false);
            AddAnimation("Engi/Rancho/engiRanchoBurp", "", "Engi/Rancho/engiRanchoPassive", false, false, false);
            AddAnimation("Engi/Rancho/engiRanchoBigDrink", "", "Engi/Rancho/engiRanchoPassive", false, false, false);
            AddAnimation("Engi/Rancho/engiRanchoQuickDrink", "", "Engi/Rancho/engiRanchoPassive", false, false, false);
        }
        public void Laugh()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Schadenfreude");
            string emote = AddHiddenAnimation(new string[] { "Engi/Laugh/Engi_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Engineer_laughlong02.ogg")]);
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/Laugh/Scout_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Scout_laughlong02.ogg")]);
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Demo/Laugh/Demo_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Demoman_laughlong02.ogg")]);
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/Laugh/Soldier_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Soldier_laughlong03.ogg")]);
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/Laugh/Pyro_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Pyro_laugh_addl04.ogg")]);
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/Laugh/Heavy_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Heavy_laugherbigsnort01.ogg")]);
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/Laugh/Medic_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Medic_laughlong01.ogg")]);
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/Laugh/Sniper_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Sniper_laughlong02.ogg")]);
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/Laugh/Spy_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Spy_laughlong01.ogg")]);
            Laugh_Emotes.Add(emote);

        }
        public void Flip()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Flippin' Awesome");


            string emote = AddHiddenAnimation(new string[] { "Demo/Flip/Demo_Flip_Start" }, new string[] { "Demo/Flip/Demo_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Demo/Flip/Demo_Flip_Throw" });
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Demo/Flip/Demo_Flip_Flip" });
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Engi/Flip/Engi_Flip_Start" }, new string[] { "Engi/Flip/Engi_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Engi/Flip/Engi_Flip_Throw" });
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Engi/Flip/Engi_Flip_Flip" });
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Medic/Flip/Medic_Flip_Start" }, new string[] { "Medic/Flip/Medic_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/Flip/Medic_Flip_Throw" });
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Medic/Flip/Medic_Flip_Flip" });
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Heavy/Flip/Heavy_Flip_Start" }, new string[] { "Heavy/Flip/Heavy_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/Flip/Heavy_Flip_Throw" });
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Heavy/Flip/Heavy_Flip_Flip" });
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Pyro/Flip/Pyro_Flip_Start" }, new string[] { "Pyro/Flip/Pyro_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/Flip/Pyro_Flip_Throw" });
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Pyro/Flip/Pyro_Flip_Flip" });
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Scout/Flip/Scout_Flip_Start" }, new string[] { "Scout/Flip/Scout_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/Flip/Scout_Flip_Throw" });
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Scout/Flip/Scout_Flip_Flip" });
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Sniper/Flip/Sniper_Flip_Start" }, new string[] { "Sniper/Flip/Sniper_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/Flip/Sniper_Flip_Throw" });
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Sniper/Flip/Sniper_Flip_Flip" });
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Soldier/Flip/Soldier_Flip_Start" }, new string[] { "Soldier/Flip/Soldier_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/Flip/Soldier_Flip_Throw" });
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Soldier/Flip/Soldier_Flip_Flip" });
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Spy/Flip/Spy_Flip_Start" }, new string[] { "Spy/Flip/Spy_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/Flip/Spy_Flip_Throw" });
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Spy/Flip/Spy_Flip_Flip" });
            Flip_Flip_Emotes.Add(emote);
        }
        public void RPS()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Rock Paper Scissors");
            CustomEmotesAPI.BlackListEmote("Rock Paper Scissors");
            //CustomEmotesAPI.AddNonAnimatingEmote("Rock", false);
            //CustomEmotesAPI.AddNonAnimatingEmote("Paper", false);
            //CustomEmotesAPI.AddNonAnimatingEmote("Scissors", false);
            string emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPSStart" }, new string[] { "Engi/RPS/EngiRPSLoop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_RWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_RLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_PWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_PLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_SWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_SLose" });
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_Start" }, new string[] { "Demo/RPS/DemoRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_RWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_RLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_PWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_PLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_SWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_SLose" });
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_Start" }, new string[] { "Soldier/RPS/SoldierRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_RWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_RLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_PWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_PLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_SWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_SLose" });
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_Start" }, new string[] { "Heavy/RPS/HeavyRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_RWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_RLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_PWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_PLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_SWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_SLose" });
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_Start" }, new string[] { "Medic/RPS/MedicRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_RWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_RLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_PWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_PLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_SWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_SLose" });
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_Start" }, new string[] { "Pyro/RPS/PyroRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_RWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_RLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_PWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_PLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_SWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_SLose" });
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_Start" }, new string[] { "Scout/RPS/ScoutRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_RWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_RLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_PWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_PLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_SWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_SLose" });
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_Start" }, new string[] { "Sniper/RPS/SniperRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_RWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_RLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_PWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_PLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_SWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_SLose" });
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_Start" }, new string[] { "Spy/RPS/SpyRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_RWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_RLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_PWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_PLose" });
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_SWin" });
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_SLose" });
            RPS_Loss_Emotes.Add(emote);
        }
        public void KazotskyKick()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Kazotsky Kick");
            CustomEmotesAPI.BlackListEmote("Kazotsky Kick");
            string emote;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Demo_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Demo_Loop" }); //names are wrong, should be Kazotsky_Sniper_Loop
            KazotskyKick_Emotes.Add(emote);
            int syncpos = BoneMapper.animClips[emote].syncPos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Engi_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Engi_Loop" });
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Heavy_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Heavy_Loop" });
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Medic_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Medic_Loop" });
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Pyro_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Pyro_Loop" });
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Scout_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Scout_Loop" });
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Sniper_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Sniper_Loop" });
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Soldier_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Soldier_Loop" });
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Spy_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Spy_Loop" });
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
        }
        public void Conga()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Conga");
            CustomEmotesAPI.BlackListEmote("Conga");

            string emote;
            emote = AddHiddenAnimation(new string[] { "Conga/Demo_Conga" }, new string[] { "Conga" }, "Conga", true);
            Conga_Emotes.Add(emote);
            int syncpos = BoneMapper.animClips[emote].syncPos;
            emote = AddHiddenAnimation(new string[] { "Conga/Engi_Conga" }, new string[] { "Conga" }, "Conga", true);
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Heavy_Conga" }, new string[] { "Conga" }, "Conga", true);
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Medic_Conga" }, new string[] { "Conga" }, "Conga", true);
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Pyro_Conga" }, new string[] { "Conga" }, "Conga", true);
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Scout_Conga" }, new string[] { "Conga" }, "Conga", true);
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Sniper_Conga" }, new string[] { "Conga" }, "Conga", true);
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Soldier_Conga" }, new string[] { "Conga" }, "Conga", true);
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Spy_Conga" }, new string[] { "Conga" }, "Conga", true);
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
        }

        private void CustomEmotesAPI_emoteSpotJoined_Body(GameObject emoteSpot, BoneMapper joiner, BoneMapper host)
        {
            string emoteSpotName = emoteSpot.name;
            if (emoteSpotName == "RPSJoinSpot")
            {
                if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                {
                    int winner = UnityEngine.Random.Range(0, 2);
                    int hostSpot = UnityEngine.Random.Range(0, 3);
                    int clientSpot;
                    if (winner == 0)
                    {
                        clientSpot = hostSpot - 1;
                    }
                    else
                    {
                        clientSpot = hostSpot + 1;
                    }
                    if (clientSpot > 2)
                    {
                        clientSpot -= 3;
                    }
                    if (clientSpot < 0)
                    {
                        clientSpot += 3;
                    }

                    hostSpot += host.props[0].GetComponent<RockPaperScissors>().charType * 3;
                    clientSpot += UnityEngine.Random.Range(0, RPS_Start_Emotes.Count) * 3;

                    if (winner == 0)
                    {
                        TF2Networker.instance.SyncEmoteToServerRpc(host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Win", hostSpot, host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                        TF2Networker.instance.SyncEmoteToServerRpc(joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Loss", clientSpot, host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    else
                    {
                        TF2Networker.instance.SyncEmoteToServerRpc(host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Loss", hostSpot, host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                        TF2Networker.instance.SyncEmoteToServerRpc(joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Win", clientSpot, host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                }
            }
            else if (emoteSpotName == "FlipJoinSpot")
            {
                if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                {
                    int hostSpot = host.props[0].GetComponent<Flip>().charType;
                    int clientSpot = UnityEngine.Random.Range(0, Flip_Flip_Emotes.Count);

                    TF2Networker.instance.SyncEmoteToServerRpc(host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Flip_Throw", hostSpot, host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    TF2Networker.instance.SyncEmoteToServerRpc(joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Flip_Flip", clientSpot, host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
        }
        private void StopAudioContainerStuff(BoneMapper mapper)
        {
            foreach (var item in AudioContainerHolder.instance.currentContainers)
            {
                if (item.mapper == mapper)
                {
                    foreach (var routine in item.coroutines)
                    {
                        if (routine is not null)
                        {
                            AudioContainerHolder.instance.StopCoroutine(routine);
                        }
                    }
                    AudioContainerHolder.instance.currentContainers.Remove(item);
                    return;
                }
            }
        }
        private void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            DebugClass.Log($"newanimation is {newAnimation}");

            if (!mapper.gameObject.GetComponent<TF2EmoteTracker>())
            {
                mapper.gameObject.AddComponent<TF2EmoteTracker>();
            }
            int targetAudioThing = -1;
            StopAudioContainerStuff(mapper);
            switch (newAnimation)
            {
                case "RanchoRelaxo":
                    GameObject g = GameObject.Instantiate(Assets.Load<GameObject>("@BadAssEmotes_badassemotes:Assets/Engi/Rancho/RanchoRelaxo.prefab"));
                    mapper.props.Add(g);
                    g.transform.SetParent(mapper.transform.parent);
                    g.transform.localEulerAngles = new Vector3(90, 0, 0);
                    g.transform.localPosition = Vector3.zero;
                    g.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                    mapper.ScaleProps();
                    mapper.props.RemoveAt(mapper.props.Count - 1);
                    ChairHandler chair = g.AddComponent<ChairHandler>();
                    chair.pos = g.transform.position;
                    chair.rot = g.transform.eulerAngles;
                    chair.scal = g.transform.lossyScale;
                    chair.mapper = mapper;

                    g = new GameObject();
                    g.name = "RanchoRelaxoProp";
                    mapper.props.Add(g);
                    g.transform.localPosition = mapper.transform.position;
                    g.transform.localEulerAngles = mapper.transform.eulerAngles;
                    g.transform.localScale = Vector3.one;
                    mapper.AssignParentGameObject(g, true, true, true, false, false);
                    chair.chair = g;
                    break;
                case "Rock Paper Scissors":
                    if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                    {
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Start", UnityEngine.Random.Range(0, RPS_Start_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    break;
                case "Flippin' Awesome":
                    if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                    {
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Flip_Wait", UnityEngine.Random.Range(0, Flip_Wait_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    break;
                case "Conga":
                    if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                    {
                        mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = "Medic_Conga";
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Conga_Start", UnityEngine.Random.Range(0, Conga_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    break;
                case "Kazotsky Kick":
                    if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                    {
                        mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = "Medic_Kazotsky";
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Kazotsky_Start", UnityEngine.Random.Range(0, KazotskyKick_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    break;
                case "Schadenfreude":
                    if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                    {
                        mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = "Medic_Laugh";
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Laugh_Start", UnityEngine.Random.Range(0, Laugh_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    break;

                    //Fleeeeep
                case "Demo_Flip_Start":
                    targetAudioThing = _demo_flip_waiting;
                    break;
                case "Demo_Flip_Throw":
                    targetAudioThing = _demo_flip_throw;
                    break;
                case "Demo_Flip_Flip":
                    targetAudioThing = _demo_flip_flip;
                    break;


                case "Engi_Flip_Start":
                    targetAudioThing = _engi_flip_waiting;
                    break;
                case "Engi_Flip_Throw":
                    targetAudioThing = _engi_flip_throw;
                    break;
                case "Engi_Flip_Flip":
                    targetAudioThing = _engi_flip_flip;
                    break;


                case "Heavy_Flip_Start":
                    targetAudioThing =  _heavy_flip_waiting;
                    break;
                case "Heavy_Flip_Throw":
                    targetAudioThing =  _heavy_flip_throw;
                    break;
                case "Heavy_Flip_Flip":
                    targetAudioThing =  _heavy_flip_flip;
                    break;


                case "Medic_Flip_Start":
                    targetAudioThing =  _medic_flip_waiting;
                    break;
                case "Medic_Flip_Throw":
                    targetAudioThing =  _medic_flip_throw;
                    break;
                case "Medic_Flip_Flip":
                    targetAudioThing =  _medic_flip_flip;
                    break;


                case "Pyro_Flip_Start":
                    targetAudioThing =  _pyro_flip_waiting;
                    break;
                case "Pyro_Flip_Throw":
                    targetAudioThing =  _pyro_flip_throw;
                    break;
                case "Pyro_Flip_Flip":
                    targetAudioThing =  _pyro_flip_flip;
                    break;


                case "Scout_Flip_Start":
                    targetAudioThing =  _scout_flip_waiting;
                    break;
                case "Scout_Flip_Throw":
                    targetAudioThing =  _scout_flip_throw;
                    break;
                case "Scout_Flip_Flip":
                    targetAudioThing =  _scout_flip_flip;
                    break;


                case "Sniper_Flip_Start":
                    targetAudioThing =  _sniper_flip_waiting;
                    break;
                case "Sniper_Flip_Throw":
                    targetAudioThing =  _sniper_flip_throw;
                    break;
                case "Sniper_Flip_Flip":
                    targetAudioThing =  _sniper_flip_flip;
                    break;


                case "Soldier_Flip_Start":
                    targetAudioThing =  _soldier_flip_waiting;
                    break;
                case "Soldier_Flip_Throw":
                    targetAudioThing =  _soldier_flip_throw;
                    break;
                case "Soldier_Flip_Flip":
                    targetAudioThing =  _soldier_flip_flip;
                    break;


                case "Spy_Flip_Start":
                    targetAudioThing =  _spy_flip_waiting;
                    break;
                case "Spy_Flip_Throw":
                    targetAudioThing =  _spy_flip_throw;
                    break;
                case "Spy_Flip_Flip":
                    targetAudioThing =  _spy_flip_flip;
                    break;





                    //RPS
                case "EngiRPSStart":
                    targetAudioThing =  _rps_engi_initiate;
                    break;
                case "EngiRPS_RLose":
                case "EngiRPS_PLose":
                case "EngiRPS_SLose":
                    targetAudioThing =  _rps_engi_loss;
                    break;
                case "EngiRPS_RWin":
                    targetAudioThing =  _rps_engi_winpaper;
                    break;
                case "EngiRPS_PWin":
                    targetAudioThing = _rps_engi_winrock;
                    break;
                case "EngiRPS_SWin":
                    targetAudioThing = _rps_engi_winscissors;
                    break;


                case "DemoRPS_Start":
                    targetAudioThing = _rps_demo_initiate;
                    break;
                case "DemoRPS_RLose":
                case "DemoRPS_PLose":
                case "DemoRPS_SLose":
                    targetAudioThing = _rps_demo_loss;
                    break;
                case "DemoRPS_RWin":
                    targetAudioThing = _rps_demo_winpaper;
                    break;
                case "DemoRPS_PWin":
                    targetAudioThing = _rps_demo_winrock;
                    break;
                case "DemoRPS_SWin":
                    targetAudioThing = _rps_demo_winscissors;
                    break;


                case "HeavyRPS_Start":
                    targetAudioThing = _rps_heavy_initiate;
                    break;
                case "HeavyRPS_RLose":
                case "HeavyRPS_PLose":
                case "HeavyRPS_SLose":
                    targetAudioThing = _rps_heavy_loss;
                    break;
                case "HeavyRPS_RWin":
                    targetAudioThing = _rps_heavy_winpaper;
                    break;
                case "HeavyRPS_PWin":
                    targetAudioThing = _rps_heavy_winrock;
                    break;
                case "HeavyRPS_SWin":
                    targetAudioThing = _rps_heavy_winscissors;
                    break;

                default:
                    break;
            }
            if (targetAudioThing != -1)
            {
                AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, targetAudioThing, mapper);
            }
            mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = newAnimation;
        }

        void Update()
        {

        }
        internal void PlayAfterSecondsNotIEnumerator(BoneMapper mapper, string animName, float seconds)
        {
            StartCoroutine(PlayAfterSeconds(mapper, animName, seconds));
        }
        internal IEnumerator PlayAfterSeconds(BoneMapper mapper, string animName, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            CustomEmotesAPI.PlayAnimation(animName, mapper);
        }
        internal void KillAfterSecondsNotIEnumerator(BoneMapper mapper, float seconds)
        {
            StartCoroutine(KillAfterSeconds(mapper, seconds));
        }
        internal IEnumerator KillAfterSeconds(BoneMapper mapper, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            //TODO
            //mapper.GetComponentInParent<CharacterModel>().body.healthComponent.Suicide();
        }

        //TODO
        internal void AddAnimation(string AnimClip, string wwise, bool looping, bool dimAudio, bool sync)
        {
            //CustomEmotesAPI.AddCustomAnimation(, looping, $"Play_{wwise}", $"Stop_{wwise}", dimWhenClose: dimAudio, syncAnim: sync, syncAudio: sync);
            AnimationClipParams clipParams = new AnimationClipParams
            {
                animationClip = [Assets.Load<AnimationClip>($"{AnimClip}.anim")],
                looping = looping,
                syncAnim = sync,
                syncAudio = sync,
                thirdPerson = true
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            string emote = AnimClip.Split('/')[AnimClip.Split('/').Length - 1];
            BoneMapper.animClips[emote].vulnerableEmote = true;

        }

        internal void AddAnimation(string AnimClip, AudioClip[] audioClip, string AnimClip2ElectricBoogaloo, bool dimAudio, bool sync)
        {
            //CustomEmotesAPI.AddCustomAnimation(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{AnimClip}.anim"), true, $"Play_{wwise}", $"Stop_{wwise}", secondaryAnimation: , dimWhenClose: dimAudio, syncAnim: sync, syncAudio: sync);
            AnimationClipParams clipParams = new AnimationClipParams
            {
                animationClip = [Assets.Load<AnimationClip>($"{AnimClip}.anim")],
                secondaryAnimation = [Assets.Load<AnimationClip>($"{AnimClip2ElectricBoogaloo}.anim")],
                looping = false,
                syncAnim = sync,
                syncAudio = sync,
                thirdPerson = true,
                _primaryAudioClips = audioClip,
                _primaryDMCAFreeAudioClips = audioClip
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            string emote = AnimClip.Split('/')[AnimClip.Split('/').Length - 1];
            BoneMapper.animClips[emote].vulnerableEmote = true;
        }
        internal void AddAnimation(string AnimClip, string wwise, string AnimClip2ElectricBoogaloo, bool dimAudio, bool sync, bool visibility)
        {
            //CustomEmotesAPI.AddCustomAnimation(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{AnimClip}.anim"), true, $"Play_{wwise}", $"Stop_{wwise}", secondaryAnimation: Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{AnimClip2ElectricBoogaloo}.anim"), dimWhenClose: dimAudio, syncAnim: sync, syncAudio: sync, visible: visibility);
            AnimationClipParams clipParams = new AnimationClipParams
            {
                animationClip = [Assets.Load<AnimationClip>($"{AnimClip}.anim")],
                secondaryAnimation = [Assets.Load<AnimationClip>($"{AnimClip2ElectricBoogaloo}.anim")],
                looping = true,
                syncAnim = sync,
                syncAudio = sync,
                visible = visibility,
                thirdPerson = true
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            string emote = AnimClip.Split('/')[AnimClip.Split('/').Length - 1];
            BoneMapper.animClips[emote].vulnerableEmote = true;

        }
        internal string AddHiddenAnimation(string[] AnimClip, string[] AnimClip2ElectricBoogaloo, JoinSpot[] joinSpots)
        {
            List<AnimationClip> primary = new List<AnimationClip>();
            foreach (var item in AnimClip)
            {
                primary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
            }
            List<AnimationClip> secondary = new List<AnimationClip>();
            foreach (var item in AnimClip2ElectricBoogaloo)
            {
                secondary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
            }
            string emote = AnimClip[0].Split('/')[AnimClip[0].Split('/').Length - 1]; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ;
            //CustomEmotesAPI.AddCustomAnimation(primary.ToArray(), true, wwise, stopwwise.ToArray(), secondaryAnimation: secondary.ToArray(), joinSpots: joinSpots, visible: false);
            AnimationClipParams clipParams = new AnimationClipParams
            {
                animationClip = primary.ToArray(),
                secondaryAnimation = secondary.ToArray(),
                looping = true,
                joinSpots = joinSpots,
                visible = false,
                thirdPerson = true
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
        internal string AddHiddenAnimation(string[] AnimClip)
        {
            List<AnimationClip> primary = new List<AnimationClip>();
            foreach (var item in AnimClip)
            {
                primary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
            }
            string emote = AnimClip[0].Split('/')[AnimClip[0].Split('/').Length - 1]; ;
            //CustomEmotesAPI.AddCustomAnimation(primary.ToArray(), false, wwise, stopwwise.ToArray(), visible: false);
            AnimationClipParams clipParams = new AnimationClipParams
            {
                animationClip = primary.ToArray(),
                looping = false,
                visible = false,
                thirdPerson = true
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
        internal string AddHiddenAnimation(string[] AnimClip, AudioClip[] audioClips)
        {
            List<AnimationClip> primary = new List<AnimationClip>();
            foreach (var item in AnimClip)
            {
                primary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
            }
            string emote = AnimClip[0].Split('/')[AnimClip[0].Split('/').Length - 1]; ;
            //CustomEmotesAPI.AddCustomAnimation(primary.ToArray(), false, wwise, stopwwise.ToArray(), visible: false);
            AnimationClipParams clipParams = new AnimationClipParams
            {
                animationClip = primary.ToArray(),
                looping = false,
                visible = false,
                thirdPerson = true,
                _primaryAudioClips = audioClips,
                _primaryDMCAFreeAudioClips = audioClips
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
        internal string AddHiddenAnimation(string[] AnimClip, string[] wwise, string stopWwise, bool sync)
        {
            List<string> stopwwise = new List<string>();
            foreach (var item in wwise)
            {
                stopwwise.Add($"Stop_{stopWwise}");
            }
            List<AnimationClip> primary = new List<AnimationClip>();
            foreach (var item in AnimClip)
            {
                primary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
            }
            string emote = AnimClip[0].Split('/')[AnimClip[0].Split('/').Length - 1]; ;
            //CustomEmotesAPI.AddCustomAnimation(primary.ToArray(), true, wwise, stopwwise.ToArray(), visible: false, syncAudio: sync);
            AnimationClipParams clipParams = new AnimationClipParams
            {
                animationClip = [.. primary],
                looping = true,
                visible = false,
                syncAudio = sync,
                thirdPerson = true
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
        internal string AddHiddenAnimation(string[] AnimClip, string[] wwise, string stopWwise, bool sync, string[] AnimClip2)
        {
            List<string> stopwwise = new List<string>();
            foreach (var item in wwise)
            {
                stopwwise.Add($"Stop_{stopWwise}");
            }
            List<AnimationClip> primary = new List<AnimationClip>();
            foreach (var item in AnimClip)
            {
                primary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
            }
            List<AnimationClip> secondary = new List<AnimationClip>();
            foreach (var item in AnimClip2)
            {
                secondary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
            }
            string emote = AnimClip[0].Split('/')[AnimClip[0].Split('/').Length - 1]; ;
            //CustomEmotesAPI.AddCustomAnimation(primary.ToArray(), false, wwise, stopwwise.ToArray(), visible: false, syncAudio: sync, secondaryAnimation: secondary.ToArray());

            AnimationClipParams clipParams = new AnimationClipParams
            {
                animationClip = primary.ToArray(),
                secondaryAnimation = secondary.ToArray(),
                looping = false,
                visible = false,
                syncAudio = sync,
                thirdPerson = true
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
    }
}
