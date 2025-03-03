using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MapBuilder : MonoBehaviour
{

    public static MapBuilder instance;

    public Vector3Int mapSize;

    public SceneryObject[,,] groundTiles;

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
        SceneryObject[] grabbedTiles = GameObject.FindObjectsOfType<SceneryObject>();

        // Find all ground tiles and sort them into the groundTiles array
        foreach (SceneryObject g in grabbedTiles)
        {
            if (g.groundTile)
            {
                Vector3Int pos = g.intPos;
                groundTiles[pos.x, pos.y, pos.z] = g;
            }
        }

        // Tile them with their respective material
        int tileRate = 4;
        float xoffset;
        float zoffset;

        foreach (SceneryObject g in groundTiles)
        {
            if (g == null) { continue; }

            xoffset = (g.intPos.x + tileRate) % tileRate;
            zoffset = (g.intPos.z + tileRate) % tileRate;


            g.gfxParent.GetComponent<Renderer>().material = Resources.Load($"MapResources/Tilesets/{g.tileset.ToString()}/GroundMat_{g.tileset.ToString()}", typeof(Material)) as Material;
            //g.gfxParent.GetComponent<Renderer>().material = Resources.Load($"MapResources/Tilesets/Desert/GroundMat_Desert", typeof (Material)) as Material;

            Material gMat = g.gfxParent.GetComponent<Renderer>().material;
            

            gMat.EnableKeyword("_NORMALMAP");


            gMat.mainTextureScale = new Vector2(((float)1 / tileRate), ((float)1 / tileRate));
            gMat.mainTextureOffset = new Vector2((float)((tileRate - xoffset) / tileRate), (float)((tileRate - zoffset) / tileRate));

            gMat.SetTextureScale("_BumpMap", new Vector2(((float)1 / tileRate), ((float)1 / tileRate)));
            gMat.SetTextureOffset("_BumpMap", new Vector2((float)((tileRate - xoffset) / tileRate), (float)((tileRate - zoffset) / tileRate)));
        }

    }
}

