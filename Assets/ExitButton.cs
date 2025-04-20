using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Debug = UnityEngine.Debug;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class ExitButtonCIN : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    void SetupSeperateButton(Transform ImagePatch, Transform parent)
    {

        // Set up exit button
        setupExitButton(ImagePatch, parent);

    }

    private void setupExitButton(Transform ImagePatch, Transform Parent)

    {

        GameObject ExitButton = Resources.Load<GameObject>(Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/Button.prefab")));

        // Instantiate
        ExitButton = Instantiate(ExitButton, Parent);

        // Set name
        ExitButton.name = "ExitButton";

        // Set rotation of the button
        ExitButton.transform.rotation = Parent.rotation;
        // Set position of the button
        ExitButton.transform.position = Parent.position;



        StandardiseExitButton(ExitButton, ImagePatch, Parent.GetComponent<RectTransform>().anchorMin,
        Parent.GetComponent<RectTransform>().anchorMax, Parent.GetComponent<RectTransform>().pivot);

        // Set Canvas to same size as button, for some reason when I set it same to Exit Button, localpositioning of Exit Button is not working
        Parent.GetComponent<RectTransform>().sizeDelta = Vector2.one;

        // Set text of the button
        ExitButton.GetComponentInChildren<TextMeshProUGUI>().text = "EXIT Game";

        /*         // Add a listener to the button
                ExitButton.GetComponent<Button>().onClick.AddListener(() => Exit()); */

    }

    private void StandardiseExitButton(GameObject Button, Transform ImagePatch, Vector2 AnchorMin, Vector2 AnchorMax, Vector2 Pivot)
    {
        // Set the font size of the Button same to width of image
        Button.GetComponentInChildren<TextMeshProUGUI>().fontSize = ImagePatch.GetComponent<RectTransform>().sizeDelta.x * 0.1f; ;

        // Set the size of the Canvas UI to 1/3 of width of image with aspect ratio of 3:1
        Button.GetComponent<RectTransform>().sizeDelta = ResizeButton(ImagePatch);

        // Set alginment of the text in the button
        Button.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Give button black background and white text
        Button.GetComponent<Image>().color = Color.black;
        Button.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        // Set text margins to 0
        Button.GetComponentInChildren<TextMeshProUGUI>().margin = new Vector4(0, 0, 0, 0);

        // Setup anchors to bottom left corner
        Button.GetComponent<RectTransform>().anchorMin = AnchorMin;
        Button.GetComponent<RectTransform>().anchorMax = AnchorMax;

        // Set pivot to top right left corner
        Button.GetComponent<RectTransform>().pivot = Pivot;

        // Scale all to one
        Button.transform.localScale = Vector3.one;

        Button.transform.localPosition = new Vector3(0, 0, 0);

    }

    private Vector2 ResizeButton(Transform ImagePatch)

    {

        // Get the width and height of the RawImage
        float width = ImagePatch.GetComponent<RectTransform>().rect.width;
        float height = ImagePatch.GetComponent<RectTransform>().rect.height;

        float scaled_width = width * ImagePatch.GetComponent<RectTransform>().localScale.x;
        float scaled_height = height * ImagePatch.GetComponent<RectTransform>().localScale.y;

        // Set the size of the Canvas UI to 1/3 of width of image with aspect ratio of 3:1
        return new UnityEngine.Vector2(scaled_width / 3, scaled_width / 6);

    }

    /*     void OnApplicationQuit()
        {
            // Set Ready2Exit to true
            Ready2Exit = true;
        }

        private void Exit()
        {
            // Get the counts of micro nuclei
            MicronucleiCounts micronucleiCounts = CollectMicroNucleiCounts();

            isReady = false;
            // Thread pool write to Python
            HelperFunctions.Write2CSV(inputfolder, micronucleiCounts);

            OnApplicationQuit();
            //ThreadPooling(new Action<string, string, string, string, string> (Write2Python), isReady,
            //new Action(OnApplicationQuit), PythonScript, python_exe, Application.streamingAssetsPath, inputfolder, micronucleiCounts);

        } */
}
