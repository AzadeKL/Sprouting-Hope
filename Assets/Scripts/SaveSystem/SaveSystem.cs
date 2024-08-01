using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace SaveSystem
{
    /*
     * How to add data to save system:
     * 1. Add a List<string> for your data, named for your class.
     * 2. Extend and implement ISaveable, to write-to(Save)/read-from(Load) your data.
     *    * Save will be called automatically
     *    * Call SaveSystem.DataManager.instance.Load(this), to load data from file, if specified
     *      * Load should probably be in Awake or Start
     *    * Data may be stored in a List<string> (Dictionaries cannot be converted to json)
     *      * AddKey and ParseKey have been provided to pack/unpack the data.
     */
    [System.Serializable]
    public class GameData
    {
        public string sceneName;
        public int sceneIndex;

        public List<string> dayNightCycleData = new List<string>();

        public List<string> playerInventoryData = new List<string>();
        public List<string> playerInventoryInventoryKeys = new List<string>();
        public List<int> playerInventoryInventoryValues = new List<int>();

        public List<string> gameManagerPlants = new List<string>();
    }
    public interface ISaveable
    {
        // Copy data to gameData
        public void Save(GameData gameData);

        // Copy data from gameData
        // * Return false, if data does not exist
        public bool Load(GameData gameData);

        // Utility for packaging data for GameData lists
        static public void AddKey<T>(List<string> values, string key, T value)
        {
            var key_value = string.Join(":", key, value);
            values.Add(key_value);
            //Debugger.Log("Added key: " + key_value);
        }

        // Utility for unpacking data from GameData lists
        static public string[] ParseKey(string key_value)
        {
            //Debugger.Log("Parsed key: " + key_value);
            return key_value.Split(':', 2);
        }
    }
}
