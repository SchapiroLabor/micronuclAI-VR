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
            string path = ConfirmExistence(PythonPath_text);
            return path;
        }

        public string GetDataFolder()
        {
            // Windows appears to handle special characters in path names better than linux
            TMP_InputField ImagePath_text = ImagePath.GetComponent<TMP_InputField>();
            string path = ConfirmExistence(ImagePath_text);
            string val_path = ConfirmContentsinDataFolder(path, ImagePath_text);
            return val_path;
        }
        private string ConfirmContentsinDataFolder(string InputFolder, TMP_InputField inputfield)
        {
            string[] allfiles = Directory.GetFiles(InputFolder);

            // Confrim if whole image, mask, patch folder and bbox file exists
            if (!File.Exists(Path.Combine(InputFolder, "img.png")) ||
            !File.Exists(Path.Combine(InputFolder, "mask.tif")) ||
            !File.Exists(Path.Combine(InputFolder, "bbox.txt")))
            {
                inputfield.text = "";
                inputfield.placeholder.GetComponent<TextMeshProUGUI>().text =
                "Please try again, ensure img.png, mask, bbox.txt exist";
                return null;
            }

            if (!Directory.Exists(Path.Combine(InputFolder, "patches")) || Directory.GetFiles(Path.Combine(InputFolder, "patches")).Length == 0)
            {
                inputfield.text = "";
                inputfield.placeholder.GetComponent<TextMeshProUGUI>().text = "Please ensure folder with patches exist or is not empty";
                return null;
            }

            // Ensure that patches are of png format

            string[] patchFiles = Directory.GetFiles(Path.Combine(InputFolder, "patches"));
            bool hasNonPngPatch = patchFiles.Any(file => !file.EndsWith(".png"));

            if (hasNonPngPatch)
            {
                inputfield.text = "";
                inputfield.placeholder.GetComponent<TextMeshProUGUI>().text = "Please ensure patches are of png format";
                return null;
            }

            else

            { return InputFolder; }



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