// See https://aka.ms/new-console-template for more information
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using System.Collections.Generic;


public class ClientRestAPI : MonoBehaviour

{
    
public static string start_endpoint = "http://127.0.0.1:5000//v1";

public static HttpClient client = new();

    private void setCanvasPosition()
    {
        float Width = GetComponent<RectTransform>().rect.width;
        float Height = GetComponent<RectTransform>().rect.height;
        float viewingDistance = CalculateViewingDistance(Mathf.Sqrt(Mathf.Pow(Width, 2) + Mathf.Pow(Height, 2)), 25);
        var canvas_position = Camera.main.transform.TransformPoint(Vector3.forward * viewingDistance);
        transform.position = canvas_position;

    }


    float CalculateViewingDistance(float objectSize, float visualAngle)
    {
        // Convert visual angle from degrees to radians
        float thetaRadians = visualAngle * Mathf.Deg2Rad;

        // Calculate viewing distance using the rearranged formula
        float viewingDistance = objectSize / (2f * Mathf.Tan(thetaRadians / 2f));

        return viewingDistance;
    }
    // Start is called before the first frame update




    // Start is called before the first frame update
    void Start()


    { setCanvasPosition();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

static async Task GetRequest(string uri)
{ //How to return json file ?? 
    try
    {
        //Send the GET request
        //Always catch network exceptions for async methods
        //Send the GET request asynchronously and retrieve the response as a string.
        HttpResponseMessage httpResponse = await client.GetAsync(uri);
        httpResponse.EnsureSuccessStatusCode();

        var jsonResponse = await httpResponse.Content.ReadAsStringAsync(); //Deserilisation is automated
        Debug.Log($"{jsonResponse}\n");


    }
    catch (Exception ex)
    {
        Debug.Log("Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message);
    }
}

static async Task PutRequest(string uri, StringContent content)
{ //How to return json file ?? 
    try
    {
    // Post the JSON and wait for a response.
    
    using HttpResponseMessage httpResponseMessage = await client.PostAsync(
        uri,
        content);

    // Make sure the post succeeded, and write out the response.
    httpResponseMessage.EnsureSuccessStatusCode();
    var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
    Debug.Log($"{httpResponseBody}\n");
    }
    catch (Exception ex)
    {
        // Write out any exceptions.
        Debug.Log("Error: " + ex.HResult.ToString("X") + " Message: " + ex.ToString());
    }
}

static async Task GetImgs(string tiff_path)
{
    // Construct the JSON to post.

    var dict = new Dictionary<string, string>();
    dict.Add("path", tiff_path);
    
    string jsonData = "{\"path\": \"$"{tiff_path}"\", \"age\": 30}";

    StringContent content = new StringContent (jsonData, Encoding.UTF8, "application/json");

    Debug.Log($"{content}\n");

    string endpoint = $"{start_endpoint}/tiff_img";

    await PutRequest(endpoint, content);

}

public async void Execute()
{

    // Construct the JSON to post.
    await GetImgs("cell_tinder//Assets//Images//dataset//image_34.tif");

    
   //await GetRequest($"{start_endpoint}/tiff_img"); //Works like a charm

}

}

