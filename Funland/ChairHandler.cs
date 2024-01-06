using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TitanFall2Emotes.IMissWwise;
using UnityEngine;

namespace TitanFall2Emotes
{
    class ChairHandler : MonoBehaviour
    {
        internal GameObject chair;
        bool check = true;
        internal Vector3 pos;
        internal Vector3 rot;
        internal Vector3 scal;
        bool check2 = false;
        bool check3 = false;
        internal BoneMapper mapper;
        float timer = 0;
        int whenToEmote = 0;

        void Start()
        {
            whenToEmote = UnityEngine.Random.Range(15, 25);
        }
        void Update()
        {
            if (check)
            {
                timer += Time.deltaTime;
            }
            if (timer > whenToEmote)
            {
                timer = 0;
                whenToEmote = UnityEngine.Random.Range(15, 25);
                switch (UnityEngine.Random.Range(0, 3))
                {
                    case 0:
                        mapper.preserveParent = true;
                        mapper.preserveProps = true;
                        DebugClass.Log($"burp");
                        mapper.PlayAnim("engiRanchoBurp", 0);
                        AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, TF2Plugin.Instance._play_ranchoburp, mapper);
                        break;
                    case 1:
                        mapper.preserveParent = true;
                        mapper.preserveProps = true;
                        DebugClass.Log($"bigDrink");
                        mapper.PlayAnim("engiRanchoBigDrink", 0);
                        AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, TF2Plugin.Instance._play_rancholong, mapper);
                        GetComponentInChildren<Animator>().SetBool("BigDrink", true);
                        break;
                    case 2:
                        mapper.preserveParent = true;
                        mapper.preserveProps = true;
                        DebugClass.Log($"quickDrink");
                        mapper.PlayAnim("engiRanchoQuickDrink", 0);
                        AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, TF2Plugin.Instance._play_ranchoquick, mapper);
                        GetComponentInChildren<Animator>().SetBool("SmallDrink", true);
                        break;
                    default:
                        break;
                }
            }
            else if (timer > 3)
            {
                GetComponentInChildren<Animator>().SetBool("BigDrink", false);
                GetComponentInChildren<Animator>().SetBool("SmallDrink", false);
            }
            if (check && !chair)
            {
                check = false;
                StartCoroutine(spinThenDestroy());
                GetComponentInChildren<Animator>().SetBool("Breaking", true);
                gameObject.transform.SetParent(null);
                gameObject.transform.position = pos;
                gameObject.transform.localEulerAngles = rot;
                gameObject.transform.localScale = scal;
                scal *= 1.333f;
                TF2Plugin.Instance.StopAudioContainerStuff(mapper);
                AudioContainerHolder.instance.PlayAudio(mapper.personalAudioSource, TF2Plugin.Instance._play_ranchoclose, mapper);
            }
            if (check3)
            {
                gameObject.transform.localScale = Vector3.Slerp(gameObject.transform.localScale, Vector3.zero, Time.deltaTime * 5);
            }
            else if (check2)
            {
                gameObject.transform.localScale = Vector3.Slerp(gameObject.transform.localScale, scal, Time.deltaTime * 30);
            }
        }

        IEnumerator spinThenDestroy()
        {
            yield return new WaitForSeconds(3.5f);
            check2 = true;
            yield return new WaitForSeconds(.15f);
            check3 = true;
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }
}
