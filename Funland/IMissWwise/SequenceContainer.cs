using System;
using System.Collections.Generic;
using System.Text;

namespace TitanFall2Emotes.IMissWwise
{
    public class SequenceContainer : BaseAudioContainer
    {
        enum PlayMode
        {
            Step,
            Continuous
        }
        bool needToIncrementContainerOnChildEnd = false;

        public override void Play()
        {
            if (childContainers.Count != 0)
            {
                if (targetContainer == childContainers.Count)
                {
                    targetContainer = 0;
                }
                childContainers[targetContainer].Play();
                needToIncrementContainerOnChildEnd = true;
            }
        }

        private void Update()
        {
            if (IsPlaying()) //do not do this wtf, just subscribe to the child event's Finished playing task
            {

            }
        }
    }
}
