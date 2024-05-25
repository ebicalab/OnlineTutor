using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Windows.WebCam;

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

    public RawImage rawImage; // UI element to display the webcam feed
    public int webcamWidth = 1280;
    public int webcamHeight = 720;
    public int webcamFPS = 30;
    private WebCamTexture webCamTexture;

    private void Start()
    {
        // Start the webcam
        StartWebcam();
    }

    private void StartWebcam()
    {
        // Find the first available webcam
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            webCamTexture = new WebCamTexture(devices[0].name, webcamWidth, webcamHeight, webcamFPS);
            rawImage.texture = webCamTexture;
            rawImage.material.mainTexture = webCamTexture;
            webCamTexture.Play();
        }
        else
        {
            Debug.LogError("No webcam found");
        }
    }

    public void TakePhoto(System.Action<string> callback)
    {
        StartCoroutine(TakePhotoCoroutine(callback));
    }

    private IEnumerator TakePhotoCoroutine(System.Action<string> callback)
    {
        // Wait for the end of the frame to ensure the webcam texture is updated
        yield return new WaitForEndOfFrame();

        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        // Save the photo as a PNG file
        byte[] bytes = photo.EncodeToPNG();
        string filePath = Path.Combine(Application.persistentDataPath, "webcam_photo.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Photo saved at: " + filePath);

        // Call the callback with the file path
        callback?.Invoke(filePath);
    }

    private void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }
    }
}