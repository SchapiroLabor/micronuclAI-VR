using System.Collections;
using System.Collections.Generic;
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
    }
}

