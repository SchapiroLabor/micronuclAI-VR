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


public class Trash : MonoBehaviour
{

    public InteractableImageStack Canvas_script;
    public GameObject trashPrefab;



    // Start is called before the first frame update

    // Update is called once per frame

    void OnCollisionEnter(Collision collision)


    {



        if (Canvas_script.rawImagecurrent != null)

        {

        // Check if the collision involves the other GameObject you are interested in
        if (collision.gameObject.name == Canvas_script.rawImagecurrent.name)
        {
            dispose();
        }


        else {
             Debug.Log(string.Format("No collision as if statement is not satisfied {0}", Canvas_script.rawImagecurrent.name));
        }



        }

        else {
             Debug.Log(string.Format("No collision as current object is missing {0}", Canvas_script.rawImagecurrent.name));
        }

    }



// This is executed once the trash object collider is triggered
    public void dispose()

    { 

        
         if (Canvas_script.current_img < (Canvas_script.n_imgs-1)){
        Canvas_script.current_img += 1;}

        else {
            Canvas_script.current_img = 0; 
        }
        
    if (Canvas_script != null)
        {

        Destroy(Canvas_script.rawImagecurrent);


        
        Canvas_script.init_current_img(Canvas_script.rawImagecurrent);
        //interactionManager.CancelInteractableSelection(GetComponent<IXRSelectInteractable>());
        closedisplaysecondimg();}

        else
        {
            Debug.Log(string.Format("This object appears to be missing {0}", Canvas_script));


        }






    }


    public void closedisplaysecondimg()

{

        if (Canvas_script.rawImagesubsequent != null){
        
        Destroy(Canvas_script.rawImagesubsequent);


        }

   
    }

    private GameObject createTrash(int N, RawImage rawImagecurrent)
    {
        Vector3 image_position = rawImagecurrent.GetComponent<RectTransform>().position;
        Debug.Log(image_position);
        float width = rawImagecurrent.GetComponent<RectTransform>().rect.width;
        float x_shift = (width / 100)/1.5f;
        Vector3 position = new Vector3(image_position.x + x_shift, image_position.y, image_position.z);
        Debug.Log(position);
        GameObject trashInstance = Instantiate(trashPrefab, position, Quaternion.identity, parent: transform);
        trashInstance.transform.position = new Vector3(image_position.x - x_shift, image_position.y, image_position.z);
        trashInstance.name = $"{N} Micronuclei";
        TMP_Text tmpText = trashInstance.GetComponentInChildren<TMP_Text>();
        tmpText.text = $"{N}";
        return trashInstance;
    }

    public void createBuckets()
    {   
        RawImage rawImagecurrent = Canvas_script.transform.Find("Image").gameObject.GetComponent<RawImage>();
        if (rawImagecurrent != null){
            var spacing = (rawImagecurrent.GetComponent<RectTransform>().rect.width/100)/2;
            Debug.Log($"Trash spacing from image is the following: {spacing}");
            
        List<GameObject> trashList = new List<GameObject>();

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

    private void Start()
    {
        createBuckets();
    }




}
