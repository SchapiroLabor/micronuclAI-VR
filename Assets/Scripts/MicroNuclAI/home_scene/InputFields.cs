using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;


namespace HomeScene
{
    public class InputFields : MonoBehaviour
    {
        public Transform ImagePath;
        public Transform PythonPath;

        void Start()
        {
            // Initialize the input fields
            Initialize();
        }

        void Initialize()
        {

            // Initialize input widget for image path
            InitializeInputWidget4Imgs();

            // Initialize input widget for python executable path
            InitializeInputWidget4PythonExec();

        }


        Transform InitializeInputWidget4Imgs()
        {

            // Enable overflow
            ImagePath.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = true;

            // Set color of text to grey
            ImagePath.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;

            return ImagePath;
        }


        Transform InitializeInputWidget4PythonExec()
        {


            // Enable overflow
            PythonPath.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = true;

            // Set color of text to grey
            PythonPath.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;

            return PythonPath;
        }


        public string AddQuotesIfRequired(string path)
        {
            return !string.IsNullOrWhiteSpace(path) ?
                path.Contains(" ") && (!path.StartsWith("\"") && !path.EndsWith("\"")) ?
                    "\"" + path + "\"" : path :
                    string.Empty;
        }

        public string GetPythonExecutable()
        {
            // Windows appears to handle special characters in path names better than linux
            TMP_InputField PythonPath_text = PythonPath.GetComponent<TMP_InputField>();
            python_exe = ConfirmExistence(PythonPath_text);
            return python_exe;
        }

        public string GetDataFolder()
        {
            // Windows appears to handle special characters in path names better than linux
            TMP_InputField ImagePath_text = ImagePath.GetComponent<TMP_InputField>();
            string path = ConfirmExistence(ImagePath_text);
            inputfolder = ConfirmContentsinDataFolder(path, ImagePath_text);
            return inputfolder;
        }

        [Header("Add to config file")]
        public string MaskPath;
        [Header("Add to config file")]
        public string ImgPath;
        [Header("Add to config file")]
        private string python_exe; //gameManaging.PythonExecutable;
        [Header("Add to config file")]
        public string inputfolder;
        [Header("Add to config file")]
        private string ImgFileName = "img"; // Default image file name, please do not change this !!
        [Header("Add to config file")]
        private string MaskFileName = "mask"; // Default mask file name, please do not change this !!
        private string ConfirmContentsinDataFolder(string InputFolder, TMP_InputField inputfield)
        {
            string[] allfiles = Directory.GetFiles(InputFolder);

            // 
            foreach (string file in allfiles)
            {
                if (file.Contains(Path.Combine(InputFolder, ImgFileName)))
                {
                    ImgPath = file;
                    continue;
                }

                else if (file.Contains(Path.Combine(InputFolder, MaskFileName)))
                {
                    MaskPath = file;
                    continue;
                }

                else
                {
                    break;
                }

            }


            // Confrim if whole image, mask, patch folder and bbox file exists
            if (ImgPath is not null & MaskPath is not null)
            {
                { return InputFolder; }
            }
            else
            {
                inputfield.text = "";
                inputfield.placeholder.GetComponent<TextMeshProUGUI>().text =
                "Please try again, ensure whole well img and mask exist";
                return null;
            }



        }

        private static string ConfirmExistence(TMP_InputField inputfield)

        {

            // Windows appears to handle special characters in path names better than linux

            string path = inputfield.text;

            // Appears to be the only method that works to sanitize path names
            string full_path = Path.GetFullPath(path);


            // Confirm if path does not exists, if not clear and prompt user to enter again

            if (Directory.Exists(full_path) || File.Exists(full_path))

            { return full_path; }



            else
            {

                inputfield.text = "";
                inputfield.placeholder.GetComponent<TextMeshProUGUI>().text = "Please try again, path does not exist";

                return null;
            }


        }

    }
}