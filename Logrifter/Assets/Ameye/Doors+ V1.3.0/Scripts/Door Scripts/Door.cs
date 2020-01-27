using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Door : MonoBehaviour
{
    public enum AudioClips { Opening, Closing, Locked }
    public AudioClips CurrentAudioClips;

    public AudioSource _previewSourceOne;
    public AudioSource _previewSourceTwo;
    public AudioClip[] AdClips;

    public AudioClip OpeningClip;
    [Range(0f, 1f)]
    public float OpeningVolume;
    [Range(0.1f, 3f)]
    public float OpeningPitch;
    [HideInInspector]
    public AudioSource OpeningSource;
    [Range(0f, 1f)]
    public float OpeningOffset;

    public AudioClip OpenedClip;
    [Range(0f, 1f)]
    public float OpenedVolume;
    [Range(0.1f, 3f)]
    public float OpenedPitch;
    [HideInInspector]
    public AudioSource OpenedSource;
    [Range(0f, 1f)]
    public float OpenedOffset;

    public AudioClip ClosingClip;
    [Range(0f, 1f)]
    public float ClosingVolume;
    [Range(0.1f, 3f)]
    public float ClosingPitch;
    [HideInInspector]
    public AudioSource ClosingSource;
    [Range(0f, 1f)]
    public float ClosingOffset;

    public AudioClip ClosedClip;
    [Range(0f, 1f)]
    public float ClosedVolume;
    [Range(0.1f, 3f)]
    public float ClosedPitch;
    [HideInInspector]
    public AudioSource ClosedSource;
    [Range(0f, 1f)]
    public float ClosedOffset;

    public AudioClip LockedClip;
    [Range(0f, 1f)]
    public float LockedVolume;
    [Range(0.1f, 3f)]
    public float LockedPitch;
    [HideInInspector]
    public AudioSource LockedSource;
    [Range(0f, 1f)]
    public float LockedOffset;

    public int TimesMoved;
    public bool MovementPending;
    public bool ResetOnLeave;

    public virtual void Start()
    {
        #region Open
        OpeningSource = gameObject.AddComponent<AudioSource>();
        OpeningSource.clip = OpeningClip;
        OpeningSource.volume = OpeningVolume;
        OpeningSource.pitch = OpeningPitch;

        OpenedSource = gameObject.AddComponent<AudioSource>();
        OpenedSource.clip = OpenedClip;
        OpenedSource.volume = OpenedVolume;
        OpenedSource.pitch = OpenedPitch;
        #endregion

        #region Close
        ClosingSource = gameObject.AddComponent<AudioSource>();
        ClosingSource.clip = ClosingClip;
        ClosingSource.volume = ClosingVolume;
        ClosingSource.pitch = ClosingPitch;

        ClosedSource = gameObject.AddComponent<AudioSource>();
        ClosedSource.clip = ClosedClip;
        ClosedSource.volume = ClosedVolume;
        ClosedSource.pitch = ClosedPitch;
        #endregion

        #region Locked
        LockedSource = gameObject.AddComponent<AudioSource>();
        LockedSource.clip = LockedClip;
        LockedSource.volume = LockedVolume;
        LockedSource.pitch = LockedPitch;
        #endregion
    }

    public bool DoorIsOpening()
        {
            return TimesMoved % 2 == 0;
        }

        public bool DoorIsClosing()
        {
            return TimesMoved % 2 != 0;
        }

        public void Play(string name)
        {
            switch (name)
            {
                case "opening":
                    OpeningSource.PlayDelayed(OpeningOffset);
                    //OpeningSource.outputAudioMixerGroup = mixer;
                    break;
                case "opened":
                    OpenedSource.PlayDelayed(OpenedOffset);
                    break;
                case "closing":
                    ClosingSource.PlayDelayed(ClosingOffset);
                    break;
                case "closed":
                    ClosedSource.PlayDelayed(ClosedOffset);
                    break;
                case "locked":
                    LockedSource.PlayDelayed(LockedOffset);
                    break;
            }
        }

        public void Preview(string mode)
        {
            _previewSourceOne = gameObject.AddComponent<AudioSource>();
            _previewSourceTwo = gameObject.AddComponent<AudioSource>();

            switch (mode)
            {
                case "Open":
                    AdClips = new[] { OpeningClip, OpenedClip };
                    break;
                case "Close":
                    AdClips = new[] { ClosingClip, ClosedClip };
                    break;
                case "Lock":
                    AdClips = new[] { LockedClip, LockedClip };
                    break;
            }

            StartCoroutine(PlayAudioSequentially());
        }

        private IEnumerator PlayAudioSequentially()
        {
            yield return null;

            _previewSourceOne.clip = AdClips[0];
            _previewSourceTwo.clip = AdClips[1];

            if (AdClips[0] == OpeningClip)
            {
                _previewSourceOne.volume = OpeningVolume;
                _previewSourceOne.pitch = OpeningPitch;

                _previewSourceTwo.volume = OpenedVolume;
                _previewSourceTwo.pitch = OpenedPitch;

                _previewSourceOne.PlayDelayed(OpeningOffset);
                _previewSourceTwo.PlayDelayed(OpenedOffset);
            }

            else if (AdClips[0] == ClosingClip)
            {
                _previewSourceOne.volume = ClosingVolume;
                _previewSourceOne.pitch = ClosingPitch;

                _previewSourceTwo.volume = ClosedVolume;
                _previewSourceTwo.pitch = ClosedPitch;

                _previewSourceOne.PlayDelayed(ClosingOffset);
                _previewSourceTwo.PlayDelayed(ClosedOffset);
            }

            else if (AdClips[0] == LockedClip)
            {
                _previewSourceOne.volume = LockedVolume;
                _previewSourceOne.pitch = LockedPitch;

                _previewSourceOne.PlayDelayed(LockedOffset);

            }

            while (_previewSourceOne.isPlaying || _previewSourceTwo.isPlaying)
            {
                yield return null;
            }

            if (!_previewSourceOne.isPlaying) DestroyImmediate(_previewSourceOne);
            if (!_previewSourceTwo.isPlaying) DestroyImmediate(_previewSourceTwo);
        }
}
