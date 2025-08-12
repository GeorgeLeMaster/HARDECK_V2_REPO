using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GFXManager : MonoBehaviour
{

    public static GFXManager instance;

    public LineRenderer LR_movement;
    public LineRenderer LR_Attack;

    public bool displayContext;
    public GameObject contextBox;
    public TextMeshProUGUI contextBoxText;

    [Header ("UI")]
    public SelectedUnitDisplayAnchor selectedUnitDisplayAnchor;

    private float uiAnimCooldown;
    public int uiAnimFramerate;

    private Material moveLineMat;
    private Texture[] moveLineSprites;
    private int moveLineAnimIndex = 0;

    private Material attackLineMat;
    private Texture[] attackLineSprites;
    private int attackLineAnimIndex = 0;

    [Range(0.25f, 1f)]
    public float uiScale;
    private float uiScaleBackend;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LR_movement.positionCount = 0;
        uiScaleBackend = uiScale;
        selectedUnitDisplayAnchor.transform.localScale = new Vector3(uiScale, uiScale, 1f);
        contextBox.GetComponent<RectTransform>().localScale = new Vector3(uiScale, uiScale, 1f);

        moveLineSprites = Resources.LoadAll<Texture>("UI/DashedLine");
        moveLineMat = Resources.Load<Material>("UI/DashedLine/MoveLineMat");

        attackLineSprites = Resources.LoadAll<Texture>("UI/BulletLine");
        attackLineMat = Resources.Load<Material>("UI/BulletLine/AttackLineMat");
    }

    // Update is called once per frame
    void Update()
    {
        if (displayContext)
        {
            contextBox.SetActive(true);
            contextBox.GetComponent<RectTransform>().position = Input.mousePosition;
        }
        else
        {
            contextBox.SetActive(false);
        }

        if (uiScale != uiScaleBackend)
        {
            uiScaleBackend = uiScale;
            selectedUnitDisplayAnchor.transform.localScale = new Vector3 (uiScale, uiScale, 1f);
            contextBox.GetComponent<RectTransform>().localScale = new Vector3(uiScale, uiScale, 1f);
        }

        if (uiAnimCooldown > 0)
        {
            uiAnimCooldown -= Time.deltaTime;   
            
        }
        else
        {
            TickUiAnims();
        }
    }

    public void UpdateSelectedUnitUI()
    {
        //Debug.Log("Updating Unit UI");

        if (GameManager.Instance.player_SelectedPlayerUnit != null)
        {
            selectedUnitDisplayAnchor.SetUnitInformationReadout(GameManager.Instance.player_SelectedPlayerUnit);
        }
        else
        {
            selectedUnitDisplayAnchor.SetUnitInformationReadout();
        }


        DisplayTargetingGFX(GameManager.Instance.player_selectedUnitAbility);

    }

    public void ClearSelectedUnitUI()
    {
        selectedUnitDisplayAnchor.SetUnitInformationReadout(null);
        DisplayTargetingGFX(null);
    }

    public void ShowContext(bool yn, string context = null)
    {
        displayContext = yn;
        contextBox.SetActive(displayContext);

        if (displayContext == true)
        {
            contextBoxText.text = context;
        }
    }

    public void DisplayTargetingGFX(Ability input)
    {

        TargetingPackage tp = GameManager.Instance.targetingPackage;
        LR_movement.positionCount = 0;
        LR_Attack.positionCount = 0;
        GameManager.Instance.target_pos_marker.transform.position = new Vector3Int(2, 0, -2);
        GameManager.Instance.target_pos_marker.SetActive(false);
        if (GameManager.Instance.CheckParameters(input) == false || tp.caster == null || input == null)
        {
            return;
        }
        // Debug.Log("Displaying");



        foreach (string str in input.effects)
        {
            switch (str)
            {
                case "mtl":

                    if (tp.targetedEntity != null)
                    {

                        Vector3Int destination = tp.targetedEntity.tilemapPosition;
                        Vector3 offset = new Vector3(0, 0.05f, 0);
                        PathObject path = MapBuilder.instance.BuildPath(tp.caster.tilemapPosition, destination);
                        // Debug.Log(path.positions.Count());
                        LR_movement.positionCount = path.positions.Count();
                        for (int i = 0; i < path.positions.Count; i++)
                        {
                            LR_movement.SetPosition(i, path.positions[i] + offset);
                        }

                        Vector3 pos1 = path.positions[0] + (new Vector3((path.positions[1] - path.positions[0]).x, (path.positions[1] - path.positions[0]).y, (path.positions[1] - path.positions[0]).z) * 0.5f);
                        LR_movement.SetPosition(0, pos1 + offset);

                        Vector3 pos2 = path.positions[path.positions.Count - 1] - (new Vector3((path.positions[path.positions.Count - 1] - path.positions[path.positions.Count - 2]).x, (path.positions[path.positions.Count - 1] - path.positions[path.positions.Count - 2]).y, (path.positions[path.positions.Count - 1] - path.positions[path.positions.Count - 2]).z) * 0.5f);
                        LR_movement.SetPosition(path.positions.Count - 1, pos2 + offset);
                        LR_movement.startColor = Color.blue;
                        LR_movement.endColor = Color.blue + new Color(0.5f,0.5f,1f);

                        GameManager.Instance.target_pos_marker.transform.position = destination;
                        GameManager.Instance.target_pos_marker.SetActive(true);

                    }
                    else
                    {
                        LR_movement.positionCount = 0;
                    }
                    break;


                case "atk":

                    if (tp.targetedEntity != null)
                    {
                        if (tp.targetedEntity.GetComponent<UnitLogic>() != null)
                        {
                            LR_Attack.positionCount = 2;
                            LR_Attack.SetPosition(0, tp.caster.transform.position + new Vector3(0, 0.5f, 0));
                            LR_Attack.SetPosition(1, tp.targetedEntity.transform.position + new Vector3(0, 0.5f, 0));
                            LR_Attack.startColor = Color.red;
                            LR_Attack.endColor = Color.red;

                        }
                        else
                        {
                            LR_Attack.positionCount = 0;
                        }
                    }
                    break;
            }
        }
    }

    public void TickUiAnims()
    {
        moveLineAnimIndex++;
        if (moveLineAnimIndex > (moveLineSprites.Count()-1))
        {
            moveLineAnimIndex = 0;
        }

        moveLineMat.mainTexture = moveLineSprites[moveLineAnimIndex];


        attackLineAnimIndex++;
        if (attackLineAnimIndex > (attackLineSprites.Count() - 1))
        {
            attackLineAnimIndex = 0;
        }

        attackLineMat.mainTexture = attackLineSprites[attackLineAnimIndex];

        uiAnimCooldown = 0.016f;
    }
}
