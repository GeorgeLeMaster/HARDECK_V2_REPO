using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    private void Awake()
    {
        Instance = this;
    }


    public int CallAbility(AbilityCallDataPackage input)
    {
        int code = -1;

        if (CheckAbilityParameters(input) == true)
        {
            switch (input.abilityId)
            {

                case 0:

                    //MOVE

                    StartCoroutine(ExecuteLongLogic(0, input));
                    break;

            }
        }

        return code;
    }

    public void DisplayAbilityPreGFX(AbilityCallDataPackage input)
    {

    }

    public void DisplayACDPGFX(AbilityCallDataPackage input)
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

    public bool CheckAbilityParameters(AbilityCallDataPackage input)
    {
        bool output = false;

        switch (input.abilityId)
        {

            case 0:
                //MOVE
                if (input.caster != null && input.target_pos != null) return true;
                break;

        }

        if (output == false)
        {
            Debug.Log("Parameter Fail!");
        }

        return output;
    }

    public IEnumerator ExecuteLongLogic(int abilityId, AbilityCallDataPackage ACDP)
    {
        GameManager.Instance.controllsLocked = true;
        bool endConditions = false;

        List<Vector3Int> path = new List<Vector3Int>();
        Vector3 prevPos = Vector3.zero;
        float timeElapsed = 0;

        if (abilityId == 0)
        {
            path = MapBuilder.instance.BuildPath(ACDP.caster.tilemapPosition, (Vector3Int)ACDP.target_pos);
            prevPos = ACDP.caster.tilemapPosition;
        }

        while (endConditions == false)
        {
            switch (abilityId)
            {
                case 0: // MOVE__________________________________________________

                    timeElapsed += Time.deltaTime;

                    if (Vector3.Distance(ACDP.caster.gameObject.transform.position, path.First()) > 0.005f)
                    {
                        ACDP.caster.gameObject.transform.position = Vector3.Lerp(prevPos, (Vector3Int)path.First(), timeElapsed/ACDP.caster.moveSpeed);
                    }
                    else
                    {
                        if (Vector3.Distance(ACDP.caster.gameObject.transform.position, (Vector3Int)ACDP.target_pos) < 0.01f)
                        {

                            ACDP.caster.transform.position = (Vector3Int)ACDP.target_pos;
                            ACDP.caster.tilemapPosition = (Vector3Int)ACDP.target_pos;

                            ACDP.target_pos = null;
                            ACDP.caster = null;

                            endConditions = true;
                        }
                        else
                        {
                            prevPos = ACDP.caster.transform.position;
                            timeElapsed = 0;
                            path.Remove(path.First());
                        }
                    }

                    break;

                default:
                    endConditions = true;
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
        GameManager.Instance.controllsLocked = false;

        DisplayACDPGFX(ACDP);

    }



}

public class AbilityObject
{
    public int abilityId;

    public int usesRemaining;
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
