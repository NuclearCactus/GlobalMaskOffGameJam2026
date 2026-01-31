using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MaskObject : MonoBehaviour
{
    private MaskData data;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int BaseMap =
        Shader.PropertyToID("_BaseMap"); // URP uses _BaseMap for the main texture

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void Setup(MaskData maskData)
    {
        data = maskData;

        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(BaseMap, maskData.texture);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    public MaskData GetData()
    {
        return data;
    }
}
