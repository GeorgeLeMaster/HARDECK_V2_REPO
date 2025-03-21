using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Assertions;


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
        List<Vector3Int> toCheck = new List<Vector3Int>();
        toCheck.Add(from);

        float actualCost = 0;
        float heuristicToBeat = 10000000;

        while (toCheck.Count() != 0)
        {

            currentCheckPos = toCheck.First();

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
                            continue;
                        }

                        // Little bit of logic here to check if its marked as pathable. if it is, then if moves past this code block. if not, we enter and see if we can still walk there
                        if (masterVoxelData[adjacentPos.x, adjacentPos.y,adjacentPos.z].pathableStatus != PathableStatus.Pathable)
                        {
                            if (masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z].pathableStatus == PathableStatus.Blocked_Pathable)
                            {
                                // if we get here, the tile is marked as blocked_pathable, meaning it is pathable when clear, but certain directions might still be pathable\

                                Vector2Int check = new Vector2Int(1 + currentCheckPos.x - adjacentPos.x, 1 + currentCheckPos.z - adjacentPos.z);
                                if (masterVoxelData[adjacentPos.x, adjacentPos.y, adjacentPos.z].obstructedDirections[check.x, check.y])
                                {
                                    // if we get in here, that means the adjacent tile is blocking travel from currentCheckPos's direction, so CONTINUE
                                    continue;
                                }
                            }
                            else
                            {
                                // if we get HERE, that means we are neither pathble, nor blocked_pathable, and we can in no circumstances go here so CONTINUE
                                continue;
                            }
                        } // <---- Pathable check

                        // when we get down here, should mean its a valid pathable tile
                        // now just check the cost and heuristic i think?

                        

                        float newHeuristic = (to - adjacentPos).magnitude;
                        float newCost = (adjacentPos - currentCheckPos).magnitude + actualCost;

                        // If the heuristic cost of the adjacent tile is shorter than the best one we've seen...
                        if (newHeuristic < heuristicToBeat)
                        {
                            // add the old best pos to the check vector to make sure it does get eventually checked in case of a dead end
                            if (bestNextPos != new Vector3Int(-1,-1,-1) && !toCheck.Contains(bestNextPos))
                            {
                                toCheck.Add(bestNextPos);
                            }

                            // set the new best heuristic to the current one
                            heuristicToBeat = newHeuristic;
                            bestNextPos = adjacentPos;
                        }
                        else
                        {
                            if (!toCheck.Contains(adjacentPos))
                            {
                                toCheck.Add(adjacentPos);
                            }
                        }

                    }


                }
            }

            bestNextPos = new Vector3Int(-1, -1, -1);
            heuristicToBeat = 0;
            toCheck.Remove(currentCheckPos);
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
}


