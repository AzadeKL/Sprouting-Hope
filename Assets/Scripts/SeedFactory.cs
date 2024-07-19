using System.Collections.Generic;
using UnityEngine;

public class SeedFactory : MonoBehaviour
{
    [SerializeField] private List<GameObject> seeds;
    [SerializeField] private int seedRandomSpawnUpperLimit = 5;

    void Awake()
    {
        if (seeds == null) seeds = new List<GameObject>();

    }



    public void CreateSeed(Vector3 pos)
    {
        if (seeds.Count == 0) return;

        var seedCount = Random.Range(1, Mathf.Max(2, seedRandomSpawnUpperLimit));

        for (int i = 0; i < seedCount; i++)
        {
            var seedTypeIndex = Random.Range(0, seeds.Count);
            Instantiate(seeds[seedTypeIndex], pos, Quaternion.identity);
        }

    }


}
