using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCueCardHandler : MonoBehaviour
{

    public Image icon;

    public TextMeshProUGUI abilityName;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI usesText;

    private string abilityDescription;

    public int index;
    public Ability ability;

    public bool selected = false;

    void Update()
    {
        if (selected && Input.GetMouseButtonDown(0))
        {
            GFXManager.instance.SelectedUnitDisplayAnchor.SetTargetingDescriptor();
        }
    }

    public void SetToAbility(Ability input)
    {
        ability = input;

        abilityName.text = input.name;
        costText.text = "x" + input.timeCost.ToString();
        
        if (input.uses != 999)
        {
            usesText.text =  "Uses: " + input.uses.ToString();
        }
        else
        {
            usesText.text = "Uses: \u221E";
        }

        icon.sprite = input.icon;

        abilityDescription = input.description;
    }

    public void DisplayContext(bool input)
    {
        if (!selected)
        {
            GFXManager.instance.ShowContext(input, abilityDescription);
        }
    }


    public void SelectCueCard()
    {
        AbilityCueCardHandler sCueCard = GFXManager.instance.SelectedUnitDisplayAnchor.selecteedCueCard;
        if (sCueCard == null)
        {
            GFXManager.instance.SelectedUnitDisplayAnchor.selecteedCueCard = this;
        }
        else if (sCueCard != this)
        {
            GFXManager.instance.SelectedUnitDisplayAnchor.selecteedCueCard.SelectCueCard();
            GFXManager.instance.SelectedUnitDisplayAnchor.selecteedCueCard = this;
        }
        else if (sCueCard == this)
        {
            GFXManager.instance.SelectedUnitDisplayAnchor.selecteedCueCard = null;
        }

        selected = !selected;
        GFXManager.instance.ShowContext(!selected, abilityDescription);

        GameObject tDesc = GFXManager.instance.SelectedUnitDisplayAnchor.targetingDescriptorObj;
        GameObject indicator = GFXManager.instance.SelectedUnitDisplayAnchor.selectionIndicator;
        tDesc.SetActive(selected);
        indicator.SetActive(selected);

        if (selected)
        {
            GameManager.Instance.player_selectedUnitAbility = ability;
            tDesc.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position + new Vector3(395, 0, 0);
            indicator.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position + new Vector3(145, 0, 0);
            GFXManager.instance.SelectedUnitDisplayAnchor.SetTargetingDescriptor();
        }


    }
}
