using System;
using System.Collections.Generic;
using System.Text;

namespace TitanFall2Emotes.IMissWwise
{
    public class RandomContainer : BaseAudioContainer
    {
        public override void Play()
        {
            if (childContainers.Count != 0)
            {
                childContainers[UnityEngine.Random.Range(0, childContainers.Count)].Play();
            }
        }
    }
}
