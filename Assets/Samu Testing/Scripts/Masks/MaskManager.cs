using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Contains the masked mapped to names
/// Returns instances of given mask type
/// </summary>
public class MaskManager : MonoBehaviour
{
    // Singleton
    public static MaskManager Instance { get; private set; }

    // Class for sorting in unity inspector
    [Serializable]
    private class MaskInstance
    {
        public MaskType type;
        public GameObject prefab;
    }

    // Masks
    [SerializeField] private MaskInstance[] masks;
    private readonly Dictionary<MaskType, GameObject> maskDict = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Populate Masks
        foreach (var mask in masks)
        {
            maskDict.Add(mask.type, mask.prefab);
        }
    }

    /// <summary>
    /// Returns random mask
    /// </summary>
    /// <returns></returns>
    public GameObject GetRandomMask()
    {
        var values = Enum.GetValues(typeof(MaskType));
        int index = UnityEngine.Random.Range(0, values.Length);
        MaskType type = (MaskType)values.GetValue(index);
        return GetMask(type);
    }


    /// <summary>
    /// Returns mask corresponding to MaskType
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameObject GetMask(MaskType type)
    {
        maskDict.TryGetValue(type, out GameObject mask);
        if (mask == null) return null;
        return Instantiate(mask);
    }
}
