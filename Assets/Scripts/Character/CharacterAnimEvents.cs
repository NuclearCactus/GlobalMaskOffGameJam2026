using UnityEngine;

public class CharacterAnimEvents : MonoBehaviour
{
    [SerializeField] private Character character;
    [SerializeField] private SoundManager sound;

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

    public void EndDash()
    {
        character.EndDash();
    }

    public void Step()
    {
        sound.PlaySound(SoundType.FootStep);
    }
}
