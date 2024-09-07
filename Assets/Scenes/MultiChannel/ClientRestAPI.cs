// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.XR.Interaction.Toolkit;





public class ClientRestAPI : MonoBehaviour

{

    public static string start_endpoint = "http://127.0.0.1:5000//v1";
    public GameObject Channel1;
    public List<Texture> Textures;
    private List<GameObject> Channels = new List<GameObject>();



 IEnumerator  GetImgs()
{ 
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
            var json = www.downloadHandler.text; //Should return UTF string of Byte Data. Only reads into serialised objects
            LoadImages( json, Textures, Channels);
        }}
}

    private void LoadImages(string jsonData, List<Texture> Textures, List<GameObject> Channels)
    {


        // Or retrieve results as binary data
        var json = JObject.Parse(jsonData); //Should return UTF string of Byte Data. Only reads into serialised objects
        //Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
        //var data = JsonConvert.DeserializeObject<Dictionary<int, int>>(json)
        //Debug.Log(data.shape);
        //data.metadata.ForEach(Debug.Log);
        int h = (int)json["shape"][0];
        int w = (int)json["shape"][1];
        int length = h*w;
        //JArray pixelArray = (JArray)data["img"][0];

        //texture.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));

        List<int> channel_sizes = json.SelectToken("channel_sizes").ToObject<List<int>>();

        byte [][] imgs =  json.SelectToken("img").ToObject<byte [][]>();

        int count = imgs.Length;

        Work(channel_sizes, imgs, w, h, count, Textures, Channels);




    }

    public static IEnumerable<Texture> Work(List<int> channel_sizes, byte[][] imgs, int w, int h, int count,
    List<Texture> Textures, List<GameObject> Channels)
    {
        for (int i = 0; i < count; i++)
        {
            var n_channels = channel_sizes[count];
            byte[] channel = imgs[i];
            Texture tex = LoadTextures(n_channels, w, h, channel);
            Textures.Add(tex);
            CreateRawImage(tex, Channels);
            yield return tex;
        }
    }


    private static Texture LoadTextures(int n_channels, int w, int h, byte[] row)
    {
        if (n_channels == 1)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.Alpha8, false);
            tex.SetPixelData(row, 0);
            tex.Apply();
            return tex;

        }
        else
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixelData(row, 0);
            tex.Apply();
            return tex;

        }

        
    }

    private static void CreateRawImage(Texture tex, List<GameObject> Channels)
    {
        Channels.Add(new GameObject("RawImage"));
        RawImage rawImage = Channels[-1].AddComponent<RawImage>();
        rawImage.texture = tex;
    }
    void Start()


    { 

StartCoroutine(NapariTest());



        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



 IEnumerator  NapariTest()
{ 
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
            var json = JObject.Parse(www.downloadHandler.text); //Should return UTF string of Byte Data. Only reads into serialised objects
            //Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
            //var data = JsonConvert.DeserializeObject<Dictionary<int, int>>(json)
            //Debug.Log(data.shape);
            //data.metadata.ForEach(Debug.Log);
            int h = (int)json["shape"][0];
            int w = (int)json["shape"][1];
            int length = h*w;
            //JArray pixelArray = (JArray)data["img"][0];

            //texture.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));

            List<int> channel_sizes = json.SelectToken("channel_sizes").ToObject<List<int>>();

  

            byte [][] img =  json.SelectToken("img").ToObject<byte [][]>();

            int count = 0;
            foreach (byte[] row in img)
            {
                    
                    var n_channels = channel_sizes[count];
                     Debug.Log(string.Format("Data length: {0}", n_channels)); 
                    if (n_channels == 1)
                    {
                        Texture2D tex = new Texture2D(w, h, TextureFormat.R8, false);
                        tex.SetPixelData(row, 0);
                        tex.Apply();
                        Channel1.GetComponent<RawImage>().texture=tex; 

                    }
                    else
                    {
                        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                        tex.SetPixelData(row, 0);
                        tex.Apply();
                        Channel1.GetComponent<RawImage>().texture=tex; 

                    }

                    

                }



            

            }

        }
}



 IEnumerator  GetRequest()
{ 

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
            var tex2 = new Texture2D(w, h, TextureFormat.RGB48, false);
            tex2.SetPixelData(channel2_byte.ToArray(), 0);
            tex2.Apply();
            GetComponent<RawImage>().texture=tex2; 

            var tex1 = new Texture2D(w, h, TextureFormat.RGB48, false);
            tex1.SetPixelData(channel1_byte.ToArray(), 0);
            Debug.Log(string.Format("Byte array 1 lengths: {0}",channel1_byte.ToArray().Length)); 
            Debug.Log(string.Format("Byte array 2 lengths: {0}", channel2_byte.ToArray().Length)); 
            tex1.Apply();
            GetComponent<RawImage>().texture=tex1; 
        }
            }

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










    public bool IsSelectableBy(UnityEngine.XR.Interaction.Toolkit.IXRSelectInteractor interactor)
    {
        throw new NotImplementedException();
    }

    public Pose GetAttachPoseOnSelect(UnityEngine.XR.Interaction.Toolkit.IXRSelectInteractor interactor)
    {
        throw new NotImplementedException();
    }

    public Pose GetLocalAttachPoseOnSelect(UnityEngine.XR.Interaction.Toolkit.IXRSelectInteractor interactor)
    {
        throw new NotImplementedException();
    }

    public Transform GetAttachTransform(UnityEngine.XR.Interaction.Toolkit.IXRInteractor interactor)
    {
        throw new NotImplementedException();
    }

    public void OnRegistered(InteractableRegisteredEventArgs args)
    {
        throw new NotImplementedException();
    }

    public void OnUnregistered(InteractableUnregisteredEventArgs args)
    {
        throw new NotImplementedException();
    }

    public void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        throw new NotImplementedException();
    }

    public float GetDistanceSqrToInteractor(UnityEngine.XR.Interaction.Toolkit.IXRInteractor interactor)
    {
        throw new NotImplementedException();
    }


}
