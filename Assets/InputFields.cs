using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Vectot2 = UnityEngine.Vector2;



public class InputFields : MonoBehaviour
{



    Transform Initialize(Transform MenuPanel)
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

        // Add content size fitter
        gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        // Organise children in vertical layout group
        gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();

        // Spacing set to 1 % of canvas height
        gameObject.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing = GetComponent<RectTransform>().sizeDelta.y * 0.01f;

        // Position canvas at near clip plane
        transform.localPosition = Vector3.zero;

        // Face Canvas to player
        transform.LookAt(Camera.main.transform);


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

        // Set size to 50% of canvas for width and 10% for height
        inputWidget.GetComponent<RectTransform>().sizeDelta = new UnityEngine.Vector2(Canvas.GetComponent<RectTransform>().sizeDelta.x * 0.5f, Canvas.GetComponent<RectTransform>().sizeDelta.y * 0.1f);

        // Face Canvas to player
        inputWidget.LookAt(Camera.main.transform);

        // Set scale to 1
        inputWidget.localScale = new Vector3(1, 1, 1);

        // Set placeholder text
        inputWidget.GetComponentInChildren<UnityEngine.UI.Text>().text = "Enter path to Input folder";

        // Set input field text to empty
        inputWidget.GetComponentInChildren<UnityEngine.UI.InputField>().text = "";

        return inputWidget;
    }


    Transform InitializeInputWidget4PythonExec(Transform inputWidget=null)
    {   
        if (inputWidget == null)
        {
            // Load the input widget
             inputWidget = transform.GetChild(0);
        }


        // Make a copy of the input widget
        inputWidget = Instantiate(inputWidget, transform);

        inputWidget.name = "PythonExecPath";

        // Set placeholder text
        inputWidget.GetComponentInChildren<UnityEngine.UI.Text>().text = "Path to Python Executable";

        // Set input field text to empty
        inputWidget.GetComponentInChildren<UnityEngine.UI.InputField>().text = "";

        return inputWidget;
    }

}
