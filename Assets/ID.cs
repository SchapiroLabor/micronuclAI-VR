using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ID : MonoBehaviour
{

    // Update is called once per frame

void Start(){}


    void Update()
    {
        DisplayImageID();
    }

    private void DisplayImageID()
    {
        if (transform.parent.GetComponent<InteractableImageStack>() != null)

        {GetComponent<TextMeshProUGUI>().text = "Image ID:" + string.Format(" {0}", transform.parent.GetComponent<InteractableImageStack>().current_img);}
        

    }}
