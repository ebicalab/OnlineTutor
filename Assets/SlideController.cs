using UnityEngine;
using System.IO;

public class SlideController : MonoBehaviour
{
    public GameObject projector;

    //the image must be 1024x2048(rotated to the right by 90 degrees)
    public void SetImage(string path)
    {
        if (projector == null)
        {
            Debug.LogError("Projector not assigned.");
            return;
        }

        Texture2D texture = LoadTexture(path);
        if (texture != null)
        {
            

            Material material = new Material(Shader.Find("HDRP/Lit"));
            material.mainTexture = texture;

            var renderer = projector.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var materials = renderer.materials;
                if (materials.Length > 1)
                {
                    materials[1] = material;
                    renderer.materials = materials;
                }
                else
                {
                    Debug.LogError("Projector material array does not have enough elements.");
                }
            }
            else
            {
                Debug.LogError("Projector does not have a MeshRenderer component.");
            }
        }
    }

    Texture2D LoadTexture(string filePath)
    {
        // Load image file data
        byte[] fileData = File.ReadAllBytes(filePath);
        // Create a new Texture2D
        Texture2D texture = new Texture2D(2, 2);
        // Load image data into the texture
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        else
        {
            Debug.LogError("Failed to load texture from " + filePath);
            return null;
        }
    }


    public void DisableProjector()
        {
            if (projector != null)
            {
                var renderer = projector.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var materials = renderer.materials;
                    materials[1].SetColor("_BaseColor", Color.clear); // Set material color to clear (invisible)
                    renderer.materials = materials;
                }
                else
                {
                    Debug.LogError("Projector does not have a MeshRenderer component.");
                }
            }
        }
}
