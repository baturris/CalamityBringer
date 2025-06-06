
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("_______Audio Source_______")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [Header("_______Audio Clip_______")]
    public AudioClip background;
    public AudioClip death;
    public AudioClip block;
    public AudioClip jump;
    public AudioClip dash;
    public AudioClip lighthit;
    public AudioClip heavyhit;
    


    private void Start()
    {

        musicSource.clip = background;
        musicSource.Play();
    }
    public void PlaySFXWithVolume(AudioClip clip, float volume)
    {
        SFXSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
