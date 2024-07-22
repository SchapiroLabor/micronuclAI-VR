using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class InteractableImageStack : MonoBehaviour
{
    public List<Texture2D> images;
    private List<string> imagePaths;
    private GameObject rawImagecurrent;
    public int current_img_indx = 0;
    public int N_image;
    private GameObject whole_img;
    private Texture2D whole_img_texture;

    private void Start()
    {
        // Set Canvas at acceptable viewing position
        setCanvasPosition();

        // Populate the list dedicated to image textures
        getImageTextures();

        // Get preloaded image
        rawImagecurrent = transform.Find("Image").gameObject;

        // Initialize the image
        init_current_img(rawImagecurrent, current_img_indx);

        create_and_display_whole_image();
    }

    private void setCanvasPosition()
    {
        float viewingDistance = CalculateViewingDistance(this.gameObject, 25);
        Vector3 canvas_position = Camera.main.transform.TransformPoint(Vector3.forward * viewingDistance);
        transform.position = canvas_position;
    }

    private float CalculateViewingDistance(GameObject instance, float visualAngle)
    {
        float Width = instance.GetComponent<RectTransform>().rect.width;
        float Height = instance.GetComponent<RectTransform>().rect.height;

        float objectSize = Mathf.Sqrt(Mathf.Pow(Width, 2) + Mathf.Pow(Height, 2));

        // Convert visual angle from degrees to radians
        float thetaRadians = visualAngle * Mathf.Deg2Rad;

        // Calculate viewing distance using the rearranged formula
        float viewingDistance = objectSize / (2f * Mathf.Tan(thetaRadians / 2f));

        return viewingDistance;
    }

    private void getImageTextures()
    {
        var ext = new List<string> { "jpg", "gif", "png" };
        imagePaths = Directory.EnumerateFiles(Application.dataPath + "/Resources/test_imgs", "*", SearchOption.AllDirectories).ToList();
        imagePaths = imagePaths.Where(path =>
        {
            string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            return ext.Contains(extension);
        }).ToList();

        foreach (string imagePath in imagePaths)
        {
            images.Add(Resources.Load<Texture2D>("test_imgs/" + Path.GetFileNameWithoutExtension(imagePath)));
        }

        N_image = images.Count;
    }

    public void init_current_img(GameObject rawImagecurrent, int current_img_indx)
    {
        if (rawImagecurrent == null)
        {
            rawImagecurrent = Instantiate(rawImagecurrent, transform);
        }

        float width = rawImagecurrent.GetComponent<RectTransform>().rect.width;
        float height = rawImagecurrent.GetComponent<RectTransform>().rect.height;
        rawImagecurrent.GetComponent<BoxCollider>().size = new Vector3(width, height, 0);
        rawImagecurrent.GetComponent<RawImage>().texture = images[current_img_indx];
        rawImagecurrent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("Patch {0}/{1}", current_img_indx + 1, N_image);
    }

    private void create_and_display_whole_image()
    {
        if (images.Count > 1)
        {
            whole_img = transform.Find("Whole Image").gameObject;

            if (whole_img == null)
            {
                string prefabPath = "Assets/Scenes/CIAnnotator/SubsequentImage.prefab";
                whole_img = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), transform.position, transform.rotation);
                whole_img.SetActive(true);
            }

            if (whole_img != null)
            {
                whole_img.transform.SetParent(transform);
                whole_img = set_texture2whole_img(whole_img);
                whole_image_title(whole_img);
            }
            else
            {
                Debug.Log("Whole image is null");
            }
        }
    }

    private GameObject set_texture2whole_img(GameObject whole_img)
    {
        whole_img_texture = LoadTexture();

        if (whole_img_texture == null)
        {
            Debug.Log("Whole image texture is null");
        }

        whole_img.GetComponent<RawImage>().texture = whole_img_texture;
        RectTransform rectTransform = whole_img.GetComponent<RawImage>().GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(whole_img_texture.width, whole_img_texture.height);
        whole_img.transform.position = new Vector3(70, 1, 80);

        return whole_img;
    }

    private Texture2D LoadTexture()
    {
        string path = "Assets/Resources/whole_img.png";
        (int width, int height) = GetDimensions(path);
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(width, height);
        texture.LoadImage(fileData);
        return texture;
    }

    public static (int width, int height) GetDimensions(string filePath)
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

            int width = BitConverter.ToInt32(new byte[] { dimensions[3], dimensions[2], dimensions[1], dimensions[0] }, 0);
            int height = BitConverter.ToInt32(new byte[] { dimensions[7], dimensions[6], dimensions[5], dimensions[4] }, 0);

            return (width, height);
        }
    }

    private void whole_image_title(GameObject whole_image)
    {
        TMP_Text tmpText = whole_image.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        tmpText.transform.localScale = new Vector3(1, 1, 1);
        tmpText.text = "Whole image";
        tmpText.fontSize = 600;
        tmpText.alignment = TextAlignmentOptions.TopGeoAligned;
    }
}
