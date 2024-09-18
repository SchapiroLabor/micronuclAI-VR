using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.IO;

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject GameManager;

    void Start()
    {   
        // Load GameManaging scriptable object
        if (GameManager == null)
        {
            Debug.Log("Loading GM");
            // Load from path
            GameManager = Resources.Load<GameObject>(Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/SceneManager.prefab")));
        }


        Transform canvas = InitializeMenuPanel();   
    }

    Transform InitializeMenuPanel()
    {
        // Load the canvas
        gameObject.name = "MenuPanel";

        // Canvas anchors to left bottom
        GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Set Pivot to centre
        GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        // Compute FOV from anglar field of view and distance to near clip plane
        /*float fov_vertical = (float) (2 * Math.Tan(camera.fieldOfView * 0.5f) * camera.nearClipPlane);

        // Fov horizontal
        float fov_horizontal = (float) (fov_vertical * camera.aspect); */

        // Make canvas same size as screen
        float height = Screen.height;
        float width = Screen.width;
        
        GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);


        // Set at camera poistion and rotation and at far clip plane. Near clip plane does not work in 2D
        // Get render camera from canvas
        Camera camera = GetComponent<Canvas>().worldCamera;
        // Set plane distance at far clip plane
        GetComponent<Canvas>().planeDistance = camera.farClipPlane;

        // Initialize input widget for image path
        Transform inputWidget = transform.GetComponentInChildren<InputFields>().Initialize(transform);

        // Initialize next button
        Transform nextButton = InitializeNextButton(transform);

        // Initialize lab logo
        InitializeLabLogo(transform);

        // Initialize game title
        InitializeGameTitle(transform);

        return transform;
    }


    Transform InitializeNextButton(Transform Canvas)

    {  
        // Get NextButton from MenuPanel
        Transform nextButton = transform.GetChild(0);

    
        nextButton.name = "NextButton";

        // Set anchors to bottom left
        nextButton.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        nextButton.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Set Pivot to centre
        nextButton.GetComponent<RectTransform>().pivot = new Vector2(1.0f, 0.5f);

        // Set scale equal to parent
        nextButton.localScale = new Vector3(1, 1, 1);

        // Set button size to 20 % of canvas width and 20 % of canvas height
        //nextButton.GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x * 0.2f, GetComponent<RectTransform>().sizeDelta.y * 0.2f);

        // Set button position to 75% + (5%/2) of canvas width and 50% + (5%/2) of canvas height

        Vector2 size = Canvas.GetComponent<RectTransform>().sizeDelta;
        nextButton.position = new Vector2(size.x, size.y/2);

        // Set button text size to 1% of button size
        nextButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = Mathf.FloorToInt(GetComponent<RectTransform>().sizeDelta.x * 0.03f);

        // Set button size to fit the text
        nextButton.GetComponent<RectTransform>().sizeDelta = new Vector2(nextButton.GetComponentInChildren<TextMeshProUGUI>().preferredWidth, nextButton.GetComponentInChildren<TextMeshProUGUI>().preferredHeight);
        

        // Add button click event
        nextButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { OnConfirmButtonClick(); });



        return nextButton;
    }

    void OnConfirmButtonClick()
    {   
        if (GameManager == null)
        {
            Debug.LogError("GameManager is null");

            if (GameManager.GetComponent<GameManaging>() == null)
            {
                Debug.LogError("GameManager.GameManaging is null");
            }
        }
        // Assign the input fields to the GameManager
        // Get text provided in input field



        List<string> output = transform.Find("InputFields").GetComponent<InputFields>().GetInputFields();

        bool valid = output.Exists(q => q != null);

        if (valid)
        {
        
        // Assign the input fields to the GameManager
        // Get text provided in List<string> output
        GameManager.GetComponent<GameManaging>().InputFolder = output[0];
        GameManager.GetComponent<GameManaging>().PythonExecutable = null;

        // Load the next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("CI-Annotator");
        }

        else
        {
            Debug.Log($"Input fields are empty: {output}");
        }



    }


    void InitializeLabLogo(Transform Canvas)
    {
        // Load the lab logo
        Transform labLogo = Canvas.GetChild(1);
        labLogo.name = "LabLogo";

        
        // Set anchors to top left
        labLogo.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        labLogo.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        //Set Pivot to centre bottom
        labLogo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);

        // Set size to 50% of canvas for width and 30% for height
        labLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(Canvas.GetComponent<RectTransform>().sizeDelta.x * 0.6f, Canvas.GetComponent<RectTransform>().sizeDelta.y * 0.3f);

        // Set position to bottom centre of canvas
        labLogo.position = new Vector3(Canvas.GetComponent<RectTransform>().sizeDelta.x * 0.5f, 0, 0);

        //
        Debug.Log($"Size: {Canvas.GetComponent<RectTransform>().sizeDelta.x}");

        // Set scale to 1
        labLogo.localScale = new Vector3(1, 1, 1); 

        // Set image
        labLogo.GetComponent<RawImage>().texture = Resources.Load<Texture>(Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("LabImage.jpg")));

    }


    void InitializeGameTitle(Transform Canvas)
    {


        // Load a prefab text object from Assets
        Transform gameTitle = Canvas.GetChild(3);

        // Set anchors to top centre
        gameTitle.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        gameTitle.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

                Vector2 size = Canvas.GetComponent<RectTransform>().sizeDelta;

        // Set Pivot to centre
        gameTitle.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1.0f);

                // Set fonsize
        gameTitle.GetComponent<TextMeshProUGUI>().fontSize = size.x*0.025f;

           // Set button size to fit the text
        gameTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(gameTitle.GetComponent<TextMeshProUGUI>().preferredWidth,
        gameTitle.GetComponent<TextMeshProUGUI>().preferredHeight);



        float depth = Canvas.transform.position.z;
        // Set position to top centre
        gameTitle.transform.position =  new Vector3(size.x/2, size.y, depth);


        // Make text bold
        gameTitle.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;



        // Set scale to 1
        gameTitle.localScale = new Vector3(1, 1, 1); 

        // Set text
        gameTitle.GetComponent<TextMeshProUGUI>().text = "MicroNuclAI virtual reality annotation tool";

        // Set text colour
        gameTitle.GetComponent<TextMeshProUGUI>().color = Color.black;



    }





}

