using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryObject : EntityBase
{
    [Header("Scenery Object Components")]
    public bool obstructive;
    public bool pathable;


    private void Awake()
    {
        flags.pathable = pathable;

        // SET INT POS
        tilemapPosition = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

        if (entityType == EntityType.GroundTile)
        {
            gameObject.name = $"GroundTile {tilemapPosition}";
        }

    }

    private void Start()
    {

    }
}
