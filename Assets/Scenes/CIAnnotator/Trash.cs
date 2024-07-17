using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;


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
        float x_shift = width / 2;
        GameObject trashInstance = Instantiate(trashPrefab, new Vector3(image_position.x + x_shift, image_position.y, image_position.z), Quaternion.identity, parent: transform);
        trashInstance.transform.position = new Vector3(image_position.x + x_shift, image_position.y, image_position.z);
        trashInstance.name = $"{N} Micronuclei";
        return trashInstance;
    }

    public void createBuckets()
    {   
        RawImage rawImagecurrent = Canvas_script.transform.Find("Image").gameObject.GetComponent<RawImage>();
        GameObject Current = null;
        if (rawImagecurrent != null){
            float spacing = rawImagecurrent.GetComponent<RectTransform>().rect.width * 0.25f;
            
        
        
        for (int n = 1; n <= 5; n++){
            GameObject trashinstance = createTrash(n, rawImagecurrent);
            if (Current != null){
                Current.transform.position = new Vector3(Current.transform.position.x, Current.transform.position.y - spacing, Current.transform.position.z);
            }
            Current = trashinstance;
        }
        }

    }

    private void Start()
    {
        createBuckets();
    }




}
