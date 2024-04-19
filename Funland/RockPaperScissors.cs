using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace TitanFall2Emotes
{
    class RockPaperScissors : MonoBehaviour
    {
        public int charType;
        public static void RPSStart(BoneMapper joinerMapper, int spot)
        {
            joinerMapper.PlayAnim(TF2Plugin.RPS_Start_Emotes[spot], 0);
            joinerMapper.props.Add(new GameObject());
            joinerMapper.props[0].AddComponent<RockPaperScissors>().charType = spot;
        }
        public static void RPSWin(BoneMapper joinerMapper, int spot, BoneMapper hostJoinerMapper, bool joinerIsEnemy)
        {
            joinerMapper.PlayAnim(TF2Plugin.RPS_Win_Emotes[spot], 0);

            GameObject g = new GameObject();
            g.name = "RPS_WinProp";
            joinerMapper.props.Add(g);
            if (hostJoinerMapper != joinerMapper)
            {
                g.transform.SetParent(hostJoinerMapper.transform);
                Vector3 scal = hostJoinerMapper.transform.lossyScale;
                g.transform.localPosition = new Vector3(0, 0, 2.5f / scal.z);
                g.transform.localEulerAngles = new Vector3(0, 180, 0);
                g.transform.localScale = Vector3.one;
                g.transform.SetParent(joinerMapper.mapperBodyTransform.parent);
                joinerMapper.AssignParentGameObject(g, true, true, false, false, true);
            }
            else
            {
                g.transform.localPosition = joinerMapper.transform.position;
                g.transform.localEulerAngles = joinerMapper.transform.eulerAngles;
                g.transform.localScale = Vector3.one;
                g.transform.SetParent(joinerMapper.mapperBodyTransform.parent);
                joinerMapper.AssignParentGameObject(g, true, true, false, false, false);
            }

            string Team = "Red";
            if (joinerIsEnemy)
            {
                Team = "Blu";
                //TODO Lore accurate
                //TF2Plugin.Instance.KillAfterSecondsNotIEnumerator(hostJoinerMapper, 6.5f);
            }
            //0 rock
            //1 paper
            //2 scissors
            int prop1 = joinerMapper.props.Count;
            switch (spot % 3)
            {
                case 0:
                    joinerMapper.props.Add(GameObject.Instantiate(Assets.Load<GameObject>($"@BadAssEmotes_badassemotes:Assets/RPS/{Team}_Rock_Win.prefab")));
                    break;
                case 1:
                    joinerMapper.props.Add(GameObject.Instantiate(Assets.Load<GameObject>($"@BadAssEmotes_badassemotes:Assets/RPS/{Team}_Paper_Win.prefab")));
                    break;
                case 2:
                    joinerMapper.props.Add(GameObject.Instantiate(Assets.Load<GameObject>($"@BadAssEmotes_badassemotes:Assets/RPS/{Team}_Scissors_Win.prefab")));
                    break;
                default:
                    break;
            }
            joinerMapper.props[prop1].transform.SetParent(joinerMapper.parentGameObject.transform);
            joinerMapper.props[prop1].transform.localEulerAngles = Vector3.zero;
            joinerMapper.props[prop1].transform.localPosition = new Vector3(0, 2.5f * joinerMapper.props[prop1].transform.lossyScale.y, 0);
            joinerMapper.ScaleProps();
        }
        public static void RPSLose(BoneMapper joinerMapper, int spot, BoneMapper hostJoinerMapper, bool joinerIsEnemy, bool hostAndJoinerAreDifferentTeams)
        {
            joinerMapper.PlayAnim(TF2Plugin.RPS_Loss_Emotes[spot], 0);

            GameObject g = new GameObject();
            g.name = "RPS_LossProp";
            joinerMapper.props.Add(g);
            if (hostJoinerMapper != joinerMapper)
            {
                g.transform.SetParent(hostJoinerMapper.transform);
                Vector3 scal = hostJoinerMapper.transform.lossyScale;
                g.transform.localPosition = new Vector3(0, 0, 2.5f / scal.z);
                g.transform.localEulerAngles = new Vector3(0, 180, 0);
                g.transform.localScale = Vector3.one;
                g.transform.SetParent(joinerMapper.mapperBodyTransform.parent);
                joinerMapper.AssignParentGameObject(g, true, true, false, false, true);
            }
            else
            {
                g.transform.localPosition = joinerMapper.transform.position;
                g.transform.localEulerAngles = joinerMapper.transform.eulerAngles;
                g.transform.localScale = Vector3.one;
                g.transform.SetParent(joinerMapper.mapperBodyTransform.parent);
                joinerMapper.AssignParentGameObject(g, true, true, false, false, false);
            }

            string Team2 = "Red";
            if (joinerIsEnemy)
            {
                Team2 = "Blu";
            }
            if (hostAndJoinerAreDifferentTeams)
            {
                //TODO lore accurate
                //TF2Plugin.Instance.KillAfterSecondsNotIEnumerator(joinerMapper, 6.5f);
            }
            //0 rock
            //1 paper
            //2 scissors
            int prop1 = joinerMapper.props.Count;
            switch (spot % 3)
            {
                case 0:
                    joinerMapper.props.Add(GameObject.Instantiate(Assets.Load<GameObject>($"@BadAssEmotes_badassemotes:Assets/RPS/{Team2}_Rock_Lose.prefab")));
                    break;
                case 1:
                    joinerMapper.props.Add(GameObject.Instantiate(Assets.Load<GameObject>($"@BadAssEmotes_badassemotes:Assets/RPS/{Team2}_Paper_Lose.prefab")));
                    break;
                case 2:
                    joinerMapper.props.Add(GameObject.Instantiate(Assets.Load<GameObject>($"@BadAssEmotes_badassemotes:Assets/RPS/{Team2}_Scissors_Lose.prefab")));
                    break;
                default:
                    break;
            }
            joinerMapper.props[prop1].transform.SetParent(joinerMapper.parentGameObject.transform);
            joinerMapper.props[prop1].transform.localEulerAngles = Vector3.zero;
            joinerMapper.props[prop1].transform.localPosition = new Vector3(0, 2.5f * joinerMapper.props[prop1].transform.lossyScale.y, 0);
            joinerMapper.ScaleProps();
        }
        public static void RPSJoin(BoneMapper joinerMapper, int spot, BoneMapper hostJoinerMapper)
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

            hostSpot += hostJoinerMapper.props[0].GetComponent<RockPaperScissors>().charType * 3;
            clientSpot += spot * 3;

            if (winner == 0)
            {
                TF2Networker.instance.SyncEmoteToClientRpc(hostJoinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Win", hostSpot, hostJoinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                TF2Networker.instance.SyncEmoteToClientRpc(joinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Loss", clientSpot, hostJoinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
            }
            else
            {
                TF2Networker.instance.SyncEmoteToClientRpc(hostJoinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Loss", hostSpot, hostJoinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
                TF2Networker.instance.SyncEmoteToClientRpc(joinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId, "RPS_Win", clientSpot, hostJoinerMapper.mapperBody.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }
}
