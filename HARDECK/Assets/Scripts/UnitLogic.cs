using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLogic : EntityBase
{

    public float moveSpeed;

    public UnitStatMods statMods;

    public List<AbilityObject> baseAbilities;
    public List<AbilityObject> currentAbilities;

    public int allianceInt;

    public string unitName;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = $"[{allianceInt}] {unitName}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
