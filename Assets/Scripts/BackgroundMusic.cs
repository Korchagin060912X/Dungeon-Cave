using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;
    [SerializeField] private string resourcesClipPath = "BGM";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        AudioSource source = GetComponent<AudioSource>();
        if (source.clip == null)
        {
            AudioClip clip = Resources.Load<AudioClip>(resourcesClipPath);
            if (clip != null)
            {
                source.clip = clip;
            }
        }

        if (source.clip != null && !source.isPlaying)
        {
            source.Play();
        }

        DontDestroyOnLoad(gameObject);
    }
}
