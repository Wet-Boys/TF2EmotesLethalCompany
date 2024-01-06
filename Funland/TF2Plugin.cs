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
        internal static List<int> validMercs = new List<int>();
        public static PluginInfo PInfo { get; private set; }

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
                catch (Exception)
                {
                }
            }
            Assets.LoadAssetBundlesFromFolder("assetbundles");
            RegisterAllSounds();
            Settings.RunAll();

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
        public static int GetMercNumber()
        {
            if (validMercs.Count == 0)
            {
                return UnityEngine.Random.Range(0, 9);
            }
            return validMercs[UnityEngine.Random.Range(0, validMercs.Count)];
        }
        private void CustomEmotesAPI_animJoined(string joinedAnimation, BoneMapper joiner, BoneMapper host)
        {
            if (joinedAnimation.EndsWith("_Conga"))
            {
                int num = GetMercNumber();
                TF2Networker.instance.SyncEmoteToServerRpc(joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Conga_Start", num, joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
            }
            if (joinedAnimation.StartsWith("Kazotsky_"))
            {
                TF2Networker.instance.SyncEmoteToServerRpc(joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Kazotsky_Start", GetMercNumber(), joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
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
        internal int _conga, _conga_demo_start, _conga_engi_start, _conga_heavy_start, _conga_medic_start, _conga_pyro_start, _conga_scout_start, _conga_sniper_start, _conga_soldier_start, _conga_spy_start, _conga_demo_loop, _conga_engi_loop, _conga_heavy_loop, _conga_medic_loop, _conga_pyro_loop, _conga_scout_loop, _conga_sniper_loop, _conga_soldier_loop, _conga_spy_loop, _demo_flip_flip, _demo_flip_throw, _demo_flip_waiting, _demo_laugh, _engi_flip_flip, _engi_flip_throw, _engi_flip_waiting, _engi_laugh, _heavy_flip_flip, _heavy_flip_throw, _heavy_flip_waiting, _heavy_laugh, _kazotsky, _medic_flip_flip, _medic_flip_throw, _medic_flip_waiting, _medic_laugh, _play_rancho, _play_ranchoburp, _play_ranchoclose, _play_rancholong, _play_ranchoquick, _pyro_flip_flip, _pyro_flip_throw, _pyro_flip_waiting, _pyro_laugh, _rps_demo_initiate, _rps_demo_loss, _rps_demo_winpaper, _rps_demo_winrock, _rps_demo_winscissors, _rps_engi_initiate, _rps_engi_loss, _rps_engi_winpaper, _rps_engi_winrock, _rps_engi_winscissors, _rps_heavy_initiate, _rps_heavy_loss, _rps_heavy_winpaper, _rps_heavy_winrock, _rps_heavy_winscissors, _rps_medic_initiate, _rps_medic_loss, _rps_medic_winpaper, _rps_medic_winrock, _rps_medic_winscissors, _rps_pyro_initiate, _rps_pyro_loss, _rps_pyro_winpaper, _rps_pyro_winrock, _rps_pyro_winscissors, _rps_scout_initiate, _rps_scout_loss, _rps_scout_lossrock, _rps_scout_winpaper, _rps_scout_winrock, _rps_scout_winscissors, _rps_sniper_loss, _rps_sniper_winpaper, _rps_sniper_winrock, _rps_sniper_winscissors, _rps_sniper_initiate_start, _rps_sniper_initiate_loop, _rps_soldier_initiate, _rps_soldier_loss, _rps_soldier_winpaper, _rps_soldier_winrock, _rps_soldier_winscissors, _rps_spy_initiate, _rps_spy_windup1, _rps_spy_windup2, _rps_spy_windup3, _rps_spy_losspaper, _rps_spy_lossrock, _rps_spy_lossscissors, _rps_spy_winpaper, _rps_spy_winrock, _rps_spy_winscissors, _scout_flip_flip, _scout_flip_throw, _scout_flip_waiting, _sniper_flip_flip, _sniper_flip_throw, _sniper_flip_waiting, _soldier_flip_flip, _soldier_flip_throw, _soldier_flip_waiting, _spy_flip_flip, _spy_flip_throw, _spy_flip_waiting, _spy_laugh;
        public void RegisterAllSounds()
        {
            ////////////////////////////Rancho
            _play_rancho = RegisterSound([["RanchoOpen"]], [1.5f], 0);
            _play_ranchoburp = RegisterSound([["RanchoBurp"]], [1.5f], 0);
            _play_ranchoclose = RegisterSound([["RanchoClose"]], [0], 0);
            _play_rancholong = RegisterSound([["RanchoDrink1"]], [2], 0);
            _play_ranchoquick = RegisterSound([["RanchoDrink2"]], [.7f], 0);

            /////////////////////////////Conga
            _conga_demo_start = RegisterSound([["Taunt_demo_conga_int_01", "Taunt_demo_conga_int_02", "Taunt_demo_conga_int_04", "Taunt_demo_conga_int_05", "Taunt_demo_conga_int_06", "Taunt_demo_conga_int_07"]], [0], 0);
            _conga_demo_loop = RegisterSound([["Taunt_demo_conga_fun_08", "Taunt_demo_conga_fun_10", "Taunt_demo_conga_fun_11", "Taunt_demo_conga_fun_12", "Taunt_demo_conga_fun_18", "Taunt_demo_conga_fun_19", "Taunt_demo_conga_fun_24"]], [3], 3);

            _conga_engi_start = RegisterSound([["Eng_taunt_cong_fun_02", "Eng_taunt_cong_fun_04", "Eng_taunt_cong_fun_08", "Eng_taunt_cong_fun_09", "Eng_taunt_cong_fun_10", "Eng_taunt_cong_fun_13", "Eng_taunt_cong_fun_14", "Eng_taunt_cong_fun_16", "Eng_taunt_cong_fun_20", "Eng_taunt_cong_fun_26", "Eng_taunt_cong_fun_30", "Eng_taunt_cong_fun_33", "Eng_taunt_cong_fun_34", "Eng_taunt_cong_fun_35", "Eng_taunt_cong_fun_36", "Eng_taunt_cong_fun_42"]], [0], 0);
            _conga_engi_loop = RegisterSound([["Eng_taunt_cong_fun_02", "Eng_taunt_cong_fun_04", "Eng_taunt_cong_fun_08", "Eng_taunt_cong_fun_09", "Eng_taunt_cong_fun_10", "Eng_taunt_cong_fun_13", "Eng_taunt_cong_fun_14", "Eng_taunt_cong_fun_16", "Eng_taunt_cong_fun_20", "Eng_taunt_cong_fun_26", "Eng_taunt_cong_fun_30", "Eng_taunt_cong_fun_33", "Eng_taunt_cong_fun_34", "Eng_taunt_cong_fun_35", "Eng_taunt_cong_fun_36", "Eng_taunt_cong_fun_42"]], [3], 3);

            _conga_heavy_start = RegisterSound([["Heavy_taunt_cong_int_11", "Heavy_taunt_cong_int_08", "Heavy_taunt_cong_int_12", "Heavy_taunt_cong_int_13"]], [0], 0);
            _conga_heavy_loop = RegisterSound([["Heavy_taunt_cong_fun_01", "Heavy_taunt_cong_fun_11", "Heavy_taunt_cong_fun_12", "Heavy_taunt_cong_fun_19", "Heavy_taunt_cong_fun_20", "Heavy_taunt_cong_int_07", "Heavy_taunt_cong_int_09"]], [3], 3);

            _conga_medic_start = RegisterSound([["Medic_taunt_cong_fun_01", "Medic_taunt_cong_fun_06", "Medic_taunt_cong_fun_07", "Medic_taunt_cong_fun_08", "Medic_taunt_cong_fun_09", "Medic_taunt_cong_fun_12", "Medic_taunt_cong_fun_15"]], [0], 0);
            _conga_medic_loop = RegisterSound([["Medic_taunt_cong_fun_01", "Medic_taunt_cong_fun_06", "Medic_taunt_cong_fun_07", "Medic_taunt_cong_fun_08", "Medic_taunt_cong_fun_09", "Medic_taunt_cong_fun_12", "Medic_taunt_cong_fun_15"]], [3], 3);

            _conga_pyro_start = RegisterSound([["Pyro_taunt_cong_fun_05", "Pyro_taunt_cong_fun_08", "Pyro_taunt_cong_fun_09", "Pyro_taunt_cong_fun_10", "Pyro_taunt_cong_fun_11", "Pyro_taunt_cong_fun_12", "Pyro_taunt_cong_fun_13", "Pyro_taunt_cong_fun_14"]], [0], 0);
            _conga_pyro_loop = RegisterSound([["Pyro_taunt_cong_fun_05", "Pyro_taunt_cong_fun_08", "Pyro_taunt_cong_fun_09", "Pyro_taunt_cong_fun_10", "Pyro_taunt_cong_fun_11", "Pyro_taunt_cong_fun_12", "Pyro_taunt_cong_fun_13", "Pyro_taunt_cong_fun_14"]], [3], 3);

            _conga_scout_start = RegisterSound([["Scout_taunt_conga_int_02", "Scout_taunt_conga_int_03", "Scout_taunt_conga_int_10"]], [0], 0);
            _conga_scout_loop = RegisterSound([["Scout_taunt_conga_fun_01", "Scout_taunt_conga_fun_02", "Scout_taunt_conga_fun_05", "Scout_taunt_conga_fun_06", "Scout_taunt_conga_fun_07", "Scout_taunt_conga_fun_08", "Scout_taunt_conga_fun_09", "Scout_taunt_conga_fun_11", "Scout_taunt_conga_fun_12", "Scout_taunt_conga_fun_14"]], [3], 3);

            _conga_sniper_start = RegisterSound([["Sniper_taunt_cong_fun_02", "Sniper_taunt_cong_fun_03", "Sniper_taunt_cong_fun_25", "Sniper_taunt_cong_int_03"]], [0], 0);
            _conga_sniper_loop = RegisterSound([["Sniper_taunt_cong_fun_01", "Sniper_taunt_cong_fun_04", "Sniper_taunt_cong_fun_05", "Sniper_taunt_cong_fun_06", "Sniper_taunt_cong_fun_10", "Sniper_taunt_cong_fun_11", "Sniper_taunt_cong_fun_12", "Sniper_taunt_cong_fun_17", "Sniper_taunt_cong_fun_18", "Sniper_taunt_cong_fun_24", "Sniper_taunt_cong_int_01", "Sniper_taunt_cong_int_02"]], [3], 3);

            _conga_soldier_start = RegisterSound([["Soldier_taunt_cong_int_03", "Soldier_taunt_cong_int_04", "Soldier_taunt_cong_int_13"]], [0], 0);
            _conga_soldier_loop = RegisterSound([["Soldier_taunt_admire_22", "Soldier_taunt_cong_fun_01", "Soldier_taunt_cong_fun_04", "Soldier_taunt_cong_fun_08", "Soldier_taunt_cong_fun_11", "Soldier_taunt_cong_fun_24", "Soldier_taunt_cong_fun_27", "Soldier_taunt_cong_fun_29"]], [3], 3);

            _conga_spy_start = RegisterSound([["Spy_taunt_cong_int_01", "Spy_taunt_cong_int_05", "Spy_taunt_cong_int_11"]], [0], 0);
            _conga_spy_loop = RegisterSound([["Spy_taunt_cong_fun_01", "Spy_taunt_cong_fun_02", "Spy_taunt_cong_fun_03", "Spy_taunt_cong_fun_05", "Spy_taunt_cong_fun_06", "Spy_taunt_cong_fun_08", "Spy_taunt_cong_fun_09", "Spy_taunt_cong_fun_10", "Spy_taunt_cong_fun_14", "Spy_taunt_cong_fun_15", "Spy_taunt_cong_fun_17"]], [3], 3);





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
            _medic_flip_throw = RegisterSound([["Medic_taunt_exert_01", "Medic_taunt_exert_02", "Medic_taunt_exert_08", "Medic_taunt_exert_09", "Medic_taunt_flip_exert_01", "Medic_taunt_flip_exert_03", "Medic_taunt_flip_exert_04", "Medic_taunt_flip_exert_05", "Medic_taunt_flip_exert_06", "Medic_taunt_flip_exert_07", "Medic_taunt_flip_exert_08", "Medic_taunt_flip_exert_09", "Medic_taunt_flip_exert_10"]], [.6f], 0);
            _medic_flip_waiting = RegisterSound([["Medic_taunt_flip_int_05", "Medic_taunt_flip_int_08", "Medic_taunt_flip_int_10", "Medic_taunt_flip_int_12", "Medic_taunt_flip_int_15"]], [0], 3);

            _pyro_flip_flip = RegisterSound([["Pyro_taunt_flip_fun_01", "Pyro_taunt_flip_fun_04", "Pyro_taunt_flip_fun_05", "Pyro_taunt_flip_fun_06", "Pyro_taunt_flip_fun_09", "Pyro_taunt_flip_fun_10", "Pyro_taunt_flip_fun_11"], ["Pyro_taunt_flip_admire_01", "Pyro_taunt_flip_admire_02", "Pyro_taunt_flip_admire_03", "Pyro_taunt_flip_admire_05", "Pyro_taunt_flip_admire_06", "Pyro_taunt_thanks_07", "Pyro_taunt_thanks_08", "Pyro_taunt_thanks_09"]], [1.2f, 2.2f], 0);
            _pyro_flip_throw = RegisterSound([["Pyro_taunt_exert_12", "Pyro_taunt_flip_exert_02", "Pyro_taunt_flip_exert_04", "Pyro_taunt_flip_exert_05", "Pyro_taunt_flip_exert_06"]], [.6f], 0);
            _pyro_flip_waiting = RegisterSound([["Pyro_taunt_flip_int_02", "Pyro_taunt_flip_int_05", "Pyro_taunt_flip_int_07"]], [0], 3);

            _scout_flip_flip = RegisterSound([["Scout_taunt_flip_fun_01", "Scout_taunt_flip_fun_02", "Scout_taunt_flip_fun_03", "Scout_taunt_flip_fun_05", "Scout_taunt_flip_fun_06", "Scout_taunt_flip_fun_08", "Scout_taunt_flip_fun_09", "Scout_taunt_flip_fun_10"], ["Scout_taunt_flip_end_01", "Scout_taunt_flip_end_03", "Scout_taunt_flip_end_05", "Scout_taunt_flip_end_07", "Scout_taunt_flip_end_08", "Scout_taunt_flip_end_17", "Scout_taunt_flip_end_19", "Scout_taunt_flip_end_22", "Scout_taunt_flip_end_27"]], [1.2f, 2.2f], 0);
            _scout_flip_throw = RegisterSound([["Scout_taunt_exert_05", "Scout_taunt_exert_13", "Scout_taunt_exert_21", "Scout_taunt_exert_23", "Scout_taunt_exert_30", "Scout_taunt_flip_exert_01", "Scout_taunt_flip_exert_05", "Scout_taunt_flip_exert_08", "Scout_taunt_flip_exert_09", "Scout_taunt_flip_exert_10", "Scout_taunt_flip_exert_13"]], [.6f], 0);
            _scout_flip_waiting = RegisterSound([["Scout_taunt_flip_int_03", "Scout_taunt_flip_int_06", "Scout_taunt_flip_int_07", "Scout_taunt_flip_int_10", "Scout_taunt_flip_int_12", "Scout_taunt_flip_int_13", "Scout_taunt_int_01", "Scout_taunt_int_03", "Scout_taunt_int_05", "Scout_taunt_int_06", "Scout_taunt_int_07", "Scout_taunt_int_08", "Scout_taunt_int_12", "Scout_taunt_int_14", "Scout_taunt_int_17", "Scout_taunt_int_18"]], [0], 3);

            _sniper_flip_flip = RegisterSound([["Sniper_taunt_admire_01", "Sniper_taunt_admire_02", "Sniper_taunt_admire_06", "Sniper_taunt_admire_09", "Sniper_taunt_admire_11", "Sniper_taunt_admire_12", "Sniper_taunt_admire_15", "Sniper_taunt_admire_16", "Sniper_taunt_admire_18", "Sniper_taunt_admire_19", "Sniper_taunt_admire_20", "Sniper_taunt_flip_end_02", "Sniper_taunt_flip_end_03", "Sniper_taunt_flip_end_04", "Sniper_taunt_flip_end_06", "Sniper_taunt_flip_end_07", "Sniper_taunt_flip_fun_05", "Sniper_taunt_flip_fun_06"]], [2.2f], 0);
            _sniper_flip_throw = RegisterSound([["Sniper_taunt_exert_03", "Sniper_taunt_exert_07", "Sniper_taunt_exert_10", "Sniper_taunt_exert_15", "Sniper_taunt_flip_exert_01", "Sniper_taunt_flip_exert_04", "Sniper_taunt_flip_exert_05", "Sniper_taunt_flip_exert_06", "Sniper_taunt_flip_exert_07"]], [.6f], 0);
            _sniper_flip_waiting = RegisterSound([["Sniper_taunt_flip_int_04", "Sniper_taunt_flip_int_06", "Sniper_taunt_flip_int_07", "Sniper_taunt_flip_int_10", "Sniper_taunt_flip_int_11", "Sniper_taunt_int_01", "Sniper_taunt_int_13"]], [0], 3);

            _soldier_flip_flip = RegisterSound([["Soldier_taunt_flip_fun_04", "Soldier_taunt_flip_fun_06", "Soldier_taunt_flip_fun_08"], ["Soldier_taunt_admire_01", "Soldier_taunt_admire_04", "Soldier_taunt_admire_09", "Soldier_taunt_admire_10", "Soldier_taunt_admire_16", "Soldier_taunt_admire_17", "Soldier_taunt_admire_18", "Soldier_taunt_admire_22", "Soldier_taunt_admire_24", "Soldier_taunt_admire_26", "Soldier_taunt_flip_end_01", "Soldier_taunt_flip_end_02", "Soldier_taunt_flip_end_03", "Soldier_taunt_flip_end_05", "Soldier_taunt_flip_end_15", "Soldier_taunt_flip_end_16", "Soldier_taunt_flip_end_17"]], [1.2f, 2.2f], 0);
            _soldier_flip_throw = RegisterSound([["Soldier_taunt_exert_02", "Soldier_taunt_exert_06", "Soldier_taunt_flip_exert_02", "Soldier_taunt_flip_exert_06", "Soldier_taunt_flip_exert_21", "Soldier_taunt_flip_exert_31"]], [.6f], 0);
            _soldier_flip_waiting = RegisterSound([["Soldier_taunt_flip_int_01", "Soldier_taunt_flip_int_03", "Soldier_taunt_flip_int_04", "Soldier_taunt_flip_int_11", "Soldier_taunt_flip_int_15", "Soldier_taunt_flip_int_17", "Soldier_taunt_flip_int_19", "Soldier_taunt_flip_int_20", "Soldier_taunt_flip_int_24"]], [0], 3);

            _spy_flip_flip = RegisterSound([["Spy_taunt_flip_fun_01", "Spy_taunt_flip_fun_02", "Spy_taunt_flip_fun_07", "Spy_taunt_flip_fun_09", "Spy_taunt_flip_fun_12", "Spy_taunt_flip_fun_13"], ["Spy_taunt_bos_int_05", "Spy_taunt_bos_kick_02", "Spy_taunt_flip_admire_05", "Spy_taunt_flip_admire_09", "Spy_taunt_flip_admire_18", "Spy_taunt_flip_admire_20", "Spy_taunt_flip_end_07", "Spy_taunt_flip_end_12", "Spy_taunt_flip_end_14", "Spy_taunt_flip_end_16"]], [1.2f, 2.2f], 0);
            _spy_flip_throw = RegisterSound([["Spy_taunt_flip_exert_01", "Spy_taunt_flip_exert_02", "Spy_taunt_flip_exert_07", "Spy_taunt_flip_exert_08", "Spy_taunt_flip_exert_09", "Spy_taunt_flip_exert_10"]], [.6f], 0);
            _spy_flip_waiting = RegisterSound([["Spy_taunt_flip_int_01", "Spy_taunt_flip_int_02", "Spy_taunt_flip_int_03", "Spy_taunt_flip_int_04", "Spy_taunt_flip_int_07", "Spy_taunt_flip_int_15", "Spy_taunt_flip_int_16", "Spy_taunt_flip_int_20", "Spy_taunt_flip_int_25", "Spy_taunt_flip_int_28"]], [0], 3);



            ///////////////////////////RPS
            _rps_demo_initiate = RegisterSound([["Taunt_demo_rps_int_01", "Taunt_demo_rps_int_06"]], [0], 3);
            _rps_demo_loss = RegisterSound([["Taunt_demo_rps_lose_04", "Taunt_demo_rps_lose_06", "Taunt_demo_rps_lose_08", "Taunt_demo_rps_lose_09", "Taunt_demo_rps_lose_10"], ["Taunt_demo_rps_exert_04"]], [5.5f, 1.7f], 0);
            _rps_demo_winpaper = RegisterSound([["Taunt_demo_rps_win_03", "Taunt_demo_rps_win_04", "Taunt_demo_rps_win_06", "Taunt_demo_rps_win_08", "Taunt_demo_rps_win_16", "Taunt_demo_rps_win_24", "Taunt_demo_rps_win_25", "Taunt_demo_rps_win_27", "Taunt_demo_rps_win_28"], ["Taunt_demo_rps_exert_04"]], [3.5f, 1.7f], 0);
            _rps_demo_winrock = RegisterSound([["Taunt_demo_rps_win_03", "Taunt_demo_rps_win_04", "Taunt_demo_rps_win_06", "Taunt_demo_rps_win_08", "Taunt_demo_rps_win_16", "Taunt_demo_rps_win_24", "Taunt_demo_rps_win_25", "Taunt_demo_rps_win_27", "Taunt_demo_rps_win_28"], ["Taunt_demo_rps_exert_04"]], [3.5f, 1.7f], 0);
            _rps_demo_winscissors = RegisterSound([["Taunt_demo_rps_win_03", "Taunt_demo_rps_win_04", "Taunt_demo_rps_win_06", "Taunt_demo_rps_win_08", "Taunt_demo_rps_win_16", "Taunt_demo_rps_win_24", "Taunt_demo_rps_win_25", "Taunt_demo_rps_win_27", "Taunt_demo_rps_win_28"], ["Taunt_demo_rps_exert_04"]], [3.5f, 1.7f], 0);

            _rps_engi_initiate = RegisterSound([["Eng_taunt_rps_int_01", "Eng_taunt_rps_int_03", "Eng_taunt_rps_int_07"]], [0], 3);
            _rps_engi_loss = RegisterSound([["Eng_taunt_rps_exert_07 (1)"], ["taunt_hard_clap1"], ["taunt_hard_clap1"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Eng_taunt_rps_exert_01"], ["Eng_taunt_rps_exert_02"], ["Eng_taunt_rps_exert_03"], ["Eng_taunt_rps_lose_22", "Eng_taunt_rps_lose_25", "Eng_taunt_rps_lose_27", "Eng_taunt_rps_lose_29", "Eng_taunt_rps_lose_31"]], [0, .5f, .7f, 1.7f, 1.928f, 2.156f, 1.7f, 2.159f, 2.63f, 5.5f], 0);
            _rps_engi_winpaper = RegisterSound([["Eng_taunt_rps_exert_07 (1)"], ["taunt_hard_clap1"], ["taunt_hard_clap1"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Eng_taunt_rps_exert_01"], ["Eng_taunt_rps_exert_02"], ["Eng_taunt_rps_exert_03"], ["Eng_taunt_rps_win_07", "Eng_taunt_rps_win_17", "Eng_taunt_rps_win_26", "Eng_taunt_rps_win_31", "Eng_taunt_rps_win_33"]], [0, .5f, .7f, 1.7f, 1.928f, 2.156f, 1.7f, 2.159f, 2.63f, 3.5f], 0);
            _rps_engi_winrock = RegisterSound([["Eng_taunt_rps_exert_07 (1)"], ["taunt_hard_clap1"], ["taunt_hard_clap1"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Eng_taunt_rps_exert_01"], ["Eng_taunt_rps_exert_02"], ["Eng_taunt_rps_exert_03"], ["Eng_taunt_rps_win_10", "Eng_taunt_rps_win_17", "Eng_taunt_rps_win_26", "Eng_taunt_rps_win_31", "Eng_taunt_rps_win_33"]], [0, .5f, .7f, 1.7f, 1.928f, 2.156f, 1.7f, 2.159f, 2.63f, 3.5f], 0);
            _rps_engi_winscissors = RegisterSound([["Eng_taunt_rps_exert_07 (1)"], ["taunt_hard_clap1"], ["taunt_hard_clap1"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Eng_taunt_rps_exert_01"], ["Eng_taunt_rps_exert_02"], ["Eng_taunt_rps_exert_03"], ["Eng_taunt_rps_win_06", "Eng_taunt_rps_win_17", "Eng_taunt_rps_win_26", "Eng_taunt_rps_win_31", "Eng_taunt_rps_win_33"]], [0, .5f, .7f, 1.7f, 1.928f, 2.156f, 1.7f, 2.159f, 2.63f, 3.5f], 0);

            _rps_heavy_initiate = RegisterSound([["Heavy_taunt_rps_int_01", "Heavy_taunt_rps_int_02", "Heavy_taunt_rps_int_04"]], [0], 3);
            _rps_heavy_loss = RegisterSound([["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["Heavy_taunt_rps_lose_11", "Heavy_taunt_rps_lose_13", "Heavy_taunt_rps_lose_18"]], [1.7f, 2.2f, 2.7f, 5.5f], 0);
            _rps_heavy_winpaper = RegisterSound([["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["Heavy_taunt_kill_02", "Heavy_taunt_kill_09", "Heavy_taunt_rps_win_02", "Heavy_taunt_rps_win_03", "Heavy_taunt_rps_win_04", "Heavy_taunt_rps_win_09", "Heavy_taunt_rps_win_11", "Heavy_taunt_rps_win_12", "Heavy_taunt_rps_win_21", "Heavy_taunt_rps_win_27", "Heavy_taunt_rps_win_34"]], [1.7f, 2.2f, 2.7f, 3.5f], 0);
            _rps_heavy_winrock = RegisterSound([["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["Heavy_taunt_kill_02", "Heavy_taunt_kill_09", "Heavy_taunt_rps_int_05", "Heavy_taunt_rps_win_02", "Heavy_taunt_rps_win_03", "Heavy_taunt_rps_win_04", "Heavy_taunt_rps_win_09", "Heavy_taunt_rps_win_11", "Heavy_taunt_rps_win_12", "Heavy_taunt_rps_win_16", "Heavy_taunt_rps_win_27", "Heavy_taunt_rps_win_33"]], [1.7f, 2.2f, 2.7f, 3.5f], 0);
            _rps_heavy_winscissors = RegisterSound([["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["fist_hit_world1", "fist_hit_world2"], ["Heavy_taunt_kill_02", "Heavy_taunt_kill_09", "Heavy_taunt_rps_win_02", "Heavy_taunt_rps_win_03", "Heavy_taunt_rps_win_04", "Heavy_taunt_rps_win_09", "Heavy_taunt_rps_win_11", "Heavy_taunt_rps_win_12", "Heavy_taunt_rps_win_21", "Heavy_taunt_rps_win_27", "Heavy_taunt_rps_win_38"]], [1.7f, 2.2f, 2.7f, 3.5f], 0);

            _rps_medic_initiate = RegisterSound([["Medic_taunt_rps_int_01"]], [0], 3);
            _rps_medic_loss = RegisterSound([["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Medic_taunt_rps_exert_01"], ["Medic_taunt_rps_exert_07"], ["Medic_taunt_rps_exert_24"], ["Medic_taunt_rps_lose_12", "Medic_taunt_rps_lose_14", "Medic_taunt_rps_lose_16", "Medic_taunt_rps_lose_17", "Medic_taunt_rps_lose_19"]], [1.7f, 2.178f, 2.656f, 1.7f, 2.178f, 2.656f, 5.5f], 0);
            _rps_medic_winpaper = RegisterSound([["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Medic_taunt_rps_exert_01"], ["Medic_taunt_rps_exert_07"], ["Medic_taunt_rps_exert_24"], ["Medic_taunt_rps_win_04", "Medic_taunt_rps_win_05", "Medic_taunt_rps_win_06", "Medic_taunt_rps_win_08", "Medic_taunt_rps_win_09"]], [1.7f, 2.178f, 2.656f, 1.7f, 2.178f, 2.656f, 3.5f], 0);
            _rps_medic_winrock = RegisterSound([["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Medic_taunt_rps_exert_01"], ["Medic_taunt_rps_exert_07"], ["Medic_taunt_rps_exert_24"], ["Medic_taunt_rps_win_04", "Medic_taunt_rps_win_05", "Medic_taunt_rps_win_06", "Medic_taunt_rps_win_08", "Medic_taunt_rps_win_09"]], [1.7f, 2.178f, 2.656f, 1.7f, 2.178f, 2.656f, 3.5f], 0);
            _rps_medic_winscissors = RegisterSound([["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Medic_taunt_rps_exert_01"], ["Medic_taunt_rps_exert_07"], ["Medic_taunt_rps_exert_24"], ["Medic_taunt_rps_win_04", "Medic_taunt_rps_win_05", "Medic_taunt_rps_win_06", "Medic_taunt_rps_win_08", "Medic_taunt_rps_win_09"]], [1.7f, 2.178f, 2.656f, 1.7f, 2.178f, 2.656f, 3.5f], 0);

            _rps_pyro_initiate = RegisterSound([["Pyro_taunt_rps_int_02", "Pyro_taunt_rps_int_04", "Pyro_taunt_rps_int_05", "Pyro_taunt_rps_int_07", "Pyro_taunt_rps_int_08"]], [0], 3);
            _rps_pyro_loss = RegisterSound([["Pyro_taunt_rps_exert_18 (1)"], ["Pyro_taunt_rps_exert_21"], ["Pyro_taunt_rps_exert_22"], ["Pyro_taunt_rps_exert_23"], ["Pyro_taunt_rps_lose_03"]], [0f, 1.7f, 2.178f, 2.656f, 5.5f], 0);
            _rps_pyro_winpaper = RegisterSound([["Pyro_taunt_rps_exert_18 (1)"], ["Pyro_taunt_rps_exert_21"], ["Pyro_taunt_rps_exert_22"], ["Pyro_taunt_rps_exert_23"], ["Pyro_laughevil01"]], [0f, 1.7f, 2.178f, 2.656f, 3.5f], 0);
            _rps_pyro_winrock = RegisterSound([["Pyro_taunt_rps_exert_18 (1)"], ["Pyro_taunt_rps_exert_21"], ["Pyro_taunt_rps_exert_22"], ["Pyro_taunt_rps_exert_23"], ["Pyro_laughevil01"]], [0f, 1.7f, 2.178f, 2.656f, 3.5f], 0);
            _rps_pyro_winscissors = RegisterSound([["Pyro_taunt_rps_exert_18 (1)"], ["Pyro_taunt_rps_exert_21"], ["Pyro_taunt_rps_exert_22"], ["Pyro_taunt_rps_exert_23"], ["Pyro_laughevil01"]], [0f, 1.7f, 2.178f, 2.656f, 3.5f], 0);

            _rps_scout_initiate = RegisterSound([["Scout_taunt_rps_int_02", "Scout_taunt_rps_int_03", "Scout_taunt_rps_int_05", "Scout_taunt_rps_int_09", "Scout_taunt_rps_int_10"]], [0], 3);
            _rps_scout_loss = RegisterSound([["Scout_taunt_rps_exert_23"], ["Scout_taunt_rps_exert_25"], ["Scout_taunt_rps_lose_01", "Scout_taunt_rps_lose_03", "Scout_taunt_rps_lose_06", "Scout_taunt_rps_lose_07"]], [1.7f, 0, 5.5f], 0);
            _rps_scout_lossrock = RegisterSound([["Scout_taunt_rps_exert_23"], ["Scout_taunt_rps_exert_25"], ["Scout_taunt_rps_lose_01", "Scout_taunt_rps_lose_03", "Scout_taunt_rps_lose_06", "Scout_taunt_rps_lose_07", "Scout_taunt_rps_lose_12"]], [1.7f, 0, 5.5f], 0);
            _rps_scout_winpaper = RegisterSound([["Scout_taunt_rps_exert_23"], ["Scout_taunt_rps_exert_25"], ["taunt_sfx_bell_single"], ["taunt_eng_swoosh"], ["taunt_eng_swoosh"], ["taunt_eng_swoosh"], ["taunt_eng_swoosh"], ["Scout_taunt_misc_03", "Scout_taunt_misc_10", "Scout_taunt_misc_14", "Scout_taunt_rps_win_27", "Scout_taunt_rps_win_51"]], [1.7f, 0, 3.5f, 5.25f, 5f, 4f, 4.5f, 3.5f], 0);
            _rps_scout_winrock = RegisterSound([["Scout_taunt_rps_exert_23"], ["Scout_taunt_rps_exert_25"], ["taunt_sfx_bell_single"], ["taunt_eng_swoosh"], ["taunt_eng_swoosh"], ["taunt_eng_swoosh"], ["taunt_eng_swoosh"], ["Scout_taunt_misc_03", "Scout_taunt_misc_10", "Scout_taunt_misc_14", "Scout_taunt_rps_win_34"]], [1.7f, 0, 3.5f, 5.25f, 5f, 4f, 4.5f, 3.5f], 0);
            _rps_scout_winscissors = RegisterSound([["Scout_taunt_rps_exert_23"], ["Scout_taunt_rps_exert_25"], ["taunt_sfx_bell_single"], ["taunt_eng_swoosh"], ["taunt_eng_swoosh"], ["taunt_eng_swoosh"], ["taunt_eng_swoosh"], ["Scout_taunt_misc_03", "Scout_taunt_misc_10", "Scout_taunt_misc_14", "Scout_taunt_rps_win_36"]], [1.7f, 0, 3.5f, 5.25f, 5f, 4f, 4.5f, 3.5f], 0);

            _rps_sniper_initiate_start = RegisterSound([["Sniper_taunt_rps_int_03", "Sniper_taunt_rps_int_01"]], [0], 0);
            _rps_sniper_initiate_loop = RegisterSound([["Sniper_taunt_rps_int_05", "Sniper_taunt_rps_int_06", "Sniper_taunt_rps_int_07", "Sniper_taunt_rps_int_11"]], [3], 3);
            _rps_sniper_loss = RegisterSound([["Sniper_taunt_rps_exert_17"], ["Sniper_taunt_rps_exert_01"], ["Sniper_taunt_rps_exert_02"], ["Sniper_taunt_rps_exert_16"], ["Sniper_taunt_rps_lose_04", "Sniper_taunt_rps_lose_06", "Sniper_taunt_rps_lose_13", "Sniper_taunt_rps_lose_15", "Sniper_taunt_rps_lose_22"]], [0, 1.7f, 2.178f, 2.656f, 5.5f], 0);
            _rps_sniper_winpaper = RegisterSound([["Sniper_taunt_rps_exert_17"], ["Sniper_taunt_rps_exert_01"], ["Sniper_taunt_rps_exert_02"], ["Sniper_taunt_rps_exert_16"], ["Sniper_taunt_rps_win_14", "Sniper_taunt_rps_win_15"]], [0, 1.7f, 2.178f, 2.656f, 3.5f], 0);
            _rps_sniper_winrock = RegisterSound([["Sniper_taunt_rps_exert_17"], ["Sniper_taunt_rps_exert_01"], ["Sniper_taunt_rps_exert_02"], ["Sniper_taunt_rps_exert_16"], ["Sniper_taunt_rps_win_18", "Sniper_taunt_rps_win_15"]], [0, 1.7f, 2.178f, 2.656f, 3.5f], 0);
            _rps_sniper_winscissors = RegisterSound([["Sniper_taunt_rps_exert_17"], ["Sniper_taunt_rps_exert_01"], ["Sniper_taunt_rps_exert_02"], ["Sniper_taunt_rps_exert_16"], ["Sniper_taunt_rps_win_20", "Sniper_taunt_rps_win_15"]], [0, 1.7f, 2.178f, 2.656f, 3.5f], 0);

            _rps_soldier_initiate = RegisterSound([["Soldier_taunt_rps_int_01", "Soldier_taunt_rps_int_03", "Soldier_taunt_rps_int_05", "Soldier_taunt_rps_int_07", "Soldier_taunt_rps_int_08"]], [0], 3);
            _rps_soldier_loss = RegisterSound([["Soldier_taunt_rps_exert_11"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Soldier_taunt_rps_exert_01"], ["Soldier_taunt_rps_lose_01", "Soldier_taunt_rps_lose_05", "Soldier_taunt_rps_lose_12", "Soldier_taunt_rps_lose_14", "Soldier_taunt_rps_lose_21", "Soldier_taunt_rps_lose_22"]], [0, 1.7f, 1.928f, 2.156f, 1.7f, 5.5f], 0);
            _rps_soldier_winpaper = RegisterSound([["Soldier_taunt_rps_exert_11"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Soldier_taunt_rps_exert_01"], ["Soldier_taunt_rps_win_55"]], [0, 1.7f, 1.928f, 2.156f, 1.7f, 3.5f], 0);
            _rps_soldier_winrock = RegisterSound([["Soldier_taunt_rps_exert_11"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Soldier_taunt_rps_exert_01"], ["Soldier_taunt_rps_win_57"]], [0, 1.7f, 1.928f, 2.156f, 1.7f, 3.5f], 0);
            _rps_soldier_winscissors = RegisterSound([["Soldier_taunt_rps_exert_11"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["item_boxing_gloves_pickup"], ["Soldier_taunt_rps_exert_01"], ["Soldier_taunt_rps_win_61"]], [0, 1.7f, 1.928f, 2.156f, 1.7f, 3.5f], 0);

            _rps_spy_initiate = RegisterSound([["Spy_rpshold01", "Spy_rpsstart01", "Spy_taunt_rps_int_01", "Spy_taunt_rps_int_05", "Spy_taunt_rps_int_07", "Spy_taunt_rps_int_08"]], [0], 3);
            _rps_spy_windup1 = RegisterSound([["Spy_rpscountone01"], ["Spy_rpscounttwo01"], ["Spy_rpscountthree01"]], [1.7f, 2.178f, 2.656f], 0);
            _rps_spy_windup2 = RegisterSound([["Spy_taunt_rps_exert_08"], ["Spy_taunt_rps_exert_09"], ["Spy_taunt_rps_exert_10"]], [1.7f, 2.178f, 2.656f], 0);
            _rps_spy_windup3 = RegisterSound([["Spy_rpscountrock01"], ["Spy_rpscountpaper01"], ["Spy_rpscountscissor01"]], [1.7f, 2.178f, 2.656f], 0);
            _rps_spy_losspaper = RegisterSound([["Spy_rpslose01", "Spy_rpslose02", "Spy_rpsregretrock01", "Spy_taunt_rps_lose_04", "Spy_taunt_rps_lose_05", "Spy_taunt_rps_lose_06", "Spy_taunt_rps_lose_09", "Spy_taunt_rps_lose_11", "Spy_taunt_rps_lose_12", "Spy_taunt_rps_lose_15"]], [5.5f], 0);
            _rps_spy_lossrock = RegisterSound([["Spy_rpslose01", "Spy_rpslose02", "Spy_rpsregretscissor01", "Spy_taunt_rps_lose_04", "Spy_taunt_rps_lose_05", "Spy_taunt_rps_lose_06", "Spy_taunt_rps_lose_09", "Spy_taunt_rps_lose_11", "Spy_taunt_rps_lose_12", "Spy_taunt_rps_lose_15"]], [5.5f], 0);
            _rps_spy_lossscissors = RegisterSound([["Spy_rpslose01", "Spy_rpslose02", "Spy_rpsregretpaper01", "Spy_taunt_rps_lose_04", "Spy_taunt_rps_lose_05", "Spy_taunt_rps_lose_06", "Spy_taunt_rps_lose_09", "Spy_taunt_rps_lose_11", "Spy_taunt_rps_lose_12", "Spy_taunt_rps_lose_15"]], [5.5f], 0);
            _rps_spy_winpaper = RegisterSound([["Spy_rpspaperwin01", "Spy_rpspaperwin02", "Spy_rpspaperwin03", "Spy_rpswin01", "Spy_rpswin02", "Spy_taunt_rps_win_02", "Spy_taunt_rps_win_03", "Spy_taunt_rps_win_09", "Spy_taunt_rps_win_11", "Spy_taunt_rps_win_12", "Spy_taunt_rps_win_13", "Spy_taunt_rps_win_14", "Spy_taunt_rps_win_15", "Spy_taunt_rps_win_16", "Spy_taunt_rps_win_17", "Spy_taunt_rps_win_18", "Spy_taunt_rps_win_19", "Spy_taunt_rps_win_20", "Spy_taunt_rps_win_21", "Spy_taunt_rps_win_22", "Spy_taunt_rps_win_23"]], [3.5f], 0);
            _rps_spy_winrock = RegisterSound([["Spy_rpsrockwin01", "Spy_rpswin01", "Spy_rpswin02", "Spy_taunt_rps_win_02", "Spy_taunt_rps_win_03", "Spy_taunt_rps_win_09", "Spy_taunt_rps_win_11", "Spy_taunt_rps_win_12", "Spy_taunt_rps_win_13", "Spy_taunt_rps_win_14", "Spy_taunt_rps_win_15", "Spy_taunt_rps_win_16", "Spy_taunt_rps_win_17", "Spy_taunt_rps_win_18", "Spy_taunt_rps_win_19", "Spy_taunt_rps_win_20", "Spy_taunt_rps_win_21", "Spy_taunt_rps_win_22", "Spy_taunt_rps_win_23"]], [3.5f], 0);
            _rps_spy_winscissors = RegisterSound([["Spy_rpsscissorwin01", "Spy_rpswin01", "Spy_rpswin02", "Spy_taunt_rps_win_02", "Spy_taunt_rps_win_03", "Spy_taunt_rps_win_09", "Spy_taunt_rps_win_11", "Spy_taunt_rps_win_12", "Spy_taunt_rps_win_13", "Spy_taunt_rps_win_14", "Spy_taunt_rps_win_15", "Spy_taunt_rps_win_16", "Spy_taunt_rps_win_17", "Spy_taunt_rps_win_18", "Spy_taunt_rps_win_19", "Spy_taunt_rps_win_20", "Spy_taunt_rps_win_21", "Spy_taunt_rps_win_22", "Spy_taunt_rps_win_23"]], [3.5f], 0);
        }
        internal int RegisterSound(string[][] audioClipNames, List<float> delays, float repeatTimer)
        {
            return AudioContainerHolder.Setup(audioClipNames, delays, repeatTimer);
        }
        public void Rancho()
        {
            AddAnimation("Engi/Rancho/RanchoRelaxo", null, "Engi/Rancho/engiRanchoPassive", false, false, "Rancho Relaxo");
            AddAnimation("Engi/Rancho/engiRanchoBurp", "", "Engi/Rancho/engiRanchoPassive", false, false, false, "Rancho Relaxo");
            AddAnimation("Engi/Rancho/engiRanchoBigDrink", "", "Engi/Rancho/engiRanchoPassive", false, false, false, "Rancho Relaxo");
            AddAnimation("Engi/Rancho/engiRanchoQuickDrink", "", "Engi/Rancho/engiRanchoPassive", false, false, false, "Rancho Relaxo");
        }
        public void Laugh()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Schadenfreude");
            string emote = AddHiddenAnimation(new string[] { "Demo/Laugh/Demo_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Demoman_laughlong02.ogg")], "Schadenfreude");
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Engi/Laugh/Engi_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Engineer_laughlong02.ogg")], "Schadenfreude");
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/Laugh/Heavy_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Heavy_laugherbigsnort01.ogg")], "Schadenfreude");
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/Laugh/Medic_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Medic_laughlong01.ogg")], "Schadenfreude");
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/Laugh/Pyro_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Pyro_laugh_addl04.ogg")], "Schadenfreude");
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/Laugh/Scout_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Scout_laughlong02.ogg")], "Schadenfreude");
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/Laugh/Sniper_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Sniper_laughlong02.ogg")], "Schadenfreude");
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/Laugh/Soldier_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Soldier_laughlong03.ogg")], "Schadenfreude");
            Laugh_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/Laugh/Spy_Laugh" }, [Assets.Load<AudioClip>($"assets/audio dump/Spy_laughlong01.ogg")], "Schadenfreude");
            Laugh_Emotes.Add(emote);

        }
        public void Flip()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Flippin' Awesome");


            string emote = AddHiddenAnimation(new string[] { "Demo/Flip/Demo_Flip_Start" }, new string[] { "Demo/Flip/Demo_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) }, "Flippin' Awesome");
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Demo/Flip/Demo_Flip_Throw" }, "Flippin' Awesome");
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Demo/Flip/Demo_Flip_Flip" }, "Flippin' Awesome");
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Engi/Flip/Engi_Flip_Start" }, new string[] { "Engi/Flip/Engi_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) }, "Flippin' Awesome");
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Engi/Flip/Engi_Flip_Throw" }, "Flippin' Awesome");
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Engi/Flip/Engi_Flip_Flip" }, "Flippin' Awesome");
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Heavy/Flip/Heavy_Flip_Start" }, new string[] { "Heavy/Flip/Heavy_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) }, "Flippin' Awesome");
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/Flip/Heavy_Flip_Throw" }, "Flippin' Awesome");
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Heavy/Flip/Heavy_Flip_Flip" }, "Flippin' Awesome");
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Medic/Flip/Medic_Flip_Start" }, new string[] { "Medic/Flip/Medic_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) }, "Flippin' Awesome");
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/Flip/Medic_Flip_Throw" }, "Flippin' Awesome");
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Medic/Flip/Medic_Flip_Flip" }, "Flippin' Awesome");
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Pyro/Flip/Pyro_Flip_Start" }, new string[] { "Pyro/Flip/Pyro_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) }, "Flippin' Awesome");
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/Flip/Pyro_Flip_Throw" }, "Flippin' Awesome");
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Pyro/Flip/Pyro_Flip_Flip" }, "Flippin' Awesome");
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Scout/Flip/Scout_Flip_Start" }, new string[] { "Scout/Flip/Scout_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) }, "Flippin' Awesome");
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/Flip/Scout_Flip_Throw" }, "Flippin' Awesome");
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Scout/Flip/Scout_Flip_Flip" }, "Flippin' Awesome");
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Sniper/Flip/Sniper_Flip_Start" }, new string[] { "Sniper/Flip/Sniper_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) }, "Flippin' Awesome");
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/Flip/Sniper_Flip_Throw" }, "Flippin' Awesome");
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Sniper/Flip/Sniper_Flip_Flip" }, "Flippin' Awesome");
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Soldier/Flip/Soldier_Flip_Start" }, new string[] { "Soldier/Flip/Soldier_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) }, "Flippin' Awesome");
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/Flip/Soldier_Flip_Throw" }, "Flippin' Awesome");
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Soldier/Flip/Soldier_Flip_Flip" }, "Flippin' Awesome");
            Flip_Flip_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Spy/Flip/Spy_Flip_Start" }, new string[] { "Spy/Flip/Spy_Flip_Wait" }, new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) }, "Flippin' Awesome");
            Flip_Wait_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/Flip/Spy_Flip_Throw" }, "Flippin' Awesome");
            Flip_Throw_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Spy/Flip/Spy_Flip_Flip" }, "Flippin' Awesome");
            Flip_Flip_Emotes.Add(emote);
        }
        public void RPS()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Rock Paper Scissors");
            CustomEmotesAPI.BlackListEmote("Rock Paper Scissors");
            //CustomEmotesAPI.AddNonAnimatingEmote("Rock", false);
            //CustomEmotesAPI.AddNonAnimatingEmote("Paper", false);
            //CustomEmotesAPI.AddNonAnimatingEmote("Scissors", false);
            string emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_Start" }, new string[] { "Demo/RPS/DemoRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) }, "Rock Paper Scissors");
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_RWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_RLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_PWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_PLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_SWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_SLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPSStart" }, new string[] { "Engi/RPS/EngiRPSLoop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) }, "Rock Paper Scissors");
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_RWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_RLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_PWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_PLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_SWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_SLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_Start" }, new string[] { "Heavy/RPS/HeavyRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) }, "Rock Paper Scissors");
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_RWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_RLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_PWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_PLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_SWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_SLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_Start" }, new string[] { "Medic/RPS/MedicRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) }, "Rock Paper Scissors");
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_RWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_RLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_PWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_PLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_SWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_SLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_Start" }, new string[] { "Pyro/RPS/PyroRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) }, "Rock Paper Scissors");
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_RWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_RLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_PWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_PLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_SWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_SLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_Start" }, new string[] { "Scout/RPS/ScoutRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) }, "Rock Paper Scissors");
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_RWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_RLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_PWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_PLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_SWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_SLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_Start" }, new string[] { "Sniper/RPS/SniperRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) }, "Rock Paper Scissors");
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_RWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_RLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_PWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_PLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_SWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_SLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_Start" }, new string[] { "Soldier/RPS/SoldierRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) }, "Rock Paper Scissors");
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_RWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_RLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_PWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_PLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_SWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_SLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);


            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_Start" }, new string[] { "Spy/RPS/SpyRPS_Loop" }, new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) }, "Rock Paper Scissors");
            RPS_Start_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_RWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_RLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_PWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_PLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);

            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_SWin" }, "Rock Paper Scissors");
            RPS_Win_Emotes.Add(emote);
            emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_SLose" }, "Rock Paper Scissors");
            RPS_Loss_Emotes.Add(emote);
        }
        public void KazotskyKick()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Kazotsky Kick");
            CustomEmotesAPI.BlackListEmote("Kazotsky Kick");
            string emote;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Demo_Start" }, [Assets.Load<AudioClip>($"assets/audio dump/KazotskyKick.ogg")], true, new string[] { "KazotskyKick/Kazotsky_Demo_Loop" }, "Kazotsky Kick"); //names are wrong, should be Kazotsky_Sniper_Loop
            KazotskyKick_Emotes.Add(emote);
            int syncpos = BoneMapper.animClips[emote].syncPos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Engi_Start" }, [Assets.Load<AudioClip>($"assets/audio dump/KazotskyKick.ogg")], true, new string[] { "KazotskyKick/Kazotsky_Engi_Loop" }, "Kazotsky Kick");
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Heavy_Start" }, [Assets.Load<AudioClip>($"assets/audio dump/KazotskyKick.ogg")], true, new string[] { "KazotskyKick/Kazotsky_Heavy_Loop" }, "Kazotsky Kick");
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Medic_Start" }, [Assets.Load<AudioClip>($"assets/audio dump/KazotskyKick.ogg")], true, new string[] { "KazotskyKick/Kazotsky_Medic_Loop" }, "Kazotsky Kick");
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Pyro_Start" }, [Assets.Load<AudioClip>($"assets/audio dump/KazotskyKick.ogg")], true, new string[] { "KazotskyKick/Kazotsky_Pyro_Loop" }, "Kazotsky Kick");
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Scout_Start" }, [Assets.Load<AudioClip>($"assets/audio dump/KazotskyKick.ogg")], true, new string[] { "KazotskyKick/Kazotsky_Scout_Loop" }, "Kazotsky Kick");
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Sniper_Start" }, [Assets.Load<AudioClip>($"assets/audio dump/KazotskyKick.ogg")], true, new string[] { "KazotskyKick/Kazotsky_Sniper_Loop" }, "Kazotsky Kick");
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Soldier_Start" }, [Assets.Load<AudioClip>($"assets/audio dump/KazotskyKick.ogg")], true, new string[] { "KazotskyKick/Kazotsky_Soldier_Loop" }, "Kazotsky Kick");
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Spy_Start" }, [Assets.Load<AudioClip>($"assets/audio dump/KazotskyKick.ogg")], true, new string[] { "KazotskyKick/Kazotsky_Spy_Loop" }, "Kazotsky Kick");
            KazotskyKick_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
        }
        public void Conga()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Conga");
            CustomEmotesAPI.BlackListEmote("Conga");

            string emote;
            emote = AddHiddenAnimation(new string[] { "Conga/Demo_Conga" }, [Assets.Load<AudioClip>($"assets/audio dump/conga.ogg")], true, "Conga");
            Conga_Emotes.Add(emote);
            int syncpos = BoneMapper.animClips[emote].syncPos;
            emote = AddHiddenAnimation(new string[] { "Conga/Engi_Conga" }, [Assets.Load<AudioClip>($"assets/audio dump/conga.ogg")], true, "Conga");
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Heavy_Conga" }, [Assets.Load<AudioClip>($"assets/audio dump/conga.ogg")], true, "Conga");
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Medic_Conga" }, [Assets.Load<AudioClip>($"assets/audio dump/conga.ogg")], true, "Conga");
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Pyro_Conga" }, [Assets.Load<AudioClip>($"assets/audio dump/conga.ogg")], true, "Conga");
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Scout_Conga" }, [Assets.Load<AudioClip>($"assets/audio dump/conga.ogg")], true, "Conga");
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Sniper_Conga" }, [Assets.Load<AudioClip>($"assets/audio dump/conga.ogg")], true, "Conga");
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Soldier_Conga" }, [Assets.Load<AudioClip>($"assets/audio dump/conga.ogg")], true, "Conga");
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
            emote = AddHiddenAnimation(new string[] { "Conga/Spy_Conga" }, [Assets.Load<AudioClip>($"assets/audio dump/conga.ogg")], true, "Conga");
            Conga_Emotes.Add(emote);
            BoneMapper.animClips[emote].syncPos = syncpos;
        }

        private void CustomEmotesAPI_emoteSpotJoined_Body(GameObject emoteSpot, BoneMapper joiner, BoneMapper host)
        {
            string emoteSpotName = emoteSpot.name;
            if (emoteSpotName == "RPSJoinSpot" && joiner.local)
            {
                TF2Networker.instance.SyncEmoteToServerRpc(joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Join", GetMercNumber(), host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);

            }
            else if (emoteSpotName == "FlipJoinSpot" && joiner.local)
            {
                TF2Networker.instance.SyncEmoteToServerRpc(joiner.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Flip_Join", GetMercNumber(), host.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
        internal void StopAudioContainerStuff(BoneMapper mapper)
        {
            for (int i = 0; i < AudioContainerHolder.instance.currentContainers.Count; i++)
            {
                if (AudioContainerHolder.instance.currentContainers[i].mapper == mapper)
                {
                    foreach (var routine in AudioContainerHolder.instance.currentContainers[i].coroutines)
                    {
                        if (routine is not null)
                        {
                            AudioContainerHolder.instance.StopCoroutine(routine);
                        }
                    }
                    AudioContainerHolder.instance.currentContainers.RemoveAt(i);
                    i--;
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
            int targetAudioThing2 = -1;
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
                    targetAudioThing = _play_rancho;
                    break;
                case "Rock Paper Scissors":
                    if (mapper.local)
                    {
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Start", GetMercNumber(), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }

                    break;
                case "Flippin' Awesome":
                    if (mapper.local)
                    {
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Flip_Wait", GetMercNumber(), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    break;
                case "Conga":
                    if (mapper.local)
                    {
                        mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = "Medic_Conga";
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Conga_Start", GetMercNumber(), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    break;
                case "Kazotsky Kick":
                    if (mapper.local)
                    {
                        mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = "Medic_Kazotsky";
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Kazotsky_Start", GetMercNumber(), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    break;
                case "Schadenfreude":
                    if (mapper.local)
                    {
                        mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = "Medic_Laugh";
                        TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Laugh_Start", GetMercNumber(), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
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
                    targetAudioThing = _heavy_flip_waiting;
                    break;
                case "Heavy_Flip_Throw":
                    targetAudioThing = _heavy_flip_throw;
                    break;
                case "Heavy_Flip_Flip":
                    targetAudioThing = _heavy_flip_flip;
                    break;


                case "Medic_Flip_Start":
                    targetAudioThing = _medic_flip_waiting;
                    break;
                case "Medic_Flip_Throw":
                    targetAudioThing = _medic_flip_throw;
                    break;
                case "Medic_Flip_Flip":
                    targetAudioThing = _medic_flip_flip;
                    break;


                case "Pyro_Flip_Start":
                    targetAudioThing = _pyro_flip_waiting;
                    break;
                case "Pyro_Flip_Throw":
                    targetAudioThing = _pyro_flip_throw;
                    break;
                case "Pyro_Flip_Flip":
                    targetAudioThing = _pyro_flip_flip;
                    break;


                case "Scout_Flip_Start":
                    targetAudioThing = _scout_flip_waiting;
                    break;
                case "Scout_Flip_Throw":
                    targetAudioThing = _scout_flip_throw;
                    break;
                case "Scout_Flip_Flip":
                    targetAudioThing = _scout_flip_flip;
                    break;


                case "Sniper_Flip_Start":
                    targetAudioThing = _sniper_flip_waiting;
                    break;
                case "Sniper_Flip_Throw":
                    targetAudioThing = _sniper_flip_throw;
                    break;
                case "Sniper_Flip_Flip":
                    targetAudioThing = _sniper_flip_flip;
                    break;


                case "Soldier_Flip_Start":
                    targetAudioThing = _soldier_flip_waiting;
                    break;
                case "Soldier_Flip_Throw":
                    targetAudioThing = _soldier_flip_throw;
                    break;
                case "Soldier_Flip_Flip":
                    targetAudioThing = _soldier_flip_flip;
                    break;


                case "Spy_Flip_Start":
                    targetAudioThing = _spy_flip_waiting;
                    break;
                case "Spy_Flip_Throw":
                    targetAudioThing = _spy_flip_throw;
                    break;
                case "Spy_Flip_Flip":
                    targetAudioThing = _spy_flip_flip;
                    break;





                //RPS
                case "EngiRPSStart":
                    targetAudioThing = _rps_engi_initiate;
                    break;
                case "EngiRPS_RLose":
                case "EngiRPS_PLose":
                case "EngiRPS_SLose":
                    targetAudioThing = _rps_engi_loss;
                    break;
                case "EngiRPS_RWin":
                    targetAudioThing = _rps_engi_winrock;
                    break;
                case "EngiRPS_PWin":
                    targetAudioThing = _rps_engi_winpaper;
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
                    targetAudioThing = _rps_demo_winrock;
                    break;
                case "DemoRPS_PWin":
                    targetAudioThing = _rps_demo_winpaper;
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
                    targetAudioThing = _rps_heavy_winrock;
                    break;
                case "HeavyRPS_PWin":
                    targetAudioThing = _rps_heavy_winpaper;
                    break;
                case "HeavyRPS_SWin":
                    targetAudioThing = _rps_heavy_winscissors;
                    break;


                case "MedicRPS_Start":
                    targetAudioThing = _rps_medic_initiate;
                    break;
                case "MedicRPS_RLose":
                case "MedicRPS_PLose":
                case "MedicRPS_SLose":
                    targetAudioThing = _rps_medic_loss;
                    break;
                case "MedicRPS_RWin":
                    targetAudioThing = _rps_medic_winrock;
                    break;
                case "MedicRPS_PWin":
                    targetAudioThing = _rps_medic_winpaper;
                    break;
                case "MedicRPS_SWin":
                    targetAudioThing = _rps_medic_winscissors;
                    break;


                case "PyroRPS_Start":
                    targetAudioThing = _rps_pyro_initiate;
                    break;
                case "PyroRPS_RLose":
                case "PyroRPS_PLose":
                case "PyroRPS_SLose":
                    targetAudioThing = _rps_pyro_loss;
                    break;
                case "PyroRPS_RWin":
                    targetAudioThing = _rps_pyro_winrock;
                    break;
                case "PyroRPS_PWin":
                    targetAudioThing = _rps_pyro_winpaper;
                    break;
                case "PyroRPS_SWin":
                    targetAudioThing = _rps_pyro_winscissors;
                    break;


                case "ScoutRPS_Start":
                    targetAudioThing = _rps_scout_initiate;
                    break;
                case "ScoutRPS_RLose":
                    targetAudioThing = _rps_scout_lossrock;
                    break;
                case "ScoutRPS_PLose":
                case "ScoutRPS_SLose":
                    targetAudioThing = _rps_scout_loss;
                    break;
                case "ScoutRPS_RWin":
                    targetAudioThing = _rps_scout_winrock;
                    break;
                case "ScoutRPS_PWin":
                    targetAudioThing = _rps_scout_winpaper;
                    break;
                case "ScoutRPS_SWin":
                    targetAudioThing = _rps_scout_winscissors;
                    break;


                case "SniperRPS_Start":
                    AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, _rps_sniper_initiate_loop, mapper);
                    targetAudioThing = _rps_sniper_initiate_start;
                    break;
                case "SniperRPS_RLose":
                case "SniperRPS_PLose":
                case "SniperRPS_SLose":
                    targetAudioThing = _rps_sniper_loss;
                    break;
                case "SniperRPS_RWin":
                    targetAudioThing = _rps_sniper_winrock;
                    break;
                case "SniperRPS_PWin":
                    targetAudioThing = _rps_sniper_winpaper;
                    break;
                case "SniperRPS_SWin":
                    targetAudioThing = _rps_sniper_winscissors;
                    break;


                case "SoldierRPS_Start":
                    targetAudioThing = _rps_soldier_initiate;
                    break;
                case "SoldierRPS_RLose":
                case "SoldierRPS_PLose":
                case "SoldierRPS_SLose":
                    targetAudioThing = _rps_soldier_loss;
                    break;
                case "SoldierRPS_RWin":
                    targetAudioThing = _rps_soldier_winrock;
                    break;
                case "SoldierRPS_PWin":
                    targetAudioThing = _rps_soldier_winpaper;
                    break;
                case "SoldierRPS_SWin":
                    targetAudioThing = _rps_soldier_winscissors;
                    break;


                case "SpyRPS_Start":
                    targetAudioThing = _rps_spy_initiate;
                    break;
                case "SpyRPS_RLose":
                    RandomSpyWindup(mapper);
                    targetAudioThing = _rps_spy_lossrock;
                    break;
                case "SpyRPS_PLose":
                    RandomSpyWindup(mapper);
                    targetAudioThing = _rps_spy_losspaper;
                    break;
                case "SpyRPS_SLose":
                    RandomSpyWindup(mapper);
                    targetAudioThing = _rps_spy_lossscissors;
                    break;
                case "SpyRPS_RWin":
                    RandomSpyWindup(mapper);
                    targetAudioThing = _rps_spy_winrock;
                    break;
                case "SpyRPS_PWin":
                    RandomSpyWindup(mapper);
                    targetAudioThing = _rps_spy_winpaper;
                    break;
                case "SpyRPS_SWin":
                    RandomSpyWindup(mapper);
                    targetAudioThing = _rps_spy_winscissors;
                    break;



                case "Demo_Conga":
                    targetAudioThing2 = _conga_demo_start;
                    targetAudioThing = _conga_demo_loop;
                    break;
                case "Engi_Conga":
                    targetAudioThing2 = _conga_engi_start;
                    targetAudioThing = _conga_engi_loop;
                    break;
                case "Heavy_Conga":
                    targetAudioThing2 = _conga_heavy_start;
                    targetAudioThing = _conga_heavy_loop;
                    break;
                case "Medic_Conga":
                    targetAudioThing2 = _conga_medic_start;
                    targetAudioThing = _conga_medic_loop;
                    break;
                case "Pyro_Conga":
                    targetAudioThing2 = _conga_pyro_start;
                    targetAudioThing = _conga_pyro_loop;
                    break;
                case "Scout_Conga":
                    targetAudioThing2 = _conga_scout_start;
                    targetAudioThing = _conga_scout_loop;
                    break;
                case "Sniper_Conga":
                    targetAudioThing2 = _conga_sniper_start;
                    targetAudioThing = _conga_sniper_loop;
                    break;
                case "Soldier_Conga":
                    targetAudioThing2 = _conga_soldier_start;
                    targetAudioThing = _conga_soldier_loop;
                    break;
                case "Spy_Conga":
                    targetAudioThing2 = _conga_spy_start;
                    targetAudioThing = _conga_spy_loop;
                    break;

                default:
                    break;
            }
            if (targetAudioThing != -1)
            {
                AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, targetAudioThing, mapper);
            }
            if (targetAudioThing2 != -1)
            {
                AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, targetAudioThing2, mapper);
            }
            mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = newAnimation;
        }
        internal void RandomSpyWindup(BoneMapper mapper)
        {
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, _rps_spy_windup1, mapper);
                    break;
                case 1:
                    AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, _rps_spy_windup2, mapper);
                    break;
                case 2:
                    AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, _rps_spy_windup3, mapper);
                    break;
                default:
                    break;
            }
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

        internal void AddAnimation(string AnimClip, string wwise, bool looping, bool dimAudio, bool sync, string customName)
        {
            //CustomEmotesAPI.AddCustomAnimation(, looping, $"Play_{wwise}", $"Stop_{wwise}", dimWhenClose: dimAudio, syncAnim: sync, syncAudio: sync);
            AnimationClipParams clipParams = new AnimationClipParams
            {
                animationClip = [Assets.Load<AnimationClip>($"{AnimClip}.anim")],
                looping = looping,
                syncAnim = sync,
                syncAudio = sync,
                thirdPerson = true,
                displayName = customName
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            string emote = AnimClip.Split('/')[AnimClip.Split('/').Length - 1];
            BoneMapper.animClips[emote].vulnerableEmote = true;

        }

        internal void AddAnimation(string AnimClip, AudioClip[] audioClip, string AnimClip2ElectricBoogaloo, bool dimAudio, bool sync, string customName)
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
                _primaryDMCAFreeAudioClips = audioClip,
                displayName = customName
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            string emote = AnimClip.Split('/')[AnimClip.Split('/').Length - 1];
            BoneMapper.animClips[emote].vulnerableEmote = true;
        }
        internal void AddAnimation(string AnimClip, string wwise, string AnimClip2ElectricBoogaloo, bool dimAudio, bool sync, bool visibility, string customName)
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
                thirdPerson = true,
                displayName = customName
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            string emote = AnimClip.Split('/')[AnimClip.Split('/').Length - 1];
            BoneMapper.animClips[emote].vulnerableEmote = true;

        }
        internal string AddHiddenAnimation(string[] AnimClip, string[] AnimClip2ElectricBoogaloo, JoinSpot[] joinSpots, string customName)
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
                thirdPerson = true,
                displayName = customName
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
        internal string AddHiddenAnimation(string[] AnimClip, string customName)
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
                displayName = customName
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
        internal string AddHiddenAnimation(string[] AnimClip, AudioClip[] audioClips, string customName)
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
                _primaryDMCAFreeAudioClips = audioClips,
                displayName = customName
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
        internal string AddHiddenAnimation(string[] AnimClip, AudioClip[] audioClips, bool sync, string customName)
        {
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
                thirdPerson = true,
                _primaryAudioClips = audioClips,
                _primaryDMCAFreeAudioClips = audioClips,
                displayName = customName
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
        internal string AddHiddenAnimation(string[] AnimClip, AudioClip[] audioClips, bool sync, string[] AnimClip2, string customName)
        {
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
                looping = true,
                visible = false,
                syncAudio = sync,
                thirdPerson = true,
                _primaryAudioClips = audioClips,
                _primaryDMCAFreeAudioClips = audioClips,
                displayName = customName
            };
            CustomEmotesAPI.AddCustomAnimation(clipParams);
            BoneMapper.animClips[emote].vulnerableEmote = true;
            return emote;
        }
    }
}
