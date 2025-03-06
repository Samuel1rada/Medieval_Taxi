using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using System.IO;


public class SplatmapNavmesh : MonoBehaviour
{
    public Terrain terrain;
    public int textureIndex = 1; // The index of the texture to base the mesh generation on
    public float textureThreshold = 0.1f; // Minimum texture strength for inclusion in the mesh
    private Mesh generatedMesh;
    /*public string savePath = Application.dataPath + "/Splatmap.png ;*/
    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain not assigned!");
            return;
        }

        GenerateMeshFromSplatmap();
    }

    void GenerateMeshFromSplatmap()
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        // Get the splatmap data (the strength of the textures at each pixel)
        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, width, height);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>(); // Optional: For adding texture coordinates

        // To create a grid of vertices, we need to track where they are
        int[,] vertexIndices = new int[width, height];

        int vertexCount = 0;
        // Go through the splatmap and create vertices where the texture strength exceeds the threshold
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float textureStrength = splatmapData[x, y, textureIndex];

                if (textureStrength > textureThreshold)
                {
                    // Convert terrain pixel to world position
                    Vector3 worldPos = terrain.transform.position;
                    float worldX = worldPos.x + ((float)x / width) * terrainData.size.x;
                    float worldZ = worldPos.z + ((float)y / height) * terrainData.size.z;
                    float worldY = terrain.terrainData.GetHeight(x, y); // Get the height of the terrain at this point

                    Vector3 vertexPosition = new Vector3(worldX, worldY, worldZ);
                    vertices.Add(vertexPosition);

                    // Optionally, store UV coordinates
                    uv.Add(new Vector2((float)x / width, (float)y / height));

                    vertexIndices[x, y] = vertexCount; // Assign index to the vertex
                    vertexCount++;
                }
                else
                {
                    vertexIndices[x, y] = -1; // Mark as invalid if the texture doesn't meet threshold
                }
            }
        }

        // Now, we need to create the triangles. We will connect adjacent vertices.
        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                // We need to make sure we are working with valid vertices
                int current = vertexIndices[x, y];
                int nextX = vertexIndices[x + 1, y];
                int nextY = vertexIndices[x, y + 1];
                int nextXY = vertexIndices[x + 1, y + 1];

                // Make sure all four vertices are valid before making a triangle
                if (current != -1 && nextX != -1 && nextY != -1 && nextXY != -1)
                {
                    // Create two triangles for each square of vertices
                    triangles.Add(current);
                    triangles.Add(nextX);
                    triangles.Add(nextXY);

                    triangles.Add(current);
                    triangles.Add(nextXY);
                    triangles.Add(nextY);
                }
            }
        }

        // Create the mesh and assign the vertices and triangles
        generatedMesh = new Mesh();
        generatedMesh.vertices = vertices.ToArray();
        generatedMesh.triangles = triangles.ToArray();
        generatedMesh.uv = uv.ToArray(); // Apply UVs if needed

        // Optionally, calculate normals for proper lighting
        generatedMesh.RecalculateNormals();

        // Create a new GameObject to hold the generated mesh
        GameObject meshObject = new GameObject("GeneratedMesh");
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = generatedMesh;

        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard")); // Assign any material you want
    }

  /*  void GenerateSplatmapImage()
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        // Get the splatmap data (strength of each texture at each pixel)
        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, width, height);

        // Create a new Texture2D to store the generated splatmap image
        Texture2D splatmapImage = new Texture2D(width, height);

        // Loop through each pixel in the splatmap data and set the pixel color in the image
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // For each texture, we'll use a separate channel (R, G, B, A)
                // For example, use R for the first texture, G for the second, etc.
                Color pixelColor = new Color(
                    splatmapData[x, y, 0], // Red channel - first texture
                    splatmapData[x, y, 1], // Green channel - second texture
                    splatmapData[x, y, 2], // Blue channel - third texture
                    splatmapData[x, y, 3]  // Alpha channel - fourth texture (if exists)
                );

                // Set the pixel color in the image
                splatmapImage.SetPixel(x, y, pixelColor);
            }
        }

        // Apply the changes to the texture
        splatmapImage.Apply();

        // Save the image as a PNG file
        byte[] pngData = splatmapImage.EncodeToPNG();
        File.WriteAllBytes(savePath, pngData);

        Debug.Log("Splatmap image saved to: " + savePath);
    }*/
}




