using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    /*
    * Recepi:
    * 0.5 seconds +
    * 2x iron plate
    */
    public class BlueprintComponentGearWheelIron : Blueprint
    {
        public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
        {
            base.BlueprintComplete(rootCard, involvedCards, print);

            List<string> ingredientIds = new List<string>
            {   "card_stacktorio_material_plate_iron"
            };

            List<int> ingredientQuantities = new List<int>
            {   2
            };

            for (int i = 0; i < involvedCards.Count; i++)
            {
                int index = ingredientIds.IndexOf(involvedCards[i].CardData.Id);
                
                if (index >= 0 && ingredientQuantities[index] > 0)
                {    
                    int reduction = Mathf.Min(ingredientQuantities[index], involvedCards[i].CardData.Value);
                    ingredientQuantities[index] -= reduction;
                    involvedCards[i].CardData.Value -= reduction;
                }                
            }
        }
    }
}