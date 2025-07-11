using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
// Import functions from another script
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using static SchapiroLabLog;
using NonGOSripts;
using General;
// With a static directive, you can access the members of the class by using the class name itself

namespace CinAnnotator
{
    public class WholeImage : MonoBehaviour
    {   // TODO: Correct this script please

        [SerializeField] private InteractableImageStack _interactableImageStack;
        [SerializeField] private GridMaker _gridMaker;
        [SerializeField] private ClickNextImage _clickNextImage;
        private GameObject Arrow;
        private float height;
        private float width;

        private float height2viz;
        private float width2viz;


        private Vector3 start_position; // Default distance to raycast from the camera, please do not change this !!!
        private Quaternion start_rotation;
        private float newWidth;
        private float newHeight;
        /* static List<element> data; */
        public TeleportationProvider teleportationProvider;
        public InputActionReference TeleportActionMap;

        [SerializeField] private ActionBasedController rightController;
        [SerializeField] private ActionBasedController leftController;

        // All functions independet of other objects can be placed in even functions Awake, OnEnable, Start



        void Awake()
        {
            gameObject.name = "Image";

            // Setup anchors and pivots
            RectTransform rectTransform = GetComponent<RectTransform>();
            NonGOSripts.HelperFunctions.SetupAnchorsAndPivots(rectTransform);

            // Set the anchors and pivots of the Canvas as sizeDelta requires absolute difference
            transform.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            transform.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);





        }


        public System.Collections.IEnumerator SetTextureOnWholeImage(string img_path)
        {
            // Remove the call to WaitForWholeImage since it is not being used
            // Load the texture from the specified path
            // Get image size from the interactable image stack
            int width_img = _interactableImageStack.bbox_dict.whole_well_img_shape_X;
            int height_img = _interactableImageStack.bbox_dict.whole_well_img_shape_Y;

            Texture2D whole_img_texture = LoadTexture(img_path, width_img, height_img);

            // Set interpolation to linear
            whole_img_texture.filterMode = FilterMode.Bilinear;

            // Size delta must be explicitly matched to the size of the image
            GetComponent<RawImage>().texture = whole_img_texture;
            //RectTransform rectTransform = GetComponent<RawImage>().GetComponent<RectTransform>();
            // Reduce Local Scale
            //rectTransform.localScale = new UnityEngine.Vector3(10/width, 10/height, 1f);
            //rectTransform.localScale = new UnityEngine.Vector3(1f, 1f, 1f);

            while (GetComponent<RawImage>().texture == null)
            {
                Debug.Log("Waiting for whole image texture to be set...");
                yield return null; // Wait for a short time before checking again
            }
        }

        [SerializeField] private InputAction unifiedSelectAction;

        void Update()
        { /*
            // Check if both controllers have their select action activated
            if (unifiedSelectAction != null)
            {
                bool Selected = unifiedSelectAction.ReadValue<float>() > 0.5f;

                if (Selected)
                {
                    if (!comboTriggered)
                    {
                        comboTriggered = true;
                        Teleoport2midpoint();
                    }

                    else
                    {
                        comboTriggered = false;
                    }

                }
            } */
        }


        // Start is called before the first frame update
        public void Initialize(Transform Panel, Camera userCamera)
        {


            width = _interactableImageStack.bbox_dict.whole_well_img_shape_X;
            height = _interactableImageStack.bbox_dict.whole_well_img_shape_Y;
            Debug.Log($"Size of img: {width} {height}");

            StartCoroutine(SetTextureOnWholeImage(_interactableImageStack.bbox_dict.whole_well_img_path));
            PositionWholeImage(Panel, userCamera);



            // Should occure after the image is positioned as we are using world coordinates
            PositionImagetitle(transform.GetChild(0));

            // Initialize Arrow
            InitializeArrow();

            // STart position of the player
            start_position = Camera.main.transform.position;
            start_rotation = Camera.main.transform.rotation;

            initialposition = Camera.main.transform.position;
            //ReturnButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Teleport2Originalposition());
            //_setupButtons.standardiseButton(ReturnButton.gameObject);



            // Set fontsize
            // The fontSize property in TextMeshProUGUI is in units relative to the Canvas settings and does not automatically scale with 
            // the parent transform's scale in World Space canvases. 
            // If the Canvas is in World Space, the apparent size of the text will be affected by the parent transform's scale, 
            // but the fontSize value itself is not automatically adjusted.
            // To ensure consistent appearance, you may need to manually adjust fontSize based on 
            // the parent's lossyScale if the Canvas is in World Space.

            //ReturnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Return";
            //ReturnButton.gameObject.SetActive(false);

            // Set anchor to pixel coordinate origin


            //TeleportActionMap.action.started += ctx => Return2Start();
        }

