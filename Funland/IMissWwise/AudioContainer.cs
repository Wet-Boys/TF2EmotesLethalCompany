using EmotesAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TitanFall2Emotes.IMissWwise
{
    internal class AudioContainer
    {
        internal List<AudioClip[]> audioClips = [];
        internal List<float> delays = [];
        internal float repeatTimer = -1f;
    }
    internal class PlayingAudioContainer
    {
        internal BoneMapper mapper;
        internal List<IEnumerator> coroutines = new List<IEnumerator>();
    }
    internal class AudioContainerHolder : MonoBehaviour
    {
        internal static List<AudioContainer> allContainers = new List<AudioContainer>();
        public static AudioContainerHolder instance;
        internal List<PlayingAudioContainer> currentContainers = new List<PlayingAudioContainer>();
        private void Awake()
        {
            instance = this;
        }

        private IEnumerator PlayGroupOfAudio(AudioSource source, AudioContainer container, PlayingAudioContainer playingContainer)
        {
            for (int i = 0; i < container.audioClips.Count; i++)
            {
                playingContainer.coroutines.Add(PlayAfterDelay(container.audioClips[i], container.delays[i], source));
                StartCoroutine(playingContainer.coroutines[playingContainer.coroutines.Count - 1]);
            }
            if (container.repeatTimer > 0)
            {
                yield return new WaitForSeconds(container.repeatTimer);
                playingContainer.coroutines.Add(PlayGroupOfAudio(source, container, playingContainer));
                StartCoroutine(playingContainer.coroutines[playingContainer.coroutines.Count - 1]);
            }
        }
        private IEnumerator PlayAfterDelay(AudioClip[] clips, float delay, AudioSource source)
        {
            yield return new WaitForSeconds(delay);
            source.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
        }
        internal void PlayAudio(AudioSource source, AudioContainer container, BoneMapper targetMapper)
        {
            if (container is null)
            {
                DebugClass.Log($"tried to play audio but AudioContainer was null!");
                return;
            }
            PlayingAudioContainer c = new PlayingAudioContainer();
            c.mapper = targetMapper;
            c.coroutines.Add(PlayGroupOfAudio(source, container, c));
            StartCoroutine(c.coroutines[0]);
            currentContainers.Add(c);
        }





        internal AudioContainer GetContainer(int i)
        {
            try
            {
                return allContainers[i];
            }
            catch (Exception)
            {
                DebugClass.Log($"container not found");
                return null;
            }
        }
        internal void PlayAudio(AudioSource source, int container, BoneMapper targetMapper)
        {
            PlayAudio(source, GetContainer(container), targetMapper);
        }
        internal static int Setup(string[][] audioClipNames, List<float> delays, float repeatTimer)
        {
            List<AudioClip[]> clips = new List<AudioClip[]>();
            for (int i = 0; i < audioClipNames.Length; i++)
            {
                List<AudioClip> clips2 = new List<AudioClip>();
                foreach (var item in audioClipNames[i])
                {
                    clips2.Add(Assets.Load<AudioClip>($"audio dump/{item}.ogg"));
                }
                clips.Add(clips2.ToArray());
            }
            return Setup(clips, delays, repeatTimer);
        }
        internal static int Setup(List<AudioClip[]> audioClips, List<float> delays, float repeatTimer)
        {
            if (audioClips.Count != delays.Count)
            {
                DebugClass.Log($"audioClips are not equal to delays in length, you can't do that lmao");
                return -69;
            }
            AudioContainer container = new AudioContainer();
            container.audioClips = audioClips;
            container.delays = delays;
            container.repeatTimer = repeatTimer;
            allContainers.Add(container);
            return allContainers.Count - 1;
        }
    }
}
