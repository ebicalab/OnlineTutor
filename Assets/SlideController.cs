using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;


public class SlideController : MonoBehaviour {
    public GameObject projector;
    public TMP_FontAsset font; // Assign this in the Inspector

    // Existing image setter
    public void SetImage(string path) {
        if (projector == null) {
            Debug.LogError("Projector not assigned.");
            return;
        }

        Texture2D texture = LoadTexture(path);
        if (texture != null) {
            Material material = new Material(Shader.Find("HDRP/Lit"));
            material.mainTexture = texture;

            var renderer = projector.GetComponent<MeshRenderer>();
            if (renderer != null) {
                var materials = renderer.materials;
                if (materials.Length > 1) {
                    materials[1] = material;
                    renderer.materials = materials;
                }
                else {
                    Debug.LogError("Projector material array does not have enough elements.");
                }
            }
            else {
                Debug.LogError("Projector does not have a MeshRenderer component.");
            }
        }
    }

    // Load texture from file (unchanged)
    private Texture2D LoadTexture(string filePath) {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData)) {
            return texture;
        }
        else {
            Debug.LogError("Failed to load texture from " + filePath);
            return null;
        }
    }

    public void DisableProjector() {
        if (projector != null) {
            var renderer = projector.GetComponent<MeshRenderer>();
            if (renderer != null) {
                var materials = renderer.materials;
                materials[1].SetColor("_BaseColor", Color.clear);
                renderer.materials = materials;
            }
            else {
                Debug.LogError("Projector does not have a MeshRenderer component.");
            }
        }
    }
}
