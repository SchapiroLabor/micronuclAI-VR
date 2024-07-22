using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class ClickNextImage : MonoBehaviour
{
    public InteractableImageStack Canvas_script;
    public GameObject rawImagesubsequentGO;
    private int subsequent_img;

    private void Start()
    {
        CreateGOForSubsequentImage();

    }

    private void CreateGOForSubsequentImage()
    {
        // Create subsequent image only when there are more than one images
        if (Canvas_script.images.Count > 1)
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

            rawImagesubsequentGO.transform.SetParent(transform.parent);
            rawImagesubsequentGO.SetActive(false);
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

public void Create_button_next2image()
{
    Canvas_script.GetComponentInChildren<whole_image>().current_cell_bbox(subsequent_img);
}

    private void Update()
    {

    }
}