        public void DisplayCell()
        {

            Rect bbox_ = _interactableImageStack.bbox_dict.GetBBOX(_clickNextImage.current_img_indx, downsampled: false, canvasscalefactor: 1f);

            // Color the pixel cluster in the bounding box
            //ColorPixelCluster(bbox_, Color.red);

            // Position the arrow at the center of the bounding box
            //PositionArrow(bbox);

            // Move player to the center of the patch
            Teleoport2midpoint();
        }

        private void ColorPixelCluster(Rect bbox, Color newColor)
        {
            // Clone the original texture
            Texture2D originalTexture = GetComponent<RawImage>().texture as Texture2D;

            int boundary_thickness = width > height ? (int)MathF.Pow(width, 1f / 3f) : (int)MathF.Pow(height, 1f / 3f);

            // Instead just teleport


            // Clamp bounding box to texture bounds
            int xMin = Mathf.Max(0, (int)bbox.xMin - boundary_thickness);
            int xMax = Mathf.Min(originalTexture.width, (int)bbox.xMax + boundary_thickness);
            int yMin = Mathf.Max(0, (int)bbox.yMin - boundary_thickness);
            int yMax = Mathf.Min(originalTexture.height, (int)bbox.yMax + boundary_thickness);

            Debug.Log($"Bounding box coordinates highlighting coordinates: xMin={xMin}, xMax={xMax}, yMin={yMin}, yMax={yMax}");

            // Compute the border coordinates and iterate through them
            List<(int x, int y)> borderCoords = new List<(int x, int y)>();
            /*
            // Top border
            for (int y = yMin; y < (int)bbox.yMin; y++)
                for (int x = xMin; x < xMax; x++)
                    borderCoords.Add((x, y));

            // Bottom border
            for (int y = (int)bbox.yMax; y < yMax; y++)
                for (int x = xMin; x < xMax; x++)
                    borderCoords.Add((x, y));

            // Left and right borders
            for (int y = (int)bbox.yMin; y < (int)bbox.yMax; y++)
            {
                for (int x = xMin; x < (int)bbox.xMin; x++)
                    borderCoords.Add((x, y));
                for (int x = (int)bbox.xMax; x < xMax; x++)
                    borderCoords.Add((x, y));
            } */

            // Generate all coordinate combinations for x: 0-100 and y: 0-100
            for (int y = (int)10057 - 200; y <= (int)10057; y++)
            {
                for (int x = (int)9072 - 200; x <= (int)9072; x++)
                {
                    borderCoords.Add((x, y));
                }
            }

            /*
            for (int x = (int)bbox.xMin; x <= (int)bbox.xMax; x++)
            {
                for (int y = (int)bbox.yMin; y <= (int)bbox.yMax; y++)
                {
                    borderCoords.Add((x, y));
                }
            }*/
            // Set the color for each border coordinate
            foreach (var (x, y) in borderCoords)
            {
                originalTexture.SetPixel(y, x, newColor);
            }
            // Apply all changes to the texture
            originalTexture.Apply();
        }


        private void PositionArrow(Rect bbox)
        {
            //UnityEngine.Vector2 pixel_coords = GetPatchMidPoint(bbox);

            //Vector2 local_coords = Pixel2UnityCoord(pixel_coords, true);

            //Arrow.transform.localRotation = UnityEngine.Quaternion.Euler(270, 0, 0);
            //Arrow.transform.localPosition = new UnityEngine.Vector3(pixel_coords.y, pixel_coords.x, 0);
            //Arrow.transform.position = new UnityEngine.Vector3(local_coords.y, local_coords.x, 0);

            //Arrow.transform.position = new UnityEngine.Vector3(0, 0, 5);

            // Log maximum and minimum local position
            if (!Arrow.activeSelf)
            {
                Arrow.SetActive(true);
            }

            //Arrow.transform.localRotation = UnityEngine.Quaternion.Euler(295, 0, -25);
        }

        private void InitializeArrow()
        {
            Arrow = transform.GetChild(1).gameObject;

            Arrow.SetActive(false);

            // Square root area of image
            Arrow.transform.localScale = new UnityEngine.Vector3(3, 6, 1);

        }



        // Methods relating to teleportation
        private Vector2 RescalePixelCoords(Vector2 pixel_coords)
        {
            // Get image resize factor
            float resize_factor_W = newWidth / width;
            float resize_factor_H = newHeight / height;
            return new Vector2(pixel_coords.x * resize_factor_W, pixel_coords.y * resize_factor_H);
        }

