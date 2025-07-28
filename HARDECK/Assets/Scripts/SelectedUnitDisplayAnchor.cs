using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class SelectedUnitDisplayAnchor : MonoBehaviour
{
    public GameObject[] abilityCueCards;

    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI unitTypeText;
    public TextMeshProUGUI unitRankText;

    public GameObject targetingDescriptorObj;
    public TextMeshProUGUI targetingDescriptorText;
    public TextMeshProUGUI targetingAbilityText;
    public Image targetingDescriptorIcon;
    public GameObject selectionIndicator;
    public AbilityCueCardHandler selecteedCueCard;
    public GameObject confirmActionButton;

    public void Start()
    {
        targetingDescriptorObj.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        targetingDescriptorObj.SetActive(false);
        selectionIndicator.SetActive(false);

        for (int i = 0; i < abilityCueCards.Length; i++)
        {
            abilityCueCards[i].GetComponent<AbilityCueCardHandler>().index = i;
        }
    }

    public void SetAbilityCueCards(UnitLogic input)
    {
        for (int i = 0; i < input.abilities.Count; i++)
        {
            abilityCueCards[i].SetActive(true);
            abilityCueCards[i].GetComponent<AbilityCueCardHandler>().SetToAbility(input.abilities[i]);
        }

        for (int i = input.abilities.Count; i < abilityCueCards.Length; i++)
        {
            abilityCueCards[i].SetActive(false);
        }
    }

    public void SetUnitInformationReadout(UnitLogic input = null)
    {
        if (input == null)
        {
            // Hides Cue Cards
            for (int i = 0; i < abilityCueCards.Length; i++)
            {
                abilityCueCards[i].SetActive(false);
            }

            unitNameText.text = "No Unit Selected";
            unitTypeText.text = "";
            unitRankText.text = "";

            targetingDescriptorObj.SetActive(false);

            return;
        }

        unitNameText.text = input.unitName;
        unitTypeText.text = input.unitType;
        unitRankText.text = input.unitRank;

        SetAbilityCueCards(input);
    }

    public void SetTargetingDescriptor()
    {
        Ability a = selecteedCueCard.ability;

        targetingAbilityText.text = a.description;

        bool paraaCheck = GameManager.Instance.CheckParameters(a);

        if (paraaCheck == true)
        {
            targetingDescriptorText.text = "CONFIRM ACTION";
            confirmActionButton.SetActive(true);

        }
        else
        {
            if (a.targetingTip != "zzz")
            {
                targetingDescriptorText.text = a.targetingTip;
                confirmActionButton.SetActive(false);

            }
            else
            {
                targetingDescriptorText.text = "CONFIRM ACTION";
                confirmActionButton.SetActive(true);

            }
        }
    }



    
}
