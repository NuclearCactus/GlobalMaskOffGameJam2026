using UnityEngine;

/// <summary>
/// Abstract base class for character
/// Ai and Player derive from this class
/// Handles effects that both player and AI should have
/// </summary>
public abstract class Character : MonoBehaviour
{
    public bool IsAtEnemyArea;
    public Character Opponent {  get; private set; }
    [SerializeField] private Animator guyAnim;
    [SerializeField] private float speed = 10;
    [SerializeField] private float attackCd = 1.5f;
    private float attackTimer = 0f;

    private void Start()
    {
        attackTimer = attackCd;
    }

    /// <summary>
    /// Sets the opponent that this character is looking at
    /// </summary>
    /// <param name="opponent"></param>
    public void SetOpponent(Character opponent)
    {
        Opponent = opponent;
    }

    /// <summary>
    /// Virtual Update can be overridden
    /// base should be called to look at opponent
    /// </summary>
    protected virtual void Update()
    {
        LookAtOpponent();
        attackTimer += Time.deltaTime;
    }

    /// <summary>
    /// Moves character
    /// </summary>
    /// <param name="dir"></param>
    public void Move(Vector3 dir)
    {
        transform.position += speed * Time.deltaTime * dir;
    }

    /// <summary>
    /// Looks at opponent
    /// </summary>
    public void LookAtOpponent()
    {
        transform.LookAt(Opponent.transform.position);
    }

    public void StartAttack()
    {
        if (attackTimer <= attackCd) return;
        attackTimer = 0f;
        guyAnim.SetTrigger("PunchL");
    }

    public void EnableHitbox()
    {

    }

    public void DisableHitbox()
    {

    }
}
