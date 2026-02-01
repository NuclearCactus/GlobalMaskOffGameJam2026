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

    [SerializeField] private GameObject topArenaObj;
    [SerializeField] private GameObject bottomArenaObj;
    [SerializeField] private float changeSideTime = 1f;
    private float changeSideTimer = 0f;
    [SerializeField] private float radius;
     
    private Character topCharacter = null;
    private Character botCharacter = null;

    bool botIsGreen = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Collider ArenaCollider = topArenaObj.GetComponent<Collider>();

        Player.SetOpponent(Ai);
        Ai.SetOpponent(Player);

        botCharacter = Player;
        topCharacter = Ai;

        SetNewColor(Color.green, bottomArenaObj);
        SetNewColor(Color.red, topArenaObj);
    }

    private void SetNewColor(Color newColor, GameObject target)
    {
        MaterialPropertyBlock block = new();
        block.SetColor("_BaseColor", newColor);
        target.GetComponent<MeshRenderer>().SetPropertyBlock(block);
    }

    private void FixedUpdate()
    {
        bool botIsAtEnemy = IsCharacterInOnTopSide(botCharacter.transform.position);
        bool topIsAtEnemy = !IsCharacterInOnTopSide(topCharacter.transform.position);

        botCharacter.IsAtEnemyArea = botIsAtEnemy;
        topCharacter.IsAtEnemyArea = topIsAtEnemy;

        if (botIsAtEnemy && topIsAtEnemy) changeSideTimer += Time.deltaTime;
        else changeSideTimer = 0f;

        // Both are at the enemy side, switch sides
        if (changeSideTimer > changeSideTime)
        {
            (botCharacter, topCharacter) = (topCharacter, botCharacter);
            topCharacter.IsAtEnemyArea = false;
            botCharacter.IsAtEnemyArea = false;
            SetNewColor(botIsGreen ? Color.red : Color.green, bottomArenaObj);
            SetNewColor(botIsGreen ? Color.green : Color.red, topArenaObj);
            botIsGreen = !botIsGreen;
            changeSideTimer = 0f;
        }
    }

    private bool IsCharacterInOnTopSide(Vector3 pos)
    {
        return pos.z > transform.position.z;
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
