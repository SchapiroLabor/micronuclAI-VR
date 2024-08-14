using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    // Insatiate an exit button that is 2*image width in distance away from the image
    public void Initialize(Transform ImagePatch)
    {
        // Set the anchors and pivots of the Canvas
        SetupAnchorsAndPivots(GetComponent<RectTransform>());

        // Set anchors to right bottom
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 0);
        rectTransform.anchorMax = new Vector2(1, 0);

        // Set pivot to center right
        rectTransform.pivot = new Vector2(1f, 0.5f);

        Vector3 image_position = ImagePatch.GetComponent<RectTransform>().position;
        float width = ImagePatch.GetComponent<RectTransform>().rect.width / 2;
        float x_shift = width;
        // Have to use local position becuase world positions provides unexpected results
        Vector3 position = new Vector3(image_position.x + x_shift, image_position.y, image_position.z);
        transform.position = position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
