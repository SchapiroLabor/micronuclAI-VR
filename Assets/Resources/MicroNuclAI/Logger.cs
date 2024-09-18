using System.IO;
using UnityEngine;



public class Logger : MonoBehaviour
{


    public static  void Log(string logString)
        

    {   string d = Path.Combine(Application.dataPath, "YOUR_LOGS");
        if (!Directory.Exists(d))
        {
            Directory.CreateDirectory(d);
        }
        string filename = Path.Combine(d, "game_logs.txt");

        using (StreamWriter writer = new StreamWriter(filename, true))
        {
            writer.WriteLine(logString);
        }
    }
}