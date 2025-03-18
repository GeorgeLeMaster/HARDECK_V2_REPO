using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public enum EntityType
{
    SceneryObj, // For in world non terrain stuff
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

public class AbilityCallDataPackage
{


    public AbilityCallDataPackage(int _abilityId = -1, UnitLogic _caster = null, UnitLogic _target_unit = null, Vector3Int? _target_pos = null)
    {
        abilityId = _abilityId;
        caster = _caster;
        target_unit = _target_unit;
        target_pos = _target_pos;
    }

    public int abilityId;
    public UnitLogic caster;
    public UnitLogic target_unit;
    public Vector3Int? target_pos;
}
public static class AbilitiesLibrary
{

    public static string[] abilityNames =
    {
        "Move",
    };

    public static string[] abilityDefinitions =
    {
        "Move :D",
    };

    public static int CallAbility(AbilityCallDataPackage input)
    {
        int code = -1;

        
        if (CheckAbilityParameters(input))
        {
            switch (input.abilityId)
            {

                case 0:

                    //MOVE

                    input.caster.transform.position = (Vector3Int) input.target_pos;
                    input.caster.tilemapPosition = (Vector3Int) input.target_pos;

                    input.target_pos = null;
                    input.caster = null;
                    break;

            }
        }

        return code;
    }

    public static void DisplayAbilityPreGFX(AbilityCallDataPackage input)
    {
        
    }

    public static void DisplayACDPGFX(AbilityCallDataPackage input)
    {
        if (input.caster != null)
        {
            GameManager.Instance.selectedUnit_marker.SetActive(true);
            GameManager.Instance.selectedUnit_marker.transform.position = (Vector3Int)input.caster.tilemapPosition;
        }
        else
        {
            GameManager.Instance.selectedUnit_marker.SetActive(false);
        }

        if (!CheckAbilityParameters(input))
        {
            GameManager.Instance.target_pos_marker.SetActive(false);
            return;
        }

        switch (input.abilityId)
        {

            case 0:
                //MOVE
                GameManager.Instance.target_pos_marker.SetActive(true);
                GameManager.Instance.target_pos_marker.transform.position = (Vector3Int)input.target_pos;

                break;

        }
    }

    public static bool CheckAbilityParameters(AbilityCallDataPackage input)
    {
        bool output = false;

        switch (input.abilityId)
        {

            case 0:
                //MOVE
                if (input.caster != null && input.target_pos != null) output = true;
                break;

        }

        if (output == false)
        {
            Debug.Log("Parameter Fail!");
        }

        return output;
    }


}

public class AbilityObject
{
    public int abilityId;

    public int usesRemaining;
}

public class MapVoxelData
{
    public MapVoxelData()
    {
        pathableStatus = PathableStatus.Undeclared;

        visabilityStatus = VisabilityStatus.Undiscovered;

        tilemapPosition = new Vector3Int(-1,-1,-1);

        sceneryObjects = new List<SceneryObject>();
    }

    public PathableStatus pathableStatus;

    public VisabilityStatus visabilityStatus;

    public Vector3Int tilemapPosition;

    public List<SceneryObject> sceneryObjects;
}

