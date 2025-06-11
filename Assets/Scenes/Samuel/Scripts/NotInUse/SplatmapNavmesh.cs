using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class SplatmapNavmesh : MonoBehaviour
{
    public Terrain terrain;
    public int textureIndex = 1;
    public float textureThreshold = 0.1f;
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

        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, width, height);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        int[,] vertexIndices = new int[width, height];

        int vertexCount = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float textureStrength = splatmapData[x, y, textureIndex];

                if (textureStrength > textureThreshold)
                {

                    Vector3 worldPos = terrain.transform.position;
                    float worldX = worldPos.x + ((float)x / width) * terrainData.size.x;
                    float worldZ = worldPos.z + ((float)y / height) * terrainData.size.z;
                    float worldY = terrain.terrainData.GetHeight(x, y);

                    Vector3 vertexPosition = new Vector3(worldX, worldY, worldZ);
                    vertices.Add(vertexPosition);

                    uv.Add(new Vector2((float)x / width, (float)y / height));

                    vertexIndices[x, y] = vertexCount;
                    vertexCount++;
                }
                else
                {
                    vertexIndices[x, y] = -1;
                }
            }
        }

        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                int current = vertexIndices[x, y];
                int nextX = vertexIndices[x + 1, y];
                int nextY = vertexIndices[x, y + 1];
                int nextXY = vertexIndices[x + 1, y + 1];

                if (current != -1 && nextX != -1 && nextY != -1 && nextXY != -1)
                {
                    triangles.Add(current);
                    triangles.Add(nextX);
                    triangles.Add(nextXY);

                    triangles.Add(current);
                    triangles.Add(nextXY);
                    triangles.Add(nextY);
                }
            }
        }

        generatedMesh = new Mesh();
        generatedMesh.vertices = vertices.ToArray();
        generatedMesh.triangles = triangles.ToArray();
        generatedMesh.uv = uv.ToArray();

        generatedMesh.RecalculateNormals();

        GameObject meshObject = new GameObject("GeneratedMesh");
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = generatedMesh;

        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }
}




