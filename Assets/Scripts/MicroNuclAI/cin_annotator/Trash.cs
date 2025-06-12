using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
using Unity.VisualScripting;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
// Import functions from another script
using System.Numerics; // With a static directive, you can access the members of the class by using the class name itself
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector3;
using System.IO;
using NonGOSripts;


namespace CinAnnotator
{
    public class Trash : MonoBehaviour
    {

        [SerializeField] private GameObject trashPrefab;
        public InteractableImageStack _interactableImageStack;
        [SerializeField] private ClickNextImage _clickNextImage;
        GameObject Texinstance;
        [SerializeField] private Camera userCamera;
        private List<GameObject> trashList = new List<GameObject>();
        public Transform current_trash;


        void Awake()
        {

            // Set anchors to left bottom
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            // Set pivot to center right
            rectTransform.pivot = new Vector2(1f, 0.5f);

        }




        public void Initialize()
        {

            Transform CurrentImage = _clickNextImage.transform;
            Vector3 image_position = CurrentImage.position;
            float width = CurrentImage.GetComponent<RectTransform>().rect.width / 2;
            float x_shift = width;
            // Have to use local position becuase world positions provides unexpected results
            Vector3 position = new Vector3(image_position.x - x_shift, image_position.y, image_position.z);
            transform.position = position;

            Vector2 fov = ResizePanel(transform.parent.GetComponent<GridMaker>().raycast_distance,
            userCamera);

            // Set Grid Layour group spacing to 10% of image width
            GridLayoutGroup gridLayoutGroup = GetComponent<GridLayoutGroup>();
            gridLayoutGroup.spacing = 0.01f * fov;
            gridLayoutGroup.cellSize = fov / 4;

            // Above only works if content size fitters exists
            createTrashs(CurrentImage);
        }


        private Vector2 ResizePanel(float WD, Camera userCamera)
        {

            // Get the FOV at the panel height
            List<float> outputs = HelperFunctions.GetFOVatWD(WD, userCamera);

            float newWidth = outputs[0] * 0.6f; // Height
            float newHeight = outputs[1] * 0.6f; // Width

            // Reduce image size whilst keeping the image aspect ratio
            float aspect_ratio = 1;

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

            return new UnityEngine.Vector2(newWidth, newHeight);

        }

        private void RepositionCurrentImage(GameObject ImageCurrent)
        {
            ImageCurrent.GetComponent<RectTransform>().localPosition = _clickNextImage.start_position;
            ImageCurrent.GetComponent<RectTransform>().rotation = _clickNextImage.start_rotation;
        }

        private GameObject InformNoMoreImages(GameObject ImageCurrent)
        {
            // Just set it off
            ImageCurrent.SetActive(false);

            // Replace with loading and instantiating text object
            // Load a text object
            GameObject textObject = Resources.Load<GameObject>(Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/Text (TMP).prefab")));

            // Instantiate the text object
            GameObject textInstance = Instantiate(textObject, ImageCurrent.transform);

            // Set parent to be that of image's
            textInstance.transform.parent = ImageCurrent.transform.parent;

            // Set the text object to be active
            textInstance.SetActive(true);

            // Set the pivot to be at the center of the image
            textInstance.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            // Set the anchors to be at the center of the image
            textInstance.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            textInstance.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);

            // Set the text object to be at the center of the image
            textInstance.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);

            // Set rotation to zero
            textInstance.GetComponent<RectTransform>().rotation = UnityEngine.Quaternion.Euler(0, 0, 0);

            // Add content size fitter to the text object
            textInstance.AddComponent<ContentSizeFitter>();
            // Horizontal and vertical fit to preferred size
            textInstance.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            textInstance.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Set font size same as image title
            textInstance.GetComponent<TextMeshProUGUI>().fontSize = ImageCurrent.GetComponentInChildren<TextMeshProUGUI>().fontSize * 0.8f;

            // Display a message to the user that there are no more images to display
            textInstance.GetComponent<TextMeshProUGUI>().text = "No more images to display";

            // Set the text to be at the center of the image
            textInstance.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Text color böack
            textInstance.GetComponent<TextMeshProUGUI>().color = Color.white;

            Debug.Log($"No more images to display, executed for: {_clickNextImage.current_img_indx}");

            return textInstance;
        }

        public void ImageStackIndexing(GameObject ImageCurrent, LinkedList<Texture2D> images)

