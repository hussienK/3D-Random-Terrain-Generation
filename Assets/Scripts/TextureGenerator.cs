/* Hussien Kenaan
 * <21 - 7 - 2022>
 * 
 * the class is responsible for creating textures for displaying*/
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    //create a color texture, also used to apply settings for noise rendering
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        //create a texture
        Texture2D texture = new Texture2D(width, height);
        //set settings
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        //apply changes and return
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        //get map length and width
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        //create a new texture
        Texture2D texture = new Texture2D(width, height);

        //create a new colot map
        Color[] colorMap = new Color[width * height];
        //turn the noise map into a color map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }
        //modify the texture to become the color map and apply settings through previous function
        return TextureFromColorMap(colorMap, width, height);
    }
}
