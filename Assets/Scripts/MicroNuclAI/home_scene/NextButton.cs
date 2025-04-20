using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;



namespace HomeScene
{
    // This script is used to load the next scene when the button is clicked
    // It also checks if the input fields are not empty and assigns them to the GameManager
    // The GameManager is a scriptable object that stores the input folder and python executable path
    // The script is attached to the button in the home scene
    public class NextButtom : MonoBehaviour
    {
        // Start is called before the first frame update

        [SerializeField] private GameManaging gameManaging;
        [SerializeField] private InputFields inputFields;

        void Start()
        {
            // Why do we need to load the GameManaging scriptable object here?
            if (gameManaging == null)
            {
                // Load from path
                gameManaging = Resources.Load<GameObject>(Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/SceneManager.prefab"))).GetComponent<GameManaging>();
            }

            if (inputFields == null)
            {
                inputFields = GameObject.Find("inputFields").GetComponent<InputFields>();
            }

            // Initialize the input fields
            Initialize();
        }

        void Initialize()

        {

            // Add button click event
            transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { OnConfirmButtonClick(); });
        }

        void OnConfirmButtonClick()
        {

            // Check !!!!!!!!!!!!!!!!!!!!!!!
            // Understand why Unity states that InputFields does not contain a definition for GetDataFolder and GetPythonExecutable
            List<string> output = new List<string> { inputFields.GetDataFolder(), inputFields.GetPythonExecutable() };

            bool valid = output.Exists(q => q != null);

            if (valid)
            {

                // Assign the input fields to the GameManager
                // Get text provided in List<string> output
                gameManaging.GetComponent<GameManaging>().InputFolder = output[0];
                gameManaging.GetComponent<GameManaging>().PythonExecutable = output[1];

                // Load the next scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("CI-Annotator");
            }

            else
            {
                Debug.Log($"Input fields are empty: {output}");
            }



        }

    }

}