using System.Collections.Generic;
using UnityEngine;

public class VisualisePigs : MonoBehaviour
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
        var pigCount = gameManager.GetNumPigs();

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
