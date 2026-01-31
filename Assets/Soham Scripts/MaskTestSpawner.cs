using UnityEngine;

public class MaskTestSpawner : MonoBehaviour
{
    void Start()
    {
        MaskObject mask = NewMaskManager.Instance.CreateRandomMask();
        mask.transform.position = Vector3.zero;
    }
}
