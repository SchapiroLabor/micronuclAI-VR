using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using System.Linq;

using UnityEngine.XR.Interaction.Toolkit;


namespace CinAnnotator
{
    public class Tinyt : MonoBehaviour
    {


        public TrashDataFrame df = new TrashDataFrame();
        [SerializeField] private InteractableImageStack _interactableImageStack;
        [SerializeField] private GameObject Image;
        [SerializeField] private ClickNextImage _clickNextImage;
        [SerializeField] private Trash _trash;



        public class TrashDataFrame

        {
            public LinkedList<int> patch_index = new LinkedList<int>();
            public LinkedList<string> patch_name = new LinkedList<string>();
            public LinkedList<int> patch_key = new LinkedList<int>();

            public void RegisterImage(int img_indx, string img_name, Transform transform)
            {
                patch_index.AddLast(img_indx);
                patch_name.AddLast(img_name);
                patch_key.AddLast(Int32.Parse(transform.gameObject.name.Substring(0, 1)));
            }

            public void RemoveImage()
            {
                patch_index.RemoveLast();
                patch_name.RemoveLast();
                patch_key.RemoveLast();
            }

            public void MergeDFs(TrashDataFrame df)
            {
                patch_index = new LinkedList<int>(patch_index.Concat(df.patch_index));
                patch_name = new LinkedList<string>(patch_name.Concat(df.patch_name));
                patch_key = new LinkedList<int>(patch_key.Concat(df.patch_key));
            }

            public void Save2CSV(string filePath)
            {
                // Open a StreamWriter to write to the CSV file
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    var indexEnum = patch_index.GetEnumerator();
                    var nameEnum = patch_name.GetEnumerator();
                    var keyEnum = patch_key.GetEnumerator();

                    while (indexEnum.MoveNext() && nameEnum.MoveNext() && keyEnum.MoveNext())
                    {
                        writer.WriteLine($"{indexEnum.Current},{nameEnum.Current},{keyEnum.Current}");
                    }
                }
            }


        }


        // This is executed once the trash object collider is triggered
        public void dispose()
        {

            GameObject ImageCurrent = _clickNextImage.gameObject;

            // Get current image index
            if (_clickNextImage.current_img_indx < _interactableImageStack.bbox_dict.Index.Count)
            {

                if (ImageCurrent != null)
                {

                    df.RegisterImage(Image.GetComponent<ClickNextImage>().current_img_indx,
                    Image.GetComponent<ClickNextImage>().img_names[Image.GetComponent<ClickNextImage>().current_img_indx],
                    transform);
                    // Delete in one frame
                    ImageCurrent.SetActive(false);

                    // Next image in the stack
                    _trash.NextImage();

                    // Switch subsequent image with current image
                    _trash.ImageStackIndexing(ImageCurrent, _clickNextImage.images);

                    // Load additional texture to maintain 6 images in the stack
                    _clickNextImage.getImageTextures();

                    _trash.current_trash = transform;

                }
                else
                {
                    Debug.Log(string.Format("This object appears to be missing {0}", ImageCurrent.name));
                }

            }
            else
            {
                Debug.Log("No more images to display");
            }





        }



        // Start is called before the first frame update
        public void Initialize(Transform Image_t)
        {
            // Setup the emission color
            ChangeColorSetup();

            _interactableImageStack = transform.parent.parent.parent.GetComponent<InteractableImageStack>();
            _clickNextImage = Image_t.GetComponent<ClickNextImage>();
            _trash = transform.parent.GetComponent<Trash>();

            Image = Image_t.gameObject;

            Image.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>().selectExited.AddListener((args) =>
            Trashifwithinbounds());

            // Add XRSIMPLEINTERACTABLE component if not already present
            if (GetComponent<XRSimpleInteractable>() == null)
            {
                gameObject.AddComponent<XRSimpleInteractable>();
            }

            // TODO: Change this to image intersecting not raycast
            GetComponent<XRSimpleInteractable>().hoverEntered.AddListener((args) => change2brightgreen());

            GetComponent<XRSimpleInteractable>().hoverExited.AddListener((args) => RevertToOriginalColor());

        }

        [SerializeField] private Material highlightMaterial;
        private Material OldMaterial;

        private void ChangeColorSetup()
        {
            // TODO: Change material instead of emission color
            OldMaterial = GetComponent<MeshRenderer>().material;
        }




        void OnCollisionEnter(Collision collision)
        {

            Trashifwithinbounds();
        }


        private void Trashifwithinbounds()

        { // TODO: Turn this into an event callback

            if (Image != null)
            {


                Bounds temp = Image.GetComponent<BoxCollider>().bounds;

                Bounds bounds = GetComponent<MeshCollider>().bounds;

                Bounds img_bounds = new Bounds(new Vector3(temp.center.x, temp.center.y, bounds.center.z),
                new Vector3(0.2f, 0.2f, 1));

                // Confrim if bounding box intersects with renderer bounds
                // If this is not put in place, 4 images at once are disposed. Investigate later why.
                if (bounds.Intersects(img_bounds))
                {
                    dispose();

                }



            }
        }




        private void change2brightgreen()
        {
            // Set the emission color to a bright green
            GetComponent<MeshRenderer>().material = highlightMaterial;
            //transform.parent.GetComponent<Trash>().dispose();}
        }


        // Call this method to revert to the original color and emission
        private void RevertToOriginalColor()
        {
            GetComponent<MeshRenderer>().material = OldMaterial;
        }




    }
}