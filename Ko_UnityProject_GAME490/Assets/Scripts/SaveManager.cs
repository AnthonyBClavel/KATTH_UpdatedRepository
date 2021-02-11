using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveManager
{
    /**
    private string level;
    private GameObject checkpoint;
    private GameObject puzzle;
    **/

    private static string path = Application.persistentDataPath + "/game_save/character_data/";

    public static bool hasPath()
    {
        return Directory.Exists(path);
    }

    public static bool hasSaveFile()
    {
        return File.Exists(path + "character_save.txt");
    }

    //public void SaveGame(string level, GameObject checkpoint, GameObject puzzle)
    public static void SaveGame(SaveSlot character)
    {
        if (!hasPath())
        {
            Directory.CreateDirectory(path);
        }
        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = new FileStream(path + "character_save.txt", FileMode.Create);
        bf.Serialize(file, character);
        file.Close();
    }

    public static SaveSlot LoadGame()
    {
        SaveSlot character;
        if (!hasSaveFile())
        {
            Debug.Log("No file to load");
            return null;
        }
        if (File.Exists(path + "character_save.txt"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = new FileStream(path + "character_save.txt", FileMode.Open);

            character = bf.Deserialize(file) as SaveSlot;
            file.Close();
            return character;
        }
        return null;

    }

    public static void DeleteGame()
    {
        File.Delete(path + "character_save.txt");
    }
}
