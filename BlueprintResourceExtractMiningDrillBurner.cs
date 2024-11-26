using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    /*
    * Recepi:
    * 2 seconds +
    * 3x iron gear wheel +
    * 3x iron plate +
    * 1x stone furnace
    */
    public class BlueprintResourceExtractMiningDrillBurner : Blueprint
    {
        public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
        {
            base.BlueprintComplete(rootCard, involvedCards, print);

            List<string> ingredientIds = new List<string>
            {   "card_stacktorio_component_gear_wheel_iron", 
                "card_stacktorio_material_plate_iron", 
                "card_stacktorio_furnace_stone"
            };

            List<int> ingredientQuantities = new List<int>
            {   3, 
                3, 
                1
            };

            for (int i = 0; i < involvedCards.Count; i++)
            {
                int index = ingredientIds.IndexOf(involvedCards[i].CardData.Id);
                
                if (index >= 0 && ingredientQuantities[index] > 0)
                {
                    // Exception required to destroy stone furnace. It is a structure, will not be auto-destroyed if value = 0.
                    if (involvedCards[i].CardData.Id == "card_stacktorio_furnace_stone")
                    {
                        ingredientQuantities[index] -= 1;
                        involvedCards[i].DestroyCard(false, false);
                    }
                    else
                    {
                        int reduction = Mathf.Min(ingredientQuantities[index], involvedCards[i].CardData.Value);
                        ingredientQuantities[index] -= reduction;
                        involvedCards[i].CardData.Value -= reduction;
                    }
                }                
            }
        }
    }
}