        private Vector2 GetPatchMidPoint(Rect bbox)
        {
            // Log minimum 
            Debug.Log($"The minimum coordinates are {bbox.xMin}, {bbox.yMin} and maximum coordinates are {bbox.xMax}, {bbox.yMax}");

            UnityEngine.Vector2 mid_point_pixel = new UnityEngine.Vector2(
                (bbox.xMin + bbox.xMax) / 2,
                (bbox.yMin + bbox.yMax) / 2
            );

            Debug.Log($"The mid point pixel coordinates are {mid_point_pixel.x}, {mid_point_pixel.y}");

            // Get the pixel position of the patch
            //UnityEngine.Vector2 rescaled_mid_point_pixel = RescalePixelCoords(mid_point_pixel);

            //Debug.Log($"The rescaled mid point pixel coordinates are {rescaled_mid_point_pixel.x}, {rescaled_mid_point_pixel.y}");

            return mid_point_pixel;
        }


        bool comboTriggered = false;

        private Vector2 Pixel2UnityCoord(UnityEngine.Vector2 pixel_coords, bool child = false)
        {

            Vector3 local_midpoint = Vector3.zero;

            if (child is false)
            {
                // Transform local midpoint to world coordinates
                Vector3 mid_point_image = transform.TransformPoint(local_midpoint);
                // Traverse by half the width and height of the image
                UnityEngine.Vector3 coords = mid_point_image - new UnityEngine.Vector3(newWidth / 2, mid_point_image.y, newHeight / 2) +
                new UnityEngine.Vector3(pixel_coords.x, transform.position.y, pixel_coords.y);
                Debug.Log($"Pixel to world coordinates are {pixel_coords} and {coords}");
                return mid_point_image;
            }
            else
            {
                // Traverse by half the width and height of the image
                // Origin is at x=-4.24 and y=-3.64

                // Normalised rect coordinates to local coordinates
                float img_width = GetComponent<RectTransform>().sizeDelta.x;
                float img_height = GetComponent<RectTransform>().sizeDelta.y;

                Debug.Log($"Image size is {img_width} and {img_height}");

                UnityEngine.Vector2 coords = local_midpoint - new UnityEngine.Vector3(img_width / 2, img_height / 2, 0) /*+
                new UnityEngine.Vector3(pixel_coords.x, pixel_coords.y, 0) */;
                Debug.Log($"Rect origin is {coords.x}, {coords.y}");

                // TODO: Write down maximum and minimum local position given by image size. 
                // Afterwards, compare to rescaled pixel coordinates given by patches
                UnityEngine.Vector2 adjusted_coords = coords + pixel_coords;

                // TODO: THe set pixels function of UNity might be buggy. Will remove that function.
                // UnityEngine.Vector2 adjusted_coords = coords + new Vector2(0, newHeight);
                Debug.Log($"Pixel to local coordinates are {adjusted_coords.x}, {adjusted_coords.y}");


                return adjusted_coords;
            }
        }


        private void Teleoport2midpoint()
        {
            // Get the mid point of the patch
            //UnityEngine.Vector2 mid_point_pixel = GetPatchMidPoint(bbox);

            // Get the mid point in world coordinates
            //UnityEngine.Vector2 mid_point_world = Pixel2UnityCoord(mid_point_pixel);

            // Create Teleportation request
            TeleportRequest telepoint = new TeleportRequest();
            // Get world coordinates of local position 0,0,0
            Vector3 centre = transform.TransformPoint(new Vector3(0, 0, 0));
            telepoint.destinationPosition = centre - new Vector3(0, 0.5f, 0); // Move the player back by 1.5 units to see the whole image
            telepoint.destinationRotation = Quaternion.Euler(0, 0, 0);
            /*
            ReturnButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = _setupButtons.fontSize;
            // Set wrapping to overflow
            ReturnButton.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = false;
            //Warp canvas onto Button size
            ReturnButton.transform.parent.GetComponent<RectTransform>().sizeDelta = ReturnButton.transform.GetComponent<RectTransform>().sizeDelta;
            ReturnButton.transform.parent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            ReturnButton.transform.parent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            ReturnButton.gameObject.SetActive(true); */
            // Move the player to the patch position
            teleportationProvider.QueueTeleportRequest(telepoint);


        }
        Vector3 initialposition;






