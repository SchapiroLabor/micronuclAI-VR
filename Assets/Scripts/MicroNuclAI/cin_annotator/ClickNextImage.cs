using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
// Import functions from another script
using UnityEngine.XR.Interaction.Toolkit.Filtering;

using System.Web;
using Unity.Mathematics;
using NonGOSripts;
using UnityEngine.XR.Interaction.Toolkit; // With a static directive, you can access the members of the class by using the class name itself

namespace CinAnnotator
{
    public class ClickNextImage : MonoBehaviour
    {
        public GameObject rawImagesubsequentGO;
        public InteractableImageStack _interactableImageStack;
        public Trash _trash;
        public List<string> img_names;
        private RawImage rawImage;
        public LinkedList<Texture2D> images = new LinkedList<Texture2D>();
        public int max_imgs_to_load = 6; // Maximum number of images to load at once
        public int current_img_indx = 0;
        private int subsequent_img_indx;
        public Vector3 start_position;
        public Quaternion start_rotation = Quaternion.Euler(0, 0, 0);
        private Camera userCamera;

        // All functions independet of other objects can be placed in even functions Awake, OnEnable, Start

        void Awake()
        {
            // Function plays when the script is loaded

            if (gameObject == null)
            {
                string prefabPath = Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/Image.prefab"));
                Instantiate(Resources.Load<GameObject>(prefabPath));
            }

            gameObject.name = "Image";

            gameObject.SetActive(true);
        }

        private System.Collections.IEnumerator MyCoroutine()
        {
            // Remove the call to WaitForWholeImage since it is not being used
            getImageTextures();

            while (images.Count < max_imgs_to_load)
            {
                yield return null; // Wait for the next frame
            }

            // Initialize the image
            InitializeCurrentImage(userCamera, start_position, start_rotation);

            // Do not trust child to be initialized during Start()

            // Create and display second image
            CreateGameObjectForSecondImage();
        }

        public void Initialize()
        {

            userCamera = Camera.main;

            PositionImageStack();

            // Start the coroutine
            StartCoroutine(MyCoroutine());

            // Add function to select entered listener
            GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>().selectEntered.AddListener((args) => DisplaySecondImage());

            _trash.Initialize();
        }




        void PositionImageStack()
        {

            // Set the anchors to the centre of the screen
            transform.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            transform.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);

            // Set the pivot to the centre of the screen
            transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            // Set side lengths of the rect transform
            transform.localScale = new UnityEngine.Vector3(1, 1, 1);

            start_position = new UnityEngine.Vector3(0, 0, -_interactableImageStack.raycast_distance);

            transform.position = start_position;

        }

        public void PositionResizeText(RectTransform CurrentImage, int img_indx, int N_image)
        {
            // Set the anchors and pivots of the Text
            RectTransform textRectTransform = CurrentImage.GetChild(0).GetComponent<RectTransform>();

            // Set the anchors and pivots of the Text as sizeDelta requires absolute difference
            textRectTransform.anchorMin = new Vector2(0, 0);
            textRectTransform.anchorMax = new Vector2(0, 0);

            // Set pivot to bottom center of the Text
            textRectTransform.pivot = new Vector2(0.5f, 0.0f);

            // Set side lengths of the rect transform
            textRectTransform.localScale = Vector3.one;

            // Set the position and rotation of the child transform
            textRectTransform.SetPositionAndRotation(CurrentImage.position, CurrentImage.rotation);

            // Set the size of the Text to be 1/3 of the width of the image
            textRectTransform.sizeDelta = new Vector2(CurrentImage.sizeDelta.x, CurrentImage.sizeDelta.y / 3);

            // Set the font size of the Text same to width of image
            textRectTransform.GetComponent<TextMeshProUGUI>().fontSize = CurrentImage.sizeDelta.x * 0.1f;

            // Set the text of the Text
            textRectTransform.GetComponent<TextMeshProUGUI>().text = string.Format("Patch {0}/{1}", img_indx, N_image);

            // Centre text in the Text
            textRectTransform.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Bottom;

            // Position to top center of the RawImage
            // Uneexpected behaviour when using localPosition sizeDelta is Gemobject size + parent size from pivot
            textRectTransform.localPosition = new Vector3((CurrentImage.sizeDelta.x / 2) - (CurrentImage.sizeDelta.x * (1 - textRectTransform.pivot.x)),
            CurrentImage.sizeDelta.y - ((CurrentImage.sizeDelta.y / 2) * (1 - textRectTransform.pivot.y)), 0);


        }

        private void ResizeImgtobewithin40percentofFOV(float WD, Camera userCamera)
        {

            // Get the FOV at the panel height
            List<float> outputs = HelperFunctions.GetFOVatWD(WD, userCamera);

            float newWidth = outputs[0] * 0.4f; // Height
            float newHeight = outputs[1] * 0.4f; // Width

            // Get the width and height of the RawImage
            float width = rawImage.texture.width;
            float height = rawImage.texture.height;

            // Reduce image size whilst keeping the image aspect ratio
            float aspect_ratio = width / height;

            // Adjust the dimensions to maintain the aspect ratio
            if (newWidth > newHeight * aspect_ratio)
            {
                newWidth = newHeight * aspect_ratio; // Aspect ratio is 1, so newWidth = newHeight
            }
            else
            {
                newHeight = newWidth / aspect_ratio; // Aspect ratio is 1, so newHeight = newWidth
            }

            // Set width and height of the Canvas
            RectTransform rectTransform = GetComponent<RectTransform>();

            rectTransform.sizeDelta = new UnityEngine.Vector2(newWidth, newHeight);

            Debug.Log($"The size of the image is: {newWidth}, {newHeight}");

        }


