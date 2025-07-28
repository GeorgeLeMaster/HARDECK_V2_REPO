using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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


    }

    // Start is called before the first frame update
    void Start()
    {
        BuildMap();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BuildMap()
    {
        // Find and Tile Ground Tiles

        // Build and texture ground tiles
        groundTiles = new SceneryObject[mapSize.x, mapSize.y, mapSize.z];
        masterVoxelData = new MapVoxelData[mapSize.x, mapSize.y, mapSize.z];

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int z = 0; z < mapSize.x; z++)
                {
                    masterVoxelData[x, y, z] = new MapVoxelData();
                }
            }
        }

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
                g.flags.pathable = true;
            }
            
        }

        foreach (SceneryObject g in grabbedTiles)
        {

            if (g.entityType == EntityType.SceneryObj)
            {
                Vector3Int pos = g.tilemapPosition;

                Vector3 offsetpos = g.tilemapPosition + g.transform.forward;
                Vector3Int pos2 = new Vector3Int(Mathf.RoundToInt( offsetpos.x ), Mathf.RoundToInt( offsetpos.y ), Mathf.RoundToInt(offsetpos.z));

                if (masterVoxelData[pos.x, pos.y, pos.z] == null)
                {
                    masterVoxelData[pos.x, pos.y, pos.z] = new MapVoxelData();
                }

                masterVoxelData[pos.x, pos.y, pos.z].sceneryObjects.Add(g);

                int i1 = DirToObstructionIndex(g.transform.forward);
                if (i1 != -1)
                    masterVoxelData[pos.x, pos.y, pos.z].obstructedDirections[i1] = true;
                masterVoxelData[pos.x, pos.y, pos.z].pathableStatus = PathableStatus.Blocked_Pathable;


                if (masterVoxelData[pos2.x, pos2.y, pos2.z] == null)
                {
                    masterVoxelData[pos2.x, pos2.y, pos2.z] = new MapVoxelData();
                }

                if (ValidatePosition(pos2))
                {
                    int i2 = DirToObstructionIndex(-g.transform.forward);
                    if (i2 != -1)
                        masterVoxelData[pos2.x, pos2.y, pos2.z].obstructedDirections[i2] = true;
                    masterVoxelData[pos2.x, pos2.y, pos2.z].pathableStatus = PathableStatus.Blocked_Pathable;
                }
            

            }

        }


        // Tile them with their respective material
        #region Texture Tiling
        float tileRate = 1f;
        float xoffset;
        float zoffset;

        foreach (SceneryObject g in groundTiles)
        {
            if (g == null) { continue; }
            if (g.entityType != EntityType.GroundTile) { continue; }

            xoffset = (g.tilemapPosition.x + tileRate) % tileRate;
            zoffset = (g.tilemapPosition.z + tileRate) % tileRate;

            //Debug.Log(g.tilemapPosition);

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

    public PathObject BuildPath(Vector3Int from, Vector3Int to)
    {
        PathObject newPath = new PathObject();

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
                        if (ValidatePosition(adjacentPos) && !closedVecRef.Contains(adjacentPos) && !openVecRef.Contains(adjacentPos) && (masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z].pathableStatus == PathableStatus.Pathable || masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z].pathableStatus == PathableStatus.Blocked_Pathable))
                        {
                            // This bool reflects if our checks here return true or not
                            bool connected = true;

                            Vector3Int dir = adjacentPos - checkPos;
                            int index = DirToObstructionIndex(dir);
                            //if (masterVoxelData[checkPos.x, checkPos.y, checkPos.z].obstructedDirections[check.x, check.y] == true)
                            //{
                            //    // if we get in here, that means the adjacent tile is blocking travel from currentCheckPos's direction
                            //    connected = false;
                            //}

                            if (masterVoxelData[checkPos.x, checkPos.y, checkPos.z].obstructedDirections[index] == true)
                            {

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


                                float h_add;
                                h_add = 0;


                                adjTile.tilemapPos = adjacentPos;
                                adjTile.g = Vector3.Distance(adjacentPos, checkPos);

                                // Incentivises straight lines
                                //if (checkTile.pathParent != null)
                                //{
                                //    if (checkTile.pathParent.tilemapPos - checkTile.tilemapPos != checkTile.tilemapPos - adjacentPos)
                                //    {
                                //        adjTile.g += 1;
                                //    }
                                //}

                                adjTile.h = Vector3.Distance(to, adjacentPos) + h_add;
                                adjTile.a = adjTile.h + adjTile.g;
                                adjTile.pathParent = checkTile;

                                TileDataStruct iterator = LLHT;
                                //Debug.Log($"LLHT Pos:{LLHT.tilemapPos}, AdjPos:{adjacentPos}, CheckPos:{checkPos}, PanicInt:{panicInt}");


                                // ALTERNATIVE FOR SIMPLE LIST
                                bool simpleList = false;

                                // Linked list logic
                                if (adjTile.a < LLHT.a || simpleList)
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
            newPath.valid = false;
            return newPath;
        }

        TileDataStruct p = LLHT;
        result.Add(LLHT.tilemapPos);
        while (p.tilemapPos != from)
        {
            p = p.pathParent;
            result.Add(p.tilemapPos);
        }
        //Debug.Log(result.Count);
        result = FlipList(result);
        newPath.origin = from;
        newPath.destination = to;
        newPath.valid = true;
        newPath.positions = result;
        return newPath;
    }

    public bool ValidatePosition(Vector3Int input)
    {
        bool result = true;

        if (input.x < 0 || input.x > mapSize.x-1 || input.z < 0 || input.z > mapSize.z-1 || input.y < 0 || input.y > mapSize.y-1 || masterVoxelData[input.x, input.y, input.z] == null)
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

    public int DirToObstructionIndex(Vector3 dir)
    {
        int result = 0;

        if (dir == new Vector3(0,0,1))
        {
            result = 0;
        }
        else if (dir == new Vector3(1,0,0))
        {
            result = 1;

        }
        else if (dir == new Vector3(0, 0, -1))
        {
            result = 2;

        }
        else if (dir == new Vector3(-1, 0, 0))
        {
            result = 3;
        }
        else
        {
            result = -1;
        }

        return result;
    }
}