        {

            if (ImageCurrent == null || ImageCurrent.activeSelf == false)
            {
                // Close second image
                _clickNextImage.rawImagesubsequentGO.SetActive(false);

                int N_images = _interactableImageStack.bbox_dict.Index.Count;

                // If current image is index is below N_images, reinitialize the image
                if (_clickNextImage.current_img_indx < N_images)
                {
                    ImageCurrent.SetActive(true);
                    // Take second image as the current image
                    ImageCurrent.GetComponent<RawImage>().texture = images.First.Value;
                    _clickNextImage.PositionResizeText(ImageCurrent.GetComponent<RectTransform>(),
                    _clickNextImage.current_img_indx, N_images);
                    RepositionCurrentImage(ImageCurrent);

                    if (Texinstance != null)
                    {
                        // nEED TO LOOK INTO IF DESTROY ADN CREATE MAY CREATE TOO MUCH OVERHEAD
                        Destroy(Texinstance);
                    }

                }
                else
                {
                    Texinstance = InformNoMoreImages(ImageCurrent);
                    RepositionCurrentImage(ImageCurrent);

                }
            }
            else
            {
                Debug.Log(string.Format("This object appears to be missing {0}", ImageCurrent.name));

            }
        }

        public void NextImage()
        {                    // Remove the first image from the linked list
            _clickNextImage.images.RemoveFirst();

            _clickNextImage.current_img_indx += 1;

        }

        public void PreviousImage()
        {   // Remove the first image from the linked list
            _clickNextImage.images.RemoveLast();

            _clickNextImage.current_img_indx -= 1;

            // Add the last trash to the linked list of images
            _clickNextImage.images.AddFirst(_clickNextImage.LoadImg(_interactableImageStack.bbox_dict,
            _clickNextImage.current_img_indx));



        }
        public void ReverseDispose()
        {   // Could use UnityEngine.Pool for this to improve memory management
            GameObject currentImage = _clickNextImage.gameObject;

            // Get current image index
            if (_clickNextImage.current_img_indx < _interactableImageStack.bbox_dict.Index.Count)
            {

                if (current_trash.GetComponent<Tinyt>().df.patch_index.Count > 0)
                {
                    current_trash.GetComponent<Tinyt>().df.RemoveImage();

                    if (currentImage != null)
                    {
                        currentImage.SetActive(false);

                        PreviousImage();

                        // Switch subsequent image with current image
                        ImageStackIndexing(currentImage, _clickNextImage.images);
                    }
                    else
                    {
                        Debug.Log(string.Format("This object appears to be missing {0}", currentImage.name));
                    }
                }
            }

        }

        public void InitializeTrash()
        {
            GameObject trashinstance = createTrash(trashList.Count + 1, _clickNextImage.transform);

            trashList.Add(trashinstance);

        }

        private GameObject createTrash(int N, Transform CurrentImage)
        {

            GameObject trashInstance = Instantiate(trashPrefab, transform);
            trashInstance.transform.position = new Vector3(trashInstance.transform.position.x,
            trashInstance.transform.position.y, transform.position.z);
            trashInstance.transform.localScale = Vector3.one;
            trashInstance.name = $"{N} Micronuclei";
            TMP_Text tmpText = trashInstance.GetComponentInChildren<TMP_Text>();
            tmpText.text = $"{N}";
            trashInstance.GetComponent<Tinyt>().Initialize(CurrentImage);

            return trashInstance;
        }

        public void createTrashs(Transform CurrentImage)
        {
            RawImage rawImagecurrent = CurrentImage.GetComponent<RawImage>();

            if (rawImagecurrent != null)
            {
                // Load the trash prefab if not already loaded
                if (trashPrefab == null)
                {
                    trashPrefab = Resources.Load<GameObject>(Path.Combine("MicroNuclAI",
                    Path.GetFileNameWithoutExtension("MicroNuclAI/trash_text.prefab")));
                }

                for (int n = 0; n <= 3; n++)
                {

                    GameObject trashinstance = createTrash(n, CurrentImage);

                    trashList.Add(trashinstance);
                }


                // Create Title
                GameObject title = new GameObject("Title");
                title.transform.parent = transform.parent;

                // Pivot of current class is at 1, 0.5
                title.AddComponent<TextMeshPro>();
                TextMeshPro titleText = title.GetComponent<TextMeshPro>();
                UnityEngine.Vector3 position = transform.position;
                title.GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 0);
                UnityEngine.Vector2 size = new UnityEngine.Vector2(rawImagecurrent.GetComponent<RectTransform>().rect.width,
                rawImagecurrent.GetComponent<RectTransform>().rect.height);

                // Add content size fitter to the title
                title.AddComponent<ContentSizeFitter>();
                title.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                title.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                title.transform.position = new UnityEngine.Vector3(position.x - (size.x / 2) * 1.5f,
                (position.y + (size.y / 2)) * 1.5f, position.z);

                titleText.text = "Micronuclei count";
                titleText.fontSize = size.x;
                titleText.alignment = TextAlignmentOptions.Center;


            }



        }

    }
}