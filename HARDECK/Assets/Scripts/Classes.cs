using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public enum EntityType
{
    SceneryObj, // For in world non terrain stuff
    GroundTile,
    Unit, // Units
    Other,
    Undeclared
}

public enum VisabilityStatus
{
    Visable,
    Invisable,
    FogOfWar,
    Undiscovered
}

public enum PathableStatus
{
    Pathable,
    Air,
    Blocked_Pathable,
    Blocked_Air,
    Undeclared
}

public enum Tileset
{
    Desert,
    Undeclared
}

public class Commander
{
    public int allianceInt;

    public List<UnitLogic> controlledUnits;
}

public class EntityBase : MonoBehaviour
{
    [Header("Entity Base Components")]
    public Flags flags = new Flags();

    public Vector3Int tilemapPosition = Vector3Int.zero;

    public EntityType entityType = EntityType.Undeclared;
    public Tileset tileset = Tileset.Undeclared;
    public VisabilityStatus visabilityStatus = VisabilityStatus.Undiscovered;

    public float currentHitpoints;
    public float maxHitpoints;

    public GameObject gfxParent;

    void Start()
    {
        maxHitpoints = currentHitpoints;
    }

    public virtual void TakeDamage(float damgeInput)
    {
        currentHitpoints -= damgeInput;

        if (currentHitpoints <= 0 )
        {
            TryDeath();
        }
    }

    public virtual void TryDeath()
    {
        Destroy(gameObject);
    }
}

public class Flags
{
    public bool destructable;
    public bool targetable;
    public bool pathable;
}

public class UnitStatMods
{
    public float moveSpeedMod;
}


public class MapVoxelData
{

    public MapVoxelData()
    {
        pathableStatus = PathableStatus.Undeclared;

        visabilityStatus = VisabilityStatus.Undiscovered;

        tilemapPosition = new Vector3Int(-1,-1,-1);

        sceneryObjects = new List<SceneryObject>();

        obstructedDirections = new bool[4];
    }

    public PathableStatus pathableStatus;

    public VisabilityStatus visabilityStatus;

    public Vector3Int tilemapPosition;

    public List<SceneryObject> sceneryObjects;

    public bool[] obstructedDirections;
}

