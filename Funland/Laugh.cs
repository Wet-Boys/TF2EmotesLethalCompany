using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TitanFall2Emotes
{
    class Laugh : MonoBehaviour
    {
        public static void PlayLaugh(BoneMapper joinerMapper, int spot)
        {
            joinerMapper.PlayAnim(TF2Plugin.Laugh_Emotes[spot], 0);
        }
    }
}
