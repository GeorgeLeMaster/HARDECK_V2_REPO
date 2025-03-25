using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.TerrainTools;

public class TileDataStruct
{

    public TileDataStruct() 
    {
        a = -1;
        h = -1;
        g = -1;

        tilemapPos = new Vector3Int(-1, -1, -1);
    }

    public TileDataStruct(TileDataStruct input)
    {
        a = input.a;
        h = input.h;
        g = input.g;

        pathParent = input.pathParent;
        listChild = input.listChild;

        tilemapPos = input.tilemapPos;
    }

    public float a;
    public float h;
    public float g;

    public TileDataStruct pathParent;
    public TileDataStruct listChild;

    public Vector3Int tilemapPos;

    public void Copy(TileDataStruct input)
    {
        a = input.a;
        h = input.h;
        g = input.g;

        pathParent = input.pathParent;
        listChild = input.listChild;

        tilemapPos = input.tilemapPos;
    }
}


public class LL_TileDataStruct
{
    public LL_TileDataStruct(Vector3Int input_v, float input_h)
    {
        pos = input_v;
        heuristic = input_h;
    }

    public Vector3Int pos;

    public float heuristic;

    public LL_TileDataStruct parentTile;

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

                    if (ValidatePosition(pos2))
                    {

                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 0] = true;
                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 1] = true;
                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 2] = true;

                    }
                }
                else if (g.transform.forward == new Vector3(0, 0, 1))
                {
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 0] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[1, 0] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[2, 0] = true;

                    if (ValidatePosition(pos2))
                    {

                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 2] = true;
                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[1, 2] = true;
                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 2] = true;

                    }
                }
                else if (g.transform.forward == new Vector3(-1, 0, 0))
                {
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 0] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 1] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 2] = true;

                    if (ValidatePosition(pos2))
                    {

                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 0] = true;
                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 1] = true;
                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 2] = true;

                    }
                }
                else if (g.transform.forward == new Vector3(0, 0, -1))
                {
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[0, 2] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[1, 2] = true;
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[2, 2] = true;

                    if (ValidatePosition(pos2))
                    {

                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[0, 0] = true;
                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[1, 0] = true;
                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[2, 0] = true;

                    }
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

        TileDataStruct checkTile = new TileDataStruct();
        checkTile.tilemapPos = from;
        checkTile.g = 0;
        
        checkTile.h = Vector3.Distance(to, from);
        checkTile.a = checkTile.h;
        Vector3Int checkPos = checkTile.tilemapPos;

        List<TileDataStruct> closed = new List<TileDataStruct>();
        List<Vector3Int> closedVecRef = new List<Vector3Int>();
        List<Vector3Int> openVecRef = new List<Vector3Int>();
        openVecRef.Add(checkPos);

        TileDataStruct LLHT = checkTile;

        while (checkPos != to && panicInt < 1000 & LLHT != null)
        {


            for (int xOffset = -1; xOffset < 2;)
            {
                for (int zOffset = -1; zOffset < 2;)
                {
                    if ((zOffset == 0 || xOffset == 0) && !(zOffset == 0 && xOffset == 0))
                    {

                        Vector3Int adjacentPos = new Vector3Int(checkPos.x + xOffset, checkPos.y, checkPos.z + zOffset);

                        // Gonna have to change this for ramps but oh well
                        if (ValidatePosition(adjacentPos) && !closedVecRef.Contains(adjacentPos) && !openVecRef.Contains(adjacentPos) && masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z] != null)
                        {
                            // This bool reflects if our checks here return true or not
                            bool connected = true;

                            Vector3Int dir = adjacentPos - checkPos;
                            Vector2Int check = new Vector2Int(dir.x + 1, 1 - dir.z );

                            if (masterVoxelData[checkPos.x, checkPos.y, checkPos.z].obstructedDirections[check.x, check.y] == true)
                            {
                                // if we get in here, that means the adjacent tile is blocking travel from currentCheckPos's direction
                                connected = false;
                            }




                            if (connected)
                            {
                                // when we get down here, should mean its a valid pathable tile
                                // now just check the cost and heuristic i think?

                                // ok the theory
                                // check all surrounding tile (which were already doing in this loop)
                                // for each tile, find the heuristic cost 

                                openVecRef.Add(adjacentPos);

                                TileDataStruct adjTile = new TileDataStruct();
                                adjTile.tilemapPos = adjacentPos;
                                adjTile.g = Vector3.Distance(adjacentPos, checkPos);
                                adjTile.h = Vector3.Distance(to, adjacentPos);
                                adjTile.a = adjTile.h + adjTile.g;
                                adjTile.pathParent = checkTile;

                                TileDataStruct iterator = LLHT;
                                Debug.Log($"LLHT Pos:{LLHT.tilemapPos}, AdjPos:{adjacentPos}, CheckPos:{checkPos}, PanicInt:{panicInt}");



                                if (adjTile.a < LLHT.a)
                                {
                                   // bumped = true;
                                    adjTile.listChild = LLHT;
                                    LLHT = adjTile;
                                }
                                else if (LLHT.listChild == null)
                                {
                                    LLHT.listChild = adjTile;
                                }
                                else
                                {
                                    while (iterator.listChild != null)
                                    {
                                        if (adjTile.a <= iterator.listChild.a)
                                        {
                                            adjTile.listChild = iterator.listChild;
                                            iterator.listChild = adjTile;
                                            break;
                                        }
                                        else
                                        {
                                            iterator = iterator.listChild;
                                        }
                                    }

                                    if (iterator.listChild == null)
                                    {
                                        iterator.listChild = adjTile;
                                    }
                                }

                            }
                        }

                    }
                    zOffset++;
                }
                xOffset++;
            } // <-- edges loop

            if (!closedVecRef.Contains(checkPos))
            {
                closed.Add(checkTile);
                closedVecRef.Add(checkPos);
            }

            if (checkTile == LLHT)
            {
                checkTile = LLHT.listChild;
                LLHT = checkTile;
            }
            else
            {
                checkTile = LLHT;
            }
            // Debug.Log(LLHT.tilemapPos);
            if (checkTile != null)
            {
                checkPos = checkTile.tilemapPos;
                panicInt++;

            }
            else
            {
                panicInt = 100000;
            }
        }

        if (panicInt >= 999)
        {
            Debug.Log("Unpathable");
            result.Clear();
            result.Add(from);
            return result;
        }

        TileDataStruct p = LLHT;
        result.Add(LLHT.tilemapPos);
        while (p.tilemapPos != from)
        {
            p = p.pathParent;
            result.Add(p.tilemapPos);
        }
        Debug.Log(result.Count);
        return FlipList(result);
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

    public List<Vector3Int> FlipList(List<Vector3Int> input)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        
        for (int i = 0; i < input.Count; i++)
        {
            result.Add(input[input.Count-1-i]);
        }

        return result;
    }
}


