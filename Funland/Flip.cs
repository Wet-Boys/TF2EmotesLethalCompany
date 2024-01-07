using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace TitanFall2Emotes
{
    class Flip : MonoBehaviour
    {
        public int charType;
        public static void FlipWait(BoneMapper joinerMapper, int spot)
        {

            joinerMapper.PlayAnim(TF2Plugin.Flip_Wait_Emotes[spot], 0);
            joinerMapper.props.Add(new GameObject());
            joinerMapper.props[0].AddComponent<Flip>().charType = spot;

            GameObject g = new GameObject();
            g.name = "Flip_WaitProp";
            joinerMapper.props.Add(g);
            g.transform.localPosition = joinerMapper.transform.position;
            g.transform.localEulerAngles = joinerMapper.transform.eulerAngles;
            g.transform.localScale = Vector3.one;
            g.transform.SetParent(joinerMapper.mapperBodyTransform.parent);
            joinerMapper.AssignParentGameObject(g, true, true, true, true, false);

        }
        public static void Flip_Flip(BoneMapper joinerMapper, int spot, BoneMapper hostJoinerMapper)
        {
            joinerMapper.PlayAnim(TF2Plugin.Flip_Flip_Emotes[spot], 0);

            GameObject g = new GameObject();
            g.name = "Flip_FlipProp";
            joinerMapper.props.Add(g);

            //Vector3 scale = hostJoinerMapper.transform.parent.localScale;
            //hostJoinerMapper.transform.parent.localScale = Vector3.one;
            g.transform.SetParent(hostJoinerMapper.transform);
            Vector3 scal = hostJoinerMapper.transform.lossyScale;
            g.transform.localPosition = new Vector3(0, 0, 1.95f / scal.z);
            g.transform.localEulerAngles = new Vector3(0, 180, 0);
            g.transform.localScale = Vector3.one;
            g.transform.SetParent(joinerMapper.mapperBodyTransform.parent);
            joinerMapper.AssignParentGameObject(g, true, true, true, true, true);
            //hostJoinerMapper.transform.parent.localScale = scale;
        }
        public static void Flip_Throw(BoneMapper joinerMapper, int spot, BoneMapper hostJoinerMapper)
        {
            joinerMapper.PlayAnim(TF2Plugin.Flip_Throw_Emotes[spot], 0);

            GameObject g = new GameObject();
            g.name = "Flip_ThrowProp";
            joinerMapper.props.Add(g);

            g.transform.localPosition = joinerMapper.transform.position;
            g.transform.localEulerAngles = joinerMapper.transform.eulerAngles;
            g.transform.localScale = Vector3.one;
            g.transform.SetParent(joinerMapper.mapperBodyTransform.parent);
            joinerMapper.AssignParentGameObject(g, true, true, true, true, false);
        }
        public static void Flip_Join(BoneMapper joinerMapper, int spot, BoneMapper hostJoinerMapper)
        {
            int hostSpot = hostJoinerMapper.props[0].GetComponent<Flip>().charType;
            int clientSpot = spot;

            TF2Networker.instance.SyncEmoteToClientRpc(hostJoinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Flip_Throw", hostSpot, hostJoinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
            TF2Networker.instance.SyncEmoteToClientRpc(joinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "Flip_Flip", clientSpot, hostJoinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }
}
