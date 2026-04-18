using System.Collections.Generic;
using UnityEngine;

public class MusicPlayerScript : MonoBehaviour
{
    public List<AudioClip> songs; // zieh die Songs hier rein im Inspector
    public AudioSource audioSource;

    private HashSet<int> playedIndices = new HashSet<int>();

    void Start()
    {
        PlayRandomSong();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
            PlayRandomSong();

        float t = Mathf.InverseLerp(SpeedManager.startSpeed, SpeedManager.maxSpeed, SpeedManager.currentSpeed);
        audioSource.pitch = Mathf.Lerp(1f, 1.5f, t);
    }

    void PlayRandomSong()
    {
        if (songs.Count == 0) return;

        // Optional: Alle Songs wurden gespielt, Liste zur�cksetzen
        if (playedIndices.Count >= songs.Count)
        {
            playedIndices.Clear();
        }

        int index;
        do
        {
            index = Random.Range(0, songs.Count);
        } while (playedIndices.Contains(index));

        playedIndices.Add(index);

        audioSource.clip = songs[index];
        audioSource.Play();
    }
}
