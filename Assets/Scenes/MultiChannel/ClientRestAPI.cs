// See https://aka.ms/new-console-template for more information
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Linq;





public class ClientRestAPI : MonoBehaviour

{
    public static string start_endpoint = "http://127.0.0.1:5000//v1";
    GameObject Channel1;
    GameObject Channel2;

   
    
    //public Dictionary<string, object> data;


    //public static HttpClient client =  createHTTP();

    [Serializable]
    public class Data
    {
    public string dtype;
    public string path;
    public List<int> shape;
    public List<int> img;
    public List<string> metadata;
    }



    private static void createHTTP()
    { 
        
        /* var socketHandler = new SocketsHttpHandler
{
    MaxConnectionsPerServer =1
};
    return new (socketHandler);
    */
    UnityWebRequest client =  new UnityWebRequest();
    
    


    }

    

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

        Channel1 = transform.GetChild(0).gameObject;
        Channel2 = transform.GetChild(1).gameObject;
        Channel2.transform.position = Channel1.transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }





 IEnumerator  GetRequest()
{ //How to return json file ?? 
    //try
    //{
        //Send the GET request
        //Always catch network exceptions for async methods
        //Send the GET request asynchronously and retrieve the response as a string.

/*
        using (UnityWebRequest www = UnityWebRequest.Get($"{start_endpoint}/tiff_img"))
        {
            // Request and wait for the desired page.
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
            
            
            }
            */

        using (UnityWebRequest www = UnityWebRequest.Get($"{start_endpoint}/tiff_img"))
        {
            // Request and wait for the desired page.
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            // Show results as text
            Debug.Log("Worked");

            // Or retrieve results as binary data
            var data = JObject.Parse(www.downloadHandler.text); //Should return UTF string of Byte Data. Only reads into serialised objects
            //Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
            //var data = JsonConvert.DeserializeObject<Dictionary<int, int>>(json)
            //Debug.Log(data.shape);
            //data.metadata.ForEach(Debug.Log);
            int h = (int)data["shape"][0];
            int w = (int)data["shape"][1];
            int length = h*w;
            //JArray pixelArray = (JArray)data["img"][0];

            //texture.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));

            List<byte> channel1_byte = new List<byte>();
            List<byte> channel2_byte = new List<byte>();


            ushort [][] data2 =  data.SelectToken("img").ToObject<ushort [][]>();
            ushort [] channel1 = data2[0];
            ushort [] channel2 = data2[1];


        Debug.Log(string.Format("Data lengths: {0}, {0}", channel1.Length, channel2.Length)); 
        foreach (ushort value1 in channel1)
        {
            channel1_byte.AddRange(BitConverter.GetBytes(value1));
        }

                foreach (ushort value2 in channel2)
        {
            channel2_byte.AddRange(BitConverter.GetBytes(value2));
        }

        Debug.Log(string.Format("Data lengths: {0}, {0}", channel1_byte.Count, channel2_byte.Count)); 
/*
        int numValuesToShow = Mathf.Min(6, pixelArray.Length);
        for (int i = 0; i < numValuesToShow; i++)
        {
            Debug.Log(pixelArray[i]);
        }
                    int [][] data2 =  data.SelectToken("img").ToObject<int [][]>();
            int [] img_array = data2[0];
            Debug.Log(string.Format("H {0} and W {0}", h, w));
            int output = data2[0].Length;
            Debug.Log(string.Format("Length {0}", length)); 
            //Debug.Log(string.Format("Data: {0}", pixelArray)); 

    /*
            // Deserialize nested list into a 2D array
            Color[] pixelData = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                JArray row = (JArray)pixelArray[y];
                for (int x = 0; x < w; x++)
                {
                    JArray rgb = (JArray)row[x];
                    //Debug.Log(string.Format("For row {0} and column {0}: {0}", y, x, rgb));
                    int r = rgb[2].Value<int>();
                    int g = rgb[1].Value<int>();
                    int b = rgb[0].Value<int>();
                    pixelData[y * w + x] = new Color(r, g, b);
                }
            }*/

 /*

                byte[][] pixelData = int byte[pixelArray.Length];
                for (int i = 0; i < pixelArray.Length; i++)
                {
                    pixelData[i] = BitConverter.GetBytes(pixelArray[i]); 
                }*/

            var tex2 = new Texture2D(w, h, TextureFormat.RGB48, false);
            tex2.SetPixelData(channel2_byte.ToArray(), 0);
            tex2.Apply();
            Channel2.GetComponent<RawImage>().texture=tex2; 

            var tex1 = new Texture2D(w, h, TextureFormat.RGB48, false);
            tex1.SetPixelData(channel1_byte.ToArray(), 0);
            Debug.Log(string.Format("Byte array 1 lengths: {0}",channel1_byte.ToArray().Length)); 
            Debug.Log(string.Format("Byte array 2 lengths: {0}", channel2_byte.ToArray().Length)); 
            tex1.Apply();
            Channel1.GetComponent<RawImage>().texture=tex1; 




            

        }
            
            
            }

       /* UnityWebRequest client =  new UnityWebRequest();
        client.downloadHandler = new DownloadHandlerBuffer(uri); // Download handler

        yield return www.SendWebRequest(); */

        

