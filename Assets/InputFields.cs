using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.XR;
using System.Web;
using System;
using System.IO;
using System.Linq;
using static UnityEngine.InputSystem.InputControlScheme.MatchResult;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using Unity.VisualScripting;


public class InputFields : MonoBehaviour
{

    public List<Transform> inputs = new List<Transform>();

    public Transform Initialize(Transform MenuPanel)
    {   
        // Set MenuPanel as parent
        transform.SetParent(MenuPanel);

        // Load the canvas
        gameObject.name = "InputFields";

        // Canvas anchors to left bottom
        GetComponent<RectTransform>().anchorMin = new UnityEngine.Vector2(0, 0);
        GetComponent<RectTransform>().anchorMax = new UnityEngine.Vector2(0, 0);

        // Set Pivot to centre
        GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 0.5f);

        // Spacing set to 1 % of canvas height
        //gameObject.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing = MenuPanel.GetComponent<RectTransform>().sizeDelta.y * 0.01f;

        // Position canvas at near clip plane
        //transform.localPosition = Vector3.zero;


        // Initialize input widget for image path
        Transform inputWidget = InitializeInputWidget4Imgs(MenuPanel);
        inputs.Add(inputWidget);

        // Initialize input widget for python executable path
        //InitializeInputWidget4PythonExec(MenuPanel);

        return transform;
    }


    Transform InitializeInputWidget4Imgs(Transform Canvas)
    {   
        // Load the input widget
        Transform inputWidget = transform.GetChild(0);

        if (inputWidget is null)
        {
        // Check if it is not null
        Logger.Log($"input null");

        }
        else
        {
            Logger.Log($"input not null");
        }



        inputWidget.gameObject.name = "ImagePath";

        Logger.Log($"input name: {inputWidget.gameObject.name}");

        // Set anchors to bottom left
        inputWidget.GetComponent<RectTransform>().anchorMin = new UnityEngine.Vector2(0, 0);
       inputWidget.GetComponent<RectTransform>().anchorMax = new UnityEngine.Vector2(0, 0);

       Logger.Log($"input anchors: {inputWidget.GetComponent<RectTransform>().anchorMin}");

        // Set Pivot to centre
        inputWidget.GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 0.0f);

        Logger.Log($"input pivot: {inputWidget.GetComponent<RectTransform>().pivot}");

        UnityEngine.Vector2 size = Canvas.GetComponent<RectTransform>().sizeDelta;

        Logger.Log($"input size: {size}");
        // Set position to the centre
        inputWidget.transform.position = new UnityEngine.Vector2(size.x*0.5f, size.y*0.5f);

        Logger.Log($"input position: {inputWidget.transform.position}");
        // Set scale to 1
        inputWidget.localScale = new UnityEngine.Vector3(1, 1, 1);
        Logger.Log($"input scale: {inputWidget.localScale}");

        // Set the text of the placeholder
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().text = "Enter path to Input folder";

        Logger.Log($"input text: {inputWidget.GetComponentInChildren<TextMeshProUGUI>().text}");

        // Enable overflow
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = true;

        Logger.Log($"input overflow: {inputWidget.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping}");

        // Set color of text to grey
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;

        Logger.Log($"input color: {inputWidget.GetComponentInChildren<TextMeshProUGUI>().color}");

        inputWidget = SetSizeofInputBar(inputWidget, Canvas);

        Logger.Log($"input size: {inputWidget.GetComponent<RectTransform>().sizeDelta}");


        return inputWidget;
    }


    Transform InitializeInputWidget4PythonExec(Transform Canvas)
    {   
        // Load the input widget
        Transform inputWidget = transform.GetChild(1);

        inputWidget.name = "PythonExecPath";

                // Set anchors to bottom left
        inputWidget.GetComponent<RectTransform>().anchorMin = new UnityEngine.Vector2(0, 0);
        inputWidget.GetComponent<RectTransform>().anchorMax = new UnityEngine.Vector2(0, 0);

        // Set Pivot to centre
        inputWidget.GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 1.0f);

        UnityEngine.Vector2 size = Canvas.GetComponent<RectTransform>().sizeDelta;
        // Set position to the centre
        inputWidget.transform.position = new UnityEngine.Vector2(size.x*0.5f, size.y*0.54f);


                // Set scale to 1
        inputWidget.localScale = new Vector3(1, 1, 1);


        // Set placeholder text
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().text  = "Path to Python Executable";

        // Enable overflow
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = true;

        // Set color of text to grey
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;

        inputWidget = SetSizeofInputBar(inputWidget, Canvas);



        return inputWidget;
    }

 Transform SetSizeofInputBar(Transform Inputbar, Transform MenuPanel)
    {

        // Set size of input bar to 25% of width and 5% of height of MenuPanel
        Vector2 size = MenuPanel.GetComponent<RectTransform>().sizeDelta;

        Inputbar.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x*0.4f, size.y*0.05f);

        return Inputbar;



    }

