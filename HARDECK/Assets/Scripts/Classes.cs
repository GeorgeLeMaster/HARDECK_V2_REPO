using JetBrains.Annotations;
using System;
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
    Bricks,
    Undeclared
}

public class Commander
{
    public int allianceInt;

    public List<UnitLogic> controlledUnits;
}

public class Ability
{
    public Ability(string input)
    {
        string[] periodSplits = input.Split('.');

        int.TryParse(periodSplits[0], out int _timeCost);
        timeCost = _timeCost;

        name = periodSplits[1];
        description = periodSplits[2];

        parameters = periodSplits[3].Split(',');
        effects = periodSplits[4].Split(',');

        int.TryParse(periodSplits[5], out int _cooldown);
        cooldown = _cooldown;

        int.TryParse(periodSplits[6], out int _uses);
        uses = _uses;

        string customDirector = "";
        string sub = periodSplits[7];
        
        if (sub.Substring(0, 1).ToLower() == "b")
        {
            customDirector = "builtIn";
        }
        else if (sub.Substring(0, 1).ToLower() == "c")
        {
            customDirector = "custom";
        }

        string abFileName = sub.Substring(1);

        icon = Resources.Load<Sprite>($"UnitComponents/Abilities/{customDirector}/{abFileName}/Icon_{abFileName}") as Sprite;

        targetingTip = periodSplits[8];
    }

    public int timeCost = -1;

    public string name = "Error";
    public string description = "Error";

    public string[] parameters;
    public string[] effects;

    public int cooldown = -1;
    public int uses = -1;

    public Sprite icon;

    public string targetingTip;
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

public class PathObject
{
    public PathObject()
    {
        valid = false;

        origin = new Vector3Int(-1,-1,-1);
        destination = new Vector3Int(-1, -1, -1);

        positions = new List<Vector3Int>();
    }

    public bool valid;

    public Vector3Int origin;
    public Vector3Int destination;

    public List<Vector3Int> positions;
}

public class TargetingPackage
{
    public UnitLogic caster;
    public EntityBase targetedEntity;
    public Vector3 targetedPos;
}

