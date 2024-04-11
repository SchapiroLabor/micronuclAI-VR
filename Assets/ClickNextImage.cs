using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Unity.VisualScripting;

public class ClickNextImage : MonoBehaviour
{
    //public XRGrabInteractable interactable;
    // Start is called before the first frame update
    
    public BoxCollider boxCollider;
    public GameObject Canvas;
    
   void Start()

   {
    if (boxCollider == true)
    Debug.Log(string.Format("Collision with {0}", "exists"));
    else
    {Debug.Log(string.Format("Collision with {0}", "does not exists"));}

   }



    void Update()


    {




    }
    public void onSelectEnter()


    { Canvas.GetComponent<InteractableImageStack>().displaysecondimg(Canvas.GetComponent<InteractableImageStack>().rawImagesubsequentprefab);
        

    }



}
















