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
using static InteractableImageStack;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using static Logger;
// With a static directive, you can access the members of the class by using the class name itself

public class whole_image : MonoBehaviour
{
  
    private GameObject GameManager;
    private GameObject Arrow;
    public ClickNextImage CurrentImage_script;
    private float height;
    private float width;
    string working_dir;
    private Vector3 start_position; // Default distance to raycast from the camera, please do not change this !!!
    private Quaternion start_rotation;
    private float newWidth;
    private float newHeight;
    static List<element> data;
    public TeleportationProvider teleportationProvider;
    public InputActionReference TeleportActionMap;
    public string data_dir;
    public Logger customLogger;

    // All functions independet of other objects can be placed in even functions Awake, OnEnable, Start



    void Awake()
    {
        gameObject.name = "Image";

                        // Load the Game Manager
        if (GameManager == null)
        {
            // Load from path
            GameManager = Resources.Load<GameObject>(Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/SceneManager.prefab")));
        }
                // Get the img and python path
        data_dir = GameManager.GetComponent<GameManaging>().InputFolder;

        // Setup anchors and pivots
        RectTransform rectTransform = GetComponent<RectTransform>();
        SetupAnchorsAndPivots(rectTransform);

        // Set the anchors and pivots of the Canvas as sizeDelta requires absolute difference
        transform.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        transform.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Reduce Local Scale
        rectTransform.localScale = new UnityEngine.Vector3(1f, 1f, 1f);



        // Plays on main thread with pauses
        StartCoroutine(MyCoroutine(Path.Combine(data_dir, "img.png")));
    }

    void Update()
    {
        if (InteractableImageStack.data_dict != null)
        {
            data = InteractableImageStack.data_dict;
        }
    }

    private System.Collections.IEnumerator MyCoroutine(string img_path)
    {
        // Remove the call to WaitForWholeImage since it is not being used
        SetTextureOnWholeImage(img_path);
        yield return null; // Wait for the next frame
    }

    // Start is called before the first frame update
    public void Initialize(Transform parent, Transform Panel, Camera userCamera, ClickNextImage CurrentImage)
    {
        gameObject.transform.SetParent(parent);
        gameObject.SetActive(true);

        PositionWholeImage(parent, Panel, userCamera);
        // Should occure after the image is positioned as we are using world coordinates
        PositionImagetitle(transform.GetChild(0));

        // Initialize Arrow
        InitializeArrow();

        // STart position of the player
        start_position = Camera.main.transform.position;
        start_rotation = Camera.main.transform.rotation;

        //data_dict = ConvertOutputToDictionary(output);

        //TeleportActionMap.action.started += ctx => Return2Start();
    }


    public void ConfirmDataDict(List<element> data_dict)
    {
        if (data_dict == null)
        {
            Logger.Log("Data dictionary is null");
        }
        else
        {
            Logger.Log("Data dictionary is not null");
        }
    }

    private List<element> ConvertOutputToDictionary(string output)
    {
        List<element> result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<element>>(output);
        return result;
    }

    private void ColorPixelCluster(Rect pixelCluster, Color newColor)
    {
        // Clone the original texture
        Texture2D originalTexture = GetComponent<RawImage>().texture as Texture2D;

        // Apply the new color to the specified cluster of pixels
        for (int x = (int)pixelCluster.xMin; x < (int)pixelCluster.xMax; x++)
        {
            for (int y = (int)pixelCluster.yMin; y < (int)pixelCluster.yMax; y++)
            {
                originalTexture.SetPixel(x, y, newColor); // The x and y are swapped because the texture is rotated
            }
        }

        // Apply all changes to the texture
        originalTexture.Apply();
    }



    private Rect get_bbox_from_df(int patch_indx)
    {
        // Get the bounding box of the pixel cluster
        element patchData = data_dict[patch_indx];
        int width = patchData.x_max - patchData.x_min;
        int height = patchData.y_max - patchData.y_min;
        Rect bbox = new Rect(patchData.x_min, patchData.y_min, width, height);
        //Debug.Log($"The axes ranges are FOR X {patchData.x_min}, {patchData.x_max} ADN FOR Y {patchData.y_min}, {patchData.y_max}");
        return bbox;
    }

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
        Logger.Log($"The minimum pixel coordinates are {bbox.xMin}, {bbox.yMin}");

        // Get the pixel position of the patch
        UnityEngine.Vector2 mid_point_pixel = RescalePixelCoords(new UnityEngine.Vector2(((bbox.xMin + bbox.xMax) / 2),
        ((bbox.yMin + bbox.yMax) / 2)));

        Logger.Log($"The mid point pixel coordinates are {mid_point_pixel.x}, {mid_point_pixel.y}");

        return mid_point_pixel;
    }

    public void current_cell_bbox(int patch_indx)
    {
        if (patch_indx < data_dict.Count)
        {
            // Get the bounding box of the pixel cluster
            Rect bbox = get_bbox_from_df(patch_indx);

            // Color the pixel cluster
            ColorPixelCluster(bbox, Color.red);
        }
    }

    public void DisplayPatch()
    {
        //Debug.Log("Displaying patch: " + CurrentImage_script.current_img_indx.ToString());

        try
        {
        int indx = CurrentImage_script.current_img_indx;

        if (indx < data_dict.Count)
        {
            // Get the bounding box of the pixel cluster
            Rect bbox = get_bbox_from_df(indx);

            // Color the pixel cluster
            //ColorPixelCluster(bbox, Color.red);
            PositionArrow(bbox);

            //MovePlayer2PixelPosition(bbox);
        }


        // Log maximum and minimum local position
        if (!Arrow.activeSelf)
        {
            Arrow.SetActive(true);
        }

        }
        catch (Exception e)
        {
            Logger.Log($"Error: {e.Message}");
        }


    }

