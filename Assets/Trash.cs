using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;


public class Trash : MonoBehaviour
{

    public InteractableImageStack Canvas_script;
    

    
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


        
        Canvas_script.init_current_img(Canvas_script.rawImagecurrentprefab);
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


}
