using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MaskObject : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private MeshRenderer maskRenderer;

    private MaskData data;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int BaseMap =
        Shader.PropertyToID("_BaseMap"); // URP uses _BaseMap for the main texture

    private void Awake()
    {
        meshRenderer = maskRenderer;
        propertyBlock = new MaterialPropertyBlock();
        rb.isKinematic = true;
        boxCollider.enabled = false;
    }

    private void FlipMask(float force, float torque)
    {
        transform.parent = null;
        rb.isKinematic = false;
        boxCollider.enabled = true;
        Vector3 forceDir = Random.onUnitSphere;
        if (forceDir.y < 0) forceDir.y *= -1f;
        Vector3 torqueDir = Random.onUnitSphere;
        rb.AddForce(forceDir * force, ForceMode.Impulse);
        rb.AddTorque(torqueDir * torque, ForceMode.Impulse);
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
        float force = Random.Range(5f, 20f);
        float torque = Random.Range(5f, 60f);
        FlipMask(force, torque);
    }

    public void StealMask(Character stealer)
    {
        float force = Random.Range(15f, 20f);
        float torque = Random.Range(25f, 60f);
        FlipMask(force, torque);

        StartCoroutine(AttachMaskAfterDelay(stealer));
    }

    public MaskData GetData()
    {
        return data;
    }

    private IEnumerator AttachMaskAfterDelay(Character stealer)
    {
        yield return new WaitForSeconds(1.5f);
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        boxCollider.enabled = false;
        Vector3 startPos = transform.position;
        Vector3 currentPos;
        float timer = 0f;
        float transitionTime = 3f;
        while (timer < transitionTime)
        {
            currentPos = Vector3.Lerp(startPos, stealer.MaskAnchor.position, timer / transitionTime);
            transform.position = currentPos;
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Mask stolen");
        stealer.AddMask(this);
    }
}
