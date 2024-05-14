// See https://aka.ms/new-console-template for more information
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Palmmedia.ReportGenerator.Core.Common;
using System.Runtime.Serialization.Json;
using Unity.VisualScripting;
using System.Linq;

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

    private static async Task<Dictionary<string, object>> GetRequest(string uri)
{ //How to return json file ?? 
    try
    {
        //Send the GET request
        //Always catch network exceptions for async methods
        //Send the GET request asynchronously and retrieve the response as a string.

         HttpResponseMessage response = await client.GetAsync(uri);
         Debug.Log("Response: " + response.EnsureSuccessStatusCode().ToString());
         //Debug.Log("Content: " + response.Content.ReadAsStringAsync());

         // TODO How do I get json output and treat it as dictionary?????????
        
        Dictionary<string, object> content = JsonUtility.FromJson<Dictionary<string, object>>(response.Content.Serialize().json);
            foreach (string key in content.Keys)
    {
        Debug.Log("Message: " + key.ToString());
    }


        

        return content;


    }
    catch (Exception ex)
    {
        Debug.Log("Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message);
        return null;
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




        //string jsonData ="{\"path\":\"/media/ibrahim/Extended Storage/cloud/Internship/IPBM/Dataset/LHA3_R5_tiny/input1/LHA3_R5_tiny_V01_Z-0.tif\"}";



        //StringContent content = new(JsonSerializer.ToJsonString(dict));

    StringContent content = new( JsonSerializer.ToJsonString(new{path = tiff_path}), Encoding.UTF8, "application/json");

    string endpoint = $"{start_endpoint}/tiff_img";

    await PutRequest(endpoint, content);

    await GetRequest(endpoint); //Works like a charm




}

public async void Execute()
{

    // Construct the JSON to post.
    await GetImgs("/media/ibrahim/Extended Storage/cloud/Internship/IPBM/Dataset/LHA3_R5_tiny/input1/LHA3_R5_tiny_V01_Z-0.tif");

    


}

}
