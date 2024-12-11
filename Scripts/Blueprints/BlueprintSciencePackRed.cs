using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    /*
    * Recepi:
    * 5 seconds +
    * 1x copper plate +
    * 1x geer wheel iron
    */
    public class BlueprintSciencePackRed : Blueprint
    {
        public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
        {
            base.BlueprintComplete(rootCard, involvedCards, print);

            List<string> ingredientIds = new List<string>
            {   "card_stacktorio_material_plate_copper",
                "card_stacktorio_component_gear_wheel_iron",
            };

            List<int> ingredientQuantities = new List<int>
            {   1,
                1
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