        private void Teleport2Originalposition()
        {
            // Get the mid point of the patch
            //UnityEngine.Vector2 mid_point_pixel = GetPatchMidPoint(bbox);

            // Get the mid point in world coordinates
            //UnityEngine.Vector2 mid_point_world = Pixel2UnityCoord(mid_point_pixel);

            // Create Teleportation request
            TeleportRequest telepoint = new TeleportRequest();
            // Get world coordinates of local position 0,0,0
            telepoint.destinationPosition = initialposition;
            telepoint.destinationRotation = Quaternion.Euler(0, 0, 0);
            //ReturnButton.gameObject.SetActive(false);
            // Move the player to the patch position
            teleportationProvider.QueueTeleportRequest(telepoint);
        }


        private void MovePlayer2PixelPosition(Rect bbox)
        {
            // Get the mid point of the patch
            UnityEngine.Vector2 mid_point_pixel = GetPatchMidPoint(bbox);

            // Get the mid point in world coordinates
            UnityEngine.Vector2 mid_point_world = Pixel2UnityCoord(mid_point_pixel);

            // Create Teleportation request
            TeleportRequest telepoint = new TeleportRequest();
            telepoint.destinationPosition = mid_point_world;
            telepoint.destinationRotation = Quaternion.Euler(0, 0, 0);

            // Move the player to the patch position
            teleportationProvider.QueueTeleportRequest(telepoint);
        }



        private List<float> GetFOVatWD(float WD, Camera userCamera)
        {
            // Pythagoras theorem to calculate the distance
            List<float> holder = new List<float>();
            float vertical_fov = userCamera.fieldOfView;
            float fov_height = (WD * Mathf.Tan(vertical_fov * 0.5f)) * 2;
            float fov_width = userCamera.aspect * fov_height;     // Aspect ratio of the camera is width/height

            holder.Add(fov_height);
            holder.Add(fov_width);
            holder.Add(WD);

            Debug.Log($"FOV at WD {WD} is {fov_width} and {fov_height}");

            return holder;
        }

        public static Vector2 GetPixelSize(RectTransform rectTransform, Vector2 size)
        {
            Vector3 scale = rectTransform.lossyScale;

            // World size
            Vector2 worldSize = new Vector2(size.x * scale.x, size.y * scale.y);

            // Convert to pixel size (if under a Canvas)
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
            {
                float scaleFactor = canvas.scaleFactor;
                worldSize *= scaleFactor;
            }

            worldSize *= 100;

            Debug.Log($"World size in pixels is {worldSize.x} and {worldSize.y} and scalefactor is {canvas.scaleFactor} and scale is {scale}");

            return worldSize;
        }


        private (float, float) ResizeImgtobewithinFOV(float WD, Camera userCamera)
        {
            // Get the FOV at the panel height
            List<float> outputs = GetFOVatWD(WD, userCamera);
            
            newHeight = outputs[0]; // Height
            float newWidth = outputs[1]; // Width
            Debug.Log($"FOV at WD adjusted {WD} is {newWidth} and {newHeight}");


            // Reduce image size whilst keeping the image aspect ratio
            float aspect_ratio = width / height;

            Debug.Log($"Aspect ratio of the image is {aspect_ratio} = {width}/{height}");
            // Adjust the dimensions to maintain the aspect ratio
            newHeight = newWidth / aspect_ratio; // Aspect ratio is 1, so newWidth = newHeight

            Debug.Log($"New size of img: {newWidth} {newHeight}");
            // Transform rect size to pixels

            Vector3 local_origin = transform.TransformVector(new Vector3(0, 0, 0));
            Vector3 local_size = transform.TransformVector(new Vector3(newWidth, newHeight, 0));

            Debug.Log($"Local origin is {local_origin} and local size is {local_size}");

            Vector3 screen_origin = userCamera.WorldToScreenPoint(local_origin);
            Vector3 screen_size = userCamera.WorldToScreenPoint(local_size);

            // Calculate the size in pixels
            float newWidth_pixels = Mathf.Abs(screen_size.x - screen_origin.x);
            float newHeight_pixels = Mathf.Abs(screen_size.y - screen_origin.y);

            Debug.Log($"New size of img: {newWidth} {newHeight} and in pixels: {newWidth_pixels} {newHeight_pixels}");


            // Get pixel size of the image
            Vector2 pixelSize = GetPixelSize(GetComponent<RectTransform>(), new Vector2(newWidth, newHeight));

            // Updated 
            newWidth_pixels = pixelSize.x;
            newHeight_pixels = pixelSize.y;

            Debug.Log($"Updated new size of img: {newWidth} {newHeight} and in pixels: {newWidth_pixels} {newHeight_pixels}");

            return new(newWidth, newHeight);
        }

        private void PositionWholeImage(Transform Panel, Camera userCamera)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            // Set at the centre of the screen
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            transform.localPosition = Vector3.zero;

