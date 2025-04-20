using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

//Create a GameManager or a similar singleton script that will persist between scenes.
public class ReferenceManager : MonoBehaviour
{
    public static ReferenceManager instance;

    // Reference to the InputField
    public InputField imagePathInput;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Output");

        if (!Directory.Exists(outputDirectory))
        {
            SchapiroLabLog.Log("Created directory " + SchapiroLabLog.FixFilePath(outputDirectory));
            Directory.CreateDirectory(outputDirectory);
        }
    }



}