    private Vector3 Pixel2UnityCoord(UnityEngine.Vector2 pixel_coords, bool child = false)
    {
        Vector3 local_midpoint = Vector3.zero;

        if (child is false)
        {
            // Transform local midpoint to world coordinates
            Vector3 mid_point_image = transform.TransformPoint(local_midpoint);
            // Traverse by half the width and height of the image
            UnityEngine.Vector3 coords = mid_point_image - new UnityEngine.Vector3(newWidth / 2, mid_point_image.y, newHeight / 2) +
            new UnityEngine.Vector3(pixel_coords.x, transform.position.y, pixel_coords.y);
            Logger.Log($"Pixel to world coordinates are {pixel_coords} and {coords}");
            return coords;
        }
        else
        {
            // Traverse by half the width and height of the image
            UnityEngine.Vector3 coords = local_midpoint - new UnityEngine.Vector3(newWidth / 2, newHeight / 2, 0) +
            new UnityEngine.Vector3(pixel_coords.x, pixel_coords.y, 0);

            Logger.Log($"Pixel to local coordinates are {pixel_coords} and {coords}");

            return coords;
        }
    }

    private void MovePlayer2PixelPosition(Rect bbox)
    {
        // Get the mid point of the patch
        UnityEngine.Vector2 mid_point_pixel = GetPatchMidPoint(bbox);

        // Get the mid point in world coordinates
        UnityEngine.Vector3 mid_point_world = Pixel2UnityCoord(mid_point_pixel) - new UnityEngine.Vector3(0, 0.5f, 0.5f);

        // Create Teleportation request
        TeleportRequest telepoint = new TeleportRequest();
        telepoint.destinationPosition = mid_point_world;
        telepoint.destinationRotation = Quaternion.Euler(0, 0, 0);

        // Move the player to the patch position
        teleportationProvider.QueueTeleportRequest(telepoint);
    }

    private void InitializeArrow()
    {
        Arrow = transform.GetChild(1).gameObject;

        if (Arrow == null)
        {
            string prefabPath = Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/Arrow.prefab"));
            Arrow = CreateGameObject(transform, prefabPath, transform);
        }

        Arrow.SetActive(false);

        // Square root area of image
        Arrow.transform.localScale = new UnityEngine.Vector3(3, 6, 1);

        // Get the rotation in 150° Angle
        //Arrow.transform.rotation = UnityEngine.Quaternion.Euler(90, 180, 0);
    }

    private void PositionArrow(Rect bbox)
    {
        UnityEngine.Vector2 pixel_coords = GetPatchMidPoint(bbox);
        Vector3 local_coords = Pixel2UnityCoord(pixel_coords, true);

        Arrow.transform.localRotation = UnityEngine.Quaternion.Euler(270, 0, 0);
        Arrow.transform.localPosition = new UnityEngine.Vector3(local_coords.x, local_coords.y, local_coords.z);
        //Arrow.transform.position = new UnityEngine.Vector3(0, 0, 5);

        // Log maximum and minimum local position
        if (!Arrow.activeSelf)
        {
            Arrow.SetActive(true);
        }

        //Arrow.transform.localRotation = UnityEngine.Quaternion.Euler(295, 0, -25);
    }

    private void SetTextureOnWholeImage(string img_path)
    {
        Texture2D whole_img_texture = LoadTexture(img_path);

        if (whole_img_texture == null)
        {
            Logger.Log("Whole image texture is null");
        }

        // Size delta must be explicitly matched to the size of the image
        GetComponent<RawImage>().texture = whole_img_texture;
        RectTransform rectTransform = GetComponent<RawImage>().GetComponent<RectTransform>();
        rectTransform.sizeDelta = new UnityEngine.Vector2(whole_img_texture.width, whole_img_texture.height);
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

        return holder;
    }

    private void ResizeImgtobewithinFOV(float WD, Camera userCamera)
    {
        // Get the FOV at the panel height
        List<float> outputs = GetFOVatWD(WD, userCamera);
        newWidth = outputs[0] * 2f; // Width
        newHeight = outputs[1] * 2f; // Height

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
    }

    private void PositionWholeImage(Transform CurrentImage, Transform Panel, Camera userCamera)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        // Set at the centre of the screen
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        transform.localPosition = Vector3.zero;

        // Set position of the image at same x position as panel and at the same z position as panel but at y position of panel + height of the image
        float y = Panel.GetComponent<RectTransform>().sizeDelta.y;

        // Set size of the image
        ResizeImgtobewithinFOV((y + transform.position.z) / 2, userCamera);

        // Set angle of the image to panel at 90°
        rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y + y, rectTransform.position.z);

        // Last incase, it affects positioning
        rectTransform.localRotation = UnityEngine.Quaternion.Euler(90, 0, 0);
    }

    private Texture2D LoadTexture(string img_path)
    {
        //Texture2D texture = Resources.Load<Texture2D>(Path.Combine("MicroNuclAI", name));
        byte[] fileData = File.ReadAllBytes(img_path);
        (float width, float height) = GetDimensions(img_path);
        Logger.Log($"Size of img: {width} {height}");
        Texture2D texture = new Texture2D((int)width, (int)height);
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

        float d_height = GetComponent<RectTransform>().rect.height;
        float d_width = GetComponent<RectTransform>().rect.width;

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
        teleportationProvider.QueueTeleportRequest(new TeleportRequest()
        {
            destinationPosition = start_position,
            destinationRotation = start_rotation
        });
    }
}
