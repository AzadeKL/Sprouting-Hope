using System.Collections.Generic;
using UnityEngine;

public class VisualisePigs : MonoBehaviour
{
    [SerializeField] private List<GameObject> pigSprites;

    [SerializeField] private PlayerInventory playerInventory;

    private FarmGameManager farmGameManager;

    private void Awake()
    {
        farmGameManager = GameObject.Find("GameManager").GetComponent<FarmGameManager>();
    }

    private void Update()
    {
        var pigCount = farmGameManager.GetNumPigs();

        for (int i = 0; i < pigSprites.Count; i++)
        {
            if (pigCount > i)
            {
                pigSprites[i].SetActive(true);
            }
            else
            {
                pigSprites[i].SetActive(false);
            }

        }
    }
}
