// See https://aka.ms/new-console-template for more information
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;


public class ClientRestAPI : MonoBehaviour
{
    
    string start_endpoint = "http://127.0.0.1:5000//v1";
    

    static async Task GetRequest(HttpClient client, string uri)
    { //How to return json file ?? 
        try
        {
            //Send the GET request
            // Always catch network exceptions for async methods
            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = await client.GetAsync(uri);
            httpResponse.EnsureSuccessStatusCode();
      

        }
        catch (Exception ex)
        {
            httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
        }
    }

    static async PutRequest(string uri, HttpStringContent content)
    { //How to return json file ?? 
        try
        {

            // Post the JSON and wait for a response.
            HttpResponseMessage httpResponseMessage = await client.PostAsync(
                uri,
                content);

            // Make sure the post succeeded, and write out the response.
            httpResponseMessage.EnsureSuccessStatusCode();
            var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            Debug.WriteLine(httpResponseBody);
        }
        catch (Exception ex)
        {
            // Write out any exceptions.
            Debug.WriteLine(ex);
        }
    }

    static async Task GetImgs(string tiff_path)
    {
        // Construct the JSON to post.
        HttpStringContent content = new HttpStringContent(
            string.Format("{ \"path\": \"{0}\" }", tiff_path),
            UnicodeEncoding.Utf8,
            "application/json");

        string endpoint = string.Format("{0}//tiff_img", start_endpoint);

        PutRequest(endpoint, content);

    }

}


