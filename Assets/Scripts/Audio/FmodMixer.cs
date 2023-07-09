using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Audio
{
    public class FMODMixer : MonoBehaviour
    {
        private Bus sfx;
        private Bus music;

        [SerializeField]
        private List<EventReference> GameSongEvents;
        private List<FMOD.GUID> gameSongGuids = new();

        private void Start()
        {
            sfx = RuntimeManager.GetBus("bus:/Sfx");
            music = RuntimeManager.GetBus("bus:/music");

            if (!PlayerPrefs.HasKey("music"))
            {
                PlayerPrefs.SetFloat("music", -5f);
                PlayerPrefs.SetFloat("sfx", -5f);
            }

            float dbMusic = PlayerPrefs.GetFloat("music");
            float dbSfx = PlayerPrefs.GetFloat("sfx");

            GameSongEvents.ForEach(e => gameSongGuids.Add(e.Guid));

            music.setVolume(DecibelToLinear(dbMusic));
            sfx.setVolume(DecibelToLinear(dbSfx));
        }

        public void ChangeMusicVolume(float dB)
        {
            dB = BottomDecibelsIfLowEnough(dB);
            music.setVolume(DecibelToLinear(dB));
            SaveVolumePreferences("music", dB);
        }

        public void ChangeSfxVolume(float dB)
        {
            dB = BottomDecibelsIfLowEnough(dB);
            sfx.setVolume(DecibelToLinear(dB));
            SaveVolumePreferences("sfx", dB);
        }

        public void KillEverySound()
        {
            if (FindObjectsOfType(typeof(StudioEventEmitter)) is StudioEventEmitter[] eventEmitters)
            {
                foreach (var eventEmitter in eventEmitters)
                {
                    eventEmitter.AllowFadeout = false;
                    eventEmitter.Stop();
                }
            }
        }

        public void KillEverySoundExcept(params FMOD.GUID[] guids)
        {
            if (guids.Length == 0)
            {
                Debug.LogWarning("Called FmodMixer#KillEverySoundExcept without any GUIDs! Please call FmodMixer#KillEverySound instead");
            }

            if (FindObjectsOfType(typeof(StudioEventEmitter)) is StudioEventEmitter[] eventEmitters)
            {
                foreach (var eventEmitter in eventEmitters)
                {
                    if (!guids.Contains(eventEmitter.EventReference.Guid))
                    {
                        eventEmitter.AllowFadeout = false;
                        eventEmitter.Stop();
                    }
                }
            }
        }

        public void FindAllSfxAndPlayPause(bool isGamePaused)
        {
            if (FindObjectsOfType(typeof(StudioEventEmitter)) is StudioEventEmitter[] eventEmitters)
            {
                // filter out events within this.GameSongEvents (we don't want to pause the music in this case)
                var sfxEvents = eventEmitters.Where(e => !gameSongGuids.Contains(e.EventReference.Guid));
                foreach (var eventEmitter in sfxEvents)
                {
                    
                    switch (isGamePaused)
                    {
                        case true when eventEmitter.IsPlaying():
                            eventEmitter.EventInstance.setPaused(true);
                            break;
                        case false when !eventEmitter.IsPlaying():
                            eventEmitter.EventInstance.setPaused(false);
                            break;
                    }
                }
            }
        }

        private float DecibelToLinear(float dB)
        {
            var linear = Mathf.Pow(10f, dB / 20f);
            return linear;
        }

        private float BottomDecibelsIfLowEnough(float dB)
        {
            return dB <= -19.9f ? -200f : dB;
        }

        private void SaveVolumePreferences(string bus, float dB)
        {
            PlayerPrefs.SetFloat(bus.ToLower(), dB);
            PlayerPrefs.Save();
        }
    }
}
