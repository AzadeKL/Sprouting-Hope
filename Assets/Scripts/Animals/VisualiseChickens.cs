using System.Collections.Generic;
using UnityEngine;

public class VisualiseChickens : MonoBehaviour
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
        var chickenOrEgg = Mathf.CeilToInt(farmGameManager.GetNumChickens() / 4) + farmGameManager.GetNumEggs() / 10;

        for (int i = 0; i < pigSprites.Count; i++)
        {
            if (chickenOrEgg > i)
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
