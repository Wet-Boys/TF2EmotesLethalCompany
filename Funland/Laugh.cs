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

            GameObject g = new GameObject();
            g.name = "Laugh_HaltProp";
            joinerMapper.props.Add(g);
            g.transform.localPosition = joinerMapper.transform.position;
            g.transform.localEulerAngles = joinerMapper.transform.eulerAngles;
            g.transform.localScale = Vector3.one;
            g.transform.SetParent(joinerMapper.mapperBodyTransform.parent);
            joinerMapper.AssignParentGameObject(g, true, true, true, true, false);
        }
    }
}
