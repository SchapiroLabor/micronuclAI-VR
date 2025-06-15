using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Debug = UnityEngine.Debug;
using System;
using Newtonsoft.Json;
using static SchapiroLabLog;
using System.Threading.Tasks;
using UnityEditor.PackageManager;




namespace NonGOSripts
{
    static class HelperFunctions
    {     // Compute 3x3 homography matrix using DLT

        public static void SetupAnchorsAndPivots(RectTransform rectTransform)
        {
            // Set the anchors and pivots of the Canvas
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        public static List<float> GetFOVatWD(float WD, Camera userCamera)
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

        public static List<float> GetFOVatNearClipping(Camera userCamera)
        {   // Must be near or else the child elements of canvas will not be visible

            // Pythagoras theorem to calculate the distance
            List<float> holder = new List<float>();
            float vertical_fov = userCamera.fieldOfView;
            float clipping_distance = userCamera.nearClipPlane;
            float fov_height = (clipping_distance * Mathf.Tan(vertical_fov * 0.5f)) * 2;
            float fov_width = userCamera.aspect * fov_height; // Aspect ratio of the camera is width/height

            holder.Add(fov_height);
            holder.Add(fov_width);
            holder.Add(clipping_distance);

            return holder;
        }


        public static Vector3 FacePlayer(float scaler)
        {
            // Face the player
            Vector3 cameraPosition = new Vector3(0, 0, 0);
            Vector3 cameraForward = new Vector3(0, 0, 1);

            return cameraPosition + cameraForward * scaler;
        }


        public static float LocalToWorldAxis(Transform transform, float localValue, string axis)
        {

            /// <summary>
            /// Converts a local-space float to world-space distance along a given axis.
            /// </summary>
            /// <param name="transform">The Transform to use</param>
            /// <param name="localValue">The value in local units</param>
            /// <param name="axis">"x", "y", or "z"</param>
            /// <returns>World-space magnitude along that axis</returns>
            Vector3 local = axis switch
            {
                "x" => new Vector3(localValue, 0f, 0f),
                "y" => new Vector3(0f, localValue, 0f),
                "z" => new Vector3(0f, 0f, localValue),
                _ => throw new System.ArgumentException("Axis must be 'x', 'y', or 'z'")
            };

            Vector3 worldOffset = transform.TransformVector(local);  // Only scale/rotation
            return worldOffset.magnitude * Mathf.Sign(localValue);
        }



        public static float SetFontSizeByWorldHeight(TMP_Text tmp, float targetWorldHeight)
        {   /// <summary>
            /// Sets the TMP fontSize so the text height in world space equals targetWorldHeight.
            /// </summary>
            // Get scale from local to world
            float worldScaleY = tmp.transform.lossyScale.y;

            // TMP uses pointSize relative to lineHeight
            float fontLineHeight = tmp.font.faceInfo.lineHeight;
            float pointSize = tmp.font.faceInfo.pointSize;

            // Compute fontSize needed to reach desired world height
            float fontSize = (targetWorldHeight / worldScaleY) * (pointSize / fontLineHeight);

            return fontSize;



        }


        public static GameObject CreateGameObject(string prefabPath)
        {
            // Create a new RawImage GameObject from the prefab
            GameObject instance = Resources.Load<GameObject>(prefabPath);

            return instance;
        }

        public static void CreateLoadingWidget(Transform parent, GameObject load_indicator, float textfontsize)
        {
            // Create a new RawImage GameObject from the prefab
            load_indicator.GetComponentInChildren<TextMeshProUGUI>().text = $"Please wait until loading ended";
            load_indicator.GetComponentInChildren<TextMeshProUGUI>().fontSize = SetFontSizeByWorldHeight(load_indicator.GetComponentInChildren<TMP_Text>(),
            textfontsize);
            load_indicator.transform.position = parent.position;
            // Set size to 1/3 of width of image with aspect ratio of 3:1
            load_indicator.GetComponent<RectTransform>().sizeDelta = new Vector2(10,
            3);
            load_indicator.SetActive(true);
        }




        /*
            private void read_csv_with_csharp(string csvFilePath)
            {

                // Reading the contents of the CSV file
                string csvData = File.ReadAllText(csvFilePath);

                // Split the data into lines based on newlines
                string[] lines = csvData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                // Loop through each line and process it
                for (int i = 0; i < lines.Length; i++)
                {
                    // Skip empty lines or lines without commas
                    if (string.IsNullOrWhiteSpace(lines[i]) || !lines[i].Contains(','))
                    {
                        continue;
                    }

                    // Split the line into values based on commas
                    string[] values = lines[i].Split(',');

                    // Ensure the correct number of values (5 expected)
                    if (values.Length != 5)
                    {
                        SchapiroLabLog.Log($"Skipping line {i + 1}: Incorrect number of values.");
                        continue;
                    }

                    // Try parsing the values to integers and skip if parsing fails
                    if (!int.TryParse(values[0], out int label) ||
                        !int.TryParse(values[3], out int x1) ||
                        !int.TryParse(values[4], out int x2) ||
                        !int.TryParse(values[1], out int y1) ||
                        !int.TryParse(values[2], out int y2))
                    {
                        SchapiroLabLog.Log($"Skipping line {i + 1}: Parsing error.");
                        continue;
                    }

                    else
                    {
                        data_dict.Add(new element
                        {
                            x_min = x1,
                            x_max = x2,
                            y_min = y1,
                            y_max = y2
                        });
                    }
                }
            }

            void Write2CSV(string data_dir, MicronucleiCounts micronucleiCounts)
            {

                string filename = "output.csv";

                // Create new directory if current one does not exists
                string results_dir = Path.Combine(data_dir, "results");
                if (!Directory.Exists(results_dir))
                {
                    Directory.CreateDirectory(results_dir);
                }

                string filePath = Path.Combine(results_dir, filename);

                micronucleiCounts.SaveToCSVcsharp(filePath);
            }
        */
    }
}