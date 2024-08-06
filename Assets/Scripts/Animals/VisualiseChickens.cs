using System.Collections.Generic;
using UnityEngine;

public class VisualiseChickens : MonoBehaviour
{
    [SerializeField] private List<GameObject> pigSprites;

    [SerializeField] private PlayerInventory playerInventory;

    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        var chickenOrEgg = 1;//Mathf.CeilToInt(gameManager.GetNumChickens() / 4) + gameManager.GetNumEggs() / 10;

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
