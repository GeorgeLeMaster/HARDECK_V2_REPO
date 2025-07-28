using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

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
                    GFXManager.instance.LR_movement.positionCount = 0;
                    StartCoroutine(ExecuteLongLogic(0, input));
                    break;

            }
        }

        return code;
    }

    public void DisplayAbilityPreGFX(AbilityCallDataPackage input)
    {
        switch(input.abilityId)
        {
            case 0:
                if (input.caster != null && input.target_pos != null)
                {

                    Vector3 offset = new Vector3(0, 0.05f, 0);
                    PathObject path = MapBuilder.instance.BuildPath(input.caster.tilemapPosition, (Vector3Int)input.target_pos);

                    GFXManager.instance.LR_movement.positionCount = path.positions.Count();
                    for (int i = 0; i < path.positions.Count; i++)
                    {
                        GFXManager.instance.LR_movement.SetPosition(i, path.positions[i] + offset);
                    }

                    Vector3 pos1 = path.positions[0] + (new Vector3((path.positions[1] - path.positions[0]).x, (path.positions[1] - path.positions[0]).y, (path.positions[1] - path.positions[0]).z) * 0.5f);
                    GFXManager.instance.LR_movement.SetPosition(0, pos1 + offset);

                    Vector3 pos2 = path.positions[path.positions.Count - 1] - (new Vector3((path.positions[path.positions.Count - 1] - path.positions[path.positions.Count - 2]).x, (path.positions[path.positions.Count - 1] - path.positions[path.positions.Count - 2]).y, (path.positions[path.positions.Count - 1] - path.positions[path.positions.Count - 2]).z) * 0.5f);
                    GFXManager.instance.LR_movement.SetPosition(path.positions.Count - 1, pos2 + offset);
                }
                else
                {
                    GFXManager.instance.LR_movement.positionCount = 0;
                }
                break;
        }
    }

    public void DisplayACDPGFX(AbilityCallDataPackage input)
    {
        if (input.caster != null)
        {
            GameManager.Instance.selectedUnitMarker.SetActive(true);
            GameManager.Instance.selectedUnitMarker.transform.position = (Vector3Int)input.caster.tilemapPosition;
        }
        else
        {
            GameManager.Instance.selectedUnitMarker.SetActive(false);
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

    public IEnumerator ExecuteLongLogic(int abilityId, AbilityCallDataPackage ACDP) // ------------------------------------------------------------------------------------------------------------ LONG LOGIC
    {
        GameManager.Instance.controllsLocked = true;
        bool endConditions = false;

        PathObject path = new PathObject();
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
                    GameManager.Instance.selectedUnitMarker.transform.position = ACDP.caster.gameObject.transform.position;

                    if (path.positions.Count == 1 && path.positions[0] == ACDP.caster.tilemapPosition)
                    {
                        endConditions = true;
                        break;
                    }

                    if (Vector3.Distance(ACDP.caster.gameObject.transform.position, path.positions.First()) > 0.005f)
                    {
                        ACDP.caster.gameObject.transform.position = Vector3.Lerp(prevPos, (Vector3Int)path.positions.First(), timeElapsed/ACDP.caster.moveSpeed);
                    }
                    else
                    {
                        path.positions.Remove(path.positions.First());
                        if (path.positions.Count == 0)
                        {

                            ACDP.caster.transform.position = (Vector3Int)ACDP.target_pos;
                            ACDP.caster.tilemapPosition = (Vector3Int)ACDP.target_pos;

                            ACDP.target_pos = null;


                            endConditions = true;
                        }
                        else
                        {
                            prevPos = ACDP.caster.transform.position;
                            timeElapsed = 0;

                            if (path.positions.Count() == 0)
                            {
                                break;
                            }
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
