  /* Hussien Kenaan
 * <20 - 7 - 2022>
 * 
 * the class is responsible for rendering texture made by MapGenerator class*/

using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    //get the map renderer
    public Renderer textureRenderer;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    //draw the noise map on a plane
    public void DrawTexture(Texture2D texture)
    {
        //modify the current renderer to view generated texture
        textureRenderer.sharedMaterial.SetTexture("_MainTex", texture);
        //set map to appropriate scale
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
    //create the mesh for 3d rendering
    
    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        //create a mesh using generated data
        meshFilter.sharedMesh = meshData.createMesh();
        //set the texture for the created mesh
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
