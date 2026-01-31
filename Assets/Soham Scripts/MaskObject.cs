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

    public void PopMask()
    {
        transform.parent = null;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        float force = Random.Range(5f, 20f);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        float torque = Random.Range(5f, 60f);
        rb.AddTorque(transform.right * torque, ForceMode.Impulse);
        GetComponent<BoxCollider>().enabled = true;
    }

    public MaskData GetData()
    {
        return data;
    }
}
