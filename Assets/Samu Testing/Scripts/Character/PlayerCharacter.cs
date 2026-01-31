using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detects player input and informs character about that
/// </summary>
public class PlayerCharacter : Character
{
    protected override void Update()
    {
        base.Update();
        Vector3 MovementDir = Vector3.zero;
        if (Keyboard.current.wKey.isPressed)
        {
            MovementDir.z = 1;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            MovementDir.z = -1;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            MovementDir.x = 1;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            MovementDir.x = -1;
        }
        Move(MovementDir.normalized);
        if (Mouse.current.leftButton.isPressed)
        {
            LeftAttack();
        }
        if (Mouse.current.rightButton.isPressed)
        {
            RightAttack();
        }
    }
}
