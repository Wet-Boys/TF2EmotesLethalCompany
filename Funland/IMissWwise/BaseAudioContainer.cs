using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TitanFall2Emotes.IMissWwise
{
    //yes I'm just recreating what I lost when I lost wwise :(
    public class BaseAudioContainer : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip clip;
        public float delay;
        public int targetContainer = 0;
        public List<BaseAudioContainer> childContainers = new List<BaseAudioContainer>();
        
        public virtual void Play()
        {
            audioSource.PlayOneShot(clip);//have some event here that will fire when the audiosource has finished playing?
        }

        public bool IsPlaying()
        {
            if (childContainers.Count == 0)
            {
                return audioSource.isPlaying;
            }
            else
            {
                return childContainers[targetContainer].IsPlaying();
            }
        }
    }
}
