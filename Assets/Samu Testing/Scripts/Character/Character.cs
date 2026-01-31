using UnityEngine;

/// <summary>
/// Abstract base class for character
/// Ai and Player derive from this class
/// Handles effects that both player and AI should have
/// </summary>
public abstract class Character : MonoBehaviour
{
    public bool IsAtEnemyArea;
    public Character Opponent { get; private set; }
    [SerializeField] private Animator guyAnim;
    [SerializeField] private float speed = 10;
    [SerializeField] private float attackCd = 1.5f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject AttackHitBox;
    [SerializeField] private string enemyTag;
    private float rightTimer = 0f;
    private float leftTimer = 0f;
    private bool isAttacking = false;
    private string attackType = "";
    public bool isHurt = false;

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
    protected virtual void Update()
    {
        rightTimer += Time.deltaTime;
        leftTimer += Time.deltaTime;
        LookAtOpponent();
    }

    /// <summary>
    /// Moves character
    /// </summary>
    /// <param name="dir"></param>
    public void Move(Vector3 dir)
    {
        if (isAttacking || isHurt) return;

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
        if (leftTimer <= attackCd || isAttacking || isHurt) return;
        leftTimer = 0f;
        if (IsAtEnemyArea)
        {
            guyAnim.SetTrigger("Uppercut");
            attackType = "up";
        }
        else
        {
            guyAnim.SetTrigger("PunchL");
            attackType = "left";
        }
        isAttacking = true;
    }

    public void RightAttack()
    {
        if (rightTimer <= attackCd || isAttacking || isHurt) return;
        rightTimer = 0f;
        if (IsAtEnemyArea)
        {
            guyAnim.SetTrigger("Uppercut");
            attackType = "up";
        }
        else
        {
            guyAnim.SetTrigger("PunchR");
            attackType = "right";
        }

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

    public void Hurt(string attackDirection)
    {
        if (isHurt) return;
        isHurt = true;
        isAttacking = false;

        if (attackDirection == "left")
        {
            guyAnim.SetTrigger("HitLeft");
        }
        if (attackDirection == "right")
        {
            guyAnim.SetTrigger("HitRight");
        }
        if (attackDirection == "up")
        {
            guyAnim.SetTrigger("HitUp");
        }
        rb.AddForce(Vector3.back * 100f);
    }

    public void EndHurt()
    {
        isHurt = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag) && other.TryGetComponent<Character>(out var character) && !character != this)
        {
            character.Hurt(attackType);
            Debug.Log(other.name + " took damage");
        }
    }
}
