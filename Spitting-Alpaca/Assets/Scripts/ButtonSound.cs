using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clickSound;
        audioSource.playOnAwake = false;  // 씬 로드 시 소리가 나지 않도록 설정
    }

    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound); // 클릭 시에만 소리가 재생되도록 설정
    }
}
