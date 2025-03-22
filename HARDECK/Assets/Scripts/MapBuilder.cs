using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Assertions;

public class TileDataStructure
{

    public TileDataStructure(Vector3Int posInput = default(Vector3Int), TileDataStructure connectedTileInput = null)
    {
        tilemapPos = posInput;
        connectedTile = connectedTileInput;
        builtOn = false;

        actualCost = -1f;
    }

    public float actualCost;
    public float heuristicCost;

    public Vector3Int tilemapPos;

    public TileDataStructure connectedTile;

    public bool builtOn;


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
        // CALL THIS IN GAME MANAGER EVENTUALLY; MAP BUILD SHOULD HAVE CAALLED FUNCTIONS AND IMPORTANT MAP MEMBERS, NOT ACTUALL DO THE RUNTIME LOGIC ITSELF
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
        groundTiles = new SceneryObject[mapSize.x + 1, mapSize.y + 1, mapSize.z + 1];
        masterVoxelData = new MapVoxelData[mapSize.x + 1, mapSize.y + 1, mapSize.z + 1];
        SceneryObject[] grabbedTiles = GameObject.FindObjectsOfType<SceneryObject>();

        // Find all ground tiles and sort them into the groundTiles array
        foreach (SceneryObject g in grabbedTiles)
        {
            if (g.groundTile)
            {
                Vector3Int pos = g.tilemapPosition;
                groundTiles[pos.x, pos.y, pos.z] = g;
                masterVoxelData[pos.x, pos.y, pos.z].pathableStatus = PathableStatus.Pathable;
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
        List<Vector3Int> result = new List<Vector3Int>();

        Vector3Int currentCheckPos;
        Vector3Int bestNextPos = new Vector3Int(-1,-1,-1);

        List<Vector3Int> closed = new List<Vector3Int>();
        List<Vector3Int> open = new List<Vector3Int>();
        open.Add(from);
        bool connected = true;

        while (open.Count() != 0)
        {
            currentCheckPos = open.First();

            // Whats happening here is:
            // we will check all tiles in a 1 tile radius around the currentCheckPos tile
            // gotta make sure they are a valid, within boundries, pathable from check tile

            for (int xOffset = -1; xOffset < 2; xOffset++)
            {
                for (int zOffset = -1; zOffset < 2; zOffset++)
                {
                    
                    Vector3Int adjacentPos = new Vector3Int(currentCheckPos.x + xOffset, currentCheckPos.y, currentCheckPos.z + zOffset);
                    // Gonna have to change this for ramps but oh well
                    if (ValidatePosition(adjacentPos))
                    {
                        // If we're here, the tile is inside the map
                        // now make sure it is pathable to 

                        if (closed.Contains(adjacentPos))
                        {
                            connected = false;
                            continue;
                        }

                        // Little bit of logic here to check if its marked as pathable. if it is, then if moves past this code block. if not, we enter and see if we can still walk there
                        if (masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z].pathableStatus != PathableStatus.Pathable)
                        {
                            if (masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z].pathableStatus == PathableStatus.Blocked_Pathable)
                            {
                                // if we get here, the tile is marked as blocked_pathable, meaning it is pathable when clear, but certain directions might still be pathable\

                                Vector2Int check = new Vector2Int(1 + currentCheckPos.x - adjacentPos.x, 1 + currentCheckPos.z - adjacentPos.z);
                                if (masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z].obstructedDirections[check.x, check.y])
                                {
                                    // if we get in here, that means the adjacent tile is blocking travel from currentCheckPos's direction
                                    connected = false;
                                }
                                else
                                {
                                    connected = true;
                                }
                            }
                            else
                            {
                                // if we get HERE, that means we are neither pathble, nor blocked_pathable, and we can in no circumstances go here
                                connected = false;

                            }
                        }
                        else
                        {
                            connected = true;
                        }// <---- Pathable check


                        if (connected)
                        {
                            // when we get down here, should mean its a valid pathable tile
                            // now just check the cost and heuristic i think?

                            // ok the theory
                            // check all surrounding tile
                            // for each tile, find the heuristic cost 
                            // stich it into the open list inbetween the closest higher and lower heuristic costs

                            float adjHeuristic = (to - adjacentPos).magnitude;

                            for (int i = 0; i < open.Count; i++)
                            {
                                if (adjHeuristic >= open[i].)
                            }

                        }
                    }

                }
            }

