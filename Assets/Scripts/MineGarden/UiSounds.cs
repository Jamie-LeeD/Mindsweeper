using UnityEngine;

namespace MineGarden
{
    /// <summary>
    /// Wood block clips from Assets/UI Soundpack, used for button clicks.
    /// </summary>
    public class UiSounds : ScriptableObject
    {
        public AudioClip[] woodBlocks;

        public static UiSounds Load()
        {
            UiSounds sounds = Resources.Load<UiSounds>("UiSounds");
            if (sounds == null)
                sounds = CreateInstance<UiSounds>();

#if UNITY_EDITOR
            sounds.FillMissingFromProject();
#endif
            return sounds;
        }

        public AudioClip Pick()
        {
            if (woodBlocks == null || woodBlocks.Length == 0)
                return null;

            int start = Random.Range(0, woodBlocks.Length);
            for (int i = 0; i < woodBlocks.Length; i++)
            {
                AudioClip clip = woodBlocks[(start + i) % woodBlocks.Length];
                if (clip != null)
                    return clip;
            }

            return null;
        }

#if UNITY_EDITOR
        void FillMissingFromProject()
        {
            if (HasClip())
                return;

            woodBlocks = new[]
            {
                LoadClip("Assets/UI Soundpack/WAV/Wood Block1.wav"),
                LoadClip("Assets/UI Soundpack/WAV/Wood Block2.wav"),
                LoadClip("Assets/UI Soundpack/WAV/Wood Block3.wav")
            };
        }

        bool HasClip()
        {
            if (woodBlocks == null)
                return false;

            for (int i = 0; i < woodBlocks.Length; i++)
            {
                if (woodBlocks[i] != null)
                    return true;
            }

            return false;
        }

        static AudioClip LoadClip(string path)
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
#endif
    }
}
