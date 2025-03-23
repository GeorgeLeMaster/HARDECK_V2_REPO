using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    Camera cam;

    public bool controllsLocked = false;

    [Header("Player Commander Components")]

    public int allianceInt_player;

    public AbilityCallDataPackage ACDP_player;

    public int selectedAbility_player;

    [Header("UI & GUI")]
    public LineRenderer lineRenderer1;

    public GameObject selectedUnit_marker;
    public GameObject target_pos_marker;
    public GameObject target_unit_marker;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameManager.Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        ACDP_player = new AbilityCallDataPackage();
    }

    // Update is called once per frame
    void Update()
    {
        // SLOTTING VARIOUS ENTITIES INTO THEIR VARIABLES, WE'LL DEAL WITH THE LOGIC AFTER
        if (Input.GetMouseButtonDown(0) && !controllsLocked)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100f))
            {
                if (hit.transform.GetComponent<EntityBase>() != null)
                {
                    EntityBase hitEntity = hit.transform.GetComponent<EntityBase>();

                    switch (hitEntity.entityType)
                    {
                        case EntityType.GroundTile:

                            SceneryObject hitGroundTile = hitEntity.GetComponent<SceneryObject>();
                            if (hitGroundTile != null)
                            {
                                if (hitGroundTile.flags.pathable == true && ACDP_player.caster != null)
                                {
                                    ACDP_player.target_pos = hitGroundTile.tilemapPosition;
                                }
                            }

                            break;

                        case EntityType.SceneryObj:

                            SceneryObject hitSceneryObject = hitEntity.GetComponent<SceneryObject>();
                            if (hitSceneryObject != null)
                            {
                                // IF THE SCENERY OBJECTG WE'VE CLICKED ON IS A PATHABLE GROUND TILE...
                                if (hitSceneryObject.flags.pathable == true && ACDP_player.caster != null)
                                {
                                    ACDP_player.target_pos = hitSceneryObject.tilemapPosition;
                                }
                            }

                            break;

                        case EntityType.Unit:

                            UnitLogic hitUnit = hitEntity.GetComponent<UnitLogic>();
                            if (hitUnit != null)
                            {
                                // IF THE UNIT WE'VE CLICKED ON IS A FRIENDLY...
                                if (hitUnit.allianceInt == allianceInt_player)
                                {
                                    ACDP_player.caster = hitUnit;
                                    Debug.Log($"Selected {hitUnit.unitName}");
                                }
                                else
                                {
                                    ACDP_player.target_unit = hitUnit;
                                    Debug.Log($"Targeting {hitUnit.unitName}");
                                }
                            }

                            break;
                    }
                }
            }

            ACDP_player.abilityId = selectedAbility_player;
            AbilityManager.Instance.DisplayACDPGFX(ACDP_player);
            AbilityManager.Instance.DisplayAbilityPreGFX( ACDP_player );
        } // <--- if (Input.GetMouseButtonDown(0))
    }

    public void TryCallAbiliy()
    {
        AbilityManager.Instance.CallAbility(ACDP_player);
    }
}
