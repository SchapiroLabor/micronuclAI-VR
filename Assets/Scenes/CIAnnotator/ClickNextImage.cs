using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class ClickNextImage : MonoBehaviour
{
    public InteractableImageStack Canvas_script;
    public GameObject rawImagesubsequentGO;
    public GameObject CanvasUI;
    private int subsequent_img;

    private void Start()
    {
        CreateGOForSubsequentImage();

    }

    private void CreateGOForSubsequentImage()
    {

        // Create subsequent image only when there are more than one images
        if (Canvas_script.N_image > 1)
        {   
            // Step 1: Define the path to the prefab within the Resources folder
            string prefabPath = "Assets/Scenes/CIAnnotator/SubsequentImage.prefab";
            // Create a new RawImage GameObject from the prefab

            if (rawImagesubsequentGO == null)
            {
                rawImagesubsequentGO = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), transform.position, transform.rotation);
            }

            else
            {
                rawImagesubsequentGO.transform.position = transform.position;
                rawImagesubsequentGO.transform.rotation = transform.rotation;
            }
            rawImagesubsequentGO.name = "SubsequentImage";
            rawImagesubsequentGO.transform.SetParent(transform.parent);
            rawImagesubsequentGO.SetActive(false);

            Create_button_next2image(x_scale: 1f, y_scale: 0.45f);
        }

        else
        {
            Debug.Log("Only one image available");
        }
    }



    public void DisplaySecondImage()
    {
        if (this.gameObject != null && rawImagesubsequentGO != null)
        {
            subsequent_img = Canvas_script.current_img_indx;

            if (subsequent_img < (Canvas_script.images.Count - 1))
            {
                subsequent_img += 1;
            }
            else
            {
                subsequent_img = 0;
            }

            rawImagesubsequentGO.GetComponent<RawImage>().texture = Canvas_script.images[subsequent_img];
            rawImagesubsequentGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("Patch {0}/{1}", subsequent_img + 1, Canvas_script.N_image);
            rawImagesubsequentGO.SetActive(true);

        }
        else
        {
            Debug.Log("RawImageCurrent is null");
        }
    }

public void Create_button_next2image(float x_scale = 0.8f, float y_scale = 1f)
{   


    if (CanvasUI == null)
    {
            // Create a button to display the next image
    string prefab_path = "Assets/Scenes/CIAnnotator/Canvas UI.prefab";
    CanvasUI = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefab_path), transform.position, transform.rotation);

    }

    else
    {
        CanvasUI.transform.position = transform.position;
        CanvasUI.transform.rotation = transform.rotation;
    }

    CanvasUI.transform.localScale = Vector3.one;
    CanvasUI.transform.position += Get_Axes_Offsets(x_scale, y_scale);

    GameObject button = CanvasUI.transform.Find("LocatePatch").gameObject;

    if (button == null)
    {
            // Create a button to display the next image
    string prefab_path = "Assets/Scenes/CIAnnotator/Button.prefab";
    button = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefab_path), transform.position, transform.rotation);
    }

    else
    {
        button.transform.position = transform.position;
        button.transform.rotation = transform.rotation;
    }


    button.transform.position += Get_Axes_Offsets(x_scale, y_scale);
    
}


private Vector3 Get_Axes_Offsets(float x_scale, float y_scale)
{

    // Get the width and height of the RawImage
    float width = GetComponent<RectTransform>().rect.width;
    float height = GetComponent<RectTransform>().rect.height;


    float scaled_width = width* GetComponent<RectTransform>().localScale.x;
    float scaled_height = height * GetComponent<RectTransform>().localScale.y;

    // Calculate the x and y offsets
    float x_offset = scaled_width  * x_scale;
    float y_offset = scaled_height * y_scale;

    return new Vector3(x_offset, y_offset, 0);




}


    private void Update()
    {

    }
}


