using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
// Import functions from another script
using static InteractableImageStack; // With a static directive, you can access the members of the class by using the class name itself

public class Trash : MonoBehaviour
{

    private GameObject trashPrefab;
    private ClickNextImage CurrentImage_script;





public void Initialize ()

{       
        if (transform.parent.Find("Image") != null)
        {
        
        CurrentImage_script = transform.parent.GetComponentInChildren<ClickNextImage>();

        trashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/CIAnnotator/trash_text.prefab");

        // Set the anchors and pivots of the Canvas
        SetupAnchorsAndPivots(GetComponent<RectTransform>());

        // Set anchors to left bottom
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        // Set pivot to center right
        rectTransform.pivot = new Vector2(1f, 0.5f);

        Vector3 image_position = CurrentImage_script.GetComponent<RectTransform>().position;
        float width = CurrentImage_script.GetComponent<RectTransform>().rect.width / 2;
        float x_shift = width;
        // Have to use local position becuase world positions provides unexpected results
        Vector3 position = new Vector3(image_position.x - x_shift, image_position.y, image_position.z);
        transform.position = position;

        Vector2 fov = ResizeImgtobewithin60percentofFOV(image_position.z);

        // Set Grid Layour group spacing to 10% of image width
        GridLayoutGroup gridLayoutGroup = GetComponent<GridLayoutGroup>();
        gridLayoutGroup.spacing =  new Vector2(0.01f, 0.01f) * fov;
        gridLayoutGroup.cellSize = fov/4;

        // Above only works if content size fitters exists

        createBuckets();

        }

}

private Vector2 ResizeImgtobewithin60percentofFOV(float WD)
{

    // Get the FOV at the panel height
    List<float> outputs = GetFOVatWD(WD);
    
    float newWidth = outputs[0]*0.6f; // Height
    float newHeight = outputs[1]*0.6f; // Width

    // Reduce image size whilst keeping the image aspect ratio
    float aspect_ratio =1;

    // Adjust the dimensions to maintain the aspect ratio
    if (newWidth > newHeight * aspect_ratio)
    {
        newWidth = newHeight * aspect_ratio; // Aspect ratio is 1, so newWidth = newHeight
    }
    else
    {
        newHeight = newWidth / aspect_ratio; // Aspect ratio is 1, so newHeight = newWidth
    }

    Debug.Log("New Width: " + newWidth + " New Height: " + newHeight);
    // Set width and height of the Canvas

    return new UnityEngine.Vector2(newWidth, newHeight);

}

private void re_init_image(GameObject ImageCurrent)

{   

    if (ImageCurrent == null || ImageCurrent.activeSelf == false)
    {
    ClickNextImage CurrentImage_script = ImageCurrent.GetComponent<ClickNextImage>();
    ImageCurrent.SetActive(true);
    ImageCurrent.GetComponent<RawImage>().texture = CurrentImage_script.images[CurrentImage_script.current_img_indx];
    ImageCurrent.GetComponent<RectTransform>().position = CurrentImage_script.start_position;
    ImageCurrent.GetComponent<RectTransform>().rotation = CurrentImage_script.start_rotation;
    CurrentImage_script.PositionResizeText(ImageCurrent.GetComponent<RectTransform>(), CurrentImage_script.current_img_indx, CurrentImage_script.N_image);
    closedisplaysecondimg();

    }

    else
    {
        Debug.Log(string.Format("This object appears to be missing {0}", ImageCurrent.name));

    }
}

// This is executed once the trash object collider is triggered
    public void dispose()

    {


        // Get current image index
        if (CurrentImage_script.current_img_indx < (CurrentImage_script.N_image - 1))
        {
            CurrentImage_script.current_img_indx += 1;
        }
        else 
        {
            CurrentImage_script.current_img_indx = 0; 
        }

        GameObject ImageCurrent = CurrentImage_script.gameObject;

        if (ImageCurrent != null)
            {
            Debug.Log(string.Format("This object is not deleted with current ID {0}", CurrentImage_script.current_img_indx));
            ImageCurrent.SetActive(false);
            re_init_image(ImageCurrent);
            }

            else
            {
                Debug.Log(string.Format("This object appears to be missing {0}", ImageCurrent.name));

            }

    }

    public void ReverseDispose()
    {

                // Get current image index
        if (CurrentImage_script.current_img_indx < (CurrentImage_script.N_image - 1) && CurrentImage_script.current_img_indx > 0)
        {
            CurrentImage_script.current_img_indx -= 1;
        }
        else 
        {
            CurrentImage_script.current_img_indx = 0; 
        }

        GameObject ImageCurrent = CurrentImage_script.gameObject;
        if (ImageCurrent != null)
            {
            Debug.Log(string.Format("This object is not deleted with current ID {0}", CurrentImage_script.current_img_indx));
            ImageCurrent.SetActive(false);
            re_init_image(ImageCurrent);
            }

            else
            {
                Debug.Log(string.Format("This object appears to be missing {0}", ImageCurrent.name));

            }

    }




    public void closedisplaysecondimg()

    {   
        GameObject rawImagesubsequent = transform.parent.Find("SubsequentImage").gameObject;
        
        rawImagesubsequent.SetActive(false);
   
    }

    private GameObject createTrash(int N, RawImage rawImagecurrent)
    {   
    
        GameObject trashInstance = Instantiate(trashPrefab, transform);
        trashInstance.transform.position = new Vector3(trashInstance.transform.position.x, trashInstance.transform.position.y, transform.position.z);
        trashInstance.transform.localScale = Vector3.one;
        trashInstance.name = $"{N} Micronuclei";
        TMP_Text tmpText = trashInstance.GetComponentInChildren<TMP_Text>();
        tmpText.text = $"{N}";
        return trashInstance;
    }



    public void createBuckets()
    {   
        RawImage rawImagecurrent = CurrentImage_script.GetComponent<RawImage>();

        if (rawImagecurrent != null){
            var spacing = (rawImagecurrent.GetComponent<RectTransform>().rect.width)/2;
            
        List<GameObject> trashList = new List<GameObject>();

        if (trashPrefab == null)
        {
            trashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/CIAnnotator/trash_text.prefab");
        }

        for (int n = 0; n <= 3; n++){

            GameObject trashinstance = createTrash(n, rawImagecurrent);

            trashList.Add(trashinstance);
        }


        // Create Title
        GameObject title = new GameObject("Title");
        title.transform.parent = transform.parent;
        Vector3 image_position = rawImagecurrent.GetComponent<RectTransform>().position;
        title.transform.position = new Vector3(image_position.x - (spacing*2), image_position.y + spacing, image_position.z);
        title.AddComponent<TextMeshPro>();
        TextMeshPro titleText = title.GetComponent<TextMeshPro>();
        titleText.text = "Micronuclei count";
        titleText.fontSize = 1;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);

        }



    }


public void CreateBucket()
{
Debug.Log("Bucket created");
}


}
