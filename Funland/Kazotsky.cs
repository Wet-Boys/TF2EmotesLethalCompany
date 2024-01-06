using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TitanFall2Emotes
{
    class Kazotsky : MonoBehaviour
    {
        public static void StartKazotsky(BoneMapper joinerMapper, int spot)
        {
            joinerMapper.PlayAnim(TF2Plugin.KazotskyKick_Emotes[spot], 0);
            //joinerMapper.SetAutoWalk(2f, true, false);
        }
    }
}
