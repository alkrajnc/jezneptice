using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioClip myClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            audioSource.PlayOneShot(myClip);
        }
    }
}
