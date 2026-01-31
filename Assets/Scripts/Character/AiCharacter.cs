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

    private AiState currentState = AiState.Idle;
    private readonly float idleTime = 0.5f;
    private float idleTimer = 0f;
    private readonly float moveTime = 0.25f;
    private float moveTimer = 1f;
    private Vector3 currentDir = Vector3.zero;

    private readonly float defensiveTime = 1f;
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
        if (isHurt)
        {
            currentState = Random.Range(0, 2) == 0 ? AiState.Offensive : AiState.Defensive;
        }
    }

    private void HandleIdle()
    {
        moveTimer += Time.fixedDeltaTime;
        if (moveTimer > moveTime)
        {
            currentDir = GetRandomDir();
            moveTimer = 0f;
        }
        Move(currentDir);

        idleTimer += Time.fixedDeltaTime;
        if (idleTimer > idleTime)
        {
            currentState = AiState.Offensive;
            idleTimer = 0f;
        }
    }

    private void HandleOffensive()
    {
        Move(Opponent.transform.position - transform.position);
        if (IsInAttackRange())
        {
            Attack();
        }
    }

    private void HandleDefensive()
    {
        Move(transform.position - Opponent.transform.position);
        defensiveTimer += Time.fixedDeltaTime;
        if (defensiveTimer > defensiveTime)
        {
            defensiveTimer = 0f;
            currentState = AiState.Idle;
        }
    }

    private bool IsInAttackRange()
    {
        return Vector2.Distance(transform.position, Opponent.transform.position) < 1f;
    }

    private void Attack()
    {
        currentState = AiState.Idle;
        defensiveTimer = 0f;

        if (Opponent.isHurt) return;

        if (leftTimer > attackCd)
        {
            LeftAttack();
            return;
        }

        if (rightTimer > attackCd)
        {
            RightAttack();
            return;
        }

        if (uppercutTimer > attackCd)
        {
            UpperCut();
            return;
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
