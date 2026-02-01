using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] audioSources;

    public void PlaySound(SoundType type)
    {
        switch (type)
        {
            case SoundType.PunchSwing:
                audioSources[0].Play();
                break;
            case SoundType.PunchHit:
                audioSources[1].Play();
                break;
            case SoundType.FootStep:
                audioSources[2].Play();
                break;
            default:
                break;
        }
    }
}


public enum SoundType
{
    None,
    PunchSwing,
    PunchHit,
    FootStep
}