using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tinyt : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Start is called before the first frame update

    // Update is called once per frame

    void OnCollisionEnter(Collision collision)


    {

        // Check if the collision involves the other GameObject you are interested in
        if (collision.gameObject.name == "Image")
        {
            transform.parent.GetComponent<Trash>().dispose();
        }

        else 
        {
             Debug.Log(string.Format("No collision as if statement is not satisfied {0}", collision.gameObject.name));
        }

    }
}
