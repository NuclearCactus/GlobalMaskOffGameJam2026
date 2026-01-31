using UnityEngine;

/// <summary>
/// Handles AI logic and informs character about that
/// (currently only moves in random directions)
/// </summary>
public class AiCharacter : Character
{
    [SerializeField] private float MovementTimer = 2f;
    private float timer = 0.0f;
    Vector3 currentDir = Vector3.zero;

    protected override void Update()
    {
        base.Update();
        // Change AI movement direction every x seconds
        timer += Time.deltaTime;
        if (timer > MovementTimer)
        {
            currentDir = GetRandomDir();
            timer = 0.0f;
            LeftAttack();
        }

        Move(currentDir);
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
