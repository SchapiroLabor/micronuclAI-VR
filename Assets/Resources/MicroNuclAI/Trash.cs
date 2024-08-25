using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
using Unity.VisualScripting;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
// Import functions from another script
using static InteractableImageStack;
using System.Numerics; // With a static directive, you can access the members of the class by using the class name itself
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector3;
using System.IO;
public class Trash : MonoBehaviour
{

    private GameObject trashPrefab;
    private ClickNextImage CurrentImage_script;
    private List<GameObject> trashList = new List<GameObject>();
    private string last_trash;




    void Awake()
    {
     // Set the anchors and pivots of the Canvas
    SetupAnchorsAndPivots(GetComponent<RectTransform>());

    // Set anchors to left bottom
    RectTransform rectTransform = GetComponent<RectTransform>();
    rectTransform.anchorMin = new Vector2(0, 0);
    rectTransform.anchorMax = new Vector2(0, 0);

    // Set pivot to center right
    rectTransform.pivot = new Vector2(1f, 0.5f);

    }



public void Initialize (Transform parent, Transform CurrentImage, Camera userCamera)

{       

    // Set parent
    transform.parent = parent;

    // Set CurrentImageScript
    CurrentImage_script = CurrentImage.GetComponent<ClickNextImage>();
        
    Vector3 image_position = CurrentImage.GetComponent<RectTransform>().position;
    float width = CurrentImage.GetComponent<RectTransform>().rect.width / 2;
    float x_shift = width;
    // Have to use local position becuase world positions provides unexpected results
    Vector3 position = new Vector3(image_position.x - x_shift, image_position.y, image_position.z);
    transform.position = position;

    Vector2 fov = ResizeImgtobewithin60percentofFOV(image_position.z, userCamera);

    // Set Grid Layour group spacing to 10% of image width
    GridLayoutGroup gridLayoutGroup = GetComponent<GridLayoutGroup>();
    gridLayoutGroup.spacing =  0.01f * fov;
    gridLayoutGroup.cellSize = fov/4;

    // Above only works if content size fitters exists

    createBuckets(CurrentImage);

        

}

private Vector2 ResizeImgtobewithin60percentofFOV(float WD, Camera userCamera)
{

    // Get the FOV at the panel height
    List<float> outputs = GetFOVatWD(WD, userCamera);
    
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
    ImageCurrent.GetComponent<RectTransform>().localPosition = CurrentImage_script.start_position;
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
    public void dispose(string Trash_name)
    {
        last_trash = Trash_name;
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
            ImageCurrent.SetActive(false);
            re_init_image(ImageCurrent);
        }
        else
        {
            Debug.Log(string.Format("This object appears to be missing {0}", ImageCurrent.name));
        }

        Debug.Log("Dispose method executed successfully");
    }

    public void ReverseDispose()
    {
        if (last_trash != null)

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

        Transform trash = transform.Find(last_trash);

        List<int> patches = trash.GetComponent<Tinyt>().patches;
        if ( patches.Count > 0)
        {
            patches.RemoveAt(patches.Count-1);
        }

        GameObject ImageCurrent = CurrentImage_script.gameObject;
        if (ImageCurrent != null)
            {
            ImageCurrent.SetActive(false);
            re_init_image(ImageCurrent);
            }

            else
            {
                Debug.Log(string.Format("This object appears to be missing {0}", ImageCurrent.name));

            }





        }


    }




    public void closedisplaysecondimg()

    {   
        GameObject rawImagesubsequent = transform.parent.Find("SubsequentImage").gameObject;
        
        rawImagesubsequent.SetActive(false);
   
    }

    private GameObject createTrash(int N, Transform CurrentImage)
    {   
    
        GameObject trashInstance = Instantiate(trashPrefab, transform);
        trashInstance.transform.position = new Vector3(trashInstance.transform.position.x, trashInstance.transform.position.y, transform.position.z);
        trashInstance.transform.localScale = Vector3.one;
        trashInstance.name = $"{N} Micronuclei";
        TMP_Text tmpText = trashInstance.GetComponentInChildren<TMP_Text>();
        tmpText.text = $"{N}";
        trashInstance.GetComponent<Tinyt>().Initialize(CurrentImage);
        
        return trashInstance;
    }



    public void createBuckets(Transform CurrentImage)
    {   
        RawImage rawImagecurrent = CurrentImage.GetComponent<RawImage>();

        if (rawImagecurrent != null){
            var spacing = (rawImagecurrent.GetComponent<RectTransform>().rect.width)/2;
            

        if (trashPrefab == null)
        {
            trashPrefab = Resources.Load<GameObject>(Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/trash_text.prefab")));
        }

        for (int n = 0; n <= 3; n++){

            GameObject trashinstance = createTrash(n, CurrentImage);

            trashList.Add(trashinstance);
        }


        // Create Title
        GameObject title = new GameObject("Title");
        title.transform.parent = transform.parent;




        // Pivot of current class is at 1, 0.5
        title.AddComponent<TextMeshPro>();
        TextMeshPro titleText = title.GetComponent<TextMeshPro>();
        UnityEngine.Vector3 position = transform.position;
        title.GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 0);
        UnityEngine.Vector2 size = new UnityEngine.Vector2(rawImagecurrent.GetComponent<RectTransform>().rect.width, rawImagecurrent.GetComponent<RectTransform>().rect.height);

        // Add content size fitter to the title
        title.AddComponent<ContentSizeFitter>();
        title.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        title.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        title.transform.position = new UnityEngine.Vector3(position.x - (size.x/2)*1.5f, (position.y + (size.y/2))*1.5f, position.z);
        
        titleText.text = "Micronuclei count";
        titleText.fontSize = size.x;
        titleText.alignment = TextAlignmentOptions.Center;


        }



    }


public void CreateBucket()
{
    GameObject trashinstance = createTrash(trashList.Count + 1, CurrentImage_script.transform);

    trashList.Add(trashinstance);

}


}
