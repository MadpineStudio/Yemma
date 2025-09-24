using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IndentifiedAudioClips
{
    public string clipId;
    public List<AudioClip> clips;
}
public class YemmaAudioManager : MonoBehaviour
{
    private string lastClipId;
    private List<AudioClip> lastAudioClipsUsed = new();
    [SerializeField] private string defaultClipId;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<IndentifiedAudioClips> audioClips = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }


    public void PlayAudio(string clipId)
    {
        GetAndPlayAudio(clipId);
    }
    public void PlayAudio()
    {
        GetAndPlayAudio(defaultClipId);
    }
    private void GetAndPlayAudio(string clipId)
    {
        if (clipId == null) return; 
        if (lastClipId != clipId)
        {
            lastClipId = clipId;
            lastAudioClipsUsed.Clear();
            IndentifiedAudioClips indentifiedAudioClips = audioClips.Find(clips => clips.clipId == clipId);
            if (indentifiedAudioClips != null)
            {
                lastAudioClipsUsed = indentifiedAudioClips.clips;
            }
        }
        if (lastAudioClipsUsed.Count != 0)
        {
            audioSource.clip = lastAudioClipsUsed[UnityEngine.Random.Range(0, lastAudioClipsUsed.Count)];
            audioSource.Play();
        }
    }
}
