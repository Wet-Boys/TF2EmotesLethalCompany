
//using System;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Networking;

//namespace TitanFall2Emotes
//{
//    class SyncRandomEmoteToClient : INetMessage
//    {
//        NetworkInstanceId netId;
//        string name;
//        int spot;
//        NetworkInstanceId secondaryNetId;
//        public SyncRandomEmoteToClient()
//        {

//        }

//        public SyncRandomEmoteToClient(NetworkInstanceId netId, string name, int spot, NetworkInstanceId secondaryNetId)
//        {
//            this.netId = netId;
//            this.name = name;
//            this.spot = spot;
//            this.secondaryNetId = secondaryNetId;
//        }

//        public void Deserialize(NetworkReader reader)
//        {
//            netId = reader.ReadNetworkId();
//            name = reader.ReadString();
//            spot = reader.ReadInt32();
//            secondaryNetId = reader.ReadNetworkId();
//        }

//        public void OnReceived()
//        {
//            GameObject bodyObject = Util.FindNetworkObject(netId);
//            BoneMapper joinerMapper = bodyObject.GetComponent<ModelLocator>().modelTransform.GetComponentInChildren<BoneMapper>();
//            GameObject hostBodyObject = Util.FindNetworkObject(secondaryNetId);
//            BoneMapper hostJoinerMapper = hostBodyObject.GetComponent<ModelLocator>().modelTransform.GetComponentInChildren<BoneMapper>();
//            GameObject g;
//            int prop1;
//            TeamIndex joinerIndex = bodyObject.GetComponent<TeamComponent>().teamIndex;
//            TeamIndex hostIndex = hostBodyObject.GetComponent<TeamComponent>().teamIndex;
//            switch (name)
//            {
//                case "RPS_Start":
//                    RockPaperScissors.RPSStart(joinerMapper, spot);
//                    break;
//                case "RPS_Win":
//                    RockPaperScissors.RPSWin(joinerMapper, spot, hostJoinerMapper, joinerIndex, hostIndex);
//                    break;
//                case "RPS_Loss":
//                    RockPaperScissors.RPSLose(joinerMapper, spot, hostJoinerMapper, joinerIndex, hostIndex);
//                    break;
//                case "Flip_Wait":
//                    Flip.FlipWait(joinerMapper, spot);
//                    break;
//                case "Flip_Throw":
//                    Flip.Flip_Throw(joinerMapper, spot, hostJoinerMapper);
//                    break;
//                case "Flip_Flip":
//                    Flip.Flip_Flip(joinerMapper, spot, hostJoinerMapper);
//                    break;
//                case "Conga_Start":
//                    Conga.StartConga(joinerMapper, spot);
//                    break;
//                case "Kazotsky_Start":
//                    Kazotsky.StartKazotsky(joinerMapper, spot);
//                    break;
//                case "Laugh_Start":
//                    Laugh.PlayLaugh(joinerMapper, spot);
//                    break;
//                default:
//                    break;
//            }
//        }

//        public void Serialize(NetworkWriter writer)
//        {
//            writer.Write(netId);
//            writer.Write(name);
//            writer.Write(spot);
//            writer.Write(secondaryNetId);
//        }
//    }
//}
