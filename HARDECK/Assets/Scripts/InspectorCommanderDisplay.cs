using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectorCommanderDisplay : MonoBehaviour
{
    public string commanderName;

    public int commanderAllianceInt;

    public List<UnitLogic> units;

    public MapVoxelData[,,] mapVoxelData;
}
