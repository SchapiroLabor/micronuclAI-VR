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


public class InputFields : MonoBehaviour
{



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

        // Initialize input widget for python executable path
        InitializeInputWidget4PythonExec(MenuPanel);

        return transform;
    }


    Transform InitializeInputWidget4Imgs(Transform Canvas)
    {   
        // Load the input widget
        Transform inputWidget = transform.GetChild(0);
        inputWidget.name = "ImagePath";

        // Set anchors to bottom left
        inputWidget.GetComponent<RectTransform>().anchorMin = new UnityEngine.Vector2(0, 0);
       inputWidget.GetComponent<RectTransform>().anchorMax = new UnityEngine.Vector2(0, 0);

        // Set Pivot to centre
        inputWidget.GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 0.0f);

        UnityEngine.Vector2 size = Canvas.GetComponent<RectTransform>().sizeDelta;
        // Set position to the centre
        inputWidget.transform.position = new UnityEngine.Vector2(size.x*0.5f, size.y*0.55f);
        // Set scale to 1
        inputWidget.localScale = new UnityEngine.Vector3(1, 1, 1);

        // Set the text of the placeholder
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().text = "Enter path to Input folder";

        // Enable overflow
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = true;

        // Set color of text to grey
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;

        inputWidget = SetSizeofInputBar(inputWidget, Canvas);



        /*        // Set all children of gametitle to size equal to parent
        foreach (Transform child in inputWidget)
        {
            child.GetComponent<RectTransform>().sizeDelta = inputWidget.GetComponent<RectTransform>().sizeDelta;

                        // anchor to bottom left
            child.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            child.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

            // Set pivot to bootom left
            child.GetComponent<RectTransform>().pivot = new Vector2(0, 0);


            // set all children of child to size equal to parent
            foreach (Transform grandChild in child)
            {
                grandChild.GetComponent<RectTransform>().sizeDelta = child.GetComponent<RectTransform>().sizeDelta;

                // anchor to bottom left
                grandChild.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                grandChild.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

                // Set pivot to bootom left
                grandChild.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

                // Set local position to 0
                grandChild.localPosition = new Vector3(0, 0, 0);
            }
        } */

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


        /*
                        // Set all children of gametitle to size equal to parent
        foreach (Transform child in inputWidget)
        {
            child.GetComponent<RectTransform>().sizeDelta = inputWidget.GetComponent<RectTransform>().sizeDelta;

                        // anchor to bottom left
            child.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            child.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

            // Set pivot to bootom left
            child.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

            // Set local position to 0
            child.localPosition = new Vector3(0, 0, 0);

            // set all children of child to size equal to parent
            foreach (Transform grandChild in child)
            {
                grandChild.GetComponent<RectTransform>().sizeDelta = child.GetComponent<RectTransform>().sizeDelta;

                // anchor to bottom left
                grandChild.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                grandChild.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

                // Set pivot to bootom left
                grandChild.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

                // Set local position to 0
                grandChild.localPosition = new Vector3(0, 0, 0);
            }
        } */


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

    public List<string> GetInputFields()
    {
        

        // Windows appears to handle special characters in path names better than linux
        
        string path1 = transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text;
        string path2 = transform.GetChild(1).GetComponentInChildren<TMP_InputField>().text;

        // Appears to be the only method that works to sanitize path names
        string InputFolder = Path.GetFullPath(path1);
        string PythonExecutable = Path.GetFullPath(path2);


        // Confirm if path does not exists, if not clear and prompt user to enter again
        
        if (!Directory.Exists(InputFolder) || !File.Exists(PythonExecutable) || InputFolder == "" || PythonExecutable == "") 
        {   
            
            transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text = "";
            transform.GetChild(0).GetComponentInChildren<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = "Please try again, path does not exist";

                            // Confirm if path exists, if not clear and prompt user to enter again
        transform.GetChild(1).GetComponentInChildren<TMP_InputField>().text = "";
        transform.GetChild(1).GetComponentInChildren<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = "Please try again, path does not exist";

        return null;   
        }


        else
        {
            string[] allfiles = Directory.GetFiles(InputFolder);

            // Confrim if whole image, mask, patch folder and bbox file exists
            if (!File.Exists(Path.Combine(InputFolder, "img.png")) || 
            !File.Exists(Path.Combine(InputFolder, "mask.tif")) || 
            !File.Exists(Path.Combine(InputFolder, "bbox.csv")))
            {
                transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text = "";
                transform.GetChild(0).GetComponentInChildren<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = 
                "Please try again, ensure img.png, mask, bbox.csv exist";

                return null;
            }

            if (!Directory.Exists(Path.Combine(InputFolder, "patches")) || Directory.GetFiles( Path.Combine(InputFolder, "patches")).Length == 0)
            {
                transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text = "";
                transform.GetChild(0).GetComponentInChildren<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = "Please ensure folder with patches exist or is not empty";

                return null;
            }

            // Ensure that patches are of png format

            string[] patchFiles = Directory.GetFiles(Path.Combine(InputFolder, "patches"));
            bool hasNonPngPatch = patchFiles.Any(file => !file.EndsWith(".png"));

            if (hasNonPngPatch)
            {
                transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text = "";
                transform.GetChild(0).GetComponentInChildren<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = "Please ensure patches are of png format";

                return null;
            }

            else
            {
                List<string> inputFields = new List<string>();
                inputFields.Add(InputFolder);
                inputFields.Add(PythonExecutable);

                // Debug log contents in list, iterate
                foreach (string inputField in inputFields)
                {
                    Debug.Log(inputField);
                }

                return inputFields;
            }


        }

    }

}