            // Set position of the image at same x position as panel and at the same z position as panel but at y position of panel + height of the image
            float y = Panel.GetComponent<RectTransform>().sizeDelta.y;

            // Hypotenuse
            float hypotenuse = Mathf.Sqrt(Mathf.Pow(y, 2) + Mathf.Pow(_interactableImageStack.raycast_distance, 2));

            // Set size of the image
            var img_size = ResizeImgtobewithinFOV(hypotenuse, userCamera);

            // Resize rectTransform to fit the canvas
            rectTransform.sizeDelta = new UnityEngine.Vector2(img_size.Item1, img_size.Item2);
            Debug.Log($"Set size delta to {img_size}");

            //rectTransform.localScale = new UnityEngine.Vector3(1f / canvasScaleFactor, 1f / canvasScaleFactor, 1f);

            // Compute logbase10 of the image size
            /*
            int log10_width = (int)Mathf.Log10(width);
            int log10_height = (int)Mathf.Log10(height);
            // Assert that the logs are equal 
            if (Mathf.Equals(log10_width, log10_height))
            {
                Debug.Log("The image is square");
                int log_10 = (int)log10_width; // Use either log10_width or log10_height as they are equal

                int downsample_factor = log_10 > 1 ? log_10 - 1 : 1; // Downsample factor is log10 - 1, but at least 1
                Debug.Log($"Downsample factor is {downsample_factor}");

                newWidth = width / downsample_factor;
                newHeight = height / downsample_factor;

                Debug.Log($"New size of img: {newWidth} {newHeight}");

                // Set size of the image
                rectTransform.sizeDelta = new UnityEngine.Vector2(newWidth, newHeight);

            }
            else
            {
                Debug.Log($"The image is not square: {log10_width} and {log10_height}");
            } */

            // Set angle of the image to panel at 90°
            rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y + y, rectTransform.position.z);

            // Last incase, it affects positioning
            rectTransform.localRotation = UnityEngine.Quaternion.Euler(90, 0, 0);





        }

        private Texture2D LoadTexture(string img_path, float width = 0, float height = 0)
        {
            // If width and height are not provided, get them from the image file
            if (width == 0 || height == 0)
            {
                (width, height) = GetDimensions(img_path);
            }

            // Load the image from the specified path

            //Texture2D texture = Resources.Load<Texture2D>(Path.Combine("MicroNuclAI", name));
            // TODO: Find a way to also load tif images, currently only png and jpg are supported
            byte[] fileData = File.ReadAllBytes(img_path);
            //(float width, float height) = GetDimensions(img_path);
            Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, true);
            texture.LoadImage(fileData);
            return texture;
        }

        public (float width, float height) GetDimensions(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                stream.Seek(8, SeekOrigin.Begin);

                byte[] chunkLength = new byte[4];
                byte[] chunkType = new byte[4];
                stream.Read(chunkLength, 0, 4);
                stream.Read(chunkType, 0, 4);

                string type = System.Text.Encoding.ASCII.GetString(chunkType);
                if (type != "IHDR")
                {
                    throw new Exception("IHDR chunk not found");
                }

                byte[] dimensions = new byte[8];
                stream.Read(dimensions, 0, 8);

                width = BitConverter.ToInt32(new byte[] { dimensions[3], dimensions[2], dimensions[1], dimensions[0] }, 0);
                height = BitConverter.ToInt32(new byte[] { dimensions[7], dimensions[6], dimensions[5], dimensions[4] }, 0);

                return (width, height);
            }
        }

        private void PositionImagetitle(Transform title)
        {
            // Create Title
            TMP_Text tmpText = title.GetComponent<TextMeshProUGUI>();

            // Position -90° from whole image, this causes its axis to rotated too
            title.GetComponent<RectTransform>().localRotation = UnityEngine.Quaternion.Euler(270, 0, 0);


            // Position at whole image height distance in z axis.
            title.GetComponent<RectTransform>().position = new UnityEngine.Vector3(transform.position.x, transform.position.y,
            transform.position.z + (height / 2) * 1.5f);

            tmpText.text = "Whole image";
            tmpText.fontSize = newHeight * 0.1f;
            tmpText.alignment = TextAlignmentOptions.Center;

            // Set all text margins to 0
            tmpText.margin = new UnityEngine.Vector4(0, 0, 0, 0);

            ContentSizeFitter fitter = title.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void OnEnable()
        {
            // Enable the teleport action
            TeleportActionMap.action.Enable();
        }

        private void OnDisable()
        {
            // Disable the teleport action
            TeleportActionMap.action.Disable();
        }

        public void Return2Start()
        {
            // Execute the function once the action is triggered
            Teleport2Originalposition();
        }
    }
}