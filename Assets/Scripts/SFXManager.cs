using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public static SFXManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if(Instance != this) Destroy(this);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1)
    {
        audioSource.PlayOneShot(clip, volumeScale);
    }
}
