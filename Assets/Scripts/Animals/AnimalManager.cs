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

    private GameObject chickenUI;
    private GameObject pigUI;



    // Start is called before the first frame update
    void Start()
    {
        chickenUI = chickenSlot.parent.parent.gameObject;
        pigUI = pigSlot.parent.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Check if the UI is active
    public bool IsModalMode()
    {
        return chickenUI.activeSelf || pigUI.activeSelf;
    }

    //Disable the UI
    public void ExitModalMode()
    {
        chickenUI.SetActive(false);
        pigUI.SetActive(false);
    }

    //Add the animals to the SaveData
    public void Save(GameData gameData)
    {
        ISaveable.AddKey(gameData.gameManagerAnimalBuildings, "Chicken",
            chickenSlot.childCount > 0 ? chickenSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity : 0);
        ISaveable.AddKey(gameData.gameManagerAnimalBuildings, "Egg",
            eggSlot.childCount > 0 ? eggSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity : 0);
        ISaveable.AddKey(gameData.gameManagerAnimalBuildings, "Pig",
            pigSlot.childCount > 0 ? pigSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity : 0);
    }

    //Load the animals from the SaveData
    public void Load(GameData gameData, PlayerInventory playerInventory)
    {
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

    //Get the chicken slot
    public Transform GetChickenSlot()
    {
        return chickenSlot;
    }

    //Get the pig slot
    public Transform GetPigSlot()
    {
        return pigSlot;
    }

    public List<TileBase> GetPigPen()
    {
        return pigPen;
    }

    public List<TileBase> GetChickenCoop()
    {
        return chickenCoop;
    }

    public GameObject GetChickenUI()
    {
        return chickenUI;
    }

    public GameObject GetPigUI()
    {
        return pigUI;
    }

    //Get the  number of Pigs
    public int GetNumPigs()
    {
        if (chickenSlot.childCount > 0) return pigSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity;
        else return 0;
    }

    //Get the number of Chickens
    public int GetNumChickens()
    {
        if (chickenSlot.childCount > 0) return chickenSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity;
        else return 0;
    }

    //Get the number of Eggs
    public int GetNumEggs()
    {
        if (eggSlot.childCount > 0) return eggSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity;
        else return 0;
    }

    public void UpdateAnimals(PlayerInventory playerInventory)
    {
        // if at least one chicken in coop, attempt at egg production
        if (chickenSlot.childCount > 0)
        {
            // if no eggs, make new icon for eggs
            if (eggSlot.childCount == 0)
            {
                InventoryIcon newIcon = Instantiate(playerInventory.inventoryIcon, eggSlot).GetComponent<InventoryIcon>();
                newIcon.InitializeVariables();
                newIcon.SetIcon("Egg");
            }
            // make 0-[# of chickens] eggs
            InventoryIcon eggIcon = eggSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>();
            int newEggs = Random.Range(0, chickenSlot.GetChild(0).GetComponent<InventoryIcon>().quantity + 1);
            Debug.Log("Making " + newEggs + " eggs!");
            Debug.Log(eggIcon.quantity);
            eggIcon.UpdateQuantity(eggIcon.quantity + newEggs);
        }

        // if at least 2 pigs, attempt at pig production
        if (pigSlot.childCount > 0 && pigSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>().quantity > 1)
        {
            InventoryIcon pigIcon = pigSlot.GetChild(0).gameObject.GetComponent<InventoryIcon>();
            // make 0-[half total pigs] more pigs
            int newPigs = UnityEngine.Random.Range(0, ((int)Mathf.Floor(pigIcon.quantity / 2)) + 1);
            Debug.Log("Making " + newPigs + " pigs!");
            Debug.Log(pigIcon.quantity);
            pigIcon.UpdateQuantity(pigIcon.quantity + newPigs);
        }
    }

    /*public void LoadAnimal(string animal, int amount)
    {
        Debug.Log("Loading Animal");
        GameObject newIcon;
        switch (animal)
        {
            case "Chicken":
                chickenCoopInventory["Chicken"] = (int)Mathf.Max(0, chickenCoopInventory["Chicken"] + amount);
                newIcon = Instantiate(playerInventory.inventoryIcon, chickenSlot);
                playerInventory.StretchAndFill(newIcon.GetComponent<RectTransform>());
                newIcon.GetComponent<InventoryIcon>().SetIcon(animal);
                newIcon.GetComponent<InventoryIcon>().UpdateQuantity(amount);
                break;
            case "Pig":
                pigPenInventory = (int)Mathf.Max(0, pigPenInventory + amount);
                newIcon = Instantiate(playerInventory.inventoryIcon, pigSlot);
                playerInventory.StretchAndFill(newIcon.GetComponent<RectTransform>());
                newIcon.GetComponent<InventoryIcon>().SetIcon(animal);
                newIcon.GetComponent<InventoryIcon>().UpdateQuantity(amount);
                break;
            case "Egg":
                chickenCoopInventory["Egg"] = (int)Mathf.Max(0, chickenCoopInventory["Egg"] + amount);
                newIcon = Instantiate(playerInventory.inventoryIcon, eggSlot);
                playerInventory.StretchAndFill(newIcon.GetComponent<RectTransform>());
                newIcon.GetComponent<InventoryIcon>().SetIcon(animal);
                newIcon.GetComponent<InventoryIcon>().UpdateQuantity(amount);
                break;
        }
    }*/



    /*public void AddAnimal(string animal, int amount)
    {
        Debug.Log("Adding animal: " + animal + " count: " + amount);

        Func<int, Transform, int> UpdateAndCreateIcon = (count, parent) =>
        {
            bool wasZero = (count == 0);
            count = (int)Mathf.Max(0, count + amount);
            bool isZero = (count == 0);
            if (wasZero)
            {
                if (!isZero)
                {
                    GameObject newIcon = Instantiate(playerInventory.inventoryIcon, parent);
                    playerInventory.StretchAndFill(newIcon.GetComponent<RectTransform>());
                    newIcon.GetComponent<InventoryIcon>().SetIcon(animal);
                    newIcon.GetComponent<InventoryIcon>().UpdateQuantity(count);
                }
            }
            if (!wasZero && !isZero)
            {
                parent.GetChild(0).gameObject.GetComponent<InventoryIcon>().UpdateQuantity(count);
            }
            return count;
        };

        switch (animal)
        {
            case "Chicken":
                chickenCoopInventory["Chicken"] = UpdateAndCreateIcon(chickenCoopInventory["Chicken"], chickenSlot);
                break;
            case "Egg":
                chickenCoopInventory["Egg"] = UpdateAndCreateIcon(chickenCoopInventory["Egg"], eggSlot);
                break;
            case "Pig":
                pigPenInventory = UpdateAndCreateIcon(pigPenInventory, pigSlot);
                break;
            default:
                Debugger.Log("Unrecognized animal: " + animal);
                return;
        }
    }*/

    /*public void AddAnimal(string animal, int amount)
    {
        Debug.Log("Adding Animal");
        GameObject newIcon;
        switch (animal)
        {
            case "Chicken":
                chickenCoopInventory["Chicken"] = (int)Mathf.Max(0, chickenCoopInventory["Chicken"] + amount);
                if (chickenSlot.childCount == 0 && chickenCoopInventory["Chicken"] > 0)
                {
                    newIcon = Instantiate(playerInventory.inventoryIcon, chickenSlot);
                    newIcon.GetComponent<InventoryIcon>().SetIcon(animal);
                    newIcon.GetComponent<InventoryIcon>().UpdateQuantity(amount);
                }
                else chickenSlot.GetChild(0).GetComponent<InventoryIcon>().UpdateQuantity(chickenCoopInventory["Chicken"]);
                break;
            case "Pig":
                pigPenInventory = (int)Mathf.Max(0, pigPenInventory + amount);
                if (pigSlot.childCount == 0 && pigPenInventory > 0)
                {
                    newIcon = Instantiate(playerInventory.inventoryIcon, pigSlot);
                    newIcon.GetComponent<InventoryIcon>().SetIcon(animal);
                    newIcon.GetComponent<InventoryIcon>().UpdateQuantity(amount);
                }
                else if (amount != 0)
                {
                    //TODO There are places that amount can be ZERO  I would normally just put this check at start and return but because not to change functionallity it is here
                    pigSlot.GetChild(0).GetComponent<InventoryIcon>().UpdateQuantity(pigPenInventory);
                }
                break;
            case "Egg":
                chickenCoopInventory["Egg"] = (int)Mathf.Max(0, chickenCoopInventory["Egg"] + amount);
                if (eggSlot.childCount == 0 && chickenCoopInventory["Egg"] + amount > 0)
                {
                    newIcon = Instantiate(playerInventory.inventoryIcon, eggSlot);
                    newIcon.GetComponent<InventoryIcon>().SetIcon(animal);
                    newIcon.GetComponent<InventoryIcon>().UpdateQuantity(amount);
                }
                else if (amount != 0)
                {
                    //TODO There are places that amount can be ZERO  I would normally just put this check at start and return but because not to change functionallity it is here
                    eggSlot.GetChild(0).GetComponent<InventoryIcon>().UpdateQuantity(chickenCoopInventory["Egg"]);
                }
                break;
        }
    }

    public void AddAnimalNumb(string animal, int amount)
    {
        Debug.Log("Adding Animal dict only");
        switch (animal)
        {
            case "Chicken":
                chickenCoopInventory["Chicken"] = (int)Mathf.Max(0, chickenCoopInventory["Chicken"] + amount);
                break;
            case "Pig":
                pigPenInventory = (int)Mathf.Max(0, pigPenInventory + amount);
                break;
            case "Egg":
                chickenCoopInventory["Egg"] = (int)Mathf.Max(0, chickenCoopInventory["Egg"] + amount);
                break;
        }
    }*/
}