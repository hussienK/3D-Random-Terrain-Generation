/* Hussien Kenaan
 * <20 - 7 - 2022>
 * 
 * the class to generate the map and creating all it's values using Noise static class, then create a texture depending on the color mode*/


using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public DrawMode drawMode;

    //map width and height
    public const int mapChunkSize = 241;
    [Range(0, 6)] public int EditorPreviewLevelOfDetail;
    //changes the zoom for the generate map
    public float noiseScale;

    //the level of details
    public int octaves;
    //how much each octave contributes to the overall shape
    [Range(0, 1)] public float persistance;
    //how much detail is added or removed at each octave
    public float lacunarity;

    //value to off to generate different map each time
    public int seed;
    //moving the nois
    public Vector2 offset;

    public bool autoUpdate;

    //list of avaliable regions
    public TerrainType[] regions;

    //variables for mesh creation
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    Queue<MapThreadingInfo<MapData>> mapDataInfoQeue = new Queue<MapThreadingInfo<MapData>>();
    Queue<MapThreadingInfo<MeshData>> meshDataInfoQeue = new Queue<MapThreadingInfo<MeshData>>();

    //Redraw the map when settings are changed in editor
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        //get display script and call the display function depending on render mode
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, EditorPreviewLevelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }

    //called to request map data generation, uses threading to avoid freezing main thread
    public void RequestMapData(Vector2 center ,Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }
    //calling the request mesh generation
    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod ,callback);
        };

        new Thread(threadStart).Start();
    }


    //the actual threading for creating the map data
    void MapDataThread(Vector2 center ,Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataInfoQeue)
        {
            mapDataInfoQeue.Enqueue(new MapThreadingInfo<MapData>(callback, mapData));
        }
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataInfoQeue)
        {
            meshDataInfoQeue.Enqueue(new MapThreadingInfo<MeshData>(callback, meshData));
        }
    }

    //handle the threading in order
    private void Update()
    {
        if (mapDataInfoQeue.Count > 0)
        {
            for (int i = 0; i < mapDataInfoQeue.Count; i++)
            {
                MapThreadingInfo<MapData> threadInfo = mapDataInfoQeue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataInfoQeue.Count > 0)
        {
            for (int i = 0; i < meshDataInfoQeue.Count; i++)
            {
                MapThreadingInfo<MeshData> threadInfo = meshDataInfoQeue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    //generate the Map data for creaeting a map
    public MapData GenerateMapData(Vector2 center)
    {
        //generate a noise map
        float[,] noiseMap = Noise.GenerateNoisMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + offset);

        //create a colorMap from the noise map generated
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        //use regions data to select current color
                        colorMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    //make sure values are viable when editing them
    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }

    struct MapThreadingInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadingInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }


    }
}
    //create a custome data structore to store terrains
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }

    //save the map data
    public struct MapData
    {
        public readonly float[,] heightMap;
        public readonly Color[] colorMap;

        public MapData(float[,] HeightMap, Color[] ColorMap)
        {
            this.heightMap = HeightMap;
            this.colorMap = ColorMap;
        }
    }
