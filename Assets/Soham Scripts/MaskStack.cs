using System.Collections.Generic;
using UnityEngine;

public class MaskStack : MonoBehaviour
{
    [SerializeField] private Transform maskAnchor; // The head transform where masks attach
    [SerializeField] private int initialMaskCount = 5;
    [SerializeField] private float maskSpacing = 0.01f; // Z-offset between stacked masks
    
    private Stack<MaskObject> masks = new Stack<MaskObject>();
    
    public MaskObject TopMask => masks.Count > 0 ? masks.Peek() : null;
    public int MaskCount => masks.Count;

    private void Start()
    {
        GenerateInitialMasks();
    }

    private void GenerateInitialMasks()
    {
        for (int i = 0; i < initialMaskCount; i++)
        {
            AddRandomMask();
        }
    }

    public void AddRandomMask()
    {
        MaskObject newMask = NewMaskManager.Instance.CreateRandomMask();
        AddMask(newMask);
    }

    public void AddMask(MaskObject mask)
    {
        // Parent to the mask anchor (head)
        mask.transform.SetParent(maskAnchor, false);
        
        // Position it in the stack (further forward with each mask)
        mask.transform.localPosition = new Vector3(0, 0, masks.Count * maskSpacing);
        mask.transform.localRotation = Quaternion.identity;
        
        masks.Push(mask);
    }

    public MaskObject RemoveTopMask()
    {
        if (masks.Count == 0)
            return null;

        MaskObject removed = masks.Pop();
        return removed;
    }

    public void DestroyTopMask()
    {
        MaskObject removed = RemoveTopMask();
        if (removed != null)
            Destroy(removed.gameObject);
    }
}