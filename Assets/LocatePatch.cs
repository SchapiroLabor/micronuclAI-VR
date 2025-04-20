using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocatePatch : MonoBehaviour
{
    // Start is called before the first frame update
    /*     void Start()
        {
            // Set up Exit Button
            InstantiateCanvasUI(CurrentImage, WholeImage, Panel);
        }

        private void InstantiateCanvasUI(Transform rawImageTransform, Transform WholeImage, Transform trash)
        {
            // Create a new Canvas UI GameObject
            //GameObject CanvasUI = CreateGameObject(transform, Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/Canvas UI.prefab")), transform);

            // MenuPanel
            CanvasUI.name = "Canvas UI";

            PositionandResizeCanvasUI(CanvasUI, rawImageTransform);

            // Setup all the required buttons
            CanvasUI.GetComponent<SetupButtons>().Initialize(rawImageTransform, CanvasUI.transform.position, CanvasUI.transform.rotation, WholeImage, trash);

        }

        private void PositionandResizeCanvasUI(GameObject CanvasUI, Transform rawImageTransform)
        {
            // Set the anchors and pivots of the Canvas UI
            SetupAnchorsAndPivots(CanvasUI.GetComponent<RectTransform>());

            // Set to bottom left corner the anchors
            CanvasUI.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            CanvasUI.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

            // Face the Canvas UI to the player
            CanvasUI.transform.rotation = Quaternion.Euler(Vector3.zero);

            // Change pivot to top left corner of the Canvas UI, so no overlap with the RawImage
            CanvasUI.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

            // Set location of the Canvas UI to the top right corner of the RawImage
            CanvasUI.transform.position = new Vector3((rawImageTransform.position.x + rawImageTransform.GetComponent<RectTransform>().sizeDelta.x / 2) * 1.1f,
            rawImageTransform.position.y + rawImageTransform.GetComponent<RectTransform>().sizeDelta.y / 2, rawImageTransform.position.z);

            // Set scale of the Canvas
            CanvasUI.transform.localScale = Vector3.one;
        } */
    // Update is called once per frame
    void Update()
    {

    }
}
