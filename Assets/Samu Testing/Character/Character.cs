using UnityEngine;

/// <summary>
/// Abstract base class for character
/// Ai and Player derive from this class
/// Handles effects that both player and AI should have
/// </summary>
public abstract class Character : MonoBehaviour
{
    [SerializeField] private float speed = 10;

    public void Move(Vector3 dir)
    {
        transform.position += speed * Time.deltaTime * dir;
    }
}
