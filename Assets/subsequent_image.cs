using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class subsequent_image : MonoBehaviour
{
    public GameObject rawImagesubsequentprefab;
    private GameObject rawImagesubsequent;
    public GameObject rawImagecurrent;

    public GameObject Canvas;

    // Start is called before the first frame update
    void Start()
    {
        create_subsequent_img();
    }

    // Update is called once per frame
    void Update()
    {

        displaysecondimg();
        
    }



    private  void create_subsequent_img()
    {   
        if (Canvas.GetComponent<InteractableImageStack>().n_imgs > 1){
            
        

        // Create a new RawImage GameObject from the prefab
        rawImagesubsequent = Instantiate(rawImagesubsequentprefab,  Canvas.transform, true);

        rawImagesubsequent.transform.position = rawImagecurrent.transform.position;


        }
    }

// This is only executed whilst the object is selected
    public void displaysecondimg()
    {   

   
        // Display the second image only when current image has moved and rawimage has spwned
        if (rawImagecurrent.transform.position != Canvas.transform.position && rawImagesubsequent != null){

        int indx = Canvas.GetComponent<InteractableImageStack>().current_img;

        if (indx < (Canvas.GetComponent<InteractableImageStack>().n_imgs-1)){
        indx += 1;}

        else {
            indx = 0; 
        }
        
        rawImagesubsequent.GetComponent<RawImage>().texture = Canvas.GetComponent<InteractableImageStack>().images[indx];
        rawImagesubsequent.SetActive(true);


        }
    }




}
