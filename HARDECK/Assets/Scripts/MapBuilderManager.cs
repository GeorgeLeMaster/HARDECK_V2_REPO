using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilderManager : MonoBehaviour
{

    public Vector3Int mapSize;

    private GameObject terrainCubePrefab;
    private GameObject catwalkPrefab;
    private GameObject barrierPrefab;


    private void Awake()
    {
        terrainCubePrefab = Resources.Load("MapResources/Building Blocks/Building Block - Terrain Cube Prefab") as GameObject;
        catwalkPrefab = Resources.Load("MapResources/Building Blocks/Building Block - Catwalk Prefab") as GameObject;
        barrierPrefab = Resources.Load("MapResources/Building Blocks/Building Block - Barrier Prefab") as GameObject;

        Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        GameObject newGameObject;
        Vector3 spawnPos = Vector3.zero;

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                spawnPos = new Vector3(x,0,z);
                newGameObject = Instantiate(terrainCubePrefab);
                newGameObject.transform.position = spawnPos;
            }
        }

    }

    public void CompileMap()
    {

    }
}
