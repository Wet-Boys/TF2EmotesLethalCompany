using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TitanFall2Emotes
{
    class Conga : MonoBehaviour
    {
        public string akSound;
        public static void StartConga(BoneMapper joinerMapper, int spot)
        {
            joinerMapper.PlayAnim(TF2Plugin.Conga_Emotes[spot], 0);
            GameObject g = new GameObject();
            g.transform.parent = joinerMapper.gameObject.transform;
            g.transform.localPosition = Vector3.zero;
            string aksound = "";
            switch (spot)
            {
                case 0:
                    aksound = "Conga_Demo";
                    break;
                case 1:
                    aksound = "Conga_Engi";
                    break;
                case 2:
                    aksound = "Conga_Heavy";
                    break;
                case 3:
                    aksound = "Conga_Medic";
                    break;
                case 4:
                    aksound = "Conga_Pyro";
                    break;
                case 5:
                    aksound = "Conga_Scout";
                    break;
                case 6:
                    aksound = "Conga_Sniper";
                    break;
                case 7:
                    aksound = "Conga_Soldier";
                    break;
                case 8:
                    aksound = "Conga_Spy";
                    break;
                default:
                    break;
            }
            g.AddComponent<Conga>().akSound = aksound;
            joinerMapper.props.Add(g);
            joinerMapper.SetAutoWalk(.2f, true);
        }
        void Start()
        {
            //TODO
            //AkSoundEngine.PostEvent(akSound, this.gameObject);
        }
        void OnDestroy()
        {
            //TODO
            //AkSoundEngine.StopAll(this.gameObject);
        }
    }
}
