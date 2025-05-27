using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using System.Linq;


namespace CinAnnotator
{
    public class Tinyt : MonoBehaviour
    {

        private Color originalColor;
        private Color originalEmissionColor;
        private Material material;
        public GameObject Image;
        private float img_height;
        private float img_width;
        private Bounds bounds;


        public class TrashDataFrame

        {
            public LinkedList<int> patches = new LinkedList<int>();
            public LinkedList<string> patches_names = new LinkedList<string>();
            public LinkedList<int> keys = new LinkedList<int>();

            public void RegisterImage(int img_indx, string img_name, int key)
            {
                patches.AddLast(img_indx);
                patches_names.AddLast(img_name);
                keys.AddLast(key);
            }

            public List<List<object>> RetrieveData()
            {
                List<List<object>> data = new List<List<object>>
                {
                    patches.Cast<object>().ToList(),
                    patches_names.Cast<object>().ToList(),
                    keys.Cast<object>().ToList()
                };
                return data;
            }

        }

        



        // Start is called before the first frame update
        public void Initialize(Transform Image_t)
        {

            Image = Image_t.gameObject;

            // Cache the Renderer's material and original color at start
            material = GetComponent<Renderer>().material;
            originalColor = material.color;

            // Ensure the material supports emission color by enabling emission
            material.EnableKeyword("_EMISSION");

            // Assuming the original emission is set and needs to be stored
            originalEmissionColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);

            GetComponent<Renderer>().material.SetColor("_EmissionColor", originalEmissionColor);

            // Log if emission is on
            SchapiroLabLog.Log("Emission enabled: " + material.IsKeywordEnabled("_EMISSION"));


            Image.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>().selectExited.AddListener((args) => Trashifwithinbounds());

            // Get the image height and width
            img_height = Image.GetComponent<RectTransform>().rect.height;
            img_width = Image.GetComponent<RectTransform>().rect.width;

        }



        // Update is called once per frame
        void Update()
        {
            confirm_if_within_bounds();
        }



        private void confirm_if_within_bounds()
        {

            if (Image != null && this != null)
            {
                Bounds temp = Image.GetComponent<BoxCollider>().bounds;

                Bounds img_bounds = new Bounds(new Vector3(temp.center.x, temp.center.y, bounds.center.z), new Vector3(0.2f, 0.2f, 1));

                // Confrim if bounding box intersects with renderer bounds

                Collider renderer = GetComponent<MeshCollider>();

                bounds = renderer.bounds;

                if (bounds.Intersects(img_bounds))
                {
                    change2brightgreen();
                }
                else
                {
                    RevertToOriginalColor();
                }
            }
        }

        public void SavePatch(int img_indx, List<string> img_names)
        {
            // Add index to list
            patches.AddLast(img_indx);

            // Add image name and the trash count to a list
            patches_names.AddLast(img_names[img_indx]);

            // Get first character of the gameobject name
            keys.AddLast(Int32.Parse(transform.gameObject.name.Substring(0, 1)));

        }

        public void RemovePatch()
        {
            if (patches.Count > 0)
            {
                // Remove index from list
                patches.RemoveLast();

                // Remove image name and the trash count from a list
                patches_names.RemoveLast();

                keys.RemoveLast();
            }

        }

        private void Trashifwithinbounds()
        {

            if (Image != null)
            {
                Collider renderer = GetComponent<MeshCollider>();

                //  Confirm if image area is intersecting with the trash area


                var bounds = renderer.bounds;

                Bounds temp = Image.GetComponent<BoxCollider>().bounds;

                Bounds img_bounds = new Bounds(new Vector3(temp.center.x, temp.center.y, bounds.center.z), new Vector3(0.2f, 0.2f, 1));

                if (bounds.Intersects(img_bounds))
                {
                    SavePatch(Image.GetComponent<ClickNextImage>().current_img_indx, Image.GetComponent<ClickNextImage>().img_names);

                    transform.parent.GetComponent<Trash>().dispose(transform.gameObject.name);

                }

            }
        }

        private void change2brightgreen()
        {         // Ensure the material supports emission color by enabling emission
            GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            Color color = new Color(0f, 1f, 0f, 1f);
            // Set the emission color to a bright green
            GetComponent<Renderer>().material.SetColor("_EmissionColor", color);

            SchapiroLabLog.Log($"Emission color on : {color}");
            //transform.parent.GetComponent<Trash>().dispose();}
        }


        // Call this method to revert to the original color and emission
        private void RevertToOriginalColor()
        {

            material.SetColor("_EmissionColor", originalEmissionColor);

            SchapiroLabLog.Log($"Emission color off : {originalEmissionColor}");
        }




    }
}