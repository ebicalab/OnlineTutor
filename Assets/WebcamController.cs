using UnityEngine;
using System.IO;

public class WebcamController : MonoBehaviour
{
    private WebCamTexture webCamTexture;
    private string savePath;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            webCamTexture = new WebCamTexture(devices[0].name);
            webCamTexture.Play();
        }
        else
        {
            Debug.LogError("No webcam found.");
        }

        // Set the save path inside the project folder
        savePath = Path.Combine(Application.dataPath, "CapturedPhotos");
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    

    public string CapturePhoto()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            byte[] bytes = photo.EncodeToPNG();

            string fileName = $"photo_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(savePath, fileName);
            File.WriteAllBytes(filePath, bytes);

            Debug.Log($"Photo saved at {filePath}");
            return filePath;
        }
        else
        {
            Debug.Log("Webcam is not ready.");
            return null;
        }
    }

    void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }
        Delete();
    }

    public void Delete()
{
    if (Directory.Exists(savePath))
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(savePath);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    Debug.Log("All photos deleted.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Failed to delete photos: " + ex.Message);
                }
            }
}
}

