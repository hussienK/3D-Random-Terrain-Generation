using UnityEngine;
using TMPro;
using System;

public class UISeed : MonoBehaviour
{
    [SerializeField] MapGenerator mapGenerator;
    [SerializeField] private TMP_InputField seedInput;
    private int seed;

    public void RegenerateMap()
    {
        Int32.TryParse(seedInput.text, out seed);
        mapGenerator.seed = seed;
        FindObjectOfType<EndlessTerrain>().UpdateVisibleChunks();
    }
}
