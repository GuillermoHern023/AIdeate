// using UnityEngine;
// using System.IO;
// using SFB; // StandaloneFileBrowser

// public class FileUploadManager : MonoBehaviour
// {
//     public string uploadedContext = "";

//     public void UploadCSV()
//     {
//         var paths = StandaloneFileBrowser.OpenFilePanel("Open CSV", "", "csv", false);
//         if (paths.Length > 0 && File.Exists(paths[0]))
//         {
//             uploadedContext = File.ReadAllText(paths[0]);
//             Debug.Log("CSV loaded:\n" + uploadedContext);
//         }
//     }


// }


