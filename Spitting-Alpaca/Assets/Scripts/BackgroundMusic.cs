using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance = null;
    public static BackgroundMusic Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        // 싱글톤 패턴을 사용하여 하나의 배경음악 오브젝트만 유지
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        // 게임 시작 시 배경음악 재생
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && !audio.isPlaying)
        {
            audio.Play();
        }
    }

    public void PlayMusic()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && !audio.isPlaying)
        {
            audio.Play();
        }
    }

    public void StopMusic()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && audio.isPlaying)
        {
            audio.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.volume = volume;
        }
    }
}
