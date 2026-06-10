using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip backgroundMusic;
    public AudioClip unitAttackSFX;
    public AudioClip unitDieSFX;
    public AudioClip buttonClickSFX;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayAttack() => PlaySFX(unitAttackSFX);
    public void PlayUnitDie() => PlaySFX(unitDieSFX);
    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
}
