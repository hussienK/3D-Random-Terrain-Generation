/* Hussien Kenaan
 * <20 - 7 - 2022>
 * 
 * this static class is called to generated a mesh*/


using UnityEngine;
public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplyier, AnimationCurve _heightCurve, int levelOfDetail)
    {
        AnimationCurve animationCurve = new AnimationCurve(_heightCurve.keys);
        //get the width and hight of this mesh
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        //position the mesh corrcetly
        float topLeftX = (width - 1) / -2;
        float topLeftZ = (height - 1) / 2;
        
        //create mesh data to fill with correct values
        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        int meshSemplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
        int VerticesPerLine = (width - 1) / meshSemplificationIncrement + 1;

        //reference google for more info on 3d rendering//

        //loop through each point on map depending on provided level of details
        for (int y = 0; y < height; y += meshSemplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSemplificationIncrement)
            {
                //assign a vertex width coordinated of point
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, animationCurve.Evaluate(heightMap[x, y]) * heightMultiplyier , topLeftZ - y);
                //create a uv for current point
                meshData.uvs[vertexIndex] = new Vector2(x /(float)width, y / (float)height);

                //if should create a new triangle, create one
                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + VerticesPerLine + 1, vertexIndex + VerticesPerLine);
                    meshData.AddTriangle(vertexIndex + VerticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        //return the generated mesh data
        return meshData;
    }
}

public class MeshData{
    //create variables
    public Vector3[] vertices;
    public int[] triangels;
    public Vector2[] uvs;

    int triangleIndex;

    //class constructor
    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangels = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    //create a rendering triangle
    public void AddTriangle(int a, int b, int c)
    {
        triangels[triangleIndex] = a;
        triangels[triangleIndex + 1] = b;
        triangels[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    //create the actial mesh using the data and return it
    public Mesh createMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangels;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

}