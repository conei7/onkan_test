using System.Collections.Generic;
using UnityEngine;

namespace AbsolutePitchGame
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource noteSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Note Clips (C4-B4)")]
        [SerializeField] private List<AudioClip> noteClips = new();

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayNote(int noteIndex)
        {
            if (!TryGetClip(noteIndex, out var clip))
            {
                Debug.LogWarning($"AudioManager: No clip configured for note index {noteIndex}.");
                return;
            }

            noteSource.clip = clip;
            noteSource.Play();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (sfxSource == null)
            {
                Debug.LogWarning("AudioManager: sfxSource is not assigned.");
                return;
            }

            sfxSource.PlayOneShot(clip);
        }

        public AudioClip GetNoteClip(int noteIndex)
        {
            return TryGetClip(noteIndex, out var clip) ? clip : null;
        }

        private bool TryGetClip(int noteIndex, out AudioClip clip)
        {
            clip = null;

            if (noteSource == null)
            {
                Debug.LogWarning("AudioManager: noteSource is not assigned.");
                return false;
            }

            if (noteIndex < 0 || noteIndex >= noteClips.Count)
            {
                return false;
            }

            clip = noteClips[noteIndex];
            return clip != null;
        }
    }
}
