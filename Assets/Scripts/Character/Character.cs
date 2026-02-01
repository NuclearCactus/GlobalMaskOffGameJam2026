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
    [SerializeField] private Transform maskAnchor; // Assign the head/face transform in inspector
    [SerializeField] private int initialMaskCount = 5;
    [SerializeField] private float maskSpacing = 0.01f; // Horizontal spacing between stacked masks

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

    /// <summary>
    /// Creates the initial stack of masks on the character's face
    /// </summary>
    private void InitializeMasks()
    {
        for (int i = 0; i < initialMaskCount; i++)
        {
            AddRandomMask();
        }
    }

    /// <summary>
    /// Adds a random mask to the stack
    /// </summary>
    private void AddRandomMask()
    {
        MaskObject newMask = NewMaskManager.Instance.CreateRandomMask();

        AddMask(newMask);
    }

    public void AddMask(MaskObject newMask)
    {
        // Parent to the mask anchor (head)
        newMask.transform.SetParent(maskAnchor, false);

        // Position it in the stack (each mask slightly forward/outward)
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
    /// Removes and destroys the top mask (when taking damage)
    /// </summary>
    public void RemoveTopMask()
    {
        if (masks.Count == 0) return;

        MaskObject maskToBeRemoved = masks[masks.Count - 1];
        masks.RemoveAt(masks.Count - 1);
        maskToBeRemoved.PopMask();

        // Check if character is defeated
        if (masks.Count == 0)
        {
            OnDefeated();
        }
        else
        {
            topMask = masks[^1].transform;
            topMask.localScale = topMaskScale;
        }
    }

    public void StealTopMask()
    {
        if (masks.Count == 0) return;

        MaskObject maskToBeRemoved = masks[masks.Count - 1];
        masks.RemoveAt(masks.Count - 1);
        maskToBeRemoved.StealMask(Opponent);

        // Check if character is defeated
        if (masks.Count == 0)
        {
            OnDefeated();
        }
        else
        {
            topMask = masks[^1].transform;
            topMask.localScale = topMaskScale;
        }
    }

    /// <summary>
    /// Called when character runs out of masks
    /// </summary>
    protected virtual void OnDefeated()
    {
        Debug.Log($"{gameObject.name} has been defeated!");
        // Override in PlayerCharacter or AiCharacter for specific behavior
    }

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


        if (attackDirection == "left")
        {
            guyAnim.SetTrigger("HitLeft");

            // Remove a mask when hurt
            RemoveTopMask();
        }
        if (attackDirection == "right")
        {
            guyAnim.SetTrigger("HitRight");

            // Remove a mask when hurt
            RemoveTopMask();
        }
        if (attackDirection == "up")
        {
            guyAnim.SetTrigger("HitUp");

            // Remove a mask when hurt
            StealTopMask();
        }
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