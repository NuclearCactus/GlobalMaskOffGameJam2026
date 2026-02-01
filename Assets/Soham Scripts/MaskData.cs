using UnityEngine;

[CreateAssetMenu(menuName = "Masks/Mask Data")]
public class MaskData : ScriptableObject
{
    public Texture2D texture;
    public string maskName;
    public string maskType;

    [Tooltip("The phrase that pops up on the player's screen when this mask is knocked off or stolen")]
    [TextArea(1, 3)]
    public string popUpPhrase;
}

