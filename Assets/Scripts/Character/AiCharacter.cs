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
    private readonly float idleTime = 2f;
    private float idleTimer = 0f;
    private readonly float moveTime = 1f;
    private float moveTimer = 1f;
    private Vector3 currentDir = Vector3.zero;

    private readonly float defensiveTime = 0.5f;
    private float defensiveTimer = 0f;


    protected override void Update()
    {
        base.Update();
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
    }

    private void HandleIdle()
    {
        moveTimer += Time.deltaTime;
        if(moveTimer > moveTime)
        {
            currentDir = GetRandomDir();
            moveTimer = 0f;
        }
        Move(currentDir);

        idleTimer += Time.deltaTime;
        if (idleTimer > idleTime) 
        {
            currentState = AiState.Offensive;
            idleTimer = 0f;
        }
    }

    private void HandleOffensive()
    {
        Move(Opponent.transform.position - transform.position);
        if (Vector2.Distance(transform.position, Opponent.transform.position) < 1)
        {
            LeftAttack();
            RightAttack();
            currentState = AiState.Defensive;
        }
    }

    private void HandleDefensive()
    {
        Move(transform.position - Opponent.transform.position);
        defensiveTimer += Time.deltaTime;
        if(defensiveTimer > defensiveTime)
        {
            defensiveTimer = 0f;
            currentState = AiState.Idle;
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