            // Set its data point in the bool array to true so we dont grab it again
        }

        return result;
    }

    public bool ValidatePosition(Vector3Int input)
    {
        bool result = true;

        if (input.x < 0 || input.x > mapSize.x-1 || input.z < 0 || input.z > mapSize.z - 1 || input.y < 0 || input.y > mapSize.y - 1)
        {
            result = false;
        }

        return result;
    }



    public List<Vector3Int> BuildPath2(Vector3Int from, Vector3Int to)
    {

        List<TileDataStructure> result = null;

        List<TileDataStructure> tts = new List<TileDataStructure>();
        List<Vector3Int> checkedPositions = new List<Vector3Int>();

        TileDataStructure firstTile = new TileDataStructure(from, null);
        firstTile.actualCost = 0;
        firstTile.heuristicCost = (to - from).magnitude;

        tts.Add(firstTile);
        checkedPositions.Add(from);

        int panicInt = 0;

        while (result == null && panicInt < 100000)
        {
            // While we still have tiles to search
            TileDataStructure checkTile = tts.First();
            tts.Remove(checkTile);
            TileDataStructure nextTile = new TileDataStructure();

            float lowestCost = 1000000;

            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    int newX = checkTile.tilemapPos.x + x,
                        newZ = checkTile.tilemapPos.z + z;

                    if (masterVoxelData[newX, checkTile.tilemapPos.y, newZ].ob != "open")
                    {
                        continue;
                    }

                    if (newX < 0 || newX >= mapSize.x || newY < 0 || newY >= mapSize.y || (newY == 0 && newX == 0))
                    {
                        continue;
                    }

                    TileDataStructure t = new TileDataStructure();

                    if (!checkedPositions.Contains(new Vector2Int(newX, newY)))
                    {
                        float costToAdd = 1;
                        if (x != 0 && y != 0)
                        {
                            costToAdd = 1.42f;

                        }
                        //  Debug.Log($"checking {newX},{newY}");
                        float heuristic = Mathf.Abs(Vector2.Distance(to, new Vector2Int(newX, newY)));

                        if (checkTile.actualCost + costToAdd + heuristic < lowestCost)
                        {
                            nextTile.tilemapPos = new Vector2Int(newX, newY);
                            nextTile.actualCost = checkTile.actualCost + costToAdd;
                            nextTile.connectedTile = checkTile;
                            lowestCost = checkTile.actualCost + costToAdd + heuristic;
                        }
                    }
                }
            }

            if (nextTile.tilemapPos != closest_To)
            {
                // Debug.Log($"checking {nextTile.pos} next");
                tts.Add(nextTile);
                checkedPositions.Add(nextTile.tilemapPos);

                lowestCost = 1000000;
            }
            else
            {
                Debug.Log($"path found. distance : {nextTile.actualCost}");

                // build the path
                TileDataStructure v = nextTile;
                panicInt = 0;
                result = new List<TileDataStructure>();
                while (v.tilemapPos != closest_From && panicInt < 1000000)
                {
                    result.Add(v);
                    nextTile = nextTile.connectedTile;
                    v = nextTile;
                }

                break;

            }
            // find the tile with the cheapest combo of actual and heuristic cost

            // from that tile, repeat above (excluding tiles in search queue already), set its previous to current tile, and add it to the search queue

            // declare current tile as searchd, remove it from the search queue

            panicInt++;
        }




        // build cleaner path

        List<Vector3> finalResult = new List<Vector3>();
        finalResult.Add(to);

        // start at the TO position and work backwards
        Vector2Int prevPoint = closest_To;
        TileDataStructure checkPoint = result.First();
        TileDataStructure lastCheckedTile = result.First();

        panicInt = 0;

        while (checkPoint.tilemapPos != closest_From && panicInt < 100000)
        {
            if (prevPoint.x == checkPoint.tilemapPos.x || prevPoint.y == checkPoint.tilemapPos.y)
            {
                // straight lines should be automatically clear right?
                checkPoint = checkPoint.connectedTile;
                panicInt++;

                continue;

            }

            Vector2 dir = checkPoint.tilemapPos - prevPoint;
            int iters = Mathf.CeilToInt(dir.magnitude);
            dir.Normalize();

            bool blocked = false;
            for (int i = 1; i < iters; i++)
            {


                Vector2Int newPos = new Vector2Int(Mathf.RoundToInt((prevPoint + (dir * i)).x), Mathf.RoundToInt((prevPoint + (dir * i)).y));
                if (tileStatusies[newPos.x, newPos.y] != "open")
                {
                    // we heeben ein serious problem
                    blocked = true;
                    break;
                }
                // maybe check the checktiles connected tile and do the same diagonal check as above?
            }

            if (blocked)
            {
                // this means the path is not clear 
                // add lastcheckedtile to pathpoints as vector3
                finalResult.Add(new Vector3(lastCheckedTile.tilemapPos.x, lastCheckedTile.tilemapPos.y, (float)(lastCheckedTile.tilemapPos.y / 10)));
                // set prevpoint = lastcheckedtile
                prevPoint = lastCheckedTile.tilemapPos;
            }
            else
            {
                lastCheckedTile = checkPoint;
                checkPoint = checkPoint.connectedTile;

            }
            panicInt++;
        }

        //finalResult.Add();

        return finalResult;
    }
}


