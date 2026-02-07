using UnityEngine;

/// <summary>
/// Handles AI logic and informs character about that
/// (currently only moves in random directions)
/// </summary>
public class AiCharacter : Character
{
    private enum AiState
    {
        Idle,
        Offensive,
        Defensive
    }

    [SerializeField] private float attackRangeDetection = 1f;

    [SerializeField] private AiState currentState = AiState.Idle;
    private readonly float idleTime = 2.5f;
    private float idleTimer = 0f;
    private readonly float moveTime = 0.5f;
    private float moveTimer = 1f;
    private Vector3 currentDir = Vector3.zero;

    private readonly float defensiveTime = 1.5f;
    private float defensiveTimer = 0f;


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        switch (currentState)
        {
            case AiState.Idle:
                HandleIdle();
                break;
            case AiState.Offensive:
                HandleOffensive();
                break;
            case AiState.Defensive:
                HandleDefensive();
                break;
            default:
                break;
        }
        if (isHurt && currentState == AiState.Offensive)
        {
            currentState = AiState.Defensive;
        }
        Move(currentDir);
    }

    private void HandleIdle()
    {
        moveTimer += Time.fixedDeltaTime;
        if (moveTimer > moveTime)
        {
            currentDir = GetRandomDir();
            moveTimer = 0f;
        }

        idleTimer += Time.fixedDeltaTime;
        if (idleTimer > idleTime)
        {
            currentState = AiState.Offensive;
            idleTimer = 0f;
            moveTimer = moveTime;
        }

        if (IsInAttackRange())
        {
            Attack();
        }
    }

    private void HandleOffensive()
    {
        currentDir = Opponent.transform.position - transform.position;
        if (IsInAttackRange())
        {
            Attack();
        }
    }

    private void HandleDefensive()
    {
        currentDir = -(Opponent.transform.position - transform.position);
        defensiveTimer += Time.fixedDeltaTime;
        if (defensiveTimer > defensiveTime)
        {
            defensiveTimer = 0f;
            currentState = AiState.Idle;
        }

        if (stamina > dashCd && IsInAttackRange())
        {
            StartDash();
        }
    }

    private bool IsInAttackRange()
    {
        return Vector3.Distance(transform.position, Opponent.transform.position) < attackRangeDetection;
    }

    private void Attack()
    {
        currentState = AiState.Defensive;
        defensiveTimer = 0f;

        if (Opponent.isHurt) return;

        if (stamina > attackCd)
        {
            int random = Random.Range(0, 3);
            if (random == 0)
                UpperCut();
            else if (random == 1)
                LeftAttack();
            else
                RightAttack();
        }
    }

    /// <summary>
    /// Simple function to get a random direction
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomDir()
    {
        Vector3 dir = new()
        {
            x = Random.Range(-1f, 1f),
            z = Random.Range(-1f, 1f)
        };
        return dir.normalized;
    }
}
