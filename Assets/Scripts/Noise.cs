/* Hussien Kenaan
 * <20 - 7 - 2022>
 * 
 * a static class to be called for generating the noise map*/


using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoisMap(int mapWidth, int mapHeight,int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        //create a nested array that contains the level for the coordinate
        float[,] noiseMap = new float[mapWidth, mapHeight];
        
        //generate a random seed
        System.Random prng = new System.Random(seed);
        //create a list of vectors the size of octaves and add a unique offset to each one
        Vector2[] ocataveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            ocataveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        //make sure scale never goes bellow the min amount to function
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfheight = mapHeight / 2f;

        //loop through mapHeight and change values in it
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //reset the amplitude and frequency
                float amplitude = 1;
                float frequncy = 1;
                float noiseHeight = 0;

                //loop through each octave on the current pixel
                for (int i = 0; i < octaves; i++)
                {
                    //determine the sampleX and sampleY to give into PerlinNois generator
                    float sampleX = (x - halfWidth) / scale * frequncy + ocataveOffsets[i].x;
                    float sampleY = (y - halfheight) / scale * frequncy + ocataveOffsets[i].y;

                    //generate the perlin value 
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    //determine the current height using the generate value
                    noiseHeight += perlinValue * amplitude;

                    //adjust next value according to current
                    amplitude *= persistance;
                    frequncy *= lacunarity;
                }

                //store max and min height
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                //apply the found heigh to the map
                noiseMap[x, y] = noiseHeight;
            }
        }

        //limit the map height
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
            }
        }

        //return the generate map
        return noiseMap;
    }
}
