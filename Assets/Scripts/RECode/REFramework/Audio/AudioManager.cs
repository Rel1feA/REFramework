using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace RECode.REFramework
{
    public enum AudioType
    {
        BGM,
        SE
    }

    public class AudioManager : MonoSingleton<AudioManager>
    {
        public List<Sound> sounds;
        private Dictionary<string, Sound> soundDics = new Dictionary<string, Sound>();
        private Sound currentBGM;
        [SerializeField]
        private AudioMixer mixer;

        protected override void Awake()
        {
            base.Awake();
            InitData();
        }

        private void InitData()
        {
            foreach (Sound sound in sounds)
            {
                GameObject obj = new GameObject(sound.clip.name);
                obj.transform.SetParent(transform);
                AudioSource source = obj.AddComponent<AudioSource>();
                source.clip = sound.clip;
                source.playOnAwake = sound.playOnAwake;
                source.loop = sound.loop;
                source.volume = sound.volume;
                source.outputAudioMixerGroup = sound.mixerGroup;
                if (source.playOnAwake)
                {
                    source.Play();
                }
                sound.audioSource = source;
                soundDics.Add(sound.clip.name, sound);
            }
        }

        public void PlayAudio(string name)
        {
            if (!soundDics.ContainsKey(name))
            {
                Debug.LogError($"未找到名为{name}的音频片段");
                return;
            }
            else
            {
                if (soundDics[name].audioType == AudioType.BGM)
                {
                    if (currentBGM != null)
                    {
                        StopAudio(currentBGM.clip.name);
                    }
                    currentBGM = soundDics[name];
                }
                soundDics[name].audioSource.Play();
            }
        }

        public void StopAudio(string name)
        {
            if (!soundDics.ContainsKey(name))
            {
                Debug.LogError($"未找到名为{name}的音频片段");
                return;
            }
            else
            {
                soundDics[name].audioSource.Stop();
            }
        }

        public Sound FindSoundByName(string name)
        {
            foreach (Sound sound in sounds)
            {
                if (sound.clip.name == name)
                {
                    return sound;
                }
            }
            Debug.LogError($"未找到名为{name}的音频片段");
            return null;
        }

        //使用以下方法时请记得去将AudioMixer的参数暴露出来以供设置
        public void ChangeValue(float value,string valueName)
        {
            mixer.SetFloat(valueName, value);
        }
    }

    [System.Serializable]
    public class Sound
    {
        public AudioClip clip;

        public AudioMixerGroup mixerGroup;

        public AudioType audioType;

        [Range(0f, 1f)]
        public float volume = 1f;

        public bool playOnAwake;

        public bool loop;

        [HideInInspector]
        public AudioSource audioSource;
    }
}

