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
        rb.isKinematic = false;
        float force = Random.Range(5f, 20f);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        float torque = Random.Range(5f, 60f);
        rb.AddTorque(transform.right * torque, ForceMode.Impulse);
        boxCollider.enabled = true;
    }

    public void StealMask(Character stealer)
    {
        transform.parent = null;
        rb.isKinematic = false;
        boxCollider.enabled = true;

        float force = Random.Range(15f, 20f);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        float torque = Random.Range(25f, 60f);
        rb.AddTorque(transform.right * torque, ForceMode.Impulse);
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
