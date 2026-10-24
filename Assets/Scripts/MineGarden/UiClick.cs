using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MineGarden
{
    /// <summary>
    /// Plays a wood block click for UI buttons. The source survives scene changes so the tap is not cut off.
    /// </summary>
    public static class UiClick
    {
        static readonly List<Button> hooked = new List<Button>();
        static AudioSource source;
        static UiSounds sounds;

        public static void Play()
        {
            if (sounds == null)
                sounds = UiSounds.Load();

            AudioClip clip = sounds != null ? sounds.Pick() : null;
            if (clip == null)
                return;

            AudioSource player = Source();
            player.PlayOneShot(clip);
        }

        public static void Hook(Button button)
        {
            if (button == null)
                return;

            for (int i = hooked.Count - 1; i >= 0; i--)
            {
                if (hooked[i] == null)
                    hooked.RemoveAt(i);
            }

            if (hooked.Contains(button))
                return;

            hooked.Add(button);
            button.onClick.AddListener(Play);
        }

        public static void HookAll(Transform root)
        {
            if (root == null)
                return;

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
                Hook(buttons[i]);
        }

        static AudioSource Source()
        {
            if (source != null)
                return source;

            var playerObject = new GameObject("Ui Click");
            Object.DontDestroyOnLoad(playerObject);
            source = playerObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }
    }
}
