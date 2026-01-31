using UnityEngine;

[CreateAssetMenu(menuName = "Masks/Mask Data")]
public class MaskData : ScriptableObject
{
    public Texture2D texture;
    public string maskName;
    public string maskType;
}

