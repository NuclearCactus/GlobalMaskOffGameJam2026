using UnityEngine;

/// <summary>
/// Can carry data between scenes
/// </summary>
public class PersistentManager : MonoBehaviour
{
    public static PersistentManager Instance { get; private set; }

    public bool PlayerWins = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}
