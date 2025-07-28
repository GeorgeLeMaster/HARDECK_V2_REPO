using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class UnitLogic : EntityBase
{

    public float moveSpeed;

    public int maxTimeUnits;
    public int currentTimeUnits;

    public UnitStatMods statMods;

    public List<Ability> abilities;

    public int allianceInt;

    public string unitName;
    public string unitType;
    public string unitRank;

    public List<string> abilityStrings;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = $"[{allianceInt}] {unitName}";
        tilemapPosition = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

        abilityStrings = new List<string>()
        {
        "2.MOVE.MOVE TO THE SELECTED LOCATION.gmmr.mtl.0.999.bMOVE.SELECT DESTINATION.10",
        "1.ATTACK.FIRE PRIMARY WEAPON AT SELECTED TARGET.eu/des.atk.0.999.bATTACK.SELECT TARGET",
        "1.DIG IN.TAKE DEFENSIVE POSITION, INCREASING SURVIVABILITY.na.dig.0.999.bDIG IN.zzz"
        };

        BuildAbilityObjects();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void BuildAbilityObjects()
    {
        //Debug.Log(abilityStrings.Count);
        abilities = new List<Ability>();
        
        for (int i = 0; i < abilityStrings.Count; i++)
        {
            Ability newAbility = new Ability(abilityStrings[i]);
            abilities.Add(newAbility);
        }
    }
}
