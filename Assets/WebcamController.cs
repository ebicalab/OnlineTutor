using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Windows.WebCam;
using System.IO;

public class WebcamController : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;

    private Resolution cameraResolution;
    private Texture2D targetTexture;

    string folderPath;

    private void Awake()
    {
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
    }

    // Public method to start photo capture, can be called from other scripts
    public void StartPhotoCapture(string folder)
    {
        folderPath = folder;
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    private void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        Debug.Log("Created PhotoCapture Object");
        photoCaptureObject = captureObject;

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = targetTexture.width;
        c.cameraResolutionHeight = targetTexture.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        Debug.Log("Started Photo Capture Mode");
        TakePicture();
    }

    private void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        Debug.Log("Saved Picture To Disk!");

        // Stop the photo mode after taking the picture
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    private void TakePicture()
    {
        Debug.Log("Taking Picture...");

      
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filename = "CapturedImage.jpg";
        string filePath = Path.Combine(folderPath, filename);

        photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
    }

    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;

        Debug.Log("Captured images have been saved at the following path:");
        Debug.Log(Path.Combine(Application.dataPath, "CapturedPhotos"));
    }
}
