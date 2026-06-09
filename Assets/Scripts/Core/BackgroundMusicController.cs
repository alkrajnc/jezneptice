using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public static BackgroundMusicController instance;

    [Header("Zvočni Posnetki")]
    public AudioClip mainMenuMusic;
    public AudioClip levelMusic;

    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Funkcija za preklop na glasbo za nivoje
    public void PlayLevelMusic()
    {
        SwitchMusic(levelMusic);
    }

    // Funkcija za preklop na glasbo za meni
    public void PlayMenuMusic()
    {
        SwitchMusic(mainMenuMusic);
    }

    private void SwitchMusic(AudioClip newClip)
    {
        if (audioSource == null || audioSource.clip == newClip) return;

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();
    }
}