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
        }

        // SET INT POS
        intPos = new Vector3Int (Mathf.RoundToInt( transform.position.x ), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));


        if (groundTile)
        {
            gameObject.name = $"GroundTile {intPos}";
        }
    }
}
