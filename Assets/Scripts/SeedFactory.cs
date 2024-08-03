using System.Collections.Generic;
using UnityEngine;

public class SeedFactory : MonoBehaviour
{
    [SerializeField] private List<GameObject> seeds;
    [SerializeField] private List<GameObject> crops;

    void Awake()
    {
        if (seeds == null) seeds = new List<GameObject>();

    }

    private int itemModifier = 0;

    public void CreateSeed(Vector3 pos)
    {
        if (seeds.Count == 0) return;

        // weighted seed spawning
        var seedCount = Random.Range(1, 2 + itemModifier);
        if (Random.Range(0, 100) < 30) seedCount = Random.Range(1, 4 + itemModifier);

        for (int i = 0; i < seedCount; i++)
        {
            Instantiate(seeds[0], pos, Quaternion.identity);
        }

    }


    public void CreateCrop(Vector3 pos, string crop)
    {
        if (crops.Count == 0) return;

        int cropCount;
        switch (crop)
        {
            case "Wheat":
                cropCount = Random.Range(1, 2 + itemModifier);

                for (int i = 0; i < cropCount; i++)
                {
                    Instantiate(crops[0], pos, Quaternion.identity);
                }
                break;
            case "Tomato":
                cropCount = Random.Range(2, 4 + itemModifier);

                for (int i = 0; i < cropCount; i++)
                {
                    Instantiate(crops[1], pos, Quaternion.identity);
                }
                // 25% chance to get seed
                if (Random.Range(0, 100) < 25) Instantiate(seeds[1], pos, Quaternion.identity);
                break;
            case "Lentils":
                cropCount = Random.Range(1, 3 + itemModifier);

                for (int i = 0; i < cropCount; i++)
                {
                    Instantiate(crops[2], pos, Quaternion.identity);
                }
                // 50% chance to get seed
                if (Random.Range(0, 100) < 50) Instantiate(seeds[2], pos, Quaternion.identity);
                break;
        }

    }

    public void HandleHandChange(GameObject item)
    {
        if (item == null)
        {
            itemModifier = 0;
        }
        else
        {
            var name = item.GetComponent<InventoryIcon>().item;
            //Debugger.Log("Name im HandChange " + name, Debugger.PriorityLevel.High);
            int result = name switch
            {
                string a when a.Contains("Rusty") => 1,
                string b when b.Contains("Bronze") => 3,
                string b when b.Contains("Silver") => 5,
                string b when b.Contains("Gold") => 10,
                _ => 0
            };
            itemModifier = result;
            //Debugger.Log("Result of HandChange " + result, Debugger.PriorityLevel.High);
        }
    }

}
