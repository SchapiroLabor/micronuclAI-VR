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


public class Trash : MonoBehaviour
{

    private InteractableImageStack Canvas_script;
    public GameObject trashPrefab;
    private ClickNextImage CurrentImage_script;



public void Initialize ()

{
    
        Canvas_script = transform.parent.GetComponent<InteractableImageStack>();
        CurrentImage_script = transform.parent.GetComponent<ClickNextImage>();
        createBuckets();
}


private void re_init_image(GameObject ImageCurrent)

{   
    if (ImageCurrent == null || ImageCurrent.activeSelf == false)
    {
    ImageCurrent.SetActive(true);
    ImageCurrent.GetComponent<RawImage>().texture = Canvas_script.images[Canvas_script.current_img_indx];
    ImageCurrent.GetComponent<RectTransform>().position = Canvas_script.start_position;
    ImageCurrent.GetComponent<RectTransform>().rotation = Canvas_script.start_rotation;
    CurrentImage_script.PositionResizeText(ImageCurrent.GetComponent<RectTransform>(), Canvas_script.current_img_indx, Canvas_script.N_image);
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
        if (Canvas_script.current_img_indx < (Canvas_script.N_image - 1))
        {
            Canvas_script.current_img_indx += 1;
        }
        else 
        {
            Canvas_script.current_img_indx = 0; 
        }
        // Get child by name
        GameObject ImageCurrent = Canvas_script.transform.Find("Image").gameObject;

        if (ImageCurrent != null)
            {
            Debug.Log(string.Format("This object is not deleted with current ID {0}", Canvas_script.current_img_indx));
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
        GameObject rawImagesubsequent = CurrentImage_script.transform.parent.Find("SubsequentImage").gameObject;
        
        rawImagesubsequent.SetActive(false);
   
    }

    private GameObject createTrash(int N, RawImage rawImagecurrent)
    {   
       


        Vector3 image_position = rawImagecurrent.GetComponent<RectTransform>().position;
        float width = rawImagecurrent.GetComponent<RectTransform>().rect.width;
        float x_shift = width;
        Vector3 position = new Vector3(- x_shift - image_position.x, image_position.y, image_position.z);
        GameObject trashInstance = Instantiate(trashPrefab, position, Quaternion.identity, parent: transform);

         // Set pivot at bottom centre
        RectTransform rectTransform = trashInstance.GetComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0.5f, 0);

        trashInstance.transform.position = position;
        trashInstance.transform.localScale = Vector3.one;
        trashInstance.name = $"{N} Micronuclei";
        TMP_Text tmpText = trashInstance.GetComponentInChildren<TMP_Text>();
        tmpText.text = $"{N}";
        return trashInstance;
    }

    public void createBuckets()
    {   
        RawImage rawImagecurrent = transform.parent.Find("Image").gameObject.GetComponent<RawImage>();
        if (rawImagecurrent != null){
            var spacing = (rawImagecurrent.GetComponent<RectTransform>().rect.width/100)/2;
            Debug.Log($"Trash spacing from image is the following: {spacing}");
            
        List<GameObject> trashList = new List<GameObject>();

        if (trashPrefab == null)
        {
            trashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/CIAnnotator/trash_text.prefab");
        }

        for (int n = 0; n <= 3; n++){


            GameObject trashinstance = createTrash(n, rawImagecurrent);

            if (trashList.Count > 0){
                GameObject Previous = trashList[trashList.Count - 1];

            if (n%2 == 0){
                trashinstance.transform.position = new Vector3(Previous.transform.position.x - spacing, Previous.transform.position.y + spacing, trashinstance.transform.position.z);}
            else {
                trashinstance.transform.position = new Vector3(Previous.transform.position.x,  Previous.transform.position.y - spacing, trashinstance.transform.position.z);}
            }
            
            trashList.Add(trashinstance);
        }

        Destroy(trashPrefab); // Destroy the prefab after creating the trash instances Laz solution

        // Create Title
        GameObject title = new GameObject("Title");
        title.transform.parent = transform;
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




}
