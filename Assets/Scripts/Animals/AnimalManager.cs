using System.Collections;
using System.Collections.Generic;
using SaveSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AnimalManager : MonoBehaviour
{
    [Header("Buildings")]
    public List<TileBase> pigPen;
    public List<TileBase> chickenCoop;
    [Space]
    [Header("Special Slots")]
    [SerializeField] private Transform pigSlot;
    [SerializeField] private Transform chickenSlot;
    [SerializeField] private Transform eggSlot;

    [SerializeField] private GameObject EggPrefab;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Save(GameData gameData){
        ISaveable.AddKey(gameData.gameManagerAnimalBuildings, "Chicken",
            chickenSlot.childCount > 0 ? chickenSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity : 0);
        ISaveable.AddKey(gameData.gameManagerAnimalBuildings, "Egg",
            eggSlot.childCount > 0 ? eggSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity : 0);
        ISaveable.AddKey(gameData.gameManagerAnimalBuildings, "Pig",
            pigSlot.childCount > 0 ? pigSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity : 0);
    }

    public void Load(GameData gameData, PlayerInventory playerInventory){
        foreach (var key_value in gameData.gameManagerAnimalBuildings)
        {
            var parsed = ISaveable.ParseKey(key_value);
            string animal = parsed[0];
            int count = System.Convert.ToInt32((string)parsed[1]);
            InventoryIcon newIcon = null;
            switch (animal)
            {
                case "Chicken":
                    newIcon = Instantiate(playerInventory.inventoryIcon, chickenSlot).GetComponent<InventoryIcon>();
                    break;
                case "Egg":
                    newIcon = Instantiate(playerInventory.inventoryIcon, eggSlot).GetComponent<InventoryIcon>();
                    break;
                case "Pig":
                    newIcon = Instantiate(playerInventory.inventoryIcon, pigSlot).GetComponent<InventoryIcon>();
                    break;
            }
            if (newIcon)
            {
                newIcon.InitializeVariables();
                Debug.Log("initialized");
                newIcon.SetIcon(animal);
                newIcon.UpdateQuantity(count);
            }
        }
    }
}
