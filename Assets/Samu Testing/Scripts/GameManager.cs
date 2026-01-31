using UnityEngine;

/// <summary>
/// Manages the game scene
/// TODO: 
/// - Set where the characters are being spawned
/// - Detect when the game ends
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerCharacter Player {  get; private set; }
    public AiCharacter Ai { get; private set; }

    [SerializeField] private PlayerCharacter PlayerPrefab;
    [SerializeField] private AiCharacter AiPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Player = Instantiate(PlayerPrefab, transform);
        Ai = Instantiate(AiPrefab, transform);

        Player.SetOpponent(Ai.transform);
        Ai.SetOpponent(Player.transform);
    }
}
