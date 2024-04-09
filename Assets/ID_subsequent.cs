using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ID_subsequent: MonoBehaviour
{

    // Update is called once per frame




    void Update()
    {
        DisplayImageID();
    }

        private void DisplayImageID()
    {

        GetComponent<TextMeshProUGUI>().text = "Image ID:" + string.Format(" {0}", transform.parent.GetComponent<InteractableImageStack>().subsequent_img);

    }}