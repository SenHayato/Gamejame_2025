using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Type the exact name of the AudioClip found in your AudioManager")]
    [SerializeField] private string _musicTrackName;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            // 1. Force the previous music to stop
            AudioManager.Instance.StopMusic();

            // 2. Play the new track
            AudioManager.Instance.PlayMusic(_musicTrackName);
        }
        else
        {
            Debug.LogWarning("SceneMusicPlayer could not find AudioManager!");
        }
    }
}