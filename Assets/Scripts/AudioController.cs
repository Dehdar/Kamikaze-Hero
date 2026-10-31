using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    private GameObject audioComponent;
    private AudioSource audioSource;
    private bool isAudioPlaying;

    void Awake()
    {
        audioComponent = GameObject.Find("MUSIC");
        if (audioComponent == null)
        {
            audioComponent = gameObject;
            audioComponent.name = "MUSIC";
            DontDestroyOnLoad(audioComponent);
            audioSource = GetComponent<AudioSource>();
            audioSource.Play();
            isAudioPlaying = true;
            StartCoroutine(LongRunningMusicManager());
        }
    }

    IEnumerator LongRunningMusicManager()
    {
        while (true)
        {
            if (isAudioPlaying && SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Game") && SpawnManager.PlayMusic == false)
            {
                audioSource.Stop();
                isAudioPlaying = false;
            }

            if (!isAudioPlaying && (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Game") || SpawnManager.PlayMusic == true))
            {
                audioSource.Play();
                isAudioPlaying = true;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
