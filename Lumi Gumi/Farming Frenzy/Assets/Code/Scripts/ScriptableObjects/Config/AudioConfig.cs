using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "ScriptableObjects/Config/AudioConfig", order = 1)]
public class AudioConfig : ScriptableObject
{
    [Range(0, 100)]
    public int BGMVolume;
    [Range(0, 100)]
    public int SFXVolume;
}
