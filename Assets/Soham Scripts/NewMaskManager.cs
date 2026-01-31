using System.Collections.Generic;
using UnityEngine;

public class NewMaskManager : MonoBehaviour
{
    public static NewMaskManager Instance { get; private set; }

    [SerializeField] private GameObject maskPrefab;
    [SerializeField] private List<MaskData> allMasks;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public MaskObject CreateMask(MaskData data)
    {
        GameObject obj = Instantiate(maskPrefab);
        MaskObject mask = obj.GetComponent<MaskObject>();
        mask.Setup(data);
        return mask;
    }

    public MaskObject CreateRandomMask()
    {
        int index = Random.Range(0, allMasks.Count);
        return CreateMask(allMasks[index]);
    }
}
