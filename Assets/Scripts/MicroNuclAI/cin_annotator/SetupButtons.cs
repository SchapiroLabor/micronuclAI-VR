using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using NonGOSripts;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using System.IO;
using Vector4 = UnityEngine.Vector4;

namespace CinAnnotator
{
    public class SetupButtons : MonoBehaviour

    {
        float fontSize;
        private UnityEngine.Vector2 buttonSize;
        private UnityEngine.Quaternion buttonRoation;
        public GameObject LocatePatch;

        private float distanceFromImageStack; // Distance from the image to place the buttons
        /* public InteractableImageStack Canvas_script; */


        void Awake()
        {
            // Add content size fitter componet
            gameObject.AddComponent<ContentSizeFitter>();
            gameObject.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            gameObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Add vertical layout group component
            gameObject.AddComponent<VerticalLayoutGroup>();
            gameObject.GetComponent<VerticalLayoutGroup>().childControlWidth = false;
            gameObject.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
            gameObject.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
            gameObject.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
            gameObject.GetComponent<VerticalLayoutGroup>().spacing = fontSize * 0.2f;

            transform.localScale = Vector3.one;

            transform.localPosition = Vector3.zero;
        }

        // Start is called before the first frame update
        public void Initialize(Transform ImagePatch, UnityEngine.Quaternion rotation, Transform Trash)
        {


            // TODO: Call this function depending on the objects required to be setup
            fontSize = ImagePatch.GetComponent<RawImage>().uvRect.width * 0.3f;
            buttonRoation = rotation;

            transform.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            transform.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

            distanceFromImageStack = ImagePatch.GetComponent<RawImage>().uvRect.width * 1.75f;
            transform.position = ImagePatch.position + new Vector3(distanceFromImageStack, 0, 0);


            buttonSize = ResizeButton(ImagePatch);

            setupLocatePatchButton();
            setupReverseButton(Trash);
            setupAddBinButton(Trash);
            setupExitButton();
        }



        private Vector2 ResizeButton(Transform ImagePatch)

        {

            // Get the width and height of the RawImage
            float width = ImagePatch.GetComponent<RectTransform>().rect.width;

            float scaled_width = width * ImagePatch.GetComponent<RectTransform>().localScale.x;

            // Set the size of the Canvas UI to 1/3 of width of image with aspect ratio of 3:1
            return new UnityEngine.Vector2(scaled_width / 3, scaled_width / 6);

        }

        private void setupLocatePatchButton()

        {

            // Set name
            LocatePatch.name = "Locate Patch";

            standardiseButton(LocatePatch);

            // Set text of the button
            LocatePatch.GetComponentInChildren<TextMeshProUGUI>().text = "Locate";

            // Add a listener to the button
            LocatePatch.GetComponent<Button>().onClick.AddListener(() => transform.parent.GetComponentInChildren<WholeImage>().DisplayCell());



        }

        [SerializeField] private GameObject ReverseButton;
        private void setupReverseButton(Transform Trash)

        {
            standardiseButton(ReverseButton);

            // Set text of the button
            ReverseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Undo";

            // Add a listener to the button
            ReverseButton.GetComponent<Button>().onClick.AddListener(() => Trash.GetComponentInChildren<Trash>().ReverseDispose());

        }

        [SerializeField] private GameObject AddBin;
        private void setupAddBinButton(Transform Trash)

        {

            standardiseButton(AddBin);

            // Set text of the button
            AddBin.GetComponentInChildren<TextMeshProUGUI>().text = "Add Bin";

            // Add a listener to the button
            AddBin.GetComponent<Button>().onClick.AddListener(() => Trash.GetComponentInChildren<Trash>().InitializeTrash());

        }

        [SerializeField] private GameObject _Exitbutton;
        private void setupExitButton()
        {
            standardiseButton(_Exitbutton);

            // Set text of the button
            _Exitbutton.GetComponentInChildren<TextMeshProUGUI>().text = "Quit";

            // Add a listener to the button
            _Exitbutton.GetComponent<Button>().onClick.AddListener(() => Exit());

            _Exitbutton.transform.localPosition = Vector3.zero;
        }

        void Exit()
        {

            // Quit the application
            Application.Quit();

            // If running in the Unity Editor, stop play mode
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif



        }



        private void standardiseButton(GameObject Button)
        {
            // Set the font size of the Button same to width of image
            Button.GetComponentInChildren<TextMeshProUGUI>().fontSize = fontSize;

            // Set location
            Button.transform.rotation = buttonRoation;

            // Set the size of the Canvas UI to 1/3 of width of image with aspect ratio of 3:1
            Button.GetComponent<RectTransform>().sizeDelta = buttonSize;

            // Set alginment of the text in the button
            Button.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Give button black background and white text
            Button.GetComponent<Image>().color = Color.black;
            Button.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

            // Set text margins to 0
            Button.GetComponentInChildren<TextMeshProUGUI>().margin = new Vector4(0, 0, 0, 0);

            HelperFunctions.SetupAnchorsAndPivots(Button.GetComponent<RectTransform>());

            // Setup anchors to bottom left corner
            Button.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            Button.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

            // Set pivot to top right left corner
            Button.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

            // Scale all to one
            Button.transform.localScale = Vector3.one;

            Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, 0);

        }





    }
}