using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
            GFXManager.instance.selectedUnitDisplayAnchor.SetTargetingDescriptor();
        }
    }

    public void SetToAbility(Ability input)
    {
        if (GameManager.Instance.controllsLocked == true) { return;  }

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
        AbilityCueCardHandler sCueCard = GFXManager.instance.selectedUnitDisplayAnchor.selectedCueCard;
        if (sCueCard == null)
        {
            GFXManager.instance.selectedUnitDisplayAnchor.selectedCueCard = this;
        }
        else if (sCueCard != this)
        {
            GFXManager.instance.selectedUnitDisplayAnchor.selectedCueCard.SelectCueCard();
            GFXManager.instance.selectedUnitDisplayAnchor.selectedCueCard = this;
        }
        else if (sCueCard == this)
        {
            GFXManager.instance.selectedUnitDisplayAnchor.selectedCueCard = null;
        }

        selected = !selected;
        GFXManager.instance.ShowContext(!selected, abilityDescription);
        GameObject tDesc = GFXManager.instance.selectedUnitDisplayAnchor.targetingDescriptorObj;
        GameObject indicator = GFXManager.instance.selectedUnitDisplayAnchor.selectionIndicator;
        tDesc.SetActive(selected);
        indicator.SetActive(selected);

        if (selected)
        {
            GameManager.Instance.player_selectedUnitAbility = ability;
            tDesc.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position + new Vector3(395 * GFXManager.instance.uiScale, 0, 0);
            indicator.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position + new Vector3(145 * GFXManager.instance.uiScale, 0, 0);
            GFXManager.instance.selectedUnitDisplayAnchor.SetTargetingDescriptor();
            GFXManager.instance.DisplayTargetingGFX(ability);

        }


    }
}
