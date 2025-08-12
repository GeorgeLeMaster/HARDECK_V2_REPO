using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;


    Camera cam;

    public bool controllsLocked = false;

    public TargetingPackage targetingPackage;

    [Header("Player Commander Components")]

    public int allianceInt_player;

    public EntityBase player_targetedTile;
    public UnitLogic player_SelectedPlayerUnit;
    public Ability player_selectedUnitAbility;
    public UnitLogic player_targetedUnit;

    [Header("UI & GUI")]
    public LineRenderer lineRenderer1;

    public GameObject markersParent;
    public GameObject selectedUnitMarker;
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
        targetingPackage = new TargetingPackage();

        cam = Camera.main;

        GFXManager.instance.UpdateSelectedUnitUI();

        selectedUnitMarker.SetActive(false);
        selectedUnitMarker.transform.parent = markersParent.transform;
        selectedUnitMarker.transform.position = new Vector3(0, -0.01f, -2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !controllsLocked)
        {
            TryCallAbility(player_selectedUnitAbility);
        }

        // SLOTTING VARIOUS ENTITIES INTO THEIR VARIABLES, WE'LL DEAL WITH THE LOGIC AFTER
        if (Input.GetMouseButtonDown(0) && !controllsLocked && !IsPointerOverUIElement())
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100f))
            {
                if (hit.transform.GetComponent<EntityBase>() != null)
                {
                    EntityBase hitEntity = hit.transform.GetComponent<EntityBase>();

                    switch (hitEntity.entityType)
                    {
                        case EntityType.GroundTile or EntityType.SceneryObj:


                            if (player_SelectedPlayerUnit != null)
                            {
                                player_targetedTile = hitEntity;
                                targetingPackage.targetedEntity = hitEntity;
                            }


                            break;


                        case EntityType.Unit:

                            UnitLogic hitUnit = hitEntity.GetComponent<UnitLogic>();
                            if (hitUnit != null)
                            {
                                // IF THE UNIT WE'VE CLICKED ON IS A FRIENDLY...
                                if (hitUnit.allianceInt == allianceInt_player)
                                {

                                    if (player_SelectedPlayerUnit == hitUnit)
                                    {
                                        player_SelectedPlayerUnit = null;

                                        selectedUnitMarker.SetActive(false);
                                        selectedUnitMarker.transform.parent = markersParent.transform;
                                        selectedUnitMarker.transform.position = new Vector3(0, -0.01f, -2f);
                                    }
                                    else
                                    {
                                        player_SelectedPlayerUnit = hitUnit;

                                        selectedUnitMarker.SetActive(true);
                                        selectedUnitMarker.transform.parent = hitUnit.transform;
                                        selectedUnitMarker.transform.position = hitUnit.transform.position;
                                    }

                                    // THIS WILL NEED UPDATING FOR FRIENDLY TARGETING FOR THINGS LIKE HEALING
                                    // probably make some "use or observe" function that takes in a unit alliance and checks if it should be displayed or used in targeting info
                                    if (GFXManager.instance.selectedUnitDisplayAnchor.selectedCueCard != null)
                                    {
                                        GFXManager.instance.selectedUnitDisplayAnchor.selectedCueCard.SelectCueCard();
                                    }
                                    player_selectedUnitAbility = null;
                                    targetingPackage.caster = player_SelectedPlayerUnit;
                                    targetingPackage.targetedEntity = null;
                                    GFXManager.instance.ClearSelectedUnitUI();
                                }
                                else
                                {
                                    targetingPackage.targetedEntity = hitUnit;
                                }


                            }

                            break;
                    }
                }
            }

            
            GFXManager.instance.UpdateSelectedUnitUI();

        } // <--- if (Input.GetMouseButtonDown(0))

        
    }

    public void TryCallAbility(Ability input)
    {
        StartCoroutine(ExecuteLongLogic(input));
    }

    public bool CheckParameters(Ability input)
    {
        if (input == null || targetingPackage.caster == null) {  return false; }

        bool result = false;

        foreach(string s in input.parameters)
        {
            bool singleTest = false;

            string[] ps = s.Split('/');

            foreach(string str in ps)
            {
                switch(str)
                {

                    case "gmmr":

                        // needs a pathable tile within the movement range
                        if (targetingPackage.caster != null && targetingPackage.targetedEntity != null)
                        {
                            if (targetingPackage.targetedEntity.flags.pathable)
                            {
                                PathObject path = MapBuilder.instance.BuildPath(targetingPackage.caster.tilemapPosition, targetingPackage.targetedEntity.tilemapPosition);
                                if (path.valid == true)
                                {
                                    singleTest = true;
                                }
                            }
                        }

                        break;
                    case "eu":
                        Debug.Log("1");
                        if (targetingPackage.targetedEntity != null)
                        {
                            UnitLogic enemyL = targetingPackage.targetedEntity.GetComponent<UnitLogic>();
                            if (enemyL != null)
                            {
                                if (enemyL.allianceInt != targetingPackage.caster.allianceInt)
                                {
                                    singleTest = true;
                                }
                            }
                        }

                        break;
                    case "des":

                        break;
                }

            }

            if (singleTest == false)
            {
                result = false;
                Debug.Log("param fail");
                return result;
            }
        }

        result = true;

        return result;
    }


    public IEnumerator ExecuteLongLogic(Ability input)
    {

        GameManager.Instance.controllsLocked = true;
        bool endConditions = false;

        PathObject path = new PathObject();
        Vector3 prevPos = Vector3.zero;
        float timeElapsed = 0;

        if (input.effects.Contains("mtl"))
        {
            path = MapBuilder.instance.BuildPath(targetingPackage.caster.tilemapPosition, targetingPackage.targetedEntity.tilemapPosition);
            prevPos = targetingPackage.caster.tilemapPosition;
        }

        while (endConditions == false)
        {
            foreach (string str in input.effects)
            {
                switch (str)
                {
                    case "mtl": // MOVE__________________________________________________

                        timeElapsed += Time.deltaTime;
                        selectedUnitMarker.transform.position = targetingPackage.caster.gameObject.transform.position;

                        if (path.positions.Count == 1 && path.positions[0] == targetingPackage.caster.tilemapPosition)
                        {
                            endConditions = true;
                            break;
                        }

                        if (Vector3.Distance(targetingPackage.caster.gameObject.transform.position, path.positions.First()) > 0.005f)
                        {
                            targetingPackage.caster.gameObject.transform.position = Vector3.Lerp(prevPos, (Vector3Int)path.positions.First(), timeElapsed / targetingPackage.caster.moveSpeed);
                        }
                        else
                        {
                            path.positions.Remove(path.positions.First());
                            if (path.positions.Count == 0)
                            {

                                targetingPackage.caster.transform.position = targetingPackage.targetedEntity.tilemapPosition;
                                targetingPackage.caster.tilemapPosition = targetingPackage.targetedEntity.tilemapPosition;


                                endConditions = true;
                            }
                            else
                            {
                                prevPos = targetingPackage.caster.transform.position;
                                timeElapsed = 0;

                                if (path.positions.Count() == 0)
                                {
                                    break;
                                }
                            }
                        }

                        break;

                    case "atk":

                        targetingPackage.targetedEntity.TakeDamage(1);
                        endConditions = true;

                        break;

                    default:
                        endConditions = true;
                        break;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        controllsLocked = false;
        player_selectedUnitAbility = null;
        GFXManager.instance.UpdateSelectedUnitUI();
        yield return new WaitForEndOfFrame();
    }



    private bool IsPointerOverUIElement()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

}
