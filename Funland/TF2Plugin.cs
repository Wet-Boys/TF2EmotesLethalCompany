using BepInEx;
using BepInEx.Configuration;
using EmotesAPI;
using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
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
        public const string PluginVersion = "1.2.2";
        internal static List<string> Conga_Emotes = new List<string>();
        internal static List<string> KazotskyKick_Emotes = new List<string>();
        internal static List<string> RPS_Start_Emotes = new List<string>();
        internal static List<string> RPS_Loss_Emotes = new List<string>();
        internal static List<string> RPS_Win_Emotes = new List<string>();
        internal static List<string> Flip_Wait_Emotes = new List<string>();
        internal static List<string> Flip_Flip_Emotes = new List<string>();
        internal static List<string> Flip_Throw_Emotes = new List<string>();
        internal static List<string> Laugh_Emotes = new List<string>();
        public void Awake()
        {
            Instance = this;
            Assets.AddBundle($"tf2emotes");
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

        public void Rancho()
        {
            //TODO
            //AddAnimation("Engi/Rancho/RanchoRelaxo", "Rancho", "Engi/Rancho/engiRanchoPassive", false, false);
            //AddAnimation("Engi/Rancho/engiRanchoBurp", "", "Engi/Rancho/engiRanchoPassive", false, false, false);
            //AddAnimation("Engi/Rancho/engiRanchoBigDrink", "", "Engi/Rancho/engiRanchoPassive", false, false, false);
            //AddAnimation("Engi/Rancho/engiRanchoQuickDrink", "", "Engi/Rancho/engiRanchoPassive", false, false, false);
        }
        public void Laugh()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Schadenfreude");
            CustomEmotesAPI.BlackListEmote("Schadenfreude");
            //TODO
            //string emote = AddHiddenAnimation(new string[] { "Engi/Laugh/Engi_Laugh" }, new string[] { "Engi_Laugh" }, "Laugh");
            //Laugh_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Scout/Laugh/Scout_Laugh" }, new string[] { "Scout_Laugh" }, "Laugh");
            //Laugh_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Demo/Laugh/Demo_Laugh" }, new string[] { "Demo_Laugh" }, "Laugh");
            //Laugh_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Soldier/Laugh/Soldier_Laugh" }, new string[] { "Soldier_Laugh" }, "Laugh");
            //Laugh_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Pyro/Laugh/Pyro_Laugh" }, new string[] { "Pyro_Laugh" }, "Laugh");
            //Laugh_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Heavy/Laugh/Heavy_Laugh" }, new string[] { "Heavy_Laugh" }, "Laugh");
            //Laugh_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Medic/Laugh/Medic_Laugh" }, new string[] { "Medic_Laugh" }, "Laugh");
            //Laugh_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Sniper/Laugh/Sniper_Laugh" }, new string[] { "Sniper_Laugh" }, "Laugh");
            //Laugh_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Spy/Laugh/Spy_Laugh" }, new string[] { "Spy_Laugh" }, "Laugh");
            //Laugh_Emotes.Add(emote);

        }
        public void Flip()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Flippin' Awesome");
            CustomEmotesAPI.BlackListEmote("Flippin' Awesome");


            //TODO
            //string emote = AddHiddenAnimation(new string[] { "Demo/Flip/Demo_Flip_Start" }, new string[] { "Demo/Flip/Demo_Flip_Wait" }, new string[] { "Demo_Flip_Waiting" }, "Flip", new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            //Flip_Wait_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Demo/Flip/Demo_Flip_Throw" }, new string[] { "Demo_Flip_Throw" }, "");
            //Flip_Throw_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Demo/Flip/Demo_Flip_Flip" }, new string[] { "Demo_Flip_Flip" }, "");
            //Flip_Flip_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Engi/Flip/Engi_Flip_Start" }, new string[] { "Engi/Flip/Engi_Flip_Wait" }, new string[] { "Engi_Flip_Waiting" }, "Flip", new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            //Flip_Wait_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Engi/Flip/Engi_Flip_Throw" }, new string[] { "Engi_Flip_Throw" }, "");
            //Flip_Throw_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Engi/Flip/Engi_Flip_Flip" }, new string[] { "Engi_Flip_Flip" }, "");
            //Flip_Flip_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Medic/Flip/Medic_Flip_Start" }, new string[] { "Medic/Flip/Medic_Flip_Wait" }, new string[] { "Medic_Flip_Waiting" }, "Flip", new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            //Flip_Wait_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Medic/Flip/Medic_Flip_Throw" }, new string[] { "Medic_Flip_Throw" }, "");
            //Flip_Throw_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Medic/Flip/Medic_Flip_Flip" }, new string[] { "Medic_Flip_Flip" }, "");
            //Flip_Flip_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Heavy/Flip/Heavy_Flip_Start" }, new string[] { "Heavy/Flip/Heavy_Flip_Wait" }, new string[] { "Heavy_Flip_Waiting" }, "Flip", new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            //Flip_Wait_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Heavy/Flip/Heavy_Flip_Throw" }, new string[] { "Heavy_Flip_Throw" }, "");
            //Flip_Throw_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Heavy/Flip/Heavy_Flip_Flip" }, new string[] { "Heavy_Flip_Flip" }, "");
            //Flip_Flip_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Pyro/Flip/Pyro_Flip_Start" }, new string[] { "Pyro/Flip/Pyro_Flip_Wait" }, new string[] { "Pyro_Flip_Waiting" }, "Flip", new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            //Flip_Wait_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Pyro/Flip/Pyro_Flip_Throw" }, new string[] { "Pyro_Flip_Throw" }, "");
            //Flip_Throw_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Pyro/Flip/Pyro_Flip_Flip" }, new string[] { "Pyro_Flip_Flip" }, "");
            //Flip_Flip_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Scout/Flip/Scout_Flip_Start" }, new string[] { "Scout/Flip/Scout_Flip_Wait" }, new string[] { "Scout_Flip_Waiting" }, "Flip", new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            //Flip_Wait_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Scout/Flip/Scout_Flip_Throw" }, new string[] { "Scout_Flip_Throw" }, "");
            //Flip_Throw_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Scout/Flip/Scout_Flip_Flip" }, new string[] { "Scout_Flip_Flip" }, "");
            //Flip_Flip_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Sniper/Flip/Sniper_Flip_Start" }, new string[] { "Sniper/Flip/Sniper_Flip_Wait" }, new string[] { "Sniper_Flip_Waiting" }, "Flip", new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            //Flip_Wait_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Sniper/Flip/Sniper_Flip_Throw" }, new string[] { "Sniper_Flip_Throw" }, "");
            //Flip_Throw_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Sniper/Flip/Sniper_Flip_Flip" }, new string[] { "Sniper_Flip_Flip" }, "");
            //Flip_Flip_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Soldier/Flip/Soldier_Flip_Start" }, new string[] { "Soldier/Flip/Soldier_Flip_Wait" }, new string[] { "Soldier_Flip_Waiting" }, "Flip", new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            //Flip_Wait_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Soldier/Flip/Soldier_Flip_Throw" }, new string[] { "Soldier_Flip_Throw" }, "");
            //Flip_Throw_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Soldier/Flip/Soldier_Flip_Flip" }, new string[] { "Soldier_Flip_Flip" }, "");
            //Flip_Flip_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Spy/Flip/Spy_Flip_Start" }, new string[] { "Spy/Flip/Spy_Flip_Wait" }, new string[] { "Spy_Flip_Waiting" }, "Flip", new JoinSpot[] { new JoinSpot("FlipJoinSpot", new Vector3(0, 0, 1.5f)) });
            //Flip_Wait_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Spy/Flip/Spy_Flip_Throw" }, new string[] { "Spy_Flip_Throw" }, "");
            //Flip_Throw_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Spy/Flip/Spy_Flip_Flip" }, new string[] { "Spy_Flip_Flip" }, "");
            //Flip_Flip_Emotes.Add(emote);
        }
        public void RPS()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Rock Paper Scissors");
            CustomEmotesAPI.BlackListEmote("Rock Paper Scissors");
            CustomEmotesAPI.AddNonAnimatingEmote("Rock", false);
            CustomEmotesAPI.AddNonAnimatingEmote("Paper", false);
            CustomEmotesAPI.AddNonAnimatingEmote("Scissors", false);
            //TODO
            //string emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPSStart" }, new string[] { "Engi/RPS/EngiRPSLoop" }, new string[] { "RPS_Engi_Initiate" }, "RPS", new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            //RPS_Start_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_RWin" }, new string[] { "RPS_Engi_WinRock" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_RLose" }, new string[] { "RPS_Engi_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_PWin" }, new string[] { "RPS_Engi_WinPaper" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_PLose" }, new string[] { "RPS_Engi_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_SWin" }, new string[] { "RPS_Engi_WinScissors" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Engi/RPS/EngiRPS_SLose" }, new string[] { "RPS_Engi_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_Start" }, new string[] { "Demo/RPS/DemoRPS_Loop" }, new string[] { "RPS_Demo_Initiate" }, "RPS", new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            //RPS_Start_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_RWin" }, new string[] { "RPS_Demo_WinRock" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_RLose" }, new string[] { "RPS_Demo_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_PWin" }, new string[] { "RPS_Demo_WinPaper" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_PLose" }, new string[] { "RPS_Demo_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_SWin" }, new string[] { "RPS_Demo_WinScissors" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Demo/RPS/DemoRPS_SLose" }, new string[] { "RPS_Demo_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_Start" }, new string[] { "Soldier/RPS/SoldierRPS_Loop" }, new string[] { "RPS_Soldier_Initiate" }, "RPS", new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            //RPS_Start_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_RWin" }, new string[] { "RPS_Soldier_WinRock" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_RLose" }, new string[] { "RPS_Soldier_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_PWin" }, new string[] { "RPS_Soldier_WinPaper" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_PLose" }, new string[] { "RPS_Soldier_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_SWin" }, new string[] { "RPS_Soldier_WinScissors" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Soldier/RPS/SoldierRPS_SLose" }, new string[] { "RPS_Soldier_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_Start" }, new string[] { "Heavy/RPS/HeavyRPS_Loop" }, new string[] { "RPS_Heavy_Initiate" }, "RPS", new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            //RPS_Start_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_RWin" }, new string[] { "RPS_Heavy_WinRock" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_RLose" }, new string[] { "RPS_Heavy_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_PWin" }, new string[] { "RPS_Heavy_WinPaper" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_PLose" }, new string[] { "RPS_Heavy_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_SWin" }, new string[] { "RPS_Heavy_WinScissors" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Heavy/RPS/HeavyRPS_SLose" }, new string[] { "RPS_Heavy_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_Start" }, new string[] { "Medic/RPS/MedicRPS_Loop" }, new string[] { "RPS_Medic_Initiate" }, "RPS", new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            //RPS_Start_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_RWin" }, new string[] { "RPS_Medic_WinRock" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_RLose" }, new string[] { "RPS_Medic_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_PWin" }, new string[] { "RPS_Medic_WinPaper" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_PLose" }, new string[] { "RPS_Medic_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_SWin" }, new string[] { "RPS_Medic_WinScissors" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Medic/RPS/MedicRPS_SLose" }, new string[] { "RPS_Medic_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_Start" }, new string[] { "Pyro/RPS/PyroRPS_Loop" }, new string[] { "RPS_Pyro_Initiate" }, "RPS", new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            //RPS_Start_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_RWin" }, new string[] { "RPS_Pyro_WinRock" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_RLose" }, new string[] { "RPS_Pyro_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_PWin" }, new string[] { "RPS_Pyro_WinPaper" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_PLose" }, new string[] { "RPS_Pyro_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_SWin" }, new string[] { "RPS_Pyro_WinScissors" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Pyro/RPS/PyroRPS_SLose" }, new string[] { "RPS_Pyro_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_Start" }, new string[] { "Scout/RPS/ScoutRPS_Loop" }, new string[] { "RPS_Scout_Initiate" }, "RPS", new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            //RPS_Start_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_RWin" }, new string[] { "RPS_Scout_WinRock" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_RLose" }, new string[] { "RPS_Scout_LossRock" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_PWin" }, new string[] { "RPS_Scout_WinPaper" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_PLose" }, new string[] { "RPS_Scout_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_SWin" }, new string[] { "RPS_Scout_WinScissors" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Scout/RPS/ScoutRPS_SLose" }, new string[] { "RPS_Scout_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_Start" }, new string[] { "Sniper/RPS/SniperRPS_Loop" }, new string[] { "RPS_Sniper_Initiate" }, "RPS", new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            //RPS_Start_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_RWin" }, new string[] { "RPS_Sniper_WinRock" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_RLose" }, new string[] { "RPS_Sniper_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_PWin" }, new string[] { "RPS_Sniper_WinPaper" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_PLose" }, new string[] { "RPS_Sniper_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_SWin" }, new string[] { "RPS_Sniper_WinScissors" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Sniper/RPS/SniperRPS_SLose" }, new string[] { "RPS_Sniper_Loss" }, "");
            //RPS_Loss_Emotes.Add(emote);


            //emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_Start" }, new string[] { "Spy/RPS/SpyRPS_Loop" }, new string[] { "RPS_Spy_Initiate" }, "RPS", new JoinSpot[] { new JoinSpot("RPSJoinSpot", new Vector3(0, 0, 1.5f)) });
            //RPS_Start_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_RWin" }, new string[] { "RPS_Spy_WinRock" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_RLose" }, new string[] { "RPS_Spy_LossRock" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_PWin" }, new string[] { "RPS_Spy_WinPaper" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_PLose" }, new string[] { "RPS_Spy_LossPaper" }, "");
            //RPS_Loss_Emotes.Add(emote);

            //emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_SWin" }, new string[] { "RPS_Spy_WinScissors" }, "");
            //RPS_Win_Emotes.Add(emote);
            //emote = AddHiddenAnimation(new string[] { "Spy/RPS/SpyRPS_SLose" }, new string[] { "RPS_Spy_LossScissors" }, "");
            //RPS_Loss_Emotes.Add(emote);
        }
        public void KazotskyKick()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Kazotsky Kick");
            CustomEmotesAPI.BlackListEmote("Kazotsky Kick");
            //TODO
            //string emote;
            //emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Demo_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Demo_Loop" }); //names are wrong, should be Kazotsky_Sniper_Loop
            //KazotskyKick_Emotes.Add(emote);
            //int syncpos = BoneMapper.animClips[emote].syncPos;
            //emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Engi_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Engi_Loop" });
            //KazotskyKick_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Heavy_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Heavy_Loop" });
            //KazotskyKick_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Medic_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Medic_Loop" });
            //KazotskyKick_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Pyro_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Pyro_Loop" });
            //KazotskyKick_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Scout_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Scout_Loop" });
            //KazotskyKick_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Sniper_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Sniper_Loop" });
            //KazotskyKick_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Soldier_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Soldier_Loop" });
            //KazotskyKick_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "KazotskyKick/Kazotsky_Spy_Start" }, new string[] { "Kazotsky" }, "Kazotsky", true, new string[] { "KazotskyKick/Kazotsky_Spy_Loop" });
            //KazotskyKick_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
        }
        public void Conga()
        {
            CustomEmotesAPI.AddNonAnimatingEmote("Conga");
            CustomEmotesAPI.BlackListEmote("Conga");

            //TODO
            //string emote;
            //emote = AddHiddenAnimation(new string[] { "Conga/Demo_Conga" }, new string[] { "Conga" }, "Conga", true);
            //Conga_Emotes.Add(emote);
            //int syncpos = BoneMapper.animClips[emote].syncPos;
            //emote = AddHiddenAnimation(new string[] { "Conga/Engi_Conga" }, new string[] { "Conga" }, "Conga", true);
            //Conga_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "Conga/Heavy_Conga" }, new string[] { "Conga" }, "Conga", true);
            //Conga_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "Conga/Medic_Conga" }, new string[] { "Conga" }, "Conga", true);
            //Conga_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "Conga/Pyro_Conga" }, new string[] { "Conga" }, "Conga", true);
            //Conga_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "Conga/Scout_Conga" }, new string[] { "Conga" }, "Conga", true);
            //Conga_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "Conga/Sniper_Conga" }, new string[] { "Conga" }, "Conga", true);
            //Conga_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "Conga/Soldier_Conga" }, new string[] { "Conga" }, "Conga", true);
            //Conga_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
            //emote = AddHiddenAnimation(new string[] { "Conga/Spy_Conga" }, new string[] { "Conga" }, "Conga", true);
            //Conga_Emotes.Add(emote);
            //BoneMapper.animClips[emote].syncPos = syncpos;
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

        private void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            if (!mapper.gameObject.GetComponent<TF2EmoteTracker>())
            {
                mapper.gameObject.AddComponent<TF2EmoteTracker>();
            }
            if (newAnimation == "RanchoRelaxo")
            {
                GameObject g = GameObject.Instantiate(Assets.Load<GameObject>("@BadAssEmotes_badassemotes:Assets/Engi/Rancho/RanchoRelaxo.prefab"));
                mapper.props.Add(g);
                g.transform.SetParent(mapper.transform.parent);
                g.transform.localEulerAngles = Vector3.one;
                g.transform.localPosition = Vector3.zero;
                g.transform.localScale = new Vector3(1.175f, 1.175f, 1.175f);
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
            }


            if (newAnimation == "Rock Paper Scissors")
            {
                if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                {
                    TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Start", UnityEngine.Random.Range(0, RPS_Start_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
            if (newAnimation == "Flippin' Awesome")
            {
                if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                {
                    TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Flip_Wait", UnityEngine.Random.Range(0, Flip_Wait_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
            if (newAnimation == "Conga")
            {
                if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                {
                    mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = "Medic_Conga";
                    TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Conga_Start", UnityEngine.Random.Range(0, Conga_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
            if (newAnimation == "Kazotsky Kick")
            {
                if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                {
                    mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = "Medic_Kazotsky";
                    TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Kazotsky_Start", UnityEngine.Random.Range(0, KazotskyKick_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
            if (newAnimation == "Schadenfreude")
            {
                if (TF2Networker.instance.IsOwner && TF2Networker.instance.IsServer)
                {
                    mapper.gameObject.GetComponent<TF2EmoteTracker>().currentAnimation = "Medic_Laugh";
                    TF2Networker.instance.SyncEmoteToServerRpc(mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Laugh_Start", UnityEngine.Random.Range(0, Laugh_Emotes.Count), mapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                }
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
        //internal void AddAnimation(string AnimClip, string wwise, bool looping, bool dimAudio, bool sync)
        //{
        //    CustomEmotesAPI.AddCustomAnimation(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{AnimClip}.anim"), looping, $"Play_{wwise}", $"Stop_{wwise}", dimWhenClose: dimAudio, syncAnim: sync, syncAudio: sync);
        //    string emote = AnimClip.Split('/')[AnimClip.Split('/').Length - 1];
        //    BoneMapper.animClips[emote].vulnerableEmote = true;

        //}

        //internal void AddAnimation(string AnimClip, string wwise, string AnimClip2ElectricBoogaloo, bool dimAudio, bool sync)
        //{
        //    CustomEmotesAPI.AddCustomAnimation(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{AnimClip}.anim"), true, $"Play_{wwise}", $"Stop_{wwise}", secondaryAnimation: Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{AnimClip2ElectricBoogaloo}.anim"), dimWhenClose: dimAudio, syncAnim: sync, syncAudio: sync);
        //    string emote = AnimClip.Split('/')[AnimClip.Split('/').Length - 1];
        //    CustomEmotesAPI.BlackListEmote(emote);
        //    BoneMapper.animClips[emote].vulnerableEmote = true;
        //}
        //internal void AddAnimation(string AnimClip, string wwise, string AnimClip2ElectricBoogaloo, bool dimAudio, bool sync, bool visibility)
        //{
        //    CustomEmotesAPI.AddCustomAnimation(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{AnimClip}.anim"), true, $"Play_{wwise}", $"Stop_{wwise}", secondaryAnimation: Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{AnimClip2ElectricBoogaloo}.anim"), dimWhenClose: dimAudio, syncAnim: sync, syncAudio: sync, visible: visibility);
        //    string emote = AnimClip.Split('/')[AnimClip.Split('/').Length - 1];
        //    BoneMapper.animClips[emote].vulnerableEmote = true;

        //}
        //internal string AddHiddenAnimation(string[] AnimClip, string[] AnimClip2ElectricBoogaloo, string[] wwise, string stopWwise, JoinSpot[] joinSpots)
        //{
        //    List<string> stopwwise = new List<string>();
        //    foreach (var item in wwise)
        //    {
        //        stopwwise.Add($"Stop_{stopWwise}");
        //    }
        //    List<AnimationClip> primary = new List<AnimationClip>();
        //    foreach (var item in AnimClip)
        //    {
        //        primary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
        //    }
        //    List<AnimationClip> secondary = new List<AnimationClip>();
        //    foreach (var item in AnimClip2ElectricBoogaloo)
        //    {
        //        secondary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
        //    }
        //    string emote = AnimClip[0].Split('/')[AnimClip[0].Split('/').Length - 1]; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ;
        //    CustomEmotesAPI.AddCustomAnimation(primary.ToArray(), true, wwise, stopwwise.ToArray(), secondaryAnimation: secondary.ToArray(), joinSpots: joinSpots, visible: false);
        //    BoneMapper.animClips[emote].vulnerableEmote = true;
        //    return emote;
        //}
        //internal string AddHiddenAnimation(string[] AnimClip, string[] wwise, string stopWwise)
        //{

        //    List<string> stopwwise = new List<string>();
        //    foreach (var item in wwise)
        //    {
        //        stopwwise.Add($"Stop_{stopWwise}");
        //    }
        //    List<AnimationClip> primary = new List<AnimationClip>();
        //    foreach (var item in AnimClip)
        //    {
        //        primary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
        //    }
        //    string emote = AnimClip[0].Split('/')[AnimClip[0].Split('/').Length - 1]; ;
        //    CustomEmotesAPI.AddCustomAnimation(primary.ToArray(), false, wwise, stopwwise.ToArray(), visible: false);
        //    BoneMapper.animClips[emote].vulnerableEmote = true;
        //    return emote;
        //}
        //internal string AddHiddenAnimation(string[] AnimClip, string[] wwise, string stopWwise, bool sync)
        //{
        //    List<string> stopwwise = new List<string>();
        //    foreach (var item in wwise)
        //    {
        //        stopwwise.Add($"Stop_{stopWwise}");
        //    }
        //    List<AnimationClip> primary = new List<AnimationClip>();
        //    foreach (var item in AnimClip)
        //    {
        //        primary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
        //    }
        //    string emote = AnimClip[0].Split('/')[AnimClip[0].Split('/').Length - 1]; ;
        //    CustomEmotesAPI.AddCustomAnimation(primary.ToArray(), true, wwise, stopwwise.ToArray(), visible: false, syncAudio: sync);
        //    BoneMapper.animClips[emote].vulnerableEmote = true;
        //    return emote;
        //}
        //internal string AddHiddenAnimation(string[] AnimClip, string[] wwise, string stopWwise, bool sync, string[] AnimClip2)
        //{
        //    List<string> stopwwise = new List<string>();
        //    foreach (var item in wwise)
        //    {
        //        stopwwise.Add($"Stop_{stopWwise}");
        //    }
        //    List<AnimationClip> primary = new List<AnimationClip>();
        //    foreach (var item in AnimClip)
        //    {
        //        primary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
        //    }
        //    List<AnimationClip> secondary = new List<AnimationClip>();
        //    foreach (var item in AnimClip2)
        //    {
        //        secondary.Add(Assets.Load<AnimationClip>($"@ExampleEmotePlugin_badassemotes:assets/{item}.anim"));
        //    }
        //    string emote = AnimClip[0].Split('/')[AnimClip[0].Split('/').Length - 1]; ;
        //    CustomEmotesAPI.AddCustomAnimation(primary.ToArray(), false, wwise, stopwwise.ToArray(), visible: false, syncAudio: sync, secondaryAnimation: secondary.ToArray());
        //    BoneMapper.animClips[emote].vulnerableEmote = true;
        //    return emote;
        //}
    }
}
