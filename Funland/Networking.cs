using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using UnityEngine.Networking;
using UnityEngine;
using EmotesAPI;
using GameNetcodeStuff;
using Unity.Netcode;

namespace TitanFall2Emotes
{
    public class TF2Networker : Unity.Netcode.NetworkBehaviour
    {
        public static TF2Networker instance;
        private void Awake()
        {
            name = "TF2_Networker";
            instance = this;
        }
        [Unity.Netcode.ClientRpc]
        public void SyncEmoteToClientRpc(ulong netId, string name, int spot, ulong secondaryNetId)
        {
            GameObject bodyObject = GetNetworkObject(netId).gameObject;
            BoneMapper joinerMapper = bodyObject.GetComponentInChildren<BoneMapper>();
            GameObject hostBodyObject = GetNetworkObject(secondaryNetId).gameObject;
            BoneMapper hostJoinerMapper = hostBodyObject.GetComponentInChildren<BoneMapper>();
            bool joinerPlayer = bodyObject.GetComponents<PlayerControllerB>().Length == 1;
            bool hostPlayer = hostBodyObject.GetComponents<PlayerControllerB>().Length == 1;
            switch (name)
            {
                case "RPS_Start":
                    RockPaperScissors.RPSStart(joinerMapper, spot);
                    break;
                case "RPS_Win":
                    RockPaperScissors.RPSWin(joinerMapper, spot, hostJoinerMapper, joinerPlayer, hostPlayer);
                    break;
                case "RPS_Loss":
                    RockPaperScissors.RPSLose(joinerMapper, spot, hostJoinerMapper, joinerPlayer, hostPlayer);
                    break;
                case "Flip_Wait":
                    Flip.FlipWait(joinerMapper, spot);
                    break;
                case "Flip_Throw":
                    Flip.Flip_Throw(joinerMapper, spot, hostJoinerMapper);
                    break;
                case "Flip_Flip":
                    Flip.Flip_Flip(joinerMapper, spot, hostJoinerMapper);
                    break;
                case "Conga_Start":
                    Conga.StartConga(joinerMapper, spot);
                    break;
                case "Kazotsky_Start":
                    Kazotsky.StartKazotsky(joinerMapper, spot);
                    break;
                case "Laugh_Start":
                    Laugh.PlayLaugh(joinerMapper, spot);
                    break;
                default:
                    break;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncEmoteToServerRpc(ulong netId, string name, int spot, ulong secondaryNetId)
        {
            GameObject bodyObject = GetNetworkObject(netId).gameObject;
            BoneMapper joinerMapper = bodyObject.GetComponentInChildren<BoneMapper>();
            GameObject hostBodyObject = GetNetworkObject(secondaryNetId).gameObject;
            BoneMapper hostJoinerMapper = hostBodyObject.GetComponentInChildren<BoneMapper>();
            bool joinerPlayer = bodyObject.GetComponents<PlayerControllerB>().Length == 1;
            bool hostPlayer = hostBodyObject.GetComponents<PlayerControllerB>().Length == 1;
            switch (name)
            {
                case "RPS_Join":
                    RockPaperScissors.RPSJoin(joinerMapper, spot, hostJoinerMapper);
                    break;
                case "Flip_Join":
                    Flip.Flip_Join(joinerMapper, spot, hostJoinerMapper);
                    break;
                default:
                    SyncEmoteToClientRpc(netId, name, spot, secondaryNetId);
                    break;
            }
        }
    }
}
