using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameSaver : MonoBehaviour
{
    public static void SaveData(SaveFile saveFile, string path)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream fs = File.Create(path))
        {
            formatter.Serialize(fs, saveFile);
        }
    }

    public static SaveFile LoadData(string path)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream fs = File.Open(path, FileMode.OpenOrCreate))
        {
            try
            {
                return (SaveFile)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Debug.Log("failed bc of" + e.Message);
                return new SaveFile();
            }
            
        }
    }
}
