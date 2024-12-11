using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    /*
    * Recepi:
    * 2 seconds +
    * 10x green circuit
    * 10x iron gear wheel
    * 4x transport belt
    */
    public class BlueprintLab : Blueprint
    {
        public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
        {
            base.BlueprintComplete(rootCard, involvedCards, print);

            List<string> ingredientIds = new List<string>
            {   "card_stacktorio_crafting_component_circuit_green",
                "card_stacktorio_component_gear_wheel_iron",
                "card_stacktorio_belt_transport_belt_transport"
            };

            List<int> ingredientQuantities = new List<int>
            {   10,
                10,
                4
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