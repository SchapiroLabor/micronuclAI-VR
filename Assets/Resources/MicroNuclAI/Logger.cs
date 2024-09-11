using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
// Import functions from another script
using static InteractableImageStack;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Logger : MonoBehaviour
{
    string filename = "";
    void OnEnable() { Application.logMessageReceived += Log;  }
    void OnDisable() { Application.logMessageReceived -= Log; }

    public void Log(string logString, string stackTrace, LogType type)
    {
        if (filename == "")
        {
            string d = System.Environment.GetFolderPath(
              System.Environment.SpecialFolder.Desktop) + "/YOUR_LOGS";
            System.IO.Directory.CreateDirectory(d);
            filename = d + "/your_happy_log.txt";
        }

        try {


            // Get the name of the script and the function where the logger was called
            string scriptName = stackTrace.Substring(stackTrace.IndexOf("at ") + 3, stackTrace.IndexOf(".") - stackTrace.IndexOf("at ") - 3);
            string functionName = stackTrace.Substring(stackTrace.IndexOf(".") + 1, stackTrace.IndexOf("(") - stackTrace.IndexOf(".") - 1);

            // Log the script name and function name
            string functionLog = $"{logString} from {scriptName}.{functionName}";
            System.IO.File.AppendAllText(filename, functionLog + "\n" + $"{stackTrace}");
        }
        catch { }
    }
}