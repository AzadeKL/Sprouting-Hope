using System.Collections.Generic;
using UnityEngine;

public class SeedFactory : MonoBehaviour
{
    [SerializeField] private List<GameObject> seeds;
    [SerializeField] private List<GameObject> crops;
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

    
    public void CreateCrop(Vector3 pos, string crop)
    {
        if (crops.Count == 0) return;

        int cropCount;
        switch (crop)
        {
            case "Wheat":
                cropCount = Random.Range(1, 2);

                for (int i = 0; i < cropCount; i++)
                {
                    Instantiate(crops[0], pos, Quaternion.identity);
                }
                break;
            case "Tomato":
                cropCount = Random.Range(2, 4);

                for (int i = 0; i < cropCount; i++)
                {
                    Instantiate(crops[1], pos, Quaternion.identity);
                }
                break;
            case "Lentil":
                cropCount = Random.Range(1, 3);

                for (int i = 0; i < cropCount; i++)
                {
                    Instantiate(crops[2], pos, Quaternion.identity);
                }
                break;
        }

    }

}
