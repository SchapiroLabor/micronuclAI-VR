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

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {   
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


        // Set at camera poistion and rotation and at far clip plane. Near clip plane does not work in 2D
        // Get render camera from canvas
        Camera camera = GetComponent<Canvas>().worldCamera;
        // Set plane distance at far clip plane
        GetComponent<Canvas>().planeDistance = camera.farClipPlane;

        // Initialize input widget for image path
        Transform inputWidget = transform.GetComponentInChildren<InputFields>().Initialize(transform);

        // Initialize next button
        Transform nextButton = InitializeNextButton();

        // Initialize lab logo
        InitializeLabLogo(transform);

        // Initialize game title
        InitializeGameTitle(transform);

        return transform;
    }


    Transform InitializeNextButton()

    {  
        // Get NextButton from MenuPanel
        Transform nextButton = transform.GetChild(0);

        /*
        nextButton.name = "NextButton";

        // Set anchors to bottom left
        nextButton.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        nextButton.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Set Pivot to centre
        nextButton.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        // Set scale equal to parent
        nextButton.localScale = new Vector3(1, 1, 1);

        // Set button size to 20 % of canvas width and 20 % of canvas height
        //nextButton.GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x * 0.2f, GetComponent<RectTransform>().sizeDelta.y * 0.2f);

        // Set button position to 75% + (5%/2) of canvas width and 50% + (5%/2) of canvas height
        nextButton.position = new Vector3(GetComponent<RectTransform>().sizeDelta.x * 0.75f, 
        GetComponent<RectTransform>().sizeDelta.y * 0.5f + GetComponent<RectTransform>().sizeDelta.y * 0.05f * 0.5f, 0);

        // Set button text size to 1% of button size
        nextButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = Mathf.FloorToInt(nextButton.GetComponent<RectTransform>().sizeDelta.x * 0.01f);

        // Set button text colour
        nextButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        // Set button colour
        nextButton.GetComponent<Image>().color = Color.black;


        // Set button size to fit the text
        nextButton.GetComponent<RectTransform>().sizeDelta = new Vector2(nextButton.GetComponentInChildren<TextMeshProUGUI>().preferredWidth, nextButton.GetComponentInChildren<TextMeshProUGUI>().preferredHeight);
        */

        // Add button click event
        nextButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { OnConfirmButtonClick(); });



        return nextButton;
    }

    void OnConfirmButtonClick()
    {
        // Load the next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("CIAnnotator");

    }


    void InitializeLabLogo(Transform Canvas)
    {
        // Load the lab logo
        Transform labLogo = Canvas.GetChild(1);
        labLogo.name = "LabLogo";

        /*
        // Set anchors to top left
        labLogo.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        labLogo.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Set Pivot to centre
        labLogo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);

        // Set size to 50% of canvas for width and 30% for height
        labLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(Canvas.GetComponent<RectTransform>().sizeDelta.x * 0.75f, Canvas.GetComponent<RectTransform>().sizeDelta.y * 0.3f);

        // Set position to bottom centre of canvas
        labLogo.localPosition = new Vector3(Canvas.GetComponent<RectTransform>().sizeDelta.x * 0.5f, 0, 0);

        // Set scale to 1
        labLogo.localScale = new Vector3(1, 1, 1); */

        // Set image
        labLogo.GetComponent<RawImage>().texture = Resources.Load<Texture>("Assets/Scenes/CIAnnotator/LabImage.jpg");


        

    }


    void InitializeGameTitle(Transform Canvas)
    {


        // Load a prefab text object from Assets
        Transform gameTitle = Canvas.GetChild(3);

/*
        // Set anchors to top centre
        gameTitle.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        gameTitle.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Set Pivot to centre
        gameTitle.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1.0f);

           // Set button size to fit the text
        gameTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(gameTitle.GetComponent<TextMeshProUGUI>().preferredWidth,
        gameTitle.GetComponent<TextMeshProUGUI>().preferredHeight);


        // Make text bold
        gameTitle.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

        // Set scale to 1
        gameTitle.localScale = new Vector3(1, 1, 1); */

        // Set text
        gameTitle.GetComponent<TextMeshProUGUI>().text = "MicroNuclAI virtual reality annotation tool";

        // Set text colour
        gameTitle.GetComponent<TextMeshProUGUI>().color = Color.black;



    }





}

