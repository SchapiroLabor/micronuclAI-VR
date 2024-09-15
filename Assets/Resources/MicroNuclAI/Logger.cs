using System.IO;
using UnityEngine;



public class Logger : MonoBehaviour
{

    public static  void Log(string logString)
    {   

            string d = System.Environment.GetFolderPath(
              System.Environment.SpecialFolder.Desktop) + "/YOUR_LOGS";
            System.IO.Directory.CreateDirectory(d);
            string filename = d + "/your_happy_log.txt";

        using (StreamWriter writer = new StreamWriter(filename, true))
        {
            writer.WriteLine(logString);
        }
    }
}