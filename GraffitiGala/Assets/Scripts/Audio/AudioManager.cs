using UnityEngine;
using FMODUnity;
using FMOD.Studio;
namespace GraffitiGala
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance { get; private set; }

        /*
        private Bus masterBus;
        private Bus sfxBus;
        private Bus bgmBus;*/

        [Range(0, 1)]
        public float masterVolume;
        [Range(0, 1)]
        public float sfxVolume;
        [Range(0, 1)]
        public float musicVolume;

        [SerializeField]
        private bool isHost = false;

        private EventInstance bgm;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.Log("There is more than one AudioManager in the scene");
            }
            instance = this;

            /*masterBus = RuntimeManager.GetBus("bus:/");
            sfxBus = RuntimeManager.GetBus("bus:/SFX");
            bgmBus = RuntimeManager.GetBus("bus:/BGM");*/

            if (FindObjectOfType<BuildManager>().BuildTypeRef == BuildType.Admin)
            {
                bgm = CreateEventInstance(FMODEventsManager.instance.BGM);
                bgm.start();
            }


        }


        public void PlayOneShot(EventReference sound, Vector3 worldPos)
        {
            RuntimeManager.PlayOneShot(sound, worldPos);
            print("One shot of" + sound);
        }

        public EventInstance CreateEventInstance(EventReference eventReference)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            //eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.GetComponent<Transform>(), gameObject.GetComponent<Rigidbody>()));
            return eventInstance;
        }

        /*
        public void UpdateVolume()
        {
            /*masterVolume = (SettingManager.instance.GetNumberSetting(SettingManager.NumberSettings.masterVol) / 100);
            masterBus.setVolume(masterVolume);
            sfxVolume = (SettingManager.instance.GetNumberSetting(SettingManager.NumberSettings.sfxVol) / 100);
            sfxBus.setVolume(sfxVolume);
            musicVolume = (SettingManager.instance.GetNumberSetting(SettingManager.NumberSettings.musicVol) / 100);
            bgmBus.setVolume(musicVolume);
        }*/
    }
}