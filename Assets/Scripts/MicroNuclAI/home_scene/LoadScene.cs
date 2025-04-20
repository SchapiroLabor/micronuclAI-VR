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


namespace HomeScene
{

    public class LoadScene : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private Transform labLogo;
        [SerializeField] private Transform SoftwareTitle;
        void Start()
        {
            // Figure out how to set up VS code properly to use the Unity API: executable etc

            InitializeMenuPanel();
        }

        Transform InitializeMenuPanel()
        {

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

            // Initialize lab logo
            InitializeLabLogo();

            // Initialize game title
            InitializeGameTitle();

            return transform;
        }

        void InitializeLabLogo()
        {


            // Set image
            labLogo.GetComponent<RawImage>().texture = Resources.Load<Texture>(Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("LabImage.jpg")));

        }


        void InitializeGameTitle()
        {


            // Make text bold
            SoftwareTitle.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

            // Set text
            SoftwareTitle.GetComponent<TextMeshProUGUI>().text = "MicroNuclAI virtual reality annotation tool";

            // Set text colour
            SoftwareTitle.GetComponent<TextMeshProUGUI>().color = Color.black;



        }





    }
}
