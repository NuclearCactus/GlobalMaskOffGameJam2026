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

    public PlayerCharacter Player;
    public AiCharacter Ai;

    [SerializeField] private PlayerCharacter PlayerPrefab;
    [SerializeField] private AiCharacter AiPrefab;

    [SerializeField] private float changeSideTime = 1f;
    [SerializeField] private float radius = 8f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Player.SetOpponent(Ai);
        Ai.SetOpponent(Player);
    }

    public Vector3 ClampCharacterPosInBounds(Vector3 deltaPos)
    {
        if (IsOutOfBounds(deltaPos))
        {
            deltaPos = Vector3.ClampMagnitude(deltaPos, radius);
        }

        return deltaPos;
    }

    public bool IsOutOfBounds(Vector3 deltaPos)
    {
        return Vector3.Distance(transform.position, deltaPos) >= radius;
    }
}
