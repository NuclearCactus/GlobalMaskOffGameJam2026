using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private Animator guyAnim;
    [SerializeField] private float speed = 10;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] private GameObject AttackHitBox;
    [SerializeField] private string enemyTag;
    [SerializeField] protected float attackCd = 1f;
    [SerializeField] protected float upperCutCd = 3f; 
    [SerializeField] protected float dashCd = 1.5f;

    protected float rightTimer = 0f;
    protected float leftTimer = 0f;
    protected float uppercutTimer = 0f;
    protected float dashTimer = 1.5f;
    private string attackType = "";

    public bool isHurt = false;
    public bool isAttacking = false;
    public bool isDashing = false;

    private float walkAnimSpeed = 10f;
    private float currentMoveX;
    private float currentMoveY;

    // === MASK SYSTEM ===
    [Header("Mask Settings")]
    [SerializeField] private Transform maskAnchor;
    [SerializeField] private int initialMaskCount = 5;
    [SerializeField] private float maskSpacing = 0.01f;

    private List<MaskObject> masks = new List<MaskObject>();
    private Transform topMask;
    private Vector3 topMaskScale = new(0.01f, 0.01f, 0.01f);
    private Vector3 maskScale = new(0.008f, 0.008f, 0.008f);

    // Public accessors
    public MaskObject TopMask => masks.Count > 0 ? masks[masks.Count - 1] : null;
    public int MaskCount => masks.Count;
    public bool IsAlive => masks.Count > 0;

    public bool IsAtEnemyArea;
    public Character Opponent { get; private set; }
    public Transform MaskAnchor { get { return maskAnchor; } }

    private void Start()
    {
        rightTimer = attackCd;
        leftTimer = attackCd;
        uppercutTimer = attackCd;
        InitializeMasks();
    }

    private void InitializeMasks()
    {
        for (int i = 0; i < initialMaskCount; i++)
        {
            AddRandomMask();
        }
    }

    private void AddRandomMask()
    {
        MaskObject newMask = NewMaskManager.Instance.CreateRandomMask();
        AddMask(newMask);
    }

    public void AddMask(MaskObject newMask)
    {
        newMask.transform.SetParent(maskAnchor, false);
        newMask.transform.SetLocalPositionAndRotation(new Vector3(0, masks.Count * maskSpacing, 0), Quaternion.identity);

        if (topMask != null)
        {
            topMask.localScale = maskScale;
        }

        newMask.transform.localScale = topMaskScale;
        topMask = newMask.transform;
        masks.Add(newMask);
    }

    /// <summary>
    /// Removes and destroys the top mask. Returns the phrase from the removed mask's data,
    /// or an empty string if there was nothing to remove.
    /// </summary>
    public string RemoveTopMask()
    {
        if (masks.Count == 0) return "";

        MaskObject maskToBeRemoved = masks[masks.Count - 1];

        // Grab the phrase before we pop it — the data is still valid after PopMask,
        // but reading it here keeps intent obvious.
        string phrase = maskToBeRemoved.GetData()?.popUpPhrase ?? "";

        masks.RemoveAt(masks.Count - 1);
        maskToBeRemoved.PopMask();

        if (masks.Count == 0)
        {
            OnDefeated();
        }
        else
        {
            topMask = masks[^1].transform;
            topMask.localScale = topMaskScale;
        }

        return phrase;
    }

    /// <summary>
    /// Steals the top mask and transfers it to the opponent. Returns the phrase
    /// from the stolen mask's data, or an empty string if there was nothing to steal.
    /// </summary>
    public string StealTopMask()
    {
        if (masks.Count == 0) return "";

        MaskObject maskToBeRemoved = masks[masks.Count - 1];

        string phrase = maskToBeRemoved.GetData()?.popUpPhrase ?? "";

        masks.RemoveAt(masks.Count - 1);
        maskToBeRemoved.StealMask(Opponent);

        if (masks.Count == 0)
        {
            OnDefeated();
        }
        else
        {
            topMask = masks[^1].transform;
            topMask.localScale = topMaskScale;
        }

        return phrase;
    }

    protected virtual void OnDefeated()
    {
        Debug.Log($"{gameObject.name} has been defeated!");
    }

    /// <summary>
    /// Called on the attacker when one of their hits successfully lands.
    /// The phrase is the popUpPhrase from the mask that was just removed or stolen.
    /// </summary>
    protected virtual void OnHitLanded(string phrase) { }

    public void SetOpponent(Character opponent)
    {
        Opponent = opponent;
    }

    protected virtual void FixedUpdate()
    {
        rightTimer += Time.fixedDeltaTime;
        leftTimer += Time.fixedDeltaTime;
        uppercutTimer += Time.fixedDeltaTime;
        dashTimer += Time.fixedDeltaTime;
        LookAtOpponent();

        if (rb.linearVelocity.magnitude > 0f)
        {
            Vector3 deltaPos = transform.position + rb.linearVelocity * Time.deltaTime;
            if (GameManager.Instance.IsOutOfBounds(deltaPos))
            {
                rb.linearVelocity *= -0.25f;
            }
        }
    }

    public void Move(Vector3 dir)
    {
        if (isAttacking || isHurt || isDashing) return;
        rb.linearVelocity = Vector3.zero;

        Vector3 movement = (speed * Time.fixedDeltaTime * dir.normalized);
        Vector3 deltaPos = transform.position + movement;

        deltaPos = GameManager.Instance.ClampCharacterPosInBounds(deltaPos);

        if (deltaPos != transform.position)
            rb.MovePosition(deltaPos);

        currentMoveX = Mathf.Lerp(currentMoveX, dir.x, walkAnimSpeed * Time.fixedDeltaTime);
        currentMoveY = Mathf.Lerp(currentMoveY, dir.z, walkAnimSpeed * Time.fixedDeltaTime);

        guyAnim.SetFloat("MoveX", currentMoveX);
        guyAnim.SetFloat("MoveY", currentMoveY);
    }

    public void LookAtOpponent()
    {
        if (isHurt) return;

        Vector3 direction = (Opponent.transform.position - transform.position).normalized;
        if (direction == Vector3.zero) return;
        Quaternion desiredRot = Quaternion.LookRotation(direction);

        Quaternion newRot = Quaternion.RotateTowards(
            rb.rotation,
            desiredRot,
            Time.fixedDeltaTime * 100f);

        rb.MoveRotation(newRot);
    }

    public bool LeftAttack()
    {
        if (leftTimer <= attackCd || isAttacking || isHurt || isDashing) return false;
        leftTimer = 0f;
        guyAnim.SetTrigger("PunchL");
        attackType = "left";
        isAttacking = true;
        return true;
    }

    public bool RightAttack()
    {
        if (rightTimer <= attackCd || isAttacking || isHurt || isDashing) return false;
        rightTimer = 0f;
        guyAnim.SetTrigger("PunchR");
        attackType = "right";
        isAttacking = true;
        return true;
    }

    public bool UpperCut()
    {
        if (uppercutTimer <= upperCutCd || isAttacking || isHurt || isDashing) return false;
        uppercutTimer = 0f;
        guyAnim.SetTrigger("Uppercut");
        attackType = "up";
        isAttacking = true;
        return true;
    }

    public void EnableHitbox()
    {
        AttackHitBox.SetActive(true);
        soundManager.PlaySound(SoundType.PunchSwing);
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
        if (isHurt || isDashing) return;
        isHurt = true;
        soundManager.PlaySound(SoundType.PunchHit);

        // Each branch removes/steals a mask and captures the phrase it returns
        string phrase = "";

        if (attackDirection == "left")
        {
            guyAnim.SetTrigger("HitLeft");
            phrase = RemoveTopMask();
        }
        if (attackDirection == "right")
        {
            guyAnim.SetTrigger("HitRight");
            phrase = RemoveTopMask();
        }
        if (attackDirection == "up")
        {
            guyAnim.SetTrigger("HitUp");
            phrase = StealTopMask();
        }

        // Notify the attacker, passing along the phrase from the mask that was just taken
        Opponent?.OnHitLanded(phrase);

        rb.AddForce((attackDirection == "up" ? 3f : 1f) * 3f * -transform.forward.normalized, ForceMode.Impulse);
        DisableHitbox();
        isAttacking = false;
        guyAnim.SetFloat("MoveX", 0f);
        guyAnim.SetFloat("MoveY", 0f);
    }

    public void EndHurt()
    {
        isHurt = false;
        DisableHitbox();
        isAttacking = false;
    }

    public void StartDash()
    {
        if (isHurt || isAttacking || dashTimer <= dashCd) return;

        isDashing = true;
        dashTimer = 0f;
        guyAnim.SetTrigger("Dash");
        rb.linearVelocity = Vector3.zero;
        Vector3 dir = new(currentMoveX, 0f, currentMoveY);
        rb.AddForce(dir * 8f, ForceMode.Impulse);
        soundManager.PlaySound(SoundType.PunchSwing);
    }

    public void EndDash()
    {
        isDashing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag) && other.TryGetComponent<Character>(out var character) && character != this)
        {
            character.Hurt(attackType);
            Debug.Log(other.name + " took damage - Masks remaining: " + character.MaskCount);
        }
    }
}