/*
         HttpResponseMessage response = await client.GetAsync(uri); //#Straemed object
         //Debug.Log(string.Format("Response: {0}", response.Status));
         Debug.Log("Response: " + response.EnsureSuccessStatusCode().ToString());
         var output = await response.Content.ReadFromJsonAsync();
         Debug.Log(string.Format("Content: {0}" ,   output)); */
         

         // TODO How do I get json output and treat it as dictionary?????????
       /* 
        Dictionary<string, object> content = JsonUtility.FromJson<Dictionary<string, object>>(output);
            foreach (string key in content.Keys)
    {
        Debug.Log("Message: " + key.ToString());
    } */
    


        //return null;

/*
    }
    catch (Exception ex)
    {
        Debug.Log("Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message);
        return null;
    } */
}

IEnumerator PutRequest()
{ //How to return json file ?? 
    //try
   // {
    // Post the JSON and wait for a response.
    /*UnityWebRequest client =  new UnityWebRequest();
    UploadHandlerRaw uploader = new(content); //Upload handler
    // Sends header: "Content-Type: custom/content-type";
    uploader.contentType = "application/json";
    client.uploadHandler = uploader;*/
    string content = JsonConvert.SerializeObject(new{path = "/media/ibrahim/Extended Storage/cloud/Internship/IPBM/well_G_1_hela_gfp_hoechst.tif"});
    using (UnityWebRequest www = UnityWebRequest.Post($"{start_endpoint}/tiff_img", content, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }

     




     }

  //  catch (Exception ex)
   // {
        // Write out any exceptions.
  //      Debug.Log("Error: " + ex.HResult.ToString("X") + " Message: " + ex.ToString());
  //  } 

 /*   
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
    } */
//}

void GetImgs(string tiff_path)
{
        // Construct the JSON to post.




        //string jsonData ="{\"path\":\"/media/ibrahim/Extended Storage/cloud/Internship/IPBM/Dataset/LHA3_R5_tiny/input1/LHA3_R5_tiny_V01_Z-0.tif\"}";



        //StringContent content = new(JsonSerializer.ToJsonString(dict));

    //StringContent content = new( JsonSerializer.ToJsonString(new{path = tiff_path}), Encoding.UTF8, "application/json");

    string content = JsonConvert.SerializeObject(new{path = tiff_path});

    string endpoint = $"{start_endpoint}/tiff_img";

   /* await PutRequest(endpoint, content);

    await GetRequest(endpoint); //Works like a charm */

     StartCoroutine(PutRequest());

     StartCoroutine(GetRequest()); //Works like a charm

}

public  void Execute()
{

    // Construct the JSON to post.
    GetImgs("/media/ibrahim/Extended Storage/cloud/Internship/shapiro/greentest.tif");

    


}

}
