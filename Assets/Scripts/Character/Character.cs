using System.Collections.Generic;
using UnityEngine;

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
    private bool isHurt = false;

    // === MASK SYSTEM ===
    [Header("Mask Settings")]
    [SerializeField] private Transform maskAnchor; // Assign the head/face transform in inspector
    [SerializeField] private int initialMaskCount = 5;
    [SerializeField] private float maskSpacing = 0.01f; // Horizontal spacing between stacked masks
    
    private List<MaskObject> masks = new List<MaskObject>();
    
    // Public accessors
    public MaskObject TopMask => masks.Count > 0 ? masks[masks.Count - 1] : null;
    public int MaskCount => masks.Count;
    public bool IsAlive => masks.Count > 0;

    private void Start()
    {
        rightTimer = attackCd;
        leftTimer = attackCd;
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
        
        // Parent to the mask anchor (head)
        newMask.transform.SetParent(maskAnchor, false);
        
        // Position it in the stack (each mask slightly forward/outward)
        newMask.transform.localPosition = new Vector3(0, masks.Count * maskSpacing, 0);
        newMask.transform.localRotation = Quaternion.identity;
        
        masks.Add(newMask);
    }

    /// <summary>
    /// Removes and destroys the top mask (when taking damage)
    /// </summary>
    public void RemoveTopMask()
    {
        if (masks.Count == 0) return;

        MaskObject topMask = masks[masks.Count - 1];
        masks.RemoveAt(masks.Count - 1);
        Destroy(topMask.gameObject);

        // Check if character is defeated
        if (masks.Count == 0)
        {
            OnDefeated();
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

    protected virtual void Update()
    {
        rightTimer += Time.deltaTime;
        leftTimer += Time.deltaTime;
        LookAtOpponent();
    }

    public void Move(Vector3 dir)
    {
        if (isAttacking || isHurt) return;

        Vector3 deltaPos = transform.position + (speed * Time.fixedDeltaTime * dir.normalized);

        deltaPos = GameManager.Instance.ClampCharacterPosInBounds(deltaPos);

        if (deltaPos != transform.position)
            rb.MovePosition(deltaPos);

        guyAnim.SetFloat("MoveX", dir.x);
        guyAnim.SetFloat("MoveY", dir.z);
    }

    public void LookAtOpponent()
    {
        transform.LookAt(Opponent.transform.position);
    }

    public void LeftAttack()
    {
        if (leftTimer <= attackCd || isAttacking || isHurt) return;
        leftTimer = 0f;
        guyAnim.SetTrigger("PunchL");
        attackType = "left";
        isAttacking = true;
    }

    public void RightAttack()
    {
        if (rightTimer <= attackCd || isAttacking || isHurt) return;
        rightTimer = 0f;
        guyAnim.SetTrigger("PunchR");
        attackType = "right";
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

        // Remove a mask when hurt
        RemoveTopMask();

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
        if (other.CompareTag(enemyTag) && other.TryGetComponent<Character>(out var character) && character != this)
        {
            character.Hurt(attackType);
            Debug.Log(other.name + " took damage - Masks remaining: " + character.MaskCount);
        }
    }
}