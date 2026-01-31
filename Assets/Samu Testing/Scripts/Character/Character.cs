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
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject AttackHitBox;
    [SerializeField] private TagHandle enemyTag;
    private float rightTimer = 0f;
    private float leftTimer = 0f;
    private bool isAttacking = false;

    private void Start()
    {
        rightTimer = attackCd;
        leftTimer = attackCd;
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
    protected virtual void FixedUpdate()
    {
        rightTimer += Time.deltaTime;
        leftTimer+= Time.deltaTime;
    }

    /// <summary>
    /// Moves character
    /// </summary>
    /// <param name="dir"></param>
    public void Move(Vector3 dir)
    {
        if (isAttacking) return;

        Vector3 deltaPos = transform.position + (speed * Time.fixedDeltaTime * dir);

        deltaPos = GameManager.Instance.ClampCharacterPosInBounds(deltaPos);

        if (deltaPos != transform.position)
            rb.MovePosition(deltaPos);

        guyAnim.SetFloat("MoveX", dir.x);
        guyAnim.SetFloat("MoveY", dir.z);
    }

    /// <summary>
    /// Looks at opponent
    /// </summary>
    public void LookAtOpponent()
    {
        transform.LookAt(Opponent.transform.position);
    }

    public void LeftAttack()
    {
        if (leftTimer <= attackCd || isAttacking) return;
        leftTimer = 0f;
        guyAnim.SetTrigger("PunchL");
        isAttacking = true;
    }

    public void RightAttack()
    {
        if (rightTimer <= attackCd || isAttacking) return;
        rightTimer = 0f;
        guyAnim.SetTrigger("PunchR");
        isAttacking = true;
    }

    public void EnableHitbox()
    {
        AttackHitBox.SetActive(true);
    }

    public void DisableHitbox()
    {
        AttackHitBox.SetActive(false);
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(enemyTag))
        {
            Debug.Log("Take damage");
        }
    }
}
