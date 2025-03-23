using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Assertions;

public class LL_TileDataStruct
{
    public LL_TileDataStruct(Vector3Int input_v, float input_h)
    {
        pos = input_v;
        heuristic = input_h;
    }

    public Vector3Int pos;

    public float heuristic;

    public LL_TileDataStruct childTile;
}

public class MapBuilder : MonoBehaviour
{

    public static MapBuilder instance;

    public Vector3Int mapSize;

    public SceneryObject[,,] groundTiles;
    public MapVoxelData[,,] masterVoxelData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        BuildMap();

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BuildMap()
    {
        // Find and Tile Ground Tiles

        // Build and texture ground tiles
        groundTiles = new SceneryObject[mapSize.x + 1, mapSize.y + 1, mapSize.z + 1];
        masterVoxelData = new MapVoxelData[mapSize.x + 1, mapSize.y + 1, mapSize.z + 1];

        //for (int x = 0; x <= mapSize.x; x++)
        //{
        //    for (int y = 0; y <= mapSize.y; y++)
        //    {
        //        for (int z = 0; z <= mapSize.x; z++)
        //        {
        //            masterVoxelData[x, y, z] = new MapVoxelData();
        //        }
        //    }
        //}

        SceneryObject[] grabbedTiles = GameObject.FindObjectsOfType<SceneryObject>();

        // Find all ground tiles and sort them into the groundTiles array
        foreach (SceneryObject g in grabbedTiles)
        {
            if (g.entityType == EntityType.GroundTile)
            {
                Vector3Int pos = g.tilemapPosition;
                groundTiles[pos.x, pos.y, pos.z] = g;

                masterVoxelData[pos.x, pos.y, pos.z] = new MapVoxelData();


                masterVoxelData[pos.x, pos.y, pos.z].pathableStatus = PathableStatus.Pathable;
            }
            
        }

        foreach (SceneryObject g in grabbedTiles)
        {

            if (g.entityType == EntityType.SceneryObj)
            {
                Vector3Int pos = g.tilemapPosition;

                Vector3 offsetpos = g.tilemapPosition + g.transform.forward;
                Vector3Int pos2 = new Vector3Int((int)offsetpos.x, (int)offsetpos.y, (int)offsetpos.z);

                if (masterVoxelData[pos.x, pos.y, pos.z] == null)
                {
                    masterVoxelData[pos.x, pos.y, pos.z] = new MapVoxelData();
                }

                masterVoxelData[pos.x, pos.y, pos.z].sceneryObjects.Add(g);
                masterVoxelData[pos.x, pos.y, pos.z].pathableStatus = PathableStatus.Blocked_Pathable;

                if (g.transform.forward == new Vector3(1,0,0))
                {
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[2, 0] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[2, 1] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[2, 2] = true;

                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 0] = true;
                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 1] = true;
                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 2] = true;
                }
                else if (g.transform.forward == new Vector3(0, 0, 1))
                {
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 0] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[1, 0] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[2, 0] = true;

                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 2] = true;
                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[1, 2] = true;
                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 2] = true;
                }
                else if (g.transform.forward == new Vector3(-1, 0, 0))
                {
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 0] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 1] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 2] = true;

                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 0] = true;
                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 1] = true;
                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 2] = true;
                }
                else if (g.transform.forward == new Vector3(0, 0, -1))
                {
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 2] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[1, 2] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[2, 2] = true;

                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 0] = true;
                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[1, 0] = true;
                    masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 0] = true;
                }

            }

        }


        // Tile them with their respective material
        #region Texture Tiling
        float tileRate = 2f;
        float xoffset;
        float zoffset;

        foreach (SceneryObject g in groundTiles)
        {
            if (g == null) { continue; }

            xoffset = (g.tilemapPosition.x + tileRate) % tileRate;
            zoffset = (g.tilemapPosition.z + tileRate) % tileRate;


            g.gfxParent.GetComponent<Renderer>().material = Resources.Load($"MapResources/Tilesets/{g.tileset.ToString()}/GroundMat_{g.tileset.ToString()}", typeof(Material)) as Material;

            Material gMat = g.gfxParent.GetComponent<Renderer>().material;
            

            gMat.EnableKeyword("_NORMALMAP");


            gMat.mainTextureScale = new Vector2(((float)1 / tileRate), ((float)1 / tileRate));
            gMat.mainTextureOffset = new Vector2((float)((tileRate - xoffset) / tileRate), (float)((tileRate - zoffset) / tileRate));

            gMat.SetTextureScale("_BumpMap", new Vector2(((float)1 / tileRate), ((float)1 / tileRate)));
            gMat.SetTextureOffset("_BumpMap", new Vector2((float)((tileRate - xoffset) / tileRate), (float)((tileRate - zoffset) / tileRate)));
        }
        #endregion


    }

    public List<Vector3Int> BuildPath(Vector3Int from, Vector3Int to)
    {
        int panicInt = 0;
        List<Vector3Int> result = new List<Vector3Int>();
        result.Add(from);
        float pathCost = 0;

        Vector3Int currentCheckPos = from;

        List<Vector3Int> closed = new List<Vector3Int>();
        List<Vector3Int> closed2 = new List<Vector3Int>();

        LL_TileDataStruct listHead = new LL_TileDataStruct(new Vector3Int(-1,-1,-1), 999999);

        bool connected = true;

        while (currentCheckPos != to && panicInt < 1000)
        {
            panicInt++;

            float heursiticToBeat = 9999999;

            // Whats happening here is:
            // we will check all tiles in a 1 tile radius around the currentCheckPos tile
            // gotta make sure they are a valid, within boundries, pathable from check tile

            for (int xOffset = -1; xOffset < 2; xOffset++)
            {
                for (int zOffset = -1; zOffset < 2; zOffset++)
                {
                    if (zOffset == 0 || xOffset == 0)
                    {

                        Vector3Int adjacentPos = new Vector3Int(currentCheckPos.x + xOffset, currentCheckPos.y, currentCheckPos.z + zOffset);
                        // Gonna have to change this for ramps but oh well
                        if (ValidatePosition(adjacentPos))
                        {
                            // If we're here, the tile is inside the map
                            // now make sure it is pathable to 
                            // && !(xOffset != 0 && zOffset != 0)

                            connected = true;
                            if (closed.Contains(adjacentPos))
                            {
                                connected = false;

                            }



                            if (masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z] == null) { connected = false; }
                            else
                            {
                                Vector3Int dir = currentCheckPos - adjacentPos;
                                Vector2Int check = new Vector2Int(dir.x + 1, 1 - dir.z);

                                if (masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z].obstructedDirections[check.x, check.y] == true)
                                {
                                    // if we get in here, that means the adjacent tile is blocking travel from currentCheckPos's direction
                                    connected = false;
                                }

                            }

                            if (connected)
                            {
                                // when we get down here, should mean its a valid pathable tile
                                // now just check the cost and heuristic i think?

                                // ok the theory
                                // check all surrounding tile (which were already doing in this loop)
                                // for each tile, find the heuristic cost 

                                float adjHeuristic = (to - adjacentPos).magnitude;

                                LL_TileDataStruct newLLS = new LL_TileDataStruct(adjacentPos, adjHeuristic);

                                if (!closed2.Contains(adjacentPos))
                                {
                                    closed2.Add(adjacentPos);
                                    if (listHead.pos == new Vector3Int(-1, -1, -1))
                                    {
                                        listHead = newLLS;
                                    }
                                    else if (listHead.heuristic > newLLS.heuristic)
                                    {
                                        newLLS.childTile = listHead;
                                        listHead = newLLS;
                                    }
                                    else
                                    {
                                        LL_TileDataStruct checkLLS = listHead;
                                        while (checkLLS.childTile != null)
                                        {
                                            if (newLLS.heuristic <= checkLLS.childTile.heuristic)
                                            {
                                                newLLS.childTile = checkLLS.childTile;
                                                break;
                                            }
                                            checkLLS = checkLLS.childTile;
                                        }
                                        checkLLS.childTile = newLLS;
                                    }
                                }
                                //if (adjHeuristic < heursiticToBeat)
                                //{
                                //    bestNextPos = adjacentPos;
                                //    heursiticToBeat = adjHeuristic;
                                //}

                            }
                        }

                    }
                }
            } // <-- edges loop
            Vector3Int bestNextPos = listHead.pos;

            closed.Add(currentCheckPos);
            
            result.Insert(result.Count-1, bestNextPos);
            currentCheckPos = bestNextPos;
            listHead = listHead.childTile;
        }

        if (panicInt >= 999)
        {
            Debug.Log("Unpathable");

        }


        return result;
    }

    public bool ValidatePosition(Vector3Int input)
    {
        bool result = true;

        if (input.x < 0 || input.x > mapSize.x || input.z < 0 || input.z > mapSize.z || input.y < 0 || input.y > mapSize.y)
        {
            result = false;
        }



        return result;
    }

    public Vector2Int ConvertV3toV2I(Vector3 input)
    {
        Vector2Int result = new Vector2Int(Mathf.RoundToInt(input.x)+1, Mathf.RoundToInt(input.z)+1);

        return result;
    }
}


