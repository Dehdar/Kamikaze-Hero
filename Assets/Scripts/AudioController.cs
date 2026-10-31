using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Plays the background music across scenes (kept alive with DontDestroyOnLoad).
/// The music is muted in the Game scene while waves are running (see SpawnManager.PlayMusic).
/// </summary>
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
            if (isAudioPlaying && SceneManager.GetActiveScene().name == SceneNames.Game && SpawnManager.PlayMusic == false)
            {
                audioSource.Stop();
                isAudioPlaying = false;
            }

            if (!isAudioPlaying && (SceneManager.GetActiveScene().name != SceneNames.Game || SpawnManager.PlayMusic == true))
            {
                audioSource.Play();
                isAudioPlaying = true;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
