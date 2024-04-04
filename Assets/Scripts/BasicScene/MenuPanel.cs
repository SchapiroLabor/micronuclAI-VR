using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public string nextSceneName = "SampleScene";  // Set the next scene name in the Inspector
    public Button NextButton;  // Button for changing the scene
    //public InputField ImagePath;

    void Start()
    {    //ReferenceManager referenceManager = FindObjectOfType<ReferenceManager>();
        // Ensure that components are assigned in the Inspector
        if (NextButton == null)
        {
            Debug.LogError("Button component is not assigned.");
        }
        else
        {

            // Add a listener to the button to call ChangeScene method when clicked
            NextButton.onClick.AddListener(ChangeScene);
                        // Assign the input field reference
            //referenceManager.imagePathInput = ImagePath;
        }
    }

    void ChangeScene()
    {
        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}

