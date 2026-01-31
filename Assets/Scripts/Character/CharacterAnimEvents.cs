using UnityEngine;

public class CharacterAnimEvents : MonoBehaviour
{
    [SerializeField] private Character character;

    public void EnableHitbox()
    {
        character.EnableHitbox();
    }

    public void DisableHitbox()
    {
        character.DisableHitbox();
    }

    public void EndAttack()
    {
        character.EndAttack();
    }

    public void EndHurt()
    {
        character.EndHurt();
    }
}
