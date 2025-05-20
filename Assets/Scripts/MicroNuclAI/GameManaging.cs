using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameManaging", menuName = "ScriptableObjects/GameManaging")]
public class GameManaging : MonoBehaviour
{
    public string InputFolder = "";
    public string ImgPath = @"D:\OneDrive\Desktop\Internship\VR_schapiro\data\data\img.png";
    public string MaskPath = @"D:\OneDrive\Desktop\Internship\VR_schapiro\data\data\mask.tif";
    public string PythonExecutable = @"C:\Users\ibrah\AppData\Local\Microsoft\WindowsApps\python.exe";
}
