/* Hussien Kenaan
 * <20 - 7 - 2022>
 * 
 * this script makes the world endlessly generate around viewer*/

using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    Vector2 viewerPositionOld;

    //set the maximum amount a player can see
    public static float maxViewDist;
    //an array of level of details for each ditance
    public LODInfo[] myDetailLevels;
    //this is a reference for the player
    public Transform viewer;

    public Material mapMaterial;

    //keeps track of the players position
    public static Vector2 viewerPosition;
    //consts to get from other script
    int chunkSize;
    int chunksVisibleViewDist;

    //
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    //keep track of terrain chunks updated to be visibil last frame
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    static MapGenerator mapGenerator;


    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDist = myDetailLevels[myDetailLevels.Length - 1].visibleDistanceThreshold;
        //get the chunk size and how many chunks player can see
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        //save the viewer position into static Vector2
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;

            UpdateVisibleChunks();
        }
    }


    //update the chunks to show the correct ones
    public void UpdateVisibleChunks()
    {
        //loop through chunks visible in previouse frame
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            //set all of them to invisible
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        //reset the chunks visible in previous frame
        terrainChunksVisibleLastUpdate.Clear();

        //get in which chunk the viewer is in right now
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);


        //start a maximum allowed view distance back and go to front maximum allowed
        for (int yOffset = -chunksVisibleViewDist; yOffset <= chunksVisibleViewDist; yOffset++)
        {
            for (int xOffset = -chunksVisibleViewDist; xOffset <= chunksVisibleViewDist; xOffset++)
            {
                //save current chunk coords in a vector2
                Vector2 viewedChunkCoords = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                //if the chunk has been discovered before, update it\s state
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoords))
                {
                    //update the current chunk
                    terrainChunkDictionary[viewedChunkCoords].UpdateTerrainChunk();

                    //if the chunk is now visible set it in last frame updated
                    if (terrainChunkDictionary[viewedChunkCoords].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoords]);
                    }
                }
                //if chunk haven't been discovered yet, create it
                else
                {
                    //create a new chunk and add to dictionary of discovered
                    terrainChunkDictionary.Add(viewedChunkCoords, new TerrainChunk(viewedChunkCoords, chunkSize, myDetailLevels, transform, mapMaterial));
                }
            }
        }
    }

    // a chunk class for managing and creating them
    public class TerrainChunk
    {
        Vector2 position;
        GameObject meshObject;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataRecieved;
        int previousLODIndex = -1;

        //class constructor
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            //determine position in word
            position = coord * size;
            //get the bounds of chunk for calculation
            bounds = new Bounds(position, Vector2.one * size);
            //get word positon with equal y
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            //create a new terrain GO
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            //set position correctly
            meshObject.transform.position = positionV3;
            //set to invisible, determine is should render later
            SetVisible(false);
            meshObject.transform.SetParent(parent);

            //request the creation of map data
            meshRenderer.material = material;
            mapGenerator.RequestMapData(position, OnMapDataRecieved);
            

            lodMeshes = new LODMesh[detailLevels.Length];

            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataRecieved);
        }

        //get a call back to request mesh generation when done generating map data
        void OnMapDataRecieved(MapData mapData)
        {
            this.mapData = mapData;
            mapDataRecieved = true;
            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }
        //when the mesh data is recieved, create a mesh
       
       

        //check if should be rendered
        public void UpdateTerrainChunk()
        {
            if (mapDataRecieved)
            {

                //get distance from player to closest point on map, update the visibility depending on wether it's in view ditance
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                
                bool visible = viewerDistanceFromNearestEdge <= maxViewDist;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMeshe = lodMeshes[lodIndex];
                        if (lodMeshe.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMeshe.mesh;
                        }
                        else if (!lodMeshe.hasRequestMesh)
                        {
                            lodMeshe.RequestMesh(mapData);
                        }
                    }
                }

                SetVisible(visible);
            }
        }

        //update visibilty function
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        //check visibility function
        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    //for fetching it's own mesh from mesh generator
    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallBack;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallBack = updateCallback;
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            mesh = meshData.createMesh();
            hasMesh = true;

            updateCallBack();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistanceThreshold;
    }
}
