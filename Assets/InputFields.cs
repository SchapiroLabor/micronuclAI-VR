using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



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
        // gameObject.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing = GetComponent<RectTransform>().sizeDelta.y * 0.01f;

        // Position canvas at near clip plane
        //transform.localPosition = Vector3.zero;


        // Initialize input widget for image path
        Transform inputWidget = InitializeInputWidget4Imgs(MenuPanel);

        // Initialize input widget for python executable path
        InitializeInputWidget4PythonExec(inputWidget);

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
        inputWidget.GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 0.5f);



        // Set scale to 1
        inputWidget.localScale = new Vector3(1, 1, 1);

        // Set the text of the placeholder
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().text = "Enter path to Input folder";

        // Enable overflow
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = true;

        // Set color of text to grey
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;



                // Set all children of gametitle to size equal to parent
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
        }

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
        inputWidget.GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 0.5f);


                // Set scale to 1
        inputWidget.localScale = new Vector3(1, 1, 1);


        // Set placeholder text
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().text  = "Path to Python Executable";

        // Enable overflow
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = true;

        // Set color of text to grey
        inputWidget.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;


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
        }


        return inputWidget;
    }

}
