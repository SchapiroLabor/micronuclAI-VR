using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "GameManaging", menuName = "ScriptableObjects/GameManaging")]
// Learn about the above using: https://learn.unity.com/tutorial/introduction-to-scriptable-objects#5cf187b7edbc2a31a3b9b123

// Instantiating once should make it persist across scenes. To make it more 
public static class GameManaging
{

    public static string InputFolder { get; set; } = @"D:\OneDrive\Desktop\Internship\UniKlinikum\Schapiro\data\data";
    public static string ImgPath { get; set; } = @"D:\OneDrive\Desktop\Internship\UniKlinikum\Schapiro\data\data\img.ome.tif";
    public static string MaskPath { get; set; } = @"D:\OneDrive\Desktop\Internship\UniKlinikum\Schapiro\data\data\mask.tif";
    public static string python_exe { get; set; } = @"D:\OneDrive\Desktop\Internship\UniKlinikum\Schapiro\repos\micronuclAI-VR\Assets\venv\MNAIVR\Scripts\python.exe";

}