        public (int width, int height) GetDimensions(InteractableImageStack.DataFrame bboxDict, int currentImgIndx)
        {
            // Get the dimensions of the image from the bounding box dictionary
            int xMin = bboxDict.X1[currentImgIndx];
            int yMin = bboxDict.Y1[currentImgIndx];
            int xMax = bboxDict.X2[currentImgIndx];
            int yMax = bboxDict.Y2[currentImgIndx];

            int width = xMax - xMin;
            int height = yMax - yMin;

            return (width, height);
        }


        public Texture2D LoadImg(InteractableImageStack.DataFrame bboxDict, int currentImgIndx)
        {


            // Load the image with absolute path
            string path = bboxDict.Image_path[currentImgIndx];
            img_names.Add(Path.GetFileNameWithoutExtension(path));
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(_interactableImageStack.target_size, _interactableImageStack.target_size);
            bool isLoaded = tex.LoadImage(fileData);

            if (!isLoaded)
            {
                Debug.Log("Texture did not load !!!");

            }

            return tex;


        }

        public void getImageTextures()
        {

            try
            {
               
                for (int Index = images.Count; Index < max_imgs_to_load; Index++)
                {
                    if (Index < max_imgs_to_load && Index + current_img_indx < _interactableImageStack.bbox_dict.Index.Count)
                    {
                        Debug.Log($"Count: {images.Count}, current_img_indx: {current_img_indx}/{_interactableImageStack.bbox_dict.Index.Count}, Index: {Index}/{max_imgs_to_load}");
                        images.AddLast(LoadImg(_interactableImageStack.bbox_dict, Index + current_img_indx));
                    }

                }



            }

            catch (Exception e)
            {
                Debug.LogError($"Error getting images: {e}");
            }


        }


        public void InitializeCurrentImage(Camera userCamera, Vector3 StartPosition, quaternion StartRotation)
        {

            transform.localPosition = StartPosition;
            transform.rotation = StartRotation;

            // Get the RawImage component
            GetComponent<RawImage>().texture = images.First.Value;

            rawImage = GetComponent<RawImage>();

            // Resize the image to be within 40% of the FOV
            ResizeImgtobewithin40percentofFOV(_interactableImageStack.raycast_distance, userCamera);

            // Set the collider size
            SetColliderSize(rawImage);

            // Position and resize the text
            PositionResizeText(rawImage.transform.GetComponent<RectTransform>(), current_img_indx,
            _interactableImageStack.bbox_dict.Index.Count);

            // Set non maskable to true
            rawImage.GetComponent<RawImage>().maskable = false;

        }

        private void SetColliderSize(RawImage rawImage)
        {
            rawImage.GetComponent<BoxCollider>().size = GetComponent<RectTransform>().sizeDelta;

        }





        private void CreateGameObjectForSecondImage()
        {

            // Create subsequent image only when there are more than one images
            if (_interactableImageStack.bbox_dict.Index.Count > 1)
            {

                // Create a new RawImage GameObject from the prefab

                if (rawImagesubsequentGO == null)
                {
                    rawImagesubsequentGO = Instantiate(Resources.Load<GameObject>(Path.Combine("MicroNuclAI",
                    Path.GetFileNameWithoutExtension("MicroNuclAI/SubsequentImage.prefab"))), transform.position,
                    transform.rotation);
                    rawImagesubsequentGO.GetComponent<RawImage>().SetNativeSize();

                }

                Debug.Log("RawImageSubsequent is not null");

                rawImagesubsequentGO.transform.localPosition = start_position;
                rawImagesubsequentGO.transform.rotation = start_rotation;
                rawImagesubsequentGO.SetActive(false);
                rawImagesubsequentGO.GetComponent<RawImage>().maskable = false;
                rawImagesubsequentGO.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
                rawImagesubsequentGO.GetComponent<RectTransform>().localScale = GetComponent<RectTransform>().localScale;
            }

            else
            {
                Debug.Log("Only one image available");
            }
        }



        private void DisplaySecondImage()
        {
            if (this.gameObject != null && rawImagesubsequentGO != null)
            {
                int N_images = _interactableImageStack.bbox_dict.Index.Count;

                subsequent_img_indx = current_img_indx + 1;

                if (subsequent_img_indx < N_images)
                {
                    // Increment the index to get the next image
                    rawImagesubsequentGO.GetComponent<RawImage>().texture = images.First.Next.Value; // Set second image texture
                    PositionResizeText(rawImagesubsequentGO.transform.GetComponent<RectTransform>(), subsequent_img_indx, N_images);
                    rawImagesubsequentGO.SetActive(true);
                }



            }
            else
            {
                Debug.Log("RawImageCurrent is null");
            }
        }




    }


}