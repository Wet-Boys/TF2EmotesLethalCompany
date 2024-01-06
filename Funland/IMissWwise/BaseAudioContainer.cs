using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TitanFall2Emotes.IMissWwise
{
    //yes I'm just recreating what I lost when I lost wwise :(
    public class BaseAudioContainer : MonoBehaviour
    {
        public BoneMapper associatedMapper;
        public AudioClip clip;
        public float delay;
        public int targetContainer = 0;
        public List<BaseAudioContainer> childContainers = new List<BaseAudioContainer>();
        
        public virtual void Play()
        {
            associatedMapper.personalAudioSource.PlayOneShot(clip);
        }

        public bool IsPlaying()
        {
            if (childContainers.Count == 0)
            {
                return associatedMapper.personalAudioSource.isPlaying;
            }
            else
            {
                return childContainers[targetContainer].IsPlaying();
            }
        }
    }
}
