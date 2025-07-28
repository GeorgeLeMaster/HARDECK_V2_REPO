using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GFXManager : MonoBehaviour
{

    public static GFXManager instance;

    public LineRenderer LR_movement;

    public bool displayContext;
    public GameObject contextBox;
    public TextMeshProUGUI contextBoxText;

    [Header ("UI")]
    public SelectedUnitDisplayAnchor SelectedUnitDisplayAnchor;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LR_movement.positionCount = 0;
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
    }

    public void UpdateSelectedUnitUI()
    {
        Debug.Log("Updating Unit UI");

        if (GameManager.Instance.player_SelectedPlayerUnit != null)
        {
            SelectedUnitDisplayAnchor.SetUnitInformationReadout(GameManager.Instance.player_SelectedPlayerUnit);
        }
        else
        {
            SelectedUnitDisplayAnchor.SetUnitInformationReadout();
        }


        DisplayTargetingGFX(GameManager.Instance.player_selectedUnitAbility);

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
        //Debug.Log("Trying to display");
        if (GameManager.Instance.CheckParameters(input) == false)
        {
            LR_movement.positionCount = 0;
            return;
        }
       // Debug.Log("Displaying");


        TargetingPackage tp = GameManager.Instance.targetingPackage;

        foreach (string str in input.effects)
        {
            switch (str)
            {
                case "mtl":

                    if (tp.caster != null && tp.targetedEntity != null)
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

                        //Vector3 pos1 = path.positions[0] + (new Vector3((path.positions[1] - path.positions[0]).x, (path.positions[1] - path.positions[0]).y, (path.positions[1] - path.positions[0]).z) * 0.5f);
                        //LR_movement.SetPosition(0, pos1 + offset);

                        //Vector3 pos2 = path.positions[path.positions.Count - 1] - (new Vector3((path.positions[path.positions.Count - 1] - path.positions[path.positions.Count - 2]).x, (path.positions[path.positions.Count - 1] - path.positions[path.positions.Count - 2]).y, (path.positions[path.positions.Count - 1] - path.positions[path.positions.Count - 2]).z) * 0.5f);
                        //LR_movement.SetPosition(path.positions.Count - 1, pos2 + offset);
                    }
                    else
                    {
                        LR_movement.positionCount = 0;
                    }




                    break;
            }
        }
    }

}
