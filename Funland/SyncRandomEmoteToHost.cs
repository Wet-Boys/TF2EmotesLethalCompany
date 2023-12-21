//using R2API.Networking.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine.Networking;

//namespace TitanFall2Emotes
//{
//    class SyncRandomEmoteToHost : INetMessage
//    {
//        NetworkInstanceId netId;
//        string name;
//        int spot;
//        NetworkInstanceId secondaryNetId;
//        public SyncRandomEmoteToHost()
//        {

//        }

//        public SyncRandomEmoteToHost(NetworkInstanceId netId, string name, int spot, NetworkInstanceId secondaryNetId)
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
//            new SyncRandomEmoteToClient(netId, name, spot, secondaryNetId).Send(R2API.Networking.NetworkDestination.Clients);
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
