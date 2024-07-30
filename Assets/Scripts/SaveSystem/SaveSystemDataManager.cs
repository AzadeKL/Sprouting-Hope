using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using SaveSystem;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Data.Common;

namespace SaveSystem
{
    public class DataManager : MonoBehaviour
    {
        private GameData gameData;

        [SerializeField]
        private string autoSaveDirectory;
        [SerializeField]
        private string autoSaveFilename = "autosave.dat";
        [SerializeField]
        private int updatesPerSave = 600;

        private FileManager autoSaveFileManager;

        private int updateCounter = 2; // Ensure everything is updated, before first save

        public static DataManager instance;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                gameData = null;
                autoSaveFileManager = new FileManager(autoSaveDirectory, autoSaveFilename);
                LoadFromFile();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (--updateCounter > 0)
            {
                return;
            }
            updateCounter = updatesPerSave;
            if (UpdateGameData())
            {
                SaveToFile();
            }
        }
        private void OnApplicationQuit()
        {
            SaveToFile();
        }

        public bool HasLoadedGameData()
        {
            return gameData != null;
        }

        public bool HasCurrentSceneData()
        {
            return (gameData != null) && (gameData.sceneIndex == SceneManager.GetActiveScene().buildIndex);
        }

        public bool Load(ISaveable saveable)
        {
            //Debugger.Log("Loading class: " + saveable.GetType().Name);
            //Debugger.Log("instance.HasCurrentSceneData(): " + instance.HasCurrentSceneData());
            if (!instance.HasCurrentSceneData())
            {
                //Debugger.Log("Loading disabled for class: " + saveable.GetType().Name);
                return false;
            }

            return saveable.Load(gameData);
        }

        public int GetLastSceneIndex()
        {
            return gameData.sceneIndex;
        }

        public void SaveToFile()
        {
            if ((gameData != null) && (autoSaveFileManager != null))
            { 
                autoSaveFileManager.Save(gameData);
            }
        }

        public void LoadFromFile()
        {
            gameData = autoSaveFileManager.Load();
        }

        public void ResetGameData()
        {
            gameData = null;
        }

        private bool UpdateGameData()
        {
            IEnumerable<ISaveable> saveableObjs = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
            if (!saveableObjs.Any())
            {
                return false;
            }
            gameData = new GameData();
            gameData.sceneName = SceneManager.GetActiveScene().name;
            gameData.sceneIndex = SceneManager.GetActiveScene().buildIndex;
            foreach (var obj in saveableObjs)
            {
                obj.Save(gameData);
            }
            return true;
        }


        public class FileManager
        {
            private string filePath;
            public FileManager(string directory, string fileName)
            {
                filePath = System.IO.Path.Combine(directory, fileName);
                //Debugger.Log("Autosave relative path: " + filePath);
                filePath = System.IO.Path.GetFullPath(filePath);
                //Debugger.Log("Autosave absolute path: " + filePath);
            }
            public bool Save(GameData gameData)
            {
                //Debugger.Log("Saving game to file: " + filePath);
                string data;
                try
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            data = JsonUtility.ToJson(gameData);
                            writer.Write(data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Log("Failed to save game data: ERROR: " + ex.ToString());
                    return false;

                }
                return true;
            }

            public GameData Load()
            {
                //Debugger.Log("Loading game from file: " + filePath);
                GameData gameData = null;
                string data;
                try
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                    using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
                    {
                        using (StreamReader reader = new StreamReader(stream))

                        data = reader.ReadToEnd();
                        gameData = JsonUtility.FromJson<GameData>(data);
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Log("Failed to load game data: ERROR: " + ex.ToString());
                }
                return gameData;
            }
        }
    }
}
