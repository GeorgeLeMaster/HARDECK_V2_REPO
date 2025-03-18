using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryObject : EntityBase
{
    [Header("Scenery Object Components")]
    public bool groundTile;

    private void Awake()
    {
        // SET FLAGS
        if (!groundTile)
        {
            flags.destructable = true;
            flags.pathable = true;
        }

        // SET INT POS
        tilemapPosition = new Vector3Int (Mathf.RoundToInt( transform.position.x ), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));


        if (groundTile)
        {
            gameObject.name = $"GroundTile {tilemapPosition}";
        }


        // Logic to be executed for all non terrain scenery Objects vvvvvv
        if (entityType == EntityType.SceneryObj && !groundTile)
        {
            


        }
    }
}