public string AddQuotesIfRequired(string path)
{
    return !string.IsNullOrWhiteSpace(path) ? 
        path.Contains(" ") && (!path.StartsWith("\"") && !path.EndsWith("\"")) ? 
            "\"" + path + "\"" : path : 
            string.Empty;
}


private string ConfirmContentsinDataFolder(string InputFolder, TMP_InputField inputfield)
{           
    string[] allfiles = Directory.GetFiles(InputFolder);

            // Confrim if whole image, mask, patch folder and bbox file exists
            if (!File.Exists(Path.Combine(InputFolder, "img.png")) || 
            !File.Exists(Path.Combine(InputFolder, "mask.tif")) || 
            !File.Exists(Path.Combine(InputFolder, "bbox.txt")))
            {
                inputfield.text = "";
                inputfield.placeholder.GetComponent<TextMeshProUGUI>().text = 
                "Please try again, ensure img.png, mask, bbox.txt exist";
                return null;
            }

            if (!Directory.Exists(Path.Combine(InputFolder, "patches")) || Directory.GetFiles( Path.Combine(InputFolder, "patches")).Length == 0)
            {
                inputfield.text = "";
                inputfield.placeholder.GetComponent<TextMeshProUGUI>().text = "Please ensure folder with patches exist or is not empty";
return null;
            }

            // Ensure that patches are of png format

            string[] patchFiles = Directory.GetFiles(Path.Combine(InputFolder, "patches"));
            bool hasNonPngPatch = patchFiles.Any(file => !file.EndsWith(".png"));

            if (hasNonPngPatch)
            {
                inputfield.text = "";
                inputfield.placeholder.GetComponent<TextMeshProUGUI>().text = "Please ensure patches are of png format";
return null;
            }

            else

            {return InputFolder;}



}

private string ConfirmExistence (TMP_InputField inputfield)

{

        // Windows appears to handle special characters in path names better than linux
        
        string path = inputfield.text;

        // Appears to be the only method that works to sanitize path names
        string full_path = Path.GetFullPath(path);


        // Confirm if path does not exists, if not clear and prompt user to enter again
        
        if (Directory.Exists(full_path) || File.Exists(full_path))

        { return full_path;}



        else
        {   
            
            inputfield.text = "";
            inputfield.placeholder.GetComponent<TextMeshProUGUI>().text = "Please try again, path does not exist";

        return null;   
        }


}

    public List<string> GetInputFields()
    {
        List<string> inputFields = new List<string>();

        foreach (Transform input in inputs)
            {
                TMP_InputField input_field = input.GetComponentInChildren<TMP_InputField>();
                string full_path = ConfirmExistence(input_field);

                if (input.gameObject.name == "ImagePath")
                {string inputpath = ConfirmContentsinDataFolder(full_path, input_field);
                inputFields.Add(inputpath);
                }
                else
                {inputFields.Add(full_path);}
            } 
        
        return inputFields;


        


    }

}
